using System;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEditor;
using ZombieSurvival;

/// <summary>
/// ゲーム仕様管理ドキュメントの自動生成・更新ツール
/// ItemDatabase のアイテム・レシピ、実装状況を読み取り
/// Assets/GameDesignDoc.md に最新の仕様書を出力する
/// </summary>
public static class GameDesignDocGenerator
{
    private const string DocPath = "Assets/GameDesignDoc.md";

    /// <summary>
    /// ドキュメントを生成し、ファイルに書き出す
    /// SceneSetupTool や他のエディタ拡張から自動呼び出し可能
    /// </summary>
    public static void Generate()
    {
        var sb = new StringBuilder();

        WriteHeader(sb);
        WriteWorldSetting(sb);
        WriteControls(sb);
        WriteItemList(sb);
        WriteCraftingRecipes(sb);
        WriteEnemyData(sb);
        WriteImplementedFeatures(sb);
        WriteFuturePlan(sb);
        WriteFooter(sb);

        File.WriteAllText(DocPath, sb.ToString(), Encoding.UTF8);
        AssetDatabase.Refresh();
        Debug.Log($"[GameDesignDoc] 仕様書を更新しました: {DocPath}");
    }

    // ========== 各セクション ==========

    private static void WriteHeader(StringBuilder sb)
    {
        sb.AppendLine("# ゾンビサバイバル × クラフト ― ゲーム仕様書");
        sb.AppendLine();
        sb.AppendLine($"> 最終更新: {DateTime.Now:yyyy-MM-dd HH:mm}");
        sb.AppendLine("> ※ このファイルは `Tools > Setup Zombie Survival` 実行時に自動生成されます。手動編集は上書きされます。");
        sb.AppendLine();
    }

    private static void WriteWorldSetting(StringBuilder sb)
    {
        sb.AppendLine("## 1. 世界観");
        sb.AppendLine();
        sb.AppendLine("文明崩壊後の世界。街にはゾンビが徘徊し、生存者は廃墟に散らばる素材を拾い集め、");
        sb.AppendLine("即席の武器をクラフトして生き延びなければならない。");
        sb.AppendLine();
        sb.AppendLine("| 項目 | 内容 |");
        sb.AppendLine("|------|------|");
        sb.AppendLine("| ジャンル | 見下ろし型アクションサバイバル |");
        sb.AppendLine("| 視点 | トップダウン（カメラ Y=18, X=65°） |");
        sb.AppendLine("| コアループ | 探索 → 素材拾い → クラフト → 戦闘 |");
        sb.AppendLine();
    }

    private static void WriteControls(StringBuilder sb)
    {
        sb.AppendLine("## 2. 操作方法");
        sb.AppendLine();
        sb.AppendLine("| キー / 入力 | アクション |");
        sb.AppendLine("|------------|-----------|");
        sb.AppendLine("| W / A / S / D | 移動（上左下右） |");
        sb.AppendLine("| マウス移動 | プレイヤーの向き（照準） |");
        sb.AppendLine("| 左クリック | 武器で射撃 / 攻撃 |");
        sb.AppendLine("| C | クラフト実行 |");
        sb.AppendLine("| 接近（トリガー） | アイテム自動取得 |");
        sb.AppendLine();
    }

    private static void WriteItemList(StringBuilder sb)
    {
        sb.AppendLine("## 3. アイテム一覧");
        sb.AppendLine();
        sb.AppendLine("| 名前 | 種別 | 説明 |");
        sb.AppendLine("|------|------|------|");

        foreach (var item in ItemDatabase.AllItems)
        {
            string typeLabel = item.itemType == ItemType.Material ? "素材" : "武器";
            sb.AppendLine($"| {item.itemName} | {typeLabel} | {item.description} |");
        }
        sb.AppendLine();
    }

    private static void WriteCraftingRecipes(StringBuilder sb)
    {
        sb.AppendLine("## 4. 合成レシピ一覧");
        sb.AppendLine();
        sb.AppendLine("| # | 素材 1 | 数量 | 素材 2 | 数量 | 成果物 |");
        sb.AppendLine("|---|--------|------|--------|------|--------|");

        int idx = 1;
        foreach (var r in ItemDatabase.Recipes)
        {
            sb.AppendLine($"| {idx} | {r.material1} | {r.amount1} | {r.material2} | {r.amount2} | {r.result} |");
            idx++;
        }
        sb.AppendLine();
    }

