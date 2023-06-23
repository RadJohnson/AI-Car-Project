using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

//[RequireComponent( Rigidbody)]
public class VehicleController : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;

    [Header("Suspension")]
    [SerializeField] private float[] suspensionLength;//NEEDS TO BE AN ARRAY SO THAT IT IS UNIQUE TO EACH WHEEL
    [SerializeField] private float suspensionTravel = 0.2f;
    [SerializeField] private float restLength = 0.5f;
    [SerializeField] private float stiffness = 4700f;
    [SerializeField] private float dampingStiffness = 640f;
    [SerializeField] private float suspensionMaxLength;
    [SerializeField] private float suspensionMinLength;
    private float dampingForce;
    private float[] suspensionLengthOld;//NEEDS TO BE AN ARRAY SO THAT IT IS UNIQUE TO EACH WHEEL

    [Header("Wheel")]
    [SerializeField] private float wheelRadius;
    [SerializeField] private GameObject wheel;
    [SerializeField] private Transform[] suspensionTransforms;
    [SerializeField] private Transform[] wheels;

    //[SerializeField] private bool[] turnyWheels;

    //Need wheelbase and rear track


    [Space(10)]

    [SerializeField] private float speed;

    //[SerializeField] private LayerMask floor;

    public Vector3[] finalForce;

    [SerializeField] private float frictionCoeficient;



    [SerializeField] float steerAngle;







    private void Awake()
    {
        suspensionMinLength = restLength - suspensionTravel;
        suspensionMaxLength = restLength + suspensionTravel;

        suspensionLength = new float[suspensionTransforms.Length];
        suspensionLengthOld = new float[suspensionTransforms.Length];
        finalForce = new Vector3[suspensionTransforms.Length];

        UpdateWheelSize();

        //for (int i = 0; i < wheels.Length; i++)
        //{
        //    wheels[i].localScale = new Vector3(wheelRadius * 1f, wheels[i].localScale.y, wheelRadius * 1f);
        //}
        //wheels = new Transform[suspensionTransforms.Length];

        //for (int i = 0; i < suspensionTransforms.Length; i++)
        //{
        //    wheels[i] = Instantiate(wheel.transform);
        //    //wheels[i].SetParent(gameObject.transform);

        //    wheels[i].gameObject.transform.localScale = new Vector3(wheelRadius * 2, wheels[i].transform.localScale.y, wheelRadius * 2);
        //}
    }

    private void UpdateWheelSize()
    {
        for (int i = 0; i < wheels.Length; i++)
        {
            wheels[i].localScale = new Vector3(wheels[i].localScale.x, wheelRadius * 1f, wheelRadius * 1f);
        }
    }



    //What if i do the suspension on the wheel and push it awway form car instead of push car from the floor

    /// <summary>
    /// Calcualtes Hookes Law
    /// </summary>
    /// <param name="K">is the Spring Constant(stiffnes)</param>
    /// <param name="X">is the Compression/Extension </param>
    /// <returns> </returns>
    float HookesLaw(float K, float X)
    {
        float F = K * X;//need negative K for when I want an extension spring not when I want compression spring
        return F;
    }

    /// <summary>
    /// Used to calculate the speed the suspension is moving so that the damping can counteract it correctly
    /// </summary>
    /// <param name="lasPos"></param>
    /// <param name="curentPos"></param>
    /// <param name="t"></param>
    /// <returns></returns>
    float Acceleration(float lastPos, float curentPos, float t)
    {
        float a = (lastPos - curentPos) / t;
        return a;
    }

    /// <summary>                                                                                   
    /// Calcualtes Force of Friction                                                                
    /// </summary>                                                                                  
    /// <param name="µ">is the Friction Coefficient(ammount of interaction between surfaces)</param>
    /// <param name="N">is the Normal Force(applied perpendicular to the surface contact)</param>   
    /// <returns></returns>                                                                         
    float Friction(float µ, float N)
    {
        float F = µ * N;
        return F;
    }

    /// <summary>
    /// Calcualtes Force of Friction
    /// </summary>
    /// <param name="µ">is the Friction Coefficient(ammount of interaction between surfaces)</param>
    /// <param name="N">is the Normal Force(applied perpendicular to the surface contact)</param>
    /// <returns></returns>
    float Friction(float µ, float N, float angle)
    {
        N = N * MathF.Cos(angle);
        float F = µ * N;
        return F;
    }

    float Force(float mass, float acceleration)
    {
        float F = mass * acceleration;
        return F;
    }


    void FixedUpdate()
    {
        //maybe need to make it so that the suspension that is calculated is random so that it updates smoother with more wheels

        for (int i = 0; i < suspensionTransforms.Length; i++)
        {
            //need to get the relative transform of the spring i think

            if (Physics.Raycast(suspensionTransforms[i].position, -transform.up, out RaycastHit intersectpoint, suspensionMaxLength + wheelRadius/*, floor*/))//This may need to change
            {
                // MAY WANT TO MAKE IT SO THAT WHAT THE SUSPOEEENSION IS ON HAS AN EQUAL FORCE APPLIED TO IT IN OPPOSITE DIRECTION
                suspensionLengthOld[i] = suspensionLength[i];

                suspensionLength[i] = intersectpoint.distance - wheelRadius;//This may need to change

                suspensionLength[i] = Mathf.Clamp(suspensionLength[i], suspensionMinLength, suspensionMaxLength);

                suspensionLength[i] = /*float.Parse(*/suspensionLength[i]/*.ToString("0.00"))*/;

                dampingForce = dampingStiffness * Acceleration(suspensionLengthOld[i], suspensionLength[i], Time.fixedDeltaTime);

                finalForce[i] = (HookesLaw(stiffness, restLength - suspensionLength[i]) + dampingForce) * suspensionTransforms[i].up;
                //Debug.DrawLine(suspensionTransforms[i].position, (suspensionTransforms[i].position + finalForce[i]), Color.blue);

                //Debug.DrawRay(suspensionTransforms[i].position, (suspensionTransforms[i].position + finalForce[i]), Color.blue);
                //rb.AddForce(finalForce);


                // steering force

                // world-space direction of the spring force.
                ///Vector3 steeringDir = suspensionTransforms[i].right;
                // world-space velocity of the suspension
                ///Vector3 tireWorldVel = rb.GetPointVelocity(suspensionTransforms[i].right);
                // what it's the tire's velocity in the steering direction?
                // note that steeringDir is a unit vector, so this returns the magnitude of tireworldVel // as projected onto steeringDir
                ///float steeringVel = Vector3.Dot(steeringDir, tireWorldVel);
                // the change in velocity that we're looking for is -steeringVel * gripFactor
                // gripFactor is in range 0-1, 0 means no grip, 1 means full grip
                ///float desiredVelChange = -steeringVel * frictionCoeficient;
                // turn change in velocity into an acceleration (acceleration change in vel / time)
                // this will produce the acceleration necessary to change the velocity by desiredVelChange in 1 physics step float desiredAccel desiredVelChange / Time.fixedDeltaTime;
                ///float desiredAccel = desiredVelChange / Time.deltaTime;

                // Force = Mass Acceleration, so multiply by the mass of the tire and apply as a force!
                ///rb.AddForceAtPosition(steeringDir * 1 * desiredAccel, suspensionTransforms[i].position);


                //Vector3 steeringDir = suspensionTransforms[i].right;
                //
                //Vector3 tireworldvel = rb.GetPointVelocity(suspensionTransforms[i].position);// may want to put this at the actual tire position rather than the top of the suspension
                //
                //float steeringVel = Vector3.Dot(steeringDir, tireworldvel);
                //
                ////float desiredVelChange = -steeringVel * tiregripforce;//replace with function for friction
                //
                ////float desiredAcel = desiredVelChange / Time.deltaTime;//replace wiht acceleration function
                //
                ////rb.AddForceAtPosition(steeringDir * 1 * Acceleration(steeringVel, Friction(frictionCoeficient, -1 * rb.mass), Time.deltaTime), suspensionTransforms[i].position);
                //
                //Vector3 force = steeringDir *  1 * Acceleration(steeringVel, Friction(frictionCoeficient, -1 * rb.mass),Time.deltaTime);
                //
                //
                //finalForce[i] += force;

                //Debug.Log( Friction(frictionCoeficient,rb.mass,Vector3.Angle(intersectpoint.normal,Vector3.up)) );

                //Vector3 force = suspensionTransforms[i].right *  1 * Acceleration(Vector3.Dot(suspensionTransforms[i].right, rb.GetRelativePointVelocity(suspensionTransforms[i].position)), Friction(frictionCoeficient, rb.mass, Vector3.Angle(suspensionTransforms[i].position, intersectpoint.normal)),Time.deltaTime);
                //finalForce[i] += force;
                //Debug.Log(force);

                Vector3 tireworldvel = rb.GetPointVelocity(suspensionTransforms[i].position);// may want to put this at the actual tire position rather than the top of the suspension 
                                                                                                                                                                                       
                var vectorsdot = Mathf.Sqrt( Vector3.Dot(suspensionTransforms[i].position, tireworldvel));                                                                             
                                                                                                                                                                                       
                var frictionforce = Friction(frictionCoeficient, tireworldvel.x, Vector3.Angle(intersectpoint.normal, Vector3.up));                                                    
                     
                Debug.Log(frictionforce);

                finalForce[i].x += frictionforce;

                //seems to work just want to inverse friction force in the function


                    //final force needs to be changed so that it doesnt go off of world space and instead goes off of local transforms

                rb.AddForceAtPosition(finalForce[i], suspensionTransforms[i].position);

                //Debug.Log($"doobeeg mosag {finalForce[i]}");

                //wheels[i];

                //wheels[i].transform.position = suspensionTransforms[i].transform.position -= new Vector3(0, (suspensionTransforms[i].position - intersectpoint.point).magnitude,0);
            }
            else
            {
                finalForce[i] = Vector3.zero;
                suspensionLength[i] = suspensionMaxLength;
            }

            if (Input.GetKey(KeyCode.W))
            {
                rb.AddForce(transform.forward, ForceMode.Acceleration);
            }
            if (Input.GetKey(KeyCode.S))
            {
                rb.AddForce(-transform.forward, ForceMode.Acceleration);
            }
            if (Input.GetKey(KeyCode.A))
            {
                rb.AddRelativeTorque(-Vector3.up * 4, ForceMode.Force);
                //rb.AddForceAtPosition(transform.right,new Vector3(Mathf.Lerp(suspensionTransforms[0].position.x,suspensionTransforms[1].position.x,0.5f),suspensionTransforms[0].position.y));
            }
            if (Input.GetKey(KeyCode.D))
            {
                rb.AddRelativeTorque(Vector3.up * 4, ForceMode.Force);
                //rb.AddForceAtPosition(-transform.right, new Vector3(Mathf.Lerp(suspensionTransforms[0].position.x, suspensionTransforms[1].position.x, 0.5f), suspensionTransforms[0].position.y));

            }
        }
    }
    /*
     bool rayDidHit = Physics.Raycast(suspensionTransforms[i].position, -transform.up, out RaycastHit tireRay, suspensionMaxLength + wheelRadius);

        if (rayDidHit)
        {
            // World-space direction of the spring force
            Vector3 springDir = suspensionTransforms[i].up;

            // World-space velocity of this tire
            Vector3 tireWorldVel = rb.GetPointVelocity(suspensionTransforms[i].position);

            // Calculate offset from the raycast
            float offset = restLength - tireRay.distance;

            // Calculate velocity along the spring direction
            float vel = Vector3.Dot(springDir, tireWorldVel);

            // Calculate the magnitude of the dampened spring force
            float dampingForce = damping * vel;

            // Calculate the magnitude of the spring force
            float springForce = stiffness * offset;

            // Calculate the total force including damping and spring forces
            float force = springForce - dampingForce;

            // Apply the force at the location of this tire, in the direction of the suspension
            rb.AddForceAtPosition(springDir * force, suspensionTransforms[i].position);
        }
        else
        {
            // No contact with the ground, set the force and suspension length to zero
            finalForce[i] = Vector3.zero;
            suspensionLength[i] = suspensionMaxLength;
        }
     */

    private void Update()
    {
        //will make the mesh appear in the Scene at origin position
        for (int i = 0; i < suspensionTransforms.Length; i++)
        {
            wheels[i].position = suspensionTransforms[i].position + (-transform.up * (suspensionLength[i] + wheelRadius / 2f));
            //Graphics.DrawMesh(wheelMesh, suspensionTransforms[i].position + (-transform.up * suspensionLength[i]), Quaternion.identity, wheelMat, 0);
        }

        UpdateWheelSize();
    }




    void CalculateSuspension()
    {

    }


    void LateralFriction()
    {

    }


    private void OnDrawGizmos()
    {
        if (suspensionTransforms.Length > 0)
        {
            for (int i = 0; i < suspensionTransforms.Length; i++)
            {
                //make it so that the red ray and green ray move based on the actuall length of the suspension

                Gizmos.color = Color.red;

                Gizmos.DrawRay(suspensionTransforms[i].position, -transform.up * restLength);

                Gizmos.color = Color.green;

                Gizmos.DrawRay(suspensionTransforms[i].position - (transform.up * restLength), -transform.up * wheelRadius);




                // make a thing to seperate the forces so you can see them all individually

                Gizmos.color = Color.blue;

                Gizmos.DrawRay(suspensionTransforms[i].position, finalForce[i] / finalForce[i].magnitude);
            }
        }
    }
    void Reset()
    {
        rb = GetComponent<Rigidbody>();
    }
}
