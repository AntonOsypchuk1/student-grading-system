using Microsoft.EntityFrameworkCore;
using SGS.Domain.Entities;

namespace SGS.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Student> Students { get; set; }
    public DbSet<Teacher> Teachers { get; set; }
    public DbSet<StudentsGroup> StudentGroups { get; set; }
    public DbSet<Course> Courses { get; set; }
    public DbSet<Assignment> Assignments { get; set; }
    public DbSet<Document> Documents { get; set; }
    public DbSet<Grade> Grades { get; set; }
    public DbSet<GradingCriteria> GradingCriteria { get; set; }
    public DbSet<AnalysisResults> AnalysisResults { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasKey(u => u.Id);
        modelBuilder.Entity<Role>().HasKey(r => r.Id);
        modelBuilder.Entity<Student>().HasKey(s => s.Id);
        modelBuilder.Entity<Teacher>().HasKey(t => t.Id);
        modelBuilder.Entity<StudentsGroup>().HasKey(sg => sg.Id);
        modelBuilder.Entity<Course>().HasKey(c => c.Id);
        modelBuilder.Entity<Assignment>().HasKey(a => a.Id);
        modelBuilder.Entity<Document>().HasKey(d => d.Id);
        modelBuilder.Entity<Grade>().HasKey(g => g.Id);
        modelBuilder.Entity<GradingCriteria>().HasKey(gc => gc.Id);
        modelBuilder.Entity<AnalysisResults>().HasKey(ar => ar.Id);

        modelBuilder.Entity<User>()
            .HasOne<Role>(u => u.Role)
            .WithMany(r => r.Users)
            .HasForeignKey(u => u.RoleId);

        modelBuilder.Entity<Student>()
            .HasOne<StudentsGroup>(s => s.Group)
            .WithMany(g => g.Students)
            .HasForeignKey(s => s.GroupId);
        
        modelBuilder.Entity<Document>()
            .HasOne<User>(d => d.User)
            .WithMany()
            .HasForeignKey(d => d.UserId);

        modelBuilder.Entity<Document>()
            .HasOne<Assignment>(d => d.Assignment)
            .WithMany(a => a.Documents)
            .HasForeignKey(d => d.AssignmentId);
        
        // Many-to-many relationships
        modelBuilder.Entity<Teacher>()
            .HasMany(t => t.StudentsGroups)
            .WithMany(g => g.Teachers)
            .UsingEntity(j => j.ToTable("TeacherGroup"));

        modelBuilder.Entity<Teacher>()
            .HasMany(t => t.Courses)
            .WithMany(c => c.Teachers)
            .UsingEntity(j => j.ToTable("CourseTeacher"));
        
        modelBuilder.Entity<StudentsGroup>()
            .HasMany(g => g.Courses)
            .WithMany(c => c.StudentsGroups)
            .UsingEntity(j => j.ToTable("GroupCourse"));
    }
}