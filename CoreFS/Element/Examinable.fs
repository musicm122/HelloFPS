namespace CoreFS.Component

open System.Threading.Tasks
open Common.Interfaces
open Common.Uti
open CoreFS.Events
open CoreFS.Global
open CoreFS.Services
open CoreFS.Services.ThirdParty.Dialogic
open Godot
open Common

type ExamineFS() =
    inherit Spatial()

    member val Log = Global.Log()

    member val DialogManager: DialogManager = Global.DialogManager()

    member this.PlayerInteractionAvailabilityChangeEvent =
        DialogEvents.PlayerInteractionAvailabilityChange.Publish

    member this.OnInteractionComplete() =
        this.Log.debug [| "OnInteractionComplete called" |]

    member val CanInteract = false with get, set

    [<Export>]
    member val ShouldRemove = false with get, set

    interface IExaminable with
        member this.OnExaminableBodyEntered(body: Node) =
            if body.IsPlayer() then
                this.Log.debug [| "OnExaminableBodyEntered" |]
                DialogEvents.PlayerInteractionAvailabilityChange.Trigger(true)
                this.CanInteract <- true

        member this.OnExaminableBodyExited(body: Node) =
            if body.IsPlayer() then
                this.Log.debug [| "OnExaminableAreaExited" |]
                DialogEvents.PlayerInteractionAvailabilityChange.Trigger(false)
                this.CanInteract <- false

    member this.OnExaminableBodyEntered(body: Node) =
        (this :> IExaminable)
            .OnExaminableBodyEntered(body)

    member this.OnExaminableBodyExited(body: Node) =
        (this :> IExaminable).OnExaminableBodyExited(body)


    [<Export>]
    member val areaPath = new NodePath("../") with get, set

    member val InteractableArea: Area = null with get, set

    [<Export>]
    member val Timeline = "" with get, set

    member this.OnLeftClick (camera: Node) (event: InputEventMouseButton) clickPos clickNormal shapeId =
        this.Log.debug [| "Left clicked " + this.Name |]

    member this.OnRightClick (camera: Node) (event: InputEventMouseButton) clickPos clickNormal shapeId =
        this.Log.debug [| "Right clicked " + this.Name |]

    member this.OnAreaInput
        (camera: Node)
        (event: InputEvent)
        (clickPos: Vector3)
        (clickNormal: Vector3)
        (shapeId: int)
        =
        match event with
        | :? InputEventMouseButton as ie ->
            match ie with
            | ie when ie.isLeftButtonPressed () -> this.OnLeftClick camera ie clickPos clickNormal shapeId
            | ie when ie.isRightButtonPressed () -> this.OnRightClick camera ie clickPos clickNormal shapeId
            | _ -> ()
        | _ -> ()

    member this.dialogic_signal(listenerArg: System.Object) = GD.Print("dialogic_signal\r\n")
    member this.Event_Start(listenerArg: System.Object) = GD.Print("Event_Start\r\n")
    member this.Event_End(listenerArg: System.Object) = GD.Print("Event_End\r\n")
    member this.Timeline_Start(listenerArg: System.Object) = GD.Print("Timeline_Start\r\n")
    member this.Timeline_End(listenerArg: System.Object) = GD.Print("Timeline_End\r\n")
    member this.l6(listenerArg: System.Object) = GD.Print("6\r\n")

    member this.DialogListener(listenerArg: System.Object) =
        //(this :> IPauseable).Pause()
        //this.OnDialogListener(listenerArg)
        DialogEvents.DialogInteractionStart.Trigger()

        Task.Run(fun () -> this.DialogComplete() |> Async.AwaitTask)
        |> ignore

    member this.DialogComplete() : Task =
        let me =
            this.DialogManager :> IDialogManager

        let waitTime = 0.2f

        GD.Print("DialogManager.DialogComplete called")
        DialogEvents.DialogInteractionComplete.Trigger()

        task {
            do! me.PauseForCompletion waitTime
            do! me.DialogComplete()
        }

    member this.startDialog() =
        let args =
            { DialogArg.timeline = this.Timeline
              shouldRemove = this.ShouldRemove
              methodName = nameof this.DialogListener
              onComplete = Some this.OnInteractionComplete }

        let dialogManager = this.DialogManager :> IDialogManager
        dialogManager.StartDialog this args
        // let dialog = DialogicSharp.Start(args.timeline)

        //let result = dialog.Connect(Timeline_End.AsString(), this, args.methodName)
        // match result with
        // | Error.Ok -> this.AddChild(dialog)
        // | _ -> GD.PrintErr("StartDialog failed with args" + args.ToString())

    //(this.DialogManager :> IDialogManager).StartDialog this args

    override this._Ready() =
        this.InteractableArea <- this.GetNode<Area>(this.areaPath)

        this.Log.debug [| "InteractableArea connection to Area BodyEntered "
                          this.InteractableArea.Connect(
                              Signals.Area.BodyEntered,
                              this,
                              nameof this.OnExaminableBodyEntered
                          ) |]

        this.Log.debug [| "InteractableArea connection to Area BodyExited "
                          this.InteractableArea.Connect(
                              Signals.Area.BodyExited,
                              this,
                              nameof this.OnExaminableBodyExited
                          ) |]

    override this._Process(_delta) =
        if this.CanInteract
           && InputUtil.IsInteractionJustPressed() then
            this.CanInteract <- false
            this.startDialog ()

type IndicatorFS() =
    inherit Area()

    member val Log = Global.Log()

    member val DialogManager: DialogManager = Global.DialogManager()
    member val CanInteract = false with get, set

    [<Export>]
    member val ShouldRemove = false with get, set

    member this.OnExaminableBodyEntered(body: Node) =
        if body.IsPlayer() then
            this.Log.debug [| "OnExaminableBodyEntered" |]
            DialogEvents.PlayerInteractionAvailabilityChange.Trigger(true)
            this.CanInteract <- true

    member this.OnExaminableBodyExited(body: Node) =
        if body.IsPlayer() then
            this.Log.debug [| "OnExaminableAreaExited" |]
            DialogEvents.PlayerInteractionAvailabilityChange.Trigger(false)
            this.CanInteract <- false

    override this._Ready() =
        this.Log.debug [| "IndicatorFS OnReady" |]
// this.Log.debug [|
//     "IndicatorFS connection to Area BodyEntered"
//     this.Connect(Signals.Area.BodyEntered, this, nameof this.OnExaminableBodyEntered)
// |]
//
// this.Log.debug [|
//     "IndicatorFS connection to Area BodyExited"
//     this.Connect(Signals.Area.BodyExited , this, nameof this.OnExaminableBodyExited)
// |]
