open System
//module Rx = FSharp.Control.Reactive.Observable
open TurbineMock

let test(logger: Logger.Simple) =
    logger.LogLine "Starting simulation..."
    let turbines =
        [new Turbine("#1-10",1333.,logger)
         new Turbine("#10-20",2555.,logger)]
    logger.LogLine "Starting turbines..."
    turbines
    |> List.map(fun t -> 
           t.dataStream
           |> Observable.subscribe
                  (fun x -> sprintf "Received: %A from %A" x t.Name |> logger.LogLine)
           |> ignore
           t.avgDataStream
           |> Observable.subscribe(fun x -> sprintf "AVG: %A from %A" x t.Name |> logger.LogLine)
           |> ignore
           t)
    |> List.map(fun t -> t.Start())
    |> List.length
    |> sprintf "Succesfully started %i turbines."
    |> logger.LogLine
    turbines

[<EntryPoint>]
let main argv =
    let logger = Logger.Simple()
    let resourses = test(logger)
    Console.ReadLine() |> ignore
    resourses |> List.iter(fun t -> (t :> IDisposable).Dispose())
    Console.ReadLine() |> ignore
    0 // return an integer exit code
