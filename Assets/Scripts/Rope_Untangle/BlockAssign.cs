using UnityEngine;

namespace Rope_Untangle
{
    public class BlockAssign : MonoBehaviour
    {
        public Block block;
        [SerializeField]private int index;
        [SerializeField]private BlockTag positionTag;

        private void Start()
        {
            GetComponent<MeshRenderer>().material = block.color;
            name = block.name;
            index = block.num;
            positionTag = block.tag;
        }
    }
}
