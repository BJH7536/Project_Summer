using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ToppingBox : Holdable
{
    [SerializeField] private List<GameObject> Topping;
    
    #region Holdable
    public override void Hold(Player holder)
    {
        Holdable topping = Instantiate(Topping[Random.Range(0, Topping.Count)]).GetComponent<Holdable>();
        
        holder.HoldTopping(topping);
    }

    public override void Release(Player holder)
    {
        
    }
    
    #endregion
}
