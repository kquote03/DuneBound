using UnityEngine;
using UnityEngine.InputSystem;

public class StickMovement1 : MonoBehaviour
{
    public Camera mainCam;
    [Header("Target")]
    [SerializeField] private Transform stickTarget; // The Walking Stick object

    [Header("Left Arm")]
    [SerializeField] private Transform leftArmBone;
    [Tooltip("Adjust X to fix initial rotation (usually 0, 90, or -90)")]
    [SerializeField] private Vector3 leftRotationOffset;

    [Header("Right Arm")]
    [SerializeField] private Transform rightArmBone;
    [Tooltip("Adjust X to fix initial rotation (usually 0, 90, or -90)")]
    [SerializeField] private Vector3 rightRotationOffset;

    [Header("Settings")]
    [Tooltip("If true, arms scale length to ensure hands touch the stick.")]
    [SerializeField] private bool stretchToReach = true;
    [SerializeField] private float originalArmLength = 1.0f;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugRays = true;

    void Start()
    {
        mainCam = Camera.main;
        Cursor.lockState = CursorLockMode.Locked;
        
        // Auto-calculate arm lengths if not set
        if (originalArmLength <= 0)
        {
            if (leftArmBone != null && leftArmBone.childCount > 0)
            {
                originalArmLength = Vector3.Distance(leftArmBone.position, leftArmBone.GetChild(0).position);
            }
            else
            {
                originalArmLength = 1.0f;
            }
        }
    }

    void Update()
    {
        if (Mouse.current == null) return;

        // Move the stick target to follow cursor
        Vector2 mousePixelPos = Mouse.current.position.ReadValue();
        Ray ray = mainCam.ScreenPointToRay(mousePixelPos);

        // Create a plane locked to the X-axis (for side-view 2D gameplay)
        Plane wallPlane = new Plane(Vector3.right, transform.position);

        if (wallPlane.Raycast(ray, out float distance))
        {
            Vector3 hitPoint = ray.GetPoint(distance);

            // Move the stick target (not the character)
            if (stickTarget != null)
            {
                stickTarget.position = hitPoint;
            }

            if (showDebugRays)
            {
                Debug.DrawLine(ray.origin, hitPoint, Color.green);
                Debug.DrawRay(hitPoint, Vector3.up * 0.5f, Color.yellow);
            }
        }
    } 

    void LateUpdate()
    {
        if (Mouse.current == null) return;

        // Get target position (either stick target or mouse world position)
        Vector3 targetPosition = stickTarget != null ? stickTarget.position : GetMouseWorldPosition();

        // Update both arms to point at and stretch to the target
        if (leftArmBone != null) 
            UpdateArm(leftArmBone, targetPosition, leftRotationOffset);

        if (rightArmBone != null) 
            UpdateArm(rightArmBone, targetPosition, rightRotationOffset);
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector2 mousePixelPos = Mouse.current.position.ReadValue();
        Ray ray = mainCam.ScreenPointToRay(mousePixelPos);

        // Define the "Gameplay Plane" (The invisible wall the cursor slides on)
        Plane gameplayPlane = new Plane(Vector3.right, transform.position);

        if (gameplayPlane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }

        // Fallback
        return transform.position;
    }

    private void UpdateArm(Transform arm, Vector3 targetPos, Vector3 offset)
    {
        // 1. Calculate direction from arm to target
        Vector3 direction = targetPos - arm.position;
        
        if (showDebugRays)
        {
            Debug.DrawLine(arm.position, targetPos, Color.cyan);
        }

        // 2. Calculate Angle in Y-Z plane (for side-view)
        //    Atan2(y, z) gives the angle in the Y-Z plane
        float angle = Mathf.Atan2(direction.y, direction.z) * Mathf.Rad2Deg;

        // 3. Apply Rotation around X-axis (this creates the hinge effect)
        //    The negative angle is correct for Unity's coordinate system
        float finalX = -angle + offset.x;
        arm.localRotation = Quaternion.Euler(finalX, offset.y, offset.z);

        // 4. Stretch the arm to reach the target
        if (stretchToReach)
        {
            // Calculate distance to target
            float dist = direction.magnitude;
            
            // Calculate stretch factor based on original length
            float stretchFactor = dist / originalArmLength;
            
            // Clamp to reasonable values (optional, prevents extreme stretching)
            stretchFactor = Mathf.Clamp(stretchFactor, 0.5f, 3.0f);

            // Apply stretch along the bone's local Y-axis (typical Unity bone orientation)
            // If your bones are oriented differently, try (stretchFactor, 1, 1) or (1, 1, stretchFactor)
            arm.localScale = new Vector3(1, stretchFactor, 1);
        }
        else
        {
            arm.localScale = Vector3.one;
        }
    }
}