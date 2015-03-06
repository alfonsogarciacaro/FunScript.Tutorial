#I "../../lib"
#r "FunScript.dll"
#r "FunScript.Interop.dll"
#r "FunScript.TypeScript.Binding.lib.dll"
#r "FunScript.HTML"

[<ReflectedDefinition>]
module Program =
    open FunScript.TypeScript
    open FunScript.HTML
    open System.Net

    type GitHub = { users: obj[]; suggestions: obj[] }

    let maxSuggestions = 3

    let setRandomSuggestion (ractive: Ractive) index =
      let users: obj[] = ractive.get("users") |> unbox
      let rnd = Globals.Math.random() * users.length
                |> Globals.Math.floor |> int
      ractive.set(sprintf "suggestions[%i]" index, users.[rnd])

    let main() =
      let options = createEmpty<RactiveNewOptions>()
      options.el <- "ractive-container"
      options.template <- "#ractive-template"
      options.data <- { users=[||]; suggestions=[||] }

      let ractive = Globals.Ractive.Create(options)
      ractive.onStream("refresh")
      |> Observable.map (fun x ->
         let offset = Globals.Math.floor(Globals.Math.random() * 500.)
         let req = WebRequest.Create(sprintf "https://api.github.com/users?since=%f" offset)
         req.AsyncGetJSONP())
      |> Observable.add (fun data ->
         async { let! users = data
                 ractive.set("users", users)
                 for i=0 to maxSuggestions do
                     setRandomSuggestion ractive i }
         |> Async.StartImmediate)

      ractive.onStream("close")
      |> Observable.add(fun (ev, _) ->
         setRandomSuggestion ractive ev.index.["i"])

FunScript.Compiler.Compiler.Compile(<@ Program.main() @>, noReturn=true, components=FunScript.HTML.Components.getHTMLComponents())
|> fun x -> sprintf "(function(global){%s}(typeof window!=='undefined'?window:global));" x
|> fun x -> System.IO.File.WriteAllText(__SOURCE_DIRECTORY__ + "/app.js", x)
