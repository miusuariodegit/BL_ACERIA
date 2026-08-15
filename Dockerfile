# [GPX-DOC-v1] ================================================================================
# Imagen Docker multi-stage de GPB-Acería (GPX.Web).
# Etapas: base (runtime) -> build (compilacion) -> publish (publicacion) -> final (imagen de ejecucion).
# ================================================================================================

# --- Etapa "base": imagen de runtime ASP.NET Core 10, minima, usada como base de la imagen final ---
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base

WORKDIR /app

# Crea el directorio de claves de proteccion de datos (Data Protection API) y lo asigna al usuario
# no root de la imagen, requerido para que la app pueda cifrar/descifrar cookies y tokens.
RUN mkdir -p /app/DataProtection-Keys \
    && chown -R $APP_UID:$APP_UID /app/DataProtection-Keys

# Ejecuta el contenedor con un usuario no root (buena practica de seguridad).
USER $APP_UID

# Puerto interno en el que escucha Kestrel (ver ENV ASPNETCORE_URLS mas abajo y k8s/deployment.yaml).
EXPOSE 8082


# --- Etapa "build": SDK completo de .NET 10, usada solo para compilar (no se publica) ---
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

ARG BUILD_CONFIGURATION=Release

WORKDIR /src

# Copia primero los archivos de proyecto (.csproj) y Directory.Packages.props para aprovechar el
# cache de capas de Docker: "dotnet restore" solo se repite si cambian las dependencias, no el codigo.
COPY ["Directory.Packages.props", "."]
COPY ["GPX.Web/GPX.Web.csproj", "GPX.Web/"]
COPY ["GPX.Negocio/GPX.Negocio.csproj", "GPX.Negocio/"]

RUN dotnet restore "GPX.Web/GPX.Web.csproj"

# Copia el resto del codigo fuente una vez restauradas las dependencias.
COPY . .

WORKDIR "/src/GPX.Web"

RUN dotnet build "GPX.Web.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/build


# --- Etapa "publish": genera el artefacto de publicacion (self-contained=false) a partir del build ---
FROM build AS publish

ARG BUILD_CONFIGURATION=Release

RUN dotnet publish "./GPX.Web.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false


# --- Etapa "final": imagen de ejecucion. Combina el runtime minimo (base) con el artefacto publicado ---
FROM base AS final

WORKDIR /app

# Copia unicamente los binarios publicados (no el SDK ni el codigo fuente), reduciendo el tamano final.
COPY --from=publish /app/publish .

# Kestrel escucha en el puerto 8082 dentro del contenedor (coincide con EXPOSE y con el Service de k8s).
ENV ASPNETCORE_URLS=http://+:8082

# Punto de entrada del contenedor.
ENTRYPOINT ["dotnet", "GPX.Web.dll"]
