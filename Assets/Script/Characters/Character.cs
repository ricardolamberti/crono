using System.Collections.Generic;
using UnityEngine;
using static DTO;
using GameConstants;

// Orientation is used for directional buildings like bridges or walls

public class Character : MonoBehaviour, IActionProposer, IInfoProvider
{
    public enum Type { Worker, Scientist, Warrior }
    public Type characterType;
    public CharacterRole role;
    public string owner;

    private Vector3 targetPosition;
    private bool moving = false;
    private float moveSpeed = 2f;

    public SpriteRenderer spriteRenderer;
    public SpriteRenderer weaponRenderer;

    public Dictionary<string, Sprite[]> animations = new();
    public string direction = "south";
    private float stepTimer = 0f;
    private int stepIndex = 0;
    private float stepDuration = 0.3f;
    private CharacterAnimator animator;
    public enum Task { None, Move, Build, Attack }
    public Task currentTask = Task.None;
    public Vector2Int taskTarget;
    public Vector2Int buildTarget;

    public int level = 1;

    public string buildingToConstruct = "";
    public enum ControlMode { Automatic, Manual }
    public ControlMode controlMode = ControlMode.Automatic;

    public enum GatherTask { Gold, Wood, Food }
    public GatherTask gatherTask = GatherTask.Food;

    private List<Vector2Int> gatherRoute;

    private GameObject attackTarget;
    private float attackTimer = 0f;
    private float attackInterval = 1f;

    public void LoadSprites(Dictionary<string, Sprite[]> loaded)
    {
        animations = loaded;
  //      SetIdleSprite();
    }
    public void Init(Type type, string ownerId)
    {
        characterType = type;
        role = GetComponent<CharacterRole>();
        owner = ownerId;
        name = $"{type}_{owner}";
        targetPosition = transform.position;

        // Asignar automáticamente el SpriteRenderer si no está asignado
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (weaponRenderer == null)
        {
            var weaponObj = new GameObject("Weapon");
            weaponObj.transform.SetParent(transform);
            weaponObj.transform.localPosition = new Vector3(0f, 0.6f, 0f);
            weaponRenderer = weaponObj.AddComponent<SpriteRenderer>();
            weaponRenderer.sortingOrder = 2;
        }

        animator = GetComponent<CharacterAnimator>();

        SetIdleSprite();
    }

    public void Init(CharacterRole roleComponent, string ownerId)
    {
        role = roleComponent;
        owner = ownerId;
        name = $"{roleComponent.Code}_{owner}";
        targetPosition = transform.position;

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (weaponRenderer == null)
        {
            var weaponObj = new GameObject("Weapon");
            weaponObj.transform.SetParent(transform);
            weaponObj.transform.localPosition = new Vector3(0f, 0.6f, 0f);
            weaponRenderer = weaponObj.AddComponent<SpriteRenderer>();
            weaponRenderer.sortingOrder = 2;
        }

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

        Orientation orient = DetermineOrientation(pos, building);
        var previewBuilding = BuildingFactory.Create(building);
        if (previewBuilding != null)
        {
            previewBuilding.Orientation = orient;
            MapLoader.instance.ShowConstructionPreview(pos, previewBuilding);
        }
        Debug.Log($"{name} ya está al lado de {pos}, construyendo {building}");
        return;
    }

    // Si no está al lado, calcular camino
    var path = Pathfinder.FindPath(GetGridPosition(), pos, MapState.cellMap, owner);
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

        Orientation orient = DetermineOrientation(pos, building);
        var previewBuilding2 = BuildingFactory.Create(building);
        if (previewBuilding2 != null)
        {
            previewBuilding2.Orientation = orient;
            MapLoader.instance.ShowConstructionPreview(pos, previewBuilding2);
        }
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

    public void StartAttack(GameObject target)
    {
        if (target == null) return;
        CancelCurrentTask();
        attackTarget = target;
        currentTask = Task.Attack;
        attackTimer = 0f;
        MoveOrChaseTarget();
    }

