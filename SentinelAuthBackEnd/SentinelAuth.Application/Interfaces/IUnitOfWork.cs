using System;
using System.Collections.Generic;
using System.Text;

namespace SentinelAuth.Application.Interfaces
{
    public interface IUnitOfWork
    {
        Task CommitAsync(CancellationToken cancellationToken = default);
    }
}
