using Microsoft.AspNetCore.Mvc;
using UniversityTasksDbFirstApi.DTOs;
using UniversityTasksDbFirstApi.Services;

namespace UniversityTasksDbFirstApi.Controllers;

[ApiController]
[Route("api/submissions")]
public class SubmissionsController : ControllerBase
{
    private readonly ISubmissionService _service;

    public SubmissionsController(
        ISubmissionService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> CreateSubmission(
        CreateSubmissionDto dto)
    {
        var id =
            await _service.CreateSubmission(
                dto);

        return Created(
            $"api/submissions/{id}",
            new { SubmissionId = id });
    }

    [HttpPut("{idSubmission}/grade")]
    public async Task<IActionResult> GradeSubmission(
        int idSubmission,
        GradeSubmissionDto dto)
    {
        await _service.GradeSubmission(
            idSubmission,
            dto);

        return NoContent();
    }

    [HttpDelete("{idSubmission}")]
    public async Task<IActionResult> DeleteSubmission(
        int idSubmission)
    {
        await _service.DeleteSubmission(
            idSubmission);

        return NoContent();
    }
}