// FluentSystemIcons/FluentSystemIcons.Gallery/UserControls/SidePanelControl.xaml.cs
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using Windows.ApplicationModel.DataTransfer;
using Microsoft.Windows.ApplicationModel.Resources;
using FluentSystemIcons.Gallery.Models;

// Define el espacio de nombres para los controles de usuario personalizados de la galería.
namespace FluentSystemIcons.Gallery.UserControls
{
    // Define la clase para el panel lateral que muestra los detalles del icono seleccionado.
    // Es 'sealed' para no poder ser heredada y 'partial' por su vínculo con un archivo XAML.
    public sealed partial class SidePanelControl : UserControl
    {
        // Define una DependencyProperty para el icono seleccionado. Esto permite enlazar un objeto IconInfo
        // desde la ventana principal a este panel. Cuando el valor cambie, se llamará a OnSelectedItemChanged.
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(nameof(SelectedItem), typeof(IconInfo), typeof(SidePanelControl), new PropertyMetadata(null, OnSelectedItemChanged));

        // Propiedad pública que envuelve la DependencyProperty para un acceso más fácil desde el código.
        // Representa el icono actualmente seleccionado cuyos detalles se muestran en el panel.
        public IconInfo? SelectedItem
        {
            get => GetValue(SelectedItemProperty) as IconInfo;
            set => SetValue(SelectedItemProperty, value);
        }

        // Temporizador para controlar la duración del mensaje de confirmación "Copiado".
        private readonly DispatcherTimer _copyFeedbackTimer;
        // Almacena una referencia al ToolTip que está mostrando activamente el mensaje "Copiado".
        private ToolTip? _activeToolTip;
        // Almacena una tupla con los iconos de "copiar" y "confirmación" del botón presionado.
        private (FontIcon CopyIcon, FontIcon CheckIcon)? _activeIcons;

