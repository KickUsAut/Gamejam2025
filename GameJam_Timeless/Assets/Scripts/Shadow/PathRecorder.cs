using UnityEngine;
using System.Collections.Generic;

/**
 * This script records the positions and rotations of the GameObject it is attached to over time.
 * It can be started and stopped via public methods.
 * - Needs to be attached to the Player GameObject.
 */
public class PathRecorder : MonoBehaviour
{
    public float recordInterval = 0.1f;
    private List<Vector3> _recordedPositions = new List<Vector3>();
    private List<Quaternion> _recordedRotations = new List<Quaternion>();
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
            _recordedRotations.Add(transform.rotation);
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
     * Returns a copy of the recorded rotations
     * @return List of recorded Quaternion rotations
     */
    public List<Quaternion> GetRecordedRotations()
    {
        return new List<Quaternion>(_recordedRotations);
    }
    
    /** 
     * Starts recording the positions
     */
    public void StartRecording()
    {
        _recordedPositions.Clear();
        _recordedRotations.Clear();
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
