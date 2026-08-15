# Documentación archivo por archivo — GPB-Acería (BL_ACERIA)

Complementa a `README.md`. Cubre cada archivo de código fuente (`.cs`, `.razor`) y los archivos de configuración/infraestructura relevantes del repositorio. Se excluyen del detalle individual los recursos estáticos sin lógica (iconos `.svg`, imágenes, hojas `.razor.css` de estilo puramente visual) porque no aportan comportamiento a documentar; se mencionan de forma agregada en la sección 6.

---

## 1. GPX.Negocio — Capa de negocio y acceso a datos

### 1.1 `Aceria/` — Dominio de planificación de acería

| Archivo | Contenido |
|---|---|
| `AceriaService.cs` | Servicio Dapper que consulta stored procedures de negocio contra SQL Server. Expone `DameNecesidadBeamBlankTrenV2Async` (necesidad virtual de beam blank por sociedad/máquina, vía `sp_DameNecesidadVirtualBeamBlankTrenV2`) y `ConsultaTundishDisponiblesAsync` (tundish disponibles según horas requeridas, fecha y tipo de semielaborado). |
| `BeamBlankNecesidad.cs` | DTO con la necesidad de beam blank por orden de fabricación: sociedad, máquina, material, calidad, longitud, toneladas/unidades a fabricar y fechas/semana de previsión. |
| `CalendarioFusionHornoService.cs` | Servicio de negocio del calendario de fusión de horno. Consulta el calendario hasta una fecha, actualiza un día completo o una hora concreta, y permite limpiar un día o limpiar a partir de una hora de inicio. |
| `ConfiguracionTundishService.cs` | Servicio de configuración de tundish: activa una versión concreta (`ActivaVersionTundish`) y consulta versiones dentro de un rango de fechas (`CansultaVercionXRango`). |
| `DetalleVersionVm.cs` | Modelo de vista del detalle de una versión de tundish: tipo de semielaborado, número de barras, longitud, calidad, fecha/semana prevista y GAP (desviación). |
| `ListDetalleNecesidadBB.cs` | Modelo de vista del detalle de necesidad de beam blank por orden y calidad: toneladas de necesidad, coladas necesarias vs. reales. |
| `ListTundishDisponibles.cs` | Modelo de vista de un tundish disponible: horario, vida útil, tipo de semielaborado, estándar seleccionado, totales por tipo de BB (1/2/3) y estado de cada boca (hasta 6). |
| `ListTundishStandard.cs` | Extiende `ORM.TundishStandard` para su uso en listados/vistas. |
| `ListaCalidadesXColada.cs` | Modelo de vista de calidades por colada: calidad, coladas requeridas/repartidas y si es calidad estándar. |
| `ListaCargaMasivaCalendario.cs` | Par fecha/valor usado en la carga masiva del calendario de fusión (p. ej. import desde archivo). |
| `ListaGestionStockSemi.cs` | Extiende `GestionStockSemi` para listados de gestión de stock de semielaborado. |
| `ListaPropuestaDistribucion.cs` | Modelo de vista de propuesta de distribución de tundish: fechas, número de coladas, vida útil y desglose por calidad (S275H, S355TI, S355V, S355W, S460A, S275TI). |
| `ListaStockBeamBlank.cs` | Extiende `StockBeamBlank` agregando número de cortes y merma. |
| `ListadoVersionVm.cs` | Modelo de vista para el listado de versiones de tundish: estado, fechas de creación/modificación, autor, número de coladas/barras, calidades y filtros disponibles. |
| `VersionTundishSeleccionadoState.cs` | Contenedor de estado (state container, inyectado como scoped) que mantiene la versión de tundish actualmente seleccionada en la UI. |

### 1.2 `ORM/` — Entidades de negocio (mapeadas a tablas SQL Server)

