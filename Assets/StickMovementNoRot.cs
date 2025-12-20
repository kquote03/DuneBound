using UnityEngine;
using UnityEngine.InputSystem;

public class StickMovement_Enhanced : MonoBehaviour
{
    public Camera mainCam;
    
    [Header("Target")]
    [SerializeField] private Transform stickTarget; // The Walking Stick object
    [SerializeField] private CapsuleCollider stickCollider; // Collider on the stick

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
    
    [Header("Ground Detection")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private bool isStickGrounded = false;
    
    [Header("Dragging Physics")]
    [SerializeField] private CharacterController characterController; // The player's character controller
    [SerializeField] private float dragSpeed = 5f;
    [SerializeField] private float dragAcceleration = 10f;
    [SerializeField] private float dragDamping = 5f;
    [Tooltip("Minimum distance stick must be from player to start dragging")]
    [SerializeField] private float minDragDistance = 0.5f;
    [Tooltip("If true, player can only be dragged when stick is grounded")]
    [SerializeField] private bool requireGroundedToDrag = true;
    [Tooltip("Force applied to player when stick pushes against ground")]
    [SerializeField] private float groundPushForce = 15f;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugRays = true;

    private Vector3 lastGroundedPosition;
    private bool wasGroundedLastFrame = false;
    private Vector3 currentVelocity = Vector3.zero;
    private Vector3 lastStickPosition;

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

        // Setup stick physics components
        SetupStickPhysics();
        
        // Ensure player has a CharacterController
        if (characterController == null)
        {
            characterController = GetComponent<CharacterController>();
            if (characterController == null)
            {
                characterController = gameObject.AddComponent<CharacterController>();
                Debug.LogWarning("Added CharacterController to player. Configure radius and height as needed!");
            }
        }

        // Initialize stick position tracking
        if (stickTarget != null)
        {
            lastStickPosition = stickTarget.position;
        }
    }

    void SetupStickPhysics()
    {
        if (stickTarget == null) return;

        // Add Collider if not present (for ground detection)
        if (stickCollider == null)
        {
            stickCollider = stickTarget.GetComponent<CapsuleCollider>();
            if (stickCollider == null)
            {
                stickCollider = stickTarget.gameObject.AddComponent<CapsuleCollider>();
                // Default capsule setup for a stick
                stickCollider.radius = 0.05f;
                stickCollider.height = 1.0f;
                stickCollider.direction = 1; // Y-axis aligned
                stickCollider.isTrigger = true; // Make it a trigger so it doesn't physically collide
            }
        }
    }

    void Update()
    {
        if (Mouse.current == null) return;

        // Check if stick is grounded
        CheckGroundContact();

        // Move the stick target based on mouse
        if (!isStickGrounded || !requireGroundedToDrag)
        {
            UpdateStickTargetPosition();
        }

        // Apply movement based on stick state
        if (isStickGrounded)
        {
            // When grounded, check if player is pushing the stick
            ApplyGroundPushMovement();
        }
        else if (!requireGroundedToDrag)
        {
            // When not grounded but drag allowed, pull player toward stick
            ApplyDraggingMovement();
        }
        else
        {
            // Apply damping when not dragging
            currentVelocity = Vector3.Lerp(currentVelocity, Vector3.zero, dragDamping * Time.deltaTime);
        }

        // Update last stick position for next frame
        if (stickTarget != null)
        {
            lastStickPosition = stickTarget.position;
        }
    }

    void LateUpdate()
    {
        if (Mouse.current == null) return;

        // Get target position
        Vector3 targetPosition = stickTarget != null ? stickTarget.position : GetMouseWorldPosition();

        // Update both arms to point at and stretch to the target
        if (leftArmBone != null) 
            UpdateArm(leftArmBone, targetPosition, leftRotationOffset);

        if (rightArmBone != null) 
            UpdateArm(rightArmBone, targetPosition, rightRotationOffset);
    }

    private void CheckGroundContact()
    {
        if (stickTarget == null || stickCollider == null) return;

        // Determine layer mask (fallback to default layers if inspector mask is empty)
        int mask = groundLayer.value;
        if (mask == 0) mask = Physics.DefaultRaycastLayers;

        // Cast from slightly above the stick straight down (use world up/down to avoid local-rotation issues)
        Vector3 origin = stickTarget.position + Vector3.up * 0.1f;
        float castDistance = (stickCollider.height * 0.5f) + groundCheckDistance + 0.1f;

        RaycastHit hitInfo;
        isStickGrounded = Physics.Raycast(origin, Vector3.down, out hitInfo, castDistance, mask);

        // Save contact point when grounded
        if (isStickGrounded && !wasGroundedLastFrame)
        {
            lastGroundedPosition = hitInfo.point;
        }

        wasGroundedLastFrame = isStickGrounded;

        // Debug visualization
        if (showDebugRays)
        {
            Color rayColor = isStickGrounded ? Color.green : Color.red;
            Debug.DrawRay(origin, Vector3.down * castDistance, rayColor);
            if (isStickGrounded)
            {
                Debug.DrawLine(hitInfo.point, hitInfo.point + Vector3.up * 0.2f, Color.green);
            }
        }
    }

