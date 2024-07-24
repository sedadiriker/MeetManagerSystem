using Microsoft.AspNetCore.Mvc;
using api.Models;
using api.Interfaces;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System;
using api.DTOs.Account;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;

        public AuthController(IUserService userService, ITokenService tokenService, IEmailService emailService)
        {
            _userService = userService;
            _tokenService = tokenService;
            _emailService = emailService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var user = await _userService.AuthenticateAsync(loginDto.Email, loginDto.Password);

            if (user == null)
            {
                return Unauthorized("Geçersiz kimlik bilgileri");
            }

            var token = _tokenService.GenerateToken(user);

            return Ok(new
            {
                Token = token,
                User = new
                {
                    user.FirstName,
                    user.LastName,
                    user.Email,
                    user.ProfilePicture
                }
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            if (await _userService.UserExistsAsync(registerDto.Email))
            {
                return BadRequest("Bu e-posta adresi zaten kayıtlı.");
            }

            var salt = GenerateSalt();
            var hashedPassword = HashPassword(registerDto.Password, salt);

            var newUser = new User
            {
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                Email = registerDto.Email,
                Phone = registerDto.Phone,
                PasswordHash = hashedPassword,
                ProfilePicture = registerDto.ProfilePicture
            };

            await _userService.CreateUserAsync(newUser);
            await _emailService.SendWelcomeEmailAsync(newUser.Email, newUser.FirstName);

            return Ok("Kayıt işlemi başarılı.");
        }

        private string GenerateSalt()
        {
            byte[] salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            return Convert.ToBase64String(salt);
        }

        private string HashPassword(string password, string salt)
        {
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: Convert.FromBase64String(salt),
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

            return $"{salt}.{hashed}";
        }
    }
}
