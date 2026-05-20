using System.Collections.Generic;
using UnityEngine;

public class MovingPlatformVisual : MonoBehaviour
{
    private static readonly int MOVING_PARAM_HASH = Animator.StringToHash("IsMoving");

    [SerializeField] private MovingPlatform movingPlatform;

    [Header("Chain")]
    [SerializeField] private Sprite chainSprite;
    [SerializeField] private float chainSpacing = 0.25f;
    private List<GameObject> chainLinks = new List<GameObject>();
    private Vector3 pointA;
    private Vector3 pointB;

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        pointA = movingPlatform.PointA;
        pointB = movingPlatform.PointB;
    }

    private void Start()
    {
        SpawnChain();
    }

    private void Update()
    {
        animator.SetBool(MOVING_PARAM_HASH, movingPlatform.IsMoving);
    }

    private void OnDestroy()
    {
        foreach (GameObject link in chainLinks)
            if (link != null) Destroy(link);
    }

    // =========================================================================
    // Chain
    // Spawns chain link sprites at regular intervals between pointA and pointB.
    // Parented to scene root so they stay fixed while the platform moves.
    // =========================================================================

    private void SpawnChain()
    {
        if (chainSprite == null) return;

        foreach (GameObject link in chainLinks)
            if (link != null) Destroy(link);
        chainLinks.Clear();

        float totalDistance = Vector3.Distance(pointA, pointB);
        int linkCount = Mathf.FloorToInt(totalDistance / chainSpacing);

        for (int i = 0; i <= linkCount; i++)
        {
            float t = linkCount == 0 ? 0f : (float)i / linkCount;
            Vector3 position = Vector3.Lerp(pointA, pointB, t);

            GameObject link = new GameObject("ChainLink");
            link.transform.position = position;

            link.transform.SetParent(ChainParent.Instance.transform, true);

            SpriteRenderer sr = link.AddComponent<SpriteRenderer>();
            sr.sprite = chainSprite;
            sr.sortingOrder = -1;

            chainLinks.Add(link);
        }
    }
}
