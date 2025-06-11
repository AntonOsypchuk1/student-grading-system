using SGS.Domain.Entities;
using SGS.Domain.Enums;
using SGS.Infrastructure.Data;

namespace SGS.IntegrationTests.TestData;

public static class TestDataSeeder
{
    // Static Guids for test entities
    public static readonly Guid GuidTeacher = Guid.Parse("00000000-0000-0000-0000-000000000001");
    public static readonly Guid GuidStudent = Guid.Parse("00000000-0000-0000-0000-000000000002");
    public static readonly Guid GuidGroup = Guid.Parse("00000000-0000-0000-0000-000000000003");
    public static readonly Guid GuidCourse1 = Guid.Parse("00000000-0000-0000-0000-000000000004");
    public static readonly Guid GuidCourse2 = Guid.Parse("00000000-0000-0000-0000-000000000005");
    public static readonly Guid GuidAssignment = Guid.Parse("00000000-0000-0000-0000-000000000006");
    public static readonly Guid GuidDocument = Guid.Parse("00000000-0000-0000-0000-000000000007");
    public static readonly Guid GuidCriteria = Guid.Parse("00000000-0000-0000-0000-000000000008");
    
    // Static properties for entity references
    public static User TeacherUser { get; private set; }
    public static User Student { get; private set; }
    public static StudentsGroup Group { get; private set; }
    public static Course Course1 { get; private set; }
    public static Course Course2 { get; private set; }
    public static Assignment Assignment { get; private set; }
    public static Document Document { get; private set; }
    public static GradingCriteria Criteria { get; private set; }
    
    // Track if the seeder has been initialized
    private static bool _isInitialized;
    private static readonly object _lockObj = new();

    /// <summary>
    /// Seeds the database with test data and initializes static entity references
    /// </summary>
    public static void Seed(AppDbContext context)
    {
        lock (_lockObj)
        {
            if (_isInitialized) return;
            
            // Only seed if no users exist
            bool needSeeding = !context.Users.Any();
            
            if (needSeeding)
            {
                SeedDatabase(context);
            }
            
            // Load entity references regardless if we just seeded
            LoadEntityReferences(context);
            
            _isInitialized = true;
        }
    }
    
    private static void SeedDatabase(AppDbContext context)
    {
        // Create teacher and student
        TeacherUser = new User
        {
            Id = GuidTeacher,
            Login = "teacher1",
            Email = "teacher1@example.com",
            FullName = "Teacher One",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
            Role = UserRole.Teacher
        };
        var teacher = new Teacher { Id = GuidTeacher, UserId = TeacherUser.Id, Courses = [] };
        
        Student = new User
        {
            Id = GuidStudent,
            Login = "student1",
            Email = "student1@example.com",
            FullName = "Student One",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
            Role = UserRole.Student
        };
        var student = new Student { Id = GuidStudent, UserId = Student.Id, GroupId = GuidGroup };

        context.Users.AddRange(TeacherUser, Student);
        context.Teachers.Add(teacher);
        context.Students.Add(student);
        context.SaveChanges();

        // Insert group
        Group = new StudentsGroup
        {
            Id = GuidGroup, 
            Name = "Group A",
            Description = "Group A Description",
            Year = 2023,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Students = [student]
        };
        context.StudentGroups.Add(Group);
        context.SaveChanges();

        // Insert teacher and student profiles if separate tables exist
        context.Teachers.Add(new Teacher { UserId = TeacherUser.Id });
        context.Students.Add(new Student { UserId = Student.Id, GroupId = Group.Id });
        context.SaveChanges();

        // Create courses
        Course1 = new Course { Id = GuidCourse1, Title = "Mathematics 101", Description = "Basic Mathematics" };
        Course2 = new Course { Id = GuidCourse2, Title = "Physics 101", Description = "Introduction to Physics", Teachers = [teacher]};
        context.Courses.AddRange(Course1, Course2);
        context.SaveChanges();

        // Setup GroupCourse and CourseTeacher relationships using navigation properties
        Group.Courses.Add(Course1);
        Group.Courses.Add(Course2);
        // TODO: Add logic to ensure that the teacher and courses are linked correctly in the database.
        context.SaveChanges();

        // Create an assignment for group under course1 (Mathematics 101)
        Assignment = new Assignment
        {
            Id = GuidAssignment,
            GroupId = Group.Id,
            CourseId = Course1.Id,
            Title = "Algebra Test",
            Description = "Solve Algebra problems",
            MaxAttempts = 3,
            DueDate = DateTime.UtcNow.AddDays(30)
        };
        context.Assignments.Add(Assignment);
        context.SaveChanges();

        // Create a document (simulate a file upload record)
        Document = new Document
        {
            Id = GuidDocument,
            UserId = Student.Id,
            AssignmentId = Assignment.Id,
            AttemptNumber = 1,
            FileName = "algebra_test_student1.pdf",
            FilePath = "/uploads/algebra_test_student1.pdf",
            UploadedAt = DateTime.UtcNow
        };
        context.Documents.Add(Document);
        context.SaveChanges();

        // Create grading criteria by teacher
        Criteria = new GradingCriteria
        {
            Id = GuidCriteria,
            Title = "Solution Correctness",
            Description = "Correctness of the solution and mathematical approach",
            Weight = 0.8,
            AssignmentId = Assignment.Id
        };
        context.GradingCriteria.Add(Criteria);
        context.SaveChanges();
    }
    
    private static void LoadEntityReferences(AppDbContext context)
    {
        // Load all entity references from the database
        TeacherUser = context.Users.FirstOrDefault(u => u.Id == GuidTeacher);
        Student = context.Users.FirstOrDefault(u => u.Id == GuidStudent);
        Group = context.StudentGroups.FirstOrDefault(g => g.Id == GuidGroup);
        Course1 = context.Courses.FirstOrDefault(c => c.Id == GuidCourse1);
        Course2 = context.Courses.FirstOrDefault(c => c.Id == GuidCourse2);
        Assignment = context.Assignments.FirstOrDefault(a => a.Id == GuidAssignment);
        Document = context.Documents.FirstOrDefault(d => d.Id == GuidDocument);
        Criteria = context.GradingCriteria.FirstOrDefault(c => c.Id == GuidCriteria);
    }
    
    /// <summary>
    /// Creates and adds a new test assignment to the database.
    /// </summary>
    public static async Task<Assignment> CreateTestAssignmentAsync(
        AppDbContext context, 
        string title = "Test Assignment",
        string description = "Test Description")
    {
        var assignment = new Assignment
        {
            Id = Guid.NewGuid(),
            GroupId = GuidGroup,
            CourseId = GuidCourse1,
            Title = title,
            Description = description,
            MaxAttempts = 3,
            DueDate = DateTime.UtcNow.AddDays(7)
        };
        
        context.Assignments.Add(assignment);
        await context.SaveChangesAsync();
        
        return assignment;
    }
    
    /// <summary>
    /// Creates a test document
    /// </summary>
    public static async Task<Document> CreateTestDocumentAsync(
        AppDbContext context,
        Guid assignmentId,
        Guid userId,
        string fileName = "test_document.pdf")
    {
        var document = new Document
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            AssignmentId = assignmentId,
            AttemptNumber = 1,
            FileName = fileName,
            FilePath = $"/uploads/{fileName}",
            UploadedAt = DateTime.UtcNow
        };
        
        context.Documents.Add(document);
        await context.SaveChangesAsync();
        
        return document;
    }
    
    // Add more helper methods for other entity types as needed
}