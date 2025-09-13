# Base runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
# اگر APP_UID تعریف نشده، این خط را فعلا حذف کن
# USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Memory-Cache.csproj", "./"]
RUN dotnet restore "Memory-Cache.csproj"
COPY . .
RUN dotnet build "Memory-Cache.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Memory-Cache.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Memory-Cache.dll"]
