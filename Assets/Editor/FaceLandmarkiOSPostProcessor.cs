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

        // ── Xcode プロジェクト: Vision.framework 追加 ──
        string projPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
        var proj = new PBXProject();
        proj.ReadFromFile(projPath);

        string frameworkTarget = proj.GetUnityFrameworkTargetGuid();
        proj.AddFrameworkToProject(frameworkTarget, "Vision.framework", false);
        proj.AddFrameworkToProject(frameworkTarget, "CoreImage.framework", false);

        proj.WriteToFile(projPath);

        // ── Info.plist: プライバシー権限の追加 ──
        string plistPath = Path.Combine(pathToBuiltProject, "Info.plist");
        var plist = new PlistDocument();
        plist.ReadFromFile(plistPath);
        var root = plist.root;

        // フォトライブラリ読み込み（NativeGallery で画像選択時に必要）
        if (!root.values.ContainsKey("NSPhotoLibraryUsageDescription"))
            root.SetString("NSPhotoLibraryUsageDescription",
                "赤ちゃんの顔写真を選択するためにフォトライブラリを使用します");

        // フォトライブラリ書き込み（合成画像の保存時に必要）
        if (!root.values.ContainsKey("NSPhotoLibraryAddUsageDescription"))
            root.SetString("NSPhotoLibraryAddUsageDescription",
                "合成した赤ちゃん画像をフォトライブラリに保存します");

        // カメラ（Vision Framework での顔認識に必要な場合がある）
        if (!root.values.ContainsKey("NSCameraUsageDescription"))
            root.SetString("NSCameraUsageDescription",
                "赤ちゃんの顔を認識するためにカメラを使用します");

        plist.WriteToFile(plistPath);

        UnityEngine.Debug.Log("[FaceLandmarkiOSPostProcessor] Vision.framework + Info.plist privacy descriptions added");
    }
}
#endif
