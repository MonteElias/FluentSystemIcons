// FluentSystemIcons/FluentSystemIcons.Gallery/MainWindow.xaml.cs
using FluentSystemIcons.Gallery.Helpers;
using FluentSystemIcons.Gallery.Models;
using FluentSystemIcons.Gallery.Services;
using Microsoft.UI;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Input;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Graphics;
using Windows.UI;
using WinRT.Interop;
using Microsoft.Windows.ApplicationModel.Resources;

// Define el espacio de nombres principal de la aplicación de la galería.
namespace FluentSystemIcons.Gallery
{
    // Define la clase de la ventana principal. Es 'sealed' para no poder ser heredada, 'partial' porque está vinculada a un archivo XAML,
    // y hereda de 'Window' de WinUI, implementando 'INotifyPropertyChanged' para notificar a la UI sobre cambios en las propiedades.
    public sealed partial class MainWindow : Window, INotifyPropertyChanged
    {
        // Campo para interactuar con la ventana de la aplicación a un nivel más bajo (tamaño, posición, barra de título).
        private AppWindow _appWindow;
        // Lista interna que almacena todos los iconos cargados desde los ensamblados.
        private List<IconInfo> _allIcons = new();
        // Colección observable de iconos que se muestra en la UI. Se actualiza dinámicamente al filtrar.
        public ObservableCollection<IconInfo> FilteredIcons { get; set; }

        // Banderas de estado para controlar la lógica de la aplicación.
        private bool _isWindowLoaded = false;
        private bool _isSidePanelVisible = false;

        // Constantes para los valores de opacidad de los botones de la barra de título en diferentes estados.
        private const byte HOVER_OPACITY = 0x15;
        private const byte PRESSED_OPACITY = 0x22;

        // Evento requerido por la interfaz INotifyPropertyChanged.
        public event PropertyChangedEventHandler? PropertyChanged;
        // Método pomocniczy para invocar el evento PropertyChanged.
        private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        // Campo privado para la propiedad ThemeButtonToolTip.
        private string _themeButtonToolTip = string.Empty;
        // Propiedad pública para el texto de ayuda del botón de tema. Es enlazable en XAML.
        public string ThemeButtonToolTip
        {
            get => _themeButtonToolTip;
            set { if (_themeButtonToolTip != value) { _themeButtonToolTip = value; OnPropertyChanged(nameof(ThemeButtonToolTip)); } }
        }

        // Constructor de la ventana principal.
        public MainWindow()
        {
            // Inicializa los componentes definidos en el archivo XAML.
            this.InitializeComponent();

            // [SOLUCIÓN] Se elimina la inicialización de _resourceLoader.
            // _resourceLoader = new ResourceLoader();

            // Obtiene la instancia de AppWindow para esta ventana.
            _appWindow = this.AppWindow;
            // Suscribe un método al evento de cierre de la ventana para guardar su estado.
            _appWindow.Closing += AppWindow_Closing;
            // Inicializa el tamaño y la posición de la ventana.
            InitializeWindowLayout();

            // Carga y aplica el tema (Claro/Oscuro) guardado.
            var theme = SettingsService.LoadTheme();
            if (this.Content is FrameworkElement rootElement)
            {
                rootElement.RequestedTheme = theme;
            }

            // Carga y aplica el efecto de fondo (Mica/Acrílico) guardado.
            var backdrop = SettingsService.LoadBackdrop();
            SetSystemBackdrop(backdrop);

            // Actualiza el texto de ayuda del botón de tema.
            UpdateThemeButtonToolTip();
            // Inicializa los controles en el menú de configuración con los valores guardados.
            SettingsFlyoutContent.InitializeTheme(theme);
            SettingsFlyoutContent.InitializeBackdrop(backdrop);
            SettingsFlyoutContent.InitializeLanguage(SettingsService.LoadLanguage());

            // Suscribe la ventana a los eventos de cambio desde el menú de configuración.
            SettingsFlyoutContent.ThemeChanged += OnThemeChangedFromSettings;
            SettingsFlyoutContent.BackdropChanged += OnBackdropChangedFromSettings;
            SettingsFlyoutContent.LanguageChanged += OnLanguageChangedFromSettings;

            // Establece los colores personalizados para la barra de título.
            SetTitleBarColors();
            // Actualiza el color de fondo de la ventana principal.
            UpdateRootBackground();

            // Configura la barra de título personalizada y sus regiones interactivas.
            AppTitleBar.Loaded += AppTitleBar_Loaded;
            AppTitleBar.SizeChanged += AppTitleBar_SizeChanged;
            ExtendsContentIntoTitleBar = true; // Habilita la barra de título personalizada.
            RootGrid.Loaded += RootGrid_Loaded; // Inicia la carga de contenido cuando la rejilla principal está lista.
            FilteredIcons = new ObservableCollection<IconInfo>(); // Inicializa la colección de iconos.

            // Suscribe un método al evento de activación de la ventana.
            Activated += MainWindow_Activated;
        }

