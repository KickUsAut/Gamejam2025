using System;
using UnityEngine;
using System.Collections.Generic;

/**
 * Script have to be placed on every Planet/Environment where a shadow shall spawn.
 * - Needs a Player who got the Script PathRecorder attached to it.
 * - Needs a Shadow Prefab to spawn.
 */
public class ShadowSpawner : MonoBehaviour
{
    [SerializeField] [Tooltip("Player reference who needs PathRecorder Script")]
    public PathRecorder recorder;
    [SerializeField] [Tooltip("Shadow prefab to spawn")]
    public GameObject shadowPrefab; // Shadow prefab to spawn
    [SerializeField] [Tooltip("Vertical offset applied along the recorded up direction when spawning the shadow")]
    public float spawnHeightOffset = 3.5f;
    private GameObject _currentShadow;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (recorder == null)
        {
            Debug.LogError("CloneSpawner: Player isn't assigned!");
        }
        if (shadowPrefab == null)
        {
            Debug.LogError("CloneSpawner: Shadow Prefab isn't assigned!");
        }
    }

    /**
     * Print Message if Player or Shadow Prefab isn't assigned
     */
    private void OnValidate()
    {
        if (recorder == null)
            Debug.LogWarning("CloneSpawner: Player isn't assigned!");
        if (shadowPrefab == null)
            Debug.LogWarning("CloneSpawner: Shadow Prefab isn't assigned!");
    }

    // Update is called once per frame
    void Update()
    {
        if (recorder == null || shadowPrefab == null) return;
        
        // Only for testing purposes: Press H to spawn a shadow === Stop recording and play
        // TODO change to swap BEFORE new planet enter.
        if (Input.GetKeyDown(KeyCode.H))
        {
            StopRecordingAndSpawnClone();
        }
        
        // Only for testing purpose: Press F to start recording == Start recording
        // TODO change to reset AFTER new planet enter.
        if (Input.GetKeyDown(KeyCode.F))
        {
            StartRecording();
        }
    }
    
    public void StartRecording()
    {
        recorder.StartRecording();
    }
    
    public void StopRecording()
    {
        recorder.StopRecording();
    }
    
    public void StopRecordingAndSpawnClone()
    {
        recorder.StopRecording();
        SpawnClone();
    }
    
    /**
     * Spawns a clone at the start of the recorded path
     * and assigns the recorded path to the clone to follow
     */
    public void SpawnClone()
    {
        // destroy old clone
        if (_currentShadow != null)
        {
            Destroy(_currentShadow);
        }
        
        List<Vector3> positions = recorder.GetRecordedPositions();
        List<Quaternion> rotations = recorder.GetRecordedRotations();
        if (positions.Count == 0 || rotations.Count == 0) return; // shouldn't happen. but safety first.

        Vector3 startPosition = positions[0];
        Quaternion startRotation = rotations[0];
        if (spawnHeightOffset != 0f)
        {
            startPosition += startRotation * Vector3.up * spawnHeightOffset;
        }

        _currentShadow = Instantiate(shadowPrefab, startPosition, startRotation);
        
        ShadowFollower follower = _currentShadow.AddComponent<ShadowFollower>();
        follower.SetPath(positions, rotations);
    }
}
