using UnityEngine;
using TMPro; // Required for TextMeshPro UI elements

public class UIVegetableDisplay : MonoBehaviour
{
    [Header("UI Reference")]
    [Tooltip("The TextMeshPro UI element that will display the vegetable name.")]
    public TextMeshProUGUI vegetableNameText;

    /// <summary>
    /// The core function that changes the text.
    /// </summary>
    private void UpdateText(string name)
    {
        if (vegetableNameText != null)
        {
            vegetableNameText.text = $"Selected: {name}";
            Debug.Log($"UI updated to show: {name}");
        }
    }

    // --- Wrapper Functions ---
    // Create one simple, public function for each vegetable you want to display.
    // These are the functions you will call from the grab event.

    public void ShowTomatoText()
    {
        UpdateText("Tomato");
    }

    public void ShowPotatoText()
    {
        UpdateText("Potato");
    }

    public void ShowCarrotText()
    {
        UpdateText("Carrot");
    }

    public void ClearText()
    {
        UpdateText(""); // Clears the text
    }
}