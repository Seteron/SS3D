using System;
using System.Collections.Generic;
using UnityEngine;

namespace SS3D.Engine.Inventory
{
    public class ClothingContainers : MonoBehaviour
    {
        public AttachedContainer Gloves => Containers["Gloves"];
        public AttachedContainer Ears => Containers["Ears"];
        public AttachedContainer Jumpsuit => Containers["Jumpsuit"];
        public AttachedContainer Exosuit => Containers["Exosuit"];
        public AttachedContainer Glasses => Containers["Glasses"];
        public AttachedContainer Mask => Containers["Mask"];
        public AttachedContainer Head => Containers["Head"];
        public AttachedContainer Shoes => Containers["Shoes"];
        public AttachedContainer Accessory => Containers["Accessory"];
        // TODO: Replace with actual clothing storage
        public AttachedContainer SuitStorage => Containers["Suit Storage"];

        [NonSerialized]
        public static readonly List<string> ClothingSlotNames = new List<string> 
        {"Ears", "Jumpsuit", "Exosuit", "Glasses", "Mask", "Gloves", "Head", "Shoes", "Accessory", "Belt", "Backpack"};

        [NonSerialized]
        public Dictionary<string, AttachedContainer> Containers = new Dictionary<string, AttachedContainer>();

        public void Start()
        {
            ContainerDescriptor[] descriptors = gameObject.GetComponents<ContainerDescriptor>();
            ClothingDisplay clothingDisplay = gameObject.GetComponent<ClothingDisplay>();

            foreach (ContainerDescriptor descriptor in descriptors)
            {
                if (ClothingSlotNames.Contains(descriptor.containerName))
                {
                    Containers.Add(descriptor.containerName, descriptor.attachedContainer);
                    descriptor.attachedContainer.ItemAttached += clothingDisplay.OnClothingAttached;
                    descriptor.attachedContainer.ItemDetached += clothingDisplay.OnClothingDetached;
                }
            }
        }
    }
}