    private static void WriteEnemyData(StringBuilder sb)
    {
        sb.AppendLine("## 5. 敵データ");
        sb.AppendLine();
        sb.AppendLine("| 種類 | HP | 攻撃力 | 移動速度 | 備考 |");
        sb.AppendLine("|------|----|----|------|------|");
        sb.AppendLine("| （未実装） | - | - | - | フェーズ2 以降で追加予定 |");
        sb.AppendLine();
        sb.AppendLine("> 敵データは `EnemyDatabase` 等を実装した際にこのセクションへ自動反映されます。");
        sb.AppendLine();
    }

    private static void WriteImplementedFeatures(StringBuilder sb)
    {
        sb.AppendLine("## 6. 実装済み機能（プロジェクト進捗）");
        sb.AppendLine();

        // スクリプトの存在をチェックして一覧を生成
        var features = new (string script, string label, string detail)[]
        {
            ("Assets/Scripts/Player/PlayerController.cs",   "プレイヤー制御",       "WASD 移動 + マウス照準回転"),
            ("Assets/Scripts/Camera/CameraFollow.cs",       "カメラ追従",           "見下ろし視点でプレイヤーを追従"),
            ("Assets/Scripts/Inventory/InventoryManager.cs", "インベントリ",         "アイテム追加 / 消費 / イベント通知"),
            ("Assets/Scripts/Pickup/ItemPickup.cs",         "アイテムピックアップ", "トリガー接触で自動取得 + 浮遊演出"),
            ("Assets/Scripts/Crafting/CraftingManager.cs",  "クラフト（合成）",     "C キーでレシピ判定 → 素材消費 → 武器生成"),
            ("Assets/Scripts/Weapon/WeaponController.cs",   "武器発射",             "左クリックで弾を生成 + クールダウン"),
            ("Assets/Scripts/Weapon/Bullet.cs",             "弾",                   "直進 + 時間消滅 + 衝突消滅"),
            ("Assets/Scripts/UI/GameHUD.cs",                "HUD",                  "インベントリ / 操作説明 / レシピ / メッセージ表示"),
            ("Assets/Scripts/Data/ItemData.cs",             "アイテムデータ定義",   "ItemData クラス + ItemType enum"),
            ("Assets/Scripts/Data/ItemDatabase.cs",         "アイテム DB",          "全アイテム + 全レシピの静的定義"),
        };

        sb.AppendLine("| 状態 | 機能 | 詳細 |");
        sb.AppendLine("|------|------|------|");

        foreach (var (script, label, detail) in features)
        {
            bool exists = File.Exists(script);
            string status = exists ? "✅" : "❌";
            sb.AppendLine($"| {status} | {label} | {detail} |");
        }
        sb.AppendLine();
    }

    private static void WriteFuturePlan(StringBuilder sb)
    {
        sb.AppendLine("## 7. 未実装・今後の予定");
        sb.AppendLine();
        sb.AppendLine("- [ ] **ゾンビ AI** ― 徘徊 / 追跡 / 攻撃 + IDamageable によるダメージ処理");
        sb.AppendLine("- [ ] **HP システム** ― プレイヤーとゾンビの体力管理、死亡処理");
        sb.AppendLine("- [ ] **近接武器** ― 釘バットなど近距離攻撃の実装");
        sb.AppendLine("- [ ] **マップ拡張** ― 障害物、建物、ランダムアイテムスポーン");
        sb.AppendLine("- [ ] **敵ドロップ** ― ゾンビ撃破時の素材ドロップ");
        sb.AppendLine("- [ ] **ウェーブ制** ― 時間経過で敵が増加する仕組み");
        sb.AppendLine("- [ ] **サウンド** ― 射撃音、取得音、BGM");
        sb.AppendLine("- [ ] **ゲームオーバー / リスタート** ― HP ゼロ時の画面遷移");
        sb.AppendLine();
    }

    private static void WriteFooter(StringBuilder sb)
    {
        sb.AppendLine("---");
        sb.AppendLine();
        sb.AppendLine("*このドキュメントは `GameDesignDocGenerator.cs` により自動生成されています。*");
        sb.AppendLine("*新しいアイテム・レシピ・敵を追加した場合、`Tools > Setup Zombie Survival` または*");
        sb.AppendLine("*`Tools > Update Game Design Doc` を実行すると自動的に反映されます。*");
    }

    /// <summary>
    /// メニューから単独でドキュメント更新を実行
    /// </summary>
    [MenuItem("Tools/Update Game Design Doc")]
    public static void UpdateFromMenu()
    {
        Generate();
        EditorUtility.DisplayDialog("Game Design Doc",
            "ゲーム仕様書を更新しました。\n" + DocPath, "OK");
    }
}
