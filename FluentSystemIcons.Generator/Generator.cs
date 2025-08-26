// FluentSystemIcons/FluentSystemIcons.Generator/Generator.cs

using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;

// Define la clase principal del programa generador.
class Generator
{
    // Método principal y punto de entrada de la aplicación de consola.
    static void Main()
    {
        // Muestra un mensaje de inicio en la consola.
        Console.WriteLine("🚀 Iniciando la generación de clases de iconos de Fluent...");

        // Obtiene una lista de todos los archivos con extensión .css en el directorio actual.
        string[] cssFiles = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.css", SearchOption.TopDirectoryOnly);

        // Comprueba si se encontraron archivos .css.
        if (cssFiles.Length == 0)
        {
            // Si no se encuentran archivos, muestra una advertencia y termina la ejecución.
            Console.WriteLine("⚠️ No se encontraron archivos .css en la carpeta de ejecución.");
            return;
        }

        // Itera sobre cada archivo .css encontrado.
        foreach (var cssFile in cssFiles)
        {
            // Obtiene el nombre del archivo sin la extensión.
            string fileName = Path.GetFileNameWithoutExtension(cssFile);
            // Informa al usuario qué archivo se está procesando.
            Console.WriteLine($"\n📄 Procesando archivo: {Path.GetFileName(cssFile)}");

            // Comprueba si el nombre del archivo contiene "Resizable".
            if (fileName.Contains("Resizable", StringComparison.OrdinalIgnoreCase))
            {
                // Si es un archivo de iconos redimensionables, llama al método específico para procesarlo.
                ProcessResizable(cssFile);
            }
            else
            {
                // Si no es "Resizable", determina el estilo (Regular, Filled, Light) según el nombre del archivo.
                if (fileName.Contains("Regular", StringComparison.OrdinalIgnoreCase))
                {
                    // Procesa el archivo CSS para el estilo "Regular".
                    ProcessCss(cssFile, fileName.Replace("-", ""), "_regular");
                }
                else if (fileName.Contains("Filled", StringComparison.OrdinalIgnoreCase))
                {
                    // Procesa el archivo CSS para el estilo "Filled".
                    ProcessCss(cssFile, fileName.Replace("-", ""), "_filled");
                }
                else if (fileName.Contains("Light", StringComparison.OrdinalIgnoreCase))
                {
                    // Procesa el archivo CSS para el estilo "Light".
                    ProcessCss(cssFile, fileName.Replace("-", ""), "_light");
                }
                else
                {
                    // Si no se puede determinar el estilo, omite el archivo.
                    Console.WriteLine($"❓ No se pudo determinar el sufijo para {fileName}. Omitiendo archivo.");
                }
            }
        }

        // Muestra un mensaje final indicando que el proceso ha terminado con éxito.
        Console.WriteLine("\n✅✅✅ Generación completada para todos los estilos encontrados. ✅✅✅");
    }

    // Procesa un archivo CSS para un estilo de icono específico (Regular, Filled, o Light).
    static void ProcessCss(string cssFile, string className, string styleSuffix)
    {
        // Lee todo el contenido del archivo CSS.
        string cssContent = File.ReadAllText(cssFile);
        // Define una expresión regular para encontrar nombres de iconos y sus códigos Unicode.
        var regex = new Regex($@"\.icon-ic_fluent_(.*?){Regex.Escape(styleSuffix)}:before\s*\{{\s*content:\s*""\\(.*?)"";", RegexOptions.IgnoreCase);

        // Usa un SortedDictionary para almacenar los iconos en orden alfabético por nombre.
        var icons = new SortedDictionary<string, string>();
        // Busca todas las coincidencias de la expresión regular en el contenido del CSS.
        foreach (Match match in regex.Matches(cssContent))
        {
            // Extrae el nombre del icono y lo convierte a formato PascalCase.
            string propName = ToPascalCase(match.Groups[1].Value);
            // Extrae el código Unicode del icono.
            string unicode = match.Groups[2].Value;
            // Añade el icono y su código al diccionario.
            icons[propName] = unicode;
        }

        // Comprueba si se encontraron iconos.
        if (icons.Count > 0)
        {
            // Si se encontraron, llama al método para escribir el archivo de la clase C#.
            WriteClassFile(className, icons);
            // Informa al usuario que la clase fue generada con éxito.
            Console.WriteLine($"   ✅ Generada clase {className}.cs con {icons.Count} iconos.");
        }
        else
        {
            // Si no se encontraron iconos, informa al usuario.
            Console.WriteLine($"   ⚠️ No se encontraron iconos para el estilo '{styleSuffix}' en {Path.GetFileName(cssFile)}.");
        }
    }

