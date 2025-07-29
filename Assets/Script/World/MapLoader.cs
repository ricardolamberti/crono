using UnityEngine;

using System.Collections; 
using System.Collections.Generic;
using UnityEngine.Networking;
using static MapGenerator;
using static DTO;
using Unity.VisualScripting.Antlr3.Runtime.Tree;



public class MapLoader : MonoBehaviour
{
    public string mapUrl = "http://localhost:8080/map/123";
    public GameObject tilePrefab;
    public Transform mapParent;

    public Sprite forestSprite;
    public Sprite mountainSprite;
    public Sprite waterSprite;
    public Sprite defaultSprite;

  
    public Sprite hutSprite;
    public Sprite barracksSprite;
    public Sprite mineSprite;
    public Sprite farmSprite;
    public Sprite sawmillSprite;
    public Sprite defaultBuildingSprite;
    public Sprite academySprite;
    public Sprite extractorSprite;
    public Sprite dockSprite;
    public Sprite atalayaSprite;
    public Sprite wallSprite;

    public Sprite hutSprite2;
    public Sprite barracksSprite2;
    public Sprite mineSprite2;
    public Sprite farmSprite2;
    public Sprite sawmillSprite2;
    public Sprite defaultBuildingSprite2;
    public Sprite academySprite2;
    public Sprite extractorSprite2;
    public Sprite dockSprite2;
    public Sprite atalayaSprite2;
    public Sprite wallSprite2;

    public Sprite hutSprite3;
    public Sprite barracksSprite3;
    public Sprite mineSprite3;
    public Sprite farmSprite3;
    public Sprite sawmillSprite3;
    public Sprite defaultBuildingSprite3;
    public Sprite academySprite3;
    public Sprite extractorSprite3;
    public Sprite dockSprite3;
    public Sprite atalayaSprite3;
    public Sprite wallSprite3;

    public Sprite hutSprite4;
    public Sprite barracksSprite4;
    public Sprite mineSprite4;
    public Sprite farmSprite4;
    public Sprite sawmillSprite4;
    public Sprite defaultBuildingSprite4;
    public Sprite academySprite4;
    public Sprite extractorSprite4;
    public Sprite dockSprite4;
    public Sprite atalayaSprite4;
    public Sprite wallSprite4;

    public Sprite airportSprite;
    public Sprite airportSprite2;

    public Sprite iconGold;
    public Sprite iconWood;
    public Sprite iconCrono;


    public GameObject characterPrefab;
    public Sprite workerSprite;
    public Sprite scientistSprite;
    public Sprite warriorSprite;

   
    public Sprite[] workerNorthSprites;
    public Sprite[] workerSouthSprites;
    public Sprite[] workerEastSprites;
    public Sprite[] workerWestSprites;

    public Sprite[] scientistNorthSprites;
    public Sprite[] scientistSouthSprites;
    public Sprite[] scientistEastSprites;
    public Sprite[] scientistWestSprites;

    public Sprite[] warriorNorthSprites;
    public Sprite[] warriorSouthSprites;
    public Sprite[] warriorEastSprites;
    public Sprite[] warriorWestSprites;

    public static MapLoader instance;
    
    private Dictionary<string, Sprite> buildingSprites;
    private Dictionary<string, Sprite> terrainSprites;
    private Dictionary<Vector2Int, GameObject> tiles = new();

    void Awake()
    {
        instance = this;
    }


    void Start()
    {
        terrainSprites = new Dictionary<string, Sprite> {
            { "forest", forestSprite },
            { "mountain", mountainSprite },
            { "water", waterSprite }
        };
        buildingSprites = new Dictionary<string, Sprite>
        {
            { "hut_1", hutSprite },
            { "hut_2", hutSprite2 },
            { "hut_3", hutSprite3 },
            { "hut_4", hutSprite4 },

            { "dock_1", dockSprite },
            { "dock_2", dockSprite2 },
            { "dock_3", dockSprite3 },
            { "dock_4", dockSprite4 },
            
            { "atalaya_1", atalayaSprite },
            { "atalaya_2", atalayaSprite2 },
            { "atalaya_3", atalayaSprite3 },
            { "atalaya_4", atalayaSprite4 },

            { "barracks_1", barracksSprite },
            { "barracks_2", barracksSprite2 },
            { "barracks_3", barracksSprite3 },
            { "barracks_4", barracksSprite4 },

            { "mine_1", mineSprite },
            { "mine_2", mineSprite2 },
            { "mine_3", mineSprite3 },
            { "mine_4", mineSprite4 },

            { "advanced_mine_1", mineSprite },
            { "advanced_mine_2", mineSprite2 },
            { "advanced_mine_3", mineSprite3 },
            { "advanced_mine_4", mineSprite4 },

            { "farm_1", farmSprite },
            { "farm_2", farmSprite2 },
            { "farm_3", farmSprite3 },
            { "farm_4", farmSprite4 },

            { "lumbermill_1", sawmillSprite },
            { "lumbermill_2", sawmillSprite2 },
            { "lumbermill_3", sawmillSprite3 },
            { "lumbermill_4", sawmillSprite4 },

            { "academy_1", academySprite },
            { "academy_2", academySprite2 },
            { "academy_3", academySprite3 },
            { "academy_4", academySprite4 },

            { "extractor_1", extractorSprite },
            { "extractor_2", extractorSprite2 },
            { "extractor_3", extractorSprite3 },
            { "extractor_4", extractorSprite4 },

            { "airport_1", airportSprite },
            { "airport_2", airportSprite2 },
          
            { "townhall_1", defaultBuildingSprite },
            { "townhall_2", defaultBuildingSprite2 },
            { "townhall_3", defaultBuildingSprite3 },
            { "townhall_4", defaultBuildingSprite4 },

             { "wall_1", wallSprite },
            { "wall_2", wallSprite2 },
            { "wall_3", wallSprite3 },
            { "wall_4", wallSprite4 }


         };


        StartCoroutine(LoadMap());

    }

