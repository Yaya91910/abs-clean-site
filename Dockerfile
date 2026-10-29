# Image unique ABS clean : l'API ASP.NET Core sert aussi le site Angular (même domaine, pas de CORS).

# 1. Site Angular
FROM node:22-alpine AS web
WORKDIR /src/web
COPY web/package.json web/package-lock.json ./
RUN npm ci
COPY web/ ./
RUN npx ng build

# 2. API .NET
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS api
WORKDIR /src
COPY api/AbsClean.Api.csproj api/
RUN dotnet restore api/AbsClean.Api.csproj
COPY api/ api/
RUN dotnet publish api/AbsClean.Api.csproj -c Release -o /app --no-restore

# 3. Exécution
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=api /app ./
COPY --from=web /src/web/dist/web/browser ./wwwroot
ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "AbsClean.Api.dll"]
