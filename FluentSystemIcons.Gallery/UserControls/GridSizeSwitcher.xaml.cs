// FluentSystemIcons/FluentSystemIcons.Gallery/UserControls/GridSizeSwitcher.xaml.cs
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using Windows.Foundation;

// Define el espacio de nombres para los controles de usuario personalizados de la galer�a.
namespace FluentSystemIcons.Gallery.UserControls
{
    // Enumeraci�n que define los posibles tama�os para la vista de cuadr�cula de iconos.
    public enum GridSizeMode
    {
        Large,  // Tama�o grande.
        Medium, // Tama�o mediano.
        Small   // Tama�o peque�o.
    }

    // Define la clase para el control de usuario GridSizeSwitcher. Es 'sealed' para no poder ser heredada,
    // y 'partial' porque est� vinculada a un archivo XAML.
    public sealed partial class GridSizeSwitcher : UserControl
    {
        // Evento que se dispara cuando el usuario selecciona un nuevo tama�o de cuadr�cula.
        // Permite que la ventana principal u otros componentes reaccionen al cambio.
        public event TypedEventHandler<GridSizeSwitcher, GridSizeMode>? SelectionChanged;

        // Declaraci�n de una DependencyProperty. Esto permite que la propiedad CurrentSizeMode
        // se pueda enlazar (bind) en XAML y soporte caracter�sticas avanzadas como animaciones y estilos.
        public static readonly DependencyProperty CurrentSizeModeProperty =
            DependencyProperty.Register(nameof(CurrentSizeMode), typeof(GridSizeMode), typeof(GridSizeSwitcher),
                new PropertyMetadata(GridSizeMode.Medium, OnCurrentSizeModeChanged));

        // Propiedad p�blica que obtiene o establece el modo de tama�o actual (Large, Medium, o Small).
        // Act�a como un contenedor para la DependencyProperty, facilitando su uso desde el c�digo C#.
        public GridSizeMode CurrentSizeMode
        {
            get => (GridSizeMode)GetValue(CurrentSizeModeProperty);
            set => SetValue(CurrentSizeModeProperty, value);
        }

        // Bandera para controlar si el dise�o inicial del control ya se ha aplicado.
        // Evita que la animaci�n de posicionamiento se ejecute antes de que el control sea visible y tenga un tama�o.
        private bool _isInitialLayoutApplied = false;

        // Constructor del control de usuario.
        public GridSizeSwitcher()
        {
            // Inicializa los componentes definidos en el archivo XAML asociado.
            this.InitializeComponent();

            // =========================================================================================
            //     CORRECCI�N 2: Reemplazamos el evento 'Loaded' por 'SizeChanged'
            // =========================================================================================
            // Se suscribe al evento SizeChanged para detectar cu�ndo el control obtiene un tama�o real en la UI.
            // Esto es m�s fiable que 'Loaded' para realizar c�lculos de posicionamiento.
            this.SizeChanged += OnGridSizeSwitcherSizeChanged;
        }

        // =========================================================================================
        //     CORRECCI�N 3: Nuevo m�todo que se ejecuta cuando el control obtiene un tama�o real
        // =========================================================================================
        // Manejador del evento SizeChanged. Se ejecuta cuando el tama�o del control cambia.
        private void OnGridSizeSwitcherSizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Nos aseguramos de que esto solo se ejecute la primera vez que el control
            // tiene un ancho v�lido, para posicionar el indicador correctamente al inicio.
            if (!_isInitialLayoutApplied && e.NewSize.Width > 0)
            {
                // Posiciona el indicador en su lugar inicial sin animaci�n.
                UpdateSelectionIndicator(false);
                // Marca que el dise�o inicial ya se ha completado para no repetir esta acci�n.
                _isInitialLayoutApplied = true;
            }
        }

