open System

//module Rx = FSharp.Control.Reactive.Observable
open TurbineMock

let test () = 
    printfn "Starting simulation..."
    let turbines =
        [new Turbine("#1-10",1333.)
         new Turbine("#10-20",2555.)]
    printfn "Starting turbines..."
    turbines
    |> List.map(fun t -> 
           t.dataStream
           |> Observable.subscribe
                  (fun x -> printfn "Received: %A from %A" x t.Name)
           |> ignore
           t.avgDataStream
           |> Observable.subscribe(fun x -> printfn "AVG: %A from %A" x t.Name)
           |> ignore
           t)
    |> List.map(fun t -> t.Start())
    |> List.length
    |> printfn "Succesfully started %i turbines."   
    turbines

[<EntryPoint>]
let main argv =
    let resourses = test ()
    Console.ReadLine() |> ignore
    resourses |> List.iter (fun t -> (t :> IDisposable).Dispose() )
    Console.ReadLine() |> ignore
    0 // return an integer exit code
