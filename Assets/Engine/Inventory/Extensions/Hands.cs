using System;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using SS3D.Engine.Interactions;

namespace SS3D.Engine.Inventory.Extensions
{
    [RequireComponent(typeof(Inventory))]
    public class Hands : InteractionSourceNetworkBehaviour, IToolHolder, IInteractionRangeLimit
    {
        [SerializeField] private Container handContainer = null;
        [SerializeField] private float handRange = 0f;

        public event Action<int> onHandChange;
        public int SelectedHand { get; private set; } = 0;
        public float Range = 1.5f;

        // Use these for inventory actions
        public Container Container => handContainer;
        public GameObject ContainerObject => Container.gameObject;
        public int HeldSlot => handSlots[SelectedHand];

        public Vector3 mouseTarget;

        public void Start()
        {
            SupportsMultipleInteractions = true;
        }

        [Server]
        public void Pickup(GameObject target)
        {
            if (GetItemInHand() == null)
            {
                inventory.AddItem(target, ContainerObject, HeldSlot);
            }
            else
            {
                Debug.LogWarning("Trying to pick up with a non-empty hand");
            }
        }

        /*
         * Command wrappers for inventory actions using the currently held item
         */
        [Server]
        public void DropHeldItem()
        {
            if (GetItemInHand() == null) return;

            var heldItem = GetItemInHand();
            var storePosition = heldItem.transform.position;
            var storeRotation = heldItem.transform.rotation;
            inventory.PlaceItem(ContainerObject, HeldSlot, storePosition);
            heldItem.transform.position = storePosition;
            heldItem.transform.rotation = storeRotation;
        }
        [Server]
        public void PlaceHeldItem(Vector3 position) => inventory.PlaceItem(ContainerObject, HeldSlot, position);
        [Server]
        public void DestroyHeldItem() => inventory.DestroyItem(ContainerObject, HeldSlot);

        public Item GetItemInHand()
        {
            return handContainer.GetItem(HeldSlot);
        }

        /**
         * Attaches a container to the player's inventory.
         * Uses the ContainerAttachment component (on the server)
         * to ensure that the container is removed from the players inventory
         * when they get out of range.
         */
        [Command]
        private void CmdConnectContainer(GameObject containerObject)
        {
            Container container = containerObject.GetComponent<Container>();

            // If there's already an attachment, don't make another one
            var prevAttaches = GetComponents<ContainerAttachment>();
            if(prevAttaches.Any(attachment => attachment.container == container))
                return;

            var attach = gameObject.AddComponent<ContainerAttachment>();
            attach.inventory = GetComponent<Inventory>();
            attach.container = container;
            attach.range = handRange;
        }

        private void Awake()
        {
            inventory = GetComponent<Inventory>();

            // Find the indices in the hand container corresponding to the correct slots
            // Because we just make calls to GetSlot, which is set pre-Awake, this is safe.
            handSlots = new int[2] { -1, -1 };
            for (int i = 0; i < handContainer.Length(); ++i) {
                if (handContainer.GetSlot(i) == Container.SlotType.LeftHand)
                    handSlots[0] = i;
                else if (handContainer.GetSlot(i) == Container.SlotType.RightHand)
                    handSlots[1] = i;
            }
            if (handSlots[0] == -1 || handSlots[1] == -1)
                Debug.LogWarning("Player container does not contain slots for hands upon initialization. Maybe they were severed though?");

        }

        public override void OnStartClient()
        {
            handContainer.onChange += (a, b, c, d) =>
            {
                //UpdateTool()
            };
            if (handContainer.GetItems().Count > 0)
            {
                inventory.holdingSlot = new Inventory.SlotReference(handContainer, handSlots[SelectedHand]);
                //UpdateTool();
            }
        }

        public override void Update()
        {
            base.Update();
            
            if (!isLocalPlayer)
                return;

            // Hand-related buttons
            if (Input.GetButtonDown("Swap Active"))
            {
                SelectedHand = 1 - SelectedHand;
                inventory.holdingSlot = new Inventory.SlotReference(handContainer, handSlots[SelectedHand]);
                onHandChange?.Invoke(SelectedHand);
                CmdSetActiveHand(SelectedHand);
                //UpdateTool();
            }

            if (Input.GetButtonDown("Drop Item")) CmdDropHeldItem();
            if (Input.GetButtonDown("Throw")) StartThrow();

        }

        [Command]
        private void CmdDropHeldItem()
        {
            DropHeldItem();
        }

        [Command]
        private void CmdSetActiveHand(int selectedHand)
        {
            if (selectedHand >= 0 && selectedHand < handSlots.Length)
            {
                SelectedHand = selectedHand;
            }
            else
            {
                Debug.Log($"Invalid hand index {selectedHand}");   
            }
        }

