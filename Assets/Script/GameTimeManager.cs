using UnityEngine;

public class GameTimeManager : MonoBehaviour
{
    public float interval = 5f; // cada 5 segundos
    public int cyclesPerMonth = 30;

    private float timer = 0f;
    private int currentCycle = 0;

    public static int CurrentMonth { get; private set; } = 1;
    public static int CurrentYear { get; private set; } = 1;

    public static event System.Action<int, int> OnDateChanged;

    void Start()
    {
        OnDateChanged?.Invoke(CurrentMonth, CurrentYear);
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= interval)
        {
            ApplyResourceFlow();
            AdvanceCycle();
            timer = 0f;
        }
    }

    void ApplyResourceFlow()
    {
        var total = new ResourceFlow();

        foreach (var cell in MapState.cellMap.Values)
        {
            if (!string.IsNullOrEmpty(cell.building))
            {
                total += ResourceProductionMatrix.GetFlow(cell.building);
            }
        }

        foreach (var character in GameObject.FindObjectsOfType<Character>())
        {
            string unitCode = character.characterType.ToString().ToLower();
            total += ResourceProductionMatrix.GetFlow(unitCode);
        }

        GameState.playerResources.AddFlow(total);
    }

    void AdvanceCycle()
    {
        currentCycle++;
        if (currentCycle >= cyclesPerMonth)
        {
            currentCycle = 0;
            CurrentMonth++;
            if (CurrentMonth > 12)
            {
                CurrentMonth = 1;
                CurrentYear++;
            }

            OnDateChanged?.Invoke(CurrentMonth, CurrentYear);
        }
    }
}
