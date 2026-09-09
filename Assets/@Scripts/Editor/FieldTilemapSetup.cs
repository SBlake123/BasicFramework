using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

[InitializeOnLoad]
public static class FieldTilemapSetup
{
    private const string SpritePath = "Assets/@Assets/Pixel Adventure 1/Assets/Background/Green 1.png";
    private const string TileFolderPath = "Assets/@Tiles";
    private const string TilePath = TileFolderPath + "/Green Ground.asset";
    private const string AutoSetupKey = "BasicFramework.FieldTilemapSetup.v1";

    private const int FieldWidth = 180;
    private const int FieldHeight = 90;

    static FieldTilemapSetup()
    {
        EditorApplication.delayCall += TryAutoSetup;
    }

    [MenuItem("Tools/Field/Build Green Tilemap")]
    public static void BuildFromMenu()
    {
        if (TryBuild())
        {
            EditorPrefs.SetBool(AutoSetupKey, true);
        }
    }

    private static void TryAutoSetup()
    {
        if (EditorPrefs.GetBool(AutoSetupKey, false) || EditorApplication.isPlayingOrWillChangePlaymode)
        {
            return;
        }

        if (TryBuild())
        {
            EditorPrefs.SetBool(AutoSetupKey, true);
        }
    }

    private static bool TryBuild()
    {
        Tilemap tilemap = FindFieldTilemap();
        if (tilemap == null)
        {
            Debug.LogWarning("Field 아래의 Grid/Tilemap을 찾지 못했습니다. 만든 뒤 Tools > Field > Build Green Tilemap을 실행하세요.");
            return false;
        }

        Sprite sprite = ImportGroundSprite();
        if (sprite == null)
        {
            Debug.LogError($"바닥 스프라이트를 불러오지 못했습니다: {SpritePath}");
            return false;
        }

        Tile tile = LoadOrCreateTile(sprite);
        Fill(tilemap, tile);

        Grid grid = tilemap.GetComponentInParent<Grid>();
        grid.cellSize = Vector3.one;
        grid.transform.localPosition = Vector3.zero;
        tilemap.transform.localPosition = Vector3.zero;

        TilemapRenderer renderer = tilemap.GetComponent<TilemapRenderer>();
        if (renderer != null)
        {
            renderer.sortingOrder = -10;
        }

        EditorUtility.SetDirty(grid);
        EditorUtility.SetDirty(tilemap);
        if (renderer != null)
        {
            EditorUtility.SetDirty(renderer);
        }

        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(tilemap.gameObject.scene);
        EditorSceneManager.SaveScene(tilemap.gameObject.scene);
        Selection.activeGameObject = tilemap.gameObject;
        SceneView.lastActiveSceneView?.FrameSelected();

        Debug.Log($"Field Tilemap 생성 완료: {FieldWidth} x {FieldHeight} 월드 유닛");
        return true;
    }

    private static Tilemap FindFieldTilemap()
    {
        Grid[] grids = Object.FindObjectsOfType<Grid>(true);
        foreach (Grid grid in grids)
        {
            if (!grid.gameObject.scene.IsValid() || grid.transform.parent == null)
            {
                continue;
            }

            if (grid.transform.parent.name == "Field")
            {
                Tilemap fieldTilemap = grid.GetComponentInChildren<Tilemap>(true);
                if (fieldTilemap != null)
                {
                    return fieldTilemap;
                }
            }
        }

        return null;
    }

    private static Sprite ImportGroundSprite()
    {
        TextureImporter importer = AssetImporter.GetAtPath(SpritePath) as TextureImporter;
        if (importer == null)
        {
            return null;
        }

        bool requiresReimport = importer.textureType != TextureImporterType.Sprite
                                || importer.spriteImportMode != SpriteImportMode.Single
                                || !Mathf.Approximately(importer.spritePixelsPerUnit, 64f);

        if (requiresReimport)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 64f;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }

        return AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath);
    }

    private static Tile LoadOrCreateTile(Sprite sprite)
    {
        if (!AssetDatabase.IsValidFolder(TileFolderPath))
        {
            AssetDatabase.CreateFolder("Assets", "@Tiles");
        }

        Tile tile = AssetDatabase.LoadAssetAtPath<Tile>(TilePath);
        if (tile == null)
        {
            tile = ScriptableObject.CreateInstance<Tile>();
            tile.name = "Green Ground";
            tile.sprite = sprite;
            AssetDatabase.CreateAsset(tile, TilePath);
        }
        else
        {
            tile.sprite = sprite;
            EditorUtility.SetDirty(tile);
        }

        return tile;
    }

    private static void Fill(Tilemap tilemap, TileBase tile)
    {
        int startX = -FieldWidth / 2;
        int startY = -FieldHeight / 2;
        BoundsInt bounds = new BoundsInt(startX, startY, 0, FieldWidth, FieldHeight, 1);
        TileBase[] tiles = new TileBase[FieldWidth * FieldHeight];

        for (int i = 0; i < tiles.Length; i++)
        {
            tiles[i] = tile;
        }

        Undo.RegisterCompleteObjectUndo(tilemap, "Build Green Field Tilemap");
        tilemap.ClearAllTiles();
        tilemap.SetTilesBlock(bounds, tiles);
        tilemap.CompressBounds();
    }
}
