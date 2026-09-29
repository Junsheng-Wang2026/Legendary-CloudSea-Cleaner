using UnityEngine;
using UnityEngine.SceneManagement;

public class MapNode : MonoBehaviour
{
    public GameObject eventPanel;   // мо EventPanel

    public void OnClickNode()
    {
        eventPanel.SetActive(true);
    }

    public void OnClickFight()
    {
        SceneManager.LoadScene("Combat");
    }

    public void OnClickRun()
    {
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.SpendTime(20f);
        }
        eventPanel.SetActive(false);
    }
}
