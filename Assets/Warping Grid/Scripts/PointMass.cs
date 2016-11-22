using UnityEngine;
using System.Collections;

public class PointMass
{
    public Vector3 Position;
    public Vector3 Velocity;
    public float InverseMass;

    private Vector3 m_Acceleration;
    private float m_Damping = 0.98f;

    public PointMass(Vector3 position, float invMass)
    {
        Position = position;
        InverseMass = invMass;
    }

    public void ApplyForce(Vector3 force)
    {
        m_Acceleration += force * InverseMass;
    }

    public void IncreaseDamping(float factor)
    {
        m_Damping *= factor;
    }

    public void Update()
    {
        Velocity += m_Acceleration;
        Position += Velocity;
        m_Acceleration = Vector3.zero;
        if (Velocity.sqrMagnitude < 0.001f * 0.001f)
            Velocity = Vector3.zero;

        Velocity *= m_Damping;
        m_Damping = 0.98f;
    }
}
