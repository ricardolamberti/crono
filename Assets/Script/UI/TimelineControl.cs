using UnityEngine;
using UnityEngine.UIElements;

public class TimelineControl : MonoBehaviour
{
    public UIDocument uiDocument;
    private Slider slider;
    private Button presentButton;
    private VisualElement tickContainer;
    private bool inPast = false;

    void OnEnable()
    {
        if (uiDocument == null) return;
        var root = uiDocument.rootVisualElement;
        slider = root.Q<Slider>("TimelineSlider");
        presentButton = root.Q<Button>("PresentButton");

        if (slider != null)
        {
            slider.lowValue = 0f;
            slider.highValue = Time.time;
            slider.showInputField = false;
            slider.SetValueWithoutNotify(Time.time);

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
    }


    void Update()
    {
        if (slider == null) return;

        slider.highValue = Time.time;

        // 🔥 Solo auto-actualiza si NO estamos en el pasado
        if (!inPast)
        {
            slider.SetValueWithoutNotify(Time.time);
            GameTimeManager.UpdateDateFromSeconds(Time.time);
            slider.label = FormatTimeLabel(Time.time);
        }

        UpdateSliderHandle();
        DrawMonthTicks();
    }

    void OnSliderChanged(ChangeEvent<float> evt)
    {
        Debug.Log($"[Timeline] Cambió slider: {evt.newValue}");
        float t = evt.newValue;
        bool past = t < Time.time - 0.1f; // margen pequeño

        GameTimeManager.UpdateDateFromSeconds(t);
        slider.label = FormatTimeLabel(t);

        if (past)
        {
            TimelineManager.Instance?.GetWorldStateAt(t);
            GameTimeManager.Instance?.SetObservationMode(true);
            inPast = true;
        }
        else
        {
            GameTimeManager.Instance?.SetObservationMode(false);
            inPast = false;
        }
    }


 

    void ReturnToPresent()
    {
        inPast = false;
        GameTimeManager.Instance?.SetObservationMode(false);
        if (slider != null)
        {
            slider.highValue = Time.time;
            slider.SetValueWithoutNotify(Time.time);
            GameTimeManager.UpdateDateFromSeconds(Time.time);
            slider.label = FormatTimeLabel(Time.time);
            UpdateSliderHandle();
            DrawMonthTicks();
        }
        TimelineManager.Instance?.GetWorldStateAt(Time.time);
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
        GameTimeManager.UpdateDateFromSeconds(seconds);
        return $"M{GameTimeManager.CurrentMonth} - A{GameTimeManager.CurrentYear}";
    }
}
