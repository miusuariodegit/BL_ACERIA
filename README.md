# GPB-Acería (BL_ACERIA)

Sistema de gestión y planificación de acería para **Siderúrgica Balboa** (grupo **CL Grupo Industrial**). Digitaliza la planificación de fusiones en horno, la configuración de tundish, el cálculo de necesidades de semielaborado (beam blank) y la generación de cortes, procesos que antes se gestionaban con hojas de cálculo y comunicación manual entre especialistas metalúrgicos.

Este documento describe el proyecto tal como está implementado en el repositorio: arquitectura, módulos, stack tecnológico, infraestructura y pipelines de CI/CD.

## 1. Resumen funcional

GPB-Acería (marca de producto "GPB", submódulo "Aceria") apoya al equipo de Maestros Horneros y especialistas metalúrgicos en:

- **Calendario de fusión de horno**: planificación de sesiones de fusión.
- **Configuración de tundish**: definición de configuraciones estándar y control de versiones de tundish.
- **Necesidad de Beam Blank**: cálculo de necesidad virtual de semielaborado por tren de colada (`sp_DameNecesidadVirtualBeamBlankTrenV2`).
- **Gestión de stock**: stock de beam blank y stock por colada.
- **Planificación de cortes y distribución de coladas**: propuestas de distribución y planificación de cortes.
- **Listado y control de versiones**: seguimiento de versiones de configuración de tundish por sociedad/centro.

La aplicación es multiempresa (parametrizada por `Sociedad` / `CentrosXsociedad`) y se integra con el resto de módulos corporativos "GPX" mediante un panel de control y un cargador de módulos (`ModuleLoader`).

## 2. Arquitectura y stack tecnológico

| Capa | Tecnología |
|---|---|
| Frontend/UI | Blazor Server (Interactive Server Components), .NET 10 |
| Componentes UI | DevExpress Blazor (`DevExpress.AIIntegration` 25.2.5) |
| Backend / lógica de negocio | .NET 10, proyecto `GPX.Negocio` (clases de servicio + Dapper para stored procedures) |
| Acceso a datos | Entity Framework Core 10 (identidad/autorización) + Dapper (consultas de negocio contra SQL Server) |
| Base de datos | SQL Server |
| Autenticación | ASP.NET Core Identity + inicio de sesión federado con Microsoft 365 (OpenID Connect) |
| Contenerización | Docker (imagen basada en `mcr.microsoft.com/dotnet/aspnet:10.0`) |
| Orquestación | Kubernetes (Amazon EKS) |
| Infraestructura como código | Terraform (`infraestructura/`) |
| CI/CD | GitHub Actions |
| Registro de imágenes | Amazon ECR |
| Análisis de código y seguridad | SonarQube, CodeQL, Trivy (escaneo de vulnerabilidades de imagen) |
| Pruebas | bUnit (pruebas de componentes Blazor) |

### Solución (`GPXGeneral.slnx`)

El repositorio es una solución .NET compuesta por dos proyectos principales:

- **`GPX.Negocio`** — capa de negocio y acceso a datos. Contiene:
  - `Aceria/` — servicios de dominio (`AceriaService`, `CalendarioFusionHornoService`, `ConfiguracionTundishService`) y sus modelos de transferencia (necesidades de beam blank, tundish disponibles, versiones, stock, etc.).
  - `ORM/` — entidades mapeadas a las tablas de negocio (Tundish, StockBeamBlank, CalendarioFusionHorno, ConfiguracionAceria, ControlColadasCargadas, RevaHornero, etc.).
  - `CRUD/` — repositorio genérico (`CrudRepository`).
  - `COP/` — registro de dependencias (`DependencyInjection.AddNegocio()`) y constantes compartidas.

- **`GPX.Web`** — aplicación Blazor Server (host web). Contiene:
  - `Components/Pages/` — páginas de nivel de aplicación (Panel de Control, Planificar, Fabricaciones, Calendario, Clientes).
  - `Components/VIEWS/Aceria/` — vistas específicas del módulo de acería (Calendario de Fusión, Configuración de Tundish, Listado de Versiones, Necesidad de Acería, Planificación de Cortes, control de Tundish reutilizable).
  - `Components/Account/` — páginas de gestión de cuenta e identidad.
  - `Data/` — `ApplicationDbContext`, entidades de identidad extendidas (`ApplicationUser`, `AppModule`, `AppProfile`, `AppProfileModule`), políticas de autorización dinámicas (`AppAuthorizationPolicyProvider`) y migraciones EF Core.
  - `Services/` — servicios transversales de la app (branding dinámico, gestión de accesos por módulo, carga dinámica de módulos, tema, tamaño de UI).
  - `Database/` — script de siembra (`gpx_identity_modules_seed.sql`) para módulos y perfiles de identidad.

