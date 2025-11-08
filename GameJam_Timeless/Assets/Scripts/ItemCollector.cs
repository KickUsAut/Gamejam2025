using UnityEngine;
using TMPro;

public class ItemCollector : MonoBehaviour
{
    public int itemCount;
    public TMP_Text itemText;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Item"))
        {
            itemCount++;
            itemText.text = itemCount.ToString();
            Destroy(other.gameObject);
        }
    }
}
