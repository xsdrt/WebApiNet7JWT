using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebApiNet7JWT.Models;

namespace WebApiNet7JWT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        public static User user = new User();
        private readonly IConfiguration _configuration;

        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("register")]
        public ActionResult<User> Register(UserDto request)         //Tested this using swagger; the password salt is generated with a new salt every time
                                                                    //though the plain text password remains the same.  So if differnt users  use the
                                                                    //same password ; will not matter as the salt will be different...
        {
            string passwordHash
                = BCrypt.Net.BCrypt.HashPassword(request.Password); //Hoover over the HashPassword and see that this automatically generates a Password salt...

            user.Username = request.Username;
            user.PasswordHash = passwordHash;

            return Ok(user);
        }

        [HttpPost("Login")]
        public ActionResult<User> Login(UserDto request)
        {
            if (user.Username != request.Username)      //First check if their is an actual registered user...
            {
                return BadRequest("User not found.");

            }

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))     //Hoover over the Verify for a explanation of what this method does...
            {
                return BadRequest("Wrong password");  // In prod would use different verbage so an attacker would not know if the user name
                                                      // or password was incorrect; such as verifying the user name and password together and
                                                      // use a generic user name or password incorrect with out Iding which one was incorrect... 
            }

            string token = CreateToken(user); //Create the token using the CreateToken method providing the user obj.....

            return Ok(token);    //If everything OK return the token...
        }

        //Create a private method that creates a JWT...

        private string CreateToken(User user)       // Cretae the token using the user obj
        {
            List<Claim> claims = new List<Claim>()      // make sure and install the using directive System.Security.Claims...
            {
                new Claim(ClaimTypes.Name, user.Username)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration.GetSection("AppSettings:Token").Value!));        //use this (value for the key) in the AppSettings.Json file , in real dev MS secrtes mangager
                                                                                //and in prod sometype of vault to store (Azure key vault,
                                                                                //AWS whatever secrets manager etc...your flav...).

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(           // make sure and add System.IdentityModel.Tokens.Jwt package to using directives...
                    claims:claims,
                    expires: DateTime.Now.AddDays(1),   // could make this hours or seconds and implement a refresh token if using a DB...
                    signingCredentials: creds
                );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token); //Write the token containg he claims and then return the jwt...

            return jwt;
        }

    }
}
