#FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
#USER $APP_UID
#WORKDIR /app
#
#
#FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
#ARG BUILD_CONFIGURATION=Release
#WORKDIR /src
#COPY ["RedLock/RedLock/RedLock.csproj", "RedLock/RedLock/"]
#RUN dotnet restore "./RedLock/RedLock/RedLock.csproj"
#COPY . .
#WORKDIR "/src/RedLock/RedLock"
#RUN dotnet build "./RedLock.csproj" -c $BUILD_CONFIGURATION -o /app/build
#
#FROM build AS publish
#ARG BUILD_CONFIGURATION=Release
#RUN dotnet publish "./RedLock.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false
#
#FROM base AS final
#WORKDIR /app
#COPY --from=publish /app/publish .
#ENTRYPOINT ["dotnet", "RedLock.dll"]




FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["RedLock.csproj", "./"]
RUN dotnet restore "RedLock.csproj"

COPY . .
RUN dotnet build "RedLock.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "RedLock.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish ./
ENTRYPOINT ["dotnet", "RedLock.dll"]