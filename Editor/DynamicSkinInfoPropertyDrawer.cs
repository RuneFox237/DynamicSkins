using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using DynamicSkinBuilder;

namespace DynamicSkinBuilder.CustomEditors
{


  [CustomEditor(typeof(DynamicSkinInfo))]
  public class DynamicSkinInfoEditor : Editor
  {
    public override void OnInspectorGUI()
    {
      base.OnInspectorGUI();

      if (GUILayout.Button("Build"))
      {
        Build(serializedObject.targetObject as DynamicSkinInfo);
      }
    }

    private void Build(DynamicSkinInfo dynamicSkinInfo)
    {

      if (dynamicSkinInfo.assetInfo == null) dynamicSkinInfo.InitializeAssetInfo();

      var path = Path.Combine(dynamicSkinInfo.assetInfo.modFolder, dynamicSkinInfo.modInfo.name + "DynamicSkin.cs");
      //dynamicSkinInfo.assetInfo.CreateNecessaryAssetsAndFillPaths(dynamicSkinInfo.modInfo.regenerateAssemblyDefinition);
      var DynamicSkinCode = new DynamicSkinTemplate(dynamicSkinInfo);
      File.WriteAllText(path, DynamicSkinCode.TransformText());

      //turns out that this still works the same even if the package is in the package cache
      File.Copy("Packages/com.runefox237.dynamicskinbuilder/Editor/CopyPath/DynamicSkinHelpers.cs", dynamicSkinInfo.assetInfo.modFolder + "/DynamicSkinHelpers.cs", true);
      File.Copy("Packages/com.runefox237.dynamicskinbuilder/Editor/CopyPath/RuneFox_Utils.cs", dynamicSkinInfo.assetInfo.modFolder + "/RuneFox_Utils.cs", true);

      AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
      AssetDatabase.ImportAsset(dynamicSkinInfo.assetInfo.modFolder + "/DynamicSkinHelpers.cs", ImportAssetOptions.ForceUpdate);
      AssetDatabase.ImportAsset(dynamicSkinInfo.assetInfo.modFolder + "/RuneFox_Utils.cs", ImportAssetOptions.ForceUpdate);


      foreach (var skin in dynamicSkinInfo.dynamicSkins)
      {
        foreach(var mod in skin.modifications)
        {
          var found = dynamicSkinInfo.modInfo.additionalResources.Exists(x => x.GetHashCode() == mod.prefab.GetHashCode());
          if (!found)
          {
            dynamicSkinInfo.modInfo.additionalResources.Add(mod.prefab);
          }
        }
      }

      Debug.Log("DynamicSkin Finished");
    }
  }
}
