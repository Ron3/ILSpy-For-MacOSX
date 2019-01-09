// Decompiled with JetBrains decompiler
// Type: BehaviorDesigner.Editor.BinarySerialization
// Assembly: BehaviorDesignerEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 99CE4D00-DFA2-42D1-ABFC-D630AB4C1372
// Assembly location: C:\Users\Ron\Desktop\bt\BehaviorDesignerEditor.dll

using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace BehaviorDesigner.Editor
{
  public class BinarySerialization
  {
    private static HashSet<int> fieldHashes = new HashSet<int>();
    private static int fieldIndex;
    private static TaskSerializationData taskSerializationData;
    private static FieldSerializationData fieldSerializationData;

    public static void Save(BehaviorSource behaviorSource)
    {
      BinarySerialization.fieldIndex = 0;
      BinarySerialization.taskSerializationData = new TaskSerializationData();
      BinarySerialization.fieldSerializationData = (FieldSerializationData) BinarySerialization.taskSerializationData.fieldSerializationData;
      if (behaviorSource.get_Variables() != null)
      {
        for (int index = 0; index < behaviorSource.get_Variables().Count; ++index)
        {
          ((List<int>) BinarySerialization.taskSerializationData.variableStartIndex).Add(((List<int>) BinarySerialization.fieldSerializationData.startIndex).Count);
          BinarySerialization.SaveSharedVariable(behaviorSource.get_Variables()[index], 0);
        }
      }
      if (!object.ReferenceEquals((object) behaviorSource.get_EntryTask(), (object) null))
        BinarySerialization.SaveTask(behaviorSource.get_EntryTask(), -1);
      if (!object.ReferenceEquals((object) behaviorSource.get_RootTask(), (object) null))
        BinarySerialization.SaveTask(behaviorSource.get_RootTask(), 0);
      if (behaviorSource.get_DetachedTasks() != null)
      {
        for (int index = 0; index < behaviorSource.get_DetachedTasks().Count; ++index)
          BinarySerialization.SaveTask(behaviorSource.get_DetachedTasks()[index], -1);
      }
      BinarySerialization.taskSerializationData.Version = (__Null) "1.5.11";
      behaviorSource.set_TaskData(BinarySerialization.taskSerializationData);
      if (behaviorSource.get_Owner() == null || ((object) behaviorSource.get_Owner()).Equals((object) null))
        return;
      BehaviorDesignerUtility.SetObjectDirty(behaviorSource.get_Owner().GetObject());
    }

    public static void Save(GlobalVariables globalVariables)
    {
      if (Object.op_Equality((Object) globalVariables, (Object) null))
        return;
      BinarySerialization.fieldIndex = 0;
      globalVariables.set_VariableData(new VariableSerializationData());
      if (globalVariables.get_Variables() == null || globalVariables.get_Variables().Count == 0)
        return;
      BinarySerialization.fieldSerializationData = (FieldSerializationData) globalVariables.get_VariableData().fieldSerializationData;
      for (int index = 0; index < globalVariables.get_Variables().Count; ++index)
      {
        ((List<int>) globalVariables.get_VariableData().variableStartIndex).Add(((List<int>) BinarySerialization.fieldSerializationData.startIndex).Count);
        BinarySerialization.SaveSharedVariable(globalVariables.get_Variables()[index], 0);
      }
      globalVariables.set_Version("1.5.11");
      BehaviorDesignerUtility.SetObjectDirty((Object) globalVariables);
    }

    private static void SaveTask(Task task, int parentTaskIndex)
    {
      ((List<string>) BinarySerialization.taskSerializationData.types).Add(((object) task).GetType().ToString());
      ((List<int>) BinarySerialization.taskSerializationData.parentIndex).Add(parentTaskIndex);
      ((List<int>) BinarySerialization.taskSerializationData.startIndex).Add(((List<int>) BinarySerialization.fieldSerializationData.startIndex).Count);
      BinarySerialization.SaveField(typeof (int), "ID", 0, (object) task.get_ID(), (FieldInfo) null);
      BinarySerialization.SaveField(typeof (string), "FriendlyName", 0, (object) task.get_FriendlyName(), (FieldInfo) null);
      BinarySerialization.SaveField(typeof (bool), "IsInstant", 0, (object) task.get_IsInstant(), (FieldInfo) null);
      BinarySerialization.SaveField(typeof (bool), "Disabled", 0, (object) task.get_Disabled(), (FieldInfo) null);
      BinarySerialization.SaveNodeData(task.get_NodeData());
      BinarySerialization.SaveFields((object) task, 0);
      if (!(task is ParentTask))
        return;
      ParentTask parentTask = task as ParentTask;
      if (parentTask.get_Children() == null || parentTask.get_Children().Count <= 0)
        return;
      for (int index = 0; index < parentTask.get_Children().Count; ++index)
        BinarySerialization.SaveTask(parentTask.get_Children()[index], ((Task) parentTask).get_ID());
    }

    private static void SaveNodeData(NodeData nodeData)
    {
      BinarySerialization.SaveField(typeof (Vector2), "NodeDataOffset", 0, (object) nodeData.get_Offset(), (FieldInfo) null);
      BinarySerialization.SaveField(typeof (string), "NodeDataComment", 0, (object) nodeData.get_Comment(), (FieldInfo) null);
      BinarySerialization.SaveField(typeof (bool), "NodeDataIsBreakpoint", 0, (object) nodeData.get_IsBreakpoint(), (FieldInfo) null);
      BinarySerialization.SaveField(typeof (bool), "NodeDataCollapsed", 0, (object) nodeData.get_Collapsed(), (FieldInfo) null);
      BinarySerialization.SaveField(typeof (int), "NodeDataColorIndex", 0, (object) nodeData.get_ColorIndex(), (FieldInfo) null);
      BinarySerialization.SaveField(typeof (List<string>), "NodeDataWatchedFields", 0, (object) nodeData.get_WatchedFieldNames(), (FieldInfo) null);
    }

    private static void SaveSharedVariable(SharedVariable sharedVariable, int hashPrefix)
    {
      if (sharedVariable == null)
        return;
      BinarySerialization.SaveField(typeof (string), "Type", hashPrefix, (object) ((object) sharedVariable).GetType().ToString(), (FieldInfo) null);
      BinarySerialization.SaveField(typeof (string), "Name", hashPrefix, (object) sharedVariable.get_Name(), (FieldInfo) null);
      if (sharedVariable.get_IsShared())
        BinarySerialization.SaveField(typeof (bool), "IsShared", hashPrefix, (object) sharedVariable.get_IsShared(), (FieldInfo) null);
      if (sharedVariable.get_IsGlobal())
        BinarySerialization.SaveField(typeof (bool), "IsGlobal", hashPrefix, (object) sharedVariable.get_IsGlobal(), (FieldInfo) null);
      if (sharedVariable.get_NetworkSync())
        BinarySerialization.SaveField(typeof (bool), "NetworkSync", hashPrefix, (object) sharedVariable.get_NetworkSync(), (FieldInfo) null);
      if (!string.IsNullOrEmpty(sharedVariable.get_PropertyMapping()))
      {
        BinarySerialization.SaveField(typeof (string), "PropertyMapping", hashPrefix, (object) sharedVariable.get_PropertyMapping(), (FieldInfo) null);
        if (!object.Equals((object) sharedVariable.get_PropertyMappingOwner(), (object) null))
          BinarySerialization.SaveField(typeof (GameObject), "PropertyMappingOwner", hashPrefix, (object) sharedVariable.get_PropertyMappingOwner(), (FieldInfo) null);
      }
      BinarySerialization.SaveFields((object) sharedVariable, hashPrefix);
    }

    private static void SaveFields(object obj, int hashPrefix)
    {
      BinarySerialization.fieldHashes.Clear();
      FieldInfo[] allFields = TaskUtility.GetAllFields(obj.GetType());
      for (int index = 0; index < allFields.Length; ++index)
      {
        if (!BehaviorDesignerUtility.HasAttribute(allFields[index], typeof (NonSerializedAttribute)) && (!allFields[index].IsPrivate && !allFields[index].IsFamily || BehaviorDesignerUtility.HasAttribute(allFields[index], typeof (SerializeField))) && (!(obj is ParentTask) || !allFields[index].Name.Equals("children")))
        {
          object objA = allFields[index].GetValue(obj);
          if (!object.ReferenceEquals(objA, (object) null))
            BinarySerialization.SaveField(allFields[index].FieldType, allFields[index].Name, hashPrefix, objA, allFields[index]);
        }
      }
    }

    private static void SaveField(
      Type fieldType,
      string fieldName,
      int hashPrefix,
      object value,
      FieldInfo fieldInfo = null)
    {
      int hashPrefix1 = hashPrefix + BinaryDeserialization.StringHash(fieldType.Name.ToString(), true) + BinaryDeserialization.StringHash(fieldName, true);
      if (BinarySerialization.fieldHashes.Contains(hashPrefix1))
        return;
      BinarySerialization.fieldHashes.Add(hashPrefix1);
      ((List<int>) BinarySerialization.fieldSerializationData.fieldNameHash).Add(hashPrefix1);
      ((List<int>) BinarySerialization.fieldSerializationData.startIndex).Add(BinarySerialization.fieldIndex);
      if (typeof (IList).IsAssignableFrom(fieldType))
      {
        Type fieldType1;
        if (fieldType.IsArray)
        {
          fieldType1 = fieldType.GetElementType();
        }
        else
        {
          Type type = fieldType;
          while (!type.IsGenericType)
            type = type.BaseType;
          fieldType1 = type.GetGenericArguments()[0];
        }
        IList list = value as IList;
        if (list == null)
        {
          BinarySerialization.AddByteData((ICollection<byte>) BinarySerialization.IntToBytes(0));
        }
        else
        {
          BinarySerialization.AddByteData((ICollection<byte>) BinarySerialization.IntToBytes(list.Count));
          if (list.Count <= 0)
            return;
          for (int index = 0; index < list.Count; ++index)
          {
            if (object.ReferenceEquals(list[index], (object) null))
              BinarySerialization.AddByteData((ICollection<byte>) BinarySerialization.IntToBytes(-1));
            else
              BinarySerialization.SaveField(fieldType1, index.ToString(), hashPrefix1 / (index + 1), list[index], fieldInfo);
          }
        }
      }
      else if (typeof (Task).IsAssignableFrom(fieldType))
      {
        if (fieldInfo != null && BehaviorDesignerUtility.HasAttribute(fieldInfo, typeof (InspectTaskAttribute)))
        {
          BinarySerialization.AddByteData((ICollection<byte>) BinarySerialization.StringToBytes(value.GetType().ToString()));
          BinarySerialization.SaveFields(value, hashPrefix1);
        }
        else
          BinarySerialization.AddByteData((ICollection<byte>) BinarySerialization.IntToBytes((value as Task).get_ID()));
      }
      else if (typeof (SharedVariable).IsAssignableFrom(fieldType))
        BinarySerialization.SaveSharedVariable(value as SharedVariable, hashPrefix1);
      else if (typeof (Object).IsAssignableFrom(fieldType))
      {
        BinarySerialization.AddByteData((ICollection<byte>) BinarySerialization.IntToBytes(((List<Object>) BinarySerialization.fieldSerializationData.unityObjects).Count));
        ((List<Object>) BinarySerialization.fieldSerializationData.unityObjects).Add(value as Object);
      }
      else if (fieldType.Equals(typeof (int)) || fieldType.IsEnum)
        BinarySerialization.AddByteData((ICollection<byte>) BinarySerialization.IntToBytes((int) value));
      else if (fieldType.Equals(typeof (short)))
        BinarySerialization.AddByteData((ICollection<byte>) BinarySerialization.Int16ToBytes((short) value));
      else if (fieldType.Equals(typeof (uint)))
        BinarySerialization.AddByteData((ICollection<byte>) BinarySerialization.UIntToBytes((uint) value));
      else if (fieldType.Equals(typeof (float)))
        BinarySerialization.AddByteData((ICollection<byte>) BinarySerialization.FloatToBytes((float) value));
      else if (fieldType.Equals(typeof (double)))
        BinarySerialization.AddByteData((ICollection<byte>) BinarySerialization.DoubleToBytes((double) value));
      else if (fieldType.Equals(typeof (long)))
        BinarySerialization.AddByteData((ICollection<byte>) BinarySerialization.LongToBytes((long) value));
      else if (fieldType.Equals(typeof (bool)))
        BinarySerialization.AddByteData((ICollection<byte>) BinarySerialization.BoolToBytes((bool) value));
      else if (fieldType.Equals(typeof (string)))
        BinarySerialization.AddByteData((ICollection<byte>) BinarySerialization.StringToBytes((string) value));
      else if (fieldType.Equals(typeof (byte)))
        BinarySerialization.AddByteData((ICollection<byte>) BinarySerialization.ByteToBytes((byte) value));
      else if (fieldType.Equals(typeof (Vector2)))
        BinarySerialization.AddByteData(BinarySerialization.Vector2ToBytes((Vector2) value));
      else if (fieldType.Equals(typeof (Vector3)))
        BinarySerialization.AddByteData(BinarySerialization.Vector3ToBytes((Vector3) value));
      else if (fieldType.Equals(typeof (Vector4)))
        BinarySerialization.AddByteData(BinarySerialization.Vector4ToBytes((Vector4) value));
      else if (fieldType.Equals(typeof (Quaternion)))
        BinarySerialization.AddByteData(BinarySerialization.QuaternionToBytes((Quaternion) value));
      else if (fieldType.Equals(typeof (Color)))
        BinarySerialization.AddByteData(BinarySerialization.ColorToBytes((Color) value));
      else if (fieldType.Equals(typeof (Rect)))
        BinarySerialization.AddByteData(BinarySerialization.RectToBytes((Rect) value));
      else if (fieldType.Equals(typeof (Matrix4x4)))
        BinarySerialization.AddByteData(BinarySerialization.Matrix4x4ToBytes((Matrix4x4) value));
      else if (fieldType.Equals(typeof (LayerMask)))
      {
        LayerMask layerMask = (LayerMask) value;
        BinarySerialization.AddByteData((ICollection<byte>) BinarySerialization.IntToBytes(((LayerMask) ref layerMask).get_value()));
      }
      else if (fieldType.Equals(typeof (AnimationCurve)))
        BinarySerialization.AddByteData(BinarySerialization.AnimationCurveToBytes((AnimationCurve) value));
      else if (fieldType.IsClass || fieldType.IsValueType && !fieldType.IsPrimitive)
      {
        if (object.ReferenceEquals(value, (object) null))
          value = Activator.CreateInstance(fieldType, true);
        BinarySerialization.SaveFields(value, hashPrefix1);
      }
      else
        Debug.LogError((object) ("Missing Serialization for " + (object) fieldType));
    }

    private static byte[] IntToBytes(int value)
    {
      return BitConverter.GetBytes(value);
    }

    private static byte[] Int16ToBytes(short value)
    {
      return BitConverter.GetBytes(value);
    }

    private static byte[] UIntToBytes(uint value)
    {
      return BitConverter.GetBytes(value);
    }

    private static byte[] FloatToBytes(float value)
    {
      return BitConverter.GetBytes(value);
    }

    private static byte[] DoubleToBytes(double value)
    {
      return BitConverter.GetBytes(value);
    }

    private static byte[] LongToBytes(long value)
    {
      return BitConverter.GetBytes(value);
    }

    private static byte[] BoolToBytes(bool value)
    {
      return BitConverter.GetBytes(value);
    }

    private static byte[] StringToBytes(string str)
    {
      if (str == null)
        str = string.Empty;
      return Encoding.UTF8.GetBytes(str);
    }

    private static byte[] ByteToBytes(byte value)
    {
      return new byte[1]{ value };
    }

    private static ICollection<byte> ColorToBytes(Color color)
    {
      List<byte> byteList = new List<byte>();
      byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes((float) color.r));
      byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes((float) color.g));
      byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes((float) color.b));
      byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes((float) color.a));
      return (ICollection<byte>) byteList;
    }

    private static ICollection<byte> Vector2ToBytes(Vector2 vector2)
    {
      List<byte> byteList = new List<byte>();
      byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes((float) vector2.x));
      byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes((float) vector2.y));
      return (ICollection<byte>) byteList;
    }

    private static ICollection<byte> Vector3ToBytes(Vector3 vector3)
    {
      List<byte> byteList = new List<byte>();
      byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes((float) vector3.x));
      byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes((float) vector3.y));
      byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes((float) vector3.z));
      return (ICollection<byte>) byteList;
    }

    private static ICollection<byte> Vector4ToBytes(Vector4 vector4)
    {
      List<byte> byteList = new List<byte>();
      byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes((float) vector4.x));
      byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes((float) vector4.y));
      byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes((float) vector4.z));
      byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes((float) vector4.w));
      return (ICollection<byte>) byteList;
    }

    private static ICollection<byte> QuaternionToBytes(Quaternion quaternion)
    {
      List<byte> byteList = new List<byte>();
      byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes((float) quaternion.x));
      byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes((float) quaternion.y));
      byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes((float) quaternion.z));
      byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes((float) quaternion.w));
      return (ICollection<byte>) byteList;
    }

    private static ICollection<byte> RectToBytes(Rect rect)
    {
      List<byte> byteList = new List<byte>();
      byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes(((Rect) ref rect).get_x()));
      byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes(((Rect) ref rect).get_y()));
      byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes(((Rect) ref rect).get_width()));
      byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes(((Rect) ref rect).get_height()));
      return (ICollection<byte>) byteList;
    }

    private static ICollection<byte> Matrix4x4ToBytes(Matrix4x4 matrix4x4)
    {
      List<byte> byteList = new List<byte>();
      byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes((float) matrix4x4.m00));
      byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes((float) matrix4x4.m01));
      byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes((float) matrix4x4.m02));
      byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes((float) matrix4x4.m03));
      byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes((float) matrix4x4.m10));
      byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes((float) matrix4x4.m11));
      byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes((float) matrix4x4.m12));
      byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes((float) matrix4x4.m13));
      byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes((float) matrix4x4.m20));
      byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes((float) matrix4x4.m21));
      byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes((float) matrix4x4.m22));
      byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes((float) matrix4x4.m23));
      byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes((float) matrix4x4.m30));
      byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes((float) matrix4x4.m31));
      byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes((float) matrix4x4.m32));
      byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes((float) matrix4x4.m33));
      return (ICollection<byte>) byteList;
    }

    private static ICollection<byte> AnimationCurveToBytes(AnimationCurve animationCurve)
    {
      List<byte> byteList = new List<byte>();
      Keyframe[] keys = animationCurve.get_keys();
      if (keys != null)
      {
        byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes(keys.Length));
        for (int index = 0; index < keys.Length; ++index)
        {
          byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes(((Keyframe) ref keys[index]).get_time()));
          byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes(((Keyframe) ref keys[index]).get_value()));
          byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes(((Keyframe) ref keys[index]).get_inTangent()));
          byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes(((Keyframe) ref keys[index]).get_outTangent()));
          byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes(((Keyframe) ref keys[index]).get_tangentMode()));
        }
      }
      else
        byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes(0));
      byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes((int) animationCurve.get_preWrapMode()));
      byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes((int) animationCurve.get_postWrapMode()));
      return (ICollection<byte>) byteList;
    }

    private static void AddByteData(ICollection<byte> bytes)
    {
      ((List<int>) BinarySerialization.fieldSerializationData.dataPosition).Add(((List<byte>) BinarySerialization.fieldSerializationData.byteData).Count);
      if (bytes != null)
        ((List<byte>) BinarySerialization.fieldSerializationData.byteData).AddRange((IEnumerable<byte>) bytes);
      ++BinarySerialization.fieldIndex;
    }
  }
}
