using MonsterMaster.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public sealed class DialoguePortraitLayoutTool : EditorWindow
{
    private const string PrefabPath = "Assets/Prefabs/UI/DialogueChoicePopup.prefab";

    private int portraitId = 2;
    private string speaker = "雾雾";
    private Sprite portrait;
    private Vector2 anchoredPosition = new Vector2(0f, -81f);
    private float scale = 1f;
    private string status;

    [MenuItem("Tools/Monster Master/Dialogue Portrait Layout")]
    private static void Open()
    {
        GetWindow<DialoguePortraitLayoutTool>("Portrait Layout");
    }

    private void OnEnable()
    {
        LoadSpeaker();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Dialogue Portrait Layout", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Each numeric portrait ID keeps an independent sprite, position and scale in DialogueChoicePopup.prefab.",
            MessageType.Info);

        EditorGUI.BeginChangeCheck();
        portraitId = EditorGUILayout.IntField("Portrait ID", portraitId);
        if (EditorGUI.EndChangeCheck()) status = null;
        speaker = EditorGUILayout.TextField("Speaker", speaker);

        EditorGUI.BeginChangeCheck();
        portrait = (Sprite)EditorGUILayout.ObjectField("Portrait", portrait, typeof(Sprite), false);
        anchoredPosition = EditorGUILayout.Vector2Field("Position", anchoredPosition);
        scale = EditorGUILayout.FloatField("Scale", scale);
        if (EditorGUI.EndChangeCheck()) ApplyPreview();

        EditorGUILayout.Space(8f);
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Load Portrait ID")) LoadSpeaker();
            if (GUILayout.Button("Save Portrait ID")) SaveSpeaker();
        }

        if (!string.IsNullOrEmpty(status))
            EditorGUILayout.HelpBox(status, MessageType.None);
    }

    private void LoadSpeaker()
    {
        if (!TryGetPopup(out GameObject prefab, out DialogueChoicePopup popup)) return;

        SerializedObject serialized = new SerializedObject(popup);
        ApplyBuiltInIdentity(serialized);
        SerializedProperty layouts = serialized.FindProperty("portraitLayouts");
        RemoveInvalidLayouts(layouts);
        serialized.ApplyModifiedPropertiesWithoutUndo();
        int index = FindPortraitId(layouts, portraitId);
        if (index >= 0)
        {
            ReadLayout(layouts.GetArrayElementAtIndex(index));
            speaker = layouts.GetArrayElementAtIndex(index).FindPropertyRelative("speaker").stringValue;
            ApplyPreview();
            status = "Loaded portrait ID " + portraitId + ".";
        }
        else
        {
            Image image = serialized.FindProperty("portraitImage").objectReferenceValue as Image;
            if (image != null && portraitId > 2)
            {
                anchoredPosition = image.rectTransform.anchoredPosition;
                scale = image.rectTransform.localScale.x;
            }

            ApplyPreview();
            status = "Portrait ID " + portraitId + " has no saved layout yet. Default values are shown.";
        }
    }

    private void ApplyBuiltInIdentity(SerializedObject serialized)
    {
        if (portraitId == 1)
        {
            speaker = "兔子护士";
            portrait = serialized.FindProperty("nursePortrait").objectReferenceValue as Sprite;
            anchoredPosition = new Vector2(0f, -81f);
            scale = 1f;
        }
        else if (portraitId == 2)
        {
            speaker = "雾雾";
            portrait = serialized.FindProperty("wuwuPortrait").objectReferenceValue as Sprite;
            anchoredPosition = new Vector2(0f, -13.9f);
            scale = 2f;
        }
    }

    private void SaveSpeaker()
    {
        if (portraitId <= 0)
        {
            status = "Portrait ID must be greater than zero.";
            return;
        }

        if (!TryGetPopup(out GameObject prefab, out DialogueChoicePopup popup)) return;
        SerializedObject serialized = new SerializedObject(popup);
        SerializedProperty layouts = serialized.FindProperty("portraitLayouts");
        RemoveInvalidLayouts(layouts);
        int index = FindPortraitId(layouts, portraitId);
        if (index < 0)
        {
            index = layouts.arraySize;
            layouts.InsertArrayElementAtIndex(index);
        }

        SerializedProperty layout = layouts.GetArrayElementAtIndex(index);
        layout.FindPropertyRelative("id").intValue = portraitId;
        layout.FindPropertyRelative("speaker").stringValue = speaker.Trim();
        layout.FindPropertyRelative("portrait").objectReferenceValue = portrait;
        layout.FindPropertyRelative("anchoredPosition").vector2Value = anchoredPosition;
        layout.FindPropertyRelative("scale").floatValue = Mathf.Max(0.01f, scale);

        ApplyPreview(serialized);

        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(popup);
        PrefabStage stage = PrefabStageUtility.GetCurrentPrefabStage();
        if (stage != null && popup.gameObject.scene == stage.scene)
        {
            EditorSceneManager.MarkSceneDirty(stage.scene);
            PrefabUtility.SaveAsPrefabAsset(stage.prefabContentsRoot, stage.assetPath);
        }
        else
        {
            PrefabUtility.SavePrefabAsset(prefab);
        }
        AssetDatabase.SaveAssets();
        status = "Saved portrait ID " + portraitId + " to " + PrefabPath + ".";
    }

    private void ApplyPreview()
    {
        if (!TryGetPopup(out _, out DialogueChoicePopup popup)) return;
        ApplyPreview(new SerializedObject(popup));
    }

    private void ApplyPreview(SerializedObject serialized)
    {
        Behaviour animator = serialized.FindProperty("portraitAnimator").objectReferenceValue as Behaviour;
        if (animator != null)
        {
            animator.enabled = speaker.Trim() == "兔子护士";
            EditorUtility.SetDirty(animator);
        }

        Image image = serialized.FindProperty("portraitImage").objectReferenceValue as Image;
        if (image == null) return;

        image.gameObject.SetActive(portrait != null);
        image.sprite = portrait;
        image.enabled = portrait != null;
        image.preserveAspect = true;
        image.rectTransform.anchoredPosition = anchoredPosition;
        image.rectTransform.localScale = Vector3.one * Mathf.Max(0.01f, scale);
        EditorUtility.SetDirty(image);
        EditorUtility.SetDirty(image.rectTransform);
        SceneView.RepaintAll();
    }

    private void ReadLayout(SerializedProperty layout)
    {
        portrait = layout.FindPropertyRelative("portrait").objectReferenceValue as Sprite;
        anchoredPosition = layout.FindPropertyRelative("anchoredPosition").vector2Value;
        scale = layout.FindPropertyRelative("scale").floatValue;
    }

    private static int FindPortraitId(SerializedProperty layouts, int targetId)
    {
        for (int i = 0; i < layouts.arraySize; i++)
        {
            SerializedProperty entry = layouts.GetArrayElementAtIndex(i);
            if (entry.FindPropertyRelative("id").intValue == targetId) return i;
        }
        return -1;
    }

    private static void RemoveInvalidLayouts(SerializedProperty layouts)
    {
        for (int i = layouts.arraySize - 1; i >= 0; i--)
        {
            SerializedProperty entry = layouts.GetArrayElementAtIndex(i);
            if (entry.FindPropertyRelative("id").intValue <= 0)
                layouts.DeleteArrayElementAtIndex(i);
        }
    }

    private bool TryGetPopup(out GameObject prefab, out DialogueChoicePopup popup)
    {
        PrefabStage stage = PrefabStageUtility.GetCurrentPrefabStage();
        if (stage != null && stage.assetPath == PrefabPath)
        {
            prefab = stage.prefabContentsRoot;
            popup = prefab != null ? prefab.GetComponent<DialogueChoicePopup>() : null;
            if (popup != null) return true;
        }

        prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        popup = prefab != null ? prefab.GetComponent<DialogueChoicePopup>() : null;
        if (popup != null) return true;

        status = "Could not find DialogueChoicePopup at " + PrefabPath + ".";
        return false;
    }
}
