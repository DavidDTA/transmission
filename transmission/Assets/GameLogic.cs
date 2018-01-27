using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLogic : MonoBehaviour {
	
	// Use this for initialization
	void Start () {
		new Level ().start ();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void placeItems(Item[] items) {
	}
}

class Level {
	public static float ROAD_LENGTH = 1000f;
	public static float ROAD_WIDTH = 10f;

	public GameObject road = GameObject.CreatePrimitive(PrimitiveType.Quad);
	public Item[] items = { new Item() };

	public Level() {
		road.transform.Rotate(new Vector3 (90, 0, 0));
		road.transform.localScale = new Vector3 (ROAD_WIDTH, ROAD_LENGTH, 1);
		road.transform.position = new Vector3 (0, 0, ROAD_LENGTH / 2);
	}

	public void start() {
	}
		
	void clear() {
		foreach (Item item in items) {
			item.destroy ();
		}
	}
}

class Item {
	public GameObject gameObject = new GameObject("item");
	public Item() {
		gameObject.AddComponent<Rigidbody>().useGravity = false;
		gameObject.AddComponent<MeshFilter>().mesh = Resources.Load<Mesh>("");
		gameObject.AddComponent<MeshRenderer>();
	}

	public void destroy() {
		GameObject.Destroy(gameObject);
	}
}