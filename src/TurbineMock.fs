module TurbineMock
open System

type State =
    | Idle
    | Working

type countMsg =
    | Start
    | Stop
    | GetState of AsyncReplyChannel<State>

type Turbine(name,interval) =
    let timer = new Timers.Timer(interval)
    let innerTurbine =
        MailboxProcessor.Start(fun inbox -> 
            let rec loop state =
                async {
                    let! msg = inbox.Receive()
                    match msg with
                    | Stop -> 
                        printfn "Turbine: %s stopped." name
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
    member _this.GetState() =
        innerTurbine.PostAndReply((fun reply -> GetState(reply)),timeout = 200)
    member _this.Name = name
    member _this.dataStream =
        timer.Elapsed 
        |> Observable.map(fun t -> t.SignalTime.Ticks % 100L |> int)
    member this.avgDataStream =
        this.dataStream
        |> Observable.scan (fun acc x -> x :: acc |> List.truncate 5) []
        |> Observable.map(List.map float >> List.average)
    interface System.IDisposable with 
        member this.Dispose() = this.Stop() |> ignore
