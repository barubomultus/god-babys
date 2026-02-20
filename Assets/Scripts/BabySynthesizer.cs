using UnityEngine;
using System.IO;

public class BabySynthesizer : MonoBehaviour
{
    public enum BabyRank { D, C, B, A, S }
    public enum SwaddleType { Newspaper, Cardboard, Towel, Silk, Golden }

    // 合成結果のキャッシュ
    Texture2D compositeTexture;
    Sprite compositeSprite;

    // 合成パラメータの保持（カスタム画像差し替え時の再合成用）
    SynthesizeParams lastParams;

    const int TEX_SIZE = 1080;

    // BabyWear PNGの顔穴パラメータ（全ランク共通位置）
    const float FACE_HOLE_CX = 0.50f;   // 顔穴中心X（比率）
    const float FACE_HOLE_CY = 0.68f;   // 顔穴中心Y（比率、下から）
    const float FACE_HOLE_RX = 0.13f;   // 顔穴半径X（比率）
    const float FACE_HOLE_RY = 0.12f;   // 顔穴半径Y（比率）

    // ===== Rank & Swaddle Determination =====

    public static BabyRank DetermineRank(int fortune)
    {
        if (fortune >= 70)  return BabyRank.S;
        if (fortune >= 55)  return BabyRank.A;
        if (fortune >= 40)  return BabyRank.B;
        if (fortune >= 25)  return BabyRank.C;
        return BabyRank.D;
    }

    public static SwaddleType DetermineSwaddle(BabyRank rank)
    {
        switch (rank)
        {
            case BabyRank.S:   return SwaddleType.Golden;
            case BabyRank.A:   return SwaddleType.Silk;
            case BabyRank.B:   return SwaddleType.Towel;
            case BabyRank.C:   return SwaddleType.Cardboard;
            default:           return SwaddleType.Newspaper;
        }
    }

    // ===== Attachment Determination =====

    static string DetermineAttachmentPath(SynthesizeParams p)
    {
        if (p.fortune >= 80)          return "BabySynth/Attachments/attach_crown";
        if (p.babyIntelligence >= 90) return "BabySynth/Attachments/attach_book";
        if (p.babyAtk >= 70)          return "BabySynth/Attachments/attach_fist";
        if (p.babyLuck >= 70)         return "BabySynth/Attachments/attach_dice";
        if (p.babyAthletic >= 80)     return "BabySynth/Attachments/attach_medal";
        if (p.babyDef >= 80)          return "BabySynth/Attachments/attach_shield";
        return null;
    }

    // ===== Main API =====

    /// <summary>
    /// レイヤー合成を実行し、結果のSpriteを返す（ソフトウェアベース）。
    /// カメラ・SpriteRenderer不使用のため、URP/ライティングの影響を受けない。
    /// </summary>
    public Sprite Synthesize(SynthesizeParams p)
    {
        Cleanup();
        lastParams = p;

        BabyRank rank = DetermineRank(p.fortune);

        Debug.Log($"[BabySynthesizer] Rank={rank}, Fortune={p.fortune}");

        // 1080x1080 のキャンバスに合成
        compositeTexture = new Texture2D(TEX_SIZE, TEX_SIZE, TextureFormat.RGBA32, false);
        var pixels = new Color[TEX_SIZE * TEX_SIZE];

        bool hasCustomFace = HasFaceTexture(p);

        // Layer 0: 背景を塗りつぶし
        DrawBackground(pixels, rank);

        // Layer 1: カスタム顔（アップロード時のみ、BabyWearの下に描画）
        if (hasCustomFace)
            DrawFace(pixels, p);

        // Layer 2: BabyWear PNG（カスタム顔がある場合は顔穴をくり抜く）
        DrawSwaddle(pixels, rank, hasCustomFace);

        // Layer 3: アタッチメント
        DrawAttachment(pixels, p);

        // Layer 3.5: 親アイテムバッジ（おくるみの四隅）
        DrawParentItemBadges(pixels, p);

        // Layer 4: 出生届フレーム
        DrawCertificate(pixels);

        compositeTexture.SetPixels(pixels);
        compositeTexture.Apply();

        compositeSprite = Sprite.Create(compositeTexture,
            new Rect(0, 0, TEX_SIZE, TEX_SIZE),
            new Vector2(0.5f, 0.5f), 100);

        Debug.Log("[BabySynthesizer] Synthesize completed");
        return compositeSprite;
    }

