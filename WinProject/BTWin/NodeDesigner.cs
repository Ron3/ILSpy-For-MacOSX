// Decompiled with JetBrains decompiler
// Type: BehaviorDesigner.Editor.NodeDesigner
// Assembly: BehaviorDesignerEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 99CE4D00-DFA2-42D1-ABFC-D630AB4C1372
// Assembly location: C:\Users\Ron\Desktop\bt\BehaviorDesignerEditor.dll

using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace BehaviorDesigner.Editor
{
  [Serializable]
  public class NodeDesigner : ScriptableObject
  {
    [SerializeField]
    private Task mTask;
    [SerializeField]
    private bool mSelected;
    private int mIdentifyUpdateCount;
    [SerializeField]
    private bool mConnectionIsDirty;
    private bool mRectIsDirty;
    private bool mIncomingRectIsDirty;
    private bool mOutgoingRectIsDirty;
    [SerializeField]
    private bool isParent;
    [SerializeField]
    private bool isEntryDisplay;
    [SerializeField]
    private bool showReferenceIcon;
    private bool showHoverBar;
    private bool hasError;
    [SerializeField]
    private string taskName;
    private Rect mRectangle;
    private Rect mOutgoingRectangle;
    private Rect mIncomingRectangle;
    private bool prevRunningState;
    private int prevCommentLength;
    private List<int> prevWatchedFieldsLength;
    private int prevFriendlyNameLength;
    [SerializeField]
    private NodeDesigner parentNodeDesigner;
    [SerializeField]
    private List<NodeConnection> outgoingNodeConnections;
    private bool mCacheIsDirty;
    private readonly Color grayColor;
    private Rect nodeCollapsedTextureRect;
    private Rect iconTextureRect;
    private Rect titleRect;
    private Rect breakpointTextureRect;
    private Rect errorTextureRect;
    private Rect referenceTextureRect;
    private Rect conditionalAbortTextureRect;
    private Rect conditionalAbortLowerPriorityTextureRect;
    private Rect disabledButtonTextureRect;
    private Rect collapseButtonTextureRect;
    private Rect incomingConnectionTextureRect;
    private Rect outgoingConnectionTextureRect;
    private Rect successReevaluatingExecutionStatusTextureRect;
    private Rect successExecutionStatusTextureRect;
    private Rect failureExecutionStatusTextureRect;
    private Rect iconBorderTextureRect;
    private Rect watchedFieldRect;
    private Rect watchedFieldNamesRect;
    private Rect watchedFieldValuesRect;
    private Rect commentRect;
    private Rect commentLabelRect;

    public NodeDesigner()
    {
      //base.\u002Ector();
    }

    public Task Task
    {
      get
      {
        return this.mTask;
      }
      set
      {
        this.mTask = value;
        this.Init();
      }
    }

    public void Select()
    {
      if (this.isEntryDisplay)
        return;
      this.mSelected = true;
    }

    public void Deselect()
    {
      this.mSelected = false;
    }

    public void MarkDirty()
    {
      this.mConnectionIsDirty = true;
      this.mRectIsDirty = true;
      this.mIncomingRectIsDirty = true;
      this.mOutgoingRectIsDirty = true;
    }

    public bool IsParent
    {
      get
      {
        return this.isParent;
      }
    }

    public bool IsEntryDisplay
    {
      get
      {
        return this.isEntryDisplay;
      }
    }

    public bool ShowReferenceIcon
    {
      set
      {
        this.showReferenceIcon = value;
      }
    }

    public bool ShowHoverBar
    {
      get
      {
        return this.showHoverBar;
      }
      set
      {
        this.showHoverBar = value;
      }
    }

    public bool HasError
    {
      set
      {
        this.hasError = value;
      }
    }

    public NodeDesigner ParentNodeDesigner
    {
      get
      {
        return this.parentNodeDesigner;
      }
      set
      {
        this.parentNodeDesigner = value;
      }
    }

    public List<NodeConnection> OutgoingNodeConnections
    {
      get
      {
        return this.outgoingNodeConnections;
      }
    }

    public Rect IncomingConnectionRect(Vector2 offset)
    {
      if (!this.mIncomingRectIsDirty)
        return this.mIncomingRectangle;
      Rect rect = this.Rectangle(offset, false, false);
      this.mIncomingRectangle = new Rect(((Rect) ref rect).get_x() + (float) (((double) ((Rect) ref rect).get_width() - 42.0) / 2.0), ((Rect) ref rect).get_y() - 14f, 42f, 14f);
      this.mIncomingRectIsDirty = false;
      return this.mIncomingRectangle;
    }

    public Rect OutgoingConnectionRect(Vector2 offset)
    {
      if (!this.mOutgoingRectIsDirty)
        return this.mOutgoingRectangle;
      Rect rect = this.Rectangle(offset, false, false);
      this.mOutgoingRectangle = new Rect(((Rect) ref rect).get_x() + (float) (((double) ((Rect) ref rect).get_width() - 42.0) / 2.0), ((Rect) ref rect).get_yMax(), 42f, 16f);
      this.mOutgoingRectIsDirty = false;
      return this.mOutgoingRectangle;
    }

    public void OnEnable()
    {
      ((Object) this).set_hideFlags((HideFlags) 61);
    }

    public void LoadTask(Task task, Behavior owner, ref int id)
    {
      if (task == null)
        return;
      this.mTask = task;
      this.mTask.set_Owner(owner);
      Task mTask1 = this.mTask;
      int num1;
      id = (num1 = id) + 1;
      int num2 = num1;
      mTask1.set_ID(num2);
      this.mTask.get_NodeData().set_NodeDesigner((object) this);
      this.mTask.get_NodeData().InitWatchedFields(this.mTask);
      if (!this.mTask.get_NodeData().get_FriendlyName().Equals(string.Empty))
      {
        this.mTask.set_FriendlyName(this.mTask.get_NodeData().get_FriendlyName());
        this.mTask.get_NodeData().set_FriendlyName(string.Empty);
      }
      this.LoadTaskIcon();
      this.Init();
      RequiredComponentAttribute[] customAttributes;
      if (Object.op_Inequality((Object) this.mTask.get_Owner(), (Object) null) && (customAttributes = ((object) this.mTask).GetType().GetCustomAttributes(typeof (RequiredComponentAttribute), true) as RequiredComponentAttribute[]).Length > 0)
      {
        Type componentType = customAttributes[0].get_ComponentType();
        if (typeof (Component).IsAssignableFrom(componentType) && Object.op_Equality((Object) ((Component) this.mTask.get_Owner()).get_gameObject().GetComponent(componentType), (Object) null))
          ((Component) this.mTask.get_Owner()).get_gameObject().AddComponent(componentType);
      }
      List<Type> baseClasses = FieldInspector.GetBaseClasses(((object) this.mTask).GetType());
      BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
      for (int index1 = baseClasses.Count - 1; index1 > -1; --index1)
      {
        FieldInfo[] fields = baseClasses[index1].GetFields(bindingAttr);
        for (int index2 = 0; index2 < fields.Length; ++index2)
        {
          if (typeof (SharedVariable).IsAssignableFrom(fields[index2].FieldType) && !fields[index2].FieldType.IsAbstract)
          {
            SharedVariable sharedVariable = fields[index2].GetValue((object) this.mTask) as SharedVariable ?? Activator.CreateInstance(fields[index2].FieldType) as SharedVariable;
            if (TaskUtility.HasAttribute(fields[index2], typeof (RequiredFieldAttribute)) || TaskUtility.HasAttribute(fields[index2], typeof (SharedRequiredAttribute)))
              sharedVariable.set_IsShared(true);
            fields[index2].SetValue((object) this.mTask, (object) sharedVariable);
          }
        }
      }
      if (!this.isParent)
        return;
      ParentTask mTask2 = this.mTask as ParentTask;
      if (mTask2.get_Children() != null)
      {
        for (int replaceNodeIndex = 0; replaceNodeIndex < mTask2.get_Children().Count; ++replaceNodeIndex)
        {
          NodeDesigner instance1 = (NodeDesigner) ScriptableObject.CreateInstance<NodeDesigner>();
          instance1.LoadTask(mTask2.get_Children()[replaceNodeIndex], owner, ref id);
          NodeConnection instance2 = (NodeConnection) ScriptableObject.CreateInstance<NodeConnection>();
          instance2.LoadConnection(this, NodeConnectionType.Fixed);
          this.AddChildNode(instance1, instance2, true, true, replaceNodeIndex);
        }
      }
      this.mConnectionIsDirty = true;
    }

    public void LoadNode(Task task, BehaviorSource behaviorSource, Vector2 offset, ref int id)
    {
      this.mTask = task;
      this.mTask.set_Owner(behaviorSource.get_Owner() as Behavior);
      Task mTask = this.mTask;
      int num1;
      id = (num1 = id) + 1;
      int num2 = num1;
      mTask.set_ID(num2);
      this.mTask.set_NodeData(new NodeData());
      this.mTask.get_NodeData().set_Offset(offset);
      this.mTask.get_NodeData().set_NodeDesigner((object) this);
      this.LoadTaskIcon();
      this.Init();
      this.mTask.set_FriendlyName(this.taskName);
      RequiredComponentAttribute[] customAttributes;
      if (Object.op_Inequality((Object) this.mTask.get_Owner(), (Object) null) && (customAttributes = ((object) this.mTask).GetType().GetCustomAttributes(typeof (RequiredComponentAttribute), true) as RequiredComponentAttribute[]).Length > 0)
      {
        Type componentType = customAttributes[0].get_ComponentType();
        if (typeof (Component).IsAssignableFrom(componentType) && Object.op_Equality((Object) ((Component) this.mTask.get_Owner()).get_gameObject().GetComponent(componentType), (Object) null))
          ((Component) this.mTask.get_Owner()).get_gameObject().AddComponent(componentType);
      }
      List<Type> baseClasses = FieldInspector.GetBaseClasses(((object) this.mTask).GetType());
      BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
      for (int index1 = baseClasses.Count - 1; index1 > -1; --index1)
      {
        FieldInfo[] fields = baseClasses[index1].GetFields(bindingAttr);
        for (int index2 = 0; index2 < fields.Length; ++index2)
        {
          if (typeof (SharedVariable).IsAssignableFrom(fields[index2].FieldType) && !fields[index2].FieldType.IsAbstract)
          {
            SharedVariable sharedVariable = fields[index2].GetValue((object) this.mTask) as SharedVariable ?? Activator.CreateInstance(fields[index2].FieldType) as SharedVariable;
            if (TaskUtility.HasAttribute(fields[index2], typeof (RequiredFieldAttribute)) || TaskUtility.HasAttribute(fields[index2], typeof (SharedRequiredAttribute)))
              sharedVariable.set_IsShared(true);
            fields[index2].SetValue((object) this.mTask, (object) sharedVariable);
          }
        }
      }
    }

    private void LoadTaskIcon()
    {
      this.mTask.get_NodeData().set_Icon((Texture) null);
      TaskIconAttribute[] customAttributes;
      if ((customAttributes = ((object) this.mTask).GetType().GetCustomAttributes(typeof (TaskIconAttribute), false) as TaskIconAttribute[]).Length > 0)
        this.mTask.get_NodeData().set_Icon((Texture) BehaviorDesignerUtility.LoadIcon(customAttributes[0].get_IconPath(), (ScriptableObject) null));
      if (!Object.op_Equality((Object) this.mTask.get_NodeData().get_Icon(), (Object) null))
        return;
      string empty = string.Empty;
      this.mTask.get_NodeData().set_Icon((Texture) BehaviorDesignerUtility.LoadIcon(!((object) this.mTask).GetType().IsSubclassOf(typeof (Action)) ? (!((object) this.mTask).GetType().IsSubclassOf(typeof (Conditional)) ? (!((object) this.mTask).GetType().IsSubclassOf(typeof (Composite)) ? (!((object) this.mTask).GetType().IsSubclassOf(typeof (Decorator)) ? "{SkinColor}EntryIcon.png" : "{SkinColor}DecoratorIcon.png") : "{SkinColor}CompositeIcon.png") : "{SkinColor}ConditionalIcon.png") : "{SkinColor}ActionIcon.png", (ScriptableObject) null));
    }

    private void Init()
    {
      this.taskName = BehaviorDesignerUtility.SplitCamelCase(((object) this.mTask).GetType().Name.ToString());
      this.isParent = ((object) this.mTask).GetType().IsSubclassOf(typeof (ParentTask));
      if (this.isParent)
        this.outgoingNodeConnections = new List<NodeConnection>();
      this.mRectIsDirty = this.mCacheIsDirty = true;
      this.mIncomingRectIsDirty = true;
      this.mOutgoingRectIsDirty = true;
    }

    public void MakeEntryDisplay()
    {
      this.isEntryDisplay = this.isParent = true;
      this.mTask.set_FriendlyName(this.taskName = "Entry");
      this.outgoingNodeConnections = new List<NodeConnection>();
    }

    public Vector2 GetAbsolutePosition()
    {
      Vector2 vector2 = this.mTask.get_NodeData().get_Offset();
      if (Object.op_Inequality((Object) this.parentNodeDesigner, (Object) null))
        vector2 = Vector2.op_Addition(vector2, this.parentNodeDesigner.GetAbsolutePosition());
      if (BehaviorDesignerPreferences.GetBool(BDPreferences.SnapToGrid))
        ((Vector2) ref vector2).Set(BehaviorDesignerUtility.RoundToNearest((float) vector2.x, 10f), BehaviorDesignerUtility.RoundToNearest((float) vector2.y, 10f));
      return vector2;
    }

    public Rect Rectangle(Vector2 offset, bool includeConnections, bool includeComments)
    {
      Rect rect = this.Rectangle(offset);
      if (includeConnections)
      {
        if (!this.isEntryDisplay)
        {
          ref Rect local = ref rect;
          ((Rect) ref local).set_yMin(((Rect) ref local).get_yMin() - 14f);
        }
        if (this.isParent)
        {
          ref Rect local = ref rect;
          ((Rect) ref local).set_yMax(((Rect) ref local).get_yMax() + 16f);
        }
      }
      if (includeComments && this.mTask != null)
      {
        if (this.mTask.get_NodeData().get_WatchedFields() != null && this.mTask.get_NodeData().get_WatchedFields().Count > 0 && (double) ((Rect) ref rect).get_xMax() < (double) ((Rect) ref this.watchedFieldRect).get_xMax())
          ((Rect) ref rect).set_xMax(((Rect) ref this.watchedFieldRect).get_xMax());
        if (!this.mTask.get_NodeData().get_Comment().Equals(string.Empty))
        {
          if ((double) ((Rect) ref rect).get_xMax() < (double) ((Rect) ref this.commentRect).get_xMax())
            ((Rect) ref rect).set_xMax(((Rect) ref this.commentRect).get_xMax());
          if ((double) ((Rect) ref rect).get_yMax() < (double) ((Rect) ref this.commentRect).get_yMax())
            ((Rect) ref rect).set_yMax(((Rect) ref this.commentRect).get_yMax());
        }
      }
      return rect;
    }

    private Rect Rectangle(Vector2 offset)
    {
      if (!this.mRectIsDirty)
        return this.mRectangle;
      this.mCacheIsDirty = true;
      if (this.mTask == null)
        return (Rect) null;
      float num1 = (float) (BehaviorDesignerUtility.TaskTitleGUIStyle.CalcSize(new GUIContent(this.ToString())).x + 20.0);
      if (!this.isParent)
      {
        float num2;
        float num3;
        BehaviorDesignerUtility.TaskCommentGUIStyle.CalcMinMaxWidth(new GUIContent(this.mTask.get_NodeData().get_Comment()), ref num2, ref num3);
        float num4 = num3 + 20f;
        num1 = (double) num1 <= (double) num4 ? num4 : num1;
      }
      float num5 = Mathf.Min(220f, Mathf.Max(100f, num1));
      Vector2 absolutePosition = this.GetAbsolutePosition();
      float num6 = (float) (20 + (!BehaviorDesignerPreferences.GetBool(BDPreferences.CompactMode) ? 52 : 22));
      this.mRectangle = new Rect((float) (absolutePosition.x + offset.x - (double) num5 / 2.0), (float) (absolutePosition.y + offset.y), num5, num6);
      this.mRectIsDirty = false;
      return this.mRectangle;
    }

    private void UpdateCache(Rect nodeRect)
    {
      if (!this.mCacheIsDirty)
        return;
      this.nodeCollapsedTextureRect = new Rect((float) ((double) ((Rect) ref nodeRect).get_x() + ((double) ((Rect) ref nodeRect).get_width() - 26.0) / 2.0 + 1.0), ((Rect) ref nodeRect).get_yMax() + 2f, 26f, 6f);
      this.iconTextureRect = new Rect(((Rect) ref nodeRect).get_x() + (float) (((double) ((Rect) ref nodeRect).get_width() - 44.0) / 2.0), (float) ((double) ((Rect) ref nodeRect).get_y() + 4.0 + 2.0), 44f, 44f);
      this.titleRect = new Rect(((Rect) ref nodeRect).get_x(), (float) ((double) ((Rect) ref nodeRect).get_yMax() - (!BehaviorDesignerPreferences.GetBool(BDPreferences.CompactMode) ? 20.0 : 28.0) - 1.0), ((Rect) ref nodeRect).get_width(), 20f);
      this.breakpointTextureRect = new Rect(((Rect) ref nodeRect).get_xMax() - 16f, ((Rect) ref nodeRect).get_y() + 3f, 14f, 14f);
      this.errorTextureRect = new Rect(((Rect) ref nodeRect).get_xMax() - 12f, ((Rect) ref nodeRect).get_y() - 8f, 20f, 20f);
      this.referenceTextureRect = new Rect(((Rect) ref nodeRect).get_x() + 2f, ((Rect) ref nodeRect).get_y() + 3f, 14f, 14f);
      this.conditionalAbortTextureRect = new Rect(((Rect) ref nodeRect).get_x() + 3f, ((Rect) ref nodeRect).get_y() + 3f, 16f, 16f);
      this.conditionalAbortLowerPriorityTextureRect = new Rect(((Rect) ref nodeRect).get_x() + 3f, ((Rect) ref nodeRect).get_y(), 16f, 16f);
      this.disabledButtonTextureRect = new Rect(((Rect) ref nodeRect).get_x() - 1f, ((Rect) ref nodeRect).get_y() - 17f, 14f, 14f);
      this.collapseButtonTextureRect = new Rect(((Rect) ref nodeRect).get_x() + 15f, ((Rect) ref nodeRect).get_y() - 17f, 14f, 14f);
      this.incomingConnectionTextureRect = new Rect(((Rect) ref nodeRect).get_x() + (float) (((double) ((Rect) ref nodeRect).get_width() - 42.0) / 2.0), (float) ((double) ((Rect) ref nodeRect).get_y() - 14.0 - 3.0 + 3.0), 42f, 17f);
      this.outgoingConnectionTextureRect = new Rect(((Rect) ref nodeRect).get_x() + (float) (((double) ((Rect) ref nodeRect).get_width() - 42.0) / 2.0), ((Rect) ref nodeRect).get_yMax() - 3f, 42f, 19f);
      this.successReevaluatingExecutionStatusTextureRect = new Rect(((Rect) ref nodeRect).get_xMax() - 37f, ((Rect) ref nodeRect).get_yMax() - 38f, 35f, 36f);
      this.successExecutionStatusTextureRect = new Rect(((Rect) ref nodeRect).get_xMax() - 37f, ((Rect) ref nodeRect).get_yMax() - 33f, 35f, 31f);
      this.failureExecutionStatusTextureRect = new Rect(((Rect) ref nodeRect).get_xMax() - 37f, ((Rect) ref nodeRect).get_yMax() - 38f, 35f, 36f);
      this.iconBorderTextureRect = new Rect(((Rect) ref nodeRect).get_x() + (float) (((double) ((Rect) ref nodeRect).get_width() - 46.0) / 2.0), (float) ((double) ((Rect) ref nodeRect).get_y() + 3.0 + 2.0), 46f, 46f);
      this.CalculateNodeCommentRect(nodeRect);
      this.mCacheIsDirty = false;
    }

    private void CalculateNodeCommentRect(Rect nodeRect)
    {
      bool flag = false;
      if (this.mTask.get_NodeData().get_WatchedFields() != null && this.mTask.get_NodeData().get_WatchedFields().Count > 0)
      {
        string str1 = string.Empty;
        string str2 = string.Empty;
        for (int index = 0; index < this.mTask.get_NodeData().get_WatchedFields().Count; ++index)
        {
          FieldInfo watchedField = this.mTask.get_NodeData().get_WatchedFields()[index];
          str1 = str1 + BehaviorDesignerUtility.SplitCamelCase(watchedField.Name) + ": \n";
          str2 = str2 + (watchedField.GetValue((object) this.mTask) == null ? "null" : watchedField.GetValue((object) this.mTask).ToString()) + "\n";
        }
        float num1;
        float num2;
        BehaviorDesignerUtility.TaskCommentGUIStyle.CalcMinMaxWidth(new GUIContent(str1), ref num1, ref num2);
        float num3;
        BehaviorDesignerUtility.TaskCommentGUIStyle.CalcMinMaxWidth(new GUIContent(str2), ref num1, ref num3);
        float num4 = num2;
        float num5 = num3;
        float num6 = Mathf.Min(220f, (float) ((double) num2 + (double) num3 + 20.0));
        if ((double) num6 == 220.0)
        {
          num4 = (float) ((double) num2 / ((double) num2 + (double) num3) * 220.0);
          num5 = (float) ((double) num3 / ((double) num2 + (double) num3) * 220.0);
        }
        this.watchedFieldRect = new Rect(((Rect) ref nodeRect).get_xMax() + 4f, ((Rect) ref nodeRect).get_y(), num6 + 8f, ((Rect) ref nodeRect).get_height());
        this.watchedFieldNamesRect = new Rect(((Rect) ref nodeRect).get_xMax() + 6f, ((Rect) ref nodeRect).get_y() + 4f, num4, ((Rect) ref nodeRect).get_height() - 8f);
        this.watchedFieldValuesRect = new Rect(((Rect) ref nodeRect).get_xMax() + 6f + num4, ((Rect) ref nodeRect).get_y() + 4f, num5, ((Rect) ref nodeRect).get_height() - 8f);
        flag = true;
      }
      if (this.mTask.get_NodeData().get_Comment().Equals(string.Empty))
        return;
      if (this.isParent)
      {
        float num1;
        float num2;
        BehaviorDesignerUtility.TaskCommentGUIStyle.CalcMinMaxWidth(new GUIContent(this.mTask.get_NodeData().get_Comment()), ref num1, ref num2);
        float num3 = Mathf.Min(220f, num2 + 20f);
        if (flag)
        {
          this.commentRect = new Rect(((Rect) ref nodeRect).get_xMin() - 12f - num3, ((Rect) ref nodeRect).get_y(), num3 + 8f, ((Rect) ref nodeRect).get_height());
          this.commentLabelRect = new Rect(((Rect) ref nodeRect).get_xMin() - 6f - num3, ((Rect) ref nodeRect).get_y() + 4f, num3, ((Rect) ref nodeRect).get_height() - 8f);
        }
        else
        {
          this.commentRect = new Rect(((Rect) ref nodeRect).get_xMax() + 4f, ((Rect) ref nodeRect).get_y(), num3 + 8f, ((Rect) ref nodeRect).get_height());
          this.commentLabelRect = new Rect(((Rect) ref nodeRect).get_xMax() + 6f, ((Rect) ref nodeRect).get_y() + 4f, num3, ((Rect) ref nodeRect).get_height() - 8f);
        }
      }
      else
      {
        float num = Mathf.Min(100f, BehaviorDesignerUtility.TaskCommentGUIStyle.CalcHeight(new GUIContent(this.mTask.get_NodeData().get_Comment()), ((Rect) ref nodeRect).get_width() - 4f));
        this.commentRect = new Rect(((Rect) ref nodeRect).get_x(), ((Rect) ref nodeRect).get_yMax() + 4f, ((Rect) ref nodeRect).get_width(), num + 4f);
        this.commentLabelRect = new Rect(((Rect) ref nodeRect).get_x(), ((Rect) ref nodeRect).get_yMax() + 4f, ((Rect) ref nodeRect).get_width() - 4f, num);
      }
    }

    public bool DrawNode(Vector2 offset, bool drawSelected, bool disabled)
    {
      if (drawSelected != this.mSelected)
        return false;
      if (this.ToString().Length != this.prevFriendlyNameLength)
      {
        this.prevFriendlyNameLength = this.ToString().Length;
        this.mRectIsDirty = true;
      }
      Rect nodeRect = this.Rectangle(offset, false, false);
      this.UpdateCache(nodeRect);
      bool flag1 = (double) this.mTask.get_NodeData().get_PushTime() != -1.0 && (double) this.mTask.get_NodeData().get_PushTime() >= (double) this.mTask.get_NodeData().get_PopTime() || this.isEntryDisplay && this.outgoingNodeConnections.Count > 0 && (double) this.outgoingNodeConnections[0].DestinationNodeDesigner.Task.get_NodeData().get_PushTime() != -1.0;
      bool flag2 = this.mIdentifyUpdateCount != -1;
      bool flag3 = this.prevRunningState != flag1;
      float num1 = !BehaviorDesignerPreferences.GetBool(BDPreferences.FadeNodes) ? 0.01f : 0.5f;
      float num2 = 0.0f;
      if (flag2)
      {
        num2 = 2000 - this.mIdentifyUpdateCount >= 500 ? 1f : (float) (2000 - this.mIdentifyUpdateCount) / 500f;
        if (this.mIdentifyUpdateCount != -1)
        {
          ++this.mIdentifyUpdateCount;
          if (this.mIdentifyUpdateCount > 2000)
            this.mIdentifyUpdateCount = -1;
        }
        flag3 = true;
      }
      else if (flag1)
        num2 = 1f;
      else if ((double) this.mTask.get_NodeData().get_PopTime() != -1.0 && (double) num1 != 0.0 && ((double) this.mTask.get_NodeData().get_PopTime() <= (double) Time.get_realtimeSinceStartup() && (double) Time.get_realtimeSinceStartup() - (double) this.mTask.get_NodeData().get_PopTime() < (double) num1) || this.isEntryDisplay && this.outgoingNodeConnections.Count > 0 && ((double) this.outgoingNodeConnections[0].DestinationNodeDesigner.Task.get_NodeData().get_PopTime() != -1.0 && (double) this.outgoingNodeConnections[0].DestinationNodeDesigner.Task.get_NodeData().get_PopTime() <= (double) Time.get_realtimeSinceStartup()) && (double) Time.get_realtimeSinceStartup() - (double) this.outgoingNodeConnections[0].DestinationNodeDesigner.Task.get_NodeData().get_PopTime() < (double) num1)
      {
        num2 = !this.isEntryDisplay ? (float) (1.0 - ((double) Time.get_realtimeSinceStartup() - (double) this.mTask.get_NodeData().get_PopTime()) / (double) num1) : (float) (1.0 - ((double) Time.get_realtimeSinceStartup() - (double) this.outgoingNodeConnections[0].DestinationNodeDesigner.Task.get_NodeData().get_PopTime()) / (double) num1);
        flag3 = true;
      }
      if (!this.isEntryDisplay && !this.prevRunningState && Object.op_Inequality((Object) this.parentNodeDesigner, (Object) null))
        this.parentNodeDesigner.BringConnectionToFront(this);
      this.prevRunningState = flag1;
      if ((double) num2 != 1.0)
      {
        GUI.set_color(disabled || this.mTask.get_Disabled() ? this.grayColor : Color.get_white());
        GUIStyle backgroundGUIStyle = !BehaviorDesignerPreferences.GetBool(BDPreferences.CompactMode) ? (!this.mSelected ? BehaviorDesignerUtility.GetTaskGUIStyle(this.mTask.get_NodeData().get_ColorIndex()) : BehaviorDesignerUtility.GetTaskSelectedGUIStyle(this.mTask.get_NodeData().get_ColorIndex())) : (!this.mSelected ? BehaviorDesignerUtility.GetTaskCompactGUIStyle(this.mTask.get_NodeData().get_ColorIndex()) : BehaviorDesignerUtility.GetTaskSelectedCompactGUIStyle(this.mTask.get_NodeData().get_ColorIndex()));
        this.DrawNodeTexture(nodeRect, BehaviorDesignerUtility.GetTaskConnectionTopTexture(this.mTask.get_NodeData().get_ColorIndex()), BehaviorDesignerUtility.GetTaskConnectionBottomTexture(this.mTask.get_NodeData().get_ColorIndex()), backgroundGUIStyle, BehaviorDesignerUtility.GetTaskBorderTexture(this.mTask.get_NodeData().get_ColorIndex()));
      }
      if ((double) num2 > 0.0)
      {
        GUIStyle backgroundGUIStyle;
        Texture2D iconBorderTexture;
        if (flag2)
        {
          backgroundGUIStyle = !BehaviorDesignerPreferences.GetBool(BDPreferences.CompactMode) ? (!this.mSelected ? BehaviorDesignerUtility.TaskIdentifyGUIStyle : BehaviorDesignerUtility.TaskIdentifySelectedGUIStyle) : (!this.mSelected ? BehaviorDesignerUtility.TaskIdentifyCompactGUIStyle : BehaviorDesignerUtility.TaskIdentifySelectedCompactGUIStyle);
          iconBorderTexture = BehaviorDesignerUtility.TaskBorderIdentifyTexture;
        }
        else
        {
          backgroundGUIStyle = !BehaviorDesignerPreferences.GetBool(BDPreferences.CompactMode) ? (!this.mSelected ? BehaviorDesignerUtility.TaskRunningGUIStyle : BehaviorDesignerUtility.TaskRunningSelectedGUIStyle) : (!this.mSelected ? BehaviorDesignerUtility.TaskRunningCompactGUIStyle : BehaviorDesignerUtility.TaskRunningSelectedCompactGUIStyle);
          iconBorderTexture = BehaviorDesignerUtility.TaskBorderRunningTexture;
        }
        Color color = disabled || this.mTask.get_Disabled() ? this.grayColor : Color.get_white();
        color.a = (__Null) (double) num2;
        GUI.set_color(color);
        Texture2D connectionTopTexture = (Texture2D) null;
        Texture2D connectionBottomTexture = (Texture2D) null;
        if (!this.isEntryDisplay)
          connectionTopTexture = !flag2 ? BehaviorDesignerUtility.TaskConnectionRunningTopTexture : BehaviorDesignerUtility.TaskConnectionIdentifyTopTexture;
        if (this.isParent)
          connectionBottomTexture = !flag2 ? BehaviorDesignerUtility.TaskConnectionRunningBottomTexture : BehaviorDesignerUtility.TaskConnectionIdentifyBottomTexture;
        this.DrawNodeTexture(nodeRect, connectionTopTexture, connectionBottomTexture, backgroundGUIStyle, iconBorderTexture);
        GUI.set_color(Color.get_white());
      }
      if (this.mTask.get_NodeData().get_Collapsed())
        GUI.DrawTexture(this.nodeCollapsedTextureRect, (Texture) BehaviorDesignerUtility.TaskConnectionCollapsedTexture);
      if (!BehaviorDesignerPreferences.GetBool(BDPreferences.CompactMode))
        GUI.DrawTexture(this.iconTextureRect, this.mTask.get_NodeData().get_Icon());
      if ((double) this.mTask.get_NodeData().get_InterruptTime() != -1.0 && (double) Time.get_realtimeSinceStartup() - (double) this.mTask.get_NodeData().get_InterruptTime() < 0.75 + (double) num1)
      {
        float num3 = (double) Time.get_realtimeSinceStartup() - (double) this.mTask.get_NodeData().get_InterruptTime() >= 0.75 ? (float) (1.0 - ((double) Time.get_realtimeSinceStartup() - ((double) this.mTask.get_NodeData().get_InterruptTime() + 0.75)) / (double) num1) : 1f;
        Color white = Color.get_white();
        white.a = (__Null) (double) num3;
        GUI.set_color(white);
        GUI.Label(nodeRect, string.Empty, BehaviorDesignerUtility.TaskHighlightGUIStyle);
        GUI.set_color(Color.get_white());
      }
      GUI.Label(this.titleRect, this.ToString(), BehaviorDesignerUtility.TaskTitleGUIStyle);
      if (this.mTask.get_NodeData().get_IsBreakpoint())
        GUI.DrawTexture(this.breakpointTextureRect, (Texture) BehaviorDesignerUtility.BreakpointTexture);
      if (this.showReferenceIcon)
        GUI.DrawTexture(this.referenceTextureRect, (Texture) BehaviorDesignerUtility.ReferencedTexture);
      if (this.hasError)
        GUI.DrawTexture(this.errorTextureRect, (Texture) BehaviorDesignerUtility.ErrorIconTexture);
      if (this.mTask is Composite && (this.mTask as Composite).get_AbortType() != null)
      {
        switch ((this.mTask as Composite).get_AbortType() - 1)
        {
          case 0:
            GUI.DrawTexture(this.conditionalAbortTextureRect, (Texture) BehaviorDesignerUtility.ConditionalAbortSelfTexture);
            break;
          case 1:
            GUI.DrawTexture(this.conditionalAbortLowerPriorityTextureRect, (Texture) BehaviorDesignerUtility.ConditionalAbortLowerPriorityTexture);
            break;
          case 2:
            GUI.DrawTexture(this.conditionalAbortTextureRect, (Texture) BehaviorDesignerUtility.ConditionalAbortBothTexture);
            break;
        }
      }
      GUI.set_color(Color.get_white());
      if (this.showHoverBar)
      {
        GUI.DrawTexture(this.disabledButtonTextureRect, !this.mTask.get_Disabled() ? (Texture) BehaviorDesignerUtility.DisableTaskTexture : (Texture) BehaviorDesignerUtility.EnableTaskTexture, (ScaleMode) 2);
        if (this.isParent || this.mTask is BehaviorReference)
        {
          bool collapsed = this.mTask.get_NodeData().get_Collapsed();
          if (this.mTask is BehaviorReference)
            collapsed = (bool) (this.mTask as BehaviorReference).collapsed;
          GUI.DrawTexture(this.collapseButtonTextureRect, !collapsed ? (Texture) BehaviorDesignerUtility.CollapseTaskTexture : (Texture) BehaviorDesignerUtility.ExpandTaskTexture, (ScaleMode) 2);
        }
      }
      return flag3;
    }

    private void DrawNodeTexture(
      Rect nodeRect,
      Texture2D connectionTopTexture,
      Texture2D connectionBottomTexture,
      GUIStyle backgroundGUIStyle,
      Texture2D iconBorderTexture)
    {
      if (!this.isEntryDisplay)
        GUI.DrawTexture(this.incomingConnectionTextureRect, (Texture) connectionTopTexture, (ScaleMode) 2);
      if (this.isParent)
        GUI.DrawTexture(this.outgoingConnectionTextureRect, (Texture) connectionBottomTexture, (ScaleMode) 2);
      GUI.Label(nodeRect, string.Empty, backgroundGUIStyle);
      if (this.mTask.get_NodeData().get_ExecutionStatus() == 2)
      {
        if (this.mTask.get_NodeData().get_IsReevaluating())
          GUI.DrawTexture(this.successReevaluatingExecutionStatusTextureRect, (Texture) BehaviorDesignerUtility.ExecutionSuccessRepeatTexture);
        else
          GUI.DrawTexture(this.successExecutionStatusTextureRect, (Texture) BehaviorDesignerUtility.ExecutionSuccessTexture);
      }
      else if (this.mTask.get_NodeData().get_ExecutionStatus() == 1)
        GUI.DrawTexture(this.failureExecutionStatusTextureRect, !this.mTask.get_NodeData().get_IsReevaluating() ? (Texture) BehaviorDesignerUtility.ExecutionFailureTexture : (Texture) BehaviorDesignerUtility.ExecutionFailureRepeatTexture);
      if (BehaviorDesignerPreferences.GetBool(BDPreferences.CompactMode))
        return;
      GUI.DrawTexture(this.iconBorderTextureRect, (Texture) iconBorderTexture);
    }

    public void DrawNodeConnection(Vector2 offset, bool disabled)
    {
      if (this.mConnectionIsDirty)
      {
        this.DetermineConnectionHorizontalHeight(this.Rectangle(offset, false, false), offset);
        this.mConnectionIsDirty = false;
      }
      if (!this.isParent)
        return;
      for (int index = 0; index < this.outgoingNodeConnections.Count; ++index)
        this.outgoingNodeConnections[index].DrawConnection(offset, disabled);
    }

    public void DrawNodeComment(Vector2 offset)
    {
      if (this.mTask.get_NodeData().get_Comment().Length != this.prevCommentLength)
      {
        this.prevCommentLength = this.mTask.get_NodeData().get_Comment().Length;
        this.mRectIsDirty = true;
      }
      if (this.mTask.get_NodeData().get_WatchedFields() != null && this.mTask.get_NodeData().get_WatchedFields().Count > 0)
      {
        if (this.mTask.get_NodeData().get_WatchedFields().Count != this.prevWatchedFieldsLength.Count)
        {
          this.mRectIsDirty = true;
          this.prevWatchedFieldsLength.Clear();
          for (int index = 0; index < this.mTask.get_NodeData().get_WatchedFields().Count; ++index)
          {
            if (this.mTask.get_NodeData().get_WatchedFields()[index] != null)
            {
              object obj = this.mTask.get_NodeData().get_WatchedFields()[index].GetValue((object) this.mTask);
              if (obj != null)
                this.prevWatchedFieldsLength.Add(obj.ToString().Length);
              else
                this.prevWatchedFieldsLength.Add(0);
            }
          }
        }
        else
        {
          for (int index = 0; index < this.mTask.get_NodeData().get_WatchedFields().Count; ++index)
          {
            if (this.mTask.get_NodeData().get_WatchedFields()[index] != null)
            {
              object obj = this.mTask.get_NodeData().get_WatchedFields()[index].GetValue((object) this.mTask);
              int num = 0;
              if (obj != null)
                num = obj.ToString().Length;
              if (num != this.prevWatchedFieldsLength[index])
              {
                this.mRectIsDirty = true;
                break;
              }
            }
          }
        }
      }
      if (this.mTask.get_NodeData().get_Comment().Equals(string.Empty) && (this.mTask.get_NodeData().get_WatchedFields() == null || this.mTask.get_NodeData().get_WatchedFields().Count == 0))
        return;
      if (this.mTask.get_NodeData().get_WatchedFields() != null && this.mTask.get_NodeData().get_WatchedFields().Count > 0)
      {
        string str1 = string.Empty;
        string str2 = string.Empty;
        for (int index = 0; index < this.mTask.get_NodeData().get_WatchedFields().Count; ++index)
        {
          FieldInfo watchedField = this.mTask.get_NodeData().get_WatchedFields()[index];
          str1 = str1 + BehaviorDesignerUtility.SplitCamelCase(watchedField.Name) + ": \n";
          str2 = str2 + (watchedField.GetValue((object) this.mTask) == null ? "null" : watchedField.GetValue((object) this.mTask).ToString()) + "\n";
        }
        GUI.Box(this.watchedFieldRect, string.Empty, BehaviorDesignerUtility.TaskDescriptionGUIStyle);
        GUI.Label(this.watchedFieldNamesRect, str1, BehaviorDesignerUtility.TaskCommentRightAlignGUIStyle);
        GUI.Label(this.watchedFieldValuesRect, str2, BehaviorDesignerUtility.TaskCommentLeftAlignGUIStyle);
      }
      if (this.mTask.get_NodeData().get_Comment().Equals(string.Empty))
        return;
      GUI.Box(this.commentRect, string.Empty, BehaviorDesignerUtility.TaskDescriptionGUIStyle);
      GUI.Label(this.commentLabelRect, this.mTask.get_NodeData().get_Comment(), BehaviorDesignerUtility.TaskCommentGUIStyle);
    }

    public bool Contains(Vector2 point, Vector2 offset, bool includeConnections)
    {
      Rect rect = this.Rectangle(offset, includeConnections, false);
      return ((Rect) ref rect).Contains(point);
    }

    public NodeConnection NodeConnectionRectContains(Vector2 point, Vector2 offset)
    {
      Rect rect1 = this.IncomingConnectionRect(offset);
      bool incomingNodeConnection;
      if (!(incomingNodeConnection = ((Rect) ref rect1).Contains(point)))
      {
        if (this.isParent)
        {
          Rect rect2 = this.OutgoingConnectionRect(offset);
          if (((Rect) ref rect2).Contains(point))
            goto label_3;
        }
        return (NodeConnection) null;
      }
label_3:
      return this.CreateNodeConnection(incomingNodeConnection);
    }

    public NodeConnection CreateNodeConnection(bool incomingNodeConnection)
    {
      NodeConnection instance = (NodeConnection) ScriptableObject.CreateInstance<NodeConnection>();
      instance.LoadConnection(this, !incomingNodeConnection ? NodeConnectionType.Outgoing : NodeConnectionType.Incoming);
      return instance;
    }

    public void ConnectionContains(
      Vector2 point,
      Vector2 offset,
      ref List<NodeConnection> nodeConnections)
    {
      if (this.outgoingNodeConnections == null || this.isEntryDisplay)
        return;
      for (int index = 0; index < this.outgoingNodeConnections.Count; ++index)
      {
        if (this.outgoingNodeConnections[index].Contains(point, offset))
          nodeConnections.Add(this.outgoingNodeConnections[index]);
      }
    }

    private void DetermineConnectionHorizontalHeight(Rect nodeRect, Vector2 offset)
    {
      if (!this.isParent)
        return;
      float num1 = float.MaxValue;
      float num2 = num1;
      for (int index = 0; index < this.outgoingNodeConnections.Count; ++index)
      {
        Rect rect = this.outgoingNodeConnections[index].DestinationNodeDesigner.Rectangle(offset, false, false);
        if ((double) ((Rect) ref rect).get_y() < (double) num1)
        {
          num1 = ((Rect) ref rect).get_y();
          num2 = ((Rect) ref rect).get_y();
        }
      }
      float num3 = (float) ((double) num1 * 0.75 + (double) ((Rect) ref nodeRect).get_yMax() * 0.25);
      if ((double) num3 < (double) ((Rect) ref nodeRect).get_yMax() + 15.0)
        num3 = ((Rect) ref nodeRect).get_yMax() + 15f;
      else if ((double) num3 > (double) num2 - 15.0)
        num3 = num2 - 15f;
      for (int index = 0; index < this.outgoingNodeConnections.Count; ++index)
        this.outgoingNodeConnections[index].HorizontalHeight = num3;
    }

    public Vector2 GetConnectionPosition(Vector2 offset, NodeConnectionType connectionType)
    {
      Vector2 vector2;
      if (connectionType == NodeConnectionType.Incoming)
      {
        Rect rect = this.IncomingConnectionRect(offset);
        ((Vector2) ref vector2).\u002Ector((float) ((Rect) ref rect).get_center().x, ((Rect) ref rect).get_y() + 7f);
      }
      else
      {
        Rect rect = this.OutgoingConnectionRect(offset);
        ((Vector2) ref vector2).\u002Ector((float) ((Rect) ref rect).get_center().x, ((Rect) ref rect).get_yMax() - 8f);
      }
      return vector2;
    }

    public bool HoverBarAreaContains(Vector2 point, Vector2 offset)
    {
      Rect rect = this.Rectangle(offset, false, false);
      ref Rect local = ref rect;
      ((Rect) ref local).set_y(((Rect) ref local).get_y() - 24f);
      return ((Rect) ref rect).Contains(point);
    }

    public bool HoverBarButtonClick(Vector2 point, Vector2 offset, ref bool collapsedButtonClicked)
    {
      Rect rect1 = this.Rectangle(offset, false, false);
      Rect rect2;
      ((Rect) ref rect2).\u002Ector(((Rect) ref rect1).get_x() - 1f, ((Rect) ref rect1).get_y() - 17f, 14f, 14f);
      Rect rect3 = rect2;
      bool flag = false;
      if (((Rect) ref rect2).Contains(point))
      {
        this.mTask.set_Disabled(!this.mTask.get_Disabled());
        flag = true;
      }
      if (!flag && (this.isParent || this.mTask is BehaviorReference))
      {
        Rect rect4;
        ((Rect) ref rect4).\u002Ector(((Rect) ref rect1).get_x() + 15f, ((Rect) ref rect1).get_y() - 17f, 14f, 14f);
        ((Rect) ref rect3).set_xMax(((Rect) ref rect4).get_xMax());
        if (((Rect) ref rect4).Contains(point))
        {
          if (this.mTask is BehaviorReference)
            (this.mTask as BehaviorReference).collapsed = (__Null) ((this.mTask as BehaviorReference).collapsed == 0 ? 1 : 0);
          else
            this.mTask.get_NodeData().set_Collapsed(!this.mTask.get_NodeData().get_Collapsed());
          collapsedButtonClicked = true;
          flag = true;
        }
      }
      if (!flag && ((Rect) ref rect3).Contains(point))
        flag = true;
      return flag;
    }

    public bool Intersects(Rect rect, Vector2 offset)
    {
      Rect rect1 = this.Rectangle(offset, false, false);
      if ((double) ((Rect) ref rect1).get_xMin() < (double) ((Rect) ref rect).get_xMax() && (double) ((Rect) ref rect1).get_xMax() > (double) ((Rect) ref rect).get_xMin() && (double) ((Rect) ref rect1).get_yMin() < (double) ((Rect) ref rect).get_yMax())
        return (double) ((Rect) ref rect1).get_yMax() > (double) ((Rect) ref rect).get_yMin();
      return false;
    }

    public void ChangeOffset(Vector2 delta)
    {
      this.mTask.get_NodeData().set_Offset(Vector2.op_Addition(this.mTask.get_NodeData().get_Offset(), delta));
      this.MarkDirty();
      if (!Object.op_Inequality((Object) this.parentNodeDesigner, (Object) null))
        return;
      this.parentNodeDesigner.MarkDirty();
    }

    public void AddChildNode(
      NodeDesigner childNodeDesigner,
      NodeConnection nodeConnection,
      bool adjustOffset,
      bool replaceNode)
    {
      this.AddChildNode(childNodeDesigner, nodeConnection, adjustOffset, replaceNode, -1);
    }

    public void AddChildNode(
      NodeDesigner childNodeDesigner,
      NodeConnection nodeConnection,
      bool adjustOffset,
      bool replaceNode,
      int replaceNodeIndex)
    {
      if (replaceNode)
      {
        (this.mTask as ParentTask).get_Children()[replaceNodeIndex] = childNodeDesigner.Task;
      }
      else
      {
        if (!this.isEntryDisplay)
        {
          ParentTask mTask = this.mTask as ParentTask;
          int index = 0;
          if (mTask.get_Children() != null)
          {
            index = 0;
            while (index < mTask.get_Children().Count && childNodeDesigner.GetAbsolutePosition().x >= (mTask.get_Children()[index].get_NodeData().get_NodeDesigner() as NodeDesigner).GetAbsolutePosition().x)
              ++index;
          }
          mTask.AddChild(childNodeDesigner.Task, index);
        }
        if (adjustOffset)
        {
          NodeData nodeData = childNodeDesigner.Task.get_NodeData();
          nodeData.set_Offset(Vector2.op_Subtraction(nodeData.get_Offset(), this.GetAbsolutePosition()));
        }
      }
      childNodeDesigner.ParentNodeDesigner = this;
      nodeConnection.DestinationNodeDesigner = childNodeDesigner;
      nodeConnection.NodeConnectionType = NodeConnectionType.Fixed;
      if (!nodeConnection.OriginatingNodeDesigner.Equals((object) this))
        nodeConnection.OriginatingNodeDesigner = this;
      this.outgoingNodeConnections.Add(nodeConnection);
      this.mConnectionIsDirty = true;
    }

    public void RemoveChildNode(NodeDesigner childNodeDesigner)
    {
      if (!this.isEntryDisplay)
        (this.mTask as ParentTask).get_Children().Remove(childNodeDesigner.Task);
      for (int index = 0; index < this.outgoingNodeConnections.Count; ++index)
      {
        NodeConnection outgoingNodeConnection = this.outgoingNodeConnections[index];
        if (outgoingNodeConnection.DestinationNodeDesigner.Equals((object) childNodeDesigner) || outgoingNodeConnection.OriginatingNodeDesigner.Equals((object) childNodeDesigner))
        {
          this.outgoingNodeConnections.RemoveAt(index);
          break;
        }
      }
      childNodeDesigner.ParentNodeDesigner = (NodeDesigner) null;
      this.mConnectionIsDirty = true;
    }

    public void SetID(ref int id)
    {
      Task mTask1 = this.mTask;
      int num1;
      id = (num1 = id) + 1;
      int num2 = num1;
      mTask1.set_ID(num2);
      if (!this.isParent)
        return;
      ParentTask mTask2 = this.mTask as ParentTask;
      if (mTask2.get_Children() == null)
        return;
      for (int index = 0; index < mTask2.get_Children().Count; ++index)
        (mTask2.get_Children()[index].get_NodeData().get_NodeDesigner() as NodeDesigner).SetID(ref id);
    }

    public int ChildIndexForTask(Task childTask)
    {
      if (this.isParent)
      {
        ParentTask mTask = this.mTask as ParentTask;
        if (mTask.get_Children() != null)
        {
          for (int index = 0; index < mTask.get_Children().Count; ++index)
          {
            if (((object) mTask.get_Children()[index]).Equals((object) childTask))
              return index;
          }
        }
      }
      return -1;
    }

    public NodeDesigner NodeDesignerForChildIndex(int index)
    {
      if (index < 0)
        return (NodeDesigner) null;
      if (this.isParent)
      {
        ParentTask mTask = this.mTask as ParentTask;
        if (mTask.get_Children() != null)
        {
          if (index >= mTask.get_Children().Count || mTask.get_Children()[index] == null)
            return (NodeDesigner) null;
          return mTask.get_Children()[index].get_NodeData().get_NodeDesigner() as NodeDesigner;
        }
      }
      return (NodeDesigner) null;
    }

    public void MoveChildNode(int index, bool decreaseIndex)
    {
      int index1 = index + (!decreaseIndex ? 1 : -1);
      ParentTask mTask = this.mTask as ParentTask;
      Task child = mTask.get_Children()[index];
      mTask.get_Children()[index] = mTask.get_Children()[index1];
      mTask.get_Children()[index1] = child;
    }

    private void BringConnectionToFront(NodeDesigner nodeDesigner)
    {
      for (int index = 0; index < this.outgoingNodeConnections.Count; ++index)
      {
        if (this.outgoingNodeConnections[index].DestinationNodeDesigner.Equals((object) nodeDesigner))
        {
          NodeConnection outgoingNodeConnection = this.outgoingNodeConnections[index];
          this.outgoingNodeConnections[index] = this.outgoingNodeConnections[this.outgoingNodeConnections.Count - 1];
          this.outgoingNodeConnections[this.outgoingNodeConnections.Count - 1] = outgoingNodeConnection;
          break;
        }
      }
    }

    public void ToggleBreakpoint()
    {
      this.mTask.get_NodeData().set_IsBreakpoint(!this.Task.get_NodeData().get_IsBreakpoint());
    }

    public void ToggleEnableState()
    {
      this.mTask.set_Disabled(!this.Task.get_Disabled());
    }

    public bool IsDisabled()
    {
      if (this.mTask.get_Disabled())
        return true;
      if (Object.op_Inequality((Object) this.parentNodeDesigner, (Object) null))
        return this.parentNodeDesigner.IsDisabled();
      return false;
    }

    public bool ToggleCollapseState()
    {
      this.mTask.get_NodeData().set_Collapsed(!this.Task.get_NodeData().get_Collapsed());
      return this.mTask.get_NodeData().get_Collapsed();
    }

    public void IdentifyNode()
    {
      this.mIdentifyUpdateCount = 0;
    }

    public bool HasParent(NodeDesigner nodeDesigner)
    {
      if (Object.op_Equality((Object) this.parentNodeDesigner, (Object) null))
        return false;
      if (this.parentNodeDesigner.Equals((object) nodeDesigner))
        return true;
      return this.parentNodeDesigner.HasParent(nodeDesigner);
    }

    public void DestroyConnections()
    {
      if (this.outgoingNodeConnections == null)
        return;
      for (int index = this.outgoingNodeConnections.Count - 1; index > -1; --index)
        Object.DestroyImmediate((Object) this.outgoingNodeConnections[index], true);
    }

    public virtual bool Equals(object obj)
    {
      return object.ReferenceEquals((object) this, obj);
    }

    public virtual int GetHashCode()
    {
      return ((Object) this).GetHashCode();
    }

    public virtual string ToString()
    {
      if (this.mTask == null)
        return string.Empty;
      if (this.mTask.get_FriendlyName().Equals(string.Empty))
        return this.taskName;
      return this.mTask.get_FriendlyName();
    }
  }
}
