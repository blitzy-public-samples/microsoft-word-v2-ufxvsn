using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using MicrosoftWord.Core.Models;
using MicrosoftWord.Core.Interfaces;
using MicrosoftWord.Core.Exceptions;
using MicrosoftWord.Core.Hubs;

namespace MicrosoftWord.Api.Services
{
    public class CollaborationService : ICollaborationService
    {
        private readonly ApplicationDbContext dbContext;
        private readonly IHubContext<CollaborationHub> hubContext;

        public CollaborationService(ApplicationDbContext context, IHubContext<CollaborationHub> hubContext)
        {
            this.dbContext = context;
            this.hubContext = hubContext;
        }

        public async Task<SessionInfo> JoinSession(string documentId, string userId)
        {
            // Find the document in the database
            var document = await dbContext.Documents.FindAsync(documentId);
            if (document == null)
            {
                throw new NotFoundException("Document not found");
            }

            // Check if the user has access to the document
            if (!await HasAccessToDocument(userId, documentId))
            {
                throw new UnauthorizedException("User does not have access to this document");
            }

            // Create a new CollaborationSession if not exists
            var session = await dbContext.CollaborationSessions.FirstOrDefaultAsync(s => s.DocumentId == documentId);
            if (session == null)
            {
                session = new CollaborationSession { DocumentId = documentId };
                dbContext.CollaborationSessions.Add(session);
            }

            // Add user to the session
            if (!session.Collaborators.Contains(userId))
            {
                session.Collaborators.Add(userId);
            }

            await dbContext.SaveChangesAsync();

            // Notify other collaborators via SignalR
            await hubContext.Clients.Group(documentId).SendAsync("UserJoined", userId);

            // Return SessionInfo object
            return new SessionInfo
            {
                DocumentId = documentId,
                Collaborators = session.Collaborators
            };
        }

        public async Task LeaveSession(string documentId, string userId)
        {
            // Find the collaboration session
            var session = await dbContext.CollaborationSessions.FirstOrDefaultAsync(s => s.DocumentId == documentId);
            if (session == null)
            {
                throw new NotFoundException("Collaboration session not found");
            }

            // Remove user from the session
            session.Collaborators.Remove(userId);

            // If session is empty, remove it
            if (session.Collaborators.Count == 0)
            {
                dbContext.CollaborationSessions.Remove(session);
            }

            await dbContext.SaveChangesAsync();

            // Notify other collaborators via SignalR
            await hubContext.Clients.Group(documentId).SendAsync("UserLeft", userId);
        }

        public async Task SyncChanges(string documentId, string userId, IEnumerable<DocumentChange> changes)
        {
            // Find the document in the database
            var document = await dbContext.Documents.FindAsync(documentId);
            if (document == null)
            {
                throw new NotFoundException("Document not found");
            }

            // Check if the user has access to the document
            if (!await HasAccessToDocument(userId, documentId))
            {
                throw new UnauthorizedException("User does not have access to this document");
            }

            // Apply changes to the document
            foreach (var change in changes)
            {
                ApplyChange(document, change);
            }

            // Save changes to the database
            await dbContext.SaveChangesAsync();

            // Broadcast changes to other collaborators via SignalR
            await hubContext.Clients.Group(documentId).SendAsync("ChangesReceived", userId, changes);
        }

        public async Task<IEnumerable<CollaboratorInfo>> GetCollaborators(string documentId)
        {
            // Find the collaboration session
            var session = await dbContext.CollaborationSessions.FirstOrDefaultAsync(s => s.DocumentId == documentId);
            if (session == null)
            {
                return Enumerable.Empty<CollaboratorInfo>();
            }

            // Retrieve collaborator information
            var collaborators = await dbContext.Users
                .Where(u => session.Collaborators.Contains(u.Id))
                .Select(u => new CollaboratorInfo { UserId = u.Id, UserName = u.UserName })
                .ToListAsync();

            return collaborators;
        }

        public async Task<bool> LockSection(string documentId, string userId, string sectionId)
        {
            // Find the document in the database
            var document = await dbContext.Documents.FindAsync(documentId);
            if (document == null)
            {
                throw new NotFoundException("Document not found");
            }

            // Check if the section is already locked
            if (document.LockedSections.ContainsKey(sectionId))
            {
                return false;
            }

            // Lock the section for the user
            document.LockedSections[sectionId] = userId;
            await dbContext.SaveChangesAsync();

            // Notify other collaborators via SignalR
            await hubContext.Clients.Group(documentId).SendAsync("SectionLocked", sectionId, userId);

            return true;
        }

        public async Task UnlockSection(string documentId, string userId, string sectionId)
        {
            // Find the document in the database
            var document = await dbContext.Documents.FindAsync(documentId);
            if (document == null)
            {
                throw new NotFoundException("Document not found");
            }

            // Check if the section is locked by the user
            if (!document.LockedSections.TryGetValue(sectionId, out var lockingUserId) || lockingUserId != userId)
            {
                throw new UnauthorizedException("User does not have the lock for this section");
            }

            // Unlock the section
            document.LockedSections.Remove(sectionId);
            await dbContext.SaveChangesAsync();

            // Notify other collaborators via SignalR
            await hubContext.Clients.Group(documentId).SendAsync("SectionUnlocked", sectionId);
        }

        public async Task BroadcastCursorPosition(string documentId, string userId, CursorPosition position)
        {
            // Find the collaboration session
            var session = await dbContext.CollaborationSessions.FirstOrDefaultAsync(s => s.DocumentId == documentId);
            if (session == null)
            {
                throw new NotFoundException("Collaboration session not found");
            }

            // Broadcast cursor position to other collaborators via SignalR
            await hubContext.Clients.GroupExcept(documentId, userId).SendAsync("CursorMoved", userId, position);
        }

        public async Task<DocumentState> GetDocumentState(string documentId)
        {
            // Find the document in the database
            var document = await dbContext.Documents.FindAsync(documentId);
            if (document == null)
            {
                throw new NotFoundException("Document not found");
            }

            // Retrieve the current document content and metadata
            return new DocumentState
            {
                DocumentId = document.Id,
                Content = document.Content,
                Version = document.Version,
                LastModified = document.LastModified,
                LockedSections = document.LockedSections
            };
        }

        private async Task<bool> HasAccessToDocument(string userId, string documentId)
        {
            // Implementation of access check logic
            // This is a placeholder and should be replaced with actual access control logic
            return await dbContext.DocumentPermissions.AnyAsync(dp => dp.UserId == userId && dp.DocumentId == documentId);
        }

        private void ApplyChange(Document document, DocumentChange change)
        {
            // Implementation of applying a single change to the document
            // This is a placeholder and should be replaced with actual change application logic
            // depending on the type of change (insert, delete, format, etc.)
        }
    }
}