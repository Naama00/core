using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Library.Services 
{
    public static class LibraryTokenService
    {
        private static readonly SymmetricSecurityKey key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes("SuperSecretLibraryKeyThatMustBeLong12345678901234567890"));                
        
        private static readonly string issuer = "library-app";
        private static readonly string audience = "library-app";

        public static SecurityToken GetToken(List<Claim> claims)
        {
            return new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2), 
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );
        }

        public static string WriteToken(SecurityToken token)
        {
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public static TokenValidationParameters GetTokenValidationParameters()
        {
            return new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateLifetime = true,
                IssuerSigningKey = key,
                ValidateIssuerSigningKey = true,
                ClockSkew = TimeSpan.Zero
            };
        }
    }
}