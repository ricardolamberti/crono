using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using static Character;
using static DTO;


public class ControlPanel : MonoBehaviour
{
    public UIDocument uiDocument;
    private VisualElement root;
    private VisualElement buttons;
    private Label title;
    private VisualElement info;
    private VisualElement resourceInfo;
    public static ControlPanel Instance { get; private set; }

    private Label goldLabel;
    private Label woodLabel;
    private Label foodLabel;
    private Label cronoLabel;
    private Label dateLabel;
    public bool freeResource = false;

    void OnEnable()
    {
        Instance = this;
        root = uiDocument.rootVisualElement.Q<VisualElement>("root");
        buttons = root.Q<VisualElement>("buttons");
        title = root.Q<Label>("InfoTitle");
        info = root.Q<VisualElement>("info");
        resourceInfo = root.Q<VisualElement>("resources-panel");
        goldLabel = root.Q<Label>("GoldLabel");
        woodLabel = root.Q<Label>("WoodLabel");
        foodLabel = root.Q<Label>("FoodLabel");
        cronoLabel = root.Q<Label>("CronoLabel");
        dateLabel = uiDocument.rootVisualElement.Q<Label>("DateLabel");
        if (resourceInfo != null)
        {
            resourceInfo.Add(new VisualElement { name = "SelectedResourceInfo" });
            resourceInfo = resourceInfo.Q<VisualElement>("SelectedResourceInfo");
        }
        GameTimeManager.OnDateChanged += UpdateDateLabel;
        UpdateDateLabel(GameTimeManager.CurrentMonth, GameTimeManager.CurrentYear);
        RegisterClickBlocker();
        UpdateResourceLabels();
        GameEvents.OnSelectionChanged += UpdatePanel;
    }
    void OnDisable()
    {
        GameEvents.OnSelectionChanged -= UpdatePanel;
        GameTimeManager.OnDateChanged -= UpdateDateLabel;
    }
    void Update()
    {
        UpdateResourceLabels();
    }
    void UpdatePanel(GameObject selected)
    {
        info.Clear();
        resourceInfo?.Clear();
        GamePlayer.Instance.Clear();
        var character = selected.GetComponent<Character>();
        if (character != null)
        {
            title.text = character.name;
            buttons.Clear();
            character.ProvideInfo(GamePlayer.Instance);
            character.ProposeActions(GamePlayer.Instance);
            foreach (var infoItem in GamePlayer.Instance.GetInfo())
            {
                AddInfoLabel(infoItem);
            }
            foreach (var act in GamePlayer.Instance.GetActions())
            {
                AddButton(act.label, act.callback);
            }
        }

        var tileProvider = selected.GetComponent<IActionProposer>();
        if (tileProvider != null && character == null)
        {
            var handler = selected.GetComponent<TileClickHandler>();
            if (handler != null)
            {
                var cell = handler.GetCellData();
                title.text = $"Tile ({cell.x}, {cell.y})";
                handler.ProvideInfo(GamePlayer.Instance);
                foreach (var infoItem in GamePlayer.Instance.GetInfo())
                {
                    AddInfoLabel(infoItem);
                }
            }

            buttons.Clear();
            tileProvider.ProposeActions(GamePlayer.Instance);
            foreach (var act in GamePlayer.Instance.GetActions())
            {
                AddButton(act.label, act.callback);
            }
            return;
        }


    }
    void SpawnWorkerNear(MapCellDTO cell, Character.Type type )
    {
        var req = BuildRules.TakeRequirements(type);
        if (!freeResource)
        {
            if (!GameState.playerResources.HasEnough(req))
            {
                Debug.Log($"No hay recursos suficientes para construir {type}");
                return;
            }
        }


        Vector2Int basePos = new(cell.x, cell.y);
        foreach (var dir in new[] {
        Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right })
        {
            Vector2Int target = basePos + dir;
            if (MapState.cellMap.TryGetValue(target, out var targetCell)
                && string.IsNullOrEmpty(targetCell.building)
                && MapLoader.instance.IsPositionFree(target))
            {
                ActionManager.Instance.Enqueue(new SpawnCharacterAction(target, type, "player1"));
                return;
            }
        }

        Debug.Log("No hay espacio disponible junto al townhall.");
    }
 

    bool TownhallExists()
    {
        return MapState.cellMap.Values.Any(c => c.building == "townhall");
    }
    public void ShowBuildOptionsForTile(GameObject tileObject)
    {
        UpdatePanel(tileObject); // reutiliza la lógica ya existente
    }

    void AddButton(string text, Action onClick)
    {
        var button = new Button(() =>
        {
            var selected = CharacterController.instance.SelectedCharacter;
            if (selected != null)
                selected.CancelCurrentTask();

            onClick();
        })
        { text = text };
        button.pickingMode = PickingMode.Position;

        buttons.Add(button);
    }

    void AddInfoLabel(InfoItem item)
    {
        if (item.type == "resource" && resourceInfo != null)
            resourceInfo.Add(new Label(item.label));
        else
            info.Add(new Label(item.label));
    }

    void RegisterClickBlocker()
    {
        var blocker = new VisualElement();
        blocker.style.position = Position.Absolute;
        blocker.style.top = 0;
        blocker.style.left = 0;
        blocker.style.right = 0;
        blocker.style.bottom = 0;
        blocker.pickingMode = PickingMode.Position;

        blocker.RegisterCallback<PointerDownEvent>(evt => evt.StopPropagation());
        blocker.RegisterCallback<ClickEvent>(evt => evt.StopPropagation());

        root.Insert(0, blocker); // al fondo para no cubrir los controles
    }

    void UpdateResourceLabels()
    {
        if (GameState.playerResources == null) return;
        var res = GameState.playerResources;
        if (goldLabel != null) goldLabel.text = $"Oro: {res.gold}";
        if (woodLabel != null) woodLabel.text = $"Madera: {res.wood}";
        if (foodLabel != null) foodLabel.text = $"Comida: {res.food}";
        if (cronoLabel != null) cronoLabel.text = $"Crono: {res.crono}";
    }

    void UpdateDateLabel(int month, int year)
    {
        if (dateLabel != null)
            dateLabel.text = $"Mes {month} - Año {year}";
    }


    void CreateUnit(string type) { Debug.Log($"Crear unidad: {type}"); }
    void Build(string type) { Debug.Log($"Construir: {type}"); }
    void Harvest(string resource) { Debug.Log($"Recolectar: {resource}"); }
    void Upgrade() { Debug.Log("Mejorar edificio"); }
    void Patrol() { Debug.Log("Patrullando..."); }
    void Attack() { Debug.Log("Atacando..."); }
    void Heal() { Debug.Log("Curando..."); }
}


