using UnityEngine;
using System.Collections.Generic;

public static class Localization
{
    static string currentLanguage = null;

    static Dictionary<string, string> ja = new Dictionary<string, string>();
    static Dictionary<string, string> en = new Dictionary<string, string>();

    static Localization()
    {
        // ===== UI Common =====
        Add("ui_close", "とじる", "Close");
        Add("ui_save", "セーブ", "Save");
        Add("ui_load", "ロード", "Load");
        Add("ui_delete", "削除", "Delete");
        Add("ui_cancel", "キャンセル", "Cancel");
        Add("ui_confirm", "決定", "Confirm");
        Add("ui_yes", "はい", "Yes");
        Add("ui_no", "いいえ", "No");
        Add("ui_back_to_title", "トップへ", "Title");
        Add("ui_back_to_title_long", "タイトルへ戻る", "Back to Title");
        Add("ui_saved", "<color=#00FF00>セーブしました！</color>", "<color=#00FF00>Saved!</color>");
        Add("ui_try_again", "{0}はちからつきた", "{0} lost all strength");

        // ===== Title Scene =====
        Add("title_save_data", "セーブデータ", "Save Data");
        Add("title_save_data_list", "セーブデータ一覧", "Save Data List");
        Add("title_slot_empty", "スロット {0}: 空き", "Slot {0}: Empty");
        Add("title_slot_info", "<color={0}>{1}</color> ({2}ヶ月)\n<size=70%>父:{3} 母:{4}</size>",
            "<color={0}>{1}</color> ({2} months)\n<size=70%>Father:{3} Mother:{4}</size>");
        Add("title_confirm_delete", "スロット{0}を削除しますか？\n<size=70%>この操作は取り消せません</size>",
            "Delete Slot {0}?\n<size=70%>This cannot be undone</size>");

        // ===== Title Intro =====
        Add("intro_line1",
            "「この世界、ハズレばっかりだと思わないか？」\n",
            "\"Don't you think this world is full of duds?\"\n");
        Add("intro_line2",
            "悪魔に支配された、クソゲーみたいなこの世界。\n 逆転の鍵は、最強の遺伝子を掛け合わせた「究極の親ガチャ」にある。\n\n",
            "A world ruled by demons, like some terrible game.\n The key to turning it around lies in the ultimate 'parent gacha' — crossing the strongest genes.\n\n");
        Add("intro_line3",
            "父の力、母の知恵。\nそこに運命のダイスが振られた瞬間、\n 天をも恐れぬ**『GOD BABY』**が誕生する。\n\n",
            "A father's power, a mother's wisdom.\nWhen the dice of fate are rolled,\n a **'GOD BABY'** fearless even of heaven is born.\n\n");
        Add("intro_line4",
            "凡才で終わるか、神の嬰児となるか。\n\n 育てろ、最強の赤子を。",
            "Will you end as ordinary, or become a divine infant?\n\n Raise the ultimate baby.");

        // ===== Birth Scene =====
        Add("birth_summon_button", "いでよ、GOD BABY!!!", "Come forth, GOD BABY!!!");
        Add("birth_reroll_button", "もう一度うむ", "Reroll");
        Add("birth_name_button", "名前をつける", "Name Your Baby");
        Add("birth_gender_title", "どちらでプレイする？", "Choose your baby's gender:");
        Add("birth_male", "男の子", "Boy");
        Add("birth_female", "女の子", "Girl");
        Add("birth_name_input_title", "赤ちゃんの名前を入力してください", "Enter your baby's name:");
        Add("birth_default_name", "名無しベイビー", "Baby");
        Add("birth_save_confirm", "セーブしますか？", "Save your game?");
        Add("birth_father_prefix", "父", "Father");
        Add("birth_mother_prefix", "母", "Mother");
        Add("birth_mother_unknown", "母: ???", "Mother: ???");

        // Birth - God Baby / Promising
        Add("birth_god_line1", "<color=#FFD700><size=120%><b>天からのお恵みだ。</b></size></color>",
            "<color=#FFD700><size=120%><b>A blessing from the heavens.</b></size></color>");
        Add("birth_god_line2", "<color=#FFD700><size=150%><b>GOD BABY 爆誕！</b></size></color>",
            "<color=#FFD700><size=150%><b>GOD BABY is born!</b></size></color>");
        Add("birth_promising_line1", "<color=#FF6B6B><size=120%><b>大物になりそうな赤ちゃんだ！</b></size></color>",
            "<color=#FF6B6B><size=120%><b>This baby is destined for greatness!</b></size></color>");
        Add("birth_normal_line1", "<color=#FFFF00><size=130%><b>【 新しい命が誕生！ 】</b></size></color>",
            "<color=#FFFF00><size=130%><b>[ A new life is born! ]</b></size></color>");
        Add("birth_separator", "──────────────────────────────────────", "──────────────────────────────────────");

        // Birth - Status labels
        Add("birth_stat_gender", "<b>性別:</b>", "<b>Gender:</b>");
        Add("birth_stat_height", "<b>身長:</b>", "<b>Height:</b>");
        Add("birth_stat_weight", "<b>体重:</b>", "<b>Weight:</b>");
        Add("birth_stat_hp", "<b>HP:</b>", "<b>HP:</b>");
        Add("birth_stat_atk", "<b>攻撃:</b>", "<b>ATK:</b>");
        Add("birth_stat_def", "<b>防御:</b>", "<b>DEF:</b>");
        Add("birth_stat_academic", "<b>学力:</b>", "<b>INT:</b>");
        Add("birth_stat_athletic", "<b>運動:</b>", "<b>AGI:</b>");
        Add("birth_stat_trait", "<b>特徴:</b>", "<b>Trait:</b>");

        // Birth - Love story
        Add("birth_story_title", "<color=#FF69B4>♥</color> 二人の出会い <color=#FF69B4>♥</color>",
            "<color=#FF69B4>♥</color> How They Met <color=#FF69B4>♥</color>");
        Add("birth_story_tap", "▼ タップで続ける ▼", "▼ Tap to continue ▼");
        Add("birth_story_default", "運命の出会いから\n愛が芽生えた...", "From a fateful encounter,\nlove blossomed...");

        // Birth - Luna failure
        Add("birth_luna_line1", "「この世界、ハズレばっかりだと思わないか？」\n",
            "\"Don't you think this world is full of duds?\"\n");
        Add("birth_luna_line2", "二人は何年も待ち続けた。\nだが、コウノトリは訪れなかった。\n",
            "They waited for years.\nBut the stork never came.\n");
        Add("birth_luna_line3", "ルナは世界一美しかったが、\n神は全てを与えはしなかった...\n",
            "Luna was the most beautiful in the world,\nbut the gods did not grant everything...\n");
        Add("birth_luna_line4", "<color=#AADDFF>もう一度運命に挑戦しよう。</color>",
            "<color=#AADDFF>Let's challenge fate once more.</color>");

        // Birth - Sakura failure (暗殺拳の継承者)
        Add("birth_sakura_line1", "サクラは夜も修行を続けていた。\n暗殺拳の道に終わりはなかった。\n",
            "Sakura continued training through the night.\nThe path of the assassination fist had no end.\n");
        Add("birth_sakura_line2", "彼女の拳は誰よりも強かったが、\nその手は誰にも触れようとしなかった。\n",
            "Her fists were stronger than anyone's,\nbut those hands refused to touch anyone.\n");
        Add("birth_sakura_line3", "全戦全勝の暗殺者は、\n愛だけには勝てなかった...\n",
            "The undefeated assassin\ncouldn't win against love alone...\n");
        Add("birth_sakura_line4", "<color=#FFAACC>それでも、拳を解き、手を繋ごう。</color>",
            "<color=#FFAACC>Even so, let's unclench our fists and hold hands.</color>");

        // Birth - Misato failure
        Add("birth_misato_line1", "ミサトは研究に没頭していた。\n量子の世界だけが、彼女の居場所だった。\n",
            "Misato was absorbed in her research.\nThe quantum world was her only home.\n");
        Add("birth_misato_line2", "二人の間にはいつも論文があった。\nベッドの上にも、心の間にも。\n",
            "There were always papers between them.\nOn the bed, and between their hearts.\n");
        Add("birth_misato_line3", "IQ270の天才は宇宙の謎を解けたが、\n二人の距離だけは縮められなかった...\n",
            "The genius with an IQ of 270 could solve\nthe mysteries of the universe,\nbut couldn't close the distance between them...\n");
        Add("birth_misato_line4", "<color=#99DDFF>それでも、量子のゆらぎを信じよう。</color>",
            "<color=#99DDFF>Even so, let's believe in quantum fluctuations.</color>");

        // Birth - Hinata failure (美容帝国CEO)
        Add("birth_hinata_line1", "ヒナタは深夜もオフィスにいた。\n美容帝国の頂点に休息はなかった。\n",
            "Hinata was still in the office at midnight.\nThere was no rest at the top of a beauty empire.\n");
        Add("birth_hinata_line2", "彼女の手は誰よりも美しかったが、\nその手は契約書しか握らなかった。\n",
            "Her hands were more beautiful than anyone's,\nbut they only held contracts.\n");
        Add("birth_hinata_line3", "28兆円の女帝は全てを手に入れたが、\n二人の時間だけは買えなかった...\n",
            "The empress of a 28-trillion-yen empire had everything,\nbut couldn't buy time for the two of them...\n");
        Add("birth_hinata_line4", "<color=#CC99FF>それでも、帝国より大切なものを信じよう。</color>",
            "<color=#CC99FF>Even so, let's believe in something greater than an empire.</color>");

        // Birth - Kaede failure (天才外科医)
        Add("birth_kaede_line1", "カエデはオペ室に戻った。\n命を救うことが、彼女の全てだった。\n",
            "Kaede returned to the operating room.\nSaving lives was everything to her.\n");
        Add("birth_kaede_line2", "どれだけ愛し合っても、\n彼女のポケベルは鳴り止まなかった。\n",
            "No matter how deep their love,\nher pager never stopped ringing.\n");
        Add("birth_kaede_line3", "天才外科医は誰でも救えたが、\n自分たちの未来だけは救えなかった...\n",
            "The genius surgeon could save anyone,\nbut couldn't save their own future...\n");
        Add("birth_kaede_line4", "<color=#88FFB0>それでも、命の奇跡を信じよう。</color>",
            "<color=#88FFB0>Even so, let's believe in the miracle of life.</color>");

        // ===== Traits =====
        Add("trait_天才肌", "天才肌", "Genius");
        Add("trait_努力家", "努力家", "Hard Worker");
        Add("trait_頑丈", "頑丈", "Tough");
        Add("trait_すばしっこい", "すばしっこい", "Quick");
        Add("trait_おだやか", "おだやか", "Calm");
        Add("trait_あまえんぼう", "あまえんぼう", "Clingy");
        Add("trait_なきむし", "なきむし", "Crybaby");
        Add("trait_くいしんぼう", "くいしんぼう", "Glutton");
        Add("trait_好奇心旺盛", "好奇心旺盛", "Curious");
        Add("trait_マイペース", "マイペース", "Easy-going");
        Add("trait_負けず嫌い", "負けず嫌い", "Competitive");
        Add("trait_やさしい", "やさしい", "Kind");
        Add("trait_ワイルド", "ワイルド", "Wild");
        Add("trait_ミステリアス", "ミステリアス", "Mysterious");
        Add("trait_あばれんぼう", "あばれんぼう", "Rowdy");
        Add("trait_覇王色", "覇王色", "Conqueror's Haki");

        // ===== Parent Names =====
        Add("parent_タケシ", "タケシ", "Takeshi");
        Add("parent_ユウキ", "ユウキ", "Yuuki");
        Add("parent_ゴウ", "ゴウ", "Gou");
        Add("parent_シンジ", "シンジ", "Shinji");
        Add("parent_リョウマ", "リョウマ", "Ryouma");
        Add("parent_テツヤ", "テツヤ", "Tetsuya");
        Add("parent_サクラ", "サクラ", "Sakura");
        Add("parent_ヒナタ", "ヒナタ", "Hinata");
        Add("parent_アキラ", "アキラ", "Akira");
        Add("parent_ミサト", "ミサト", "Misato");
        Add("parent_カエデ", "カエデ", "Kaede");
        Add("parent_ルナ", "ルナ", "Luna");

        // ===== Parent Intros =====
        Add("intro_タケシ", "元・格闘技世界王者 / 握力: 180kg", "Ex-World Martial Arts Champion / Grip: 180kg");
        Add("intro_ユウキ", "天才ハッカー / 特許数: 3,200件", "Genius Hacker / Patents: 3,200");
        Add("intro_ゴウ", "伝説の傭兵 / 戦闘力: 計測不能", "Legendary Mercenary / Power: Immeasurable");
        Add("intro_シンジ", "ノーベル賞3回受賞 / IQ: 250", "3x Nobel Laureate / IQ: 250");
        Add("intro_リョウマ", "総資産: 43兆円 / 世界一の実業家", "Net Worth: $430B / World's Top Tycoon");
        Add("intro_テツヤ", "伝説のロックスター / ファン数: 8億人", "Legendary Rockstar / Fans: 800M");
        Add("intro_サクラ", "暗殺拳の継承者 / 全戦全勝", "Heir of Assassination Arts / Undefeated");
        Add("intro_ヒナタ", "総資産: 28兆円 / 美容帝国CEO", "Net Worth: 28 Trillion Yen / Beauty Empire CEO");
        Add("intro_アキラ", "五輪金メダル7個 / 100m走: 10.1秒", "7 Olympic Golds / 100m: 10.1s");
        Add("intro_ミサト", "量子物理学者 / IQ: 270", "Quantum Physicist / IQ: 270");
        Add("intro_カエデ", "天才外科医 / 手術成功率: 100%", "Genius Surgeon / Success Rate: 100%");
        Add("intro_ルナ", "世界的スーパーモデル / 身長: 180cm", "Global Supermodel / Height: 180cm");

        // ===== Love Stories =====
        // タケシ × mothers
        Add("love_タケシ_サクラ",
            "裏格闘技界の頂点を決める戦い。\nタケシとサクラは決勝で激突した。\n\n拳と暗殺拳が交錯する中、\n二人は互いの強さに惹かれていく。\n\n死闘は引き分けに終わり、\n「決着は別の形でつけよう」と\nタケシが差し出した手を、\nサクラは静かに握り返した。\n最強の血統がここに誕生する。",
            "A battle to decide the king of underground fighting.\nTakeshi and Sakura clashed in the finals.\n\nAs fists met assassination arts,\nthey were drawn to each other's strength.\n\nThe death match ended in a draw.\n\"Let's settle this another way,\"\nTakeshi extended his hand,\nand Sakura quietly took it.\nThe strongest bloodline was born.");
        Add("love_タケシ_ヒナタ",
            "「格闘家専用コスメを作りたい」\nヒナタからの突然の依頼。\n\nビジネスミーティングのはずが、\nタケシの素朴な優しさに触れ、\nヒナタの心は揺れ始める。\n\n「数字じゃ測れないものがある」\nタケシの言葉に、\n28兆円の帝国を築いた女は\n初めて涙を流した。\n愛は最高の投資だと知った日。",
            "\"I want to create fighter-only cosmetics.\"\nA sudden request from Hinata.\n\nWhat was meant to be a business meeting—\nTakeshi's simple kindness\nshook Hinata's heart.\n\n\"Some things can't be measured in numbers.\"\nAt those words, the woman who built\na $280B empire shed tears\nfor the first time.\nThe day she learned love is the best investment.");
        Add("love_タケシ_アキラ",
            "オリンピック選手村での出会い。\n格闘技代表のタケシと\n陸上代表のアキラ。\n\n食堂で偶然隣り合わせになり、\n互いの鍛え抜かれた肉体に\n目を奪われた。\n\n「一緒にトレーニングしないか？」\nその一言から始まった朝練は、\nいつしか二人だけの時間に変わり、\n閉会式の夜、二人は結ばれた。",
            "They met at the Olympic Village.\nTakeshi, martial arts representative,\nand Akira, track and field star.\n\nSeated by chance in the cafeteria,\nthey were captivated by each other's\ntrained physiques.\n\n\"Want to train together?\"\nThose morning sessions became\ntheir private time, and on the night\nof the closing ceremony, they united.");
        Add("love_タケシ_ミサト",
            "「筋肉の収縮は量子力学で\n説明できるんですよ」\n\n学会に招かれたタケシに、\nミサトは熱心に語りかけた。\n\n「難しいことはわからねえが、\nあんたの目は本気だな」\n\n理論と実践、正反対の二人。\nだが夜通し語り合ううちに、\n科学者の心は格闘家に奪われ、\n最強の頭脳と肉体が融合した。",
            "\"Muscle contraction can be explained\nby quantum mechanics, you know.\"\n\nMisato eagerly spoke to Takeshi,\ninvited to a scientific conference.\n\n\"I don't get the hard stuff,\nbut your eyes are serious.\"\n\nTheory and practice — polar opposites.\nBut after talking all night,\nthe scientist's heart was captured,\nand the strongest mind and body merged.");
        Add("love_タケシ_カエデ",
            "世界格闘技選手権の決勝戦。\nタケシは宿敵との死闘の末、\n右腕を複雑骨折した。\n\n「二度と戦えない」と宣告される中、\n唯一の希望は天才外科医カエデだった。\n\n12時間に及ぶ手術。\n目覚めたタケシの最初の言葉は\n「俺の腕を救ってくれた君を、\n俺の人生に迎えたい」だった。",
            "The World Martial Arts Championship finals.\nTakeshi suffered a compound fracture\nafter a death match with his rival.\n\nTold he'd \"never fight again,\"\nhis only hope was genius surgeon Kaede.\n\nAfter 12 hours of surgery,\nTakeshi's first words upon waking:\n\"I want you, who saved my arm,\nto be part of my life.\"");
        Add("love_タケシ_ルナ",
            "スポーツ雑誌の表紙撮影。\n格闘家とスーパーモデルの共演。\n\nカメラの前で火花が散り、\n「もっと近づいて」という\nカメラマンの指示に、\n二人の心臓が高鳴る。\n\n撮影後、ルナが言った。\n「あなたの隣にいると、\n自分が美しく見える気がする」\nスポットライトの下で恋が始まった。",
            "A sports magazine cover shoot.\nA fighter and supermodel together.\n\nSparks flew before the camera.\n\"Get closer,\" the photographer said,\nand both their hearts raced.\n\nAfter the shoot, Luna said:\n\"Standing next to you,\nI feel more beautiful.\"\nLove began under the spotlight.");

        // ユウキ × mothers
        Add("love_ユウキ_サクラ",
            "暗殺組織のサーバーに侵入した夜、\nユウキは追手に囲まれた。\n\nその中にいたのがサクラ。\n「殺すつもりはない。\nあなたの腕が必要なの」\n\n組織を裏切り、共に逃亡する日々。\n追われる中で芽生えた信頼は、\nいつしか愛に変わっていた。\n\n「俺のファイアウォールは\n君だけ通過できる」\n不器用な告白だった。",
            "The night Yuuki infiltrated an\nassassination group's server,\nhe was surrounded by pursuers.\n\nAmong them was Sakura.\n\"I won't kill you.\nI need your skills.\"\n\nBetraying the organization, fleeing together.\nTrust born while on the run\neventually became love.\n\n\"My firewall only lets you through.\"\nAn awkward confession.");
        Add("love_ユウキ_ヒナタ",
            "美容帝国のDX化プロジェクト。\n億単位の契約書を前に、\nユウキは言った。\n\n「報酬はいらない。\nその代わり、週に一度\n食事に付き合ってほしい」\n\n最初は呆れていたヒナタも、\n彼の純粋さに惹かれていく。\n\n「私に値段をつけない人は\n初めてよ」\n28兆円より価値ある愛を知った。",
            "A digital transformation project\nfor the beauty empire.\nFacing a billion-dollar contract,\nYuuki said:\n\n\"I don't need payment.\nInstead, have dinner with me\nonce a week.\"\n\nHinata, initially speechless,\nwas drawn to his sincerity.\n\n\"You're the first person\nwho didn't put a price on me.\"\nShe discovered love worth more than $280B.");
        Add("love_ユウキ_アキラ",
            "アスリート向けAIトレーナーの開発中、\nテストランナーとして\nアキラが研究所に現れた。\n\n「データが全然取れない...\n君は規格外すぎる」\n困惑するユウキに、\nアキラは笑って言った。\n\n「じゃあ毎日来てあげる」\n\nデータ収集という名目の\nデートが始まり、\n数値では測れない感情が芽生えた。",
            "While developing an AI trainer for athletes,\nAkira appeared at the lab\nas a test runner.\n\n\"I can't get any proper data...\nYou're way beyond spec.\"\nTo the puzzled Yuuki,\nAkira laughed and said:\n\n\"Then I'll come every day.\"\n\nDates disguised as data collection began,\nand feelings beyond measurement bloomed.");
        Add("love_ユウキ_ミサト",
            "量子コンピュータの共同研究。\n世界最高峰の頭脳が二つ、\n同じ研究室に集まった。\n\n夜通しのプログラミング、\nコーヒーカップが触れ合う音、\n「この暗号、解ける？」\n「君となら、どんな問題でも」\n\n二人だけの言語で愛を語り、\n論文より大切な答えを見つけた。\nそれは「共に生きる」という\nシンプルな真実だった。",
            "Joint research on quantum computers.\nTwo of the world's greatest minds\ngathered in one lab.\n\nAll-night programming,\nthe clink of coffee cups,\n\"Can you crack this cipher?\"\n\"With you, any problem.\"\n\nSpeaking love in their own language,\nthey found an answer more precious\nthan any paper: the simple truth\nof \"living together.\"");
        Add("love_ユウキ_カエデ",
            "大病院のシステムがハッキングされた。\n犯人を追うカエデの前に現れたのは、\nセキュリティ専門家のユウキだった。\n\n夜通しの作業、\nコードを書く指とメスを握る指が\n偶然触れ合った瞬間、\n二人は目を合わせた。\n\n「君の手は人を救う手だ」\n「あなたの手もよ」\n異なる世界の天才が、\n同じ未来を見つめ始めた。",
            "A major hospital's system was hacked.\nAppearing before Kaede as she tracked\nthe culprit was security expert Yuuki.\n\nWorking through the night,\nwhen coding fingers and surgical fingers\naccidentally touched,\ntheir eyes met.\n\n\"Your hands save lives.\"\n\"So do yours.\"\nTwo geniuses from different worlds\nbegan looking toward the same future.");
        Add("love_ユウキ_ルナ",
            "SNSで炎上したルナ。\n誹謗中傷の嵐の中、\n匿名の誰かが彼女を守り続けた。\n\n悪質な投稿を消し、\n真実を広め、\n見えない騎士のように戦った。\n\nある日、IPアドレスを辿ったルナは\nユウキを見つけた。\n「なぜ私のために？」\n「君の笑顔を守りたかった」\nその日、二人は恋人になった。",
            "Luna was under fire on social media.\nAmidst a storm of hate,\nsomeone anonymous kept protecting her.\n\nDeleting malicious posts,\nspreading the truth,\nfighting like an invisible knight.\n\nOne day, Luna traced the IP address\nand found Yuuki.\n\"Why did you do this for me?\"\n\"I wanted to protect your smile.\"\nThat day, they became lovers.");

        // ゴウ × mothers
        Add("love_ゴウ_サクラ",
            "暗殺任務で鉢合わせた二人。\n互いに銃口を向けながら、\n奇妙な沈黙が流れた。\n\n「お前を殺す理由がない」\n「私もよ」\n\n銃を下ろした瞬間、\n組織に追われる身となった。\n\n「一緒に逃げないか」\n「どこまでも」\n\n世界中を逃げ回る日々が、\n二人を離れられない関係にした。",
            "Two who met on an assassination mission.\nGuns pointed at each other,\nan eerie silence fell.\n\n\"I have no reason to kill you.\"\n\"Neither do I.\"\n\nThe moment they lowered their guns,\nthey became hunted by the organization.\n\n\"Run away with me?\"\n\"Anywhere.\"\n\nDays fleeing across the world\nmade them inseparable.");
        Add("love_ゴウ_ヒナタ",
            "要人警護の任務。\n標的にされたのはヒナタだった。\n\n三度の暗殺未遂、\nその全てからゴウは彼女を守った。\n三発目の銃弾を\n自らの体で受け止めた時、\nヒナタは悟った。\n\n「お金じゃ買えないものがある」\n\n病室で目覚めたゴウに、\n彼女は涙ながらに言った。\n「私の人生を守って」",
            "A VIP protection mission.\nHinata was the target.\n\nThree assassination attempts —\nGou protected her from all of them.\nWhen he took the third bullet\nwith his own body,\nHinata realized:\n\n\"There are things money can't buy.\"\n\nTo Gou, waking in the hospital,\nshe said through tears:\n\"Protect my life.\"");
        Add("love_ゴウ_アキラ",
            "紛争地帯でのスポーツ親善大使。\nアキラの警護を任されたゴウは、\n彼女の無邪気さに戸惑った。\n\n「怖くないのか？」\n「あなたがいるから」\n\n銃声の中でも笑顔を絶やさない彼女。\n守るべき存在が、\nいつしか愛する人に変わっていた。\n\n任務終了の日、\nゴウは傭兵を辞める決意をした。",
            "A sports goodwill ambassador in a conflict zone.\nGou, assigned to guard Akira,\nwas bewildered by her innocence.\n\n\"Aren't you afraid?\"\n\"You're here, so no.\"\n\nShe never stopped smiling amid gunfire.\nThe person he had to protect\nbecame the person he loved.\n\nOn the last day of the mission,\nGou decided to quit being a mercenary.");
        Add("love_ゴウ_ミサト",
            "軍事衛星のデータ解析依頼。\n冷徹な傭兵ゴウと、\n純粋な物理学者ミサト。\n\n「なぜ人を殺すの？」\n直球の質問に、\nゴウは言葉を失った。\n\n「...答えが見つからない」\n「一緒に探しましょう」\n\nミサトの純粋さが、\n凍った心を少しずつ溶かしていく。\n戦場の狼が愛を知った瞬間だった。",
            "A military satellite data analysis request.\nGou, the cold mercenary,\nand Misato, the pure physicist.\n\n\"Why do you kill people?\"\nAt her blunt question,\nGou was speechless.\n\n\"...I can't find the answer.\"\n\"Let's search together.\"\n\nMisato's purity slowly melted\nhis frozen heart.\nThe moment a battlefield wolf learned love.");
        Add("love_ゴウ_カエデ",
            "戦場で倒れた仲間を救うため、\nゴウは国境を越えて\n天才外科医を探した。\n\n「報酬はいくらでも払う」\n「お金じゃないの。\nあなたが連れてきて」\n\n危険な戦地に飛び込んだカエデ。\n命がけの手術を終えた夜、\nゴウは初めて泣いた。\n\n「俺の人生を守ってくれないか」\n傭兵の不器用なプロポーズだった。",
            "To save a fallen comrade,\nGou crossed borders seeking\na genius surgeon.\n\n\"I'll pay any price.\"\n\"It's not about money.\nBring me there.\"\n\nKaede dove into the war zone.\nAfter a life-risking surgery that night,\nGou cried for the first time.\n\n\"Will you protect my life?\"\nA mercenary's clumsy proposal.");
        Add("love_ゴウ_ルナ",
            "戦場カメラマンとして同行したルナ。\n「真実を伝えたい」という\n彼女の覚悟に、ゴウは驚いた。\n\n砲撃の夜、塹壕で肩を寄せ合い、\n生と死の狭間で\n二人は唇を重ねた。\n\n「生きて帰ろう」\n「ああ、一緒にな」\n\n戦場で誓った愛は、\nどんな平和な恋より強く、\n深く結ばれていた。",
            "Luna joined as a war photographer.\n\"I want to show the truth\" —\nher resolve surprised Gou.\n\nUnder shelling, huddled in a trench,\nbetween life and death,\nthey kissed.\n\n\"Let's make it home alive.\"\n\"Yeah, together.\"\n\nLove sworn on the battlefield\nwas stronger and deeper\nthan any peacetime romance.");

        // シンジ × mothers
        Add("love_シンジ_サクラ",
            "「暗殺拳の科学的解明」\nその研究テーマに、\nサクラは協力を申し出た。\n\n動きを解析するうちに、\nシンジの目は彼女自身に向いていた。\n\n「論文より君を研究したい」\n「それ、口説いてる？」\n「...多分」\n\n世界一不器用な告白に、\n暗殺者は初めて頬を染めた。\n愛は科学で証明できないと知った。",
            "\"Scientific Analysis of Assassination Arts\"\nSakura volunteered to help\nwith this research topic.\n\nWhile analyzing her movements,\nShinji's eyes turned to her.\n\n\"I want to study you, not the paper.\"\n\"Is that a pickup line?\"\n\"...Probably.\"\n\nAt the world's most awkward confession,\nthe assassin blushed for the first time.\nHe learned love can't be proven by science.");
        Add("love_シンジ_ヒナタ",
            "「美の方程式」を共著で出版したい。\nヒナタからの依頼に、\nシンジは興味を持った。\n\n数式とビジネス、\n異色のコラボレーション。\n\nグラフを描くうちに、\n二人の線は一点で交わった。\n\n「この交点が僕たちの未来だ」\n「ロマンチストね、意外と」\n\n28兆円の女帝が、\n数式に恋をした日だった。",
            "\"The Equation of Beauty\" — a co-authored book.\nHinata's proposal caught\nShinji's interest.\n\nMathematics and business,\nan unusual collaboration.\n\nAs they drew graphs,\ntheir two lines intersected.\n\n\"This intersection is our future.\"\n\"How romantic, unexpectedly.\"\n\nThe day a $280B empress\nfell in love with equations.");
        Add("love_シンジ_アキラ",
            "「人体の限界」を科学する研究。\n被験者として現れたアキラの\n笑顔を見た瞬間、\nシンジの心拍データは乱れた。\n\n「先生、大丈夫？」\n「い、異常値が出ている...\n僕の心臓に」\n\n「それ、恋って言うんですよ」\nアキラの言葉に、\n天才科学者は顔を真っ赤にした。\n答えは最初から出ていたのだ。",
            "Research on \"The Limits of the Human Body.\"\nThe moment Akira appeared\nas a test subject and smiled,\nShinji's heart rate data went haywire.\n\n\"Professor, are you okay?\"\n\"Th-there's an anomaly...\nin my heart.\"\n\n\"That's called love, you know.\"\nAt Akira's words,\nthe genius scientist turned bright red.\nThe answer had been there all along.");
        Add("love_シンジ_ミサト",
            "国際物理学会での激論。\n「あなたの理論は穴だらけよ」\n「君こそ基礎が甘い」\n\n壇上で火花を散らした二人は、\nなぜかホテルのバーで再会した。\n\nIQ250とIQ270。\n合わせて520の恋が始まる。\n\n「数式より美しいものを見つけた」\n「何？」\n「君だよ」\n天才にしては陳腐な台詞だった。",
            "A heated debate at an international physics conference.\n\"Your theory is full of holes.\"\n\"Your fundamentals are weak.\"\n\nThe two who clashed on stage\nsomehow met again at the hotel bar.\n\nIQ 250 and IQ 270.\nA romance with a combined IQ of 520 begins.\n\n\"I found something more beautiful than equations.\"\n\"What?\"\n\"You.\"\nA cliché line for a genius.");
        Add("love_シンジ_カエデ",
            "ノーベル医学賞授賞式。\n偶然隣り合わせになった二人は、\n授賞式そっちのけで議論を始めた。\n\n「君の論文、3箇所間違ってる」\n「あなたこそ、5箇所よ」\n\n火花を散らす天才同士。\nだがパーティーが終わる頃には、\n互いを認め合っていた。\n\n「共同研究しないか」\n「いいわ、人生のパートナーとして」\n人類最高の遺伝子が誕生した。",
            "The Nobel Prize in Medicine ceremony.\nSeated next to each other by chance,\nthey ignored the ceremony to debate.\n\n\"Your paper has 3 mistakes.\"\n\"Yours has 5.\"\n\nGeniuses sparking against each other.\nBut by the end of the party,\nthey had mutual respect.\n\n\"Want to do joint research?\"\n\"Sure — as life partners.\"\nHumanity's finest genes were born.");
        Add("love_シンジ_ルナ",
            "「完璧な顔の数学的定義」\nその研究のため、\nルナがモデルとして協力した。\n\n何百枚もの写真、\n何千ものデータポイント。\n\n「結論が出たよ」\n「どんな顔が完璧なの？」\n「君だ。君以外にない」\n\n論文には書けない結論だった。\n美しさの究極の答えは、\n愛する人の顔だと気づいた。",
            "\"The Mathematical Definition of a Perfect Face.\"\nLuna cooperated as a model\nfor this research.\n\nHundreds of photos,\nthousands of data points.\n\n\"I've reached a conclusion.\"\n\"What makes a perfect face?\"\n\"You. No one else.\"\n\nA conclusion that couldn't be written in a paper.\nThe ultimate answer to beauty\nis the face of the one you love.");

        // リョウマ × mothers
        Add("love_リョウマ_サクラ",
            "ボディガードとして雇った暗殺者。\n命を預けた相手に、\n心まで奪われるとは思わなかった。\n\n「金で動く女か」\n「いいえ、あなたを守りたいから」\n\n嘘のない瞳だった。\n\n43兆円あっても買えないもの。\nそれは信頼と愛だと、\nリョウマは初めて知った。\n「俺の傍にいてくれ、永遠に」",
            "An assassin hired as a bodyguard.\nHe never thought he'd lose his heart\nto the person he trusted with his life.\n\n\"A woman who works for money?\"\n\"No — I want to protect you.\"\n\nHer eyes held no lies.\n\nSomething $430B can't buy:\ntrust and love.\nRyouma learned this for the first time.\n\"Stay by my side, forever.\"");
        Add("love_リョウマ_ヒナタ",
            "美容帝国との合併話。\n二つの巨大企業、\n最初は敵対から始まった。\n\n「あなたには負けないわ」\n「俺もだ」\n\n激しい交渉の末、\n二人は互いを認め合った。\n\n「合併より、\n結婚しないか」\n「...それ、逆じゃない？」\n\n71兆円の帝国が誕生した。\n株式より価値ある絆と共に。",
            "Merger talks with the beauty empire.\nTwo massive corporations,\nstarting as rivals.\n\n\"I won't lose to you.\"\n\"Neither will I.\"\n\nAfter fierce negotiations,\nthey recognized each other.\n\n\"Instead of a merger,\nwhy not marry me?\"\n\"...Isn't that backwards?\"\n\nA $710B empire was born.\nAlong with bonds worth more than stocks.");
        Add("love_リョウマ_アキラ",
            "スポーツ球団買収の記者会見。\n看板選手アキラとの握手の瞬間、\n世界一の資産家は恋に落ちた。\n\n「君をチームの顔にしたい」\n「顔じゃなくて、\n私を見てほしいな」\n\n真っ直ぐな言葉が胸を打った。\n\n株価より大切なもの。\n利益より価値あるもの。\nそれは彼女の笑顔だった。",
            "A press conference for a sports team acquisition.\nThe moment he shook hands\nwith star player Akira,\nthe world's richest man fell in love.\n\n\"I want you as the team's face.\"\n\"Don't look at my face —\nlook at me.\"\n\nHer straightforward words struck his heart.\n\nMore precious than stock prices.\nMore valuable than profits.\nIt was her smile.");
        Add("love_リョウマ_ミサト",
            "研究所への100億円の投資。\nその見返りに求めたのは、\n論文でも特許でもなかった。\n\n「週に一度、\n一緒に星を見てほしい」\n\nミサトは驚きながらも頷いた。\n\n屋上で星を眺める夜が続き、\n宇宙の話から人生の話へ。\n\n「君という星を見つけた」\n物理学者は、\nその方程式を解けなかった。",
            "A $100M investment in a research lab.\nWhat he asked in return\nwasn't papers or patents.\n\n\"Once a week,\nwatch the stars with me.\"\n\nMisato nodded, surprised.\n\nNights gazing at stars from the rooftop,\ntalks shifting from the cosmos to life.\n\n\"I found my star — you.\"\nThe physicist\ncouldn't solve that equation.");
        Add("love_リョウマ_カエデ",
            "病院チェーンのM&A交渉。\nビジネスランチのはずが、\n話は医療の未来へと広がった。\n\n「君の理想を実現するには\nいくら必要だ？」\n「お金の問題じゃないの」\n\nその言葉に、リョウマは衝撃を受けた。\n43兆円の資産が無意味に思えた。\n\n「なら、僕の人生を投資させてくれ」\n契約書にない想いを込めた。",
            "Hospital chain M&A negotiations.\nWhat was supposed to be a business lunch\nexpanded into the future of medicine.\n\n\"How much do you need\nto realize your dream?\"\n\"It's not about money.\"\n\nRyouma was shocked.\nHis $430B fortune seemed meaningless.\n\n\"Then let me invest my life.\"\nA sentiment not found in any contract.");
        Add("love_リョウマ_ルナ",
            "プライベートジェットで偶然の隣席。\nパリへ向かう12時間、\n二人は語り合った。\n\n仕事のこと、夢のこと、\n誰にも言えない弱さのこと。\n\n雲の上、地上から離れた空間で、\n肩書きも資産も意味を失った。\n\n着陸した時、\n二人は恋人になっていた。\n「地上に降りても、この気持ちは変わらない」",
            "Seated by chance on a private jet.\nDuring 12 hours to Paris,\nthey talked.\n\nAbout work, dreams,\nweaknesses they could tell no one.\n\nAbove the clouds, far from earth,\ntitles and wealth lost all meaning.\n\nWhen they landed,\nthey were lovers.\n\"Even back on the ground,\nthese feelings won't change.\"");

        // テツヤ × mothers
        Add("love_テツヤ_サクラ",
            "新曲MVの殺陣シーン。\n指導者として現れたサクラの\n鋭い動きに、テツヤは見惚れた。\n\n「もっと本気で来て」\n「怪我させるぞ」\n「それくらいが丁度いい」\n\nステージで刃を交えるうちに、\n二人の距離は縮まっていった。\n\n撮影終了後の楽屋で、\n二人は激しく唇を重ねた。",
            "A sword fight scene for a new music video.\nTetsuya was mesmerized by\nSakura's sharp movements as instructor.\n\n\"Come at me for real.\"\n\"I'll hurt you.\"\n\"That's exactly what I want.\"\n\nAs they crossed blades on stage,\nthe distance between them closed.\n\nIn the dressing room after filming,\nthey shared a passionate kiss.");
        Add("love_テツヤ_ヒナタ",
            "化粧品CMソングの打ち合わせ。\n譜面を見るふりをして、\nテツヤはヒナタを見つめていた。\n\n「曲より私を見てない？」\n「バレた？」\n「わかりやすいのよ、あなた」\n\nスタジオに響く笑い声。\nその日、二人は朝まで語り合った。\n\n「君のための歌を書きたい」\n「それ、プロポーズ？」\n「かもしれない」",
            "A meeting for a cosmetics CM song.\nPretending to read the score,\nTetsuya was staring at Hinata.\n\n\"You're looking at me, not the music.\"\n\"Caught me?\"\n\"You're obvious.\"\n\nLaughter echoed through the studio.\nThat day, they talked until dawn.\n\n\"I want to write a song for you.\"\n\"Is that a proposal?\"\n\"Maybe.\"");
        Add("love_テツヤ_アキラ",
            "オリンピック応援ソングの依頼。\n「勝利の歌を書いてほしい」\n\nアキラの走る姿を見て、\nテツヤのペンが走り出した。\n\nスタジアムに響く歌声、\n金メダルを取った瞬間、\nアキラはテツヤのもとへ走った。\n\n「この歌があったから勝てた」\n「君がいたから書けた」\n\n金メダルより輝く愛が生まれた。",
            "A request for an Olympic cheer song.\n\"Write me a victory anthem.\"\n\nWatching Akira run,\nTetsuya's pen flew across the page.\n\nAs his voice echoed in the stadium,\nthe moment she won gold,\nAkira ran to Tetsuya.\n\n\"I won because of your song.\"\n\"I wrote it because of you.\"\n\nA love brighter than gold was born.");
        Add("love_テツヤ_ミサト",
            "「音楽と物理学の共通点」\n雑誌のインタビューで出会った二人。\n\n「音は波でしょ？\n愛も波かもしれない」\n「周波数が合えば共鳴する...」\n「そう、今の僕たちみたいに」\n\n理屈っぽい会話が心地よかった。\n\nインタビューは終わっても、\n二人の会話は終わらなかった。\n共鳴した心は離れられない。",
            "\"The Common Ground of Music and Physics.\"\nTwo who met in a magazine interview.\n\n\"Sound is a wave, right?\nMaybe love is a wave too.\"\n\"If frequencies match, they resonate...\"\n\"Like us, right now.\"\n\nTheir intellectual banter felt natural.\n\nThe interview ended,\nbut their conversation didn't.\nResonating hearts can't be parted.");
        Add("love_テツヤ_カエデ",
            "ライブ中に声が出なくなった。\n「二度と歌えない」\n絶望するテツヤを救ったのは、\n天才外科医カエデだった。\n\n奇跡の手術から3ヶ月、\n声を取り戻した日、\nテツヤは病院でゲリラライブを開いた。\n\n「最初の歌は君に捧げる」\nそれは愛の歌だった。\nカエデは涙を流しながら聴いていた。",
            "He lost his voice during a concert.\n\"You'll never sing again.\"\nThe one who saved Tetsuya from despair\nwas genius surgeon Kaede.\n\n3 months after a miraculous surgery,\non the day his voice returned,\nTetsuya held a guerrilla concert at the hospital.\n\n\"This first song is for you.\"\nIt was a love song.\nKaede listened through her tears.");
        Add("love_テツヤ_ルナ",
            "ワールドツアー、50都市。\n同じ夢を追う二人は、\n世界中を一緒に回った。\n\n「疲れないか？」\n「あなたがいるから平気」\n\nステージの上と、ランウェイの上。\n輝く場所は違っても、\n見つめ合う瞳は同じだった。\n\n最後の公演、アンコール。\nテツヤはステージ上で跪いた。\n「結婚してくれ」\n8億人のファンが証人となった。",
            "World tour, 50 cities.\nTwo chasing the same dream\ntraveled the world together.\n\n\"Aren't you tired?\"\n\"With you here, I'm fine.\"\n\nOn stage and on the runway.\nDifferent places to shine,\nbut the same eyes gazing at each other.\n\nThe final show, encore.\nTetsuya knelt on stage.\n\"Marry me.\"\n800 million fans were witnesses.");

        // ===== Battle Scene =====
        // Enemy names
        Add("enemy_わるいベイビー", "わるいベイビー", "Evil Baby");
        Add("enemy_村の王シバ", "村の王シバ", "King Shiba of the Village");
        Add("enemy_やんちゃベイビー", "やんちゃベイビー", "Naughty Baby");
        Add("enemy_いじわるベイビー", "いじわるベイビー", "Mean Baby");
        Add("enemy_なきむしベイビー", "なきむしベイビー", "Crybaby Baby");
        Add("enemy_あばれんぼうベイビー", "あばれんぼうベイビー", "Rowdy Baby");
        Add("enemy_わがままベイビー", "わがままベイビー", "Spoiled Baby");

        // Battle UI labels
        Add("battle_normal_attack", "通常攻撃", "Attack");
        Add("battle_special_attack", "特殊攻撃", "Sp. Attack");
        Add("battle_defend", "防御", "Defend");
        Add("battle_special_skill", "必殺技", "Ultimate");
        Add("battle_defend_name", "ぼうぎょ", "Guard");
        Add("battle_defend_desc", "防御態勢をとり、受けるダメージを半減する", "Take a defensive stance and halve incoming damage");
        Add("battle_default_attack", "パンチ", "Punch");
        Add("battle_default_special", "GOD SMASH", "GOD SMASH");
        Add("battle_default_attack_desc", "基本的なパンチ攻撃", "A basic punch attack");
        Add("battle_default_special_desc", "全力の必殺技で大ダメージを与える", "An all-out ultimate that deals massive damage");
        Add("battle_default_mother_attack", "体当たり", "Tackle");
        Add("battle_default_mother_desc", "全身でぶつかる基本攻撃", "A basic full-body charge attack");

        // Battle messages
        Add("battle_enemy_appeared", "<color=#FF0000>{0}</color> があらわれた！",
            "<color=#FF0000>{0}</color> appeared!");
        Add("battle_boy_power", "<color=#66ccff>おとこのこパワー！</color>\nこうげきりょく UP！ (ATK:{0})",
            "<color=#66ccff>Boy Power!</color>\nAttack UP! (ATK:{0})");
        Add("battle_girl_power", "<color=#ff99cc>ちいさくて すばしっこい！</color>\nかいひりょく {0}%！",
            "<color=#ff99cc>Small and agile!</color>\nEvasion {0}%!");
        Add("battle_start", "バトル スタート！", "Battle Start!");
        Add("battle_player_first", "<color=#00FFFF>すばやさで まさった！ せんこうだ！</color>",
            "<color=#00FFFF>You're faster! First strike!</color>");
        Add("battle_enemy_first", "<color=#FF8800>{0} のほうが すばやい！</color>",
            "<color=#FF8800>{0} is faster!</color>");
        Add("battle_your_turn", "あなたのターン！ 行動を選んでください",
            "Your turn! Choose an action");
        Add("battle_enemy_attack", "{0} のこうげき！", "{0} attacks!");
        Add("battle_evaded", "<color=#00FFFF>ひらりとかわした！</color>",
            "<color=#00FFFF>Dodged it!</color>");
        Add("battle_defended", "ぼうぎょした！ {0} ダメージ！", "Guarded! {0} damage!");
        Add("battle_took_damage", "{0} ダメージをうけた！", "Took {0} damage!");
        Add("battle_punch", "パンチ！ <color=#FFA500>{0}</color>！", "Punch! <color=#FFA500>{0}</color>!");
        Add("battle_attack_hit", "<color=#FFA500>{0}</color> が きまった！\n{1} に {2} ダメージ！",
            "<color=#FFA500>{0}</color> landed!\n{2} damage to {1}!");
        Add("battle_defend_stance", "ぼうぎょ体勢をとった！", "Took a defensive stance!");
        Add("battle_mother_skill", "<color=#55DDAA>{0}</color>！", "<color=#55DDAA>{0}</color>!");
        Add("battle_mother_damage", "<color=#55DDAA>{0}</color> で {1} ダメージ！",
            "<color=#55DDAA>{0}</color> dealt {1} damage!");
        Add("battle_heal", "<color=#00FF00>HPが {0} かいふくした！</color>",
            "<color=#00FF00>HP recovered by {0}!</color>");
        Add("battle_poison", "<color=#AA00FF>{0} は どくに おかされた！</color>",
            "<color=#AA00FF>{0} was poisoned!</color>");
        Add("battle_evasion_up", "<color=#00FFFF>かいひりょくが アップした！</color>",
            "<color=#00FFFF>Evasion UP!</color>");
        Add("battle_def_down", "<color=#FFAA00>{0} の ぼうぎょが ダウン！</color>",
            "<color=#FFAA00>{0}'s defense DOWN!</color>");
        Add("battle_atk_down", "<color=#FFAA00>{0} の こうげきが ダウン！</color>",
            "<color=#FFAA00>{0}'s attack DOWN!</color>");
        Add("battle_drain", "<color=#00FF00>HPを {0} きゅうしゅうした！</color>",
            "<color=#00FF00>Absorbed {0} HP!</color>");
        Add("battle_poison_damage", "<color=#AA00FF>{0} は どくで {1} ダメージ！</color>",
            "<color=#AA00FF>{0} took {1} poison damage!</color>");
        Add("battle_god_special", "<color=#FFD700>GOD BABY の ひっさつわざ！</color>\n<color=#FFD700>神・{0}！</color>",
            "<color=#FFD700>GOD BABY's Ultimate!</color>\n<color=#FFD700>Divine {0}!</color>");
        Add("battle_special", "ひっさつわざ！\n<color=#FFFF00>{0}！</color>",
            "Ultimate Move!\n<color=#FFFF00>{0}!</color>");
        Add("battle_god_special_hit", "<color=#FFD700>神・{0}</color> が さくれつ！\n{1} ダメージ！",
            "<color=#FFD700>Divine {0}</color> landed!\n{1} damage!");
        Add("battle_special_hit", "<color=#FFFF00>{0}</color> が さくれつ！\n{1} ダメージ！",
            "<color=#FFFF00>{0}</color> landed!\n{1} damage!");
        Add("battle_special_miss", "{0}...\nしかし はずれてしまった...",
            "{0}...\nBut it missed...");
        Add("battle_fighting_spirit", "<color=#FFD700>とうしが みなぎる！ こうげきりょく UP！</color>",
            "<color=#FFD700>Fighting spirit surges! ATK UP!</color>");
        Add("battle_conqueror_revive", "覇王色の覚醒！ たおれかけたが ふっかつした！",
            "Conqueror's Haki awakens! Nearly fell, but revived!");
        Add("battle_enemy_defeated", "<color=#FFFF00>{0} をたおした！</color>",
            "<color=#FFFF00>{0} defeated!</color>");
        Add("battle_victory", "<color=#00FF00><size=130%>しょうり！</size></color>",
            "<color=#00FF00><size=130%>Victory!</size></color>");
        Add("battle_defeat", "<color=#FF0000>たおれてしまった...</color>",
            "<color=#FF0000>You were defeated...</color>");
        Add("battle_game_over", "<color=#FF0000>GAME OVER</color>",
            "<color=#FF0000>GAME OVER</color>");
        Add("battle_exp_gained", "<color=#00FFFF>けいけんち {0} をかくとく！</color>",
            "<color=#00FFFF>Gained {0} EXP!</color>");
        Add("battle_age_up", "<color=#FFD700><size=150%>\U0001f382 {0}ヶ月になった！ \U0001f382</size></color>",
            "<color=#FFD700><size=150%>\U0001f382 Now {0} months old! \U0001f382</size></color>");
        Add("battle_growth_title", "<color=#FFD700>\u2728 せいちょう！ \u2728</color>",
            "<color=#FFD700>\u2728 Growth! \u2728</color>");
        Add("battle_stat_atk", "こうげき", "ATK");
        Add("battle_stat_def", "ぼうぎょ", "DEF");
        Add("battle_stat_hp_label", "HP", "HP");
        Add("battle_stat_athletic", "うんどう", "AGI");
        Add("battle_hp_full_heal", "<color=#00FF00>HPがぜんかいふく！</color>",
            "<color=#00FF00>HP fully restored!</color>");
        Add("battle_exp_remaining", "つぎのせいちょうまで あと <color=#FFFF00>{0}</color> けいけんち\n({1}/{2})",
            "Next growth in <color=#FFFF00>{0}</color> EXP\n({1}/{2})");
        Add("battle_months", "ヶ月", " months");

        // Boss defeat
        Add("boss_defeat_line1", "村の王シバ が たおれた...\n", "King Shiba has fallen...\n");
        Add("boss_defeat_line2", "「これは 試練に すぎなかった。」\n",
            "\"That was merely a trial.\"\n");
        Add("boss_defeat_line3", "村の外には さらに強大な 敵が待っている。\nおまえの ちからは まだ 足りない。\n",
            "Beyond the village, even mightier foes await.\nYour power is not yet enough.\n");
        Add("boss_defeat_line4", "成長し、すべての 敵を 打ち砕け。\n世界は おまえを 待っている。\n",
            "Grow stronger, and crush all your enemies.\nThe world is waiting for you.\n");

        // Victory screen
        Add("battle_saved_return", "<color=#FFD700>{0}({1}ヶ月)</color>\n<color=#00FF00>セーブしました！</color>",
            "<color=#FFD700>{0}({1} months)</color>\n<color=#00FF00>Saved!</color>");
        Add("battle_returning", "むらに もどります...", "Returning to the village...");

        // Save in battle
        Add("battle_save_button", "セーブ", "Save");
        Add("battle_top_button", "トップへ", "Title");

        // ===== Map Scene =====
        Add("map_boss_sign", "<color=#FF4444>ボスのやかた</color>", "<color=#FF4444>Boss Mansion</color>");
        Add("map_help_text", "矢印キー / WASD: 移動　　スペース: 調べる　　ESC: メニュー",
            "Arrow Keys / WASD: Move    Space: Interact    ESC: Menu");
        Add("map_encounter", "<color=#FF0000>てきが あらわれた！</color>",
            "<color=#FF0000>An enemy appeared!</color>");
        Add("map_boss_enter", "<color=#FF2222><size=130%>ボスのやかた に はいった！</size></color>\n\n<size=80%>つよい てきの けはいがする...</size>",
            "<color=#FF2222><size=130%>Entered the Boss Mansion!</size></color>\n\n<size=80%>A powerful presence lurks...</size>");

        // Map interact messages
        Add("map_flower", "きれいな はなが さいている。", "Beautiful flowers are blooming.");
        Add("map_path", "むらの みちだ。", "A village road.");
        Add("map_dark_dirt", "あやしい きはいが する...", "Something ominous lurks...");
        Add("map_water", "きれいな いけだ。さかなが おいでる。", "A clear pond. Fish are swimming.");
        Add("map_house", "だれかの おうちだ。", "Someone's house.");
        Add("map_rock", "おおきな いわだ。どかせない...", "A big rock. Can't move it...");
        Add("map_nothing", "...", "...");
        Add("map_golden_egg", "<color=#FFD700>★ 金のたまご を てにいれた！★</color>",
            "<color=#FFD700>★ Got a Golden Egg! ★</color>");

        // Map menu
        Add("map_menu_status", "ステータス", "Status");
        Add("map_menu_inventory", "もちもの", "Inventory");
        Add("map_menu_save", "セーブ", "Save");
        Add("map_menu_title", "タイトルへ戻る", "Back to Title");
        Add("map_menu_close", "とじる", "Close");

        // Map status panel
        Add("map_status_title", "ステータス", "Status");
        Add("map_status_months", "ヶ月", " months");
        Add("map_status_hp", "<b>HP:</b>", "<b>HP:</b>");
        Add("map_status_atk", "<b>攻撃:</b>", "<b>ATK:</b>");
        Add("map_status_def", "<b>防御:</b>", "<b>DEF:</b>");
        Add("map_status_academic", "<b>学力:</b>", "<b>INT:</b>");
        Add("map_status_athletic", "<b>運動:</b>", "<b>AGI:</b>");
        Add("map_status_height", "<b>身長:</b>", "<b>Height:</b>");
        Add("map_status_weight", "<b>体重:</b>", "<b>Weight:</b>");
        Add("map_status_trait", "<b>特徴:</b>", "<b>Trait:</b>");
        Add("map_status_exp", "<b>経験値:</b>", "<b>EXP:</b>");
        Add("map_status_enemies", "<b>倒した敵:</b>", "<b>Defeated:</b>");
        Add("map_status_father", "<b>父:</b>", "<b>Father:</b>");
        Add("map_status_mother", "<b>母:</b>", "<b>Mother:</b>");

        // Map inventory panel
        Add("map_inventory_title", "もちもの", "Inventory");
        Add("map_inventory_empty", "なにも もっていない", "No items");
        Add("map_item_golden_egg", "金のたまご", "Golden Egg");

        // Map save panel
        Add("map_save_title", "どのスロットにセーブする？", "Choose a save slot:");
        Add("map_save_slot", "スロット{0}: ", "Slot {0}: ");
        Add("map_save_slot_empty", "<color=#666666>空き</color>", "<color=#666666>Empty</color>");
        Add("map_save_overwrite_msg", "スロット{0}には\n<b>{1}</b> のデータがあります\n\n<color=#FF6666>上書きしますか？</color>",
            "Slot {0} contains\n<b>{1}</b>'s data\n\n<color=#FF6666>Overwrite?</color>");
        Add("map_save_overwrite", "上書きする", "Overwrite");
        Add("map_save_cancel", "やめる", "Cancel");

        // Mother skill data
        Add("mskill_name_サクラ", "ヒーリングストライク", "Healing Strike");
        Add("mskill_desc_サクラ", "攻撃しつつ自分のHPを回復する医療の技", "Attack while healing your HP with medical arts");
        Add("mskill_name_ヒナタ", "毒霧", "Poison Mist");
        Add("mskill_desc_ヒナタ", "敵に毒を浴びせ、3ターンの間じわじわダメージを与える", "Spray poison on the enemy, dealing damage over 3 turns");
        Add("mskill_name_アキラ", "疾風ステップ", "Gale Step");
        Add("mskill_desc_アキラ", "素早い動きで攻撃し、2ターンの間回避率が上がる", "Quick attack that boosts evasion for 2 turns");
        Add("mskill_name_ミサト", "分析波動", "Analysis Wave");
        Add("mskill_desc_ミサト", "敵の弱点を解析し、2ターンの間敵の防御を下げる", "Analyze enemy weakness, lowering DEF for 2 turns");
        Add("mskill_name_カエデ", "威圧のオーラ", "Intimidation Aura");
        Add("mskill_desc_カエデ", "圧倒的な威圧感で、2ターンの間敵の攻撃力を下げる", "Overwhelming aura that lowers enemy ATK for 2 turns");
        Add("mskill_name_ルナ", "スターダスト", "Stardust");
        Add("mskill_desc_ルナ", "星屑をまとった攻撃。与ダメージの一部をHPとして吸収する", "An attack wrapped in stardust. Absorb part of damage dealt as HP");

        // ===== Labels for character row =====
        Add("label_fathers", "父親", "Fathers");
        Add("label_mothers", "母親", "Mothers");
    }

