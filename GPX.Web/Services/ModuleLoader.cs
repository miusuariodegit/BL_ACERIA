using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.JSInterop;

// [GPX-DOC-v1] ================================================================================
// Carga modulos de JavaScript de forma segura desde Blazor Server.
// ================================================================================================

namespace GPX.Web.Services;

/// <summary>
/// Clase ModuleLoader. Carga modulos de JavaScript de forma segura desde Blazor Server.
/// </summary>
public class ModuleLoader : IAsyncDisposable {
    readonly CancellationTokenSource _disposeCts = new();
    readonly IJSRuntime _jsRuntime;
    readonly ConcurrentDictionary<string, ValueTask<IJSObjectReference?>> _modules = new();

    /// <summary>
    /// Inicializa una nueva instancia de la clase ModuleLoader.
    /// </summary>
    public ModuleLoader(IJSRuntime jsRuntime) {
        _jsRuntime = jsRuntime;
    }

    /// <summary>
    /// Carga de forma segura un modulo JavaScript, evitando errores si el circuito de Blazor ya se ha
    /// cerrado.
    /// </summary>
    public async ValueTask<IJSObjectReference?> GetJSModuleSafeAsync(string jsModule) {
        return await _modules.GetOrAdd(jsModule, async moduleName => {
            try {
                return await _jsRuntime.InvokeAsync<IJSObjectReference>("import", _disposeCts.Token, $"./scripts/{jsModule}");
            } catch {
                return null;
            }
        });
    }

    /// <summary>
    /// Libera los recursos del modulo JavaScript cargado.
    /// </summary>
    public async ValueTask DisposeAsync() {
        _disposeCts.Cancel();
        try {
            foreach(var item in _modules) {
                var module = await item.Value;
                if(module != null)
                    await module.DisposeAsync();
            }
            _modules.Clear();
        } catch(JSDisconnectedException) { }
        _disposeCts.Dispose();
    }
}
