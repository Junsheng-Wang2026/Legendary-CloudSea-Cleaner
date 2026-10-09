using UnityEngine;
using UnityEngine.SceneManagement;

public class GoToBoot : MonoBehaviour
{
    public void LoadBoot()
    {
        SceneManager.LoadScene("Boot");
    }
}
