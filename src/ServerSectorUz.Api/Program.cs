using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using ServerSectorUz.Core.Models.Constants;
using ServerSectorUz.Core.Models.Configurations;
using ServerSectorUz.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructureServices(builder.Configuration);
JwtOptions jwtOptions = builder.Configuration
    .GetSection(JwtOptions.SectionName)
    .Get<JwtOptions>() ?? new JwtOptions();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
                Encoding.UTF8.GetBytes(jwtOptions.SecurityKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdmin", policy =>
        policy.RequireRole(SystemRoles.Admin));

    options.AddPolicy("RequireHr", policy =>
        policy.RequireRole(SystemRoles.Hr));

    options.AddPolicy("RequireAccountant", policy =>
        policy.RequireRole(SystemRoles.Accountant));

    options.AddPolicy("RequireOfficeManager", policy =>
        policy.RequireRole(SystemRoles.OfficeManager));

    options.AddPolicy("RequireInstaller", policy =>
        policy.RequireRole(SystemRoles.Installer));

    options.AddPolicy("RequireUser", policy =>
        policy.RequireRole(SystemRoles.User));
});
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
