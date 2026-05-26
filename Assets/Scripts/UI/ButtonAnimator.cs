using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Add to any button GameObject alongside the Button component.
/// Handles hover scale-up and click scale-down animations.
/// </summary>
public class ButtonAnimator : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler
{
    [Header("Hover")]
    [SerializeField] private float hoverScale = 1.1f;
    [SerializeField] private float hoverDuration = 0.1f;

    [Header("Click")]
    [SerializeField] private float clickScale = 0.9f;
    [SerializeField] private float clickDuration = 0.07f;

    private Vector3 originalScale;
    private Coroutine scaleCoroutine;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ScaleTo(originalScale * hoverScale, hoverDuration);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ScaleTo(originalScale, hoverDuration);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        ScaleTo(originalScale * clickScale, clickDuration);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Return to hover scale if still hovering, original if not
        bool isHovering = RectTransformUtility.RectangleContainsScreenPoint(
            GetComponent<RectTransform>(),
            eventData.position,
            eventData.pressEventCamera
        );

        ScaleTo(isHovering ? originalScale * hoverScale : originalScale, clickDuration);
    }

    private void ScaleTo(Vector3 targetScale, float duration)
    {
        if (scaleCoroutine != null)
            StopCoroutine(scaleCoroutine);

        scaleCoroutine = StartCoroutine(ScaleRoutine(targetScale, duration));
    }

    private IEnumerator ScaleRoutine(Vector3 targetScale, float duration)
    {
        Vector3 startScale = transform.localScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // Smoothstep for a natural feel
            float smoothT = t * t * (3f - 2f * t);

            transform.localScale = Vector3.LerpUnclamped(startScale, targetScale, smoothT);
            yield return null;
        }

        transform.localScale = targetScale;
    }

    // Reset scale if the object is disabled mid-animation (e.g. panel closes)
    private void OnDisable()
    {
        if (scaleCoroutine != null)
            StopCoroutine(scaleCoroutine);

        transform.localScale = originalScale;
    }
}