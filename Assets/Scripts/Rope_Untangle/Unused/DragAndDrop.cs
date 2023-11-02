using System;
using System.Collections;
using System.Collections.Generic;
using Rope_Untangle;
using UnityEngine;

public class DragAndDrop : MonoBehaviour
{
    private Vector3 offset;
    public String Destinationtag = "DropArea";
    RU_GameManager RuGameManager;

    private void Start()
    {
        if (RuGameManager != null)
        {
            RuGameManager = RU_GameManager.Instance;
        }
    }
    
    private void OnMouseDown()
    {
        int current = gameObject.GetComponent<Rope>().currentPos;
        //RuGameManager.MoveUpward(Current);
    }

    private void OnMouseUp()
    {
        var rayOrigin = Camera.main.transform.position;
        var rayDirection = MouseWorldPosition() - Camera.main.transform.position;
        RaycastHit htiInfo;
        if (Physics.Raycast(rayOrigin, rayDirection, out htiInfo))
        {
            if (htiInfo.transform.CompareTag(Destinationtag))
            {
                var position = htiInfo.transform.position;
                transform.position = new Vector3(position.x,transform.position.y,position.z);
                
            }
        }

        transform.GetComponent<Collider>().enabled = true;
        
        var init = transform.position;
        transform.position = new Vector3(init.x, init.y - 0.25f, init.z);
    }
    private Vector3 MouseWorldPosition()
    {
        var mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = Camera.main.WorldToScreenPoint(transform.position).z;
        return Camera.main.ScreenToWorldPoint(mouseScreenPos);
    }

    
}
