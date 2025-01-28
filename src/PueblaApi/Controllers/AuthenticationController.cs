using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.RegularExpressions;
using PueblaApi.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using PueblaApi.Database;
using PueblaApi.Entities;
using PueblaApi.Helpers;
using PueblaApi.Services.Interfaces;
using SodaAPI.RequestHelpers;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PueblaApi.DTOS.Auth;
using IPacientesApi.Dtos.User;
using PueblaApi.DTOS.Base;
using PasswordGenerator;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using PueblaApi.Repositories.Interfaces;


namespace PueblaApi.Controllers;

[Route(ApiControllerRoutes.Authentication)]
[ApiController]
public class AuthenticationController : ControllerBase
{
    // 1. Indicate dependency injection container to Inject the UserManager.
    private readonly UserManager<ApplicationUser> _userManager;
    // 2. Inject the JWTConfig to make sure the key is encrypted in the requests.
    private readonly IConfiguration _configuration;
    // 9. Inject ApplicationDatabaseContext.
    private readonly ApplicationDbContext _context;
    // 10. Inject TokenValidationParameters.
    private readonly TokenValidationParameters _tokenValidationParameters;
    // 11. Inject RoleManager.
    private readonly RoleManager<IdentityRole> _roleManager;
    // 12. Inject the logger.
    private readonly ILogger<AuthenticationController> _logger;
    // 14. Inject JWT configuration.
    private readonly JwtConfiguration _jwtConfiguration;
    // 15. Inject email service.
    private readonly IEmailService _emailService;
    // 16. Inject email activation code repository.
    private readonly IEmailConfirmationCodeRepository _emailConfirmationCodeRepository;
    // 17. Inject web client settings.
    private readonly WebClientSettings _webClientSettings;

    public AuthenticationController(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        ApplicationDbContext context,
        TokenValidationParameters tokenValidationParameters,
        RoleManager<IdentityRole> roleManager,
        ILogger<AuthenticationController> logger,
        JwtConfiguration jwtConfiguration,
        IEmailService emailService,
        IEmailConfirmationCodeRepository emailActivationCodeRepository,
        WebClientSettings webClientSettings)
    {
        // 1. Indicate dependency injection container to Inject the UserManager.
        this._userManager = userManager;
        // 2. Inject the JWTConfig to make sure the key is encrypted in the requests.
        this._configuration = configuration;
        // 9. Inject ApplicationDatabaseContext.
        this._context = context;
        // 10. Inject TokenValidationParameters.
        this._tokenValidationParameters = tokenValidationParameters;
        // 11. Inject RoleManager.
        this._roleManager = roleManager;
        // 12. Inject the logger.
        this._logger = logger;
        // 14. Inject JWT configuration.
        this._jwtConfiguration = jwtConfiguration;
        // 15. Inject email service.
        this._emailService = emailService;
        // 16. Inject email activation code repository.
        this._emailConfirmationCodeRepository = emailActivationCodeRepository;
        // 17. Inject web client settings.
        this._webClientSettings = webClientSettings;
    }
    #region Endpoints

