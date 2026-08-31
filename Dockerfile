# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY *.sln ./
COPY Website_Documents.API/Website_Documents.API.csproj ./Website_Documents.API/
COPY Website_Documents.Service/Website_Documents.Service.csproj ./Website_Documents.Service/
COPY Website_Documents.Repository/Website_Documents.Repository.csproj ./Website_Documents.Repository/

# Restore dependencies
RUN dotnet restore

# Copy all source code
COPY . .

# Build với Production mode
RUN dotnet publish Website_Documents.API/Website_Documents.API.csproj -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Tạo thư mục uploads
RUN mkdir -p /app/wwwroot/uploads

# Copy published files
COPY --from=build /app/publish .

# Set environment
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "Website_Documents.API.dll"]
