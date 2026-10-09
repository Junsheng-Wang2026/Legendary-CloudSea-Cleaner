using UnityEngine;

[CreateAssetMenu(fileName = "NewNode", menuName = "Cleaner/Node Data")]
public class NodeData : ScriptableObject
{
    public string nodeName;
    public enum NodeType { Fight, Event }
    public NodeType type;
    public int depthCost = 20;  // 过这个节点扣多少高度
}
