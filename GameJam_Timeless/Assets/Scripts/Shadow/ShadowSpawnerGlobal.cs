using UnityEngine;
using System.Collections.Generic;

public class ShadowSpawnerGlobal : MonoBehaviour
{
    [SerializeField] [Tooltip("Player reference who needs PathRecorder Script")]
    public PathRecorder recorder;
    [SerializeField] [Tooltip("Shadow prefab to spawn")]
    public GameObject shadowPrefab; // Shadow prefab to spawn
    private GameObject shadow;
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // game start with recording
        // StartRecording();
    }

    // Update is called once per frame
    void Update()
    {
        // Only for testing purpose: Press J to start recording ==> Start recording
        // TODO change to reset AFTER new planet enter.
        if (Input.GetKeyDown(KeyCode.J))
        {
            StartRecording();
        }
        
        // Only for testing purpose: Press Space to let the Shadow start running
        // TODO change to reset AFTER new planet enter.
        if (Input.GetKeyDown(KeyCode.Space))
        {
            PlayShadow();
        }
        
        // Only for testing purposes: Press L to to stop recording and save it on shadow === Stop recording and save
        // TODO change to swap BEFORE new planet enter.
        if (Input.GetKeyDown(KeyCode.L))
        {
            StopRecording();
            
            // save new path
            SavePath();
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

    public void SavePath()
    {
        // Avoid duplicate cursed empty shadows because functions are called to often in Teleporter
        if (recorder.GetRecordedPositions() == null || recorder.GetRecordedPositions().Count == 0) return;
        
        List<Vector3> recordedPositions = recorder.GetRecordedPositions();
        List<Quaternion> recordedRotations = recorder.GetRecordedRotations();
        if (shadow == null)
        {
            // Create a shadow if there isn't already one
            shadow = Instantiate(shadowPrefab, recordedPositions[0], recordedRotations[0]);
        }
        else
        {
            shadow.transform.position = recordedPositions[0];
            shadow.transform.rotation = recordedRotations[0];
        }

        ShadowFollower shadowFollower = shadow.GetComponent<ShadowFollower>();
        
        shadowFollower.SetPath(recordedPositions, recordedRotations);
    }
    
    public void PlayShadow()
    {
        if (shadow == null) return;
        shadow.GetComponent<ShadowFollower>().ResetPath();
    }
}
