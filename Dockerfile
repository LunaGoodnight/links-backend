# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY LinksService/LinksService.csproj ./LinksService/
RUN dotnet restore ./LinksService/LinksService.csproj

# Copy everything else and build
COPY . .
RUN dotnet publish ./LinksService/LinksService.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

# Install EF Core tools for migrations
ENV PATH="$PATH:/root/.dotnet/tools"

# Reduce runtime memory on VPS
ENV DOTNET_gcServer=0
ENV DOTNET_GCConserveMemory=9

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "LinksService.dll"]
