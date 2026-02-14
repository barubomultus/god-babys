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
        Add("birth_father_gacha", "父親", "Father");
        Add("birth_mother_gacha", "母親", "Mother");
        Add("birth_next", "次へ", "Next");

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

        // ===== Phase Cut-in =====
        Add("cutin_who_father", "呼び声に応えるのは――", "Answering the call――");
        Add("birth_find_mother", "宿命を定める", "Determine Destiny");
        Add("birth_nurture_love", "愛を育む", "Nurture Love");
        Add("cutin_who_mother", "惹かれ合う、もう一つの魂", "Another soul, drawn together");
        Add("cutin_baby_born", "子宝に恵まれた！", "Blessed with a child!");
        Add("birth_love_begin", "二人の物語を紡ぐ", "Weave Their Story");

        // ===== Father Cut-in =====
        Add("cutin_タケシ", "元・格闘技世界王者のタケシだ！！", "It's Takeshi, the ex-World Champion!!");
        Add("cutin_ユウキ", "天才ハッカーのユウキだ！！", "It's Yuuki, the genius hacker!!");
        Add("cutin_ゴウ", "伝説の傭兵のゴウだ！！", "It's Gou, the legendary mercenary!!");
        Add("cutin_シンジ", "ノーベル賞受賞者のシンジだ！！", "It's Shinji, the Nobel laureate!!");
        Add("cutin_リョウマ", "世界一の実業家のリョウマだ！！", "It's Ryouma, the world's top tycoon!!");
        Add("cutin_テツヤ", "伝説のロックスターのテツヤだ！！", "It's Tetsuya, the legendary rockstar!!");

        // ===== Mother Cut-in =====
        Add("cutin_サクラ", "暗殺拳の継承者のサクラだ！！", "It's Sakura, heir of assassination arts!!");
        Add("cutin_ヒナタ", "美容帝国CEOのヒナタだ！！", "It's Hinata, the beauty empire CEO!!");
        Add("cutin_アキラ", "五輪金メダリストのアキラだ！！", "It's Akira, the Olympic gold medalist!!");
        Add("cutin_ミサト", "量子物理学者のミサトだ！！", "It's Misato, the quantum physicist!!");
        Add("cutin_カエデ", "天才外科医のカエデだ！！", "It's Kaede, the genius surgeon!!");
        Add("cutin_ルナ", "世界的スーパーモデルのルナだ！！", "It's Luna, the global supermodel!!");

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

        // ===== Father Bios =====
        Add("bio_タケシ",
            "幼少期から喧嘩に明け暮れ、14歳で地元の不良グループを壊滅させた伝説を持つ。16歳で格闘技の道に進み、わずか3年でプロデビュー。圧倒的なパワーと不屈の精神力で世界王座を5度防衛した。引退後は山奥で修行を続け、握力180kgという超人的な肉体を維持。「強さとは守るためにある」が信条で、普段は穏やかだが、大切な人を傷つける者には容赦しない。現在は秘境の道場で次世代の格闘家を育成しながら、密かに復帰を狙っている。",
            "From childhood, Takeshi lived for fighting — at 14, he single-handedly dismantled the local gang. At 16, he entered professional martial arts and debuted within three years. With overwhelming power and iron will, he defended his world title five times. After retiring, he continues training in a mountain retreat, maintaining a superhuman 180kg grip. His creed: \"Strength exists to protect.\" Calm by nature, he shows no mercy to those who threaten his loved ones. He now trains the next generation at a secluded dojo while secretly planning his comeback.");
        Add("bio_ユウキ",
            "5歳でプログラミングを独学し、8歳で政府機関のセキュリティを突破した天才少年。12歳で逮捕されるも、才能を見込まれてサイバー防衛の特別顧問に抜擢される。17歳で立ち上げたスタートアップは3年で世界最大のAI企業に成長。特許数3200件は人類史上最多記録。表向きはクールなIT長者だが、実はゲーム廃人で週末は72時間ぶっ通しでMMOをプレイしている。「世界のバグは俺が直す」と豪語するが、自室の散らかりようは致命的なバグそのもの。",
            "Self-taught programming at 5, breached government security at 8 — a true prodigy. Arrested at 12, his talent earned him a position as a special cybersecurity advisor. His startup, founded at 17, became the world's largest AI company in three years. His 3,200 patents set a human record. Outwardly a cool tech mogul, he's secretly a gaming addict who plays MMOs for 72 hours straight on weekends. He boasts \"I'll fix every bug in the world,\" yet his own room is a catastrophic bug itself.");
        Add("bio_ゴウ",
            "孤児として紛争地帯で育ち、10歳で少年兵として戦場に立った。15歳で傭兵団を脱走し、以後は一匹狼のフリーランス傭兵として世界中の紛争を渡り歩く。100を超える作戦を遂行し、一度も任務に失敗したことがない。その戦闘力は計測不能とされ、各国の軍事機関が恐れる存在。しかし、非戦闘員には一切手を出さない独自の掟を持つ。戦場から離れると花が好きで、拠点には小さな庭を作る習慣がある。「本当の強さは戦わずに済む力だ」と語るが、彼の前に立てる者はいない。",
            "Raised as an orphan in a conflict zone, Gou became a child soldier at 10. At 15, he deserted his mercenary corps and went freelance, drifting through global conflicts. Over 100 operations completed, never a single failure. His combat ability is classified as immeasurable, feared by military agencies worldwide. Yet he lives by a code: never harm non-combatants. Away from battle, he loves flowers and always plants a small garden at his base. \"True strength means never having to fight,\" he says — but no one dares stand before him.");
        Add("bio_シンジ",
            "3歳で微分方程式を解き、7歳で大学に飛び級入学した超天才。15歳でMITの博士課程を修了し、量子力学・遺伝子工学・宇宙工学の3分野でノーベル賞を受賞。IQ250は公式に測定された人類最高記録。しかし本人は「知性に限界はない」と更なる高みを目指し続ける。研究に没頭すると3日間食事を忘れるほどの集中力を見せる一方、日常生活では靴を左右逆に履いて出歩くこともしばしば。「宇宙の真理を解き明かす」という壮大な夢のため、現在は秘密研究所で禁断の実験を行っている。",
            "Solving differential equations at 3, entering university at 7 — a super-genius. Completed his MIT doctorate at 15 and won Nobel Prizes in quantum mechanics, genetic engineering, and aerospace. His IQ of 250 is the highest officially recorded in history. Yet he insists \"intelligence has no limits\" and keeps pushing higher. When deep in research, he forgets to eat for three days — but in daily life, he often walks around with his shoes on the wrong feet. Pursuing his grand dream to \"unravel the truth of the universe,\" he now conducts forbidden experiments in a secret lab.");
        Add("bio_リョウマ",
            "貧しい漁村で生まれ、15歳で裸一貫から起業。最初のビジネスは中古の自転車修理だったが、独自の商才で事業を拡大し、20歳で初めての会社を上場させる。その後、IT・金融・宇宙開発と次々に新規事業を立ち上げ、30歳で総資産43兆円の世界一の実業家に。「稼いだ金は未来への投資だ」という信念のもと、資産の半分を教育と医療の支援に充てている。見た目はブランドスーツに身を包んだ冷徹な経営者だが、故郷の漁村には毎年必ず帰り、幼馴染と酒を酌み交わすのが唯一の息抜き。",
            "Born in a poor fishing village, Ryouma started from nothing at 15. His first business was fixing used bicycles, but his natural business instincts grew it rapidly — IPO at 20. He then launched ventures in IT, finance, and space, becoming the world's richest man at 30 with $430B. Believing \"money earned is invested in the future,\" he donates half his wealth to education and healthcare. In his designer suits, he appears ruthless, but he returns to his hometown every year without fail to drink with his childhood friends — his only escape.");
        Add("bio_テツヤ",
            "中学時代にバンドを結成し、高校中退後に上京。路上ライブで注目を集め、17歳でメジャーデビュー。デビューシングルが全世界で1億枚を売り上げ、一夜にしてスターダムへ。カリスマ的なステージパフォーマンスとアーティスティックな楽曲で8億人のファンを獲得。しかし栄光の裏で薬物依存に苦しみ、一時は活動休止に追い込まれた。地獄のようなリハビリを経て復帰し、その経験を綴った楽曲「Rebirth」は音楽史上最も感動的な曲と称される。現在はソロ活動の傍ら、音楽で人を救う慈善活動にも力を入れている。",
            "Formed a band in middle school, dropped out of high school and moved to Tokyo. Street performances caught attention, and he debuted at 17. His first single sold 100 million copies worldwide, catapulting him to stardom overnight. His charismatic stage presence and artistic songs amassed 800 million fans. But behind the glory, he battled addiction and was forced into hiatus. After hellish rehabilitation, he returned — his song \"Rebirth\" is hailed as the most moving in music history. Now he pursues solo work alongside charity, using music to save lives.");

        // ===== Mother Bios =====
        Add("bio_サクラ",
            "名門暗殺一族の末裔として生まれ、物心つく前から暗殺拳の修行を課せられた。7歳で一族の試練を全て突破し、史上最年少で「継承者」の称号を得る。しかし15歳の時、暗殺任務の標的が無実の人間だと知り、一族を裏切り逃亡。追手を全て返り討ちにしながら世界を放浪し、裏格闘技界に身を投じる。全戦全勝の戦績を持ち、その拳は「触れた者の命を刈り取る」と恐れられている。だが素顔は甘いものに目がない少女で、任務後は必ずパフェを食べに行くのが唯一の秘密。",
            "Born into an elite assassination clan, Sakura was trained in deadly arts before she could walk. At 7, she passed every trial and became the youngest heir in history. But at 15, she discovered her target was innocent, betrayed the clan, and fled. She defeated every pursuer while wandering the world, entering underground fighting. Undefeated in all bouts, her fists are said to reap the lives of those they touch. Yet beneath it all, she's a girl with a sweet tooth — her one secret is always getting a parfait after every mission.");
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
        Add("enemy_どくベイビー", "どくベイビー", "Poison Baby");
        Add("enemy_のろいベイビー", "のろいベイビー", "Cursed Baby");
        Add("enemy_やみベイビー", "やみベイビー", "Dark Baby");
        Add("enemy_あくまベイビー", "あくまベイビー", "Demon Baby");
        Add("enemy_じゃあくベイビー", "じゃあくベイビー", "Wicked Baby");
        Add("enemy_まおうベイビー", "まおうベイビー", "Overlord Baby");
        Add("enemy_デヴィル傭兵A", "デヴィル傭兵A", "Devil Mercenary A");
        Add("enemy_デヴィル傭兵B", "デヴィル傭兵B", "Devil Mercenary B");
        Add("enemy_デヴィル夫人", "デヴィル夫人", "Devil Lady");

        // Enemy bios
        Add("enemy_bio_なきむしベイビー",
            "いつもメソメソ泣いている気弱な赤ちゃん。でも油断は禁物。涙は弱さの証ではなく、溜め込んだ感情がいつか爆発する前触れなのだ。泣き声を聞いた敵は不思議と力が抜けてしまう。本人は友達がほしいだけなのに、泣き声のせいで誰も近づいてくれないのが悩み。「えーん、ぼくと遊んでよぉ…」が口ぐせ。実は夜中にこっそり星を見るのが好き。",
            "A timid baby who's always crying. But don't let your guard down—those tears aren't a sign of weakness, they're a sign of emotions about to explode. Enemies who hear the crying mysteriously lose their strength. All this baby wants is a friend, but the crying keeps everyone away. Favorite phrase: \"Waah, play with me...\" Secretly loves watching stars at night.");
        Add("enemy_bio_やんちゃベイビー",
            "とにかく元気いっぱいで落ち着きのない赤ちゃん。じっとしていることが大の苦手で、目に入るもの全てにちょっかいを出す。その無尽蔵の体力は村でも有名で、大人たちも手を焼いている。いたずらの天才で、村の柵を何度も壊しては直させている。本人に悪気はなく、ただ世界が楽しすぎるだけ。将来は誰よりも速く走れる戦士になりたいらしい。",
            "An endlessly energetic baby who can't sit still. Everything in sight becomes a target for mischief. Famous in the village for inexhaustible stamina that exhausts even adults. A prank genius who keeps breaking the village fences. No ill intent—the world is just too fun. Dreams of becoming the fastest warrior someday.");
        Add("enemy_bio_いじわるベイビー",
            "ニヤニヤ笑いながら他の赤ちゃんをからかうのが趣味の小悪魔的な赤ちゃん。頭の回転が速く、相手の弱点を見抜くのが得意。そのずる賢さは戦闘でも発揮され、予想外の攻撃で翻弄してくる。でも実は誰よりも寂しがり屋で、構ってほしくてイジワルをしているだけ。夜になると一人でぬいぐるみを抱きしめている姿を目撃されたことがあるとか。",
            "A little devil who loves teasing other babies with a sly grin. Quick-witted and skilled at finding weaknesses. That cunning shines in battle with unexpected attacks. But deep down, this baby is lonelier than anyone—the teasing is just a cry for attention. Rumor has it they've been spotted hugging a stuffed toy alone at night.");
        Add("enemy_bio_わがままベイビー",
            "「ぼくの！ぜんぶぼくの！」が口ぐせの、自己主張が強すぎる赤ちゃん。欲しいものは絶対に手に入れる執念を持ち、その強い意志は戦闘において驚くべき粘り強さとなって現れる。村のおやつを独り占めしようとして何度も怒られているが、全く反省しない。でもたまに気まぐれで他の赤ちゃんにおやつを分けることもあり、根は優しい一面も。",
            "\"Mine! It's all mine!\" is this baby's motto. With overwhelming determination, what they want, they get. That willpower translates into remarkable tenacity in battle. Constantly scolded for hoarding village snacks but never learns. Occasionally shares snacks on a whim, showing a hidden gentle side.");
        Add("enemy_bio_あばれんぼうベイビー",
            "村で一番力が強いと恐れられている赤ちゃん。怒ると手がつけられなくなり、村の岩を素手で砕いたという伝説がある。その圧倒的なパワーは生まれつきのもので、本人もコントロールしきれていない。暴れるのは力を持て余しているからで、本当は花を育てるのが好きな優しい心の持ち主。花畑の花は実はこの子が密かに世話しているらしい。",
            "The most feared baby in the village for sheer strength. Once angered, there's no stopping them—legends say they shattered a boulder with bare hands. That overwhelming power is innate and even they can't fully control it. The rampaging comes from pent-up energy. In truth, this baby loves growing flowers—the village flower patch is secretly their handiwork.");
        Add("enemy_bio_村の王シバ",
            "この村を治める長であり、最強の戦士。かつては悪魔族と戦うことを夢見て、日夜修行に明け暮れていた。しかし実際に悪魔と対峙すると身体が震えて動けなくなるという体質が判明。自ら悪魔と戦うことを諦めたシバは、代わりに悪魔と戦える強い赤ちゃんを育てることに人生を捧げた。村の子どもたちに厳しくも愛情深い指導を行い、いつか悪魔を倒す勇者が現れることを信じている。必殺技「シバの裁き」は容赦ないが、それも愛ゆえ。",
            "The village chief and its strongest warrior. Once dreamed of fighting demons and trained relentlessly. But upon actually facing a demon, discovered an involuntary trembling that left him immobile. Having given up fighting demons himself, Shiba dedicated his life to raising babies strong enough to fight in his stead. He trains the village children with strictness born of deep love, believing a hero who can defeat the demons will one day emerge. His ultimate technique 'Shiba's Judgment' shows no mercy—but it's all out of love.");
        Add("enemy_bio_わるいベイビー",
            "なぜ悪いのか、誰にもわからない。生まれた時からどこか影のある赤ちゃん。何を考えているか読めない表情と、不気味な笑顔で村の赤ちゃんたちを怯えさせている。でもたまに迷子の子猫を助けたり、雨の日に花にそっと傘をかけたりする姿が目撃されている。悪いのは表面だけで、本当は繊細で傷つきやすい心を守るための鎧なのかもしれない。",
            "Why so bad? Nobody knows. A baby with a shadow from birth. An unreadable expression and eerie smile that frightens the village babies. But occasionally spotted rescuing lost kittens or sheltering flowers from rain. Perhaps the 'badness' is just armor protecting a sensitive, easily hurt heart underneath.");
        Add("enemy_bio_どくベイビー",
            "悪魔村に漂う瘴気を浴びて生まれた赤ちゃん。紫色の肌から常に毒の霧を放ち、近づくだけで体が痺れてしまう。本人は毒を出していることに気づいておらず、なぜ誰も遊んでくれないのか不思議に思っている。唯一の友達は毒に耐性のある小さな毒蛙で、いつも頭の上に乗せている。「あそぼーよー」と近づくたびに周りの花が枯れていくのが切ない。",
            "A baby born bathed in the miasma of Devil Village. Purple skin constantly emits a poisonous mist that numbs anyone who gets close. Unaware of its own toxicity, it wonders why nobody will play. Its only friend is a small poison frog that sits on its head. Every time it approaches saying \"Let's play!\" the nearby flowers wilt—a tragic sight.");
        Add("enemy_bio_のろいベイビー",
            "悪魔村の古い祠の前で見つかった赤ちゃん。生まれながらにして呪いの力を持ち、睨んだ相手の体が重くなるという恐ろしい能力がある。しかしその力は本人の意思とは関係なく発動するため、笑顔を見せようとしても相手を呪ってしまう悲しい体質。暗い場所が好きで、祠の中で一人きりで古い絵本を読んでいる姿がたまに目撃される。実は笑うと呪いが弱まることに最近気づいた。",
            "A baby found before an ancient shrine in Devil Village. Born with the power of curses, its gaze makes others' bodies grow heavy. But the power activates beyond its will—even a smile curses others. Prefers dark places, sometimes spotted reading old picture books alone in the shrine. Recently discovered that laughing weakens the curse.");
        Add("enemy_bio_やみベイビー",
            "闇そのものから生まれたとされる赤ちゃん。影の中を自在に移動でき、気配を完全に消すことができる。その速さは悪魔村でも随一で、姿を捉えることすら困難。しかし本当は暗闇が怖くて、明るい場所に憧れている矛盾した性格の持ち主。夜な夜な村の灯りを遠くから眺めているのは、その温かさに惹かれているから。いつか光の中で暮らしたいと密かに願っている。",
            "Said to be born from darkness itself. Can freely move through shadows and erase its presence completely. The fastest in Devil Village—even catching a glimpse is difficult. Yet paradoxically, it fears the dark and yearns for light. Night after night, it watches village lanterns from afar, drawn to their warmth. Secretly wishes to live in the light someday.");
        Add("enemy_bio_あくまベイビー",
            "悪魔の血を色濃く受け継いだ赤ちゃん。小さな角と尻尾を持ち、生まれた時から圧倒的な戦闘本能を備えている。怒ると目が赤く光り、触れるもの全てを焼き尽くす業火を放つ。悪魔村の中でも恐れられる存在だが、実は甘いミルクが大好きで、こっそり村の牧場からミルクを盗んでは至福の表情を浮かべている。「俺様が最強だ」と叫ぶ姿は可愛いらしいと評判。",
            "A baby with strong demon blood. Born with small horns and a tail, possessing overwhelming combat instinct. When angered, its eyes glow red and it unleashes hellfire that incinerates everything. Feared even in Devil Village, but secretly loves sweet milk—stealing from the village dairy with a blissful expression. Its shout of \"I'm the strongest!\" is considered adorable.");
        Add("enemy_bio_じゃあくベイビー",
            "悪魔村の最深部に棲む、邪悪な力を宿した赤ちゃん。周囲の空気を歪めるほどの強大な魔力を持ち、その泣き声は聞いた者の心を蝕む。かつては普通の赤ちゃんだったが、禁断の魔法陣に触れてしまい変貌した。時折、元の姿に戻りかけることがあり、その瞬間だけは穏やかな笑顔を見せる。完全に邪悪になりきれない中途半端さが、逆に不気味さを増している。",
            "A baby dwelling in Devil Village's deepest area, harboring wicked power. Its magical force distorts the air around it, and its cry erodes the hearts of all who hear. Once an ordinary baby, it was transformed after touching a forbidden magic circle. Occasionally reverts partially, showing a gentle smile in those moments. Its inability to become fully evil only makes it more unsettling.");
        Add("enemy_bio_まおうベイビー",
            "悪魔村を統べる王の血族とされる伝説の赤ちゃん。その存在は悪魔村でも噂でしか語られず、実際に目撃した者はほとんどいない。全身から放たれる漆黒のオーラは、見る者の魂を凍りつかせる。しかし伝承によれば、この赤ちゃんは世界のバランスを保つために生まれた存在であり、本当の敵ではないとも言われている。真実は誰にもわからない。その瞳の奥には、悲しみとも覚悟ともつかない光が宿っている。",
            "A legendary baby said to be of the bloodline that rules Devil Village. Its existence is spoken of only in rumors—almost no one has actually seen it. The jet-black aura radiating from its body freezes the souls of onlookers. Yet legend says this baby was born to maintain the world's balance and may not be a true enemy. No one knows the truth. Deep within its eyes dwells a light that could be sorrow or resolve.");
        Add("enemy_bio_デヴィル傭兵A",
            "デヴィル夫人に忠誠を誓う傭兵の一人。元は人間だったが、夫人の毒の力に魅せられて配下となった。常に館の入口を守り、侵入者を容赦なく排除する。無表情で冷酷だが、夫人のことを語る時だけは目に光が宿る。戦闘では毒を纏った拳で攻撃し、相手の体力をじわじわと奪っていく。「夫人の命令は絶対だ」が口ぐせ。",
            "One of the mercenaries sworn to Devil Lady. Once human, he was captivated by the Lady's poisonous power and became her subordinate. Always guarding the mansion entrance, he eliminates intruders without mercy. Cold and expressionless, but his eyes light up only when speaking of the Lady. Fights with poison-coated fists that slowly drain the enemy's stamina. His motto: \"The Lady's orders are absolute.\"");
        Add("enemy_bio_デヴィル傭兵B",
            "デヴィル夫人の右腕とも呼ばれる傭兵。傭兵Aとは幼馴染で、共に夫人に仕えることを選んだ。Aよりも攻撃的な性格で、戦闘では容赦ない連続攻撃を仕掛けてくる。しかし仲間想いの一面もあり、傭兵Aが倒された時は怒りに燃えるという噂がある。趣味は毒花の栽培で、館の裏庭には美しくも危険な花が咲き誇っている。",
            "A mercenary called Devil Lady's right hand. Childhood friends with Mercenary A, they both chose to serve the Lady. More aggressive than A, unleashing relentless combo attacks in battle. Yet has a caring side—rumor says he burns with rage when Mercenary A falls. His hobby is cultivating poisonous flowers, and the mansion's back garden blooms with beautiful yet dangerous flora.");
        Add("enemy_bio_デヴィル夫人",
            "悪魔村の奥深くに建つ館の主。かつては高名な薬師だったが、禁断の毒を研究するうちに闇に堕ちた。その美貌は年齢を超越し、毒の力で永遠の若さを保っているとされる。館に足を踏み入れた者は、彼女の「毒の洗礼」から逃れることはできない。しかし、その瞳の奥には薬師だった頃の面影が残っており、本当は世界を救いたいという想いが眠っているのかもしれない。必殺技「毒の洗礼」は触れた者の魂まで蝕む。",
            "The mistress of the mansion deep in Devil Village. Once a renowned pharmacist, she fell to darkness while researching forbidden poisons. Her beauty transcends age, preserved eternally by poisonous power. Those who enter her mansion cannot escape her 'Poison Baptism.' Yet deep in her eyes lingers the shadow of her pharmacist days—perhaps a sleeping wish to save the world. Her ultimate technique 'Poison Baptism' corrodes even the soul of those it touches.");

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
        Add("battle_enemy_special", "<color=#FF4444>{0} の とくしゅこうげき！</color>",
            "<color=#FF4444>{0} uses a special attack!</color>");
        Add("battle_enemy_special_hit", "<color=#FF4444>{0} の とくしゅこうげき！</color>\n{1} ダメージ！",
            "<color=#FF4444>{0}'s special attack!</color>\n{1} damage!");
        Add("battle_enemy_defend", "<color=#4488FF>{0} は ぼうぎょ体勢をとった！</color>",
            "<color=#4488FF>{0} takes a defensive stance!</color>");
        Add("battle_enemy_defend_heal", "<color=#4488FF>{0} は ぼうぎょしつつ HPが {1} かいふくした！</color>",
            "<color=#4488FF>{0} defends and recovers {1} HP!</color>");
        Add("battle_shiba_charging", "<color=#FF4444><size=130%>シバが ちからを ためている…！</size></color>\n<color=#FFAA00>つぎのターン ひっさつわざが くる！</color>",
            "<color=#FF4444><size=130%>Shiba is gathering power...!</size></color>\n<color=#FFAA00>An ultimate attack is coming next turn!</color>");
        Add("battle_shiba_ultimate_announce", "<color=#FF0000><size=150%>シバ「くらえ！！」</size></color>",
            "<color=#FF0000><size=150%>Shiba: \"Take this!!\"</size></color>");
        Add("battle_shiba_ultimate_name", "<color=#FF0000><size=140%>🔥 おうのいかり 🔥</size></color>",
            "<color=#FF0000><size=140%>🔥 King's Wrath 🔥</size></color>");
        Add("battle_shiba_ultimate_hit", "<color=#FF0000>おうのいかり が さくれつ！</color>\n<color=#FF4444>{0} の だいダメージ！</color>",
            "<color=#FF0000>King's Wrath explodes!</color>\n<color=#FF4444>{0} massive damage!</color>");
        Add("battle_shiba_ultimate_blocked", "<color=#4488FF>ぼうぎょで こらえた！</color>\n{0} ダメージ！",
            "<color=#4488FF>Held on with defense!</color>\n{0} damage!");
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

        // Player poison (devil village enemies)
        Add("battle_player_poisoned", "<color=#AA00FF>どくに おかされた！ からだが しびれる...</color>",
            "<color=#AA00FF>Poisoned! Body going numb...</color>");
        Add("battle_player_poison_already", "<color=#AA00FF>すでに どくに おかされている...</color>",
            "<color=#AA00FF>Already poisoned...</color>");
        Add("battle_player_poison_damage", "<color=#AA00FF>どくで {0} ダメージ！</color>",
            "<color=#AA00FF>Took {0} poison damage!</color>");
        Add("map_poison_damage", "<color=#AA00FF>どくで {0} ダメージ！</color>",
            "<color=#AA00FF>Took {0} poison damage!</color>");
        Add("map_poison_cured", "<color=#00FF00>どくが なおった！</color>",
            "<color=#00FF00>Poison cured!</color>");

        // Run / Escape
        Add("battle_run", "にげる", "Run");
        Add("battle_run_name", "にげる", "Escape");
        Add("battle_run_desc", "戦闘から逃げる（成功率50%）", "Flee from battle (50% success rate)");
        Add("battle_run_success", "うまく にげきれた！", "Got away safely!");
        Add("battle_run_fail", "にげられなかった！", "Couldn't escape!");
        Add("battle_run_boss", "ボスからは にげられない！", "Can't run from a boss!");

        // Devil village
        Add("map_devil_village", "悪魔村", "Devil Village");
        Add("map_devil_boss_sign", "<color=#AA44FF>デヴィル夫人のやかた</color>", "<color=#AA44FF>Devil Lady's Mansion</color>");

        // Boss defeat
        Add("boss_defeat_line1", "村の王シバ が たおれた...\n", "King Shiba has fallen...\n");
        Add("boss_defeat_line2", "「これは 試練に すぎなかった。」\n",
            "\"That was merely a trial.\"\n");
        Add("boss_defeat_line3", "村の外には さらに強大な 敵が待っている。\nおまえの ちからは まだ 足りない。\n",
            "Beyond the village, even mightier foes await.\nYour power is not yet enough.\n");
        Add("boss_defeat_line4", "成長し、すべての 敵を 打ち砕け。\n世界は おまえを 待っている。\n",
            "Grow stronger, and crush all your enemies.\nThe world is waiting for you.\n");

        // Devil Lady's Mansion
        Add("map_mansion_enter", "<color=#AA44FF><size=130%>デヴィル夫人のやかたに\n足を踏み入れた…</size></color>",
            "<color=#AA44FF><size=130%>You entered\nDevil Lady's Mansion...</size></color>");
        Add("map_mansion_mercenary_block", "<color=#FF4444>デヴィル傭兵が 立ちはだかっている！</color>",
            "<color=#FF4444>A Devil Mercenary stands in the way!</color>");
        Add("map_mansion_boss_locked", "傭兵を 倒さないと 進めない…",
            "Must defeat the mercenaries to proceed...");
        Add("battle_run_fixed", "この てきからは にげられない！",
            "Can't run from this enemy!");
        Add("map_mansion_boss_enter", "<color=#AA44FF><size=130%>デヴィル夫人 が 立ちはだかる！</size></color>",
            "<color=#AA44FF><size=130%>Devil Lady stands in your way!</size></color>");

        // Devil Lady battle
        Add("battle_devil_lady_charge", "<color=#AA00FF><size=130%>デヴィル夫人が 毒の力を 溜めている…！</size></color>\n<color=#FFAA00>つぎのターン ひっさつわざが くる！</color>",
            "<color=#AA00FF><size=130%>Devil Lady is gathering poison power...!</size></color>\n<color=#FFAA00>An ultimate attack is coming next turn!</color>");
        Add("battle_devil_lady_ultimate", "<color=#AA00FF><size=150%>デヴィル夫人「毒の洗礼よ！」</size></color>",
            "<color=#AA00FF><size=150%>Devil Lady: \"Poison Baptism!\"</size></color>");
        Add("battle_devil_lady_ultimate_name", "<color=#AA00FF><size=140%>\u2620 毒の洗礼 \u2620</size></color>",
            "<color=#AA00FF><size=140%>\u2620 Poison Baptism \u2620</size></color>");
        Add("battle_devil_lady_ultimate_hit", "<color=#AA00FF>毒の洗礼 が さくれつ！</color>\n<color=#FF4444>{0} の だいダメージ！</color>",
            "<color=#AA00FF>Poison Baptism explodes!</color>\n<color=#FF4444>{0} massive damage!</color>");
        Add("battle_devil_lady_ultimate_blocked", "<color=#4488FF>ぼうぎょで こらえた！</color>\n{0} ダメージ！",
            "<color=#4488FF>Held on with defense!</color>\n{0} damage!");
        Add("battle_devil_lady_poisoned", "<color=#AA00FF>猛毒に おかされた！ 5ターンの間 どくダメージ！</color>",
            "<color=#AA00FF>Severely poisoned! Poison damage for 5 turns!</color>");

        // Devil Lady defeat
        Add("devil_lady_defeat_line1", "デヴィル夫人 が たおれた...\n", "Devil Lady has fallen...\n");
        Add("devil_lady_defeat_line2", "「…まさか この私が…\nあの頃の 光を 思い出すとは…」\n",
            "\"...To think that I...\nwould remember the light of those days...\"\n");
        Add("devil_lady_defeat_line3", "館に 静寂が 戻った。\n毒の霧が 晴れていく…\n",
            "Silence returns to the mansion.\nThe poisonous mist clears...\n");

        // 109 mansion
        Add("map_109_sign", "<color=#FF69B4>109</color>", "<color=#FF69B4>109</color>");
        Add("map_109_enter", "<color=#FF69B4><size=130%>109に\n足を踏み入れた…</size></color>",
            "<color=#FF69B4><size=130%>You entered\n109...</size></color>");
        Add("map_109_boss_enter", "<color=#FF69B4><size=130%>メロディアス女王 が 立ちはだかる！</size></color>",
            "<color=#FF69B4><size=130%>Queen Melodias stands in your way!</size></color>");

        // Melodias Queen battle
        Add("battle_melodias_charge", "<color=#FF69B4><size=130%>メロディアス女王が 魅惑の力を 溜めている…！</size></color>\n<color=#FFAA00>つぎのターン ひっさつわざが くる！</color>",
            "<color=#FF69B4><size=130%>Queen Melodias is gathering enchanting power...!</size></color>\n<color=#FFAA00>An ultimate attack is coming next turn!</color>");
        Add("battle_melodias_ultimate", "<color=#FF69B4><size=150%>メロディアス女王「聴きなさい…」</size></color>",
            "<color=#FF69B4><size=150%>Queen Melodias: \"Listen...\"</size></color>");
        Add("battle_melodias_ultimate_name", "<color=#FFD700><size=140%>\u266B 魅惑のメロディ \u266B</size></color>",
            "<color=#FFD700><size=140%>\u266B Enchanting Melody \u266B</size></color>");
        Add("battle_melodias_ultimate_hit", "<color=#FF69B4>魅惑のメロディ が さくれつ！</color>\n<color=#FF4444>{0} の だいダメージ！</color>",
            "<color=#FF69B4>Enchanting Melody explodes!</color>\n<color=#FF4444>{0} massive damage!</color>");
        Add("battle_melodias_ultimate_blocked", "<color=#4488FF>ぼうぎょで こらえた！</color>\n{0} ダメージ！",
            "<color=#4488FF>Held on with defense!</color>\n{0} damage!");
        Add("battle_melodias_debuffed", "<color=#FF69B4>魅惑の力で こうげきりょくが さがった！ 3ターン！</color>",
            "<color=#FF69B4>Attack power dropped from enchanting power! 3 turns!</color>");

        // Melodias Queen defeat
        Add("melodias_defeat_line1", "メロディアス女王 が たおれた...\n", "Queen Melodias has fallen...\n");
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
        Add("map_milk_heal", "赤ちゃんミルクで元気いっぱい！", "Baby milk! Feeling great!");
        Add("map_milk_full", "もう元気いっぱいだよ！", "Already feeling great!");

        // Map menu
        Add("map_menu_status", "ステータス", "Status");
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
        Add("enishi_section_enemies", "倒した敵", "Defeated Enemies");
        Add("enishi_section_fathers", "父親", "Fathers");
        Add("enishi_section_mothers", "母親", "Mothers");
        Add("ui_back", "戻る", "Back");

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