    /// <summary>
    /// スクリーンショット用テクスチャを返す
    /// </summary>
    public Texture2D CaptureToTexture2D()
    {
        if (compositeTexture == null) return null;

        // 現在の合成テクスチャのコピーを返す
        var copy = new Texture2D(compositeTexture.width, compositeTexture.height, TextureFormat.RGBA32, false);
        copy.SetPixels(compositeTexture.GetPixels());
        copy.Apply();
        return copy;
    }

    /// <summary>
    /// カスタム顔画像で差し替え → 再合成してSpriteを返す
    /// </summary>
    public Sprite SetCustomFaceTexture(Texture2D tex, float offsetX = 0f, float offsetY = 0f, float scale = 1f)
    {
        if (tex == null) return compositeSprite;

        // customImagePath をファイル保存して lastParams を更新
        // （すでに BirthSystem 側で保存済みなのでここではパラメータのみ更新）
        var updatedParams = lastParams;
        // カスタム画像は直接テクスチャとして渡されるので、一時保存してパスを設定
        string fileName = "baby_synth_custom.png";
        string savePath = Path.Combine(Application.persistentDataPath, fileName);
        File.WriteAllBytes(savePath, tex.EncodeToPNG());
        updatedParams.customImagePath = fileName;
        updatedParams.faceOffsetX = offsetX;
        updatedParams.faceOffsetY = offsetY;
        updatedParams.faceScale = scale;

        return Synthesize(updatedParams);
    }

    public void Cleanup()
    {
        compositeTexture = null;
        compositeSprite = null;
    }

    // ===== Layer Drawing Methods =====

    void DrawBackground(Color[] pixels, BabyRank rank)
    {
        // PNGアセットがあればそちらを使用
        string path = GetBackgroundPath(rank);
        Sprite bgSprite = Resources.Load<Sprite>(path);
        if (bgSprite != null && bgSprite.texture.isReadable)
        {
            BlitSpriteToCanvas(pixels, bgSprite, 0, 0, TEX_SIZE, TEX_SIZE);
            return;
        }

        // プロシージャルフォールバック: ランク別グラデーション背景
        Color top = GetBackgroundTopColor(rank);
        Color bottom = GetBackgroundBottomColor(rank);
        for (int y = 0; y < TEX_SIZE; y++)
        {
            float t = (float)y / TEX_SIZE;
            Color c = Color.Lerp(bottom, top, t);
            for (int x = 0; x < TEX_SIZE; x++)
            {
                pixels[y * TEX_SIZE + x] = c;
            }
        }
    }

    /// <summary>
    /// カスタム顔画像をBabyWearの顔穴位置に楕円マスクで描画する。
    /// BabyWearの下レイヤーに描画され、BabyWearの顔穴から見える。
    /// </summary>
    void DrawFace(Color[] pixels, SynthesizeParams p)
    {
        Texture2D faceTex = LoadFaceTexture(p);
        if (faceTex == null) return;

        // BabyWearの顔穴に合わせた描画エリア（穴より少し大きめ）
        float cx = TEX_SIZE * FACE_HOLE_CX;
        float cy = TEX_SIZE * FACE_HOLE_CY;
        float rx = TEX_SIZE * FACE_HOLE_RX * 1.05f; // 穴より5%大きく（隙間防止）
        float ry = TEX_SIZE * FACE_HOLE_RY * 1.05f;

        int faceX = (int)(cx - rx);
        int faceY = (int)(cy - ry);
        int faceW = (int)(rx * 2);
        int faceH = (int)(ry * 2);

        float faceScale = p.faceScale > 0.01f ? p.faceScale : 1.0f;

        // アスペクト比を維持するため、均一なサンプリング基準サイズを使用
        // （大きい方に合わせて正方形基準にし、楕円マスクがクリップ）
        float uniformR = Mathf.Max(rx, ry);

        int faceTexW = faceTex.width;
        int faceTexH = faceTex.height;
        Color[] facePixels = faceTex.GetPixels();

        for (int dy = 0; dy < faceH; dy++)
        {
            for (int dx = 0; dx < faceW; dx++)
            {
                int px = faceX + dx;
                int py = faceY + dy;
                if (px < 0 || px >= TEX_SIZE || py < 0 || py >= TEX_SIZE) continue;

                // 楕円マスクチェック
                float ex = (px - cx) / rx;
                float ey = (py - cy) / ry;
                if (ex * ex + ey * ey > 1f) continue;

                // テクスチャからサンプリング（均一UV + offset/scale対応）
                float u = (px - cx) / (uniformR * 2f) + 0.5f;
                float v = (py - cy) / (uniformR * 2f) + 0.5f;
                float u_adj = (u - 0.5f) / faceScale + 0.5f - p.faceOffsetX;
                float v_adj = (v - 0.5f) / faceScale + 0.5f - p.faceOffsetY;
                if (u_adj < 0f || u_adj > 1f || v_adj < 0f || v_adj > 1f) continue;

                int srcX = Mathf.Clamp((int)(u_adj * faceTexW), 0, faceTexW - 1);
                int srcY = Mathf.Clamp((int)(v_adj * faceTexH), 0, faceTexH - 1);
                Color srcColor = facePixels[srcY * faceTexW + srcX];

                if (srcColor.a < 0.01f) continue;

                int idx = py * TEX_SIZE + px;
                pixels[idx] = AlphaBlend(pixels[idx], srcColor);
            }
        }
    }

