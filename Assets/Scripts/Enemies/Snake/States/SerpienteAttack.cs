using UnityEngine;

public class SerpienteAttack : IState
{
    private EnemySnake snake;
    private bool hasExited = false;
    private float attackStartTime;
    private float maxAttackDuration = 2f;
    private float lastDebugTime = 0f;
    private float debugInterval = 0.5f;
    private float lastRangeCheckTime = 0f;
    private float rangeCheckInterval = 0.1f;
    private bool hasCheckedAfterAnimation = false;
    private float minTimeInState = 0.5f;

    public SerpienteAttack(EnemySnake snake)
    {
        this.snake = snake;
    }

    public void Enter()
    {
        hasExited = false;
        attackStartTime = Time.time;
        lastDebugTime = Time.time;
        lastRangeCheckTime = Time.time;
        hasCheckedAfterAnimation = false;

        Debug.Log($"[SNAKE ATTACK] ═══════ ENTER ═══════ Time: {Time.time:F2}");

        if (snake.CheckIfPlayerIsDead())
        {
            Debug.Log("[SNAKE ATTACK] Player is dead, going to patrol");
            snake.StateMachine.ChangeState(new SerpientePatrol(snake));
            return;
        }

        if (!snake.IsPlayerInAttackRange())
        {
            Debug.Log("[SNAKE ATTACK] Player NOT in range, exiting to movement");
            ExitToMovement();
            return;
        }

        if (snake.Player != null)
        {
            float dir = snake.Player.position.x - snake.transform.position.x;
            if (dir > 0 && !snake.facingRight) 
                snake.Flip();
            else if (dir < 0 && snake.facingRight) 
                snake.Flip();
        }

        Debug.Log($"[SNAKE ATTACK] Starting attack sequence");
        snake.StartAttack();
        snake.StopHissSound();
    }

    public void Update()
    {
        if (hasExited) return;

        AnimatorStateInfo currentStateInfo = snake.animator.GetCurrentAnimatorStateInfo(0);
        float timeInState = Time.time - attackStartTime;

        if (Time.time - lastDebugTime >= debugInterval)
        {
            lastDebugTime = Time.time;
            
            Debug.Log($"[SNAKE ATTACK] UPDATE | " +
                     $"Time: {timeInState:F2}s | " +
                     $"isAttacking: {snake.IsCurrentlyAttacking()} | " +
                     $"AnimState: {currentStateInfo.shortNameHash} | " +
                     $"AnimTime: {currentStateInfo.normalizedTime:F2} | " +
                     $"InRange: {snake.IsPlayerInAttackRange()} | " +
                     $"CanAttack: {snake.CanAttack()} | " +
                     $"HasChecked: {hasCheckedAfterAnimation}");
        }

        if (timeInState > maxAttackDuration)
        {
            Debug.LogWarning($"[SNAKE ATTACK] ⚠️ TIMEOUT after {timeInState:F2}s - Forcing exit");
            ForceExit();
            return;
        }

        if (snake.CheckIfPlayerIsDead())
        {
            Debug.Log("[SNAKE ATTACK] Player died during attack");
            ExitToPatrol();
            return;
        }

        if (Time.time - lastRangeCheckTime >= rangeCheckInterval)
        {
            lastRangeCheckTime = Time.time;
            
            if (!snake.IsPlayerInAttackRange() && snake.IsCurrentlyAttacking())
            {
                Debug.Log("[SNAKE ATTACK] ⚠️ Player left range DURING attack - Canceling!");
                snake.ForceStopAttack();
                hasCheckedAfterAnimation = true;
                ExitToMovement();
                return;
            }
        }

        if (snake.IsCurrentlyAttacking())
        {
            snake.StopMovement();
            return;
        }

        if (!hasCheckedAfterAnimation && timeInState >= minTimeInState)
        {
            if (currentStateInfo.IsName("attack") && currentStateInfo.normalizedTime >= 0.9f)
            {
                hasCheckedAfterAnimation = true;
                Debug.Log($"[SNAKE ATTACK] ✓ Animation complete - Checking next state");
                
                CheckStateAfterAttack();
            }
        }
    }

    private void CheckStateAfterAttack()
    {
        if (hasExited) return;

        bool playerInRange = snake.IsPlayerInAttackRange();
        bool canAttackAgain = snake.CanAttack();
        bool playerDead = snake.CheckIfPlayerIsDead();

        Debug.Log($"[SNAKE ATTACK] CheckState | InRange: {playerInRange} | CanAttack: {canAttackAgain} | PlayerDead: {playerDead}");

        if (playerDead)
        {
            ExitToPatrol();
            return;
        }

        if (playerInRange && canAttackAgain)
        {
            Debug.Log("[SNAKE ATTACK] ↻ Player still in range - Restarting attack");
            
            hasCheckedAfterAnimation = false;
            attackStartTime = Time.time;
            lastRangeCheckTime = Time.time;
            
            snake.StartAttack();
        }
        else if (playerInRange && !canAttackAgain)
        {
            Debug.Log("[SNAKE ATTACK] ⏳ Player in range but cooldown active - Exiting to chase");
            ExitToMovement();
        }
        else
        {
            Debug.Log("[SNAKE ATTACK] ← Player out of range - Exiting");
            ExitToMovement();
        }
    }

    private void ForceExit()
    {
        if (hasExited) return;
        
        Debug.LogError("[SNAKE ATTACK] ⚠️ FORCE EXIT!");
        snake.ForceStopAttack();
        hasCheckedAfterAnimation = true;
        ExitToMovement();
    }

    private void ExitToMovement()
    {
        if (hasExited) return;
        hasExited = true;

        Debug.Log("[SNAKE ATTACK] ═══════ EXIT TO MOVEMENT ═══════");

        snake.OnAttackEnd();
        if (snake.biteCollider != null)
            snake.biteCollider.SetActive(false);

        snake.animator.ResetTrigger("Attack");
        snake.animator.ResetTrigger("Damaged");

        if (snake.CanSeePlayer())
        {
            Debug.Log("[SNAKE ATTACK] → Changing to CHASE");
            snake.animator.SetBool("isMoving", false);
            snake.animator.SetBool("isChasing", true);
            snake.animator.Play("walk", 0, 0f);
            snake.StateMachine.ChangeState(new SerpienteChase(snake));
        }
        else
        {
            ExitToPatrol();
        }
    }

    private void ExitToPatrol()
    {
        if (hasExited) return;
        hasExited = true;

        Debug.Log("[SNAKE ATTACK] ═══════ EXIT TO PATROL ═══════");

        snake.OnAttackEnd();
        if (snake.biteCollider != null)
            snake.biteCollider.SetActive(false);

        snake.animator.ResetTrigger("Attack");
        snake.animator.ResetTrigger("Damaged");
        snake.animator.SetBool("isChasing", false);
        snake.animator.SetBool("isMoving", true);
        snake.animator.Play("walk", 0, 0f);
        
        snake.StateMachine.ChangeState(new SerpientePatrol(snake));
    }

    public void Exit()
    {
        Debug.Log("[SNAKE ATTACK] ═══════ EXIT CALLED ═══════");
        snake.OnAttackEnd();
        snake.animator.ResetTrigger("Attack");
        if (snake.biteCollider != null)
            snake.biteCollider.SetActive(false);
    }
}