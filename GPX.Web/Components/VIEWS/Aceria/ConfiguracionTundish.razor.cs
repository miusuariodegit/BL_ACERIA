
using DevExpress.Blazor;
using GPX.Negocio.Aceria;
using GPX.Negocio.COP;
using GPX.Negocio.CRUD;
using GPX.Negocio.ORM;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Globalization;
using System.Security.Claims;
using System.Text.Json;

// [GPX-DOC-v1] ================================================================================
// Codigo tras la vista de configuracion de tundish: calculo de necesidad, estandares, propuesta de
// distribucion de coladas y guardado de versiones.
// ================================================================================================

namespace GPX.Web.Components.VIEWS.Aceria;

/// <summary>
/// Clase ConfiguracionTundish. Codigo tras la vista de configuracion de tundish: calculo de necesidad,
/// estandares, propuesta de distribucion de coladas y guardado de versiones.
/// </summary>
public partial class ConfiguracionTundish : ComponentBase
{
    [Inject] protected CrudRepository CrudRepository { get; set; } = default!;
    [Inject] protected AceriaService AceriaService { get; set; } = default!;
    [Inject] protected ConfiguracionTundishService ConfiguracionTundishService { get; set; } = default!;
    [Inject] protected AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;
    [Inject] protected IDialogService DialogService { get; set; } = default!;

    protected bool IsLoading { get; set; }
    protected bool RestablecerTundish { get; set; }
    protected bool PopupConfiguracionVisible { get; set; }
    protected DateTime FechaInicio { get; set; } = DateTime.Now;
    protected DateTime FechaFin { get; set; } = DateTime.Now.AddDays(10);

    protected DateTime FechaInicioTundish { get; set; } = DateTime.Now;
    protected DateTime FechaFinTundish { get; set; } = DateTime.Now.AddDays(10);



    protected string EstadoVersion { get; set; } = "---";
    protected string EstadoVersionCss { get; set; } = string.Empty;

    protected int CantidadTundish { get; set; }
    protected int NumColadasReales { get; set; }
    protected int NumMinutosNeceReales { get; set; }
    protected decimal ColExtraStd { get; set; }
    protected decimal TnExtraBB1 { get; set; }
    protected decimal TnExtraBB2 { get; set; }
    protected decimal TnExtraBB3 { get; set; }

    protected ListTundishStandard? EstandarSeleccionado { get; set; }
    protected ConfiguracionTundishControl? VersionSeleccionada { get; set; }
    protected List<ListTundishStandard> ListaTundisStandar { get; set; } = new();
    protected List<ConfiguracionAceria> ListaConfiguracionAceria { get; set; } = new();
    protected List<BeamBlankNecesidad> ListaNecesidadesBeamBlanks { get; set; } = new();
    protected List<BeamBlankNecesidad> ListaNecesidadesBeamBlanksAgrupago { get; set; } = new();
    protected List<ListTundishDisponibles> ListaTundishReales { get; set; } = new();
    protected List<ListTundishDisponibles> ListaTundishRealesBk { get; set; } = new();
    protected List<ListDetalleNecesidadBB> ListaNecesidadDetalleBB { get; set; } = new();
    protected List<ListaPropuestaDistribucion> ListaPropuestaDistribucionColadas { get; set; } = new();
    protected List<ConfiguracionTundishControl> ListaConfiguracionTundishControl { get; set; } = new();
    protected IGrid? GridDetalleNecesidad { get; set; }
    protected IGrid? GridDistribucion { get; set; }

    protected ConfiguracionAceria? Config => ListaConfiguracionAceria.FirstOrDefault();
    protected IEnumerable<ListDetalleNecesidadBB> DetalleNecesidadGridData => ListaNecesidadDetalleBB.OrderBy(x => x.OrdenFab);

