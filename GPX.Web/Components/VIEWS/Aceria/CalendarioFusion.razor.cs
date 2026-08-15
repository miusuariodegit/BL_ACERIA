using DevExpress.Blazor;
using GPX.Negocio.Aceria;
using GPX.Negocio.CRUD;
using GPX.Negocio.ORM;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.Globalization;

// [GPX-DOC-v1] ================================================================================
// Codigo tras la vista de calendario de fusion de horno: catalogos, consulta/edicion del calendario,
// validacion, carga masiva y exportacion.
// ================================================================================================

namespace GPX.Web.Components.VIEWS.Aceria;

/// <summary>
/// Clase CalendarioFusion. Codigo tras la vista de calendario de fusion de horno: catalogos,
/// consulta/edicion del calendario, validacion, carga masiva y exportacion.
/// </summary>
public partial class CalendarioFusion : ComponentBase
{
    [Inject] protected IDialogService DialogService { get; set; } = default!;
    [Inject] protected CrudRepository CrudRepository { get; set; } = default!;
    [Inject] protected CalendarioFusionHornoService CalendarioFusionHornoService { get; set; } = default!;
    [Inject] protected AceriaService AceriaService { get; set; } = default!;

    protected bool IsLoading { get; set; }
    protected DateTime FechaHasta { get; set; } = DateTime.Now.AddDays(10);
    protected IGrid? GridCalendario { get; set; }

    protected List<CalendarioFusionHorno> ListaCalendarioFusion { get; set; } = new();
    protected List<ConfiguracionAceria> ListaConfiguracionAceria { get; set; } = new();

    protected List<ListTundishDisponibles> ListaTundishDisponiblesBB { get; set; } = new();
    protected List<ListTundishDisponibles> ListaTundishDisponiblesP140 { get; set; } = new();
    protected List<ListTundishDisponibles> ListaTundishDisponiblesP160 { get; set; } = new();

    protected bool PopupCambioTundishVisible { get; set; }
    protected bool PopupCargaVisible { get; set; }
    protected bool PopupListadoTundishVisible { get; set; }

    protected DateTime CambioFechaInicio { get; set; } = DateTime.Now;
    protected DateTime CambioFechaFin { get; set; } = DateTime.Now;
    protected string? CambioTipoSemi { get; set; }
    protected IBrowserFile? ArchivoCalendario { get; set; }

    protected readonly string[] TiposSemi = ["BB", "P140", "P160"];
    protected readonly List<HoraCalendario> Horas = Enumerable.Range(1, 24)
        .Select(x => new HoraCalendario(x, $"{x}hrs", $"cafHora{x}", $"TipoSemiHora{x}"))
        .ToList();

