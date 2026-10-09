using UnityEngine;

public class TestDepthButton : MonoBehaviour
{
    public HeightMeter meter;

    public void OnClickAddDepth()
    {
        meter.AddDepth(20);
    }
}
