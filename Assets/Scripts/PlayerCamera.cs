using UnityEngine;
using System.Threading.Tasks;

using ShinduPlayer;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] Transform baseCameraPoint, cameraFocusPoint;
    [SerializeField] float cameraSensitivity = 300f, moveSpeed = 5, rotationSpeed = 25, focusAlignSpeed = 30f, cameraTiltSpeed = 10f;
    [SerializeField] LayerMask obstacleLayer;

    [SerializeField] float DEBUG_ANGLE = 0;
    float xRotation = 0f, yRotation = 0f;
    bool turningCamera = false, isClipping = false;
    Vector3 originalCameraPosition, DEBUG_HIT_POSITION;
    Quaternion originalCameraRotation;

    void Start()
    {
        originalCameraPosition = Camera.main.transform.localPosition;
        originalCameraRotation = Camera.main.transform.localRotation;
    }

    bool CheckFocus()
    {
        return Input.GetButton("LeftBumper") && PlayerManager.Instance.isGrounded;
    }

    void LateUpdate()
    {
        PlayerManager.Instance.focused = CheckFocus();
        if (PlayerManager.Instance.focused)
        {
            FreeView();
        } else
            {
                xRotation = yRotation = 0f; // reset rotation value references
                AlignCamera();
            }
            
        _ = CheckCameraClip(); // run and forget about it
    }

    // player position/rotation is locked and the camera can be rotated in a fixed motion degree
    void FreeView()
    {
        // rotate camera pivot so cameras initial forward is based on the player object forward
        Quaternion targetRotation = Quaternion.LookRotation(baseCameraPoint.forward);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, focusAlignSpeed * Time.deltaTime);
        
        if (!isClipping)
        {
            // smoothly move the camera
            Camera.main.transform.position = Vector3.Lerp(
                Camera.main.transform.position,
                cameraFocusPoint.position,
                10f * Time.deltaTime
            );
        }

        // rotate camera via left-joystick
        float x = Input.GetAxis("Horizontal") * cameraSensitivity * Time.deltaTime;
        float z = Input.GetAxis("Vertical") * cameraSensitivity * Time.deltaTime;

        xRotation -= z;
        xRotation = Mathf.Clamp(xRotation, -60f, 60f); // clamp vertical rotation

        yRotation += x;
        yRotation = Mathf.Clamp(yRotation, -60f, 60f);

        Camera.main.transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);
    }

    // camera locks either its current orientation or an enemy
    void LockView()
    {
        
    }

    //##############################################################################
    //##########################  AUTO CAMERA LOGIC  ###############################
    //##############################################################################

    float GetAngle()
    {
        float angle = Vector3.Angle(baseCameraPoint.forward, transform.forward);
        return angle;
    }

    bool isMoving()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 inputDir = new Vector3(x, 0f, z).normalized;
        return inputDir != Vector3.zero;
    }

    void AlignCamera()
    {
        // smoothly rotate the camera
        float angleDiff = Quaternion.Angle(Camera.main.transform.localRotation, originalCameraRotation);

        if (angleDiff > 0.1f) // threshold in degrees
        {
            Camera.main.transform.localRotation = Quaternion.Slerp(
                Camera.main.transform.localRotation,
                originalCameraRotation,
                cameraTiltSpeed * Time.deltaTime
            );
        } else
            {
                // snap to target
                Camera.main.transform.localRotation = originalCameraRotation;
            }

        // smoothly move the camera-pivot
        transform.position = Vector3.Lerp(transform.position, baseCameraPoint.position, moveSpeed * Time.deltaTime);
        bool suggestTurn = GetAngle() >= 60 && GetAngle() <= 100;

        if (!turningCamera && suggestTurn)
        {
            // only run this once and forget about it
            _ = TurnCamera();
        }
    }

    async Task TurnCamera() // runs in the background without halting the main thread
    {
        turningCamera = true;

        while (isMoving() && (GetAngle() > 0.1f && GetAngle() < 100f)) // we never fully reach 0
        {
            // rotate camera pivot
            Quaternion targetRotation = Quaternion.LookRotation(baseCameraPoint.forward);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            await Task.Yield(); // needed wait
        }

        turningCamera = false;
    }

    async Task CheckCameraClip()
    {
        Transform cam = Camera.main.transform;

        RaycastHit hit;
        Vector3 rayDir = (cam.position - baseCameraPoint.position).normalized;

        bool rayLanded = Physics.Raycast(baseCameraPoint.position, rayDir, out hit, 5, obstacleLayer);

        if (rayLanded)
        {
            Debug.Log("Camera Clipping Obstacle!");
            isClipping = true;

            // try to find the best position for the camera where the camera-view
            // can see through meshes
            Vector3 dir2base = (baseCameraPoint.position - hit.point).normalized;
            Vector3 optimalCamPos = hit.point + (dir2base * 0.25f);

            DEBUG_HIT_POSITION = optimalCamPos;

            // move the camera forward until its outside the wall
            cam.position = Vector3.Lerp(cam.position, optimalCamPos, 10f * Time.deltaTime);
        } else
            {
                // move camera back to its original place
                cam.localPosition = Vector3.Lerp(cam.localPosition, originalCameraPosition, 10f * Time.deltaTime);
                isClipping = false;
            }

        await Task.Yield();
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawLine(baseCameraPoint.position, baseCameraPoint.position + baseCameraPoint.forward);
        
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(baseCameraPoint.position, baseCameraPoint.position + transform.forward);

        Transform cam = Camera.main.transform;

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(DEBUG_HIT_POSITION, 0.25f);

        DEBUG_ANGLE = GetAngle();
    }
}//EndScript