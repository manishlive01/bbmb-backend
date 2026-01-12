var builder = DistributedApplication.CreateBuilder(args); 
builder.AddProject<Projects.BBMB_BudgetService>("bbmb-budgetservice").WithHttpHealthCheck("/health");
//builder.AddExternalService("bbmb-budgetservice", "https://localhost:44345");
builder.Build().Run();
