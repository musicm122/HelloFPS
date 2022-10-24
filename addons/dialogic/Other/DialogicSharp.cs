using Godot;
using GC = Godot.Collections;

public static class DialogicSharp
{
    private const string DEFAULT_DIALOG_RESOURCE = "res://addons/dialogic/Nodes/DialogNode.tscn";
    private static readonly Script _dialogic = GD.Load<Script>("res://addons/dialogic/Other/DialogicClass.gd");

    // ------------------------------------------------------------------------------------------
    // 				OTHER STUFF
    // ------------------------------------------------------------------------------------------
    public static string CurrentTimeline
    {
        get => (string)_dialogic.Call("get_current_timeline");
        set => _dialogic.Call("set_current_timeline", value);
    }

    public static GC.Dictionary Definitions => (GC.Dictionary)_dialogic.Call("get_definitions");

    public static GC.Dictionary DefaultDefinitions => (GC.Dictionary)_dialogic.Call("get_default_definitions");

    public static bool Autosave
    {
        get => (bool)_dialogic.Call("get_autosave");
        set => _dialogic.Call("set_autosave", value);
    }

    // Check the documentation of the DialogicClass for more information on how to use these functions!
    public static Node Start(string timeline = "", string default_timeline = "", bool useCanvasInstead = true)
    {
        return Start<Node>(timeline, default_timeline, DEFAULT_DIALOG_RESOURCE, useCanvasInstead);
    }

    public static T Start<T>(string timeline = "", string default_timeline = "", string dialogScenePath = "",
        bool useCanvasInstead = true) where T : class
    {
        return (T)_dialogic.Call("start", timeline, default_timeline, dialogScenePath, useCanvasInstead);
    }

    // ------------------------------------------------------------------------------------------
    // 				SAVING/LOADING
    // ------------------------------------------------------------------------------------------
    public static void Load(string slot_name = "")
    {
        _dialogic.Call("load", slot_name);
    }

    public static void Save(string slot_name = "")
    {
        _dialogic.Call("save", slot_name);
    }

    public static GC.Array GetSlotNames()
    {
        return (GC.Array)_dialogic.Call("get_slot_names");
    }

    public static void EraseSlot(string slot_name)
    {
        _dialogic.Call("erase_slot", slot_name);
    }

    public static bool HasCurrentDialogNode()
    {
        return (bool)_dialogic.Call("has_current_dialog_node");
    }

    public static void ResetSaves()
    {
        _dialogic.Call("reset_saves");
    }

    public static string GetCurrentSlot()
    {
        return (string)_dialogic.Call("get_current_slot");
    }

    // ------------------------------------------------------------------------------------------
    // 				IMPORT/EXPORT
    // ------------------------------------------------------------------------------------------
    public static GC.Dictionary Export()
    {
        return (GC.Dictionary)_dialogic.Call("export");
    }

    public static void Import(GC.Dictionary data)
    {
        _dialogic.Call("import", data);
    }

    // ------------------------------------------------------------------------------------------
    // 				DEFINITIONS
    // ------------------------------------------------------------------------------------------
    public static string GetVariable(string name)
    {
        return (string)_dialogic.Call("get_variable", name);
    }

    public static void SetVariable(string name, string value)
    {
        _dialogic.Call("set_variable", name, value);
    }
}