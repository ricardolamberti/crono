using UnityEngine;

[CreateAssetMenu(menuName="TimeRequestConfig")]
public class TimeRequestConfig : ScriptableObject
{
    public string id; // "worker", "soldier_raso", "gold" etc.
    public int minFutureYears = 0;
    public int maxFutureYears = 5;
    public int baseCost = 1;
    public float costFactor = 1f;
}
