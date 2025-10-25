using UnityEngine;

public class TireRotator : MonoBehaviour
{
    [Header("Rotation Settings")]
    public float rotationSpeed = 180f; // Degrees per second
    public bool rotateClockwise = true;
    public bool rotateOnlyWhenMoving = false;
    
    [Header("Movement Detection (if rotateOnlyWhenMoving is true)")]
    public float minSpeedThreshold = 0.1f;
    
    private Rigidbody2D rb;
    private float currentRotationSpeed;
    
    void Start()
    {
        // Get Rigidbody2D component if it exists (for movement detection)
        rb = GetComponent<Rigidbody2D>();
        
        // Set initial rotation speed
        currentRotationSpeed = rotateClockwise ? rotationSpeed : -rotationSpeed;
    }
    
    void Update()
    {
        // Check if we should rotate based on movement
        bool shouldRotate = true;
        
        if (rotateOnlyWhenMoving && rb != null)
        {
            // Only rotate if the object is moving above the threshold
            float currentSpeed = rb.velocity.magnitude;
            shouldRotate = currentSpeed >= minSpeedThreshold;
        }
        
        // Rotate the tire
        if (shouldRotate)
        {
            float rotationAmount = currentRotationSpeed * Time.deltaTime;
            transform.Rotate(0, 0, rotationAmount);
        }
    }
    
    // Method to change rotation speed at runtime
    public void SetRotationSpeed(float newSpeed)
    {
        rotationSpeed = newSpeed;
        currentRotationSpeed = rotateClockwise ? rotationSpeed : -rotationSpeed;
    }
    
    // Method to reverse rotation direction
    public void ReverseRotation()
    {
        rotateClockwise = !rotateClockwise;
        currentRotationSpeed = rotateClockwise ? rotationSpeed : -rotationSpeed;
    }
    
    // Method to stop rotation
    public void StopRotation()
    {
        rotationSpeed = 0f;
        currentRotationSpeed = 0f;
    }
    
    // Method to start rotation
    public void StartRotation(float speed = 180f)
    {
        rotationSpeed = speed;
        currentRotationSpeed = rotateClockwise ? rotationSpeed : -rotationSpeed;
    }
}

