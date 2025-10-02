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
    [Range(10, 100)]
    [SerializeField] float decelerationRate;
    [Range(100,1000)]
    [SerializeField] float maxSpeed;
    [Range(10,100)]
    [SerializeField] float boostRate;
    [Range(1,50)]
    [SerializeField] float jumpHeight;

    // Important References
    PlayerAnimator playerAnimator;
    PlayerEffects playerVFX;

    // Variables for Movement
    Rigidbody playerRB;
    float baseSpeed;
    float baseMaxSpeed;
    float driftDirection;

    // Variables for Boosting
    FlameTrailGeneration flameTrailGen;

    // Variables for Jumping
    bool hasJumped;
    float jumpTimer;
    readonly float jumpTime = 0.2f;

    // Player States
    bool isBoosting;
    bool isDrifting;
    bool isJumping;
    bool isGrounded;

    // Input Variables
    float inputAccel;
    float inputDrift;
    float inputSteer;
    bool inputBoost_Pressed;
    bool inputBoost_Held;
    bool inputBoost_Released;
    bool inputJump_Pressed;
    InputAction playerInput_Boost;
    InputAction playerInput_Jump;

    #region Initialization
    void Awake()
    {
        PlayerInput playerInput = GetComponent<PlayerInput>();
        playerInput_Boost = playerInput.actions["Boost"];
        playerInput_Jump = playerInput.actions["Jump"];       
    }
    void Start()
    {
        playerRB = GetComponent<Rigidbody>();
        baseMaxSpeed = maxSpeed;

        flameTrailGen = GetComponent<FlameTrailGeneration>();

        playerAnimator = GetComponentInChildren<PlayerAnimator>();
        playerVFX = GetComponentInChildren<PlayerEffects>();
    }
    #endregion

    void Update()
    {
        CheckInputs();

        SteerAction();
        DriftAction();

        BoostAction();       
        JumpAction();
    }
    void FixedUpdate()
    {
        CheckGrounded();

        AcceleratePhysics();
        SteerPhysics();

        BoostPhysics();    
        DriftPhysics();
     
        JumpPhysics();
    }

    #region Accelerate
    void AcceleratePhysics()
    {
        if (inputAccel > 0 || isBoosting)
        {
            float forwardInput = (isBoosting) ? 1 : inputAccel;
            baseSpeed += forwardInput * accelerationRate * Time.fixedDeltaTime;
            if (baseSpeed > maxSpeed) baseSpeed = maxSpeed;
        }
        else
        {
            float decelerationInput = inputDrift + 0.1f;
            baseSpeed -= decelerationInput * decelerationRate * Time.fixedDeltaTime;
            if (baseSpeed < 0f) baseSpeed = 0f;
        }

        Vector3 playerVelocity = baseSpeed * transform.forward;
        playerVelocity.y = playerRB.linearVelocity.y;
        playerRB.linearVelocity = playerVelocity;
    }
    #endregion

    #region Boost
    void BoostAction()
    {
        if (inputBoost_Pressed) StartBoost();
        if (inputBoost_Released) StopBoost();

        playerAnimator.BoostAnimation(isBoosting && isGrounded);
    }
    void BoostPhysics()
    {
        isBoosting = inputBoost_Held;

        if (isBoosting)
        {
            if (isGrounded) StartBoost();
            else StopBoost();

            maxSpeed += boostRate * Time.fixedDeltaTime;
        }
        else
        {
            if (maxSpeed > baseMaxSpeed) maxSpeed -= (boostRate * 2f) * Time.fixedDeltaTime;
            else maxSpeed = baseMaxSpeed;
        }
    }

    void StartBoost()
    {
        if (!flameTrailGen.IsGenerating())
        {
            flameTrailGen.StartBoostTrail();
            playerVFX.ActivateBoostEffect(true);
        } 
    }
    void StopBoost()
    {
        if (flameTrailGen.IsGenerating())
        {
            flameTrailGen.StopBoostTrail();
            playerVFX.ActivateBoostEffect(false);
        }
    }
    #endregion

    #region Steer
    void SteerAction()
    {
        playerAnimator.SteerAnimation(inputSteer);
    }
    void SteerPhysics()
    {
        playerRB.MoveRotation(Quaternion.Euler(0f, inputSteer * steerSensitivity * (Time.fixedDeltaTime * 10f), 0f) * playerRB.rotation);
    }
    #endregion

    #region Drift
    void DriftAction()
    {
        playerAnimator.DriftAnimation(isDrifting, driftDirection);
        playerVFX.ActivateDriftSparks(isDrifting);
    }
    void DriftPhysics()
    {
        if (inputDrift != 0f && (inputAccel > 0 && inputSteer != 0))
        {
            if (!isBoosting)
            {
                driftDirection = (inputSteer > 0f) ? 1 : -1;
                isDrifting = true;
            }

            if (isDrifting)
            {
                float rotationInfluence = inputSteer * (inputSteer == driftDirection ? 2f : 1f);
                float rotationAmount = ((driftStrength * driftDirection) + rotationInfluence) * (Time.fixedDeltaTime * 10f);
                Quaternion newRotation = Quaternion.Euler(0f, rotationAmount, 0f) * playerRB.rotation;
                playerRB.MoveRotation(newRotation);
            }
        }
        else isDrifting = false;
    }
    #endregion

    #region Jump
    void JumpAction()
    {
        if (inputJump_Pressed)
        {
            if (isGrounded)
            {
                playerRB.AddForce(jumpHeight * transform.up, ForceMode.Impulse);
                playerAnimator.JumpAnimation(true);

                jumpTimer = 0f;
                hasJumped = true;
                isJumping = true;
            }         
        }
        playerAnimator.JumpAnimation(isJumping);
    }
    void JumpPhysics()
    {
        if (hasJumped)
        {
            jumpTimer += Time.fixedDeltaTime;
            if (jumpTimer > jumpTime) hasJumped = false;
        }
        else if (isGrounded) isJumping = false;
    }
    #endregion

    #region Other Functions
    void OnTriggerStay(Collider other)
    {
        if(other.CompareTag("Trail"))
        {
            Debug.Log("BOOST!!!!  " + Time.time);
        }
    }

    void CheckGrounded()
    {
        Vector3 origin = transform.position;
        Vector3 direction = Vector3.down;
        float distance = 1f;

        Debug.DrawRay(origin, direction * distance, Color.red);
        isGrounded = Physics.Raycast(origin, direction, distance) && !hasJumped;
    }
    #endregion

    #region Input Handling
    public void InputSteer(InputAction.CallbackContext stickInput)
    {
        inputSteer = stickInput.ReadValue<Vector2>().x;
    }
    public void InputDrift(InputAction.CallbackContext floatInput)
    {
        inputDrift = floatInput.ReadValue<float>();

    }
    public void InputAccelerate(InputAction.CallbackContext floatInput)
    {
        inputAccel = floatInput.ReadValue<float>();
        
    }   
    public void InputBoost(InputAction.CallbackContext buttonInput)
    {
        inputBoost_Held = buttonInput.ReadValueAsButton();
    }
    private void CheckInputs()
    {
        if (playerInput_Boost.WasPressedThisFrame()) inputBoost_Pressed = true;
        else inputBoost_Pressed = false;
        if (playerInput_Boost.WasReleasedThisFrame()) inputBoost_Released = true;
        else inputBoost_Released = false;

        if (playerInput_Jump.WasPressedThisFrame()) inputJump_Pressed = true;
        else inputJump_Pressed = false;
    }
    #endregion
}