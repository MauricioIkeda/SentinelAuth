using Microsoft.AspNetCore.Identity;
using SentinelAuth.Application.Interfaces;
using SentinelAuth.Domain.Entities;

namespace SentinelAuth.Infrastructure.Security;

public class MicrosoftPasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<User> _passwordHasher = new();
    
    public string HashPassword(string password)
    {
        return _passwordHasher.HashPassword(null!, password);
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        var result =
            _passwordHasher.VerifyHashedPassword(null!, hashedPassword: hashedPassword, providedPassword: password);
        
        return result == PasswordVerificationResult.Success || result == PasswordVerificationResult.SuccessRehashNeeded;
    }
}