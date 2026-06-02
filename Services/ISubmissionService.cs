using UniversityTasksDbFirstApi.DTOs;

namespace UniversityTasksDbFirstApi.Services;

public interface ISubmissionService
{
    Task<List<CourseDto>> GetCourses(
        bool activeOnly);

    Task<List<AssignmentDto>> GetAssignments(
        int courseId,
        bool publishedOnly);

    Task<StudentDashboardDto> GetDashboard(
        int studentId);

    Task<int> CreateSubmission(
        CreateSubmissionDto dto);

    Task GradeSubmission(
        int submissionId,
        GradeSubmissionDto dto);

    Task DeleteSubmission(
        int submissionId);
}