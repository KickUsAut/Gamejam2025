using UnityEngine;

public class PlayerKnockback : MonoBehaviour
{
    [Header("Knockback Einstellungen")]
    [Tooltip("Stärke des Knockbacks")]
    public float knockbackForce = 10f;
    
    [Tooltip("Dauer des Knockbacks in Sekunden")]
    public float knockbackDuration = 0.3f;
    
    [Header("Tags für Kollision")]
    [Tooltip("Tags von Objekten die Knockback auslösen (z.B. 'OrbitSphere', 'Planet')")]
    public string[] knockbackTags = { "OrbitSphere", "Planet" };
    
    private Rigidbody rb;
    private bool isKnockedBack = false;
    private float knockbackTimer = 0f;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        if (rb == null)
        {
            Debug.LogError("Kein Rigidbody am Spieler gefunden! Füge einen Rigidbody hinzu.");
        }
    }
    
    void Update()
    {
        // Knockback Timer
        if (isKnockedBack)
        {
            knockbackTimer -= Time.deltaTime;
            if (knockbackTimer <= 0)
            {
                isKnockedBack = false;
            }
        }
    }
    
    void OnCollisionEnter(Collision collision)
    {
        // Prüfe ob das Objekt ein Knockback-Tag hat
        bool shouldKnockback = false;
        foreach (string tag in knockbackTags)
        {
            if (collision.gameObject.CompareTag(tag))
            {
                shouldKnockback = true;
                break;
            }
        }
        
        if (shouldKnockback && !isKnockedBack)
        {
            ApplyKnockback(collision);
        }
    }
    
    void ApplyKnockback(Collision collision)
    {
        if (rb == null) return;
        
        // Berechne Richtung weg vom Kollisionspunkt
        Vector3 knockbackDirection = transform.position - collision.contacts[0].point;
        knockbackDirection.y = 0; // Nur horizontal wegschleudern
        knockbackDirection.Normalize();
        
        // Füge etwas Aufwärts-Kraft hinzu für einen schöneren Effekt
        knockbackDirection += Vector3.up * 0.3f;
        
        // Wende Knockback an
        rb.velocity = Vector3.zero; // Reset aktuelle Geschwindigkeit
        rb.AddForce(knockbackDirection * knockbackForce, ForceMode.Impulse);
        
        // Setze Knockback-Status
        isKnockedBack = true;
        knockbackTimer = knockbackDuration;
        
        Debug.Log("Knockback angewendet!");
    }
    
    // Optional: Getter um zu prüfen ob Spieler gerade knocked back ist
    public bool IsKnockedBack()
    {
        return isKnockedBack;
    }
}