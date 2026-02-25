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
        Add("ui_saved", "セーブしました！", "Saved!");
        Add("ui_try_again", "{0}は強制帰宅。", "{0} was sent home.");

        // ===== Title Scene =====
        Add("title_save_data", "つづきから", "Continue");
        Add("title_new_game", "はじめから", "New Game");
        Add("title_tap_start", "タップでスタート", "Tap to Start");
        Add("title_reset_profile", "ユーザー名リセット", "Reset Profile");
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
            "ちょっぴり つまらない この世界。\n 逆転の鍵は、最強の遺伝子を掛け合わせた「究極の親ガチャ」にある。\n\n",
            "A world ruled by demons, like some terrible game.\n The key to turning it around lies in the ultimate 'parent gacha' — crossing the strongest genes.\n\n");
        Add("intro_line3",
            "父の力、母の知恵。\nそこに運命のダイスが振られた瞬間、\n みんなを えがおにする**『STAR BABY』**が誕生する。\n\n",
            "A father's power, a mother's wisdom.\nWhen the dice of fate are rolled,\n a **'STAR BABY'** fearless even of heaven is born.\n\n");
        Add("intro_line4",
            "凡才で終わるか、星の嬰児となるか。\n\n 育てろ、最強の赤子を。",
            "Will you end as ordinary, or become a star infant?\n\n Raise the ultimate baby.");

        // ===== Birth Scene =====
        Add("birth_summon_button", "いでよ、STAR BABY!!!", "Come forth, STAR BABY!!!");
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
        Add("birth_father_gacha", "父親", "Father");
        Add("birth_mother_gacha", "母親", "Mother");
        Add("birth_next", "次へ", "Next");

        // Birth - Star Baby / Promising
        Add("birth_god_line1", "<color=#FFD700><size=120%><b>星からのお恵みだ。</b></size></color>",
            "<color=#FFD700><size=120%><b>A blessing from the stars.</b></size></color>");
        Add("birth_god_line2", "<color=#FFD700><size=150%><b>STAR BABY 爆誕！</b></size></color>",
            "<color=#FFD700><size=150%><b>STAR BABY is born!</b></size></color>");
        Add("birth_promising_line1", "<color=#FF6B6B><size=120%><b>大物になりそうな赤ちゃんだ！</b></size></color>",
            "<color=#FF6B6B><size=120%><b>This baby is destined for greatness!</b></size></color>");
        Add("birth_normal_line1", "<color=#FFFF00><size=130%><b>【 新しい命が誕生！ 】</b></size></color>",
            "<color=#FFFF00><size=130%><b>[ A new life is born! ]</b></size></color>");
        Add("birth_separator", "──────────────────────────────────────", "──────────────────────────────────────");

        // Birth - Status labels
        Add("birth_stat_gender", "<b>性別:</b>", "<b>Gender:</b>");
        Add("birth_stat_hp", "<b>ごきげん度:</b>", "<b>Mood:</b>");
        Add("birth_stat_atk", "<b>ぬくもり:</b>", "<b>Warmth:</b>");
        Add("birth_stat_def", "<b>おちつき:</b>", "<b>Calm:</b>");
        Add("birth_stat_intelligence", "<b>ちえ:</b>", "<b>Wisdom:</b>");
        Add("birth_stat_athletic", "<b>運動:</b>", "<b>AGI:</b>");
        Add("birth_stat_luck", "<b>運勢:</b>", "<b>LUK:</b>");
        Add("birth_stat_fortune", "<b>資産:</b>", "<b>FTN:</b>");
        Add("birth_stat_trait", "<b>特徴:</b>", "<b>Trait:</b>");

        // Birth - Love story
        Add("birth_story_title", "<color=#FF69B4>♥</color> 邂逅（かいこう）の記憶 <color=#FF69B4>♥</color>",
            "<color=#FF69B4>♥</color> Memories of Encounter <color=#FF69B4>♥</color>");
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

        // Birth - Sakura failure (ぶじゅつの達人)
        Add("birth_sakura_line1", "サクラは夜も修行を続けていた。\nぶじゅつの道に終わりはなかった。\n",
            "Sakura continued training through the night.\nThe path of martial arts had no end.\n");
        Add("birth_sakura_line2", "彼女の拳は誰よりも強かったが、\nその手は誰にも触れようとしなかった。\n",
            "Her fists were stronger than anyone's,\nbut those hands refused to touch anyone.\n");
        Add("birth_sakura_line3", "ぜんせんむてきの つわものは、\n愛だけには勝てなかった...\n",
            "The undefeated warrior\ncouldn't win against love alone...\n");
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
        Add("parent_ゼニガタ", "ゼニガタ", "Zenigata");
        Add("parent_ツクモ", "ツクモ", "Tsukumo");
        Add("parent_サトウ", "サトウ", "Satou");
        Add("parent_イワオ", "イワオ", "Iwao");
        Add("parent_アキトシ", "アキトシ", "Akitoshi");
        Add("parent_ネオ", "ネオ", "Neo");
        Add("parent_イザナミ", "イザナミ", "Izanami");
        Add("parent_ミク", "ミク", "Miku");
        Add("parent_カヨコ", "カヨコ", "Kayoko");
        Add("parent_フクトク", "フクトク", "Fukutoku");
        Add("parent_ヨネ", "ヨネ", "Yone");
        Add("parent_ドクコ", "ドクコ", "Dokuko");

        // ===== Phase Cut-in =====
        Add("cutin_who_father", "呼び声に応えるのは――", "Answering the call――");
        Add("birth_find_mother", "宿命を定める", "Determine Destiny");
        Add("birth_nurture_love", "愛を育む", "Nurture Love");
        Add("cutin_who_mother", "惹かれ合う、もう一つの魂", "Another soul, drawn together");
        Add("cutin_baby_born", "子宝に恵まれた！", "Blessed with a child!");
        Add("cutin_birth_wish", "その願いは、新たな命へ――", "That wish becomes a new life――");
        Add("cutin_fate_moment", "運命が重なる刻（とき）", "The Moment Fates Align");
        Add("birth_love_begin", "二人の物語を紡ぐ", "Weave Their Story");

        // ===== Father Cut-in =====
        Add("cutin_タケシ", "元・格闘技世界王者のタケシだ！！", "It's Takeshi, the ex-World Champion!!");
        Add("cutin_ユウキ", "天才ハッカーのユウキだ！！", "It's Yuuki, the genius hacker!!");
        Add("cutin_ゴウ", "つよい ぼうけんかの ゴウだ！！", "It's Gou, the legendary adventurer!!");
        Add("cutin_シンジ", "ノーベル賞受賞者のシンジだ！！", "It's Shinji, the Nobel laureate!!");
        Add("cutin_リョウマ", "世界一の実業家のリョウマだ！！", "It's Ryouma, the world's top tycoon!!");
        Add("cutin_テツヤ", "伝説のロックスターのテツヤだ！！", "It's Tetsuya, the legendary rockstar!!");
        Add("cutin_ゼニガタ", "石油王のゼニガタだ！！", "It's Zenigata, the oil king!!");
        Add("cutin_ツクモ", "自称・予言者のツクモだ！！", "It's Tsukumo, the self-proclaimed prophet!!");
        Add("cutin_サトウ", "中堅企業の係長のサトウだ！！", "It's Satou, the section chief!!");
        Add("cutin_イワオ", "元・土木作業員のイワオだ！！", "It's Iwao, the ex-construction worker!!");
        Add("cutin_アキトシ", "プロギャンブラーのアキトシだ！！", "It's Akitoshi, the pro gambler!!");
        Add("cutin_ネオ", "永遠のニートのネオだ！！", "It's Neo, the eternal NEET!!");

        // ===== Mother Cut-in =====
        Add("cutin_サクラ", "つよーい おかあさんの サクラだ！！", "It's Sakura, the super-strong mama!!");
        Add("cutin_ヒナタ", "美容帝国CEOのヒナタだ！！", "It's Hinata, the beauty empire CEO!!");
        Add("cutin_アキラ", "五輪金メダリストのアキラだ！！", "It's Akira, the Olympic gold medalist!!");
        Add("cutin_ミサト", "量子物理学者のミサトだ！！", "It's Misato, the quantum physicist!!");
        Add("cutin_カエデ", "天才外科医のカエデだ！！", "It's Kaede, the genius surgeon!!");
        Add("cutin_ルナ", "世界的スーパーモデルのルナだ！！", "It's Luna, the global supermodel!!");
        Add("cutin_イザナミ", "伝説の女帝のイザナミだ！！", "It's Izanami, the legendary empress!!");
        Add("cutin_ミク", "自称・モデルのミクだ！！", "It's Miku, the self-proclaimed model!!");
        Add("cutin_カヨコ", "商店街の看板娘のカヨコだ！！", "It's Kayoko, the shopping street darling!!");
        Add("cutin_フクトク", "宝くじ1等当選者のフクトクだ！！", "It's Fukutoku, the lottery jackpot winner!!");
        Add("cutin_ヨネ", "内職の鬼のヨネだ！！", "It's Yone, the piecework master!!");
        Add("cutin_ドクコ", "闇金の取り立て屋のドクコだ！！", "It's Dokuko, the loan shark enforcer!!");

        // ===== Parent Intros =====
        Add("intro_タケシ", "元・格闘技世界王者 / 握力: 180kg", "Ex-World Martial Arts Champion / Grip: 180kg");
        Add("intro_ユウキ", "天才ハッカー / 特許数: 3,200件", "Genius Hacker / Patents: 3,200");
        Add("intro_ゴウ", "つよい ぼうけんか / ぼうけんりょく: 計測不能", "Legendary Adventurer / Adventure Power: Immeasurable");
        Add("intro_シンジ", "ノーベル賞3回受賞 / IQ: 250", "3x Nobel Laureate / IQ: 250");
        Add("intro_リョウマ", "総資産: 43兆円 / 世界一の実業家", "Net Worth: $430B / World's Top Tycoon");
        Add("intro_テツヤ", "伝説のロックスター / ファン数: 8億人", "Legendary Rockstar / Fans: 800M");
        Add("intro_サクラ", "つよーい おかあさん / ぜんせん むてき", "Super-Strong Mama / Undefeated");
        Add("intro_ヒナタ", "総資産: 28兆円 / 美容帝国CEO", "Net Worth: 28 Trillion Yen / Beauty Empire CEO");
        Add("intro_アキラ", "五輪金メダル7個 / 100m走: 10.1秒", "7 Olympic Golds / 100m: 10.1s");
        Add("intro_ミサト", "量子物理学者 / IQ: 270", "Quantum Physicist / IQ: 270");
        Add("intro_カエデ", "天才外科医 / 手術成功率: 100%", "Genius Surgeon / Success Rate: 100%");
        Add("intro_ルナ", "世界的スーパーモデル / 身長: 180cm", "Global Supermodel / Height: 180cm");
        Add("intro_ゼニガタ", "石油王 / 口癖は『金で買えないものはない』", "Oil King / Motto: 'Money buys everything'");
        Add("intro_ツクモ", "自称・予言者 / IQ300、常に宇宙と交信中", "Self-Proclaimed Prophet / IQ 300, always in cosmic contact");
        Add("intro_サトウ", "中堅企業の係長 / 趣味は洗車", "Section Chief / Hobby: car washing");
        Add("intro_イワオ", "元・土木作業員 / 素手で岩を砕く", "Ex-Construction Worker / Crushes rocks bare-handed");
        Add("intro_アキトシ", "プロギャンブラー / 通帳残高は常に0", "Pro Gambler / Bank balance: always 0");
        Add("intro_ネオ", "永遠のニート / 30年間実家から出ていない", "Eternal NEET / Hasn't left home in 30 years");
        Add("intro_イザナミ", "伝説の女帝 / 一言で国家予算が動く", "Legendary Empress / One word moves national budgets");
        Add("intro_ミク", "自称・モデル / フォロワー多数、内情は火の車", "Self-Proclaimed Model / Many followers, finances in flames");
        Add("intro_カヨコ", "商店街の看板娘 / 街の人気者", "Shopping Street Darling / Everyone's favorite");
        Add("intro_フクトク", "宝くじ1等当選者 / 才能は皆無だが強運", "Lottery Winner / No talent but incredible luck");
        Add("intro_ヨネ", "内職の鬼 / ティッシュ配りの速さは音速", "Piecework Master / Tissue-folding at the speed of sound");
        Add("intro_ドクコ", "闇金の取り立て屋 / 恐怖の回収率100%", "Loan Shark Enforcer / 100% terror collection rate");

        // ===== Father Bios =====
        Add("bio_タケシ",
            "幼少期から喧嘩に明け暮れ、14歳で地元の不良グループを壊滅させた伝説を持つ。16歳で格闘技の道に進み、わずか3年でプロデビュー。圧倒的なパワーと不屈の精神力で世界王座を5度防衛した。引退後は山奥で修行を続け、握力180kgという超人的な肉体を維持。「強さとは守るためにある」が信条で、普段は穏やかだが、大切な人を傷つける者には容赦しない。現在は秘境の道場で次世代の格闘家を育成しながら、密かに復帰を狙っている。",
            "From childhood, Takeshi lived for fighting — at 14, he single-handedly dismantled the local gang. At 16, he entered professional martial arts and debuted within three years. With overwhelming power and iron will, he defended his world title five times. After retiring, he continues training in a mountain retreat, maintaining a superhuman 180kg grip. His creed: \"Strength exists to protect.\" Calm by nature, he shows no mercy to those who threaten his loved ones. He now trains the next generation at a secluded dojo while secretly planning his comeback.");
        Add("bio_ユウキ",
            "5歳でプログラミングを独学し、8歳で政府機関のセキュリティを突破した天才少年。12歳で逮捕されるも、才能を見込まれてサイバー防衛の特別顧問に抜擢される。17歳で立ち上げたスタートアップは3年で世界最大のAI企業に成長。特許数3200件は人類史上最多記録。表向きはクールなIT長者だが、実はゲーム廃人で週末は72時間ぶっ通しでMMOをプレイしている。「世界のバグは俺が直す」と豪語するが、自室の散らかりようは致命的なバグそのもの。",
            "Self-taught programming at 5, breached government security at 8 — a true prodigy. Arrested at 12, his talent earned him a position as a special cybersecurity advisor. His startup, founded at 17, became the world's largest AI company in three years. His 3,200 patents set a human record. Outwardly a cool tech mogul, he's secretly a gaming addict who plays MMOs for 72 hours straight on weekends. He boasts \"I'll fix every bug in the world,\" yet his own room is a catastrophic bug itself.");
        Add("bio_ゴウ",
            "ちいさな村で育ち、10歳でぼうけんの旅に出た。15歳でひとりの ぼうけんかとして 世界を旅しはじめる。以後は一匹狼のフリーランスぼうけんかとして世界中を渡り歩く。100を超えるぼうけんを達成し、一度も任務に失敗したことがない。そのぼうけんりょくは計測不能とされ、各国が一目置く存在。しかし、よわいものには ぜったいに 手を出さない独自の掟を持つ。ぼうけんの合間に花が好きで、拠点には小さな庭を作る習慣がある。「本当の強さは戦わずに済む力だ」と語るが、彼の前に立てる者はいない。",
            "Raised in a small village, Gou set out on an adventure at 10. At 15, he began traveling the world as a solo adventurer, drifting through lands far and wide. Over 100 adventures completed, never a single failure. His adventure power is classified as immeasurable, respected by nations worldwide. Yet he lives by a code: never harm the weak. Between adventures, he loves flowers and always plants a small garden at his base. \"True strength means never having to fight,\" he says — but no one dares stand before him.");
        Add("bio_シンジ",
            "3歳で微分方程式を解き、7歳で大学に飛び級入学した超天才。15歳でMITの博士課程を修了し、量子力学・遺伝子工学・宇宙工学の3分野でノーベル賞を受賞。IQ250は公式に測定された人類最高記録。しかし本人は「知性に限界はない」と更なる高みを目指し続ける。研究に没頭すると3日間食事を忘れるほどの集中力を見せる一方、日常生活では靴を左右逆に履いて出歩くこともしばしば。「宇宙の真理を解き明かす」という壮大な夢のため、現在は秘密研究所で禁断の実験を行っている。",
            "Solving differential equations at 3, entering university at 7 — a super-genius. Completed his MIT doctorate at 15 and won Nobel Prizes in quantum mechanics, genetic engineering, and aerospace. His IQ of 250 is the highest officially recorded in history. Yet he insists \"intelligence has no limits\" and keeps pushing higher. When deep in research, he forgets to eat for three days — but in daily life, he often walks around with his shoes on the wrong feet. Pursuing his grand dream to \"unravel the truth of the universe,\" he now conducts forbidden experiments in a secret lab.");
        Add("bio_リョウマ",
            "貧しい漁村で生まれ、15歳で裸一貫から起業。最初のビジネスは中古の自転車修理だったが、独自の商才で事業を拡大し、20歳で初めての会社を上場させる。その後、IT・金融・宇宙開発と次々に新規事業を立ち上げ、30歳で総資産43兆円の世界一の実業家に。「稼いだ金は未来への投資だ」という信念のもと、資産の半分を教育と医療の支援に充てている。見た目はブランドスーツに身を包んだ冷徹な経営者だが、故郷の漁村には毎年必ず帰り、幼馴染と酒を酌み交わすのが唯一の息抜き。",
            "Born in a poor fishing village, Ryouma started from nothing at 15. His first business was fixing used bicycles, but his natural business instincts grew it rapidly — IPO at 20. He then launched ventures in IT, finance, and space, becoming the world's richest man at 30 with $430B. Believing \"money earned is invested in the future,\" he donates half his wealth to education and healthcare. In his designer suits, he appears ruthless, but he returns to his hometown every year without fail to drink with his childhood friends — his only escape.");
        Add("bio_テツヤ",
            "中学時代にバンドを結成し、高校中退後に上京。路上ライブで注目を集め、17歳でメジャーデビュー。デビューシングルが全世界で1億枚を売り上げ、一夜にしてスターダムへ。カリスマ的なステージパフォーマンスとアーティスティックな楽曲で8億人のファンを獲得。しかし栄光の裏でつらい時期があり、一時は活動休止に追い込まれた。長い休養を経て復帰し、その経験を綴った楽曲「Rebirth」は音楽史上最も感動的な曲と称される。現在はソロ活動の傍ら、音楽で人を救う慈善活動にも力を入れている。",
            "Formed a band in middle school, dropped out of high school and moved to Tokyo. Street performances caught attention, and he debuted at 17. His first single sold 100 million copies worldwide, catapulting him to stardom overnight. His charismatic stage presence and artistic songs amassed 800 million fans. But behind the glory, he went through a difficult period and was forced into hiatus. After a long rest, he returned — his song \"Rebirth\" is hailed as the most moving in music history. Now he pursues solo work alongside charity, using music to save lives.");

        // ===== Mother Bios =====
        Add("bio_サクラ",
            "つよさで名高い武術一族のまっすぐな血を引き、物心つく前からぶじゅつの修行を課せられた。7歳で一族の試練を全て突破し、史上最年少で「継承者」の称号を得る。しかし15歳の時、一族のやり方がまちがっていると気づき、一族を離れ旅に出た。追手をぜんいん かわしながら世界を放浪し、ぶじゅつの世界に身を投じる。全戦全勝の戦績を持ち、その拳は「だれにも負けない」と恐れられている。だが素顔は甘いものに目がない少女で、修行後は必ずパフェを食べに行くのが唯一の秘密。",
            "Born into a renowned martial arts clan with a proud lineage, Sakura was trained in the fighting arts before she could walk. At 7, she passed every trial and became the youngest heir in history. But at 15, she realized the clan's ways were wrong, left them, and set out on her own journey. She evaded every pursuer while wandering the world, entering the martial arts world. Undefeated in all bouts, her fists are said to be unbeatable. Yet beneath it all, she's a girl with a sweet tooth — her one secret is always getting a parfait after every training session.");
        Add("bio_ヒナタ",
            "地方の貧しい家庭に生まれたが、幼少期から「美」への執着が人並み外れていた。13歳で独自の美容法をSNSで発信し始め、16歳でフォロワー1000万人を突破。高校在学中に美容ブランドを立ち上げ、卒業と同時に会社を設立。革新的なスキンケア技術と天性のビジネスセンスで、わずか5年で総資産28兆円の美容帝国を築き上げた。「美は力、力は自信、自信は世界を変える」が座右の銘。完璧主義者で部下には厳しいが、孤児院への匿名寄付を20年間続けている優しさも持つ。",
            "Born into a poor rural family, Hinata had an extraordinary obsession with beauty from childhood. At 13, she started sharing her own beauty methods online; by 16, she had 10 million followers. She launched a beauty brand while still in high school and founded her company upon graduation. With innovative skincare technology and natural business sense, she built a 28-trillion-yen beauty empire in just five years. Her motto: \"Beauty is power, power is confidence, confidence changes the world.\" A perfectionist who's tough on employees, she's secretly donated to orphanages anonymously for 20 years.");
        Add("bio_アキラ",
            "元陸上選手の父と体操選手の母の間に生まれたサラブレッド。3歳で逆上がりをマスターし、5歳であらゆるスポーツの才能を見せ始める。10歳で陸上の全国大会を制覇し、14歳でオリンピック代表に選出。以降、陸上・水泳・体操・射撃・フェンシング・馬術・テコンドーの7種目で金メダルを獲得した超人アスリート。100m走10.1秒の記録は女性世界最速。練習量は1日12時間、「努力しない天才に価値はない」と語る。趣味は料理で、遠征先では必ず地元の市場で食材を買い込む。",
            "Born to a former track athlete father and gymnast mother — a true thoroughbred. She mastered the pull-up bar at 3 and showed talent in every sport by 5. National track champion at 10, Olympic team member at 14. She went on to win gold in seven events: track, swimming, gymnastics, shooting, fencing, equestrian, and taekwondo. Her 100m time of 10.1 seconds is the fastest female record. Training 12 hours a day, she says \"A genius who doesn't work hard is worthless.\" Her hobby is cooking — she always hits the local market when traveling for competitions.");
        Add("bio_ミサト",
            "2歳で文字を覚え、4歳で量子力学の入門書を読破した神童。10歳でオックスフォード大学に入学し、13歳で博士号を取得。IQ270は非公式ながら人類史上最高とされる。量子コンピュータの実用化に最も近い研究者として世界中から注目を浴び、各国政府が争って研究費を提供している。しかし本人は名声に興味がなく、「宇宙の仕組みを知りたいだけ」と淡々と語る。研究室にこもると1週間出てこないこともあるが、猫だけは別で、研究所には3匹の保護猫が自由に歩き回っている。",
            "A prodigy who learned to read at 2 and finished an intro to quantum mechanics at 4. She entered Oxford at 10 and earned her doctorate at 13. Her IQ of 270, while unofficial, is considered the highest in human history. As the researcher closest to practical quantum computing, she draws worldwide attention and governments compete to fund her work. Yet she cares nothing for fame — \"I just want to understand how the universe works,\" she says flatly. She sometimes disappears into her lab for a week, but cats are her exception: three rescue cats roam her lab freely.");
        Add("bio_カエデ",
            "医師一家の三代目として生まれ、幼少期から「人を救う」ことへの使命感が強かった。12歳で医学書を読破し、飛び級で16歳で医学部に入学。22歳で外科医としてデビューし、初年度から手術成功率100%という驚異的な記録を打ち立てる。特に心臓外科の腕は世界一と評され、他の医師が匙を投げた患者を何人も救ってきた。「この手が動く限り、一人も死なせない」という信念で年間300件以上の手術をこなす。休日は山登りが趣味で、頂上で飲むコーヒーが至福の時間だと語る。",
            "Born into a three-generation medical family, Kaede felt a powerful calling to save lives from early childhood. She devoured medical texts by 12 and entered medical school at 16 through accelerated study. Debuting as a surgeon at 22, she achieved a 100% success rate from her very first year. Widely regarded as the world's best cardiac surgeon, she has saved countless patients others gave up on. Living by the creed \"As long as these hands move, I won't let anyone die,\" she performs over 300 surgeries a year. On days off, she hikes mountains — her bliss is coffee at the summit.");
        Add("bio_ルナ",
            "東欧の小さな村で生まれ、幼い頃から周囲を圧倒する美貌の持ち主だった。14歳でスカウトされてパリに渡り、デビューショーで世界中のファッション誌の表紙を独占。身長180cm、完璧なプロポーションと神秘的なオーラで「歩く芸術品」と称される。しかし華やかな世界の裏で過酷な減量やパワハラに苦しみ、一度はモデルを辞める決意をした。だが「自分の美で誰かを勇気づけたい」という想いで復帰し、ボディポジティブ運動の先駆者となる。現在は自身のブランドを持ち、売上の30%を母国の教育支援に寄付している。",
            "Born in a small Eastern European village, Luna possessed overwhelming beauty from childhood. Scouted at 14 and brought to Paris, her debut show landed her on every fashion magazine cover worldwide. At 180cm with perfect proportions and a mystical aura, she's called \"a walking work of art.\" But behind the glamour, she suffered through extreme dieting and harassment, and once resolved to quit. Yet she returned, driven by the desire to inspire others through her beauty, becoming a pioneer of the body positivity movement. She now owns her own brand, donating 30% of sales to education in her homeland.");

        // ===== New Father Bios =====
        Add("bio_ゼニガタ",
            "代々続く石油採掘一族の跡取りとして生まれ、5歳で初めて油田を視察。12歳で原油先物取引を始め、15歳で初の10億ドルを稼ぎ出す。20歳で一族の全事業を継承し、中東・アフリカ・北極圏に油田を拡大。総資産は国家予算を超え、「金で買えないものはない」が口癖。プラチナ製のゆりかごを我が子に贈り、おむつ交換ですら執事に任せる。しかし子供の寝顔を見る時だけは、世界一の富豪もただの親バカに戻る。密かに全資産の70%を教育基金に遺贈する遺言を書いている。",
            "Born as heir to a dynasty of oil barons, Zenigata inspected his first oil field at 5. He started crude oil futures at 12 and made his first billion by 15. At 20, he inherited all family operations, expanding to the Middle East, Africa, and the Arctic. His fortune exceeds national budgets — \"Money buys everything\" is his motto. He gifted his child a platinum cradle and leaves even diaper changes to the butler. Yet when watching his baby sleep, the world's richest man becomes just another doting dad. He's secretly written a will donating 70% of his wealth to education.");
        Add("bio_ツクモ",
            "幼少期から「見えないものが見える」と周囲を困惑させた不思議な少年。7歳で近所の火事を予言し、10歳で地震の発生を言い当て、地元では「神童」と呼ばれた。15歳で量子物理学に目覚め、「宇宙の波動と人間の意識は同期している」という独自理論を構築。IQ300の頭脳で大学を3つ卒業するも、学会からは異端視され続ける。現在は自宅の屋根裏部屋で宇宙と交信しながら、時折驚くほど正確な予言を的中させる。育児中も上の空で、おむつの替え時だけは宇宙に聞いている。",
            "A mysterious boy who unsettled everyone by \"seeing the invisible\" from early childhood. He predicted a neighbor's fire at 7 and an earthquake at 10, earning him the label of prodigy. At 15, he discovered quantum physics and built his own theory that \"cosmic waves sync with human consciousness.\" With an IQ of 300, he graduated from three universities but remains shunned by academia. Now he communicates with the universe from his attic, occasionally making startlingly accurate predictions. Even while parenting, his mind wanders — he consults the cosmos only for diaper-change timing.");
        Add("bio_サトウ",
            "埼玉県出身。県立高校を卒業後、地元の中堅メーカーに就職。以来30年間、無遅刻無欠勤で係長まで昇進した。特技は洗車。休日の楽しみはホームセンター巡り。年収は平均的、容姿も平均的、運動能力も平均的。あらゆるステータスが日本人男性の中央値をぴったり示す奇跡の存在。しかしその「普通さ」こそが最大の武器であり、どんな修羅場でも動じない鋼のメンタルを持つ。「普通が一番」が座右の銘。妻の手料理と子供の笑顔があれば他に何もいらないと本気で思っている。",
            "From Saitama. After graduating from a prefectural high school, he joined a mid-tier manufacturer and has been promoted to section chief over 30 years with perfect attendance. His specialty is car washing. Weekend fun: home center visits. Average income, average looks, average athleticism — a miraculous existence whose every stat hits the Japanese male median. Yet this \"ordinariness\" is his greatest weapon, giving him nerves of steel in any crisis. His motto: \"Normal is best.\" He genuinely believes his wife's cooking and his child's smile are all he needs.");
        Add("bio_イワオ",
            "山奥の寒村で生まれ、小学校に通う代わりに父親と共に石を割って育った。12歳で大人の作業員を腕相撲で全員倒し、15歳で単身上京。土木作業員として橋やダムの建設に携わり、「素手で岩を砕く男」として現場では伝説的な存在。握力は両手合わせて350kg。しかし極度の貧乏で、日雇いの給料はほぼ全額を故郷の母に仕送りしている。プロテインを買う金がないため、河原の石を持ち上げてトレーニングする日々。言葉は少ないが、その背中で全てを語る不器用な男。",
            "Born in a remote mountain village, he grew up splitting rocks with his father instead of attending school. At 12, he beat every adult worker at arm wrestling; at 15, he moved to Tokyo alone. As a construction worker building bridges and dams, he became legendary as \"the man who crushes rocks bare-handed.\" Combined grip strength: 350kg. Yet he's extremely poor, sending nearly all his day-labor wages to his mother back home. Unable to afford protein powder, he trains by lifting river stones. A man of few words who says everything with his back.");
        Add("bio_アキトシ",
            "中学時代に競馬にハマり、高校を中退してパチンコ店に入り浸る。18歳で「確率と心理の天才」と呼ばれるようになり、ポーカーの世界大会で準優勝。しかし賞金は翌日のカジノで全額蒸発させる筋金入りの浪費家。通帳残高は常にゼロだが、勝負の嗅覚だけは超一流。「人生はギャンブルだ。全部賭けてこそ面白い」が信条。新聞紙でおくるみを作り、子供のおもちゃはサイコロとトランプ。負け続けても笑っていられる精神力だけは誰にも負けない。最近、子供の笑顔が最高の当たりだと気づき始めている。",
            "Hooked on horse racing in middle school, he dropped out of high school and haunted pachinko parlors. By 18, he was called \"a genius of probability and psychology\" and placed second at the World Poker Championship. But he blew the entire prize at a casino the next day — a true spendthrift. His bank balance is perpetually zero, yet his instinct for a gamble is world-class. \"Life is a gamble — it's only fun when you go all in.\" He wraps his baby in newspaper; the toys are dice and cards. His unbreakable spirit that laughs through every loss is unmatched. Lately, he's starting to realize his child's smile is the greatest jackpot.");
        Add("bio_ネオ",
            "生まれてから30年間、一度も実家の敷地から出たことがない伝説の引きこもり。しかしネット上では別人格を持ち、複数のMMOで世界ランカー、匿名掲示板では「神」と崇められている。プログラミング・イラスト・作曲を独学でマスターし、実はフリーランスとして月収100万円を稼いでいるが、全額ガチャに溶かしている。布団から出るのは冷蔵庫に行く時だけ。日光を浴びると体調を崩す特異体質。子供が生まれてから唯一変わったのは、おむつを買いにコンビニまで歩くようになったこと。それが彼にとっては冒険そのものだった。",
            "A legendary shut-in who hasn't left his family home's grounds in 30 years. Online, however, he's a different person — a world-ranked MMO player worshipped as a \"god\" on anonymous forums. Self-taught in programming, illustration, and music composition, he actually earns a million yen monthly as a freelancer but blows it all on gacha. He only leaves his futon to visit the fridge. Sunlight literally makes him sick. The one thing that changed after his child was born: he now walks to the convenience store for diapers. For him, that's a genuine adventure.");

        // ===== New Mother Bios =====
        Add("bio_イザナミ",
            "神話の時代から続く皇統の末裔を自称する謎の女帝。その一言で国家予算が動き、指一本で政権が変わると噂される。幼少期から帝王学を叩き込まれ、10歳で国際会議に出席、15歳で初の外交条約を締結。美貌と知性と冷酷さを兼ね備え、「氷の女帝」と恐れられている。しかし我が子の前でだけは別人のように穏やかになり、手ずから離乳食を作る姿は側近すら見たことがない秘密。最高級の教育環境を用意し、子供には「世界を統べる者」になることを期待している。縁側で静かに茶を啜る姿は、ただの優しい母親にしか見えない。",
            "A mysterious empress claiming descent from a mythological imperial line. One word from her moves national budgets; one finger reportedly topples governments. Trained in statecraft from birth, she attended international conferences at 10 and signed her first treaty at 15. Beautiful, brilliant, and ruthless — feared as the \"Ice Empress.\" Yet before her child, she transforms completely, secretly preparing baby food by hand — a sight even her aides have never witnessed. She provides the finest education, expecting her child to \"rule the world.\" Sipping tea quietly on the veranda, she looks like nothing more than a gentle mother.");
        Add("bio_ミク",
            "地方のギャルから成り上がった自称モデル兼インフルエンサー。フォロワー数は500万人を超えるが、その半分は購入したもの。美意識だけは本物で、毎朝2時間のメイクと毎晩3時間のスキンケアを欠かさない。収入の9割を美容と自己プロデュースに注ぎ込み、生活は常に火の車。しかし「見栄を張ることが生きること」という信念は揺るがない。子供にはブランド服を着せ、哺乳瓶すらデコる徹底ぶり。内心では「ありのままの自分」を誰かに認めてほしいと願っている。カメラが回っていない時の素顔は、意外なほど素朴で優しい。",
            "A self-proclaimed model and influencer who rose from small-town gyaru culture. Over 5 million followers — half of them purchased. Her beauty standards are genuine: 2 hours of morning makeup and 3 hours of nightly skincare, never missed. She pours 90% of her income into beauty and self-branding, keeping her finances perpetually ablaze. Yet her belief that \"keeping up appearances is living\" never wavers. She dresses her baby in designer clothes and even decorates the baby bottle. Deep down, she wishes someone would accept her true self. Off-camera, her real face is surprisingly simple and kind.");
        Add("bio_カヨコ",
            "商店街の惣菜屋「かよこ食堂」の三代目看板娘。祖母から受け継いだコロッケのレシピは門外不出。朝5時に起きて仕込みを始め、閉店後は翌日の仕入れと帳簿付け。365日休みなしで働くが、「お客さんの『おいしい』が聞ければ十分」と笑顔を絶やさない。商店街の全住民から愛され、困っている人がいれば必ず声をかける。実は料理の腕は全国レベルで、テレビ出演のオファーを何度も断っている。「この街にいるのが一番幸せ」という言葉に嘘はない。子供にも「人を笑顔にする料理」を教えたいと思っている。",
            "Third-generation poster girl of the shopping street deli \"Kayoko Shokudou.\" Her grandmother's croquette recipe is a closely guarded secret. She rises at 5 AM to prep and handles purchasing and bookkeeping after closing — 365 days a year with no days off. Yet \"hearing customers say 'delicious' is enough\" keeps her smiling. Beloved by every resident on the shopping street, she always reaches out to anyone in trouble. Her cooking is actually nationally competitive, but she's turned down TV appearances repeatedly. \"Being in this town makes me happiest\" — and she means it. She wants to teach her child \"cooking that makes people smile.\"");
        Add("bio_フクトク",
            "生まれつき異常な強運の持ち主。初めて買った宝くじで3億円当選、二度目で7億円当選。ガチャは必ず最高レアが出る。じゃんけんは生涯無敗。しかし本人には何の才能もなく、勉強もスポーツも平均以下。「運だけで生きてきた」という自覚はあるが、それを恥じるどころか「運も実力のうち」と開き直っている。宝くじ売り場のパートで働きながら、当選金は堅実に投資して資産を増やしている。子供にも運が遺伝することを密かに期待しているが、「運がなくても愛があれば大丈夫」と言い聞かせている。",
            "Born with extraordinary luck. Her first lottery ticket won 300 million yen; her second, 700 million. Gacha always gives her the rarest pull. She's never lost at rock-paper-scissors in her life. Yet she has zero talent — below average in both academics and sports. She's aware she's \"lived on luck alone\" but isn't ashamed — \"Luck is a skill too,\" she insists. Working part-time at a lottery booth, she steadily invests her winnings. She secretly hopes her child will inherit her luck but reassures them: \"Even without luck, love is enough.\"");
        Add("bio_ヨネ",
            "内職歴40年の大ベテラン。ティッシュ折り、シール貼り、封入作業、あらゆる内職をこなし、その速度は「音速の手」と称される。一日に折るティッシュは5000枚、ミシンの縫い目は0.1mmの狂いもない。収入は微々たるものだが、1円たりとも無駄にしない生活哲学を持つ。手作りにこだわり、子供のおくるみも哺乳瓶カバーも全て手縫い。「買えるものより作れるものの方が温かい」が信条。質素だが丁寧な暮らしの中に、確かな幸せを見出している。夜中にミシンを踏む音が、子供にとっての子守唄になっている。",
            "A 40-year veteran of piecework. Tissue folding, sticker affixing, envelope stuffing — she does it all, her speed earning her the title \"hands at the speed of sound.\" She folds 5,000 tissues a day with sewing accuracy within 0.1mm. Her income is meager, but she lives by a philosophy of wasting not a single yen. Committed to handmade goods, she sews every baby blanket and bottle cover herself. \"What you make with your hands is warmer than what you buy.\" She finds genuine happiness in her modest but meticulous life. The sound of her sewing machine at night has become her child's lullaby.");
        Add("bio_ドクコ",
            "闇金融「毒蛇ファイナンス」のNo.1取り立て屋。回収率100%という恐怖の記録を持ち、その名を聞くだけで債務者が震え上がる。元は薬学部の優等生だったが、製薬会社の不正を告発して業界から追放された過去を持つ。「毒と薬は紙一重」が座右の銘で、怪しい瓶が並ぶ自宅の棚には実は高度な漢方薬が詰まっている。恐ろしい形相の裏に隠された母性は深く、子供が熱を出せば夜通し看病し、手作りの薬草茶を飲ませる。「トイチ」を子供の最初の言葉にしようとしているが、今のところ「ママ」が優勢。",
            "The No.1 enforcer of \"Viper Finance,\" an underground lending operation. Her 100% collection rate is legendary — debtors tremble at her name alone. Originally a top pharmacy student, she was expelled from the industry after exposing corporate fraud. \"Poison and medicine are two sides of the same coin\" is her motto; the suspicious bottles lining her shelves actually contain sophisticated herbal remedies. Behind her terrifying exterior lies deep maternal instinct — she nurses her sick child through the night with homemade herbal tea. She's trying to make \"compound interest\" her baby's first words, but \"Mama\" is currently winning.");

        // ===== Love Stories =====
        // タケシ × mothers
        Add("love_タケシ_サクラ",
            "ぶじゅつかいの 頂点を決める大会。\nタケシとサクラは決勝で激突した。\n\nつよいわざが 交錯する中、\n二人は互いの強さに惹かれていく。\n\nしあいは 引き分けに終わり、\n「決着は別の形でつけよう」と\nタケシが差し出した手を、\nサクラは静かに握り返した。\n最強の血統がここに誕生する。",
            "A tournament to decide the champion of martial arts.\nTakeshi and Sakura clashed in the finals.\n\nAs powerful techniques collided,\nthey were drawn to each other's strength.\n\nThe match ended in a draw.\n\"Let's settle this another way,\"\nTakeshi extended his hand,\nand Sakura quietly took it.\nThe strongest bloodline was born.");
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
            "世界格闘技選手権の決勝戦。\nタケシはライバルとの はげしい しあいの末、\n右腕をけがしてしまった。\n\n「二度とうごかせない」と宣告される中、\n唯一の希望は天才外科医カエデだった。\n\n12時間に及ぶ手術。\n目覚めたタケシの最初の言葉は\n「俺の腕を救ってくれた君を、\n俺の人生に迎えたい」だった。",
            "The World Martial Arts Championship finals.\nTakeshi injured his arm badly\nafter an intense match with his rival.\n\nTold he'd \"never move it again,\"\nhis only hope was genius surgeon Kaede.\n\nAfter 12 hours of surgery,\nTakeshi's first words upon waking:\n\"I want you, who saved my arm,\nto be part of my life.\"");
        Add("love_タケシ_ルナ",
            "スポーツ雑誌の表紙撮影。\n格闘家とスーパーモデルの共演。\n\nカメラの前で火花が散り、\n「もっと近づいて」という\nカメラマンの指示に、\n二人の心臓が高鳴る。\n\n撮影後、ルナが言った。\n「あなたの隣にいると、\n自分が美しく見える気がする」\nスポットライトの下で恋が始まった。",
            "A sports magazine cover shoot.\nA fighter and supermodel together.\n\nSparks flew before the camera.\n\"Get closer,\" the photographer said,\nand both their hearts raced.\n\nAfter the shoot, Luna said:\n\"Standing next to you,\nI feel more beautiful.\"\nLove began under the spotlight.");

        // ユウキ × mothers
        Add("love_ユウキ_サクラ",
            "あやしい組織のサーバーに侵入した夜、\nユウキは追手に囲まれた。\n\nその中にいたのがサクラ。\n「あなたを とめるつもりはない。\nあなたの腕が必要なの」\n\n組織を離れ、共に逃亡する日々。\n追われる中で芽生えた信頼は、\nいつしか愛に変わっていた。\n\n「俺のファイアウォールは\n君だけ通過できる」\n不器用な告白だった。",
            "The night Yuuki infiltrated a\nsuspicious organization's server,\nhe was surrounded by pursuers.\n\nAmong them was Sakura.\n\"I won't stop you.\nI need your skills.\"\n\nLeaving the organization, fleeing together.\nTrust born while on the run\neventually became love.\n\n\"My firewall only lets you through.\"\nAn awkward confession.");
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
            "ぼうけんの途中で 出会った二人。\nにらみ合いながら、\n奇妙な沈黙が流れた。\n\n「きみと たたかう理由がない」\n「私もよ」\n\nにらみあいをやめた瞬間、\n組織に追われる身となった。\n\n「一緒に逃げないか」\n「どこまでも」\n\n世界中を逃げ回る日々が、\n二人を離れられない関係にした。",
            "Two who met in the middle of an adventure.\nStaring each other down,\nan eerie silence fell.\n\n\"I have no reason to fight you.\"\n\"Neither do I.\"\n\nThe moment they stopped their standoff,\nthey became hunted by the organization.\n\n\"Run away with me?\"\n\"Anywhere.\"\n\nDays fleeing across the world\nmade them inseparable.");
        Add("love_ゴウ_ヒナタ",
            "要人警護の任務。\nねらわれていたのはヒナタだった。\n\n三度のきけん、\nその全てからゴウは彼女を守った。\n三度目のきけんから\n身を挺してかばった時、\nヒナタは悟った。\n\n「お金じゃ買えないものがある」\n\n病室で目覚めたゴウに、\n彼女は涙ながらに言った。\n「私の人生を守って」",
            "A VIP protection mission.\nHinata was the one being targeted.\n\nThree dangers —\nGou protected her from all of them.\nWhen he shielded her\nwith his own body the third time,\nHinata realized:\n\n\"There are things money can't buy.\"\n\nTo Gou, waking in the hospital,\nshe said through tears:\n\"Protect my life.\"");
        Add("love_ゴウ_アキラ",
            "とおい国でのスポーツ親善大使。\nアキラの警護を任されたゴウは、\n彼女の無邪気さに戸惑った。\n\n「怖くないのか？」\n「あなたがいるから」\n\nきけんの中でも笑顔を絶やさない彼女。\n守るべき存在が、\nいつしか愛する人に変わっていた。\n\n任務終了の日、\nゴウは ぼうけんかを やめる決意をした。",
            "A sports goodwill ambassador in a distant land.\nGou, assigned to guard Akira,\nwas bewildered by her innocence.\n\n\"Aren't you afraid?\"\n\"You're here, so no.\"\n\nShe never stopped smiling amid danger.\nThe person he had to protect\nbecame the person he loved.\n\nOn the last day of the mission,\nGou decided to quit being an adventurer.");
        Add("love_ゴウ_ミサト",
            "ふしぎな ほしのデータ解析依頼。\nつよい ぼうけんかゴウと、\n純粋な物理学者ミサト。\n\n「なぜ ひとりで たたかうの？」\n直球の質問に、\nゴウは言葉を失った。\n\n「...答えが見つからない」\n「一緒に探しましょう」\n\nミサトの純粋さが、\n凍った心を少しずつ溶かしていく。\nぼうけんの おおかみが 愛を知った瞬間だった。",
            "A request to analyze data from a mysterious star.\nGou, the strong adventurer,\nand Misato, the pure physicist.\n\n\"Why do you fight alone?\"\nAt her blunt question,\nGou was speechless.\n\n\"...I can't find the answer.\"\n\"Let's search together.\"\n\nMisato's purity slowly melted\nhis frozen heart.\nThe moment the adventure wolf learned love.");
        Add("love_ゴウ_カエデ",
            "ぼうけんで たおれた仲間を救うため、\nゴウは国境を越えて\n天才外科医を探した。\n\n「報酬はいくらでも払う」\n「お金じゃないの。\nあなたが連れてきて」\n\nきけんな場所に 飛び込んだカエデ。\n命がけの手術を終えた夜、\nゴウは初めて泣いた。\n\n「俺の人生を守ってくれないか」\nぼうけんかの 不器用な プロポーズだった。",
            "To save a comrade who fell during an adventure,\nGou crossed borders seeking\na genius surgeon.\n\n\"I'll pay any price.\"\n\"It's not about money.\nBring me there.\"\n\nKaede dove into the dangerous place.\nAfter a life-risking surgery that night,\nGou cried for the first time.\n\n\"Will you protect my life?\"\nAn adventurer's clumsy proposal.");
        Add("love_ゴウ_ルナ",
            "ぼうけん写真家として同行したルナ。\n「真実を伝えたい」という\n彼女の覚悟に、ゴウは驚いた。\n\nあらしの夜、ほら穴で 肩を寄せ合い、\nつよい きずなの なかで\n二人は唇を重ねた。\n\n「生きて帰ろう」\n「ああ、一緒にな」\n\nぼうけんの中で 誓った愛は、\nどんな平和な恋より強く、\n深く結ばれていた。",
            "Luna joined as an adventure photographer.\n\"I want to show the truth\" —\nher resolve surprised Gou.\n\nOn a stormy night, huddled in a cave,\nwithin their strong bond,\nthey kissed.\n\n\"Let's make it home alive.\"\n\"Yeah, together.\"\n\nLove sworn during an adventure\nwas stronger and deeper\nthan any peacetime romance.");

        // シンジ × mothers
        Add("love_シンジ_サクラ",
            "「ぶじゅつの科学的解明」\nその研究テーマに、\nサクラは協力を申し出た。\n\n動きを解析するうちに、\nシンジの目は彼女自身に向いていた。\n\n「論文より君を研究したい」\n「それ、口説いてる？」\n「...多分」\n\n世界一不器用な告白に、\nつわものは 初めて 頬を染めた。\n愛は科学で証明できないと知った。",
            "\"Scientific Analysis of Martial Arts\"\nSakura volunteered to help\nwith this research topic.\n\nWhile analyzing her movements,\nShinji's eyes turned to her.\n\n\"I want to study you, not the paper.\"\n\"Is that a pickup line?\"\n\"...Probably.\"\n\nAt the world's most awkward confession,\nthe warrior blushed for the first time.\nHe learned love can't be proven by science.");
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
            "ボディガードとして雇った つわもの。\n命を預けた相手に、\n心まで奪われるとは思わなかった。\n\n「金で動く女か」\n「いいえ、あなたを守りたいから」\n\n嘘のない瞳だった。\n\n43兆円あっても買えないもの。\nそれは信頼と愛だと、\nリョウマは初めて知った。\n「俺の傍にいてくれ、永遠に」",
            "A warrior hired as a bodyguard.\nHe never thought he'd lose his heart\nto the person he trusted with his life.\n\n\"A woman who works for money?\"\n\"No — I want to protect you.\"\n\nHer eyes held no lies.\n\nSomething $430B can't buy:\ntrust and love.\nRyouma learned this for the first time.\n\"Stay by my side, forever.\"");
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
            "新曲MVのアクションシーン。\n指導者として現れたサクラの\n鋭い動きに、テツヤは見惚れた。\n\n「もっと本気で来て」\n「怪我させるぞ」\n「それくらいが丁度いい」\n\nステージで わざを 交えるうちに、\n二人の距離は縮まっていった。\n\n撮影終了後の楽屋で、\n二人は激しく唇を重ねた。",
            "An action scene for a new music video.\nTetsuya was mesmerized by\nSakura's sharp movements as instructor.\n\n\"Come at me for real.\"\n\"I'll hurt you.\"\n\"That's exactly what I want.\"\n\nAs they exchanged techniques on stage,\nthe distance between them closed.\n\nIn the dressing room after filming,\nthey shared a passionate kiss.");
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

        // ゼニガタ（石油王）× 新母親
        Add("love_ゼニガタ_イザナミ",
            "世界経済フォーラムの最前列。\n石油王と女帝が隣り合った。\n\n「この会場、買い取ろうか？」\n「もう買ってあるわ」\n\n互いの資産自慢が\nいつの間にか笑い合いに変わり、\n晩餐会では二人だけの席を用意した。\n\n「金で買えないものはない」\n「でも、あなたの心は私がもらうわ」\n世界最強の権力カップル誕生。",
            "Front row at the World Economic Forum.\nThe oil king and the empress sat side by side.\n\n\"Shall I buy this venue?\"\n\"I already own it.\"\n\nTheir battle of wealth\nturned into shared laughter.\n\n\"Money can buy anything.\"\n\"But I'll take your heart for free.\"\nThe world's most powerful couple was born.");
        Add("love_ゼニガタ_ミク",
            "インフルエンサー案件の依頼。\n「石油を世界一オシャレに撮れ」\n\nゼニガタの無茶な依頼に\nミクは全力で応えた。\n油田をバックにキメポーズ。\n\n「フォロワー100万人増えたぞ」\n「でしょ？私の実力よ」\n\n見栄っ張り同士、\n嘘と本音の境界が溶けた夜、\nゼニガタは言った。\n「君だけは本物だ」",
            "An influencer sponsorship deal.\n\"Make oil look fashionable.\"\n\nMiku gave it her all\nfor Zenigata's absurd request.\nPosing in front of oil fields.\n\n\"Gained a million followers!\"\n\"See? That's my talent.\"\n\nTwo show-offs together,\nthe line between lies and truth melted.\n\"You're the only real thing,\" he said.");
        Add("love_ゼニガタ_カヨコ",
            "高級車で商店街に迷い込んだ石油王。\nコロッケの匂いに導かれ、\n看板娘カヨコの店にたどり着いた。\n\n「このコロッケ、いくらだ？」\n「80円ですよ」\n「80億出す」\n「おつり出せません」\n\n毎日通うゼニガタ。\nプラチナカードより\n温かいコロッケが欲しかった。\n「金じゃなく、心で買えるものがあるんだな」",
            "The oil king got lost in a shopping street.\nLed by the smell of croquettes,\nhe found Kayoko's shop.\n\n\"How much for this croquette?\"\n\"80 yen.\"\n\"I'll pay 8 billion.\"\n\"I can't make change for that.\"\n\nZenigata came every day.\nHe wanted warm croquettes\nmore than platinum cards.\n\"Some things money can't buy.\"");
        Add("love_ゼニガタ_フクトク",
            "カジノVIPルームでの出会い。\nフクトクは持ち金0から\nルーレットだけで1億を稼いだ。\n\n「その運、買いたい」\n「運は売れませんよ」\n\n金で買えない唯一のもの。\nゼニガタは初めて挫折を味わった。\n\nだが隣にいるだけで\n事業がうまくいく不思議。\n「君は僕の最高の投資だ」\n「私は無料よ」",
            "A meeting in the casino VIP room.\nFukutoku turned nothing\ninto 100 million at roulette.\n\n\"I want to buy that luck.\"\n\"Luck isn't for sale.\"\n\nThe one thing money can't buy.\nZenigata tasted failure for the first time.\n\nBut with her nearby,\nbusiness always went well.\n\"You're my best investment.\"\n\"I'm free of charge.\"");
        Add("love_ゼニガタ_ヨネ",
            "節税対策で訪れた下町の税理士事務所。\n待合室で内職をしていたヨネの\nティッシュ折りの速さに目を奪われた。\n\n「君、うちの工場で働かないか？」\n「時給いくらですか？」\n「好きなだけ」\n\nだがヨネは断った。\n「手作りに意味があるんです」\n\nその言葉が石油王の心を揺さぶった。\n金で買えない職人魂に、恋をした。",
            "Visiting a downtown tax office.\nIn the waiting room, Yone was doing piecework.\nHer tissue-folding speed was mesmerizing.\n\n\"Work at my factory?\"\n\"What's the hourly rate?\"\n\"Whatever you want.\"\n\nBut Yone refused.\n\"Handmade things have meaning.\"\n\nThose words shook the oil king's heart.\nHe fell in love with a craftsman's soul\nthat money couldn't buy.");
        Add("love_ゼニガタ_ドクコ",
            "借金取りが石油王の屋敷に乗り込んだ。\n「利息、払ってもらおうか」\n\n実は前妻の借金だった。\nゼニガタは札束で頬を叩こうとしたが、\nドクコは微動だにしなかった。\n\n「金で黙ると思うな」\n「...面白い女だ」\n\n恐怖を知らない女に、\n石油王は初めて震えた。\nそれは恐怖ではなく、恋だった。",
            "A debt collector stormed the oil king's mansion.\n\"Time to pay the interest.\"\n\nIt was his ex-wife's debt.\nZenigata tried to slap her with cash,\nbut Dokuko didn't flinch.\n\n\"Don't think money shuts me up.\"\n\"...Interesting woman.\"\n\nBefore this fearless woman,\nthe oil king trembled for the first time.\nIt wasn't fear. It was love.");

        // ツクモ（自称予言者）× 新母親
        Add("love_ツクモ_イザナミ",
            "「3日後、あなたの帝国に\n危機が訪れる」\n\nツクモの予言を鼻で笑った\nイザナミだったが、\n本当に株が大暴落した。\n\n「次は何が見える？」\n「あなたと僕が結ばれる未来」\n「...それだけはハズレね」\n\nだが1年後、二人は一緒にいた。\n「予言は当たったな」\n「偶然よ」\n女帝は赤くなった顔を隠した。",
            "\"In 3 days, your empire\nwill face a crisis.\"\n\nIzanami laughed off Tsukumo's prophecy,\nbut the stock market really crashed.\n\n\"What do you see next?\"\n\"A future where we're together.\"\n\"...That's the one that'll be wrong.\"\n\nBut a year later, they were together.\n\"My prophecy came true.\"\n\"Coincidence.\"\nThe empress hid her blushing face.");
        Add("love_ツクモ_ミク",
            "「来世の運命を占います」\n怪しい路上占い師ツクモに、\nミクはネタ目的で近づいた。\n\n「あなたは...本当は\n見栄を張るのに疲れている」\n\n図星だった。\nカメラを止めた瞬間、\nミクは泣き出した。\n\n「誰にも言えなかったの」\n「宇宙は全部知ってるよ」\n\n嘘ばかりの世界で、\n本音を見抜く男に惹かれた。",
            "\"I'll read your destiny.\"\nMiku approached the shady fortune teller\njust for content.\n\n\"You're... actually tired\nof keeping up appearances.\"\n\nBullseye.\nThe moment the camera stopped,\nMiku burst into tears.\n\n\"I couldn't tell anyone.\"\n\"The universe knows everything.\"\n\nIn a world of lies,\nshe fell for the man who saw the truth.");
        Add("love_ツクモ_カヨコ",
            "商店街の福引でツクモが大当たりを\n連発した。\n「明日は雨」「当たり」\n「来週、猫が来る」「来た」\n\n看板娘カヨコは半信半疑だったが、\nある日ツクモが言った。\n\n「明日、君は恋をする」\n「え、誰と？」\n「僕と」\n\n次の日、雨で店に駆け込んできた\nツクモを見て、カヨコは笑った。\n「当たりかもね」",
            "Tsukumo hit the jackpot repeatedly\nat the shopping street lottery.\n\"Rain tomorrow.\" Correct.\n\"A cat next week.\" It came.\n\nKayoko was skeptical,\nbut one day Tsukumo said:\n\n\"Tomorrow, you'll fall in love.\"\n\"With whom?\"\n\"With me.\"\n\nThe next day, when Tsukumo\nrushed in from the rain,\nKayoko laughed. \"Maybe you're right.\"");
        Add("love_ツクモ_フクトク",
            "「あなたには\n常軌を逸した運がある」\nツクモはフクトクを見て断言した。\n\n「知ってるわ、宝くじ当たったし」\n「そうじゃない。\n僕と出会ったことが最大の幸運だ」\n\nドン引きするフクトクだったが、\nツクモの隣にいると\nなぜか良いことが重なった。\n\n「予言者とラッキーガール。\n確率論の破壊者ね、私たち」",
            "\"You have\nextraordinary luck.\"\nTsukumo declared upon seeing Fukutoku.\n\n\"I know, I won the lottery.\"\n\"That's not it.\nMeeting me is your greatest fortune.\"\n\nFukutoku was put off,\nbut good things kept happening\nnear Tsukumo.\n\n\"A prophet and a lucky girl.\nWe're probability's worst nightmare.\"");
        Add("love_ツクモ_ヨネ",
            "「宇宙が...ティッシュを\n折れと言っている」\n\n内職場にふらりと現れた\n自称・予言者に、\nヨネは冷たく言った。\n「手を動かして」\n\nだがツクモの内職スピードは\n驚異的だった。IQ300の手先。\n\n「あなた、予言より\nこっちの方が向いてるわよ」\n「君の隣なら何でもいい」\n宇宙より近い距離で恋が芽生えた。",
            "\"The universe says...\nfold tissues.\"\n\nThe self-proclaimed prophet drifted\ninto Yone's workshop.\n\"Use your hands,\" she said coldly.\n\nBut Tsukumo's speed was incredible.\nIQ 300 fingers.\n\n\"You're better at this\nthan prophecy.\"\n\"Anything's fine if I'm beside you.\"\nLove bloomed closer than the cosmos.");
        Add("love_ツクモ_ドクコ",
            "「3日以内に返済しないと...」\nドクコの取り立てに、\nツクモは静かに言った。\n\n「明後日、宝くじの\n当選番号を教えよう」\n「ふざけるな」\n\nだが本当に当たった。\n\n「なぜわかる？」\n「宇宙と交信してるから」\n「...次も当ててみろ」\n\n取り立てがデートに変わった。\n闇金業者が予言者に墜ちた。",
            "\"Pay up in 3 days or else...\"\nTo Dokuko's threat,\nTsukumo calmly said:\n\n\"I'll tell you tomorrow's\nwinning lottery numbers.\"\n\"Don't mess with me.\"\n\nBut it really hit.\n\n\"How did you know?\"\n\"I communicate with the universe.\"\n\"...Do it again.\"\n\nDebt collection became dates.\nThe loan shark fell for the prophet.");

        // サトウ（普通の係長）× 新母親
        Add("love_サトウ_イザナミ",
            "区役所の窓口で順番待ちをする\n女帝イザナミ。\n「なぜ私が並ばなければ...」\n\n隣のサトウが静かに言った。\n「みんな平等ですよ、ここでは」\n\nその「普通」に、\nイザナミは衝撃を受けた。\n\n「あなた、面白いわね」\n「よく言われます」\n\n世界を支配する女が、\n世界一普通の男に恋をした。\n平凡こそが最大の魅力だった。",
            "The empress Izanami waiting in line\nat the ward office.\n\"Why must I wait...\"\n\nSatou quietly said:\n\"Everyone's equal here.\"\n\nThat 'ordinariness'\nshocked Izanami.\n\n\"You're interesting.\"\n\"I get that a lot.\"\n\nThe woman who rules the world\nfell for the world's most ordinary man.\nNormality was the greatest charm.");
        Add("love_サトウ_ミク",
            "「映えるランチ」を探すミクが\n偶然入った定食屋で、\nサトウが生姜焼きを食べていた。\n\n「それ、全然映えないですよ」\n「うまいよ？食べてみな」\n\n一口食べたミクの目が輝いた。\n「...おいしい」\n\n「映え」より「旨い」を\n教えてくれた男。\nSNSに載せない幸せを、\nミクは初めて知った。",
            "Miku was hunting for photogenic food\nwhen she stumbled into a diner.\nSatou was eating ginger pork.\n\n\"That's not Instagram-worthy at all.\"\n\"It's good though. Try it.\"\n\nOne bite and Miku's eyes lit up.\n\"...Delicious.\"\n\nThe man who taught her\n'tasty' beats 'pretty.'\nMiku discovered happiness\nthat doesn't need posting.");
        Add("love_サトウ_カヨコ",
            "毎朝コロッケを買いに来る\nサラリーマン。\n「いつもの一個ください」\n「はい、いつもの」\n\n雨の日も風の日も、\n10年間変わらないやり取り。\n\nある日カヨコが風邪で休むと、\nサトウはコロッケの代わりに\n薬を持ってきた。\n\n「いつもの恩返しです」\n\n日常に溶け込んだ愛。\nそれが一番温かかった。",
            "A salaryman who buys croquettes every morning.\n\"The usual, please.\"\n\"Here's your usual.\"\n\nRain or shine,\nthe same exchange for 10 years.\n\nOne day when Kayoko was sick,\nSatou brought medicine\ninstead of buying croquettes.\n\n\"Returning the favor.\"\n\nLove woven into daily life.\nThat was the warmest kind.");
        Add("love_サトウ_フクトク",
            "フクトクが宝くじを買った売り場で\n偶然後ろに並んでいたサトウ。\n\n「一枚だけ買うんですか？」\n「一枚で十分。当たるから」\n\n本当に当たった。\n驚くサトウに、フクトクは言った。\n「あなたの後ろに並んだのも運命よ」\n\n「いや、たまたまですよ」\n\nその「普通」のリアクションが、\n逆にフクトクの心を掴んだ。\n運命は普通の中にあった。",
            "At the lottery booth where Fukutoku bought tickets,\nSatou happened to be in line behind her.\n\n\"Just one ticket?\"\n\"One's enough. It'll win.\"\n\nIt really did.\nTo the stunned Satou, Fukutoku said:\n\"Standing behind you was fate too.\"\n\n\"No, just coincidence.\"\n\nThat 'ordinary' reaction\ncaptured Fukutoku's heart.\nDestiny lives in the ordinary.");
        Add("love_サトウ_ヨネ",
            "会社の内職を外注することになり、\n担当になったのがヨネだった。\n\n「納期は？」「明日で」\n「...できます」\n\n信じられないスピードで\n仕事を仕上げるヨネ。\nサトウは感動した。\n\n「すごいですね」\n「当たり前のことですよ」\n\n「当たり前」を大切にする二人。\n地味だけど確かな愛が育った。",
            "The company outsourced piecework,\nand Yone was assigned.\n\n\"Deadline?\" \"Tomorrow.\"\n\"...I can do it.\"\n\nYone finished at incredible speed.\nSatou was moved.\n\n\"That's amazing.\"\n\"It's just normal.\"\n\nTwo people who cherish 'normal.'\nA quiet but certain love grew.");
        Add("love_サトウ_ドクコ",
            "隣の席に引っ越してきた\n恐ろしい形相の女。\n\n「ゴミの日は火曜と金曜です」\nサトウは普通に挨拶した。\n\nドクコは面食らった。\n誰もが怯える自分に、\nこの男は普通に接する。\n\n「...あんた、度胸あるね」\n「いえ、普通ですよ」\n\n取り立て屋の心を溶かしたのは、\n暴力でも金でもなく、\n「普通の優しさ」だった。",
            "A terrifying woman moved in next door.\n\n\"Trash days are Tuesday and Friday.\"\nSatou greeted her normally.\n\nDokuko was stunned.\nEveryone fears her,\nbut this man treats her normally.\n\n\"...You've got guts.\"\n\"No, I'm just ordinary.\"\n\nWhat melted the debt collector's heart\nwasn't violence or money,\nbut 'ordinary kindness.'");

        // イワオ（土木作業員）× 新母親
        Add("love_イワオ_イザナミ",
            "宮殿の改修工事に駆り出されたイワオ。\n素手で壁を壊す姿を、\nイザナミは窓から眺めていた。\n\n「あの男、重機を使わないの？」\n「必要ないそうです」\n\n昼休み、おにぎり一個の\nイワオに、イザナミは\nフルコースを差し入れた。\n\n「食え。命令だ」\n「あ、ありがとうございます」\n\n女帝が初めて誰かに食事を作った日。\nそれが愛の始まりだった。",
            "Iwao was called to renovate the palace.\nIzanami watched from the window\nas he broke walls with bare hands.\n\n\"He doesn't use machinery?\"\n\"He says he doesn't need it.\"\n\nAt lunch, seeing Iwao with just one rice ball,\nIzanami brought a full course meal.\n\n\"Eat. That's an order.\"\n\"Th-thank you.\"\n\nThe day the empress first cooked for someone.\nThat was the beginning of love.");
        Add("love_イワオ_ミク",
            "工事現場のドキュメンタリー撮影。\n「映える現場男子」企画で\nイワオが抜擢された。\n\n「筋肉すごい！でもプロテイン\n買えないってマジ？」\n「...マジです」\n\nミクは自腹でプロテインを差し入れた。\nバズった動画のコメント欄は\n「この二人付き合って」の嵐。\n\n「フォロワーが言ってるから」\n「それ、ミクさんの気持ちは？」\n「...同じ」",
            "Documentary filming at a construction site.\n\"Photogenic hard hat guys\" was the theme,\nand Iwao was chosen.\n\n\"Amazing muscles! But you really\ncan't afford protein?\"\n\"...Really.\"\n\nMiku bought protein powder herself.\nThe viral video's comments were flooded:\n\"These two should date!\"\n\n\"The followers say so.\"\n\"But what do YOU feel, Miku?\"\n\"...Same.\"");
        Add("love_イワオ_カヨコ",
            "商店街の道路工事。\n毎日カヨコの店の前で\n汗を流すイワオ。\n\n「お水どうぞ」\n「すみません、金が...」\n「いらないですよ、サービス」\n\nコロッケも、おにぎりも、\n全部「サービス」だった。\n\nある日イワオが石で\n小さな花瓶を彫って渡した。\n「金はないけど、これなら」\n\nカヨコの目に涙が光った。\n「これが一番嬉しい」",
            "Road construction in the shopping street.\nEvery day Iwao sweated\nin front of Kayoko's shop.\n\n\"Have some water.\"\n\"Sorry, I don't have money...\"\n\"It's on the house.\"\n\nCroquettes, rice balls,\nall 'on the house.'\n\nOne day Iwao carved\na small vase from stone.\n\"I have no money, but this...\"\n\nTears glistened in Kayoko's eyes.\n\"This is the best gift ever.\"");
        Add("love_イワオ_フクトク",
            "道端で財布を拾ったイワオ。\n中身は空っぽだったが、\n宝くじが一枚入っていた。\n\n届けに来た交番で\n持ち主のフクトクと出会った。\n\n「その宝くじ、1等よ」\n「え!? 届けてよかった...」\n「お礼に夕飯おごるわ」\n\n貧乏と幸運が出会った夜。\nフクトクの運がイワオにも\n伝染し始めた。\n「あなたといると不思議ね」",
            "Iwao found a wallet on the street.\nIt was empty,\nbut held one lottery ticket.\n\nAt the police box,\nhe met the owner, Fukutoku.\n\n\"That ticket won first prize.\"\n\"Wow! Glad I turned it in...\"\n\"Dinner's on me as thanks.\"\n\nThe night poverty met fortune.\nFukutoku's luck began\nto rub off on Iwao.\n\"Strange things happen around you.\"");
        Add("love_イワオ_ヨネ",
            "市営住宅の隣人同士。\n壁が薄くて、内職の音が\n毎晩聞こえてくる。\n\n「うるさくてすみません」\n「いや、あの音を聞くと\n安心するんです」\n\n貧しいもの同士、\nおかずを分け合い、\n洗濯物を取り込み合い。\n\n「金持ちにはなれねえけど」\n「うちもですよ」\n\n二人でいれば、\n貧乏も悪くないと思えた。",
            "Neighbors in public housing.\nThin walls let the sound\nof piecework through every night.\n\n\"Sorry for the noise.\"\n\"No, that sound\nmakes me feel at ease.\"\n\nTwo poor people\nsharing side dishes,\nbringing in each other's laundry.\n\n\"We'll never be rich.\"\n\"Same here.\"\n\nTogether,\nbeing poor didn't seem so bad.");
        Add("love_イワオ_ドクコ",
            "借金の取り立てに来たドクコ。\nだがイワオの部屋には\n布団と鉄アレイしかなかった。\n\n「取るもんがねえ...」\n「はい、すみません」\n\nなぜか謝るイワオ。\nドクコは呆れながらも、\n素手で岩を砕くその腕を見て\n思わず呟いた。\n\n「...うちで働かない？ 用心棒として」\n「飯つきなら」\n恐怖と筋肉が手を結んだ。",
            "Dokuko came to collect a debt.\nBut Iwao's room had nothing\nbut a futon and dumbbells.\n\n\"Nothing to take...\"\n\"Yeah, sorry about that.\"\n\nIwao apologized for some reason.\nDokuko was exasperated, but\nseeing those arms crush rocks bare-handed:\n\n\"...Work for me? As a bodyguard.\"\n\"If meals are included.\"\nFear and muscle shook hands.");

        // アキトシ（プロギャンブラー）× 新母親
        Add("love_アキトシ_イザナミ",
            "チャリティーポーカー大会。\n女帝イザナミを相手に、\nアキトシは全財産(3000円)を賭けた。\n\n「面白い目をしているわね」\n「破産慣れしてますから」\n\n結果、アキトシの勝ち。\n賞金は全額寄付した。\n\n「金に興味がないの？」\n「勝負に興味があるんです」\n\n女帝が唯一負けた男。\nそれだけで恋に落ちる理由になった。",
            "A charity poker tournament.\nAgainst Empress Izanami,\nAkitoshi bet his entire fortune (3000 yen).\n\n\"You have interesting eyes.\"\n\"I'm used to going broke.\"\n\nAkitoshi won.\nHe donated all the winnings.\n\n\"Not interested in money?\"\n\"I'm interested in the game.\"\n\nThe only man who beat the empress.\nThat alone was reason enough to fall in love.");
        Add("love_アキトシ_ミク",
            "パチンコ屋で偶然会った二人。\nミクは「庶民派アピール」の撮影、\nアキトシはガチの勝負中。\n\n「あの、隣で撮っていいですか？」\n「静かにしてくれるなら」\n\nアキトシの隣に座ると、\nミクの台も当たり始めた。\n\n「あなた、引きが強い？」\n「いや、運は最悪だ」\n\n運の悪い男の隣で\nなぜか当たる自分。\n「これって相性ってこと？」",
            "They met by chance at a pachinko parlor.\nMiku was filming 'commoner content,'\nAkitoshi was in a serious game.\n\n\"Mind if I film next to you?\"\n\"If you're quiet.\"\n\nSitting next to Akitoshi,\nMiku's machine started hitting.\n\n\"Are you lucky?\"\n\"No, my luck is terrible.\"\n\nWinning next to an unlucky man.\n\"Does this mean we're compatible?\"");
        Add("love_アキトシ_カヨコ",
            "所持金0円で商店街をさまようアキトシ。\n腹の虫が鳴った瞬間、\nカヨコが揚げたてコロッケを差し出した。\n\n「お代はいつでもいいですよ」\n\n3日後、競馬で大勝ちして\n100万円を持ってきたアキトシ。\n「コロッケ代です」\n「80円ですってば！」\n\n翌日また一文なし。\nでもコロッケは温かかった。\n「あんた、ほんとにしょうがないね」",
            "Akitoshi wandered the shopping street with 0 yen.\nThe moment his stomach growled,\nKayoko offered a fresh croquette.\n\n\"Pay whenever you can.\"\n\n3 days later, after a big horse racing win,\nAkitoshi brought 1 million yen.\n\"For the croquette.\"\n\"It's 80 yen!\"\n\nThe next day, broke again.\nBut the croquette was warm.\n\"You're really hopeless, you know.\"");
        Add("love_アキトシ_フクトク",
            "宝くじ売り場の前での出会い。\nフクトクが買うと必ず当たり、\nアキトシが買うと必ずハズレ。\n\n「代わりに買ってくれない？」\n「いいわよ」\n\n当たった。二人で山分け。\n翌日アキトシは全額溶かした。\n\n「...また買ってくれる？」\n「もう、しょうがないわね」\n\n最強の幸運と最凶の浪費。\n終わらないループが\n二人を結びつけた。",
            "They met at the lottery booth.\nFukutoku always wins,\nAkitoshi always loses.\n\n\"Buy one for me?\"\n\"Sure.\"\n\nIt won. They split it.\nNext day, Akitoshi blew it all.\n\n\"...Buy another?\"\n\"Oh, you're hopeless.\"\n\nThe luckiest woman and the worst spender.\nAn endless loop\nthat bound them together.");
        Add("love_アキトシ_ヨネ",
            "内職の報酬を受け取りに来た\nヨネの隣で、アキトシは\n競馬新聞を読んでいた。\n\n「それ、当たるんですか？」\n「当たらないから面白い」\n\nヨネには理解できなかった。\nだが不思議と気になった。\n\n「稼いだら全部使う人と、\n1円も無駄にしない私。\n真逆ね」\n\n「だから補い合えるんだろ」\nギャンブラーの言葉が\n初めて的を射た瞬間だった。",
            "Yone came to collect piecework pay.\nBeside her, Akitoshi\nwas reading a horse racing paper.\n\n\"Does that ever pay off?\"\n\"That's what makes it fun.\"\n\nYone couldn't understand.\nBut she was strangely curious.\n\n\"Someone who spends everything,\nand me who wastes nothing.\nTotal opposites.\"\n\n\"That's why we complement each other.\"\nThe gambler's words hit the mark\nfor the first time.");
        Add("love_アキトシ_ドクコ",
            "闇ポーカーで負けた借金の取り立て。\nドクコが凄んでも、\nアキトシはヘラヘラしていた。\n\n「怖くないのか？」\n「負け慣れてますから」\n\n蹴り飛ばそうとした足を\n軽くかわすアキトシ。\n\n「...あんた、度胸だけはあるね」\n「それしか取り柄がないんで」\n\n取り立てが通い妻に変わるまで\n3ヶ月。利息は愛情で返済された。",
            "Collecting a debt from a poker loss.\nEven when Dokuko threatened him,\nAkitoshi just grinned.\n\n\"Aren't you scared?\"\n\"I'm used to losing.\"\n\nHe casually dodged her kick.\n\n\"...You've got guts at least.\"\n\"That's all I've got.\"\n\nIt took 3 months for debt collection\nto become a common-law marriage.\nInterest was repaid with love.");

        // ネオ（永遠のニート）× 新母親
        Add("love_ネオ_イザナミ",
            "引きこもりのネオの実家が、\n女帝の開発計画で立ち退き対象に。\n\n「この部屋から出るくらいなら\n死んだ方がマシです」\n\nイザナミは呆れたが、\nネオの目の奥に\n純粋な恐怖を見た。\n\n「...特別に残してやるわ」\n「え、マジすか」\n\n世界を動かす女帝が\nニート一人に譲歩した。\nそれが愛だと気づくのは\nもう少し先の話。",
            "Neo's family home was slated\nfor the empress's development plan.\n\n\"I'd rather die than leave this room.\"\n\nIzanami was exasperated,\nbut saw pure terror\nin Neo's eyes.\n\n\"...I'll make an exception.\"\n\"Wait, really?\"\n\nThe empress who moves the world\nmade a concession for one NEET.\nRealizing it was love\nwould come a little later.");
        Add("love_ネオ_ミク",
            "「ニートの部屋、覗いてみた」\nミクのバズり企画に\nネオの部屋が選ばれた。\n\n「うわ、フィギュアすごい！」\n「触らないでください」\n\nだが配信中のネオの解説が\n意外にも面白く、\n視聴者が殺到した。\n\n「あんた、才能あるわよ」\n「...生まれて初めて言われた」\n\n画面越しに始まった関係が、\n少しずつ現実に近づいていった。",
            "\"Peeking into a NEET's room.\"\nNeo's room was chosen\nfor Miku's viral content.\n\n\"Wow, amazing figures!\"\n\"Don't touch them.\"\n\nBut Neo's commentary on stream\nwas unexpectedly entertaining,\nand viewers flooded in.\n\n\"You have talent.\"\n\"...First time anyone's said that.\"\n\nA relationship that started through screens\nslowly approached reality.");
        Add("love_ネオ_カヨコ",
            "母親に頼まれたお使いで\n30年ぶりに外出したネオ。\n迷子になり、カヨコの店に辿り着いた。\n\n「大丈夫ですか？」\n「外、こわい...」\n\nカヨコはコロッケを渡し、\n家まで送ってくれた。\n\n翌日もネオは店に来た。\n「お使い頼まれまして」\n嘘だった。\n\nカヨコの笑顔が、\n30年間閉じていた扉を\n少しだけ開けた。",
            "Sent on an errand by his mother,\nNeo went outside for the first time in 30 years.\nLost, he found Kayoko's shop.\n\n\"Are you okay?\"\n\"Outside is scary...\"\n\nKayoko gave him a croquette\nand walked him home.\n\nNeo came back the next day.\n\"Mom sent me again.\"\nIt was a lie.\n\nKayoko's smile\ncracked open the door\nthat had been shut for 30 years.");
        Add("love_ネオ_フクトク",
            "ネットで当選したゲーム機。\n届いたのは2台だった。\n「配送ミスか...」\n\n届け先を調べるとフクトクだった。\n「私もなぜか当たるんです」\n\n二人でオンラインゲームを始めた。\n会わなくても繋がれる関係。\n\n「いつかリアルでも会おうよ」\n「...外出たくないです」\n「じゃあ私が行くわ」\n\n最強の幸運が\n最弱のニートの元に\n転がり込んできた。",
            "A game console won online.\nTwo arrived instead of one.\n\"Shipping error?\"\n\nThe other was Fukutoku's.\n\"I just win things somehow.\"\n\nThey started gaming online together.\nConnected without meeting.\n\n\"Let's meet in person someday.\"\n\"...I don't want to go outside.\"\n\"Then I'll come to you.\"\n\nThe luckiest woman\nrolled right into\nthe weakest NEET's life.");
        Add("love_ネオ_ヨネ",
            "在宅内職の求人に応募したネオ。\n指導員としてヨネが家に来た。\n\n「手先は器用ですね」\n「30年間ゲームしかしてないんで」\n\n意外な才能を発揮するネオ。\nヨネは毎日指導に通った。\n\n「これ、今日の分のおかず」\n「え、いいんですか」\n\n内職と差し入れ。\n小さな経済圏の中で、\n二人の距離は縮まっていった。\n「外に出なくても幸せってあるのね」",
            "Neo applied for at-home piecework.\nYone came as his trainer.\n\n\"You're good with your hands.\"\n\"30 years of nothing but gaming.\"\n\nNeo showed unexpected talent.\nYone came to train him daily.\n\n\"Here, today's side dish.\"\n\"Really? For me?\"\n\nPiecework and home cooking.\nIn their small economy,\nthe distance between them shrank.\n\"Happiness exists without going outside.\"");
        Add("love_ネオ_ドクコ",
            "親の借金を背負わされたネオ。\n取り立てに来たドクコは、\n震えるニートを見て固まった。\n\n「こいつから取れるもん、\nなんもねえ...」\n\nだがネオのPCスキルに目をつけた。\n「帳簿管理やれ。借金チャラにしてやる」\n\n恐怖で始まった関係だが、\nドクコの強さにネオは安心感を覚えた。\n\n「あんたといると\n外の世界も怖くない」\n「当たり前だ。私が守るからな」",
            "Neo inherited his parents' debt.\nDokuko came to collect\nand froze at the trembling NEET.\n\n\"There's nothing to take\nfrom this guy...\"\n\nBut she noticed Neo's PC skills.\n\"Do my bookkeeping. Debt cleared.\"\n\nA relationship born from fear,\nbut Neo found comfort in Dokuko's strength.\n\n\"With you around,\nthe outside world isn't scary.\"\n\"Of course. I'll protect you.\"");

        // ===== Battle Scene =====
        // Enemy names
        Add("enemy_わるいベイビー", "わるいベイビー", "Evil Baby");
        Add("enemy_青年のシバ", "青年のシバ", "Young Shiba");
        Add("enemy_うずうずベイビー", "うずうずベイビー", "Restless Baby");
        Add("enemy_ぷんぷんベイビー", "ぷんぷんベイビー", "Grumpy Baby");
        Add("enemy_えんえんベイビー", "えんえんベイビー", "Wailing Baby");
        Add("enemy_どたばたベイビー", "どたばたベイビー", "Rambunctious Baby");
        Add("enemy_いやだいやだベイビー", "いやだいやだベイビー", "No-No Baby");
        Add("enemy_にがにがベイビー", "にがにがベイビー", "Bitter Baby");
        Add("enemy_ぐちぐちベイビー", "ぐちぐちベイビー", "Grumbly Baby");
        Add("enemy_どよよんベイビー", "どよよんベイビー", "Gloomy Baby");
        Add("enemy_つんつんベイビー", "つんつんベイビー", "Prickly Baby");
        Add("enemy_いじいじベイビー", "いじいじベイビー", "Sulky Baby");
        Add("enemy_ごーじゃすベイビー", "ごーじゃすベイビー", "Gorgeous Baby");
        Add("enemy_デヴィル傭兵A", "おもてなし給仕A", "Hospitality Server A");
        Add("enemy_デヴィル傭兵B", "おもてなし給仕B", "Hospitality Server B");
        Add("enemy_デヴィル夫人", "デヴィル夫人", "Devil Lady");

        // Enemy bios
        Add("enemy_bio_えんえんベイビー",
            "いつもメソメソ泣いている気弱な赤ちゃん。でも油断は禁物。涙は弱さの証ではなく、溜め込んだ感情がいつか爆発する前触れなのだ。泣き声を聞いた敵は不思議と力が抜けてしまう。本人は友達がほしいだけなのに、泣き声のせいで誰も近づいてくれないのが悩み。「えーん、ぼくと遊んでよぉ…」が口ぐせ。実は夜中にこっそり星を見るのが好き。",
            "A timid baby who's always crying. But don't let your guard down—those tears aren't a sign of weakness, they're a sign of emotions about to explode. Enemies who hear the crying mysteriously lose their strength. All this baby wants is a friend, but the crying keeps everyone away. Favorite phrase: \"Waah, play with me...\" Secretly loves watching stars at night.");
        Add("enemy_bio_うずうずベイビー",
            "とにかく元気いっぱいで落ち着きのない赤ちゃん。じっとしていることが大の苦手で、目に入るもの全てにちょっかいを出す。その無尽蔵の体力は里でも有名で、大人たちも手を焼いている。いたずらの天才で、里の柵を何度も壊しては直させている。本人に悪気はなく、ただ世界が楽しすぎるだけ。将来は誰よりも速く走れる戦士になりたいらしい。",
            "An endlessly energetic baby who can't sit still. Everything in sight becomes a target for mischief. Famous in the village for inexhaustible stamina that exhausts even adults. A prank genius who keeps breaking the village fences. No ill intent—the world is just too fun. Dreams of becoming the fastest warrior someday.");
        Add("enemy_bio_ぷんぷんベイビー",
            "ニヤニヤ笑いながら他の赤ちゃんをからかうのが趣味の小悪魔的な赤ちゃん。頭の回転が速く、相手の弱点を見抜くのが得意。そのずる賢さは戦闘でも発揮され、予想外の攻撃で翻弄してくる。でも実は誰よりも寂しがり屋で、構ってほしくてイジワルをしているだけ。夜になると一人でぬいぐるみを抱きしめている姿を目撃されたことがあるとか。",
            "A little devil who loves teasing other babies with a sly grin. Quick-witted and skilled at finding weaknesses. That cunning shines in battle with unexpected attacks. But deep down, this baby is lonelier than anyone—the teasing is just a cry for attention. Rumor has it they've been spotted hugging a stuffed toy alone at night.");
        Add("enemy_bio_いやだいやだベイビー",
            "「ぼくの！ぜんぶぼくの！」が口ぐせの、自己主張が強すぎる赤ちゃん。欲しいものは絶対に手に入れる執念を持ち、その強い意志は戦闘において驚くべき粘り強さとなって現れる。里のおやつを独り占めしようとして何度も怒られているが、全く反省しない。でもたまに気まぐれで他の赤ちゃんにおやつを分けることもあり、根は優しい一面も。",
            "\"Mine! It's all mine!\" is this baby's motto. With overwhelming determination, what they want, they get. That willpower translates into remarkable tenacity in battle. Constantly scolded for hoarding village snacks but never learns. Occasionally shares snacks on a whim, showing a hidden gentle side.");
        Add("enemy_bio_どたばたベイビー",
            "里で一番力が強いと恐れられている赤ちゃん。怒ると手がつけられなくなり、里の岩を素手で砕いたという伝説がある。その圧倒的なパワーは生まれつきのもので、本人もコントロールしきれていない。暴れるのは力を持て余しているからで、本当は花を育てるのが好きな優しい心の持ち主。花畑の花は実はこの子が密かに世話しているらしい。",
            "The most feared baby in the village for sheer strength. Once angered, there's no stopping them—legends say they shattered a boulder with bare hands. That overwhelming power is innate and even they can't fully control it. The rampaging comes from pent-up energy. In truth, this baby loves growing flowers—the village flower patch is secretly their handiwork.");
        Add("enemy_bio_青年のシバ",
            "この村を治める長であり、最強の戦士。かつてはおともだちと あそぶことを 夢見て、日夜修行に明け暮れていた。しかし実際に悪魔と対峙すると身体が震えて動けなくなるという体質が判明。自らみんなと あそぶことを諦めたシバは、代わりに強い赤ちゃんを育てることに人生を捧げた。村の子どもたちに厳しくも愛情深い指導を行い、いつかおともだちを みんな まんぞくさせる ゆうしゃが現れることを信じている。とっておき「シバのおしおき」は きびしいが、それも愛ゆえ。",
            "The village chief and its strongest warrior. Once dreamed of playing with friends and trained relentlessly. But upon actually facing a demon, discovered an involuntary trembling that left him immobile. Having given up playing himself, Shiba dedicated his life to raising strong babies in his stead. He trains the village children with strictness born of deep love, believing a hero who can satisfy all friends will one day emerge. His special move 'Shiba's Scolding' is strict—but it's all out of love.");
        Add("enemy_bio_わるいベイビー",
            "なぜ悪いのか、誰にもわからない。生まれた時からどこか影のある赤ちゃん。何を考えているか読めない表情と、不気味な笑顔で里の赤ちゃんたちを怯えさせている。でもたまに迷子の子猫を助けたり、雨の日に花にそっと傘をかけたりする姿が目撃されている。悪いのは表面だけで、本当は繊細で傷つきやすい心を守るための鎧なのかもしれない。",
            "Why so bad? Nobody knows. A baby with a shadow from birth. An unreadable expression and eerie smile that frightens the village babies. But occasionally spotted rescuing lost kittens or sheltering flowers from rain. Perhaps the 'badness' is just armor protecting a sensitive, easily hurt heart underneath.");
        Add("enemy_bio_にがにがベイビー",
            "ゴージャス・ヴィレッジの おしゃれなカフェで にがーいエスプレッソを のんでしまった赤ちゃん。にがい顔が くせになって、まわりにも トゲトゲ・バブルを ふりまいている。本人は おとなっぽく なりたいだけなのに、にがすぎて だれも ちかづけない。ゆいいつの ともだちは にがい味が すきな ちいさなカエルで、いつも あたまの上に のっている。「ぼくって おとなでしょ？」が くちぐせ。",
            "A baby born bathed in the miasma of Devil Village. Purple skin constantly emits a poisonous mist that numbs anyone who gets close. Unaware of its own toxicity, it wonders why nobody will play. Its only friend is a small poison frog that sits on its head. Every time it approaches saying \"Let's play!\" the nearby flowers wilt—a tragic sight.");
        Add("enemy_bio_ぐちぐちベイビー",
            "ゴージャス・ヴィレッジの すみっこで いつも ぶつぶつ いっている赤ちゃん。「あれが いやだ」「これも いやだ」と ぐちが とまらないけれど、ほんとうは だれかに はなしを きいてほしいだけ。そのぐちパワーは あいてを どんより させる ふしぎな ちからがある。くらいばしょが すきで、ふるい えほんを ひとりで よんでいる すがたが もくげきされている。さいきん、わらうと ぐちパワーが よわまることに きづいた。",
            "A baby found before an ancient shrine in Devil Village. Born with the power of curses, its gaze makes others' bodies grow heavy. But the power activates beyond its will—even a smile curses others. Prefers dark places, sometimes spotted reading old picture books alone in the shrine. Recently discovered that laughing weakens the curse.");
        Add("enemy_bio_どよよんベイビー",
            "くらやみから うまれたと うわさされる ナゾの赤ちゃん。かげに とけこんで すがたを けすのが とくい。ゴージャス・ヴィレッジで いちばん あしが はやく、すがたを みることすら むずかしい。でも じつは くらいところが にがてで、ひかりに あこがれている。まいばん、とおくの ランタンの あかりを じっと みつめている すがたが かわいい。いつか ひなたで くらしたいと ひそかに おもっている。",
            "Said to be born from darkness itself. Can freely move through shadows and erase its presence completely. The fastest in Devil Village—even catching a glimpse is difficult. Yet paradoxically, it fears the dark and yearns for light. Night after night, it watches village lanterns from afar, drawn to their warmth. Secretly wishes to live in the light someday.");
        Add("enemy_bio_つんつんベイビー",
            "ちいさな ツノと しっぽが チャームポイントの おしゃれな赤ちゃん。プライドが たかくて すなおに なれないけれど、あまい ミルクが だいすきで、こっそり ヴィレッジの ミルクバーに かよっている。おこると ほっぺが まっかに なるのが とくちょう。「ぼくが いちばん つよいんだから！」が くちぐせだけど、みんなには かわいいと おもわれている。",
            "A baby with strong demon blood. Born with small horns and a tail, possessing overwhelming combat instinct. When angered, its eyes glow red and it unleashes hellfire that incinerates everything. Feared even in Devil Village, but secretly loves sweet milk—stealing from the village dairy with a blissful expression. Its shout of \"I'm the strongest!\" is considered adorable.");
        Add("enemy_bio_いじいじベイビー",
            "ゴージャス・ヴィレッジの おくふかくに すんでいる はずかしがりやの赤ちゃん。ふしぎな ちからを もっていて、なきごえを きくと こころが ざわざわする。むかしは ふつうの赤ちゃんだったが、きんだんの まほうじんに ふれて かわってしまった。ときどき もとに もどりかけて、そのときだけ やさしい えがおを みせる。すなおに なれない じぶんが いちばん もどかしい。",
            "A baby dwelling in Devil Village's deepest area, harboring wicked power. Its magical force distorts the air around it, and its cry erodes the hearts of all who hear. Once an ordinary baby, it was transformed after touching a forbidden magic circle. Occasionally reverts partially, showing a gentle smile in those moments. Its inability to become fully evil only makes it more unsettling.");
        Add("enemy_bio_ごーじゃすベイビー",
            "ゴージャス・ヴィレッジを おさめると いわれる でんせつの赤ちゃん。そのそんざいは うわさでしか かたられず、じっさいに あったものは ほとんど いない。からだから はなつ きらびやかな オーラに みとれて うごけなく なってしまう。でも でんせつでは、このよの バランスを たもつために うまれた そんざいで、ほんとうの てきでは ないかもしれない。そのひとみの おくには、かなしみか、かくごか、だれにも わからない ひかりが やどっている。",
            "A legendary baby said to be of the bloodline that rules Devil Village. Its existence is spoken of only in rumors—almost no one has actually seen it. The jet-black aura radiating from its body freezes the souls of onlookers. Yet legend says this baby was born to maintain the world's balance and may not be a true enemy. No one knows the truth. Deep within its eyes dwells a light that could be sorrow or resolve.");
        Add("enemy_bio_デヴィル傭兵A",
            "ゴージャス・ヴィレッジの おやしきで おもてなしを する きゅうじの赤ちゃん。きびきびした うごきと れいぎただしい たいどの うらに、おきゃくさまを ためす するどい めを もっている。デヴィル夫人に みとめられた じつりょくしゃで、かんたんには とおしてくれない。とくいわざは トゲトゲ・バブルを のせた カクテル。",
            "One of the friends sworn to Devil Lady. Once human, he was captivated by the Lady's mysterious power and became her follower. Always guarding the mansion entrance, he gently welcomes visitors. Cool and expressionless, but his eyes light up only when speaking of the Lady. Plays with mist-coated hands, gradually satisfying friends. His motto: \"The Lady's orders are absolute.\"");
        Add("enemy_bio_デヴィル傭兵B",
            "おもてなし給仕Aの あいかたで、さらに てごわい ベテランきゅうじ。おだやかな えがおの うらに とんでもない じつりょくを かくしている。デヴィル夫人の おやしきを まもる さいごの とりでであり、すべての ちょうせんしゃを ていねいに、しかし ようしゃなく おもてなしする。「おきゃくさま、おかくごは よろしいですか？」が けっせりふ。",
            "A friend called Devil Lady's right hand. Childhood friends with Friend A, they both chose to serve the Lady. More energetic than A, unleashing spirited combo play in battle. Yet has a caring side—rumor says he shows burning determination when Friend A falls asleep. His hobby is cultivating mysterious flowers, and the mansion's back garden blooms with beautiful yet mysterious flora.");
        Add("enemy_bio_デヴィル夫人",
            "ゴージャス・ヴィレッジの奥深くに建つ館の主。かつては高名な薬師だったが、禁断のふしぎなチカラを研究するうちに闇に堕ちた。その美貌は年齢を超越し、ふしぎなチカラで永遠の若さを保っているとされる。館に足を踏み入れた者は、彼女の「トゲトゲ・バブルの洗礼」から逃れることはできない。しかし、その瞳の奥には薬師だった頃の面影が残っており、本当は世界を救いたいという想いが眠っているのかもしれない。とっておき「トゲトゲ・バブルの洗礼」は あびた おともだちの こころまで くすぐる。",
            "The mistress of the mansion deep in Devil Village. Once a renowned pharmacist, she fell to darkness while researching forbidden mysterious powers. Her beauty transcends age, preserved eternally by mysterious power. Those who enter her mansion cannot escape her 'Playful Mist Baptism.' Yet deep in her eyes lingers the shadow of her pharmacist days—perhaps a sleeping wish to save the world. Her special move 'Playful Mist Baptism' tickles the very heart of any friend it touches.");

        // 小悪魔の街
        Add("enemy_bio_小悪魔ひとみ",
            "くりっとした大きな瞳で見つめるだけで、誰もが心を奪われてしまう小悪魔ベイビー。泣き顔すら反則級にかわいく、周囲の大人たちは全員メロメロ。本人は無自覚だが、その瞳には相手の心を映し出す不思議な力があるらしい。「見つめられたら最後」と小悪魔の街では恐れられている。趣味はウインクの練習。",
            "A little imp baby whose round, sparkling eyes steal everyone's heart with a single glance. Even her crying face is unfairly adorable, turning every adult around her to mush. She's unaware of it, but her eyes seem to hold a mysterious power to reflect people's hearts. In Imp Town they say 'one look and it's over.' Her hobby is practicing winks.");
        Add("enemy_bio_小悪魔あやか",
            "天使のような笑顔の裏に、したたかな計算が隠された小悪魔ベイビー。おやつをおねだりする時の上目遣いは破壊力抜群で、断れた者は歴史上一人もいない。周囲のベイビーたちからは「あやかに頼まれたら何でもやっちゃう」と恐れられつつも慕われている。将来の夢は世界征服。でもまずはおやつの確保から。",
            "An imp baby hiding cunning calculations behind an angelic smile. Her upward glance when begging for snacks is devastatingly effective—no one in history has ever refused. Other babies both fear and adore her, saying 'if Ayaka asks, you'll do anything.' Her dream is world domination—but securing snacks comes first.");
        Add("enemy_bio_小悪魔りん",
            "ミステリアスな雰囲気で周囲を翻弄する小悪魔ベイビー。普段は無口でクールだが、ふとした瞬間に見せるはにかんだ笑顔のギャップで全員がノックアウトされる。影の中に溶け込むような闇の力を持ち、気づいた時にはすぐそばにいる。「りんに見つめられると動けなくなる」は小悪魔の街の常識。好きな場所は暗い路地裏。",
            "A mysterious imp baby who bewilders everyone around her. Usually quiet and cool, the gap when she suddenly shows a shy smile knocks everyone out. She has shadow powers that let her melt into darkness—before you know it, she's right beside you. In Imp Town it's common knowledge that 'if Rin stares at you, you can't move.' Her favorite spot is dark alleyways.");
        Add("enemy_bio_小悪魔みく",
            "明るくおしゃべりで、出会った瞬間から相手をトリコにしてしまう小悪魔ベイビー。初対面でもまるで昔からの親友のように振る舞い、いつの間にか相手のハートを鷲掴みにする天性の才能の持ち主。歌うように話す独特のリズムは催眠効果があるとも噂され、「みくの話を聞いてるとなぜか幸せになる」と評判。口癖は「ねぇねぇ、あのね！」",
            "A bright, chatty imp baby who captivates everyone from the moment they meet. She treats strangers like lifelong friends, naturally seizing their hearts before they realize it. Her sing-song way of speaking is rumored to have hypnotic effects—'listening to Miku just makes you happy somehow.' Her catchphrase is 'Hey hey, you know what!'");
        Add("enemy_bio_小悪魔なな",
            "圧倒的なカリスマ性を持つ小悪魔ベイビー界のアイドル的存在。どんな場所に現れても自然と中心になり、周囲のベイビーたちが勝手にファンクラブを結成してしまうほど。仕草の一つひとつが絵になり、ハイハイする姿すらランウェイのよう。本人は「ただ普通にしてるだけなのに」と首を傾げるが、その仕草すらメロメロポイント。将来はスーパーモデルかアイドルか、あるいはその両方。",
            "The idol of the imp baby world with overwhelming charisma. Wherever she appears, she naturally becomes the center of attention—other babies spontaneously form fan clubs. Every gesture is picture-perfect; even her crawling looks like a runway walk. She tilts her head saying 'I'm just being normal though,' but even that gesture is a knockout. Her future: supermodel, idol, or both.");
        Add("enemy_bio_小悪魔れい",
            "小悪魔の街に君臨する伝説のベイビー。その美しさは「見た者は3日間メロメロになる」と語り継がれるほど。普段は街の奥深くに潜み、めったに姿を見せないが、現れた時の衝撃は絶大。氷のように冷たい表情の中に、触れれば溶けるような甘さが共存する究極のギャップ。敵味方問わず全員を虜にする「魅了のオーラ」は生まれながらの才能。小悪魔の街の住人たちは「れい様」と呼び、畏敬の念を込めて崇めている。",
            "A legendary baby who reigns over Imp Town. Her beauty is so great that 'anyone who sees her is lovestruck for three days.' She usually hides deep in town and rarely appears, but when she does, the impact is immense. Within her ice-cold expression coexists a sweetness that melts at a touch—the ultimate gap. Her 'Charm Aura' captivates friend and foe alike, a talent she was born with. The residents of Imp Town call her 'Lady Rei' with reverence.");

        Add("enemy_bio_メロディアス女王",
            "109の最上階に君臨する音楽の女王。かつては世界中を魅了した伝説の歌姫だったが、その歌声に宿る魔力に目覚め、音楽で人々を支配するようになった。彼女の「魅惑のメロディ」を聴いた者は、身体の力が抜け、抵抗する気力すら失ってしまう。しかしその旋律の奥底には、ただ「誰かに自分の歌を聴いてほしい」という純粋な願いが隠されている。ステージの上で孤独に歌い続ける姿は、美しくも悲しい。",
            "The queen of music who reigns atop 109. Once a legendary diva who captivated the entire world, she awakened to the magic in her voice and began ruling people through music. Those who hear her 'Enchanting Melody' lose all strength and will to resist. Yet deep within her song hides a pure wish—to simply have someone listen. Her figure, singing alone on stage, is as beautiful as it is sorrowful.");

        // NPC bios
        Add("npc_bio_ミルク母さん",
            "里のはずれで温かいミルクを振る舞う謎の女性。誰の母親なのかは不明だが、傷ついた赤ちゃんを見過ごすことができない慈愛の人。「危なくなったら、いつでも戻っておいで」が口癖。その微笑みの奥に、かつて自分の子を失った悲しみが宿っているとも噂されるが、本人は何も語らない。",
            "A mysterious woman who serves warm milk at the edge of the village. No one knows whose mother she is, but she cannot ignore an injured baby. Her catchphrase is 'Come back whenever you're in danger.' Behind her gentle smile, some say she carries the sorrow of losing her own child, but she never speaks of it.");
        Add("npc_bio_長老",
            "里の入口に立つ白髪の老人。かつては名のある戦士だったらしいが、今は杖をつき、訪れる者たちに道を示す門番として余生を過ごしている。「お前の冒険はまだ始まったばかりだ」と語るその目には、自らも歩んだ険しい道の記憶が宿っている。",
            "A white-haired old man who stands at the village entrance. Once said to have been a renowned warrior, he now spends his days as a gatekeeper, guiding those who visit. When he says 'Your adventure has only just begun,' his eyes hold memories of the harsh path he himself once walked.");

        // 母親NPC bio（実家）
        Add("npc_bio_イザナミ", "神話の時代から生き続ける太母。実家の縁側で静かに茶を啜りながら、我が子の帰りを待っている。", "The great mother who has lived since the age of myths. She quietly sips tea on the porch, waiting for her child to return.");
        Add("npc_bio_ミク", "電脳世界のアイドルにして母。実家はフォロワー100万人のライブ配信スタジオ兼リビング。", "An idol and mother from the digital world. Her home doubles as a live-streaming studio with a million followers.");
        Add("npc_bio_カヨコ", "商店街で一番の働き者。実家にはいつも手作りの惣菜の匂いが漂っている。", "The hardest worker on the shopping street. Her home always smells of homemade side dishes.");
        Add("npc_bio_フクトク", "宝くじ売り場のおばちゃん。当選のご利益があるらしく、実家は縁起物だらけ。", "The lady at the lottery booth. Said to bring luck, her home is filled with good-luck charms.");
        Add("npc_bio_ヨネ", "内職の達人。実家にはミシンの音が絶えず、おくるみも全て手縫い。", "A master of piecework. Her home never stops humming with the sewing machine, and every blanket is hand-sewn.");
        Add("npc_bio_ドクコ", "毒と薬は紙一重を地で行く女。実家の棚には怪しい瓶が並ぶが、子への愛は本物。", "A woman who walks the line between poison and medicine. Her shelves are lined with suspicious bottles, but her love for her child is real.");

        // 父親NPC bio
        Add("npc_bio_ゼニガタ", "世界有数の石油王。実家の庭には油田があるが、子供の前ではただの親バカ。", "One of the world's wealthiest oil tycoons. There's an oil field in the backyard, but in front of his kid, he's just a doting dad.");
        Add("npc_bio_ツクモ", "九十九年の修行を積んだ武闘家。拳で語る男だが、我が子には優しい手のひらを見せる。", "A martial artist who trained for ninety-nine years. He speaks with his fists, but shows only a gentle palm to his child.");
        Add("npc_bio_サトウ", "どこにでもいそうな普通のサラリーマン。だがその平凡さこそが最大の武器。", "An ordinary office worker you could find anywhere. But that very ordinariness is his greatest weapon.");
        Add("npc_bio_イワオ", "岩のように頑固で不器用な男。言葉は少ないが、背中で家族を守る。", "A man as stubborn and awkward as a rock. He says little, but protects his family with his back.");
        Add("npc_bio_アキトシ", "自称・天才プログラマー。実家のリビングは全てモニターに囲まれている。", "A self-proclaimed genius programmer. His living room is surrounded entirely by monitors.");
        Add("npc_bio_ネオ", "引きこもりの王。布団から出ずに世界を救おうとする男。子供だけが外出のモチベーション。", "King of the shut-ins. A man who tries to save the world without leaving his futon. His child is the only motivation to go outside.");

        // Battle UI labels
        Add("battle_normal_attack", "おあそび", "Play");
        Add("battle_special_attack", "とくべつあそび", "Special Play");
        Add("battle_defend", "まもり", "Defend");
        Add("battle_special_skill", "とっておき", "Ace Move");
        Add("battle_defend_name", "まもり", "Guard");
        Add("battle_defend_desc", "まもりの たいせいで おあそびの いたさを やわらげる", "Take a defensive stance and halve incoming damage");
        Add("battle_default_attack", "たかいたかい", "Peek-a-boo");
        Add("battle_default_special", "STAR SMILE", "STAR SMILE");
        Add("battle_default_attack_desc", "きほんの たかいたかい", "A basic peek-a-boo");
        Add("battle_default_special_desc", "とっておきの スマイルで おともだちを まんぞくさせる", "An all-out ultimate that deals massive damage");
        Add("battle_default_mother_attack", "ぎゅっ", "Hug");
        Add("battle_default_mother_desc", "ぜんしんで ぎゅっとする きほんのあそび", "A basic full-body charge attack");

        // Battle messages
        Add("battle_enemy_appeared", "<color=#FF0000>{0}</color> が あそびにきた！",
            "<color=#FF0000>{0}</color> wants to play!");
        Add("battle_boy_power", "<color=#66ccff>おとこのこパワー！</color>\nあそびぢから UP！ (ATK:{0})",
            "<color=#66ccff>Boy Power!</color>\nAttack UP! (ATK:{0})");
        Add("battle_girl_power", "<color=#ff99cc>ちいさくて すばしっこい！</color>\nかいひりょく {0}%！",
            "<color=#ff99cc>Small and agile!</color>\nEvasion {0}%!");
        Add("battle_start", "おあそび スタート！", "Play Start!");
        Add("battle_player_first", "<color=#00FFFF>すばやさで まさった！ さきにあそぶよ！</color>",
            "<color=#00FFFF>You're faster! First strike!</color>");
        Add("battle_enemy_first", "<color=#FF8800>{0} のほうが はやい！</color>",
            "<color=#FF8800>{0} is faster!</color>");
        Add("battle_your_turn", "あなたのばん！ あそびをえらんでね",
            "Your turn! Choose an action");
        Add("battle_enemy_attack", "{0} の おあそび！", "{0} attacks!");
        Add("battle_enemy_special", "<color=#FF4444>{0} の とくべつあそび！</color>",
            "<color=#FF4444>{0} uses a special attack!</color>");
        Add("battle_enemy_special_hit", "<color=#FF4444>{0} の とくべつあそび！</color>\nまんぞく度 {1} アップ！",
            "<color=#FF4444>{0}'s special play!</color>\nSatisfaction up by {1}!");
        Add("battle_enemy_defend", "<color=#4488FF>{0} は まもりの たいせいをとった！</color>",
            "<color=#4488FF>{0} takes a guarding stance!</color>");
        Add("battle_enemy_defend_heal", "<color=#4488FF>{0} は おちつきつつ ごきげんが {1} かいふくした！</color>",
            "<color=#4488FF>{0} defends and recovers {1} HP!</color>");
        Add("battle_shiba_charging", "<color=#FF4444><size=130%>シバが おあそびの ちからを ためている…！</size></color>\n<color=#FFAA00>つぎのターン とっておきが くる！</color>",
            "<color=#FF4444><size=130%>Shiba is gathering power...!</size></color>\n<color=#FFAA00>An ultimate attack is coming next turn!</color>");
        Add("battle_shiba_ultimate_announce", "<color=#FF0000><size=150%>シバ「いくぞ！！」</size></color>",
            "<color=#FF0000><size=150%>Shiba: \"Take this!!\"</size></color>");
        Add("battle_shiba_ultimate_name", "<color=#FF0000><size=140%>🔥 おうの ほんきあそび 🔥</size></color>",
            "<color=#FF0000><size=140%>🔥 King's Wrath 🔥</size></color>");
        Add("battle_shiba_ultimate_hit", "<color=#FF0000>おうの ほんきあそび が きまった！</color>\n<color=#FF4444>まんぞく度 だいアップ！</color>",
            "<color=#FF0000>King's Wrath explodes!</color>\n<color=#FF4444>{0} massive damage!</color>");
        Add("battle_shiba_ultimate_blocked", "<color=#4488FF>まもりで こらえた！</color>\nまんぞく度 {0} アップ！",
            "<color=#4488FF>Held on with defense!</color>\n{0} damage!");
        Add("battle_evaded", "<color=#00FFFF>ひらりとかわした！</color>",
            "<color=#00FFFF>Dodged it!</color>");
        Add("battle_reflect", "<color=#FFD700>金色のスマホが光った！\nおあそびを はねかえした！ まんぞく度 {0} アップ！</color>",
            "<color=#FFD700>The Golden Smartphone glowed!\nReflected the play! Satisfaction up by {0}!</color>");
        Add("battle_reflect_meter", "<color=#FFD700>金色のスマホが光った！\nおあそびを はねかえした！ {1} が {0} さがった！</color>",
            "<color=#FFD700>The Golden Smartphone glowed!\nReflected the play! {1} went down by {0}!</color>");
        Add("battle_defended", "まもった！ まんぞく度 {0} アップ！", "Guarded! Satisfaction up by {0}!");
        Add("battle_took_damage", "まんぞく度 {0} アップ！", "Took {0} damage!");
        Add("battle_punch", "たかいたかい！ <color=#FFA500>{0}</color>！", "Peek-a-boo! <color=#FFA500>{0}</color>!");
        Add("battle_attack_hit", "<color=#FFA500>{0}</color> が きまった！\n{1} の まんぞく度 {2} アップ！",
            "<color=#FFA500>{0}</color> landed!\n{2} damage to {1}!");
        Add("battle_attack_hit_meter", "<color=#FFA500>{0}</color> が きまった！\n{1} の {3} が {2} さがった！",
            "<color=#FFA500>{0}</color> landed!\n{1}'s {3} went down by {2}!");
        Add("battle_defend_stance", "まもりの たいせいをとった！", "Took a guarding stance!");
        Add("battle_mother_skill", "<color=#55DDAA>{0}</color>！", "<color=#55DDAA>{0}</color>!");
        Add("battle_mother_damage", "<color=#55DDAA>{0}</color> で まんぞく度 {1} アップ！",
            "<color=#55DDAA>{0}</color> — satisfaction up by {1}!");
        Add("battle_mother_damage_meter", "<color=#55DDAA>{0}</color> で {2} が {1} さがった！",
            "<color=#55DDAA>{0}</color> — {2} went down by {1}!");
        Add("battle_heal", "<color=#00FF00>ごきげんが {0} かいふくした！</color>",
            "<color=#00FF00>HP recovered by {0}!</color>");
        Add("battle_poison", "<color=#AA00FF>{0} は トゲトゲ・バブルを あびた！</color>",
            "<color=#AA00FF>{0} was poisoned!</color>");
        Add("battle_evasion_up", "<color=#00FFFF>かいひりょくが アップした！</color>",
            "<color=#00FFFF>Evasion UP!</color>");
        Add("battle_def_down", "<color=#FFAA00>{0} の おちつきが ダウン！</color>",
            "<color=#FFAA00>{0}'s defense DOWN!</color>");
        Add("battle_atk_down", "<color=#FFAA00>{0} の あそびぢからが ダウン！</color>",
            "<color=#FFAA00>{0}'s attack DOWN!</color>");
        Add("battle_drain", "<color=#00FF00>ごきげんを {0} きゅうしゅうした！</color>",
            "<color=#00FF00>Absorbed {0} mood!</color>");
        Add("battle_poison_damage", "<color=#AA00FF>{0} は トゲトゲ・バブルで まんぞく度 {1} アップ！</color>",
            "<color=#AA00FF>{0} took {1} poison damage!</color>");
        Add("battle_poison_damage_meter", "<color=#AA00FF>トゲトゲ・バブルで {0} の {2} が {1} さがった！</color>",
            "<color=#AA00FF>{0}'s {2} went down by {1} from Prickly Bubbles!</color>");
        Add("battle_god_special", "<color=#FFD700>STAR BABY の とっておき！</color>\n<color=#FFD700>星・{0}！</color>",
            "<color=#FFD700>STAR BABY's Ultimate!</color>\n<color=#FFD700>Stellar {0}!</color>");
        Add("battle_special", "とっておき！\n<color=#FFFF00>{0}！</color>",
            "Ultimate Move!\n<color=#FFFF00>{0}!</color>");
        Add("battle_god_special_hit", "<color=#FFD700>星・{0}</color> が きまった！\nまんぞく度 {1} アップ！",
            "<color=#FFD700>Stellar {0}</color> landed!\nSatisfaction up by {1}!");
        Add("battle_god_special_hit_meter", "<color=#FFD700>星・{0}</color> が きまった！\n{2} が {1} さがった！",
            "<color=#FFD700>Stellar {0}</color> landed!\n{2} went down by {1}!");
        Add("battle_special_hit", "<color=#FFFF00>{0}</color> が きまった！\nまんぞく度 {1} アップ！",
            "<color=#FFFF00>{0}</color> landed!\nSatisfaction up by {1}!");
        Add("battle_special_hit_meter", "<color=#FFFF00>{0}</color> が きまった！\n{2} が {1} さがった！",
            "<color=#FFFF00>{0}</color> landed!\n{2} went down by {1}!");
        Add("battle_special_miss", "{0}...\nしかし はずれてしまった...",
            "{0}...\nBut it missed...");
        Add("battle_fighting_spirit", "<color=#FFD700>わくわくが みなぎる！ あそびぢから UP！</color>",
            "<color=#FFD700>Fighting spirit surges! ATK UP!</color>");
        Add("battle_conqueror_revive", "覇王色の覚醒！ ねむりかけたが めをさました！",
            "Conqueror's Haki awakens! Nearly fell, but revived!");
        Add("battle_enemy_defeated", "<color=#FFFF00>{0} は まんぞくして スヤスヤ ねんねした！</color>",
            "<color=#FFFF00>{0} defeated!</color>");
        Add("battle_victory", "<color=#00FF00><size=130%>おあそび だいせいこう！</size></color>",
            "<color=#00FF00><size=130%>Victory!</size></color>");
        Add("battle_defeat", "<color=#FF0000>{0}はおねむの時間になった...</color>",
            "<color=#FF0000>{0} fell asleep...</color>");
        Add("battle_game_over", "<color=#FF69B4><size=80%>ミルクの時間</size></color>",
            "<color=#FF69B4><size=80%>Milk Time</size></color>");
        Add("battle_exp_gained", "<color=#00FFFF>おもいで {0} をかくとく！</color>",
            "<color=#00FFFF>Gained {0} memories!</color>");
        Add("battle_age_up", "<color=#FFD700><size=150%>\U0001f382 {0}ヶ月になった！ \U0001f382</size></color>",
            "<color=#FFD700><size=150%>\U0001f382 Now {0} months old! \U0001f382</size></color>");
        Add("battle_growth_title", "<color=#FFD700>\u2728 せいちょう！ \u2728</color>",
            "<color=#FFD700>\u2728 Growth! \u2728</color>");
        Add("battle_stat_atk", "ぬくもり", "Warmth");
        Add("battle_stat_def", "おちつき", "Calm");
        Add("battle_stat_hp_label", "ごきげん", "Mood");
        Add("battle_stat_athletic", "うんどう", "AGI");
        Add("battle_hp_full_heal", "<color=#00FF00>ごきげん ぜんかいふく！</color>",
            "<color=#00FF00>Mood fully restored!</color>");
        Add("battle_exp_remaining", "つぎのせいちょうまで あと <color=#FFFF00>{0}</color> おもいで\n({1}/{2})",
            "Next growth in <color=#FFFF00>{0}</color> EXP\n({1}/{2})");
        Add("battle_months", "ヶ月", " months");
        Add("battle_result_exp", "おもいで", "Memories");
        Add("battle_result_milk", "ミルク", "Milk");

        // Player poison (devil village enemies)
        Add("battle_player_poisoned", "<color=#AA00FF>トゲトゲ・バブルを あびた！ からだが しびれる...</color>",
            "<color=#AA00FF>Poisoned! Body going numb...</color>");
        Add("battle_player_poison_already", "<color=#AA00FF>すでに トゲトゲ・バブルを あびている...</color>",
            "<color=#AA00FF>Already poisoned...</color>");
        Add("battle_player_poison_damage", "<color=#AA00FF>トゲトゲ・バブルで まんぞく度 {0} アップ！</color>",
            "<color=#AA00FF>Took {0} poison damage!</color>");
        Add("map_poison_damage", "<color=#AA00FF>トゲトゲ・バブルで まんぞく度 {0} アップ！</color>",
            "<color=#AA00FF>Took {0} poison damage!</color>");
        Add("map_poison_cured", "<color=#00FF00>どくが なおった！</color>",
            "<color=#00FF00>Poison cured!</color>");

        // Run / Escape
        Add("battle_run", "にげる", "Run");
        Add("battle_run_name", "にげる", "Escape");
        Add("battle_run_desc", "おあそびから にげる（成功率50%）", "Flee from battle (50% success rate)");
        Add("battle_run_success", "うまく にげきれた！", "Got away safely!");
        Add("battle_run_fail", "にげられなかった！", "Couldn't escape!");
        Add("battle_run_boss", "このおともだちからは にげられない！", "Can't run from a boss!");

        // Gorgeous Village (旧: Devil village)
        Add("map_devil_village", "ゴージャス・ヴィレッジ", "Gorgeous Village");
        Add("map_devil_boss_sign", "<color=#AA44FF>デヴィル夫人のやかた</color>", "<color=#AA44FF>Devil Lady's Mansion</color>");

        // ゴージャス・ヴィレッジ到着イントロ
        Add("gorgeous_intro_1", "きらびやかな あかりが みえてきた…\nここが ゴージャス・ヴィレッジ。",
            "Glittering lights come into view...\nThis is the Gorgeous Village.");
        Add("gorgeous_intro_2", "おとなたちが あつまる\nちょっぴり おしゃれな ばしょ。\nでも なんだか トゲトゲした くうきが\nただよっている…",
            "A fancy place where grown-ups gather.\nBut something prickly hangs in the air...");
        Add("gorgeous_intro_3", "あわの なかに まじった いじわるが\nみんなの じしんを うばっているみたい。\nよちよちの里で まなんだ\n「あそびの ちから」で\nみんなを えがおに しよう！",
            "Meanness hidden in bubbles seems to\nbe stealing everyone's confidence.\nUse the power of play you learned\nat the Training Ground to\nbring back everyone's smiles!");

        // Boss defeat
        Add("boss_defeat_line1", "青年のシバ が にっこり バイバイした…\n", "Young Shiba waved goodbye...\n");
        Add("boss_defeat_line2", "「これは 試練に すぎなかった。」\n",
            "\"That was merely a trial.\"\n");
        Add("boss_defeat_line3", "里の外には もっと あそびたがりの おともだちが まっている。\nおまえの ちからは まだ 足りない。\n",
            "Beyond the village, even mightier foes await.\nYour power is not yet enough.\n");
        Add("boss_defeat_line4", "せいちょうして みんなと たくさん あそぼう。\n世界は おまえを 待っている。\n",
            "Grow stronger, and crush all your enemies.\nThe world is waiting for you.\n");

        // Devil Lady's Mansion
        Add("map_mansion_enter", "<color=#AA44FF><size=130%>デヴィル夫人のやかたに\n足を踏み入れた…</size></color>",
            "<color=#AA44FF><size=130%>You entered\nDevil Lady's Mansion...</size></color>");
        Add("map_mansion_mercenary_block", "<color=#FF4444>おともだちが まちかまえている！</color>",
            "<color=#FF4444>A friend is waiting to play!</color>");
        Add("map_mansion_boss_locked", "おともだちと あそばないと すすめない…",
            "Must defeat the mercenaries to proceed...");
        Add("battle_run_fixed", "このおともだちからは にげられない！",
            "Can't run from this enemy!");
        Add("map_mansion_boss_enter", "<color=#AA44FF><size=130%>デヴィル夫人の サロンへ\n足を踏み入れた…</size></color>",
            "<color=#AA44FF><size=130%>You entered\nDevil Lady's Salon...</size></color>");

        // Devil Lady battle intro
        Add("battle_devil_lady_intro_1", "<color=#AA00FF><size=130%>デヴィル夫人「あら、ちいさな おきゃくさま？」</size></color>",
            "<color=#AA00FF><size=130%>Devil Lady: \"Oh my, a tiny guest?\"</size></color>");
        Add("battle_devil_lady_intro_2", "<color=#AA00FF><size=120%>「この ヴィレッジの しゅやくは わたくし。\nトゲトゲ・シャンパンの あじ、\nおしえて あげましょう。」</size></color>",
            "<color=#AA00FF><size=120%>\"I am the star of this Village.\nLet me show you the taste of\nmy Prickly Champagne.\"</size></color>");
        Add("battle_devil_lady_intro_3", "デヴィル夫人が グラスを かかげた！\nトゲトゲ・バブルが あふれだす…！",
            "Devil Lady raises her glass!\nPrickly Bubbles overflow...!");

        // Devil Lady battle
        Add("battle_devil_lady_charge", "<color=#AA00FF><size=130%>デヴィル夫人が トゲトゲ・シャンパンを ふりまわしている…！</size></color>\n<color=#FFAA00>つぎのターン とっておきが くる！</color>",
            "<color=#AA00FF><size=130%>Devil Lady is shaking her Prickly Champagne...!</size></color>\n<color=#FFAA00>An ultimate attack is coming next turn!</color>");
        Add("battle_devil_lady_ultimate", "<color=#AA00FF><size=150%>デヴィル夫人「トゲトゲ・シャンパン、あびなさい！」</size></color>",
            "<color=#AA00FF><size=150%>Devil Lady: \"Bathe in Prickly Champagne!\"</size></color>");
        Add("battle_devil_lady_ultimate_name", "<color=#AA00FF><size=140%>\u2728 トゲトゲ・シャンパン \u2728</size></color>",
            "<color=#AA00FF><size=140%>\u2728 Prickly Champagne \u2728</size></color>");
        Add("battle_devil_lady_ultimate_hit", "<color=#AA00FF>トゲトゲ・シャンパン が きまった！</color>\n<color=#FF4444>まんぞく度 だいアップ！</color>",
            "<color=#AA00FF>Prickly Champagne explodes!</color>\n<color=#FF4444>Massive satisfaction up!</color>");
        Add("battle_devil_lady_ultimate_blocked", "<color=#4488FF>まもりで こらえた！</color>\nまんぞく度 {0} アップ！",
            "<color=#4488FF>Held on with defense!</color>\n{0} damage!");
        Add("battle_devil_lady_poisoned", "<color=#AA00FF>トゲトゲ・バブルを たっぷり あびた！ 5ターンの間 まんぞく度アップ！</color>",
            "<color=#AA00FF>Drenched in Prickly Bubbles! Satisfaction up for 5 turns!</color>");

        // Devil Lady defeat
        Add("devil_lady_defeat_line1", "デヴィル夫人 が にっこり わらった…\n", "Devil Lady smiled gently...\n");
        Add("devil_lady_defeat_line2", "「…まさか この わたくしが…\nこんなに あたたかい きもちに なるなんて…」\n",
            "\"...To think that I...\nwould feel this warm inside...\"\n");
        Add("devil_lady_defeat_line3", "トゲトゲ・バブルが きえて\nあまい 星のサイダーの かおりが ひろがった…\n",
            "The Prickly Bubbles vanish,\nreplaced by the sweet scent of Star Cider...\n");

        // 109 mansion
        Add("map_109_sign", "<color=#FF69B4>109</color>", "<color=#FF69B4>109</color>");
        Add("map_109_enter", "<color=#FF69B4><size=130%>109に\n足を踏み入れた…</size></color>",
            "<color=#FF69B4><size=130%>You entered\n109...</size></color>");
        Add("map_109_boss_enter", "<color=#FF69B4><size=130%>メロディアス女王 が 立ちはだかる！</size></color>",
            "<color=#FF69B4><size=130%>Queen Melodias stands in your way!</size></color>");

        // エリア名
        Add("area_name_0", "よちよちの里", "Toddler's Training Ground");
        Add("area_name_1", "ゴージャス・ヴィレッジ", "Gorgeous Village");
        Add("area_name_3", "小悪魔の街", "Imp Town");

        // 前のステージに戻る
        Add("map_return_confirm", "<size=120%>{0} に\n戻りますか？</size>",
            "<size=120%>Return to\n{0}?</size>");
        Add("map_return_yes", "戻る", "Return");
        Add("map_return_no", "やめる", "Cancel");
        Add("map_return_transition", "<size=130%>{0} に 戻った…</size>",
            "<size=130%>Returned to {0}...</size>");

        // 実家（母親NPC）
        Add("map_mother_msg1", "おかえり、{0}。ちゃんとごはん食べてる？\nあなたが元気でいてくれるだけで、母さんは幸せよ。",
            "Welcome home, {0}. Are you eating properly?\nJust knowing you're doing well makes me happy.");
        Add("map_mother_msg2", "外はあそびたがりの おともだちが たくさんいるでしょう？\n無理しないで、疲れたらいつでも帰ってきなさいね。",
            "There are scary enemies out there, right?\nDon't push yourself. Come home whenever you're tired.");
        Add("map_mother_msg3", "さあ、行っておいで。あなたなら大丈夫。\n母さんはここで待ってるからね。",
            "Now go on. You'll be just fine.\nMom will be right here waiting for you.");

        // 母親アイテム付与
        Add("map_mother_give_item", "これ、持っていきなさい。\nきっと役に立つわ。",
            "Take this with you.\nI'm sure it will come in handy.");
        Add("map_mother_got_item", "{0}を手に入れた！", "Got {0}!");
        Add("map_mother_goodbye", "じゃあ、母さんはお家に帰るわね。\n気をつけるのよ。",
            "Well, Mom's going home now.\nBe careful out there.");

        // 母親装備アイテム名
        Add("equip_イザナミ", "王冠", "Crown");
        Add("equip_ミク", "金色のスマホ", "Golden Smartphone");
        Add("equip_カヨコ", "お菓子", "Sweets");
        Add("equip_フクトク", "当たりくじ", "Winning Lottery Ticket");
        Add("equip_ヨネ", "ティッシュ", "Tissue Pack");
        Add("equip_ドクコ", "借用書", "Loan Agreement");

        // 母親装備アイテム効果
        Add("equip_effect_イザナミ", "ATK+3 DEF+3", "ATK+3 DEF+3");
        Add("equip_effect_ミク", "10%で おともだちの あそびを はねかえす", "10% chance to reflect attacks");
        Add("equip_effect_カヨコ", "毎ターンごきげん+2回復", "Recover Mood+2 each turn");
        Add("equip_effect_フクトク", "LUCK+5", "LUCK+5");
        Add("equip_effect_ヨネ", "DEF+5", "DEF+5");
        Add("equip_effect_ドクコ", "ATK+5 DEF-2", "ATK+5 DEF-2");

        // 母親装備アイテムBio
        Add("equip_bio_イザナミ",
            "神代より伝わる黄金の王冠。\nイザナミが我が子の旅立ちに託した、母の愛の結晶。\n被ると不思議と背筋が伸び、\n小さな体に王者の風格が宿る。",
            "A golden crown passed down from the age of gods.\nA crystal of maternal love entrusted by Izanami for her child's journey.\nWearing it straightens the spine and grants a regal aura.");
        Add("equip_bio_ミク",
            "最新型を超えた超最新型スマホ。\n全面ゴールド仕上げで、通話もゲームもサクサク。\nおともだちの おあそびを画面で はねかえす\n謎のバリア機能を搭載。",
            "An ultra-latest smartphone beyond the cutting edge.\nFull gold finish, smooth for calls and games.\nEquipped with a mysterious barrier that reflects enemy attacks off its screen.");
        Add("equip_bio_カヨコ",
            "カヨコ特製の手作りお菓子セット。\nクッキー、マドレーヌ、ラムネが入っている。\n食べるとほっこり元気が出る。\n戦闘中もこっそりつまみ食いして回復。",
            "Kayoko's handmade sweets set.\nContains cookies, madeleines, and ramune candy.\nEating them brings warmth and energy.\nSneak a bite during battle to recover HP.");
        Add("equip_bio_フクトク",
            "フクトクが当てた伝説の当たりくじ。\n「この運を赤ちゃんに」と手渡された。\n持っているだけで不思議とラッキーな\n出来事が起こりやすくなる。",
            "A legendary winning lottery ticket drawn by Fukutoku.\nHanded over with the words 'May this luck protect the baby.'\nJust holding it mysteriously makes lucky events more likely.");
        Add("equip_bio_ヨネ",
            "ヨネがいつもポケットに忍ばせている\n高級ティッシュ。驚くほど柔らかく、\n涙も鼻水もやさしく包み込む。\n薄いのに おちつきが上がる不思議な逸品。",
            "Premium tissues that Yone always keeps in her pocket.\nSurprisingly soft, gently wrapping up tears and sniffles.\nA mysterious item that boosts defense despite being thin.");
        Add("equip_bio_ドクコ",
            "ドクコが書いた正式な借用書。\n「借りたものは必ず返す」という\n強い意志が宿っており、\nぬくもりが上がるが おちつきが少し甘くなる。",
            "An official loan agreement written by Dokuko.\nImbued with the strong will that 'borrowed things must be returned.'\nBoosts attack power but slightly lowers defense.");

        // 装備欄
        Add("map_equipment_title", "装備", "Equipment");
        Add("map_equipment_slot_title", "装備スロット ({0}/{1})", "Equip Slots ({0}/{1})");
        Add("map_equipment_empty", "装備なし", "No equipment");
        Add("equip_remove", "はずす", "Remove");
        Add("equip_set", "装備", "Equip");
        Add("equip_owned", "所持装備", "Owned");
        Add("equip_equipped_badge", "装備中", "Equipped");
        Add("equip_unequipped_section", "未装備", "Unequipped");

        // 父親の家トランジション
        Add("map_father_house_enter",
            "<size=130%>実家に入った…</size>",
            "<size=130%>Entered the family home...</size>");

        // 実家（父親NPC）
        Add("map_father_msg1", "おう、{0}。帰ってきたか。\n…別に心配なんかしてねぇからな。",
            "Oh, {0}. You're back.\n...It's not like I was worried or anything.");
        Add("map_father_msg2", "強くなったな。…まあ、俺の子だからな。\n当然だ。",
            "You've gotten stronger. ...Well, you are my kid after all.\nOf course you have.");
        Add("map_father_msg3", "いいか、負けても帰ってこい。\n…ここはお前の家だ。いつでもな。",
            "Listen, even if you lose, come back home.\n...This is your house. Always.");

        // Melodias Queen battle
        Add("battle_melodias_charge", "<color=#FF69B4><size=130%>メロディアス女王が 魅惑の力を 溜めている…！</size></color>\n<color=#FFAA00>つぎのターン とっておきが くる！</color>",
            "<color=#FF69B4><size=130%>Queen Melodias is gathering enchanting power...!</size></color>\n<color=#FFAA00>An ultimate attack is coming next turn!</color>");
        Add("battle_melodias_ultimate", "<color=#FF69B4><size=150%>メロディアス女王「聴きなさい…」</size></color>",
            "<color=#FF69B4><size=150%>Queen Melodias: \"Listen...\"</size></color>");
        Add("battle_melodias_ultimate_name", "<color=#FFD700><size=140%>\u266B 魅惑のメロディ \u266B</size></color>",
            "<color=#FFD700><size=140%>\u266B Enchanting Melody \u266B</size></color>");
        Add("battle_melodias_ultimate_hit", "<color=#FF69B4>魅惑のメロディ が きまった！</color>\n<color=#FF4444>まんぞく度 だいアップ！</color>",
            "<color=#FF69B4>Enchanting Melody explodes!</color>\n<color=#FF4444>{0} massive damage!</color>");
        Add("battle_melodias_ultimate_blocked", "<color=#4488FF>まもりで こらえた！</color>\nまんぞく度 {0} アップ！",
            "<color=#4488FF>Held on with defense!</color>\n{0} damage!");
        Add("battle_melodias_debuffed", "<color=#FF69B4>ふしぎな ちからで あそびぢからが さがった！ 3ターン！</color>",
            "<color=#FF69B4>Attack power dropped from enchanting power! 3 turns!</color>");

        // Melodias Queen defeat
        Add("melodias_defeat_line1", "メロディアス女王 が にっこり おやすみした…\n", "Queen Melodias has fallen...\n");
        Add("melodias_defeat_line2", "「…私の メロディが…\n届かなかった というの…？」\n",
            "\"...My melody...\ndidn't reach you...?\"\n");
        Add("melodias_defeat_line3", "109に 静寂が 戻った。\n魅惑の音色が 消えていく…\n",
            "Silence returns to 109.\nThe enchanting melody fades...\n");

        // Victory screen
        Add("battle_saved_return", "<color=#FFD700>{0}({1}ヶ月)</color>\n<color=#00FF00>セーブしました！</color>",
            "<color=#FFD700>{0}({1} months)</color>\n<color=#00FF00>Saved!</color>");
        Add("battle_victory_return", "<color=#FFD700>{0}({1}ヶ月)</color>\n<color=#AAAAAA>しょうり！</color>",
            "<color=#FFD700>{0}({1} months)</color>\n<color=#AAAAAA>Victory!</color>");
        Add("battle_returning", "むらに もどります...", "Returning to the village...");

        // Save in battle
        Add("battle_save_button", "セーブ", "Save");
        Add("battle_top_button", "トップへ", "Title");

        // ===== Map Scene =====
        Add("map_boss_sign", "<color=#FF4444>ボスのやかた</color>", "<color=#FF4444>Boss Mansion</color>");
        Add("map_help_text", "矢印キー / WASD: 移動　　スペース: 調べる　　ESC: メニュー",
            "Arrow Keys / WASD: Move    Space: Interact    ESC: Menu");
        Add("map_encounter", "<color=#FF0000>おともだちが あそびにきた！</color>",
            "<color=#FF0000>An enemy appeared!</color>");
        Add("map_boss_enter", "<color=#FF2222><size=130%>おともだちの おやかたに はいった！</size></color>\n\n<size=80%>つよい てきの けはいがする...</size>",
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
        Add("map_golden_egg_hint", "きんのたまごを てにいれた！\nメニューの「もちもの」から\nかくにん してみよう！",
            "Got a golden egg!\nCheck it in Inventory\nfrom the menu!");
        Add("map_milk_heal", "赤ちゃんミルクで元気いっぱい！", "Baby milk! Feeling great!");
        Add("map_milk_full", "もう元気いっぱいだよ！", "Already feeling great!");

        // Map menu
        Add("map_menu_status", "ステータス", "Status");
        Add("map_menu_equipment", "そうび", "Equipment");
        Add("map_menu_inventory", "もちもの", "Inventory");
        Add("map_menu_home", "ホーム", "Home");
        Add("map_menu_save", "セーブ", "Save");
        Add("map_menu_title", "タイトルへ戻る", "Back to Title");
        Add("map_menu_close", "とじる", "Close");

        // Home panel
        Add("home_title", "プレイヤー情報", "Player Info");
        Add("home_slot", "セーブスロット", "Save Slot");
        Add("home_no_save", "未セーブ", "Not Saved");
        Add("home_meet", "運命のガチャ", "Destiny Gacha");
        Add("home_gacha_desc", "運命のガチャを引くことができます。", "You can pull the gacha of destiny.");
        Add("home_babys", "旅に出る", "Go on a Journey");
        Add("home_enishi", "縁（えにし）の書", "Book of Bonds");
        Add("enishi_title", "縁（えにし）の書", "Book of Bonds");
        Add("enishi_added", "{0}が縁（えにし）の書に追加された。", "{0} was added to the Book of Bonds.");
        Add("enishi_empty", "まだ誰とも縁を結んでいない。", "No bonds have been formed yet.");
        Add("enishi_section_enemies", "あそんだ おともだち", "Defeated Enemies");
        Add("enishi_section_npcs", "出会った人々", "People Met");
        Add("enishi_section_fathers", "父親", "Fathers");
        Add("enishi_section_mothers", "母親", "Mothers");
        Add("ui_back", "戻る", "Back");

        // Map status panel
        Add("map_status_title", "ステータス", "Status");
        Add("map_status_months", "ヶ月", " months");
        Add("map_status_hp", "<b>ごきげん度:</b>", "<b>Mood:</b>");
        Add("map_status_atk", "<b>ぬくもり:</b>", "<b>Warmth:</b>");
        Add("map_status_def", "<b>おちつき:</b>", "<b>Calm:</b>");
        Add("map_status_intelligence", "<b>ちえ:</b>", "<b>Wisdom:</b>");
        Add("map_status_athletic", "<b>運動:</b>", "<b>AGI:</b>");
        Add("map_status_luck", "<b>運勢:</b>", "<b>LUK:</b>");
        Add("map_status_fortune", "<b>資産:</b>", "<b>FTN:</b>");
        Add("map_status_trait", "<b>特徴:</b>", "<b>Trait:</b>");
        Add("map_status_exp", "<b>おもいで:</b>", "<b>EXP:</b>");
        Add("map_status_enemies", "<b>あそんだ おともだち:</b>", "<b>Defeated:</b>");
        Add("map_status_father", "<b>父:</b>", "<b>Father:</b>");
        Add("map_status_mother", "<b>母:</b>", "<b>Mother:</b>");

        // Map inventory panel
        Add("inv_milk", "<color=#FFB6C1>🍼 ミルク: {0}ml</color>", "<color=#FFB6C1>🍼 Milk: {0}ml</color>");
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
        Add("mskill_name_サクラ", "ヒーリングタッチ", "Healing Touch");
        Add("mskill_desc_サクラ", "あそびつつ じぶんの ごきげんを かいふくする やさしいわざ", "Play gently while restoring own mood");
        Add("mskill_name_ヒナタ", "トゲトゲ・バブル", "Playful Mist");
        Add("mskill_desc_ヒナタ", "おともだちに ミストをかけて、3ターンの あいだ じわじわ まんぞくさせる", "Spray mist on friend, satisfying them over 3 turns");
        Add("mskill_name_アキラ", "かぜのステップ", "Breeze Step");
        Add("mskill_desc_アキラ", "すばやい うごきで あそび、2ターンの あいだ かいひりょくが あがる", "Quick play that boosts evasion for 2 turns");
        Add("mskill_name_ミサト", "おべんきょうウェーブ", "Study Wave");
        Add("mskill_desc_ミサト", "おともだちの にがてを みつけて、2ターンの あいだ おちつきを さげる", "Find friend's weakness, lowering calm for 2 turns");
        Add("mskill_name_カエデ", "ほんわかオーラ", "Cozy Aura");
        Add("mskill_desc_カエデ", "ほんわかした ふんいきで、2ターンの あいだ あそびぢからを さげる", "Cozy aura that lowers play power for 2 turns");
        Add("mskill_name_ルナ", "スターダスト", "Stardust");
        Add("mskill_desc_ルナ", "ほしくずを まとった あそび。まんぞく度の いちぶを ごきげんとして きゅうしゅうする", "Play wrapped in stardust. Absorb part of satisfaction as mood");

        // ===== Touch UI =====
        Add("touch_interact", "調べる", "Interact");
        Add("touch_menu", "メニュー", "Menu");
        Add("map_help_text_touch", "D-pad: 移動　[調べる]: タイル調査　[メニュー]: メニュー",
            "D-pad: Move  [Interact]: Investigate  [Menu]: Menu");

        // ===== Labels for character row =====
        Add("label_fathers", "父親", "Fathers");
        Add("label_mothers", "母親", "Mothers");

        // ===== Profile Scene =====
        Add("profile_title", "プレイヤー設定", "Player Setup");
        Add("profile_name_label", "プレイヤー名", "Player Name");
        Add("profile_name_placeholder", "名前を入力", "Enter your name");
        Add("profile_start", "はじめる", "Start");
        Add("profile_save", "保存する", "Save");
        Add("profile_icon_select", "アイコンを選択", "Select Icon");
        Add("profile_icon_saved", "アイコンを保存しました", "Icon saved");

        // ===== Weapon Shop =====
        Add("shop_title", "おあそびどうぐや", "Weapon Shop");
        Add("shop_milk_label", "ミルク: {0}ml", "Milk: {0}ml");
        Add("shop_buy", "買う ({0})", "Buy ({0})");
        Add("shop_purchased", "購入済み", "Purchased");
        Add("shop_not_enough", "ミルクが足りない…", "Not enough milk...");
        Add("shop_bought", "{0} を手に入れた！", "Got {0}!");
        Add("shop_item_garagara", "ガラガラ", "Rattle Sword");
        Add("shop_item_yodare", "よだれかけ", "Bib Shield");
        Add("shop_item_oshaburi", "おしゃぶりチャーム", "Pacifier Charm");
        Add("shop_item_omutsu", "まほうのおむつ", "Magic Diaper");
        Add("shop_item_honyubin", "きんいろのほ乳瓶", "Golden Bottle");
        Add("shop_item_tiara", "ほしのティアラ", "Devil Tiara");
        Add("shop_effect_garagara", "ATK+4", "ATK+4");
        Add("shop_effect_yodare", "DEF+4", "DEF+4");
        Add("shop_effect_oshaburi", "毎ターンごきげん+3回復", "Mood+3 per turn");
        Add("shop_effect_omutsu", "DEF+3 トゲトゲ・バブル耐性", "DEF+3 Poison resist");
        Add("shop_effect_honyubin", "ATK+3 DEF+3", "ATK+3 DEF+3");
        Add("shop_effect_tiara", "ATK+6 DEF-2", "ATK+6 DEF-2");
        Add("shop_item_nakineko", "なきねこミット", "Crying Cat Punch");
        Add("shop_item_yodarekake_mini", "ミニよだれかけ", "Mini Bib");
        Add("shop_item_niji_rattle", "にじいろガラガラ", "Rainbow Rattle");
        Add("shop_effect_nakineko", "ATK+2", "ATK+2");
        Add("shop_effect_yodarekake_mini", "DEF+2", "DEF+2");
        Add("shop_effect_niji_rattle", "ATK+5 DEF+2", "ATK+5 DEF+2");
        Add("shop_s_rank_only", "Sランク限定", "S Rank Only");
        Add("shop_interact", "おあそびどうぐやだ。入ってみよう。", "A weapon shop. Let's go in.");
        Add("shop_enter", "おあそびどうぐやに はいった…", "Entered the weapon shop...");
        Add("shop_merchant_greet", "いらっしゃい！なにが ほしいのかな？", "Welcome! What can I get ya?");
        Add("shop_merchant_already_owned", "もう もってるよ？ ほかのを みてみてね！", "Hey, you already got that one! Check out something else!");
        Add("shop_merchant_need_s_rank", "Sランクの あかちゃんじゃないと あつかえないんだ。もっと せいちょうしてから きてね！", "That one's for S Rank babies only! Come back when you're stronger!");
        Add("battle_milk_gained", "<color=#FFB6C1>ミルク {0}ml をかくとく！</color>",
            "<color=#FFB6C1>Gained {0}ml milk!</color>");
        Add("battle_oshaburi_heal", "<color=#00FF00>おしゃぶりチャームで ごきげん+{0}！</color>",
            "<color=#00FF00>Pacifier Charm restores Mood+{0}!</color>");

        // ===== Juku (Cram school) =====
        Add("juku_intro", "きょうも おべんきょう しましょう！\nいまの ちえ: {0}\n3もん れんぞく せいかいで ちえ アップ！",
            "Let's study today!\nCurrent Wisdom: {0}\nGet 3 correct in a row to level up!");
        Add("juku_wisdom_up_title", "ちえ アップ！", "Wisdom Up!");
        Add("juku_reward_text", "すばらしい！\nちえが 1 あがった！", "Wonderful!\nWisdom increased by 1!");

        // ===== Cutscene (after profile creation) =====
        Add("cutscene_line1",
            "この世界を支配している悪魔を倒すため。",
            "To defeat the demon that rules this world.");
        Add("cutscene_line2",
            "まずは、運命のボタンを押そう。",
            "First, press the button of destiny.");
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

    public static string GetParentCutin(string jaName)
    {
        return Get("cutin_" + jaName);
    }

    public static string GetParentBio(string jaName)
    {
        return Get("bio_" + jaName);
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

    public static string GetEnemyBio(string jaName)
    {
        return Get("enemy_bio_" + jaName);
    }

    public static string GetNpcBio(string jaName)
    {
        return Get("npc_bio_" + jaName);
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