    IEnumerator LoadMap()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("map_data");
        if (jsonFile == null)
        {
            Debug.LogError("Archivo de mapa no encontrado");
            yield return null;
        }


        MapCellListWrapper wrapper = JsonUtility.FromJson<MapCellListWrapper>(jsonFile.text);

        if (wrapper == null || wrapper.cells == null)
        {
            Debug.LogError("Error al parsear JSON embebido");
            yield break;

        }
        MapState.cellMap.Clear();
        foreach (var cell in wrapper.cells)
        {
            if (cell.level <= 0)
                cell.level = 1;
            Vector2Int coord = new(cell.x, cell.y);
            MapState.cellMap[coord] = cell;
        }

        GenerateGrid(wrapper.cells);
        CenterCamera(wrapper.cells);
        PlayerManager.Instance?.InitializePlayers();
        yield return null;
    }
    public void ShowConstructionPreview(Vector2Int pos, string building, int level = 1)
    {
        if (!tiles.TryGetValue(pos, out var tile)) return;

        GameObject preview = new GameObject($"Preview_{building}");
        preview.transform.SetParent(tile.transform);
        preview.transform.localPosition = new Vector3(-0.5f, .7f, -0.25f);
        preview.transform.localScale = Vector3.one * 0.35f;
        preview.transform.localRotation = Quaternion.Euler(-32, 0, 32);

        var renderer = preview.AddComponent<SpriteRenderer>();
        string key = $"{building}_{level}";
        renderer.sprite = buildingSprites.ContainsKey(key) ? buildingSprites[key] : defaultBuildingSprite;
        renderer.color = new Color(1f, 1f, 1f, 0.4f); // semitransparente
        renderer.sortingOrder = 8;

        tile.GetComponent<TileClickHandler>().previewObject = preview;
    }
    public void ClearConstructionPreview(Vector2Int pos)
    {
        if (!tiles.TryGetValue(pos, out var tile)) return;

        var handler = tile.GetComponent<TileClickHandler>();
        if (handler != null && handler.previewObject != null)
        {
            GameObject.Destroy(handler.previewObject);
            handler.previewObject = null;
        }
    }

    void CenterCamera(List<MapCellDTO> cells)
    {
        if (cells == null || cells.Count == 0) return;

        float minX = float.MaxValue, maxX = float.MinValue;
        float minZ = float.MaxValue, maxZ = float.MinValue;

        foreach (var cell in cells)
        {
            if (cell.x < minX) minX = cell.x;
            if (cell.x > maxX) maxX = cell.x;
            if (cell.y < minZ) minZ = cell.y;
            if (cell.y > maxZ) maxZ = cell.y;
        }

        Vector3 center = new Vector3((minX + maxX) / 2f, 0, (minZ + maxZ) / 2f);
        Vector3 offset = new Vector3(10, 10, -10); // Ajustá este ángulo si querés otro punto de vista

        Camera.main.transform.position = center + offset;
        Camera.main.transform.LookAt(center);
    }

    void GenerateGrid(List<MapCellDTO> cells)
    {

        foreach (var cell in cells)
        {
            Vector2Int coord = new(cell.x, cell.y);

            GameObject tile = Instantiate(tilePrefab, mapParent);
            float tileWidth = 2.5f;  // Ajustalo a lo que mida tu sprite en X
            float tileHeight = 2.5f; // Para isométrico puede ser menor

            Vector3 pos = new Vector3(cell.x * tileWidth, 0, cell.y * tileHeight);
            tile.transform.position = pos;
            tile.name = $"Tile_{cell.x}_{cell.y}";
            tile.AddComponent<TileClickHandler>().SetData(cell);
            tile.AddComponent<TileActionProvider>();

            ApplyTerrain(tile, cell.terrain);
            ApplyBuilding(tile, cell.building, cell.level);

            var buildingObj = BuildingFactory.Create(cell.building, cell.level);
            if (buildingObj != null)
                MapState.buildings[coord] = buildingObj;

            tiles[coord] = tile;

            if (cell.resources != null)
            {
                //if (cell.resources.gold > 0 && iconGold != null)
                //    DrawResourceIcon(tile, "gold", iconGold);
                if (cell.resources.wood > 0 && treeSprites.Count > 0)
                {
                    int count = Mathf.Clamp(cell.resources.wood / 10, 1, 5);
                    DrawTrees(tile, count);
                }

                if (cell.resources.crono > 0 && iconCrono != null)
                    DrawResourceIcon(tile, "crono", iconCrono);
            }

        }

        // Después de crear todos los tiles
        Vector3 center = new Vector3(1, 0, 1); // Cambiá según el centro real de tu mapa
        Camera.main.transform.position = center + new Vector3(5, 10, -5); // Ángulo en diagonal
        Camera.main.transform.LookAt(center); // Apunta al centro del mapa

    }
    public bool IsPositionFree(Vector2Int pos)
    {
        // 1. Que exista el tile
        if (!MapState.cellMap.ContainsKey(pos))
            return false;

        var cell = MapState.cellMap[pos];

        // 2. Que no tenga edificio
        if (!string.IsNullOrEmpty(cell.building))
            return false;

        // 3. Que no haya un personaje en esa celda
        foreach (var character in GameObject.FindObjectsOfType<Character>())
        {
            if (character.GetGridPosition() == pos)
                return false;
        }

        return true;
    }

    void ApplyTerrain(GameObject tile, string terrain)
    {

        var spriteRenderer = tile.GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer == null) return;

        if (terrainSprites != null && terrainSprites.ContainsKey(terrain))
            spriteRenderer.sprite = terrainSprites[terrain];
        else
            spriteRenderer.sprite = defaultSprite;
       
    }
    void ApplyBuilding(GameObject tile, string building, int level)
    {
        if (string.IsNullOrEmpty(building)) return;

        GameObject buildingIcon = new GameObject($"Building_{building}");
        buildingIcon.transform.SetParent(tile.transform);
        buildingIcon.transform.localPosition = new Vector3(0, 1f, -0.25f);
        buildingIcon.transform.localScale = Vector3.one * 0.45f;
        buildingIcon.transform.localRotation = Quaternion.Euler(-90, -30, 40);
        buildingIcon.tag = building;
        var renderer = buildingIcon.AddComponent<SpriteRenderer>();
        string key = $"{building}_{level}";
        renderer.sprite = buildingSprites.ContainsKey(key) ? buildingSprites[key] : defaultBuildingSprite;
        renderer.sortingOrder = 10;

    }

    void DrawTrees(GameObject tile, int treeCount)
    {
        for (int i = 0; i < treeCount; i++)
        {
            GameObject tree = new GameObject($"Tree_{i}");
            var renderer = tree.AddComponent<SpriteRenderer>();
            renderer.sprite = treeSprites[Random.Range(0, treeSprites.Count)];
            renderer.sortingOrder = Mathf.RoundToInt(-transform.position.z * 100);

            tree.transform.SetParent(tile.transform);

            // ❗ Posición aleatoria dentro del tile (descentrado, XZ)
            tree.transform.localPosition = new Vector3(
                Random.Range(-0.4f, 0.4f),
                0.01f,
                Random.Range(-0.4f, 0.4f)
            );

            // ❗ Escala aleatoria
            float scale = Random.Range(0.3f, 0.6f);
            tree.transform.localScale = new Vector3(scale, scale, scale);
            tree.AddComponent<Billboard>();
          

            // ❗ Rotación levemente distinta por árbol (solo para romper simetría visual)
            tree.transform.localRotation = Quaternion.Euler(
                90,
                Random.Range(-15f, 15f), // rotación suave sobre eje vertical
                0
            );
        }
    }
    public void DrawBuilding(Vector2Int pos, string building, int level)
    {
        if (!tiles.TryGetValue(pos, out var tile)) return;

        GameObject buildingIcon = new GameObject($"Building_{building}");
        buildingIcon.transform.SetParent(tile.transform);
        buildingIcon.transform.localPosition = new Vector3(-0.5f, .7f, -0.25f);
        buildingIcon.transform.localScale = Vector3.one * 0.35f;
        buildingIcon.transform.localRotation = Quaternion.Euler(-32, 0, 32);

        var renderer = buildingIcon.AddComponent<SpriteRenderer>();
        string key = $"{building}_{level}";
        renderer.sprite = buildingSprites.ContainsKey(key) ? buildingSprites[key] : defaultBuildingSprite;
        renderer.sortingOrder = 10;

        var obj = BuildingFactory.Create(building, level);
        if (obj != null)
            MapState.buildings[pos] = obj;
    }


    public List<Sprite> treeSprites;
    void DrawResourceIcon(GameObject tile, string resource, Sprite icon)
    {
        GameObject iconObj = new GameObject($"Icon_{resource}");
        var renderer = iconObj.AddComponent<SpriteRenderer>();
        renderer.sprite = icon;
        renderer.sortingOrder = 5;
        iconObj.transform.SetParent(tile.transform);
        iconObj.transform.localPosition = new Vector3(0.5f, 0.5f, 0f);
        iconObj.transform.localScale = Vector3.one * 0.15f;
        iconObj.transform.localRotation = Quaternion.Euler(-90, -30, 40);
        iconObj.AddComponent<Billboard>();


    }


    public void SpawnCharacter(Vector2Int pos, Character.Type type, string owner)
    {
        GameObject go = Instantiate(characterPrefab);
        go.transform.position = GridUtils.GridToWorld(pos);
        go.transform.localScale = Vector3.one;

        var character = go.GetComponent<Character>();
        var renderer = go.GetComponent<SpriteRenderer>();
        var animator = go.GetComponent<CharacterAnimator>();

        if (!go.TryGetComponent(out HealthComponent _))
        {
            go.AddComponent<HealthComponent>();
        }



        // Asignar sprites al animator
        CharacterRole role = null;
        if (type == Character.Type.Worker && animator != null)
        {
            go.tag = "Worker";
            animator.spriteRenderer = renderer;
            animator.northSprites = workerNorthSprites;
            animator.southSprites = workerSouthSprites;
            animator.eastSprites = workerEastSprites;
            animator.westSprites = workerWestSprites;
            role = go.AddComponent<WorkerRole>();
        }
        if (type == Character.Type.Scientist && animator != null)
        {
            go.tag = "Scientist";
            animator.spriteRenderer = renderer;
            animator.northSprites = scientistNorthSprites;
            animator.southSprites = scientistSouthSprites;
            animator.eastSprites = scientistEastSprites;
            animator.westSprites = scientistWestSprites;
            role = go.AddComponent<ScientistRole>();
        }
        if (type == Character.Type.Warrior && animator != null)
        {
            go.tag = "Soldier";
            animator.spriteRenderer = renderer;
            animator.northSprites = warriorNorthSprites;
            animator.southSprites = warriorSouthSprites;
            animator.eastSprites = warriorEastSprites;
            animator.westSprites = warriorWestSprites;
            role = go.AddComponent<WarriorRole>();
        }
        character.LoadSprites(new Dictionary<string, Sprite[]>
            {
                { "north", animator.northSprites },
                { "south", animator.southSprites },
                { "east",  animator.eastSprites },
                { "west",  animator.westSprites }
            });


        if (role != null)
            character.Init(role, owner);
        else
            character.Init(type, owner);

        go.transform.LookAt(Camera.main.transform);
        go.transform.rotation = Quaternion.Euler(0, go.transform.rotation.eulerAngles.y, 0);
    }

    public void DemolishBuilding(Vector2Int pos)
    {
        if (!MapState.cellMap.TryGetValue(pos, out var cell))
            return;

        if (cell.building == "townhall")
        {
            Debug.LogWarning("No se puede destruir la casa central.");
            return;
        }

        string oldBuilding = cell.building;
        cell.building = null;
        GameState.DecrementBuilding(oldBuilding);
        MapState.buildings.Remove(pos);

        if (tiles.TryGetValue(pos, out var tile))
        {
            foreach (Transform child in tile.transform)
            {
                if (child.name.StartsWith("Building_"))
                {
                    Destroy(child.gameObject);
                    break;
                }
            }
        }

        Debug.Log($"Edificio en {pos} ha sido demolido.");
    }

    public void UpgradeBuilding(Vector2Int pos, string newBuilding, int level)
    {
        if (!MapState.cellMap.TryGetValue(pos, out var cell))
            return;

        string old = cell.building;
        cell.building = newBuilding;
        cell.level = level;
        GameState.DecrementBuilding(old);
        GameState.IncrementBuilding(newBuilding);
        MapState.buildings[pos] = BuildingFactory.Create(newBuilding, level);

        if (tiles.TryGetValue(pos, out var tile))
        {
            foreach (Transform child in tile.transform)
            {
                if (child.name.StartsWith("Building_"))
                {
                    Destroy(child.gameObject);
                    break;
                }
            }
        }

        DrawBuilding(pos, newBuilding, level);
        Debug.Log($"Edificio en {pos} actualizado de {old} a {newBuilding}");
    }


}
