using Microsoft.EntityFrameworkCore;
using UniversityTasksDbFirstApi.Data;
using UniversityTasksDbFirstApi.DTOs;
using UniversityTasksDbFirstApi.Models;

namespace UniversityTasksDbFirstApi.Services;

public class SubmissionService : ISubmissionService
{
    private readonly UniversityTasksDbContext _context;

    public SubmissionService(UniversityTasksDbContext context)
    {
        _context = context;
    }

    public async Task<List<CourseDto>> GetCourses(bool activeOnly)
    {
        var query = _context.Courses.AsNoTracking();

        if (activeOnly)
        {
            query = query.Where(c => c.IsActive);
        }

        return await query
            .Select(c => new CourseDto
            {
                CourseId = c.CourseId,
                Code = c.Code,
                Name = c.Name,
                Credits = c.Credits,
                AssignmentCount = c.Assignments.Count
            })
            .ToListAsync();
    }

    public async Task<List<AssignmentDto>> GetAssignments(
        int courseId,
        bool publishedOnly)
    {
        var courseExists = await _context.Courses
            .AnyAsync(c => c.CourseId == courseId);

        if (!courseExists)
            throw new Exception("Course not found");

        var query = _context.Assignments
            .AsNoTracking()
            .Where(a => a.CourseId == courseId);

        if (publishedOnly)
        {
            query = query.Where(a => a.IsPublished);
        }

        return await query
            .Select(a => new AssignmentDto
            {
                AssignmentId = a.AssignmentId,
                Title = a.Title,
                DueDate = a.DueDate,
                MaxPoints = a.MaxPoints,
                IsPublished = a.IsPublished,
                SubmissionCount = a.Submissions.Count
            })
            .ToListAsync();
    }

    public async Task<StudentDashboardDto> GetDashboard(int studentId)
    {
        var student = await _context.Students
            .Include(s => s.Enrollments)
                .ThenInclude(e => e.Course)
            .Include(s => s.Submissions)
                .ThenInclude(s => s.Assignment)
            .FirstOrDefaultAsync(s => s.StudentId == studentId);

        if (student == null)
            throw new Exception("Student not found");

        return new StudentDashboardDto
        {
            StudentId = student.StudentId,
            IndexNumber = student.IndexNumber,
            FullName = student.FullName,
            IsActive = student.IsActive,

            Enrollments = student.Enrollments
                .Select(e =>
                    $"{e.Course.Code} - {e.Course.Name} ({e.Status})")
                .ToList(),

            Submissions = student.Submissions
                .Select(s => new SubmissionDto
                {
                    SubmissionId = s.SubmissionId,
                    AssignmentTitle = s.Assignment.Title,
                    RepositoryUrl = s.RepositoryUrl,
                    SubmittedAt = s.SubmittedAt,
                    Status = s.Status,
                    Score = s.Score,
                    Feedback = s.Feedback
                })
                .ToList()
        };
    }

    public async Task<int> CreateSubmission(CreateSubmissionDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.RepositoryUrl))
            throw new Exception("RepositoryUrl is required");

        if (!dto.RepositoryUrl.StartsWith("https://"))
            throw new Exception("RepositoryUrl must start with https://");

        var student = await _context.Students
            .FirstOrDefaultAsync(s => s.StudentId == dto.StudentId);

        if (student == null)
            throw new Exception("Student not found");

        if (!student.IsActive)
            throw new Exception("Student is inactive");

        var assignment = await _context.Assignments
            .FirstOrDefaultAsync(a => a.AssignmentId == dto.AssignmentId);

        if (assignment == null)
            throw new Exception("Assignment not found");

        if (!assignment.IsPublished)
            throw new Exception("Assignment is not published");

        var enrollmentExists = await _context.Enrollments
            .AnyAsync(e =>
                e.StudentId == dto.StudentId &&
                e.CourseId == assignment.CourseId &&
                (e.Status == "Active" ||
                 e.Status == "Completed"));

        if (!enrollmentExists)
            throw new Exception("Student is not enrolled");

        var alreadySubmitted = await _context.Submissions
            .AnyAsync(s =>
                s.StudentId == dto.StudentId &&
                s.AssignmentId == dto.AssignmentId);

        if (alreadySubmitted)
            throw new Exception("Submission already exists");

        var submission = new Submission
        {
            AssignmentId = dto.AssignmentId,
            StudentId = dto.StudentId,
            RepositoryUrl = dto.RepositoryUrl,
            SubmittedAt = DateTime.Now,
            Status = assignment.DueDate < DateTime.Now
                ? "Late"
                : "Submitted"
        };

        _context.Submissions.Add(submission);

        await _context.SaveChangesAsync();

        return submission.SubmissionId;
    }

    public async Task GradeSubmission(
        int submissionId,
        GradeSubmissionDto dto)
    {
        var submission = await _context.Submissions
            .Include(s => s.Assignment)
            .FirstOrDefaultAsync(
                s => s.SubmissionId == submissionId);

        if (submission == null)
            throw new Exception("Submission not found");

        if (dto.Score < 0)
            throw new Exception("Score cannot be negative");

        if (dto.Score > submission.Assignment.MaxPoints)
            throw new Exception("Score exceeds MaxPoints");

        submission.Score = dto.Score;
        submission.Feedback = dto.Feedback;
        submission.Status = "Graded";

        await _context.SaveChangesAsync();
    }

    public async Task DeleteSubmission(int submissionId)
    {
        var submission = await _context.Submissions
            .FirstOrDefaultAsync(
                s => s.SubmissionId == submissionId);

        if (submission == null)
            throw new Exception("Submission not found");

        if (submission.Status == "Graded")
            throw new Exception(
                "Graded submission cannot be deleted");

        _context.Submissions.Remove(submission);

        await _context.SaveChangesAsync();
    }
}

