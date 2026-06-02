namespace UniversityTasksDbFirstApi.Models;

public partial class Assignment
{
    public bool IsOverdue(DateTime now)
    {
        return DueDate < now;
    }
}