using UnityEngine;
using System.Collections;

public class Demo_Grid : MonoBehaviour {

    public Grid grid;

    [Header("Explosive")]
    public float explosiveForce = 2f;
    public float explosiveRadius = 2f;

    [Header("Implosive")]
    public float implosiveForce = 2f;
    public float implosiveRadius = 2f;

    [Header("Directional")]
    public float directionalForce = 2f;
    public float directionalRadius = 2f;

    void Update () {
	
        if(Input.GetMouseButtonUp(0))
        {
            grid.ApplyExplosiveForce(explosiveForce, Camera.main.ScreenToWorldPoint(Input.mousePosition), explosiveRadius);
        }
        else if(Input.GetMouseButtonUp(1))
        {
            grid.ApplyImplosiveForce(implosiveForce, Camera.main.ScreenToWorldPoint(Input.mousePosition), implosiveRadius);
        }

        var direction = Vector2.zero;

        if(Input.GetKeyUp(KeyCode.LeftArrow))
        {
            direction = Vector2.left;
        }
        else if (Input.GetKeyUp(KeyCode.DownArrow))
        {
            direction = Vector2.down;
        }
        else if (Input.GetKeyUp(KeyCode.RightArrow))
        {
            direction = Vector2.right;
        }
        else if (Input.GetKeyUp(KeyCode.UpArrow))
        {
            direction = Vector2.up;
        }

        if (direction != Vector2.zero)
        {
            grid.ApplyDirectedForce(direction * directionalForce, Vector3.zero, directionalRadius);
        }
	}
}
