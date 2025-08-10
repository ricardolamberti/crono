using UnityEngine;
using UnityEngine.UIElements;

public class TimelineControl : MonoBehaviour
{
    public UIDocument uiDocument;
    private SliderInt slider;
    private Button presentButton;
    private Button timeTravelButton;
    private VisualElement tickContainer;
    private bool inPast = false;
    private int backupMonth;
    private int backupYear;
    private float selectedTime;

    void OnEnable()
    {
        if (uiDocument == null) return;
        var root = uiDocument.rootVisualElement;
        slider = root.Q<SliderInt>("TimelineSlider");
        presentButton = root.Q<Button>("PresentButton");
        timeTravelButton = root.Q<Button>("TimeTravelButton");

        if (slider != null)
        {
            slider.lowValue = 0;
            slider.highValue = Mathf.RoundToInt(GameClock.Time);
            slider.showInputField = false;
            slider.SetValueWithoutNotify(Mathf.RoundToInt(GameClock.Time));

            selectedTime = GameClock.Time;

            GameTimeManager.UpdateDateFromSeconds(GameClock.Time);
            slider.label = FormatTimeLabel(GameClock.Time);

            slider.RegisterValueChangedCallback(OnSliderChanged);

            // ✅ Espera hasta que el layout tenga tamaño real
            slider.schedule.Execute(() =>
            {
                UpdateSliderHandle();
                DrawMonthTicks();
            }).StartingIn(100);
        }

        if (presentButton != null)
        {
            presentButton.clicked += ReturnToPresent;
            presentButton.SetEnabled(false);
        }
        if (timeTravelButton != null)
            timeTravelButton.clicked += BeginTimeTravel;
    }


    void Update()
    {
        if (slider == null) return;

        slider.highValue = Mathf.RoundToInt(GameClock.Time);

        // 🔥 Solo auto-actualiza si NO estamos en el pasado
        if (!inPast)
        {
            int now = Mathf.RoundToInt(GameClock.Time);
            slider.SetValueWithoutNotify(now);
            GameTimeManager.UpdateDateFromSeconds(now);
            slider.label = FormatTimeLabel(now);
            selectedTime = now;
        }

        UpdateSliderHandle();
        DrawMonthTicks();
    }

    void OnSliderChanged(ChangeEvent<int> evt)
    {
        Debug.Log($"[Timeline] Cambió slider: {evt.newValue}");
        float t = evt.newValue;
        selectedTime = t;
        bool past = t < GameClock.Time - 0.1f; // margen pequeño

        if (past)
        {
            if (!inPast)
            {
                TimelineManager.Instance?.SaveSnapshot(true);
                backupMonth = GameTimeManager.CurrentMonth;
                backupYear = GameTimeManager.CurrentYear;
            }

      
            TimelineManager.Instance?.GetWorldStateAt(t);
            slider.SetValueWithoutNotify(Mathf.RoundToInt(t));
            GameTimeManager.UpdateDateFromSeconds(t);
            slider.label = FormatTimeLabel(t);
            GameTimeManager.Instance?.SetObservationMode(true);
            inPast = true;
            presentButton?.SetEnabled(true);
        }
        else
        {
            TimelineManager.Instance?.GetWorldStateAt(GameClock.Time);
            if (inPast)
            {
                TimelineManager.Instance?.RemoveLastSnapshot();
                GameTimeManager.UpdateDateFromSeconds(GameClock.Time);
                slider.label = FormatTimeLabel(GameClock.Time);
            }
            else
            {
                GameTimeManager.UpdateDateFromSeconds(GameClock.Time);
                slider.label = FormatTimeLabel(GameClock.Time);
            }
            GameTimeManager.Instance?.SetObservationMode(false);
            inPast = false;
            presentButton?.SetEnabled(false);
            selectedTime = GameClock.Time;
        }
    }




    void BeginTimeTravel()
    {
        TimelineManager.Instance?.BeginTimeTravelTo(selectedTime);
        GameTimeManager.Instance?.SetObservationMode(false);
        slider?.SetEnabled(false);
        timeTravelButton?.SetEnabled(false);
        presentButton?.SetEnabled(true);
        inPast = false;
    }

