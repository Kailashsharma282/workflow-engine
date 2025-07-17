using Microsoft.OpenApi.Models;
using WorkflowEngine.Models;
using WorkflowEngine.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Workflow Engine API",
        Version = "v1",
        Description = "Configurable State Machine Workflow Engine",
        Contact = new OpenApiContact
        {
            Name = "Developer",
            Email = "dev@example.com"
        }
    });

    // Include XML comments if available
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Register workflow service
builder.Services.AddSingleton<IWorkflowService, WorkflowService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Workflow Engine v1");
    c.RoutePrefix = string.Empty; // Set Swagger UI at root
});

app.UseHttpsRedirection();
app.UseCors("AllowAll");

// Health check endpoint
app.MapGet("/", () => Results.Ok(new
{
    Status = "Running",
    Timestamp = DateTime.UtcNow
})).WithTags("System");

// Workflow Definition Endpoints
app.MapPost("/definitions",
    (WorkflowDefinition definition, IWorkflowService service) =>
    {
        try
        {
            var result = service.CreateDefinition(definition);
            return Results.Created($"/definitions/{result.Id}", result);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Invalid workflow definition",
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest);
        }
    })
    .WithName("CreateWorkflowDefinition")
    .WithOpenApi(operation => new(operation)
    {
        Summary = "Create a new workflow definition",
        Description = "Creates a state machine workflow with specified states and transitions"
    });

app.MapGet("/definitions",
    (IWorkflowService service) => Results.Ok(service.GetAllDefinitions()))
    .WithName("GetAllWorkflowDefinitions")
    .WithOpenApi();

app.MapGet("/definitions/{id}",
    (Guid id, IWorkflowService service) =>
        service.GetDefinition(id) is { } definition
            ? Results.Ok(definition)
            : Results.NotFound())
    .WithName("GetWorkflowDefinition")
    .WithOpenApi();

// Workflow Instance Endpoints
app.MapPost("/instances",
    (CreateInstanceRequest request, IWorkflowService service) =>
    {
        try
        {
            var instance = service.CreateInstance(request.DefinitionId);
            return Results.Created($"/instances/{instance.Id}", instance);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Failed to create instance",
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest);
        }
    })
    .WithName("CreateWorkflowInstance")
    .WithOpenApi();

app.MapGet("/instances",
    (IWorkflowService service) => Results.Ok(service.GetAllInstances()))
    .WithName("GetAllWorkflowInstances")
    .WithOpenApi();

app.MapGet("/instances/{id}",
    (Guid id, IWorkflowService service) =>
        service.GetInstance(id) is { } instance
            ? Results.Ok(instance)
            : Results.NotFound())
    .WithName("GetWorkflowInstance")
    .WithOpenApi();

app.MapPost("/instances/{id}/execute",
    (Guid id, ExecuteActionRequest request, IWorkflowService service) =>
    {
        try
        {
            var instance = service.ExecuteAction(id, request.ActionId);
            return Results.Ok(instance);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Failed to execute action",
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest);
        }
    })
    .WithName("ExecuteWorkflowAction")
    .WithOpenApi();

app.Run();

// Request DTOs
public record CreateInstanceRequest(Guid DefinitionId);
public record ExecuteActionRequest(string ActionId);