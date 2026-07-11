using System.IO;
using MonsterMaster.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class DialoguePopupPrefabBuilder
{
    private const string ScenePath = "Assets/Scenes/UI/main.unity";
    private const string PrefabPath = "Assets/Prefabs/UI/DialogueChoicePopup.prefab";
    private const string FontPath = "Assets/Art/Fonts/HaiPaiQiangDiaoSenXiYuan-Shan(ShanghaiFace-SenGBT-Flash)-2 SDF.asset";
    private static TMP_FontAsset font;

    [MenuItem("Tools/Monster Master/Build Dialogue Choice Popup")]
    public static void Build()
    {
        Directory.CreateDirectory("Assets/Prefabs/UI");
        font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        Canvas canvas = Object.FindObjectOfType<Canvas>();
        GameObject nurse = GameObject.Find("npc_nurse");
        if (canvas == null || nurse == null) throw new System.InvalidOperationException("Canvas or npc_nurse not found.");
        GameObjectUtility.RemoveMonoBehavioursWithMissingScript(nurse);

        Transform old = canvas.transform.Find("DialogueChoicePopup");
        if (old != null) Object.DestroyImmediate(old.gameObject);
        Transform legacy = canvas.transform.Find("NurseActionPopup");
        if (legacy != null) Object.DestroyImmediate(legacy.gameObject);
        AssetDatabase.DeleteAsset("Assets/Prefabs/UI/NurseActionPopup.prefab");

        GameObject root = UIObject("DialogueChoicePopup", null);
        Stretch(root.GetComponent<RectTransform>());
        Image overlay = root.AddComponent<Image>();
        overlay.color = new Color(0f, 0f, 0f, 0.48f);
        Button overlayButton = root.AddComponent<Button>();
        overlayButton.transition = Selectable.Transition.None;

        GameObject dialogue = UIObject("DialoguePanel", root.transform);
        SetRect(dialogue.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(1040f, 330f), new Vector2(0f, 55f), new Vector2(0.5f, 0f));
        Image dialogueImage = dialogue.AddComponent<Image>();
        dialogueImage.color = new Color(0.96f, 0.96f, 0.96f, 1f);

        GameObject namePlate = UIObject("NamePlate", dialogue.transform);
        SetRect(namePlate.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(220f, 62f), new Vector2(28f, -22f), new Vector2(0f, 1f));
        namePlate.AddComponent<Image>().color = new Color(0.55f, 0.75f, 0.67f, 1f);
        TMP_Text speaker = TextObject("SpeakerName", namePlate.transform, "兔子护士", 28, TextAlignmentOptions.Center);
        Stretch(speaker.rectTransform);

        TMP_Text body = TextObject("DialogueText", dialogue.transform, "今天想做什么呢？", 32, TextAlignmentOptions.TopLeft);
        SetRect(body.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(610f, 205f), new Vector2(42f, 28f), new Vector2(0f, 0f));

        GameObject options = UIObject("OptionsPanel", dialogue.transform);
        SetRect(options.GetComponent<RectTransform>(), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(330f, 270f), new Vector2(-32f, 0f), new Vector2(1f, 0.5f));
        options.AddComponent<Image>().color = new Color(0.89f, 0.89f, 0.89f, 1f);
        options.AddComponent<CanvasGroup>();
        VerticalLayoutGroup layout = options.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(18, 18, 18, 18);
        layout.spacing = 14f;
        layout.childControlHeight = false;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;
        Button optionTemplate = OptionButton(options.transform, "OptionButtonTemplate", "选项");

        DialogueChoicePopup popup = root.AddComponent<DialogueChoicePopup>();
        SerializedObject so = new SerializedObject(popup);
        so.FindProperty("speakerNameText").objectReferenceValue = speaker;
        so.FindProperty("dialogueText").objectReferenceValue = body;
        so.FindProperty("optionsPanel").objectReferenceValue = options.GetComponent<RectTransform>();
        so.FindProperty("optionButtonTemplate").objectReferenceValue = optionTemplate;
        so.ApplyModifiedPropertiesWithoutUndo();
        UnityEditor.Events.UnityEventTools.AddPersistentListener(overlayButton.onClick, popup.Close);

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, canvas.transform);
        instance.name = "DialogueChoicePopup";
        DialogueChoicePopup scenePopup = instance.GetComponent<DialogueChoicePopup>();

        DialoguePopupTrigger opener = nurse.GetComponent<DialoguePopupTrigger>();
        if (opener == null) opener = nurse.AddComponent<DialoguePopupTrigger>();
        SerializedObject openerSO = new SerializedObject(opener);
        openerSO.FindProperty("popup").objectReferenceValue = scenePopup;
        openerSO.FindProperty("dialogueId").stringValue = "nurse_main";
        openerSO.ApplyModifiedPropertiesWithoutUndo();
        instance.SetActive(false);

        EditorUtility.SetDirty(nurse);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("Built generic dialogue choice popup, config table, and nurse trigger.");
    }

    private static Button OptionButton(Transform parent, string name, string label)
    {
        GameObject go = UIObject(name, parent);
        go.AddComponent<LayoutElement>().preferredHeight = 68f;
        Image image = go.AddComponent<Image>();
        image.color = Color.white;
        Button button = go.AddComponent<Button>();
        button.targetGraphic = image;
        TMP_Text text = TextObject("Label", go.transform, label, 24, TextAlignmentOptions.Center);
        Stretch(text.rectTransform);
        return button;
    }

    private static TMP_Text TextObject(string name, Transform parent, string value, float size, TextAlignmentOptions alignment)
    {
        GameObject go = UIObject(name, parent);
        TextMeshProUGUI text = go.AddComponent<TextMeshProUGUI>();
        text.text = value;
        if (font != null) text.font = font;
        text.fontSize = size;
        text.alignment = alignment;
        text.color = new Color(0.12f, 0.12f, 0.12f, 1f);
        text.raycastTarget = false;
        return text;
    }

    private static GameObject UIObject(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.layer = LayerMask.NameToLayer("UI");
        if (parent != null) go.transform.SetParent(parent, false);
        return go;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
    }

    private static void SetRect(RectTransform rect, Vector2 min, Vector2 max, Vector2 size, Vector2 position, Vector2 pivot)
    {
        rect.anchorMin = min; rect.anchorMax = max; rect.sizeDelta = size;
        rect.anchoredPosition = position; rect.pivot = pivot;
    }
}
