using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RoRSkinBuilder.Data;
using RuneFoxMods.DynamicSkins;

namespace RuneFoxMods.DynamicSkinBuilder
{
  [AddComponentMenu("RoR Skins/Dynamic Skin Info")]
  public class DynamicSkinInfo : MonoBehaviour
  {
    public SkinModInfo modInfo;
  
    public AssetsInfo assetInfo;
  
    [SerializeField]
    public List<DynamicSkin> dynamicSkins = new List<DynamicSkin>();

    [Serializable]
    public class DynamicSkin
    {
      public SkinDefinition skinDef;
      public List<DynamicModification> modifications;
    }

    [Serializable]
    public class DynamicModification
    {
      public GameObject prefab;
      public DynamicBone dynamicBone; //DynamicBone needs to be on the Root of the prefab in the scene for the template to work correctly
      public GameObject parentBone;
      public bool affectsBaseModel = true;
    }

    public DynamicSkinInfo(SkinModInfo modinfo_)
    {
      modInfo = modinfo_;
      InitializeAssetInfo();
    }
  
    public DynamicSkinInfo(DynamicSkinInfo dynamicSkinInfo_)
    {
      modInfo = dynamicSkinInfo_.modInfo;
      dynamicSkins = dynamicSkinInfo_.dynamicSkins;
      InitializeAssetInfo();
    }
  
    public void InitializeAssetInfo()
    {
      assetInfo = new AssetsInfo(modInfo);
    }

    internal SkinnedMeshRenderer GetMainRenderer(DynamicSkin skin)
    {
      //All parentBones should lead to the same TopParent so we grab first one.
      var renderers = DynamicSkinHelpers.GetTopParent(skin.modifications[0].parentBone.transform).GetComponentsInChildren<SkinnedMeshRenderer>();

      List<SkinnedMeshRenderer> MainRends = new List<SkinnedMeshRenderer>();
      foreach (var rend in renderers)
      {
        bool rendHasAll = true;
        foreach (var mod in skin.modifications)
        {
          if (rend.bones.Contains(mod.parentBone.transform) == false)
          {
            rendHasAll = false;
            break;
          }
        }

        if (rendHasAll)
          MainRends.Add(rend);
      }

      if (MainRends.Count == 0)
      {
        Debug.LogError("Could Not find a renderer that contained the parent bones of all modifications");
        return null;
      }
      if (MainRends.Count > 1)
      {
        //Debug.Log("Found more than one renderer that matched the criteria for Main Renderer of " + skin.skinDef.name + ", using the first one found");
      }
      return MainRends[0];
    }

  }
}
