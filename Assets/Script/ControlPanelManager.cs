using UnityEngine;
using UnityEngine.UIElements;

public class ControlPanelManager : MonoBehaviour
{
    private VisualElement root;

    void Awake()
    {
        var uiDocument = GetComponent<UIDocument>();
        root = uiDocument.rootVisualElement;

        // Ejemplo: ocultar el panel inicialmente
        root.style.display = DisplayStyle.None;
    }

    public void ShowPanelFor(GameObject selected)
    {
        root.style.display = DisplayStyle.Flex;

        // Aquí podés usar: selected.GetComponent<Character>(), etc.
    }
}
