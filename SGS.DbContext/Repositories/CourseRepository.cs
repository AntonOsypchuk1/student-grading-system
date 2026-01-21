using Microsoft.EntityFrameworkCore;
using SGS.DbContext.Data;
using SGS.DbContext.Repositories.BaseRepository;
using SGS.DbContext.Repositories.Interfaces;
using SGS.Domain.Entities;

namespace SGS.DbContext.Repositories;

public class CourseRepository(AppDbContext context)
    : BaseRepository<Course>(context), ICourseRepository
{
    public override async Task<IEnumerable<Course>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await Context.Courses
            .Include(c => c.Teachers)
            .ThenInclude(t => t.User)
            .Include(c => c.StudentsGroups)
            .Include(c => c.Assignments)
            .ToListAsync(cancellationToken);
    }
    
    public override async Task<Course?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Context.Courses
            .Include(c => c.Teachers)
            .ThenInclude(t => t.User)
            .Include(c => c.StudentsGroups)
            .Include(c => c.Assignments)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Course>> GetByTeacherIdAsync(Guid teacherId,
        CancellationToken cancellationToken = default)
    {
        return await Context.Courses
            .Where(c => c.Teachers.Any(t => t.Id == teacherId || t.UserId == teacherId))
            .Include(c => c.Teachers)
            .ThenInclude(t => t.User)
            .Include(c => c.StudentsGroups)
            .Include(c => c.Assignments)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Course>> GetByStudentIdAsync(Guid studentId,
        CancellationToken cancellationToken = default)
    {
        return await Context.Courses
            .Where(c => c.StudentsGroups.Any(g => 
                g.Students.Any(s => s.UserId == studentId || s.Id == studentId)))
            .Include(c => c.Teachers)
            .ThenInclude(t => t.User)
            .Include(c => c.Assignments)
            .ToListAsync(cancellationToken);
    }

    public async Task AddTeacherToCourseAsync(Guid courseId, Guid teacherId, CancellationToken cancellationToken = default)
    {
        var course = await GetByIdAsync(courseId, cancellationToken);
        var teacher = await Context.Teachers
            .FirstOrDefaultAsync(t => t.Id == teacherId || t.UserId == teacherId, cancellationToken);
        
        if (course != null && teacher != null)
        {
            course.Teachers.Add(teacher);
            Context.Courses.Update(course);
        }
    }

    public async Task AddStudentsGroupToCourseAsync(Guid courseId, Guid groupId, CancellationToken cancellationToken = default)
    {
        var course = await GetByIdAsync(courseId, cancellationToken);
        var group = await Context.StudentGroups.
            FirstOrDefaultAsync(g => g.Id == groupId, cancellationToken);
        
        if (course != null && group != null)
        {
            course.StudentsGroups.Add(group);
            Context.Courses.Update(course);
        }
    }
    
    public async Task AddCourseAsync(Course course, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(course.Title) || string.IsNullOrEmpty(course.Description))
            throw new ArgumentException("Course must have a title and description");
            
        await CreateAsync(course, cancellationToken);
    }
}