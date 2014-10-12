// First include references to the libraries we'll be using
#I "../../lib"
#r "FunScript.dll"
#r "FunScript.HTML.dll"
#r "FunScript.Interop.dll"
#r "FunScript.TypeScript.Binding.lib.dll"

// You must always mark the code you want to compile to JavaScript
// with the attribute ReflectedDefinition. This will ask the F# compiler
// to create the expression tree that FunScript will read and compile to JS.
// Alternatively, you can use the alias FunScript.JS
[<ReflectedDefinition>]
module Program =
    open FunScript.TypeScript

    // For convenience, we'll be using main() as the program entry point
    let main() =
        // Write to the console using the JS method
        Globals.console.log("Hello JS!")

        // Write to the console using .NET method with formatting
        System.Console.WriteLine("Hello {0} at {1:d} {1:t}!", ".NET", System.DateTime.Now)

        // Write to the web page
        let h1 = Globals.document.getElementsByTagName_h1().[0]
        h1.textContent <- "Hello World!"


// This will compile the code to JS and copy the html file and the genereated script to the parent directory
open System.IO
let dir = __SOURCE_DIRECTORY__
// External libraries can provide additional components to FunScript compiler
// In most of the tutorials we'll be using components from FunScript.HTML extensions
let components = FunScript.HTML.Components.getHTMLComponents()
let code = FunScript.Compiler.Compiler.Compile(<@ Program.main() @>, noReturn=true, components=components)
File.WriteAllText(Path.Combine(dir, "../app.js"), code)
File.Copy(Path.Combine(dir, "index.html"), Path.Combine(dir, "../index.html"), overwrite=true)