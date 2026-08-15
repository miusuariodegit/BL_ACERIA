

using DevExpress.Blazor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using GPX.Negocio.Aceria;
using GPX.Negocio.ORM;
using GPX.Negocio.COP;

// [GPX-DOC-v1] ================================================================================
// Codigo tras la vista de necesidades de aceria (beam blank): catalogos, consulta, detalle por orden y
// exportacion.
// ================================================================================================

namespace GPX.Web.Components.VIEWS.Aceria;

/// <summary>
/// Clase NecesidadAceria. Codigo tras la vista de necesidades de aceria (beam blank): catalogos,
/// consulta, detalle por orden y exportacion.
/// </summary>
public partial class NecesidadAceria : ComponentBase
{
    [Inject] protected AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;
    [Inject] protected NavigationManager NavigationManager { get; set; } = default!;
    [Inject] protected IDialogService DialogService { get; set; } = default!;
    [Inject] protected GPX.Negocio.CRUD.CrudRepository CrudRepository { get; set; } = default!;
    [Inject] protected GPX.Negocio.Aceria.AceriaService AceriaService { get; set; } = default!;

    protected CancellationTokenSource disposalTokenSource = new();
    protected bool IsLoading;
    protected bool IsContactPanelPinned { get; set; }
    protected bool IsContactPanelOpen { get; set; }
    protected SizeMode SizeMode { get; set; }
    protected DrawerMode DrawerMode => IsContactPanelPinned ? DrawerMode.Shrink : DrawerMode.Overlap;

    protected CentrosXsociedad? MaquinaSeleccionada;
    protected List<BeamBlankNecesidad>? ListaNecesidadesBeamBlanks { get; set; }
    protected List<BeamBlankNecesidad>? ListaNecesidadesBeamBlanksAgrupago { get; set; }
    protected List<CentrosXsociedad>? ListaCentros { get; set; }

    protected IGrid? gvNececiadesAceria { get; set; }
    protected string? SearchText { get; set; }