    private void ApplyGroundPushMovement()
    {
        if (stickTarget == null || characterController == null) return;

        // Calculate the vector from player to stick
        Vector3 playerToStick = stickTarget.position - transform.position;
        float distanceToStick = playerToStick.magnitude;

        // If player is within range and trying to move the grounded stick
        if (distanceToStick > minDragDistance)
        {
            // The player is far from the stick - pull them toward it
            ApplyDraggingMovement();
        }
        else
        {
            // Player is close to the stick
            // Check if the mouse is trying to push the stick away (lever action)
            Vector2 mousePixelPos = Mouse.current.position.ReadValue();
            Ray ray = mainCam.ScreenPointToRay(mousePixelPos);
            Plane wallPlane = new Plane(Vector3.right, transform.position);

            if (wallPlane.Raycast(ray, out float distance))
            {
                Vector3 mouseWorldPos = ray.GetPoint(distance);
                Vector3 stickToMouse = mouseWorldPos - stickTarget.position;
                
                // Calculate push direction (away from stick toward mouse direction)
                Vector3 pushDirection = stickToMouse.normalized;
                
                // Only push if mouse is pulling stick away from player (creating leverage)
                Vector3 playerToMouse = mouseWorldPos - transform.position;
                float leverageFactor = Vector3.Dot(pushDirection, playerToMouse.normalized);
                
                if (leverageFactor > 0.3f) // Mouse is on the far side, creating leverage
                {
                    // Calculate push based on how far mouse is trying to move the stick
                    float mousePullDistance = stickToMouse.magnitude;
                    
                    // Push player in the direction created by the lever action
                    Vector3 targetVelocity = pushDirection * groundPushForce * Mathf.Clamp01(mousePullDistance);
                    
                    currentVelocity = Vector3.Lerp(
                        currentVelocity,
                        targetVelocity,
                        dragAcceleration * Time.deltaTime
                    );

                    // Apply damping
                    currentVelocity *= (1f - dragDamping * 0.5f * Time.deltaTime);

                    // Move the character
                    characterController.Move(currentVelocity * Time.deltaTime);

                    // Debug visualization
                    if (showDebugRays)
                    {
                        Debug.DrawRay(transform.position, pushDirection * 2f, Color.blue);
                        Debug.DrawLine(stickTarget.position, mouseWorldPos, Color.white);
                    }
                }
                else
                {
                    // Not enough leverage - gradually stop
                    currentVelocity = Vector3.Lerp(currentVelocity, Vector3.zero, dragDamping * Time.deltaTime);
                }
            }
        }
    }

    private void ApplyDraggingMovement()
    {
        if (stickTarget == null || characterController == null) return;

        // Calculate direction from player to stick
        Vector3 directionToStick = stickTarget.position - transform.position;
        float distance = directionToStick.magnitude;

        // Only drag if stick is far enough away
        if (distance > minDragDistance)
        {
            // Calculate desired movement direction
            Vector3 dragDirection = directionToStick.normalized;
            
            // Accelerate velocity toward the stick
            Vector3 targetVelocity = dragDirection * dragSpeed;
            currentVelocity = Vector3.Lerp(
                currentVelocity, 
                targetVelocity, 
                dragAcceleration * Time.deltaTime
            );

            // Apply damping
            currentVelocity *= (1f - dragDamping * Time.deltaTime);

            // Move the character
            characterController.Move(currentVelocity * Time.deltaTime);

            // Debug visualization
            if (showDebugRays)
            {
                Debug.DrawRay(transform.position, dragDirection * distance, Color.magenta);
                Debug.DrawRay(transform.position, currentVelocity, Color.yellow);
            }
        }
        else
        {
            // Too close - gradually stop
            currentVelocity = Vector3.Lerp(currentVelocity, Vector3.zero, dragDamping * Time.deltaTime);
        }
    }

    private void UpdateStickTargetPosition()
    {
        Vector2 mousePixelPos = Mouse.current.position.ReadValue();
        Ray ray = mainCam.ScreenPointToRay(mousePixelPos);

        // Create a plane locked to the X-axis (for side-view 2D gameplay)
        Plane wallPlane = new Plane(Vector3.right, transform.position);

        if (wallPlane.Raycast(ray, out float distance))
        {
            Vector3 targetHitPoint = ray.GetPoint(distance);

            // Clamping logic
            if (stickTarget != null)
            {
                // 1. Determine the "Center" of the body/shoulders
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
                    targetHitPoint = anchorPoint + (directionToMouse.normalized * maxReach);
                }

                // Move stick directly - no physics simulation needed
                stickTarget.position = targetHitPoint;
            }

            if (showDebugRays)
            {
                Debug.DrawLine(ray.origin, targetHitPoint, Color.green);
                Debug.DrawRay(targetHitPoint, Vector3.up * 0.5f, Color.yellow);
            }
        }
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
            
            // Limit the stretch factor visually
            stretchFactor = Mathf.Clamp(stretchFactor, 0.5f, maxStretchMultiplier);

            arm.localScale = new Vector3(1, stretchFactor, 1);
        }
        else
        {
            arm.localScale = Vector3.one;
        }
    }

    // Optional: Visualize grounded state in editor
    private void OnDrawGizmos()
    {
        if (!showDebugRays || stickTarget == null) return;

        // Draw ground check sphere (use last contact point when available)
        Gizmos.color = isStickGrounded ? Color.green : Color.red;
        Vector3 checkPos;
        if (isStickGrounded)
        {
            checkPos = lastGroundedPosition;
        }
        else
        {
            checkPos = stickTarget.position - Vector3.up * (stickCollider != null ? stickCollider.height * 0.5f + 0.05f : 0.55f);
        }
        Gizmos.DrawWireSphere(checkPos, 0.12f);

        // Draw drag range
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, minDragDistance);
    }
}