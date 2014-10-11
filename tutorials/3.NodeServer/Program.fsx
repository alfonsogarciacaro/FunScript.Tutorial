#I "../../lib"
#r "FunScript.dll"
#r "FunScript.Interop.dll"
#r "FunScript.TypeScript.Binding.lib.dll"
#r "FunScript.TypeScript.Binding.node.dll"

[<ReflectedDefinition>]
module Program =
    open FunScript.TypeScript

    type Node with
        [<FunScript.JSEmitInline("var http = require('http')")>]
        static member require_http(): unit = failwith "never"

    let main() =
        Node.require_http()

        http.Globals
            .createServer(System.Func<_,_,_>(fun req res ->
                res.writeHead 200.
                res._end "Hello World!"
            )).listen(Globals.Number.Invoke(Globals._process.argv.[2]))

open System.IO
let dir = __SOURCE_DIRECTORY__
let code = FunScript.Compiler.compileWithoutReturn <@ Program.main() @>
File.WriteAllText(Path.Combine(dir, "../simplehttpserver.js"), code)