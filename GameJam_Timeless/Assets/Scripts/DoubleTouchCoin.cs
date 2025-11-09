using UnityEngine;

public class DoubleTouchCoinTimer : MonoBehaviour
{
    public Color firstTouchColor = Color.white;
    public float cooldownTime = 0.5f; // Zeit zwischen Touches
    
    private int touchCount = 0;
    private Renderer rend;
    private float lastTouchTime = -999f;
    
    private void Start()
    {
        rend = GetComponent<Renderer>();
    }
    
    private void OnTriggerEnter(Collider collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Prüfen ob genug Zeit seit letztem Touch vergangen ist
            if (Time.time - lastTouchTime >= cooldownTime)
            {
                touchCount++;
                lastTouchTime = Time.time;
                
                if (touchCount == 1)
                {
                    // Erster Touch - Farbe ändern
                    if (rend != null)
                    {
                        rend.material.color = firstTouchColor;
                    }
                }
                else if (touchCount >= 2)
                { 
                    // Coin zerstören
                    Destroy(gameObject);
                }
            }
        }
    }
}