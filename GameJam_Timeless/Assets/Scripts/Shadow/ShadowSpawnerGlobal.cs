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
    
    void StartRecording()
    {
        recorder.StartRecording();
    }

    void ReplayAllRecorded()
    {
        foreach (GameObject shadow in shadowPrefabs.Values)
        {
            shadow.GetComponent<ShadowFollower>().ResetPath();
        }
    }
    
    void StopRecording()
    {
        recorder.StopRecording();
    }

    void CreateShadowPrefabs(string name="default")
    {
        List<Vector3> recordedPositions = recorder.GetRecordedPositions();
        List<Quaternion> recordedRotations = recorder.GetRecordedRotations();
        GameObject shadow = Instantiate(shadowPrefab, recordedPositions[0], recordedRotations[0]);
        shadow.GetComponent<ShadowFollower>().SetPath(recordedPositions, recordedRotations);
        
        shadowPrefabs[name] = shadow;
    }
}
