using KnewinApi.Data;                 // Para o AppDbContext
using Microsoft.EntityFrameworkCore;  // Para o UseNpgsql
using Microsoft.AspNetCore.HttpOverrides; // Para o ForwardedHeaders
using System.Text.Json.Serialization;  // Para a correção do loop do JSON
using Microsoft.Extensions.Logging;     // Para o ILogger

// Correção do Fuso Horário do Postgres
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// --- 1. DEFINIÇÃO DA POLÍTICA DE CORS ---
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

// --- 2. CONFIGURAÇÃO DOS SERVIÇOS ---
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// --- 3. CONSTRUÇÃO DO APP ---
var app = builder.Build();

// --- APLICA MIGRATIONS NA INICIALIZAÇÃO ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var dbContext = services.GetRequiredService<AppDbContext>();
        await dbContext.Database.MigrateAsync();
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


// --- 4. CONFIGURAÇÃO DO PIPELINE (ORDEM CORRIGIDA) ---
app.UseForwardedHeaders();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// --- MUDANÇA ---
// 1. Aplica a política de CORS PRIMEIRO
app.UseCors(myAllowSpecificOrigins);

// 2. Redireciona para HTTPS DEPOIS
app.UseHttpsRedirection();
// --- FIM DA MUDANÇA ---

// app.UseAuthorization(); // (Corretamente removido)

app.MapControllers();
app.Run();
