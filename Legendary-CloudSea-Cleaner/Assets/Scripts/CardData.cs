using UnityEngine;

[CreateAssetMenu(fileName = "NewCard", menuName = "Cleaner/Card Data")]
public class CardData : ScriptableObject
{
    public string cardName;        // ����
    public int cost;               // ����
    public int damage;             // �����Ѫ��0 = ����
    public int block; 
    public Sprite frame;             // �Ӷ��ٸ񵲣�0 = ����
    [TextArea] public string description;  // ��ť����ʾ����
}
