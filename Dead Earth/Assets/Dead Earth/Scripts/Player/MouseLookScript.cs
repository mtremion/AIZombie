using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLookScript : MonoBehaviour
{
    #region Serialize Field
    [Header("Caractéristiques")]
    [Tooltip("Sensitivité de la souris.")]
    [SerializeField] float mouseSensitivity = 100f;
    [Tooltip("Transform du FPSPlayer.")]
    [SerializeField] Transform playerBody = null;
    #endregion

    #region Private & Protected
    float _xRotation = 0f;
    #endregion

    #region System
    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -60f, 90f);

        transform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * mouseX);
    }
    #endregion
}
