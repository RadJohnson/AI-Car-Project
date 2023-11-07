using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class VehicleController : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;


    [Serializable]
    struct Suspension
    {
        [Header("Suspension")]
        [SerializeField] internal Transform suspensionTransforms;
        internal float suspensionLength;//NEEDS TO BE AN ARRAY SO THAT IT IS UNIQUE TO EACH WHEEL
        internal float suspensionLengthOld;//NEEDS TO BE AN ARRAY SO THAT IT IS UNIQUE TO EACH WHEEL
        [SerializeField] internal float suspensionTravelf;// Rectify this stuff to make it easier to set susspension length
        [SerializeField] internal float restLength;
        [SerializeField] internal float stiffness;
        [SerializeField] internal float dampingStiffness;
        internal float suspensionMaxLength;
        internal float suspensionMinLength;
        [SerializeField] bool Steering;
        [SerializeField] bool drive;

        //private float dampingForce;
        //private float suspensionForce;
        [Header("Wheel")]
        [SerializeField] private float wheelRadius;
        [SerializeField] private GameObject wheel;
        [SerializeField] private float frictionCoefficient;
    }
    [SerializeField] Suspension[] suspensions;




    [Header("Suspension")]
    [SerializeField] private float[] suspensionLength;//NEEDS TO BE AN ARRAY SO THAT IT IS UNIQUE TO EACH WHEEL
    private float[] suspensionLengthOld;//NEEDS TO BE AN ARRAY SO THAT IT IS UNIQUE TO EACH WHEEL
    [SerializeField] private float suspensionTravel = 0.2f;// Rectify this stuff to make it easier to set susspension length
    [SerializeField] private float restLength = 0.5f;
    [SerializeField] private float stiffness = 4700f;
    [SerializeField] private float dampingStiffness = 640f;
    [SerializeField] private float suspensionMaxLength;
    [SerializeField] private float suspensionMinLength;
    private float dampingForce;
    private float suspensionForce;

    [Header("Wheel")]
    [SerializeField] private float wheelRadius;
    [SerializeField] private GameObject wheel;
    [SerializeField] private Transform[] suspensionTransforms;
    [SerializeField] private Transform[] wheels;

    //[SerializeField] private bool[] turnyWheels;

    //wheelbase(width between wheels) and rear track(length between wheels) this could b usefulc


    [Space(10)]


    //[SerializeField] private LayerMask floor;

    [SerializeField] private float frictionCoefficient;
    //[SerializeField] private float rollingCoefficeint;


    [SerializeField, Range(0, 60)] private float maximumSteeringAngle;
    private float steerAngle;
    [SerializeField] private float steerForce;

    [SerializeField] private Vector3 finalForceWorld;
    private Vector3 objectOnSusspensionLastPos;


    [SerializeField] float lateralForce;
    [SerializeField] float driveForce;

    [SerializeField] private bool spiderCar;

    [SerializeField, Range(0.05f, 1)] private float vehicleDragCoeficient;


    [SerializeField] float engineForce;

    [SerializeField] float dragConstant = 0f;
    [SerializeField] float rollingResistanceConstant = 0f;
    [SerializeField] float sDragConstant = 0f;
    float rho = 1.29f;

    [SerializeField] AnimationCurve frictionCurve;
    private float engineForceMultiplier;

    private void Awake()
    {
        Application.targetFrameRate = -1;

        float frontalArea = 0;
        float sideArea = 0;

        if (TryGetComponent(out Collider col))
        {
            Vector3 localExtents = col.bounds.extents;

            frontalArea = Mathf.Abs(localExtents.x) * Mathf.Abs(localExtents.z) * 4f;
            sideArea = Mathf.Abs(localExtents.y) * Mathf.Abs(localExtents.z) * 4f;
        }

        dragConstant = AirResistance(vehicleDragCoeficient, frontalArea, rho);
        rollingResistanceConstant = dragConstant * 30;
        sDragConstant = AirResistance(vehicleDragCoeficient, sideArea, rho);

        suspensionLength = new float[suspensionTransforms.Length];
        suspensionLengthOld = new float[suspensionTransforms.Length];

        suspensionMinLength = restLength - suspensionTravel;// Rectify this stuff to make it easier to set susspension length
        suspensionMaxLength = restLength + suspensionTravel;// Rectify this stuff to make it easier to set susspension length

        UpdateWheelSize();
    }

    private void UpdateWheelSize()
    {
        for (int i = 0; i < wheels.Length; i++)
        {
            wheels[i].localScale = new Vector3(wheels[i].localScale.x, wheelRadius * 1f, wheelRadius * 1f);
        }
    }


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

    ///// <summary>
    ///// Calcualtes Force of Friction
    ///// </summary>
    ///// <param name="µ">is the Friction Coefficient(ammount of interaction between surfaces)</param>
    ///// <param name="N">is the Normal Force(applied perpendicular to the surface contact)</param>
    ///// <returns></returns>
    //float Friction(float µ, float N, float angle)
    //{
    //    N = N * MathF.Cos(angle);
    //    float F = -µ * N;
    //    return F;
    //}


    /// <summary>
    /// Calculates RollingFriction output as F
    /// </summary>
    /// <param name="f">f is the coefficient of rolling friction</param>
    /// <param name="W">W is the weight of the cylinder converted to force, or the force between the cylinder and the flat surface</param>
    /// <param name="R">R is radius of the cylinder</param>
    /// <returns></returns>
    float RollingFriction(float f, float W, float R)
    {
        float F = f * W / R;
        return F;
    }

    float Force(float M, float A)
    {
        float F = M * A;
        return F;
    }

    float Acceleration(float F, float M)
    {
        float A = F / M;
        return A;
    }

    /// <summary>
    /// Calculates force to be applied by wheels
    /// </summary>
    /// <param name="u">Unit vector for heading  could just be forward vector for vehicle?</param>
    /// <param name="EngineForce">Power of the engine</param>
    /// <returns></returns>
    float Traction(float u, float EngineForce)
    {
        float F = u * EngineForce;
        return F;
    }

    //F(traction) = u* Engineforce, where u is a unit vector in the direction of the car's heading.

    /// <summary>
    /// 
    /// </summary>
    /// <param name="C"></param>
    /// <param name="V"></param>
    /// <returns></returns>
    float Drag(float C, float V)
    {
        var F = -C * V * V;
        return F;
    }

    //F(drag) = - C(drag) * v * |v| where C(drag) is a constant and v is the velocity vector and the notation |v| refers to the magnitude of vector v

    /// <summary>
    /// 
    /// </summary>
    /// <param name="C"></param>
    /// <param name="V"></param>
    /// <returns></returns>
    float RollingResistance(float C, float V)
    {
        float F = -C * V;
        return F;
    }

    //F(rr) = - C(rr) * v  where C(rr) is a constant and v is the velocity vector.

    float RollingResistanceGreg(float rollingResistanceConstant, float velocityZ)
    {
        // Calculate the rolling resistance force
        float rollingResistanceForce = -rollingResistanceConstant * velocityZ;

        // Apply damping to slow down the car over time
        float dampingFactor = 0.5f; // Experiment with the value to adjust the damping effect
        rollingResistanceForce += dampingFactor * velocityZ;

        return rollingResistanceForce;
    }

    float RollingResistanceConstantResistance(float N, float μ)
    {
        float F = N * μ;
        return F;
    }

    //F = Nμ, where:

    //F is the rolling resistance force.

    //N is the normal force.

    //μ is the rolling resistance coefficient.

    float LongtitudinalForce(float Traction, float Drag, float RollingResistance)
    {
        float F = Traction + Drag + RollingResistance;
        return F;
    }
    //F(long) = F(traction) + F(drag) + F(rr) Note that if you're driving in a straight line the drag and rolling resistance forces will be in the opposite
    //direction from the traction force.  So in terms of  magnitude, you're subtracting the resistance force from the traction force.
    //When the car is cruising at a constant speed the forces are in equilibrium and Flong is zero.

    /// <summary>
    /// 
    /// </summary>
    /// <param name="C">vehicle coefficient of drag</param>
    /// <param name="A">frontal area of car</param>
    /// <param name="Rho">density of air</param>
    /// <param name="V">speed of the car</param>
    /// <returns></returns>
    float AirResistance(float C, float A, float Rho)
    {
        float F = 0.5f * C * A * Rho;
        return F;
    }

    //Fdrag =  0.5 * Cd* A * rho* v2
    //
    //where Cd = coefficient of friction
    //A is frontal area of car
    //rho(Greek symbol)= density of air
    //v = speed of the car



    //float Engine(float Tourque,float gearRatio)
    //{

    //    return;
    //}

    //Fdrive = u* Tengine * xg* xd * n / Rw
    //where
    //u is a unit vector which reflects the car's orientation,
    //Tengine is the torque of the engine at a given rpm,
    //xg is the gear ratio,
    //xd is the differential ratio,
    //n is transmission efficiency and
    //Rw is wheel radius.


    /*
    How do you calculate normal force?
    Normal force (Fn) can be calculated by the application of Newton's Second Law (F=m*a). 
    On a flat surface, for example, Fn can be calculate by Fn=m*g. 
    On a surface inclined with an angle X, Fn can be calculated by Fn=m*g*cosX.
    */

    float idleRPM, maxRPM, RPM;



    float NormalForce(float M, float g)
    {
        float F = M * g;
        return F;
    }
    float NormalForce(float M, float g, float X)
    {
        float F = M * g * Mathf.Cos(X);
        return F;
    }

    void FixedUpdate()
    {
        //maybe need to make it so that the suspension that is calculated is random so that it updates smoother with more wheels

        for (int i = 0; i < suspensionTransforms.Length; i++)
        {
            if (Physics.Raycast(suspensionTransforms[i].position, -transform.up, out RaycastHit intersectPoint, suspensionMaxLength + wheelRadius/*, floor*/))//This may need to change
            {
                ///Suspension

                suspensionLengthOld[i] = suspensionLength[i];

                suspensionLength[i] = intersectPoint.distance - wheelRadius;

                suspensionLength[i] = Mathf.Clamp(suspensionLength[i], suspensionMinLength, suspensionMaxLength);

                suspensionLength[i] = suspensionLength[i];

                dampingForce = dampingStiffness * Acceleration(suspensionLengthOld[i], suspensionLength[i], Time.fixedDeltaTime);

                suspensionForce = HookesLaw(stiffness, restLength - suspensionLength[i]) + dampingForce;

                if (!spiderCar)//this amkes the car more stable when turned on (could consider making it switch on and off based off of some condition)
                    suspensionForce = Mathf.Max(0.0f, suspensionForce);//Mathf.Clamp(suspensionForce, 0, Mathf.Infinity);
                // this line above is needed so that the car doesnt stick to cielings or walls

                //This bit needs to hold the objects that are on the suspension points to be fully robust and handle more than one object without issue
                //GameObject objectOnSusspension = intersectPoint.collider.gameObject;
                //if (objectOnSusspension.TryGetComponent(out Rigidbody otherObjectRb))
                //{
                //    otherObjectRb.AddForceAtPosition(new Vector3(0, HookesLaw(stiffness, restLength - suspensionLength[i]) +
                //        (dampingStiffness * Acceleration(objectOnSusspensionLastPos.y, objectOnSusspension.transform.position.y, Time.fixedDeltaTime)), 0), intersectPoint.point);
                //}
                //objectOnSusspensionLastPos = intersectPoint.collider.gameObject.transform.position;


                ///LateralForce

                //lateralForce = LateralFriction(suspensionTransforms[i], frictionCoefficient);

                Vector3 tireWorldVel = rb.GetPointVelocity(suspensionTransforms[i].position);
                Vector3 tireLocalVel = suspensionTransforms[i].InverseTransformDirection(tireWorldVel);
                lateralForce = -Friction(frictionCoefficient, tireLocalVel.x);
                //lateralForce = Friction(frictionCoefficient, NormalForce(rb.mass, Physics.gravity.y, Vector3.Angle(intersectPoint.normal, Vector3.up))*tireLocalVel.x);

                //Debug.Log(NormalForce(rb.mass, Physics.gravity.y, Vector3.Angle(intersectPoint.normal, Vector3.up)));
                //Debug.Log(Vector3.Angle(intersectPoint.normal, Vector3.up));
                //Debug.Log($"tireWorldVel {tireWorldVel} tireLocalVel {tireLocalVel}");
                //lateralForce = Acceleration(lateralForce,rb.mass);

                ///RollingFriction

                //driveForce = (engineForce * engineForceMultiplier)/4 + Friction(rollingResistanceConstant, tireLocalVel.z);

                //float rollingFrictionForce = Friction(rollingCoefficeint, tireLocalVel.z);

                //float rollingFriction = RollingFriction(rollingCoefficeint, rb.mass, wheelRadius);

                //Debug.Log(rollingFriction);
                //float maxDriveForce = 100.0f;

                //Debug.Log($"EngineForceMultiplier {engineForceMultiplier}");

                //Debug.Log($"EngineForce {engineForce}");
                //float dampingFactorDrive = 0.2f; // Experiment with the value to adjust the damping effect




                driveForce = /*Friction(frictionCoefficient, tireLocalVel.z) */+Traction(engineForceMultiplier, engineForce);
                //Debug.Log($"Drive force traction {driveForce}");

                //driveForce += Drag(dragConstant, tireLocalVel.z) + RollingResistance(rollingResistanceConstant, tireLocalVel.z); //make drag apply on rigidbody rahter than the wheels

                //Debug.Log($"Drive force after drag {driveForce} Drag{Drag(dragConstant, tireLocalVel.z)} roll res {RollingResistance(rollingResistanceConstant, tireLocalVel.z)}");

                //driveForce = Acceleration(driveForce,rb.mass);

                //driveForce = Mathf.Clamp(driveForce,-20,450);
                //driveForce -= dampingFactorDrive * rb.velocity.z;

                //driveForce = Mathf.Clamp(driveForce, -maxDriveForce, maxDriveForce);

                // Convert the final force to world space

                finalForceWorld = suspensionTransforms[i].TransformDirection(new Vector3(lateralForce, suspensionForce, driveForce));

                //Debug.Log(Drag(dragConstant, tireLocalVel.z) + RollingResistance(rollingResistanceConstant, tireLocalVel.z));

                var s = transform.TransformDirection(new Vector3(Drag(sDragConstant, rb.velocity.x), 0, Drag(dragConstant, rb.velocity.z) + RollingResistance(rollingResistanceConstant, tireLocalVel.z)));
                //var s = transform.TransformDirection(new Vector3(Drag(sDragConstant, rb.velocity.x), 0, Drag(dragConstant, transform.InverseTransformDirection(rb.velocity).z) + RollingResistance(rollingResistanceConstant, transform.InverseTransformDirection(rb.velocity).z)));

                //rb.AddForceAtPosition(s, transform.position);//drag forces


                // Apply the force at the suspension position
                rb.AddForceAtPosition(finalForceWorld + s, suspensionTransforms[i].position /*+ (-transform.up * (suspensionLength[i] + wheelRadius / 2f))*/);
                //rb.AddRelativeForce(0, 0, driveForce, ForceMode.Force);
            }
            else
            {
                suspensionForce = 0;
                lateralForce = 0;
                driveForce = 0;
                finalForceWorld = Vector3.zero;
                suspensionLength[i] = suspensionMaxLength;
            }


            if (Input.GetKey(KeyCode.W))
            {
                engineForceMultiplier++;
                //rb.AddForce(transform.forward, ForceMode.Acceleration);
            }
            else if (Input.GetKey(KeyCode.S))
            {
                engineForceMultiplier--;
                //rb.AddForce(-transform.forward, ForceMode.Acceleration);
            }
            else
            {
                RecenterThrottle();
            }
            engineForceMultiplier = Mathf.Clamp(engineForceMultiplier, -1.5f, 1.5f);

            ///Steering
            if (Input.GetKey(KeyCode.A))
            {
                steerAngle -= steerForce;
            }
            else if (Input.GetKey(KeyCode.D))
            {
                steerAngle += steerForce;
            }
            else
            {
                RecenterSteering();
            }

            steerAngle = Mathf.Clamp(steerAngle, -maximumSteeringAngle, maximumSteeringAngle);
            suspensionTransforms[0].localEulerAngles = new Vector3(suspensionTransforms[0].gameObject.transform.localEulerAngles.x, steerAngle, suspensionTransforms[0].gameObject.transform.localEulerAngles.z);
            suspensionTransforms[1].localEulerAngles = new Vector3(suspensionTransforms[0].gameObject.transform.localEulerAngles.x, steerAngle, suspensionTransforms[0].gameObject.transform.localEulerAngles.z);
        }
    }
    void RecenterSteering()
    {
        if (steerAngle > 0.05f || steerAngle < -0.05f)
            steerAngle = Mathf.Lerp(steerAngle, 0, Time.deltaTime);
        else steerAngle = 0;
    }
    void RecenterThrottle()
    {
        if (engineForceMultiplier > 0.05f || engineForceMultiplier < -0.05f)
            engineForceMultiplier = Mathf.Lerp(engineForceMultiplier, 0, Time.deltaTime);
        else engineForceMultiplier = 0;
    }


    private void Update()
    {
        //float frontalArea = 0;

        //if (TryGetComponent(out Collider col))
        //{
        //    Bounds bounds = col.bounds;
        //    frontalArea = bounds.size.x * bounds.size.z;
        //}

        //dragConstant = AirResistance(vehicleDragCoeficient, frontalArea, rho);
        //rollingResistanceConstant = dragConstant * 30;

        for (int i = 0; i < suspensionTransforms.Length; i++)
        {
            wheels[i].position = suspensionTransforms[i].position + (-transform.up * (suspensionLength[i] + wheelRadius / 2f));
            wheels[i].localRotation = suspensionTransforms[i].localRotation;
        }

        UpdateWheelSize();
    }




    private float CalculateSuspension(RaycastHit ray, ref Suspension suspension)
    {
        ///Suspension
        float suspensionForce, dampingForce;
        suspension.suspensionLengthOld = suspension.suspensionLength;

        suspension.suspensionLength = ray.distance - wheelRadius;

        suspension.suspensionLength = Mathf.Clamp(suspension.suspensionLength, suspensionMinLength, suspensionMaxLength);

        dampingForce = suspension.dampingStiffness * Acceleration(suspension.suspensionLengthOld, suspension.suspensionLength, Time.fixedDeltaTime);


        suspensionForce = HookesLaw(suspension.stiffness, suspension.restLength - suspension.suspensionLength) + dampingForce;

        if (!spiderCar)//this amkes the car more stable when turned on (could consider making it switch on and off based off of some condition)
            suspensionForce = Mathf.Clamp(suspensionForce, 0, Mathf.Infinity);
        // this line above is needed so that the car doesnt stick to cielings or walls




        //This bit needs to hold the objects that are on the suspension points to be fully robust and handle more than one object without issue
        GameObject objectOnSusspension = ray.collider.gameObject;
        if (objectOnSusspension.TryGetComponent(out Rigidbody otherObjectRb))
        {
            otherObjectRb.AddForceAtPosition(new Vector3(0, HookesLaw(stiffness, restLength - suspension.suspensionLength) +
                (suspension.dampingStiffness * Acceleration(objectOnSusspensionLastPos.y, objectOnSusspension.transform.position.y, Time.fixedDeltaTime)), 0), ray.point);
        }
        objectOnSusspensionLastPos = ray.collider.gameObject.transform.position;



        return suspensionForce;
    }


    float LateralFriction(Transform _suspensionTransform, float _frictionCoeficient)
    {
        Vector3 tireWorldVel = rb.GetPointVelocity(_suspensionTransform.position);
        Vector3 tireLocalVel = _suspensionTransform.InverseTransformDirection(tireWorldVel);
        return Friction(_frictionCoeficient, tireLocalVel.x);
    }


    private void OnDrawGizmos()
    {
        if (suspensionTransforms.Length > 0)
        {
            for (int i = 0; i < suspensionTransforms.Length; i++)
            {
                //Spring
                Gizmos.color = Color.yellow;
                Gizmos.DrawRay(suspensionTransforms[i].position, -transform.up * suspensionLength[i]);

                //Wheel height
                Gizmos.color = Color.gray;
                Gizmos.DrawRay(suspensionTransforms[i].position - (transform.up * suspensionLength[i]), -transform.up * wheelRadius);

                //Make a thing to seperate the forces so you can see them all individually

                Gizmos.color = Color.red;
                Gizmos.DrawRay(suspensionTransforms[i].position, lateralForce * suspensionTransforms[i].right);

                Gizmos.color = Color.green;
                Gizmos.DrawRay(suspensionTransforms[i].position, suspensionForce * suspensionTransforms[i].up);

                Gizmos.color = Color.blue;
                Gizmos.DrawRay(suspensionTransforms[i].position, driveForce * suspensionTransforms[i].forward);

                //Combined forces
                //Gizmos.color = new Color(finalForceWorld.x, finalForceWorld.y, finalForceWorld.z);
                //Gizmos.DrawRay(suspensionTransforms[i].position, finalForceWorld / finalForceWorld.magnitude);

                Gizmos.color = Color.magenta;
                Gizmos.DrawSphere(suspensionTransforms[i].position + (-transform.up * (suspensionLength[i] + wheelRadius)), 0.1f);
            }
        }
    }

    void Reset()
    {
        rb = GetComponent<Rigidbody>();

        suspensionTravel = 0.2f;
        restLength = 0.5f;
        //stiffness = 2000f;
        //dampingStiffness = 250f;

        stiffness = 1700f;
        dampingStiffness = 900f;

        wheelRadius = 0.43f;

        frictionCoefficient = 200f;

        maximumSteeringAngle = 30f;

        steerForce = 0.5f;


        vehicleDragCoeficient = 0.534f;
        engineForce = 100;

        spiderCar = false;

    }
}