using UnityEngine;


public class BusController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Joystick joystick;

    private Rigidbody2D rb;

    // Movement boundaries
    private float minX = -7.46f;
    private float maxX = 7.46f;
    private float minY = -0.73f;
    private float maxY = 1.37f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        // Use both X and Y input from the joystick
        float horizontalInput = joystick.Horizontal;
        float verticalInput = joystick.Vertical;

        // Apply joystick input to velocity
        Vector2 newVelocity = new Vector2(horizontalInput * moveSpeed, verticalInput * moveSpeed);
        rb.velocity = newVelocity;

        // Clamp position to stay within bounds
        Vector2 clampedPosition = rb.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, minX, maxX);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, minY, maxY);
        rb.position = clampedPosition;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // If the object has a PooledObjectMover script, treat it as a pickup
        PooledObjectMover pooledObj = other.GetComponent<PooledObjectMover>();
        if (pooledObj != null)
        {
            ScoreManager.Instance.AddPoints(4);
            pooledObj.ReturnToPool();
        }
    }
}
