using GPX.Negocio.Aceria;
using GPX.Negocio.ORM;
using Microsoft.AspNetCore.Components;
using System.Globalization;

// [GPX-DOC-v1] ================================================================================
// Codigo tras el control de usuario que representa un tundish individual: configuracion de bocas y
// calculo de rendimiento.
// ================================================================================================

namespace GPX.Web.Components.VIEWS.Aceria;

/// <summary>
/// Clase UcTundish. Codigo tras el control de usuario que representa un tundish individual:
/// configuracion de bocas y calculo de rendimiento.
/// </summary>
public partial class UcTundish : ComponentBase
{
    [Parameter] public int Numero { get; set; }
    [Parameter] public string TundishId { get; set; } = string.Empty;
    [Parameter] public int VidaUtilTundish { get; set; }
    [Parameter] public ListTundishDisponibles? Tundish { get; set; }
    [Parameter] public ListTundishStandard? EstandarSeleccionado { get; set; }
    [Parameter] public List<ListTundishStandard> ListaTundisStandar { get; set; } = new();
    [Parameter] public List<ConfiguracionAceria> ListaConfiguracionAceria { get; set; } = new();
    [Parameter] public EventCallback<ListTundishDisponibles> TundishChanged { get; set; }

    protected int? SelectedStandard => (Tundish?.EstandardSeleccionado ?? EstandarSeleccionado)?.tsId;

    protected static readonly BocaEstadoOption[] EstadosBoca =
    [
        new("Abierta", "ABIERTA"),
        new("Cerrada", "CERRADA"),
        new("Bloqueada/Perforada", "BLOQUEDA/PERFORADA")
    ];

    /// <summary>
    /// On Parameters Set.
    /// </summary>
    protected override void OnParametersSet()
    {
        if (Tundish is null)
            return;

        if (string.IsNullOrWhiteSpace(Tundish.NombreTundish))
            Tundish.NombreTundish = string.IsNullOrWhiteSpace(TundishId) ? $"UcTundish_{Numero}" : TundishId;

        if (Tundish.HorasVida <= 0 && VidaUtilTundish > 0)
            Tundish.HorasVida = VidaUtilTundish;

        if (Tundish.EstandardSeleccionado is null && EstandarSeleccionado is not null)
            Tundish.EstandardSeleccionado = EstandarSeleccionado;

        CargarConfiguracionRendimiento(Tundish.EstandardSeleccionado ?? EstandarSeleccionado);
    }

    /// <summary>
    /// On Standard Changed.
    /// </summary>
    protected async Task OnStandardChanged(int? id)
    {
        if (Tundish is null)
            return;

        var tundishEstandar = ListaTundisStandar.FirstOrDefault(x => x.tsId == id);
        if (tundishEstandar is null)
            return;

        Tundish.tstotalBB1 = 0;
        Tundish.tstotalBB2 = 0;
        Tundish.tstotalBB3 = 0;
        Tundish.EstandardSeleccionado = tundishEstandar;
        CargarConfiguracionRendimiento(tundishEstandar);
        await NotifyChanged();
    }

    /// <summary>
    /// On Activo Changed.
    /// </summary>
    protected async Task OnActivoChanged(bool value)
    {
        if (Tundish is null)
            return;

        Tundish.TundishActivo = value;
        CargarConfiguracionRendimiento(Tundish.EstandardSeleccionado ?? EstandarSeleccionado);
        await NotifyChanged();
    }

    /// <summary>
    /// On Estado Boca Changed.
    /// </summary>
    protected async Task OnEstadoBocaChanged(int numeroBoca, string estatusBoca)
    {
        if (Tundish is null)
            return;

        var tipoBB = GetBoca(numeroBoca).Tipo;
        ConfigurarBoca(Tundish, tipoBB, estatusBoca, numeroBoca);
        CargarConfiguracionRendimiento(Tundish.EstandardSeleccionado ?? EstandarSeleccionado);
        await NotifyChanged();
    }

    /// <summary>
    /// Cargar Configuracion Rendimiento.
    /// </summary>
    public void CargarConfiguracionRendimiento(ListTundishStandard? tundishEstandar)
    {
        if (Tundish is null || tundishEstandar is null || ListaConfiguracionAceria.Count == 0)
            return;

        Tundish.EstandardSeleccionado = tundishEstandar;

        if (Tundish.tstotalBB1 == 0 && Tundish.tstotalBB2 == 0 && Tundish.tstotalBB3 == 0)
            CargarDatosEstandar(tundishEstandar);

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

        if (VidaUtilTundish > 0)
        {
            Tundish.NumColadas = Math.Ceiling((VidaUtilTundish * 60) / minXColada);
            if (Tundish.NumColadas > 1)
                Tundish.NumColadas--;
        }

        if (!Tundish.TundishActivo)
            Tundish.NumColadas = 0;
    }

