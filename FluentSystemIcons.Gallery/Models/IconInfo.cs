// FluentSystemIcons/FluentSystemIcons.Gallery/Models/IconInfo.cs
using Microsoft.UI.Xaml.Media;
using System.Collections.Generic;

// Define el espacio de nombres para los modelos de datos de la aplicación.
namespace FluentSystemIcons.Gallery.Models
{
    // Define la clase 'IconInfo', que actúa como un modelo de datos para representar un único icono.
    // Cada instancia de esta clase contiene toda la información necesaria para mostrar,
    // buscar y generar código para un icono específico.
    public class IconInfo
    {
        // El nombre programático del icono, usualmente en formato PascalCase (ej. "Home24Filled").
        // Se usa para generar código y para la lógica interna.
        public string Name { get; set; }

        // El nombre del icono formateado para ser legible por humanos (ej. "Home 24 Filled").
        // Se muestra en la interfaz de usuario y se utiliza en las búsquedas.
        public string DisplayName { get; set; }

        // El carácter Unicode real que representa el icono (ej. "\uF488").
        // Este es el glifo que se renderiza usando la fuente de iconos.
        public string Glyph { get; set; }

        // La representación del glifo en formato de entidad de caracteres XAML (ej. "&#xF488;").
        // Se usa para copiar el código para su uso directo en archivos XAML.
        public string XamlCode { get; set; }

        // El objeto FontFamily (ej. "FluentSystemIcons-Filled") necesario para que el sistema de renderizado
        // sepa qué fuente usar para mostrar el glifo correctamente.
        public FontFamily FontFamily { get; set; }

        // La clave de recurso estático (StaticResource) de XAML asociada con la FontFamily (ej. "FluentIconsFilled").
        // Se utiliza para generar fragmentos de código XAML correctos.
        public string FontKey { get; set; }

        // Una lista de palabras clave asociadas con el icono para mejorar la funcionalidad de búsqueda.
        // Actualmente inicializada como una lista vacía.
        public List<string> Keywords { get; set; } = new();

        // Constructor de la clase IconInfo.
        public IconInfo()
        {
            // Inicializa todas las propiedades de tipo string a 'string.Empty' para evitar valores nulos.
            Name = string.Empty;
            DisplayName = string.Empty;
            Glyph = string.Empty;
            XamlCode = string.Empty;
            FontKey = string.Empty;
            // Inicializa la propiedad FontFamily con 'null!' (operador de anulación de nulos),
            // indicando al compilador que esta propiedad será asignada más tarde y no será nula cuando se use.
            FontFamily = null!;
        }
    }
}