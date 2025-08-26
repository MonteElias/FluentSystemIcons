// FluentSystemIcons/FluentSystemIcons.Gallery/AnimationExtensions.cs
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Threading.Tasks;

// Define el espacio de nombres principal de la aplicación de la galería.
namespace FluentSystemIcons.Gallery
{
    // Define una clase estática para contener métodos de extensión.
    // Los métodos de extensión permiten "agregar" nuevos métodos a tipos existentes sin modificar su código fuente.
    public static class AnimationExtensions
    {
        // Define un método de extensión para la clase 'Storyboard'.
        // El 'this Storyboard storyboard' indica que este método se puede llamar en cualquier objeto Storyboard
        // como si fuera un método de instancia (ej. miStoryboard.BeginAsync()).
        // Devuelve un 'Task', lo que permite que su ejecución sea esperada (await) en un método asíncrono.
        public static Task BeginAsync(this Storyboard storyboard)
        {
            // Crea una instancia de TaskCompletionSource (TCS). Este objeto es una herramienta fundamental
            // para crear un 'Task' a partir de operaciones que no son nativamente asíncronas,
            // como las que se basan en eventos (como en este caso).
            var tcs = new TaskCompletionSource<bool>();

            // Declara una función local que actuará como manejador del evento 'Completed' del Storyboard.
            // Se define aquí para poder suscribirse y desuscribirse de ella fácilmente.
            void onCompleted(object? sender, object e)
            {
                // Es una buena práctica desuscribirse del evento una vez que se ha disparado para evitar fugas de memoria,
                // especialmente si el Storyboard pudiera ser reutilizado.
                storyboard.Completed -= onCompleted;
                // Llama a SetResult(true) en el TaskCompletionSource. Esto marca el 'Task' asociado
                // como completado exitosamente, lo que permite que cualquier código que esté esperando ('await')
                // el Task continúe su ejecución.
                tcs.SetResult(true);
            }

            // Se suscribe al evento 'Completed' del Storyboard. Cuando la animación termine,
            // se ejecutará la función 'onCompleted'.
            storyboard.Completed += onCompleted;
            // Inicia la animación del Storyboard. Esta es la llamada original, que no se puede esperar.
            storyboard.Begin();

            // Devuelve la Tarea ('Task') del TaskCompletionSource. El código que llame a este método
            // recibirá esta Tarea y podrá usar 'await' sobre ella, pausando su ejecución hasta que
            // 'tcs.SetResult()' sea llamado dentro del manejador de eventos 'onCompleted'.
            return tcs.Task;
        }
    }
}