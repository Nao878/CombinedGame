using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using ZombieSurvival;

/// <summary>
/// ゾンビサバイバル ゲームのシーン一括セットアップツール（2D版）
/// メニュー: Tools > Setup Zombie Survival
/// </summary>
public class SceneSetupTool : Editor
{
    [MenuItem("Tools/Setup Zombie Survival")]
    public static void SetupScene()
    {
        if (!EditorUtility.DisplayDialog(
            "Zombie Survival Setup (2D)",
            "2Dシーンをセットアップします。\n実行しますか？",
            "実行", "キャンセル"))
        {
            return;
        }

        CleanupExisting();

        SetupCamera();
        SetupGround();
        SetupPlayer();
        SetupItemPickups();
        SetupUI();

        // ゲーム仕様書の自動生成・更新
        GameDesignDocGenerator.Generate();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("[SceneSetup] 2D ゾンビサバイバル シーンのセットアップが完了しました！");
    }

    private static void CleanupExisting()
    {
        string[] objectNames = {
            "Player", "Ground", "Main Camera", "Directional Light",
            "GameCanvas", "GameManagers",
            "Pickup_ScrapMetal_1", "Pickup_ScrapMetal_2",
            "Pickup_Gunpowder_1", "Pickup_Gunpowder_2",
            "Pickup_Wood_1", "Pickup_Nail_1",
            "EventSystem"
        };

        foreach (var name in objectNames)
        {
            var obj = GameObject.Find(name);
            if (obj != null) DestroyImmediate(obj);
        }
    }

    // ==============================
    //  カメラ（Orthographic 2D）
    // ==============================
    private static void SetupCamera()
    {
        GameObject camObj = new GameObject("Main Camera");
        camObj.tag = "MainCamera";

        Camera cam = camObj.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 8f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.08f, 0.08f, 0.12f);
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 100f;

        camObj.transform.position = new Vector3(0f, 0f, -10f);
        camObj.transform.rotation = Quaternion.identity;

