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
	private float speed = 1.8f;
	private float deceleration = 0.988f;
	private float acceleration = 1.01f;

	// Used for turning
	private Quaternion targetRotation;
	private Side turnDirection = Side.Right;
	private Direction currentDirection;
	private Movement movementState = Movement.STRAIGHT;
	private Vector3 targetPoint;
	Vector3 [] targetPoints;
	int pointIndex = 0;

	// Use this for initialization
	void Start () {
		roadStraight.transform.localScale = new Vector3 (1, 1, -1);
		roadIntersectionT.transform.localScale = new Vector3 (1, 1, -1);
		instantiateRoad (Road.STRAIGHT, 0, 0, Direction.NORTH);
		instantiateRoad (Road.INTERSECTION_T, 0, 1, Direction.NORTH);
		instantiateRoad (Road.STRAIGHT, -1, 1, Direction.EAST);
		instantiateRoad (Road.INTERSECTION_T, -2, 1, Direction.EAST);
		instantiateRoad (Road.STRAIGHT, -2, 0, Direction.SOUTH);
		instantiateRoad (Road.INTERSECTION_T, -2, -1, Direction.SOUTH);
		instantiateRoad (Road.STRAIGHT, -1, -1, Direction.WEST);
		instantiateRoad (Road.INTERSECTION_T, 0, -1, Direction.WEST);
		targetPoints = new Vector3[] {
			new Vector3 (0, 0, 2),
			new Vector3 (-4, 0, 2),
			new Vector3 (-4, 0, -2),
			new Vector3 (0, 0, -2)
		};

		targetPoint = targetPoints [pointIndex];
		pointIndex++;

		targetRotation = mainCamera.transform.rotation;

		currentDirection = Direction.NORTH;
	}

	private float turnAngle = 0f;
	private float targetAngle = 0f;
	private float rotateSpeed = 1f;
	private Vector3 rotatePoint;
	
	// Update is called once per frame
	void Update () {
		MoveCar ();

		// Listen for left and right turns.
		if (Input.GetKeyDown (KeyCode.RightArrow)) {
			turnDirection = Side.Right;
		}
		if (Input.GetKeyDown (KeyCode.LeftArrow)) {
			turnDirection = Side.Left;
		}
			
		mainCamera.transform.rotation = Quaternion.Lerp (mainCamera.transform.rotation, targetRotation , 3.3f * Time.deltaTime);
	}

	Vector3 getRotatePointOffset() {
		int x = 0;
		int y = 0;
		if (movementState == Movement.TURN_RIGHT) {
			switch (currentDirection) {
			case Direction.NORTH:
				x += 1;
				y -= 1;
				break;
			case Direction.EAST:
				x -= 1;
				y -= 1;
				break;
			case Direction.SOUTH:
				x -= 1;
				y += 1;
				break;
			case Direction.WEST:
				x += 1;
				y += 1;
				break;
			}
		} else if (movementState == Movement.TURN_LEFT) {
			switch (currentDirection) {
			case Direction.NORTH:
				x -= 1;
				y -= 1;
				break;
			case Direction.EAST:
				x -= 1;
				y += 1;
				break;
			case Direction.SOUTH:
				x += 1;
				y += 1;
				break;
			case Direction.WEST:
				x += 1;
				y -= 1;
				break;
			}
		}
		return new Vector3 (x, 0, y);
	}

	float getCurrentAngle() {
		switch (currentDirection) {
		case Direction.NORTH:
			return Mathf.PI / 2.0f;
		case Direction.EAST:
			return 0;
		case Direction.SOUTH:
			return -Mathf.PI / 2.0f;
		case Direction.WEST:
			return Mathf.PI;
		}
		return 0;
	}

	int mod(int x, int m) {
		return (x%m + m)%m;
	}

	void setTurn(Side direction) {
		switch (direction) {
		case Side.Right:
			targetRotation = mainCamera.transform.rotation * Quaternion.AngleAxis (90, Vector3.up);
			movementState = Movement.TURN_RIGHT;
			turnAngle = getCurrentAngle () + (Mathf.PI / 2f);
			targetAngle = turnAngle - Mathf.PI / 2;
			rotatePoint = targetPoint + getRotatePointOffset ();
			currentDirection = (Direction)(mod((int)currentDirection + 1, 4));
			break;
		case Side.Left: 
			targetRotation = mainCamera.transform.rotation * Quaternion.AngleAxis (-90, Vector3.up);
			movementState = Movement.TURN_LEFT;
			turnAngle = getCurrentAngle () - (Mathf.PI / 2f);
			Debug.Log (currentDirection);

			Debug.Log (turnAngle);
			targetAngle = turnAngle + Mathf.PI / 2;
			rotatePoint = targetPoint + getRotatePointOffset ();
			currentDirection = (Direction)(mod((int)currentDirection - 1, 4));
			break;
		}
	}

	Vector3 getCarPos() {
		return new Vector3 (
			mainCamera.transform.position.x,
			0f,
			mainCamera.transform.position.z
		);
	}

	void MoveCar() {
		// Move car; either to continue straight or to turn.
		switch (movementState) {
		case Movement.STRAIGHT:
			mainCamera.transform.position += new Vector3 (
					mainCamera.transform.forward.x,
					0f,
					mainCamera.transform.forward.z
				) * speed * Time.deltaTime;
			if (Vector3.Distance(getCarPos(), targetPoint) < 1) {
				setTurn(turnDirection);
			}
			break;
		case Movement.TURN_LEFT:
			turnAngle += rotateSpeed * Time.deltaTime;
			var offset = new Vector3 (Mathf.Cos (turnAngle), 0, Mathf.Sin (turnAngle)) * 1f;
			mainCamera.transform.position = rotatePoint + offset + new Vector3 (0, .2f, 0);
			if (Mathf.Abs ((turnAngle % (2 * Mathf.PI)) - (targetAngle % (2 * Mathf.PI))) < 0.03f) {
				offset = new Vector3 (Mathf.Cos (targetAngle), 0, Mathf.Sin (targetAngle)) * 1f;
				mainCamera.transform.position = rotatePoint + offset + new Vector3 (0, .2f, 0);
				movementState = Movement.STRAIGHT;
				mainCamera.transform.rotation = targetRotation;

				targetPoint = targetPoints [pointIndex];
				pointIndex++;
			}
			break;
		case Movement.TURN_RIGHT:
			turnAngle -= rotateSpeed * Time.deltaTime;
			offset = new Vector3 (Mathf.Cos (turnAngle), 0, Mathf.Sin (turnAngle)) * 1f;
			mainCamera.transform.position = rotatePoint + offset + new Vector3 (0, .2f, 0);
			if (Mathf.Abs ((turnAngle % (2 * Mathf.PI)) - (targetAngle % (2 * Mathf.PI))) < 0.03f) {
				offset = new Vector3 (Mathf.Cos (targetAngle), 0, Mathf.Sin (targetAngle)) * 1f;
				mainCamera.transform.position = rotatePoint + offset + new Vector3 (0, .2f, 0);
				movementState = Movement.STRAIGHT;
				mainCamera.transform.rotation = targetRotation;

				targetPoint = targetPoints [pointIndex];
				pointIndex++;
			}
			break;
		}
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
		float xOff;
		float zOff;
		GameObject template = environmentTrees[(int) (Random.value * environmentTrees.Length) % environmentTrees.Length];
		Instantiate (template, new Vector3(2 * x, 0, 2 * z) + environmentObjectOffset(roadType, heading, side), Quaternion.identity);
	}
}

enum Direction {
	NORTH = 0, EAST = 1, SOUTH = 2, WEST = 3
}

enum Movement {
	STRAIGHT, TURN_LEFT, TURN_RIGHT
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
	
class EnvironmentObject {
	public EnvironmentObject() {
	}
}
	
class Tree : EnvironmentObject {
	public Color color;

	public Tree(Color color) {
		this.color = color;
	}
}
	
class Sign : EnvironmentObject {
	public Color color;

	public Sign(Position pos, Color color) {
		this.color = color;
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
				new KeyValuePair<Side, EnvironmentObject>(Side.Right, new Tree(Color.blue)),
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
				new KeyValuePair<Side, EnvironmentObject>(Side.Left, new Tree(Color.red)),
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
				new KeyValuePair<Side, EnvironmentObject>(Side.Left, new Tree(Color.red)),
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
