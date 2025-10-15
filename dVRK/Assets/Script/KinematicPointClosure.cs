using UnityEngine;

public class KinematicPointClosure : MonoBehaviour
{
    [Header("Link & Anchor")]
    public Transform linkA;
    public Transform anchorOnA;   // figlio di linkA (il punto target)
    public Transform linkB;
    public Transform anchorOnB;   // figlio di linkB (il punto da far coincidere)

    public enum Mode { PositionOnly, FullPose }
    [Header("Modo di chiusura")]
    public Mode mode = Mode.PositionOnly;

    [Header("Aggiornamento")]
    public bool useFixedUpdate = false;   // true se interagisci con altri Rigidbody in FixedUpdate
    public float positionEpsilon = 1e-6f; // tolleranza per evitare micro-update

    [Header("Debug")]
    public bool drawGizmos = true;

    void LateUpdate()
    {
        if (!useFixedUpdate) ApplyConstraint();
    }

    void FixedUpdate()
    {
        if (useFixedUpdate) ApplyConstraint();
    }

    private void ApplyConstraint()
    {
        if (linkA == null || linkB == null || anchorOnA == null || anchorOnB == null) return;

        if (mode == Mode.PositionOnly)
        {
            // Coincidenza del SOLO punto: sposto B in modo che anchorB arrivi esattamente su anchorA.
            Vector3 delta = anchorOnA.position - anchorOnB.position;
            if (delta.sqrMagnitude > positionEpsilon * positionEpsilon)
            {
                if (linkB.TryGetComponent<Rigidbody>(out var rb) && !rb.isKinematic)
                    rb.MovePosition(rb.position + delta);
                else
                    linkB.position += delta;
            }
        }
        else // FullPose
        {
            // Coincidenza del punto E dell'orientamento dell'ancora (6 DoF).
            // Formula esatta (nessuna iterazione):   B = A * inv( (anchorOnB) local )
            // Rotazione:
            Quaternion rotB = anchorOnA.rotation * Quaternion.Inverse(anchorOnB.localRotation);
            // Posizione: pB = pA - Rb * p_local
            Vector3 posB = anchorOnA.position - (rotB * anchorOnB.localPosition);

            if (linkB.TryGetComponent<Rigidbody>(out var rb) && !rb.isKinematic)
            {
                rb.MoveRotation(rotB);
                rb.MovePosition(posB);
            }
            else
            {
                linkB.SetPositionAndRotation(posB, rotB);
            }
        }
    }

    void OnDrawGizmos()
    {
        if (!drawGizmos || anchorOnA == null || anchorOnB == null) return;
        Gizmos.DrawLine(anchorOnA.position, anchorOnB.position);
        Gizmos.DrawSphere(anchorOnA.position, 0.005f);
        Gizmos.DrawWireSphere(anchorOnB.position, 0.005f);
    }
}