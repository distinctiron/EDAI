using System.Text;
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
using EDAI.Server.Tools;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: true)
    .AddEnvironmentVariables();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

Console.WriteLine(connectionString);


builder.Services.AddDbContext<EdaiContext>(options => 
    options.UseNpgsql(connectionString));

builder.Services.AddIdentity<EDAIUser, IdentityRole>()
    .AddEntityFrameworkStores<EdaiContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "EDAI",
            ValidAudience = "EDAI-Client",
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

builder.Services.AddAuthorization();

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
builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddSignalR();
builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        ["application/octet-stream" ]);
});

builder.Services.AddHangfire(configuration => configuration.UsePostgreSqlStorage(connectionString, new PostgreSqlStorageOptions
{
    SchemaName = "HangFire"
}));
builder.Services.AddHangfireServer();

builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo{Title = "EDAI", Version = "1.0.0"});
        
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: 'Bearer 12345abcdef'",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            }
        );
        
        c.OperationFilter<AuthorizeCheckOperationFilter>();
        
        /*c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    },
                    Scheme = "Bearer",
                    Name = "Bearer",
                    In = ParameterLocation.Header
                },
                new List<string>()
            }
        });*/
    }
);

//builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var usermanager = services.GetRequiredService<UserManager<EDAIUser>>();
    var rolemanager = services.GetRequiredService<RoleManager<IdentityRole>>();
    
    var db = scope.ServiceProvider.GetRequiredService<EdaiContext>();
    db.Database.Migrate();
    if (!db.Set<StudentClass>().Any())
            {
                var organisation = new Organisation() { Name = "Katedralskolen", CVR = "1234567890" };
                var studentClass = new StudentClass() { Class = "1C", Organisation = organisation };
                db.Set<StudentClass>().Add(studentClass);
                var testStudent1 = new Student()
                    { FirstName = "Alice", LastName = "Doe", Class = "1C", GraduationYear = 2030, StudentClass = studentClass};
                var testStudent2 = new Student()
                    { FirstName = " Alma", LastName = "Doe", Class = "1C", GraduationYear = 2030, StudentClass = studentClass};
                var testStudent3 = new Student()
                    { FirstName = " Amalie", LastName = "Doe", Class = "1C", GraduationYear = 2030, StudentClass = studentClass};
                var testStudent4 = new Student()
                    { FirstName = " Anne Mona", LastName = "Doe", Class = "1C", GraduationYear = 2030, StudentClass = studentClass};
                var testStudent5 = new Student()
                    { FirstName = " Asbjørn", LastName = "Doe", Class = "1C", GraduationYear = 2030, StudentClass = studentClass};
                var testStudent6 = new Student()
                    { FirstName = " August", LastName = "Doe", Class = "1C", GraduationYear = 2030, StudentClass = studentClass};
                var testStudent7 = new Student()
                    { FirstName = " Camille", LastName = "Doe", Class = "1C", GraduationYear = 2030, StudentClass = studentClass};
                var testStudent8 = new Student()
                    { FirstName = " Carl", LastName = "Doe", Class = "1C", GraduationYear = 2030, StudentClass = studentClass};
                var testStudent9 = new Student()
                    { FirstName = " Carla", LastName = "Doe", Class = "1C", GraduationYear = 2030, StudentClass = studentClass};
                var testStudent10 = new Student()
                    { FirstName = " Coco", LastName = "Doe", Class = "1C", GraduationYear = 2030, StudentClass = studentClass};
                var testStudent11 = new Student()
                    { FirstName = " Ella", LastName = "Doe", Class = "1C", GraduationYear = 2030, StudentClass = studentClass};
                var testStudent12 = new Student()
                    { FirstName = " Elsa", LastName = "Doe", Class = "1C", GraduationYear = 2030, StudentClass = studentClass};
                var testStudent13 = new Student()
                    { FirstName = " Esther", LastName = "Doe", Class = "1C", GraduationYear = 2030, StudentClass = studentClass};
                var testStudent14 = new Student()
                    { FirstName = " Ida H", LastName = "Doe", Class = "1C", GraduationYear = 2030, StudentClass = studentClass};
                var testStudent15 = new Student()
                    { FirstName = " Ida C", LastName = "Doe", Class = "1C", GraduationYear = 2030, StudentClass = studentClass};
                var testStudent16 = new Student()
                    { FirstName = " Ingrid", LastName = "Doe", Class = "1C", GraduationYear = 2030, StudentClass = studentClass};
                var testStudent17 = new Student()
                    { FirstName = " Janus", LastName = "Doe", Class = "1C", GraduationYear = 2030, StudentClass = studentClass};
                var testStudent18 = new Student()
                    { FirstName = " Jonathan", LastName = "Doe", Class = "1C", GraduationYear = 2030, StudentClass = studentClass};
                var testStudent19 = new Student()
                    { FirstName = " Karl", LastName = "Doe", Class = "1C", GraduationYear = 2030, StudentClass = studentClass};
                var testStudent20 = new Student()
                    { FirstName = " Laura", LastName = "Doe", Class = "1C", GraduationYear = 2030, StudentClass = studentClass};
                var testStudent21 = new Student()
                    { FirstName = " Luka", LastName = "Doe", Class = "1C", GraduationYear = 2030, StudentClass = studentClass};
                var testStudent22 = new Student()
                    { FirstName = " Noah", LastName = "Doe", Class = "1C", GraduationYear = 2030, StudentClass = studentClass};
                var testStudent23 = new Student()
                    { FirstName = " Othilia", LastName = "Doe", Class = "1C", GraduationYear = 2030, StudentClass = studentClass};
                var testStudent24 = new Student()
                    { FirstName = " Pelle", LastName = "Doe", Class = "1C", GraduationYear = 2030, StudentClass = studentClass};
                var testStudent25 = new Student()
                    { FirstName = " Theodor", LastName = "Doe", Class = "1C", GraduationYear = 2030, StudentClass = studentClass};
                var testStudent26 = new Student()
                    { FirstName = " Timian", LastName = "Doe", Class = "1C", GraduationYear = 2030, StudentClass = studentClass};
                var testStudent27 = new Student()
                    { FirstName = " Ursula", LastName = "Doe", Class = "1C", GraduationYear = 2030, StudentClass = studentClass};

                
                db.Set<Student>().Add(testStudent1);
                db.Set<Student>().Add(testStudent2);
                db.Set<Student>().Add(testStudent3);
                db.Set<Student>().Add(testStudent4);
                db.Set<Student>().Add(testStudent5);
                db.Set<Student>().Add(testStudent6);
                db.Set<Student>().Add(testStudent7);
                db.Set<Student>().Add(testStudent8);
                db.Set<Student>().Add(testStudent9);
                db.Set<Student>().Add(testStudent10);
                db.Set<Student>().Add(testStudent11);
                db.Set<Student>().Add(testStudent12);
                db.Set<Student>().Add(testStudent13);
                db.Set<Student>().Add(testStudent14);
                db.Set<Student>().Add(testStudent15);
                db.Set<Student>().Add(testStudent16);
                db.Set<Student>().Add(testStudent17);
                db.Set<Student>().Add(testStudent18);
                db.Set<Student>().Add(testStudent19);
                db.Set<Student>().Add(testStudent20);
                db.Set<Student>().Add(testStudent21);
                db.Set<Student>().Add(testStudent22);
                db.Set<Student>().Add(testStudent23);
                db.Set<Student>().Add(testStudent24);
                db.Set<Student>().Add(testStudent25);
                db.Set<Student>().Add(testStudent26);
                db.Set<Student>().Add(testStudent27);
                await db.SaveChangesAsync();
                await DataSeeder.SeedUserAsync(usermanager, rolemanager, db);
            }
    
    
}

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

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

app.MapWhen(ctx => !ctx.Request.Path.StartsWithSegments("/api"), builder =>
{
    builder.UseBlazorFrameworkFiles();
    builder.UseStaticFiles();
    builder.UseRouting();
    builder.UseEndpoints(endpoints =>
    {
        endpoints.MapFallbackToFile("index.html");
    });
});

app.MapHub<MessageHub>("/messagehub");

app.Run();