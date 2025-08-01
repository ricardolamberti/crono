using UnityEngine;

using System.Collections; 
using System.Collections.Generic;
using UnityEngine.Networking;
using static MapGenerator;
using GameConstants;
using static DTO;

// Orientation enum used for bridges/walls
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
    public Sprite wallSpriteVertical;

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
    public Sprite wallSprite2Vertical;

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
    public Sprite wallSprite3Vertical;

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
    public Sprite wallSprite4Vertical;

    // Sprites for bridges. Horizontal corresponds to the normal orientation
    // while vertical is the inverted version used instead of rotating the sprite
    public Sprite bridgeHorizontalSprite;
    public Sprite bridgeVerticalSprite;

    public Sprite airportSprite;
    public Sprite airportSprite2;

    public Sprite temporalBreachSprite;

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

    public DirectionalSprites soldadoRasoSprites;
    public DirectionalSprites arqueroSprites;
    public DirectionalSprites tanqueSprites;
    public DirectionalSprites eliteSprites;
    public DirectionalSprites destructorSprites;

    public Sprite arrowWeaponSprite;
    public Sprite bulletWeaponSprite;
    public Sprite rayWeaponSprite;

    private Dictionary<WarriorClass, DirectionalSprites> warriorSpriteMap;
    private Dictionary<WeaponType, Sprite> weaponSpriteMap;

    public static MapLoader instance;
    
    private Dictionary<string, Sprite> buildingSprites;
    private Dictionary<string, Sprite> terrainSprites;
    private Dictionary<Vector2Int, GameObject> tiles = new();

    void Awake()
    {
        instance = this;

        warriorSpriteMap = new Dictionary<WarriorClass, DirectionalSprites>
        {
            { WarriorClass.SoldadoRaso, soldadoRasoSprites },
            { WarriorClass.Arquero, arqueroSprites },
            { WarriorClass.Tanque, tanqueSprites },
            { WarriorClass.Elite, eliteSprites },
            { WarriorClass.Destructor, destructorSprites }
        };

        weaponSpriteMap = new Dictionary<WeaponType, Sprite>
        {
            { WeaponType.Arrow, arrowWeaponSprite },
            { WeaponType.Bullet, bulletWeaponSprite },
            { WeaponType.Ray, rayWeaponSprite }
        };
    }

    public Sprite GetWeaponSprite(WeaponType type)
    {
        if (weaponSpriteMap != null && weaponSpriteMap.TryGetValue(type, out var s))
            return s;
        return null;
    }


    void Start()
    {
        terrainSprites = new Dictionary<string, Sprite> {
            { TerrainTypes.Forest, forestSprite },
            { TerrainTypes.Mountain, mountainSprite },
            { TerrainTypes.Water, waterSprite }
        };
        buildingSprites = new Dictionary<string, Sprite>
        {
            { $"{BuildingCodes.Hut}_1", hutSprite },
            { $"{BuildingCodes.Hut}_2", hutSprite2 },
            { $"{BuildingCodes.Hut}_3", hutSprite3 },
            { $"{BuildingCodes.Hut}_4", hutSprite4 },

            { $"{BuildingCodes.Dock}_1", dockSprite },
            { $"{BuildingCodes.Dock}_2", dockSprite2 },
            { $"{BuildingCodes.Dock}_3", dockSprite3 },
            { $"{BuildingCodes.Dock}_4", dockSprite4 },
            
            { $"{BuildingCodes.Atalaya}_1", atalayaSprite },
            { $"{BuildingCodes.Atalaya}_2", atalayaSprite2 },
            { $"{BuildingCodes.Atalaya}_3", atalayaSprite3 },
            { $"{BuildingCodes.Atalaya}_4", atalayaSprite4 },

            { $"{BuildingCodes.Barracks}_1", barracksSprite },
            { $"{BuildingCodes.Barracks}_2", barracksSprite2 },
            { $"{BuildingCodes.Barracks}_3", barracksSprite3 },
            { $"{BuildingCodes.Barracks}_4", barracksSprite4 },

            { $"{BuildingCodes.Mine}_1", mineSprite },
            { $"{BuildingCodes.Mine}_2", mineSprite2 },
            { $"{BuildingCodes.Mine}_3", mineSprite3 },
            { $"{BuildingCodes.Mine}_4", mineSprite4 },

            { $"{BuildingCodes.AdvancedMine}_1", mineSprite },
            { $"{BuildingCodes.AdvancedMine}_2", mineSprite2 },
            { $"{BuildingCodes.AdvancedMine}_3", mineSprite3 },
            { $"{BuildingCodes.AdvancedMine}_4", mineSprite4 },

            { $"{BuildingCodes.Farm}_1", farmSprite },
            { $"{BuildingCodes.Farm}_2", farmSprite2 },
            { $"{BuildingCodes.Farm}_3", farmSprite3 },
            { $"{BuildingCodes.Farm}_4", farmSprite4 },

            { $"{BuildingCodes.Lumbermill}_1", sawmillSprite },
            { $"{BuildingCodes.Lumbermill}_2", sawmillSprite2 },
            { $"{BuildingCodes.Lumbermill}_3", sawmillSprite3 },
            { $"{BuildingCodes.Lumbermill}_4", sawmillSprite4 },

            { $"{BuildingCodes.Academy}_1", academySprite },
            { $"{BuildingCodes.Academy}_2", academySprite2 },
            { $"{BuildingCodes.Academy}_3", academySprite3 },
            { $"{BuildingCodes.Academy}_4", academySprite4 },

            { $"{BuildingCodes.Extractor}_1", extractorSprite },
            { $"{BuildingCodes.Extractor}_2", extractorSprite2 },
            { $"{BuildingCodes.Extractor}_3", extractorSprite3 },
            { $"{BuildingCodes.Extractor}_4", extractorSprite4 },

            { $"{BuildingCodes.Bridge}_H", bridgeHorizontalSprite },
            { $"{BuildingCodes.Bridge}_V", bridgeVerticalSprite },

            { $"{BuildingCodes.Airport}_1", airportSprite },
            { $"{BuildingCodes.Airport}_2", airportSprite2 },

            { $"{BuildingCodes.TemporalBreach}_1", temporalBreachSprite },
          
            { $"{BuildingCodes.Townhall}_1", defaultBuildingSprite },
            { $"{BuildingCodes.Townhall}_2", defaultBuildingSprite2 },
            { $"{BuildingCodes.Townhall}_3", defaultBuildingSprite3 },
            { $"{BuildingCodes.Townhall}_4", defaultBuildingSprite4 },

             { $"{BuildingCodes.Wall}_1_H", wallSprite },
            { $"{BuildingCodes.Wall}_1_V", wallSpriteVertical },
            { $"{BuildingCodes.Wall}_2_H", wallSprite2 },
            { $"{BuildingCodes.Wall}_2_V", wallSprite2Vertical },
            { $"{BuildingCodes.Wall}_3_H", wallSprite3 },
            { $"{BuildingCodes.Wall}_3_V", wallSprite3Vertical },
            { $"{BuildingCodes.Wall}_4_H", wallSprite4 },
            { $"{BuildingCodes.Wall}_4_V", wallSprite4Vertical }


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
    public void ShowConstructionPreview(Vector2Int pos, Building building)
    {
        if (building == null) return;
        if (!tiles.TryGetValue(pos, out var tile)) return;

        GameObject preview = new GameObject($"Preview_{building.Code}");
        preview.transform.SetParent(tile.transform);
        preview.transform.localPosition = new Vector3(-0.5f, .7f, -0.25f);
        preview.transform.localScale = Vector3.one * 0.35f;
        float yRot = building.ShouldRotate ? (building.Orientation == Orientation.Horizontal ? 0f : 90f) : 0f;
        preview.transform.localRotation = Quaternion.Euler(-32, yRot, 32);

        var renderer = preview.AddComponent<SpriteRenderer>();
        string key = building.SpriteKey;
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
            var buildingObj = BuildingFactory.Create(cell.building, cell.level);
            if (buildingObj != null)
            {
                ApplyBuilding(tile, buildingObj);
                MapState.buildings[coord] = buildingObj;
            }

            AddFog(tile, coord);

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

    public void ReloadFromState()
    {
        if (mapParent == null) return;

        foreach (Transform child in mapParent)
        {
            Destroy(child.gameObject);
        }

        tiles.Clear();
        MapState.buildings.Clear();

        var cells = new List<MapCellDTO>(MapState.cellMap.Values);
        GenerateGrid(cells);
        CenterCamera(cells);
        PlayerManager.Instance?.InitializePlayers();
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
    void ApplyBuilding(GameObject tile, Building building)
    {
        if (building == null) return;

        GameObject buildingIcon = new GameObject($"Building_{building.Code}");
        buildingIcon.transform.SetParent(tile.transform);
        buildingIcon.transform.localPosition = new Vector3(-.9f, .8f, -0.25f);
        buildingIcon.transform.localScale = Vector3.one * 0.35f;
        float yRot = building.ShouldRotate ? (building.Orientation == Orientation.Horizontal ? -10f : 90f) : -10f;
        buildingIcon.transform.localRotation = Quaternion.Euler(-60, yRot, 35);
        buildingIcon.tag = building.Code;
        var renderer = buildingIcon.AddComponent<SpriteRenderer>();
        string key = building.SpriteKey;
        renderer.sprite = buildingSprites.ContainsKey(key) ? buildingSprites[key] : defaultBuildingSprite;
        renderer.sortingOrder = 10;

        var health = buildingIcon.AddComponent<StructureHealth>();
        var pos = new Vector2Int(Mathf.RoundToInt(tile.transform.position.x), Mathf.RoundToInt(tile.transform.position.z));
        health.Initialize(pos, building.MaxResistance);

        if (building.Code == BuildingCodes.Atalaya)
        {
            var tower = buildingIcon.AddComponent<TowerAttack>();
            tower.Initialize(pos);
        }

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
    public void DrawBuilding(Vector2Int pos, Building building)
    {
        if (building == null) return;
        if (!tiles.TryGetValue(pos, out var tile)) return;

        GameObject buildingIcon = new GameObject($"Building_{building.Code}");
        buildingIcon.transform.SetParent(tile.transform);
        buildingIcon.transform.localPosition = new Vector3(-0.9f, .8f, -0.25f);
        buildingIcon.transform.localScale = Vector3.one * 0.35f;
        float yRot = building.ShouldRotate ? (building.Orientation == Orientation.Horizontal ? -10f : 90f) : -10f;
        buildingIcon.transform.localRotation = Quaternion.Euler(-60, yRot, 35);

        var renderer = buildingIcon.AddComponent<SpriteRenderer>();
        string key = building.SpriteKey;
        renderer.sprite = buildingSprites.ContainsKey(key) ? buildingSprites[key] : defaultBuildingSprite;
        renderer.sortingOrder = 10;

        var health = buildingIcon.AddComponent<StructureHealth>();
        health.Initialize(pos, building.MaxResistance);
        if (building.Code == BuildingCodes.Atalaya)
        {
            var tower = buildingIcon.AddComponent<TowerAttack>();
            tower.Initialize(pos);
        }
    }

    public void PlaceBuilding(Vector2Int pos, string code, string owner, int level = 1)
    {
        if (!MapState.cellMap.TryGetValue(pos, out var cell))
            return;

        if (!string.IsNullOrEmpty(cell.building))
            return;

        cell.building = code;
        cell.level = level;
        cell.owner = owner;
        GameState.IncrementBuilding(code);

        var buildingObj = BuildingFactory.Create(code, level);
        if (buildingObj != null)
        {
            MapState.buildings[pos] = buildingObj;
            DrawBuilding(pos, buildingObj);
            RevealRadius(pos, buildingObj.VisibilityRadius);
        }

        if (TimelineManager.Instance != null)
        {
            var ev = TimelineManager.Instance.RecordEvent(
                owner,
                "place_building",
                new Dictionary<string, string>
                {
                    {"code", code},
                    {"x", pos.x.ToString()},
                    {"y", pos.y.ToString()}
                }
            );
            TimelineManager.Instance.RegisterObject($"building:{pos.x},{pos.y}", ev);
        }
    }

    public void UpdateBuildingOrientation(Vector2Int pos, Orientation orientation)
    {
        if (!tiles.TryGetValue(pos, out var tile)) return;

        MapState.buildings.TryGetValue(pos, out var building);
        if (building != null)
            building.Orientation = orientation;

        foreach (Transform child in tile.transform)
        {
            if (child.name.StartsWith("Building_"))
            {
                float yRot = (building != null && !building.ShouldRotate) ? 0f :(orientation == Orientation.Horizontal ? 0f : 90f);
                child.localRotation = Quaternion.Euler(-32, yRot, 32);

                if (building != null && !building.ShouldRotate && child.TryGetComponent<SpriteRenderer>(out var rend))
                {
                    string key = building.SpriteKey;
                    if (buildingSprites.ContainsKey(key))
                        rend.sprite = buildingSprites[key];
                }
                break;
            }
        }

        TimelineManager.Instance?.RecordEvent(
            "system",
            "update_orientation",
            new Dictionary<string, string>
            {
                {"x", pos.x.ToString()},
                {"y", pos.y.ToString()},
                {"orientation", orientation.ToString()}
            }
        );
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

    void AddFog(GameObject tile, Vector2Int coord)
    {
        var baseRenderer = tile.GetComponentInChildren<SpriteRenderer>();
        if (baseRenderer == null) return;

        GameObject fogObj = new GameObject("Fog");

        // 🔥 Posicionar en mundo igual que el tile base
        fogObj.transform.position = baseRenderer.transform.position + new Vector3(0, 0.01f, 0);
        fogObj.transform.rotation = baseRenderer.transform.rotation;
        fogObj.transform.localScale = baseRenderer.transform.lossyScale; // 🔥 Usa escala absoluta

        // 🔥 Padre opcional: mapa general en vez del tile para evitar herencia de transform
        fogObj.transform.SetParent(tile.transform.parent);

        var fogRenderer = fogObj.AddComponent<SpriteRenderer>();
        fogRenderer.sprite = baseRenderer.sprite;
        fogRenderer.color = new Color(0, 0, 0, 1f);
        // Ensure fog covers buildings and other objects on the tile
        fogRenderer.sortingOrder = 11;
        fogRenderer.sortingLayerID = baseRenderer.sortingLayerID;

        var fog = tile.AddComponent<FogTile>();
        fog.Initialize(coord, fogRenderer);
    }



    public void RevealTile(Vector2Int pos)
    {
        if (!tiles.TryGetValue(pos, out var tile)) return;
        var fog = tile.GetComponent<FogTile>();
        if (fog != null) fog.Reveal();
    }

    public void RevealRadius(Vector2Int center, int radius)
    {
        for (int dx = -radius; dx <= radius; dx++)
        {
            for (int dy = -radius; dy <= radius; dy++)
            {
                Vector2Int p = new Vector2Int(center.x + dx, center.y + dy);
                RevealTile(p);
            }
        }
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
            var wr = go.AddComponent<WarriorRole>();
            wr.warriorClass = WarriorClass.SoldadoRaso;
            role = wr;

            if (warriorSpriteMap != null && warriorSpriteMap.TryGetValue(wr.warriorClass, out var set))
            {
                animator.northSprites = set.north;
                animator.southSprites = set.south;
                animator.eastSprites = set.east;
                animator.westSprites = set.west;
            }
            else
            {
                animator.northSprites = warriorNorthSprites;
                animator.southSprites = warriorSouthSprites;
                animator.eastSprites = warriorEastSprites;
                animator.westSprites = warriorWestSprites;
            }

            if (weaponSpriteMap != null && weaponSpriteMap.TryGetValue(wr.Stats.weapon, out var ws))
            {
                character.SetWeaponSprite(ws);
            }
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

        if (TimelineManager.Instance != null)
        {
            var ev = TimelineManager.Instance.RecordEvent(
                owner,
                "spawn_character",
                new Dictionary<string, string>
                {
                    {"type", type.ToString()},
                    {"x", pos.x.ToString()},
                    {"y", pos.y.ToString()}
                }
            );
            TimelineManager.Instance.RegisterObject(go.GetInstanceID().ToString(), ev);
        }
    }

    public void DemolishBuilding(Vector2Int pos)
    {
        if (!MapState.cellMap.TryGetValue(pos, out var cell))
            return;

        if (cell.building == BuildingCodes.Townhall)
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

        TimelineManager.Instance?.RecordEvent(
            "system",
            "demolish_building",
            new Dictionary<string, string>
            {
                {"code", oldBuilding},
                {"x", pos.x.ToString()},
                {"y", pos.y.ToString()}
            }
        );
    }

    public void UpgradeBuilding(Vector2Int pos, Building newBuilding)
    {
        if (newBuilding == null) return;
        if (!MapState.cellMap.TryGetValue(pos, out var cell))
            return;

        string old = cell.building;
        cell.building = newBuilding.Code;
        cell.level = newBuilding.Level;
        GameState.DecrementBuilding(old);
        GameState.IncrementBuilding(newBuilding.Code);
        Orientation orient = Orientation.Horizontal;
        if (MapState.buildings.TryGetValue(pos, out var existing))
            orient = existing.Orientation;
        newBuilding.Orientation = orient;
        MapState.buildings[pos] = newBuilding;

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

        DrawBuilding(pos, newBuilding);
        if (MapState.buildings.TryGetValue(pos, out var b) &&
            (PlayerManager.Instance == null || PlayerManager.Instance.IsHumanPlayer(cell.owner)))
            RevealRadius(pos, b.VisibilityRadius);
        Debug.Log($"Edificio en {pos} actualizado de {old} a {newBuilding.Code}");

        TimelineManager.Instance?.RecordEvent(
            cell.owner,
            "upgrade_building",
            new Dictionary<string, string>
            {
                {"from", old},
                {"to", newBuilding.Code},
                {"x", pos.x.ToString()},
                {"y", pos.y.ToString()}
            }
        );
    }


}
