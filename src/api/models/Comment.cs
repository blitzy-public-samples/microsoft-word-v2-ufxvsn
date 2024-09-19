using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace Microsoft.Word.V2.Api.Models
{
    /// <summary>
    /// Represents a comment on a document in the Microsoft Word application
    /// </summary>
    [Table("Comments")]
    public class Comment
    {
        [Key]
        [Required]
        public string Id { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Content { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public DateTime UpdatedAt { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        [Required]
        public User User { get; set; }

        [Required]
        public string DocumentId { get; set; }

        [ForeignKey("DocumentId")]
        [Required]
        public Document Document { get; set; }

        [Required]
        public int Position { get; set; }

        public bool IsResolved { get; set; }

        public string ParentCommentId { get; set; }

        [ForeignKey("ParentCommentId")]
        public Comment ParentComment { get; set; }

        [InverseProperty("ParentComment")]
        public ICollection<Comment> Replies { get; set; }

        /// <summary>
        /// Marks the comment as resolved
        /// </summary>
        public void Resolve()
        {
            // Set IsResolved to true
            IsResolved = true;
            // Update UpdatedAt to the current timestamp
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Marks the comment as unresolved
        /// </summary>
        public void Unresolve()
        {
            // Set IsResolved to false
            IsResolved = false;
            // Update UpdatedAt to the current timestamp
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Adds a reply to this comment
        /// </summary>
        /// <param name="reply">The reply comment to add</param>
        public void AddReply(Comment reply)
        {
            // Set the reply's ParentCommentId to this comment's Id
            reply.ParentCommentId = this.Id;
            // Add the reply to the Replies collection
            Replies.Add(reply);
            // Update UpdatedAt to the current timestamp
            UpdatedAt = DateTime.UtcNow;
        }
    }
}