    void ReturnToPresent()
    {
        TimelineManager.Instance?.FinishTimeTravel();
        if (inPast)
        {
            TimelineManager.Instance?.GetWorldStateAt(GameClock.Time);
            TimelineManager.Instance?.RemoveLastSnapshot();
            GameTimeManager.Instance?.SetObservationMode(false);
            GameTimeManager.UpdateDateFromSeconds(GameClock.Time);
        }
        GameTimeManager.Instance?.SetObservationMode(false);
        if (slider != null)
        {
            int now = Mathf.RoundToInt(GameClock.Time);
            slider.highValue = now;
            slider.SetValueWithoutNotify(now);
            slider.label = FormatTimeLabel(now);
            UpdateSliderHandle();
            DrawMonthTicks();
            slider.SetEnabled(true);
            selectedTime = now;
        }
        timeTravelButton?.SetEnabled(true);
        presentButton?.SetEnabled(false);
        inPast = false;
    }

    void UpdateSliderHandle()
    {
        if (slider == null) return;
        var dragger = slider.Q(className: "unity-dragger");
        if (dragger == null) return;

        dragger.style.flexGrow = 0;
        dragger.style.flexShrink = 0;

        float totalSeconds = slider.highValue - slider.lowValue;
        if (totalSeconds <= 0f) return;

        float monthSeconds = GameTimeManager.SecondsPerMonth;
        float ratio = monthSeconds / totalSeconds;
        float width = slider.resolvedStyle.width * ratio;
        if (width < 4f) width = 4f;

        dragger.style.width = width;
    }

    void DrawMonthTicks()
    {
        if (slider == null || slider.resolvedStyle.width <= 0) return;

        if (tickContainer == null)
        {
            tickContainer = new VisualElement();
            tickContainer.style.position = Position.Absolute;
            tickContainer.style.bottom = 0;
            tickContainer.style.left = 0;
            tickContainer.style.right = 0;
            tickContainer.style.height = 20;
            tickContainer.pickingMode = PickingMode.Ignore;
            slider.parent.Add(tickContainer);
        }

        tickContainer.Clear();

        float totalSeconds = slider.highValue - slider.lowValue;
        if (totalSeconds <= 0f) return;

        int totalMonths = Mathf.CeilToInt(totalSeconds / GameTimeManager.SecondsPerMonth);
        float width = slider.resolvedStyle.width;

        int monthStep = Mathf.Max(1, Mathf.CeilToInt(totalMonths / 12f));

        for (int m = 0; m <= totalMonths; m += monthStep)
        {
            float x = (m / (float)totalMonths) * width;
            float timeAtTick = slider.lowValue + (m * GameTimeManager.SecondsPerMonth);

            var tick = new VisualElement();
            tick.style.position = Position.Absolute;
            tick.style.left = x;
            tick.style.bottom = 0;
            tick.style.width = 2;
            tick.style.height = 10;
            tick.style.backgroundColor = Color.white;
            tickContainer.Add(tick);

            var label = new Label(FormatTimeLabel(timeAtTick));
            label.style.position = Position.Absolute;
            label.style.left = x - 12;
            label.style.bottom = 12;
            label.style.fontSize = 8;
            label.style.color = Color.white;
            tickContainer.Add(label);
        }

        if (totalMonths % monthStep != 0)
        {
            float x = width;
            float timeAtTick = slider.highValue;

            var tick = new VisualElement();
            tick.style.position = Position.Absolute;
            tick.style.left = x;
            tick.style.bottom = 0;
            tick.style.width = 2;
            tick.style.height = 10;
            tick.style.backgroundColor = Color.white;
            tickContainer.Add(tick);

            var label = new Label(FormatTimeLabel(timeAtTick));
            label.style.position = Position.Absolute;
            label.style.left = x - 12;
            label.style.bottom = 12;
            label.style.fontSize = 8;
            label.style.color = Color.white;
            tickContainer.Add(label);
        }
    }


    string FormatTimeLabel(float seconds)
    {
        GameTimeManager.SecondsToDate(seconds, out var month, out var year);
        return $"M{month} - A{year}";
    }
}
