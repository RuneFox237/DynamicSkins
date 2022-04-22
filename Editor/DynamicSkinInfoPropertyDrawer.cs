using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using DynamicSkinBuilder;

namespace DynamicSkinBuilder.CustomEditors
{
  public class CopyDBasmdef : Editor
  {
    //[MenuItem("Assets/Create/DynamicSkins/Set Api Compatability Level", false, 1)]
    //static void SetNetCompat()
    //{
    //  //UnityEditor.PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.Unknown, ApiCompatibilityLevel.NET_4_6);
    //  Debug.Log(UnityEditor.PlayerSettings.GetApiCompatibilityLevel(BuildTargetGroup.Standalone));
    //
    //}

    [MenuItem("Assets/Create/DynamicSkins/Create DynamicBones.asmdef",false,1)]
    static void TryMove()
    {
      bool TryZip = false;
      bool TryManaged = false;

      //Debug.Log(AssetDatabase.GetAssetPath(Selection.activeObject.GetInstanceID()));
      var path = AssetDatabase.GetAssetPath(Selection.activeObject.GetInstanceID());
      try //if DynamicSkins was added from zip
      {
        File.Copy("Packages/com.runefox237.dynamicskinbuilder/Editor/DynamicBones~/DynamicBones.asmdef", path + "/DynamicBones.asmdef", true);
        File.Copy("Packages/com.runefox237.dynamicskinbuilder/Editor/DynamicBones~/DynamicBones.asmdef.meta", path + "/DynamicBones.asmdef.meta", true);
      }
      catch
      {
        TryZip = true;//An exception has occured
      }

      try //if DynamicSkins was added from package manager
      {
        //File.Copy("Packages/com.runefox237.dynamicskinbuilder/Editor/DynamicBones~/DynamicBones.asmdef", AssetDatabase.GetAssetPath(Selection.activeObject.GetInstanceID()) + "/DynamicSkins.asmdef", true);
        //File.Copy("Packages/com.runefox237.dynamicskinbuilder/Editor/DynamicBones~/DynamicBones.asmdef.meta", AssetDatabase.GetAssetPath(Selection.activeObject.GetInstanceID()) + "/DynamicSkins.asmdef.meta", true);
      }
      catch
      {
        TryManaged = true;//An exception has occured
      }

      if (TryManaged && TryZip)
      {
        Debug.LogError("Error when creating DynamicBones.asmdef");
      }


      AssetDatabase.StartAssetEditing();
      AssetDatabase.ImportAsset(path + "/DynamicBones.asmdef", ImportAssetOptions.ForceUpdate);
      AssetDatabase.ImportAsset(path + "/DynamicBones.asmdef.meta", ImportAssetOptions.ForceUpdate);
      AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
      AssetDatabase.StopAssetEditing();


      //AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
      //AssetDatabase.ImportAsset(dynamicSkinInfo.assetInfo.modFolder + "/DynamicSkinHelpers.cs", ImportAssetOptions.ForceUpdate);
      //AssetDatabase.ImportAsset(dynamicSkinInfo.assetInfo.modFolder + "/RuneFox_Utils.cs", ImportAssetOptions.ForceUpdate);

    }
  }

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

      bool TryZip = false;
      bool TryManaged = false;

      try //if DynamicSkins was added from zip
      {
        File.Copy("Packages/com.runefox237.dynamicskinbuilder/Editor/CopyPath/DynamicSkinHelpers.cs", dynamicSkinInfo.assetInfo.modFolder + "/DynamicSkinHelpers.cs", true);
        File.Copy("Packages/com.runefox237.dynamicskinbuilder/Editor/CopyPath/RuneFox_Utils.cs", dynamicSkinInfo.assetInfo.modFolder + "/RuneFox_Utils.cs", true);
      }
      catch
      {
        TryZip = true;//An exception has occured
      }

      try //if DynamicSkins was added from package manager
      {
        File.Copy("Packages/com.runefox237.dynamicskinbuilder/Editor/CopyPath/DynamicSkinHelpers.cs", dynamicSkinInfo.assetInfo.modFolder + "/DynamicSkinHelpers.cs", true);
        File.Copy("Packages/com.runefox237.dynamicskinbuilder/Editor/CopyPath/RuneFox_Utils.cs", dynamicSkinInfo.assetInfo.modFolder + "/RuneFox_Utils.cs", true);
      }
      catch
      {
        TryManaged = true;//An exception has occured
      }

      if (TryManaged && TryZip)
      {
        Debug.LogError("Dynamic Skins has errored on copying files to the Mod Folder: " + dynamicSkinInfo.assetInfo.modFolder);
      }

      AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
      AssetDatabase.ImportAsset(dynamicSkinInfo.assetInfo.modFolder + "/DynamicSkinHelpers.cs", ImportAssetOptions.ForceUpdate);
      AssetDatabase.ImportAsset(dynamicSkinInfo.assetInfo.modFolder + "/RuneFox_Utils.cs", ImportAssetOptions.ForceUpdate);

      //var HelperCode = new 

      foreach (var mod in dynamicSkinInfo.DynamicModifications)
      {
        var found = dynamicSkinInfo.modInfo.additionalResources.Exists(x => x.GetHashCode() == mod.prefab.GetHashCode());
        if (!found)
        {
          dynamicSkinInfo.modInfo.additionalResources.Add(mod.prefab);
        }
      }

      Debug.Log("DynamicSkin Finished");
    }
  }
}