    /// <summary>
    /// Get Boca.
    /// </summary>
    protected BocaView GetBoca(int numeroBoca)
    {
        if (Tundish is null)
            return new BocaView(string.Empty, "ABIERTA");

        return numeroBoca switch
        {
            1 => new BocaView(Tundish.tsCierre1, Tundish.StatusBoca1),
            2 => new BocaView(Tundish.tsCierre2, Tundish.StatusBoca2),
            3 => new BocaView(Tundish.tsCierre3, Tundish.StatusBoca3),
            4 => new BocaView(Tundish.tsCierre4, Tundish.StatusBoca4),
            5 => new BocaView(Tundish.tsCierre5, Tundish.StatusBoca5),
            6 => new BocaView(Tundish.tsCierre6, Tundish.StatusBoca6),
            _ => new BocaView(string.Empty, "ABIERTA")
        };
    }

    /// <summary>
    /// Get Estado Color.
    /// </summary>
    protected static string GetEstadoColor(string? estado) => estado switch
    {
        "ABIERTA" => "#31b404",
        "CERRADA" => "#ffbf00",
        "BLOQUEDA/PERFORADA" => "#ff8000",
        _ => "#e5e7eb"
    };

    /// <summary>
    /// Get Tipo Class.
    /// </summary>
    protected static string GetTipoClass(string? tipo) => tipo?.Trim().ToUpperInvariant() switch
    {
        "BB1" => "bb1",
        "BB2" => "bb2",
        "BB3" => "bb3",
        _ => string.Empty
    };

    /// <summary>
    /// Format Number.
    /// </summary>
    protected static string FormatNumber(decimal? value) => (value ?? 0).ToString("N2", CultureInfo.CurrentCulture);

    /// <summary>
    /// Cargar Datos Estandar.
    /// </summary>
    private void CargarDatosEstandar(ListTundishStandard tundishEstandar)
    {
        if (Tundish is null)
            return;

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
    }

    /// <summary>
    /// Configurar Boca.
    /// </summary>
    private static void ConfigurarBoca(ListTundishDisponibles tundish, string idTipoBB, string estatusBoca, int numeroBoca)
    {
        var estatusAnterior = ObtenerStatusBocaActual(tundish, numeroBoca);

        switch (estatusBoca)
        {
            case "ABIERTA":
                AjustarTotal(idTipoBB, tundish, +1);
                AsignarStatusBoca(tundish, numeroBoca, "ABIERTA");
                break;
            case "CERRADA":
                if (estatusAnterior != "BLOQUEDA/PERFORADA")
                    AjustarTotal(idTipoBB, tundish, -1);
                AsignarStatusBoca(tundish, numeroBoca, "CERRADA");
                break;
            case "BLOQUEDA/PERFORADA":
                if (estatusAnterior != "CERRADA")
                    AjustarTotal(idTipoBB, tundish, -1);
                AsignarStatusBoca(tundish, numeroBoca, "BLOQUEDA/PERFORADA");
                break;
        }
    }

    /// <summary>
    /// Ajustar Total.
    /// </summary>
    private static void AjustarTotal(string idTipoBB, ListTundishDisponibles tundish, int delta)
    {
        switch (idTipoBB)
        {
            case "BB1":
                tundish.tstotalBB1 = Math.Max(0, tundish.tstotalBB1 + delta);
                break;
            case "BB2":
                tundish.tstotalBB2 = Math.Max(0, tundish.tstotalBB2 + delta);
                break;
            case "BB3":
                tundish.tstotalBB3 = Math.Max(0, tundish.tstotalBB3 + delta);
                break;
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
    private static void AsignarStatusBoca(ListTundishDisponibles tundish, int numeroBoca, string nuevoStatus)
    {
        switch (numeroBoca)
        {
            case 1: tundish.StatusBoca1 = nuevoStatus; break;
            case 2: tundish.StatusBoca2 = nuevoStatus; break;
            case 3: tundish.StatusBoca3 = nuevoStatus; break;
            case 4: tundish.StatusBoca4 = nuevoStatus; break;
            case 5: tundish.StatusBoca5 = nuevoStatus; break;
            case 6: tundish.StatusBoca6 = nuevoStatus; break;
        }
    }

    /// <summary>
    /// Notify Changed.
    /// </summary>
    private async Task NotifyChanged()
    {
        if (Tundish is not null && TundishChanged.HasDelegate)
            await TundishChanged.InvokeAsync(Tundish);
    }

    protected readonly record struct BocaView(string Tipo, string Estado);
    protected readonly record struct BocaEstadoOption(string Text, string Value);
}
