using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLogic : MonoBehaviour {
	public GameObject roadStraight;
	public GameObject roadIntersectionT;

	// Use this for initialization
	void Start () {
		roadStraight.transform.localScale = new Vector3 (1, 1, -1);
		roadIntersectionT.transform.localScale = new Vector3 (1, 1, -1);
		instantiateRoadStraight (0, 0, Direction.NORTH);
		instantiateRoadIntersectionT (0, 1, Direction.NORTH);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void instantiateRoad(GameObject roadType, int x, int z, int rotation) {
		Instantiate (roadType, new Vector3(2 * x, 0, 2 * z), Quaternion.Euler(new Vector3(0, rotation * 90, 0)));
	}

	void instantiateRoadStraight(int x, int z, Direction dir) {
		instantiateRoad(roadStraight, x, z, (int) dir);
	}

	void instantiateRoadIntersectionT(int x, int z, Direction dir) {
		instantiateRoad(roadIntersectionT, x, z, -((int) dir + 1));
	}
}

enum Direction {
	NORTH = 0, EAST = 1, SOUTH = 2, WEST = 3
}