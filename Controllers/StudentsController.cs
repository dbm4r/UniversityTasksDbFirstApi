using Microsoft.AspNetCore.Mvc;
using UniversityTasksDbFirstApi.Services;

namespace UniversityTasksDbFirstApi.Controllers;

[ApiController]
[Route("api/students")]
public class StudentsController : ControllerBase
{
    private readonly ISubmissionService _service;

    public StudentsController(
        ISubmissionService service)
    {
        _service = service;
    }

    [HttpGet("{idStudent}/dashboard")]
    public async Task<IActionResult> GetDashboard(
        int idStudent)
    {
        var result =
            await _service.GetDashboard(
                idStudent);

        return Ok(result);
    }
}