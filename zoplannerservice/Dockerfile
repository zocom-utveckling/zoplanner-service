# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies (cached separately for better layer caching)
COPY zoplannerservice/zoplannerservice.csproj ./zoplannerservice/
RUN dotnet restore zoplannerservice/zoplannerservice.csproj

# Copy all code and build
COPY . .
RUN dotnet publish zoplannerservice/zoplannerservice.csproj -c Release -o /app/publish

# Runtime stage (lightweight runtime image)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Environment variables and port
ENV ASPNETCORE_URLS=http://+:5027
EXPOSE 5027

# Run the application
ENTRYPOINT ["dotnet", "zoplannerservice.dll"]