        private void SetTarget()
        {
            Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var hit);
            mouseTarget = hit.point;
            CmdSetTarget(mouseTarget);
        }
        [Command]
        private void CmdSetTarget(Vector3 _mouseTarget)
        {
            mouseTarget = _mouseTarget;
        }

        private void StartThrow()
        {
            if (GetItemInHand() == null) return;
            GameObject heldItem = GetItemInHand().gameObject;

            SetTarget();

            Vector3 lookRotation = mouseTarget - heldItem.transform.position;
            lookRotation.y = 0;
            transform.root.rotation = Quaternion.LookRotation(lookRotation, Vector3.up);

            Animator playerAnimator = GetComponentInParent<Animator>();
            playerAnimator.SetLayerWeight(1, 1);
            playerAnimator.SetInteger("ActiveHand",SelectedHand);

            if(Input.GetButton("Harm")) StartThrowOver(heldItem, playerAnimator);
            else StartThrowUnder(heldItem, playerAnimator);
        }

        private void StartThrowUnder(GameObject heldItem, Animator playerAnimator)
        {
            float throwWeight = (mouseTarget - heldItem.transform.position).magnitude * 0.3f;
            throwWeight = Mathf.Clamp(throwWeight, 0.15f, 1f);
            playerAnimator.SetLayerWeight(1, throwWeight);
            playerAnimator.SetTrigger("ThrowUnder");
        }

        private void StartThrowOver(GameObject heldItem, Animator playerAnimator)
        {
            float throwWeight = (mouseTarget - heldItem.transform.position).magnitude * 0.3f;
            throwWeight = Mathf.Clamp(throwWeight, 0.3f, 0.7f);
            playerAnimator.SetLayerWeight(1, throwWeight);
            playerAnimator.SetTrigger("ThrowOver");
        }

        [Server]
        private void DoThrowUnder()
        {
            if (GetItemInHand() == null) return;
            GameObject heldItem = GetItemInHand().gameObject;
            DropHeldItem();

            Vector3 targetPosition = mouseTarget - heldItem.transform.position;
            targetPosition = Vector3.ClampMagnitude(targetPosition, 4);

            Rigidbody rigid = heldItem.GetComponent<Rigidbody>();
            rigid.AddForce(CannonAngle(heldItem.transform.position, targetPosition) * rigid.mass, ForceMode.Impulse);
            rigid.AddTorque(new Vector3(3, 3, 3), ForceMode.Impulse);
        }

        [Server]
        private void DoThrowOver()
        {
            if (GetItemInHand() == null) return;
            GameObject heldItem = GetItemInHand().gameObject;
            DropHeldItem();
            Vector3 aimVector = mouseTarget - heldItem.transform.position;
            aimVector.y = 0f;
            Vector3 launchVector = aimVector.normalized * 12 + Vector3.up * 2;
            Rigidbody rigid = heldItem.GetComponent<Rigidbody>();
            rigid.AddForce(launchVector, ForceMode.Impulse);
            Quaternion torque = Quaternion.FromToRotation(Vector3.forward, aimVector.normalized);
            heldItem.transform.rotation = Quaternion.FromToRotation(Vector3.forward, aimVector.normalized);
            rigid.maxAngularVelocity = 15;
            rigid.angularVelocity = torque * Vector3.right * 15;
  //          Time.timeScale = 0.2f;
        }

        public Vector3 CannonAngle(Vector3 sourcePosition, Vector3 targetPosition)
        {
    
            Vector3 dir = targetPosition; // get Target Direction
            float height = dir.y; // get height difference
            dir.y = 0; // retain only the horizontal difference
            float dist = dir.magnitude; // get horizontal direction
            float a = 45 * Mathf.Deg2Rad; // Convert angle to radians
            dir.y = dist * Mathf.Tan(a); // set dir to the elevation angle.
            dist += height / Mathf.Tan(a); // Correction for small height differences

            // Calculate the velocity magnitude
            float velocity = Mathf.Sqrt(dist * Physics.gravity.magnitude / Mathf.Sin(2 * a));
            return velocity * dir.normalized; // Return a normalized vector.
        }
        // The indices in the container that contains the hands
        private int[] handSlots;
        private Inventory inventory;
        public IInteractionSource GetActiveTool()
        {
            Item itemInHand = GetItemInHand();
            if (itemInHand == null)
            {
                return null;
            }

            IInteractionSource interactionSource = itemInHand.prefab.GetComponent<IInteractionSource>();
            if (interactionSource != null)
            {
                interactionSource.Parent = this;
            }
            return interactionSource;
        }

        public float GetInteractionRange()
        {
            return Range;
        }
    }
}