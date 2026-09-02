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

ENV PORT=8080
ENV DOTNET_GCHeapHardLimit=134217728
ENTRYPOINT ["sh", "-c", "ASPNETCORE_URLS=http://+:${PORT} dotnet EstoqueIgreja.Api.dll"]