    /// <summary>
    /// カスタム顔テクスチャが存在するか判定
    /// </summary>
    bool HasFaceTexture(SynthesizeParams p)
    {
        // カスタムアップロード画像がある場合のみ顔穴をくり抜く
        if (!string.IsNullOrEmpty(p.customImagePath))
        {
            string fullPath = Path.Combine(Application.persistentDataPath, p.customImagePath);
            if (File.Exists(fullPath)) return true;
        }
        return false;
    }

    void DrawSwaddle(Color[] pixels, BabyRank rank, bool punchFaceHole)
    {
        // ランク別BabyWear PNGを読み込み
        string wearPath = GetWearPath(rank);
        Sprite wearSprite = Resources.Load<Sprite>(wearPath);
        if (wearSprite != null && wearSprite.texture.isReadable)
        {
            if (punchFaceHole)
                BlitSpriteWithFaceHole(pixels, wearSprite, 0, 0, TEX_SIZE, TEX_SIZE);
            else
                BlitSpriteToCanvas(pixels, wearSprite, 0, 0, TEX_SIZE, TEX_SIZE);
            return;
        }

        // 旧SwaddleType別PNGフォールバック
        SwaddleType type = DetermineSwaddle(rank);
        string path = GetSwaddlePath(type);
        Sprite swaddleSprite = Resources.Load<Sprite>(path);
        if (swaddleSprite != null && swaddleSprite.texture.isReadable)
        {
            if (punchFaceHole)
                BlitSpriteWithFaceHole(pixels, swaddleSprite, 0, 0, TEX_SIZE, TEX_SIZE);
            else
                BlitSpriteToCanvas(pixels, swaddleSprite, 0, 0, TEX_SIZE, TEX_SIZE);
            return;
        }

        // プロシージャルフォールバック: おくるみ形状（顔穴付き）
        Color swaddleColor = GetSwaddleColor(type);
        Color swaddleShadow = new Color(swaddleColor.r * 0.8f, swaddleColor.g * 0.8f, swaddleColor.b * 0.8f, 1f);

        int wrapTop = TEX_SIZE * 55 / 100;
        int wrapBottom = TEX_SIZE * 5 / 100;
        float holeCx = TEX_SIZE / 2f;
        float holeCy = TEX_SIZE * 55 / 100f;
        float holeRx = TEX_SIZE * 22 / 100f;
        float holeRy = TEX_SIZE * 26 / 100f;

        for (int y = wrapBottom; y < wrapTop; y++)
        {
            for (int x = 0; x < TEX_SIZE; x++)
            {
                float bx = (x - TEX_SIZE / 2f) / (TEX_SIZE * 0.42f);
                float by = (y - TEX_SIZE * 0.3f) / (TEX_SIZE * 0.35f);
                if (bx * bx + by * by > 1f) continue;

                float hx = (x - holeCx) / holeRx;
                float hy = (y - holeCy) / holeRy;
                if (hx * hx + hy * hy < 0.85f) continue;

                float edgeFactor = Mathf.Clamp01(bx * bx + by * by);
                Color c = Color.Lerp(swaddleColor, swaddleShadow, edgeFactor * 0.5f);

                int idx = y * TEX_SIZE + x;
                pixels[idx] = AlphaBlend(pixels[idx], c);
            }
        }
    }

