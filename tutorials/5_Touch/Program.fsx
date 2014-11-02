#I "../../lib"
#r "FunScript.dll"
#r "FunScript.Interop.dll"
#r "FunScript.TypeScript.Binding.lib.dll"
#r "FunScript.HTML.dll"

// Adapted from http://mobiforge.com/design-development/html5-mobile-web-touch-events

module Literals =
    let [<Literal>] timeLapse = 50.

[<ReflectedDefinition>]
module Program =
    open System
    open System.Collections.Generic
    open FunScript.TypeScript
    open FunScript.HTML

    type TStart = { startX: float; startY: float; time: DateTime }
    type TMove  = { startX: float; startY: float; endX: float; endY: float; time: DateTime; isDrawn: bool ref }

    type TouchPosition =
        | TStart of TStart
        | TMove  of TMove

    type MousePosition =
        | MStart of (float*float*string)
        | MMove  of (float*float*float*float*string)
        | MIdle

    let main() =
        let colors = [|"red"; "green"; "blue"; "yellow"; "black"|]
        let canvas = Globals.document.getElementsByTagName_canvas().[0]
        let ctx: CanvasRenderingContext2D = unbox(canvas.getContext "2d")

        canvas.ontouchStream
        |> Observable.scan (fun (positions: Dictionary<_,_>) ev ->
            match ev with
            | TouchStart e ->
                for i = 0 to e.changedTouches.Length - 1 do
                    let touch = e.changedTouches.[i]
                    positions.Add(touch.identifier, TStart {startX=touch.pageX; startY=touch.pageY; time=DateTime.Now})
            | TouchMove e ->
                for i = 0 to e.changedTouches.Length - 1 do
                    let touch = e.changedTouches.[i]
                    if positions.ContainsKey(touch.identifier) then
                        positions.[touch.identifier] <-
                            match positions.[touch.identifier] with
                            | TStart { startX = x; startY = y; time = dt }
                            | TMove  { endX = x;   endY = y;   time = dt } ->
                                // Not an ideal solution, but keep some time lapse to prevent too much jiggling
                                if (DateTime.Now - dt).TotalMilliseconds < Literals.timeLapse
                                then positions.[touch.identifier]
                                else TMove {startX=x; startY=y; endX=touch.pageX; endY=touch.pageY; time=DateTime.Now; isDrawn=ref false}
            | TouchCancel e
            | TouchEnd e ->
                for i = 0 to e.changedTouches.Length - 1 do
                    positions.Remove(e.changedTouches.[i].identifier) |> ignore
            positions) (Dictionary<float, TouchPosition>())
        |> Observable.add (fun positions ->
            for pt in positions do
                match pt.Value with
                | TMove {startX=startX; startY=startY; endX=endX; endY=endY; isDrawn=isDrawn} when !isDrawn = false ->
                    ctx.beginPath()
                    ctx.moveTo(startX, startY)
                    ctx.lineTo(endX, endY)
                    ctx.strokeStyle <- colors.[int pt.Key]
                    ctx.stroke()
                | _ -> ())

        // Deal also with no-touching devices
        canvas.onmouseStream
        |> Observable.scan (fun pos case ->
            match case with
            | MouseDown ev ->
                let rnd = Globals.Math.random() * 5. |> floor
                MStart (ev.offsetX, ev.offsetY, colors.[int rnd])
            | MouseMove ev ->
                match pos with
                | MStart (x, y, color)
                | MMove (_, _, x, y, color) -> MMove (x, y, ev.offsetX, ev.offsetY, color)
                | MIdle -> MIdle
            | _ -> MIdle) MIdle
        |> Observable.add (function
            | MMove(startX, startY, endX, endY, color) ->
                ctx.beginPath()
                ctx.moveTo(startX, startY)
                ctx.lineTo(endX, endY)
                ctx.strokeStyle <- color
                ctx.stroke()
            | _ -> ())

open System.IO
let dir = __SOURCE_DIRECTORY__
let code = FunScript.Compiler.Compiler.Compile(<@ Program.main() @>, noReturn=true, components=FunScript.HTML.Components.getHTMLComponents())
File.WriteAllText(Path.Combine(dir, "../app.js"), code)
File.Copy(Path.Combine(dir, "index.html"), Path.Combine(dir, "../index.html"), overwrite=true)