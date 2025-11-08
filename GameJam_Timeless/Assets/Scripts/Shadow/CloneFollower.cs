using UnityEngine;
using System.Collections.Generic;

public class CloneFollower : MonoBehaviour
{
    public float moveSpeed = 5f; // TODO change to player speed | or sth that feels good
    public float rotationSpeed = 5f; // TODO change to player rotation speed | or sth that feels good
    private List<Vector3> _pathPosition;
    private List<Quaternion> _pathRotations;
    private int _currentIndex = 0;

    // Update is called once per frame
    void Update()
    {
        if (HasStartedPath() || HasFinishedPath()) return;

        Vector3 targetPosition = _pathPosition[_currentIndex];
        Quaternion targetRotation = _pathRotations[_currentIndex];
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        
        // The if is there, because MoveTowards might not reach the target in one frame (tolerance value)
        // So it goes to the next target only when it is close enough to the current target
        // Could also check for rotation difference, but position is important and enough for now
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            _currentIndex++;
        }
    }

    /**
     * Checks if the clone has started following the path
     * @return true if the path is not set, false otherwise
     */
    private bool HasStartedPath()
    {
        return _pathPosition == null || _pathRotations == null;
    }
    
    /**
     * Checks if the clone has finished following the path
     * @return true if the clone has reached the end of the path, false otherwise
     */
    private bool HasFinishedPath()
    {
        return _currentIndex >= _pathPosition.Count || _currentIndex >= _pathRotations.Count;
    }

    /**
     * Sets the path for the clone to follow
     * @param recordedPath The path recorded by the PathRecorder (Position)
     */
    public void SetPath(List<Vector3> recordedPathPosition, List<Quaternion> recordedPathRotations)
    {
        _pathPosition = recordedPathPosition;
        _pathRotations = recordedPathRotations;
    }
}
