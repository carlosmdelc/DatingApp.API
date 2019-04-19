using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using VSDatingApp.Dto;
using VSDatingApp.Interfaces;
using VSDatingApp.Models;

namespace VSDatingApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _authRepository;
        private readonly IConfiguration _configuration;

        // Inject the created repository
        public AuthController(IAuthRepository authRepository, IConfiguration configuration)
        {
            _configuration = configuration;
            _authRepository = authRepository;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody]UserForRegisterDto userForRegistrationDto)
        {
            // Validate Request
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            userForRegistrationDto.Username = userForRegistrationDto.Username.ToLower();

            if (await _authRepository.UserExists(userForRegistrationDto.Username))
                return BadRequest("Username already exists");

            var userToCreate = new User
            {
                UserName = userForRegistrationDto.Username
            };

            var createdUser = await _authRepository.Register(userToCreate, userForRegistrationDto.Password);

            return StatusCode(201);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForLoginDto userForLoginDto)
        {
            var userFromRepo = await _authRepository.Login(userForLoginDto.Username.ToLower(), userForLoginDto.Password);

            if (userFromRepo == null)
                return Unauthorized();

            // Build the token - User Id, and username
            // Will have two claims one is the Id and the second is the username
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userFromRepo.Id.ToString()),
                new Claim(ClaimTypes.Name, userFromRepo.UserName)
            };

            // We are going to store this key inside appsettings, as we stored the connection string
            // so we need to inject our Configuration into the Controller -> go to constructor and inject IConfiguration and initialize it. using Microsoft.Extensions.Configuration
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            // This will have all the details sent to client.
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return Ok(new
            {
                // We are writing the token in the response and will be send to the client.
                token = tokenHandler.WriteToken(token)
            });
        }
    }
}