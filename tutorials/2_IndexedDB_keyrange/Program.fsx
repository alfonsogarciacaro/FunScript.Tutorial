#I "../../lib"
#r "FunScript.dll"
#r "FunScript.HTML.dll"
#r "FunScript.Interop.dll"
#r "FunScript.TypeScript.Binding.lib.dll"

// Adapted from the example in MDN: mdn.github.io/IDBKeyRange-example
// For more detailed comments, see the IndexedDB_index example

[<ReflectedDefinition>]
module Program =
    open FunScript.TypeScript
    open FunScript.HTML
    open FunScript.HTML.Storage

    // Define the type we'll use to create the Object Store
    // The store will have the name of the type
    type Thing = 
        {
            fThing: string
            fRating: int
        }
        static member CreateDummyData() = [|
          { fThing= "Drum kit"; fRating= 10 }
          { fThing= "Family"; fRating= 10 }
          { fThing= "Batman"; fRating= 9 }      
          { fThing= "Brass eye"; fRating= 9 }
          { fThing= "The web"; fRating= 9 }
          { fThing= "Mozilla"; fRating= 9 }
          { fThing= "Firefox OS"; fRating= 9 }
          { fThing= "Curry"; fRating= 9 }
          { fThing= "Paneer cheese"; fRating= 8 }
          { fThing= "Mexican food"; fRating= 8 }
          { fThing= "Chocolate"; fRating= 7 }
          { fThing= "Heavy metal"; fRating= 10 }
          { fThing= "Monty Python"; fRating= 8 }
          { fThing= "Aphex Twin"; fRating= 8 }
          { fThing= "Gaming"; fRating= 7 }
          { fThing= "Frank Zappa"; fRating= 9 }
          { fThing= "Open minds"; fRating= 10 }
          { fThing= "Hugs"; fRating= 9 }
          { fThing= "Ale"; fRating= 9 }
          { fThing= "Christmas"; fRating= 8 }
        |]

    // Define a type with default constructor and implementing the DBImplementation interface
    // The name of this type will be used to identify the database
    // When we open the database, if it doesn't exists or it does with an inferior version number, Upgrade will be triggered
    type ThingDB() =
        interface DBImplementation with
            member x.Version with get() = 1u
            member x.Upgrade(db) =
                let store = db.createStore<Thing,string> (KeyPath <@ fun t -> t.fThing @>)
                store.createIndex <@ fun t -> t.fRating @>

    let query<'T> selector =
        unbox<'T>(Globals.document.querySelector(selector))
             
    let displayData() = async {
        try
            let listEl = query<HTMLElement> "ul"
            let onlyText = query<HTMLInputElement> "#onlytext"
            let rangeLowerText = query<HTMLInputElement> "#rangelowertext"
            let rangeUpperText = query<HTMLInputElement> "#rangeuppertext"
            let lowerBoundText = query<HTMLInputElement> "#lowerboundtext"
            let upperBoundText = query<HTMLInputElement> "#upperboundtext"

            let keyRangeValue =
                match query<HTMLInputElement>("input[name='value']:checked").value with
                | "none" -> Unchecked.defaultof<IDBKeyRange>
                | "only" -> Globals.IDBKeyRange.only(onlyText.value)
                | "range" -> Globals.IDBKeyRange.bound(rangeLowerText.value, rangeUpperText.value, false, false)
                | "lower" -> Globals.IDBKeyRange.lowerBound(lowerBoundText.value)
                | "upper" -> Globals.IDBKeyRange.upperBound(upperBoundText.value)
                | _ -> failwith "Unrecognized KeyRange selection"

            listEl.innerHTML <- ""
            let db = IndexedDB<ThingDB>()
            do! db.useStore<Thing,_>(fun cs -> async {
                for thing in cs.openCursor(range=keyRangeValue) do
                    let item = Globals.document.createElement("li")
                    item.innerHTML <- "<strong>" + thing.fThing + "</strong>, " + (string thing.fRating)
                    listEl.appendChild item |> ignore
            })
         with
         | ex -> Globals.console.error("Error when displaying data: ", ex)
    }

    let populatedata(things: #seq<Thing>) = async {
        try
            let db = IndexedDB<ThingDB>()
            do! db.useStoreRW<Thing,_>(fun cs -> async { for t in things do cs.put(t) })
            do! displayData()
        with
        | ex -> Globals.console.error("Error when populating data: ", ex)
    }

    let main() =
        query<HTMLElement>("button").onclickStream
        |> Observable.add (fun ev ->
            ev.preventDefault()
            displayData() |> Async.StartImmediate)

        // Populate the database with initial data
        Thing.CreateDummyData()
        |> populatedata
        |> Async.StartImmediate

// Compile the code to JS and copy the html file and the generated script to the parent directory
open System.IO
let dir = __SOURCE_DIRECTORY__
let components = FunScript.HTML.Components.getHTMLComponents()
let code = FunScript.Compiler.Compiler.Compile(<@ Program.main() @>, noReturn=true, components=components)
File.WriteAllText(Path.Combine(dir, "../app.js"), code)
File.Copy(Path.Combine(dir, "index.html"), Path.Combine(dir, "../index.html"), overwrite=true)