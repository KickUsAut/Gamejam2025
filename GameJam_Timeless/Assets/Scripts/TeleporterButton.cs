using UnityEngine;

public class TeleporterButton : MonoBehaviour
{
    [Header("Teleporter Referenz")]
    public GameObject teleporter; // Ziehe hier deinen Teleporter rein
    
    [Header("Geist Einstellungen")]
    public string ghostTag = "Ghost"; // Passe den Tag-Namen hier an
    
    [Header("Visuelles Feedback")]
    public Color inactiveColor = Color.red;
    public Color activeColor = Color.green;
    public float glowIntensity = 2f;
    
    private bool isActivated = false;
    private Renderer buttonRenderer;
    private Material buttonMaterial;

    void Start()
    {
        // Material Setup
        buttonRenderer = GetComponent<Renderer>();
        if (buttonRenderer != null)
        {
            buttonMaterial = buttonRenderer.material;
            buttonMaterial.EnableKeyword("_EMISSION");
            UpdateButtonVisuals();
        }
        
        // Teleporter initial deaktivieren
        if (teleporter != null)
        {
            teleporter.SetActive(false);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Etwas hat den Button berührt: " + other.gameObject.name + " (Tag: " + other.tag + ")");
        
        // Prüfe ob es der Geist ist (passe den Tag an deinen Geist an)
        if (other.CompareTag(ghostTag) && !isActivated)
        {
            ActivateTeleporter();
        }
        else if (!other.CompareTag(ghostTag))
        {
            Debug.LogWarning("Falscher Tag! Erwartet: '" + ghostTag + "', gefunden: '" + other.tag + "'");
        }
    }
    
    // Alternative Methode falls OnTriggerEnter nicht funktioniert
    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collision mit: " + collision.gameObject.name + " (Tag: " + collision.gameObject.tag + ")");
        
        if (collision.gameObject.CompareTag(ghostTag) && !isActivated)
        {
            ActivateTeleporter();
        }
    }

    void ActivateTeleporter()
    {
        isActivated = true;
        
        // Teleporter aktivieren
        if (teleporter != null)
        {
            teleporter.SetActive(true);
            Debug.Log("Teleporter freigeschaltet!");
        }
        
        // Visuelles Feedback
        UpdateButtonVisuals();
        
        // Optional: Sound abspielen
        // AudioSource audio = GetComponent<AudioSource>();
        // if (audio != null) audio.Play();
    }

    void UpdateButtonVisuals()
    {
        if (buttonMaterial != null)
        {
            Color currentColor = isActivated ? activeColor : inactiveColor;
            buttonMaterial.color = currentColor;
            buttonMaterial.SetColor("_EmissionColor", currentColor * glowIntensity);
        }
    }
}