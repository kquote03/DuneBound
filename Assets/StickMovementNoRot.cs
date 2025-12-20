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
    
    [Tooltip("How many times the original length can the arm stretch?")]
    [SerializeField] private float maxStretchMultiplier = 3.0f; 
    
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
            Vector3 targetHitPoint = ray.GetPoint(distance);

            // --- NEW: CLAMPING LOGIC START ---
            if (stickTarget != null)
            {
                // 1. Determine the "Center" of the body/shoulders
                // If we have both arms, use the center point between them. Otherwise use player position.
                Vector3 anchorPoint = transform.position;
                if (leftArmBone != null && rightArmBone != null)
                {
                    anchorPoint = (leftArmBone.position + rightArmBone.position) * 0.5f;
                }

                // 2. Calculate vector from body to mouse
                Vector3 directionToMouse = targetHitPoint - anchorPoint;
                float currentDistance = directionToMouse.magnitude;

                // 3. Calculate Maximum Allowed Reach
                float maxReach = originalArmLength * maxStretchMultiplier;

                // 4. If mouse is too far, clamp the position
                if (currentDistance > maxReach)
                {
                    // Normalize the direction and multiply by max reach to keep it on the edge
                    targetHitPoint = anchorPoint + (directionToMouse.normalized * maxReach);
                }

                // Apply position
                stickTarget.position = targetHitPoint;
            }
            // --- NEW: CLAMPING LOGIC END ---

            if (showDebugRays)
            {
                Debug.DrawLine(ray.origin, targetHitPoint, Color.green);
                Debug.DrawRay(targetHitPoint, Vector3.up * 0.5f, Color.yellow);
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
        Plane gameplayPlane = new Plane(Vector3.right, transform.position);

        if (gameplayPlane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }
        return transform.position;
    }

    private void UpdateArm(Transform arm, Vector3 targetPos, Vector3 offset)
    {
        Vector3 direction = targetPos - arm.position;
        
        if (showDebugRays)
        {
            Debug.DrawLine(arm.position, targetPos, Color.cyan);
        }

        float angle = Mathf.Atan2(direction.y, direction.z) * Mathf.Rad2Deg;
        float finalX = -angle + offset.x;
        arm.localRotation = Quaternion.Euler(finalX, offset.y, offset.z);

        if (stretchToReach)
        {
            float dist = direction.magnitude;
            float stretchFactor = dist / originalArmLength;
            
            // Limit the stretch factor visually as well, using the new variable
            stretchFactor = Mathf.Clamp(stretchFactor, 0.5f, maxStretchMultiplier);

            arm.localScale = new Vector3(1, stretchFactor, 1);
        }
        else
        {
            arm.localScale = Vector3.one;
        }
    }
}