| Archivo | Contenido |
|---|---|
| `CalendarioFusionHorno.cs` | Entidad del calendario de fusión: identificador, sociedad, centro, fecha y hasta 16 franjas horarias (`cafHora1`…`cafHora16`). |
| `CentrosXsociedad.cs` | Catálogo de centros productivos por sociedad, con códigos SAP/GESAC, rendimiento y merma. |
| `ConfiguracionAceria.cs` | Parámetros de configuración de una acería por sociedad/máquina: toneladas por cuchara, tiempos mínimo/máximo de colada, tiempo de horno-cuchara (LF), máximo de perfiles, vida útil de tundish, peso lineal y velocidad máxima por tipo de BB, límites de calidades estándar y minutos de cambio de tundish. |
| `ConfiguracionTundishControl.cs` | Cabecera de una versión de configuración de tundish: autor, fechas de creación/modificación, necesidad por tipo de BB y total, estándar seleccionado, y listas de configuración/necesidades/tundish/distribución asociadas. |
| `ControlColadasCargadas.cs` | Registro de coladas cargadas: fabricación, colada, calidad/tipo/longitud del semielaborado, unidades cargadas, fecha y usuario que realizó la carga. |
| `GestionStockSemi.cs` | Movimiento de stock de semielaborado: tipo de operación, toneladas y unidades brutas/stock/asignadas/libres, familia, longitud, peso y usuario. |
| `OrdenCalidadPlanificacion.cs` | Orden de prioridad visual de una calidad en la planificación (color, color de fuente, orden). |
| `RevaHornero.cs` | Registro de "revisión hornero": fabricación, fecha/semana prevista de fin, orden de mezcla, familia, ciclo, sociedad, máquina, estatus, material, calidad y longitud del semielaborado. |
| `StockBBColada.cs` | Stock de beam blank por colada: sociedad, tipo, calidad, longitud, colada, stock en unidades/toneladas, ubicación y unidades asignadas. |
| `StockBeamBlank.cs` | Stock agregado de beam blank: teórico, real, asignado y libre, en toneladas y unidades, más peso del semielaborado. |
| `Tundish.cs` | Entidad tundish: sociedad, necesidad por tipo de BB, si está activo, tipo estándar y hora real de cierre de hasta 6 bocas. |
| `TundishStandard.cs` | Estándar de tundish: horas de cierre por boca (1-6), si está activo, prioridad y totales por tipo de BB. |

### 1.3 `CRUD/` — Repositorio genérico

| Archivo | Contenido |
|---|---|
| `CrudRepository.cs` | Repositorio central (≈870 líneas) con operaciones de alta/consulta/actualización/eliminación (vía Dapper) para las entidades principales del dominio: `GestionStockSemi`, `OrdenCalidadPlanificacion`, `CalendarioFusionHorno`, `TundishStandard`, `Tundish`, `ConfiguracionAceria`, `ControlColadasCargadas`, `ConfiguracionTundishControl` y `CentrosXsociedad`. Es el punto único de acceso a datos CRUD para la mayoría de vistas. |

### 1.4 `COP/` — Configuración de la capa de negocio

| Archivo | Contenido |
|---|---|
| `Constantes.cs` | Constantes compartidas por la capa de negocio. |
| `DependencyInjection.cs` | Método de extensión `AddNegocio()` que registra en el contenedor de DI los servicios de negocio (`CrudRepository`, `AceriaService`, `CalendarioFusionHornoService`, `ConfiguracionTundishService`) y el state container `VersionTundishSeleccionadoState`. Se invoca desde `Program.cs`. |

### 1.5 `GPX.Negocio.csproj`

Proyecto de clase .NET 10 con `Nullable`/`ImplicitUsings` habilitados. Dependencias: `Dapper`, `Microsoft.Data.SqlClient` y `Microsoft.Extensions.Configuration.Abstractions`. No referencia ASP.NET Core: es una capa de negocio agnóstica de la web, reutilizable desde otros hosts.

---

## 2. GPX.Web — Aplicación Blazor Server

### 2.1 Arranque

| Archivo | Contenido |
|---|---|
| `Program.cs` | Punto de entrada. Configura Razor Components interactivos (Server), DevExpress Blazor, la capa de negocio (`AddNegocio`), autenticación por cookie (`BL.SGPP.Auth`, 1h deslizante), ASP.NET Core Identity con política de contraseñas, SSO opcional con Microsoft 365 (OpenID Connect + PKCE) si hay credenciales configuradas, `ApplicationDbContext` sobre SQL Server con reintentos, autorización dinámica por políticas (`AppAuthorizationPolicyProvider`), `pathbase` configurable y el pipeline HTTP estándar (HTTPS, HSTS, autenticación/autorización, endpoints de Identity). |
| `ThemeInfo.cs` | Modelo simple con el estado de tema oscuro/claro persistido entre renders. |
| `UserInfo.cs` | Modelo simple con el perfil del usuario autenticado, usado para estado de autenticación persistente. |

