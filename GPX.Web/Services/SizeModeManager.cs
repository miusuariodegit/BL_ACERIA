using DevExpress.Blazor;
using Microsoft.JSInterop;

// [GPX-DOC-v1] ================================================================================
// Gestiona el modo de tamano de la interfaz (compacto/normal) y su persistencia.
// ================================================================================================

namespace GPX.Web.Services;

/// <summary>
/// Clase SizeModeManager. Gestiona el modo de tamano de la interfaz (compacto/normal) y su
/// persistencia.
/// </summary>
public class SizeModeManager {
    readonly ModuleLoader _moduleLoader;
    /// <summary>
    /// Inicializa una nueva instancia de la clase SizeModeManager.
    /// </summary>
    public SizeModeManager(ModuleLoader moduleLoader) {
        _moduleLoader = moduleLoader;
    }

    /// <summary>
    /// Cambia el modo de tamano de la interfaz y persiste la preferencia.
    /// </summary>
    public async ValueTask SwitchToSizeModeAsync(SizeMode sizeMode) {
        var module = await _moduleLoader.GetJSModuleSafeAsync("utils.js");
        if(module != null)
            await module.InvokeVoidAsync("setBodyClass", GetClassName(sizeMode));
    }

    /// <summary>
    /// Obtiene la clase CSS asociada a un modo de tamano.
    /// </summary>
    public string GetClassName(SizeMode sizeMode) => sizeMode switch {
        SizeMode.Small => "small-size",
        SizeMode.Large => "large-size",
        _ => "medium-size"
    };
}
