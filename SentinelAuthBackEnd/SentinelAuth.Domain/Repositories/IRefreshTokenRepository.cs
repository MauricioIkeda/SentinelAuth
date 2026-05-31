using SentinelAuth.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SentinelAuth.Domain.Repositories
{
    public interface IRefreshTokenRepository
    {
        Task AddAsync(
            RefreshToken refreshToken,
            CancellationToken cancellationToken = default
        );

        void Remove(RefreshToken refreshToken);

        Task<RefreshToken?> GetByTokenHashAsync(
            string tokenHash,
            CancellationToken cancellationToken = default
        );
    }
}
