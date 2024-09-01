using Health.Api.Services;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Health.Api.Data;
using Health.Api.Repositories;
using Health.Api.Services.Patients;
using Health.Api.Services.Users;
using Health.Api.Repositories.Users;
using Health.Api.Repositories.Patients;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Health.Api.Authentication;
using Health.Api.Models.Constants;
using Health.Api.Repositories.Customers;
using Health.Api.Services.Customers;
using Health.Api.Repositories.Operational;
using Health.Api.Services.Operational;

string allowedOriginsKey = "MyAllowedOrigins";

var builder = WebApplication.CreateBuilder(args);

var levelSwitch = new LoggingLevelSwitch();
levelSwitch.MinimumLevel = LogEventLevel.Debug;
var logger = new LoggerConfiguration()
    .MinimumLevel.ControlledBy(levelSwitch)
    .WriteTo.Console()
    .WriteTo.File("health-api.log.txt"
        , rollOnFileSizeLimit: true
        , rollingInterval: RollingInterval.Month
        , fileSizeLimitBytes: 314572800)
    .CreateLogger();

builder.Logging.AddSerilog(logger);
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: allowedOriginsKey,
    policy =>
    {
        policy.WithOrigins(new string[] { "http://localhost:3000", "http://localhost:9000" })
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

var securityScheme = new OpenApiSecurityScheme
{
    Name = "Authorization",
    Type = SecuritySchemeType.ApiKey,
    Scheme = "Bearer",
    BearerFormat = "JWT",
    In = ParameterLocation.Header,
    Description = "JSON Web token based security"
};

var securityReq = new OpenApiSecurityRequirement
{
    {
        new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
            }
        },
        new string[] {}
    }
};

var contact = new OpenApiContact
{
    Name = "Yogesh Kadalgikar",
    Email = "yogesh@example.com",
    Url = new Uri("https://www.example.com")
};

var license = new OpenApiLicense()
{
    Name = "Free License",
    Url = new Uri("https://www.example.com")
};

var info = new OpenApiInfo()
{
    Version = "v1",
    Title = "Minimal API - JWT Authentication with Swagger demo",
    Description = "Implementing JWT Authentication in Minimal API",
    TermsOfService = new Uri("https://www.example.com"),
    Contact = contact,
    License = license
};

builder.Services.AddAuthentication(o =>
{
    o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    o.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(o =>
{
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey
            (Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "my-key")),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ClockSkew = TimeSpan.Zero
    };
    o.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
            {
                context.Response.Headers.Append("token-expired", "true");
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    o.SwaggerDoc("v1", info);
    o.AddSecurityDefinition("Bearer", securityScheme);
    o.AddSecurityRequirement(securityReq);
});
builder.Services.AddDbContext<HASDbContext>(options =>
{
    string connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? default!;
    options.UseMySQL(connectionString);
});

builder.Services.AddScoped<ITokenManager, TokenManager>();
builder.Services.AddScoped<IDataAuditHistoryRepository, DataAuditHistoryRepository>();
builder.Services.AddScoped<IMasterRepository, MasterRepository>();
builder.Services.AddScoped<IReportingRepository, ReportingRepository>();
builder.Services.AddScoped<IAppUserRepository, AppUserRepository>();
builder.Services.AddScoped<IAppUserAddressRepository, AppUserAddressRepository>();
builder.Services.AddScoped<IAppUserPhoneRepository, AppUserPhoneRepository>();
builder.Services.AddScoped<IAppUserEmailRepository, AppUserEmailRepository>();
builder.Services.AddScoped<IAppUserIdentityRepository, AppUserIdentityRepository>();
builder.Services.AddScoped<IPatientRepository, PatientRepository>();
builder.Services.AddScoped<IPatientAddressRepository, PatientAddressRepository>();
builder.Services.AddScoped<IPatientPhoneRepository, PatientPhoneRepository>();
builder.Services.AddScoped<IPatientEmailRepository, PatientEmailRepository>();
builder.Services.AddScoped<IPatientIdentityRepository, PatientIdentityRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<ICustomerAddressRepository, CustomerAddressRepository>();
builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddScoped<IMedicalServiceRepository, MedicalServiceRepository>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();

builder.Services.AddAuthorizationBuilder()
    .AddPolicy(AuthPolicy.SystemAdmin, policy => policy.RequireRole("SYSADMIN"));

var app = builder.Build();
app.UseCors(allowedOriginsKey);
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => "Hello World!");

MarketService.Register(app);
MasterService.Register(app);
AppUserService.Register(app);
PatientService.Register(app);
CustomerService.Register(app);
ReportingService.Register(app);
OperationalService.Register(app);

app.Run();

public partial class Program { }