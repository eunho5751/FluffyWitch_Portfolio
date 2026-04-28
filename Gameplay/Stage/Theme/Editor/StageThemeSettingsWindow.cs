using UnityEditor;
using UnityEngine;

public class StageThemeSettingsWindow : EditorWindow
{
    private StageThemeSettings _target;
    private SerializedObject _serializedObject;

    private int _selectedTab;
    private Vector2 _scrollPosition;

    private static readonly string[] TabNames = { "Enemy Range Rules", "Hurdle Range Rules", "Boss Range Rules" };
    private static readonly Color RowColorEven = new Color(0f, 0f, 0f, 0.1f);
    private static readonly Color RowColorOdd = new Color(0f, 0f, 0f, 0.02f);
    private static GUIContent _errorIconContent;
    private static GUIStyle _boldToolbarButton;

    private StageThemeSettings[] _allSettings;

    [MenuItem("FluffyWitch/Stage Theme Settings")]
    private static void Open()
    {
        GetWindow<StageThemeSettingsWindow>("Stage Theme Settings");
    }

    private void OnEnable()
    {
        _errorIconContent = EditorGUIUtility.IconContent("console.erroricon.sml");
        RefreshSettingsList();
    }

    private void RefreshSettingsList()
    {
        var guids = AssetDatabase.FindAssets("t:StageThemeSettings");
        _allSettings = new StageThemeSettings[guids.Length];
        for (int i = 0; i < guids.Length; i++)
        {
            var path = AssetDatabase.GUIDToAssetPath(guids[i]);
            _allSettings[i] = AssetDatabase.LoadAssetAtPath<StageThemeSettings>(path);
        }

        if (_target == null && _allSettings.Length > 0)
            _target = _allSettings[0];

        if (_target != null)
            _serializedObject = new SerializedObject(_target);
    }

    private void OnGUI()
    {
        DrawTargetSelector();

        if (_serializedObject == null)
            return;

        _serializedObject.Update();

        EditorGUILayout.Space(4);
        DrawMaxStageField();
        EditorGUILayout.Space(8);

        _selectedTab = GUILayout.Toolbar(_selectedTab, TabNames);
        EditorGUILayout.Space(4);

        string arrayPropName = _selectedTab switch
        {
            0 => "_enemySpawnRules",
            1 => "_hurdleSpawnRules",
            _ => "_bossSpawnRules",
        };
        var arrayProp = _serializedObject.FindProperty(arrayPropName);

        DrawToolbar(arrayProp);
        EditorGUILayout.Space(4);

        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

        int maxStage = _serializedObject.FindProperty("_maxStage").intValue;

        switch (_selectedTab)
        {
            case 0: DrawEnemyTable(arrayProp, maxStage); break;
            case 1: DrawHurdleTable(arrayProp, maxStage); break;
            case 2: DrawBossTable(arrayProp, maxStage); break;
        }

        EditorGUILayout.EndScrollView();

        if (_serializedObject.ApplyModifiedProperties())
        {
            // Trigger OnValidate to recalculate editor context
            EditorUtility.SetDirty(_target);
        }
    }

    private void DrawTargetSelector()
    {
        EditorGUILayout.BeginHorizontal();

        if (_allSettings == null || _allSettings.Length == 0)
        {
            EditorGUILayout.HelpBox("스테이지 테마 설정 에셋을 만들어주세요.", MessageType.Warning);
            if (GUILayout.Button("↻", GUILayout.Width(24), GUILayout.Height(20)))
                RefreshSettingsList();
            EditorGUILayout.EndHorizontal();
            return;
        }

        for (int i = 0; i < _allSettings.Length; i++)
        {
            var settings = _allSettings[i];
            if (settings == null) continue;

            bool isSelected = _target == settings;

            EditorGUI.BeginChangeCheck();
            if (_boldToolbarButton == null)
            {
                _boldToolbarButton = new GUIStyle(EditorStyles.toolbarButton);
                _boldToolbarButton.fontStyle = FontStyle.Bold;
            }
            GUILayout.Toggle(isSelected, settings.name, _boldToolbarButton);
            if (EditorGUI.EndChangeCheck())
            {
                _target = settings;
                _serializedObject = new SerializedObject(_target);
                _scrollPosition = Vector2.zero;
            }
        }

        GUILayout.FlexibleSpace();

        if (GUILayout.Button("↻", GUILayout.Width(24), GUILayout.Height(20)))
            RefreshSettingsList();

        EditorGUILayout.EndHorizontal();
    }

