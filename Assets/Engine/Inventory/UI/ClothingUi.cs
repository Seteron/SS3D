using Mirror;
using SS3D.Content;
using UnityEngine;

namespace SS3D.Engine.Inventory.UI
{
    public class ClothingUi : MonoBehaviour
    {
        public void Start()
        {
            if (NetworkServer.active && !NetworkClient.active)
            {
                Destroy(this);
                return;
            }
            
            // Connects ui clothing slots to containers on the creature
            var inventory = transform.GetComponentInParent<InventoryUi>().Inventory;
            GameObject creature = inventory.Hands.GetComponentInParent<Entity>().gameObject;
            var clothingContainers = creature.GetComponent<ClothingContainers>();
            var slots = GetComponentsInChildren<SingleItemContainerSlot>();
            foreach (SingleItemContainerSlot slot in slots)
            {
                if (clothingContainers.Containers.TryGetValue(slot.name, out AttachedContainer container))
                {
                    slot.Inventory = inventory;
                    slot.Container = container;

                    if (slot.name == "Jumpsuit" || slot.name == "Exosuit")
                    {
                        container.ItemAttached += OnSubstorageAttached;
                        container.ItemDetached += OnSubstorageDetached;
                    }
                }
            }
        }


        public void OnSubstorageAttached(object sender, Item item)
        {
            var slots = GetComponentsInChildren<SingleItemContainerSlot>();
            var inventory = transform.GetComponentInParent<InventoryUi>().Inventory;
            ContainerDescriptor[] containers = item.gameObject.GetComponents<ContainerDescriptor>();

            if(containers.Length == 0)
            {
                return;
            }

            foreach (SingleItemContainerSlot slot in slots)
            {
                foreach(ContainerDescriptor container in containers)
                {
                    if(slot.name == container.containerName)
                    {
                        slot.Inventory = inventory;
                        slot.Container = container.attachedContainer;
                    }
                }
            }
        }

        public void OnSubstorageDetached(object sender, Item item)
        {
            var slots = GetComponentsInChildren<SingleItemContainerSlot>();
            ContainerDescriptor[] containers = item.gameObject.GetComponents<ContainerDescriptor>();

            if(containers.Length == 0)
            {
                return;
            }

            foreach (SingleItemContainerSlot slot in slots)
            {
                foreach (ContainerDescriptor container in containers)
                {
                    if (slot.name == container.containerName)
                    {
                        slot.Inventory = null;
                        slot.Container = null;
                    }
                }
            }            
        }
    }
}