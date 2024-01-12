using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PanelSlide : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI buttonText;
    [SerializeField] private RectTransform panel;
    [SerializeField] private bool isShown;


    private void Start()
    {
        button.onClick.AddListener(PanPanel);
    }

    private void PanPanel()
    {
        //make more efficient later by making a permanent Vector 2 for both sliding in and out
        //maybe add a lerp transition
        if (isShown)
        {
            panel.anchoredPosition -= new Vector2(panel.rect.width + 5f, 0);
            buttonText.text = ">";
        }
        else
        {
            panel.anchoredPosition += new Vector2(panel.rect.width + 5f, 0);
            buttonText.text = "<";
        }

        isShown = !isShown;
    }
    private void Reset()
    {
        button = GetComponent<Button>();
        buttonText = GetComponentInChildren<TextMeshProUGUI>();
        transform.parent.TryGetComponent(out panel);
        isShown = true;
    }
}
