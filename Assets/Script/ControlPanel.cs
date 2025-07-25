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
    public static ControlPanel Instance { get; private set; }


    void OnEnable()
    {
        Instance = this;
        root = uiDocument.rootVisualElement.Q<VisualElement>("root");
        buttons = root.Q<VisualElement>("buttons");
        title = root.Q<Label>("InfoTitle");
        info = root.Q<VisualElement>("info");

        RegisterClickBlocker();

        GameEvents.OnSelectionChanged += UpdatePanel;
    }
    void OnDisable()
    {
        GameEvents.OnSelectionChanged -= UpdatePanel;
    }

    void UpdatePanel(GameObject selected)
    {
        info.Clear();
        var character = selected.GetComponent<Character>();
        if (character != null)
        {
            title.text = character.name;
            buttons.Clear();

         
            // Acción: Atacar (solo si es Guerrero)
            if (character.characterType == Character.Type.Warrior)
                AddButton("Atacar", () => GameEvents.RequestAttack(character));

            // Acción: Curar (solo si es Científico)
            if (character.characterType == Character.Type.Scientist)
                AddButton("Curar", () => GameEvents.RequestHeal(character));
           
            info.Add(new Label($"Tipo: {character.characterType}"));
            info.Add(new Label($"Control: {character.controlMode}"));
            info.Add(new Label($"Dueño: {character.owner}"));

            if (character.TryGetComponent(out HealthComponent health))
            {
                info.Add(new Label($"Salud: {health.currentHealth}/{health.maxHealth}"));
            }

        }



        Character FindFreeWorker()
        {
            var all = GameObject.FindObjectsOfType<Character>();
            foreach (var c in all)
            {
                if (c.characterType == Character.Type.Worker &&
                    c.controlMode == Character.ControlMode.Automatic &&
                    c.currentTask == Character.Task.None)
                {
                    return c;
                }
            }
            foreach (var c in all)
            {
                if (c.characterType == Character.Type.Worker &&
                    c.controlMode == Character.ControlMode.Manual &&
                    c.currentTask == Character.Task.None)
                {
                    return c;
                }
            }

            return null;
        }

      


        var tileHandler = selected.GetComponent<TileClickHandler>();
        if (tileHandler != null)
        {
            var cell = tileHandler.GetCellData();
            title.text = $"Tile ({cell.x}, {cell.y})";
            buttons.Clear();
            info.Clear();

            if (cell.resources != null)
            {
                info.Add(new Label($"Oro: {cell.resources.gold}"));
                info.Add(new Label($"Madera: {cell.resources.wood}"));
                info.Add(new Label($"Crono: {cell.resources.crono}"));
            }
              if (!string.IsNullOrEmpty(cell.building))
            {
                info.Add(new Label($"Construcción existente: {cell.building}"));

                if (cell.building == "townhall")
                {
                    AddButton("Crear obrero", () => {
                        SpawnWorkerNear(tileHandler.GetCellData(), Character.Type.Worker);
                    });
                }
                if (cell.building == "academy")
                {
                    AddButton("Crear cientifico", () => {
                        SpawnWorkerNear(tileHandler.GetCellData(), Character.Type.Scientist);
                    });
                }
                if (cell.building == "barracks")
                {
                    AddButton("Crear soldado", () => {
                        SpawnWorkerNear(tileHandler.GetCellData(), Character.Type.Warrior);
                    });
                }

                if (cell.building != "townhall")
                {
                    AddButton("Derrumbar", () =>
                    {
                        MapLoader.instance.DemolishBuilding(new Vector2Int(cell.x, cell.y));
                        GameEvents.RaiseSelection(selected); // 🔄 Refresca el panel tras destruir
                    });
                }
                return;
            }

            Vector2Int pos = new(cell.x, cell.y);

            // 🔧 Función auxiliar para simplificar
            void IntentarConstruir(string nombre, string buildingCode)
            {
                AddButton(nombre, () => {
                    var req = BuildRules.TakeRequirements(buildingCode);

                    if (!GameState.playerResources.HasEnough(req))
                    {
                        Debug.Log($"No hay recursos suficientes para construir {buildingCode}");
                        return;
                    }

                    var worker = FindFreeWorker();
                    if (worker != null)
                    {
                        GameState.playerResources.Consume(req);
                        worker.AssignBuildTask(pos, buildingCode);
                    }

                });
            }
  

            // 🌄 Construcciones según tipo de terreno
            if (cell.terrain == "mountain")
            {
                IntentarConstruir("Construir mina", "mine");
            }
            else if (cell.terrain == "water")
            {
                IntentarConstruir("Construir puerto", "dock");
            }
            else if (cell.terrain == "forest")
            {
                if (!TownhallExists())
                {
                    AddButton("Construir casa central", () => {
                        var worker = FindFreeWorker();
                        if (worker != null)
                            worker.AssignBuildTask(new Vector2Int(cell.x, cell.y), "townhall");
                        else
                            Debug.Log("No hay obreros disponibles.");
                    });
                } else
                {
                    IntentarConstruir("Construir choza", "hut");
                    IntentarConstruir("Construir aserradero", "lumbermill");
                    IntentarConstruir("Construir granja", "farm");
                    IntentarConstruir("Construir academia", "academy");
                    IntentarConstruir("Construir barraca", "barracks");

                }

            }
           

            // 🔵 Crono en cualquier terreno
            if (cell.resources?.crono > 0)
            {
                IntentarConstruir("Construir extractor de crono", "extractor");
            }
            if (GameState.playerResources != null)
            {
                var res = GameState.playerResources;
                info.Add(new Label($"Recursos:"));
                info.Add(new Label($"Oro: {res.gold}"));
                info.Add(new Label($"Madera: {res.wood}"));
                info.Add(new Label($"Comida: {res.food}"));
                info.Add(new Label($"Crono: {res.crono}"));
                info.Add(new Label($"Habitaciones: {res.freeHousing}"));
                info.Add(new Label($"Ciencia: {res.academicUnits}"));
            }

            return;
        }


    }
    void SpawnWorkerNear(MapCellDTO cell, Character.Type type )
    {
        var req = BuildRules.TakeRequirements(type);

        if (!GameState.playerResources.HasEnough(req))
        {
            Debug.Log($"No hay recursos suficientes para construir { type}");
            return;
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
                MapLoader.instance.SpawnCharacter(target, type, "player1" );
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

    void CreateUnit(string type) { Debug.Log($"Crear unidad: {type}"); }
    void Build(string type) { Debug.Log($"Construir: {type}"); }
    void Harvest(string resource) { Debug.Log($"Recolectar: {resource}"); }
    void Upgrade() { Debug.Log("Mejorar edificio"); }
    void Patrol() { Debug.Log("Patrullando..."); }
    void Attack() { Debug.Log("Atacando..."); }
    void Heal() { Debug.Log("Curando..."); }
}


