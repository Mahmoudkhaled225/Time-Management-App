using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using src;
using src.A_GenericRepository;
using src.B_UnitOfWork;
using src.Context;
using src.Interfaces;
using src.Middlewares;
using src.Repositories;
using src.Services;

// <PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />
// <Using Include="BCrypt.Net-Next" Alias="bcrypt"/>
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo { Title = "Time-Management-App", Version = "v1" });
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid access token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
});

// Connection To The Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Authentication 
// fetch the jwt section from app settings.json
builder.Services.Configure<Jwt>(builder.Configuration.GetSection("JWT"));
// configuration 
builder.Services.AddAuthentication(options => {
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.SaveToken = true;
    //cause i work on http not https
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        RequireExpirationTime = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"])),
        // for removing the delay of token when it expires which is 5 minutes by default so we set it to zer
        ClockSkew = TimeSpan.Zero,
        // AuthenticationType = "all" // Change scheme to "all" instead of "Bearer"

    };
    options.Events = new JwtBearerEvents
    {
        // to change the header from Authorization to token
        // OnMessageReceived = context =>
        // {
        //     context.Token = context.Request.Headers["token"].FirstOrDefault(); // Change header to "token"
        //     return Task.CompletedTask;
        // },
        OnChallenge = context =>
        {
            context.HandleResponse();
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            var response = new { message = "Custom Unauthorized Message login first" };
            return context.Response.WriteAsJsonAsync(response);
        },
        OnForbidden = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";
            var response = new { message = "Custom Forbidden Message Roles thing" };
            return context.Response.WriteAsJsonAsync(response);
        }
    };
});

// Auto Mapper Configuration
builder.Services.AddAutoMapper(typeof(MappingProfile));

// CORS
builder.Services.AddCors(options => {
    options.AddDefaultPolicy(b => {
        b.AllowAnyHeader();
        b.AllowAnyOrigin();
        b.AllowAnyMethod();
    });
});


builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(e => e.Value.Errors.Count > 0)
            .SelectMany(x => x.Value.Errors)
            .Select(x => x.ErrorMessage).ToArray();

        return new BadRequestObjectResult(new { message = errors });
    };
});

// Add Project Services
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IUserRepository), typeof(UserRepository));
builder.Services.AddScoped(typeof(IToDoRepository), typeof(ToDoRepository));
builder.Services.AddScoped<IAuthService, AuthService>();

//Mail
builder.Services.Configure<Email>(builder.Configuration.GetSection("Email"));
builder.Services.AddScoped<IEmailService, EmailService>();
//Sms
builder.Services.Configure<Sms>(builder.Configuration.GetSection("Sms"));
builder.Services.AddScoped<ISmsService, SmsServiceService>();
//Cloudinary
builder.Services.Configure<ImgUpload>(builder.Configuration.GetSection("Cloudinary"));
builder.Services.AddScoped<IUploadImgService, UploadImgService>();


// For Eager Loading
builder.Services.AddControllers().AddJsonOptions(x =>
    x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve);


var app = builder.Build();
 

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ErrorHandlerMiddleware>();
app.UseCors(); 

app.UseStatusCodePages(async context => 
{
    context.HttpContext.Response.ContentType = "application/json";
    await context.HttpContext.Response.WriteAsync("invalid route.");
});


app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();