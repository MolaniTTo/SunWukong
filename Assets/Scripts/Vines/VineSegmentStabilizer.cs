using UnityEngine;

public class VineSegmentStabilizer : MonoBehaviour
{
    private DistanceJoint2D distanceJoint;
    public float distanceBetweenSegments = 0.5f;

    void Start()
    {
        // Obtener el HingeJoint2D existente
        HingeJoint2D hingeJoint = GetComponent<HingeJoint2D>();

        if (hingeJoint != null && hingeJoint.connectedBody != null)
        {
            // Añadir un DistanceJoint2D para mantener la distancia
            distanceJoint = gameObject.AddComponent<DistanceJoint2D>();
            distanceJoint.connectedBody = hingeJoint.connectedBody;
            distanceJoint.autoConfigureDistance = false;
            distanceJoint.distance = distanceBetweenSegments;
            distanceJoint.maxDistanceOnly = true;
            distanceJoint.enableCollision = false;
        }
    }
}