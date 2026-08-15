using System.Text.Json.Serialization;

// [GPX-DOC-v1] ================================================================================
// Estructura clave/valor usada para (de)serializar preferencias de usuario en almacenamiento
// persistente.
// ================================================================================================

//https://github.com/dotnet/aspnetcore/issues/52947
namespace GPX.Web.Utils {
    /// <summary>
    /// Clase KeyValuePairSerializer. Estructura clave/valor usada para (de)serializar preferencias de
    /// usuario en almacenamiento persistente.
    /// </summary>
    public class KeyValuePairSerializer<TKey, TValue> {
        /// <summary>
        /// Inicializa una nueva instancia de la clase KeyValuePairSerializer.
        /// </summary>
        public KeyValuePairSerializer(TKey key, TValue value) {
            Key = key;
            Value = value;
        }

        public TKey Key { get; set; }

        public TValue Value { get; set; }

        [JsonIgnore] public KeyValuePair<TKey, TValue> ToKeyValuePair => new(Key, Value);
    }
}
