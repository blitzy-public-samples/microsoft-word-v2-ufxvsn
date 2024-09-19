using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using MicrosoftWord.Api.Controllers;
using MicrosoftWord.Core.Services;
using MicrosoftWord.Core.Models;
using MicrosoftWord.Core.Exceptions;

namespace MicrosoftWord.Tests.Api.Controllers
{
    public class DocumentControllerTests
    {
        private readonly Mock<IDocumentService> mockDocumentService;
        private readonly DocumentController controller;

        public DocumentControllerTests()
        {
            // Initialize mockDocumentService
            mockDocumentService = new Mock<IDocumentService>();

            // Create controller instance with mockDocumentService.Object
            controller = new DocumentController(mockDocumentService.Object);
        }

        [Fact]
        public async Task CreateDocument_ValidRequest_ReturnsCreatedResult()
        {
            // Arrange: Set up mock for CreateDocument
            var newDocument = CreateSampleDocument("1");
            mockDocumentService.Setup(s => s.CreateDocument(It.IsAny<DocumentDto>()))
                .ReturnsAsync(newDocument);

            // Act: Call controller.CreateDocument with valid request
            var result = await controller.CreateDocument(new DocumentDto());

            // Assert: Verify result is CreatedAtActionResult
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);

            // Assert: Verify returned document matches expected
            var returnedDocument = Assert.IsType<DocumentDto>(createdResult.Value);
            Assert.Equal(newDocument.Id, returnedDocument.Id);
        }

        [Fact]
        public async Task GetDocument_ExistingId_ReturnsOkResult()
        {
            // Arrange: Set up mock for GetDocument
            var existingDocument = CreateSampleDocument("1");
            mockDocumentService.Setup(s => s.GetDocument("1"))
                .ReturnsAsync(existingDocument);

            // Act: Call controller.GetDocument with existing ID
            var result = await controller.GetDocument("1");

            // Assert: Verify result is OkObjectResult
            var okResult = Assert.IsType<OkObjectResult>(result);

            // Assert: Verify returned document matches expected
            var returnedDocument = Assert.IsType<DocumentDto>(okResult.Value);
            Assert.Equal(existingDocument.Id, returnedDocument.Id);
        }

        [Fact]
        public async Task GetDocument_NonExistingId_ReturnsNotFound()
        {
            // Arrange: Set up mock for GetDocument to return null
            mockDocumentService.Setup(s => s.GetDocument("999"))
                .ReturnsAsync((DocumentDto)null);

            // Act: Call controller.GetDocument with non-existing ID
            var result = await controller.GetDocument("999");

            // Assert: Verify result is NotFoundResult
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task UpdateDocument_ValidRequest_ReturnsNoContent()
        {
            // Arrange: Set up mock for UpdateDocument
            mockDocumentService.Setup(s => s.UpdateDocument(It.IsAny<DocumentDto>()))
                .Returns(Task.CompletedTask);

            // Act: Call controller.UpdateDocument with valid request
            var result = await controller.UpdateDocument(new DocumentDto());

            // Assert: Verify result is NoContentResult
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteDocument_ExistingId_ReturnsNoContent()
        {
            // Arrange: Set up mock for DeleteDocument
            mockDocumentService.Setup(s => s.DeleteDocument("1"))
                .Returns(Task.CompletedTask);

            // Act: Call controller.DeleteDocument with existing ID
            var result = await controller.DeleteDocument("1");

            // Assert: Verify result is NoContentResult
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task GetDocumentCollaborators_ExistingId_ReturnsOkResult()
        {
            // Arrange: Set up mock for GetDocumentCollaborators
            var collaborators = new[] { CreateSampleCollaborator("1"), CreateSampleCollaborator("2") };
            mockDocumentService.Setup(s => s.GetDocumentCollaborators("1"))
                .ReturnsAsync(collaborators);

            // Act: Call controller.GetDocumentCollaborators with existing ID
            var result = await controller.GetDocumentCollaborators("1");

            // Assert: Verify result is OkObjectResult
            var okResult = Assert.IsType<OkObjectResult>(result);

            // Assert: Verify returned collaborators match expected
            var returnedCollaborators = Assert.IsType<CollaboratorDto[]>(okResult.Value);
            Assert.Equal(collaborators.Length, returnedCollaborators.Length);
        }

        [Fact]
        public async Task AddCollaborator_ValidRequest_ReturnsOkResult()
        {
            // Arrange: Set up mock for AddCollaborator
            mockDocumentService.Setup(s => s.AddCollaborator("1", It.IsAny<CollaboratorDto>()))
                .Returns(Task.CompletedTask);

            // Act: Call controller.AddCollaborator with valid request
            var result = await controller.AddCollaborator("1", new CollaboratorDto());

            // Assert: Verify result is OkResult
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task RemoveCollaborator_ValidRequest_ReturnsNoContent()
        {
            // Arrange: Set up mock for RemoveCollaborator
            mockDocumentService.Setup(s => s.RemoveCollaborator("1", "collaboratorId"))
                .Returns(Task.CompletedTask);

            // Act: Call controller.RemoveCollaborator with valid request
            var result = await controller.RemoveCollaborator("1", "collaboratorId");

            // Assert: Verify result is NoContentResult
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task GetDocumentVersions_ExistingId_ReturnsOkResult()
        {
            // Arrange: Set up mock for GetDocumentVersions
            var versions = new[] { new DocumentVersionDto(), new DocumentVersionDto() };
            mockDocumentService.Setup(s => s.GetDocumentVersions("1"))
                .ReturnsAsync(versions);

            // Act: Call controller.GetDocumentVersions with existing ID
            var result = await controller.GetDocumentVersions("1");

            // Assert: Verify result is OkObjectResult
            var okResult = Assert.IsType<OkObjectResult>(result);

            // Assert: Verify returned versions match expected
            var returnedVersions = Assert.IsType<DocumentVersionDto[]>(okResult.Value);
            Assert.Equal(versions.Length, returnedVersions.Length);
        }

        [Fact]
        public async Task RevertToVersion_ValidRequest_ReturnsOkResult()
        {
            // Arrange: Set up mock for RevertToVersion
            var revertedDocument = CreateSampleDocument("1");
            mockDocumentService.Setup(s => s.RevertToVersion("1", "versionId"))
                .ReturnsAsync(revertedDocument);

            // Act: Call controller.RevertToVersion with valid request
            var result = await controller.RevertToVersion("1", "versionId");

            // Assert: Verify result is OkObjectResult
            var okResult = Assert.IsType<OkObjectResult>(result);

            // Assert: Verify returned document matches expected
            var returnedDocument = Assert.IsType<DocumentDto>(okResult.Value);
            Assert.Equal(revertedDocument.Id, returnedDocument.Id);
        }

        private DocumentDto CreateSampleDocument(string id)
        {
            return new DocumentDto { Id = id, Title = "Sample Document" };
        }

        private CollaboratorDto CreateSampleCollaborator(string id)
        {
            return new CollaboratorDto { Id = id, Name = "Sample Collaborator" };
        }
    }
}