using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Builds the level card grid dynamically from the LevelCard prefab.
/// Assign level thumbnails in the inspector — one sprite per level in order.
/// </summary>
public class LevelSelectUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject levelCardPrefab;
    [SerializeField] private Transform gridParent;
    [SerializeField] private Button backButton;

    [Header("Thumbnails")]
    [Tooltip("Assign one thumbnail sprite per level, in order (index 0 = Level 1).")]
    [SerializeField] private Sprite[] levelThumbnails;

    private void Start()
    {
        backButton.onClick.AddListener(OnBackClicked);
        BuildGrid();
    }

    private void BuildGrid()
    {
        foreach (Transform child in gridParent)
            Destroy(child.gameObject);

        for (int i = 1; i <= SaveSystem.Instance.TOTAL_LEVELS; i++)
        {
            GameObject cardObject = Instantiate(levelCardPrefab, gridParent);
            LevelCard card = cardObject.GetComponent<LevelCard>();

            Sprite thumbnail = i <= levelThumbnails.Length ? levelThumbnails[i - 1] : null;
            card.Setup(i, thumbnail);
        }
    }

    private void OnBackClicked()
    {
        SceneLoader.Instance.LoadScene(SceneLoader.Scene.MainMenu);
    }
}