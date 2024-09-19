using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using MicrosoftWord.Core.Services;
using MicrosoftWord.Core.Models;
using MicrosoftWord.Core.Repositories;
using MicrosoftWord.Core.Exceptions;

namespace MicrosoftWord.Tests.Api.Services
{
    public class DocumentServiceTests
    {
        private readonly Mock<IDocumentRepository> mockDocumentRepository;
        private readonly Mock<IUserRepository> mockUserRepository;
        private readonly DocumentService documentService;

        public DocumentServiceTests()
        {
            // Initialize mockDocumentRepository
            mockDocumentRepository = new Mock<IDocumentRepository>();

            // Initialize mockUserRepository
            mockUserRepository = new Mock<IUserRepository>();

            // Create documentService instance with mocked repositories
            documentService = new DocumentService(mockDocumentRepository.Object, mockUserRepository.Object);
        }

        [Fact]
        public async Task CreateDocument_ValidRequest_ReturnsCreatedDocument()
        {
            // Arrange: Set up mock for CreateDocument in repository
            var newDocument = CreateSampleDocument("1");
            mockDocumentRepository.Setup(repo => repo.CreateDocument(It.IsAny<Document>()))
                .ReturnsAsync(newDocument);

            // Act: Call documentService.CreateDocument with valid request
            var result = await documentService.CreateDocument(newDocument);

            // Assert: Verify returned document matches expected
            Assert.Equal(newDocument, result);
        }

        [Fact]
        public async Task GetDocument_ExistingId_ReturnsDocument()
        {
            // Arrange: Set up mock for GetDocument in repository
            var existingDocument = CreateSampleDocument("1");
            mockDocumentRepository.Setup(repo => repo.GetDocument("1"))
                .ReturnsAsync(existingDocument);

            // Act: Call documentService.GetDocument with existing ID
            var result = await documentService.GetDocument("1");

            // Assert: Verify returned document matches expected
            Assert.Equal(existingDocument, result);
        }

        [Fact]
        public async Task GetDocument_NonExistingId_ThrowsNotFoundException()
        {
            // Arrange: Set up mock for GetDocument to return null
            mockDocumentRepository.Setup(repo => repo.GetDocument("nonexistent"))
                .ReturnsAsync((Document)null);

            // Act & Assert: Verify documentService.GetDocument throws NotFoundException
            await Assert.ThrowsAsync<NotFoundException>(() => documentService.GetDocument("nonexistent"));
        }

        [Fact]
        public async Task UpdateDocument_ValidRequest_UpdatesDocument()
        {
            // Arrange: Set up mocks for GetDocument and UpdateDocument
            var existingDocument = CreateSampleDocument("1");
            mockDocumentRepository.Setup(repo => repo.GetDocument("1"))
                .ReturnsAsync(existingDocument);
            mockDocumentRepository.Setup(repo => repo.UpdateDocument(It.IsAny<Document>()))
                .ReturnsAsync(existingDocument);

            // Act: Call documentService.UpdateDocument with valid request
            var updatedDocument = new Document { Id = "1", Title = "Updated Title" };
            var result = await documentService.UpdateDocument(updatedDocument);

            // Assert: Verify document was updated in repository
            mockDocumentRepository.Verify(repo => repo.UpdateDocument(It.Is<Document>(d => d.Title == "Updated Title")), Times.Once);
            Assert.Equal("Updated Title", result.Title);
        }

        [Fact]
        public async Task DeleteDocument_ExistingId_DeletesDocument()
        {
            // Arrange: Set up mocks for GetDocument and DeleteDocument
            var existingDocument = CreateSampleDocument("1");
            mockDocumentRepository.Setup(repo => repo.GetDocument("1"))
                .ReturnsAsync(existingDocument);
            mockDocumentRepository.Setup(repo => repo.DeleteDocument("1"))
                .Returns(Task.CompletedTask);

            // Act: Call documentService.DeleteDocument with existing ID
            await documentService.DeleteDocument("1");

            // Assert: Verify document was deleted from repository
            mockDocumentRepository.Verify(repo => repo.DeleteDocument("1"), Times.Once);
        }

