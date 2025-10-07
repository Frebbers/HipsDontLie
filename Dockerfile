# ---------- Build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG ENVIRONMENT=Development
WORKDIR /src

# Copy project files
COPY HipsDontLie/HipsDontLie.csproj HipsDontLie/
COPY HipsDontLie.Blazor/HipsDontLie.Blazor.csproj HipsDontLie.Blazor/
COPY HipsDontLie.Shared/HipsDontLie.Shared.csproj HipsDontLie.Shared/

# Restore dependencies
RUN dotnet restore "HipsDontLie/HipsDontLie.csproj"

# Copy the rest of the source
COPY . .

# Publish the app (includes Blazor WASM)
RUN dotnet publish "HipsDontLie/HipsDontLie.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ---------- Runtime stage ----------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Expose HTTP/HTTPS
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80
EXPOSE 443

# Copy published output from build stage
COPY --from=build /app/publish .

# Start the app
ENTRYPOINT ["dotnet", "HipsDontLie.dll"]
