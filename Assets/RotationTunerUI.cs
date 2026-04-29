using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RotationTunerUI : MonoBehaviour
{
    [Header("Target Script")]
    [SerializeField] private RotateOnInput rotateOnInput;

    [Header("Sliders")]
    [SerializeField] private Slider rotationSpeedSlider;
    [SerializeField] private Slider smoothTimeSlider;
    [SerializeField] private Slider inertiaDampingSlider;
    [SerializeField] private Slider autoRotateSpeedSlider;

    [Header("Live Value Labels")]
    [SerializeField] private TextMeshProUGUI rotationSpeedLabel;
    [SerializeField] private TextMeshProUGUI smoothTimeLabel;
    [SerializeField] private TextMeshProUGUI inertiaDampingLabel;
    [SerializeField] private TextMeshProUGUI autoRotateSpeedLabel;

    private void Start()
    {
        // configure ranges and defaults
        SetupSlider(rotationSpeedSlider, 60f, 600f, 240f);
        SetupSlider(smoothTimeSlider, 0.01f, 0.25f, 0.07f);
        SetupSlider(inertiaDampingSlider, 0.5f, 8f, 2.0f);
        SetupSlider(autoRotateSpeedSlider, 10f, 180f, 60f);

        // wire sliders → setters
        rotationSpeedSlider.onValueChanged.AddListener(v => { rotateOnInput.SetRotationSpeed(v); UpdateLabel(rotationSpeedLabel, v, "F0"); });
        smoothTimeSlider.onValueChanged.AddListener(v => { rotateOnInput.SetSmoothTime(v); UpdateLabel(smoothTimeLabel, v, "F2"); });
        inertiaDampingSlider.onValueChanged.AddListener(v => { rotateOnInput.SetInertiaDamping(v); UpdateLabel(inertiaDampingLabel, v, "F1"); });
        autoRotateSpeedSlider.onValueChanged.AddListener(v => { rotateOnInput.SetAutoRotateSpeed(v); UpdateLabel(autoRotateSpeedLabel, v, "F0"); });

        // set initial label text
        UpdateLabel(rotationSpeedLabel, rotationSpeedSlider.value, "F0");
        UpdateLabel(smoothTimeLabel, smoothTimeSlider.value, "F2");
        UpdateLabel(inertiaDampingLabel, inertiaDampingSlider.value, "F1");
        UpdateLabel(autoRotateSpeedLabel, autoRotateSpeedSlider.value, "F0");
    }

    private void SetupSlider(Slider s, float min, float max, float defaultVal)
    {
        if (s == null) return;
        s.minValue = min;
        s.maxValue = max;
        s.value = defaultVal;
    }

    private void UpdateLabel(TextMeshProUGUI label, float value, string format)
    {
        if (label != null)
            label.text = value.ToString(format);
    }
}