using UnityEngine;


public class GripperKeyboardController : MonoBehaviour
{
    [Header("Riferimenti ai Giunti del Gripper")]
    [Tooltip("Assegna l'ArticulationBody del giunto destro del gripper (J3_dx_TOOL1)")]
    public ArticulationBody gripperRightJoint;

    [Tooltip("Assegna l'ArticulationBody del giunto sinistro del gripper (J3_sx_TOOL1)")]
    public ArticulationBody gripperLeftJoint;

    [Header("Parametri di Controllo")]
    [Tooltip("Velocità di apertura e chiusura del gripper in gradi al secondo.")]
    public float speed = 50.0f;

    [Tooltip("L'angolo massimo di apertura per una singola pinza in gradi. Valore positivo.")]
    public float maxOpeningAngle = 45.0f;

    [Tooltip("L'angolo minimo di chiusura (spesso 0 o un valore leggermente negativo).")]
    public float minOpeningAngle = 0.0f;

    [Header("Tasti di Input")]
    [Tooltip("Tasto per aprire il gripper.")]
    public KeyCode openKey;

    [Tooltip("Tasto per chiudere il gripper.")]
    public KeyCode closeKey;

    private float _currentTargetAngle;

    void Start()
    {
        if (gripperRightJoint == null || gripperLeftJoint == null)
        {
            Debug.LogError("Assegnare entrambi i giunti del gripper nell'Inspector prima di avviare la scena!");
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
            movementDirection = 1f; // Apri
        }
        else if (Input.GetKey(closeKey))
        {
            movementDirection = -1f; // Chiudi
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
        leftDrive.target = targetAngleDegrees; // Non serve il segno negativo qui perchè già assegnato sopra
        gripperLeftJoint.xDrive = leftDrive;
    }
}