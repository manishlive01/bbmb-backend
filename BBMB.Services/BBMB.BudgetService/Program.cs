// =======================
// USING STATEMENTS
// =======================
using BBMB.BudgetService.Infrastructure.Persistence;
using BBMB.BudgetService.Shared.Behaviors;
using BBMB.BudgetService.Shared.Endpoints.EndpointExtensions;
using FluentValidation;
using Mapster;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// ===================================================================
// DATABASE CONFIGURATION
// ===================================================================
builder.Services.AddDbContextPool<BBMBContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<DapperContext>();

// ===================================================================
// MEDIATR (CQRS)
// ===================================================================
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

// Validation pipeline before handler execution
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
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHealthChecks();

builder.Services.AddSwaggerGen(c =>
{
    // JWT Bearer authentication support in Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter JWT with Bearer into field",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });
});

var app = builder.Build();

// ===================================================================
// AUTO-OPEN SWAGGER UI
// ===================================================================

// 🔹 Redirect application root ("/") directly to Swagger UI
// 🔹 So when you run the project, Swagger opens automatically
app.MapGet("/", () => Results.Redirect("/swagger"));

// ===================================================================
// OPENAPI
// ===================================================================
app.MapOpenApi();

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

    // Optional custom styling
    options.InjectStylesheet("/swagger-ui/custom.css");
});

// ===================================================================
// SECURITY & EXCEPTION HANDLING
// ===================================================================
app.UseHttpsRedirection();

// Global exception handler
app.UseExceptionHandler(appBuilder =>
{
    appBuilder.Run(async context =>
    {
        var exception = context.Features
            .Get<IExceptionHandlerFeature>()?.Error;

        // FluentValidation errors
        if (exception is FluentValidation.ValidationException validationException)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";

            var errors = validationException.Errors
                .Select(e => e.ErrorMessage)
                .ToArray();

            await context.Response.WriteAsJsonAsync(new
            {
                message = "Validation Message",
                errors
            });
            return;
        }

        // Invalid JSON payload
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

        // Fallback error
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
app.MapHealthChecks("/health");
// ===================================================================
// APPLICATION START
// ===================================================================
app.Run();
