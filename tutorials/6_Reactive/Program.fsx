
#I "../../lib"
#r "FunScript"
#r "FunScript.Interop"
#r "FunScript.TypeScript.Binding.lib"
#r "FunScript.TypeScript.Binding.jquery"
#r "FunScript.HTML"
#r "System.Reactive.Linq"
#r "FSharp.Control.Reactive"
#r "FunScript.Rx"

[<ReflectedDefinition>]
module Program =
    open FunScript.HTML
    open FunScript.TypeScript
    open System
    open System.Collections.Generic
    open FSharp.Control.Reactive

    [<FunScript.JSEmit("""return Rx.Observable.fromEventPattern(function (h) { {0}.on({1}, h); }, function (h) { {0}.off({1}, h); })""")>]
    let fromRactive ractive eventName: IObservable<RactiveEvent> = failwith "never"

    [<FunScript.JSEmitInline("Rx.Observable.fromPromise({0})")>]
    let fromPromise promise: IObservable<_> = failwith "never"

    let main() =
        let maxSuggestions = 3
        let setRandomSuggestion (ractive : Ractive, index: int) =
            let users: IList<_> = unbox <| ractive.get("users")
            let rnd = floor <| Globals.Math.random() * users.length
            ractive.set(String.Format("suggestions[{0}]", index), users.[int rnd])
        
        let ractive = Globals.Ractive.CreateFast("ractive-container", "#ractive-template")

        fromRactive ractive "refresh"
        |> Observable.startWith []
        |> Observable.map (fun _ -> floor <| Globals.Math.random() * 500.)
        |> Observable.map (fun offset -> "https://api.github.com/users?since=" + (string offset))
        |> Observable.flatmap (fun url -> fromPromise <| Globals.jQuery.getJSON url)
        |> Observable.add (fun users ->
            ractive.set("users", users)
            for i = 0 to maxSuggestions - 1 do
                setRandomSuggestion(ractive, i))

        fromRactive ractive "close"
        |> Observable.add (fun ev -> setRandomSuggestion(ractive, ev.index.["i"]))


// This will compile the code to JS and copy the html file and the generated script to the parent directory
open System.IO
let dir = __SOURCE_DIRECTORY__
let code = FunScript.Compiler.Compiler.Compile
            (<@ Program.main() @>,
            noReturn = true,
            isEventMappingEnabled = false,
            components = FunScript.Rx.Interop.components())

File.WriteAllText(Path.Combine(dir, "../app.js"), code)
File.Copy(Path.Combine(dir, "index.html"), Path.Combine(dir, "../index.html"), overwrite=true)