### 2.2 `Data/` — Identidad, autorización y contexto de datos

| Archivo | Contenido |
|---|---|
| `ApplicationDbContext.cs` | `DbContext` de EF Core que extiende `IdentityDbContext` con las entidades `ApplicationUser`, `AppProfile`, `AppModule`, `AppProfileModule`. |
| `ApplicationDbInitializer.cs` | Aplica migraciones pendientes e inicializa datos base al arrancar la aplicación (invocado desde `Program.cs`). |
| `ApplicationUser.cs` | Extiende `IdentityUser` con `FullName` y relación a `AppProfile` (perfil de acceso). |
| `ApplicationUserClaimsPrincipalFactory.cs` | Genera los claims del usuario autenticado (incluye utilidades para añadir/eliminar claims de un `ClaimsIdentity`). |
| `AppAuthorizationPolicyProvider.cs` | Proveedor de políticas de autorización dinámicas: genera políticas de módulo/permiso "sobre la marcha" a partir de la configuración de perfiles y módulos, sin necesidad de registrarlas una a una. |
| `AppClaimTypes.cs` | Constantes con los nombres de los tipos de claim propios de la aplicación. |
| `AppModule.cs` | Entidad de módulo de navegación/funcionalidad: código, nombre, ruta, icono, módulo padre (para menús jerárquicos), orden de visualización y si está habilitado. |
| `AppPolicies.cs` | Helpers estáticos para construir nombres de política por módulo (`Module(code)`) o por permiso (`Permission(code)`). |
| `AppProfile.cs` | Entidad de perfil de usuario (rol funcional): nombre, descripción, usuarios y módulos asociados. |
| `AppProfileModule.cs` | Tabla puente entre `AppProfile` y `AppModule` (qué módulos ve cada perfil). |
| `ModuleNavigationModels.cs` | Records `ModuleDefinition` y `ModuleGroupDefinition` usados para construir el menú de navegación agrupado. |
| `Migrations/00000000000000_CreateIdentitySchema.cs` (+ `.Designer.cs`) | Migración inicial de EF Core: crea el esquema estándar de ASP.NET Core Identity (usuarios, roles, claims, logins, tokens). |
| `Migrations/20260330000100_AddProfilesAndModules.cs` | Migración que añade las tablas `AppProfiles`, `AppModules` y `AppProfileModules`. |
| `Migrations/20260331000100_AddModuleNavigationParents.cs` | Migración que añade la jerarquía padre/hijo a `AppModules` para el menú de navegación. |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Snapshot autogenerado por EF Core del modelo de datos actual (no se edita a mano). |

### 2.3 `Services/` — Servicios transversales de la aplicación web

| Archivo | Contenido |
|---|---|
| `BrandingService.cs` | Resuelve el branding dinámico (logos, nombre de grupo/producto, texto de bienvenida, fondo de login) a partir de `Branding` en `appsettings.json`; soporta multi-marca por subempresa. |
| `GestionManagerModels.cs` | Records/DTOs usados por el módulo de administración: resumen del dashboard, opciones de módulo, usuarios, roles, resultado de operación (`Success`/`Failure`). |
| `GestionManagerService.cs` | Servicio central (≈575 líneas) del módulo de administración de usuarios/roles/perfiles: obtiene resumen del dashboard, listas de usuarios/roles/perfiles, construye y guarda editores de usuario/rol/perfil y de módulos por perfil, y sincroniza claims de permisos. |
| `ModuleAccessService.cs` | Resuelve qué módulos y grupos de módulos puede ver un usuario autenticado según su perfil, y expone su nombre de perfil/nombre para mostrar. |
| `ModuleLoader.cs` | Carga módulos de JavaScript de forma segura desde Blazor Server (`IAsyncDisposable`). |
| `SizeModeManager.cs` | Gestiona el modo de tamaño de la interfaz (compacto/normal) y su clase CSS asociada. |
| `ThemeManager.cs` | Gestiona el tema claro/oscuro de la interfaz y su persistencia. |

