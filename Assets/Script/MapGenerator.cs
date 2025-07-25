using System.Collections.Generic;
using UnityEngine;
using System.IO;
using static DTO;

public class MapGenerator : MonoBehaviour
{
   

    void Start()
    {
       // GenerateAndSaveMap();

    }
    public int width = 40;
    public int height = 40;
    public int numRivers = 1;
    public int numMountainClusters = 3;
    public int mountainClusterSize = 10;

    private string[] buildings = { "", "", "", "hut", "barracks", "farm" };
    private string[] owners = { "", "", "player1", "player2", "player3" };

    private HashSet<Vector2Int> waterTiles = new();
    private HashSet<Vector2Int> mountainTiles = new();

    public void GenerateAndSaveMap()
    {
        List<MapCellDTO> map = new();

        GenerateRivers();
        GenerateMountainClusters();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2Int pos = new(x, y);
                string terrain = "forest";

                if (waterTiles.Contains(pos)) terrain = "water";
                else if (mountainTiles.Contains(pos)) terrain = "mountain";

                
                var res = new ResourceBundle();

                if (terrain == "mountain")
                {
                    res.gold = Random.Range(10, 30);
                }
                else if (terrain == "forest")
                {
                    res.wood = Random.Range(10, 50);
                }

                // chance baja de crono en cualquier celda
                if (Random.value < 0.01f)
                {
                    res.crono = 1;
                }

                map.Add(new MapCellDTO
                {
                    x = x,
                    y = y,
                    terrain = terrain,
                    building = "",//buildings[Random.Range(0, buildings.Length)],
                    owner = "",//owners[Random.Range(0, owners.Length)],
                    resources = res
                });

            }
        }

        MapCellListWrapper wrapper = new() { cells = map };
        string json = JsonUtility.ToJson(wrapper, true);
        string path = Application.dataPath + "/Resources/map_data.json";
        File.WriteAllText(path, json);
        Debug.Log("Mapa generado y guardado en: " + path);
    }

    void GenerateRivers()
    {
        for (int i = 0; i < numRivers; i++)
        {
            int startX = Random.Range(0, width);
            int x = startX;
            for (int y = 0; y < height; y++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    int nx = Mathf.Clamp(x + dx, 0, width - 1);
                    waterTiles.Add(new Vector2Int(nx, y));
                }

                // serpenteo
                x += Random.Range(-1, 2);
                x = Mathf.Clamp(x, 1, width - 2);
            }
        }
    }

    void GenerateMountainClusters()
    {
        for (int i = 0; i < numMountainClusters; i++)
        {
            int cx = Random.Range(0, width);
            int cy = Random.Range(0, height);

            for (int j = 0; j < mountainClusterSize; j++)
            {
                int nx = cx + Random.Range(-2, 3);
                int ny = cy + Random.Range(-2, 3);
                Vector2Int pos = new(nx, ny);
                if (nx >= 0 && ny >= 0 && nx < width && ny < height)
                    if (!waterTiles.Contains(pos))
                        mountainTiles.Add(pos);
            }
        }
    }
}
