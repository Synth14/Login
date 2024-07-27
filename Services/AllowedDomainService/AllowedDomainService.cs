using Login.Data;
using Login.Models;
using Microsoft.EntityFrameworkCore;

namespace Login.Services.AllowedDomainService
{
    public class AllowedDomainService
    {
        private readonly ApplicationDbContext _context;

        public AllowedDomainService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AddAllowedDomainAsync(string domainName)
        {
            if (await _context.AllowedDomains.AnyAsync(d => d.DomainName == domainName))
            {
                return false;
            }

            _context.AllowedDomains.Add(new AllowedDomain { DomainName = domainName });
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
