#if UNITY_IOS
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.IO;

public class FaceLandmarkiOSPostProcessor
{
    [PostProcessBuild(100)]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {
        if (target != BuildTarget.iOS) return;

        // ── Xcode Project: Framework additions ──
        string projPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
        var proj = new PBXProject();
        proj.ReadFromFile(projPath);

        string frameworkTarget = proj.GetUnityFrameworkTargetGuid();
        proj.AddFrameworkToProject(frameworkTarget, "Vision.framework", false);
        proj.AddFrameworkToProject(frameworkTarget, "CoreImage.framework", false);

        proj.WriteToFile(projPath);

        // ── Info.plist: Privacy descriptions + settings ──
        string plistPath = Path.Combine(pathToBuiltProject, "Info.plist");
        var plist = new PlistDocument();
        plist.ReadFromFile(plistPath);
        var root = plist.root;

        // Photo Library (read) - for selecting baby face photos
        if (!root.values.ContainsKey("NSPhotoLibraryUsageDescription"))
            root.SetString("NSPhotoLibraryUsageDescription",
                "Select a photo to customize your baby character.");

        // Photo Library (write) - for saving synthesized images
        if (!root.values.ContainsKey("NSPhotoLibraryAddUsageDescription"))
            root.SetString("NSPhotoLibraryAddUsageDescription",
                "Save your synthesized baby image to your photo library.");

        // Camera - for Vision Framework face detection
        if (!root.values.ContainsKey("NSCameraUsageDescription"))
            root.SetString("NSCameraUsageDescription",
                "Use the camera for face detection to customize your baby.");

        // Localized descriptions (Japanese)
        // These will be added to InfoPlist.strings via localization, but
        // we set English as the base in Info.plist.

        // ITSAppUsesNonExemptEncryption = NO (no encryption used)
        // This avoids the export compliance questionnaire on App Store Connect
        if (!root.values.ContainsKey("ITSAppUsesNonExemptEncryption"))
            root.SetBoolean("ITSAppUsesNonExemptEncryption", false);

        plist.WriteToFile(plistPath);

        // ── InfoPlist.strings (Japanese localization) ──
        CreateJapaneseInfoPlistStrings(pathToBuiltProject);

        UnityEngine.Debug.Log("[iOSPostProcessor] Frameworks, Info.plist, and localization configured.");
    }

    static void CreateJapaneseInfoPlistStrings(string pathToBuiltProject)
    {
        // Create ja.lproj directory
        string jaDir = Path.Combine(pathToBuiltProject, "ja.lproj");
        if (!Directory.Exists(jaDir))
            Directory.CreateDirectory(jaDir);

        string stringsContent =
            "NSPhotoLibraryUsageDescription = \"赤ちゃんの顔写真を選択するためにフォトライブラリを使用します\";\n" +
            "NSPhotoLibraryAddUsageDescription = \"合成した赤ちゃん画像をフォトライブラリに保存します\";\n" +
            "NSCameraUsageDescription = \"赤ちゃんの顔を認識するためにカメラを使用します\";\n";

        File.WriteAllText(Path.Combine(jaDir, "InfoPlist.strings"), stringsContent);

        // Add to Xcode project
        string projPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
        var proj = new PBXProject();
        proj.ReadFromFile(projPath);

        string mainTarget = proj.GetUnityMainTargetGuid();
        string stringsPath = "ja.lproj/InfoPlist.strings";
        string fileGuid = proj.AddFile(stringsPath, stringsPath, PBXSourceTree.Source);
        proj.AddFileToBuild(mainTarget, fileGuid);

        // Add Japanese to known regions
        proj.WriteToFile(projPath);

        // Update project.pbxproj known regions
        string pbxPath = Path.Combine(pathToBuiltProject, "Unity-iPhone.xcodeproj", "project.pbxproj");
        if (File.Exists(pbxPath))
        {
            string content = File.ReadAllText(pbxPath);
            if (!content.Contains("ja,"))
            {
                content = content.Replace(
                    "knownRegions = (\n\t\t\t\ten,\n\t\t\t\tBase,",
                    "knownRegions = (\n\t\t\t\ten,\n\t\t\t\tja,\n\t\t\t\tBase,");
                File.WriteAllText(pbxPath, content);
            }
        }
    }
}
#endif
