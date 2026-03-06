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
            new Vector2(20f, -20f), new Vector2(200f, 30f), TextAnchor.UpperLeft);
        SetAnchors(turnTextObj, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        turnTextObj.GetComponent<Text>().fontSize = 20;
        turnTextObj.GetComponent<Text>().color = new Color(0.8f, 1f, 0.5f);

        // HP表示
        GameObject hpTextObj = CreateTextUI(canvasObj, "HPText", "HP: 3/3",
            new Vector2(20f, -55f), new Vector2(200f, 30f), TextAnchor.UpperLeft);
        SetAnchors(hpTextObj, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        hpTextObj.GetComponent<Text>().fontSize = 20;
        hpTextObj.GetComponent<Text>().color = Color.white;

        // 装備表示
        GameObject weaponTextObj = CreateTextUI(canvasObj, "WeaponText", "装備: なし",
            new Vector2(20f, -90f), new Vector2(200f, 30f), TextAnchor.UpperLeft);
        SetAnchors(weaponTextObj, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        weaponTextObj.GetComponent<Text>().fontSize = 18;
        weaponTextObj.GetComponent<Text>().color = new Color(1f, 0.9f, 0.3f);

        // メッセージ
        GameObject msgTextObj = CreateTextUI(canvasObj, "MessageText", "",
            new Vector2(0f, -30f), new Vector2(600f, 36f), TextAnchor.MiddleCenter);
        SetAnchors(msgTextObj, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
        msgTextObj.GetComponent<Text>().fontSize = 18;
        msgTextObj.GetComponent<Text>().color = Color.yellow;

        // 操作説明（右下）
        GameObject ctrlTextObj = CreateTextUI(canvasObj, "ControlsText",
            "WASD移動 / Space攻撃 / ドラッグで合成",
            new Vector2(-20f, 20f), new Vector2(320f, 30f), TextAnchor.LowerLeft);
        SetAnchors(ctrlTextObj, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f));
        ctrlTextObj.GetComponent<Text>().fontSize = 14;
        ctrlTextObj.GetComponent<Text>().color = new Color(0.7f, 0.7f, 0.7f);

        // ============ 合成エリア（右上） ============
        GameObject craftArea = new GameObject("CraftArea");
        craftArea.transform.SetParent(canvasObj.transform, false);
        RectTransform craftRt = craftArea.AddComponent<RectTransform>();
        craftRt.anchorMin = new Vector2(1f, 1f);
        craftRt.anchorMax = new Vector2(1f, 1f);
        craftRt.pivot = new Vector2(1f, 1f);
        craftRt.anchoredPosition = new Vector2(-20f, -20f);
        craftRt.sizeDelta = new Vector2(350f, 100f);

        // 合成エリア背景
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
        clRt.sizeDelta = new Vector2(0f, 20f);
        Text clText = craftLabel.AddComponent<Text>();
        clText.text = "― 合成 ―";
        clText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        clText.fontSize = 14;
        clText.color = new Color(0.7f, 0.7f, 0.7f);
        clText.alignment = TextAnchor.MiddleCenter;

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

        // 素材枠1
        CraftSlot craftSlot1 = CreateCraftSlotUI(slotContainer, "Slot1", "素材1");
        // ＋ ラベル
        CreateSlotLabel(slotContainer, "+Label", "＋");
        // 素材枠2
        CraftSlot craftSlot2 = CreateCraftSlotUI(slotContainer, "Slot2", "素材2");
        // ＝ ラベル
        CreateSlotLabel(slotContainer, "=Label", "＝");
        // 結果枠
        GameObject resultSlot = CreateResultSlotUI(slotContainer, "ResultSlot");
        Text resultText = resultSlot.GetComponentInChildren<Text>();

        // ============ 手札エリア（下部） ============
        GameObject handAreaObj = new GameObject("HandArea");
        handAreaObj.transform.SetParent(canvasObj.transform, false);

        RectTransform handRt = handAreaObj.AddComponent<RectTransform>();
        handRt.anchorMin = new Vector2(0f, 0f);
        handRt.anchorMax = new Vector2(0.7f, 0f);
        handRt.pivot = new Vector2(0f, 0f);
        handRt.anchoredPosition = new Vector2(10f, 10f);
        handRt.sizeDelta = new Vector2(0f, 110f);

        // 手札背景
        Image handBg = handAreaObj.AddComponent<Image>();
        handBg.color = new Color(0.05f, 0.05f, 0.08f, 0.8f);

        // HorizontalLayoutGroup
        HorizontalLayoutGroup handLayout = handAreaObj.AddComponent<HorizontalLayoutGroup>();
        handLayout.spacing = 8f;
        handLayout.padding = new RectOffset(10, 10, 10, 10);
        handLayout.childAlignment = TextAnchor.MiddleLeft;
        handLayout.childForceExpandWidth = false;
        handLayout.childForceExpandHeight = false;

        // レシピ表示（手札エリア右　→　右下）
        GameObject recipeTextObj = CreateTextUI(canvasObj, "RecipeText", "",
            new Vector2(-20f, 120f), new Vector2(300f, 100f), TextAnchor.LowerLeft);
        SetAnchors(recipeTextObj, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f));
        recipeTextObj.GetComponent<Text>().fontSize = 13;

        // インベントリテキスト（非表示だがGameHUDが参照する）
        GameObject invTextObj = CreateTextUI(canvasObj, "InventoryText", "",
            new Vector2(0f, 0f), new Vector2(1f, 1f), TextAnchor.UpperLeft);
        invTextObj.SetActive(false); // 非表示（カードUIが代替）

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
