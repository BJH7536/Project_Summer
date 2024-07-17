using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Topping : Holdable
{
    [SerializeField] private Define.Topping _type;
    
    private int FloorLayer;
    private int InteractableLayer;

    private void Awake()
    {
        FloorLayer = LayerMask.NameToLayer("Floor");
        InteractableLayer = LayerMask.NameToLayer("Interactable");
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.layer == FloorLayer)
        {
            Destroy(gameObject);
        } 
        else if (other.gameObject.layer == InteractableLayer && other.gameObject.CompareTag("Dough"))
        {
            // 도우의 stack에 나를 추가
            Dough dough = other.gameObject.GetComponent<Dough>();
            dough.AddTopping(_type);
            
            transform.SetParent(dough.transform);
            transform.localScale *= 0.5f;

            Destroy(this);
            Destroy(GetComponent<Rigidbody>());
            Destroy(GetComponent<Collider>());
        }
    }

    #region Holdable
    public override void Hold(Player holder)
    {
        holder.HoldTopping(this);
    }

    public override void Release(Player holder)
    {
        holder.ReleaseTopping(this);
    }
    
    #endregion
}
