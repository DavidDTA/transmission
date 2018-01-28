using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLogic : MonoBehaviour {
	public GameObject roadStraight;
	public GameObject roadIntersectionT;
	public GameObject roadTurn;
	public GameObject[] environmentTrees;
	public GameObject mainCamera;

	// speed constants
	private float speed = 1.8f;
	private float turnCameraSpeed = 2.3f; 
	private float rotateSpeed = 1f;


	// Used for turning
	private Quaternion targetRotation;
	private Side turnDirection = Side.Right;
	private Direction currentDirection;
	private Vector3 targetPoint;
	private Vector3 rotatePoint;
	private float turnAngle = 0f;
	private float targetAngle = 0f;



	// Positioning
	private Movement movementState = Movement.STRAIGHT;

	int roadbuildingZ = 0;
	int roadbuildingX = 0;

	// Levels
	Levels levelManager;

	// Use this for initialization
	void Start () {
		roadStraight.transform.localScale = new Vector3 (1, 1, -1);
		roadIntersectionT.transform.localScale = new Vector3 (1, 1, -1);
		levelManager = new Levels ();
		instantiateLevel (levelManager.getNextLevel());

		targetRotation = mainCamera.transform.rotation;

		currentDirection = Direction.NORTH;
	}
	
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
			
		mainCamera.transform.rotation = Quaternion.Lerp (mainCamera.transform.rotation, targetRotation , turnCameraSpeed * Time.deltaTime);
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
			currentDirection = (Direction)(mod ((int)currentDirection + 1, 4));
			break;
		case Side.Left: 
			targetRotation = mainCamera.transform.rotation * Quaternion.AngleAxis (-90, Vector3.up);
			movementState = Movement.TURN_LEFT;
			turnAngle = getCurrentAngle () - (Mathf.PI / 2f);
			targetAngle = turnAngle + Mathf.PI / 2;
			rotatePoint = targetPoint + getRotatePointOffset ();
			currentDirection = (Direction)(mod((int)currentDirection - 1, 4));
			break;
		}
		instantiateLevel (levelManager.getNextLevel());
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

	private void instantiateLevel(Level level) {
		foreach (RoadSegment segment in level.road) {
			foreach (KeyValuePair<Side, EnvironmentObject> environmentObject in segment.environmentObjects) {
				instantiateEnvironmentObject (environmentObject.Value, roadbuildingX, roadbuildingZ, segment.road, currentDirection, environmentObject.Key);
			}
			instantiateRoad (segment.road, roadbuildingX, roadbuildingZ, currentDirection);
		}
		targetPoint = new Vector3 (roadbuildingX * 2, 0, roadbuildingZ * 2);
		instantiateRoad (Road.INTERSECTION_T, roadbuildingX, roadbuildingZ, currentDirection);
	}


	private GameObject instantiateRoad(GameObject roadType, int x, int z, int rotation) {
		switch ((Direction) rotation){ 
		case Direction.NORTH:
			roadbuildingZ += 1;
			break;
		case Direction.EAST:
			roadbuildingX += 1;
			break;
		case Direction.SOUTH:
			roadbuildingZ -= 1;
			break;
		case Direction.WEST:
			roadbuildingX -= 1;
			break;
		}
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

public enum Direction {
	NORTH = 0, EAST = 1, SOUTH = 2, WEST = 3
}

public enum Movement {
	STRAIGHT, TURN_LEFT, TURN_RIGHT
}

public enum Side {Right, Left};

public enum Road {
	STRAIGHT, TURN_LEFT, TURN_RIGHT, INTERSECTION_T, BRANCH_LEFT, BRANCH_RIGHT
}
	
public struct Position {
	public int roadSection;
	public Side side;

	public Position (int roadSection, Side side) {
		this.roadSection = roadSection;
		this.side = side;
	}
}
	
public abstract class EnvironmentObject {
	public EnvironmentObject() {
	}

	public abstract GameObject pickTemplate (GameLogic gameLogic);
}
	
public class Tree : EnvironmentObject {
	public Color color;

	public Tree(Color color) {
		this.color = color;
	}

	public override GameObject pickTemplate(GameLogic gameLogic) {
		GameObject[] treeArray = gameLogic.environmentTrees;
		return treeArray[(int) (Random.value * treeArray.Length) % treeArray.Length];
	}
}
	
public class Sign : EnvironmentObject {
	public Color color;

	public Sign(Color color) {
		this.color = color;
	}

	public override GameObject pickTemplate(GameLogic gameLogic) {
		return null;
	}
}

public struct RoadSegment {
	public Road road;
	public KeyValuePair<Side, EnvironmentObject>[] environmentObjects;

	public RoadSegment(Road road, KeyValuePair<Side, EnvironmentObject>[] environmentObjects) {
		this.road = road;
		this.environmentObjects = environmentObjects;
	}

}
	
public struct Level {
	public string rules;
	public Side correctTurn;
	public RoadSegment[] road;

	public Level(string rules, Side correctTurn, RoadSegment[] road) {
		this.rules = rules;
		this.correctTurn = correctTurn;
		this.road = road;
	}
}

public class Levels {

	int levelIndex = 0;
	Level[] levels; 


	public Levels() {
		levels = new Level[] { level1, level2, level3 }; 
	}

	public Level getNextLevel() {
		return levels[levelIndex];
		levelIndex = (levelIndex + 1) % levels.Length;
	}



	public Level level1 = new Level(
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
