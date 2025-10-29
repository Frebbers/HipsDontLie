# ---------- Build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Optional: pass environment at build time
ARG ENVIRONMENT=Development

# Copy project files first
COPY HipsDontLie.Server/HipsDontLie.Server.csproj HipsDontLie.Server/
COPY HipsDontLie.Client/HipsDontLie.Client.csproj HipsDontLie.Client/
COPY HipsDontLie.Shared/HipsDontLie.Shared.csproj HipsDontLie.Shared/
COPY HipsDontLie.Test/HipsDontLie.Test.csproj HipsDontLie.Test/

# Restore dependencies
RUN dotnet restore "HipsDontLie.Server/HipsDontLie.Server.csproj"

# Copy the rest of the source code
COPY . .

# Run tests before publish
RUN dotnet test "HipsDontLie.Test/HipsDontLie.Test.csproj" --verbosity normal

# Publish the Server (which automatically builds Client)
RUN dotnet publish "HipsDontLie.Server/HipsDontLie.Server.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

# ---------- Runtime stage ----------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Environment variable for ASP.NET
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

# Expose HTTP/HTTPS
EXPOSE 80
EXPOSE 443

# Copy published output from build stage
COPY --from=build /app/publish .

# Start the application
ENTRYPOINT ["dotnet", "HipsDontLie.Server.dll"]
