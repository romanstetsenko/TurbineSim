module Logger

type Logger  =
    let innerLogger = MailboxProcessor.Start(fun inbox ->
        )

