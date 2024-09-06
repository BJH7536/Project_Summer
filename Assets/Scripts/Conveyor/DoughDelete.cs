using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DoughDelete : MonoBehaviour
{
    [SerializeField] private PizzaScoreData pizzaScoreData;
    public TextMeshProUGUI scoreText;
    public GameObject PopupUI;
    private int score;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Dough"))
        {
            Dough dough = other.gameObject.GetComponent<Dough>();
            score = pizzaScoreData.CalculateTotalScore(dough.Type, dough.GetAllToppings());
            scoreText.text = score+"";
            
            PopupUI.SetActive(true);
            
            Destroy(other.gameObject);
        }
    }
}
