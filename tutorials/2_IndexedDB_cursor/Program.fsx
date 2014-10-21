#I "../../lib"
#r "FunScript.dll"
#r "FunScript.HTML.dll"
#r "FunScript.Interop.dll"
#r "FunScript.TypeScript.Binding.lib.dll"

// Adapted from the IDBIndex example in MDN: mdn.github.io/IDBcursor-example
// For more detailed comments, see the IndexedDB_index example

[<ReflectedDefinition>]
module Program =
    open FSharp.Control
    open Microsoft.FSharp.Quotations
    open FunScript.TypeScript
    open FunScript.HTML
    open FunScript.HTML.Storage

    // Define the type we'll use to create the Object Store
    // The store will have the name of the type
    type Record = 
        {
            albumTitle: string
            year: int
        }
        static member CreateDummyData() = [|
          { albumTitle= "Power windows"; year= 1985 }
          { albumTitle= "Grace under pressure"; year= 1984 }
          { albumTitle= "Signals"; year= 1982 }
          { albumTitle= "Moving pictures"; year= 1981 }
          { albumTitle= "Permanent waves"; year= 1980 }
          { albumTitle= "Hemispheres"; year= 1978 }
          { albumTitle= "A farewell to kings"; year= 1977 }
          { albumTitle= "2112"; year= 1976 }
          { albumTitle= "Caress of steel"; year= 1975 }
          { albumTitle= "Fly by night"; year= 1975 }
          { albumTitle= "Rush"; year= 1974 }
        |]

    // Define a type with default constructor and implementing the DBImplementation interface
    // The name of this type will be used to identify the database
    // When we open the database, if it doesn't exists or it does with an inferior version number,
    // Upgrade will be triggered
    type RecordDB() =
        interface DBImplementation with
            member x.Version with get() = 1u
            member x.Upgrade(db) =
                let store = db.createStore<Record,string> (KeyPath <@ fun r -> r.albumTitle @>)
                store.createIndex <@ fun r -> r.year @>

    let displayData(cursorGetter: DBStore<Record>->AsyncSeq<Record>) = async {
        try
            let listEl: HTMLElement = unbox(Globals.document.querySelector "ul")
            listEl.innerHTML <- ""
            let db = IndexedDB<RecordDB>()
            do! db.useStore<Record,_>(fun cs -> async {
                let cursor = cursorGetter(cs)
                for record in cursor do
                    let item = Globals.document.createElement("li")
                    item.innerHTML <- "<strong>" + record.albumTitle + "</strong>, " + (string record.year)
                    listEl.appendChild item |> ignore
            })
         with
         | ex -> Globals.console.error("Error when displaying data: ", ex)
    }

    let populatedata(records: #seq<Record>) = async {
        try
            let db = IndexedDB<RecordDB>()
            do! db.useStoreRW<Record,_>(fun cs -> async { for r in records do cs.put(r) })
            do! displayData(fun cs -> cs.openCursor())
        with
        | ex -> Globals.console.error("Error when populating data: ", ex)
    }

    let main() =
        // Add events
        unbox<HTMLElement>(Globals.document.querySelector ".continue").onclickStream
        |> Observable.add (fun _ -> displayData(fun cs -> cs.openCursor()) |> Async.StartImmediate)

        unbox<HTMLElement>(Globals.document.querySelector ".advance").onclickStream
        |> Observable.add (fun _ -> displayData(fun cs -> cs.openCursor(step=2u)) |> Async.StartImmediate)

        unbox<HTMLElement>(Globals.document.querySelector ".direction").onclickStream
        |> Observable.add (fun _ -> displayData(fun cs -> cs.openCursor(direction=DBCursorDirection.Prev)) |> Async.StartImmediate)

        unbox<HTMLElement>(Globals.document.querySelector ".delete").onclickStream
        |> Observable.add (fun _ ->
            let ops =
                async { let db = IndexedDB<RecordDB>()
                        do! db.useStoreRW<Record,_>(fun cs -> async { 
                            do! cs.deleteAsync("Grace under pressure")   
                            Globals.console.log("Deleted that mediocre album from 1984. Even Power windows is better.") })
                        do! displayData(fun cs -> cs.openCursor()) }
            Async.StartImmediate ops)

        unbox<HTMLElement>(Globals.document.querySelector ".update").onclickStream
        |> Observable.add (fun _ ->
            let ops =
                async { let db = IndexedDB<RecordDB>()
                        do! db.useStoreRW<Record,_>(fun cs -> async { 
                            let! key = cs.putAsync({ albumTitle="A farewell to kings"; year=2050 })
                            Globals.console.log("A better album year?") })
                        do! displayData(fun cs -> cs.openCursor()) }
            Async.StartImmediate ops)

        // Populate the database with initial data
        Record.CreateDummyData()
        |> populatedata
        |> Async.StartImmediate

// Compile the code to JS and copy the html file and the generated script to the parent directory
open System.IO
let dir = __SOURCE_DIRECTORY__
let components = FunScript.HTML.Components.getHTMLComponents()
let code = FunScript.Compiler.Compiler.Compile(<@ Program.main() @>, noReturn=true, components=components)
File.WriteAllText(Path.Combine(dir, "../app.js"), code)
File.Copy(Path.Combine(dir, "index.html"), Path.Combine(dir, "../index.html"), overwrite=true)