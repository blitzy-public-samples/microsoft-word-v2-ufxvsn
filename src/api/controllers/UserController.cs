using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MicrosoftWord.Core.Services;
using MicrosoftWord.Core.Models;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace MicrosoftWord.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService userService;

        public UserController(IUserService userService)
        {
            this.userService = userService;
        }

        [HttpGet("me")]
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            // Get the current user's ID from the authenticated user
            var userId = User.Identity.Name;

            // Call userService.GetUser(userId)
            var user = await userService.GetUser(userId);

            // If user is null, return NotFound
            if (user == null)
            {
                return NotFound();
            }

            // Return Ok with the user information
            return Ok(user);
        }

        [HttpPut("me")]
        public async Task<ActionResult<UserDto>> UpdateUser(UpdateUserRequest request)
        {
            // Get the current user's ID from the authenticated user
            var userId = User.Identity.Name;

            // Call userService.UpdateUser(userId, request)
            var updatedUser = await userService.UpdateUser(userId, request);

            // If update fails, return BadRequest
            if (updatedUser == null)
            {
                return BadRequest("Failed to update user information");
            }

            // Return Ok with the updated user information
            return Ok(updatedUser);
        }

        [HttpGet("settings")]
        public async Task<ActionResult<UserSettingsDto>> GetUserSettings()
        {
            // Get the current user's ID from the authenticated user
            var userId = User.Identity.Name;

            // Call userService.GetUserSettings(userId)
            var settings = await userService.GetUserSettings(userId);

            // Return Ok with the user settings
            return Ok(settings);
        }

        [HttpPut("settings")]
        public async Task<ActionResult<UserSettingsDto>> UpdateUserSettings(UpdateUserSettingsRequest request)
        {
            // Get the current user's ID from the authenticated user
            var userId = User.Identity.Name;

            // Call userService.UpdateUserSettings(userId, request)
            var updatedSettings = await userService.UpdateUserSettings(userId, request);

            // If update fails, return BadRequest
            if (updatedSettings == null)
            {
                return BadRequest("Failed to update user settings");
            }

            // Return Ok with the updated user settings
            return Ok(updatedSettings);
        }

        [HttpPost("change-password")]
        public async Task<ActionResult> ChangePassword(ChangePasswordRequest request)
        {
            // Get the current user's ID from the authenticated user
            var userId = User.Identity.Name;

            // Call userService.ChangePassword(userId, request.CurrentPassword, request.NewPassword)
            var result = await userService.ChangePassword(userId, request.CurrentPassword, request.NewPassword);

            // If change fails, return BadRequest
            if (!result)
            {
                return BadRequest("Failed to change password");
            }

            // Return NoContent
            return NoContent();
        }

        [HttpGet("documents")]
        public async Task<ActionResult<IEnumerable<DocumentSummaryDto>>> GetUserDocuments()
        {
            // Get the current user's ID from the authenticated user
            var userId = User.Identity.Name;

            // Call userService.GetUserDocuments(userId)
            var documents = await userService.GetUserDocuments(userId);

            // Return Ok with the list of document summaries
            return Ok(documents);
        }

        [HttpGet("collaborations")]
        public async Task<ActionResult<IEnumerable<DocumentSummaryDto>>> GetUserCollaborations()
        {
            // Get the current user's ID from the authenticated user
            var userId = User.Identity.Name;

            // Call userService.GetUserCollaborations(userId)
            var collaborations = await userService.GetUserCollaborations(userId);

            // Return Ok with the list of document summaries
            return Ok(collaborations);
        }
    }
}