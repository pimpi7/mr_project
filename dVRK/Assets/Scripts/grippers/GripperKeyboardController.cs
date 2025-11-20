using UnityEngine;


public class GripperKeyboardController : MonoBehaviour
{
    [Header("Reference to gripper joints")]
    [Tooltip("Assign the ArticulationBody of the right joint gripper (J3_dx_TOOL1)")]
    public ArticulationBody gripperRightJoint;

    [Tooltip("Assign the ArticulationBody of the left joint gripper (J3_sx_TOOL1)")]
    public ArticulationBody gripperLeftJoint;

    [Header("Parametri di Controllo")]
    [Tooltip("Velocità di apertura e chiusura del gripper in gradi al secondo.")]
    public float speed = 50.0f;

    [Tooltip("Max opening angle for a single gripper in degrees. Positive value.")]
    public float maxOpeningAngle = 45.0f;

    [Tooltip("Minimum close angle (Should be 0).")]
    public float minOpeningAngle = 0.0f;

    [Header("Input keys")]
    [Tooltip("Key for opening the gripper.")]
    public KeyCode openKey;

    [Tooltip("Key for closing the gripper.")]
    public KeyCode closeKey;

    private float _currentTargetAngle;

    void Start()
    {
        if (gripperRightJoint == null || gripperLeftJoint == null)
        {
            Debug.LogError("ASSIGN THE JOINT GRIPPERS IN THE SCENE!");
            this.enabled = false; 
            return;
        }

        _currentTargetAngle = gripperRightJoint.jointPosition[0] * Mathf.Rad2Deg;
    }

    void FixedUpdate()
    {
        float movementDirection = 0f;
        if (Input.GetKey(openKey))
        {
            movementDirection = 1f; 
        }
        else if (Input.GetKey(closeKey))
        {
            movementDirection = -1f; 
        }

        if (movementDirection != 0f)
        {
            _currentTargetAngle += movementDirection * speed * Time.fixedDeltaTime;

            _currentTargetAngle = Mathf.Clamp(_currentTargetAngle, minOpeningAngle, maxOpeningAngle);
            
            SetGripperTargetAngle(_currentTargetAngle);
        }
    }



    private void SetGripperTargetAngle(float targetAngleDegrees)
    {
        ArticulationDrive rightDrive = gripperRightJoint.xDrive;
        rightDrive.target = targetAngleDegrees;
        gripperRightJoint.xDrive = rightDrive;

        ArticulationDrive leftDrive = gripperLeftJoint.xDrive;
        leftDrive.target = targetAngleDegrees; 
        gripperLeftJoint.xDrive = leftDrive;
    }
}
