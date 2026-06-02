using Microsoft.AspNetCore.Mvc;
using UniversityTasksDbFirstApi.Services;

namespace UniversityTasksDbFirstApi.Controllers;

[ApiController]
[Route("api/courses")]
public class CoursesController : ControllerBase
{
    private readonly ISubmissionService _service;

    public CoursesController(ISubmissionService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetCourses(
        [FromQuery] bool activeOnly = false)
    {
        var result =
            await _service.GetCourses(activeOnly);

        return Ok(result);
    }

    [HttpGet("{idCourse}/assignments")]
    public async Task<IActionResult> GetAssignments(
        int idCourse,
        [FromQuery] bool publishedOnly = false)
    {
        var result =
            await _service.GetAssignments(
                idCourse,
                publishedOnly);

        return Ok(result);
    }
}