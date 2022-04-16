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

      var path = Path.Combine(dynamicSkinInfo.assetInfo.modFolder, dynamicSkinInfo.skinDef.name + "DynamicSkin.cs");
      //dynamicSkinInfo.assetInfo.CreateNecessaryAssetsAndFillPaths(dynamicSkinInfo.modInfo.regenerateAssemblyDefinition);
      var DynamicSkinCode = new DynamicSkinTemplate(dynamicSkinInfo);
      File.WriteAllText(path, DynamicSkinCode.TransformText());

      File.Copy("Packages/com.runefox237.dynamicskinbuilder/Editor/CopyPath/DynamicSkinHelpers.cs", dynamicSkinInfo.assetInfo.modFolder + "/DynamicSkinHelpers.cs", true);
      File.Copy("Packages/com.runefox237.dynamicskinbuilder/Editor/CopyPath/RuneFox_Utils.cs", dynamicSkinInfo.assetInfo.modFolder + "/RuneFox_Utils.cs", true);

      AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
      AssetDatabase.ImportAsset(dynamicSkinInfo.assetInfo.modFolder + "/DynamicSkinHelpers.cs", ImportAssetOptions.ForceUpdate);
      AssetDatabase.ImportAsset(dynamicSkinInfo.assetInfo.modFolder + "/RuneFox_Utils.cs", ImportAssetOptions.ForceUpdate);

      //var HelperCode = new 

      foreach (var mod in dynamicSkinInfo.DynamicModifications)
      {
        var found = dynamicSkinInfo.modInfo.additionalResources.Exists(x => x.name == mod.Prefab.name);
        if (!found)
        {
          dynamicSkinInfo.modInfo.additionalResources.Add(mod.Prefab);
        }
      }

      Debug.Log("DynamicSkin Finished");
    }
  }
}
