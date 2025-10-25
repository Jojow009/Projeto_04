# Estágio 1: Build (Compilar o projeto)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia todos os arquivos do projeto para o container
COPY . .

# Restaura os pacotes do projeto correto
RUN dotnet restore "backend/KnewinApi/KnewinApi.csproj"

# Publica o projeto em modo Release para a pasta /app/publish
RUN dotnet publish "backend/KnewinApi/KnewinApi.csproj" -c Release -o /app/publish

# Estágio 2: Final (Rodar a aplicação)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Inicia a API (o nome do .dll vem do nome do .csproj)
ENTRYPOINT ["dotnet", "KnewinApi.dll"]