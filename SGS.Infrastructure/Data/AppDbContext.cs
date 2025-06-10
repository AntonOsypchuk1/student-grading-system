using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SGS.Domain.Entities;

namespace SGS.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<Student> Students { get; set; }
    public DbSet<Teacher> Teachers { get; set; }
    public DbSet<StudentsGroup> StudentGroups { get; set; }
    public DbSet<Course> Courses { get; set; }
    public DbSet<Assignment> Assignments { get; set; }
    public DbSet<Document> Documents { get; set; }
    public DbSet<Grade> Grades { get; set; }
    public DbSet<GradingCriteria> GradingCriteria { get; set; }
    public DbSet<AnalysisResults> AnalysisResults { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Role)
                .HasColumnName("Role")
                .HasConversion<int>()
                .IsRequired();
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.HasOne<StudentsGroup>(s => s.Group)
                .WithMany(g => g.Students)
                .HasForeignKey(s => s.GroupId)
                .IsRequired(false);
        });

        modelBuilder.Entity<Teacher>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.HasMany(t => t.Courses)
                .WithMany(c => c.Teachers)
                .UsingEntity(j => j.ToTable("CourseTeacher"));
        });
        
        modelBuilder.Entity<StudentsGroup>(entity =>
        {
            entity.HasKey(sg => sg.Id);
            entity.HasMany(g => g.Courses)
                .WithMany(c => c.StudentsGroups)
                .UsingEntity(j => j.ToTable("GroupCourse"));
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.HasMany(c => c.StudentsGroups)
                .WithMany(g => g.Courses)
                .UsingEntity(j => j.ToTable("GroupCourse"));
            entity.HasMany(c => c.Teachers)
                .WithMany(t => t.Courses)
                .UsingEntity(j => j.ToTable("CourseTeacher"));
        });

        modelBuilder.Entity<Assignment>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.HasOne<Course>(a => a.Course)
                .WithMany(c => c.Assignments)
                .HasForeignKey(a => a.CourseId);
        });

        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.HasOne<User>(d => d.User)
                .WithMany()
                .HasForeignKey(d => d.UserId);
            entity.HasOne<Assignment>(d => d.Assignment)
                .WithMany(a => a.Documents)
                .HasForeignKey(d => d.AssignmentId);
        });

        modelBuilder.Entity<Grade>(entity =>
        {
            entity.HasKey(g => g.Id);
        });

        modelBuilder.Entity<GradingCriteria>(entity =>
        {
            entity.HasKey(gc => gc.Id);
            entity.HasOne<Assignment>(gc => gc.Assignment)
                .WithMany(a => a.GradingCriteria)
                .HasForeignKey(gc => gc.AssignmentId);
        });

        modelBuilder.Entity<AnalysisResults>(entity =>
        {
            entity.HasKey(ar => ar.Id);
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(rt => rt.Id);
        });
    }
}