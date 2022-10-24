namespace Common.Uti

open Common.Constants
open Godot
open Microsoft.FSharp.Reflection

module InputUtil =
    let getDirectionInput (globalBasis: Basis) (inputAxis: Vector2) =
        globalBasis.z * -inputAxis.x
        + globalBasis.x * inputAxis.y

    let IsInteractionJustPressed () =
        Input.IsActionJustPressed(InputActions.Sprint)

    let IsJumpJustPressed () =
        Input.IsActionJustPressed(InputActions.Jump)

    let IsSprintPressed () =
        Input.IsActionPressed(InputActions.Sprint)

    let getInputAxis () =
        Input.GetVector(InputActions.MoveBack, InputActions.MoveForward, InputActions.MoveLeft, InputActions.MoveRight)

    let IsAnyKeyJustPressed () =
        InputActions.AllInputs
        |> Array.exists (Input.IsActionJustPressed)

    let IsAnyKeyPressed () =
        InputActions.AllInputs
        |> Array.exists (Input.IsActionPressed)


module MouseUtil =
    let isMouseButtonPressed (btnEvent: InputEventMouseButton) (btnListItem: ButtonList) =
        btnEvent.ButtonIndex = int btnListItem
        && btnEvent.Pressed

    let isLeftMouseButtonPressed (btnEvent: InputEventMouseButton) =
        isMouseButtonPressed btnEvent ButtonList.Left

    let isRightMouseButtonPressed (btnEvent: InputEventMouseButton) =
        isMouseButtonPressed btnEvent ButtonList.Right

module ActivePatterns =
    let (|EmptySeq|_|) a =
        if Seq.isEmpty a then
            Some()
        else
            Option.None


module DIUtil =
    let UnionCasesOf<'A> () =
        FSharpType.GetUnionCases typeof<'A>
        |> Array.map (fun case -> FSharpValue.MakeUnion(case, [||]) :?> 'A)


module ProjectSetting =
    let getGravity =
        ProjectSettings.GetSetting("physics/3d/default_gravity") :?> float32

module NodeUtil =

    let inline getNodeResultFromPath<'a when 'a: not struct> (node: Node) (path: NodePath) =
        try
            match node.GetNode<'a>(path) with
            | result -> Result.Ok result
        with
        | e -> Result.Error e.Message

    let inline getNodeFromPath<'a when 'a: not struct> (node: Node) (path: NodePath) = node.GetNode<'a>(path)
