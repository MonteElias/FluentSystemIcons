// FluentSystemIcons/FluentSystemIcons.Gallery/UserControls/SettingsControl.xaml.cs
using FluentSystemIcons.Gallery.Services;
using Microsoft.UI.Xaml;
using System.Linq;
using Microsoft.UI.Xaml.Controls;
using System;
using Windows.ApplicationModel;

// Define el espacio de nombres para los controles de usuario personalizados de la galería.
namespace FluentSystemIcons.Gallery.UserControls
{
    // Define la clase para el control de usuario SettingsControl, que contiene las opciones de configuración de la aplicación.
    // Es 'sealed' para no poder ser heredada y 'partial' por su vínculo con un archivo XAML.
    public sealed partial class SettingsControl : UserControl
    {
        // Define un evento que se dispara cuando el usuario selecciona un nuevo tema (Claro, Oscuro, Sistema).
        // La ventana principal (MainWindow) se suscribirá a este evento para aplicar el cambio.
        public event Action<ElementTheme>? ThemeChanged;

        // Define un evento que se dispara cuando el usuario selecciona un nuevo efecto de fondo (Mica, Acrílico, etc.).
        // La ventana principal se suscribirá para actualizar el fondo de la ventana.
        public event Action<BackdropType>? BackdropChanged;

        // Define un evento que se dispara cuando el usuario selecciona un nuevo idioma.
        public event Action<string>? LanguageChanged;

        // Constructor de la clase SettingsControl.
        public SettingsControl()
        {
            // Carga el XAML asociado y construye el árbol visual de los componentes del control.
            this.InitializeComponent();
            // Llama al método para obtener y mostrar la versión actual de la aplicación.
            AppVersionTextBlock.Text = GetAppVersion();
        }

        // Manejador del evento que se activa cuando cambia la selección en los RadioButtons del tema.
        private void ThemeRadioButtons_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Se asegura de que el emisor sea el control RadioButtons y que haya un elemento seleccionado.
            if (sender is RadioButtons radioButtons && radioButtons.SelectedItem is RadioButton selectedRadioButton)
            {
                // Convierte la propiedad 'Tag' (un string como "Light") del RadioButton seleccionado al tipo enumerado 'ElementTheme'.
                var theme = (string)selectedRadioButton.Tag switch
                {
                    "Light" => ElementTheme.Light,
                    "Dark" => ElementTheme.Dark,
                    _ => ElementTheme.Default, // Por defecto, se usa la configuración del sistema.
                };

                // Dispara el evento 'ThemeChanged', pasando el nuevo tema seleccionado como argumento.
                ThemeChanged?.Invoke(theme);
            }
        }

        // Manejador del evento que se activa cuando cambia la selección en el ComboBox del efecto de fondo.
        private void BackdropComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Se asegura de que el emisor sea el ComboBox y que haya un elemento seleccionado.
            if (sender is ComboBox comboBox && comboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                // Intenta convertir la propiedad 'Tag' (un string como "Mica") del item seleccionado al tipo enumerado 'BackdropType'.
                if (Enum.TryParse<BackdropType>((string)selectedItem.Tag, out var backdrop))
                {
                    // Si la conversión es exitosa, dispara el evento 'BackdropChanged' con el nuevo valor.
                    BackdropChanged?.Invoke(backdrop);
                }
            }
        }

        // Método público que permite a la ventana principal establecer el estado inicial de los RadioButtons de tema al cargar la aplicación.
        public void InitializeTheme(ElementTheme theme)
        {
            // Selecciona el RadioButton correspondiente basándose en el tema cargado desde la configuración.
            switch (theme)
            {
                case ElementTheme.Light:
                    ThemeRadioButtons.SelectedIndex = 0;
                    break;
                case ElementTheme.Dark:
                    ThemeRadioButtons.SelectedIndex = 1;
                    break;
                case ElementTheme.Default:
                default:
                    ThemeRadioButtons.SelectedIndex = 2;
                    break;
            }
        }

        // Método público que permite a la ventana principal establecer el estado inicial del ComboBox de efecto de fondo.
        public void InitializeBackdrop(BackdropType backdrop)
        {
            // Selecciona el item del ComboBox correspondiente basándose en el tipo de fondo cargado desde la configuración.
            switch (backdrop)
            {
                case BackdropType.MicaAlt:
                    BackdropComboBox.SelectedIndex = 1;
                    break;
                case BackdropType.Acrylic:
                    BackdropComboBox.SelectedIndex = 2;
                    break;
                case BackdropType.None:
                    BackdropComboBox.SelectedIndex = 3;
                    break;
                case BackdropType.Mica:
                default:
                    BackdropComboBox.SelectedIndex = 0;
                    break;
            }
        }

        // Manejador del evento que se activa cuando cambia la selección en el ComboBox de idioma.
        private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Se asegura de que el emisor sea el ComboBox y que haya un elemento seleccionado.
            if (sender is ComboBox comboBox && comboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                // Obtiene la etiqueta de idioma (ej. "en-US", "es-ES") de la propiedad 'Tag' del item.
                var langTag = (string)selectedItem.Tag;
                // Dispara el evento 'LanguageChanged' con la nueva etiqueta de idioma.
                LanguageChanged?.Invoke(langTag);
            }
        }

        // Método público para establecer el estado inicial del ComboBox de idioma.
        public void InitializeLanguage(string langTag)
        {
            // Busca en los items del ComboBox aquel cuyo 'Tag' coincida con la etiqueta de idioma guardada.
            var itemToSelect = LanguageComboBox.Items
                                .OfType<ComboBoxItem>()
                                .FirstOrDefault(item => (string)item.Tag == langTag);

            if (itemToSelect != null)
            {
                // Si se encuentra un item coincidente, lo selecciona.
                LanguageComboBox.SelectedItem = itemToSelect;
            }
            else
            {
                // Si no se encuentra (o el idioma guardado no es válido), selecciona la opción por defecto ("Sistema").
                LanguageComboBox.SelectedIndex = 0;
            }
        }

        // Método privado que obtiene la versión de la aplicación para mostrarla en la UI de configuración.
        private string GetAppVersion()
        {
            try
            {
                // Intenta obtener la información de la versión del paquete de la aplicación.
                var packageVersion = Package.Current.Id.Version;
                // Formatea la versión en un string legible (ej. "v1.2.3").
                return $"v{packageVersion.Major}.{packageVersion.Minor}.{packageVersion.Build}";
            }
            catch (Exception)
            {
                // Si ocurre un error (por ejemplo, al ejecutar en modo de depuración sin empaquetar),
                // devuelve una cadena de texto predeterminada.
                return "v1.0.0 (Debug)";
            }
        }
    }
}