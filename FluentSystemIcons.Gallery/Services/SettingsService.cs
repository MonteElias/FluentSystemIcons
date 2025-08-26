// FluentSystemIcons/FluentSystemIcons.Gallery/Services/SettingsService.cs
using FluentSystemIcons.Gallery.UserControls;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Windows.Graphics;
using Windows.Storage;

// Define el espacio de nombres para los servicios de la aplicación.
namespace FluentSystemIcons.Gallery.Services
{
    // Enumeración que define los tipos de efectos de fondo disponibles para la ventana.
    public enum BackdropType
    {
        Mica,
        MicaAlt,
        Acrylic,
        None
    }

    // Clase estática que gestiona el guardado y la carga de las configuraciones del usuario.
    // Utiliza ApplicationData.Current.LocalSettings para persistir los datos en el dispositivo local.
    public static class SettingsService
    {
        // Campo estático que proporciona acceso al contenedor de configuraciones locales de la aplicación.
        private static readonly ApplicationDataContainer LocalSettings = ApplicationData.Current.LocalSettings;

        // Definición de las claves utilizadas para almacenar y recuperar cada ajuste.
        // Usar constantes asegura consistencia y evita errores de tipeo.
        private const string GridSizeModeKey = "GridSizeMode";
        private const string ThemeKey = "AppTheme";
        private const string BackdropKey = "AppBackdrop";
        private const string WindowPosXKey = "WindowPositionX";
        private const string WindowPosYKey = "WindowPositionY";
        private const string WindowWidthKey = "WindowWidth";
        private const string WindowHeightKey = "WindowHeight";
        private const string LanguageKey = "AppLanguage";

        #region Posición y Tamaño de la Ventana

        // Guarda la posición (X, Y) y el tamaño (ancho, alto) de la ventana de la aplicación.
        public static void SaveWindowLayout(AppWindow appWindow)
        {
            if (appWindow == null) return;

            // Almacena cada valor en el contenedor de configuraciones locales con su clave correspondiente.
            LocalSettings.Values[WindowPosXKey] = appWindow.Position.X;
            LocalSettings.Values[WindowPosYKey] = appWindow.Position.Y;
            LocalSettings.Values[WindowWidthKey] = appWindow.Size.Width;
            LocalSettings.Values[WindowHeightKey] = appWindow.Size.Height;
        }

        // Carga la última posición y tamaño guardados de la ventana.
        public static RectInt32? LoadWindowLayout()
        {
            // Comprueba si todas las claves necesarias para el diseño de la ventana existen.
            if (LocalSettings.Values.ContainsKey(WindowWidthKey) &&
                LocalSettings.Values.ContainsKey(WindowHeightKey) &&
                LocalSettings.Values.ContainsKey(WindowPosXKey) &&
                LocalSettings.Values.ContainsKey(WindowPosYKey))
            {
                // Si existen, recupera los valores y los convierte a enteros.
                int width = (int)LocalSettings.Values[WindowWidthKey];
                int height = (int)LocalSettings.Values[WindowHeightKey];
                int posX = (int)LocalSettings.Values[WindowPosXKey];
                int posY = (int)LocalSettings.Values[WindowPosYKey];

                // Devuelve un objeto RectInt32 con la geometría de la ventana guardada.
                return new RectInt32(posX, posY, width, height);
            }

            // Si falta alguna clave, devuelve null, indicando que no hay un diseño guardado.
            return null;
        }

        #endregion

        #region Modo de Tamaño de la Cuadrícula

        // Guarda el modo de tamaño de la cuadrícula de iconos seleccionado por el usuario.
        public static void SaveGridSizeMode(GridSizeMode mode)
        {
            // Convierte el valor del enum a string y lo guarda en la configuración local.
            LocalSettings.Values[GridSizeModeKey] = mode.ToString();
        }

        // Carga el modo de tamaño de la cuadrícula guardado.
        public static GridSizeMode LoadGridSizeMode()
        {
            // Intenta obtener el valor asociado a la clave.
            if (LocalSettings.Values.TryGetValue(GridSizeModeKey, out object? value))
            {
                // Comprueba si el valor es un string.
                if (value is string modeString)
                {
                    // Intenta convertir el string de vuelta a un valor del enum GridSizeMode.
                    if (System.Enum.TryParse<GridSizeMode>(modeString, out var mode))
                    {
                        // Si tiene éxito, devuelve el valor guardado.
                        return mode;
                    }
                }
            }
            // Si no se encuentra un valor, no es válido o no se puede convertir, devuelve el valor por defecto.
            return GridSizeMode.Medium;
        }

        #endregion

        #region Tema de la Aplicación

        // Guarda el tema de la aplicación (Claro, Oscuro o Sistema) seleccionado por el usuario.
        public static void SaveTheme(ElementTheme theme)
        {
            // Convierte el valor del enum a string y lo almacena.
            LocalSettings.Values[ThemeKey] = theme.ToString();
        }

        // Carga el tema de la aplicación guardado.
        public static ElementTheme LoadTheme()
        {
            // Intenta obtener el valor asociado a la clave del tema.
            if (LocalSettings.Values.TryGetValue(ThemeKey, out object? value))
            {
                // Comprueba si el valor es un string.
                if (value is string themeString)
                {
                    // Intenta convertir el string de vuelta a un valor del enum ElementTheme.
                    if (System.Enum.TryParse<ElementTheme>(themeString, out var theme))
                    {
                        // Si tiene éxito, devuelve el tema guardado.
                        return theme;
                    }
                }
            }
            // Si no se encuentra un valor o no es válido, devuelve el tema por defecto del sistema.
            return ElementTheme.Default;
        }

        #endregion

        #region Fondo de la Aplicación

        // Guarda el tipo de efecto de fondo (Mica, Acrílico, etc.) seleccionado por el usuario.
        public static void SaveBackdrop(BackdropType backdrop)
        {
            // Convierte el valor del enum a string para su almacenamiento.
            LocalSettings.Values[BackdropKey] = backdrop.ToString();
        }

        // Carga el tipo de efecto de fondo guardado.
        public static BackdropType LoadBackdrop()
        {
            // Intenta obtener el valor asociado a la clave del fondo.
            if (LocalSettings.Values.TryGetValue(BackdropKey, out object? value))
            {
                // Comprueba si el valor es un string.
                if (value is string backdropString)
                {
                    // Intenta convertir el string de vuelta a un valor del enum BackdropType.
                    if (System.Enum.TryParse<BackdropType>(backdropString, out var backdrop))
                    {
                        // Si tiene éxito, devuelve el tipo de fondo guardado.
                        return backdrop;
                    }
                }
            }
            // Si no se encuentra un valor o no es válido, devuelve el tipo de fondo por defecto.
            return BackdropType.Mica;
        }

        #endregion

        #region Idioma de la Aplicación

        // Guarda la etiqueta de idioma (ej. "en-US", "es-ES") seleccionada por el usuario.
        public static void SaveLanguage(string langTag)
        {
            LocalSettings.Values[LanguageKey] = langTag;
        }

        // Carga la etiqueta de idioma guardada.
        public static string LoadLanguage()
        {
            // Intenta obtener el valor y comprueba si es un string no nulo.
            if (LocalSettings.Values.TryGetValue(LanguageKey, out object? value) && value is string langTag)
            {
                // Devuelve la etiqueta de idioma guardada.
                return langTag;
            }
            // Si no hay ningún idioma guardado, devuelve "System" como valor predeterminado,
            // indicando que la aplicación debe usar el idioma del sistema operativo.
            return "System";
        }

        #endregion
    }
}