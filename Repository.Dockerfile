# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY . ./

# Copy everything else and build
WORKDIR /app/Demo365.Repository
RUN dotnet publish -c Release -o out Demo365.Repository.csproj

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:3.1
WORKDIR /app
COPY --from=build-env /app/Demo365.Repository/out .
ENTRYPOINT ["dotnet", "Demo365.Repository.dll"]