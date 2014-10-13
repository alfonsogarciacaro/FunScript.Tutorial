#I "../../lib"
#r "FunScript.dll"
#r "FunScript.HTML.dll"
#r "FunScript.Interop.dll"
#r "FunScript.TypeScript.Binding.lib.dll"

// A disadvantage of using FunScript is that the generated JS code may become very obfuscated
// (This may be actually a good point depending on the situation though).
// Debugging thus becomes difficult. To overcome this, you can use the methods
// WriteLine and WriteLineIF from the System.Diagnostics.Debug class.
// This statements will only compile to JS if you add "--define:DEBUG" to
// Tools > Options > F# Tools > F# Interactive > F# interactive Options
// in Visual Studio or Xamarin Studio (the path may to the option may vary a bit)
// When you're finished debugging, remove it and your code will be ready for production!

[<ReflectedDefinition>]
module Program =
    open FunScript.TypeScript
    open FunScript.HTML

    let main() =
        let h1 = Globals.document.getElementsByTagName_h1().[0]
        h1.textContent <- "Hello World!"

        // This will be printed always
        // Remember to use Console.WriteLine() as Console.Write() is not allowed
        let valueA = 25
        System.Console.WriteLine("Hello World. valueA: {0}", valueA)

        // This will only be printed if the DEBUG symbol is defined
        // Note you also have formatting capabilities in this method
        let valueB = 50
        System.Diagnostics.Debug.WriteLine("Hello World Debug. valueB: {0}", valueB)

        // These will only be printed if the DEBUG symbol is defined
        // and the condition is fulfilled
        let now = System.DateTime.Now
        System.Diagnostics.Debug.WriteLineIf(now.Second % 2 = 0, "Seconds are even: " + now.ToString())
        System.Diagnostics.Debug.WriteLineIf(now.Second % 2 = 1, "Seconds are odd: " + now.ToString())

        // By opening FunScript.HTML you can count on another useful tool for debugging:
        // Add objects to the JS global namespace (window) and watch them through the browser console
        // By typing "state.valueA", for example
        #if DEBUG
        Globals.window.["state"] <- dict [("valueA", valueA); ("valueB", valueB)]
        #endif

open System.IO
let dir = __SOURCE_DIRECTORY__
let components = FunScript.HTML.Components.getHTMLComponents()
let code = FunScript.Compiler.Compiler.Compile(<@ Program.main() @>, noReturn=true, components=components)
File.WriteAllText(Path.Combine(dir, "../app.js"), code)
File.Copy(Path.Combine(dir, "index.html"), Path.Combine(dir, "../index.html"), overwrite=true)