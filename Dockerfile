# Dockerfile for ASP.NET Core 9 application
# Use official .NET SDK to build
FROM mcr.microsoft.com/dotnet/sdk:9.0-preview AS build

# Set working directory for build stage
WORKDIR /src

# Copy solution file if exists (replace *.sln with actual solution name if needed)
COPY *.sln .
COPY GameTogetherAPI/*.csproj ./GameTogetherAPI/
COPY GameTogetherAPI/*.*proj ./GameTogetherAPI/

# Restore NuGet packages
RUN dotnet restore "GameTogetherAPI/GameTogetherAPI.csproj" --disable-parallel

# Copy everything else
COPY . .

# Build application
WORKDIR /src/GameTogetherAPI
RUN dotnet publish -c Release -o /app/publish

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0-preview AS final
WORKDIR /app
COPY --from=build /app/publish .

# Entry point for the application
ENTRYPOINT ["dotnet", "GameTogetherAPI.dll"]