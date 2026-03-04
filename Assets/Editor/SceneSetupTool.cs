using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using ZombieSurvival;

/// <summary>
/// ゾンビサバイバル（漢字合成 × グリッドベース版）シーンセットアップ
/// メニュー: Tools > Setup Zombie Survival
/// </summary>
public class SceneSetupTool : Editor
{
    // グリッドサイズ
    private const float GRID = 1f;

    // マップ範囲
    private const int MAP_MIN_X = -9;
    private const int MAP_MAX_X = 9;
    private const int MAP_MIN_Y = -6;
    private const int MAP_MAX_Y = 6;

    [MenuItem("Tools/Setup Zombie Survival")]
    public static void SetupScene()
    {
        if (!EditorUtility.DisplayDialog(
            "Zombie Survival Setup (Grid)",
            "グリッドベース・ターン制シーンをセットアップします。\n実行しますか？",
            "実行", "キャンセル"))
        {
            return;
        }

        CleanupExisting();

        SetupCamera();
        SetupGround();
        SetupPlayer();
        SetupKanjiPickups();
        SetupUI();

        GameDesignDocGenerator.Generate();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("[SceneSetup] グリッドベース 漢字合成シーンのセットアップが完了しました！");
    }

    private static void CleanupExisting()
    {
        // ItemPickupオブジェクトをすべて削除
        var pickups = GameObject.FindObjectsByType<ItemPickup>(FindObjectsSortMode.None);
        foreach (var p in pickups) DestroyImmediate(p.gameObject);

        string[] objectNames = {
            "Player", "Ground", "GridLines", "Main Camera",
            "Directional Light", "GameCanvas", "GameManagers", "EventSystem"
        };
        foreach (var name in objectNames)
        {
            var obj = GameObject.Find(name);
            if (obj != null) DestroyImmediate(obj);
        }
    }

    // ==============================
    //  カメラ（Orthographic、左寄せ）
    // ==============================
    private static void SetupCamera()
    {
        GameObject camObj = new GameObject("Main Camera");
        camObj.tag = "MainCamera";

        Camera cam = camObj.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 8f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.05f, 0.05f, 0.08f);

