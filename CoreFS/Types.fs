namespace CoreFS.Types

open Common


[<Struct>]
type SupportedInput =
    | UICancel
    | ChangeMouseInput
    
    member this.AsString =
        match this with
        | UICancel -> "ui_cancel"
        | ChangeMouseInput -> "change_mouse_input"
            
    static member Cases =
        DIUtil.UnionCasesOf<SupportedInput>()

    static member InputStrings() =
        SupportedInput.Cases |> Array.map(fun case -> case.AsString)
        
