using UnityEngine;

public class EarthquakeWave : MonoBehaviour
{
    private Gorila gorila; //Referencia al gorila que ha creat l'ona
    public float speed = 8f;
    public float lifetime = 3f;
    public float damage = 20f;
    private float direction;

    private void Start()
    {
        gorila = FindAnyObjectByType<Gorila>();
        if ( gorila == null) { Debug.LogError("EarthquakeWave: No s'ha trobat el component Gorila al pare!"); }
        direction = -Mathf.Sign(gorila.transform.localScale.x);
        Destroy(gameObject, lifetime);

    }

    private void Update()
    {
        Vector2 movement = Vector2.right * direction * speed * Time.deltaTime;
        transform.Translate(movement, Space.World);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("EarthquakeWave: Colision amb el jugador detectada.");
            KnockBack knockBack = other.GetComponent<KnockBack>();
            if (knockBack != null) 
            {
                Vector2 knockBackDirection = new Vector2(direction, 1).normalized; //Direcció diagonal cap amunt
                knockBack.ApplyKnockBack(knockBackDirection);
            }
            var health = other.GetComponent<PlayerHealth>();
            if (health != null) 
            {
                health.TakeDamage(damage); 
                Destroy(gameObject); //destrueix l'ona després de colisionar amb el jugador
            }
        }
    }


}
