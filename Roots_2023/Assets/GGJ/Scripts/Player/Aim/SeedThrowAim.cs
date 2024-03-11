using GGJ.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GGJ.Player.Aim
{
    public class SeedThrowAim : MonoBehaviour
    {

        [SerializeField]
        private Transform aimRoot;

        [SerializeField]
        private float maxAimDistance;

        [SerializeField]
        private LineRenderer line;

        [SerializeField]
        private float lineStartDistance;

        [SerializeField]
        private float lineEndDistance;

        [SerializeField]
        private SpriteRenderer reticle;

        [SerializeField]
        private InputActionReference aimAction;

        [SerializeField]
        private InputActionReference shootAction;

        public Vector2 AimDirectionAndForce { get; private set; }

        private bool isAimingWithMouse = false;
        private bool isAimingWithAction = false;

        private Inventory.Inventory inventory = default;

        private void Awake()
        {
            inventory = GameManager.Instance.Inventory;

            ToggleGraphics(false);

            shootAction.action.performed += Action_performed;
            shootAction.action.canceled += Action_canceled;
        }

        private void Action_performed(InputAction.CallbackContext obj)
        {
            isAimingWithMouse = true;
        }

        private void Action_canceled(InputAction.CallbackContext obj)
        {
            isAimingWithMouse = false;
        }

        private void Update()
        {
            if (inventory.CurrentItem == null) return;

            Vector2 aimActionValue = aimAction.action.ReadValue<Vector2>();

            isAimingWithAction = aimActionValue.magnitude > .1f;
            if (isAimingWithAction)
            {
                AimWithAction(aimActionValue);
            }
            else
            {
                AimWithMouse();
            }

            bool isInventoryVisible = GGJ.Core.GameManager.Instance.Inventory.UI.IsVisible;
            bool isAiming = (isAimingWithAction || isAimingWithMouse) && !isInventoryVisible;

            ToggleGraphics(isAiming);

            if (inventory.CurrentItem != null && isAiming)
            {
                DrawLine();
            }
        }

        private void ToggleGraphics(bool isAiming)
        {
            reticle.enabled = isAiming;
            line.enabled = isAiming;
        }

        private void AimWithMouse()
        {
            Vector2 worldMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 aimDirection = worldMousePosition - (Vector2)aimRoot.position;
            if (aimDirection.magnitude > maxAimDistance)
            {
                aimDirection = aimDirection.normalized * maxAimDistance;
            }

            Aim(aimDirection);
        }

        private void AimWithAction(Vector2 aimActionValue)
        {
            Aim(aimActionValue * maxAimDistance);
        }

        private void Aim(Vector2 aimDirection)
        {
            reticle.transform.localPosition = aimDirection;

            float force = Mathf.InverseLerp(0, maxAimDistance, aimDirection.magnitude);

            AimDirectionAndForce = aimDirection.normalized * force;
        }

        private void DrawLine()
        {
            Vector2 startPoint = (Vector2)transform.position + AimDirectionAndForce.normalized * lineStartDistance;
            Vector2 endPoint = (Vector2)reticle.transform.position - AimDirectionAndForce.normalized * lineEndDistance;

            line.SetPositions(new Vector3[] { startPoint, endPoint });
        }

    }

}