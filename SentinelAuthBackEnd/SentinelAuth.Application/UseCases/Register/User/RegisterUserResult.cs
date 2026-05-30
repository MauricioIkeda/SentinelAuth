using System;
using System.Collections.Generic;
using System.Text;

namespace SentinelAuth.Application.UseCases.Register.User
{
    public class RegisterUserResult
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }
}
