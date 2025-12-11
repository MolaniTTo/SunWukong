using UnityEngine;

public class GorilaRetreating : IState
{
    private Gorila gorila;
    private float retreatTimer;
    private float retreatDuration = 1.2f; // Tiempo que retrocede
    private float retreatSpeedMultiplier = 0.7f; // Velocidad reducida para que se vea natural

    public GorilaRetreating(Gorila gorila)
    {
        this.gorila = gorila;
    }
    public void Enter()
    {
        retreatTimer = 0f;
        gorila.animator.speed = 0.8f;
        gorila.lockFacing = true; //revisar ya que quiero que para el retroceso gire dejando el player atras
        gorila.animator.SetBool("isRunning", true); // Usa la misma animación de correr
        if (gorila.gorilaAudioSource != null)
        {
            gorila.gorilaAudioSource.clip = gorila.Walk;
            gorila.gorilaAudioSource.loop = true;
            gorila.gorilaAudioSource.Play();
        }
    }

    public void Exit()
    {
        gorila.animator.SetBool("isRunning", false);
        gorila.StopMovement();
        gorila.lockFacing = false;
        gorila.gorilaAudioSource.Stop();
    }


    public void Update()
    {
        if (gorila.CheckIfPlayerIsDead())
        {
            gorila.StateMachine.ChangeState(gorila.IdleState);
            return;
        }
        retreatTimer += Time.deltaTime;

        Vector2 retreatDir = new Vector2(-gorila.facingDirection, 0);

        float speed = (gorila.characterHealth.currentHealth <= gorila.lowHealthThreshold ? gorila.speedAtLowHealth : gorila.baseSpeed) * retreatSpeedMultiplier;

        gorila.rb.linearVelocity = retreatDir * speed;

        if (retreatTimer >= retreatDuration)
        {
            gorila.StateMachine.ChangeState(gorila.IdleState);
        }
    }
}
