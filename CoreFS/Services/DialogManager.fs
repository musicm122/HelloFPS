namespace CoreFS.Services

open System.Threading.Tasks
open Common.Interfaces
open Common.Types
open CoreFS.Events
open Godot
open Common
open CoreFS.Services.ThirdParty.Dialogic

type DialogManager() =
    inherit Node()
    static member val private _dialogic =
        ((GD.Load<Script>("res://addons/dialogic/Other/DialogicClass.gd"))) with get, set
    static member val private DEFAULT_DIALOG_RESOURCE = ("res://addons/dialogic/Nodes/DialogNode.tscn") with get, set
    static member StartTimeline(timeline: string):Node =
        //let callResult = DialogManager._dialogic.Call("start", timeline)
        let callResult = DialogManager._dialogic.Call("start", timeline, "", DialogManager.DEFAULT_DIALOG_RESOURCE, false)
        let result = callResult :?> Node
        result
    //private static readonly Script _dialogic = GD.Load<Script>("res://addons/dialogic/Other/DialogicClass.gd");

    member this.PlayerInteractingCompleted =
        DialogEvents.DialogInteractionComplete.Publish

    member this.PauseEvent =
        PauseEvents.Pause.Publish

    member this.UnpauseEvent =
        PauseEvents.Unpause.Publish

    member this.OnDialogListener(arg: System.Object) =
        GD.Print("OnDialogListener called with " + arg.ToString())

    interface IPauseable with
        member this.Pause() = PauseEvents.Pause.Trigger()
        member this.Unpause() = PauseEvents.Pause.Trigger()

    interface IDialogManager with
        member this.PauseForCompletion seconds =
            let runner =
                (task { do! this.WaitForSeconds seconds })

            runner.ConfigureAwait(false) |> ignore
            runner

        member this.StartDialog(dialogArg: DialogArg) =
            GD.Print(
                "StartDialog called with args"
                + dialogArg.ToString()
            )

            //DialogEvents.DialogInteractionStart.Trigger()

            let dialog =
                DialogManager.StartTimeline(dialogArg.timeline)

            // let (signalArg: SignalConnection) =
            //     SignalConnection.Default("dialogic_signal", dialog, (nameof this.OnDialogListener), this)

            //let (signalArg: SignalConnection) = SignalConnection.Default("dialogic_signal", dialog, (nameof this.OnDialogListener), this)
            
            let result = dialog.Connect("dialogic_signal", this, "DialogListener")
            match result with
            | Error.Ok -> this.AddChild(dialog)
            | _ ->
                GD.PrintErr(
                    "StartDialog failed with args"
                    + dialogArg.ToString()
                )

        member this.DialogComplete() : Task =
            let me = this :> IDialogManager
            let waitTime = 0.2f
            GD.Print("DialogManager.DialogComplete called")
            DialogEvents.DialogInteractionComplete.Trigger()
            task { do! me.PauseForCompletion waitTime }

        member this.DialogListener(listenerArg) =
            let me = this :> IDialogManager
            (this :> IPauseable).Pause()
            this.OnDialogListener(listenerArg)

            Task.Run(fun () -> me.DialogComplete() |> Async.AwaitTask)
            |> ignore
