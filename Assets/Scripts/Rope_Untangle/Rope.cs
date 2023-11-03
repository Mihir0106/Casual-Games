using UnityEngine;

namespace Rope_Untangle
{
    public class Rope : MonoBehaviour
    {
        public int currentPos;
        public bool inAir;
        private RU_GameManager _gameManager;
        
        private void Start()
        {
            _gameManager = RU_GameManager.Instance;
        }

        private void OnMouseDown()
        {
            if(_gameManager.midAirRope == null)
            {
                _gameManager.MoveUp(gameObject);
                inAir = true;
            }
        }
        
        public void PrintCurrentPos()
        {
            Debug.Log("Currently player at : " + currentPos);
        }
    }
}


/*
private void OnMouseUp()
    {
        var position1 = _cam.transform.position;
        var rayOrigin = position1;
        var rayDirection = MouseWorldPosition() - position1;

        if (Physics.Raycast(rayOrigin, rayDirection, out var htiInfo))
        {
            if (htiInfo.transform.CompareTag(DestinationTag))
            {
                int num = htiInfo.transform.gameObject.GetComponent<PlaceableCoords>().coord;
                Debug.Log("target Place Coords : " + num);
                _gameManager.Move(num);
            }
        }
    }

    private Vector3 MouseWorldPosition()
    {
        var mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = _cam.WorldToScreenPoint(transform.position).z;
        return _cam.ScreenToWorldPoint(mouseScreenPos);
    }
*/
