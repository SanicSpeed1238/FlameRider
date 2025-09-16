using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Stats")]
    [Range(1,10)]
    [SerializeField] float steerSensitivity;
    [Range(1,10)]
    [SerializeField] float driftStrength;
    [Range(10,100)]
    [SerializeField] float accelerationRate;
    [Range(100,1000)]
    [SerializeField] float maxSpeed;
    [Range(10,100)]
    [SerializeField] float boostRate;
    [Range(1,50)]
    [SerializeField] float jumpHeight;

    // Variables Needed for Movement
    Rigidbody playerRB;
    float baseSpeed;
    float baseMaxSpeed;
    float driftDirection;

    // Input Variables
    float inputAccel; 
    float inputDrift;   
    float inputSteer;
    bool inputBoost;
    bool inputJump;

    // Important References
    PlayerAnimator playerAnimator;
    PlayerEffects playerVFX;

    enum ControllerState
    {
        Default,
        Boosting,
        Drifting,
        Jumping,
    }
    ControllerState state = ControllerState.Default;

    void Start()
    {
        playerRB = GetComponentInChildren<Rigidbody>();
        baseMaxSpeed = maxSpeed;

        playerAnimator = GetComponentInChildren<PlayerAnimator>();
        playerVFX = GetComponentInChildren<PlayerEffects>();
    }

    void FixedUpdate()
    {
        Accelerate();
        Boost();

        Steer();
        Drift();
        
        Jump();
    }

    void Accelerate()
    {
        float forwardInput = (state == ControllerState.Boosting) ? 1 : inputAccel;
        baseSpeed += forwardInput * accelerationRate * Time.fixedDeltaTime;
        if (baseSpeed > maxSpeed) baseSpeed = maxSpeed;

        Vector3 playerVelocity = baseSpeed * transform.forward;
        playerVelocity.y = playerRB.linearVelocity.y;
        playerRB.linearVelocity = playerVelocity;
    }

    void Boost()
    {
        if (inputBoost)
        {
            maxSpeed += boostRate * Time.fixedDeltaTime;

            if(state == ControllerState.Default)
            {
                playerAnimator.BoostAnimation(true);
                playerVFX.ActivateBoostEffect(true);
                state = ControllerState.Boosting;
            }
        }
        else
        {
            if (maxSpeed > baseMaxSpeed) maxSpeed -= (boostRate * 2f) * Time.fixedDeltaTime;
            else maxSpeed = baseMaxSpeed;

            if (state == ControllerState.Boosting)
            {
                playerAnimator.BoostAnimation(false);
                playerVFX.ActivateBoostEffect(false);
                state = ControllerState.Default;
            }
        }
    }

    void Steer()
    {
        if(inputDrift == 0f)
        {
            playerRB.MoveRotation(Quaternion.Euler(0f, inputSteer * steerSensitivity * (Time.fixedDeltaTime * 10f), 0f) * playerRB.rotation);
            playerAnimator.SteerAnimation(inputSteer);
        }
    }

    void Drift()
    {
        if (inputDrift != 0f && (inputAccel > 0 && inputSteer != 0))
        {
            if (state == ControllerState.Default)
            {
                driftDirection = (inputSteer > 0f) ? 1 : -1;
                playerAnimator.DriftAnimation(true, driftDirection);
                playerVFX.ActivateDriftSparks(true);
                state = ControllerState.Drifting;
            }

            if (state == ControllerState.Drifting)
            {
                float rotationInfluence = inputSteer * (inputSteer == driftDirection ? 2f : 1f);
                float rotationAmount = ((driftStrength * driftDirection) + rotationInfluence) * (Time.fixedDeltaTime * 10f);
                Quaternion newRotation = Quaternion.Euler(0f, rotationAmount, 0f) * playerRB.rotation;
                playerRB.MoveRotation(newRotation);
            }
        }
        else
        {
            if (state == ControllerState.Drifting)
            {
                playerAnimator.DriftAnimation(false, 0);
                playerVFX.ActivateDriftSparks(false);
                state = ControllerState.Default;
            }
        }
    }

    void Jump()
    {
        if (inputJump)
        {
            if (state == ControllerState.Default)
            {
                if (Physics.Raycast(transform.position, Vector3.down, 1f))
                {
                    playerRB.AddForce(jumpHeight * transform.up, ForceMode.Impulse);
                    playerAnimator.JumpAnimation(true);
                    state = ControllerState.Jumping;
                }
            }         
        }
        else
        {
            if (state == ControllerState.Jumping)
            {
                if (Physics.Raycast(transform.position, Vector3.down, 1f))
                {              
                    playerAnimator.JumpAnimation(false);
                    state = ControllerState.Default;
                }
            }
        }
    }

    #region Input Handling
    public void OnSteer(InputAction.CallbackContext stickInput)
    {
        inputSteer = stickInput.ReadValue<Vector2>().x;
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
    #endregion
}