        camObj.AddComponent<AudioListener>();
        camObj.AddComponent<CameraFollow>();
    }

    // ==============================
    //  地面（2Dスプライト）
    // ==============================
    private static void SetupGround()
    {
        GameObject ground = new GameObject("Ground");
        ground.transform.position = Vector3.zero;

        SpriteRenderer sr = ground.AddComponent<SpriteRenderer>();
        sr.sprite = GetSquareSprite();
        sr.color = new Color(0.18f, 0.30f, 0.12f);
        sr.sortingOrder = -10;

        // 地面を大きく
        ground.transform.localScale = new Vector3(40f, 40f, 1f);
    }

    // ==============================
    //  プレイヤー（2Dスプライト）
    // ==============================
    private static void SetupPlayer()
    {
        // --- プレイヤー本体 ---
        GameObject player = new GameObject("Player");
        player.tag = "Player";
        player.transform.position = Vector3.zero;

        SpriteRenderer sr = player.AddComponent<SpriteRenderer>();
        sr.sprite = GetSquareSprite();
        sr.color = new Color(0.2f, 0.6f, 1f); // 青
        sr.sortingOrder = 10;
        player.transform.localScale = new Vector3(0.8f, 0.8f, 1f);

        // Rigidbody2D
        Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // BoxCollider2D
        BoxCollider2D col = player.AddComponent<BoxCollider2D>();
        col.size = Vector2.one;

        // スクリプト
        player.AddComponent<PlayerController>();
        WeaponController weapon = player.AddComponent<WeaponController>();

        // --- 銃口マーカー（子オブジェクト）---
        GameObject muzzle = new GameObject("MuzzlePoint");
        muzzle.transform.SetParent(player.transform);
        muzzle.transform.localPosition = new Vector3(0f, 0.6f, 0f); // transform.up方向

        SpriteRenderer muzzleSR = muzzle.AddComponent<SpriteRenderer>();
        muzzleSR.sprite = GetSquareSprite();
        muzzleSR.color = Color.red;
        muzzleSR.sortingOrder = 11;
        muzzle.transform.localScale = new Vector3(0.2f, 0.3f, 1f);

        // firePoint をリフレクションで設定
        SetPrivateField(weapon, "firePoint", muzzle.transform);

        // --- GameManagers ---
        GameObject managers = new GameObject("GameManagers");
        managers.AddComponent<InventoryManager>();
        managers.AddComponent<CraftingManager>();
    }

    // ==============================
    //  フィールドアイテム
    // ==============================
    private static void SetupItemPickups()
    {
        CreatePickup("Pickup_ScrapMetal_1", "鉄くず", new Vector3(4f, 4f, 0f));
        CreatePickup("Pickup_ScrapMetal_2", "鉄くず", new Vector3(-6f, 2f, 0f));
        CreatePickup("Pickup_Gunpowder_1", "火薬",   new Vector3(-4f, 6f, 0f));
        CreatePickup("Pickup_Gunpowder_2", "火薬",   new Vector3(5f, -4f, 0f));
        CreatePickup("Pickup_Wood_1",      "木材",   new Vector3(-2f, -5f, 0f));
        CreatePickup("Pickup_Nail_1",      "釘",     new Vector3(7f, -1f, 0f));
    }

    private static void CreatePickup(string objName, string itemName, Vector3 position)
    {
        GameObject pickup = new GameObject(objName);
        pickup.transform.position = position;
        pickup.transform.localScale = new Vector3(0.5f, 0.5f, 1f);

        // SpriteRenderer
        SpriteRenderer sr = pickup.AddComponent<SpriteRenderer>();
        sr.sprite = GetCircleSprite();
        sr.sortingOrder = 5;

        // 色設定
        ItemData data = ItemDatabase.GetByName(itemName);
        if (data != null)
        {
            sr.color = data.displayColor;
        }

        // CircleCollider2D（トリガー）
        CircleCollider2D trigger = pickup.AddComponent<CircleCollider2D>();
        trigger.isTrigger = true;
        trigger.radius = 2f;

        // ItemPickup スクリプト
        ItemPickup itemPickup = pickup.AddComponent<ItemPickup>();
        itemPickup.SetItemName(itemName);
    }

    // ==============================
    //  UI
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

        // インベントリ（左上）
        GameObject invTextObj = CreateTextUI(canvasObj, "InventoryText",
            "--- インベントリ ---\n(空)",
            new Vector2(20f, -20f), new Vector2(300f, 200f), TextAnchor.UpperLeft);
        SetAnchors(invTextObj, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));

        // 操作説明（右下）
        GameObject ctrlTextObj = CreateTextUI(canvasObj, "ControlsText",
            "--- 操作方法 ---\nWASD : 移動\nマウス : 照準\n左クリック : 射撃\nC : クラフト",
            new Vector2(-20f, 20f), new Vector2(280f, 160f), TextAnchor.LowerRight);
        SetAnchors(ctrlTextObj, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f));

        // メッセージ（中央上）
        GameObject msgTextObj = CreateTextUI(canvasObj, "MessageText", "",
            new Vector2(0f, -60f), new Vector2(600f, 50f), TextAnchor.MiddleCenter);
        SetAnchors(msgTextObj, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
        msgTextObj.GetComponent<Text>().fontSize = 22;
        msgTextObj.GetComponent<Text>().color = Color.yellow;

        // レシピ（左下）
        GameObject recipeTextObj = CreateTextUI(canvasObj, "RecipeText", "",
            new Vector2(20f, 20f), new Vector2(350f, 120f), TextAnchor.LowerLeft);
        SetAnchors(recipeTextObj, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f));

        // GameHUD
        GameHUD hud = canvasObj.AddComponent<GameHUD>();
        SetPrivateField(hud, "inventoryText", invTextObj.GetComponent<Text>());
        SetPrivateField(hud, "controlsText", ctrlTextObj.GetComponent<Text>());
        SetPrivateField(hud, "messageText", msgTextObj.GetComponent<Text>());
        SetPrivateField(hud, "recipeText", recipeTextObj.GetComponent<Text>());
    }

    // ==============================
    //  ヘルパー
    // ==============================

    private static void SetAnchors(GameObject obj, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot)
    {
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
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

        // 半透明黒背景
        GameObject bgObj = new GameObject(name + "_BG");
        bgObj.transform.SetParent(obj.transform, false);
        bgObj.transform.SetAsFirstSibling();

        RectTransform bgRt = bgObj.AddComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = new Vector2(-10f, -5f);
        bgRt.offsetMax = new Vector2(10f, 5f);

        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = new Color(0f, 0f, 0f, 0.5f);
        bgImage.raycastTarget = false;

        return obj;
    }

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(fieldName,
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null) field.SetValue(target, value);
    }

    // ==============================
    //  スプライト取得
    // ==============================

    private static Sprite GetSquareSprite()
    {
        return FindBuiltinSprite("UISprite", "Square");
    }

    private static Sprite GetCircleSprite()
    {
        return FindBuiltinSprite("Knob", "Circle");
    }

    /// <summary>
    /// Unity内蔵スプライトを名前で検索
    /// </summary>
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
