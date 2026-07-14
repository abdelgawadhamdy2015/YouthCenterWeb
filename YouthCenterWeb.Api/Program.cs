using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using YouthCenterWeb.Infrastructure.Firebase;
using YouthCenterWeb.YouthCenterWeb.API.Middleware;
using YouthCenterWeb.YouthCenterWeb.Application.Common.Constants;
using YouthCenterWeb.YouthCenterWeb.Application.Common.Enums;
using YouthCenterWeb.YouthCenterWeb.Application.Services;
using YouthCenterWeb.YouthCenterWeb.Infrastructure;
using YouthCenterWeb.YouthCenterWeb.Infrastructure.Configurations;
using YouthCenterWeb.YouthCenterWeb.Infrastructure.Data;
using YouthCenterWeb.YouthCenterWeb.Infrastructure.Hubs;

var builder = WebApplication.CreateBuilder(args);

#region 🔹 Controllers + JSON
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new TimeOnlyConverter());
        options.JsonSerializerOptions.DefaultIgnoreCondition =
            System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddEndpointsApiExplorer();
#endregion

#region 🔹 Swagger + JWT Auth UI
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "YouthCenter API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {your_token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
    });
});
#endregion

#region 🔹 Dependency Injection
builder.Services.AddIDependencyInjection(builder.Configuration);

var serviceAccountJson = builder.Configuration["Firebase:ServiceAccountJson"];

if (string.IsNullOrWhiteSpace(serviceAccountJson))
{
    throw new InvalidOperationException(
        "Firebase:ServiceAccountJson configuration value is missing. " +
        "Set it via 'dotnet user-secrets' locally or as an environment variable in production.");
}

try
{
    FirebaseSetup.InitializeFromJson(serviceAccountJson);
}
catch (Exception ex)
{
    Console.WriteLine($"[STARTUP ERROR] Firebase init failed: {ex.Message}");
    throw;
}
#endregion

#region 🔹 JWT AUTH (single source of truth — bound options + session validation + SignalR support)

var jwtOptions = builder.Configuration
    .GetSection(JwtOptions.SectionName)
    .Get<JwtOptions>();

if (jwtOptions is null || string.IsNullOrWhiteSpace(jwtOptions.Key))
{
    throw new Exception("JWT configuration is missing or invalid");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtOptions.Key))
        };

        options.Events = new JwtBearerEvents
        {
            // Lets SignalR read the JWT from the query string on the hub's
            // WebSocket handshake, since it can't set an Authorization header there.
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                if (!string.IsNullOrEmpty(accessToken) &&
                    path.StartsWithSegments("/hubs/session"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            },

            // Enforces single active session: rejects any token whose "sid"
            // claim doesn't match the user's CurrentSessionId in the DB.
            OnTokenValidated = async context =>
            {
                var userIdClaim = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
                var sidClaim = context.Principal?.FindFirstValue(ClaimsConstants.SessionId);

                if (userIdClaim == null || sidClaim == null)
                {
                    context.Fail("Missing session claim.");
                    return;
                }

                var dbContext = context.HttpContext.RequestServices
                    .GetRequiredService<AppDbContext>();

                var currentSessionId = await dbContext.Users
                    .Where(u => u.Id == int.Parse(userIdClaim))
                    .Select(u => u.CurrentSessionId)
                    .FirstOrDefaultAsync();

                if (currentSessionId == null || currentSessionId.ToString() != sidClaim)
                {
                    context.Fail("Session revoked — logged in on another device.");
                }
            }
        };
    });

#endregion

#region 🔹 Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(Policies.RequireAuthenticated, policy =>
        policy.RequireAuthenticatedUser());

    options.AddPolicy(Policies.RequireAdmin, policy =>
        policy.RequireClaim(ClaimTypes.Role,
            UserRole.Admin.ToString(),
            UserRole.SuperAdmin.ToString()));

    options.AddPolicy(Policies.RequireSuperAdmin, policy =>
        policy.RequireClaim(ClaimTypes.Role,
            UserRole.SuperAdmin.ToString()));
});
#endregion

#region 🔹 Background Services
builder.Services.AddHostedService<ReservationStatusBackgroundService>();
#endregion

var app = builder.Build();

#region 🔹 Swagger UI
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "YouthCenter API V1");
    c.RoutePrefix = string.Empty;
});
#endregion

#region 🔹 Production Error Handling
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler(handler =>
    {
        handler.Run(async context =>
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsJsonAsync(new
            {
                Message = "Unexpected server error",
                Status = 500
            });
        });
    });

    app.UseHsts();
}
#endregion

#region 🔹 Middleware Pipeline
app.UseHttpsRedirection();
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<SessionHub>("/hubs/session");
app.MapHealthChecks("/health");
#endregion

app.Run();