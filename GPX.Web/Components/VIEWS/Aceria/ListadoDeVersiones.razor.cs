using DevExpress.Blazor;
using DevExpress.Data.Mask.Internal;
using GPX.Negocio.Aceria;
using GPX.Negocio.ORM;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Text.Json;

// [GPX-DOC-v1] ================================================================================
// Codigo tras la vista de listado de versiones de tundish: consulta, activacion y generacion de
// version de corte.
// ================================================================================================

namespace GPX.Web.Components.VIEWS.Aceria
{
    /// <summary>
    /// Clase ListadoDeVersiones. Codigo tras la vista de listado de versiones de tundish: consulta,
    /// activacion y generacion de version de corte.
    /// </summary>
    public partial class ListadoDeVersiones : ComponentBase
    {

        // INYECCIONES

        [Inject] protected IDialogService DialogService { get; set; } = default!;
        [Inject] protected ConfiguracionTundishService ConfiguracionTundishService { get; set; } = default!;

        [Inject] protected VersionTundishSeleccionadoState ConfiguracionTundishState { get; set; } = default!;

        [Inject] protected NavigationManager Navigation { get; set; } = default!;
        // PROPIEDADES PÚBLICAS / PROTEGIDAS

        protected bool IsLoading { get; set; }

        protected DateTime FechaInicio { get; set; } = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        protected DateTime FechaFin { get; set; } = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(1).AddDays(-1);

        protected List<ConfiguracionTundishControl> ListaVersionesTundish { get; set; } = new();

        protected List<ListadoVersionVm> Versiones { get; set; } = new();

        protected List<VersionesPorNecesidadVm> VersionesAgrupadas => Versiones
            .GroupBy(x => new { Inicio = x.FechaInicio.Date, Fin = x.FechaFin.Date })
            .OrderBy(g => g.Key.Inicio)
            .ThenBy(g => g.Key.Fin)
            .Select(g => new VersionesPorNecesidadVm
            {
                FechaInicio = g.Key.Inicio,
                FechaFin = g.Key.Fin,
                Versiones = g.OrderBy(x => x.Version).ToList()
            })
            .ToList();

        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

        // CICLO DE VIDA BLAZOR

        /// <summary>
        /// On Initialized (operacion asincrona).
        /// </summary>
        protected override async Task OnInitializedAsync()
        {
            //await CargaCatalogosAsync();

            // await OnConsultarClick();
        }


        // CARGA DE DATOS

        /// <summary>
        /// Get Calidad Class.
        /// </summary>
        private string GetCalidadClass(string calidad)
        {
            return calidad?.Trim().ToUpper() switch
            {
                "S275-H" => "col-s275-h",
                "S355-TI" => "col-s355-ti",
                "S355-V" => "col-s355-v",
                "S355W" => "col-s355-w",
                "S460-A" => "col-s460-a",
                "S460" => "col-s460-a",
                "S275-TI" => "col-s275-ti",
                _ => "col-default"
            };
        }

        /// <summary>
        /// Limpieza.
        /// </summary>
        protected void Limpieza()
        {
            ListaVersionesTundish = new List<ConfiguracionTundishControl>();
            Versiones = new List<ListadoVersionVm>();
        }


        // EVENTOS UI

        /// <summary>
        /// On Consultar Click.
        /// </summary>
        protected async Task OnConsultarClick()
        {
            IsLoading = true;
            await InvokeAsync(StateHasChanged);

            var inicio = FechaInicio.Date;
            var fin = FechaFin.Date;

            Limpieza();

            ListaVersionesTundish = await ConfiguracionTundishService.CansultaVercionXRango(FechaInicio.Date, FechaFin.Date);


            // solo las versiones donde incio y fin se = al rango  buscado

            // ListaVersionesTundish = ListaVersionesTundish.Where(x => x.NecesidadfechaInicio == FechaInicio.Date && x.NecesidadfechaFin == FechaFin.Date).ToList();


            if (ListaVersionesTundish.Count == 0)
            {
                IsLoading = false;
                await InvokeAsync(StateHasChanged);
                await ShowAlertAsync("Consulta", "No existen versiones disponibles en el rango seleccionado.", MessageBoxRenderStyle.Warning);
            }

            CargarMockVersiones();

            IsLoading = false;
            await InvokeAsync(StateHasChanged);
        }