    /// <summary>
    /// On Initialized (operacion asincrona).
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        await CargaCatalogosAsync();
        await base.OnInitializedAsync();
    }

    /// <summary>
    /// Carga Catalogos (operacion asincrona).
    /// </summary>
    protected async Task CargaCatalogosAsync()
    {
        ListaCentros = await CrudRepository.ConsultaCentrosXsociedadAsync();
        ListaCentros = ListaCentros
            .Where(x => x.csCodSociedad == Constantes.SociedadXDefecto && x.csActivo)
            .ToList();
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
        ListaNecesidadesBeamBlanksAgrupago = new();
        ListaNecesidadesBeamBlanks = new();

        if (MaquinaSeleccionada is null)
        {
                await DialogService.AlertAsync(new MessageBoxOptions
                {
                    Title = "Validacion",
                    Text = "Selecciona una máquina para consultar las necesidades de beam blanks.",
                    OkButtonText = "Si",
                    RenderStyle = MessageBoxRenderStyle.Info,
                    CancelButtonText = "No"
                });
                return;
        }



      

        ListaNecesidadesBeamBlanks = await AceriaService
            .DameNecesidadBeamBlankTrenV2Async(Constantes.SociedadXDefecto, CodMaquina: MaquinaSeleccionada.csCodCentro);

        ListaNecesidadesBeamBlanksAgrupago = ListaNecesidadesBeamBlanks
            .GroupBy(x => new
            {
                x.Sociedad,
                x.CodMaquina,
                x.MatSemi,
                x.CalidadSemi,
                x.LongitudSemi,
                x.SemanaPrevIni,
                x.NombreSemana
            })
            .Select(g => new BeamBlankNecesidad
            {
                numRegistro = HashCode.Combine(
                    g.Key.Sociedad,
                    g.Key.CodMaquina,
                    g.Key.MatSemi,
                    g.Key.CalidadSemi,
                    g.Key.LongitudSemi,
                    g.Key.SemanaPrevIni),
                Sociedad = g.Key.Sociedad,
                CodMaquina = g.Key.CodMaquina,
                IdFab = 0,
                MatSemi = g.Key.MatSemi,
                CalidadSemi = g.Key.CalidadSemi,
                LongitudSemi = g.Key.LongitudSemi,
                Familia = "",
                Calidad = "",
                Longitud = 0,
                TnsAFabSemi = g.Sum(y => y.TnsAFabSemi),
                UdsAFabSemi = g.Sum(y => y.UdsAFabSemi),
                SemanaPrevIni = g.Key.SemanaPrevIni
                //NombreSemana = g.Key.NombreSemana
            })
            .OrderBy(x => ConvertirSemanaAOrden(x.SemanaPrevIni))
            .ThenBy(x => x.NombreSemana)
            .ThenBy(x => x.MatSemi)
            .ThenBy(x => x.CalidadSemi)
            .ToList();
    }

    /// <summary>
    /// Convertir Semana A Orden.
    /// </summary>
    protected int ConvertirSemanaAOrden(string? semanaPrevIni)
        => int.TryParse(semanaPrevIni, out var n) ? n : int.MaxValue;

    /// <summary>
    /// Obtener Detalle.
    /// </summary>
    protected List<BeamBlankNecesidad> ObtenerDetalle(BeamBlankNecesidad master)
    {
        return ListaNecesidadesBeamBlanks?
            .Where(x =>
                x.CalidadSemi == master.CalidadSemi &&
                x.MatSemi.Trim() == master.MatSemi.Trim() &&
                x.LongitudSemi == master.LongitudSemi &&
                x.SemanaPrevIni == master.SemanaPrevIni)
            .ToList()
            ?? new List<BeamBlankNecesidad>();
    }

    void OnGridCustomizeElement(GridCustomizeElementEventArgs e)
    {
        // Solo actuar sobre celdas de datos
        if (e.ElementType != GridElementType.DataCell)
            return;

        // Solo la columna que te interesa
        if (e.Column.Name != "CalidadSemiLong")
            return;

        // Obtener el objeto de la fila actual
        var fila = (BeamBlankNecesidad)e.Grid.GetDataItem(e.VisibleIndex);
        if (fila is null || string.IsNullOrWhiteSpace(fila.CalidadSemiLong))
            return;

        var calidad = fila.CalidadSemiLong.Trim();

        if (calidad.Contains("S355-TI", StringComparison.OrdinalIgnoreCase))
            e.CssClass = "calidad-s355ti";
        else if (calidad.Contains("S355-V", StringComparison.OrdinalIgnoreCase))
            e.CssClass = "calidad-s355v";
        else if (calidad.Contains("S355W", StringComparison.OrdinalIgnoreCase))
            e.CssClass = "calidad-s355w";
        else if (calidad.Contains("S460A", StringComparison.OrdinalIgnoreCase))
            e.CssClass = "calidad-s460a";
        else if (calidad.Contains("S275-TI", StringComparison.OrdinalIgnoreCase))
            e.CssClass = "calidad-s275ti";
        // S275-H lo dejas sin estilo, como en tu código viejo
    }



    /// <summary>
    /// On Column Chooser Item Click.
    /// </summary>
    protected void OnColumnChooserItemClick(ToolbarItemClickEventArgs e)
        => gvNececiadesAceria?.ShowColumnChooser();

    /// <summary>
    /// Export All Data To Excel.
    /// </summary>
    protected Task ExportAllDataToExcel()
        => gvNececiadesAceria?.ExportToXlsxAsync("NecesidadesAceria") ?? Task.CompletedTask;

    /// <summary>
    /// Export All Data To Csv.
    /// </summary>
    protected Task ExportAllDataToCsv()
        => gvNececiadesAceria?.ExportToCsvAsync("NecesidadesAceria") ?? Task.CompletedTask;

    /// <summary>
    /// Export All Data To Pdf.
    /// </summary>
    protected Task ExportAllDataToPdf()
        => gvNececiadesAceria?.ExportToPdfAsync("NecesidadesAceria") ?? Task.CompletedTask;


}