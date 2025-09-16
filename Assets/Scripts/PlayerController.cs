using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Stats")]
    [Range(1, 10)]
    [SerializeField] float steerSensitivity;
    [Range(1, 10)]
    [SerializeField] float driftStrength;
    [Range(1,100)]
    [SerializeField] float accelerationRate;
    [Range(100,1000)]
    [SerializeField] float maxSpeed;
    [Range(10,100)]
    [SerializeField] float boostRate;
    [Range(1, 10)]
    [SerializeField] float jumpHeight;

    Rigidbody playerRB;
    float baseSpeed;
    float baseMaxSpeed;

    float inputAccel; 
    float inputDrift;   
    float inputSteer;
    bool inputBoost;
    bool inputJump;

    void Start()
    {
        playerRB = GetComponentInChildren<Rigidbody>();
        baseMaxSpeed = maxSpeed;
    }

    void FixedUpdate()
    {
        Steer();
        Drift();
        Accelerate();
        Boost();
        Jump();
    }

    void Accelerate()
    {
        baseSpeed += inputAccel * accelerationRate * Time.fixedDeltaTime;
        if (baseSpeed > maxSpeed) baseSpeed = maxSpeed;

        Vector3 playerVelocity = transform.forward * baseSpeed;
        playerVelocity.y = playerRB.linearVelocity.y;
        playerRB.linearVelocity = playerVelocity;
    }

    void Boost()
    {
        if (inputBoost)
        {
            maxSpeed += boostRate * Time.fixedDeltaTime;
        }
        else
        {
            if (maxSpeed > baseMaxSpeed) maxSpeed -= ((boostRate * 2f) * Time.fixedDeltaTime);
            else maxSpeed = baseMaxSpeed;
        }
    }

    void Jump()
    {
        if (inputJump)
        {
            if (Physics.Raycast(transform.position, Vector3.down, 1f))
            {
                playerRB.AddForce(jumpHeight * transform.up, ForceMode.Impulse);
            }
        }
    }

    void Steer()
    {
        if(inputDrift == 0f) playerRB.MoveRotation(Quaternion.Euler(0f, inputSteer * steerSensitivity, 0f) * playerRB.rotation);
    }

    void Drift()
    {
        if (inputDrift != 0f) Debug.Log("Drifting");
    }

    public void OnAccelerate(InputAction.CallbackContext floatInput)
    {
        inputAccel = floatInput.ReadValue<float>();
        
    }
    public void OnDrift(InputAction.CallbackContext floatInput)
    {
        inputDrift = floatInput.ReadValue<float>();

    }    
    public void OnBoost(InputAction.CallbackContext buttonInput)
    {
        inputBoost = buttonInput.ReadValueAsButton();
    }
    public void OnJump(InputAction.CallbackContext buttonInput)
    {
        inputJump = buttonInput.ReadValueAsButton();
    }
    public void OnSteer(InputAction.CallbackContext stickInput)
    {
        inputSteer = stickInput.ReadValue<Vector2>().x;
    }
}