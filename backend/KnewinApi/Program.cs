
using KnewinApi.Data;                 // Para o AppDbContext
using Microsoft.EntityFrameworkCore;  // Para o UseNpgsql
using Microsoft.AspNetCore.HttpOverrides; // Para o ForwardedHeaders
using System.Text.Json.Serialization;  // Para a correção do loop do JSON
// Adicionado para ILogger
using Microsoft.Extensions.Logging; 

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

// --- NOVO BLOCO: APLICAR MIGRATIONS NA INICIALIZAÇÃO ---
// Isso garante que o banco de dados na nuvem seja criado e atualizado
// ANTES que a API comece a rodar.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var dbContext = services.GetRequiredService<AppDbContext>();
        // Aplica qualquer migration pendente
        await dbContext.Database.MigrateAsync();
    }
    catch (Exception ex)
    {
        // Se falhar, registra no log (você pode ver no log do Render)
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocorreu um erro ao aplicar as migrations.");

        // --- MUDANÇA: Adiciona log explícito no Console ---
        // Isso é para garantir que vejamos o erro no log do Render
        Console.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
        Console.WriteLine("!!!   ERRO AO EXECUTAR DBContext.Database.MigrateAsync()   !!!");
        Console.WriteLine($"!!!   ERRO: {ex.Message}   !!!");
        Console.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
        // --- FIM DA MUDANÇA ---
    }
}
// --- FIM DO NOVO BLOCO ---


// --- 4. CONFIGURAÇÃO DO PIPELINE ---
app.UseForwardedHeaders();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(myAllowSpecificOrigins);

// app.UseAuthorization(); // (Corretamente removido)

app.MapControllers();
app.Run();
