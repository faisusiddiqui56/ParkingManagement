# Multi-stage Dockerfile for a .NET 10 Razor Pages app
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and all files
COPY ["ParkingManagement.slnx", "./"]
COPY . .

# Restore NuGet packages
RUN dotnet restore "ParkingManagement.slnx"

# Publish the app (Release)
RUN dotnet publish "ParkingManagement.slnx" -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Expose default HTTP port
EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80

# Copy published output from build stage
COPY --from=build /app/publish ./

# Set the entrypoint (assumes the project assembly is ParkingManagement.dll)
ENTRYPOINT ["dotnet", "ParkingManagement.dll"]
