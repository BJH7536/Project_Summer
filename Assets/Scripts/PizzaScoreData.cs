using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "PizzaScoreData", menuName = "ScriptableObjects/PizzaScoreData", order = 1)]
public class PizzaScoreData : ScriptableObject
{
    // 피자의 종류별 토핑의 점수
    [System.Serializable]
    public class ToppingRecipe
    {
        public Define.PizzaType PizzaType;
        public int bulgogiPizzaScore;
        public int cheesePizzaScore;
        public int pepperoniPizzaScore;
        public int hawaiianPizzaScore;
        public int shrimpPizzaScore;
        public int mushroomPizzaScore;
        public int baconPizzaScore;

        // 조회를 위한 딕셔너리
        private Dictionary<Define.Topping, int> recipe;

        // 딕셔너리 초기화
        public void InitializeDictionary()
        {
            recipe = new Dictionary<Define.Topping, int>
            {
                { Define.Topping.Bulgogi, bulgogiPizzaScore },
                { Define.Topping.Cheese, cheesePizzaScore },
                { Define.Topping.Pepperoni, pepperoniPizzaScore },
                { Define.Topping.Pineapple, hawaiianPizzaScore },
                { Define.Topping.Shrimp, shrimpPizzaScore },
                { Define.Topping.Mushroom, mushroomPizzaScore },
                { Define.Topping.Bacon, baconPizzaScore }
            };
        }

        // 특정 토핑에 대한 점수를 반환
        public int GetScore(Define.Topping topping)
        {
            if (recipe.TryGetValue(topping, out int score))
            {
                return score;
            }
            
            Debug.LogWarning("Can't find topping point!");
            return 0;
        }
    }

    /// <summary>
    /// 토핑 점수 배열
    /// </summary>
    public ToppingRecipe[] toppingScores;
    
    /// <summary>
    /// 조회를 위한 딕셔너리
    /// </summary>
    private Dictionary<Define.PizzaType, ToppingRecipe> toppingScoreDict;

    private void OnEnable()
    {
        toppingScoreDict = new Dictionary<Define.PizzaType, ToppingRecipe>();
        foreach (ToppingRecipe toppingScore in toppingScores)
        {
            toppingScore.InitializeDictionary();
            toppingScoreDict[toppingScore.PizzaType] = toppingScore;
        }
    }

    // pizzaType 피자에 대한 topping 토핑의 점수는
    private int GetScore(Define.PizzaType pizzaType, Define.Topping topping)
    {
        // Evil 토핑에 대한 특별 처리
        if (topping == Define.Topping.Evil) return -50;
        
        if (toppingScoreDict.TryGetValue(pizzaType, out ToppingRecipe toppingScore))
        {
            return toppingScore.GetScore(topping);
        }
        
        return 0; // 토핑 점수를 찾지 못한 경우
    }
    
    // 피자의 총 점수를 계산
    public int CalculateTotalScore(Define.PizzaType pizzaType, Define.Topping[] toppings)
    {
        int totalScore = 0;
        foreach (Define.Topping topping in toppings)
        {
            // Evil 토핑이 있는 경우 총 점수를 -50으로 설정하고 종료
            if (topping == Define.Topping.Evil)
            {
                return -50;
            }

            totalScore += GetScore(pizzaType, topping);
        }
        
        Debug.Log($"{pizzaType.ToString()} 피자 주문에 대한 점수는 {totalScore}");
        
        return totalScore;
    }
}
