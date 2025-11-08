using UnityEngine;
using System.Collections.Generic;

public class PathRecorder : MonoBehaviour
{
    public float recordInterval = 0.1f;
    private List<Vector3> _recordedPositions = new List<Vector3>();
    // TODO later add a List<Vector3> for rotation (for planet)
    private float _timer = 0f;
    private bool _isRecording = false;

    // Update is called once per frame
    void Update()
    {
        if (!_isRecording) return;
        
        _timer += Time.deltaTime;
        if (_timer >= recordInterval)
        {
            _recordedPositions.Add(transform.position);
            _timer = 0f;
        }
    }
    
    /** 
     * Returns a copy of the recorded positions
     * @return List of recorded Vector3 positions
     */
    public List<Vector3> GetRecordedPositions()
    {
        return new List<Vector3>(_recordedPositions);
    }
    
    /** 
     * Starts recording the positions
     */
    public void StartRecording()
    {
        _recordedPositions.Clear();
        _timer = 0f;
        _isRecording = true;
    }
    
    /** 
     * Ends recording and clears the recorded positions
     */
    public void StopRecording()
    {
        _isRecording = false;
    }
}
