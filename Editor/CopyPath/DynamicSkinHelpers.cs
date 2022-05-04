using UnityEngine;
using System.Collections.Generic;

namespace RuneFoxMods.DynamicSkinBuilder
{
  public static class DynamicSkinHelpers
  {
    ////////////////////////////////////////////////////////////////////////////
    ////// Utility Funcs

    //The targetBone has to be located within the armature used by the bone list in order to find the index
    //Bones are in a different order than in prefab. Things like Clavicle.l, Neck, Clavicle.R where the order in transform is clavicle.l, clavical.r, neck
    //!!!THIS DOESN"T WORK!!!! - Use FindBoneIndex2, I think that one works as intended
    public static int FindBoneIndex(Transform targetBone, List<Transform> BoneList)
    {
      /*
       * idea: find parent bone we want to add bones to
       * go up one to find it's parent
       * find the next bone after the bone we want to parent to in it's parent's heiarchy
       * find that bone's location in the bone array
       * go one step back and we should be at the place to insert
       * 
       * edge cases:
       * attatching to Root, just go to end of list
       * if parent bone is last bone in it's parent we have to go one step up, repeat until we hit root or a parent that isn't the last node in it's parent's list
       * NOTE: this will probably break on bones that have more than just bones in their child heiarchy.
       */

      if (targetBone.parent.name.Contains("Armature") == true)//if this was ROOT, ROOT is always a child of Armature on base characters
      {
        return BoneList.Count; //return the end of the list as adding a bone there will make it a child of the root
      }
      var parent = targetBone.parent;
      var index = targetBone.GetSiblingIndex();
      var childcount = parent.childCount;

      if (index == childcount - 1)//if current bone is at end of list of children of parent bone we need to go up to the next parent
      {
        return FindBoneIndex(targetBone.parent, BoneList);
      }
      else
      {
        //otherwise we can find the index of next bone by searching the bonelist for it's name
        var nextBone = targetBone.parent.GetChild(index + 1);

        //NOTE: if this is gonna break due to non_bone children it'll probably be here
        var nextBoneIndex = BoneList.FindIndex(x => x == nextBone); //will return -1 if nextbone cannot be found

        if (nextBoneIndex == -1)
        {
          //couldn't find the index of the next sibling in bone index so we look for the next-next sibling using the next sibling
          FindBoneIndex(nextBone, BoneList);
        }

        return nextBoneIndex;
      }
    }

    public static int FindBoneIndex2(Transform targetBone, List<Transform> BoneList)
    {
      /*Dumb idea #2
       * We find and mark all bones in bonelist that have target as a 'root' parent then find the one with the lowest index after the target and set index to that + 1
       */

      //bool is true if transform has target as parent somehwere in it's heiarchy
      Dictionary<Transform, bool> BoneDictionary = new Dictionary<Transform, bool>();

      int targetIndex = 99999;//picked an arbitrarily large number. if there's somehow this many bones, we got bigger problems

      //could use a foreach here but for feels cleaner to interpret
      for (int i = 0; i < BoneList.Count; i++)
      {
        var bone = BoneList[i];
        if (bone.name == "ROOT") //top most bone, can't search for parent here so targetbone can't be parent, This should also be the top most bone in bonelist but who knows *shrug*
        {
          //DEBUG          Debug.Log("Root added to dict");
          //add root to bonedictionary
          BoneDictionary.Add(bone, false);
          continue;
        }

        //There are three cases
        //bone is target
        //bone is child of target
        //bone is not child of target

        if (bone == targetBone)
        {
          targetIndex = i;
        }

        var parent = bone.transform.parent;

        //check if parent is target
        if (parent == targetBone)
        {
          //DEBUG          Debug.Log("target added to dict");
          //add bone to dictionary
          BoneDictionary.Add(bone, true);
          continue;
        }

        //check if parent is in dictionary
        bool parentInDictionary = BoneDictionary.ContainsKey(parent);

        if (parentInDictionary)
        {
          //check if parent is a child of target, children of a bone that is already a child of target will also be children of target
          if (BoneDictionary[parent] == true)//parent is a child of target
          {
            //add bone to dictionary
            //DEBUG            Debug.Log("child added to dict");
            BoneDictionary.Add(bone, true);
          }
          else
          {
            //parent not a child of target, therefore children cannot be child of target
            //add bone to dictionary
            //DEBUG            Debug.Log("Non-Child added to dict");
            BoneDictionary.Add(bone, false);

            //check if index is higher than target index. this means we are either in the children of the target, or we are in the next bone after the target, which is what we want
            if (i > targetIndex)
            {
              //DEBUG              Debug.Log(highestIndex + "  " + bone.name);
              return i;//looking for lowest index higher than index of target since that is the index of the end of all the children of target
            }
          }
        }
        else //parent is not in dictionary
        {
          //Not sure what to do here, theoretically all bones should have their parent visited first before they are visited
          //DEBUG          Debug.Log("A bone was found whose parent was not in the dictionary");
          //DEBUG          Debug.Log("Bone Name: " + bone.name);
          //DEBUG          Debug.Log("Parent Name: " + parent.name);
        }
      }

      return BoneList.Count - 1;//returns end of list as that would give index that is last child of list
    }

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
}