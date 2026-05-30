using System;
using System.Collections.Generic;
using System.Text;

namespace SentinelAuth.Domain.Shared
{
    public enum ErrorType
    {
        Failure = 0,
        Validation = 1,
        NotFound = 2,
        Conflict = 3,
    }
}
