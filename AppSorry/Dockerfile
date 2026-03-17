# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY AppSorry.csproj ./AppSorry/
RUN dotnet restore AppSorry/AppSorry.csproj

# Copy all source files and build
COPY AppSorry/ ./AppSorry/
WORKDIR /src/AppSorry
RUN dotnet build AppSorry.csproj -c Release -o /app/build

# Publish the application
RUN dotnet publish AppSorry.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Create uploads directory
RUN mkdir -p /app/wwwroot/uploads/images /app/wwwroot/uploads/music

COPY --from=build /app/publish .

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "AppSorry.dll"]