    private void DrawMaxStageField()
    {
        var maxStageProp = _serializedObject.FindProperty("_maxStage");
        EditorGUILayout.PropertyField(maxStageProp, new GUIContent("Max Stage"));
        if (maxStageProp.intValue < 1)
            maxStageProp.intValue = 1;
    }

    private void DrawToolbar(SerializedProperty arrayProp)
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        if (GUILayout.Button("+  Add Rule", GUILayout.Width(100), GUILayout.Height(22)))
        {
            arrayProp.InsertArrayElementAtIndex(arrayProp.arraySize);
        }

        if (GUILayout.Button("-  Remove Last", GUILayout.Width(110), GUILayout.Height(22)))
        {
            if (arrayProp.arraySize > 0)
                arrayProp.DeleteArrayElementAtIndex(arrayProp.arraySize - 1);
        }

        EditorGUILayout.EndHorizontal();
    }

    // ─────────────────────────────────────────────
    //  Enemy Table
    // ─────────────────────────────────────────────

    private void DrawEnemyTable(SerializedProperty arrayProp, int maxStage)
    {
        if (arrayProp.arraySize == 0)
        {
            EditorGUILayout.HelpBox("규칙이 없습니다. + Add Rule 로 추가하세요.", MessageType.None);
            return;
        }

        // Header
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.Label("#", EditorStyles.miniLabel, GUILayout.Width(90));
        GUILayout.Label("Stage Range", EditorStyles.miniLabel, GUILayout.Width(80));
        GUILayout.Label("Interval", EditorStyles.miniLabel, GUILayout.Width(70));
        GUILayout.Label("Count", EditorStyles.miniLabel, GUILayout.Width(60));
        GUILayout.Label("Spawn Entries", EditorStyles.miniLabel);
        GUILayout.Label("", GUILayout.Width(20));
        EditorGUILayout.EndHorizontal();

        int removeIndex = -1;
        int startStage = 1;
        for (int i = 0; i < arrayProp.arraySize; i++)
        {
            var element = arrayProp.GetArrayElementAtIndex(i);
            var stageRangeProp = element.FindPropertyRelative("_stageRange");
            var ruleProp = element.FindPropertyRelative("_rule");

            int range = stageRangeProp.intValue;
            bool overflow = startStage + range - 1 > maxStage;
            string label = $"{startStage} ~ {startStage + range - 1}";

            var rowRect = EditorGUILayout.BeginHorizontal();
            EditorGUI.DrawRect(rowRect, i % 2 == 0 ? RowColorEven : RowColorOdd);

            // Error icon
            if (overflow)
            {
                _errorIconContent.tooltip = $"Max Stage({maxStage})를 초과합니다.";
                GUILayout.Label(_errorIconContent, GUILayout.Width(18), GUILayout.Height(18));
            }

            // Stage Label (read-only)
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField(label, GUILayout.Width(overflow ? 72 : 90));
            EditorGUI.EndDisabledGroup();

            // Stage Range
            EditorGUILayout.PropertyField(stageRangeProp, GUIContent.none, GUILayout.Width(80));
            if (stageRangeProp.intValue < 1) stageRangeProp.intValue = 1;

            // Interval
            var intervalProp = ruleProp.FindPropertyRelative("_spawnInterval");
            EditorGUILayout.PropertyField(intervalProp, GUIContent.none, GUILayout.Width(70));
            if (intervalProp.floatValue < 0f) intervalProp.floatValue = 0f;

            // Count
            var countProp = ruleProp.FindPropertyRelative("_spawnCount");
            EditorGUILayout.PropertyField(countProp, GUIContent.none, GUILayout.Width(60));

            // Spawn Entries
            var entriesProp = ruleProp.FindPropertyRelative("_spawnEntries");
            DrawSpawnEntriesCompact(entriesProp);

            // Remove button
            if (GUILayout.Button("X", GUILayout.Width(20), GUILayout.Height(18)))
                removeIndex = i;

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(4);

            startStage += range;
        }

        if (removeIndex >= 0)
            arrayProp.DeleteArrayElementAtIndex(removeIndex);
    }

    // ─────────────────────────────────────────────
    //  Hurdle Table
    // ─────────────────────────────────────────────

    private void DrawHurdleTable(SerializedProperty arrayProp, int maxStage)
    {
        if (arrayProp.arraySize == 0)
        {
            EditorGUILayout.HelpBox("규칙이 없습니다. + Add Rule 로 추가하세요.", MessageType.None);
            return;
        }

        // Header
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.Label("#", EditorStyles.miniLabel, GUILayout.Width(90));
        GUILayout.Label("Stage Range", EditorStyles.miniLabel, GUILayout.Width(80));
        GUILayout.Label("Interval", EditorStyles.miniLabel, GUILayout.Width(70));
        GUILayout.Label("Gap", EditorStyles.miniLabel, GUILayout.Width(60));
        GUILayout.Label("Speed", EditorStyles.miniLabel, GUILayout.Width(60));
        GUILayout.Label("Spawn Entries", EditorStyles.miniLabel);
        GUILayout.Label("", GUILayout.Width(20));
        EditorGUILayout.EndHorizontal();

        int removeIndex = -1;
        int startStage = 1;
        for (int i = 0; i < arrayProp.arraySize; i++)
        {
            var element = arrayProp.GetArrayElementAtIndex(i);
            var stageRangeProp = element.FindPropertyRelative("_stageRange");
            var ruleProp = element.FindPropertyRelative("_rule");

            int range = stageRangeProp.intValue;
            bool overflow = startStage + range - 1 > maxStage;
            string label = $"{startStage} ~ {startStage + range - 1}";

            var rowRect = EditorGUILayout.BeginHorizontal();
            EditorGUI.DrawRect(rowRect, i % 2 == 0 ? RowColorEven : RowColorOdd);

            // Error icon
            if (overflow)
            {
                _errorIconContent.tooltip = $"Max Stage({maxStage})를 초과합니다.";
                GUILayout.Label(_errorIconContent, GUILayout.Width(18), GUILayout.Height(18));
            }

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField(label, GUILayout.Width(overflow ? 72 : 90));
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.PropertyField(stageRangeProp, GUIContent.none, GUILayout.Width(80));
            if (stageRangeProp.intValue < 1) stageRangeProp.intValue = 1;

            var intervalProp = ruleProp.FindPropertyRelative("_spawnInterval");
            EditorGUILayout.PropertyField(intervalProp, GUIContent.none, GUILayout.Width(70));
            if (intervalProp.floatValue < 0f) intervalProp.floatValue = 0f;

            var gapProp = ruleProp.FindPropertyRelative("_gap");
            EditorGUILayout.PropertyField(gapProp, GUIContent.none, GUILayout.Width(60));

            var speedProp = ruleProp.FindPropertyRelative("_speed");
            EditorGUILayout.PropertyField(speedProp, GUIContent.none, GUILayout.Width(60));

            var entriesProp = ruleProp.FindPropertyRelative("_spawnEntries");
            DrawSpawnEntriesCompact(entriesProp);

            if (GUILayout.Button("X", GUILayout.Width(20), GUILayout.Height(18)))
                removeIndex = i;

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(4);

            startStage += range;
        }

        if (removeIndex >= 0)
            arrayProp.DeleteArrayElementAtIndex(removeIndex);
    }

    // ─────────────────────────────────────────────
    //  Boss Table
    // ─────────────────────────────────────────────

    private void DrawBossTable(SerializedProperty arrayProp, int maxStage)
    {
        if (arrayProp.arraySize == 0)
        {
            EditorGUILayout.HelpBox("규칙이 없습니다. + Add Rule 로 추가하세요.", MessageType.None);
            return;
        }

        // Header
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.Label("#", EditorStyles.miniLabel, GUILayout.Width(90));
        GUILayout.Label("Stage Range", EditorStyles.miniLabel, GUILayout.Width(80));
        GUILayout.Label("Interval", EditorStyles.miniLabel, GUILayout.Width(70));
        GUILayout.Label("Spawn Entries", EditorStyles.miniLabel);
        GUILayout.Label("", GUILayout.Width(20));
        EditorGUILayout.EndHorizontal();

        int removeIndex = -1;
        int startStage = 1;
        for (int i = 0; i < arrayProp.arraySize; i++)
        {
            var element = arrayProp.GetArrayElementAtIndex(i);
            var stageRangeProp = element.FindPropertyRelative("_stageRange");
            var ruleProp = element.FindPropertyRelative("_rule");

            int range = stageRangeProp.intValue;
            bool overflow = startStage + range - 1 > maxStage;
            string label = $"{startStage} ~ {startStage + range - 1}";

            var rowRect = EditorGUILayout.BeginHorizontal();
            EditorGUI.DrawRect(rowRect, i % 2 == 0 ? RowColorEven : RowColorOdd);

            if (overflow)
            {
                _errorIconContent.tooltip = $"Max Stage({maxStage})를 초과합니다.";
                GUILayout.Label(_errorIconContent, GUILayout.Width(18), GUILayout.Height(18));
            }

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField(label, GUILayout.Width(overflow ? 72 : 90));
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.PropertyField(stageRangeProp, GUIContent.none, GUILayout.Width(80));
            if (stageRangeProp.intValue < 1) stageRangeProp.intValue = 1;

            var intervalProp = ruleProp.FindPropertyRelative("_spawnInterval");
            EditorGUILayout.PropertyField(intervalProp, GUIContent.none, GUILayout.Width(70));
            if (intervalProp.floatValue < 0f) intervalProp.floatValue = 0f;

            var entriesProp = ruleProp.FindPropertyRelative("_spawnEntries");
            DrawSpawnEntriesCompact(entriesProp);

            if (GUILayout.Button("X", GUILayout.Width(20), GUILayout.Height(18)))
                removeIndex = i;

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(4);

            startStage += range;
        }

        if (removeIndex >= 0)
            arrayProp.DeleteArrayElementAtIndex(removeIndex);
    }

    // ─────────────────────────────────────────────
    //  Spawn Entries (compact inline)
    // ─────────────────────────────────────────────

    private void DrawSpawnEntriesCompact(SerializedProperty entriesProp)
    {
        int removeIndex = -1;

        EditorGUILayout.BeginVertical();

        for (int j = 0; j < entriesProp.arraySize; j++)
        {
            var entry = entriesProp.GetArrayElementAtIndex(j);
            var entityProp = entry.FindPropertyRelative("_entity");
            var weightProp = entry.FindPropertyRelative("_weight");
            var dataProp = entityProp.FindPropertyRelative("_data");

            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(dataProp, GUIContent.none, GUILayout.MinWidth(100));
            if (EditorGUI.EndChangeCheck())
            {
                var guidProp = entityProp.FindPropertyRelative("_guid");
                var dataObj = dataProp.objectReferenceValue;
                guidProp.stringValue = dataObj != null
                    ? AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(dataObj))
                    : "";
            }

            EditorGUILayout.PropertyField(weightProp, GUIContent.none, GUILayout.Width(40));

            if (GUILayout.Button("-", GUILayout.Width(18), GUILayout.Height(16)))
                removeIndex = j;

            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button("+ Entry", EditorStyles.miniButton, GUILayout.Width(60)))
        {
            entriesProp.InsertArrayElementAtIndex(entriesProp.arraySize);
        }

        EditorGUILayout.EndVertical();

        if (removeIndex >= 0)
            entriesProp.DeleteArrayElementAtIndex(removeIndex);
    }
}
