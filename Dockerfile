# Stage 1: Build & Publish
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# 1. Copy all project definition files to preserve relative folder structure
COPY ["src/QueryQuill.Domain/QueryQuill.Domain.csproj", "src/QueryQuill.Domain/"]
COPY ["src/QueryQuill.Core/QueryQuill.Core.csproj", "src/QueryQuill.Core/"]
COPY ["src/QueryQuill.Infrastructure/QueryQuill.Infrastructure.csproj", "src/QueryQuill.Infrastructure/"]
COPY ["src/QueryQuill.Api/QueryQuill.Api.csproj", "src/QueryQuill.Api/"]

# 2. Restore NuGet dependencies (cached unless a .csproj changes)
RUN dotnet restore "src/QueryQuill.Api/QueryQuill.Api.csproj"

# 3. Copy full source code and build Release package
COPY . .
WORKDIR "/src/src/QueryQuill.Api"
RUN dotnet publish "QueryQuill.Api.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# Stage 2: Hardened Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Run as non-root user (built into Microsoft .NET 8+ base images)
USER $APP_UID

COPY --from=build --chown=$APP_UID:$APP_UID /app/publish .

# Configure standard ASP.NET Core port
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "QueryQuill.Api.dll"]