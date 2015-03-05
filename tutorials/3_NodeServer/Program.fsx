#I "../../lib"
#r "FunScript.dll"
#r "FunScript.Interop.dll"
#r "FunScript.TypeScript.Binding.lib.dll"
#r "FunScript.TypeScript.Binding.node.dll"  // Add reference to node.js binding

[<ReflectedDefinition>]
module Program =
    open FunScript.TypeScript

    // The TypeScript definition define the module methods as static, which create conflicts
    // with the require statement. To solve this, we create an extension for each requirement.
    type Node with
        [<FunScript.JSEmitInline("var http = require('http')")>]
        static member require_http(): unit = failwith "never"

    let main() =
        Node.require_http()
        let port = Globals.Number.Invoke(Globals._process.argv.[2])

        // Then, to use the module methods we need to open <module>.Globals
        http.Globals
            .createServer(System.Func<_,_,_>(fun req res ->
                res.writeHead 200.
                res._end "Hello World!"
            )).listen(port)

// Compile the program and copy the JS code to the parent directory
open System.IO
let dir = __SOURCE_DIRECTORY__
let code = FunScript.Compiler.compileWithoutReturn <@ Program.main() @>
File.WriteAllText(Path.Combine(dir, "../simplehttpserver.js"), code)