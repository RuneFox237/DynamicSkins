using UnityEngine;
using System.Collections.Generic;
using RoRSkinBuilder.Data;
using RoRSkinBuilder;

namespace DynamicSkinBuilder
{

partial class DynamicSkinTemplate
{
  private DynamicSkin dynamicSkin;
  public DynamicSkinTemplate(DynamicSkin skin) { dynamicSkin = skin;}

}

public class DynamicSkin
{
  public SkinDefinition skinDef;
  public SkinModInfo modInfo;
  public AssetsInfo assetInfo;

  public List<DynamicModification> Mods = new List<DynamicModification>();

    DynamicSkin(SkinModInfo modinfo_, SkinDefinition skindef_)
    {
      skinDef = skindef_;
      modInfo = modinfo_;
      assetInfo = new AssetsInfo(modinfo_);
    }
}

public class DynamicModification
{
    public GameObject Prefab;
    public DynamicBone DynamicBone;
    public GameObject ParentBone;
    public bool AffectsBaseModel = true;
}

}

