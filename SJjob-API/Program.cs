using BusinessObjects.Models;
using DataAccess;
using DataAccess.DAO;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Net.payOS;
using Repository.Implementations;
using Repository.Interfaces;
using SJjob_API.Middleware;
using SJjob_API.Services;
using Sjob_API.Services;
using System;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "SJOB API", Version = "v1" });

    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            Array.Empty<string>()
        }
    });
});// Add DbContext
builder.Services.AddDbContext<SjobPContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));



// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
});

builder.Services.AddSingleton<PayOS>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var clientId = configuration["Environment:PAYOS_CLIENT_ID"];
    var apiKey = configuration["Environment:PAYOS_API_KEY"];
    var checksumKey = configuration["Environment:PAYOS_CHECKSUM_KEY"];

    if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(checksumKey))
    {
        throw new Exception("PayOS configuration is missing. Please check appsettings.json");
    }

    return new PayOS(clientId, apiKey, checksumKey);
});


// Add PayOS Service
builder.Services.AddScoped<PayOSService>();

// Register DAO
builder.Services.AddScoped<UserDAO>();
builder.Services.AddScoped<JobPostDAO>();
builder.Services.AddScoped<WishlistDAO>();
builder.Services.AddScoped<ApplicationDAO>();
builder.Services.AddScoped<WorkerVisitDAO>();
builder.Services.AddScoped<CustomerDAO>();
builder.Services.AddScoped<JobPostManagementDAO>();
builder.Services.AddScoped<EmployerDAO>();
builder.Services.AddScoped<ApplicationManagementDAO>();
builder.Services.AddScoped<CreditDAO>();
builder.Services.AddScoped<CustomerServiceDAO>();
builder.Services.AddScoped<CreditTransactionDAO>();

// Register Repository
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IWorkerVisitRepository, WorkerVisitRepository>();
builder.Services.AddScoped<IWishlistRepository, WishlistRepository>();
builder.Services.AddScoped<IApplicationRepository, ApplicationRepository>();
builder.Services.AddScoped<IJobPostRepository, JobPostRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IJobPostManagementRepository, JobPostManagementRepository>();
builder.Services.AddScoped<IEmployerRepository, EmployerRepository>();
builder.Services.AddScoped<IApplicationManagementRepository, ApplicationManagementRepository>();
builder.Services.AddScoped<ICreditRepository, CreditRepository>();
builder.Services.AddScoped<ICustomerServiceRepository, CustomerServiceRepository>();
builder.Services.AddScoped<ICreditTransactionRepository, CreditTransactionRepository>();


builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddHostedService<ExpiredSubscriptionService>();


// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.ASCII.GetBytes(jwtSettings["Secret"]);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddGoogle(options =>
{
    var googleAuthSection = builder.Configuration.GetSection("GoogleAuth");
    options.ClientId = googleAuthSection["ClientId"];
    options.ClientSecret = googleAuthSection["ClientSecret"];
    options.CallbackPath = googleAuthSection["CallbackPath"];
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        ClockSkew = TimeSpan.Zero
    };
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication(); // Add this line
app.UseAuthorization();
app.UseMiddleware<ExpiredSubscriptionMiddleware>();
app.MapControllers();

app.Run();
