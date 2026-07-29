FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base

WORKDIR /app

RUN mkdir -p /app/DataProtection-Keys \
    && chown -R $APP_UID:$APP_UID /app/DataProtection-Keys

USER $APP_UID

EXPOSE 8082


FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

ARG BUILD_CONFIGURATION=Release

WORKDIR /src

COPY ["Directory.Packages.props", "."]
COPY ["GPX.Web/GPX.Web.csproj", "GPX.Web/"]
COPY ["GPX.Negocio/GPX.Negocio.csproj", "GPX.Negocio/"]

RUN dotnet restore "GPX.Web/GPX.Web.csproj"

COPY . .

WORKDIR "/src/GPX.Web"

RUN dotnet build "GPX.Web.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/build


FROM build AS publish

ARG BUILD_CONFIGURATION=Release

RUN dotnet publish "./GPX.Web.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false


FROM base AS final

WORKDIR /app

COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:8082

ENTRYPOINT ["dotnet", "GPX.Web.dll"]