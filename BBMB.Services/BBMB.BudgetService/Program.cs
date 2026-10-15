// =======================
// USING STATEMENTS
// =======================
using BBMB.BudgetService.Authorization.Extension;
using BBMB.BudgetService.Features.Estimates.GetById;
using BBMB.BudgetService.Features.Estimates.Update;
using BBMB.BudgetService.Infrastructure.DependencyInjection;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Infrastructure.Persistence.EmployeeContexts;
using BBMB.BudgetService.Infrastructure.Persistence.FinanceContexts;
using BBMB.BudgetService.Shared.Behaviors;
using BBMB.BudgetService.Shared.Endpoints.EndpointExtensions;
using BBMB.BudgetService.Shared.Services;
using BBMB.BudgetService.Shared.Services.EmployeeChargeServices;
using BBMB.BudgetService.Shared.Settings;
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

builder.Services.AddDatabaseContexts(builder.Configuration);
builder.Services.Configure<FileSettings>(
    builder.Configuration.GetSection("File"));

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
builder.Services.AddScoped<IFileStorageService,  FileStorageService>();
// Malware scanner boundary (design §4.8, OI-12): ClamAV when configured, no-scan in Development only, else refuse.
BBMB.BudgetService.Shared.Services.Attachments.AttachmentScannerRegistration.AddAttachmentScanner(
    builder.Services, builder.Configuration, builder.Environment);
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IChargeResolver, ChargeResolver>();
// Budget process: actor, action scope (DDD §7.1) and office scope (design §4.3, §4.4).
builder.Services.AddScoped<BBMB.BudgetService.Shared.Services.BudgetOffices.IBudgetOfficeScope,
    BBMB.BudgetService.Shared.Services.BudgetOffices.BudgetOfficeScope>();
builder.Services.AddScoped<BBMB.BudgetService.Shared.Services.BudgetActors.IBudgetActorSource,
    BBMB.BudgetService.Shared.Services.BudgetActors.HttpBudgetActorSource>();
builder.Services.AddScoped<BBMB.BudgetService.Shared.Services.BudgetActors.IBudgetActionScope,
    BBMB.BudgetService.Shared.Services.BudgetActors.BudgetActionScope>();
// External boundary (design §4.11): no gateway integrated, nothing is sent.
builder.Services.AddSingleton<BBMB.BudgetService.Shared.Services.Notifications.IBudgetNotificationGateway,
    BBMB.BudgetService.Shared.Services.Notifications.NoOpBudgetNotificationGateway>();
builder.Services.AddSingleton<BBMB.BudgetService.Shared.Services.FinancialAccounting.IFinancialAccountingGateway,
    BBMB.BudgetService.Shared.Services.FinancialAccounting.NotAvailableFinancialAccountingGateway>();
// The single path by which a proposal moves (design §6.2).
// T-17 value in force (design §5.3).
builder.Services.AddScoped<BBMB.BudgetService.Features.BudgetProcess.Settings.IBudgetSettingsReader,
    BBMB.BudgetService.Features.BudgetProcess.Settings.BudgetSettingsReader>();
// Demand line rule set (design §6.4).
builder.Services.AddScoped<BBMB.BudgetService.Features.BudgetProcess.DemandLines.Rules.DemandLineRuleInputsLoader>();
builder.Services.AddScoped<BBMB.BudgetService.Features.BudgetProcess.DemandLines.Rules.IDemandLineRules,
    BBMB.BudgetService.Features.BudgetProcess.DemandLines.Rules.DemandLineRules>();
// Work-master rules (design §5.3 first-cycle guard, §11.3 reference rule).
builder.Services.AddScoped<BBMB.BudgetService.Features.Works.Rules.WorkRules>();
builder.Services.AddScoped<BBMB.BudgetService.Features.BudgetProcess.Proposals.Rules.IProposalTransitionService,
    BBMB.BudgetService.Features.BudgetProcess.Proposals.Rules.ProposalTransitionService>();
// T-19 delegation limits in force (design §5.3; BUD-055 reads it).
builder.Services.AddScoped<BBMB.BudgetService.Features.BudgetProcess.Settings.IDelegationLimitReader,
    BBMB.BudgetService.Features.BudgetProcess.Settings.DelegationLimitReader>();
