using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerStateMachine : MonoBehaviour
{
    public enum PlayerState
    {
        Idle,
        Running,
        OnAir,
        Healing,
        AttackPunch,
        AttackTail,
        SpecialAttackPunch,
        BeingHit,
        Death,
        Swinging,
        Block,
        Climbing,
        SpecialAttackStaff,
    }

    [Header("Movment")]
    public float speed = 5f;
    public float jumpForce = 15f;
    [SerializeField] private LayerMask groundLayer;
    private float groundCheckDelay = 0.1f;
    private float lastJumpTime = 0f;
    public bool facingRight = true; //esta mirant a la dreta (default)
    public bool canFlip = true;

    [Header("Control Modifiers")]
    public bool invertedControls = false;


    [Header("Ki System")]
    public float maxKi = 100f;
    public float currentKi;

    [Header("Ki Costs")]
    public float specialAttackPunchCost = 50f;
    public float specialAttackStaffCost = 1f;
    public float healingKiCostPerSecond = 10f;


    [Header("Ki Regeneration")]
    public float kiPerEnemyKilled = 20f;

    [Header("Stats")]
    public Rigidbody2D rb;
    [SerializeField] private bool isGrounded = true;
    [SerializeField] private bool isHealing = false;
    public bool hasStaff = false;
    public bool isDead = false; 
    public bool isBlocking => currentState == PlayerState.Block;
    public bool isComingFromClimbing = false;
    public bool wakeUpFromSleep = false;

    [Header("Refs")]
    public Animator animator;
    [SerializeField] private PlayerStaffController staffController;
    public GameObject punchDamageCollider;
    public GameObject tailDamageCollider;
    public GameObject staffObj;
    [SerializeField] private CameraShake cameraShake;
    [SerializeField] private GameObject earthquakePrefab;
    [SerializeField] private Transform earthquakeSpawnPoint;
    public CharacterHealth characterHealth;
    public Transform lastCheckPoint;
    public FirstSequence firstSequence;
    public bool isPlayerOnGorilaBossZone = false; 
    public bool isPlayerOnMonjeBossZone = false;
    public GameObject colliderAttackStaff;
    public ParticleSystem staffImpactEffect;


    [Header("Jump tuning")]
    [SerializeField] private float fallMultiplier = 2.5f;
    [SerializeField] private float lowJumpMultiplier = 2f;

    [Header("Swing")]
    [SerializeField] private LayerMask vineLayer;
    [SerializeField] private float vineCheckRadius = 1.5f;
    [SerializeField] private Transform vineCheckPoint;
    private bool nearVine = false;
    private Collider2D cachedVineCollider = null;
    private HingeJoint2D currentVineJoint;

    [Header("Vine Settings")]
    public float maxVineDistance = 3.5f; //Distancia máxima per agafar una liana
    public float vineAttachSpeedDamping = 0.7f; //Reducció de velocitat al agafar la liana
    public float maxSwingSpeed = 12f; //Velocitat máxima de balanceig
    public float swingDrag = 0.5f; //Resistència al balanceig

    [Header("Dialogue")]
    [SerializeField] public bool dialogueLocked = false; //true mentre el diàleg està actiu

    [Header("Slope Handling")]
    [SerializeField] private float maxSlopeAngle = 45f;
    private float slopeAngle;
    private Vector2 slopeNormal;
    public bool onSlope;

    [Header("Wall Check")]
    [SerializeField] private float wallCheckDistance = 0.6f;
    [SerializeField] private bool isAgainstWall;
    public GameObject wallCheckPosition;

    [Header("Slope Step")]
    [SerializeField] private float stepHeight = 0.25f;
    [SerializeField] private float stepCheckDistance = 0.1f;
    public Transform lowerOrigin;
    public Transform upperOrigin;

    [Header("Particles")]
    [SerializeField] private GameObject touchGroundParticlePrefab;
    private Vector2 lastGroundPoint;
    public ParticleSystem dizzyPS;
    [SerializeField] private GameObject healingAura;

    [Header("Attack Cooldowns")]
    public float attackCooldown = 0.1f;
    private float lastAttackTime = 0f;


    private float defaultGravity = 2f;
    private float currentGravity = 2f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip runSound;  
    public AudioClip jumpSound;
    public AudioClip landSound;
    public AudioClip healSound;
    public AudioClip punchAttackSound;
    public AudioClip tailAttackSound;
    public AudioClip specialAttackSound;
    public AudioClip staffClimbSound;
    public AudioClip blockSound;
    public AudioClip deathSound;
    public AudioClip hurtSound;
    public AudioClip specialAttackStaff;
    public AudioClip swingSound;

    [Header("New Input System")]
    private InputSystem_Actions inputActions;
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction attackPunchAction;
    private InputAction attackTailAction;
    private InputAction healAction;
    private InputAction blockAction;
    private InputAction specialAttackPunchAction;
    private InputAction swingAction;
    private InputAction staffClimbAction;
    private InputAction specialAttackStaffAction;

    private Vector2 moveInput;
    private bool jumpPressed;
    private bool jumpReleased;
    private bool attackPunchPressed;
    private bool attackTailPressed;
    private bool healPressed;
    private bool healReleased;
    private bool blockPressed;
    private bool blockReleased;
    private bool specialAttackPunchPressed;
    private bool swingPressed;
    private bool swingReleased;
    private bool staffClimbPressed;
    private bool staffClimbReleased;
    private bool specialAttackStaffPressed;


    public PlayerState currentState;

  
    public event System.Action<float> OnKiChanged;  

    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        SetGravity(2f);
        animator = GetComponent<Animator>();

        inputActions = new InputSystem_Actions();
        moveAction = inputActions.Player.Move;
        jumpAction = inputActions.Player.Jump;
        attackPunchAction = inputActions.Player.AttackPunch;
        attackTailAction = inputActions.Player.AttackTail;
        healAction = inputActions.Player.Heal;
        blockAction = inputActions.Player.Block;
        specialAttackPunchAction = inputActions.Player.SpecialAttackPunch;
        swingAction = inputActions.Player.Swing;
        staffClimbAction = inputActions.Player.Climb;
        specialAttackStaffAction = inputActions.Player.SpecialAttackStaff;

        punchDamageCollider.SetActive(false); //desactivem el collider de dany al iniciar
        tailDamageCollider.SetActive(false); //desactivem el collider de dany al iniciar
        colliderAttackStaff.SetActive(false); //desactivem el collider de dany del basto al iniciar
        if (!hasStaff) { staffObj.SetActive(false); }

    if (healingAura != null)
        {
            healingAura.SetActive(false);
        }
        currentKi = maxKi;
        OnKiChanged?.Invoke(currentKi);
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();

        jumpAction.performed += OnJumpPerformed;
        jumpAction.canceled += OnJumpCanceled;

        attackPunchAction.performed += OnAttackPunchPerformed;
        attackTailAction.performed += OnAttackTailPerformed;

        healAction.performed += OnHealPerformed;
        healAction.canceled += OnHealCanceled;

        blockAction.performed += OnBlockPerformed;
        blockAction.canceled += OnBlockCanceled;

        specialAttackPunchAction.performed += OnSpecialAttackPunchPerformed;

        swingAction.performed += OnSwingPerformed;
        swingAction.canceled += OnSwingCanceled;

        staffClimbAction.performed += OnStaffClimbPerformed;
        staffClimbAction.canceled += OnStaffClimbCanceled;
        specialAttackStaffAction.performed += OnSpecialAttackStaffPerformed;

        CombatEvents.OnEnemyKilled += OnEnemyKilled;
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();

        jumpAction.performed -= OnJumpPerformed;
        jumpAction.canceled -= OnJumpCanceled;

        attackPunchAction.performed -= OnAttackPunchPerformed;
        attackTailAction.performed -= OnAttackTailPerformed;

        healAction.performed -= OnHealPerformed;
        healAction.canceled -= OnHealCanceled;

        specialAttackPunchAction.performed -= OnSpecialAttackPunchPerformed;

        swingAction.performed -= OnSwingPerformed;
        swingAction.canceled -= OnSwingCanceled;

        if (hasStaff)
        {
            blockAction.performed -= OnBlockPerformed;
            blockAction.canceled -= OnBlockCanceled;
            staffClimbAction.performed -= OnStaffClimbPerformed;
            staffClimbAction.canceled -= OnStaffClimbCanceled;
            specialAttackStaffAction.performed -= OnSpecialAttackStaffPerformed;
        }

        CombatEvents.OnEnemyKilled -= OnEnemyKilled;
    }


    private void Start()
    {
        currentState = PlayerState.Idle;

        CombatEvents.OnEnemyKilled += OnEnemyKilled;
    }

    private void OnDestroy()
    {
        CombatEvents.OnEnemyKilled -= OnEnemyKilled;
    }

    private void Update()
    {
        if (isDead) { return; } //si estem morts, no fem res

        if (!dialogueLocked)
        {
            Vector2 rawInput = moveAction.ReadValue<Vector2>();

            float deadzone = 0.25f;

            if (Mathf.Abs(rawInput.x) < deadzone)
            {
                moveInput.x = 0f;
            }

            if (Mathf.Abs(rawInput.y) < deadzone)
            {
                rawInput.y = 0f;
            }

            moveInput = rawInput;

            if (invertedControls)
            {
                moveInput.x = -moveInput.x;
            }
           
        }
        else
        {
            moveInput = Vector2.zero;
        }

        animator.SetBool("isGrounded", isGrounded); //important per les transicions cap a OnAir i Idle/Running
        animator.SetFloat("speed", Mathf.Abs(moveInput.x)); //important per les transicions cap a Running i Idle
        animator.SetFloat("verticalVelocity", rb.linearVelocity.y); //important per el blend tree de saltar

        CheckIfNearVine(); //Funcio que comprova si estem a prop d'una liana
        ApplyJumpMultiplier(); //Funcio que aplica el jump multiplier per fer saltos mes naturals
        HandleSwingInput(); //Funcio que comprova el input de liana AJUNTAR I FER UNA UNICA FUNCIO D'INPUTS?
        
        ProcessInputActions(); //Funcio que processa els inputs d'atac, curar, bloqueig, basto i atac especial

        switch (currentState)
        {
            case PlayerState.Idle:
                HandleIdle();
                break;

            case PlayerState.Running:
                HandleRunning();
                break;

            case PlayerState.OnAir:
                HandleJumping();
                break;

            case PlayerState.Healing:
                HandleHealing();
                break;

            case PlayerState.AttackTail:
                HandleAttackTail();
                break;

            case PlayerState.AttackPunch:
                HandleAttackPunch();
                break;

            case PlayerState.BeingHit:
                HandleBeingHit();
                break;

            case PlayerState.SpecialAttackPunch:
                HandleSpecialAttackPunch();
                break;

            case PlayerState.Death:
                HandleDeath();
                break;

            case PlayerState.Swinging:
                HandleSwinging();
                break;

            case PlayerState.Climbing:
                HandleClimbing();
                break;

            case PlayerState.Block:
                HandleBlock();
                break;

            case PlayerState.SpecialAttackStaff:
                HandleSpecialAttackStaff();
                break;

            default:
                currentState = PlayerState.Idle;
                break;
        }

        ResetInputFlags();

    }

    private void FixedUpdate()
    {
        if(dialogueLocked) //si el diàleg està actiu, no processem moviments
        {
            return;
        }

        HandleFlip(); //el posem aqui ja que ho mirem just despres del moveInput
        CheckIfGrounded();
        CheckWallCollision();

        if (onSlope && moveInput.x == 0 && isGrounded) 
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            SetGravity(0f);
        }
        else if (onSlope && moveInput.x != 0)
        {
            RestoreDefaultGravity();
        }

        if (currentState == PlayerState.Running || currentState == PlayerState.OnAir || currentState == PlayerState.AttackPunch || currentState == PlayerState.AttackTail) //podem moure'ns en aquests estats
        {
            Move();
            HandleSlopeStep();
        }

        if (currentState == PlayerState.Swinging)
        {
            ApplySwingPhysics();
        }
            
    }

    // ==================== CALLBACKS DE INPUTS ====================

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        if (dialogueLocked) return;
        jumpPressed = true;
    }

    private void OnJumpCanceled(InputAction.CallbackContext context)
    {
        jumpReleased = true;
    }

    private void OnAttackPunchPerformed(InputAction.CallbackContext context)
    {
        if(dialogueLocked) return;
        attackPunchPressed = true;
    }

    private void OnAttackTailPerformed(InputAction.CallbackContext context)
    {
        if (dialogueLocked) return;
        attackTailPressed = true;
    }

    private void OnHealPerformed(InputAction.CallbackContext context)
    {
        if (dialogueLocked) return;
        healPressed = true;
    }

    private void OnHealCanceled(InputAction.CallbackContext context)
    {
        healReleased = true;
    }

    private void OnBlockPerformed(InputAction.CallbackContext context)
    {
        if (dialogueLocked) return;
        blockPressed = true;
    }

    private void OnBlockCanceled(InputAction.CallbackContext context)
    {
        blockReleased = true;
    }

    private void OnSpecialAttackPunchPerformed(InputAction.CallbackContext context)
    {
        if (dialogueLocked) return;
        specialAttackPunchPressed = true;
    }

    private void OnSwingPerformed(InputAction.CallbackContext context)
    {
        if (dialogueLocked) return;
        swingPressed = true;
    }

    private void OnSwingCanceled(InputAction.CallbackContext context)
    {
        swingReleased = true;
    }

    private void OnStaffClimbPerformed(InputAction.CallbackContext context)
    {
        Debug.Log("Staff Climb Performed");
        if (dialogueLocked || !hasStaff) return;
        staffClimbPressed = true;
    }

    private void OnStaffClimbCanceled(InputAction.CallbackContext context)
    {
        staffClimbReleased = true;
    }

    private void OnSpecialAttackStaffPerformed(InputAction.CallbackContext context)
    {
        if (dialogueLocked || !hasStaff) return;
        specialAttackStaffPressed = true;
    }

    private void ResetInputFlags()
    {
        jumpPressed = false;
        jumpReleased = false;
        attackPunchPressed = false;
        attackTailPressed = false;
        healPressed = false;
        healReleased = false;
        blockPressed = false;
        blockReleased = false;
        specialAttackPunchPressed = false;
        swingPressed = false;
        swingReleased = false;
        staffClimbPressed = false;
        staffClimbReleased = false;
        specialAttackStaffPressed = false;
    }



    // ==================== KI SYSTEM ====================
    private void OnEnemyKilled(GameObject enemy)
    {
        // Regenerar Ki al matar un enemigo
        AddKi(kiPerEnemyKilled);
        Debug.Log($"¡Enemigo eliminado! +{kiPerEnemyKilled} Ki. Ki actual: {currentKi}/{maxKi}");
    }

    private void AddKi(float amount)
    {
        currentKi += amount;
        currentKi = Mathf.Clamp(currentKi, 0, maxKi);
        OnKiChanged?.Invoke(currentKi);
    }

    private bool TryConsumeKi(float amount)
    {
        if (currentKi >= amount)
        {
            currentKi -= amount;
            currentKi = Mathf.Clamp(currentKi, 0, maxKi);
            OnKiChanged?.Invoke(currentKi);
            return true;
        }
        return false;
    }

    private void ConsumeKiOverTime(float amountPerSecond)
    {
        if (currentKi > 0)
        {
            currentKi -= amountPerSecond * Time.deltaTime;
            currentKi = Mathf.Clamp(currentKi, 0, maxKi);
            OnKiChanged?.Invoke(currentKi);
        }
    }

    public bool HasEnoughKi(float amount)
    {
        return currentKi >= amount;
    }

    public void RestoreFullKi()
    {
        currentKi = maxKi;
        OnKiChanged?.Invoke(currentKi);
    }

    // ==================== PROCESS INPUT ACTIONS ====================

    private void ProcessInputActions()
    {
        //BLOQUEIG
        if (blockPressed && isGrounded && hasStaff) //si premem el boto de bloqueig
        {
            ChangeState(PlayerState.Block);
            animator.SetBool("Blocking", true);
            if (blockSound != null)
            {
                audioSource.Stop();
                audioSource.PlayOneShot(blockSound);
            }
        }
        if (blockReleased && currentState == PlayerState.Block && hasStaff) //si deixem de prémer el botó de bloqueig
        {
            animator.SetBool("Blocking", false);
            ReturnToDefaultState();
        }
        //HEAL
        if (healPressed && isGrounded && currentKi > 0)
        {
            isHealing = true; //revisar pq crec q es pot treure
            animator.SetBool("HealButton", true);
            ChangeState(PlayerState.Healing);
        }
       
        if (healReleased && currentState == PlayerState.Healing)
        {
            isHealing = false; //revisar pq crec q es pot treure
            animator.SetBool("HealButton", false);
        }

        //ATACS
        if (attackPunchPressed && Time.time >= lastAttackTime + attackCooldown) //si premem el boto d'atac de puny i estem en un estat que ho permet i ha passat el cooldown
        {
            if (currentState == PlayerState.Idle ||
                currentState == PlayerState.Running ||
                currentState == PlayerState.OnAir)
            {
                lastAttackTime = Time.time;
                canFlip = false; //evitem que es giri durant l'animacio d'atac
                ChangeState(PlayerState.AttackPunch);
                animator.SetTrigger("AttackPunch");
                if (punchAttackSound != null)
                {
                    audioSource.PlayOneShot(punchAttackSound);
                }
            }
        }

        if (attackTailPressed && Time.time >= lastAttackTime + attackCooldown) //si premem el boto d'atac de cua i estem en un estat que ho permet
        {
            if (currentState == PlayerState.Idle ||
                currentState == PlayerState.Running ||
                currentState == PlayerState.OnAir)
            {
                lastAttackTime = Time.time;
                canFlip = false; //evitem que es giri durant l'animacio d'atac
                ChangeState(PlayerState.AttackTail);
                animator.SetTrigger("AttackTail");
                if (tailAttackSound != null)
                {
                    audioSource.PlayOneShot(tailAttackSound);
                }
            }
        }

        if (specialAttackPunchPressed && Time.time >= lastAttackTime + attackCooldown)
        {
            if (currentState == PlayerState.Idle || currentState == PlayerState.Running)
            {
                if (TryConsumeKi(specialAttackPunchCost))
                {
                    lastAttackTime = Time.time;
                    canFlip = false; //evitem que es giri durant l'animacio d'atac
                    ChangeState(PlayerState.SpecialAttackPunch);
                    animator.SetTrigger("SpecialAttackPunch");
                }
            }
        }

        //BASTO
        if (staffClimbPressed && isGrounded && hasStaff && !onSlope) //si premem el boto dret del ratoli i estem a terra i tenim el basto i no estem a una pendent
        {
            staffController.ResetStaff();
            animator.SetTrigger("StaffClimbing");
        }
    }

    private void HandleIdle() 
    {
        if(dialogueLocked) 
        { 
            animator.SetFloat("speed", 0);
            animator.SetBool("isGrounded", true);
            isHealing = false;
            healingAura.SetActive(false);
            return; 
        }
        if (Mathf.Abs(moveInput.x) > 0.25f) //Si es mou
        {
            ChangeState(PlayerState.Running);
            return;
        }

        if (jumpPressed && isGrounded) //Si premem saltar i esta a terra
        {
            Jump();
            ChangeState(PlayerState.OnAir);
        }
    }

    private void HandleRunning()
    {
        if(runSound != null && !audioSource.isPlaying) //Si hi ha so de correr i no s'està reproduint
        {
            audioSource.clip = runSound;
            audioSource.loop = true;
            audioSource.Play();
        }

        if (Mathf.Abs(moveInput.x) < 0.25f) //Si no es mou
        {
            audioSource.Stop();
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); //Aturem el moviment horitzontal
            ChangeState(PlayerState.Idle); //Canviem a estat Idle
            return;
        }
        if (jumpPressed && isGrounded) //Si premem saltar i esta a terra
        {
            audioSource.Stop();
            Jump();
            isGrounded = false;
            ChangeState(PlayerState.OnAir);
        }
    }

    private void HandleJumping()
    {
        if (isGrounded)
        {
            SpawnTouchGroundParticle();
            if(landSound != null)
            {
                audioSource.PlayOneShot(landSound);
            }
            animator.SetTrigger("TouchGround"); //animacio d'aterrar
            ChangeState(Mathf.Abs(moveInput.x) > 0.1f ? PlayerState.Running : PlayerState.Idle); //Si es mou, a Running, si no a Idle
        }
        if(isComingFromClimbing && specialAttackStaffPressed && !isGrounded) //si ve de escalar i premem el atac especial del basto
        {
            if (TryConsumeKi(specialAttackStaffCost))
            {
                animator.SetTrigger("SpecialAttackStaff");
                audioSource.PlayOneShot(specialAttackStaff);
                rb.gravityScale = 2f;
                ChangeState(PlayerState.SpecialAttackStaff);
            }

            isComingFromClimbing = false;
            
        }
    }

    private void HandleHealing()
    {
        if (isHealing)
        {
            // Activar el aura si no está activa
            if (healingAura != null && !healingAura.activeSelf && dialogueLocked == false)
            {
                healingAura.SetActive(true);
            }

            if (currentKi > 0)
            {
                characterHealth.Heal(20f * Time.deltaTime);
                ConsumeKiOverTime(healingKiCostPerSecond);
                if (healSound != null && !audioSource.isPlaying)
                {
                    audioSource.clip = healSound;
                    audioSource.loop = true;
                    audioSource.Play();
                }
            }
            else
            {
                // Si se acaba el Ki, detener curación
                isHealing = false;
                animator.SetBool("HealButton", false);
                Debug.Log("¡Ki agotado! No puedes seguir curándote.");
                audioSource.Stop();
                
                // Desactivar el aura
                if (healingAura != null)
                {
                    healingAura.SetActive(false);
                }
            }
        }

        if (!isHealing)
        {
            // Desactivar el aura cuando dejamos de curar
            if (healingAura != null && healingAura.activeSelf)
            {
                healingAura.SetActive(false);
            }

            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.IsName("Idle"))
            {
                ChangeState(PlayerState.Idle);
            }
        }
    }

    private void HandleAttackPunch()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        if (!stateInfo.IsName("PunchAttack")) //Comprovem si estem a l'animacio d'atac i si ha acabat
        {
            ReturnToDefaultState(); //torna a l'estat per defecte segons si estem a terra o en l'aire
        }
    }

    private void HandleAttackTail()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0); //agafa la info de l'animacio actual

        if (!stateInfo.IsName("AttackTail")) //si estem a l'animacio d'atac i ha acabat
        {
            ReturnToDefaultState(); //torna a l'estat per defecte segons si estem a terra o en l'aire
        }
    }

    private void HandleBeingHit()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        if (!stateInfo.IsName("BeingHit"))
        {
            ReturnToDefaultState(); //torna a l'estat per defecte segons si estem a terra o en l'aire
        }
    }

    private void HandleSpecialAttackPunch()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        if (!stateInfo.IsName("SpecialAttackPunch"))
        {
            ReturnToDefaultState(); //torna a l'estat per defecte segons si estem a terra o en l'aire
        }
    }

    private void HandleDeath()
    {
        Debug.Log("Player has died.");
        rb.linearVelocity = Vector2.zero;
        isDead = true;
        //gameManager fa la resta per nosaltres
    }

    private void HandleSwinging()
    {
        if (currentVineJoint == null) return;

        if(swingSound != null && !audioSource.isPlaying)
        {
            audioSource.clip = swingSound;
            audioSource.loop = true;
            audioSource.Play();
        }

        if (Mathf.Abs(moveInput.x) > 0.1f) //si hi ha input per balancejar-se
        {
            Vector2 ropeDir = (transform.position - currentVineJoint.connectedBody.transform.position).normalized; //vector que va desde el punt de connexio de la liana fins al jugador

            Vector2 tangent = new Vector2(-ropeDir.y, ropeDir.x); //vector tangent a la liana (perpendicular al vector ropeDir)

            rb.AddForce(tangent * moveInput.x * 5f, ForceMode2D.Force); //apliquem una força en la direccio tangent per simular el balanceig
        }
    }

    private void HandleClimbing()
    {
        isComingFromClimbing = false;

        if (!staffController.touchingGround) //si no toca a terra el pal
        {
            staffController.ExtendDown(); //estirem la part del pal que va cap avall
        }
        else if (!staffController.reachedTop) //si ja toca a terra pero no ha arribat al maxim d'extensio cap amunt
        {
            staffController.ExtendUp();
            transform.position += Vector3.up * (staffController.extendSpeedUp * Time.deltaTime);
        }
        else
        {
            rb.linearVelocity = Vector2.zero; //si el pal esta completament estirat, parem el moviment del jugador
        }

        if (staffClimbReleased) //si deixem anar el boto dret del ratoli
        {
            staffController.ResetStaff(); //reiniciem el pal
            animator.SetTrigger("StopStaffClimbing"); //fa la animacio de treure el pal del terra
            RestoreDefaultGravity();
            ChangeState(PlayerState.OnAir); //anem a estat OnAir
        }

        if (jumpPressed) //si premem saltar mentre estem escalant
        {
            staffController.ResetStaff();
            animator.SetTrigger("StopStaffClimbing");
            RestoreDefaultGravity();
            Jump();
            isComingFromClimbing = true; //indiquem que venim de escalar per poder fer l'atac especial del basto en l'aire
            ChangeState(PlayerState.OnAir); //anem a estat OnAir
        }

        if (specialAttackStaffPressed)
        {
            if (TryConsumeKi(specialAttackStaffCost))
            {
                staffController.ResetStaff();
                animator.SetTrigger("SpecialAttackStaff"); 
                rb.gravityScale = 2f;
                ChangeState(PlayerState.SpecialAttackStaff);
            }
            else
            {
                Debug.Log("¡No tienes suficiente Ki para el ataque especial del bastón!");
            }
        }
    }

    private void HandleBlock()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); // detener movimiento horizontal
        if (!isGrounded) //si no estem a terra
        {
            animator.SetBool("Blocking", false);
            ReturnToDefaultState(); //torna a l'estat per defecte segons si estem a terra o en l'aire
            return;
        }
        if (blockReleased) //si deixem de prémer el botó de bloqueig
        {
            animator.SetBool("Blocking", false);
            ReturnToDefaultState(); //torna a l'estat per defecte segons si estem a terra o en l'aire
        }
    }

    private void HandleSpecialAttackStaff()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0); //agafa la info de l'animacio actual
        if (!stateInfo.IsName("SpecialAttackStaff")) //si estem a l'animacio d'atac especial del basto i ha acabat
        {
            if (Mathf.Abs(moveInput.x) > 0.1f && isGrounded) { ChangeState(PlayerState.Running); } //si estem a terra i ens movem anem a Running
            else if (isGrounded) { ChangeState(PlayerState.Idle); } //sino anem a Idle
        }
    }


    //--------------ANIMATION EVENTS--------------//

    public void OnPunchImpact() //Cridat des de l'animacio mitjancant un Animation Event
    {
        punchDamageCollider.SetActive(true); //activa el collider del puny per fer dany
        CombatEvents.PlayerAttack(); //notifiquem als subscrits que el jugador ha atacat
    }

    public void OnPunchImpactEnd() //Cridat des de l'animacio mitjancant un Animation Event
    {
        punchDamageCollider.SetActive(false); //desactiva el collider del puny per fer dany
    }

    public void OnTailImpact() //Cridat des de l'animacio mitjancant un Animation Event
    {
        tailDamageCollider.SetActive(true); //activa el collider de la cua per fer dany
        CombatEvents.PlayerAttack(); //notifiquem als subscrits que el jugador ha atacat
    }

    public void OnTailImpactEnd() //Cridat des de l'animacio mitjancant un Animation Event
    {
        tailDamageCollider.SetActive(false); //desactiva el collider de la cua per fer dany
    }

    public void OnSpecialPunchImpact() //Cridat des de l'animacio mitjancant un Animation Event
    {
        CombatEvents.PlayerAttack(); //notifiquem als subscrits que el jugador ha atacat
        audioSource.PlayOneShot(specialAttackSound);
        if (earthquakePrefab != null && earthquakeSpawnPoint != null)
        {
            GameObject wave = Object.Instantiate(
                earthquakePrefab,
                earthquakeSpawnPoint.position,
                Quaternion.identity
            );

            float sign = Mathf.Sign(transform.localScale.x);
            wave.transform.localScale = new Vector3(
                Mathf.Abs(wave.transform.localScale.x) * sign,
                wave.transform.localScale.y,
                wave.transform.localScale.z
            );

            cameraShake.Shake(3f, 3.5f, 1.2f); //sacsegem la camara en l'impacte del puny
        }
    }

    public void OnStaffClimbStart() //Cridat des de l'animacio mitjancant un Animation Event
    {
        SetGravity(0f);
        rb.linearVelocity = Vector2.zero;
        ChangeState(PlayerState.Climbing);
    }

    private bool hasImpacted = false;
    public float linearVelocityNeededForImpact = -20f;

    public void OnSpecialAttackStaffImpact() //Cridat des de l'animacio mitjancant un Animation Event
    {
        Debug.Log("Checking Staff Special Attack Impact...");
        if (rb.linearVelocity.y <= linearVelocityNeededForImpact)
        {
            hasImpacted = true;
            colliderAttackStaff.SetActive(true); //activa el collider de l'atac especial del basto per fer dany
            staffImpactEffect.Play();
            Debug.Log("Staff Special Attack Impact!");
        }
    }

    public void OnSpecialAttackStaffImpactEnd() //Cridat des de l'animacio mitjancant un Animation Event
    {
        Debug.Log("Ending Staff Special Attack Impact...");
        if (hasImpacted)
        {
            colliderAttackStaff.SetActive(false); //desactiva el collider de l'atac especial del basto per fer dany
            hasImpacted = false;
            Debug.Log("Staff Special Attack Impact End!");
        }
            
    }


    //--------------OTHER FUNCTIONS--------------//

    private void CheckIfNearVine() //Comprova si estem a prop d'una liana
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(vineCheckPoint.position, vineCheckRadius, vineLayer); //busquem totes les lianes a prop del vineCheckPoint dins del radi vineCheckRadius
        if (hits.Length > 0) //si hem trobat alguna liana
        {
            float minDist = float.MaxValue; //inicialitzem la distancia minima a un valor molt alt
            Collider2D closest = null; //inicialitzem la liana mes propera a null

            foreach (var hit in hits) //per cada liana trobada
            {
                if (hit.attachedRigidbody == null) continue;

                float dist = Vector2.Distance(vineCheckPoint.position, hit.transform.position); //calculem la distancia entre el vineCheckPoint i la liana
                if (dist < minDist) //si la distancia es menor que la distancia minima actual
                {
                    minDist = dist; //actualitzem la distancia minima
                    closest = hit; //actualitzem la liana mes propera
                }
            }

            cachedVineCollider = closest; //guardem la liana mes propera a la cache
            nearVine = true;
        }
        else
        {
            cachedVineCollider = null;
            nearVine = false;
        }
    }

    private float originalMass;

    private void AttachToVine() //funcio per enganxar-se a la liana
    {
        if (cachedVineCollider == null || currentVineJoint != null) { return; } //si no hi ha liana o ja estem enganxats a una liana, sortim

        //Amortiguem la velocitat abans de enganxar-nos a la liana perque no quedi brusc i no s'estiri massa la liana
        float fallSpeed = Mathf.Abs(rb.linearVelocity.y);
        if (fallSpeed > 5f) //si caiem molt rapid
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x * vineAttachSpeedDamping,
                rb.linearVelocity.y * vineAttachSpeedDamping
            );
        }

        originalMass = rb.mass;
        rb.mass = originalMass * 0.3f;

        currentVineJoint = gameObject.AddComponent<HingeJoint2D>(); //creem un nou HingeJoint2D al jugador
        currentVineJoint.connectedBody = cachedVineCollider.attachedRigidbody; //connectem el HingeJoint2D al Rigidbody2D de la liana
        currentVineJoint.autoConfigureConnectedAnchor = false; //desactivem l'auto configuracio dels anchors (per defecte estan a (0,0))
        currentVineJoint.anchor = new Vector2(1.1f, 2.3f); //posicio de l'anchor al jugador
        currentVineJoint.connectedAnchor = Vector2.zero;
        currentVineJoint.enableCollision = false; //desactivem la colisio entre el jugador i la liana

        currentVineJoint.useLimits = true;
        JointAngleLimits2D limits = currentVineJoint.limits;
        limits.min = -85f;
        limits.max = 85f;
        currentVineJoint.limits = limits;
        //rb.linearVelocity = Vector2.zero; ho tinc commentat perque crec que queda millor si conserva la velocitat que portava abans d'agafar la liana
        SetGravity(1f); //revisar si cal posar-ho aqui
        ChangeState(PlayerState.Swinging); //canviem a estat Swinging
    }

    private void DetachFromVine()
    {
        if (currentVineJoint != null) //si ja tenim un HingeJoint2D creat
        {
            currentVineJoint.enabled = false;
            Destroy(currentVineJoint); //eliminem el HingeJoint2D
            currentVineJoint = null;
        }

        rb.mass = originalMass;

        cachedVineCollider = null;
        nearVine = false;
        RestoreDefaultGravity();
        animator.SetTrigger("ExitSwing");
        ChangeState(PlayerState.OnAir);
    }

    private void HandleSwingInput()
    {

        if (jumpPressed && currentState == PlayerState.OnAir && nearVine) //si li donem a la Q i estem en l'aire i a prop d'una liana
        {
            AttachToVine(); //s'agafa a la liana
            animator.SetTrigger("Swing"); //fa la animacio de agafar la liana
        }

        if (jumpReleased && currentState == PlayerState.Swinging) //si li deixem anar el espai i estem enganxats a una liana
        {
            if (currentVineJoint == null || !currentVineJoint.enabled) return;

            if (currentVineJoint.connectedBody == null)
            {
                DetachFromVine();
                return;
            }

            //guardem les dades abans de desenganxar-nos
            Transform anchor = currentVineJoint.connectedBody.transform;
            Vector2 anchorPos = anchor.position;
            Vector2 playerPos = transform.position;
            Vector2 currentSwingVelocity = rb.linearVelocity;

            //Direccio de la corda
            Vector2 ropeDir = (playerPos - anchorPos).normalized;

            //Tangent (perpendicular a la corda)
            Vector2 tangent = new Vector2(ropeDir.y, -ropeDir.x); // (puedes invertir signos si se invierte el lado)

            // Verificar que la tangente apunta en la dirección correcta del movimiento
            if (Vector2.Dot(tangent, currentSwingVelocity) < 0)
            {
                tangent = -tangent;
            }

            //Mirar la magnitud de la velocitat per aplicar-la al salt
            float currentSpeed = currentSwingVelocity.magnitude;
            float speedRatio = Mathf.Clamp01(currentSpeed / maxSwingSpeed);

            //Calcul de la veliitat de salt segons dos factors:
            //Tangent (més ràpid = més tangent)
            //Velocitat actual (més ràpid = més horitzontal)
            float horizontalInfluence = 0.7f + (speedRatio * 0.3f); // De 0.7 a 1.0
            float verticalInfluence = 0.6f + ((1f - speedRatio) * 0.4f); // De 1.0 a 0.6

            Vector2 jumpDirection = (tangent * horizontalInfluence + Vector2.up * verticalInfluence).normalized;

            DetachFromVine();

            float momentumRetention = 0.75f; // Conservamos 75% del momentum
            float jumpBoost = jumpForce * 1.3f; // Impulso base

            float dynamicBoost = jumpBoost * Mathf.Lerp(1.2f, 0.9f, speedRatio);

            Vector2 finalVelocity = (currentSwingVelocity * momentumRetention) + (jumpDirection * dynamicBoost);

            //Limirar la velocitat vertical maxima per evitar salts massa alts
            if (finalVelocity.y > 20f)
            {
                finalVelocity.y = 20f;
            }

            rb.linearVelocity = finalVelocity;

            audioSource.Stop();
            if (jumpSound != null)
            {
                audioSource.PlayOneShot(jumpSound); //reproduim el so de saltar sense que es talli el que s'estigui reproduint
            }
        }
    }

    public void ApplySwingPhysics()
    {
        if (currentVineJoint == null || !currentVineJoint.enabled) return;

        //Limitar la velocitat maxima de balanceig
        if (rb.linearVelocity.magnitude > maxSwingSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSwingSpeed;
        }

        //Aplicar fregament per reduir la velocitat amb el temps
        if (Mathf.Abs(moveInput.x) < 0.1f) //si no hi ha input de moviment
        {
            rb.linearVelocity = Vector2.Lerp(
                rb.linearVelocity,
                rb.linearVelocity * (1f - swingDrag),
                Time.fixedDeltaTime * 5f // Multiplicamos por 5 para que el lerp sea más efectivo
            );
        }

        else
        {
            //si no hi ha input de moviment aplicar una força addicional en la direccio del moviment
            //aixo permet mantenir la velocitat de balanceig quan es dona input
            float pushForce = moveInput.x * 2f;
            rb.AddForce(new Vector2(pushForce, 0f), ForceMode2D.Force);
        }

    }

    private void HandleFlip()
    {
        if(!canFlip) return;

        if (currentState == PlayerState.Idle ||
            currentState == PlayerState.Running ||
            currentState == PlayerState.OnAir && canFlip)
        {
            if (moveInput.x > 0 && !facingRight) //si es mou a la dreta i no esta mirant a la dreta
            {
                Flip();
            }
            else if (moveInput.x < 0 && facingRight) //si es mou a l'esquerra i no esta mirant a l'esquerra
            {
                Flip();
            }
        }
    }

    private void ReturnToDefaultState()
    {
        Debug.Log("Returning to default state.");
        if (!isGrounded) { ChangeState(PlayerState.OnAir); } //si encara no estem a terra anem a OnAir

        else if (Mathf.Abs(moveInput.x) > 0.1f) { ChangeState(PlayerState.Running); } //si estem a terra i ens movem anem a Running

        else { ChangeState(PlayerState.Idle); } //sino anem a Idle
    }

    private void Flip()
    {
        facingRight = !facingRight; //canviem la direccio
        Vector3 localScale = transform.localScale; //agafem l'escala actual
        localScale.x *= -1; //invertim l'escala en X
        transform.localScale = localScale; //apliquem l'escala invertida
    }


    private void ChangeState(PlayerState newState) //El currentState passa a ser el newState
    {
        currentState = newState;
    }

    public void ForceNewState(PlayerState newState)
    {
        currentState = newState;
        Debug.Log($"Player state forcibly changed to: {newState}");
    }

    private void Move()
    { 
        if (isAgainstWall)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        Vector2 targetVelocity = new Vector2(moveInput.x * speed, rb.linearVelocity.y);

        rb.linearVelocity = targetVelocity;
    }

    private void Jump()
    {
        RestoreDefaultGravity();
        //hem de posar el soroll de saltar aqui
        if (jumpSound != null)
        {
            audioSource.PlayOneShot(jumpSound); //reproduim el so de saltar sense que es talli el que s'estigui reproduint
        }
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        isGrounded = false;
        lastJumpTime = Time.time;
    }

    private void ApplyJumpMultiplier()
    {
        if (rb.linearVelocity.y < 0) //si esta  
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime; //apliquem una força extra cap avall per fer que caigui mes rapid
        }

        else if (rb.linearVelocity.y > 0 && !jumpAction.IsPressed()) //si esta pujant pero no premem el boto de saltar
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime; //apliquem una força extra cap avall per fer que no pugi tant
        }
    }


    void CheckIfGrounded()
    {
        if (Time.time - lastJumpTime < groundCheckDelay) return; // Evita comprovar si està a terra immediatament després de saltar
        
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 1.1f, LayerMask.GetMask("Ground"));

        if (hit.collider != null)
        {
            isGrounded = true;
            lastGroundPoint = hit.point;

            slopeNormal = hit.normal;
            slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
            onSlope = slopeAngle > 0f && slopeAngle <= maxSlopeAngle;
        }
        else
        {
            isGrounded = false;
            onSlope = false;
        }

        Debug.DrawRay(transform.position, Vector2.down * 1.0f, isGrounded ? Color.green : Color.red);
    }

    private void CheckWallCollision()
    {
        Vector2 direction = facingRight ? Vector2.right : Vector2.left;

        RaycastHit2D hit = Physics2D.Raycast(   
            wallCheckPosition.transform.position,
            direction,
            wallCheckDistance,
            groundLayer
        );
        
        isAgainstWall = hit.collider != null;

        Debug.DrawRay(
            wallCheckPosition.transform.position,
            direction * wallCheckDistance,
            isAgainstWall ? Color.red : Color.green
        );
    }

    private void HandleSlopeStep()
    {
        if (onSlope) return;

        Vector2 dir = new Vector2(Mathf.Sign(moveInput.x), 0); //direccio del moviment horitzontal
        Vector2 originLow = lowerOrigin.position; //origen del raycast a nivell baix
        Vector2 originUp = this.upperOrigin.position; //origen del raycast a nivell alt

        RaycastHit2D lowerHit = Physics2D.Raycast(originLow, dir, stepCheckDistance, groundLayer); //raycast a nivell baix per detectar obstacles petits

        RaycastHit2D upperHit = Physics2D.Raycast(originUp, dir, stepCheckDistance, groundLayer);

        Debug.DrawRay(originLow, dir * stepCheckDistance, Color.red);
        Debug.DrawRay(originUp, dir * stepCheckDistance, Color.green);

        if (lowerHit && !upperHit)
        {
            transform.position += new Vector3(0, stepHeight, 0);
        }
    }

    public void ActivateStaff() //ho cridaria el gameManager quan el monje ens dona el basto
    {
        staffObj.SetActive(true);
        hasStaff = true;
    }

    private void SpawnTouchGroundParticle()
    {
        Debug.Log("SpawnTouchGroundParticle called");
        if (touchGroundParticlePrefab != null)
        {
            Instantiate(touchGroundParticlePrefab,lastGroundPoint, Quaternion.identity);
        }

    }



    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("CheckPoint"))
        {
            lastCheckPoint = collision.transform;
        }
    }

    public void EnterDialogueMode()
    {
        audioSource.Stop();
        Debug.Log("Entering dialogue mode from player controller.");
        dialogueLocked = true;

        ResetInputFlags();
        isHealing = false;
        moveInput = Vector2.zero;

        animator.SetBool("HealButton", false);
        animator.SetBool("Blocking", false);
        animator.ResetTrigger("AttackPunch");
        animator.ResetTrigger("AttackTail");
        animator.ResetTrigger("SpecialAttackPunch");
        animator.ResetTrigger("StaffClimbing");
        animator.SetFloat("speed", 0f);
        animator.SetBool("isGrounded", true);

        ForceNewState(PlayerState.Idle);
        if (wakeUpFromSleep)
        {
            animator.SetTrigger("ForceIdle");
        }

    }

    public void ExitDialogueMode()
    {
        Debug.Log("Exiting dialogue mode from player controller.");
        dialogueLocked = false;
        
        ResetInputFlags();
        moveInput = Vector2.zero;

        animator.SetBool("HealButton", false);
        animator.SetBool("Blocking", false);

        animator.ResetTrigger("AttackPunch");
        animator.ResetTrigger("AttackTail");
        animator.ResetTrigger("SpecialAttackPunch");
        animator.ResetTrigger("StaffClimbing");

        ForceNewState(PlayerState.Idle);
        animator.SetTrigger("ForceIdle");
        animator.SetFloat("speed", 0f);
        animator.SetBool("isGrounded", true);
        ReturnToDefaultState(); //torna a l'estat per defecte segons si estem a terra o en l'aire
    }

    public void EndFirstSequence()
    {
        if (firstSequence != null)
        {
            firstSequence.EndSequence();
        }
    }

    private void SetGravity(float value)
    {
        currentGravity = value;
        rb.gravityScale = value;
    }

    private void RestoreDefaultGravity()
    {
        SetGravity(defaultGravity);
    }

    public void InvertControlsForSeconds(float duration)
    {
        StartCoroutine(InvertControlsCoroutine(duration));
    }

    private IEnumerator InvertControlsCoroutine(float duration)
    {
        //aqui falta posar el so o efecte visual que indica que els controls estan invertits
        invertedControls = true;
        dizzyPS.Play();
        yield return new WaitForSeconds(duration);
        invertedControls = false;
    }

    public void ResetFlip() //metodo que se llama desde una animacion para permitir que el jugador pueda girar de nuevo
    {
        canFlip = true;
        Debug.Log("Player can flip again.");
    }


}




