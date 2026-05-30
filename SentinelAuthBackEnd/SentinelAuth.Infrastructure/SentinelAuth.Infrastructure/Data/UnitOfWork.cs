using SentinelAuth.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace SentinelAuth.Infrastructure.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> CommitAsync()
        {
            var success = await _context.SaveChangesAsync() > 0;
            return success;
        }
    }
}
