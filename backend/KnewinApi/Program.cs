using KnewinApi.Data; // Importa seu DbContext
using Microsoft.EntityFrameworkCore; // Importa o Entity Framework

var builder = WebApplication.CreateBuilder(args);

// --- Configuração dos Serviços ---

// 1. Adiciona os serviços de API (Controllers)
builder.Services.AddControllers();

// 2. Configura o DbContext para usar Postgres com a sua Connection String
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 3. Configura o CORS para permitir que o React (rodando na porta 5173) acesse esta API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        builder => builder.WithOrigins("http://localhost:5173") // Porta padrão do Vite (React)
                          .AllowAnyHeader()
                          .AllowAnyMethod());
});

// 4. Adiciona serviços do Swagger (para documentação da API)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// --- Construção do Aplicativo ---
var app = builder.Build();

// Configure the HTTP request pipeline (A ordem aqui é importante)

// 1. Usa o Swagger em ambiente de desenvolvimento
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 2. Redireciona de HTTP para HTTPS (se configurado)
app.UseHttpsRedirection();

// 3. Aplica a política de CORS que definimos ("AllowReactApp")
app.UseCors("AllowReactApp");

// 4. Habilita a autorização (se você usar)
app.UseAuthorization();

// 5. Mapeia os seus Controllers (EmpresasController, FornecedoresController, etc.)
app.MapControllers();

// 6. Inicia a aplicação
app.Run();