using UnityEngine;
using System.Collections.Generic;

public class ShadowSpawnerGlobal : MonoBehaviour
{
    [SerializeField] [Tooltip("Player reference who needs PathRecorder Script")]
    public PathRecorder recorder;
    [SerializeField] [Tooltip("Shadow prefab to spawn")]
    public GameObject shadowPrefab; // Shadow prefab to spawn
    private Dictionary<string, GameObject> shadowPrefabs = new Dictionary<string, GameObject>();
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // game start with recording
        StartRecording();
    }

    // Update is called once per frame
    void Update()
    {
        // Only for testing purpose: Press F to start recording == Start recording
        // TODO change to reset AFTER new planet enter.
        if (Input.GetKeyDown(KeyCode.F))
        {
            StartRecording();
        }
        
        // Only for testing purpose: Press G to replay all Recorded
        // TODO change to reset AFTER new planet enter.
        if (Input.GetKeyDown(KeyCode.G))
        {
            ReplayAllRecorded();
        }
        
        // Only for testing purposes: Press H to spawn a shadow === Stop recording and play
        // TODO change to swap BEFORE new planet enter.
        if (Input.GetKeyDown(KeyCode.H))
        {
            StopRecording();
            CreateShadowPrefabs();
        }
    }
    
    public void StartRecording()
    {
        recorder.StartRecording();
    }

    public void ReplayAllRecorded()
    {
        foreach (GameObject shadow in shadowPrefabs.Values)
        {
            shadow.GetComponent<ShadowFollower>().ResetPath();
        }
    }
    
    public void StopRecording()
    {
        recorder.StopRecording();
    }

    public void CreateShadowPrefabs(string name="default")
    {
        // Avoid duplicate cursed empty shadows because functions are called to often in Teleporter
        if (recorder.GetRecordedPositions() == null || recorder.GetRecordedPositions().Count == 0) return;
        
        List<Vector3> recordedPositions = recorder.GetRecordedPositions();
        List<Quaternion> recordedRotations = recorder.GetRecordedRotations();
        GameObject shadow = Instantiate(shadowPrefab, recordedPositions[0], recordedRotations[0]);
        shadow.GetComponent<ShadowFollower>().SetPath(recordedPositions, recordedRotations);
        
        shadowPrefabs[name] = shadow;
    }
}
