using System.Collections.Generic;
using UnityEngine;

public class CombineArea : MonoBehaviour
{
    public List<string> requiredIngredients;
    public GameObject resultPrefab;

    private List<GameObject> currentIngredients = new List<GameObject>();

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Ingredients ingredient = collision.GetComponent<Ingredients>();

        if (ingredient != null)
        {
            if (!currentIngredients.Contains(collision.gameObject))
            {
                currentIngredients.Add(collision.gameObject);
                CheckCombination();
            }
        }
    }

    void CheckCombination()
    {
        List<string> collectedIDs = new List<string>();

        foreach (GameObject obj in currentIngredients)
        {
            collectedIDs.Add(obj.GetComponent<Ingredients>().ingredientID);
        }

        foreach (string required in requiredIngredients)
        {
            if (!collectedIDs.Contains(required))
                return;
        }

        Combine();
    }

    void Combine()
    {
        foreach (GameObject obj in currentIngredients)
        {
            Destroy(obj);
        }

        Instantiate(resultPrefab, transform.position, Quaternion.identity);

        currentIngredients.Clear();
    }
}