    /// <summary>
    /// On Initialized (operacion asincrona).
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        await CargaCatalogosAsync();
        //await CargarDataAsync();
    }

    /// <summary>
    /// Carga Catalogos (operacion asincrona).
    /// </summary>
    protected async Task CargaCatalogosAsync()
    {
        // TODO BD:
        ListaConfiguracionAceria = await CrudRepository.ConsultaConfiguracionAceriaAsync();
        //await Task.CompletedTask;
    }

    /// <summary>
    /// On Consultar Click.
    /// </summary>
    protected async Task OnConsultarClick()
    {
        IsLoading = true;
        await InvokeAsync(StateHasChanged);

        await CargarDataAsync();

        IsLoading = false;
        await InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Cargar Data (operacion asincrona).
    /// </summary>
    protected async Task CargarDataAsync()
    {
        // TODO BD:
        ListaCalendarioFusion = await CalendarioFusionHornoService.DameCalendarioFusionHastaAsync(FechaHasta);

    }



    /// <summary>
    /// On Edit Model Saving.
    /// </summary>
    protected async Task OnEditModelSaving(GridEditModelSavingEventArgs e)
    {
        var edit = (CalendarioFusionHorno)e.EditModel;
        var errores = ValidarRegistro(edit);

        if (errores.Count > 0)
        {
            e.Cancel = true;

            await DialogService.AlertAsync(new MessageBoxOptions
            {
                Title = "Validación",
                Text = string.Join(Environment.NewLine, errores),
                OkButtonText = "Aceptar",
                RenderStyle = MessageBoxRenderStyle.Warning
            });

            return;
        }

        LimpiarTiposSinHoraActiva(edit);

        // TODO BD:
        await CrudRepository.ActualizarCalendarioFusionHornoAsync(edit);

        await CargarDataAsync();
    }

    /// <summary>
    /// Validar Registro.
    /// </summary>
    protected List<string> ValidarRegistro(CalendarioFusionHorno item)
    {
        var errores = new List<string>();

        for (var i = 1; i <= 24; i++)
        {
            if (GetHoraActiva(item, i) && string.IsNullOrWhiteSpace(GetTipoSemi(item, i)))
                errores.Add($"Si la hora {i} se activa es necesario seleccionar un tipo de semi.");
        }

        return errores;
    }

    /// <summary>
    /// Limpiar Tipos Sin Hora Activa.
    /// </summary>
    protected void LimpiarTiposSinHoraActiva(CalendarioFusionHorno item)
    {
        for (var i = 1; i <= 24; i++)
        {
            if (!GetHoraActiva(item, i))
                SetTipoSemi(item, i, string.Empty);
        }
    }

    /// <summary>
    /// On Grid Customize Element.
    /// </summary>
    protected void OnGridCustomizeElement(GridCustomizeElementEventArgs e)
    {
        // Estilo badge: el coloreado se hace dentro del CellDisplayTemplate con .grid-badge
        // ya no se pinta la celda completa.
    }

    /// <summary>
    /// Get Tipo Css.
    /// </summary>
    protected string GetTipoCss(string? tipo, bool activo)
    {
        if (!activo) return "hora-inactiva";

        return tipo?.Trim().ToUpperInvariant() switch
        {
            "BB" => "tipo-bb",
            "P140" => "tipo-p140",
            "P160" => "tipo-p160",
            _ => "tipo-sin-definir"
        };
    }

    /// <summary>
    /// On Abrir Cambio Tundish.
    /// </summary>
    protected async Task OnAbrirCambioTundish()
    {
        CambioFechaInicio = DateTime.Now;
        CambioFechaFin = DateTime.Now;
        CambioTipoSemi = "BB";
        PopupCambioTundishVisible = true;
        await Task.CompletedTask;
    }

    /// <summary>
    /// On Abrir Carga Calendario.
    /// </summary>
    protected void OnAbrirCargaCalendario()
    {
        ArchivoCalendario = null;
        PopupCargaVisible = true;
    }

    /// <summary>
    /// On Abrir Listado Tundish.
    /// </summary>
    protected async Task OnAbrirListadoTundish()
    {
        IsLoading = true;
        await InvokeAsync(StateHasChanged);

        await CargarListadoTundishAsync();

        IsLoading = false;
        PopupListadoTundishVisible = true;
    }

    /// <summary>
    /// On Guardar Cambio Tundish.
    /// </summary>
    protected async Task OnGuardarCambioTundish()
    {
        if (CambioFechaInicio > CambioFechaFin)
        {
            await DialogService.AlertAsync(new MessageBoxOptions
            {
                Title = "Validación",
                Text = "La fecha de inicio no puede ser mayor a la fecha de fin.",
                OkButtonText = "Aceptar",
                RenderStyle = MessageBoxRenderStyle.Warning
            });
            return;
        }

        if (string.IsNullOrWhiteSpace(CambioTipoSemi))
        {


            await DialogService.AlertAsync(new MessageBoxOptions
            {
                Title = "Validación",
                Text = "Selecciona el tipo de semi.",
                OkButtonText = "Aceptar",
                RenderStyle = MessageBoxRenderStyle.Warning
            });
            return;
        }

        for (var fecha = CambioFechaInicio; fecha <= CambioFechaFin; fecha = fecha.AddHours(1))
        {
            // TODO BD:
            await CalendarioFusionHornoService.ActualizaSoloDiaFusionAsync(fecha, CambioTipoSemi);
        }

        PopupCambioTundishVisible = false;

        await DialogService.AlertAsync(new MessageBoxOptions
        {
            Title = "Calendario",
            Text = "Cambio ejecutado.",
            OkButtonText = "Aceptar",
            RenderStyle = MessageBoxRenderStyle.Success
        });


        await CargarDataAsync();
    }

    /// <summary>
    /// On Archivo Seleccionado.
    /// </summary>
    protected void OnArchivoSeleccionado(InputFileChangeEventArgs e)
    {
        ArchivoCalendario = e.File;
    }

    /// <summary>
    /// On Procesar Archivo Calendario.
    /// </summary>
    protected async Task OnProcesarArchivoCalendario()
    {
        if (ArchivoCalendario is null)
            return;

        var registros = new List<ListaCargaMasivaCalendario>();
        var valorMinCarga = ListaConfiguracionAceria.FirstOrDefault()?.caCargaMasivaCalendarioValor ?? 0;

        await using var stream = ArchivoCalendario.OpenReadStream(maxAllowedSize: 20 * 1024 * 1024);
        using var reader = new StreamReader(stream);

        string? line;

        while ((line = await reader.ReadLineAsync()) != null)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var rows = line.Split(';');
            if (rows.Length < 4)
                continue;

            if (DateTime.TryParse(rows[1], CultureInfo.CurrentCulture, DateTimeStyles.None, out var fecha) &&
                int.TryParse(rows[3], out var valor))
            {
                registros.Add(new ListaCargaMasivaCalendario(fecha, valor));
            }
        }

        var filtrados = registros.Where(x => x.valor > valorMinCarga).ToList();
        if (filtrados.Count == 0)
        {


            await DialogService.AlertAsync(new MessageBoxOptions
            {
                Title = "Validación",
                Text = "No existen registros en el archivo que cumplan con el criterio.",
                OkButtonText = "Aceptar",
                RenderStyle = MessageBoxRenderStyle.Warning
            });

            return;
        }

        foreach (var fechaOrigen in filtrados.Select(x => x.fecha.Date).Distinct())
        {
            var nuevo = new CalendarioFusionHorno { cafFecha = fechaOrigen };

            // 2026-05-10 solo se limpia  de la hora del reguistro en adelante  no todo el dia

            DateTime fechaOrigenHora = filtrados
                .Where(x => x.fecha.Date == fechaOrigen)
                .OrderBy(x => x.fecha)
                .First().fecha;




            await CalendarioFusionHornoService.LimpiarApartiDeHoraInicioFusionAsync(fechaOrigenHora);

            foreach (var diaOrigen in filtrados.Where(x => x.fecha.Date == fechaOrigen).OrderBy(x => x.fecha))
            {
                var hora = diaOrigen.fecha.Hour;
                var propIndex = hora == 0 ? 24 : hora;
                SetHoraActiva(nuevo, propIndex, true);
                SetTipoSemi(nuevo, propIndex, "BB");
            }

            // TODO BD:
            await CalendarioFusionHornoService.ActualizaDiaCompletoFusionAsync(nuevo);
        }

        var ultimaFecha = filtrados.Max(x => x.fecha);
        for (var fecha = ultimaFecha.AddDays(1).Date; fecha <= DateTime.Now.AddMonths(1).Date; fecha = fecha.AddDays(1))
        {
            // TODO BD:
            await CalendarioFusionHornoService.LimpiarDiaCompletoFusionAsync(fecha);
        }

        PopupCargaVisible = false;

        await DialogService.AlertAsync(new MessageBoxOptions
        {
            Title = "Calendario",
            Text = "Calendario cargado.",
            OkButtonText = "Aceptar",
            RenderStyle = MessageBoxRenderStyle.Success
        });

        await CargarDataAsync();
    }

    /// <summary>
    /// Cargar Listado Tundish (operacion asincrona).
    /// </summary>
    protected async Task CargarListadoTundishAsync()
    {
        var inicio = DateTime.Now.Date;

        ListaTundishDisponiblesBB = await AceriaService.ConsultaTundishDisponiblesAsync(1000, inicio, "BB");
        ListaTundishDisponiblesP140 = await AceriaService.ConsultaTundishDisponiblesAsync(1000, inicio, "P140");
        ListaTundishDisponiblesP160 = await AceriaService.ConsultaTundishDisponiblesAsync(1000, inicio, "P160");

        var fechaMinima = DateTime.Now.AddDays(-1);

        ListaTundishDisponiblesBB = ListaTundishDisponiblesBB
            .Where(x => x.FechaInicio >= fechaMinima)
            .ToList();

        ListaTundishDisponiblesP140 = ListaTundishDisponiblesP140
            .Where(x => x.FechaInicio >= fechaMinima)
            .ToList();

        ListaTundishDisponiblesP160 = ListaTundishDisponiblesP160
            .Where(x => x.FechaInicio >= fechaMinima)
            .ToList();
    }



    /// <summary>
    /// On Tundish Edit Model Saving.
    /// </summary>
    protected async Task OnTundishEditModelSaving(GridEditModelSavingEventArgs e)
    {
        var item = (ListTundishDisponibles)e.EditModel;

        if (string.IsNullOrWhiteSpace(item.TipoSemi))
        {
            e.Cancel = true;

            await DialogService.AlertAsync(new MessageBoxOptions
            {
                Title = "Validación",
                Text = "Selecciona el tipo de semi.",
                OkButtonText = "Aceptar",
                RenderStyle = MessageBoxRenderStyle.Warning
            });


            return;
        }

        for (var i = 1; i < item.HorasVida + 1; i++)
        {
            var diaHoraInicio = item.FechaInicio.AddHours(i);
            // TODO BD:
            await CalendarioFusionHornoService.ActualizaSoloDiaHoraFusionAsync(diaHoraInicio, diaHoraInicio.Hour, item.TipoSemi);
        }



        await DialogService.AlertAsync(new MessageBoxOptions
        {
            Title = "Calendario",
            Text = "Cambio ejecutado.",
            OkButtonText = "Aceptar",
            RenderStyle = MessageBoxRenderStyle.Success
        });


        await CargarListadoTundishAsync();
        await CargarDataAsync();
    }

    /// <summary>
    /// On Column Chooser Item Click.
    /// </summary>
    protected void OnColumnChooserItemClick(ToolbarItemClickEventArgs e)
        => GridCalendario?.ShowColumnChooser();

    /// <summary>
    /// Export All Data To Excel.
    /// </summary>
    protected Task ExportAllDataToExcel()
        => GridCalendario?.ExportToXlsxAsync("CalendarioFusionHorno") ?? Task.CompletedTask;

    /// <summary>
    /// Export All Data To Csv.
    /// </summary>
    protected Task ExportAllDataToCsv()
        => GridCalendario?.ExportToCsvAsync("CalendarioFusionHorno") ?? Task.CompletedTask;

    /// <summary>
    /// Export All Data To Pdf.
    /// </summary>
    protected Task ExportAllDataToPdf()
        => GridCalendario?.ExportToPdfAsync("CalendarioFusionHorno") ?? Task.CompletedTask;

    /// <summary>
    /// Get Hora Activa.
    /// </summary>
    protected static bool GetHoraActiva(CalendarioFusionHorno item, int hora)
        => item.GetType().GetProperty($"cafHora{hora}")?.GetValue(item) is bool value && value;

    /// <summary>
    /// Get Tipo Semi.
    /// </summary>
    protected static string GetTipoSemi(CalendarioFusionHorno item, int hora)
    {
        var TipoSemi = item.GetType().GetProperty($"TipoSemiHora{hora}")?.GetValue(item).ToString();


        return string.IsNullOrWhiteSpace(TipoSemi) ? "BB" : TipoSemi;



    }
    

    /// <summary>
    /// Set Hora Activa.
    /// </summary>
    protected static void SetHoraActiva(CalendarioFusionHorno item, int hora, bool value)
        => item.GetType().GetProperty($"cafHora{hora}")?.SetValue(item, value);

    /// <summary>
    /// Set Tipo Semi.
    /// </summary>
    protected static void SetTipoSemi(CalendarioFusionHorno item, int hora, string? value)
        => item.GetType().GetProperty($"TipoSemiHora{hora}")?.SetValue(item, value ?? string.Empty);

    /// <summary>
    /// Registro HoraCalendario. Codigo tras la vista de calendario de fusion de horno: catalogos,
    /// consulta/edicion del calendario, validacion, carga masiva y exportacion.
    /// </summary>
    protected sealed record HoraCalendario(int Numero, string Caption, string CampoHora, string CampoTipo);

}





