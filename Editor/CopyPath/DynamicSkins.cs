using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using RoR2;
using BepInEx.Logging;

namespace RuneFoxMods.DynamicSkins
{
  public static class DynamicSkinHelpers
  {
    ////////////////////////////////////////////////////////////////////////////
    ////// Utility Funcs

    public static SkinnedMeshRenderer[] GetBaseSkinRenderers(GameObject modelObject)
    {
      //assumption of meshes on base characters is that they are put as direct children to ModelObject
      var renderers = modelObject.GetComponentsInChildren<SkinnedMeshRenderer>(true);

      List<SkinnedMeshRenderer> baseskinrenderers = new List<SkinnedMeshRenderer>();
      foreach (var renderer in renderers)
      {
        //if renderer is attatched to a child of the modelobject then it is a base skin renderer
        if (renderer.transform.parent == modelObject.transform)
        {
          baseskinrenderers.Add(renderer);
        }
      }

      return baseskinrenderers.ToArray();
    }

    //NOTE: I have no idea how this works with long and wide armatures as the 2B ones are only 3 bones total
    //this will likely break
    public static Transform[] BoneArrayBuilder(Transform NewBoneRoot)
    {
      List<Transform> NewBoneList = new List<Transform>();

      //read the new bone heiarchy to array depth first
      BoneArrayBuilderHelper(NewBoneRoot, NewBoneList);

      return NewBoneList.ToArray();
    }

    public static void BoneArrayBuilderHelper(Transform parent, List<Transform> list)
    {
      if (!parent.name.EndsWith("_end"))
        list.Add(parent);

      for (int i = 0; i < parent.childCount; i++)
      {
        BoneArrayBuilderHelper(parent.GetChild(i), list);
      }
    }

    //Searches the gameobject for the first object with Armature in its name and returns it
    public static Transform GetArmature(GameObject obj)
    {
      return GetArmatureHelper(obj);
    }

    public static Transform GetArmatureHelper(GameObject obj)
    {
      if (obj.name.ToLower().Contains("armature"))
      {
        return obj.transform;
      }

      for (int i = 0; i < obj.transform.childCount; i++)
      {
        var armature = GetArmatureHelper(obj.transform.GetChild(i).gameObject);
        if (armature) return armature;
      }
      return null;
    }

    public static Transform GetTopParent(Transform obj)
    {
      Transform walker = obj;
      while (walker.parent != null)
      {
        walker = walker.parent;
      }

      return walker;
    }

    public static string GetPrevBoneInList(Transform targetBone, SkinnedMeshRenderer meshRenderer)
    {
      var bonearray = meshRenderer.bones;
      for (int i = 0; i <= bonearray.Length - 1; i++)
      {
        if (bonearray[i + 1].name == targetBone.name)
          return bonearray[i].name;
      }
      return null;
    }

    public static int GetBoneIndexInList(Transform targetBone, SkinnedMeshRenderer meshRenderer)
    {
      var bonearray = meshRenderer.bones;
      //Debug.Log(meshRenderer.name);
      for (int i = 0; i <= bonearray.Length; i++)
      {
        //Debug.Log(targetBone.name + "   " + bonearray[i + 1].name);
        if (bonearray[i].name == targetBone.name)
          return i;
      }
      return -1;
    }

    public static int GetPrevBoneIndexInList(Transform targetBone, SkinnedMeshRenderer meshRenderer)
    {
      var bonearray = meshRenderer.bones;
      for (int i = 0; i <= bonearray.Length - 1; i++)
      {
        if (bonearray[i + 1].name == targetBone.name)
          return i;
      }
      return -1;
    }

    ////// Utility Funcs
    ////////////////////////////////////////////////////////////////////////////
  }

  class DynamicSkinManager
  {
    ///////////////////////////////////////////////////////////
    /// Local Declarations
    internal Dictionary<string, SkinDef> SkinDefs = new System.Collections.Generic.Dictionary<string, SkinDef>();
    private GameObject LastModelObject;

