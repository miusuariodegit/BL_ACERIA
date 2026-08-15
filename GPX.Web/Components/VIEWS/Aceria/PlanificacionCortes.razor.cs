using DevExpress.Blazor;
using DevExpress.Data.Automation.Text.Primitives.Models;
using GPX.Negocio.Aceria;
using GPX.Negocio.ORM;
using Microsoft.AspNetCore.Components;
using System.Globalization;
using System.Text.Json;

// [GPX-DOC-v1] ================================================================================
// Codigo tras la vista de planificacion de cortes: ciclo de vida de coladas, calculo de capacidad por
// linea, asignaciones automaticas y ajustes manuales.
// ================================================================================================

namespace GPX.Web.Components.VIEWS.Aceria;

/// <summary>
/// Clase PlanificacionCortes. Codigo tras la vista de planificacion de cortes: ciclo de vida de
/// coladas, calculo de capacidad por linea, asignaciones automaticas y ajustes manuales.
/// </summary>
public partial class PlanificacionCortes : ComponentBase
{
    [Inject] protected IDialogService DialogService { get; set; } = default!;
    [Inject] protected ConfiguracionTundishService ConfiguracionTundishService { get; set; } = default!;
    [Inject] protected VersionTundishSeleccionadoState ConfiguracionTundishState { get; set; } = default!;

    protected bool IsLoading { get; set; }
    protected ConfiguracionTundishControl? VersionesTundish { get; set; } = new();
    protected ListadoVersionVm versionHader = new();
    protected List<DetalleVersionVm> versionDetail = new();

    protected List<ListTundishDisponibles> ListaTundishReales { get; set; } = new();
    protected List<ListDetalleNecesidadBB> ListaNecesidadDetalleBB { get; set; } = new();
    protected List<BeamBlankNecesidad> ListaNcBBAgrupago { get; set; } = new();
    protected List<BeamBlankNecesidad> ListaNecesidadCompleta { get; set; } = new();
    protected List<ListaPropuestaDistribucion> ListaPropuestaDistribucionColadas { get; set; } = new();
    protected List<ConfiguracionAceria> ListaConfiguracionAceria { get; set; } = new();
    protected List<CorteVersionVm> VersionesCorte { get; set; } = new();
    protected string VersionCorteActiva { get; set; } = "A1";
    protected ListTundishDisponibles? SelectedTundish { get; set; }
    protected List<TundishTreeVm> ArbolTundish { get; set; } = new();

    protected List<CorteAsignacionVm> AsignacionesActuales =>
        VersionesCorte.FirstOrDefault(x => x.Clave == VersionCorteActiva)?.Asignaciones ?? new();

    protected List<CorteAsignacionVm> AsignacionesTundishSeleccionado => SelectedTundish is null
        ? new()
        : AsignacionesActuales.Where(x => x.NumTundish == SelectedTundish.NumTundish).ToList();

    protected int SelectedTundishValue => SelectedTundish?.NumTundish ?? 0;
    protected List<TundishSelectorItem> TundishSelectorItems => ListaTundishReales
        .OrderBy(x => x.NumTundish)
        .Select(x => new TundishSelectorItem(x.NumTundish, $"{x.NumTundish}    [{x.tsCierre1} - {x.tsCierre2} - {x.tsCierre3} - {x.tsCierre4} - {x.tsCierre5} - {x.tsCierre6}]"))
        .ToList();

    protected List<CorteResumenVm> ResumenCorteActual { get; set; } = new();
    protected List<ComparativaVersionVm> ComparativaVersiones { get; set; } = new();
    protected int DetalleTabIndex { get; set; }
    protected bool PopupColadaManualVisible { get; set; }
    protected ManualColadaVm ColadaManualEdit { get; set; } = new();
    protected bool PuedeEliminarVersionActiva => VersionCorteActiva != "A1" && VersionesCorte.Exists(x => x.Clave == VersionCorteActiva);

    protected static readonly BocaEstadoOption[] EstadosBoca =
    [
        new("Abierta", "ABIERTA"),
        new("Cerrada", "CERRADA"),
        new("Bloqueada/Perforada", "BLOQUEDA/PERFORADA")
    ];

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly string[] tipoSemiOrden = ["BB1", "BB2", "BB3"];
    private readonly HashSet<string> coladasCerradas = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<CorteAsignacionVm> asignacionesManuales = new();

    /// <summary>
    /// On Initialized.
    /// </summary>
    protected override void OnInitialized()
    {
        VersionesTundish = ConfiguracionTundishState.VersionTundish;
        if (VersionesTundish is null || VersionesTundish.IdVersion <= 0)
            return;

        CargarDatosBase();
        CrearVersionInicial();
        SelectedTundish = ListaTundishReales.OrderBy(x => x.NumTundish).FirstOrDefault();
        RecalcularPlanCortes();
    }

    /// <summary>
    /// Cargar Datos Base.
    /// </summary>
    private void CargarDatosBase()
    {
        ListaTundishReales = Deserialize<List<ListTundishDisponibles>>(VersionesTundish?.ListaTundishReales ?? string.Empty) ?? new();
        ListaNecesidadDetalleBB = Deserialize<List<ListDetalleNecesidadBB>>(VersionesTundish?.ListaNecesidadDetalleBB ?? string.Empty) ?? new();
        ListaNcBBAgrupago = Deserialize<List<BeamBlankNecesidad>>(VersionesTundish?.ListaNecesidadesBeamBlanksAgrupago ?? string.Empty) ?? new();
        ListaNecesidadCompleta = Deserialize<List<BeamBlankNecesidad>>(VersionesTundish?.ListaNecesidadesBeamBlanks ?? string.Empty) ?? new();
        ListaPropuestaDistribucionColadas = Deserialize<List<ListaPropuestaDistribucion>>(VersionesTundish?.ListaPropuestaDistribucionColadas ?? string.Empty) ?? new();
        ListaConfiguracionAceria = Deserialize<List<ConfiguracionAceria>>(VersionesTundish?.ListaConfiguracionAceria ?? string.Empty) ?? new();

        versionHader.IdVersion = VersionesTundish?.IdVersion.ToString() ?? string.Empty;
        versionHader.Version = VersionesTundish?.IdVersion.ToString() ?? string.Empty;
        versionHader.TundishActivos = ListaTundishReales.Count(x => x.TundishActivo);
        versionHader.EstadoOk = VersionesTundish?.Estatus == 1;
        versionHader.FechaInicio = VersionesTundish?.NecesidadfechaInicio ?? DateTime.MinValue;
        versionHader.FechaFin = VersionesTundish?.NecesidadfechaFin ?? DateTime.MinValue;
        versionHader.FechaCreacion = VersionesTundish?.FechaCreacionVersion ?? DateTime.MinValue;
        versionHader.FechaModificacion = VersionesTundish?.FechaUltimaModificacion ?? DateTime.MinValue;
        versionHader.Autor = VersionesTundish?.UsuarioAutor ?? string.Empty;
        versionHader.NumeroColadas = VersionesTundish?.NumColadasReales ?? 0;
        versionHader.NumeroBarras = ListaNcBBAgrupago.Where(IsTipoSemiValido).Sum(x => x.UdsAFabSemi);
        versionHader.Calidades = ListaNecesidadDetalleBB
            .Where(x => !string.IsNullOrWhiteSpace(x.Calidad))
            .Select(x => x.Calidad.Trim())
            .Distinct()
            .OrderBy(x => x)
            .ToList();

        versionDetail = ListaNcBBAgrupago
            .Where(IsTipoSemiValido)
            .GroupBy(x => new
            {
                Tipo = (x.MatSemi ?? string.Empty).Trim(),
                Calidad = x.CalidadSemi ?? string.Empty,
                Longitud = x.LongitudSemi
            })
            .OrderBy(g => Array.IndexOf(tipoSemiOrden, g.Key.Tipo))
            .ThenBy(g => g.Min(x => x.FechaPrevIni))
            .ThenBy(g => g.Key.Calidad)
            .ThenBy(g => g.Key.Longitud)
            .Select((g, idx) => new DetalleVersionVm
            {
                IdDetalle = $"{versionHader.IdVersion}-{g.Key.Tipo}-{g.Key.Calidad}-{g.Key.Longitud}-{idx}",
                TipoSemi = g.Key.Tipo,
                Calidad = g.Key.Calidad,
                Longitud = g.Key.Longitud,
                NumeroBarras = g.Sum(x => x.UdsAFabSemi),
                FechaPrevIni = g.Min(x => x.FechaPrevIni),
                SemanaPrevIni = g.First().SemanaPrevIni,
                GAP = false
            })
            .ToList();

        versionHader.Detalles = versionDetail;
        versionHader.AllCalidades = versionDetail.Where(x => !string.IsNullOrWhiteSpace(x.Calidad)).Select(x => x.Calidad.Trim()).Distinct().OrderBy(x => x).ToList();
        versionHader.AllLongitudes = versionDetail.Select(x => x.Longitud).Distinct().OrderBy(x => x).ToList();
    }

