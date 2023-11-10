using Emergency.Classes;
using Emergency.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Emergency.Services
{
    public interface IJwtService
    {
        public string GenerateToken(Mobile mobile);
        public IEnumerable<Claim> ValidateToken(string token);
    }

    public class JwtService : IJwtService
    {
        private readonly AppSettings _appSettings;

        public JwtService(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }

        public string GenerateToken(Mobile mobile)
        {
            var key = Encoding.ASCII.GetBytes(_appSettings.JWT.Secret);
            var credetial = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);
            Claim[] claimsArr = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, mobile.Id.ToString()),
                new Claim(ClaimTypes.Name , mobile.Owner),
                new Claim(ClaimTypes.Surname , mobile.Owner),
                new Claim(ClaimTypes.Role ,UserRoles.MOBILE),
                new Claim("AndroidId" , mobile.AndroidId),
            };
            var token = new JwtSecurityToken(
                //issuer: _appSettings.JWT.Issuer, 
                //audience: _appSettings.JWT.Audience,
                claims: claimsArr,
                expires: DateTime.UtcNow.AddDays(_appSettings.JWT.ValidityDays),
                signingCredentials: credetial
            );

            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(token);
        }

        public IEnumerable<Claim> ValidateToken(string token)
        {
            if (token == null)
                return null;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.JWT.Secret);
            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                return jwtToken.Claims;
            }
            catch
            {
                // return null if validation fails
                return null;
            }
        }
    }
}