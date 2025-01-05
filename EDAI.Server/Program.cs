using System.Text.Json.Serialization;
using EDAI.Server.Data;
using EDAI.Shared.Models;
using Json.More;
using Microsoft.EntityFrameworkCore;
using JsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<EdaiContext>(options => 
    options.UseSqlite(connectionString)
        .UseSeeding((context, _) =>
        {
            var testStudent = new Student()
                { FirstName = "Egon", LastName = "Olsen", Class = "5C", GraduationYear = 2030 };
            context.Set<Student>().Add(testStudent);
            var testAssignment = new Assignment() {Name = "First Assignment", Description = "This is the first assignment.", Open = true};
            context.Set<Assignment>().Add(testAssignment);
            var testEssay = new Essay() { Assignment = testAssignment, Student = testStudent, Evaluated = false };
            context.Set<Essay>().Add(testEssay);
            var testScore = new Score() {AssignmentAnswer = "Nice", EloquenceScore = 43, GrammarScore = 56, AssignmentAnswerScore = 32, OverallScore = 44, OverallStructure = "Very structured", Essay = testEssay};
            context.Set<Score>().Add(testScore);
            context.SaveChanges();
        }));

// Add services to the container.

builder.Services.AddControllersWithViews().AddJsonOptions(options => 
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve);
builder.Services.Configure<JsonOptions>(options => options.SerializerOptions.ReferenceHandler = ReferenceHandler.Preserve);
builder.Services.AddRazorPages();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseWebAssemblyDebugging();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();