    /// <summary>
    /// Crear Version Inicial.
    /// </summary>
    private void CrearVersionInicial()
    {
        VersionesCorte =
        [
            new CorteVersionVm { Clave = "A1", Nombre = "A1 - Version inicial", EsInicial = true }
        ];
        VersionCorteActiva = "A1";
    }

    /// <summary>
    /// Seleccionar Tundish.
    /// </summary>
    protected void SeleccionarTundish(ListTundishDisponibles tundish)
    {
        SelectedTundish = tundish;
    }

    /// <summary>
    /// On Selected Tundish Value Changed.
    /// </summary>
    protected void OnSelectedTundishValueChanged(int value)
    {
        SelectedTundish = ListaTundishReales.FirstOrDefault(x => x.NumTundish == value) ?? SelectedTundish;
    }

    /// <summary>
    /// On Finalizar Tundish Click.
    /// </summary>
    protected async Task OnFinalizarTundishClick(ListTundishDisponibles tundish)
    {
        await ShowAlertAsync("Tundish", $"Tundish {tundish.NumTundish} marcado para finalizar.", MessageBoxRenderStyle.Info);
    }

    /// <summary>
    /// On Finalizar Colada Click.
    /// </summary>
    protected async Task OnFinalizarColadaClick(ColadaTreeVm colada)
    {
        if (!PuedeFinalizarColada(colada))
            return;

        var confirmar = await DialogService.ConfirmAsync(new MessageBoxOptions
        {
            Title = "Finalizar colada",
            Text = $"Deseas finalizar la colada {colada.Numero} del {colada.NumTundish}?",
            OkButtonText = "Aceptar",
            CancelButtonText = "Cancelar",
            RenderStyle = MessageBoxRenderStyle.Warning
        });

        if (!confirmar)
            return;

        coladasCerradas.Add(GetColadaKey(colada.Version, colada.NumTundish, colada.Numero));
        ConstruirArbolTundish();
        await ShowAlertAsync("Colada", $"Colada {colada.Numero} cerrada.", MessageBoxRenderStyle.Info);
    }

    /// <summary>
    /// On Version Corte Changed.
    /// </summary>
    protected void OnVersionCorteChanged(string value)
    {
        VersionCorteActiva = value;
        RecalcularPlanCortes();
    }

    /// <summary>
    /// On Nueva Version Click.
    /// </summary>
    protected async Task OnNuevaVersionClick()
    {
        if (VersionesCorte.Count >= 5)
        {
            await ShowAlertAsync("Versiones", "Solo se permiten 5 versiones de corte.", MessageBoxRenderStyle.Warning);
            return;
        }

        var claves = new[] { "A1", "B2", "C1", "D1", "E" };
        var nuevaClave = claves.First(x => VersionesCorte.All(v => v.Clave != x));
        VersionesCorte.Add(new CorteVersionVm { Clave = nuevaClave, Nombre = $"{nuevaClave} - Alternativa {VersionesCorte.Count + 1}" });
        VersionCorteActiva = nuevaClave;
        RecalcularPlanCortes();
    }

    /// <summary>
    /// On Eliminar Version Click.
    /// </summary>
    protected async Task OnEliminarVersionClick()
    {
        if (!PuedeEliminarVersionActiva)
            return;

        VersionesCorte.RemoveAll(x => x.Clave == VersionCorteActiva);
        VersionCorteActiva = "A1";
        RecalcularPlanCortes();
        await ShowAlertAsync("Versiones", "Version eliminada.", MessageBoxRenderStyle.Info);
    }

    /// <summary>
    /// On Tundish Activo Changed.
    /// </summary>
    protected void OnTundishActivoChanged(bool value)
    {
        if (SelectedTundish is null)
            return;

        SelectedTundish.TundishActivo = value;
        RecalcularCapacidadTundish(SelectedTundish);
        RecalcularPlanCortes();
    }

    /// <summary>
    /// On Estado Boca Changed.
    /// </summary>
    protected void OnEstadoBocaChanged(int numeroBoca, string estatusBoca)
    {
        if (SelectedTundish is null)
            return;

        var tipo = GetBoca(numeroBoca).Tipo;
        ConfigurarBoca(SelectedTundish, tipo, estatusBoca, numeroBoca);
        RecalcularCapacidadTundish(SelectedTundish);
        RecalcularPlanCortes();
    }

    /// <summary>
    /// On Disminuir Coladas Click.
    /// </summary>
    protected async Task OnDisminuirColadasClick()
    {
        if (SelectedTundish is null || SelectedTundish.NumColadas <= 0)
            return;

        var confirmar = await DialogService.ConfirmAsync(new MessageBoxOptions
        {
            Title = "Disminuir coladas",
            Text = $"Se reducira {SelectedTundish.NombreTundish} a {(SelectedTundish.NumColadas - 1m):N0} coladas y se recalculara la distribucion desde este tundish. Deseas continuar?",
            OkButtonText = "Aceptar",
            CancelButtonText = "Cancelar",
            RenderStyle = MessageBoxRenderStyle.Warning
        });

        if (!confirmar)
            return;

        SelectedTundish.NumColadas = Math.Max(0m, SelectedTundish.NumColadas - 1m);
        RemoverColadasManualesFueraDeRango(SelectedTundish.NumTundish, (int)SelectedTundish.NumColadas);
        RecalcularPlanCortes();
    }

    /// <summary>
    /// On Aumentar Coladas Click.
    /// </summary>
    protected void OnAumentarColadasClick()
    {
        if (SelectedTundish is null)
            return;

        var nuevaColada = (int)SelectedTundish.NumColadas + 1;
        ColadaManualEdit = CrearColadaManualEdit(SelectedTundish, nuevaColada);
        PopupColadaManualVisible = true;
    }

    /// <summary>
    /// On Guardar Colada Manual Click.
    /// </summary>
    protected async Task OnGuardarColadaManualClick()
    {
        if (SelectedTundish is null || ColadaManualEdit.Lineas.Count == 0)
            return;

        foreach (var linea in ColadaManualEdit.Lineas)
        {
            if (linea.Abierta && linea.Longitud <= 0)
            {
                await ShowAlertAsync("Validacion", $"La longitud de la linea {linea.Linea} debe ser mayor a cero.", MessageBoxRenderStyle.Warning);
                return;
            }

            if (linea.Abierta && linea.Barras < 0)
            {
                await ShowAlertAsync("Validacion", $"Las barras de la linea {linea.Linea} no pueden ser negativas.", MessageBoxRenderStyle.Warning);
                return;
            }

            if (linea.Produccion < 0 || linea.Toneladas < 0)
            {
                await ShowAlertAsync("Validacion", $"Produccion y Tn totales de la linea {linea.Linea} no pueden ser negativas.", MessageBoxRenderStyle.Warning);
                return;
            }
        }

        asignacionesManuales.RemoveAll(x =>
            x.EsManual &&
            x.Version == VersionCorteActiva &&
            x.NumTundish == ColadaManualEdit.NumTundish &&
            x.NumColada == ColadaManualEdit.NumColada);

        asignacionesManuales.AddRange(ColadaManualEdit.Lineas
            .Where(x => x.Abierta)
            .Select(x => new CorteAsignacionVm
            {
                Version = VersionCorteActiva,
                NumTundish = ColadaManualEdit.NumTundish,
                NumColada = ColadaManualEdit.NumColada,
                TipoSemi = x.TipoSemi,
                Linea = x.Linea,
                LineaEstado = "ABIERTA",
                Calidad = ColadaManualEdit.Calidad,
                Longitud = x.Longitud,
                LongitudOriginal = x.Longitud,
                BarrasAsignadas = x.Barras,
                BarrasFabricadas = x.Produccion,
                ToneladasAsignadas = Math.Round(x.Toneladas, 2),
                SemanaPrevIni = "Manual",
                FechaPrevIni = SelectedTundish.FechaInicio,
                EsManual = true
            }));

        SelectedTundish.NumColadas = Math.Max(SelectedTundish.NumColadas, (decimal)ColadaManualEdit.NumColada);
        FusionarAsignacionesManuales();
        RecalcularResumenYComparativa();
        ConstruirArbolTundish();
        PopupColadaManualVisible = false;
    }

    /// <summary>
    /// On Cancelar Colada Manual Click.
    /// </summary>
    protected void OnCancelarColadaManualClick()
    {
        PopupColadaManualVisible = false;
    }

