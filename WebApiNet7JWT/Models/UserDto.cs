namespace WebApiNet7JWT.Models
{
    public class UserDto
    {
        // FYI: In case wondering , not storing the User in a database for this example so no Id required...
        public required string Username { get; set; }
        public required string Password { get; set; }
    }
}