    //private static readonly Dictionary<GameObject, Modification> appliedModificatons = new Dictionary<GameObject, Modification>();
    internal Dictionary<string, SortedList<int, Modification>> ModificationList = new Dictionary<string, SortedList<int, Modification>>();//storage for modifications
    private Dictionary<GameObject, AppliedModifications> ModifiedObjects = new Dictionary<GameObject, AppliedModifications>();

    //This uses Name of class 
    internal ManualLogSource InstanceLogger;// => Instance?.Logger;
    /// Local Declarations
    ///////////////////////////////////////////////////////////

    ////////////////////////////////////////////////////////////////////////////
    ////// Dynamic Skin Functions
    internal void AddModification(string skinNameToken, int boneIndex, Modification modification)
    {
      SortedList<int, Modification> modList;
      if (ModificationList.TryGetValue(skinNameToken, out modList) == false)
      {
        modList = new SortedList<int, Modification>();
        ModificationList.Add(skinNameToken, modList);
      }

      modList.Add(boneIndex, modification);
    }

    internal void AddSkinDef(SkinDef skinDef)
    {
      SkinDefs.Add(skinDef.nameToken, skinDef);
    }
    
    internal void SkinDefApply(Action<SkinDef, GameObject> orig, SkinDef self, GameObject modelObject)
    {
      orig(self, modelObject);

      RemoveInvalidModelObjects();

      ModifiedObjects.TryGetValue(modelObject, out var modificatons);

      try
      {

        //if we are on another character/skin
        SkinDef test;
        var found = SkinDefs.TryGetValue(self.nameToken, out test);
        if (!found)
        {
          if (modificatons != null)
          {
            ClearSkinModifications(LastModelObject, modificatons);
          }
          return;
        }

        if (modificatons == null)
        {
          //otherwise if are now applying modded skin and no modifcations have been made, then apply modifications

          //create new Applied Entry and pass into Apply
          AppliedModifications NewMods = new AppliedModifications();
          ModifiedObjects.Add(modelObject, NewMods);
          ApplySkinModifications(self, modelObject, NewMods);
        }
      }
      catch (Exception e)
      {
        //error logging may need to be skin specific
        InstanceLogger.LogWarning("An error occured while adding accessories to a skin");
        InstanceLogger.LogError(e);
      }

      //print heiarchy
      //Utils.readheiarchy(modelObject);
    }

    private void RemoveInvalidModelObjects()
    {
      foreach (var modelObject in ModifiedObjects.Keys.Where(el => !el).ToList())
      {
        ModifiedObjects.Remove(modelObject);
      }
    }

    private void ClearSkinModifications(GameObject modelObject, AppliedModifications modifications)
    {
      //NOTE: modifications that modify the base bone list need to have their bones removed first before destruction
      //Modifications that modify the base bone list have to be removed in reverse order in order to maintain correct indexing

      //Clear Mods that modify base bone list, these need to be done in reverse order
      while (modifications.BaseModelModifications.Count != 0)
      {
        var mod = modifications.BaseModelModifications.Pop();

        clearModification(mod, modelObject, modifications);
      }

      //clear rest of mods
      while (modifications.OtherModifications.Count != 0)
      {
        clearModification(modifications.OtherModifications[0], modelObject, modifications); //clearmodification calls remove on applied Mod.
      }

      //remove the object from modified once all mods are cleared
      ModifiedObjects.Remove(modelObject);
    }

    private void ApplySkinModifications(SkinDef skindef, GameObject modelObject, AppliedModifications modifications)
    {
      var characterModel = modelObject.GetComponent<CharacterModel>();

      LastModelObject = modelObject;
      SortedList<int, Modification> modlist;
      if (ModificationList.TryGetValue(skindef.nameToken, out modlist))
      {
        foreach (var mod in modlist)
        {
          ApplyModification(modelObject, characterModel, mod.Value, modifications);
        }
      }
    }

