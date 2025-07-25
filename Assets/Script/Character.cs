using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    public enum Type { Worker, Scientist, Warrior }
    public Type characterType;
    public string owner;

    private Vector3 targetPosition;
    private bool moving = false;
    private float moveSpeed = 2f;

    public SpriteRenderer spriteRenderer;

    public Dictionary<string, Sprite[]> animations = new();
    public string direction = "south";
    private float stepTimer = 0f;
    private int stepIndex = 0;
    private float stepDuration = 0.3f;
    private CharacterAnimator animator;
    public enum Task { None, Move, Build }
    public Task currentTask = Task.None;
    public Vector2Int taskTarget;
    public Vector2Int buildTarget;

    public string buildingToConstruct = "";
    public enum ControlMode { Automatic, Manual }
    public ControlMode controlMode = ControlMode.Automatic;

    public enum GatherTask { Gold, Wood, Food }
    public GatherTask gatherTask = GatherTask.Food;

    private List<Vector2Int> gatherRoute;

    public void LoadSprites(Dictionary<string, Sprite[]> loaded)
    {
        animations = loaded;
  //      SetIdleSprite();
    }
    public void Init(Type type, string ownerId)
    {
        characterType = type;
        owner = ownerId;
        name = $"{type}_{owner}";
        targetPosition = transform.position;

        // Asignar automáticamente el SpriteRenderer si no está asignado
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        animator = GetComponent<CharacterAnimator>();

        SetIdleSprite();
    }

    private Queue<Vector2Int> currentPath;
public void AssignBuildTask(Vector2Int pos, string building)
{
    buildTarget = pos;
    buildingToConstruct = building;

    var distance = Vector2Int.Distance(GetGridPosition(), pos);

    if (distance <= 1.01f)
    {
        // 🔧 Ya está en posición para construir
        currentTask = Task.Build;
        currentPath = null;
        taskTarget = GetGridPosition(); // o el pos adyacente exacto

        MapLoader.instance.ShowConstructionPreview(pos, building);
        Debug.Log($"{name} ya está al lado de {pos}, construyendo {building}");
        return;
    }

    // Si no está al lado, calcular camino
    var path = Pathfinder.FindPath(GetGridPosition(), pos, MapState.cellMap);
    if (path != null && path.Count > 1)
    {
        Vector2Int lastReachable = path[^1];

        if (Vector2Int.Distance(lastReachable, pos) > 1.01f)
        {
            Debug.LogWarning($"{name} no puede alcanzar {pos} para construir {building}");
            return;
        }

        currentTask = Task.Build;
        taskTarget = lastReachable;

        MapLoader.instance.ShowConstructionPreview(pos, building);
        SetPath(path);
        Debug.Log($"{name} va a construir {building} en {pos} desde {lastReachable}");
    }
    else
    {
        Debug.LogWarning($"{name} no tiene camino para construir {building} en {pos}");
    }
}



    public void SetPath(List<Vector2Int> path)
    {
        currentPath = new Queue<Vector2Int>(path);
        MoveToNext();
    }

    public void SetGatherRoute(List<Vector2Int> route)
    {
        if (route == null || route.Count == 0)
        {
            gatherRoute = null;
            currentTask = Task.None;
            return;
        }

        gatherRoute = route;
        SetPath(route);
        currentTask = Task.Move;
    }

    void MoveToNext()
    {
        if (currentPath == null || currentPath.Count == 0)
        {
            currentPath = null; // 🔥 importante para detectar final del camino
            animator.SetWalking(false);
            return;
        }

    
        animator.SetWalking(true);

Vector2Int next = currentPath.Dequeue();
MoveTo(GridUtils.GridToWorld(next));
    }

    private GameObject selector;

    void Start()
    {
        selector = transform.Find("Selector")?.gameObject;
        if (selector != null)
            selector.SetActive(false);
    }

    public void SetSelected(bool value)
    {
        if (selector != null)
            selector.SetActive(value);
    }
    void BuildAt(Vector2Int pos, string building)
    {

        if (MapState.cellMap.TryGetValue(pos, out var cell))
        {
            if (!string.IsNullOrEmpty(cell.building))
            {
                Debug.LogWarning("Ya hay un edificio aquí. Cancelando construcción.");
                return;
            }
            cell.building = building;
            GameState.IncrementBuilding(building);
            MapLoader.instance.DrawBuilding(pos, building); // <- 🔥 Dibuja en el tile
            Debug.Log($"{name} construyó {building} en {pos}");
            MapLoader.instance.ClearConstructionPreview(pos);

            GameEvents.RaiseSelection(gameObject);
        }
    }
     

    public void MoveTo(Vector3 destination)
    {
        targetPosition = destination;
        moving = true;
    }
    bool ShouldProcessUpdate()
    {
        return controlMode == ControlMode.Manual || currentTask != Task.None;
    }

    void Update()
    {
        if (!ShouldProcessUpdate())
            return;

        if (!moving && currentTask == Task.Build && (currentPath == null || currentPath.Count==0))
        {
            BuildAt(buildTarget, buildingToConstruct);
            currentTask = Task.None;
            return;
        }

        if (!moving && currentTask == Task.Move && gatherRoute != null && (currentPath == null || currentPath.Count == 0))
        {
            SetPath(gatherRoute);
        }

        if (moving)
        {
            Vector3 dir = (targetPosition - transform.position).normalized;

            if (animator != null)
            {
                animator.SetDirection(dir);
                animator.SetWalking(true);
            }

            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);


            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                transform.position = targetPosition;
                moving = false;

                if (animator != null)
                    animator.SetWalking(false);
                if (currentTask == Task.Build && (currentPath == null|| currentPath.Count==0))
                {
                    BuildAt(buildTarget, buildingToConstruct);
                    currentTask = Task.None;
                }
                else
                {
                    MoveToNext();
                }
            }
        }
    }
    Vector2 Get2DPosition()
    {
        return new Vector2(transform.position.x, transform.position.z);
    }

    void UpdateWalkingSprite()
    {
        if (animations.ContainsKey(direction))
            spriteRenderer.sprite = animations[direction][stepIndex];
    }

    void SetIdleSprite()
    {
        Debug.Log($"South sprites length: {animator.southSprites?.Length}");
        if (spriteRenderer != null && animations.ContainsKey(direction))
            spriteRenderer.sprite = animations[direction][0];

        if (animator != null)
            animator.SetWalking(false);
    }
    public Vector2Int GetGridPosition()
    {
        return GridUtils.WorldToGrid(transform.position);
    }
    public void SetControlMode(ControlMode mode)
    {
        controlMode = mode;

        if (controlMode == ControlMode.Automatic)
            animator?.SetWalking(false); // detener movimiento si pasa a automático
    }

    public void CancelCurrentTask()
    {
        currentTask = Task.None;
        controlMode = ControlMode.Manual;
        animator?.SetWalking(false);
        // Podés limpiar path y movimiento si querés
        currentPath = null;
        moving = false;
        gatherRoute = null;
    }
}
