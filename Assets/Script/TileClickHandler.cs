using UnityEngine;
using UnityEngine.UIElements;
using static DTO;

public class TileClickHandler : MonoBehaviour
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
}
