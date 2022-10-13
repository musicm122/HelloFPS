namespace Common

open Godot
open Microsoft.FSharp.Reflection

module MouseUtil =
    let isMouseButtonPressed (btnEvent:InputEventMouseButton) (btnListItem:ButtonList) =        
        btnEvent.ButtonIndex = int btnListItem && btnEvent.Pressed 

    let isLeftMouseButtonPressed (btnEvent:InputEventMouseButton) =
        isMouseButtonPressed btnEvent ButtonList.Left

    let isRightMouseButtonPressed (btnEvent:InputEventMouseButton) =
        isMouseButtonPressed btnEvent ButtonList.Right

module ActivePatterns =
    let (|EmptySeq|_|) a = if Seq.isEmpty a then Some() else None


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


[<AutoOpen>]
module Extensions =
    
    type InputEventMouseButton with
        member this.isMouseButtonPressed (btnListItem:ButtonList) =        
            this.ButtonIndex = int btnListItem && this.Pressed 

        member this.isLeftButtonPressed() =
            this.isMouseButtonPressed  ButtonList.Left

        member this.isRightButtonPressed() =
            this.isMouseButtonPressed  ButtonList.Right
    
    
    type Node with
        member this.GetOwnerAsSpatial() = this.Owner :?> Spatial

        member inline this.GetOwnerAs<'a when 'a :> Node>() = this.Owner :?> 'a


    type Vector3 with
        member this.WithX(newX) = Vector3(newX, this.y, this.z)

        member this.AddToX(newX) = Vector3(this.x + newX, this.y, this.z)
        member this.SubToX(newX) = Vector3(this.x - newX, this.y, this.z)

        member this.WithY(newY) = Vector3(this.x, newY, this.z)

        member this.AddToY(newY) = Vector3(this.x, this.y + newY, this.z)
        member this.SubFromY(newY) = Vector3(this.x, this.y - newY, this.z)

        
        member this.WithZ(newZ) = Vector3(this.x, this.y, newZ)
        member this.AddToZ(newZ) = Vector3(this.x, this.y, this.z + newZ)
        member this.SubFromZ(newZ) = Vector3(this.x, this.y, this.z - newZ)

        member this.Add(v2: Vector3) =
            let x = this.x + v2.x
            let y = this.y + v2.y
            let z = this.z + v2.z
            Vector3(x, y, z)

        member this.Subtract(v2: Vector3) =
            let x = this.x - v2.x
            let y = this.y - v2.y
            let z = this.z - v2.z
            Vector3(x, y, z)

    type KinematicCollision with
        member this.ColliderIsInGroup(groupName: string) : bool =
            if this <> null then
                let body = this.Collider :?> PhysicsBody
                body.IsInGroup groupName
            else
                false

    type KinematicBody with
        member this.GetAllColliders() : Option<seq<KinematicCollision>> =
            if this = null then
                None
            else
                let slideCount = this.GetSlideCount()

                match slideCount with
                | 0 -> None
                | _ ->
                    let result =
                        seq {
                            for x in 0 .. this.GetSlideCount() - 1 do
                                yield this.GetSlideCollision(x)
                        }

                    Some(result)

        member this.GetAllCollidersInGroup(groupName: string) =
            let inGroup (collider: KinematicCollision) : bool = collider.ColliderIsInGroup(groupName)

            match this.GetAllColliders() with
            | Some colliders -> colliders |> Seq.filter inGroup
            | None -> Seq.empty

