using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLogic : MonoBehaviour {
	public GameObject roadStraight;
	public GameObject roadIntersectionT;
	public GameObject roadTurn;
	public GameObject[] environmentTrees;
	public GameObject mainCamera;

	// speed/acceleration constants
	private float speed = 0.8f;
	private float deceleration = 0.988f;
	private float acceleration = 1.01f;

	// Used for turning
	private bool isTurning = false;
	private GameObject targetIntersection;
	private Quaternion targetRotation;
	private Side? turnDirection = null;

	// Use this for initialization
	void Start () {
		roadStraight.transform.localScale = new Vector3 (1, 1, -1);
		roadIntersectionT.transform.localScale = new Vector3 (1, 1, -1);
		instantiateRoad (Road.STRAIGHT, 0, 0, Direction.NORTH);
		instantiateRoad (Road.TURN_LEFT, 0, 1, Direction.NORTH);
		instantiateRoad (Road.TURN_RIGHT, -1, 1, Direction.WEST);
		instantiateRoad (Road.BRANCH_LEFT, -1, 2, Direction.NORTH);
		instantiateRoad (Road.TURN_RIGHT, -1, 3, Direction.NORTH);
		instantiateRoad (Road.TURN_LEFT, 0, 3, Direction.EAST);

		instantiateEnvironmentObject (new Tree (Color.blue), 0, 1, Road.STRAIGHT, Direction.NORTH, Side.Right);

		targetIntersection = instantiateRoad (Road.INTERSECTION_T, 0, 4, Direction.NORTH);
		targetRotation = mainCamera.transform.rotation;
	}

	private float angle = 0f;
	private float rotateSpeed = 0.001f;
	
	// Update is called once per frame
	void Update () {
		// Listen for left and right turns.
		if (Input.GetKeyDown (KeyCode.RightArrow)) {
			turnDirection = Side.Right;
		}
		if (Input.GetKeyDown (KeyCode.LeftArrow)) {
			turnDirection = Side.Left;
		}

		mainCamera.transform.position += new Vector3(
			Camera.main.transform.forward.x,
			0f,
			Camera.main.transform.forward.z
		) * speed * Time.deltaTime;

		angle += rotateSpeed * Time.deltaTime;

		var offset = new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle) * 10);
		//mainCamera.transform.position = targetIntersection.transform.position + offset;
		mainCamera.transform.rotation = Quaternion.Lerp (mainCamera.transform.rotation, targetRotation , 1.5f * Time.deltaTime);
	}


	private GameObject instantiateRoad(Road roadType, int x, int z, Direction heading) {
		switch (roadType) {
		case Road.STRAIGHT:
			return instantiateRoad (roadStraight, x, z, (int) heading);
		case Road.TURN_LEFT:
			return instantiateRoad (roadTurn, x, z, (int) heading - 1);
		case Road.TURN_RIGHT:
			return instantiateRoad (roadTurn, x, z, (int) heading - 2);
		case Road.BRANCH_LEFT:
			return instantiateRoad (roadIntersectionT, x, z, (int) heading);
		case Road.BRANCH_RIGHT:
			return instantiateRoad (roadIntersectionT, x, z, (int) heading - 2);
		case Road.INTERSECTION_T:
			return instantiateRoad (roadIntersectionT, x, z, (int) heading - 1);
		default:
			Debug.Log ("Unimplemented road instantiation");
			return null;
		}
	}

	private GameObject instantiateRoad(GameObject roadType, int x, int z, int rotation) {
		return Instantiate (roadType, new Vector3(2 * x, 0, 2 * z), Quaternion.Euler(new Vector3(0, rotation * 90, 0)));
	}

	private Vector3 environmentObjectOffset(Road roadType, Direction heading, Side side) {
		switch (roadType) {
		case Road.STRAIGHT:
			return new Vector3 (side == Side.Left ? -.875f : .875f, 0, Random.value * 2 - 1);
		default:
			Debug.Log ("Unimplemented environment offset");
			return Vector3.zero;
		}
	}

	private void instantiateEnvironmentObject(EnvironmentObject environmentObject, int x, int z, Road roadType, Direction heading, Side side) {
		Instantiate (environmentObject.pickTemplate(this), new Vector3(2 * x, 0, 2 * z) + environmentObjectOffset(roadType, heading, side), Quaternion.identity);
	}
}

enum Direction {
	NORTH = 0, EAST = 1, SOUTH = 2, WEST = 3
}

enum Side {Right, Left};

enum Road {
	STRAIGHT, TURN_LEFT, TURN_RIGHT, INTERSECTION_T, BRANCH_LEFT, BRANCH_RIGHT
}
	
struct Position {
	public int roadSection;
	public Side side;

	public Position (int roadSection, Side side) {
		this.roadSection = roadSection;
		this.side = side;
	}
}
	
