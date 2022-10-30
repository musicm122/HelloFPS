namespace CoreFS.Services

open System.Threading.Tasks
open Common.Interfaces
open CoreFS.Events
open Godot
open Common
open CoreFS.Services.ThirdParty.Dialogic

type DialogManager() =
    inherit Node()

    member this.PlayerInteractingCompleted =
        DialogEvents.DialogInteractionComplete.Publish

    member this.PauseEvent =
        PauseEvents.Pause.Publish

    member this.UnpauseEvent =
        PauseEvents.Unpause.Publish

    member this.OnDialogListener(arg: System.Object) =
        GD.Print("OnDialogListener called with " + arg.ToString())

    member this.DialogListener(listenerArg: System.Object) =
        let me = this :> IDialogManager
        me.DialogListener(listenerArg)

    member this.DialogCompleted() =
        let me = this :> IDialogManager
        me.DialogComplete()

    interface IPauseable with
        member this.Pause() = PauseEvents.Pause.Trigger()
        member this.Unpause() = PauseEvents.Pause.Trigger()

    interface IDialogManager with
        member this.PauseForCompletion seconds =
            let runner =
                (task { do! this.WaitForSeconds seconds })

            runner.ConfigureAwait(false) |> ignore
            runner

        member this.DialogListener(listenerArg: System.Object) =
            let me = this :> IDialogManager
            (this :> IPauseable).Pause()
            this.OnDialogListener(listenerArg)

            Task.Run(fun () -> me.DialogComplete() |> Async.AwaitTask)
            |> ignore

        member this.StartDialog (owner: Node) (dialogArg: DialogArg) =
            try
                let iDialogManager = this :> IDialogManager

                GD.Print(
                    "StartDialog called with args"
                    + dialogArg.ToString()
                )

                DialogEvents.DialogInteractionStart.Trigger()
                let dialog = DialogicSharp.Start(dialogArg.timeline)
                let result = dialog.Connect(Timeline_End.AsString(), owner, dialogArg.methodName)

                match result with
                | Error.Ok -> owner.AddChild(dialog)
                | _ ->
                    GD.PrintErr(
                        "StartDialog failed with args"
                        + dialogArg.ToString()
                    )
            with
            | ex ->
                GD.PrintErr(
                    "StartDialog failed with args"
                    + dialogArg.ToString()
                )

                failwith "StartDialog failed. Check error log for details."


        member this.DialogComplete() : Task =
            let me = this :> IDialogManager
            let waitTime = 0.2f

            GD.Print("DialogManager.DialogComplete called")
            DialogEvents.DialogInteractionComplete.Trigger()

            task {
                do! me.PauseForCompletion waitTime
                do! me.DialogComplete()
            }
