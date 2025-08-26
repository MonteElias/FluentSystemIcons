
# FluentSystemIcons.WinUI
![alt text](https://img.shields.io/nuget/v/FluentSystemIcons.WinUI.svg)  

![alt text](https://img.shields.io/badge/License-MIT-yellow.svg)
Una biblioteca simple y ligera para usar fácilmente el conjunto completo de Iconos del Sistema Fluent de Microsoft en tus aplicaciones de WinUI 3. Proporciona todos los estilos de iconos (Regular y Filled) y los expone a través de clases de C# fuertemente tipadas para una experiencia de desarrollo fluida con IntelliSense y enlaces compilados ({x:Bind}).
Olvídate de buscar códigos Unicode. Empieza a usar nombres semánticos para tus iconos.


## Tabla de Contenidos
- [Características](#características)
- [Instalación](#instalación)
- [Cómo Empezar](#cómo-empezar)
  - [Paso 1: Configurar App.xaml](#paso-1-configurar-appxaml)
  - [Paso 2: Usar los Iconos en XAML](#paso-2-usar-los-iconos-en-xaml)
- [Iconos Disponibles y Nomenclatura](#iconos-disponibles-y-nomenclatura)
- [Para Contribuidores: Actualizar los Iconos](#para-contribuidores-actualizar-los-iconos)
  - [Prerrequisitos](#prerrequisitos)
  - [Pasos para Generar las Clases](#pasos-para-generar-las-clases)
- [Agradecimientos](#agradecimientos)
- [Licencia](#licencia)

## Características  
- **Conjunto Completo de Iconos:** Acceso a miles de iconos del sistema Fluent.
- **Todos los Estilos Incluidos:** Soporte completo para Regular y Filled.
- **Fuertemente Tipado:** Usa constantes de C# en lugar de códigos mágicos. Aprovecha el autocompletado de IntelliSense y la seguridad en tiempo de compilación.
- **Optimizado para WinUI 3:** Utiliza {x:Bind} para un rendimiento máximo, en lugar de {x:Static} que no está soportado.
- **Fácil de Instalar:** Un solo paquete NuGet para empezar en segundos.
- **Aplicación de Galería Incluida:** El repositorio incluye una aplicación de demostración para explorar y buscar visualmente todos los iconos.


## Instalacion

Puedes instalar el paquete a través del Administrador de Paquetes NuGet o la CLI de .NET.  

**Administrador de Paquetes NuGet:**
```sh
PM> Install-Package FluentSystemIcons.WinUI
```

**.NET CLI:**
```sh
dotnet add package FluentSystemIcons.WinUI
```  




## Cómo Empezar  

Usar la biblioteca es un proceso de dos pasos: configurar **App.xaml** una vez y luego usar los iconos en cualquier página que necesites.

**Paso 1: Configurar App.xaml**  
Para que tu aplicación reconozca las fuentes de los iconos, debes agregar el diccionario de recursos de la biblioteca a tu archivo App.xaml. Esto carga las definiciones de FontFamily y las hace disponibles en toda la aplicación.

<!-- En App.xaml -->
<Application
    ...>
    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <XamlControlsResources xmlns="using:Microsoft.UI.Xaml.Controls" />
                
                <!-- Añade esta línea para cargar los recursos de los iconos -->
                <ResourceDictionary Source="ms-appx:///FluentSystemIcons.WinUI/Resources/FluentIcons.xaml"/>
                
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Application.Resources>
</Application>
<!-- Fin de ejemplo -->  

**Paso 2: Usar los Iconos en XAML**
Ahora puedes usar cualquier icono en tus páginas XAML.
Añade el namespace de los símbolos en la cabecera de tu página XAML.
Usa un control como FontIcon.
Establece la FontFamily a la clave del estilo que desees (FluentIconsRegular o FluentIconsFilled).
Usa {x:Bind} para enlazar el Glyph a la constante del icono deseado.
Aquí tienes un ejemplo práctico:  
  
<Page
    ...
    xmlns:symbols="using:FluentSystemIcons">

    <StackPanel Spacing="20" Orientation="Horizontal">
    
        <!-- Ejemplo con FontIcon y el estilo Regular -->
        <FontIcon
            FontFamily="{StaticResource FluentIconsRegular}"
            Glyph="{x:Bind symbols:FluentSystemIconsRegular.Accessibility24, Mode=OneTime}"
            FontSize="48"/>

        <!-- Ejemplo con FontIcon y el estilo Filled -->
        <FontIcon
            FontFamily="{StaticResource FluentIconsFilled}"
            Glyph="{x:Bind symbols:FluentSystemIconsFilled.Accessibility24, Mode=OneTime}"
            FontSize="48" />
            
    </StackPanel>
</Page>  





## Iconos Disponibles y Nomenclatura
Los iconos están organizados en clases estáticas que corresponden a su estilo. La convención de nomenclatura convierte el nombre del icono original a PascalCase.  
- **Nombre Original:** ic_fluent_access_time_24_regular  

- **Clase Generada:** FluentSystemIconsRegular  

- **Propiedad Constante:** AccessTime24  
- **Las clases disponibles son:** FluentSystemIconsRegular  FluentSystemIconsFilled  
 
Para una lista completa y una búsqueda visual de todos los iconos, explora y ejecuta el proyecto FluentSystemIcons.Gallery incluido en esta solución.

## Para Contribuidores: Actualizar los Iconos 
Si Microsoft actualiza los iconos del sistema Fluent, puedes regenerar las clases de C# fácilmente usando el generador incluido en la solución.

- ## Prerrequisitos
- **El Proyecto Generador:** La solución incluye un proyecto de consola llamado IconCodeGenerator (o similar) que contiene el código C# que has proporcionado.
- **Activos Oficiales de Iconos:** Necesitas descargar los archivos de fuentes originales del repositorio oficial de Microsoft.  

- ° **Enlace de descarga:** [Microsoft Fluent UI System Icons](https://github.com/microsoft/fluentui-system-icons) Repository  

Pasos para Generar las Clases
Descargar los Activos de Microsoft: Ve al repositorio de GitHub de Fluent UI System Icons, haz clic en Code y luego en Download ZIP. Descomprime el archivo en tu máquina.  

- **Localizar los Archivos .css:**  
 Dentro de la carpeta que has descomprimido, navega a fonts/. Aquí encontrarás los archivos CSS que el generador necesita, como:  
FluentSystemIcons-Regular.css  
FluentSystemIcons-Filled.css

- **Preparar el Generador**
Abre el proyecto del generador en Visual Studio.
Navega a su carpeta de ejecución. La forma más fácil de encontrarla es hacer clic derecho en el proyecto del generador, seleccionar Abrir carpeta en el Explorador de archivos, y luego navegar a bin/Debug/netX.X/.
Copia los archivos .css del paso anterior (FluentSystemIcons-Regular.css, etc.) y pégalos en esta carpeta de ejecución.  
- **Ejecutar el Generador:**  
Compila y ejecuta el proyecto del generador (puedes presionar F5 en Visual Studio). La aplicación de consola se ejecutará, procesará los archivos .css y creará los nuevos archivos .cs en la misma carpeta.  
- **Actualizar la Biblioteca:**  
En la carpeta de ejecución del generador, encontrarás los nuevos archivos generados (ej:  FluentSystemIconsRegular.cs, FluentSystemIconsFilled.cs).

Copia estos archivos .cs.
Pégalos en la carpeta del proyecto de la biblioteca FluentSystemIcons.WinUI, sobrescribiendo los archivos existentes.
¡Listo! Has actualizado la biblioteca con el último conjunto de iconos de Fluent.




## Agradecimientos
Este paquete no sería posible sin el increíble trabajo del equipo de Microsoft. Todos los iconos son obtenidos directamente del repositorio oficial:  
[Microsoft Fluent UI System Icons](https://github.com/microsoft/fluentui-system-icons)

## Licencia
[![Licencia](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
Este proyecto está bajo la Licencia MIT. Consulta el archivo LICENSE.md para más detalles.