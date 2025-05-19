using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using SGS.Application.DTOs;
using SGS.IntegrationTests.Controllers.Bases;
using SGS.IntegrationTests.TestData;

namespace SGS.IntegrationTests.Controllers;

public class DocumentControllerTests(CustomWebApplicationFactory<Program> factory) : BaseControllerTests(factory)
{
    private const string BaseUrl = "/api/documents";

    [Fact]
    public async Task UploadDocument_ReturnsDocumentDto()
    {
        await AuthenticateAsTeacher();
        // Arrange
        var multipartContent = new MultipartFormDataContent();

        var fileBytes = "Test file content"u8.ToArray();
        var fileContent = new ByteArrayContent(fileBytes);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/pdf");

        multipartContent.Add(fileContent, "File", "test.pdf");
        multipartContent.Add(new StringContent(TestDataSeeder.GuidStudent.ToString()), "UserId");
        multipartContent.Add(new StringContent(TestDataSeeder.GuidAssignment.ToString()), "AssignmentId");

        // Act
        var response = await Client.PostAsync($"{BaseUrl}/upload", multipartContent);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var documentDto = await response.Content.ReadFromJsonAsync<DocumentDto>();
        Assert.NotNull(documentDto);
        Assert.NotEqual(Guid.Empty, documentDto!.Id);
        Assert.NotEmpty(documentDto.FilePath);
        
        // Check if uploaded time is within a reasonable window
        var now = DateTime.UtcNow;
        var timeDifference = Math.Abs((now - documentDto.UploadedAt).TotalSeconds);
        Assert.True(timeDifference < 10, "Document upload timestamp should be close to current time");
    }
    
    [Fact]
    public async Task GetDocumentById_WithValidId_ReturnsDocument()
    {
        // Act
        var response = await Client.GetAsync($"{BaseUrl}/{TestDataSeeder.GuidDocument}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var document = await response.Content.ReadFromJsonAsync<DocumentDto>();
        Assert.NotNull(document);
        Assert.Equal(TestDataSeeder.GuidDocument, document!.Id);
        Assert.Equal(TestDataSeeder.Document.FileName, document.FileName);
    }
    
    [Fact]
    public async Task GetDocumentsByAssignment_ReturnsDocuments()
    {
        // Act
        var response = await Client.GetAsync($"{BaseUrl}/assignment/{TestDataSeeder.GuidAssignment}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var documents = await response.Content.ReadFromJsonAsync<IEnumerable<DocumentDto>>();
        Assert.NotNull(documents);
        Assert.Contains(documents!, d => d.Id == TestDataSeeder.GuidDocument);
    }
    
    [Fact]
    public async Task GetDocumentsByUser_ReturnsDocuments()
    {
        // Act
        var response = await Client.GetAsync($"{BaseUrl}/user/{TestDataSeeder.GuidStudent}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var documents = await response.Content.ReadFromJsonAsync<IEnumerable<DocumentDto>>();
        Assert.NotNull(documents);
        Assert.Contains(documents!, d => d.Id == TestDataSeeder.GuidDocument);
    }
}