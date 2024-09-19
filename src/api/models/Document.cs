using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("Documents")]
public class Document
{
    // Unique identifier for the document
    [Key]
    [Required]
    public string Id { get; set; }

    // Title of the document
    [Required]
    [MaxLength(255)]
    public string Title { get; set; }

    // Content of the document in a serialized format
    [Column(TypeName = "nvarchar(max)")]
    public string Content { get; set; }

    // ID of the user who owns the document
    [Required]
    public string OwnerId { get; set; }

    // Navigation property to the owner of the document
    [ForeignKey("OwnerId")]
    [Required]
    public User Owner { get; set; }

    // Timestamp of when the document was created
    [Required]
    public DateTime CreatedAt { get; set; }

    // Timestamp of when the document was last modified
    [Required]
    public DateTime LastModifiedAt { get; set; }

    // Collection of users collaborating on this document
    public ICollection<UserDocument> Collaborators { get; set; }

    // Collection of document versions
    public ICollection<DocumentVersion> Versions { get; set; }

    // Collection of comments on this document
    public ICollection<Comment> Comments { get; set; }

    // Indicates whether the document is publicly accessible
    public bool IsPublic { get; set; }

    // Collection of tags associated with this document
    public ICollection<DocumentTag> Tags { get; set; }

    // Adds a collaborator to the document
    public void AddCollaborator(User user)
    {
        // Check if the user is not already a collaborator
        if (Collaborators.Any(c => c.UserId == user.Id))
        {
            return;
        }

        // Create a new UserDocument object
        var userDocument = new UserDocument
        {
            UserId = user.Id,
            DocumentId = this.Id
        };

        // Add the UserDocument to the Collaborators collection
        Collaborators.Add(userDocument);
    }

    // Removes a collaborator from the document
    public bool RemoveCollaborator(User user)
    {
        // Find the UserDocument for the given user
        var userDocument = Collaborators.FirstOrDefault(c => c.UserId == user.Id);

        // If found, remove it from the Collaborators collection
        if (userDocument != null)
        {
            Collaborators.Remove(userDocument);
            return true;
        }

        return false;
    }

    // Creates a new version of the document
    public DocumentVersion CreateVersion()
    {
        // Create a new DocumentVersion object
        var newVersion = new DocumentVersion
        {
            DocumentId = this.Id,
            Content = this.Content,
            CreatedAt = DateTime.UtcNow,
            VersionNumber = Versions.Count + 1
        };

        // Add the new version to the Versions collection
        Versions.Add(newVersion);

        // Return the created DocumentVersion
        return newVersion;
    }
}