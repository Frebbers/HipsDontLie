# Dockerfile for ASP.NET Core 9 application
# Use official .NET SDK to build
FROM mcr.microsoft.com/dotnet/sdk:9.0-preview AS build
WORKDIR /app

# Copy project files and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0-preview
WORKDIR /app
COPY --from=build /app/out .

# !!! IMPORTANT !!!
# Configure your application to use an external MySQL database
# Update your appsettings.json or use environment variables to provide:
# - DB_HOST (e.g., mysql-container-name or external-db-host)
# - DB_NAME
# - DB_USER
# - DB_PASSWORD

# Expose the port your application uses
EXPOSE 80

# Entry point for the application
ENTRYPOINT ["dotnet", "YourApplication.dll"]