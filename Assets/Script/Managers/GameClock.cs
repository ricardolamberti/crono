public static class GameClock
{
    public static float Time { get; private set; }

    public static void Advance(float deltaTime)
    {
        Time += deltaTime;
    }

    public static void Set(float value)
    {
        Time = value;
    }
}
