using NRedi2Read.Models;
using NRedi2Read.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System;

namespace NRedi2Read.Controllers
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

        /// <summary>
        /// Create a User
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> Create(User user)
        {
            await _userService.Create(user);
            var uri = new Uri($"{Request.Scheme}://{Request.Host}/api/users/{user.Id}");
            return Created(uri, user);
        }

        /// <summary>
        /// Get's a single user
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get(string id)
        {
            try
            {
                var data = await _userService.Read(id);
                return Ok(data);
            }
            catch(RedisKeyNotFoundException)
            {
                return NotFound();
            }
            catch(Exception ex)
            {
                return Problem(ex.ToString());
            }
        }

        /// <summary>
        /// Bulk loads a set of users to seed the database.
        /// </summary>
        /// <param name="users"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("load")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Load(User[] users)
        {
            await _userService.CreateBulk(users);
            return Ok();
        }
    }
}