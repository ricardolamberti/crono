using System.Collections.Generic;
using UnityEngine;

public class DTO
{
    [System.Serializable]
    public class MapCellDTO
    {
        public int x;
        public int y;
        public string terrain;
        public string building;
        public string owner;
        public ResourceBundle resources;

    }
    [System.Serializable]
    public class ResourceBundle
    {
        public int gold = 0;
        public int wood = 0;
        public int crono = 0;
    }

    [System.Serializable]
    public class MapCellListWrapper
    {
        public List<MapCellDTO> cells;
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


}
