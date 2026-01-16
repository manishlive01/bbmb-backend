// =======================
// USING STATEMENTS
// =======================
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Infrastructure.Persistence.FinanceContexts;
using BBMB.BudgetService.Shared.Behaviors;
using BBMB.BudgetService.Shared.Endpoints.EndpointExtensions;
using FluentValidation;
using Mapster;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

// =======================
// BUILDER
// =======================
var builder = WebApplication.CreateBuilder(args);

// ===================================================================
// DATABASE CONFIGURATION (Single DB, Multiple Schemas)
// ===================================================================

// Budget
builder.Services.AddDbContextPool<BudgetContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<BudgetDContext>();

// Finance
builder.Services.AddDbContextPool<FinanceContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<FinanceDContext>();

// ===================================================================
// CORS CONFIGURATION
// ===================================================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins(
                "http://10.147.8.83:8087",
                "http://localhost:3000"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); // remove if not using cookies/auth
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

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "BBMB Budget API",
        Version = "v1"
    });

    // JWT Support (if required later)
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Enter JWT with Bearer prefix",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });
});

builder.Services.AddHealthChecks()
    .AddNpgSql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        name: "postgres-db");

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

// ===================================================================
// SWAGGER MIDDLEWARE
// ===================================================================
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.DocumentTitle = "Budget API's";
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
    options.InjectStylesheet("/swagger-ui/custom.css");
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
