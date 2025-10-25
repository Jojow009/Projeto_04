using KnewinApi.Data;                 // Para o AppDbContext
using Microsoft.EntityFrameworkCore;  // Para o UseNpgsql
using Microsoft.AspNetCore.HttpOverrides; // Para o ForwardedHeaders (Render)
using System.Text.Json.Serialization;  // Para o ReferenceHandler.IgnoreCycles

var builder = WebApplication.CreateBuilder(args);

// --- 1. DEFINIÇÃO DA POLÍTICA DE CORS ---
// Definimos um nome para a política de CORS
var myAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: myAllowSpecificOrigins,
                      policy =>
                      {
                          // Permite que seu frontend local (localhost:3000 ou 5173) 
                          // se comunique com a API
                          policy.WithOrigins("http://localhost:3000",
                                             "http://localhost:5173")
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                      });
});

// --- 2. CONFIGURAÇÃO DOS SERVIÇOS ---

// Adiciona os Controllers e corrige o erro de "ciclo infinito" (loop) do JSON
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

// Configura o DbContext para usar Postgres com a Connection String do appsettings.json
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configura o Proxy (Forwarded Headers) para funcionar corretamente no Render
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

// --- 4. CONFIGURAÇÃO DO PIPELINE (A ORDEM É IMPORTANTE) ---

// Usa o Forwarded Headers EM PRIMEIRO LUGAR
// Isso corrige o aviso de "https port redirect" no Render
app.UseForwardedHeaders();

// Usa o Swagger apenas em ambiente de desenvolvimento
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Redireciona de HTTP para HTTPS
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
