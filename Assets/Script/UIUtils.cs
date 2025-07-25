using UnityEngine.EventSystems;

public static class UIUtils
{
    public static bool IsPointerOverUI()
    {
        if (EventSystem.current == null)
            return false;
        return EventSystem.current.IsPointerOverGameObject();
    }
}