        /// <summary>
        /// On Activar Version Click.
        /// </summary>
        protected async Task OnActivarVersionClick(ListadoVersionVm version)
        {


            await ConfiguracionTundishService.ActivaVersionTundish(IdVersion: version.IdVersion);


            Versiones.Where(x => x.IdVersion == version.IdVersion).First().EstadoOk = true;

            await DialogService.AlertAsync(new MessageBoxOptions
            {
                Title = "Acción",
                Text = $"Version {version.Version} activada correctamente.",
                OkButtonText = "Aceptar",
                RenderStyle = MessageBoxRenderStyle.Primary
            });
        }

        /// <summary>
        /// On Generar Version Corte Click.
        /// </summary>
        protected async Task OnGenerarVersionCorteClick(ListadoVersionVm version)
        {
            await DialogService.ConfirmAsync(new MessageBoxOptions
            {
                Title = "Confirmación",
                Text = $"¿Está seguro de generar una versión de corte para la versión {version.Version}?",
                OkButtonText = "Sí",
                CancelButtonText = "No",
                RenderStyle = MessageBoxRenderStyle.Warning
            }).ContinueWith(async result =>
            {
                if (result.Result == true)
                {
                    // Aquí iría la lógica para generar la versión de corte
                    // guardamos la version seleccionada en el state container

                    ConfiguracionTundishState.VersionTundish = ListaVersionesTundish.FirstOrDefault(x => x.IdVersion.ToString() == version.IdVersion);

                    Navigation.NavigateTo("/VIEWS/Aceria/PlanificacionCortes");


                }
            });


        }


        // MÉTODOS AUXILIARES

        /// <summary>
        /// Show Alert (operacion asincrona).
        /// </summary>
        private Task ShowAlertAsync(string title, string text, MessageBoxRenderStyle renderStyle)
        {
            return DialogService.AlertAsync(new MessageBoxOptions
            {
                Title = title,
                Text = text,
                OkButtonText = "Aceptar",
                RenderStyle = renderStyle
            });
        }

        private static T? Deserialize<T>(string json) => string.IsNullOrWhiteSpace(json) ? default : JsonSerializer.Deserialize<T>(json, JsonOptions);