    static void Add(string key, string jaValue, string enValue)
    {
        ja[key] = jaValue;
        en[key] = enValue;
    }

    public static string CurrentLanguage
    {
        get
        {
            if (currentLanguage == null)
                currentLanguage = PlayerPrefs.GetString("language", "");
            return currentLanguage;
        }
    }

    public static bool HasLanguageSet()
    {
        return !string.IsNullOrEmpty(CurrentLanguage);
    }

    public static void SetLanguage(string lang)
    {
        currentLanguage = lang;
        PlayerPrefs.SetString("language", lang);
        PlayerPrefs.Save();
    }

    public static string Get(string key)
    {
        var dict = CurrentLanguage == "en" ? en : ja;
        if (dict.TryGetValue(key, out string value))
            return value;
        // Fallback to Japanese
        if (ja.TryGetValue(key, out string jaValue))
            return jaValue;
        return key;
    }

    public static string Get(string key, params object[] args)
    {
        string template = Get(key);
        return string.Format(template, args);
    }

    /// <summary>
    /// Get localized trait name from Japanese trait key
    /// </summary>
    public static string GetTrait(string jaTrait)
    {
        return Get("trait_" + jaTrait);
    }

    /// <summary>
    /// Get localized parent name from Japanese parent name
    /// </summary>
    public static string GetParent(string jaName)
    {
        return Get("parent_" + jaName);
    }

