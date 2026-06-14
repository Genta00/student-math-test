using Microsoft.EntityFrameworkCore;
using StudentMathTest.Application.Data;
using StudentMathTest.Application.Services;
using StudentMathTest.Domain.Interfaces;
using StudentMathTest.MathEngine;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=mathtest.db"));

builder.Services.AddScoped<IMathEngine, MathEngineAdapter>();
builder.Services.AddScoped<IXmlProcessor, XmlProcessorService>();
builder.Services.AddScoped<IExamService, ExamService>();
builder.Services.AddScoped<ITeacherService, TeacherService>();

builder.Services.AddRazorPages();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Student Math Test API",
        Version = "v1",
        Description = "API for teachers to upload student math exams and for students to review their results."
    });
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Student Math Test API v1");
    c.RoutePrefix = "swagger";
});

app.UseStaticFiles();
app.MapRazorPages();
app.MapControllers();

app.Run();
