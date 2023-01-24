using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApiNet7JWT.Models;

namespace WebApiNet7JWT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        public static User user = new User();

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

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return BadRequest("Wrong password");  // In prod would use different verbage so an attacker would not know if the user name
                                                      // or password was incorrect; such as verifying the user name and password toghether and
                                                      // use a generic user name or password incorrect with out Iding which one... 
            }

            return Ok(user);    //If everything OK return the user...
        }

    }
}
