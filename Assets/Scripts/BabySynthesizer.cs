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

    // ★★★ デバッグ: 顔穴自動検出を無視し、画像中央に固定サイズで配置 ★★★
    // true にすると: おくるみなし、画像中央に正方形の顔領域、デバッグドットで座標検証
    // 検証完了後は false に戻すこと
    public const bool DEBUG_FIXED_FACE = false;
    // 外部からデバッグモード判定用
    public static bool IsDebugFixedFace => DEBUG_FIXED_FACE;

    // BabyWear PNGの顔穴パラメータ（Default_Wearの透過エリアから自動計算）
    float FACE_HOLE_CX = 0.50f;   // 顔穴中心X（比率）
    float FACE_HOLE_CY = 0.68f;   // 顔穴中心Y（比率、下から）
    float FACE_HOLE_RX = 0.13f;   // 顔穴半径X（比率）
    float FACE_HOLE_RY = 0.12f;   // 顔穴半径Y（比率）
    bool faceHoleDetected = false;

    // Baby Morph パラメータ（BirthSystemから設定、Synthesize/CaptureTransparent時に自動適用）
    bool morphEnabled = false;
    Vector2 morphLeftEye;
    Vector2 morphRightEye;
    Vector2 morphMouthPos;
    FaceLandmarkResult? morphLandmarks;

    // 外部参照用ゲッター
    public float GetFaceHoleCX() { EnsureFaceHoleParams(); return FACE_HOLE_CX; }
    public float GetFaceHoleCY() { EnsureFaceHoleParams(); return FACE_HOLE_CY; }
    public float GetFaceHoleRX() { EnsureFaceHoleParams(); return FACE_HOLE_RX; }
    public float GetFaceHoleRY() { EnsureFaceHoleParams(); return FACE_HOLE_RY; }

    void EnsureFaceHoleParams()
    {
        if (faceHoleDetected) return;
        if (DEBUG_FIXED_FACE)
        {
            FACE_HOLE_CX = 0.50f;
            FACE_HOLE_CY = 0.50f;
            FACE_HOLE_RX = 0.15f;
            FACE_HOLE_RY = 0.15f;
            faceHoleDetected = true;
            Debug.Log("[BabySynthesizer] EnsureFaceHoleParams: DEBUG_FIXED_FACE forced center values");
        }
        else
        {
            DetectFaceHoleFromWear();
        }
    }

    /// <summary>
    /// Baby Morph を有効化。以降の Synthesize / CaptureTransparent で自動適用。
    /// leftEye/rightEye: composite正規化座標 (0-1)
    /// </summary>
    public void EnableBabyMorph(Vector2 leftEye, Vector2 rightEye, Vector2 mouthPos, FaceLandmarkResult? landmarks)
    {
        morphEnabled = true;
        morphLeftEye = leftEye;
        morphRightEye = rightEye;
        morphMouthPos = mouthPos;
        morphLandmarks = landmarks;
        Debug.Log($"[BabySynthesizer] Morph ENABLED: L({leftEye.x:F3},{leftEye.y:F3}) R({rightEye.x:F3},{rightEye.y:F3}) Mouth({mouthPos.x:F3},{mouthPos.y:F3})");
    }

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

        if (DEBUG_FIXED_FACE)
        {
            // ★ デバッグモード: 顔穴自動検出を無視、画像中央に固定配置
            FACE_HOLE_CX = 0.50f;
            FACE_HOLE_CY = 0.50f;  // 画像の真ん中
            FACE_HOLE_RX = 0.15f;  // 半径15%（直径30% = 324px）
            FACE_HOLE_RY = 0.15f;
            faceHoleDetected = true;
            Debug.Log($"[BabySynthesizer] ★ DEBUG_FIXED_FACE: center=(0.50,0.50) radius=(0.15,0.15) = {TEX_SIZE * 0.30f:F0}x{TEX_SIZE * 0.30f:F0}px at image center");
        }
        else
        {
            // 通常モード: Default_Wearの透過エリアから顔穴パラメータを自動検出
            if (!faceHoleDetected)
                DetectFaceHoleFromWear();
        }

        BabyRank rank = DetermineRank(p.fortune);

        Debug.Log($"[BabySynthesizer] Rank={rank}, Fortune={p.fortune}");

        // 1080x1080 のキャンバスに合成
        compositeTexture = new Texture2D(TEX_SIZE, TEX_SIZE, TextureFormat.RGBA32, false);
        var pixels = new Color[TEX_SIZE * TEX_SIZE];

        bool hasCustomFace = HasFaceTexture(p);

        // Layer 0: 背景を塗りつぶし
        DrawBackground(pixels, rank);

        if (DEBUG_FIXED_FACE)
        {
            // ★ デバッグモード: おくるみなし、顔のみ中央に描画
            if (hasCustomFace)
                DrawFaceDebugFixed(pixels, p);
        }
        else
        {
            // Layer 1+2: 顔 + おくるみ（PNGの実際の透過をマスクとして使用）
            DrawSwaddleAndFace(pixels, rank, hasCustomFace, p);
        }

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
    /// スクリーンショット用テクスチャを返す（背景あり）
    /// </summary>
    public Texture2D CaptureToTexture2D()
    {
        if (compositeTexture == null) return null;

        // 現在の合成テクスチャのコピーを返す（モーフなし）
        var copy = new Texture2D(compositeTexture.width, compositeTexture.height, TextureFormat.RGBA32, false);
        copy.SetPixels(compositeTexture.GetPixels());
        copy.Apply();
        return copy;
    }

    /// <summary>
    /// compositeTexture のピクセルを外部から更新（UI表示に反映するため）
    /// </summary>
    public void UpdateCompositeFromPixels(Color[] pixels)
    {
        if (compositeTexture == null || pixels == null) return;
        compositeTexture.SetPixels(pixels);
        compositeTexture.Apply();
        Debug.Log("[BabySynthesizer] compositeTexture updated from external pixels");
    }

    /// <summary>
    /// バトル/マップ用の透過テクスチャを返す（背景レイヤーをスキップ）
    /// </summary>
    public Texture2D CaptureTransparentTexture2D()
    {
        var p = lastParams;
        BabyRank rank = DetermineRank(p.fortune);

        var tex = new Texture2D(TEX_SIZE, TEX_SIZE, TextureFormat.RGBA32, false);
        var pixels = new Color[TEX_SIZE * TEX_SIZE];
        // 背景なし（透明のまま）

        bool hasCustomFace = HasFaceTexture(p);
        if (DEBUG_FIXED_FACE)
        {
            if (hasCustomFace) DrawFaceDebugFixed(pixels, p);
        }
        else
        {
            DrawSwaddleAndFace(pixels, rank, hasCustomFace, p);
        }
        DrawAttachment(pixels, p);
        DrawParentItemBadges(pixels, p);
        DrawCertificate(pixels);

        // Baby Morph 自動適用（透過版にも同じ変形を適用）
        if (morphEnabled)
        {
            pixels = ApplyBabyMorphToPixels(pixels, TEX_SIZE, TEX_SIZE,
                morphLeftEye, morphRightEye, morphMouthPos, morphLandmarks);
            Debug.Log("[BabySynthesizer] Auto-applied BabyMorph in CaptureTransparentTexture2D");
        }

        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
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

    /// <summary>
    /// compositeTexture にモーフを適用する（SaveSynthBabyImage から呼ぶ用）。
    /// compositeSprite も更新されるため、UI表示にも反映される。
    /// </summary>
    public Sprite ApplyMorphToCompositeIfEnabled()
    {
        if (!morphEnabled || compositeTexture == null)
        {
            Debug.Log($"[BabySynthesizer] ApplyMorphToComposite SKIP: morphEnabled={morphEnabled} tex={compositeTexture != null}");
            return compositeSprite;
        }
        Debug.Log($"[BabySynthesizer] ApplyMorphToComposite START: tex={compositeTexture.width}x{compositeTexture.height}");
        Color[] src = compositeTexture.GetPixels();
        Color[] morphed = ApplyBabyMorphToPixels(src, compositeTexture.width, compositeTexture.height,
            morphLeftEye, morphRightEye, morphMouthPos, morphLandmarks);
        compositeTexture.SetPixels(morphed);
        compositeTexture.Apply();
        compositeSprite = Sprite.Create(compositeTexture,
            new Rect(0, 0, TEX_SIZE, TEX_SIZE),
            new Vector2(0.5f, 0.5f), 100);
        Debug.Log("[BabySynthesizer] ApplyMorphToComposite DONE — compositeTexture morphed");
        return compositeSprite;
    }

    // ─── Baby Shape パラメータ（審美的に調整しやすい構造）───
    // 強度を変えて試すときはここだけ触ればOK
    const float BABY_CHEEK_PUFF   = 0.25f;  // 頬ふっくら（下半分の横膨らみ）
    const float BABY_FOREHEAD     = 0.10f;  // おでこを丸く広げる
    const float BABY_CHIN_ROUND   = 0.12f;  // 顎先を丸く広げる
    const float BABY_V_SQUISH     = 0.08f;  // わずかな縦圧縮（丸顔化）
    const float BABY_EYE_INWARD   = 0.05f;  // 両目を中央寄りに（求心顔化, 5%内側）
    const float BABY_EYE_DOWN     = 0.05f;  // 両目を下へ（5%下側、おでこ広く見せる）
    const float BABY_EYE_MAG      = 2.0f;   // 目のバルジ倍率（大きなデカ目）
    const float BABY_EYE_RADIUS   = 0.08f;  // 目拡大の影響半径（composite正規化）
    const float BABY_BLUSH_STRENGTH = 0.30f; // ほっぺ赤みの強さ（0=なし, 1=最大）
    const float BABY_BLUSH_RADIUS = 0.55f;   // ほっぺ赤みの半径（顔穴RXに対する比率）

    /// <summary>
    /// 合成済みcompositeに「ベビーシェイプ」補正を適用（公開ラッパー）。
    /// 通常は EnableBabyMorph → Synthesize で自動適用されるので直接呼ぶ必要なし。
    /// </summary>
    public Sprite ApplyBabyMorphToComposite(Vector2 leftEye, Vector2 rightEye, Vector2 mouthPos, FaceLandmarkResult? landmarks)
    {
        if (compositeTexture == null) return compositeSprite;

        int w = compositeTexture.width;
        int h = compositeTexture.height;
        Color[] src = compositeTexture.GetPixels();
        Color[] dst = ApplyBabyMorphToPixels(src, w, h, leftEye, rightEye, mouthPos, landmarks);

        compositeTexture.SetPixels(dst);
        compositeTexture.Apply();
        compositeSprite = Sprite.Create(compositeTexture,
            new Rect(0, 0, TEX_SIZE, TEX_SIZE),
            new Vector2(0.5f, 0.5f), 100);
        return compositeSprite;
    }

    /// <summary>
    /// ピクセル配列に「ベビーシェイプ」補正を適用して返す（内部コア）。
    /// 赤ちゃん黄金比: 下ぶくれ頬、丸い顎、広いおでこ、求心顔の目、目拡大、ほっぺ赤み。
    /// </summary>
    // 唇の血色感パラメータ
    const float BABY_LIP_STRENGTH = 0.35f;  // 唇の赤み強さ
    const float BABY_LIP_RADIUS   = 0.45f;  // 唇の赤み範囲（顔穴RXに対する比率）

    Color[] ApplyBabyMorphToPixels(Color[] src, int w, int h, Vector2 leftEye, Vector2 rightEye, Vector2 mouthPos, FaceLandmarkResult? landmarks)
    {
        // 顔穴中心（composite ピクセル座標）
        float cx = w * FACE_HOLE_CX;
        float cy = h * FACE_HOLE_CY;
        float faceRx = w * FACE_HOLE_RX;
        float faceRy = h * FACE_HOLE_RY;
        float halfW = faceRx * 1.8f; // 影響半径（顔穴の1.8倍、端がなめらかに消える）
        float halfH = faceRy * 1.8f;

        // 目位置（composite px）— 求心補正を適用済みの位置で拡大
        bool hasLeftEye = leftEye.x >= 0f && leftEye.y >= 0f;
        bool hasRightEye = rightEye.x >= 0f && rightEye.y >= 0f;
        float lEyePx = leftEye.x * w;
        float lEyePy = leftEye.y * h;
        float rEyePx = rightEye.x * w;
        float rEyePy = rightEye.y * h;
        // 目の中点
        float eyeMidX = (hasLeftEye && hasRightEye) ? (lEyePx + rEyePx) * 0.5f : cx;
        float eyeMidY = (hasLeftEye && hasRightEye) ? (lEyePy + rEyePy) * 0.5f : cy + faceRy * 0.3f; // 目は顔中心より上（bottom-originでは+）

        Color[] dst = new Color[w * h];
        System.Array.Copy(src, dst, src.Length);

        // 処理範囲（パフォーマンス）
        int startX = Mathf.Max((int)(cx - halfW * 1.3f), 0);
        int endX   = Mathf.Min((int)(cx + halfW * 1.3f), w);
        int startY = Mathf.Max((int)(cy - halfH * 1.3f), 0);
        int endY   = Mathf.Min((int)(cy + halfH * 1.3f), h);

        for (int oy = startY; oy < endY; oy++)
        {
            for (int ox = startX; ox < endX; ox++)
            {
                // 顔穴中心からの正規化距離
                float fnx = (ox - cx) / halfW;
                float fny = (oy - cy) / halfH;
                float dist2 = fnx * fnx + fny * fny;
                if (dist2 > 1.69f) continue; // 1.3倍半径で切り

                // smoothstep 減衰（端で滑らかにゼロ）
                float t = Mathf.Sqrt(dist2) / 1.3f;
                float falloff = 1f - t * t * (3f - 2f * t);
                if (falloff < 0.001f) continue;

                float sx = (float)ox;
                float sy = (float)oy;

                // ── 1. 下ぶくれ頬ふっくら ──
                // 顔中心より下（fny < 0 = bottom-origin で下）の領域ほど横に膨らむ
                // センターライン保護: 鼻筋(fnx≈0)では強度0、左右の頬(|fnx|>0.3)で最大
                float centerGuard = Mathf.Clamp01((Mathf.Abs(fnx) - 0.1f) / 0.25f);
                float cheekWeight = Mathf.Clamp01(-fny * 1.5f); // 下半分で強く、上はゼロ
                float cheekH = 1f + BABY_CHEEK_PUFF * cheekWeight * falloff * centerGuard * Mathf.Exp(-fnx * fnx * 3f);
                sx = cx + (ox - cx) / cheekH;

                // ── 2. おでこ丸み ──
                // 顔中心より上（fny > 0 = bottom-origin で上）の領域を少し外側に丸く広げる
                float foreheadWeight = Mathf.Clamp01(fny * 1.5f); // 上半分で強く
                float foreheadH = 1f + BABY_FOREHEAD * foreheadWeight * falloff * Mathf.Exp(-fnx * fnx * 3f);
                sx = cx + (sx - cx) / foreheadH;

                // ── 3. 顎を丸く広げる ──
                // 最下部で横にも縦にも少し膨らむ → 尖った顎を防ぐ
                float chinWeight = Mathf.Clamp01((-fny - 0.4f) * 2.5f); // 下40%以降で効く（fny < -0.4 = bottom-origin で下側）
                float chinH = 1f + BABY_CHIN_ROUND * chinWeight * falloff;
                sx = cx + (sx - cx) / chinH;
                // 顎先を上に押して短くする（丸く）— 上=sy増（上から内容を引き込む→顎が圧縮）
                sy = sy + BABY_CHIN_ROUND * chinWeight * falloff * faceRy * 0.5f;

                // ── 4. わずかな縦圧縮 ──
                float vFactor = BABY_V_SQUISH * falloff * Mathf.Exp(-fny * fny * 3f);
                sy = cy + (sy - cy) * (1f + vFactor);

                // ── 5. 求心顔化（目を中央・下寄りに移動）──
                // 目の付近のピクセルを中心方向に引き寄せる
                if (hasLeftEye && hasRightEye)
                {
                    // 左目周辺: 右へ寄せる
                    float dxL = ox - lEyePx;
                    float dyL = oy - lEyePy;
                    float distL = Mathf.Sqrt(dxL * dxL + dyL * dyL);
                    float eyeInfluenceR = faceRx * 0.7f;
                    if (distL < eyeInfluenceR)
                    {
                        float ei = 1f - distL / eyeInfluenceR;
                        ei = ei * ei * (3f - 2f * ei); // smoothstep
                        sx -= BABY_EYE_INWARD * w * ei;  // 右（中央）へ（逆ワープ: sx減→特徴が右へ移動）
                        sy += BABY_EYE_DOWN * h * ei;     // 下へ（逆ワープ: sy増→特徴が下へ移動）
                    }
                    // 右目周辺: 左へ寄せる
                    float dxR = ox - rEyePx;
                    float dyR = oy - rEyePy;
                    float distR = Mathf.Sqrt(dxR * dxR + dyR * dyR);
                    if (distR < eyeInfluenceR)
                    {
                        float ei = 1f - distR / eyeInfluenceR;
                        ei = ei * ei * (3f - 2f * ei);
                        sx += BABY_EYE_INWARD * w * ei;  // 左（中央）へ（逆ワープ: sx増→特徴が左へ移動）
                        sy += BABY_EYE_DOWN * h * ei;     // 下へ
                    }
                }

                // ── 6. 目バルジは専用パスで後から適用 ──

                // バイリニア補間でサンプリング（ジャギー防止）
                dst[oy * w + ox] = BilinearSample(src, w, h, sx, sy);
            }
        }

        // ── 目バルジ専用パス（Morphループの範囲制限に依存しない）──
        if (hasLeftEye || hasRightEye)
        {
            Color[] eyeSrc = new Color[w * h];
            System.Array.Copy(dst, eyeSrc, dst.Length);

            float eyeRadPx = BABY_EYE_RADIUS * w;
            int eyeMargin = (int)(eyeRadPx * 1.2f);

            void ApplyEyePass(float enx, float eny)
            {
                int ecx2 = (int)(enx * w);
                int ecy2 = (int)(eny * h);
                int esx = Mathf.Max(ecx2 - eyeMargin, 0);
                int eex = Mathf.Min(ecx2 + eyeMargin, w);
                int esy = Mathf.Max(ecy2 - eyeMargin, 0);
                int eey = Mathf.Min(ecy2 + eyeMargin, h);

                for (int ey = esy; ey < eey; ey++)
                {
                    for (int ex = esx; ex < eex; ex++)
                    {
                        float esx2 = (float)ex;
                        float esy2 = (float)ey;
                        ApplyEyeMagnify(ref esx2, ref esy2, w, h, enx, eny, BABY_EYE_RADIUS, BABY_EYE_MAG);
                        dst[ey * w + ex] = BilinearSample(eyeSrc, w, h, esx2, esy2);
                    }
                }
            }

            if (hasLeftEye)
            {
                ApplyEyePass(leftEye.x, leftEye.y);
                Debug.Log($"[BabySynthesizer] Eye magnify LEFT: pos=({leftEye.x:F3},{leftEye.y:F3}) px=({leftEye.x * w:F0},{leftEye.y * h:F0}) radius={eyeRadPx:F0}px mag={BABY_EYE_MAG}");
            }
            if (hasRightEye)
            {
                ApplyEyePass(rightEye.x, rightEye.y);
                Debug.Log($"[BabySynthesizer] Eye magnify RIGHT: pos=({rightEye.x:F3},{rightEye.y:F3}) px=({rightEye.x * w:F0},{rightEye.y * h:F0}) radius={eyeRadPx:F0}px mag={BABY_EYE_MAG}");
            }
        }

        // ── ほっぺ赤み（composite空間で直接ペイント）──
        // 頬位置: 顔穴中心から左右・やや下
        float blushOffsetX = faceRx * 0.65f;
        float blushOffsetY = faceRy * 0.25f;
        float blushR = faceRx * BABY_BLUSH_RADIUS;
        float blushLx = cx - blushOffsetX;
        float blushRx = cx + blushOffsetX;
        float blushY  = cy - blushOffsetY; // 頬は顔中心より下（bottom-originでは-）

        // ランドマークから頬位置を使う（あれば）
        if (landmarks.HasValue)
        {
            var lm = landmarks.Value;
            // composite空間に変換された頬位置がleftEye/rightEyeと同じ方法で渡されていないので
            // 顔穴の相対位置から推定
            float cheekNormLx = FACE_HOLE_CX - FACE_HOLE_RX * 0.55f;
            float cheekNormRx = FACE_HOLE_CX + FACE_HOLE_RX * 0.55f;
            float cheekNormY  = FACE_HOLE_CY - FACE_HOLE_RY * 0.2f; // 頬は顔中心より下
            blushLx = cheekNormLx * w;
            blushRx = cheekNormRx * w;
            blushY  = cheekNormY * h;
        }

        Color blushColor = new Color(1f, 0.55f, 0.62f); // パステルピンク
        for (int oy = Mathf.Max((int)(blushY - blushR * 1.5f), 0); oy < Mathf.Min((int)(blushY + blushR * 1.5f), h); oy++)
        {
            for (int ox = Mathf.Max((int)(Mathf.Min(blushLx, blushRx) - blushR * 1.5f), 0);
                 ox < Mathf.Min((int)(Mathf.Max(blushLx, blushRx) + blushR * 1.5f), w); ox++)
            {
                // 左ほっぺ
                float dL = Mathf.Sqrt((ox - blushLx) * (ox - blushLx) + (oy - blushY) * (oy - blushY));
                // 右ほっぺ
                float dR = Mathf.Sqrt((ox - blushRx) * (ox - blushRx) + (oy - blushY) * (oy - blushY));
                float dMin = Mathf.Min(dL, dR);

                if (dMin < blushR)
                {
                    float t = 1f - dMin / blushR;
                    t = t * t * (3f - 2f * t); // smoothstep
                    float blend = BABY_BLUSH_STRENGTH * t;

                    int idx = oy * w + ox;
                    Color c = dst[idx];
                    if (c.a < 0.01f) continue; // 透明部分はスキップ
                    c.r = Mathf.Lerp(c.r, blushColor.r, blend);
                    c.g = Mathf.Lerp(c.g, blushColor.g, blend);
                    c.b = Mathf.Lerp(c.b, blushColor.b, blend);
                    dst[idx] = c;
                }
            }
        }

        // ── 唇の血色感（ほんのりピンク〜赤を口元に乗せる）──
        float lipX = mouthPos.x * w;
        float lipY = mouthPos.y * h;
        float lipRx = faceRx * BABY_LIP_RADIUS;
        float lipRy = lipRx * 0.45f; // 唇は横長
        Color lipColor = new Color(0.92f, 0.45f, 0.50f); // 淡いローズピンク

        for (int oy = Mathf.Max((int)(lipY - lipRy * 1.5f), 0); oy < Mathf.Min((int)(lipY + lipRy * 1.5f), h); oy++)
        {
            for (int ox = Mathf.Max((int)(lipX - lipRx * 1.5f), 0); ox < Mathf.Min((int)(lipX + lipRx * 1.5f), w); ox++)
            {
                // 楕円距離
                float edx = (ox - lipX) / lipRx;
                float edy = (oy - lipY) / lipRy;
                float edist2 = edx * edx + edy * edy;
                if (edist2 > 1f) continue;

                float lt = 1f - Mathf.Sqrt(edist2);
                lt = lt * lt * (3f - 2f * lt); // smoothstep
                float blend = BABY_LIP_STRENGTH * lt;

                int idx = oy * w + ox;
                Color c = dst[idx];
                if (c.a < 0.01f) continue;
                c.r = Mathf.Lerp(c.r, lipColor.r, blend);
                c.g = Mathf.Lerp(c.g, lipColor.g, blend);
                c.b = Mathf.Lerp(c.b, lipColor.b, blend);
                dst[idx] = c;
            }
        }

        Debug.Log($"[BabySynthesizer] BabyShape applied: eyeMag={BABY_EYE_MAG} blushPos=L({blushLx:F0},{blushY:F0}) R({blushRx:F0},{blushY:F0}) lipPos=({lipX:F0},{lipY:F0})");

        return dst;
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
            BlitSpriteToCanvas(pixels, bgSprite, 0, 0, TEX_SIZE, TEX_SIZE, coverMode: true);
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
    /// ★ デバッグ専用: おくるみなし、画像中央の固定位置に顔テクスチャを円形に描画。
    /// offset=0, scale=1 を強制。ユーザー調整を無視して純粋な座標検証を行う。
    /// 黄色=楕円境界、グレー=顔テクスチャが届かない領域。
    /// </summary>
    void DrawFaceDebugFixed(Color[] pixels, SynthesizeParams p)
    {
        Texture2D faceTex = LoadFaceTexture(p);
        if (faceTex == null)
        {
            Debug.LogWarning("[BabySynthesizer] DrawFaceDebugFixed: no face texture");
            return;
        }

        // ★ 固定パラメータ（DrawFace/DrawSwaddleAndFace と同じ計算式）
        float cx = TEX_SIZE * FACE_HOLE_CX;
        float cy = TEX_SIZE * FACE_HOLE_CY;
        float rx = TEX_SIZE * FACE_HOLE_RX * 1.05f;  // ★ DrawFaceと同じ1.05f乗数
        float ry = TEX_SIZE * FACE_HOLE_RY * 1.05f;
        float uniformR = Mathf.Max(rx, ry);

        // ★★★ デバッグモード: offset=0, scale=1 を強制 ★★★
        // ユーザーの顔位置調整は無視して、テクスチャ中央が顔穴中央に一致する状態でテスト
        const float faceScale = 1.0f;
        const float faceOffsetX = 0f;
        const float faceOffsetY = 0f;

        int faceTexW = faceTex.width;
        int faceTexH = faceTex.height;
        Color[] facePixels = faceTex.GetPixels();
        float cropSize = Mathf.Min(faceTexW, faceTexH);
        float cropOffsetX = (faceTexW - cropSize) * 0.5f;
        float cropOffsetY = (faceTexH - cropSize) * 0.5f;

        int faceX = (int)(cx - uniformR);
        int faceY = (int)(cy - uniformR);
        int faceSize = (int)(uniformR * 2);

        Debug.Log($"[BabySynthesizer] ★ DrawFaceDebugFixed: center=({cx:F0},{cy:F0}) rx={rx:F0} ry={ry:F0} uniformR={uniformR:F0} " +
                  $"faceRect=({faceX},{faceY})→({faceX + faceSize},{faceY + faceSize}) " +
                  $"scale=1.0 offset=(0,0) ← ユーザー調整は無視");

        for (int dy = 0; dy < faceSize; dy++)
        {
            for (int dx = 0; dx < faceSize; dx++)
            {
                int px = faceX + dx;
                int py = faceY + dy;
                if (px < 0 || px >= TEX_SIZE || py < 0 || py >= TEX_SIZE) continue;

                // 楕円マスク
                float ex = (px - cx) / rx;
                float ey = (py - cy) / ry;
                float ellipseDist = ex * ex + ey * ey;

                // 楕円境界線（黄色、太さ3px相当）
                if (ellipseDist > 0.94f && ellipseDist < 1.06f)
                {
                    pixels[py * TEX_SIZE + px] = Color.yellow;
                    continue;
                }
                if (ellipseDist > 1f) continue;

                // 顔テクスチャサンプリング（DrawFaceと完全に同じUV計算、ただしscale=1,offset=0）
                float u = (px - cx) / (uniformR * 2f) + 0.5f;
                float v = (py - cy) / (uniformR * 2f) + 0.5f;
                float u_adj = (u - 0.5f) / faceScale + 0.5f - faceOffsetX;  // = u
                float v_adj = (v - 0.5f) / faceScale + 0.5f - faceOffsetY;  // = v

                if (u_adj >= 0f && u_adj <= 1f && v_adj >= 0f && v_adj <= 1f)
                {
                    int srcX = Mathf.Clamp((int)(u_adj * cropSize + cropOffsetX), 0, faceTexW - 1);
                    int srcY = Mathf.Clamp((int)(v_adj * cropSize + cropOffsetY), 0, faceTexH - 1);
                    Color srcColor = facePixels[srcY * faceTexW + srcX];
                    if (srcColor.a > 0.01f)
                        pixels[py * TEX_SIZE + px] = srcColor;
                    else
                        pixels[py * TEX_SIZE + px] = new Color(0.85f, 0.85f, 0.85f, 1f);
                }
                else
                {
                    pixels[py * TEX_SIZE + px] = new Color(0.85f, 0.85f, 0.85f, 1f);
                }
            }
        }

        // 十字線を描画（顔穴の中心位置を示す、シアン色）
        for (int i = (int)(cx - uniformR); i <= (int)(cx + uniformR); i++)
        {
            if (i >= 0 && i < TEX_SIZE && (int)cy >= 0 && (int)cy < TEX_SIZE)
                pixels[(int)cy * TEX_SIZE + i] = Color.cyan;
        }
        for (int j = (int)(cy - uniformR); j <= (int)(cy + uniformR); j++)
        {
            if ((int)cx >= 0 && (int)cx < TEX_SIZE && j >= 0 && j < TEX_SIZE)
                pixels[j * TEX_SIZE + (int)cx] = Color.cyan;
        }

        FillFaceHoleSkinGaps(pixels, cx, cy, rx, ry);
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

        // scale-and-cropと同じ正方形中央クロップ（プレビューとの一致）
        float cropSize = Mathf.Min(faceTexW, faceTexH);
        float cropOffsetX = (faceTexW - cropSize) * 0.5f;
        float cropOffsetY = (faceTexH - cropSize) * 0.5f;

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

                // 正方形中央クロップでサンプリング（プレビューのscale-and-cropと一致）
                int srcX = Mathf.Clamp((int)(u_adj * cropSize + cropOffsetX), 0, faceTexW - 1);
                int srcY = Mathf.Clamp((int)(v_adj * cropSize + cropOffsetY), 0, faceTexH - 1);
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

    /// <summary>
    /// 顔穴楕円内の透明ピクセルを周囲の肌色で膨張塗り（塗り足し）。
    /// 顔テクスチャが楕円の端まで届かない場合に、肌色で隙間を埋める。
    /// </summary>
    void FillFaceHoleSkinGaps(Color[] pixels, float faceCx, float faceCy, float faceRx, float faceRy)
    {
        int startX = Mathf.Max((int)(faceCx - faceRx), 0);
        int endX   = Mathf.Min((int)(faceCx + faceRx), TEX_SIZE);
        int startY = Mathf.Max((int)(faceCy - faceRy), 0);
        int endY   = Mathf.Min((int)(faceCy + faceRy), TEX_SIZE);

        // 反復膨張: 最大60パス（1px/パスなので楕円半径分は十分）
        for (int pass = 0; pass < 60; pass++)
        {
            bool changed = false;
            for (int py = startY; py < endY; py++)
            {
                for (int px = startX; px < endX; px++)
                {
                    // 楕円マスクチェック
                    float ex = (px - faceCx) / faceRx;
                    float ey = (py - faceCy) / faceRy;
                    if (ex * ex + ey * ey > 1f) continue;

                    int idx = py * TEX_SIZE + px;
                    if (pixels[idx].a >= 0.5f) continue; // 既に塗られている

                    // 4近傍の不透明ピクセルの平均色を取得
                    Color sum = Color.clear;
                    int count = 0;
                    int[] ndx = { -1, 1, 0, 0 };
                    int[] ndy = { 0, 0, -1, 1 };
                    for (int n = 0; n < 4; n++)
                    {
                        int nx = px + ndx[n];
                        int ny = py + ndy[n];
                        if (nx >= 0 && nx < TEX_SIZE && ny >= 0 && ny < TEX_SIZE)
                        {
                            Color nc = pixels[ny * TEX_SIZE + nx];
                            if (nc.a >= 0.5f)
                            {
                                sum.r += nc.r;
                                sum.g += nc.g;
                                sum.b += nc.b;
                                count++;
                            }
                        }
                    }
                    if (count > 0)
                    {
                        pixels[idx] = new Color(sum.r / count, sum.g / count, sum.b / count, 1f);
                        changed = true;
                    }
                }
            }
            if (!changed) break;
        }
        Debug.Log("[BabySynthesizer] FillFaceHoleSkinGaps completed");
    }

    /// <summary>
    /// おくるみとカスタム顔を1パスで合成する。
    /// おくるみPNGの実際のピクセル透過をマスクとして使用するため、
    /// 顔穴パラメータの検出精度に依存しない。
    /// </summary>
    void DrawSwaddleAndFace(Color[] pixels, BabyRank rank, bool hasCustomFace, SynthesizeParams p)
    {
        Sprite wearSprite = LoadWearSprite(rank);

        // カスタム顔テクスチャの読み込み
        Texture2D faceTex = hasCustomFace ? LoadFaceTexture(p) : null;

        if (wearSprite != null && wearSprite.texture.isReadable)
        {
            var swaddleTex = wearSprite.texture;
            int srcW = swaddleTex.width;
            int srcH = swaddleTex.height;
            Color[] swaddlePixels = swaddleTex.GetPixels();

            Debug.Log($"[BabySynthesizer] DrawSwaddleAndFace: PNG loaded ({srcW}x{srcH}), hasCustomFace={hasCustomFace}");

            // カスタム顔がない場合 → おくるみのみ描画
            if (faceTex == null)
            {
                BlitSpriteToCanvas(pixels, wearSprite, 0, 0, TEX_SIZE, TEX_SIZE, coverMode: true);
                return;
            }

            // 顔テクスチャの準備
            int faceTexW = faceTex.width;
            int faceTexH = faceTex.height;
            Color[] facePixels = faceTex.GetPixels();

            // scale-and-cropと同じ正方形中央クロップ（プレビューとの一致）
            float faceCropSize = Mathf.Min(faceTexW, faceTexH);
            float faceCropOffsetX = (faceTexW - faceCropSize) * 0.5f;
            float faceCropOffsetY = (faceTexH - faceCropSize) * 0.5f;

            float faceCx = TEX_SIZE * FACE_HOLE_CX;
            float faceCy = TEX_SIZE * FACE_HOLE_CY;
            float faceRx = TEX_SIZE * FACE_HOLE_RX * 1.05f;
            float faceRy = TEX_SIZE * FACE_HOLE_RY * 1.05f;
            float uniformR = Mathf.Max(faceRx, faceRy);
            float faceScale = p.faceScale > 0.01f ? p.faceScale : 1.0f;


            // スキャンラインで各行の不透明境界を事前計算（外周透過 vs 顔穴透過の区別用）
            // 各行で左端・右端の不透明ピクセル位置を記録し、その間の透過=顔穴、外側の透過=背景
            int[] rowLeftOpaque = new int[srcH];
            int[] rowRightOpaque = new int[srcH];
            for (int y = 0; y < srcH; y++)
            {
                rowLeftOpaque[y] = -1;
                rowRightOpaque[y] = -1;
                for (int x = 0; x < srcW; x++)
                {
                    if (swaddlePixels[y * srcW + x].a >= 0.5f)
                    {
                        if (rowLeftOpaque[y] < 0) rowLeftOpaque[y] = x;
                        rowRightOpaque[y] = x;
                    }
                }
            }

            // おくるみのアスペクト比を維持してキャンバスにフィット（cover mode）
            float coverScale = Mathf.Max((float)TEX_SIZE / srcW, (float)TEX_SIZE / srcH);
            float visibleW = TEX_SIZE / coverScale;
            float visibleH = TEX_SIZE / coverScale;
            float coverOffsetX = (srcW - visibleW) * 0.5f;
            float coverOffsetY = (srcH - visibleH) * 0.5f;

            // 1パスで合成
            for (int dy = 0; dy < TEX_SIZE; dy++)
            {
                for (int dx = 0; dx < TEX_SIZE; dx++)
                {
                    // おくるみテクスチャからサンプリング（cover mode: アスペクト比維持）
                    int sx = Mathf.Clamp((int)(coverOffsetX + (float)dx / coverScale), 0, srcW - 1);
                    int sy = Mathf.Clamp((int)(coverOffsetY + (float)dy / coverScale), 0, srcH - 1);
                    Color swaddleColor = swaddlePixels[sy * srcW + sx];

                    int idx = dy * TEX_SIZE + dx;

                    if (swaddleColor.a >= 0.5f)
                    {
                        // おくるみが不透明 → おくるみを描画
                        pixels[idx] = AlphaBlend(pixels[idx], swaddleColor);
                    }
                    else
                    {
                        // 透過ピクセル: 顔穴（内側）か背景（外側）かを判定
                        bool isFaceHole = rowLeftOpaque[sy] >= 0
                            && sx > rowLeftOpaque[sy]
                            && sx < rowRightOpaque[sy];

                        if (isFaceHole)
                        {
                            // 顔穴内 → 顔テクスチャを描画
                            float fu = (dx - faceCx) / (uniformR * 2f) + 0.5f;
                            float fv = (dy - faceCy) / (uniformR * 2f) + 0.5f;
                            float fu_adj = (fu - 0.5f) / faceScale + 0.5f - p.faceOffsetX;
                            float fv_adj = (fv - 0.5f) / faceScale + 0.5f - p.faceOffsetY;

                            if (fu_adj >= 0f && fu_adj <= 1f && fv_adj >= 0f && fv_adj <= 1f)
                            {
                                // 正方形中央クロップでサンプリング（プレビューのscale-and-cropと一致）
                                int fsx = Mathf.Clamp((int)(fu_adj * faceCropSize + faceCropOffsetX), 0, faceTexW - 1);
                                int fsy = Mathf.Clamp((int)(fv_adj * faceCropSize + faceCropOffsetY), 0, faceTexH - 1);
                                Color faceColor = facePixels[fsy * faceTexW + fsx];
                                if (faceColor.a > 0.01f)
                                    pixels[idx] = AlphaBlend(pixels[idx], faceColor);
                            }

                            // 半透明のおくるみピクセルを顔の上に重ねる（装飾枠など）
                            if (swaddleColor.a > 0.01f)
                                pixels[idx] = AlphaBlend(pixels[idx], swaddleColor);
                        }
                        // 背景（外側）→ 何も描画しない（背景レイヤーがそのまま残る）
                    }
                }
            }

            // ★ 顔穴内の肌色補完は削除（楕円アルファマスクで透過制御するため、
            // FillFaceHoleSkinGaps が透過ピクセルを埋め戻すと白い背景が出るバグの原因になる）
            // 残った隙間は DrawBackground の暖色(#F7E7CE)が見えるので自然に馴染む

            return;
        }

        // === フォールバック: PNG読み込み失敗時のプロシージャルおくるみ ===
        Debug.LogWarning("[BabySynthesizer] Using procedural swaddle fallback (PNG not loaded)");

        // まず顔を描画
        if (faceTex != null)
            DrawFace(pixels, p);

        SwaddleType type = DetermineSwaddle(rank);
        Color swaddleColor2 = GetSwaddleColor(type);
        Color swaddleShadow = new Color(swaddleColor2.r * 0.8f, swaddleColor2.g * 0.8f, swaddleColor2.b * 0.8f, 1f);

        int wrapTop = TEX_SIZE * 55 / 100;
        int wrapBottom = TEX_SIZE * 5 / 100;
        float holeCx = TEX_SIZE * FACE_HOLE_CX;
        float holeCy = TEX_SIZE * FACE_HOLE_CY;
        float holeRx = TEX_SIZE * FACE_HOLE_RX * 1.5f;
        float holeRy = TEX_SIZE * FACE_HOLE_RY * 1.5f;

        for (int y = wrapBottom; y < wrapTop; y++)
        {
            for (int x = 0; x < TEX_SIZE; x++)
            {
                float bx = (x - TEX_SIZE / 2f) / (TEX_SIZE * 0.42f);
                float by = (y - TEX_SIZE * 0.3f) / (TEX_SIZE * 0.35f);
                if (bx * bx + by * by > 1f) continue;

                float hx = (x - holeCx) / holeRx;
                float hy = (y - holeCy) / holeRy;
                if (faceTex != null && hx * hx + hy * hy < 0.85f) continue;

                float edgeFactor = Mathf.Clamp01(bx * bx + by * by);
                Color c = Color.Lerp(swaddleColor2, swaddleShadow, edgeFactor * 0.5f);

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
            BlitSpriteToCanvas(pixels, certSprite, 0, 0, TEX_SIZE, TEX_SIZE, coverMode: true);
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

    // ===== Star Babys フィルターパイプライン =====
    //
    // Pipeline（全て顔穴クロップ後も効果が残るよう中心寄りで強めに設計）:
    //   1) BabyMorph — 顔幅30%拡大 + あご15%縮小 + 目2倍拡大（局所バルジ）
    //   2) BabySoft  — 露出大幅UP + 赤色強調 + ソフトコントラスト + ほっぺピンク
    //   3) LightLeak — 中心寄りの暖色光漏れをScreenブレンド（顔穴内でも見える）

    /// <summary>
    /// 非破壊フィルター: 元テクスチャを変更せず、フィルター適用済みの新テクスチャを返す。
    /// leftEye/rightEye: ユーザーが指定した目の正規化座標 (0-1)。(-1,-1) の場合は目拡大スキップ。
    /// </summary>
    public static Texture2D CreateBabyFilteredTexture(Texture2D original,
        Vector2 leftEye, Vector2 rightEye)
    {
        if (original == null) return null;
        return ApplyStudioGhibliStyleFilter(original, leftEye, rightEye, null);
    }

    /// <summary>デッサン風フィルター（dev用）</summary>
    public static Texture2D CreateDessinFilteredTexture(Texture2D original,
        Vector2 leftEye, Vector2 rightEye, FaceLandmarkResult? landmarks = null)
    {
        if (original == null) return null;
        return ApplyDessinFilter(original, leftEye, rightEye, landmarks);
    }

    /// <summary>旧API互換（破壊的適用、目拡大なし）</summary>
    public static void ApplyBabyFilter(Texture2D tex)
    {
        if (tex == null) return;
        var noEye = new Vector2(-1, -1);
        var filtered = CreateBabyFilteredTexture(tex, noEye, noEye);
        if (filtered == null) return;
        tex.SetPixels(filtered.GetPixels());
        tex.Apply();
        Object.Destroy(filtered);
    }

    // ─── ランドマーク対応版フィルター ───

    public static Texture2D CreateBabyFilteredTexture(Texture2D original,
        Vector2 leftEye, Vector2 rightEye, FaceLandmarkResult landmarks)
    {
        if (original == null) return null;
        return ApplyStudioGhibliStyleFilter(original, leftEye, rightEye, landmarks);
    }

    // ─── Stage 1: BabyMorph ───
    // 1パスで全変形を逆マッピング: 顔幅拡大 + 縦圧縮(丸顔) + あご縮小 + 目拡大
    // leftEye/rightEye: ユーザーがタップした目の正規化座標。(-1,-1) なら目拡大スキップ。
    static Color[] ApplyBabyMorph(Color[] src, int w, int h, Vector2 leftEye, Vector2 rightEye)
    {
        float cx = w * 0.5f;
        float cy = h * 0.5f;
        Color[] dst = new Color[w * h];

        float hStrength = 0.4f;    // 顔幅拡大（ぷっくり感を維持しつつ自然に）
        float vStrength = 0.1f;    // 縦圧縮（潰れすぎ防止）
        float chinStrength = 0.15f;

        // 目の拡大パラメータ
        bool hasLeftEye = leftEye.x >= 0f && leftEye.y >= 0f;
        bool hasRightEye = rightEye.x >= 0f && rightEye.y >= 0f;
        float eyeRadius = 0.13f;
        float eyeMag = 2.0f;

        for (int oy = 0; oy < h; oy++)
        {
            for (int ox = 0; ox < w; ox++)
            {
                float nx = (ox - cx) / cx;
                float ny = (oy - cy) / cy;

                // --- 顔幅拡大（ガウシアンフォールオフ + センターライン保護）---
                // 鼻筋(nx≈0)では膨らみ0、左右に向かって徐々に強くなる
                float centerGuardH = Mathf.Clamp01((Mathf.Abs(nx) - 0.15f) / 0.3f);
                float hFactor = 1f + hStrength * centerGuardH * Mathf.Exp(-nx * nx * 2.5f);
                float sx = cx + (ox - cx) / hFactor;

                // --- 縦圧縮（丸顔化: 中心付近ほど強く圧縮）---
                float vFactor = 1f + vStrength * Mathf.Exp(-ny * ny * 2.5f);
                float sy = cy + (oy - cy) * vFactor;

                // --- あご縮小（下半分を圧縮）---
                if (ny > 0.10f)
                {
                    float chinT = (ny - 0.10f) / 0.90f;
                    float chinFactor = 1f + chinStrength * chinT;
                    sy = cy + (sy - cy) / chinFactor;
                }

                // --- 目の拡大（ユーザー指定位置）---
                if (hasLeftEye)
                    ApplyEyeMagnify(ref sx, ref sy, w, h, leftEye.x, leftEye.y, eyeRadius, eyeMag);
                if (hasRightEye)
                    ApplyEyeMagnify(ref sx, ref sy, w, h, rightEye.x, rightEye.y, eyeRadius, eyeMag);

                dst[oy * w + ox] = BilinearSample(src, w, h, sx, sy);
            }
        }

        return dst;
    }

    /// <summary>
    /// 目の局所拡大（逆マッピング）: outputの座標(sx,sy)をsource側に引き寄せる
    /// smoothstep減衰で自然なバルジ効果
    /// </summary>
    static void ApplyEyeMagnify(ref float sx, ref float sy, int w, int h,
        float eyeNX, float eyeNY, float radiusNorm, float mag)
    {
        float ecx = eyeNX * w;
        float ecy = eyeNY * h;
        float radius = radiusNorm * w;

        float dx = sx - ecx;
        float dy = sy - ecy;
        float dist = Mathf.Sqrt(dx * dx + dy * dy);

        if (dist < radius && dist > 0.001f)
        {
            // t: 0（中心）→ 1（端）
            float t = dist / radius;
            // smoothstep: 端で滑らかに1.0へ収束
            float smooth = t * t * (3f - 2f * t);
            // 拡大率: 中心でmag倍 → 端で1.0倍
            float localMag = Mathf.Lerp(mag, 1f, smooth);
            // 逆マッピング: source座標を目の中心に引き寄せる（=output側で拡大される）
            sx = ecx + dx / localMag;
            sy = ecy + dy / localMag;
        }
    }

    // ─── Stage 1L: ランドマーク基準の BabyMorph ───
    // 顔中心を基準にした変形。画像中心ではなく検出された顔位置から変形する。

    static Color[] ApplyBabyMorphLandmark(Color[] src, int w, int h,
        Vector2 leftEye, Vector2 rightEye, FaceLandmarkResult lm)
    {
        // 顔の中心（画像中心ではなく検出された顔中心）
        float cx = lm.FaceCenter.x * w;
        float cy = lm.FaceCenter.y * h;

        // 顔サイズ（変形の影響範囲をスケール）
        float faceW = lm.faceBounds.width * w;
        float faceH = lm.faceBounds.height * h;
        float halfFaceW = Mathf.Max(faceW * 0.5f, 1f);
        float halfFaceH = Mathf.Max(faceH * 0.5f, 1f);

        Color[] dst = new Color[w * h];

        float hStrength = 0.4f;    // 顔幅拡大（ぷっくり感を維持しつつ自然に）
        float vStrength = 0.1f;    // 縦圧縮（潰れすぎ防止）
        float chinStrength = 0.15f;

        bool hasLeftEye = leftEye.x >= 0f && leftEye.y >= 0f;
        bool hasRightEye = rightEye.x >= 0f && rightEye.y >= 0f;
        float eyeRadius = 0.13f;
        float eyeMag = 2.0f;

        // あご縮小開始位置: 鼻の位置から下
        float chinStartY = lm.noseCenter.y >= 0 ? lm.noseCenter.y : (lm.FaceCenter.y + lm.faceBounds.height * 0.1f);

        Debug.Log($"[BabyMorphLandmark] cx={cx:F0} cy={cy:F0} faceW={faceW:F0} faceH={faceH:F0} imgSize={w}x{h}");

        for (int oy = 0; oy < h; oy++)
        {
            for (int ox = 0; ox < w; ox++)
            {
                // 顔中心からの相対位置（顔サイズで正規化）
                float fnx = (ox - cx) / halfFaceW;
                float fny = (oy - cy) / halfFaceH;

                // 顔領域外はそのままコピー（変形しない）— 範囲を広げて9倍に
                float faceDist = fnx * fnx + fny * fny;
                if (faceDist > 9f)
                {
                    dst[oy * w + ox] = SafeGet(src, w, h, ox, oy);
                    continue;
                }

                // --- 顔幅拡大（ガウシアンフォールオフ、顔中心基準）---
                float hFactor = 1f + hStrength * Mathf.Exp(-fnx * fnx * 2.5f);
                float sx = cx + (ox - cx) / hFactor;

                // --- 縦圧縮（丸顔化: 顔中心付近ほど強く）---
                float vFactor = 1f + vStrength * Mathf.Exp(-fny * fny * 2.5f);
                float sy = cy + (oy - cy) * vFactor;

                // --- あご縮小（鼻より下の領域を圧縮）---
                float nyGlobal = (float)oy / h;
                if (nyGlobal > chinStartY)
                {
                    float chinT = (nyGlobal - chinStartY) / Mathf.Max(1f - chinStartY, 0.01f);
                    float chinFactor = 1f + chinStrength * chinT;
                    sy = cy + (sy - cy) / chinFactor;
                }

                // --- 目の拡大（検出位置 or ユーザー指定位置）---
                if (hasLeftEye)
                    ApplyEyeMagnify(ref sx, ref sy, w, h, leftEye.x, leftEye.y, eyeRadius, eyeMag);
                if (hasRightEye)
                    ApplyEyeMagnify(ref sx, ref sy, w, h, rightEye.x, rightEye.y, eyeRadius, eyeMag);

                dst[oy * w + ox] = BilinearSample(src, w, h, sx, sy);
            }
        }
        return dst;
    }

    // ─── Stage 2L: ランドマーク基準の BabySoft ───
    // 検出された頬位置にピンポイントでほっぺピンク。顔サイズ比例のブラシ半径。

    static void ApplyBabySoftLandmark(Color[] pixels, int w, int h, FaceLandmarkResult lm)
    {
        // 頬のピクセル座標
        float leftCheekPx = lm.leftCheek.x * w;
        float leftCheekPy = lm.leftCheek.y * h;
        float rightCheekPx = lm.rightCheek.x * w;
        float rightCheekPy = lm.rightCheek.y * h;

        // ブラシ半径を顔サイズに比例
        float blushRadius = lm.faceBounds.width * w * 0.18f;
        if (blushRadius < 10f) blushRadius = 10f;

        bool hasValidCheeks = lm.leftCheek.x >= 0 && lm.rightCheek.x >= 0;

        for (int i = 0; i < pixels.Length; i++)
        {
            Color c = pixels[i];
            if (c.a < 0.01f) continue;

            // ソフトコントラスト（ふんわり）
            float r = (c.r - 0.5f) * 0.70f + 0.5f;
            float g = (c.g - 0.5f) * 0.70f + 0.5f;
            float b = (c.b - 0.5f) * 0.70f + 0.5f;

            // 露出UP + 赤色強調
            r += 0.14f; g += 0.06f; b -= 0.03f;

            // 明るさブースト
            r *= 1.20f; g *= 1.15f; b *= 1.10f;

            // 彩度ダウン
            float gray = r * 0.299f + g * 0.587f + b * 0.114f;
            r = Mathf.Lerp(r, gray, 0.18f);
            g = Mathf.Lerp(g, gray, 0.18f);
            b = Mathf.Lerp(b, gray, 0.18f);

            // ほっぺピンク（検出頬位置基準）
            if (hasValidCheeks)
            {
                int px = i % w;
                int py = i / w;
                float dL = Mathf.Sqrt(
                    (px - leftCheekPx) * (px - leftCheekPx) +
                    (py - leftCheekPy) * (py - leftCheekPy));
                float dR = Mathf.Sqrt(
                    (px - rightCheekPx) * (px - rightCheekPx) +
                    (py - rightCheekPy) * (py - rightCheekPy));
                float cheekBlend = Mathf.Clamp01(1f - Mathf.Min(dL, dR) / blushRadius) * 0.22f;
                r = Mathf.Lerp(r, 1.0f, cheekBlend);
                g = Mathf.Lerp(g, 0.55f, cheekBlend);
                b = Mathf.Lerp(b, 0.62f, cheekBlend);
            }

            pixels[i] = new Color(Mathf.Clamp01(r), Mathf.Clamp01(g), Mathf.Clamp01(b), c.a);
        }
    }

    // ─── Stage 2: BabySoft ───
    // 色調補正: 露出大幅UP + 赤色強調 + ソフトコントラスト + ほっぺピンク
    // 全て「中心寄り」に効果が集中するよう設計（顔穴クロップ後も残る）
    static void ApplyBabySoft(Color[] pixels, int w, int h)
    {
        float cx = w * 0.5f;
        float cy = h * 0.5f;

        for (int i = 0; i < pixels.Length; i++)
        {
            Color c = pixels[i];
            if (c.a < 0.01f) continue;

            // ソフトコントラスト（ふんわり）
            float r = (c.r - 0.5f) * 0.70f + 0.5f;
            float g = (c.g - 0.5f) * 0.70f + 0.5f;
            float b = (c.b - 0.5f) * 0.70f + 0.5f;

            // 露出UP + 赤色強調（血色の良い赤ちゃん肌）
            r += 0.14f;
            g += 0.06f;
            b -= 0.03f;

            // 明るさブースト
            r *= 1.20f;
            g *= 1.15f;
            b *= 1.10f;

            // 彩度ダウン（ふんわり柔らか）
            float gray = r * 0.299f + g * 0.587f + b * 0.114f;
            r = Mathf.Lerp(r, gray, 0.18f);
            g = Mathf.Lerp(g, gray, 0.18f);
            b = Mathf.Lerp(b, gray, 0.18f);

            // ほっぺピンク（顔の中心寄りに配置）
            int px = i % w;
            int py = i / w;
            float cnx = (px - cx) / cx;
            float cny = (py - cy) / cy;
            // 左右の頬: (±0.30, +0.20) — 中心寄り
            float dL = Mathf.Sqrt((cnx + 0.30f) * (cnx + 0.30f) + (cny - 0.20f) * (cny - 0.20f));
            float dR = Mathf.Sqrt((cnx - 0.30f) * (cnx - 0.30f) + (cny - 0.20f) * (cny - 0.20f));
            float cheekBlend = Mathf.Clamp01(1f - Mathf.Min(dL, dR) / 0.35f) * 0.22f;
            r = Mathf.Lerp(r, 1.0f, cheekBlend);
            g = Mathf.Lerp(g, 0.55f, cheekBlend);
            b = Mathf.Lerp(b, 0.62f, cheekBlend);

            pixels[i] = new Color(Mathf.Clamp01(r), Mathf.Clamp01(g), Mathf.Clamp01(b), c.a);
        }
    }

    // ─── Stage 3: LightLeak ───
    // 中心寄りの暖色光漏れ（顔穴クロップ後も見えるよう光源を中心付近に配置）
    // Screen blend: result = 1 - (1 - base) * (1 - leak)
    static void ApplyLightLeak(Color[] pixels, int w, int h)
    {
        for (int i = 0; i < pixels.Length; i++)
        {
            Color c = pixels[i];
            if (c.a < 0.01f) continue;

            int px = i % w;
            int py = i / w;
            float nx = (float)px / w;
            float ny = (float)py / h;

            // 光源1: 右上寄り（中心に近づけた）ゴールドピンク
            float d1 = Mathf.Sqrt((nx - 0.70f) * (nx - 0.70f) + (ny - 0.20f) * (ny - 0.20f));
            float i1 = Mathf.Clamp01(1f - d1 / 0.5f);
            i1 = i1 * i1 * 0.30f;

            // 光源2: 左下寄り ミントグリーン (#AAF0D1)
            float d2 = Mathf.Sqrt((nx - 0.25f) * (nx - 0.25f) + (ny - 0.75f) * (ny - 0.75f));
            float i2 = Mathf.Clamp01(1f - d2 / 0.45f);
            i2 = i2 * i2 * 0.22f;

            // 光源3: 上部中央 サクラピンク (#FFB7C5)
            float d3 = Mathf.Sqrt((nx - 0.5f) * (nx - 0.5f) * 3f + (ny - 0.15f) * (ny - 0.15f));
            float i3 = Mathf.Clamp01(1f - d3 / 0.4f);
            i3 = i3 * i3 * 0.25f;

            // リークカラー合成
            float lr = 1.0f * i1 + 0.67f * i2 + 1.0f * i3;
            float lg = 0.78f * i1 + 0.94f * i2 + 0.72f * i3;
            float lb = 0.55f * i1 + 0.82f * i2 + 0.77f * i3;

            // Screen blend
            float r = 1f - (1f - c.r) * (1f - lr);
            float g = 1f - (1f - c.g) * (1f - lg);
            float b = 1f - (1f - c.b) * (1f - lb);

            pixels[i] = new Color(Mathf.Clamp01(r), Mathf.Clamp01(g), Mathf.Clamp01(b), c.a);
        }
    }

    // ─── フィルター共通定数 ───
    const int   FILTER_WORK_SIZE         = 768;
    const int   FILTER_BLUR_RADIUS       = 3;
    const float FILTER_EDGE_THRESHOLD    = 0.08f;

    // ─── デッサン風フィルター定数 ───
    const float DESSIN_SATURATION_BOOST  = 1.35f;
    const float DESSIN_SHADOW_THRESH     = 0.35f;
    const float DESSIN_HIGHLIGHT_THRESH  = 0.72f;
    const float DESSIN_SHADOW_DARKEN     = 0.78f;
    const float DESSIN_HIGHLIGHT_BRIGHTEN = 1.15f;
    const float DESSIN_TONE_TRANSITION   = 0.08f;
    const float DESSIN_EDGE_SENSITIVITY  = 0.12f;
    const float DESSIN_OUTLINE_OPACITY   = 0.55f;

    // ─── ジブリ風（セルルック）フィルター定数 ───
    const int   GHIBLI_WORK_SIZE         = 512;      // 顔クロップ後の正方形サイズ（バイラテラル高速化）
    const float GHIBLI_SKIN_BLEND        = 0.80f;    // 理想肌色への置換強度（強く）
    const float GHIBLI_SHADOW_THRESH     = 0.45f;    // セルシェーディング: 影の閾値
    const float GHIBLI_HIGHLIGHT_THRESH  = 0.62f;    // セルシェーディング: ハイライトの閾値（広い範囲をハイライト化）
    const float GHIBLI_SHADOW_DARKEN     = 0.68f;    // 影の暗さ（掛け率: 32%暗く → 段差が歴然）
    const float GHIBLI_HIGHLIGHT_LIFT    = 1.18f;    // ハイライトの明るさ（18%明るく）
    const float GHIBLI_TONE_SOFTNESS     = 0.04f;    // 陰影境界のぼかし幅（シャープ → アニメ塗り感）
    const float GHIBLI_CHEEK_PINK        = 0.35f;    // ほっぺピンク強度（強く）
    const float GHIBLI_PEACH_CLAMP       = 0.82f;    // 白飛び防止: この輝度以上をピーチに引き戻す（積極的）

    // ─── デッサン風フィルター パイプライン ───

    /// <summary>
    /// デッサン風: ダウンスケール → エッジ保持ブラー → 彩度ブースト → セルシェーディング → 輪郭線 → ハイライト
    /// </summary>
    static Texture2D ApplyDessinFilter(Texture2D original, Vector2 leftEye, Vector2 rightEye, FaceLandmarkResult? landmarks)
    {
        int origW = original.width;
        int origH = original.height;
        Color[] origPixels = original.GetPixels();

        int workSize = FILTER_WORK_SIZE;
        Color[] work = DownscaleToWorkSize(origPixels, origW, origH, workSize);

        Color[] blurred = EdgePreservingSmooth(work, workSize, workSize);
        ApplySaturationBoost(blurred, workSize, workSize, DESSIN_SATURATION_BOOST);
        ApplyCelShading(blurred, workSize, workSize);
        ApplyOutlines(blurred, work, workSize, workSize);
        ApplySpecularHighlights(blurred, workSize, workSize, leftEye, rightEye, landmarks);

        var filtered = new Texture2D(workSize, workSize, TextureFormat.RGBA32, false);
        filtered.SetPixels(blurred);
        filtered.Apply();
        return filtered;
    }

    // ─── スタジオジブリ風（セルルック）フィルター パイプライン ───

    /// <summary>
    /// スタジオジブリ風セルルックフィルター:
    ///   1) バイラテラルフィルタ3回で肌を陶器のようにフラット化
    ///   2) 理想肌色への置換（白桃色ピンクベージュ）
    ///   3) 3階調セルシェーディング（陰影を段差に単純化、境界をソフトにぼかす）
    ///   4) 白飛び→ピーチ引き戻しトーンマッピング（輝度0.92以上を暖色に引き戻す）
    ///   5) 目ディテールアップ（黒目強調 + ジブリ風2点キャッチライト）
    ///   6) 口の単純化（唇を薄ピンク1色に）
    ///   7) ほっぺピンク（サーモンピンク）
    ///   8) 楕円アルファマスク（顔外側を透過にして白い背景を防ぐ）
    /// ブライトネスブースト・グロー・スペキュラは一切なし。
    /// </summary>
    static Texture2D ApplyStudioGhibliStyleFilter(Texture2D original, Vector2 leftEye, Vector2 rightEye, FaceLandmarkResult? landmarks)
    {
        int origW = original.width;
        int origH = original.height;
        Color[] origPixels = original.GetPixels();

        // ══════════════════════════════════════════════
        // Stage 0: 顔中心クロップ（目の位置基準で正方形に切り出し → 512x512に縮小）
        // ══════════════════════════════════════════════
        bool hasLeft  = leftEye.x  >= 0f && leftEye.y  >= 0f;
        bool hasRight = rightEye.x >= 0f && rightEye.y >= 0f;

        int workSize = GHIBLI_WORK_SIZE;
        Color[] work;

        if (hasLeft && hasRight)
        {
            // 目の位置（ピクセル座標）
            float lx = leftEye.x  * origW, ly = leftEye.y  * origH;
            float rx = rightEye.x * origW, ry = rightEye.y * origH;
            float eyeMidX = (lx + rx) * 0.5f;
            float eyeMidY = (ly + ry) * 0.5f;
            float eyeDist = Mathf.Sqrt((rx - lx) * (rx - lx) + (ry - ly) * (ry - ly));
            if (eyeDist < 10f) eyeDist = Mathf.Min(origW, origH) * 0.25f; // フォールバック

            // クロップ: 目の距離の1.8倍 = おでこ〜顎をカバーする正方形
            float cropRadius = eyeDist * 1.8f;
            float cropSize = cropRadius * 2f;

            // 顔中心 = 目の中点より少し下（顔の重心は目と顎の中間）
            float faceCX = eyeMidX;
            float faceCY = eyeMidY + eyeDist * 0.35f;

            // クロップ範囲（画像端でクランプ）
            int cropX0 = Mathf.Max(0, Mathf.RoundToInt(faceCX - cropRadius));
            int cropY0 = Mathf.Max(0, Mathf.RoundToInt(faceCY - cropRadius));
            int cropX1 = Mathf.Min(origW, Mathf.RoundToInt(faceCX + cropRadius));
            int cropY1 = Mathf.Min(origH, Mathf.RoundToInt(faceCY + cropRadius));

            // クランプ後のサイズを正方形に揃える
            int cropW = cropX1 - cropX0;
            int cropH = cropY1 - cropY0;
            int cropSide = Mathf.Min(cropW, cropH);
            // 正方形にするために中心寄せ
            cropX0 = cropX0 + (cropW - cropSide) / 2;
            cropY0 = cropY0 + (cropH - cropSide) / 2;
            cropX1 = cropX0 + cropSide;
            cropY1 = cropY0 + cropSide;
            // 安全クランプ
            if (cropX0 < 0) { cropX0 = 0; cropX1 = Mathf.Min(cropSide, origW); }
            if (cropY0 < 0) { cropY0 = 0; cropY1 = Mathf.Min(cropSide, origH); }
            if (cropX1 > origW) { cropX1 = origW; cropX0 = Mathf.Max(0, origW - cropSide); }
            if (cropY1 > origH) { cropY1 = origH; cropY0 = Mathf.Max(0, origH - cropSide); }
            cropSide = Mathf.Min(cropX1 - cropX0, cropY1 - cropY0);

            Debug.Log($"[GhibliFilter] Stage0 FaceCrop: eye=({eyeMidX:F0},{eyeMidY:F0}) dist={eyeDist:F0} " +
                $"crop=({cropX0},{cropY0})-({cropX1},{cropY1}) side={cropSide} → {workSize}x{workSize}");

            // クロップしてワークサイズにリサイズ
            Color[] cropped = new Color[cropSide * cropSide];
            for (int y = 0; y < cropSide; y++)
            {
                int srcY = cropY0 + y;
                for (int x = 0; x < cropSide; x++)
                {
                    int srcX = cropX0 + x;
                    cropped[y * cropSide + x] = origPixels[srcY * origW + srcX];
                }
            }
            work = DownscaleKeepAspect(cropped, cropSide, cropSide, workSize, workSize);

            // 目の座標をクロップ後の正規化座標にリマップ
            leftEye = new Vector2(
                (lx - cropX0) / cropSide,
                (ly - cropY0) / cropSide);
            rightEye = new Vector2(
                (rx - cropX0) / cropSide,
                (ry - cropY0) / cropSide);

            // ランドマークもリマップ
            if (landmarks.HasValue)
            {
                var lm = landmarks.Value;
                lm.leftEyeCenter  = RemapLandmark(lm.leftEyeCenter, origW, origH, cropX0, cropY0, cropSide);
                lm.rightEyeCenter = RemapLandmark(lm.rightEyeCenter, origW, origH, cropX0, cropY0, cropSide);
                lm.noseCenter     = RemapLandmark(lm.noseCenter, origW, origH, cropX0, cropY0, cropSide);
                lm.mouthCenter    = RemapLandmark(lm.mouthCenter, origW, origH, cropX0, cropY0, cropSide);
                lm.leftCheek      = RemapLandmark(lm.leftCheek, origW, origH, cropX0, cropY0, cropSide);
                lm.rightCheek     = RemapLandmark(lm.rightCheek, origW, origH, cropX0, cropY0, cropSide);
                lm.faceBounds = new Rect(
                    (lm.faceBounds.x * origW - cropX0) / cropSide,
                    (lm.faceBounds.y * origH - cropY0) / cropSide,
                    lm.faceBounds.width * origW / cropSide,
                    lm.faceBounds.height * origH / cropSide);
                if (lm.jawlinePoints != null)
                {
                    for (int i = 0; i < lm.jawlinePoints.Length; i++)
                        lm.jawlinePoints[i] = RemapLandmark(lm.jawlinePoints[i], origW, origH, cropX0, cropY0, cropSide);
                }
                landmarks = lm;
            }
        }
        else
        {
            // 目の位置不明 → 従来通り中央クロップ（フォールバック）
            int minDim = Mathf.Min(origW, origH);
            int cropX0 = (origW - minDim) / 2;
            int cropY0 = (origH - minDim) / 2;

            Color[] cropped = new Color[minDim * minDim];
            for (int y = 0; y < minDim; y++)
            {
                int srcY = cropY0 + y;
                for (int x = 0; x < minDim; x++)
                {
                    int srcX = cropX0 + x;
                    cropped[y * minDim + x] = origPixels[srcY * origW + srcX];
                }
            }
            work = DownscaleKeepAspect(cropped, minDim, minDim, workSize, workSize);
        }

        // ═══ フィルター全段無効化: まずクロップだけ確認 ═══
        // TODO: クロップ確認後、フィルターを段階的に有効化

        // 楕円マスクのみ適用（白い背景防止）
        ApplyEllipticalAlphaMask(work, workSize, workSize, leftEye, rightEye);

        var filtered = new Texture2D(workSize, workSize, TextureFormat.RGBA32, false);
        filtered.SetPixels(work);
        filtered.Apply();
        return filtered;
    }

    /// <summary>ランドマーク座標を元画像空間からクロップ後の正規化座標にリマップ</summary>
    static Vector2 RemapLandmark(Vector2 pt, int origW, int origH, int cropX0, int cropY0, int cropSide)
    {
        return new Vector2(
            (pt.x * origW - cropX0) / cropSide,
            (pt.y * origH - cropY0) / cropSide);
    }

    /// <summary>アスペクト比を保持してバイリニア縮小</summary>
    static Color[] DownscaleKeepAspect(Color[] src, int srcW, int srcH, int dstW, int dstH)
    {
        Color[] dst = new Color[dstW * dstH];
        float scaleX = (float)srcW / dstW;
        float scaleY = (float)srcH / dstH;

        for (int y = 0; y < dstH; y++)
        {
            for (int x = 0; x < dstW; x++)
            {
                float sx = x * scaleX;
                float sy = y * scaleY;
                dst[y * dstW + x] = BilinearSample(src, srcW, srcH, sx, sy);
            }
        }
        return dst;
    }

    /// <summary>
    /// 楕円マスク: 顔領域の外側を透過(alpha=0)にする。
    /// 目の位置から顔の中心とサイズを推定し、その外側をフェードアウト。
    /// </summary>
    static void ApplyEllipticalAlphaMask(Color[] pixels, int w, int h, Vector2 leftEye, Vector2 rightEye)
    {
        bool hasLeft  = leftEye.x  >= 0f && leftEye.y  >= 0f;
        bool hasRight = rightEye.x >= 0f && rightEye.y >= 0f;

        // 顔の中心とサイズを推定
        float faceCX, faceCY, faceRadX, faceRadY;
        if (hasLeft && hasRight)
        {
            float eyeMidX = (leftEye.x + rightEye.x) * 0.5f;
            float eyeMidY = (leftEye.y + rightEye.y) * 0.5f;
            float eyeDist = Vector2.Distance(leftEye, rightEye);

            faceCX = eyeMidX;
            faceCY = eyeMidY + eyeDist * 0.25f; // 目より少し下が顔の中心
            faceRadX = eyeDist * 1.0f;            // 横幅 = 目の距離の2.0倍（引き締め）
            faceRadY = eyeDist * 1.3f;            // 縦幅 = 横より長い（顔は縦長）
        }
        else
        {
            // 目の位置不明 → 画像中心に大きめの楕円
            faceCX = 0.5f;
            faceCY = 0.5f;
            faceRadX = 0.45f;
            faceRadY = 0.48f;
        }

        int cx = Mathf.RoundToInt(faceCX * w);
        int cy = Mathf.RoundToInt(faceCY * h);
        float rx = faceRadX * w;
        float ry = faceRadY * h;

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float dx = (x - cx) / rx;
                float dy = (y - cy) / ry;
                float dist = dx * dx + dy * dy;

                if (dist > 1f)
                {
                    // 楕円の外側 → 完全透過
                    int idx = y * w + x;
                    pixels[idx] = new Color(0, 0, 0, 0);
                }
                else if (dist > 0.85f)
                {
                    // 楕円の端 → ソフトフェードアウト
                    float fade = 1f - (dist - 0.85f) / 0.15f;
                    fade = fade * fade * (3f - 2f * fade); // smoothstep
                    int idx = y * w + x;
                    Color c = pixels[idx];
                    pixels[idx] = new Color(c.r, c.g, c.b, c.a * fade);
                }
            }
        }
    }

    // ─── ポスタライズ ───

    /// <summary>各チャンネルの階調を減らして面を作る</summary>
    static void ApplyPosterize(Color[] pixels, int w, int h, int levels)
    {
        float step = 1f / (levels - 1);
        for (int i = 0; i < pixels.Length; i++)
        {
            Color c = pixels[i];
            if (c.a < 0.01f) continue;
            pixels[i] = new Color(
                Mathf.Round(c.r / step) * step,
                Mathf.Round(c.g / step) * step,
                Mathf.Round(c.b / step) * step,
                c.a);
        }
    }

    // ─── 理想肌色置換 ───

    /// <summary>肌色領域を検出し、理想的な白桃色（ピンクベージュ）に置換</summary>
    static void ApplyIdealSkinTone(Color[] pixels, int w, int h, float blend)
    {
        // 理想肌色: 暖かく透明感のあるピンクベージュ
        Color idealSkin = new Color(0.96f, 0.85f, 0.78f);   // ベース
        Color idealSkinWarm = new Color(0.98f, 0.82f, 0.74f); // やや暖色寄り（陰部分）

        for (int i = 0; i < pixels.Length; i++)
        {
            Color c = pixels[i];
            if (c.a < 0.01f) continue;

            // 肌色判定: HSV空間で暖色系 + 中〜高明度
            float cmax = Mathf.Max(c.r, Mathf.Max(c.g, c.b));
            float cmin = Mathf.Min(c.r, Mathf.Min(c.g, c.b));
            float delta = cmax - cmin;
            float lum = c.r * 0.299f + c.g * 0.587f + c.b * 0.114f;

            // 肌色: Rが最大チャンネル、彩度が低〜中、明度が中以上
            bool isSkinLike = (c.r >= c.g && c.r >= c.b)   // Rが最大
                && (delta < 0.45f)                          // 高彩度すぎない
                && (lum > 0.25f && lum < 0.95f);            // 明度の範囲

            if (isSkinLike)
            {
                // 明るい部分は明るい肌色、暗い部分は暖色寄り
                Color target = Color.Lerp(idealSkinWarm, idealSkin, Mathf.Clamp01((lum - 0.3f) / 0.5f));
                pixels[i] = new Color(
                    Mathf.Lerp(c.r, target.r, blend),
                    Mathf.Lerp(c.g, target.g, blend),
                    Mathf.Lerp(c.b, target.b, blend),
                    c.a);
            }
        }
    }

    // ─── 目のディテールアップ ───

    /// <summary>目の中をクリアに: 白目を白く、黒目を大きく、ジブリ風ハイライト2点描画</summary>
    static void ApplyEyeDetailUp(Color[] pixels, int w, int h, Vector2 leftEye, Vector2 rightEye)
    {
        bool hasLeft  = leftEye.x  >= 0f && leftEye.y  >= 0f;
        bool hasRight = rightEye.x >= 0f && rightEye.y >= 0f;
        if (!hasLeft && !hasRight) return;

        float eyeDist = hasLeft && hasRight
            ? Vector2.Distance(leftEye, rightEye) : 0.25f;
        float eyeAreaR = eyeDist * 0.22f; // 目の領域半径（正規化）

        if (hasLeft)  ApplyEyeCleanup(pixels, w, h, leftEye, eyeAreaR);
        if (hasRight) ApplyEyeCleanup(pixels, w, h, rightEye, eyeAreaR);
    }

    /// <summary>単一の目をクリーンアップ + キャッチライト追加</summary>
    static void ApplyEyeCleanup(Color[] pixels, int w, int h, Vector2 eyePos, float radiusNorm)
    {
        int ecx = Mathf.RoundToInt(eyePos.x * w);
        int ecy = Mathf.RoundToInt(eyePos.y * h);
        int radius = Mathf.RoundToInt(radiusNorm * w);
        int r2 = radius * radius;

        int xMin = Mathf.Max(0, ecx - radius);
        int xMax = Mathf.Min(w - 1, ecx + radius);
        int yMin = Mathf.Max(0, ecy - radius);
        int yMax = Mathf.Min(h - 1, ecy + radius);

        // 黒目の中心を探す（最も暗いピクセルのクラスター中心）
        float darkSumX = 0, darkSumY = 0, darkCount = 0;
        for (int y = yMin; y <= yMax; y++)
        {
            for (int x = xMin; x <= xMax; x++)
            {
                int dx = x - ecx;
                int dy = y - ecy;
                if (dx * dx + dy * dy > r2) continue;
                Color c = pixels[y * w + x];
                float lum = c.r * 0.299f + c.g * 0.587f + c.b * 0.114f;
                if (lum < 0.30f)
                {
                    darkSumX += x;
                    darkSumY += y;
                    darkCount++;
                }
            }
        }

        // 黒目中心（見つからなければ目の位置をそのまま使う）
        int pupilCX = darkCount > 5 ? Mathf.RoundToInt(darkSumX / darkCount) : ecx;
        int pupilCY = darkCount > 5 ? Mathf.RoundToInt(darkSumY / darkCount) : ecy;
        int pupilR  = Mathf.Max(3, radius / 3); // 黒目半径

        // Pass 1: 白目のクリア化 + 黒目のコントラスト強調
        for (int y = yMin; y <= yMax; y++)
        {
            for (int x = xMin; x <= xMax; x++)
            {
                int dx = x - ecx;
                int dy = y - ecy;
                int d2 = dx * dx + dy * dy;
                if (d2 > r2) continue;

                float dist = Mathf.Sqrt(d2) / (float)radius;
                float influence = 1f - dist;
                influence = influence * influence;

                int idx = y * w + x;
                Color c = pixels[idx];
                float lum = c.r * 0.299f + c.g * 0.587f + c.b * 0.114f;

                // 黒目内: コントラスト強め（より黒く）
                int pdx = x - pupilCX;
                int pdy = y - pupilCY;
                float pupilDist = Mathf.Sqrt(pdx * pdx + pdy * pdy) / (float)pupilR;
                if (pupilDist < 1.0f)
                {
                    float darken = (1f - pupilDist) * 0.6f; // 中心ほど強く黒に
                    pixels[idx] = new Color(
                        Mathf.Clamp01(c.r - darken),
                        Mathf.Clamp01(c.g - darken),
                        Mathf.Clamp01(c.b - darken),
                        c.a);
                }
                // 白目: 白を明るくクリアに
                else if (lum > 0.45f && dist > 0.2f)
                {
                    float whiten = influence * 0.30f;
                    pixels[idx] = new Color(
                        Mathf.Clamp01(c.r + whiten),
                        Mathf.Clamp01(c.g + whiten),
                        Mathf.Clamp01(c.b + whiten),
                        c.a);
                }
            }
        }

        // Pass 2: ジブリ風2点キャッチライト（黒目の上部に大小2点の白い光）
        // ハイライト1: 右上（大きめ）
        int h1r = Mathf.Max(2, Mathf.RoundToInt(w * 0.01f));
        int h1x = pupilCX + pupilR / 4;
        int h1y = pupilCY - pupilR / 2;
        DrawHighlightDot(pixels, w, h, h1x, h1y, h1r);

        // ハイライト2: 左下（小さめ）
        int h2r = Mathf.Max(1, Mathf.RoundToInt(w * 0.006f));
        int h2x = pupilCX - pupilR / 3;
        int h2y = pupilCY - pupilR / 4;
        DrawHighlightDot(pixels, w, h, h2x, h2y, h2r);
    }

    /// <summary>パキッとした白い光点を描画</summary>
    static void DrawHighlightDot(Color[] pixels, int w, int h, int cx, int cy, int radius)
    {
        int r2 = radius * radius;
        for (int y = Mathf.Max(0, cy - radius); y <= Mathf.Min(h - 1, cy + radius); y++)
        {
            for (int x = Mathf.Max(0, cx - radius); x <= Mathf.Min(w - 1, cx + radius); x++)
            {
                int dx = x - cx;
                int dy = y - cy;
                if (dx * dx + dy * dy > r2) continue;

                float dist = Mathf.Sqrt(dx * dx + dy * dy) / (float)radius;
                float strength = 1f - dist * dist; // 中心でパキッと白
                strength = Mathf.Clamp01(strength);

                int idx = y * w + x;
                Color c = pixels[idx];
                pixels[idx] = new Color(
                    Mathf.Lerp(c.r, 1f, strength * 0.95f),
                    Mathf.Lerp(c.g, 1f, strength * 0.95f),
                    Mathf.Lerp(c.b, 1f, strength * 0.95f),
                    c.a);
            }
        }
    }

    // ─── 口の単純化 ───

    /// <summary>口を滑らかなピンク1色で塗りつぶし、小さく可愛い唇形にデフォルメ</summary>
    static void ApplyMouthSimplify(Color[] pixels, int w, int h,
        Vector2 leftEye, Vector2 rightEye, FaceLandmarkResult? landmarks)
    {
        bool hasLeft  = leftEye.x  >= 0f && leftEye.y  >= 0f;
        bool hasRight = rightEye.x >= 0f && rightEye.y >= 0f;
        if (!hasLeft && !hasRight) return;

        // 口の位置推定
        Vector2 eyeCenter;
        float eyeDist;
        if (hasLeft && hasRight)
        {
            eyeCenter = (leftEye + rightEye) * 0.5f;
            eyeDist = Vector2.Distance(leftEye, rightEye);
        }
        else
        {
            eyeCenter = hasLeft ? leftEye : rightEye;
            eyeDist = 0.25f;
        }

        // 口の中心: ランドマークがあればそれを使う、なければ目の下に推定
        float mouthCX, mouthCY;
        if (landmarks.HasValue && landmarks.Value.mouthCenter.x > 0)
        {
            mouthCX = landmarks.Value.mouthCenter.x;
            mouthCY = landmarks.Value.mouthCenter.y;
        }
        else
        {
            mouthCX = eyeCenter.x;
            mouthCY = eyeCenter.y + eyeDist * 1.0f;
        }

        int mcx = Mathf.RoundToInt(mouthCX * w);
        int mcy = Mathf.RoundToInt(mouthCY * h);

        // 口のサイズ（小さく可愛い）
        float mouthRadX = eyeDist * 0.18f * w; // 横幅
        float mouthRadY = eyeDist * 0.08f * h; // 縦幅（薄め）

        // 唇の色: 透明感のある薄ピンク
        Color lipColor = new Color(0.92f, 0.62f, 0.65f);

        // Pass 1: 元の口周辺を肌色でフラットに塗り潰す（シワ消し）
        float eraserRadX = mouthRadX * 1.6f;
        float eraserRadY = mouthRadY * 2.2f;
        Color skinFlat = new Color(0.96f, 0.84f, 0.77f);

        for (int y = Mathf.Max(0, (int)(mcy - eraserRadY)); y <= Mathf.Min(h - 1, (int)(mcy + eraserRadY)); y++)
        {
            for (int x = Mathf.Max(0, (int)(mcx - eraserRadX)); x <= Mathf.Min(w - 1, (int)(mcx + eraserRadX)); x++)
            {
                float edx = (x - mcx) / eraserRadX;
                float edy = (y - mcy) / eraserRadY;
                float ed2 = edx * edx + edy * edy;
                if (ed2 > 1f) continue;

                float t = 1f - Mathf.Sqrt(ed2);
                t = t * t * (3f - 2f * t); // smoothstep
                float blend = t * 0.6f; // 完全には消さず自然に

                int idx = y * w + x;
                Color c = pixels[idx];
                pixels[idx] = new Color(
                    Mathf.Lerp(c.r, skinFlat.r, blend),
                    Mathf.Lerp(c.g, skinFlat.g, blend),
                    Mathf.Lerp(c.b, skinFlat.b, blend),
                    c.a);
            }
        }

        // Pass 2: 小さく可愛い唇を描画（楕円、上唇のアーチ付き）
        for (int y = Mathf.Max(0, (int)(mcy - mouthRadY * 1.5f)); y <= Mathf.Min(h - 1, (int)(mcy + mouthRadY * 1.5f)); y++)
        {
            for (int x = Mathf.Max(0, (int)(mcx - mouthRadX)); x <= Mathf.Min(w - 1, (int)(mcx + mouthRadX)); x++)
            {
                float edx = (x - mcx) / mouthRadX;
                float edy = (y - mcy) / mouthRadY;

                // 上唇のアーチ（キューピッドの弓）
                if (edy < 0)
                    edy -= Mathf.Abs(edx) * 0.3f; // 中央が下がる形

                float ed2 = edx * edx + edy * edy;
                if (ed2 > 1f) continue;

                float t = 1f - Mathf.Sqrt(ed2);
                t = t * t * (3f - 2f * t);

                int idx = y * w + x;
                Color c = pixels[idx];
                pixels[idx] = new Color(
                    Mathf.Lerp(c.r, lipColor.r, t * 0.75f),
                    Mathf.Lerp(c.g, lipColor.g, t * 0.75f),
                    Mathf.Lerp(c.b, lipColor.b, t * 0.75f),
                    c.a);
            }
        }
    }

    // ─── ジブリ風3階調セルシェーディング ───

    /// <summary>
    /// 写真の複雑なグラデーションを3階調（影・中間・ハイライト）に単純化。
    /// 段差の境界をsmoothstepで意図的にぼかし、手描きのような柔らかさを出す。
    /// </summary>
    static void ApplyGhibliCelShading(Color[] pixels, int w, int h)
    {
        for (int i = 0; i < pixels.Length; i++)
        {
            Color c = pixels[i];
            if (c.a < 0.01f) continue;

            float lum = c.r * 0.299f + c.g * 0.587f + c.b * 0.114f;

            // 3段階: 影 / 中間 / ハイライト
            // smoothstepで境界を柔らかくぼかす
            float shadowBlend = 1f - Smoothstep(
                GHIBLI_SHADOW_THRESH - GHIBLI_TONE_SOFTNESS,
                GHIBLI_SHADOW_THRESH + GHIBLI_TONE_SOFTNESS, lum);
            float highlightBlend = Smoothstep(
                GHIBLI_HIGHLIGHT_THRESH - GHIBLI_TONE_SOFTNESS,
                GHIBLI_HIGHLIGHT_THRESH + GHIBLI_TONE_SOFTNESS, lum);

            // 中間調はそのまま、影は暗く、ハイライトはわずかに明るく
            float scale = 1f;
            scale = Mathf.Lerp(scale, GHIBLI_SHADOW_DARKEN, shadowBlend);
            scale = Mathf.Lerp(scale, GHIBLI_HIGHLIGHT_LIFT, highlightBlend);

            pixels[i] = new Color(
                Mathf.Clamp01(c.r * scale),
                Mathf.Clamp01(c.g * scale),
                Mathf.Clamp01(c.b * scale),
                c.a);
        }
    }

    // ─── 白飛び→ピーチ引き戻しトーンマッピング ───

    /// <summary>
    /// 輝度が高すぎる領域を柔らかいピーチカラーに引き戻す。
    /// 「白」ではなく「暖かいピーチ」をハイライトの上限にする。
    /// </summary>
    static void ApplyPeachToneMapping(Color[] pixels, int w, int h)
    {
        // ピーチカラー（白飛びの代わりにこの色へ引き戻す）
        Color peach = new Color(0.96f, 0.87f, 0.82f);

        for (int i = 0; i < pixels.Length; i++)
        {
            Color c = pixels[i];
            if (c.a < 0.01f) continue;

            float lum = c.r * 0.299f + c.g * 0.587f + c.b * 0.114f;
            if (lum <= GHIBLI_PEACH_CLAMP) continue;

            // 輝度が閾値を超えた分だけ、ピーチカラーにブレンド
            float excess = (lum - GHIBLI_PEACH_CLAMP) / (1f - GHIBLI_PEACH_CLAMP);
            excess = Mathf.Clamp01(excess);
            float blend = excess * excess; // 二次関数で柔らかく効かせる

            pixels[i] = new Color(
                Mathf.Lerp(c.r, peach.r, blend),
                Mathf.Lerp(c.g, peach.g, blend),
                Mathf.Lerp(c.b, peach.b, blend),
                c.a);
        }
    }

    // ─── ジブリ風ほっぺピンク ───

    /// <summary>目の下にジブリ特有のふんわりピンクの頬紅</summary>
    static void ApplyGhibliCheekPink(Color[] pixels, int w, int h, Vector2 leftEye, Vector2 rightEye)
    {
        bool hasLeft  = leftEye.x  >= 0f && leftEye.y  >= 0f;
        bool hasRight = rightEye.x >= 0f && rightEye.y >= 0f;
        if (!hasLeft && !hasRight) return;

        float eyeDist = hasLeft && hasRight
            ? Vector2.Distance(leftEye, rightEye) : 0.25f;
        float cheekRadius = eyeDist * 0.30f;

        if (hasLeft)
        {
            float cx = leftEye.x - eyeDist * 0.05f;
            float cy = leftEye.y + eyeDist * 0.55f;
            ApplyGhibliCheekSpot(pixels, w, h, cx, cy, cheekRadius, GHIBLI_CHEEK_PINK);
        }
        if (hasRight)
        {
            float cx = rightEye.x + eyeDist * 0.05f;
            float cy = rightEye.y + eyeDist * 0.55f;
            ApplyGhibliCheekSpot(pixels, w, h, cx, cy, cheekRadius, GHIBLI_CHEEK_PINK);
        }
    }

    /// <summary>単一ほっぺピンクスポット（ジブリ風: サーモンピンクを柔らかく）</summary>
    static void ApplyGhibliCheekSpot(Color[] pixels, int w, int h,
        float cx, float cy, float radiusNorm, float strength)
    {
        // ジブリ風チーク色: 柔らかいサーモンピンク
        Color cheekColor = new Color(0.95f, 0.70f, 0.68f);

        int centerX = Mathf.RoundToInt(cx * w);
        int centerY = Mathf.RoundToInt(cy * h);
        int radius  = Mathf.RoundToInt(radiusNorm * w);
        int r2      = radius * radius;

        int xMin = Mathf.Max(0, centerX - radius);
        int xMax = Mathf.Min(w - 1, centerX + radius);
        int yMin = Mathf.Max(0, centerY - radius);
        int yMax = Mathf.Min(h - 1, centerY + radius);

        for (int y = yMin; y <= yMax; y++)
        {
            for (int x = xMin; x <= xMax; x++)
            {
                int dx = x - centerX;
                int dy = y - centerY;
                int dist2 = dx * dx + dy * dy;
                if (dist2 > r2) continue;

                float dist = Mathf.Sqrt(dist2) / radius;
                float blend = Mathf.Exp(-dist * dist * 2.5f) * strength;

                int idx = y * w + x;
                Color c = pixels[idx];
                pixels[idx] = new Color(
                    Mathf.Lerp(c.r, cheekColor.r, blend),
                    Mathf.Lerp(c.g, cheekColor.g, blend),
                    Mathf.Lerp(c.b, cheekColor.b, blend),
                    c.a);
            }
        }
    }

    /// <summary>バイリニア補間でworkSize x workSizeに縮小</summary>
    static Color[] DownscaleToWorkSize(Color[] src, int srcW, int srcH, int dstSize)
    {
        Color[] dst = new Color[dstSize * dstSize];
        float scaleX = (float)srcW / dstSize;
        float scaleY = (float)srcH / dstSize;

        for (int y = 0; y < dstSize; y++)
        {
            for (int x = 0; x < dstSize; x++)
            {
                float sx = x * scaleX;
                float sy = y * scaleY;
                dst[y * dstSize + x] = BilinearSample(src, srcW, srcH, sx, sy);
            }
        }
        return dst;
    }

    /// <summary>1Dガウスカーネル生成</summary>
    static float[] MakeGaussianKernel(int radius)
    {
        int size = radius * 2 + 1;
        float[] kernel = new float[size];
        float sigma = radius * 0.5f;
        float sum = 0f;

        for (int i = 0; i < size; i++)
        {
            float d = i - radius;
            kernel[i] = Mathf.Exp(-d * d / (2f * sigma * sigma));
            sum += kernel[i];
        }
        // 正規化
        for (int i = 0; i < size; i++)
            kernel[i] /= sum;

        return kernel;
    }

    /// <summary>分離可能ガウシアンブラー（水平→垂直の2パス）</summary>
    static Color[] GaussianBlurSeparable(Color[] src, int w, int h, int radius)
    {
        float[] kernel = MakeGaussianKernel(radius);
        int kSize = radius * 2 + 1;
        Color[] temp = new Color[w * h];
        Color[] dst = new Color[w * h];

        // 水平パス
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float r = 0f, g = 0f, b = 0f, a = 0f;
                for (int k = 0; k < kSize; k++)
                {
                    int sx = Mathf.Clamp(x + k - radius, 0, w - 1);
                    Color c = src[y * w + sx];
                    r += c.r * kernel[k];
                    g += c.g * kernel[k];
                    b += c.b * kernel[k];
                    a += c.a * kernel[k];
                }
                temp[y * w + x] = new Color(r, g, b, a);
            }
        }

        // 垂直パス
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float r = 0f, g = 0f, b = 0f, a = 0f;
                for (int k = 0; k < kSize; k++)
                {
                    int sy = Mathf.Clamp(y + k - radius, 0, h - 1);
                    Color c = temp[sy * w + x];
                    r += c.r * kernel[k];
                    g += c.g * kernel[k];
                    b += c.b * kernel[k];
                    a += c.a * kernel[k];
                }
                dst[y * w + x] = new Color(r, g, b, a);
            }
        }

        return dst;
    }

    /// <summary>エッジ保持スムージング: ガウシアンブラー + 輝度ゲート混合</summary>
    static Color[] EdgePreservingSmooth(Color[] src, int w, int h)
    {
        Color[] blurred = GaussianBlurSeparable(src, w, h, FILTER_BLUR_RADIUS);

        for (int i = 0; i < src.Length; i++)
        {
            Color orig = src[i];
            Color blur = blurred[i];

            float lumOrig = orig.r * 0.299f + orig.g * 0.587f + orig.b * 0.114f;
            float lumBlur = blur.r * 0.299f + blur.g * 0.587f + blur.b * 0.114f;
            float diff = Mathf.Abs(lumOrig - lumBlur);

            // 輝度差が小さい → ブラーを採用（スムーズ化）
            // 輝度差が大きい → 元画像を維持（エッジ保存）
            float t = Mathf.Clamp01(diff / FILTER_EDGE_THRESHOLD);
            blurred[i] = Color.Lerp(blur, orig, t);
        }

        return blurred;
    }

    /// <summary>彩度ブースト: アニメ的な鮮やかな発色</summary>
    static void ApplySaturationBoost(Color[] pixels, int w, int h, float boost)
    {
        for (int i = 0; i < pixels.Length; i++)
        {
            Color c = pixels[i];
            if (c.a < 0.01f) continue;

            float gray = c.r * 0.299f + c.g * 0.587f + c.b * 0.114f;
            float r = gray + (c.r - gray) * boost;
            float g = gray + (c.g - gray) * boost;
            float b = gray + (c.b - gray) * boost;

            pixels[i] = new Color(Mathf.Clamp01(r), Mathf.Clamp01(g), Mathf.Clamp01(b), c.a);
        }
    }

    /// <summary>3階調セルシェーディング: smoothstepで滑らかに3トーン分離</summary>
    static void ApplyCelShading(Color[] pixels, int w, int h)
    {
        for (int i = 0; i < pixels.Length; i++)
        {
            Color c = pixels[i];
            if (c.a < 0.01f) continue;

            float lum = c.r * 0.299f + c.g * 0.587f + c.b * 0.114f;

            // smoothstepで影/ハイライトの遷移を滑らかに
            float shadowFactor = 1f - Smoothstep(DESSIN_SHADOW_THRESH - DESSIN_TONE_TRANSITION,
                                                  DESSIN_SHADOW_THRESH + DESSIN_TONE_TRANSITION, lum);
            float highlightFactor = Smoothstep(DESSIN_HIGHLIGHT_THRESH - DESSIN_TONE_TRANSITION,
                                               DESSIN_HIGHLIGHT_THRESH + DESSIN_TONE_TRANSITION, lum);

            // 影は暗くし、ハイライトは明るくする
            float scale = 1f;
            scale = Mathf.Lerp(scale, DESSIN_SHADOW_DARKEN, shadowFactor);
            scale = Mathf.Lerp(scale, DESSIN_HIGHLIGHT_BRIGHTEN, highlightFactor);

            pixels[i] = new Color(
                Mathf.Clamp01(c.r * scale),
                Mathf.Clamp01(c.g * scale),
                Mathf.Clamp01(c.b * scale),
                c.a);
        }
    }

    /// <summary>Sobel輪郭線検出 + 合成</summary>
    static void ApplyOutlines(Color[] pixels, Color[] edgeSrc, int w, int h)
    {
        // エッジ検出用の輝度バッファを生成（edgeSrcはブラー前のバッファ）
        float[] lum = new float[w * h];
        for (int i = 0; i < edgeSrc.Length; i++)
        {
            Color c = edgeSrc[i];
            lum[i] = c.r * 0.299f + c.g * 0.587f + c.b * 0.114f;
        }

        // 輪郭線色: 温かみのあるダークブラウン
        float outR = 0.15f, outG = 0.10f, outB = 0.08f;

        for (int y = 1; y < h - 1; y++)
        {
            for (int x = 1; x < w - 1; x++)
            {
                // Sobel 3x3
                float tl = lum[(y - 1) * w + (x - 1)];
                float tc = lum[(y - 1) * w + x];
                float tr = lum[(y - 1) * w + (x + 1)];
                float ml = lum[y * w + (x - 1)];
                float mr = lum[y * w + (x + 1)];
                float bl = lum[(y + 1) * w + (x - 1)];
                float bc = lum[(y + 1) * w + x];
                float br = lum[(y + 1) * w + (x + 1)];

                float gx = -tl - 2f * ml - bl + tr + 2f * mr + br;
                float gy = -tl - 2f * tc - tr + bl + 2f * bc + br;
                float edge = Mathf.Sqrt(gx * gx + gy * gy);

                // エッジ強度をsmoothstepで滑らかに
                float edgeAlpha = Smoothstep(DESSIN_EDGE_SENSITIVITY * 0.5f,
                                              DESSIN_EDGE_SENSITIVITY * 1.5f, edge);
                edgeAlpha *= DESSIN_OUTLINE_OPACITY;

                if (edgeAlpha > 0.01f)
                {
                    int idx = y * w + x;
                    Color c = pixels[idx];
                    pixels[idx] = new Color(
                        Mathf.Lerp(c.r, outR, edgeAlpha),
                        Mathf.Lerp(c.g, outG, edgeAlpha),
                        Mathf.Lerp(c.b, outB, edgeAlpha),
                        c.a);
                }
            }
        }
    }

    /// <summary>おでこ・頬にアニメ風スペキュラハイライト追加</summary>
    static void ApplySpecularHighlights(Color[] pixels, int w, int h,
        Vector2 leftEye, Vector2 rightEye, FaceLandmarkResult? landmarks)
    {
        bool hasLeftEye  = leftEye.x  >= 0f && leftEye.y  >= 0f;
        bool hasRightEye = rightEye.x >= 0f && rightEye.y >= 0f;

        if (!hasLeftEye && !hasRightEye) return;

        // 目の中点を計算
        Vector2 eyeCenter;
        if (hasLeftEye && hasRightEye)
            eyeCenter = (leftEye + rightEye) * 0.5f;
        else if (hasLeftEye)
            eyeCenter = leftEye;
        else
            eyeCenter = rightEye;

        float eyeDist = hasLeftEye && hasRightEye
            ? Vector2.Distance(leftEye, rightEye) : 0.25f;

        // おでこハイライト: 目の中点より上
        float foreheadX = eyeCenter.x;
        float foreheadY = eyeCenter.y - eyeDist * 0.8f;
        ApplyHighlightSpot(pixels, w, h, foreheadX, foreheadY, 0.06f, 0.25f);

        // 頬ハイライト
        if (hasLeftEye && hasRightEye)
        {
            // 左頬
            float cheekLX = leftEye.x - eyeDist * 0.15f;
            float cheekLY = leftEye.y + eyeDist * 0.5f;
            ApplyHighlightSpot(pixels, w, h, cheekLX, cheekLY, 0.04f, 0.15f);

            // 右頬
            float cheekRX = rightEye.x + eyeDist * 0.15f;
            float cheekRY = rightEye.y + eyeDist * 0.5f;
            ApplyHighlightSpot(pixels, w, h, cheekRX, cheekRY, 0.04f, 0.15f);
        }
    }

    /// <summary>単一ハイライトスポット: Screenブレンドで白を乗せる</summary>
    static void ApplyHighlightSpot(Color[] pixels, int w, int h,
        float cx, float cy, float radiusRatio, float intensity)
    {
        int centerX = Mathf.RoundToInt(cx * w);
        int centerY = Mathf.RoundToInt(cy * h);
        int radius  = Mathf.RoundToInt(radiusRatio * w);
        int r2      = radius * radius;

        int xMin = Mathf.Max(0, centerX - radius);
        int xMax = Mathf.Min(w - 1, centerX + radius);
        int yMin = Mathf.Max(0, centerY - radius);
        int yMax = Mathf.Min(h - 1, centerY + radius);

        for (int y = yMin; y <= yMax; y++)
        {
            for (int x = xMin; x <= xMax; x++)
            {
                int dx = x - centerX;
                int dy = y - centerY;
                int dist2 = dx * dx + dy * dy;
                if (dist2 > r2) continue;

                float dist = Mathf.Sqrt(dist2) / radius;
                // ガウシアンフォールオフ
                float strength = Mathf.Exp(-dist * dist * 3f) * intensity;

                int idx = y * w + x;
                Color c = pixels[idx];
                // Screen blend: 1 - (1 - c) * (1 - white*strength)
                float sr = 1f - (1f - c.r) * (1f - strength);
                float sg = 1f - (1f - c.g) * (1f - strength);
                float sb = 1f - (1f - c.b) * (1f - strength);
                pixels[idx] = new Color(Mathf.Clamp01(sr), Mathf.Clamp01(sg), Mathf.Clamp01(sb), c.a);
            }
        }
    }

    // ─── バイラテラルフィルタ（エッジ保存ぼかし）───

    /// <summary>
    /// バイラテラルフィルタ: 空間距離 + 色差の両方でウェイトを決定。
    /// 肌のヒゲ跡や質感を消しつつ、目・眉・輪郭のエッジは保存する。
    /// </summary>
    static Color[] ApplyBilateralFilter(Color[] src, int w, int h, int radius, float sigmaColor)
    {
        Color[] dst = new Color[w * h];
        float sigmaSpatial = radius * 0.5f;
        float invSigmaS2 = -1f / (2f * sigmaSpatial * sigmaSpatial);
        float invSigmaC2 = -1f / (2f * sigmaColor * sigmaColor);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                Color center = src[y * w + x];
                if (center.a < 0.01f) { dst[y * w + x] = center; continue; }

                float centerLum = center.r * 0.299f + center.g * 0.587f + center.b * 0.114f;

                float sumR = 0, sumG = 0, sumB = 0, sumW = 0;

                int yMin = Mathf.Max(0, y - radius);
                int yMax = Mathf.Min(h - 1, y + radius);
                int xMin = Mathf.Max(0, x - radius);
                int xMax = Mathf.Min(w - 1, x + radius);

                for (int ky = yMin; ky <= yMax; ky++)
                {
                    for (int kx = xMin; kx <= xMax; kx++)
                    {
                        Color neighbor = src[ky * w + kx];
                        float nLum = neighbor.r * 0.299f + neighbor.g * 0.587f + neighbor.b * 0.114f;

                        // 空間距離ウェイト
                        float dSpatial = (x - kx) * (x - kx) + (y - ky) * (y - ky);
                        float wSpatial = Mathf.Exp(dSpatial * invSigmaS2);

                        // 色差ウェイト（輝度ベース）
                        float dColor = (centerLum - nLum) * (centerLum - nLum);
                        float wColor = Mathf.Exp(dColor * invSigmaC2);

                        float weight = wSpatial * wColor;
                        sumR += neighbor.r * weight;
                        sumG += neighbor.g * weight;
                        sumB += neighbor.b * weight;
                        sumW += weight;
                    }
                }

                if (sumW > 0.0001f)
                    dst[y * w + x] = new Color(sumR / sumW, sumG / sumW, sumB / sumW, center.a);
                else
                    dst[y * w + x] = center;
            }
        }
        return dst;
    }

    /// <summary>smoothstep補間 (GLSL互換)</summary>
    static float Smoothstep(float edge0, float edge1, float x)
    {
        float t = Mathf.Clamp01((x - edge0) / (edge1 - edge0));
        return t * t * (3f - 2f * t);
    }

    // ─── ユーティリティ ───

    /// <summary>バイリニア補間サンプリング</summary>
    static Color BilinearSample(Color[] pixels, int w, int h, float x, float y)
    {
        int x0 = Mathf.FloorToInt(x);
        int y0 = Mathf.FloorToInt(y);
        int x1 = x0 + 1;
        int y1 = y0 + 1;
        float fx = x - x0;
        float fy = y - y0;

        Color c00 = SafeGet(pixels, w, h, x0, y0);
        Color c10 = SafeGet(pixels, w, h, x1, y0);
        Color c01 = SafeGet(pixels, w, h, x0, y1);
        Color c11 = SafeGet(pixels, w, h, x1, y1);

        Color top = Color.Lerp(c00, c10, fx);
        Color bot = Color.Lerp(c01, c11, fx);
        return Color.Lerp(top, bot, fy);
    }

    static Color SafeGet(Color[] pixels, int w, int h, int x, int y)
    {
        x = Mathf.Clamp(x, 0, w - 1);
        y = Mathf.Clamp(y, 0, h - 1);
        return pixels[y * w + x];
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

    void BlitSpriteToCanvas(Color[] canvas, Sprite sprite, int dstX, int dstY, int dstW, int dstH, bool coverMode = false)
    {
        var tex = sprite.texture;
        int srcW = tex.width;
        int srcH = tex.height;
        Color[] srcPixels = tex.GetPixels();

        // cover mode: アスペクト比を維持して領域を埋める（はみ出し部分はクロップ）
        float cvScale = 1f, cvOffX = 0f, cvOffY = 0f;
        if (coverMode && (srcW != dstW || srcH != dstH))
        {
            cvScale = Mathf.Max((float)dstW / srcW, (float)dstH / srcH);
            cvOffX = (srcW - dstW / cvScale) * 0.5f;
            cvOffY = (srcH - dstH / cvScale) * 0.5f;
        }

        for (int dy = 0; dy < dstH; dy++)
        {
            for (int dx = 0; dx < dstW; dx++)
            {
                int px = dstX + dx;
                int py = dstY + dy;
                if (px < 0 || px >= TEX_SIZE || py < 0 || py >= TEX_SIZE) continue;

                int sx, sy;
                if (coverMode)
                {
                    sx = Mathf.Clamp((int)(cvOffX + (float)dx / cvScale), 0, srcW - 1);
                    sy = Mathf.Clamp((int)(cvOffY + (float)dy / cvScale), 0, srcH - 1);
                }
                else
                {
                    float u = (float)dx / dstW;
                    float v = (float)dy / dstH;
                    sx = Mathf.Clamp((int)(u * srcW), 0, srcW - 1);
                    sy = Mathf.Clamp((int)(v * srcH), 0, srcH - 1);
                }
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
        return "BabySynth/Swaddles/Default_Wear";
    }

    /// <summary>
    /// BabyWearスプライトを確実に読み込む（spriteMode:2対応）
    /// </summary>
    public static Sprite LoadWearSprite(BabyRank rank)
    {
        string wearPath = GetWearPath(rank);
        Sprite s = Resources.Load<Sprite>(wearPath);
        if (s != null) return s;
        // spriteMode:2 (Multiple)の場合、LoadAllで取得
        Sprite[] all = Resources.LoadAll<Sprite>(wearPath);
        if (all != null && all.Length > 0) return all[0];
        return null;
    }

    /// <summary>
    /// Default_Wear PNGの透過エリアをスキャンし、顔穴の中心・半径を自動検出する。
    /// スキャンライン方式: 各行で左端・右端の不透明ピクセルを見つけ、
    /// その間にある透明ピクセルのみを顔穴として検出する（外周の透明背景を無視）。
    /// </summary>
    void DetectFaceHoleFromWear()
    {
        faceHoleDetected = true;

        Sprite wearSprite = LoadWearSprite(BabyRank.D);
        if (wearSprite == null || !wearSprite.texture.isReadable)
        {
            Debug.LogWarning("[BabySynthesizer] DetectFaceHoleFromWear: Failed to load wear sprite");
            return;
        }

        var tex = wearSprite.texture;
        int w = tex.width;
        int h = tex.height;
        Color[] pixels = tex.GetPixels();

        // スキャンライン方式: おくるみ内部の「連続した」透明ピクセル領域（顔穴）のみ検出
        // デコレーション透過（スキャラップ、シワ等）は短い連続長なのでフィルタする
        int holeMinX = w, holeMaxX = 0, holeMinY = h, holeMaxY = 0;
        int holeCount = 0;
        int minRunLength = Mathf.Max(w / 20, 5); // 最低連続長: 画像幅の5%

        for (int y = 0; y < h; y++)
        {
            // この行の左端・右端の不透明ピクセルを探す
            int leftOpaque = -1, rightOpaque = -1;
            for (int x = 0; x < w; x++)
            {
                if (pixels[y * w + x].a >= 0.5f)
                {
                    if (leftOpaque < 0) leftOpaque = x;
                    rightOpaque = x;
                }
            }

            // 不透明ピクセルがない行 → スキップ
            if (leftOpaque < 0) continue;

            // 左端と右端の間の連続透明ランを検出（短いランは無視）
            int runStart = -1;
            for (int x = leftOpaque + 1; x <= rightOpaque; x++)
            {
                bool isTransparent = (x < rightOpaque) && (pixels[y * w + x].a < 0.1f);
                if (isTransparent)
                {
                    if (runStart < 0) runStart = x;
                }
                else
                {
                    // ランの終了 — 長さチェック
                    if (runStart >= 0)
                    {
                        int runLen = x - runStart;
                        if (runLen >= minRunLength)
                        {
                            // この連続透明領域は顔穴の一部
                            if (runStart < holeMinX) holeMinX = runStart;
                            if (x - 1 > holeMaxX) holeMaxX = x - 1;
                            if (y < holeMinY) holeMinY = y;
                            if (y > holeMaxY) holeMaxY = y;
                            holeCount += runLen;
                        }
                    }
                    runStart = -1;
                }
            }
        }

        if (holeCount < 10 || holeMaxX <= holeMinX || holeMaxY <= holeMinY) return;

        // 中心と半径を比率で算出
        float cx = (holeMinX + holeMaxX) * 0.5f / w;
        float cy = (holeMinY + holeMaxY) * 0.5f / h;
        float rx = (holeMaxX - holeMinX) * 0.5f / w;
        float ry = (holeMaxY - holeMinY) * 0.5f / h;

        // 合理性チェック: 顔穴はおくるみの小さな楕円のはず（半径が画像の22%を超えたらデフォルトに戻す）
        const float MAX_FACE_HOLE_R = 0.22f;
        if (rx > MAX_FACE_HOLE_R || ry > MAX_FACE_HOLE_R)
        {
            Debug.LogWarning($"[BabySynthesizer] Face hole too large: ({rx:F3},{ry:F3}) — using defaults (0.13,0.12)");
            return;
        }

        FACE_HOLE_CX = cx;
        FACE_HOLE_CY = cy;
        FACE_HOLE_RX = rx;
        FACE_HOLE_RY = ry;

        Debug.Log($"[BabySynthesizer] Face hole detected: center=({cx:F3},{cy:F3}), radius=({rx:F3},{ry:F3})");
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
