using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class StickMovementFinal : MonoBehaviour
{
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private GameObject theFuckingStick; // Better naming
    [SerializeField] private GameObject theEndOfTheFuckingStick; // Greater naming


    [SerializeField] private GameObject LeftArm;
    [SerializeField] private GameObject RightArm;

    [SerializeField] private GameObject PlayerBody; 
    [SerializeField] public float maxAllowedDistance = 1.5f;

    //[SerializeField] public float soemthig;
    private Camera mainCam;

    private Vector3 offset = new Vector3(63,0,0);

    //States
    enum StickState
    {
        FREE,
        LOCKED
    }
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
        if(!isTheFuckingStickFar())
            theFuckingStick.transform.position = new Vector3(0, mousePos.y, mousePos.z);
        Debug.Log(isTheFuckingStickFar());
        UpdateArm(LeftArm.transform, GetMouseWorldPosition(), offset);
        UpdateArm(RightArm.transform, GetMouseWorldPosition(), offset);
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

    bool isTheFuckingStickFar()
    {
        float distanceBetweenPlayerAndMouse = Vector3.Distance(GetMouseWorldPosition(), LeftArm.transform.position);
        if(distanceBetweenPlayerAndMouse > maxAllowedDistance)
        {
            return true;
        }
        else return false;
    }
    private void UpdateArm(Transform arm, Vector3 targetPos, Vector3 offset)
    {
        Vector3 direction = targetPos - arm.position;
        

        float angle = Mathf.Atan2(direction.y, direction.z) * Mathf.Rad2Deg;
        float finalX = -angle + offset.x;
        arm.localRotation = Quaternion.Euler(finalX, offset.y, offset.z);

            float dist = direction.magnitude;
            float stretchFactor = dist / 0.75f;
            
            // Limit the stretch factor visually
            stretchFactor = Mathf.Clamp(stretchFactor, 0.5f, 2); //Max Stretch Multiplier at the end

            arm.localScale = new Vector3(1, stretchFactor, 1);
    }
}
