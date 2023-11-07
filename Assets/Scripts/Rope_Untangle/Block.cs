using UnityEngine;

namespace Rope_Untangle
{
    public enum BlockTag
    {
        Starting,
        Ending
    }
    
    [CreateAssetMenu(fileName = "New Block",menuName = "Block")]
    public class Block : ScriptableObject
    {
        public new string name;
        public BlockTag tag;
        public Material color;
        public int num;
    }
}
