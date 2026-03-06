#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;

/// <summary>
/// iOS build helper. Menu: Tools > Build iOS
/// Builds the Xcode project to ../StarBabys_iOS/
/// </summary>
public class iOSBuildHelper
{
    static readonly string BuildPath = Path.Combine(
        Directory.GetParent(Application.dataPath).FullName, "..", "StarBabys_iOS");

    [MenuItem("Tools/Build iOS")]
    public static void BuildiOS()
    {
        // Ensure all scenes are included
        var scenes = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path)
            .ToArray();

        if (scenes.Length == 0)
        {
            Debug.LogError("[iOSBuild] No scenes in Build Settings! Add scenes first.");
            return;
        }

        Debug.Log("[iOSBuild] Building iOS to: " + BuildPath);
        Debug.Log("[iOSBuild] Scenes: " + string.Join(", ", scenes));

        // Build options
        var options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = BuildPath,
            target = BuildTarget.iOS,
            options = BuildOptions.None,
        };

        var report = BuildPipeline.BuildPlayer(options);

        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.Log("[iOSBuild] Build succeeded! Output: " + BuildPath);
            Debug.Log("[iOSBuild] Next: Open " + BuildPath + "/Unity-iPhone.xcodeproj in Xcode");
            EditorUtility.RevealInFinder(BuildPath);
        }
        else
        {
            Debug.LogError("[iOSBuild] Build failed: " + report.summary.totalErrors + " errors");
        }
    }

    [MenuItem("Tools/Build iOS (Development)")]
    public static void BuildiOSDev()
    {
        var scenes = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path)
            .ToArray();

        if (scenes.Length == 0)
        {
            Debug.LogError("[iOSBuild] No scenes in Build Settings!");
            return;
        }

        var options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = BuildPath,
            target = BuildTarget.iOS,
            options = BuildOptions.Development | BuildOptions.AllowDebugging,
        };

        var report = BuildPipeline.BuildPlayer(options);

        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.Log("[iOSBuild] Development build succeeded! Output: " + BuildPath);
            EditorUtility.RevealInFinder(BuildPath);
        }
        else
        {
            Debug.LogError("[iOSBuild] Build failed: " + report.summary.totalErrors + " errors");
        }
    }
}
#endif