abstract class EnvironmentObject {
	public EnvironmentObject() {
	}

	public abstract GameObject pickTemplate (GameLogic gameLogic);
}
	
class Tree : EnvironmentObject {
	public Color color;

	public Tree(Color color) {
		this.color = color;
	}

	public override GameObject pickTemplate(GameLogic gameLogic) {
		GameObject[] treeArray = gameLogic.environmentTrees;
		return treeArray[(int) (Random.value * treeArray.Length) % treeArray.Length];
	}
}
	
class Sign : EnvironmentObject {
	public Color color;

	public Sign(Position pos, Color color) {
		this.color = color;
	}

	public override GameObject pickTemplate(GameLogic gameLogic) {
		return null;
	}
}

struct RoadSegment {
	public Road road;
	KeyValuePair<Side, EnvironmentObject>[] environmentObjects;

	public RoadSegment(Road road, KeyValuePair<Side, EnvironmentObject>[] environmentObjects) {
		this.road = road;
		this.environmentObjects = environmentObjects;
	}

}
	
struct Level {
	public string rules;
	public Side correctTurn;
	RoadSegment[] road;

	public Level(string rules, Side correctTurn, RoadSegment[] road) {
		this.rules = rules;
		this.correctTurn = correctTurn;
		this.road = road;
	}
}

public class Levels {

