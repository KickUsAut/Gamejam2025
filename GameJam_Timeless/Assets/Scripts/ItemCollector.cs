using UnityEngine;
using TMPro;

public class ItemCollector : MonoBehaviour
{
    public int score = 0;
    public int pointsPerItem = 100;
    private TMP_Text scoreText;
    
    private float cooldownTime = 0.1f;
    private float lastCollectTime = -1f;

    void Start()
    {
        // Text automatisch finden, falls nicht zugewiesen
        if (scoreText == null)
        {
            scoreText = GameObject.Find("ScoreText").GetComponent<TMP_Text>();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Item") && Time.time > lastCollectTime + cooldownTime)
        {
            score += pointsPerItem;
            scoreText.text = score.ToString();
            lastCollectTime = Time.time;
            Destroy(other.gameObject);
        }
    }
}