        /// <summary>
        /// Cargar Mock Versiones.
        /// </summary>
        private void CargarMockVersiones()
        {



            foreach (var vm in ListaVersionesTundish)
            {
                ListadoVersionVm versionHader = new ListadoVersionVm();
                List<DetalleVersionVm> versionDetail = new List<DetalleVersionVm>();

                List<ListTundishDisponibles> ListaTundishReales = Deserialize<List<ListTundishDisponibles>>(vm.ListaTundishReales) ?? new();
                List<ListDetalleNecesidadBB> ListaNecesidadDetalleBB = Deserialize<List<ListDetalleNecesidadBB>>(vm.ListaNecesidadDetalleBB) ?? new();
                List<BeamBlankNecesidad> ListaNcBBAgrupago = Deserialize<List<BeamBlankNecesidad>>(vm.ListaNecesidadesBeamBlanksAgrupago) ?? new();

                versionHader.IdVersion = vm.IdVersion.ToString();
                versionHader.Version = vm.IdVersion.ToString();
                versionHader.TundishActivos = ListaTundishReales.Where(x => x.TundishActivo == true).Count();
                versionHader.EstadoOk = vm.Estatus == 1 ? true : false;
                versionHader.FechaInicio = vm.NecesidadfechaInicio;
                versionHader.FechaFin = vm.NecesidadfechaFin;
                versionHader.FechaCreacion = vm.FechaCreacionVersion;
                versionHader.FechaModificacion = vm.FechaUltimaModificacion;
                versionHader.Autor = vm.UsuarioAutor;
                versionHader.NumeroColadas = vm.NumColadasReales;
                versionHader.NumeroBarras = ListaNcBBAgrupago.Where(x => x.MatSemi.Trim() == "BB1" || x.MatSemi.Trim() == "BB2" || x.MatSemi.Trim() == "BB3").Select(x => x.UdsAFabSemi).Sum();
                versionHader.Calidades = ListaNecesidadDetalleBB.Where(x => x.Calidad != null && x.Calidad != "").Select(x => x.Calidad).Distinct().ToList();


                // ahora los detalles agrupados por TipoSemi (BB1/BB2/BB3), Calidad y Longitud

                string[] TipoSemi = { "BB1", "BB2", "BB3" };

                var detallesAgrupados = ListaNcBBAgrupago
                    .Where(x => TipoSemi.Contains(x.MatSemi.Trim()))
                    .GroupBy(x => new
                    {
                        Tipo = x.MatSemi.Trim()!,
                        Calidad = x.CalidadSemi ?? string.Empty,
                        Longitud = x.LongitudSemi
                    })
                    .OrderBy(g => Array.IndexOf(TipoSemi, g.Key.Tipo))
                        .ThenBy(g => g.Min(x => x.FechaPrevIni))
                        .ThenBy(g => g.Key.Calidad)
                        .ThenBy(g => g.Key.Longitud)
                    .Select((g, idx) => new DetalleVersionVm
                    {
                        IdDetalle = $"{vm.IdVersion}-{g.Key.Tipo}-{g.Key.Calidad}-{g.Key.Longitud}-{idx}",
                        TipoSemi = g.Key.Tipo,
                        Calidad = g.Key.Calidad,
                        Longitud = g.Key.Longitud,
                        NumeroBarras = g.Sum(x => x.UdsAFabSemi),
                        FechaPrevIni = g.Min(x => x.FechaPrevIni),
                        SemanaPrevIni = g.First().SemanaPrevIni,
                        GAP = false

                    })
                    .ToList();

                versionDetail.AddRange(detallesAgrupados);

                versionHader.Detalles = versionDetail;

                versionHader.AllCalidades = versionDetail.Where(x => !string.IsNullOrWhiteSpace(x.Calidad))
                                                       .Select(x => x.Calidad.Trim()).Distinct().OrderBy(x => x).ToList();
                versionHader.AllLongitudes = versionDetail.Select(x => x.Longitud).Distinct().OrderBy(x => x).ToList();

                Versiones.Add(versionHader);
            }

            Versiones = Versiones
                .OrderBy(x => x.FechaInicio)
                .ThenBy(x => x.FechaFin)
                .ThenBy(x => x.Version)
                .ToList();

            //    new ListadoVersionVm
            //    {
            //        IdVersion = "50-V2",
            //        Version = "V2",
            //        TundishActivos = 2,
            //        EstadoOk = false,
            //        FechaInicio = new DateTime(2025, 12, 12),
            //        FechaFin = new DateTime(2025, 12, 14),
            //        NumeroColadas = 8,
            //        NumeroBarras = 80,
            //        Calidades = ["S275TI", "S355"],
            //        Detalles =
            //        [
            //            new DetalleVersionVm { IdDetalle = "50-V2-1", Linea = "BB2", Numero = 6, Medida = 10000, Calidad = "S275H" },
            //            new DetalleVersionVm { IdDetalle = "50-V2-2", Linea = "BB1", Numero = 10, Medida = 11900, Calidad = "S355" },
            //            new DetalleVersionVm { IdDetalle = "50-V2-3", Linea = "BB2", Numero = 4, Medida = 10000, Calidad = "S275H" }
            //        ]
            //    }
            //];
        }



    }

    /// <summary>
    /// Clase VersionesPorNecesidadVm. Codigo tras la vista de listado de versiones de tundish: consulta,
    /// activacion y generacion de version de corte.
    /// </summary>
    public sealed class VersionesPorNecesidadVm
    {
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public List<ListadoVersionVm> Versiones { get; set; } = new();
    }
}
