using Microsoft.AspNetCore.Mvc;
using SGS.Application.DTOs;
using SGS.Application.Interfaces;

namespace SGS.API.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController(IUserService userService) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserCommand command)
        {
            try
            {
                var userDto = await userService.RegisterUserAsync(command);
                return Ok(userDto);
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        [HttpGet("me")]
        public async Task<IActionResult> GetUser([FromQuery] string email)
        {
            var userDto = await userService.GetUserByEmailAsync(email);
            if(userDto == null)
            {
                return NotFound();
            }
            return Ok(userDto);
        }
    }
}