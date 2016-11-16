using UnityEngine;
using System.Collections;

public class Demo_Grid : MonoBehaviour {

    public Grid grid;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
        if(Input.GetMouseButtonUp(0))
        {
            grid.ApplyDirectedForce(Vector2.up * 10f, Camera.main.ScreenToWorldPoint(Input.mousePosition), 1f);
        }
	}
}
