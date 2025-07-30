#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;
using static TilemapEditor;

[Serializable]
public class TilemapEditor : EditorWindow
{
    public static TilemapEditor instance { get; private set; }

    private int tabIndex = 0; // Selected window tab.
    private int tilemapIndex = 0; // Selected tilemap index.
    private int selectionModeIndex = 0; // Selection mode index.

    private Grid selectedGrid; // Selected grid.
    private Tilemap selectedTilemap; // Selected tilemap.

    private Vector3 cellSize = Vector3.one; // Size of one cell based on cell size.
    private Vector3 cellSelectionSize = Vector3.one; // Size of selection area.

    private int brushTileTypeIndex = 0; // Brush tile type index.
    private bool isBrushSizing = false; // Determine if user already set position 1 for brush size.
    private Vector3 brushSizePos1 = Vector3.zero; // Position 1 of setting brush size.
    private Vector3 brushSizePos2 = Vector3.zero; // Position 2 of setting brush size.
    private TileTool previousTool = TileTool.Paint; // Saved previous tool while changing Brush size.
    private float brushSizeY = 0f; // Brush size in Y axis.
    private Vector3 brushLimitSize = Vector3.one * 10f; // Brush size limitation in each dimension.

    private GameObject currentPreviewObject; // Current transparent object
    private List<Vector3> paintedPositions = new List<Vector3>(); // Array of position that user is currently painting.
    private int paletteFolderIndex = 0; // Palette selection index.

    private List<TilemapPalette> palettesList = new List<TilemapPalette>(); // List of prefab object in palettes. (path, prefab)
    private List<TilemapPalette> paletteSelectedList = new List<TilemapPalette>(); // Palette objects that being selected.

    private Vector2 paletteScrollPosition; // Scroll bar position of palette window.

    private string searchText = ""; // Current search text in search field.
    private bool isDragging = false; // Determine if user is dragging asset or not.

    private TileTool currentTool = TileTool.Paint; // Current selected tool.

    private TilemapSetting setting;
    private TilemapGroupBrush groupBrush;
    private SerializedObject groupBrushSerializedObject;

    public string TilemapPath => setting.path;

    private enum TileTool
    {
        None = 0,
        Paint = 1,
        Eraser = 2,
        Dropper = 3,
        Brush_Size = 4
    }

    private enum Vector3Axis
    {
        X, Y, Z
    }

    public enum BrushTileType
    {
        Center, Corner
    }

    [MenuItem("3D Tilemap Editor/Show Window")]
    public static void ShowWindow()
    {
        GetWindow<TilemapEditor>(false, "3D Tilemap Editor");
    }

    [MenuItem("3D Tilemap Editor/Clear Data")]
    public static void ClearData()
    {
        ClearTilemapData();
    }

    private void OnEnable()
    {
        instance = this;

        // If there's no tilemap data, generate new one.
        if (setting == null)
        {
            setting = new TilemapSetting();
        }

        // If there's no groupbrush data, generate new one.
        if (!groupBrush)
        {
            groupBrush = ScriptableObject.CreateInstance<TilemapGroupBrush>();
            groupBrushSerializedObject = new SerializedObject(groupBrush);
            groupBrush.serializedObject = groupBrushSerializedObject;
        }

        LoadTilemapData();
    }

    private void OnDisable()
    {
        SaveTilemapData();
    }

    private void Update()
    {
        // If the game is playing, destroy the previewObject
        if (Application.isPlaying)
        {
            DestroyPreview();
        }
    }

    private void OnFocus()
    {
        // Make the scene keep refeshing.
        SceneView.duringSceneGui -= OnSceneGUI;
        SceneView.duringSceneGui += OnSceneGUI;

        RefreshPalette();
    }

