using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomWeightedSprite : MonoBehaviour {
	[System.Serializable]
	public struct SpriteWeight{
		public Sprite sprite;
		public float weight;
	}
	public SpriteWeight[] weights;
	void Start()
	{
		AssignSprite();
	}
	void AssignSprite(){
		SpriteRenderer rend = GetComponent<SpriteRenderer>();
		if (weights.Length == 0){
			Debug.Log("No SpriteWeights to choose from.");
			return;
		}
		float totalWeight = 0.0f;
		foreach (SpriteWeight thisWeight in weights)
		{
			totalWeight += thisWeight.weight;
		}
		float countedWeight = 0.0f;
		float targetWeight = Random.value * totalWeight;
		foreach (SpriteWeight thisWeight in weights){
			countedWeight += thisWeight.weight;
			if (targetWeight <= countedWeight){
				rend.sprite = thisWeight.sprite;
				return;
			}
		}
		rend.sprite = weights[weights.Length - 1].sprite;
	}
}
