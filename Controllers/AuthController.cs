using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using RedMango_API.Data;
using RedMango_API.Model;
using RedMango_API.Model.DTO;
using RedMango_API.Services;
using RedMango_API.Utility;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace RedMango_API.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private string secretKey;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private ApiResponse _response;

        public AuthController(ApplicationDbContext db, IConfiguration configuration, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _db = db;
            secretKey = configuration.GetValue<string>("ApiSettings:Secret");
            _response = new ApiResponse();
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO loginRequestDTO)
        {
            ApplicationUser userFromDb = _db.ApplicationUsers
                .FirstOrDefault(u => u.UserName.ToLower() == loginRequestDTO.UserName.ToLower());
            bool isValid = await _userManager.CheckPasswordAsync(userFromDb, loginRequestDTO.Password);
            if (isValid == false)
            {
                _response.Result = new LoginResponseDTO();
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Username or password is incorrect!");
                return BadRequest(_response);
            }

            //generate JWT Token
            var roles = await _userManager.GetRolesAsync(userFromDb);
            JwtSecurityTokenHandler tokenHandler = new();
            byte[] key = Encoding.ASCII.GetBytes(secretKey);

            SecurityTokenDescriptor tokenDescriptor = new()
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim("fullName", userFromDb.Name),
                    new Claim("id", userFromDb.Id.ToString()),
                    new Claim(ClaimTypes.Email, userFromDb.UserName.ToString()),
                    new Claim(ClaimTypes.Role, roles.FirstOrDefault())

                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);

            LoginResponseDTO loginResponseDTO = new()
            {

                Email = userFromDb.Email,
                Token = tokenHandler.WriteToken(token)
            };

            if (loginResponseDTO.Email == null || string.IsNullOrEmpty(loginResponseDTO.Token))
            {
                _response.Result = new LoginResponseDTO();
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Username or password is incorrect!");
                return BadRequest(_response);
            }
            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            _response.Result = loginResponseDTO;
            return Ok(_response);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody]RegisterRequestDTO registerRequestDTO)
        {
            ApplicationUser userFromDb = _db.ApplicationUsers
                .FirstOrDefault(u => u.UserName.ToLower() == registerRequestDTO.UserName.ToLower());
            if(userFromDb != null)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Username already exists!");
                return BadRequest(_response);

               
            }
            ApplicationUser newUser = new()
            {
                UserName = registerRequestDTO.UserName,
                Email = registerRequestDTO.UserName,
                NormalizedEmail = registerRequestDTO.UserName.ToUpper(),
                Name = registerRequestDTO.Name
            };
            try
            {
                var result = await _userManager.CreateAsync(newUser, registerRequestDTO.Password);
                if (result.Succeeded)
                {
                    if (!_roleManager.RoleExistsAsync(SD.Role_Admin).GetAwaiter().GetResult())
                    {
                        //create role in database
                        await _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin));
                        await _roleManager.CreateAsync(new IdentityRole(SD.Role_Customer));
                    }
                    if (registerRequestDTO.Role.ToLower() == SD.Role_Admin)
                    {
                        await _userManager.AddToRoleAsync(newUser, SD.Role_Admin);
                    }
                    else
                    {
                        await _userManager.AddToRoleAsync(newUser, SD.Role_Customer);
                    }
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    return Ok(_response);
                }
            }
            catch(Exception ex)
            {
                //_response.ErrorMessages.Add(ex.ToString());
            }
            _response.StatusCode = HttpStatusCode.BadRequest;
            _response.IsSuccess = false;
            _response.ErrorMessages.Add("Error while registering");
            return BadRequest(_response);

        }

    }
}

