using UnityEditor;
using UnityEngine;

public static class GameViewSetup
{
    [MenuItem("Tools/Game View to Right (9:16)")]
    public static void MoveGameViewToRight()
    {
        var gameViewType = typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView");
        if (gameViewType == null) return;

        // Undock: create as a standalone floating window
        var gameView = EditorWindow.GetWindow(gameViewType, false, "Game", true);
        if (gameView == null) return;

        // Screen size
        int screenW = Screen.currentResolution.width;
        int screenH = Screen.currentResolution.height;

        // 9:16 proportions, fit in screen height with margin
        int winH = screenH - 120;
        int winW = winH * 9 / 16;

        // Position: right edge of screen
        int x = screenW - winW - 20;
        int y = 50;

        gameView.position = new Rect(x, y, winW, winH);
        gameView.Repaint();

        Debug.Log($"[GameViewSetup] Game view moved to right ({winW}x{winH} at x={x})");
    }
}