    private static void ApplyModification(GameObject modelObject, CharacterModel characterModel, Modification modification, AppliedModifications modifications)
    {
      //Get aramture bone that new prefab will be parented to
      var bodyname = modification.bodyname;
      var parentname = modification.parentname;

      var parentBone = Utils.FindChildInTree(modelObject.transform, parentname);
      //var parentBone = modelObject.transform.Find(ChildHelper.GetPath(bodyname, parentname));

      GameObject newPart;

      //we have to instantiate the prefabs in two different ways based on if they affect the model or not
      if (modification.affectsbasemodel)
      {
        //instantiate the model
        newPart = GameObject.Instantiate(modification.prefab, parentBone, false);
        newPart.name = Utils.RemoveCloneNaming(newPart.name);
        modification.instance = newPart;
        modification.inst_armature = newPart; //the armature of the modifications that affect the base mode is the whole prefab

        ModificationApplyBones(modelObject, modification, modifications);
      }
      else
      {
        //instantiate it w/ model as parent
        newPart = GameObject.Instantiate(modification.prefab, modelObject.transform, false);
        newPart.name = Utils.RemoveCloneNaming(newPart.name);
        modification.instance = newPart;

        var armature = DynamicSkinHelpers.GetArmature(newPart);

        //then parent the armature to the parentboned
        armature.transform.SetParent(parentBone, false);
        modification.inst_armature = armature.gameObject;
      }
      modification.instance = newPart;

      //TODO: add a way to load multiple DynamicBone Scripts for a single modification

      ///////////////////////////////////////////////////////////
      /// Add dynamic bones stuff here
      /// Things like DynamicBones Component, assigning values to dynamic bones component, adding DB_Colliders and editing them, etc.
      if (modification.dynamicBoneData != null)
      {
        ModificationApplyDynamicBones(modelObject, modification);
      }
      /// Add dynamic bones stuff here
      ///////////////////////////////////////////////////////////

      ///////////////////////////////////////////////////////////
      /// Add renderers to the character's renderer list
      ModificationAddRenderers(newPart, characterModel);
      /// Add renderers to the character's renderer list
      ///////////////////////////////////////////////////////////

      //Push to applied modifications when done
      modifications.OtherModifications.Add(modification);
    }

    private static void ModificationApplyBones(GameObject modelObject, Modification modification, AppliedModifications modifications)
    {
      var skinRenderers = DynamicSkinHelpers.GetBaseSkinRenderers(modelObject);

      var newBones = skinRenderers[0].bones.ToList();//assumption is that we find a skinrenderer here, if not then whoops

      //var newBoneIndex = DynamicSkinHelpers.FindBoneIndex2(parentBone, newBones);
      //insert new bones into array at end of array
      var modBoneArray = DynamicSkinHelpers.BoneArrayBuilder(modification.instance.transform);
      newBones.InsertRange(modification.boneIndex, modBoneArray);

      //add index and bonecount to modification
      //modification.boneIndex = newBoneIndex;
      modification.boneCount = modBoneArray.Length;

      //assign bones to skin renderers
      foreach (var renderer in skinRenderers)
      {
        renderer.bones = newBones.ToArray();
      }

      modifications.BaseModelModifications.Push(modification);
    }