### 2.4 `Utils/`

| Archivo | Contenido |
|---|---|
| `KeyValuePairSerializer.cs` | Estructura simple clave/valor usada para (de)serializar preferencias (tema, tamaño de UI) en almacenamiento persistente. |
| `ServiceExtensions.cs` | Método de extensión `AddAppServices()` que registra los servicios propios de `GPX.Web` (branding, temas, gestión de accesos, etc.) en el contenedor de DI. |

### 2.5 `Components/Account/` — Identidad (scaffolding ASP.NET Core Identity)

Conjunto estándar de páginas y clases de soporte generadas por el scaffolding de Identity de ASP.NET Core, adaptadas al branding y al flujo de la aplicación. Se documentan de forma agrupada por ser funcionalidad conocida de la plataforma:

**Soporte (`Components/Account/*.cs`)**

| Archivo | Contenido |
|---|---|
| `CookieEvents.cs` | Extiende `CookieAuthenticationEvents` para redirigir correctamente al login cuando expira la sesión en escenarios Blazor Server. |
| `IdentityComponentsEndpointRouteBuilderExtensions.cs` | Registra los endpoints adicionales de Identity (login/logout/gestión) que no son componentes Razor. |
| `IdentityNoOpEmailSender.cs` | Implementación "no-op" de `IEmailSender` (no envía correos reales; usada mientras no hay proveedor de email configurado). |
| `IdentityRedirectManager.cs` | Helper para redirecciones tras operaciones de Identity, con o sin mensaje de estado. |
| `IdentityUserAccessor.cs` | Obtiene el usuario autenticado actual o lanza si no existe. |
| `PersistingServerAuthenticationStateProvider.cs` | Proveedor de estado de autenticación que persiste el estado entre el render estático y el interactivo de Blazor Server. |

**Páginas (`Components/Account/Pages/*.razor`)**: `Login`, `Register`, `RegisterConfirmation`, `ExternalLogin` (incluye botón de Microsoft 365), `ForgotPassword` (+ confirmación), `ResetPassword` (+ confirmación), `ResendEmailConfirmation`, `ConfirmEmail`, `ConfirmEmailChange`, `LoginWith2fa`, `LoginWithRecoveryCode`, `Lockout`, `AccessDenied`, `InvalidUser`, `InvalidPasswordReset` — cubren el flujo completo de autenticación local y externa.

**Gestión de cuenta (`Components/Account/Pages/Manage/*.razor`)**: `Index` (perfil), `Email`, `ChangePassword`, `SetPassword`, `TwoFactorAuthentication`, `EnableAuthenticator`, `Disable2fa`, `ResetAuthenticator`, `GenerateRecoveryCodes`, `ExternalLogins`, `PersonalData`, `DeletePersonalData` — autogestión de cuenta por parte del usuario.

**Compartidos (`Components/Account/Shared/*.razor`)**: `AccountLayout`, `ManageLayout`, `ManageNavMenu`, `ExternalLoginPicker`, `ShowRecoveryCodes`, `StatusMessage`, `InteractiveServerCheckbox`, `InteractiveServerMaskedInput`, `InteractiveServerTextBox` — layouts y controles reutilizables adaptados a la renderización interactiva de Blazor Server.

### 2.6 `Components/GestionManager/` — Administración de usuarios, roles y perfiles

| Archivo | Ruta | Contenido |
|---|---|---|
| `Resumen.razor` | `/gestion` | Panel resumen del módulo de administración (dashboard). |
| `Usuarios.razor` | `/gestion/usuarios` | Alta, edición y listado de usuarios. |
| `Roles.razor` | `/gestion/roles` | Alta, edición y listado de roles. |
| `UsuariosPorRol.razor` | `/gestion/usuarios-por-rol` | Consulta de usuarios agrupados por rol. |
| `Perfiles.razor` | `/gestion/perfiles` | Alta, edición y listado de perfiles funcionales (`AppProfile`). |
| `PerfilesModulos.razor` | `/gestion/perfiles-modulos` | Asignación de módulos visibles a cada perfil. |
| `Claims.razor` | `/gestion/claims` | Consulta de claims de permisos sincronizados. |

