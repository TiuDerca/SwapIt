using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ExileCore.Shared.Attributes;
using ExileCore.Shared.Interfaces;
using ExileCore.Shared.Nodes;

namespace SwapIt;

public class SwapItSetting : ISettings
{
    public SwapItSetting()
    {
        Speed = new RangeNode<int>(100, 50, 500);

        StartSwap = Keys.A;
        InvHotkey = Keys.V;
        //AdditKey1 = Keys.ControlKey;
        //AdditKey2 = Keys.LShiftKey;
        ShowPoint = new ToggleNode(false);
    }


    #region Setting

    public ToggleNode Enable { get; set; } = new(true);
    public RangeNode<int> Speed { get; set; }


    //[Menu("Record", Tooltip = "Enable macro recording. To finish recording, press additional key 1 or 2 and turn it off.")]
    //public HotkeyNode Record { get; set; }

    [Menu("Swap key")]
    public HotkeyNode StartSwap { get; set; }

    [Menu("Set your inv key", Tooltip = "Button to open your inventory in the game")]
    public HotkeyNode InvHotkey { get; set; }

    //[Menu("Additional key 1")]
    //public HotkeyNode AdditKey1 { get; set; }

    //[Menu("Additional key 2")]
    //public HotkeyNode AdditKey2 { get; set; }

    [Menu("Show points", 10)]
    public ToggleNode ShowPoint { get; set; }
    #endregion

        
    public List<Tuple<float, float, MouseButtons>> CustomMouseClicks { get; set; }  = new();
}