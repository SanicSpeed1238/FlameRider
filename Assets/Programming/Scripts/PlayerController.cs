using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Speed")]
    [Range(100, 200)]
    [SerializeField] float baseSpeed;
    [Range(10, 100)]
    [SerializeField] float accelerationRate;
    [Range(10, 100)]
    [SerializeField] float decelerationRate;

    [Header("Handling")]
    [Range(1,10)]
    [SerializeField] float steerSensitivity;
    [Range(1,10)]
    [SerializeField] float driftStrength;

    [Header("Boost")]
    [Range(1,50)]
    [SerializeField] float boostRate;
    [Range(1,50)]
    [SerializeField] float energyRate;   

    [Header("Aerials")]
    [Range(10, 100)]
    [SerializeField] float jumpHeight;

    // Important References
    PlayerAnimator playerAnimator;
    PlayerEffects playerVFX;
    PlayerAudio playerSFX;
    Rigidbody playerRB;
    Transform currentCheckpoint;
    int passedCheckpoint;
    int lapsCompleted;

    // Variables for Speed    
    float currentSpeed;
    float maxSpeed;
    LayerMask groundLayer;
    readonly float groundLerp = 30f;

    // Variables for Handling
    float driftDirection;
    float currentDrift;

    // Variables for Boosting
    FlameTrailGeneration flameTrail;
    float currentFlameEnergy;
    bool onFlameTrail;
    float offFlameTrailTimer;
    readonly float offFlameTrailTime = 0.8f;

    // Variables for Aerials
    bool hasJumped;
    float jumpTimer;
    readonly float jumpTime = 0.2f;

    // Player States
    bool isBoosting;
    bool isDrifting;
    bool isGrounded;
    bool isFlying;
    bool isRespawning;
    bool isFinished;

    // Input Variables
    float inputAccel;
    float inputDrift;
    float inputSteer;
    bool inputBoost_Pressed;
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
        lapsCompleted = 0;
        passedCheckpoint = -1;
        currentCheckpoint = gameObject.transform;

        playerRB = GetComponent<Rigidbody>();
        maxSpeed = baseSpeed;
        if (driftStrength <= steerSensitivity) driftStrength = steerSensitivity + 0.1f;

        flameTrail = GetComponent<FlameTrailGeneration>();
        currentFlameEnergy = 50f;

        playerAnimator = GetComponentInChildren<PlayerAnimator>();
        playerVFX = GetComponentInChildren<PlayerEffects>();
        playerSFX = GetComponentInChildren<PlayerAudio>();

        groundLayer = LayerMask.GetMask("Ground");

        isFinished = false;
    }
    #endregion

    void Update()
    {
        if (isFinished) return;
        if (!CanMove()) return;

        CheckInputs();

        SteerAction();
        DriftAction();

        BoostAction();       
        JumpAction();
    }
    void FixedUpdate()
    {
        if (isFinished) return;

        CheckGrounded();

        if (!CanMove()) return;   

        AcceleratePhysics();
        SteerPhysics();

        BoostPhysics();    
        DriftPhysics();
     
        JumpPhysics();
    }

    #region Accelerate
    void AcceleratePhysics()
    {
        if (isFlying) return;

        // Accelerate or Decelerate based on input and/or conditions
        if (inputAccel > 0 || (isBoosting || onFlameTrail))
        {
            float forwardInput = (isBoosting || onFlameTrail) ? 1 : inputAccel;
            currentSpeed += forwardInput * ((currentSpeed < baseSpeed || maxSpeed > baseSpeed) ? accelerationRate : (accelerationRate * 0.01f)) * Time.fixedDeltaTime;
            if (currentSpeed > maxSpeed) currentSpeed = maxSpeed;
            playerSFX.StartSound(playerSFX.movingSound);
        }
        else
        {
            float decelerationInput = inputDrift + 0.1f;
            currentSpeed -= decelerationInput * decelerationRate * Time.fixedDeltaTime;
            if (currentSpeed < 0f) currentSpeed = 0f;
            playerSFX.StopSound(playerSFX.movingSound);
        }
        
        // Apply velocity to player rigidbody
        Vector3 playerVelocity = currentSpeed * playerRB.transform.forward;
        playerVelocity.y = playerRB.linearVelocity.y;
        playerRB.linearVelocity = playerVelocity;

        // Calculate real speed and display on UI
        float transformSpeed = Mathf.Abs(Vector3.Dot(playerRB.linearVelocity, playerRB.transform.forward));
        PlayerHUD.Instance.UpdateSpeedValue(transformSpeed, onFlameTrail);
        
        // Visualization effects of speed
        playerAnimator.SetSpeed(transformSpeed);
        playerVFX.SetMotionBlurIntensity(transformSpeed / 300f);
        
        // Naturally regen fire energy, auto boost if energy full
        if (inputAccel > 0 && !isBoosting) RegenerateFireEngery(0.5f);
        if (currentFlameEnergy == 100f && isGrounded) StartBoost();
    }
    #endregion

    #region Boost
    void BoostAction()
    {
        if (inputBoost_Pressed)
        {
            if (!isBoosting) StartBoost();
            else StopBoost();
        }
        if (isBoosting) playerVFX.ActivateFlameGenerate(true);
    }
    void BoostPhysics()
    {
        if (isBoosting && currentFlameEnergy > 0)
        {
            if (isGrounded) StartBoost();
            else StopBoost();

            maxSpeed += boostRate * Time.fixedDeltaTime;

            currentFlameEnergy -= Time.fixedDeltaTime * (energyRate * 1.5f);
            if (currentFlameEnergy <= 0f) StopBoost();
            PlayerHUD.Instance.UpdateFireEnergy(currentFlameEnergy);
        }
        else
        {
            if (!onFlameTrail)
            {
                if (maxSpeed > baseSpeed) maxSpeed -= (boostRate * 2f) * Time.fixedDeltaTime;
                else maxSpeed = baseSpeed;
            }          
        }

        if (offFlameTrailTimer < offFlameTrailTime)
        {
            offFlameTrailTimer += Time.fixedDeltaTime;
            if (offFlameTrailTimer >= offFlameTrailTime)
            {
                onFlameTrail = false;
                playerVFX.ActivateFlameLines(false);
                playerSFX.StopSound(playerSFX.boostingSound);
            }
        }
    }

    void StartBoost()
    {
        if (!flameTrail.IsGenerating() && currentFlameEnergy > 0)
        {
            isBoosting = true;
            flameTrail.StartBoostTrail();
            playerVFX.ActivateFlameGenerate(true);
            playerVFX.ActivateBoostEffect(true);
            playerSFX.StartSound(playerSFX.boostingSound);
        } 
    }
    void StopBoost()
    {
        if (flameTrail.IsGenerating())
        {
            isBoosting = false;
            flameTrail.StopBoostTrail();
            playerVFX.ActivateFlameGenerate(false);
            playerVFX.ActivateBoostEffect(false);
            playerSFX.StopSound(playerSFX.boostingSound);
        }
    }

    void RegenerateFireEngery(float multiplier)
    {
        currentFlameEnergy += Time.fixedDeltaTime * (energyRate * multiplier);
        if (currentFlameEnergy >= 100f) currentFlameEnergy = 100f;

        PlayerHUD.Instance.UpdateFireEnergy(currentFlameEnergy);
    }
    #endregion

    #region Steer
    void SteerAction()
    {
        // Update animator based on left stick input
        if(!isDrifting) playerAnimator.SteerAnimation(inputSteer);
    }
    void SteerPhysics()
    {
        // Rotate rigidbody based on left stick input
        playerRB.MoveRotation(Quaternion.Euler(0f, inputSteer * steerSensitivity * (Time.fixedDeltaTime * 10f), 0f) * playerRB.rotation);
    }
    #endregion

    #region Drift
    void DriftAction()
    {
        // Start or Stop drift based on input
        if (!isDrifting) StartDrift();
        else if (inputDrift == 0) StopDrift();
    }
    void DriftPhysics()
    {
        if (isDrifting)
        {
            // Gradually increase to the max drift strength
            if (currentDrift < driftStrength) currentDrift += (currentSpeed / 50f) * Time.fixedDeltaTime;
            else currentDrift = driftStrength;
            
            // Manipulate drift amount based on left stick direction
            float driftInfluence = inputSteer;
            if (driftDirection * inputSteer > 0f) driftInfluence *= Mathf.Clamp(currentSpeed / 100f, 1f, 5f);
            else driftInfluence = (currentDrift * -driftDirection) + (0.5f * driftDirection);

            // Rotate the rigidbody based on initial drift direction, current strength, and influence from left stick
            float rotationAmount = ((currentDrift * driftDirection) + driftInfluence) * (Time.fixedDeltaTime * 10f);
            Quaternion newRotation = Quaternion.Euler(0f, rotationAmount, 0f) * playerRB.rotation;
            playerRB.MoveRotation(newRotation);

            // Activate certain effects if grounded
            if (isGrounded)
            {
                RegenerateFireEngery(1f);
                playerSFX.StartSound(playerSFX.driftingSound);
            }
        }
    }
    void StartDrift()
    {
        // Sets the drift direction at time of input (cannot be changed mid-drift)
        if (inputDrift >= 0.5f)
        {
            driftDirection = inputSteer;

            if (isGrounded && (driftDirection > -0.5f && driftDirection < 0.5f)) driftDirection = 0;
            else if (driftDirection <= -.5f) driftDirection = -1;
            else if (driftDirection >= .5f) driftDirection = 1;
            else { return; }

            isDrifting = true;            
            currentDrift = 0.1f;

            playerAnimator.DriftAnimation(true, driftDirection);
            playerVFX.ActivateFlameTire(true);        
        }    
    }
    void StopDrift()
    {
        isDrifting = false;

        playerAnimator.DriftAnimation(false, driftDirection);
        playerVFX.ActivateFlameTire(false);
        playerSFX.StopSound(playerSFX.driftingSound);
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
                playerSFX.PlaySound(playerSFX.jumpSound);

                jumpTimer = 0f;
                hasJumped = true;
            }         
        }
    }
    void JumpPhysics()
    {
        if (hasJumped)
        {
            jumpTimer += Time.fixedDeltaTime;
            if (jumpTimer > jumpTime) hasJumped = false;
        }
    }
    #endregion

    #region Other Functions
    public void RideFlameTrail(float trailSpeedBoost)
    {
        if (CanMove())
        {
            onFlameTrail = true;
            offFlameTrailTimer = 0f;
            playerVFX.ActivateFlameLines(true);
            playerSFX.StartSound(playerSFX.boostingSound);
            maxSpeed += trailSpeedBoost * Time.fixedDeltaTime;
        }
    }
    public void UseFlameRing(float ringSpeedBoost, Transform ringTransform)
    {
        isFlying = true;
        playerRB.AddForce(ringSpeedBoost * ringTransform.forward, ForceMode.VelocityChange);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<FlameTrailObject>())
        {
            UseFlameRing(other.GetComponent<FlameTrailObject>().speedBoost, other.transform);
        }

        if (other.CompareTag("Checkpoint"))
        {
            currentCheckpoint = other.gameObject.transform;
            int checkpointIndex = Array.IndexOf(other.GetComponentInParent<TrackManager>().checkPoints, other.gameObject);

            if (checkpointIndex == 0 && passedCheckpoint >= other.GetComponentInParent<TrackManager>().checkPoints.Length - 15)
            {
                lapsCompleted++;
                PlayerHUD.Instance.UpdateLapNumber(lapsCompleted + 1);

                if (lapsCompleted == 3)
                {
                    GameState.Instance.WinGame();
                    PlayerHUD.Instance.UpdateLapNumber(3);

                    playerVFX.StopAllEffects();
                    playerSFX.StopAllAudio();
                    playerAnimator.SetGrounded(true);

                    GetComponent<BasicComputerPlayer>().SetAutoMove();
                    isFinished = true;
                }
                else
                {  
                    PlayerHUD.Instance.DisplayMessage("LAP " + (lapsCompleted + 1) + "!");
                    GameState.Instance.lapSound.PlayOneShot(GameState.Instance.lapSound.clip);
                }
            }

            else if (passedCheckpoint < checkpointIndex && passedCheckpoint >= checkpointIndex - 15)
            {
                passedCheckpoint = checkpointIndex;
            }
        }

        if (other.CompareTag("Deadzone"))
        {
            StartCoroutine(RespawnPlayer());
        }
    }

    void CheckGrounded()
    {
        Vector3 origin = transform.position;
        Vector3 direction = Vector3.down;
        float distance = 1f;

        //Debug.DrawRay(origin, direction * distance, Color.red);
        isGrounded = Physics.Raycast(origin, direction, distance) && !hasJumped;

        AlignToGround();
        playerAnimator.SetGrounded(isGrounded);
        if (isFlying && isGrounded) isFlying = false;
    }
    void AlignToGround()
    {
        Vector3 rayOrigin = playerRB.position + Vector3.up * 0.1f;
        if (Physics.Raycast(rayOrigin, -playerRB.transform.up, out RaycastHit hitInfo, 10f, groundLayer))
        {
            Quaternion groundRotation = Quaternion.FromToRotation(playerRB.transform.up, hitInfo.normal) * playerRB.rotation;
            Vector3 euler = groundRotation.eulerAngles;
            euler.y = playerRB.rotation.eulerAngles.y;

            Quaternion alignmentRotation = Quaternion.Euler(euler);
            Quaternion smoothedRotation = Quaternion.Slerp(playerRB.rotation, alignmentRotation, groundLerp * Time.deltaTime);
            playerRB.MoveRotation(smoothedRotation);
        }
        else
        {
            Quaternion uprightRotation = Quaternion.Euler(0f, playerRB.rotation.eulerAngles.y, 0f);
            Quaternion smoothedRotation = Quaternion.Slerp(playerRB.rotation, uprightRotation, groundLerp * Time.deltaTime);
            playerRB.MoveRotation(smoothedRotation);
        }
    }

    IEnumerator RespawnPlayer()
    {
        isRespawning = true;

        playerRB.position = currentCheckpoint.position;
        playerRB.rotation = currentCheckpoint.rotation;
        playerRB.linearVelocity = Vector3.down;
        currentSpeed = 0f;

        StopBoost();
        playerVFX.StopAllEffects();

        playerSFX.PlaySound(playerSFX.respawnSound);
        yield return new WaitForSeconds(1f);

        isRespawning = false;
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
    private void CheckInputs()
    {
        if (playerInput_Boost.WasPressedThisFrame()) inputBoost_Pressed = true;
        else inputBoost_Pressed = false;

        if (playerInput_Jump.WasPressedThisFrame()) inputJump_Pressed = true;
        else inputJump_Pressed = false;
    }
    private bool CanMove()
    {
        if (GameState.Instance.isPlaying && !isRespawning) return true;
        else return false;
    }
    #endregion
}