using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MicrosoftWord.Core.Services;
using MicrosoftWord.Core.Models;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DocumentController : ControllerBase
{
    private readonly IDocumentService documentService;

    public DocumentController(IDocumentService documentService)
    {
        this.documentService = documentService;
    }

    [HttpPost]
    public async Task<ActionResult<DocumentDto>> CreateDocument(CreateDocumentRequest request)
    {
        // Create a new document using the document service
        var newDocument = await documentService.CreateDocument(request);
        
        // Return a 201 Created response with the new document
        return CreatedAtAction(nameof(GetDocument), new { id = newDocument.Id }, newDocument);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DocumentDto>> GetDocument(string id)
    {
        // Retrieve the document from the document service
        var document = await documentService.GetDocument(id);
        
        // If the document is not found, return a 404 Not Found response
        if (document == null)
        {
            return NotFound();
        }
        
        // Return the document with a 200 OK response
        return Ok(document);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateDocument(string id, UpdateDocumentRequest request)
    {
        // Update the document using the document service
        var result = await documentService.UpdateDocument(id, request);
        
        // If the update fails (e.g., document not found), return a 404 Not Found response
        if (!result)
        {
            return NotFound();
        }
        
        // Return a 204 No Content response on successful update
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteDocument(string id)
    {
        // Delete the document using the document service
        var result = await documentService.DeleteDocument(id);
        
        // If the deletion fails (e.g., document not found), return a 404 Not Found response
        if (!result)
        {
            return NotFound();
        }
        
        // Return a 204 No Content response on successful deletion
        return NoContent();
    }

    [HttpGet("{id}/collaborators")]
    public async Task<ActionResult<IEnumerable<CollaboratorDto>>> GetDocumentCollaborators(string id)
    {
        // Retrieve the list of collaborators for the document
        var collaborators = await documentService.GetDocumentCollaborators(id);
        
        // Return the list of collaborators with a 200 OK response
        return Ok(collaborators);
    }

    [HttpPost("{id}/collaborators")]
    public async Task<ActionResult> AddCollaborator(string id, AddCollaboratorRequest request)
    {
        // Add a collaborator to the document using the document service
        await documentService.AddCollaborator(id, request.Email);
        
        // Return a 200 OK response
        return Ok();
    }

    [HttpDelete("{id}/collaborators/{collaboratorId}")]
    public async Task<ActionResult> RemoveCollaborator(string id, string collaboratorId)
    {
        // Remove a collaborator from the document using the document service
        await documentService.RemoveCollaborator(id, collaboratorId);
        
        // Return a 204 No Content response
        return NoContent();
    }

    [HttpGet("{id}/versions")]
    public async Task<ActionResult<IEnumerable<DocumentVersionDto>>> GetDocumentVersions(string id)
    {
        // Retrieve the version history of the document
        var versions = await documentService.GetDocumentVersions(id);
        
        // Return the list of versions with a 200 OK response
        return Ok(versions);
    }

    [HttpPost("{id}/versions/{versionId}/revert")]
    public async Task<ActionResult<DocumentDto>> RevertToVersion(string id, string versionId)
    {
        // Revert the document to a specific version using the document service
        var revertedDocument = await documentService.RevertToVersion(id, versionId);
        
        // Return the reverted document with a 200 OK response
        return Ok(revertedDocument);
    }
}