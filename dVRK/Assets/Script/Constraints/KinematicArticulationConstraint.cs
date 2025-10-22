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
            bindARotLocal = Quaternion.Inverse(anchorOnA.parent.rotation) * anchorOnA.rotation;
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

