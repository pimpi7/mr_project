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
        public KeyCode positiveKey; 
        public KeyCode negativeKey;

        [Header("Settings")]
        [Tooltip("Speed. Use ~0.1 for Prismatic (sliding) joints, ~30-100 for Revolute (rotating) joints.")]
        public float speed = 50.0f;

        [Tooltip("Invert the direction of movement?")]
        public bool invertDirection = false;

        [HideInInspector] 
        public float currentTargetPosition;
    }

    [Header("Joint List")]
    [Tooltip("Add as many joints as you need here.")]
    public List<JointMap> joints = new List<JointMap>();

    void Start()
    {

        foreach (var map in joints)
        {
            if (map.jointBody == null)
            {
                Debug.LogError($"Joint Body missing in configuration: {map.jointName}");
                continue;
            }

            map.currentTargetPosition = map.jointBody.jointPosition[0];
            
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

            if (Input.GetKey(map.positiveKey))
            {
                inputVal = 1f;
            }
            else if (Input.GetKey(map.negativeKey))
            {
                inputVal = -1f;
            }

            if (inputVal != 0f)
            {
                if (map.invertDirection) inputVal *= -1f;

                float change = inputVal * map.speed * Time.fixedDeltaTime;
                map.currentTargetPosition += change;

                var drive = map.jointBody.xDrive;

                map.currentTargetPosition = Mathf.Clamp(map.currentTargetPosition, drive.lowerLimit, drive.upperLimit);

                drive.target = map.currentTargetPosition;
                map.jointBody.xDrive = drive;
            }
        }
    }
}