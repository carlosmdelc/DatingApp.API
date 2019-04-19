using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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

        // Inject the created repository
        public AuthController(IAuthRepository authRepository)
        {
            _authRepository = authRepository;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserForRegisterDto userForRegistrationDto)
        {
            // Validate Request
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
    }
}