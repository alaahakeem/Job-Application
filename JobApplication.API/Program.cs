using System.Text;
using JobApplication.API.Services;
using JobApplication.Application.Common;
using JobApplication.Application.Interfaces;
using JobApplication.Infrastructure.Auth;
using JobApplication.Infrastructure.Persistence;
using JobApplication.Infrastructure.Repositories;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace JobApplication.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            var connectionString =
            builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string"
                + "'DefaultConnection' not found.");

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            builder.Services.AddScoped<ICvStorage, LocalCvStorage>();

            // ---------- CQRS / MediatR ----------
            builder.Services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(typeof(JobApplication.Application.AssemblyReference).Assembly));

            // ---------- Identity (users + passwords) ----------
            builder.Services
                .AddIdentityCore<ApplicationUser>(options =>
                {
                    options.User.RequireUniqueEmail = true;

                    options.Password.RequiredLength = 8;
                    options.Password.RequireDigit = true;
                    options.Password.RequireLowercase = true;
                    options.Password.RequireUppercase = true;
                    options.Password.RequireNonAlphanumeric = true;

                    options.Lockout.MaxFailedAccessAttempts = 5;
                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                })
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            // ---------- JWT settings ----------
            builder.Services.Configure<JwtSettings>(
                builder.Configuration.GetSection(JwtSettings.SectionName));

            var jwtSettings = builder.Configuration
                .GetSection(JwtSettings.SectionName)
                .Get<JwtSettings>()
                ?? throw new InvalidOperationException("The 'Jwt' section is missing from configuration.");

            if (string.IsNullOrWhiteSpace(jwtSettings.SecretKey) || jwtSettings.SecretKey.Length < 32)
                throw new InvalidOperationException(
                    "Jwt:SecretKey is missing or shorter than 32 characters. Set it with user-secrets.");

            // ---------- Authentication (validate incoming tokens) ----------
            builder.Services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    // Keep the claim names exactly as written in the token ("sub", "email", "name").
                    options.MapInboundClaims = false;

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = jwtSettings.Issuer,
                        ValidateAudience = true,
                        ValidAudience = jwtSettings.Audience,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero,
                        RoleClaimType = Roles.ClaimType // makes [Authorize(Roles = ...)] read the "role" claim
                    };
                });

            builder.Services.AddAuthorization();

            // ---------- Auth use case Infrastructure implementations (consumed directly by the Auth handlers) ----------
            builder.Services.AddScoped<IIdentityService, IdentityService>();
            builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

            // ---------- Swagger (with an "Authorize" button for the JWT) ----------
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Paste the token only (without the word 'Bearer')."
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
                        Array.Empty<string>()
                    }
                });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            // Order matters: who are you? (authentication) BEFORE are you allowed? (authorization)
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
