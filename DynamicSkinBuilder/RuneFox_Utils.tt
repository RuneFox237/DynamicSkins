using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2;


namespace RuneFoxMods
{
  class Utils
  {

    static public void PrintBodyCatalog()
    {
      Debug.Log("\nBodyCatalog");
      foreach (var bod in BodyCatalog.allBodyPrefabs)
      {
        Debug.Log(bod.name);
      }
      Debug.Log("\n");
    }

    static public void ReadChildren(GameObject parent, int tabs, bool includecomponents = true)
    {
      string tabstring = "";
      for (int x = 0; x < tabs; x++)
      {
        tabstring += "  ";
      }

      //Debug.Log("children: " + parent.transform.childCount);

      for (int i = 0; i < parent.transform.childCount; i++)
      {
        var child = parent.transform.GetChild(i);
        if (child == null)
          return;

        string active = child.gameObject.activeInHierarchy.ToString();

        Debug.Log(tabstring + child.name + " " + active + " " + child.transform.position);
        if (includecomponents)
        {
          ReadComponents(child.gameObject, tabs + 1);
          Debug.Log("");//new line
        }
        ReadChildren(child.gameObject, tabs + 1, includecomponents);
      }
    }

    static public void readheiarchy(GameObject parent, bool includeComponents = true)
    {
      //read game object heiarchy
      Debug.Log(parent.name);
      if (includeComponents)
      {
        ReadComponents(parent, 1);
        Debug.Log("");//new line
      }
      ReadChildren(parent, 1, includeComponents);

    }

    static public void ReadComponents(GameObject obj, int tabs)
    {
      string tabstring = "";
      for (int x = 0; x < tabs; x++)
      {
        tabstring += "  ";
      }

      var list = obj.GetComponents(typeof(Component));
      foreach (var comp in list)
      {
        string Behavior_enable = "";
        //var beh = comp.GetComponent<Behaviour>();
        //if (beh)
        //  Behavior_enable = beh.isActiveAndEnabled.ToString();

        Debug.Log(tabstring + "Comp: " + comp.GetType().ToString() + "  " + Behavior_enable);
      }
    }

    static public void PrintAllPaths(GameObject parent)
    {
      //InstanceLogger.LogWarning("Start Print All Paths");
      Debug.Log(parent.name);
      PrintAllPathsInner(parent, parent.name);
      //InstanceLogger.LogWarning("End Print All Paths");
    }

    static private void PrintAllPathsInner(GameObject parent, string parent_string)
    {
      int childcount = parent.transform.childCount;
      for (int i = 0; i < childcount; i++)
      {
        var child = parent.transform.GetChild(i);

        //don't log _end transforms
        if (child.name.EndsWith("_end") == true)
          break;

        string path_string = parent_string + "/" + child.name;
        Debug.Log("  " + child.name + "\t\t" + path_string);
        PrintAllPathsInner(child.gameObject, path_string);
      }
    }

    public static void PrintDynamicBone(DynamicBone DB)
    {
      Debug.Log("Root: " + DB.m_Root);
      Debug.Log("Damping: " + DB.m_Damping);
      Debug.Log("Damping Dist: " + DB.m_DampingDistrib);
      Debug.Log("Elasticity: " + DB.m_Elasticity);
      Debug.Log("Elasticity Dist: " + DB.m_ElasticityDistrib);
      Debug.Log("Stiffness: " + DB.m_Stiffness);
      Debug.Log("Stiffness Dist: " + DB.m_StiffnessDistrib);
      Debug.Log("Inert: " + DB.m_Inert);
      Debug.Log("Inert Dist: " + DB.m_InertDistrib);
      Debug.Log("Radius: " + DB.m_Radius);
      Debug.Log("Radius Dist: " + DB.m_RadiusDistrib);
      Debug.Log("End Length: " + DB.m_EndLength);
      Debug.Log("End Offset: " + DB.m_EndOffset);
      Debug.Log("Gravity: " + DB.m_Gravity);
      Debug.Log("Force: " + DB.m_Force);
      Debug.Log("FreezeAxis: " + DB.m_FreezeAxis);

      Debug.Log("Colliders: " + DB.m_Colliders.Count);
      foreach (var collider in DB.m_Colliders)
      {
        Debug.Log("\t Parent: " + collider.transform.name);
        Debug.Log("\t Direction: " + collider.m_Direction);
        Debug.Log("\t Center " + collider.m_Center);
        Debug.Log("\t Bound " + collider.m_Bound);
        Debug.Log("\t Radius " + collider.m_Radius);
        Debug.Log("\t Height " + collider.m_Height);
      }

      Debug.Log("Exclusions: " + DB.m_Exclusions.Count);
      foreach (var trans in DB.m_Exclusions)
      {
        Debug.Log("\t Transform: " + trans.name);
      }
    }
      //finds childlocator from the Character Body
      //should work with CharacterBody.GameObject and BodyCatalog.FindPrefab GameObjects
      static public ChildLocator GetChildLocator(GameObject body)
    {
      ChildLocator locator = null;
      var child1 = body.transform.GetChild(0);
      if (child1)
      {
        var child2 = child1.GetChild(0);
        if (child2)
        {
          locator = child2.GetComponent<ChildLocator>();
          if (locator)
          {
            Debug.Log("Locator Found");
          }
        }
      }

      return locator;
    }


    //does a breadth first Search of Transform's children for a child with the given name 
    static public Transform FindChildInTree(Transform Root, string name)
    {
      Queue<Transform> transformQueue = new Queue<Transform>();

      transformQueue.Enqueue(Root);

      while (transformQueue.Count != 0)
      {
        var transform = transformQueue.Dequeue();

        //Debug.Log(transform.name);
        
        if (transform.name == name)
          return transform;

        for (int i = 0; i < transform.childCount; i++)
        {
          var child = transform.GetChild(i);
          transformQueue.Enqueue(child);
        }
      }

      return null; //could not find the child in heiarchy
    }

    //removes the annoying "(Clone)" from instantiated objects
    static public string RemoveCloneNaming(string str)
    {
      return (str.Remove(str.Length - 7));
    }

  }
}
