using System.Text.Json.Serialization;
using EDAI.Server.Data;
using EDAI.Services;
using EDAI.Services.Interfaces;
using EDAI.Shared.Models;
using EDAI.Shared.Models.Entities;
using EDAI.Shared.Tools;
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
            var fileBytes = new byte[10];
            var testDocument = new EdaiDocument() {DocumentFile = fileBytes, DocumentFileExtension = "docx", DocumentName = "TestDocument"};
            context.Set<EdaiDocument>().Add(testDocument);
            var testEssay = new Essay() { Assignment = testAssignment, Student = testStudent, Evaluated = false, Document = testDocument};
            context.Set<Essay>().Add(testEssay);
            //var testScore = new Score() {AssignmentAnswer = "Nice", EloquenceScore = 4, GrammarScore = 3, AssignmentAnswerScore = 3, OverallScore = 4, OverallStructure = "Very structured", Essay = testEssay};
            //context.Set<Score>().Add(testScore);
            context.SaveChanges();
        }));

// Add services to the container.

builder.Services.AddScoped<IWordFileHandler, WordFileHandler>();
builder.Services.AddScoped<IOpenAiService, OpenAiService>();
builder.Services.AddControllersWithViews();
    //.AddJsonOptions(options => options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve);
builder.Services.Configure<JsonOptions>(options => options.SerializerOptions.ReferenceHandler = ReferenceHandler.Preserve);
builder.Services.AddRazorPages();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(typeof(MappingProfile));

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