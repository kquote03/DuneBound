using UnityEngine;
using UnityEngine.InputSystem;
using System;
using UnityEngine.UIElements;

public class StickMovementFinal : MonoBehaviour
{
    public enum StickState
    {
        FREE,
        LOCKED
    }


    [SerializeField] public StickState state = StickState.FREE;

    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private GameObject theFuckingStick; // Better naming
    [SerializeField] private GameObject theEndOfTheFuckingStick; // Greater naming


    [SerializeField] private GameObject LeftArm;
    [SerializeField] private GameObject RightArm;

    [SerializeField] private GameObject PlayerBody;
    [SerializeField] public float maxAllowedDistance = 1.5f;
    [SerializeField] public GameObject EndOfArm;
    [SerializeField] float maxRayDist = 0.2f;
    [SerializeField] Player player;

    //[SerializeField] public float soemthig;
    private Camera mainCam;

    private Vector3 armOffset = new Vector3(63, 0, 0);
    [SerializeField] public Vector3 StickOffset;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        int mask = groundLayer.value;
        mainCam = Camera.main;

    }

    // Update is called once per frame
    void Update()
    {

        Vector3 mousePos = GetMouseWorldPosition();
        IfMouseDirectionDownwards();

        if (CheckForGround())
        {
            state = StickState.LOCKED;
        }

        if (state == StickState.FREE)
        {
            theFuckingStick.transform.position = (EndOfArm.transform.position - StickOffset);
            Vector3 offset = transform.position - PlayerBody.transform.position;
            UpdateArm(LeftArm.transform, GetMouseWorldPosition(), armOffset);
            UpdateArm(RightArm.transform, GetMouseWorldPosition(), armOffset);
        }
        else if (state == StickState.LOCKED)
        {
            if (IfMouseDirectionUpwards())
            {
                // Give an upward bit to 'lodge' it out of the ground
                //transform.position = new Vector3(transform.position.x, transform.position.y+1, transform.position.z);
                state = StickState.FREE;
            }
            
            if (IfMouseDirectionDownwards())
            {
                PlayerBody.position = new Vector(PlayerBody.position.x,PlayerBody.position.y,PlayerBody.position.y);
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


        float angle = Mathf.Atan2(direction.y, direction.z) * Mathf.Rad2Deg;
        float finalX = -angle + offset.x;
        arm.localRotation = Quaternion.Euler(finalX, offset.y, offset.z);

        float dist = direction.magnitude;
        float stretchFactor = dist;

        // Limit the stretch factor visually
        stretchFactor = Mathf.Clamp(stretchFactor, 0.5f, 2); //Max Stretch Multiplier at the end

        arm.localScale = new Vector3(1, stretchFactor, 1);
    }

    public bool CheckForGround()
    {
        RaycastHit rayInfo;


        bool GroundTouch = transform.GetComponentInChildren<CapsuleCollider>().Raycast(new Ray(transform.position, Vector3.down), out rayInfo, maxRayDist);
        Debug.Log(GroundTouch);

        return GroundTouch;
    }

    public bool IfMouseDirectionUpwards()
{
    if (Mouse.current != null)
    {
        Vector2 delta = Mouse.current.delta.ReadValue();
        
        // Change < 0 to > 0 for upward movement
        if (delta.y > 0)
        {
            Debug.Log("Upwards");
            return true;
        }
    }
    
    return false;
}

public bool IfMouseDirectionDownwards()
    {
        if(Mouse.current != null)
        {
            Vector2 delta = Mouse.current.delta.ReadValue();
            if(delta.y < 0)
            {
                Debug.Log("Downwards");
                return true;
            }
        } else
        {
            return false;
        }

        return false;
    }

}
