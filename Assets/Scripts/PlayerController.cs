using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Stats")]
    [Range(1,100)]
    [SerializeField] float accelerationRate;
    [Range(100,1000)]
    [SerializeField] float maxBaseSpeed;
    [Range(1,10)]
    [SerializeField] float steerSensitivity;

    Rigidbody playerRB;
    float currentSpeed;

    float inputAccel;
    float inputBoost;
    float inputDrift;
    float inputJump;
    float inputSteer;

    void Start()
    {
        playerRB = GetComponentInChildren<Rigidbody>();
    }

    void FixedUpdate()
    {
        Steer();
        Accelerate();     
    }

    void Accelerate()
    {
        currentSpeed += inputAccel * accelerationRate * Time.fixedDeltaTime;
        if (currentSpeed > maxBaseSpeed) currentSpeed = maxBaseSpeed;

        playerRB.linearVelocity = transform.forward * currentSpeed;
    }

    void Steer()
    {
        playerRB.MoveRotation(Quaternion.Euler(0f, inputSteer * steerSensitivity, 0f) * playerRB.rotation);
    }

    public void OnAccelerate(InputAction.CallbackContext buttonInput)
    {
        inputAccel = buttonInput.ReadValue<float>();
        
    }
    public void OnBoost(InputAction.CallbackContext buttonInput)
    {
        inputBoost = buttonInput.ReadValue<float>();
    }
    public void OnDrift(InputAction.CallbackContext buttonInput)
    {
        inputDrift = buttonInput.ReadValue<float>();

    }
    public void OnJump(InputAction.CallbackContext buttonInput)
    {
        inputJump = buttonInput.ReadValue<float>();
    }
    public void OnSteer(InputAction.CallbackContext stickInput)
    {
        inputSteer = stickInput.ReadValue<Vector2>().x;
    }
}