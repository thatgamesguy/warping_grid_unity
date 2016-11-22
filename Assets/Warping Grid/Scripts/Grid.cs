using UnityEngine;
using System.Collections.Generic;

public class Grid : MonoBehaviour
{

    public Rect size;
    public Vector2 spacing;
    public float minLineWidth = 0.01f;
    public float maxLineWidth = 0.03f;
    public enum DrawType
    {
        Quick,
        Smooth
    }
    public DrawType drawType = DrawType.Smooth;
    public int maxInstantiatedLines = 1520;
    public GameObject linePrefab;
    public Color insideColour = Color.red;
    public Color outsideColour = Color.blue;

    private Spring[] m_Springs;
    private PointMass[,] m_Points;
    private List<SpriteRenderer> m_Renderers;
    private int m_LineIndex = 0;

    void Start()
    {
        m_Renderers = new List<SpriteRenderer>();

        var springList = new List<Spring>();

        int numColumns = (int)(size.width / spacing.x) + 1;
        int numRows = (int)(size.height / spacing.y) + 1;
        m_Points = new PointMass[numColumns, numRows];

        // these fixed points will be used to anchor the grid to fixed positions on the screen
        PointMass[,] fixedPoints = new PointMass[numColumns, numRows];

        // create the point masses
        int column = 0, row = 0;
        for (float y = size.yMin; y <= size.yMax; y += spacing.y)
        {
            for (float x = size.xMin; x <= size.xMax; x += spacing.x)
            {
                m_Points[column, row] = new PointMass(new Vector3(x, y, 0), 1);
                fixedPoints[column, row] = new PointMass(new Vector3(x, y, 0), 0);
                column++;
            }
            row++;
            column = 0;
        }

        int count = 0;
        // link the point masses with springs
        for (int y = 0; y < numRows; y++)
        {
            for (int x = 0; x < numColumns; x++)
            {
                if (x == 0 || y == 0 || x == numColumns - 1 || y == numRows - 1)    // anchor the border of the grid 
                {
                    springList.Add(new Spring(fixedPoints[x, y], m_Points[x, y], 0.1f, 0.1f));
                }
                else if (x % 3 == 0 && y % 3 == 0)                                  // loosely anchor 1/9th of the point masses 
                {
                    springList.Add(new Spring(fixedPoints[x, y], m_Points[x, y], 0.002f, 0.02f));
                }

                const float stiffness = 0.28f;
                const float damping = 0.06f;
                if (x > 0)
                {
                    springList.Add(new Spring(m_Points[x - 1, y], m_Points[x, y], stiffness, damping));
                }

                if (y > 0)
                {
                    springList.Add(new Spring(m_Points[x, y - 1], m_Points[x, y], stiffness, damping));
                }

                count++;
            }
        }

        for(int i = 0; i < maxInstantiatedLines; i++)
        {
            m_Renderers.Add(GetNewLineRenderer());
        }

        m_Springs = springList.ToArray();
    }

    private SpriteRenderer GetNewLineRenderer()
    {
        GameObject line = (GameObject)Instantiate(linePrefab);
        line.transform.SetParent(transform);
        return line.GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        foreach (var spring in m_Springs)
        {
            spring.Update();
        }

        foreach (var mass in m_Points)
        {
            mass.Update();
        }

        Draw();

        if (numDrawn > maxInstantiatedLines)
        {
            print("Attempted to draw: " + numDrawn + " but constrained by maxInstantiatedLines: " + maxInstantiatedLines);
        }
        numDrawn = 0;
    }

    private void Draw()
    {
        if(drawType == DrawType.Smooth)
        {
            SmoothDraw();
        }
        else if(drawType == DrawType.Quick)
        {
            QuickDraw();
        }

    }

