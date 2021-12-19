
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SS3D.Engine.Inventory;

[ExecuteAlways]
public class ClothingDisplay : MonoBehaviour
{
    private Dictionary<Clothing, SkinnedMeshRenderer> clothes = new Dictionary<Clothing, SkinnedMeshRenderer>();

    private SkinnedMeshRenderer bodyMesh;

    private bool dirty;


    void Start()
    {
        bodyMesh = GetComponentInChildren<SkinnedMeshRenderer>();
        MarkDirty();
    }

    void OnValidate()
    {
        MarkDirty();
    }

    public void MarkDirty()
    {
        dirty = true;
    }

    void Update()
    {
        if (dirty)
        {
            dirty = false;
            RefreshAppearance();
        }
    }


    void RefreshAppearance()
    {
        Cleanup();

        foreach (KeyValuePair<Clothing, SkinnedMeshRenderer> cloth in clothes)
        {
            CreateInstance(cloth.Key, Color.white);
        }
    }

    void CreateInstance(Clothing clothingItem, Color color)
    {
        if (clothingItem.skinnedMesh != null)
        {
            SkinnedMeshRenderer clothingInstance = Instantiate<SkinnedMeshRenderer>(clothingItem.skinnedMesh);

            clothingInstance.transform.parent = this.transform;
            clothingInstance.bones = bodyMesh.bones;
            clothingInstance.rootBone = bodyMesh.rootBone;

            clothes[clothingItem] = clothingInstance;
        }
    }


    public void OnClothingAttached(object sender, Item item)
    {
        Clothing clothingItem = item.GetComponent<Clothing>();
        if (clothingItem != null)
        {
            CreateInstance(clothingItem, Color.white);
        }
    }

    public void OnClothingDetached(object sender, Item item)
    {
        Clothing clothingItem = item.GetComponent<Clothing>();
        if (clothingItem != null)
        {
            EditorAndRuntime.Destroy(clothes[clothingItem].gameObject);
            clothes.Remove(clothingItem);
        }
    }


    void OnDisable()
    {
        Cleanup();
    }

    void Cleanup()
    {
        foreach (KeyValuePair<Clothing, SkinnedMeshRenderer> cloth in clothes)
        {
            EditorAndRuntime.Destroy(cloth.Value.gameObject);
        }
        clothes.Clear();
    }
}