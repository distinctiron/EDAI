using System.Text.Json.Serialization;
using EDAI.Server.Data;
using EDAI.Server.Hubs;
using EDAI.Services;
using EDAI.Services.Interfaces;
using EDAI.Shared.Factories;
using EDAI.Shared.Models;
using EDAI.Shared.Models.Entities;
using EDAI.Shared.Tools;
using Json.More;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using JsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;
using Hangfire;
using Hangfire.MemoryStorage;

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

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "MyAllowSpecificOrigins",
        policy =>
        {
            policy.WithOrigins("localhost:44388");
            policy.WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS");
            policy.AllowCredentials();
        });
});

// Add services to the container.

builder.Services.AddTransient<IWordFileHandlerFactory, WordFileHandlerFactory>();
builder.Services.AddScoped<IOpenAiService, OpenAiService>();
builder.Services.AddControllersWithViews();
    //.AddJsonOptions(options => options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve);
builder.Services.Configure<JsonOptions>(options => options.SerializerOptions.ReferenceHandler = ReferenceHandler.Preserve);
builder.Services.AddRazorPages();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddSignalR();
builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        ["application/octet-stream" ]);
});

builder.Services.AddHangfire(configuration => configuration.UseMemoryStorage());

var app = builder.Build();

app.UseResponseCompression();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseWebAssemblyDebugging();
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseHangfireDashboard();
}

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.MapHub<MessageHub>("/messagehub");

app.Run();