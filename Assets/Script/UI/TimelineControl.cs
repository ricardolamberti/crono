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
            slider.highValue = Mathf.RoundToInt(Time.time);
            slider.showInputField = false;
            slider.SetValueWithoutNotify(Mathf.RoundToInt(Time.time));

            GameTimeManager.UpdateDateFromSeconds(Time.time);
            slider.label = FormatTimeLabel(Time.time);

            slider.RegisterValueChangedCallback(OnSliderChanged);

            // ✅ Espera hasta que el layout tenga tamaño real
            slider.schedule.Execute(() =>
            {
                UpdateSliderHandle();
                DrawMonthTicks();
            }).StartingIn(100);
        }

        if (presentButton != null)
            presentButton.clicked += ReturnToPresent;
        if (timeTravelButton != null)
            timeTravelButton.clicked += BeginTimeTravel;
    }


    void Update()
    {
        if (slider == null) return;

        slider.highValue = Mathf.RoundToInt(Time.time);

        // 🔥 Solo auto-actualiza si NO estamos en el pasado
        if (!inPast)
        {
            int now = Mathf.RoundToInt(Time.time);
            slider.SetValueWithoutNotify(now);
            GameTimeManager.UpdateDateFromSeconds(now);
            slider.label = FormatTimeLabel(now);
        }

        UpdateSliderHandle();
        DrawMonthTicks();
    }

    void OnSliderChanged(ChangeEvent<int> evt)
    {
        Debug.Log($"[Timeline] Cambió slider: {evt.newValue}");
        float t = evt.newValue;
        bool past = t < Time.time - 0.1f; // margen pequeño

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
        }
        else
        {
            TimelineManager.Instance?.GetWorldStateAt(Time.time);
            if (inPast)
            {
                TimelineManager.Instance?.RemoveLastSnapshot();
                GameTimeManager.UpdateDateFromSeconds(Time.time);
                slider.label = FormatTimeLabel(Time.time);
            }
            else
            {
                GameTimeManager.UpdateDateFromSeconds(Time.time);
                slider.label = FormatTimeLabel(Time.time);
            }
            GameTimeManager.Instance?.SetObservationMode(false);
            inPast = false;
        }
    }




    void BeginTimeTravel()
    {
        TimelineManager.Instance?.BeginTimeTravelTo();
    }

    void ReturnToPresent()
    {
        TimelineManager.Instance?.FinishTimeTravel();
        if (inPast)
        {
            TimelineManager.Instance?.GetWorldStateAt(Time.time);
            TimelineManager.Instance?.RemoveLastSnapshot();
            GameTimeManager.Instance?.SetObservationMode(false);
            GameTimeManager.UpdateDateFromSeconds(Time.time);
        }
        GameTimeManager.Instance?.SetObservationMode(false);
        if (slider != null)
        {
            int now = Mathf.RoundToInt(Time.time);
            slider.highValue = now;
            slider.SetValueWithoutNotify(now);
            slider.label = FormatTimeLabel(now);
            UpdateSliderHandle();
            DrawMonthTicks();
        }
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

        for (int i = 0; i <= totalMonths; i++)
        {
            float x = (i / (float)totalMonths) * width;
            float timeAtTick = slider.lowValue + (i * GameTimeManager.SecondsPerMonth);

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
