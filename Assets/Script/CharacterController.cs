using System.Collections.Generic;
using UnityEngine;
using static Character;

public class CharacterController : MonoBehaviour
{
    public static CharacterController instance;
    private Character selectedCharacter;
    enum ActionMode { None, Move, Attack, Heal }
    ActionMode actionMode = ActionMode.None;
    Character currentActionCharacter = null;
    // En CharacterController.cs
    public Character SelectedCharacter => selectedCharacter;

    void Awake()
    {
        instance = this;
    }
    void OnEnable()
    {
        GameEvents.OnMovementRequested += PrepareMove;
        GameEvents.OnAttackRequested += PrepareAttack;
        GameEvents.OnHealRequested += PrepareHeal;
    }

    void OnDisable()
    {
        GameEvents.OnMovementRequested -= PrepareMove;
        GameEvents.OnAttackRequested -= PrepareAttack;
        GameEvents.OnHealRequested -= PrepareHeal;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (UIUtils.IsPointerOverUI())
                return;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Vector3 hitPos = hit.point;
                Vector2Int gridPos = new(Mathf.RoundToInt(hitPos.x), Mathf.RoundToInt(hitPos.z));

                // Procesar acción especial si está en modo
                if (actionMode == ActionMode.None && currentActionCharacter != null)
                {
                    actionMode = ActionMode.Move;
                }
                if (actionMode == ActionMode.Move && currentActionCharacter != null)
                {
                    var path = Pathfinder.FindPath(currentActionCharacter.GetGridPosition(), gridPos, MapState.cellMap);
                    if (path != null)
                        currentActionCharacter.SetPath(path);
                    ResetMode();
                    return;
                }
                if (actionMode == ActionMode.Attack && currentActionCharacter != null)
                {
                    Debug.Log($"{currentActionCharacter.name} atacaría a {hit.collider.name}");
                    ResetMode();
                    return;
                }
                if (actionMode == ActionMode.Heal && currentActionCharacter != null)
                {
                    Debug.Log($"{currentActionCharacter.name} curaría a {hit.collider.name}");
                    ResetMode();
                    return;
                }

                // Si no es acción especial → selección o movimiento normal
                if (hit.collider.TryGetComponent(out Character c))
                {
                    SelectCharacter(c);
                }
                else
                {
                    HandleTileClick(gridPos);
                }
            }
        }
    }

    void ResetMode()
    {
        actionMode = ActionMode.None;
        currentActionCharacter = null;
    }



    void SelectCharacter(Character character)
    {
        GameEvents.RaiseSelection(character.gameObject);
        character.SetControlMode(Character.ControlMode.Manual);
    }



    public Vector2Int GetGridPosition()
    {
        return new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));
    }

    void PrepareAttack(Character c)
    {
        currentActionCharacter = c;
        actionMode = ActionMode.Attack;
    }

    void PrepareHeal(Character c)
    {
        currentActionCharacter = c;
        actionMode = ActionMode.Heal;
    }
    void PrepareMove(Character c)
    {
        currentActionCharacter = c;
        actionMode = ActionMode.Move;
    }
    void HandleTileClick(Vector2Int gridPos)
    {
        if (selectedCharacter == null) return;

        selectedCharacter.CancelCurrentTask();

        // Mover personaje
        List<Vector2Int> path = Pathfinder.FindPath(selectedCharacter.GetGridPosition(), gridPos, MapState.cellMap);
        if (path != null)
            selectedCharacter.SetPath(path);

        // Mostrar opciones si es trabajador y se puede construir allí
        if (selectedCharacter.characterType == Character.Type.Worker
            && MapState.cellMap.TryGetValue(gridPos, out var cell)
            && string.IsNullOrEmpty(cell.building))
        {
            ControlPanel.Instance.ShowBuildOptionsForTile(selectedCharacter.gameObject);
        }
    }


}
