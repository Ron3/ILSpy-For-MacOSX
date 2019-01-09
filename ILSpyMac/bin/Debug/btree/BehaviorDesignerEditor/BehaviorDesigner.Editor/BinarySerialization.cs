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
		private static int fieldIndex;

		private static TaskSerializationData taskSerializationData;

		private static FieldSerializationData fieldSerializationData;

		private static HashSet<int> fieldHashes = new HashSet<int>();

		public static void Save(BehaviorSource behaviorSource)
		{
			BinarySerialization.fieldIndex = 0;
			BinarySerialization.taskSerializationData = new TaskSerializationData();
			BinarySerialization.fieldSerializationData = BinarySerialization.taskSerializationData.fieldSerializationData;
			if (behaviorSource.get_Variables() != null)
			{
				for (int i = 0; i < behaviorSource.get_Variables().get_Count(); i++)
				{
					BinarySerialization.taskSerializationData.variableStartIndex.Add(BinarySerialization.fieldSerializationData.startIndex.get_Count());
					BinarySerialization.SaveSharedVariable(behaviorSource.get_Variables().get_Item(i), 0);
				}
			}
			if (!object.ReferenceEquals(behaviorSource.get_EntryTask(), null))
			{
				BinarySerialization.SaveTask(behaviorSource.get_EntryTask(), -1);
			}
			if (!object.ReferenceEquals(behaviorSource.get_RootTask(), null))
			{
				BinarySerialization.SaveTask(behaviorSource.get_RootTask(), 0);
			}
			if (behaviorSource.get_DetachedTasks() != null)
			{
				for (int j = 0; j < behaviorSource.get_DetachedTasks().get_Count(); j++)
				{
					BinarySerialization.SaveTask(behaviorSource.get_DetachedTasks().get_Item(j), -1);
				}
			}
			BinarySerialization.taskSerializationData.Version = "1.5.11";
			behaviorSource.set_TaskData(BinarySerialization.taskSerializationData);
			if (behaviorSource.get_Owner() != null && !behaviorSource.get_Owner().Equals(null))
			{
				BehaviorDesignerUtility.SetObjectDirty(behaviorSource.get_Owner().GetObject());
			}
		}

		public static void Save(GlobalVariables globalVariables)
		{
			if (globalVariables == null)
			{
				return;
			}
			BinarySerialization.fieldIndex = 0;
			globalVariables.set_VariableData(new VariableSerializationData());
			if (globalVariables.get_Variables() == null || globalVariables.get_Variables().get_Count() == 0)
			{
				return;
			}
			BinarySerialization.fieldSerializationData = globalVariables.get_VariableData().fieldSerializationData;
			for (int i = 0; i < globalVariables.get_Variables().get_Count(); i++)
			{
				globalVariables.get_VariableData().variableStartIndex.Add(BinarySerialization.fieldSerializationData.startIndex.get_Count());
				BinarySerialization.SaveSharedVariable(globalVariables.get_Variables().get_Item(i), 0);
			}
			globalVariables.set_Version("1.5.11");
			BehaviorDesignerUtility.SetObjectDirty(globalVariables);
		}

		private static void SaveTask(Task task, int parentTaskIndex)
		{
			BinarySerialization.taskSerializationData.types.Add(task.GetType().ToString());
			BinarySerialization.taskSerializationData.parentIndex.Add(parentTaskIndex);
			BinarySerialization.taskSerializationData.startIndex.Add(BinarySerialization.fieldSerializationData.startIndex.get_Count());
			BinarySerialization.SaveField(typeof(int), "ID", 0, task.get_ID(), null);
			BinarySerialization.SaveField(typeof(string), "FriendlyName", 0, task.get_FriendlyName(), null);
			BinarySerialization.SaveField(typeof(bool), "IsInstant", 0, task.get_IsInstant(), null);
			BinarySerialization.SaveField(typeof(bool), "Disabled", 0, task.get_Disabled(), null);
			BinarySerialization.SaveNodeData(task.get_NodeData());
			BinarySerialization.SaveFields(task, 0);
			if (task is ParentTask)
			{
				ParentTask parentTask = task as ParentTask;
				if (parentTask.get_Children() != null && parentTask.get_Children().get_Count() > 0)
				{
					for (int i = 0; i < parentTask.get_Children().get_Count(); i++)
					{
						BinarySerialization.SaveTask(parentTask.get_Children().get_Item(i), parentTask.get_ID());
					}
				}
			}
		}

		private static void SaveNodeData(NodeData nodeData)
		{
			BinarySerialization.SaveField(typeof(Vector2), "NodeDataOffset", 0, nodeData.get_Offset(), null);
			BinarySerialization.SaveField(typeof(string), "NodeDataComment", 0, nodeData.get_Comment(), null);
			BinarySerialization.SaveField(typeof(bool), "NodeDataIsBreakpoint", 0, nodeData.get_IsBreakpoint(), null);
			BinarySerialization.SaveField(typeof(bool), "NodeDataCollapsed", 0, nodeData.get_Collapsed(), null);
			BinarySerialization.SaveField(typeof(int), "NodeDataColorIndex", 0, nodeData.get_ColorIndex(), null);
			BinarySerialization.SaveField(typeof(List<string>), "NodeDataWatchedFields", 0, nodeData.get_WatchedFieldNames(), null);
		}

		private static void SaveSharedVariable(SharedVariable sharedVariable, int hashPrefix)
		{
			if (sharedVariable == null)
			{
				return;
			}
			BinarySerialization.SaveField(typeof(string), "Type", hashPrefix, sharedVariable.GetType().ToString(), null);
			BinarySerialization.SaveField(typeof(string), "Name", hashPrefix, sharedVariable.get_Name(), null);
			if (sharedVariable.get_IsShared())
			{
				BinarySerialization.SaveField(typeof(bool), "IsShared", hashPrefix, sharedVariable.get_IsShared(), null);
			}
			if (sharedVariable.get_IsGlobal())
			{
				BinarySerialization.SaveField(typeof(bool), "IsGlobal", hashPrefix, sharedVariable.get_IsGlobal(), null);
			}
			if (sharedVariable.get_NetworkSync())
			{
				BinarySerialization.SaveField(typeof(bool), "NetworkSync", hashPrefix, sharedVariable.get_NetworkSync(), null);
			}
			if (!string.IsNullOrEmpty(sharedVariable.get_PropertyMapping()))
			{
				BinarySerialization.SaveField(typeof(string), "PropertyMapping", hashPrefix, sharedVariable.get_PropertyMapping(), null);
				if (!object.Equals(sharedVariable.get_PropertyMappingOwner(), null))
				{
					BinarySerialization.SaveField(typeof(GameObject), "PropertyMappingOwner", hashPrefix, sharedVariable.get_PropertyMappingOwner(), null);
				}
			}
			BinarySerialization.SaveFields(sharedVariable, hashPrefix);
		}

		private static void SaveFields(object obj, int hashPrefix)
		{
			BinarySerialization.fieldHashes.Clear();
			FieldInfo[] allFields = TaskUtility.GetAllFields(obj.GetType());
			for (int i = 0; i < allFields.Length; i++)
			{
				if (!BehaviorDesignerUtility.HasAttribute(allFields[i], typeof(NonSerializedAttribute)) && ((!allFields[i].get_IsPrivate() && !allFields[i].get_IsFamily()) || BehaviorDesignerUtility.HasAttribute(allFields[i], typeof(SerializeField))) && (!(obj is ParentTask) || !allFields[i].get_Name().Equals("children")))
				{
					object value = allFields[i].GetValue(obj);
					if (!object.ReferenceEquals(value, null))
					{
						BinarySerialization.SaveField(allFields[i].get_FieldType(), allFields[i].get_Name(), hashPrefix, value, allFields[i]);
					}
				}
			}
		}

		private static void SaveField(Type fieldType, string fieldName, int hashPrefix, object value, FieldInfo fieldInfo = null)
		{
			int num = hashPrefix + BinaryDeserialization.StringHash(fieldType.get_Name().ToString(), true) + BinaryDeserialization.StringHash(fieldName, true);
			if (BinarySerialization.fieldHashes.Contains(num))
			{
				return;
			}
			BinarySerialization.fieldHashes.Add(num);
			BinarySerialization.fieldSerializationData.fieldNameHash.Add(num);
			BinarySerialization.fieldSerializationData.startIndex.Add(BinarySerialization.fieldIndex);
			if (typeof(IList).IsAssignableFrom(fieldType))
			{
				Type fieldType2;
				if (fieldType.get_IsArray())
				{
					fieldType2 = fieldType.GetElementType();
				}
				else
				{
					Type type = fieldType;
					while (!type.get_IsGenericType())
					{
						type = type.get_BaseType();
					}
					fieldType2 = type.GetGenericArguments()[0];
				}
				IList list = value as IList;
				if (list == null)
				{
					BinarySerialization.AddByteData(BinarySerialization.IntToBytes(0));
				}
				else
				{
					BinarySerialization.AddByteData(BinarySerialization.IntToBytes(list.get_Count()));
					if (list.get_Count() > 0)
					{
						for (int i = 0; i < list.get_Count(); i++)
						{
							if (object.ReferenceEquals(list.get_Item(i), null))
							{
								BinarySerialization.AddByteData(BinarySerialization.IntToBytes(-1));
							}
							else
							{
								BinarySerialization.SaveField(fieldType2, i.ToString(), num / (i + 1), list.get_Item(i), fieldInfo);
							}
						}
					}
				}
			}
			else if (typeof(Task).IsAssignableFrom(fieldType))
			{
				if (fieldInfo != null && BehaviorDesignerUtility.HasAttribute(fieldInfo, typeof(InspectTaskAttribute)))
				{
					BinarySerialization.AddByteData(BinarySerialization.StringToBytes(value.GetType().ToString()));
					BinarySerialization.SaveFields(value, num);
				}
				else
				{
					BinarySerialization.AddByteData(BinarySerialization.IntToBytes((value as Task).get_ID()));
				}
			}
			else if (typeof(SharedVariable).IsAssignableFrom(fieldType))
			{
				BinarySerialization.SaveSharedVariable(value as SharedVariable, num);
			}
			else if (typeof(Object).IsAssignableFrom(fieldType))
			{
				BinarySerialization.AddByteData(BinarySerialization.IntToBytes(BinarySerialization.fieldSerializationData.unityObjects.get_Count()));
				BinarySerialization.fieldSerializationData.unityObjects.Add(value as Object);
			}
			else if (fieldType.Equals(typeof(int)) || fieldType.get_IsEnum())
			{
				BinarySerialization.AddByteData(BinarySerialization.IntToBytes((int)value));
			}
			else if (fieldType.Equals(typeof(short)))
			{
				BinarySerialization.AddByteData(BinarySerialization.Int16ToBytes((short)value));
			}
			else if (fieldType.Equals(typeof(uint)))
			{
				BinarySerialization.AddByteData(BinarySerialization.UIntToBytes((uint)value));
			}
			else if (fieldType.Equals(typeof(float)))
			{
				BinarySerialization.AddByteData(BinarySerialization.FloatToBytes((float)value));
			}
			else if (fieldType.Equals(typeof(double)))
			{
				BinarySerialization.AddByteData(BinarySerialization.DoubleToBytes((double)value));
			}
			else if (fieldType.Equals(typeof(long)))
			{
				BinarySerialization.AddByteData(BinarySerialization.LongToBytes((long)value));
			}
			else if (fieldType.Equals(typeof(bool)))
			{
				BinarySerialization.AddByteData(BinarySerialization.BoolToBytes((bool)value));
			}
			else if (fieldType.Equals(typeof(string)))
			{
				BinarySerialization.AddByteData(BinarySerialization.StringToBytes((string)value));
			}
			else if (fieldType.Equals(typeof(byte)))
			{
				BinarySerialization.AddByteData(BinarySerialization.ByteToBytes((byte)value));
			}
			else if (fieldType.Equals(typeof(Vector2)))
			{
				BinarySerialization.AddByteData(BinarySerialization.Vector2ToBytes((Vector2)value));
			}
			else if (fieldType.Equals(typeof(Vector3)))
			{
				BinarySerialization.AddByteData(BinarySerialization.Vector3ToBytes((Vector3)value));
			}
			else if (fieldType.Equals(typeof(Vector4)))
			{
				BinarySerialization.AddByteData(BinarySerialization.Vector4ToBytes((Vector4)value));
			}
			else if (fieldType.Equals(typeof(Quaternion)))
			{
				BinarySerialization.AddByteData(BinarySerialization.QuaternionToBytes((Quaternion)value));
			}
			else if (fieldType.Equals(typeof(Color)))
			{
				BinarySerialization.AddByteData(BinarySerialization.ColorToBytes((Color)value));
			}
			else if (fieldType.Equals(typeof(Rect)))
			{
				BinarySerialization.AddByteData(BinarySerialization.RectToBytes((Rect)value));
			}
			else if (fieldType.Equals(typeof(Matrix4x4)))
			{
				BinarySerialization.AddByteData(BinarySerialization.Matrix4x4ToBytes((Matrix4x4)value));
			}
			else if (fieldType.Equals(typeof(LayerMask)))
			{
				BinarySerialization.AddByteData(BinarySerialization.IntToBytes(((LayerMask)value).get_value()));
			}
			else if (fieldType.Equals(typeof(AnimationCurve)))
			{
				BinarySerialization.AddByteData(BinarySerialization.AnimationCurveToBytes((AnimationCurve)value));
			}
			else if (fieldType.get_IsClass() || (fieldType.get_IsValueType() && !fieldType.get_IsPrimitive()))
			{
				if (object.ReferenceEquals(value, null))
				{
					value = Activator.CreateInstance(fieldType, true);
				}
				BinarySerialization.SaveFields(value, num);
			}
			else
			{
				Debug.LogError("Missing Serialization for " + fieldType);
			}
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
			{
				str = string.Empty;
			}
			return Encoding.get_UTF8().GetBytes(str);
		}

		private static byte[] ByteToBytes(byte value)
		{
			return new byte[]
			{
				value
			};
		}

		private static ICollection<byte> ColorToBytes(Color color)
		{
			List<byte> list = new List<byte>();
			list.AddRange(BitConverter.GetBytes(color.r));
			list.AddRange(BitConverter.GetBytes(color.g));
			list.AddRange(BitConverter.GetBytes(color.b));
			list.AddRange(BitConverter.GetBytes(color.a));
			return list;
		}

		private static ICollection<byte> Vector2ToBytes(Vector2 vector2)
		{
			List<byte> list = new List<byte>();
			list.AddRange(BitConverter.GetBytes(vector2.x));
			list.AddRange(BitConverter.GetBytes(vector2.y));
			return list;
		}

		private static ICollection<byte> Vector3ToBytes(Vector3 vector3)
		{
			List<byte> list = new List<byte>();
			list.AddRange(BitConverter.GetBytes(vector3.x));
			list.AddRange(BitConverter.GetBytes(vector3.y));
			list.AddRange(BitConverter.GetBytes(vector3.z));
			return list;
		}

		private static ICollection<byte> Vector4ToBytes(Vector4 vector4)
		{
			List<byte> list = new List<byte>();
			list.AddRange(BitConverter.GetBytes(vector4.x));
			list.AddRange(BitConverter.GetBytes(vector4.y));
			list.AddRange(BitConverter.GetBytes(vector4.z));
			list.AddRange(BitConverter.GetBytes(vector4.w));
			return list;
		}

		private static ICollection<byte> QuaternionToBytes(Quaternion quaternion)
		{
			List<byte> list = new List<byte>();
			list.AddRange(BitConverter.GetBytes(quaternion.x));
			list.AddRange(BitConverter.GetBytes(quaternion.y));
			list.AddRange(BitConverter.GetBytes(quaternion.z));
			list.AddRange(BitConverter.GetBytes(quaternion.w));
			return list;
		}

		private static ICollection<byte> RectToBytes(Rect rect)
		{
			List<byte> list = new List<byte>();
			list.AddRange(BitConverter.GetBytes(rect.get_x()));
			list.AddRange(BitConverter.GetBytes(rect.get_y()));
			list.AddRange(BitConverter.GetBytes(rect.get_width()));
			list.AddRange(BitConverter.GetBytes(rect.get_height()));
			return list;
		}

		private static ICollection<byte> Matrix4x4ToBytes(Matrix4x4 matrix4x4)
		{
			List<byte> list = new List<byte>();
			list.AddRange(BitConverter.GetBytes(matrix4x4.m00));
			list.AddRange(BitConverter.GetBytes(matrix4x4.m01));
			list.AddRange(BitConverter.GetBytes(matrix4x4.m02));
			list.AddRange(BitConverter.GetBytes(matrix4x4.m03));
			list.AddRange(BitConverter.GetBytes(matrix4x4.m10));
			list.AddRange(BitConverter.GetBytes(matrix4x4.m11));
			list.AddRange(BitConverter.GetBytes(matrix4x4.m12));
			list.AddRange(BitConverter.GetBytes(matrix4x4.m13));
			list.AddRange(BitConverter.GetBytes(matrix4x4.m20));
			list.AddRange(BitConverter.GetBytes(matrix4x4.m21));
			list.AddRange(BitConverter.GetBytes(matrix4x4.m22));
			list.AddRange(BitConverter.GetBytes(matrix4x4.m23));
			list.AddRange(BitConverter.GetBytes(matrix4x4.m30));
			list.AddRange(BitConverter.GetBytes(matrix4x4.m31));
			list.AddRange(BitConverter.GetBytes(matrix4x4.m32));
			list.AddRange(BitConverter.GetBytes(matrix4x4.m33));
			return list;
		}

		private static ICollection<byte> AnimationCurveToBytes(AnimationCurve animationCurve)
		{
			List<byte> list = new List<byte>();
			Keyframe[] keys = animationCurve.get_keys();
			if (keys != null)
			{
				list.AddRange(BitConverter.GetBytes(keys.Length));
				for (int i = 0; i < keys.Length; i++)
				{
					list.AddRange(BitConverter.GetBytes(keys[i].get_time()));
					list.AddRange(BitConverter.GetBytes(keys[i].get_value()));
					list.AddRange(BitConverter.GetBytes(keys[i].get_inTangent()));
					list.AddRange(BitConverter.GetBytes(keys[i].get_outTangent()));
					list.AddRange(BitConverter.GetBytes(keys[i].get_tangentMode()));
				}
			}
			else
			{
				list.AddRange(BitConverter.GetBytes(0));
			}
			list.AddRange(BitConverter.GetBytes(animationCurve.get_preWrapMode()));
			list.AddRange(BitConverter.GetBytes(animationCurve.get_postWrapMode()));
			return list;
		}

		private static void AddByteData(ICollection<byte> bytes)
		{
			BinarySerialization.fieldSerializationData.dataPosition.Add(BinarySerialization.fieldSerializationData.byteData.get_Count());
			if (bytes != null)
			{
				BinarySerialization.fieldSerializationData.byteData.AddRange(bytes);
			}
			BinarySerialization.fieldIndex++;
		}
	}
}