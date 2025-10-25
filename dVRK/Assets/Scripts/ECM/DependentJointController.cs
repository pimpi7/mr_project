using UnityEngine;
using Unity.Robotics.UrdfImporter.Control; // We need this to access the 'RotationDirection' enum

/// <summary>
/// This script controls dependent joints based on the position of a main joint.
/// VERSION 3: This version is fully decoupled. It infers whether a joint is being
/// manually controlled by checking the 'direction' property on the 'JointControl'
/// component, which is managed by the main Controller.cs.
/// </summary>
public class DependentJointController : MonoBehaviour
{
    [Header("Main Driver Joint")]
    [Tooltip("The ArticulationBody of the main joint that drives the others (J2).")]
    public ArticulationBody mainJoint;

    [Header("Dependent Joints")]
    [Tooltip("The ArticulationBody for the Jp21 joint.")]
    public ArticulationBody dependentJoint21;

    [Tooltip("The ArticulationBody for the Jp22 joint.")]
    public ArticulationBody dependentJoint22;

    [Tooltip("The ArticulationBody for the Jp23 joint.")]
    public ArticulationBody dependentJoint23;

    [Header("Dependent Joint Drive Settings")]
    [Tooltip("High stiffness ensures the joints rigidly follow their target.")]
    public float stiffness = 100000f;
    [Tooltip("High damping prevents oscillations.")]
    public float damping = 1000f;

    // Private references to the JointControl scripts on each dependent joint
    private JointControl jointControl21;
    private JointControl jointControl22;
    private JointControl jointControl23;

    void Start()
    {
        // Get the JointControl components that the main Controller.cs adds to every joint.
        // We will use these to check if manual control is active.
        if (dependentJoint21 != null) jointControl21 = dependentJoint21.GetComponent<JointControl>();
        if (dependentJoint22 != null) jointControl22 = dependentJoint22.GetComponent<JointControl>();
        if (dependentJoint23 != null) jointControl23 = dependentJoint23.GetComponent<JointControl>();

        // This script will now manage its own joints' drive settings to ensure they are always correct.
        SetupDependentJoint(dependentJoint21);
        SetupDependentJoint(dependentJoint22);
        SetupDependentJoint(dependentJoint23);
    }

    void FixedUpdate()
    {
        // Safety check for the main joint
        if (mainJoint == null) return;

        // --- 1. Read the Main Joint's Position ---
        float mainJointPositionRad = mainJoint.jointPosition[0];

        // --- 2. Calculate the Target Positions for Dependent Joints ---
        float targetPosRad21 = 1.0f * mainJointPositionRad;
        float targetPosRad22 = -1.0f * mainJointPositionRad;
        float targetPosRad23 = 1.0f * mainJointPositionRad;

        // --- 3. Apply drive targets, but ONLY if the joint is not being manually moved ---
        // Check the flag on the JointControl script. If direction is 'None', we are in control.
        if (jointControl21 != null && jointControl21.direction == RotationDirection.None)
        {
            SetDriveTarget(dependentJoint21, targetPosRad21 * Mathf.Rad2Deg);
        }

        if (jointControl22 != null && jointControl22.direction == RotationDirection.None)
        {
            SetDriveTarget(dependentJoint22, targetPosRad22 * Mathf.Rad2Deg);
        }

        if (jointControl23 != null && jointControl23.direction == RotationDirection.None)
        {
            SetDriveTarget(dependentJoint23, targetPosRad23 * Mathf.Rad2Deg);
        }
    }
    
    /// <summary>
    /// Sets the drive properties for a dependent joint to ensure it follows the target rigidly.
    /// This also ensures our high stiffness/damping settings are reapplied if the main Controller changes them.
    /// </summary>
    private void SetupDependentJoint(ArticulationBody body)
    {
        if (body != null)
        {
            ArticulationDrive drive = body.xDrive;
            drive.stiffness = stiffness;
            drive.damping = damping;
            body.xDrive = drive;
        }
    }

    /// <summary>
    /// Sets the primary axis drive target for a given Articulation Body.
    /// </summary>
    private void SetDriveTarget(ArticulationBody body, float targetDegrees)
    {
        // We must re-assert our desired stiffness and damping in case the other script changed it.
        ArticulationDrive drive = body.xDrive;
        if(drive.stiffness != stiffness) drive.stiffness = stiffness;
        if(drive.damping != damping) drive.damping = damping;

        drive.target = targetDegrees;
        body.xDrive = drive;
    }
}