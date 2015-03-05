// First include references to the libraries we'll be using
#I "../../lib"
#r "FunScript.dll"
#r "FunScript.Interop.dll"
#r "FunScript.TypeScript.Binding.lib.dll"

// You must always mark the code you want to compile to JavaScript
// with the ReflectedDefinition attribute. This will ask the F# compiler
// to create the expression tree that FunScript will read and compile to JS.
// Alternatively, you can use the FunScript.JSAttribute alias.
[<ReflectedDefinition>]
module Program =
    open FunScript.TypeScript

    // For convenience, we'll be using main() as the program entry point
    let main() =
        // Write to the console using the JS method
        Globals.console.log("Hello JS!")

        // Write to the console using .NET method with formatting
        System.Console.WriteLine("Hello {0} at {1:d} {1:t}!", ".NET", System.DateTime.Now)

        System.Diagnostics.Debug.WriteLine("Debug message")

        // Write to the web page
        let h1 = Globals.document.getElementsByTagName_h1().[0]
        h1.textContent <- "Hello World!"

// Compile the program and copy the JS code to the parent directory
FunScript.Compiler.Compiler.Compile(<@ Program.main() @>, noReturn=true)
|> fun x -> sprintf "(function(global){%s}(typeof window!=='undefined'?window:global));" x
|> fun x -> System.IO.File.WriteAllText(__SOURCE_DIRECTORY__ + "/app.js", x)
