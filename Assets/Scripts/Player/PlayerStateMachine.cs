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

    [Header("Stats")]
    private Rigidbody2D rb;
    private Vector2 moveInput;
    [SerializeField] private bool isGrounded = true;
    [SerializeField] private bool isHealing = false;

    [Header("Refs")]
    [SerializeField] private Animator animator;
    [SerializeField] private LayerMask vineLayer; // asigna en el inspector la capa "Liana"
    [SerializeField] private float vineCheckRadius = 0.5f; // radio de detección de la liana
    [SerializeField] private bool nearVine = false; // si está cerca de una liana

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

    private void CheckIfNearVine() //deteccio de liana
    {
        //posicion del mono +1 en y para que el circulo este a la altura del mono y no en los pies
        Vector2 checkPosition = new Vector2(transform.position.x, transform.position.y + 2f);
        nearVine = Physics2D.OverlapCircle(checkPosition, vineCheckRadius, vineLayer);
        //dibujar el raycast en la escena para debug
        Debug.DrawRay(checkPosition, Vector2.right * vineCheckRadius, nearVine ? Color.green : Color.red);

    }

    private void HandleSwingInput()
    {
        if (Input.GetKeyDown(KeyCode.Q) && currentState == PlayerState.OnAir && nearVine) //si li donem a la Q i estem en l'aire i a prop d'una liana
        {
            Debug.Log("Agafant la liana");
            ChangeState(PlayerState.Swinging);
            animator.SetTrigger("Swing");
            rb.linearVelocity = Vector2.zero; //revisar aveure com fem lo de la liana
            rb.gravityScale = 0; //revisar tambe
        }

        if (Input.GetKeyUp(KeyCode.Q) && currentState == PlayerState.Swinging) //si deixem anar la Q mentre estem a l'estat de Swinging
        {
            rb.gravityScale = 1; //revisar tambe
            Jump(); //saltem de la liana
            ChangeState(PlayerState.OnAir); //anem a OnAir
        }
    }

    private void HandleSwinging()
    {
        //per sortir de l'estat ja ho fem amb el propi handleSwingInput()
        //aqui va la logica de moure's a la liana
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
    }

    void CheckIfGrounded()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 1.1f, LayerMask.GetMask("Ground"));
        isGrounded = hit.collider != null;
        Debug.DrawRay(transform.position, Vector2.down * 1.0f, isGrounded ? Color.green : Color.red);
    }

    private void FixedUpdate()
    {
        moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), 0); 

        if (currentState == PlayerState.Running || currentState == PlayerState.OnAir)
        {
            Move();
        }
    }
}


