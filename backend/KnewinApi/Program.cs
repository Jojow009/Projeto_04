using KnewinApi.Data;                 
using Microsoft.EntityFrameworkCore;  
using Microsoft.AspNetCore.HttpOverrides; 
using System.Text.Json.Serialization;  
using Microsoft.Extensions.Logging;     

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// --- CONSTRUÇÃO MANUAL DA CONNECTION STRING ---
var dbHost = Environment.GetEnvironmentVariable("PGHOST");
var dbUser = Environment.GetEnvironmentVariable("PGUSER");
var dbPass = Environment.GetEnvironmentVariable("PGPASSWORD");
var dbName = Environment.GetEnvironmentVariable("PGDATABASE");
var connectionString = $"Host={dbHost};Database={dbName};Username={dbUser};Password={dbPass};";
// --- FIM ---

// --- 1. POLÍTICA DE CORS ---
var myAllowSpecificOrigins = "_myAllowSpecificOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: myAllowSpecificOrigins,
                      policy =>
                      {
                          policy.AllowAnyOrigin() 
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                      });
});

// --- 2. SERVIÇOS ---
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString)); // <-- Usa a string manual

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// --- 3. CONSTRUÇÃO DO APP ---
var app = builder.Build();

// --- APLICA MIGRATIONS ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var dbContext = services.GetRequiredService<AppDbContext>();
        await dbContext.Database.MigrateAsync(); 
        // Log de Sucesso (para vermos no Render)
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Migrations aplicadas com SUCESSO.");
        Console.WriteLine(">>> Migrations aplicadas com SUCESSO. <<<");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocorreu um erro ao aplicar as migrations.");

        Console.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
        Console.WriteLine("!!!   ERRO AO EXECUTAR DBContext.Database.MigrateAsync()   !!!");
        Console.WriteLine($"!!!   ERRO: {ex.Message}   !!!");
        Console.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
    }
}
// --- FIM DO BLOCO ---


// --- 4. PIPELINE (ORDEM CORRIGIDA) ---
app.UseForwardedHeaders();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(myAllowSpecificOrigins); // <-- CORS ANTES
app.UseHttpsRedirection();           // <-- HTTPS DEPOIS

// app.UseAuthorization(); // (Corretamente removido)

app.MapControllers();
app.Run();

