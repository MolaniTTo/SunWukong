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
        //ConBaston
        Block,
        SpecialAttackStaff,

        Climbing,
        
    }

    [Header("Movment")]
    public float speed = 5f;
    public float jumpForce = 15f;
    [SerializeField] private LayerMask groundLayer;
    private float groundCheckDelay = 0.1f;
    private float lastJumpTime = 0f;
    private bool facingRight = true; //esta mirant a la dreta (default)

    [Header("Stats")]
    private Rigidbody2D rb;
    private Vector2 moveInput;
    [SerializeField] private bool isGrounded = true;
    [SerializeField] private bool isHealing = false;

    [Header("Refs")]
    [SerializeField] private Animator animator;

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
    

    public PlayerState currentState;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        currentState = PlayerState.Idle;
    }

    private void Update()
    {
        animator.SetBool("isGrounded", isGrounded); //important per les transicions cap a OnAir i Idle/Running
        animator.SetFloat("speed", Mathf.Abs(moveInput.x)); //important per les transicions cap a Running i Idle
        animator.SetFloat("verticalVelocity", rb.linearVelocity.y); //important per el blend tree de saltar

        HandleHealingInput(); //Funcio que comprova el input de curar
        HandleAttackInput(); //Funcio que mira els inputs d'atac
        HandleSwingInput(); //Funcio que comprova el input de liana
        CheckIfNearVine(); //Funcio que comprova si estem a prop d'una liana
        ApplyJumpMultiplier(); //Funcio que aplica el jump multiplier per fer saltos mes naturals

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

            case PlayerState.Block:
                //HandleBlock();
                break;

            case PlayerState.SpecialAttackStaff:
                //HandleSpecialAttackStaff();
                break;



            case PlayerState.Climbing:
                //HandleClimbing();
                break;



            default:
                currentState = PlayerState.Idle;
                break;
        }

        CheckIfGrounded();
    }

    private void HandleIdle() 
    {

        if (Mathf.Abs(moveInput.x) > 0.1f)
        {
            ChangeState(PlayerState.Running);
        }
        else if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Jump();
            ChangeState(PlayerState.OnAir);
        }
    }

    private void HandleRunning()
    {
        Move(); //Cridem al metode de moure's cada frame

        if (Mathf.Abs(moveInput.x) < 0.1f) //Si no es mou
        {
            ChangeState(PlayerState.Idle); //Canviem a estat Idle
        }
        else if (Input.GetKeyDown(KeyCode.Space) && isGrounded) //Si premem saltar i esta a terra
        {
            Jump(); //Saltem
            isGrounded = false;
            ChangeState(PlayerState.OnAir); //Canviem a estat OnAir
        }
    }

    private void HandleJumping()
    {
        Move(); //Cridem al metode de moure's cada frame
        CheckIfGrounded();

        if (isGrounded)
        {
            animator.SetTrigger("TouchGround"); //animacio d'aterrar
            ChangeState(Mathf.Abs(moveInput.x) > 0.1f ? PlayerState.Running : PlayerState.Idle); //Si es mou, a Running, si no a Idle
        }
    }

    private void HandleHealing()
    {
        if (!isHealing && animator.GetCurrentAnimatorStateInfo(0).IsName("StopHealing")) //si ens hem acabat de curar, i estem a la animacio de StopHealing (vol dir que hem deixat anat el boto)
        {
            ChangeState(PlayerState.Idle); //Nomes podem anar a Idle despres de healing
        }
        if(isHealing) //si ens estem curant
        {
            //aqui pujarem la vida i farem lo de baixar el KI de la UI
           
        }
    }

    private void HandleHealingInput()
    {
        if (Input.GetKeyDown(KeyCode.E)) //per exemple la E es per curar
        {
            if(isGrounded && !isHealing) //nomes ens podem curar si estem a terra i si no ens estem curant
            {
                isHealing = true;
                animator.SetBool("HealButton", true);
                ChangeState(PlayerState.Healing); //Canviem a estat Healing
            }

        }
        if (Input.GetKeyUp(KeyCode.E) && isHealing) //si deixem anar el boto de curar mentre ens estem curant
        {
            isHealing = false;
            animator.SetBool("HealButton", false);
        }
    }
   

    private void HandleAttackPunch()
    {
        Move(); //podem moure mentre fa la animacio d'atac

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        if (!stateInfo.IsName("PunchAttack")) //Comprovem si estem a l'animacio d'atac i si ha acabat
        {
            if (!isGrounded) { ChangeState(PlayerState.OnAir); } //si encara no estem a terra anem a OnAir

            else if (Mathf.Abs(moveInput.x) > 0.1f) { ChangeState(PlayerState.Running); } //si estem a terra i ens movem anem a Running

            else { ChangeState(PlayerState.Idle); } //sino anem a Idle
        }
    }

    public void OnPunchImpact() //Cridat des de l'animacio mitjancant un Animation Event
    {
        //activa el collider del puny per fer dany

    }

    public void OnPunchImpactEnd() //Cridat des de l'animacio mitjancant un Animation Event
    {
        //desactiva el collider del puny per no fer dany
    }

    private void HandleAttackTail()
    {
        Move(); //volem que es pugui moure durant la animacio cridem a move()

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0); //agafa la info de l'animacio actual

        if (!stateInfo.IsName("AttackTail")) //si estem a l'animacio d'atac i ha acabat
        {
            if (!isGrounded) { ChangeState(PlayerState.OnAir); } //si encara no estem a terra anem a OnAir

            else if (Mathf.Abs(moveInput.x) > 0.1f) { ChangeState(PlayerState.Running); } //si estem a terra i ens movem anem a Running

            else { ChangeState(PlayerState.Idle); } //sino anem a Idle
        }
    }

    public void OnTailImpact() //Cridat des de l'animacio mitjancant un Animation Event
    {

    }

    public void OnTailImpactEnd() //Cridat des de l'animacio mitjancant un Animation Event
    {

    }

    private void HandleAttackInput()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (currentState == PlayerState.Idle || currentState == PlayerState.Running || currentState == PlayerState.OnAir) //podem atacar desde terra en idle, corrent o en el aire
            {
                ChangeState(PlayerState.AttackPunch);
                animator.SetTrigger("AttackPunch");
            }
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            if (currentState == PlayerState.Idle || currentState == PlayerState.Running || currentState == PlayerState.OnAir)
            {
                ChangeState(PlayerState.AttackTail);
                animator.SetTrigger("AttackTail");
            }
        }

        if( Input.GetKeyDown(KeyCode.G))
        {
            if (currentState == PlayerState.Idle || currentState == PlayerState.Running)
            {
                ChangeState(PlayerState.SpecialAttackPunch);
                animator.SetTrigger("SpecialAttackPunch");
            }
        }
    }

    private void HandleBeingHit()
    {
        rb.linearVelocity = Vector2.zero;
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        
        if (!stateInfo.IsName("BeingHit"))
        {
            if (!isGrounded) { ChangeState(PlayerState.OnAir); } //si encara no estem a terra anem a OnAir

            else if (Mathf.Abs(moveInput.x) > 0.1f) { ChangeState(PlayerState.Running); } //si estem a terra i ens movem anem a Running

            else { ChangeState(PlayerState.Idle); } //sino anem a Idle
        }
    }


    private void HandleSpecialAttackPunch()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        if (!stateInfo.IsName("SpecialAttackPunch"))
        {
            if (Mathf.Abs(moveInput.x) > 0.1f && isGrounded) { ChangeState(PlayerState.Running); } //si estem a terra i ens movem anem a Running
            else if (isGrounded) { ChangeState(PlayerState.Idle); } //sino anem a Idle
        }
    }

    public void OnSpecialPunchImpact() //Cridat des de l'animacio mitjancant un Animation Event
    {
        //funcio de aturdir als enemics que te davant
    }

    public void Die() //revisar perque tenim la classe generica de characterHealth
    {
        if (currentState == PlayerState.Death) return;

        ChangeState(PlayerState.Death);
        animator.SetTrigger("Death");
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false; // desactiva físicas para que no se mueva más
    }

    private void HandleDeath() 
    {
        //logica de respawn
    }
    private void CheckIfNearVine()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(vineCheckPoint.position, vineCheckRadius, vineLayer);
        if (hits.Length > 0)
        {
            float minDist = float.MaxValue;
            Collider2D closest = null;

            foreach (var hit in hits)
            {
                float dist = Vector2.Distance(vineCheckPoint.position, hit.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = hit;
                }
            }

            cachedVineCollider = closest;
            nearVine = true;
        }
        else
        {
            cachedVineCollider = null;
            nearVine = false;
        }
    }


    private void AttachToVine()
    {
        if (cachedVineCollider == null || currentVineJoint != null) { return; } //si no hi ha liana o ja estem enganxats a una liana, sortim

        currentVineJoint = gameObject.AddComponent<HingeJoint2D>(); //creem un nou HingeJoint2D al jugador
        currentVineJoint.connectedBody = cachedVineCollider.attachedRigidbody; //connectem el HingeJoint2D al Rigidbody2D de la liana
        currentVineJoint.autoConfigureConnectedAnchor = false; //desactivem l'auto configuracio dels anchors (per defecte estan a (0,0))

        currentVineJoint.anchor = new Vector2(1.1f, 2.3f);
        currentVineJoint.connectedAnchor = Vector2.zero;

        rb.linearVelocity = Vector2.zero; //aturar la velocitat del jugador al enganxar-se a la liana
        rb.gravityScale = 1f; //revisar si cal posar-ho aqui
        ChangeState(PlayerState.Swinging); //canviem a estat Swinging
    }


    private void DetachFromVine()
    {
        if (currentVineJoint != null) //si ja tenim un HingeJoint2D creat
        {
            Destroy(currentVineJoint); //eliminem el HingeJoint2D
            currentVineJoint = null;
        }

        cachedVineCollider = null;
        nearVine = false;
        rb.gravityScale = 1f; //revisar si cal posar-ho aqui
        animator.SetTrigger("ExitSwing");
        ChangeState(PlayerState.OnAir);
    }


    private void HandleSwingInput()
    {
        if (Input.GetKeyDown(KeyCode.Q) && currentState == PlayerState.OnAir && nearVine) //si li donem a la Q i estem en l'aire i a prop d'una liana
        {
            AttachToVine();
            animator.SetTrigger("Swing");
        }

        if (Input.GetKeyUp(KeyCode.Q) && currentState == PlayerState.Swinging)
        {
            Vector2 ropeDir = (transform.position - currentVineJoint.connectedBody.transform.position).normalized;
            Vector2 jumpDir = (ropeDir + Vector2.up).normalized;

            DetachFromVine();
            rb.linearVelocity = jumpDir * jumpForce * 8f; //apliquem una velocitat en la direccio de saltar
            Debug.Log("Desenganxat de la liana");
        }
    }

    private void HandleSwinging()
    {
        if (currentVineJoint == null) return;

        float swingInput = Input.GetAxisRaw("Horizontal");

        if(Mathf.Abs(swingInput) > 0.1f)
        {
            Vector2 ropeDir = (transform.position - currentVineJoint.connectedBody.transform.position).normalized; //vector que va des de la liana al jugador

            Vector2 tangent = new Vector2(-ropeDir.y, ropeDir.x); //vector tangent a la liana (perpendicular al vector ropeDir)

            rb.AddForce(tangent * swingInput * 5f, ForceMode2D.Force); //apliquem una força en la direccio tangent per simular el balanceig
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Vector2 ropeDir = (transform.position - currentVineJoint.connectedBody.transform.position).normalized;
            Vector2 jumpDir = (ropeDir + Vector2.up).normalized;

            DetachFromVine();
            rb.AddForce(jumpDir * 3f, ForceMode2D.Impulse);
        }
    }

    private void HandleFlip()
    {
        if(currentState == PlayerState.Idle ||
            currentState == PlayerState.Running ||
            currentState == PlayerState.OnAir)
        {
            if(moveInput.x > 0 && !facingRight) //si es mou a la dreta i no esta mirant a la dreta
            {
                Flip();
            }
            else if(moveInput.x < 0 && facingRight) //si es mou a l'esquerra i no esta mirant a l'esquerra
            {
                Flip();
            }
        }
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

    private void Move()
    {
        rb.linearVelocity = new Vector2(moveInput.x * speed, rb.linearVelocity.y);
        CheckIfGrounded();
    }
    
    private void Jump()
    {
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
        isGrounded = hit.collider != null;
        Debug.DrawRay(transform.position, Vector2.down * 1.0f, isGrounded ? Color.green : Color.red);
    }

    private void FixedUpdate()
    {
        moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), 0); //Agafem input horitzontal per moure'ns

        HandleFlip(); //el posem aqui ja que ho mirem just despres del moveInput

        if (currentState == PlayerState.Running || currentState == PlayerState.OnAir) //podem moure'ns en aquests estats
        {
            Move();
        }
    }
}