Todas estas vistas se apoyan en `GestionManagerService` para su lógica.

### 2.7 `Components/Layout/`

| Archivo | Contenido |
|---|---|
| `MainLayout.razor` | Layout principal de la aplicación autenticada (≈326 líneas): navegación por módulos, tema, modo de tamaño y branding dinámico. |
| `AccountManager.razor` | Menú/panel de cuenta de usuario en el layout principal. |
| `UserAvatar.razor` | Avatar del usuario autenticado. |

### 2.8 `Components/Pages/` — Páginas generales de la plataforma

| Archivo | Ruta | Contenido |
|---|---|---|
| `PanelControl.razor` | `/modulos/panel-control` | Página contenedora del panel de control (carga el módulo correspondiente). |
| `Planificar.razor` | `/modulos/planificar` | Página contenedora del módulo de planificación. |
| `Fabricaciones.razor` | `/modulos/fabricaciones` | Página contenedora del módulo de fabricaciones. |
| `CalendarioPro.razor` | `/modulos/calendario-pro` | Página contenedora del calendario "Pro" (agenda general, distinta del calendario de fusión de Acería). |
| `Clientes.razor` | `/modulos/clientes` | Página contenedora del módulo de clientes. |
| `Error.razor` | `/Error` | Página de error genérica de la aplicación. |

### 2.9 `Components/VIEWS/Aceria/` — Módulo funcional de Acería (núcleo del proyecto)

| Archivo | Ruta | Contenido |
|---|---|---|
| `CalendarioFusion.razor` + `.razor.cs` | `/VIEWS/Aceria/CalendarioFusion` | Vista de calendario de fusión de horno. El code-behind (≈424 líneas) carga catálogos, consulta y edita el calendario, valida cada registro, permite carga masiva desde archivo, ofrece exportación a Excel/CSV/PDF y gestiona el estado de "hora activa"/tipo de semielaborado por celda. |
| `ConfiguracionTundish.razor` + `.razor.cs` | `/VIEWS/Aceria/ConfiguracionTundish` | Vista más extensa del módulo (code-behind ≈954 líneas). Permite calcular la necesidad de tundish para un rango de fechas, cargar/aplicar estándares, calcular el detalle de necesidad y la propuesta de distribución de coladas por calidad, editar manualmente la distribución con validación, y guardar una nueva versión serializada de configuración. |
| `ListadoDeVersiones.razor` + `.razor.cs` | `/VIEWS/Aceria/ListadoDeVersiones` | Listado de versiones de configuración de tundish generadas: consulta, activación de una versión y generación de la versión de corte a partir de ella. |
| `NecesidadAceria.razor` + `.razor.cs` | `/VIEWS/Aceria/NecesidadAceria` | Vista de necesidades de acería (beam blank): carga catálogos, consulta datos, convierte semana a orden cronológico, muestra el detalle por orden y exporta a Excel/CSV/PDF. |
| `PlanificacionCortes.razor` + `.razor.cs` | `/VIEWS/Aceria/PlanificacionCortes` | Vista más compleja del módulo (code-behind ≈1487 líneas). Construye el árbol de tundish/coladas, gestiona el ciclo de vida de una colada (asignación, finalización, cancelación), calcula capacidad por línea, genera asignaciones automáticas y permite ajustes manuales, calcula resúmenes comparativos y controla el estado de cada "boca" del tundish. |
| `UcTundish.razor` + `.razor.cs` | *(componente reutilizable, sin ruta propia)* | Control de usuario que representa un tundish individual: estándar aplicado, configuración de bocas, cálculo de rendimiento y notificación de cambios al componente padre (`TundishChanged`). Reutilizado por las vistas de configuración y planificación. |
| `dashboard_tundish_versions_coladas_semana_buena_con_resumen.html` | — | Prototipo/mockup HTML estático de un dashboard de tundish, versiones y coladas por semana, usado como referencia visual durante el diseño. |

### 2.10 Composición de la aplicación

