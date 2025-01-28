using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using PueblaApi.Database;
using PueblaApi.Entities;
using PueblaApi.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using PueblaApi.DTOS.Base;
using PueblaApi.DTOS.Auth;

namespace PueblaApi.Helpers;

/**
    Helper methods related to JWT and refresh tokens.
**/
public class TokenHelper
{
    public static async Task<string> GenerateJWTToken(ApplicationUser user, JwtConfiguration _jwtConfig, ApplicationDbContext _context,
                                                                        UserManager<ApplicationUser> _userManager, RoleManager<IdentityRole> _roleManager)
    {

        // 1. Instantiate token handler.
        JwtSecurityTokenHandler jwtTokenHandler = new JwtSecurityTokenHandler();
        // 2. Transform secret (defined in appsettings.json or an app secret.0) to an array of bytes
        byte[] key = Encoding.UTF8.GetBytes(_jwtConfig.Secret);

        // 3. Get all the user's claims.
        IEnumerable<Claim> userClaims = await TokenHelper.GetAllValidClaims(user, _userManager, _roleManager);

        // 4. Create a Token descriptor
        // A token descriptor is where we put the information that goes inside the payload of the token.
        SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor()
        {
            Issuer = _jwtConfig.Issuer,
            Audience = _jwtConfig.Audience,
            TokenType = "JWT",

            Subject = new ClaimsIdentity(userClaims),
            // 5. Define expiration time for the token by reading it from appsettings.json.
            Expires = DateTime.UtcNow.Add( // Equivalent to JwtRegisteredClaimNames.Exp. (Current time + Expiration time)
                TimeSpan.Parse(  // Parse string returned by appsettings.json into a DateTime.
                    _jwtConfig.ExpiryTimeFrame
                )
            ),
            // 6. Define signing credentials.
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256) // Sign the credentials using a symmetric key encrypted with HMAC256.
        };

        // 7. Create a Security Token using the descriptor.
        SecurityToken securityToken = jwtTokenHandler.CreateToken(tokenDescriptor);
        // 8. Create a JWT token using the Security Token and return it.
        return jwtTokenHandler.WriteToken(securityToken);
    }

    // Get all the claims that correspond to a user
    private static async Task<List<Claim>> GetAllValidClaims(ApplicationUser user, UserManager<ApplicationUser> _userManager, RoleManager<IdentityRole> _roleManager)
    {
        // Get all the options
        IdentityOptions _options = new IdentityOptions();
        List<Claim> claims = new List<Claim>(){
                // Each claim is key-value field inside the payload.
                // See: https://jwt.io/ for an example.
                new Claim("Id", user.Id),
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // Unique identifier for the JWT.
                new Claim(JwtRegisteredClaimNames.Iat, DateTime.Now.ToUniversalTime().ToString()), // Issued at.
                new Claim(JwtRegisteredClaimNames.Exp, DateTime.Now.ToUniversalTime().ToString())

                // Use UTC (Universal time) for times.
            };
        // 1. Get all the claims automatically added to the user.
        IEnumerable<Claim> automaticallyAddedClaims = await _userManager.GetClaimsAsync(user);
        // 1.1. Add these claims to the first list we already have (claims).
        claims.AddRange(automaticallyAddedClaims);
        // 2. Get all the users' roles and add them to the claims.
        // 2.1. Get all the users' roles.
        IList<string> userRoles = await _userManager.GetRolesAsync(user);
        foreach (string rolename in userRoles)
        {
            // 2.2. Add CLAIMS related to role.
            // 2.2.1. Find the role using the name.
            IdentityRole role = await _roleManager.FindByNameAsync(rolename);
            if (role != null)
            {
                // 2.2.2 Add them to the claims.
                claims.Add(new Claim(ClaimTypes.Role, rolename));
                // 2.2.3. Get claims related to role.
                IEnumerable<Claim> roleClaims = await _roleManager.GetClaimsAsync(role);
                // 2.2.4. Add claims related to role.
                claims.AddRange(roleClaims);
            }
        }
        // 3. Return claims.
        return claims;
    }

    private static string GenerateRandomString(int length)
    {
        Random random = new Random();
        string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890abcdefghijklmnopqrstuvwxyz";
        return new string(
            Enumerable.Repeat(chars, length) // 1. Build a string where every character (chars) is selected ONCE.
                .Select(
                    result => result[random.Next(result.Length)] // 2. Return a random (between 0 and length) character.
                                                                 // 3. Repeat this process N (length) times.
            ).ToArray());
    }

    public static async Task<ApplicationUser> GetUserFromJWTClaim(ClaimsIdentity identity, UserManager<ApplicationUser> _userManager)
    {
        if (identity == null)
            return null;

        // 2. Extract User ID from claims (Id)
        string userId = identity.FindFirst("Id").Value;
        if (userId.IsNullOrEmpty())
            return null;

        // 3. Search user by id (obtained from token)
        return await _userManager.FindByIdAsync(userId); // Might be null, if it doesn't find the user.
    }
}