    private static void ModificationApplyDynamicBones(GameObject modelObject, Modification modification)
    {
      //======================================
      //Add Dynamic Bone Component
      DynamicBone DB = modification.instance.AddComponent<DynamicBone>();
      modification.inst_dynamicBone = DB;

      //======================================
      /// Add DynamicBones Colliders to other armature bones (Need to do this before Modifying Dynamic Bones component as we add them to the DB list during modification)
      List<DynamicBoneCollider> bonelist = new List<DynamicBoneCollider>();

      foreach (var colliderData in modification.dynamicBoneData.m_Colliders)
      {
        var parent = Utils.FindChildInTree(modelObject.transform, colliderData.m_parent_name);
        //var parent = modelObject.transform.Find(ChildHelper.GetPath(bodyname, colliderData.m_parent_name));

        var bonecollider = parent.gameObject.AddComponent<DynamicBoneCollider>();

        bonecollider.m_Direction = colliderData.m_Direction;
        bonecollider.m_Center = colliderData.m_Center;
        bonecollider.m_Bound = colliderData.m_Bound;
        bonecollider.m_Radius = colliderData.m_Radius;
        bonecollider.m_Height = colliderData.m_Height;

        bonelist.Add(bonecollider);
      }

      modification.inst_DB_colliders = bonelist;

      //======================================
      // Modify DynamicBones Component with data

      var root = Utils.FindChildInTree(modification.inst_armature.transform, modification.dynamicBoneData.m_Root);

      DB.m_Root = root;
      DB.m_Damping = modification.dynamicBoneData.m_Damping;
      DB.m_DampingDistrib = modification.dynamicBoneData.m_DampingDistrib;
      DB.m_Elasticity = modification.dynamicBoneData.m_Elasticity;
      DB.m_ElasticityDistrib = modification.dynamicBoneData.m_ElasticityDistrib;
      DB.m_Stiffness = modification.dynamicBoneData.m_Stiffness;
      DB.m_StiffnessDistrib = modification.dynamicBoneData.m_StiffnessDistrib;
      DB.m_Inert = modification.dynamicBoneData.m_Inert;
      DB.m_InertDistrib = modification.dynamicBoneData.m_InertDistrib;
      DB.m_Radius = modification.dynamicBoneData.m_Radius;
      DB.m_RadiusDistrib = modification.dynamicBoneData.m_RadiusDistrib;
      DB.m_EndLength = modification.dynamicBoneData.m_EndLength;
      DB.m_EndOffset = modification.dynamicBoneData.m_EndOffset;
      DB.m_Gravity = modification.dynamicBoneData.m_Gravity;
      DB.m_Force = modification.dynamicBoneData.m_Force;

      DB.m_Colliders = bonelist;
      DB.m_Exclusions = new List<Transform>();
      foreach (var exclude in modification.dynamicBoneData.m_Exclusions)
      {
        //NOTE: Assumption here is that the dynamic bone root is part of the new armature and we are only excluding bones located in root
        var transform = Utils.FindChildInTree(root, exclude);

        if (transform != null)
          DB.m_Exclusions.Add(transform);
        else
          Debug.LogWarning("Tried to exclude a transform that could not be found");
      }


      DB.m_FreezeAxis = modification.dynamicBoneData.m_FreezeAxis;

      //TODO: Read DB and compare it to what's made in OG mod cause skirt is behaving oddly
    }

    private static void ModificationAddRenderers(GameObject newPart, CharacterModel characterModel)
    {

      //get renderers
      var renderers = newPart.GetComponentsInChildren<SkinnedMeshRenderer>(true);

      //resize render array to account for new renderers
      Array.Resize(ref characterModel.baseRendererInfos, characterModel.baseRendererInfos.Length + renderers.Length);

      //NOTE: Need to save the number of renderers added to the character render info so we can remove them cleanly. Probably add this to modifications
      if (renderers.Length != 0)
      {
        int i = renderers.Length;
        foreach (var renderer in renderers)
        {
          //2 to add - 3 in
          //resize array to 5
          //first is added at 5-2 (3) which is position 4
          //second is added at 5-1 (4) which is position 5
          //exits

          characterModel.baseRendererInfos[characterModel.baseRendererInfos.Length - i] = new CharacterModel.RendererInfo
          {
            renderer = renderers[renderers.Length - i],
            ignoreOverlays = false,
            defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
            defaultMaterial = renderer.sharedMaterial
          };

          i--; //decrement i to reach the next renederer
        }
      }
    }

