using System;
using BepInEx;
using UnityEngine;

namespace CultistToolbox.UI;

public class WindowController(int id, string initialTitle, Action windowFunc, Rect initialRect)
{
    public Rect CurrentRect { set; get; } = initialRect;
    public string Title { set; get; } = initialTitle;
    public bool BlocksInput { set; get; } = true;
    private GUIStyle _windowStyle;

    public bool IsMouseInWindow()
    {
        return CurrentRect.Contains(GetMousePosition());
    }

    public void RenderWindow()
    {
        if (_windowStyle == null)
        {
            _windowStyle = new GUIStyle(GUI.skin.window);
            _windowStyle.font = IconFontLocator.IconFont;
        }

        CurrentRect = GUI.Window(id, CurrentRect, DrawWindow, Title, _windowStyle);
    }

    private void DrawWindow(int _)
    {
        windowFunc();

        // Allow dragging
        GUI.DragWindow();

        // Don't pass any input to the rest of the game
        Vector2 mousePosition = UnityInput.Current.mousePosition;
        mousePosition.y = Screen.height - mousePosition.y;
        if (BlocksInput && IsMouseInWindow())
        {
            Input.ResetInputAxes();
        }
    }

    public static Vector2 GetMousePosition()
    {
        Vector2 mousePosition = UnityInput.Current.mousePosition;
        mousePosition.y = Screen.height - mousePosition.y;
        return mousePosition;
    }
}