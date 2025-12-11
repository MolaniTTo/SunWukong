using UnityEngine;

public class PlayerStaffController : MonoBehaviour
{
    [Header("Bones")]
    public Transform topBone;
    public Transform tipBone;

    [Header("Stretch Settings")]
    public float currentDownLength = 0f;
    public float currentUpLength = 0f;
    public float maxDownLength = 5f;
    public float maxUpLength = 3f;
    public float extendSpeedDown = 4f;
    public float extendSpeedUp = 1.5f;

    [Header("Ground Check")]
    public LayerMask groundLayer;
    public float tipRadius = 0.15f;
    public bool touchingGround = false;
    public bool reachedTop = false;
    public Vector3 groundPoint;

    [Header("Audio Source")]
    public AudioSource staffAudioSource;
    public AudioClip growingSound;

    private Vector3 topBoneDefaultPos;
    private Vector3 tipBoneDefaultPos;

    private void Awake()
    {
        topBoneDefaultPos = topBone.localPosition;
        tipBoneDefaultPos = tipBone.localPosition;
    }

    public void ResetStaff()
    {
        currentDownLength = 0f;
        currentUpLength = 0f;
        touchingGround = false;
        reachedTop = false;

        ApplyTransform();
    }

    public void ExtendDown()
    {
        if (currentDownLength >= maxDownLength) return;
        Debug.Log("Extending Down");
        currentDownLength += extendSpeedDown * Time.deltaTime;
        ApplyTransform();
        //aplicar el soroll
        if (!staffAudioSource.isPlaying)
        {
            staffAudioSource.clip = growingSound;
            staffAudioSource.Play();
        }

        Collider2D hit = Physics2D.OverlapCircle(tipBone.position, tipRadius, groundLayer);
        if (hit != null)
        {
            Debug.Log("Staff Tip Touching Ground");
            touchingGround = true;
            groundPoint = tipBone.position;
        }

    }

    public void ExtendUp()
    {
        if (currentUpLength >= maxUpLength)
        {
            reachedTop = true;
            return;
        }

        currentUpLength += extendSpeedUp * Time.deltaTime;
        ApplyTransform();

        tipBone.position = groundPoint;
    }

    private void ApplyTransform()
    {
        // move top bone up slowly
        topBone.localPosition = new Vector3(
            topBoneDefaultPos.x + currentUpLength,
            topBoneDefaultPos.y,
            topBoneDefaultPos.z
        );

        // move tip bone down fast
        tipBone.localPosition = new Vector3(
            tipBoneDefaultPos.x + currentDownLength,
            tipBoneDefaultPos.y,
            tipBoneDefaultPos.z
        );
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        if (tipBone != null)
            Gizmos.DrawWireSphere(tipBone.position, tipRadius);
    }
}
