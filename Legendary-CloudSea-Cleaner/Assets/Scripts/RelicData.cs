using UnityEngine;

[CreateAssetMenu(fileName = "NewRelic", menuName = "Cleaner/Relic Data")]
public class RelicData : ScriptableObject
{
    public string relicName;
    [TextArea] public string description;
    public enum EffectType { ExtraDraw, ExtraBlock, HealTurn }
    public EffectType effect;
    public int value;  // 效果数值
}
