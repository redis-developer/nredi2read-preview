using NRedi2Read.Models;
using NRedi2Read.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System;
using System.Security.Claims;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

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
            var currentUser = await _userService.GetUserWithEmail(user.Email);
            if (currentUser != null)
            {
                return Conflict("User with that email already exists!");
            }
            user.Id = Guid.NewGuid().ToString();
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

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody]LoginCredentials creds){
            var user = await _userService.ValidateUserCredentials(creds.Email, creds.Password);
            if(user!=null){
                var principal = GetPrincipal(user, Startup.COOKIE_AUTH_SCHEME);
                var authProperties = new AuthenticationProperties
                {
                    AllowRefresh = true,
                    ExpiresUtc = DateTimeOffset.Now.AddDays(1),
                    IsPersistent = true,
                };

                await HttpContext.SignInAsync(Startup.COOKIE_AUTH_SCHEME, principal, authProperties);
                return Json(new User{Email = user.Email, Books = user.Books, Name = user.Name, Id = user.Id});
            }
            return Unauthorized();
        }

        /// <summary>
        /// Get the Claims Principle for the cookie
        /// </summary>
        /// <param name="users"></param>
        /// <param name="authScheme"></param>
        /// <returns></returns>
        private ClaimsPrincipal GetPrincipal(User user, string authScheme){
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email,user.Email),
                new Claim(ClaimTypes.Role,"User")
            };
            return new ClaimsPrincipal(new ClaimsIdentity(claims,authScheme));
        }

        [HttpPost("logout")]
        [Authorize(AuthenticationSchemes = Startup.COOKIE_AUTH_SCHEME)]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return StatusCode(200);
        }
    }
}