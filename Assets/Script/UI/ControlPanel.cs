using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using static Character;
using static DTO;
using GameConstants;
using System.IO;


public class ControlPanel : MonoBehaviour
{
    public UIDocument uiDocument;
    private VisualElement root;
    private VisualElement buttons;
    private Label title;
    private VisualElement info;
    private VisualElement resourceInfo;
    public static ControlPanel Instance { get; private set; }
    private GameObject currentSelection;

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
        currentSelection = selected;
        info.Clear();
        resourceInfo?.Clear();
        GamePlayer.Instance.Clear();
        var breach = selected.GetComponentInChildren<TimeBreach>();
        if (breach != null)
        {
            title.text = "Brecha Temporal";
            buttons.Clear();
            ShowBreachRoot(breach);
            return;
        }
        var character = selected.GetComponent<Character>();
        if (character != null)
        {
            title.text = character.name;
            buttons.Clear();
            character.ProvideInfo(GamePlayer.Instance);

            if (!string.IsNullOrEmpty(character.owner) && character.owner != "player1")
            {
                GamePlayer.Instance.AddAction(new ControlPanelAction("Atacar", () => Debug.Log("Atacando...")));
            }
            else
            {
                character.ProposeActions(GamePlayer.Instance);
            }

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
 

    bool TownhallExists()
    {
        return MapState.cellMap.Values.Any(c =>
            c.building == BuildingCodes.Townhall && c.owner == "player1");
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

    public void ShowLoadMenu()
    {
        buttons.Clear();
        info.Clear();
        title.text = "Cargar estado";
        foreach (var file in SaveSystem.GetSavedFiles())
        {
            string path = file;
            AddButton(Path.GetFileNameWithoutExtension(file), () =>
            {
                SaveSystem.LoadGame(path);
                UpdatePanel(currentSelection);
            });
        }
        AddButton("Cancelar", () => UpdatePanel(currentSelection));
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

    void ShowBreachRoot(TimeBreach breach)
    {
        buttons.Clear();
        AddButton("Pedir Recurso", () => ShowBreachCategory(breach, "resource"));
        AddButton("Pedir Obrero", () => ShowBreachWorker(breach));
        AddButton("Pedir Soldado", () => ShowBreachCategory(breach, "soldier"));
    }

    void ShowBreachCategory(TimeBreach breach, string type)
    {
        buttons.Clear();
        TimeRequestConfig[] configs = type == "resource" ? breach.resourceConfigs : breach.soldierConfigs;
        if (configs == null) return;
        foreach (var cfg in configs)
        {
            string label = cfg.id;
            AddButton(label, () => ShowBreachSlider(breach, cfg));
        }
        AddButton("Volver", () => ShowBreachRoot(breach));
    }

    void ShowBreachWorker(TimeBreach breach)
    {
        ShowBreachSlider(breach, breach.workerConfig);
    }

    void ShowBreachSlider(TimeBreach breach, TimeRequestConfig config)
    {
        buttons.Clear();
        var slider = new SliderInt(config.minFutureYears, config.maxFutureYears);
        slider.value = config.minFutureYears;
        slider.style.flexGrow = 1;

        var label = new Label();
        void UpdateText(int val)
        {
            int cost = breach.CalculateCost(config, val);
            label.text = $"Pedido en {val} años - Costo: {cost} crono";
        }
        UpdateText(slider.value);
        slider.RegisterValueChangedCallback(evt => UpdateText(evt.newValue));

        var confirm = new Button(() =>
        {
            breach.MakeRequest(config, slider.value);
            ShowBreachRoot(breach);
        }) { text = "Confirmar" };

        buttons.Add(slider);
        buttons.Add(label);
        buttons.Add(confirm);
        AddButton("Volver", () => ShowBreachRoot(breach));
    }
}


