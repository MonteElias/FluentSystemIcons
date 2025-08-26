// FluentSystemIcons/FluentSystemIcons.Gallery/UserControls/CodeViewSwitcher.xaml.cs
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using Windows.Foundation;

// Define el espacio de nombres para los controles de usuario personalizados de la galería.
namespace FluentSystemIcons.Gallery.UserControls
{
    // Enumeración para definir los posibles modos de vista del código.
    public enum CodeViewMode
    {
        Xaml,   // Representa la vista de código XAML.
        CSharp  // Representa la vista de código C#.
    }

    // Define la clase para el control de usuario CodeViewSwitcher. Es 'sealed' para no poder ser heredada,
    // y 'partial' porque está vinculada a un archivo XAML.
    public sealed partial class CodeViewSwitcher : UserControl
    {
        // Evento que se dispara cuando cambia la selección entre XAML y C#.
        // Permite que otras partes de la aplicación reaccionen al cambio.
        public event TypedEventHandler<CodeViewSwitcher, CodeViewMode>? SelectionChanged;

        // Declaración de una DependencyProperty. Esto permite que la propiedad CurrentViewMode
        // se pueda enlazar (bind) en XAML y soporte características como animaciones y estilos.
        public static readonly DependencyProperty CurrentViewModeProperty =
            DependencyProperty.Register(nameof(CurrentViewMode), typeof(CodeViewMode), typeof(CodeViewSwitcher),
                new PropertyMetadata(CodeViewMode.Xaml, OnCurrentViewModeChanged));

        // Propiedad pública que obtiene o establece el modo de vista actual (Xaml o CSharp).
        // Utiliza GetValue y SetValue para interactuar con la DependencyProperty subyacente.
        public CodeViewMode CurrentViewMode
        {
            get => (CodeViewMode)GetValue(CurrentViewModeProperty);
            set => SetValue(CurrentViewModeProperty, value);
        }

        // Bandera para controlar si el diseño inicial del control ya se ha aplicado.
        // Se usa para evitar ejecutar la animación de posicionamiento antes de que el control sea visible y tenga un tamaño.
        private bool _isInitialLayoutApplied = false;

        // Constructor del control de usuario.
        public CodeViewSwitcher()
        {
            // Inicializa los componentes definidos en el archivo XAML asociado.
            this.InitializeComponent();
            // Suscribe un manejador al evento SizeChanged para saber cuándo el control obtiene su tamaño final.
            this.SizeChanged += OnSwitcherSizeChanged;
        }

        // Manejador del evento SizeChanged. Se ejecuta cuando el tamaño del control cambia.
        private void OnSwitcherSizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Comprueba si el diseño inicial aún no se ha aplicado y si el control ya tiene un ancho.
            if (!_isInitialLayoutApplied && e.NewSize.Width > 0)
            {
                // Posiciona el indicador de selección en su lugar inicial sin animación.
                UpdateSelectionIndicator(false);
                // Marca que el diseño inicial ya se ha completado.
                _isInitialLayoutApplied = true;
            }
        }

        // Método de devolución de llamada estático que se ejecuta cuando el valor de la DependencyProperty CurrentViewMode cambia.
        private static void OnCurrentViewModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Se asegura de que el objeto es una instancia de CodeViewSwitcher.
            if (d is CodeViewSwitcher control)
            {
                // Solo anima el indicador si el control ya ha sido dibujado en la pantalla.
                if (control._isInitialLayoutApplied)
                {
                    // Llama al método para actualizar la posición del indicador, esta vez con animación.
                    control.UpdateSelectionIndicator(true);
                }
                // Dispara el evento SelectionChanged para notificar a los suscriptores sobre el nuevo modo seleccionado.
                control.SelectionChanged?.Invoke(control, (CodeViewMode)e.NewValue);
            }
        }

        // Manejador del evento de clic para los botones XAML y C#.
        private void OnViewButtonClicked(object sender, RoutedEventArgs e)
        {
            // Comprueba si el emisor del evento es un botón y si su propiedad Tag contiene un nombre de vista válido.
            if (sender is Button { Tag: string viewName } && Enum.TryParse<CodeViewMode>(viewName, out var newMode))
            {
                // Si el Tag se puede convertir a un valor de CodeViewMode, actualiza la propiedad CurrentViewMode.
                // Esto a su vez disparará el método OnCurrentViewModeChanged.
                CurrentViewMode = newMode;
            }
        }

        // Actualiza la posición del elemento visual que indica la selección actual (el rectángulo de color).
        private void UpdateSelectionIndicator(bool animate)
        {
            // Si el control aún no tiene un ancho real, no hace nada para evitar cálculos incorrectos.
            if (SwitcherGrid.ActualWidth == 0) return;

            // Obtiene el índice del modo actual (0 para Xaml, 1 para CSharp).
            int selectedIndex = (int)this.CurrentViewMode;
            // Calcula el ancho disponible para las pestañas, restando el padding del contenedor.
            double availableWidth = SwitcherGrid.ActualWidth - SwitcherGrid.Padding.Left - SwitcherGrid.Padding.Right;
            // Calcula el ancho de una sola pestaña (hay 2).
            double tabWidth = availableWidth / 2;
            // Calcula la coordenada X de destino para el indicador.
            double targetX = tabWidth * selectedIndex;

            // Decide si mover el indicador instantáneamente o con una animación.
            if (animate)
            {
                // Crea un Storyboard para orquestar la animación.
                var storyboard = new Storyboard();
                // Define una animación de tipo Double que cambiará la propiedad X de la transformación.
                var animation = new DoubleAnimation
                {
                    To = targetX, // Valor final de la animación.
                    Duration = TimeSpan.FromMilliseconds(300), // Duración de la animación.
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } // Efecto de aceleración/desaceleración suave.
                };
                // Asocia la animación con la transformación de renderizado del indicador (IndicatorTransform).
                Storyboard.SetTarget(animation, IndicatorTransform);
                // Especifica que la propiedad a animar es "X".
                Storyboard.SetTargetProperty(animation, "X");
                // Añade la animación al Storyboard y la inicia.
                storyboard.Children.Add(animation);
                storyboard.Begin();
            }
            else
            {
                // Si no se debe animar, mueve el indicador a su posición de destino directamente.
                IndicatorTransform.X = targetX;
            }
        }
    }
}