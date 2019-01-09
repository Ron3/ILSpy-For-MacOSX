// Decompiled with JetBrains decompiler
// Type: BehaviorDesigner.Editor.ObjectDrawerUtility
// Assembly: BehaviorDesignerEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 99CE4D00-DFA2-42D1-ABFC-D630AB4C1372
// Assembly location: C:\Users\Ron\Desktop\bt\BehaviorDesignerEditor.dll

using BehaviorDesigner.Runtime.Tasks;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace BehaviorDesigner.Editor
{
  internal static class ObjectDrawerUtility
  {
    private static Dictionary<Type, Type> objectDrawerTypeMap = new Dictionary<Type, Type>();
    private static Dictionary<int, ObjectDrawer> objectDrawerMap = new Dictionary<int, ObjectDrawer>();
    private static bool mapBuilt = false;

    private static void BuildObjectDrawers()
    {
      if (ObjectDrawerUtility.mapBuilt)
        return;
      foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
      {
        if (assembly != null)
        {
          try
          {
            foreach (Type exportedType in assembly.GetExportedTypes())
            {
              if (typeof (ObjectDrawer).IsAssignableFrom(exportedType) && exportedType.IsClass && !exportedType.IsAbstract)
              {
                CustomObjectDrawer[] customAttributes;
                if ((customAttributes = exportedType.GetCustomAttributes(typeof (CustomObjectDrawer), false) as CustomObjectDrawer[]).Length > 0)
                  ObjectDrawerUtility.objectDrawerTypeMap.Add(customAttributes[0].Type, exportedType);
              }
            }
          }
          catch (Exception ex)
          {
          }
        }
      }
      ObjectDrawerUtility.mapBuilt = true;
    }

    private static bool ObjectDrawerForType(
      Type type,
      ref ObjectDrawer objectDrawer,
      ref Type objectDrawerType,
      int hash)
    {
      ObjectDrawerUtility.BuildObjectDrawers();
      if (!ObjectDrawerUtility.objectDrawerTypeMap.ContainsKey(type))
        return false;
      objectDrawerType = ObjectDrawerUtility.objectDrawerTypeMap[type];
      if (ObjectDrawerUtility.objectDrawerMap.ContainsKey(hash))
        objectDrawer = ObjectDrawerUtility.objectDrawerMap[hash];
      return true;
    }

    public static ObjectDrawer GetObjectDrawer(Task task, FieldInfo field)
    {
      ObjectDrawer objectDrawer = (ObjectDrawer) null;
      Type objectDrawerType = (Type) null;
      if (!ObjectDrawerUtility.ObjectDrawerForType(field.FieldType, ref objectDrawer, ref objectDrawerType, field.GetHashCode()))
        return (ObjectDrawer) null;
      if (objectDrawer != null)
        return objectDrawer;
      ObjectDrawer instance = Activator.CreateInstance(objectDrawerType) as ObjectDrawer;
      instance.FieldInfo = field;
      instance.Task = task;
      ObjectDrawerUtility.objectDrawerMap.Add(field.GetHashCode(), instance);
      return instance;
    }

    public static ObjectDrawer GetObjectDrawer(
      Task task,
      ObjectDrawerAttribute attribute)
    {
      ObjectDrawer objectDrawer = (ObjectDrawer) null;
      Type objectDrawerType = (Type) null;
      if (!ObjectDrawerUtility.ObjectDrawerForType(((object) attribute).GetType(), ref objectDrawer, ref objectDrawerType, ((Attribute) attribute).GetHashCode()))
        return (ObjectDrawer) null;
      if (objectDrawer != null)
        return objectDrawer;
      ObjectDrawer instance = Activator.CreateInstance(objectDrawerType) as ObjectDrawer;
      instance.Attribute = attribute;
      instance.Task = task;
      ObjectDrawerUtility.objectDrawerMap.Add(((Attribute) attribute).GetHashCode(), instance);
      return instance;
    }
  }
}
