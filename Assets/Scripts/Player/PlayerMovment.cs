using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovment : MonoBehaviour, InputSystem_Actions.IPlayerActions //hereda de la interficie IPlayerActions generada pel Input System per implementar els mètodes d'input
{
    [Header("Movment")]
    public float speed = 5f;
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private bool isGrounded = true;
    public float jumpForce = 15f;

    [SerializeField] private LayerMask groundLayer;

    private InputSystem_Actions controls;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>(); //agafa el component Rigidbody2D
        controls = new InputSystem_Actions(); //crea la instancia del mapa de controls
        controls.Player.SetCallbacks(this); //assigna els callbacks d'aquest script al mapa de controls de Player
    }

    void OnEnable()
    {
        controls.Player.Enable(); //activa el mapa de controls de Player (per defecte està desactivat)
    }

    void OnDisable()
    {
        controls.Player.Disable(); //desactiva el mapa de controls de Player (quan l'script està desactivat o destruït)
    }

    public void OnMove(InputAction.CallbackContext context) //mètode cridat quan hi ha input de moviment
    {
        moveInput = context.ReadValue<Vector2>(); //llegeix el valor de l'input de moviment (Vector2) i l'assigna a moveInput
    }

    public void OnJump(InputAction.CallbackContext context) //mètode cridat quan hi ha input de salt
    {
        CheckIfGrounded(); //comprova si el jugador està a terra
        if (context.performed && isGrounded) //si l'input de salt s'ha realitzat i el jugador està a terra
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce); //aplica una força de salt al Rigidbody2D en l'eix Y
        }

    }

    private void CheckIfGrounded() //comprova si el jugador està a terra mitjançant un raycast
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 1.1f, groundLayer); //tira un raycast cap avall des de la posició del jugador de 0.6 de llargada i només col·lisiona amb el layer de terra
        isGrounded = hit.collider != null ? true : false; //operador ternari: si el raycast col·lisiona amb alguna cosa, isGrounded és true, sinó és false
    }


    public void OnFire(InputAction.CallbackContext context)
    {
       
    }

    public void OnLook(InputAction.CallbackContext context)
    {

    }

    void FixedUpdate()
    {
        Vector2 velocity = rb.linearVelocity; //agafa la velocitat actual del Rigidbody2D
        velocity.x = moveInput.x * speed; //calcula la nova velocitat en l'eix X segons l'input de moviment i la velocitat definida
        rb.linearVelocity = velocity; //assigna la nova velocitat al Rigidbody2D

    }

    private void OnDrawGizmos()
    {
        //Dibuixa el raycast per comprovar si està a terra
        Gizmos.color = Color.red;

        //Distancia del raycast per comprovar si està a terra
        float rayDistance = 1.1f;

        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * rayDistance);
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
        
    }

    public void OnPrevious(InputAction.CallbackContext context)
    {
      
    }

    public void OnNext(InputAction.CallbackContext context)
    {
       
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
     
    }
}
