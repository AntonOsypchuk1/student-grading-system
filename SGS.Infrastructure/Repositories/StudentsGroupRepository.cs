using Microsoft.EntityFrameworkCore;
using SGS.Domain.Entities;
using SGS.Infrastructure.Data;
using SGS.Infrastructure.Repositories.BaseRepository;
using SGS.Infrastructure.Repositories.Interfaces;

namespace SGS.Infrastructure.Repositories;

public class StudentsGroupRepository(AppDbContext context)
    : BaseRepository<StudentsGroup>(context), IStudentsGroupRepository
{
    public override async Task<IEnumerable<StudentsGroup>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await Context.StudentGroups
            .Include(sg => sg.Students)
            .ThenInclude(s => s.User)
            .Include(sg => sg.Courses)
            .ToListAsync(cancellationToken);
    }
    
    public override async Task<StudentsGroup?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Context.StudentGroups
            .Include(sg => sg.Students)
            .ThenInclude(s => s.User)
            .Include(sg => sg.Courses)
            .FirstOrDefaultAsync(sg => sg.Id == id, cancellationToken);
    }
    
    public async Task<IEnumerable<StudentsGroup>> GetByCourseIdAsync(Guid courseId, CancellationToken cancellationToken = default)
    {
        return await Context.StudentGroups
            .Where(sg => sg.Courses.Any(c => c.Id == courseId))
            .Include(sg => sg.Students)
            .ThenInclude(s => s.User)
            .Include(sg => sg.Courses)
            .ToListAsync(cancellationToken);
    }

    public async Task<StudentsGroup?> GetByStudentIdAsync(Guid studentId, CancellationToken cancellationToken = default)
    {
        return await Context.StudentGroups
            .Include(g => g.Students)
            .Include(sg => sg.Courses)
            .FirstOrDefaultAsync(g => g.Students
                .Any(s => s.UserId == studentId || s.Id == studentId), cancellationToken);
    }

    public async Task<StudentsGroup?> GetByGroupByNameAsync(string name,
        CancellationToken cancellationToken = default)
    {
        return await Context.StudentGroups
            .Include(sg => sg.Students)
            .Include(sg => sg.Courses)
            .FirstOrDefaultAsync(g => g.Name == name, cancellationToken);
    }
    
    public async Task AddStudentToGroupAsync(Guid groupId, Guid studentId, CancellationToken cancellationToken = default)
    {
        var group = await GetByIdAsync(groupId, cancellationToken);
        var student = await Context.Students
            .FirstOrDefaultAsync(s => s.Id == studentId || s.UserId == studentId, cancellationToken);
        
        if (group != null && student != null)
        {
            student.GroupId = groupId;
            student.Group = group;
            Context.Students.Update(student);
        }
    }
    
    public async Task AddGroupAsync(StudentsGroup group, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(group.Name))
            throw new ArgumentException("Group must have a name");
            
        await CreateAsync(group, cancellationToken);
    }
    
    public async Task RemoveStudentFromGroupAsync(Guid studentId, Guid groupId, CancellationToken cancellationToken = default)
    {
        var student = await Context.Students.FindAsync([studentId], cancellationToken);
    
        if (student != null && student.GroupId == groupId)
        {
            student.GroupId = null;
            student.Group = null;
            Context.Students.Update(student);
        }
    }
}