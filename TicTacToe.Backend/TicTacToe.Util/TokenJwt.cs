using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace TicTacToe.Util
{
    public static class TokenJwt
    {
        private static IConfiguration? _configuration;

        public static void Initialize(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public static string GenerateTempJSONWebToken(string email)
        {
            ArgumentNullException.ThrowIfNull(_configuration);

            var key = _configuration["Jwt:Key"]
                ?? throw new InvalidOperationException("Jwt:Key não encontrado");

            var issuer = _configuration["Jwt:Issuer"]
                ?? throw new InvalidOperationException("Jwt:Issuer não encontrado");

            var hoursStr = _configuration["Jwt:Hours"]
                ?? throw new InvalidOperationException("Jwt:Hours não encontrado");

            if (!int.TryParse(hoursStr, out var hours))
                throw new InvalidOperationException("Jwt:Hours inválido");

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer,
                issuer,
                new[]
                {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
                new Claim(JwtRegisteredClaimNames.UniqueName, email),
                new Claim(JwtRegisteredClaimNames.Email, email)
                },
                expires: DateTime.UtcNow.AddHours(hours),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
