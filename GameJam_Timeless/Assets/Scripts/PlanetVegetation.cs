using UnityEngine;
using System.Collections.Generic;

public class PlanetVegetation : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private GameObject plantPrefab;
    [SerializeField] private int plantCount = 50;
    [SerializeField] private float minDistance = 0f;
    [SerializeField] private float plantScale = 0.1f;
    [SerializeField] private float scaleVariationMin = 0.8f;
    [SerializeField] private float scaleVariationMax = 1.2f;
    [SerializeField] private bool spawnOnStart = true;

    private List<GameObject> spawnedPlants = new List<GameObject>();
    private float planetRadius;

    void Start()
    {
        // Get planet radius from scale
        planetRadius = transform.localScale.x / 2f;

        if (spawnOnStart)
        {
            SpawnPlants();
        }
    }

    public void SpawnPlants()
    {
        if (plantPrefab == null)
        {
            Debug.LogError("Plant Prefab nicht zugewiesen!");
            return;
        }

        ClearPlants();

        List<Vector3> spawnedPositions = new List<Vector3>();

        for (int i = 0; i < plantCount; i++)
        {
            int attempts = 0;
            bool validPosition = false;
            Vector3 position = Vector3.zero;
            Vector3 normal = Vector3.zero;

            // Versuche eine gültige Position zu finden
            while (!validPosition && attempts < 50)
            {
                // Zufällige sphärische Koordinaten
                float theta = Random.Range(0f, Mathf.PI * 2f);
                float phi = Mathf.Acos(2f * Random.Range(0f, 1f) - 1f);

                // Umwandlung in kartesische Koordinaten
                float x = planetRadius * Mathf.Sin(phi) * Mathf.Cos(theta);
                float y = planetRadius * Mathf.Sin(phi) * Mathf.Sin(theta);
                float z = planetRadius * Mathf.Cos(phi);

                position = new Vector3(x, y, z);

                // Prüfe Mindestabstand zu anderen Pflanzen
                validPosition = true;
                foreach (Vector3 pos in spawnedPositions)
                {
                    if (Vector3.Distance(position, pos) < minDistance)
                    {
                        validPosition = false;
                        break;
                    }
                }

                attempts++;
            }

            if (validPosition)
            {
                spawnedPositions.Add(position);

                // Berechne Normale (zeigt vom Planetenzentrum weg)
                normal = position.normalized;

                // Erstelle Pflanze
                GameObject plant = Instantiate(plantPrefab, transform);
                plant.transform.localPosition = position;

                // Richte Pflanze entlang der Normale aus
                plant.transform.rotation = Quaternion.FromToRotation(Vector3.up, normal);

                // Zufällige Rotation um die Normale herum
                float randomRotation = Random.Range(0f, 360f);
                plant.transform.Rotate(normal, randomRotation, Space.World);

                // Zufällige Skalierung
                float scaleVariation = Random.Range(scaleVariationMin, scaleVariationMax);
                plant.transform.localScale *= scaleVariation;

                spawnedPlants.Add(plant);
            }
        }

        Debug.Log($"{spawnedPlants.Count} Pflanzen auf Planet gespawnt");
    }

    public void ClearPlants()
    {
        foreach (GameObject plant in spawnedPlants)
        {
            if (plant != null)
            {
                if (Application.isPlaying)
                    Destroy(plant);
                else
                    DestroyImmediate(plant);
            }
        }
        spawnedPlants.Clear();
    }

    // Editor-Button zum Testen
    [ContextMenu("Spawn Plants")]
    void EditorSpawnPlants()
    {
        planetRadius = transform.localScale.x / 2f;
        SpawnPlants();
    }

    [ContextMenu("Clear Plants")]
    void EditorClearPlants()
    {
        ClearPlants();
    }
}

// VERWENDUNG:
// 1. Dieses Script auf dein Planet GameObject ziehen
// 2. Plant Prefab im Inspector zuweisen
// 3. Einstellungen anpassen (Anzahl, Mindestabstand, etc.)
// 4. Play drücken oder Rechtsklick auf Script -> "Spawn Plants"