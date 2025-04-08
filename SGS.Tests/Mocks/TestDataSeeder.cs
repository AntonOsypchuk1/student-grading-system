using SGS.Domain.Entities;
using SGS.Domain.Enums;
using System;
using SGS.Application.Helpers;
using SGS.Infrastructure.Data;

namespace SGS.Tests.Mocks;

public static class TestDataSeeder
{
    public static void Seed(AppDbContext context)
    {
        // Avoid reseeding if data exists
        if (context.Users.Any()) return;

        // Create teacher and student
        var teacher = new User
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Login = "teacher1",
            Email = "teacher1@example.com",
            FullName = "Teacher One",
            PasswordHash = PasswordHelper.HashPassword("Password123!"),
            Role = UserRole.Teacher
        };
        var student = new User
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
            Login = "student1",
            Email = "student1@example.com",
            FullName = "Student One",
            PasswordHash = PasswordHelper.HashPassword("Password123!"),
            Role = UserRole.Student
        };

        context.Users.AddRange(teacher, student);
        context.SaveChanges();

        // Insert group
        var group = new StudentsGroup { Id = Guid.NewGuid(), Name = "Group A" };
        context.StudentGroups.Add(group);
        context.SaveChanges();

        // Insert teacher and student profiles if separate tables exist
        context.Teachers.Add(new Teacher { UserId = teacher.Id });
        context.Students.Add(new Student { UserId = student.Id, GroupId = group.Id });
        context.SaveChanges();

        // Create courses
        var course1 = new Course { Id = Guid.NewGuid(), Title = "Mathematics 101", Description = "Basic Mathematics" };
        var course2 = new Course { Id = Guid.NewGuid(), Title = "Physics 101", Description = "Introduction to Physics" };
        context.Courses.AddRange(course1, course2);
        context.SaveChanges();

        // Setup GroupCourse and CourseTeacher relationships using navigation properties
        group.Courses.Add(course1);
        group.Courses.Add(course2);
        // TODO: Add logic to ensure that the teacher and courses are linked correctly in the database.
        // teacher.Courses.Add(course1);
        // teacher.Courses.Add(course2);
        context.SaveChanges();

        // Create an assignment for group under course1 (Mathematics 101)
        var assignment = new Assignment
        {
            Id = Guid.NewGuid(),
            GroupId = group.Id,
            CourseId = course1.Id,
            Title = "Algebra Test",
            Description = "Solve Algebra problems",
            MaxAttempts = 3,
            DueDate = DateTime.UtcNow.AddDays(30)
        };
        context.Assignments.Add(assignment);
        context.SaveChanges();

        // Create a document (simulate a file upload record)
        var document = new Document
        {
            Id = Guid.NewGuid(),
            UserId = student.Id,
            AssignmentId = assignment.Id,
            AttemptNumber = 1,
            FilePath = "/uploads/algebra_test_student1.pdf",
            UploadedAt = DateTime.UtcNow
        };
        context.Documents.Add(document);
        context.SaveChanges();

        // Create grading criteria by teacher
        var criteria = new GradingCriteria
        {
            Id = Guid.NewGuid(),
            UserId = teacher.Id,
            CriteriaDescription = "Correctness of Solution",
            CreatedAt = DateTime.UtcNow
        };
        context.GradingCriteria.Add(criteria);
        context.SaveChanges();
    }
}
