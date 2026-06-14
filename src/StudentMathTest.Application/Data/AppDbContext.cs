using Microsoft.EntityFrameworkCore;
using StudentMathTest.Domain.Models;

namespace StudentMathTest.Application.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Teacher> Teachers => Set<Teacher>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Exam> Exams => Set<Exam>();
    public DbSet<TaskResult> TaskResults => Set<TaskResult>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Teacher>(e =>
        {
            e.HasKey(t => t.Id);
            e.Property(t => t.Name).IsRequired().HasMaxLength(200);
            e.HasMany(t => t.Students).WithOne(s => s.Teacher).HasForeignKey(s => s.TeacherId);
        });

        modelBuilder.Entity<Student>(e =>
        {
            e.HasKey(s => s.Id);
            e.Property(s => s.Name).IsRequired().HasMaxLength(200);
            e.HasMany(s => s.Exams).WithOne(ex => ex.Student).HasForeignKey(ex => ex.StudentId);
        });

        modelBuilder.Entity<Exam>(e =>
        {
            e.HasKey(ex => ex.Id);
            e.Property(ex => ex.XmlExamId).IsRequired().HasMaxLength(100);
            e.HasMany(ex => ex.Tasks).WithOne(t => t.Exam).HasForeignKey(t => t.ExamId);
        });

        modelBuilder.Entity<TaskResult>(e =>
        {
            e.HasKey(t => t.Id);
            e.Property(t => t.XmlTaskId).IsRequired().HasMaxLength(100);
            e.Property(t => t.RawExpression).IsRequired().HasMaxLength(500);
            e.Property(t => t.StudentAnswer).HasPrecision(18, 6);
            e.Property(t => t.ComputedResult).HasPrecision(18, 6);
        });
    }
}
