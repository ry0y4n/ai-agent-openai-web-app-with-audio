using dotnetfashionassistant.Components;
using dotnetfashionassistant.Configuration;
using dotnetfashionassistant.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<SpeechOptions>(builder.Configuration.GetSection(SpeechOptions.SectionName));

// Add services to the container.
builder.Services.AddSignalR(options =>
{
    // Increase maximum message size so voice payloads don't disconnect the circuit.
    // Default is 32 KB which isn't enough for multi-second PCM audio.
    options.MaximumReceiveMessageSize = 1024 * 1024; // 1 MB
});

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Default HttpClient for cart and inventory API calls
builder.Services.AddHttpClient("LocalApi", (serviceProvider, client) =>
{
    // Get the current HttpContext to determine the base URL
    var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
    var request = httpContextAccessor.HttpContext?.Request;
    
    string baseUrl;

    if (request?.Host.HasValue == true)
    {
        // Use the current request (available during standard HTTP pipeline)
        baseUrl = $"{request.Scheme}://{request.Host}/";
    }
    else
    {
        // Blazor Server circuits don't have HttpContext; fall back to environment/configuration
    var websiteHost = Environment.GetEnvironmentVariable("WEBSITE_HOSTNAME");
    var configuredBaseUrl = serviceProvider.GetRequiredService<IConfiguration>()["App:PublicBaseUrl"]?.Trim();

        if (!string.IsNullOrWhiteSpace(configuredBaseUrl))
        {
            baseUrl = EnsureTrailingSlash(configuredBaseUrl);
        }
        else if (!string.IsNullOrWhiteSpace(websiteHost))
        {
            baseUrl = $"https://{websiteHost.TrimEnd('/')}/";
        }
        else
        {
            baseUrl = "https://localhost/";
        }
    }

    client.BaseAddress = new Uri(baseUrl);
});

static string EnsureTrailingSlash(string value)
{
    if (string.IsNullOrWhiteSpace(value))
    {
        return value;
    }

    return value.EndsWith('/') ? value : value + "/";
}

builder.Services.AddBlazorBootstrap();

// Register CartUpdateService as a singleton so it can be used for cross-component communication
builder.Services.AddSingleton<dotnetfashionassistant.Services.CartUpdateService>();

// Register the AzureAIAgentService
builder.Services.AddScoped<dotnetfashionassistant.Services.AzureAIAgentService>();

// Speech認識サービス
builder.Services.AddScoped<SpeechRecognitionService>();

// Register AgentModeService as a singleton to persist mode state across the application
builder.Services.AddSingleton<dotnetfashionassistant.Services.AgentModeService>();

// Register InventoryFilterService as a singleton to share filter state across components
builder.Services.AddSingleton<dotnetfashionassistant.Services.InventoryFilterService>();

// Add HttpContextAccessor to access the current request context
builder.Services.AddHttpContextAccessor();

// Add controllers for API endpoints
builder.Services.AddControllers();

// Register the Swagger generator and define the OpenAPI specification
builder.Services.AddSwaggerGen(c =>
{    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Fashion Store Inventory API",
        Version = "v1",
        Description = "API for managing fashion store inventory"
    });

    // Use XML comments for Swagger documentation
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    
    // Enable XML comments if the file exists
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler("/Error", createScopeForErrors: true);
app.UseHsts();
app.UseHttpsRedirection();

// Configure Swagger for production use only
app.UseSwagger(c => 
{
    // Dynamically set the server URL based on request
    c.PreSerializeFilters.Add((swaggerDoc, httpReq) => 
    {
        swaggerDoc.Servers = new List<OpenApiServer> 
        { 
            new OpenApiServer { Url = $"{httpReq.Scheme}://{httpReq.Host.Value}" } 
        };
    });
});

// Configure Swagger UI for production
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Fashion Store Inventory API v1");
    c.RoutePrefix = "api/docs";
    c.EnableFilter(); // Add filtering capability
    c.DisplayRequestDuration(); // Show request timing info
});

app.UseStaticFiles();
app.UseAntiforgery();

// Map controllers for API endpoints
app.MapControllers();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
