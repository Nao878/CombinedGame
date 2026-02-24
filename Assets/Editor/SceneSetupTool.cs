using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using ZombieSurvival;

/// <summary>
/// ゾンビサバイバル ゲームのシーン一括セットアップツール
/// メニュー: Tools > Setup Zombie Survival
/// </summary>
public class SceneSetupTool : Editor
{
    [MenuItem("Tools/Setup Zombie Survival")]
    public static void SetupScene()
    {
        // 既存オブジェクトのクリーンアップ確認
        if (!EditorUtility.DisplayDialog(
            "Zombie Survival Setup",
            "シーンをセットアップします。既存のGameObjectは維持されます。\n実行しますか？",
            "実行", "キャンセル"))
        {
            return;
        }

        // 既存の重複を削除
        CleanupExisting();

        // 各オブジェクトの生成
        SetupGround();
        SetupLighting();
        SetupCamera();
        GameObject player = SetupPlayer();
        SetupItemPickups();
        SetupUI();

        // ゲーム仕様書の自動生成・更新
        GameDesignDocGenerator.Generate();

        // シーンを保存
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("[SceneSetup] ゾンビサバイバル シーンのセットアップが完了しました！");
    }

    /// <summary>
    /// 既存のセットアップオブジェクトを削除
    /// </summary>
    private static void CleanupExisting()
    {
        string[] objectNames = {
            "Player", "Ground", "Main Camera", "Directional Light",
            "GameCanvas", "GameManagers",
            "Pickup_ScrapMetal_1", "Pickup_ScrapMetal_2",
            "Pickup_Gunpowder_1", "Pickup_Gunpowder_2",
            "Pickup_Wood_1", "Pickup_Nail_1"
        };

        foreach (var name in objectNames)
        {
            var obj = GameObject.Find(name);
            if (obj != null)
            {
                DestroyImmediate(obj);
            }
        }
    }

    /// <summary>
    /// 地面の作成
    /// </summary>
    private static void SetupGround()
    {
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.position = Vector3.zero;
        ground.transform.localScale = new Vector3(5f, 1f, 5f); // 50x50の地面

        // 地面の色を暗い緑に
        Renderer rend = ground.GetComponent<Renderer>();
        if (rend != null)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(0.2f, 0.35f, 0.15f);
            rend.material = mat;
        }

