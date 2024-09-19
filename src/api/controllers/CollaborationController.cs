using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using MicrosoftWord.Core.Services;
using MicrosoftWord.Core.Models;
using System.Threading.Tasks;

namespace MicrosoftWord.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CollaborationController : ControllerBase
    {
        private readonly ICollaborationService collaborationService;
        private readonly IHubContext<CollaborationHub> hubContext;

        public CollaborationController(ICollaborationService collaborationService, IHubContext<CollaborationHub> hubContext)
        {
            this.collaborationService = collaborationService;
            this.hubContext = hubContext;
        }

        [HttpPost("{documentId}/join")]
        public async Task<ActionResult<SessionInfo>> JoinSession(string documentId)
        {
            // Get the current user's ID from the authenticated user
            var userId = User.Identity.Name;

            // Call collaborationService.JoinSession(documentId, userId)
            var sessionInfo = await collaborationService.JoinSession(documentId, userId);

            // If join fails, return BadRequest
            if (sessionInfo == null)
            {
                return BadRequest("Failed to join the session.");
            }

            // Notify other collaborators via SignalR hub
            await hubContext.Clients.Group(documentId).SendAsync("UserJoined", userId);

            // Return Ok with the session information
            return Ok(sessionInfo);
        }

        [HttpPost("{documentId}/leave")]
        public async Task<ActionResult> LeaveSession(string documentId)
        {
            // Get the current user's ID from the authenticated user
            var userId = User.Identity.Name;

            // Call collaborationService.LeaveSession(documentId, userId)
            await collaborationService.LeaveSession(documentId, userId);

            // Notify other collaborators via SignalR hub
            await hubContext.Clients.Group(documentId).SendAsync("UserLeft", userId);

            // Return NoContent
            return NoContent();
        }

        [HttpPost("{documentId}/sync")]
        public async Task<ActionResult> SyncChanges(string documentId, SyncChangesRequest request)
        {
            // Get the current user's ID from the authenticated user
            var userId = User.Identity.Name;

            // Call collaborationService.SyncChanges(documentId, userId, request.Changes)
            await collaborationService.SyncChanges(documentId, userId, request.Changes);

            // Broadcast changes to other collaborators via SignalR hub
            await hubContext.Clients.Group(documentId).SendAsync("ChangesSynced", userId, request.Changes);

            // Return Ok
            return Ok();
        }

        [HttpGet("{documentId}/collaborators")]
        public async Task<ActionResult<IEnumerable<CollaboratorInfo>>> GetCollaborators(string documentId)
        {
            // Call collaborationService.GetCollaborators(documentId)
            var collaborators = await collaborationService.GetCollaborators(documentId);

            // Return Ok with the list of collaborators
            return Ok(collaborators);
        }

        [HttpPost("{documentId}/lock")]
        public async Task<ActionResult> LockSection(string documentId, LockSectionRequest request)
        {
            // Get the current user's ID from the authenticated user
            var userId = User.Identity.Name;

            // Call collaborationService.LockSection(documentId, userId, request.SectionId)
            var lockResult = await collaborationService.LockSection(documentId, userId, request.SectionId);

            if (!lockResult)
            {
                return BadRequest("Failed to lock the section.");
            }

            // Notify other collaborators about the locked section via SignalR hub
            await hubContext.Clients.Group(documentId).SendAsync("SectionLocked", userId, request.SectionId);

            // Return Ok
            return Ok();
        }

        [HttpPost("{documentId}/unlock")]
        public async Task<ActionResult> UnlockSection(string documentId, UnlockSectionRequest request)
        {
            // Get the current user's ID from the authenticated user
            var userId = User.Identity.Name;

            // Call collaborationService.UnlockSection(documentId, userId, request.SectionId)
            var unlockResult = await collaborationService.UnlockSection(documentId, userId, request.SectionId);

            if (!unlockResult)
            {
                return BadRequest("Failed to unlock the section.");
            }

            // Notify other collaborators about the unlocked section via SignalR hub
            await hubContext.Clients.Group(documentId).SendAsync("SectionUnlocked", userId, request.SectionId);

            // Return Ok
            return Ok();
        }
    }
}