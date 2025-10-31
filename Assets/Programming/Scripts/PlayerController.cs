using System;
using System.Collections;
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
    [Range(1,50)]
    [SerializeField] float boostRate;
    [Range(1,50)]
    [SerializeField] float energyRate;
    [Range(1,50)]
    [SerializeField] float jumpHeight;

    // Important References
    PlayerAnimator playerAnimator;
    PlayerEffects playerVFX;
    PlayerAudio playerSFX;
    Transform currentCheckpoint;
    int passedCheckpoint;
    int lapsCompleted;

    // Variables for Movement
    Rigidbody playerRB;
    float baseSpeed;
    float baseMaxSpeed;
    float driftDirection;

    // Variables for Boosting
    FlameTrailGeneration flameTrailGen;
    float currentFlameEnergy;
    bool onFlameTrail;
    float offFlameTrailTimer;
    readonly float offFlameTrailTime = 0.8f;

    // Variables for Jumping
    bool hasJumped;
    float jumpTimer;
    readonly float jumpTime = 0.2f;

    // Player States
    bool isBoosting;
    bool isDrifting;
    bool isGrounded;
    bool isRespawning;

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
        baseMaxSpeed = maxSpeed;

        flameTrailGen = GetComponent<FlameTrailGeneration>();
        currentFlameEnergy = 50f;

        playerAnimator = GetComponentInChildren<PlayerAnimator>();
        playerVFX = GetComponentInChildren<PlayerEffects>();
        playerSFX = GetComponentInChildren<PlayerAudio>();
    }
    #endregion

    void Update()
    {
        if (!CanMove()) return;

        CheckInputs();

        SteerAction();
        DriftAction();

        BoostAction();       
        JumpAction();
    }
    void FixedUpdate()
    {
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
        if (inputAccel > 0 || (isBoosting || onFlameTrail))
        {
            float forwardInput = (isBoosting || onFlameTrail) ? 1 : inputAccel;
            baseSpeed += forwardInput * accelerationRate * Time.fixedDeltaTime;
            if (baseSpeed > maxSpeed) baseSpeed = maxSpeed;
            playerSFX.StartSound(playerSFX.movingSound);
        }
        else
        {
            float decelerationInput = inputDrift + 0.1f;
            baseSpeed -= decelerationInput * decelerationRate * Time.fixedDeltaTime;
            if (baseSpeed < 0f) baseSpeed = 0f;
            playerSFX.StopSound(playerSFX.movingSound);
        }

        Vector3 playerVelocity = baseSpeed * transform.forward;
        playerVelocity.y = playerRB.linearVelocity.y;
        playerRB.linearVelocity = playerVelocity;
        PlayerHUD.Instance.UpdateSpeedValue(Mathf.Abs(Vector3.Dot(playerRB.linearVelocity, transform.forward)));

        if (inputAccel > 0 && !isBoosting) RegenerateFireEngery(0.5f);
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
    }
    void BoostPhysics()
    {
        if (isBoosting && currentFlameEnergy > 0)
        {
            if (isGrounded) StartBoost();
            else StopBoost();

            maxSpeed += boostRate * Time.fixedDeltaTime;

            currentFlameEnergy -= Time.fixedDeltaTime * energyRate;
            if (currentFlameEnergy <= 0f) StopBoost();
            PlayerHUD.Instance.UpdateFireEnergy(currentFlameEnergy);
        }
        else
        {
            if (!onFlameTrail)
            {
                if (maxSpeed > baseMaxSpeed) maxSpeed -= (boostRate * 2f) * Time.fixedDeltaTime;
                else maxSpeed = baseMaxSpeed;
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
        if (!flameTrailGen.IsGenerating() && currentFlameEnergy > 0)
        {
            isBoosting = true;
            flameTrailGen.StartBoostTrail();
            playerVFX.ActivateFlameTire(true);
            playerVFX.ActivateBoostEffect(true);
            playerSFX.StartSound(playerSFX.boostingSound);
        } 
    }
    void StopBoost()
    {
        if (flameTrailGen.IsGenerating())
        {
            isBoosting = false;
            flameTrailGen.StopBoostTrail();
            playerVFX.ActivateFlameTire(false);
            playerVFX.ActivateBoostEffect(false);
            playerSFX.StopSound(playerSFX.boostingSound);
        }
    }

    void RegenerateFireEngery(float mult)
    {
        currentFlameEnergy += Time.fixedDeltaTime * (energyRate * mult);
        if (currentFlameEnergy >= 100f) currentFlameEnergy = 100f;
        PlayerHUD.Instance.UpdateFireEnergy(currentFlameEnergy);
    }
    #endregion

    #region Steer
    void SteerAction()
    {
        if(!isDrifting) playerAnimator.SteerAnimation(inputSteer);
    }
    void SteerPhysics()
    {
        playerRB.MoveRotation(Quaternion.Euler(0f, inputSteer * steerSensitivity * (Time.fixedDeltaTime * 10f), 0f) * playerRB.rotation);
    }
    #endregion

    #region Drift
    void DriftAction()
    {
        if (!isDrifting) StartDrift();
        else if (inputDrift == 0) StopDrift();

        if (isDrifting && isGrounded) playerSFX.StartSound(playerSFX.driftingSound);
        else playerSFX.StopSound(playerSFX.driftingSound);
    }
    void DriftPhysics()
    {
        if (isDrifting)
        {
            float rotationAmount = ((driftDirection * driftStrength) + inputSteer) * (Time.fixedDeltaTime * 10f);
            Quaternion newRotation = Quaternion.Euler(0f, rotationAmount, 0f) * playerRB.rotation;
            playerRB.MoveRotation(newRotation);

            if (isGrounded) RegenerateFireEngery(1f);
        }
    }
    void StartDrift()
    {
        if (inputDrift >= 0.5f)
        {
            isDrifting = true;

            driftDirection = inputSteer;
            if (driftDirection > -0.5f && driftDirection < 0.5f) driftDirection = 0;
            else if (driftDirection <= -.5f) driftDirection = -1;
            else if (driftDirection >= .5f) driftDirection = 1;

            playerAnimator.DriftAnimation(true, driftDirection);
        }    
    }
    void StopDrift()
    {
        isDrifting = false;

        playerAnimator.DriftAnimation(false, 0);
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
    void OnTriggerStay(Collider other)
    {
        if(other.CompareTag("Trail"))
        {
            onFlameTrail = true;
            offFlameTrailTimer = 0f;
            playerVFX.ActivateFlameLines(true);
            playerSFX.StartSound(playerSFX.boostingSound);
            maxSpeed += other.GetComponentInParent<FlameTrailObject>().speedBoost * Time.fixedDeltaTime;
        }    
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Checkpoint"))
        {
            currentCheckpoint = other.gameObject.transform;
            int checkpointIndex = Array.IndexOf(other.GetComponentInParent<TrackManager>().checkPoints, other.gameObject);

            if (checkpointIndex == 0 && passedCheckpoint >= other.GetComponentInParent<TrackManager>().checkPoints.Length - 15)
            {
                lapsCompleted++;
                PlayerHUD.Instance.UpdateLapNumber(lapsCompleted + 1);

                if (lapsCompleted == 3) { GameState.Instance.WinGame(); playerSFX.StopAllAudio(); }
                else { PlayerHUD.Instance.DisplayMessage("LAP " + (lapsCompleted + 1) + "!"); GameState.Instance.lapSound.PlayOneShot(GameState.Instance.lapSound.clip); }
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

        Debug.DrawRay(origin, direction * distance, Color.red);
        isGrounded = Physics.Raycast(origin, direction, distance) && !hasJumped;
        playerAnimator.SetGrounded(isGrounded);
    }

    IEnumerator RespawnPlayer()
    {
        isRespawning = true;

        playerRB.position = currentCheckpoint.position;
        playerRB.rotation = currentCheckpoint.rotation;
        playerRB.linearVelocity = Vector3.down;

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