	private Level level1 = new Level(
		"If one side of the road has more yellow trees, turn to that side",
		Side.Left,
		new RoadSegment[] {
			new RoadSegment(Road.STRAIGHT, new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Left, new Tree(Color.yellow)),
			}),
			new RoadSegment(Road.STRAIGHT, new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Right, new Tree(Color.green)),
			}),
			new RoadSegment(Road.STRAIGHT, new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(Road.STRAIGHT, new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Left, new Tree(Color.yellow)),
			}),
			new RoadSegment(Road.STRAIGHT, new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(Road.STRAIGHT, new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(Road.STRAIGHT, new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Left, new Tree(Color.green)),
			}),
			new RoadSegment(Road.STRAIGHT, new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(Road.STRAIGHT, new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(Road.STRAIGHT, new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Right, new Tree(Color.yellow)),
			}),
		});

	private Level level2 = new Level(
		"If one side of the road has more yellow trees, turn to that side",
		Side.Right,
		new RoadSegment[] {
			new RoadSegment(Road.STRAIGHT, new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Right, new Tree(Color.yellow)),
			}),
			new RoadSegment(Road.STRAIGHT, new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Right, new Tree(Color.green)),
			}),
			new RoadSegment(Road.STRAIGHT, new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(Road.STRAIGHT, new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Left, new Tree(Color.yellow)),
			}),
			new RoadSegment(Road.STRAIGHT, new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Left, new Tree(Color.green)),
				new KeyValuePair<Side, EnvironmentObject>(Side.Right, new Tree(Color.yellow)),
			}),
		});

	private Level level3 = new Level(
		"If one side of the road has more yellow trees, turn to that side." +
		"UNLESS there is a red stop sign at the intersection, in which case you always turn right",
		Side.Left,
		new RoadSegment[] {
			new RoadSegment(Road.STRAIGHT, new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(Road.STRAIGHT, new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Right, new Tree(Color.yellow)),
				new KeyValuePair<Side, EnvironmentObject>(Side.Left, new Tree(Color.yellow)),
				new KeyValuePair<Side, EnvironmentObject>(Side.Right, new Tree(Color.green)),
			}),
			new RoadSegment(Road.STRAIGHT, new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(Road.STRAIGHT, new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Left, new Tree(Color.yellow)),
			}),
			new RoadSegment(Road.STRAIGHT, new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Left, new Tree(Color.green)),
				new KeyValuePair<Side, EnvironmentObject>(Side.Left, new Tree(Color.yellow)),
			}),
			new RoadSegment(Road.STRAIGHT, new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(Road.STRAIGHT, new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(Road.STRAIGHT, new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Right, new Sign(Color.blue)),
			}),
		});

	private Level level4 = new Level(
		"If one side of the road has more yellow trees, turn to that side." +
		"UNLESS there is a red stop sign at the intersection, in which case you always turn right",
		Side.Right,
		new RoadSegment[] {
			new RoadSegment(Road.STRAIGHT, new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(Road.STRAIGHT, new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Right, new Tree(Color.yellow)),
				new KeyValuePair<Side, EnvironmentObject>(Side.Left, new Tree(Color.yellow)),
				new KeyValuePair<Side, EnvironmentObject>(Side.Right, new Tree(Color.yellow)),
			}),
			new RoadSegment(Road.STRAIGHT, new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(Road.STRAIGHT, new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Left, new Tree(Color.green)),
			}),
			new RoadSegment(Road.STRAIGHT, new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Left, new Tree(Color.yellow)),
				new KeyValuePair<Side, EnvironmentObject>(Side.Right, new Tree(Color.green)),
			}),
			new RoadSegment(Road.STRAIGHT, new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(Road.STRAIGHT, new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(Road.STRAIGHT, new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Left, new Sign(Color.red)),
			}),
		});

	private Level level5 = new Level(
		"If one side of the road has more yellow trees, turn to that side." +
		"UNLESS there is a red stop sign at the intersection, in which case you always turn right\n" +
		"If you see a black tree, switch yellow and green all other rules",
		Side.Right,
		new RoadSegment[] {
			new RoadSegment(Road.STRAIGHT, new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(Road.STRAIGHT, new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Right, new Tree(Color.green)),
				new KeyValuePair<Side, EnvironmentObject>(Side.Left, new Tree(Color.yellow)),
				new KeyValuePair<Side, EnvironmentObject>(Side.Right, new Tree(Color.yellow)),
			}),
			new RoadSegment(Road.STRAIGHT, new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(Road.STRAIGHT, new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Left, new Tree(Color.green)),
			}),
			new RoadSegment(Road.STRAIGHT, new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Left, new Tree(Color.green)),
				new KeyValuePair<Side, EnvironmentObject>(Side.Right, new Tree(Color.yellow)),
			}),
			new RoadSegment(Road.STRAIGHT, new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(Road.STRAIGHT, new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(Road.STRAIGHT, new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Left, new Sign(Color.red)),
			}),
		});

	private Level level6 = new Level(
		"If one side of the road has more yellow trees, turn to that side." +
		"UNLESS there is a red stop sign at the intersection, in which case you always turn right\n" +
		"If you see a black tree, switch yellow and green for all other rules",
		Side.Right,
		new RoadSegment[] {
			new RoadSegment(Road.STRAIGHT, new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(Road.STRAIGHT, new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Right, new Tree(Color.yellow)),
			}),
			new RoadSegment(Road.STRAIGHT, new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Left, new Tree(Color.yellow)),
			}),
			new RoadSegment(Road.STRAIGHT, new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(Road.STRAIGHT, new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Right, new Tree(Color.green)),
				new KeyValuePair<Side, EnvironmentObject>(Side.Left, new Tree(Color.black)),
			}),
			new RoadSegment(Road.STRAIGHT, new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Right, new Tree(Color.yellow)),
			}),
		});

	private Level level7 = new Level(
		"If one side of the road has more yellow trees, turn to that side." +
		"UNLESS there is a red stop sign at the intersection, in which case you always turn right\n" +
		"If you see a black tree, switch yellow and green for all other rules\n" +
		"NEVER turn right twice in a row. If the other rules say to, turn left instead.",
		Side.Left,
		new RoadSegment[] {
			new RoadSegment(Road.STRAIGHT, new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(Road.STRAIGHT, new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Left, new Tree(Color.yellow)),
			}),
			new RoadSegment(Road.STRAIGHT, new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Left, new Tree(Color.green)),
			}),
			new RoadSegment(Road.STRAIGHT, new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(Road.STRAIGHT, new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Left, new Tree(Color.green)),
				new KeyValuePair<Side, EnvironmentObject>(Side.Right, new Tree(Color.yellow)),
				new KeyValuePair<Side, EnvironmentObject>(Side.Right, new Tree(Color.yellow)),
			}),
			new RoadSegment(Road.STRAIGHT, new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Right, new Tree(Color.yellow)),
			}),
		});

	private Level level8 = new Level(
		"If one side of the road has more yellow trees, turn to that side." +
		"UNLESS there is a red stop sign at the intersection, in which case you always turn right\n" +
		"If you see a black tree, switch yellow and green for all other rules\n" +
		"NEVER turn right twice in a row. If the other rules say to, turn left instead.",
		Side.Right,
		new RoadSegment[] {
			new RoadSegment(Road.STRAIGHT, new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(Road.STRAIGHT, new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Right, new Tree(Color.yellow)),
			}),
			new RoadSegment(Road.STRAIGHT, new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Left, new Tree(Color.yellow)),
			}),
			new RoadSegment(Road.STRAIGHT, new KeyValuePair<Side, EnvironmentObject>[] {}),
			new RoadSegment(Road.STRAIGHT, new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Right, new Tree(Color.green)),
				new KeyValuePair<Side, EnvironmentObject>(Side.Left, new Tree(Color.green)),
			}),
			new RoadSegment(Road.STRAIGHT, new KeyValuePair<Side, EnvironmentObject>[] {
				new KeyValuePair<Side, EnvironmentObject>(Side.Right, new Tree(Color.yellow)),
			}),
		});
}
