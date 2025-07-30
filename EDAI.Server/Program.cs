using System.Text.Json.Serialization;
using EDAI.Server.Data;
using EDAI.Server.Hubs;
using EDAI.Server.Jobs;
using EDAI.Services;
using EDAI.Services.Interfaces;
using EDAI.Shared.Factories;
using EDAI.Shared.Models;
using EDAI.Shared.Models.Entities;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using JsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;
using Hangfire;
using Hangfire.MemoryStorage;
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<EdaiContext>(options => 
    options.UseSqlite(connectionString)
        .UseSeeding((context, _) =>
        {
            var studentClass = new StudentClass() { Class = "1C", School = "Katedralskolen" };
            context.Set<StudentClass>().Add(studentClass);
            var testStudent1 = new Student()
                { FirstName = "John", LastName = "Doe", Class = "1C", GraduationYear = 2030, StudentClass = studentClass};
            var testStudent2 = new Student()
                { FirstName = "Jane", LastName = "Doe", Class = "1C", GraduationYear = 2030, StudentClass = studentClass };
            var testStudent3 = new Student()
                { FirstName = "Ellen", LastName = "Doe", Class = "1C", GraduationYear = 2030, StudentClass = studentClass };
            context.Set<Student>().Add(testStudent1);
            context.Set<Student>().Add(testStudent2);
            context.Set<Student>().Add(testStudent3);
            var testAssignment = new Assignment() {Name = "First Assignment", Description = "This is the first assignment.", Open = true};
            
            context.Set<Assignment>().Add(testAssignment);
            context.SaveChanges();
        }));

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "MyAllowSpecificOrigins",
        policy =>
        {
            //policy.WithOrigins("localhost:44388");
            policy.AllowAnyOrigin().AllowAnyHeader();
                //.AllowAnyMethod().AllowCredentials();

            policy.WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS");
            policy.AllowCredentials();
        });
});

// Add services to the container.

builder.Services.AddTransient<IWordFileHandlerFactory, WordFileHandlerFactory>();
builder.Services.AddScoped<IOpenAiService, OpenAiService>();
builder.Services.AddScoped<IGenerateScoreService, GenerateScoreService>();
builder.Services.AddScoped<IGenerateStudentSummaryService, GenerateStudentSummaryService>();
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
builder.Services.AddHangfireServer();

var app = builder.Build();

app.UseResponseCompression();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    Env.Load();
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