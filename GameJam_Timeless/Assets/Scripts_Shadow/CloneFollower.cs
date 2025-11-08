using UnityEngine;
using System.Collections.Generic;

public class CloneFollower : MonoBehaviour
{
    public float moveSpeed = 5f; // TODO change to player speed | or sth that feels good
    private List<Vector3> _path;
    private int _currentIndex = 0;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (_path == null || _currentIndex >= _path.Count) return;

        Vector3 target = _path[_currentIndex];
        transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
        
        // The if is there, because MoveTowards might not reach the target in one frame (tolerance value)
        // So it goes to the next target only when it is close enough to the current target
        if (Vector3.Distance(transform.position, target) < 0.1f)
        {
            _currentIndex++;
        }
    }

    /*
     * Sets the path for the clone to follow
     * @param recordedPath The path recorded by the PathRecorder (Position)
     */
    public void SetPath(List<Vector3> recordedPath)
    {
        _path = recordedPath;
    }
}
