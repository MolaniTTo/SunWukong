using UnityEngine;

public class LevelContainer : MonoBehaviour
{
    [Header("Ascenso Automático")]
    public float climbSpeed = 2f;

    [Header("Límite superior opcional")]
    public float maxHeight = 0f;
    public bool stopAtMaxHeight = false;

    void Update()
    {
        Vector3 movement = Vector3.up * climbSpeed * Time.deltaTime;

        if (stopAtMaxHeight && maxHeight > 0)
        {
            if (transform.position.y < maxHeight)
                transform.position += movement;
        }
        else
        {
            transform.position += movement;
        }
    }
}
