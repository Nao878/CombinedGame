using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using ZombieSurvival;

/// <summary>
/// ゾンビサバイバル（漢字合成版）シーン一括セットアップツール（2D）
/// メニュー: Tools > Setup Zombie Survival
/// </summary>
public class SceneSetupTool : Editor
{
    [MenuItem("Tools/Setup Zombie Survival")]
    public static void SetupScene()
    {
        if (!EditorUtility.DisplayDialog(
            "Zombie Survival Setup (2D 漢字合成)",
            "2Dシーンをセットアップします（漢字合成版）。\n実行しますか？",
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
        Debug.Log("[SceneSetup] 2D 漢字合成ゾンビサバイバル シーンのセットアップが完了しました！");
    }

    private static void CleanupExisting()
    {
        // 動的名称のピックアップも含めてすべて削除
        var pickups = GameObject.FindObjectsByType<ItemPickup>(FindObjectsSortMode.None);
        foreach (var p in pickups)
        {
            DestroyImmediate(p.gameObject);
        }

        string[] objectNames = {
            "Player", "Ground", "Main Camera", "Directional Light",
            "GameCanvas", "GameManagers", "EventSystem"
        };
        foreach (var name in objectNames)
        {
            var obj = GameObject.Find(name);
            if (obj != null) DestroyImmediate(obj);
        }
    }

    // ==============================
    //  カメラ
    // ==============================
    private static void SetupCamera()
    {
        GameObject camObj = new GameObject("Main Camera");
        camObj.tag = "MainCamera";

        Camera cam = camObj.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 8f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.06f, 0.06f, 0.10f);

        camObj.transform.position = new Vector3(0f, 0f, -10f);
        camObj.AddComponent<AudioListener>();
        camObj.AddComponent<CameraFollow>();
    }

    // ==============================
    //  地面
    // ==============================
    private static void SetupGround()
    {
        GameObject ground = new GameObject("Ground");
        SpriteRenderer sr = ground.AddComponent<SpriteRenderer>();
        sr.sprite = FindBuiltinSprite("UISprite", "Square");
        sr.color = new Color(0.12f, 0.14f, 0.10f);
        sr.sortingOrder = -10;
        ground.transform.localScale = new Vector3(40f, 40f, 1f);
    }

    // ==============================
    //  プレイヤー
    // ==============================
    private static void SetupPlayer()
    {
        GameObject player = new GameObject("Player");
        player.tag = "Player";

        // 「人」の漢字で表示（TextMesh）
        TextMesh tm = player.AddComponent<TextMesh>();
        tm.text = "人";
        tm.fontSize = 64;
        tm.characterSize = 0.2f;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.color = new Color(0.3f, 0.7f, 1f); // 青色
        tm.fontStyle = FontStyle.Bold;

        MeshRenderer mr = player.GetComponent<MeshRenderer>();
        if (mr != null)
        {
            mr.sortingOrder = 10;
        }

        // Rigidbody2D
        Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // BoxCollider2D
        BoxCollider2D col = player.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.8f, 0.8f);

        // スクリプト
        player.AddComponent<PlayerController>();
        WeaponController weapon = player.AddComponent<WeaponController>();

        // 銃口（発射位置）
        GameObject muzzle = new GameObject("MuzzlePoint");
        muzzle.transform.SetParent(player.transform);
        muzzle.transform.localPosition = new Vector3(0f, 0.8f, 0f);

        SetPrivateField(weapon, "firePoint", muzzle.transform);

        // Managers
        GameObject managers = new GameObject("GameManagers");
        managers.AddComponent<InventoryManager>();
        managers.AddComponent<CraftingManager>();
    }

    // ==============================
    //  漢字パーツピックアップ
    // ==============================
    private static void SetupKanjiPickups()
    {
        // 銃 = 金 + 充
        CreateKanjiPickup("金", new Vector3(4f, 4f, 0f));
        CreateKanjiPickup("充", new Vector3(-5f, 6f, 0f));

        // 板 = 木 + 反
        CreateKanjiPickup("木", new Vector3(-6f, -3f, 0f));
        CreateKanjiPickup("反", new Vector3(6f, -5f, 0f));

        // 剣 = 金 + 刃
        CreateKanjiPickup("金", new Vector3(-3f, 2f, 0f));
        CreateKanjiPickup("刃", new Vector3(7f, 1f, 0f));

        // 爆 = 火 + 薬
        CreateKanjiPickup("火", new Vector3(2f, -6f, 0f));
        CreateKanjiPickup("薬", new Vector3(-7f, -1f, 0f));
    }

