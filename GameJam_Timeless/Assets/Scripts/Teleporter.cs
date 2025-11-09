using UnityEngine;
using System.Collections;

public class Teleporter : MonoBehaviour
{
    [Header("Ziel")]
    public Transform target;
    public Transform arrivalPlanet;   // <- hier den Ziel-Planeten zuweisen
    public GameObject currentPlanet;
    public ShadowSpawnerGlobal shadows;
    public float heightOffset = 1.5f;
    public float delaySeconds = 0.35f;
    public bool oneUse = false;

    [Header("Optional")]
    public bool faceTargetForward = true;

    bool used;

    void OnTriggerEnter(Collider other)
    {
        if (used && oneUse) return;
        StartCoroutine(TeleportAfterDelay(other));
        if (oneUse) used = true;
    }

    IEnumerator TeleportAfterDelay(Collider col)
    {
        yield return new WaitForSeconds(delaySeconds);
        if (target == null) yield break;
        
        // Before Teleportation stop recording and create shadow prefab
        shadows.GetComponent<ShadowSpawnerGlobal>().StopRecording();
        shadows.GetComponent<ShadowSpawnerGlobal>().CreateShadowPrefabs(currentPlanet.name);

        Transform t = col.transform;
        Vector3 dest = target.position + Vector3.up * heightOffset;

        var cc = t.GetComponent<CharacterController>();
        var rb = t.GetComponent<Rigidbody>();

        // Bewegung kurz einfrieren
        if (cc != null) cc.enabled = false;
        if (rb != null) rb.isKinematic = true;

        // Position + Blickrichtung
        t.position = dest;
        if (faceTargetForward) t.rotation = target.rotation;

        // Ziel-Planet an den Player melden
        var fc = t.GetComponent<FixedController>();
        if (fc != null)
        {
            // bevorzugt explizit zugewiesener Planet
            Transform planetT = arrivalPlanet != null ? arrivalPlanet : FindClosestPlanet(t.position);
            if (planetT != null)
            {
                fc.OnTeleported(planetT.gameObject);
                // After Teleportation start recording again
                shadows.GetComponent<ShadowSpawnerGlobal>().StartRecording();
            }
        }

        if (rb != null) rb.isKinematic = false;
        if (cc != null) cc.enabled = true;
    }

    // Fallback: nächsten Planet anhand Tag "Planet" suchen
    Transform FindClosestPlanet(Vector3 pos)
    {
        GameObject[] planets = GameObject.FindGameObjectsWithTag("Planet");
        if (planets.Length == 0) return null;
        float best = float.PositiveInfinity;
        Transform bestT = null;
        foreach (var p in planets)
        {
            float d = (p.transform.position - pos).sqrMagnitude;
            if (d < best) { best = d; bestT = p.transform; }
        }
        return bestT;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (target == null) return;
        Gizmos.color = Color.cyan;
        Vector3 dest = target.position + Vector3.up * heightOffset;
        Gizmos.DrawWireSphere(dest, 0.3f);
        Gizmos.DrawLine(transform.position, dest);
    }
#endif
}
