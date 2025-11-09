using UnityEngine;

public class MultipleOrbits : MonoBehaviour
{
    [Header("Planet Einstellungen")]
    [Tooltip("Der Planet (dieses GameObject)")]
    public Transform planet;
    
    [Header("Kugel Einstellungen")]
    [Tooltip("Das Kugel-Prefab das um den Planeten fliegen soll")]
    public GameObject spherePrefab;
    
    [Header("Route 1 - Äquator")]
    [Tooltip("Anzahl der Kugeln auf Route 1")]
    public int route1_numberOfSpheres = 5;
    [Tooltip("Radius/Abstand zum Planeten")]
    public float route1_orbitRadius = 5f;
    [Tooltip("Geschwindigkeit (Grad pro Sekunde)")]
    public float route1_orbitSpeed = 50f;
    [Tooltip("Neigung X-Achse")]
    public float route1_tiltX = 0f;
    [Tooltip("Neigung Y-Achse")]
    public float route1_tiltY = 0f;
    [Tooltip("Neigung Z-Achse")]
    public float route1_tiltZ = 0f;
    [Tooltip("Farbe der Orbit-Linie")]
    public Color route1_lineColor = Color.yellow;
    
    [Header("Route 2 - Polar")]
    [Tooltip("Anzahl der Kugeln auf Route 2")]
    public int route2_numberOfSpheres = 3;
    [Tooltip("Radius/Abstand zum Planeten")]
    public float route2_orbitRadius = 5f;
    [Tooltip("Geschwindigkeit (Grad pro Sekunde)")]
    public float route2_orbitSpeed = 40f;
    [Tooltip("Neigung X-Achse")]
    public float route2_tiltX = 0f;
    [Tooltip("Neigung Y-Achse")]
    public float route2_tiltY = 0f;
    [Tooltip("Neigung Z-Achse")]
    public float route2_tiltZ = 90f;
    [Tooltip("Farbe der Orbit-Linie")]
    public Color route2_lineColor = Color.cyan;
    
    [Header("Route 3 - Diagonal")]
    [Tooltip("Anzahl der Kugeln auf Route 3")]
    public int route3_numberOfSpheres = 4;
    [Tooltip("Radius/Abstand zum Planeten")]
    public float route3_orbitRadius = 6f;
    [Tooltip("Geschwindigkeit (Grad pro Sekunde)")]
    public float route3_orbitSpeed = 30f;
    [Tooltip("Neigung X-Achse")]
    public float route3_tiltX = 45f;
    [Tooltip("Neigung Y-Achse")]
    public float route3_tiltY = 45f;
    [Tooltip("Neigung Z-Achse")]
    public float route3_tiltZ = 0f;
    [Tooltip("Farbe der Orbit-Linie")]
    public Color route3_lineColor = Color.magenta;
    
    [Header("Visuelle Einstellungen")]
    [Tooltip("Zeige Orbit-Linien im Editor")]
    public bool showOrbitLines = true;
    [Tooltip("Zeige Startpositionen der Kugeln")]
    public bool showStartPositions = true;
    
    private GameObject[] route1_spheres;
    private float[] route1_startAngles;
    
    private GameObject[] route2_spheres;
    private float[] route2_startAngles;
    
    private GameObject[] route3_spheres;
    private float[] route3_startAngles;
    
    void Start()
    {
        if (planet == null)
        {
            planet = transform;
        }
        
        CreateAllOrbits();
    }
    
    void CreateAllOrbits()
    {
        // Lösche alte Kugeln
        DestroyRoute(route1_spheres);
        DestroyRoute(route2_spheres);
        DestroyRoute(route3_spheres);
        
        // Erstelle neue Kugeln
        route1_spheres = CreateOrbitRoute(route1_numberOfSpheres, out route1_startAngles, "Route1");
        route2_spheres = CreateOrbitRoute(route2_numberOfSpheres, out route2_startAngles, "Route2");
        route3_spheres = CreateOrbitRoute(route3_numberOfSpheres, out route3_startAngles, "Route3");
    }
    
    void DestroyRoute(GameObject[] spheres)
    {
        if (spheres != null)
        {
            foreach (GameObject sphere in spheres)
            {
                if (sphere != null)
                    Destroy(sphere);
            }
        }
    }
    