    private void QuickDraw()
    {
        int width = m_Points.GetLength(0);
        int height = m_Points.GetLength(1);

        for (int y = 1; y < height; y++)
        {
            for (int x = 1; x < width; x++)
            {

                Vector2 left = new Vector2(), up = new Vector2();

                Vector2 p = ToVec2(m_Points[x, y].Position);

                if (x > 1)
                {
                    left = ToVec2(m_Points[x - 1, y].Position);

                    bool thickLine = y % 3 == 1;
                    Color colour = outsideColour;
                    float thickness = minLineWidth;

                    if(thickLine)
                    {
                        colour = insideColour;
                        thickness = maxLineWidth;
                    }


                    DrawLine(left, p, colour, thickness, m_Renderers[m_LineIndex]);
                    m_LineIndex = (m_LineIndex + 1) % m_Renderers.Count;
                }
                if (y > 1)
                {
                    up = ToVec2(m_Points[x, y - 1].Position);

                    bool thickLine = x % 3 == 1;
                    Color colour = outsideColour;
                    float thickness = minLineWidth;

                    if (thickLine)
                    {
                        colour = insideColour;
                        thickness = maxLineWidth;
                    }

                    DrawLine(up, p, colour, thickness, m_Renderers[m_LineIndex]);
                    m_LineIndex = (m_LineIndex + 1) % m_Renderers.Count;
                }

                if (x > 1 && y > 1)
                {
                    Vector2 upLeft = ToVec2(m_Points[x - 1, y - 1].Position);

                    DrawLine(0.5f * (upLeft + up), 0.5f * (left + p), outsideColour, minLineWidth, m_Renderers[m_LineIndex]);   // vertical line
                    m_LineIndex = (m_LineIndex + 1) % m_Renderers.Count;

                    DrawLine(0.5f * (upLeft + left), 0.5f * (up + p), outsideColour, minLineWidth, m_Renderers[m_LineIndex]);   // horizontal line
                    m_LineIndex = (m_LineIndex + 1) % m_Renderers.Count;

                }
            }
        }
    }

    private void SmoothDraw()
    {
        int width = m_Points.GetLength(0);
        int height = m_Points.GetLength(1);

        for (int y = 1; y < height; y++)
        {
            for (int x = 1; x < width; x++)
            {
                Vector2 left = new Vector2(), up = new Vector2();
                Vector2 p = ToVec2(m_Points[x, y].Position);
                if (x > 1)
                {
                    left = ToVec2(m_Points[x - 1, y].Position);

                    bool thickLine = y % 3 == 1;
                    Color colour = outsideColour;
                    float thickness = minLineWidth;

                    if (thickLine)
                    {
                        colour = insideColour;
                        thickness = maxLineWidth;
                    }

                    // use Catmull-Rom interpolation to help smooth bends in the grid
                    int clampedX = Mathf.Min(x + 1, width - 1);
                    Vector2 mid = Interpolate.CatmullRom(ToVec2(m_Points[x - 2, y].Position), left, p, ToVec2(m_Points[clampedX, y].Position), 0.5f, 05f);

                    // If the grid is very straight here, draw a single straight line. Otherwise, draw lines to our
                    // new interpolated midpoint
                    var dist = Vector2.Distance(mid, (left + p) / 2);
                    if (dist * dist > 1)
                    {
                        DrawLine(left, mid, colour, thickness, m_Renderers[m_LineIndex]);
                        m_LineIndex = (m_LineIndex + 1) % m_Renderers.Count;

                        DrawLine(mid, p, colour, thickness, m_Renderers[m_LineIndex]);
                        m_LineIndex = (m_LineIndex + 1) % m_Renderers.Count;
                    }
                    else
                    {
                        DrawLine(left, p, colour, thickness, m_Renderers[m_LineIndex]);
                        m_LineIndex = (m_LineIndex + 1) % m_Renderers.Count;
                    }
                }
                if (y > 1)
                {
                    up = ToVec2(m_Points[x, y - 1].Position);
                    bool thickLine = x % 3 == 1;
                    Color colour = outsideColour;
                    float thickness = minLineWidth;

                    if (thickLine)
                    {
                        colour = insideColour;
                        thickness = maxLineWidth;
                    }
                    int clampedY = Mathf.Min(y + 1, height - 1);
                    Vector2 mid = Interpolate.CatmullRom(ToVec2(m_Points[x, y - 2].Position), up, p, ToVec2(m_Points[x, clampedY].Position), 0.5f, 0.5f);

                    var dist = Vector2.Distance(mid, (up + p) / 2);
                    if (dist * dist > 1)
                    {
                        DrawLine(up, mid, colour, thickness, m_Renderers[m_LineIndex]);
                        m_LineIndex = (m_LineIndex + 1) % m_Renderers.Count;

                        DrawLine(mid, p, colour, thickness, m_Renderers[m_LineIndex]);
                        m_LineIndex = (m_LineIndex + 1) % m_Renderers.Count;
                    }
                    else
                    {
                        DrawLine(up, p, colour, thickness, m_Renderers[m_LineIndex]);
                        m_LineIndex = (m_LineIndex + 1) % m_Renderers.Count;
                    }
                }

                // Add interpolated lines halfway between our point masses. This makes the grid look
                // denser without the cost of simulating more springs and point masses.
                if (x > 1 && y > 1)
                {
                    Vector2 upLeft = ToVec2(m_Points[x - 1, y - 1].Position);
                    DrawLine(0.5f * (upLeft + up), 0.5f * (left + p), outsideColour, minLineWidth, m_Renderers[m_LineIndex]);   // vertical line
                    m_LineIndex = (m_LineIndex + 1) % m_Renderers.Count;

                    DrawLine(0.5f * (upLeft + left), 0.5f * (up + p), outsideColour, minLineWidth, m_Renderers[m_LineIndex]);   // horizontal line
                    m_LineIndex = (m_LineIndex + 1) % m_Renderers.Count;
                }

            }
        }
    }


