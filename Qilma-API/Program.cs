using System.Text;
using CrypticWizard.RandomWordGenerator;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Qilma_API.Constants;
using Qilma_API.Data;
using Qilma_API.Services;
using Qilma_API.Services.Interfaces;
using Qilma_API.Settings;
using Qilma_API.Validators;
using WeCantSpell.Hunspell;
using DotNetEnv;

Env.Load(".env");

var builder = WebApplication.CreateBuilder(args);

// -------------------- Environment Variables --------------------

// Database:
var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? throw new Exception("DB_PASSWORD environment variable not set");

// JWT
var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY") ?? throw new Exception("JWT_KEY environment variable not set");

// Email settings
var name = Environment.GetEnvironmentVariable("EMAIL_SENDER_NAME") ?? throw new Exception("EMAIL_SENDER_NAME environment variable not set");
var emailAddress = Environment.GetEnvironmentVariable("EMAIL_ADDRESS") ?? throw new Exception("EMAIL_ADDRESS environment variable not set");
var emailAppPassword = Environment.GetEnvironmentVariable("EMAIL_APP_PASSWORD") ?? throw new Exception("EMAIL_APP_PASSWORD environment variable not set");
var smtpHost = Environment.GetEnvironmentVariable("SMTP_HOST") ?? throw new Exception("SMTP_HOST environment variable not set");
var smtpPort = Environment.GetEnvironmentVariable("SMPT_PORT") ?? throw new Exception("SMPT_PORT environment variable not set");

// Frontend URL
var frontendUrl = Environment.GetEnvironmentVariable("FRONTEND_URL") ?? throw new Exception("FRONTEND_URL environment variable not set");

builder.Configuration["ConnectionStrings:DefaultConnection"] = builder.Configuration["ConnectionStrings:DefaultConnection"]!.Replace("{__DB_PASSWORD__}", dbPassword);
builder.Configuration["Jwt:Key"] = jwtKey;
builder.Configuration["Email:EmailSenderName"] = name;
builder.Configuration["Email:SenderAddress"] = emailAddress;
builder.Configuration["Email:AppPassword"] = emailAppPassword;
builder.Configuration["Email:SmtpHost"] = smtpHost;
builder.Configuration["Email:SmtpPort"] = smtpPort;
builder.Configuration["frontendUrl:Url"] = frontendUrl;


// -------------------- Controllers & Swagger --------------------
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

// -------------------- Database --------------------
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// -------------------- Dependency Injection --------------------
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IGuestService, GuestService>();
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IWordService, WordService>();
builder.Services.AddScoped<IStatisticService, StatisticService>();
builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddTransient<UserValidator>();
builder.Services.AddTransient<GameValidator>();
builder.Services.AddTransient<TokenValidator>();
builder.Services.AddTransient<PasswordValidator>();

builder.Services.AddSingleton<EmailValidator>();
builder.Services.AddSingleton<WordGenerator>();
builder.Services.AddSingleton(WordList.CreateFromFiles("Dictionaries/en_US.dic", "Dictionaries/en_US.aff"));

// -------------------- Email Settings --------------------
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("Email")
);

// -------------------- Authentication (JWT) --------------------
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var key = Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"]!);
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateLifetime = false, // set true in production
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"]
        };
        options.Events = new JwtBearerEvents
        {
            OnChallenge = async context =>
            {
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(ErrorMessages.UNAUTHORIZED_MESSAGE);
                context.HandleResponse();
            }
        };
    });

// -------------------- CORS --------------------

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(builder.Configuration["FrontendUrl:Url"]!)
            .AllowAnyHeader()
            .AllowAnyMethod());
});

// -------------------- Build App --------------------
var app = builder.Build();

// -------------------- Middleware --------------------
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseCors();
app.UseAuthorization();
app.MapControllers();

// -------------------- Run --------------------
app.Run();