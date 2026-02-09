using EStore.API.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EStore.API.Services
{
    public class  TokenService
    {
        private readonly IConfiguration _config;
        public TokenService(IConfiguration config)
        {
            _config = config;
        }
        public string CreateToken(User user)
        { 
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier,user.Id.ToString()),
                new Claim(ClaimTypes.Email,user.Email),
                new Claim(ClaimTypes.Role,user.Role)

            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _config["Jwt:key"]));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddMinutes(
                    Convert.ToDouble(_config["Jwt:DurationInMinutes"])),
                signingCredentials: creds,
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"]
                );
            return new JwtSecurityTokenHandler().WriteToken(token);


        }
    }
}