// F-01 account-code rules (design §8.4, VR-01, BUD-308).
builder.Services.AddScoped<BBMB.BudgetService.Features.HeadOfAccounts.Rules.AccountCodeRules>();
// F-03 / F-04 application rules (design §8.4).
builder.Services.AddScoped<BBMB.BudgetService.Features.BudgetProcess.AdminApprovals.Rules.AdminApprovalRules>();
// F-06 application rules: VR-02 under the DB-09 lock key, VR-11 via the FA gateway (design §8.4).
builder.Services.AddScoped<BBMB.BudgetService.Features.BudgetProcess.TechnicalSanctions.Rules.TechnicalSanctionRules>();
// Attachment download: "caller may view an owning record" (design §8.2).
builder.Services.AddScoped<BBMB.BudgetService.Features.BudgetProcess.Attachments.Rules.AttachmentAccessRules>();
// F-19 allocation: the plan computation shared by preview and allocate (design §8.6).
builder.Services.AddScoped<BBMB.BudgetService.Features.BudgetProcess.Allocations.Rules.AllocationRules>();
// F-20 freeze: the three preconditions of a cycle freeze (design §8.6, DB-19).
builder.Services.AddScoped<BBMB.BudgetService.Features.BudgetProcess.Freezes.Rules.FreezeRules>();
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
// Rights matrix read at each evaluation through IOptionsMonitor (design §4.4, BUD-033).
builder.Services.Configure<Auth.Authorization.Requirements.BudgetRightsOptions>(
    builder.Configuration.GetSection(Auth.Authorization.Requirements.BudgetRightsOptions.SectionName));

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
        // Environment-specific API base path
        var basePath = builder.Environment.IsDevelopment()
            ? ""
            : "/bbmb-api";

        document.Servers =
        [
            new OpenApiServer
            {
                Url = basePath
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
// ======================================================
// ROOT → SWAGGER
// ======================================================
if (app.Environment.IsDevelopment())
{
    app.MapGet("/", () => Results.Redirect("/swagger"));
}
else
{
    app.MapGet("/", () => Results.Redirect("/bbmb-api/swagger"));

}

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
    options.DocumentTitle = $"Environment: {app.Environment.EnvironmentName} - BBMB API's";
    if (app.Environment.IsDevelopment())
    {
        options.SwaggerEndpoint("/openapi/v1.json", "API v1");

    }
    else
    {
        options.SwaggerEndpoint("/bbmb-api/openapi/v1.json", "API v1");
    }
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
        // Form-data binding errors (YOUR CASE)
        if (exception is BadHttpRequestException badFormRequest)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;

            await context.Response.WriteAsJsonAsync(new
            {
                message = "Invalid form data",
                error = badFormRequest.Message
            });
            return;
        }// Null reference inside FluentValidation (your case)
        if (exception is NullReferenceException nullEx)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsJsonAsync(new
            {
                message = "Validation Error",
                error = "A required object was null. Please check request payload.",
                details = nullEx.Message
            });

            return;
        }

        // Database rule failures (design §4.9, §10): catalogue message, no table/constraint name in the body.
        var logger = context.RequestServices.GetRequiredService<ILoggerFactory>()
            .CreateLogger("BBMB.BudgetService.ExceptionHandler");
        var mapped = BBMB.BudgetService.Shared.Endpoints.BudgetExceptionMapper.TryMap(exception);
        if (mapped is not null)
        {
            // A DB rule caught what a pre-check should have: Warning, with the constraint/trigger in the log only.
            logger.LogWarning("Database rule refused the request: {MessageId} {Detail}",
                mapped.Body.MessageId, mapped.LogDetail);
            context.Response.StatusCode = mapped.StatusCode;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(mapped.Body);
            return;
        }
        // Fallback (includes DB-28 'ACTOR': a handler wrote without opening the action scope).
        logger.LogError(exception, "Unhandled exception");
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

// Estimate read/update are static map-extension endpoints (not IEndpoint classes), so they are wired here
// beside app.MapEndpoints(). Both carry their design §9.3 policy (GET -> Budget.View, PUT -> Legacy.BudgetEntry)
// and are re-keyed to {workId:int} after the P4 work-code reset (P4-03). Without these two calls the routes
// are unreachable even though their query/command/handler exist — a defect, not intended behaviour.
app.MapGetEstimateByIdEndpoint();
app.MapUpdateEstimateEndpoint();

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
