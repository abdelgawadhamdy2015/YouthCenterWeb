using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using YouthCenterWeb.Models;
using YouthCenterWeb.YouthCenterWeb.Application.Common.Constants;
using YouthCenterWeb.YouthCenterWeb.Application.Common.Enums;
using YouthCenterWeb.YouthCenterWeb.Infrastructure;

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

#region 🔹 DependencyInjection
builder.Services.AddIDependencyInjection(builder.Configuration);
#endregion

#region 🔹 JWT CONFIG (FIXED 100%)
var jwtSection = builder.Configuration.GetSection("Jwt");

var jwtKey = jwtSection["Key"];
var jwtIssuer = jwtSection["Issuer"];
var jwtAudience = jwtSection["Audience"];
if (string.IsNullOrWhiteSpace(jwtKey))
{
    throw new Exception("JWT Key is missing in appsettings.json");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,

        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtKey)
        )
    };
});
#endregion

#region  🔹 Policy
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(Policies.RequireAuthenticated, policy =>
        policy.RequireAuthenticatedUser());

    options.AddPolicy(Policies.RequireAdmin, policy =>
        policy.RequireClaim(ClaimTypes.Role, UserRole.Admin.ToString(), UserRole.SuperAdmin.ToString()));

    options.AddPolicy(Policies.RequireSuperAdmin, policy =>
        policy.RequireClaim(ClaimTypes.Role, UserRole.SuperAdmin.ToString()));

});

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

#region 🔹 Error Handling (Production)
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

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
#endregion

app.Run();