    private void OnDestroy()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        DestroyPreview();
    }

    private void OnGUI()
    {
        if (!groupBrush)
        {
            groupBrush = ScriptableObject.CreateInstance<TilemapGroupBrush>();
        }

        if (groupBrushSerializedObject == null || groupBrush.serializedObject == null)
        {
            groupBrushSerializedObject = new SerializedObject(groupBrush);
            groupBrush.serializedObject = groupBrushSerializedObject;
        }

        EditorGUILayout.Space(10f);

        EditorGUI.indentLevel++;

        #region Tab Selection

        Rect tabRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(EditorGUIUtility.singleLineHeight * 1.5f));

        float tabMargin = 10f;

        tabRect.x += tabMargin;
        tabRect.width -= tabMargin * 2f;

        string[] tabs = new string[] { "Settings", "Palette" };
        tabIndex = GUI.Toolbar(tabRect, tabIndex, tabs);

        #endregion

        EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);

        switch (tabIndex)
        {
            case 0:
            SettingsTab();
            break;

            case 1:
            PaletteTab();
            break;
        }

        DragAndDropHandler();
    }

    // Function to draw Settings tab.
    private void SettingsTab()
    {
        GUIStyle titleStyle = new GUIStyle();
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.normal.textColor = Color.white;

        #region Selected Tilemap

        List<Tilemap> tileMaps = new List<Tilemap>(FindObjectsOfType<Tilemap>());

        // If tilemap is disabled, don't add into list.
        for (int i = 0; i < tileMaps.Count; i++)
        {
            if (!tileMaps[i].gameObject.activeInHierarchy)
            {
                tileMaps.RemoveAt(i);
            }
        }

        string[] tilemapList = new string[tileMaps.Count];

        for (int i = 0; i < tileMaps.Count; i++)
        {
            tilemapList[i] = tileMaps[i].gameObject.name;
        }

        // If there's no tilemap and grid, disable everything.
        if (tileMaps.Count == 0)
        {
            EditorGUILayout.HelpBox("There's no Grid and Tilemap in the game scene!", MessageType.Error);
            tilemapIndex = 0;

            // Generate tilemap button.
            Rect buttonRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(EditorGUIUtility.singleLineHeight * 2f));

            float buttonWidth = 200f;
            buttonRect.x += (buttonRect.width / 2f) - (buttonWidth / 2f);
            buttonRect.width = buttonWidth;

            // [Generate Tilemap] - Button to generate tilemap.
            if (GUI.Button(buttonRect, "Generate Tilemap"))
            {
                GameObject gridObject = new GameObject("Grid");
                Grid grid = gridObject.AddComponent<Grid>();
                grid.cellSwizzle = GridLayout.CellSwizzle.XZY;

                GameObject tilemapObject = new GameObject("Tilemap");
                tilemapObject.transform.SetParent(gridObject.transform);
                tilemapObject.AddComponent<Tilemap>();

                Undo.RegisterCreatedObjectUndo(gridObject, "Create Tilemap");
            }

            DestroyPreview();
            return;
        }
        else
        {
            // If index is exceed maximum amount of tilemap, set to maximum.
            if (tilemapIndex >= tileMaps.Count)
            {
                tilemapIndex = tileMaps.Count - 1;
            }
        }

        GUIContent tilemapContent = new GUIContent("Selected Tilemap");
        tilemapContent.tooltip = "The focus tilemap to modify objects in that tilemap";
        tilemapIndex = EditorGUILayout.Popup(tilemapContent, tilemapIndex, tilemapList);

        selectedTilemap = tileMaps.Count > 0 ? tileMaps[tilemapIndex] : null;
        selectedGrid = selectedTilemap ? selectedTilemap.layoutGrid : null;

        #endregion

        #region Selection Mode

        string[] selectionModeList = new string[] { "Auto", "Manual" };
        GUIContent selectionContent = new GUIContent("Selection Mode");
        selectionContent.tooltip = "Selection based on options:\n- 'Auto' will select tile based on cursor's position\n- 'Manual' will select tile with offset";
        selectionModeIndex = EditorGUILayout.Popup(selectionContent, selectionModeIndex, selectionModeList);

        // If it's Manual mode, show Offset variables.
        if (selectionModeIndex == 1)
        {
            // Floor
            Rect floorRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(EditorGUIUtility.singleLineHeight));
            floorRect.x += 10f;
            GUIContent floorLabel = new GUIContent("Floor");
            floorLabel.tooltip = "Floor level from grid's position";
            EditorGUI.LabelField(floorRect, floorLabel);

            floorRect.x = 140f;
            floorRect.width = position.width - 143f;

            setting.gridFloor = EditorGUI.IntField(floorRect, GUIContent.none, setting.gridFloor);

            EditorGUILayout.Space(EditorGUIUtility.singleLineHeight * 0.15f);

            // Grid Offset
            Rect offsetRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(EditorGUIUtility.singleLineHeight));
            offsetRect.x += 10f;
            GUIContent offsetLabel = new GUIContent("Grid Offset");
            offsetLabel.tooltip = "Offset from grid";
            EditorGUI.LabelField(offsetRect, offsetLabel);

            offsetRect.x = 140f;
            offsetRect.width = position.width - 143f;

            setting.gridOffset = EditorGUI.Vector2Field(offsetRect, GUIContent.none, setting.gridOffset);
        }

        #endregion

        EditorGUILayout.Space(10f);

        EditorGUILayout.BeginHorizontal();

        // Combine Button
        GUIContent combineLabel = new GUIContent("Combine");
        combineLabel.tooltip = "Combine the tile with TileCombiner (If there's script in the tilemap)";

        if (GUILayout.Button(combineLabel, GUILayout.Height(EditorGUIUtility.singleLineHeight * 1.5f)))
        {
            TileCombiner tileCombiner;

            if (selectedTilemap && selectedTilemap.TryGetComponent(out tileCombiner))
            {
                tileCombiner.Combine();
            }
            else
            {
                Debug.LogWarning("There's no tilemap or Tile Combiner in the tilemap. Please check and try again!");
            }
        }

        // Clear Button
        GUIContent clearLabel = new GUIContent("Clear");
        clearLabel.tooltip = "Clear the tile with TileCombiner (If there's script in the tilemap)";

        if (GUILayout.Button(clearLabel, GUILayout.Height(EditorGUIUtility.singleLineHeight * 1.5f)))
        {
            TileCombiner tileCombiner;

            if (selectedTilemap && selectedTilemap.TryGetComponent(out tileCombiner))
            {
                tileCombiner.Clear();
            }
            else
            {
                Debug.LogWarning("There's no tilemap or Tile Combiner in the tilemap. Please check and try again!");
            }
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(10f);

        #region Brush Settings

        EditorGUILayout.LabelField("Brush Settings", titleStyle);

        // Brush Tile Type
        string[] tileMapTypeList = new string[] { "Center", "Corner" };
        GUIContent tileMapTypeContent = new GUIContent("Brush Tile Type");
        tileMapTypeContent.tooltip = "Tile selection based on options:\n- 'Center' will select the center of the tile\n- 'Corner' will select the corner of the tile";
        brushTileTypeIndex = EditorGUILayout.Popup(tileMapTypeContent, brushTileTypeIndex, tileMapTypeList);

        // Brush layer
        GUIContent brushLayerLabel = new GUIContent("Brush Layer");
        brushLayerLabel.tooltip = "Layer that will focus while using tilemap tool";

        List<string> layers = new List<string>();
        List<int> layersNum = new List<int>();

        // Filter to only mask with names.
        for (int i = 0; i < 32; i++)
        {
            string layerName = LayerMask.LayerToName(i);

            if (!layerName.Equals(""))
            {
                layers.Add(layerName);
                layersNum.Add(i);
            }
        }

        int maskWithoutEmpty = 0;

        for (int i = 0; i < layersNum.Count; i++)
        {
            if (((1 << layersNum[i]) & setting.brushLayer.value) > 0)
            {
                maskWithoutEmpty |= (1 << i);
            }
        }

        maskWithoutEmpty = EditorGUILayout.MaskField(brushLayerLabel, maskWithoutEmpty, layers.ToArray());

        int mask = 0;

        // Convert mask back to default format (32 layer which including empty layers)
        for (int i = 0; i < layersNum.Count; i++)
        {
            if ((maskWithoutEmpty & (1 << i)) > 0)
            {
                mask |= (1 << layersNum[i]);
            }
        }

        setting.brushLayer.value = mask;

        // Brush Offset
        Rect brushOffsetRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(EditorGUIUtility.singleLineHeight));
        brushOffsetRect.x += 3f;
        GUIContent brushOffsetLabel = new GUIContent("Brush Offset");
        brushOffsetLabel.tooltip = "Offset from selected tile";
        float brushOffsetWidth = GUI.skin.label.CalcSize(brushOffsetLabel).x;
        EditorGUI.LabelField(brushOffsetRect, brushOffsetLabel);

        brushOffsetRect.x += brushOffsetWidth + 65f;
        brushOffsetRect.width = brushOffsetRect.width - brushOffsetWidth - 71f;

        setting.brushOffset = EditorGUI.Vector3Field(brushOffsetRect, GUIContent.none, setting.brushOffset);

        EditorGUILayout.Space(3f);

        // Rotation
        Rect brushRotationRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(EditorGUIUtility.singleLineHeight));
        brushRotationRect.x += 3f;
        brushRotationRect.width = (brushRotationRect.width - 3f) / 2f;

        GUIContent brushRotationLabel = new GUIContent("Brush Rotation");
        brushRotationLabel.tooltip = "Rotate the brush";
        setting.brushRotation = EditorGUI.FloatField(brushRotationRect, brushRotationLabel, setting.brushRotation);

        brushRotationRect.x += brushRotationRect.width;
        brushRotationRect.width -= 3f;

        GUIContent brushRotationAmountLabel = new GUIContent("Rotate by");
        brushRotationAmountLabel.tooltip = "Amount of rotation degree when press [R]";
        setting.brushRotationAmount = EditorGUI.FloatField(brushRotationRect, brushRotationAmountLabel, setting.brushRotationAmount);
        setting.brushRotationAmount = Mathf.Clamp(setting.brushRotationAmount, -360f, 360f);

        if (setting.brushRotation >= 360f)
        {
            setting.brushRotation = 0f;
        }

        EditorGUILayout.Space(10f);

        // Reset Button
        GUIContent resetLabel = new GUIContent("Reset to Default");
        resetLabel.tooltip = "Reset Brush Offset, Brush Rotation, Rotate by to default value";

        if (GUILayout.Button(resetLabel, GUILayout.Height(EditorGUIUtility.singleLineHeight * 1.5f)))
        {
            setting.brushOffset = Vector3.zero;
            setting.brushRotation = 0f;
            setting.brushRotationAmount = 45f;
        }

        EditorGUILayout.Space(10f);

        #endregion

        #region Grid

        EditorGUILayout.LabelField("Grid Settings", titleStyle);

        // Show Grid
        GUIContent showGridLabel = new GUIContent("Show Grid");
        showGridLabel.tooltip = "Determine to show grid at selected cell or not";
        setting.showGrid = EditorGUILayout.Toggle(showGridLabel, setting.showGrid);

        GUI.enabled = setting.showGrid;

        // Grid Color
        GUIContent gridColorLabel = new GUIContent("Color");
        gridColorLabel.tooltip = "Color of the grid";
        setting.gridColor = EditorGUILayout.ColorField(gridColorLabel, setting.gridColor);

        // Grid Thickness
        GUIContent gridThicknessLabel = new GUIContent("Line Thickness");
        gridThicknessLabel.tooltip = "Thickness of lines";
        setting.gridThickness = EditorGUILayout.Slider(gridThicknessLabel, setting.gridThickness, 0.01f, 0.1f);

        GUI.enabled = true;

        #endregion
    }

    // Function to draw Group Brush tab.
    public void GroupBrush()
    {
        EditorGUILayout.Space(5f);
        
        #region Density

        GUIContent densityLabel = new GUIContent("Density");
        densityLabel.tooltip = "Amount of density/prefab to paint on a single tile";
        groupBrush.density = EditorGUILayout.IntField(densityLabel, groupBrush.density);
        groupBrush.density = Mathf.Clamp(groupBrush.density, 1, 20);

        #endregion

        #region Random Rotation

        GUIContent randomRotateLabel = new GUIContent("Random Rotation");
        randomRotateLabel.tooltip = "Determine to random rotation or not";
        groupBrush.randomRotation = EditorGUILayout.Toggle(randomRotateLabel, groupBrush.randomRotation);

        #endregion
    }

    // Function to draw Palette tab.
    private void PaletteTab()
    {
        GUIStyle titleStyle = new GUIStyle();
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.normal.textColor = Color.white;

        #region Group Brush

        Rect groupBrushHeaderRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(EditorGUIUtility.singleLineHeight * 1.5f));
        groupBrushHeaderRect.x += 3f;
        groupBrushHeaderRect.width -= 6f;

        Color defaultColor = GUI.backgroundColor;

        GUI.backgroundColor = groupBrush.isEnable ? Color.green : defaultColor;
        GUI.Box(groupBrushHeaderRect, GUIContent.none, GUI.skin.window);

        Rect buttonRect = groupBrushHeaderRect;
        buttonRect.x += 40f;
        buttonRect.width -= 40f;

        GUIContent groupBrushLabel = new GUIContent("Group Brush");
        groupBrushLabel.tooltip = "Paint multiple palettes in one brush";

        GUIStyle groupBrushLabelStyle = new GUIStyle(GUI.skin.box);
        groupBrushLabelStyle.normal.textColor = Color.white;
        groupBrushLabelStyle.fontStyle = FontStyle.Bold;
        groupBrushLabelStyle.alignment = TextAnchor.MiddleCenter;

        // Button to expand/collapse the group brush section.
        if (GUI.Button(buttonRect, groupBrushLabel, groupBrushLabelStyle))
        {
            groupBrush.isExpand = !groupBrush.isExpand;
        }

        GUIContent clickToExpandLabel = new GUIContent(groupBrush.isExpand ? "Click to Collapse" : "Click to Expand");
        float clickToExpandWidth = GUI.skin.label.CalcSize(clickToExpandLabel).x;

        GUIStyle clickToExpandStyle = new GUIStyle();
        clickToExpandStyle.normal.textColor = Color.gray;
        clickToExpandStyle.fontStyle = FontStyle.Italic;
        clickToExpandStyle.alignment = TextAnchor.MiddleRight;

        Rect clickToExpandRect = groupBrushHeaderRect;
        clickToExpandRect.x = groupBrushHeaderRect.width - clickToExpandWidth - 10f;
        clickToExpandRect.width = clickToExpandWidth;

        EditorGUI.LabelField(clickToExpandRect, clickToExpandLabel, clickToExpandStyle);

        GUI.backgroundColor = defaultColor;

        Rect enableRect = groupBrushHeaderRect;
        enableRect.x = 20f;
        enableRect.width = 10f;

        groupBrush.isEnable = GUI.Toggle(enableRect, groupBrush.isEnable, GUIContent.none);

        if (groupBrush.isExpand)
        {
            GroupBrush();
        }

        #endregion

        EditorGUILayout.Space(10f);

        #region Toolbar

        GUIContent[] toolBar = new GUIContent[] {
            GetToolTab("Paint Tool (B)", "Paint Icon"),
            GetToolTab("Eraser Tool (E)", "Eraser Icon"),
            GetToolTab("Dropper Tool (I)", "Dropper Icon")
        };

        Rect toolbarRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(EditorGUIUtility.singleLineHeight * 1.25f));

        float toolbarWidth = 200f;

        toolbarRect.x += (toolbarRect.width - toolbarWidth) / 2f;
        toolbarRect.width = toolbarWidth;

        int currentIndex = GUI.Toolbar(toolbarRect, (int)currentTool - 1, toolBar);
        currentTool = (TileTool)(currentIndex + 1);

        if (currentTool != TileTool.None)
        {
            Tools.current = Tool.None;
        }

        if (Tools.current != Tool.None)
        {
            currentTool = TileTool.None;
        }

        #endregion

        EditorGUILayout.Space(5f);

        #region Path & Palette Selection

        EditorGUILayout.BeginHorizontal();

        GUI.enabled = false;
        setting.path = EditorGUILayout.TextField("Path", setting.path);
        GUI.enabled = true;

        // [Browse] button to change path directory.
        if (GUILayout.Button("Browse"))
        {
            string fullPath = EditorUtility.OpenFolderPanel("Select prefab path", setting.path, "");
            string[] split = fullPath.Split("/");

            bool assetPath = false;
            string newPath = "";

            for (int i = 0; i < split.Length; i++)
            {
                if (split[i].Equals("Assets"))
                {
                    assetPath = true;
                }

                if (assetPath)
                {
                    newPath += split[i] + "/";
                }
            }
            
            // If player chose new path, set new path.
            if (newPath.Length > 0)
            {
                setting.path = newPath.Remove(newPath.Length - 1, 1);
            }
        }

        EditorGUILayout.EndHorizontal();

        DirectoryInfo folderPath;

        try
        {
            folderPath = new DirectoryInfo(setting.path);
        }
        catch (ArgumentException) { return; }
        
        string[] paletteFolders = new string[folderPath.GetDirectories().Length];

        for (int i = 0; i < paletteFolders.Length; i++)
        {
            paletteFolders[i] = folderPath.GetDirectories()[i].Name;
        }

        if (paletteFolders.Length == 0)
        {
            EditorGUILayout.HelpBox("There's no folder inside specific folder path! Press 'Create Palette' down below to create new folder.", MessageType.Warning);
        }

        // If folder index is out of range (amount of folder decrease or missing), set back to select the first palette folder.
        if (paletteFolderIndex > paletteFolders.Length - 1)
        {
            paletteFolderIndex = 0;
        }

        GUIContent paletteContent = new GUIContent("Selected Palette");
        paletteContent.tooltip = "The palette that will be use to paint.";

        int previousPalette = paletteFolderIndex;
        paletteFolderIndex = EditorGUILayout.Popup(paletteContent, paletteFolderIndex, paletteFolders);

        // If user change palette folder, refresh.
        if (paletteFolderIndex != previousPalette)
        {
            RefreshPalette();
        }

        EditorGUILayout.Space(10f);

        #region Paint Settings

        EditorGUILayout.LabelField("Paint Settings", titleStyle);

        // Mechanic Object.
        GUIContent mechanicContent = new GUIContent("Mechanic Object");
        mechanicContent.tooltip = "Determine if selected palette is mechanic object or not.";
        setting.isMechanicObject = EditorGUILayout.Toggle(mechanicContent, setting.isMechanicObject);
        
        // Replace Object.
        GUIContent replaceContent = new GUIContent("Replace Mode");
        replaceContent.tooltip = "Determine if player can use Paint tool to replace existing object on selected tile or not.";
        setting.isReplaceMode = EditorGUILayout.Toggle(replaceContent, setting.isReplaceMode);

        #endregion

        #endregion

        EditorGUILayout.Space(5f);

        #region Create Palette & Refresh Button

        EditorGUILayout.Space(5f);
        EditorGUILayout.BeginHorizontal();

        // If there's Create Palette window, disable the button.
        if (TilemapCreatePalette.IsOpen())
        {
            GUI.enabled = false;
        }

        // [Create Palette] - Button to create palette.
        if (GUILayout.Button("Create Palette", GUILayout.Height(EditorGUIUtility.singleLineHeight * 1.25f)))
        {
            Rect mouseRect = new Rect(Event.current.mousePosition, Vector2.one);
            TilemapCreatePalette createPaletteWindow = new TilemapCreatePalette();
            PopupWindow.Show(mouseRect, createPaletteWindow);
        }

        GUI.enabled = true;

        // [Refresh] - Button to refresh the palette.
        if (GUILayout.Button("Refresh", GUILayout.Height(EditorGUIUtility.singleLineHeight * 1.25f)))
        {
            RefreshPalette();
        }

        EditorGUILayout.EndHorizontal();

        #endregion

        EditorGUILayout.Space(10f);

        #region Palette Window

        Rect barRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(EditorGUIUtility.singleLineHeight));

        // Search bar
        Rect searchRect = new Rect(barRect.x, barRect.y, barRect.width * 0.75f, barRect.height);
        searchText = EditorGUI.TextField(searchRect, GUIContent.none, searchText);

        if (searchText.Equals(""))
        {
            GUIStyle searchStyle = new GUIStyle();
            searchStyle.normal.textColor = Color.gray;
            searchStyle.fontStyle = FontStyle.Italic;
            EditorGUI.LabelField(searchRect, " Search...", searchStyle);
        }

        // Selection size
        Rect selectionSizeRect = new Rect(barRect.width * 0.78f, barRect.y, barRect.width * 0.2f, barRect.height);
        setting.paletteWindowSize = GUI.HorizontalSlider(selectionSizeRect, setting.paletteWindowSize, 50, position.width);

        EditorGUILayout.Space(10f);

        // If user is dragging object in window, show as area to paste object instead of showing palette.
        if (isDragging)
        {
            Rect pasteRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.ExpandHeight(true));

            GUIContent pasteLabel = new GUIContent("Drag Prefab or FBX files here to move them into current palette folder");
            GUIStyle pasteStyle = new GUIStyle();
            pasteStyle.normal.textColor = Color.gray;
            pasteStyle.alignment = TextAnchor.MiddleCenter;
            pasteStyle.wordWrap = true;
            pasteStyle.fontStyle = FontStyle.Bold;
            pasteStyle.fontSize = 15;

            GUI.Box(pasteRect, GUIContent.none, GUI.skin.window);

            pasteRect.x += 10f;
            pasteRect.width -= pasteRect.x * 2f;

            EditorGUI.LabelField(pasteRect, pasteLabel, pasteStyle);
            return;
        }

        // If there's no prefab, show warning.
        if (palettesList.Count == 0)
        {
            EditorGUILayout.HelpBox("Looks like there's no prefab assets available OR your folder path is invalid, please try again and press 'Refresh'", MessageType.Warning);
        }
        else
        {
            // Check if there's any changes in palette, refresh.
            bool makeChanges = false;

            foreach (TilemapPalette palette in palettesList)
            {
                if (!palette.prefab || !Directory.Exists(palette.path))
                {
                    makeChanges = true;
                    break;
                }
            }

            if (makeChanges)
            {
                RefreshPalette();
                return;
            }

            // Find all palettes that will be show.
            List<TilemapPalette> palettesSearch = new List<TilemapPalette>();

            // Check if search field isn't empty, show objects in order based on search field.
            if (searchText.Equals(""))
            {
                foreach (TilemapPalette palette in palettesList)
                {
                    if (palette.directory.Equals(paletteFolders[paletteFolderIndex]))
                    {
                        palettesSearch.Add(palette);
                    }
                }
            }
            else
            {
                foreach (TilemapPalette palette in palettesList)
                {
                    // If the palette is in different palette folder, skip.
                    if (!paletteFolders[paletteFolderIndex].Equals(palette.directory))
                    {
                        continue;
                    }

                    // If the search field has match string with palette name, add to list.
                    if (palette.prefab.name.ToLower().Contains(searchText.ToLower()))
                    {
                        palettesSearch.Add(palette);
                    }
                    else
                    {
                        // If the search field has start string match with palette name, add to list.
                        if (palette.prefab.name.ToLower().StartsWith(searchText.ToLower()))
                        {
                            palettesSearch.Add(palette);
                        }
                    }
                }
            }

            // If there's no palette that match with your search, show label.
            if (palettesSearch.Count == 0)
            {
                GUIStyle centerStyle = new GUIStyle();
                centerStyle.normal.textColor = Color.gray;
                centerStyle.alignment = TextAnchor.MiddleCenter;

                if (!searchText.Equals(""))
                {
                    EditorGUILayout.LabelField("Can't find palette name from your search\nTry check your files name or search spelling and try again", centerStyle);
                }
                else
                {
                    EditorGUILayout.LabelField("There's no prefab files in this palette yet\nTry add some prefabs into palette folder and refresh", centerStyle);
                }

                return;
            }

            // Draw palette as button.
            List<GUIContent> prefabPreview = new List<GUIContent>();

            foreach (TilemapPalette palette in palettesSearch)
            {
                GameObject prefab = palette.prefab;

                GUIContent previewContent = new GUIContent();
                previewContent.image = AssetPreview.GetAssetPreview(prefab);
                previewContent.text = prefab.name;
                previewContent.tooltip = prefab.name;

                prefabPreview.Add(previewContent);
            }

            List<TilemapPalette> previousPaletteSelected = new List<TilemapPalette>(paletteSelectedList);

            Rect scrollViewRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.ExpandHeight(true));
            Rect contentViewRect = scrollViewRect;

            float additionalY = 0f;

            Vector2 pos = new Vector2(0f, contentViewRect.y);

            for (int i = 0; i < prefabPreview.Count; i++)
            {
                pos.x += setting.paletteWindowSize;

                if (pos.x + setting.paletteWindowSize > position.width)
                {
                    pos.x = 0f;
                    pos.y += setting.paletteWindowSize;

                    if (pos.y + setting.paletteWindowSize > position.height)
                    {
                        additionalY = (pos.y + setting.paletteWindowSize) - position.height;
                    }
                }
            }

            contentViewRect.height += additionalY;
            paletteScrollPosition = GUI.BeginScrollView(scrollViewRect, paletteScrollPosition, contentViewRect, false, false, GUIStyle.none, GUI.skin.verticalScrollbar);

            Rect paletteRect = scrollViewRect;
            paletteRect.width = setting.paletteWindowSize;
            paletteRect.height = setting.paletteWindowSize;

            GUIStyle previewStyle = new GUIStyle(GUI.skin.button);
            previewStyle.imagePosition = ImagePosition.ImageOnly;

            GUIStyle previewLabelStyle = new GUIStyle();
            previewLabelStyle.normal.textColor = Color.white;
            previewLabelStyle.fontStyle = FontStyle.Bold;
            previewLabelStyle.alignment = TextAnchor.MiddleCenter;

            float labelOffset = setting.paletteWindowSize - (EditorGUIUtility.singleLineHeight + 5f);

            // If there's no selection at the beginning, select the first one.
            if (paletteSelectedList.Count == 0)
            {
                paletteSelectedList.Add(palettesSearch[0]);
            }

            // If it's not Group Brush mode, select only one palette.
            if (!groupBrush.isEnable && paletteSelectedList.Count > 1)
            {
                for (int i = 1; i < paletteSelectedList.Count; i++)
                {
                    paletteSelectedList.RemoveAt(i);
                }
            }

            for (int i = 0; i < prefabPreview.Count; i++)
            {
                bool isPressed = paletteSelectedList.Contains(palettesSearch[i]);
                isPressed = GUI.Toggle(paletteRect, isPressed, prefabPreview[i], previewStyle);

                if (isPressed)
                {
                    // If it's not Group Brush mode, disable others and enable only selected one, else, make it toggle like normal.
                    if (groupBrush.isEnable)
                    {
                        if (!paletteSelectedList.Contains(palettesSearch[i]))
                        {
                            paletteSelectedList.Add(palettesSearch[i]);
                        }
                    }
                    else
                    {
                        paletteSelectedList.Clear();
                        paletteSelectedList.Add(palettesSearch[i]);
                    }
                }
                else
                {
                    if (groupBrush.isEnable)
                    {
                        paletteSelectedList.Remove(palettesSearch[i]);
                    }
                }

                Event e = Event.current;

                int pingIndex = i;

                // If player press RMB in palette button, show button that will ping asset in Asset folder.
                if (paletteRect.Contains(e.mousePosition) && e.button == 1 && e.type == EventType.Used)
                {
                    GenericMenu menu = new GenericMenu();

                    menu.AddItem(new GUIContent("Show in asset folder"), false, delegate
                    {
                        TilemapPalette palette = palettesSearch[pingIndex];
                        EditorGUIUtility.PingObject(palette.prefab);
                    });

                    menu.ShowAsContext();
                }

                Rect labelRect = new Rect(paletteRect.x, paletteRect.y + labelOffset, paletteRect.width, EditorGUIUtility.singleLineHeight);

                // Check if name is longer than palette window, set text to the left instead of center.
                string label = prefabPreview[i].text;
                float width = GUI.skin.label.CalcSize(new GUIContent(label)).x;

                if (width >= setting.paletteWindowSize - 10f)
                {
                    string newLabel = "";
                    float currentWidth = 0f;

                    for (int c = 0; c < prefabPreview[i].text.Length; c++)
                    {
                        float charWidth = GUI.skin.label.CalcSize(new GUIContent(label[c].ToString())).x;

                        if (currentWidth + charWidth >= setting.paletteWindowSize - 10f)
                        {
                            label = newLabel + "...";
                            break;
                        }

                        newLabel += label[c];
                        currentWidth += charWidth;
                    }
                }

                GUI.Label(labelRect, label, previewLabelStyle);

                paletteRect.x += setting.paletteWindowSize;

                if (paletteRect.x + paletteRect.width > position.width)
                {
                    paletteRect.x = 0f;
                    paletteRect.y += setting.paletteWindowSize;
                }
            }

            GUI.EndScrollView();

            // If player change palette, focus on sceneview.
            if (previousPaletteSelected.Count != paletteSelectedList.Count)
            {
                if (SceneView.sceneViews.Count > 0)
                {
                    SceneView sceneView = SceneView.sceneViews[0] as SceneView;
                    sceneView.Focus();
                }
            }
            else
            {
                for (int i = 0; i < paletteSelectedList.Count; i++)
                {
                    // If prefab isn't the same one, focus on sceneview.
                    if (!paletteSelectedList[i].prefab.Equals(previousPaletteSelected[i].prefab))
                    {
                        if (SceneView.sceneViews.Count > 0)
                        {
                            SceneView sceneView = SceneView.sceneViews[0] as SceneView;
                            sceneView.Focus();
                        }

                        break;
                    }
                }
            }
        }

        #endregion
    }

    // Function to handlle shortcut keys.
    private void ShortcutHandler()
    {
        Event e = Event.current;

        // Shortcut keys to change tools.
        if (e.type == EventType.KeyDown)
        {
            switch (e.keyCode)
            {
                // [B] - Change to Paint tool.
                case KeyCode.B:
                e.Use();
                Tools.current = Tool.None;
                currentTool = TileTool.Paint;
                break;

                // [E] - Change to Eraser tool.
                case KeyCode.E:
                e.Use();
                Tools.current = Tool.None;
                currentTool = TileTool.Eraser;
                break;

                // [I] - Change to Dropper tool.
                case KeyCode.I:
                e.Use();
                Tools.current = Tool.None;
                currentTool = TileTool.Dropper;
                break;

                // [R] - Rotate.
                case KeyCode.R:
                if (paletteSelectedList.Count > 0 && currentTool == TileTool.Paint)
                {
                    e.Use();
                    setting.brushRotation += setting.brushRotationAmount;

                    // Clamp amount of rotation to -360 to 360 degrees.
                    if (setting.brushRotation >= 360f)
                    {
                        setting.brushRotation -= 360f;
                    }

                    if (setting.brushRotation <= -360f)
                    {
                        setting.brushRotation += 360f;
                    }
                }
                break;

                // [Up Arrow] - Increase floor height (for Manual Selection).
                case KeyCode.UpArrow:
                if (paletteSelectedList.Count > 0 && selectionModeIndex == 1 && currentTool != TileTool.None)
                {
                    e.Use();
                    setting.gridFloor++;
                }

                break;

                // [Down Arrow] - Decrease floor height (for Manual Selection).
                case KeyCode.DownArrow:
                if (paletteSelectedList.Count > 0 && selectionModeIndex == 1 && currentTool != TileTool.None)
                {
                    e.Use();
                    setting.gridFloor--;
                }
                break;
            }
        }

        BrushSizeHandler();

        // If user is focusing on Unity's tool, unselect all tools and destroy preview object.
        if (Tools.current != Tool.None)
        {
            currentTool = TileTool.None;

            if (currentPreviewObject)
            {
                DestroyImmediate(currentPreviewObject);
            }
        }

        Repaint();
    }

    // Function to draw tool GUI in Editor window.
    private GUIContent GetToolTab(string name, string fileName)
    {
        GUIContent toolContent = new GUIContent();

        Sprite sprite = (Sprite)AssetDatabase.LoadAssetAtPath("Assets/Scripts/Editors/Tilemap Editor/Icons/" + fileName + ".png", typeof(Sprite));
        toolContent.image = sprite.texture;
        toolContent.tooltip = name;

        return toolContent;
    }

    // Function to update scene GUI.
    private void OnSceneGUI(SceneView sceneView)
    {
        // Check if user is in Prefab isolation scene, then return.
        if (PrefabStageUtility.GetCurrentPrefabStage())
        {
            currentTool = TileTool.None;

            if (currentPreviewObject)
            {
                DestroyImmediate(currentPreviewObject);
            }

            return;
        }

        //DrawGUI(sceneView.position);
        ShortcutHandler();

        // Check if you're using brush and have grid and tilemap to modify with.
        if (selectedGrid && selectedTilemap && currentTool != TileTool.None && !Application.isPlaying)
        {
            // If user is editing brush size, then ignore.
            if (currentTool == TileTool.Brush_Size)
            {
                return;
            }

            Vector3 selectedCell = GetSelectedCell(Event.current.mousePosition);
            HandleSceneViewInput();
            VisualSelection(selectedCell);

            // If player release mouse, clear paint array.
            if (Event.current.type == EventType.MouseUp)
            {
                paintedPositions.Clear();
            }

            Event e = Event.current;

            // Check if user is dragging and not viewing object or using Unity tools.
            if ((e.type == EventType.MouseDrag || e.type == EventType.MouseDown) && e.button == 0 && !Tools.viewToolActive)
            {
                // Get selection based on brush size.
                Vector3 position1 = selectedCell;
                Vector3 position2 = selectedCell + cellSelectionSize;

                position2.x += cellSelectionSize.x > 0f ? -1f : 1f;
                position2.y += cellSelectionSize.y > 0f ? -1f : 1f;
                position2.z += cellSelectionSize.z > 0f ? -1f : 1f;

                List<Vector3> selection = GetSelection(selectedCell, position1, position2);

                for (int i = 0; i < selection.Count; i++)
                {
                    if (paintedPositions.Contains(selection[i]))
                    {
                        selection.RemoveAt(i);
                    }
                }

                Dictionary<Vector3, List<GameObject>> objectList = FindObjectsInArea(selection);
                bool makeChanges = false;

                switch (currentTool)
                {
                    case TileTool.Paint:
                    // Check if palette selection is valid and currently not using any tool and using left click.
                    if (paletteSelectedList.Count > 0)
                    {
                        List<Vector3> paintArea = new List<Vector3>();
                        List<GameObject> replaceObject = new List<GameObject>();

                        foreach (KeyValuePair<Vector3, List<GameObject>> currentObject in objectList)
                        {
                            bool placeable = false;

                            // If there's no object in the area, set to placeable.
                            if (currentObject.Value.Count == 0 && !setting.isReplaceMode)
                            {
                                placeable = true;
                            }

                            // If it's occupied but in replace mode, add object to replace list and set to placeable.
                            if (currentObject.Value.Count > 0 && setting.isReplaceMode)
                            {
                                for (int i = 0; i < currentObject.Value.Count; i++)
                                {
                                    // If object isn't in the list yet, add one.
                                    if (!replaceObject.Contains(currentObject.Value[i]))
                                    {
                                        replaceObject.Add(currentObject.Value[i]);
                                    }
                                }

                                placeable = true;
                            }

                            // If it's placeable and position is new one, add to paintArea list.
                            if (placeable && !paintedPositions.Contains(currentObject.Key))
                            {
                                paintArea.Add(currentObject.Key);
                            }
                        }

                        // Destroy all replaced objects.
                        for (int i = 0; i < replaceObject.Count; i++)
                        {
                            if (replaceObject[i])
                            {
                                Undo.DestroyObjectImmediate(replaceObject[i]);
                                DestroyImmediate(replaceObject[i]);
                            }
                        }

                        Paint(paintArea);
                        makeChanges = paintArea.Count > 0;
                    }

                    break;

                    case TileTool.Eraser:
                    // Remove all game objects in selected positions.
                    foreach (KeyValuePair<Vector3, List<GameObject>> objects in objectList)
                    {
                        if (objects.Value.Count > 0)
                        {
                            for (int i = 0; i < objects.Value.Count; i++)
                            {
                                if (objects.Value[i] && IsInLayer(setting.brushLayer, objects.Value[i].layer))
                                {
                                    paintedPositions.Add(objects.Key);
                                    Erase(objects.Value[i]);
                                    makeChanges = true;
                                }
                            }
                        }
                    }
                    break;

                    case TileTool.Dropper:

                    // Check if you press down only once.
                    if (Event.current.type == EventType.MouseDown)
                    {
                        List<GameObject> gameObjects = FindObjectsInPosition(selectedCell);

                        // If the object list isn't empty, then use dropper for that selected cell.
                        if (gameObjects.Count > 0)
                        {
                            Dropper(gameObjects);
                            makeChanges = true;
                        }
                    }

                    break;
                }

                // If there's make changes to the scene, repaint the scene.
                if (makeChanges)
                {
                    sceneView.Repaint();
                }
            }
        }
    }

    // Function that execute when start painting.
    private void Paint(List<Vector3> positions)
    {
        for (int i = 0; i < positions.Count; i++)
        {
            // Add position to array to prevent from painting at same position.
            if (!paintedPositions.Contains(positions[i]))
            {
                paintedPositions.Add(positions[i]);
            }
        }

        // Paint with Group Brush
        if (groupBrush.isEnable)
        {
            int amount = groupBrush.density;
            Vector3 offset = cellSize * 0.5f;

            foreach (Vector3 position in positions)
            {
                Vector2 minPos = new Vector2(position.x - offset.x, position.z - offset.z);
                Vector2 maxPos = new Vector2(position.x + offset.x, position.z + offset.z);

                float rangeX = maxPos.x - minPos.x;
                float averageX = rangeX / (float)amount;
                float randomX = UnityEngine.Random.Range(0f, averageX * 0.5f);

                for (int i = 0; i < amount; i++)
                {
                    int randomPrefab = UnityEngine.Random.Range(0, paletteSelectedList.Count);

                    float x = (averageX * i) + randomX;
                    float y = UnityEngine.Random.Range(0f, cellSize.z);

                    Vector3 newPosition = position + new Vector3(x, 0f, y) - new Vector3(offset.x, 0f, offset.z);

                    GameObject prefabObject = paletteSelectedList[randomPrefab].prefab;

                    if (prefabObject)
                    {
                        GameObject instatiateObject = (GameObject)PrefabUtility.InstantiatePrefab(prefabObject);
                        instatiateObject.transform.position = newPosition + setting.brushOffset;
                        instatiateObject.transform.eulerAngles = Vector3.up * setting.brushRotation;

                        // If it's mechanic object, set to last sibling. Else, tie it to the tilemap object.
                        if (setting.isMechanicObject)
                        {
                            instatiateObject.transform.SetAsLastSibling();
                        }
                        else
                        {
                            instatiateObject.transform.SetParent(selectedTilemap.transform);
                        }

                        if (groupBrush.randomRotation)
                        {
                            Vector3 eulerAngle = instatiateObject.transform.eulerAngles;
                            eulerAngle.y = UnityEngine.Random.Range(0f, 360f);
                            instatiateObject.transform.eulerAngles = eulerAngle;
                        }

                        Undo.RegisterCreatedObjectUndo(instatiateObject, "Created " + instatiateObject.name);
                    }
                }
            }

            return;
        }

        foreach (Vector3 position in positions)
        {
            GameObject instatiateObject = (GameObject)PrefabUtility.InstantiatePrefab(paletteSelectedList[0].prefab);
            instatiateObject.transform.position = position + setting.brushOffset;
            instatiateObject.transform.eulerAngles = Vector3.up * setting.brushRotation;

            // If it's mechanic object, set to last sibling. Else, tie it to the tilemap object.
            if (setting.isMechanicObject)
            {
                instatiateObject.transform.SetAsLastSibling();
            }
            else
            {
                instatiateObject.transform.SetParent(selectedTilemap.transform);
            }

            Undo.RegisterCreatedObjectUndo(instatiateObject, "Created " + instatiateObject.name);
        }
    }

    // Function that excute when start erasing.
    private void Erase(GameObject prefab)
    {
        Undo.DestroyObjectImmediate(prefab);
        DestroyImmediate(prefab);
    }

    // Function that execute when using dropper.
    private void Dropper(List<GameObject> prefabs)
    {
        GameObject prefabRoot = null;

        for (int j = 0; j < prefabs.Count; j++)
        {
            prefabRoot = PrefabUtility.GetCorrespondingObjectFromOriginalSource(prefabs[j]);

            if (prefabRoot)
            {
                break;
            }
        }

        // If there's no object with actual prefab root, cancel.
        if (!prefabRoot)
        {
            return;
        }

        for (int k = 0; k < palettesList.Count; k++)
        {
            // If prefab is the same as in the palette, set selected palette index.
            if (palettesList[k].prefab.Equals(prefabRoot))
            {
                // If it's group brush mode, add to selection list, else, select that object as main palette to paint.
                if (groupBrush.isEnable)
                {
                    // Check if that prefab is already selected, if not, add to selected prefab list.
                    bool containPrefab = false;

                    for (int l = 0; l < paletteSelectedList.Count; l++)
                    {
                        if (paletteSelectedList[l].prefab.Equals(prefabRoot))
                        {
                            containPrefab = true;
                        }
                    }

                    if (!containPrefab)
                    {
                        paletteSelectedList.Add(palettesList[k]);
                    }
                }
                else
                {
                    paletteSelectedList.Clear();
                    paletteSelectedList.Add(palettesList[k]);
                }

                // Set tool back to Paint tool.
                Tools.current = Tool.None;
                currentTool = TileTool.Paint;

                Repaint();
                break;
            }
        }
    }

    // Function to draw selection frame.
    private void VisualSelection(Vector3 selectedCell)
    {
        Transform gridTrans = selectedGrid.transform;

        Vector3 offset = cellSize * 0.5f;

        Vector3 bottomUpLeft = selectedCell;
        Vector3 bottomUpRight = selectedCell;
        Vector3 bottomDownLeft = selectedCell;
        Vector3 bottomDownRight = selectedCell;

        PreviewObjectHandler(selectedCell);

        Vector3 gridX = gridTrans.right; // X axis (in 2D) based on current grid swizzle.
        Vector3 gridY = gridTrans.up; // Y axis (in 2D) based on current grid swizzle.
        Vector3 gridForward = gridTrans.up; // Direction of cell to extend to.

        switch (selectedGrid.cellSwizzle)
        {
            case GridLayout.CellSwizzle.XYZ:
            gridForward = -gridTrans.forward;
            break;

            case GridLayout.CellSwizzle.XZY:
            gridY = gridTrans.forward;
            break;

            case GridLayout.CellSwizzle.YXZ:
            gridForward = -gridTrans.forward;
            break;

            case GridLayout.CellSwizzle.YZX:
            gridY = gridTrans.forward;
            break;

            case GridLayout.CellSwizzle.ZXY:
            gridForward = gridTrans.right;
            gridX = gridTrans.forward;
            break;

            case GridLayout.CellSwizzle.ZYX:
            gridForward = gridTrans.right;
            gridX = gridTrans.forward;
            break;
        }

        // Get amount of extension
        float forwardAxis = cellSelectionSize.y;

        // Get direction from current brush rotation (Similar to Unity's transform.forward)
        Vector3 transformDirectionX = Quaternion.AngleAxis(setting.brushRotation, gridForward) * gridX;
        Vector3 transformDirectionY = Quaternion.AngleAxis(setting.brushRotation, gridForward) * gridY;

        // Get size of extension in X and Y axis
        float sizeX = GetAxis(cellSelectionSize, Vector3Axis.X);
        float sizeY = GetAxis(cellSelectionSize, Vector3Axis.Y);

        // If X axis direction is more than 1 and positive, increase size
        if (sizeX > 1f)
        {
            bottomUpRight += Vector3.Scale(transformDirectionX * (sizeX - 1f), cellSize);
            bottomDownRight += Vector3.Scale(transformDirectionX * (sizeX - 1f), cellSize);
        }

        // If X axis direction is more than 1 and negative, increase size
        if (sizeX < -1f)
        {
            bottomUpLeft += Vector3.Scale(transformDirectionX * (sizeX + 1f), cellSize);
            bottomDownLeft += Vector3.Scale(transformDirectionX * (sizeX + 1f), cellSize);
        }

        // If Y axis direction is more than 1 and positive, increase size
        if (sizeY > 1f)
        {
            bottomUpLeft += Vector3.Scale(transformDirectionY * (sizeY - 1f), cellSize);
            bottomUpRight += Vector3.Scale(transformDirectionY * (sizeY - 1f), cellSize);
        }

        // If Y axis direction is more than 1 and negative, increase size
        if (sizeY < -1f)
        {
            bottomDownLeft += Vector3.Scale(transformDirectionY * (sizeY + 1f), cellSize);
            bottomDownRight += Vector3.Scale(transformDirectionY * (sizeY + 1f), cellSize);
        }

        bottomUpLeft += Vector3.Scale(-transformDirectionX, offset) + Vector3.Scale(transformDirectionY, offset);
        bottomUpRight += Vector3.Scale(transformDirectionX, offset) + Vector3.Scale(transformDirectionY, offset);
        bottomDownLeft += Vector3.Scale(-transformDirectionX, offset) + Vector3.Scale(-transformDirectionY, offset);
        bottomDownRight += Vector3.Scale(transformDirectionX, offset) + Vector3.Scale(-transformDirectionY, offset);

        Vector3 topUpLeft = bottomUpLeft + (gridForward * forwardAxis);
        Vector3 topUpRight = bottomUpRight + (gridForward * forwardAxis);
        Vector3 topDownLeft = bottomDownLeft + (gridForward * forwardAxis);
        Vector3 topDownRight = bottomDownRight + (gridForward * forwardAxis);

        Vector3[] topPlane = { topUpLeft, topUpRight, topUpRight, topDownRight, topDownRight, topDownLeft, topDownLeft, topUpLeft };
        Vector3[] bottomPlane = { bottomUpLeft, bottomUpRight, bottomUpRight, bottomDownRight, bottomDownRight, bottomDownLeft, bottomDownLeft, bottomUpLeft };

        // Set color of selected tile based on current tool.
        switch (currentTool)
        {
            case TileTool.Paint:
            Handles.color = Color.green;
            break;

            case TileTool.Eraser:
            Handles.color = Color.red;
            break;

            case TileTool.Dropper:
            Handles.color = Color.white;
            break;

            case TileTool.Brush_Size:
            Handles.color = Color.white;
            break;
        }

        Handles.DrawLines(bottomPlane);
        Handles.DrawLines(topPlane);
        Handles.DrawLine(topUpLeft, bottomUpLeft);
        Handles.DrawLine(topUpRight, bottomUpRight);
        Handles.DrawLine(topDownLeft, bottomDownLeft);
        Handles.DrawLine(topDownRight, bottomDownRight);

        SceneView.RepaintAll();
    }

    // Function to handle transparent object.
    private void PreviewObjectHandler(Vector3 selectedCell)
    {
        GameObject instantiateObject = null;

        if (!groupBrush.isEnable)
        {
            for (int i = 0; i < palettesList.Count; i++)
            {
                if (paletteSelectedList.Contains(palettesList[i]))
                {
                    instantiateObject = palettesList[i].prefab;
                    break;
                }
            }
        }
        else
        {
            if (paletteSelectedList.Count > 0)
            {
                instantiateObject = paletteSelectedList[0].prefab;
            }
        }

        // Show transparent object placement position.
        if (currentTool == TileTool.Paint && palettesList.Count > 0 && instantiateObject)
        {
            if (!currentPreviewObject)
            {
                currentPreviewObject = Instantiate(new GameObject("PreviewTileObject"), selectedCell, Quaternion.identity);

                GameObject prefabObject = PrefabUtility.InstantiatePrefab(instantiateObject) as GameObject;

                foreach (Component comp in prefabObject.GetComponents<Component>())
                {
                    if (!(comp is Transform) && !(comp is MeshRenderer) && !(comp is MeshFilter))
                    {
                        DestroyImmediate(comp);
                    }
                }

                for (int i = 0; i < prefabObject.transform.childCount; i++)
                {
                    foreach (Component comp in prefabObject.transform.GetChild(i).GetComponents<Component>())
                    {
                        if (!(comp is Transform) && !(comp is MeshRenderer) && !(comp is MeshFilter))
                        {
                            DestroyImmediate(comp);
                        }
                    }
                }

                prefabObject.transform.SetParent(currentPreviewObject.transform);
                prefabObject.transform.SetAsFirstSibling();
                prefabObject.transform.localPosition = Vector3.zero;
                prefabObject.transform.eulerAngles = Vector3.up * setting.brushRotation;
            }
            else
            {
                // Remove duplicate trans tile object.
                Transform[] transformObjects = FindObjectsOfType<Transform>();

                for (int i = 0; i < transformObjects.Length; ++i)
                {
                    GameObject gameObject = transformObjects[i].gameObject;

                    if (gameObject.name.StartsWith("PreviewTileObject") && !gameObject.Equals(currentPreviewObject))
                    {
                        DestroyImmediate(gameObject);
                    }
                }

                // If user has change the palette, destroy and replace with new one.
                if (currentPreviewObject.transform.GetChild(0).name != instantiateObject.name)
                {
                    if (currentPreviewObject.transform.childCount > 0)
                    {
                        DestroyImmediate(currentPreviewObject.transform.GetChild(0).gameObject);
                    }

                    GameObject prefabObject = PrefabUtility.InstantiatePrefab(instantiateObject) as GameObject;

                    foreach (Component comp in prefabObject.GetComponents<Component>())
                    {
                        if (!(comp is Transform) && !(comp is MeshRenderer) && !(comp is MeshFilter))
                        {
                            DestroyImmediate(comp);
                        }
                    }

                    for (int i = 0; i < prefabObject.transform.childCount; i++)
                    {
                        foreach (Component comp in prefabObject.transform.GetChild(i).GetComponents<Component>())
                        {
                            if (!(comp is Transform) && !(comp is MeshRenderer) && !(comp is MeshFilter))
                            {
                                DestroyImmediate(comp);
                            }
                        }
                    }

                    prefabObject.transform.SetParent(currentPreviewObject.transform);
                    prefabObject.transform.SetAsFirstSibling();
                    prefabObject.transform.localPosition = Vector3.zero;
                    prefabObject.transform.eulerAngles = Vector3.up * setting.brushRotation;
                }
            }

            currentPreviewObject.transform.position = selectedCell + setting.brushOffset;
            currentPreviewObject.hideFlags = HideFlags.HideAndDontSave;
            currentPreviewObject.transform.GetChild(0).eulerAngles = Vector3.up * setting.brushRotation;

            if (setting.showGrid)
            {
                // If there's no line object to draw grid, draw one.
                if (currentPreviewObject.transform.childCount <= 1)
                {
                    Vector3 position = selectedCell;

                    // If there's brush offset, set grid back to normal (Don't offset follow the trans object)
                    if (setting.brushOffset != Vector3.zero)
                    {
                        position -= setting.brushOffset;
                    }

                    // Draw Grid Line
                    Vector3 offset = cellSize * 0.5f;

                    int size = 1;

                    Vector3[] linesX = new Vector3[(size * 2) + 2];
                    Vector3[] linesZ = new Vector3[(size * 2) + 2];

                    float gridLength = ((float)size * cellSize.x) + cellSize.x;

                    float gridSizeX = -(((float)size * cellSize.x) + offset.x);
                    float gridSizeZ = -(((float)size * cellSize.z) + offset.z);

                    for (int i = 0; i < linesX.Length; i++)
                    {
                        Vector3 topX = new Vector3(position.x + gridSizeX, position.y, position.z + gridLength);
                        Vector3 bottomX = new Vector3(position.x + gridSizeX, position.y, position.z - gridLength);

                        GameObject lineObject = new GameObject("GridLine-X-" + (i + 1));
                        lineObject.transform.SetParent(currentPreviewObject.transform);

                        LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>();
                        lineRenderer.enabled = setting.showGrid;
                        lineRenderer.useWorldSpace = false;
                        lineRenderer.material = new Material(Shader.Find("Unlit/Color"));
                        lineRenderer.startColor = setting.gridColor;
                        lineRenderer.endColor = setting.gridColor;
                        lineRenderer.startWidth = setting.gridThickness;
                        lineRenderer.endWidth = setting.gridThickness;
                        lineRenderer.SetPosition(0, topX);
                        lineRenderer.SetPosition(1, bottomX);

                        gridSizeX += (size * cellSize.x);
                    }

                    for (int i = 0; i < linesZ.Length; i++)
                    {
                        Vector3 topZ = new Vector3(position.x + gridLength, position.y, position.z + gridSizeZ);
                        Vector3 bottomZ = new Vector3(position.x - gridLength, position.y, position.z + gridSizeZ);

                        GameObject lineObject = new GameObject("GridLine-Z-" + (i + 1));
                        lineObject.transform.SetParent(currentPreviewObject.transform);

                        LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>();
                        lineRenderer.enabled = setting.showGrid;
                        lineRenderer.useWorldSpace = false;
                        lineRenderer.material = new Material(Shader.Find("Unlit/Color"));
                        lineRenderer.startColor = setting.gridColor;
                        lineRenderer.endColor = setting.gridColor;
                        lineRenderer.startWidth = setting.gridThickness;
                        lineRenderer.endWidth = setting.gridThickness;
                        lineRenderer.SetPosition(0, topZ);
                        lineRenderer.SetPosition(1, bottomZ);

                        gridSizeZ += (size * cellSize.z);
                    }
                }
                else
                {
                    for (int i = 1; i < currentPreviewObject.transform.childCount; i++)
                    {
                        if (!currentPreviewObject.transform.GetChild(i).GetComponent<LineRenderer>())
                        {
                            continue;
                        }

                        LineRenderer lineRenderer = currentPreviewObject.transform.GetChild(i).GetComponent<LineRenderer>();
                        lineRenderer.enabled = setting.showGrid;
                        lineRenderer.startColor = setting.gridColor;
                        lineRenderer.endColor = setting.gridColor;
                        lineRenderer.startWidth = setting.gridThickness;
                        lineRenderer.endWidth = setting.gridThickness;
                    }
                }
            }
            else
            {
                for (int i = 0; i < currentPreviewObject.transform.childCount; i++)
                {
                    Transform child = currentPreviewObject.transform.GetChild(i);

                    if ((child.name.StartsWith("GridLine-X-") || child.name.StartsWith("GridLine-Z-")) && child.GetComponent<LineRenderer>())
                    {
                        DestroyImmediate(child.gameObject);
                    }
                }
            }
        }
        else
        {
            DestroyPreview();
        }
    }

    // Function to refresh palette list.
    private void RefreshPalette()
    {
        palettesList.Clear();

        DirectoryInfo folderPath;

        try
        {
            folderPath = new DirectoryInfo(setting.path);
        }
        catch (Exception) { return; }
         
        string[] paletteFolders = new string[folderPath.GetDirectories().Length];

        // If there's no palette folder anywhere, cancel.
        if (paletteFolders.Length == 0)
        {
            return;
        }

        for (int i = 0; i < paletteFolders.Length; i++)
        {
            paletteFolders[i] = folderPath.GetDirectories()[i].Name;
        }

        for (int i = 0; i < paletteFolders.Length; i++)
        {
            List<string> fileLists = new List<string>();

            string finalPath = setting.path + "/" + paletteFolders[i];
            string[] prefabFiles = Directory.GetFiles(finalPath, "*.prefab");
            string[] FBXFiles = Directory.GetFiles(finalPath, "*.fbx");

            for (int j = 0; j < prefabFiles.Length; j++)
            {
                fileLists.Add(prefabFiles[j]);
            }

            for (int j = 0; j < FBXFiles.Length; j++)
            {
                fileLists.Add(FBXFiles[j]);
            }

            // If the there's no files, skip to next folder.
            if (fileLists.Count == 0)
            {
                continue;
            }

            foreach (string file in fileLists)
            {
                System.Object rawObject = AssetDatabase.LoadAssetAtPath(file, typeof(GameObject));

                // Check if that object file is GameObject, then add to palette.
                if (rawObject as GameObject)
                {
                    GameObject objectPrefab = (GameObject)rawObject;
                    TilemapPalette palette = new TilemapPalette(finalPath, paletteFolders[i], objectPrefab);
                    palettesList.Add(palette);
                }
            }
        }
    }

    // Function to handle scene input.
    private void HandleSceneViewInput()
    {
        if (Event.current.type == EventType.Layout)
        {
            HandleUtility.AddDefaultControl(0);
        }
    }

    // THIS FUNCTION IS STILL IN-DEVELOPMENT.
    // Function to draw GUI on scene view.
    private void DrawGUI(Rect rect)
    {
        Handles.BeginGUI();

        float maxWidth = 200f;
        float maxHeight = 100f;

        Rect windowRect = new Rect(rect.width - maxWidth - 5f, rect.height - maxHeight - 50f, maxWidth, maxHeight);
        GUI.Box(windowRect, GUIContent.none, GUI.skin.window);

        Handles.EndGUI();
    }

    // Function to handle drag and drop.
    private void DragAndDropHandler()
    {
        Event e = Event.current;

        switch (e.type)
        {
            case EventType.DragPerform:
            DragAndDrop.AcceptDrag();

            // Determine if user is dragging Asset (Not gameobject from scene or random files)
            if (DragAndDrop.paths.Length == DragAndDrop.objectReferences.Length)
            {
                DirectoryInfo folderPath = new DirectoryInfo(setting.path);
                string[] paletteFolders = new string[folderPath.GetDirectories().Length];

                // If there's no palette folder anywhere, cancel.
                if (paletteFolders.Length == 0)
                {
                    return;
                }

                for (int i = 0; i < paletteFolders.Length; i++)
                {
                    paletteFolders[i] = folderPath.GetDirectories()[i].Name;
                }

                string destinationPath = setting.path + "/" + paletteFolders[paletteFolderIndex];
                bool update = false;

                for (int i = 0; i < DragAndDrop.objectReferences.Length; i++)
                {
                    string objectPath = DragAndDrop.paths[i];

                    // Check if drag object is prefab or FBX, copy object to current palette.
                    if (objectPath.EndsWith(".prefab") || objectPath.EndsWith(".fbx"))
                    {
                        string fileName = Path.GetFileName(objectPath);
                        string path = objectPath.Substring(0, objectPath.Length - fileName.Length - 1);

                        // Check if path are the same, then skip.
                        if (path.Equals(destinationPath))
                        {
                            continue;
                        }
                        
                        AssetDatabase.MoveAsset(objectPath, destinationPath + "/" + fileName);
                        update = true;
                    }
                }

                // If there's any update to files, refresh palette.
                if (update)
                {
                    RefreshPalette();
                }
            }

            isDragging = false;

            break;

            case EventType.DragUpdated:

            // Determine if user is dragging Asset (Not gameobject from scene or random files)
            if (DragAndDrop.paths.Length == DragAndDrop.objectReferences.Length)
            {
                bool pastable = true;

                for (int i = 0; i < DragAndDrop.objectReferences.Length; i++)
                {
                    string objectPath = DragAndDrop.paths[i];

                    // Check if drag object isn't prefab or FBX, make it not pastable.
                    if (!objectPath.EndsWith(".prefab") && !objectPath.EndsWith(".fbx"))
                    {
                        pastable = false;
                        break;
                    }
                }

                DragAndDrop.visualMode = pastable ? DragAndDropVisualMode.Move : DragAndDropVisualMode.Rejected;
            }
            else
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
            }

            isDragging = true;
            
            e.Use();
            break;

            case EventType.DragExited:
            isDragging = false;
            break;
        }

    }

    // Function to handle brush size changes.
    private void BrushSizeHandler()
    {
        Event e = Event.current;

        // If user doesn't change brush size anymore, stop the changing brush size process.
        if (currentTool != TileTool.Brush_Size)
        {
            isBrushSizing = false;
        }

        // If user using Paint/Eraser tool and hold Control, then change to Brush Size mode.
        if (currentTool == TileTool.Paint || currentTool == TileTool.Eraser)
        {
            if (e.control)
            {
                if (!isBrushSizing && e.type == EventType.MouseDown && e.button == 0)
                {
                    previousTool = currentTool;
                    currentTool = TileTool.Brush_Size;
                    brushSizePos1 = GetSelectedCell(Event.current.mousePosition);

                    if (selectionModeIndex != 0)
                    {
                        brushSizePos1.y = selectedGrid.transform.position.y + setting.gridFloor;
                    }

                    isBrushSizing = true;
                    brushSizeY = 0f;
                }
            }
        }

        // If user is changing brush size, set position of position 1 and 2.
        if (currentTool == TileTool.Brush_Size)
        {
            // If mouse cursor leaves sceneview, set size immediately.
            if (e.type == EventType.MouseLeaveWindow || e.type == EventType.MouseUp)
            {
                currentTool = previousTool;
                isBrushSizing = false;
                return;
            }

            float y = brushSizeY;

            // If user scroll wheel, change size in Y axis.
            if (e.type == EventType.ScrollWheel)
            {
                e.Use();

                // Zoom out (decrease)
                if (e.delta.y > 0f)
                {
                    y -= cellSize.y;
                }

                // Zoom in (increase)
                if (e.delta.y < 0f)
                {
                    y += cellSize.y;
                }

                y = Mathf.Clamp(y, 0f, brushLimitSize.y - 1f);
            }

            brushSizeY = y;

            Vector3Axis axis = Vector3Axis.Z;
            Vector3 transformUp = selectedGrid.transform.up;

            switch (selectedGrid.cellSwizzle)
            {
                case GridLayout.CellSwizzle.XYZ:
                transformUp = -selectedGrid.transform.forward;
                axis = Vector3Axis.Z;
                break;

                case GridLayout.CellSwizzle.XZY:
                axis = Vector3Axis.Y;
                break;

                case GridLayout.CellSwizzle.YXZ:
                transformUp = -selectedGrid.transform.forward;
                axis = Vector3Axis.Z;
                break;

                case GridLayout.CellSwizzle.YZX:
                axis = Vector3Axis.Y;
                break;

                case GridLayout.CellSwizzle.ZXY:
                transformUp = selectedGrid.transform.right;
                axis = Vector3Axis.X;
                break;

                case GridLayout.CellSwizzle.ZYX:
                transformUp = selectedGrid.transform.right;
                axis = Vector3Axis.X;
                break;
            }

            brushSizePos2 = GetSelectedCell(Event.current.mousePosition);

            // Limit brush size.
            if (Mathf.Abs(brushSizePos2.x - brushSizePos1.x) + 1 > brushLimitSize.x)
            {
                if (brushSizePos2.x >= brushSizePos1.x)
                {
                    float difference = (brushSizePos2.x - brushSizePos1.x) + 1;
                    brushSizePos2.x -= difference - brushLimitSize.x;
                }
                else
                {
                    float difference = (brushSizePos1.x - brushSizePos2.x) + 1;
                    brushSizePos2.x += difference - brushLimitSize.x;
                }
            }

            if (Mathf.Abs(brushSizePos2.z - brushSizePos1.z) + 1 > brushLimitSize.z)
            {
                if (brushSizePos2.z >= brushSizePos1.z)
                {
                    float difference = (brushSizePos2.z - brushSizePos1.z) + 1;
                    brushSizePos2.z -= difference - brushLimitSize.z;
                }
                else
                {
                    float difference = (brushSizePos1.z - brushSizePos2.z) + 1;
                    brushSizePos2.z += difference - brushLimitSize.z;
                }
            }

            // Set brush extension size based on grid transform.
            switch (axis)
            {
                case Vector3Axis.X:

                if (selectionModeIndex == 0)
                {
                    brushSizePos2.x = brushSizePos1.x;
                }
                else
                {
                    brushSizePos2.x = selectedGrid.transform.position.y + setting.gridFloor;
                }

                break;

                case Vector3Axis.Y:

                if (selectionModeIndex == 0)
                {
                    brushSizePos2.y = brushSizePos1.y;
                }
                else
                {
                    brushSizePos2.y = selectedGrid.transform.position.y + setting.gridFloor;
                }

                break;

                case Vector3Axis.Z:

                if (selectionModeIndex == 0)
                {
                    brushSizePos2.z = brushSizePos1.z;
                }
                else
                {
                    brushSizePos2.z = selectedGrid.transform.position.y + setting.gridFloor;
                }

                break;
            }

            brushSizePos2 += transformUp * y;
            VisualSelection(brushSizePos1);

            float width = brushSizePos2.x - brushSizePos1.x;
            float height = brushSizePos2.y - brushSizePos1.y;
            float length = brushSizePos2.z - brushSizePos1.z;

            width += width >= 0f ? 1f : -1f;
            height += height >= 0f ? 1f : -1f;
            length += length >= 0f ? 1f : -1f;

            cellSelectionSize = new Vector3(width, height, length);

            // When release mouse, set tool back to previous one.
            if (isBrushSizing && e.type == EventType.MouseUp && e.button == 0)
            {
                currentTool = previousTool;
                isBrushSizing = false;
            }
        }
    }

    // Function to return selected cell based on mouse position.
    private Vector3 GetSelectedCell(Vector3 position)
    {
        cellSize = new Vector3(selectedGrid.cellSize.x, selectedGrid.cellSize.y, selectedGrid.cellSize.x);

        Ray ray = HandleUtility.GUIPointToWorldRay(position); // Ray from GUI camera to world.
        RaycastHit hit; // Hit position from ray.
        Vector3 mousePosition, pos, objectOffset = new Vector3(); // Mouse position, Final position, Offset if ray hit something.
        Vector3 offset = cellSize * 0.5f; // Offset to set position to center of grid.
        float rayLength = 0f; // Length of ray.

        Physics.Raycast(ray, out hit, Mathf.Infinity, setting.brushLayer);

        // If the Selection Mode is Auto, use position on top of object's surface at mouse position (For Paint tool).
        if (selectionModeIndex == 0)
        {
            if (hit.collider)
            {
                GameObject selectedObject = hit.collider.gameObject;
                Vector3 hitPoint = hit.point;
                rayLength = -Vector3.Distance(ray.origin, hitPoint);

                // Check if the selected object has Mesh Renderer component.
                if (selectedObject.GetComponent<MeshRenderer>())
                {
                    // NOT WORK WITH CUSTOM MESH RENDERER WITH SMALLER/BIGGER THAN GRID TILE
                    /*
                    float y = selectedObject.GetComponent<MeshRenderer>().bounds.size.y;
                    Vector3 highestPoint = selectedObject.transform.position + (selectedGrid.transform.up * y);

                    // If the hit point is very near or on the top of collider, set offset to this position.
                    if (Mathf.Abs(hitPoint.y - highestPoint.y) < 0.01f && currentTool == TileTool.Paint)
                    {
                        objectOffset = selectedGrid.transform.up * (y + selectedObject.transform.position.y);
                    }
                    else
                    {
                        // If not, add offset to current object's Y position.
                        objectOffset = selectedGrid.transform.up * selectedObject.transform.position.y;
                    }
                    */

                    // Fetch raw Y position of hit object and check if should place above or same tile.
                    int hitNum = (int)GetAxis(hitPoint, Vector3Axis.Z);
                    float hitDecimal = GetAxis(hitPoint, Vector3Axis.Z) % 1;
                    float y = hitDecimal >= 0.5f && !hit.collider.isTrigger ? (float)hitNum + 1f : (float)hitNum;

                    Vector3 transformUp = selectedGrid.transform.up;

                    switch (selectedGrid.cellSwizzle)
                    {
                        case GridLayout.CellSwizzle.XYZ:
                        transformUp = selectedGrid.transform.forward;
                        break;

                        case GridLayout.CellSwizzle.YXZ:
                        transformUp = -selectedGrid.transform.forward;
                        break;

                        case GridLayout.CellSwizzle.YZX:
                        transformUp = selectedGrid.transform.up;
                        break;

                        case GridLayout.CellSwizzle.ZXY:
                        transformUp = selectedGrid.transform.right;
                        break;

                        case GridLayout.CellSwizzle.ZYX:
                        transformUp = selectedGrid.transform.right;
                        break;
                    }

                    // If it's paint tool, choose above tile.
                    if (currentTool == TileTool.Paint)
                    {
                        y -= selectedGrid.transform.position.y;
                        objectOffset = transformUp * y;
                    }
                    else
                    {
                        // For other tool (Ex. Eraser, Dropper), use current tile for better selection.
                        y = GetAxis(selectedObject.transform.position, Vector3Axis.Z) - selectedGrid.transform.position.y;
                        objectOffset = transformUp * y;
                    }
                }
            }
            else
            {
                // Use Manual mode to grid offset position.
                objectOffset = new Vector3(setting.gridOffset.x, 0f, setting.gridOffset.y);
            }
        }
        else
        {
            // Use Manual mode to grid offset position.
            objectOffset = new Vector3(setting.gridOffset.x, setting.gridFloor, setting.gridOffset.y);
        }

        // If ray length doesn't being calculate yet (not detect any object in mouse position), then use grid floor for set raycast length.
        if (rayLength == 0f)
        {
            // Create a plane based on grid position.
            float planeOffsetY = selectionModeIndex == 0 ? 0f : setting.gridFloor;
            Plane plane = new Plane(Vector3.up, selectedGrid.transform.position + (Vector3.up * planeOffsetY));
            Ray invertRay = new Ray(ray.origin, -ray.direction);

            // Raycast invert ray back to camera and return the length of ray.
            plane.Raycast(invertRay, out rayLength);
        }

        mousePosition = ray.origin - (ray.direction * rayLength);

        // If position is out of grid bound, then return.
        if (IsInfinite(mousePosition))
        {
            return Vector3.zero;
        }

        Vector3 cellPos = selectedGrid.WorldToCell(mousePosition);
        pos = Vector3.Scale(cellPos, cellSize);

        // Offset the cell position based on brush tile type.
        if (brushTileTypeIndex == 0)
        {
            pos += Vector3.Scale(Vector3.right, offset) + Vector3.Scale(Vector3.up, offset);
        }

        float newX = GetAxis(pos, Vector3Axis.X);
        float newY = GetAxis(pos, Vector3Axis.Y);
        float newZ = GetAxis(pos, Vector3Axis.Z);

        pos = new Vector3(newX, newY, newZ);
        pos += selectedGrid.transform.position + objectOffset;

        return pos;
    }

    // Function to return all position of brush size.
    private List<Vector3> GetSelection(Vector3 selectedCell, Vector3 position1, Vector3 position2)
    {
        // If the position is infinite, return empty list.
        if (IsInfinite(position1) || IsInfinite(position2))
        {
            return new List<Vector3>();
        }

        List<Vector3> selections = new List<Vector3>();

        for (float x = Mathf.Min(position1.x, position2.x); x <= Mathf.Max(position1.x, position2.x); x += cellSize.x)
        {
            for (float y = Mathf.Min(position1.y, position2.y); y <= Mathf.Max(position1.y, position2.y); y += cellSize.y)
            {
                for (float z = Mathf.Min(position1.z, position2.z); z <= Mathf.Max(position1.z, position2.z); z += cellSize.z)
                {
                    selections.Add(new Vector3(x, y, z));
                }
            }
        }

        // Rotate based on brushRotation.
        for (int i = 0; i < selections.Count; i++)
        {
            Vector3 direction = selections[i] - selectedCell;
            direction = Quaternion.Euler(Vector3.up * setting.brushRotation) * direction;
            Vector3 newPosition = selectedCell + direction;
            selections[i] = newPosition;
        }

        return selections;
    }

    // Function to fetch all game objects in list of positions.
    private Dictionary<Vector3, List<GameObject>> FindObjectsInArea(List<Vector3> selection)
    {
        Dictionary<Vector3, List<GameObject>> objectsDict = new Dictionary<Vector3, List<GameObject>>();

        List<GameObject> childs = new List<GameObject>();

        for (int i = 0; i < selectedTilemap.transform.childCount; i++)
        {
            Transform child = selectedTilemap.transform.GetChild(i);

            if (IsInLayer(setting.brushLayer, child.gameObject.layer))
            {
                childs.Add(child.gameObject);
            }
        }

        // If brush rotation isn't square, use old method. If not, use Bound method.
        if (setting.brushRotation % 90f != 0f)
        {
            // Add nearby object in tile into list.
            foreach (Vector3 pos in selection)
            {
                // If that position already in the list, skip.
                if (paintedPositions.Contains(pos))
                {
                    continue;
                }

                List<GameObject> gameObjectLists = new List<GameObject>();

                // Find all objects (without collider) around selected cell.
                Bounds bound = new Bounds(pos, cellSize * 0.99f);

                for (int i = 0; i < childs.Count; i++)
                {
                    GameObject childObject = childs[i];

                    // If that child object is in area of bound/cell, add to list.
                    if (!gameObjectLists.Contains(childObject) && bound.Contains(childObject.transform.position))
                    {
                        gameObjectLists.Add(childObject);
                    }
                }

                // If there's no object around, add position without gameobject.
                if (gameObjectLists.Count == 0)
                {
                    objectsDict.Add(pos, new List<GameObject>());
                }
                else
                {
                    objectsDict.Add(pos, gameObjectLists);
                }
            }
        }
        else
        {
            // Create a boundary based on list of vector3.
            Vector3 center = GetSelectedCell(Event.current.mousePosition);
            Vector3 size = Vector3.one;

            // If there's more than one position, find size.
            if (selection.Count > 0)
            {
                float centerX = 0f;
                float centerY = 0f;
                float centerZ = 0f;

                for (int i = 0; i < selection.Count; i++)
                {
                    centerX += selection[i].x;
                    centerY += selection[i].y;
                    centerZ += selection[i].z;
                }

                centerX /= selection.Count;
                centerY /= selection.Count;
                centerZ /= selection.Count;

                center = new Vector3(centerX, centerY, centerZ);

                float sizeX = Mathf.Abs(selection[0].x - selection[selection.Count - 1].x) + cellSize.x;
                float sizeY = Mathf.Abs(selection[0].y - selection[selection.Count - 1].y) + cellSize.y;
                float sizeZ = Mathf.Abs(selection[0].z - selection[selection.Count - 1].z) + cellSize.z;

                size = new Vector3(sizeX, sizeY, sizeZ);
            }
            
            Bounds bound = new Bounds(center, size);

            List<Vector3> newSelection = new List<Vector3>();

            // Initiate all position dict.
            for (int i = 0; i < selection.Count; i++)
            {
                if (!paintedPositions.Contains(selection[i]))
                {
                    newSelection.Add(selection[i]);
                    objectsDict.Add(selection[i], new List<GameObject>());
                    continue;
                }
            }

            for (int i = 0; i < childs.Count; i++)
            {
                // If child object isn't being add to any yet and currently in bound, add one.
                if (bound.Contains(childs[i].transform.position))
                {
                    foreach (Vector3 pos in newSelection)
                    {
                        Bounds posBound = new Bounds(pos, cellSize);

                        // If child object is in bound of that position, add to that position dict.
                        if (posBound.Contains(childs[i].transform.position))
                        {
                            // If it's not contain inside yet, add one.
                            if (!objectsDict[pos].Contains(childs[i]))
                            {
                                objectsDict[pos].Add(childs[i]);
                            }
                            break;
                        }
                    }
                }
            }
        }

        return objectsDict;
    }

    // Function to find game objects in that position.
    private List<GameObject> FindObjectsInPosition(Vector3 position)
    {
        List<GameObject> gameObjectLists = new List<GameObject>();

        // Find all objects (without collider) around selected cell.
        Bounds bound = new Bounds(position, cellSize * 0.99f);

        for (int i = 0; i < selectedTilemap.transform.childCount; i++)
        {
            GameObject childObject = selectedTilemap.transform.GetChild(i).gameObject;

            // If that child object is in area of bound/cell, add to list.
            if (bound.Contains(childObject.transform.position))
            {
                // If this object isn't in the list and in brush layer, continue.
                if (!gameObjectLists.Contains(childObject) && IsInLayer(setting.brushLayer, childObject.layer))
                {
                    gameObjectLists.Add(childObject);
                }
            }
        }

        return gameObjectLists;
    }

    // Function to determine if position is infinite or out of world bound.
    private bool IsInfinite(Vector3 position)
    {
        return position.x >= Mathf.Infinity || position.x <= Mathf.NegativeInfinity ||
            position.y >= Mathf.Infinity || position.y <= Mathf.NegativeInfinity ||
            position.z >= Mathf.Infinity || position.z <= Mathf.NegativeInfinity;
    }

    // Function to fetch axis based on cell swizzle of selected grid.
    private float GetAxis(Vector3 vector, Vector3Axis axis)
    {
        switch (selectedGrid.cellSwizzle)
        {
            case GridLayout.CellSwizzle.XYZ:
            switch (axis)
            {
                case Vector3Axis.X:
                return vector.x;

                case Vector3Axis.Y:
                return vector.y;

                case Vector3Axis.Z:
                return vector.z;
            }

            break;

            case GridLayout.CellSwizzle.XZY:
            switch (axis)
            {
                case Vector3Axis.X:
                return vector.x;

                case Vector3Axis.Y:
                return vector.z;

                case Vector3Axis.Z:
                return vector.y;
            }

            break;

            case GridLayout.CellSwizzle.YXZ:
            switch (axis)
            {
                case Vector3Axis.X:
                return vector.y;

                case Vector3Axis.Y:
                return vector.x;

                case Vector3Axis.Z:
                return vector.z;
            }

            break;

            case GridLayout.CellSwizzle.YZX:
            switch (axis)
            {
                case Vector3Axis.X:
                return vector.y;

                case Vector3Axis.Y:
                return vector.z;

                case Vector3Axis.Z:
                return vector.x;
            }

            break;

            case GridLayout.CellSwizzle.ZXY:
            switch (axis)
            {
                case Vector3Axis.X:
                return vector.z;

                case Vector3Axis.Y:
                return vector.x;

                case Vector3Axis.Z:
                return vector.y;
            }

            break;

            case GridLayout.CellSwizzle.ZYX:
            switch (axis)
            {
                case Vector3Axis.X:
                return vector.z;

                case Vector3Axis.Y:
                return vector.y;

                case Vector3Axis.Z:
                return vector.x;
            }
            break;
        }

        return 0f;
    }

    // Function determine if layer is in layermask.
    private bool IsInLayer(LayerMask layerMask, int layer) => (layerMask.value & (1 << layer)) > 0;

    // Function to destroy preview object.
    private void DestroyPreview()
    {
        if (currentPreviewObject)
        {
            DestroyImmediate(currentPreviewObject);
        }
    }

    // Function to load Tilemap data.
    private void LoadTilemapData()
    {
        string tilemapData = EditorPrefs.GetString("TilemapData", JsonUtility.ToJson(this.setting, false));
        string tilemapGroupBrushData = EditorPrefs.GetString("TilemapGroupBrushData", JsonUtility.ToJson(groupBrush, false));

        JsonUtility.FromJsonOverwrite(tilemapData, this);
        JsonUtility.FromJsonOverwrite(tilemapGroupBrushData, groupBrush);
    }

    // Function to save Tilemap data.
    private void SaveTilemapData()
    {
        string tileMapData = JsonUtility.ToJson(this, false);
        string tileMapGroupBrushData = JsonUtility.ToJson(groupBrush, false);

        EditorPrefs.SetString("TilemapData", tileMapData);
        EditorPrefs.SetString("TilemapGroupBrushData", tileMapGroupBrushData);
    }

    // Function to clear Tilemap data.
    private static void ClearTilemapData()
    {
        EditorPrefs.SetString("TilemapData", null);
        EditorPrefs.SetString("TilemapGroupBrushData", null);

        Debug.Log("Cleared Tilemap Data");
    }
}