La capa de negocio se registra en el contenedor de dependencias mediante `AddNegocio()` (`GPX.Negocio/COP/DependencyInjection.cs`), y se invoca desde `Program.cs` junto con `AddAppServices()` del lado web.

## 3. Autenticación y autorización

- Autenticación local con ASP.NET Core Identity (cookie `BL.SGPP.Auth`, expiración deslizante de 1 hora).
- Soporte opcional de inicio de sesión único con **Microsoft 365** vía OpenID Connect (PKCE), activable configurando `Authentication:Microsoft365` (TenantId, AppId, ClientSecret).
- Autorización basada en módulos y perfiles: `AppModule` / `AppProfile` / `AppProfileModule` definen qué módulos ve cada perfil de usuario; `AppAuthorizationPolicyProvider` genera políticas dinámicas a partir de esa configuración.
- Requisitos de contraseña: mínimo 8 caracteres, con mayúscula, minúscula y dígito (sin símbolo obligatorio).

## 4. Configuración

La configuración vive en `GPX.Web/appsettings.json` (y su variante `appsettings.Development.json`):

- `ConnectionStrings:DefaultConnection` — cadena de conexión a SQL Server.
- `Branding` — nombre de grupo/producto, logos y textos de la pantalla de acceso (multi-marca, parametrizable por subempresa).
- `ThemeSettings` — tema visual DevExpress (Fluent / color / modo claro-oscuro).
- `Authentication:Microsoft365` — credenciales de SSO corporativo.
- `pathbase` — permite desplegar la app bajo una ruta base (útil detrás de un ingress).

> **Nota de seguridad**: la cadena de conexión de ejemplo incluida en el repositorio contiene credenciales en texto plano. Se recomienda moverlas a un gestor de secretos (AWS Secrets Manager / variables de entorno inyectadas por el pipeline) antes de cualquier despliegue real y rotarlas si ya se han expuesto.

## 5. Base de datos

- Motor: SQL Server.
- El esquema de identidad y de negocio se administra con **migraciones de EF Core** (`GPX.Web/Data/Migrations`): esquema de identidad base, perfiles/módulos y jerarquía de navegación de módulos.
- `ApplicationDbInitializer.InitializeAsync` aplica las migraciones e inicializa datos al arrancar la aplicación.
- El script `Database/gpx_identity_modules_seed.sql` siembra los módulos y perfiles de acceso iniciales.
- Las consultas de negocio (planificación, tundish, stock, necesidades) se ejecutan contra **stored procedures** de SQL Server mediante Dapper (p. ej. `sp_DameNecesidadVirtualBeamBlankTrenV2`).

## 6. CI/CD (GitHub Actions)

Dos workflows en `.github/workflows/`:

### `miworkflow.yml` — Pipeline CI/CD (push a `main`)
1. **SonarQube**: análisis estático de calidad de código.
2. **CodeQL** (`security-extended`): análisis de seguridad del código C#.
3. Build de la solución con .NET 10 SDK (`dotnet restore` + `dotnet build`).
4. Ejecución de pruebas con **bUnit** (`dotnet test`).
5. Autenticación contra AWS y build/push de imagen Docker.

### `deployversion.yml` — Construcción y despliegue de una versión específica (push de tag `v*`)
1. CodeQL + build + pruebas (igual que el pipeline anterior).
2. Autenticación en AWS y login en **Amazon ECR**.
3. Build de la imagen Docker.
4. **Escaneo de vulnerabilidades con Trivy** (bloquea el pipeline ante severidad `CRITICAL`/`HIGH` no corregida).
5. Actualización de kubeconfig contra el clúster EKS (`eks-aceria-east1`, `us-east-1`).
6. Push de la imagen a ECR, etiquetada con el SHA del commit.

El versionado de despliegue se dispara con tags semánticos (`git tag -a vX.Y.Z` + `git push origin vX.Y.Z`).

## 7. Infraestructura (Terraform + Kubernetes)

