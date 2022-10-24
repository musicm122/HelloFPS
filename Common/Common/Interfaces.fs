namespace Common.Interfaces

open System.Threading.Tasks
open Godot

type DialogArg =
    { timeline: string
      shouldRemove: bool
      onComplete: (unit -> unit) option }

type IPauseable =
    abstract Pause: unit -> unit
    abstract Unpause: unit -> unit

type ICanApplyPause =
    abstract OnPause: unit -> unit
    abstract OnUnpause: unit -> unit

[<Interface>]
type IExaminer =
    abstract member PlayerInteracting: IEvent<unit>
    abstract member PlayerInteractingComplete: IEvent<unit>
    abstract member PlayerInteractingAvailable: IEvent<unit>
    abstract member PlayerInteractingUnavailable: IEvent<unit>

[<Interface>]
type ILogger =
    abstract debug: obj [] -> unit
    abstract error: obj [] -> unit

[<Interface>]
type IDialogManager =
    abstract member DialogListener: System.Object -> unit
    abstract member DialogComplete: unit -> Task
    abstract member StartDialog: DialogArg -> unit
    abstract member PauseForCompletion: float32 -> Task

[<Interface>]
type IExaminable =
    abstract member OnExaminableBodyEntered: Node -> unit
    abstract member OnExaminableBodyExited: Node -> unit
