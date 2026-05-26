using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attach to: each level card prefab in the Level Select grid.
/// Requires:  Button, Image (thumbnail), Image (lock overlay),
///            TextMeshProUGUI (level number), Image[] (stars).
/// </summary>
[RequireComponent(typeof(Button))]
public class LevelCard : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI levelNumberText;
    [SerializeField] private Image thumbnailImage;
    [SerializeField] private GameObject lockOverlay;
    [SerializeField] private Image[] starImages;

    [Header("Stars")]
    [SerializeField] private Color starEarnedColor = Color.yellow;
    [SerializeField] private Color starEmptyColor = Color.grey;

    private Button button;
    private int levelIndex;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    public void Setup(int index, Sprite thumbnail)
    {
        levelIndex = index;
        levelNumberText.text = index.ToString();

        if (thumbnail != null)
            thumbnailImage.sprite = thumbnail;

        SaveSystem.LevelData data = SaveSystem.Instance.GetLevelData(levelIndex);

        if (data.isUnlocked)
        {
            lockOverlay.SetActive(false);
            button.interactable = true;
            button.onClick.AddListener(OnCardClicked);
            UpdateStars(data.stars);
        }
        else
        {
            lockOverlay.SetActive(true);
            button.interactable = false;
            ResetStars();
        }
    }

    private void UpdateStars(int earnedStars)
    {
        for (int i = 0; i < starImages.Length; i++)
            starImages[i].color = i < earnedStars ? starEarnedColor : starEmptyColor;
    }

    private void ResetStars()
    {
        foreach (Image star in starImages)
            star.color = starEmptyColor;
    }

    private void OnCardClicked()
    {
        // Level scenes start at build index 2 (0=MainMenu, 1=LevelSelect, 2=Level1...)
        SceneLoader.Instance.LoadSceneByIndex(levelIndex + 1);
    }
}