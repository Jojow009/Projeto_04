using KnewinApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.HttpOverrides;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using System; // Para a classe Uri

// Correção do Fuso Horário do Postgres
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// --- MUDANÇA (Conversor de Conexão Robusto) ---
// 1. Pega a string de conexão (VEM DO RENDER ENV VAR)
var connectionStringUrl = builder.Configuration.GetConnectionString("DefaultConnection");
string connectionString;

// 2. Verifica se é uma URL (do Render) ou uma string normal (local)
if (!string.IsNullOrEmpty(connectionStringUrl) && connectionStringUrl.StartsWith("postgresql://"))
{
    // É a URL do Render, vamos converter
    var databaseUri = new Uri(connectionStringUrl);
    var userInfo = databaseUri.UserInfo.Split(':');
    var dbUser = userInfo[0];
    var dbPass = userInfo[1];
    var dbHost = databaseUri.Host;
    var dbPort = databaseUri.Port;
    var dbName = databaseUri.LocalPath.TrimStart('/');

    connectionString = $"Host={dbHost};Port={dbPort};Database={dbName};Username={dbUser};Password={dbPass};";
}
else
{
    // Se não encontrou a variável do Render, vai usar a string local (Host=...) do appsettings.json
    // Se essa também falhar (como agora), 'connectionString' fica null e o app falha.
    connectionString = connectionStringUrl;
}
// --- FIM DA MUDANÇA ---


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

// --- MUDANÇA (Usa a nossa string final) ---
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString)); // <-- Usa a string que processámos

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
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation(">>> Migrations aplicadas com SUCESSO. <<<");
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

// --- 4. PIPELINE ---
app.UseForwardedHeaders();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(myAllowSpecificOrigins);
app.UseHttpsRedirection();
// app.UseAuthorization(); (Removido)

app.MapControllers();
app.Run();