    void DrawAttachment(Color[] pixels, SynthesizeParams p)
    {
        string path = DetermineAttachmentPath(p);
        if (path == null) return;

        Sprite attachSprite = Resources.Load<Sprite>(path);
        if (attachSprite != null && attachSprite.texture.isReadable)
        {
            // 右上に小さく配置
            int attachSize = TEX_SIZE * 15 / 100;
            int attachX = TEX_SIZE * 70 / 100;
            int attachY = TEX_SIZE * 70 / 100;
            BlitSpriteToCanvas(pixels, attachSprite, attachX, attachY, attachSize, attachSize);
        }
        // PNGがない場合はアタッチメントを表示しない
    }

    void DrawCertificate(Color[] pixels)
    {
        Sprite certSprite = Resources.Load<Sprite>("BabySynth/Certificate/cert_frame");
        if (certSprite != null && certSprite.texture.isReadable)
        {
            BlitSpriteToCanvas(pixels, certSprite, 0, 0, TEX_SIZE, TEX_SIZE);
        }
        // PNGがない場合はフレームを表示しない
    }

    // ===== Parent Item Badges =====

    void DrawParentItemBadges(Color[] pixels, SynthesizeParams p)
    {
        Sprite fatherItem = null;
        Sprite motherItem = null;

        if (!string.IsNullOrEmpty(p.fatherItemPath))
            fatherItem = Resources.Load<Sprite>(p.fatherItemPath);
        if (!string.IsNullOrEmpty(p.motherItemPath))
            motherItem = Resources.Load<Sprite>(p.motherItemPath);

        if (fatherItem == null && motherItem == null) return;

        int badgeSize = TEX_SIZE * 11 / 100; // ~120px

        // おくるみの四隅に配置（対角に同じ親のアイテム）
        // 左上肩 & 右下足元 = 父アイテム
        // 右上肩 & 左下足元 = 母アイテム
        int shoulderY = TEX_SIZE * 40 / 100;
        int footY     = TEX_SIZE * 10 / 100;
        int leftX     = TEX_SIZE * 8 / 100;
        int rightX    = TEX_SIZE * 81 / 100;

        if (fatherItem != null && fatherItem.texture.isReadable)
        {
            BlitSpriteToCanvas(pixels, fatherItem, leftX, shoulderY, badgeSize, badgeSize);
            BlitSpriteToCanvas(pixels, fatherItem, rightX, footY, badgeSize, badgeSize);
        }
        if (motherItem != null && motherItem.texture.isReadable)
        {
            BlitSpriteToCanvas(pixels, motherItem, rightX, shoulderY, badgeSize, badgeSize);
            BlitSpriteToCanvas(pixels, motherItem, leftX, footY, badgeSize, badgeSize);
        }
    }

    // ===== Face Loading =====

    Texture2D LoadFaceTexture(SynthesizeParams p)
    {
        // Priority 1: カスタム画像
        if (!string.IsNullOrEmpty(p.customImagePath))
        {
            string fullPath = Path.Combine(Application.persistentDataPath, p.customImagePath);
            if (File.Exists(fullPath))
            {
                byte[] data = File.ReadAllBytes(fullPath);
                var tex = new Texture2D(2, 2);
                if (tex.LoadImage(data))
                    return tex;
                Destroy(tex);
            }
        }

        // Priority 2: Resources/babys/{father}_{mother}_{gender}
        string babyImagePath = $"babys/{p.fatherImageName}_{p.motherImageName}_{p.genderKey}";
        var sprite = Resources.Load<Sprite>(babyImagePath);
        if (sprite != null && sprite.texture.isReadable)
        {
            return sprite.texture;
        }

        return null;
    }

    // ===== Procedural Face Generation =====

