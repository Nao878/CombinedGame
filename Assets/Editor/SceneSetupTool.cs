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
        SetupEnemies();
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

        // EnemyAIオブジェクトをすべて削除
        var enemies = GameObject.FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);
        foreach (var e in enemies) DestroyImmediate(e.gameObject);

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

        // HP
        player.AddComponent<PlayerHealth>();

        // 装備武器表示（向き方向の隣に表示）
        GameObject weaponDisp = new GameObject("EquippedWeaponDisplay");
        weaponDisp.transform.SetParent(player.transform);
        weaponDisp.transform.localPosition = new Vector3(0f, 1f, 0f); // 初期=上向き

        TextMesh wtm = weaponDisp.AddComponent<TextMesh>();
        wtm.text = "";
        wtm.fontSize = 48;
        wtm.characterSize = 0.14f;
        wtm.anchor = TextAnchor.MiddleCenter;
        wtm.alignment = TextAlignment.Center;
        wtm.color = new Color(1f, 0.9f, 0.3f); // 黄金色
        wtm.fontStyle = FontStyle.Bold;

        MeshRenderer wmr = weaponDisp.GetComponent<MeshRenderer>();
        if (wmr != null)
        {
            wmr.sortingOrder = 11;
            wmr.enabled = false; // 初期非表示
        }

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
    //  敵ゾンビ
    // ==============================
    private static void SetupEnemies()
    {
        CreateEnemy("腐", 7, 5);
        CreateEnemy("腐", -6, -4);
        CreateEnemy("腐", -8, 3);
    }

    /// <summary>
    /// 敵「腐」をグリッド座標に配置
    /// </summary>
    private static void CreateEnemy(string kanjiChar, int gridX, int gridY)
    {
        GameObject enemy = new GameObject("Enemy_" + kanjiChar);
        enemy.transform.position = new Vector3(gridX * GRID, gridY * GRID, 0f);

        // 赤い漢字で表示
        TextMesh tm = enemy.AddComponent<TextMesh>();
        tm.text = kanjiChar;
        tm.fontSize = 64;
        tm.characterSize = 0.18f;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.color = new Color(1f, 0.2f, 0.15f); // 赤色
        tm.fontStyle = FontStyle.Bold;

        MeshRenderer mr = enemy.GetComponent<MeshRenderer>();
        if (mr != null) mr.sortingOrder = 9;

        // 背景の赤い光
        GameObject glow = new GameObject("Glow");
        glow.transform.SetParent(enemy.transform);
        glow.transform.localPosition = Vector3.zero;
        glow.transform.localScale = new Vector3(1f, 1f, 1f);

        SpriteRenderer glowSR = glow.AddComponent<SpriteRenderer>();
        glowSR.sprite = FindBuiltinSprite("Knob", "Circle");
        glowSR.color = new Color(1f, 0f, 0f, 0.15f);
        glowSR.sortingOrder = 8;

        // EnemyAI スクリプト
        enemy.AddComponent<EnemyAI>();
    }

    // ==============================
    //  UI
    // ==============================
    private static void SetupUI()
    {
        // EventSystem
        if (GameObject.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject esObj = new GameObject("EventSystem");
            esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // Canvas
        GameObject canvasObj = new GameObject("GameCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // ============ 左上ステータス ============

        // ターン表示
        GameObject turnTextObj = CreateTextUI(canvasObj, "TurnText", "ターン: 0",
            new Vector2(20f, -20f), new Vector2(400f, 60f), TextAnchor.UpperLeft);
        SetAnchors(turnTextObj, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        SetTextOverflow(turnTextObj);
        turnTextObj.GetComponent<Text>().fontSize = 40;
        turnTextObj.GetComponent<Text>().color = new Color(0.8f, 1f, 0.5f);

        // HP表示
        GameObject hpTextObj = CreateTextUI(canvasObj, "HPText", "HP: 3/3",
            new Vector2(20f, -85f), new Vector2(400f, 60f), TextAnchor.UpperLeft);
        SetAnchors(hpTextObj, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        SetTextOverflow(hpTextObj);
        hpTextObj.GetComponent<Text>().fontSize = 40;
        hpTextObj.GetComponent<Text>().color = Color.white;

        // 装備表示
        GameObject weaponTextObj = CreateTextUI(canvasObj, "WeaponText", "装備: なし",
            new Vector2(20f, -150f), new Vector2(400f, 60f), TextAnchor.UpperLeft);
        SetAnchors(weaponTextObj, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        SetTextOverflow(weaponTextObj);
        weaponTextObj.GetComponent<Text>().fontSize = 36;
        weaponTextObj.GetComponent<Text>().color = new Color(1f, 0.9f, 0.3f);

        // メッセージ（中央上）
        GameObject msgTextObj = CreateTextUI(canvasObj, "MessageText", "",
            new Vector2(0f, -30f), new Vector2(800f, 60f), TextAnchor.MiddleCenter);
        SetAnchors(msgTextObj, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
        SetTextOverflow(msgTextObj);
        msgTextObj.GetComponent<Text>().fontSize = 36;
        msgTextObj.GetComponent<Text>().color = Color.yellow;

        // ============ 操作説明（右上、パネル付き） ============
        GameObject ctrlPanel = new GameObject("ControlsPanel");
        ctrlPanel.transform.SetParent(canvasObj.transform, false);
        RectTransform cpRt = ctrlPanel.AddComponent<RectTransform>();
        cpRt.anchorMin = new Vector2(1f, 1f);
        cpRt.anchorMax = new Vector2(1f, 1f);
        cpRt.pivot = new Vector2(1f, 1f);
        cpRt.anchoredPosition = new Vector2(-15f, -15f);
        cpRt.sizeDelta = new Vector2(340f, 90f);

        Image cpBg = ctrlPanel.AddComponent<Image>();
        cpBg.color = new Color(0.05f, 0.05f, 0.1f, 0.75f);

        // 操作テキスト
        GameObject ctrlTextObj = new GameObject("ControlsText");
        ctrlTextObj.transform.SetParent(ctrlPanel.transform, false);
        RectTransform ctRt = ctrlTextObj.AddComponent<RectTransform>();
        ctRt.anchorMin = Vector2.zero;
        ctRt.anchorMax = Vector2.one;
        ctRt.offsetMin = new Vector2(10f, 5f);
        ctRt.offsetMax = new Vector2(-10f, -5f);

        Text ctText = ctrlTextObj.AddComponent<Text>();
        ctText.text = "WASD : 移動\nSpace : 攻撃\nドラッグ : 合成";
        ctText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        ctText.fontSize = 22;
        ctText.color = new Color(0.8f, 0.8f, 0.8f);
        ctText.alignment = TextAnchor.UpperLeft;
        ctText.horizontalOverflow = HorizontalWrapMode.Overflow;
        ctText.verticalOverflow = VerticalWrapMode.Overflow;

        // ============ 手札エリア（下部） ============
        GameObject handAreaObj = new GameObject("HandArea");
        handAreaObj.transform.SetParent(canvasObj.transform, false);

        RectTransform handRt = handAreaObj.AddComponent<RectTransform>();
        handRt.anchorMin = new Vector2(0f, 0f);
        handRt.anchorMax = new Vector2(0.65f, 0f);
        handRt.pivot = new Vector2(0f, 0f);
        handRt.anchoredPosition = new Vector2(10f, 10f);
        handRt.sizeDelta = new Vector2(0f, 120f);

        Image handBg = handAreaObj.AddComponent<Image>();
        handBg.color = new Color(0.05f, 0.05f, 0.08f, 0.8f);

        HorizontalLayoutGroup handLayout = handAreaObj.AddComponent<HorizontalLayoutGroup>();
        handLayout.spacing = 8f;
        handLayout.padding = new RectOffset(10, 10, 10, 10);
        handLayout.childAlignment = TextAnchor.MiddleLeft;
        handLayout.childForceExpandWidth = false;
        handLayout.childForceExpandHeight = false;

        // ============ 合成エリア（右下、手札の右隣） ============
        GameObject craftArea = new GameObject("CraftArea");
        craftArea.transform.SetParent(canvasObj.transform, false);
        RectTransform craftRt = craftArea.AddComponent<RectTransform>();
        craftRt.anchorMin = new Vector2(1f, 0f);
        craftRt.anchorMax = new Vector2(1f, 0f);
        craftRt.pivot = new Vector2(1f, 0f);
        craftRt.anchoredPosition = new Vector2(-10f, 10f);
        craftRt.sizeDelta = new Vector2(420f, 120f);

        Image craftBg = craftArea.AddComponent<Image>();
        craftBg.color = new Color(0.08f, 0.08f, 0.12f, 0.85f);

        // 合成ラベル
        GameObject craftLabel = new GameObject("CraftLabel");
        craftLabel.transform.SetParent(craftArea.transform, false);
        RectTransform clRt = craftLabel.AddComponent<RectTransform>();
        clRt.anchorMin = new Vector2(0f, 1f);
        clRt.anchorMax = new Vector2(1f, 1f);
        clRt.pivot = new Vector2(0.5f, 1f);
        clRt.anchoredPosition = new Vector2(0f, 0f);
        clRt.sizeDelta = new Vector2(0f, 25f);
        Text clText = craftLabel.AddComponent<Text>();
        clText.text = "― 合成 ―";
        clText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        clText.fontSize = 22;
        clText.color = new Color(0.7f, 0.7f, 0.7f);
        clText.alignment = TextAnchor.MiddleCenter;
        clText.horizontalOverflow = HorizontalWrapMode.Overflow;

        // スロットコンテナ
        GameObject slotContainer = new GameObject("SlotContainer");
        slotContainer.transform.SetParent(craftArea.transform, false);
        RectTransform scRt = slotContainer.AddComponent<RectTransform>();
        scRt.anchorMin = new Vector2(0f, 0f);
        scRt.anchorMax = new Vector2(1f, 0.8f);
        scRt.offsetMin = new Vector2(10f, 5f);
        scRt.offsetMax = new Vector2(-10f, -5f);

        HorizontalLayoutGroup slotLayout = slotContainer.AddComponent<HorizontalLayoutGroup>();
        slotLayout.spacing = 8f;
        slotLayout.childAlignment = TextAnchor.MiddleCenter;
        slotLayout.childForceExpandWidth = true;
        slotLayout.childForceExpandHeight = true;

        CraftSlot craftSlot1 = CreateCraftSlotUI(slotContainer, "Slot1", "素材1");
        CreateSlotLabel(slotContainer, "+Label", "＋");
        CraftSlot craftSlot2 = CreateCraftSlotUI(slotContainer, "Slot2", "素材2");
        CreateSlotLabel(slotContainer, "=Label", "＝");
        GameObject resultSlot = CreateResultSlotUI(slotContainer, "ResultSlot");
        Text resultText = resultSlot.GetComponentInChildren<Text>();

        // レシピ表示（合成エリア上）
        GameObject recipeTextObj = CreateTextUI(canvasObj, "RecipeText", "",
            new Vector2(-10f, 140f), new Vector2(420f, 100f), TextAnchor.LowerRight);
        SetAnchors(recipeTextObj, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f));
        SetTextOverflow(recipeTextObj);
        recipeTextObj.GetComponent<Text>().fontSize = 22;

        // インベントリテキスト（非表示、GameHUD参照用）
        GameObject invTextObj = CreateTextUI(canvasObj, "InventoryText", "",
            new Vector2(0f, 0f), new Vector2(1f, 1f), TextAnchor.UpperLeft);
        invTextObj.SetActive(false);

        // ============ CraftingUI コンポーネント ============
        CraftingUI craftingUI = canvasObj.AddComponent<CraftingUI>();
        craftingUI.SetReferences(
            handAreaObj.transform,
            craftSlot1,
            craftSlot2,
            resultText,
            canvas
        );

        // ============ GameHUD ============
        GameHUD hud = canvasObj.AddComponent<GameHUD>();
        SetPrivateField(hud, "inventoryText", invTextObj.GetComponent<Text>());
        SetPrivateField(hud, "controlsText", ctrlTextObj.GetComponent<Text>());
        SetPrivateField(hud, "messageText", msgTextObj.GetComponent<Text>());
        SetPrivateField(hud, "recipeText", recipeTextObj.GetComponent<Text>());
        SetPrivateField(hud, "turnText", turnTextObj.GetComponent<Text>());
        SetPrivateField(hud, "hpText", hpTextObj.GetComponent<Text>());
        SetPrivateField(hud, "weaponText", weaponTextObj.GetComponent<Text>());
    }

    // ==============================
    //  合成スロット生成ヘルパー
    // ==============================
    private static CraftSlot CreateCraftSlotUI(GameObject parent, string name, string label)
    {
        GameObject slotObj = new GameObject(name);
        slotObj.transform.SetParent(parent.transform, false);

        RectTransform rt = slotObj.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(70f, 70f);

        Image bg = slotObj.AddComponent<Image>();
        bg.color = new Color(0.2f, 0.2f, 0.3f, 0.8f);

        // ラベル
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(slotObj.transform, false);
        RectTransform lrt = labelObj.AddComponent<RectTransform>();
        lrt.anchorMin = new Vector2(0f, 0f);
        lrt.anchorMax = new Vector2(1f, 0.3f);
        lrt.offsetMin = Vector2.zero;
        lrt.offsetMax = Vector2.zero;

        Text labelText = labelObj.AddComponent<Text>();
        labelText.text = label;
        labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        labelText.fontSize = 11;
        labelText.color = new Color(0.5f, 0.5f, 0.5f);
        labelText.alignment = TextAnchor.MiddleCenter;

        CraftSlot slot = slotObj.AddComponent<CraftSlot>();
        return slot;
    }

    private static void CreateSlotLabel(GameObject parent, string name, string text)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent.transform, false);
        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(30f, 70f);

        LayoutElement le = obj.AddComponent<LayoutElement>();
        le.preferredWidth = 30f;

        Text t = obj.AddComponent<Text>();
        t.text = text;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = 28;
        t.color = Color.white;
        t.alignment = TextAnchor.MiddleCenter;
    }

    private static GameObject CreateResultSlotUI(GameObject parent, string name)
    {
        GameObject slotObj = new GameObject(name);
        slotObj.transform.SetParent(parent.transform, false);

        RectTransform rt = slotObj.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(70f, 70f);

        Image bg = slotObj.AddComponent<Image>();
        bg.color = new Color(0.3f, 0.25f, 0.1f, 0.8f);

        // 結果テキスト
        GameObject textObj = new GameObject("ResultText");
        textObj.transform.SetParent(slotObj.transform, false);
        RectTransform trt = textObj.AddComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;

        Text text = textObj.AddComponent<Text>();
        text.text = "？";
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 36;
        text.color = Color.gray;
        text.alignment = TextAnchor.MiddleCenter;

        return slotObj;
    }

    // ==============================
    //  ヘルパー
    // ==============================
    private static void SetAnchors(GameObject obj, Vector2 min, Vector2 max, Vector2 pivot)
    {
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = min; rt.anchorMax = max; rt.pivot = pivot;
    }

    private static void SetTextOverflow(GameObject obj)
    {
        Text text = obj.GetComponent<Text>();
        if (text != null)
        {
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
        }
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
