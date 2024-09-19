using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.Word.Api.Models
{
    /// <summary>
    /// Represents a user in the Microsoft Word application
    /// </summary>
    [Table("Users")]
    public class User
    {
        /// <summary>
        /// Unique identifier for the user
        /// </summary>
        [Key]
        [Required]
        public string Id { get; set; }

        /// <summary>
        /// Username of the user
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Username { get; set; }

        /// <summary>
        /// Email address of the user
        /// </summary>
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        /// <summary>
        /// Hashed password of the user
        /// </summary>
        [Required]
        public string PasswordHash { get; set; }

        /// <summary>
        /// First name of the user
        /// </summary>
        [MaxLength(50)]
        public string FirstName { get; set; }

        /// <summary>
        /// Last name of the user
        /// </summary>
        [MaxLength(50)]
        public string LastName { get; set; }

        /// <summary>
        /// Timestamp of when the user account was created
        /// </summary>
        [Required]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Timestamp of the user's last login
        /// </summary>
        public DateTime? LastLoginAt { get; set; }

        /// <summary>
        /// Indicates whether the user account is active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Collection of documents owned by the user
        /// </summary>
        [InverseProperty("Owner")]
        public ICollection<Document> OwnedDocuments { get; set; }

        /// <summary>
        /// Collection of documents the user is collaborating on
        /// </summary>
        public ICollection<UserDocument> CollaboratingDocuments { get; set; }

        /// <summary>
        /// Collection of comments made by the user
        /// </summary>
        public ICollection<Comment> Comments { get; set; }

        /// <summary>
        /// User-specific settings
        /// </summary>
        public UserSettings Settings { get; set; }

        /// <summary>
        /// Gets the full name of the user
        /// </summary>
        /// <returns>The full name of the user</returns>
        public string FullName()
        {
            // Combine FirstName and LastName
            string fullName = $"{FirstName} {LastName}";

            // Trim any leading or trailing whitespace
            fullName = fullName.Trim();

            // Return the combined name
            return fullName;
        }

        /// <summary>
        /// Adds a document to the user's owned documents
        /// </summary>
        /// <param name="document">The document to add</param>
        public void AddOwnedDocument(Document document)
        {
            // Add the document to the OwnedDocuments collection
            OwnedDocuments.Add(document);

            // Set the document's OwnerId to this user's Id
            document.OwnerId = this.Id;
        }

        /// <summary>
        /// Adds a document collaboration for the user
        /// </summary>
        /// <param name="document">The document to collaborate on</param>
        public void AddCollaboration(Document document)
        {
            // Create a new UserDocument object
            var userDocument = new UserDocument
            {
                // Set the UserDocument's UserId to this user's Id
                UserId = this.Id,
                // Set the UserDocument's DocumentId to the given document's Id
                DocumentId = document.Id
            };

            // Add the UserDocument to the CollaboratingDocuments collection
            CollaboratingDocuments.Add(userDocument);
        }
    }
}