- `infraestructura/` — Terraform (`main.tf`, `variables.tf`, `providers.tf`, `outputs.tf`) para aprovisionar el clúster EKS y red asociada en AWS (región `us-east-1`).
- `k8s/` — manifiestos de Kubernetes para desplegar la aplicación sobre el clúster, incluyendo el ingress controller NGINX.

Flujo típico de aprovisionamiento (ver `infraestructura/Readme.md` y `k8s/Readme.md`):

```bash
# 1. Inicializar y aplicar Terraform (clúster EKS)
terraform init
terraform apply -var="ssh_key_name=<key>" -var="ssh_cidr=<ip>/32"

# 2. Configurar acceso al clúster
aws eks update-kubeconfig --region us-east-1 --name eks-aceria-east1

# 3. Desplegar el ingress controller
kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/controller-v1.12.2/deploy/static/provider/aws/deploy.yaml

# 4. Aplicar los manifiestos de la aplicación
cd k8s
kubectl apply -f .

# 5. Disparar el despliegue de una versión (vía GitHub Actions)
git tag -a v1.0.14 -m "release versión 1.0.14"
git push origin v1.0.14
```

Para desmontar el entorno: `kubectl delete -f .`, eliminar el ingress controller y ejecutar `terraform destroy`.

> Recomendación: las IP en los ejemplos (`ssh_cidr`) son direcciones concretas de puesto de trabajo; deben sustituirse por la IP/rango real autorizado en cada despliegue y no reutilizarse tal cual.

## 8. Contenerización

`Dockerfile` (multi-stage):

1. **base** — runtime `aspnet:10.0`, crea el directorio de claves de protección de datos (`DataProtection-Keys`) y expone el puerto **8082**.
2. **build** — SDK `dotnet/sdk:10.0`, restaura paquetes con `Directory.Packages.props` y compila `GPX.Web.csproj`.
3. **publish** — `dotnet publish` en modo Release.
4. **final** — copia el resultado publicado sobre la imagen base y arranca con `dotnet GPX.Web.dll` (`ASPNETCORE_URLS=http://+:8082`).

## 9. Estructura del repositorio (resumen)

```
BL_ACERIA/
├── GPX.Negocio/            # Lógica de negocio y acceso a datos (Dapper + EF Core)
│   ├── Aceria/              # Servicios y modelos del dominio de Acería
│   ├── ORM/                 # Entidades de negocio
│   ├── CRUD/                # Repositorio genérico
│   └── COP/                 # Inyección de dependencias y constantes
├── GPX.Web/                 # Aplicación Blazor Server
│   ├── Components/Pages/    # Páginas generales de la plataforma
│   ├── Components/VIEWS/Aceria/  # Vistas del módulo de Acería
│   ├── Components/Account/  # Gestión de cuenta e identidad
│   ├── Data/                 # DbContext, identidad extendida, migraciones
│   ├── Services/              # Branding, accesos por módulo, tema, etc.
│   └── Database/               # Script de siembra
├── infraestructura/          # Terraform (EKS, red)
├── k8s/                       # Manifiestos Kubernetes
├── .github/workflows/         # Pipelines CI/CD
├── Dockerfile
├── Directory.Packages.props   # Versiones centralizadas de paquetes NuGet
└── GPXGeneral.slnx             # Solución .NET
```

## 10. Cómo ejecutar en local

```bash
# Restaurar y compilar
dotnet restore
dotnet build --configuration Release

# Configurar GPX.Web/appsettings.Development.json con una cadena de conexión válida
# a una instancia de SQL Server local o accesible

# Ejecutar la aplicación (aplica migraciones e inicializa datos al arrancar)
dotnet run --project GPX.Web
```

La aplicación queda disponible según el puerto configurado en `launchSettings`/`ASPNETCORE_URLS` (en contenedor, `8082`).

## 11. Pruebas

- **Pruebas unitarias/de componentes**: bUnit, ejecutadas en el pipeline CI con `dotnet test`.
- **Análisis estático**: SonarQube (calidad) y CodeQL (seguridad), integrados en cada push a `main` y en cada release.
- **Seguridad de contenedor**: Trivy, con corte del pipeline ante vulnerabilidades críticas o altas sin parche.

---

*Documento generado a partir del código fuente del repositorio `BL_ACERIA` como documentación técnica de acompañamiento al TFM "Sistema de Gestión y Planificación de Acería (GPB-Acería)".*