        camObj.transform.position = new Vector3(0f, 0f, -10f);
        camObj.AddComponent<AudioListener>();
        camObj.AddComponent<CameraFollow>();
    }

    // ==============================
    //  地面（暗い背景）
    // ==============================
    private static void SetupGround()
    {
        // 地面
        GameObject ground = new GameObject("Ground");
        SpriteRenderer sr = ground.AddComponent<SpriteRenderer>();
        sr.sprite = FindBuiltinSprite("UISprite", "Square");
        sr.color = new Color(0.10f, 0.12f, 0.08f);
        sr.sortingOrder = -10;
        ground.transform.localScale = new Vector3(22f, 16f, 1f);

        // グリッドライン（視覚的なガイド）
        CreateGridLines();
    }

    /// <summary>
    /// グリッドの視覚ライン生成
    /// </summary>
    private static void CreateGridLines()
    {
        GameObject gridParent = new GameObject("GridLines");

        Color lineColor = new Color(0.2f, 0.25f, 0.15f, 0.3f);

        // 縦線
        for (int x = MAP_MIN_X; x <= MAP_MAX_X; x++)
        {
            GameObject line = new GameObject($"VLine_{x}");
            line.transform.SetParent(gridParent.transform);
            SpriteRenderer sr = line.AddComponent<SpriteRenderer>();
            sr.sprite = FindBuiltinSprite("UISprite", "Square");
            sr.color = lineColor;
            sr.sortingOrder = -9;
            line.transform.position = new Vector3(x, 0f, 0f);
            line.transform.localScale = new Vector3(0.02f, (MAP_MAX_Y - MAP_MIN_Y + 1), 1f);
        }

        // 横線
        for (int y = MAP_MIN_Y; y <= MAP_MAX_Y; y++)
        {
            GameObject line = new GameObject($"HLine_{y}");
            line.transform.SetParent(gridParent.transform);
            SpriteRenderer sr = line.AddComponent<SpriteRenderer>();
            sr.sprite = FindBuiltinSprite("UISprite", "Square");
            sr.color = lineColor;
            sr.sortingOrder = -9;
            line.transform.position = new Vector3(0f, y, 0f);
            line.transform.localScale = new Vector3((MAP_MAX_X - MAP_MIN_X + 1), 0.02f, 1f);
        }
    }

    // ==============================
    //  プレイヤー
    // ==============================
    private static void SetupPlayer()
    {
        GameObject player = new GameObject("Player");
        player.tag = "Player";
        player.transform.position = Vector3.zero; // グリッド(0,0)

        // 「人」の漢字で表示
        TextMesh tm = player.AddComponent<TextMesh>();
        tm.text = "人";
        tm.fontSize = 64;
        tm.characterSize = 0.18f;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.color = new Color(0.3f, 0.7f, 1f);
        tm.fontStyle = FontStyle.Bold;

        MeshRenderer mr = player.GetComponent<MeshRenderer>();
        if (mr != null) mr.sortingOrder = 10;

        // グリッド移動（物理なし）
        GridMovement gm = player.AddComponent<GridMovement>();
        gm.SetBounds(MAP_MIN_X, MAP_MAX_X, MAP_MIN_Y, MAP_MAX_Y);

        // 武器コントローラー
        player.AddComponent<WeaponController>();

        // Managers
        GameObject managers = new GameObject("GameManagers");
        managers.AddComponent<InventoryManager>();
        managers.AddComponent<CraftingManager>();
        managers.AddComponent<TurnManager>();
    }

    // ==============================
    //  漢字パーツピックアップ
    // ==============================
    private static void SetupKanjiPickups()
    {
        // 銃 = 金 + 充
        CreateKanjiPickup("金", 4, 4);
        CreateKanjiPickup("充", -5, 5);

        // 板 = 木 + 反
        CreateKanjiPickup("木", -6, -3);
        CreateKanjiPickup("反", 6, -4);

        // 剣 = 金 + 刃
        CreateKanjiPickup("金", -3, 2);
        CreateKanjiPickup("刃", 7, 1);

        // 爆 = 火 + 薬
        CreateKanjiPickup("火", 2, -5);
        CreateKanjiPickup("薬", -7, -1);
    }

    /// <summary>
    /// 漢字パーツをグリッド座標に配置
    /// </summary>
    private static void CreateKanjiPickup(string kanjiName, int gridX, int gridY)
    {
        ItemData data = ItemDatabase.GetByName(kanjiName);
        if (data == null)
        {
            Debug.LogWarning($"[SceneSetup] '{kanjiName}' がデータベースにありません");
            return;
        }

        GameObject pickup = new GameObject("Pickup_" + kanjiName);
        pickup.transform.position = new Vector3(gridX * GRID, gridY * GRID, 0f);

        // 漢字表示
        TextMesh tm = pickup.AddComponent<TextMesh>();
        tm.text = data.displayCharacter;
        tm.fontSize = 48;
        tm.characterSize = 0.12f;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.color = data.displayColor;
        tm.fontStyle = FontStyle.Bold;

        MeshRenderer mr = pickup.GetComponent<MeshRenderer>();
        if (mr != null) mr.sortingOrder = 5;

        // 背景の光る丸
        GameObject glow = new GameObject("Glow");
        glow.transform.SetParent(pickup.transform);
        glow.transform.localPosition = Vector3.zero;
        glow.transform.localScale = new Vector3(0.8f, 0.8f, 1f);

        SpriteRenderer glowSR = glow.AddComponent<SpriteRenderer>();
        glowSR.sprite = FindBuiltinSprite("Knob", "Circle");
        Color glowColor = data.displayColor;
        glowColor.a = 0.2f;
        glowSR.color = glowColor;
        glowSR.sortingOrder = 4;

        // ItemPickup スクリプト（コライダー不要）
        ItemPickup itemPickup = pickup.AddComponent<ItemPickup>();
        itemPickup.SetItemName(kanjiName);
    }

    // ==============================
    //  UI（右側パネル）
    // ==============================
    private static void SetupUI()
    {
        GameObject canvasObj = new GameObject("GameCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // 所持漢字（右上）
        GameObject invTextObj = CreateTextUI(canvasObj, "InventoryText",
            "--- 所持漢字 ---\n(空)",
            new Vector2(-20f, -20f), new Vector2(320f, 300f), TextAnchor.UpperLeft);
        SetAnchors(invTextObj, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f));

        // 操作説明（右下）
        GameObject ctrlTextObj = CreateTextUI(canvasObj, "ControlsText",
            "--- 操作 ---\nWASD : 1マス移動\nSpace : 攻撃\nC : 漢字合成",
            new Vector2(-20f, 20f), new Vector2(320f, 140f), TextAnchor.LowerLeft);
        SetAnchors(ctrlTextObj, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f));

        // メッセージ（中央上）
        GameObject msgTextObj = CreateTextUI(canvasObj, "MessageText", "",
            new Vector2(0f, -30f), new Vector2(600f, 40f), TextAnchor.MiddleCenter);
        SetAnchors(msgTextObj, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
        msgTextObj.GetComponent<Text>().fontSize = 20;
        msgTextObj.GetComponent<Text>().color = Color.yellow;

        // レシピ（右中央）
        GameObject recipeTextObj = CreateTextUI(canvasObj, "RecipeText", "",
            new Vector2(-20f, 0f), new Vector2(320f, 160f), TextAnchor.MiddleLeft);
        SetAnchors(recipeTextObj, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f));

        // ターン表示（左上）
        GameObject turnTextObj = CreateTextUI(canvasObj, "TurnText", "ターン: 0",
            new Vector2(20f, -20f), new Vector2(200f, 40f), TextAnchor.UpperLeft);
        SetAnchors(turnTextObj, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        turnTextObj.GetComponent<Text>().fontSize = 22;
        turnTextObj.GetComponent<Text>().color = new Color(0.8f, 1f, 0.5f);

        // HUD
        GameHUD hud = canvasObj.AddComponent<GameHUD>();
        SetPrivateField(hud, "inventoryText", invTextObj.GetComponent<Text>());
        SetPrivateField(hud, "controlsText", ctrlTextObj.GetComponent<Text>());
        SetPrivateField(hud, "messageText", msgTextObj.GetComponent<Text>());
        SetPrivateField(hud, "recipeText", recipeTextObj.GetComponent<Text>());
        SetPrivateField(hud, "turnText", turnTextObj.GetComponent<Text>());
    }

    // ==============================
    //  ヘルパー
    // ==============================
    private static void SetAnchors(GameObject obj, Vector2 min, Vector2 max, Vector2 pivot)
    {
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = min; rt.anchorMax = max; rt.pivot = pivot;
    }

    private static GameObject CreateTextUI(GameObject parent, string name, string content,
        Vector2 position, Vector2 size, TextAnchor alignment)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent.transform, false);

        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchoredPosition = position;
        rt.sizeDelta = size;

        Text text = obj.AddComponent<Text>();
        text.text = content;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 16;
        text.color = Color.white;
        text.alignment = alignment;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;

        // 背景
        GameObject bgObj = new GameObject(name + "_BG");
        bgObj.transform.SetParent(obj.transform, false);
        bgObj.transform.SetAsFirstSibling();

        RectTransform bgRt = bgObj.AddComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = new Vector2(-10f, -5f);
        bgRt.offsetMax = new Vector2(10f, 5f);

        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = new Color(0f, 0f, 0f, 0.6f);
        bgImage.raycastTarget = false;

        return obj;
    }

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(fieldName,
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null) field.SetValue(target, value);
    }

    private static Sprite FindBuiltinSprite(string primaryName, string fallbackName)
    {
        Sprite[] allSprites = Resources.FindObjectsOfTypeAll<Sprite>();
        Sprite fallback = null;
        foreach (var s in allSprites)
        {
            if (s.name == primaryName) return s;
            if (s.name == fallbackName) fallback = s;
        }
        return fallback;
    }
}
