using UnityEngine;

public class Player : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public CharacterController characterController;
    private Vector3 Velocity = Vector3.zero;
    private float verticalVelocity = 0f;
    private float gravity = -9.81f;
    void Start()
    {
        characterController = GetComponentInChildren<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        Gravity();
        Vector3 combined = Velocity + Vector3.up * verticalVelocity;
        characterController.Move(combined * Time.deltaTime);
    }

    private void Gravity()
    {

        if (characterController.isGrounded)
        {
            ;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }
    }

    public bool getGroundedState()
    {
        return characterController.isGrounded;
    }

}
