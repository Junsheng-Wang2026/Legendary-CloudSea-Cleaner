using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Checkpoint : MonoBehaviour
{
    public TMP_Text messageText;  // 拖一个提示文字

    void Start()
    {
        gameObject.SetActive(true);
    }

    public void OnClickCheckpoint()
    {
        // 回满血（通过 GameManager 存全局血量）
        if (GameManager.Instance != null)
        {
            GameManager.Instance.currentHP = GameManager.Instance.maxHP;
        }

        // 显示提示
        if (messageText != null)
        {
            messageText.text = "Checkpoint! HP restored.";
            messageText.gameObject.SetActive(true);
        }
    }
}
