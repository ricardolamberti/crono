using UnityEngine;

public class FogTile : MonoBehaviour
{
    public Vector2Int position;
    private SpriteRenderer overlay;

    public void Initialize(Vector2Int coord, SpriteRenderer renderer)
    {
        position = coord;
        overlay = renderer;
        UpdateVisibility();
    }

    public void Reveal()
    {
        if (overlay != null)
            overlay.enabled = false;
        MapState.exploredCells.Add(position);
    }

    public void UpdateVisibility()
    {
        if (overlay != null)
            overlay.enabled = !MapState.exploredCells.Contains(position);
    }
}
