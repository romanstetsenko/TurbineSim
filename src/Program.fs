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
let sample = """

let handlerHigh (t,d)
    {
        if d > 90 
        {
            Logger.LogLine ("Turbine " + t.Name + " overcame avg:" + d)
            let consumer = Consumer.connect "#c1-10"
            consumer.Post "We are at the full power."
            consumer.Post "Trying to stop the secondary turbine: #10-20"
            let t2 = Turbine.connect "#10-20"
            let state = t2.GetState ()
            if state = TurbineState.Working
            {
                let avg2 = t2.GetAverage()
                if avg2 > 10
                {
                    t2.Stop()
                    Logger.LogLine ("The secondary turbine #10-20 stopped.")
                }
            }
        }
    }

let handlerLow (t,d)
    {
        if d < 10 
        {
            Logger.LogLine ("Turbine " + t.Name + " has low avg: " + d)
            Logger.LogLine ("Trying to start the secondary turbine: #10-20")
            let t2 = Turbine.connect "#10-20"
            let state = t2.GetState ()
            if state = TurbineState.Idle
            {
                t1.Start()
                Logger.LogLine ("The secondary turbine #10-20 started.")
            }
        }
    }

let wf1
    {
        let t1 = Turbine.connect "#1-10"
        let state = t1.GetState ()
        if state = TurbineState.Idle
        {
            t1.Start()
        }
        t1.avgDataStream.Subscribe handlerAvg
        t1.avgDataStream.Subscribe handlerHigh
        t1.avgDataStream.Subscribe handlerHigh
        let aConsumer = Consumer.connect "#c1-*"
        
        t1.avgDataStream.SubscribeConsumer aConsumer
    }
"""

[<EntryPoint>]
let main argv =
    let logger = Logger.Simple()
    let resourses = test(logger)
    Console.ReadLine() |> ignore
    resourses |> List.iter(fun t -> (t :> IDisposable).Dispose())
    Console.ReadLine() |> ignore
    0 // return an integer exit code