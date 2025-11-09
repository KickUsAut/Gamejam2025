using UnityEngine;

public class OrbitAround : MonoBehaviour
{
    [Header("Planet")]
    public Transform planet;
    
    [Header("Stein Prefab")]
    public GameObject rockPrefab;
    
    [Header("Ring 1")]
    public int ring1_rocks = 8;
    public float ring1_radius = 10f;
    public float ring1_speed = 30f;
    public float ring1_height = 0f;
    public Vector3 ring1_axis = Vector3.up;
    
    [Header("Ring 2")]
    public bool ring2_active = false;
    public int ring2_rocks = 6;
    public float ring2_radius = 12f;
    public float ring2_speed = -20f;
    public float ring2_height = 3f;
    public Vector3 ring2_axis = Vector3.up;
    
    [Header("Ring 3")]
    public bool ring3_active = false;
    public int ring3_rocks = 10;
    public float ring3_radius = 8f;
    public float ring3_speed = 40f;
    public float ring3_height = -2f;
    public Vector3 ring3_axis = Vector3.up;

    private GameObject[] ring1Objects;
    private GameObject[] ring2Objects;
    private GameObject[] ring3Objects;

    void Start()
    {
        ring1Objects = CreateRing(ring1_rocks, ring1_radius, ring1_height, ring1_axis, "Ring1");
        
        if (ring2_active)
            ring2Objects = CreateRing(ring2_rocks, ring2_radius, ring2_height, ring2_axis, "Ring2");
        
        if (ring3_active)
            ring3Objects = CreateRing(ring3_rocks, ring3_radius, ring3_height, ring3_axis, "Ring3");
    }

    GameObject[] CreateRing(int count, float radius, float height, Vector3 axis, string name)
    {
        GameObject[] objects = new GameObject[count];
        float angleStep = 360f / count;

        for (int i = 0; i < count; i++)
        {
            float angle = i * angleStep;
            float radian = angle * Mathf.Deg2Rad;
            
            Vector3 position;
            if (axis == Vector3.up)
            {
                position = planet.position + new Vector3(
                    Mathf.Cos(radian) * radius,
                    height,
                    Mathf.Sin(radian) * radius
                );
            }
            else if (axis == Vector3.right)
            {
                position = planet.position + new Vector3(
                    height,
                    Mathf.Cos(radian) * radius,
                    Mathf.Sin(radian) * radius
                );
            }
            else
            {
                position = planet.position + new Vector3(
                    Mathf.Cos(radian) * radius,
                    height,
                    Mathf.Sin(radian) * radius
                );
            }
            
            objects[i] = Instantiate(rockPrefab, position, Quaternion.identity);
            objects[i].transform.parent = this.transform;
            objects[i].name = name + "_Rock_" + i;
        }
        
        return objects;
    }

    void Update()
    {
        RotateRing(ring1Objects, ring1_speed, ring1_axis);
        
        if (ring2_active && ring2Objects != null)
            RotateRing(ring2Objects, ring2_speed, ring2_axis);
        
        if (ring3_active && ring3Objects != null)
            RotateRing(ring3Objects, ring3_speed, ring3_axis);
    }

    void RotateRing(GameObject[] objects, float speed, Vector3 axis)
    {
        if (objects == null) return;
        
        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i] != null)
            {
                objects[i].transform.RotateAround(planet.position, axis, speed * Time.deltaTime);
            }
        }
    }
}