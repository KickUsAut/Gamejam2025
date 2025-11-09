using UnityEngine;

public class MultipleOrbits : MonoBehaviour
{
    [Header("Planet Einstellungen")]
    [Tooltip("Der Planet (dieses GameObject)")]
    public Transform planet;
    
    [Header("Kugel Einstellungen")]
    [Tooltip("Das Kugel-Prefab das um den Planeten fliegen soll")]
    public GameObject spherePrefab;
    
    [Tooltip("Anzahl der Kugeln")]
    public int numberOfSpheres = 5;
    
    [Tooltip("Radius/Abstand zum Planeten")]
    public float orbitRadius = 5f;
    
    [Tooltip("Geschwindigkeit (Grad pro Sekunde)")]
    public float orbitSpeed = 50f;
    
    [Header("Visuelle Linien")]
    [Tooltip("Zeige Linien im Editor")]
    public bool showOrbitLines = true;
    
    [Tooltip("Farbe der Orbit-Linien")]
    public Color lineColor = Color.yellow;
    
    private GameObject[] orbitingSpheres;
    private float[] startAngles;
    
    void Start()
    {
        if (planet == null)
        {
            planet = transform;
        }
        
        CreateOrbitingSpheres();
    }
    
    void CreateOrbitingSpheres()
    {
        // Lösche alte Kugeln falls vorhanden
        if (orbitingSpheres != null)
        {
            foreach (GameObject sphere in orbitingSpheres)
            {
                if (sphere != null)
                    Destroy(sphere);
            }
        }
        
        orbitingSpheres = new GameObject[numberOfSpheres];
        startAngles = new float[numberOfSpheres];
        
        // Berechne gleichmäßige Verteilung
        float angleStep = 360f / numberOfSpheres;
        
        for (int i = 0; i < numberOfSpheres; i++)
        {
            // Erstelle Kugel
            GameObject sphere;
            if (spherePrefab != null)
            {
                sphere = Instantiate(spherePrefab);
            }
            else
            {
                // Erstelle Standard-Kugel falls kein Prefab zugewiesen
                sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.transform.localScale = Vector3.one * 0.3f;
            }
            
            sphere.name = "OrbitSphere_" + i;
            sphere.transform.parent = transform;
            
            // Setze Startwinkel
            startAngles[i] = i * angleStep;
            
            orbitingSpheres[i] = sphere;
        }
    }
    
    void Update()
    {
        if (orbitingSpheres == null || planet == null) return;
        
        for (int i = 0; i < orbitingSpheres.Length; i++)
        {
            if (orbitingSpheres[i] == null) continue;
            
            // Berechne aktuellen Winkel
            float currentAngle = startAngles[i] + (orbitSpeed * Time.time);
            float radians = currentAngle * Mathf.Deg2Rad;
            
            // Berechne Position
            float x = Mathf.Cos(radians) * orbitRadius;
            float z = Mathf.Sin(radians) * orbitRadius;
            
            // Setze Position
            orbitingSpheres[i].transform.position = planet.position + new Vector3(x, 0, z);
        }
    }
    
    // Zeichne Orbit-Linien im Editor
    void OnDrawGizmos()
    {
        if (!showOrbitLines) return;
        
        Transform center = planet != null ? planet : transform;
        
        Gizmos.color = lineColor;
        
        // Zeichne Kreis für jede Orbit-Linie
        if (numberOfSpheres > 0)
        {
            float angleStep = 360f / numberOfSpheres;
            
            for (int s = 0; s < numberOfSpheres; s++)
            {
                // Zeichne den kompletten Orbit-Kreis
                Vector3 prevPos = Vector3.zero;
                for (int i = 0; i <= 64; i++)
                {
                    float angle = (i / 64f) * 360f * Mathf.Deg2Rad;
                    float x = Mathf.Cos(angle) * orbitRadius;
                    float z = Mathf.Sin(angle) * orbitRadius;
                    Vector3 pos = center.position + new Vector3(x, 0, z);
                    
                    if (i > 0)
                        Gizmos.DrawLine(prevPos, pos);
                    
                    prevPos = pos;
                }
                
                // Zeichne Startposition der Kugel
                float startAngle = s * angleStep * Mathf.Deg2Rad;
                float startX = Mathf.Cos(startAngle) * orbitRadius;
                float startZ = Mathf.Sin(startAngle) * orbitRadius;
                Vector3 spherePos = center.position + new Vector3(startX, 0, startZ);
                
                Gizmos.color = Color.cyan;
                Gizmos.DrawSphere(spherePos, 0.2f);
                Gizmos.color = lineColor;
            }
        }
    }
    
    // Button im Inspector zum neu generieren
    void OnValidate()
    {
        if (Application.isPlaying)
        {
            CreateOrbitingSpheres();
        }
    }
}