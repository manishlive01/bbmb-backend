// =======================
// USING STATEMENTS
// =======================
using BBMB.BudgetService.Authorization.Extension;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Infrastructure.Persistence.FinanceContexts;
using BBMB.BudgetService.Shared.Behaviors;
using BBMB.BudgetService.Shared.Endpoints.EndpointExtensions;
using FluentValidation;
using Mapster;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using OfficeOpenXml;
using System.Security.Cryptography;

// =======================
// BUILDER
// =======================
var builder = WebApplication.CreateBuilder(args);
Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;

// ===================================================================
// DATABASE CONFIGURATION (Single DB, Multiple Schemas)
// ===================================================================

// Budget
builder.Services.AddDbContextPool<BudgetContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")),
    poolSize: 5);
 

// Finance
builder.Services.AddDbContextPool<FinanceContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")),
    poolSize: 5);
 

// ===================================================================
// CORS CONFIGURATION
// ===================================================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
             .AllowAnyOrigin() 
            .AllowAnyHeader()
            .AllowAnyMethod() ;  
    });
});

// ===================================================================
// MEDIATR (CQRS)
// ===================================================================
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

// Validation pipeline
builder.Services.AddTransient(
    typeof(IPipelineBehavior<,>),
    typeof(ValidationBehavior<,>)
);

// ===================================================================
// MAPSTER
// ===================================================================
TypeAdapterConfig.GlobalSettings.Scan(typeof(Program).Assembly);
builder.Services.AddSingleton(TypeAdapterConfig.GlobalSettings);
builder.Services.AddScoped<IMapper, ServiceMapper>(); 

// ===================================================================
// SWAGGER / OPENAPI
// ===================================================================
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddAuthentication("Bearer")
.AddJwtBearer("Bearer", options =>
{
    options.Authority = builder.Configuration["SSO:Authority"];   // IMPORTANT
    options.RequireHttpsMetadata = false; // because your SSO is HTTP

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        //ValidIssuer = builder.Configuration["SSO:Issuer"],//use this in case of only trusted one Issuer

        ValidateAudience = true,
        ValidAudiences = builder.Configuration
    .GetSection("SSO:ValidAudiences")
    .Get<string[]>(),

        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };

    options.TokenValidationParameters.TokenDecryptionKey =
        new SymmetricSecurityKey(
            Convert.FromBase64String(builder.Configuration["SSO:EncryptionKey"]));

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine("AUTH FAILED: " + context.Exception.Message);
            return Task.CompletedTask;
        }
    };
});


// ======================================================
// 🔐 CUSTOM PROJECT ROLE AUTHORIZATION
// ======================================================
builder.Services.AddProjectRoleAuthorization();

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

        // Add Bearer Security Scheme
        document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Enter JWT token"
        };

        // Apply globally
        document.Security =
        [
            new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecuritySchemeReference("Bearer"),
                    []
                }
            }
        ];

        document.SetReferenceHostDocument();

        return Task.CompletedTask;
    });
});
builder.Services.AddHealthChecks()
    .AddNpgSql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        name: "postgres-db"); 
builder.Services.AddAuthorization();

// ===================================================================
// For excel
// ===================================================================
ExcelPackage.License.SetNonCommercialOrganization("BBMB");


// ===================================================================
// BUILD APP
// ===================================================================
var app = builder.Build();

// ===================================================================
// AUTO REDIRECT TO SWAGGER
// ===================================================================
app.MapGet("/", () => Results.Redirect("/swagger"));

// ===================================================================
// CORS (MUST BE BEFORE ROUTING)
// ===================================================================
app.UseCors("AllowFrontend");

// ===================================================================
// CORE MIDDLEWARE
// ===================================================================
app.UseRouting();
app.UseStaticFiles();
 app.UseAuthentication();
app.UseAuthorization();


app.MapOpenApi();
// ======================================================
// SWAGGER UI
// ======================================================
app.UseSwaggerUI(options =>
{
    options.DocumentTitle = $"Environment: {app.Environment.EnvironmentName} - Auth API's";
    options.SwaggerEndpoint("/openapi/v1.json", "API v1");
});
// ===================================================================
// SECURITY
// ===================================================================
app.UseHttpsRedirection();

// ===================================================================
// GLOBAL EXCEPTION HANDLER
// ===================================================================
app.UseExceptionHandler(appBuilder =>
{
    appBuilder.Run(async context =>
    {
        var exception = context.Features
            .Get<IExceptionHandlerFeature>()?.Error;

        // FluentValidation errors
        if (exception is ValidationException validationException)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";

            var errors = validationException.Errors
                .Select(e => e.ErrorMessage)
                .ToArray();

            await context.Response.WriteAsJsonAsync(new
            {
                message = "Validation Error",
                errors
            });
            return;
        }

        // Invalid JSON
        if (exception is BadHttpRequestException badRequest &&
            badRequest.InnerException is System.Text.Json.JsonException jsonEx)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsJsonAsync(new
            {
                message = "Invalid JSON payload",
                error = jsonEx.Message
            });
            return;
        }

        // Fallback
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(new
        {
            message = "An unexpected error occurred"
        });
    });
});

// ===================================================================
// ENDPOINT MAPPING
// ===================================================================
app.MapEndpoints();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";

        var response = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                error = e.Value.Exception?.Message
            })
        };

        await context.Response.WriteAsJsonAsync(response);
    }
});

// ===================================================================
// RUN APPLICATION
// ===================================================================
app.Run();
