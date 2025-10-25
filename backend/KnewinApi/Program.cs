using KnewinApi.Data;                 // Para o AppDbContext
using Microsoft.EntityFrameworkCore;  // Para o UseNpgsql
using Microsoft.AspNetCore.HttpOverrides; // Para o ForwardedHeaders (correção do Render)
using System.Text.Json.Serialization;  // Para a correção do loop do JSON

var builder = WebApplication.CreateBuilder(args);

// --- 1. DEFINIÇÃO DA POLÍTICA DE CORS ---
// Definimos um nome para a política de CORS
var myAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: myAllowSpecificOrigins,
                      policy =>
                      {
                          // Permite que seu frontend local (rodando em localhost) 
                          // se comunique com a API no Render.
                          // Adicione outras portas se precisar (ex: "http://localhost:3000")
                          policy.WithOrigins("http://localhost:5173",
                                             "http://localhost:3000") 
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                      });
});

// --- 2. CONFIGURAÇÃO DOS SERVIÇOS ---

// Adiciona os Controllers E corrige o erro de "ciclo infinito" (loop) do JSON
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

// Configura o DbContext para usar Postgres com a Connection String do appsettings.json
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configura o Proxy (Forwarded Headers) para funcionar corretamente no Render
// Isso corrige o aviso "Failed to determine the https port for redirect"
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

// Adiciona serviços do Swagger (para documentação da API)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// --- 3. CONSTRUÇÃO DO APP ---
var app = builder.Build();

// --- 4. CONFIGURAÇÃO DO PIPELINE (A ORDEM É MUITO IMPORTANTE) ---

// Usa o Forwarded Headers EM PRIMEIRO LUGAR
// Isso informa ao app que ele está atrás de um proxy (o Render)
app.UseForwardedHeaders();

// Usa o Swagger apenas em ambiente de desenvolvimento
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Redireciona de HTTP para HTTPS (agora vai funcionar)
app.UseHttpsRedirection();

// Aplica a política de CORS que definimos
// DEVE VIR ANTES de UseAuthorization
app.UseCors(myAllowSpecificOrigins);

// Habilita a autorização
app.UseAuthorization();

// Mapeia os seus Controllers (EmpresasController, etc.)
app.MapControllers();

// Inicia a aplicação
app.Run();

