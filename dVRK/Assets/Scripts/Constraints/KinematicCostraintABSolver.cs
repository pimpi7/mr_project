using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(11000)] // esegue dopo la maggior parte dei controller (mettilo > JointControl)
public class KinematicConstraintABSolver : MonoBehaviour
{
    static readonly List<KinematicArticulationConstraint> cons = new();
    public static void Register(KinematicArticulationConstraint c) { if (!cons.Contains(c)) cons.Add(c); }
    public static void Unregister(KinematicArticulationConstraint c) { cons.Remove(c); }

    [Header("Solver")]
    [Min(1)] public int iterations = 12;
    public bool useFixedUpdate = true;   // con Articulation/PhysX: sì
    public bool autoBindAnchorsOnStart = true;

    void Start() { if (autoBindAnchorsOnStart) foreach (var c in cons) c.BindLocal(); }

    void FixedUpdate() { if (useFixedUpdate) SolveAll(); }
    void LateUpdate()  { if (!useFixedUpdate) SolveAll(); }

    void SolveAll()
    {
        if (cons.Count == 0) return;

        foreach (var c in cons) c.RestoreFromLocal();

        for (int it = 0; it < iterations; it++)
            for (int i = 0; i < cons.Count; i++)
                SolveOnce(cons[i]);
    }

    void SolveOnce(KinematicArticulationConstraint c)
    {
        if (!c || !c.anchorOnA || !c.anchorOnB || c.joints == null || c.joints.Length == 0) return;

        // errore pos
        Vector3 pA = c.anchorOnA.position;
        Vector3 pB = c.anchorOnB.position;
        Vector3 eP = pA - pB; // m

        // errore orient (angolo-asse)
        Quaternion dq = c.anchorOnA.rotation * Quaternion.Inverse(c.anchorOnB.rotation);
        dq.ToAngleAxis(out float angDegRaw, out Vector3 axis);
        float angDeg = float.IsNaN(angDegRaw) ? 0f : (angDegRaw > 180f ? 360f - angDegRaw : angDegRaw);
        Vector3 axisSigned = (angDegRaw > 180f ? -axis : axis);

        bool posOk = eP.magnitude <= c.posTolerance;
        bool rotOk = (c.mode == KinematicArticulationConstraint.Mode.PositionOnly) || (angDeg <= c.angToleranceDeg);
        if (posOk && rotOk) return;

        // Per ogni giunto: proiettiamo l'errore sulla sua DOF (Jacobian-Transpose lite)
        foreach (var j in c.joints)
        {
            if (!j) continue;
            // assumiamo 1-DOF: Revolute (Twist X). In Unity AB l'asse del giunto è j.transform.localRotation drive? 
            // Usciamo nel mondo: l'asse X del giunto in mondo
            Vector3 axisWorld = j.transform.TransformDirection(Vector3.forward); // per ArticulationBody: X è l'asse dell'hinge
            Vector3 jointPos = j.transform.position;

            // contributo pos: quanto ruotare per spostare pB verso pA
            // v = axis x (r); dError ~ axis x (r) * dTheta
            Vector3 r = pB - jointPos;
            Vector3 v = Vector3.Cross(axisWorld, r);     // direzione di spostamento del punto quando aumenta il giunto
            float denom = Vector3.Dot(v, v) + 1e-12f;
            float dThetaPos = Vector3.Dot(v, eP) / denom; // rad (piccolo)

            // contributo orient: se FullPose, cerca di allineare orientazioni proiettando l'errore angolare sull'asse
            float dThetaRot = 0f;
            if (c.mode == KinematicArticulationConstraint.Mode.FullPose && angDeg > c.angToleranceDeg)
            {
                Vector3 axisWorldNorm = axisWorld.normalized;
                float sign = Mathf.Sign(Vector3.Dot(axisWorldNorm, axisSigned.normalized));
                dThetaRot = sign * Mathf.Deg2Rad * angDeg;
            }

            // mix e clamp
            float dTheta = c.posGain * dThetaPos + c.rotGain * dThetaRot;
            float maxStep = Mathf.Deg2Rad * c.maxDeltaDegPerIter;
            dTheta = Mathf.Clamp(dTheta, -maxStep, +maxStep);

            // applica al drive target (in gradi)
            var drive = j.xDrive;
            drive.target += dTheta * Mathf.Rad2Deg;
            j.xDrive = drive;
        }
    }
}
