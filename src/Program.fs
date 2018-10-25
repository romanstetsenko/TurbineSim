// Learn more about F# at http://fsharp.org
open System

type State =
    | Idle
    | Working

type countMsg =
    | Start
    | Stop
    | GetState of AsyncReplyChannel<State>

type Turbine(name, interval) =
    let timer = new Timers.Timer(interval)
    do timer.Enabled <- true
    
    let innerTurbine =
        MailboxProcessor.Start(fun inbox -> 
            let rec loop state =
                async {
                    let! msg = inbox.Receive()
                    match msg with
                    | Stop -> 
                        timer.Stop()
                        return! loop Idle
                    | Start -> 
                        printfn "Turbine: %s started." name
                        timer.Start()
                        return! loop Working
                    | GetState(reply) -> 
                        reply.Reply(state)
                        return! loop state
                }
            loop Idle)
    
    do printfn "Turbine: %s created." name
    
    member this.Start() =
        innerTurbine.Post(Start)
        this
    
    member this.Stop() =
        innerTurbine.Post(Stop)
        this
    
    member _this.Fetch() =
        innerTurbine.PostAndReply((fun reply -> GetState(reply)),timeout = 200)
    member _this.Sub = timer.Elapsed
    member _this.Name = name

[<EntryPoint>]
let main argv =
    printfn "Starting simulation..."
    let turbines =
        [Turbine("#1-10", 1333.)
         Turbine("#10-20", 1999.) ]
    printfn "Starting turbines..."
    turbines
    |> List.map(fun t -> t.Start())
    |> List.map(fun t -> t.Sub.Add(fun x -> printfn "Received: %A from %A" x.SignalTime t.Name ))
    |> List.length
    |> printfn "Succesfully started %i turbines."
    
   
    Console.ReadLine() |> ignore
    0 // return an integer exit code
