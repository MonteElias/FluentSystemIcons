// FluentSystemIcons/FluentSystemIcons.Gallery/Converters/CodeViewVisibilityConverters.cs
using FluentSystemIcons.Gallery.UserControls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

// Define el espacio de nombres para las clases convertidoras de la aplicación.
// Los convertidores son clases de utilidad que se usan en los enlaces de datos de XAML
// para transformar un tipo de dato en otro (ej. un enum a un valor de Visibilidad).
namespace FluentSystemIcons.Gallery.Converters
{
    // Define un convertidor que devuelve 'Visible' si el modo de vista de código actual
    // coincide con un parámetro específico. En caso contrario, devuelve 'Collapsed'.
    // Se utiliza en XAML para mostrar un elemento (como el texto blanco de un botón)
    // solo cuando ese botón representa el estado seleccionado.
    public class SelectedCodeViewToVisibilityConverter : IValueConverter
    {
        // El método principal que realiza la conversión de datos.
        // Se llama automáticamente por el motor de enlace de XAML.
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            // Comprueba si los datos de entrada son válidos:
            // 'value' debe ser del tipo CodeViewMode (el modo actual).
            // 'parameter' debe ser un string (el Tag del botón, que representa el modo que este botón activa).
            if (value is CodeViewMode current && parameter is string tag &&
                Enum.TryParse<CodeViewMode>(tag, out var mode)) // Intenta convertir el string del parámetro al enum.
            {
                // Compara el modo actual con el modo que representa el botón.
                // Si son iguales, devuelve Visible; de lo contrario, devuelve Collapsed.
                return current == mode ? Visibility.Visible : Visibility.Collapsed;
            }
            // Si los datos de entrada no son válidos, devuelve Collapsed por seguridad.
            return Visibility.Collapsed;
        }

        // Este método se usaría para la conversión inversa (de Visibilidad a CodeViewMode).
        // No es necesario en este caso, por lo que se lanza una excepción para indicar que no está implementado.
        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }

    // Define un convertidor que devuelve 'Visible' si el modo de vista de código actual
    // NO coincide con un parámetro específico. En caso contrario, devuelve 'Collapsed'.
    // Es el inverso del convertidor anterior y se usa para mostrar el elemento
    // de un botón no seleccionado (como el texto con el color del tema).
    public class UnselectedCodeViewToVisibilityConverter : IValueConverter
    {
        // El método principal que realiza la conversión de datos.
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            // Realiza las mismas validaciones de entrada que el convertidor anterior.
            if (value is CodeViewMode current && parameter is string tag &&
                Enum.TryParse<CodeViewMode>(tag, out var mode))
            {
                // La lógica aquí es la inversa: compara el modo actual con el modo del botón.
                // Si son DIFERENTES, devuelve Visible; si son iguales, devuelve Collapsed.
                return current != mode ? Visibility.Visible : Visibility.Collapsed;
            }
            // Si los datos de entrada no son válidos, devuelve Visible como valor predeterminado seguro,
            // asumiendo que el estado no seleccionado es el más común.
            return Visibility.Visible;
        }

        // La conversión inversa no está implementada para este convertidor.
        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }
}