        // Inicializa la posición y el tamaño de la ventana, restaurando la última sesión si existe.
        private void InitializeWindowLayout()
        {
            var savedLayout = SettingsService.LoadWindowLayout();
            if (savedLayout.HasValue)
            {
                _appWindow.MoveAndResize(savedLayout.Value); // Restaura el tamaño y posición guardados.
            }
            else
            {
                _appWindow.Resize(new SizeInt32(1280, 800)); // Establece un tamaño predeterminado.
            }
        }

        // Se ejecuta justo antes de que la ventana se cierre.
        private void AppWindow_Closing(AppWindow sender, AppWindowClosingEventArgs args)
        {
            // Guarda el tamaño y la posición actuales de la ventana.
            SettingsService.SaveWindowLayout(_appWindow);
        }

        // Se ejecuta cuando la ventana se activa o desactiva.
        private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
        {
            // Si la ventana no está desactivada, actualiza los colores de la barra de título.
            if (args.WindowActivationState != WindowActivationState.Deactivated)
            {
                SetTitleBarColors();
            }
        }

        // Configura los colores de los botones de la barra de título (minimizar, maximizar, cerrar) según el tema.
        private void SetTitleBarColors()
        {
            if (_appWindow == null) return;

            var titleBar = _appWindow.TitleBar;
            if (this.Content is not FrameworkElement rootElement) return;
            var theme = rootElement.ActualTheme; // Obtiene el tema actual (Claro/Oscuro).

            // Hace que el fondo de los botones sea transparente.
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

            // Aplica colores diferentes para tema oscuro y claro.
            if (theme == ElementTheme.Dark)
            {
                titleBar.ButtonForegroundColor = Colors.White;
                titleBar.ButtonHoverForegroundColor = Colors.White;
                titleBar.ButtonPressedForegroundColor = Colors.White;
                titleBar.ButtonHoverBackgroundColor = Color.FromArgb(HOVER_OPACITY, 0xFF, 0xFF, 0xFF);
                titleBar.ButtonPressedBackgroundColor = Color.FromArgb(PRESSED_OPACITY, 0xFF, 0xFF, 0xFF);
            }
            else
            {
                titleBar.ButtonForegroundColor = Colors.Black;
                titleBar.ButtonHoverForegroundColor = Colors.Black;
                titleBar.ButtonPressedForegroundColor = Colors.Black;
                titleBar.ButtonHoverBackgroundColor = Color.FromArgb(HOVER_OPACITY, 0x00, 0x00, 0x00);
                titleBar.ButtonPressedBackgroundColor = Color.FromArgb(PRESSED_OPACITY, 0x00, 0x00, 0x00);
            }
        }

