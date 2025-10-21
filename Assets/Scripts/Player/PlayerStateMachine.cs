using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR;

public class PlayerStateMachine : MonoBehaviour
{
    public enum PlayerState
    {
        Idle,
        Running,
        Jumping,
        Attaking,
        Climbing,
        Swinging
    }

    [Header("Movment")]
    public float speed = 5f;
    public float jumpForce = 15f;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    [SerializeField] private bool isGrounded = true;

    public PlayerState currentState;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        currentState = PlayerState.Idle;
    }

    private void Update()
    {
        switch(currentState)
        {
            case PlayerState.Idle:
                HandleIdle();
                break;

            case PlayerState.Running:
                HandleRunning();
                break;

            case PlayerState.Jumping:
                HandleJumping();
                break;

            case PlayerState.Attaking:
                break;

            case PlayerState.Climbing:
                break;

            default:
                currentState = PlayerState.Idle;
                break;

        }
        
    }

    private void HandleIdle() 
    {
        switch (true) //aqui dins van les coses que pot fer en el idle
        {
            case bool _ when moveInput.x != 0: //pot canviar el valor de x (es pot moure)? -> si
                ChangeState(PlayerState.Running);

                break;

            case bool _ when Input.GetKeyDown(KeyCode.Space) && isGrounded: //pot saltar? -> si
                Jump();
                ChangeState(PlayerState.Jumping);

                break;
        }
    }

    private void HandleRunning()
    {
        Move(); //Cridem al metode de moure's cada frame

        switch (true)
        {
            case bool _ when Mathf.Abs(moveInput.x) < 0.1: //pot quedarse quiet? -> si
                ChangeState(PlayerState.Idle);

                break;

            case bool _ when Input.GetKeyDown(KeyCode.Space) && isGrounded: //pot saltar? -> si
                Jump();
                ChangeState(PlayerState.Jumping);

                break;
        }
    }

    private void HandleJumping()
    {
        rb.linearVelocity = new Vector2(moveInput.x * speed, rb.linearVelocity.y);

        CheckIfGrounded(); //cridem a cada frame que comprovi si esta a terra o a l'aire

        if (isGrounded) //pot estar a terra? -> si
        {
            ChangeState(Mathf.Abs(moveInput.x) > 0.1f ? PlayerState.Running : PlayerState.Idle);
        }
    }


    private void ChangeState(PlayerState newState) //El currentState passa a ser el newState
    {
        currentState = newState;
    }

    private void Move()
    {
        if (Mathf.Abs(moveInput.x) < 0.1f)
            moveInput.x = 0;

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
    }

    private void FixedUpdate()
    {
        moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), 0); 

        if (currentState == PlayerState.Running || currentState == PlayerState.Jumping)
        {
            Move();
        }
    }
}