[Serializable]
public class TilemapSetting
{
    public Vector2 gridOffset = new Vector2(); // Offset from grid.
    public int gridFloor = 0; // Current floor from grid. (Default = 0)
    public BrushTileType brushTileType = BrushTileType.Center; // Type of brush tile (Center = Center of the tile, Corner = Corner of the tile)
    public Vector3 brushOffset = new Vector3(); // Offset from selected tile.
    public float brushRotation = 0f; // Rotation of prefab.
    public float brushRotationAmount = 45f; // Amount of rotation.
    public LayerMask brushLayer = ~0; // Layer that will be focus for selection and action.

    public bool showGrid = true; // Determine to show grid at selected cell or not.
    public Color gridColor = Color.white; // Color of grid.
    public float gridThickness = 0.05f; // Line thickness of grid.

    public string path = "Assets/Prefabs"; // Path to load all palettes (prefabs).
    public bool isMechanicObject = false; // Determine if current palette is mechanic object.
    public bool isReplaceMode = false; // Determine if paint tool is in replace mode or not.
    public float paletteWindowSize = 100f; // Current palette window size.
}

public struct TilemapPalette
{
    public string path; // Path of this prefab.
    public string directory; // Directory name of this prefab.
    public GameObject prefab; // Prefab game object.

    public TilemapPalette(string path, string directory, GameObject prefab)
    {
        this.path = path;
        this.directory = directory;
        this.prefab = prefab;
    }
}
#endif