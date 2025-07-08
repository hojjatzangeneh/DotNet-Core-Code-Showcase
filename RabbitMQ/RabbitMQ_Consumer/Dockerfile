# Base image for running the application in the final container
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

# Build stage: includes restore and build steps
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy only the project file to restore dependencies early
COPY ["RabbitMQ_Consumer.csproj", "./"]

# Restore NuGet packages
RUN dotnet restore "./RabbitMQ_Consumer.csproj"

# Copy the rest of the source code
COPY . .

# Set the working directory to project root
WORKDIR "/src"

# Build the application
RUN dotnet build "RabbitMQ_Consumer.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish stage: prepares the app for production
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "RabbitMQ_Consumer.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final image: minimal runtime-only container
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RabbitMQ_Consumer.dll"]
