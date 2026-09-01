
using System.Net;
using System.Net.Http.Json;
using TmsApi.Tests;

namespace Tms.Tests;

public class CoursesApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CoursesApiTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetCourses_ReturnsOkAndPagedJson()
    {
        // Act
        var response = await _client.GetAsync(
            "/api/v2.0/courses?page=1&pageSize=10");

        // Assert
        response.EnsureSuccessStatusCode();

        var page = await response.Content
            .ReadFromJsonAsync<PagedCoursesJson>();

        Assert.NotNull(page);
        Assert.NotNull(page.Data);
    }

    [Fact]
    public async Task CreateCourse_InvalidCode_ReturnsValidationError()
    {
        // Act
        var response = await _client.PostAsJsonAsync(
            "/api/v2.0/courses",
            new
            {
                code = "",
                title = "Intro to TMS Security",
                maxCapacity = 30
            });

        // Assert
        Assert.True(
            response.StatusCode is
                HttpStatusCode.BadRequest or
                HttpStatusCode.UnprocessableEntity);
    }

    private sealed class PagedCoursesJson
    {
        public List<CourseRowJson> Data { get; set; } = default!;
        public MetaJson Meta { get; set; } = default!;
    }

    private sealed class MetaJson
    {
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasNext { get; set; }
        public bool HasPrevious { get; set; }
    }

    private sealed class CourseRowJson
    {
        public int Id { get; set; }
        public string Code { get; set; } = "";
        public string Title { get; set; } = "";
        public int MaxCapacity { get; set; }
        public int EnrollmentCount { get; set; }
    }
}

