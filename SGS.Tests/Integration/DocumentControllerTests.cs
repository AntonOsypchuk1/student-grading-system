using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using SGS.Application.DTOs;

namespace SGS.Tests.Integration;

public class DocumentControllerTests(WebApplicationFactory<Program> webApplicationFactory)
    : BaseWebApplication
{
    private readonly HttpClient _client = webApplicationFactory.CreateClient();

    [Fact]
    public async Task UploadDocument_ReturnsDocumentDto()
    {
        var multipartContent = new MultipartFormDataContent();

        var fileBytes = "Test file content"u8.ToArray();
        var fileContent = new ByteArrayContent(fileBytes);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/pdf");

        multipartContent.Add(fileContent, "File", "test.pdf");

        multipartContent.Add(new StringContent(Guid.NewGuid().ToString()), "UserId");
        multipartContent.Add(new StringContent(Guid.NewGuid().ToString()), "AssignmentId");

        var response = await _client.PostAsync("/api/documents/upload", multipartContent);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var documentDto = await response.Content.ReadFromJsonAsync<DocumentDto>();
        documentDto.Should().NotBeNull();
        documentDto.Id.Should().NotBeEmpty();
        documentDto.FilePath.Should().NotBeNullOrEmpty();
        documentDto.UploadedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(10));
    }
}