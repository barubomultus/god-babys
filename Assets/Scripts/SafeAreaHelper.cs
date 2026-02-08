using UnityEngine;
using UnityEngine.InputSystem;

public static class SafeAreaHelper
{
    public static (float left, float right, float top, float bottom) GetSafeAreaInsets(Canvas canvas)
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        return (0f, 0f, 0f, 0f);
#else
        if (canvas == null) return (0f, 0f, 0f, 0f);

        Rect safeArea = Screen.safeArea;
        float scale = canvas.scaleFactor;
        if (scale <= 0f) scale = 1f;

        float left = safeArea.x / scale;
        float right = (Screen.width - safeArea.xMax) / scale;
        float top = (Screen.height - safeArea.yMax) / scale;
        float bottom = safeArea.y / scale;

        return (left, right, top, bottom);
#endif
    }

    public static bool IsTouchDevice()
    {
#if UNITY_IOS || UNITY_ANDROID
        return true;
#else
        return Touchscreen.current != null;
#endif
    }
}
