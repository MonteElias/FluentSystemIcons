// FluentSystemIcons/FluentSystemIcons.Gallery/UserControls/IconCard.xaml.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using FluentSystemIcons.Gallery.Models;

// Define el espacio de nombres para los controles de usuario personalizados de la galería.
namespace FluentSystemIcons.Gallery.UserControls
{
    // Define la clase para el control de usuario IconCard, que representa visualmente un único icono en la cuadrícula.
    // Es 'sealed' para no poder ser heredada y 'partial' por su vínculo con un archivo XAML.
    public sealed partial class IconCard : UserControl
    {
        // Define la propiedad de dependencia 'Item'. Esto permite enlazar un objeto 'IconInfo'
        // desde el control contenedor (como un GridView) a esta tarjeta.
        // Cada IconCard mostrará los datos del IconInfo que se le asigne.
        public IconInfo Item
        {
            get { return (IconInfo)GetValue(ItemProperty); }
            set { SetValue(ItemProperty, value); }
        }

        // Registro estático de la DependencyProperty 'Item'.
        // Especifica el nombre de la propiedad, su tipo (IconInfo), el tipo propietario (IconCard)
        // y sus metadatos (valor por defecto null).
        public static readonly DependencyProperty ItemProperty =
            DependencyProperty.Register("Item", typeof(IconInfo), typeof(IconCard), new PropertyMetadata(null));

        // Define la propiedad de dependencia 'IconFontSize'.
        // Esto permite controlar el tamaño de la fuente del glifo del icono desde el exterior,
        // por ejemplo, desde las plantillas de datos (DataTemplate) del GridView.
        public double IconFontSize
        {
            get { return (double)GetValue(IconFontSizeProperty); }
            set { SetValue(IconFontSizeProperty, value); }
        }

        // Registro estático de la DependencyProperty 'IconFontSize'.
        // Especifica el nombre, el tipo (double), el tipo propietario (IconCard)
        // y un valor predeterminado de 32.0 píxeles.
        public static readonly DependencyProperty IconFontSizeProperty =
            DependencyProperty.Register("IconFontSize", typeof(double), typeof(IconCard), new PropertyMetadata(32.0));

        // Constructor de la clase IconCard.
        public IconCard()
        {
            // Este método es esencial y es generado por el compilador XAML.
            // Se encarga de cargar el XAML asociado a esta clase y construir el árbol visual de sus componentes.
            this.InitializeComponent();
        }
    }
}