        // 地面のコライダーにタグ設定なし（デフォルト）
    }

    /// <summary>
    /// 照明の設定
    /// </summary>
    private static void SetupLighting()
    {
        GameObject light = new GameObject("Directional Light");
        Light lightComp = light.AddComponent<Light>();
        lightComp.type = LightType.Directional;
        lightComp.color = new Color(1f, 0.95f, 0.84f);
        lightComp.intensity = 1.2f;
        lightComp.shadows = LightShadows.Soft;
        light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
    }

    /// <summary>
    /// カメラの設定（見下ろし視点）
    /// </summary>
    private static void SetupCamera()
    {
        GameObject camObj = new GameObject("Main Camera");
        camObj.tag = "MainCamera";
        Camera cam = camObj.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.1f, 0.1f, 0.15f);
        cam.fieldOfView = 60f;

        // 見下ろし位置
        camObj.transform.position = new Vector3(0f, 18f, -8f);
        camObj.transform.rotation = Quaternion.Euler(65f, 0f, 0f);

        // AudioListener
        camObj.AddComponent<AudioListener>();

        // カメラ追従スクリプト
        camObj.AddComponent<CameraFollow>();
    }

    /// <summary>
    /// プレイヤーの作成
    /// </summary>
    private static GameObject SetupPlayer()
    {
        // プレイヤー本体（Cube）
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Cube);
        player.name = "Player";
        player.tag = "Player";
        player.transform.position = new Vector3(0f, 0.5f, 0f);
        player.layer = LayerMask.NameToLayer("Default");

        // プレイヤーの色
        Renderer rend = player.GetComponent<Renderer>();
        if (rend != null)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(0.2f, 0.6f, 1f); // 青色
            rend.material = mat;
        }

        // 銃口の目印（小さなCube）
        GameObject muzzle = GameObject.CreatePrimitive(PrimitiveType.Cube);
        muzzle.name = "MuzzlePoint";
        muzzle.transform.SetParent(player.transform);
        muzzle.transform.localPosition = new Vector3(0f, 0f, 0.7f);
        muzzle.transform.localScale = new Vector3(0.15f, 0.15f, 0.3f);
        Renderer muzzleRend = muzzle.GetComponent<Renderer>();
        if (muzzleRend != null)
        {
            Material muzzleMat = new Material(Shader.Find("Standard"));
            muzzleMat.color = Color.red;
            muzzleRend.material = muzzleMat;
        }
        // 銃口のコライダーを無効化
        var muzzleCol = muzzle.GetComponent<Collider>();
        if (muzzleCol != null) DestroyImmediate(muzzleCol);

        // CharacterController
        CharacterController cc = player.AddComponent<CharacterController>();
        cc.height = 1f;
        cc.radius = 0.4f;
        cc.center = Vector3.zero;

        // スクリプトの追加
        player.AddComponent<PlayerController>();
        WeaponController weapon = player.AddComponent<WeaponController>();

        // firePointの設定（Serializeされているが、エディターで手動で設定が必要）
        // → リフレクションで設定
        var firePointField = typeof(WeaponController).GetField("firePoint",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (firePointField != null)
        {
            firePointField.SetValue(weapon, muzzle.transform);
        }

        // GameManagersオブジェクト（シングルトン群）
        GameObject managers = new GameObject("GameManagers");
        managers.AddComponent<InventoryManager>();
        managers.AddComponent<CraftingManager>();

        return player;
    }

    /// <summary>
    /// フィールドアイテムの配置
    /// </summary>
    private static void SetupItemPickups()
    {
        // 鉄くず x2
        CreatePickup("Pickup_ScrapMetal_1", "鉄くず", new Vector3(5f, 0.5f, 5f), PrimitiveType.Sphere);
        CreatePickup("Pickup_ScrapMetal_2", "鉄くず", new Vector3(-8f, 0.5f, 3f), PrimitiveType.Sphere);

        // 火薬 x2
        CreatePickup("Pickup_Gunpowder_1", "火薬", new Vector3(-5f, 0.5f, 8f), PrimitiveType.Sphere);
        CreatePickup("Pickup_Gunpowder_2", "火薬", new Vector3(7f, 0.5f, -6f), PrimitiveType.Sphere);

        // 木材 x1
        CreatePickup("Pickup_Wood_1", "木材", new Vector3(-3f, 0.5f, -7f), PrimitiveType.Cube);

        // 釘 x1
        CreatePickup("Pickup_Nail_1", "釘", new Vector3(10f, 0.5f, -2f), PrimitiveType.Cylinder);
    }

    /// <summary>
    /// アイテムピックアップオブジェクトの生成
    /// </summary>
    private static void CreatePickup(string objName, string itemName, Vector3 position, PrimitiveType shape)
    {
        GameObject pickup = GameObject.CreatePrimitive(shape);
        pickup.name = objName;
        pickup.transform.position = position;
        pickup.transform.localScale = Vector3.one * 0.5f;

        // トリガーコライダーに変更
        Collider col = pickup.GetComponent<Collider>();
        if (col != null)
        {
            DestroyImmediate(col);
        }

        // 大きめのトリガーコライダーを追加（取得範囲）
        SphereCollider trigger = pickup.AddComponent<SphereCollider>();
        trigger.isTrigger = true;
        trigger.radius = 2f;

        // 色設定
        ItemData data = ItemDatabase.GetByName(itemName);
        if (data != null)
        {
            Renderer rend = pickup.GetComponent<Renderer>();
            if (rend != null)
            {
                Material mat = new Material(Shader.Find("Standard"));
                mat.color = data.displayColor;

                // Emissionを有効にして光らせる
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", data.displayColor * 0.5f);

                rend.material = mat;
            }
        }

        // ItemPickupスクリプト追加
        ItemPickup itemPickup = pickup.AddComponent<ItemPickup>();
        itemPickup.SetItemName(itemName);
    }

    /// <summary>
    /// UI Canvasの作成
    /// </summary>
    private static void SetupUI()
    {
        // Canvas
        GameObject canvasObj = new GameObject("GameCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // === インベントリテキスト（左上）===
        GameObject invTextObj = CreateTextUI(canvasObj, "InventoryText",
            "--- インベントリ ---\n(空)",
            new Vector2(20f, -20f),
            new Vector2(300f, 200f),
            TextAnchor.UpperLeft);
        invTextObj.GetComponent<RectTransform>().anchorMin = new Vector2(0f, 1f);
        invTextObj.GetComponent<RectTransform>().anchorMax = new Vector2(0f, 1f);
        invTextObj.GetComponent<RectTransform>().pivot = new Vector2(0f, 1f);

        // === 操作説明テキスト（右下）===
        GameObject ctrlTextObj = CreateTextUI(canvasObj, "ControlsText",
            "--- 操作方法 ---\nWASD : 移動\nマウス : 照準\n左クリック : 射撃\nC : クラフト",
            new Vector2(-20f, 20f),
            new Vector2(280f, 160f),
            TextAnchor.LowerRight);
        ctrlTextObj.GetComponent<RectTransform>().anchorMin = new Vector2(1f, 0f);
        ctrlTextObj.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 0f);
        ctrlTextObj.GetComponent<RectTransform>().pivot = new Vector2(1f, 0f);

        // === メッセージテキスト（中央上）===
        GameObject msgTextObj = CreateTextUI(canvasObj, "MessageText",
            "",
            new Vector2(0f, -60f),
            new Vector2(600f, 50f),
            TextAnchor.MiddleCenter);
        msgTextObj.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 1f);
        msgTextObj.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 1f);
        msgTextObj.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1f);
        msgTextObj.GetComponent<Text>().fontSize = 22;
        msgTextObj.GetComponent<Text>().color = Color.yellow;

        // === レシピテキスト（左下）===
        GameObject recipeTextObj = CreateTextUI(canvasObj, "RecipeText",
            "",
            new Vector2(20f, 20f),
            new Vector2(350f, 120f),
            TextAnchor.LowerLeft);
        recipeTextObj.GetComponent<RectTransform>().anchorMin = new Vector2(0f, 0f);
        recipeTextObj.GetComponent<RectTransform>().anchorMax = new Vector2(0f, 0f);
        recipeTextObj.GetComponent<RectTransform>().pivot = new Vector2(0f, 0f);

        // === GameHUDスクリプト追加 ===
        GameHUD hud = canvasObj.AddComponent<GameHUD>();

        // リフレクションでテキスト参照を設定
        SetPrivateField(hud, "inventoryText", invTextObj.GetComponent<Text>());
        SetPrivateField(hud, "controlsText", ctrlTextObj.GetComponent<Text>());
        SetPrivateField(hud, "messageText", msgTextObj.GetComponent<Text>());
        SetPrivateField(hud, "recipeText", recipeTextObj.GetComponent<Text>());
    }

    /// <summary>
    /// テキストUIオブジェクトの生成ヘルパー
    /// </summary>
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

        // 背景（半透明黒）の追加
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

    /// <summary>
    /// プライベートフィールドにリフレクションで値を設定
    /// </summary>
    private static void SetPrivateField(object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(fieldName,
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(target, value);
        }
    }
}
