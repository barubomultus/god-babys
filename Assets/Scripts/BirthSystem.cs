using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class BirthSystem : MonoBehaviour
{
    [Header("UI Reference")]
    public TextMeshProUGUI childStatusText;
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

    // 自動生成される親UI
    GameObject parentPanel;
    Image fatherFaceImage;
    TextMeshProUGUI fatherNameText;
    TextMeshProUGUI fatherIntroText;
    Image motherFaceImage;
    TextMeshProUGUI motherNameText;
    TextMeshProUGUI motherIntroText;

    // フラッシュ演出用
    Image flashOverlay;

    // 稲妻演出用
    GameObject lightningContainer;

    // テキスト背景用
    GameObject statusTextBackground;

    // ?マーク
    TextMeshProUGUI introText;
    GameObject introPanel;

    // アニメーション中フラグ
    bool isAnimating;

    // 性別選択UI
    GameObject genderSelectPanel;
    string selectedGender;
    bool waitingForGenderSelect;

    // 名前入力UI
    GameObject nameInputPanel;
    TMP_InputField nameInputField;
    string enteredName;
    bool waitingForNameInput;

    // セーブ確認UI
    GameObject saveConfirmPanel;
    bool waitingForSaveConfirm;
    bool saveConfirmResult;

    // 父親6パターン
    static readonly ParentData[] Fathers = new[]
    {
        new ParentData("タケシ",   "takeshi", 70, 30, 180, 178, 40, 80, 85, new Color(0.9f, 0.7f, 0.5f), "元・格闘技世界王者 / 握力: 180kg"),
        new ParentData("ユウキ",   "yuuki",   50, 50, 150, 172, 70, 68, 60, new Color(0.6f, 0.8f, 1.0f), "天才ハッカー / 特許数: 3,200件"),
        new ParentData("ゴウ",     "gou",     80, 25, 190, 185, 30, 88, 90, new Color(1.0f, 0.5f, 0.4f), "伝説の傭兵 / 戦闘力: 計測不能"),
        new ParentData("シンジ",   "shinji",  30, 70, 140, 168, 95, 62, 35, new Color(0.7f, 0.7f, 1.0f), "ノーベル賞3回受賞 / IQ: 250"),
        new ParentData("リョウマ", "ryouma",  60, 60, 170, 180, 55, 75, 70, new Color(0.5f, 1.0f, 0.6f), "総資産: 43兆円 / 世界一の実業家"),
        new ParentData("テツヤ",   "tetuya", 45, 45, 160, 170, 60, 72, 55, new Color(1.0f, 0.9f, 0.5f), "伝説のロックスター / ファン数: 8億人"),
    };

    // 母親6パターン
    static readonly ParentData[] Mothers = new[]
    {
        new ParentData("サクラ",   "sakura",  25, 70, 130, 158, 85, 50, 40, new Color(1.0f, 0.7f, 0.8f), "天才外科医 / 手術成功率: 100%"),
        new ParentData("ヒナタ",   "hinata",  40, 60, 150, 162, 60, 55, 65, new Color(0.8f, 0.6f, 1.0f), "暗殺拳の継承者 / 全戦全勝"),
        new ParentData("アキラ",   "akira",   65, 30, 160, 170, 45, 58, 80, new Color(1.0f, 0.6f, 0.4f), "五輪金メダル7個 / 100m走: 10.1秒"),
        new ParentData("ミサト",   "misato",  35, 55, 140, 155, 90, 48, 30, new Color(0.6f, 0.9f, 1.0f), "量子物理学者 / IQ: 270"),
        new ParentData("カエデ",   "kaede",   50, 50, 145, 165, 70, 52, 60, new Color(0.5f, 1.0f, 0.7f), "総資産: 28兆円 / 美容帝国CEO"),
        new ParentData("ルナ",     "luna",    55, 40, 135, 160, 50, 46, 70, new Color(1.0f, 1.0f, 0.6f), "世界的スーパーモデル / 身長: 180cm"),
    };

    // 特徴リスト
    static readonly string[] Traits =
    {
        "天才肌", "努力家", "頑丈", "すばしっこい", "おだやか",
        "あまえんぼう", "なきむし", "くいしんぼう", "好奇心旺盛", "マイペース",
        "負けず嫌い", "やさしい", "ワイルド", "ミステリアス",
    };

    // 36通りの恋愛ストーリー（父親名_母親名 → ストーリー）
    static readonly System.Collections.Generic.Dictionary<string, string> LoveStories = new System.Collections.Generic.Dictionary<string, string>
    {
        // タケシ（格闘家）× 各母親
        {"タケシ_サクラ", "世界格闘技選手権の決勝戦。\nタケシは宿敵との死闘の末、\n右腕を複雑骨折した。\n\n「二度と戦えない」と宣告される中、\n唯一の希望は天才外科医サクラだった。\n\n12時間に及ぶ手術。\n目覚めたタケシの最初の言葉は\n「俺の腕を救ってくれた君を、\n俺の人生に迎えたい」だった。"},
        {"タケシ_ヒナタ", "裏格闘技界の頂点を決める戦い。\nタケシとヒナタは決勝で激突した。\n\n拳と暗殺拳が交錯する中、\n二人は互いの強さに惹かれていく。\n\n死闘は引き分けに終わり、\n「決着は別の形でつけよう」と\nタケシが差し出した手を、\nヒナタは静かに握り返した。\n最強の血統がここに誕生する。"},
        {"タケシ_アキラ", "オリンピック選手村での出会い。\n格闘技代表のタケシと\n陸上代表のアキラ。\n\n食堂で偶然隣り合わせになり、\n互いの鍛え抜かれた肉体に\n目を奪われた。\n\n「一緒にトレーニングしないか？」\nその一言から始まった朝練は、\nいつしか二人だけの時間に変わり、\n閉会式の夜、二人は結ばれた。"},
        {"タケシ_ミサト", "「筋肉の収縮は量子力学で\n説明できるんですよ」\n\n学会に招かれたタケシに、\nミサトは熱心に語りかけた。\n\n「難しいことはわからねえが、\nあんたの目は本気だな」\n\n理論と実践、正反対の二人。\nだが夜通し語り合ううちに、\n科学者の心は格闘家に奪われ、\n最強の頭脳と肉体が融合した。"},
        {"タケシ_カエデ", "「格闘家専用コスメを作りたい」\nカエデからの突然の依頼。\n\nビジネスミーティングのはずが、\nタケシの素朴な優しさに触れ、\nカエデの心は揺れ始める。\n\n「数字じゃ測れないものがある」\nタケシの言葉に、\n28兆円の帝国を築いた女は\n初めて涙を流した。\n愛は最高の投資だと知った日。"},
        {"タケシ_ルナ", "スポーツ雑誌の表紙撮影。\n格闘家とスーパーモデルの共演。\n\nカメラの前で火花が散り、\n「もっと近づいて」という\nカメラマンの指示に、\n二人の心臓が高鳴る。\n\n撮影後、ルナが言った。\n「あなたの隣にいると、\n自分が美しく見える気がする」\nスポットライトの下で恋が始まった。"},

        // ユウキ（ハッカー）× 各母親
        {"ユウキ_サクラ", "大病院のシステムがハッキングされた。\n犯人を追うサクラの前に現れたのは、\nセキュリティ専門家のユウキだった。\n\n夜通しの作業、\nコードを書く指とメスを握る指が\n偶然触れ合った瞬間、\n二人は目を合わせた。\n\n「君の手は人を救う手だ」\n「あなたの手もよ」\n異なる世界の天才が、\n同じ未来を見つめ始めた。"},
        {"ユウキ_ヒナタ", "暗殺組織のサーバーに侵入した夜、\nユウキは追手に囲まれた。\n\nその中にいたのがヒナタ。\n「殺すつもりはない。\nあなたの腕が必要なの」\n\n組織を裏切り、共に逃亡する日々。\n追われる中で芽生えた信頼は、\nいつしか愛に変わっていた。\n\n「俺のファイアウォールは\n君だけ通過できる」\n不器用な告白だった。"},
        {"ユウキ_アキラ", "アスリート向けAIトレーナーの開発中、\nテストランナーとして\nアキラが研究所に現れた。\n\n「データが全然取れない...\n君は規格外すぎる」\n困惑するユウキに、\nアキラは笑って言った。\n\n「じゃあ毎日来てあげる」\n\nデータ収集という名目の\nデートが始まり、\n数値では測れない感情が芽生えた。"},
        {"ユウキ_ミサト", "量子コンピュータの共同研究。\n世界最高峰の頭脳が二つ、\n同じ研究室に集まった。\n\n夜通しのプログラミング、\nコーヒーカップが触れ合う音、\n「この暗号、解ける？」\n「君となら、どんな問題でも」\n\n二人だけの言語で愛を語り、\n論文より大切な答えを見つけた。\nそれは「共に生きる」という\nシンプルな真実だった。"},
        {"ユウキ_カエデ", "美容帝国のDX化プロジェクト。\n億単位の契約書を前に、\nユウキは言った。\n\n「報酬はいらない。\nその代わり、週に一度\n食事に付き合ってほしい」\n\n最初は呆れていたカエデも、\n彼の純粋さに惹かれていく。\n\n「私に値段をつけない人は\n初めてよ」\n28兆円より価値ある愛を知った。"},
        {"ユウキ_ルナ", "SNSで炎上したルナ。\n誹謗中傷の嵐の中、\n匿名の誰かが彼女を守り続けた。\n\n悪質な投稿を消し、\n真実を広め、\n見えない騎士のように戦った。\n\nある日、IPアドレスを辿ったルナは\nユウキを見つけた。\n「なぜ私のために？」\n「君の笑顔を守りたかった」\nその日、二人は恋人になった。"},

        // ゴウ（傭兵）× 各母親
        {"ゴウ_サクラ", "戦場で倒れた仲間を救うため、\nゴウは国境を越えて\n天才外科医を探した。\n\n「報酬はいくらでも払う」\n「お金じゃないの。\nあなたが連れてきて」\n\n危険な戦地に飛び込んだサクラ。\n命がけの手術を終えた夜、\nゴウは初めて泣いた。\n\n「俺の人生を守ってくれないか」\n傭兵の不器用なプロポーズだった。"},
        {"ゴウ_ヒナタ", "暗殺任務で鉢合わせた二人。\n互いに銃口を向けながら、\n奇妙な沈黙が流れた。\n\n「お前を殺す理由がない」\n「私もよ」\n\n銃を下ろした瞬間、\n組織に追われる身となった。\n\n「一緒に逃げないか」\n「どこまでも」\n\n世界中を逃げ回る日々が、\n二人を離れられない関係にした。"},
        {"ゴウ_アキラ", "紛争地帯でのスポーツ親善大使。\nアキラの警護を任されたゴウは、\n彼女の無邪気さに戸惑った。\n\n「怖くないのか？」\n「あなたがいるから」\n\n銃声の中でも笑顔を絶やさない彼女。\n守るべき存在が、\nいつしか愛する人に変わっていた。\n\n任務終了の日、\nゴウは傭兵を辞める決意をした。"},
        {"ゴウ_ミサト", "軍事衛星のデータ解析依頼。\n冷徹な傭兵ゴウと、\n純粋な物理学者ミサト。\n\n「なぜ人を殺すの？」\n直球の質問に、\nゴウは言葉を失った。\n\n「...答えが見つからない」\n「一緒に探しましょう」\n\nミサトの純粋さが、\n凍った心を少しずつ溶かしていく。\n戦場の狼が愛を知った瞬間だった。"},
        {"ゴウ_カエデ", "要人警護の任務。\n標的にされたのはカエデだった。\n\n三度の暗殺未遂、\nその全てからゴウは彼女を守った。\n三発目の銃弾を\n自らの体で受け止めた時、\nカエデは悟った。\n\n「お金じゃ買えないものがある」\n\n病室で目覚めたゴウに、\n彼女は涙ながらに言った。\n「私の人生を守って」"},
        {"ゴウ_ルナ", "戦場カメラマンとして同行したルナ。\n「真実を伝えたい」という\n彼女の覚悟に、ゴウは驚いた。\n\n砲撃の夜、塹壕で肩を寄せ合い、\n生と死の狭間で\n二人は唇を重ねた。\n\n「生きて帰ろう」\n「ああ、一緒にな」\n\n戦場で誓った愛は、\nどんな平和な恋より強く、\n深く結ばれていた。"},

        // シンジ（天才科学者）× 各母親
        {"シンジ_サクラ", "ノーベル医学賞授賞式。\n偶然隣り合わせになった二人は、\n授賞式そっちのけで議論を始めた。\n\n「君の論文、3箇所間違ってる」\n「あなたこそ、5箇所よ」\n\n火花を散らす天才同士。\nだがパーティーが終わる頃には、\n互いを認め合っていた。\n\n「共同研究しないか」\n「いいわ、人生のパートナーとして」\n人類最高の遺伝子が誕生した。"},
        {"シンジ_ヒナタ", "「暗殺拳の科学的解明」\nその研究テーマに、\nヒナタは協力を申し出た。\n\n動きを解析するうちに、\nシンジの目は彼女自身に向いていた。\n\n「論文より君を研究したい」\n「それ、口説いてる？」\n「...多分」\n\n世界一不器用な告白に、\n暗殺者は初めて頬を染めた。\n愛は科学で証明できないと知った。"},
        {"シンジ_アキラ", "「人体の限界」を科学する研究。\n被験者として現れたアキラの\n笑顔を見た瞬間、\nシンジの心拍データは乱れた。\n\n「先生、大丈夫？」\n「い、異常値が出ている...\n僕の心臓に」\n\n「それ、恋って言うんですよ」\nアキラの言葉に、\n天才科学者は顔を真っ赤にした。\n答えは最初から出ていたのだ。"},
        {"シンジ_ミサト", "国際物理学会での激論。\n「あなたの理論は穴だらけよ」\n「君こそ基礎が甘い」\n\n壇上で火花を散らした二人は、\nなぜかホテルのバーで再会した。\n\nIQ250とIQ270。\n合わせて520の恋が始まる。\n\n「数式より美しいものを見つけた」\n「何？」\n「君だよ」\n天才にしては陳腐な台詞だった。"},
        {"シンジ_カエデ", "「美の方程式」を共著で出版したい。\nカエデからの依頼に、\nシンジは興味を持った。\n\n数式とビジネス、\n異色のコラボレーション。\n\nグラフを描くうちに、\n二人の線は一点で交わった。\n\n「この交点が僕たちの未来だ」\n「ロマンチストね、意外と」\n\n28兆円の女帝が、\n数式に恋をした日だった。"},
        {"シンジ_ルナ", "「完璧な顔の数学的定義」\nその研究のため、\nルナがモデルとして協力した。\n\n何百枚もの写真、\n何千ものデータポイント。\n\n「結論が出たよ」\n「どんな顔が完璧なの？」\n「君だ。君以外にない」\n\n論文には書けない結論だった。\n美しさの究極の答えは、\n愛する人の顔だと気づいた。"},

        // リョウマ（実業家）× 各母親
        {"リョウマ_サクラ", "病院チェーンのM&A交渉。\nビジネスランチのはずが、\n話は医療の未来へと広がった。\n\n「君の理想を実現するには\nいくら必要だ？」\n「お金の問題じゃないの」\n\nその言葉に、リョウマは衝撃を受けた。\n43兆円の資産が無意味に思えた。\n\n「なら、僕の人生を投資させてくれ」\n契約書にない想いを込めた。"},
        {"リョウマ_ヒナタ", "ボディガードとして雇った暗殺者。\n命を預けた相手に、\n心まで奪われるとは思わなかった。\n\n「金で動く女か」\n「いいえ、あなたを守りたいから」\n\n嘘のない瞳だった。\n\n43兆円あっても買えないもの。\nそれは信頼と愛だと、\nリョウマは初めて知った。\n「俺の傍にいてくれ、永遠に」"},
        {"リョウマ_アキラ", "スポーツ球団買収の記者会見。\n看板選手アキラとの握手の瞬間、\n世界一の資産家は恋に落ちた。\n\n「君をチームの顔にしたい」\n「顔じゃなくて、\n私を見てほしいな」\n\n真っ直ぐな言葉が胸を打った。\n\n株価より大切なもの。\n利益より価値あるもの。\nそれは彼女の笑顔だった。"},
        {"リョウマ_ミサト", "研究所への100億円の投資。\nその見返りに求めたのは、\n論文でも特許でもなかった。\n\n「週に一度、\n一緒に星を見てほしい」\n\nミサトは驚きながらも頷いた。\n\n屋上で星を眺める夜が続き、\n宇宙の話から人生の話へ。\n\n「君という星を見つけた」\n物理学者は、\nその方程式を解けなかった。"},
        {"リョウマ_カエデ", "美容帝国との合併話。\n二つの巨大企業、\n最初は敵対から始まった。\n\n「あなたには負けないわ」\n「俺もだ」\n\n激しい交渉の末、\n二人は互いを認め合った。\n\n「合併より、\n結婚しないか」\n「...それ、逆じゃない？」\n\n71兆円の帝国が誕生した。\n株式より価値ある絆と共に。"},
        {"リョウマ_ルナ", "プライベートジェットで偶然の隣席。\nパリへ向かう12時間、\n二人は語り合った。\n\n仕事のこと、夢のこと、\n誰にも言えない弱さのこと。\n\n雲の上、地上から離れた空間で、\n肩書きも資産も意味を失った。\n\n着陸した時、\n二人は恋人になっていた。\n「地上に降りても、この気持ちは変わらない」"},

        // テツヤ（ロックスター）× 各母親
        {"テツヤ_サクラ", "ライブ中に声が出なくなった。\n「二度と歌えない」\n絶望するテツヤを救ったのは、\n天才外科医サクラだった。\n\n奇跡の手術から3ヶ月、\n声を取り戻した日、\nテツヤは病院でゲリラライブを開いた。\n\n「最初の歌は君に捧げる」\nそれは愛の歌だった。\nサクラは涙を流しながら聴いていた。"},
        {"テツヤ_ヒナタ", "新曲MVの殺陣シーン。\n指導者として現れたヒナタの\n鋭い動きに、テツヤは見惚れた。\n\n「もっと本気で来て」\n「怪我させるぞ」\n「それくらいが丁度いい」\n\nステージで刃を交えるうちに、\n二人の距離は縮まっていった。\n\n撮影終了後の楽屋で、\n二人は激しく唇を重ねた。"},
        {"テツヤ_アキラ", "オリンピック応援ソングの依頼。\n「勝利の歌を書いてほしい」\n\nアキラの走る姿を見て、\nテツヤのペンが走り出した。\n\nスタジアムに響く歌声、\n金メダルを取った瞬間、\nアキラはテツヤのもとへ走った。\n\n「この歌があったから勝てた」\n「君がいたから書けた」\n\n金メダルより輝く愛が生まれた。"},
        {"テツヤ_ミサト", "「音楽と物理学の共通点」\n雑誌のインタビューで出会った二人。\n\n「音は波でしょ？\n愛も波かもしれない」\n「周波数が合えば共鳴する...」\n「そう、今の僕たちみたいに」\n\n理屈っぽい会話が心地よかった。\n\nインタビューは終わっても、\n二人の会話は終わらなかった。\n共鳴した心は離れられない。"},
        {"テツヤ_カエデ", "化粧品CMソングの打ち合わせ。\n譜面を見るふりをして、\nテツヤはカエデを見つめていた。\n\n「曲より私を見てない？」\n「バレた？」\n「わかりやすいのよ、あなた」\n\nスタジオに響く笑い声。\nその日、二人は朝まで語り合った。\n\n「君のための歌を書きたい」\n「それ、プロポーズ？」\n「かもしれない」"},
        {"テツヤ_ルナ", "ワールドツアー、50都市。\n同じ夢を追う二人は、\n世界中を一緒に回った。\n\n「疲れないか？」\n「あなたがいるから平気」\n\nステージの上と、ランウェイの上。\n輝く場所は違っても、\n見つめ合う瞳は同じだった。\n\n最後の公演、アンコール。\nテツヤはステージ上で跪いた。\n「結婚してくれ」\n8億人のファンが証人となった。"},
    };

    // ストーリー表示用UI
    GameObject storyPanel;
    TextMeshProUGUI storyText;
    bool waitingForStoryConfirm;

    void Start()
    {
        Debug.Log("[BirthSystem] Start() called");
        if (canvas == null)
            canvas = FindAnyObjectByType<Canvas>();

        // childStatusText が未設定なら自動生成
        if (childStatusText == null && canvas != null)
        {
            var textObj = new GameObject("ChildStatusText");
            textObj.transform.SetParent(canvas.transform, false);
            var rect = textObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0, -200);
            rect.sizeDelta = new Vector2(800, 350);
            childStatusText = textObj.AddComponent<TextMeshProUGUI>();
            childStatusText.fontSize = 28;
            childStatusText.alignment = TextAlignmentOptions.Center;
            childStatusText.color = Color.white;
        }
        if (childStatusText != null)
            childStatusText.richText = true;
        SetButtonText(generateLifeButton, "いでよ、GOD BABY!!!");
        SetButtonText(anotherGalButton, "もう一度うむ");
        SetButtonText(gotoBattleButton, "名前をつける");
        CreateStatusTextBackground();
        CreateParentUI();
        CreateBabyFaceUI(); // babyFaceを作成
        CreateFlashOverlay();
        CreateLightningOverlay();
        CreateIntroPanel();
        CreateGenderSelectUI();
        CreateNameInputUI();
        CreateSaveConfirmUI();
        CreateStoryUI();
        //CreateCharacterListUI();
        CreateMenuBar();
        if (parentPanel != null) parentPanel.SetActive(false);
    }

    // ===== ボタンから呼ばれるメソッド =====

    public void SpinRoulette()
    {
        Debug.Log("[BirthSystem] SpinRoulette button clicked");
        if (isAnimating) return;

        // イントロパネルを非表示
        if (introPanel != null)
            introPanel.SetActive(false);

        StartCoroutine(SpinRouletteAnimation());
    }

    public void ResetParents()
    {
        if (isAnimating) return;
        StartCoroutine(SpinRouletteAnimation());
    }

    public void GoToBattle()
    {
        StartCoroutine(NameAndSaveSequence());
    }

    IEnumerator NameAndSaveSequence()
    {
        // 名前入力ダイアログ表示
        if (nameInputPanel != null)
        {
            nameInputPanel.SetActive(true);
            if (nameInputField != null)
            {
                nameInputField.text = "";
                nameInputField.Select();
                nameInputField.ActivateInputField();
            }
        }

        waitingForNameInput = true;
        while (waitingForNameInput)
            yield return null;

        nameInputPanel.SetActive(false);

        // DataCarrierに名前を保存
        if (DataCarrier.Instance != null)
        {
            DataCarrier.Instance.babyName = enteredName;
        }

        // セーブ確認ダイアログ表示
        if (saveConfirmPanel != null)
            saveConfirmPanel.SetActive(true);

        waitingForSaveConfirm = true;
        while (waitingForSaveConfirm)
            yield return null;

        saveConfirmPanel.SetActive(false);

        // セーブする場合（新規赤ちゃんなので空きスロットを探す）
        if (saveConfirmResult && DataCarrier.Instance != null)
        {
            int emptySlot = DataCarrier.FindEmptySlot();
            if (emptySlot >= 0)
            {
                DataCarrier.Instance.SaveToSlot(emptySlot);
            }
            else
            {
                // 空きがなければスロット0に上書き
                DataCarrier.Instance.SaveToSlot(0);
            }
        }

        // バトルシーンへ
        SceneManager.LoadScene("BattleScene");
    }

    public void OnNameConfirm()
    {
        if (nameInputField != null)
        {
            enteredName = nameInputField.text;
            if (string.IsNullOrEmpty(enteredName))
                enteredName = "名無しベイビー";
        }
        waitingForNameInput = false;
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

    // ===== 性別選択 =====

    public void SelectMale()
    {
        selectedGender = "男の子";
        waitingForGenderSelect = false;
    }

    public void SelectFemale()
    {
        selectedGender = "女の子";
        waitingForGenderSelect = false;
    }

    // ===== メインアニメーション =====

    IEnumerator SpinRouletteAnimation()
    {
        Debug.Log("[BirthSystem] SpinRouletteAnimation started");
        isAnimating = true;

        // レイアウトをルーレット用にリセット
        ResetToRouletteLayout();

        // ボタンを即非表示、?マークを隠す、背景も非表示
        if (generateLifeButton != null) generateLifeButton.SetActive(false);
        if (anotherGalButton != null) anotherGalButton.SetActive(false);
        if (gotoBattleButton != null) gotoBattleButton.SetActive(false);
        if (introText != null) introText.gameObject.SetActive(false);
        if (statusTextBackground != null) statusTextBackground.SetActive(false);

        // 結果を先に決定
        int fIdx = Random.Range(0, Fathers.Length);
        int mIdx = Random.Range(0, Mothers.Length);
        ParentData father = Fathers[fIdx];
        ParentData mother = Mothers[mIdx];

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
        int c_academic = (father.academic + mother.academic) / 2 + GaussianRandomRange(-30, 30);
        int c_athletic = (father.athletic + mother.athletic) / 2 + GaussianRandomRange(-30, 30);

        // 身長: 基準100cm、±30cmの範囲（大半は90-110cm程度）
        int c_height   = 100 + GaussianRandomRange(-30, 30);
        // 体重: 基準10000g(10kg)、±5000gの範囲（大半は8000-12000g程度）
        int c_weight   = 10000 + GaussianRandomRange(-5000, 5000);

        // 最低値の保証
        c_atk = Mathf.Max(1, c_atk);
        c_def = Mathf.Max(1, c_def);
        c_hp = Mathf.Max(50, c_hp);
        c_academic = Mathf.Max(1, c_academic);
        c_athletic = Mathf.Max(1, c_athletic);
        c_height = Mathf.Max(60, c_height);
        c_weight = Mathf.Max(5000, c_weight);

        // リョウマの子供は体重2倍
        if (father.name == "リョウマ")
        {
            c_weight *= 2;
        }

        string trait1 = DetermineTrait(c_atk, c_def, c_hp, c_academic, c_athletic);
        string trait2 = Traits[Random.Range(0, Traits.Length)];
        while (trait2 == trait1)
            trait2 = Traits[Random.Range(0, Traits.Length)];

        // ── フェーズ1: パネル表示、母親側は「???」で伏せる ──
        if (parentPanel != null) parentPanel.SetActive(true);
        childStatusText.text = "";

        // 紹介文を非表示にリセット
        if (fatherIntroText != null) fatherIntroText.gameObject.SetActive(false);
        if (motherIntroText != null) motherIntroText.gameObject.SetActive(false);

        // 母親側を「???」で伏せる
        if (motherFaceImage != null)
        {
            motherFaceImage.sprite = null;
            motherFaceImage.color = new Color(0.3f, 0.3f, 0.4f);
        }
        if (motherNameText != null)
            motherNameText.text = "母: ???";

        // ── フェーズ2: 父親ルーレット ──
        // 高速シャッフル（15回×0.06秒）
        for (int i = 0; i < 15; i++)
        {
            int tmpF = Random.Range(0, Fathers.Length);
            ShowSingleParentPreview(tmpF, Fathers[tmpF], true);
            yield return new WaitForSeconds(0.06f);
        }
        // 減速シャッフル（6回）
        for (int i = 0; i < 6; i++)
        {
            int tmpF = (i < 4) ? Random.Range(0, Fathers.Length) : fIdx;
            ShowSingleParentPreview(tmpF, Fathers[tmpF], true);
            float delay = Mathf.Lerp(0.12f, 0.35f, i / 5f);
            yield return new WaitForSeconds(delay);
        }
        // 父親確定
        ShowSingleParentPreview(fIdx, father, true);
        // 紹介文表示
        if (fatherIntroText != null)
        {
            fatherIntroText.text = father.intro;
            fatherIntroText.gameObject.SetActive(true);
        }
        yield return new WaitForSeconds(1.5f);

        // ── フェーズ3: 母親ルーレット ──
        // 高速シャッフル（15回×0.06秒）
        for (int i = 0; i < 15; i++)
        {
            int tmpM = Random.Range(0, Mothers.Length);
            ShowSingleParentPreview(tmpM, Mothers[tmpM], false);
            yield return new WaitForSeconds(0.06f);
        }
        // 減速シャッフル（6回）
        for (int i = 0; i < 6; i++)
        {
            int tmpM = (i < 4) ? Random.Range(0, Mothers.Length) : mIdx;
            ShowSingleParentPreview(tmpM, Mothers[tmpM], false);
            float delay = Mathf.Lerp(0.12f, 0.35f, i / 5f);
            yield return new WaitForSeconds(delay);
        }
        // 母親確定
        ShowSingleParentPreview(mIdx, mother, false);
        // 紹介文表示
        if (motherIntroText != null)
        {
            motherIntroText.text = mother.intro;
            motherIntroText.gameObject.SetActive(true);
        }
        yield return new WaitForSeconds(1.5f);

        // ── フェーズ4: 性別選択 ──
        Debug.Log("[BirthSystem] Phase 4: Gender selection");
        if (genderSelectPanel != null)
        {
            genderSelectPanel.SetActive(true);
            waitingForGenderSelect = true;
            selectedGender = "";

            // 選択されるまで待機
            while (waitingForGenderSelect)
            {
                yield return null;
            }

            genderSelectPanel.SetActive(false);
            Debug.Log($"[BirthSystem] Gender selected: {selectedGender}");
        }
        else
        {
            // パネルがない場合はランダム
            selectedGender = Random.Range(0, 2) == 0 ? "男の子" : "女の子";
            Debug.Log($"[BirthSystem] Gender random: {selectedGender}");
        }

        // 女の子は身長・体重が小さくなる（男の子の70-90%程度）
        if (selectedGender == "女の子")
        {
            float sizeMultiplier = 0.7f + Random.value * 0.2f; // 0.7〜0.9
            c_height = Mathf.RoundToInt(c_height * sizeMultiplier);
            c_weight = Mathf.RoundToInt(c_weight * sizeMultiplier);
            c_height = Mathf.Max(50, c_height);
            c_weight = Mathf.Max(4000, c_weight);
        }

        // ── フェーズ4.5: 恋愛ストーリー表示 ──
        yield return StartCoroutine(ShowLoveStory(father.name, mother.name));

        // ── フェーズ4.6: ルナの場合80%で子供に恵まれない ──
        if (mother.name == "ルナ")
        {
            bool lunaSuccess = Random.Range(0, 100) < 20; // 20%で成功
            if (!lunaSuccess)
            {
                // 失敗演出
                yield return StartCoroutine(ShowLunaFailure());
                isAnimating = false;
                yield break; // ここで終了
            }
            else
            {
                // 成功時はステータスボーナス（+20%）
                c_atk = Mathf.RoundToInt(c_atk * 1.2f);
                c_def = Mathf.RoundToInt(c_def * 1.2f);
                c_hp = Mathf.RoundToInt(c_hp * 1.2f);
                c_academic = Mathf.RoundToInt(c_academic * 1.2f);
                c_athletic = Mathf.RoundToInt(c_athletic * 1.2f);
                c_height = Mathf.RoundToInt(c_height * 1.1f);
                c_weight = Mathf.RoundToInt(c_weight * 1.1f);
            }
        }

        // ── フェーズ5: フラッシュ ──
        Debug.Log("[BirthSystem] Phase 5: Flash effect");
        yield return StartCoroutine(FlashEffect());

        // ── フェーズ5.5: レイアウト切り替え（親を端に、赤ちゃんを大きく中央に） ──
        Debug.Log("[BirthSystem] Phase 5.5: Layout switch and baby display");
        SwitchToBirthResultLayout();

        // ── フェーズ6: ステータス1行ずつ表示 ──
        // 上位1%判定（GOD BABY判定）、上位10%判定（大物判定）
        bool isGodBaby = IsGodBaby(c_atk, c_def, c_hp, c_academic, c_athletic);

        // 赤ちゃんの顔：専用画像があればそれを使用、なければ自動生成
        string genderKey = selectedGender == "男の子" ? "male" : "female";
        string babyImagePath = $"babys/{father.imageName}_{mother.imageName}_{genderKey}";
        Sprite babySprite = Resources.Load<Sprite>(babyImagePath);

        Debug.Log($"[BirthSystem] Baby image path: {babyImagePath}, Sprite loaded: {babySprite != null}");
        Debug.Log($"[BirthSystem] babyFace: {babyFace != null}, babyFaceImage: {babyFaceImage != null}");

        if (babySprite != null)
        {
            // 専用画像がある場合はそれを表示
            ShowBabySprite(babySprite, isGodBaby);
        }
        else
        {
            // 専用画像がない場合は自動生成
            GenerateBabyFace(c_weight, c_height, c_atk, c_academic, c_athletic, selectedGender, isGodBaby);
        }
        bool isPromisingBaby = !isGodBaby && IsPromisingBaby(c_atk, c_def, c_hp, c_academic, c_athletic);

        // ── 稲妻演出（GOD BABY or 大物の場合） ──
        if (isGodBaby || isPromisingBaby)
        {
            yield return StartCoroutine(LightningEffect(isGodBaby));
        }

        // ステータス表示前に背景を表示し、位置を調整
        RepositionStatusTextForDisplay();
        if (statusTextBackground != null) statusTextBackground.SetActive(true);

        string genderColor = selectedGender == "男の子" ? "#66ccff" : "#ff99cc";
        string line1;
        string line2;

        if (isGodBaby)
        {
            // 上位1%: GOD BABY演出（金屏風）
            line1 = "<color=#FFD700><size=120%><b>天からのお恵みだ。</b></size></color>";
            line2 = "<color=#FFD700><size=150%><b>GOD BABY 爆誕！</b></size></color>";
        }
        else if (isPromisingBaby)
        {
            // 上位10%: 大物演出（赤屏風）
            line1 = "<color=#FF6B6B><size=120%><b>大物になりそうな赤ちゃんだ！</b></size></color>";
            line2 = "──────────────────────────────────────";
        }
        else
        {
            line1 = "<color=#FFFF00><size=130%><b>【 新しい命が誕生！ 】</b></size></color>";
            line2 = "──────────────────────────────────────";
        }

        string line3 = $"<b>性別:</b> <color={genderColor}>{selectedGender}</color>    <b>身長:</b> {c_height} cm    <b>体重:</b> {c_weight} g";
        string line4 = $"<b>HP:</b> {c_hp}    <b>攻撃:</b> {c_atk}    <b>防御:</b> {c_def}";
        string line5 = $"<b>学力:</b> {c_academic}    <b>運動:</b> {c_athletic}";
        string line6 = $"<b>特徴:</b>  <color=#FFA500>{trait1}</color>    <color=#00FF00>{trait2}</color>";

        childStatusText.text = line1;
        yield return new WaitForSeconds(isGodBaby ? 1.0f : (isPromisingBaby ? 0.6f : 0.3f));
        childStatusText.text = line1 + "\n" + line2;
        yield return new WaitForSeconds(isGodBaby ? 1.0f : (isPromisingBaby ? 0.4f : 0.15f));
        childStatusText.text = line1 + "\n" + line2 + "\n\n" + line3;
        yield return new WaitForSeconds(0.15f);
        childStatusText.text = line1 + "\n" + line2 + "\n\n" + line3 + "\n" + line4;
        yield return new WaitForSeconds(0.15f);
        childStatusText.text = line1 + "\n" + line2 + "\n\n" + line3 + "\n" + line4 + "\n" + line5;
        yield return new WaitForSeconds(0.25f);
        childStatusText.text = line1 + "\n" + line2 + "\n\n" + line3 + "\n" + line4 + "\n" + line5 + "\n\n" + line6;

        // ── DataCarrier に保存 ──
        if (DataCarrier.Instance != null)
        {
            DataCarrier.Instance.babyAtk = c_atk;
            DataCarrier.Instance.babyDef = c_def;
            DataCarrier.Instance.babyHp = c_hp;
            DataCarrier.Instance.babyAcademic = c_academic;
            DataCarrier.Instance.babyWeight = c_weight;
            DataCarrier.Instance.babyAthletic = c_athletic;
            DataCarrier.Instance.babyHeight = c_height;
            DataCarrier.Instance.trait1 = trait1;
            DataCarrier.Instance.trait2 = trait2;
            DataCarrier.Instance.fatherName = father.name;
            DataCarrier.Instance.motherName = mother.name;
            DataCarrier.Instance.babyGender = selectedGender;
            DataCarrier.Instance.isGodBaby = isGodBaby;
        }

        // ── ボタン表示 ──
        yield return new WaitForSeconds(0.3f);
        if (anotherGalButton != null) anotherGalButton.SetActive(true);
        if (gotoBattleButton != null) gotoBattleButton.SetActive(true);

        isAnimating = false;
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

        // 稲妻の回数（GOD BABYは多め）
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
                    ? new Color(1f, 0.9f, 0.3f, 0.7f)  // GOD BABY: 金色フラッシュ
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
                ? new Color(1f, 0.85f, 0f, 0.9f)  // GOD BABY: 強い金色
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

    // ===== ルーレット中のプレビュー表示（父 or 母 個別） =====

    void ShowSingleParentPreview(int idx, ParentData data, bool isFather)
    {
        Image faceImage = isFather ? fatherFaceImage : motherFaceImage;
        TextMeshProUGUI nameT = isFather ? fatherNameText : motherNameText;
        Sprite[] sprites = isFather ? fatherSprites : motherSprites;
        string prefix = isFather ? "父" : "母";

        if (faceImage != null)
        {
            // Inspector経由で設定されたSprite配列を使用
            Sprite sp = (sprites != null && sprites.Length > idx) ? sprites[idx] : null;
            if (sp != null)
            {
                faceImage.sprite = sp;
                faceImage.color = Color.white;
            }
            else
            {
                faceImage.sprite = null;
                faceImage.color = data.faceColor;
            }
        }
        if (nameT != null)
            nameT.text = $"{prefix}: {data.name}";
    }

    // ===== ステータス判定 =====

    string DetermineTrait(int atk, int def, int hp, int academic, int athletic)
    {
        int max = Mathf.Max(atk, def, hp, academic, athletic);
        if (max == academic && academic > 80) return "天才肌";
        if (max == atk && atk > 70)           return "ワイルド";
        if (max == def && def > 70)            return "頑丈";
        if (max == athletic && athletic > 80)  return "すばしっこい";
        if (max == hp && hp > 180)             return "タフ";
        return Traits[Random.Range(0, Traits.Length)];
    }

    /// <summary>
    /// 上位1%の「GOD BABY」判定
    /// ポテンシャルスコアが閾値を超えるか、単一ステータスが極端に高い場合にtrue
    /// </summary>
    bool IsGodBaby(int atk, int def, int hp, int academic, int athletic)
    {
        // ポテンシャルスコア計算
        // 平均的な赤ちゃん: ATK50 + DEF50 + HP160/2 + Academic60 + Athletic60 = 300
        // 上位1%閾値: 360以上（かなりの上振れが必要）
        int potentialScore = atk + def + (hp / 2) + academic + athletic;

        if (potentialScore >= 360)
            return true;

        // 単一ステータスが極端に高い場合も GOD BABY
        // 正規分布で約2.5σ以上 = 上位約1%
        if (atk >= 90) return true;      // 攻撃の天才
        if (def >= 90) return true;      // 防御の天才
        if (hp >= 250) return true;      // 生命力の塊
        if (academic >= 100) return true; // 超天才
        if (athletic >= 100) return true; // 超人アスリート

        return false;
    }

    /// <summary>
    /// 上位10%の「大物」判定
    /// ポテンシャルスコアが閾値を超えるか、単一ステータスがやや高い場合にtrue
    /// </summary>
    bool IsPromisingBaby(int atk, int def, int hp, int academic, int athletic)
    {
        // ポテンシャルスコア計算
        // 平均的な赤ちゃん: ATK50 + DEF50 + HP160/2 + Academic60 + Athletic60 = 300
        // 上位10%閾値: 330以上（やや上振れが必要）
        int potentialScore = atk + def + (hp / 2) + academic + athletic;

        if (potentialScore >= 330)
            return true;

        // 単一ステータスがやや高い場合も 大物
        // 正規分布で約1.3σ以上 = 上位約10%
        if (atk >= 75) return true;      // 攻撃の才能
        if (def >= 75) return true;      // 防御の才能
        if (hp >= 210) return true;      // 生命力が高い
        if (academic >= 85) return true; // 秀才
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

        // GOD BABYオーラを追加
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

        // 赤ちゃんを大きく、両親の上端とラインが揃うように配置
        // 親パネルはy=280、カード高さ380なので上端はy=280+190=470
        babyFace.anchorMin = new Vector2(0.5f, 0.5f);
        babyFace.anchorMax = new Vector2(0.5f, 0.5f);
        babyFace.anchoredPosition = new Vector2(0, 250); // 高い位置
        babyFace.sizeDelta = new Vector2(450, 450); // 大きいサイズ（上端が親と揃う）

        // babyFaceを最前面に表示
        babyFace.SetAsLastSibling();
    }

    void RepositionStatusTextForDisplay()
    {
        // ステータステキストの位置はInspectorで設定されたままにする（変更しない）
    }

    void GenerateBabyFace(int weight, int height, int atk, int academic, int athletic, string gender, bool isGodBaby)
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

        float faceScale = 1.8f + (weight / 20000f) * 0.4f; // 大きいスケール（450x450に合わせて）
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

        // GOD BABYオーラ
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
        Color hairBase = hairColors[Mathf.Clamp(academic / 22, 0, 4)];
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
        float eyeSize = 14 + (academic / 12f);

        Create3DEye(babyFace, -22, 8, eyeSize, eyeColor, gender == "女の子", baseSkin);
        Create3DEye(babyFace, 22, 8, eyeSize, eyeColor, gender == "女の子", baseSkin);

        // 眉
        float browAngle = (atk - 50) * 0.25f;
        Create3DEyebrow(babyFace, -24, 32, browAngle, hairBase);
        Create3DEyebrow(babyFace, 24, 32, -browAngle, hairBase);

        // 鼻（3D風）
        Create3DNose(babyFace, baseSkin, shadowSkin, height);

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

    void Create3DNose(Transform parent, Color skinColor, Color shadowColor, int height)
    {
        float noseLen = 12 + height / 15f;

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
        if (canvas == null)
            canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null) return;

        parentPanel = new GameObject("ParentPanel");
        parentPanel.transform.SetParent(canvas.transform, false);

        var panelRect = parentPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 0.5f);
        panelRect.anchorMax = new Vector2(1, 0.5f);
        panelRect.anchoredPosition = new Vector2(0, 280);
        panelRect.sizeDelta = new Vector2(0, 420);

        CreateParentCard(parentPanel.transform, -450, out fatherFaceImage, out fatherNameText, out fatherIntroText);
        CreateParentCard(parentPanel.transform, 450, out motherFaceImage, out motherNameText, out motherIntroText);
    }

    void CreateParentCard(Transform parent, float xPos, out Image faceImage, out TextMeshProUGUI nameText, out TextMeshProUGUI introText)
    {
        var card = new GameObject("ParentCard");
        card.transform.SetParent(parent, false);

        var cardRect = card.AddComponent<RectTransform>();
        cardRect.anchorMin = new Vector2(0.5f, 0.5f);
        cardRect.anchorMax = new Vector2(0.5f, 0.5f);
        cardRect.anchoredPosition = new Vector2(xPos, 0);
        cardRect.sizeDelta = new Vector2(280, 380);

        var bg = card.AddComponent<Image>();
        bg.color = new Color(0.2f, 0.2f, 0.3f, 0.8f);
        bg.raycastTarget = false;

        var faceObj = new GameObject("Face");
        faceObj.transform.SetParent(card.transform, false);
        var faceRect = faceObj.AddComponent<RectTransform>();
        faceRect.anchorMin = new Vector2(0.5f, 1f);
        faceRect.anchorMax = new Vector2(0.5f, 1f);
        faceRect.anchoredPosition = new Vector2(0, -120);
        faceRect.sizeDelta = new Vector2(200, 200);
        faceImage = faceObj.AddComponent<Image>();
        faceImage.color = Color.white;
        faceImage.raycastTarget = false;
        faceImage.preserveAspect = true;

        var nameObj = new GameObject("Name");
        nameObj.transform.SetParent(card.transform, false);
        var nameRect = nameObj.AddComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0, 0);
        nameRect.anchorMax = new Vector2(1, 0);
        nameRect.anchoredPosition = new Vector2(0, 70);
        nameRect.sizeDelta = new Vector2(0, 50);
        nameText = nameObj.AddComponent<TextMeshProUGUI>();
        nameText.fontSize = 32;
        nameText.alignment = TextAlignmentOptions.Center;
        nameText.color = Color.white;
        nameText.raycastTarget = false;

        var introObj = new GameObject("Intro");
        introObj.transform.SetParent(card.transform, false);
        var introRect = introObj.AddComponent<RectTransform>();
        introRect.anchorMin = new Vector2(0, 0);
        introRect.anchorMax = new Vector2(1, 0);
        introRect.anchoredPosition = new Vector2(0, 25);
        introRect.sizeDelta = new Vector2(0, 50);
        introText = introObj.AddComponent<TextMeshProUGUI>();
        introText.fontSize = 18;
        introText.alignment = TextAlignmentOptions.Center;
        introText.color = new Color(1f, 0.9f, 0.5f);
        introText.raycastTarget = false;
        introObj.SetActive(false);
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
    void SwitchToBirthResultLayout()
    {
        // 親パネルと親カードの位置・サイズはそのまま維持
        // 赤ちゃんだけ大きくして上に配置（RepositionBabyFaceForDisplayで行う）
    }

    // ルーレット用のレイアウトにリセット
    void ResetToRouletteLayout()
    {
        // 赤ちゃんの位置をリセット（ルーレット時は小さく、?マーク表示用）
        if (babyFace != null)
        {
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
        // 親パネルとステータステキストの位置は変更しない（元のまま）
    }

    void CreateStatusTextBackground()
    {
        if (childStatusText == null) return;

        // childStatusTextの親を取得
        Transform parentTransform = childStatusText.transform.parent;
        if (parentTransform == null) parentTransform = canvas.transform;

        // 背景パネルを作成
        statusTextBackground = new GameObject("StatusTextBackground");
        statusTextBackground.transform.SetParent(parentTransform, false);

        // childStatusTextより先に描画されるようにSibling Indexを設定
        statusTextBackground.transform.SetSiblingIndex(childStatusText.transform.GetSiblingIndex());

        var bgRect = statusTextBackground.AddComponent<RectTransform>();
        // childStatusTextと同じ位置・サイズに設定
        RectTransform statusRect = childStatusText.GetComponent<RectTransform>();
        bgRect.anchorMin = statusRect.anchorMin;
        bgRect.anchorMax = statusRect.anchorMax;
        bgRect.anchoredPosition = statusRect.anchoredPosition;
        bgRect.sizeDelta = statusRect.sizeDelta + new Vector2(40, 30); // 少し大きめに
        bgRect.pivot = statusRect.pivot;

        var bgImage = statusTextBackground.AddComponent<Image>();
        bgImage.color = new Color(0f, 0f, 0f, 0.7f); // 半透明の黒背景
        bgImage.raycastTarget = false;

        // 初期状態は非表示
        statusTextBackground.SetActive(false);
    }

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

    void CreateGenderSelectUI()
    {
        if (canvas == null) return;

        genderSelectPanel = new GameObject("GenderSelectPanel");
        genderSelectPanel.transform.SetParent(canvas.transform, false);

        var panelRect = genderSelectPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = new Vector2(0, -50);
        panelRect.sizeDelta = new Vector2(400, 200);

        var panelBg = genderSelectPanel.AddComponent<Image>();
        panelBg.color = new Color(0.15f, 0.15f, 0.25f, 0.95f);

        // タイトルテキスト
        var titleObj = new GameObject("Title");
        titleObj.transform.SetParent(genderSelectPanel.transform, false);
        var titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.anchoredPosition = new Vector2(0, -30);
        titleRect.sizeDelta = new Vector2(0, 50);
        var titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "どちらでプレイする？";
        titleText.fontSize = 32;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = Color.white;
        titleText.fontStyle = FontStyles.Bold;
        titleText.raycastTarget = false;

        // 男の子ボタン
        CreateGenderButton(genderSelectPanel.transform, -90, "男の子", new Color(0.4f, 0.6f, 1f), SelectMale);

        // 女の子ボタン
        CreateGenderButton(genderSelectPanel.transform, 90, "女の子", new Color(1f, 0.5f, 0.7f), SelectFemale);

        genderSelectPanel.SetActive(false);
    }

    void CreateGenderButton(Transform parent, float xPos, string label, Color bgColor, UnityEngine.Events.UnityAction onClick)
    {
        var btnObj = new GameObject(label + "Button");
        btnObj.transform.SetParent(parent, false);

        var btnRect = btnObj.AddComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.5f, 0);
        btnRect.anchorMax = new Vector2(0.5f, 0);
        btnRect.anchoredPosition = new Vector2(xPos, 70);
        btnRect.sizeDelta = new Vector2(140, 80);

        var btnImg = btnObj.AddComponent<Image>();
        btnImg.color = bgColor;

        var btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = btnImg;
        btn.onClick.AddListener(onClick);

        // ボタンテキスト
        var textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        var textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        var btnText = textObj.AddComponent<TextMeshProUGUI>();
        btnText.text = label;
        btnText.fontSize = 28;
        btnText.alignment = TextAlignmentOptions.Center;
        btnText.color = Color.white;
        btnText.fontStyle = FontStyles.Bold;
        btnText.raycastTarget = false;
    }

    void CreateNameInputUI()
    {
        if (canvas == null) return;

        nameInputPanel = new GameObject("NameInputPanel");
        nameInputPanel.transform.SetParent(canvas.transform, false);

        var panelRect = nameInputPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(450, 220);

        var panelBg = nameInputPanel.AddComponent<Image>();
        panelBg.color = new Color(0.1f, 0.1f, 0.2f, 0.95f);

        // タイトルテキスト
        var titleObj = new GameObject("Title");
        titleObj.transform.SetParent(nameInputPanel.transform, false);
        var titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.anchoredPosition = new Vector2(0, -30);
        titleRect.sizeDelta = new Vector2(0, 50);
        var titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "赤ちゃんの名前を入力してください";
        titleText.fontSize = 26;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = Color.white;
        titleText.fontStyle = FontStyles.Bold;
        titleText.raycastTarget = false;

        // 入力フィールド
        var inputObj = new GameObject("InputField");
        inputObj.transform.SetParent(nameInputPanel.transform, false);
        var inputRect = inputObj.AddComponent<RectTransform>();
        inputRect.anchorMin = new Vector2(0.5f, 0.5f);
        inputRect.anchorMax = new Vector2(0.5f, 0.5f);
        inputRect.anchoredPosition = new Vector2(0, 10);
        inputRect.sizeDelta = new Vector2(350, 50);

        var inputBg = inputObj.AddComponent<Image>();
        inputBg.color = new Color(0.2f, 0.2f, 0.3f, 1f);

        nameInputField = inputObj.AddComponent<TMP_InputField>();
        nameInputField.characterLimit = 12;

        // テキストエリア
        var textAreaObj = new GameObject("TextArea");
        textAreaObj.transform.SetParent(inputObj.transform, false);
        var textAreaRect = textAreaObj.AddComponent<RectTransform>();
        textAreaRect.anchorMin = Vector2.zero;
        textAreaRect.anchorMax = Vector2.one;
        textAreaRect.offsetMin = new Vector2(10, 5);
        textAreaRect.offsetMax = new Vector2(-10, -5);
        textAreaObj.AddComponent<RectMask2D>();

        // 入力テキスト
        var inputTextObj = new GameObject("Text");
        inputTextObj.transform.SetParent(textAreaObj.transform, false);
        var inputTextRect = inputTextObj.AddComponent<RectTransform>();
        inputTextRect.anchorMin = Vector2.zero;
        inputTextRect.anchorMax = Vector2.one;
        inputTextRect.offsetMin = Vector2.zero;
        inputTextRect.offsetMax = Vector2.zero;
        var inputText = inputTextObj.AddComponent<TextMeshProUGUI>();
        inputText.fontSize = 28;
        inputText.alignment = TextAlignmentOptions.Left;
        inputText.color = Color.white;
        nameInputField.textComponent = inputText;
        nameInputField.textViewport = textAreaRect;

        // 確定ボタン
        var confirmObj = new GameObject("ConfirmButton");
        confirmObj.transform.SetParent(nameInputPanel.transform, false);
        var confirmRect = confirmObj.AddComponent<RectTransform>();
        confirmRect.anchorMin = new Vector2(0.5f, 0);
        confirmRect.anchorMax = new Vector2(0.5f, 0);
        confirmRect.anchoredPosition = new Vector2(0, 45);
        confirmRect.sizeDelta = new Vector2(160, 50);

        var confirmBg = confirmObj.AddComponent<Image>();
        confirmBg.color = new Color(0.3f, 0.7f, 0.4f);

        var confirmBtn = confirmObj.AddComponent<Button>();
        confirmBtn.targetGraphic = confirmBg;
        confirmBtn.onClick.AddListener(OnNameConfirm);

        var confirmTextObj = new GameObject("Text");
        confirmTextObj.transform.SetParent(confirmObj.transform, false);
        var confirmTextRect = confirmTextObj.AddComponent<RectTransform>();
        confirmTextRect.anchorMin = Vector2.zero;
        confirmTextRect.anchorMax = Vector2.one;
        confirmTextRect.offsetMin = Vector2.zero;
        confirmTextRect.offsetMax = Vector2.zero;
        var confirmText = confirmTextObj.AddComponent<TextMeshProUGUI>();
        confirmText.text = "決定";
        confirmText.fontSize = 26;
        confirmText.alignment = TextAlignmentOptions.Center;
        confirmText.color = Color.white;
        confirmText.fontStyle = FontStyles.Bold;
        confirmText.raycastTarget = false;

        nameInputPanel.SetActive(false);
    }

    void CreateSaveConfirmUI()
    {
        if (canvas == null) return;

        saveConfirmPanel = new GameObject("SaveConfirmPanel");
        saveConfirmPanel.transform.SetParent(canvas.transform, false);

        var panelRect = saveConfirmPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(400, 180);

        var panelBg = saveConfirmPanel.AddComponent<Image>();
        panelBg.color = new Color(0.1f, 0.1f, 0.2f, 0.95f);

        // タイトルテキスト
        var titleObj = new GameObject("Title");
        titleObj.transform.SetParent(saveConfirmPanel.transform, false);
        var titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.anchoredPosition = new Vector2(0, -40);
        titleRect.sizeDelta = new Vector2(0, 60);
        var titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "セーブしますか？";
        titleText.fontSize = 32;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = Color.white;
        titleText.fontStyle = FontStyles.Bold;
        titleText.raycastTarget = false;

        // はいボタン
        var yesObj = new GameObject("YesButton");
        yesObj.transform.SetParent(saveConfirmPanel.transform, false);
        var yesRect = yesObj.AddComponent<RectTransform>();
        yesRect.anchorMin = new Vector2(0.5f, 0);
        yesRect.anchorMax = new Vector2(0.5f, 0);
        yesRect.anchoredPosition = new Vector2(-80, 50);
        yesRect.sizeDelta = new Vector2(120, 50);

        var yesBg = yesObj.AddComponent<Image>();
        yesBg.color = new Color(0.3f, 0.7f, 0.4f);

        var yesBtn = yesObj.AddComponent<Button>();
        yesBtn.targetGraphic = yesBg;
        yesBtn.onClick.AddListener(OnSaveYes);

        var yesTextObj = new GameObject("Text");
        yesTextObj.transform.SetParent(yesObj.transform, false);
        var yesTextRect = yesTextObj.AddComponent<RectTransform>();
        yesTextRect.anchorMin = Vector2.zero;
        yesTextRect.anchorMax = Vector2.one;
        yesTextRect.offsetMin = Vector2.zero;
        yesTextRect.offsetMax = Vector2.zero;
        var yesText = yesTextObj.AddComponent<TextMeshProUGUI>();
        yesText.text = "はい";
        yesText.fontSize = 26;
        yesText.alignment = TextAlignmentOptions.Center;
        yesText.color = Color.white;
        yesText.fontStyle = FontStyles.Bold;
        yesText.raycastTarget = false;

        // いいえボタン
        var noObj = new GameObject("NoButton");
        noObj.transform.SetParent(saveConfirmPanel.transform, false);
        var noRect = noObj.AddComponent<RectTransform>();
        noRect.anchorMin = new Vector2(0.5f, 0);
        noRect.anchorMax = new Vector2(0.5f, 0);
        noRect.anchoredPosition = new Vector2(80, 50);
        noRect.sizeDelta = new Vector2(120, 50);

        var noBg = noObj.AddComponent<Image>();
        noBg.color = new Color(0.6f, 0.3f, 0.3f);

        var noBtn = noObj.AddComponent<Button>();
        noBtn.targetGraphic = noBg;
        noBtn.onClick.AddListener(OnSaveNo);

        var noTextObj = new GameObject("Text");
        noTextObj.transform.SetParent(noObj.transform, false);
        var noTextRect = noTextObj.AddComponent<RectTransform>();
        noTextRect.anchorMin = Vector2.zero;
        noTextRect.anchorMax = Vector2.one;
        noTextRect.offsetMin = Vector2.zero;
        noTextRect.offsetMax = Vector2.zero;
        var noText = noTextObj.AddComponent<TextMeshProUGUI>();
        noText.text = "いいえ";
        noText.fontSize = 26;
        noText.alignment = TextAlignmentOptions.Center;
        noText.color = Color.white;
        noText.fontStyle = FontStyles.Bold;
        noText.raycastTarget = false;

        saveConfirmPanel.SetActive(false);
    }

    void SetButtonText(GameObject button, string text)
    {
        if (button == null) return;
        var tmp = button.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp != null) tmp.text = text;
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
        CreateCharacterRow(listPanel.transform, "父親", Fathers, fatherSprites, 25);
        // 母親行
        CreateCharacterRow(listPanel.transform, "母親", Mothers, motherSprites, -25);
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

        Sprite sp = (sprites != null && sprites.Length > idx) ? sprites[idx] : null;
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
        nameText.text = data.name;
        nameText.fontSize = 12;
        nameText.alignment = TextAlignmentOptions.Center;
        nameText.color = new Color(1f, 1f, 1f, 0.9f);
        nameText.raycastTarget = false;
    }

    // ===== 恋愛ストーリーUI =====

    void CreateStoryUI()
    {
        if (canvas == null) return;

        storyPanel = new GameObject("StoryPanel");
        storyPanel.transform.SetParent(canvas.transform, false);

        var panelRect = storyPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        var panelBg = storyPanel.AddComponent<Image>();
        panelBg.color = new Color(0f, 0f, 0f, 0.85f);

        // タイトル
        var titleObj = new GameObject("Title");
        titleObj.transform.SetParent(storyPanel.transform, false);
        var titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 0.75f);
        titleRect.anchorMax = new Vector2(1, 0.9f);
        titleRect.offsetMin = new Vector2(20, 0);
        titleRect.offsetMax = new Vector2(-20, 0);
        var titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "<color=#FF69B4>♥</color> 二人の出会い <color=#FF69B4>♥</color>";
        titleText.fontSize = 42;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = new Color(1f, 0.85f, 0.9f);
        titleText.fontStyle = FontStyles.Bold;
        titleText.raycastTarget = false;

        // ストーリーテキスト
        var storyObj = new GameObject("StoryText");
        storyObj.transform.SetParent(storyPanel.transform, false);
        var storyRect = storyObj.AddComponent<RectTransform>();
        storyRect.anchorMin = new Vector2(0.1f, 0.35f);
        storyRect.anchorMax = new Vector2(0.9f, 0.7f);
        storyRect.offsetMin = Vector2.zero;
        storyRect.offsetMax = Vector2.zero;
        storyText = storyObj.AddComponent<TextMeshProUGUI>();
        storyText.fontSize = 32;
        storyText.alignment = TextAlignmentOptions.Center;
        storyText.color = Color.white;
        storyText.raycastTarget = false;

        // タップで続行のヒント
        var hintObj = new GameObject("Hint");
        hintObj.transform.SetParent(storyPanel.transform, false);
        var hintRect = hintObj.AddComponent<RectTransform>();
        hintRect.anchorMin = new Vector2(0, 0.1f);
        hintRect.anchorMax = new Vector2(1, 0.2f);
        hintRect.offsetMin = Vector2.zero;
        hintRect.offsetMax = Vector2.zero;
        var hintText = hintObj.AddComponent<TextMeshProUGUI>();
        hintText.text = "▼ タップで続ける ▼";
        hintText.fontSize = 24;
        hintText.alignment = TextAlignmentOptions.Center;
        hintText.color = new Color(1f, 1f, 1f, 0.6f);
        hintText.raycastTarget = false;

        // タップ検出用のボタン
        var tapBtn = storyPanel.AddComponent<Button>();
        tapBtn.transition = Selectable.Transition.None;
        tapBtn.onClick.AddListener(OnStoryTap);

        storyPanel.SetActive(false);
    }

    void OnStoryTap()
    {
        waitingForStoryConfirm = false;
    }

    IEnumerator ShowLoveStory(string fatherName, string motherName)
    {
        string key = $"{fatherName}_{motherName}";
        string story = "運命の出会いから\n愛が芽生えた...";

        if (LoveStories.TryGetValue(key, out string foundStory))
        {
            story = foundStory;
        }

        if (storyPanel != null && storyText != null)
        {
            // 親パネルを一時的に隠す
            if (parentPanel != null) parentPanel.SetActive(false);

            storyText.text = "";
            storyPanel.SetActive(true);

            // ストーリーを1文字ずつ表示（タイプライター効果）
            foreach (char c in story)
            {
                storyText.text += c;
                yield return new WaitForSeconds(0.03f);
            }

            // タップ待ち（最大3秒でタイムアウト、またはOnStoryTapで進む）
            waitingForStoryConfirm = true;
            float timeout = 3f;
            float elapsed = 0f;
            while (waitingForStoryConfirm && elapsed < timeout)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
            waitingForStoryConfirm = false;

            storyPanel.SetActive(false);

            // 親パネルを再表示
            if (parentPanel != null) parentPanel.SetActive(true);
        }

        yield return new WaitForSeconds(0.3f);
    }

    IEnumerator ShowLunaFailure()
    {
        // 親パネルを非表示
        if (parentPanel != null) parentPanel.SetActive(false);
        if (statusTextBackground != null) statusTextBackground.SetActive(false);
        childStatusText.text = "";

        // タイトルイントロと同じスライドイン演出
        Canvas canvas = FindObjectOfType<Canvas>();

        // 暗転パネル
        var panel = new GameObject("LunaFailPanel");
        panel.transform.SetParent(canvas.transform, false);
        var panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        var panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0f);
        panelImage.raycastTarget = true;

        // フェードイン
        float fadeInDuration = 0.5f;
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            panelImage.color = new Color(0f, 0f, 0f, Mathf.Lerp(0f, 0.9f, elapsed / fadeInDuration));
            yield return null;
        }
        panelImage.color = new Color(0f, 0f, 0f, 0.9f);

        // スライドインパラメータ（タイトルと同じ）
        float slideDuration = 1.5f;
        float lineInterval = 1.0f;
        float startOffsetX = -800f;
        float verticalStart = 100f;
        float lineSpacing = 120f;

        string[] sadLines = new string[]
        {
            "「この世界、ハズレばっかりだと思わないか？」\n",
            "二人は何年も待ち続けた。\nだが、コウノトリは訪れなかった。\n",
            "ルナは世界一美しかったが、\n神は全てを与えはしなかった...\n",
            "<color=#AADDFF>もう一度運命に挑戦しよう。</color>",
        };

        for (int i = 0; i < sadLines.Length; i++)
        {
            var textObj = new GameObject("LunaLine_" + i);
            textObj.transform.SetParent(panel.transform, false);
            var textRect = textObj.AddComponent<RectTransform>();
            var tmp = textObj.AddComponent<TextMeshProUGUI>();

            tmp.text = sadLines[i];
            tmp.fontSize = i == 0 ? 36 : 32;
            tmp.color = i == 0 ? Color.white : new Color(0.8f, 0.8f, 1f);
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableWordWrapping = true;
            tmp.raycastTarget = false;

            float yPos = verticalStart - (i * lineSpacing);
            textRect.sizeDelta = new Vector2(800f, 100f);
            textRect.anchoredPosition = new Vector2(startOffsetX, yPos);

            // スライドインアニメーション
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

        yield return new WaitForSeconds(2.0f);

        // フェードアウト
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

        // やり直しボタンのみ表示
        childStatusText.text = "";
        if (generateLifeButton != null) generateLifeButton.SetActive(true);
        if (anotherGalButton != null) anotherGalButton.SetActive(false);
        if (gotoBattleButton != null) gotoBattleButton.SetActive(false);
    }

    // ===== メニューバー =====

    void CreateMenuBar()
    {
        if (canvas == null) return;

        var bar = new GameObject("MenuBar");
        bar.transform.SetParent(canvas.transform, false);

        var barRect = bar.AddComponent<RectTransform>();
        barRect.anchorMin = new Vector2(1, 1);
        barRect.anchorMax = new Vector2(1, 1);
        barRect.pivot = new Vector2(1, 1);
        barRect.anchoredPosition = new Vector2(-20, -20);
        barRect.sizeDelta = new Vector2(180, 60);

        var barBg = bar.AddComponent<Image>();
        barBg.color = new Color(0.15f, 0.15f, 0.2f, 0.85f);
        barBg.raycastTarget = false;

        var layout = bar.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 12;
        layout.padding = new RectOffset(12, 12, 6, 6);
        layout.childAlignment = TextAnchor.MiddleRight;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = true;

        CreateMenuButton(bar.transform, "トップへ", () => SceneManager.LoadScene("TitleScene"));
    }

    void CreateMenuButton(Transform parent, string label, UnityEngine.Events.UnityAction action)
    {
        var btnObj = new GameObject(label);
        btnObj.transform.SetParent(parent, false);

        var btnImage = btnObj.AddComponent<Image>();
        btnImage.color = new Color(0.3f, 0.3f, 0.45f, 1f);

        var btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = btnImage;

        var colors = btn.colors;
        colors.highlightedColor = new Color(0.45f, 0.45f, 0.65f, 1f);
        colors.pressedColor = new Color(0.2f, 0.2f, 0.35f, 1f);
        btn.colors = colors;

        btn.onClick.AddListener(action);

        var textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);

        var textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        var tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 24;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.raycastTarget = false;
    }
}

public struct ParentData
{
    public string name;
    public string imageName; // 英語名（小文字）
    public int atk, def, hp, height, academic, weight, athletic;
    public Color faceColor;
    public string intro;

    public ParentData(string name, string imageName, int atk, int def, int hp, int height, int academic, int weight, int athletic, Color faceColor, string intro)
    {
        this.name = name;
        this.imageName = imageName;
        this.atk = atk;
        this.def = def;
        this.hp = hp;
        this.height = height;
        this.academic = academic;
        this.weight = weight;
        this.athletic = athletic;
        this.faceColor = faceColor;
        this.intro = intro;
    }
}
