// Decompiled with JetBrains decompiler
// Type: BehaviorDesigner.Editor.FieldInspector
// Assembly: BehaviorDesignerEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 99CE4D00-DFA2-42D1-ABFC-D630AB4C1372
// Assembly location: C:\Users\Ron\Desktop\bt\BehaviorDesignerEditor.dll

using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace BehaviorDesigner.Editor
{
  public static class FieldInspector
  {
    private static int currentKeyboardControl = -1;
    private static bool editingArray = false;
    private static int savedArraySize = -1;
    private static HashSet<int> drawnObjects = new HashSet<int>();
    private const string c_EditorPrefsFoldoutKey = "BehaviorDesigner.Editor.Foldout.";
    private static int editingFieldHash;
    public static BehaviorSource behaviorSource;
    private static string[] layerNames;
    private static int[] maskValues;

    public static void Init()
    {
      FieldInspector.InitLayers();
    }

    public static bool DrawFoldout(int hash, GUIContent guiContent)
    {
      string str = "BehaviorDesigner.Editor.Foldout.." + (object) hash + "." + guiContent.get_text();
      bool flag1 = EditorPrefs.GetBool(str, true);
      bool flag2 = EditorGUILayout.Foldout(flag1, guiContent);
      if (flag2 != flag1)
        EditorPrefs.SetBool(str, flag2);
      return flag2;
    }

    public static object DrawFields(Task task, object obj)
    {
      return FieldInspector.DrawFields(task, obj, (GUIContent) null);
    }

    public static object DrawFields(Task task, object obj, GUIContent guiContent)
    {
      if (obj == null)
        return (object) null;
      List<Type> baseClasses = FieldInspector.GetBaseClasses(obj.GetType());
      BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
      for (int index1 = baseClasses.Count - 1; index1 > -1; --index1)
      {
        FieldInfo[] fields = baseClasses[index1].GetFields(bindingAttr);
        for (int index2 = 0; index2 < fields.Length; ++index2)
        {
          if (!BehaviorDesignerUtility.HasAttribute(fields[index2], typeof (NonSerializedAttribute)) && !BehaviorDesignerUtility.HasAttribute(fields[index2], typeof (HideInInspector)) && (!fields[index2].IsPrivate && !fields[index2].IsFamily || BehaviorDesignerUtility.HasAttribute(fields[index2], typeof (SerializeField))) && (!(obj is ParentTask) || !fields[index2].Name.Equals("children")))
          {
            if (guiContent == null)
            {
              string name = fields[index2].Name;
              TooltipAttribute[] customAttributes;
              guiContent = (customAttributes = fields[index2].GetCustomAttributes(typeof (TooltipAttribute), false) as TooltipAttribute[]).Length <= 0 ? new GUIContent(BehaviorDesignerUtility.SplitCamelCase(name)) : new GUIContent(BehaviorDesignerUtility.SplitCamelCase(name), customAttributes[0].get_Tooltip());
            }
            EditorGUI.BeginChangeCheck();
            object obj1 = FieldInspector.DrawField(task, guiContent, fields[index2], fields[index2].GetValue(obj));
            if (EditorGUI.EndChangeCheck())
            {
              fields[index2].SetValue(obj, obj1);
              GUI.set_changed(true);
            }
            guiContent = (GUIContent) null;
          }
        }
      }
      return obj;
    }

    public static List<Type> GetBaseClasses(Type t)
    {
      List<Type> typeList = new List<Type>();
      for (; t != null && !t.Equals(typeof (ParentTask)) && (!t.Equals(typeof (Task)) && !t.Equals(typeof (SharedVariable))); t = t.BaseType)
        typeList.Add(t);
      return typeList;
    }

    public static object DrawField(
      Task task,
      GUIContent guiContent,
      FieldInfo field,
      object value)
    {
      ObjectDrawer objectDrawer1;
      if ((objectDrawer1 = ObjectDrawerUtility.GetObjectDrawer(task, field)) != null)
      {
        if (value == null && !field.FieldType.IsAbstract)
          value = Activator.CreateInstance(field.FieldType, true);
        objectDrawer1.Value = value;
        objectDrawer1.OnGUI(guiContent);
        if (objectDrawer1.Value != value)
        {
          value = objectDrawer1.Value;
          GUI.set_changed(true);
        }
        return value;
      }
      ObjectDrawerAttribute[] customAttributes;
      ObjectDrawer objectDrawer2;
      if ((customAttributes = field.GetCustomAttributes(typeof (ObjectDrawerAttribute), true) as ObjectDrawerAttribute[]).Length <= 0 || (objectDrawer2 = ObjectDrawerUtility.GetObjectDrawer(task, customAttributes[0])) == null)
        return FieldInspector.DrawField(task, guiContent, field, field.FieldType, value);
      if (value == null)
        value = Activator.CreateInstance(field.FieldType, true);
      objectDrawer2.Value = value;
      objectDrawer2.OnGUI(guiContent);
      if (objectDrawer2.Value != value)
      {
        value = objectDrawer2.Value;
        GUI.set_changed(true);
      }
      return value;
    }

    private static object DrawField(
      Task task,
      GUIContent guiContent,
      FieldInfo fieldInfo,
      Type fieldType,
      object value)
    {
      if (typeof (IList).IsAssignableFrom(fieldType))
        return FieldInspector.DrawArrayField(task, guiContent, fieldInfo, fieldType, value);
      return FieldInspector.DrawSingleField(task, guiContent, fieldInfo, fieldType, value);
    }

    private static object DrawArrayField(
      Task task,
      GUIContent guiContent,
      FieldInfo fieldInfo,
      Type fieldType,
      object value)
    {
      Type type1;
      if (fieldType.IsArray)
      {
        type1 = fieldType.GetElementType();
      }
      else
      {
        Type type2 = fieldType;
        while (!type2.IsGenericType)
          type2 = type2.BaseType;
        type1 = type2.GetGenericArguments()[0];
      }
      IList list;
      if (value == null)
      {
        if (fieldType.IsGenericType || fieldType.IsArray)
          list = Activator.CreateInstance(typeof (List<>).MakeGenericType(type1), true) as IList;
        else
          list = Activator.CreateInstance(fieldType, true) as IList;
        if (fieldType.IsArray)
        {
          Array instance = Array.CreateInstance(type1, list.Count);
          list.CopyTo(instance, 0);
          list = (IList) instance;
        }
        GUI.set_changed(true);
      }
      else
        list = (IList) value;
      EditorGUILayout.BeginVertical(new GUILayoutOption[0]);
      if (FieldInspector.DrawFoldout(guiContent.get_text().GetHashCode(), guiContent))
      {
        EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() + 1);
        bool flag = guiContent.get_text().GetHashCode() == FieldInspector.editingFieldHash;
        int num = !flag ? list.Count : FieldInspector.savedArraySize;
        int length = EditorGUILayout.IntField("Size", num, new GUILayoutOption[0]);
        if (flag && FieldInspector.editingArray && (GUIUtility.get_keyboardControl() != FieldInspector.currentKeyboardControl || Event.get_current().get_keyCode() == 13))
        {
          if (length != list.Count)
          {
            Array instance1 = Array.CreateInstance(type1, length);
            int index1 = -1;
            for (int index2 = 0; index2 < length; ++index2)
            {
              if (index2 < list.Count)
                index1 = index2;
              if (index1 != -1)
              {
                object instance2 = list[index1];
                if (index2 >= list.Count && !typeof (Object).IsAssignableFrom(type1) && !typeof (string).IsAssignableFrom(type1))
                  instance2 = Activator.CreateInstance(list[index1].GetType(), true);
                instance1.SetValue(instance2, index2);
              }
              else
                break;
            }
            if (fieldType.IsArray)
            {
              list = (IList) instance1;
            }
            else
            {
              if (fieldType.IsGenericType)
                list = Activator.CreateInstance(typeof (List<>).MakeGenericType(type1), true) as IList;
              else
                list = Activator.CreateInstance(fieldType, true) as IList;
              for (int index2 = 0; index2 < instance1.Length; ++index2)
                list.Add(instance1.GetValue(index2));
            }
          }
          FieldInspector.editingArray = false;
          FieldInspector.savedArraySize = -1;
          FieldInspector.editingFieldHash = -1;
          GUI.set_changed(true);
        }
        else if (length != num)
        {
          if (!FieldInspector.editingArray)
          {
            FieldInspector.currentKeyboardControl = GUIUtility.get_keyboardControl();
            FieldInspector.editingArray = true;
            FieldInspector.editingFieldHash = guiContent.get_text().GetHashCode();
          }
          FieldInspector.savedArraySize = length;
        }
        for (int index = 0; index < list.Count; ++index)
        {
          GUILayout.BeginHorizontal(new GUILayoutOption[0]);
          guiContent.set_text("Element " + (object) index);
          list[index] = FieldInspector.DrawField(task, guiContent, fieldInfo, type1, list[index]);
          GUILayout.Space(6f);
          GUILayout.EndHorizontal();
        }
        EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() - 1);
      }
      EditorGUILayout.EndVertical();
      return (object) list;
    }

    private static object DrawSingleField(
      Task task,
      GUIContent guiContent,
      FieldInfo fieldInfo,
      Type fieldType,
      object value)
    {
      if (fieldType.Equals(typeof (int)))
        return (object) EditorGUILayout.IntField(guiContent, (int) value, new GUILayoutOption[0]);
      if (fieldType.Equals(typeof (float)))
        return (object) EditorGUILayout.FloatField(guiContent, (float) value, new GUILayoutOption[0]);
      if (fieldType.Equals(typeof (double)))
        return (object) EditorGUILayout.FloatField(guiContent, Convert.ToSingle((double) value), new GUILayoutOption[0]);
      if (fieldType.Equals(typeof (long)))
        return (object) (long) EditorGUILayout.IntField(guiContent, Convert.ToInt32((long) value), new GUILayoutOption[0]);
      if (fieldType.Equals(typeof (bool)))
        return (object) EditorGUILayout.Toggle(guiContent, (bool) value, new GUILayoutOption[0]);
      if (fieldType.Equals(typeof (string)))
        return (object) EditorGUILayout.TextField(guiContent, (string) value, new GUILayoutOption[0]);
      if (fieldType.Equals(typeof (byte)))
        return (object) Convert.ToByte(EditorGUILayout.IntField(guiContent, Convert.ToInt32(value), new GUILayoutOption[0]));
      if (fieldType.Equals(typeof (Vector2)))
        return (object) EditorGUILayout.Vector2Field(guiContent, (Vector2) value, new GUILayoutOption[0]);
      if (fieldType.Equals(typeof (Vector3)))
        return (object) EditorGUILayout.Vector3Field(guiContent, (Vector3) value, new GUILayoutOption[0]);
      if (fieldType.Equals(typeof (Vector4)))
        return (object) EditorGUILayout.Vector4Field(guiContent.get_text(), (Vector4) value, new GUILayoutOption[0]);
      if (fieldType.Equals(typeof (Quaternion)))
      {
        Quaternion quaternion = (Quaternion) value;
        Vector4 zero = Vector4.get_zero();
        ((Vector4) ref zero).Set((float) quaternion.x, (float) quaternion.y, (float) quaternion.z, (float) quaternion.w);
        Vector4 vector4 = EditorGUILayout.Vector4Field(guiContent.get_text(), zero, new GUILayoutOption[0]);
        ((Quaternion) ref quaternion).Set((float) vector4.x, (float) vector4.y, (float) vector4.z, (float) vector4.w);
        return (object) quaternion;
      }
      if (fieldType.Equals(typeof (Color)))
        return (object) EditorGUILayout.ColorField(guiContent, (Color) value, new GUILayoutOption[0]);
      if (fieldType.Equals(typeof (Rect)))
        return (object) EditorGUILayout.RectField(guiContent, (Rect) value, new GUILayoutOption[0]);
      if (fieldType.Equals(typeof (Matrix4x4)))
      {
        GUILayout.BeginVertical(new GUILayoutOption[0]);
        if (FieldInspector.DrawFoldout(guiContent.get_text().GetHashCode(), guiContent))
        {
          EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() + 1);
          Matrix4x4 matrix4x4 = (Matrix4x4) value;
          for (int index1 = 0; index1 < 4; ++index1)
          {
            for (int index2 = 0; index2 < 4; ++index2)
            {
              EditorGUI.BeginChangeCheck();
              ((Matrix4x4) ref matrix4x4).set_Item(index1, index2, EditorGUILayout.FloatField("E" + index1.ToString() + index2.ToString(), ((Matrix4x4) ref matrix4x4).get_Item(index1, index2), new GUILayoutOption[0]));
              if (EditorGUI.EndChangeCheck())
                GUI.set_changed(true);
            }
          }
          value = (object) matrix4x4;
          EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() - 1);
        }
        GUILayout.EndVertical();
        return value;
      }
      if (fieldType.Equals(typeof (AnimationCurve)))
      {
        if (value == null)
        {
          value = (object) AnimationCurve.EaseInOut(0.0f, 0.0f, 1f, 1f);
          GUI.set_changed(true);
        }
        return (object) EditorGUILayout.CurveField(guiContent, (AnimationCurve) value, new GUILayoutOption[0]);
      }
      if (fieldType.Equals(typeof (LayerMask)))
        return (object) FieldInspector.DrawLayerMask(guiContent, (LayerMask) value);
      if (typeof (SharedVariable).IsAssignableFrom(fieldType))
        return (object) FieldInspector.DrawSharedVariable(task, guiContent, fieldInfo, fieldType, value as SharedVariable);
      if (typeof (Object).IsAssignableFrom(fieldType))
        return (object) EditorGUILayout.ObjectField(guiContent, (Object) value, fieldType, true, new GUILayoutOption[0]);
      if (fieldType.IsEnum)
        return (object) EditorGUILayout.EnumPopup(guiContent, (Enum) value, new GUILayoutOption[0]);
      if (fieldType.IsClass || fieldType.IsValueType && !fieldType.IsPrimitive)
      {
        if (typeof (Delegate).IsAssignableFrom(fieldType))
          return (object) null;
        int hashCode = guiContent.get_text().GetHashCode();
        if (FieldInspector.drawnObjects.Contains(hashCode))
          return (object) null;
        try
        {
          FieldInspector.drawnObjects.Add(hashCode);
          GUILayout.BeginVertical(new GUILayoutOption[0]);
          if (value == null)
          {
            if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof (Nullable<>))
              fieldType = Nullable.GetUnderlyingType(fieldType);
            value = Activator.CreateInstance(fieldType, true);
          }
          if (FieldInspector.DrawFoldout(hashCode, guiContent))
          {
            EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() + 1);
            value = FieldInspector.DrawFields(task, value);
            EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() - 1);
          }
          FieldInspector.drawnObjects.Remove(hashCode);
          GUILayout.EndVertical();
          return value;
        }
        catch (Exception ex)
        {
          GUILayout.EndVertical();
          FieldInspector.drawnObjects.Remove(hashCode);
          return (object) null;
        }
      }
      else
      {
        EditorGUILayout.LabelField("Unsupported Type: " + (object) fieldType, new GUILayoutOption[0]);
        return (object) null;
      }
    }

    public static SharedVariable DrawSharedVariable(
      Task task,
      GUIContent guiContent,
      FieldInfo fieldInfo,
      Type fieldType,
      SharedVariable sharedVariable)
    {
      if (!fieldType.Equals(typeof (SharedVariable)) && sharedVariable == null)
      {
        sharedVariable = Activator.CreateInstance(fieldType, true) as SharedVariable;
        if (TaskUtility.HasAttribute(fieldInfo, typeof (RequiredFieldAttribute)) || TaskUtility.HasAttribute(fieldInfo, typeof (SharedRequiredAttribute)))
          sharedVariable.set_IsShared(true);
        GUI.set_changed(true);
      }
      if (sharedVariable == null || sharedVariable.get_IsShared())
      {
        GUILayout.BeginHorizontal(new GUILayoutOption[0]);
        string[] names = (string[]) null;
        int globalStartIndex = -1;
        int variablesOfType = FieldInspector.GetVariablesOfType(sharedVariable == null ? (Type) null : ((object) sharedVariable).GetType().GetProperty("Value").PropertyType, sharedVariable != null && sharedVariable.get_IsGlobal(), sharedVariable == null ? string.Empty : sharedVariable.get_Name(), FieldInspector.behaviorSource, out names, ref globalStartIndex, fieldType.Equals(typeof (SharedVariable)));
        Color backgroundColor = GUI.get_backgroundColor();
        if (variablesOfType == 0 && !TaskUtility.HasAttribute(fieldInfo, typeof (SharedRequiredAttribute)))
          GUI.set_backgroundColor(Color.get_red());
        int num = variablesOfType;
        int index = EditorGUILayout.Popup(guiContent.get_text(), variablesOfType, names, BehaviorDesignerUtility.SharedVariableToolbarPopup, new GUILayoutOption[0]);
        GUI.set_backgroundColor(backgroundColor);
        if (index != num)
        {
          if (index == 0)
          {
            if (fieldType.Equals(typeof (SharedVariable)))
            {
              sharedVariable = (SharedVariable) null;
            }
            else
            {
              sharedVariable = Activator.CreateInstance(fieldType, true) as SharedVariable;
              sharedVariable.set_IsShared(true);
            }
          }
          else
            sharedVariable = globalStartIndex == -1 || index < globalStartIndex ? FieldInspector.behaviorSource.GetVariable(names[index]) : GlobalVariables.get_Instance().GetVariable(names[index].Substring(8, names[index].Length - 8));
          GUI.set_changed(true);
        }
        if (!fieldType.Equals(typeof (SharedVariable)) && !TaskUtility.HasAttribute(fieldInfo, typeof (RequiredFieldAttribute)) && !TaskUtility.HasAttribute(fieldInfo, typeof (SharedRequiredAttribute)))
        {
          sharedVariable = FieldInspector.DrawSharedVariableToggleSharedButton(sharedVariable);
          GUILayout.Space(-3f);
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(3f);
      }
      else
      {
        GUILayout.BeginHorizontal(new GUILayoutOption[0]);
        ObjectDrawerAttribute[] customAttributes;
        ObjectDrawer objectDrawer;
        if (fieldInfo != null && (customAttributes = fieldInfo.GetCustomAttributes(typeof (ObjectDrawerAttribute), true) as ObjectDrawerAttribute[]).Length > 0 && (objectDrawer = ObjectDrawerUtility.GetObjectDrawer(task, customAttributes[0])) != null)
        {
          objectDrawer.Value = (object) sharedVariable;
          objectDrawer.OnGUI(guiContent);
        }
        else
          FieldInspector.DrawFields(task, (object) sharedVariable, guiContent);
        if (!TaskUtility.HasAttribute(fieldInfo, typeof (RequiredFieldAttribute)) && !TaskUtility.HasAttribute(fieldInfo, typeof (SharedRequiredAttribute)))
          sharedVariable = FieldInspector.DrawSharedVariableToggleSharedButton(sharedVariable);
        GUILayout.EndHorizontal();
      }
      return sharedVariable;
    }

    public static int GetVariablesOfType(
      Type valueType,
      bool isGlobal,
      string name,
      BehaviorSource behaviorSource,
      out string[] names,
      ref int globalStartIndex,
      bool getAll)
    {
      if (behaviorSource == null)
      {
        names = new string[0];
        return 0;
      }
      List<SharedVariable> variables1 = behaviorSource.get_Variables();
      int num = 0;
      List<string> stringList = new List<string>();
      stringList.Add("None");
      if (variables1 != null)
      {
        for (int index = 0; index < variables1.Count; ++index)
        {
          if (variables1[index] != null)
          {
            Type propertyType = ((object) variables1[index]).GetType().GetProperty("Value").PropertyType;
            if (valueType == null || getAll || valueType.IsAssignableFrom(propertyType))
            {
              stringList.Add(variables1[index].get_Name());
              if (!isGlobal && variables1[index].get_Name().Equals(name))
                num = stringList.Count - 1;
            }
          }
        }
      }
      GlobalVariables instance;
      if (Object.op_Inequality((Object) (instance = GlobalVariables.get_Instance()), (Object) null))
      {
        globalStartIndex = stringList.Count;
        List<SharedVariable> variables2 = instance.get_Variables();
        if (variables2 != null)
        {
          for (int index = 0; index < variables2.Count; ++index)
          {
            if (variables2[index] != null)
            {
              Type propertyType = ((object) variables2[index]).GetType().GetProperty("Value").PropertyType;
              if (valueType == null || getAll || propertyType.Equals(valueType))
              {
                stringList.Add("Globals/" + variables2[index].get_Name());
                if (isGlobal && variables2[index].get_Name().Equals(name))
                  num = stringList.Count - 1;
              }
            }
          }
        }
      }
      names = stringList.ToArray();
      return num;
    }

    internal static SharedVariable DrawSharedVariableToggleSharedButton(
      SharedVariable sharedVariable)
    {
      if (sharedVariable == null)
        return (SharedVariable) null;
      if (GUILayout.Button((Texture) (!sharedVariable.get_IsShared() ? BehaviorDesignerUtility.VariableButtonTexture : BehaviorDesignerUtility.VariableButtonSelectedTexture), BehaviorDesignerUtility.PlainButtonGUIStyle, new GUILayoutOption[1]
      {
        GUILayout.Width(15f)
      }))
      {
        bool flag = !sharedVariable.get_IsShared();
        sharedVariable = !((object) sharedVariable).GetType().Equals(typeof (SharedVariable)) ? Activator.CreateInstance(((object) sharedVariable).GetType(), true) as SharedVariable : Activator.CreateInstance(FieldInspector.FriendlySharedVariableName(((object) sharedVariable).GetType().GetProperty("Value").PropertyType), true) as SharedVariable;
        sharedVariable.set_IsShared(flag);
      }
      return sharedVariable;
    }

    internal static Type FriendlySharedVariableName(Type type)
    {
      if (type.Equals(typeof (bool)))
        return TaskUtility.GetTypeWithinAssembly("BehaviorDesigner.Runtime.SharedBool");
      if (type.Equals(typeof (int)))
        return TaskUtility.GetTypeWithinAssembly("BehaviorDesigner.Runtime.SharedInt");
      if (type.Equals(typeof (float)))
        return TaskUtility.GetTypeWithinAssembly("BehaviorDesigner.Runtime.SharedFloat");
      if (type.Equals(typeof (string)))
        return TaskUtility.GetTypeWithinAssembly("BehaviorDesigner.Runtime.SharedString");
      if (typeof (Object).IsAssignableFrom(type))
      {
        Type typeWithinAssembly = TaskUtility.GetTypeWithinAssembly("BehaviorDesigner.Runtime.Shared" + type.Name);
        if (typeWithinAssembly != null)
          return typeWithinAssembly;
      }
      else
      {
        Type typeWithinAssembly = TaskUtility.GetTypeWithinAssembly("Shared" + type.Name);
        if (typeWithinAssembly != null)
          return typeWithinAssembly;
      }
      return type;
    }

    private static LayerMask DrawLayerMask(GUIContent guiContent, LayerMask layerMask)
    {
      if (FieldInspector.layerNames == null)
        FieldInspector.InitLayers();
      int num1 = 0;
      for (int index = 0; index < FieldInspector.layerNames.Length; ++index)
      {
        if ((((LayerMask) ref layerMask).get_value() & FieldInspector.maskValues[index]) == FieldInspector.maskValues[index])
          num1 |= 1 << index;
      }
      int num2 = EditorGUILayout.MaskField(guiContent, num1, FieldInspector.layerNames, new GUILayoutOption[0]);
      if (num2 != num1)
      {
        int num3 = 0;
        for (int index = 0; index < FieldInspector.layerNames.Length; ++index)
        {
          if ((num2 & 1 << index) != 0)
            num3 |= FieldInspector.maskValues[index];
        }
        ((LayerMask) ref layerMask).set_value(num3);
      }
      return layerMask;
    }

    private static void InitLayers()
    {
      List<string> stringList = new List<string>();
      List<int> intList = new List<int>();
      for (int index = 0; index < 32; ++index)
      {
        string name = LayerMask.LayerToName(index);
        if (!string.IsNullOrEmpty(name))
        {
          stringList.Add(name);
          intList.Add(1 << index);
        }
      }
      FieldInspector.layerNames = stringList.ToArray();
      FieldInspector.maskValues = intList.ToArray();
    }
  }
}