    /// <summary>
    /// On Asignacion Edit Model Saving.
    /// </summary>
    protected async Task OnAsignacionEditModelSaving(GridEditModelSavingEventArgs e)
    {
        var edit = (CorteAsignacionVm)e.EditModel;
        if (EstaColadaCerrada(edit.Version, edit.NumTundish, edit.NumColada))
        {
            e.Cancel = true;
            await ShowAlertAsync("Validacion", "La colada esta cerrada y no se puede editar.", MessageBoxRenderStyle.Warning);
            return;
        }

        if (string.Equals(edit.TipoSemi, "Cerrada", StringComparison.OrdinalIgnoreCase))
        {
            e.Cancel = true;
            await ShowAlertAsync("Validacion", "Las lineas cerradas no se pueden editar.", MessageBoxRenderStyle.Warning);
            return;
        }

        if (edit.Longitud <= 0)
        {
            e.Cancel = true;
            await ShowAlertAsync("Validacion", "La longitud debe ser mayor a cero.", MessageBoxRenderStyle.Warning);
            return;
        }

        var longitudExiste = versionDetail.Any(x =>
            string.Equals(x.TipoSemi, edit.TipoSemi, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(x.Calidad, edit.Calidad, StringComparison.OrdinalIgnoreCase) &&
            x.Longitud == edit.Longitud);

        if (!longitudExiste)
        {
            e.Cancel = true;
            await ShowAlertAsync("Validacion", $"La longitud {edit.Longitud:N0} no existe para la calidad {edit.Calidad} y tipo {edit.TipoSemi}.", MessageBoxRenderStyle.Warning);
            return;
        }

        RecalcularAsignacionEditada(edit);
        e.CopyChangesToDataItem();
        SincronizarAsignacionManual(edit);
        RecalcularResumenYComparativa();
        ConstruirArbolTundish();
    }

    /// <summary>
    /// Recalcular Plan Cortes.
    /// </summary>
    private void RecalcularPlanCortes()
    {
        foreach (var version in VersionesCorte)
        {
            version.Asignaciones = GenerarAsignaciones(version.Clave);
            version.Asignaciones.AddRange(asignacionesManuales.Where(x => x.Version == version.Clave).Select(CloneAsignacion));
        }

        RecalcularResumenYComparativa();
        ConstruirArbolTundish();
    }

    /// <summary>
    /// Construir Arbol Tundish.
    /// </summary>
    private void ConstruirArbolTundish()
    {
        ArbolTundish = ListaTundishReales
            .OrderBy(x => x.NumTundish)
            .Select(tundish => new TundishTreeVm
            {
                Tundish = NormalizarTundish(tundish),
                Versiones = VersionesCorte
                    .Select(version => ConstruirVersionTree(tundish, version))
                    .Where(x => x.Coladas.Count > 0 || x.BarrasTeoricas > 0)
                    .ToList()
            })
            .ToList();
    }

    /// <summary>
    /// Construir Version Tree.
    /// </summary>
    private CorteVersionTreeVm ConstruirVersionTree(ListTundishDisponibles tundish, CorteVersionVm version)
    {
        var asignaciones = version.Asignaciones
            .Where(x => x.NumTundish == tundish.NumTundish)
            .ToList();
        var comparativa = ComparativaVersiones.FirstOrDefault(x => x.Version == version.Clave);
        var coladas = ConstruirColadasTree(tundish, version.Clave, asignaciones);

        return new CorteVersionTreeVm
        {
            Version = version.Clave,
            Activa = version.Clave == VersionCorteActiva,
            Fecha = asignaciones.Select(x => x.FechaPrevIni).DefaultIfEmpty(tundish.FechaInicio).Min(),
            BarrasTeoricas = asignaciones.Sum(x => x.BarrasAsignadas),
            Toneladas = asignaciones.Sum(x => x.ToneladasAsignadas),
            ResumenCortes = CrearResumenCortes(asignaciones),
            Score = comparativa?.ScoreTotal ?? 0,
            Coladas = coladas
        };
    }

    /// <summary>
    /// Construir Coladas Tree.
    /// </summary>
    private List<ColadaTreeVm> ConstruirColadasTree(ListTundishDisponibles tundish, string version, List<CorteAsignacionVm> asignaciones)
    {
        var distribucion = ListaPropuestaDistribucionColadas.FirstOrDefault(x => x.NumTundish == tundish.NumTundish);
        if (distribucion is null)
            return new();

        var numero = 1;
        var coladas = new List<ColadaTreeVm>();
        var lineas = ObtenerLineasTundish(tundish).ToList();

        var coladasAutomaticas = ObtenerNumeroColadasAutomaticas(version, tundish);
        foreach (var calidad in ObtenerSecuenciaColadas(distribucion).Take(coladasAutomaticas))
        {
            var numeroColada = numero++;
            var coladaCerrada = EstaColadaCerrada(version, tundish.NumTundish, numeroColada);
            var perfilesColada = asignaciones
                .Where(x => x.NumColada == numeroColada)
                .OrderBy(x => x.Linea)
                .ThenBy(x => Array.IndexOf(tipoSemiOrden, x.TipoSemi))
                .ThenBy(x => x.Longitud)
                .ToList();

            perfilesColada.AddRange(lineas
                .Where(x => !x.Abierta && perfilesColada.All(p => p.Linea != x.Numero))
                .Select(x => CrearLineaCerrada(version, tundish.NumTundish, numeroColada, calidad, x)));

            coladas.Add(new ColadaTreeVm
            {
                Version = version,
                NumTundish = tundish.NumTundish,
                Numero = numeroColada,
                Calidad = calidad,
                Coladas = 1,
                Barras = perfilesColada.Sum(x => x.BarrasAsignadas),
                Toneladas = perfilesColada.Sum(x => x.ToneladasAsignadas),
                Cerrada = coladaCerrada,
                PuedeFinalizar = !coladaCerrada && PuedeFinalizarColada(version, tundish.NumTundish, numeroColada),
                EsManual = perfilesColada.Any(x => x.EsManual),
                Perfiles = perfilesColada
                    .OrderBy(x => x.Linea)
                    .ThenBy(x => x.Longitud)
                    .ToList()
            });
        }

        var manuales = asignaciones
            .Where(x => x.EsManual)
            .GroupBy(x => x.NumColada)
            .OrderBy(x => x.Key);

        foreach (var grupoManual in manuales)
        {
            if (coladas.Any(x => x.Numero == grupoManual.Key))
                continue;

            var perfilesColada = grupoManual
                .OrderBy(x => x.Linea)
                .ThenBy(x => x.Longitud)
                .ToList();
            var coladaCerrada = EstaColadaCerrada(version, tundish.NumTundish, grupoManual.Key);

            coladas.Add(new ColadaTreeVm
            {
                Version = version,
                NumTundish = tundish.NumTundish,
                Numero = grupoManual.Key,
                Calidad = perfilesColada.FirstOrDefault()?.Calidad ?? "Manual",
                Coladas = 1,
                Barras = perfilesColada.Sum(x => x.BarrasAsignadas),
                Toneladas = perfilesColada.Sum(x => x.ToneladasAsignadas),
                Cerrada = coladaCerrada,
                PuedeFinalizar = !coladaCerrada && PuedeFinalizarColada(version, tundish.NumTundish, grupoManual.Key),
                EsManual = true,
                Perfiles = perfilesColada
            });
        }

        return coladas;
    }

    /// <summary>
    /// Puede Finalizar Colada.
    /// </summary>
    private bool PuedeFinalizarColada(ColadaTreeVm colada) =>
        PuedeFinalizarColada(colada.Version, colada.NumTundish, colada.Numero);

    /// <summary>
    /// Puede Finalizar Colada.
    /// </summary>
    private bool PuedeFinalizarColada(string version, int numTundish, int numColada)
    {
        if (!string.Equals(version, VersionCorteActiva, StringComparison.OrdinalIgnoreCase))
            return false;

        if (EstaColadaCerrada(version, numTundish, numColada))
            return false;

        for (var i = 1; i < numColada; i++)
        {
            if (!EstaColadaCerrada(version, numTundish, i))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Esta Colada Cerrada.
    /// </summary>
    private bool EstaColadaCerrada(string version, int numTundish, int numColada) =>
        coladasCerradas.Contains(GetColadaKey(version, numTundish, numColada));

    /// <summary>
    /// Get Colada Key.
    /// </summary>
    private static string GetColadaKey(string version, int numTundish, int numColada) =>
        $"{version}|{numTundish}|{numColada}";

    /// <summary>
    /// Obtener Numero Coladas Automaticas.
    /// </summary>
    private int ObtenerNumeroColadasAutomaticas(string version, ListTundishDisponibles tundish)
    {
        var manuales = asignacionesManuales
            .Where(x => x.Version == version && x.NumTundish == tundish.NumTundish && x.EsManual)
            .Select(x => x.NumColada)
            .Distinct()
            .Count();

        return Math.Max(0, (int)tundish.NumColadas - manuales);
    }

    /// <summary>
    /// Remover Coladas Manuales Fuera De Rango.
    /// </summary>
    private void RemoverColadasManualesFueraDeRango(int numTundish, int maxColada)
    {
        asignacionesManuales.RemoveAll(x => x.NumTundish == numTundish && x.NumColada > maxColada);
        foreach (var version in VersionesCorte)
            version.Asignaciones.RemoveAll(x => x.EsManual && x.NumTundish == numTundish && x.NumColada > maxColada);
    }

    /// <summary>
    /// Fusionar Asignaciones Manuales.
    /// </summary>
    private void FusionarAsignacionesManuales()
    {
        foreach (var version in VersionesCorte)
        {
            version.Asignaciones.RemoveAll(x => x.EsManual);
            version.Asignaciones.AddRange(asignacionesManuales.Where(x => x.Version == version.Clave).Select(CloneAsignacion));
        }
    }

    /// <summary>
    /// Sincronizar Asignacion Manual.
    /// </summary>
    private void SincronizarAsignacionManual(CorteAsignacionVm edit)
    {
        if (!edit.EsManual)
            return;

        var manual = asignacionesManuales.FirstOrDefault(x =>
            x.Version == edit.Version &&
            x.NumTundish == edit.NumTundish &&
            x.NumColada == edit.NumColada &&
            x.Linea == edit.Linea &&
            x.TipoSemi == edit.TipoSemi);

        if (manual is null)
            return;

        manual.Longitud = edit.Longitud;
        manual.LongitudOriginal = edit.LongitudOriginal;
        manual.LongitudModificada = edit.LongitudModificada;
        manual.BarrasAsignadas = edit.BarrasAsignadas;
        manual.BarrasFabricadas = edit.BarrasFabricadas;
        manual.ToneladasAsignadas = edit.ToneladasAsignadas;
    }

    /// <summary>
    /// Normalizar Tundish.
    /// </summary>
    private ListTundishDisponibles NormalizarTundish(ListTundishDisponibles tundish)
    {
        if (string.IsNullOrWhiteSpace(tundish.NombreTundish))
            tundish.NombreTundish = $"UcTundish_{tundish.NumTundish}";

        return tundish;
    }

    /// <summary>
    /// Crear Resumen Cortes.
    /// </summary>
    private static string CrearResumenCortes(List<CorteAsignacionVm> asignaciones)
    {
        if (asignaciones.Count == 0)
            return "Sin cortes asignados";

        return string.Join(", ", asignaciones
            .GroupBy(x => x.TipoSemi)
            .OrderBy(x => x.Key)
            .Select(g =>
            {
                var barras = g.Sum(x => x.BarrasAsignadas);
                var longitudPromedio = g.Sum(x => x.BarrasAsignadas) == 0
                    ? 0
                    : g.Sum(x => x.BarrasAsignadas * (x.Longitud / 1000m)) / g.Sum(x => x.BarrasAsignadas);
                return $"{g.Key}: {barras:N0} ({longitudPromedio:N1} m)";
            }));
    }

    /// <summary>
    /// Calcular Capacidad Linea.
    /// </summary>
    private decimal CalcularCapacidadLinea(
    string tipoBB,
    string statusBoca,
    ListTundishDisponibles tundish)
    {
        if (statusBoca != "ABIERTA")
            return 0;

        decimal toneladas = tipoBB switch
        {
            "BB1" => tundish.TnXLineaBB1,
            "BB2" => tundish.TnXLineaBB2,
            "BB3" => tundish.TnXLineaBB3,
            _ => 0
        };

        if (toneladas <= 0)
            return 0;

        decimal pesoLineal = GetPesoLineal(tipoBB);

        if (pesoLineal <= 0)
            return 0;

        return (toneladas * 1000m) / pesoLineal;
    }

    /// <summary>
    /// Generar Asignaciones.
    /// </summary>
    private List<CorteAsignacionVm> GenerarAsignaciones(string version)
    {
        var resultado = new List<CorteAsignacionVm>();
        var pendientes = versionDetail.ToDictionary(x => x.IdDetalle, x => x.NumeroBarras);

        foreach (var tundish in ListaTundishReales.Where(x => x.TundishActivo).OrderBy(x => x.NumTundish))
        {
            var distribucion = ListaPropuestaDistribucionColadas.FirstOrDefault(x => x.NumTundish == tundish.NumTundish);
            if (distribucion is null)
                continue;

            var numeroColada = 0;
            var coladasAutomaticas = ObtenerNumeroColadasAutomaticas(version, tundish);
            foreach (var calidad in ObtenerSecuenciaColadas(distribucion).Take(coladasAutomaticas))
            {
                numeroColada++;
                var lineas = ObtenerLineasTundish(tundish)
                    .Select(x => x with { CapacidadRestanteMm = x.CapacidadMetros * 1000m })
                    .ToList();

                foreach (var tipo in tipoSemiOrden.Where(tipo => TundishTieneTipo(tundish, tipo)))
                {
                    var lineasTipo = lineas.Where(x => x.TipoSemi == tipo && x.Abierta).OrderBy(x => x.Numero).ToList();
                    if (lineasTipo.Count == 0)
                        continue;

                    var detalles = OrdenarDetallesParaCalidad(versionDetail
                        .Where(x => x.TipoSemi == tipo && x.Calidad == calidad && pendientes[x.IdDetalle] > 0), calidad);

                    foreach (var detalle in detalles)
                        RepartirDetalleEnLineas(version, tundish, numeroColada, detalle, pendientes, lineasTipo, resultado);

                    RellenarStockColada(version, tundish, numeroColada, calidad, tipo, lineasTipo, resultado);
                }
            }
        }

        return resultado;
    }

    /// <summary>
    /// Ordenar Detalles Para Calidad.
    /// </summary>
    private List<DetalleVersionVm> OrdenarDetallesParaCalidad(IEnumerable<DetalleVersionVm> detalles, string calidad)
    {
        var esCalidadNormal = EsCalidadNormal(calidad);
        var query = esCalidadNormal
            ? detalles.OrderBy(x => x.FechaPrevIni).ThenBy(x => x.Longitud)
            : detalles.OrderByDescending(x => x.FechaPrevIni).ThenBy(x => x.Longitud);

        return query.ToList();
    }

    /// <summary>
    /// Repartir Detalle En Lineas.
    /// </summary>
    private void RepartirDetalleEnLineas(
        string version,
        ListTundishDisponibles tundish,
        int numeroColada,
        DetalleVersionVm detalle,
        Dictionary<string, int> pendientes,
        List<LineaTundishVm> lineasTipo,
        List<CorteAsignacionVm> resultado)
    {
        if (!pendientes.TryGetValue(detalle.IdDetalle, out var pendiente) || pendiente <= 0)
            return;

        if (detalle.Longitud <= 0)
            return;

        var tnPorBarra = GetTnPorBarra(detalle.TipoSemi, detalle.Longitud);
        if (tnPorBarra <= 0)
            return;

        var asignadasPorLinea = lineasTipo.ToDictionary(x => x.Numero, _ => 0);
        var huboAsignacion = true;
        while (pendiente > 0 && huboAsignacion)
        {
            huboAsignacion = false;
            foreach (var linea in lineasTipo.OrderBy(x => x.Numero))
            {
                if (pendiente <= 0)
                    break;

                if (!PuedeAsignarCorte(linea, detalle.Longitud))
                    continue;

                linea.CapacidadRestanteMm -= detalle.Longitud;
                asignadasPorLinea[linea.Numero]++;
                pendiente--;
                huboAsignacion = true;
            }
        }

        foreach (var asignacionLinea in asignadasPorLinea.Where(x => x.Value > 0).OrderBy(x => x.Key))
        {
            var linea = lineasTipo.First(x => x.Numero == asignacionLinea.Key);
            var barrasAsignadas = asignacionLinea.Value;
            resultado.Add(new CorteAsignacionVm
            {
                Version = version,
                NumTundish = tundish.NumTundish,
                NumColada = numeroColada,
                TipoSemi = detalle.TipoSemi,
                Linea = linea.Numero,
                LineaEstado = linea.Estado,
                Calidad = detalle.Calidad,
                Longitud = detalle.Longitud,
                LongitudOriginal = detalle.Longitud,
                BarrasAsignadas = barrasAsignadas,
                BarrasFabricadas = 0,
                ToneladasAsignadas = Math.Round(barrasAsignadas * tnPorBarra, 2),
                SemanaPrevIni = detalle.SemanaPrevIni ?? string.Empty,
                FechaPrevIni = detalle.FechaPrevIni
            });
        }

        pendientes[detalle.IdDetalle] = pendiente;
    }

    /// <summary>
    /// Rellenar Stock Colada.
    /// </summary>
    private void RellenarStockColada(
        string version,
        ListTundishDisponibles tundish,
        int numeroColada,
        string calidad,
        string tipo,
        List<LineaTundishVm> lineasTipo,
        List<CorteAsignacionVm> resultado)
    {
        const int longitudStock = 12100;
        var tnPorBarraStock = GetTnPorBarra(tipo, longitudStock);
        if (tnPorBarraStock <= 0)
            return;

        foreach (var linea in lineasTipo.OrderBy(x => x.Numero))
        {
            if (resultado.Any(x => x.NumTundish == tundish.NumTundish && x.NumColada == numeroColada && x.Linea == linea.Numero))
                continue;

            var barrasStock = CalcularCortesPosibles(linea.CapacidadRestanteMm, longitudStock);
            if (barrasStock <= 0)
                continue;

            linea.CapacidadRestanteMm -= barrasStock * longitudStock;
            resultado.Add(new CorteAsignacionVm
            {
                Version = version,
                NumTundish = tundish.NumTundish,
                NumColada = numeroColada,
                TipoSemi = tipo,
                Linea = linea.Numero,
                LineaEstado = linea.Estado,
                Calidad = calidad,
                Longitud = longitudStock,
                LongitudOriginal = longitudStock,
                BarrasAsignadas = barrasStock,
                BarrasFabricadas = 0,
                ToneladasAsignadas = Math.Round(barrasStock * tnPorBarraStock, 2),
                SemanaPrevIni = "Stock",
                FechaPrevIni = tundish.FechaInicio
            });
        }
    }

    /// <summary>
    /// Crear Colada Manual Edit.
    /// </summary>
    private ManualColadaVm CrearColadaManualEdit(ListTundishDisponibles tundish, int numeroColada)
    {
        var lineas = ObtenerLineasTundish(tundish)
            .Select(x => new ManualColadaLineaVm
            {
                Linea = x.Numero,
                TipoSemi = x.TipoSemi,
                Estado = x.Estado,
                Abierta = x.Abierta,
                Longitud = x.Abierta ? 12100 : 0,
                Barras = 0,
                Produccion = 0,
                Toneladas = 0
            })
            .ToList();

        return new ManualColadaVm
        {
            Version = VersionCorteActiva,
            NumTundish = tundish.NumTundish,
            NombreTundish = tundish.NombreTundish,
            NumColada = numeroColada,
            Calidad = versionDetail.OrderBy(x => x.FechaPrevIni).Select(x => x.Calidad).FirstOrDefault() ?? "Manual",
            Lineas = lineas
        };
    }

    /// <summary>
    /// Recalcular Asignacion Editada.
    /// </summary>
    private void RecalcularAsignacionEditada(CorteAsignacionVm edit)
    {
        var tundish = ListaTundishReales.FirstOrDefault(x => x.NumTundish == edit.NumTundish);
        var linea = tundish is null
            ? null
            : ObtenerLineasTundish(tundish).FirstOrDefault(x => x.Numero == edit.Linea && string.Equals(x.TipoSemi, edit.TipoSemi, StringComparison.OrdinalIgnoreCase));

        var longitudBase = edit.LongitudOriginal > 0 ? edit.LongitudOriginal : edit.Longitud;
        var capacidadAsignadaMm = edit.BarrasAsignadas > 0
            ? edit.BarrasAsignadas * longitudBase
            : (linea?.CapacidadMetros ?? 0) * 1000m;

        edit.BarrasAsignadas = CalcularCortesPosibles(capacidadAsignadaMm, edit.Longitud);
        edit.ToneladasAsignadas = Math.Round(edit.BarrasAsignadas * GetTnPorBarra(edit.TipoSemi, edit.Longitud), 2);
        edit.LongitudModificada = edit.LongitudOriginal > 0 && edit.Longitud != edit.LongitudOriginal;
        edit.BarrasFabricadas = Math.Max(0, edit.BarrasFabricadas);
    }

    /// <summary>
    /// On Manual Linea Changed.
    /// </summary>
    protected void OnManualLineaChanged(ManualColadaLineaVm linea)
    {
        linea.Longitud = Math.Max(0, linea.Longitud);
        linea.Barras = Math.Max(0, linea.Barras);
        linea.Produccion = Math.Max(0, linea.Produccion);
        linea.Toneladas = Math.Max(0, linea.Toneladas);
    }

    /// <summary>
    /// Puede Asignar Corte.
    /// </summary>
    private static bool PuedeAsignarCorte(LineaTundishVm linea, int longitud)
    {
        if (!linea.Abierta || longitud <= 0)
            return false;

        return linea.CapacidadRestanteMm >= longitud || linea.CapacidadRestanteMm >= longitud * 0.5m;
    }

    /// <summary>
    /// Calcular Cortes Posibles.
    /// </summary>
    private static int CalcularCortesPosibles(decimal capacidadRestanteMm, int longitud)
    {
        if (longitud <= 0 || capacidadRestanteMm <= 0)
            return 0;

        var cortes = (int)Math.Floor(capacidadRestanteMm / longitud);
        var restante = capacidadRestanteMm - cortes * longitud;
        if (restante >= longitud * 0.5m)
            cortes++;

        return cortes;
    }

    /// <summary>
    /// Obtener Lineas Tundish.
    /// </summary>
    private List<LineaTundishVm> ObtenerLineasTundish(ListTundishDisponibles tundish) =>
    [
        CrearLineaTundish(tundish, 1, tundish.tsCierre1, tundish.StatusBoca1),
        CrearLineaTundish(tundish, 2, tundish.tsCierre2, tundish.StatusBoca2),
        CrearLineaTundish(tundish, 3, tundish.tsCierre3, tundish.StatusBoca3),
        CrearLineaTundish(tundish, 4, tundish.tsCierre4, tundish.StatusBoca4),
        CrearLineaTundish(tundish, 5, tundish.tsCierre5, tundish.StatusBoca5),
        CrearLineaTundish(tundish, 6, tundish.tsCierre6, tundish.StatusBoca6)
    ];

    /// <summary>
    /// Crear Linea Tundish.
    /// </summary>
    private LineaTundishVm CrearLineaTundish(ListTundishDisponibles tundish, int numero, string tipoSemi, string estado)
    {
        var tipo = tipoSemi?.Trim().ToUpperInvariant() ?? string.Empty;
        var estadoNormalizado = estado?.Trim().ToUpperInvariant() ?? string.Empty;
        var capacidadMetros = CalcularCapacidadLinea(tipo, estadoNormalizado, tundish);
        return new LineaTundishVm(numero, tipo, estadoNormalizado, estadoNormalizado == "ABIERTA", capacidadMetros)
        {
            CapacidadRestanteMm = capacidadMetros * 1000m
        };
    }

    /// <summary>
    /// Crear Linea Cerrada.
    /// </summary>
    private static CorteAsignacionVm CrearLineaCerrada(string version, int numTundish, int numColada, string calidad, LineaTundishVm linea) => new()
    {
        Version = version,
        NumTundish = numTundish,
        NumColada = numColada,
        TipoSemi = "Cerrada",
        Linea = linea.Numero,
        LineaEstado = linea.Estado,
        Calidad = calidad,
        Longitud = 0,
        LongitudOriginal = 0,
        BarrasAsignadas = 0,
        BarrasFabricadas = 0,
        ToneladasAsignadas = 0
    };

    /// <summary>
    /// Es Calidad Normal.
    /// </summary>
    private static bool EsCalidadNormal(string calidad)
    {
        var value = calidad?.Trim().ToUpperInvariant().Replace("-", string.Empty) ?? string.Empty;
        return value == "S275H";
    }

    /// <summary>
    /// Recalcular Resumen Y Comparativa.
    /// </summary>
    private void RecalcularResumenYComparativa()
    {
        var asignaciones = AsignacionesActuales;
        ResumenCorteActual = versionDetail
            .Select(detalle =>
            {
                var id = GetResumenId(detalle);
                var asignadas = asignaciones
                    .Where(x => x.TipoSemi == detalle.TipoSemi && x.Calidad == detalle.Calidad && x.Longitud == detalle.Longitud)
                    .ToList();
                var asignadasBarras = asignadas.Sum(x => x.BarrasAsignadas);
                var fabricadas = asignadas.Sum(x => x.BarrasFabricadas);

                return new CorteResumenVm
                {
                    Id = id,
                    TipoSemi = detalle.TipoSemi,
                    Calidad = detalle.Calidad,
                    FechaPrevista = detalle.FechaPrevIni,
                    Longitud = detalle.Longitud,
                    NecesidadBarras = detalle.NumeroBarras,
                    AsignadasBarras = asignadasBarras,
                    FabricadasBarras = fabricadas,
                    PendientesBarras = Math.Max(0, detalle.NumeroBarras - asignadasBarras - fabricadas),
                    ToneladasNecesidad = Math.Round(detalle.NumeroBarras * GetTnPorBarra(detalle.TipoSemi, detalle.Longitud), 2),
                    ToneladasAsignadas = Math.Round(asignadas.Sum(x => x.ToneladasAsignadas), 2),
                    DetallesOrigen = ObtenerDetalleOrigenResumen(detalle)
                };
            })
            .ToList();

        ComparativaVersiones = VersionesCorte.Select(CalcularComparativa).ToList();
    }

    /// <summary>
    /// Calcular Comparativa.
    /// </summary>
    private ComparativaVersionVm CalcularComparativa(CorteVersionVm version)
    {
        var resumen = CrearResumen(version.Asignaciones);
        var necesidadTotal = resumen.Sum(x => x.ToneladasNecesidad);
        var asignadoTotal = resumen.Sum(x => Math.Min(x.ToneladasNecesidad, x.ToneladasAsignadas));
        var producidoTotal = resumen.Sum(x => x.ToneladasAsignadas);
        var capacidadTotal = ListaTundishReales.Where(x => x.TundishActivo).Sum(x => x.NumColadas * (x.TnXColadaBB1 + x.TnXColadaBB2 + x.TnXColadaBB3));

        var calidadScore = PromedioCobertura(resumen.GroupBy(x => x.Calidad), 25m);
        var medidaScore = PromedioCobertura(resumen.GroupBy(x => $"{x.TipoSemi}|{x.Calidad}|{x.Longitud}"), 20m);
        var cambiosScore = CalcularScoreCambios(version.Asignaciones);
        var yieldScore = capacidadTotal <= 0 ? 0 : Math.Min(20m, (asignadoTotal / capacidadTotal) * 20m);
        var tundishDisponibles = Math.Max(1, ListaTundishReales.Count(x => x.TundishActivo));
        var tundishUsados = version.Asignaciones.Select(x => x.NumTundish).Distinct().Count();
        var tundishScore = Math.Min(5m, (decimal)tundishUsados / tundishDisponibles * 5m);
        var deficitScore = necesidadTotal <= 0 ? 0 : Math.Min(10m, (asignadoTotal / necesidadTotal) * 10m);
        var sobreProduccion = producidoTotal <= 0 ? 0 : Math.Max(0, producidoTotal - necesidadTotal);
        var sobreScore = producidoTotal <= 0 ? 0 : Math.Max(0, 10m - (sobreProduccion / producidoTotal * 10m));

        return new ComparativaVersionVm
        {
            Version = version.Clave,
            AjusteCalidad = Math.Round(calidadScore, 2),
            AjusteMedida = Math.Round(medidaScore, 2),
            CambiosCalidad = Math.Round(cambiosScore, 2),
            YieldEstimado = Math.Round(yieldScore, 2),
            TundishConsumidos = Math.Round(tundishScore, 2),
            DeficitCalidad = Math.Round(deficitScore, 2),
            Sobreproduccion = Math.Round(sobreScore, 2),
            ScoreTotal = Math.Round(calidadScore + medidaScore + cambiosScore + yieldScore + tundishScore + deficitScore + sobreScore, 2)
        };
    }

    /// <summary>
    /// Crear Resumen.
    /// </summary>
    private List<CorteResumenVm> CrearResumen(List<CorteAsignacionVm> asignaciones)
    {
        return versionDetail.Select(detalle =>
        {
            var asignadas = asignaciones
                .Where(x => x.TipoSemi == detalle.TipoSemi && x.Calidad == detalle.Calidad && x.Longitud == detalle.Longitud)
                .ToList();
            return new CorteResumenVm
            {
                Id = GetResumenId(detalle),
                TipoSemi = detalle.TipoSemi,
                Calidad = detalle.Calidad,
                FechaPrevista = detalle.FechaPrevIni,
                Longitud = detalle.Longitud,
                NecesidadBarras = detalle.NumeroBarras,
                AsignadasBarras = asignadas.Sum(x => x.BarrasAsignadas),
                FabricadasBarras = asignadas.Sum(x => x.BarrasFabricadas),
                PendientesBarras = Math.Max(0, detalle.NumeroBarras - asignadas.Sum(x => x.BarrasAsignadas) - asignadas.Sum(x => x.BarrasFabricadas)),
                ToneladasNecesidad = Math.Round(detalle.NumeroBarras * GetTnPorBarra(detalle.TipoSemi, detalle.Longitud), 2),
                ToneladasAsignadas = Math.Round(asignadas.Sum(x => x.ToneladasAsignadas), 2),
                DetallesOrigen = ObtenerDetalleOrigenResumen(detalle)
            };
        }).ToList();
    }

    /// <summary>
    /// Obtener Detalle Origen Resumen.
    /// </summary>
    private List<CorteResumenDetalleOrigenVm> ObtenerDetalleOrigenResumen(DetalleVersionVm detalle)
    {
        return ListaNecesidadCompleta
            .Where(x => string.Equals((x.MatSemi ?? string.Empty).Trim(), detalle.TipoSemi, StringComparison.OrdinalIgnoreCase))
            .Where(x => string.Equals((x.CalidadSemi ?? string.Empty).Trim(), detalle.Calidad, StringComparison.OrdinalIgnoreCase))
            .Where(x => x.LongitudSemi == detalle.Longitud)
            .GroupBy(x => new
            {
                Familia = (x.Familia ?? string.Empty).Trim(),
                LongitudPT = x.Longitud,
                FechaNecesidad = x.FechaPrevIni.Date
            })
            .OrderBy(g => g.Key.FechaNecesidad)
            .ThenBy(g => g.Key.Familia)
            .ThenBy(g => g.Key.LongitudPT)
            .Select((g, index) => new CorteResumenDetalleOrigenVm
            {
                Id = $"{detalle.IdDetalle}-{index}",
                Familia = string.IsNullOrWhiteSpace(g.Key.Familia) ? "Sin familia" : g.Key.Familia,
                LongitudPT = g.Key.LongitudPT,
                Necesidad = g.Sum(x => x.UdsAFabSemi),
                FechaNecesidad = g.Key.FechaNecesidad
            })
            .ToList();
    }

    /// <summary>
    /// Promedio Cobertura.
    /// </summary>
    private static decimal PromedioCobertura(IEnumerable<IGrouping<string, CorteResumenVm>> grupos, decimal peso)
    {
        var lista = grupos.ToList();
        if (lista.Count == 0)
            return 0;

        return lista.Average(g =>
        {
            var necesidad = g.Sum(x => x.ToneladasNecesidad);
            if (necesidad <= 0)
                return 0;

            var asignadas = g.Sum(x => x.ToneladasAsignadas);
            return Math.Min(1m, asignadas / necesidad) * peso;
        });
    }

    /// <summary>
    /// Calcular Score Cambios.
    /// </summary>
    private static decimal CalcularScoreCambios(List<CorteAsignacionVm> asignaciones)
    {
        var grupos = asignaciones.GroupBy(x => x.NumTundish).ToList();
        if (grupos.Count == 0)
            return 0;

        return grupos.Average(g =>
        {
            var calidades = g.OrderBy(x => x.FechaPrevIni).ThenBy(x => x.Longitud).Select(x => x.Calidad).ToList();
            var cambios = 0;
            for (var i = 1; i < calidades.Count; i++)
            {
                if (!string.Equals(calidades[i], calidades[i - 1], StringComparison.OrdinalIgnoreCase))
                    cambios++;
            }

            return cambios switch
            {
                <= 1 => 10m,
                2 => 7m,
                3 => 5m,
                _ => 2m
            };
        });
    }

    /// <summary>
    /// Get Boca.
    /// </summary>
    protected BocaView GetBoca(int numeroBoca)
    {
        if (SelectedTundish is null)
            return new BocaView(string.Empty, "ABIERTA");

        return numeroBoca switch
        {
            1 => new BocaView(SelectedTundish.tsCierre1, SelectedTundish.StatusBoca1),
            2 => new BocaView(SelectedTundish.tsCierre2, SelectedTundish.StatusBoca2),
            3 => new BocaView(SelectedTundish.tsCierre3, SelectedTundish.StatusBoca3),
            4 => new BocaView(SelectedTundish.tsCierre4, SelectedTundish.StatusBoca4),
            5 => new BocaView(SelectedTundish.tsCierre5, SelectedTundish.StatusBoca5),
            6 => new BocaView(SelectedTundish.tsCierre6, SelectedTundish.StatusBoca6),
            _ => new BocaView(string.Empty, "ABIERTA")
        };
    }

    /// <summary>
    /// Get Tipo Css.
    /// </summary>
    protected static string GetTipoCss(string? tipo) => tipo?.Trim().ToUpperInvariant() switch
    {
        "BB1" => "bb1",
        "BB2" => "bb2",
        "BB3" => "bb3",
        _ => string.Empty
    };

    /// <summary>
    /// Get Estado Color.
    /// </summary>
    protected static string GetEstadoColor(string? estado) => estado?.Trim().ToUpperInvariant() switch
    {
        "ABIERTA" => "#31b404",
        "CERRADA" => "#ffbf00",
        "BLOQUEDA/PERFORADA" => "#ff8000",
        _ => "#e5e7eb"
    };

    /// <summary>
    /// Get Calidad Class.
    /// </summary>
    protected static string GetCalidadClass(string calidad)
    {
        return calidad?.Trim().ToUpperInvariant() switch
        {
            "S275-H" => "col-s275-h",
            "S355-TI" => "col-s355-ti",
            "S355-V" => "col-s355-v",
            "S355-W" => "col-s355-w",
            "S355W" => "col-s355-w",
            "S460-A" => "col-s460-a",
            "S460" => "col-s460-a",
            "S275-TI" => "col-s275-ti",
            _ => "col-default"
        };
    }

    /// <summary>
    /// Get Tipo Badge Class.
    /// </summary>
    protected static string GetTipoBadgeClass(string tipoSemi) => tipoSemi?.Trim().ToUpperInvariant() switch
    {
        "BB1" => "badge-bb1",
        "BB2" => "badge-bb2",
        "BB3" => "badge-bb3",
        _ => "badge-default"
    };

    /// <summary>
    /// Get Tipo Display.
    /// </summary>
    protected static string GetTipoDisplay(CorteAsignacionVm row) =>
        string.Equals(row.TipoSemi, "Cerrada", StringComparison.OrdinalIgnoreCase)
            ? "Cerrada"
            : row.TipoSemi;

    /// <summary>
    /// On Resumen Grid Customize Element.
    /// </summary>
    protected void OnResumenGridCustomizeElement(GridCustomizeElementEventArgs e)
    {
        if (e.ElementType != GridElementType.DataCell)
            return;

        var fieldName = (e.Column as IGridDataColumn)?.FieldName;
        if (e.Grid.GetDataItem(e.VisibleIndex) is not CorteResumenVm item)
            return;

        var cssClass = fieldName switch
        {
            nameof(CorteResumenVm.TipoSemi) => GetTipoBadgeClass(item.TipoSemi),
            nameof(CorteResumenVm.Calidad) => GetCalidadClass(item.Calidad),
            _ => string.Empty
        };

        if (!string.IsNullOrWhiteSpace(cssClass))
            e.CssClass = $"{e.CssClass} badge-cell {cssClass}".Trim();
    }

    /// <summary>
    /// On Asignacion Grid Customize Element.
    /// </summary>
    protected void OnAsignacionGridCustomizeElement(GridCustomizeElementEventArgs e)
    {
        if (e.ElementType != GridElementType.DataCell)
            return;

        var fieldName = (e.Column as IGridDataColumn)?.FieldName;
        if (e.Grid.GetDataItem(e.VisibleIndex) is not CorteAsignacionVm item)
            return;

        var cssClass = fieldName switch
        {
            nameof(CorteAsignacionVm.TipoSemi) => GetTipoBadgeClass(item.TipoSemi),
            nameof(CorteAsignacionVm.Calidad) => GetCalidadClass(item.Calidad),
            _ => string.Empty
        };

        if (!string.IsNullOrWhiteSpace(cssClass))
            e.CssClass = $"{e.CssClass} badge-cell {cssClass}".Trim();
    }

    /// <summary>
    /// Recalcular Capacidad Tundish.
    /// </summary>
    private void RecalcularCapacidadTundish(ListTundishDisponibles tundish)
    {
        var config = ListaConfiguracionAceria.FirstOrDefault();
        if (config is null)
            return;

        var tnHoraBB1 = config.caTnxHoraBB1 * tundish.tstotalBB1;
        var tnHoraBB2 = config.caTnxHoraBB2 * tundish.tstotalBB2;
        var tnHoraBB3 = config.caTnxHoraBB3 * tundish.tstotalBB3;
        var tnMinTotal = Math.Round((tnHoraBB1 + tnHoraBB2 + tnHoraBB3) / 60, 2);
        if (tnMinTotal <= 0)
        {
            tundish.NumColadas = 0;
            tundish.MinutosXColada = 0;
            tundish.TnXColadaBB1 = 0;
            tundish.TnXColadaBB2 = 0;
            tundish.TnXColadaBB3 = 0;
            return;
        }

        var minXColada = Math.Round(config.caToneladasXCuchara / tnMinTotal, 2);
        tundish.MinutosXColada = minXColada;
        tundish.TnXColadaBB1 = Math.Round(tundish.tstotalBB1 == 0 ? 0 : (tnHoraBB1 / 60) * minXColada, 2);
        tundish.TnXColadaBB2 = Math.Round(tundish.tstotalBB2 == 0 ? 0 : (tnHoraBB2 / 60) * minXColada, 2);
        tundish.TnXColadaBB3 = Math.Round(tundish.tstotalBB3 == 0 ? 0 : (tnHoraBB3 / 60) * minXColada, 2);
        tundish.NumColadas = tundish.TundishActivo && tundish.HorasVida > 0 ? Math.Max(0, Math.Ceiling((tundish.HorasVida * 60) / minXColada) - 1) : 0;
    }

    /// <summary>
    /// Configurar Boca.
    /// </summary>
    private static void ConfigurarBoca(ListTundishDisponibles tundish, string tipo, string estatusBoca, int numeroBoca)
    {
        var anterior = ObtenerStatusBocaActual(tundish, numeroBoca);
        switch (estatusBoca)
        {
            case "ABIERTA":
                AjustarTotal(tundish, tipo, +1);
                AsignarStatusBoca(tundish, numeroBoca, "ABIERTA");
                break;
            case "CERRADA":
                if (anterior != "BLOQUEDA/PERFORADA")
                    AjustarTotal(tundish, tipo, -1);
                AsignarStatusBoca(tundish, numeroBoca, "CERRADA");
                break;
            case "BLOQUEDA/PERFORADA":
                if (anterior != "CERRADA")
                    AjustarTotal(tundish, tipo, -1);
                AsignarStatusBoca(tundish, numeroBoca, "BLOQUEDA/PERFORADA");
                break;
        }
    }

    /// <summary>
    /// Ajustar Total.
    /// </summary>
    private static void AjustarTotal(ListTundishDisponibles tundish, string tipo, int delta)
    {
        switch (tipo)
        {
            case "BB1": tundish.tstotalBB1 = Math.Max(0, tundish.tstotalBB1 + delta); break;
            case "BB2": tundish.tstotalBB2 = Math.Max(0, tundish.tstotalBB2 + delta); break;
            case "BB3": tundish.tstotalBB3 = Math.Max(0, tundish.tstotalBB3 + delta); break;
        }
    }

    /// <summary>
    /// Obtener Status Boca Actual.
    /// </summary>
    private static string ObtenerStatusBocaActual(ListTundishDisponibles tundish, int numeroBoca) => numeroBoca switch
    {
        1 => tundish.StatusBoca1,
        2 => tundish.StatusBoca2,
        3 => tundish.StatusBoca3,
        4 => tundish.StatusBoca4,
        5 => tundish.StatusBoca5,
        6 => tundish.StatusBoca6,
        _ => string.Empty
    };

    /// <summary>
    /// Asignar Status Boca.
    /// </summary>
    private static void AsignarStatusBoca(ListTundishDisponibles tundish, int numeroBoca, string status)
    {
        switch (numeroBoca)
        {
            case 1: tundish.StatusBoca1 = status; break;
            case 2: tundish.StatusBoca2 = status; break;
            case 3: tundish.StatusBoca3 = status; break;
            case 4: tundish.StatusBoca4 = status; break;
            case 5: tundish.StatusBoca5 = status; break;
            case 6: tundish.StatusBoca6 = status; break;
        }
    }

    /// <summary>
    /// Obtener Segmentos Calidad.
    /// </summary>
    private IEnumerable<SegmentoCalidadVm> ObtenerSegmentosCalidad(ListaPropuestaDistribucion fila)
    {
        if (fila.S275H > 0) yield return new SegmentoCalidadVm("S275-H", fila.S275H);
        if (fila.S355TI > 0) yield return new SegmentoCalidadVm("S355-TI", fila.S355TI);
        if (fila.S355V > 0) yield return new SegmentoCalidadVm("S355-V", fila.S355V);
        if (fila.S355W > 0) yield return new SegmentoCalidadVm("S355-W", fila.S355W);
        if (fila.S460A > 0) yield return new SegmentoCalidadVm("S460-A", fila.S460A);
        if (fila.S275TI > 0) yield return new SegmentoCalidadVm("S275-TI", fila.S275TI);
    }

    /// <summary>
    /// Obtener Secuencia Coladas.
    /// </summary>
    private IEnumerable<string> ObtenerSecuenciaColadas(ListaPropuestaDistribucion fila)
    {
        foreach (var segmento in ObtenerSegmentosCalidad(fila))
        {
            for (var i = 0; i < segmento.Coladas; i++)
                yield return segmento.Calidad;
        }
    }

    /// <summary>
    /// Clone Asignacion.
    /// </summary>
    private static CorteAsignacionVm CloneAsignacion(CorteAsignacionVm source) => new()
    {
        Version = source.Version,
        NumTundish = source.NumTundish,
        NumColada = source.NumColada,
        TipoSemi = source.TipoSemi,
        Linea = source.Linea,
        LineaEstado = source.LineaEstado,
        Calidad = source.Calidad,
        Longitud = source.Longitud,
        LongitudOriginal = source.LongitudOriginal,
        LongitudModificada = source.LongitudModificada,
        BarrasAsignadas = source.BarrasAsignadas,
        BarrasFabricadas = source.BarrasFabricadas,
        ToneladasAsignadas = source.ToneladasAsignadas,
        SemanaPrevIni = source.SemanaPrevIni,
        FechaPrevIni = source.FechaPrevIni,
        EsManual = source.EsManual
    };

    /// <summary>
    /// Tundish Tiene Tipo.
    /// </summary>
    private static bool TundishTieneTipo(ListTundishDisponibles tundish, string tipo) => tipo switch
    {
        "BB1" => tundish.tstotalBB1 > 0 || tundish.TnXColadaBB1 > 0,
        "BB2" => tundish.tstotalBB2 > 0 || tundish.TnXColadaBB2 > 0,
        "BB3" => tundish.tstotalBB3 > 0 || tundish.TnXColadaBB3 > 0,
        _ => false
    };

    /// <summary>
    /// Get Tn X Colada.
    /// </summary>
    private static decimal GetTnXColada(ListTundishDisponibles tundish, string tipo) => tipo switch
    {
        "BB1" => tundish.TnXColadaBB1,
        "BB2" => tundish.TnXColadaBB2,
        "BB3" => tundish.TnXColadaBB3,
        _ => 0
    };

    /// <summary>
    /// Get Tn Por Barra.
    /// </summary>
    private decimal GetTnPorBarra(string tipoSemi, int longitud)
    {
        var pesoLineal = GetPesoLineal(tipoSemi);
        return Math.Round(((longitud / 1000m) * pesoLineal) / 1000m, 4);
    }

    /// <summary>
    /// Get Peso Lineal.
    /// </summary>
    private decimal GetPesoLineal(string tipoSemi)
    {
        var config = ListaConfiguracionAceria.FirstOrDefault();
        return tipoSemi switch
        {
            "BB1" => config?.caPesoLinealBB1 > 0 ? config.caPesoLinealBB1 : 0m,
            "BB2" => config?.caPesoLinealBB2 > 0 ? config.caPesoLinealBB2 : 0m,
            "BB3" => config?.caPesoLinealBB3 > 0 ? config.caPesoLinealBB3 : 0m,
            _ => 0
        };
    }

    /// <summary>
    /// Is Tipo Semi Valido.
    /// </summary>
    private bool IsTipoSemiValido(BeamBlankNecesidad item) => tipoSemiOrden.Contains(item.MatSemi?.Trim() ?? string.Empty);

    /// <summary>
    /// Get Resumen Id.
    /// </summary>
    private static string GetResumenId(DetalleVersionVm detalle) => $"{detalle.TipoSemi}|{detalle.Calidad}|{detalle.Longitud}";

    /// <summary>
    /// Limpieza.
    /// </summary>
    protected void Limpieza()
    {
        VersionesTundish = new ConfiguracionTundishControl();
        ListaTundishReales = new();
        ListaNecesidadDetalleBB = new();
        ListaNcBBAgrupago = new();
        ListaPropuestaDistribucionColadas = new();
        VersionesCorte = new();
        ResumenCorteActual = new();
        ComparativaVersiones = new();
        ArbolTundish = new();
        asignacionesManuales.Clear();
        SelectedTundish = null;
    }

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
    /// Clase CorteVersionVm. Codigo tras la vista de planificacion de cortes: ciclo de vida de coladas,
    /// calculo de capacidad por linea, asignaciones automaticas y ajustes manuales.
    /// </summary>
    protected sealed class CorteVersionVm
    {
        public string Clave { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public bool EsInicial { get; set; }
        public List<CorteAsignacionVm> Asignaciones { get; set; } = new();
    }

    /// <summary>
    /// Clase CorteAsignacionVm. Codigo tras la vista de planificacion de cortes: ciclo de vida de coladas,
    /// calculo de capacidad por linea, asignaciones automaticas y ajustes manuales.
    /// </summary>
    protected sealed class CorteAsignacionVm
    {
        public string Version { get; set; } = string.Empty;
        public int NumTundish { get; set; }
        public int NumColada { get; set; }
        public string TipoSemi { get; set; } = string.Empty;
        public int Linea { get; set; }
        public string LineaEstado { get; set; } = string.Empty;
        public string Calidad { get; set; } = string.Empty;
        public int Longitud { get; set; }
        public int LongitudOriginal { get; set; }
        public bool LongitudModificada { get; set; }
        public int BarrasAsignadas { get; set; }
        public int BarrasFabricadas { get; set; }
        public decimal ToneladasAsignadas { get; set; }
        public string SemanaPrevIni { get; set; } = string.Empty;
        public DateTime FechaPrevIni { get; set; }
        public bool EsManual { get; set; }
    }

    /// <summary>
    /// Clase CorteResumenVm. Codigo tras la vista de planificacion de cortes: ciclo de vida de coladas,
    /// calculo de capacidad por linea, asignaciones automaticas y ajustes manuales.
    /// </summary>
    protected sealed class CorteResumenVm
    {
        public string Id { get; set; } = string.Empty;
        public string TipoSemi { get; set; } = string.Empty;
        public string Calidad { get; set; } = string.Empty;
        public DateTime FechaPrevista { get; set; }
        public int Longitud { get; set; }
        public int NecesidadBarras { get; set; }
        public int AsignadasBarras { get; set; }
        public int FabricadasBarras { get; set; }
        public int PendientesBarras { get; set; }
        public decimal ToneladasNecesidad { get; set; }
        public decimal ToneladasAsignadas { get; set; }
        public List<CorteResumenDetalleOrigenVm> DetallesOrigen { get; set; } = new();
    }

    /// <summary>
    /// Clase CorteResumenDetalleOrigenVm. Codigo tras la vista de planificacion de cortes: ciclo de vida de
    /// coladas, calculo de capacidad por linea, asignaciones automaticas y ajustes manuales.
    /// </summary>
    protected sealed class CorteResumenDetalleOrigenVm
    {
        public string Id { get; set; } = string.Empty;
        public string Familia { get; set; } = string.Empty;
        public int LongitudPT { get; set; }
        public int Necesidad { get; set; }
        public DateTime FechaNecesidad { get; set; }
    }

    /// <summary>
    /// Clase ComparativaVersionVm. Codigo tras la vista de planificacion de cortes: ciclo de vida de
    /// coladas, calculo de capacidad por linea, asignaciones automaticas y ajustes manuales.
    /// </summary>
    protected sealed class ComparativaVersionVm
    {
        public string Version { get; set; } = string.Empty;
        public decimal AjusteCalidad { get; set; }
        public decimal AjusteMedida { get; set; }
        public decimal CambiosCalidad { get; set; }
        public decimal YieldEstimado { get; set; }
        public decimal TundishConsumidos { get; set; }
        public decimal DeficitCalidad { get; set; }
        public decimal Sobreproduccion { get; set; }
        public decimal ScoreTotal { get; set; }
    }

    /// <summary>
    /// Clase TundishTreeVm. Codigo tras la vista de planificacion de cortes: ciclo de vida de coladas,
    /// calculo de capacidad por linea, asignaciones automaticas y ajustes manuales.
    /// </summary>
    protected sealed class TundishTreeVm
    {
        public ListTundishDisponibles Tundish { get; set; } = new();
        public List<CorteVersionTreeVm> Versiones { get; set; } = new();
    }

    /// <summary>
    /// Clase CorteVersionTreeVm. Codigo tras la vista de planificacion de cortes: ciclo de vida de coladas,
    /// calculo de capacidad por linea, asignaciones automaticas y ajustes manuales.
    /// </summary>
    protected sealed class CorteVersionTreeVm
    {
        public string Version { get; set; } = string.Empty;
        public bool Activa { get; set; }
        public DateTime Fecha { get; set; }
        public int BarrasTeoricas { get; set; }
        public decimal Toneladas { get; set; }
        public string ResumenCortes { get; set; } = string.Empty;
        public decimal Score { get; set; }
        public List<ColadaTreeVm> Coladas { get; set; } = new();
    }

    /// <summary>
    /// Clase ColadaTreeVm. Codigo tras la vista de planificacion de cortes: ciclo de vida de coladas,
    /// calculo de capacidad por linea, asignaciones automaticas y ajustes manuales.
    /// </summary>
    protected sealed class ColadaTreeVm
    {
        public string Version { get; set; } = string.Empty;
        public int NumTundish { get; set; }
        public int Numero { get; set; }
        public string Calidad { get; set; } = string.Empty;
        public int Coladas { get; set; }
        public int Barras { get; set; }
        public decimal Toneladas { get; set; }
        public bool Cerrada { get; set; }
        public bool PuedeFinalizar { get; set; }
        public bool EsManual { get; set; }
        public List<CorteAsignacionVm> Perfiles { get; set; } = new();
    }

    /// <summary>
    /// Clase ManualColadaVm. Codigo tras la vista de planificacion de cortes: ciclo de vida de coladas,
    /// calculo de capacidad por linea, asignaciones automaticas y ajustes manuales.
    /// </summary>
    protected sealed class ManualColadaVm
    {
        public string Version { get; set; } = string.Empty;
        public int NumTundish { get; set; }
        public string NombreTundish { get; set; } = string.Empty;
        public int NumColada { get; set; }
        public string Calidad { get; set; } = string.Empty;
        public List<ManualColadaLineaVm> Lineas { get; set; } = new();
    }

    /// <summary>
    /// Clase ManualColadaLineaVm. Codigo tras la vista de planificacion de cortes: ciclo de vida de
    /// coladas, calculo de capacidad por linea, asignaciones automaticas y ajustes manuales.
    /// </summary>
    protected sealed class ManualColadaLineaVm
    {
        public int Linea { get; set; }
        public string TipoSemi { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public bool Abierta { get; set; }
        public int Longitud { get; set; }
        public int Barras { get; set; }
        public int Produccion { get; set; }
        public decimal Toneladas { get; set; }
    }

    protected readonly record struct BocaView(string Tipo, string Estado);
    protected readonly record struct BocaEstadoOption(string Text, string Value);
    protected readonly record struct TundishSelectorItem(int NumTundish, string DisplayText);
    private readonly record struct SegmentoCalidadVm(string Calidad, int Coladas);
    private sealed record class LineaTundishVm(int Numero, string TipoSemi, string Estado, bool Abierta, decimal CapacidadMetros)
    {
        public decimal CapacidadRestanteMm { get; set; }
    }
}