    protected decimal NecesidadBB1 => ListaNecesidadesBeamBlanksAgrupago.Where(IsBB1).Sum(x => x.TnsAFabSemi);
    protected decimal NecesidadBB2 => ListaNecesidadesBeamBlanksAgrupago.Where(IsBB2).Sum(x => x.TnsAFabSemi);
    protected decimal NecesidadBB3 => ListaNecesidadesBeamBlanksAgrupago.Where(IsBB3).Sum(x => x.TnsAFabSemi);
    protected decimal NecesidadTotal => ListaNecesidadesBeamBlanksAgrupago.Where(IsBB).Sum(x => x.TnsAFabSemi);
    protected int? EstandarSeleccionadoValue => EstandarSeleccionado?.tsId;
    protected long? VersionSeleccionadaValue => VersionSeleccionada?.IdVersion;
    //protected List<VersionComboItem> VersionItems => ListaConfiguracionTundishControl
    //    .Select(x => new VersionComboItem(x.IdVersion, $"V{x.IdVersion} | {x.UsuarioAutor} | {x.NecesidadTotal:N2}"))
    //    .ToList();
    protected bool PuedeGuardarVersionActual => VersionSeleccionada?.IdVersion > 0 && ListaTundishReales.Count > 0;

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    /// <summary>
    /// On Initialized (operacion asincrona).
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        await CargarCatalogosConstantes();
        Limpieza(false);
    }

    /// <summary>
    /// Cargar Catalogos Constantes.
    /// </summary>
    protected async Task CargarCatalogosConstantes()
    {
        ListaTundisStandar = (await CrudRepository.ConsultaTundishStandardAsync())
            .Select(MapTundishStandard)
            .OrderBy(x => x.tsPrioridad)
            .ThenBy(x => x.tsId)
            .ToList();

        ListaConfiguracionAceria = await CrudRepository.ConsultaConfiguracionAceriaAsync();
    }

    /// <summary>
    /// Limpieza.
    /// </summary>
    protected void Limpieza(bool limpiarCatalogos = true)
    {
        ListaTundishReales = new();
        ListaTundishRealesBk = new();
        EstandarSeleccionado = null;
        ListaNecesidadesBeamBlanks = new();
        ListaNecesidadesBeamBlanksAgrupago = new();
        ListaNecesidadDetalleBB = new();
        ListaPropuestaDistribucionColadas = new();
        ListaConfiguracionTundishControl = limpiarCatalogos ? new() : ListaConfiguracionTundishControl;
        VersionSeleccionada = null;
        CantidadTundish = 0;
        NumColadasReales = 0;
        NumMinutosNeceReales = 0;
        ColExtraStd = 0;
        TnExtraBB1 = 0;
        TnExtraBB2 = 0;
        TnExtraBB3 = 0;
        RestablecerTundish = false;
        ActualizarEstadoVersion("---");
    }

    /// <summary>
    /// On Fecha Inicio Changed.
    /// </summary>
    protected async Task OnFechaInicioChanged(DateTime fecha)
    {
        FechaInicio = fecha;
        Limpieza();
        await Task.CompletedTask;
    }

    /// <summary>
    /// On Fecha Fin Changed.
    /// </summary>
    protected async Task OnFechaFinChanged(DateTime fecha)
    {
        FechaFin = fecha;
        Limpieza();
        await Task.CompletedTask;
    }

    /// <summary>
    /// On Consultar Click.
    /// </summary>
    protected async Task OnConsultarClick()
    {
        try
        {
            IsLoading = true;
            var estandarActual = EstandarSeleccionado;
            Limpieza();
            EstandarSeleccionado = estandarActual;

            var necesidades = await AceriaService.DameNecesidadBeamBlankTrenV2Async(Constantes.SociedadXDefecto, "4");
            ListaNecesidadesBeamBlanks = necesidades
                .Where(x => x.FechaPrevIni.Date >= FechaInicio.Date && x.FechaPrevIni.Date <= FechaFin.Date)
                .ToList();

            ListaConfiguracionTundishControl = await ConfiguracionTundishService.CansultaVercionXRango(FechaInicio.Date, FechaFin.Date);
            ListaNecesidadesBeamBlanksAgrupago = ListaNecesidadesBeamBlanks
                .GroupBy(x => new { x.Sociedad, x.CodMaquina, x.MatSemi, x.CalidadSemi, x.LongitudSemi, x.SemanaPrevIni })
                .Select(x => new BeamBlankNecesidad
                {
                    numRegistro = x.Key.GetHashCode(),
                    Sociedad = x.Key.Sociedad,
                    CodMaquina = x.Key.CodMaquina,
                    MatSemi = x.Key.MatSemi,
                    CalidadSemi = x.Key.CalidadSemi,
                    LongitudSemi = x.Key.LongitudSemi,
                    TnsAFabSemi = x.Sum(y => y.TnsAFabSemi),
                    UdsAFabSemi = x.Sum(y => y.UdsAFabSemi),
                    FechaPrevIni=x.Min(y=> y.FechaPrevIni),
                    SemanaPrevIni = x.Key.SemanaPrevIni
                })
                .ToList();

            if (ListaConfiguracionTundishControl.Count > 0)
            {
                await ShowAlertAsync("Consulta", "Existen versiones disponibles en el rango seleccionado.", MessageBoxRenderStyle.Info);
            }
               
         
        }
        catch (Exception ex)
        {
            await ShowAlertAsync("Error", ex.Message, MessageBoxRenderStyle.Danger);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// On Estandar Inicial Changed.
    /// </summary>
    protected async Task OnEstandarInicialChanged(int? id)
    {
        EstandarSeleccionado = ListaTundisStandar.FirstOrDefault(x => x.tsId == id);
        await Task.CompletedTask;
    }

    /// <summary>
    /// On Calcular Tundish Click.
    /// </summary>
    protected async Task OnCalcularTundishClick()
    {
        try
        {
            IsLoading = true;

            if (ListaNecesidadesBeamBlanksAgrupago.Count == 0)
            {
                await ShowAlertAsync("Validacion", "Selecciona y consulta un rango para continuar.", MessageBoxRenderStyle.Warning);
                return;
            }

            if (EstandarSeleccionado is null)
            {
                await ShowAlertAsync("Validacion", "Es necesario seleccionar un estandar inicial.", MessageBoxRenderStyle.Warning);
                return;
            }

            if (RestablecerTundish || ListaTundishReales.Count == 0)
            {
                var horas = Math.Max(1, Convert.ToInt32(Math.Ceiling((FechaFinTundish.Date.AddDays(1) - FechaInicioTundish.Date).TotalHours)));
                ListaTundishReales = await AceriaService.ConsultaTundishDisponiblesAsync(horas, FechaInicioTundish.Date, "BB");
                RestablecerTundish = false;
            }

            if (ListaTundishReales.Count == 0)
            {
                await ShowAlertAsync("Validacion", "No hay tundish disponibles para las horas requeridas. verifique su rango.", MessageBoxRenderStyle.Warning);
                return;
            }

            CantidadTundish = ListaTundishReales.Count;
            CargarConfiguracionTundish();
            ActualizarEstadoVersion(ExistenCambios() ? "Existen cambios sin guardar." : $"Sin cambios en version-{VersionSeleccionada?.IdVersion ?? 0}");
        }
        catch (Exception ex)
        {
            await ShowAlertAsync("Error", ex.Message, MessageBoxRenderStyle.Danger);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Cargar Configuracion Tundish.
    /// </summary>
    protected void CargarConfiguracionTundish()
    {
        for (var i = 0; i < ListaTundishReales.Count; i++)
        {
            var tundish = ListaTundishReales[i];
            tundish.NumTundish = tundish.NumTundish == 0 ? i + 1 : tundish.NumTundish;
            tundish.NombreTundish = $"UcTundish_{tundish.NumTundish}";
            tundish.EstandardSeleccionado ??= EstandarSeleccionado;
            CargarConfiguracionRendimiento(tundish, tundish.EstandardSeleccionado);
        }

        RecalcularDetalleYPropuesta();
    }

    /// <summary>
    /// Cargar Configuracion Rendimiento.
    /// </summary>
    public void CargarConfiguracionRendimiento(ListTundishDisponibles Tundish, ListTundishStandard? tundishEstandar)
    {
        if (Tundish is null || tundishEstandar is null || ListaConfiguracionAceria.Count == 0)
            return;

        Tundish.EstandardSeleccionado = tundishEstandar;

        if (Tundish.tstotalBB1 == 0 && Tundish.tstotalBB2 == 0 && Tundish.tstotalBB3 == 0)
            CargarDatosEstandar(Tundish, tundishEstandar);

        Tundish.tsCierre1 = tundishEstandar.tsCierre1.Trim();
        Tundish.tsCierre2 = tundishEstandar.tsCierre2.Trim();
        Tundish.tsCierre3 = tundishEstandar.tsCierre3.Trim();
        Tundish.tsCierre4 = tundishEstandar.tsCierre4.Trim();
        Tundish.tsCierre5 = tundishEstandar.tsCierre5.Trim();
        Tundish.tsCierre6 = tundishEstandar.tsCierre6.Trim();

        Tundish.StatusBoca1 = "ABIERTA";
        Tundish.StatusBoca2 = "ABIERTA";
        Tundish.StatusBoca3 = "ABIERTA";
        Tundish.StatusBoca4 = "ABIERTA";
        Tundish.StatusBoca5 = "ABIERTA";
        Tundish.StatusBoca6 = "ABIERTA";

        Tundish.tstotalBB1 = tundishEstandar.tstotalBB1;
        Tundish.tstotalBB2 = tundishEstandar.tstotalBB2;
        Tundish.tstotalBB3 = tundishEstandar.tstotalBB3;

        var configuracion = ListaConfiguracionAceria[0];
        var tnXCuchara = Convert.ToDecimal(configuracion.caToneladasXCuchara, CultureInfo.InvariantCulture);

        var tnHoraBB1 = configuracion.caTnxHoraBB1 * Tundish.tstotalBB1;
        var tnHoraBB2 = configuracion.caTnxHoraBB2 * Tundish.tstotalBB2;
        var tnHoraBB3 = configuracion.caTnxHoraBB3 * Tundish.tstotalBB3;

        var tnXminutoBB1 = Math.Round(tnHoraBB1 / 60, 2);
        var tnXminutoBB2 = Math.Round(tnHoraBB2 / 60, 2);
        var tnXminutoBB3 = Math.Round(tnHoraBB3 / 60, 2);
        var tnXminutoTotal = Math.Round((tnHoraBB1 + tnHoraBB2 + tnHoraBB3) / 60, 2);

        if (tnXminutoTotal <= 0)
        {
            Tundish.MinutosXColada = 0;
            Tundish.TnXColadaBB1 = 0;
            Tundish.TnXColadaBB2 = 0;
            Tundish.TnXColadaBB3 = 0;
            Tundish.NumColadas = 0;
            return;
        }

        var minXColada = Math.Round(tnXCuchara / tnXminutoTotal, 2);
        Tundish.MinutosXColada = minXColada;
        Tundish.TnXColadaBB1 = Math.Round(Tundish.tstotalBB1 == 0 ? 0 : tnXminutoBB1 * minXColada, 2);
        Tundish.TnXColadaBB2 = Math.Round(Tundish.tstotalBB2 == 0 ? 0 : tnXminutoBB2 * minXColada, 2);
        Tundish.TnXColadaBB3 = Math.Round(Tundish.tstotalBB3 == 0 ? 0 : tnXminutoBB3 * minXColada, 2);

        if (Tundish.HorasVida > 0)
        {
            Tundish.NumColadas = Math.Ceiling((Tundish.HorasVida * 60) / minXColada);
            if (Tundish.NumColadas > 1)
                Tundish.NumColadas--;
        }

        if (!Tundish.TundishActivo)
            Tundish.NumColadas = 0;
    }

    /// <summary>
    /// Cargar Datos Estandar.
    /// </summary>
    private static void CargarDatosEstandar(ListTundishDisponibles tundish, ListTundishStandard tundishEstandar)
    {
        tundish.tsCierre1 = tundishEstandar.tsCierre1.Trim();
        tundish.tsCierre2 = tundishEstandar.tsCierre2.Trim();
        tundish.tsCierre3 = tundishEstandar.tsCierre3.Trim();
        tundish.tsCierre4 = tundishEstandar.tsCierre4.Trim();
        tundish.tsCierre5 = tundishEstandar.tsCierre5.Trim();
        tundish.tsCierre6 = tundishEstandar.tsCierre6.Trim();

        tundish.StatusBoca1 = "ABIERTA";
        tundish.StatusBoca2 = "ABIERTA";
        tundish.StatusBoca3 = "ABIERTA";
        tundish.StatusBoca4 = "ABIERTA";
        tundish.StatusBoca5 = "ABIERTA";
        tundish.StatusBoca6 = "ABIERTA";

        tundish.tstotalBB1 = tundishEstandar.tstotalBB1;
        tundish.tstotalBB2 = tundishEstandar.tstotalBB2;
        tundish.tstotalBB3 = tundishEstandar.tstotalBB3;
    }


    /// <summary>
    /// On Tundish Changed.
    /// </summary>
    protected async Task OnTundishChanged(ListTundishDisponibles _)
    {
        RecalcularDetalleYPropuesta();
        ActualizarEstadoVersion(ExistenCambios() ? "Existen cambios sin guardar." : $"Sin cambios en version-{VersionSeleccionada?.IdVersion ?? 0}");
        await Task.CompletedTask;
    }

    /// <summary>
    /// Recalcular Detalle Y Propuesta.
    /// </summary>
    protected void RecalcularDetalleYPropuesta()
    {
        var medianaTnXColadaBB1 = CalcularMediana(ListaTundishReales.Where(x => x.TnXColadaBB1 > 0).Select(x => x.TnXColadaBB1).ToList());
        var medianaTnXColadaBB2 = CalcularMediana(ListaTundishReales.Where(x => x.TnXColadaBB2 > 0).Select(x => x.TnXColadaBB2).ToList());
        var medianaTnXColadaBB3 = CalcularMediana(ListaTundishReales.Where(x => x.TnXColadaBB3 > 0).Select(x => x.TnXColadaBB3).ToList());
        var medianaMinXColada = CalcularMediana(ListaTundishReales.Where(x => x.MinutosXColada > 0).Select(x => x.MinutosXColada).ToList());

        CalcularDetalleNecesidad(medianaTnXColadaBB1, medianaTnXColadaBB2, medianaTnXColadaBB3, medianaMinXColada);
        CalcularPropuestaDistribucion();
    }

    /// <summary>
    /// Calcular Detalle Necesidad.
    /// </summary>
    protected void CalcularDetalleNecesidad(decimal tnXColadaBB1, decimal tnXColadaBB2, decimal tnXColadaBB3, decimal minXColada)
    {
        ListaNecesidadDetalleBB = new();
        NumColadasReales = 0;
        NumMinutosNeceReales = 0;
        ColExtraStd = 0;
        TnExtraBB1 = 0;
        TnExtraBB2 = 0;
        TnExtraBB3 = 0;

        if (ListaNecesidadesBeamBlanks.Count == 0)
            return;

        var hayBB1 = ListaTundishReales.Exists(x => TundishTieneBB1(x));
        var hayBB2 = ListaTundishReales.Exists(x => TundishTieneBB2(x));
        var hayBB3 = ListaTundishReales.Exists(x => TundishTieneBB3(x));
        var counter = 1;

        ListaNecesidadDetalleBB = ListaNecesidadesBeamBlanks
            .Where(x => IsBB(x))
            .OrderBy(x => x.CalidadSemi)
            .GroupBy(x => new { TipoBB = x.MatSemi?.Trim() ?? string.Empty, Calidad = x.CalidadSemi ?? string.Empty })
            .Select(g => new ListDetalleNecesidadBB
            {
                OrdenFab = counter++,
                Calidad = g.Key.Calidad,
                TipoBB = g.Key.TipoBB,
                TnNecesidad = g.Sum(y => y.TnsAFabSemi)
            })
            .ToList();

        foreach (var calidad in ListaNecesidadDetalleBB.Select(x => x.Calidad).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToList())
        {
            AgregarTipoFaltante(calidad, "BB1", hayBB1, ref counter);
            AgregarTipoFaltante(calidad, "BB2", hayBB2, ref counter);
            AgregarTipoFaltante(calidad, "BB3", hayBB3, ref counter);
        }

        foreach (var item in ListaNecesidadDetalleBB)
        {
            item.ColadasNecesarias = item.TipoBB switch
            {
                "BB1" when tnXColadaBB1 > 0 => Math.Ceiling(item.TnNecesidad / tnXColadaBB1),
                "BB2" when tnXColadaBB2 > 0 => Math.Ceiling(item.TnNecesidad / tnXColadaBB2),
                "BB3" when tnXColadaBB3 > 0 => Math.Ceiling(item.TnNecesidad / tnXColadaBB3),
                _ => 0
            };
        }

        ListaNecesidadDetalleBB = ListaNecesidadDetalleBB.OrderBy(x => x.Calidad).ThenBy(x => x.TipoBB).ToList();
        for (var i = 0; i < ListaNecesidadDetalleBB.Count; i++)
            ListaNecesidadDetalleBB[i].OrdenFab = i + 1;

        var coladasTotalesAllTundish = ListaTundishReales.Sum(x => x.NumColadas);
        var coladasTotalesBB3 = ListaTundishReales.Where(x => TundishTieneBB3(x)).Sum(x => x.NumColadas);

        NumColadasReales = Convert.ToInt32(ListaNecesidadDetalleBB.GroupBy(x => x.Calidad).Sum(g => g.Max(x => x.ColadasNecesarias)));
        NumMinutosNeceReales = Convert.ToInt32(Math.Ceiling(NumColadasReales * minXColada));
        ColExtraStd = Math.Max(0, coladasTotalesAllTundish - NumColadasReales);
        TnExtraBB1 = ColExtraStd * tnXColadaBB1;
        TnExtraBB2 = ColExtraStd * tnXColadaBB2;
        TnExtraBB3 = coladasTotalesBB3 * tnXColadaBB3;
    }

    /// <summary>
    /// Calcular Propuesta Distribucion.
    /// </summary>
    protected void CalcularPropuestaDistribucion()
    {
        ListaPropuestaDistribucionColadas = new();
        var tundishActivos = ListaTundishReales.Where(x => x.TundishActivo).OrderBy(x => x.NumTundish).ToList();
        if (tundishActivos.Count == 0 || ListaNecesidadDetalleBB.Count == 0)
            return;

        var propuesta = tundishActivos.Select(x => new ListaPropuestaDistribucion
        {
            NumTRegistro = x.NumTundish,
            NumTundish = x.NumTundish,
            FechaInicioTundis = x.FechaInicio,
            FechaFinTundis = x.FechaFin,
            NumColadas = x.NumColadas,
            VidaUtil = x.HorasVida
        }).ToList();

        var propuestaPorTundish = propuesta.ToDictionary(x => x.NumTundish, x => x);
        var ordenCalidadesEspeciales = new[] { "S355-TI", "S355-V", "S355W", "S460-A", "S275-TI" };
        var pendientes = ListaNecesidadDetalleBB.ToDictionary(x => $"{x.Calidad}|{x.TipoBB}", x => Convert.ToInt32(x.ColadasNecesarias));

        int GetPendiente(string calidad, string tipo) => pendientes.TryGetValue($"{calidad}|{tipo}", out var value) ? value : 0;
        void RestarPendiente(string calidad, string tipo, int cantidad)
        {
            var key = $"{calidad}|{tipo}";
            if (pendientes.ContainsKey(key))
                pendientes[key] = Math.Max(0, pendientes[key] - cantidad);
        }

        foreach (var tundish in tundishActivos)
        {
            var fila = propuestaPorTundish[tundish.NumTundish];
            var capacidad = Convert.ToInt32(tundish.NumColadas);
            var tieneBB1 = TundishTieneBB1(tundish);
            var tieneBB2 = TundishTieneBB2(tundish);
            var tieneBB3 = TundishTieneBB3(tundish);

            if (tieneBB3)
            {
                foreach (var calidad in ordenCalidadesEspeciales)
                {
                    if (capacidad <= 0 || GetPendiente(calidad, "BB3") <= 0)
                        continue;

                    var pendientesActivos = new List<int>();
                    if (tieneBB1 && GetPendiente(calidad, "BB1") > 0) pendientesActivos.Add(GetPendiente(calidad, "BB1"));
                    if (tieneBB2 && GetPendiente(calidad, "BB2") > 0) pendientesActivos.Add(GetPendiente(calidad, "BB2"));
                    if (tieneBB3 && GetPendiente(calidad, "BB3") > 0) pendientesActivos.Add(GetPendiente(calidad, "BB3"));
                    if (pendientesActivos.Count == 0)
                        continue;

                    var coladasAsignar = Math.Min(capacidad, pendientesActivos.Min());
                    SetValorEspecialFila(fila, calidad, GetValorEspecialFila(fila, calidad) + coladasAsignar);
                    if (tieneBB1) RestarPendiente(calidad, "BB1", coladasAsignar);
                    if (tieneBB2) RestarPendiente(calidad, "BB2", coladasAsignar);
                    if (tieneBB3) RestarPendiente(calidad, "BB3", coladasAsignar);
                    capacidad -= coladasAsignar;
                }

                fila.S275H += Math.Max(0, capacidad);
                continue;
            }

            string? calidadSeleccionada = null;
            var maxPendiente = 0;
            foreach (var calidad in ordenCalidadesEspeciales)
            {
                var pendientesActivos = new List<int>();
                if (tieneBB1) pendientesActivos.Add(GetPendiente(calidad, "BB1"));
                if (tieneBB2) pendientesActivos.Add(GetPendiente(calidad, "BB2"));
                var mayorPendiente = pendientesActivos.Count > 0 ? pendientesActivos.Max() : 0;
                if (mayorPendiente > 0)
                {
                    calidadSeleccionada = calidad;
                    maxPendiente = mayorPendiente;
                    break;
                }
            }

            if (!string.IsNullOrWhiteSpace(calidadSeleccionada))
            {
                var coladasAsignar = Math.Min(capacidad, maxPendiente);
                SetValorEspecialFila(fila, calidadSeleccionada, coladasAsignar);
                if (tieneBB1) RestarPendiente(calidadSeleccionada, "BB1", coladasAsignar);
                if (tieneBB2) RestarPendiente(calidadSeleccionada, "BB2", coladasAsignar);
                capacidad -= coladasAsignar;
            }

            fila.S275H += Math.Max(0, capacidad);
        }

        AplicarPropuestaADetalle(propuestaPorTundish, tundishActivos);
        ListaPropuestaDistribucionColadas = propuesta;
    }

    /// <summary>
    /// Aplicar Propuesta A Detalle.
    /// </summary>
    protected void AplicarPropuestaADetalle(Dictionary<int, ListaPropuestaDistribucion> propuestaPorTundish, List<ListTundishDisponibles> tundishActivos)
    {
        foreach (var item in ListaNecesidadDetalleBB)
            item.ColadasReales = 0;

        foreach (var tundish in tundishActivos)
        {
            var fila = propuestaPorTundish[tundish.NumTundish];
            void Sumar(string calidad, int coladas)
            {
                if (coladas <= 0) return;
                if (TundishTieneBB1(tundish))
                    SumarSiExiste(calidad, "BB1", coladas);
                if (TundishTieneBB2(tundish))
                    SumarSiExiste(calidad, "BB2", coladas);
                if (TundishTieneBB3(tundish))
                    SumarSiExiste(calidad, "BB3", coladas);
            }

            Sumar("S275-H", fila.S275H);
            Sumar("S355-TI", fila.S355TI);
            Sumar("S355-V", fila.S355V);
            Sumar("S355W", fila.S355W);
            Sumar("S460-A", fila.S460A);
            Sumar("S275-TI", fila.S275TI);
        }

        var medianaTnXColadaBB1 = CalcularMediana(ListaTundishReales.Where(x => x.TnXColadaBB1 > 0).Select(x => x.TnXColadaBB1).ToList());
        var medianaTnXColadaBB2 = CalcularMediana(ListaTundishReales.Where(x => x.TnXColadaBB2 > 0).Select(x => x.TnXColadaBB2).ToList());
        var medianaTnXColadaBB3 = CalcularMediana(ListaTundishReales.Where(x => x.TnXColadaBB3 > 0).Select(x => x.TnXColadaBB3).ToList());
        var medianaMinXColada = CalcularMediana(ListaTundishReales.Where(x => x.MinutosXColada > 0).Select(x => x.MinutosXColada).ToList());

        foreach (var item in ListaNecesidadDetalleBB)
        {
            item.TnReales = item.TipoBB switch
            {
                "BB1" => item.ColadasReales * medianaTnXColadaBB1,
                "BB2" => item.ColadasReales * medianaTnXColadaBB2,
                "BB3" => item.ColadasReales * medianaTnXColadaBB3,
                _ => 0
            };
        }

        NumColadasReales = Convert.ToInt32(tundishActivos.Sum(x => x.NumColadas));
        NumMinutosNeceReales = Convert.ToInt32(Math.Ceiling(NumColadasReales * medianaMinXColada));
    }

    /// <summary>
    /// Sumar Si Existe.
    /// </summary>
    protected void SumarSiExiste(string calidad, string tipo, int coladas)
    {
        var registro = ListaNecesidadDetalleBB.FirstOrDefault(x => x.Calidad == calidad && x.TipoBB == tipo);
        if (registro is not null)
            registro.ColadasReales += coladas;
    }

    /// <summary>
    /// On Guardar Version Click.
    /// </summary>
    protected async Task OnGuardarVersionClick(bool nuevaVersion)
    {
        try
        {
            IsLoading = true;

            if (ListaTundishReales.Count == 0)
                return;

            var datos = await CrearVersionSerializada(nuevaVersion);
            if (nuevaVersion)
                await CrudRepository.AltaConfiguracionTundishControlAsync(datos);
            else
            {
                datos.IdVersion = VersionSeleccionada?.IdVersion ?? 0;
                await CrudRepository.ActualizarConfiguracionTundishControlAsync(datos);
            }

            VersionSeleccionada = datos;
            ListaTundishRealesBk = Clone(ListaTundishReales);
            ListaConfiguracionTundishControl = await ConfiguracionTundishService.CansultaVercionXRango(FechaInicio.Date, FechaFin.Date);
            ActualizarEstadoVersion(nuevaVersion ? $"Version Guardada {DateTime.Now:dd/MM/yyyy HH:mm}" : $"Version actualizada {DateTime.Now:dd/MM/yyyy HH:mm}");
            await ShowAlertAsync("Guardado", "Version guardada correctamente.", MessageBoxRenderStyle.Success);
        }
        catch (Exception ex)
        {
            await ShowAlertAsync("Error", ex.Message, MessageBoxRenderStyle.Danger);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Crear Version Serializada.
    /// </summary>
    protected async Task<ConfiguracionTundishControl> CrearVersionSerializada(bool nuevaVersion)
    {
        var auth = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        var usuario = auth.User.FindFirst(ClaimTypes.Email)?.Value ?? auth.User.Identity?.Name ?? "Blazor";

        return new ConfiguracionTundishControl
        {
            IdVersion = nuevaVersion ? 0 : VersionSeleccionada?.IdVersion ?? 0,
            UsuarioAutor = usuario,
            FechaCreacionVersion = nuevaVersion ? DateTime.Now : VersionSeleccionada?.FechaCreacionVersion ?? DateTime.Now,
            FechaUltimaModificacion = DateTime.Now,
            NecesidadfechaInicio = FechaInicio.Date,
            NecesidadfechaFin = FechaFin.Date,
            NecesidadBB1 = NecesidadBB1,
            NecesidadBB2 = NecesidadBB2,
            NecesidadBB3 = NecesidadBB3,
            NecesidadTotal = NecesidadTotal,
            NecesidadBB1Real = ListaNecesidadDetalleBB.Where(x => x.TipoBB == "BB1").Sum(x => x.TnReales),
            NecesidadBB2Real= ListaNecesidadDetalleBB.Where(x => x.TipoBB == "BB2").Sum(x => x.TnReales),
            NecesidadBB3Real = ListaNecesidadDetalleBB.Where(x => x.TipoBB == "BB3").Sum(x => x.TnReales),
            NecesidadTotalReal =  ListaNecesidadDetalleBB.Where(x => x.TipoBB == "BB1" || x.TipoBB == "BB2" || x.TipoBB == "BB3").Sum(x => x.TnReales),

            EstandarSeleccionado = JsonSerializer.Serialize(EstandarSeleccionado, JsonOptions),
            ListaConfiguracionAceria = JsonSerializer.Serialize(ListaConfiguracionAceria, JsonOptions),
            ListaNecesidadesBeamBlanks = JsonSerializer.Serialize(ListaNecesidadesBeamBlanks, JsonOptions),
            ListaNecesidadesBeamBlanksAgrupago = JsonSerializer.Serialize(ListaNecesidadesBeamBlanksAgrupago, JsonOptions),
            ListaTundishReales = JsonSerializer.Serialize(ListaTundishReales, JsonOptions),
            ListaNecesidadDetalleBB = JsonSerializer.Serialize(ListaNecesidadDetalleBB, JsonOptions),
            ListaPropuestaDistribucionColadas = JsonSerializer.Serialize(ListaPropuestaDistribucionColadas, JsonOptions),
            CantidadTundish = CantidadTundish,
            NumColadasReales = NumColadasReales,
            NumMinutosNeceReales = NumMinutosNeceReales
        };
    }

    /// <summary>
    /// On Version Changed.
    /// </summary>
    protected async Task OnVersionChanged(long? id)
    {
        VersionSeleccionada = ListaConfiguracionTundishControl.FirstOrDefault(x => x.IdVersion == id);
        if (VersionSeleccionada is null)
            return;

        CargarVersionSelecionada();
        await Task.CompletedTask;
    }

    /// <summary>
    /// Cargar Version Selecionada.
    /// </summary>
    protected void CargarVersionSelecionada()
    {
        if (VersionSeleccionada is null || VersionSeleccionada.IdVersion <= 0)
            return;

        EstandarSeleccionado = Deserialize<ListTundishStandard>(VersionSeleccionada.EstandarSeleccionado);
        ListaConfiguracionAceria = Deserialize<List<ConfiguracionAceria>>(VersionSeleccionada.ListaConfiguracionAceria) ?? new();
        ListaNecesidadesBeamBlanks = Deserialize<List<BeamBlankNecesidad>>(VersionSeleccionada.ListaNecesidadesBeamBlanks) ?? new();
        ListaNecesidadesBeamBlanksAgrupago = Deserialize<List<BeamBlankNecesidad>>(VersionSeleccionada.ListaNecesidadesBeamBlanksAgrupago) ?? new();
        ListaTundishReales = Deserialize<List<ListTundishDisponibles>>(VersionSeleccionada.ListaTundishReales) ?? new();
        ListaNecesidadDetalleBB = Deserialize<List<ListDetalleNecesidadBB>>(VersionSeleccionada.ListaNecesidadDetalleBB) ?? new();
        ListaPropuestaDistribucionColadas = Deserialize<List<ListaPropuestaDistribucion>>(VersionSeleccionada.ListaPropuestaDistribucionColadas) ?? new();
        CantidadTundish = VersionSeleccionada.CantidadTundish;
        NumColadasReales = VersionSeleccionada.NumColadasReales;
        NumMinutosNeceReales = VersionSeleccionada.NumMinutosNeceReales;
        ListaTundishRealesBk = Clone(ListaTundishReales);
        ActualizarEstadoVersion($"Version {VersionSeleccionada.IdVersion} - {DateTime.Now:dd/MM/yyyy HH:mm}");
    }

    /// <summary>
    /// On Distribucion Changed.
    /// </summary>
    protected async Task OnDistribucionChanged(ListaPropuestaDistribucion fila, string campo, int valor)
    {
        if (valor < 0)
        {
            await ShowAlertAsync("Validacion", "El valor debe ser un entero mayor o igual a 0.", MessageBoxRenderStyle.Warning);
            return;
        }

        var anterior = GetCampoDistribucion(fila, campo);
        SetCampoDistribucion(fila, campo, valor);
        var error = ValidarFilaDistribucion(fila);
        if (!string.IsNullOrWhiteSpace(error))
        {
            SetCampoDistribucion(fila, campo, anterior);
            await ShowAlertAsync("Validacion", error, MessageBoxRenderStyle.Warning);
            return;
        }

        var activos = ListaTundishReales.Where(x => x.TundishActivo).OrderBy(x => x.NumTundish).ToList();
        AplicarPropuestaADetalle(ListaPropuestaDistribucionColadas.ToDictionary(x => x.NumTundish, x => x), activos);
        ActualizarEstadoVersion("Existen cambios sin guardar.");
        await Task.CompletedTask;
    }

    /// <summary>
    /// On Distribucion Edit Model Saving.
    /// </summary>
    protected async Task OnDistribucionEditModelSaving(GridEditModelSavingEventArgs e)
    {
        var edit = (ListaPropuestaDistribucion)e.EditModel;
        if (edit.S275H < 0 || edit.S355TI < 0 || edit.S355V < 0 || edit.S355W < 0 || edit.S460A < 0 || edit.S275TI < 0)
        {
            e.Cancel = true;
            await ShowAlertAsync("Validacion", "Todos los valores de distribucion deben ser enteros mayores o iguales a 0.", MessageBoxRenderStyle.Warning);
            return;
        }

        var original = ListaPropuestaDistribucionColadas.FirstOrDefault(x => x.NumTRegistro == edit.NumTRegistro);
        if (original is null)
        {
            e.Cancel = true;
            await ShowAlertAsync("Validacion", "No se encontro la fila de distribucion seleccionada.", MessageBoxRenderStyle.Warning);
            return;
        }

        var propuestaTemporal = ListaPropuestaDistribucionColadas
            .Select(CloneDistribucion)
            .ToList();
        var filaTemporal = propuestaTemporal.First(x => x.NumTRegistro == edit.NumTRegistro);
        CopiarValoresEditables(edit, filaTemporal);

        var error = ValidarFilaDistribucion(filaTemporal, propuestaTemporal);
        if (!string.IsNullOrWhiteSpace(error))
        {
            e.Cancel = true;
            await ShowAlertAsync("Validacion", error, MessageBoxRenderStyle.Warning);
            return;
        }

        CopiarValoresEditables(edit, original);

        var activos = ListaTundishReales.Where(x => x.TundishActivo).OrderBy(x => x.NumTundish).ToList();
        AplicarPropuestaADetalle(ListaPropuestaDistribucionColadas.ToDictionary(x => x.NumTundish, x => x), activos);
        ActualizarEstadoVersion("Existen cambios sin guardar.");
    }

    /// <summary>
    /// On Detalle Grid Customize Element.
    /// </summary>
    private void OnDetalleGridCustomizeElement(GridCustomizeElementEventArgs e)
    {
        if (e.ElementType != GridElementType.DataCell)
            return;

        if (e.Column is not DxGridDataColumn column)
            return;

        if (column.FieldName != nameof(ListDetalleNecesidadBB.Calidad))
            return;

        if (e.Grid.GetDataItem(e.VisibleIndex) is not ListDetalleNecesidadBB item)
            return;

        var cssClass = item.Calidad?.Trim().ToUpperInvariant() switch
        {
            "S275-H" => "calidad-s275-h",
            "S355-TI" => "calidad-s355-ti",
            "S355-V" => "calidad-s355-v",
            "S355W" => "calidad-s355-w",
            "S460-A" => "calidad-s460-a",
            "S460" => "calidad-s460",
            "S275-TI" => "calidad-s275-ti",
            _ => string.Empty
        };

        if (!string.IsNullOrWhiteSpace(cssClass))
            e.CssClass = $"{e.CssClass} {cssClass}".Trim();
    }

    /// <summary>
    /// On Distribucion Grid Customize Element.
    /// </summary>
    private void OnDistribucionGridCustomizeElement(GridCustomizeElementEventArgs e)
    {
        if (e.Column is not DxGridDataColumn column)
            return;

        var cssClass = column.FieldName switch
        {
            nameof(ListaPropuestaDistribucion.S275H) => "col-s275-h",
            nameof(ListaPropuestaDistribucion.S355TI) => "col-s355-ti",
            nameof(ListaPropuestaDistribucion.S355V) => "col-s355-v",
            nameof(ListaPropuestaDistribucion.S355W) => "col-s355-w",
            nameof(ListaPropuestaDistribucion.S460A) => "col-s460-a",
            nameof(ListaPropuestaDistribucion.S275TI) => "col-s275-ti",
            _ => string.Empty
        };

        if (string.IsNullOrWhiteSpace(cssClass))
            return;

        if (e.ElementType == GridElementType.DataCell ||
            e.ElementType == GridElementType.HeaderCell)
        {
            e.CssClass = $"{e.CssClass} {cssClass}".Trim();
        }
    }

    /// <summary>
    /// Validar Fila Distribucion.
    /// </summary>
    protected string ValidarFilaDistribucion(ListaPropuestaDistribucion fila, IEnumerable<ListaPropuestaDistribucion>? propuesta = null)
    {
        var tundish = ListaTundishReales.FirstOrDefault(x => x.TundishActivo && x.NumTundish == fila.NumTundish);
        if (tundish is null)
            return string.Empty;

        var total = fila.S275H + fila.S355TI + fila.S355V + fila.S355W + fila.S460A + fila.S275TI;
        var totalTundish = Convert.ToInt32(tundish.NumColadas);
        if (total != totalTundish)
            return $"La suma del tundish {fila.NumTundish} debe ser exactamente {totalTundish} coladas. Capturadas: {total}.";

        var minimoS275H = tundish.HorasVida < 7 ? 3 : 5;
        var totalEspeciales = fila.S355TI + fila.S355V + fila.S355W + fila.S460A + fila.S275TI;
        if (totalEspeciales > 0 && fila.S275H < minimoS275H)
            return $"Si el tundish {fila.NumTundish} tiene calidades especiales, S275-H debe ser al menos {minimoS275H} coladas.";

        foreach (var calidad in new[] { "S355-TI", "S355-V", "S355W", "S460-A", "S275-TI" })
        {
            var totalCalidad = (propuesta ?? ListaPropuestaDistribucionColadas).Sum(f => GetValorEspecialFila(f, calidad));
            var maxPermitido = ListaNecesidadDetalleBB.Where(x => x.Calidad == calidad).Select(x => Convert.ToInt32(x.ColadasNecesarias)).DefaultIfEmpty(0).Max();
            if (totalCalidad > maxPermitido)
                return $"El total de {calidad} no puede exceder {maxPermitido} coladas.";
        }

        return string.Empty;
    }

    /// <summary>
    /// Existen Cambios.
    /// </summary>
    protected bool ExistenCambios()
    {
        var actual = JsonSerializer.Serialize(ListaTundishReales, JsonOptions);
        var original = JsonSerializer.Serialize(ListaTundishRealesBk, JsonOptions);
        return actual != original;
    }

    /// <summary>
    /// Calcular Mediana.
    /// </summary>
    protected decimal CalcularMediana(List<decimal> numeros)
    {
        if (numeros.Count == 0)
            return 0;
        numeros.Sort();
        var mitad = numeros.Count / 2;
        return numeros.Count % 2 != 0 ? numeros[mitad] : (numeros[mitad - 1] + numeros[mitad]) / 2m;
    }

    /// <summary>
    /// Actualizar Estado Version.
    /// </summary>
    protected void ActualizarEstadoVersion(string mensaje)
    {
        EstadoVersion = mensaje;
        EstadoVersionCss = mensaje.Contains("sin guardar", StringComparison.OrdinalIgnoreCase) ? "estado-warning" :
            mensaje == "---" ? string.Empty : "estado-ok";
    }

    /// <summary>
    /// Existe Calidad.
    /// </summary>
    protected bool ExisteCalidad(string calidad) => ListaNecesidadDetalleBB.Exists(x => x.Calidad == calidad && x.ColadasNecesarias > 0);
    /// <summary>
    /// Get Visible Css.
    /// </summary>
    protected string GetVisibleCss(bool visible) => visible ? string.Empty : "hidden-col";
    /// <summary>
    /// Get Tipo Row Css.
    /// </summary>
    protected static string GetTipoRowCss(string tipo) => tipo switch { "BB1" => "row-bb1", "BB2" => "row-bb2", "BB3" => "row-bb3", _ => string.Empty };
    /// <summary>
    /// Get Calidad Css.
    /// </summary>
    protected static string GetCalidadCss(string calidad) => calidad.Replace("-", string.Empty).ToLowerInvariant();

    /// <summary>
    /// Agregar Tipo Faltante.
    /// </summary>
    private void AgregarTipoFaltante(string calidad, string tipo, bool disponible, ref int counter)
    {
        if (disponible && !ListaNecesidadDetalleBB.Exists(x => x.Calidad == calidad && x.TipoBB == tipo))
        {
            ListaNecesidadDetalleBB.Add(new ListDetalleNecesidadBB { OrdenFab = counter++, Calidad = calidad, TipoBB = tipo });
        }
    }

    /// <summary>
    /// Is BB.
    /// </summary>
    private static bool IsBB(BeamBlankNecesidad x) => IsBB1(x) || IsBB2(x) || IsBB3(x);
    /// <summary>
    /// Is BB.
    /// </summary>
    private static bool IsBB1(BeamBlankNecesidad x) => x.CodMaquina == 4 && (x.MatSemi?.Trim() == "BB1");
    /// <summary>
    /// Is BB.
    /// </summary>
    private static bool IsBB2(BeamBlankNecesidad x) => x.CodMaquina == 4 && (x.MatSemi?.Trim() == "BB2");
    /// <summary>
    /// Is BB.
    /// </summary>
    private static bool IsBB3(BeamBlankNecesidad x) => x.CodMaquina == 4 && (x.MatSemi?.Trim() == "BB3");
    /// <summary>
    /// Tundish Tiene BB.
    /// </summary>
    private static bool TundishTieneBB1(ListTundishDisponibles t) => t.TnXColadaBB1 > 0 || t.tstotalBB1 > 0;
    /// <summary>
    /// Tundish Tiene BB.
    /// </summary>
    private static bool TundishTieneBB2(ListTundishDisponibles t) => t.TnXColadaBB2 > 0 || t.tstotalBB2 > 0;
    /// <summary>
    /// Tundish Tiene BB.
    /// </summary>
    private static bool TundishTieneBB3(ListTundishDisponibles t) => t.TnXColadaBB3 > 0 || t.tstotalBB3 > 0;

    /// <summary>
    /// Get Valor Especial Fila.
    /// </summary>
    private static int GetValorEspecialFila(ListaPropuestaDistribucion fila, string calidad) => calidad switch
    {
        "S355-TI" => fila.S355TI,
        "S355-V" => fila.S355V,
        "S355W" => fila.S355W,
        "S460-A" => fila.S460A,
        "S275-TI" => fila.S275TI,
        _ => 0
    };

    /// <summary>
    /// Set Valor Especial Fila.
    /// </summary>
    private static void SetValorEspecialFila(ListaPropuestaDistribucion fila, string calidad, int valor)
    {
        switch (calidad)
        {
            case "S355-TI": fila.S355TI = valor; break;
            case "S355-V": fila.S355V = valor; break;
            case "S355W": fila.S355W = valor; break;
            case "S460-A": fila.S460A = valor; break;
            case "S275-TI": fila.S275TI = valor; break;
        }
    }

    /// <summary>
    /// Get Campo Distribucion.
    /// </summary>
    private static int GetCampoDistribucion(ListaPropuestaDistribucion fila, string campo) => campo switch
    {
        nameof(ListaPropuestaDistribucion.S275H) => fila.S275H,
        nameof(ListaPropuestaDistribucion.S355TI) => fila.S355TI,
        nameof(ListaPropuestaDistribucion.S355V) => fila.S355V,
        nameof(ListaPropuestaDistribucion.S355W) => fila.S355W,
        nameof(ListaPropuestaDistribucion.S460A) => fila.S460A,
        nameof(ListaPropuestaDistribucion.S275TI) => fila.S275TI,
        _ => 0
    };

    /// <summary>
    /// Set Campo Distribucion.
    /// </summary>
    private static void SetCampoDistribucion(ListaPropuestaDistribucion fila, string campo, int valor)
    {
        switch (campo)
        {
            case nameof(ListaPropuestaDistribucion.S275H): fila.S275H = valor; break;
            case nameof(ListaPropuestaDistribucion.S355TI): fila.S355TI = valor; break;
            case nameof(ListaPropuestaDistribucion.S355V): fila.S355V = valor; break;
            case nameof(ListaPropuestaDistribucion.S355W): fila.S355W = valor; break;
            case nameof(ListaPropuestaDistribucion.S460A): fila.S460A = valor; break;
            case nameof(ListaPropuestaDistribucion.S275TI): fila.S275TI = valor; break;
        }
    }

    /// <summary>
    /// Clone Distribucion.
    /// </summary>
    private static ListaPropuestaDistribucion CloneDistribucion(ListaPropuestaDistribucion source) => new()
    {
        NumTRegistro = source.NumTRegistro,
        NumTundish = source.NumTundish,
        FechaInicioTundis = source.FechaInicioTundis,
        FechaFinTundis = source.FechaFinTundis,
        NumColadas = source.NumColadas,
        VidaUtil = source.VidaUtil,
        S275H = source.S275H,
        S355TI = source.S355TI,
        S355V = source.S355V,
        S355W = source.S355W,
        S460A = source.S460A,
        S275TI = source.S275TI
    };

    /// <summary>
    /// Copiar Valores Editables.
    /// </summary>
    private static void CopiarValoresEditables(ListaPropuestaDistribucion source, ListaPropuestaDistribucion target)
    {
        target.S275H = source.S275H;
        target.S355TI = source.S355TI;
        target.S355V = source.S355V;
        target.S355W = source.S355W;
        target.S460A = source.S460A;
        target.S275TI = source.S275TI;
    }

    /// <summary>
    /// Map Tundish Standard.
    /// </summary>
    private static ListTundishStandard MapTundishStandard(TundishStandard source) => new()
    {
        tsId = source.tsId,
        tsSociedad = source.tsSociedad,
        tsCierre1 = source.tsCierre1,
        tsCierre2 = source.tsCierre2,
        tsCierre3 = source.tsCierre3,
        tsCierre4 = source.tsCierre4,
        tsCierre5 = source.tsCierre5,
        tsCierre6 = source.tsCierre6,
        tsActivo = source.tsActivo,
        tsPrioridad = source.tsPrioridad,
        tstotalBB1 = source.tstotalBB1,
        tstotalBB2 = source.tstotalBB2,
        tstotalBB3 = source.tstotalBB3
    };

    private static T? Deserialize<T>(string json) => string.IsNullOrWhiteSpace(json) ? default : JsonSerializer.Deserialize<T>(json, JsonOptions);
    private static List<T> Clone<T>(List<T> values) => JsonSerializer.Deserialize<List<T>>(JsonSerializer.Serialize(values, JsonOptions), JsonOptions) ?? new();
    //protected readonly record struct VersionComboItem(long IdVersion, string DisplayText);


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
}
