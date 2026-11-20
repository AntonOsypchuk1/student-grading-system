FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Packages.props", "./"]
COPY ["SGS.API/SGS.API.csproj", "SGS.API/"]
COPY ["SGS.Application/SGS.Application.csproj", "SGS.Application/"]
COPY ["SGS.DbContext/SGS.DbContext.csproj", "SGS.DbContext/"]
COPY ["SGS.Domain/SGS.Domain.csproj", "SGS.Domain/"]
COPY ["SGS.SemanticAnalysis.Client/SGS.SemanticAnalysis.Client.csproj", "SGS.SemanticAnalysis.Client/"]
RUN dotnet restore "SGS.API/SGS.API.csproj"
COPY . .
WORKDIR "/src/SGS.API"
RUN dotnet build "SGS.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "SGS.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SGS.API.dll"]
