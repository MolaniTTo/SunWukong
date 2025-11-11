using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR;

public class PlayerStateMachine : MonoBehaviour
{
    public enum PlayerState
    {
        Idle,
        Running,
        OnAir,
        AttackPunch,
        AttackTail,
        Block,
        BeingHit,
        Healing,
        SpecialAttackPunch,
        SpecialAttackStaff,
        Swinging,
        Climbing,
        Death
    }

    [Header("Movment")]
    public float speed = 5f;
    public float jumpForce = 15f;
    [SerializeField] private LayerMask groundLayer;

   [Header("Animation")]
   [SerializeField] private Animator animator;


    private Rigidbody2D rb;
    private Vector2 moveInput;
    [SerializeField] private bool isGrounded = true;
    [SerializeField] private bool isHealing = false;

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
                //HandleAttackTail();
                break;

            case PlayerState.Block:
                //HandleBlock();
                break;

            case PlayerState.BeingHit:
                //HandleBeingHit();
                break;

            case PlayerState.SpecialAttackPunch:
                //HandleSpecialAttackPunch();
                break;

            case PlayerState.SpecialAttackStaff:
                //HandleSpecialAttackStaff();
                break;

            case PlayerState.Swinging:
                //HandleSwinging();
                break;

            case PlayerState.Climbing:
                //HandleClimbing();
                break;

            case PlayerState.Death:
                //HandleDeath();
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


