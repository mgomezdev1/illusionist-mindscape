using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController controller;
    private CharacterController ownController;
    public Player Player { get; private set; }

    [SerializeField]
    private Transform cam;
    [SerializeField]
    private float walkSpeed;
    [SerializeField]
    private float sprintSpeed;
    private float currentSpeed;
    private float turnSmoothTime = 0.1f;
    private float turnSmoothVelocity = 0.1f;

    void Awake()
    {
        Player = GetComponent<Player>();
        currentSpeed = walkSpeed;
        ownController = GetComponent<CharacterController>();
        controller = ownController;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public bool IsAffectedBy(Effect.Tag effect)
    {
        return Player.AsEffectTarget().IsAffectedBy(effect);
    }
    // Update is called once per frame
    void Update()
    {
        if (GameController.Instance.IsPaused) return;
        if (Player.IsDead) return;
        float effectiveSpeed = Player.AsEffectTarget().GetSpeed(currentSpeed);

        if (controller != null && !Player.Spellcasting.Casting && !Player.Spellcasting.Channeling)
        {
            Vector2 movement;
            movement = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            Vector3 direction = new Vector3(movement.x, 0, movement.y).normalized;
            if (direction.magnitude >= 0.1f && effectiveSpeed > 0.0f)
            {
                float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
                float angle = Mathf.SmoothDampAngle(controller.transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime * currentSpeed / effectiveSpeed);
                controller.transform.rotation = Quaternion.Euler(0f, angle, 0f);

                Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
                controller.Move(effectiveSpeed * Time.deltaTime * moveDirection.normalized);
            }
        }
    }
}
