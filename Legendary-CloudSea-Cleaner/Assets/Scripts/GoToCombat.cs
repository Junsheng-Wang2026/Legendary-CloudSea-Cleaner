using UnityEngine;
using UnityEngine.SceneManagement;

public class GoToCombat : MonoBehaviour
{
    public void LoadCombat()
    {
        SceneManager.LoadScene("Combat");
    }
}
