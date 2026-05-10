using Microsoft.AspNetCore.Mvc;
using remoteControllerApp.DTOs;
using remoteControllerApp.Services;

namespace remoteControllerApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly UserService _userService;

    public UserController(UserService userService)
    {
        _userService = userService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto request)
    {
        if (string.IsNullOrWhiteSpace(request.DisplayName))
        {
            return BadRequest(new
            {
                message = "DisplayName is required."
            });
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return BadRequest(new
            {
                message = "Email is required."
            });
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new
            {
                message = "Password is required."
            });
        }

        try
        {
            var user = await _userService.CreateUserAsync(request);

            return Ok(user);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginUserDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return BadRequest(new
            {
                message = "Email is required."
            });
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new
            {
                message = "Password is required."
            });
        }

        var user = await _userService.LoginAsync(request);

        if (user == null)
        {
            return Unauthorized(new
            {
                message = "Invalid email or password."
            });
        }

        return Ok(new
        {
            success = true,
            user
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _userService.GetAllUsersAsync();

        return Ok(users);
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetUserById(string userId)
    {
        var user = await _userService.GetUserByIdAsync(userId);

        if (user == null)
        {
            return NotFound(new
            {
                message = "User not found."
            });
        }

        return Ok(user);
    }

    [HttpPatch("{userId}")]
    public async Task<IActionResult> UpdateUser(
        string userId,
        [FromBody] UpdateUserDto request)
    {
        var updated = await _userService.UpdateUserAsync(userId, request);

        if (!updated)
        {
            return NotFound(new
            {
                message = "User not found."
            });
        }

        return Ok(new
        {
            success = true,
            message = "User updated."
        });
    }

    [HttpPost("{userId}/deactivate")]
    public async Task<IActionResult> DeactivateUser(string userId)
    {
        var deactivated = await _userService.DeactivateUserAsync(userId);

        if (!deactivated)
        {
            return NotFound(new
            {
                message = "User not found."
            });
        }

        return Ok(new
        {
            success = true,
            message = "User deactivated."
        });
    }
}