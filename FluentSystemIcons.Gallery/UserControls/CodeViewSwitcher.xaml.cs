// FluentSystemIcons/FluentSystemIcons.Gallery/UserControls/CodeViewSwitcher.xaml.cs
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using Windows.Foundation;

// Define el espacio de nombres para los controles de usuario personalizados de la galer�a.
namespace FluentSystemIcons.Gallery.UserControls
{
    // Enumeraci�n para definir los posibles modos de vista del c�digo.
    public enum CodeViewMode
    {
        Xaml,   // Representa la vista de c�digo XAML.
        CSharp  // Representa la vista de c�digo C#.
    }

    // Define la clase para el control de usuario CodeViewSwitcher. Es 'sealed' para no poder ser heredada,
    // y 'partial' porque est� vinculada a un archivo XAML.
    public sealed partial class CodeViewSwitcher : UserControl
    {
        // Evento que se dispara cuando cambia la selecci�n entre XAML y C#.
        // Permite que otras partes de la aplicaci�n reaccionen al cambio.
        public event TypedEventHandler<CodeViewSwitcher, CodeViewMode>? SelectionChanged;

        // Declaraci�n de una DependencyProperty. Esto permite que la propiedad CurrentViewMode
        // se pueda enlazar (bind) en XAML y soporte caracter�sticas como animaciones y estilos.
        public static readonly DependencyProperty CurrentViewModeProperty =
            DependencyProperty.Register(nameof(CurrentViewMode), typeof(CodeViewMode), typeof(CodeViewSwitcher),
                new PropertyMetadata(CodeViewMode.Xaml, OnCurrentViewModeChanged));

        // Propiedad p�blica que obtiene o establece el modo de vista actual (Xaml o CSharp).
        // Utiliza GetValue y SetValue para interactuar con la DependencyProperty subyacente.
        public CodeViewMode CurrentViewMode
        {
            get => (CodeViewMode)GetValue(CurrentViewModeProperty);
            set => SetValue(CurrentViewModeProperty, value);
        }

        // Bandera para controlar si el dise�o inicial del control ya se ha aplicado.
        // Se usa para evitar ejecutar la animaci�n de posicionamiento antes de que el control sea visible y tenga un tama�o.
        private bool _isInitialLayoutApplied = false;

        // Constructor del control de usuario.
        public CodeViewSwitcher()
        {
            // Inicializa los componentes definidos en el archivo XAML asociado.
            this.InitializeComponent();
            // Suscribe un manejador al evento SizeChanged para saber cu�ndo el control obtiene su tama�o final.
            this.SizeChanged += OnSwitcherSizeChanged;
        }

        // Manejador del evento SizeChanged. Se ejecuta cuando el tama�o del control cambia.
        private void OnSwitcherSizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Comprueba si el dise�o inicial a�n no se ha aplicado y si el control ya tiene un ancho.
            if (!_isInitialLayoutApplied && e.NewSize.Width > 0)
            {
                // Posiciona el indicador de selecci�n en su lugar inicial sin animaci�n.
                UpdateSelectionIndicator(false);
                // Marca que el dise�o inicial ya se ha completado.
                _isInitialLayoutApplied = true;
            }
        }

        // M�todo de devoluci�n de llamada est�tico que se ejecuta cuando el valor de la DependencyProperty CurrentViewMode cambia.
        private static void OnCurrentViewModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Se asegura de que el objeto es una instancia de CodeViewSwitcher.
            if (d is CodeViewSwitcher control)
            {
                // Solo anima el indicador si el control ya ha sido dibujado en la pantalla.
                if (control._isInitialLayoutApplied)
                {
                    // Llama al m�todo para actualizar la posici�n del indicador, esta vez con animaci�n.
                    control.UpdateSelectionIndicator(true);
                }
                // Dispara el evento SelectionChanged para notificar a los suscriptores sobre el nuevo modo seleccionado.
                control.SelectionChanged?.Invoke(control, (CodeViewMode)e.NewValue);
            }
        }

        // Manejador del evento de clic para los botones XAML y C#.
        private void OnViewButtonClicked(object sender, RoutedEventArgs e)
        {
            // Comprueba si el emisor del evento es un bot�n y si su propiedad Tag contiene un nombre de vista v�lido.
            if (sender is Button { Tag: string viewName } && Enum.TryParse<CodeViewMode>(viewName, out var newMode))
            {
                // Si el Tag se puede convertir a un valor de CodeViewMode, actualiza la propiedad CurrentViewMode.
                // Esto a su vez disparar� el m�todo OnCurrentViewModeChanged.
                CurrentViewMode = newMode;
            }
        }

        // Actualiza la posici�n del elemento visual que indica la selecci�n actual (el rect�ngulo de color).
        private void UpdateSelectionIndicator(bool animate)
        {
            // Si el control a�n no tiene un ancho real, no hace nada para evitar c�lculos incorrectos.
            if (SwitcherGrid.ActualWidth == 0) return;

            // Obtiene el �ndice del modo actual (0 para Xaml, 1 para CSharp).
            int selectedIndex = (int)this.CurrentViewMode;
            // Calcula el ancho disponible para las pesta�as, restando el padding del contenedor.
            double availableWidth = SwitcherGrid.ActualWidth - SwitcherGrid.Padding.Left - SwitcherGrid.Padding.Right;
            // Calcula el ancho de una sola pesta�a (hay 2).
            double tabWidth = availableWidth / 2;
            // Calcula la coordenada X de destino para el indicador.
            double targetX = tabWidth * selectedIndex;

            // Decide si mover el indicador instant�neamente o con una animaci�n.
            if (animate)
            {
                // Crea un Storyboard para orquestar la animaci�n.
                var storyboard = new Storyboard();
                // Define una animaci�n de tipo Double que cambiar� la propiedad X de la transformaci�n.
                var animation = new DoubleAnimation
                {
                    To = targetX, // Valor final de la animaci�n.
                    Duration = TimeSpan.FromMilliseconds(300), // Duraci�n de la animaci�n.
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } // Efecto de aceleraci�n/desaceleraci�n suave.
                };
                // Asocia la animaci�n con la transformaci�n de renderizado del indicador (IndicatorTransform).
                Storyboard.SetTarget(animation, IndicatorTransform);
                // Especifica que la propiedad a animar es "X".
                Storyboard.SetTargetProperty(animation, "X");
                // A�ade la animaci�n al Storyboard y la inicia.
                storyboard.Children.Add(animation);
                storyboard.Begin();
            }
            else
            {
                // Si no se debe animar, mueve el indicador a su posici�n de destino directamente.
                IndicatorTransform.X = targetX;
            }
        }
    }
}