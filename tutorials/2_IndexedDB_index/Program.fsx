#I "../../lib"
#r "FunScript.dll"
#r "FunScript.HTML.dll"
#r "FunScript.Interop.dll"
#r "FunScript.TypeScript.Binding.lib.dll"

// Adapted from the IDBIndex example in MDN: mdn.github.io/IDBIndex-example

[<ReflectedDefinition>]
module Program =
    open Microsoft.FSharp.Quotations
    open FunScript.TypeScript
    open FunScript.HTML

    // This module contains the types for interacting with HTML5 IndexedDB API
    // in a type-safe and async-workflow compatible way
    open FunScript.HTML.Storage

    // Define the type we'll use to create the Object Store
    // The store will have the name of the type
    type Contact =
        {
        id: int
        fName: string
        lName: string
        jTitle: string
        company: string
        eMail: string
        phone: string
        age: int
        }
        // Create somme dummy data to start with
        static member CreateDummyData() = [|
          { id= 1; fName= "Brian"; lName= "Damage"; jTitle= "Master of Synergies"; company= "Acme"; eMail= "brian@acme.com"; phone= "+441210000000"; age= 37 };
          { id= 2; fName= "Ted"; lName= "Maul"; jTitle= "Chief Reporter"; company= "Brass eye"; eMail= "ted@itsthenews.co.uk"; phone= "+442081111111"; age= 46 };
          { id= 3; fName= "Mr"; lName= "Bungle"; jTitle= "Bad Clown"; company= "Stub a Dub"; eMail= "bungle@maiof.com"; phone= "+1508888888"; age= 50 };
          { id= 4; fName= "Richard"; lName= "James"; jTitle= "Sound Engineer"; company= "Aphex Twin"; eMail= "richard@drukqs.com"; phone= "+1517777777"; age= 43 };
          { id= 5; fName= "Brian"; lName= "Umlaut"; jTitle= "Shredmeister"; company= "Minions of metal"; eMail= "brian@raiseyourhorns.com"; phone= "+14086666666"; age= 40 };
          { id= 6; fName= "Jonathan"; lName= "Crane"; jTitle= "Freelance Psychologist"; company= "Arkham"; eMail= "jon@arkham.com"; phone= "n/a"; age= 38 };
          { id= 7; fName= "Julian"; lName= "Day"; jTitle= "Schedule Keeper"; company= "Arkham"; eMail= "julian@arkham.com"; phone= "n/a"; age= 43 };
          { id= 8; fName= "Bolivar"; lName= "Trask"; jTitle= "Head of R&D"; company= "Trask"; eMail= "bolivar@trask.com"; phone= "+14095555555"; age= 55 };
          { id= 9; fName= "Cloud"; lName= "Strife"; jTitle= "Weapons Instructor"; company= "Avalanche"; eMail= "cloud@avalanche.com"; phone= "+17083333333"; age= 24 };
          { id= 10; fName= "Bilbo"; lName= "Bagshot"; jTitle= "Comic Shop Owner"; company= "Fantasy Bazaar"; eMail= "bilbo@fantasybazaar.co.uk"; phone= "+12084444444"; age= 43 }
        |]

    // Define a type with default constructor and implementing the DBImplementation interface
    // The name of this type will be used to identify the database
    type ContactDB() =
        interface DBImplementation with
            // The version number must be an unsigned integer
            member x.Version with get() = 1u

            // When we open the database, if it doesn't exists or it does with an inferior
            // version number, this method will be triggered
            member x.Upgrade(db) =
                // We create a store using a property of the object as the primary key (must be unique)
                // We use an expression with a lambda to define the key in a type-safe way
                // Note that the compiler will complain if the property returns a type different from 'TKey
                let store = db.createStore<Contact,int>(KeyPath <@ fun c -> c.id @>)

                // If we prefer to generate keys automatically, we write the following: 
                // let store = db.createStore<Contact,_>(AutoIncrement)

                // Now create the indices too using expressions and setting the unique constraint if needed
                store.createIndex(<@ fun c -> c.lName @>)
                store.createIndex(<@ fun c -> c.fName @>)
                store.createIndex(<@ fun c -> c.jTitle @>)
                store.createIndex(<@ fun c -> c.company @>)
                store.createIndex(<@ fun c -> c.eMail @>, unique=true)
                store.createIndex(<@ fun c -> c.phone @>)
                store.createIndex(<@ fun c -> c.age @>)


    let createRow(contact: Contact) =
        let tableRow = Globals.document.createElement("tr")
        tableRow.innerHTML <-  "<td>" + (string contact.id) + "</td>"
                             + "<td>" + contact.lName + "</td>"
                             + "<td>" + contact.fName + "</td>"
                             + "<td>" + contact.jTitle + "</td>"
                             + "<td>" + contact.company + "</td>"
                             + "<td>" + contact.eMail + "</td>"
                             + "<td>" + contact.phone + "</td>"
                             + "<td>" + (string contact.age) + "</td>"
        tableRow

    let displayData(activeIndex: Expr option) = async {
        try
            let tableEntry: HTMLElement = unbox(Globals.document.querySelector "tbody")
            tableEntry.innerHTML <- ""

            // Create an instance of Storage.IndexedDB passing the type defined above
            let db = IndexedDB<ContactDB>()

            // Open the database passing the types of the stores (up to four) we want to open
            // and the type of the transaction result if necessary. As an argmument we pass
            // a nested async function that will be executed during the transaction
            do! db.useStore<Contact,_>(fun cs -> async {
                let cursor =
                    match activeIndex with
                    // If no index is passed, open a cursor using the primary key
                    | None -> cs.openCursor()
                    // As we want to accept indices of different types, in this case
                    // we use .indexUnsafe which accepts an untyped expression
                    | Some expr -> cs.indexUnsafe(expr).openCursor()
                
                // The cursor is an AsyncSeq (see http://tomasp.net/blog/async-sequences.aspx/)
                // We can iterate with a simple for loop
                for contact in cursor do
                    tableEntry.appendChild(createRow contact) |> ignore

// For more complex queries, open FSharp.Control and use the functions in the AsyncSeq module
//                do! cursor
//                    |> AsyncSeq.map (fun contact -> createRow contact)
//                    |> AsyncSeq.iter (fun row -> tableEntry.appendChild row |> ignore)
            })

// If we want to open more stores or return a result we would write something like
//            let! result = db.useStore<Customer, Order, DateTime>(fun c o -> async {
//                let! customer = c.getAsync(customerID)
//                let! order = o.getAsync(customer.orderID)
//                return order.dueDate
//            })
         with
         | ex -> Globals.console.error("Error when displaying data: ", ex)
    }

    let populatedata(contacts: #seq<Contact>) = async {
        try
            let db = IndexedDB<ContactDB>()

            // To write data use the RW (read-write) method instead
            do! db.useStoreRW<Contact,_>(fun cs -> async {
                // We don't use putAsync here because we are just interested
                // in the moment when the whole transaction finishes
                for contact in contacts do
                    cs.put(contact)
            })
            do! displayData(None)
        with
        | ex -> Globals.console.error("Error when populating data: ", ex)
    }

    let main() =
        // Add events
        let thControls: HTMLElement[] = unbox(Globals.document.querySelectorAll "th")
        for activeThead in thControls do
            activeThead.onclickStream
            |> Observable.add (fun e ->
                let activeIndex: HTMLElement = unbox e.target
                if activeIndex.innerHTML = "ID" then
                    Async.StartImmediate(displayData(None))
                else
                    // Because we have indices with different types here
                    // we use an untyped expression (see .indexUnsafe above)
                    let index =
                        match activeIndex.innerHTML with
                        | "Last name" ->    <@@ fun c -> c.lName @@>
                        | "First name" ->   <@@ fun c -> c.fName @@>
                        | "Job title" ->    <@@ fun c -> c.jTitle @@>
                        | "Company" ->      <@@ fun c -> c.company @@>
                        | "E-mail" ->       <@@ fun c -> c.eMail @@>
                        | "Phone" ->        <@@ fun c -> c.phone @@>
                        | "Age" ->          <@@ fun c -> c.age @@>
                        | _ -> failwith "unrecognized header"
                    Async.StartImmediate(displayData(Some index)))
        
        // Populate the database with initial data
        Contact.CreateDummyData()
        |> populatedata
        |> Async.StartImmediate

// Compile the code to JS and copy the html file and the genereated script to the parent directory
open System.IO
let dir = __SOURCE_DIRECTORY__
let components = FunScript.HTML.Components.getHTMLComponents()
let code = FunScript.Compiler.Compiler.Compile(<@ Program.main() @>, noReturn=true, components=components)
File.WriteAllText(Path.Combine(dir, "../app.js"), code)
File.Copy(Path.Combine(dir, "index.html"), Path.Combine(dir, "../index.html"), overwrite=true)