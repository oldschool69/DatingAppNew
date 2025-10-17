using System;
using API.Entities;
using API.Interfaces;

namespace API.Services;

public class TokenService : ITokenService
{
    public string CreateToken(AppUser user)
    {
        // Implementation for creating a token
        return "token";
    }
}
