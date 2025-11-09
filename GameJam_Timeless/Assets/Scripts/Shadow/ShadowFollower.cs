using System;
using UnityEngine;
using System.Collections.Generic;

/**
 * This script makes the clone follow a recorded path of positions and rotations.
 * It should be attached to the clone GameObject Automatically when spawned.
 * So no manual needed for it.
 */
public class ShadowFollower : MonoBehaviour
{
    public float moveSpeed = 5f; // TODO change to player speed | or sth that feels good
    public float rotationSpeed = 5f; // TODO change to player rotation speed | or sth that feels good
    private List<Vector3> _pathPosition;
    private List<Quaternion> _pathRotations;
    private int _currentIndex = 0;

    private void Start()
    {
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (HasStartedPath()) return;

        if (HasFinishedPath()) 
        {
            DestroyClone();
            return;
        }
        
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
    public bool HasStartedPath()
    {
        return _pathPosition == null || _pathRotations == null || 
            _pathPosition.Count == 0 || _pathRotations.Count == 0;
    }
    
    /**
     * Checks if the clone has finished following the path
     * @return true if the clone has reached the end of the path, false otherwise
     */
    public bool HasFinishedPath()
    {
        return _currentIndex >= _pathPosition.Count || _currentIndex >= _pathRotations.Count;
    }

    /**
     * Sets the path for the clone to follow
     * @param recordedPath The path recorded by the PathRecorder (Position)
     */
    public void SetPath(List<Vector3> recordedPathPosition, List<Quaternion> recordedPathRotations)
    {
        _currentIndex = 0;
        _pathPosition = recordedPathPosition;
        _pathRotations = recordedPathRotations;
    }
    
    /**
     * Destroys the clone game object
     */
    public void DestroyClone()
    {
        // Destroy(gameObject);
        gameObject.SetActive(false);
    }
    
    /**
     * Resets the clone to the start of the path and reactivates it
     */
    public void ResetPath()
    {
        _currentIndex = 0;
        transform.position = _pathPosition[_currentIndex];
        transform.rotation = _pathRotations[_currentIndex];
        gameObject.SetActive(true);
    }
}
