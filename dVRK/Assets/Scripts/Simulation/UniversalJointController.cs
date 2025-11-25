using UnityEngine;
using System.Collections.Generic;

public class UniversalJointController : MonoBehaviour
{
    [System.Serializable]
    public class JointMap
    {
        [Header("Joint Configuration")]
        [Tooltip("Name this to help you remember which joint it is (e.g., 'Base Rotation' or 'Gripper')")]
        public string jointName;

        [Tooltip("The ArticulationBody component you want to move.")]
        public ArticulationBody jointBody;

        [Header("Input")]
        public KeyCode positiveKey; // Move Right / Forward / Open
        public KeyCode negativeKey; // Move Left / Back / Close

        [Header("Settings")]
        [Tooltip("Speed. Use ~0.1 for Prismatic (sliding) joints, ~30-100 for Revolute (rotating) joints.")]
        public float speed = 50.0f;

        [Tooltip("Invert the direction of movement?")]
        public bool invertDirection = false;

        // Internal variable to track where we want the joint to be
        [HideInInspector] 
        public float currentTargetPosition;
    }

    [Header("Joint List")]
    [Tooltip("Add as many joints as you need here.")]
    public List<JointMap> joints = new List<JointMap>();

    void Start()
    {
        // Initialize the target position for every joint in the list
        // so they don't snap to 0 when the game starts.
        foreach (var map in joints)
        {
            if (map.jointBody == null)
            {
                Debug.LogError($"Joint Body missing in configuration: {map.jointName}");
                continue;
            }

            // Get the current physical position of the joint
            // Index 0 is the primary degree of freedom for standard joints
            map.currentTargetPosition = map.jointBody.jointPosition[0];
            
            // If the joint is revolute (rotational), Unity stores it in Radians, 
            // but the drive target expects Degrees (usually).
            if (map.jointBody.jointType == ArticulationJointType.RevoluteJoint)
            {
                map.currentTargetPosition *= Mathf.Rad2Deg;
            }
        }
    }

    void FixedUpdate()
    {
        foreach (var map in joints)
        {
            if (map.jointBody == null) continue;

            float inputVal = 0f;

            // Check for key inputs
            if (Input.GetKey(map.positiveKey))
            {
                inputVal = 1f;
            }
            else if (Input.GetKey(map.negativeKey))
            {
                inputVal = -1f;
            }

            // If keys are pressed, calculate new position
            if (inputVal != 0f)
            {
                // Apply Inversion if checked
                if (map.invertDirection) inputVal *= -1f;

                // Calculate the change
                float change = inputVal * map.speed * Time.fixedDeltaTime;
                map.currentTargetPosition += change;

                // Get the drive to read limits
                var drive = map.jointBody.xDrive;

                // Clamp the target so we don't exceed the joint's physical limits
                // The drive limits are usually already set in the ArticulationBody component in the Inspector
                map.currentTargetPosition = Mathf.Clamp(map.currentTargetPosition, drive.lowerLimit, drive.upperLimit);

                // Apply the new target
                drive.target = map.currentTargetPosition;
                map.jointBody.xDrive = drive;
            }
        }
    }
}