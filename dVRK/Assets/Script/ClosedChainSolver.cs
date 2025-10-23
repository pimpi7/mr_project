using UnityEngine;
using System;
using System.Collections.Generic;

[DefaultExecutionOrder(1000)] // esegui dopo i controller dei giunti
public class ClosedChainSolver : MonoBehaviour
{
    [Serializable]
    public class Body
    {
        public Transform transform;
        [Tooltip("Se presente e non cinematic, verrà usato MovePosition/MoveRotation. Consigliato: kinematic = true.")]
        public Rigidbody rb;

        [Range(0f, 1f)] public float invMass = 1f;      // 0 = fisso (non si muove), 1 = libero
        [Range(0f, 1f)] public float invRotInertia = 1f; // peso rotazionale (0 = non ruota)

        public void ApplyPositionDelta(Vector3 dp, bool useFixed)
        {
            if (rb != null && !rb.isKinematic && useFixed)
                rb.MovePosition(rb.position + dp);
            else
                transform.position += dp;
        }

        public void ApplyRotationDelta(Quaternion dq, bool useFixed)
        {
            if (dq == Quaternion.identity) return;
            if (rb != null && !rb.isKinematic && useFixed)
                rb.MoveRotation(dq * rb.rotation);
            else
                transform.rotation = dq * transform.rotation;
        }
    }

    [Serializable]
    public class Constraint
    {
        [Header("Bodies")]
        public Body A;
        public Body B;

        [Header("Anchors (local)")]
        public Vector3 localAnchorA; // punto su A in local space
        public Vector3 localAnchorB; // punto su B in local space

        [Header("Positional")]
        [Range(0f, 1f)] public float positionalStiffness = 1f; // 1 = chiusura rigida
        [Tooltip("Baumgarte beta (stabilizzazione). 0.3–0.7 consigliato.")]
        [Range(0f, 1f)] public float beta = 0.5f;
        [Tooltip("Tolleranza per evitare micro-oscillazioni")]
        public float positionEpsilon = 1e-6f;

        [Header("Orientational (optional)")]
        public bool alignAxis = false;
        [Tooltip("Asse locale in A che vuoi allineare all'asse locale in B")]
        public Vector3 localAxisA = Vector3.forward;
        public Vector3 localAxisB = Vector3.forward;
        [Range(0f, 1f)] public float rotationalStiffness = 1f; // 1 = allineamento rigido
        public float angleEpsilonDeg = 0.01f;

        // cache (runtime)
        [NonSerialized] public Vector3 worldAnchorA, worldAnchorB;
        [NonSerialized] public Vector3 worldAxisA, worldAxisB;
    }

    [Header("Constraints list")]
    public List<Constraint> constraints = new();

    [Header("Solver")]
    [Tooltip("Numero di iterazioni di proiezione per frame. 8–20 va bene; aumenta se noti drift.")]
    [Range(1, 64)] public int iterations = 12;
    [Tooltip("Usa FixedUpdate se interagisci con RB non-kinematic.")]
    public bool useFixedUpdate = false;

    [Header("Debug")]
    public bool drawGizmos = true;
    public float gizmoSphere = 0.004f;

    void LateUpdate()
    {
        if (!useFixedUpdate) Solve(Time.deltaTime);
    }

    void FixedUpdate()
    {
        if (useFixedUpdate) Solve(Time.fixedDeltaTime);
    }

    void Solve(float dt)
    {
        if (constraints == null || constraints.Count == 0) return;

        // Iterative Gauss–Seidel
        for (int it = 0; it < iterations; it++)
        {
            foreach (var c in constraints)
            {
                if (c.A == null || c.A.transform == null || c.B == null || c.B.transform == null) continue;

                // --- POSIZIONALE: pA == pB ---
                c.worldAnchorA = c.A.transform.TransformPoint(c.localAnchorA);
                c.worldAnchorB = c.B.transform.TransformPoint(c.localAnchorB);

                Vector3 errP = c.worldAnchorB - c.worldAnchorA; // vogliamo zero
                float errLen2 = errP.sqrMagnitude;

                if (errLen2 > c.positionEpsilon * c.positionEpsilon)
                {
                    float wA = Mathf.Max(0f, c.A.invMass);
                    float wB = Mathf.Max(0f, c.B.invMass);
                    float wSum = wA + wB;
                    if (wSum > 0f)
                    {
                        // Baumgarte: spingi proporzionalmente all'errore e a beta/dt
                        float baumgarte = (c.beta > 0f && dt > 0f) ? (c.beta / dt) : 0f;

                        // correzione (PBD style): proietta con stiffness
                        Vector3 corr = -(errP) * c.positionalStiffness;

                        // aggiungi una spinta proporzionale all'errore (stabilizzazione)
                        corr += -(errP) * baumgarte * 0.001f; // piccolo fattore per non irrigidire troppo

                        Vector3 dA = (wA / wSum) * corr;
                        Vector3 dB = -(wB / wSum) * corr;

                        c.A.ApplyPositionDelta(dA, useFixedUpdate);
                        c.B.ApplyPositionDelta(dB, useFixedUpdate);
                    }
                }

                // --- ORIENTAZIONALE: asseA == asseB (opzionale) ---
                if (c.alignAxis && c.rotationalStiffness > 0f)
                {
                    c.worldAxisA = c.A.transform.TransformDirection(c.localAxisA.normalized);
                    c.worldAxisB = c.B.transform.TransformDirection(c.localAxisB.normalized);

                    // Rotazione che porta B→A
                    Quaternion qErr = Quaternion.FromToRotation(c.worldAxisB, c.worldAxisA);
                    float ang; Vector3 axis;
                    qErr.ToAngleAxis(out ang, out axis);
                    if (float.IsNaN(axis.x)) axis = Vector3.zero; // sicurezza

                    if (ang > c.angleEpsilonDeg && axis.sqrMagnitude > 1e-8f)
                    {
                        float wA = Mathf.Max(0f, c.A.invRotInertia);
                        float wB = Mathf.Max(0f, c.B.invRotInertia);
                        float wSum = wA + wB;
                        if (wSum > 0f)
                        {
                            // Piccolo step nella direzione dell’errore (slerp scalato)
                            float step = c.rotationalStiffness; // 0..1 per iterazione
                            Quaternion dqA = Quaternion.AngleAxis(+ang * step * (wA / wSum), axis);
                            Quaternion dqB = Quaternion.AngleAxis(-ang * step * (wB / wSum), axis);

                            c.A.ApplyRotationDelta(dqA, useFixedUpdate);
                            c.B.ApplyRotationDelta(dqB, useFixedUpdate);
                        }
                    }
                }
            }
        }
    }

    void OnDrawGizmos()
    {
        if (!drawGizmos || constraints == null) return;
        Gizmos.matrix = Matrix4x4.identity;
        foreach (var c in constraints)
        {
            if (c == null || c.A == null || c.B == null) continue;
            Vector3 pA = (c.A.transform != null) ? c.A.transform.TransformPoint(c.localAnchorA) : Vector3.zero;
            Vector3 pB = (c.B.transform != null) ? c.B.transform.TransformPoint(c.localAnchorB) : Vector3.zero;
            Gizmos.DrawLine(pA, pB);
            Gizmos.DrawSphere(pA, gizmoSphere);
            Gizmos.DrawWireSphere(pB, gizmoSphere);
        }
    }
}
