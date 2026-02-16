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

    // フラッシュ演出用
    Image flashOverlay;

    // 稲妻演出用
    GameObject lightningContainer;

    // ?マーク
    TextMeshProUGUI introText;
    GameObject introPanel;

    // 親情報パネル
    UIE.Button parentInfoButton;
    UIE.VisualElement parentBioOverlayEl;

    // 画像アップロードボタン
    UIE.Button uploadImageBtn;
    UIE.VisualElement customBabyImageEl;

    // 背景
    Image mainBgImg;

    // アニメーション中フラグ
    bool isAnimating;
    bool skipRequested;

    string selectedGender;

    // 名前入力UI
    string enteredName;
    bool waitingForNameInput;

    // セーブ確認UI
    bool waitingForSaveConfirm;
    bool saveConfirmResult;

    // 父親6パターン
    static readonly ParentData[] Fathers = new[]
    {
        new ParentData("タケシ",   "takeshi", 70, 30, 180, 40, 85, 30, 20, new Color(0.9f, 0.7f, 0.5f), "元・格闘技世界王者 / 握力: 180kg"),
        new ParentData("ユウキ",   "yuuki",   50, 50, 150, 70, 60, 50, 40, new Color(0.6f, 0.8f, 1.0f), "天才ハッカー / 特許数: 3,200件"),
        new ParentData("ゴウ",     "gou",     80, 25, 190, 30, 90, 20, 15, new Color(1.0f, 0.5f, 0.4f), "伝説の傭兵 / 戦闘力: 計測不能"),
        new ParentData("シンジ",   "shinji",  30, 70, 140, 95, 35, 60, 30, new Color(0.7f, 0.7f, 1.0f), "ノーベル賞3回受賞 / IQ: 250"),
        new ParentData("リョウマ", "ryouma",  60, 60, 170, 55, 70, 70, 95, new Color(0.5f, 1.0f, 0.6f), "総資産: 43兆円 / 世界一の実業家"),
        new ParentData("テツヤ",   "tetuya",  45, 45, 160, 60, 55, 80, 60, new Color(1.0f, 0.9f, 0.5f), "伝説のロックスター / ファン数: 8億人"),
    };

    // 母親6パターン
    static readonly ParentData[] Mothers = new[]
    {
        new ParentData("サクラ",   "sakura",  40, 60, 150, 60, 65, 40, 25, new Color(1.0f, 0.7f, 0.8f), "暗殺拳の継承者 / 全戦全勝"),
        new ParentData("ヒナタ",   "hinata",  50, 50, 145, 70, 60, 55, 90, new Color(0.8f, 0.6f, 1.0f), "総資産: 28兆円 / 美容帝国CEO"),
        new ParentData("アキラ",   "akira",   65, 30, 160, 45, 80, 45, 35, new Color(1.0f, 0.6f, 0.4f), "五輪金メダル7個 / 100m走: 10.1秒"),
        new ParentData("ミサト",   "misato",  35, 55, 140, 90, 30, 65, 45, new Color(0.6f, 0.9f, 1.0f), "量子物理学者 / IQ: 270"),
        new ParentData("カエデ",   "kaede",   25, 70, 130, 85, 40, 50, 50, new Color(0.5f, 1.0f, 0.7f), "天才外科医 / 手術成功率: 100%"),
        new ParentData("ルナ",     "luna",    55, 40, 135, 50, 70, 75, 70, new Color(1.0f, 1.0f, 0.6f), "世界的スーパーモデル / 身長: 180cm"),
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
        {"タケシ_サクラ", "裏格闘技界の頂点を決める戦い。\nタケシとサクラは決勝で激突した。\n\n拳と暗殺拳が交錯する中、\n二人は互いの強さに惹かれていく。\n\n死闘は引き分けに終わり、\n「決着は別の形でつけよう」と\nタケシが差し出した手を、\nサクラは静かに握り返した。\n最強の血統がここに誕生する。"},
        {"タケシ_ヒナタ", "「格闘家専用コスメを作りたい」\nヒナタからの突然の依頼。\n\nビジネスミーティングのはずが、\nタケシの素朴な優しさに触れ、\nヒナタの心は揺れ始める。\n\n「数字じゃ測れないものがある」\nタケシの言葉に、\n28兆円の帝国を築いた女は\n初めて涙を流した。\n愛は最高の投資だと知った日。"},
        {"タケシ_アキラ", "オリンピック選手村での出会い。\n格闘技代表のタケシと\n陸上代表のアキラ。\n\n食堂で偶然隣り合わせになり、\n互いの鍛え抜かれた肉体に\n目を奪われた。\n\n「一緒にトレーニングしないか？」\nその一言から始まった朝練は、\nいつしか二人だけの時間に変わり、\n閉会式の夜、二人は結ばれた。"},
        {"タケシ_ミサト", "「筋肉の収縮は量子力学で\n説明できるんですよ」\n\n学会に招かれたタケシに、\nミサトは熱心に語りかけた。\n\n「難しいことはわからねえが、\nあんたの目は本気だな」\n\n理論と実践、正反対の二人。\nだが夜通し語り合ううちに、\n科学者の心は格闘家に奪われ、\n最強の頭脳と肉体が融合した。"},
        {"タケシ_カエデ", "世界格闘技選手権の決勝戦。\nタケシは宿敵との死闘の末、\n右腕を複雑骨折した。\n\n「二度と戦えない」と宣告される中、\n唯一の希望は天才外科医カエデだった。\n\n12時間に及ぶ手術。\n目覚めたタケシの最初の言葉は\n「俺の腕を救ってくれた君を、\n俺の人生に迎えたい」だった。"},
        {"タケシ_ルナ", "スポーツ雑誌の表紙撮影。\n格闘家とスーパーモデルの共演。\n\nカメラの前で火花が散り、\n「もっと近づいて」という\nカメラマンの指示に、\n二人の心臓が高鳴る。\n\n撮影後、ルナが言った。\n「あなたの隣にいると、\n自分が美しく見える気がする」\nスポットライトの下で恋が始まった。"},

        // ユウキ（ハッカー）× 各母親
        {"ユウキ_サクラ", "暗殺組織のサーバーに侵入した夜、\nユウキは追手に囲まれた。\n\nその中にいたのがサクラ。\n「殺すつもりはない。\nあなたの腕が必要なの」\n\n組織を裏切り、共に逃亡する日々。\n追われる中で芽生えた信頼は、\nいつしか愛に変わっていた。\n\n「俺のファイアウォールは\n君だけ通過できる」\n不器用な告白だった。"},
        {"ユウキ_ヒナタ", "美容帝国のDX化プロジェクト。\n億単位の契約書を前に、\nユウキは言った。\n\n「報酬はいらない。\nその代わり、週に一度\n食事に付き合ってほしい」\n\n最初は呆れていたヒナタも、\n彼の純粋さに惹かれていく。\n\n「私に値段をつけない人は\n初めてよ」\n28兆円より価値ある愛を知った。"},
        {"ユウキ_アキラ", "アスリート向けAIトレーナーの開発中、\nテストランナーとして\nアキラが研究所に現れた。\n\n「データが全然取れない...\n君は規格外すぎる」\n困惑するユウキに、\nアキラは笑って言った。\n\n「じゃあ毎日来てあげる」\n\nデータ収集という名目の\nデートが始まり、\n数値では測れない感情が芽生えた。"},
        {"ユウキ_ミサト", "量子コンピュータの共同研究。\n世界最高峰の頭脳が二つ、\n同じ研究室に集まった。\n\n夜通しのプログラミング、\nコーヒーカップが触れ合う音、\n「この暗号、解ける？」\n「君となら、どんな問題でも」\n\n二人だけの言語で愛を語り、\n論文より大切な答えを見つけた。\nそれは「共に生きる」という\nシンプルな真実だった。"},
        {"ユウキ_カエデ", "大病院のシステムがハッキングされた。\n犯人を追うカエデの前に現れたのは、\nセキュリティ専門家のユウキだった。\n\n夜通しの作業、\nコードを書く指とメスを握る指が\n偶然触れ合った瞬間、\n二人は目を合わせた。\n\n「君の手は人を救う手だ」\n「あなたの手もよ」\n異なる世界の天才が、\n同じ未来を見つめ始めた。"},
        {"ユウキ_ルナ", "SNSで炎上したルナ。\n誹謗中傷の嵐の中、\n匿名の誰かが彼女を守り続けた。\n\n悪質な投稿を消し、\n真実を広め、\n見えない騎士のように戦った。\n\nある日、IPアドレスを辿ったルナは\nユウキを見つけた。\n「なぜ私のために？」\n「君の笑顔を守りたかった」\nその日、二人は恋人になった。"},

        // ゴウ（傭兵）× 各母親
        {"ゴウ_サクラ", "暗殺任務で鉢合わせた二人。\n互いに銃口を向けながら、\n奇妙な沈黙が流れた。\n\n「お前を殺す理由がない」\n「私もよ」\n\n銃を下ろした瞬間、\n組織に追われる身となった。\n\n「一緒に逃げないか」\n「どこまでも」\n\n世界中を逃げ回る日々が、\n二人を離れられない関係にした。"},
        {"ゴウ_ヒナタ", "要人警護の任務。\n標的にされたのはヒナタだった。\n\n三度の暗殺未遂、\nその全てからゴウは彼女を守った。\n三発目の銃弾を\n自らの体で受け止めた時、\nヒナタは悟った。\n\n「お金じゃ買えないものがある」\n\n病室で目覚めたゴウに、\n彼女は涙ながらに言った。\n「私の人生を守って」"},
        {"ゴウ_アキラ", "紛争地帯でのスポーツ親善大使。\nアキラの警護を任されたゴウは、\n彼女の無邪気さに戸惑った。\n\n「怖くないのか？」\n「あなたがいるから」\n\n銃声の中でも笑顔を絶やさない彼女。\n守るべき存在が、\nいつしか愛する人に変わっていた。\n\n任務終了の日、\nゴウは傭兵を辞める決意をした。"},
        {"ゴウ_ミサト", "軍事衛星のデータ解析依頼。\n冷徹な傭兵ゴウと、\n純粋な物理学者ミサト。\n\n「なぜ人を殺すの？」\n直球の質問に、\nゴウは言葉を失った。\n\n「...答えが見つからない」\n「一緒に探しましょう」\n\nミサトの純粋さが、\n凍った心を少しずつ溶かしていく。\n戦場の狼が愛を知った瞬間だった。"},
        {"ゴウ_カエデ", "戦場で倒れた仲間を救うため、\nゴウは国境を越えて\n天才外科医を探した。\n\n「報酬はいくらでも払う」\n「お金じゃないの。\nあなたが連れてきて」\n\n危険な戦地に飛び込んだカエデ。\n命がけの手術を終えた夜、\nゴウは初めて泣いた。\n\n「俺の人生を守ってくれないか」\n傭兵の不器用なプロポーズだった。"},
        {"ゴウ_ルナ", "戦場カメラマンとして同行したルナ。\n「真実を伝えたい」という\n彼女の覚悟に、ゴウは驚いた。\n\n砲撃の夜、塹壕で肩を寄せ合い、\n生と死の狭間で\n二人は唇を重ねた。\n\n「生きて帰ろう」\n「ああ、一緒にな」\n\n戦場で誓った愛は、\nどんな平和な恋より強く、\n深く結ばれていた。"},

        // シンジ（天才科学者）× 各母親
        {"シンジ_サクラ", "「暗殺拳の科学的解明」\nその研究テーマに、\nサクラは協力を申し出た。\n\n動きを解析するうちに、\nシンジの目は彼女自身に向いていた。\n\n「論文より君を研究したい」\n「それ、口説いてる？」\n「...多分」\n\n世界一不器用な告白に、\n暗殺者は初めて頬を染めた。\n愛は科学で証明できないと知った。"},
        {"シンジ_ヒナタ", "「美の方程式」を共著で出版したい。\nヒナタからの依頼に、\nシンジは興味を持った。\n\n数式とビジネス、\n異色のコラボレーション。\n\nグラフを描くうちに、\n二人の線は一点で交わった。\n\n「この交点が僕たちの未来だ」\n「ロマンチストね、意外と」\n\n28兆円の女帝が、\n数式に恋をした日だった。"},
        {"シンジ_アキラ", "「人体の限界」を科学する研究。\n被験者として現れたアキラの\n笑顔を見た瞬間、\nシンジの心拍データは乱れた。\n\n「先生、大丈夫？」\n「い、異常値が出ている...\n僕の心臓に」\n\n「それ、恋って言うんですよ」\nアキラの言葉に、\n天才科学者は顔を真っ赤にした。\n答えは最初から出ていたのだ。"},
        {"シンジ_ミサト", "国際物理学会での激論。\n「あなたの理論は穴だらけよ」\n「君こそ基礎が甘い」\n\n壇上で火花を散らした二人は、\nなぜかホテルのバーで再会した。\n\nIQ250とIQ270。\n合わせて520の恋が始まる。\n\n「数式より美しいものを見つけた」\n「何？」\n「君だよ」\n天才にしては陳腐な台詞だった。"},
        {"シンジ_カエデ", "ノーベル医学賞授賞式。\n偶然隣り合わせになった二人は、\n授賞式そっちのけで議論を始めた。\n\n「君の論文、3箇所間違ってる」\n「あなたこそ、5箇所よ」\n\n火花を散らす天才同士。\nだがパーティーが終わる頃には、\n互いを認め合っていた。\n\n「共同研究しないか」\n「いいわ、人生のパートナーとして」\n人類最高の遺伝子が誕生した。"},
        {"シンジ_ルナ", "「完璧な顔の数学的定義」\nその研究のため、\nルナがモデルとして協力した。\n\n何百枚もの写真、\n何千ものデータポイント。\n\n「結論が出たよ」\n「どんな顔が完璧なの？」\n「君だ。君以外にない」\n\n論文には書けない結論だった。\n美しさの究極の答えは、\n愛する人の顔だと気づいた。"},

        // リョウマ（実業家）× 各母親
        {"リョウマ_サクラ", "ボディガードとして雇った暗殺者。\n命を預けた相手に、\n心まで奪われるとは思わなかった。\n\n「金で動く女か」\n「いいえ、あなたを守りたいから」\n\n嘘のない瞳だった。\n\n43兆円あっても買えないもの。\nそれは信頼と愛だと、\nリョウマは初めて知った。\n「俺の傍にいてくれ、永遠に」"},
        {"リョウマ_ヒナタ", "美容帝国との合併話。\n二つの巨大企業、\n最初は敵対から始まった。\n\n「あなたには負けないわ」\n「俺もだ」\n\n激しい交渉の末、\n二人は互いを認め合った。\n\n「合併より、\n結婚しないか」\n「...それ、逆じゃない？」\n\n71兆円の帝国が誕生した。\n株式より価値ある絆と共に。"},
        {"リョウマ_アキラ", "スポーツ球団買収の記者会見。\n看板選手アキラとの握手の瞬間、\n世界一の資産家は恋に落ちた。\n\n「君をチームの顔にしたい」\n「顔じゃなくて、\n私を見てほしいな」\n\n真っ直ぐな言葉が胸を打った。\n\n株価より大切なもの。\n利益より価値あるもの。\nそれは彼女の笑顔だった。"},
        {"リョウマ_ミサト", "研究所への100億円の投資。\nその見返りに求めたのは、\n論文でも特許でもなかった。\n\n「週に一度、\n一緒に星を見てほしい」\n\nミサトは驚きながらも頷いた。\n\n屋上で星を眺める夜が続き、\n宇宙の話から人生の話へ。\n\n「君という星を見つけた」\n物理学者は、\nその方程式を解けなかった。"},
        {"リョウマ_カエデ", "病院チェーンのM&A交渉。\nビジネスランチのはずが、\n話は医療の未来へと広がった。\n\n「君の理想を実現するには\nいくら必要だ？」\n「お金の問題じゃないの」\n\nその言葉に、リョウマは衝撃を受けた。\n43兆円の資産が無意味に思えた。\n\n「なら、僕の人生を投資させてくれ」\n契約書にない想いを込めた。"},
        {"リョウマ_ルナ", "プライベートジェットで偶然の隣席。\nパリへ向かう12時間、\n二人は語り合った。\n\n仕事のこと、夢のこと、\n誰にも言えない弱さのこと。\n\n雲の上、地上から離れた空間で、\n肩書きも資産も意味を失った。\n\n着陸した時、\n二人は恋人になっていた。\n「地上に降りても、この気持ちは変わらない」"},

        // テツヤ（ロックスター）× 各母親
        {"テツヤ_サクラ", "新曲MVの殺陣シーン。\n指導者として現れたサクラの\n鋭い動きに、テツヤは見惚れた。\n\n「もっと本気で来て」\n「怪我させるぞ」\n「それくらいが丁度いい」\n\nステージで刃を交えるうちに、\n二人の距離は縮まっていった。\n\n撮影終了後の楽屋で、\n二人は激しく唇を重ねた。"},
        {"テツヤ_ヒナタ", "化粧品CMソングの打ち合わせ。\n譜面を見るふりをして、\nテツヤはヒナタを見つめていた。\n\n「曲より私を見てない？」\n「バレた？」\n「わかりやすいのよ、あなた」\n\nスタジオに響く笑い声。\nその日、二人は朝まで語り合った。\n\n「君のための歌を書きたい」\n「それ、プロポーズ？」\n「かもしれない」"},
        {"テツヤ_アキラ", "オリンピック応援ソングの依頼。\n「勝利の歌を書いてほしい」\n\nアキラの走る姿を見て、\nテツヤのペンが走り出した。\n\nスタジアムに響く歌声、\n金メダルを取った瞬間、\nアキラはテツヤのもとへ走った。\n\n「この歌があったから勝てた」\n「君がいたから書けた」\n\n金メダルより輝く愛が生まれた。"},
        {"テツヤ_ミサト", "「音楽と物理学の共通点」\n雑誌のインタビューで出会った二人。\n\n「音は波でしょ？\n愛も波かもしれない」\n「周波数が合えば共鳴する...」\n「そう、今の僕たちみたいに」\n\n理屈っぽい会話が心地よかった。\n\nインタビューは終わっても、\n二人の会話は終わらなかった。\n共鳴した心は離れられない。"},
        {"テツヤ_カエデ", "ライブ中に声が出なくなった。\n「二度と歌えない」\n絶望するテツヤを救ったのは、\n天才外科医カエデだった。\n\n奇跡の手術から3ヶ月、\n声を取り戻した日、\nテツヤは病院でゲリラライブを開いた。\n\n「最初の歌は君に捧げる」\nそれは愛の歌だった。\nカエデは涙を流しながら聴いていた。"},
        {"テツヤ_ルナ", "ワールドツアー、50都市。\n同じ夢を追う二人は、\n世界中を一緒に回った。\n\n「疲れないか？」\n「あなたがいるから平気」\n\nステージの上と、ランウェイの上。\n輝く場所は違っても、\n見つめ合う瞳は同じだった。\n\n最後の公演、アンコール。\nテツヤはステージ上で跪いた。\n「結婚してくれ」\n8億人のファンが証人となった。"},
    };

    // ストーリー表示用UI (UI Toolkit)
    UIE.VisualElement storyPanel;
    UIE.Label storyText;
    bool waitingForStoryConfirm;

    // ストーリー画面の親カード用 (UI Toolkit)
    UIE.VisualElement storyFatherFace, storyMotherFace;
    UIE.Label storyFatherName, storyMotherName;
    UIE.Label storyFatherIntro, storyMotherIntro;

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

        // いでよGodBabyボタンをパチンコ風赤ボタンにスタイリング
        StylePachinkoButton(generateLifeButton);

        StylePillButton(anotherGalButton, 700, 120, Localization.Get("birth_reroll_button"), 36);
        StylePillButton(gotoBattleButton, 700, 120, Localization.Get("birth_name_button"), 36);

        // ボタンを縦組に配置（名前をつけるが上、もう一度が下）
        if (gotoBattleButton != null)
        {
            var rect = gotoBattleButton.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(0, -662);
        }
        if (anotherGalButton != null)
        {
            var rect = anotherGalButton.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(0, -802);
        }
        CreateBabyFaceUI(); // babyFaceを作成
        CreateFlashOverlay();
        CreateLightningOverlay();
        CreateIntroPanel();
        //CreateCharacterListUI();

        // UI Toolkit overlay (menu, name input, save confirm, parent bio, parent cards, story)
        overlayPanelSettings = UIHelper.CreatePanelSettings(10f);
        var overlayObj = new GameObject("BirthOverlayUI");
        overlayObj.transform.SetParent(transform, false);
        overlayRoot = UIHelper.SetupUIDocument(overlayObj,
            new[] { "UI/CommonStyle", "UI/BirthStyle" }, overlayPanelSettings);
        overlayRoot.pickingMode = UIE.PickingMode.Ignore;
        CreateParentUI();
        if (parentPanel != null) parentPanel.style.display = UIE.DisplayStyle.None;
        CreateStoryUI();
        CreateMenuBar();
        CreateNameInputUI();
        CreateSaveConfirmUI();
    }

    void OnDestroy()
    {
        if (overlayPanelSettings != null)
            Destroy(overlayPanelSettings);
    }

    // ===== ボタンから呼ばれるメソッド =====

    public void SpinRoulette()
    {
        Debug.Log("[BirthSystem] SpinRoulette button clicked");
        if (isAnimating) return;

        // イントロパネルを非表示
        if (introPanel != null)
            introPanel.SetActive(false);

        StartCoroutine(PachinkoStartSequence());
    }

    IEnumerator PachinkoStartSequence()
    {
        isAnimating = true;

        // ボタンのエフェクトを停止、PUSHロゴ非表示
        if (generateLifeButton != null)
        {
            var effect = generateLifeButton.GetComponent<PachinkoButtonEffect>();
            if (effect != null) effect.enabled = false;
        }


        // ── フェーズ1: ボタンが震える（3秒） ──
        float shakeDuration = 3f;
        float elapsed = 0f;
        float shakeIntensity = 3f;
        Vector3 originalPos = Vector3.zero;
        RectTransform btnRect = null;

        if (generateLifeButton != null)
        {
            btnRect = generateLifeButton.GetComponent<RectTransform>();
            if (btnRect != null) originalPos = btnRect.anchoredPosition;
        }

        // 震えが徐々に激しくなる
        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / shakeDuration;
            // 強度は時間とともに増す（最初は微振動、最後は激しく）
            float currentIntensity = Mathf.Lerp(shakeIntensity, shakeIntensity * 8f, progress * progress);
            if (btnRect != null)
            {
                float offX = Random.Range(-currentIntensity, currentIntensity);
                float offY = Random.Range(-currentIntensity, currentIntensity);
                btnRect.anchoredPosition = new Vector2(originalPos.x + offX, originalPos.y + offY);
            }
            yield return null;
        }

        // 位置リセット
        if (btnRect != null)
            btnRect.anchoredPosition = new Vector2(originalPos.x, originalPos.y);

        // ── フェーズ2: 雷演出 + フラッシュ ──
        if (lightningContainer != null) lightningContainer.SetActive(true);

        for (int i = 0; i < 4; i++)
        {
            var bolt = CreateLightningBolt();

            if (flashOverlay != null)
            {
                flashOverlay.gameObject.SetActive(true);
                flashOverlay.color = new Color(1f, 1f, 0.8f, 0.6f);
            }

            // 画面全体も震わせる
            if (canvas != null)
            {
                var cRect = canvas.GetComponent<RectTransform>();
                if (cRect != null)
                {
                    cRect.anchoredPosition = new Vector2(
                        Random.Range(-12f, 12f), Random.Range(-12f, 12f));
                }
            }

            yield return new WaitForSeconds(0.08f);

            if (flashOverlay != null)
            {
                flashOverlay.color = new Color(1f, 1f, 1f, 0f);
                flashOverlay.gameObject.SetActive(false);
            }

            yield return new WaitForSeconds(0.06f);
            if (bolt != null) Destroy(bolt);
            yield return new WaitForSeconds(Random.Range(0.05f, 0.12f));
        }

        // 最後の大フラッシュ（白）
        if (flashOverlay != null)
        {
            flashOverlay.gameObject.SetActive(true);
            flashOverlay.color = new Color(1f, 1f, 1f, 0.95f);
        }

        yield return new WaitForSeconds(0.15f);

        // Canvas位置リセット
        if (canvas != null)
        {
            var cRect = canvas.GetComponent<RectTransform>();
            if (cRect != null) cRect.anchoredPosition = Vector2.zero;
        }

        if (lightningContainer != null) lightningContainer.SetActive(false);

        // フラッシュフェードアウト
        if (flashOverlay != null)
        {
            float fadeDur = 0.5f;
            float fadeElapsed = 0f;
            while (fadeElapsed < fadeDur)
            {
                fadeElapsed += Time.deltaTime;
                float a = Mathf.Lerp(0.95f, 0f, fadeElapsed / fadeDur);
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

    public void ResetParents()
    {
        if (isAnimating) return;
        SceneManager.LoadScene("BirthScene");
    }

    public void GoToBattle()
    {
        StartCoroutine(NameAndSaveSequence());
    }

    IEnumerator NameAndSaveSequence()
    {
        // 名前をつける・もう一度ボタンを無効化
        if (gotoBattleButton != null) gotoBattleButton.SetActive(false);
        if (anotherGalButton != null) anotherGalButton.SetActive(false);

        // 名前入力ダイアログ表示
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

        // DataCarrierに名前を保存（スロットへの自動セーブはしない）
        if (DataCarrier.Instance != null)
        {
            DataCarrier.Instance.babyName = enteredName;
            DataCarrier.Instance.currentSlot = -1; // 新規なのでスロット未割当
        }

        // バトルシーンへ
        SceneManager.LoadScene("BattleScene");
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

        // ボタンを即非表示、?マークを隠す、背景も非表示
        if (generateLifeButton != null) generateLifeButton.SetActive(false);

        if (anotherGalButton != null) anotherGalButton.SetActive(false);
        if (gotoBattleButton != null) gotoBattleButton.SetActive(false);
        if (introText != null) introText.gameObject.SetActive(false);

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

        // 紹介文を非表示にリセット
        if (fatherIntroText != null) fatherIntroText.style.display = UIE.DisplayStyle.None;
        if (motherIntroText != null) motherIntroText.style.display = UIE.DisplayStyle.None;
        if (nextButtonEl != null) nextButtonEl.style.display = UIE.DisplayStyle.None;

        // 父親カードのみ表示、母親カードは非表示
        if (fatherCard != null) fatherCard.style.display = UIE.DisplayStyle.Flex;
        if (motherCard != null) motherCard.style.display = UIE.DisplayStyle.None;
        // ── フェーズ2: 父親ルーレット（父親カードのみ表示） ──
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
        // カットイン演出
        yield return StartCoroutine(ShowParentCutin(father.name));
        // 紹介文表示
        if (fatherIntroText != null)
        {
            fatherIntroText.text = Localization.GetParentIntro(father.name).Replace(" / ", "\n");
            fatherIntroText.style.display = UIE.DisplayStyle.Flex;
        }
        // 「愛する女性を探す」ボタンで待機
        yield return new WaitForSeconds(0.5f);
        CreateParentInfoButton(father.name, fatherCard);
        SetNextButtonText(Localization.Get("birth_find_mother"));
        if (nextButtonEl != null) nextButtonEl.style.display = UIE.DisplayStyle.Flex;
        waitingForNext = true;
        while (waitingForNext) yield return null;
        if (nextButtonEl != null) nextButtonEl.style.display = UIE.DisplayStyle.None;
        DestroyParentInfoButton();
        SetNextButtonText(Localization.Get("birth_next"));

        // ── フェーズ3: 「惹かれ合う、もう一つの魂」カットイン → 母親ルーレット ──
        yield return StartCoroutine(ShowMotherCutinText(Localization.Get("cutin_who_mother")));

        if (fatherCard != null) fatherCard.style.display = UIE.DisplayStyle.None;
        if (motherCard != null) motherCard.style.display = UIE.DisplayStyle.Flex;
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
        // カットイン演出
        yield return StartCoroutine(ShowParentCutin(mother.name));
        // 紹介文表示
        if (motherIntroText != null)
        {
            motherIntroText.text = Localization.GetParentIntro(mother.name).Replace(" / ", "\n");
            motherIntroText.style.display = UIE.DisplayStyle.Flex;
        }
        // 「恋の始まり」ボタンで待機
        yield return new WaitForSeconds(0.5f);
        CreateParentInfoButton(mother.name, motherCard);
        SetNextButtonText(Localization.Get("birth_love_begin"));
        if (nextButtonEl != null) nextButtonEl.style.display = UIE.DisplayStyle.Flex;
        waitingForNext = true;
        while (waitingForNext) yield return null;
        if (nextButtonEl != null) nextButtonEl.style.display = UIE.DisplayStyle.None;
        DestroyParentInfoButton();
        SetNextButtonText(Localization.Get("birth_next"));

        // ── フェーズ4: 性別をランダム決定 ──
        selectedGender = Random.Range(0, 2) == 0 ? "男の子" : "女の子";
        Debug.Log($"[BirthSystem] Gender random: {selectedGender}");

        // 両親カード非表示
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

        // ── フェーズ4.7: サクラ（暗殺拳）の場合20%で子供に恵まれない ──
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

        // ── フェーズ5.5: レイアウト切り替え（親を端に、赤ちゃんを大きく中央に） ──
        Debug.Log("[BirthSystem] Phase 5.5: Layout switch and baby display");
        SwitchToBirthResultLayout();

        // ── フェーズ6: ステータス1行ずつ表示 ──
        // 上位1%判定（GOD BABY判定）、上位10%判定（大物判定）
        bool isGodBaby = IsGodBaby(c_atk, c_def, c_hp, c_intelligence, c_athletic);

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
            GenerateBabyFace(c_atk, c_intelligence, c_athletic, selectedGender, isGodBaby);
        }

        // 画像アップロードボタン（赤ちゃん画像の下に配置）
        if (birthResultCard != null) CreateUploadButton();

        bool isPromisingBaby = !isGodBaby && IsPromisingBaby(c_atk, c_def, c_hp, c_intelligence, c_athletic);

        // ── 稲妻演出（GOD BABY or 大物の場合） ──
        if (isGodBaby || isPromisingBaby)
        {
            yield return StartCoroutine(LightningEffect(isGodBaby));
        }

        // ステータステキストはカード内に配置済み

        string genderColor = selectedGender == "男の子" ? "#66ccff" : "#ff99cc";
        string displayGender = Localization.GetGender(selectedGender);

        // 性別は画像の上に表示
        if (genderLabelEl != null)
        {
            genderLabelEl.text = $"{Localization.Get("birth_stat_gender")}  <color={genderColor}>{displayGender}</color>";
        }

        string stat1 = $"{Localization.Get("birth_stat_hp")} {c_hp}    {Localization.Get("birth_stat_atk")} {c_atk}    {Localization.Get("birth_stat_def")} {c_def}";
        string stat2 = $"{Localization.Get("birth_stat_intelligence")} {c_intelligence}    {Localization.Get("birth_stat_athletic")} {c_athletic}";
        string stat3 = $"{Localization.Get("birth_stat_luck")} {c_luck}    {Localization.Get("birth_stat_fortune")} {c_fortune}";
        string stat4 = $"{Localization.Get("birth_stat_trait")}  <color=#FFA500>{Localization.GetTrait(trait1)}</color>";

        if (childStatusText != null) childStatusText.text = stat1;
        yield return new WaitForSeconds(0.2f);
        if (childStatusText != null) childStatusText.text = stat1 + "\n\n" + stat2;
        yield return new WaitForSeconds(0.15f);
        if (childStatusText != null) childStatusText.text = stat1 + "\n\n" + stat2 + "\n\n" + stat3;
        yield return new WaitForSeconds(0.25f);
        if (childStatusText != null) childStatusText.text = stat1 + "\n\n" + stat2 + "\n\n" + stat3 + "\n\n" + stat4;

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
        UIE.VisualElement faceImg = isFather ? fatherFaceImage : motherFaceImage;
        UIE.Label nameT = isFather ? fatherNameText : motherNameText;
        Sprite[] sprites = isFather ? fatherSprites : motherSprites;
        string prefix = isFather ? Localization.Get("birth_father_prefix") : Localization.Get("birth_mother_prefix");

        if (faceImg != null)
        {
            Sprite sp = (sprites != null && sprites.Length > idx) ? sprites[idx] : null;
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
        // 紹介文を非表示
        if (fatherIntroText != null) fatherIntroText.style.display = UIE.DisplayStyle.None;
        if (motherIntroText != null) motherIntroText.style.display = UIE.DisplayStyle.None;

        // 両カードを表示
        if (fatherCard != null) fatherCard.style.display = UIE.DisplayStyle.Flex;
        if (motherCard != null) motherCard.style.display = UIE.DisplayStyle.Flex;

        // カードを小さくして左右に並べる
        if (fatherCard != null)
        {
            fatherCard.style.width = 492;
            fatherCard.style.height = 600;
            fatherCard.style.translate = new UIE.StyleTranslate(
                new UIE.Translate(new UIE.Length(-50, UIE.LengthUnit.Percent), -480));
            fatherCard.style.left = new UIE.StyleLength(new UIE.Length(25, UIE.LengthUnit.Percent));
        }
        if (motherCard != null)
        {
            motherCard.style.width = 492;
            motherCard.style.height = 600;
            motherCard.style.translate = new UIE.StyleTranslate(
                new UIE.Translate(new UIE.Length(-50, UIE.LengthUnit.Percent), -480));
            motherCard.style.left = new UIE.StyleLength(new UIE.Length(75, UIE.LengthUnit.Percent));
        }

        // 顔画像を小さくリサイズ
        if (fatherFaceImage != null)
        {
            fatherFaceImage.style.width = 380;
            fatherFaceImage.style.height = 380;
        }
        if (motherFaceImage != null)
        {
            motherFaceImage.style.width = 380;
            motherFaceImage.style.height = 380;
        }

        // 確定済みの親を表示
        ShowSingleParentPreview(fIdx, father, true);
        ShowSingleParentPreview(mIdx, mother, false);
    }

    // カードをルーレット用の大きいサイズに戻す
    void ResetCardToFullSize(UIE.VisualElement card, UIE.VisualElement faceImg)
    {
        if (card == null) return;
        card.style.width = 1016;
        card.style.height = 1300;
        card.style.left = new UIE.StyleLength(new UIE.Length(50, UIE.LengthUnit.Percent));
        card.style.translate = new UIE.StyleTranslate(
            new UIE.Translate(new UIE.Length(-50, UIE.LengthUnit.Percent), -750));

        if (faceImg != null)
        {
            faceImg.style.width = 850;
            faceImg.style.height = 850;
        }
    }

    // ===== ステータス判定 =====

    // DetermineTrait は廃止 — 16種からランダムに選ばれる

    /// <summary>
    /// 上位1%の「GOD BABY」判定
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

        // 単一ステータスが極端に高い場合も GOD BABY
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

        // ── 「次へ」ボタン ──
        var nextWrapper = new UIE.VisualElement();
        nextWrapper.AddToClassList("birth-next-btn-wrapper");

        var shadow = new UIE.VisualElement();
        shadow.AddToClassList("shadow-layer");
        nextWrapper.Add(shadow);

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
        waitingForNext = false;
    }

    void CreateParentInfoButton(string parentName, UIE.VisualElement targetCard)
    {
        if (targetCard == null) return;
        DestroyParentInfoButton();

        parentInfoButton = new UIE.Button();
        parentInfoButton.AddToClassList("birth-parent-info-btn");
        UIHelper.ApplyFont(parentInfoButton);
        parentInfoButton.text = "i";
        string bioName = parentName;
        parentInfoButton.clicked += () => ShowParentBioPanel(bioName);
        targetCard.Add(parentInfoButton);
    }

    void DestroyParentInfoButton()
    {
        if (parentInfoButton != null)
        {
            parentInfoButton.RemoveFromHierarchy();
            parentInfoButton = null;
        }
    }

    void ShowParentBioPanel(string parentName)
    {
        if (parentBioOverlayEl != null) return;

        parentBioOverlayEl = UIHelper.CreateOverlay();
        parentBioOverlayEl.RegisterCallback<UIE.ClickEvent>(evt =>
        {
            if (evt.target == parentBioOverlayEl) CloseParentBioPanel();
        });

        var card = new UIE.VisualElement();
        card.AddToClassList("birth-bio-card");

        // Close button
        var closeBtn = new UIE.Button();
        closeBtn.AddToClassList("birth-bio-close");
        UIHelper.ApplyFont(closeBtn);
        closeBtn.text = "\u00d7";
        closeBtn.clicked += CloseParentBioPanel;
        card.Add(closeBtn);

        // Name
        var nameLabel = UIHelper.CreateLabel(Localization.GetParent(parentName), "birth-bio-name");
        card.Add(nameLabel);

        // Separator
        var sep = new UIE.VisualElement();
        sep.AddToClassList("birth-bio-separator");
        card.Add(sep);

        // Bio text
        string bio = Localization.GetParentBio(parentName);
        if (!string.IsNullOrEmpty(bio))
        {
            var bioLabel = UIHelper.CreateLabel(bio, "birth-bio-text");
            card.Add(bioLabel);
        }

        parentBioOverlayEl.Add(card);
        overlayRoot.Add(parentBioOverlayEl);
    }

    void CloseParentBioPanel()
    {
        if (parentBioOverlayEl != null)
        {
            parentBioOverlayEl.RemoveFromHierarchy();
            parentBioOverlayEl = null;
        }
    }

    void CreateParentCard(UIE.VisualElement parent, out UIE.VisualElement cardObj, out UIE.VisualElement faceImage,
        out UIE.Label nameText, out UIE.Label introText, string role)
    {
        var card = new UIE.VisualElement();
        card.AddToClassList("birth-parent-card");
        card.AddToClassList($"birth-parent-card-{role}");
        card.style.left = new UIE.StyleLength(new UIE.Length(50, UIE.LengthUnit.Percent));
        card.style.translate = new UIE.StyleTranslate(
            new UIE.Translate(new UIE.Length(-50, UIE.LengthUnit.Percent), -750));
        cardObj = card;

        // カード内側背景
        var inner = new UIE.VisualElement();
        inner.AddToClassList("birth-parent-inner");
        inner.AddToClassList($"birth-parent-inner-{role}");
        card.Add(inner);

        // 名前テキスト (上部)
        nameText = UIHelper.CreateLabel("", "birth-parent-name");
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

        // 紹介文テキスト (下部)
        introText = UIHelper.CreateLabel("", "birth-parent-intro");
        introText.style.display = UIE.DisplayStyle.None;
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
    void SwitchToBirthResultLayout()
    {
        if (overlayRoot == null) return;

        // 親パネルを非表示
        if (parentPanel != null) parentPanel.style.display = UIE.DisplayStyle.None;

        // 既存のカードがあれば削除
        if (birthResultCard != null) birthResultCard.RemoveFromHierarchy();

        // 赤ちゃんカードを作成（UI Toolkit）
        birthResultCard = new UIE.VisualElement();
        birthResultCard.AddToClassList("birth-result-card");

        // カード内側背景
        var inner = new UIE.VisualElement();
        inner.AddToClassList("birth-result-inner");
        Sprite babyBgSprite = Resources.Load<Sprite>("BackGrounds/baby-background");
        if (babyBgSprite != null)
            inner.style.backgroundImage = new UIE.StyleBackground(babyBgSprite);
        birthResultCard.Add(inner);

        // 性別ラベル
        genderLabelEl = UIHelper.CreateLabel("");
        genderLabelEl.AddToClassList("birth-result-gender");
        genderLabelEl.enableRichText = true;
        birthResultCard.Add(genderLabelEl);

        // 区切り線
        var separator = new UIE.VisualElement();
        separator.AddToClassList("birth-result-separator");
        birthResultCard.Add(separator);

        // ステータスカード（下部）
        statusCardObj = new UIE.VisualElement();
        statusCardObj.AddToClassList("birth-result-status-card");

        var scInner = new UIE.VisualElement();
        scInner.AddToClassList("birth-result-status-inner");

        childStatusText = UIHelper.CreateLabel("");
        childStatusText.AddToClassList("birth-result-status-text");
        childStatusText.enableRichText = true;
        scInner.Add(childStatusText);

        statusCardObj.Add(scInner);
        birthResultCard.Add(statusCardObj);

        overlayRoot.Add(birthResultCard);

        // babyFaceをCanvas上で絶対位置配置（カードの中央部分に重なるように）
        if (babyFace != null)
        {
            babyFace.SetParent(canvas.transform, false);
            babyFace.anchorMin = new Vector2(0.5f, 0.5f);
            babyFace.anchorMax = new Vector2(0.5f, 0.5f);
            // カード中央Y=150, ステータスカード上端からの計算
            float cardOffsetY = 150f;
            float statusCardTopInCard = 1300f * 0.3f - 1300f / 2f; // = 390 - 650 = -260
            float faceSize = 1016f * 0.94f; // ≈ 955
            float faceCenterY = statusCardTopInCard + 32f + faceSize / 2f;
            babyFace.anchoredPosition = new Vector2(0, cardOffsetY + faceCenterY);
            babyFace.sizeDelta = new Vector2(faceSize, faceSize);
            babyFace.SetAsLastSibling();
        }
    }

    // ルーレット用のレイアウトにリセット
    void ResetToRouletteLayout()
    {
        // 情報パネル系をクリーンアップ
        DestroyParentInfoButton();
        CloseParentBioPanel();

        // カードを元のサイズに戻す
        ResetCardToFullSize(fatherCard, fatherFaceImage);
        ResetCardToFullSize(motherCard, motherFaceImage);

        // カスタム画像要素を削除
        if (customBabyImageEl != null) { customBabyImageEl.RemoveFromHierarchy(); customBabyImageEl = null; }

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

        // birthResultCardを削除（UI Toolkit）
        if (uploadImageBtn != null) { uploadImageBtn.RemoveFromHierarchy(); uploadImageBtn = null; }
        if (birthResultCard != null)
        {
            birthResultCard.RemoveFromHierarchy();
            birthResultCard = null;
            genderLabelEl = null;
            statusCardObj = null;
            childStatusText = null;
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
        }
        else
        {
            mainBgImg.color = new Color(0.953f, 0.969f, 0.973f);
        }
    }

    // ===== 画像アップロード機能 =====

    void CreateUploadButton()
    {
        if (uploadImageBtn != null) uploadImageBtn.RemoveFromHierarchy();
        if (birthResultCard == null) return;

        uploadImageBtn = new UIE.Button();
        uploadImageBtn.AddToClassList("birth-result-upload-btn");
        UIHelper.ApplyFont(uploadImageBtn);
        uploadImageBtn.text = "\u270E";
        uploadImageBtn.clicked += PickImageFromGallery;
        birthResultCard.Add(uploadImageBtn);
    }

    void PickImageFromGallery()
    {
        Debug.Log("[BirthSystem] PickImageFromGallery called");

#if UNITY_EDITOR
        // Editorではファイルダイアログを直接使用
        string path = UnityEditor.EditorUtility.OpenFilePanel("赤ちゃんの画像を選択", "", "");
        Debug.Log($"[BirthSystem] Editor file dialog returned: '{path}'");
        if (!string.IsNullOrEmpty(path))
            LoadAndApplyImage(path);
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
            byte[] fileData = File.ReadAllBytes(path);
            Debug.Log($"[BirthSystem] File read: {fileData.Length} bytes");

            string fileName = "baby_custom.png";
            string savePath = Path.Combine(Application.persistentDataPath, fileName);
            File.WriteAllBytes(savePath, fileData);

            var tex = new Texture2D(2, 2);
            if (!tex.LoadImage(fileData))
            {
                Debug.LogError("[BirthSystem] Failed to load image from gallery");
                Destroy(tex);
                return;
            }
            Debug.Log($"[BirthSystem] Texture loaded: {tex.width}x{tex.height}");

            // 大きすぎる画像はリサイズ（メモリ節約）
            int maxSize = 1024;
            if (tex.width > maxSize || tex.height > maxSize)
            {
                float scale = Mathf.Min((float)maxSize / tex.width, (float)maxSize / tex.height);
                int newW = Mathf.RoundToInt(tex.width * scale);
                int newH = Mathf.RoundToInt(tex.height * scale);
                var rt = RenderTexture.GetTemporary(newW, newH);
                Graphics.Blit(tex, rt);
                var prev = RenderTexture.active;
                RenderTexture.active = rt;
                var resized = new Texture2D(newW, newH, TextureFormat.RGBA32, false);
                resized.ReadPixels(new Rect(0, 0, newW, newH), 0, 0);
                resized.Apply();
                RenderTexture.active = prev;
                RenderTexture.ReleaseTemporary(rt);
                Destroy(tex);
                tex = resized;
                Debug.Log($"[BirthSystem] Resized to: {newW}x{newH}");
            }

            if (DataCarrier.Instance != null)
                DataCarrier.Instance.customBabyImagePath = fileName;

            Sprite spr = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            Debug.Log($"[BirthSystem] Sprite created, calling UpdateBabyFaceWithCustomImage");
            UpdateBabyFaceWithCustomImage(spr);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[BirthSystem] Gallery image load failed: {e.Message}\n{e.StackTrace}");
        }
    }

    void UpdateBabyFaceWithCustomImage(Sprite spr)
    {
        Debug.Log($"[BirthSystem] UpdateBabyFaceWithCustomImage - spr: {spr != null}, babyFace: {babyFace != null}, birthResultCard: {birthResultCard != null}");

        // Canvas上のbabyFaceを非表示にする（UI Toolkitで表示するため）
        if (babyFace != null)
            babyFace.gameObject.SetActive(false);

        // UI Toolkit側で画像を表示
        if (birthResultCard == null)
        {
            Debug.LogError("[BirthSystem] birthResultCard is null, cannot display custom image");
            return;
        }

        // 既存のカスタム画像要素があれば削除
        if (customBabyImageEl != null)
            customBabyImageEl.RemoveFromHierarchy();

        // VisualElementとして画像を表示（birthResultCard内、ステータスカードの上）
        customBabyImageEl = new UIE.VisualElement();
        customBabyImageEl.name = "custom-baby-image";
        customBabyImageEl.AddToClassList("birth-result-custom-image");
        customBabyImageEl.style.backgroundImage = new UIE.StyleBackground(spr);
        birthResultCard.Add(customBabyImageEl);

        // アップロードボタンを最前面に
        if (uploadImageBtn != null)
            uploadImageBtn.BringToFront();

        Debug.Log("[BirthSystem] Custom baby image displayed in UI Toolkit");
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

        var card = new UIE.VisualElement();
        card.AddToClassList("birth-name-card");

        var title = UIHelper.CreateLabel(Localization.Get("birth_name_input_title"), "birth-name-title");
        card.Add(title);

        nameTextField = new UIE.TextField();
        nameTextField.AddToClassList("birth-name-field");
        nameTextField.maxLength = 12;
        UIHelper.ApplyFont(nameTextField);
        card.Add(nameTextField);

        var confirmBtn = UIHelper.CreatePillButton(Localization.Get("ui_confirm"), "pill-button-medium");
        confirmBtn.clicked += OnNameConfirmUIToolkit;
        card.Add(confirmBtn);

        nameInputOverlayEl.Add(card);
        // Not added to overlayRoot yet — shown on demand
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

                // メタリックリム（外周 92%-100%）
                if (normDist > 0.92f)
                {
                    float rimT = (normDist - 0.92f) / 0.08f;
                    // クロームリム: 光の角度によるグラデーション
                    float rimAngle = (dy / dist + 1f) * 0.5f; // 0(下)〜1(上)
                    float rimLight = 0.3f + 0.5f * rimAngle + 0.15f * Mathf.Pow(rimAngle, 4f);
                    Color rimColor = new Color(rimLight * 0.85f, rimLight * 0.8f, rimLight * 0.75f, aa);
                    pixels[y * size + x] = rimColor;
                    continue;
                }

                // メタリック溝（88%-92%: 暗い線）
                if (normDist > 0.88f)
                {
                    float grooveT = (normDist - 0.88f) / 0.04f;
                    float grooveV = 0.15f + 0.05f * Mathf.Sin(grooveT * Mathf.PI);
                    pixels[y * size + x] = new Color(grooveV, grooveV * 0.05f, grooveV * 0.05f, aa);
                    continue;
                }

                // 赤いドーム本体（0%-88%）
                float bodyNorm = normDist / 0.88f;

                // 3Dドーム：中央が明るく、端が暗い（放物線的）
                float dome = 1f - bodyNorm * bodyNorm;

                // 上方向からの照明（yが上ほど明るい）
                float lightDir = (dy / (outerR * 0.88f) + 1f) * 0.5f; // 0(下)〜1(上)

                // ベース赤色にドーム陰影と方向照明を合成
                float baseR = 0.65f + 0.35f * dome * (0.6f + 0.4f * lightDir);
                float baseG = 0.02f + 0.12f * dome * lightDir;
                float baseB = 0.02f + 0.08f * dome * lightDir;

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

        // ── 外側のグローオーラ ──
        var glowObj = new GameObject("Glow");
        glowObj.transform.SetParent(button.transform, false);
        glowObj.transform.SetAsFirstSibling();
        var glowRect = glowObj.AddComponent<RectTransform>();
        glowRect.anchorMin = new Vector2(0.5f, 0.5f);
        glowRect.anchorMax = new Vector2(0.5f, 0.5f);
        glowRect.sizeDelta = new Vector2(size + 100, size + 100);
        var glowImg = glowObj.AddComponent<Image>();
        glowImg.sprite = GetCircleSprite(128);
        glowImg.color = new Color(1f, 0.2f, 0.1f, 0.35f);
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
            rayImg.color = new Color(1f, 0.85f, 0.3f, 0.1f);
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
        baseImg.color = new Color(0.12f, 0.02f, 0.02f, 0.7f);
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

        // ── きらめきパーティクル（4つの小さな星） ──
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
            spImg.color = new Color(1f, 1f, 0.7f, 0.8f);
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
        CreateCharacterRow(listPanel.transform, Localization.Get("label_fathers"), Fathers, fatherSprites, 25);
        // 母親行
        CreateCharacterRow(listPanel.transform, Localization.Get("label_mothers"), Mothers, motherSprites, -25);
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

        // 背景画像（上下反転）— パネル背景色はUSS維持
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

        // 親カード行
        var cardsRow = new UIE.VisualElement();
        cardsRow.AddToClassList("birth-story-cards-row");

        // 左: 父親カード
        CreateStoryParentCard(cardsRow, true,
            out storyFatherFace, out storyFatherName, out storyFatherIntro);

        // 右: 母親カード
        CreateStoryParentCard(cardsRow, false,
            out storyMotherFace, out storyMotherName, out storyMotherIntro);

        storyPanel.Add(cardsRow);

        // ストーリーテキスト
        storyText = UIHelper.CreateLabel("");
        storyText.AddToClassList("birth-story-text");
        storyPanel.Add(storyText);

        // 「愛を育む」ボタン行
        var loveBtnRow = new UIE.VisualElement();
        loveBtnRow.AddToClassList("birth-story-love-btn-row");

        var shadow = new UIE.VisualElement();
        shadow.AddToClassList("shadow-layer");
        shadow.style.borderTopLeftRadius = 60;
        shadow.style.borderTopRightRadius = 60;
        shadow.style.borderBottomLeftRadius = 60;
        shadow.style.borderBottomRightRadius = 60;
        loveBtnRow.Add(shadow);

        var loveBtn = new UIE.Button();
        loveBtn.AddToClassList("birth-story-love-btn");
        UIHelper.ApplyFont(loveBtn);
        loveBtn.text = Localization.Get("birth_nurture_love");
        loveBtn.clicked += OnStoryTap;
        loveBtnRow.Add(loveBtn);

        storyPanel.Add(loveBtnRow);

        storyPanel.style.display = UIE.DisplayStyle.None;
        overlayRoot.Add(storyPanel);
    }

    void OnStoryTap()
    {
        waitingForStoryConfirm = false;
    }

    void CreateStoryParentCard(UIE.VisualElement parent, bool isFather,
        out UIE.VisualElement faceEl, out UIE.Label nameLabel, out UIE.Label introLabel)
    {
        var card = new UIE.VisualElement();
        card.AddToClassList("birth-story-card");

        // 名前テキスト (上部)
        nameLabel = UIHelper.CreateLabel("");
        nameLabel.AddToClassList("birth-story-card-name");
        card.Add(nameLabel);

        // 顔画像（角丸クリッピング）
        faceEl = new UIE.VisualElement();
        faceEl.AddToClassList("birth-story-card-face");
        card.Add(faceEl);

        // 紹介文テキスト (画像の下)
        introLabel = UIHelper.CreateLabel("");
        introLabel.AddToClassList("birth-story-card-intro");
        card.Add(introLabel);

        parent.Add(card);
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

        // ── 帯（画面中央の横帯） ──
        var band = new UIE.VisualElement();
        band.style.position = UIE.Position.Absolute;
        band.style.left = 0;
        band.style.right = 0;
        band.style.top = new UIE.StyleLength(new UIE.Length(38, UIE.LengthUnit.Percent));
        band.style.bottom = new UIE.StyleLength(new UIE.Length(38, UIE.LengthUnit.Percent));
        band.style.backgroundColor = new Color(0, 0, 0, 0);
        band.style.alignItems = UIE.Align.Center;
        band.style.justifyContent = UIE.Justify.Center;
        overlayRoot.Add(band);

        // ── テキスト ──
        var label = UIHelper.CreateLabel(cutinText);
        label.style.fontSize = 52;
        label.style.color = new Color(1, 1, 1, 0);
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
            band.style.backgroundColor = new Color(0, 0, 0, 0.8f * easeT);
            label.style.color = new Color(1, 1, 1, easeT);
            label.style.translate = new UIE.StyleTranslate(
                new UIE.Translate(slideOffset * (1f - easeT), 0));
            yield return null;
        }
        band.style.backgroundColor = new Color(0, 0, 0, 0.8f);
        label.style.color = Color.white;
        label.style.translate = new UIE.StyleTranslate(new UIE.Translate(0, 0));

        yield return new WaitForSeconds(holdDuration);

        elapsed = 0;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeOutDuration);
            band.style.backgroundColor = new Color(0, 0, 0, 0.8f * (1f - t));
            label.style.color = new Color(1, 1, 1, 1f - t);
            yield return null;
        }

        band.RemoveFromHierarchy();
    }

    IEnumerator ShowMotherCutinText(string cutinText)
    {
        if (overlayRoot == null || string.IsNullOrEmpty(cutinText)) yield break;

        // ── 帯（画面中央の横帯） ──
        var band = new UIE.VisualElement();
        band.style.position = UIE.Position.Absolute;
        band.style.left = 0;
        band.style.right = 0;
        band.style.top = new UIE.StyleLength(new UIE.Length(38, UIE.LengthUnit.Percent));
        band.style.bottom = new UIE.StyleLength(new UIE.Length(38, UIE.LengthUnit.Percent));
        band.style.backgroundColor = new Color(0, 0, 0, 0);
        band.style.alignItems = UIE.Align.Center;
        band.style.justifyContent = UIE.Justify.Center;
        overlayRoot.Add(band);

        // ── テキスト ──
        var label = UIHelper.CreateLabel(cutinText);
        label.style.fontSize = 52;
        label.style.color = new Color(1, 1, 1, 0);
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
            band.style.backgroundColor = new Color(0, 0, 0, 0.8f * easeT);
            label.style.color = new Color(1, 1, 1, easeT);
            label.style.scale = new UIE.StyleScale(new UIE.Scale(new Vector3(s, s, 1f)));
            yield return null;
        }
        band.style.backgroundColor = new Color(0, 0, 0, 0.8f);
        label.style.color = Color.white;
        label.style.scale = new UIE.StyleScale(new UIE.Scale(Vector3.one));

        yield return new WaitForSeconds(holdDuration);

        elapsed = 0;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeOutDuration);
            float easeT = t * t;
            float s = Mathf.Lerp(1.0f, 1.05f, easeT);
            band.style.backgroundColor = new Color(0, 0, 0, 0.8f * (1f - easeT));
            label.style.color = new Color(1, 1, 1, 1f - easeT);
            label.style.scale = new UIE.StyleScale(new UIE.Scale(new Vector3(s, s, 1f)));
            yield return null;
        }

        band.RemoveFromHierarchy();
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

        // ── Phase 1: 暗転 ──
        float elapsed = 0f;
        float fadeInDur = 0.8f;
        while (elapsed < fadeInDur && !skipRequested)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeInDur);
            panel.style.backgroundColor = new Color(0.01f, 0.005f, 0.04f, t * 0.97f);
            yield return null;
        }
        panel.style.backgroundColor = new Color(0.01f, 0.005f, 0.04f, 0.97f);

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
            lightCore.style.backgroundColor = new Color(1f, 1f, 0.92f, 0f);
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
                lightCore.style.backgroundColor = new Color(1f, 1f, 0.92f, easeT * 0.9f);
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
            textLabel.style.position = UIE.Position.Absolute;
            textLabel.style.left = 0;
            textLabel.style.right = 0;
            textLabel.style.top = new UIE.StyleLength(new UIE.Length(46, UIE.LengthUnit.Percent));
            textLabel.style.fontSize = 52;
            textLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            textLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            textLabel.style.color = new Color(1f, 0.95f, 0.82f, 0f);
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
                textLabel.style.color = new Color(1f, 0.95f, 0.82f, alpha);
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

        // ── Phase 1: 暗転 ──
        float elapsed = 0f;
        float fadeDur = 1.0f;
        while (elapsed < fadeDur && !skipRequested)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDur);
            panel.style.backgroundColor = new Color(0.01f, 0.01f, 0.03f, t * t * 0.98f);
            yield return null;
        }
        panel.style.backgroundColor = new Color(0.01f, 0.01f, 0.03f, 0.98f);
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
            seed.style.backgroundColor = new Color(1f, 1f, 0.95f, 0f);
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
                seed.style.backgroundColor = new Color(1f, 1f, 0.95f, t * t * 0.85f);
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
            textLabel.style.position = UIE.Position.Absolute;
            textLabel.style.left = 0; textLabel.style.right = 0;
            textLabel.style.top = new UIE.StyleLength(new UIE.Length(44, UIE.LengthUnit.Percent));
            textLabel.style.fontSize = 46;
            textLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            textLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            textLabel.style.color = new Color(1f, 1f, 1f, 0f);
            panel.Add(textLabel);

            elapsed = 0f;
            while (elapsed < 2.2f && !skipRequested)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / 2.2f);
                float alpha = Mathf.Clamp01(elapsed / 0.4f);
                float easeT = 1f - (1f - t) * (1f - t);
                float s = Mathf.Lerp(0.85f, 1.05f, easeT) * (1f + Mathf.Sin(elapsed * Mathf.PI * 2f) * 0.02f);
                textLabel.style.color = new Color(1f, 1f, 1f, alpha);
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
        ripple.style.borderTopColor = new Color(1f, 1f, 0.95f, 0.7f);
        ripple.style.borderBottomColor = new Color(1f, 1f, 0.95f, 0.7f);
        ripple.style.borderLeftColor = new Color(1f, 1f, 0.95f, 0.7f);
        ripple.style.borderRightColor = new Color(1f, 1f, 0.95f, 0.7f);
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
            ripple.style.borderTopColor = new Color(1f, 1f, 0.95f, alpha);
            ripple.style.borderBottomColor = new Color(1f, 1f, 0.95f, alpha);
            ripple.style.borderLeftColor = new Color(1f, 1f, 0.95f, alpha);
            ripple.style.borderRightColor = new Color(1f, 1f, 0.95f, alpha);

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
                if (fatherSprites != null && fIdx < fatherSprites.Length && fatherSprites[fIdx] != null)
                {
                    storyFatherFace.style.backgroundImage = new UIE.StyleBackground(fatherSprites[fIdx]);
                    storyFatherFace.style.backgroundColor = UIE.StyleKeyword.None;
                }
                else
                {
                    storyFatherFace.style.backgroundImage = UIE.StyleKeyword.None;
                    storyFatherFace.style.backgroundColor = father.faceColor;
                }
            }
            if (storyFatherName != null) storyFatherName.text = Localization.GetParent(father.name);
            if (storyFatherIntro != null) storyFatherIntro.text = Localization.GetParentIntro(father.name).Replace(" / ", "\n");

            // 母親カードを設定
            if (storyMotherFace != null)
            {
                if (motherSprites != null && mIdx < motherSprites.Length && motherSprites[mIdx] != null)
                {
                    storyMotherFace.style.backgroundImage = new UIE.StyleBackground(motherSprites[mIdx]);
                    storyMotherFace.style.backgroundColor = UIE.StyleKeyword.None;
                }
                else
                {
                    storyMotherFace.style.backgroundImage = UIE.StyleKeyword.None;
                    storyMotherFace.style.backgroundColor = mother.faceColor;
                }
            }
            if (storyMotherName != null) storyMotherName.text = Localization.GetParent(mother.name);
            if (storyMotherIntro != null) storyMotherIntro.text = Localization.GetParentIntro(mother.name).Replace(" / ", "\n");

            storyText.text = "";
            storyPanel.style.display = UIE.DisplayStyle.Flex;

            // スキップボタン（テキスト全表示のみ、ページ送りはしない）
            var skipBtn = CreateSkipButton(storyPanel);

            // ストーリーを1文字ずつ表示（タイプライター効果）
            foreach (char c in story)
            {
                if (skipRequested) break;
                storyText.text += c;
                yield return new WaitForSeconds(0.05f);
            }

            // スキップ時は全文を即表示
            if (skipRequested)
            {
                storyText.text = story;
                skipRequested = false;
            }

            skipBtn.RemoveFromHierarchy();

            // タップ待ち（タップするまで進まない）
            waitingForStoryConfirm = true;
            while (waitingForStoryConfirm)
            {
                yield return null;
            }
            storyPanel.style.display = UIE.DisplayStyle.None;

            // Canvas背景を復元
            if (mainBgImg != null) mainBgImg.gameObject.SetActive(true);

            // 親パネルを再表示
            if (parentPanel != null) parentPanel.style.display = UIE.DisplayStyle.Flex;
        }

        if (!skipRequested)
            yield return new WaitForSeconds(0.6f);
    }

    IEnumerator ShowFailureSequence(string locPrefix, Color lineColor)
    {
        if (parentPanel != null) parentPanel.style.display = UIE.DisplayStyle.None;
        if (childStatusText != null) childStatusText.text = "";

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
        if (generateLifeButton != null) generateLifeButton.SetActive(true);
        if (anotherGalButton != null) anotherGalButton.SetActive(false);
        if (gotoBattleButton != null) gotoBattleButton.SetActive(false);
    }

    // ===== メニューバー =====

    void CreateMenuBar()
    {
        var (safeTop, _, _, _) = UIHelper.GetSafeMargins();

        // Menu button (top-right)
        var menuBtn = new UIE.Button();
        menuBtn.AddToClassList("birth-menu-btn");
        menuBtn.style.top = 24 + safeTop;
        for (int i = 0; i < 3; i++)
        {
            var line = new UIE.VisualElement();
            line.AddToClassList("birth-menu-line");
            menuBtn.Add(line);
        }
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

    void Update()
    {
        time += Time.deltaTime;

        // グローの脈動（ゆっくり大きくなったり小さくなったり）
        if (glowTf != null && glowImg != null)
        {
            float pulse = 1f + 0.12f * Mathf.Sin(time * 2.5f);
            glowTf.localScale = new Vector3(pulse, pulse, 1f);
            float alpha = 0.3f + 0.15f * Mathf.Sin(time * 2.5f);
            glowImg.color = new Color(1f, 0.3f, 0.15f, alpha);
        }

        // 光線の回転
        if (raysTf != null)
        {
            raysTf.localRotation = Quaternion.Euler(0, 0, time * 15f);
        }

        // きらめき：回転 + サイズ脈動 + 点滅
        if (sparkleTfs != null)
        {
            for (int i = 0; i < sparkleTfs.Length; i++)
            {
                float offset = i * 1.57f; // π/2ずつずらす
                float angle = time * 1.2f + offset;
                float dist = 185f + 10f * Mathf.Sin(time * 3f + offset);
                sparkleTfs[i].GetComponent<RectTransform>().anchoredPosition =
                    new Vector2(Mathf.Cos(angle) * dist, Mathf.Sin(angle) * dist);

                float sparkleScale = 0.8f + 0.5f * Mathf.Sin(time * 5f + offset);
                sparkleTfs[i].localScale = new Vector3(sparkleScale, sparkleScale, 1f);

                if (sparkleImgs != null && i < sparkleImgs.Length)
                {
                    float sa = 0.5f + 0.5f * Mathf.Sin(time * 4f + offset);
                    sparkleImgs[i].color = new Color(1f, 1f, 0.7f, sa);
                }
            }
        }
    }
}
