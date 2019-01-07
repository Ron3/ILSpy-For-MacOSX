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
	[Serializable]
	public class GraphDesigner : ScriptableObject
	{
		private NodeDesigner mEntryNode;

		private NodeDesigner mRootNode;

		private List<NodeDesigner> mDetachedNodes = new List<NodeDesigner>();

		[SerializeField]
		private List<NodeDesigner> mSelectedNodes = new List<NodeDesigner>();

		private NodeDesigner mHoverNode;

		private NodeConnection mActiveNodeConnection;

		[SerializeField]
		private List<NodeConnection> mSelectedNodeConnections = new List<NodeConnection>();

		[SerializeField]
		private int mNextTaskID;

		private List<int> mNodeSelectedID = new List<int>();

		[SerializeField]
		private int[] mPrevNodeSelectedID;

		public NodeDesigner RootNode
		{
			get
			{
				return this.mRootNode;
			}
		}

		public List<NodeDesigner> DetachedNodes
		{
			get
			{
				return this.mDetachedNodes;
			}
		}

		public List<NodeDesigner> SelectedNodes
		{
			get
			{
				return this.mSelectedNodes;
			}
		}

		public NodeDesigner HoverNode
		{
			get
			{
				return this.mHoverNode;
			}
			set
			{
				this.mHoverNode = value;
			}
		}

		public NodeConnection ActiveNodeConnection
		{
			get
			{
				return this.mActiveNodeConnection;
			}
			set
			{
				this.mActiveNodeConnection = value;
			}
		}

		public List<NodeConnection> SelectedNodeConnections
		{
			get
			{
				return this.mSelectedNodeConnections;
			}
		}

		public void OnEnable()
		{
			base.set_hideFlags(61);
		}

		public NodeDesigner AddNode(BehaviorSource behaviorSource, Type type, Vector2 position)
		{
			Task task = Activator.CreateInstance(type, true) as Task;
			if (task == null)
			{
				EditorUtility.DisplayDialog("Unable to Add Task", string.Format("Unable to create task of type {0}. Is the class name the same as the file name?", type), "OK");
				return null;
			}
			return this.AddNode(behaviorSource, task, position);
		}

		private NodeDesigner AddNode(BehaviorSource behaviorSource, Task task, Vector2 position)
		{
			if (this.mEntryNode == null)
			{
				Task task2 = Activator.CreateInstance(TaskUtility.GetTypeWithinAssembly("BehaviorDesigner.Runtime.Tasks.EntryTask")) as Task;
				this.mEntryNode = ScriptableObject.CreateInstance<NodeDesigner>();
				this.mEntryNode.LoadNode(task2, behaviorSource, new Vector2(position.x, position.y - 120f), ref this.mNextTaskID);
				this.mEntryNode.MakeEntryDisplay();
			}
			NodeDesigner nodeDesigner = ScriptableObject.CreateInstance<NodeDesigner>();
			nodeDesigner.LoadNode(task, behaviorSource, position, ref this.mNextTaskID);
			TaskNameAttribute[] array;
			if ((array = (task.GetType().GetCustomAttributes(typeof(TaskNameAttribute), false) as TaskNameAttribute[])).Length > 0)
			{
				task.set_FriendlyName(array[0].get_Name());
			}
			if (this.mEntryNode.OutgoingNodeConnections.get_Count() == 0)
			{
				this.mActiveNodeConnection = ScriptableObject.CreateInstance<NodeConnection>();
				this.mActiveNodeConnection.LoadConnection(this.mEntryNode, NodeConnectionType.Outgoing);
				this.ConnectNodes(behaviorSource, nodeDesigner);
			}
			else
			{
				this.mDetachedNodes.Add(nodeDesigner);
			}
			return nodeDesigner;
		}

		public NodeDesigner NodeAt(Vector2 point, Vector2 offset)
		{
			if (this.mEntryNode == null)
			{
				return null;
			}
			for (int i = 0; i < this.mSelectedNodes.get_Count(); i++)
			{
				if (this.mSelectedNodes.get_Item(i).Contains(point, offset, false))
				{
					return this.mSelectedNodes.get_Item(i);
				}
			}
			NodeDesigner result;
			for (int j = this.mDetachedNodes.get_Count() - 1; j > -1; j--)
			{
				if (this.mDetachedNodes.get_Item(j) != null && (result = this.NodeChildrenAt(this.mDetachedNodes.get_Item(j), point, offset)) != null)
				{
					return result;
				}
			}
			if (this.mRootNode != null && (result = this.NodeChildrenAt(this.mRootNode, point, offset)) != null)
			{
				return result;
			}
			if (this.mEntryNode.Contains(point, offset, true))
			{
				return this.mEntryNode;
			}
			return null;
		}

		private NodeDesigner NodeChildrenAt(NodeDesigner nodeDesigner, Vector2 point, Vector2 offset)
		{
			if (nodeDesigner.Contains(point, offset, true))
			{
				return nodeDesigner;
			}
			if (nodeDesigner.IsParent)
			{
				ParentTask parentTask = nodeDesigner.Task as ParentTask;
				if (!parentTask.get_NodeData().get_Collapsed() && parentTask.get_Children() != null)
				{
					for (int i = 0; i < parentTask.get_Children().get_Count(); i++)
					{
						NodeDesigner result;
						if (parentTask.get_Children().get_Item(i) != null && (result = this.NodeChildrenAt(parentTask.get_Children().get_Item(i).get_NodeData().get_NodeDesigner() as NodeDesigner, point, offset)) != null)
						{
							return result;
						}
					}
				}
			}
			return null;
		}

		public List<NodeDesigner> NodesAt(Rect rect, Vector2 offset)
		{
			List<NodeDesigner> list = new List<NodeDesigner>();
			if (this.mRootNode != null)
			{
				this.NodesChildrenAt(this.mRootNode, rect, offset, ref list);
			}
			for (int i = 0; i < this.mDetachedNodes.get_Count(); i++)
			{
				this.NodesChildrenAt(this.mDetachedNodes.get_Item(i), rect, offset, ref list);
			}
			return (list.get_Count() <= 0) ? null : list;
		}

		private void NodesChildrenAt(NodeDesigner nodeDesigner, Rect rect, Vector2 offset, ref List<NodeDesigner> nodes)
		{
			if (nodeDesigner.Intersects(rect, offset))
			{
				nodes.Add(nodeDesigner);
			}
			if (nodeDesigner.IsParent)
			{
				ParentTask parentTask = nodeDesigner.Task as ParentTask;
				if (!parentTask.get_NodeData().get_Collapsed() && parentTask.get_Children() != null)
				{
					for (int i = 0; i < parentTask.get_Children().get_Count(); i++)
					{
						if (parentTask.get_Children().get_Item(i) != null)
						{
							this.NodesChildrenAt(parentTask.get_Children().get_Item(i).get_NodeData().get_NodeDesigner() as NodeDesigner, rect, offset, ref nodes);
						}
					}
				}
			}
		}

		public bool IsSelected(NodeDesigner nodeDesigner)
		{
			return this.mSelectedNodes.Contains(nodeDesigner);
		}

		public bool IsParentSelected(NodeDesigner nodeDesigner)
		{
			return nodeDesigner.ParentNodeDesigner != null && (this.IsSelected(nodeDesigner.ParentNodeDesigner) || this.IsParentSelected(nodeDesigner.ParentNodeDesigner));
		}

		public void Select(NodeDesigner nodeDesigner)
		{
			this.Select(nodeDesigner, true);
		}

		public void Select(NodeDesigner nodeDesigner, bool addHash)
		{
			if (this.mSelectedNodes.Contains(nodeDesigner))
			{
				return;
			}
			if (this.mSelectedNodes.get_Count() == 1)
			{
				this.IndicateReferencedTasks(this.mSelectedNodes.get_Item(0).Task, false);
			}
			this.mSelectedNodes.Add(nodeDesigner);
			if (addHash)
			{
				this.mNodeSelectedID.Add(nodeDesigner.Task.get_ID());
			}
			nodeDesigner.Select();
			if (this.mSelectedNodes.get_Count() == 1)
			{
				this.IndicateReferencedTasks(this.mSelectedNodes.get_Item(0).Task, true);
			}
		}

		public void Deselect(NodeDesigner nodeDesigner)
		{
			this.mSelectedNodes.Remove(nodeDesigner);
			this.mNodeSelectedID.Remove(nodeDesigner.Task.get_ID());
			nodeDesigner.Deselect();
			this.IndicateReferencedTasks(nodeDesigner.Task, false);
		}

		public void DeselectAll(NodeDesigner exceptionNodeDesigner)
		{
			for (int i = this.mSelectedNodes.get_Count() - 1; i >= 0; i--)
			{
				if (exceptionNodeDesigner == null || !this.mSelectedNodes.get_Item(i).Equals(exceptionNodeDesigner))
				{
					this.mSelectedNodes.get_Item(i).Deselect();
					this.mSelectedNodes.RemoveAt(i);
					this.mNodeSelectedID.RemoveAt(i);
				}
			}
			if (exceptionNodeDesigner != null)
			{
				this.IndicateReferencedTasks(exceptionNodeDesigner.Task, false);
			}
		}

		public void ClearNodeSelection()
		{
			if (this.mSelectedNodes.get_Count() == 1)
			{
				this.IndicateReferencedTasks(this.mSelectedNodes.get_Item(0).Task, false);
			}
			for (int i = 0; i < this.mSelectedNodes.get_Count(); i++)
			{
				this.mSelectedNodes.get_Item(i).Deselect();
			}
			this.mSelectedNodes.Clear();
			this.mNodeSelectedID.Clear();
		}

		public void DeselectWithParent(NodeDesigner nodeDesigner)
		{
			for (int i = this.mSelectedNodes.get_Count() - 1; i >= 0; i--)
			{
				if (this.mSelectedNodes.get_Item(i).HasParent(nodeDesigner))
				{
					this.Deselect(this.mSelectedNodes.get_Item(i));
				}
			}
		}

		public bool ReplaceSelectedNode(BehaviorSource behaviorSource, Type taskType)
		{
			BehaviorUndo.RegisterUndo("Replace", behaviorSource.get_Owner().GetObject());
			Vector2 absolutePosition = this.SelectedNodes.get_Item(0).GetAbsolutePosition();
			NodeDesigner parentNodeDesigner = this.SelectedNodes.get_Item(0).ParentNodeDesigner;
			List<Task> list = (!this.SelectedNodes.get_Item(0).IsParent) ? null : (this.SelectedNodes.get_Item(0).Task as ParentTask).get_Children();
			UnknownTask unknownTask = this.SelectedNodes.get_Item(0).Task as UnknownTask;
			this.RemoveNode(this.SelectedNodes.get_Item(0));
			this.mSelectedNodes.Clear();
			TaskReferences.CheckReferences(behaviorSource);
			NodeDesigner nodeDesigner = null;
			if (unknownTask != null)
			{
				Task task = null;
				if (!string.IsNullOrEmpty(unknownTask.JSONSerialization))
				{
					Dictionary<int, Task> dictionary = new Dictionary<int, Task>();
					Dictionary<string, object> dictionary2 = MiniJSON.Deserialize(unknownTask.JSONSerialization) as Dictionary<string, object>;
					if (dictionary2.ContainsKey("Type"))
					{
						dictionary2.set_Item("Type", taskType.ToString());
					}
					task = JSONDeserialization.DeserializeTask(behaviorSource, dictionary2, ref dictionary, null);
				}
				else
				{
					TaskSerializationData taskSerializationData = new TaskSerializationData();
					taskSerializationData.types.Add(taskType.ToString());
					taskSerializationData.startIndex.Add(0);
					FieldSerializationData fieldSerializationData = new FieldSerializationData();
					fieldSerializationData.fieldNameHash = unknownTask.fieldNameHash;
					fieldSerializationData.startIndex = unknownTask.startIndex;
					fieldSerializationData.dataPosition = unknownTask.dataPosition;
					fieldSerializationData.unityObjects = unknownTask.unityObjects;
					fieldSerializationData.byteDataArray = unknownTask.byteData.ToArray();
					List<Task> list2 = new List<Task>();
					BinaryDeserialization.LoadTask(taskSerializationData, fieldSerializationData, ref list2, ref behaviorSource);
					if (list2.get_Count() > 0)
					{
						task = list2.get_Item(0);
					}
				}
				if (task != null)
				{
					nodeDesigner = this.AddNode(behaviorSource, task, absolutePosition);
				}
			}
			else
			{
				nodeDesigner = this.AddNode(behaviorSource, taskType, absolutePosition);
			}
			if (nodeDesigner == null)
			{
				return false;
			}
			if (parentNodeDesigner != null)
			{
				this.ActiveNodeConnection = parentNodeDesigner.CreateNodeConnection(false);
				this.ConnectNodes(behaviorSource, nodeDesigner);
			}
			if (nodeDesigner.IsParent && list != null)
			{
				for (int i = 0; i < list.get_Count(); i++)
				{
					this.ActiveNodeConnection = nodeDesigner.CreateNodeConnection(false);
					this.ConnectNodes(behaviorSource, list.get_Item(i).get_NodeData().get_NodeDesigner() as NodeDesigner);
					if (i >= (nodeDesigner.Task as ParentTask).MaxChildren())
					{
						break;
					}
				}
			}
			this.Select(nodeDesigner);
			return true;
		}

		public void Hover(NodeDesigner nodeDesigner)
		{
			if (!nodeDesigner.ShowHoverBar)
			{
				nodeDesigner.ShowHoverBar = true;
				this.HoverNode = nodeDesigner;
			}
		}

		public void ClearHover()
		{
			if (this.HoverNode)
			{
				this.HoverNode.ShowHoverBar = false;
				this.HoverNode = null;
			}
		}

		private void IndicateReferencedTasks(Task task, bool indicate)
		{
			List<Task> referencedTasks = TaskInspector.GetReferencedTasks(task);
			if (referencedTasks != null && referencedTasks.get_Count() > 0)
			{
				for (int i = 0; i < referencedTasks.get_Count(); i++)
				{
					if (referencedTasks.get_Item(i) != null && referencedTasks.get_Item(i).get_NodeData() != null)
					{
						NodeDesigner nodeDesigner = referencedTasks.get_Item(i).get_NodeData().get_NodeDesigner() as NodeDesigner;
						if (nodeDesigner != null)
						{
							nodeDesigner.ShowReferenceIcon = indicate;
						}
					}
				}
			}
		}

		public bool DragSelectedNodes(Vector2 delta, bool dragChildren)
		{
			if (this.mSelectedNodes.get_Count() == 0)
			{
				return false;
			}
			bool flag = this.mSelectedNodes.get_Count() == 1;
			for (int i = 0; i < this.mSelectedNodes.get_Count(); i++)
			{
				this.DragNode(this.mSelectedNodes.get_Item(i), delta, dragChildren);
			}
			if (flag && dragChildren && this.mSelectedNodes.get_Item(0).IsEntryDisplay && this.mRootNode != null)
			{
				this.DragNode(this.mRootNode, delta, dragChildren);
			}
			return true;
		}

		private void DragNode(NodeDesigner nodeDesigner, Vector2 delta, bool dragChildren)
		{
			if (this.IsParentSelected(nodeDesigner) && dragChildren)
			{
				return;
			}
			nodeDesigner.ChangeOffset(delta);
			if (nodeDesigner.ParentNodeDesigner != null)
			{
				int num = nodeDesigner.ParentNodeDesigner.ChildIndexForTask(nodeDesigner.Task);
				if (num != -1)
				{
					int index = num - 1;
					bool flag = false;
					NodeDesigner nodeDesigner2 = nodeDesigner.ParentNodeDesigner.NodeDesignerForChildIndex(index);
					if (nodeDesigner2 != null && nodeDesigner.Task.get_NodeData().get_Offset().x < nodeDesigner2.Task.get_NodeData().get_Offset().x)
					{
						nodeDesigner.ParentNodeDesigner.MoveChildNode(num, true);
						flag = true;
					}
					if (!flag)
					{
						index = num + 1;
						nodeDesigner2 = nodeDesigner.ParentNodeDesigner.NodeDesignerForChildIndex(index);
						if (nodeDesigner2 != null && nodeDesigner.Task.get_NodeData().get_Offset().x > nodeDesigner2.Task.get_NodeData().get_Offset().x)
						{
							nodeDesigner.ParentNodeDesigner.MoveChildNode(num, false);
						}
					}
				}
			}
			if (nodeDesigner.IsParent && !dragChildren)
			{
				ParentTask parentTask = nodeDesigner.Task as ParentTask;
				if (parentTask.get_Children() != null)
				{
					for (int i = 0; i < parentTask.get_Children().get_Count(); i++)
					{
						NodeDesigner nodeDesigner3 = parentTask.get_Children().get_Item(i).get_NodeData().get_NodeDesigner() as NodeDesigner;
						nodeDesigner3.ChangeOffset(-delta);
					}
				}
			}
			this.MarkNodeDirty(nodeDesigner);
		}

		public bool DrawNodes(Vector2 mousePosition, Vector2 offset)
		{
			if (this.mEntryNode == null)
			{
				return false;
			}
			this.mEntryNode.DrawNodeConnection(offset, false);
			if (this.mRootNode != null)
			{
				this.DrawNodeConnectionChildren(this.mRootNode, offset, this.mRootNode.Task.get_Disabled());
			}
			for (int i = 0; i < this.mDetachedNodes.get_Count(); i++)
			{
				this.DrawNodeConnectionChildren(this.mDetachedNodes.get_Item(i), offset, this.mDetachedNodes.get_Item(i).Task.get_Disabled());
			}
			for (int j = 0; j < this.mSelectedNodeConnections.get_Count(); j++)
			{
				this.mSelectedNodeConnections.get_Item(j).DrawConnection(offset, this.mSelectedNodeConnections.get_Item(j).OriginatingNodeDesigner.IsDisabled());
			}
			if (mousePosition != new Vector2(-1f, -1f) && this.mActiveNodeConnection != null)
			{
				this.mActiveNodeConnection.HorizontalHeight = (this.mActiveNodeConnection.OriginatingNodeDesigner.GetConnectionPosition(offset, this.mActiveNodeConnection.NodeConnectionType).y + mousePosition.y) / 2f;
				this.mActiveNodeConnection.DrawConnection(this.mActiveNodeConnection.OriginatingNodeDesigner.GetConnectionPosition(offset, this.mActiveNodeConnection.NodeConnectionType), mousePosition, this.mActiveNodeConnection.NodeConnectionType == NodeConnectionType.Outgoing && this.mActiveNodeConnection.OriginatingNodeDesigner.IsDisabled());
			}
			this.mEntryNode.DrawNode(offset, false, false);
			bool result = false;
			if (this.mRootNode != null && this.DrawNodeChildren(this.mRootNode, offset, this.mRootNode.Task.get_Disabled()))
			{
				result = true;
			}
			for (int k = 0; k < this.mDetachedNodes.get_Count(); k++)
			{
				if (this.DrawNodeChildren(this.mDetachedNodes.get_Item(k), offset, this.mDetachedNodes.get_Item(k).Task.get_Disabled()))
				{
					result = true;
				}
			}
			for (int l = 0; l < this.mSelectedNodes.get_Count(); l++)
			{
				if (this.mSelectedNodes.get_Item(l).DrawNode(offset, true, this.mSelectedNodes.get_Item(l).IsDisabled()))
				{
					result = true;
				}
			}
			if (this.mRootNode != null)
			{
				this.DrawNodeCommentChildren(this.mRootNode, offset);
			}
			for (int m = 0; m < this.mDetachedNodes.get_Count(); m++)
			{
				this.DrawNodeCommentChildren(this.mDetachedNodes.get_Item(m), offset);
			}
			return result;
		}

		private bool DrawNodeChildren(NodeDesigner nodeDesigner, Vector2 offset, bool disabledNode)
		{
			if (nodeDesigner == null)
			{
				return false;
			}
			bool result = false;
			if (nodeDesigner.DrawNode(offset, false, disabledNode))
			{
				result = true;
			}
			if (nodeDesigner.IsParent)
			{
				ParentTask parentTask = nodeDesigner.Task as ParentTask;
				if (!parentTask.get_NodeData().get_Collapsed() && parentTask.get_Children() != null)
				{
					for (int i = parentTask.get_Children().get_Count() - 1; i > -1; i--)
					{
						if (parentTask.get_Children().get_Item(i) != null && this.DrawNodeChildren(parentTask.get_Children().get_Item(i).get_NodeData().get_NodeDesigner() as NodeDesigner, offset, parentTask.get_Disabled() || disabledNode))
						{
							result = true;
						}
					}
				}
			}
			return result;
		}

		private void DrawNodeConnectionChildren(NodeDesigner nodeDesigner, Vector2 offset, bool disabledNode)
		{
			if (nodeDesigner == null)
			{
				return;
			}
			if (!nodeDesigner.Task.get_NodeData().get_Collapsed())
			{
				nodeDesigner.DrawNodeConnection(offset, nodeDesigner.Task.get_Disabled() || disabledNode);
				if (nodeDesigner.IsParent)
				{
					ParentTask parentTask = nodeDesigner.Task as ParentTask;
					if (parentTask.get_Children() != null)
					{
						for (int i = 0; i < parentTask.get_Children().get_Count(); i++)
						{
							if (parentTask.get_Children().get_Item(i) != null)
							{
								this.DrawNodeConnectionChildren(parentTask.get_Children().get_Item(i).get_NodeData().get_NodeDesigner() as NodeDesigner, offset, parentTask.get_Disabled() || disabledNode);
							}
						}
					}
				}
			}
		}

		private void DrawNodeCommentChildren(NodeDesigner nodeDesigner, Vector2 offset)
		{
			if (nodeDesigner == null)
			{
				return;
			}
			nodeDesigner.DrawNodeComment(offset);
			if (nodeDesigner.IsParent)
			{
				ParentTask parentTask = nodeDesigner.Task as ParentTask;
				if (!parentTask.get_NodeData().get_Collapsed() && parentTask.get_Children() != null)
				{
					for (int i = 0; i < parentTask.get_Children().get_Count(); i++)
					{
						if (parentTask.get_Children().get_Item(i) != null)
						{
							this.DrawNodeCommentChildren(parentTask.get_Children().get_Item(i).get_NodeData().get_NodeDesigner() as NodeDesigner, offset);
						}
					}
				}
			}
		}

		private void RemoveNode(NodeDesigner nodeDesigner)
		{
			if (nodeDesigner.IsEntryDisplay)
			{
				return;
			}
			if (nodeDesigner.IsParent)
			{
				for (int i = 0; i < nodeDesigner.OutgoingNodeConnections.get_Count(); i++)
				{
					NodeDesigner destinationNodeDesigner = nodeDesigner.OutgoingNodeConnections.get_Item(i).DestinationNodeDesigner;
					this.mDetachedNodes.Add(destinationNodeDesigner);
					destinationNodeDesigner.Task.get_NodeData().set_Offset(destinationNodeDesigner.GetAbsolutePosition());
					destinationNodeDesigner.ParentNodeDesigner = null;
				}
			}
			if (nodeDesigner.ParentNodeDesigner != null)
			{
				nodeDesigner.ParentNodeDesigner.RemoveChildNode(nodeDesigner);
			}
			if (this.mRootNode != null && this.mRootNode.Equals(nodeDesigner))
			{
				this.mEntryNode.RemoveChildNode(nodeDesigner);
				this.mRootNode = null;
			}
			if (this.mRootNode != null)
			{
				this.RemoveReferencedTasks(this.mRootNode, nodeDesigner.Task);
			}
			if (this.mDetachedNodes != null)
			{
				for (int j = 0; j < this.mDetachedNodes.get_Count(); j++)
				{
					this.RemoveReferencedTasks(this.mDetachedNodes.get_Item(j), nodeDesigner.Task);
				}
			}
			this.mDetachedNodes.Remove(nodeDesigner);
			BehaviorUndo.DestroyObject(nodeDesigner, false);
		}

		private void RemoveReferencedTasks(NodeDesigner nodeDesigner, Task task)
		{
			bool flag = false;
			bool flag2 = false;
			FieldInfo[] allFields = TaskUtility.GetAllFields(nodeDesigner.Task.GetType());
			for (int i = 0; i < allFields.Length; i++)
			{
				if ((!allFields[i].get_IsPrivate() && !allFields[i].get_IsFamily()) || BehaviorDesignerUtility.HasAttribute(allFields[i], typeof(SerializeField)))
				{
					if (typeof(IList).IsAssignableFrom(allFields[i].get_FieldType()))
					{
						if (typeof(Task).IsAssignableFrom(allFields[i].get_FieldType().GetElementType()) || (allFields[i].get_FieldType().get_IsGenericType() && typeof(Task).IsAssignableFrom(allFields[i].get_FieldType().GetGenericArguments()[0])))
						{
							Task[] array = allFields[i].GetValue(nodeDesigner.Task) as Task[];
							if (array != null)
							{
								for (int j = array.Length - 1; j > -1; j--)
								{
									if (array[j] != null && (nodeDesigner.Task.Equals(task) || array[j].Equals(task)))
									{
										TaskInspector.ReferenceTasks(nodeDesigner.Task, task, allFields[i], ref flag, ref flag2, false, false);
									}
								}
							}
						}
					}
					else if (typeof(Task).IsAssignableFrom(allFields[i].get_FieldType()))
					{
						Task task2 = allFields[i].GetValue(nodeDesigner.Task) as Task;
						if (task2 != null && (nodeDesigner.Task.Equals(task) || task2.Equals(task)))
						{
							TaskInspector.ReferenceTasks(nodeDesigner.Task, task, allFields[i], ref flag, ref flag2, false, false);
						}
					}
				}
			}
			if (nodeDesigner.IsParent)
			{
				ParentTask parentTask = nodeDesigner.Task as ParentTask;
				if (parentTask.get_Children() != null)
				{
					for (int k = 0; k < parentTask.get_Children().get_Count(); k++)
					{
						if (parentTask.get_Children().get_Item(k) != null)
						{
							this.RemoveReferencedTasks(parentTask.get_Children().get_Item(k).get_NodeData().get_NodeDesigner() as NodeDesigner, task);
						}
					}
				}
			}
		}

		public bool NodeCanOriginateConnection(NodeDesigner nodeDesigner, NodeConnection connection)
		{
			return !nodeDesigner.IsEntryDisplay || (nodeDesigner.IsEntryDisplay && connection.NodeConnectionType == NodeConnectionType.Outgoing);
		}

		public bool NodeCanAcceptConnection(NodeDesigner nodeDesigner, NodeConnection connection)
		{
			if ((!nodeDesigner.IsEntryDisplay || connection.NodeConnectionType != NodeConnectionType.Incoming) && (nodeDesigner.IsEntryDisplay || (!nodeDesigner.IsParent && (nodeDesigner.IsParent || connection.NodeConnectionType != NodeConnectionType.Outgoing))))
			{
				return false;
			}
			if (nodeDesigner.IsEntryDisplay || connection.OriginatingNodeDesigner.IsEntryDisplay)
			{
				return true;
			}
			HashSet<NodeDesigner> hashSet = new HashSet<NodeDesigner>();
			NodeDesigner nodeDesigner2 = (connection.NodeConnectionType != NodeConnectionType.Outgoing) ? connection.OriginatingNodeDesigner : nodeDesigner;
			NodeDesigner nodeDesigner3 = (connection.NodeConnectionType != NodeConnectionType.Outgoing) ? nodeDesigner : connection.OriginatingNodeDesigner;
			return !this.CycleExists(nodeDesigner2, ref hashSet) && !hashSet.Contains(nodeDesigner3);
		}

		private bool CycleExists(NodeDesigner nodeDesigner, ref HashSet<NodeDesigner> set)
		{
			if (set.Contains(nodeDesigner))
			{
				return true;
			}
			set.Add(nodeDesigner);
			if (nodeDesigner.IsParent)
			{
				ParentTask parentTask = nodeDesigner.Task as ParentTask;
				if (parentTask.get_Children() != null)
				{
					for (int i = 0; i < parentTask.get_Children().get_Count(); i++)
					{
						if (this.CycleExists(parentTask.get_Children().get_Item(i).get_NodeData().get_NodeDesigner() as NodeDesigner, ref set))
						{
							return true;
						}
					}
				}
			}
			return false;
		}

		public void ConnectNodes(BehaviorSource behaviorSource, NodeDesigner nodeDesigner)
		{
			NodeConnection nodeConnection = this.mActiveNodeConnection;
			this.mActiveNodeConnection = null;
			if (nodeConnection != null && !nodeConnection.OriginatingNodeDesigner.Equals(nodeDesigner))
			{
				NodeDesigner originatingNodeDesigner = nodeConnection.OriginatingNodeDesigner;
				if (nodeConnection.NodeConnectionType == NodeConnectionType.Outgoing)
				{
					this.RemoveParentConnection(nodeDesigner);
					this.CheckForLastConnectionRemoval(originatingNodeDesigner);
					originatingNodeDesigner.AddChildNode(nodeDesigner, nodeConnection, true, false);
				}
				else
				{
					this.RemoveParentConnection(originatingNodeDesigner);
					this.CheckForLastConnectionRemoval(nodeDesigner);
					nodeDesigner.AddChildNode(originatingNodeDesigner, nodeConnection, true, false);
				}
				if (nodeConnection.OriginatingNodeDesigner.IsEntryDisplay)
				{
					this.mRootNode = nodeConnection.DestinationNodeDesigner;
				}
				this.mDetachedNodes.Remove(nodeConnection.DestinationNodeDesigner);
			}
		}

		private void RemoveParentConnection(NodeDesigner nodeDesigner)
		{
			if (nodeDesigner.ParentNodeDesigner != null)
			{
				NodeDesigner parentNodeDesigner = nodeDesigner.ParentNodeDesigner;
				NodeConnection nodeConnection = null;
				for (int i = 0; i < parentNodeDesigner.OutgoingNodeConnections.get_Count(); i++)
				{
					if (parentNodeDesigner.OutgoingNodeConnections.get_Item(i).DestinationNodeDesigner.Equals(nodeDesigner))
					{
						nodeConnection = parentNodeDesigner.OutgoingNodeConnections.get_Item(i);
						break;
					}
				}
				if (nodeConnection != null)
				{
					this.RemoveConnection(nodeConnection);
				}
			}
		}

		private void CheckForLastConnectionRemoval(NodeDesigner nodeDesigner)
		{
			if (nodeDesigner.IsEntryDisplay)
			{
				if (nodeDesigner.OutgoingNodeConnections.get_Count() == 1)
				{
					this.RemoveConnection(nodeDesigner.OutgoingNodeConnections.get_Item(0));
				}
			}
			else
			{
				ParentTask parentTask = nodeDesigner.Task as ParentTask;
				if (parentTask.get_Children() != null && parentTask.get_Children().get_Count() + 1 > parentTask.MaxChildren())
				{
					NodeConnection nodeConnection = null;
					for (int i = 0; i < nodeDesigner.OutgoingNodeConnections.get_Count(); i++)
					{
						if (nodeDesigner.OutgoingNodeConnections.get_Item(i).DestinationNodeDesigner.Equals(parentTask.get_Children().get_Item(parentTask.get_Children().get_Count() - 1).get_NodeData().get_NodeDesigner() as NodeDesigner))
						{
							nodeConnection = nodeDesigner.OutgoingNodeConnections.get_Item(i);
							break;
						}
					}
					if (nodeConnection != null)
					{
						this.RemoveConnection(nodeConnection);
					}
				}
			}
		}

		public void NodeConnectionsAt(Vector2 point, Vector2 offset, ref List<NodeConnection> nodeConnections)
		{
			if (this.mEntryNode == null)
			{
				return;
			}
			this.NodeChildrenConnectionsAt(this.mEntryNode, point, offset, ref nodeConnections);
			if (this.mRootNode != null)
			{
				this.NodeChildrenConnectionsAt(this.mRootNode, point, offset, ref nodeConnections);
			}
			for (int i = 0; i < this.mDetachedNodes.get_Count(); i++)
			{
				this.NodeChildrenConnectionsAt(this.mDetachedNodes.get_Item(i), point, offset, ref nodeConnections);
			}
		}

		private void NodeChildrenConnectionsAt(NodeDesigner nodeDesigner, Vector2 point, Vector2 offset, ref List<NodeConnection> nodeConnections)
		{
			if (nodeDesigner.Task.get_NodeData().get_Collapsed())
			{
				return;
			}
			nodeDesigner.ConnectionContains(point, offset, ref nodeConnections);
			if (nodeDesigner.IsParent)
			{
				ParentTask parentTask = nodeDesigner.Task as ParentTask;
				if (parentTask != null && parentTask.get_Children() != null)
				{
					for (int i = 0; i < parentTask.get_Children().get_Count(); i++)
					{
						if (parentTask.get_Children().get_Item(i) != null)
						{
							this.NodeChildrenConnectionsAt(parentTask.get_Children().get_Item(i).get_NodeData().get_NodeDesigner() as NodeDesigner, point, offset, ref nodeConnections);
						}
					}
				}
			}
		}

		public void RemoveConnection(NodeConnection nodeConnection)
		{
			nodeConnection.DestinationNodeDesigner.Task.get_NodeData().set_Offset(nodeConnection.DestinationNodeDesigner.GetAbsolutePosition());
			this.mDetachedNodes.Add(nodeConnection.DestinationNodeDesigner);
			nodeConnection.OriginatingNodeDesigner.RemoveChildNode(nodeConnection.DestinationNodeDesigner);
			if (nodeConnection.OriginatingNodeDesigner.IsEntryDisplay)
			{
				this.mRootNode = null;
			}
		}

		public bool IsSelected(NodeConnection nodeConnection)
		{
			for (int i = 0; i < this.mSelectedNodeConnections.get_Count(); i++)
			{
				if (this.mSelectedNodeConnections.get_Item(i).Equals(nodeConnection))
				{
					return true;
				}
			}
			return false;
		}

		public void Select(NodeConnection nodeConnection)
		{
			this.mSelectedNodeConnections.Add(nodeConnection);
			nodeConnection.select();
		}

		public void Deselect(NodeConnection nodeConnection)
		{
			this.mSelectedNodeConnections.Remove(nodeConnection);
			nodeConnection.deselect();
		}

		public void ClearConnectionSelection()
		{
			for (int i = 0; i < this.mSelectedNodeConnections.get_Count(); i++)
			{
				this.mSelectedNodeConnections.get_Item(i).deselect();
			}
			this.mSelectedNodeConnections.Clear();
		}

		public void GraphDirty()
		{
			if (this.mEntryNode == null)
			{
				return;
			}
			this.mEntryNode.MarkDirty();
			if (this.mRootNode != null)
			{
				this.MarkNodeDirty(this.mRootNode);
			}
			for (int i = this.mDetachedNodes.get_Count() - 1; i > -1; i--)
			{
				this.MarkNodeDirty(this.mDetachedNodes.get_Item(i));
			}
		}

		private void MarkNodeDirty(NodeDesigner nodeDesigner)
		{
			nodeDesigner.MarkDirty();
			if (nodeDesigner.IsEntryDisplay)
			{
				if (nodeDesigner.OutgoingNodeConnections.get_Count() > 0 && nodeDesigner.OutgoingNodeConnections.get_Item(0).DestinationNodeDesigner != null)
				{
					this.MarkNodeDirty(nodeDesigner.OutgoingNodeConnections.get_Item(0).DestinationNodeDesigner);
				}
			}
			else if (nodeDesigner.IsParent)
			{
				ParentTask parentTask = nodeDesigner.Task as ParentTask;
				if (parentTask.get_Children() != null)
				{
					for (int i = 0; i < parentTask.get_Children().get_Count(); i++)
					{
						if (parentTask.get_Children().get_Item(i) != null)
						{
							this.MarkNodeDirty(parentTask.get_Children().get_Item(i).get_NodeData().get_NodeDesigner() as NodeDesigner);
						}
					}
				}
			}
		}

		public List<BehaviorSource> FindReferencedBehaviors()
		{
			List<BehaviorSource> result = new List<BehaviorSource>();
			if (this.mRootNode != null)
			{
				this.FindReferencedBehaviors(this.mRootNode, ref result);
			}
			for (int i = 0; i < this.mDetachedNodes.get_Count(); i++)
			{
				this.FindReferencedBehaviors(this.mDetachedNodes.get_Item(i), ref result);
			}
			return result;
		}

		public void FindReferencedBehaviors(NodeDesigner nodeDesigner, ref List<BehaviorSource> behaviors)
		{
			FieldInfo[] publicFields = TaskUtility.GetPublicFields(nodeDesigner.Task.GetType());
			for (int i = 0; i < publicFields.Length; i++)
			{
				Type fieldType = publicFields[i].get_FieldType();
				if (typeof(IList).IsAssignableFrom(fieldType))
				{
					Type type = fieldType;
					if (fieldType.get_IsGenericType())
					{
						while (!type.get_IsGenericType())
						{
							type = type.get_BaseType();
						}
						type = fieldType.GetGenericArguments()[0];
					}
					else
					{
						type = fieldType.GetElementType();
					}
					if (type != null)
					{
						if (typeof(ExternalBehavior).IsAssignableFrom(type) || typeof(Behavior).IsAssignableFrom(type))
						{
							IList list = publicFields[i].GetValue(nodeDesigner.Task) as IList;
							if (list != null)
							{
								for (int j = 0; j < list.get_Count(); j++)
								{
									if (list.get_Item(j) != null)
									{
										BehaviorSource behaviorSource;
										if (list.get_Item(j) is ExternalBehavior)
										{
											behaviorSource = (list.get_Item(j) as ExternalBehavior).get_BehaviorSource();
											if (behaviorSource.get_Owner() == null)
											{
												behaviorSource.set_Owner(list.get_Item(j) as ExternalBehavior);
											}
										}
										else
										{
											behaviorSource = (list.get_Item(j) as Behavior).GetBehaviorSource();
											if (behaviorSource.get_Owner() == null)
											{
												behaviorSource.set_Owner(list.get_Item(j) as Behavior);
											}
										}
										behaviors.Add(behaviorSource);
									}
								}
							}
						}
						else if (typeof(Behavior).IsAssignableFrom(type))
						{
						}
					}
				}
				else if (typeof(ExternalBehavior).IsAssignableFrom(fieldType) || typeof(Behavior).IsAssignableFrom(fieldType))
				{
					object value = publicFields[i].GetValue(nodeDesigner.Task);
					if (value != null)
					{
						BehaviorSource behaviorSource2;
						if (value is ExternalBehavior)
						{
							behaviorSource2 = (value as ExternalBehavior).get_BehaviorSource();
							if (behaviorSource2.get_Owner() == null)
							{
								behaviorSource2.set_Owner(value as ExternalBehavior);
							}
							behaviors.Add(behaviorSource2);
						}
						else
						{
							behaviorSource2 = (value as Behavior).GetBehaviorSource();
							if (behaviorSource2.get_Owner() == null)
							{
								behaviorSource2.set_Owner(value as Behavior);
							}
						}
						behaviors.Add(behaviorSource2);
					}
				}
			}
			if (nodeDesigner.IsParent)
			{
				ParentTask parentTask = nodeDesigner.Task as ParentTask;
				if (parentTask.get_Children() != null)
				{
					for (int k = 0; k < parentTask.get_Children().get_Count(); k++)
					{
						if (parentTask.get_Children().get_Item(k) != null)
						{
							this.FindReferencedBehaviors(parentTask.get_Children().get_Item(k).get_NodeData().get_NodeDesigner() as NodeDesigner, ref behaviors);
						}
					}
				}
			}
		}

		public void SelectAll()
		{
			for (int i = this.mSelectedNodes.get_Count() - 1; i > -1; i--)
			{
				this.Deselect(this.mSelectedNodes.get_Item(i));
			}
			if (this.mRootNode != null)
			{
				this.SelectAll(this.mRootNode);
			}
			for (int j = this.mDetachedNodes.get_Count() - 1; j > -1; j--)
			{
				this.SelectAll(this.mDetachedNodes.get_Item(j));
			}
		}

		private void SelectAll(NodeDesigner nodeDesigner)
		{
			this.Select(nodeDesigner);
			if (nodeDesigner.Task.GetType().IsSubclassOf(typeof(ParentTask)))
			{
				ParentTask parentTask = nodeDesigner.Task as ParentTask;
				if (parentTask.get_Children() != null)
				{
					for (int i = 0; i < parentTask.get_Children().get_Count(); i++)
					{
						this.SelectAll(parentTask.get_Children().get_Item(i).get_NodeData().get_NodeDesigner() as NodeDesigner);
					}
				}
			}
		}

		public void IdentifyNode(NodeDesigner nodeDesigner)
		{
			nodeDesigner.IdentifyNode();
		}

		public List<TaskSerializer> Copy(Vector2 graphOffset, float graphZoom)
		{
			List<TaskSerializer> list = new List<TaskSerializer>();
			for (int i = 0; i < this.mSelectedNodes.get_Count(); i++)
			{
				TaskSerializer taskSerializer;
				if ((taskSerializer = TaskCopier.CopySerialized(this.mSelectedNodes.get_Item(i).Task)) != null)
				{
					if (this.mSelectedNodes.get_Item(i).IsParent)
					{
						ParentTask parentTask = this.mSelectedNodes.get_Item(i).Task as ParentTask;
						if (parentTask.get_Children() != null)
						{
							List<int> list2 = new List<int>();
							for (int j = 0; j < parentTask.get_Children().get_Count(); j++)
							{
								int num;
								if ((num = this.mSelectedNodes.IndexOf(parentTask.get_Children().get_Item(j).get_NodeData().get_NodeDesigner() as NodeDesigner)) != -1)
								{
									list2.Add(num);
								}
							}
							taskSerializer.childrenIndex = list2;
						}
					}
					taskSerializer.offset = (taskSerializer.offset + graphOffset) * graphZoom;
					list.Add(taskSerializer);
				}
			}
			return (list.get_Count() <= 0) ? null : list;
		}

		public bool Paste(BehaviorSource behaviorSource, Vector3 position, List<TaskSerializer> copiedTasks, Vector2 graphOffset, float graphZoom)
		{
			if (copiedTasks == null || copiedTasks.get_Count() == 0)
			{
				return false;
			}
			this.ClearNodeSelection();
			this.ClearConnectionSelection();
			this.RemapIDs();
			List<NodeDesigner> list = new List<NodeDesigner>();
			for (int i = 0; i < copiedTasks.get_Count(); i++)
			{
				TaskSerializer taskSerializer = copiedTasks.get_Item(i);
				Task task = TaskCopier.PasteTask(behaviorSource, taskSerializer);
				NodeDesigner nodeDesigner = ScriptableObject.CreateInstance<NodeDesigner>();
				nodeDesigner.LoadTask(task, (behaviorSource.get_Owner() == null) ? null : (behaviorSource.get_Owner().GetObject() as Behavior), ref this.mNextTaskID);
				nodeDesigner.Task.get_NodeData().set_Offset(taskSerializer.offset / graphZoom - graphOffset);
				list.Add(nodeDesigner);
				this.mDetachedNodes.Add(nodeDesigner);
				this.Select(nodeDesigner);
			}
			for (int j = 0; j < copiedTasks.get_Count(); j++)
			{
				TaskSerializer taskSerializer2 = copiedTasks.get_Item(j);
				if (taskSerializer2.childrenIndex != null)
				{
					for (int k = 0; k < taskSerializer2.childrenIndex.get_Count(); k++)
					{
						NodeDesigner nodeDesigner2 = list.get_Item(j);
						NodeConnection nodeConnection = ScriptableObject.CreateInstance<NodeConnection>();
						nodeConnection.LoadConnection(nodeDesigner2, NodeConnectionType.Outgoing);
						nodeDesigner2.AddChildNode(list.get_Item(taskSerializer2.childrenIndex.get_Item(k)), nodeConnection, true, false);
						this.mDetachedNodes.Remove(list.get_Item(taskSerializer2.childrenIndex.get_Item(k)));
					}
				}
			}
			if (this.mEntryNode == null)
			{
				Task task2 = Activator.CreateInstance(TaskUtility.GetTypeWithinAssembly("BehaviorDesigner.Runtime.Tasks.EntryTask")) as Task;
				this.mEntryNode = ScriptableObject.CreateInstance<NodeDesigner>();
				this.mEntryNode.LoadNode(task2, behaviorSource, new Vector2(position.x, position.y - 120f), ref this.mNextTaskID);
				this.mEntryNode.MakeEntryDisplay();
				if (this.mDetachedNodes.get_Count() > 0)
				{
					this.mActiveNodeConnection = ScriptableObject.CreateInstance<NodeConnection>();
					this.mActiveNodeConnection.LoadConnection(this.mEntryNode, NodeConnectionType.Outgoing);
					this.ConnectNodes(behaviorSource, this.mDetachedNodes.get_Item(0));
				}
			}
			this.Save(behaviorSource);
			return true;
		}

		public bool Delete(BehaviorSource behaviorSource, BehaviorDesignerWindow.TaskCallbackHandler callback)
		{
			bool flag = false;
			if (this.mSelectedNodeConnections != null)
			{
				for (int i = 0; i < this.mSelectedNodeConnections.get_Count(); i++)
				{
					this.RemoveConnection(this.mSelectedNodeConnections.get_Item(i));
				}
				this.mSelectedNodeConnections.Clear();
				flag = true;
			}
			if (this.mSelectedNodes != null)
			{
				for (int j = 0; j < this.mSelectedNodes.get_Count(); j++)
				{
					if (callback != null)
					{
						callback(behaviorSource, this.mSelectedNodes.get_Item(j).Task);
					}
					this.RemoveNode(this.mSelectedNodes.get_Item(j));
				}
				this.mSelectedNodes.Clear();
				flag = true;
			}
			if (flag)
			{
				BehaviorUndo.RegisterUndo("Delete", behaviorSource.get_Owner().GetObject());
				TaskReferences.CheckReferences(behaviorSource);
				this.Save(behaviorSource);
			}
			return flag;
		}

		public bool RemoveSharedVariableReferences(SharedVariable sharedVariable)
		{
			if (this.mEntryNode == null)
			{
				return false;
			}
			bool result = false;
			if (this.mRootNode != null && this.RemoveSharedVariableReference(this.mRootNode, sharedVariable))
			{
				result = true;
			}
			if (this.mDetachedNodes != null)
			{
				for (int i = 0; i < this.mDetachedNodes.get_Count(); i++)
				{
					if (this.RemoveSharedVariableReference(this.mDetachedNodes.get_Item(i), sharedVariable))
					{
						result = true;
					}
				}
			}
			return result;
		}

		private bool RemoveSharedVariableReference(NodeDesigner nodeDesigner, SharedVariable sharedVariable)
		{
			bool result = false;
			FieldInfo[] allFields = TaskUtility.GetAllFields(nodeDesigner.Task.GetType());
			for (int i = 0; i < allFields.Length; i++)
			{
				if (typeof(SharedVariable).IsAssignableFrom(allFields[i].get_FieldType()))
				{
					SharedVariable sharedVariable2 = allFields[i].GetValue(nodeDesigner.Task) as SharedVariable;
					if (sharedVariable2 != null && !string.IsNullOrEmpty(sharedVariable2.get_Name()) && sharedVariable2.get_IsGlobal() == sharedVariable.get_IsGlobal() && sharedVariable2.get_Name().Equals(sharedVariable.get_Name()))
					{
						if (!allFields[i].get_FieldType().get_IsAbstract())
						{
							sharedVariable2 = (Activator.CreateInstance(allFields[i].get_FieldType()) as SharedVariable);
							sharedVariable2.set_IsShared(true);
							allFields[i].SetValue(nodeDesigner.Task, sharedVariable2);
						}
						result = true;
					}
				}
			}
			if (nodeDesigner.IsParent)
			{
				ParentTask parentTask = nodeDesigner.Task as ParentTask;
				if (parentTask.get_Children() != null)
				{
					for (int j = 0; j < parentTask.get_Children().get_Count(); j++)
					{
						if (parentTask.get_Children().get_Item(j) != null && this.RemoveSharedVariableReference(parentTask.get_Children().get_Item(j).get_NodeData().get_NodeDesigner() as NodeDesigner, sharedVariable))
						{
							result = true;
						}
					}
				}
			}
			return result;
		}

		private void RemapIDs()
		{
			if (this.mEntryNode == null)
			{
				return;
			}
			this.mNextTaskID = 0;
			this.mEntryNode.SetID(ref this.mNextTaskID);
			if (this.mRootNode != null)
			{
				this.mRootNode.SetID(ref this.mNextTaskID);
			}
			for (int i = 0; i < this.mDetachedNodes.get_Count(); i++)
			{
				this.mDetachedNodes.get_Item(i).SetID(ref this.mNextTaskID);
			}
			this.mNodeSelectedID.Clear();
			for (int j = 0; j < this.mSelectedNodes.get_Count(); j++)
			{
				this.mNodeSelectedID.Add(this.mSelectedNodes.get_Item(j).Task.get_ID());
			}
		}

		public Rect GraphSize(Vector3 offset)
		{
			if (this.mEntryNode == null)
			{
				return default(Rect);
			}
			Rect result = default(Rect);
			result.set_xMin(3.40282347E+38f);
			result.set_xMax(-3.40282347E+38f);
			result.set_yMin(3.40282347E+38f);
			result.set_yMax(-3.40282347E+38f);
			this.GetNodeMinMax(offset, this.mEntryNode, ref result);
			if (this.mRootNode != null)
			{
				this.GetNodeMinMax(offset, this.mRootNode, ref result);
			}
			for (int i = 0; i < this.mDetachedNodes.get_Count(); i++)
			{
				this.GetNodeMinMax(offset, this.mDetachedNodes.get_Item(i), ref result);
			}
			return result;
		}

		private void GetNodeMinMax(Vector2 offset, NodeDesigner nodeDesigner, ref Rect minMaxRect)
		{
			Rect rect = nodeDesigner.Rectangle(offset, true, true);
			if (rect.get_xMin() < minMaxRect.get_xMin())
			{
				minMaxRect.set_xMin(rect.get_xMin());
			}
			if (rect.get_yMin() < minMaxRect.get_yMin())
			{
				minMaxRect.set_yMin(rect.get_yMin());
			}
			if (rect.get_xMax() > minMaxRect.get_xMax())
			{
				minMaxRect.set_xMax(rect.get_xMax());
			}
			if (rect.get_yMax() > minMaxRect.get_yMax())
			{
				minMaxRect.set_yMax(rect.get_yMax());
			}
			if (nodeDesigner.IsParent)
			{
				ParentTask parentTask = nodeDesigner.Task as ParentTask;
				if (parentTask.get_Children() != null)
				{
					for (int i = 0; i < parentTask.get_Children().get_Count(); i++)
					{
						this.GetNodeMinMax(offset, parentTask.get_Children().get_Item(i).get_NodeData().get_NodeDesigner() as NodeDesigner, ref minMaxRect);
					}
				}
			}
		}

		public void Save(BehaviorSource behaviorSource)
		{
			if (object.ReferenceEquals(behaviorSource.get_Owner().GetObject(), null))
			{
				return;
			}
			this.RemapIDs();
			List<Task> list = new List<Task>();
			for (int i = 0; i < this.mDetachedNodes.get_Count(); i++)
			{
				list.Add(this.mDetachedNodes.get_Item(i).Task);
			}
			behaviorSource.Save((!(this.mEntryNode != null)) ? null : this.mEntryNode.Task, (!(this.mRootNode != null)) ? null : this.mRootNode.Task, list);
			if (BehaviorDesignerPreferences.GetBool(BDPreferences.BinarySerialization))
			{
				BinarySerialization.Save(behaviorSource);
			}
			else
			{
				JSONSerialization.Save(behaviorSource);
			}
		}

		public bool Load(BehaviorSource behaviorSource, bool loadPrevBehavior, Vector2 nodePosition)
		{
			if (behaviorSource == null)
			{
				this.Clear(false);
				return false;
			}
			this.DestroyNodeDesigners();
			if (behaviorSource.get_Owner() != null && behaviorSource.get_Owner() is Behavior && (behaviorSource.get_Owner() as Behavior).get_ExternalBehavior() != null)
			{
				List<SharedVariable> list = null;
				bool flag = !Application.get_isPlaying();
				if (Application.get_isPlaying() && !(behaviorSource.get_Owner() as Behavior).get_HasInheritedVariables())
				{
					behaviorSource.CheckForSerialization(true, null);
					list = behaviorSource.GetAllVariables();
					(behaviorSource.get_Owner() as Behavior).set_HasInheritedVariables(true);
					flag = true;
				}
				ExternalBehavior externalBehavior = (behaviorSource.get_Owner() as Behavior).get_ExternalBehavior();
				externalBehavior.get_BehaviorSource().set_Owner(externalBehavior);
				externalBehavior.get_BehaviorSource().CheckForSerialization(flag, behaviorSource);
				if (list != null)
				{
					for (int i = 0; i < list.get_Count(); i++)
					{
						behaviorSource.SetVariable(list.get_Item(i).get_Name(), list.get_Item(i));
					}
				}
			}
			else
			{
				behaviorSource.CheckForSerialization(!Application.get_isPlaying(), null);
			}
			if (behaviorSource.get_EntryTask() == null && behaviorSource.get_RootTask() == null && behaviorSource.get_DetachedTasks() == null)
			{
				this.Clear(false);
				return false;
			}
			if (loadPrevBehavior)
			{
				this.mSelectedNodes.Clear();
				this.mSelectedNodeConnections.Clear();
				if (this.mPrevNodeSelectedID != null)
				{
					for (int j = 0; j < this.mPrevNodeSelectedID.Length; j++)
					{
						this.mNodeSelectedID.Add(this.mPrevNodeSelectedID[j]);
					}
					this.mPrevNodeSelectedID = null;
				}
			}
			else
			{
				this.Clear(false);
			}
			this.mNextTaskID = 0;
			this.mEntryNode = null;
			this.mRootNode = null;
			this.mDetachedNodes.Clear();
			Task task;
			Task task2;
			List<Task> list2;
			behaviorSource.Load(ref task, ref task2, ref list2);
			if (BehaviorDesignerUtility.AnyNullTasks(behaviorSource) || (behaviorSource.get_TaskData() != null && BehaviorDesignerUtility.HasRootTask(behaviorSource.get_TaskData().JSONSerialization) && behaviorSource.get_RootTask() == null))
			{
				behaviorSource.CheckForSerialization(true, null);
				behaviorSource.Load(ref task, ref task2, ref list2);
			}
			if (task == null)
			{
				if (task2 != null || (list2 != null && list2.get_Count() > 0))
				{
					behaviorSource.set_EntryTask(task = (Activator.CreateInstance(TaskUtility.GetTypeWithinAssembly("BehaviorDesigner.Runtime.Tasks.EntryTask"), true) as Task));
					this.mEntryNode = ScriptableObject.CreateInstance<NodeDesigner>();
					if (task2 != null)
					{
						this.mEntryNode.LoadNode(task, behaviorSource, new Vector2(task2.get_NodeData().get_Offset().x, task2.get_NodeData().get_Offset().y - 120f), ref this.mNextTaskID);
					}
					else
					{
						this.mEntryNode.LoadNode(task, behaviorSource, new Vector2(nodePosition.x, nodePosition.y - 120f), ref this.mNextTaskID);
					}
					this.mEntryNode.MakeEntryDisplay();
				}
			}
			else
			{
				this.mEntryNode = ScriptableObject.CreateInstance<NodeDesigner>();
				this.mEntryNode.LoadTask(task, (behaviorSource.get_Owner() == null) ? null : (behaviorSource.get_Owner().GetObject() as Behavior), ref this.mNextTaskID);
				this.mEntryNode.MakeEntryDisplay();
			}
			if (task2 != null)
			{
				this.mRootNode = ScriptableObject.CreateInstance<NodeDesigner>();
				this.mRootNode.LoadTask(task2, (behaviorSource.get_Owner() == null) ? null : (behaviorSource.get_Owner().GetObject() as Behavior), ref this.mNextTaskID);
				NodeConnection nodeConnection = ScriptableObject.CreateInstance<NodeConnection>();
				nodeConnection.LoadConnection(this.mEntryNode, NodeConnectionType.Fixed);
				this.mEntryNode.AddChildNode(this.mRootNode, nodeConnection, false, false);
				this.LoadNodeSelection(this.mRootNode);
				if (this.mEntryNode.OutgoingNodeConnections.get_Count() == 0)
				{
					this.mActiveNodeConnection = ScriptableObject.CreateInstance<NodeConnection>();
					this.mActiveNodeConnection.LoadConnection(this.mEntryNode, NodeConnectionType.Outgoing);
					this.ConnectNodes(behaviorSource, this.mRootNode);
				}
			}
			if (list2 != null)
			{
				for (int k = 0; k < list2.get_Count(); k++)
				{
					if (list2.get_Item(k) != null)
					{
						NodeDesigner nodeDesigner = ScriptableObject.CreateInstance<NodeDesigner>();
						nodeDesigner.LoadTask(list2.get_Item(k), (behaviorSource.get_Owner() == null) ? null : (behaviorSource.get_Owner().GetObject() as Behavior), ref this.mNextTaskID);
						this.mDetachedNodes.Add(nodeDesigner);
						this.LoadNodeSelection(nodeDesigner);
					}
				}
			}
			return true;
		}

		public bool HasEntryNode()
		{
			return this.mEntryNode != null && this.mEntryNode.Task != null;
		}

		public Vector2 EntryNodeOffset()
		{
			return this.mEntryNode.Task.get_NodeData().get_Offset();
		}

		public void SetStartOffset(Vector2 offset)
		{
			Vector2 vector = offset - this.mEntryNode.Task.get_NodeData().get_Offset();
			this.mEntryNode.Task.get_NodeData().set_Offset(offset);
			for (int i = 0; i < this.mDetachedNodes.get_Count(); i++)
			{
				NodeData expr_4F = this.mDetachedNodes.get_Item(i).Task.get_NodeData();
				expr_4F.set_Offset(expr_4F.get_Offset() + vector);
			}
		}

		private void LoadNodeSelection(NodeDesigner nodeDesigner)
		{
			if (nodeDesigner == null)
			{
				return;
			}
			if (this.mNodeSelectedID != null && this.mNodeSelectedID.Contains(nodeDesigner.Task.get_ID()))
			{
				this.Select(nodeDesigner, false);
			}
			if (nodeDesigner.IsParent)
			{
				ParentTask parentTask = nodeDesigner.Task as ParentTask;
				if (parentTask.get_Children() != null)
				{
					for (int i = 0; i < parentTask.get_Children().get_Count(); i++)
					{
						if (parentTask.get_Children().get_Item(i) != null && parentTask.get_Children().get_Item(i).get_NodeData() != null)
						{
							this.LoadNodeSelection(parentTask.get_Children().get_Item(i).get_NodeData().get_NodeDesigner() as NodeDesigner);
						}
					}
				}
			}
		}

		public void Clear(bool saveSelectedNodes)
		{
			if (saveSelectedNodes)
			{
				if (this.mNodeSelectedID.get_Count() > 0)
				{
					this.mPrevNodeSelectedID = this.mNodeSelectedID.ToArray();
				}
			}
			else
			{
				this.mPrevNodeSelectedID = null;
			}
			this.mNodeSelectedID.Clear();
			this.mSelectedNodes.Clear();
			this.mSelectedNodeConnections.Clear();
			this.DestroyNodeDesigners();
		}

		public void DestroyNodeDesigners()
		{
			if (this.mEntryNode != null)
			{
				this.Clear(this.mEntryNode);
			}
			if (this.mRootNode != null)
			{
				this.Clear(this.mRootNode);
			}
			for (int i = this.mDetachedNodes.get_Count() - 1; i > -1; i--)
			{
				this.Clear(this.mDetachedNodes.get_Item(i));
			}
			this.mEntryNode = null;
			this.mRootNode = null;
			this.mDetachedNodes = new List<NodeDesigner>();
		}

		private void Clear(NodeDesigner nodeDesigner)
		{
			if (nodeDesigner == null)
			{
				return;
			}
			if (nodeDesigner.IsParent)
			{
				ParentTask parentTask = nodeDesigner.Task as ParentTask;
				if (parentTask != null && parentTask.get_Children() != null)
				{
					for (int i = parentTask.get_Children().get_Count() - 1; i > -1; i--)
					{
						if (parentTask.get_Children().get_Item(i) != null)
						{
							this.Clear(parentTask.get_Children().get_Item(i).get_NodeData().get_NodeDesigner() as NodeDesigner);
						}
					}
				}
			}
			nodeDesigner.DestroyConnections();
			Object.DestroyImmediate(nodeDesigner, true);
		}
	}
}
