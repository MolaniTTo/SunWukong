using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Ray : MonoBehaviour
{
    [Header("Bones")]
    public Transform topRay; //os de adal
    public Transform tipRay; //os de abaix

    [Header("Ray Growing Settings")]
    public float growSpeed = 8f; //velocitat de creixement cap a baix
    public float retractSpeed = 10f; //velocitat de retracció cap a dalt
    public float maxLength = 8f; //limit de longitud máxima

    [Header("Ground Check")]
    public float tipRadius = 0.1f;
    public LayerMask groundLayer;

    [Header("References")]
    public Animator circleAnimator; //animació del cercle a terra
    public GameObject circle;
    private RayManager rayManager;

    [Header("Light Settings")]
    public Light2D freeformLight; //referencia a la luz freeform
    private Vector3[] originalShape; //array amb les posicions dels vertex de la freeform light
    private int[] bottomIndices = null; //punts del shape de la freeform light que corresponen a la part de abaix
    public int[] topIndices = null; //punts del shape de la freeform light que corresponen a la part de adal
    public float bottomOffset = -0.2f; //en negatiu = més abaix
    public float topOffset = 0.2f; //en positiu = més amunt

    public bool hitGround = false;
    private Vector3 groundPoint;

    private void Start()
    {
        rayManager = FindFirstObjectByType<RayManager>();

        if (freeformLight != null && freeformLight.lightType == Light2D.LightType.Freeform)
        {
            originalShape = freeformLight.shapePath.ToArray(); //guardem la forma original de la freeform light

            if(originalShape.Length >= 2) //assegurem que hi ha almenys dos vertex
            {
                var indexed = originalShape
                    .Select((value, index) => new { index, y = value.y }) //crea un objecte anònim amb l'index i la posició y del vertex
                    .OrderBy(x => x.y) //ordena els vertex per la seva posició y (de menor a major)
                    .ToArray(); //converteix a array

                bottomIndices = new int[2] { indexed[0].index, indexed[1].index }; //agafa els dos primers (els de baix) i els guarda a bottomIndices

                var topIndexed = originalShape
                    .Select((value, index) => new { index, y = value.y })
                    .OrderByDescending(x => x.y) //ordena de major a menor
                    .ToArray();

                topIndices = new int[2] { topIndexed[0].index, topIndexed[1].index }; //agafa els dos primers (els de dalt) i els guarda a topIndices

            }
        }
        StartCoroutine(RayRoutine());
        circle.SetActive(false); //desactiva el cercle fins que toqui terra
    }


    private IEnumerator RayRoutine()
    {
        //creix fins a tocar terra
        while (!hitGround)
        {

            //mou el os tip cap avall (estira la mesh)
            tipRay.position += Vector3.down * growSpeed * Time.deltaTime;

            //MODIFICACIONS DE LA MESH DE LA FREEFORM LIGHT
            UpdateFreeformShape();

            //per si de cas s'ha passat del maxim
            if (Vector3.Distance(topRay.position, tipRay.position) > maxLength) { break; }

            //detecta el el terra
            Collider2D hit = Physics2D.OverlapCircle(tipRay.position, tipRadius, groundLayer);
            if (hit != null)
            {
                hitGround = true;
                groundPoint = tipRay.position;

                circle.SetActive(true); //activa el cercle
                if (circleAnimator != null) { circleAnimator.SetTrigger("Hit"); }
                rayManager.monje.OnRayImpactShakeCam();
            }

            yield return null;
        }

        
        yield return new WaitForSeconds(2f);

        while (Vector3.Distance(topRay.position, tipRay.position) > 0.05f)
        {
            //mou el os top cap avall
            topRay.position = Vector3.MoveTowards(topRay.position, tipRay.position, retractSpeed * Time.deltaTime);

            UpdateFreeformShape();

            yield return null;
        }

        //Destruir el raig
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(tipRay.position, tipRadius);
    }

    private void UpdateFreeformShape()
    {
        if (freeformLight != null && freeformLight.lightType == Light2D.LightType.Freeform && originalShape != null && originalShape.Length >= 2) //assegura que es una freeform light i que tenim la forma original
        {
            Vector3[] newShape = new Vector3[originalShape.Length]; //crea un nou array per la nova forma

            for (int i = 0; i < originalShape.Length; i++)
            {
                newShape[i] = originalShape[i]; //copia la forma original
            }

            Vector3 tipLocal = freeformLight.transform.InverseTransformPoint(tipRay.position); //converteix la posició del tipRay a coordenades locals de la light

            Vector3 topLocal = freeformLight.transform.InverseTransformPoint(topRay.position); //converteix la posició del topRay a coordenades locals de la light

            if (bottomIndices != null && bottomIndices.Length >= 2)
            {
                float yBottom = tipLocal.y + bottomOffset; //calcula la posició y dels punts de baix amb l'offset
                for (int i = 0; i < bottomIndices.Length; i++)
                {
                    int index = bottomIndices[i];
                    newShape[index] = new Vector3(newShape[index].x, yBottom, newShape[index].z); //actualitza la posició y dels punts de baix al y del tipRay
                }
            }

            if (topIndices != null && topIndices.Length >= 2)
            {
                float yTop = topLocal.y + topOffset; //calcula la posició y dels punts de dalt amb l'offset
                for (int i = 0; i < topIndices.Length; i++)
                {
                    int index = topIndices[i];
                    newShape[index] = new Vector3(newShape[index].x, yTop, newShape[index].z); //actualitza la posició y dels punts de dalt al y del topRay
                }
            }

            freeformLight.SetShapePath(newShape); //actualitza la forma de la freeform light

        }
    }
}
