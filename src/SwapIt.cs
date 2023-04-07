using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ExileCore;
using ExileCore.PoEMemory.MemoryObjects;
using ImGuiNET;
using SharpDX;

namespace SwapIt;

public class SwapIt : BaseSettingsPlugin<SwapItSetting>
{
    private IngameState _ingameState;
    private Vector2 _windowOffset;
    private bool _recording;
    private bool _mousePresed;

    public override bool Initialise()
    {
        base.Initialise();
        _ingameState = GameController.Game.IngameState;
        _windowOffset = GameController.Window.GetWindowRectangle().TopLeft;

        Input.RegisterKey(Settings.StartSwap);
        Settings.StartSwap.OnValueChanged += () => { Input.RegisterKey(Settings.StartSwap); };

        return true;
    }

    public override void DrawSettings()
    {
        var textButton = _recording ? "stop record" : "start record";
        if (ImGui.Button(textButton))
        {
            _recording = _recording == false;
        }

        base.DrawSettings();
        if (ImGui.Button("Clear Mouse Clicks"))
            Settings.CustomMouseClicks.Clear();
    }

    private async void AddClick(MouseButtons mouseButtons)
    {
        if (!Settings.Enable)
            return;

        var currentTupple = Settings.CustomMouseClicks;

        var pos = Input.MousePositionNum;
        pos.X += _windowOffset.X;
        pos.Y += _windowOffset.Y;

        await Task.Delay(300);

        if (_recording == false)
            return;

        currentTupple.Add(new Tuple<float, float, MouseButtons>(pos.X, pos.Y, mouseButtons));

        //foreach (var tuple in currentTupple)
        //    LogMessage($"X:{tuple.Item1} Y:{tuple.Item2} Left:{tuple.Item3}", 1);
    }

    public override void Render()
    {
        if (!Settings.Enable)
            return;
        try
        {
            if (_recording)
            {
                if (_mousePresed == false && Control.MouseButtons is MouseButtons.Left or MouseButtons.Right)
                {
                    _mousePresed = true;

                    AddClick(Control.MouseButtons);
                }

                if (_mousePresed && Control.MouseButtons == MouseButtons.None)
                {
                    _mousePresed = false;
                }
            }

            if (Settings.StartSwap.PressedOnce())
            {
                DoCustomMouseClicks();
            }

            if (Settings.ShowPoint)
                DrawCustomLines();
        }
        catch (Exception e)
        {
            File.AppendAllText("log.txt", $"{e.Source} || {e.Message} \n");
        }
        //base.Render();
    }

    private void DrawCustomLines()
    {
        var colorList = GetGradients(Color.Red, Color.Lime, Settings.CustomMouseClicks.Count);
        for (var i = 0; i < Settings.CustomMouseClicks.Count; i++)
        {
            if (i >= Settings.CustomMouseClicks.Count - 1)
                break;
            Color[] enumerable = colorList as Color[] ?? colorList.ToArray();

            Graphics.DrawLine(
                new Vector2(Settings.CustomMouseClicks[i].Item1,
                    Settings.CustomMouseClicks[i].Item2 - _windowOffset.Y),
                new Vector2(Settings.CustomMouseClicks[i + 1].Item1,
                    Settings.CustomMouseClicks[i + 1].Item2 - _windowOffset.Y), 2, enumerable[i]);
        }
    }

    private static IEnumerable<Color> GetGradients(Color start, Color end, int steps)
    {
        var stepA = (end.A - start.A) / (steps - 1);
        var stepR = (end.R - start.R) / (steps - 1);
        var stepG = (end.G - start.G) / (steps - 1);
        var stepB = (end.B - start.B) / (steps - 1);
        for (var i = 0; i < steps; i++)
            yield return new Color(start.R + stepR * i, start.G + stepG * i, start.B + stepB * i,
                start.A + stepA * i);
    }

    private void DoCustomMouseClicks()
    {
        _recording = false;
        var invNeedClosed = false;
        if (!_ingameState.IngameUi.InventoryPanel.IsVisible)
        {
            invNeedClosed = true;
            Input.KeyDown(Settings.InvHotkey.Value);
            Thread.Sleep(Settings.Speed);
            Input.KeyUp(Settings.InvHotkey.Value);
        }
            

        var mousePosition = Input.MousePositionNum;
        mousePosition.X += _windowOffset.X;
        mousePosition.Y += _windowOffset.Y;

        foreach (var pointClick in Settings.CustomMouseClicks)
        {
            //LogMessage($"X:{pointClick.Item1} Y:{pointClick.Item2} Key:{pointClick.Item3}", 1);

            Input.SetCursorPos(new System.Numerics.Vector2(pointClick.Item1, pointClick.Item2));
            Thread.Sleep(Settings.Speed);
            Input.Click(pointClick.Item3);
            Thread.Sleep(Settings.Speed);
        }

        if (invNeedClosed)
        {
            Input.KeyDown(Settings.InvHotkey.Value);
            Thread.Sleep(Settings.Speed);
            Input.KeyUp(Settings.InvHotkey.Value);
        }

        Input.SetCursorPos(mousePosition);
    }
}

