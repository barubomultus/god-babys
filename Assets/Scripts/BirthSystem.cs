using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Collections;
using System.IO;
using UIE = UnityEngine.UIElements;

public class BirthSystem : MonoBehaviour
{
    [Header("UI Reference")]
    public Canvas canvas;

    [Header("Baby Face")]
    public RectTransform babyFace;
    public Image babyFaceImage;

    [Header("Baby Sprites (patterns)")]
    public Sprite[] babySprites;

    [Header("Parent Face Sprites (6 each)")]
    public Sprite[] fatherSprites;
    public Sprite[] motherSprites;

    [Header("Button Controls")]
    public GameObject generateLifeButton;
    public GameObject anotherGalButton;
    public GameObject gotoBattleButton;

    // UI Toolkit overlay
    UIE.PanelSettings overlayPanelSettings;
    UIE.VisualElement overlayRoot;
    UIE.VisualElement menuOverlayEl;
    UIE.VisualElement nameInputOverlayEl;
    UIE.TextField nameTextField;
    UIE.VisualElement saveConfirmOverlayEl;

    // 自動生成される親UI (UI Toolkit)
    UIE.VisualElement parentPanel;
    UIE.VisualElement fatherCard;
    UIE.VisualElement motherCard;
    UIE.VisualElement fatherFaceImage;
    UIE.Label fatherNameText;
    UIE.Label fatherIntroText;
    UIE.VisualElement motherFaceImage;
    UIE.Label motherNameText;
    UIE.Label motherIntroText;
    UIE.VisualElement nextButtonEl;
    bool waitingForNext;
    UIE.VisualElement birthResultCard;
    UIE.VisualElement statusCardObj;
    UIE.Label childStatusText;
    UIE.Label genderLabelEl;
    UIE.VisualElement traitBadgeRow;
    UIE.Label babyNameLabel;
    UIE.Label babyBioLabel;
    UIE.VisualElement memoTapeEl;
    UIE.Label memoTextLabel;
    UIE.Label parentsLineLabel;

    // ボタンモード（運命を吹き込む / 名前をつける）
    bool isUploadMode = true;

    // フラッシュ演出用
    Image flashOverlay;

    // 稲妻演出用
    GameObject lightningContainer;

    // SE
    AudioSource seSource;
    AudioSource seBabySource; // ピッチ変化用の専用AudioSource
    AudioClip seKettei;
    AudioClip seThunder;
    AudioClip seBabyVoice;
    AudioClip seKirakira;
    AudioClip seLevelUp;
    AudioClip sePeta; // 名前テープ貼り付けSE
    AudioClip seKirakira4; // アコーディオン展開SE

    // 名前テープ演出
    UIE.VisualElement nameTapeEl;
    UIE.Label nameTapeLabel;
    UIE.Button shareBtn;
    bool isShareMode;

    // ?マーク
    TextMeshProUGUI introText;
    GameObject introPanel;

    // 親ステータスアコーディオン
    UIE.Button statusCheckButton;
    UIE.VisualElement accordionContainer;
    bool isAccordionOpen;
    Coroutine accordionAnimCoroutine;
    ParentData currentAccordionParent;
    UIE.VisualElement currentAccordionTargetCard;

    // カードフリップ（Bio表示）
    UIE.VisualElement cardBackFace;
    bool isCardFlipped;
    Coroutine flipAnimCoroutine;
    AudioClip seCardFlip;

    // 魔法陣 (宿命を定めるボタン)
    UIE.VisualElement magicCircleEl;
    Coroutine magicCircleRotateCoroutine;

    // ストーリーポップアップ (絵本風モーダル)
    UIE.VisualElement storyPopupOverlay;
    Coroutine storyPopupTypewriterCoroutine;
    bool isStoryPopupOpen;

    // キャラクター背景パーティクル
    Coroutine fatherParticleCoroutine;
    Coroutine motherParticleCoroutine;

    // ステータスチェックボタンのパルスアニメーション
    Coroutine statusCheckPulseCoroutine;

    // ジャイロパララックス
    Vector2 smoothAccel;
    const float PARALLAX_FACE_AMOUNT = 8f;   // 顔画像の移動量(px)
    const float PARALLAX_PARTICLE_AMOUNT = 15f; // パーティクルの移動量(px)
    const float PARALLAX_SMOOTH = 5f;         // スムージング係数

    // 画像アップロードボタン
    UIE.Button uploadImageBtn;
    UIE.VisualElement customBabyImageEl;

    // BabySynthesizer合成結果の表示要素
    UIE.VisualElement synthBabyImageEl;

    // BabySynthesizer (SpriteRenderer + SpriteMask layer compositing)
    BabySynthesizer babySynthesizer;

    // 顔調整オーバーレイ
    UIE.VisualElement faceAdjustOverlayEl;
    UIE.VisualElement facePreviewImage;
    float facePreviewCenterX, facePreviewCenterY; // プレビュー内の顔中心座標(px)
    float faceAdjOffsetX, faceAdjOffsetY, faceAdjScale = 1f;
    bool isDraggingFace;
    Vector2 dragStartPos;
    float dragStartOffsetX, dragStartOffsetY;
    Texture2D pendingFaceTexture;
    Texture2D originalFaceTexture; // フィルター前の元画像（非破壊保持）
    int originalFaceW, originalFaceH; // 元画像サイズ（クロップ補正用）
    Texture2D hollowWearTexture;
    UIE.VisualElement filterLoadingOverlayEl;

    // 目タップ関連
    UIE.VisualElement eyeMarkOverlayEl;
    UIE.VisualElement eyeMarkPreview;
    Vector2 eyePos1 = new Vector2(-1, -1); // 左目（正規化座標 0-1）
    Vector2 eyePos2 = new Vector2(-1, -1); // 右目
    int eyeTapCount;
    float eyeMarkSavedOffsetX, eyeMarkSavedOffsetY, eyeMarkSavedScale;
    FaceLandmarkResult? detectedFaceLandmarks; // ネイティブ顔検出結果
    SynthesizeParams lastSynthParams;

    // ぷにぷにインタラクション
    bool puniInteractionEnabled;
    UIE.VisualElement faceInteractionClip; // 顔穴位置のクリップコンテナ（overflow:hidden+楕円）
    UIE.VisualElement leftPupilEl;
    UIE.VisualElement rightPupilEl;
    Vector2 leftPupilBasePos;  // 左目瞳の基準位置（faceInteractionClipローカル座標）
    Vector2 rightPupilBasePos; // 右目瞳の基準位置
    Vector2 leftPupilOffset;   // 現在の左瞳オフセット
    Vector2 rightPupilOffset;  // 現在の右瞳オフセット
    Coroutine puniSquishCoroutine;
    UIE.VisualElement diagnosisOverlayEl;
    Coroutine diagnosisTypewriterCoroutine;

    // DrawFace と同じ定数からプレビューサイズを計算（BabySynthesizerの検出値を使用）
    float GetFacePreviewUniform()
    {
        float rx = babySynthesizer != null ? babySynthesizer.GetFaceHoleRX() : 0.13f;
        float ry = babySynthesizer != null ? babySynthesizer.GetFaceHoleRY() : 0.12f;
        return Mathf.Max(rx, ry) * 1.05f * 2f * 700f;
    }
    float GetFaceHoleTopPct()
    {
        float cy = babySynthesizer != null ? babySynthesizer.GetFaceHoleCY() : 0.68f;
        return (1f - cy) * 100f;
    }

    // 背景
    Image mainBgImg;

    // アニメーション中フラグ
    bool isAnimating;
    bool skipRequested;

    string selectedGender;

    // 名前入力UI
    string enteredName;
    bool waitingForNameInput;
    bool nameInputCancelled;

    // セーブ確認UI
    bool waitingForSaveConfirm;
    bool saveConfirmResult;

    // 父親6パターン
    static readonly ParentData[] Fathers = new[]
    {
        new ParentData("タケシ",   "takeshi", 70, 30, 180, 40, 85, 30, 20, new Color(0.9f, 0.7f, 0.5f), "元・格闘技世界王者 / 握力: 180kg"),
        new ParentData("ユウキ",   "yuuki",   50, 50, 150, 70, 60, 50, 40, new Color(0.6f, 0.8f, 1.0f), "天才ハッカー / 特許数: 3,200件"),
        new ParentData("ゴウ",     "gou",     80, 25, 190, 30, 90, 20, 15, new Color(1.0f, 0.5f, 0.4f), "つよい ぼうけんか / ぼうけんりょく: 計測不能"),
        new ParentData("シンジ",   "shinji",  30, 70, 140, 95, 35, 60, 30, new Color(0.7f, 0.7f, 1.0f), "ノーベル賞3回受賞 / IQ: 250"),
        new ParentData("リョウマ", "ryouma",  60, 60, 170, 55, 70, 70, 95, new Color(0.5f, 1.0f, 0.6f), "総資産: 43兆円 / 世界一の実業家"),
        new ParentData("テツヤ",   "tetuya",  45, 45, 160, 60, 55, 80, 60, new Color(1.0f, 0.9f, 0.5f), "伝説のロックスター / ファン数: 8億人"),
    };

    // 母親6パターン
    static readonly ParentData[] Mothers = new[]
    {
        new ParentData("サクラ",   "sakura",  40, 60, 150, 60, 65, 40, 25, new Color(1.0f, 0.7f, 0.8f), "つよーい おかあさん / ぜんせん むてき"),
        new ParentData("ヒナタ",   "hinata",  50, 50, 145, 70, 60, 55, 90, new Color(0.8f, 0.6f, 1.0f), "総資産: 28兆円 / 美容帝国CEO"),
        new ParentData("アキラ",   "akira",   65, 30, 160, 45, 80, 45, 35, new Color(1.0f, 0.6f, 0.4f), "五輪金メダル7個 / 100m走: 10.1秒"),
        new ParentData("ミサト",   "misato",  35, 55, 140, 90, 30, 65, 45, new Color(0.6f, 0.9f, 1.0f), "量子物理学者 / IQ: 270"),
        new ParentData("カエデ",   "kaede",   25, 70, 130, 85, 40, 50, 50, new Color(0.5f, 1.0f, 0.7f), "天才外科医 / 手術成功率: 100%"),
        new ParentData("ルナ",     "luna",    55, 40, 135, 50, 70, 75, 70, new Color(1.0f, 1.0f, 0.6f), "世界的スーパーモデル / 身長: 180cm"),
    };

    // 新父親6パターン
    static readonly ParentData[] NewFathers = new[]
    {
        new ParentData("ゼニガタ", "zenigata", 30, 90, 140, 99, 85, 99, 100, new Color(1.0f, 0.85f, 0.3f), "石油王 / 口癖は『金で買えないものはない』。ゆりかごはプラチナ製。"),
        new ParentData("ツクモ",   "tukumo",  20, 35, 110,  5, 99, 60, 100, new Color(0.6f, 0.4f, 1.0f), "自称・予言者 / IQ300。常に宇宙と交信しており、育児中も上の空。"),
        new ParentData("サトウ",   "satou",    55, 50, 150, 50, 55, 45, 100, new Color(0.7f, 0.8f, 0.7f), "中堅企業の係長 / 趣味は洗車。突出した能力はないが、安定した愛を注ぐ。"),
        new ParentData("イワオ",   "iwao",     95, 85, 200, 20, 15, 10, 100, new Color(0.8f, 0.6f, 0.4f), "元・土木作業員 / 素手で巨大な岩を砕くが、極度の貧乏でプロテインが買えない。"),
        new ParentData("アキトシ", "akitoshi", 45, 25, 130, 99, 30,  5, 100, new Color(1.0f, 0.4f, 0.4f), "プロギャンブラー / 通帳記入が趣味(残高は常に0)。おくるみは新聞紙。"),
        new ParentData("ネオ",     "neo",      15, 10,  90, 10, 70,  2, 100, new Color(0.5f, 0.5f, 0.6f), "永遠のニート / 30年間一度も実家から出たことがない。初期資産はほぼゼロ。"),
    };

    // 新母親6パターン
    static readonly ParentData[] NewMothers = new[]
    {
        new ParentData("イザナミ", "izanami",  85, 80, 180, 90, 95, 99, 100, new Color(0.9f, 0.3f, 0.5f), "伝説の女帝 / その一言で国家予算が動く。最高級の教育を約束する。"),
        new ParentData("ミク",     "miku",     35, 40, 130, 75, 40, 25, 25, new Color(1.0f, 0.6f, 0.8f), "自称・モデル / フォロワー数は多いが、内情は火の車。見栄えだけは良い。"),
        new ParentData("カヨコ",   "kayoko",  40, 65, 160, 65, 55, 50, 30, new Color(1.0f, 0.85f, 0.7f), "商店街の看板娘 / 資産はないが、街の人からお裾分け（アイテム）をもらえる。"),
        new ParentData("フクトク", "hukutoku", 20, 20, 120,150, 25, 65, 75, new Color(1.0f, 1.0f, 0.4f), "宝くじ1等当選者 / 才能は皆無だが、強運だけで修羅場を潜り抜けてきた。"),
        new ParentData("ヨネ",     "yone",     20, 15, 110, 45, 60, 10, 15, new Color(0.7f, 0.65f, 0.6f), "内職の鬼 / ティッシュ配りの速さは音速。赤ちゃんのスタイは自作。"),
        new ParentData("ドクコ",   "dokuko",   80, 70, 170, 15, 75,  5, 40, new Color(0.4f, 0.2f, 0.5f), "闇金の取り立て屋 / 赤ちゃんの最初の言葉を『トイチ』に教育しようとしている。"),
    };

    // 特徴リスト
    static readonly string[] Traits =
    {
        "天才肌", "努力家", "頑丈", "すばしっこい", "おだやか",
        "あまえんぼう", "なきむし", "くいしんぼう", "好奇心旺盛", "マイペース",
        "負けず嫌い", "やさしい", "ワイルド", "ミステリアス", "あばれんぼう",
        "覇王色",
    };

    // 36通りの恋愛ストーリー（父親名_母親名 → ストーリー）
    static readonly System.Collections.Generic.Dictionary<string, string> LoveStories = new System.Collections.Generic.Dictionary<string, string>
    {
        // タケシ（格闘家）× 各母親
        {"タケシ_サクラ", "ぶじゅつかいの頂点を決める大会。\nタケシとサクラは決勝で激突した。\n\nつよいわざが交錯する中、\n二人は互いの強さに惹かれていく。\n\nしあいは引き分けに終わり、\n「決着は別の形でつけよう」と\nタケシが差し出した手を、\nサクラは静かに握り返した。\n最強の血統がここに誕生する。"},
        {"タケシ_ヒナタ", "「格闘家専用コスメを作りたい」\nヒナタからの突然の依頼。\n\nビジネスミーティングのはずが、\nタケシの素朴な優しさに触れ、\nヒナタの心は揺れ始める。\n\n「数字じゃ測れないものがある」\nタケシの言葉に、\n28兆円の帝国を築いた女は\n初めて涙を流した。\n愛は最高の投資だと知った日。"},
        {"タケシ_アキラ", "オリンピック選手村での出会い。\n格闘技代表のタケシと\n陸上代表のアキラ。\n\n食堂で偶然隣り合わせになり、\n互いの鍛え抜かれた肉体に\n目を奪われた。\n\n「一緒にトレーニングしないか？」\nその一言から始まった朝練は、\nいつしか二人だけの時間に変わり、\n閉会式の夜、二人は結ばれた。"},
        {"タケシ_ミサト", "「筋肉の収縮は量子力学で\n説明できるんですよ」\n\n学会に招かれたタケシに、\nミサトは熱心に語りかけた。\n\n「難しいことはわからねえが、\nあんたの目は本気だな」\n\n理論と実践、正反対の二人。\nだが夜通し語り合ううちに、\n科学者の心は格闘家に奪われ、\n最強の頭脳と肉体が融合した。"},
        {"タケシ_カエデ", "世界格闘技選手権の決勝戦。\nタケシは宿敵とのはげしいしあいの末、\n右腕をけがしてしまった。\n\n「二度とうごかせない」と宣告される中、\n唯一の希望は天才外科医カエデだった。\n\n12時間に及ぶ手術。\n目覚めたタケシの最初の言葉は\n「俺の腕を救ってくれた君を、\n俺の人生に迎えたい」だった。"},
        {"タケシ_ルナ", "スポーツ雑誌の表紙撮影。\n格闘家とスーパーモデルの共演。\n\nカメラの前で火花が散り、\n「もっと近づいて」という\nカメラマンの指示に、\n二人の心臓が高鳴る。\n\n撮影後、ルナが言った。\n「あなたの隣にいると、\n自分が美しく見える気がする」\nスポットライトの下で恋が始まった。"},

        // ユウキ（ハッカー）× 各母親
        {"ユウキ_サクラ", "あやしい組織のサーバーに侵入した夜、\nユウキは追手に囲まれた。\n\nその中にいたのがサクラ。\n「あなたを とめるつもりはない。\nあなたの腕が必要なの」\n\n組織を裏切り、共に逃亡する日々。\n追われる中で芽生えた信頼は、\nいつしか愛に変わっていた。\n\n「俺のファイアウォールは\n君だけ通過できる」\n不器用な告白だった。"},
        {"ユウキ_ヒナタ", "美容帝国のDX化プロジェクト。\n億単位の契約書を前に、\nユウキは言った。\n\n「報酬はいらない。\nその代わり、週に一度\n食事に付き合ってほしい」\n\n最初は呆れていたヒナタも、\n彼の純粋さに惹かれていく。\n\n「私に値段をつけない人は\n初めてよ」\n28兆円より価値ある愛を知った。"},
        {"ユウキ_アキラ", "アスリート向けAIトレーナーの開発中、\nテストランナーとして\nアキラが研究所に現れた。\n\n「データが全然取れない...\n君は規格外すぎる」\n困惑するユウキに、\nアキラは笑って言った。\n\n「じゃあ毎日来てあげる」\n\nデータ収集という名目の\nデートが始まり、\n数値では測れない感情が芽生えた。"},
        {"ユウキ_ミサト", "量子コンピュータの共同研究。\n世界最高峰の頭脳が二つ、\n同じ研究室に集まった。\n\n夜通しのプログラミング、\nコーヒーカップが触れ合う音、\n「この暗号、解ける？」\n「君となら、どんな問題でも」\n\n二人だけの言語で愛を語り、\n論文より大切な答えを見つけた。\nそれは「共に生きる」という\nシンプルな真実だった。"},
        {"ユウキ_カエデ", "大病院のシステムがハッキングされた。\n犯人を追うカエデの前に現れたのは、\nセキュリティ専門家のユウキだった。\n\n夜通しの作業、\nコードを書く指とメスを握る指が\n偶然触れ合った瞬間、\n二人は目を合わせた。\n\n「君の手は人を救う手だ」\n「あなたの手もよ」\n異なる世界の天才が、\n同じ未来を見つめ始めた。"},
        {"ユウキ_ルナ", "SNSで炎上したルナ。\n誹謗中傷の嵐の中、\n匿名の誰かが彼女を守り続けた。\n\n悪質な投稿を消し、\n真実を広め、\n見えない騎士のように戦った。\n\nある日、IPアドレスを辿ったルナは\nユウキを見つけた。\n「なぜ私のために？」\n「君の笑顔を守りたかった」\nその日、二人は恋人になった。"},

        // ゴウ（ぼうけんか）× 各母親
        {"ゴウ_サクラ", "ぼうけんの途中で出会った二人。\nにらみ合いながら、\n奇妙な沈黙が流れた。\n\n「きみとたたかう理由がない」\n「私もよ」\n\nにらみあいをやめた瞬間、\n組織に追われる身となった。\n\n「一緒に逃げないか」\n「どこまでも」\n\n世界中を逃げ回る日々が、\n二人を離れられない関係にした。"},
        {"ゴウ_ヒナタ", "要人警護の任務。\n標的にされたのはヒナタだった。\n\n三度のきけん、\nその全てからゴウは彼女を守った。\n三度目のきけんを\n身を挺してかばった時、\nヒナタは悟った。\n\n「お金じゃ買えないものがある」\n\n病室で目覚めたゴウに、\n彼女は涙ながらに言った。\n「私の人生を守って」"},
        {"ゴウ_アキラ", "とおい国でのスポーツ親善大使。\nアキラの警護を任されたゴウは、\n彼女の無邪気さに戸惑った。\n\n「怖くないのか？」\n「あなたがいるから」\n\nきけんの中でも笑顔を絶やさない彼女。\n守るべき存在が、\nいつしか愛する人に変わっていた。\n\n任務終了の日、\nゴウはぼうけんかを辞める決意をした。"},
        {"ゴウ_ミサト", "ふしぎなほしのデータ解析依頼。\nつよいぼうけんかゴウと、\n純粋な物理学者ミサト。\n\n「なぜひとりでたたかうの？」\n直球の質問に、\nゴウは言葉を失った。\n\n「...答えが見つからない」\n「一緒に探しましょう」\n\nミサトの純粋さが、\n凍った心を少しずつ溶かしていく。\nぼうけんのおおかみが愛を知った瞬間だった。"},
        {"ゴウ_カエデ", "ぼうけんでたおれた仲間を救うため、\nゴウは国境を越えて\n天才外科医を探した。\n\n「報酬はいくらでも払う」\n「お金じゃないの。\nあなたが連れてきて」\n\nきけんな場所に飛び込んだカエデ。\n命がけの手術を終えた夜、\nゴウは初めて泣いた。\n\n「俺の人生を守ってくれないか」\nぼうけんかの不器用なプロポーズだった。"},
        {"ゴウ_ルナ", "ぼうけん写真家として同行したルナ。\n「真実を伝えたい」という\n彼女の覚悟に、ゴウは驚いた。\n\nあらしの夜、ほら穴で肩を寄せ合い、\nつよいきずなの中で\n二人は唇を重ねた。\n\n「生きて帰ろう」\n「ああ、一緒にな」\n\nぼうけんの中で誓った愛は、\nどんな平和な恋より強く、\n深く結ばれていた。"},

        // シンジ（天才科学者）× 各母親
        {"シンジ_サクラ", "「ぶじゅつの科学的解明」\nその研究テーマに、\nサクラは協力を申し出た。\n\n動きを解析するうちに、\nシンジの目は彼女自身に向いていた。\n\n「論文より君を研究したい」\n「それ、口説いてる？」\n「...多分」\n\n世界一不器用な告白に、\nつわものは初めて頬を染めた。\n愛は科学で証明できないと知った。"},
        {"シンジ_ヒナタ", "「美の方程式」を共著で出版したい。\nヒナタからの依頼に、\nシンジは興味を持った。\n\n数式とビジネス、\n異色のコラボレーション。\n\nグラフを描くうちに、\n二人の線は一点で交わった。\n\n「この交点が僕たちの未来だ」\n「ロマンチストね、意外と」\n\n28兆円の女帝が、\n数式に恋をした日だった。"},
        {"シンジ_アキラ", "「人体の限界」を科学する研究。\n被験者として現れたアキラの\n笑顔を見た瞬間、\nシンジの心拍データは乱れた。\n\n「先生、大丈夫？」\n「い、異常値が出ている...\n僕の心臓に」\n\n「それ、恋って言うんですよ」\nアキラの言葉に、\n天才科学者は顔を真っ赤にした。\n答えは最初から出ていたのだ。"},
        {"シンジ_ミサト", "国際物理学会での激論。\n「あなたの理論は穴だらけよ」\n「君こそ基礎が甘い」\n\n壇上で火花を散らした二人は、\nなぜかホテルのバーで再会した。\n\nIQ250とIQ270。\n合わせて520の恋が始まる。\n\n「数式より美しいものを見つけた」\n「何？」\n「君だよ」\n天才にしては陳腐な台詞だった。"},
        {"シンジ_カエデ", "ノーベル医学賞授賞式。\n偶然隣り合わせになった二人は、\n授賞式そっちのけで議論を始めた。\n\n「君の論文、3箇所間違ってる」\n「あなたこそ、5箇所よ」\n\n火花を散らす天才同士。\nだがパーティーが終わる頃には、\n互いを認め合っていた。\n\n「共同研究しないか」\n「いいわ、人生のパートナーとして」\n人類最高の遺伝子が誕生した。"},
        {"シンジ_ルナ", "「完璧な顔の数学的定義」\nその研究のため、\nルナがモデルとして協力した。\n\n何百枚もの写真、\n何千ものデータポイント。\n\n「結論が出たよ」\n「どんな顔が完璧なの？」\n「君だ。君以外にない」\n\n論文には書けない結論だった。\n美しさの究極の答えは、\n愛する人の顔だと気づいた。"},

        // リョウマ（実業家）× 各母親
        {"リョウマ_サクラ", "ボディガードとして雇ったつわもの。\n命を預けた相手に、\n心まで奪われるとは思わなかった。\n\n「金で動く女か」\n「いいえ、あなたを守りたいから」\n\n嘘のない瞳だった。\n\n43兆円あっても買えないもの。\nそれは信頼と愛だと、\nリョウマは初めて知った。\n「俺の傍にいてくれ、永遠に」"},
        {"リョウマ_ヒナタ", "美容帝国との合併話。\n二つの巨大企業、\n最初は敵対から始まった。\n\n「あなたには負けないわ」\n「俺もだ」\n\n激しい交渉の末、\n二人は互いを認め合った。\n\n「合併より、\n結婚しないか」\n「...それ、逆じゃない？」\n\n71兆円の帝国が誕生した。\n株式より価値ある絆と共に。"},
        {"リョウマ_アキラ", "スポーツ球団買収の記者会見。\n看板選手アキラとの握手の瞬間、\n世界一の資産家は恋に落ちた。\n\n「君をチームの顔にしたい」\n「顔じゃなくて、\n私を見てほしいな」\n\n真っ直ぐな言葉が胸を打った。\n\n株価より大切なもの。\n利益より価値あるもの。\nそれは彼女の笑顔だった。"},
        {"リョウマ_ミサト", "研究所への100億円の投資。\nその見返りに求めたのは、\n論文でも特許でもなかった。\n\n「週に一度、\n一緒に星を見てほしい」\n\nミサトは驚きながらも頷いた。\n\n屋上で星を眺める夜が続き、\n宇宙の話から人生の話へ。\n\n「君という星を見つけた」\n物理学者は、\nその方程式を解けなかった。"},
        {"リョウマ_カエデ", "病院チェーンのM&A交渉。\nビジネスランチのはずが、\n話は医療の未来へと広がった。\n\n「君の理想を実現するには\nいくら必要だ？」\n「お金の問題じゃないの」\n\nその言葉に、リョウマは衝撃を受けた。\n43兆円の資産が無意味に思えた。\n\n「なら、僕の人生を投資させてくれ」\n契約書にない想いを込めた。"},
        {"リョウマ_ルナ", "プライベートジェットで偶然の隣席。\nパリへ向かう12時間、\n二人は語り合った。\n\n仕事のこと、夢のこと、\n誰にも言えない弱さのこと。\n\n雲の上、地上から離れた空間で、\n肩書きも資産も意味を失った。\n\n着陸した時、\n二人は恋人になっていた。\n「地上に降りても、この気持ちは変わらない」"},

        // テツヤ（ロックスター）× 各母親
        {"テツヤ_サクラ", "新曲MVのアクションシーン。\n指導者として現れたサクラの\n鋭い動きに、テツヤは見惚れた。\n\n「もっと本気で来て」\n「怪我させるぞ」\n「それくらいが丁度いい」\n\nステージでわざを交えるうちに、\n二人の距離は縮まっていった。\n\n撮影終了後の楽屋で、\n二人は激しく唇を重ねた。"},
        {"テツヤ_ヒナタ", "化粧品CMソングの打ち合わせ。\n譜面を見るふりをして、\nテツヤはヒナタを見つめていた。\n\n「曲より私を見てない？」\n「バレた？」\n「わかりやすいのよ、あなた」\n\nスタジオに響く笑い声。\nその日、二人は朝まで語り合った。\n\n「君のための歌を書きたい」\n「それ、プロポーズ？」\n「かもしれない」"},
        {"テツヤ_アキラ", "オリンピック応援ソングの依頼。\n「勝利の歌を書いてほしい」\n\nアキラの走る姿を見て、\nテツヤのペンが走り出した。\n\nスタジアムに響く歌声、\n金メダルを取った瞬間、\nアキラはテツヤのもとへ走った。\n\n「この歌があったから勝てた」\n「君がいたから書けた」\n\n金メダルより輝く愛が生まれた。"},
        {"テツヤ_ミサト", "「音楽と物理学の共通点」\n雑誌のインタビューで出会った二人。\n\n「音は波でしょ？\n愛も波かもしれない」\n「周波数が合えば共鳴する...」\n「そう、今の僕たちみたいに」\n\n理屈っぽい会話が心地よかった。\n\nインタビューは終わっても、\n二人の会話は終わらなかった。\n共鳴した心は離れられない。"},
        {"テツヤ_カエデ", "ライブ中に声が出なくなった。\n「二度と歌えない」\n絶望するテツヤを救ったのは、\n天才外科医カエデだった。\n\n奇跡の手術から3ヶ月、\n声を取り戻した日、\nテツヤは病院でゲリラライブを開いた。\n\n「最初の歌は君に捧げる」\nそれは愛の歌だった。\nカエデは涙を流しながら聴いていた。"},
        {"テツヤ_ルナ", "ワールドツアー、50都市。\n同じ夢を追う二人は、\n世界中を一緒に回った。\n\n「疲れないか？」\n「あなたがいるから平気」\n\nステージの上と、ランウェイの上。\n輝く場所は違っても、\n見つめ合う瞳は同じだった。\n\n最後の公演、アンコール。\nテツヤはステージ上で跪いた。\n「結婚してくれ」\n8億人のファンが証人となった。"},

        // ゼニガタ（石油王）× 各母親
        {"ゼニガタ_イザナミ", "世界経済フォーラムの最前列。\n石油王と女帝が隣り合った。\n\n「この会場、買い取ろうか？」\n「もう買ってあるわ」\n\n互いの資産自慢が\nいつの間にか笑い合いに変わり、\n晩餐会では二人だけの席を用意した。\n\n「金で買えないものはない」\n「でも、あなたの心は私がもらうわ」\n世界最強の権力カップル誕生。"},
        {"ゼニガタ_ミク", "インフルエンサー案件の依頼。\n「石油を世界一オシャレに撮れ」\n\nゼニガタの無茶な依頼に\nミクは全力で応えた。\n油田をバックにキメポーズ。\n\n「フォロワー100万人増えたぞ」\n「でしょ？私の実力よ」\n\n見栄っ張り同士、\n嘘と本音の境界が溶けた夜、\nゼニガタは言った。\n「君だけは本物だ」"},
        {"ゼニガタ_カヨコ", "高級車で商店街に迷い込んだ石油王。\nコロッケの匂いに導かれ、\n看板娘カヨコの店にたどり着いた。\n\n「このコロッケ、いくらだ？」\n「80円ですよ」\n「80億出す」\n「おつり出せません」\n\n毎日通うゼニガタ。\nプラチナカードより\n温かいコロッケが欲しかった。\n「金じゃなく、心で買えるものがあるんだな」"},
        {"ゼニガタ_フクトク", "カジノVIPルームでの出会い。\nフクトクは持ち金0から\nルーレットだけで1億を稼いだ。\n\n「その運、買いたい」\n「運は売れませんよ」\n\n金で買えない唯一のもの。\nゼニガタは初めて挫折を味わった。\n\nだが隣にいるだけで\n事業がうまくいく不思議。\n「君は僕の最高の投資だ」\n「私は無料よ」"},
        {"ゼニガタ_ヨネ", "節税対策で訪れた下町の税理士事務所。\n待合室で内職をしていたヨネの\nティッシュ折りの速さに目を奪われた。\n\n「君、うちの工場で働かないか？」\n「時給いくらですか？」\n「好きなだけ」\n\nだがヨネは断った。\n「手作りに意味があるんです」\n\nその言葉が石油王の心を揺さぶった。\n金で買えない職人魂に、恋をした。"},
        {"ゼニガタ_ドクコ", "借金取りが石油王の屋敷に乗り込んだ。\n「利息、払ってもらおうか」\n\n実は前妻の借金だった。\nゼニガタは札束で頬を叩こうとしたが、\nドクコは微動だにしなかった。\n\n「金で黙ると思うな」\n「...面白い女だ」\n\n恐怖を知らない女に、\n石油王は初めて震えた。\nそれは恐怖ではなく、恋だった。"},

        // ツクモ（自称予言者）× 各母親
        {"ツクモ_イザナミ", "「3日後、あなたの帝国に\n危機が訪れる」\n\nツクモの予言を鼻で笑った\nイザナミだったが、\n本当に株が大暴落した。\n\n「次は何が見える？」\n「あなたと僕が結ばれる未来」\n「...それだけはハズレね」\n\nだが1年後、二人は一緒にいた。\n「予言は当たったな」\n「偶然よ」\n女帝は赤くなった顔を隠した。"},
        {"ツクモ_ミク", "「来世の運命を占います」\n怪しい路上占い師ツクモに、\nミクはネタ目的で近づいた。\n\n「あなたは...本当は\n見栄を張るのに疲れている」\n\n図星だった。\nカメラを止めた瞬間、\nミクは泣き出した。\n\n「誰にも言えなかったの」\n「宇宙は全部知ってるよ」\n\n嘘ばかりの世界で、\n本音を見抜く男に惹かれた。"},
        {"ツクモ_カヨコ", "商店街の福引でツクモが大当たりを\n連発した。\n「明日は雨」「当たり」\n「来週、猫が来る」「来た」\n\n看板娘カヨコは半信半疑だったが、\nある日ツクモが言った。\n\n「明日、君は恋をする」\n「え、誰と？」\n「僕と」\n\n次の日、雨で店に駆け込んできた\nツクモを見て、カヨコは笑った。\n「当たりかもね」"},
        {"ツクモ_フクトク", "「あなたには\n常軌を逸した運がある」\nツクモはフクトクを見て断言した。\n\n「知ってるわ、宝くじ当たったし」\n「そうじゃない。\n僕と出会ったことが最大の幸運だ」\n\nドン引きするフクトクだったが、\nツクモの隣にいると\nなぜか良いことが重なった。\n\n「予言者とラッキーガール。\n確率論の破壊者ね、私たち」"},
        {"ツクモ_ヨネ", "「宇宙が...ティッシュを\n折れと言っている」\n\n内職場にふらりと現れた\n自称・予言者に、\nヨネは冷たく言った。\n「手を動かして」\n\nだがツクモの内職スピードは\n驚異的だった。IQ300の手先。\n\n「あなた、予言より\nこっちの方が向いてるわよ」\n「君の隣なら何でもいい」\n宇宙より近い距離で恋が芽生えた。"},
        {"ツクモ_ドクコ", "「3日以内に返済しないと...」\nドクコの取り立てに、\nツクモは静かに言った。\n\n「明後日、宝くじの\n当選番号を教えよう」\n「ふざけるな」\n\nだが本当に当たった。\n\n「なぜわかる？」\n「宇宙と交信してるから」\n「...次も当ててみろ」\n\n取り立てがデートに変わった。\n闇金業者が予言者に墜ちた。"},

        // サトウ（普通の係長）× 各母親
        {"サトウ_イザナミ", "区役所の窓口で順番待ちをする\n女帝イザナミ。\n「なぜ私が並ばなければ...」\n\n隣のサトウが静かに言った。\n「みんな平等ですよ、ここでは」\n\nその「普通」に、\nイザナミは衝撃を受けた。\n\n「あなた、面白いわね」\n「よく言われます」\n\n世界を支配する女が、\n世界一普通の男に恋をした。\n平凡こそが最大の魅力だった。"},
        {"サトウ_ミク", "「映えるランチ」を探すミクが\n偶然入った定食屋で、\nサトウが生姜焼きを食べていた。\n\n「それ、全然映えないですよ」\n「うまいよ？食べてみな」\n\n一口食べたミクの目が輝いた。\n「...おいしい」\n\n「映え」より「旨い」を\n教えてくれた男。\nSNSに載せない幸せを、\nミクは初めて知った。"},
        {"サトウ_カヨコ", "毎朝コロッケを買いに来る\nサラリーマン。\n「いつもの一個ください」\n「はい、いつもの」\n\n雨の日も風の日も、\n10年間変わらないやり取り。\n\nある日カヨコが風邪で休むと、\nサトウはコロッケの代わりに\n薬を持ってきた。\n\n「いつもの恩返しです」\n\n日常に溶け込んだ愛。\nそれが一番温かかった。"},
        {"サトウ_フクトク", "フクトクが宝くじを買った売り場で\n偶然後ろに並んでいたサトウ。\n\n「一枚だけ買うんですか？」\n「一枚で十分。当たるから」\n\n本当に当たった。\n驚くサトウに、フクトクは言った。\n「あなたの後ろに並んだのも運命よ」\n\n「いや、たまたまですよ」\n\nその「普通」のリアクションが、\n逆にフクトクの心を掴んだ。\n運命は普通の中にあった。"},
        {"サトウ_ヨネ", "会社の内職を外注することになり、\n担当になったのがヨネだった。\n\n「納期は？」「明日で」\n「...できます」\n\n信じられないスピードで\n仕事を仕上げるヨネ。\nサトウは感動した。\n\n「すごいですね」\n「当たり前のことですよ」\n\n「当たり前」を大切にする二人。\n地味だけど確かな愛が育った。"},
        {"サトウ_ドクコ", "隣の席に引っ越してきた\n恐ろしい形相の女。\n\n「ゴミの日は火曜と金曜です」\nサトウは普通に挨拶した。\n\nドクコは面食らった。\n誰もが怯える自分に、\nこの男は普通に接する。\n\n「...あんた、度胸あるね」\n「いえ、普通ですよ」\n\n取り立て屋の心を溶かしたのは、\n暴力でも金でもなく、\n「普通の優しさ」だった。"},

        // イワオ（土木作業員）× 各母親
        {"イワオ_イザナミ", "宮殿の改修工事に駆り出されたイワオ。\n素手で壁を壊す姿を、\nイザナミは窓から眺めていた。\n\n「あの男、重機を使わないの？」\n「必要ないそうです」\n\n昼休み、おにぎり一個の\nイワオに、イザナミは\nフルコースを差し入れた。\n\n「食え。命令だ」\n「あ、ありがとうございます」\n\n女帝が初めて誰かに食事を作った日。\nそれが愛の始まりだった。"},
        {"イワオ_ミク", "工事現場のドキュメンタリー撮影。\n「映える現場男子」企画で\nイワオが抜擢された。\n\n「筋肉すごい！でもプロテイン\n買えないってマジ？」\n「...マジです」\n\nミクは自腹でプロテインを差し入れた。\nバズった動画のコメント欄は\n「この二人付き合って」の嵐。\n\n「フォロワーが言ってるから」\n「それ、ミクさんの気持ちは？」\n「...同じ」"},
        {"イワオ_カヨコ", "商店街の道路工事。\n毎日カヨコの店の前で\n汗を流すイワオ。\n\n「お水どうぞ」\n「すみません、金が...」\n「いらないですよ、サービス」\n\nコロッケも、おにぎりも、\n全部「サービス」だった。\n\nある日イワオが石で\n小さな花瓶を彫って渡した。\n「金はないけど、これなら」\n\nカヨコの目に涙が光った。\n「これが一番嬉しい」"},
        {"イワオ_フクトク", "道端で財布を拾ったイワオ。\n中身は空っぽだったが、\n宝くじが一枚入っていた。\n\n届けに来た交番で\n持ち主のフクトクと出会った。\n\n「その宝くじ、1等よ」\n「え!? 届けてよかった...」\n「お礼に夕飯おごるわ」\n\n貧乏と幸運が出会った夜。\nフクトクの運がイワオにも\n伝染し始めた。\n「あなたといると不思議ね」"},
        {"イワオ_ヨネ", "市営住宅の隣人同士。\n壁が薄くて、内職の音が\n毎晩聞こえてくる。\n\n「うるさくてすみません」\n「いや、あの音を聞くと\n安心するんです」\n\n貧しいもの同士、\nおかずを分け合い、\n洗濯物を取り込み合い。\n\n「金持ちにはなれねえけど」\n「うちもですよ」\n\n二人でいれば、\n貧乏も悪くないと思えた。"},
        {"イワオ_ドクコ", "借金の取り立てに来たドクコ。\nだがイワオの部屋には\n布団と鉄アレイしかなかった。\n\n「取るもんがねえ...」\n「はい、すみません」\n\nなぜか謝るイワオ。\nドクコは呆れながらも、\n素手で岩を砕くその腕を見て\n思わず呟いた。\n\n「...うちで働かない？ 用心棒として」\n「飯つきなら」\n恐怖と筋肉が手を結んだ。"},

        // アキトシ（プロギャンブラー）× 各母親
        {"アキトシ_イザナミ", "チャリティーポーカー大会。\n女帝イザナミを相手に、\nアキトシは全財産(3000円)を賭けた。\n\n「面白い目をしているわね」\n「破産慣れしてますから」\n\n結果、アキトシの勝ち。\n賞金は全額寄付した。\n\n「金に興味がないの？」\n「勝負に興味があるんです」\n\n女帝が唯一負けた男。\nそれだけで恋に落ちる理由になった。"},
        {"アキトシ_ミク", "パチンコ屋で偶然会った二人。\nミクは「庶民派アピール」の撮影、\nアキトシはガチの勝負中。\n\n「あの、隣で撮っていいですか？」\n「静かにしてくれるなら」\n\nアキトシの隣に座ると、\nミクの台も当たり始めた。\n\n「あなた、引きが強い？」\n「いや、運は最悪だ」\n\n運の悪い男の隣で\nなぜか当たる自分。\n「これって相性ってこと？」"},
        {"アキトシ_カヨコ", "所持金0円で商店街をさまようアキトシ。\n腹の虫が鳴った瞬間、\nカヨコが揚げたてコロッケを差し出した。\n\n「お代はいつでもいいですよ」\n\n3日後、競馬で大勝ちして\n100万円を持ってきたアキトシ。\n「コロッケ代です」\n「80円ですってば！」\n\n翌日また一文なし。\nでもコロッケは温かかった。\n「あんた、ほんとにしょうがないね」"},
        {"アキトシ_フクトク", "宝くじ売り場の前での出会い。\nフクトクが買うと必ず当たり、\nアキトシが買うと必ずハズレ。\n\n「代わりに買ってくれない？」\n「いいわよ」\n\n当たった。二人で山分け。\n翌日アキトシは全額溶かした。\n\n「...また買ってくれる？」\n「もう、しょうがないわね」\n\n最強の幸運と最凶の浪費。\n終わらないループが\n二人を結びつけた。"},
        {"アキトシ_ヨネ", "内職の報酬を受け取りに来た\nヨネの隣で、アキトシは\n競馬新聞を読んでいた。\n\n「それ、当たるんですか？」\n「当たらないから面白い」\n\nヨネには理解できなかった。\nだが不思議と気になった。\n\n「稼いだら全部使う人と、\n1円も無駄にしない私。\n真逆ね」\n\n「だから補い合えるんだろ」\nギャンブラーの言葉が\n初めて的を射た瞬間だった。"},
        {"アキトシ_ドクコ", "闇ポーカーで負けた借金の取り立て。\nドクコが凄んでも、\nアキトシはヘラヘラしていた。\n\n「怖くないのか？」\n「負け慣れてますから」\n\n蹴り飛ばそうとした足を\n軽くかわすアキトシ。\n\n「...あんた、度胸だけはあるね」\n「それしか取り柄がないんで」\n\n取り立てが通い妻に変わるまで\n3ヶ月。利息は愛情で返済された。"},

        // ネオ（永遠のニート）× 各母親
        {"ネオ_イザナミ", "引きこもりのネオの実家が、\n女帝の開発計画で立ち退き対象に。\n\n「この部屋から出るくらいなら\n死んだ方がマシです」\n\nイザナミは呆れたが、\nネオの目の奥に\n純粋な恐怖を見た。\n\n「...特別に残してやるわ」\n「え、マジすか」\n\n世界を動かす女帝が\nニート一人に譲歩した。\nそれが愛だと気づくのは\nもう少し先の話。"},
        {"ネオ_ミク", "「ニートの部屋、覗いてみた」\nミクのバズり企画に\nネオの部屋が選ばれた。\n\n「うわ、フィギュアすごい！」\n「触らないでください」\n\nだが配信中のネオの解説が\n意外にも面白く、\n視聴者が殺到した。\n\n「あんた、才能あるわよ」\n「...生まれて初めて言われた」\n\n画面越しに始まった関係が、\n少しずつ現実に近づいていった。"},
        {"ネオ_カヨコ", "母親に頼まれたお使いで\n30年ぶりに外出したネオ。\n迷子になり、カヨコの店に辿り着いた。\n\n「大丈夫ですか？」\n「外、こわい...」\n\nカヨコはコロッケを渡し、\n家まで送ってくれた。\n\n翌日もネオは店に来た。\n「お使い頼まれまして」\n嘘だった。\n\nカヨコの笑顔が、\n30年間閉じていた扉を\n少しだけ開けた。"},
        {"ネオ_フクトク", "ネットで当選したゲーム機。\n届いたのは2台だった。\n「配送ミスか...」\n\n届け先を調べるとフクトクだった。\n「私もなぜか当たるんです」\n\n二人でオンラインゲームを始めた。\n会わなくても繋がれる関係。\n\n「いつかリアルでも会おうよ」\n「...外出たくないです」\n「じゃあ私が行くわ」\n\n最強の幸運が\n最弱のニートの元に\n転がり込んできた。"},
        {"ネオ_ヨネ", "在宅内職の求人に応募したネオ。\n指導員としてヨネが家に来た。\n\n「手先は器用ですね」\n「30年間ゲームしかしてないんで」\n\n意外な才能を発揮するネオ。\nヨネは毎日指導に通った。\n\n「これ、今日の分のおかず」\n「え、いいんですか」\n\n内職と差し入れ。\n小さな経済圏の中で、\n二人の距離は縮まっていった。\n「外に出なくても幸せってあるのね」"},
        {"ネオ_ドクコ", "親の借金を背負わされたネオ。\n取り立てに来たドクコは、\n震えるニートを見て固まった。\n\n「こいつから取れるもん、\nなんもねえ...」\n\nだがネオのPCスキルに目をつけた。\n「帳簿管理やれ。借金チャラにしてやる」\n\n恐怖で始まった関係だが、\nドクコの強さにネオは安心感を覚えた。\n\n「あんたといると\n外の世界も怖くない」\n「当たり前だ。私が守るからな」"},
    };

    // 36通りの人生の要約（父親名_母親名 → 要約）
    static readonly System.Collections.Generic.Dictionary<string, string> LifeSummaries = new System.Collections.Generic.Dictionary<string, string>
    {
        // ゼニガタ（石油王）× 各母親
        {"ゼニガタ_イザナミ", "金と権力の完全なる融合。この子の初泣きで株価が動く。"},
        {"ゼニガタ_ミク", "パパの財布でママのフォロワーが増えるシステム。子供は生まれながらのインフルエンサー。"},
        {"ゼニガタ_カヨコ", "商店街にプラチナ製の看板を寄贈する石油王。子供のお年玉は原油先物。"},
        {"ゼニガタ_フクトク", "宝くじ当選者と石油王の悪魔合体。運も金も使い切れない。"},
        {"ゼニガタ_ヨネ", "年収数兆円なのに妻が内職をやめない。おくるみは手縫い。"},
        {"ゼニガタ_ドクコ", "取り立てる側と取り立てられる側の禁断の愛。利息は愛で返済。"},

        // ツクモ（自称・予言者）× 各母親
        {"ツクモ_イザナミ", "宇宙との交信と国家運営を両立する夫婦。子供の名前は星座から選ばれた。"},
        {"ツクモ_ミク", "予言者の息子がバズる未来は予言済み。フォロワー数も星の数ほど。"},
        {"ツクモ_カヨコ", "商店街の未来を予言したら全部外れた。コロッケの売上だけは上がった。"},
        {"ツクモ_フクトク", "予言は外れるが宝くじは当たる。矛盾した幸運が子供に流れ込む。"},
        {"ツクモ_ヨネ", "宇宙からのメッセージを受信中に妻がティッシュを折る音。家庭内ノイズキャンセリング不可。"},
        {"ツクモ_ドクコ", "来世の借金まで予言してしまい、妻に怒られる日々。子供は霊感と威圧感を継承。"},

        // サトウ（中堅企業の係長）× 各母親
        {"サトウ_イザナミ", "係長が女帝と結婚した結果、社内の力関係が宇宙規模に崩壊。子供の参観日に黒塗りの車。"},
        {"サトウ_ミク", "普通の父と映える母。子供の運動会をインスタに上げたら地味すぎて逆にバズった。"},
        {"サトウ_カヨコ", "普通すぎる家庭に絶望し、すでに2回目の人生を諦めている。"},
        {"サトウ_フクトク", "平凡な夫と幸運な妻。年末ジャンボだけで生活が成り立つ奇跡の家計。"},
        {"サトウ_ヨネ", "夫は係長、妻は内職。日本で最も平均的な家庭から、最も平均的な赤ちゃんが誕生。"},
        {"サトウ_ドクコ", "隣の席の取り立て屋と結婚した係長。社内で最も話しかけづらい夫婦。"},

        // イワオ（元・土木作業員）× 各母親
        {"イワオ_イザナミ", "素手で宮殿を増築する夫と国を動かす妻。子供は生まれた瞬間に瓦を割った。"},
        {"イワオ_ミク", "筋肉がバズった結果、プロテインのCMが決まった。報酬は現物支給。"},
        {"イワオ_カヨコ", "コロッケで育った筋肉。子供の離乳食は揚げ物オンリー。"},
        {"イワオ_フクトク", "貧乏だが幸運で食いつなぐ一家。子供は石を投げれば金塊に当たる。"},
        {"イワオ_ヨネ", "夫は日雇い、妻は内職。月収は合わせて米一俵分だが、愛だけは重量級。"},
        {"イワオ_ドクコ", "用心棒と取り立て屋の最恐カップル。子供が泣くと近隣住民が家賃を前払いする。"},

        // アキトシ（プロギャンブラー）× 各母親
        {"アキトシ_イザナミ", "国家予算をルーレットに賭けようとして女帝に殴られた。子供は賭け事禁止で育つ。"},
        {"アキトシ_ミク", "パパは全財産を溶かし、ママはフォロワーを溶かす。子供は生まれながらの炎上体質。"},
        {"アキトシ_カヨコ", "所持金ゼロでもコロッケは温かい。子供の初めての言葉は『ツケで』。"},
        {"アキトシ_フクトク", "最強の幸運と最凶の浪費が子供の中で戦っている。お年玉は即日蒸発。"},
        {"アキトシ_ヨネ", "妻が1円ずつ貯めた金を夫が1秒で溶かす。子供はその光景を見て育つ。"},
        {"アキトシ_ドクコ", "借金の取り立てが求婚になった稀有なケース。子供は利息の計算が異常に速い。"},

        // ネオ（永遠のニート）× 各母親
        {"ネオ_イザナミ", "30年間実家から出ない男を女帝が養う構図。子供は玉座の間でゲームをする。"},
        {"ネオ_ミク", "ニートの部屋から始まった配信が伝説に。子供は生まれた瞬間からライブ配信。"},
        {"ネオ_カヨコ", "30年ぶりの外出先がコロッケ屋。子供は商店街で唯一の引きこもり二世。"},
        {"ネオ_フクトク", "働かなくても宝くじで暮らせる最強の怠惰。子供は努力という概念を知らない。"},
        {"ネオ_ヨネ", "家計の全てを内職が支える。父は布団の中でレベルアップを夢見る。"},
        {"ネオ_ドクコ", "借金を背負わされたニートと取り立て屋。子供は恐怖で正しい姿勢を学ぶ。"},
    };

    // ストーリー表示用UI (UI Toolkit)
    UIE.VisualElement storyPanel;
    UIE.Label storyText;
    bool waitingForStoryConfirm;

    // ストーリー画面の親カード用 (UI Toolkit)
    UIE.VisualElement storyFatherFace, storyMotherFace;
    UIE.Label storyFatherName, storyMotherName;
    UIE.Label storyFatherIntro, storyMotherIntro;
    UIE.VisualElement storyAlbumContent; // ハートパーティクル発射先
    UIE.VisualElement activeStoryTooltip; // 表示中のふきだし

    void Start()
    {
        Debug.Log("[BirthSystem] Start() called");
        if (canvas == null)
            canvas = FindAnyObjectByType<Canvas>();

        // CanvasScaler調整: 縦向き9:16、幅基準
        var canvasScaler = canvas.GetComponent<CanvasScaler>();
        if (canvasScaler != null)
        {
            canvasScaler.referenceResolution = new Vector2(1080, 1920);
            canvasScaler.matchWidthOrHeight = 0f;
        }

        // childStatusText は SwitchToBirthResultLayout() で UI Toolkit Label として作成
        // 全画面背景
        var bgObj = new GameObject("Background");
        bgObj.transform.SetParent(canvas.transform, false);
        bgObj.transform.SetAsFirstSibling();
        var bgRect = bgObj.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        mainBgImg = bgObj.AddComponent<Image>();
        var birthBgSprite = Resources.Load<Sprite>("BackGrounds/birth-background");
        if (birthBgSprite != null)
        {
            mainBgImg.sprite = birthBgSprite;
            mainBgImg.type = Image.Type.Simple;
            mainBgImg.preserveAspect = false;
            mainBgImg.color = Color.white;
        }
        else
        {
            mainBgImg.color = new Color(0.953f, 0.969f, 0.973f);
        }
        mainBgImg.raycastTarget = false;
        // object-fit: cover — アスペクト比を維持しつつ画面全体を覆う
        var fitter = bgObj.AddComponent<AspectRatioFitter>();
        fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
        if (birthBgSprite != null)
            fitter.aspectRatio = (float)birthBgSprite.texture.width / birthBgSprite.texture.height;

        // いでよGodBabyボタンをパチンコ風赤ボタンにスタイリング
        StylePachinkoButton(generateLifeButton);

        StylePillButton(anotherGalButton, 700, 120, Localization.Get("birth_reroll_button"), 36);
        StylePillButton(gotoBattleButton, 700, 120, Localization.Get("birth_name_button"), 36);

        // 名前をつけるボタン: ポストイット風スタイリング
        if (gotoBattleButton != null)
        {
            var rect = gotoBattleButton.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0, 180);
            rect.localRotation = Quaternion.identity;
            var img = gotoBattleButton.GetComponent<Image>();
            if (img != null) img.color = new Color(1f, 1f, 0.78f); // ポストイット黄色
        }
        if (anotherGalButton != null)
        {
            var rect = anotherGalButton.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0, 60);
        }
        CreateBabyFaceUI(); // babyFaceを作成
        // BabySynthesizer初期化
        var synthObj = new GameObject("BabySynthesizerHost");
        synthObj.transform.SetParent(transform, false);
        babySynthesizer = synthObj.AddComponent<BabySynthesizer>();

        CreateFlashOverlay();
        CreateLightningOverlay();
        CreateIntroPanel();

        // SE読み込み
        seSource = gameObject.AddComponent<AudioSource>();
        seBabySource = gameObject.AddComponent<AudioSource>();
        seKettei = Resources.Load<AudioClip>("SE/kettei-button");
        seThunder = Resources.Load<AudioClip>("SE/thunder-magic");
        seBabyVoice = Resources.Load<AudioClip>("SE/baby-voice");
        seKirakira = Resources.Load<AudioClip>("SE/きらきら輝く6");
        seLevelUp = Resources.Load<AudioClip>("SE/レベルアップ");
        sePeta = Resources.Load<AudioClip>("SE/tap-effect");
        seKirakira4 = Resources.Load<AudioClip>("SE/きらきら輝く4");
        seCardFlip = Resources.Load<AudioClip>("SE/SE_Tap");
        //CreateCharacterListUI();

        // UI Toolkit overlay (menu, name input, save confirm, parent bio, parent cards, story)
        overlayPanelSettings = UIHelper.CreatePanelSettings(10f);
        var overlayObj = new GameObject("BirthOverlayUI");
        overlayObj.transform.SetParent(transform, false);
        overlayRoot = UIHelper.SetupUIDocument(overlayObj,
            new[] { "UI/CommonStyle", "UI/BirthStyle" }, overlayPanelSettings);
        overlayRoot.pickingMode = UIE.PickingMode.Ignore;
        UIHelper.RegisterTapSE(overlayRoot);
        CreateParentUI();
        if (parentPanel != null) parentPanel.style.display = UIE.DisplayStyle.None;
        CreateStoryUI();
        CreateMenuBar();
        CreateNameInputUI();
        CreateSaveConfirmUI();

        // デバッグ用: 赤ちゃん画面スキップボタン（UI Toolkit）
        CreateDebugSkipButton();
    }

    void OnDestroy()
    {
        if (overlayPanelSettings != null)
            Destroy(overlayPanelSettings);
    }

    // ===== デバッグ用: 赤ちゃん画面スキップ =====

    UIE.VisualElement debugSkipBtnContainer;
    void CreateDebugSkipButton()
    {
        if (overlayRoot == null) return;

        // 画面下部に固定配置するコンテナ
        debugSkipBtnContainer = new UIE.VisualElement();
        debugSkipBtnContainer.style.position = UIE.Position.Absolute;
        debugSkipBtnContainer.style.bottom = 340;
        debugSkipBtnContainer.style.left = 0;
        debugSkipBtnContainer.style.right = 0;
        debugSkipBtnContainer.style.alignItems = UIE.Align.Center;
        debugSkipBtnContainer.pickingMode = UIE.PickingMode.Ignore;

        var btn = new UIE.Button();
        btn.AddToClassList("pill-button");
        UIHelper.ApplyFont(btn);
        btn.text = "赤ちゃんよう(dev)";
        btn.style.width = 500;
        btn.style.height = 80;
        btn.style.fontSize = 28;
        btn.style.backgroundColor = new Color(0.67f, 0.94f, 0.82f); // #AAF0D1
        btn.style.borderTopLeftRadius = 40;
        btn.style.borderTopRightRadius = 40;
        btn.style.borderBottomLeftRadius = 40;
        btn.style.borderBottomRightRadius = 40;
        btn.clicked += () => StartCoroutine(DebugSkipToBabyResult());
        debugSkipBtnContainer.Add(btn);
        overlayRoot.Add(debugSkipBtnContainer);
    }

    IEnumerator DebugSkipToBabyResult()
    {
        Debug.Log("[BirthSystem] DEBUG: Skipping to baby result");
        if (isAnimating) yield break;
        isAnimating = true;

        // ダミーデータ
        selectedGender = "女の子";
        ParentData father = NewFathers[0];
        ParentData mother = NewMothers[0];
        int c_fortune = 80;

        // ボタン・イントロを非表示
        if (generateLifeButton != null) generateLifeButton.SetActive(false);
        if (introPanel != null) introPanel.SetActive(false);
        if (debugSkipBtnContainer != null) debugSkipBtnContainer.style.display = UIE.DisplayStyle.None;

        // 背景を青空に切り替え
        if (mainBgImg != null)
        {
            var aozoraSprite = Resources.Load<Sprite>("BackGrounds/aozora-background");
            if (aozoraSprite != null)
            {
                mainBgImg.sprite = aozoraSprite;
                mainBgImg.type = Image.Type.Simple;
                mainBgImg.preserveAspect = false;
                mainBgImg.color = Color.white;
            }
        }

        // レイアウト切り替え
        var rank = BabySynthesizer.DetermineRank(c_fortune);
        SwitchToBirthResultLayout(rank);
        yield return null;

        // 合成
        if (babyFace != null) babyFace.gameObject.SetActive(false);
        var synthParams = new SynthesizeParams
        {
            fortune = c_fortune,
            isGodBaby = false,
            fatherImageName = father.imageName,
            motherImageName = mother.imageName,
            genderKey = "female",
            father = father,
            mother = mother,
            babyAtk = 50, babyDef = 50, babyHp = 150,
            babyIntelligence = 50, babyAthletic = 50, babyLuck = 50,
            customImagePath = DataCarrier.Instance != null ? DataCarrier.Instance.customBabyImagePath : "",
            fatherItemPath = GetParentItemPath(father.imageName, true),
            motherItemPath = GetParentItemPath(mother.imageName, false),
        };
        lastSynthParams = synthParams;
        Sprite synthSprite = babySynthesizer.Synthesize(synthParams);
        if (synthSprite != null)
        {
            DisplaySynthesizedBaby(synthSprite);
        }

        // 結果コンテナを表示
        var resultContainer = UIE.UQueryExtensions.Q(overlayRoot, className: "birth-result-container");
        if (resultContainer != null) resultContainer.style.opacity = 1;

        // アップロードボタン
        if (birthResultCard != null) CreateUploadButton();
        if (birthResultCard != null) CreateScreenshotButton();

        // 名前ラベル表示
        if (babyNameLabel != null)
            babyNameLabel.text = "<color=#ff99cc>女の子</color>";

        isAnimating = false;
        Debug.Log("[BirthSystem] DEBUG: Baby result displayed");
    }

    // ===== ボタンから呼ばれるメソッド =====

    public void SpinRoulette()
    {
        Debug.Log("[BirthSystem] SpinRoulette button clicked");
        if (isAnimating) return;

        // 決定ボタンSE
        if (seSource != null && seKettei != null)
            seSource.PlayOneShot(seKettei, 0.8f);

        // イントロパネルを非表示
        if (introPanel != null)
            introPanel.SetActive(false);

        StartCoroutine(PachinkoStartSequence());
    }

    IEnumerator PachinkoStartSequence()
    {
        isAnimating = true;

        // ボタンのエフェクトを停止
        if (generateLifeButton != null)
        {
            var effect = generateLifeButton.GetComponent<PachinkoButtonEffect>();
            if (effect != null) effect.enabled = false;
        }

        // ── フェーズ1: ボタンがやさしく震える + 光が集まる（2.5秒） ──
        float shakeDuration = 2.5f;
        float elapsed = 0f;
        float shakeIntensity = 2f;
        Vector3 originalPos = Vector3.zero;
        RectTransform btnRect = null;

        if (generateLifeButton != null)
        {
            btnRect = generateLifeButton.GetComponent<RectTransform>();
            if (btnRect != null) originalPos = btnRect.anchoredPosition;
        }

        // やさしい振動（徐々に強くなるがマイルドに）
        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / shakeDuration;
            float currentIntensity = Mathf.Lerp(shakeIntensity, shakeIntensity * 3f, progress * progress);
            if (btnRect != null)
            {
                float offX = Random.Range(-currentIntensity, currentIntensity);
                float offY = Random.Range(-currentIntensity, currentIntensity);
                btnRect.anchoredPosition = new Vector2(originalPos.x + offX, originalPos.y + offY);
            }

            // 振動中にスターダストを散らす（0.3秒ごと）
            if (progress > 0.2f && Time.frameCount % 18 == 0)
            {
                SpawnCelestialParticle();
            }

            yield return null;
        }

        // 位置リセット
        if (btnRect != null)
            btnRect.anchoredPosition = new Vector2(originalPos.x, originalPos.y);

        // ── フェーズ2: ハート＆星が舞い上がる + ソフトフラッシュ ──
        // 一斉に多数のパーティクルを放出
        for (int i = 0; i < 12; i++)
        {
            SpawnCelestialParticle();
        }

        // ソフトなフラッシュ（ピンクがかった白）
        if (flashOverlay != null)
        {
            flashOverlay.gameObject.SetActive(true);
            flashOverlay.color = new Color(1f, 0.95f, 0.96f, 0.4f);
        }
        yield return new WaitForSeconds(0.2f);

        if (flashOverlay != null)
            flashOverlay.color = new Color(1f, 1f, 1f, 0f);

        // もう一波
        for (int i = 0; i < 8; i++)
        {
            SpawnCelestialParticle();
        }
        yield return new WaitForSeconds(0.3f);

        // 最後のやさしいホワイトアウト
        if (flashOverlay != null)
        {
            flashOverlay.gameObject.SetActive(true);
            flashOverlay.color = new Color(1f, 1f, 1f, 0.85f);
        }

        yield return new WaitForSeconds(0.2f);

        // フラッシュフェードアウト（やわらかく）
        if (flashOverlay != null)
        {
            float fadeDur = 0.6f;
            float fadeElapsed = 0f;
            while (fadeElapsed < fadeDur)
            {
                fadeElapsed += Time.deltaTime;
                float a = Mathf.Lerp(0.85f, 0f, fadeElapsed / fadeDur);
                flashOverlay.color = new Color(1f, 1f, 1f, a);
                yield return null;
            }
            flashOverlay.gameObject.SetActive(false);
        }

        // 背景を青空に切り替え
        if (mainBgImg != null)
        {
            var aozoraSprite = Resources.Load<Sprite>("BackGrounds/aozora-background");
            if (aozoraSprite != null)
            {
                mainBgImg.sprite = aozoraSprite;
                mainBgImg.type = Image.Type.Simple;
                mainBgImg.preserveAspect = false;
                mainBgImg.color = Color.white;
                var fitter = mainBgImg.GetComponent<AspectRatioFitter>();
                if (fitter != null)
                    fitter.aspectRatio = (float)aozoraSprite.texture.width / aozoraSprite.texture.height;
            }
        }

        // ボタンを非表示
        if (generateLifeButton != null) generateLifeButton.SetActive(false);

        // 青空の余韻（フラッシュ後の静寂）
        yield return new WaitForSeconds(1.0f);

        // ── フェーズ3: 「父親は誰だろう！？」カットイン ──
        yield return StartCoroutine(ShowCutinText(Localization.Get("cutin_who_father")));

        // カットイン後の余韻
        yield return new WaitForSeconds(0.8f);

        // ── フェーズ4: ルーレットへ遷移 ──
        isAnimating = false;
        StartCoroutine(SpinRouletteAnimation());
    }

    // ── 天界パーティクル（ハート・星がふわっと舞い上がる） ──
    static readonly string[] celestialSymbols = { "\u2665", "\u2606", "\u2726", "\u2727", "\u2764" };
    static readonly Color[] celestialColors = {
        new Color(1f, 0.72f, 0.77f, 1f),   // ピンク
        new Color(0.97f, 0.91f, 0.81f, 1f), // メイン
        new Color(0.67f, 0.94f, 0.82f, 1f), // ミント
        new Color(1f, 0.85f, 0.65f, 1f),    // ゴールド
        new Color(1f, 1f, 1f, 1f),           // ホワイト
    };

    void SpawnCelestialParticle()
    {
        if (canvas == null) return;
        var particleObj = new GameObject("CelestialParticle");
        particleObj.transform.SetParent(canvas.transform, false);
        var rect = particleObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        // ボタン周辺から発生（ボタン位置 y=-400 付近）
        float startX = Random.Range(-200f, 200f);
        float startY = -400f + Random.Range(-80f, 80f);
        rect.anchoredPosition = new Vector2(startX, startY);
        rect.sizeDelta = new Vector2(60, 60);

        var tmp = particleObj.AddComponent<TextMeshProUGUI>();
        tmp.text = celestialSymbols[Random.Range(0, celestialSymbols.Length)];
        tmp.fontSize = Random.Range(24, 44);
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = celestialColors[Random.Range(0, celestialColors.Length)];
        tmp.raycastTarget = false;

        StartCoroutine(AnimateCelestialParticle(rect, tmp));
    }

    IEnumerator AnimateCelestialParticle(RectTransform rect, TextMeshProUGUI tmp)
    {
        if (rect == null || tmp == null) yield break;
        float duration = Random.Range(1.2f, 2.0f);
        float elapsed = 0f;
        Vector2 startPos = rect.anchoredPosition;
        float driftX = Random.Range(-60f, 60f);
        float riseY = Random.Range(300f, 600f);
        float startScale = Random.Range(0.5f, 1.0f);
        Color startColor = tmp.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float eased = 1f - (1f - t) * (1f - t); // ease-out quadratic

            if (rect == null) yield break;
            rect.anchoredPosition = new Vector2(
                startPos.x + driftX * eased + 8f * Mathf.Sin(t * Mathf.PI * 3f),
                startPos.y + riseY * eased
            );
            float s = startScale * (1f + 0.3f * Mathf.Sin(t * Mathf.PI));
            rect.localScale = new Vector3(s, s, 1f);

            if (tmp != null)
                tmp.color = new Color(startColor.r, startColor.g, startColor.b, 1f - t * t);

            yield return null;
        }

        if (rect != null) Destroy(rect.gameObject);
    }

    public void ResetParents()
    {
        if (isAnimating) return;
        SceneManager.LoadScene("BirthScene");
    }

    public void GoToBattle()
    {
        if (isUploadMode)
        {
            // 「運命を吹き込む」モード → 画像アップロード
            PickImageFromGallery();
            return;
        }
        StartCoroutine(NameAndSaveSequence());
    }

    void SwitchToUploadMode()
    {
        isUploadMode = true;
        if (gotoBattleButton == null) return;
        var tmp = gotoBattleButton.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp != null) tmp.text = "運命を吹き込む";
    }

    void SwitchToNamingMode()
    {
        isUploadMode = false;
        if (gotoBattleButton == null) return;
        var tmp = gotoBattleButton.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp != null) tmp.text = Localization.Get("birth_name_button");
    }

    IEnumerator NameAndSaveSequence()
    {
        // 名前をつける・もう一度ボタンを無効化
        if (gotoBattleButton != null) gotoBattleButton.SetActive(false);
        if (anotherGalButton != null) anotherGalButton.SetActive(false);

        // 名前入力ダイアログ表示
        nameInputCancelled = false;
        if (nameInputOverlayEl != null && nameTextField != null)
        {
            nameTextField.value = "";
            overlayRoot.Add(nameInputOverlayEl);
            nameTextField.Focus();
        }

        waitingForNameInput = true;
        while (waitingForNameInput)
            yield return null;

        nameInputOverlayEl?.RemoveFromHierarchy();

        // キャンセル時はボタンを復帰して終了
        if (nameInputCancelled)
        {
            if (gotoBattleButton != null) gotoBattleButton.SetActive(true);
            yield break;
        }

        // DataCarrierの進行データをリセットし、名前を保存
        if (DataCarrier.Instance != null)
        {
            DataCarrier.Instance.ResetForNewBaby();
            DataCarrier.Instance.babyName = enteredName;
        }

        // ── 名前テープ貼り付け演出 ──
        yield return StartCoroutine(NameTapeAnimation(enteredName));

        // ── 保存・シェアモードに切り替え ──
        yield return StartCoroutine(SwitchToShareMode());
    }

    IEnumerator NameTapeAnimation(string babyName)
    {
        if (overlayRoot == null || birthResultCard == null) yield break;

        // 名前テープ要素を作成 → カードのフレーム内に配置
        nameTapeEl = new UIE.VisualElement();
        nameTapeEl.AddToClassList("birth-name-tape");
        nameTapeEl.style.alignSelf = UIE.Align.Center;
        nameTapeEl.style.scale = new UIE.StyleScale(new UIE.Scale(new Vector2(1.3f, 1.3f)));
        nameTapeEl.style.opacity = 0f;

        nameTapeLabel = UIHelper.CreateLabel(babyName);
        nameTapeLabel.AddToClassList("birth-name-tape-text");
        UIHelper.ApplyFont(nameTapeLabel);
        nameTapeEl.Add(nameTapeLabel);

        // フレームの特徴バッジ行の前に挿入（カード内のフロー配置）
        var frame = UIE.UQueryExtensions.Q(birthResultCard, className: "birth-polaroid-frame");
        if (frame != null)
            frame.Insert(0, nameTapeEl);
        else
            birthResultCard.Add(nameTapeEl);

        // フェードイン
        float fadeIn = 0.2f;
        float elapsed = 0f;
        while (elapsed < fadeIn)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeIn);
            nameTapeEl.style.opacity = t;
            yield return null;
        }
        nameTapeEl.style.opacity = 1f;

        yield return new WaitForSeconds(0.1f);

        // スケールダウン + 回転アニメーション（大きい状態 → 貼り付き）
        float targetScale = 1.0f;
        float targetRotate = -2.5f;
        float startScale = 1.3f;

        float slideDuration = 0.3f;
        elapsed = 0f;
        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / slideDuration);
            float ease = 1f - (1f - t) * (1f - t) * (1f - t);

            float curScale = Mathf.Lerp(startScale, targetScale, ease);
            float curRotate = Mathf.Lerp(0f, targetRotate, ease);

            nameTapeEl.style.scale = new UIE.StyleScale(new UIE.Scale(new Vector2(curScale, curScale)));
            nameTapeEl.style.rotate = new UIE.StyleRotate(new UIE.Rotate(new UIE.Angle(curRotate, UIE.AngleUnit.Degree)));
            yield return null;
        }

        nameTapeEl.style.scale = new UIE.StyleScale(new UIE.Scale(new Vector2(targetScale, targetScale)));
        nameTapeEl.style.rotate = new UIE.StyleRotate(new UIE.Rotate(new UIE.Angle(targetRotate, UIE.AngleUnit.Degree)));

        // 「ペタッ」バウンス効果
        float bounceDuration = 0.12f;
        elapsed = 0f;
        while (elapsed < bounceDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / bounceDuration);
            float bounceScale = targetScale + 0.08f * Mathf.Sin(t * Mathf.PI);
            nameTapeEl.style.scale = new UIE.StyleScale(new UIE.Scale(new Vector2(bounceScale, bounceScale)));
            yield return null;
        }
        nameTapeEl.style.scale = new UIE.StyleScale(new UIE.Scale(new Vector2(targetScale, targetScale)));

        // 「ペタッ」SE
        if (seSource != null && sePeta != null)
            seSource.PlayOneShot(sePeta, 1.0f);

        // 光パーティクル演出
        StartCoroutine(SpawnTapeSparkles());
    }

    IEnumerator SpawnTapeSparkles()
    {
        if (nameTapeEl == null) yield break;

        // テープの周囲にキラキラを配置（テープ要素内に absolute で）
        for (int i = 0; i < 8; i++)
        {
            var sparkle = UIHelper.CreateLabel("*");
            sparkle.AddToClassList("birth-tape-sparkle");
            UIHelper.ApplyFont(sparkle);
            sparkle.style.position = UIE.Position.Absolute;
            sparkle.style.left = new UIE.StyleLength(new UIE.Length(Random.Range(10f, 90f), UIE.LengthUnit.Percent));
            sparkle.style.top = new UIE.StyleLength(new UIE.Length(Random.Range(-50f, 100f), UIE.LengthUnit.Percent));
            sparkle.style.fontSize = Random.Range(18, 32);
            sparkle.style.opacity = 1f;

            nameTapeEl.Add(sparkle);

            StartCoroutine(FadeOutSparkle(sparkle));
            yield return new WaitForSeconds(0.04f);
        }
    }

    IEnumerator FadeOutSparkle(UIE.Label sparkle)
    {
        float duration = 0.6f;
        float elapsed = 0f;
        float driftY = Random.Range(-80f, -20f);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            sparkle.style.top = new UIE.StyleLength(
                sparkle.resolvedStyle.top + driftY * Time.deltaTime);
            sparkle.style.opacity = 1f - t;
            float s = 1f + 0.3f * Mathf.Sin(t * Mathf.PI);
            sparkle.style.scale = new UIE.StyleScale(new UIE.Scale(new Vector2(s, s)));
            yield return null;
        }
        sparkle.RemoveFromHierarchy();
    }

    IEnumerator SwitchToShareMode()
    {
        isShareMode = true;

        yield return new WaitForSeconds(0.3f);

        if (overlayRoot == null) yield break;

        // SNSシェアボタンをフェードイン
        var shareBtnWrapper = new UIE.VisualElement();
        shareBtnWrapper.AddToClassList("birth-share-wrapper");
        shareBtnWrapper.style.opacity = 0f;

        // シェアボタン
        shareBtn = new UIE.Button();
        shareBtn.AddToClassList("birth-share-btn");
        UIHelper.ApplyFont(shareBtn);
        shareBtn.text = "シェアする";
        shareBtn.clicked += OnShareButtonClicked;
        shareBtnWrapper.Add(shareBtn);

        // しんだんボタン
        var diagnosisBtn = new UIE.Button();
        diagnosisBtn.AddToClassList("birth-diagnosis-btn");
        UIHelper.ApplyFont(diagnosisBtn);
        diagnosisBtn.text = Localization.Get("diagnosis_btn");
        diagnosisBtn.clicked += () => ShowDiagnosisModal();
        shareBtnWrapper.Add(diagnosisBtn);

        // バトルへ進むボタン
        var battleBtn = new UIE.Button();
        battleBtn.AddToClassList("birth-share-battle-btn");
        UIHelper.ApplyFont(battleBtn);
        battleBtn.text = "おあそびへ";
        battleBtn.clicked += () => StartCoroutine(GoToBattleFromShare());
        shareBtnWrapper.Add(battleBtn);

        overlayRoot.Add(shareBtnWrapper);

        // シェアモードでは photoBtn のアップロード機能を無効化（ぷにぷに優先）
        if (uploadImageBtn != null)
        {
            uploadImageBtn.clicked -= PickImageFromGallery;
            uploadImageBtn.pickingMode = UIE.PickingMode.Ignore;
            Debug.Log("[BirthSystem] Disabled photoBtn click for share mode");
        }

        // ぷにぷにインタラクション セットアップ
        SetupBabyFaceInteractions();

        // フェードイン
        float fadeIn = 0.4f;
        float elapsed = 0f;
        while (elapsed < fadeIn)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeIn);
            // ease-out
            float ease = 1f - (1f - t) * (1f - t);
            shareBtnWrapper.style.opacity = ease;
            yield return null;
        }
        shareBtnWrapper.style.opacity = 1f;
    }

    void OnShareButtonClicked()
    {
        // スクリーンショット撮影 → シェア
        TakeScreenshot();
    }

    IEnumerator GoToBattleFromShare()
    {
        yield return StartCoroutine(ShowChapterTitle());
        SceneManager.LoadScene("BattleScene");
    }

    IEnumerator ShowChapterTitle()
    {
        if (overlayRoot == null) yield break;

        // メインカラー背景
        Color bgColor = new Color(0.969f, 0.906f, 0.808f); // #F7E7CE
        Color textColor = new Color(0.051f, 0.051f, 0.078f); // #0d0d14
        Color subColor = new Color(1f, 0.718f, 0.773f); // #FFB7C5

        var panel = new UIE.VisualElement();
        panel.style.position = UIE.Position.Absolute;
        panel.style.left = 0; panel.style.right = 0;
        panel.style.top = 0; panel.style.bottom = 0;
        panel.style.backgroundColor = new Color(bgColor.r, bgColor.g, bgColor.b, 0f);
        panel.style.alignItems = UIE.Align.Center;
        panel.style.justifyContent = UIE.Justify.Center;
        overlayRoot.Add(panel);

        // フェードイン
        float elapsed = 0f;
        while (elapsed < 0.8f)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / 0.8f);
            panel.style.backgroundColor = new Color(bgColor.r, bgColor.g, bgColor.b, t);
            yield return null;
        }
        panel.style.backgroundColor = bgColor;

        yield return new WaitForSeconds(0.3f);

        // 章タイトルテキスト
        var titleLabel = UIHelper.CreateLabel("第一章");
        UIHelper.ApplyFontBold(titleLabel);
        titleLabel.style.fontSize = 112;
        titleLabel.style.color = new Color(textColor.r, textColor.g, textColor.b, 0f);
        titleLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        titleLabel.style.letterSpacing = 8;
        panel.Add(titleLabel);

        // サブタイトル
        var subLabel = UIHelper.CreateLabel("はじめての おあそび");
        UIHelper.ApplyFontBold(subLabel);
        subLabel.style.fontSize = 76;
        subLabel.style.color = new Color(subColor.r, subColor.g, subColor.b, 0f);
        subLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        subLabel.style.marginTop = 16;
        panel.Add(subLabel);

        // テキストフェードイン
        elapsed = 0f;
        while (elapsed < 1.0f)
        {
            elapsed += Time.deltaTime;
            float a = Mathf.Clamp01(elapsed / 1.0f);
            titleLabel.style.color = new Color(textColor.r, textColor.g, textColor.b, a);
            subLabel.style.color = new Color(subColor.r, subColor.g, subColor.b, a);
            yield return null;
        }
        titleLabel.style.color = textColor;
        subLabel.style.color = subColor;

        yield return new WaitForSeconds(1.5f);

        // フェードアウト
        elapsed = 0f;
        while (elapsed < 1.0f)
        {
            elapsed += Time.deltaTime;
            float a = 1f - Mathf.Clamp01(elapsed / 1.0f);
            titleLabel.style.color = new Color(textColor.r, textColor.g, textColor.b, a);
            subLabel.style.color = new Color(subColor.r, subColor.g, subColor.b, a);
            panel.style.backgroundColor = new Color(bgColor.r, bgColor.g, bgColor.b, a);
            yield return null;
        }

        yield return new WaitForSeconds(0.3f);
        panel.RemoveFromHierarchy();
    }

    void OnNameConfirmUIToolkit()
    {
        if (nameTextField == null) return;

        string text = nameTextField.value != null ? nameTextField.value.Trim() : "";
        if (string.IsNullOrEmpty(text))
        {
            StartCoroutine(ShakeNameField());
            return;
        }

        enteredName = text;
        waitingForNameInput = false;
    }

    IEnumerator ShakeNameField()
    {
        if (nameTextField == null) yield break;
        float duration = 0.3f;
        float elapsed = 0f;
        float magnitude = 15f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float x = Mathf.Sin(elapsed * 40f) * magnitude * (1f - elapsed / duration);
            nameTextField.style.translate = new UIE.Translate(x, 0);
            yield return null;
        }
        nameTextField.style.translate = new UIE.Translate(0, 0);
    }

    public void OnSaveYes()
    {
        saveConfirmResult = true;
        waitingForSaveConfirm = false;
    }

    public void OnSaveNo()
    {
        saveConfirmResult = false;
        waitingForSaveConfirm = false;
    }

    // ===== メインアニメーション =====

    IEnumerator SpinRouletteAnimation()
    {
        Debug.Log("[BirthSystem] SpinRouletteAnimation started");
        isAnimating = true;

        // レイアウトをルーレット用にリセット
        ResetToRouletteLayout();

        // 前回のカスタム画像パスをクリア（前の赤ちゃんの顔が残らないように）
        if (DataCarrier.Instance != null)
            DataCarrier.Instance.customBabyImagePath = "";

        // ボタンモードをリセット
        isUploadMode = true;

        // ボタンを即非表示、?マークを隠す、背景も非表示
        if (generateLifeButton != null) generateLifeButton.SetActive(false);

        if (anotherGalButton != null) anotherGalButton.SetActive(false);
        if (gotoBattleButton != null) gotoBattleButton.SetActive(false);
        if (introText != null) introText.gameObject.SetActive(false);

        // 結果を先に決定
        int fIdx = Random.Range(0, NewFathers.Length);
        int mIdx = Random.Range(0, NewMothers.Length);
        ParentData father = NewFathers[fIdx];
        ParentData mother = NewMothers[mIdx];

        // ステータス計算（正規分布ランダム：極端な値は出にくい）
        // 親の個体差（±30の範囲、大半は±10程度に収まる）
        int f_atk = DeviationRandom(father.atk, 30);
        int f_def = DeviationRandom(father.def, 30);
        int f_hp  = DeviationRandom(father.hp, 50);
        int m_atk = DeviationRandom(mother.atk, 30);
        int m_def = DeviationRandom(mother.def, 30);
        int m_hp  = DeviationRandom(mother.hp, 50);

        // 子供のステータス = 両親の平均 + 個体差（±20の範囲）
        int c_atk      = (f_atk + m_atk) / 2 + GaussianRandomRange(-20, 20);
        int c_def      = (f_def + m_def) / 2 + GaussianRandomRange(-20, 20);
        int c_hp       = (f_hp + m_hp) / 2 + GaussianRandomRange(-40, 40);
        int c_intelligence = (father.intelligence + mother.intelligence) / 2 + GaussianRandomRange(-30, 30);
        int c_athletic = (father.athletic + mother.athletic) / 2 + GaussianRandomRange(-30, 30);
        int c_luck = (father.luck + mother.luck) / 2 + GaussianRandomRange(-20, 20);
        int c_fortune = (father.fortune + mother.fortune) / 2 + GaussianRandomRange(-20, 20);

        // 最低値の保証
        c_atk = Mathf.Max(1, c_atk);
        c_def = Mathf.Max(1, c_def);
        c_hp = Mathf.Max(50, c_hp);
        c_intelligence = Mathf.Max(1, c_intelligence);
        c_athletic = Mathf.Max(1, c_athletic);
        c_luck = Mathf.Max(1, c_luck);
        c_fortune = Mathf.Max(1, c_fortune);

        string trait1 = Traits[Random.Range(0, Traits.Length)];

        // ── フェーズ1: パネル表示、一人ずつ表示 ──
        if (parentPanel != null) parentPanel.style.display = UIE.DisplayStyle.Flex;
        if (childStatusText != null) childStatusText.text = "";
        if (babyBioLabel != null) babyBioLabel.text = "";

        // 紹介文を非表示にリセット（visibilityでレイアウト確保）
        if (fatherIntroText != null) fatherIntroText.style.visibility = UIE.Visibility.Hidden;
        if (motherIntroText != null) motherIntroText.style.visibility = UIE.Visibility.Hidden;
        if (nextButtonEl != null) nextButtonEl.style.display = UIE.DisplayStyle.None;

        // 父親カードのみ表示、母親カードは非表示
        if (fatherCard != null) fatherCard.style.display = UIE.DisplayStyle.Flex;
        if (motherCard != null) motherCard.style.display = UIE.DisplayStyle.None;
        // ── フェーズ2: 父親ルーレット（父親カードのみ表示） ──
        // 高速シャッフル（15回×0.06秒）
        for (int i = 0; i < 15; i++)
        {
            int tmpF = Random.Range(0, NewFathers.Length);
            ShowSingleParentPreview(tmpF, NewFathers[tmpF], true);
            PlayBabyVoiceSE(true);
            yield return new WaitForSeconds(0.06f);
        }
        // 減速シャッフル（6回）
        for (int i = 0; i < 6; i++)
        {
            int tmpF = (i < 4) ? Random.Range(0, NewFathers.Length) : fIdx;
            ShowSingleParentPreview(tmpF, NewFathers[tmpF], true);
            PlayBabyVoiceSE(true);
            float delay = Mathf.Lerp(0.12f, 0.35f, i / 5f);
            yield return new WaitForSeconds(delay);
        }
        // 父親確定
        ShowSingleParentPreview(fIdx, father, true);
        PlayBabyVoiceSE(true);
        // カットイン演出
        yield return StartCoroutine(ShowParentCutin(father.name));
        // 紹介文表示（0.5秒遅延のふわっとフェードアップ）
        if (fatherIntroText != null)
        {
            fatherIntroText.text = Localization.GetParentIntro(father.name).Replace(" / ", "\n");
            StartCoroutine(FadeUpIntroText(fatherIntroText));
        }
        // キャラクター背景パーティクル開始
        fatherParticleCoroutine = StartCoroutine(SpawnCharacterParticles(fatherFaceImage, father));
        // 「愛する女性を探す」ボタンで待機
        yield return new WaitForSeconds(0.5f);
        CreateStatusCheckButton(father, fatherCard, true);
        SetNextButtonText(Localization.Get("birth_find_mother"));
        ShowNextButton();
        waitingForNext = true;
        while (waitingForNext) yield return null;
        HideNextButton();
        DestroyStatusCheckButton();
        SetNextButtonText(Localization.Get("birth_next"));

        // ── フェーズ3: 「惹かれ合う、もう一つの魂」カットイン → 母親ルーレット ──
        yield return StartCoroutine(ShowMotherCutinText(Localization.Get("cutin_who_mother")));

        StopCharacterParticles();
        if (fatherCard != null) fatherCard.style.display = UIE.DisplayStyle.None;
        if (motherCard != null) motherCard.style.display = UIE.DisplayStyle.Flex;
        // 高速シャッフル（15回×0.06秒）
        for (int i = 0; i < 15; i++)
        {
            int tmpM = Random.Range(0, NewMothers.Length);
            ShowSingleParentPreview(tmpM, NewMothers[tmpM], false);
            PlayBabyVoiceSE(false);
            yield return new WaitForSeconds(0.06f);
        }
        // 減速シャッフル（6回）
        for (int i = 0; i < 6; i++)
        {
            int tmpM = (i < 4) ? Random.Range(0, NewMothers.Length) : mIdx;
            ShowSingleParentPreview(tmpM, NewMothers[tmpM], false);
            PlayBabyVoiceSE(false);
            float delay = Mathf.Lerp(0.12f, 0.35f, i / 5f);
            yield return new WaitForSeconds(delay);
        }
        // 母親確定
        ShowSingleParentPreview(mIdx, mother, false);
        PlayBabyVoiceSE(false);
        // カットイン演出
        yield return StartCoroutine(ShowParentCutin(mother.name));
        // 紹介文表示（0.5秒遅延のふわっとフェードアップ）
        if (motherIntroText != null)
        {
            motherIntroText.text = Localization.GetParentIntro(mother.name).Replace(" / ", "\n");
            StartCoroutine(FadeUpIntroText(motherIntroText));
        }
        // キャラクター背景パーティクル開始
        motherParticleCoroutine = StartCoroutine(SpawnCharacterParticles(motherFaceImage, mother));
        // 「恋の始まり」ボタンで待機
        yield return new WaitForSeconds(0.5f);
        CreateStatusCheckButton(mother, motherCard, true);
        SetNextButtonText(Localization.Get("birth_love_begin"));
        ShowNextButton();
        waitingForNext = true;
        while (waitingForNext) yield return null;
        HideNextButton();
        DestroyStatusCheckButton();
        SetNextButtonText(Localization.Get("birth_next"));

        // ── フェーズ4: 性別をランダム決定 ──
        selectedGender = Random.Range(0, 2) == 0 ? "男の子" : "女の子";
        Debug.Log($"[BirthSystem] Gender random: {selectedGender}");

        // パーティクル停止 + 両親カード非表示
        StopCharacterParticles();
        if (fatherCard != null) fatherCard.style.display = UIE.DisplayStyle.None;
        if (motherCard != null) motherCard.style.display = UIE.DisplayStyle.None;


        // ── フェーズ4.4: 「運命が重なる刻」光の引力カットイン ──
        yield return StartCoroutine(ShowFateCutin(father.faceColor, mother.faceColor));

        // ── フェーズ4.5: 恋愛ストーリー表示 ──
        yield return StartCoroutine(ShowLoveStory(father, mother, fIdx, mIdx));

        // ── フェーズ4.6: ルナの場合80%で子供に恵まれない ──
        if (mother.name == "ルナ")
        {
            bool lunaSuccess = Random.Range(0, 100) < 20; // 20%で成功
            if (!lunaSuccess)
            {
                // 失敗演出
                yield return StartCoroutine(ShowFailureSequence("birth_luna", new Color(0.8f, 0.8f, 1f)));
                isAnimating = false;
                yield break; // ここで終了
            }
            else
            {
                // 成功時はステータスボーナス（+20%）
                c_atk = Mathf.RoundToInt(c_atk * 1.2f);
                c_def = Mathf.RoundToInt(c_def * 1.2f);
                c_hp = Mathf.RoundToInt(c_hp * 1.2f);
                c_intelligence = Mathf.RoundToInt(c_intelligence * 1.2f);
                c_athletic = Mathf.RoundToInt(c_athletic * 1.2f);
                c_luck = Mathf.RoundToInt(c_luck * 1.2f);
                c_fortune = Mathf.RoundToInt(c_fortune * 1.2f);
            }
        }

        // ── フェーズ4.7: サクラ（ぶじゅつの達人）の場合20%で子供に恵まれない ──
        if (mother.name == "サクラ")
        {
            bool sakuraSuccess = Random.Range(0, 100) < 80; // 80%で成功
            if (!sakuraSuccess)
            {
                yield return StartCoroutine(ShowFailureSequence("birth_sakura", new Color(1f, 0.8f, 0.85f)));
                isAnimating = false;
                yield break;
            }
        }

        // ── フェーズ4.8: ミサトの場合30%で子供に恵まれない ──
        if (mother.name == "ミサト")
        {
            bool misatoSuccess = Random.Range(0, 100) < 70; // 70%で成功
            if (!misatoSuccess)
            {
                yield return StartCoroutine(ShowFailureSequence("birth_misato", new Color(0.7f, 0.9f, 1f)));
                isAnimating = false;
                yield break;
            }
            else
            {
                // 成功時はステータスボーナス（+3%）
                c_atk = Mathf.RoundToInt(c_atk * 1.03f);
                c_def = Mathf.RoundToInt(c_def * 1.03f);
                c_hp = Mathf.RoundToInt(c_hp * 1.03f);
                c_intelligence = Mathf.RoundToInt(c_intelligence * 1.03f);
                c_athletic = Mathf.RoundToInt(c_athletic * 1.03f);
                c_luck = Mathf.RoundToInt(c_luck * 1.03f);
                c_fortune = Mathf.RoundToInt(c_fortune * 1.03f);
            }
        }

        // ── フェーズ4.9: ヒナタ（美容帝国CEO）の場合50%で子供に恵まれない ──
        if (mother.name == "ヒナタ")
        {
            bool hinataSuccess = Random.Range(0, 100) < 50; // 50%で成功
            if (!hinataSuccess)
            {
                yield return StartCoroutine(ShowFailureSequence("birth_hinata", new Color(0.8f, 0.7f, 1f)));
                isAnimating = false;
                yield break;
            }
            else
            {
                // 成功時はステータスボーナス（+5%）
                c_atk = Mathf.RoundToInt(c_atk * 1.05f);
                c_def = Mathf.RoundToInt(c_def * 1.05f);
                c_hp = Mathf.RoundToInt(c_hp * 1.05f);
                c_intelligence = Mathf.RoundToInt(c_intelligence * 1.05f);
                c_athletic = Mathf.RoundToInt(c_athletic * 1.05f);
                c_luck = Mathf.RoundToInt(c_luck * 1.05f);
                c_fortune = Mathf.RoundToInt(c_fortune * 1.05f);
            }
        }

        // ── フェーズ4.10: カエデの場合10%で子供に恵まれない ──
        if (mother.name == "カエデ")
        {
            bool kaedeSuccess = Random.Range(0, 100) < 90; // 90%で成功
            if (!kaedeSuccess)
            {
                yield return StartCoroutine(ShowFailureSequence("birth_kaede", new Color(0.6f, 1f, 0.8f)));
                isAnimating = false;
                yield break;
            }
            else
            {
                // 成功時はステータスボーナス（+5%）
                c_atk = Mathf.RoundToInt(c_atk * 1.05f);
                c_def = Mathf.RoundToInt(c_def * 1.05f);
                c_hp = Mathf.RoundToInt(c_hp * 1.05f);
                c_intelligence = Mathf.RoundToInt(c_intelligence * 1.05f);
                c_athletic = Mathf.RoundToInt(c_athletic * 1.05f);
                c_luck = Mathf.RoundToInt(c_luck * 1.05f);
                c_fortune = Mathf.RoundToInt(c_fortune * 1.05f);
            }
        }

        // ── フェーズ5: 聖なる光の誕生演出 ──
        Debug.Log("[BirthSystem] Phase 5: Sacred birth cutin");
        yield return StartCoroutine(ShowBirthCutin());

        // ── フェーズ5.1: 誕生アナウンス ──
        yield return StartCoroutine(ShowBirthAnnouncement(father, mother));

        // カットイン除去後に1フレーム描画を挟み、画面遷移を反映させる
        yield return null;

        // ── フェーズ5.5: レイアウト切り替え（親を端に、赤ちゃんを大きく中央に） ──
        Debug.Log("[BirthSystem] Phase 5.5: Layout switch and baby display");
        SwitchToBirthResultLayout(BabySynthesizer.DetermineRank(c_fortune));

        // レイアウト反映のため1フレーム描画を挟む
        yield return null;

        // ── フェーズ6: ステータス1行ずつ表示 ──
        // 上位1%判定（STAR BABY判定）、上位10%判定（大物判定）
        bool isGodBaby = IsGodBaby(c_atk, c_def, c_hp, c_intelligence, c_athletic);

        // BabySynthesizer でレイヤー合成表示
        string genderKey = selectedGender == "男の子" ? "male" : "female";

        // Canvas上のbabyFaceを非表示（SpriteRenderer合成に切り替え）
        if (babyFace != null)
            babyFace.gameObject.SetActive(false);

        var synthParams = new SynthesizeParams
        {
            fortune = c_fortune,
            isGodBaby = isGodBaby,
            fatherImageName = father.imageName,
            motherImageName = mother.imageName,
            genderKey = genderKey,
            father = father,
            mother = mother,
            babyAtk = c_atk,
            babyDef = c_def,
            babyHp = c_hp,
            babyIntelligence = c_intelligence,
            babyAthletic = c_athletic,
            babyLuck = c_luck,
            customImagePath = DataCarrier.Instance != null ? DataCarrier.Instance.customBabyImagePath : "",
            fatherItemPath = GetParentItemPath(father.imageName, true),
            motherItemPath = GetParentItemPath(mother.imageName, false),
        };
        lastSynthParams = synthParams;
        Sprite synthSprite = babySynthesizer.Synthesize(synthParams);
        if (synthSprite != null)
        {
            DisplaySynthesizedBaby(synthSprite);
            SaveSynthBabyImage();
        }

        // 結果コンテナを表示（合成完了後）
        var resultContainer = UIE.UQueryExtensions.Q(overlayRoot, className: "birth-result-container");
        if (resultContainer != null) resultContainer.style.opacity = 1;

        // ── 誕生インパクト演出（SE + シェイク + 紙吹雪） ──
        PlayBirthRevealEffect();

        Debug.Log($"[BirthSystem] BabySynthesizer.Synthesize() called, fortune={c_fortune}, rank={BabySynthesizer.DetermineRank(c_fortune)}");

        // 画像アップロードボタン（赤ちゃん画像の下に配置）
        if (birthResultCard != null) CreateUploadButton();

        // スクリーンショットボタン（シェア用）
        if (birthResultCard != null) CreateScreenshotButton();

        bool isPromisingBaby = !isGodBaby && IsPromisingBaby(c_atk, c_def, c_hp, c_intelligence, c_athletic);

        // ── 稲妻演出（STAR BABY or 大物の場合） ──
        if (isGodBaby || isPromisingBaby)
        {
            yield return StartCoroutine(LightningEffect(isGodBaby));
        }

        // ── コンパクトポラロイド: ステータス表示 ──

        // 1. 特徴バッジ追加
        if (traitBadgeRow != null)
        {
            var badge = UIHelper.CreateLabel(Localization.GetTrait(trait1));
            badge.AddToClassList("birth-trait-badge");
            UIHelper.ApplyFont(badge);
            traitBadgeRow.Add(badge);
        }
        yield return new WaitForSeconds(0.3f);

        // 2. 性別＋名前表示
        string genderColor = selectedGender == "男の子" ? "#66ccff" : "#ff99cc";
        string displayGender = Localization.GetGender(selectedGender);
        if (babyNameLabel != null)
            babyNameLabel.text = $"<color={genderColor}>{displayGender}</color>";
        yield return new WaitForSeconds(0.2f);

        // 2.5. Bio（紹介文）表示
        if (babyBioLabel != null)
        {
            string summaryKey = $"{father.name}_{mother.name}";
            string summary = LifeSummaries.ContainsKey(summaryKey)
                ? LifeSummaries[summaryKey]
                : "\u6CE2\u4E71\u4E07\u4E08\u306E\u4EBA\u751F\u304C\u59CB\u307E\u308B\u3002";
            babyBioLabel.text = $"\u300C{summary}\u300D";
        }
        yield return new WaitForSeconds(0.3f);

        // 3. ステータスメモ
        string line1 = $"ごきげん {c_hp}  ぬくもり {c_atk}  おちつき {c_def}";
        string line2 = $"ちえ {c_intelligence}  体 {c_athletic}  運 {c_luck}  財 {c_fortune}";
        if (memoTextLabel != null) memoTextLabel.text = line1;
        yield return new WaitForSeconds(0.2f);
        if (memoTextLabel != null) memoTextLabel.text = line1 + "\n" + line2;
        yield return new WaitForSeconds(0.2f);

        // 4. 両親情報
        if (parentsLineLabel != null)
        {
            string fIntro = father.intro.Contains("/") ? father.intro.Split('/')[0].Trim() : father.intro;
            string mIntro = mother.intro.Contains("/") ? mother.intro.Split('/')[0].Trim() : mother.intro;
            parentsLineLabel.text = $"{father.name}\uFF08{fIntro}\uFF09\u00D7 {mother.name}\uFF08{mIntro}\uFF09";
        }

        // ── DataCarrier に保存 ──
        if (DataCarrier.Instance != null)
        {
            DataCarrier.Instance.babyAtk = c_atk;
            DataCarrier.Instance.babyDef = c_def;
            DataCarrier.Instance.babyHp = c_hp;
            DataCarrier.Instance.babyIntelligence = c_intelligence;
            DataCarrier.Instance.babyAthletic = c_athletic;
            DataCarrier.Instance.babyLuck = c_luck;
            DataCarrier.Instance.babyFortune = c_fortune;
            DataCarrier.Instance.trait1 = trait1;
            DataCarrier.Instance.fatherName = father.name;
            DataCarrier.Instance.motherName = mother.name;
            DataCarrier.Instance.babyGender = selectedGender;
            DataCarrier.Instance.isGodBaby = isGodBaby;
        }

        // ── 「運命を吹き込む」ボタンを表示（画像アップロード誘導） ──
        yield return new WaitForSeconds(0.3f);
        SwitchToUploadMode();
        if (gotoBattleButton != null)
        {
            gotoBattleButton.SetActive(true);
            StartCoroutine(NamingButtonBounceAnimation());
        }

        isAnimating = false;
    }

    // ===== 誕生インパクト演出 =====

    void PlayBirthRevealEffect()
    {
        if (seLevelUp != null)
            seSource.PlayOneShot(seLevelUp, 1f);
        StartCoroutine(ScreenShakeCoroutine(0.5f, 8f));
        SpawnConfetti();
    }

    IEnumerator ScreenShakeCoroutine(float duration, float intensity)
    {
        var cam = Camera.main;
        if (cam == null) yield break;
        Vector3 originalPos = cam.transform.localPosition;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float decay = 1f - (elapsed / duration);
            float x = Random.Range(-1f, 1f) * intensity * decay;
            float y = Random.Range(-1f, 1f) * intensity * decay;
            cam.transform.localPosition = originalPos + new Vector3(x, y, 0);
            yield return null;
        }
        cam.transform.localPosition = originalPos;
    }

    void SpawnConfetti()
    {
        var cam = Camera.main;
        if (cam == null) return;

        var go = new GameObject("BirthConfetti");
        float camH = cam.orthographicSize;
        float camW = camH * cam.aspect;
        go.transform.position = new Vector3(
            cam.transform.position.x,
            cam.transform.position.y + camH * 0.5f,
            0f);

        var ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.duration = 0.3f;
        main.loop = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(2f, 3.5f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(2f, 6f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.25f);
        main.gravityModifier = 0.8f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startRotation = new ParticleSystem.MinMaxCurve(0, Mathf.PI * 2);

        // 紙吹雪カラー（ピンク〜ゴールドのランダム）
        var colorGrad = new Gradient();
        colorGrad.mode = GradientMode.Fixed;
        colorGrad.SetKeys(
            new[] {
                new GradientColorKey(new Color(1f, 0.3f, 0.5f), 0f),
                new GradientColorKey(new Color(1f, 0.85f, 0.2f), 0.25f),
                new GradientColorKey(new Color(0.3f, 0.8f, 1f), 0.5f),
                new GradientColorKey(new Color(0.5f, 1f, 0.5f), 0.75f),
                new GradientColorKey(new Color(1f, 0.6f, 0.2f), 1f),
            },
            new[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(1, 1) }
        );
        main.startColor = new ParticleSystem.MinMaxGradient(colorGrad);

        // バースト発生（一瞬で80個）
        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 80) });

        // 画面幅いっぱいに散布
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(camW * 2f, 0.5f, 0.5f);

        // フェードアウト
        var colorOverLife = ps.colorOverLifetime;
        colorOverLife.enabled = true;
        var fadeGrad = new Gradient();
        fadeGrad.SetKeys(
            new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.7f), new GradientAlphaKey(0f, 1f) }
        );
        colorOverLife.color = fadeGrad;

        // 回転（ひらひら感）
        var rotOverLife = ps.rotationOverLifetime;
        rotOverLife.enabled = true;
        rotOverLife.z = new ParticleSystem.MinMaxCurve(-3f, 3f);

        // マテリアル
        var rend = go.GetComponent<ParticleSystemRenderer>();
        rend.material = new Material(Shader.Find("Sprites/Default"));
        rend.sortingOrder = 100;

        ps.Play();
        Destroy(go, 5f);
    }

    // ===== フラッシュ演出 =====

    IEnumerator FlashEffect()
    {
        if (flashOverlay == null) yield break;

        // 白フラッシュ
        flashOverlay.color = new Color(1f, 1f, 1f, 0.9f);
        flashOverlay.gameObject.SetActive(true);

        float duration = 0.5f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0.9f, 0f, elapsed / duration);
            flashOverlay.color = new Color(1f, 1f, 1f, alpha);
            yield return null;
        }

        flashOverlay.gameObject.SetActive(false);
    }

    // ===== 稲妻演出 =====

    IEnumerator LightningEffect(bool isGodBaby)
    {
        if (lightningContainer == null) yield break;

        lightningContainer.SetActive(true);

        // 稲妻の回数（STAR BABYは多め）
        int lightningCount = isGodBaby ? 5 : 3;

        for (int i = 0; i < lightningCount; i++)
        {
            // 稲妻ボルトを生成
            var bolt = CreateLightningBolt();

            // フラッシュ効果
            if (flashOverlay != null)
            {
                flashOverlay.gameObject.SetActive(true);
                flashOverlay.color = isGodBaby
                    ? new Color(1f, 0.9f, 0.3f, 0.7f)  // STAR BABY: 金色フラッシュ
                    : new Color(0.5f, 1f, 0.7f, 0.5f); // 大物: 緑色フラッシュ
            }

            yield return new WaitForSeconds(0.08f);

            // フラッシュを消す
            if (flashOverlay != null)
            {
                flashOverlay.color = new Color(1f, 1f, 1f, 0f);
                flashOverlay.gameObject.SetActive(false);
            }

            yield return new WaitForSeconds(0.05f);

            // 稲妻を削除
            if (bolt != null) Destroy(bolt);

            yield return new WaitForSeconds(Random.Range(0.1f, 0.2f));
        }

        // 最後に大きなフラッシュ
        if (flashOverlay != null)
        {
            flashOverlay.gameObject.SetActive(true);
            flashOverlay.color = isGodBaby
                ? new Color(1f, 0.85f, 0f, 0.9f)  // STAR BABY: 強い金色
                : new Color(0.3f, 1f, 0.5f, 0.7f); // 大物: 強い緑色

            float duration = 0.4f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(0.9f, 0f, elapsed / duration);
                flashOverlay.color = isGodBaby
                    ? new Color(1f, 0.85f, 0f, alpha)
                    : new Color(0.3f, 1f, 0.5f, alpha);
                yield return null;
            }
            flashOverlay.gameObject.SetActive(false);
        }

        lightningContainer.SetActive(false);
    }

    GameObject CreateLightningBolt()
    {
        var bolt = new GameObject("LightningBolt");
        bolt.transform.SetParent(lightningContainer.transform, false);

        var rect = bolt.AddComponent<RectTransform>();

        // ランダムな位置に稲妻を配置
        float xPos = Random.Range(-300f, 300f);
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(xPos, 0);
        rect.sizeDelta = new Vector2(80, 600);

        // 稲妻の形を作る（複数のセグメント）
        float currentY = 0;
        float segmentHeight = 60;
        float currentX = 0;

        for (int i = 0; i < 10; i++)
        {
            var segment = new GameObject($"Segment{i}");
            segment.transform.SetParent(bolt.transform, false);

            var segRect = segment.AddComponent<RectTransform>();
            float nextX = currentX + Random.Range(-40f, 40f);

            segRect.anchorMin = new Vector2(0.5f, 1);
            segRect.anchorMax = new Vector2(0.5f, 1);
            segRect.anchoredPosition = new Vector2(currentX, currentY);
            segRect.sizeDelta = new Vector2(8 - i * 0.5f, segmentHeight);

            // 角度を計算
            float angle = Mathf.Atan2(nextX - currentX, -segmentHeight) * Mathf.Rad2Deg;
            segRect.localRotation = Quaternion.Euler(0, 0, -angle);

            var img = segment.AddComponent<Image>();
            img.color = new Color(1f, 1f, 0.8f, 0.95f); // 明るい黄白色
            img.raycastTarget = false;

            currentY -= segmentHeight;
            currentX = nextX;
        }

        return bolt;
    }

    // ===== ルーレット「バブ」SE（ピッチ変化付き） =====
    void PlayBabyVoiceSE(bool isFather)
    {
        if (seBabySource == null || seBabyVoice == null) return;
        // 父親: 基本1.1倍、母親: 基本1.3倍 + ランダム揺らぎ
        float basePitch = isFather ? 1.1f : 1.3f;
        seBabySource.pitch = basePitch + Random.Range(-0.1f, 0.1f);
        seBabySource.PlayOneShot(seBabyVoice, 0.7f);
    }

    // ===== ルーレット中のプレビュー表示（父 or 母 個別） =====

    void ShowSingleParentPreview(int idx, ParentData data, bool isFather)
    {
        UIE.VisualElement faceImg = isFather ? fatherFaceImage : motherFaceImage;
        UIE.Label nameT = isFather ? fatherNameText : motherNameText;
        string prefix = isFather ? Localization.Get("birth_father_prefix") : Localization.Get("birth_mother_prefix");

        if (faceImg != null)
        {
            Sprite sp = Resources.Load<Sprite>($"Parents/{data.imageName}");
            if (sp != null)
            {
                faceImg.style.backgroundImage = new UIE.StyleBackground(sp);
                faceImg.style.backgroundColor = UIE.StyleKeyword.None;
            }
            else
            {
                faceImg.style.backgroundImage = UIE.StyleKeyword.None;
                faceImg.style.backgroundColor = data.faceColor;
            }
        }
        if (nameT != null)
            nameT.text = $"{prefix}: {Localization.GetParent(data.name)}";
    }

    // ===== 両親を小さく並べて表示（性別選択時） =====

    void ShowBothParentsMini(int fIdx, ParentData father, int mIdx, ParentData mother)
    {
        // 紹介文を非表示（visibilityでレイアウト確保）
        if (fatherIntroText != null) fatherIntroText.style.visibility = UIE.Visibility.Hidden;
        if (motherIntroText != null) motherIntroText.style.visibility = UIE.Visibility.Hidden;

        // 両カードを表示
        if (fatherCard != null) fatherCard.style.display = UIE.DisplayStyle.Flex;
        if (motherCard != null) motherCard.style.display = UIE.DisplayStyle.Flex;

        // カードを小さくして左右に並べる
        if (fatherCard != null)
        {
            fatherCard.style.width = new UIE.StyleLength(new UIE.Length(46, UIE.LengthUnit.Percent));
            fatherCard.style.maxWidth = 492;
            fatherCard.style.height = new UIE.StyleLength(new UIE.Length(32, UIE.LengthUnit.Percent));
            fatherCard.style.translate = new UIE.StyleTranslate(
                new UIE.Translate(new UIE.Length(-50, UIE.LengthUnit.Percent), new UIE.Length(-100, UIE.LengthUnit.Percent)));
            fatherCard.style.left = new UIE.StyleLength(new UIE.Length(25, UIE.LengthUnit.Percent));
            fatherCard.style.top = new UIE.StyleLength(new UIE.Length(25, UIE.LengthUnit.Percent));
        }
        if (motherCard != null)
        {
            motherCard.style.width = new UIE.StyleLength(new UIE.Length(46, UIE.LengthUnit.Percent));
            motherCard.style.maxWidth = 492;
            motherCard.style.height = new UIE.StyleLength(new UIE.Length(32, UIE.LengthUnit.Percent));
            motherCard.style.translate = new UIE.StyleTranslate(
                new UIE.Translate(new UIE.Length(-50, UIE.LengthUnit.Percent), new UIE.Length(-100, UIE.LengthUnit.Percent)));
            motherCard.style.left = new UIE.StyleLength(new UIE.Length(75, UIE.LengthUnit.Percent));
            motherCard.style.top = new UIE.StyleLength(new UIE.Length(25, UIE.LengthUnit.Percent));
        }

        // 顔画像を小さくリサイズ
        if (fatherFaceImage != null)
        {
            fatherFaceImage.style.width = new UIE.StyleLength(new UIE.Length(80, UIE.LengthUnit.Percent));
            fatherFaceImage.style.height = UIE.StyleKeyword.Auto;
        }
        if (motherFaceImage != null)
        {
            motherFaceImage.style.width = new UIE.StyleLength(new UIE.Length(80, UIE.LengthUnit.Percent));
            motherFaceImage.style.height = UIE.StyleKeyword.Auto;
        }

        // 確定済みの親を表示
        ShowSingleParentPreview(fIdx, father, true);
        ShowSingleParentPreview(mIdx, mother, false);
    }

    // カードをルーレット用の大きいサイズに戻す
    void ResetCardToFullSize(UIE.VisualElement card, UIE.VisualElement faceImg)
    {
        if (card == null) return;
        card.style.width = new UIE.StyleLength(new UIE.Length(94, UIE.LengthUnit.Percent));
        card.style.maxWidth = 1016;
        card.style.height = new UIE.StyleLength(new UIE.Length(68, UIE.LengthUnit.Percent));
        card.style.left = new UIE.StyleLength(new UIE.Length(50, UIE.LengthUnit.Percent));
        card.style.top = new UIE.StyleLength(new UIE.Length(50, UIE.LengthUnit.Percent));
        card.style.translate = new UIE.StyleTranslate(
            new UIE.Translate(new UIE.Length(-50, UIE.LengthUnit.Percent), new UIE.Length(-50, UIE.LengthUnit.Percent)));

        if (faceImg != null)
        {
            faceImg.style.width = new UIE.StyleLength(new UIE.Length(85, UIE.LengthUnit.Percent));
            faceImg.style.height = UIE.StyleKeyword.Auto;
        }
    }

    // ===== ステータス判定 =====

    // DetermineTrait は廃止 — 16種からランダムに選ばれる

    /// <summary>
    /// 上位1%の「STAR BABY」判定
    /// ポテンシャルスコアが閾値を超えるか、単一ステータスが極端に高い場合にtrue
    /// </summary>
    bool IsGodBaby(int atk, int def, int hp, int intelligence, int athletic)
    {
        // ポテンシャルスコア計算
        // 平均的な赤ちゃん: ATK50 + DEF50 + HP160/2 + INT60 + Athletic60 = 300
        // 上位1%閾値: 360以上（かなりの上振れが必要）
        int potentialScore = atk + def + (hp / 2) + intelligence + athletic;

        if (potentialScore >= 360)
            return true;

        // 単一ステータスが極端に高い場合も STAR BABY
        // 正規分布で約2.5σ以上 = 上位約1%
        if (atk >= 90) return true;      // 攻撃の天才
        if (def >= 90) return true;      // 防御の天才
        if (hp >= 250) return true;      // 生命力の塊
        if (intelligence >= 100) return true; // 超天才
        if (athletic >= 100) return true; // 超人アスリート

        return false;
    }

    /// <summary>
    /// 上位10%の「大物」判定
    /// ポテンシャルスコアが閾値を超えるか、単一ステータスがやや高い場合にtrue
    /// </summary>
    bool IsPromisingBaby(int atk, int def, int hp, int intelligence, int athletic)
    {
        // ポテンシャルスコア計算
        // 平均的な赤ちゃん: ATK50 + DEF50 + HP160/2 + INT60 + Athletic60 = 300
        // 上位10%閾値: 330以上（やや上振れが必要）
        int potentialScore = atk + def + (hp / 2) + intelligence + athletic;

        if (potentialScore >= 330)
            return true;

        // 単一ステータスがやや高い場合も 大物
        // 正規分布で約1.3σ以上 = 上位約10%
        if (atk >= 75) return true;      // 攻撃の才能
        if (def >= 75) return true;      // 防御の才能
        if (hp >= 210) return true;      // 生命力が高い
        if (intelligence >= 85) return true; // 秀才
        if (athletic >= 85) return true; // アスリート素質

        return false;
    }

    // ===== 正規分布ランダム（偏差値風） =====

    /// <summary>
    /// 正規分布に基づくランダム値を返す（Box-Muller法）
    /// 平均0、標準偏差1のガウス分布
    /// </summary>
    float GaussianRandom()
    {
        float u1 = 1.0f - Random.value; // (0,1]
        float u2 = 1.0f - Random.value;
        return Mathf.Sqrt(-2.0f * Mathf.Log(u1)) * Mathf.Sin(2.0f * Mathf.PI * u2);
    }

    /// <summary>
    /// 正規分布ランダム（指定範囲）
    /// 約68%が±1σ、約95%が±2σ、約99.7%が±3σに収まる
    /// </summary>
    int GaussianRandomRange(int min, int max)
    {
        float mean = (min + max) / 2f;
        float sigma = (max - min) / 6f; // ±3σで範囲をカバー
        float value = mean + GaussianRandom() * sigma;
        return Mathf.RoundToInt(Mathf.Clamp(value, min, max));
    }

    /// <summary>
    /// 偏差値風ランダム（基準値からの変動）
    /// baseValue: 基準値
    /// range: 最大変動幅（±range）
    /// 極端な値（上振れ・下振れ）は出にくい
    /// </summary>
    int DeviationRandom(int baseValue, int range)
    {
        return baseValue + GaussianRandomRange(-range, range);
    }

    void ShowBabySprite(Sprite sprite, bool isGodBaby)
    {
        if (babyFace == null)
        {
            Debug.LogError("[BirthSystem] babyFace is null!");
            return;
        }

        Debug.Log($"[BirthSystem] ShowBabySprite called, sprite: {sprite.name}");

        // babyFaceを大きくして中央上部に配置
        RepositionBabyFaceForDisplay();

        // introTextを非表示
        if (introText != null)
            introText.gameObject.SetActive(false);

        // 既存の子オブジェクトを削除（babyFaceImageとintroText以外）
        for (int i = babyFace.childCount - 1; i >= 0; i--)
        {
            Transform child = babyFace.GetChild(i);
            bool isPreserved = false;
            if (babyFaceImage != null && child.gameObject == babyFaceImage.gameObject) isPreserved = true;
            if (introText != null && child.gameObject == introText.gameObject) isPreserved = true;
            if (!isPreserved)
            {
                Destroy(child.gameObject);
            }
        }

        // 画像を表示するImageを取得または作成
        Image targetImage = babyFaceImage;

        // babyFaceImageが無効な場合は新しく作成
        if (targetImage == null || targetImage.gameObject == null)
        {
            Debug.Log("[BirthSystem] Creating new Image for baby sprite");
            // babyFaceの子として新しいImageオブジェクトを作成
            var imgObj = new GameObject("BabyImage");
            imgObj.transform.SetParent(babyFace, false);
            var imgRect = imgObj.AddComponent<RectTransform>();
            imgRect.anchorMin = Vector2.zero;
            imgRect.anchorMax = Vector2.one;
            imgRect.offsetMin = Vector2.zero;
            imgRect.offsetMax = Vector2.zero;
            targetImage = imgObj.AddComponent<Image>();
        }
        else
        {
            targetImage.gameObject.SetActive(true);
        }

        targetImage.enabled = true;
        targetImage.sprite = sprite;
        targetImage.color = Color.white;
        targetImage.preserveAspect = true;
        targetImage.raycastTarget = false;

        Debug.Log($"[BirthSystem] Baby sprite set on image, enabled: {targetImage.enabled}");

        // STAR BABYオーラを追加
        if (isGodBaby)
        {
            for (int i = 3; i >= 0; i--)
            {
                var aura = CreatePart("Aura" + i, babyFace, Vector2.zero, new Vector2(450 + i * 30, 470 + i * 30));
                var auraImg = aura.AddComponent<Image>();
                auraImg.color = new Color(1f, 0.85f, 0.2f, 0.1f - i * 0.02f);
                auraImg.raycastTarget = false;
                aura.transform.SetAsFirstSibling(); // 画像の後ろに配置
            }
        }

        babyFace.localScale = Vector3.one;
    }

    void RepositionBabyFaceForDisplay()
    {
        if (babyFace == null) return;

        // babyFaceはCanvas上に絶対配置（カード中央+10pxの位置）
        babyFace.anchorMin = new Vector2(0.5f, 0.5f);
        babyFace.anchorMax = new Vector2(0.5f, 0.5f);
        float cardOffsetY = 150f;
        babyFace.anchoredPosition = new Vector2(0, cardOffsetY + 10f);
        babyFace.sizeDelta = new Vector2(850, 850);
        babyFace.SetAsLastSibling();
    }

    void RepositionStatusTextForDisplay()
    {
        // ステータステキストの位置はInspectorで設定されたままにする（変更しない）
    }

    void GenerateBabyFace(int atk, int intelligence, int athletic, string gender, bool isGodBaby)
    {
        Debug.Log($"[BirthSystem] GenerateBabyFace called - babyFace: {babyFace != null}");
        if (babyFace == null)
        {
            Debug.LogError("[BirthSystem] babyFace is null, cannot generate face!");
            return;
        }

        // babyFaceを大きくして中央上部に配置
        RepositionBabyFaceForDisplay();

        // introTextを非表示
        if (introText != null)
            introText.gameObject.SetActive(false);

        // 既存の子オブジェクトを削除（babyFaceImageとintroText以外）
        for (int i = babyFace.childCount - 1; i >= 0; i--)
        {
            Transform child = babyFace.GetChild(i);
            bool isPreserved = false;
            if (babyFaceImage != null && child.gameObject == babyFaceImage.gameObject) isPreserved = true;
            if (introText != null && child.gameObject == introText.gameObject) isPreserved = true;
            if (!isPreserved)
            {
                Destroy(child.gameObject);
            }
        }

        if (babyFaceImage != null)
        {
            babyFaceImage.gameObject.SetActive(false);
            babyFaceImage.enabled = false;
        }

        float faceScale = 2.0f;
        babyFace.localScale = new Vector3(faceScale, faceScale, 1f);

        // 肌色のバリエーション
        Color[] skinTones = new Color[] {
            new Color(0.98f, 0.89f, 0.82f),
            new Color(0.95f, 0.83f, 0.74f),
            new Color(0.88f, 0.73f, 0.62f),
            new Color(0.78f, 0.61f, 0.48f),
        };
        Color baseSkin = skinTones[Random.Range(0, skinTones.Length)];
        Color shadowSkin = new Color(baseSkin.r * 0.85f, baseSkin.g * 0.82f, baseSkin.b * 0.8f);
        Color highlightSkin = new Color(
            Mathf.Min(1f, baseSkin.r * 1.1f),
            Mathf.Min(1f, baseSkin.g * 1.08f),
            Mathf.Min(1f, baseSkin.b * 1.05f)
        );

        // STAR BABYオーラ
        if (isGodBaby)
        {
            for (int i = 3; i >= 0; i--)
            {
                var aura = CreatePart("Aura" + i, babyFace, Vector2.zero, new Vector2(350 + i * 25, 370 + i * 25));
                aura.AddComponent<Image>().color = new Color(1f, 0.85f, 0.2f, 0.12f - i * 0.02f);
            }
        }

        // 顔の影（後ろ）
        var faceShadow = CreatePart("FaceShadow", babyFace, new Vector2(3, -5), new Vector2(125, 150));
        faceShadow.AddComponent<Image>().color = new Color(0, 0, 0, 0.15f);

        // 顔ベース
        var faceBase = CreatePart("FaceBase", babyFace, Vector2.zero, new Vector2(120, 145));
        faceBase.AddComponent<Image>().color = baseSkin;

        // 顔の左側シャドウ
        var leftShadow = CreatePart("LeftShadow", babyFace, new Vector2(-45, 0), new Vector2(35, 120));
        leftShadow.AddComponent<Image>().color = new Color(shadowSkin.r, shadowSkin.g, shadowSkin.b, 0.4f);

        // 顔の右側ハイライト
        var rightHighlight = CreatePart("RightHighlight", babyFace, new Vector2(30, 15), new Vector2(40, 80));
        rightHighlight.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.15f);

        // おでこハイライト
        var foreheadHL = CreatePart("ForeheadHL", babyFace, new Vector2(0, 45), new Vector2(70, 35));
        foreheadHL.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.2f);

        // 髪
        Color[] hairColors = new Color[] {
            new Color(0.08f, 0.06f, 0.05f),
            new Color(0.2f, 0.12f, 0.08f),
            new Color(0.35f, 0.22f, 0.12f),
            new Color(0.55f, 0.38f, 0.2f),
            new Color(0.8f, 0.65f, 0.35f),
        };
        Color hairBase = hairColors[Mathf.Clamp(intelligence / 22, 0, 4)];
        Color hairShadow = new Color(hairBase.r * 0.6f, hairBase.g * 0.6f, hairBase.b * 0.6f);
        Color hairHighlight = new Color(
            Mathf.Min(1f, hairBase.r * 1.4f),
            Mathf.Min(1f, hairBase.g * 1.4f),
            Mathf.Min(1f, hairBase.b * 1.3f)
        );

        Create3DHair(babyFace, hairBase, hairShadow, hairHighlight, athletic, gender == "女の子");

        // 目
        Color[] eyeColors = new Color[] {
            new Color(0.18f, 0.12f, 0.08f),
            new Color(0.35f, 0.5f, 0.65f),
            new Color(0.3f, 0.55f, 0.35f),
            new Color(0.45f, 0.35f, 0.25f),
        };
        Color eyeColor = eyeColors[Random.Range(0, eyeColors.Length)];
        float eyeSize = 14 + (intelligence / 12f);

        Create3DEye(babyFace, -22, 8, eyeSize, eyeColor, gender == "女の子", baseSkin);
        Create3DEye(babyFace, 22, 8, eyeSize, eyeColor, gender == "女の子", baseSkin);

        // 眉
        float browAngle = (atk - 50) * 0.25f;
        Create3DEyebrow(babyFace, -24, 32, browAngle, hairBase);
        Create3DEyebrow(babyFace, 24, 32, -browAngle, hairBase);

        // 鼻（3D風）
        Create3DNose(babyFace, baseSkin, shadowSkin);

        // 口
        Create3DMouth(babyFace, athletic, isGodBaby, baseSkin);

        // 耳
        Create3DEar(babyFace, -58, 5, baseSkin, shadowSkin, true);
        Create3DEar(babyFace, 58, 5, baseSkin, shadowSkin, false);

        // ほっぺ
        CreateRealisticCheek(babyFace, -32, -12, gender == "女の子");
        CreateRealisticCheek(babyFace, 32, -12, gender == "女の子");

        // あご影
        var chinShadow = CreatePart("ChinShadow", babyFace, new Vector2(0, -60), new Vector2(80, 20));
        chinShadow.AddComponent<Image>().color = new Color(shadowSkin.r, shadowSkin.g, shadowSkin.b, 0.25f);
    }

    GameObject CreatePart(string name, Transform parent, Vector2 pos, Vector2 size)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        var rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = pos;
        rect.sizeDelta = size;
        return obj;
    }

    void Create3DHair(Transform parent, Color baseC, Color shadowC, Color hlC, int athletic, bool isFemale)
    {
        // 髪の影
        var hairShadow = CreatePart("HairShadow", parent, new Vector2(2, 52), new Vector2(135, 65));
        hairShadow.AddComponent<Image>().color = new Color(0, 0, 0, 0.2f);

        // メイン髪
        var hairMain = CreatePart("HairMain", parent, new Vector2(0, 55), new Vector2(130, 60));
        hairMain.AddComponent<Image>().color = baseC;

        // 髪ハイライト
        var hairHL = CreatePart("HairHL", parent, new Vector2(15, 60), new Vector2(40, 25));
        hairHL.AddComponent<Image>().color = new Color(hlC.r, hlC.g, hlC.b, 0.5f);

        // トップ髪
        var topHair = CreatePart("TopHair", parent, new Vector2(0, 75), new Vector2(100, 30));
        topHair.AddComponent<Image>().color = baseC;

        // サイド髪（左）
        var leftHair = CreatePart("LeftHair", parent, new Vector2(-52, 15), new Vector2(28, 75));
        leftHair.AddComponent<Image>().color = shadowC;

        // サイド髪（右）
        var rightHair = CreatePart("RightHair", parent, new Vector2(52, 15), new Vector2(28, 75));
        rightHair.AddComponent<Image>().color = baseC;

        // 前髪
        if (athletic > 40)
        {
            int bangCount = 3 + athletic / 30;
            for (int i = 0; i < bangCount; i++)
            {
                float xPos = -35 + (70f / (bangCount - 1)) * i;
                var bang = CreatePart($"Bang{i}", parent, new Vector2(xPos, 38 + Random.Range(-3f, 3f)), new Vector2(18, 28 + Random.Range(0, 10)));
                bang.transform.localRotation = Quaternion.Euler(0, 0, Random.Range(-8f, 8f));
                bang.AddComponent<Image>().color = i % 2 == 0 ? baseC : shadowC;
            }
        }

        // 女の子はリボンやアクセサリー
        if (isFemale && Random.value > 0.3f)
        {
            var ribbon = CreatePart("Ribbon", parent, new Vector2(Random.Range(-25f, 25f), 70), new Vector2(20, 15));
            ribbon.AddComponent<Image>().color = new Color(1f, 0.4f, 0.5f);
            var ribbonCenter = CreatePart("RibbonCenter", ribbon.transform, Vector2.zero, new Vector2(8, 8));
            ribbonCenter.AddComponent<Image>().color = new Color(1f, 0.6f, 0.65f);
        }
    }

    void Create3DEye(Transform parent, float x, float y, float size, Color irisColor, bool isFemale, Color skinColor)
    {
        // まぶた影
        var lidShadow = CreatePart("LidShadow", parent, new Vector2(x, y + size * 0.4f), new Vector2(size * 1.8f, size * 0.5f));
        lidShadow.AddComponent<Image>().color = new Color(skinColor.r * 0.8f, skinColor.g * 0.75f, skinColor.b * 0.7f, 0.5f);

        // 白目（影付き）
        var eyeSocket = CreatePart("EyeSocket", parent, new Vector2(x, y), new Vector2(size * 1.6f, size * 1.1f));
        eyeSocket.AddComponent<Image>().color = new Color(0.95f, 0.95f, 0.97f);

        // 白目ハイライト
        var eyeHL = CreatePart("EyeHL", parent, new Vector2(x + 2, y + 2), new Vector2(size * 1.3f, size * 0.8f));
        eyeHL.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.7f);

        // 虹彩
        var iris = CreatePart("Iris", parent, new Vector2(x, y - 1), new Vector2(size * 0.9f, size * 0.9f));
        iris.AddComponent<Image>().color = irisColor;

        // 虹彩グラデ
        var irisInner = CreatePart("IrisInner", parent, new Vector2(x, y - 1), new Vector2(size * 0.6f, size * 0.6f));
        irisInner.AddComponent<Image>().color = new Color(irisColor.r * 0.6f, irisColor.g * 0.6f, irisColor.b * 0.6f);

        // 瞳孔
        var pupil = CreatePart("Pupil", parent, new Vector2(x, y - 1), new Vector2(size * 0.35f, size * 0.35f));
        pupil.AddComponent<Image>().color = new Color(0.02f, 0.02f, 0.02f);

        // メインハイライト
        var hl1 = CreatePart("HL1", parent, new Vector2(x - size * 0.15f, y + size * 0.15f), new Vector2(size * 0.25f, size * 0.25f));
        hl1.AddComponent<Image>().color = Color.white;

        // サブハイライト
        var hl2 = CreatePart("HL2", parent, new Vector2(x + size * 0.2f, y - size * 0.1f), new Vector2(size * 0.12f, size * 0.12f));
        hl2.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.7f);

        // まつげ
        if (isFemale)
        {
            for (int i = 0; i < 5; i++)
            {
                float lashX = x - size * 0.5f + (size / 4f) * i;
                var lash = CreatePart($"Lash{i}", parent, new Vector2(lashX, y + size * 0.55f), new Vector2(2, 6 + Random.Range(0, 4)));
                lash.transform.localRotation = Quaternion.Euler(0, 0, -20 + i * 10);
                lash.AddComponent<Image>().color = new Color(0.1f, 0.08f, 0.06f);
            }
        }

        // 下まつげライン
        var lowerLid = CreatePart("LowerLid", parent, new Vector2(x, y - size * 0.45f), new Vector2(size * 1.4f, 2));
        lowerLid.AddComponent<Image>().color = new Color(skinColor.r * 0.85f, skinColor.g * 0.8f, skinColor.b * 0.75f, 0.6f);
    }

    void Create3DEyebrow(Transform parent, float x, float y, float angle, Color hairColor)
    {
        Color browColor = new Color(hairColor.r * 0.7f, hairColor.g * 0.7f, hairColor.b * 0.7f);

        // 眉メイン
        var brow = CreatePart("Brow", parent, new Vector2(x, y), new Vector2(28, 5));
        brow.transform.localRotation = Quaternion.Euler(0, 0, angle);
        brow.AddComponent<Image>().color = browColor;

        // 眉の太い部分
        var browThick = CreatePart("BrowThick", parent, new Vector2(x + (x > 0 ? -5 : 5), y), new Vector2(15, 6));
        browThick.transform.localRotation = Quaternion.Euler(0, 0, angle);
        browThick.AddComponent<Image>().color = browColor;
    }

    void Create3DNose(Transform parent, Color skinColor, Color shadowColor)
    {
        float noseLen = 19f;

        // 鼻筋ハイライト
        var noseHL = CreatePart("NoseHL", parent, new Vector2(1, -2), new Vector2(6, noseLen));
        noseHL.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.2f);

        // 鼻筋影（左）
        var noseShadowL = CreatePart("NoseShadowL", parent, new Vector2(-4, -5), new Vector2(5, noseLen * 0.7f));
        noseShadowL.AddComponent<Image>().color = new Color(shadowColor.r, shadowColor.g, shadowColor.b, 0.25f);

        // 鼻先
        var noseTip = CreatePart("NoseTip", parent, new Vector2(0, -8 - noseLen * 0.3f), new Vector2(12, 10));
        noseTip.AddComponent<Image>().color = new Color(skinColor.r * 0.95f, skinColor.g * 0.92f, skinColor.b * 0.9f, 0.6f);

        // 鼻の穴（影で表現）
        var nostrilL = CreatePart("NostrilL", parent, new Vector2(-4, -12 - noseLen * 0.2f), new Vector2(4, 3));
        nostrilL.AddComponent<Image>().color = new Color(shadowColor.r * 0.7f, shadowColor.g * 0.7f, shadowColor.b * 0.7f, 0.4f);

        var nostrilR = CreatePart("NostrilR", parent, new Vector2(4, -12 - noseLen * 0.2f), new Vector2(4, 3));
        nostrilR.AddComponent<Image>().color = new Color(shadowColor.r * 0.7f, shadowColor.g * 0.7f, shadowColor.b * 0.7f, 0.4f);
    }

    void Create3DMouth(Transform parent, int athletic, bool isGodBaby, Color skinColor)
    {
        float smile = athletic / 100f;
        Color lipColor = isGodBaby
            ? new Color(0.85f, 0.35f, 0.4f)
            : new Color(0.82f, 0.55f, 0.55f);
        Color lipShadow = new Color(lipColor.r * 0.7f, lipColor.g * 0.6f, lipColor.b * 0.6f);
        Color lipHL = new Color(
            Mathf.Min(1f, lipColor.r * 1.2f),
            Mathf.Min(1f, lipColor.g * 1.15f),
            Mathf.Min(1f, lipColor.b * 1.1f), 0.6f
        );

        float mouthWidth = 22 + smile * 12;

        // 口の影
        var mouthShadow = CreatePart("MouthShadow", parent, new Vector2(0, -34), new Vector2(mouthWidth + 4, 10));
        mouthShadow.AddComponent<Image>().color = new Color(skinColor.r * 0.85f, skinColor.g * 0.8f, skinColor.b * 0.78f, 0.4f);

        // 上唇
        var upperLip = CreatePart("UpperLip", parent, new Vector2(0, -30), new Vector2(mouthWidth, 6));
        upperLip.AddComponent<Image>().color = lipColor;

        // 上唇ハイライト
        var upperLipHL = CreatePart("UpperLipHL", parent, new Vector2(0, -28), new Vector2(mouthWidth * 0.6f, 3));
        upperLipHL.AddComponent<Image>().color = lipHL;

        // 下唇
        var lowerLip = CreatePart("LowerLip", parent, new Vector2(0, -36), new Vector2(mouthWidth * 0.9f, 7));
        lowerLip.AddComponent<Image>().color = lipColor;

        // 下唇ハイライト
        var lowerLipHL = CreatePart("LowerLipHL", parent, new Vector2(2, -35), new Vector2(mouthWidth * 0.4f, 4));
        lowerLipHL.AddComponent<Image>().color = lipHL;

        // 口の線
        var mouthLine = CreatePart("MouthLine", parent, new Vector2(0, -33), new Vector2(mouthWidth * 0.85f, 2));
        mouthLine.AddComponent<Image>().color = lipShadow;

        // 笑顔の口角
        if (smile > 0.5f || isGodBaby)
        {
            var cornerL = CreatePart("CornerL", parent, new Vector2(-mouthWidth / 2 - 2, -31), new Vector2(5, 4));
            cornerL.transform.localRotation = Quaternion.Euler(0, 0, 25 + smile * 15);
            cornerL.AddComponent<Image>().color = lipColor;

            var cornerR = CreatePart("CornerR", parent, new Vector2(mouthWidth / 2 + 2, -31), new Vector2(5, 4));
            cornerR.transform.localRotation = Quaternion.Euler(0, 0, -25 - smile * 15);
            cornerR.AddComponent<Image>().color = lipColor;
        }
    }

    void Create3DEar(Transform parent, float x, float y, Color skinColor, Color shadowColor, bool isLeft)
    {
        // 耳の影
        var earShadow = CreatePart("EarShadow", parent, new Vector2(x + (isLeft ? -2 : 2), y - 2), new Vector2(18, 28));
        earShadow.AddComponent<Image>().color = new Color(0, 0, 0, 0.15f);

        // 耳本体
        var ear = CreatePart("Ear", parent, new Vector2(x, y), new Vector2(16, 26));
        ear.AddComponent<Image>().color = skinColor;

        // 耳の内側
        var earInner = CreatePart("EarInner", parent, new Vector2(x + (isLeft ? 2 : -2), y), new Vector2(10, 18));
        earInner.AddComponent<Image>().color = new Color(shadowColor.r, shadowColor.g, shadowColor.b, 0.35f);

        // 耳たぶハイライト
        var earLobe = CreatePart("EarLobe", parent, new Vector2(x, y - 8), new Vector2(10, 10));
        earLobe.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.15f);
    }

    void CreateRealisticCheek(Transform parent, float x, float y, bool isFemale)
    {
        float alpha = isFemale ? 0.35f : 0.2f;
        Color cheekColor = isFemale
            ? new Color(1f, 0.5f, 0.55f, alpha)
            : new Color(1f, 0.7f, 0.7f, alpha);

        // メインほっぺ
        var cheek = CreatePart("Cheek", parent, new Vector2(x, y), new Vector2(28, 22));
        cheek.AddComponent<Image>().color = cheekColor;

        // ほっぺハイライト
        var cheekHL = CreatePart("CheekHL", parent, new Vector2(x + 3, y + 3), new Vector2(15, 12));
        cheekHL.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.15f);
    }

    // ===== UI自動生成 =====

    void CreateParentUI()
    {
        if (overlayRoot == null) return;

        parentPanel = new UIE.VisualElement();
        parentPanel.AddToClassList("birth-parent-panel");
        parentPanel.pickingMode = UIE.PickingMode.Ignore;
        overlayRoot.Add(parentPanel);

        // 両カードとも中央に配置（一人ずつ表示するため）
        CreateParentCard(parentPanel, out fatherCard, out fatherFaceImage, out fatherNameText, out fatherIntroText, "father");
        CreateParentCard(parentPanel, out motherCard, out motherFaceImage, out motherNameText, out motherIntroText, "mother");

        // ── 「次へ」ボタン（シャンパンゴールド + 魔法陣） ──
        var nextWrapper = new UIE.VisualElement();
        nextWrapper.AddToClassList("birth-next-btn-wrapper");

        var shadow = new UIE.VisualElement();
        shadow.AddToClassList("shadow-layer");
        nextWrapper.Add(shadow);

        // 魔法陣（ボタンの背後にゆっくり回転するリング）
        magicCircleEl = new UIE.VisualElement();
        magicCircleEl.AddToClassList("birth-magic-circle");
        magicCircleEl.pickingMode = UIE.PickingMode.Ignore;
        var ringOuter = new UIE.VisualElement();
        ringOuter.AddToClassList("birth-magic-ring-outer");
        ringOuter.pickingMode = UIE.PickingMode.Ignore;
        magicCircleEl.Add(ringOuter);
        var ringInner = new UIE.VisualElement();
        ringInner.AddToClassList("birth-magic-ring-inner");
        ringInner.pickingMode = UIE.PickingMode.Ignore;
        magicCircleEl.Add(ringInner);
        // 8シンボルを45度間隔で配置
        string[] magicSymbols = { "\u2726", "\u2606", "\u2727", "\u2661", "\u25c7", "\u2726", "\u2606", "\u2727" };
        for (int i = 0; i < 8; i++)
        {
            var sym = UIHelper.CreateLabel(magicSymbols[i]);
            sym.AddToClassList("birth-magic-symbol");
            sym.pickingMode = UIE.PickingMode.Ignore;
            float angle = i * 45f * Mathf.Deg2Rad;
            float radius = 46f; // パーセント (50% - 4%)
            float cx = 50f + radius * Mathf.Cos(angle);
            float cy = 50f - radius * Mathf.Sin(angle);
            sym.style.left = new UIE.StyleLength(new UIE.Length(cx, UIE.LengthUnit.Percent));
            sym.style.top = new UIE.StyleLength(new UIE.Length(cy, UIE.LengthUnit.Percent));
            sym.style.translate = new UIE.StyleTranslate(
                new UIE.Translate(new UIE.Length(-50, UIE.LengthUnit.Percent), new UIE.Length(-50, UIE.LengthUnit.Percent)));
            magicCircleEl.Add(sym);
        }
        nextWrapper.Add(magicCircleEl);

        var nextBtn = new UIE.Button();
        nextBtn.AddToClassList("birth-next-btn");
        UIHelper.ApplyFont(nextBtn);
        nextBtn.text = Localization.Get("birth_next");
        nextBtn.clicked += OnNextPressed;
        nextWrapper.Add(nextBtn);

        nextButtonEl = nextWrapper;
        nextButtonEl.style.display = UIE.DisplayStyle.None;
        parentPanel.Add(nextButtonEl);
    }

    void OnNextPressed()
    {
        StartCoroutine(WhiteoutThenProceed());
    }

    // ── 魔法陣回転アニメーション ──
    IEnumerator AnimateMagicCircle()
    {
        float angle = 0f;
        while (magicCircleEl != null && magicCircleEl.parent != null)
        {
            angle += Time.deltaTime * 8f;
            magicCircleEl.style.rotate = new UIE.StyleRotate(
                new UIE.Rotate(new UIE.Angle(angle, UIE.AngleUnit.Degree)));
            yield return null;
        }
    }

    void ShowNextButton()
    {
        if (nextButtonEl != null)
        {
            nextButtonEl.style.display = UIE.DisplayStyle.Flex;
            if (magicCircleRotateCoroutine != null) StopCoroutine(magicCircleRotateCoroutine);
            magicCircleRotateCoroutine = StartCoroutine(AnimateMagicCircle());
        }
    }

    void HideNextButton()
    {
        if (nextButtonEl != null)
        {
            nextButtonEl.style.display = UIE.DisplayStyle.None;
            if (magicCircleRotateCoroutine != null)
            {
                StopCoroutine(magicCircleRotateCoroutine);
                magicCircleRotateCoroutine = null;
            }
        }
    }

    // ── ホワイトアウト遷移 ──
    IEnumerator WhiteoutThenProceed()
    {
        // 二重タップ防止
        var btn = UIE.UQueryExtensions.Q<UIE.Button>(nextButtonEl);
        if (btn != null) btn.SetEnabled(false);

        // ゴールドリップル（ボタン中心から放射状に拡がる金の波紋）
        if (nextButtonEl != null)
            StartCoroutine(SpawnGoldRipple(nextButtonEl));

        // フラッシュイン（warm white）
        if (flashOverlay != null)
        {
            flashOverlay.gameObject.SetActive(true);
            float fadeInDur = 0.3f;
            float elapsed = 0f;
            while (elapsed < fadeInDur)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / fadeInDur);
                float easeT = t * t;
                flashOverlay.color = new Color(1f, 0.98f, 0.95f, 0.85f * easeT);
                yield return null;
            }
        }

        yield return new WaitForSeconds(0.15f);

        // メインコルーチンに進行許可
        waitingForNext = false;

        // フラッシュアウト
        if (flashOverlay != null)
        {
            float fadeOutDur = 0.5f;
            float elapsed2 = 0f;
            while (elapsed2 < fadeOutDur)
            {
                elapsed2 += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed2 / fadeOutDur);
                flashOverlay.color = new Color(1f, 0.98f, 0.95f, 0.85f * (1f - t));
                yield return null;
            }
            flashOverlay.gameObject.SetActive(false);
        }

        if (btn != null) btn.SetEnabled(true);
    }

    // ── ゴールドリップル（宿命を定めるボタン押下時） ──
    IEnumerator SpawnGoldRipple(UIE.VisualElement parent)
    {
        // 2本の波紋を少しずらして発射
        for (int r = 0; r < 2; r++)
        {
            var ripple = new UIE.VisualElement();
            ripple.style.position = UIE.Position.Absolute;
            ripple.style.left = new UIE.StyleLength(new UIE.Length(50, UIE.LengthUnit.Percent));
            ripple.style.top = new UIE.StyleLength(new UIE.Length(50, UIE.LengthUnit.Percent));
            float startSize = 30f;
            ripple.style.width = startSize;
            ripple.style.height = startSize;
            ripple.style.translate = new UIE.StyleTranslate(
                new UIE.Translate(-startSize / 2, -startSize / 2));
            SetAllRadius(ripple, startSize / 2);
            ripple.style.backgroundColor = new Color(0, 0, 0, 0);
            float bw = r == 0 ? 3f : 2f;
            ripple.style.borderTopWidth = bw;
            ripple.style.borderBottomWidth = bw;
            ripple.style.borderLeftWidth = bw;
            ripple.style.borderRightWidth = bw;
            Color goldColor = new Color(0.776f, 0.627f, 0.314f, 0.7f); // rgb(198,160,80)
            ripple.style.borderTopColor = goldColor;
            ripple.style.borderBottomColor = goldColor;
            ripple.style.borderLeftColor = goldColor;
            ripple.style.borderRightColor = goldColor;
            parent.Add(ripple);

            StartCoroutine(AnimateGoldRipple(ripple, r == 0 ? 800f : 1200f, r == 0 ? 0.8f : 1.0f));
            yield return new WaitForSeconds(0.1f);
        }
    }

    IEnumerator AnimateGoldRipple(UIE.VisualElement ripple, float maxSize, float duration)
    {
        float startSize = 30f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float easeT = 1f - (1f - t) * (1f - t); // ease-out
            float s = Mathf.Lerp(startSize, maxSize, easeT);
            ripple.style.width = s;
            ripple.style.height = s;
            ripple.style.translate = new UIE.StyleTranslate(
                new UIE.Translate(-s / 2, -s / 2));
            SetAllRadius(ripple, s / 2);

            float alpha = 0.7f * (1f - t);
            Color c = new Color(0.776f, 0.627f, 0.314f, alpha);
            ripple.style.borderTopColor = c;
            ripple.style.borderBottomColor = c;
            ripple.style.borderLeftColor = c;
            ripple.style.borderRightColor = c;
            yield return null;
        }
        if (ripple.parent != null)
            ripple.RemoveFromHierarchy();
    }

    // ===== 親ステータスアコーディオン =====

    string GetStatusCheckButtonText(UIE.VisualElement targetCard)
    {
        if (targetCard != null && targetCard.ClassListContains("birth-parent-card-father"))
            return "どんなパパなの？ ✨";
        if (targetCard != null && targetCard.ClassListContains("birth-parent-card-mother"))
            return "どんなママなの？ ✨";
        return "どんな親なの？ ✨";
    }

    void CreateStatusCheckButton(ParentData parent, UIE.VisualElement targetCard, bool isFirstMeet = false)
    {
        if (targetCard == null) return;
        DestroyStatusCheckButton();

        statusCheckButton = new UIE.Button();
        statusCheckButton.AddToClassList("birth-status-check-btn");
        UIHelper.ApplyFontBold(statusCheckButton);
        // 親の role に応じてボタンテキストを動的に設定
        statusCheckButton.text = GetStatusCheckButtonText(targetCard);

        currentAccordionParent = parent;
        currentAccordionTargetCard = targetCard;
        statusCheckButton.clicked += () =>
        {
            // パルスを止めてからアコーディオン（ストーリーポップアップ）を開く
            StopStatusCheckPulse();
            ToggleAccordion(currentAccordionParent, currentAccordionTargetCard);
        };
        targetCard.Add(statusCheckButton);

        // 初回遭遇キャラならパルスアニメーションで注意を引く
        if (isFirstMeet && !HasMetParent(parent.name))
        {
            statusCheckPulseCoroutine = StartCoroutine(PulseStatusCheckButton(statusCheckButton));
        }
    }

    bool HasMetParent(string parentName)
    {
        return PlayerPrefs.GetInt("met_parent_" + parentName, 0) == 1;
    }

    void MarkParentAsMet(string parentName)
    {
        PlayerPrefs.SetInt("met_parent_" + parentName, 1);
    }

    void StopStatusCheckPulse()
    {
        if (statusCheckPulseCoroutine != null)
        {
            StopCoroutine(statusCheckPulseCoroutine);
            statusCheckPulseCoroutine = null;
        }
        // スケールをリセット
        if (statusCheckButton != null)
            statusCheckButton.style.scale = new UIE.StyleScale(new UIE.Scale(Vector2.one));
    }

    IEnumerator PulseStatusCheckButton(UIE.Button btn)
    {
        float elapsed = 0f;
        while (btn != null && btn.parent != null)
        {
            elapsed += Time.deltaTime;
            // ゆっくりしたパルス (周期1.2秒)
            float t = Mathf.Sin(elapsed * Mathf.PI * 2f / 1.2f);
            float s = 1f + 0.04f * t;
            btn.style.scale = new UIE.StyleScale(new UIE.Scale(new Vector2(s, s)));
            yield return null;
        }
    }

    void DestroyStatusCheckButton()
    {
        // パルスアニメーション停止
        StopStatusCheckPulse();

        // ストーリーポップアップが開いていたら閉じる
        CloseStoryPopup();

        // フリップ状態のクリーンアップ
        if (flipAnimCoroutine != null)
        {
            StopCoroutine(flipAnimCoroutine);
            flipAnimCoroutine = null;
        }
        if (cardBackFace != null)
        {
            cardBackFace.RemoveFromHierarchy();
            cardBackFace = null;
        }
        if (isCardFlipped && currentAccordionTargetCard != null)
        {
            // scaleリセット＋front子要素復元
            currentAccordionTargetCard.style.scale = new UIE.StyleScale(new UIE.Scale(Vector2.one));
            SetFrontChildrenVisibility(currentAccordionTargetCard, true);
        }
        isCardFlipped = false;

        // アコーディオンのクリーンアップ
        if (accordionAnimCoroutine != null)
        {
            StopCoroutine(accordionAnimCoroutine);
            accordionAnimCoroutine = null;
        }
        if (accordionContainer != null)
        {
            accordionContainer.RemoveFromHierarchy();
            accordionContainer = null;
        }
        isAccordionOpen = false;
        if (statusCheckButton != null)
        {
            statusCheckButton.RemoveFromHierarchy();
            statusCheckButton = null;
        }
    }

    void ToggleAccordion(ParentData parent, UIE.VisualElement targetCard)
    {
        // フリップ中なら先に戻す
        if (isCardFlipped)
        {
            FlipCardToFront(targetCard);
            return;
        }

        if (isStoryPopupOpen)
        {
            CloseStoryPopup();
        }
        else
        {
            // SE再生
            if (seSource != null && seKirakira4 != null)
                seSource.PlayOneShot(seKirakira4, 0.8f);
            ShowStoryPopup(parent, targetCard);
        }
    }

    // ===== 絵本風ストーリーポップアップ =====

    void ShowStoryPopup(ParentData parent, UIE.VisualElement targetCard)
    {
        if (overlayRoot == null) return;

        isStoryPopupOpen = true;
        // 初回閲覧を記録（次回以降パルスしない）
        MarkParentAsMet(parent.name);
        if (statusCheckButton != null)
            statusCheckButton.text = "\u25b2 \u3068\u3058\u308b";

        // オーバーレイ
        storyPopupOverlay = new UIE.VisualElement();
        storyPopupOverlay.AddToClassList("birth-story-popup-overlay");
        storyPopupOverlay.RegisterCallback<UIE.ClickEvent>(evt =>
        {
            if (evt.target == storyPopupOverlay)
                CloseStoryPopup();
        });

        // 羊皮紙カード
        var card = new UIE.VisualElement();
        card.AddToClassList("birth-story-popup-card");

        // テクスチャレイヤー
        var texture = new UIE.VisualElement();
        texture.AddToClassList("birth-story-popup-texture");
        texture.pickingMode = UIE.PickingMode.Ignore;
        card.Add(texture);

        // 内側装飾ボーダー
        var innerBorder = new UIE.VisualElement();
        innerBorder.AddToClassList("birth-story-popup-inner-border");
        innerBorder.pickingMode = UIE.PickingMode.Ignore;
        card.Add(innerBorder);

        // キャッチコピー
        string intro = Localization.GetParentIntro(parent.name);
        if (string.IsNullOrEmpty(intro)) intro = parent.intro;
        intro = intro.Replace(" / ", "\n");
        var catchphrase = UIHelper.CreateLabel(intro, "birth-story-popup-catchphrase");
        UIHelper.ApplyFontBold(catchphrase);
        card.Add(catchphrase);

        // 区切り線
        var sep = new UIE.VisualElement();
        sep.AddToClassList("birth-story-popup-separator");
        card.Add(sep);

        // Bio本文（タイプライターで表示）
        string bio = Localization.GetParentBio(parent.name);
        var bioLabel = UIHelper.CreateLabel("", "birth-story-popup-bio");
        UIHelper.ApplyFont(bioLabel);
        card.Add(bioLabel);

        // ステータス確認ボタン
        var statusBtn = new UIE.Button();
        statusBtn.AddToClassList("birth-story-popup-status-btn");
        UIHelper.ApplyFontBold(statusBtn);
        statusBtn.text = "\u3068\u304f\u3061\u3087\u3046\uff08\u30b9\u30c6\u30fc\u30bf\u30b9\uff09\u3092\u78ba\u8a8d \ud83d\udcca";
        ParentData capturedParent = parent;
        statusBtn.clicked += () =>
        {
            CloseStoryPopup();
            FlipCardToStatus(capturedParent, currentAccordionTargetCard);
        };
        card.Add(statusBtn);

        // とじるボタン
        var closeBtn = new UIE.Button();
        closeBtn.AddToClassList("birth-story-popup-close-btn");
        UIHelper.ApplyFontBold(closeBtn);
        closeBtn.text = "\u3068\u3058\u308b";
        closeBtn.clicked += () => CloseStoryPopup();
        card.Add(closeBtn);

        storyPopupOverlay.Add(card);
        overlayRoot.Add(storyPopupOverlay);

        // タイプライター開始
        if (!string.IsNullOrEmpty(bio))
        {
            storyPopupTypewriterCoroutine = StartCoroutine(TypewriterPopupBio(bioLabel, bio));
        }
    }

    IEnumerator TypewriterPopupBio(UIE.Label label, string fullText)
    {
        label.text = "";
        foreach (char c in fullText)
        {
            if (!isStoryPopupOpen) break;
            label.text += c;
            yield return new WaitForSeconds(0.04f);
        }
        label.text = fullText;
    }

    void CloseStoryPopup()
    {
        if (storyPopupTypewriterCoroutine != null)
        {
            StopCoroutine(storyPopupTypewriterCoroutine);
            storyPopupTypewriterCoroutine = null;
        }
        isStoryPopupOpen = false;

        if (storyPopupOverlay != null && storyPopupOverlay.parent != null)
            storyPopupOverlay.RemoveFromHierarchy();
        storyPopupOverlay = null;

        // ボタンテキスト復元
        if (statusCheckButton != null && currentAccordionTargetCard != null)
            statusCheckButton.text = GetStatusCheckButtonText(currentAccordionTargetCard);
    }

    UIE.VisualElement BuildAccordionContent(ParentData parent, string role)
    {
        var container = new UIE.VisualElement();
        container.AddToClassList("birth-accordion");

        var inner = new UIE.VisualElement();
        inner.AddToClassList("birth-accordion-inner");
        container.Add(inner);

        // キャッチコピー（大きく表示）
        string intro = Localization.GetParentIntro(parent.name);
        if (string.IsNullOrEmpty(intro)) intro = parent.intro;
        intro = intro.Replace(" / ", "\n");
        var catchphrase = UIHelper.CreateLabel(intro, "birth-accordion-catchphrase");
        UIHelper.ApplyFontBold(catchphrase);
        inner.Add(catchphrase);

        // 区切り線
        var sep = new UIE.VisualElement();
        sep.AddToClassList("birth-accordion-separator");
        inner.Add(sep);

        // Bio本文
        string bio = Localization.GetParentBio(parent.name);
        if (!string.IsNullOrEmpty(bio))
        {
            var bioLabel = UIHelper.CreateLabel(bio, "birth-accordion-bio");
            UIHelper.ApplyFont(bioLabel);
            inner.Add(bioLabel);
        }

        // ステータス確認ボタン
        var statusBtn = new UIE.Button();
        statusBtn.AddToClassList("birth-bio-view-btn");
        UIHelper.ApplyFontBold(statusBtn);
        statusBtn.text = "\u3068\u304f\u3061\u3087\u3046\uff08\u30b9\u30c6\u30fc\u30bf\u30b9\uff09\u3092\u78ba\u8a8d \ud83d\udcca";
        ParentData capturedParent = parent;
        statusBtn.clicked += () => FlipCardToStatus(capturedParent, currentAccordionTargetCard);
        inner.Add(statusBtn);

        return container;
    }

    // ===== キャラクター背景パーティクル =====

    void GetCharacterParticleConfig(ParentData parent, out string[] symbols, out Color[] colors)
    {
        if (parent.fortune >= 80)
        {
            symbols = new[] { "\u2726", "\u2727", "\u2b50", "\u2728", "\ud83d\udcb0" };
            colors = new[] {
                new Color(1f, 0.84f, 0f),
                new Color(1f, 0.9f, 0.5f),
                new Color(0.8f, 0.65f, 0.2f),
                Color.white
            };
            return;
        }
        if (parent.intelligence >= 80)
        {
            symbols = new[] { "\u2726", "\u2727", "\u2728", "\ud83d\udca1", "\u2b50" };
            colors = new[] {
                new Color(0.6f, 0.8f, 1f),
                new Color(0.67f, 0.94f, 0.82f),
                new Color(0.8f, 0.9f, 1f),
                Color.white
            };
            return;
        }
        if (parent.atk >= 70)
        {
            symbols = new[] { "\u2726", "\ud83d\udd25", "\u2728", "\u26a1", "\u2b50" };
            colors = new[] {
                new Color(1f, 0.5f, 0.4f),
                new Color(1f, 0.7f, 0.3f),
                new Color(1f, 0.85f, 0.5f),
                Color.white
            };
            return;
        }
        symbols = new[] { "\u2726", "\u2727", "\u2b50", "\u2728", "\u2661" };
        colors = new[] {
            new Color(1f, 0.72f, 0.77f),
            new Color(0.97f, 0.91f, 0.81f),
            new Color(0.67f, 0.94f, 0.82f),
            Color.white
        };
    }

    IEnumerator SpawnCharacterParticles(UIE.VisualElement faceContainer, ParentData parent)
    {
        GetCharacterParticleConfig(parent, out string[] symbols, out Color[] colors);

        while (faceContainer != null && faceContainer.parent != null)
        {
            var particle = UIHelper.CreateLabel(symbols[Random.Range(0, symbols.Length)]);
            UIHelper.ApplyFont(particle);
            particle.AddToClassList("birth-char-particle");
            particle.pickingMode = UIE.PickingMode.Ignore;
            particle.style.left = new UIE.StyleLength(
                new UIE.Length(Random.Range(5f, 95f), UIE.LengthUnit.Percent));
            particle.style.top = new UIE.StyleLength(
                new UIE.Length(Random.Range(60f, 95f), UIE.LengthUnit.Percent));
            particle.style.fontSize = Random.Range(16, 32);
            particle.style.color = colors[Random.Range(0, colors.Length)];
            particle.style.opacity = 0.8f;
            faceContainer.Add(particle);
            StartCoroutine(AnimateCharacterParticle(particle));
            yield return new WaitForSeconds(Random.Range(0.3f, 0.6f));
        }
    }

    IEnumerator AnimateCharacterParticle(UIE.Label particle)
    {
        float duration = Random.Range(1.2f, 2.0f);
        float elapsed = 0f;
        float startTop = particle.resolvedStyle.top;
        float startLeft = particle.resolvedStyle.left;
        float riseSpeed = Random.Range(40f, 100f);
        float driftX = Random.Range(-20f, 20f);

        while (elapsed < duration && particle != null && particle.parent != null)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            float newTop = startTop - riseSpeed * t;
            particle.style.top = newTop;

            // パーティクル独自のパララックスドリフト（顔より大きく動く）
            float extraX = smoothAccel.x * PARALLAX_PARTICLE_AMOUNT + driftX * t;
            particle.style.left = startLeft + extraX;

            particle.style.opacity = 0.8f * (1f - t * t);

            float s = 1f + 0.2f * Mathf.Sin(t * Mathf.PI);
            particle.style.scale = new UIE.StyleScale(new UIE.Scale(new Vector2(s, s)));

            yield return null;
        }

        if (particle != null && particle.parent != null)
            particle.RemoveFromHierarchy();
    }

    void StopCharacterParticles()
    {
        if (fatherParticleCoroutine != null)
        {
            StopCoroutine(fatherParticleCoroutine);
            fatherParticleCoroutine = null;
        }
        if (motherParticleCoroutine != null)
        {
            StopCoroutine(motherParticleCoroutine);
            motherParticleCoroutine = null;
        }
    }

    // ── 紹介文フェードアップ演出（0.5秒遅延 → ふわっと上昇） ──
    IEnumerator FadeUpIntroText(UIE.Label introLabel)
    {
        if (introLabel == null) yield break;

        // 初期状態: 透明 + 少し下にオフセット
        introLabel.style.visibility = UIE.Visibility.Visible;
        introLabel.style.opacity = 0f;
        introLabel.style.translate = new UIE.StyleTranslate(new UIE.Translate(0, 20));

        // 0.5秒の遅延
        yield return new WaitForSeconds(0.5f);

        // 0.4秒かけてフェードアップ
        float dur = 0.4f;
        float elapsed = 0f;
        while (elapsed < dur)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / dur);
            // ease-out
            float easeT = 1f - (1f - t) * (1f - t);
            introLabel.style.opacity = easeT;
            float y = Mathf.Lerp(20f, 0f, easeT);
            introLabel.style.translate = new UIE.StyleTranslate(new UIE.Translate(0, y));
            yield return null;
        }
        introLabel.style.opacity = 1f;
        introLabel.style.translate = new UIE.StyleTranslate(new UIE.Translate(0, 0));
    }

    void AddStatRow(UIE.VisualElement parent, string label, int value, int maxValue, string fillClass)
    {
        var row = new UIE.VisualElement();
        row.AddToClassList("birth-stat-row");

        var lbl = UIHelper.CreateLabel(label, "birth-stat-label");
        UIHelper.ApplyFont(lbl);
        row.Add(lbl);

        var barBg = new UIE.VisualElement();
        barBg.AddToClassList("birth-stat-bar-bg");

        var barFill = new UIE.VisualElement();
        barFill.AddToClassList("birth-stat-bar-fill");
        barFill.AddToClassList(fillClass);
        barFill.style.width = new UIE.StyleLength(new UIE.Length(0, UIE.LengthUnit.Percent));
        barBg.Add(barFill);
        row.Add(barBg);

        var val = UIHelper.CreateLabel(value.ToString(), "birth-stat-value");
        UIHelper.ApplyFont(val);
        row.Add(val);

        parent.Add(row);
    }

    IEnumerator AnimateAccordionOpen(UIE.VisualElement accordion)
    {
        accordion.style.opacity = 0f;
        accordion.style.maxHeight = 0;
        accordion.style.overflow = UIE.Overflow.Hidden;

        yield return null; // 1フレーム待ってレイアウト確定

        float targetHeight = 800f;
        float duration = 0.35f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
            accordion.style.maxHeight = targetHeight * t;
            accordion.style.opacity = t;
            yield return null;
        }

        accordion.style.maxHeight = UIE.StyleKeyword.None;
        accordion.style.overflow = UIE.Overflow.Visible;

        // スパークルパーティクル
        StartCoroutine(SpawnAccordionSparkles(accordion));
    }

    IEnumerator AnimateAccordionClose(UIE.VisualElement accordion, System.Action onComplete)
    {
        float startHeight = accordion.resolvedStyle.height;
        if (startHeight <= 0) startHeight = 800f;
        accordion.style.overflow = UIE.Overflow.Hidden;

        float duration = 0.25f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            accordion.style.maxHeight = Mathf.Lerp(startHeight, 0, t);
            accordion.style.opacity = 1f - t;
            yield return null;
        }

        accordion.RemoveFromHierarchy();
        onComplete?.Invoke();
    }

    IEnumerator AnimateStatBars(UIE.VisualElement accordion)
    {
        if (accordion == null) yield break;

        var rows = UIE.UQueryExtensions.Query(accordion, className: "birth-stat-bar-fill").ToList();
        for (int i = 0; i < rows.Count; i++)
        {
            var fill = rows[i];
            // statRowからvalue情報を取得: 親のstatContainerのAddStatRow順に対応
            var row = fill.parent?.parent; // fill → barBg → row
            if (row == null) continue;
            var valLabel = UIE.UQueryExtensions.Q(row, className: "birth-stat-value") as UIE.Label;
            int value = 0;
            int maxValue = 100;
            if (valLabel != null) int.TryParse(valLabel.text, out value);

            // 対応するmaxValueを推定（行インデックスから）
            switch (i)
            {
                case 2: maxValue = 200; break; // ごきげん
                case 3: maxValue = 150; break; // ちえ
                default: maxValue = 100; break;
            }

            float targetPercent = Mathf.Clamp01((float)value / maxValue) * 100f;

            // スタガー付きアニメーション
            yield return new WaitForSeconds(0.06f);
            float barDuration = 0.4f;
            float barElapsed = 0f;
            while (barElapsed < barDuration)
            {
                if (fill.parent == null) yield break;
                barElapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(barElapsed / barDuration));
                fill.style.width = new UIE.StyleLength(new UIE.Length(targetPercent * t, UIE.LengthUnit.Percent));
                yield return null;
            }
            fill.style.width = new UIE.StyleLength(new UIE.Length(targetPercent, UIE.LengthUnit.Percent));
        }
    }

    IEnumerator SpawnAccordionSparkles(UIE.VisualElement container)
    {
        if (container == null) yield break;

        string[] sparkleChars = { "\u2726", "\u2727", "\u2b50", "*", "\u2728" };
        Color[] sparkleColors = {
            new Color(1f, 0.84f, 0f),
            new Color(1f, 0.72f, 0.77f),
            new Color(0.67f, 0.94f, 0.82f),
            Color.white
        };

        for (int i = 0; i < 10; i++)
        {
            if (container.parent == null) yield break;
            var sparkle = UIHelper.CreateLabel(sparkleChars[Random.Range(0, sparkleChars.Length)]);
            UIHelper.ApplyFont(sparkle);
            sparkle.style.position = UIE.Position.Absolute;
            sparkle.style.left = new UIE.StyleLength(
                new UIE.Length(Random.Range(5f, 95f), UIE.LengthUnit.Percent));
            sparkle.style.top = new UIE.StyleLength(
                new UIE.Length(Random.Range(-10f, 110f), UIE.LengthUnit.Percent));
            sparkle.style.fontSize = Random.Range(16, 28);
            sparkle.style.color = sparkleColors[Random.Range(0, sparkleColors.Length)];
            sparkle.style.opacity = 1f;
            container.Add(sparkle);
            StartCoroutine(FadeOutSparkle(sparkle));
            yield return new WaitForSeconds(0.05f);
        }
    }

    // ===== カードフリップ（Bio表示） =====

    void FlipCardToStatus(ParentData parent, UIE.VisualElement card)
    {
        if (isCardFlipped || card == null) return;
        if (flipAnimCoroutine != null)
        {
            StopCoroutine(flipAnimCoroutine);
            flipAnimCoroutine = null;
        }

        string role = card.ClassListContains("birth-parent-card-father") ? "father" : "mother";
        cardBackFace = BuildCardBackFace(parent, role);
        cardBackFace.style.display = UIE.DisplayStyle.None;
        card.Add(cardBackFace);

        PlayCardFlipSE();
        isCardFlipped = true;
        flipAnimCoroutine = StartCoroutine(AnimateCardFlip(card, true));
    }

    void FlipCardToFront(UIE.VisualElement card)
    {
        if (!isCardFlipped || card == null) return;
        if (flipAnimCoroutine != null)
        {
            StopCoroutine(flipAnimCoroutine);
            flipAnimCoroutine = null;
        }

        PlayCardFlipSE();
        flipAnimCoroutine = StartCoroutine(AnimateCardFlip(card, false));
    }

    IEnumerator AnimateCardFlip(UIE.VisualElement card, bool toBack)
    {
        // Phase 1: scaleX 1 → 0 (ease-in)
        float duration = 0.2f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float scaleX = Mathf.Lerp(1f, 0f, t * t); // ease-in (quadratic)
            card.style.scale = new UIE.StyleScale(new UIE.Scale(new Vector2(scaleX, 1f)));
            yield return null;
        }
        card.style.scale = new UIE.StyleScale(new UIE.Scale(new Vector2(0f, 1f)));

        // 中間切替
        if (toBack)
        {
            // front子要素を非表示
            SetFrontChildrenVisibility(card, false);
            if (cardBackFace != null)
                cardBackFace.style.display = UIE.DisplayStyle.Flex;
        }
        else
        {
            // back非表示、front復元
            if (cardBackFace != null)
                cardBackFace.style.display = UIE.DisplayStyle.None;
            SetFrontChildrenVisibility(card, true);
        }

        // Phase 2: scaleX 0 → 1 (ease-out)
        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float easeOut = 1f - (1f - t) * (1f - t); // ease-out (quadratic)
            float scaleX = Mathf.Lerp(0f, 1f, easeOut);
            card.style.scale = new UIE.StyleScale(new UIE.Scale(new Vector2(scaleX, 1f)));
            yield return null;
        }
        card.style.scale = new UIE.StyleScale(new UIE.Scale(Vector2.one));

        if (toBack)
        {
            // ステータス面に切り替え後、バーフィルアニメーション
            if (cardBackFace != null)
                StartCoroutine(AnimateStatBars(cardBackFace));
        }
        else
        {
            // 戻りの場合、裏面を破棄
            if (cardBackFace != null)
            {
                cardBackFace.RemoveFromHierarchy();
                cardBackFace = null;
            }
            isCardFlipped = false;
        }
    }

    void SetFrontChildrenVisibility(UIE.VisualElement card, bool visible)
    {
        var vis = visible ? UIE.Visibility.Visible : UIE.Visibility.Hidden;
        for (int i = 0; i < card.childCount; i++)
        {
            var child = card[i];
            if (child == cardBackFace) continue; // 裏面はスキップ
            child.style.visibility = vis;
        }
    }

    UIE.VisualElement BuildCardBackFace(ParentData parent, string role)
    {
        var back = new UIE.VisualElement();
        back.AddToClassList("birth-card-back");

        // タイトル
        var title = UIHelper.CreateLabel("\u2694 \u30b9\u30c6\u30fc\u30bf\u30b9", "birth-card-back-title");
        UIHelper.ApplyFontBold(title);
        back.Add(title);

        // 親名
        var nameLabel = UIHelper.CreateLabel(Localization.GetParent(parent.name), "birth-card-back-name");
        UIHelper.ApplyFontBold(nameLabel);
        back.Add(nameLabel);

        // 区切り線
        var sep = new UIE.VisualElement();
        sep.AddToClassList("birth-card-back-separator");
        back.Add(sep);

        // ステータスバー
        var statsContainer = new UIE.VisualElement();
        statsContainer.style.width = new UIE.StyleLength(new UIE.Length(100, UIE.LengthUnit.Percent));
        statsContainer.style.alignItems = UIE.Align.Center;
        statsContainer.style.flexGrow = 1;
        back.Add(statsContainer);

        string fillClass = "birth-stat-bar-fill-" + role;
        AddStatRow(statsContainer, "\u306c\u304f\u3082\u308a", parent.atk, 100, fillClass);
        AddStatRow(statsContainer, "\u304a\u3061\u3064\u304d", parent.def, 100, fillClass);
        AddStatRow(statsContainer, "\u3054\u304d\u3052\u3093", parent.hp, 200, fillClass);
        AddStatRow(statsContainer, "\u3061\u3048", parent.intelligence, 150, fillClass);
        AddStatRow(statsContainer, "\u3046\u3093\u3069\u3046", parent.athletic, 100, fillClass);
        AddStatRow(statsContainer, "\u3046\u3093", parent.luck, 100, fillClass);
        AddStatRow(statsContainer, "\u3056\u3044\u3055\u3093", parent.fortune, 100, fillClass);

        // 戻るボタン
        var returnBtn = new UIE.Button();
        returnBtn.AddToClassList("birth-card-back-btn");
        UIHelper.ApplyFontBold(returnBtn);
        returnBtn.text = "\u25bc \u8868\u306b\u623b\u3059";
        UIE.VisualElement capturedCard = currentAccordionTargetCard;
        returnBtn.clicked += () => FlipCardToFront(capturedCard);
        back.Add(returnBtn);

        return back;
    }

    void PlayCardFlipSE()
    {
        if (seSource != null && seCardFlip != null)
            seSource.PlayOneShot(seCardFlip, 0.9f);
    }

    void CreateParentCard(UIE.VisualElement parent, out UIE.VisualElement cardObj, out UIE.VisualElement faceImage,
        out UIE.Label nameText, out UIE.Label introText, string role)
    {
        var card = new UIE.VisualElement();
        card.AddToClassList("birth-parent-card");
        card.AddToClassList($"birth-parent-card-{role}");
        card.style.translate = new UIE.StyleTranslate(
            new UIE.Translate(new UIE.Length(-50, UIE.LengthUnit.Percent), new UIE.Length(-50, UIE.LengthUnit.Percent)));
        cardObj = card;

        // カード内側背景
        var inner = new UIE.VisualElement();
        inner.AddToClassList("birth-parent-inner");
        inner.AddToClassList($"birth-parent-inner-{role}");
        card.Add(inner);

        // 名前テキスト (上部)
        nameText = UIHelper.CreateLabel("", "birth-parent-name");
        UIHelper.ApplyFontBold(nameText);
        card.Add(nameText);

        // 区切り線
        var sep = new UIE.VisualElement();
        sep.AddToClassList("birth-parent-separator");
        sep.AddToClassList($"birth-parent-separator-{role}");
        card.Add(sep);

        // 顔画像
        var face = new UIE.VisualElement();
        face.AddToClassList("birth-parent-face");
        card.Add(face);
        faceImage = face;

        // 紹介文テキスト (下部) — visibilityで切り替え（レイアウト確保）
        introText = UIHelper.CreateLabel("", "birth-parent-intro");
        UIHelper.ApplyFontBold(introText);
        introText.style.visibility = UIE.Visibility.Hidden;
        card.Add(introText);

        parent.Add(card);
    }

    void CreateBabyFaceUI()
    {
        if (canvas == null)
            canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null) return;

        // babyFaceが既に設定されている場合は何もしない
        if (babyFace != null) return;

        // babyFaceコンテナを作成
        var babyFaceObj = new GameObject("BabyFace");
        babyFaceObj.transform.SetParent(canvas.transform, false);

        babyFace = babyFaceObj.AddComponent<RectTransform>();
        babyFace.anchorMin = new Vector2(0.5f, 0.5f);
        babyFace.anchorMax = new Vector2(0.5f, 0.5f);
        babyFace.anchoredPosition = new Vector2(0, -50); // 初期位置（ルーレット時）
        babyFace.sizeDelta = new Vector2(180, 180);

        // babyFaceImageを作成（スプライト表示用）
        var imgObj = new GameObject("BabyImage");
        imgObj.transform.SetParent(babyFace, false);
        var imgRect = imgObj.AddComponent<RectTransform>();
        imgRect.anchorMin = Vector2.zero;
        imgRect.anchorMax = Vector2.one;
        imgRect.offsetMin = Vector2.zero;
        imgRect.offsetMax = Vector2.zero;
        babyFaceImage = imgObj.AddComponent<Image>();
        babyFaceImage.color = new Color(1f, 1f, 1f, 0f); // 初期は透明
        babyFaceImage.preserveAspect = true;
        babyFaceImage.raycastTarget = false;

        Debug.Log("[BirthSystem] CreateBabyFaceUI: babyFace and babyFaceImage created");
    }

    // 誕生後のレイアウトに切り替え（赤ちゃんを大きく中央上部に）
    void SwitchToBirthResultLayout(BabySynthesizer.BabyRank rank = BabySynthesizer.BabyRank.D)
    {
        if (overlayRoot == null) return;

        // 親パネルを非表示
        if (parentPanel != null) parentPanel.style.display = UIE.DisplayStyle.None;

        // 既存のカードがあれば削除
        if (birthResultCard != null) birthResultCard.RemoveFromHierarchy();
        // コンテナも削除
        if (overlayRoot != null)
        {
            var oldContainer = UIE.UQueryExtensions.Q(overlayRoot, className: "birth-result-container");
            if (oldContainer != null) oldContainer.RemoveFromHierarchy();
        }

        // ── センタリング用コンテナ（画面上部〜ボタン手前の領域でflexbox中央揃え） ──
        var resultContainer = new UIE.VisualElement();
        resultContainer.AddToClassList("birth-result-container");
        resultContainer.pickingMode = UIE.PickingMode.Ignore;

        // ── ポラロイドカード ──
        birthResultCard = new UIE.VisualElement();
        birthResultCard.AddToClassList("birth-polaroid-wrapper");

        // ドロップシャドウ（フレーム背後に配置）
        var shadow = new UIE.VisualElement();
        shadow.AddToClassList("birth-polaroid-shadow");
        birthResultCard.Add(shadow);

        // ポラロイドフレーム（白枠）
        var frame = new UIE.VisualElement();
        frame.AddToClassList("birth-polaroid-frame");

        // 写真エリア（ボタン化：タップで画像アップロード）
        var photoBtn = new UIE.Button();
        photoBtn.AddToClassList("birth-polaroid-photo");
        photoBtn.clicked += PickImageFromGallery;
        uploadImageBtn = photoBtn;

        // プレースホルダー（画像エリア全体に大きく表示）
        var placeholderLabel = UIHelper.CreateLabel("魂を吹き込む\n（画像をタップ）");
        placeholderLabel.AddToClassList("birth-polaroid-placeholder");
        UIHelper.ApplyFont(placeholderLabel);
        photoBtn.Add(placeholderLabel);

        frame.Add(photoBtn);

        // 性別＋名前ラベル
        babyNameLabel = UIHelper.CreateLabel("");
        babyNameLabel.AddToClassList("birth-baby-name");
        babyNameLabel.enableRichText = true;
        frame.Add(babyNameLabel);

        // 特徴バッジ行（名前の下に配置、後でバッジ追加）
        traitBadgeRow = new UIE.VisualElement();
        traitBadgeRow.AddToClassList("birth-trait-row");
        frame.Add(traitBadgeRow);

        // 子供のBio（紹介文・複数行）
        babyBioLabel = UIHelper.CreateLabel("");
        babyBioLabel.AddToClassList("birth-baby-bio");
        babyBioLabel.enableRichText = true;
        frame.Add(babyBioLabel);

        // マスキングテープ風ステータスメモ
        memoTapeEl = new UIE.VisualElement();
        memoTapeEl.AddToClassList("birth-memo-tape");
        memoTextLabel = UIHelper.CreateLabel("");
        memoTextLabel.AddToClassList("birth-memo-text");
        memoTextLabel.enableRichText = true;
        memoTapeEl.Add(memoTextLabel);
        frame.Add(memoTapeEl);

        // 両親情報
        parentsLineLabel = UIHelper.CreateLabel("");
        parentsLineLabel.AddToClassList("birth-parents-line");
        parentsLineLabel.enableRichText = true;
        frame.Add(parentsLineLabel);

        // 後方互換（他で参照される場合）
        genderLabelEl = babyNameLabel;
        childStatusText = memoTextLabel;

        birthResultCard.Add(frame);
        resultContainer.Add(birthResultCard);
        overlayRoot.Add(resultContainer);

        // 赤ちゃん合成完了まで非表示（プレースホルダーが一瞬見えるのを防止）
        resultContainer.style.opacity = 0;

        // Canvas上のbabyFaceは非表示（UI Toolkit合成表示に切替）
        if (babyFace != null)
            babyFace.gameObject.SetActive(false);
    }

    // ルーレット用のレイアウトにリセット
    void ResetToRouletteLayout()
    {
        // BabySynthesizer のレイヤー合成を破棄
        if (babySynthesizer != null)
            babySynthesizer.Cleanup();

        // 情報パネル系をクリーンアップ
        DestroyStatusCheckButton();

        // カードを元のサイズに戻す
        ResetCardToFullSize(fatherCard, fatherFaceImage);
        ResetCardToFullSize(motherCard, motherFaceImage);

        // カスタム画像要素を削除
        if (customBabyImageEl != null) { customBabyImageEl.RemoveFromHierarchy(); customBabyImageEl = null; }
        if (synthBabyImageEl != null) { synthBabyImageEl.RemoveFromHierarchy(); synthBabyImageEl = null; }

        // babyFaceをルーレット用サイズに戻す
        if (babyFace != null)
        {
            babyFace.gameObject.SetActive(true);
            babyFace.SetParent(canvas.transform, false);
            babyFace.anchorMin = new Vector2(0.5f, 0.5f);
            babyFace.anchorMax = new Vector2(0.5f, 0.5f);
            babyFace.anchoredPosition = new Vector2(0, -50);
            babyFace.sizeDelta = new Vector2(180, 180);
            babyFace.localScale = Vector3.one;

            // 既存の動的生成された子オブジェクトを削除（babyFaceImageとintroText以外）
            for (int i = babyFace.childCount - 1; i >= 0; i--)
            {
                Transform child = babyFace.GetChild(i);
                bool isPreserved = false;
                if (babyFaceImage != null && child.gameObject == babyFaceImage.gameObject) isPreserved = true;
                if (introText != null && child.gameObject == introText.gameObject) isPreserved = true;
                if (!isPreserved)
                {
                    Destroy(child.gameObject);
                }
            }
        }

        // 名前テープとシェアボタンを削除
        if (nameTapeEl != null) { nameTapeEl.RemoveFromHierarchy(); nameTapeEl = null; nameTapeLabel = null; }
        if (shareBtn != null) { shareBtn = null; }
        // シェアラッパーを削除
        if (overlayRoot != null)
        {
            var sw = UIE.UQueryExtensions.Q(overlayRoot, className: "birth-share-wrapper");
            if (sw != null) sw.RemoveFromHierarchy();
        }
        isShareMode = false;

        // birthResultCard & コンテナを削除（UI Toolkit）
        if (screenshotBtn != null) { screenshotBtn.RemoveFromHierarchy(); screenshotBtn = null; }
        if (uploadImageBtn != null) { uploadImageBtn.RemoveFromHierarchy(); uploadImageBtn = null; }
        if (overlayRoot != null)
        {
            var container = UIE.UQueryExtensions.Q(overlayRoot, className: "birth-result-container");
            if (container != null) container.RemoveFromHierarchy();
        }
        if (birthResultCard != null)
        {
            birthResultCard.RemoveFromHierarchy();
            birthResultCard = null;
            genderLabelEl = null;
            statusCardObj = null;
            childStatusText = null;
            traitBadgeRow = null;
            babyNameLabel = null;
            babyBioLabel = null;
            memoTapeEl = null;
            memoTextLabel = null;
            parentsLineLabel = null;
        }

        // babyFaceImageを非表示、introTextを表示
        if (babyFaceImage != null)
        {
            babyFaceImage.color = new Color(1f, 1f, 1f, 0f);
            babyFaceImage.enabled = false;
        }
        if (introText != null)
        {
            introText.gameObject.SetActive(true);
        }
    }

    void ResetBackgroundToBirth()
    {
        if (mainBgImg == null) return;
        var birthBgSprite = Resources.Load<Sprite>("BackGrounds/birth-background");
        if (birthBgSprite != null)
        {
            mainBgImg.sprite = birthBgSprite;
            mainBgImg.type = Image.Type.Simple;
            mainBgImg.preserveAspect = false;
            mainBgImg.color = Color.white;
            var fitter = mainBgImg.GetComponent<AspectRatioFitter>();
            if (fitter != null)
                fitter.aspectRatio = (float)birthBgSprite.texture.width / birthBgSprite.texture.height;
        }
        else
        {
            mainBgImg.color = new Color(0.953f, 0.969f, 0.973f);
        }
    }

    // ===== スクリーンショット・シェア機能 =====

    UIE.Button screenshotBtn;

    void CreateScreenshotButton()
    {
        if (screenshotBtn != null) screenshotBtn.RemoveFromHierarchy();
        if (birthResultCard == null) return;

        screenshotBtn = new UIE.Button();
        screenshotBtn.AddToClassList("birth-polaroid-screenshot-btn");
        UIHelper.ApplyFont(screenshotBtn);
        screenshotBtn.text = "撮";
        screenshotBtn.clicked += TakeScreenshot;

        // ポラロイドフレーム内に配置
        var frame = UIE.UQueryExtensions.Q(birthResultCard, className: "birth-polaroid-frame");
        if (frame != null)
            frame.Add(screenshotBtn);
        else
            birthResultCard.Add(screenshotBtn);
    }

    void TakeScreenshot()
    {
        if (babySynthesizer == null) return;

        var tex = babySynthesizer.CaptureToTexture2D();
        if (tex == null)
        {
            Debug.LogError("[BirthSystem] Screenshot capture failed");
            return;
        }

        byte[] pngData = tex.EncodeToPNG();
        Destroy(tex);

        string fileName = $"godbaby_{System.DateTime.Now:yyyyMMdd_HHmmss}.png";
        string savePath = Path.Combine(Application.persistentDataPath, fileName);
        File.WriteAllBytes(savePath, pngData);
        Debug.Log($"[BirthSystem] Screenshot saved: {savePath}");

#if UNITY_IOS || UNITY_ANDROID
        NativeGallery.SaveImageToGallery(pngData, "GodBabys", fileName, (success, path) =>
        {
            Debug.Log($"[BirthSystem] Gallery save: success={success}, path={path}");
        });
#endif
    }

    // ===== 画像アップロード機能 =====

    void CreateUploadButton()
    {
        // 写真エリア自体がアップロードボタンとして機能
        // SwitchToBirthResultLayout で photoBtn に PickImageFromGallery を登録済み
    }

    void PickImageFromGallery()
    {
        Debug.Log("[BirthSystem] PickImageFromGallery called");

#if UNITY_EDITOR
        // Editorではファイルダイアログを直接使用（HEIC含む）
        string path = UnityEditor.EditorUtility.OpenFilePanel("赤ちゃんの画像を選択", "", "png,jpg,jpeg,heic,heif");
        Debug.Log($"[BirthSystem] Editor file dialog returned: '{path}'");
        if (!string.IsNullOrEmpty(path))
        {
            string ext = Path.GetExtension(path).ToLowerInvariant();
            if (ext == ".heic" || ext == ".heif")
            {
                // macOS の sips コマンドで HEIC → PNG 変換
                string tmpPath = Path.Combine(Application.temporaryCachePath, "heic_converted.png");
                try
                {
                    var psi = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "/usr/bin/sips",
                        Arguments = $"-s format png \"{path}\" --out \"{tmpPath}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    };
                    var proc = System.Diagnostics.Process.Start(psi);
                    proc.WaitForExit(5000);
                    if (File.Exists(tmpPath))
                    {
                        Debug.Log($"[BirthSystem] HEIC → PNG converted: {tmpPath}");
                        path = tmpPath;
                    }
                    else
                    {
                        Debug.LogError("[BirthSystem] HEIC conversion failed");
                        return;
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[BirthSystem] HEIC conversion error: {e.Message}");
                    return;
                }
            }
            LoadAndApplyImage(path);
        }
        else
            Debug.Log("[BirthSystem] Editor file dialog cancelled or empty path");
#else
        // パーミッションチェック
        var permission = NativeGallery.CheckPermission(NativeGallery.PermissionType.Read, NativeGallery.MediaType.Image);
        Debug.Log($"[BirthSystem] Gallery permission: {permission}");
        if (permission == NativeGallery.Permission.Denied)
        {
            Debug.Log("[BirthSystem] Gallery permission denied, opening settings");
            NativeGallery.OpenSettings();
            return;
        }
        if (permission == NativeGallery.Permission.ShouldAsk)
        {
            NativeGallery.RequestPermission(NativeGallery.PermissionType.Read, NativeGallery.MediaType.Image);
        }

        NativeGallery.GetImageFromGallery((path) =>
        {
            if (!string.IsNullOrEmpty(path))
                LoadAndApplyImage(path);
            else
                Debug.Log("[BirthSystem] Gallery returned null/empty path");
        }, "赤ちゃんの画像を選択");
#endif
    }

    void LoadAndApplyImage(string path)
    {
        Debug.Log($"[BirthSystem] LoadAndApplyImage: {path}");
        try
        {
            int maxSize = 1024;

            // NativeGallery.LoadImageAtPath はHEIC/PNG/JPG全対応（iOS変換含む）
            Texture2D tex = NativeGallery.LoadImageAtPath(path, maxSize, false);
            if (tex == null)
            {
                Debug.LogError($"[BirthSystem] Failed to load image. Path: {path}");
                return;
            }
            Debug.Log($"[BirthSystem] Texture loaded: {tex.width}x{tex.height}");

#if UNITY_EDITOR
            // Editor ではネイティブの EXIF 回転処理が行われないため C# で補正
            tex = CorrectExifOrientation(tex, path);
#endif

            // 元画像を非破壊保存
            string origFileName = "baby_custom_original.png";
            File.WriteAllBytes(Path.Combine(Application.persistentDataPath, origFileName), tex.EncodeToPNG());

            if (originalFaceTexture != null) Destroy(originalFaceTexture);
            originalFaceTexture = tex;
            originalFaceW = tex.width;
            originalFaceH = tex.height;

            // 顔ランドマーク検出（iOS Vision / Android MLKit / Editor=null）
            detectedFaceLandmarks = FaceLandmarkBridge.DetectFace(tex);
            if (detectedFaceLandmarks.HasValue)
            {
                var lm = detectedFaceLandmarks.Value;
                Debug.Log($"[BirthSystem] Face detected! Eyes: L({lm.leftEyeCenter.x:F2},{lm.leftEyeCenter.y:F2}) R({lm.rightEyeCenter.x:F2},{lm.rightEyeCenter.y:F2}) Nose:({lm.noseCenter.x:F2},{lm.noseCenter.y:F2})");
            }
            else
            {
                Debug.Log("[BirthSystem] No face detected — manual eye tap will be used");
            }

            // Step1: まず元画像で顔位置調整
            pendingFaceTexture = tex;
            ShowFaceAdjustmentOverlay();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[BirthSystem] Gallery image load failed: {e.Message}\n{e.StackTrace}");
        }
    }

    // ===== フィルターローディングオーバーレイ =====

    void ShowFilterLoadingOverlay()
    {
        if (overlayRoot == null) return;
        if (filterLoadingOverlayEl != null) filterLoadingOverlayEl.RemoveFromHierarchy();

        filterLoadingOverlayEl = new UIE.VisualElement();
        filterLoadingOverlayEl.AddToClassList("filter-loading-overlay");
        overlayRoot.Add(filterLoadingOverlayEl);

        var card = new UIE.VisualElement();
        card.AddToClassList("filter-loading-card");
        filterLoadingOverlayEl.Add(card);

        // キラキラ演出テキスト
        var sparkle = new UIE.Label("\u2726");
        sparkle.AddToClassList("filter-loading-sparkle");
        card.Add(sparkle);

        var label = new UIE.Label("魔法をかけています...");
        label.AddToClassList("filter-loading-text");
        UIHelper.ApplyFont(label);
        card.Add(label);

        // SE（キラキラ音があれば）
        if (seKirakira != null && seSource != null)
            seSource.PlayOneShot(seKirakira, 0.5f);
    }

    void HideFilterLoadingOverlay()
    {
        if (filterLoadingOverlayEl != null)
        {
            filterLoadingOverlayEl.RemoveFromHierarchy();
            filterLoadingOverlayEl = null;
        }
    }

    // ===== 目の位置タップオーバーレイ =====

    void ShowEyeMarkOverlay()
    {
        if (originalFaceTexture == null || overlayRoot == null) return;

        eyeTapCount = 0;
        eyePos1 = new Vector2(-1, -1);
        eyePos2 = new Vector2(-1, -1);

        if (eyeMarkOverlayEl != null) eyeMarkOverlayEl.RemoveFromHierarchy();

        eyeMarkOverlayEl = new UIE.VisualElement();
        eyeMarkOverlayEl.style.position = UIE.Position.Absolute;
        eyeMarkOverlayEl.style.left = 0; eyeMarkOverlayEl.style.top = 0;
        eyeMarkOverlayEl.style.right = 0; eyeMarkOverlayEl.style.bottom = 0;
        eyeMarkOverlayEl.style.backgroundColor = new Color(0, 0, 0, 0.75f);
        eyeMarkOverlayEl.style.alignItems = UIE.Align.Center;
        eyeMarkOverlayEl.style.justifyContent = UIE.Justify.Center;
        overlayRoot.Add(eyeMarkOverlayEl);

        var card = new UIE.VisualElement();
        card.AddToClassList("face-adjust-card");
        eyeMarkOverlayEl.Add(card);

        // タイトル
        var title = new UIE.Label("目の位置をタップ");
        title.AddToClassList("face-adjust-title");
        UIHelper.ApplyFont(title);
        card.Add(title);

        var subtitle = new UIE.Label("左目 \u2192 右目 の順にタップ");
        subtitle.style.fontSize = 26;
        subtitle.style.color = new Color(0.5f, 0.5f, 0.55f);
        subtitle.style.unityTextAlign = UnityEngine.TextAnchor.MiddleCenter;
        subtitle.style.marginBottom = 16;
        UIHelper.ApplyFont(subtitle);
        card.Add(subtitle);

        // プレビュー（元画像を表示）
        eyeMarkPreview = new UIE.VisualElement();
        eyeMarkPreview.style.width = 600;
        eyeMarkPreview.style.height = 600;
        eyeMarkPreview.style.backgroundImage = new UIE.StyleBackground(originalFaceTexture);
        eyeMarkPreview.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;
        eyeMarkPreview.style.alignSelf = UIE.Align.Center;
        eyeMarkPreview.style.position = UIE.Position.Relative;
        card.Add(eyeMarkPreview);

        // タップイベント
        eyeMarkPreview.RegisterCallback<UIE.PointerDownEvent>(OnEyeMarkTap);

        // ボタン行
        var btnRow = new UIE.VisualElement();
        btnRow.AddToClassList("face-adjust-btn-row");
        btnRow.style.marginTop = 16;
        card.Add(btnRow);

        var applyBtn = new UIE.Button(() => OnEyeMarkConfirm());
        applyBtn.AddToClassList("face-adjust-confirm-btn");
        applyBtn.text = "\u2726 魔法をかける";
        UIHelper.ApplyFont(applyBtn);
        btnRow.Add(applyBtn);

        var skipBtn = new UIE.Button(() => OnEyeMarkSkip());
        skipBtn.AddToClassList("face-adjust-cancel-btn");
        skipBtn.text = "スキップ";
        UIHelper.ApplyFont(skipBtn);
        btnRow.Add(skipBtn);

        // dev用: デッサン風フィルター
        var dessinBtn = new UIE.Button(() => OnEyeMarkConfirmDessin());
        dessinBtn.style.height = 60;
        dessinBtn.style.paddingLeft = 16;
        dessinBtn.style.paddingRight = 16;
        dessinBtn.style.marginTop = 8;
        dessinBtn.style.backgroundColor = new UIE.StyleColor(new Color(0.85f, 0.85f, 0.85f));
        dessinBtn.style.color = new UIE.StyleColor(Color.black);
        dessinBtn.style.fontSize = 24;
        dessinBtn.style.borderTopLeftRadius = 30;
        dessinBtn.style.borderTopRightRadius = 30;
        dessinBtn.style.borderBottomLeftRadius = 30;
        dessinBtn.style.borderBottomRightRadius = 30;
        dessinBtn.text = "[DEV] デッサン風";
        UIHelper.ApplyFont(dessinBtn);
        card.Add(dessinBtn);
    }

    void OnEyeMarkTap(UIE.PointerDownEvent evt)
    {
        if (eyeMarkPreview == null) return;

        // タップ位置をプレビュー内の正規化座標に変換
        Vector2 local = evt.localPosition;
        float previewW = eyeMarkPreview.resolvedStyle.width;
        float previewH = eyeMarkPreview.resolvedStyle.height;
        if (previewW <= 0 || previewH <= 0) return;

        // ScaleToFitの実際の描画領域を計算
        float texAspect = (float)originalFaceTexture.width / originalFaceTexture.height;
        float previewAspect = previewW / previewH;
        float drawW, drawH, drawX, drawY;
        if (texAspect > previewAspect)
        {
            drawW = previewW;
            drawH = previewW / texAspect;
            drawX = 0;
            drawY = (previewH - drawH) * 0.5f;
        }
        else
        {
            drawH = previewH;
            drawW = previewH * texAspect;
            drawX = (previewW - drawW) * 0.5f;
            drawY = 0;
        }

        float nx = (local.x - drawX) / drawW;
        float ny = (local.y - drawY) / drawH;
        if (nx < 0 || nx > 1 || ny < 0 || ny > 1) return;

        eyeTapCount++;
        if (eyeTapCount == 1)
        {
            eyePos1 = new Vector2(nx, ny);
            AddEyeMarker(local.x, local.y, "L");
            Debug.Log($"[BirthSystem] Left eye marked: ({nx:F2}, {ny:F2})");
        }
        else if (eyeTapCount == 2)
        {
            eyePos2 = new Vector2(nx, ny);
            AddEyeMarker(local.x, local.y, "R");
            Debug.Log($"[BirthSystem] Right eye marked: ({nx:F2}, {ny:F2})");
        }
        // 3回以上はリセット
        else
        {
            eyeTapCount = 1;
            eyePos1 = new Vector2(nx, ny);
            eyePos2 = new Vector2(-1, -1);
            // マーカー全削除して新しく追加
            var markers = UIE.UQueryExtensions.Query(eyeMarkPreview, className: "eye-marker-dot").ToList();
            foreach (var m in markers) m.RemoveFromHierarchy();
            AddEyeMarker(local.x, local.y, "L");
        }
    }

    void AddEyeMarker(float x, float y, string label)
    {
        var marker = new UIE.VisualElement();
        marker.AddToClassList("eye-marker-dot");
        marker.style.position = UIE.Position.Absolute;
        marker.style.left = x - 20;
        marker.style.top = y - 20;
        marker.style.width = 40;
        marker.style.height = 40;
        marker.style.borderTopLeftRadius = 20;
        marker.style.borderTopRightRadius = 20;
        marker.style.borderBottomLeftRadius = 20;
        marker.style.borderBottomRightRadius = 20;
        marker.style.backgroundColor = new Color(1f, 0.72f, 0.77f, 0.7f);
        marker.style.borderTopWidth = 3;
        marker.style.borderBottomWidth = 3;
        marker.style.borderLeftWidth = 3;
        marker.style.borderRightWidth = 3;
        marker.style.borderTopColor = Color.white;
        marker.style.borderBottomColor = Color.white;
        marker.style.borderLeftColor = Color.white;
        marker.style.borderRightColor = Color.white;
        marker.style.alignItems = UIE.Align.Center;
        marker.style.justifyContent = UIE.Justify.Center;
        marker.pickingMode = UIE.PickingMode.Ignore;

        var text = new UIE.Label(label);
        text.style.fontSize = 18;
        text.style.color = Color.white;
        text.style.unityFontStyleAndWeight = UnityEngine.FontStyle.Bold;
        text.style.unityTextAlign = UnityEngine.TextAnchor.MiddleCenter;
        text.pickingMode = UIE.PickingMode.Ignore;
        marker.Add(text);

        eyeMarkPreview.Add(marker);
    }

    void OnEyeMarkConfirm()
    {
        // 目の位置が2つマークされていなくてもフィルター自体は適用する
        Vector2 leftEye = eyePos1.x >= 0 ? eyePos1 : new Vector2(0.35f, 0.38f);
        Vector2 rightEye = eyePos2.x >= 0 ? eyePos2 : new Vector2(0.65f, 0.38f);

        // 目タップ座標からランドマークを構築（morph用）
        // 座標空間: 元画像空間 (0-1, top-left原点) = ネイティブランドマークと同じ
        if (eyePos1.x >= 0 && eyePos2.x >= 0)
        {
            Vector2 eyeMid = (leftEye + rightEye) * 0.5f;
            float eyeDist = Vector2.Distance(leftEye, rightEye);
            detectedFaceLandmarks = new FaceLandmarkResult
            {
                faceBounds = new Rect(
                    eyeMid.x - eyeDist, eyeMid.y - eyeDist * 0.5f,
                    eyeDist * 2f, eyeDist * 2.5f),
                leftEyeCenter = leftEye,
                rightEyeCenter = rightEye,
                mouthCenter = new Vector2(eyeMid.x, eyeMid.y + eyeDist * 1.0f),
                noseCenter = new Vector2(eyeMid.x, eyeMid.y + eyeDist * 0.5f),
                leftCheek = new Vector2(leftEye.x - eyeDist * 0.3f, eyeMid.y + eyeDist * 0.4f),
                rightCheek = new Vector2(rightEye.x + eyeDist * 0.3f, eyeMid.y + eyeDist * 0.4f),
                jawlinePoints = new Vector2[0],
            };
            Debug.Log($"[BirthSystem] Eye-tap → landmarks: L({leftEye.x:F2},{leftEye.y:F2}) R({rightEye.x:F2},{rightEye.y:F2}) Mouth({detectedFaceLandmarks.Value.mouthCenter.x:F2},{detectedFaceLandmarks.Value.mouthCenter.y:F2})");
        }

        if (eyeMarkOverlayEl != null)
        {
            eyeMarkOverlayEl.RemoveFromHierarchy();
            eyeMarkOverlayEl = null;
        }
        eyeMarkPreview = null;

        // フィルター適用 → 合成
        StartCoroutine(ApplyFilterAndComposite(leftEye, rightEye));
    }

    void OnEyeMarkConfirmDessin()
    {
        Vector2 leftEye = eyePos1.x >= 0 ? eyePos1 : new Vector2(0.35f, 0.38f);
        Vector2 rightEye = eyePos2.x >= 0 ? eyePos2 : new Vector2(0.65f, 0.38f);

        if (eyePos1.x >= 0 && eyePos2.x >= 0)
        {
            Vector2 eyeMid = (leftEye + rightEye) * 0.5f;
            float eyeDist = Vector2.Distance(leftEye, rightEye);
            detectedFaceLandmarks = new FaceLandmarkResult
            {
                faceBounds = new Rect(
                    eyeMid.x - eyeDist, eyeMid.y - eyeDist * 0.5f,
                    eyeDist * 2f, eyeDist * 2.5f),
                leftEyeCenter = leftEye,
                rightEyeCenter = rightEye,
                mouthCenter = new Vector2(eyeMid.x, eyeMid.y + eyeDist * 1.0f),
                noseCenter = new Vector2(eyeMid.x, eyeMid.y + eyeDist * 0.5f),
                leftCheek = new Vector2(leftEye.x - eyeDist * 0.3f, eyeMid.y + eyeDist * 0.4f),
                rightCheek = new Vector2(rightEye.x + eyeDist * 0.3f, eyeMid.y + eyeDist * 0.4f),
                jawlinePoints = new Vector2[0],
            };
        }

        if (eyeMarkOverlayEl != null)
        {
            eyeMarkOverlayEl.RemoveFromHierarchy();
            eyeMarkOverlayEl = null;
        }
        eyeMarkPreview = null;

        StartCoroutine(ApplyFilterAndComposite(leftEye, rightEye, useDessin: true));
    }

    void OnEyeMarkSkip()
    {
        if (eyeMarkOverlayEl != null)
        {
            eyeMarkOverlayEl.RemoveFromHierarchy();
            eyeMarkOverlayEl = null;
        }
        eyeMarkPreview = null;

        // 目拡大なし（-1,-1 で無効化）→ 色調+光漏れのみ
        StartCoroutine(ApplyFilterAndComposite(
            new Vector2(-1, -1), new Vector2(-1, -1)));
    }

    IEnumerator ApplyFilterAndComposite(Vector2 leftEye, Vector2 rightEye, bool useDessin = false)
    {
        ShowFilterLoadingOverlay();
        yield return null;

        // フィルター適用
        Texture2D filtered;
        if (useDessin)
        {
            filtered = BabySynthesizer.CreateDessinFilteredTexture(
                originalFaceTexture, leftEye, rightEye,
                detectedFaceLandmarks.HasValue ? detectedFaceLandmarks.Value : (FaceLandmarkResult?)null);
        }
        else if (detectedFaceLandmarks.HasValue)
        {
            filtered = BabySynthesizer.CreateBabyFilteredTexture(
                originalFaceTexture, leftEye, rightEye, detectedFaceLandmarks.Value);
        }
        else
        {
            filtered = BabySynthesizer.CreateBabyFilteredTexture(
                originalFaceTexture, leftEye, rightEye);
        }
        if (filtered == null)
        {
            HideFilterLoadingOverlay();
            yield break;
        }

        // 保存
        string fileName = "baby_custom.png";
        File.WriteAllBytes(Path.Combine(Application.persistentDataPath, fileName), filtered.EncodeToPNG());
        if (DataCarrier.Instance != null)
            DataCarrier.Instance.customBabyImagePath = fileName;

        // ★ BabyMorph パラメータを先に設定（Synthesize内で自動適用される）
        {
            float holeCX = babySynthesizer.GetFaceHoleCX();
            float holeCY = babySynthesizer.GetFaceHoleCY();
            float holeRX = babySynthesizer.GetFaceHoleRX();
            float holeRY = babySynthesizer.GetFaceHoleRY();

            Debug.Log($"[CoordTransform] ═══ 座標変換パイプライン開始 ═══\n" +
                $"  元画像サイズ: {originalFaceW}x{originalFaceH} (aspect={((float)originalFaceW / Mathf.Max(originalFaceH, 1)):F3})\n" +
                $"  クロップ方式: Center Crop (cropSize={Mathf.Min(originalFaceW, originalFaceH)})\n" +
                $"  顔穴パラメータ: center=({holeCX:F3},{holeCY:F3}) radius=({holeRX:F3},{holeRY:F3})\n" +
                $"  ユーザー調整: scale={eyeMarkSavedScale:F2} offset=({eyeMarkSavedOffsetX:F3},{eyeMarkSavedOffsetY:F3})\n" +
                $"  ランドマーク有無: {detectedFaceLandmarks.HasValue}");

            // ★ 目は顔中心より上 → bottom-origin では holeCY + offset
            Vector2 morphLeftEye = new Vector2(holeCX - holeRX * 0.50f, holeCY + holeRY * 0.35f);
            Vector2 morphRightEye = new Vector2(holeCX + holeRX * 0.50f, holeCY + holeRY * 0.35f);
            // ★ 口は顔中心より下 → bottom-origin では holeCY - offset
            Vector2 morphMouth = new Vector2(holeCX, holeCY - holeRY * 0.55f);
            if (detectedFaceLandmarks.HasValue)
            {
                var lm = detectedFaceLandmarks.Value;
                Debug.Log($"[CoordTransform] ランドマーク入力(top-left): " +
                    $"leftEye=({lm.leftEyeCenter.x:F3},{lm.leftEyeCenter.y:F3}) " +
                    $"rightEye=({lm.rightEyeCenter.x:F3},{lm.rightEyeCenter.y:F3}) " +
                    $"mouth=({lm.mouthCenter.x:F3},{lm.mouthCenter.y:F3})");
                var lmLeft = FaceLandmarkToComposite(lm.leftEyeCenter);
                var lmRight = FaceLandmarkToComposite(lm.rightEyeCenter);
                var lmMouth = FaceLandmarkToComposite(lm.mouthCenter);

                // 顔穴内に収まるか検証
                bool inBounds = lmLeft.x > holeCX - holeRX && lmLeft.x < holeCX + holeRX &&
                    lmLeft.y > holeCY - holeRY && lmLeft.y < holeCY + holeRY;
                Debug.Log($"[CoordTransform] 変換結果: " +
                    $"leftEye=({lmLeft.x:F3},{lmLeft.y:F3}) " +
                    $"rightEye=({lmRight.x:F3},{lmRight.y:F3}) " +
                    $"mouth=({lmMouth.x:F3},{lmMouth.y:F3}) " +
                    $"顔穴内={inBounds} [bounds: x({holeCX - holeRX:F3}~{holeCX + holeRX:F3}) y({holeCY - holeRY:F3}~{holeCY + holeRY:F3})]");

                if (inBounds)
                {
                    morphLeftEye = lmLeft;
                    morphRightEye = lmRight;
                    morphMouth = lmMouth;
                    Debug.Log($"[BirthSystem] ★ ランドマーク座標を採用");
                }
                else
                {
                    Debug.LogWarning($"[BirthSystem] ★ ランドマークが顔穴外 → ジオメトリフォールバック使用");
                }
            }
            Debug.Log($"[BirthSystem] EnableBabyMorph 最終値: L({morphLeftEye.x:F3},{morphLeftEye.y:F3}) R({morphRightEye.x:F3},{morphRightEye.y:F3}) Mouth({morphMouth.x:F3},{morphMouth.y:F3})");
            babySynthesizer.EnableBabyMorph(morphLeftEye, morphRightEye, morphMouth, detectedFaceLandmarks);
        }

        // 合成（顔位置調整で保存したパラメータを使用）
        float synthOffX = BabySynthesizer.IsDebugFixedFace ? 0f : eyeMarkSavedOffsetX;
        float synthOffY = BabySynthesizer.IsDebugFixedFace ? 0f : eyeMarkSavedOffsetY;
        float synthScale = BabySynthesizer.IsDebugFixedFace ? 1f : eyeMarkSavedScale;
        babySynthesizer.SetCustomFaceTexture(
            filtered, synthOffX, synthOffY, synthScale);

        // ★ モーフを合成画像に直接適用（多眼バグ修正: アルファブレンド不要に）
        Sprite updatedSprite = babySynthesizer.ApplyMorphToCompositeIfEnabled();

        HideFilterLoadingOverlay();

        if (updatedSprite != null)
            DisplaySynthesizedBaby(updatedSprite);

        if (babyFace != null)
            babyFace.gameObject.SetActive(false);

        SaveSynthBabyImage();

        if (seKirakira != null && seSource != null)
            seSource.PlayOneShot(seKirakira, 1f);

        SwitchToNamingMode();
        if (gotoBattleButton != null)
        {
            gotoBattleButton.SetActive(true);
            StartCoroutine(NamingButtonBounceAnimation());
        }

        Destroy(filtered);
        Debug.Log($"[BirthSystem] Filter applied with eyes: L({leftEye.x:F2},{leftEye.y:F2}) R({rightEye.x:F2},{rightEye.y:F2})");
    }

    void UpdateBabyFaceWithCustomImage(Sprite spr)
    {
        Debug.Log($"[BirthSystem] UpdateBabyFaceWithCustomImage - redirecting to BabySynthesizer");

        // BabySynthesizer経由でカスタム画像を差し替え → 再レンダリング → UI表示更新
        if (babySynthesizer != null && spr != null && spr.texture != null)
        {
            Sprite updatedSprite = babySynthesizer.SetCustomFaceTexture(spr.texture);
            if (updatedSprite != null)
            {
                DisplaySynthesizedBaby(updatedSprite);
                SaveSynthBabyImage();

                // キラキラ演出（アップロード完了）
                if (seKirakira != null)
                    seSource.PlayOneShot(seKirakira, 0.8f);
            }
        }

        // Canvas上のbabyFaceを非表示（SpriteRenderer合成を使用）
        if (babyFace != null)
            babyFace.gameObject.SetActive(false);
    }

    void SaveSynthBabyImage()
    {
        if (babySynthesizer == null) return;

        // ★ モーフは ApplyFilterAndComposite で既に composite に直接適用済み
        // アルファブレンド合成は不要（多眼バグの原因だったため削除）

        string fileName = "synth_baby.png";
        string savePath = Path.Combine(Application.persistentDataPath, fileName);

        // 背景あり版（モーフ適用済み composite をそのまま保存）
        var bgTex = babySynthesizer.CaptureToTexture2D();
        if (bgTex != null)
        {
            File.WriteAllBytes(savePath, bgTex.EncodeToPNG());
            Destroy(bgTex);
            Debug.Log("[BirthSystem] synth_baby.png saved (morph applied in composite)");
        }

        // 透過版（バトル/マップ用: 背景なし + モーフ適用）
        var transTex = babySynthesizer.CaptureTransparentTexture2D();
        if (transTex != null)
        {
            string transFileName = "synth_baby_transparent.png";
            string transSavePath = Path.Combine(Application.persistentDataPath, transFileName);
            File.WriteAllBytes(transSavePath, transTex.EncodeToPNG());
            Destroy(transTex);
        }

        if (DataCarrier.Instance != null)
            DataCarrier.Instance.synthBabyImagePath = fileName;

        Debug.Log($"[BirthSystem] Synth baby image saved: {savePath}");
    }

    // ===== 顔調整オーバーレイ =====

    void ShowFaceAdjustmentOverlay()
    {
        if (pendingFaceTexture == null || overlayRoot == null) return;

        // 初期化
        faceAdjOffsetX = 0f;
        faceAdjOffsetY = 0f;
        faceAdjScale = 1f;
        isDraggingFace = false;

        // 既存のオーバーレイがあれば削除
        if (faceAdjustOverlayEl != null)
            faceAdjustOverlayEl.RemoveFromHierarchy();

        // オーバーレイ暗幕
        faceAdjustOverlayEl = new UIE.VisualElement();
        faceAdjustOverlayEl.style.position = UIE.Position.Absolute;
        faceAdjustOverlayEl.style.left = 0;
        faceAdjustOverlayEl.style.top = 0;
        faceAdjustOverlayEl.style.right = 0;
        faceAdjustOverlayEl.style.bottom = 0;
        faceAdjustOverlayEl.style.backgroundColor = new Color(0, 0, 0, 0.7f);
        faceAdjustOverlayEl.style.alignItems = UIE.Align.Center;
        faceAdjustOverlayEl.style.justifyContent = UIE.Justify.Center;
        overlayRoot.Add(faceAdjustOverlayEl);

        // カード
        var card = new UIE.VisualElement();
        card.AddToClassList("face-adjust-card");
        faceAdjustOverlayEl.Add(card);

        // タイトル
        var title = new UIE.Label("顔の位置を調整");
        title.AddToClassList("face-adjust-title");
        UIHelper.ApplyFont(title);
        card.Add(title);

        // プレビューエリア（顔画像 → BabyWear(くり抜き済み)の順で重ねる）
        var preview = new UIE.VisualElement();
        preview.AddToClassList("face-adjust-preview");
        preview.style.position = UIE.Position.Relative;
        preview.style.overflow = UIE.Overflow.Hidden;
        card.Add(preview);

        // 顔画像の基準位置（ピクセル）— BabySynthesizerの検出値に基づく
        float faceCenterX = 700f * (babySynthesizer != null ? babySynthesizer.GetFaceHoleCX() : 0.50f);
        float faceCenterY = 700f * GetFaceHoleTopPct() / 100f;
        facePreviewCenterX = faceCenterX;
        facePreviewCenterY = faceCenterY;
        float facePreviewSize = GetFacePreviewUniform();
        float halfFace = facePreviewSize / 2f;

        // Layer 1: 顔画像（最背面）— DrawFace の uniformR*2 に対応する正方形
        facePreviewImage = new UIE.VisualElement();
        facePreviewImage.AddToClassList("face-adjust-face-img");
        facePreviewImage.style.position = UIE.Position.Absolute;
        facePreviewImage.style.left = faceCenterX - halfFace;
        facePreviewImage.style.top = faceCenterY - halfFace;
        facePreviewImage.style.width = facePreviewSize;
        facePreviewImage.style.height = facePreviewSize;
        facePreviewImage.style.backgroundImage = new UIE.StyleBackground(pendingFaceTexture);
        // ★ DrawFace の正方形中央クロップと同じ表示にする（StretchToFill だと非正方形画像で歪む）
        facePreviewImage.style.unityBackgroundScaleMode = ScaleMode.ScaleAndCrop;
        facePreviewImage.pickingMode = UIE.PickingMode.Ignore;
        preview.Add(facePreviewImage);

        // Layer 2: BabyWear オーバーレイ（顔穴をくり抜いた状態）
        var rank = BabySynthesizer.DetermineRank(lastSynthParams.fortune);
        Sprite wearSprite = BabySynthesizer.LoadWearSprite(rank);
        if (wearSprite != null && wearSprite.texture.isReadable)
        {
            // BabyWearテクスチャの顔穴をくり抜いたコピーを作成
            if (hollowWearTexture != null) Destroy(hollowWearTexture);
            hollowWearTexture = CreateHollowWearTexture(wearSprite.texture);

            var wearImg = new UIE.VisualElement();
            wearImg.AddToClassList("face-adjust-wear");
            wearImg.style.position = UIE.Position.Absolute;
            wearImg.style.left = 0;
            wearImg.style.top = 0;
            wearImg.style.width = 700;
            wearImg.style.height = 700;
            wearImg.style.backgroundImage = new UIE.StyleBackground(hollowWearTexture);
            wearImg.pickingMode = UIE.PickingMode.Ignore;
            preview.Add(wearImg);
        }

        // ドラッグイベント登録（preview全体で受ける）
        preview.RegisterCallback<UIE.PointerDownEvent>(OnFacePointerDown);
        preview.RegisterCallback<UIE.PointerMoveEvent>(OnFacePointerMove);
        preview.RegisterCallback<UIE.PointerUpEvent>(OnFacePointerUp);

        // スライダー行
        var sliderRow = new UIE.VisualElement();
        sliderRow.AddToClassList("face-adjust-slider-row");
        card.Add(sliderRow);

        var sliderLabel = new UIE.Label("ズーム");
        sliderLabel.AddToClassList("face-adjust-slider-label");
        UIHelper.ApplyFont(sliderLabel);
        sliderRow.Add(sliderLabel);

        var slider = new UIE.Slider(null, 0.5f, 6.0f);
        slider.value = 1f;
        slider.AddToClassList("face-adjust-slider");
        slider.RegisterCallback<UIE.ChangeEvent<float>>(evt =>
        {
            faceAdjScale = evt.newValue;
            UpdateFacePreviewTransform();
        });
        sliderRow.Add(slider);

        // 回転ボタン行
        var rotateRow = new UIE.VisualElement();
        rotateRow.style.flexDirection = UIE.FlexDirection.Row;
        rotateRow.style.justifyContent = UIE.Justify.Center;
        rotateRow.style.marginTop = 8;
        rotateRow.style.marginBottom = 8;
        card.Add(rotateRow);

        var rotateBtn = new UIE.Button(() => RotateFaceTexture90CW());
        rotateBtn.style.width = 200;
        rotateBtn.style.height = 60;
        rotateBtn.style.fontSize = 26;
        rotateBtn.style.borderTopLeftRadius = 30;
        rotateBtn.style.borderTopRightRadius = 30;
        rotateBtn.style.borderBottomLeftRadius = 30;
        rotateBtn.style.borderBottomRightRadius = 30;
        rotateBtn.style.backgroundColor = new Color(0.97f, 0.91f, 0.81f);
        rotateBtn.text = "回転";
        UIHelper.ApplyFont(rotateBtn);
        rotateRow.Add(rotateBtn);

        // ボタン行
        var btnRow = new UIE.VisualElement();
        btnRow.AddToClassList("face-adjust-btn-row");
        card.Add(btnRow);

        var confirmBtn = new UIE.Button(() => CloseFaceAdjustOverlay(true));
        confirmBtn.AddToClassList("face-adjust-confirm-btn");
        confirmBtn.text = "決定";
        UIHelper.ApplyFont(confirmBtn);
        btnRow.Add(confirmBtn);

        var cancelBtn = new UIE.Button(() => CloseFaceAdjustOverlay(false));
        cancelBtn.AddToClassList("face-adjust-cancel-btn");
        cancelBtn.text = "キャンセル";
        UIHelper.ApplyFont(cancelBtn);
        btnRow.Add(cancelBtn);

        Debug.Log("[BirthSystem] Face adjustment overlay shown");
    }

    void OnFacePointerDown(UIE.PointerDownEvent evt)
    {
        isDraggingFace = true;
        dragStartPos = evt.position;
        dragStartOffsetX = faceAdjOffsetX;
        dragStartOffsetY = faceAdjOffsetY;
        UIE.PointerCaptureHelper.CapturePointer((UIE.VisualElement)evt.currentTarget, evt.pointerId);
    }

    void OnFacePointerMove(UIE.PointerMoveEvent evt)
    {
        if (!isDraggingFace) return;

        // ドラッグ差分をオフセットに変換（DrawFaceのuniformR*2に対応）
        float uniformSize = GetFacePreviewUniform();
        float dx = evt.position.x - dragStartPos.x;
        float dy = evt.position.y - dragStartPos.y;
        faceAdjOffsetX = dragStartOffsetX + dx / (uniformSize * faceAdjScale);
        faceAdjOffsetY = dragStartOffsetY - dy / (uniformSize * faceAdjScale);
        UpdateFacePreviewTransform();
    }

    void OnFacePointerUp(UIE.PointerUpEvent evt)
    {
        isDraggingFace = false;
        UIE.PointerCaptureHelper.ReleasePointer((UIE.VisualElement)evt.currentTarget, evt.pointerId);
    }

    void UpdateFacePreviewTransform()
    {
        if (facePreviewImage == null) return;

        float uniformSize = GetFacePreviewUniform();
        float scaledSize = uniformSize * faceAdjScale;

        // CSS scale ではなく要素サイズを直接変更
        // （UIToolkit の style.scale は overflow:hidden のクリップに反映されないため）
        facePreviewImage.style.width = scaledSize;
        facePreviewImage.style.height = scaledSize;

        // オフセットをスクリーンピクセルに変換
        float txPx = faceAdjOffsetX * scaledSize;
        float tyPx = -faceAdjOffsetY * scaledSize;

        // 顔中心を基準に配置（overflow:hidden で顔穴外は自動クリップ）
        facePreviewImage.style.left = facePreviewCenterX - scaledSize / 2f + txPx;
        facePreviewImage.style.top = facePreviewCenterY - scaledSize / 2f + tyPx;

        // CSS transform をリセット（以前の値が残っている場合）
        facePreviewImage.style.scale = new UIE.StyleScale(new UIE.Scale(Vector2.one));
        facePreviewImage.style.translate = new UIE.StyleTranslate(new UIE.Translate(0, 0));
    }

    void RotateFaceTexture90CW()
    {
        if (pendingFaceTexture == null) return;

        int w = pendingFaceTexture.width;
        int h = pendingFaceTexture.height;
        Color[] srcPx = pendingFaceTexture.GetPixels();

        // 90° CW: dst(y, w-1-x) = src(x, y)  → dst is h×w
        Texture2D rotated = new Texture2D(h, w, TextureFormat.RGBA32, false);
        Color[] dstPx = new Color[w * h];
        for (int y = 0; y < w; y++)
            for (int x = 0; x < h; x++)
                dstPx[y * h + x] = srcPx[x * w + (w - 1 - y)];
        rotated.SetPixels(dstPx);
        rotated.Apply();

        Destroy(pendingFaceTexture);
        pendingFaceTexture = rotated;
        originalFaceTexture = rotated;
        originalFaceW = rotated.width;
        originalFaceH = rotated.height;

        // プレビュー更新
        if (facePreviewImage != null)
            facePreviewImage.style.backgroundImage = new UIE.StyleBackground(rotated);

        // ズーム/オフセットリセット
        faceAdjOffsetX = 0f;
        faceAdjOffsetY = 0f;
        faceAdjScale = 1f;
        UpdateFacePreviewTransform();

        Debug.Log($"[BirthSystem] Face rotated 90° CW: {w}x{h} → {rotated.width}x{rotated.height}");
    }

    void CloseFaceAdjustOverlay(bool confirmed)
    {
        // 位置パラメータを保存
        eyeMarkSavedOffsetX = faceAdjOffsetX;
        eyeMarkSavedOffsetY = faceAdjOffsetY;
        eyeMarkSavedScale = faceAdjScale;

        pendingFaceTexture = null;

        if (faceAdjustOverlayEl != null)
        {
            faceAdjustOverlayEl.RemoveFromHierarchy();
            faceAdjustOverlayEl = null;
        }
        facePreviewImage = null;

        if (hollowWearTexture != null)
        {
            Destroy(hollowWearTexture);
            hollowWearTexture = null;
        }

        if (confirmed && originalFaceTexture != null && babySynthesizer != null)
        {
            if (detectedFaceLandmarks.HasValue)
            {
                // 顔検出成功 → 目タップスキップ、検出座標で直接フィルター適用
                var lm = detectedFaceLandmarks.Value;
                Debug.Log("[BirthSystem] Face landmarks available — skipping eye mark step");
                StartCoroutine(ApplyFilterAndComposite(lm.leftEyeCenter, lm.rightEyeCenter));
            }
            else
            {
                // 顔検出失敗 → 従来の手動目タップ
                ShowEyeMarkOverlay();
            }
        }
        else
        {
            Debug.Log("[BirthSystem] Face adjustment cancelled");
        }
    }

    /// <summary>
    /// BabyWearテクスチャの顔穴部分を透明にしたコピーを作成する。
    /// Default_Wear PNGは顔穴部分が既に透過されているので、そのままコピーする。
    /// 透過がない場合はBabySynthesizerのパラメータで楕円くり抜きにフォールバック。
    /// </summary>
    Texture2D CreateHollowWearTexture(Texture2D srcTex)
    {
        int w = srcTex.width;
        int h = srcTex.height;
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        Color[] pixels = srcTex.GetPixels();

        // PNGに透過ピクセルが存在するかチェック
        bool hasTransparency = false;
        for (int i = 0; i < pixels.Length; i++)
        {
            if (pixels[i].a < 0.1f) { hasTransparency = true; break; }
        }

        if (!hasTransparency)
        {
            // 透過がない場合はBabySynthesizerのパラメータで楕円くり抜き
            float holeCx = w * (babySynthesizer != null ? babySynthesizer.GetFaceHoleCX() : 0.50f);
            float holeCy = h * (babySynthesizer != null ? babySynthesizer.GetFaceHoleCY() : 0.68f);
            float holeRx = w * (babySynthesizer != null ? babySynthesizer.GetFaceHoleRX() : 0.20f);
            float holeRy = h * (babySynthesizer != null ? babySynthesizer.GetFaceHoleRY() : 0.20f);
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    float fx = (x - holeCx) / holeRx;
                    float fy = (y - holeCy) / holeRy;
                    if (fx * fx + fy * fy < 1f)
                        pixels[y * w + x] = new Color(0, 0, 0, 0);
                }
        }
        // PNGに既に透過がある場合はそのままコピー

        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }

    /// <summary>
    /// 親のimageNameからアイテムスプライトのResourcesパスを生成する
    /// </summary>
    static string GetParentItemPath(string imageName, bool isFather)
    {
        if (string.IsNullOrEmpty(imageName)) return "";
        string pascal = char.ToUpper(imageName[0]) + imageName.Substring(1);
        string prefix = isFather ? "Father" : "Mother";
        return $"ParentItems/{prefix}_{pascal}_Item";
    }

    /// <summary>
    /// BabySynthesizerの合成結果SpriteをUI Toolkit上に表示する
    /// </summary>
    void DisplaySynthesizedBaby(Sprite synthSprite)
    {
        if (birthResultCard == null) return;

        // ポラロイドの写真ボタンを取得
        var photoBtn = UIE.UQueryExtensions.Q<UIE.Button>(birthResultCard, className: "birth-polaroid-photo");
        if (photoBtn == null) return;

        // シルエット等をクリアして赤ちゃん画像を表示
        photoBtn.Clear();

        if (synthBabyImageEl != null)
            synthBabyImageEl.RemoveFromHierarchy();

        synthBabyImageEl = new UIE.VisualElement();
        synthBabyImageEl.name = "synth-baby-image";
        synthBabyImageEl.pickingMode = UIE.PickingMode.Ignore;
        synthBabyImageEl.AddToClassList("birth-polaroid-image");
        synthBabyImageEl.style.backgroundImage = new UIE.StyleBackground(synthSprite);
        photoBtn.Add(synthBabyImageEl);

        // Light Leak オーバーレイ（虹色の光漏れをUI層で重ねる）
        var lightLeak = new UIE.VisualElement();
        lightLeak.name = "light-leak-overlay";
        lightLeak.pickingMode = UIE.PickingMode.Ignore;
        lightLeak.AddToClassList("birth-light-leak-overlay");
        photoBtn.Add(lightLeak);

        // バウンド・アニメーション（SE同期）
        birthResultCard.style.scale = new UIE.StyleScale(
            new UIE.Scale(new Vector3(0.8f, 0.8f, 1f)));
        StartCoroutine(PolaroidBounceAnimation());

        Debug.Log("[BirthSystem] Synthesized baby displayed in polaroid");
    }

    IEnumerator PolaroidBounceAnimation()
    {
        if (birthResultCard == null) yield break;

        float duration = 0.4f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float scale;
            if (t < 0.5f)
            {
                // 0.8 → 1.1 (弾む)
                float t2 = t * 2f;
                scale = Mathf.Lerp(0.8f, 1.1f, 1f - (1f - t2) * (1f - t2));
            }
            else
            {
                // 1.1 → 1.0 (落ち着く)
                float t2 = (t - 0.5f) * 2f;
                scale = Mathf.Lerp(1.1f, 1.0f, t2 * t2);
            }
            birthResultCard.style.scale = new UIE.StyleScale(
                new UIE.Scale(new Vector3(scale, scale, 1f)));
            yield return null;
        }
        birthResultCard.style.scale = new UIE.StyleScale(
            new UIE.Scale(Vector3.one));
    }

    IEnumerator NamingButtonBounceAnimation()
    {
        if (gotoBattleButton == null) yield break;
        var rect = gotoBattleButton.GetComponent<RectTransform>();
        if (rect == null) yield break;

        float duration = 0.4f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float scale;
            if (t < 0.5f)
            {
                float t2 = t * 2f;
                scale = Mathf.Lerp(0f, 1.15f, 1f - (1f - t2) * (1f - t2));
            }
            else
            {
                float t2 = (t - 0.5f) * 2f;
                scale = Mathf.Lerp(1.15f, 1.0f, t2 * t2);
            }
            rect.localScale = new Vector3(scale, scale, 1f);
            yield return null;
        }
        rect.localScale = Vector3.one;
    }

    // CreateStatusTextBackground removed — status card serves as background

    void CreateIntroPanel()
    {
        if (canvas == null)
            canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null) return;

        // イントロパネルを作成（画面中央上部に配置）
        introPanel = new GameObject("IntroPanel");
        introPanel.transform.SetParent(canvas.transform, false);

        var panelRect = introPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = new Vector2(0, 100);
        panelRect.sizeDelta = new Vector2(800, 200);

        // テキスト
        var textObj = new GameObject("IntroText");
        textObj.transform.SetParent(introPanel.transform, false);
        var textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        introText = textObj.AddComponent<TextMeshProUGUI>();
        FontHelper.Apply(introText);
        introText.text = "";
        introText.fontSize = 36;
        introText.alignment = TextAlignmentOptions.Center;
        introText.color = Color.black;
        introText.raycastTarget = false;

        introPanel.SetActive(true);
    }

    void CreateFlashOverlay()
    {
        if (canvas == null) return;

        var flashObj = new GameObject("FlashOverlay");
        flashObj.transform.SetParent(canvas.transform, false);

        var rect = flashObj.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        flashOverlay = flashObj.AddComponent<Image>();
        flashOverlay.color = new Color(1f, 1f, 1f, 0f);
        flashOverlay.raycastTarget = false;
        flashObj.SetActive(false);
    }

    void CreateLightningOverlay()
    {
        if (canvas == null) return;

        lightningContainer = new GameObject("LightningContainer");
        lightningContainer.transform.SetParent(canvas.transform, false);

        var rect = lightningContainer.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        lightningContainer.SetActive(false);
    }

    void CreateNameInputUI()
    {
        nameInputOverlayEl = UIHelper.CreateOverlay();

        // 背景クリックで閉じる
        nameInputOverlayEl.pickingMode = UIE.PickingMode.Position;
        nameInputOverlayEl.RegisterCallback<UIE.ClickEvent>(evt =>
        {
            if (evt.target == nameInputOverlayEl)
                OnNameCancel();
        });

        var card = new UIE.VisualElement();
        card.AddToClassList("birth-name-card");

        var title = UIHelper.CreateLabel(Localization.Get("birth_name_input_title"), "birth-name-title");
        card.Add(title);

        nameTextField = new UIE.TextField();
        nameTextField.AddToClassList("birth-name-field");
        nameTextField.maxLength = 12;
        UIHelper.ApplyFont(nameTextField);
        card.Add(nameTextField);

        var btnRow = new UIE.VisualElement();
        btnRow.AddToClassList("birth-name-btn-row");

        var confirmBtn = UIHelper.CreatePillButton("決定", "birth-name-confirm-btn");
        confirmBtn.clicked += OnNameConfirmUIToolkit;
        btnRow.Add(confirmBtn);

        var backBtn = UIHelper.CreatePillButton(Localization.Get("ui_back"), "birth-name-back-btn");
        backBtn.clicked += OnNameCancel;
        btnRow.Add(backBtn);

        card.Add(btnRow);

        nameInputOverlayEl.Add(card);
        // Not added to overlayRoot yet — shown on demand
    }

    void OnNameCancel()
    {
        nameInputCancelled = true;
        waitingForNameInput = false;
    }

    void CreateSaveConfirmUI()
    {
        saveConfirmOverlayEl = UIHelper.CreateOverlay();

        var panel = new UIE.VisualElement();
        panel.AddToClassList("birth-save-panel");

        var title = UIHelper.CreateLabel(Localization.Get("birth_save_confirm"), "birth-save-title");
        panel.Add(title);

        var btnRow = new UIE.VisualElement();
        btnRow.AddToClassList("row");

        var yesBtn = new UIE.Button();
        yesBtn.AddToClassList("birth-save-yes");
        UIHelper.ApplyFont(yesBtn);
        yesBtn.text = Localization.Get("ui_yes");
        yesBtn.clicked += OnSaveYes;
        btnRow.Add(yesBtn);

        var noBtn = new UIE.Button();
        noBtn.AddToClassList("birth-save-no");
        UIHelper.ApplyFont(noBtn);
        noBtn.text = Localization.Get("ui_no");
        noBtn.clicked += OnSaveNo;
        btnRow.Add(noBtn);

        panel.Add(btnRow);
        saveConfirmOverlayEl.Add(panel);
        // Not added to overlayRoot yet — shown on demand
    }

    void SetButtonText(GameObject button, string text)
    {
        if (button == null) return;
        var tmp = button.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp != null) tmp.text = text;
    }

    void SetNextButtonText(string text)
    {
        if (nextButtonEl == null) return;
        var btn = UIE.UQueryExtensions.Q<UIE.Button>(nextButtonEl);
        if (btn != null) btn.text = text;
    }

    static Sprite _pachinkoButtonSprite;

    static Sprite GetPachinkoButtonSprite()
    {
        if (_pachinkoButtonSprite != null) return _pachinkoButtonSprite;

        int size = 512;
        float center = size / 2f;
        float outerR = size / 2f;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var pixels = new Color[size * size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - center;
                float dy = y - center;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                float normDist = dist / outerR;

                if (normDist > 1f)
                {
                    pixels[y * size + x] = new Color(0, 0, 0, 0);
                    continue;
                }

                // アンチエイリアス
                float aa = Mathf.Clamp01((outerR - dist) * 2f);

                // ゴールドメタリックリム（外周 92%-100%）
                if (normDist > 0.92f)
                {
                    float rimT = (normDist - 0.92f) / 0.08f;
                    float rimAngle = (dy / dist + 1f) * 0.5f;
                    float rimLight = 0.3f + 0.5f * rimAngle + 0.15f * Mathf.Pow(rimAngle, 4f);
                    Color rimColor = new Color(rimLight * 0.95f, rimLight * 0.78f, rimLight * 0.45f, aa);
                    pixels[y * size + x] = rimColor;
                    continue;
                }

                // ゴールド溝（88%-92%）
                if (normDist > 0.88f)
                {
                    float grooveT = (normDist - 0.88f) / 0.04f;
                    float grooveV = 0.15f + 0.05f * Mathf.Sin(grooveT * Mathf.PI);
                    pixels[y * size + x] = new Color(grooveV * 0.8f, grooveV * 0.6f, grooveV * 0.3f, aa);
                    continue;
                }

                // パステルピンクドーム本体（0%-88%）
                float bodyNorm = normDist / 0.88f;

                // 3Dドーム：中央が明るく、端が暗い（放物線的）
                float dome = 1f - bodyNorm * bodyNorm;

                // 上方向からの照明（yが上ほど明るい）
                float lightDir = (dy / (outerR * 0.88f) + 1f) * 0.5f;

                // ベースパステルピンク（#FFB7C5）にドーム陰影と方向照明を合成
                float baseR = 0.82f + 0.18f * dome * (0.6f + 0.4f * lightDir);
                float baseG = 0.50f + 0.22f * dome * (0.6f + 0.4f * lightDir);
                float baseB = 0.55f + 0.22f * dome * (0.6f + 0.4f * lightDir);

                // 上半分のスペキュラーハイライト（鏡面反射）
                float specX = dx / (outerR * 0.88f);
                float specY = (dy / (outerR * 0.88f)) - 0.3f; // 中心より少し上
                float specDist = Mathf.Sqrt(specX * specX * 1.8f + specY * specY * 3.5f);
                float specular = Mathf.Pow(Mathf.Clamp01(1f - specDist), 6f) * 0.7f;

                // 小さく強いハイライト点（左上）
                float hotX = (dx / (outerR * 0.88f)) + 0.25f;
                float hotY = (dy / (outerR * 0.88f)) - 0.25f;
                float hotDist = Mathf.Sqrt(hotX * hotX + hotY * hotY);
                float hotspot = Mathf.Pow(Mathf.Clamp01(1f - hotDist * 3.5f), 4f) * 0.5f;

                // 下部の環境反射（薄い）
                float envX = dx / (outerR * 0.88f);
                float envY = (dy / (outerR * 0.88f)) + 0.6f;
                float envDist = Mathf.Sqrt(envX * envX * 2f + envY * envY * 5f);
                float envReflect = Mathf.Pow(Mathf.Clamp01(1f - envDist), 3f) * 0.1f;

                float r = Mathf.Clamp01(baseR + specular + hotspot + envReflect);
                float g = Mathf.Clamp01(baseG + specular * 0.9f + hotspot * 0.95f + envReflect * 0.8f);
                float b = Mathf.Clamp01(baseB + specular * 0.85f + hotspot * 0.9f + envReflect * 0.7f);

                pixels[y * size + x] = new Color(r, g, b, aa);
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        tex.filterMode = FilterMode.Bilinear;
        _pachinkoButtonSprite = Sprite.Create(tex, new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f), 100);
        return _pachinkoButtonSprite;
    }

    void StylePachinkoButton(GameObject button)
    {
        if (button == null) return;

        float size = 340f;
        var rect = button.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.sizeDelta = new Vector2(size, size);
            rect.anchoredPosition = new Vector2(0, -400);
        }

        // 既存の子オブジェクトを削除
        for (int i = button.transform.childCount - 1; i >= 0; i--)
            Destroy(button.transform.GetChild(i).gameObject);

        // ── 外側のグローオーラ（ソフトピンク） ──
        var glowObj = new GameObject("Glow");
        glowObj.transform.SetParent(button.transform, false);
        glowObj.transform.SetAsFirstSibling();
        var glowRect = glowObj.AddComponent<RectTransform>();
        glowRect.anchorMin = new Vector2(0.5f, 0.5f);
        glowRect.anchorMax = new Vector2(0.5f, 0.5f);
        glowRect.sizeDelta = new Vector2(size + 100, size + 100);
        var glowImg = glowObj.AddComponent<Image>();
        glowImg.sprite = GetCircleSprite(128);
        glowImg.color = new Color(1f, 0.72f, 0.78f, 0.30f);
        glowImg.raycastTarget = false;

        // ── 回転する光線（放射状） ──
        var raysObj = new GameObject("Rays");
        raysObj.transform.SetParent(button.transform, false);
        raysObj.transform.SetAsFirstSibling();
        var raysRect = raysObj.AddComponent<RectTransform>();
        raysRect.anchorMin = new Vector2(0.5f, 0.5f);
        raysRect.anchorMax = new Vector2(0.5f, 0.5f);
        raysRect.sizeDelta = new Vector2(size + 200, size + 200);
        raysRect.localRotation = Quaternion.identity;
        for (int i = 0; i < 12; i++)
        {
            var rayObj = new GameObject($"Ray{i}");
            rayObj.transform.SetParent(raysObj.transform, false);
            var rayRect = rayObj.AddComponent<RectTransform>();
            rayRect.anchorMin = new Vector2(0.5f, 0.5f);
            rayRect.anchorMax = new Vector2(0.5f, 0.5f);
            rayRect.sizeDelta = new Vector2(12, size + 160);
            rayRect.localRotation = Quaternion.Euler(0, 0, i * 30f);
            var rayImg = rayObj.AddComponent<Image>();
            rayImg.color = new Color(1f, 0.85f, 0.65f, 0.08f);
            rayImg.raycastTarget = false;
        }

        // ── ボタン台座（下に影を含むベース） ──
        var baseObj = new GameObject("Base");
        baseObj.transform.SetParent(button.transform, false);
        var baseRect = baseObj.AddComponent<RectTransform>();
        baseRect.anchorMin = new Vector2(0.5f, 0.5f);
        baseRect.anchorMax = new Vector2(0.5f, 0.5f);
        baseRect.sizeDelta = new Vector2(size + 20, size + 20);
        baseRect.anchoredPosition = new Vector2(0, -6);
        var baseImg = baseObj.AddComponent<Image>();
        baseImg.sprite = GetCircleSprite(128);
        baseImg.color = new Color(0.55f, 0.38f, 0.25f, 0.45f);
        baseImg.raycastTarget = false;

        // ── 3Dドームボタン本体（プロシージャル生成） ──
        var domeObj = new GameObject("Dome");
        domeObj.transform.SetParent(button.transform, false);
        var domeRect = domeObj.AddComponent<RectTransform>();
        domeRect.anchorMin = new Vector2(0.5f, 0.5f);
        domeRect.anchorMax = new Vector2(0.5f, 0.5f);
        domeRect.sizeDelta = new Vector2(size, size);
        var domeImg = domeObj.AddComponent<Image>();
        domeImg.sprite = GetPachinkoButtonSprite();
        domeImg.raycastTarget = false;

        // ── PUSHロゴ（ボタン中央） ──
        var pushSprite = Resources.Load<Sprite>("UI/push-logo");
        if (pushSprite != null)
        {
            var logoObj = new GameObject("PushLogo");
            logoObj.transform.SetParent(button.transform, false);
            var logoRect = logoObj.AddComponent<RectTransform>();
            logoRect.anchorMin = new Vector2(0.5f, 0.5f);
            logoRect.anchorMax = new Vector2(0.5f, 0.5f);
            // ボタンから大きくはみ出るサイズ（幅はボタンの150%）
            float logoW = size * 1.5f;
            float logoH = logoW * (612f / 1280f);
            logoRect.sizeDelta = new Vector2(logoW, logoH);
            logoRect.anchoredPosition = Vector2.zero;
            var logoImg = logoObj.AddComponent<Image>();
            logoImg.sprite = pushSprite;
            logoImg.preserveAspect = true;
            logoImg.raycastTarget = false;
        }

        // ── きらめきパーティクル（4つの小さな星、パステルカラー） ──
        Color[] sparkleColors = {
            new Color(1f, 0.72f, 0.77f, 0.8f),   // パステルピンク (#FFB7C5)
            new Color(0.67f, 0.94f, 0.82f, 0.8f), // ミント (#AAF0D1)
            new Color(0.97f, 0.91f, 0.81f, 0.8f), // メイン (#F7E7CE)
            new Color(1f, 1f, 1f, 0.8f),           // ホワイト
        };
        for (int i = 0; i < 4; i++)
        {
            var sparkObj = new GameObject($"Sparkle{i}");
            sparkObj.transform.SetParent(button.transform, false);
            var spRect = sparkObj.AddComponent<RectTransform>();
            spRect.anchorMin = new Vector2(0.5f, 0.5f);
            spRect.anchorMax = new Vector2(0.5f, 0.5f);
            spRect.sizeDelta = new Vector2(14, 14);
            float angle = i * 90f * Mathf.Deg2Rad;
            float dist = size * 0.55f;
            spRect.anchoredPosition = new Vector2(Mathf.Cos(angle) * dist, Mathf.Sin(angle) * dist);
            var spImg = sparkObj.AddComponent<Image>();
            spImg.sprite = GetCircleSprite(128);
            spImg.color = sparkleColors[i];
            spImg.raycastTarget = false;
        }

        // Button設定（ボタン自体のImageを透明だがraycast有効にし、全面クリック可能に）
        var img = button.GetComponent<Image>();
        if (img != null)
        {
            img.sprite = null;
            img.color = new Color(0, 0, 0, 0);
            img.raycastTarget = true;
        }
        var btn = button.GetComponent<Button>();
        if (btn != null)
        {
            btn.targetGraphic = img;
            btn.navigation = new Navigation { mode = Navigation.Mode.None };
            var colors = btn.colors;
            colors.normalColor = new Color(0, 0, 0, 0);
            colors.highlightedColor = new Color(0, 0, 0, 0);
            colors.pressedColor = new Color(0, 0, 0, 0);
            colors.selectedColor = new Color(0, 0, 0, 0);
            btn.colors = colors;
        }

        // 押下時スケール + 押し込みアニメーション
        var trigger = button.GetComponent<EventTrigger>();
        if (trigger == null) trigger = button.AddComponent<EventTrigger>();
        trigger.triggers.Clear();

        var pointerDown = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
        pointerDown.callback.AddListener((data) => {
            button.transform.localScale = new Vector3(0.92f, 0.92f, 1f);
        });
        trigger.triggers.Add(pointerDown);

        var pointerUp = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
        pointerUp.callback.AddListener((data) => {
            button.transform.localScale = Vector3.one;
        });
        trigger.triggers.Add(pointerUp);

        var pointerExit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        pointerExit.callback.AddListener((data) => {
            button.transform.localScale = Vector3.one;
        });
        trigger.triggers.Add(pointerExit);

        // エフェクトアニメーション用コンポーネント追加
        var effect = button.GetComponent<PachinkoButtonEffect>();
        if (effect == null) effect = button.AddComponent<PachinkoButtonEffect>();
    }

    void StylePillButton(GameObject button, float width, float height, string label, int fontSize)
    {
        if (button == null) return;

        int pillRadius = (int)(height / 2);

        var rect = button.GetComponent<RectTransform>();
        if (rect != null)
            rect.sizeDelta = new Vector2(width, height);

        // 既存の子オブジェクトを削除
        for (int i = button.transform.childCount - 1; i >= 0; i--)
            Destroy(button.transform.GetChild(i).gameObject);

        // 背景Image: pill shape, 白背景
        var img = button.GetComponent<Image>();
        if (img != null)
        {
            img.enabled = true;
            img.sprite = GetPillSprite(pillRadius);
            img.type = Image.Type.Sliced;
            img.color = Color.white;
        }

        // Box shadow（ボタンの子要素、中央透明で外周のみ影）
        int blur = 20;
        var shadowObj = new GameObject("Shadow");
        shadowObj.transform.SetParent(button.transform, false);
        shadowObj.transform.SetAsFirstSibling();
        var shadowRect = shadowObj.AddComponent<RectTransform>();
        shadowRect.anchorMin = Vector2.zero;
        shadowRect.anchorMax = Vector2.one;
        shadowRect.offsetMin = new Vector2(-blur, -blur - 4);
        shadowRect.offsetMax = new Vector2(blur, blur - 4);
        var shadowImg = shadowObj.AddComponent<Image>();
        shadowImg.sprite = GetShadowSprite(pillRadius, blur);
        shadowImg.type = Image.Type.Sliced;
        shadowImg.color = new Color(0f, 0f, 0f, 0.18f);
        shadowImg.raycastTarget = false;

        // テキスト
        var textObj = new GameObject("Text");
        textObj.transform.SetParent(button.transform, false);
        var textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        var tmp = textObj.AddComponent<TextMeshProUGUI>();
        FontHelper.Apply(tmp);
        tmp.text = label;
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(0.45f, 0.45f, 0.5f);
        tmp.fontStyle = FontStyles.Bold;
        tmp.raycastTarget = false;

        // Button設定
        var btn = button.GetComponent<Button>();
        if (btn != null)
        {
            btn.targetGraphic = img;
            btn.navigation = new Navigation { mode = Navigation.Mode.None };
            var colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = Color.white;
            colors.pressedColor = new Color(0.92f, 0.92f, 0.92f);
            colors.selectedColor = Color.white;
            colors.fadeDuration = 0.08f;
            btn.colors = colors;
        }

        // 押下時スケールアニメーション
        var trigger = button.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = button.AddComponent<EventTrigger>();
        trigger.triggers.Clear();

        var pointerDown = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
        pointerDown.callback.AddListener((data) => {
            button.transform.localScale = new Vector3(0.95f, 0.95f, 1f);
        });
        trigger.triggers.Add(pointerDown);

        var pointerUp = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
        pointerUp.callback.AddListener((data) => {
            button.transform.localScale = Vector3.one;
        });
        trigger.triggers.Add(pointerUp);

        var pointerExit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        pointerExit.callback.AddListener((data) => {
            button.transform.localScale = Vector3.one;
        });
        trigger.triggers.Add(pointerExit);
    }

    // ===== キャラクターリストUI =====

    void CreateCharacterListUI()
    {
        if (canvas == null) return;

        var listPanel = new GameObject("CharacterList");
        listPanel.transform.SetParent(canvas.transform, false);

        var listRect = listPanel.AddComponent<RectTransform>();
        listRect.anchorMin = new Vector2(0, 0);
        listRect.anchorMax = new Vector2(1, 0);
        listRect.pivot = new Vector2(0.5f, 0);
        listRect.anchoredPosition = new Vector2(0, 5);
        listRect.sizeDelta = new Vector2(-20, 110);

        var listBg = listPanel.AddComponent<Image>();
        listBg.color = new Color(0.1f, 0.1f, 0.15f, 0.7f);
        listBg.raycastTarget = false;

        // 父親行
        CreateCharacterRow(listPanel.transform, Localization.Get("label_fathers"), NewFathers, fatherSprites, 25);
        // 母親行
        CreateCharacterRow(listPanel.transform, Localization.Get("label_mothers"), NewMothers, motherSprites, -25);
    }

    void CreateCharacterRow(Transform parent, string label, ParentData[] parents, Sprite[] sprites, float yCards)
    {
        // ラベル
        var labelObj = new GameObject(label + "Label");
        labelObj.transform.SetParent(parent, false);
        var labelRect = labelObj.AddComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0, 0.5f);
        labelRect.anchorMax = new Vector2(0, 0.5f);
        labelRect.anchoredPosition = new Vector2(40, yCards);
        labelRect.sizeDelta = new Vector2(50, 24);
        var labelText = labelObj.AddComponent<TextMeshProUGUI>();
        FontHelper.Apply(labelText);
        labelText.text = label;
        labelText.fontSize = 14;
        labelText.alignment = TextAlignmentOptions.Center;
        labelText.color = new Color(1f, 1f, 1f, 0.8f);
        labelText.fontStyle = FontStyles.Bold;
        labelText.raycastTarget = false;

        // キャラカード6個
        float startX = 100;
        float spacing = 120;
        for (int i = 0; i < parents.Length; i++)
        {
            float x = startX + i * spacing;
            CreateMiniCard(parent, x, yCards, parents[i], sprites, i);
        }
    }

    void CreateMiniCard(Transform parent, float x, float y, ParentData data, Sprite[] sprites, int idx)
    {
        var card = new GameObject("Mini_" + data.name);
        card.transform.SetParent(parent, false);
        var cardRect = card.AddComponent<RectTransform>();
        cardRect.anchorMin = new Vector2(0, 0.5f);
        cardRect.anchorMax = new Vector2(0, 0.5f);
        cardRect.anchoredPosition = new Vector2(x, y);
        cardRect.sizeDelta = new Vector2(110, 45);

        // 顔画像
        var faceObj = new GameObject("Face");
        faceObj.transform.SetParent(card.transform, false);
        var faceRect = faceObj.AddComponent<RectTransform>();
        faceRect.anchorMin = new Vector2(0, 0.5f);
        faceRect.anchorMax = new Vector2(0, 0.5f);
        faceRect.anchoredPosition = new Vector2(18, 0);
        faceRect.sizeDelta = new Vector2(36, 36);
        var faceImg = faceObj.AddComponent<Image>();
        faceImg.raycastTarget = false;

        Sprite sp = Resources.Load<Sprite>($"Parents/{data.imageName}");
        if (sp != null)
        {
            faceImg.sprite = sp;
            faceImg.color = Color.white;
        }
        else
        {
            faceImg.color = data.faceColor;
        }

        // 名前
        var nameObj = new GameObject("Name");
        nameObj.transform.SetParent(card.transform, false);
        var nameRect = nameObj.AddComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0, 0.5f);
        nameRect.anchorMax = new Vector2(1, 0.5f);
        nameRect.anchoredPosition = new Vector2(15, 0);
        nameRect.sizeDelta = new Vector2(0, 20);
        var nameText = nameObj.AddComponent<TextMeshProUGUI>();
        FontHelper.Apply(nameText);
        nameText.text = Localization.GetParent(data.name);
        nameText.fontSize = 12;
        nameText.alignment = TextAlignmentOptions.Center;
        nameText.color = new Color(1f, 1f, 1f, 0.9f);
        nameText.raycastTarget = false;
    }

    // ===== 恋愛ストーリーUI =====

    void CreateStoryUI()
    {
        if (overlayRoot == null)
        {
            Debug.LogError("[BirthSystem] CreateStoryUI: overlayRoot is null!");
            return;
        }

        Debug.Log("[BirthSystem] CreateStoryUI: creating story panel on overlayRoot");
        storyPanel = new UIE.VisualElement();
        storyPanel.AddToClassList("birth-story-panel");

        // 背景画像（上下反転・薄く重ねる）
        var loveBgSprite = Resources.Load<Sprite>("BackGrounds/love-background");
        if (loveBgSprite != null)
        {
            var bgWrapper = new UIE.VisualElement();
            bgWrapper.AddToClassList("birth-story-bg-wrapper");
            bgWrapper.style.backgroundImage = new UIE.StyleBackground(loveBgSprite);
            storyPanel.Add(bgWrapper);
        }

        // タイトル
        var titleLabel = UIHelper.CreateLabel(Localization.Get("birth_story_title"));
        titleLabel.AddToClassList("birth-story-title");
        storyPanel.Add(titleLabel);

        // アルバムコンテンツ（2カラム: 父ポラロイド | 母ポラロイド）
        var albumContent = new UIE.VisualElement();
        albumContent.AddToClassList("birth-story-album-content");
        storyAlbumContent = albumContent;

        // 左: 父親ポラロイド
        CreateStoryParentCard(albumContent, true,
            out storyFatherFace, out storyFatherName, out storyFatherIntro);

        // 右: 母親ポラロイド
        CreateStoryParentCard(albumContent, false,
            out storyMotherFace, out storyMotherName, out storyMotherIntro);

        storyPanel.Add(albumContent);

        // 装飾線
        var divider = new UIE.VisualElement();
        divider.AddToClassList("birth-story-album-divider");
        storyPanel.Add(divider);

        // ストーリーテキスト（不透明背景コンテナでZ重なり解消）
        var storyTextContainer = new UIE.VisualElement();
        storyTextContainer.AddToClassList("birth-story-text-container");
        storyText = UIHelper.CreateLabel("");
        storyText.AddToClassList("birth-story-text");
        storyTextContainer.Add(storyText);
        storyPanel.Add(storyTextContainer);

        // 「愛を育む」ボタン行
        var loveBtnRow = new UIE.VisualElement();
        loveBtnRow.AddToClassList("birth-story-love-btn-row");

        var loveBtn = new UIE.Button();
        loveBtn.AddToClassList("birth-story-love-btn");
        UIHelper.ApplyFont(loveBtn);
        loveBtn.text = Localization.Get("birth_nurture_love");
        loveBtn.clicked += OnStoryTap;

        // ピンクグラデーションオーバーレイ（下半分に微かなピンク）
        var gradientOverlay = new UIE.VisualElement();
        gradientOverlay.style.position = UIE.Position.Absolute;
        gradientOverlay.style.left = 0;
        gradientOverlay.style.right = 0;
        gradientOverlay.style.top = new UIE.StyleLength(new UIE.Length(50, UIE.LengthUnit.Percent));
        gradientOverlay.style.bottom = 0;
        gradientOverlay.style.backgroundColor = new Color(1f, 0.718f, 0.773f, 0.15f);
        gradientOverlay.style.borderBottomLeftRadius = 50;
        gradientOverlay.style.borderBottomRightRadius = 50;
        gradientOverlay.pickingMode = UIE.PickingMode.Ignore;
        loveBtn.Add(gradientOverlay);

        loveBtnRow.Add(loveBtn);

        storyPanel.Add(loveBtnRow);

        storyPanel.style.display = UIE.DisplayStyle.None;
        overlayRoot.Add(storyPanel);
    }

    void OnStoryTap()
    {
        StartCoroutine(StoryWhiteoutThenProceed());
    }

    IEnumerator StoryWhiteoutThenProceed()
    {
        // フェードイン（warm white）
        if (flashOverlay != null)
        {
            flashOverlay.gameObject.SetActive(true);
            float fadeInDur = 0.4f;
            float elapsed = 0f;
            while (elapsed < fadeInDur)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / fadeInDur);
                float easeT = t * t;
                flashOverlay.color = new Color(1f, 0.99f, 0.97f, easeT);
                yield return null;
            }
            flashOverlay.color = new Color(1f, 0.99f, 0.97f, 1f);
        }

        // ホワイト中にストーリーパネルを閉じる
        waitingForStoryConfirm = false;

        // ホールド
        yield return new WaitForSeconds(0.2f);

        // フェードアウト
        if (flashOverlay != null)
        {
            float fadeOutDur = 0.5f;
            float elapsed2 = 0f;
            while (elapsed2 < fadeOutDur)
            {
                elapsed2 += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed2 / fadeOutDur);
                flashOverlay.color = new Color(1f, 0.99f, 0.97f, 1f - t);
                yield return null;
            }
            flashOverlay.gameObject.SetActive(false);
        }
    }

    void CreateStoryParentCard(UIE.VisualElement parent, bool isFather,
        out UIE.VisualElement faceEl, out UIE.Label nameLabel, out UIE.Label introLabel)
    {
        var card = new UIE.VisualElement();
        card.AddToClassList("birth-story-album-card");
        card.AddToClassList(isFather ? "birth-story-album-card-father" : "birth-story-album-card-mother");

        // ドロップシャドウ（立体感）
        var shadow = new UIE.VisualElement();
        shadow.AddToClassList("birth-story-polaroid-shadow");
        shadow.pickingMode = UIE.PickingMode.Ignore;
        card.Add(shadow);

        // ポラロイドフレーム（父は左傾き、母は右傾き）— ボタンに変更（タップでツールチップ表示）
        var polaroidFrame = new UIE.Button();
        polaroidFrame.AddToClassList("birth-story-polaroid-frame");
        float rotation = isFather ? -3f : 3f;
        polaroidFrame.style.rotate = new UIE.StyleRotate(
            new UIE.Rotate(new UIE.Angle(rotation, UIE.AngleUnit.Degree)));

        // 写真エリア
        var polaroidInner = new UIE.VisualElement();
        polaroidInner.AddToClassList("birth-story-polaroid-inner");

        faceEl = new UIE.VisualElement();
        faceEl.AddToClassList("birth-story-polaroid-image");
        polaroidInner.Add(faceEl);

        polaroidFrame.Add(polaroidInner);

        // キャプション（名前）
        nameLabel = UIHelper.CreateLabel("");
        nameLabel.AddToClassList("birth-story-polaroid-caption");
        polaroidFrame.Add(nameLabel);

        card.Add(polaroidFrame);

        // コーナーシール（4隅、パーセント指定で配置）
        AddStoryCornerSeal(card, 16f, 0f);   // top-left
        AddStoryCornerSeal(card, 77f, 0f);   // top-right
        AddStoryCornerSeal(card, 16f, 85f);  // bottom-left
        AddStoryCornerSeal(card, 77f, 85f);  // bottom-right

        // 肩書き（短いタイトルのみ、"/" の前の部分）
        introLabel = UIHelper.CreateLabel("");
        introLabel.AddToClassList("birth-story-card-intro");
        card.Add(introLabel);

        // 写真タップでふきだしツールチップ表示
        var introRef = introLabel; // outパラメータはラムダ内で使えないためローカル変数にコピー
        polaroidFrame.clicked += () =>
        {
            string fullIntro = introRef.userData as string;
            if (!string.IsNullOrEmpty(fullIntro))
                ShowStoryTooltip(card, fullIntro, isFather);
        };

        parent.Add(card);
    }

    void AddStoryCornerSeal(UIE.VisualElement parent, float xPercent, float yPercent)
    {
        var seal = new UIE.VisualElement();
        seal.AddToClassList("birth-story-corner-seal");
        seal.style.left = new UIE.StyleLength(new UIE.Length(xPercent, UIE.LengthUnit.Percent));
        seal.style.top = new UIE.StyleLength(new UIE.Length(yPercent, UIE.LengthUnit.Percent));
        seal.style.rotate = new UIE.StyleRotate(
            new UIE.Rotate(new UIE.Angle(45f, UIE.AngleUnit.Degree)));
        parent.Add(seal);
    }

    void ShowStoryTooltip(UIE.VisualElement anchorCard, string text, bool isFather)
    {
        // 既存のツールチップがあれば消す
        if (activeStoryTooltip != null)
        {
            activeStoryTooltip.RemoveFromHierarchy();
            activeStoryTooltip = null;
            return; // トグル: 同じ写真を再タップで閉じる
        }

        if (storyPanel == null) return;

        // ふきだしコンテナ（storyPanel上にabsolute配置）
        var tooltip = new UIE.VisualElement();
        tooltip.AddToClassList("birth-story-tooltip");
        // 父は左寄り、母は右寄り
        if (isFather)
            tooltip.AddToClassList("birth-story-tooltip-left");
        else
            tooltip.AddToClassList("birth-story-tooltip-right");

        var tooltipText = UIHelper.CreateLabel(text);
        tooltipText.AddToClassList("birth-story-tooltip-text");
        UIHelper.ApplyFont(tooltipText);
        tooltip.Add(tooltipText);

        // タップで閉じるボタン
        var closeBtn = new UIE.Button();
        closeBtn.AddToClassList("birth-story-tooltip-close");
        UIHelper.ApplyFont(closeBtn);
        closeBtn.text = "✕";
        closeBtn.clicked += () =>
        {
            if (activeStoryTooltip != null)
            {
                activeStoryTooltip.RemoveFromHierarchy();
                activeStoryTooltip = null;
            }
        };
        tooltip.Add(closeBtn);

        storyPanel.Add(tooltip);
        activeStoryTooltip = tooltip;

        // 2秒後に自動で消える
        StartCoroutine(AutoDismissTooltip(tooltip, 3f));
    }

    IEnumerator AutoDismissTooltip(UIE.VisualElement tooltip, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (activeStoryTooltip == tooltip && tooltip.parent != null)
        {
            tooltip.RemoveFromHierarchy();
            activeStoryTooltip = null;
        }
    }

    IEnumerator ShowParentCutin(string parentName)
    {
        string cutinText = Localization.GetParentCutin(parentName);
        if (string.IsNullOrEmpty(cutinText)) yield break;
        yield return StartCoroutine(ShowCutinText(cutinText));
    }

    IEnumerator ShowCutinText(string cutinText)
    {
        if (overlayRoot == null || string.IsNullOrEmpty(cutinText)) yield break;

        // テーマカラー
        Color bandColor = new Color(0.969f, 0.906f, 0.808f); // #F7E7CE
        Color textColor = new Color(0.051f, 0.051f, 0.078f); // #0d0d14

        // ── 帯（画面中央の横帯） ──
        var band = new UIE.VisualElement();
        band.style.position = UIE.Position.Absolute;
        band.style.left = 0;
        band.style.right = 0;
        band.style.top = new UIE.StyleLength(new UIE.Length(38, UIE.LengthUnit.Percent));
        band.style.bottom = new UIE.StyleLength(new UIE.Length(38, UIE.LengthUnit.Percent));
        band.style.backgroundColor = new Color(bandColor.r, bandColor.g, bandColor.b, 0);
        band.style.alignItems = UIE.Align.Center;
        band.style.justifyContent = UIE.Justify.Center;
        overlayRoot.Add(band);

        // ── テキスト ──
        var label = UIHelper.CreateLabel(cutinText);
        UIHelper.ApplyFontBold(label);
        label.style.fontSize = 52;
        label.style.color = new Color(textColor.r, textColor.g, textColor.b, 0);
        label.style.whiteSpace = UIE.WhiteSpace.NoWrap;
        label.style.paddingLeft = 40;
        label.style.paddingRight = 40;
        band.Add(label);

        // ── アニメーション: フェードイン + スライド ──
        float slideOffset = 300f;
        float fadeInDuration = 0.25f;
        float holdDuration = 1.2f;
        float fadeOutDuration = 0.3f;

        float elapsed = 0;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeInDuration);
            float easeT = 1f - (1f - t) * (1f - t);
            band.style.backgroundColor = new Color(bandColor.r, bandColor.g, bandColor.b, 0.9f * easeT);
            label.style.color = new Color(textColor.r, textColor.g, textColor.b, easeT);
            label.style.translate = new UIE.StyleTranslate(
                new UIE.Translate(slideOffset * (1f - easeT), 0));
            yield return null;
        }
        band.style.backgroundColor = new Color(bandColor.r, bandColor.g, bandColor.b, 0.9f);
        label.style.color = textColor;
        label.style.translate = new UIE.StyleTranslate(new UIE.Translate(0, 0));

        yield return new WaitForSeconds(holdDuration);

        elapsed = 0;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeOutDuration);
            band.style.backgroundColor = new Color(bandColor.r, bandColor.g, bandColor.b, 0.9f * (1f - t));
            label.style.color = new Color(textColor.r, textColor.g, textColor.b, 1f - t);
            yield return null;
        }

        band.RemoveFromHierarchy();
    }

    IEnumerator ShowMotherCutinText(string cutinText)
    {
        if (overlayRoot == null || string.IsNullOrEmpty(cutinText)) yield break;

        // テーマカラー
        Color bandColor = new Color(0.969f, 0.906f, 0.808f); // #F7E7CE
        Color textColor = new Color(0.051f, 0.051f, 0.078f); // #0d0d14

        // ── 帯（画面中央の横帯） ──
        var band = new UIE.VisualElement();
        band.style.position = UIE.Position.Absolute;
        band.style.left = 0;
        band.style.right = 0;
        band.style.top = new UIE.StyleLength(new UIE.Length(38, UIE.LengthUnit.Percent));
        band.style.bottom = new UIE.StyleLength(new UIE.Length(38, UIE.LengthUnit.Percent));
        band.style.backgroundColor = new Color(bandColor.r, bandColor.g, bandColor.b, 0f);
        band.style.alignItems = UIE.Align.Center;
        band.style.justifyContent = UIE.Justify.Center;
        overlayRoot.Add(band);

        // ── テキスト（太字） ──
        var label = UIHelper.CreateLabel(cutinText);
        UIHelper.ApplyFontBold(label);
        label.style.fontSize = 52;
        label.style.color = new Color(textColor.r, textColor.g, textColor.b, 0f);
        label.style.whiteSpace = UIE.WhiteSpace.NoWrap;
        label.style.paddingLeft = 40;
        label.style.paddingRight = 40;
        band.Add(label);

        // ── アニメーション: じわ〜っとフェードイン + スケールアップ ──
        float fadeInDuration = 0.8f;
        float holdDuration = 1.2f;
        float fadeOutDuration = 0.5f;
        float scaleFrom = 0.6f;
        float scaleTo = 1.0f;

        float elapsed = 0;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeInDuration);
            float easeT = 1f - (1f - t) * (1f - t) * (1f - t);
            float s = Mathf.Lerp(scaleFrom, scaleTo, easeT);
            band.style.backgroundColor = new Color(bandColor.r, bandColor.g, bandColor.b, 0.9f * easeT);
            label.style.color = new Color(textColor.r, textColor.g, textColor.b, easeT);
            label.style.scale = new UIE.StyleScale(new UIE.Scale(new Vector3(s, s, 1f)));
            yield return null;
        }
        band.style.backgroundColor = new Color(bandColor.r, bandColor.g, bandColor.b, 0.9f);
        label.style.color = textColor;
        label.style.scale = new UIE.StyleScale(new UIE.Scale(Vector3.one));

        yield return new WaitForSeconds(holdDuration);

        elapsed = 0;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeOutDuration);
            float easeT = t * t;
            float s = Mathf.Lerp(1.0f, 1.05f, easeT);
            band.style.backgroundColor = new Color(bandColor.r, bandColor.g, bandColor.b, 0.9f * (1f - easeT));
            label.style.color = new Color(textColor.r, textColor.g, textColor.b, 1f - easeT);
            label.style.scale = new UIE.StyleScale(new UIE.Scale(new Vector3(s, s, 1f)));
            yield return null;
        }

        band.RemoveFromHierarchy();
    }

    // ===== 誕生アナウンス =====

    IEnumerator ShowBirthAnnouncement(ParentData father, ParentData mother)
    {
        if (overlayRoot == null) yield break;

        var panel = new UIE.VisualElement();
        panel.style.position = UIE.Position.Absolute;
        panel.style.left = 0; panel.style.right = 0;
        panel.style.top = 0; panel.style.bottom = 0;
        panel.style.backgroundColor = new Color(0.969f, 0.906f, 0.808f, 0f);
        panel.style.alignItems = UIE.Align.Center;
        panel.style.justifyContent = UIE.Justify.Center;
        overlayRoot.Add(panel);

        // コンテンツラッパー（opacity で一括フェード）
        var content = new UIE.VisualElement();
        content.style.alignItems = UIE.Align.Center;
        content.style.opacity = 0f;
        panel.Add(content);

        // 親画像行: [父] ♥ [母]
        var faceRow = new UIE.VisualElement();
        faceRow.style.flexDirection = UIE.FlexDirection.Row;
        faceRow.style.alignItems = UIE.Align.Center;
        faceRow.style.justifyContent = UIE.Justify.Center;
        faceRow.style.marginBottom = 32;
        content.Add(faceRow);

        // 父画像（丸）
        var fatherFace = new UIE.VisualElement();
        fatherFace.style.width = 300;
        fatherFace.style.height = 300;
        fatherFace.style.borderTopLeftRadius = 150;
        fatherFace.style.borderTopRightRadius = 150;
        fatherFace.style.borderBottomLeftRadius = 150;
        fatherFace.style.borderBottomRightRadius = 150;
        fatherFace.style.borderTopWidth = 5;
        fatherFace.style.borderBottomWidth = 5;
        fatherFace.style.borderLeftWidth = 5;
        fatherFace.style.borderRightWidth = 5;
        fatherFace.style.borderTopColor = Color.white;
        fatherFace.style.borderBottomColor = Color.white;
        fatherFace.style.borderLeftColor = Color.white;
        fatherFace.style.borderRightColor = Color.white;
        fatherFace.style.overflow = UIE.Overflow.Hidden;
        Sprite fatherSp = Resources.Load<Sprite>($"Parents/{father.imageName}");
        if (fatherSp != null)
            fatherFace.style.backgroundImage = new UIE.StyleBackground(fatherSp);
        else
            fatherFace.style.backgroundColor = father.faceColor;
        faceRow.Add(fatherFace);

        // ハートマーク
        var heart = UIHelper.CreateLabel("\u2665");
        UIHelper.ApplyFontBold(heart);
        heart.style.fontSize = 80;
        heart.style.color = new Color(1f, 0.718f, 0.773f); // #FFB7C5 サブカラー
        heart.style.unityTextAlign = TextAnchor.MiddleCenter;
        heart.style.marginLeft = 24;
        heart.style.marginRight = 24;
        faceRow.Add(heart);

        // 母画像（丸）
        var motherFace = new UIE.VisualElement();
        motherFace.style.width = 300;
        motherFace.style.height = 300;
        motherFace.style.borderTopLeftRadius = 150;
        motherFace.style.borderTopRightRadius = 150;
        motherFace.style.borderBottomLeftRadius = 150;
        motherFace.style.borderBottomRightRadius = 150;
        motherFace.style.borderTopWidth = 5;
        motherFace.style.borderBottomWidth = 5;
        motherFace.style.borderLeftWidth = 5;
        motherFace.style.borderRightWidth = 5;
        motherFace.style.borderTopColor = Color.white;
        motherFace.style.borderBottomColor = Color.white;
        motherFace.style.borderLeftColor = Color.white;
        motherFace.style.borderRightColor = Color.white;
        motherFace.style.overflow = UIE.Overflow.Hidden;
        Sprite motherSp = Resources.Load<Sprite>($"Parents/{mother.imageName}");
        if (motherSp != null)
            motherFace.style.backgroundImage = new UIE.StyleBackground(motherSp);
        else
            motherFace.style.backgroundColor = mother.faceColor;
        faceRow.Add(motherFace);

        // テキスト
        string fatherDisplay = Localization.GetParent(father.name);
        string motherDisplay = Localization.GetParent(mother.name);
        var label = UIHelper.CreateLabel($"{fatherDisplay} と {motherDisplay} の\nあかちゃんが たんじょうした！");
        UIHelper.ApplyFontBold(label);
        label.style.fontSize = 40;
        label.style.color = new Color(0.051f, 0.051f, 0.078f);
        label.style.unityTextAlign = TextAnchor.MiddleCenter;
        label.style.whiteSpace = UIE.WhiteSpace.Normal;
        label.style.width = 800;
        content.Add(label);

        // フェードイン（背景 + コンテンツ）
        float elapsed = 0f;
        while (elapsed < 0.6f)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / 0.6f);
            panel.style.backgroundColor = new Color(0.969f, 0.906f, 0.808f, t);
            content.style.opacity = t;
            yield return null;
        }
        panel.style.backgroundColor = new Color(0.969f, 0.906f, 0.808f, 1f);
        content.style.opacity = 1f;

        // ハートバウンスアニメーション
        float bounceElapsed = 0f;
        float bounceDur = 0.4f;
        while (bounceElapsed < bounceDur)
        {
            bounceElapsed += Time.deltaTime;
            float bt = Mathf.Clamp01(bounceElapsed / bounceDur);
            float s;
            if (bt < 0.5f)
                s = Mathf.Lerp(1f, 1.4f, bt * 2f);
            else
                s = Mathf.Lerp(1.4f, 1f, (bt - 0.5f) * 2f);
            heart.style.scale = new UIE.StyleScale(new UIE.Scale(new Vector2(s, s)));
            yield return null;
        }
        heart.style.scale = new UIE.StyleScale(new UIE.Scale(Vector2.one));

        yield return new WaitForSeconds(1.2f);

        // フェードアウト
        elapsed = 0f;
        while (elapsed < 0.5f)
        {
            elapsed += Time.deltaTime;
            float t = 1f - Mathf.Clamp01(elapsed / 0.5f);
            panel.style.backgroundColor = new Color(0.969f, 0.906f, 0.808f, t);
            content.style.opacity = t;
            yield return null;
        }

        panel.RemoveFromHierarchy();
    }

    // ===== 運命カットイン: 光の引力演出 =====

    IEnumerator ShowFateCutin(Color colorA, Color colorB)
    {
        if (overlayRoot == null) yield break;

        // デフォルトカラー（白と金）
        if (colorA.a < 0.1f) colorA = new Color(1f, 1f, 1f, 1f);
        if (colorB.a < 0.1f) colorB = new Color(1f, 0.84f, 0.13f, 1f);

        // Canvas背景を隠す
        if (mainBgImg != null) mainBgImg.gameObject.SetActive(false);

        // ── 全画面パネル ──
        var panel = new UIE.VisualElement();
        panel.style.position = UIE.Position.Absolute;
        panel.style.left = 0;
        panel.style.right = 0;
        panel.style.top = 0;
        panel.style.bottom = 0;
        panel.style.backgroundColor = new Color(0, 0, 0, 0);
        panel.style.alignItems = UIE.Align.Center;
        panel.style.justifyContent = UIE.Justify.Center;
        panel.style.overflow = UIE.Overflow.Hidden;
        overlayRoot.Add(panel);

        // Skipボタン
        var skipBtn = CreateSkipButton(panel);

        // ── Phase 1: テーマカラーにフェードイン ──
        float elapsed = 0f;
        float fadeInDur = 0.8f;
        while (elapsed < fadeInDur && !skipRequested)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeInDur);
            panel.style.backgroundColor = new Color(0.969f, 0.906f, 0.808f, t * 0.97f);
            yield return null;
        }
        panel.style.backgroundColor = new Color(0.969f, 0.906f, 0.808f, 0.97f);

        if (!skipRequested) yield return new WaitForSeconds(0.3f);

        if (!skipRequested)
        {
            // ── Phase 2: 中央に光の点が現れる ──
            var lightCore = new UIE.VisualElement();
            lightCore.style.position = UIE.Position.Absolute;
            lightCore.style.left = new UIE.StyleLength(new UIE.Length(50, UIE.LengthUnit.Percent));
            lightCore.style.top = new UIE.StyleLength(new UIE.Length(48, UIE.LengthUnit.Percent));
            float coreSize = 8f;
            lightCore.style.width = coreSize;
            lightCore.style.height = coreSize;
            lightCore.style.translate = new UIE.StyleTranslate(
                new UIE.Translate(-coreSize / 2, -coreSize / 2));
            SetAllRadius(lightCore, coreSize / 2);
            lightCore.style.backgroundColor = new Color(1f, 0.718f, 0.773f, 0f); // サブカラー
            panel.Add(lightCore);

            elapsed = 0f;
            float coreAppearDur = 0.6f;
            while (elapsed < coreAppearDur && !skipRequested)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / coreAppearDur);
                float easeT = t * t;
                float s = Mathf.Lerp(8f, 20f, easeT);
                lightCore.style.width = s;
                lightCore.style.height = s;
                lightCore.style.translate = new UIE.StyleTranslate(
                    new UIE.Translate(-s / 2, -s / 2));
                SetAllRadius(lightCore, s / 2);
                lightCore.style.backgroundColor = new Color(1f, 0.718f, 0.773f, easeT * 0.9f);
                yield return null;
            }
            yield return StartCoroutine(SkippableWait(0.2f));
        }

        if (!skipRequested)
        {
            // ── Phase 3-6: 螺旋→フラッシュ→テキスト→ホワイトアウト ──
            var orbA = CreateLightOrb(colorA);
            var orbB = CreateLightOrb(colorB);
            panel.Add(orbA);
            panel.Add(orbB);
            var glowA = CreateLightOrb(colorA);
            glowA.style.opacity = 0.3f;
            var glowB = CreateLightOrb(colorB);
            glowB.style.opacity = 0.3f;
            panel.Add(glowA);
            panel.Add(glowB);

            float spiralDur = 2.2f;
            float startRadius = 420f;
            elapsed = 0f;
            while (elapsed < spiralDur && !skipRequested)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / spiralDur);
                float easeT = t < 0.5f
                    ? 2f * t * t
                    : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;
                float radius = Mathf.Lerp(startRadius, 0f, easeT);
                float angle = t * 900f * Mathf.Deg2Rad;
                float ax = Mathf.Cos(angle) * radius;
                float ay = Mathf.Sin(angle) * radius;
                float bx = Mathf.Cos(angle + Mathf.PI) * radius;
                float by = Mathf.Sin(angle + Mathf.PI) * radius;
                float orbSize = Mathf.Lerp(44f, 20f, easeT);
                float glowSize = orbSize * 2.5f;
                PositionOrb(orbA, ax, ay, orbSize);
                PositionOrb(orbB, bx, by, orbSize);
                PositionOrb(glowA, ax, ay, glowSize);
                PositionOrb(glowB, bx, by, glowSize);
                yield return null;
            }
            orbA.RemoveFromHierarchy();
            orbB.RemoveFromHierarchy();
            glowA.RemoveFromHierarchy();
            glowB.RemoveFromHierarchy();
        }

        if (!skipRequested)
        {
            // ── Phase 4-5: テキスト出現 + 脈動 ──
            var textLabel = UIHelper.CreateLabel(Localization.Get("cutin_fate_moment"));
            UIHelper.ApplyFontBold(textLabel);
            textLabel.style.position = UIE.Position.Absolute;
            textLabel.style.left = 0;
            textLabel.style.right = 0;
            textLabel.style.top = new UIE.StyleLength(new UIE.Length(46, UIE.LengthUnit.Percent));
            textLabel.style.fontSize = 52;
            textLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            textLabel.style.color = new Color(0.051f, 0.051f, 0.078f, 0f); // ダークテキスト
            panel.Add(textLabel);

            float textInDur = 1.8f;
            elapsed = 0f;
            while (elapsed < textInDur && !skipRequested)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / textInDur);
                float alpha = Mathf.Clamp01(t * 6f);
                float easeT = 1f - (1f - t) * (1f - t);
                float baseScale = Mathf.Lerp(0.8f, 1.1f, easeT);
                float pulse = 1f + Mathf.Sin(elapsed * Mathf.PI * 2.5f) * 0.025f;
                float finalScale = baseScale * pulse;
                textLabel.style.color = new Color(0.051f, 0.051f, 0.078f, alpha);
                textLabel.style.scale = new UIE.StyleScale(
                    new UIE.Scale(new Vector3(finalScale, finalScale, 1f)));
                yield return null;
            }
            yield return StartCoroutine(SkippableWait(0.4f));
        }

        // パネル除去
        panel.RemoveFromHierarchy();

        // Canvas背景を復元
        if (mainBgImg != null) mainBgImg.gameObject.SetActive(true);
    }

    UIE.VisualElement CreateLightOrb(Color color)
    {
        var orb = new UIE.VisualElement();
        orb.style.position = UIE.Position.Absolute;
        orb.style.left = new UIE.StyleLength(new UIE.Length(50, UIE.LengthUnit.Percent));
        orb.style.top = new UIE.StyleLength(new UIE.Length(48, UIE.LengthUnit.Percent));
        float size = 44f;
        orb.style.width = size;
        orb.style.height = size;
        orb.style.translate = new UIE.StyleTranslate(
            new UIE.Translate(-size / 2, -size / 2));
        SetAllRadius(orb, size / 2);
        orb.style.backgroundColor = new Color(color.r, color.g, color.b, 0.85f);
        return orb;
    }

    void PositionOrb(UIE.VisualElement orb, float offsetX, float offsetY, float size)
    {
        orb.style.width = size;
        orb.style.height = size;
        orb.style.translate = new UIE.StyleTranslate(
            new UIE.Translate(offsetX - size / 2, offsetY - size / 2));
        SetAllRadius(orb, size / 2);
    }

    void SetAllRadius(UIE.VisualElement el, float r)
    {
        el.style.borderTopLeftRadius = r;
        el.style.borderTopRightRadius = r;
        el.style.borderBottomLeftRadius = r;
        el.style.borderBottomRightRadius = r;
    }

    void ApplyParallaxOffset(UIE.VisualElement el, Vector2 accel, float amount)
    {
        if (el == null || el.resolvedStyle.display == UIE.DisplayStyle.None) return;
        if (el.parent == null) return;
        float x = accel.x * amount;
        float y = -accel.y * amount; // Y軸反転（上に傾けると上に移動）
        el.style.translate = new UIE.StyleTranslate(new UIE.Translate(x, y));
    }

    UIE.VisualElement CreateSkipButton(UIE.VisualElement parent)
    {
        skipRequested = false;

        // フレックスボックスで右下寄せするラッパー
        var wrapper = new UIE.VisualElement();
        wrapper.AddToClassList("cutin-skip-wrapper");
        parent.Add(wrapper);

        var btn = new UIE.Button();
        btn.AddToClassList("cutin-skip-btn");
        UIHelper.ApplyFont(btn);
        btn.text = "SKIP \u25B6\u25B6";
        btn.clicked += () => { skipRequested = true; };
        wrapper.Add(btn);
        return wrapper;
    }

    IEnumerator SkippableWait(float seconds)
    {
        float t = 0f;
        while (t < seconds && !skipRequested)
        {
            t += Time.deltaTime;
            yield return null;
        }
    }

    // ===== 聖なる光の誕生演出 =====

    IEnumerator ShowBirthCutin()
    {
        if (overlayRoot == null) yield break;

        // キラキラSE（5秒のアニメ全体にかぶせて流しっぱなし）
        if (seKirakira != null)
            seSource.PlayOneShot(seKirakira, 1f);

        // Canvas背景を隠す
        if (mainBgImg != null) mainBgImg.gameObject.SetActive(false);

        // ── 全画面パネル ──
        var panel = new UIE.VisualElement();
        panel.style.position = UIE.Position.Absolute;
        panel.style.left = 0;
        panel.style.right = 0;
        panel.style.top = 0;
        panel.style.bottom = 0;
        panel.style.backgroundColor = new Color(0, 0, 0, 0);
        panel.style.overflow = UIE.Overflow.Hidden;
        overlayRoot.Add(panel);

        // Skipボタン
        var skipBtn = CreateSkipButton(panel);

        // ── Phase 1: テーマカラーにフェードイン ──
        float elapsed = 0f;
        float fadeDur = 1.0f;
        while (elapsed < fadeDur && !skipRequested)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDur);
            panel.style.backgroundColor = new Color(0.969f, 0.906f, 0.808f, t * t * 0.98f);
            yield return null;
        }
        panel.style.backgroundColor = new Color(0.969f, 0.906f, 0.808f, 0.98f);
        yield return StartCoroutine(SkippableWait(0.6f));

        if (!skipRequested)
        {
            // ── Phase 2: 光の種 ──
            var seed = new UIE.VisualElement();
            seed.style.position = UIE.Position.Absolute;
            seed.style.left = new UIE.StyleLength(new UIE.Length(50, UIE.LengthUnit.Percent));
            seed.style.top = new UIE.StyleLength(new UIE.Length(48, UIE.LengthUnit.Percent));
            float seedSize = 6f;
            seed.style.width = seedSize;
            seed.style.height = seedSize;
            seed.style.translate = new UIE.StyleTranslate(
                new UIE.Translate(-seedSize / 2, -seedSize / 2));
            SetAllRadius(seed, seedSize / 2);
            seed.style.backgroundColor = new Color(1f, 0.718f, 0.773f, 0f); // サブカラー
            panel.Add(seed);

            elapsed = 0f;
            while (elapsed < 0.8f && !skipRequested)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / 0.8f);
                float s = Mathf.Lerp(6f, 18f, t * t);
                seed.style.width = s;
                seed.style.height = s;
                seed.style.translate = new UIE.StyleTranslate(
                    new UIE.Translate(-s / 2, -s / 2));
                SetAllRadius(seed, s / 2);
                seed.style.backgroundColor = new Color(1f, 0.718f, 0.773f, t * t * 0.85f);
                yield return null;
            }
            yield return StartCoroutine(SkippableWait(0.3f));

            // ── Phase 3: 鼓動 × 3 ──
            for (int beat = 0; beat < 3 && !skipRequested; beat++)
            {
                float beatPeak = beat < 2 ? 36f : 50f;
                elapsed = 0f;
                while (elapsed < 0.14f && !skipRequested)
                {
                    elapsed += Time.deltaTime;
                    float t = Mathf.Clamp01(elapsed / 0.14f);
                    float s = Mathf.Lerp(18f, beatPeak, 1f - (1f - t) * (1f - t));
                    seed.style.width = s; seed.style.height = s;
                    seed.style.translate = new UIE.StyleTranslate(new UIE.Translate(-s / 2, -s / 2));
                    SetAllRadius(seed, s / 2);
                    yield return null;
                }
                StartCoroutine(SpawnRipple(panel, beat));
                elapsed = 0f;
                while (elapsed < 0.21f && !skipRequested)
                {
                    elapsed += Time.deltaTime;
                    float t = Mathf.Clamp01(elapsed / 0.21f);
                    float s = Mathf.Lerp(beatPeak, 18f, t * t);
                    seed.style.width = s; seed.style.height = s;
                    seed.style.translate = new UIE.StyleTranslate(new UIE.Translate(-s / 2, -s / 2));
                    SetAllRadius(seed, s / 2);
                    yield return null;
                }
                yield return StartCoroutine(SkippableWait(beat == 0 ? 0.7f : beat == 1 ? 0.5f : 0.3f));
            }
        }

        if (!skipRequested)
        {
            // ── Phase 4: テキスト出現 ──
            var textLabel = UIHelper.CreateLabel(Localization.Get("cutin_birth_wish"));
            UIHelper.ApplyFontBold(textLabel);
            textLabel.style.position = UIE.Position.Absolute;
            textLabel.style.left = 0; textLabel.style.right = 0;
            textLabel.style.top = new UIE.StyleLength(new UIE.Length(44, UIE.LengthUnit.Percent));
            textLabel.style.fontSize = 46;
            textLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            textLabel.style.color = new Color(0.051f, 0.051f, 0.078f, 0f); // ダークテキスト
            panel.Add(textLabel);

            elapsed = 0f;
            while (elapsed < 2.2f && !skipRequested)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / 2.2f);
                float alpha = Mathf.Clamp01(elapsed / 0.4f);
                float easeT = 1f - (1f - t) * (1f - t);
                float s = Mathf.Lerp(0.85f, 1.05f, easeT) * (1f + Mathf.Sin(elapsed * Mathf.PI * 2f) * 0.02f);
                textLabel.style.color = new Color(0.051f, 0.051f, 0.078f, alpha);
                textLabel.style.scale = new UIE.StyleScale(new UIE.Scale(new Vector3(s, s, 1f)));
                yield return null;
            }
            yield return StartCoroutine(SkippableWait(0.5f));
        }

        // パネル除去
        panel.RemoveFromHierarchy();

        // Canvas背景を復元
        if (mainBgImg != null) mainBgImg.gameObject.SetActive(true);
    }

    IEnumerator SpawnRipple(UIE.VisualElement parent, int intensity)
    {
        var ripple = new UIE.VisualElement();
        ripple.style.position = UIE.Position.Absolute;
        ripple.style.left = new UIE.StyleLength(new UIE.Length(50, UIE.LengthUnit.Percent));
        ripple.style.top = new UIE.StyleLength(new UIE.Length(48, UIE.LengthUnit.Percent));
        float startSize = 20f;
        ripple.style.width = startSize;
        ripple.style.height = startSize;
        ripple.style.translate = new UIE.StyleTranslate(
            new UIE.Translate(-startSize / 2, -startSize / 2));
        SetAllRadius(ripple, startSize / 2);
        ripple.style.backgroundColor = new Color(0, 0, 0, 0);
        // 波紋は白い輪（borderで表現）
        float borderWidth = intensity >= 3 ? 4f : 2f;
        ripple.style.borderTopWidth = borderWidth;
        ripple.style.borderBottomWidth = borderWidth;
        ripple.style.borderLeftWidth = borderWidth;
        ripple.style.borderRightWidth = borderWidth;
        ripple.style.borderTopColor = new Color(1f, 0.718f, 0.773f, 0.7f);
        ripple.style.borderBottomColor = new Color(1f, 0.718f, 0.773f, 0.7f);
        ripple.style.borderLeftColor = new Color(1f, 0.718f, 0.773f, 0.7f);
        ripple.style.borderRightColor = new Color(1f, 0.718f, 0.773f, 0.7f);
        parent.Add(ripple);

        // 波紋の拡大 + フェードアウト
        float maxSize = intensity >= 3 ? 1800f : 600f + intensity * 300f;
        float rippleDur = intensity >= 3 ? 1.2f : 1.0f;
        float elapsed = 0f;
        while (elapsed < rippleDur)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / rippleDur);
            float easeT = 1f - (1f - t) * (1f - t); // ease-out
            float s = Mathf.Lerp(startSize, maxSize, easeT);
            ripple.style.width = s;
            ripple.style.height = s;
            ripple.style.translate = new UIE.StyleTranslate(
                new UIE.Translate(-s / 2, -s / 2));
            SetAllRadius(ripple, s / 2);

            // フェードアウト
            float alpha = 0.7f * (1f - t);
            ripple.style.borderTopColor = new Color(1f, 0.718f, 0.773f, alpha);
            ripple.style.borderBottomColor = new Color(1f, 0.718f, 0.773f, alpha);
            ripple.style.borderLeftColor = new Color(1f, 0.718f, 0.773f, alpha);
            ripple.style.borderRightColor = new Color(1f, 0.718f, 0.773f, alpha);

            yield return null;
        }

        ripple.RemoveFromHierarchy();
    }

    IEnumerator ShowLoveStory(ParentData father, ParentData mother, int fIdx, int mIdx)
    {
        string story = Localization.GetLoveStory(father.name, mother.name);

        Debug.Log($"[BirthSystem] ShowLoveStory called - storyPanel: {storyPanel != null}, storyText: {storyText != null}, story: {(story != null ? story.Length + " chars" : "null")}");

        if (storyPanel != null && storyText != null)
        {
            // Canvas背景を隠す（Screen Space - OverlayがUI Toolkitの上に描画されるため）
            if (mainBgImg != null) mainBgImg.gameObject.SetActive(false);

            // 親パネルを一時的に隠す
            if (parentPanel != null) parentPanel.style.display = UIE.DisplayStyle.None;

            // 父親カードを設定
            if (storyFatherFace != null)
            {
                Sprite fSp = Resources.Load<Sprite>($"Parents/{father.imageName}");
                if (fSp != null)
                {
                    storyFatherFace.style.backgroundImage = new UIE.StyleBackground(fSp);
                    storyFatherFace.style.backgroundColor = UIE.StyleKeyword.None;
                }
                else
                {
                    storyFatherFace.style.backgroundImage = UIE.StyleKeyword.None;
                    storyFatherFace.style.backgroundColor = father.faceColor;
                }
            }
            if (storyFatherName != null) storyFatherName.text = Localization.GetParent(father.name);
            if (storyFatherIntro != null)
            {
                string fullIntro = Localization.GetParentIntro(father.name);
                string shortTitle = fullIntro.Contains(" / ") ? fullIntro.Split(new[] { " / " }, System.StringSplitOptions.None)[0] : fullIntro;
                storyFatherIntro.text = shortTitle;
                storyFatherIntro.userData = fullIntro.Replace(" / ", "\n"); // フルテキストをツールチップ用に保存
            }

            // 母親カードを設定
            if (storyMotherFace != null)
            {
                Sprite mSp = Resources.Load<Sprite>($"Parents/{mother.imageName}");
                if (mSp != null)
                {
                    storyMotherFace.style.backgroundImage = new UIE.StyleBackground(mSp);
                    storyMotherFace.style.backgroundColor = UIE.StyleKeyword.None;
                }
                else
                {
                    storyMotherFace.style.backgroundImage = UIE.StyleKeyword.None;
                    storyMotherFace.style.backgroundColor = mother.faceColor;
                }
            }
            if (storyMotherName != null) storyMotherName.text = Localization.GetParent(mother.name);
            if (storyMotherIntro != null)
            {
                string fullIntro = Localization.GetParentIntro(mother.name);
                string shortTitle = fullIntro.Contains(" / ") ? fullIntro.Split(new[] { " / " }, System.StringSplitOptions.None)[0] : fullIntro;
                storyMotherIntro.text = shortTitle;
                storyMotherIntro.userData = fullIntro.Replace(" / ", "\n");
            }

            storyText.text = "";
            storyText.style.opacity = 0f;
            storyPanel.style.display = UIE.DisplayStyle.Flex;

            // スキップボタン（テキスト全表示のみ、ページ送りはしない）
            var skipBtn = CreateSkipButton(storyPanel);

            // ストーリーを1文字ずつ表示（フェードイン + タイプライター効果）
            int charCount = 0;
            foreach (char c in story)
            {
                if (skipRequested) break;
                storyText.text += c;
                charCount++;
                // 最初の10文字でフェードイン（0→1）
                if (charCount <= 10)
                    storyText.style.opacity = Mathf.Clamp01(charCount / 10f);
                yield return new WaitForSeconds(0.05f);
            }

            // スキップ時は全文を即表示
            storyText.style.opacity = 1f;
            if (skipRequested)
            {
                storyText.text = story;
                skipRequested = false;
            }

            skipBtn.RemoveFromHierarchy();

            // ストーリー完了時にハートパーティクル発射
            if (storyAlbumContent != null)
                StartCoroutine(SpawnStoryHeartParticles(storyAlbumContent));

            // タップ待ち（タップするまで進まない）
            waitingForStoryConfirm = true;
            while (waitingForStoryConfirm)
            {
                yield return null;
            }
            // ツールチップが残っていたら閉じる
            if (activeStoryTooltip != null)
            {
                activeStoryTooltip.RemoveFromHierarchy();
                activeStoryTooltip = null;
            }

            storyPanel.style.display = UIE.DisplayStyle.None;

            // Canvas背景を復元
            if (mainBgImg != null) mainBgImg.gameObject.SetActive(true);

            // 親パネルを再表示
            if (parentPanel != null) parentPanel.style.display = UIE.DisplayStyle.Flex;
        }

        if (!skipRequested)
            yield return new WaitForSeconds(0.1f); // ホワイトアウトが間を持たせるため短縮
    }

    // ── ストーリー完了時のハートパーティクル ──
    IEnumerator SpawnStoryHeartParticles(UIE.VisualElement container)
    {
        string[] hearts = { "\u2665", "\u2764", "\u2661" };
        for (int i = 0; i < 12; i++)
        {
            var heart = UIHelper.CreateLabel(hearts[Random.Range(0, hearts.Length)]);
            UIHelper.ApplyFont(heart);
            heart.pickingMode = UIE.PickingMode.Ignore;
            heart.style.position = UIE.Position.Absolute;
            heart.style.fontSize = Random.Range(24, 44);
            heart.style.color = new Color(1f, 0.718f, 0.773f, 1f); // #FFB7C5
            float xPct = Random.Range(20f, 80f);
            float yPct = Random.Range(10f, 90f);
            heart.style.left = new UIE.StyleLength(new UIE.Length(xPct, UIE.LengthUnit.Percent));
            heart.style.top = new UIE.StyleLength(new UIE.Length(yPct, UIE.LengthUnit.Percent));
            heart.style.opacity = 1f;
            container.Add(heart);
            StartCoroutine(FadeOutHeart(heart));
            yield return new WaitForSeconds(0.08f);
        }
    }

    IEnumerator FadeOutHeart(UIE.Label heart)
    {
        float duration = 1.2f;
        float elapsed = 0f;
        float driftY = Random.Range(-100f, -40f);

        while (elapsed < duration && heart != null && heart.parent != null)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            heart.style.top = new UIE.StyleLength(
                heart.resolvedStyle.top + driftY * Time.deltaTime);
            heart.style.opacity = 1f - t;
            float s = 1f + 0.4f * Mathf.Sin(t * Mathf.PI);
            heart.style.scale = new UIE.StyleScale(new UIE.Scale(new Vector2(s, s)));
            yield return null;
        }
        if (heart != null && heart.parent != null)
            heart.RemoveFromHierarchy();
    }

    IEnumerator ShowFailureSequence(string locPrefix, Color lineColor)
    {
        if (parentPanel != null) parentPanel.style.display = UIE.DisplayStyle.None;
        if (childStatusText != null) childStatusText.text = "";
        if (babyBioLabel != null) babyBioLabel.text = "";

        Canvas cv = FindObjectOfType<Canvas>();

        var panel = new GameObject("FailPanel");
        panel.transform.SetParent(cv.transform, false);
        var panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        var panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0f);
        panelImage.raycastTarget = true;

        float fadeInDuration = 0.5f;
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            panelImage.color = new Color(0f, 0f, 0f, Mathf.Lerp(0f, 0.9f, elapsed / fadeInDuration));
            yield return null;
        }
        panelImage.color = new Color(0f, 0f, 0f, 0.9f);

        float slideDuration = 1.5f;
        float lineInterval = 2.0f;
        float startOffsetX = -800f;
        float verticalStart = 350f;
        float lineSpacing = 250f;

        string[] sadLines = new string[]
        {
            Localization.Get(locPrefix + "_line1"),
            Localization.Get(locPrefix + "_line2"),
            Localization.Get(locPrefix + "_line3"),
            Localization.Get(locPrefix + "_line4"),
        };

        for (int i = 0; i < sadLines.Length; i++)
        {
            var textObj = new GameObject("FailLine_" + i);
            textObj.transform.SetParent(panel.transform, false);
            var textRect = textObj.AddComponent<RectTransform>();
            var tmp = textObj.AddComponent<TextMeshProUGUI>();
            FontHelper.Apply(tmp);

            tmp.text = sadLines[i];
            tmp.fontSize = i == 0 ? 72 : 60;
            tmp.color = i == 0 ? Color.white : lineColor;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableWordWrapping = true;
            tmp.raycastTarget = false;

            float yPos = verticalStart - (i * lineSpacing);
            textRect.sizeDelta = new Vector2(900f, 220f);
            textRect.anchoredPosition = new Vector2(startOffsetX, yPos);

            float slideElapsed = 0f;
            Vector2 startPos = new Vector2(startOffsetX, yPos);
            Vector2 endPos = new Vector2(0f, yPos);

            while (slideElapsed < slideDuration)
            {
                slideElapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, slideElapsed / slideDuration);
                textRect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
                yield return null;
            }
            textRect.anchoredPosition = endPos;

            if (i < sadLines.Length - 1)
            {
                yield return new WaitForSeconds(lineInterval);
            }
        }

        yield return new WaitForSeconds(4.0f);

        CanvasGroup canvasGroup = panel.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 1f;
        elapsed = 0f;
        float fadeOutDuration = 1.0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeOutDuration);
            yield return null;
        }

        Destroy(panel);

        ResetBackgroundToBirth();
        if (childStatusText != null) childStatusText.text = "";
        if (babyBioLabel != null) babyBioLabel.text = "";
        if (generateLifeButton != null) generateLifeButton.SetActive(true);
        if (anotherGalButton != null) anotherGalButton.SetActive(false);
        if (gotoBattleButton != null) gotoBattleButton.SetActive(false);
    }

    // ===== メニューバー =====

    void CreateMenuBar()
    {
        var (safeTop, _, _, _) = UIHelper.GetSafeMargins();

        // Menu button (top-right, star icon)
        var menuBtn = new UIE.Button();
        menuBtn.AddToClassList("birth-menu-btn");
        menuBtn.style.top = 24 + safeTop;
        var starLabel = UIHelper.CreateLabel("\u2606", "birth-menu-star");
        menuBtn.Add(starLabel);
        menuBtn.clicked += ToggleMenuPanel;
        overlayRoot.Add(menuBtn);

        // Menu overlay (backdrop + card)
        menuOverlayEl = new UIE.VisualElement();
        menuOverlayEl.AddToClassList("birth-menu-overlay");
        menuOverlayEl.RegisterCallback<UIE.ClickEvent>(evt =>
        {
            // Close when clicking the backdrop (not the card)
            if (evt.target == menuOverlayEl)
                ToggleMenuPanel();
        });

        var menuCard = new UIE.VisualElement();
        menuCard.AddToClassList("birth-menu-card");
        menuCard.style.top = 114 + safeTop;

        // User icon + name
        var userRow = new UIE.VisualElement();
        userRow.AddToClassList("birth-menu-user-row");

        var iconMask = new UIE.VisualElement();
        iconMask.AddToClassList("birth-menu-user-icon");

        string[] iconNames = { "kayo", "ikemen", "inteli", "matcho", "old-women", "sexy-lady" };
        int iconIdx = DataCarrier.Instance != null ? DataCarrier.Instance.playerIcon : DataCarrier.GetProfileIcon();
        string icoName = (iconIdx >= 0 && iconIdx < iconNames.Length) ? iconNames[iconIdx] : iconNames[0];
        Sprite icoSpr = Resources.Load<Sprite>($"Icons/{icoName}");
        if (icoSpr != null)
            iconMask.style.backgroundImage = new UIE.StyleBackground(icoSpr);
        userRow.Add(iconMask);

        string pName = DataCarrier.Instance != null ? DataCarrier.Instance.playerName : DataCarrier.GetProfileName();
        var nameLabel = UIHelper.CreateLabel(string.IsNullOrEmpty(pName) ? "???" : pName, "birth-menu-user-name");
        userRow.Add(nameLabel);

        menuCard.Add(userRow);

        // Separator
        var sep = new UIE.VisualElement();
        sep.AddToClassList("birth-menu-separator");
        menuCard.Add(sep);

        var homeBtn = new UIE.Button();
        homeBtn.AddToClassList("birth-menu-item-btn");
        UIHelper.ApplyFont(homeBtn);
        homeBtn.text = Localization.Get("map_menu_home");
        homeBtn.clicked += () => SceneManager.LoadScene("HomeScene");
        menuCard.Add(homeBtn);

        var titleBtn = new UIE.Button();
        titleBtn.AddToClassList("birth-menu-item-btn");
        UIHelper.ApplyFont(titleBtn);
        titleBtn.text = Localization.Get("ui_back_to_title");
        titleBtn.clicked += () => SceneManager.LoadScene("TitleScene");
        menuCard.Add(titleBtn);

        menuOverlayEl.Add(menuCard);

        // Hidden initially
    }

    void ToggleMenuPanel()
    {
        if (menuOverlayEl == null) return;
        if (menuOverlayEl.parent != null)
            menuOverlayEl.RemoveFromHierarchy();
        else
            overlayRoot.Add(menuOverlayEl);
    }

    static Sprite _circleSprite;
    static Sprite GetCircleSprite(int radius)
    {
        if (_circleSprite != null) return _circleSprite;
        int size = radius * 2 + 2;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float center = (size - 1) / 2f;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dist = Mathf.Sqrt((x - center) * (x - center) + (y - center) * (y - center)) - radius;
                if (dist < -1f) tex.SetPixel(x, y, Color.white);
                else if (dist <= 0f) tex.SetPixel(x, y, new Color(1, 1, 1, -dist));
                else tex.SetPixel(x, y, new Color(0, 0, 0, 0));
            }
        tex.Apply();
        var border = new Vector4(radius, radius, radius, radius);
        _circleSprite = Sprite.Create(tex, new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f), 100, 0, SpriteMeshType.FullRect, border);
        return _circleSprite;
    }

    // ===== 角丸スプライト生成 =====

    static Sprite _roundedRectSprite;

    static Sprite GetRoundedRectSprite(int radius = 32)
    {
        if (_roundedRectSprite != null) return _roundedRectSprite;

        int size = radius * 2 + 2;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color clear = new Color(0, 0, 0, 0);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                // 四隅の角丸判定
                float dx = 0, dy = 0;
                if (x < radius) dx = radius - x;
                else if (x >= size - radius) dx = x - (size - radius - 1);
                if (y < radius) dy = radius - y;
                else if (y >= size - radius) dy = y - (size - radius - 1);

                if (dx > 0 && dy > 0)
                {
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    if (dist > radius)
                        tex.SetPixel(x, y, clear);
                    else if (dist > radius - 1)
                        tex.SetPixel(x, y, new Color(1, 1, 1, radius - dist));
                    else
                        tex.SetPixel(x, y, Color.white);
                }
                else
                {
                    tex.SetPixel(x, y, Color.white);
                }
            }
        }
        tex.Apply();

        var border = new Vector4(radius, radius, radius, radius);
        _roundedRectSprite = Sprite.Create(tex, new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f), 100, 0, SpriteMeshType.FullRect, border);
        return _roundedRectSprite;
    }

    // ===== Pill型スプライト生成（border-radius: 50%）=====

    static Sprite _pillSprite;
    static Sprite _shadowSprite;

    static Sprite GetPillSprite(int radius)
    {
        if (_pillSprite != null) return _pillSprite;

        int size = radius * 2 + 2;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float center = (size - 1) / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float px = x - center;
                float py = y - center;
                float dist = Mathf.Sqrt(px * px + py * py) - radius;

                if (dist <= -1f)
                    tex.SetPixel(x, y, Color.white);
                else if (dist <= 0f)
                    tex.SetPixel(x, y, new Color(1, 1, 1, -dist));
                else
                    tex.SetPixel(x, y, new Color(0, 0, 0, 0));
            }
        }
        tex.Apply();

        var border = new Vector4(radius, radius, radius, radius);
        _pillSprite = Sprite.Create(tex, new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f), 100, 0, SpriteMeshType.FullRect, border);
        return _pillSprite;
    }

    static Sprite GetShadowSprite(int radius, int blur)
    {
        if (_shadowSprite != null) return _shadowSprite;

        int size = (radius + blur) * 2 + 2;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float center = (size - 1) / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float px = x - center;
                float py = y - center;
                float dist = Mathf.Sqrt(px * px + py * py) - radius;

                float alpha;
                if (dist <= 0f)
                    alpha = 0f; // 中央は透明（ボタン白背景と重なる部分）
                else if (dist >= blur)
                    alpha = 0f;
                else
                {
                    float t = dist / blur;
                    alpha = (1f - t) * (1f - t);
                }

                tex.SetPixel(x, y, new Color(1, 1, 1, alpha));
            }
        }
        tex.Apply();

        int borderVal = radius + blur;
        var border = new Vector4(borderVal, borderVal, borderVal, borderVal);
        _shadowSprite = Sprite.Create(tex, new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f), 100, 0, SpriteMeshType.FullRect, border);
        return _shadowSprite;
    }

    // ── ジャイロパララックス (毎フレーム更新) ──
    void Update()
    {
        Vector3 accel = Input.acceleration;
        Vector2 targetAccel = new Vector2(
            Mathf.Clamp(accel.x, -1f, 1f),
            Mathf.Clamp(accel.y, -1f, 1f));
        smoothAccel = Vector2.Lerp(smoothAccel, targetAccel, Time.deltaTime * PARALLAX_SMOOTH);

        // 顔画像にパララックス適用
        ApplyParallaxOffset(fatherFaceImage, smoothAccel, PARALLAX_FACE_AMOUNT);
        ApplyParallaxOffset(motherFaceImage, smoothAccel, PARALLAX_FACE_AMOUNT);
    }

    // ================================================================
    // ぷにぷにインタラクション（ほっぺ変形 + 視線追従 + 診断）
    // ================================================================

    // ================================================================
    // 座標変換パイプライン
    // ================================================================
    //
    // ★ 座標空間の定義:
    //   A) 元画像空間:    0-1, top-left原点 (ランドマーク/目タップの入力)
    //   B) クロップUV空間: 0-1, bottom-left原点 (DrawFace の u_adj/v_adj と同一)
    //   C) composite空間:  0-1, bottom-left原点 (pixels[y*w+x] で y=0 が最下部)
    //
    // ★ 非正方形画像の処理方式: 中央切り抜き (Center Crop)
    //   cropSize = min(W, H) の正方形を画像中央から切り出す。
    //   余白(Padding/Letterbox)は存在しない。
    //   DrawFace/DrawSwaddleAndFace も同じ cropSize/cropOffset で
    //   ソースピクセルをサンプリングしている。
    // ================================================================

    /// <summary>
    /// 元画像の正規化座標(0-1, top-left原点) → クロップUV(0-1, bottom-left原点)。
    ///
    /// DrawFace の逆関数:
    ///   DrawFace: u_adj → srcX = u_adj * cropSize + cropOffsetX
    ///   本関数:   srcX → u_adj = (srcX - cropOffsetX) / cropSize
    ///
    /// 3ステップで変換:
    ///   Step1 (Normalization): 入力の正規化座標をそのまま使用 (0-1)
    ///   Step2 (Aspect Correction): center-crop オフセットを適用
    ///   Step3 (V-Flip): top-left → bottom-left Y軸反転
    /// </summary>
    Vector2 ImageNormToCropUV(Vector2 imageNorm, int imgW, int imgH)
    {
        if (imgW <= 0 || imgH <= 0)
        {
            Debug.LogWarning($"[CoordTransform] ImageNormToCropUV: invalid image size ({imgW}x{imgH}), fallback Y-flip only");
            return new Vector2(imageNorm.x, 1f - imageNorm.y);
        }

        float aspectRatio = (float)imgW / imgH;

        // ── Step1: Normalization ──
        // 元画像ピクセル座標 (top-left 原点)
        float pixX_topLeft = imageNorm.x * imgW;
        float pixY_topLeft = imageNorm.y * imgH;

        // ── Step2: Aspect Correction (中央切り抜き) ──
        // DrawFace と同じ: cropSize = min(W,H), offset = (dim - cropSize) / 2
        float cropSize = Mathf.Min(imgW, imgH);
        float cropOffX = (imgW - cropSize) * 0.5f;
        float cropOffY = (imgH - cropSize) * 0.5f;

        // top-left原点のままクロップ空間に変換 (top-left)
        float cropNormX_topLeft = (pixX_topLeft - cropOffX) / cropSize;
        float cropNormY_topLeft = (pixY_topLeft - cropOffY) / cropSize;

        // ── Step3: V-Flip (top-left → bottom-left) ──
        // Unity の GetPixels/SetPixels は bottom-left 原点
        float cropNormX = cropNormX_topLeft;
        float cropNormY = 1f - cropNormY_topLeft;

        Debug.Log($"[CoordTransform] ImageNormToCropUV: " +
            $"imgSize=({imgW}x{imgH}) aspect={aspectRatio:F3} " +
            $"cropSize={cropSize:F0} cropOff=({cropOffX:F0},{cropOffY:F0}) | " +
            $"input(top-left)=({imageNorm.x:F3},{imageNorm.y:F3}) → " +
            $"px=({pixX_topLeft:F1},{pixY_topLeft:F1}) → " +
            $"cropTopLeft=({cropNormX_topLeft:F3},{cropNormY_topLeft:F3}) → " +
            $"cropUV(bottom-left)=({cropNormX:F3},{cropNormY:F3})");

        return new Vector2(cropNormX, cropNormY);
    }

    /// <summary>
    /// 元画像空間のランドマーク座標(A) → composite正規化座標(C)。
    ///
    /// DrawFace の逆関数を3ステップで実行:
    ///   Step1: 元画像座標 → クロップUV (ImageNormToCropUV)
    ///   Step2: ユーザーの位置調整 (zoom/pan) を適用 — DrawFace の u_adj→u の逆
    ///   Step3: 顔穴パラメータで composite 空間にマッピング — DrawFace の px→u の逆
    ///
    /// 検証方法: DrawFace で composite pixel (px, py) からサンプリングされる
    ///           ソースピクセルが、この関数の入力ランドマークと一致すること。
    /// </summary>
    Vector2 FaceLandmarkToComposite(Vector2 imageNorm)
    {
        // ── Step1: 元画像座標 → クロップUV ──
        // ImageNormToCropUV: top-left→bottom-left Y反転 + center-crop アスペクト比補正
        Vector2 cropUV = ImageNormToCropUV(imageNorm, originalFaceW, originalFaceH);

        // ── Step2: ユーザーの位置調整 (zoom/pan) を適用 ──
        // DrawFace の逆: u_adj = (u - 0.5) / faceScale + 0.5 - faceOffsetX
        //             → u = (u_adj - 0.5 + faceOffsetX) * faceScale + 0.5
        float faceScale = eyeMarkSavedScale > 0.01f ? eyeMarkSavedScale : 1f;
        float faceOffsetX = eyeMarkSavedOffsetX;
        float faceOffsetY = eyeMarkSavedOffsetY;
        float u = (cropUV.x - 0.5f + faceOffsetX) * faceScale + 0.5f;
        float v = (cropUV.y - 0.5f + faceOffsetY) * faceScale + 0.5f;

        // ── Step3: 顔穴パラメータで composite 空間にマッピング ──
        // DrawFace の逆: u = (px - cx) / (uniformR * 2) + 0.5
        //             → px = (u - 0.5) * uniformR * 2 + cx
        //             → composite_norm = px / TEX_SIZE
        //               = (u - 0.5) * (uniformR_px * 2 / TEX_SIZE) + cx / TEX_SIZE
        //               = (u - 0.5) * (max(RX, RY) * 1.05 * 2) + CX
        float cx = babySynthesizer != null ? babySynthesizer.GetFaceHoleCX() : 0.50f;
        float cy = babySynthesizer != null ? babySynthesizer.GetFaceHoleCY() : 0.68f;
        float rx = babySynthesizer != null ? babySynthesizer.GetFaceHoleRX() : 0.13f;
        float ry = babySynthesizer != null ? babySynthesizer.GetFaceHoleRY() : 0.12f;
        float uniformR = Mathf.Max(rx, ry) * 1.05f;

        Vector2 result = new Vector2(
            cx + (u - 0.5f) * uniformR * 2f,
            cy + (v - 0.5f) * uniformR * 2f);

        Debug.Log($"[CoordTransform] FaceLandmarkToComposite: " +
            $"input=({imageNorm.x:F3},{imageNorm.y:F3}) → " +
            $"cropUV=({cropUV.x:F3},{cropUV.y:F3}) → " +
            $"adjusted(u,v)=({u:F3},{v:F3}) [scale={faceScale:F2} off=({faceOffsetX:F3},{faceOffsetY:F3})] → " +
            $"composite=({result.x:F3},{result.y:F3}) " +
            $"[hole=({cx:F2},{cy:F2}) r=({rx:F3},{ry:F3}) uniformR={uniformR:F4}]");

        // ── 検証: DrawFace の順方向計算で同じソースピクセルを得るか確認 ──
        {
            float TEX = 1080f;
            float cx_px = cx * TEX;
            float cy_px = cy * TEX;
            float uniformR_px = uniformR * TEX;
            float px = result.x * TEX;
            float py = result.y * TEX;
            float u_chk = (px - cx_px) / (uniformR_px * 2f) + 0.5f;
            float v_chk = (py - cy_px) / (uniformR_px * 2f) + 0.5f;
            float uAdj_chk = (u_chk - 0.5f) / faceScale + 0.5f - faceOffsetX;
            float vAdj_chk = (v_chk - 0.5f) / faceScale + 0.5f - faceOffsetY;
            float cropSize = Mathf.Min(originalFaceW, originalFaceH);
            float cropOffX = (originalFaceW - cropSize) * 0.5f;
            float cropOffY = (originalFaceH - cropSize) * 0.5f;
            float srcX = uAdj_chk * cropSize + cropOffX;
            float srcY = vAdj_chk * cropSize + cropOffY;
            // bottom-left → top-left
            float srcY_topLeft = originalFaceH > 0 ? originalFaceH - srcY : srcY;
            float srcNormX = originalFaceW > 0 ? srcX / originalFaceW : 0;
            float srcNormY = originalFaceH > 0 ? srcY_topLeft / originalFaceH : 0;
            Debug.Log($"[CoordTransform] ★検証(DrawFace順方向): " +
                $"composite({result.x:F3},{result.y:F3}) → " +
                $"DrawFace u_adj=({uAdj_chk:F3},{vAdj_chk:F3}) → " +
                $"srcPx=({srcX:F1},{srcY:F1}) → " +
                $"srcNorm(top-left)=({srcNormX:F3},{srcNormY:F3}) " +
                $"[期待値=({imageNorm.x:F3},{imageNorm.y:F3}) " +
                $"差=({Mathf.Abs(srcNormX - imageNorm.x):F4},{Mathf.Abs(srcNormY - imageNorm.y):F4})]");
        }

        return result;
    }

    /// <summary>
    /// 合成画像の正規化座標 → faceInteractionClip 内のローカルpx座標に変換
    /// </summary>
    Vector2 CompositeToClipLocal(Vector2 compositeNorm, float clipL, float clipT, float imgW, float imgH)
    {
        float px = compositeNorm.x * imgW - clipL;
        float py = compositeNorm.y * imgH - clipT;
        return new Vector2(px, py);
    }

    /// <summary>
    /// synthBabyImageEl のローカルpx座標 → 合成画像の正規化座標
    /// </summary>
    Vector2 ImageLocalToComposite(Vector2 localPos)
    {
        if (synthBabyImageEl == null) return Vector2.zero;
        float w = synthBabyImageEl.resolvedStyle.width;
        float h = synthBabyImageEl.resolvedStyle.height;
        if (w <= 0 || h <= 0) return Vector2.zero;
        return new Vector2(localPos.x / w, localPos.y / h);
    }

    /// <summary>
    /// シェアモードでのインタラクション一括セットアップ
    /// </summary>
    void SetupBabyFaceInteractions()
    {
        if (synthBabyImageEl == null) return;

        // タップイベントを受け付ける（synthBabyImageEl全体で受信 → 座標で判定）
        synthBabyImageEl.pickingMode = UIE.PickingMode.Position;
        synthBabyImageEl.RegisterCallback<UIE.PointerDownEvent>(OnBabyFaceTap);
        synthBabyImageEl.RegisterCallback<UIE.PointerMoveEvent>(OnGazePointerMove);
        synthBabyImageEl.RegisterCallback<UIE.PointerLeaveEvent>(OnGazePointerLeave);

        // レイアウト確定後に顔クリップ＋瞳を配置
        synthBabyImageEl.schedule.Execute(() => SetupFaceClipAndPupils());

        puniInteractionEnabled = true;
        Debug.Log("[BirthSystem] Baby face interactions enabled");
    }

    /// <summary>
    /// 顔穴位置にクリップコンテナを作成し、瞳オーバーレイを配置
    /// </summary>
    void SetupFaceClipAndPupils()
    {
        if (synthBabyImageEl == null) return;

        float imgW = synthBabyImageEl.resolvedStyle.width;
        float imgH = synthBabyImageEl.resolvedStyle.height;
        if (imgW <= 0 || imgH <= 0) return;

        float cx = babySynthesizer != null ? babySynthesizer.GetFaceHoleCX() : 0.50f;
        float cy = babySynthesizer != null ? babySynthesizer.GetFaceHoleCY() : 0.68f;
        float rx = babySynthesizer != null ? babySynthesizer.GetFaceHoleRX() : 0.13f;
        float ry = babySynthesizer != null ? babySynthesizer.GetFaceHoleRY() : 0.12f;

        // 顔穴の表示ピクセル位置
        float clipW = rx * 2f * imgW;
        float clipH = ry * 2f * imgH;
        float clipL = (cx - rx) * imgW;
        float clipT = (cy - ry) * imgH;

        // 顔クリップコンテナ（楕円、overflow:hidden）
        faceInteractionClip = new UIE.VisualElement();
        faceInteractionClip.name = "face-interaction-clip";
        faceInteractionClip.pickingMode = UIE.PickingMode.Ignore;
        faceInteractionClip.style.position = UIE.Position.Absolute;
        faceInteractionClip.style.left = clipL;
        faceInteractionClip.style.top = clipT;
        faceInteractionClip.style.width = clipW;
        faceInteractionClip.style.height = clipH;
        faceInteractionClip.style.overflow = UIE.Overflow.Hidden;
        faceInteractionClip.style.borderTopLeftRadius = new UIE.Length(50, UIE.LengthUnit.Percent);
        faceInteractionClip.style.borderTopRightRadius = new UIE.Length(50, UIE.LengthUnit.Percent);
        faceInteractionClip.style.borderBottomLeftRadius = new UIE.Length(50, UIE.LengthUnit.Percent);
        faceInteractionClip.style.borderBottomRightRadius = new UIE.Length(50, UIE.LengthUnit.Percent);
        synthBabyImageEl.Add(faceInteractionClip);

        Debug.Log($"[BirthSystem] Face clip created: L={clipL:F1} T={clipT:F1} W={clipW:F1} H={clipH:F1} (image={imgW:F0}x{imgH:F0})");
        Debug.Log($"[BirthSystem] Face hole: cx={cx:F2} cy={cy:F2} rx={rx:F2} ry={ry:F2}");
        Debug.Log($"[BirthSystem] Face adjust: offsetX={eyeMarkSavedOffsetX:F3} offsetY={eyeMarkSavedOffsetY:F3} scale={eyeMarkSavedScale:F3}");

        // 瞳オーバーレイ配置（ランドマーク検出時のみ）
        if (detectedFaceLandmarks.HasValue)
        {
            var lm = detectedFaceLandmarks.Value;

            // 顔テクスチャ空間 → 合成画像空間 → クリップローカル座標
            Vector2 leftEyeComp = FaceLandmarkToComposite(lm.leftEyeCenter);
            Vector2 rightEyeComp = FaceLandmarkToComposite(lm.rightEyeCenter);

            Vector2 leftEyeLocal = CompositeToClipLocal(leftEyeComp, clipL, clipT, imgW, imgH);
            Vector2 rightEyeLocal = CompositeToClipLocal(rightEyeComp, clipL, clipT, imgW, imgH);

            // 瞳サイズ（クリップ領域の幅の7%程度）
            float pupilSize = clipW * 0.07f;
            pupilSize = Mathf.Clamp(pupilSize, 4f, 30f);

            leftPupilBasePos = leftEyeLocal;
            leftPupilEl = CreatePupilOverlay(leftEyeLocal, pupilSize);

            rightPupilBasePos = rightEyeLocal;
            rightPupilEl = CreatePupilOverlay(rightEyeLocal, pupilSize);

            Debug.Log($"[BirthSystem] Landmark (face-tex): leftEye=({lm.leftEyeCenter.x:F2},{lm.leftEyeCenter.y:F2}) rightEye=({lm.rightEyeCenter.x:F2},{lm.rightEyeCenter.y:F2})");
            Debug.Log($"[BirthSystem] Landmark (composite): leftEye=({leftEyeComp.x:F3},{leftEyeComp.y:F3}) rightEye=({rightEyeComp.x:F3},{rightEyeComp.y:F3})");
            Debug.Log($"[BirthSystem] Pupil (clip-local): leftEye=({leftEyeLocal.x:F1},{leftEyeLocal.y:F1}) rightEye=({rightEyeLocal.x:F1},{rightEyeLocal.y:F1}) size={pupilSize:F1}");

            // ほっぺ位置もログ出力
            Vector2 leftCheekComp = FaceLandmarkToComposite(lm.leftCheek);
            Vector2 rightCheekComp = FaceLandmarkToComposite(lm.rightCheek);
            Debug.Log($"[BirthSystem] Cheek (composite): L=({leftCheekComp.x:F3},{leftCheekComp.y:F3}) R=({rightCheekComp.x:F3},{rightCheekComp.y:F3})");
        }
        else
        {
            Debug.Log("[BirthSystem] No landmarks — pupil overlay skipped");
        }
    }

    // --- 機能1: ほっぺ「ぷにぷに」変形 ---

    void OnBabyFaceTap(UIE.PointerDownEvent evt)
    {
        if (!puniInteractionEnabled || synthBabyImageEl == null) return;

        // 親の photoBtn にイベントが伝搬しないようにする
        evt.StopPropagation();

        // タップ位置を合成画像の正規化座標に変換
        Vector2 localPos = evt.localPosition;
        Vector2 compositeNorm = ImageLocalToComposite(localPos);

        // ほっぺの基準位置を合成画像空間で計算
        Vector2 leftCheekComp, rightCheekComp;
        float cheekThreshold;

        if (detectedFaceLandmarks.HasValue)
        {
            var lm = detectedFaceLandmarks.Value;
            leftCheekComp = FaceLandmarkToComposite(lm.leftCheek);
            rightCheekComp = FaceLandmarkToComposite(lm.rightCheek);
            // 閾値: 目間距離の合成画像空間での半分程度
            Vector2 leftEyeComp = FaceLandmarkToComposite(lm.leftEyeCenter);
            Vector2 rightEyeComp = FaceLandmarkToComposite(lm.rightEyeCenter);
            cheekThreshold = Vector2.Distance(leftEyeComp, rightEyeComp) * 0.5f;
        }
        else
        {
            // ランドマーク未検出時: 顔穴中心基準で疑似ほっぺ位置
            float cx = babySynthesizer != null ? babySynthesizer.GetFaceHoleCX() : 0.50f;
            float cy = babySynthesizer != null ? babySynthesizer.GetFaceHoleCY() : 0.68f;
            float rx = babySynthesizer != null ? babySynthesizer.GetFaceHoleRX() : 0.13f;
            leftCheekComp = new Vector2(cx - rx * 0.5f, cy + rx * 0.3f);
            rightCheekComp = new Vector2(cx + rx * 0.5f, cy + rx * 0.3f);
            cheekThreshold = rx * 0.6f;
        }

        // ほっぺとの距離判定
        float distLeft = Vector2.Distance(compositeNorm, leftCheekComp);
        float distRight = Vector2.Distance(compositeNorm, rightCheekComp);

        Debug.Log($"[BirthSystem] Tap at composite=({compositeNorm.x:F3},{compositeNorm.y:F3}) " +
                  $"distL={distLeft:F3} distR={distRight:F3} threshold={cheekThreshold:F3}");

        if (distLeft < cheekThreshold)
        {
            Debug.Log("[BirthSystem] Puni tap: left cheek");
            if (puniSquishCoroutine != null) StopCoroutine(puniSquishCoroutine);
            puniSquishCoroutine = StartCoroutine(PuniSquishAnimation(true));
            SpawnPuniRipple(localPos);
        }
        else if (distRight < cheekThreshold)
        {
            Debug.Log("[BirthSystem] Puni tap: right cheek");
            if (puniSquishCoroutine != null) StopCoroutine(puniSquishCoroutine);
            puniSquishCoroutine = StartCoroutine(PuniSquishAnimation(false));
            SpawnPuniRipple(localPos);
        }
    }

    IEnumerator PuniSquishAnimation(bool isLeft)
    {
        if (synthBabyImageEl == null) yield break;

        // SE再生（タップ音を高ピッチで「ぷに」感）
        if (seSource != null && seCardFlip != null)
        {
            seSource.pitch = 1.4f;
            seSource.PlayOneShot(seCardFlip, 0.4f);
            seSource.pitch = 1f;
        }

        float translateDir = isLeft ? 8f : -8f;

        // Phase 1: ぷにっとつぶれ（0 → 80ms）
        float phase1 = 0.08f;
        float elapsed = 0f;
        while (elapsed < phase1)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / phase1);
            float scaleX = Mathf.Lerp(1f, 0.94f, t);
            float scaleY = Mathf.Lerp(1f, 1.06f, t);
            float tx = Mathf.Lerp(0f, translateDir, t);
            synthBabyImageEl.style.scale = new UIE.StyleScale(
                new UIE.Scale(new Vector3(scaleX, scaleY, 1f)));
            synthBabyImageEl.style.translate = new UIE.StyleTranslate(
                new UIE.Translate(tx, 0));
            yield return null;
        }

        // Phase 2: オーバーシュート戻り（80ms → 200ms）
        float phase2 = 0.12f;
        elapsed = 0f;
        while (elapsed < phase2)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / phase2);
            float ease = t * t * (3f - 2f * t);
            float scaleX = Mathf.Lerp(0.94f, 1.02f, ease);
            float scaleY = Mathf.Lerp(1.06f, 0.98f, ease);
            float tx = Mathf.Lerp(translateDir, 0f, ease);
            synthBabyImageEl.style.scale = new UIE.StyleScale(
                new UIE.Scale(new Vector3(scaleX, scaleY, 1f)));
            synthBabyImageEl.style.translate = new UIE.StyleTranslate(
                new UIE.Translate(tx, 0));
            yield return null;
        }

        // Phase 3: 定位置に戻る（200ms → 300ms）
        float phase3 = 0.1f;
        elapsed = 0f;
        while (elapsed < phase3)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / phase3);
            float ease = t * t * (3f - 2f * t);
            float scaleX = Mathf.Lerp(1.02f, 1f, ease);
            float scaleY = Mathf.Lerp(0.98f, 1f, ease);
            synthBabyImageEl.style.scale = new UIE.StyleScale(
                new UIE.Scale(new Vector3(scaleX, scaleY, 1f)));
            yield return null;
        }

        // リセット
        synthBabyImageEl.style.scale = new UIE.StyleScale(
            new UIE.Scale(Vector3.one));
        synthBabyImageEl.style.translate = new UIE.StyleTranslate(
            new UIE.Translate(0, 0));
        puniSquishCoroutine = null;
    }

    /// <summary>
    /// ぷにぷにリップル波紋（顔クリップ内に生成）
    /// </summary>
    void SpawnPuniRipple(Vector2 imageLocalPos)
    {
        // クリップコンテナ内にリップルを配置（はみ出し部分は自動クリップ）
        UIE.VisualElement parent = faceInteractionClip != null ? faceInteractionClip : synthBabyImageEl;
        if (parent == null) return;

        // imageLocalPos → parent内のローカル座標に変換
        float rippleX = imageLocalPos.x;
        float rippleY = imageLocalPos.y;
        if (faceInteractionClip != null)
        {
            rippleX -= faceInteractionClip.resolvedStyle.left;
            rippleY -= faceInteractionClip.resolvedStyle.top;
        }

        var ripple = new UIE.VisualElement();
        ripple.AddToClassList("puni-ripple");
        ripple.pickingMode = UIE.PickingMode.Ignore;
        ripple.style.left = rippleX - 30f;
        ripple.style.top = rippleY - 30f;
        ripple.style.opacity = 0.6f;
        ripple.style.scale = new UIE.StyleScale(
            new UIE.Scale(new Vector3(0.3f, 0.3f, 1f)));
        parent.Add(ripple);

        StartCoroutine(PuniRippleAnimation(ripple));
    }

    IEnumerator PuniRippleAnimation(UIE.VisualElement ripple)
    {
        float duration = 0.35f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float ease = 1f - (1f - t) * (1f - t);
            float s = Mathf.Lerp(0.3f, 1.2f, ease);
            float a = Mathf.Lerp(0.6f, 0f, ease);
            ripple.style.scale = new UIE.StyleScale(
                new UIE.Scale(new Vector3(s, s, 1f)));
            ripple.style.opacity = a;
            yield return null;
        }
        ripple.RemoveFromHierarchy();
    }

    // --- 機能2: 視線追従 ---

    UIE.VisualElement CreatePupilOverlay(Vector2 centerPos, float size)
    {
        if (faceInteractionClip == null) return null;

        var pupil = new UIE.VisualElement();
        pupil.AddToClassList("baby-pupil-overlay");
        pupil.pickingMode = UIE.PickingMode.Ignore;
        pupil.style.width = size;
        pupil.style.height = size;
        pupil.style.left = centerPos.x - size / 2f;
        pupil.style.top = centerPos.y - size / 2f;
        faceInteractionClip.Add(pupil); // クリップコンテナ内に配置
        return pupil;
    }

    void OnGazePointerMove(UIE.PointerMoveEvent evt)
    {
        if (leftPupilEl == null || rightPupilEl == null || faceInteractionClip == null) return;

        // ポインタ位置をクリップコンテナのローカル座標に変換
        Vector2 pointerLocal = evt.localPosition;
        float clipL = faceInteractionClip.resolvedStyle.left;
        float clipT = faceInteractionClip.resolvedStyle.top;
        Vector2 pointerInClip = new Vector2(pointerLocal.x - clipL, pointerLocal.y - clipT);

        float clipW = faceInteractionClip.resolvedStyle.width;
        float maxOffset = clipW * 0.04f;
        maxOffset = Mathf.Clamp(maxOffset, 2f, 8f);

        UpdatePupilPosition(leftPupilEl, leftPupilBasePos, pointerInClip, maxOffset, true);
        UpdatePupilPosition(rightPupilEl, rightPupilBasePos, pointerInClip, maxOffset, false);
    }

    void UpdatePupilPosition(UIE.VisualElement pupil, Vector2 basePos, Vector2 pointerPos, float maxOffset, bool isLeft)
    {
        Vector2 dir = pointerPos - basePos;
        float dist = dir.magnitude;
        Vector2 off = Vector2.zero;
        if (dist > 0.01f)
        {
            dir /= dist;
            float o = Mathf.Min(dist * 0.1f, maxOffset);
            off = new Vector2(dir.x * o, dir.y * o);
        }
        if (isLeft) leftPupilOffset = off; else rightPupilOffset = off;
        pupil.style.translate = new UIE.StyleTranslate(
            new UIE.Translate(off.x, off.y));
    }

    void OnGazePointerLeave(UIE.PointerLeaveEvent evt)
    {
        StartCoroutine(ReturnPupilsToCenter());
    }

    IEnumerator ReturnPupilsToCenter()
    {
        if (leftPupilEl == null && rightPupilEl == null) yield break;

        float lx = leftPupilOffset.x, ly = leftPupilOffset.y;
        float rx = rightPupilOffset.x, ry = rightPupilOffset.y;

        float duration = 0.2f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float ease = t * t * (3f - 2f * t);

            if (leftPupilEl != null)
                leftPupilEl.style.translate = new UIE.StyleTranslate(
                    new UIE.Translate(
                        Mathf.Lerp(lx, 0f, ease),
                        Mathf.Lerp(ly, 0f, ease)));
            if (rightPupilEl != null)
                rightPupilEl.style.translate = new UIE.StyleTranslate(
                    new UIE.Translate(
                        Mathf.Lerp(rx, 0f, ease),
                        Mathf.Lerp(ry, 0f, ease)));

            yield return null;
        }

        if (leftPupilEl != null)
            leftPupilEl.style.translate = new UIE.StyleTranslate(new UIE.Translate(0, 0));
        if (rightPupilEl != null)
            rightPupilEl.style.translate = new UIE.StyleTranslate(new UIE.Translate(0, 0));
        leftPupilOffset = Vector2.zero;
        rightPupilOffset = Vector2.zero;
    }

    // --- 機能3: 診断スコア生成 ---

    static readonly string[] diagnosisCategoryKeys = {
        "diagnosis_oil_king",
        "diagnosis_idol",
        "diagnosis_scientist",
        "diagnosis_adventurer",
        "diagnosis_angel",
        "diagnosis_ruler",
        "diagnosis_artist",
        "diagnosis_lucky"
    };

    string CalculateDiagnosis()
    {
        var dc = DataCarrier.Instance;
        var lm = detectedFaceLandmarks;

        // ランドマーク比率の計算（検出あり時のみボーナス）
        float faceWidthRatio = lm.HasValue ? lm.Value.faceBounds.width : 0.5f;
        float eyeDistRatio = lm.HasValue ? lm.Value.EyeDistance : 0.1f;
        float foreheadRatio = lm.HasValue && lm.Value.faceBounds.height > 0 ?
            (lm.Value.leftEyeCenter.y - lm.Value.faceBounds.y) / lm.Value.faceBounds.height : 0.3f;
        float faceSizeRatio = lm.HasValue ?
            lm.Value.faceBounds.width * lm.Value.faceBounds.height : 0.1f;

        // 各カテゴリのスコア計算
        float[] scores = new float[8];
        scores[0] = dc.babyFortune * 2f + faceWidthRatio * 100f;          // 石油王
        scores[1] = dc.babyLuck * 2f + eyeDistRatio * 500f;               // アイドル
        scores[2] = dc.babyIntelligence * 2f + foreheadRatio * 200f;      // 天才科学者
        scores[3] = dc.babyAtk + dc.babyAthletic * 1.5f;                  // ぼうけんか
        scores[4] = dc.babyDef * 1.5f + dc.babyHp;                        // いやしの天使
        scores[5] = dc.babyAtk * 2f + faceSizeRatio * 500f;               // 覇王
        scores[6] = dc.babyIntelligence + dc.babyLuck * 1.5f;             // アーティスト
        scores[7] = dc.babyLuck + dc.babyFortune * 1.5f;                  // ラッキースター

        // 最大スコアのカテゴリを選択
        int maxIdx = 0;
        for (int i = 1; i < 8; i++)
            if (scores[i] > scores[maxIdx]) maxIdx = i;

        Debug.Log($"[BirthSystem] Diagnosis: {diagnosisCategoryKeys[maxIdx]} (score={scores[maxIdx]:F1})");
        return diagnosisCategoryKeys[maxIdx];
    }

    void ShowDiagnosisModal()
    {
        if (overlayRoot == null || diagnosisOverlayEl != null) return;

        string categoryKey = CalculateDiagnosis();

        // SE再生
        if (seSource != null && seKirakira != null)
            seSource.PlayOneShot(seKirakira, 0.5f);

        // 半透明黒背景
        diagnosisOverlayEl = new UIE.VisualElement();
        diagnosisOverlayEl.AddToClassList("birth-diagnosis-overlay");
        diagnosisOverlayEl.pickingMode = UIE.PickingMode.Position; // イベントをブロック（背面操作防止）
        diagnosisOverlayEl.style.opacity = 0f;

        // モーダルカード
        var card = new UIE.VisualElement();
        card.AddToClassList("birth-diagnosis-card");

        // タイトル
        var titleLabel = new UIE.Label();
        titleLabel.AddToClassList("birth-diagnosis-title");
        UIHelper.ApplyFontBold(titleLabel);
        titleLabel.text = Localization.Get("diagnosis_title");
        card.Add(titleLabel);

        // 前置きテキスト
        var prefixLabel = new UIE.Label();
        prefixLabel.AddToClassList("birth-diagnosis-prefix");
        UIHelper.ApplyFontBold(prefixLabel);
        prefixLabel.text = "";
        card.Add(prefixLabel);

        // 結果テキスト
        var resultLabel = new UIE.Label();
        resultLabel.AddToClassList("birth-diagnosis-result");
        UIHelper.ApplyFontBold(resultLabel);
        resultLabel.text = "";
        resultLabel.style.opacity = 0f;
        card.Add(resultLabel);

        // 閉じるボタン
        var closeBtn = new UIE.Button();
        closeBtn.AddToClassList("birth-diagnosis-close-btn");
        UIHelper.ApplyFont(closeBtn);
        closeBtn.text = Localization.Get("ui_close");
        closeBtn.clicked += CloseDiagnosisModal;
        card.Add(closeBtn);

        diagnosisOverlayEl.Add(card);
        overlayRoot.Add(diagnosisOverlayEl);

        // フェードイン + タイプライター演出
        if (diagnosisTypewriterCoroutine != null)
            StopCoroutine(diagnosisTypewriterCoroutine);
        diagnosisTypewriterCoroutine = StartCoroutine(
            DiagnosisRevealAnimation(prefixLabel, resultLabel, categoryKey));
    }

    IEnumerator DiagnosisRevealAnimation(UIE.Label prefixLabel, UIE.Label resultLabel, string categoryKey)
    {
        // フェードイン
        float fadeIn = 0.3f;
        float elapsed = 0f;
        while (elapsed < fadeIn)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeIn);
            float ease = 1f - (1f - t) * (1f - t);
            if (diagnosisOverlayEl != null)
                diagnosisOverlayEl.style.opacity = ease;
            yield return null;
        }
        if (diagnosisOverlayEl != null)
            diagnosisOverlayEl.style.opacity = 1f;

        // 前置きテキストのタイプライター
        string prefix = Localization.Get("diagnosis_prefix");
        for (int i = 0; i <= prefix.Length; i++)
        {
            prefixLabel.text = prefix.Substring(0, i);
            yield return new WaitForSeconds(0.06f);
        }

        yield return new WaitForSeconds(0.3f);

        // 結果をドカンと表示
        string result = Localization.Get(categoryKey);
        resultLabel.text = result;

        // SE
        if (seSource != null && seLevelUp != null)
            seSource.PlayOneShot(seLevelUp, 0.6f);

        // スケールバウンスで登場
        resultLabel.style.opacity = 1f;
        resultLabel.style.scale = new UIE.StyleScale(
            new UIE.Scale(new Vector3(0.3f, 0.3f, 1f)));

        float bounce = 0.4f;
        elapsed = 0f;
        while (elapsed < bounce)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / bounce);
            float p = 0.4f;
            float s = Mathf.Pow(2f, -10f * t) * Mathf.Sin((t - p / 4f) * (2f * Mathf.PI) / p) + 1f;
            resultLabel.style.scale = new UIE.StyleScale(
                new UIE.Scale(new Vector3(s, s, 1f)));
            yield return null;
        }
        resultLabel.style.scale = new UIE.StyleScale(
            new UIE.Scale(Vector3.one));

        diagnosisTypewriterCoroutine = null;
    }

    void CloseDiagnosisModal()
    {
        if (diagnosisTypewriterCoroutine != null)
        {
            StopCoroutine(diagnosisTypewriterCoroutine);
            diagnosisTypewriterCoroutine = null;
        }

        if (diagnosisOverlayEl != null)
        {
            diagnosisOverlayEl.RemoveFromHierarchy();
            diagnosisOverlayEl = null;
        }
    }

    // ===== EXIF 回転補正（Editor用） =====
#if UNITY_EDITOR
    /// <summary>
    /// JPEG の EXIF Orientation を読み取り、テクスチャのピクセルを回転・反転して補正する。
    /// iOS/Android ではネイティブ側で処理されるが、Editor では LoadImage が EXIF を無視するため必要。
    /// </summary>
    static Texture2D CorrectExifOrientation(Texture2D tex, string path)
    {
        int orientation = ReadJpegExifOrientation(path);
        Debug.Log($"[BirthSystem] EXIF orientation={orientation} for {Path.GetFileName(path)} (tex={tex.width}x{tex.height})");
        if (orientation <= 1 || orientation > 8) return tex;

        // ★ 二重回転防止: orientation 5-8 は90°/270°回転（W/Hが入れ替わる）
        // NativeGallery が既に回転を適用している場合、テクスチャが
        // 回転後の縦横比になっている → スキップ
        if (orientation >= 5 && tex.width != tex.height)
        {
            // 例: orientation=6(90°CW) → 元データ landscape → 表示 portrait
            // NativeGallery適用済み: portrait (h > w) → 回転スキップ
            // NativeGallery未適用: landscape (w > h) → 回転必要
            if (tex.height > tex.width)
            {
                Debug.Log($"[BirthSystem] EXIF skip: orientation={orientation} but tex already portrait ({tex.width}x{tex.height}) — likely pre-rotated");
                return tex;
            }
        }

        return ApplyExifRotation(tex, orientation);
    }

    static int ReadJpegExifOrientation(string path)
    {
        try
        {
            byte[] data;
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                int readLen = (int)Mathf.Min(fs.Length, 65536);
                data = new byte[readLen];
                fs.Read(data, 0, readLen);
            }
            if (data.Length < 12) return 1;
            if (data[0] != 0xFF || data[1] != 0xD8) return 1;

            int offset = 2;
            while (offset < data.Length - 4)
            {
                if (data[offset] != 0xFF) break;
                byte marker = data[offset + 1];

                if (marker == 0xE1) // APP1 (EXIF)
                {
                    int exifStart = offset + 4;
                    if (exifStart + 6 > data.Length) return 1;
                    if (data[exifStart] != 0x45 || data[exifStart + 1] != 0x78 ||
                        data[exifStart + 2] != 0x69 || data[exifStart + 3] != 0x66) return 1;

                    int tiffStart = exifStart + 6;
                    if (tiffStart + 8 > data.Length) return 1;
                    bool bigEndian = data[tiffStart] == 0x4D;

                    int ifdOffset = ExifReadInt32(data, tiffStart + 4, bigEndian);
                    int ifdPos = tiffStart + ifdOffset;
                    if (ifdPos + 2 > data.Length) return 1;

                    int entryCount = ExifReadInt16(data, ifdPos, bigEndian);
                    ifdPos += 2;
                    for (int i = 0; i < entryCount; i++)
                    {
                        if (ifdPos + 12 > data.Length) break;
                        int tag = ExifReadInt16(data, ifdPos, bigEndian);
                        if (tag == 0x0112) return ExifReadInt16(data, ifdPos + 8, bigEndian);
                        ifdPos += 12;
                    }
                    return 1;
                }
                else if (marker == 0xDA) break;
                else
                {
                    int segLen = (data[offset + 2] << 8) | data[offset + 3];
                    offset += 2 + segLen;
                }
            }
        }
        catch (System.Exception) { }
        return 1;
    }

    static int ExifReadInt16(byte[] d, int o, bool big) =>
        big ? (d[o] << 8) | d[o + 1] : d[o] | (d[o + 1] << 8);

    static int ExifReadInt32(byte[] d, int o, bool big) =>
        big ? (d[o] << 24) | (d[o + 1] << 16) | (d[o + 2] << 8) | d[o + 3]
            : d[o] | (d[o + 1] << 8) | (d[o + 2] << 16) | (d[o + 3] << 24);

    static Texture2D ApplyExifRotation(Texture2D src, int orientation)
    {
        Color[] srcPx = src.GetPixels();
        int w = src.width, h = src.height;
        Texture2D dst;
        Color[] dstPx;

        switch (orientation)
        {
            case 6: // 90° CW（縦撮りで最も一般的）
                dst = new Texture2D(h, w, TextureFormat.RGBA32, false);
                dstPx = new Color[w * h];
                for (int y = 0; y < w; y++)
                    for (int x = 0; x < h; x++)
                        dstPx[y * h + x] = srcPx[x * w + (w - 1 - y)];
                break;
            case 8: // 90° CCW
                dst = new Texture2D(h, w, TextureFormat.RGBA32, false);
                dstPx = new Color[w * h];
                for (int y = 0; y < w; y++)
                    for (int x = 0; x < h; x++)
                        dstPx[y * h + x] = srcPx[(h - 1 - x) * w + y];
                break;
            case 3: // 180°
                dst = new Texture2D(w, h, TextureFormat.RGBA32, false);
                dstPx = new Color[w * h];
                for (int y = 0; y < h; y++)
                    for (int x = 0; x < w; x++)
                        dstPx[y * w + x] = srcPx[(h - 1 - y) * w + (w - 1 - x)];
                break;
            case 2: // 水平反転
                dst = new Texture2D(w, h, TextureFormat.RGBA32, false);
                dstPx = new Color[w * h];
                for (int y = 0; y < h; y++)
                    for (int x = 0; x < w; x++)
                        dstPx[y * w + x] = srcPx[y * w + (w - 1 - x)];
                break;
            case 4: // 垂直反転
                dst = new Texture2D(w, h, TextureFormat.RGBA32, false);
                dstPx = new Color[w * h];
                for (int y = 0; y < h; y++)
                    for (int x = 0; x < w; x++)
                        dstPx[y * w + x] = srcPx[(h - 1 - y) * w + x];
                break;
            case 5: // Transpose
                dst = new Texture2D(h, w, TextureFormat.RGBA32, false);
                dstPx = new Color[w * h];
                for (int y = 0; y < w; y++)
                    for (int x = 0; x < h; x++)
                        dstPx[y * h + x] = srcPx[x * w + y];
                break;
            case 7: // Transverse
                dst = new Texture2D(h, w, TextureFormat.RGBA32, false);
                dstPx = new Color[w * h];
                for (int y = 0; y < w; y++)
                    for (int x = 0; x < h; x++)
                        dstPx[y * h + x] = srcPx[(h - 1 - x) * w + (w - 1 - y)];
                break;
            default:
                return src;
        }

        dst.SetPixels(dstPx);
        dst.Apply();
        Debug.Log($"[BirthSystem] EXIF rotation applied: orientation={orientation}, {w}x{h} → {dst.width}x{dst.height}");
        UnityEngine.Object.Destroy(src);
        return dst;
    }
#endif
}

public struct ParentData
{
    public string name;
    public string imageName; // 英語名（小文字）
    public int atk, def, hp, intelligence, athletic, luck, fortune;
    public Color faceColor;
    public string intro;

    public ParentData(string name, string imageName, int atk, int def, int hp, int intelligence, int athletic, int luck, int fortune, Color faceColor, string intro)
    {
        this.name = name;
        this.imageName = imageName;
        this.atk = atk;
        this.def = def;
        this.hp = hp;
        this.intelligence = intelligence;
        this.athletic = athletic;
        this.luck = luck;
        this.fortune = fortune;
        this.faceColor = faceColor;
        this.intro = intro;
    }
}

public class PachinkoButtonEffect : MonoBehaviour
{
    float time;
    Transform glowTf;
    Image glowImg;
    Transform raysTf;
    Transform[] sparkleTfs;
    Image[] sparkleImgs;

    void Start()
    {
        // 子要素を名前で取得
        var glow = transform.Find("Glow");
        if (glow != null)
        {
            glowTf = glow;
            glowImg = glow.GetComponent<Image>();
        }
        var rays = transform.Find("Rays");
        if (rays != null) raysTf = rays;

        // Sparkleを収集
        var sparkles = new System.Collections.Generic.List<Transform>();
        var sparkleImgList = new System.Collections.Generic.List<Image>();
        for (int i = 0; i < 4; i++)
        {
            var sp = transform.Find($"Sparkle{i}");
            if (sp != null)
            {
                sparkles.Add(sp);
                sparkleImgList.Add(sp.GetComponent<Image>());
            }
        }
        sparkleTfs = sparkles.ToArray();
        sparkleImgs = sparkleImgList.ToArray();
    }

    // パステルきらめきカラー（ピンク、ミント、メイン、ホワイト）
    static readonly Color[] pastelSparkleColors = {
        new Color(1f, 0.72f, 0.77f, 1f),
        new Color(0.67f, 0.94f, 0.82f, 1f),
        new Color(0.97f, 0.91f, 0.81f, 1f),
        new Color(1f, 1f, 1f, 1f),
    };

    void Update()
    {
        time += Time.deltaTime;

        // グローの脈動（ソフトピンク）
        if (glowTf != null && glowImg != null)
        {
            float pulse = 1f + 0.10f * Mathf.Sin(time * 2f);
            glowTf.localScale = new Vector3(pulse, pulse, 1f);
            float alpha = 0.25f + 0.12f * Mathf.Sin(time * 2f);
            glowImg.color = new Color(1f, 0.72f, 0.78f, alpha);
        }

        // 光線の回転（ゆっくり）
        if (raysTf != null)
        {
            raysTf.localRotation = Quaternion.Euler(0, 0, time * 10f);
        }

        // きらめき：回転 + サイズ脈動 + パステルカラー点滅
        if (sparkleTfs != null)
        {
            for (int i = 0; i < sparkleTfs.Length; i++)
            {
                float offset = i * 1.57f;
                float angle = time * 1.0f + offset;
                float dist = 185f + 10f * Mathf.Sin(time * 2.5f + offset);
                sparkleTfs[i].GetComponent<RectTransform>().anchoredPosition =
                    new Vector2(Mathf.Cos(angle) * dist, Mathf.Sin(angle) * dist);

                float sparkleScale = 0.8f + 0.5f * Mathf.Sin(time * 4f + offset);
                sparkleTfs[i].localScale = new Vector3(sparkleScale, sparkleScale, 1f);

                if (sparkleImgs != null && i < sparkleImgs.Length)
                {
                    float sa = 0.5f + 0.5f * Mathf.Sin(time * 3.5f + offset);
                    var baseColor = pastelSparkleColors[i % pastelSparkleColors.Length];
                    sparkleImgs[i].color = new Color(baseColor.r, baseColor.g, baseColor.b, sa);
                }
            }
        }
    }
}

