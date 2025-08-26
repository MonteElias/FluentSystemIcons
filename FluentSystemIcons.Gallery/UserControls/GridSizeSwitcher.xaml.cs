// FluentSystemIcons/FluentSystemIcons.Gallery/UserControls/GridSizeSwitcher.xaml.cs
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using Windows.Foundation;

// Define el espacio de nombres para los controles de usuario personalizados de la galería.
namespace FluentSystemIcons.Gallery.UserControls
{
    // Enumeración que define los posibles tamaños para la vista de cuadrícula de iconos.
    public enum GridSizeMode
    {
        Large,  // Tamaño grande.
        Medium, // Tamaño mediano.
        Small   // Tamaño pequeño.
    }

    // Define la clase para el control de usuario GridSizeSwitcher. Es 'sealed' para no poder ser heredada,
    // y 'partial' porque está vinculada a un archivo XAML.
    public sealed partial class GridSizeSwitcher : UserControl
    {
        // Evento que se dispara cuando el usuario selecciona un nuevo tamaño de cuadrícula.
        // Permite que la ventana principal u otros componentes reaccionen al cambio.
        public event TypedEventHandler<GridSizeSwitcher, GridSizeMode>? SelectionChanged;

        // Declaración de una DependencyProperty. Esto permite que la propiedad CurrentSizeMode
        // se pueda enlazar (bind) en XAML y soporte características avanzadas como animaciones y estilos.
        public static readonly DependencyProperty CurrentSizeModeProperty =
            DependencyProperty.Register(nameof(CurrentSizeMode), typeof(GridSizeMode), typeof(GridSizeSwitcher),
                new PropertyMetadata(GridSizeMode.Medium, OnCurrentSizeModeChanged));

        // Propiedad pública que obtiene o establece el modo de tamaño actual (Large, Medium, o Small).
        // Actúa como un contenedor para la DependencyProperty, facilitando su uso desde el código C#.
        public GridSizeMode CurrentSizeMode
        {
            get => (GridSizeMode)GetValue(CurrentSizeModeProperty);
            set => SetValue(CurrentSizeModeProperty, value);
        }

        // Bandera para controlar si el diseño inicial del control ya se ha aplicado.
        // Evita que la animación de posicionamiento se ejecute antes de que el control sea visible y tenga un tamaño.
        private bool _isInitialLayoutApplied = false;

        // Constructor del control de usuario.
        public GridSizeSwitcher()
        {
            // Inicializa los componentes definidos en el archivo XAML asociado.
            this.InitializeComponent();

            // =========================================================================================
            //     CORRECCIÓN 2: Reemplazamos el evento 'Loaded' por 'SizeChanged'
            // =========================================================================================
            // Se suscribe al evento SizeChanged para detectar cuándo el control obtiene un tamaño real en la UI.
            // Esto es más fiable que 'Loaded' para realizar cálculos de posicionamiento.
            this.SizeChanged += OnGridSizeSwitcherSizeChanged;
        }

        // =========================================================================================
        //     CORRECCIÓN 3: Nuevo método que se ejecuta cuando el control obtiene un tamaño real
        // =========================================================================================
        // Manejador del evento SizeChanged. Se ejecuta cuando el tamaño del control cambia.
        private void OnGridSizeSwitcherSizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Nos aseguramos de que esto solo se ejecute la primera vez que el control
            // tiene un ancho válido, para posicionar el indicador correctamente al inicio.
            if (!_isInitialLayoutApplied && e.NewSize.Width > 0)
            {
                // Posiciona el indicador en su lugar inicial sin animación.
                UpdateSelectionIndicator(false);
                // Marca que el diseño inicial ya se ha completado para no repetir esta acción.
                _isInitialLayoutApplied = true;
            }
        }

        // Método de devolución de llamada estático que se ejecuta cuando el valor de CurrentSizeMode cambia.
        private static void OnCurrentSizeModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Se asegura de que el objeto es una instancia de GridSizeSwitcher.
            if (d is GridSizeSwitcher control)
            {
                // La animación solo debe ejecutarse si el control ya está visible y cargado
                // (es decir, después de que el posicionamiento inicial sin animación ya haya ocurrido).
                if (control._isInitialLayoutApplied)
                {
                    // Llama al método para actualizar la posición del indicador, esta vez con animación.
                    control.UpdateSelectionIndicator(true);
                }
                // Dispara el evento SelectionChanged para notificar a los suscriptores sobre el nuevo modo seleccionado.
                control.SelectionChanged?.Invoke(control, (GridSizeMode)e.NewValue);
            }
        }

        // Manejador del evento de clic para los botones de cambio de tamaño.
        private void OnSizeButtonClicked(object sender, RoutedEventArgs e)
        {
            // Se asegura de que el emisor del evento sea un botón y que su propiedad Tag contenga un string.
            if (sender is not Button clickedButton || clickedButton.Tag is not string sizeName)
                return;

            // Intenta convertir el string del Tag (ej. "Large") a un valor de la enumeración GridSizeMode.
            if (Enum.TryParse<GridSizeMode>(sizeName, out GridSizeMode newMode))
            {
                // Si la conversión es exitosa, actualiza la propiedad CurrentSizeMode.
                // Este cambio activará automáticamente el método OnCurrentSizeModeChanged.
                CurrentSizeMode = newMode;
            }
        }

        // Actualiza la posición del elemento visual que indica la selección actual.
        private void UpdateSelectionIndicator(bool animate)
        {
            // Esta protección sigue siendo útil en caso de que el método sea llamado
            // antes de que el layout esté listo y el control no tenga un ancho real.
            if (GridSizeSwitcherGrid.ActualWidth == 0) return;

            // Obtiene el índice numérico del modo actual (0 para Large, 1 para Medium, 2 para Small).
            int selectedIndex = (int)this.CurrentSizeMode;
            // Calcula el ancho disponible para los botones, descontando el padding del contenedor.
            double availableWidth = GridSizeSwitcherGrid.ActualWidth - GridSizeSwitcherGrid.Padding.Left - GridSizeSwitcherGrid.Padding.Right;
            // Calcula el ancho de cada una de las 3 "pestañas" o botones.
            double tabWidth = availableWidth / 3;
            // Calcula la coordenada X de destino para el indicador de fondo.
            double targetBackgroundX = tabWidth * selectedIndex;

            // Decide si el movimiento del indicador debe ser instantáneo o animado.
            if (animate)
            {
                // Crea un Storyboard para gestionar la animación.
                var storyboard = new Storyboard();
                var duration = new Duration(TimeSpan.FromMilliseconds(300));
                var easing = new CubicEase { EasingMode = EasingMode.EaseOut };

                // Define una animación de tipo Double que cambiará la propiedad X de la transformación de traslación.
                var backgroundAnimation = new DoubleAnimation
                {
                    To = targetBackgroundX, // Valor final de la coordenada X.
                    Duration = duration,    // Duración de la animación.
                    EasingFunction = easing // Efecto de suavizado para la animación.
                };
                // Asocia la animación con la transformación del indicador (BackgroundTranslateTransform).
                Storyboard.SetTarget(backgroundAnimation, BackgroundTranslateTransform);
                // Especifica que la propiedad a animar es "X".
                Storyboard.SetTargetProperty(backgroundAnimation, "X");
                storyboard.Children.Add(backgroundAnimation);

                // Inicia la animación.
                storyboard.Begin();
            }
            else
            {
                // Si no se debe animar, establece la posición X de la transformación directamente.
                BackgroundTranslateTransform.X = targetBackgroundX;
            }
        }
    }
}