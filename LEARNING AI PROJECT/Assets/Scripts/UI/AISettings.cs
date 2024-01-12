using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AISettings : MonoBehaviour
{
    [SerializeField] private AIManager AIManager;
    [SerializeField] private TMP_InputField agentFilePathInput;
    [SerializeField] private TextMeshProUGUI populationSizeText;
    [SerializeField] private Slider populationSizeSlider;
    [SerializeField] private Slider maximumTrainingTimeSlider;
    [SerializeField] private TextMeshProUGUI maximumTrainingTimeText;
    [SerializeField] private Toggle saveNetworksToggle;

    private void Awake()
    {
        var tmpButton = transform.parent.GetComponentInChildren<Button>();
        tmpButton.onClick.AddListener(SaveSettings);
        populationSizeSlider.onValueChanged.AddListener(UpdatePopulationUI);
        maximumTrainingTimeSlider.onValueChanged.AddListener(UpdateTrainingTimeUI);

    }

    private void SaveSettings()
    {
        AIManager.savedGenerationFilePath = agentFilePathInput.text;
        AIManager.populationSize = (int)populationSizeSlider.value;
        AIManager.maxTimeBetweenGenerations = (int)maximumTrainingTimeSlider.value;
        AIManager.saveNetworks = saveNetworksToggle.isOn;

        AIManager.isTraining = false;

        gameObject.SetActive(false);
    }

    private void UpdatePopulationUI(float sliderValue)
    {
        populationSizeText.text = $"Population Size: {sliderValue}";
    }

    private void UpdateTrainingTimeUI(float sliderValue)
    {
        maximumTrainingTimeText.text = $"Maximum Training Time: {sliderValue}s";
    }

    private void Reset()
    {
        AIManager = FindObjectOfType<AIManager>();
        agentFilePathInput = GetComponentInChildren<TMP_InputField>();
        populationSizeText = GetComponentsInChildren<TextMeshProUGUI>()[4];
        populationSizeSlider = GetComponentsInChildren<Slider>()[0];
        maximumTrainingTimeSlider = GetComponentsInChildren<Slider>()[1];
        maximumTrainingTimeText = GetComponentsInChildren<TextMeshProUGUI>()[5];
        saveNetworksToggle = GetComponentInChildren<Toggle>();
    }
}
