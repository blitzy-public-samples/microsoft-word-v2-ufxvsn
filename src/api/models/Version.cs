using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.Word.Api.Models
{
    /// <summary>
    /// Represents a version of a document in the Microsoft Word application
    /// </summary>
    [Table("Versions")]
    public class Version
    {
        [Key]
        [Required]
        public string Id { get; set; }

        [Required]
        public string DocumentId { get; set; }

        [ForeignKey("DocumentId")]
        [Required]
        public Document Document { get; set; }

        [Required]
        public int VersionNumber { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(max)")]
        public string Content { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public string CreatedById { get; set; }

        [ForeignKey("CreatedById")]
        [Required]
        public User CreatedBy { get; set; }

        [MaxLength(500)]
        public string Comment { get; set; }

        public long Size { get; set; }

        public bool IsCurrentVersion { get; set; }

        /// <summary>
        /// Sets this version as the current version of the document
        /// </summary>
        public void SetAsCurrent()
        {
            // Set IsCurrentVersion to true
            IsCurrentVersion = true;

            // Update the Document's Content with this version's Content
            Document.Content = Content;

            // Update the Document's LastModifiedAt to the current timestamp
            Document.LastModifiedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Calculates and sets the size of the version content
        /// </summary>
        public void CalculateSize()
        {
            // Calculate the size of the Content in bytes
            Size = System.Text.Encoding.UTF8.GetByteCount(Content);
        }

        /// <summary>
        /// Creates a new version from the current state of a document
        /// </summary>
        /// <param name="document">The document to create a version from</param>
        /// <param name="user">The user creating the version</param>
        /// <param name="comment">Optional comment describing the changes</param>
        /// <returns>A new Version instance</returns>
        public static Version CreateFromDocument(Document document, User user, string comment = null)
        {
            // Create a new Version instance
            var version = new Version
            {
                // Set the DocumentId, Content, CreatedById, and Comment properties
                DocumentId = document.Id,
                Content = document.Content,
                CreatedById = user.Id,
                Comment = comment,

                // Set CreatedAt to the current timestamp
                CreatedAt = DateTime.UtcNow,

                // Calculate and set the VersionNumber
                VersionNumber = document.Versions.Count + 1
            };

            // Call CalculateSize()
            version.CalculateSize();

            // Return the new Version instance
            return version;
        }
    }
}