        // Actualiza el fondo de la rejilla principal. Es transparente para efectos como Mica/Acrílico, o sólido si no hay efecto.
        private void UpdateRootBackground()
        {
            var backdrop = SettingsService.LoadBackdrop();

            if (backdrop == BackdropType.None)
            {
                // Si no hay efecto, establece un color sólido según el tema.
                var theme = SettingsService.LoadTheme();

                ElementTheme finalTheme;
                if (theme == ElementTheme.Default)
                {
                    finalTheme = (this.Content as FrameworkElement)?.ActualTheme ?? ElementTheme.Default;
                }
                else
                {
                    finalTheme = theme;
                }

                // Asigna el pincel de fondo desde los recursos de la aplicación.
                RootGrid.Background = (SolidColorBrush)Application.Current.Resources[
                    finalTheme == ElementTheme.Dark
                        ? "SolidBackgroundBrushDark"
                        : "SolidBackgroundBrushLight"
                ];
            }
            else
            {
                // Si hay un efecto de fondo, el fondo de la rejilla debe ser transparente para que se vea.
                RootGrid.Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent);
            }
        }

        #region Lógica de Sincronización de Tema y Fondo

        // Método centralizado para cambiar el tema de la aplicación.
        private void ChangeTheme(ElementTheme newTheme)
        {
            if (this.Content is FrameworkElement rootElement && rootElement.RequestedTheme != newTheme)
            {
                rootElement.RequestedTheme = newTheme; // Aplica el nuevo tema.
                SettingsService.SaveTheme(newTheme); // Guarda la preferencia.
                UpdateThemeButtonToolTip(); // Actualiza el texto de ayuda.
                SettingsFlyoutContent.InitializeTheme(newTheme); // Sincroniza con el menú de configuración.
                UpdateRootBackground(); // Actualiza el color de fondo si es necesario.

                // Asegura que los colores de la barra de título se actualicen en el hilo de la UI.
                this.DispatcherQueue.TryEnqueue(() =>
                {
                    SetTitleBarColors();
                });
            }
        }

