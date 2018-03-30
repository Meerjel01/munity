﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerController : MonoBehaviour {
	public float accelleration = 1;
	public float maxspeed = 3;

	float accellAmountZ = 0;
	float accellAmountX = 0;
	float accellAmountY = 0;
	float z,x,y = 0;
	bool touched;
	public int currentPolygon = -1;

	public int maxView = 3;

	//private List<int> activePolygons = new List<int>();
	private bool[] activePolygons = new bool[0];
	private	int activeCount = 0;
	private	int processedCount = 0;

	// Use this for initialization
	void Start () {
		transform.Find("playerCamera/touchCollider").gameObject.GetComponent<CapsuleCollider>().isTrigger = true;
		//getCurrentPolygon();
		// Debug.Log(currentPolygon);
		// for(int seg = 0; seg < GlobalData.map.segments.Count; seg++) {
		// 	if (seg != 3){
		// 		GlobalData.map.segments[seg],showHide(false);
		// 	}
		// }

		currentPolygon = -1;

		//gameObject.GetComponent<CharacterController>().enabled = false;
	}
	
		// Update is called once per frame

	void Update () {
		transform.Find("playerCamera").GetComponent<mouselook>().lockCursor = !Input.GetKey(KeyCode.LeftControl);
		CharacterController cc = GetComponent<CharacterController>();
		
		if (Input.GetKey("w") || Input.GetKey("s")) {
			accellAmountZ += accelleration * Time.deltaTime;
		} else {
			accellAmountZ -= accelleration * Time.deltaTime;
		}

		if (Input.GetKey("a") || Input.GetKey("d")) {
			accellAmountX += accelleration * Time.deltaTime;
		} else {
			accellAmountX -= accelleration * Time.deltaTime;
		}

		if (Input.GetKey("space") || Input.GetKey("space")) {
			accellAmountY += accelleration * Time.deltaTime;
		} else {
			accellAmountY -= accelleration * Time.deltaTime;
		}

		if (accellAmountZ > 1) {accellAmountZ = 1;}
		if (accellAmountX > 1) {accellAmountX = 1;}
		if (accellAmountY > 10) {accellAmountY = 1;}
		if (accellAmountZ <= 0) {
			accellAmountZ = 0;
			z = 0;
		}
		if (accellAmountX <= 0) {
			accellAmountX = 0;
			x = 0;
		}
		if (accellAmountY <= 0) {
			accellAmountY = 0;
			y = 0;
		}
		// float z = 0;
		// float x = 0;
		if (Input.GetKey("w")){z = 1;}
		if (Input.GetKey("s")){z = -1;}
		if (Input.GetKey("a")){x = -1;}
		if (Input.GetKey("d")){x = 1;}
		if (Input.GetKey("space")){y = 1;}

		Vector3 fwspeed = transform.forward * accelleration * (z*accellAmountZ);
		Vector3 rtspeed = transform.right * accelleration * (x*accellAmountX);

		Vector3 move = Vector3.ClampMagnitude(fwspeed + rtspeed, maxspeed);

		if (Input.GetKey("space")) {
			// Debug.Log("upp");
			cc.Move(new Vector3(0, 0.03f * accelleration * (y*accellAmountY),0));
		}
		cc.SimpleMove(move);

		if (Input.GetKey("e")){
			transform.Find("playerCamera/touchCollider").gameObject.SetActive(true);


		} else {
			transform.Find("playerCamera/touchCollider").gameObject.SetActive(false);
			touched = false;
		}

		getCurrentPolygon();
		if (currentPolygon >= 0) {
			drawActivePolygons();
			calculateVisibility();
		}
		if (Input.GetKey("f")){castRay();}

	}

	void getCurrentPolygon() {

		RaycastHit hit;
		//GameObject camera = transform.Find("playerCamera").gameObject;
		if (Physics.Raycast(gameObject.transform.position, Vector3.down, out hit, 50)) {
			if (hit.collider.transform.parent != null && hit.collider.transform.parent.name == "polygon(Clone)") {
				if (hit.collider.transform.parent.GetComponent<MapSegment>().id != currentPolygon) {
					//Debug.Log(hit.collider.transform.parent.GetComponent<MapSegment>().id);
				}
				currentPolygon = hit.collider.transform.parent.GetComponent<MapSegment>().id;
			}
		}

	}

	/// <summary>
	/// OnTriggerEnter is called when the Collider other enters the trigger.
	/// </summary>
	/// <param name="other">The other Collider involved in this collision.</param>
	void OnTriggerEnter(Collider other)
	{
		if (!touched){
			MapSegment ms = null;
			if (other.name == "floor" ||other.name == "ceiling" || other.name == "wall" || other.name == "transparent" || other.name == "polygonElement(Clone)"){

				if (other.transform.parent.name == "polygon(Clone)") {
					ms = other.transform.parent.GetComponent<MapSegment>();
				} else if (other.transform.parent.name == "upperPlatform" || other.transform.parent.name == "lowerPlatform") {
					ms =  other.transform.parent.parent.GetComponent<MapSegment>();
				}
				if (ms != null) {
					touched = ms.playerTouch(other.gameObject);
				}
				// Debug.Log(other.)
			}
		}
	}

	void drawActivePolygons() {
		bool[] avtive = GlobalData.map.segments[currentPolygon].activePolygons;
		for (int i = 0; i < avtive.Length; i++) {
			if (!GlobalData.map.segments[i].impossible) {
				GlobalData.map.segments[i].showHide(avtive[i]);
			} else {
				GlobalData.map.segments[i].showHide(false);
			}
		}


	}

	void calculateVisibility() {
	// Debug.Log("----------------------------------------");
		activePolygons = new bool[GlobalData.map.segments.Count];
		bool[] processedPolys = new bool[GlobalData.map.segments.Count];
		List<int> collides = new List<int>();
		List<int> deferred = new List<int>();
		activeCount = 0;
		processedCount = 0;
		float[] distances = new float[GlobalData.map.segments.Count];
		for (int i = 0; i < GlobalData.map.segments.Count; i++) {
			if (GlobalData.map.segments[i].impossible &&
					GlobalData.map.segments[currentPolygon].activePolygons[i]) {
				activePolygons[i] = true;
				activeCount++;
				GlobalData.map.segments[i].showHide(false);
				float distance;
				distances[i] = 7777777;
				for (int s = 0; s < GlobalData.map.segments[i].sides.Count; s++) {
					if (GlobalData.map.segments[i].sides[s].connectionID != -1) {
						Vector3 v1 = GlobalData.map.segments[i].vertices[s];
						// Vector2 v2;
						// if (s < GlobalData.map.segments[i].sides.Count-1) {
						// 	v2 = GlobalData.map.segments[i].vertices[s+1];
						// } else {
						// 	v2 = GlobalData.map.segments[i].vertices[0];
						// }
						distance = Vector3.Distance(gameObject.transform.position, GlobalData.map.segments[i].transform.TransformPoint(v1));
						if (distance < distances[i]) {distances[i] = distance;}
					}
					// foreach (Vector3 v in GlobalData.map.segments[i].vertices) {
					// 	distance = Vector3.Distance(gameObject.transform.position, GlobalData.map.segments[i].transform.TransformPoint(v));
					// 	if (distance < distances[i]) { distances[i] = distance;}
					// }
				}
				//distances[i] = Vector3.Distance(gameObject.transform.position, GlobalData.map.segments[i].transform.position);

			}
		}

		MapSegment pol  = GlobalData.map.segments[currentPolygon];

		if (pol.impossible) {
			activePolygons[currentPolygon] = true;
			pol.showHide(true);
			collides.AddRange(pol.collidesWith);

		}
		for( int s = 0; s < pol.sides.Count; s++) {
			if (pol.sides[s].connectionID >= 0 && 
			GlobalData.map.segments[pol.sides[s].connectionID].impossible) {
					GlobalData.map.segments[pol.sides[s].connectionID].showHide(true);
					processedPolys[pol.sides[s].connectionID] = true;
					processedCount++;
					collides.AddRange(GlobalData.map.segments[pol.sides[s].connectionID].collidesWith);
			}
		}
		

		while (processedCount < activeCount) {
			float distance = 7777777;
			int closest = -1;
			// int connections = 0;
			for (int i = 0; i < activePolygons.Length; i++) {
				if (activePolygons[i] && !processedPolys[i]){
					if (distances[i] < distance) {
						distance = distances[i];
						closest = i;
					}
				}
			}
			activePolygons[closest] = false;
			for( int s = 0; s < GlobalData.map.segments[closest].sides.Count; s++) {
				if (GlobalData.map.segments[closest].sides[s].connectionID >= 0 &&
					!GlobalData.map.segments[GlobalData.map.segments[closest].sides[s].connectionID].hidden) {
					
					Vector3 point1, point2;
					point1 = GlobalData.map.segments[closest].vertices[s];
					if (s+1 < GlobalData.map.segments[closest].vertices.Count) {
						point2 = GlobalData.map.segments[closest].vertices[s+1];
					} else {
						point2 = GlobalData.map.segments[closest].vertices[0];
					}
					point1 = GlobalData.map.segments[closest].transform.TransformPoint(point1);
					point2 = GlobalData.map.segments[closest].transform.TransformPoint(point2);
					if (getRectVisibility(point1, point2, GlobalData.map.segments[closest].height)) {
						if (!collides.Contains(closest)) {
							GlobalData.map.segments[closest].showHide(true);
							collides.AddRange(GlobalData.map.segments[closest].collidesWith);
						} else {
							deferred.Add(closest);
						}
						break;
					}
				}
			}

			processedPolys[closest] = true; 
			processedCount++;

			// drawPolygonList(true);

		}
		// drawPolygonList(false);
		//Debug.Log(deferred.Count);

		foreach (int i in deferred) {
			float distance = 7777777;
			int closest = -1;
			foreach (int s in GlobalData.map.segments[i].collidesWith) {
				if (distances[s] < distance && GlobalData.map.segments[s].hidden == false) {
					distance = distances[s];
					closest = s;
				}
			}
			if (closest > -1) {
				clipSegments(GlobalData.map.segments[i], GlobalData.map.segments[closest]);
			}
		}
	}

	void clipSegments(MapSegment segment1, MapSegment segment2) {
		List<Vector2> intersectPoints = new List<Vector2>();
		Vector3 point1a, point2a, point1b, point2b;

		for (int a = 0; a < segment1.sides.Count; a++) {
			point1a = segment1.vertices[a];
			if (a < segment1.vertices.Count-1) {
				point2a = segment1.vertices[a+1];
			} else {
				point2a = segment1.vertices[0];
			}
			
			point1a = segment1.transform.TransformPoint(point1a);
			point2a = segment1.transform.TransformPoint(point2a);

			float a1 = point2a.z - point1a.z;
			float b1 = point1a.x - point2a.x;
			float c1 = a1*point1a.x + b1*point1a.z;

			for (int b = 0; b < segment2.sides.Count; b++) {
				point1b = segment2.vertices[b];
				if (b < segment2.vertices.Count-1) {
					point2b = segment2.vertices[b+1];
				} else {
					point2b = segment2.vertices[0];
				}
				
				point1b = segment2.transform.TransformPoint(point1b);
				point2b = segment2.transform.TransformPoint(point2b);

				float a2 = point2b.z - point1b.z;
				float b2 = point1b.x - point2b.x;
				float c2 = a2*point1b.x + b2*point1b.z;


				float delta = a1*b2 - a2*b1;
				if(delta != 0) {
					Vector2 intersect = new Vector2((b2*c1 - b1*c2)/delta, (a1*c2 - a2*c1)/delta);
					
					float d1 = Vector2.Distance(new Vector2(point1a.x, point1a.z), intersect);
					float d2 = Vector2.Distance(new Vector2(point2a.x, point2a.z), intersect);
					float d3 = Vector2.Distance(new Vector2(point1a.x, point1a.z), new Vector2(point2a.x, point2a.z));

					float d4 = Vector2.Distance(new Vector2(point1b.x, point1b.z), intersect);
					float d5 = Vector2.Distance(new Vector2(point2b.x, point2b.z), intersect);
					float d6 = Vector2.Distance(new Vector2(point1b.x, point1b.z), new Vector2(point2b.x, point2b.z));


					if (d1 + d2 - 0.001 < d3 + 0.001 && d4 + d5 - 0.001 < d6 + 0.001) {
						intersectPoints.Add(intersect);
					}

					
				}

			}
		}
				Debug.Log("'--------'");
 
	foreach(Vector2 i in intersectPoints) {
		Debug.Log(i);
	}
		
	}

// Vector3 getPlaneIntersection(Plane p1, Plane p2, Vector3 r_point, Vector3 r_normal) {
//     Vector3 p3_normal = Vector3.Cross(p1.normal,p2.normal);
//     float det = p3_normal.sqrMagnitude;

//     // If the determinant is 0, that means parallel planes, no intersection.
//     // note: you may want to check against an epsilon value here.
//     if (det != 0.0) {
//         // calculate the final (point, normal)
//         r_point = (Vector3.Cross(p3_normal, p2.normal) * p1.distance) +
//                    (Vector3.Cross(p1.normal, p3_normal * p2.distance)) / det;
//         r_normal = p3_normal;
// 		return r_point;
// 	} else {
// 		return new Vector3(0,0,0);
// 	}
// }

	void drawPolygonList (bool showOnly) {
		for (int i = 0; i < activePolygons.Length; i++) {
			if (GlobalData.map.segments[i].impossible && GlobalData.map.segments[currentPolygon].activePolygons[i] &&
					(activePolygons[i] || !showOnly)) {
				GlobalData.map.segments[i].showHide(activePolygons[i]);
			}
		}
	}


	bool getRectVisibility (Vector3 point1, Vector3 point2, Vector3 height) {
		bool isVisible = false;
		RaycastHit hit;
		GameObject camera;
		camera = transform.Find("playerCamera").gameObject;

		Vector3[] points = new Vector3[8];
		Vector3 p1, p2, p3, h1, h2, h3;
		h1 = height*0.005f;
		h2 = height*0.995f;
		h3 = height*0.5f;
		p1 = (point1-point2)*0.005f;
		p2 = (point1-point2)*0.995f;
		p3 = (point1-point2)*0.5f;
		

		Vector3 midpoint = p3 + point2 + h3;

		points[0] = p1 + point2  + h1;
		points[1] = p2 + point2  + h1;
		points[2] = p1 + point2  + h2;
		points[3] = p2 + point2  + h2;
		points[4] = p3 + point2  + h2;
		points[5] = p3 + point2  + h1;
		points[6] = p1 + point2  + h3;
		points[7] = p2 + point2  + h3;


		Vector3 castPoint;
		float rayCount = 3f;
		isVisible = false;

		isVisible = (Physics.Raycast(midpoint, camera.transform.position-midpoint, out hit, 50)) 
			&& hit.collider.gameObject == camera ;

		if (isVisible) {
			Debug.DrawRay(midpoint, camera.transform.position-midpoint, Color.red);
		} else {
			Debug.DrawRay(midpoint, camera.transform.position-midpoint, Color.green);
		}

		Color colour  = Random.ColorHSV();
		for (int i = 1; i <= rayCount && !isVisible; i++) {
			for (int p = 0; p < points.Length && !isVisible; p++){
				castPoint = (points[p]-midpoint)*((1f/rayCount)*(float)i) + midpoint;
				isVisible = (Physics.Raycast(castPoint, camera.transform.position-castPoint, out hit, 50)) 
							&& hit.collider.gameObject == camera ;
				if (isVisible) {
					Debug.DrawRay(castPoint, camera.transform.position-castPoint, Color.red);
					return true;
				} else {
					Debug.DrawRay(castPoint, camera.transform.position-castPoint, Color.green);
				}
			}
		}

		return isVisible;

	}
	void castRay() {
		RaycastHit hit;
		Vector3 cameraPos;
		// cameraPos = transform.Find("playerCamera").position;
		cameraPos = transform.Find("playerCamera").transform.position;

		if (Physics.Raycast(cameraPos, transform.Find("playerCamera").forward, out hit, 20)) {
			Debug.Log(hit.collider.name);
			Debug.DrawRay(cameraPos, transform.Find("playerCamera").forward, Color.yellow, 5f);

		}
// 		Vector3 point1, point2, midpoint;
// 		MapSegment seg = GlobalData.map.segments[0].GetComponent<MapSegment>();
// 		MapSegmentSide side = GlobalData.map.segments[0].GetComponent<MapSegment>().sides[3];
// 			//if (side.connectionID >= 0 && !activePolygons.Contains(side.connectionID)) {
// 				point1 = seg.vertices[3];
// 				point2 = seg.vertices[4];




// 				point1 = GlobalData.map.segments[0].transform.TransformPoint(point1);
// 				point2 = GlobalData.map.segments[0].transform.TransformPoint(point2);
// 		bool isVisible;		
// 		midpoint = (point1-point2)/2 + point2 + (seg.height/2);
// 				isVisible = (Physics.Raycast(midpoint, cameraPos-midpoint, out hit, 20)) 
// 								&& hit.collider.gameObject == gameObject ;
// 		Debug.Log(hit.collider.gameObject.name);

// Debug.DrawRay(point1, cameraPos-point1, Color.green);
// Debug.DrawRay(point2, cameraPos-point2, Color.green);
// Debug.DrawRay(midpoint, cameraPos-midpoint, Color.green);

		//	}

	}

	/// <summary>
	/// This function is called every fixed framerate frame, if the MonoBehaviour is enabled.
	/// </summary>
	void FixedUpdate()
	{
		//touched = false;

		// Rigidbody rb = GetComponent<Rigidbody>();


		// float z = 0;
		// float x = 0;
		// if (Input.GetKey("w")){z += 1;}
		// if (Input.GetKey("s")){z -= 1;}
		// if (Input.GetKey("a")){x -= 1;}
		// if (Input.GetKey("d")){x += 1;}

		// Vector3 fwspeed = transform.forward * accelleration * z;
		// Vector3 rtspeed = transform.right * accelleration * x;
		// if (rb.velocity.magnitude < maxspeed) {
		// 	rb.velocity += fwspeed + rtspeed;
		// }
	}

}
