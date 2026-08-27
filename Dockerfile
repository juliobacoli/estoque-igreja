FROM node:24-alpine AS frontend-build
WORKDIR /app/frontend

COPY source/frontend/package*.json ./
RUN npm ci

COPY source/frontend/ ./
RUN npm run build -- --configuration production

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS backend-build
WORKDIR /app/backend

COPY source/backend/EstoqueIgreja.sln ./
COPY source/backend/EstoqueIgreja.Domain/*.csproj ./EstoqueIgreja.Domain/
COPY source/backend/EstoqueIgreja.Application/*.csproj ./EstoqueIgreja.Application/
COPY source/backend/EstoqueIgreja.Infrastructure/*.csproj ./EstoqueIgreja.Infrastructure/
COPY source/backend/EstoqueIgreja.Api/*.csproj ./EstoqueIgreja.Api/
RUN dotnet restore EstoqueIgreja.sln

COPY source/backend/ ./

COPY --from=frontend-build /app/frontend/dist/frontend/browser ./EstoqueIgreja.Api/wwwroot

RUN dotnet publish EstoqueIgreja.Api/EstoqueIgreja.Api.csproj -c Release -o /app/publish
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS final
WORKDIR /app

RUN apk add --no-cache icu-libs tzdata fontconfig ttf-dejavu

COPY --from=backend-build /app/publish .

EXPOSE 8080

# O GC deriva seu heap hard limit do limite do container (1 GB), e um teto de
# 750 MB o deixa sem incentivo para compactar. O teto explícito de 100 MB, com
# ConserveMemory devolvendo segmentos ao SO, mede ~8% de RSS a menos sob carga.
# gcConcurrent=0 foi testado e descartado: sem background GC as coleções gen2
# viram bloqueantes e mais raras, e o RSS subiu 37%.
ENV PORT=8080 \
    DOTNET_GCHeapHardLimit=0x6400000 \
    DOTNET_GCConserveMemory=9

ENTRYPOINT ["sh", "-c", "ASPNETCORE_URLS=http://+:${PORT} dotnet EstoqueIgreja.Api.dll"]
