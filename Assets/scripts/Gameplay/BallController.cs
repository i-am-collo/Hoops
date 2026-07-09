using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BallController : MonoBehaviour
{
    [Header("Physics & Launch")]
    public Rigidbody2D rb;
    public CircleCollider2D col;
    public LineRenderer lineRenderer;
    public TrailRenderer trailRenderer;

    [Header("Swipe Settings")]
    public float forceMultiplier = 5f;
    public float maxForce = 20f;
    public float spinMultiplier = 30f;
    public float respawnDelay = 0.4f;

    [Header("Trajectory Settings")]
    public int trajectoryStepCount = 20;
    public float trajectoryStepTime = 0.05f;

    private Vector2 spawnPosition;
    private bool isDragging = false;
    private Vector2 dragStartPos; // Screen coordinates
    private Camera mainCam;
    private bool canShoot = true;
    private bool hasLauched = false;

    private float idleTimer = 0f;
    private const float MaxIdleTimeAfterLaunch = 4f; // Auto-reset if stuck

    private void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (col == null) col = GetComponent<CircleCollider2D>();
        if (lineRenderer == null) lineRenderer = GetComponent<LineRenderer>();
        if (trailRenderer == null) trailRenderer = GetComponent<TrailRenderer>();

        mainCam = Camera.main;
        spawnPosition = transform.position;

        // Initialize LineRenderer
        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 0;
            lineRenderer.startWidth = 0.08f;
            lineRenderer.endWidth = 0.02f;
        }

        ResetBall();
    }

    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Gameplay)
        {
            isDragging = false;
            if (lineRenderer != null) lineRenderer.positionCount = 0;
            return;
        }

        HandleInput();

        // Handle auto-respawn if ball gets stuck in gameplay scene
        if (hasLauched)
        {
            idleTimer += Time.deltaTime;
            // If ball falls below viewport or is idle too long, respawn
            if (mainCam == null) mainCam = Camera.main;
            if (mainCam != null)
            {
                Vector3 viewportPos = mainCam.WorldToViewportPoint(transform.position);
                if (viewportPos.y < -0.2f || viewportPos.x < -0.5f || viewportPos.x > 1.5f || idleTimer > MaxIdleTimeAfterLaunch)
                {
                    // This is a miss!
                    if (ScoreManager.Instance != null)
                    {
                        ScoreManager.Instance.ResetCombo();
                    }
                    TriggerRespawn();
                }
            }
        }
    }

    private void HandleInput()
    {
        if (!canShoot) return;

        bool isPressing = false;
        Vector2 pointerScreenPos = Vector2.zero;

        Pointer pointer = Pointer.current;
        if (pointer != null)
        {
            isPressing = pointer.press.isPressed;
            pointerScreenPos = pointer.position.ReadValue();
        }
        else if (Mouse.current != null)
        {
            isPressing = Mouse.current.leftButton.isPressed;
            pointerScreenPos = Mouse.current.position.ReadValue();
        }
        else if (Touchscreen.current != null && Touchscreen.current.touches.Count > 0)
        {
            isPressing = Touchscreen.current.touches[0].press.isPressed;
            pointerScreenPos = Touchscreen.current.touches[0].position.ReadValue();
        }
        else
        {
            return;
        }

        if (isPressing)
        {
            if (!isDragging)
            {
                // Touch began - check if on basketball or near it
                if (mainCam == null) mainCam = Camera.main;
                if (mainCam == null) return;
                
                Vector2 pointerWorldPos = mainCam.ScreenToWorldPoint(pointerScreenPos);
                float distToBall = Vector2.Distance(pointerWorldPos, transform.position);

                // Check overlap or distance to make dragging extremely reliable
                if (distToBall < 1.5f) // Allow slightly wider touch margin for Android/portrait
                {
                    isDragging = true;
                    dragStartPos = pointerScreenPos;
                }
            }
            else
            {
                // Dragging - update trajectory line
                Vector2 currentScreenPos = pointerScreenPos;
                Vector2 launchForce = CalculateLaunchForce(dragStartPos, currentScreenPos);
                DrawTrajectory(launchForce);
            }
        }
        else
        {
            if (isDragging)
            {
                // Released - Launch Ball!
                isDragging = false;
                if (lineRenderer != null) lineRenderer.positionCount = 0;

                Vector2 launchForce = CalculateLaunchForce(dragStartPos, pointerScreenPos);
                if (launchForce.magnitude > 1.5f) // Minimum swipe threshold
                {
                    Launch(launchForce);
                }
            }
        }
    }

    private Vector2 CalculateLaunchForce(Vector2 start, Vector2 end)
    {
        // Direct swipe: end - start (swipe forward to shoot forward)
        Vector2 swipe = end - start;

        // Convert swipe from screen to world-relative force
        float screenDiagonal = Mathf.Sqrt(Screen.width * Screen.width + Screen.height * Screen.height);
        float swipeNormalized = swipe.magnitude / (screenDiagonal * 0.2f); // 20% of screen size as max force gauge

        Vector2 direction = swipe.normalized;
        float forceMagnitude = Mathf.Clamp(swipeNormalized * forceMultiplier, 0.5f, maxForce);

        return direction * forceMagnitude;
    }

    private void DrawTrajectory(Vector2 force)
    {
        if (lineRenderer == null) return;

        lineRenderer.positionCount = trajectoryStepCount;
        Vector2 velocity = force / rb.mass;
        Vector2 currentPos = transform.position;
        Vector2 gravity = Physics2D.gravity * 1f; // Fix: Use 1f instead of rb.gravityScale (which is 0f when kinematic before shooting)

        for (int i = 0; i < trajectoryStepCount; i++)
        {
            lineRenderer.SetPosition(i, new Vector3(currentPos.x, currentPos.y, 0f));
            float t = trajectoryStepTime;
            currentPos += velocity * t + 0.5f * gravity * t * t;
            velocity += gravity * t;
        }
    }

    private void Launch(Vector2 force)
    {
        canShoot = false;
        hasLauched = true;
        idleTimer = 0f;

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 1f;
        rb.linearVelocity = force;

        // Apply backspin depending on launch X direction
        rb.angularVelocity = -force.x * spinMultiplier;

        if (trailRenderer != null)
        {
            trailRenderer.enabled = true;
            trailRenderer.Clear();
        }

        // Play throw sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.countdownBeepClip); // Use countdownBeepClip or similar throw sound
        }
    }

    public void TriggerRespawn()
    {
        if (!hasLauched) return;
        hasLauched = false;
        StartCoroutine(RespawnSequence());
    }

    private IEnumerator RespawnSequence()
    {
        yield return new WaitForSeconds(respawnDelay);
        ResetBall();
    }

    public void ResetBall()
    {
        isDragging = false;
        hasLauched = false;
        canShoot = true;
        idleTimer = 0f;

        transform.position = spawnPosition;
        transform.rotation = Quaternion.identity;

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.gravityScale = 0f;

        if (trailRenderer != null)
        {
            trailRenderer.enabled = false;
            trailRenderer.Clear();
        }
        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 0;
        }

        // Notify score manager that a ball was reset (resets combo if ball missed)
        // If the ball has launched and reset without scoring, it's a miss
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Fix: Also check parent for the "Hoop" tag because RimLeft/RimRight colliders are children of the Hoop GameObject
        if (collision.gameObject.CompareTag("Hoop") || (collision.transform.parent != null && collision.transform.parent.CompareTag("Hoop")))
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayRimBounce();
            }
        }
    }
}
