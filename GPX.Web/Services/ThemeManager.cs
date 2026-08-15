using GPX.Web.Utils;
using DevExpress.Blazor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Configuration;
using Microsoft.JSInterop;

// [GPX-DOC-v1] ================================================================================
// Gestiona el tema claro/oscuro de la interfaz y su persistencia.
// ================================================================================================

namespace GPX.Web.Services {
    /// <summary>
    /// Clase ThemeManager. Gestiona el tema claro/oscuro de la interfaz y su persistencia.
    /// </summary>
    public class ThemeManager {
        private const string IsDarkThemeCookieKey = "is-dark-theme";
        private const string DefaultPresetColor = nameof(ThemeFluentAccentColor.Blue);

        private readonly ThemeFluentAccentColor _accentColor;
        private readonly ModuleLoader _moduleLoader;
        private readonly PersistentComponentState _componentState;
        private readonly PersistingComponentStateSubscription? _subscription;
        private readonly IThemeChangeService _themeService;

        /// <summary>
        /// Inicializa una nueva instancia de la clase ThemeManager.
        /// </summary>
        public ThemeManager(
            IConfiguration configuration,
            ModuleLoader moduleLoader,
            IThemeChangeService themeService,
            PersistentComponentState componentState) {
            _moduleLoader = moduleLoader;
            _themeService = themeService;
            _componentState = componentState;
            _accentColor = ResolveAccentColor(configuration["ThemeSettings:PresetColor"]);
            IsDarkTheme = string.Equals(configuration["ThemeSettings:ColorMode"], "Dark", StringComparison.OrdinalIgnoreCase);

            if(componentState.TryTakeFromJson(nameof(ThemeInfo), out ThemeInfo? themeInfo)) {
                IsDarkTheme = themeInfo?.IsDarkTheme ?? IsDarkTheme;
            } else {
                _subscription = _componentState.RegisterOnPersisting(() => {
                    componentState.PersistAsJson(nameof(ThemeInfo), new ThemeInfo { IsDarkTheme = IsDarkTheme });
                    return Task.CompletedTask;
                }, RenderMode.InteractiveServer);
            }
        }

        public bool IsDarkTheme { get; private set; }
        public string CurrentColorMode => IsDarkTheme ? "Dark" : "Light";
        public string CurrentPresetColor => _accentColor.ToString();
        public ITheme CurrentTheme => CreateTheme(IsDarkTheme);

        /// <summary>
        /// Alterna entre el tema claro y oscuro y persiste la preferencia.
        /// </summary>
        public async Task ToggleThemeAsync() {
            IsDarkTheme = !IsDarkTheme;
            if(await _themeService.SetTheme(CurrentTheme)) {
                var module = await _moduleLoader.GetJSModuleSafeAsync("utils.js");
                if(module != null) {
                    var themeData = new KeyValuePairSerializer<string, bool>(IsDarkThemeCookieKey, IsDarkTheme);
                    await module.InvokeVoidAsync("setThemeData", themeData);
                }
            }
        }

        /// <summary>
        /// Obtiene el tema actual a partir de las preferencias almacenadas.
        /// </summary>
        public void ObtainTheme(IEnumerable<KeyValuePairSerializer<string, string>>? cookie) {
            var record = cookie?.FirstOrDefault(entry => entry.Key == IsDarkThemeCookieKey)?.ToKeyValuePair;
            if(record.HasValue && bool.TryParse(record.Value.Value, out var isDarkTheme)) {
                IsDarkTheme = isDarkTheme;
            }
        }

        /// <summary>
        /// Create Theme.
        /// </summary>
        private ITheme CreateTheme(bool isDarkTheme) =>
            Themes.Fluent.Clone(properties => {
                properties.Name = isDarkTheme
                    ? $"Fluent-{_accentColor}-Dark"
                    : $"Fluent-{_accentColor}-Light";
                properties.Mode = isDarkTheme ? ThemeMode.Dark : ThemeMode.Light;
                properties.AccentColor = _accentColor;
                properties.ApplyToPageElements = true;
            });

        /// <summary>
        /// Resolve Accent Color.
        /// </summary>
        private static ThemeFluentAccentColor ResolveAccentColor(string? presetColor) =>
            Enum.TryParse<ThemeFluentAccentColor>(presetColor, true, out var parsedColor)
                ? parsedColor
                : Enum.Parse<ThemeFluentAccentColor>(DefaultPresetColor, true);
    }
}
