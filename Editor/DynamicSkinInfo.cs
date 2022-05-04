using System;
using System.Collections.Generic;
using UnityEngine;
using RoRSkinBuilder.Data;

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
  
  }
}