    private void clearModification(Modification modification, GameObject modelObject, AppliedModifications modifications)
    {
      //Destroy Dynamic Bones colliders
      if (modification.inst_DB_colliders != null)
      {
        foreach (var collider in modification.inst_DB_colliders)
        {
          UnityEngine.GameObject.Destroy(collider);
        }
      }

      //Remove Additions to Bone Arrays
      if (modification.affectsbasemodel)
      {
        var renderers = DynamicSkinHelpers.GetBaseSkinRenderers(modelObject);
        var oldBones = renderers[0].bones.ToList();

        oldBones.RemoveRange(modification.boneIndex, modification.boneCount);

        foreach (var renderer in renderers)
        {
          renderer.bones = oldBones.ToArray();
        }
      }

      //Destroy Dynamic Bones Component (Probably don't have to do this since it will be destroyed along with PrefabInstance if parented to it)
      UnityEngine.GameObject.Destroy(modifications.OtherModifications[0].inst_dynamicBone);
      //Destroy Armature
      UnityEngine.GameObject.Destroy(modifications.OtherModifications[0].inst_armature);
      //Destroy Prefab Instance
      UnityEngine.GameObject.Destroy(modifications.OtherModifications[0].instance);

      bool removed = modifications.OtherModifications.Remove(modification);
      if (!removed) InstanceLogger.LogError("Skin Modification was not removed");
    }

    ////// Dynamic Skin Functions
    ////////////////////////////////////////////////////////////////////////////

  }

  ////////////////////////////////////////////////////////////////////////////
  ////// Dynamic Skin Classes

  class Modification
  {

    public Modification(string PrefabPath, string ParentName, string BodyName, string ParentSkinToken, int BoneIndex, bool AffectsBaseModel, AssetBundle assetBundle)
    {
      bodyname = BodyName;
      prefabpath = PrefabPath;
      parentname = ParentName;
      parentSkinToken = ParentSkinToken;
      affectsbasemodel = AffectsBaseModel;
      boneIndex = BoneIndex;
      prefab = assetBundle.LoadAsset<GameObject>(@prefabpath);
      if (prefab == null) { Debug.LogWarning("Asset at " + PrefabPath + " was not loaded"); } //DEBUG check for if asset was not loaded
    }

    //////////////////////////////////////////////////////
    /// These are created when the Modification is created
    public string prefabpath;
    public string bodyname; //Name of the base BodyName. i.e. MercBody or MageBody
    public string parentname; //the name of the bone we want to parent this modification to
    public GameObject prefab;
    public bool affectsbasemodel; //if the modification affects the base model then we need to do additional steps
                                  //TODO: Add Support for multiple Dynamic Bone Scripts per modification
    public DynamicBoneData dynamicBoneData;
    public string parentSkinToken;
    /// 
    //////////////////////////////////////////////////////

    //////////////////////////////////////////////////////
    /// Used for bones that need to be added to base model
    public int boneIndex; //index of bone in bone array, created on modification
    public int boneCount; //number of bones in prefab bone armature
    ///
    //////////////////////////////////////////////////////

    //////////////////////////////////////////////////////
    /// These objects are instanceated and destroyed on skinDefApply
    public GameObject instance; //The created instance of the prefab attatched to the character
    public GameObject inst_armature; //the armature of the created instance

    //TODO: add support for multiple DynamicBone Scripts per modification
    public DynamicBone inst_dynamicBone; //the dynamic bone attatched to the instance
    public List<DynamicBoneCollider> inst_DB_colliders = new List<DynamicBoneCollider>(); //List of Dynamic Bone Colliders that were attatched to other bones for this modification
    ///
    //////////////////////////////////////////////////////

