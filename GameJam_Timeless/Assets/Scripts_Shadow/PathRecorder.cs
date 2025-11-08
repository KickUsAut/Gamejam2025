using UnityEngine;
using System.Collections.Generic;

public class PathRecorder : MonoBehaviour
{
    public float recordInterval = 0.1f;
    private List<Vector3> _recordedPositions = new List<Vector3>();
    // TODO later add a List<Vector3> for rotation (for planet)
    private float _timer = 0f;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // TODO later change to start Recording and not do it in Update
        _timer += Time.deltaTime;
        if (_timer >= recordInterval)
        {
            _recordedPositions.Add(transform.position);
            _timer = 0f;
        }
    }
    
    /* 
     * Returns a copy of the recorded positions
     * @return List of recorded Vector3 positions
     */
    public List<Vector3> GetRecordedPositions()
    {
        return new List<Vector3>(_recordedPositions);
    }
    
    /* 
     * Resets the recorded positions => used to reset and track again
     */
    public void ResetRecording()
    {
        _recordedPositions.Clear();
        _timer = 0f;
    }
}
