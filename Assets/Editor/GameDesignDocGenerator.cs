using System;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEditor;
using ZombieSurvival;

/// <summary>
/// ゲーム仕様管理ドキュメントの自動生成・更新ツール
/// </summary>
public static class GameDesignDocGenerator
{
    private const string DocPath = "Assets/GameDesignDoc.md";

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

    private static void WriteHeader(StringBuilder sb)
    {
        sb.AppendLine("# 漢字サバイバル ― ゲーム仕様書");
        sb.AppendLine();
        sb.AppendLine($"> 最終更新: {DateTime.Now:yyyy-MM-dd HH:mm}");
        sb.AppendLine("> ※ このファイルは `Tools > Setup Zombie Survival` 実行時に自動生成されます。手動編集は上書きされます。");
        sb.AppendLine();
    }

    private static void WriteWorldSetting(StringBuilder sb)
    {
        sb.AppendLine("## 1. 世界観");
        sb.AppendLine();
        sb.AppendLine("言葉が力を失った世界。文明は崩壊し、ゾンビが闊歩する廃墟で、");
        sb.AppendLine("ただ一人の生存者は『漢字を組み合わせて万物を具現化する』能力に覚醒した。");
        sb.AppendLine("グリッド状に区切られた廃墟を1マスずつ慎重に進み、");
        sb.AppendLine("散らばる漢字パーツを集めて武器を合成し、生き延びろ。");
        sb.AppendLine();
        sb.AppendLine("| 項目 | 内容 |");
        sb.AppendLine("|------|------|");
        sb.AppendLine("| ジャンル | グリッドベース・ターン制サバイバルパズル |");
        sb.AppendLine("| 視点 | 2D トップダウン（Orthographic） |");
        sb.AppendLine("| テーマ | 漢字合成による武器創造 |");
        sb.AppendLine("| 移動システム | 1マス単位のグリッド移動（物理演算なし） |");
        sb.AppendLine("| ターン制 | プレイヤー行動 → 敵ターン → 次のターン |");
        sb.AppendLine("| 画面構成 | 左70%: ゲーム画面 / 右30%: インベントリ&合成UI |");
        sb.AppendLine("| コアループ | 探索 → 漢字パーツ収集 → 漢字合成 → 戦闘 |");
        sb.AppendLine();
    }

    private static void WriteControls(StringBuilder sb)
    {
        sb.AppendLine("## 2. 操作方法");
        sb.AppendLine();
        sb.AppendLine("| キー / 入力 | アクション |");
        sb.AppendLine("|------------|-----------|");
        sb.AppendLine("| W / A / S / D（矢印キー） | 1マス移動（グリッドスナップ） |");
        sb.AppendLine("| Space | 漢字武器で攻撃（1ターン消費） |");
        sb.AppendLine("| C | 漢字合成の実行 |");
        sb.AppendLine("| 同一マスに移動 | 漢字パーツ自動取得 |");
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
            string typeLabel = item.itemType == ItemType.Material ? "漢字パーツ" : "漢字武器";
            sb.AppendLine($"| {item.itemName} | {typeLabel} | {item.description} |");
        }
        sb.AppendLine();
    }

    private static void WriteCraftingRecipes(StringBuilder sb)
    {
        sb.AppendLine("## 4. 合成レシピ一覧");
        sb.AppendLine();
        sb.AppendLine("| # | パーツ 1 | 数量 | パーツ 2 | 数量 | 成果物 |");
        sb.AppendLine("|---|---------|------|---------|------|--------|");

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
        sb.AppendLine("| 種類 | 表示 | HP | 攻撃力 | AI | 備考 |");
        sb.AppendLine("|------|------|----|----|------|------|");
        sb.AppendLine("| ゾンビ | 「腐」（赤字） | - | 1 | X/Y距離優先追跡 | 毎ターン1マス移動 |");
        sb.AppendLine();
    }

    private static void WriteImplementedFeatures(StringBuilder sb)
    {
        sb.AppendLine("## 6. 実装済み機能");
        sb.AppendLine();

        var features = new (string script, string label, string detail)[]
        {
            ("Assets/Scripts/Player/GridMovement.cs",       "グリッド移動",         "WASD 1マス移動 + 範囲制限 + ターン通知"),
            ("Assets/Scripts/Core/TurnManager.cs",          "ターンマネージャー",   "プレイヤー行動→敵ターン→次ターンの進行制御"),
            ("Assets/Scripts/Enemy/EnemyAI.cs",              "敵AI（ゾンビ）",       "「腐」グリッド追跡 + 同マス攻撃"),
            ("Assets/Scripts/Camera/CameraFollow.cs",       "カメラ追従",           "Orthographic 2D カメラ"),
            ("Assets/Scripts/Inventory/InventoryManager.cs", "インベントリ",         "漢字パーツ管理 + イベント通知"),
            ("Assets/Scripts/Pickup/ItemPickup.cs",         "漢字パーツ拾得",       "グリッド座標一致で自動取得"),
            ("Assets/Scripts/Crafting/CraftingManager.cs",  "漢字合成",             "C キーで合成 → パーツ消費 → 武器生成"),
            ("Assets/Scripts/Weapon/WeaponController.cs",   "漢字武器攻撃",         "Space キーで弾発射（Transform移動）"),
            ("Assets/Scripts/Weapon/Bullet.cs",             "弾",                   "Transform直進 + 時間消滅"),
            ("Assets/Scripts/UI/GameHUD.cs",                "HUD",                  "所持漢字 / 操作 / レシピ / ターン / メッセージ"),
            ("Assets/Scripts/Data/ItemData.cs",             "漢字データ定義",       "ItemData + displayCharacter"),
            ("Assets/Scripts/Data/ItemDatabase.cs",         "漢字 DB",              "漢字パーツ + 武器 + レシピ"),
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
        sb.AppendLine("- [ ] **HP システム** ― プレイヤーとゾンビの体力");
        sb.AppendLine("- [ ] **近接武器** ― 剣の隣接マス攻撃");
        sb.AppendLine("- [ ] **障害物** ― 通行不可マスの実装");
        sb.AppendLine("- [ ] **マップ生成** ― ランダムダンジョン");
        sb.AppendLine("- [ ] **ゲームオーバー** ― HP ゼロ時の処理");
        sb.AppendLine();
    }

    private static void WriteFooter(StringBuilder sb)
    {
        sb.AppendLine("---");
        sb.AppendLine();
        sb.AppendLine("*このドキュメントは `GameDesignDocGenerator.cs` により自動生成されています。*");
        sb.AppendLine("*`Tools > Setup Zombie Survival` または `Tools > Update Game Design Doc` で更新。*");
    }

    [MenuItem("Tools/Update Game Design Doc")]
    public static void UpdateFromMenu()
    {
        Generate();
        EditorUtility.DisplayDialog("Game Design Doc",
            "ゲーム仕様書を更新しました。\n" + DocPath, "OK");
    }
}
