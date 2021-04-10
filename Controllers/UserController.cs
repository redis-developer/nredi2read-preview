using dotnetredis.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace dotnetredis.Controllers
{
    [ApiController]
    [Route("/api/users")]
    public class UserController : Controller
    {

        [HttpPost]
        [Route("create")]
        public void Create(User user)
        {
            var db = Program.GetDatabase();
            var key = $"User:{user.Id}";

            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
            var userHash = Program.ToHashEntries(user);

            db.HashSet(key, userHash);
        }

        [HttpGet]
        [Route("read")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Get(string id)
        {
            var db = Program.GetDatabase();
            var key = $"User:{id}";

            var userHash = db.HashGetAll(key);
            if (userHash.Length == 0) return NotFound();
            
            var user = Program.ConvertFromRedis<User>(userHash);

            return Ok(user);
        }

        [HttpPost]
        [Route("load")]
        public void Load(User[] users)
        {
            foreach (var user in users)
            {
                Create(user);
            }
        }
    }
}