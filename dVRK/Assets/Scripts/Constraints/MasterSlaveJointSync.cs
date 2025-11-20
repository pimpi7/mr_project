using UnityEngine;

public class MasterSlaveJointSync : MonoBehaviour
{
    [Header("Master Joint")]
    public ArticulationBody masterJoint;

    [Header("Slave Joints")]
    public ArticulationBody[] slaveJoints; 

    void FixedUpdate()
    {
        if (masterJoint == null || slaveJoints.Length == 0)
            return;

        float masterAngle = masterJoint.jointPosition[0]; 

        foreach (ArticulationBody slave in slaveJoints)
        {
            if (slave == null)
                continue;

            ArticulationDrive drive = slave.xDrive;
            drive.target = masterAngle * Mathf.Rad2Deg; 
            drive.stiffness = 100000f;  
            drive.damping = 10000f;     
            slave.xDrive = drive;
            Debug.Log($"Slave '{slave.name}': target = {drive.target:F2}°, position = {slave.jointPosition[0] * Mathf.Rad2Deg:F2}°");
        }
    }
}
