using UnityEngine;

public class MasterSlaveJointSync : MonoBehaviour
{
    [Header("Master Joint")]
    public ArticulationBody masterJoint;

    [Header("Slave Joints")]
    public ArticulationBody[] slaveJoints; // assign 4 in inspector

    void FixedUpdate()
    {
        if (masterJoint == null || slaveJoints.Length == 0)
            return;

        float masterAngle = masterJoint.jointPosition[0]; // joint angle in radians

        foreach (ArticulationBody slave in slaveJoints)
        {
            if (slave == null)
                continue;

            ArticulationDrive drive = slave.xDrive;
            drive.target = masterAngle * Mathf.Rad2Deg; // optional: convert to degrees if needed
            drive.stiffness = 100000f;  // very high = faster response
            drive.damping = 10000f;     // high damping prevents oscillation
            slave.xDrive = drive;
            Debug.Log($"Slave '{slave.name}': target = {drive.target:F2}°, position = {slave.jointPosition[0] * Mathf.Rad2Deg:F2}°");
        }
    }
}
