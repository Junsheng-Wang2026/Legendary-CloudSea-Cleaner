using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CardRewardPanel : MonoBehaviour
{
    public GameObject cardPrefab;       // 拖 Card1 卡牌预制体
    public Transform cardContainer;     // 拖卡牌容器
    public Button skipButton;           // 跳过按钮
    public CombatManager combatManager;

    List<CardData> _options = new List<CardData>();
    List<GameObject> _spawned = new List<GameObject>();

    void Awake()
    {
        if (skipButton != null) skipButton.onClick.AddListener(OnFinished);
    }

    public void ShowRewards(List<CardData> pool)
    {
        gameObject.SetActive(true);
        ClearSpawned();
        _options.Clear();

        // 从奖池随机抽3张不重复
        List<CardData> temp = new List<CardData>(pool);
        int count = Mathf.Min(3, temp.Count);
        for (int i = 0; i < count; i++)
        {
            int idx = Random.Range(0, temp.Count);
            _options.Add(temp[idx]);
            temp.RemoveAt(idx);
        }

        // 用手牌预制体生成选项
        for (int i = 0; i < _options.Count; i++)
        {
            GameObject obj = Instantiate(cardPrefab, cardContainer);
            CardButton cb = obj.GetComponent<CardButton>();
            cb.enableHoverRaise = false;  // 奖励牌不置顶，避免布局鬼畜
            cb.SetCard(_options[i], null);  // combatManager传null，点击不会打出

            Button btn = obj.GetComponent<Button>();
            int index = i;
            if (btn != null) btn.onClick.AddListener(() => ChooseCard(index));

            _spawned.Add(obj);
        }
    }

    void ClearSpawned()
    {
        foreach (GameObject o in _spawned)
        {
            if (o != null) Destroy(o);
        }
        _spawned.Clear();
    }

    void ChooseCard(int index)
    {
        if (index < _options.Count && GameManager.Instance != null)
        {
            GameManager.Instance.playerDeck.Add(_options[index]);
            Debug.Log("选择奖励牌: " + _options[index].cardName);
        }
        OnFinished();
    }

    void OnFinished()
    {
        gameObject.SetActive(false);
        if (combatManager != null) combatManager.ShowVictoryAfterReward();
    }
}
