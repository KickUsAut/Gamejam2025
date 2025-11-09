using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(LoadEndScene());
        }
    }

    private IEnumerator LoadEndScene()
    {
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene("End");
    }
}
