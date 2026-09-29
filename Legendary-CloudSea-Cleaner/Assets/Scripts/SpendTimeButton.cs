using UnityEngine;

/// <summary>
/// 挂在测试按钮上。点一下扣 1 分钟。
/// 按钮的 OnClick 在 Inspector 里绑：把挂这个脚本的物体拖进去，选 SpendTimeButton.Spend1Minute。
/// </summary>
public class SpendTimeButton : MonoBehaviour
{
    public void Spend1Minute()
    {
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.SpendTime(1f);
        }
    }
}
