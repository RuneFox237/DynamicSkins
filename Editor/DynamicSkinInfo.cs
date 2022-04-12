using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RoRSkinBuilder.Data;

[AddComponentMenu("RoR Skins/Dynamic Skin Info")]
public class DynamicSkinInfo : MonoBehaviour
{
  public SkinDefinition skinDef;
  public SkinModInfo modInfo;

  public AssetsInfo assetInfo;

  [SerializeField]
  public List<DynamicModification> DynamicModifications = new List<DynamicModification>();

  public DynamicSkinInfo(SkinModInfo modinfo_, SkinDefinition skindef_)
  {
    skinDef = skindef_;
    modInfo = modinfo_;
    assetInfo = new AssetsInfo(modinfo_);
  }
  public DynamicSkinInfo(DynamicSkinInfo dynamicSkinInfo_)
  {
    skinDef = dynamicSkinInfo_.skinDef;
    modInfo = dynamicSkinInfo_.modInfo;
    assetInfo = new AssetsInfo(modInfo);
  }

  public void InitializeAssetInfo()
  {
    assetInfo = new AssetsInfo(modInfo);
  }

  [Serializable]
  public class DynamicModification
  
  {
    public GameObject Prefab;
    public DynamicBone DynamicBone;
    public GameObject ParentBone;
    public bool AffectsBaseModel = true;
  }
}
