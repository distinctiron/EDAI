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
using Microsoft.AspNetCore.HttpOverrides;
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

var allowed = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];


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
    options.AddPolicy(name: "ClientPolicyProd",
        policy =>
        {
            //policy.WithOrigins("localhost:44388");
            //policy.AllowAnyOrigin().AllowAnyHeader();
            //.AllowAnyMethod().AllowCredentials();

            policy.WithOrigins(allowed);
            policy.AllowAnyHeader();
            policy.AllowAnyMethod();
            //policy.WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS");
            //policy.AllowCredentials();
        });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "ClientPolicyDev",
        policy =>
        {
            //policy.WithOrigins("localhost:44388");
            policy.AllowAnyOrigin().AllowAnyHeader();
            //.AllowAnyMethod().AllowCredentials();

            //policy.WithOrigins(allowed);
            policy.AllowAnyMethod();
            //policy.WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS");
            //policy.AllowCredentials();
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
    if (!db.Set<EDAIUser>().Any())
    {
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
        
    app.UseCors("ClientPolicyDev");
    app.UseHangfireDashboard();
}
else
{
    app.UseCors("ClientPolicyProd");
}

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedFor
});

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