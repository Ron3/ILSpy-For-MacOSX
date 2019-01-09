// Decompiled with JetBrains decompiler
// Type: BehaviorDesigner.Editor.GraphDesigner
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
  [Serializable]
  public class GraphDesigner : ScriptableObject
  {
    private NodeDesigner mEntryNode;
    private NodeDesigner mRootNode;
    private List<NodeDesigner> mDetachedNodes;
    [SerializeField]
    private List<NodeDesigner> mSelectedNodes;
    private NodeDesigner mHoverNode;
    private NodeConnection mActiveNodeConnection;
    [SerializeField]
    private List<NodeConnection> mSelectedNodeConnections;
    [SerializeField]
    private int mNextTaskID;
    private List<int> mNodeSelectedID;
    [SerializeField]
    private int[] mPrevNodeSelectedID;

    public GraphDesigner()
    {
      base.\u002Ector();
    }

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
      ((Object) this).set_hideFlags((HideFlags) 61);
    }

    public NodeDesigner AddNode(
      BehaviorSource behaviorSource,
      Type type,
      Vector2 position)
    {
      Task instance = Activator.CreateInstance(type, true) as Task;
      if (instance != null)
        return this.AddNode(behaviorSource, instance, position);
      EditorUtility.DisplayDialog("Unable to Add Task", string.Format("Unable to create task of type {0}. Is the class name the same as the file name?", (object) type), "OK");
      return (NodeDesigner) null;
    }

    private NodeDesigner AddNode(
      BehaviorSource behaviorSource,
      Task task,
      Vector2 position)
    {
      if (Object.op_Equality((Object) this.mEntryNode, (Object) null))
      {
        Task instance = Activator.CreateInstance(TaskUtility.GetTypeWithinAssembly("BehaviorDesigner.Runtime.Tasks.EntryTask")) as Task;
        this.mEntryNode = (NodeDesigner) ScriptableObject.CreateInstance<NodeDesigner>();
        this.mEntryNode.LoadNode(instance, behaviorSource, new Vector2((float) position.x, (float) (position.y - 120.0)), ref this.mNextTaskID);
        this.mEntryNode.MakeEntryDisplay();
      }
      NodeDesigner instance1 = (NodeDesigner) ScriptableObject.CreateInstance<NodeDesigner>();
      instance1.LoadNode(task, behaviorSource, position, ref this.mNextTaskID);
      TaskNameAttribute[] customAttributes;
      if ((customAttributes = ((object) task).GetType().GetCustomAttributes(typeof (TaskNameAttribute), false) as TaskNameAttribute[]).Length > 0)
        task.set_FriendlyName(customAttributes[0].get_Name());
      if (this.mEntryNode.OutgoingNodeConnections.Count == 0)
      {
        this.mActiveNodeConnection = (NodeConnection) ScriptableObject.CreateInstance<NodeConnection>();
        this.mActiveNodeConnection.LoadConnection(this.mEntryNode, NodeConnectionType.Outgoing);
        this.ConnectNodes(behaviorSource, instance1);
      }
      else
        this.mDetachedNodes.Add(instance1);
      return instance1;
    }

    public NodeDesigner NodeAt(Vector2 point, Vector2 offset)
    {
      if (Object.op_Equality((Object) this.mEntryNode, (Object) null))
        return (NodeDesigner) null;
      for (int index = 0; index < this.mSelectedNodes.Count; ++index)
      {
        if (this.mSelectedNodes[index].Contains(point, offset, false))
          return this.mSelectedNodes[index];
      }
      for (int index = this.mDetachedNodes.Count - 1; index > -1; --index)
      {
        NodeDesigner nodeDesigner;
        if (Object.op_Inequality((Object) this.mDetachedNodes[index], (Object) null) && Object.op_Inequality((Object) (nodeDesigner = this.NodeChildrenAt(this.mDetachedNodes[index], point, offset)), (Object) null))
          return nodeDesigner;
      }
      NodeDesigner nodeDesigner1;
      if (Object.op_Inequality((Object) this.mRootNode, (Object) null) && Object.op_Inequality((Object) (nodeDesigner1 = this.NodeChildrenAt(this.mRootNode, point, offset)), (Object) null))
        return nodeDesigner1;
      if (this.mEntryNode.Contains(point, offset, true))
        return this.mEntryNode;
      return (NodeDesigner) null;
    }

    private NodeDesigner NodeChildrenAt(
      NodeDesigner nodeDesigner,
      Vector2 point,
      Vector2 offset)
    {
      if (nodeDesigner.Contains(point, offset, true))
        return nodeDesigner;
      if (nodeDesigner.IsParent)
      {
        ParentTask task = nodeDesigner.Task as ParentTask;
        if (!((Task) task).get_NodeData().get_Collapsed() && task.get_Children() != null)
        {
          for (int index = 0; index < task.get_Children().Count; ++index)
          {
            NodeDesigner nodeDesigner1;
            if (task.get_Children()[index] != null && Object.op_Inequality((Object) (nodeDesigner1 = this.NodeChildrenAt(task.get_Children()[index].get_NodeData().get_NodeDesigner() as NodeDesigner, point, offset)), (Object) null))
              return nodeDesigner1;
          }
        }
      }
      return (NodeDesigner) null;
    }

    public List<NodeDesigner> NodesAt(Rect rect, Vector2 offset)
    {
      List<NodeDesigner> nodes = new List<NodeDesigner>();
      if (Object.op_Inequality((Object) this.mRootNode, (Object) null))
        this.NodesChildrenAt(this.mRootNode, rect, offset, ref nodes);
      for (int index = 0; index < this.mDetachedNodes.Count; ++index)
        this.NodesChildrenAt(this.mDetachedNodes[index], rect, offset, ref nodes);
      if (nodes.Count > 0)
        return nodes;
      return (List<NodeDesigner>) null;
    }

    private void NodesChildrenAt(
      NodeDesigner nodeDesigner,
      Rect rect,
      Vector2 offset,
      ref List<NodeDesigner> nodes)
    {
      if (nodeDesigner.Intersects(rect, offset))
        nodes.Add(nodeDesigner);
      if (!nodeDesigner.IsParent)
        return;
      ParentTask task = nodeDesigner.Task as ParentTask;
      if (((Task) task).get_NodeData().get_Collapsed() || task.get_Children() == null)
        return;
      for (int index = 0; index < task.get_Children().Count; ++index)
      {
        if (task.get_Children()[index] != null)
          this.NodesChildrenAt(task.get_Children()[index].get_NodeData().get_NodeDesigner() as NodeDesigner, rect, offset, ref nodes);
      }
    }

    public bool IsSelected(NodeDesigner nodeDesigner)
    {
      return this.mSelectedNodes.Contains(nodeDesigner);
    }

    public bool IsParentSelected(NodeDesigner nodeDesigner)
    {
      if (!Object.op_Inequality((Object) nodeDesigner.ParentNodeDesigner, (Object) null))
        return false;
      if (this.IsSelected(nodeDesigner.ParentNodeDesigner))
        return true;
      return this.IsParentSelected(nodeDesigner.ParentNodeDesigner);
    }

    public void Select(NodeDesigner nodeDesigner)
    {
      this.Select(nodeDesigner, true);
    }

    public void Select(NodeDesigner nodeDesigner, bool addHash)
    {
      if (this.mSelectedNodes.Contains(nodeDesigner))
        return;
      if (this.mSelectedNodes.Count == 1)
        this.IndicateReferencedTasks(this.mSelectedNodes[0].Task, false);
      this.mSelectedNodes.Add(nodeDesigner);
      if (addHash)
        this.mNodeSelectedID.Add(nodeDesigner.Task.get_ID());
      nodeDesigner.Select();
      if (this.mSelectedNodes.Count != 1)
        return;
      this.IndicateReferencedTasks(this.mSelectedNodes[0].Task, true);
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
      for (int index = this.mSelectedNodes.Count - 1; index >= 0; --index)
      {
        if (Object.op_Equality((Object) exceptionNodeDesigner, (Object) null) || !this.mSelectedNodes[index].Equals((object) exceptionNodeDesigner))
        {
          this.mSelectedNodes[index].Deselect();
          this.mSelectedNodes.RemoveAt(index);
          this.mNodeSelectedID.RemoveAt(index);
        }
      }
      if (!Object.op_Inequality((Object) exceptionNodeDesigner, (Object) null))
        return;
      this.IndicateReferencedTasks(exceptionNodeDesigner.Task, false);
    }

    public void ClearNodeSelection()
    {
      if (this.mSelectedNodes.Count == 1)
        this.IndicateReferencedTasks(this.mSelectedNodes[0].Task, false);
      for (int index = 0; index < this.mSelectedNodes.Count; ++index)
        this.mSelectedNodes[index].Deselect();
      this.mSelectedNodes.Clear();
      this.mNodeSelectedID.Clear();
    }

    public void DeselectWithParent(NodeDesigner nodeDesigner)
    {
      for (int index = this.mSelectedNodes.Count - 1; index >= 0; --index)
      {
        if (this.mSelectedNodes[index].HasParent(nodeDesigner))
          this.Deselect(this.mSelectedNodes[index]);
      }
    }

    public bool ReplaceSelectedNode(BehaviorSource behaviorSource, Type taskType)
    {
      BehaviorUndo.RegisterUndo("Replace", behaviorSource.get_Owner().GetObject());
      Vector2 absolutePosition = this.SelectedNodes[0].GetAbsolutePosition();
      NodeDesigner parentNodeDesigner = this.SelectedNodes[0].ParentNodeDesigner;
      List<Task> taskList1 = !this.SelectedNodes[0].IsParent ? (List<Task>) null : (this.SelectedNodes[0].Task as ParentTask).get_Children();
      UnknownTask task1 = this.SelectedNodes[0].Task as UnknownTask;
      this.RemoveNode(this.SelectedNodes[0]);
      this.mSelectedNodes.Clear();
      TaskReferences.CheckReferences(behaviorSource);
      NodeDesigner nodeDesigner = (NodeDesigner) null;
      if (task1 != null)
      {
        Task task2 = (Task) null;
        if (!string.IsNullOrEmpty((string) task1.JSONSerialization))
        {
          Dictionary<int, Task> dictionary1 = new Dictionary<int, Task>();
          Dictionary<string, object> dictionary2 = MiniJSON.Deserialize((string) task1.JSONSerialization) as Dictionary<string, object>;
          if (dictionary2.ContainsKey("Type"))
            dictionary2["Type"] = (object) taskType.ToString();
          task2 = JSONDeserialization.DeserializeTask(behaviorSource, dictionary2, ref dictionary1, (List<Object>) null);
        }
        else
        {
          TaskSerializationData serializationData1 = new TaskSerializationData();
          ((List<string>) serializationData1.types).Add(taskType.ToString());
          ((List<int>) serializationData1.startIndex).Add(0);
          FieldSerializationData serializationData2 = new FieldSerializationData();
          serializationData2.fieldNameHash = task1.fieldNameHash;
          serializationData2.startIndex = task1.startIndex;
          serializationData2.dataPosition = task1.dataPosition;
          serializationData2.unityObjects = task1.unityObjects;
          serializationData2.byteDataArray = (__Null) ((List<byte>) task1.byteData).ToArray();
          List<Task> taskList2 = new List<Task>();
          BinaryDeserialization.LoadTask(serializationData1, serializationData2, ref taskList2, ref behaviorSource);
          if (taskList2.Count > 0)
            task2 = taskList2[0];
        }
        if (task2 != null)
          nodeDesigner = this.AddNode(behaviorSource, task2, absolutePosition);
      }
      else
        nodeDesigner = this.AddNode(behaviorSource, taskType, absolutePosition);
      if (Object.op_Equality((Object) nodeDesigner, (Object) null))
        return false;
      if (Object.op_Inequality((Object) parentNodeDesigner, (Object) null))
      {
        this.ActiveNodeConnection = parentNodeDesigner.CreateNodeConnection(false);
        this.ConnectNodes(behaviorSource, nodeDesigner);
      }
      if (nodeDesigner.IsParent && taskList1 != null)
      {
        for (int index = 0; index < taskList1.Count; ++index)
        {
          this.ActiveNodeConnection = nodeDesigner.CreateNodeConnection(false);
          this.ConnectNodes(behaviorSource, taskList1[index].get_NodeData().get_NodeDesigner() as NodeDesigner);
          if (index >= (nodeDesigner.Task as ParentTask).MaxChildren())
            break;
        }
      }
      this.Select(nodeDesigner);
      return true;
    }

    public void Hover(NodeDesigner nodeDesigner)
    {
      if (nodeDesigner.ShowHoverBar)
        return;
      nodeDesigner.ShowHoverBar = true;
      this.HoverNode = nodeDesigner;
    }

    public void ClearHover()
    {
      if (!Object.op_Implicit((Object) this.HoverNode))
        return;
      this.HoverNode.ShowHoverBar = false;
      this.HoverNode = (NodeDesigner) null;
    }

    private void IndicateReferencedTasks(Task task, bool indicate)
    {
      List<Task> referencedTasks = TaskInspector.GetReferencedTasks(task);
      if (referencedTasks == null || referencedTasks.Count <= 0)
        return;
      for (int index = 0; index < referencedTasks.Count; ++index)
      {
        if (referencedTasks[index] != null && referencedTasks[index].get_NodeData() != null)
        {
          NodeDesigner nodeDesigner = referencedTasks[index].get_NodeData().get_NodeDesigner() as NodeDesigner;
          if (Object.op_Inequality((Object) nodeDesigner, (Object) null))
            nodeDesigner.ShowReferenceIcon = indicate;
        }
      }
    }

    public bool DragSelectedNodes(Vector2 delta, bool dragChildren)
    {
      if (this.mSelectedNodes.Count == 0)
        return false;
      bool flag = this.mSelectedNodes.Count == 1;
      for (int index = 0; index < this.mSelectedNodes.Count; ++index)
        this.DragNode(this.mSelectedNodes[index], delta, dragChildren);
      if (flag && dragChildren && (this.mSelectedNodes[0].IsEntryDisplay && Object.op_Inequality((Object) this.mRootNode, (Object) null)))
        this.DragNode(this.mRootNode, delta, dragChildren);
      return true;
    }

    private void DragNode(NodeDesigner nodeDesigner, Vector2 delta, bool dragChildren)
    {
      if (this.IsParentSelected(nodeDesigner) && dragChildren)
        return;
      nodeDesigner.ChangeOffset(delta);
      if (Object.op_Inequality((Object) nodeDesigner.ParentNodeDesigner, (Object) null))
      {
        int index1 = nodeDesigner.ParentNodeDesigner.ChildIndexForTask(nodeDesigner.Task);
        if (index1 != -1)
        {
          int index2 = index1 - 1;
          bool flag = false;
          NodeDesigner nodeDesigner1 = nodeDesigner.ParentNodeDesigner.NodeDesignerForChildIndex(index2);
          if (Object.op_Inequality((Object) nodeDesigner1, (Object) null) && nodeDesigner.Task.get_NodeData().get_Offset().x < nodeDesigner1.Task.get_NodeData().get_Offset().x)
          {
            nodeDesigner.ParentNodeDesigner.MoveChildNode(index1, true);
            flag = true;
          }
          if (!flag)
          {
            int index3 = index1 + 1;
            NodeDesigner nodeDesigner2 = nodeDesigner.ParentNodeDesigner.NodeDesignerForChildIndex(index3);
            if (Object.op_Inequality((Object) nodeDesigner2, (Object) null) && nodeDesigner.Task.get_NodeData().get_Offset().x > nodeDesigner2.Task.get_NodeData().get_Offset().x)
              nodeDesigner.ParentNodeDesigner.MoveChildNode(index1, false);
          }
        }
      }
      if (nodeDesigner.IsParent && !dragChildren)
      {
        ParentTask task = nodeDesigner.Task as ParentTask;
        if (task.get_Children() != null)
        {
          for (int index = 0; index < task.get_Children().Count; ++index)
            (task.get_Children()[index].get_NodeData().get_NodeDesigner() as NodeDesigner).ChangeOffset(Vector2.op_UnaryNegation(delta));
        }
      }
      this.MarkNodeDirty(nodeDesigner);
    }

    public bool DrawNodes(Vector2 mousePosition, Vector2 offset)
    {
      if (Object.op_Equality((Object) this.mEntryNode, (Object) null))
        return false;
      this.mEntryNode.DrawNodeConnection(offset, false);
      if (Object.op_Inequality((Object) this.mRootNode, (Object) null))
        this.DrawNodeConnectionChildren(this.mRootNode, offset, this.mRootNode.Task.get_Disabled());
      for (int index = 0; index < this.mDetachedNodes.Count; ++index)
        this.DrawNodeConnectionChildren(this.mDetachedNodes[index], offset, this.mDetachedNodes[index].Task.get_Disabled());
      for (int index = 0; index < this.mSelectedNodeConnections.Count; ++index)
        this.mSelectedNodeConnections[index].DrawConnection(offset, this.mSelectedNodeConnections[index].OriginatingNodeDesigner.IsDisabled());
      if (Vector2.op_Inequality(mousePosition, new Vector2(-1f, -1f)) && Object.op_Inequality((Object) this.mActiveNodeConnection, (Object) null))
      {
        this.mActiveNodeConnection.HorizontalHeight = (float) ((this.mActiveNodeConnection.OriginatingNodeDesigner.GetConnectionPosition(offset, this.mActiveNodeConnection.NodeConnectionType).y + mousePosition.y) / 2.0);
        this.mActiveNodeConnection.DrawConnection(this.mActiveNodeConnection.OriginatingNodeDesigner.GetConnectionPosition(offset, this.mActiveNodeConnection.NodeConnectionType), mousePosition, this.mActiveNodeConnection.NodeConnectionType == NodeConnectionType.Outgoing && this.mActiveNodeConnection.OriginatingNodeDesigner.IsDisabled());
      }
      this.mEntryNode.DrawNode(offset, false, false);
      bool flag = false;
      if (Object.op_Inequality((Object) this.mRootNode, (Object) null) && this.DrawNodeChildren(this.mRootNode, offset, this.mRootNode.Task.get_Disabled()))
        flag = true;
      for (int index = 0; index < this.mDetachedNodes.Count; ++index)
      {
        if (this.DrawNodeChildren(this.mDetachedNodes[index], offset, this.mDetachedNodes[index].Task.get_Disabled()))
          flag = true;
      }
      for (int index = 0; index < this.mSelectedNodes.Count; ++index)
      {
        if (this.mSelectedNodes[index].DrawNode(offset, true, this.mSelectedNodes[index].IsDisabled()))
          flag = true;
      }
      if (Object.op_Inequality((Object) this.mRootNode, (Object) null))
        this.DrawNodeCommentChildren(this.mRootNode, offset);
      for (int index = 0; index < this.mDetachedNodes.Count; ++index)
        this.DrawNodeCommentChildren(this.mDetachedNodes[index], offset);
      return flag;
    }

    private bool DrawNodeChildren(NodeDesigner nodeDesigner, Vector2 offset, bool disabledNode)
    {
      if (Object.op_Equality((Object) nodeDesigner, (Object) null))
        return false;
      bool flag = false;
      if (nodeDesigner.DrawNode(offset, false, disabledNode))
        flag = true;
      if (nodeDesigner.IsParent)
      {
        ParentTask task = nodeDesigner.Task as ParentTask;
        if (!((Task) task).get_NodeData().get_Collapsed() && task.get_Children() != null)
        {
          for (int index = task.get_Children().Count - 1; index > -1; --index)
          {
            if (task.get_Children()[index] != null && this.DrawNodeChildren(task.get_Children()[index].get_NodeData().get_NodeDesigner() as NodeDesigner, offset, ((Task) task).get_Disabled() || disabledNode))
              flag = true;
          }
        }
      }
      return flag;
    }

    private void DrawNodeConnectionChildren(
      NodeDesigner nodeDesigner,
      Vector2 offset,
      bool disabledNode)
    {
      if (Object.op_Equality((Object) nodeDesigner, (Object) null) || nodeDesigner.Task.get_NodeData().get_Collapsed())
        return;
      nodeDesigner.DrawNodeConnection(offset, nodeDesigner.Task.get_Disabled() || disabledNode);
      if (!nodeDesigner.IsParent)
        return;
      ParentTask task = nodeDesigner.Task as ParentTask;
      if (task.get_Children() == null)
        return;
      for (int index = 0; index < task.get_Children().Count; ++index)
      {
        if (task.get_Children()[index] != null)
          this.DrawNodeConnectionChildren(task.get_Children()[index].get_NodeData().get_NodeDesigner() as NodeDesigner, offset, ((Task) task).get_Disabled() || disabledNode);
      }
    }

    private void DrawNodeCommentChildren(NodeDesigner nodeDesigner, Vector2 offset)
    {
      if (Object.op_Equality((Object) nodeDesigner, (Object) null))
        return;
      nodeDesigner.DrawNodeComment(offset);
      if (!nodeDesigner.IsParent)
        return;
      ParentTask task = nodeDesigner.Task as ParentTask;
      if (((Task) task).get_NodeData().get_Collapsed() || task.get_Children() == null)
        return;
      for (int index = 0; index < task.get_Children().Count; ++index)
      {
        if (task.get_Children()[index] != null)
          this.DrawNodeCommentChildren(task.get_Children()[index].get_NodeData().get_NodeDesigner() as NodeDesigner, offset);
      }
    }

    private void RemoveNode(NodeDesigner nodeDesigner)
    {
      if (nodeDesigner.IsEntryDisplay)
        return;
      if (nodeDesigner.IsParent)
      {
        for (int index = 0; index < nodeDesigner.OutgoingNodeConnections.Count; ++index)
        {
          NodeDesigner destinationNodeDesigner = nodeDesigner.OutgoingNodeConnections[index].DestinationNodeDesigner;
          this.mDetachedNodes.Add(destinationNodeDesigner);
          destinationNodeDesigner.Task.get_NodeData().set_Offset(destinationNodeDesigner.GetAbsolutePosition());
          destinationNodeDesigner.ParentNodeDesigner = (NodeDesigner) null;
        }
      }
      if (Object.op_Inequality((Object) nodeDesigner.ParentNodeDesigner, (Object) null))
        nodeDesigner.ParentNodeDesigner.RemoveChildNode(nodeDesigner);
      if (Object.op_Inequality((Object) this.mRootNode, (Object) null) && this.mRootNode.Equals((object) nodeDesigner))
      {
        this.mEntryNode.RemoveChildNode(nodeDesigner);
        this.mRootNode = (NodeDesigner) null;
      }
      if (Object.op_Inequality((Object) this.mRootNode, (Object) null))
        this.RemoveReferencedTasks(this.mRootNode, nodeDesigner.Task);
      if (this.mDetachedNodes != null)
      {
        for (int index = 0; index < this.mDetachedNodes.Count; ++index)
          this.RemoveReferencedTasks(this.mDetachedNodes[index], nodeDesigner.Task);
      }
      this.mDetachedNodes.Remove(nodeDesigner);
      BehaviorUndo.DestroyObject((Object) nodeDesigner, false);
    }

    private void RemoveReferencedTasks(NodeDesigner nodeDesigner, Task task)
    {
      bool fullSync = false;
      bool doReference = false;
      FieldInfo[] allFields = TaskUtility.GetAllFields(((object) nodeDesigner.Task).GetType());
      for (int index1 = 0; index1 < allFields.Length; ++index1)
      {
        if (!allFields[index1].IsPrivate && !allFields[index1].IsFamily || BehaviorDesignerUtility.HasAttribute(allFields[index1], typeof (SerializeField)))
        {
          if (typeof (IList).IsAssignableFrom(allFields[index1].FieldType))
          {
            if (typeof (Task).IsAssignableFrom(allFields[index1].FieldType.GetElementType()) || allFields[index1].FieldType.IsGenericType && typeof (Task).IsAssignableFrom(allFields[index1].FieldType.GetGenericArguments()[0]))
            {
              Task[] taskArray = allFields[index1].GetValue((object) nodeDesigner.Task) as Task[];
              if (taskArray != null)
              {
                for (int index2 = taskArray.Length - 1; index2 > -1; --index2)
                {
                  if (taskArray[index2] != null && (((object) nodeDesigner.Task).Equals((object) task) || ((object) taskArray[index2]).Equals((object) task)))
                    TaskInspector.ReferenceTasks(nodeDesigner.Task, task, allFields[index1], ref fullSync, ref doReference, false, false);
                }
              }
            }
          }
          else if (typeof (Task).IsAssignableFrom(allFields[index1].FieldType))
          {
            Task task1 = allFields[index1].GetValue((object) nodeDesigner.Task) as Task;
            if (task1 != null && (((object) nodeDesigner.Task).Equals((object) task) || ((object) task1).Equals((object) task)))
              TaskInspector.ReferenceTasks(nodeDesigner.Task, task, allFields[index1], ref fullSync, ref doReference, false, false);
          }
        }
      }
      if (!nodeDesigner.IsParent)
        return;
      ParentTask task2 = nodeDesigner.Task as ParentTask;
      if (task2.get_Children() == null)
        return;
      for (int index = 0; index < task2.get_Children().Count; ++index)
      {
        if (task2.get_Children()[index] != null)
          this.RemoveReferencedTasks(task2.get_Children()[index].get_NodeData().get_NodeDesigner() as NodeDesigner, task);
      }
    }

    public bool NodeCanOriginateConnection(NodeDesigner nodeDesigner, NodeConnection connection)
    {
      if (!nodeDesigner.IsEntryDisplay)
        return true;
      if (nodeDesigner.IsEntryDisplay)
        return connection.NodeConnectionType == NodeConnectionType.Outgoing;
      return false;
    }

    public bool NodeCanAcceptConnection(NodeDesigner nodeDesigner, NodeConnection connection)
    {
      if ((!nodeDesigner.IsEntryDisplay || connection.NodeConnectionType != NodeConnectionType.Incoming) && (nodeDesigner.IsEntryDisplay || !nodeDesigner.IsParent && (nodeDesigner.IsParent || connection.NodeConnectionType != NodeConnectionType.Outgoing)))
        return false;
      if (nodeDesigner.IsEntryDisplay || connection.OriginatingNodeDesigner.IsEntryDisplay)
        return true;
      HashSet<NodeDesigner> set = new HashSet<NodeDesigner>();
      NodeDesigner nodeDesigner1 = connection.NodeConnectionType != NodeConnectionType.Outgoing ? connection.OriginatingNodeDesigner : nodeDesigner;
      NodeDesigner nodeDesigner2 = connection.NodeConnectionType != NodeConnectionType.Outgoing ? nodeDesigner : connection.OriginatingNodeDesigner;
      return !this.CycleExists(nodeDesigner1, ref set) && !set.Contains(nodeDesigner2);
    }

    private bool CycleExists(NodeDesigner nodeDesigner, ref HashSet<NodeDesigner> set)
    {
      if (set.Contains(nodeDesigner))
        return true;
      set.Add(nodeDesigner);
      if (nodeDesigner.IsParent)
      {
        ParentTask task = nodeDesigner.Task as ParentTask;
        if (task.get_Children() != null)
        {
          for (int index = 0; index < task.get_Children().Count; ++index)
          {
            if (this.CycleExists(task.get_Children()[index].get_NodeData().get_NodeDesigner() as NodeDesigner, ref set))
              return true;
          }
        }
      }
      return false;
    }

    public void ConnectNodes(BehaviorSource behaviorSource, NodeDesigner nodeDesigner)
    {
      NodeConnection activeNodeConnection = this.mActiveNodeConnection;
      this.mActiveNodeConnection = (NodeConnection) null;
      if (!Object.op_Inequality((Object) activeNodeConnection, (Object) null) || activeNodeConnection.OriginatingNodeDesigner.Equals((object) nodeDesigner))
        return;
      NodeDesigner originatingNodeDesigner = activeNodeConnection.OriginatingNodeDesigner;
      if (activeNodeConnection.NodeConnectionType == NodeConnectionType.Outgoing)
      {
        this.RemoveParentConnection(nodeDesigner);
        this.CheckForLastConnectionRemoval(originatingNodeDesigner);
        originatingNodeDesigner.AddChildNode(nodeDesigner, activeNodeConnection, true, false);
      }
      else
      {
        this.RemoveParentConnection(originatingNodeDesigner);
        this.CheckForLastConnectionRemoval(nodeDesigner);
        nodeDesigner.AddChildNode(originatingNodeDesigner, activeNodeConnection, true, false);
      }
      if (activeNodeConnection.OriginatingNodeDesigner.IsEntryDisplay)
        this.mRootNode = activeNodeConnection.DestinationNodeDesigner;
      this.mDetachedNodes.Remove(activeNodeConnection.DestinationNodeDesigner);
    }

    private void RemoveParentConnection(NodeDesigner nodeDesigner)
    {
      if (!Object.op_Inequality((Object) nodeDesigner.ParentNodeDesigner, (Object) null))
        return;
      NodeDesigner parentNodeDesigner = nodeDesigner.ParentNodeDesigner;
      NodeConnection nodeConnection = (NodeConnection) null;
      for (int index = 0; index < parentNodeDesigner.OutgoingNodeConnections.Count; ++index)
      {
        if (parentNodeDesigner.OutgoingNodeConnections[index].DestinationNodeDesigner.Equals((object) nodeDesigner))
        {
          nodeConnection = parentNodeDesigner.OutgoingNodeConnections[index];
          break;
        }
      }
      if (!Object.op_Inequality((Object) nodeConnection, (Object) null))
        return;
      this.RemoveConnection(nodeConnection);
    }

    private void CheckForLastConnectionRemoval(NodeDesigner nodeDesigner)
    {
      if (nodeDesigner.IsEntryDisplay)
      {
        if (nodeDesigner.OutgoingNodeConnections.Count != 1)
          return;
        this.RemoveConnection(nodeDesigner.OutgoingNodeConnections[0]);
      }
      else
      {
        ParentTask task = nodeDesigner.Task as ParentTask;
        if (task.get_Children() == null || task.get_Children().Count + 1 <= task.MaxChildren())
          return;
        NodeConnection nodeConnection = (NodeConnection) null;
        for (int index = 0; index < nodeDesigner.OutgoingNodeConnections.Count; ++index)
        {
          if (nodeDesigner.OutgoingNodeConnections[index].DestinationNodeDesigner.Equals((object) (task.get_Children()[task.get_Children().Count - 1].get_NodeData().get_NodeDesigner() as NodeDesigner)))
          {
            nodeConnection = nodeDesigner.OutgoingNodeConnections[index];
            break;
          }
        }
        if (!Object.op_Inequality((Object) nodeConnection, (Object) null))
          return;
        this.RemoveConnection(nodeConnection);
      }
    }

    public void NodeConnectionsAt(
      Vector2 point,
      Vector2 offset,
      ref List<NodeConnection> nodeConnections)
    {
      if (Object.op_Equality((Object) this.mEntryNode, (Object) null))
        return;
      this.NodeChildrenConnectionsAt(this.mEntryNode, point, offset, ref nodeConnections);
      if (Object.op_Inequality((Object) this.mRootNode, (Object) null))
        this.NodeChildrenConnectionsAt(this.mRootNode, point, offset, ref nodeConnections);
      for (int index = 0; index < this.mDetachedNodes.Count; ++index)
        this.NodeChildrenConnectionsAt(this.mDetachedNodes[index], point, offset, ref nodeConnections);
    }

    private void NodeChildrenConnectionsAt(
      NodeDesigner nodeDesigner,
      Vector2 point,
      Vector2 offset,
      ref List<NodeConnection> nodeConnections)
    {
      if (nodeDesigner.Task.get_NodeData().get_Collapsed())
        return;
      nodeDesigner.ConnectionContains(point, offset, ref nodeConnections);
      if (!nodeDesigner.IsParent)
        return;
      ParentTask task = nodeDesigner.Task as ParentTask;
      if (task == null || task.get_Children() == null)
        return;
      for (int index = 0; index < task.get_Children().Count; ++index)
      {
        if (task.get_Children()[index] != null)
          this.NodeChildrenConnectionsAt(task.get_Children()[index].get_NodeData().get_NodeDesigner() as NodeDesigner, point, offset, ref nodeConnections);
      }
    }

    public void RemoveConnection(NodeConnection nodeConnection)
    {
      nodeConnection.DestinationNodeDesigner.Task.get_NodeData().set_Offset(nodeConnection.DestinationNodeDesigner.GetAbsolutePosition());
      this.mDetachedNodes.Add(nodeConnection.DestinationNodeDesigner);
      nodeConnection.OriginatingNodeDesigner.RemoveChildNode(nodeConnection.DestinationNodeDesigner);
      if (!nodeConnection.OriginatingNodeDesigner.IsEntryDisplay)
        return;
      this.mRootNode = (NodeDesigner) null;
    }

    public bool IsSelected(NodeConnection nodeConnection)
    {
      for (int index = 0; index < this.mSelectedNodeConnections.Count; ++index)
      {
        if (((Object) this.mSelectedNodeConnections[index]).Equals((object) nodeConnection))
          return true;
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
      for (int index = 0; index < this.mSelectedNodeConnections.Count; ++index)
        this.mSelectedNodeConnections[index].deselect();
      this.mSelectedNodeConnections.Clear();
    }

    public void GraphDirty()
    {
      if (Object.op_Equality((Object) this.mEntryNode, (Object) null))
        return;
      this.mEntryNode.MarkDirty();
      if (Object.op_Inequality((Object) this.mRootNode, (Object) null))
        this.MarkNodeDirty(this.mRootNode);
      for (int index = this.mDetachedNodes.Count - 1; index > -1; --index)
        this.MarkNodeDirty(this.mDetachedNodes[index]);
    }

    private void MarkNodeDirty(NodeDesigner nodeDesigner)
    {
      nodeDesigner.MarkDirty();
      if (nodeDesigner.IsEntryDisplay)
      {
        if (nodeDesigner.OutgoingNodeConnections.Count <= 0 || !Object.op_Inequality((Object) nodeDesigner.OutgoingNodeConnections[0].DestinationNodeDesigner, (Object) null))
          return;
        this.MarkNodeDirty(nodeDesigner.OutgoingNodeConnections[0].DestinationNodeDesigner);
      }
      else
      {
        if (!nodeDesigner.IsParent)
          return;
        ParentTask task = nodeDesigner.Task as ParentTask;
        if (task.get_Children() == null)
          return;
        for (int index = 0; index < task.get_Children().Count; ++index)
        {
          if (task.get_Children()[index] != null)
            this.MarkNodeDirty(task.get_Children()[index].get_NodeData().get_NodeDesigner() as NodeDesigner);
        }
      }
    }

    public List<BehaviorSource> FindReferencedBehaviors()
    {
      List<BehaviorSource> behaviors = new List<BehaviorSource>();
      if (Object.op_Inequality((Object) this.mRootNode, (Object) null))
        this.FindReferencedBehaviors(this.mRootNode, ref behaviors);
      for (int index = 0; index < this.mDetachedNodes.Count; ++index)
        this.FindReferencedBehaviors(this.mDetachedNodes[index], ref behaviors);
      return behaviors;
    }

    public void FindReferencedBehaviors(
      NodeDesigner nodeDesigner,
      ref List<BehaviorSource> behaviors)
    {
      FieldInfo[] publicFields = TaskUtility.GetPublicFields(((object) nodeDesigner.Task).GetType());
      for (int index1 = 0; index1 < publicFields.Length; ++index1)
      {
        Type fieldType = publicFields[index1].FieldType;
        if (typeof (IList).IsAssignableFrom(fieldType))
        {
          Type type = fieldType;
          Type c;
          if (fieldType.IsGenericType)
          {
            while (!type.IsGenericType)
              type = type.BaseType;
            c = fieldType.GetGenericArguments()[0];
          }
          else
            c = fieldType.GetElementType();
          if (c != null)
          {
            if (typeof (ExternalBehavior).IsAssignableFrom(c) || typeof (Behavior).IsAssignableFrom(c))
            {
              IList list = publicFields[index1].GetValue((object) nodeDesigner.Task) as IList;
              if (list != null)
              {
                for (int index2 = 0; index2 < list.Count; ++index2)
                {
                  if (list[index2] != null)
                  {
                    BehaviorSource behaviorSource;
                    if (list[index2] is ExternalBehavior)
                    {
                      behaviorSource = (list[index2] as ExternalBehavior).get_BehaviorSource();
                      if (behaviorSource.get_Owner() == null)
                        behaviorSource.set_Owner((IBehavior) (list[index2] as ExternalBehavior));
                    }
                    else
                    {
                      behaviorSource = (list[index2] as Behavior).GetBehaviorSource();
                      if (behaviorSource.get_Owner() == null)
                        behaviorSource.set_Owner((IBehavior) (list[index2] as Behavior));
                    }
                    behaviors.Add(behaviorSource);
                  }
                }
              }
            }
            else if (!typeof (Behavior).IsAssignableFrom(c))
              ;
          }
        }
        else if (typeof (ExternalBehavior).IsAssignableFrom(fieldType) || typeof (Behavior).IsAssignableFrom(fieldType))
        {
          object obj = publicFields[index1].GetValue((object) nodeDesigner.Task);
          if (obj != null)
          {
            BehaviorSource behaviorSource;
            if (obj is ExternalBehavior)
            {
              behaviorSource = (obj as ExternalBehavior).get_BehaviorSource();
              if (behaviorSource.get_Owner() == null)
                behaviorSource.set_Owner((IBehavior) (obj as ExternalBehavior));
              behaviors.Add(behaviorSource);
            }
            else
            {
              behaviorSource = (obj as Behavior).GetBehaviorSource();
              if (behaviorSource.get_Owner() == null)
                behaviorSource.set_Owner((IBehavior) (obj as Behavior));
            }
            behaviors.Add(behaviorSource);
          }
        }
      }
      if (!nodeDesigner.IsParent)
        return;
      ParentTask task = nodeDesigner.Task as ParentTask;
      if (task.get_Children() == null)
        return;
      for (int index = 0; index < task.get_Children().Count; ++index)
      {
        if (task.get_Children()[index] != null)
          this.FindReferencedBehaviors(task.get_Children()[index].get_NodeData().get_NodeDesigner() as NodeDesigner, ref behaviors);
      }
    }

    public void SelectAll()
    {
      for (int index = this.mSelectedNodes.Count - 1; index > -1; --index)
        this.Deselect(this.mSelectedNodes[index]);
      if (Object.op_Inequality((Object) this.mRootNode, (Object) null))
        this.SelectAll(this.mRootNode);
      for (int index = this.mDetachedNodes.Count - 1; index > -1; --index)
        this.SelectAll(this.mDetachedNodes[index]);
    }

    private void SelectAll(NodeDesigner nodeDesigner)
    {
      this.Select(nodeDesigner);
      if (!((object) nodeDesigner.Task).GetType().IsSubclassOf(typeof (ParentTask)))
        return;
      ParentTask task = nodeDesigner.Task as ParentTask;
      if (task.get_Children() == null)
        return;
      for (int index = 0; index < task.get_Children().Count; ++index)
        this.SelectAll(task.get_Children()[index].get_NodeData().get_NodeDesigner() as NodeDesigner);
    }

    public void IdentifyNode(NodeDesigner nodeDesigner)
    {
      nodeDesigner.IdentifyNode();
    }

    public List<TaskSerializer> Copy(Vector2 graphOffset, float graphZoom)
    {
      List<TaskSerializer> taskSerializerList = new List<TaskSerializer>();
      for (int index1 = 0; index1 < this.mSelectedNodes.Count; ++index1)
      {
        TaskSerializer taskSerializer;
        if ((taskSerializer = TaskCopier.CopySerialized(this.mSelectedNodes[index1].Task)) != null)
        {
          if (this.mSelectedNodes[index1].IsParent)
          {
            ParentTask task = this.mSelectedNodes[index1].Task as ParentTask;
            if (task.get_Children() != null)
            {
              List<int> intList = new List<int>();
              for (int index2 = 0; index2 < task.get_Children().Count; ++index2)
              {
                int num;
                if ((num = this.mSelectedNodes.IndexOf(task.get_Children()[index2].get_NodeData().get_NodeDesigner() as NodeDesigner)) != -1)
                  intList.Add(num);
              }
              taskSerializer.childrenIndex = intList;
            }
          }
          taskSerializer.offset = Vector2.op_Multiply(Vector2.op_Addition(taskSerializer.offset, graphOffset), graphZoom);
          taskSerializerList.Add(taskSerializer);
        }
      }
      if (taskSerializerList.Count > 0)
        return taskSerializerList;
      return (List<TaskSerializer>) null;
    }

    public bool Paste(
      BehaviorSource behaviorSource,
      Vector3 position,
      List<TaskSerializer> copiedTasks,
      Vector2 graphOffset,
      float graphZoom)
    {
      if (copiedTasks == null || copiedTasks.Count == 0)
        return false;
      this.ClearNodeSelection();
      this.ClearConnectionSelection();
      this.RemapIDs();
      List<NodeDesigner> nodeDesignerList = new List<NodeDesigner>();
      for (int index = 0; index < copiedTasks.Count; ++index)
      {
        TaskSerializer copiedTask = copiedTasks[index];
        Task task = TaskCopier.PasteTask(behaviorSource, copiedTask);
        NodeDesigner instance = (NodeDesigner) ScriptableObject.CreateInstance<NodeDesigner>();
        instance.LoadTask(task, behaviorSource.get_Owner() == null ? (Behavior) null : behaviorSource.get_Owner().GetObject() as Behavior, ref this.mNextTaskID);
        instance.Task.get_NodeData().set_Offset(Vector2.op_Subtraction(Vector2.op_Division(copiedTask.offset, graphZoom), graphOffset));
        nodeDesignerList.Add(instance);
        this.mDetachedNodes.Add(instance);
        this.Select(instance);
      }
      for (int index1 = 0; index1 < copiedTasks.Count; ++index1)
      {
        TaskSerializer copiedTask = copiedTasks[index1];
        if (copiedTask.childrenIndex != null)
        {
          for (int index2 = 0; index2 < copiedTask.childrenIndex.Count; ++index2)
          {
            NodeDesigner nodeDesigner = nodeDesignerList[index1];
            NodeConnection instance = (NodeConnection) ScriptableObject.CreateInstance<NodeConnection>();
            instance.LoadConnection(nodeDesigner, NodeConnectionType.Outgoing);
            nodeDesigner.AddChildNode(nodeDesignerList[copiedTask.childrenIndex[index2]], instance, true, false);
            this.mDetachedNodes.Remove(nodeDesignerList[copiedTask.childrenIndex[index2]]);
          }
        }
      }
      if (Object.op_Equality((Object) this.mEntryNode, (Object) null))
      {
        Task instance = Activator.CreateInstance(TaskUtility.GetTypeWithinAssembly("BehaviorDesigner.Runtime.Tasks.EntryTask")) as Task;
        this.mEntryNode = (NodeDesigner) ScriptableObject.CreateInstance<NodeDesigner>();
        this.mEntryNode.LoadNode(instance, behaviorSource, new Vector2((float) position.x, (float) (position.y - 120.0)), ref this.mNextTaskID);
        this.mEntryNode.MakeEntryDisplay();
        if (this.mDetachedNodes.Count > 0)
        {
          this.mActiveNodeConnection = (NodeConnection) ScriptableObject.CreateInstance<NodeConnection>();
          this.mActiveNodeConnection.LoadConnection(this.mEntryNode, NodeConnectionType.Outgoing);
          this.ConnectNodes(behaviorSource, this.mDetachedNodes[0]);
        }
      }
      this.Save(behaviorSource);
      return true;
    }

    public bool Delete(
      BehaviorSource behaviorSource,
      BehaviorDesignerWindow.TaskCallbackHandler callback)
    {
      bool flag = false;
      if (this.mSelectedNodeConnections != null)
      {
        for (int index = 0; index < this.mSelectedNodeConnections.Count; ++index)
          this.RemoveConnection(this.mSelectedNodeConnections[index]);
        this.mSelectedNodeConnections.Clear();
        flag = true;
      }
      if (this.mSelectedNodes != null)
      {
        for (int index = 0; index < this.mSelectedNodes.Count; ++index)
        {
          if (callback != null)
            callback(behaviorSource, this.mSelectedNodes[index].Task);
          this.RemoveNode(this.mSelectedNodes[index]);
        }
        this.mSelectedNodes.Clear();
        flag = true;
      }
      if (flag)
      {
        BehaviorUndo.RegisterUndo(nameof (Delete), behaviorSource.get_Owner().GetObject());
        TaskReferences.CheckReferences(behaviorSource);
        this.Save(behaviorSource);
      }
      return flag;
    }

    public bool RemoveSharedVariableReferences(SharedVariable sharedVariable)
    {
      if (Object.op_Equality((Object) this.mEntryNode, (Object) null))
        return false;
      bool flag = false;
      if (Object.op_Inequality((Object) this.mRootNode, (Object) null) && this.RemoveSharedVariableReference(this.mRootNode, sharedVariable))
        flag = true;
      if (this.mDetachedNodes != null)
      {
        for (int index = 0; index < this.mDetachedNodes.Count; ++index)
        {
          if (this.RemoveSharedVariableReference(this.mDetachedNodes[index], sharedVariable))
            flag = true;
        }
      }
      return flag;
    }

    private bool RemoveSharedVariableReference(
      NodeDesigner nodeDesigner,
      SharedVariable sharedVariable)
    {
      bool flag = false;
      FieldInfo[] allFields = TaskUtility.GetAllFields(((object) nodeDesigner.Task).GetType());
      for (int index = 0; index < allFields.Length; ++index)
      {
        if (typeof (SharedVariable).IsAssignableFrom(allFields[index].FieldType))
        {
          SharedVariable sharedVariable1 = allFields[index].GetValue((object) nodeDesigner.Task) as SharedVariable;
          if (sharedVariable1 != null && !string.IsNullOrEmpty(sharedVariable1.get_Name()) && (sharedVariable1.get_IsGlobal() == sharedVariable.get_IsGlobal() && sharedVariable1.get_Name().Equals(sharedVariable.get_Name())))
          {
            if (!allFields[index].FieldType.IsAbstract)
            {
              SharedVariable instance = Activator.CreateInstance(allFields[index].FieldType) as SharedVariable;
              instance.set_IsShared(true);
              allFields[index].SetValue((object) nodeDesigner.Task, (object) instance);
            }
            flag = true;
          }
        }
      }
      if (nodeDesigner.IsParent)
      {
        ParentTask task = nodeDesigner.Task as ParentTask;
        if (task.get_Children() != null)
        {
          for (int index = 0; index < task.get_Children().Count; ++index)
          {
            if (task.get_Children()[index] != null && this.RemoveSharedVariableReference(task.get_Children()[index].get_NodeData().get_NodeDesigner() as NodeDesigner, sharedVariable))
              flag = true;
          }
        }
      }
      return flag;
    }

    private void RemapIDs()
    {
      if (Object.op_Equality((Object) this.mEntryNode, (Object) null))
        return;
      this.mNextTaskID = 0;
      this.mEntryNode.SetID(ref this.mNextTaskID);
      if (Object.op_Inequality((Object) this.mRootNode, (Object) null))
        this.mRootNode.SetID(ref this.mNextTaskID);
      for (int index = 0; index < this.mDetachedNodes.Count; ++index)
        this.mDetachedNodes[index].SetID(ref this.mNextTaskID);
      this.mNodeSelectedID.Clear();
      for (int index = 0; index < this.mSelectedNodes.Count; ++index)
        this.mNodeSelectedID.Add(this.mSelectedNodes[index].Task.get_ID());
    }

    public Rect GraphSize(Vector3 offset)
    {
      if (Object.op_Equality((Object) this.mEntryNode, (Object) null))
        return (Rect) null;
      Rect minMaxRect = (Rect) null;
      ((Rect) ref minMaxRect).set_xMin(float.MaxValue);
      ((Rect) ref minMaxRect).set_xMax(float.MinValue);
      ((Rect) ref minMaxRect).set_yMin(float.MaxValue);
      ((Rect) ref minMaxRect).set_yMax(float.MinValue);
      this.GetNodeMinMax(Vector2.op_Implicit(offset), this.mEntryNode, ref minMaxRect);
      if (Object.op_Inequality((Object) this.mRootNode, (Object) null))
        this.GetNodeMinMax(Vector2.op_Implicit(offset), this.mRootNode, ref minMaxRect);
      for (int index = 0; index < this.mDetachedNodes.Count; ++index)
        this.GetNodeMinMax(Vector2.op_Implicit(offset), this.mDetachedNodes[index], ref minMaxRect);
      return minMaxRect;
    }

    private void GetNodeMinMax(Vector2 offset, NodeDesigner nodeDesigner, ref Rect minMaxRect)
    {
      Rect rect = nodeDesigner.Rectangle(offset, true, true);
      if ((double) ((Rect) ref rect).get_xMin() < (double) ((Rect) ref minMaxRect).get_xMin())
        ((Rect) ref minMaxRect).set_xMin(((Rect) ref rect).get_xMin());
      if ((double) ((Rect) ref rect).get_yMin() < (double) ((Rect) ref minMaxRect).get_yMin())
        ((Rect) ref minMaxRect).set_yMin(((Rect) ref rect).get_yMin());
      if ((double) ((Rect) ref rect).get_xMax() > (double) ((Rect) ref minMaxRect).get_xMax())
        ((Rect) ref minMaxRect).set_xMax(((Rect) ref rect).get_xMax());
      if ((double) ((Rect) ref rect).get_yMax() > (double) ((Rect) ref minMaxRect).get_yMax())
        ((Rect) ref minMaxRect).set_yMax(((Rect) ref rect).get_yMax());
      if (!nodeDesigner.IsParent)
        return;
      ParentTask task = nodeDesigner.Task as ParentTask;
      if (task.get_Children() == null)
        return;
      for (int index = 0; index < task.get_Children().Count; ++index)
        this.GetNodeMinMax(offset, task.get_Children()[index].get_NodeData().get_NodeDesigner() as NodeDesigner, ref minMaxRect);
    }

    public void Save(BehaviorSource behaviorSource)
    {
      if (object.ReferenceEquals((object) behaviorSource.get_Owner().GetObject(), (object) null))
        return;
      this.RemapIDs();
      List<Task> taskList = new List<Task>();
      for (int index = 0; index < this.mDetachedNodes.Count; ++index)
        taskList.Add(this.mDetachedNodes[index].Task);
      behaviorSource.Save(!Object.op_Inequality((Object) this.mEntryNode, (Object) null) ? (Task) null : this.mEntryNode.Task, !Object.op_Inequality((Object) this.mRootNode, (Object) null) ? (Task) null : this.mRootNode.Task, taskList);
      if (BehaviorDesignerPreferences.GetBool(BDPreferences.BinarySerialization))
        BinarySerialization.Save(behaviorSource);
      else
        JSONSerialization.Save(behaviorSource);
    }

    public bool Load(BehaviorSource behaviorSource, bool loadPrevBehavior, Vector2 nodePosition)
    {
      if (behaviorSource == null)
      {
        this.Clear(false);
        return false;
      }
      this.DestroyNodeDesigners();
      if (behaviorSource.get_Owner() != null && behaviorSource.get_Owner() is Behavior && Object.op_Inequality((Object) (behaviorSource.get_Owner() as Behavior).get_ExternalBehavior(), (Object) null))
      {
        List<SharedVariable> sharedVariableList = (List<SharedVariable>) null;
        bool flag = !Application.get_isPlaying();
        if (Application.get_isPlaying() && !(behaviorSource.get_Owner() as Behavior).get_HasInheritedVariables())
        {
          behaviorSource.CheckForSerialization(true, (BehaviorSource) null);
          sharedVariableList = behaviorSource.GetAllVariables();
          (behaviorSource.get_Owner() as Behavior).set_HasInheritedVariables(true);
          flag = true;
        }
        ExternalBehavior externalBehavior = (behaviorSource.get_Owner() as Behavior).get_ExternalBehavior();
        externalBehavior.get_BehaviorSource().set_Owner((IBehavior) externalBehavior);
        externalBehavior.get_BehaviorSource().CheckForSerialization(flag, behaviorSource);
        if (sharedVariableList != null)
        {
          for (int index = 0; index < sharedVariableList.Count; ++index)
            behaviorSource.SetVariable(sharedVariableList[index].get_Name(), sharedVariableList[index]);
        }
      }
      else
        behaviorSource.CheckForSerialization(!Application.get_isPlaying(), (BehaviorSource) null);
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
          for (int index = 0; index < this.mPrevNodeSelectedID.Length; ++index)
            this.mNodeSelectedID.Add(this.mPrevNodeSelectedID[index]);
          this.mPrevNodeSelectedID = (int[]) null;
        }
      }
      else
        this.Clear(false);
      this.mNextTaskID = 0;
      this.mEntryNode = (NodeDesigner) null;
      this.mRootNode = (NodeDesigner) null;
      this.mDetachedNodes.Clear();
      Task instance1;
      Task task;
      List<Task> taskList;
      behaviorSource.Load(ref instance1, ref task, ref taskList);
      if (BehaviorDesignerUtility.AnyNullTasks(behaviorSource) || behaviorSource.get_TaskData() != null && BehaviorDesignerUtility.HasRootTask((string) behaviorSource.get_TaskData().JSONSerialization) && behaviorSource.get_RootTask() == null)
      {
        behaviorSource.CheckForSerialization(true, (BehaviorSource) null);
        behaviorSource.Load(ref instance1, ref task, ref taskList);
      }
      if (instance1 == null)
      {
        if (task != null || taskList != null && taskList.Count > 0)
        {
          behaviorSource.set_EntryTask(instance1 = Activator.CreateInstance(TaskUtility.GetTypeWithinAssembly("BehaviorDesigner.Runtime.Tasks.EntryTask"), true) as Task);
          this.mEntryNode = (NodeDesigner) ScriptableObject.CreateInstance<NodeDesigner>();
          if (task != null)
            this.mEntryNode.LoadNode(instance1, behaviorSource, new Vector2((float) task.get_NodeData().get_Offset().x, (float) (task.get_NodeData().get_Offset().y - 120.0)), ref this.mNextTaskID);
          else
            this.mEntryNode.LoadNode(instance1, behaviorSource, new Vector2((float) nodePosition.x, (float) (nodePosition.y - 120.0)), ref this.mNextTaskID);
          this.mEntryNode.MakeEntryDisplay();
        }
      }
      else
      {
        this.mEntryNode = (NodeDesigner) ScriptableObject.CreateInstance<NodeDesigner>();
        this.mEntryNode.LoadTask(instance1, behaviorSource.get_Owner() == null ? (Behavior) null : behaviorSource.get_Owner().GetObject() as Behavior, ref this.mNextTaskID);
        this.mEntryNode.MakeEntryDisplay();
      }
      if (task != null)
      {
        this.mRootNode = (NodeDesigner) ScriptableObject.CreateInstance<NodeDesigner>();
        this.mRootNode.LoadTask(task, behaviorSource.get_Owner() == null ? (Behavior) null : behaviorSource.get_Owner().GetObject() as Behavior, ref this.mNextTaskID);
        NodeConnection instance2 = (NodeConnection) ScriptableObject.CreateInstance<NodeConnection>();
        instance2.LoadConnection(this.mEntryNode, NodeConnectionType.Fixed);
        this.mEntryNode.AddChildNode(this.mRootNode, instance2, false, false);
        this.LoadNodeSelection(this.mRootNode);
        if (this.mEntryNode.OutgoingNodeConnections.Count == 0)
        {
          this.mActiveNodeConnection = (NodeConnection) ScriptableObject.CreateInstance<NodeConnection>();
          this.mActiveNodeConnection.LoadConnection(this.mEntryNode, NodeConnectionType.Outgoing);
          this.ConnectNodes(behaviorSource, this.mRootNode);
        }
      }
      if (taskList != null)
      {
        for (int index = 0; index < taskList.Count; ++index)
        {
          if (taskList[index] != null)
          {
            NodeDesigner instance2 = (NodeDesigner) ScriptableObject.CreateInstance<NodeDesigner>();
            instance2.LoadTask(taskList[index], behaviorSource.get_Owner() == null ? (Behavior) null : behaviorSource.get_Owner().GetObject() as Behavior, ref this.mNextTaskID);
            this.mDetachedNodes.Add(instance2);
            this.LoadNodeSelection(instance2);
          }
        }
      }
      return true;
    }

    public bool HasEntryNode()
    {
      if (Object.op_Inequality((Object) this.mEntryNode, (Object) null))
        return this.mEntryNode.Task != null;
      return false;
    }

    public Vector2 EntryNodeOffset()
    {
      return this.mEntryNode.Task.get_NodeData().get_Offset();
    }

    public void SetStartOffset(Vector2 offset)
    {
      Vector2 vector2 = Vector2.op_Subtraction(offset, this.mEntryNode.Task.get_NodeData().get_Offset());
      this.mEntryNode.Task.get_NodeData().set_Offset(offset);
      for (int index = 0; index < this.mDetachedNodes.Count; ++index)
      {
        NodeData nodeData = this.mDetachedNodes[index].Task.get_NodeData();
        nodeData.set_Offset(Vector2.op_Addition(nodeData.get_Offset(), vector2));
      }
    }

    private void LoadNodeSelection(NodeDesigner nodeDesigner)
    {
      if (Object.op_Equality((Object) nodeDesigner, (Object) null))
        return;
      if (this.mNodeSelectedID != null && this.mNodeSelectedID.Contains(nodeDesigner.Task.get_ID()))
        this.Select(nodeDesigner, false);
      if (!nodeDesigner.IsParent)
        return;
      ParentTask task = nodeDesigner.Task as ParentTask;
      if (task.get_Children() == null)
        return;
      for (int index = 0; index < task.get_Children().Count; ++index)
      {
        if (task.get_Children()[index] != null && task.get_Children()[index].get_NodeData() != null)
          this.LoadNodeSelection(task.get_Children()[index].get_NodeData().get_NodeDesigner() as NodeDesigner);
      }
    }

    public void Clear(bool saveSelectedNodes)
    {
      if (saveSelectedNodes)
      {
        if (this.mNodeSelectedID.Count > 0)
          this.mPrevNodeSelectedID = this.mNodeSelectedID.ToArray();
      }
      else
        this.mPrevNodeSelectedID = (int[]) null;
      this.mNodeSelectedID.Clear();
      this.mSelectedNodes.Clear();
      this.mSelectedNodeConnections.Clear();
      this.DestroyNodeDesigners();
    }

    public void DestroyNodeDesigners()
    {
      if (Object.op_Inequality((Object) this.mEntryNode, (Object) null))
        this.Clear(this.mEntryNode);
      if (Object.op_Inequality((Object) this.mRootNode, (Object) null))
        this.Clear(this.mRootNode);
      for (int index = this.mDetachedNodes.Count - 1; index > -1; --index)
        this.Clear(this.mDetachedNodes[index]);
      this.mEntryNode = (NodeDesigner) null;
      this.mRootNode = (NodeDesigner) null;
      this.mDetachedNodes = new List<NodeDesigner>();
    }

    private void Clear(NodeDesigner nodeDesigner)
    {
      if (Object.op_Equality((Object) nodeDesigner, (Object) null))
        return;
      if (nodeDesigner.IsParent)
      {
        ParentTask task = nodeDesigner.Task as ParentTask;
        if (task != null && task.get_Children() != null)
        {
          for (int index = task.get_Children().Count - 1; index > -1; --index)
          {
            if (task.get_Children()[index] != null)
              this.Clear(task.get_Children()[index].get_NodeData().get_NodeDesigner() as NodeDesigner);
          }
        }
      }
      nodeDesigner.DestroyConnections();
      Object.DestroyImmediate((Object) nodeDesigner, true);
    }
  }
}
