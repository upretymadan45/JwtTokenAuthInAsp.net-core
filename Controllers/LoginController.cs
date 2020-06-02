using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using TokenAuthDemo.Models;
using TokenAuthDemo.Repository;

namespace TokenAuthDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IUserRefreshTokenRepository _userRefreshTokenRepository;

        public LoginController(IConfiguration configuration,IUserRefreshTokenRepository userRefreshTokenRepository)
        {
            _configuration = configuration;
            _userRefreshTokenRepository = userRefreshTokenRepository;
        }

        [HttpPost]
        [Route("login")]
        public IActionResult Login([FromBody] User user)
        {
            if(user.Username.Equals("admin") && user.Password.Equals("password"))
            {
                user.Id = Guid.NewGuid().ToString();

                var jwtToken = GenerateJwtToken(user);

                _userRefreshTokenRepository.SaveOrUpdateUserRefreshToken(new UserRefreshToken
                {
                    RefreshToken = jwtToken.RefreshToken,
                    Username = user.Username
                });

                return Ok(jwtToken);
            }

            return BadRequest("Invalid user");
        }

        [HttpPost]
        [Route("refreshToken")]
        public IActionResult RefreshToken([FromBody] JwtToken jwtToken)
        {
            if (jwtToken == null)
            {
                return BadRequest("Invalid request");
            }

            var handler = new JwtSecurityTokenHandler();

            SecurityToken validatedToken;

            IPrincipal principal = handler.ValidateToken(jwtToken.Token, GetTokenValidationParameters(), out validatedToken);

            var username = principal.Identity.Name;

            if (_userRefreshTokenRepository.CheckIfRefreshTokenIsValid(username, jwtToken.RefreshToken))
            {
                var user = new User { Username = username };
                var newJwtToken = GenerateJwtToken(user);

                _userRefreshTokenRepository.SaveOrUpdateUserRefreshToken(new UserRefreshToken
                {
                    Username = user.Username,
                    RefreshToken = newJwtToken.RefreshToken
                });

                return Ok(newJwtToken);
            }

            return BadRequest("Invalid Request");
            
        }

        private TokenValidationParameters GetTokenValidationParameters()
        {
            var securityKey = Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]);

            return new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(securityKey),
                ValidIssuers = new string[] { _configuration["Jwt:Issuer"] },
                ValidAudiences = new string[] { _configuration["Jwt:Issuer"] },
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true
            };
        }

        private JwtToken GenerateJwtToken(User user)
        {
            var securityKey = Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]);

            var claims = new Claim[] {
                    new Claim(ClaimTypes.Name,user.Username),
                    new Claim(ClaimTypes.Email,user.Username)
                };

            var credentials = new SigningCredentials(new SymmetricSecurityKey(securityKey), SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(_configuration["Jwt:Issuer"],
              _configuration["Jwt:Issuer"],
              claims,
              expires: DateTime.Now.AddDays(7),
              signingCredentials: credentials);


            var jwtToken = new JwtToken {RefreshToken = new RefreshTokenGenerator().GenerateRefreshToken(32),
                                        Token = new JwtSecurityTokenHandler().WriteToken(token)};

            return jwtToken;
        }
    }
}