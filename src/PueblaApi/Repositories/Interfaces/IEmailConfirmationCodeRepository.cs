using PueblaApi.Entities;
using PueblaApi.Repositories.Interfaces;
using System;

namespace PueblaApi.Repositories.Interfaces;
public interface IEmailConfirmationCodeRepository : IRepository<EmailConfirmationCode>
{
    Task DeleteRange(List<EmailConfirmationCode> codes);
    Task<EmailConfirmationCode?> GetByCode(Guid code);
    Task<EmailConfirmationCode> GetByUser(ApplicationUser user);

}
