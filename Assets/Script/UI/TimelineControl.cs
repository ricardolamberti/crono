using UnityEngine;
using UnityEngine.UIElements;

public class TimelineControl : MonoBehaviour
{
    public UIDocument uiDocument;
    private Slider slider;
    private Button presentButton;
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
            slider.SetValueWithoutNotify(Time.time);
            GameTimeManager.UpdateDateFromSeconds(Time.time);
            slider.label = $"Mes {GameTimeManager.CurrentMonth} - Año {GameTimeManager.CurrentYear}";
            slider.RegisterValueChangedCallback(OnSliderChanged);
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
            slider.label = $"Mes {GameTimeManager.CurrentMonth} - Año {GameTimeManager.CurrentYear}";
        }
    }

    void OnSliderChanged(ChangeEvent<float> evt)
    {
        float t = evt.newValue;
        TimelineManager.Instance?.GetWorldStateAt(t);
        GameTimeManager.UpdateDateFromSeconds(t);
        slider.label = $"Mes {GameTimeManager.CurrentMonth} - Año {GameTimeManager.CurrentYear}";
        bool past = t < Time.time;
        if (past != inPast)
        {
            inPast = past;
            GameTimeManager.Instance?.SetObservationMode(inPast);
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
            slider.label = $"Mes {GameTimeManager.CurrentMonth} - Año {GameTimeManager.CurrentYear}";
        }
        TimelineManager.Instance?.GetWorldStateAt(Time.time);
    }
}