    Texture2D GenerateProceduralFace(SynthesizeParams p)
    {
        int w = 512, h = 640;
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        var pixels = new Color[w * h];

        Color skin = new Color(0.95f, 0.83f, 0.74f);
        Color bg = new Color(0, 0, 0, 0);

        float cx = w / 2f, cy = h / 2f;
        float rx = w * 0.42f, ry = h * 0.46f;

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float dx = (x - cx) / rx;
                float dy = (y - cy) / ry;
                float d = dx * dx + dy * dy;
                if (d <= 1f)
                {
                    float shade = 1f - d * 0.15f;
                    // 右側にハイライト
                    float highlight = Mathf.Max(0, (x - cx) / rx * 0.08f);
                    pixels[y * w + x] = new Color(
                        Mathf.Min(1f, skin.r * shade + highlight),
                        Mathf.Min(1f, skin.g * shade + highlight * 0.8f),
                        Mathf.Min(1f, skin.b * shade + highlight * 0.6f), 1f);
                }
                else
                {
                    pixels[y * w + x] = bg;
                }
            }
        }

        // 目（白目 + 瞳孔）
        int eyeY = (int)(cy + h * 0.05f);
        int eyeSpacing = (int)(w * 0.14f);
        DrawFilledEllipse(pixels, w, h, (int)(cx - eyeSpacing), eyeY, 24, 20, Color.white);
        DrawFilledEllipse(pixels, w, h, (int)(cx + eyeSpacing), eyeY, 24, 20, Color.white);
        DrawFilledCircle(pixels, w, h, (int)(cx - eyeSpacing), eyeY, 11, new Color(0.2f, 0.15f, 0.1f));
        DrawFilledCircle(pixels, w, h, (int)(cx + eyeSpacing), eyeY, 11, new Color(0.2f, 0.15f, 0.1f));
        // 光沢
        DrawFilledCircle(pixels, w, h, (int)(cx - eyeSpacing + 4), eyeY + 3, 4, new Color(1f, 1f, 1f, 0.9f));
        DrawFilledCircle(pixels, w, h, (int)(cx + eyeSpacing + 4), eyeY + 3, 4, new Color(1f, 1f, 1f, 0.9f));

        // 眉毛
        int browY = eyeY + 30;
        for (int i = -15; i <= 15; i++)
        {
            int bx1 = (int)(cx - eyeSpacing + i);
            int bx2 = (int)(cx + eyeSpacing + i);
            int by = browY + Mathf.Abs(i) / 5;
            if (bx1 >= 0 && bx1 < w && by >= 0 && by < h)
            {
                pixels[by * w + bx1] = new Color(0.3f, 0.2f, 0.15f);
                if (by + 1 < h) pixels[(by + 1) * w + bx1] = new Color(0.3f, 0.2f, 0.15f);
            }
            if (bx2 >= 0 && bx2 < w && by >= 0 && by < h)
            {
                pixels[by * w + bx2] = new Color(0.3f, 0.2f, 0.15f);
                if (by + 1 < h) pixels[(by + 1) * w + bx2] = new Color(0.3f, 0.2f, 0.15f);
            }
        }

        // 鼻
        int noseY = eyeY - 25;
        DrawFilledEllipse(pixels, w, h, (int)cx, noseY, 6, 8, new Color(skin.r * 0.9f, skin.g * 0.88f, skin.b * 0.85f));

        // 口（にっこり）
        int mouthY = (int)(cy - h * 0.1f);
        for (int i = -20; i <= 20; i++)
        {
            int mx = (int)cx + i;
            int my = mouthY - (int)(Mathf.Sqrt(Mathf.Max(0, 400 - i * i)) * 0.3f);
            if (mx >= 0 && mx < w && my >= 0 && my < h)
            {
                pixels[my * w + mx] = new Color(0.85f, 0.45f, 0.45f);
                if (my - 1 >= 0) pixels[(my - 1) * w + mx] = new Color(0.9f, 0.5f, 0.5f);
            }
        }

        // ほっぺ（ピンク）
        DrawFilledEllipse(pixels, w, h, (int)(cx - eyeSpacing - 15), eyeY - 25, 18, 12,
            new Color(1f, 0.7f, 0.7f, 0.3f));
        DrawFilledEllipse(pixels, w, h, (int)(cx + eyeSpacing + 15), eyeY - 25, 18, 12,
            new Color(1f, 0.7f, 0.7f, 0.3f));

        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }

    // ===== Drawing Utilities =====

    static Color AlphaBlend(Color dst, Color src)
    {
        float outA = src.a + dst.a * (1f - src.a);
        if (outA < 0.001f) return new Color(0, 0, 0, 0);
        float r = (src.r * src.a + dst.r * dst.a * (1f - src.a)) / outA;
        float g = (src.g * src.a + dst.g * dst.a * (1f - src.a)) / outA;
        float b = (src.b * src.a + dst.b * dst.a * (1f - src.a)) / outA;
        return new Color(r, g, b, outA);
    }

    static void DrawFilledCircle(Color[] pixels, int w, int h, int cx, int cy, int r, Color color)
    {
        for (int y = cy - r; y <= cy + r; y++)
        {
            for (int x = cx - r; x <= cx + r; x++)
            {
                if (x < 0 || x >= w || y < 0 || y >= h) continue;
                if ((x - cx) * (x - cx) + (y - cy) * (y - cy) <= r * r)
                {
                    int idx = y * w + x;
                    pixels[idx] = AlphaBlend(pixels[idx], color);
                }
            }
        }
    }

    static void DrawFilledEllipse(Color[] pixels, int w, int h, int cx, int cy, int rx, int ry, Color color)
    {
        for (int y = cy - ry; y <= cy + ry; y++)
        {
            for (int x = cx - rx; x <= cx + rx; x++)
            {
                if (x < 0 || x >= w || y < 0 || y >= h) continue;
                float dx = (float)(x - cx) / rx;
                float dy = (float)(y - cy) / ry;
                if (dx * dx + dy * dy <= 1f)
                {
                    int idx = y * w + x;
                    pixels[idx] = AlphaBlend(pixels[idx], color);
                }
            }
        }
    }

    /// <summary>
    /// スプライトを描画するが、顔穴エリア内のピクセルはスキップする。
    /// カスタム顔が下レイヤーにあるとき、BabyWearの顔部分をくり抜くために使用。
    /// </summary>
    void BlitSpriteWithFaceHole(Color[] canvas, Sprite sprite, int dstX, int dstY, int dstW, int dstH)
    {
        var tex = sprite.texture;
        int srcW = tex.width;
        int srcH = tex.height;
        Color[] srcPixels = tex.GetPixels();

        float holeCx = TEX_SIZE * FACE_HOLE_CX;
        float holeCy = TEX_SIZE * FACE_HOLE_CY;
        float holeRx = TEX_SIZE * FACE_HOLE_RX;
        float holeRy = TEX_SIZE * FACE_HOLE_RY;

        for (int dy = 0; dy < dstH; dy++)
        {
            for (int dx = 0; dx < dstW; dx++)
            {
                int px = dstX + dx;
                int py = dstY + dy;
                if (px < 0 || px >= TEX_SIZE || py < 0 || py >= TEX_SIZE) continue;

                // 顔穴の内側はスキップ（くり抜き）
                float fx = (px - holeCx) / holeRx;
                float fy = (py - holeCy) / holeRy;
                if (fx * fx + fy * fy < 1f) continue;

                float u = (float)dx / dstW;
                float v = (float)dy / dstH;
                int sx = Mathf.Clamp((int)(u * srcW), 0, srcW - 1);
                int sy = Mathf.Clamp((int)(v * srcH), 0, srcH - 1);
                Color srcColor = srcPixels[sy * srcW + sx];

                if (srcColor.a < 0.01f) continue;

                int idx = py * TEX_SIZE + px;
                canvas[idx] = AlphaBlend(canvas[idx], srcColor);
            }
        }
    }

    void BlitSpriteToCanvas(Color[] canvas, Sprite sprite, int dstX, int dstY, int dstW, int dstH)
    {
        var tex = sprite.texture;
        int srcW = tex.width;
        int srcH = tex.height;
        Color[] srcPixels = tex.GetPixels();

        for (int dy = 0; dy < dstH; dy++)
        {
            for (int dx = 0; dx < dstW; dx++)
            {
                int px = dstX + dx;
                int py = dstY + dy;
                if (px < 0 || px >= TEX_SIZE || py < 0 || py >= TEX_SIZE) continue;

                float u = (float)dx / dstW;
                float v = (float)dy / dstH;
                int sx = Mathf.Clamp((int)(u * srcW), 0, srcW - 1);
                int sy = Mathf.Clamp((int)(v * srcH), 0, srcH - 1);
                Color srcColor = srcPixels[sy * srcW + sx];

                if (srcColor.a < 0.01f) continue;

                int idx = py * TEX_SIZE + px;
                canvas[idx] = AlphaBlend(canvas[idx], srcColor);
            }
        }
    }

    // ===== Resource Path Helpers =====

    static string GetBackgroundPath(BabyRank rank)
    {
        switch (rank)
        {
            case BabyRank.S:   return "BabySynth/Backgrounds/none"; // 背景なし（白フォールバック）
            case BabyRank.A:   return "BabySynth/Backgrounds/bg_soft_glow";
            case BabyRank.B:   return "BabySynth/Backgrounds/bg_plain";
            case BabyRank.C:   return "BabySynth/Backgrounds/bg_dull";
            default:           return "BabySynth/Backgrounds/bg_dark_room";
        }
    }

    public static string GetWearPath(BabyRank rank)
    {
        switch (rank)
        {
            case BabyRank.S:   return "BabySynth/Swaddles/S_Wear";
            case BabyRank.A:   return "BabySynth/Swaddles/A_Wear";
            case BabyRank.B:   return "BabySynth/Swaddles/B_Wear";
            case BabyRank.C:   return "BabySynth/Swaddles/C_Wear";
            default:           return "BabySynth/Swaddles/D_Wear";
        }
    }

    static string GetSwaddlePath(SwaddleType type)
    {
        switch (type)
        {
            case SwaddleType.Golden:    return "BabySynth/Swaddles/swaddle_golden";
            case SwaddleType.Silk:      return "BabySynth/Swaddles/swaddle_silk";
            case SwaddleType.Towel:     return "BabySynth/Swaddles/swaddle_towel";
            case SwaddleType.Cardboard: return "BabySynth/Swaddles/swaddle_cardboard";
            default:                    return "BabySynth/Swaddles/swaddle_newspaper";
        }
    }

    // ===== Fallback Colors =====

    static Color GetBackgroundTopColor(BabyRank rank)
    {
        switch (rank)
        {
            case BabyRank.S:   return Color.white;
            case BabyRank.A:   return new Color(1.0f, 0.98f, 0.9f);
            case BabyRank.B:   return new Color(0.95f, 0.93f, 0.9f);
            case BabyRank.C:   return new Color(0.75f, 0.73f, 0.7f);
            default:           return new Color(0.35f, 0.33f, 0.3f);
        }
    }

    static Color GetBackgroundBottomColor(BabyRank rank)
    {
        switch (rank)
        {
            case BabyRank.S:   return Color.white;
            case BabyRank.A:   return new Color(0.92f, 0.9f, 0.8f);
            case BabyRank.B:   return new Color(0.85f, 0.82f, 0.78f);
            case BabyRank.C:   return new Color(0.6f, 0.58f, 0.55f);
            default:           return new Color(0.2f, 0.18f, 0.15f);
        }
    }

    static Color GetSwaddleColor(SwaddleType type)
    {
        switch (type)
        {
            case SwaddleType.Golden:    return new Color(1.0f, 0.85f, 0.3f);
            case SwaddleType.Silk:      return new Color(0.95f, 0.92f, 0.88f);
            case SwaddleType.Towel:     return new Color(0.8f, 0.85f, 0.9f);
            case SwaddleType.Cardboard: return new Color(0.7f, 0.55f, 0.35f);
            default:                    return new Color(0.75f, 0.73f, 0.7f);
        }
    }
}

public struct SynthesizeParams
{
    public int fortune;
    public bool isGodBaby;
    public string fatherImageName;
    public string motherImageName;
    public string genderKey;
    public ParentData father;
    public ParentData mother;
    public int babyAtk;
    public int babyDef;
    public int babyHp;
    public int babyIntelligence;
    public int babyAthletic;
    public int babyLuck;
    public string customImagePath;
    public string fatherItemPath; // e.g. "ParentItems/Father_Zenigata_Item"
    public string motherItemPath; // e.g. "ParentItems/Mother_Izanami_Item"
    public float faceOffsetX;  // 水平オフセット (0=中央)
    public float faceOffsetY;  // 垂直オフセット (0=中央)
    public float faceScale;    // 1.0=デフォルト, >1.0=ズームイン
}
