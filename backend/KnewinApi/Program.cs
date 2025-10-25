using KnewinApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.HttpOverrides;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using System; 

// Correção do Fuso Horário do Postgres
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// --- MUDANÇA (Conversor de Connection String) ---
// 1. Pega a string de conexão "Internal Database URL" que VEM do Render
var connectionStringUrl = builder.Configuration.GetConnectionString("DefaultConnection");
string connectionString;

if (string.IsNullOrEmpty(connectionStringUrl))
{
    // Se não houver, usa o localhost (para seus testes locais)
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
}
else
{
    // 2. Converte a URL (postgresql://user:pass@host:port/db) para o formato Host=...
    var databaseUri = new Uri(connectionStringUrl);
    var userInfo = databaseUri.UserInfo.Split(':');
    var dbUser = userInfo[0];
    var dbPass = userInfo[1];
    var dbHost = databaseUri.Host;
    var dbPort = databaseUri.Port;
    var dbName = databaseUri.LocalPath.TrimStart('/');

    connectionString = $"Host={dbHost};Port={dbPort};Database={dbName};Username={dbUser};Password={dbPass};";
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

// --- MUDANÇA (Usa a nossa string convertida) ---
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString)); // <-- Usa a string que CONVERTEMOS

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


app.MapControllers();
app.Run();
