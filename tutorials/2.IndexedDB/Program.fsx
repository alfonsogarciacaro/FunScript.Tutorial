#I "../../lib"
#r "FunScript.dll"
#r "FunScript.HTML.dll"
#r "FunScript.Interop.dll"
#r "FunScript.TypeScript.Binding.lib.dll"

// Adapted from the IDBIndex example in MDN: mdn.github.io/IDBIndex-example

[<ReflectedDefinition>]
module Program =
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
                // Note that the compiler will complain if the property returns a different type than 'TKey
                let store = db.createStore<Contact,int>(KeyPath <@ fun c -> c.id @>)

                // If we prefer to generate keys automatically, we write the following: 
                // let store = db.createStore<Contact,_>(AutoIncrement)

                // Now we create the indices, using an expression here too and setting the unique constraint
                store.createIndex(<@ fun c -> c.lName @>, unique=false)
                store.createIndex(<@ fun c -> c.fName @>, unique=false)
                store.createIndex(<@ fun c -> c.jTitle @>, unique=false)
                store.createIndex(<@ fun c -> c.company @>, unique=false)
                store.createIndex(<@ fun c -> c.eMail @>, unique=true)
                store.createIndex(<@ fun c -> c.phone @>, unique=false)
                store.createIndex(<@ fun c -> c.phone @>, unique=false)



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

    let displayDataByKey() = async {
        let tableEntry: HTMLElement = unbox(Globals.document.querySelector "tbody")
        tableEntry.innerHTML <- ""
        let db = IndexedDB<ContactDB>()
        do! db.useStore<Contact,_>(fun cs -> async {
            let! cursor = cs.openCursorAsync()
            for contact in cursor do
                tableEntry.appendChild(createRow contact) |> ignore
        })
    }

    let displayDataByIndex(activeIndex) = async {
        let tableEntry: HTMLElement = unbox(Globals.document.querySelector "tbody")
        tableEntry.innerHTML <- ""
        let db = IndexedDB<ContactDB>()
        do! db.useStore<Contact,_>(fun cs -> async {
            let myIndex = cs.indexByName(activeIndex)
            let! cursor = myIndex.openCursorAsync()
            for contact in cursor do
                tableEntry.appendChild(createRow contact) |> ignore
        })
    }

    let populatedata(contacts: #seq<Contact>) = async {
        let db = IndexedDB<ContactDB>()
        do! db.useStoreRW<Contact,_>(fun cs -> async {
            for contact in contacts do
                cs.put(contact)
        })
        do! displayDataByKey()
    }

    let main() =
        // Add events
        let thControls: HTMLElement[] = unbox(Globals.document.querySelectorAll "th")
        for activeThead in thControls do
            activeThead.onclickStream
            |> Observable.add (fun e ->
                let activeIndex: HTMLElement = unbox e.target
                if activeIndex.innerHTML = "ID" then
                    Async.StartImmediate(displayDataByKey())
                else
                    let indexName =
                        match activeIndex.innerHTML with
                        | "Last name" -> "lName"
                        | "First name" -> "fName"
                        | "Job title" -> "jTitle"
                        | "Company" -> "company"
                        | "E-mail" -> "eMail"
                        | "Phone" -> "phone"
                        | "Age" -> "age"
                        | _ -> failwith "unrecognized header"
                    Async.StartImmediate(displayDataByIndex(indexName)))

        // Populate the database with initial data
        Contact.CreateDummyData()
        |> populatedata
        |> Async.StartImmediate

open System.IO
let dir = __SOURCE_DIRECTORY__
let code = FunScript.Compiler.compileWithoutReturn <@ Program.main() @>
File.WriteAllText(Path.Combine(dir, "../app.js"), code)
File.Copy(Path.Combine(dir, "index.html"), Path.Combine(dir, "../index.html"), overwrite=true)