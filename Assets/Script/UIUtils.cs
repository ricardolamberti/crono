using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public static class UIUtils
{
    /// <summary>
    /// Returns true when the pointer is currently over any UI Toolkit element.
    /// Works with both UI Toolkit and the legacy EventSystem.
    /// </summary>
    public static bool IsPointerOverUI()
    {
        // Legacy EventSystem check (e.g. for UGUI)
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return true;

        // Fallback for UI Toolkit panels
#if ENABLE_INPUT_SYSTEM
        Vector2 pos = UnityEngine.InputSystem.Mouse.current?.position.ReadValue() ?? Vector2.zero;
#else
        Vector2 pos = Input.mousePosition;
#endif

        foreach (var doc in Object.FindObjectsOfType<UIDocument>())
        {
            var panel = doc.rootVisualElement.panel;
            if (panel == null)
                continue;

            var picked = panel.Pick(pos);
            if (picked != null && picked != doc.rootVisualElement)
                return true;
        }

        return false;
    }
}