        // M�todo de devoluci�n de llamada est�tico que se ejecuta cuando el valor de CurrentSizeMode cambia.
        private static void OnCurrentSizeModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Se asegura de que el objeto es una instancia de GridSizeSwitcher.
            if (d is GridSizeSwitcher control)
            {
                // La animaci�n solo debe ejecutarse si el control ya est� visible y cargado
                // (es decir, despu�s de que el posicionamiento inicial sin animaci�n ya haya ocurrido).
                if (control._isInitialLayoutApplied)
                {
                    // Llama al m�todo para actualizar la posici�n del indicador, esta vez con animaci�n.
                    control.UpdateSelectionIndicator(true);
                }
                // Dispara el evento SelectionChanged para notificar a los suscriptores sobre el nuevo modo seleccionado.
                control.SelectionChanged?.Invoke(control, (GridSizeMode)e.NewValue);
            }
        }

        // Manejador del evento de clic para los botones de cambio de tama�o.
        private void OnSizeButtonClicked(object sender, RoutedEventArgs e)
        {
            // Se asegura de que el emisor del evento sea un bot�n y que su propiedad Tag contenga un string.
            if (sender is not Button clickedButton || clickedButton.Tag is not string sizeName)
                return;

            // Intenta convertir el string del Tag (ej. "Large") a un valor de la enumeraci�n GridSizeMode.
            if (Enum.TryParse<GridSizeMode>(sizeName, out GridSizeMode newMode))
            {
                // Si la conversi�n es exitosa, actualiza la propiedad CurrentSizeMode.
                // Este cambio activar� autom�ticamente el m�todo OnCurrentSizeModeChanged.
                CurrentSizeMode = newMode;
            }
        }

        // Actualiza la posici�n del elemento visual que indica la selecci�n actual.
        private void UpdateSelectionIndicator(bool animate)
        {
            // Esta protecci�n sigue siendo �til en caso de que el m�todo sea llamado
            // antes de que el layout est� listo y el control no tenga un ancho real.
            if (GridSizeSwitcherGrid.ActualWidth == 0) return;

            // Obtiene el �ndice num�rico del modo actual (0 para Large, 1 para Medium, 2 para Small).
            int selectedIndex = (int)this.CurrentSizeMode;
            // Calcula el ancho disponible para los botones, descontando el padding del contenedor.
            double availableWidth = GridSizeSwitcherGrid.ActualWidth - GridSizeSwitcherGrid.Padding.Left - GridSizeSwitcherGrid.Padding.Right;
            // Calcula el ancho de cada una de las 3 "pesta�as" o botones.
            double tabWidth = availableWidth / 3;
            // Calcula la coordenada X de destino para el indicador de fondo.
            double targetBackgroundX = tabWidth * selectedIndex;

            // Decide si el movimiento del indicador debe ser instant�neo o animado.
            if (animate)
            {
                // Crea un Storyboard para gestionar la animaci�n.
                var storyboard = new Storyboard();
                var duration = new Duration(TimeSpan.FromMilliseconds(300));
                var easing = new CubicEase { EasingMode = EasingMode.EaseOut };

                // Define una animaci�n de tipo Double que cambiar� la propiedad X de la transformaci�n de traslaci�n.
                var backgroundAnimation = new DoubleAnimation
                {
                    To = targetBackgroundX, // Valor final de la coordenada X.
                    Duration = duration,    // Duraci�n de la animaci�n.
                    EasingFunction = easing // Efecto de suavizado para la animaci�n.
                };
                // Asocia la animaci�n con la transformaci�n del indicador (BackgroundTranslateTransform).
                Storyboard.SetTarget(backgroundAnimation, BackgroundTranslateTransform);
                // Especifica que la propiedad a animar es "X".
                Storyboard.SetTargetProperty(backgroundAnimation, "X");
                storyboard.Children.Add(backgroundAnimation);

                // Inicia la animaci�n.
                storyboard.Begin();
            }
            else
            {
                // Si no se debe animar, establece la posici�n X de la transformaci�n directamente.
                BackgroundTranslateTransform.X = targetBackgroundX;
            }
        }
    }
}