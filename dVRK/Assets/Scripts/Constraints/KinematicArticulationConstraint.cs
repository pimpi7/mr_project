using UnityEngine;

public class KinematicArticulationConstraint : MonoBehaviour
{
    public enum Mode { PositionOnly, FullPose }

    [Header("Anchors (children dei link interessati)")]
    public Transform anchorOnA;
    public Transform anchorOnB;

    [Header("Giunti (ordine dalla base verso la punta)")]
    public ArticulationBody[] joints;     // i 2 (o più) AB che correggono il vincolo

    [Header("Impostazioni")]
    public Mode mode = Mode.FullPose;
    [Range(0f, 1f)] public float posGain = 0.4f;
    [Range(0f, 1f)] public float rotGain = 0.3f;
    public float posTolerance = 1e-4f;
    public float angToleranceDeg = 0.05f;
    public float maxDeltaDegPerIter = 1.5f;

    [HideInInspector] public Vector3 bindAPosLocal, bindBPosLocal;
    [HideInInspector] public Quaternion bindARotLocal, bindBRotLocal;

    void OnEnable()  { KinematicConstraintABSolver.Register(this); }
    void OnDisable() { KinematicConstraintABSolver.Unregister(this); }

    public void BindLocal()
    {
        if (anchorOnA && anchorOnA.parent)
        {
            bindAPosLocal = anchorOnA.parent.InverseTransformPoint(anchorOnA.position);

            /*store the rotation taking account difference in framing from URDF to Unity*/
            Vector3 urdfRPYDegrees = new Vector3(90f, 180f, 145f); //convert in degrees
            Quaternion urdfCorrection = Quaternion.Euler(urdfRPYDegrees.z, urdfRPYDegrees.x, urdfRPYDegrees.y); //apply rotation to correct
            bindARotLocal = Quaternion.Inverse(anchorOnA.parent.rotation) * anchorOnA.rotation * Quaternion.Inverse(urdfCorrection);
        }
        if (anchorOnB && anchorOnB.parent)
        {
            bindBPosLocal = anchorOnB.parent.InverseTransformPoint(anchorOnB.position);
            bindBRotLocal = Quaternion.Inverse(anchorOnB.parent.rotation) * anchorOnB.rotation;
        }
    }

    public void RestoreFromLocal()
    {
        if (anchorOnA && anchorOnA.parent)
        {
            anchorOnA.position = anchorOnA.parent.TransformPoint(bindAPosLocal);
            anchorOnA.rotation = anchorOnA.parent.rotation * bindARotLocal;
        }
        if (anchorOnB && anchorOnB.parent)
        {
            anchorOnB.position = anchorOnB.parent.TransformPoint(bindBPosLocal);
            anchorOnB.rotation = anchorOnB.parent.rotation * bindBRotLocal;
        }
    }
}