    //This only contained the Skinned Mesh Renderers and I think I can do these inline instead
    // //////////////////////////////////////////////////////
    // /// These don't seem to be created or destroyed and are just assigned to
    // 
    // //Note: it looks like all the mesh renderers are using the same bone list as the base code only ever looked at the one and assigned to both
    // //these were taken from the meshes at root of the model
    // SkinnedMeshRenderer[] meshRenderers;
    // 
    // /// These don't seem to be created or destroyed and are just assigned to
    // //////////////////////////////////////////////////////
  }

  class AppliedModifications
  {
    public Stack<Modification> BaseModelModifications = new Stack<Modification>();
    public List<Modification> OtherModifications = new List<Modification>();//storage for modifications
  }

  //Data classes include strings in place of transforms as we need to search for the transforms when we load the data in
  class DynamicBoneData
  {
    public DynamicBoneData(string root,
                   float damping, AnimationCurve damping_dist,
                   float elasticity, AnimationCurve elasticity_dist,
                   float stiffness, AnimationCurve stiffness_dist,
                   float inert, AnimationCurve inert_dist,
                   //float friction, AnimationCurve friction_dist, //NOTE: looks like ROR2 is using an older version of DynBone that doesn't have friction
                   float radius, AnimationCurve radius_dist,
                   float end_length, Vector3 end_offset,
                   Vector3 gravity, Vector3 force,
                   List<DynamicBoneColliderData> colliders,
                   List<string> exclusions,
                   DynamicBone.FreezeAxis freeze_axis)
    {
      m_Root = root;
      m_Damping = damping;
      m_DampingDistrib = damping_dist;
      m_Elasticity = elasticity;
      m_ElasticityDistrib = elasticity_dist;
      m_Stiffness = stiffness;
      m_StiffnessDistrib = stiffness_dist;
      m_Inert = inert;
      m_InertDistrib = inert_dist;
      //new_DB.m_Friction = friction;
      //new_DB.m_FrictionDistrib = friction_dist;
      m_Radius = radius;
      m_RadiusDistrib = radius_dist;
      m_EndLength = end_length;
      m_EndOffset = end_offset;
      m_Gravity = gravity;
      m_Force = force;
      m_Colliders = colliders;
      m_Exclusions = exclusions;
      m_FreezeAxis = freeze_axis;
    }

    //would include string for parent_name but all dynamicbones should be created on modification_instance

    public string m_Root;
    public float m_Damping;
    public AnimationCurve m_DampingDistrib;
    public float m_Elasticity;
    public AnimationCurve m_ElasticityDistrib;
    public float m_Stiffness;
    public AnimationCurve m_StiffnessDistrib;
    public float m_Inert;
    public AnimationCurve m_InertDistrib;
    //public float friction; public AnimationCurve friction_dist; //NOTE: looks like ROR2 is using an older version of DynBone that doesn't have friction
    public float m_Radius;
    public AnimationCurve m_RadiusDistrib;
    public float m_EndLength;
    public Vector3 m_EndOffset;
    public Vector3 m_Gravity;
    public Vector3 m_Force;
    public List<DynamicBoneColliderData> m_Colliders;
    public List<string> m_Exclusions;
    public DynamicBone.FreezeAxis m_FreezeAxis;
  }

  class DynamicBoneColliderData
  {
    public DynamicBoneColliderData(string parent_name, DynamicBoneCollider.Direction direction, Vector3 Center, DynamicBoneCollider.Bound bound, float radius, float heaight)
    {
      m_parent_name = parent_name;
      m_Direction = direction;
      m_Center = Center;
      m_Bound = bound;
      m_Radius = radius;
      m_Height = heaight;
    }

    public string m_parent_name;
    public DynamicBoneCollider.Direction m_Direction;
    public Vector3 m_Center;
    public DynamicBoneCollider.Bound m_Bound;
    public float m_Radius;
    public float m_Height;

  }

  ////// Dynamic Skin Classes
  ////////////////////////////////////////////////////////////////////////////
}