    /// <summary>
    /// Sign up a new non-admin user.
    /// </summary>
    /// <param name="dto">JSON that includes all information necessary to create an account</param>
    /// <returns></returns>
    // [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = $"{ApiRoles.Admin},{ApiRoles.Manager}")] // Requires a JWT that has the role Admin or Manager.
    [HttpPost("signup")]
    public async Task<ActionResult> Signup(SignupRequest dto)
    {
        // Regex expression used to remove anything that is not a letter or number.
        try
        {
            // 1. Check if the username or national id already exists.
            var userExists = await this._userManager.FindByNameAsync(dto.Username) ?? await this._userManager.FindByEmailAsync(dto.Email);
            if (userExists != null) // Assigns 'null' if the user doesn't exist.
                return BadRequest(this.GenerateUnsuccessfulAuthenticationResponse("Nombre de usuario o correo electrónico no están disponibles."));

            // 2. Create new user. 
            ApplicationUser newUser = new ApplicationUser()
            {
                UserName = dto.Username.ToLower(),
                Email = dto.Email.ToLower(),
                // Remove anything that is not a letter or number.
                //NationalId = Regex.Replace(dto.NationalId, @"[^A-Za-z0-9]", "", RegexOptions.IgnoreCase),
                FirstName = dto.FirstName.ToUpper(),
                LastName = dto.LastName.ToUpper(),
                IsEnabled = true,
                EmailConfirmed = false

            };
            var userWasCreated = await this._userManager.CreateAsync(newUser, dto.Password); // Creates user (password is encrypted in the function)
            if (!userWasCreated.Succeeded)
            {
                return BadRequest(this.GenerateUnsuccessfulAuthenticationResponse(userWasCreated.Errors.Select(e => e.Description).ToList()));
            }
            // 3. Add ROLES to new user.
            // 3.1. Verify the role exists
            IdentityRole roleToBeAdded = await this._roleManager.FindByNameAsync(dto.Role);
            if (roleToBeAdded == null)
            {
                return BadRequest(this.GenerateUnsuccessfulAuthenticationResponse("Rol no existe."));
            }
            // 3.2. Add role.
            IdentityResult roleWasAdded = await this._userManager.AddToRoleAsync(newUser, dto.Role);
            if (!roleWasAdded.Succeeded)
            {
                return BadRequest(
                    new List<string>() { "No se pudo crear usuario." }
                    .Concat(roleWasAdded.Errors.Select(error => error.Description).ToList())
                );
            }
            // 4. Save changes related to user.
            await this._context.SaveChangesAsync();
            // 5. Generate email activation code.
            await this.GenerateEmailConfirmationCode(newUser);
            // 5. Return URL where we can retrieve user's information and the user information.
            return CreatedAtAction(nameof(GetUser), new { id = newUser.Id }, ResponseHelper.SuccessfulResponse<UserResponse>(
                new UserResponse()
                {
                    Id = newUser.Id
                })
            );
        }
        catch (HttpRequestException e)
        {
            return ErrorHelper.Internal(this._logger, e.StackTrace, "No hay conexión con el servidor de correo.");
        }
        catch (Exception e)
        {
            return ErrorHelper.Internal(this._logger, e.StackTrace);
        }
    }

    /**
        Authenticate user and issue a JWT and a refresh token.
    **/
    [HttpPost("login")]
    public async Task<ActionResult> Login([FromBody] LoginRequest dto)
    {
        try
        {
            // 1. Search user by username or email.
            ApplicationUser user = await this._userManager.FindByNameAsync(dto.Name) ?? await this._userManager.FindByEmailAsync(dto.Name);
            if (user == null)
                return BadRequest(this.GenerateUnsuccessfulAuthenticationResponse("Usuario no existe"));

            // 2. If the user exists, check the password.
            bool passwordIsCorrect = await this._userManager.CheckPasswordAsync(user, dto.Password);
            if (!passwordIsCorrect)
            {
                return BadRequest(this.GenerateUnsuccessfulAuthenticationResponse("Contraseña o usuario incorrecto."));
            }
            // 3. If the user is not enabled, we don't allow it to login.
            if (!user.IsEnabled)
            {
                return BadRequest(this.GenerateUnsuccessfulAuthenticationResponse($"Usuario {user.UserName} fue desactivado."));
            }
            // 4. Check email is enabled
            if (!(await this._userManager.IsEmailConfirmedAsync(user)))
            {
                return BadRequest(this.GenerateUnsuccessfulAuthenticationResponse($"Usuario {user.UserName} debe activar el correo electrónico."));
            }
            // 4. If the user exists, we generate the token and return it.
            return Ok(await this.GenerateSuccessfulAuthenticationResponse(user));
        }
        catch (Exception e)
        {
            return ErrorHelper.Internal(this._logger, e.StackTrace);
        }
    }