        [Fact]
        public async Task GetDocumentCollaborators_ExistingId_ReturnsCollaborators()
        {
            // Arrange: Set up mock for GetDocumentCollaborators
            var collaborators = new[] { CreateSampleUser("1"), CreateSampleUser("2") };
            mockDocumentRepository.Setup(repo => repo.GetDocumentCollaborators("1"))
                .ReturnsAsync(collaborators);

            // Act: Call documentService.GetDocumentCollaborators with existing ID
            var result = await documentService.GetDocumentCollaborators("1");

            // Assert: Verify returned collaborators match expected
            Assert.Equal(collaborators, result);
        }

        [Fact]
        public async Task AddCollaborator_ValidRequest_AddsCollaborator()
        {
            // Arrange: Set up mocks for GetDocument and AddCollaborator
            var existingDocument = CreateSampleDocument("1");
            var collaborator = CreateSampleUser("2");
            mockDocumentRepository.Setup(repo => repo.GetDocument("1"))
                .ReturnsAsync(existingDocument);
            mockDocumentRepository.Setup(repo => repo.AddCollaborator("1", "2"))
                .Returns(Task.CompletedTask);

            // Act: Call documentService.AddCollaborator with valid request
            await documentService.AddCollaborator("1", "2");

            // Assert: Verify collaborator was added to the document
            mockDocumentRepository.Verify(repo => repo.AddCollaborator("1", "2"), Times.Once);
        }

        [Fact]
        public async Task RemoveCollaborator_ValidRequest_RemovesCollaborator()
        {
            // Arrange: Set up mocks for GetDocument and RemoveCollaborator
            var existingDocument = CreateSampleDocument("1");
            mockDocumentRepository.Setup(repo => repo.GetDocument("1"))
                .ReturnsAsync(existingDocument);
            mockDocumentRepository.Setup(repo => repo.RemoveCollaborator("1", "2"))
                .Returns(Task.CompletedTask);

            // Act: Call documentService.RemoveCollaborator with valid request
            await documentService.RemoveCollaborator("1", "2");

            // Assert: Verify collaborator was removed from the document
            mockDocumentRepository.Verify(repo => repo.RemoveCollaborator("1", "2"), Times.Once);
        }

        [Fact]
        public async Task GetDocumentVersions_ExistingId_ReturnsVersions()
        {
            // Arrange: Set up mock for GetDocumentVersions
            var versions = new[] { new DocumentVersion { Id = "1", Version = 1 }, new DocumentVersion { Id = "1", Version = 2 } };
            mockDocumentRepository.Setup(repo => repo.GetDocumentVersions("1"))
                .ReturnsAsync(versions);

            // Act: Call documentService.GetDocumentVersions with existing ID
            var result = await documentService.GetDocumentVersions("1");

            // Assert: Verify returned versions match expected
            Assert.Equal(versions, result);
        }

        [Fact]
        public async Task RevertToVersion_ValidRequest_RevertsDocument()
        {
            // Arrange: Set up mocks for GetDocument, GetDocumentVersion, and UpdateDocument
            var existingDocument = CreateSampleDocument("1");
            var oldVersion = new DocumentVersion { Id = "1", Version = 1, Content = "Old content" };
            mockDocumentRepository.Setup(repo => repo.GetDocument("1"))
                .ReturnsAsync(existingDocument);
            mockDocumentRepository.Setup(repo => repo.GetDocumentVersion("1", 1))
                .ReturnsAsync(oldVersion);
            mockDocumentRepository.Setup(repo => repo.UpdateDocument(It.IsAny<Document>()))
                .ReturnsAsync(existingDocument);

            // Act: Call documentService.RevertToVersion with valid request
            await documentService.RevertToVersion("1", 1);

            // Assert: Verify document was reverted to the specified version
            mockDocumentRepository.Verify(repo => repo.UpdateDocument(It.Is<Document>(d => d.Content == "Old content")), Times.Once);
        }

        private Document CreateSampleDocument(string id)
        {
            return new Document
            {
                Id = id,
                Title = "Sample Document",
                Content = "Sample content",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        private User CreateSampleUser(string id)
        {
            return new User
            {
                Id = id,
                Username = $"user{id}",
                Email = $"user{id}@example.com"
            };
        }
    }
}