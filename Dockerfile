# ---------- Etapa 1: build do Angular ----------
FROM node:24-alpine AS frontend-build
WORKDIR /app/frontend

COPY source/frontend/package*.json ./
RUN npm ci

COPY source/frontend/ ./
RUN npm run build -- --configuration production

# ---------- Etapa 2: build do .NET ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS backend-build
WORKDIR /app/backend

# Só os .csproj primeiro: assim a camada de restore só invalida quando uma
# dependência muda, não a cada alteração de código.
COPY source/backend/EstoqueIgreja.sln ./
COPY source/backend/EstoqueIgreja.Domain/*.csproj ./EstoqueIgreja.Domain/
COPY source/backend/EstoqueIgreja.Application/*.csproj ./EstoqueIgreja.Application/
COPY source/backend/EstoqueIgreja.Infrastructure/*.csproj ./EstoqueIgreja.Infrastructure/
COPY source/backend/EstoqueIgreja.Api/*.csproj ./EstoqueIgreja.Api/
RUN dotnet restore EstoqueIgreja.sln

COPY source/backend/ ./

# Caminho conferido: builder @angular/build:application + outputPath dist/frontend
# geram os arquivos em dist/frontend/browser.
COPY --from=frontend-build /app/frontend/dist/frontend/browser ./EstoqueIgreja.Api/wwwroot

RUN dotnet publish EstoqueIgreja.Api/EstoqueIgreja.Api.csproj -c Release -o /app/publish

# ---------- Etapa 3: imagem final de execução ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=backend-build /app/publish .

EXPOSE 8080

# O Railway injeta a porta em PORT. Sem PORT definida (build local), cai em 8080.
ENV PORT=8080
ENTRYPOINT ["sh", "-c", "ASPNETCORE_URLS=http://+:${PORT} dotnet EstoqueIgreja.Api.dll"]
