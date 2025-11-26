# Use the official ASP.NET Core runtime as a parent image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Use the SDK image for building the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore as distinct layers
# Copy all project files preserving directory structure
COPY ["CarDexBackend/CarDexBackend.Api/CarDexBackend.Api.csproj", "CarDexBackend/CarDexBackend.Api/"]
COPY ["CarDexBackend/CarDexBackend.Domain/CarDexBackend.Domain.csproj", "CarDexBackend/CarDexBackend.Domain/"]
COPY ["CarDexBackend/CarDexBackend.Services/CarDexBackend.Services.csproj", "CarDexBackend/CarDexBackend.Services/"]
COPY ["CarDexBackend/CarDexBackend.Repository/CarDexBackend.Repository.csproj", "CarDexBackend/CarDexBackend.Repository/"]
COPY ["CarDexBackend/Shared/CarDexBackend.Dtos/CarDexBackend.Dtos.csproj", "CarDexBackend/Shared/CarDexBackend.Dtos/"]
COPY ["CarDexDatabase/CarDexDatabase.csproj", "CarDexDatabase/"]

# Restore dependencies
RUN dotnet restore "CarDexBackend/CarDexBackend.Api/CarDexBackend.Api.csproj"

# Copy the remaining source code
COPY . .

# Build the application
WORKDIR "/src/CarDexBackend/CarDexBackend.Api"
RUN dotnet build "CarDexBackend.Api.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "CarDexBackend.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage/image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CarDexBackend.Api.dll"]