    // Procesa específicamente el archivo CSS que contiene todos los iconos redimensionables (Resizable).
    static void ProcessResizable(string cssFile)
    {
        // Lee todo el contenido del archivo CSS.
        string cssContent = File.ReadAllText(cssFile);

        // Define los diferentes estilos (sufijos y nombres de clase) que se encuentran dentro del archivo Resizable.
        var styles = new[]
        {
            new { Suffix = "_regular", Class = "FluentSystemIconsResizableRegular" },
            new { Suffix = "_filled",  Class = "FluentSystemIconsResizableFilled"  },
            new { Suffix = "_light",   Class = "FluentSystemIconsResizableLight"   }
        };

        // Itera sobre cada uno de los estilos definidos.
        foreach (var style in styles)
        {
            // Define la expresión regular para el estilo actual.
            var regex = new Regex($@"\.icon-ic_fluent_(.*?){Regex.Escape(style.Suffix)}:before\s*\{{\s*content:\s*""\\(.*?)"";", RegexOptions.IgnoreCase);

            // Crea un diccionario para almacenar los iconos de este estilo.
            var icons = new SortedDictionary<string, string>();
            // Busca todas las coincidencias para el estilo actual.
            foreach (Match match in regex.Matches(cssContent))
            {
                // Extrae el nombre del icono y lo convierte a PascalCase.
                string propName = ToPascalCase(match.Groups[1].Value);
                // Extrae el código Unicode.
                string unicode = match.Groups[2].Value;
                // Añade el icono al diccionario.
                icons[propName] = unicode;
            }

            // Comprueba si se encontraron iconos para este estilo.
            if (icons.Count > 0)
            {
                // Si se encontraron, escribe el archivo de la clase C# correspondiente.
                WriteClassFile(style.Class, icons);
                // Informa al usuario sobre la generación exitosa.
                Console.WriteLine($"   ✅ Generada clase {style.Class}.cs con {icons.Count} iconos.");
            }
            else
            {
                // Si no, informa al usuario que no se encontraron iconos para este estilo.
                Console.WriteLine($"   ⚠️ No se encontraron iconos para el estilo '{style.Suffix}' en el archivo Resizable.");
            }
        }
    }

    // Escribe el contenido de una clase C# en un archivo .cs.
    static void WriteClassFile(string className, SortedDictionary<string, string> icons)
    {
        // Usa un StringBuilder para construir eficientemente el contenido del archivo.
        var sb = new StringBuilder();
        // Añade la declaración del namespace.
        sb.AppendLine("namespace FluentSystemIcons");
        sb.AppendLine("{");
        // Añade la declaración de la clase estática.
        sb.AppendLine($"    public static class {className}");
        sb.AppendLine("    {");
        // Itera sobre cada icono en el diccionario.
        foreach (var icon in icons)
        {
            // Obtiene el nombre en PascalCase y el código hexadecimal en mayúsculas.
            string pascalName = icon.Key;
            string hexCodeUpper = icon.Value.ToUpper();

            // Genera un nombre legible para mostrar (ej. "AccessTime24" -> "Access Time 24").
            string displayName = SplitPascalCase(pascalName);

            // Crea la representación del glifo para C# (ej. \uE001).
            string csharpGlyph = $"\\u{hexCodeUpper}";
            // Crea el código para XAML (ej. &#x_E001;).
            string xamlCode = $"&#x{hexCodeUpper};";

            // Añade la constante pública con el glifo para C#.
            sb.AppendLine($"        public const string {pascalName} = \"{csharpGlyph}\";");
            sb.AppendLine();

            // Añade la constante pública con el código para XAML.
            sb.AppendLine($"        public const string {pascalName}XamlCode = \"{xamlCode}\";");
            sb.AppendLine();

            // Añade la constante con el nombre legible, junto con comentarios de documentación XML.
            sb.AppendLine($"        /// <summary>");
            sb.AppendLine($"        /// Human-readable name for the icon. Value: \"{displayName}\"");
            sb.AppendLine($"        /// </summary>");
            sb.AppendLine($"        public const string {pascalName}DisplayName = \"{displayName}\";");
            sb.AppendLine();
        }
        // Cierra las llaves de la clase y el namespace.
        sb.AppendLine("    }");
        sb.AppendLine("}");

        // Escribe todo el contenido generado en un archivo .cs con codificación UTF-8.
        File.WriteAllText($"{className}.cs", sb.ToString(), Encoding.UTF8);
    }

    // Convierte una cadena de texto (ej. "access_time_24") a formato PascalCase (ej. "AccessTime24").
    static string ToPascalCase(string input)
    {
        // Reemplaza los guiones bajos por espacios.
        string processedInput = input.Replace('_', ' ');
        // Divide la cadena en partes usando el espacio como delimitador.
        var parts = processedInput.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        // Itera sobre cada parte.
        for (int i = 0; i < parts.Length; i++)
        {
            if (parts[i].Length > 0)
            {
                // Convierte el primer carácter a mayúscula y el resto a minúsculas.
                parts[i] = char.ToUpperInvariant(parts[i][0]) + parts[i].Substring(1).ToLowerInvariant();
            }
        }
        // Une todas las partes para formar la cadena final en PascalCase.
        return string.Join("", parts);
    }

    // Divide un nombre en formato PascalCase en una cadena legible por humanos (ej. "SplitPascalCase" -> "Split Pascal Case").
    static string SplitPascalCase(string input)
    {
        // Si la cadena es nula o vacía, la devuelve tal cual.
        if (string.IsNullOrEmpty(input))
            return input;

        // Usa un StringBuilder para construir la nueva cadena.
        var sb = new StringBuilder();
        // Añade el primer carácter.
        sb.Append(input[0]);

        // Itera sobre el resto de la cadena.
        for (int i = 1; i < input.Length; i++)
        {
            char c = input[i];
            // Si el carácter actual es una letra mayúscula, añade un espacio antes.
            if (char.IsUpper(c))
            {
                sb.Append(' ');
            }
            // O si es un número y el carácter anterior no era un número, también añade un espacio.
            else if (char.IsDigit(c) && !char.IsDigit(input[i - 1]))
            {
                sb.Append(' ');
            }
            // Añade el carácter actual.
            sb.Append(c);
        }
        // Devuelve la cadena resultante.
        return sb.ToString();
    }
}