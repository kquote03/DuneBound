using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class NewMonoBehaviourScript : MonoBehaviour
{
    private Camera cam;
    [Header("Settings")]
    public float sensitivity = 0.5f;
    public float minAngle = -90f;
    public float maxAngle = 60f;

    // We track the rotation in this variable to avoid 0-360 wrap-around issues
    private float _currentZAngle;
    private Vector3 _initialRotation;
    void Start()
    {
        cam = Camera.main;
        Cursor.lockState = CursorLockMode.Locked;
        // 1. Store the initial X and Y so we don't mess them up
        _initialRotation = transform.localEulerAngles;

        // 2. Normalize the starting Z angle to be between -180 and 180
        //    (Unity usually returns 0-360, so 350 becomes -10)
        _currentZAngle = _initialRotation.z;
        if (_currentZAngle > 180) _currentZAngle -= 360;
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log($"Maus: {Mouse.current.delta.ReadValue()}"); 
        //transform.Rotate(new Vector3(0, 0, Mouse.current.delta.ReadValue().x) * Time.deltaTime * 10);
        //Debug.Log(transform.rotation.eulerAngles);

        float mouseDelta = Mouse.current.delta.x.ReadValue();

        // 3. Modify our private variable
        //    (Subtracting delta because dragging left usually means rotating counter-clockwise)
        _currentZAngle -= mouseDelta * sensitivity;

        // 4. Clamp the variable
        _currentZAngle = Mathf.Clamp(_currentZAngle, minAngle, maxAngle);

        // 5. Apply the rotation, keeping the original X and Y
        transform.localRotation = Quaternion.Euler(180, _initialRotation.y, _currentZAngle);
    }
}