    /// <summary>
    /// 漢字パーツのピックアップオブジェクトを生成
    /// TextMesh で漢字1文字を表示
    /// </summary>
    private static void CreateKanjiPickup(string kanjiName, Vector3 position)
    {
        ItemData data = ItemDatabase.GetByName(kanjiName);
        if (data == null)
        {
            Debug.LogWarning($"[SceneSetup] アイテム '{kanjiName}' がデータベースに見つかりません");
            return;
        }

        GameObject pickup = new GameObject("Pickup_" + kanjiName);
        pickup.transform.position = position;

        // --- 漢字文字表示（TextMesh）---
        TextMesh tm = pickup.AddComponent<TextMesh>();
        tm.text = data.displayCharacter;
        tm.fontSize = 64;
        tm.characterSize = 0.15f;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.color = data.displayColor;
        tm.fontStyle = FontStyle.Bold;

        // MeshRenderer の sortingOrder を設定
        MeshRenderer mr = pickup.GetComponent<MeshRenderer>();
        if (mr != null)
        {
            mr.sortingOrder = 5;
        }

        // --- 背景の光る丸（子オブジェクト）---
        GameObject glow = new GameObject("Glow");
        glow.transform.SetParent(pickup.transform);
        glow.transform.localPosition = Vector3.zero;
        glow.transform.localScale = new Vector3(1.2f, 1.2f, 1f);

        SpriteRenderer glowSR = glow.AddComponent<SpriteRenderer>();
        glowSR.sprite = FindBuiltinSprite("Knob", "Circle");
        Color glowColor = data.displayColor;
        glowColor.a = 0.25f;
        glowSR.color = glowColor;
        glowSR.sortingOrder = 4;

        // --- コライダー（トリガー）---
        CircleCollider2D trigger = pickup.AddComponent<CircleCollider2D>();
        trigger.isTrigger = true;
        trigger.radius = 1.5f;

        // --- ItemPickup スクリプト ---
        ItemPickup itemPickup = pickup.AddComponent<ItemPickup>();
        itemPickup.SetItemName(kanjiName);
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

        GameObject invTextObj = CreateTextUI(canvasObj, "InventoryText",
            "--- 所持漢字 ---\n(空)",
            new Vector2(20f, -20f), new Vector2(300f, 200f), TextAnchor.UpperLeft);
        SetAnchors(invTextObj, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));

        GameObject ctrlTextObj = CreateTextUI(canvasObj, "ControlsText",
            "--- 操作方法 ---\nWASD : 移動\nマウス : 照準\n左クリック : 射撃\nC : 漢字合成",
            new Vector2(-20f, 20f), new Vector2(280f, 160f), TextAnchor.LowerRight);
        SetAnchors(ctrlTextObj, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f));

        GameObject msgTextObj = CreateTextUI(canvasObj, "MessageText", "",
            new Vector2(0f, -60f), new Vector2(600f, 50f), TextAnchor.MiddleCenter);
        SetAnchors(msgTextObj, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
        msgTextObj.GetComponent<Text>().fontSize = 22;
        msgTextObj.GetComponent<Text>().color = Color.yellow;

        GameObject recipeTextObj = CreateTextUI(canvasObj, "RecipeText", "",
            new Vector2(20f, 20f), new Vector2(350f, 120f), TextAnchor.LowerLeft);
        SetAnchors(recipeTextObj, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f));

        GameHUD hud = canvasObj.AddComponent<GameHUD>();
        SetPrivateField(hud, "inventoryText", invTextObj.GetComponent<Text>());
        SetPrivateField(hud, "controlsText", ctrlTextObj.GetComponent<Text>());
        SetPrivateField(hud, "messageText", msgTextObj.GetComponent<Text>());
        SetPrivateField(hud, "recipeText", recipeTextObj.GetComponent<Text>());
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
