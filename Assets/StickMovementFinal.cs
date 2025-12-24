using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class StickMovementFinal : MonoBehaviour
{

    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private GameObject TheStick;
    [SerializeField] private GameObject theEndOfTheFuckingStick;


    [SerializeField] private GameObject LeftArm;
    [SerializeField] private GameObject RightArm;

    [SerializeField] private GameObject PlayerBody;
    [SerializeField] public float maxAllowedDistance = 0.5f;
    [SerializeField] public GameObject EndOfArm;
    [SerializeField] Player player;
    [SerializeField] TextMeshProUGUI textMesh;
    [SerializeField] private CapsuleCollider stickCollider;
    //[SerializeField] public float soemthig;
    private Camera mainCam;

    private Vector3 armOffset = new Vector3(63, 0, 0);
    [SerializeField] public Vector3 StickOffset;
    private Mouse mouse;
    bool canMoveStick = true;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        int mask = groundLayer.value;
        mainCam = Camera.main;
        mouse = Mouse.current;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mousePos = GetMouseWorldPosition();
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();
        textMesh.text = $"Mouse Delta: {mouseDelta}";


        if (mouse.leftButton.isPressed && CheckForGround())
        {
            canMoveStick = false;
            PlayerBody.transform.position += new Vector3(0, mouseDelta.y, mouseDelta.x) * -0.01f;
            UpdateArms(GetMouseWorldPosition());
        }
        else
        {
            canMoveStick = true;
        }

        if (canMoveStick)
        {
            TheStick.transform.position = EndOfArm.transform.position;// - StickOffset;
            UpdateArms(GetMouseWorldPosition());
        }
    }

    private void UpdateArms(Vector3 target)
    {
        UpdateArm(LeftArm.transform, target, armOffset);
        UpdateArm(RightArm.transform, target, armOffset);
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
        stretchFactor = Mathf.Clamp(stretchFactor, 0.1f, maxAllowedDistance); //Max Stretch Multiplier at the end

        arm.localScale = new Vector3(1, stretchFactor, 1);
    }

    private bool CheckForGround()
    {
        return Physics.CheckCapsule(TheStick.transform.position, theEndOfTheFuckingStick.transform.position + new Vector3(0, 0.01f, 0), 0.0005f, layerMask: groundLayer);
    }

}