    void MoveOrChaseTarget()
    {
        if (attackTarget == null) return;
        Vector2Int targetPos = GridUtils.WorldToGrid(attackTarget.transform.position);
        int range = GetAttackRange();
        if (Vector2Int.Distance(GetGridPosition(), targetPos) > range)
        {
            var path = Pathfinder.FindPath(GetGridPosition(), targetPos, MapState.cellMap, owner);
            if (path != null && path.Count > 0)
            {
                path.RemoveAt(path.Count - 1); // no entrar en la casilla del objetivo
                SetPath(path);
            }
        }
    }

    int GetAttackRange()
    {
        if (role is WarriorRole wr && wr.Stats.weapon != WeaponType.None)
            return 3;
        return 1;
    }

    int GetAttackDamage()
    {
        if (role is WarriorRole wr)
        {
            return wr.Stats.weapon != WeaponType.None ? wr.Stats.longRangeDamage : wr.Stats.shortRangeDamage;
        }
        return 1;
    }

    IEnumerator ShootProjectile(Vector3 targetPos, WeaponType weapon)
    {
        if (MapLoader.instance == null) yield break;
        Sprite s = MapLoader.instance.GetWeaponSprite(weapon);
        if (s == null) yield break;
        GameObject proj = new GameObject("Projectile");
        var sr = proj.AddComponent<SpriteRenderer>();
        sr.sprite = s;
        sr.sortingOrder = 12;
        proj.transform.position = transform.position + new Vector3(0f, 0.6f, 0f);
        Vector3 start = proj.transform.position;
        Vector3 end = targetPos + new Vector3(0f, 0.6f, 0f);
        float t = 0f;
        while (t < 1f)
        {
            proj.transform.position = Vector3.Lerp(start, end, t);
            t += Time.deltaTime * 5f;
            yield return null;
        }
        Destroy(proj);
    }

    IEnumerator PerformAttack()
    {
        if (attackTarget == null) yield break;
        Vector3 targetPos = attackTarget.transform.position;
        WeaponType wtype = WeaponType.None;
        if (role is WarriorRole wr)
            wtype = wr.Stats.weapon;
        if (wtype != WeaponType.None)
            yield return ShootProjectile(targetPos, wtype);

        if (attackTarget.TryGetComponent(out HealthComponent hc))
            hc.TakeDamage(GetAttackDamage());
        else if (attackTarget.TryGetComponent(out StructureHealth sh))
            sh.TakeDamage(GetAttackDamage());
    }

    void MoveToNext()
    {
        if (currentPath == null || currentPath.Count == 0)
        {
            currentPath = null; // 🔥 importante para detectar final del camino
            animator.SetWalking(false);
            return;
        }

        Vector2Int next = currentPath.Peek();
        if (!MapState.cellMap.TryGetValue(next, out var cell) ||
            !Pathfinder.IsWalkable(cell.terrain, cell.building, cell.owner, owner))
        {
            AbortGatherRoute();
            PlanGatherRoute();
            return;
        }

        currentPath.Dequeue();

        animator.SetWalking(true);

        MoveTo(GridUtils.GridToWorld(next));
    }

