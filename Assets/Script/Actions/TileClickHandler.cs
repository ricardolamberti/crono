using UnityEngine;
using UnityEngine.UIElements;
using static DTO;

public class TileClickHandler : MonoBehaviour, IInfoProvider
{
    private MapCellDTO cellData;
    public GameObject previewObject;
  
    public void SetData(MapCellDTO cell)
    {
        cellData = cell;
    }

    void OnMouseDown()
    {
        if (UIUtils.IsPointerOverUI())
            return;

        GameEvents.RaiseSelection(gameObject); // reusar el evento ya existente
    }

    public MapCellDTO GetCellData()
    {
        return cellData;
    }

    public void ProvideInfo(GamePlayer player)
    {
        if (cellData == null) return;

        if (cellData.resources != null)
        {
            if (cellData.resources.gold > 0)
                player.AddInfo(new InfoItem($"Oro: {cellData.resources.gold}", "resource"));
            if (cellData.resources.wood > 0)
                player.AddInfo(new InfoItem($"Madera: {cellData.resources.wood}", "resource"));
            if (cellData.resources.crono > 0)
                player.AddInfo(new InfoItem($"Crono: {cellData.resources.crono}", "resource"));
        }

        if (!string.IsNullOrEmpty(cellData.building))
        {
            Vector2Int pos = new(cellData.x, cellData.y);
            if (MapState.buildings.TryGetValue(pos, out var b))
                player.AddInfo(new InfoItem($"Construcción existente: {b.Code} nivel {b.Level}", "detail"));
            else
                player.AddInfo(new InfoItem($"Construcción existente: {cellData.building}", "detail"));
        }
    }
}
