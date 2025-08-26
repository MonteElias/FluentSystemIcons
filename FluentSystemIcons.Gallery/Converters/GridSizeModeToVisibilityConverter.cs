// FluentSystemIcons/FluentSystemIcons.Gallery/Converters/GridSizeVisibilityConverters.cs
using FluentSystemIcons.Gallery.UserControls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

// El espacio de nombres se ha dejado como en el original, aunque las dependencias no utilizadas han sido eliminadas por el compilador.
namespace FluentSystemIcons.Gallery.Converters
{
    // Define un convertidor que devuelve 'Visible' si el tamaño de cuadrícula actual
    // coincide con un parámetro específico. En caso contrario, devuelve 'Collapsed'.
    // Se utiliza en XAML para mostrar un elemento visual (como un icono coloreado) que representa
    // el estado "seleccionado" de un botón en el conmutador de tamaño de cuadrícula.
    public class SelectedGridSizeToVisibilityConverter : IValueConverter
    {
        // El método principal que realiza la conversión de datos para el enlace XAML.
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            // Realiza una serie de comprobaciones para asegurar que los datos de entrada son correctos.
            // 'value' debe ser del tipo GridSizeMode (el tamaño de cuadrícula actual).
            // 'parameter' debe ser un string (el Tag del botón, que representa el tamaño que este activa).
            if (value is GridSizeMode current && parameter is string tag &&
                Enum.TryParse<GridSizeMode>(tag, out var mode)) // Intenta convertir el string del parámetro al enum GridSizeMode.
            {
                // Compara el tamaño de cuadrícula actual con el tamaño que representa el botón.
                // Si son iguales, devuelve Visible; de lo contrario, devuelve Collapsed.
                return current == mode ? Visibility.Visible : Visibility.Collapsed;
            }
            // Si alguna de las comprobaciones falla, devuelve Collapsed como valor predeterminado seguro.
            return Visibility.Collapsed;
        }

        // Este método se usaría para la conversión inversa (de Visibilidad a GridSizeMode).
        // Como no es necesario para esta aplicación, se lanza una excepción para indicar que no está implementado.
        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }

    // Define un convertidor que devuelve 'Visible' si el tamaño de cuadrícula actual
    // NO coincide con un parámetro específico. En caso contrario, devuelve 'Collapsed'.
    // Es el complemento del convertidor anterior y se usa para mostrar el elemento visual
    // de un botón no seleccionado (como un icono con el color de texto por defecto).
    public class UnselectedGridSizeToVisibilityConverter : IValueConverter
    {
        // El método principal que realiza la conversión de datos.
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            // Realiza las mismas validaciones de entrada que el convertidor anterior.
            if (value is GridSizeMode current && parameter is string tag &&
                Enum.TryParse<GridSizeMode>(tag, out var mode))
            {
                // La lógica aquí es la inversa: compara el tamaño de cuadrícula actual con el del botón.
                // Si son DIFERENTES, devuelve Visible; si son iguales, devuelve Collapsed.
                return current != mode ? Visibility.Visible : Visibility.Collapsed;
            }
            // Si las comprobaciones fallan, devuelve Visible, asumiendo que el estado no seleccionado
            // es el estado visual predeterminado.
            return Visibility.Visible;
        }

        // La conversión inversa no está implementada para este convertidor.
        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }
}