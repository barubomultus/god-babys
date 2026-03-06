#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// App Store screenshot capture tool.
/// Play mode: Press F12 to capture screenshot.
/// Saves to project root /Screenshots/ folder.
/// </summary>
[InitializeOnLoad]
public class ScreenshotCapture
{
    static readonly string ScreenshotDir = Path.Combine(
        Directory.GetParent(Application.dataPath).FullName, "Screenshots");

    [MenuItem("Tools/Capture Screenshot (Game View)")]
    public static void CaptureFromMenu()
    {
        if (!Directory.Exists(ScreenshotDir))
            Directory.CreateDirectory(ScreenshotDir);

        string filename = "StarBabys_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png";
        string path = Path.Combine(ScreenshotDir, filename);
        ScreenCapture.CaptureScreenshot(path, 2); // 2x supersampling
        Debug.Log("[Screenshot] Saved to: " + path);
    }
}

/// <summary>
/// Runtime F12 screenshot capture (works in Play mode)
/// </summary>
public class RuntimeScreenshot : MonoBehaviour
{
    void Update()
    {
        if (UnityEngine.InputSystem.Keyboard.current != null &&
            UnityEngine.InputSystem.Keyboard.current.f12Key.wasPressedThisFrame)
        {
            string dir = Path.Combine(Application.persistentDataPath, "Screenshots");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            string filename = "StarBabys_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png";
            string path = Path.Combine(dir, filename);
            ScreenCapture.CaptureScreenshot(path, 2);
            Debug.Log("[Screenshot] F12 captured: " + path);
        }
    }
}
#endif
