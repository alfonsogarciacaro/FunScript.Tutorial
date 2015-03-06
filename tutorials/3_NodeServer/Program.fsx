#I "../../lib"
#r "FunScript.dll"
#r "FunScript.Interop.dll"
#r "FunScript.TypeScript.Binding.lib.dll"
#r "FunScript.TypeScript.Binding.node.dll"  // Add reference to node.js binding

[<ReflectedDefinition>]
module Program =
  open FunScript
  open FunScript.TypeScript

  [<FunScript.JSEmitInline("global[{0}] = require({0})")>]
  let require (s: string): unit = failwith "never"

  let main() =
      require "http"
      let port = Globals.Number.Invoke(Globals._process.argv.[2])
        
      http.Globals
         .createServer(System.Func<_,_,_>(fun req res ->
               res.writeHead 200.
               res._end "Hello World!"
         )).listen(port)

// Compile the program and copy the JS code to the parent directory
FunScript.Compiler.Compiler.Compile(<@ Program.main() @>, noReturn=true)
|> fun x -> sprintf "(function(global){%s}(typeof window!=='undefined'?window:global));" x
|> fun x -> System.IO.File.WriteAllText(__SOURCE_DIRECTORY__ + "/simplehttpserver.js", x)
