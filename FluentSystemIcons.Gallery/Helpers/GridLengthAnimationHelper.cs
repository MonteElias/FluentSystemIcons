// FluentSystemIcons/FluentSystemIcons.Gallery/Helpers/GridLengthAnimationHelper.cs
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// Define el espacio de nombres para las clases de ayuda o utilidad de la aplicación.
namespace FluentSystemIcons.Gallery.Helpers
{
    // Define la clase GridLengthAnimationHelper. Su propósito es actuar como un intermediario
    // para permitir la animación de la propiedad 'Width' de un 'ColumnDefinition',
    // ya que 'GridLength' no es un tipo que se pueda animar directamente.
    // Hereda de FrameworkElement, lo que le permite ser instanciado como un recurso en XAML
    // y formar parte del árbol lógico de la UI, haciéndolo más robusto y compatible.
    public class GridLengthAnimationHelper : FrameworkElement
    {
        // Define una DependencyProperty llamada 'AnimatedValue'. Esta es la clave del mecanismo.
        // Las animaciones en WinUI (como DoubleAnimation) solo pueden apuntar a DependencyProperties.
        // Por lo tanto, animaremos esta propiedad de tipo 'double'.
        public static readonly DependencyProperty AnimatedValueProperty =
            DependencyProperty.Register(nameof(AnimatedValue), typeof(double), typeof(GridLengthAnimationHelper),
            new PropertyMetadata(0.0, OnAnimatedValueChanged));

        // Propiedad pública de C# que envuelve la DependencyProperty 'AnimatedValue'.
        // Es esta propiedad la que será el objetivo de un Storyboard y su DoubleAnimation.
        public double AnimatedValue
        {
            get { return (double)GetValue(AnimatedValueProperty); }
            set { SetValue(AnimatedValueProperty, value); }
        }

        // Propiedad pública para mantener una referencia a la ColumnDefinition real que queremos animar.
        // Esta referencia se debe asignar desde el código (code-behind) antes de iniciar la animación.
        public ColumnDefinition? TargetColumn { get; set; }

        // Método de devolución de llamada estático que se invoca automáticamente cada vez que
        // el valor de la propiedad 'AnimatedValue' cambia (es decir, en cada fotograma de la animación).
        private static void OnAnimatedValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Convierte el objeto de dependencia de nuevo a nuestra clase helper.
            var helper = (GridLengthAnimationHelper)d;
            // Se asegura de que la columna de destino haya sido asignada.
            if (helper.TargetColumn != null)
            {
                // Obtiene el nuevo valor numérico del evento de cambio.
                var newWidth = (double)e.NewValue;
                // Aplica el nuevo valor de ancho a la propiedad 'Width' de la ColumnDefinition de destino.
                // El valor 'double' se envuelve en una estructura 'GridLength', que es lo que la propiedad Width espera.
                helper.TargetColumn.Width = new GridLength(newWidth);
            }
        }
    }
}