using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RoRSkinBuilder.Data;

[AddComponentMenu("RoR Skins/Dynamic Skin Info")]
public class DynamicSkinInfo : MonoBehaviour
{
  internal List<SkinDefinition> skinDefs = new List<SkinDefinition>();
  public SkinModInfo modInfo;

  public AssetsInfo assetInfo;

  [SerializeField]
  public List<DynamicModification> DynamicModifications = new List<DynamicModification>();

  public DynamicSkinInfo(SkinModInfo modinfo_, List<SkinDefinition> skindef_)
  {
    skinDefs = skindef_;
    modInfo = modinfo_;
    assetInfo = new AssetsInfo(modinfo_);
  }
  public DynamicSkinInfo(DynamicSkinInfo dynamicSkinInfo_)
  {
    skinDefs = new List<SkinDefinition>(dynamicSkinInfo_.skinDefs);
    modInfo = dynamicSkinInfo_.modInfo;
    assetInfo = new AssetsInfo(modInfo);
  }

  public void InitializeAssetInfo()
  {
    assetInfo = new AssetsInfo(modInfo);

    skinDefs.Clear();
    foreach(var modification in DynamicModifications)
    {

      if (!skinDefs.Exists(x => x.GetHashCode() == modification.skinDef.GetHashCode()))
      {
        skinDefs.Add(modification.skinDef);
      }

    }

  } 

  [Serializable]
  public class DynamicModification
  {
    public GameObject prefab;
    public DynamicBone dynamicBone;
    public GameObject parentBone;
    public SkinDefinition skinDef;
    public bool affectsBaseModel = true;
  }
}