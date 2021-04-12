using dotnetredis.Models;
using dotnetredis.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace dotnetredis.Controllers
{
    [ApiController]
    [Route("/api/users")]
    public class UserController : Controller
    {
        
        private readonly UserService _userService;
        
        public UserController(UserService service)
        {
            _userService = service;
        }

        [HttpPost]
        [Route("create")]
        public void Create(User user)
        {
            _userService.Create(user);
        }

        [HttpGet]
        [Route("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Get(long id)
        {
            try
            {
                return Ok(_userService.Read(id));
            }
            catch
            {
                return NoContent();
            }
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