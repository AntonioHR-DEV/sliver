using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MovingPlatform : MonoBehaviour
{
    [Header("Points")]
    [SerializeField] private Transform pointATransform;
    [SerializeField] private Transform pointBTransform;

    [Header("Movement")]
    [SerializeField] private float speed = 3f;
    [SerializeField] private float pauseDuration = 0.5f;

    private Rigidbody2D rb;
    private Vector2 pointA;
    private Vector2 pointB;
    private Vector2 target;

    private bool isPaused;

    public Vector2 DeltaMovement { get; private set; }
    public bool IsMoving => !isPaused;
    public Vector3 PointA => pointATransform.position;
    public Vector3 PointB => pointBTransform.position;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    private void Start()
    {
        pointA = pointATransform.position;
        pointB = pointBTransform.position;

        rb.position = pointA;
        target = pointB;
    }

    private void FixedUpdate()
    {
        DeltaMovement = Vector2.zero;

        if (isPaused) return;

        Vector2 oldPos = rb.position;
        Vector2 newPos = Vector2.MoveTowards(oldPos, target, speed * Time.fixedDeltaTime);

        rb.MovePosition(newPos);
        DeltaMovement = newPos - oldPos;

        if (Vector2.Distance(newPos, target) < 0.01f)
        {
            StartCoroutine(PauseAtEndpoint());
        }
    }

    private IEnumerator PauseAtEndpoint()
    {
        isPaused = true;
        yield return new WaitForSeconds(pauseDuration);
        target = (target == pointA) ? pointB : pointA;
        isPaused = false;
    }

    private void OnDrawGizmos()
    {
        if (pointATransform == null || pointBTransform == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(pointATransform.position, 0.15f);
        Gizmos.DrawSphere(pointBTransform.position, 0.15f);
        Gizmos.DrawLine(pointATransform.position, pointBTransform.position);
    }
}