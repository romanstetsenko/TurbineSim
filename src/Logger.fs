module Logger

open System

type Simple() =
    let innerLogger =
        MailboxProcessor.Start(fun inbox -> 
            let rec loop () =
                async {
                    let! (msg: string) = inbox.Receive()
                    Console.WriteLine(msg)
                    return! loop ()
                }
            loop ())
    
    member _this.LogLine x = innerLogger.Post(x)
