// First include references to the libraries we'll be using
#I "../../lib"
#r "FunScript.dll"
#r "FunScript.HTML.dll"
#r "FunScript.Interop.dll"
#r "FunScript.TypeScript.Binding.lib.dll"

#I "./lib"
#r "FunScript.TypeScript.Binding.three.dll" // Generated from the standard three.js typescript def 
#r "FunScript.TypeScript.Binding.threex.dll" // Generated from threex.d.ts in this project

open FunScript
open FunScript.TypeScript

// You must always mark the code you want to compile to JavaScript
// with the ReflectedDefinition attribute. This will ask the F# compiler
// to create the expression tree that FunScript will read and compile to JS.
// Alternatively, you can use the FunScript.JSAttribute alias.
[<ReflectedDefinition>]
module Program =

    // Helper function to invoke the js ~ operator
    [<JS; JSEmit("return ~ {0} ")>]
    let jsNot (a:'a) = failwith "never"

    open FunScript.TypeScript

    // Ported from: http://threejs.org/examples/#webgl_geometry_terrain
    // Note: this sample is intended to be a direct port of the original, and doesn't attempt to refactor the original 
    // to be more "functional".
    let main() =
        let worldWidth = 256
        let worldDepth = 256
        let worldHalfWidth = worldWidth / 2
        let worldHalfDepth = worldDepth / 2

        let clock = THREE.Clock.Create()

        let rand() = 
            Globals.Math.random() 

        let generateHeight width height =
            let size = width * height
            // Beware, "Array.zeroCreate size" will create an array filled with "undefined" values, not "0" values.
            // It is probably an issue with the the current FunScript compiler.
            let data:float [] = Array.init size (fun x -> 0.0) 
            let perlin = ImprovedNoise.Create()
            let mutable quality = 1.0
            let z = rand() * 100.0

            for j in [0..3] do
             // Would like to use a [0..size - 1] range here, but the compiler will generate a series of functions for the enumerator
             // and with a large sequence like this, the JS stack explodes.  So use a good ol' while loop.
             let mutable i = 0
             while i < size do
                let x = i % width
                // I presume double-not is used here for this reason: http://james.padolsey.com/javascript/double-bitwise-not/
                // Note, since we use a custom jsNot function to invoke the operator, parens (or pipes) are required to enforce the
                // proper association (that is, "jsNot jsNot (i /width)" would not produce the correct result).
                let y = jsNot (jsNot ( i / width ))

                let noise = perlin.noise(float x / quality, float y / quality, z ) * quality * 1.75
                data.[i] <- data.[i] + (Globals.Math.abs ( noise ))
                i <- i + 1

             quality <- quality * 5.0
            data

        let generateTexture (data:float []) (width:float) (height:float) = 
            let vector3 = THREE.Vector3.Create(0.0, 0.0, 0.0)
            let sun = 
                THREE.Vector3.Create(1.0, 1.0, 1.0)
                    .normalize()

            let canvas = Globals.document.createElement_canvas()
            canvas.width <- width
            canvas.height <- height

            let context = canvas.getContext_2d()
            context.fillStyle <- "#000"
            context.fillRect(0.0, 0.0, width, height)

            let image = context.getImageData( 0.0, 0.0, canvas.width, canvas.height)
            let imageData = image.data

            let mutable i = 0
            let mutable j = 0
            let mutable l = imageData.Length

            while i < l do
                // Note, data elements -2 and -1 are accessed here at the start of the loop.  
                // Its a bug in the original (producing NaN after normalize()), but javascript
                // just keeps on truckin'.  There is a similar issue with z.  The result is 
                // that imageData is set to zero (black) in these cases
                vector3.x <- data.[ j - 2 ] - data.[ j + 2 ]
                vector3.y <- 2.0
                vector3.z <- data.[ j - int width * 2 ] - data.[ j + int width * 2 ]
                vector3.normalize() |> ignore

                let shade = vector3.dot(sun)

                imageData.[ i ] <- ( 96.0 + shade * 128.0 ) * ( 0.5 + data.[ j ] * 0.007 )
                imageData.[ i + 1 ] <- ( 32.0 + shade * 96.0 ) * ( 0.5 + data.[ j ] * 0.007 )
                imageData.[ i + 2 ] <- ( shade * 96.0 ) * ( 0.5 + data.[ j ] * 0.007 )

                i <- i + 4
                j <- j + 1

            context.putImageData( image, 0.0, 0.0 );

            let canvasScaled = Globals.document.createElement_canvas()
            canvasScaled.width <- width * 4.0
            canvasScaled.height <- height * 4.0

            let context = canvasScaled.getContext_2d()
            context.scale(4.0,4.0)
            context.drawImage(canvas, 0.0, 0.0)

            let image = context.getImageData( 0.0, 0.0, canvasScaled.width, canvasScaled.height )
            let imageData = image.data

            let mutable i = 0
            let mutable l = imageData.Length
            while i < l do
                let v = jsNot (jsNot (rand() * 5.0))
                imageData.[ i ] <- imageData.[ i ] + v
                imageData.[ i + 1 ] <- imageData.[ i + 1 ] + v
                imageData.[ i + 2 ] <- imageData.[ i + 2 ] + v
                i <- i + 4

            context.putImageData( image, 0.0, 0.0 )

            canvasScaled

        let init() =
            // bind window just because we use it all over the place.
            let wnd = Globals.window
            let container = Globals.document.getElementById("container")
            let camera = THREE.PerspectiveCamera.Create(60.0, wnd.innerWidth / wnd.innerHeight, 1.0, 20000.0 )
            let scene = THREE.Scene.Create() :?> THREE.Scene

            // THREE.FirstPersonControls is not a standard part of THREE; the included "threex.d.ts" defines an interface
            // for it.  Same goes for the perlin noise library.
            // The FunScript.TypeScript library can be used to generate an assembly for custom type defs (albeit with a little work)
            let controls = THREE.FirstPersonControls.Create(camera :?> THREE.Camera)
            controls.movementSpeed <- 1000.0
            controls.lookSpeed <- 0.1

            let data = generateHeight worldWidth worldDepth

            camera.position.y <- float (int data.[worldHalfWidth + worldHalfDepth * worldWidth] * 10 + 500)

            let geometry = THREE.PlaneBufferGeometry.Create(7500.0, 7500.0, float (worldWidth - 1), float (worldDepth - 1))
            geometry.applyMatrix(THREE.Matrix4.Create().makeRotationX( - Globals.Math.PI  / 2.0 ))

            let vertices = geometry.getAttribute("position") :?> THREE.BufferAttribute
            let vertices = vertices.array
            let l = vertices.Length
            let mutable i = 0
            let mutable j = 0
            while i < l do
                vertices.[j + 1] <- data.[i] * 10.0
                i <- i + 1
                j <- j + 3

            let texCanvas = generateTexture data (float worldWidth) (float worldDepth )
            let texture = THREE.Texture.CreateOverload2(texCanvas, THREE.Globals.UVMapping, THREE.Globals.ClampToEdgeWrapping, THREE.Globals.ClampToEdgeWrapping)
            texture.needsUpdate <- true

            // We use createEmpty here to create an empty object used to set configuration parameters.
            // The type qualifier indicates what fields we will be able to set on the resulting object.
            // For those fields that are enum types, the possible values are usually found in THREE.Globals
            let matProps = createEmpty<THREE.MeshBasicMaterialParameters>()
            matProps.map <- texture

            let mesh = THREE.Mesh.Create((geometry :?> THREE.Geometry),  THREE.MeshBasicMaterial.Create(matProps))
            scene.add mesh

            let renderer = THREE.WebGLRenderer.Create()
            renderer.setClearColor("#bfd1e5")
            renderer.setSize( wnd.innerWidth, wnd.innerHeight )
            container.innerHTML <- ""
            container.appendChild (renderer.domElement) |> ignore

            let onWindowResize(e:UIEvent):obj =
                let wnd = Globals.window
                camera.aspect <- wnd.innerWidth / wnd.innerHeight
                camera.updateProjectionMatrix()
                renderer.setSize(wnd.innerWidth, wnd.innerHeight)
                controls.handleResize() |> ignore 
                null

            Globals.addEventListener_resize(new System.Func<UIEvent,obj>(onWindowResize), false)

            renderer, scene, camera, controls

        let renderer,scene,camera,controls = init()

        let render() =
            controls.update(clock.getDelta()) |> ignore // I probably should have specified that this returns void in the threex.d.ts file
            renderer.render(scene, camera)

        let rec animate (dt:float) =
            Globals.requestAnimationFrame(new FrameRequestCallbackDelegate(animate)) |> ignore
            render()

        // kick it off
        animate(0.0)

// This will compile the code to JS and copy the html file and the generated script to the parent directory
open System.IO
let dir = __SOURCE_DIRECTORY__
// External libraries can provide additional components to FunScript compiler
// In most of the tutorials we'll be using components from FunScript.HTML extensions
let components = FunScript.HTML.Components.getHTMLComponents()
let code = FunScript.Compiler.Compiler.Compile(<@ Program.main() @>, noReturn=true, components=components)
File.WriteAllText(Path.Combine(dir, "../app.js"), code)
File.Copy(Path.Combine(dir, "index.html"), Path.Combine(dir, "../index.html"), overwrite=true)