using Microsoft.EntityFrameworkCore;
using PueblaApi.Database;
using PueblaApi.Exceptions;
using PueblaApi.Repositories.Interfaces;
using System.Linq.Expressions;
using System;
using PueblaApi.Entities;

namespace PueblaApi.Repositories
{
    public class EmailConfirmationCodeRepository : IEmailConfirmationCodeRepository
    {
        private readonly ApplicationDbContext _context;

        public EmailConfirmationCodeRepository(ApplicationDbContext context)
        {
            this._context = context;
        }

        public async Task<bool> Any(Expression<Func<EmailConfirmationCode, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await this._context.EmailConfirmationCodes.AnyAsync(predicate, cancellationToken);
        }

        public async Task<EmailConfirmationCode> Create(EmailConfirmationCode item)
        {
            this._context.EmailConfirmationCodes.Add(item);
            var result = await this._context.SaveChangesAsync();
            if (result == 0)
                throw new ApiException("No se añadió EmailConfirmationCode a a base de datos.");
            return item;
        }

        public async Task Delete(EmailConfirmationCode item)
        {
            this._context.EmailConfirmationCodes.Remove(item);
            int result = await this._context.SaveChangesAsync();
            if (result == 0)
                throw new ApiException("No se eliminó EmailActivationCodea en base de datos.");
        }

        public async Task<EmailConfirmationCode> Update(EmailConfirmationCode item)
        {
            var result = await this._context.SaveChangesAsync();
            if (result == 0)
                throw new ApiException("No se actualizó EmailActivationCodea en base de datos.");
            return item;
        }

        public async Task<EmailConfirmationCode> GetByCode(Guid code)
        {
            IQueryable<EmailConfirmationCode> query = this._context.EmailConfirmationCodes.AsQueryable()
                .Include(item =>item.User);
            return await query.FirstOrDefaultAsync(i => i.Code == code);
        }

        public async Task<EmailConfirmationCode> GetByUser(ApplicationUser user)
        {
            IQueryable<EmailConfirmationCode> query = this._context.EmailConfirmationCodes.AsQueryable()
                .Where(i => i.User == user)
                .Include(item => item.User);
            return await query.FirstOrDefaultAsync();
        }

        public async Task DeleteRange(List<EmailConfirmationCode> codes)
        {
            this._context.RemoveRange(codes);
            await this._context.SaveChangesAsync();
        }
    }
}
