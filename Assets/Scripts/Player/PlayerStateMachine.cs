using System.Collections;
using System.Collections.Specialized;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using UnityEngine.XR;

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

    [Header("Control Modifiers")]
    public bool invertedControls = false;


    [Header("Ki System")]
    public float maxKi = 100f;
    [HideInInspector] public float currentKi;

    [Header("Ki Costs")]
    public float specialAttackPunchCost = 50f;
    public float specialAttackStaffCost = 1f;
    public float healingKiCostPerSecond = 10f;


    [Header("Ki Regeneration")]
    public float kiPerEnemyKilled = 20f;

    [Header("Stats")]
    public Rigidbody2D rb;
    private Vector2 moveInput;
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

    private float defaultGravity = 2f;
    private float currentGravity = 2f;

    private bool wasRTPressed = false;
    private bool wasLTPressed = false;
    private bool wasLTAndRTPressed = false;

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





    public PlayerState currentState;

    private struct InputFlags
    {
        public float horizontal;
        //SALT
        public bool jumpDown; //tecla de espai baixada
        public bool jumpUp; //tecla de espai aixecada
        //CURAR
        public bool healDown; //tecla de curar baixada
        public bool healUp; //tecla de curar aixecada
        //ATACS
        public bool attackPunchDown; //tecla d'atac de puny baixada
        public bool attackTailDown; //tecla d'atac de cua baixada
        public bool specialAttackPunch; //tecla d'atac especial de puny baixada
        //LIANA
        public bool swingDown; //tecla de liana baixada
        public bool swingUp; //tecla de liana aixecada
        //BLOQUEIG
        public bool blockDown; //tecla de bloqueig baixada
        public bool blockUp; //tecla de bloqueig aixecada
        //BASTO
        public bool staffClimbDown; //tecla de liana baixada
        public bool staffClimbUp; //tecla de liana aixecada
        //ATAC ESPECIAL BASTO
        public bool specialAttackStaffDown; //tecla d'atac especial de pal baixada

    }
    public event System.Action<float> OnKiChanged;  
    private InputFlags input;

    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        SetGravity(2f);
        animator = GetComponent<Animator>();

        punchDamageCollider.SetActive(false); //desactivem el collider de dany al iniciar
        tailDamageCollider.SetActive(false); //desactivem el collider de dany al iniciar
        if (!hasStaff) { staffObj.SetActive(false); }

         currentKi = maxKi;
        OnKiChanged?.Invoke(currentKi);
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

        if (!dialogueLocked) { HandleInputs(); } 

        animator.SetBool("isGrounded", isGrounded); //important per les transicions cap a OnAir i Idle/Running
        animator.SetFloat("speed", Mathf.Abs(moveInput.x)); //important per les transicions cap a Running i Idle
        animator.SetFloat("verticalVelocity", rb.linearVelocity.y); //important per el blend tree de saltar

        CheckIfNearVine(); //Funcio que comprova si estem a prop d'una liana
        ApplyJumpMultiplier(); //Funcio que aplica el jump multiplier per fer saltos mes naturals
        HandleSwingInput(); //Funcio que comprova el input de liana AJUNTAR I FER UNA UNICA FUNCIO D'INPUTS?
        
        //HandleHealingInput(); //Funcio que comprova el input de curar AJUNTAR I FER UNA UNICA FUNCIO D'INPUTS?
        //HandleAttackInput(); //Funcio que mira els inputs d'atac AJUNTAR I FER UNA UNICA FUNCIO D'INPUTS?
        //HandleBlockInput(); //Funcio que comprova el input de bloqueig AJUNTAR I FER UNA UNICA FUNCIO D'INPUTS?

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

    }

    private void FixedUpdate()
    {
        if(dialogueLocked) //si el diàleg està actiu, no processem moviments
        {
            return;
        }
        float h = Input.GetAxisRaw("Horizontal");
        h = invertedControls ? -h : h;
        moveInput = new Vector2(h, 0); //nomes ens interessa l'eix horitzontal per moure'ns

        HandleFlip(); //el posem aqui ja que ho mirem just despres del moveInput
        CheckIfGrounded();
        CheckWallCollision();
        if(onSlope && input.horizontal == 0) //si estem en una pendent i no hi ha input horitzontal, parem el moviment
        {
            rb.linearVelocity = Vector2.zero;
            SetGravity(0f);
            return;
        }
        else if (onSlope && input.horizontal != 0)
        {
            RestoreDefaultGravity();
        }

        if (currentState == PlayerState.Running || currentState == PlayerState.OnAir || currentState == PlayerState.AttackPunch || currentState == PlayerState.AttackTail) //podem moure'ns en aquests estats
        {
            Move();
            HandleSlopeStep();
        }
    }

    private void HandleInputs()
    {
        if (dialogueLocked) //si el diàleg està actiu, no processem inputs
        {
            return;
        }
        //Horizontal
        float h = Input.GetAxisRaw("Horizontal");
        input.horizontal = invertedControls ? -h : h; //invertem els controls si cal

        //SALT
        input.jumpDown = Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.JoystickButton0); //tecla de espai baixada o botó A del joystick
        input.jumpUp = Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.JoystickButton0); ; //tecla de espai aixecada o botó A del joystick

        //CURAR
        input.healDown = Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.JoystickButton3);
        input.healUp = Input.GetKeyUp(KeyCode.E) || Input.GetKeyUp(KeyCode.JoystickButton3);

        //ATACS
        input.attackPunchDown = Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.JoystickButton2);
        input.attackTailDown = Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.JoystickButton1);

        //SPECIAL ATTACK PUNCH
        bool rtPressed = Input.GetAxis("RightTrigger") > 0.5f; //detecta si el gatell dret esta premut
        input.specialAttackPunch = Input.GetKeyDown(KeyCode.G) || (rtPressed && !wasRTPressed); 

        //LIANA
        input.swingDown = Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.JoystickButton0);
        input.swingUp = Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.JoystickButton0);

        bool ltPressed = false;
        bool ltAndRtPressed = false;

        if (hasStaff)
        {
            //BLOQUEIG
            input.blockDown = Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.JoystickButton4);
            input.blockUp = Input.GetKeyUp(KeyCode.Q) || Input.GetKeyUp(KeyCode.JoystickButton4);

            //BASTO
            ltPressed = Input.GetAxis("LeftTrigger") > 0.5f; //detecta si el gatell esquerre esta premut
            input.staffClimbDown = Input.GetMouseButtonDown(1) || (ltPressed && !wasLTPressed);
            input.staffClimbUp = Input.GetMouseButtonUp(1) || (!ltPressed && wasLTPressed);

            //ATAC ESPECIAL BASTO
            rtPressed = Input.GetAxis("RightTrigger") > 0.5f; //detecta si el gatell dret esta premut
            input.specialAttackStaffDown = Input.GetKeyDown(KeyCode.V) || (rtPressed && !wasRTPressed); //si premem V o si premem el gatell dret

        }

        wasRTPressed = rtPressed;
        wasLTPressed = ltPressed;
        wasLTAndRTPressed = ltAndRtPressed;

        ProcessInputActions();
    }

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

    private void ProcessInputActions()
    {
        //BLOQUEIG
        if (input.blockDown) //si premem el boto de bloqueig
        {
            if (isGrounded)
            {
                ChangeState(PlayerState.Block);
                animator.SetBool("Blocking", true);
                if (blockSound != null)
                {
                    audioSource.Stop();
                    audioSource.PlayOneShot(blockSound);
                }
            }
        }
        if (input.blockUp) //si deixem de prémer el botó de bloqueig
        {
            if (currentState == PlayerState.Block)
            {
                animator.SetBool("Blocking", false);
                ReturnToDefaultState();
            }
        }

        //HEAL
        if (input.healDown)
        {
            if (isGrounded && !isHealing && currentKi > 0)
            {
                isHealing = true;
                animator.SetBool("HealButton", true);
                ChangeState(PlayerState.Healing);
            }
            else if (currentKi <= 0)
            {
                Debug.Log("¡No tienes Ki para curarte!");
            }
        }
        if (input.healUp)
        {
            if (isHealing)
            {
                isHealing = false;
                animator.SetBool("HealButton", false);
            }
        }

        //ATACS
        if (input.attackPunchDown) //si premem el boto d'atac de puny i estem en un estat que ho permet
        {
            if (currentState == PlayerState.Idle ||
                currentState == PlayerState.Running ||
                currentState == PlayerState.OnAir)
            {
                ChangeState(PlayerState.AttackPunch);
                animator.SetTrigger("AttackPunch");
                if (punchAttackSound != null)
                {
                    audioSource.PlayOneShot(punchAttackSound);
                }
            }
        }

        if (input.attackTailDown) //si premem el boto d'atac de cua i estem en un estat que ho permet
        {
            if (currentState == PlayerState.Idle ||
                currentState == PlayerState.Running ||
                currentState == PlayerState.OnAir)
            {
                ChangeState(PlayerState.AttackTail);
                animator.SetTrigger("AttackTail");
                if (tailAttackSound != null)
                {
                    audioSource.PlayOneShot(tailAttackSound);
                }
            }
        }

        if (input.specialAttackPunch)
        {
            if (currentState == PlayerState.Idle || currentState == PlayerState.Running)
            {
                if (TryConsumeKi(specialAttackPunchCost))
                {
                    ChangeState(PlayerState.SpecialAttackPunch);
                    animator.SetTrigger("SpecialAttackPunch");

                }
                else
                {
                    Debug.Log("¡No tienes suficiente Ki para el ataque especial de puño!");
                }
            }
        }

        //BASTO
        if (input.staffClimbDown && isGrounded && hasStaff) //si premem el boto dret del ratoli i estem a terra i tenim el basto
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
            return; 
        }
        if (Mathf.Abs(moveInput.x) > 0.1f) //Si es mou
        {
            ChangeState(PlayerState.Running);
            return;
        }

        if (input.jumpDown && isGrounded) //Si premem saltar i esta a terra
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

        if (Mathf.Abs(moveInput.x) < 0.1f) //Si no es mou
        {
            audioSource.Stop();
            ChangeState(PlayerState.Idle); //Canviem a estat Idle
            return;
        }
        if (input.jumpDown && isGrounded) //Si premem saltar i esta a terra
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
        if(isComingFromClimbing && input.specialAttackStaffDown && !isGrounded) //si ve de escalar i premem el atac especial del basto
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
            if (currentKi > 0)
            {
                characterHealth.Heal(1f * Time.deltaTime);
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
            }
    }

    if (!isHealing)
    {
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

        float swingInput = Input.GetAxisRaw("Horizontal"); //agafa el input horitzontal per balancejar-se

        if (Mathf.Abs(swingInput) > 0.1f) //si hi ha input per balancejar-se
        {
            Vector2 ropeDir = (transform.position - currentVineJoint.connectedBody.transform.position).normalized; //vector que va desde el punt de connexio de la liana fins al jugador

            Vector2 tangent = new Vector2(-ropeDir.y, ropeDir.x); //vector tangent a la liana (perpendicular al vector ropeDir)

            rb.AddForce(tangent * swingInput * 5f, ForceMode2D.Force); //apliquem una força en la direccio tangent per simular el balanceig
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

        if (input.staffClimbUp) //si deixem anar el boto dret del ratoli
        {
            staffController.ResetStaff(); //reiniciem el pal
            animator.SetTrigger("StopStaffClimbing"); //fa la animacio de treure el pal del terra
            RestoreDefaultGravity();
            ChangeState(PlayerState.OnAir); //anem a estat OnAir
        }

        if (input.jumpDown) //si premem saltar mentre estem escalant
        {
            staffController.ResetStaff();
            animator.SetTrigger("StopStaffClimbing");
            RestoreDefaultGravity();
            Jump();
            isComingFromClimbing = true; //indiquem que venim de escalar per poder fer l'atac especial del basto en l'aire
            ChangeState(PlayerState.OnAir); //anem a estat OnAir
        }

        if (input.specialAttackStaffDown)
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
        if (input.blockUp) //si deixem de prémer el botó de bloqueig
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

    private void OnSpecialAttackStaffImpact() //Cridat des de l'animacio mitjancant un Animation Event
    {
        //activar collider del pal per fer dany
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

    private void AttachToVine() //funcio per enganxar-se a la liana
    {
        if (cachedVineCollider == null || currentVineJoint != null) { return; } //si no hi ha liana o ja estem enganxats a una liana, sortim

        currentVineJoint = gameObject.AddComponent<HingeJoint2D>(); //creem un nou HingeJoint2D al jugador
        currentVineJoint.connectedBody = cachedVineCollider.attachedRigidbody; //connectem el HingeJoint2D al Rigidbody2D de la liana
        currentVineJoint.autoConfigureConnectedAnchor = false; //desactivem l'auto configuracio dels anchors (per defecte estan a (0,0))

        currentVineJoint.anchor = new Vector2(1.1f, 2.3f); //posicio de l'anchor al jugador
        currentVineJoint.connectedAnchor = Vector2.zero;

        //rb.linearVelocity = Vector2.zero; ho tinc commentat perque crec que queda millor si conserva la velocitat que portava abans d'agafar la liana
        SetGravity(1f); //revisar si cal posar-ho aqui
        ChangeState(PlayerState.Swinging); //canviem a estat Swinging
    }

    private void DetachFromVine()
    {
        if (currentVineJoint != null) //si ja tenim un HingeJoint2D creat
        {
            DestroyImmediate(currentVineJoint); //eliminem el HingeJoint2D
            currentVineJoint = null;
        }
        cachedVineCollider = null;
        nearVine = false;
        RestoreDefaultGravity();
        animator.SetTrigger("ExitSwing");
        ChangeState(PlayerState.OnAir);
    }

    private void HandleSwingInput()
    {

        if (input.jumpDown && currentState == PlayerState.OnAir && nearVine) //si li donem a la Q i estem en l'aire i a prop d'una liana
        {
            AttachToVine(); //s'agafa a la liana
            animator.SetTrigger("Swing"); //fa la animacio de agafar la liana
        }

        if (input.jumpUp && currentState == PlayerState.Swinging) //si li deixem anar el espai i estem enganxats a una liana
        {
            if (currentVineJoint == null) return;

            Transform anchor = currentVineJoint.connectedBody.transform;

            Vector2 ropeDir = (transform.position - anchor.position).normalized;

            Vector2 tangent = new Vector2(ropeDir.y, -ropeDir.x); // (puedes invertir signos si se invierte el lado)

            Vector2 jumpDir = (tangent * 1.0f + Vector2.up * 0.8f).normalized;

            Vector2 currentSwungVelocity = rb.linearVelocity;

            Debug.DrawRay(transform.position, ropeDir * 2f, Color.red, 2f);
            Debug.DrawRay(transform.position, tangent * 2f, Color.cyan, 2f);
            Debug.DrawRay(transform.position, jumpDir * 2f, Color.yellow, 2f);

            DetachFromVine();


            //tallem el so de la liana i reproduim el so de saltar
            audioSource.Stop();
            if (jumpSound != null)
            {
                audioSource.PlayOneShot(jumpSound); //reproduim el so de saltar sense que es talli el que s'estigui reproduint
            }



            rb.linearVelocity = (currentSwungVelocity * 0.8f) + (jumpDir * (jumpForce * 1.6f));
        }
    }

    private void HandleFlip()
    {
        if (currentState == PlayerState.Idle ||
            currentState == PlayerState.Running ||
            currentState == PlayerState.OnAir)
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

        else if (rb.linearVelocity.y > 0 && !Input.GetKey(KeyCode.Space)) //si esta pujant pero no premem el boto de saltar
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

        input = new InputFlags();

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
        input = new InputFlags(); //reset input flags
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
        yield return new WaitForSeconds(duration);
        invertedControls = false;
    }


}




