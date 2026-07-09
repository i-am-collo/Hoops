using UnityEngine;

public class HoopController : MonoBehaviour
{
    [Header("Movement")]
    public float baseMovementWidth = 2.2f; // Max horizontal movement distance from center
    public float currentSpeed = 0f;

    [Header("Rim & Net References")]
    public Transform leftRim;
    public Transform rightRim;
    public Collider2D scoreTrigger;

    private float startX;
    private int movementDirection = 1;

    private void Start()
    {
        startX = transform.position.x;
        UpdateHoopSpeed(ScoreManager.Instance != null ? ScoreManager.Instance.CurrentLevel : 1);

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnLevelChanged += UpdateHoopSpeed;
        }
    }

    private void OnDestroy()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnLevelChanged -= UpdateHoopSpeed;
        }
    }

    private void Update()
    {
        if (currentSpeed > 0f && GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Gameplay)
        {
            float targetX = transform.position.x + movementDirection * currentSpeed * Time.deltaTime;
            
            // Reverse direction if hitting limits
            if (Mathf.Abs(targetX - startX) > baseMovementWidth)
            {
                movementDirection *= -1;
                targetX = startX + movementDirection * baseMovementWidth;
            }

            transform.position = new Vector3(targetX, transform.position.y, transform.position.z);
        }
    }

    public void UpdateHoopSpeed(int level)
    {
        if (ScoreManager.Instance != null)
        {
            LevelConfig config = ScoreManager.Instance.GetCurrentLevelConfig();
            if (config != null)
            {
                currentSpeed = config.hoopSpeed;
            }
            else
            {
                // Fallbacks from GDD
                switch (level)
                {
                    case 1: currentSpeed = 0f; break;
                    case 2: currentSpeed = 2f; break;
                    case 3: currentSpeed = 3f; break;
                    case 4: currentSpeed = 4.5f; break;
                    default: currentSpeed = 6f; break;
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerBall"))
        {
            Rigidbody2D ballRb = other.GetComponent<Rigidbody2D>();
            BallController ballCtrl = other.GetComponent<BallController>();

            if (ballRb != null && ballCtrl != null)
            {
                // Ensure the ball is falling downward to trigger a valid basket (prevent scoring from bottom)
                if (ballRb.linearVelocity.y < 0.1f && other.transform.position.y > transform.position.y)
                {
                    // Success! Basket scored!
                    if (AudioManager.Instance != null)
                    {
                        AudioManager.Instance.PlaySwish();
                    }

                    if (ScoreManager.Instance != null)
                    {
                        ScoreManager.Instance.AddScore();
                    }

                    // Trigger ball respawn immediately
                    ballCtrl.TriggerRespawn();
                }
            }
        }
    }
}
