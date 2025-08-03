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
            slider.RegisterCallback<GeometryChangedEvent>(_ => {
                UpdateSliderHandle();
                DrawMonthTicks();
            });
        }

        if (presentButton != null)
            presentButton.clicked += ReturnToPresent;
    }

    void Update()
    {
        if (!inPast && slider != null)
        {
            slider.highValue = Time.time;
            slider.SetValueWithoutNotify(Time.time);
            GameTimeManager.UpdateDateFromSeconds(Time.time);
            slider.label = FormatTimeLabel(Time.time);

            UpdateSliderHandle();
            DrawMonthTicks();
        }
    }

    void OnSliderChanged(ChangeEvent<float> evt)
    {
        float t = evt.newValue;
        bool past = t < Time.time;

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
        if (slider == null) return;

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
            tick.style.width = 1;
            tick.style.height = 8;
            tick.style.backgroundColor = Color.white;
            tickContainer.Add(tick);

            // 🔥 Label flotante con el mes/año
            var label = new Label(FormatTimeLabel(timeAtTick));
            label.style.position = Position.Absolute;
            label.style.left = x - 12;
            label.style.bottom = 10;
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
