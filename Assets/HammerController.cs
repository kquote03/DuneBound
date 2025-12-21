using UnityEngine;

public class MouseFollowPhysics : MonoBehaviour
{
    [Header("Configuration")]
    public Transform bodyPivot; // Drag your 'Armature' or 'Game_Character' here
    public float rotationSpeed = 30f;
    
    [Header("Correction")]
    // Adjust this if your bone points 90 degrees wrong (Try 0, 90, 180, or -90)
    public float angleOffset = -90f; 

    private Rigidbody rb;
    private Camera mainCam;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        mainCam = Camera.main;

        // Important: Decouple max velocity so the hammer can move fast
        rb.maxAngularVelocity = 100f; 
    }

    void FixedUpdate()
    {
        TrackMouse();
    }

    void TrackMouse()
    {
        // 1. Get Mouse Position in World Space
        Vector3 mouseScreenPos = Input.mousePosition;
        // Set Z distance so the conversion works on the game plane
        mouseScreenPos.z = Mathf.Abs(mainCam.transform.position.z - transform.position.z);
        Vector3 mouseWorldPos = mainCam.ScreenToWorldPoint(mouseScreenPos);

        // 2. Calculate Direction from the PIVOT (Body) to the Mouse
        // We use the bodyPivot position, not the bone's current position, for better arc control
        Vector3 direction = mouseWorldPos - bodyPivot.position;

        // 3. Calculate Angle in Degrees
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // 4. Create the target rotation (Z-axis is for 2D rotation)
        Quaternion targetRotation = Quaternion.Euler(0, 0, angle + angleOffset);

        // 5. Apply Physics Rotation
        // We use Lerp for a slight smoothness, but keep speed high for responsiveness
        rb.MoveRotation(Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime));
    }
}