        // Manejador del clic en el botón de cambio de tema.
        private void ThemeButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.Content is FrameworkElement rootElement)
            {
                // Alterna entre tema oscuro y claro.
                var newTheme = rootElement.ActualTheme == ElementTheme.Dark ? ElementTheme.Light : ElementTheme.Dark;
                ChangeTheme(newTheme);
            }
        }

        // Actualiza el texto de ayuda (tooltip) del botón de tema para reflejar la acción que realizará.
        private void UpdateThemeButtonToolTip()
        {
            if (this.Content is FrameworkElement rootElement)
            {
                var resourceManager = new ResourceManager(); // Administrador para obtener recursos de texto.

                // [SOLUCIÓN] Usar los nuevos nombres de clave sin puntos.
                var resourceKey = rootElement.ActualTheme == ElementTheme.Dark
                    ? "ThemeButton_Dark_ToolTip"
                    : "ThemeButton_Light_ToolTip";

                // Esta línea ahora funcionará porque la clave es simple y no tiene jerarquía.
                ThemeButtonToolTip = resourceManager.MainResourceMap.GetValue($"Resources/{resourceKey}").ValueAsString;
            }
        }

        // Se ejecuta cuando el tema se cambia desde el menú de configuración.
        private void OnThemeChangedFromSettings(ElementTheme newTheme)
        {
            ChangeTheme(newTheme);
        }

        // Se ejecuta cuando el tipo de fondo se cambia desde el menú de configuración.
        private void OnBackdropChangedFromSettings(BackdropType newBackdrop)
        {
            SetSystemBackdrop(newBackdrop);
            SettingsService.SaveBackdrop(newBackdrop);
        }

        // Se ejecuta cuando el idioma se cambia desde el menú de configuración.
        private async void OnLanguageChangedFromSettings(string newLang)
        {
            var currentLang = SettingsService.LoadLanguage();
            if (currentLang != newLang)
            {
                SettingsService.SaveLanguage(newLang); // Guarda el nuevo idioma.

                SettingsButton.Flyout.Hide(); // Cierra el menú de configuración.

                // Muestra un diálogo informando que se necesita reiniciar la aplicación.
                var resourceManager = new ResourceManager();
                var dialog = new ContentDialog
                {
                    Title = resourceManager.MainResourceMap.GetValue("Resources/Settings_RestartRequired_Title").ValueAsString,
                    Content = resourceManager.MainResourceMap.GetValue("Resources/Settings_RestartRequired_Message").ValueAsString,
                    CloseButtonText = "OK",
                    XamlRoot = this.Content.XamlRoot
                };
                await dialog.ShowAsync();
            }
        }

        // Aplica el efecto de fondo seleccionado a la ventana.
        private void SetSystemBackdrop(BackdropType type)
        {
            this.SystemBackdrop = null; // Reinicia el fondo actual.

            switch (type)
            {
                case BackdropType.Mica:
                    try { this.SystemBackdrop = new MicaBackdrop() { Kind = MicaKind.Base }; } catch { }
                    break;
                case BackdropType.MicaAlt:
                    try { this.SystemBackdrop = new MicaBackdrop() { Kind = MicaKind.BaseAlt }; } catch { }
                    break;
                case BackdropType.Acrylic:
                    try { this.SystemBackdrop = new DesktopAcrylicBackdrop(); } catch { }
                    break;
                case BackdropType.None:
                default:
                    break; // No hace nada si no se selecciona ningún efecto.
            }

            // Asegura que el fondo de la rejilla se actualice en el hilo de la UI.
            this.DispatcherQueue.TryEnqueue(() =>
            {
                UpdateRootBackground();
            });
        }

        #endregion

        #region Lógica de la Barra de Título Personalizada

        // Manejadores de eventos para establecer las regiones de la barra de título cuando se carga o cambia de tamaño.
        private void AppTitleBar_Loaded(object sender, RoutedEventArgs e) { SetRegionsForCustomTitleBar(); }
        private void AppTitleBar_SizeChanged(object sender, SizeChangedEventArgs e) { SetRegionsForCustomTitleBar(); }

        // Define qué áreas de la barra de título son interactivas (botones) y cuáles son para arrastrar la ventana.
        private void SetRegionsForCustomTitleBar()
        {
            if (ExtendsContentIntoTitleBar)
            {
                double scaleAdjustment = AppTitleBar.XamlRoot.RasterizationScale; // Ajuste para pantallas con alta densidad de píxeles (HiDPI).
                RightPaddingColumn.Width = new GridLength(_appWindow.TitleBar.RightInset / scaleAdjustment);

                // Crea una lista de rectángulos para los controles en la barra de título que deben ser interactivos.
                var rects = new List<RectInt32>
                {
                    GetRect(AppTitlePanel, scaleAdjustment),
                    GetRect(IconSearchBox, scaleAdjustment),
                    GetRect(ThemeButton, scaleAdjustment),
                    GetRect(SettingsButton, scaleAdjustment)
                };

                // Informa al sistema operativo sobre estas regiones para que los clics pasen a través de ellas a los controles.
                InputNonClientPointerSource.GetForWindowId(this.AppWindow.Id).SetRegionRects(NonClientRegionKind.Passthrough, rects.ToArray());
            }
        }

        // Método auxiliar para obtener las coordenadas y el tamaño de un control en la pantalla.
        private RectInt32 GetRect(FrameworkElement element, double scale)
        {
            GeneralTransform transform = element.TransformToVisual(null);
            Rect bounds = transform.TransformBounds(new Rect(0, 0, element.ActualWidth, element.ActualHeight));
            return new RectInt32((int)Math.Round(bounds.X * scale), (int)Math.Round(bounds.Y * scale), (int)Math.Round(bounds.Width * scale), (int)Math.Round(bounds.Height * scale));
        }

        #endregion

        #region Carga de la Aplicación y Lógica Principal

        // Se ejecuta cuando la rejilla principal de la UI está cargada y lista.
        private void RootGrid_Loaded(object sender, RoutedEventArgs e) { this.DispatcherQueue.TryEnqueue(async () => { await PerformInitialLoadAsync(); }); }

        // Realiza la carga inicial de datos y las animaciones de entrada.
        private async Task PerformInitialLoadAsync()
        {
            // Restaura el modo de visualización de la cuadrícula (grande, mediano, pequeño).
            var savedMode = SettingsService.LoadGridSizeMode();
            IconViewSwitcher.CurrentSizeMode = savedMode;
            // Carga todos los iconos de forma asíncrona.
            await LoadAllIconsAsync();

            // [SOLUCIÓN] Usar un ResourceManager local para formatear el contador de iconos.
            var resourceManager = new ResourceManager();
            var formatString = resourceManager.MainResourceMap.GetValue("Resources/IconCount_Text").ValueAsString;
            IconCountTextBlock.Text = string.Format(formatString, _allIcons.Count); // Muestra el total de iconos.

            // Define y ejecuta las animaciones para desvanecer la pantalla de carga y mostrar el contenido principal.
            var splashFadeOutStoryboard = new Storyboard();
            var splashFadeOutAnimation = new DoubleAnimation { To = 0, Duration = new Duration(TimeSpan.FromMilliseconds(300)), EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut } };
            Storyboard.SetTarget(splashFadeOutAnimation, SplashGrid);
            Storyboard.SetTargetProperty(splashFadeOutAnimation, "Opacity");
            splashFadeOutStoryboard.Children.Add(splashFadeOutAnimation);

            var contentFadeInStoryboard = new Storyboard();
            var contentFadeInAnimation = new DoubleAnimation { To = 1, Duration = new Duration(TimeSpan.FromMilliseconds(500)), EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut } };
            Storyboard.SetTarget(contentFadeInAnimation, ContentContainer);
            Storyboard.SetTargetProperty(contentFadeInAnimation, "Opacity");
            contentFadeInStoryboard.Children.Add(contentFadeInAnimation);

            var titleBarFadeInStoryboard = new Storyboard();
            var titleBarFadeInAnimation = new DoubleAnimation { To = 1, Duration = new Duration(TimeSpan.FromMilliseconds(500)), EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut } };
            Storyboard.SetTarget(titleBarFadeInAnimation, AppTitleBar);
            Storyboard.SetTargetProperty(titleBarFadeInAnimation, "Opacity");
            titleBarFadeInStoryboard.Children.Add(titleBarFadeInAnimation);

            // Hace visibles los contenedores antes de iniciar la animación.
            AppTitleBar.Visibility = Visibility.Visible;
            ContentContainer.Visibility = Visibility.Visible;

            // Inicia las animaciones.
            splashFadeOutStoryboard.Begin();
            contentFadeInStoryboard.Begin();
            titleBarFadeInStoryboard.Begin();
            await splashFadeOutStoryboard.BeginAsync(); // Espera a que la pantalla de carga se desvanezca.

            SplashGrid.Visibility = Visibility.Collapsed; // Oculta la pantalla de carga para liberar recursos.
            _isWindowLoaded = true; // Marca la ventana como completamente cargada.
        }

        // Carga todos los iconos desde las clases generadas (ej. FluentSystemIconsRegular) usando reflexión.
        private async Task LoadAllIconsAsync()
        {
            _allIcons = new List<IconInfo>();
            var regularFont = Application.Current.Resources["FluentIconsRegular"] as FontFamily;
            var filledFont = Application.Current.Resources["FluentIconsFilled"] as FontFamily;

            // Ejecuta la carga en un hilo de fondo para no bloquear la UI.
            await Task.Run(() =>
            {
                if (regularFont != null)
                {
                    LoadIconsFromType(typeof(FluentSystemIconsRegular), regularFont, "FluentIconsRegular");
                }
                if (filledFont != null)
                {
                    LoadIconsFromType(typeof(FluentSystemIconsFilled), filledFont, "FluentSystemIconsFilled");
                }
            });

            // Añade todos los iconos cargados a la colección observable que se muestra en la UI.
            _allIcons.ForEach(icon => FilteredIcons.Add(icon));
        }

        // Lee los campos estáticos de una clase de iconos y crea objetos IconInfo.
        private void LoadIconsFromType(Type type, FontFamily fontFamily, string fontKey)
        {
            // Obtiene todos los campos públicos, estáticos y de tipo string (que son constantes).
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static).Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string));
            // Agrupa los campos por nombre base del icono (ej. todos los campos relacionados con "Home").
            var iconGroups = fields.GroupBy(f => f.Name.Replace("XamlCode", "").Replace("DisplayName", ""));
            foreach (var group in iconGroups)
            {
                // Encuentra los campos específicos para el glifo, el código XAML y el nombre para mostrar.
                var nameField = group.FirstOrDefault(f => !f.Name.EndsWith("XamlCode") && !f.Name.EndsWith("DisplayName"));
                var xamlCodeField = group.FirstOrDefault(f => f.Name.EndsWith("XamlCode"));
                var displayNameField = group.FirstOrDefault(f => f.Name.EndsWith("DisplayName"));

                // Si se encontraron todos los campos necesarios, crea y añade un nuevo objeto IconInfo.
                if (nameField != null && xamlCodeField != null && displayNameField != null)
                {
                    _allIcons.Add(new IconInfo
                    {
                        Name = nameField.Name,
                        DisplayName = displayNameField.GetValue(null) as string ?? string.Empty,
                        Glyph = nameField.GetValue(null) as string ?? string.Empty,
                        XamlCode = xamlCodeField.GetValue(null) as string ?? string.Empty,
                        FontFamily = fontFamily,
                        FontKey = fontKey
                    });
                }
            }
        }
        #endregion

        #region Lógica de Búsqueda

        // Se ejecuta cada vez que el texto en el cuadro de búsqueda cambia por acción del usuario.
        private void IconSearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                var searchTerm = sender.Text;
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    sender.ItemsSource = null; // Borra las sugerencias si no hay texto.
                    UpdateFilteredIcons(string.Empty); // Muestra todos los iconos.
                }
                else
                {
                    sender.ItemsSource = GetRankedSearchResults(searchTerm); // Muestra sugerencias basadas en la búsqueda.
                }
            }
        }

        // Se ejecuta cuando el usuario envía la búsqueda (ej. presiona Enter).
        private void IconSearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            var chosenSuggestion = args.ChosenSuggestion as IconInfo;
            var searchTerm = chosenSuggestion?.DisplayName ?? args.QueryText;
            UpdateFilteredIcons(searchTerm); // Filtra la cuadrícula de iconos con el término de búsqueda.
        }

        // Se ejecuta cuando el usuario elige una sugerencia de la lista.
        private void IconSearchBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            var selectedIcon = args.SelectedItem as IconInfo;
            if (selectedIcon != null)
            {
                sender.Text = selectedIcon.DisplayName; // Actualiza el texto del cuadro de búsqueda.
                UpdateFilteredIcons(selectedIcon.DisplayName); // Filtra la cuadrícula.
            }
        }

        // Actualiza la colección FilteredIcons basándose en un término de búsqueda.
        private void UpdateFilteredIcons(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                FilteredIcons.Clear();
                _allIcons.ForEach(icon => FilteredIcons.Add(icon)); // Si no hay búsqueda, muestra todos los iconos.
                return;
            }
            var results = GetRankedSearchResults(searchTerm); // Obtiene los resultados de búsqueda.
            FilteredIcons.Clear();
            results.ForEach(icon => FilteredIcons.Add(icon)); // Muestra solo los resultados.
        }

        // Obtiene una lista de iconos clasificados por relevancia según el término de búsqueda.
        private List<IconInfo> GetRankedSearchResults(string searchTerm)
        {
            return _allIcons
                .Select(icon => new { Icon = icon, Score = CalculateScore(icon, searchTerm) }) // Calcula una puntuación para cada icono.
                .Where(item => item.Score > 0) // Descarta los que no coinciden.
                .OrderByDescending(item => item.Score) // Ordena por la puntuación más alta.
                .Select(item => item.Icon)
                .Take(100) // Limita el número de resultados.
                .ToList();
        }

        // Algoritmo para calcular la puntuación de coincidencia de un icono con un término de búsqueda.
        private int CalculateScore(IconInfo icon, string searchTerm)
        {
            var name = icon.Name.ToLowerInvariant();
            var displayName = icon.DisplayName.ToLowerInvariant();
            var term = searchTerm.ToLowerInvariant();

            // Regla 1: Coincidencia exacta (puntuación máxima).
            if (name.Equals(term) || displayName.Equals(term)) return 100;

            if (!string.IsNullOrEmpty(icon.XamlCode))
            {
                var hexCode = icon.XamlCode.Replace("&#x", "").Replace(";", "").ToLowerInvariant();

                // Regla 2: Coincidencia con el código hexadecimal (muy alta prioridad).
                if (hexCode.Equals(term)) return 90;
                if (hexCode.StartsWith(term)) return 60;
            }

            // Reglas de coincidencia parcial con menor prioridad.
            if (name.StartsWith(term)) return 50;
            if (displayName.StartsWith(term)) return 40;
            if (name.Contains(term)) return 10;
            if (displayName.Contains(term)) return 5;

            return 0; // No hay coincidencia.
        }

        #endregion

        #region Manejadores de Eventos de la UI (Panel Lateral, GridView, etc.)

        // Manejador del menú contextual para copiar el glifo del icono.
        private void CopyGlyph_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is IconInfo icon) { var dataPackage = new DataPackage(); dataPackage.SetText(icon.XamlCode); Clipboard.SetContent(dataPackage); }
        }

        // Manejador del menú contextual para copiar el código XAML de FontIcon.
        private void CopyFontIcon_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is IconInfo icon) { string fontIconXaml = $"<FontIcon FontFamily=\"{{StaticResource {icon.FontKey}}}\" Glyph=\"{icon.XamlCode}\" />"; var dataPackage = new DataPackage(); dataPackage.SetText(fontIconXaml); Clipboard.SetContent(dataPackage); }
        }

        // Se ejecuta cuando el usuario cambia el tamaño de los iconos en la cuadrícula.
        private async void IconViewSwitcher_SelectionChanged(UserControls.GridSizeSwitcher sender, UserControls.GridSizeMode args)
        {
            if (!_isWindowLoaded) { ApplyTemplateWithoutAnimation(args); return; } // Si la ventana no está cargada, cambia sin animación.

            // Crea una animación de desvanecimiento para la cuadrícula.
            var fadeOutStoryboard = new Storyboard();
            var fadeOutAnimation = new DoubleAnimation { To = 0, Duration = new Duration(TimeSpan.FromMilliseconds(150)), EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn } };
            Storyboard.SetTarget(fadeOutAnimation, IconGridView);
            Storyboard.SetTargetProperty(fadeOutAnimation, "Opacity");
            fadeOutStoryboard.Children.Add(fadeOutAnimation);

            await fadeOutStoryboard.BeginAsync(); // Espera a que termine el desvanecimiento.

            ApplyTemplateWithoutAnimation(args); // Cambia la plantilla de los ítems.
            SettingsService.SaveGridSizeMode(args); // Guarda la nueva preferencia.

            // Crea una animación para mostrar la nueva vista de la cuadrícula.
            var fadeInStoryboard = new Storyboard();
            var fadeInAnimation = new DoubleAnimation { To = 1, Duration = new Duration(TimeSpan.FromMilliseconds(250)), EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut } };
            Storyboard.SetTarget(fadeInAnimation, IconGridView);
            Storyboard.SetTargetProperty(fadeInAnimation, "Opacity");
            fadeInStoryboard.Children.Add(fadeInAnimation);
            fadeInStoryboard.Begin();
        }

        // Cambia la plantilla de los ítems de la cuadrícula para ajustar el tamaño de los iconos.
        private void ApplyTemplateWithoutAnimation(UserControls.GridSizeMode mode)
        {
            string templateKey;
            switch (mode) { case UserControls.GridSizeMode.Large: templateKey = "LargeIconTemplate"; break; case UserControls.GridSizeMode.Small: templateKey = "SmallIconTemplate"; break; case UserControls.GridSizeMode.Medium: default: templateKey = "MediumIconTemplate"; break; }
            if (this.ContentContainer.Resources.TryGetValue(templateKey, out var template)) { IconGridView.ItemTemplate = template as DataTemplate; }
        }

        // Maneja el clic del botón para mostrar/ocultar el panel lateral.
        private void ToggleSidePanelButton_Click(object sender, RoutedEventArgs e)
        {
            var animationHelper = MainContentGrid.Resources["ColumnAnimationHelper"] as GridLengthAnimationHelper;
            if (animationHelper == null) return;
            animationHelper.TargetColumn = SidePanelColumn;

            // Crea una animación para el ancho de la columna del panel lateral.
            var storyboard = new Storyboard();
            var duration = TimeSpan.FromMilliseconds(300);
            var easing = new CubicEase { EasingMode = EasingMode.EaseOut };
            var animation = new DoubleAnimation { Duration = duration, EasingFunction = easing, EnableDependentAnimation = true };
            Storyboard.SetTarget(animation, animationHelper);
            Storyboard.SetTargetProperty(animation, "AnimatedValue");

            // Anima para ocultar o mostrar el panel.
            if (_isSidePanelVisible) { animation.From = SidePanelColumn.ActualWidth; animation.To = 0; } else { animation.From = 0; animation.To = 400; }

            storyboard.Children.Add(animation);
            storyboard.Begin();
            _isSidePanelVisible = !_isSidePanelVisible; // Actualiza el estado de visibilidad.
        }

        #endregion

        #region Lógica de Animación de Selección en GridView

        // Se ejecuta cuando cambia la selección en la cuadrícula de iconos.
        private void IconGridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Anima el borde del ítem deseleccionado para que desaparezca.
            if (e.RemovedItems.Any()) { var unselectedItem = e.RemovedItems.FirstOrDefault(); if (IconGridView.ContainerFromItem(unselectedItem) is GridViewItem unselectedContainer) { AnimateSelectionBorder(unselectedContainer, 0); } }
            // Anima el borde del ítem seleccionado para que aparezca.
            if (e.AddedItems.Any()) { var selectedItem = e.AddedItems.FirstOrDefault(); if (IconGridView.ContainerFromItem(selectedItem) is GridViewItem selectedContainer) { AnimateSelectionBorder(selectedContainer, 1); } }
            // Actualiza el panel lateral con la información del nuevo ítem seleccionado.
            SidePanel.SelectedItem = IconGridView.SelectedItem as IconInfo;
        }

        // Anima la opacidad del borde de selección de un ítem de la cuadrícula.
        private void AnimateSelectionBorder(Control container, double toOpacity)
        {
            if (FindVisualChildByName<Border>(container, "SelectionIndicator") is Border selectionBorder)
            {
                var storyboard = new Storyboard();
                var animation = new DoubleAnimation { To = toOpacity, Duration = TimeSpan.FromMilliseconds(200), EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };
                Storyboard.SetTarget(animation, selectionBorder);
                Storyboard.SetTargetProperty(animation, "Opacity");
                storyboard.Children.Add(animation);
                storyboard.Begin();
            }
        }

        // Método de utilidad para encontrar un control hijo dentro del árbol visual por su nombre.
        public static T? FindVisualChildByName<T>(DependencyObject? parent, string childName) where T : FrameworkElement
        {
            if (parent == null) return null;
            T? foundChild = null;
            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is not T childType) { foundChild = FindVisualChildByName<T>(child, childName); if (foundChild != null) break; }
                else if (!string.IsNullOrEmpty(childName)) { if (childType.Name == childName) { foundChild = childType; break; } else { foundChild = FindVisualChildByName<T>(child, childName); if (foundChild != null) break; } }
            }
            return foundChild;
        }

        #endregion
    }
}