| Archivo | Contenido |
|---|---|
| `Components/App.razor` | Documento raíz de la aplicación Blazor: `<head>`/`<body>`, carga de tema y modo de tamaño. |
| `Components/Routes.razor` | Configura el `Router` de Blazor y el layout por defecto. |
| `Components/Index.razor` | Ruta `/`; redirige según el estado de autenticación. |
| `Components/RedirectToPage.razor` | Componente utilitario para redirección programática a otra ruta. |
| `Components/ModuleAccessSummary.razor` | Resumen de los módulos accesibles para el usuario actual (basado en `ModuleAccessService`). |
| `Components/_Imports.razor` | `@using` globales compartidos por todos los componentes Razor de `GPX.Web`. |

### 2.11 `GPX.Web.csproj`

Proyecto web SDK `Microsoft.NET.Sdk.Web`, .NET 10, `AnyCPU`/`x64`, target Docker Linux. Gestión centralizada de versiones de paquete (`ManagePackageVersionsCentrally`) contra `Directory.Packages.props`. Referencias: DevExpress Blazor/AIIntegration/Drawing.Skia, ASP.NET Core Identity + EF Core (SqlServer), autenticación OpenID Connect y MSAL. Compila explícitamente todo `.cs` del proyecto y publica `Components/**/*.razor`, `Components/**/*.css`, `wwwroot/**/*` y los `*.json` de configuración.

---

## 3. Infraestructura como código

### 3.1 `infraestructura/` — Terraform (Amazon EKS)

| Archivo | Contenido |
|---|---|
| `providers.tf` | Declara los proveedores Terraform: `aws` (~> 5.0), `tls` (~> 4.0) y `dns` (~> 3.0). Región AWS parametrizada por `var.region`. |
| `variables.tf` | Variables de entrada: región (`us-east-1` por defecto), nombre del clúster (`eks-aceria-east1`), versión de Kubernetes, tipo de instancia de los nodos, número deseado/mínimo/máximo de nodos, nombre de la clave SSH y CIDR permitido para SSH. |
| `main.tf` | Recurso principal: crea una VPC dedicada con subredes públicas en dos zonas de disponibilidad, internet gateway y tabla de rutas; roles IAM para el clúster y los nodos EKS; grupos de seguridad para la API del clúster y para SSH a los nodos; el clúster EKS y su node group gestionado; el proveedor OIDC para IRSA; el rol IAM y el addon del driver CSI de EBS (para volúmenes persistentes); y un `aws_key_pair` para acceso SSH. |
| `outputs.tf` | Expone como salidas el nombre, endpoint, versión y certificado del clúster, el ARN del rol del CSI de EBS, y las IPs públicas/privadas de los nodos y de la API del clúster. |
| `Readme.md` | Instrucciones operativas: `terraform init`/`apply`, actualización del kubeconfig, despliegue del ingress controller NGINX, disparo de despliegue vía tag de Git y `terraform destroy` para desmontar el entorno. |

### 3.2 `k8s/` — Manifiestos de Kubernetes

| Archivo | Contenido |
|---|---|
| `deployment.yaml` | Define un `Deployment` (2 réplicas) que ejecuta la imagen de la aplicación publicada en Amazon ECR, exponiendo el puerto 8082, y un `Service` de tipo `LoadBalancer` que enruta el tráfico externo al mismo puerto. |
| `Readme.md` | Instrucciones para aprovisionar el clúster con Terraform, configurar el acceso, aplicar los manifiestos (`kubectl apply -f .`) y desmontar el entorno. |

### 3.3 CI/CD (`.github/workflows/`)

| Archivo | Contenido |
|---|---|
| `miworkflow.yml` | Pipeline de integración continua disparado en cada push a `main`: análisis SonarQube, análisis de seguridad CodeQL, build y pruebas (bUnit) con .NET 10, autenticación AWS y build/push de imagen Docker. |
| `deployversion.yml` | Pipeline de construcción y despliegue de una versión concreta, disparado por push de un tag `v*`: CodeQL, build, pruebas, login en Amazon ECR, build de imagen, **escaneo de vulnerabilidades con Trivy** (bloqueante en severidad crítica/alta), actualización de kubeconfig contra EKS y push de la imagen etiquetada con el SHA del commit a ECR. |

### 3.4 Contenerización y build