    /// <summary>
    /// Endpoint that changes the password of an account and sends it to its email.
    /// </summary>
    /// <param name="dto">JSON that contains username and email used to create the account</param>
    /// <returns></returns>
    [HttpPut("recover-password")]
    public async Task<ActionResult> RecoverPassword([FromBody] RecoverPasswordRequest dto)
    {
        try
        {
            int passwordLength = 10;

            // 1. Search the username and email match.
            ApplicationUser user = await this._userManager.FindByNameAsync(dto.Username);
            if (user == null)
                return BadRequest(this.GenerateUnsuccessfulAuthenticationResponse($"Usuario {dto.Username} no existe."));
            if (!user.IsEnabled)
                return BadRequest(this.GenerateUnsuccessfulAuthenticationResponse($"Usuario {dto.Username} fue desactivado."));
            // 2. Check email provided matches email linked to account.
            if (user.Email != dto.Email)
                return BadRequest(this.GenerateUnsuccessfulAuthenticationResponse($"Correo no corresponde a usuario."));

            // 3. If the email hasn't been activated/confirmed, send a new confirmation code.
            // Otherwise, send a new password
            if (!(await this._userManager.IsEmailConfirmedAsync(user)))
            {
                this._logger.LogInformation("Attempting to send a new email confirmation code to the user.");
                await this.GenerateEmailConfirmationCode(user);
                return Ok(ResponseHelper.SuccessfulResponse($"Correo de confirmación enviado a {user.Email}"));
            }
            else
            {
                // 4. Create a new password.
                this._logger.LogInformation("Creating new password...");
                string newPassword = new Password(passwordLength).IncludeNumeric().IncludeLowercase().IncludeUppercase().IncludeSpecial("#_!=").Next().ToString();
                this._logger.LogInformation("New password created.");

                // 5. Send the new password on an email.
                this._logger.LogInformation("Attempting to send email to user with new password.");
                string emailSubject = $"IPACIENTES - Se ha reestablecido la contraseña";
                string emailHtmlContent = $"IPACIENTES - RESTABLACIMIENTO DE CONTRASEÑA <br><br>La nueva contraseña para el usuario <i>{user.UserName}</i> es:<br><br><b>{newPassword}</b><br><br>Este es un mensaje generado automáticamente. Por favor, no respondas a este correo electrónico.";
                await this._emailService.SendEmail(user.Email, emailSubject, emailHtmlContent);

                // 6. Change the user's password.
                this._logger.LogInformation("Removing old password.");
                await this._userManager.RemovePasswordAsync(user);
                this._logger.LogInformation("Setting new password.");
                await this._userManager.AddPasswordAsync(user, newPassword);

                // 7. Save changes.
                await this._userManager.UpdateAsync(user);
                // 8. Return response.
                return Ok(ResponseHelper.SuccessfulResponse("Contraseña restablecida."));
            }
        }
        catch (HttpRequestException e)
        {
            return ErrorHelper.Internal(this._logger, e.StackTrace, "No hay conexión con el servidor de correo.");
        }
        catch (Exception e)
        {
            return ErrorHelper.Internal(this._logger, e.StackTrace);
        }
    }