    public void PlanGatherRoute()
    {
        Vector2Int start = this.GetGridPosition();
        Vector2Int townhall = FindNearest(start, c => c.building == BuildingCodes.Townhall && c.owner == owner);
        if (townhall.x < 0) return;

        List<Vector2Int> route = new();
        List<Vector2Int> segment;
        switch (this.gatherTask)
        {
            case Character.GatherTask.Gold:
                Vector2Int mine = FindNearest(start, c => c.building == BuildingCodes.Mine && c.owner == owner);
                if (mine.x < 0) return;
                segment = Pathfinder.FindPath(start, mine, MapState.cellMap, owner);
                route.AddRange(segment);
                segment = Pathfinder.FindPath(mine, townhall, MapState.cellMap, owner);
                for (int i = 1; i < segment.Count; i++) route.Add(segment[i]);
                segment = Pathfinder.FindPath(townhall, mine, MapState.cellMap, owner);
                for (int i = 1; i < segment.Count; i++) route.Add(segment[i]);
                break;
            case Character.GatherTask.Wood:
                Vector2Int lumber = FindNearest(start, c => c.building == BuildingCodes.Lumbermill && c.owner == owner);
                if (lumber.x < 0) return;
                Vector2Int tree = FindNearest(lumber, c => c.resources != null && c.resources.wood > 0);
                if (tree.x < 0) return;
                segment = Pathfinder.FindPath(start, lumber, MapState.cellMap, owner);
                route.AddRange(segment);
                segment = Pathfinder.FindPath(lumber, tree, MapState.cellMap, owner);
                for (int i = 1; i < segment.Count; i++) route.Add(segment[i]);
                segment = Pathfinder.FindPath(tree, lumber, MapState.cellMap, owner);
                for (int i = 1; i < segment.Count; i++) route.Add(segment[i]);
                segment = Pathfinder.FindPath(lumber, townhall, MapState.cellMap, owner);
                for (int i = 1; i < segment.Count; i++) route.Add(segment[i]);
                segment = Pathfinder.FindPath(townhall, lumber, MapState.cellMap, owner);
                for (int i = 1; i < segment.Count; i++) route.Add(segment[i]);
                break;
            case Character.GatherTask.Food:
                Vector2Int farm = FindNearest(start, c => c.building == BuildingCodes.Farm && c.owner == owner);
                if (farm.x < 0) return;
                segment = Pathfinder.FindPath(start, farm, MapState.cellMap, owner);
                route.AddRange(segment);
                segment = Pathfinder.FindPath(farm, townhall, MapState.cellMap, owner);
                for (int i = 1; i < segment.Count; i++) route.Add(segment[i]);
                segment = Pathfinder.FindPath(townhall, farm, MapState.cellMap, owner);
                for (int i = 1; i < segment.Count; i++) route.Add(segment[i]);
                break;
        }

        if (route.Count > 0)
            this.SetGatherRoute(route);
    }

