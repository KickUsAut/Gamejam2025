using UnityEngine;

public class FLO_CustomGravity : MonoBehaviour
{
    public static Vector3 planetCenter = Vector3.zero;

    void Awake() {
        // Setze das Planeten-Zentrum auf die Position dieses GameObjects
        planetCenter = transform.position;
    }

    public static Vector3 GetUpAxis (Vector3 position) {
        Vector3 up = (position - planetCenter).normalized;
        return Physics.gravity.y < 0f ? up : -up;
    }

    public static Vector3 GetGravity (Vector3 position) {
        Vector3 up = (position - planetCenter).normalized;
        return -up * Mathf.Abs(Physics.gravity.y);
    }
    
    public static Vector3 GetGravity (Vector3 position, out Vector3 upAxis) {
        Vector3 up = (position - planetCenter).normalized;
        upAxis = up;
        return -up * Mathf.Abs(Physics.gravity.y);
    }
}