using System.Collections.Generic;
using UnityEngine;

public class Pool : MonoBehaviour
{
	[SerializeField]
	private GameObject templateObject = default;

	[Tooltip("Use a random object from the randomizedObjectPool instead of the template object?")]
	[SerializeField] private bool useRandomizedObjectPoolInstead = false;
	[SerializeField] private List<GameObject> randomizedObjectPool = new List<GameObject>();

	[SerializeField]
	private int numberOfPregenerated = 0;

	private List<GameObject> objects;

	private void Awake()
	{
		if(templateObject != null && templateObject.gameObject.scene.name != null) templateObject.SetActive(false);
		objects = new List<GameObject>();

		GameObject tmpObject;
		for (int i = 0; i < numberOfPregenerated; i++)
		{
			tmpObject = CreateObject();
			tmpObject.SetActive(false);
		}
	}

	public GameObject GetObject()
	{
		foreach (GameObject obj in objects)
		{
			if (obj.activeInHierarchy == false)
			{
				obj.SetActive(true);
				return obj;
			}
		}

		GameObject newObject = CreateObject();
		newObject.SetActive(true);

		return newObject;
	}

	private GameObject CreateObject()
	{
		GameObject newObject = null;
		if (useRandomizedObjectPoolInstead)
		{
			if (randomizedObjectPool.Count == 0)
			{
				Debug.LogError("No randomized Object Pool has been set!", this);
			}
			
			newObject = Instantiate(randomizedObjectPool[Random.Range(0, randomizedObjectPool.Count)]);
		}
		else
		{
			newObject = Instantiate(templateObject);

		}
		
		newObject.transform.parent = transform;

		objects.Add(newObject);
		
		newObject.SetActive(false);

		return newObject;
	}
}