    /// <summary>
    /// Get localized parent intro from Japanese parent name
    /// </summary>
    public static string GetParentIntro(string jaName)
    {
        return Get("intro_" + jaName);
    }

    /// <summary>
    /// Get localized love story from Japanese parent names
    /// </summary>
    public static string GetLoveStory(string jaFather, string jaMother)
    {
        string key = $"love_{jaFather}_{jaMother}";
        var dict = CurrentLanguage == "en" ? en : ja;
        if (dict.TryGetValue(key, out string value))
            return value;
        return Get("birth_story_default");
    }

    /// <summary>
    /// Get localized enemy name from Japanese enemy name
    /// </summary>
    public static string GetEnemy(string jaName)
    {
        string key = "enemy_" + jaName;
        var dict = CurrentLanguage == "en" ? en : ja;
        if (dict.TryGetValue(key, out string value))
            return value;
        return jaName;
    }

    /// <summary>
    /// Get localized mother skill name
    /// </summary>
    public static string GetMotherSkillName(string jaMotherName)
    {
        return Get("mskill_name_" + jaMotherName);
    }

    /// <summary>
    /// Get localized mother skill description
    /// </summary>
    public static string GetMotherSkillDesc(string jaMotherName)
    {
        return Get("mskill_desc_" + jaMotherName);
    }

    /// <summary>
    /// Get localized gender name
    /// </summary>
    public static string GetGender(string jaGender)
    {
        if (jaGender == "男の子") return Get("birth_male");
        if (jaGender == "女の子") return Get("birth_female");
        return jaGender;
    }

    /// <summary>
    /// Get age display string
    /// </summary>
    public static string GetAge(int age)
    {
        if (CurrentLanguage == "en")
            return $"{age} months";
        return $"{age}ヶ月";
    }
}