    /// <summary>
    /// Allows the user to modify its information.
    /// </summary>
    /// <param name="dto">Contains the information to change and the current password.</param>
    /// <returns></returns>
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] // Requires a JWT.
    [HttpPut("users/me")]
    public async Task<ActionResult> Update([FromBody] UpdateUserRequest dto)
    {
        try
        {
            // 1. Get user using id obtained from token
            ApplicationUser user = await TokenHelper.GetUserFromJWTClaim(HttpContext.User.Identity as ClaimsIdentity, _userManager);
            if (user == null)
                return BadRequest(this.GenerateUnsuccessfulAuthenticationResponse("Token inválido. Vuelva a iniciar sesión."));

            // 2. If the user exists, check the current password.
            bool passwordIsCorrect = await this._userManager.CheckPasswordAsync(user, dto.CurrentPassword);
            if (!passwordIsCorrect)
                return BadRequest(this.GenerateUnsuccessfulAuthenticationResponse("Contraseña actual es incorrecta."));

            // 3. Update user's information.
            // If the new information matches the old one, don't change it.
            if (!dto.FirstName.IsNullOrEmpty() && user.FirstName != dto.FirstName)
                user.FirstName = dto.FirstName.ToUpper();
            if (!dto.LastName.IsNullOrEmpty() && user.LastName != dto.LastName)
                user.LastName = dto.LastName.ToUpper();
            // if (!dto.NationalId.IsNullOrEmpty() && user.NationalId != dto.NationalId)
            //    user.NationalId = Regex.Replace(dto.NationalId, @"[^A-Za-z0-9]", "", RegexOptions.IgnoreCase);
            if (!dto.Email.IsNullOrEmpty() && user.Email != dto.Email)
            {
                // 3.4. Check email availability.
                if (await this._userManager.FindByEmailAsync(dto.Email) != null)
                    return BadRequest(this.GenerateUnsuccessfulAuthenticationResponse($"Correo {dto.Email} ya está en uso."));
                else
                {
                    user.Email = dto.Email;
                    // Force user to confirm its new email.
                    user.EmailConfirmed = false;
                }
            }
            // Forbidden to change the 'admin' username.
            if (!dto.Username.IsNullOrEmpty() && user.UserName != dto.Username && user.UserName != "admin")
            {
                // 3.5. Check username availability.
                if (await this._userManager.FindByNameAsync(dto.Username) != null)
                    return BadRequest(this.GenerateUnsuccessfulAuthenticationResponse($"Nombre de usuario {dto.Username} ya está en uso."));
                else
                {
                    user.UserName = dto.Username;
                }
            }
            // 5. Change password
            if (!dto.Password.IsNullOrEmpty())
            {
                var passwordChangeResult = await this._userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.Password);
                if (!passwordChangeResult.Succeeded)
                    // Fails the process and it doesn't let it continue.
                    return BadRequest(this.GenerateUnsuccessfulAuthenticationResponse(
                        passwordChangeResult.Errors.Select(e => e.Description).ToList()
                ));
            }
            // 6. Write date the user was updated and save changes.
            await this._userManager.UpdateAsync(user);
            // 7. Send email confirmation after the changes have been saved.
            if (!user.EmailConfirmed)
                await this.GenerateEmailConfirmationCode(user);
            // 8. Issue new JWT and refresh token.
            return Ok(await this.GenerateSuccessfulAuthenticationResponse(user));
        }
        catch (Exception e)
        {
            return ErrorHelper.Internal(this._logger, e.StackTrace);
        }
    }

    /// <summary>
    /// Allows to update any user without the need of using the current password. 
    /// Only the administrator user is allowed to use this endpoint.
    /// </summary>
    /// <param name="dto">JSON with data to be changed</param>
    /// <returns></returns>
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = $"{ApiRoles.Admin}")] // Requires a JWT.
    [HttpPut("users/{userId}")]
    public async Task<ActionResult> UpdateAny(string userId, [FromBody] UpdateAnyUserRequest dto)
    {
        // IMPORTANT: Using this endpoint bypasses email confirmation. This is by design.
        try
        {
            // 1. Get user using id obtained from path parameter
            ApplicationUser user = await this._userManager.FindByIdAsync(userId);
            if (user == null)
                return BadRequest(this.GenerateUnsuccessfulAuthenticationResponse("No existe un usuario con este ID."));
            // 2. Don't allow admin user to be updated through this endpoint.
            if (user.UserName == "admin")
                return BadRequest(this.GenerateUnsuccessfulAuthenticationResponse("No se puede modificar el usuario administrador de esta manera."));

            // 3. Update user's information.
            // If the new information matches the old one, don't change it.
            if (!dto.FirstName.IsNullOrEmpty() && user.FirstName != dto.FirstName)
                user.FirstName = dto.FirstName;
            if (!dto.LastName.IsNullOrEmpty() && user.LastName != dto.LastName)
                user.LastName = dto.LastName;
            //if (!dto.NationalId.IsNullOrEmpty() && user.NationalId != dto.NationalId)
            //    user.NationalId = dto.NationalId;
            if (!dto.Email.IsNullOrEmpty() && user.Email != dto.Email)
            {
                // 3.4. Check email availability.
                if (await this._userManager.FindByEmailAsync(dto.Email) != null)
                    return BadRequest(this.GenerateUnsuccessfulAuthenticationResponse($"Correo {dto.Email} ya está en uso."));
                else
                {
                    user.Email = dto.Email;
                    // user.EmailConfirmed = false;
                }
            }
            // Forbidden to change the 'admin' username.
            if (!dto.Username.IsNullOrEmpty() && user.UserName != dto.Username)
            {
                // 3.5. Check username availability.
                if (await this._userManager.FindByNameAsync(dto.Username) != null)
                    return BadRequest(this.GenerateUnsuccessfulAuthenticationResponse($"Nombre de usuario {dto.Username} ya está en uso."));
                else
                    user.UserName = dto.Username;
            }
            // 5. Change password
            if (!dto.Password.IsNullOrEmpty())
            {

                IdentityResult passwordChangeResult = await this._userManager.RemovePasswordAsync(user);
                if (passwordChangeResult.Succeeded)
                    passwordChangeResult = await this._userManager.AddPasswordAsync(user, dto.Password);
                if (!passwordChangeResult.Succeeded)
                    // Fails the process and it doesn't let it continue.
                    return BadRequest(this.GenerateUnsuccessfulAuthenticationResponse(
                        passwordChangeResult.Errors.Select(e => e.Description).ToList()
                ));
            }
            // 6. Save changes.
            await this._userManager.UpdateAsync(user);
            // 7. Change the user's roles.
            if (!dto.Role.IsNullOrEmpty())
            {
                dto.Role = dto.Role.ToLower();
                // User can only have one role at the time, therefore, we have to remove any other role it has.
                // 7.1. Check if new role exists.
                bool roleExists = await this._roleManager.RoleExistsAsync(dto.Role);
                if (roleExists)
                {
                    var roles = await this._userManager.GetRolesAsync(user);
                    // 7.2. Remove existent roles.
                    await this._userManager.RemoveFromRolesAsync(user, roles);
                    // 7.3. Add new roles.
                    await this._userManager.AddToRoleAsync(user, dto.Role);
                }
                else
                {
                    return BadRequest(this.GenerateUnsuccessfulAuthenticationResponse("Rol no existe."));
                }
            }

            // 7. Return response.
            return Ok(ResponseHelper.SuccessfulResponse("Información de usuario actualizada."));
        }
        catch (Exception e)
        {
            return ErrorHelper.Internal(this._logger, e.StackTrace);
        }
    }


    /// <summary>
    /// Retrieves user information.
    /// </summary>
    /// <param name="userId">ID of the user whose information we are going to retrieve.</param>
    /// <returns>JSON with the information of the user</returns>
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpGet("users/{userId}")]
    public async Task<ActionResult> GetUser(string userId)
    {
        try
        {
            // 1. Search user.
            ApplicationUser user = await this._userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();

            // 2. Get roles and return information.
            return Ok(ResponseHelper.SuccessfulResponse<UserResponse>(new()
            {
                Id = user.Id,
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                //NationalId = user.NationalId,
                Email = user.Email,
                Roles = (await this._userManager.GetRolesAsync(user)).ToList(),
                IsEnabled = user.IsEnabled
            }));
        }
        catch (Exception e)
        {
            return ErrorHelper.Internal(this._logger, e.StackTrace);
        }
    }

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = $"{ApiRoles.Admin}")]
    [HttpGet("users")]
    public async Task<ActionResult> GetUsers([FromQuery] SearchUserRequest request)
    {
        try
        {
            IQueryable<ApplicationUser> query = this._context.Users;
            // 1. Define query by username, first name, last name, national ID (ignoring cases) and enabled/disabled.
            if (!request.Q.IsNullOrEmpty())
                query = query.Where(u => (EF.Functions.ILike(u.UserName, $"%{request.Q}%") ||
                                        EF.Functions.ILike(u.FirstName, $"%{request.Q}%") ||
                                        EF.Functions.ILike(u.LastName, $"%{request.Q}%")) &&
                                        // || EF.Functions.ILike(u.NationalId, $"%{request.Q}%")) &&
                                        u.IsEnabled == request.IsEnabled
                );
            else
            {
                query = query.Where(u => u.IsEnabled == request.IsEnabled);
            }
            // 2. Apply sorting column.
            Expression<Func<ApplicationUser, object>> keySelector = request.SortColumn?.ToLower() switch
            {
                "username" => x => x.UserName,
                "firstname" => x => x.FirstName,
                "lastname" => x => x.LastName,
                //"nationalid" => x => x.NationalId,
                _ => x => x.Id // Default.
            };
            // 3. Apply sorting order (descending or ascending)
            if (request.SortOrder?.ToLower() == "desc")
                query = query.OrderByDescending(keySelector);
            else
                query = query.OrderBy(keySelector);
            // 4. Build search response object that will execute the query and return additional information.
            var queryResult = await SearchResponse<ApplicationUser>.CreateAsync(query, request.Page, request.PageSize);
            // 5. Map ApplicationUser to UserResponse and return the result.
            List<UserResponse> items = [];
            foreach (ApplicationUser u in queryResult.Items)
            {
                items.Add(new UserResponse()
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    Email = u.Email,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    //NationalId = u.NationalId,
                    Roles = (await this._userManager.GetRolesAsync(u)).ToList(),
                    IsEnabled = u.IsEnabled
                });
            }
            return Ok(ResponseHelper.SuccessfulResponse(
                new
                {
                    items,
                    queryResult.Page,
                    queryResult.PageSize,
                    queryResult.TotalCount,
                    queryResult.HasNextPage,
                    queryResult.HasPreviousPage
                }
            ));
        }
        catch (Exception e)
        {
            return ErrorHelper.Internal(this._logger, e.StackTrace);
        }

    }

    /// <summary>
    /// Get user based on JWT.
    /// </summary>
    /// <returns></returns>
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpGet("users/me")]
    public async Task<ActionResult> GetCurrentUser()
    {
        try
        {
            // 1. Get user using id obtained from token
            ApplicationUser user = await TokenHelper.GetUserFromJWTClaim(HttpContext.User.Identity as ClaimsIdentity, _userManager);
            if (user == null)
                return BadRequest(this.GenerateUnsuccessfulAuthenticationResponse("Token inválido. Vuelva a iniciar sesión."));

            // 2. Get roles and return information.
            return Ok(ResponseHelper.SuccessfulResponse<UserResponse>(new()
            {
                Id = user.Id,
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                //NationalId = user.NationalId,
                Email = user.Email,
                Roles = (await this._userManager.GetRolesAsync(user)).ToList(),
            }));
        }
        catch (Exception e)
        {
            return ErrorHelper.Internal(this._logger, e.StackTrace);
        }
    }

    /// <summary>
    /// Deactivates/reactivates a user.
    /// </summary>
    /// <returns>JSON with result.</returns>
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = $"{ApiRoles.Admin}")]
    [HttpPut("users/{userId}/toggle")]
    public async Task<ActionResult> Toggle(string userId)
    {
        try
        {
            // 1. Get user using id obtained from path parameter
            ApplicationUser user = await this._userManager.FindByIdAsync(userId);
            if (user == null)
                return BadRequest(this.GenerateUnsuccessfulAuthenticationResponse("No existe un usuario con este ID."));

            // 2. Don't allow admin user to be deactivated.
            if (user.UserName == "admin")
                return BadRequest(this.GenerateUnsuccessfulAuthenticationResponse("No se puede desactivar el usuario administrador."));

            // 3. Deactivate/reactivate account and save changes.
            user.IsEnabled = !user.IsEnabled;
            await this._userManager.UpdateAsync(user);

            // 5. Return response.
            return Ok(ResponseHelper.SuccessfulResponse($"Cuenta {user.UserName} fue {(user.IsEnabled ? "activada" : "desactivada")} ."));
        }
        catch (Exception e)
        {
            return ErrorHelper.Internal(this._logger, e.StackTrace);
        }
    }

    [HttpGet("confirm/{code}")]
    public async Task<ActionResult> ConfirmEmail(Guid code)
    {
        try
        {
            // 1. Search token.
            EmailConfirmationCode activationCode = await this._emailConfirmationCodeRepository.GetByCode(code);
            if (activationCode == null)
                return NotFound();

            ApplicationUser user = activationCode.User;

            if (DateTimeOffset.UtcNow.CompareTo(activationCode.ExpirationDate) > 0)
            {
                return Unauthorized(this.GenerateUnsuccessfulAuthenticationResponse("Código ha expirado, recupera la cuenta para obtener uno nuevo"));
            }

            // 2. Change state of the account / activate email.
            user.EmailConfirmed = true;
            // 3. Delete token.
            await this._emailConfirmationCodeRepository.Delete(activationCode);
            // 4. Save changes and return response.
            return Ok(ResponseHelper.SuccessfulResponse($"Correo para cuenta {user.UserName} ha sido confirmado."));
        }
        catch (Exception e)
        {
            return ErrorHelper.Internal(this._logger, e.StackTrace);
        }
    }

    private async Task GenerateEmailConfirmationCode(ApplicationUser user)
    {
        // 1. Verify there are no codes for this user.
        // If there is other code, delete it.
        EmailConfirmationCode existentCode = await this._emailConfirmationCodeRepository.GetByUser(user);
        if (existentCode != null)
        {
            await this._emailConfirmationCodeRepository.Delete(existentCode);
        }
        // 2. Create and store new code.
        EmailConfirmationCode code = new EmailConfirmationCode()
        {
            User = user,
            UserId = user.Id,
            Code = Guid.NewGuid(),
            ExpirationDate = DateTimeOffset.UtcNow.AddMinutes(20) // 20 minutes.
        };
        code = await this._emailConfirmationCodeRepository.Create(code);
        // 3. Send email.
        // 4. Send the new password on an email.
        this._logger.LogInformation("Attempting to send code to new user.");
        string emailSubject = $"puebla - Se necesita activar la cuenta";
        string emailLink = $"{this._webClientSettings.Host}/auth/confirm/{code.Code}";
        string emailHtmlContent = $"puebla - Activación de cuenta <br><br>Para activar el usuario <i>{user.UserName}</i> haga click en el siguiente enlace:<br><br><a href='{emailLink}'>{emailLink}</a><br><br>Este es un mensaje generado automáticamente. Por favor, no respondas a este correo electrónico.";
        await this._emailService.SendEmail(user.Email, emailSubject, emailHtmlContent);

    }

    /// <summary>
    /// Returns list of all available roles.
    /// </summary>
    /// <returns></returns>
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = $"{ApiRoles.Admin}")]
    [HttpGet("roles")]
    public async Task<ActionResult> GetAllRoles()
    {
        try
        {
            var roles = (await this._roleManager.Roles.ToListAsync());
            var result = roles.Select(role => new RoleResponse()
            {
                Name = role.Name,
            }).ToList();
            return Ok(ResponseHelper.SuccessfulResponse<List<RoleResponse>>(
                result
            ));
        }
        catch (Exception e)
        {
            return ErrorHelper.Internal(this._logger, e.StackTrace);
        }
    }

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = $"{ApiRoles.Admin}")]
    [HttpDelete("users/{userId}")]
    public async Task<ActionResult> Delete(string userId)
    {
        try
        {
            // 1. Get user using id obtained from path parameter
            ApplicationUser user = await this._userManager.FindByIdAsync(userId);
            if (user == null)
                return BadRequest(this.GenerateUnsuccessfulAuthenticationResponse("No existe un usuario con este ID."));

            // 2. Don't allow admin user to be deleted.
            if (user.UserName == "admin")
                return BadRequest(this.GenerateUnsuccessfulAuthenticationResponse("No se puede desactivar el usuario administrador."));

            string username = user.UserName;

            // 3. Delete the user.
            // TODO: Add non-admin or manager user trying to delete other user check.
            await this._userManager.DeleteAsync(user);

            // 5. Return response.
            return Ok(ResponseHelper.SuccessfulResponse($"Cuenta {username} fue eliminada."));
        }
        catch (Exception e)
        {
            return ErrorHelper.Internal(this._logger, e.StackTrace);
        }
    }





    #endregion

    #region Helpers
    private async Task<Response<AuthResponse>> GenerateSuccessfulAuthenticationResponse(ApplicationUser user)
    {
        // IMPORTANT: Response (the object "Response.Headers", not the class) comes from the HttpContext 
        // from the incoming HTTP request.

        // Generate a JWT, refresh token, and add the roles to the response object.
        string jwt = await TokenHelper.GenerateJWTToken(user, _jwtConfiguration,
         _context, _userManager, _roleManager);

        // Add JWT to response's headers.
        Response.Headers.Append("Authorization", $"Bearer {jwt}");

        return new Response<AuthResponse>()
        {
            Result = true,
            Object = new()
            {
                // Add personal information.
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email!
            }
        };
    }
    private Response<AuthResponse> GenerateUnsuccessfulAuthenticationResponse(string error)
    {
        return ResponseHelper.UnsuccessfulResponse<AuthResponse>(new List<string>(){
            error
        });
    }
    private Response<AuthResponse> GenerateUnsuccessfulAuthenticationResponse(List<string> errors)
    {
        return ResponseHelper.UnsuccessfulResponse<AuthResponse>(errors);
    }
    #endregion
};