        // Constructor del control SidePanelControl.
        public SidePanelControl()
        {
            // Carga y construye el árbol visual del control desde su archivo XAML.
            this.InitializeComponent();
            // Llama a UpdateUI para establecer el estado inicial (panel vacío).
            UpdateUI();
            // Establece la vista de código inicial en XAML.
            UpdateCodeViewUI(CodeViewMode.Xaml);

            // Inicializa el temporizador para la retroalimentación de copiado.
            _copyFeedbackTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1.5) };
            _copyFeedbackTimer.Tick += OnCopyFeedbackTimerTick;
            // Se asegura de que el temporizador se detenga cuando el control se descargue para evitar fugas de memoria.
            this.Unloaded += (s, e) => _copyFeedbackTimer.Stop();
        }

        // Método estático que se ejecuta cada vez que cambia el valor de la propiedad SelectedItem.
        private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Si el objeto es una instancia de SidePanelControl, llama a su método para actualizar la UI.
            if (d is SidePanelControl control)
            {
                control.UpdateUI();
            }
        }

        // Actualiza toda la interfaz de usuario del panel con la información del icono seleccionado.
        private void UpdateUI()
        {
            // Si hay un icono seleccionado, muestra sus detalles.
            if (SelectedItem != null)
            {
                // Configura la vista previa del icono grande.
                IconDisplayTextBlock.FontFamily = SelectedItem.FontFamily;
                IconDisplayTextBlock.Glyph = SelectedItem.Glyph;
                IconNameValueTextBlock.Text = SelectedItem.DisplayName;

                // Rellena los campos de código XAML.
                GlyphCodeValueTextBlock.Text = SelectedItem.XamlCode;
                FontIconValueTextBlock.Text = $"<FontIcon FontFamily=\"{{StaticResource {SelectedItem.FontKey}}}\" Glyph=\"{SelectedItem.XamlCode}\" />";
                SymbolIconValueTextBlock.Text = $"<SymbolIcon Symbol=\"{SelectedItem.Name}\" />";

                // Si el glifo no está vacío, calcula y rellena los campos de código C#.
                if (!string.IsNullOrEmpty(SelectedItem.Glyph))
                {
                    int codepoint = SelectedItem.Glyph[0];
                    string hexCode = codepoint.ToString("X4");
                    string literalGlyphCode = $"\\u{hexCode}";

                    CSharpGlyphCodeValueTextBlock.Text = literalGlyphCode;
                    CSharpFontIconValueTextBlock.Text = $"var fontIcon = new FontIcon {{ Glyph = \"{literalGlyphCode}\" }};";
                    CSharpSymbolIconValueTextBlock.Text = $"var symbolIcon = new SymbolIcon {{ Symbol = Symbol.{SelectedItem.Name} }};";
                }
                else
                {
                    // Si no hay glifo, vacía los campos de C#.
                    CSharpGlyphCodeValueTextBlock.Text = string.Empty;
                    CSharpFontIconValueTextBlock.Text = string.Empty;
                    CSharpSymbolIconValueTextBlock.Text = string.Empty;
                }

                // Oculta el mensaje inicial y muestra la cuadrícula de detalles con una animación.
                InitialMessageTextBlock.Visibility = Visibility.Collapsed;
                AnimateOpacity(DetailsGrid, 1.0);
            }
            else
            {
                // Si no hay ningún icono seleccionado, muestra el mensaje inicial y oculta los detalles.
                InitialMessageTextBlock.Visibility = Visibility.Visible;
                AnimateOpacity(DetailsGrid, 0.0);
            }
        }

        // Centraliza la lógica para mostrar u ocultar los bloques de código XAML o C# según la selección.
        private void UpdateCodeViewUI(CodeViewMode mode)
        {
            // Determina si la vista actual es XAML.
            bool isXaml = (mode == CodeViewMode.Xaml);
            // Establece la visibilidad para los controles XAML y C# en función del modo.
            var xamlVisibility = isXaml ? Visibility.Visible : Visibility.Collapsed;
            var csharpVisibility = isXaml ? Visibility.Collapsed : Visibility.Visible;

            // Actualiza los títulos de las secciones de código para reflejar el idioma actual.
            var resourceManager = new ResourceManager();
            GlyphCodeTitleTextBlock.Text = isXaml ? resourceManager.MainResourceMap.GetValue("Resources/SidePanel_GlyphTextHeader_Text").ValueAsString : resourceManager.MainResourceMap.GetValue("Resources/SidePanel_GlyphCodeHeader_Text").ValueAsString;
            FontIconTitleTextBlock.Text = isXaml ? resourceManager.MainResourceMap.GetValue("Resources/SidePanel_FontIconXamlHeader_Text").ValueAsString : resourceManager.MainResourceMap.GetValue("Resources/SidePanel_FontIconCSharpHeader_Text").ValueAsString;
            SymbolIconTitleTextBlock.Text = isXaml ? resourceManager.MainResourceMap.GetValue("Resources/SidePanel_SymbolIconXamlHeader_Text").ValueAsString : resourceManager.MainResourceMap.GetValue("Resources/SidePanel_SymbolIconCSharpHeader_Text").ValueAsString;

            // Alterna la visibilidad de los bloques de texto y botones de copia para el glifo.
            GlyphCodeValueTextBlock.Visibility = xamlVisibility;
            CopyGlyphButton.Visibility = xamlVisibility;
            CSharpGlyphCodeValueTextBlock.Visibility = csharpVisibility;
            CopyCSharpGlyphButton.Visibility = csharpVisibility;

            // Alterna la visibilidad para FontIcon.
            FontIconValueTextBlock.Visibility = xamlVisibility;
            CopyFontIconButton.Visibility = xamlVisibility;
            CSharpFontIconValueTextBlock.Visibility = csharpVisibility;
            CopyCSharpFontIconButton.Visibility = csharpVisibility;

            // Alterna la visibilidad para SymbolIcon.
            SymbolIconValueTextBlock.Visibility = xamlVisibility;
            CopySymbolIconButton.Visibility = xamlVisibility;
            CSharpSymbolIconValueTextBlock.Visibility = csharpVisibility;
            CopyCSharpSymbolIconButton.Visibility = csharpVisibility;
        }

        // Manejador del evento del conmutador de código (CodeViewSwitcher).
        private void CodeSwitcher_SelectionChanged(CodeViewSwitcher sender, CodeViewMode args)
        {
            // Llama al método de actualización de la UI con el nuevo modo seleccionado.
            UpdateCodeViewUI(args);
        }

        #region Botones de Copiado y Animación

        // Método central para copiar texto al portapapeles y mostrar una confirmación visual.
        private void CopyToClipboardAndShowConfirmation(string text, ToolTip? toolTip, Button button)
        {
            // Realiza validaciones para evitar errores.
            if (string.IsNullOrEmpty(text) || toolTip == null || button == null) return;
            // Si una animación de copiado ya está en curso, la finaliza antes de empezar una nueva.
            if (_copyFeedbackTimer.IsEnabled) OnCopyFeedbackTimerTick(null, null);

            // Crea un paquete de datos y establece su contenido de texto.
            var dataPackage = new DataPackage();
            dataPackage.SetText(text);
            // Coloca el paquete de datos en el portapapeles del sistema.
            Clipboard.SetContent(dataPackage);

            // Encuentra los iconos de "copiar" y "confirmación" dentro del botón presionado usando sus Tags.
            var copyIcon = FindVisualChildByTag<FontIcon>(button, "Copy");
            var checkIcon = FindVisualChildByTag<FontIcon>(button, "Check");

            // Si ambos iconos se encuentran, procede con la animación y la retroalimentación.
            if (copyIcon != null && checkIcon != null)
            {
                // Guarda referencias al tooltip y los iconos activos.
                _activeIcons = (copyIcon, checkIcon);
                _activeToolTip = toolTip;
                // Inicia la animación de fundido cruzado entre los dos iconos.
                AnimateCopyIcon(copyIcon, checkIcon);

                // Cambia el texto del tooltip a "Copiado", lo muestra e inicia el temporizador.
                var resourceManager = new ResourceManager();
                _activeToolTip.Content = resourceManager.MainResourceMap.GetValue("Resources/SidePanel_Copied_Text").ValueAsString;
                _activeToolTip.IsOpen = true;
                _copyFeedbackTimer.Start();
            }
        }

        // Realiza una animación de fundido cruzado entre el icono de "copiar" y el de "confirmación".
        private void AnimateCopyIcon(UIElement copyIcon, UIElement checkIcon, bool reverse = false)
        {
            var storyboard = new Storyboard();
            var duration = TimeSpan.FromMilliseconds(150);
            var easing = new QuadraticEase { EasingMode = EasingMode.EaseOut };

            // Animación para desvanecer el icono de copia (o aparecer si es en reversa).
            var fadeOut = new DoubleAnimation { To = reverse ? 1.0 : 0.0, Duration = duration, EasingFunction = easing };
            Storyboard.SetTarget(fadeOut, copyIcon);
            Storyboard.SetTargetProperty(fadeOut, "Opacity");

            // Animación para mostrar el icono de confirmación (o desaparecer si es en reversa).
            var fadeIn = new DoubleAnimation { To = reverse ? 0.0 : 1.0, Duration = duration, EasingFunction = easing };
            Storyboard.SetTarget(fadeIn, checkIcon);
            Storyboard.SetTargetProperty(fadeIn, "Opacity");

            storyboard.Children.Add(fadeOut);
            storyboard.Children.Add(fadeIn);
            storyboard.Begin();
        }

        // Se ejecuta cuando el temporizador de retroalimentación de copiado finaliza.
        private void OnCopyFeedbackTimerTick(object? sender, object? e)
        {
            _copyFeedbackTimer.Stop();
            // Si hay iconos activos, revierte la animación para volver a mostrar el icono de "copiar".
            if (_activeIcons.HasValue)
            {
                AnimateCopyIcon(_activeIcons.Value.CopyIcon, _activeIcons.Value.CheckIcon, reverse: true);
                _activeIcons = null;
            }
            // Si hay un tooltip activo, lo oculta.
            if (_activeToolTip != null)
            {
                _activeToolTip.IsOpen = false;
                _activeToolTip = null;
            }
        }

        // Restaura el texto original del tooltip ("Copiar...") cuando el puntero sale del botón.
        private void CopyButton_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (sender is not Button button) return;

            var toolTip = ToolTipService.GetToolTip(button) as ToolTip;
            // Solo actúa si el tooltip existe y no es el que está mostrando el mensaje "Copiado".
            if (toolTip is null || _activeToolTip == toolTip) return;

            var resourceManager = new ResourceManager();
            string resourceKey = string.Empty;

            // Determina qué recurso de texto restaurar basándose en el nombre del botón.
            switch (button.Name)
            {
                case "CopyNameButton": resourceKey = "CopyNameButton.ToolTipService.ToolTip"; break;
                case "CopyGlyphButton": resourceKey = "CopyGlyphButton.ToolTipService.ToolTip"; break;
                case "CopyCSharpGlyphButton": resourceKey = "CopyCSharpGlyphButton.ToolTipService.ToolTip"; break;
                case "CopyFontIconButton": resourceKey = "CopyFontIconButton.ToolTipService.ToolTip"; break;
                case "CopyCSharpFontIconButton": resourceKey = "CopyCSharpFontIconButton.ToolTipService.ToolTip"; break;
                case "CopySymbolIconButton": resourceKey = "CopySymbolIconButton.ToolTipService.ToolTip"; break;
                case "CopyCSharpSymbolIconButton": resourceKey = "CopyCSharpSymbolIconButton.ToolTipService.ToolTip"; break;
            }

            // Si se encontró una clave de recurso, la busca y actualiza el contenido del tooltip.
            if (!string.IsNullOrEmpty(resourceKey))
            {
                var resource = resourceManager.MainResourceMap.TryGetValue($"Resources/{resourceKey}");
                if (resource != null)
                {
                    toolTip.Content = resource.ValueAsString;
                }
            }
        }

        // Método auxiliar genérico para manejar el evento de clic de cualquier botón de copia.
        private void HandleCopyClick(object sender, Func<IconInfo, string> getTextFunc)
        {
            if (sender is Button button && SelectedItem != null)
            {
                var toolTip = ToolTipService.GetToolTip(button) as ToolTip;
                // Usa la función delegada para obtener el texto específico que se debe copiar.
                var textToCopy = getTextFunc(SelectedItem);
                // Llama al método central de copiado y confirmación.
                CopyToClipboardAndShowConfirmation(textToCopy, toolTip, button);
            }
        }

        // Manejadores de eventos de clic para cada botón de copia.
        // Cada uno llama al método genérico 'HandleCopyClick' con una expresión lambda
        // que especifica qué propiedad o texto formateado del IconInfo se debe copiar.
        private void CopyNameButton_Click(object sender, RoutedEventArgs e) => HandleCopyClick(sender, item => item.DisplayName);
        private void CopyGlyphButton_Click(object sender, RoutedEventArgs e) => HandleCopyClick(sender, item => item.XamlCode);
        private void CopyFontIconButton_Click(object sender, RoutedEventArgs e) => HandleCopyClick(sender, item => $"<FontIcon FontFamily=\"{{StaticResource {item.FontKey}}}\" Glyph=\"{item.XamlCode}\" />");
        private void CopySymbolIconButton_Click(object sender, RoutedEventArgs e) => HandleCopyClick(sender, item => $"<SymbolIcon Symbol=\"{item.Name}\" />");
        private void CopyCSharpGlyphButton_Click(object sender, RoutedEventArgs e) => HandleCopyClick(sender, item => CSharpGlyphCodeValueTextBlock.Text);
        private void CopyCSharpFontIconButton_Click(object sender, RoutedEventArgs e) => HandleCopyClick(sender, item => CSharpFontIconValueTextBlock.Text);
        private void CopyCSharpSymbolIconButton_Click(object sender, RoutedEventArgs e) => HandleCopyClick(sender, item => CSharpSymbolIconValueTextBlock.Text);

        #endregion

        // Método auxiliar para animar la opacidad de un elemento de la UI.
        private void AnimateOpacity(UIElement target, double toValue)
        {
            var storyboard = new Storyboard();
            var animation = new DoubleAnimation { To = toValue, Duration = TimeSpan.FromMilliseconds(250), EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut } };
            Storyboard.SetTarget(animation, target);
            Storyboard.SetTargetProperty(animation, "Opacity");
            storyboard.Children.Add(animation);
            storyboard.Begin();
        }

        // Método auxiliar recursivo para buscar un control hijo en el árbol visual por su propiedad 'Tag'.
        private static T? FindVisualChildByTag<T>(DependencyObject? parent, string tag) where T : FrameworkElement
        {
            if (parent == null) return null;

            // Recorre todos los hijos directos del elemento padre.
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                // Si el hijo es del tipo buscado y su Tag coincide, lo devuelve.
                if (child is T element && element.Tag is string elementTag && elementTag == tag)
                {
                    return element;
                }
                // Si no coincide, realiza una búsqueda recursiva en los descendientes de este hijo.
                var result = FindVisualChildByTag<T>(child, tag);
                if (result != null)
                {
                    return result; // Si se encuentra en la recursión, lo devuelve.
                }
            }
            return null; // Si no se encuentra, devuelve null.
        }
    }
}