    int numDrawn = 0;
    private void DrawLine(Vector2 start, Vector2 end, Color color, float thickness, SpriteRenderer renderer)
    {
        numDrawn++;
        var heading = end - start;
        var distance = heading.magnitude;
        var direction = heading / distance;

        //Debug.Log("Start: " + start);
        Vector3 centerPos = new Vector3(start.x + end.x, start.y + end.y) / 2;
        renderer.transform.position = centerPos;

        // angle
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        renderer.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);


        //length
        var objectWidthSize = 10f / 5f; // 10 = pixels of line sprite, 5f = pixels per units of line sprite.
        renderer.transform.localScale = new Vector3(distance / objectWidthSize, thickness, renderer.transform.localScale.z);

        //colour
        renderer.color = color;

    }


    public Vector2 ToVec2(Vector3 v)
    {
        // do a perspective projection
        float factor = (v.z + 2000) / 2000;

        var screenSize = new Vector2(Screen.width, Screen.height);

        return (new Vector2(v.x, v.y) - screenSize / 2f) * factor + screenSize / 2;
    }

    public void ApplyDirectedForce(Vector3 force, Vector3 position, float radius)
    {
        foreach (var mass in m_Points)
        {
            var dist = Vector2.Distance(position, mass.Position);
            if (dist < radius)
                mass.ApplyForce(10 * force / (10 + Vector3.Distance(position, mass.Position)));
        }
    }

    public void ApplyImplosiveForce(float force, Vector3 position, float radius)
    {
        foreach (var mass in m_Points)
        {

            float dist2 = Vector2.Distance(position, mass.Position);
            if (dist2 < radius)
            {
                mass.ApplyForce(10 * force * (position - mass.Position) / (100 + dist2));
                mass.IncreaseDamping(0.6f);
            }
        }
    }

    public void ApplyExplosiveForce(float force, Vector3 position, float radius)
    {
        foreach (var mass in m_Points)
        {
            float dist2 = Vector2.Distance(position, mass.Position);
    
            if (dist2 < radius)
            {
                mass.ApplyForce(100 * force * (mass.Position - position) / (10000 + dist2));
                mass.IncreaseDamping(0.6f);
            }
        }
    }
}