| Archivo | Contenido |
|---|---|
| `Dockerfile` | Build multi-stage: `base` (runtime `aspnet:10.0`, expone el puerto 8082 y prepara el directorio de claves de protección de datos), `build`/`publish` (SDK `dotnet/sdk:10.0`, restaura, compila y publica `GPX.Web.csproj`), `final` (copia el publish sobre la imagen base y arranca con `dotnet GPX.Web.dll`). |
| `.dockerignore` | Excluye del contexto de build artefactos de IDE, `bin`/`obj`, `.git`, `node_modules`, ficheros de secretos de desarrollo (`.env`, `secrets.dev.yaml`) y el propio `Dockerfile`/`README.md`. |
| `Directory.Packages.props` | Gestión centralizada de versiones de paquetes NuGet para toda la solución (DevExpress 25.2.5, EF Core/Identity/Authentication 10.0.5, MSAL 4.83.1, IdentityModel 8.17.0). |
| `GPXGeneral.slnx` | Definición de la solución .NET (formato `.slnx`) que agrupa los proyectos `GPX.Web` y `GPX.Negocio`. |

### 3.5 Base de datos

| Archivo | Contenido |
|---|---|
| `GPX.Web/Database/gpx_identity_modules_seed.sql` | Script T-SQL idempotente (dentro de una transacción) que recrea el esquema de Identity y las tablas propias (`AppProfiles`, `AppModules`, `AppProfileModules`) y siembra los datos mínimos de arranque: perfil, módulo, rol, usuario y claim iniciales, junto con el árbol de navegación padre/hijo de módulos. |
| `GPX.Web/Data/Migrations/*.cs` | Ver sección 2.2 — migraciones EF Core equivalentes, usadas por `ApplicationDbInitializer` en cada arranque. |

### 3.6 Calidad y análisis

| Archivo | Contenido |
|---|---|
| `sonar-project.properties` | Configuración del proyecto SonarQube: clave/nombre de proyecto `GPX.Web` y rutas de código fuente analizadas (`GPX.Web`, `GPX.Negocio`). |
| `scriptsonar.sh` | Script de aprovisionamiento de un servidor SonarQube self-hosted (ajustes de kernel/límites, Java 21, PostgreSQL) — infraestructura de soporte para el análisis estático, no forma parte del runtime de la aplicación. |

### 3.7 Configuración de la aplicación

| Archivo | Contenido |
|---|---|
| `GPX.Web/appsettings.json` | Configuración base: cadena de conexión a SQL Server, tema DevExpress, branding (grupo "CL Grupo Industrial", producto "GPB", subempresa "Aceria"), SSO con Microsoft 365 y niveles de logging. Contiene una cadena de conexión de ejemplo con credenciales en texto plano — ver recomendación de seguridad en `README.md`. |
| `GPX.Web/appsettings.Development.json` | Overrides de entorno de desarrollo: activa `DetailedErrors`. |

---

## 4. Otros archivos de repositorio

| Archivo | Contenido |
|---|---|
| `.gitignore` / `.gitattributes` | Reglas estándar de exclusión y normalización de línea de Git para un proyecto .NET. |
| `.github/workflows/` | Ver sección 3.3. |

---

## 5. Resumen cuantitativo

| Tipo | Cantidad aproximada | Cobertura en este documento |
|---|---|---|
| Clases/servicios/entidades C# (`.cs`) | 70 | Documentadas todas (agrupadas por carpeta) |
| Componentes Blazor (`.razor` + code-behind) | 68 | Documentadas todas (agrupadas; boilerplate de Identity agrupado por ser scaffolding estándar) |
| Infraestructura como código (`.tf`) | 4 | Documentadas todas |
| Manifiestos Kubernetes | 1 | Documentado |
| Pipelines CI/CD | 2 | Documentados |
| Configuración/proyecto (`.json`, `.csproj`, `.slnx`, `Dockerfile`, etc.) | ~10 | Documentados los relevantes |
| Recursos estáticos (`.svg`, `.png`, `.jpg`, `.ico`, `.razor.css`) | ~530 | No documentados individualmente (iconografía e imágenes de `wwwroot/` y estilos de ámbito por componente; no contienen lógica) |

---

*Documento generado a partir del código fuente del repositorio `BL_ACERIA`, como anexo de documentación técnica al TFM "Sistema de Gestión y Planificación de Acería (GPB-Acería)".*