    Vector2Int FindNearest(Vector2Int origin, System.Func<MapCellDTO, bool> filter)
    {
        int best = int.MaxValue;
        Vector2Int bestPos = new(-1, -1);
        foreach (var kvp in MapState.cellMap)
        {
            if (!filter(kvp.Value))
                continue;
            int dist = Mathf.Abs(kvp.Key.x - origin.x) + Mathf.Abs(kvp.Key.y - origin.y);
            if (dist < best)
            {
                best = dist;
                bestPos = kvp.Key;
            }
        }
        return bestPos;
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

    Orientation DetermineOrientation(Vector2Int pos, string building)
    {
        bool horiz = false;
        bool vert = false;
        Vector2Int[] horizDirs = { Vector2Int.left, Vector2Int.right };
        foreach (var d in horizDirs)
        {
            Vector2Int n = pos + d;
            if (MapState.cellMap.TryGetValue(n, out var cell) && cell.building == building)
                horiz = true;
        }
        Vector2Int[] vertDirs = { Vector2Int.up, Vector2Int.down };
        foreach (var d in vertDirs)
        {
            Vector2Int n = pos + d;
            if (MapState.cellMap.TryGetValue(n, out var cell) && cell.building == building)
                vert = true;
        }
        if (vert && !horiz) return Orientation.Vertical;
        return Orientation.Horizontal;
    }

    void AdjustNeighbours(Vector2Int pos, string building, Orientation orientation)
    {
        Vector2Int[] dirs = orientation == Orientation.Horizontal ?
            new[] { Vector2Int.left, Vector2Int.right } :
            new[] { Vector2Int.up, Vector2Int.down };
        foreach (var d in dirs)
        {
            Vector2Int n = pos + d;
            if (MapState.buildings.TryGetValue(n, out var b) && b.Code == building)
            {
                b.Orientation = orientation;
                MapLoader.instance.UpdateBuildingOrientation(n, orientation);
            }
        }
    }
    void BuildAt(Vector2Int pos, string building, int level = 1)
    {

        if (MapState.cellMap.TryGetValue(pos, out var cell))
        {
            if (!string.IsNullOrEmpty(cell.building))
            {
                Debug.LogWarning("Ya hay un edificio aquí. Cancelando construcción.");
                return;
            }
            cell.building = building;
            cell.level = level;
            cell.owner = owner;

            if (cell.resources != null && cell.resources.wood > 0)
            {
                GameState.playerResources.wood += cell.resources.wood;
                cell.resources.wood = 0;
            }
            GameState.IncrementBuilding(building);
            Orientation orient = DetermineOrientation(pos, building);
            var buildingObj = BuildingFactory.Create(building, level);
            if (buildingObj != null)
            {
                buildingObj.Orientation = orient;
                MapState.buildings[pos] = buildingObj;
                MapLoader.instance.DrawBuilding(pos, buildingObj); // <- 🔥 Dibuja en el tile
            }
            AdjustNeighbours(pos, building, orient);
            if (buildingObj != null &&
                (PlayerManager.Instance == null || PlayerManager.Instance.IsHumanPlayer(owner)))
                MapLoader.instance.RevealRadius(pos, buildingObj.VisibilityRadius);
            Debug.Log($"{name} construyó {building} en {pos}");
            MapLoader.instance.ClearConstructionPreview(pos);

            if (building == BuildingCodes.Townhall)
            {
                foreach (var c in GameObject.FindObjectsOfType<Character>())
                {
                    if (c.role is WorkerRole && c.owner == owner)
                        c.PlanGatherRoute();
                }
            }

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
        return controlMode == ControlMode.Manual || currentTask != Task.None || controlMode == ControlMode.Automatic;
    }

    void Update()
    {
        if (!ShouldProcessUpdate())
            return;

        if (!moving && currentTask == Task.Build && (currentPath == null || currentPath.Count==0))
        {
            BuildAt(buildTarget, buildingToConstruct, 1);
            currentTask = Task.None;
            return;
        }

        if (!moving && currentTask == Task.Move && gatherRoute != null && (currentPath == null || currentPath.Count == 0))
        {
            SetPath(gatherRoute);
        }

        if (!moving && currentTask == Task.Attack)
        {
            MoveOrChaseTarget();
            if (attackTarget != null)
            {
                Vector2Int tpos = GridUtils.WorldToGrid(attackTarget.transform.position);
                if (Vector2Int.Distance(GetGridPosition(), tpos) <= GetAttackRange())
                {
                    attackTimer += Time.deltaTime;
                    if (attackTimer >= attackInterval)
                    {
                        attackTimer = 0f;
                        StartCoroutine(PerformAttack());
                    }
                }
            }
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

                if (PlayerManager.Instance == null ||
                    PlayerManager.Instance.IsHumanPlayer(owner))
                {
                    MapLoader.instance?.RevealRadius(GetGridPosition(), 1);
                }

                if (animator != null)
                    animator.SetWalking(false);
                if (currentTask == Task.Build && (currentPath == null|| currentPath.Count==0))
                {
                    BuildAt(buildTarget, buildingToConstruct, 1);
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

        if (weaponRenderer != null)
            weaponRenderer.sprite = null;

        if (animator != null)
            animator.SetWalking(false);
    }
    public Vector2Int GetGridPosition()
    {
        return GridUtils.WorldToGrid(transform.position);
    }

    public void SetWeaponSprite(Sprite sprite)
    {
        if (weaponRenderer != null)
            weaponRenderer.sprite = sprite;
    }

    public void SetLevel(int value)
    {
        level = Mathf.Max(1, value);
    }

    public void LevelUp()
    {
        level += 1;
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
        attackTarget = null;
    }

    void AbortGatherRoute()
    {
        currentTask = Task.None;
        currentPath = null;
        gatherRoute = null;
        moving = false;
        animator?.SetWalking(false);
    }

    public void ProposeActions(GamePlayer player)
    {
        role?.ProposeActions(this, player);
    }

    public void ProvideInfo(GamePlayer player)
    {
        string typeName = role != null ? role.Code : characterType.ToString();
        player.AddInfo(new InfoItem($"Tipo: {typeName}", "detail"));
        player.AddInfo(new InfoItem($"Control: {controlMode}", "detail"));
        player.AddInfo(new InfoItem($"Dueño: {owner}", "detail"));
        player.AddInfo(new InfoItem($"Nivel: {level}", "detail"));

        if (TryGetComponent(out HealthComponent health))
        {
            player.AddInfo(new InfoItem($"Salud: {health.currentHealth}/{health.maxHealth}", "status"));
        }
    }
}
