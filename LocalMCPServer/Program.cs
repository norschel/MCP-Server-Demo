
Console.WriteLine("Starting MCP server...");
Console.WriteLine("Enter AzD PAT token:");
var pat = Console.ReadLine();
Environment.SetEnvironmentVariable("AZURE_DEVOPS_PAT", pat);

//var builder = Host.CreateApplicationBuilder(args);
var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddConsole(consoleLogOptions =>
{
    // Configure all logs to go to stderr
    consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
});

builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    //.WithStdioServerTransport()
    .WithToolsFromAssembly();

var app = builder.Build();

app.MapMcp();

await app.RunAsync("http://localhost:3001");