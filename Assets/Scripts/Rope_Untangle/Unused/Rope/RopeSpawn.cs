using UnityEngine;

namespace Rope_Untangle.Unused.Rope
{
    public class RopeSpawn : MonoBehaviour
    {
        [SerializeField] private GameObject[] starting;
        [SerializeField] private GameObject[] ending;

        [SerializeField] GameObject partPrefab, parentObject;
        [SerializeField] [Range(1, 1000)] int length = 1;
        [SerializeField] float partDistance = 0.21f;

        [SerializeField] private bool reset, spawn;


        // Update is called once per frame
        void Update()
        {
            if (reset)
            {
                foreach (GameObject tmp in GameObject.FindGameObjectsWithTag("Player"))
                {
                    Destroy(tmp);
                }

                reset = false;
            }

            if (spawn)
            {
                Spawn(starting[0],ending[0]);
                spawn = false;
            }
        }

        public void Spawn(GameObject start,GameObject end)
        {
            var startingPos = start.transform;
            int count = (int)(length / partDistance);
            for (int x = 0; x < count; x++)
            {
                GameObject tmp;
                var position = startingPos.position;
                tmp = Instantiate(partPrefab,
                    new Vector3(position.x, transform.position.y + partDistance * (x + 1), position.z),
                    Quaternion.identity, parentObject.transform);
                tmp.transform.eulerAngles = new Vector3(180, 0, 0);

                tmp.name = parentObject.transform.childCount.ToString();
                if (x == 0)
                    Destroy(tmp.GetComponent<CharacterJoint>());
                else
                    tmp.GetComponent<CharacterJoint>().connectedBody = parentObject.transform
                        .Find((parentObject.transform.childCount - 1).ToString()).GetComponent<Rigidbody>();
            }
            
        }
    }
}