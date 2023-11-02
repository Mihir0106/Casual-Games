using UnityEngine;

namespace Rope_Untangle
{   
    public class PlaceableCoords : MonoBehaviour
    {
        public int coord;
        public bool occupied;
        private RU_GameManager _gameManager;
    
        private void Start()
        {
            _gameManager = RU_GameManager.Instance;
        }

        public void UpdateStatus(bool status)
        {
            occupied = status;
        }
        
        private void OnMouseDown()
        {
            if(occupied == false)
            {
                _gameManager.Move(coord);
                occupied = true;
            }
            else
            {
                _gameManager.MoveDown();
            }
        }
    }
}
