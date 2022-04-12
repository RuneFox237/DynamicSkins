using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
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
      DynamicSkinTemplate
      var path = Path.Combine(dynamicSkinInfo.assetInfo.modFolder, dynamicSkinInfo.assetInfo.uccModName + "Plugin.cs");
      
      var pluginCode = new DynamicSkinTemplate(dynamicSkinInfo);
      File.WriteAllText(path, pluginCode.TransformText());

    }
  }
}
