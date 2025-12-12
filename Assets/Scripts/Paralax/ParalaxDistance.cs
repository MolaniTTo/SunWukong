using UnityEngine;

public class ParallaxStatic : MonoBehaviour
{
    public Transform cameraTarget;
    [Header("1 = igual que el suelo, >1 más lejos, <1 más cerca")]
    public float distance = 2f;
    private Vector3 startPos;
    private float startCamX;
    public bool stopPararallax = false;
    void Start()
    {
        if (cameraTarget == null)
            cameraTarget = Camera.main.transform;
        startPos = transform.position;
        startCamX = cameraTarget.position.x;
    }
    void LateUpdate()
    {
        if (stopPararallax) return;
        float camDeltaX = cameraTarget.position.x - startCamX;
        // Parallax relativo (no mueve el objeto de su sitio real)
        float offsetX = camDeltaX * (1f - (1f / distance));
        transform.position = new Vector3(
            startPos.x + offsetX,
            startPos.y,
            startPos.z
        );
    }
}