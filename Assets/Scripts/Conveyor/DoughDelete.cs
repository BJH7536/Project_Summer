using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoughDelete : MonoBehaviour
{
    [SerializeField] private PizzaScoreData pizzaScoreData;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Dough"))
        {
            Dough dough = other.gameObject.GetComponent<Dough>();
            pizzaScoreData.CalculateTotalScore(dough.Type, dough.GetAllToppings());
            
            Destroy(other.gameObject);
        }
    }
}
