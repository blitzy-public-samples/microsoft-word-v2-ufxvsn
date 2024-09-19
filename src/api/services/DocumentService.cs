using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MicrosoftWord.Core.Models;
using MicrosoftWord.Core.Interfaces;
using MicrosoftWord.Core.Exceptions;

public class DocumentService : IDocumentService
{
    private readonly ApplicationDbContext dbContext;
    private readonly IUserService userService;

    public DocumentService(ApplicationDbContext context, IUserService userService)
    {
        // Assign context to this.dbContext
        this.dbContext = context;
        // Assign userService to this.userService
        this.userService = userService;
    }

    public async Task<DocumentDto> CreateDocument(CreateDocumentRequest request, string userId)
    {
        // Validate the request
        if (request == null || string.IsNullOrEmpty(request.Title))
        {
            throw new ArgumentException("Invalid request");
        }

        // Create a new Document object
        var document = new Document
        {
            // Set properties based on the request
            Title = request.Title,
            Content = request.Content,
            // Set OwnerId to userId
            OwnerId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Add the document to dbContext
        await dbContext.Documents.AddAsync(document);
        // Save changes to the database
        await dbContext.SaveChangesAsync();

        // Create and return a DocumentDto
        return new DocumentDto(document);
    }

    public async Task<DocumentDto> GetDocument(string id, string userId)
    {
        // Find the document in the database
        var document = await dbContext.Documents.FindAsync(id);

        // If not found, throw NotFoundException
        if (document == null)
        {
            throw new NotFoundException("Document not found");
        }

        // Check if the user has access to the document
        if (document.OwnerId != userId && !await dbContext.UserDocuments.AnyAsync(ud => ud.DocumentId == id && ud.UserId == userId))
        {
            // If no access, throw UnauthorizedException
            throw new UnauthorizedException("User does not have access to this document");
        }

        // Create and return a DocumentDto
        return new DocumentDto(document);
    }

    public async Task<DocumentDto> UpdateDocument(string id, UpdateDocumentRequest request, string userId)
    {
        // Find the document in the database
        var document = await dbContext.Documents.FindAsync(id);

        // If not found, throw NotFoundException
        if (document == null)
        {
            throw new NotFoundException("Document not found");
        }

        // Check if the user has permission to edit
        if (document.OwnerId != userId && !await dbContext.UserDocuments.AnyAsync(ud => ud.DocumentId == id && ud.UserId == userId))
        {
            throw new UnauthorizedException("User does not have permission to edit this document");
        }

        // Update document properties based on the request
        document.Title = request.Title ?? document.Title;
        document.Content = request.Content ?? document.Content;
        document.UpdatedAt = DateTime.UtcNow;

        // Create a new Version
        var version = new DocumentVersion
        {
            DocumentId = document.Id,
            Content = document.Content,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };
        await dbContext.DocumentVersions.AddAsync(version);

        // Save changes to the database
        await dbContext.SaveChangesAsync();

        // Create and return a DocumentDto
        return new DocumentDto(document);
    }

    public async Task DeleteDocument(string id, string userId)
    {
        // Find the document in the database
        var document = await dbContext.Documents.FindAsync(id);

        // If not found, throw NotFoundException
        if (document == null)
        {
            throw new NotFoundException("Document not found");
        }

        // Check if the user is the owner
        if (document.OwnerId != userId)
        {
            // If not owner, throw UnauthorizedException
            throw new UnauthorizedException("Only the owner can delete this document");
        }

        // Remove the document from dbContext
        dbContext.Documents.Remove(document);

        // Save changes to the database
        await dbContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<CollaboratorDto>> GetDocumentCollaborators(string id, string userId)
    {
        // Find the document in the database
        var document = await dbContext.Documents.FindAsync(id);

        // If not found, throw NotFoundException
        if (document == null)
        {
            throw new NotFoundException("Document not found");
        }

        // Check if the user has access to the document
        if (document.OwnerId != userId && !await dbContext.UserDocuments.AnyAsync(ud => ud.DocumentId == id && ud.UserId == userId))
        {
            throw new UnauthorizedException("User does not have access to this document");
        }

        // Retrieve collaborators from UserDocument table
        var collaborators = await dbContext.UserDocuments
            .Where(ud => ud.DocumentId == id)
            .Select(ud => ud.User)
            .ToListAsync();

        // Create and return a list of CollaboratorDto objects
        return collaborators.Select(c => new CollaboratorDto(c));
    }

    public async Task AddCollaborator(string id, string collaboratorEmail, string userId)
    {
        // Find the document in the database
        var document = await dbContext.Documents.FindAsync(id);

        // If not found, throw NotFoundException
        if (document == null)
        {
            throw new NotFoundException("Document not found");
        }

        // Check if the user is the owner
        if (document.OwnerId != userId)
        {
            throw new UnauthorizedException("Only the owner can add collaborators");
        }

        // Find the collaborator user by email
        var collaborator = await userService.GetUserByEmail(collaboratorEmail);

        // If collaborator not found, throw NotFoundException
        if (collaborator == null)
        {
            throw new NotFoundException("Collaborator not found");
        }

        // Add a new UserDocument entry
        var userDocument = new UserDocument
        {
            DocumentId = id,
            UserId = collaborator.Id
        };
        await dbContext.UserDocuments.AddAsync(userDocument);

        // Save changes to the database
        await dbContext.SaveChangesAsync();
    }

    public async Task RemoveCollaborator(string id, string collaboratorId, string userId)
    {
        // Find the document in the database
        var document = await dbContext.Documents.FindAsync(id);

        // If not found, throw NotFoundException
        if (document == null)
        {
            throw new NotFoundException("Document not found");
        }

        // Check if the user is the owner
        if (document.OwnerId != userId)
        {
            throw new UnauthorizedException("Only the owner can remove collaborators");
        }

        // Find the UserDocument entry
        var userDocument = await dbContext.UserDocuments.FirstOrDefaultAsync(ud => ud.DocumentId == id && ud.UserId == collaboratorId);

        // If not found, throw NotFoundException
        if (userDocument == null)
        {
            throw new NotFoundException("Collaborator not found for this document");
        }

        // Remove the UserDocument entry
        dbContext.UserDocuments.Remove(userDocument);

        // Save changes to the database
        await dbContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<DocumentVersionDto>> GetDocumentVersions(string id, string userId)
    {
        // Find the document in the database
        var document = await dbContext.Documents.FindAsync(id);

        // If not found, throw NotFoundException
        if (document == null)
        {
            throw new NotFoundException("Document not found");
        }

        // Check if the user has access to the document
        if (document.OwnerId != userId && !await dbContext.UserDocuments.AnyAsync(ud => ud.DocumentId == id && ud.UserId == userId))
        {
            throw new UnauthorizedException("User does not have access to this document");
        }

        // Retrieve versions from the database
        var versions = await dbContext.DocumentVersions
            .Where(v => v.DocumentId == id)
            .OrderByDescending(v => v.CreatedAt)
            .ToListAsync();

        // Create and return a list of DocumentVersionDto objects
        return versions.Select(v => new DocumentVersionDto(v));
    }

    public async Task<DocumentDto> RevertToVersion(string id, string versionId, string userId)
    {
        // Find the document in the database
        var document = await dbContext.Documents.FindAsync(id);

        // If not found, throw NotFoundException
        if (document == null)
        {
            throw new NotFoundException("Document not found");
        }

        // Check if the user has permission to edit
        if (document.OwnerId != userId && !await dbContext.UserDocuments.AnyAsync(ud => ud.DocumentId == id && ud.UserId == userId))
        {
            throw new UnauthorizedException("User does not have permission to edit this document");
        }

        // Find the specified version
        var version = await dbContext.DocumentVersions.FirstOrDefaultAsync(v => v.Id == versionId && v.DocumentId == id);

        // If version not found, throw NotFoundException
        if (version == null)
        {
            throw new NotFoundException("Version not found");
        }

        // Update document content with version content
        document.Content = version.Content;
        document.UpdatedAt = DateTime.UtcNow;

        // Create a new Version to record the revert
        var newVersion = new DocumentVersion
        {
            DocumentId = document.Id,
            Content = document.Content,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };
        await dbContext.DocumentVersions.AddAsync(newVersion);

        // Save changes to the database
        await dbContext.SaveChangesAsync();

        // Create and return a DocumentDto
        return new DocumentDto(document);
    }
}