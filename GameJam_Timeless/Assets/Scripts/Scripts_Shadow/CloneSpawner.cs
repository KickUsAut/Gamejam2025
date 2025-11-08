using System;
using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(PathRecorder))]
public class CloneSpawner : MonoBehaviour
{
    public GameObject clonePrefab; // Clone prefab to spawn
    private PathRecorder _recorder;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _recorder = GetComponent<PathRecorder>();
    }

    // Update is called once per frame
    void Update()
    {
        // Only for testing purposes: Press H to spawn a clone === Stop recording and play
        // TODO change to swap BEFORE new planet enter.
        if (Input.GetKeyDown(KeyCode.H))
        {
            _recorder.StopRecording();
            SpawnClone();
        }
        
        // Only for testing purpose: Press F to start recording == Start recording
        // TODO change to reset AFTER new planet enter.
        if (Input.GetKeyDown(KeyCode.F))
        {
            _recorder.StartRecording();
        }
    }
    
    /**
     * Spawns a clone at the start of the recorded path
     * and assigns the recorded path to the clone to follow
     */
    void SpawnClone()
    {
        List<Vector3> positions = _recorder.GetRecordedPositions();
        List<Quaternion> rotations = _recorder.GetRecordedRotations();
        if (positions.Count == 0 || rotations.Count == 0) return; // shouldn't happen. but safety first.

        Vector3 startPosition = positions[0];
        Quaternion startRotation = rotations[0];
        GameObject clone = Instantiate(clonePrefab, startPosition, startRotation);
        
        CloneFollower follower = clone.AddComponent<CloneFollower>();
        follower.SetPath(positions, rotations);
    }
}