    GameObject[] CreateOrbitRoute(int numberOfSpheres, out float[] startAngles, string routeName)
    {
        GameObject[] spheres = new GameObject[numberOfSpheres];
        startAngles = new float[numberOfSpheres];
        
        float angleStep = 360f / numberOfSpheres;
        
        for (int i = 0; i < numberOfSpheres; i++)
        {
            GameObject sphere;
            if (spherePrefab != null)
            {
                sphere = Instantiate(spherePrefab);
            }
            else
            {
                sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.transform.localScale = Vector3.one * 0.3f;
            }
            
            sphere.name = $"{routeName}_Sphere_{i}";
            sphere.transform.parent = transform;
            
            startAngles[i] = i * angleStep;
            spheres[i] = sphere;
        }
        
        return spheres;
    }
    
    void Update()
    {
        if (planet == null) return;
        
        UpdateRoute(route1_spheres, route1_startAngles, route1_orbitRadius, route1_orbitSpeed, 
                    route1_tiltX, route1_tiltY, route1_tiltZ);
        UpdateRoute(route2_spheres, route2_startAngles, route2_orbitRadius, route2_orbitSpeed, 
                    route2_tiltX, route2_tiltY, route2_tiltZ);
        UpdateRoute(route3_spheres, route3_startAngles, route3_orbitRadius, route3_orbitSpeed, 
                    route3_tiltX, route3_tiltY, route3_tiltZ);
    }
    
    void UpdateRoute(GameObject[] spheres, float[] startAngles, float radius, float speed, 
                     float tiltX, float tiltY, float tiltZ)
    {
        if (spheres == null) return;
        
        for (int i = 0; i < spheres.Length; i++)
        {
            if (spheres[i] == null) continue;
            
            float currentAngle = startAngles[i] + (speed * Time.time);
            float radians = currentAngle * Mathf.Deg2Rad;
            
            float x = Mathf.Cos(radians) * radius;
            float z = Mathf.Sin(radians) * radius;
            
            Vector3 localPos = new Vector3(x, 0, z);
            Quaternion rotation = Quaternion.Euler(tiltX, tiltY, tiltZ);
            Vector3 rotatedPos = rotation * localPos;
            
            spheres[i].transform.position = planet.position + rotatedPos;
        }
    }
    
    void OnDrawGizmos()
    {
        if (!showOrbitLines) return;
        
        Transform center = planet != null ? planet : transform;
        
        DrawOrbitGizmo(center, route1_orbitRadius, route1_numberOfSpheres, route1_tiltX, route1_tiltY, route1_tiltZ, route1_lineColor);
        DrawOrbitGizmo(center, route2_orbitRadius, route2_numberOfSpheres, route2_tiltX, route2_tiltY, route2_tiltZ, route2_lineColor);
        DrawOrbitGizmo(center, route3_orbitRadius, route3_numberOfSpheres, route3_tiltX, route3_tiltY, route3_tiltZ, route3_lineColor);
    }
    
    void DrawOrbitGizmo(Transform center, float radius, int numberOfSpheres, 
                        float tiltX, float tiltY, float tiltZ, Color lineColor)
    {
        if (numberOfSpheres <= 0) return;
        
        Gizmos.color = lineColor;
        Quaternion rotation = Quaternion.Euler(tiltX, tiltY, tiltZ);
        
        // Zeichne Orbit-Kreis
        Vector3 prevPos = Vector3.zero;
        int segments = 64;
        
        for (int i = 0; i <= segments; i++)
        {
            float angle = (i / (float)segments) * 360f * Mathf.Deg2Rad;
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;
            
            Vector3 localPos = new Vector3(x, 0, z);
            Vector3 rotatedPos = rotation * localPos;
            Vector3 pos = center.position + rotatedPos;
            
            if (i > 0)
                Gizmos.DrawLine(prevPos, pos);
            
            prevPos = pos;
        }
        
        // Zeichne Startpositionen
        if (showStartPositions)
        {
            float angleStep = 360f / numberOfSpheres;
            
            for (int s = 0; s < numberOfSpheres; s++)
            {
                float startAngle = s * angleStep * Mathf.Deg2Rad;
                float startX = Mathf.Cos(startAngle) * radius;
                float startZ = Mathf.Sin(startAngle) * radius;
                
                Vector3 localPos = new Vector3(startX, 0, startZ);
                Vector3 rotatedPos = rotation * localPos;
                Vector3 spherePos = center.position + rotatedPos;
                
                Gizmos.color = Color.white;
                Gizmos.DrawWireSphere(spherePos, 0.15f);
            }
        }
    }
    
    void OnValidate()
    {
        if (Application.isPlaying)
        {
            CreateAllOrbits();
        }
    }
}