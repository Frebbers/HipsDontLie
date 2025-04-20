# Dockerfile for ASP.NET Core 8 application
# Use official .NET SDK to build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG ENVIRONMENT=Development #Takes an environment name as argument. Defaults to dev
ENV PATH $PATH:/root/.dotnet/tools
RUN dotnet tool install -g dotnet-ef --version 8.0.5
# Set working directory for build stage
WORKDIR /src
EXPOSE 80
EXPOSE 443

# Copy solution file if exists
COPY GameTogether-Backend.sln .
COPY GameTogetherAPI/*.csproj ./GameTogetherAPI/
COPY GameTogetherAPI.Test/*.csproj ./GameTogetherAPI.Test/

# Restore NuGet packages
RUN dotnet restore "GameTogetherAPI/GameTogetherAPI.csproj"
RUN dotnet restore "GameTogetherAPI.Test/GameTogetherAPI.Test.csproj"


# Copy everything else
COPY . .

# Build application
WORKDIR /src/
RUN dotnet publish -c Release -o /app/publish

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

RUN apt-get update && \
    apt-get install -y curl && \
    rm -rf /var/lib/apt/lists/*

# Entry point for the application
ENTRYPOINT ["dotnet", "GameTogetherAPI.dll"]