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

		private int mIdentifyUpdateCount = -1;

		[SerializeField]
		private bool mConnectionIsDirty;

		private bool mRectIsDirty = true;

		private bool mIncomingRectIsDirty = true;

		private bool mOutgoingRectIsDirty = true;

		[SerializeField]
		private bool isParent;

		[SerializeField]
		private bool isEntryDisplay;

		[SerializeField]
		private bool showReferenceIcon;

		private bool showHoverBar;

		private bool hasError;

		[SerializeField]
		private string taskName = string.Empty;

		private Rect mRectangle;

		private Rect mOutgoingRectangle;

		private Rect mIncomingRectangle;

		private bool prevRunningState;

		private int prevCommentLength = -1;

		private List<int> prevWatchedFieldsLength = new List<int>();

		private int prevFriendlyNameLength = -1;

		[SerializeField]
		private NodeDesigner parentNodeDesigner;

		[SerializeField]
		private List<NodeConnection> outgoingNodeConnections;

		private bool mCacheIsDirty = true;

		private readonly Color grayColor = new Color(0.7f, 0.7f, 0.7f);

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

		public void Select()
		{
			if (!this.isEntryDisplay)
			{
				this.mSelected = true;
			}
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

		public Rect IncomingConnectionRect(Vector2 offset)
		{
			if (!this.mIncomingRectIsDirty)
			{
				return this.mIncomingRectangle;
			}
			Rect rect = this.Rectangle(offset, false, false);
			this.mIncomingRectangle = new Rect(rect.get_x() + (rect.get_width() - 42f) / 2f, rect.get_y() - 14f, 42f, 14f);
			this.mIncomingRectIsDirty = false;
			return this.mIncomingRectangle;
		}

		public Rect OutgoingConnectionRect(Vector2 offset)
		{
			if (!this.mOutgoingRectIsDirty)
			{
				return this.mOutgoingRectangle;
			}
			Rect rect = this.Rectangle(offset, false, false);
			this.mOutgoingRectangle = new Rect(rect.get_x() + (rect.get_width() - 42f) / 2f, rect.get_yMax(), 42f, 16f);
			this.mOutgoingRectIsDirty = false;
			return this.mOutgoingRectangle;
		}

		public void OnEnable()
		{
			base.set_hideFlags(61);
		}

		public void LoadTask(Task task, Behavior owner, ref int id)
		{
			if (task == null)
			{
				return;
			}
			this.mTask = task;
			this.mTask.set_Owner(owner);
			this.mTask.set_ID(id++);
			this.mTask.get_NodeData().set_NodeDesigner(this);
			this.mTask.get_NodeData().InitWatchedFields(this.mTask);
			if (!this.mTask.get_NodeData().get_FriendlyName().Equals(string.Empty))
			{
				this.mTask.set_FriendlyName(this.mTask.get_NodeData().get_FriendlyName());
				this.mTask.get_NodeData().set_FriendlyName(string.Empty);
			}
			this.LoadTaskIcon();
			this.Init();
			RequiredComponentAttribute[] array;
			if (this.mTask.get_Owner() != null && (array = (this.mTask.GetType().GetCustomAttributes(typeof(RequiredComponentAttribute), true) as RequiredComponentAttribute[])).Length > 0)
			{
				Type componentType = array[0].get_ComponentType();
				if (typeof(Component).IsAssignableFrom(componentType) && this.mTask.get_Owner().get_gameObject().GetComponent(componentType) == null)
				{
					this.mTask.get_Owner().get_gameObject().AddComponent(componentType);
				}
			}
			List<Type> baseClasses = FieldInspector.GetBaseClasses(this.mTask.GetType());
			BindingFlags bindingFlags = 54;
			for (int i = baseClasses.get_Count() - 1; i > -1; i--)
			{
				FieldInfo[] fields = baseClasses.get_Item(i).GetFields(bindingFlags);
				for (int j = 0; j < fields.Length; j++)
				{
					if (typeof(SharedVariable).IsAssignableFrom(fields[j].get_FieldType()) && !fields[j].get_FieldType().get_IsAbstract())
					{
						SharedVariable sharedVariable = fields[j].GetValue(this.mTask) as SharedVariable;
						if (sharedVariable == null)
						{
							sharedVariable = (Activator.CreateInstance(fields[j].get_FieldType()) as SharedVariable);
						}
						if (TaskUtility.HasAttribute(fields[j], typeof(RequiredFieldAttribute)) || TaskUtility.HasAttribute(fields[j], typeof(SharedRequiredAttribute)))
						{
							sharedVariable.set_IsShared(true);
						}
						fields[j].SetValue(this.mTask, sharedVariable);
					}
				}
			}
			if (this.isParent)
			{
				ParentTask parentTask = this.mTask as ParentTask;
				if (parentTask.get_Children() != null)
				{
					for (int k = 0; k < parentTask.get_Children().get_Count(); k++)
					{
						NodeDesigner nodeDesigner = ScriptableObject.CreateInstance<NodeDesigner>();
						nodeDesigner.LoadTask(parentTask.get_Children().get_Item(k), owner, ref id);
						NodeConnection nodeConnection = ScriptableObject.CreateInstance<NodeConnection>();
						nodeConnection.LoadConnection(this, NodeConnectionType.Fixed);
						this.AddChildNode(nodeDesigner, nodeConnection, true, true, k);
					}
				}
				this.mConnectionIsDirty = true;
			}
		}

		public void LoadNode(Task task, BehaviorSource behaviorSource, Vector2 offset, ref int id)
		{
			this.mTask = task;
			this.mTask.set_Owner(behaviorSource.get_Owner() as Behavior);
			this.mTask.set_ID(id++);
			this.mTask.set_NodeData(new NodeData());
			this.mTask.get_NodeData().set_Offset(offset);
			this.mTask.get_NodeData().set_NodeDesigner(this);
			this.LoadTaskIcon();
			this.Init();
			this.mTask.set_FriendlyName(this.taskName);
			RequiredComponentAttribute[] array;
			if (this.mTask.get_Owner() != null && (array = (this.mTask.GetType().GetCustomAttributes(typeof(RequiredComponentAttribute), true) as RequiredComponentAttribute[])).Length > 0)
			{
				Type componentType = array[0].get_ComponentType();
				if (typeof(Component).IsAssignableFrom(componentType) && this.mTask.get_Owner().get_gameObject().GetComponent(componentType) == null)
				{
					this.mTask.get_Owner().get_gameObject().AddComponent(componentType);
				}
			}
			List<Type> baseClasses = FieldInspector.GetBaseClasses(this.mTask.GetType());
			BindingFlags bindingFlags = 54;
			for (int i = baseClasses.get_Count() - 1; i > -1; i--)
			{
				FieldInfo[] fields = baseClasses.get_Item(i).GetFields(bindingFlags);
				for (int j = 0; j < fields.Length; j++)
				{
					if (typeof(SharedVariable).IsAssignableFrom(fields[j].get_FieldType()) && !fields[j].get_FieldType().get_IsAbstract())
					{
						SharedVariable sharedVariable = fields[j].GetValue(this.mTask) as SharedVariable;
						if (sharedVariable == null)
						{
							sharedVariable = (Activator.CreateInstance(fields[j].get_FieldType()) as SharedVariable);
						}
						if (TaskUtility.HasAttribute(fields[j], typeof(RequiredFieldAttribute)) || TaskUtility.HasAttribute(fields[j], typeof(SharedRequiredAttribute)))
						{
							sharedVariable.set_IsShared(true);
						}
						fields[j].SetValue(this.mTask, sharedVariable);
					}
				}
			}
		}

		private void LoadTaskIcon()
		{
			this.mTask.get_NodeData().set_Icon(null);
			TaskIconAttribute[] array;
			if ((array = (this.mTask.GetType().GetCustomAttributes(typeof(TaskIconAttribute), false) as TaskIconAttribute[])).Length > 0)
			{
				this.mTask.get_NodeData().set_Icon(BehaviorDesignerUtility.LoadIcon(array[0].get_IconPath(), null));
			}
			if (this.mTask.get_NodeData().get_Icon() == null)
			{
				string iconName = string.Empty;
				if (this.mTask.GetType().IsSubclassOf(typeof(Action)))
				{
					iconName = "{SkinColor}ActionIcon.png";
				}
				else if (this.mTask.GetType().IsSubclassOf(typeof(Conditional)))
				{
					iconName = "{SkinColor}ConditionalIcon.png";
				}
				else if (this.mTask.GetType().IsSubclassOf(typeof(Composite)))
				{
					iconName = "{SkinColor}CompositeIcon.png";
				}
				else if (this.mTask.GetType().IsSubclassOf(typeof(Decorator)))
				{
					iconName = "{SkinColor}DecoratorIcon.png";
				}
				else
				{
					iconName = "{SkinColor}EntryIcon.png";
				}
				this.mTask.get_NodeData().set_Icon(BehaviorDesignerUtility.LoadIcon(iconName, null));
			}
		}

		private void Init()
		{
			this.taskName = BehaviorDesignerUtility.SplitCamelCase(this.mTask.GetType().get_Name().ToString());
			this.isParent = this.mTask.GetType().IsSubclassOf(typeof(ParentTask));
			if (this.isParent)
			{
				this.outgoingNodeConnections = new List<NodeConnection>();
			}
			this.mRectIsDirty = (this.mCacheIsDirty = true);
			this.mIncomingRectIsDirty = true;
			this.mOutgoingRectIsDirty = true;
		}

		public void MakeEntryDisplay()
		{
			this.isEntryDisplay = (this.isParent = true);
			this.mTask.set_FriendlyName(this.taskName = "Entry");
			this.outgoingNodeConnections = new List<NodeConnection>();
		}

		public Vector2 GetAbsolutePosition()
		{
			Vector2 vector = this.mTask.get_NodeData().get_Offset();
			if (this.parentNodeDesigner != null)
			{
				vector += this.parentNodeDesigner.GetAbsolutePosition();
			}
			if (BehaviorDesignerPreferences.GetBool(BDPreferences.SnapToGrid))
			{
				vector.Set(BehaviorDesignerUtility.RoundToNearest(vector.x, 10f), BehaviorDesignerUtility.RoundToNearest(vector.y, 10f));
			}
			return vector;
		}

		public Rect Rectangle(Vector2 offset, bool includeConnections, bool includeComments)
		{
			Rect result = this.Rectangle(offset);
			if (includeConnections)
			{
				if (!this.isEntryDisplay)
				{
					result.set_yMin(result.get_yMin() - 14f);
				}
				if (this.isParent)
				{
					result.set_yMax(result.get_yMax() + 16f);
				}
			}
			if (includeComments && this.mTask != null)
			{
				if (this.mTask.get_NodeData().get_WatchedFields() != null && this.mTask.get_NodeData().get_WatchedFields().get_Count() > 0 && result.get_xMax() < this.watchedFieldRect.get_xMax())
				{
					result.set_xMax(this.watchedFieldRect.get_xMax());
				}
				if (!this.mTask.get_NodeData().get_Comment().Equals(string.Empty))
				{
					if (result.get_xMax() < this.commentRect.get_xMax())
					{
						result.set_xMax(this.commentRect.get_xMax());
					}
					if (result.get_yMax() < this.commentRect.get_yMax())
					{
						result.set_yMax(this.commentRect.get_yMax());
					}
				}
			}
			return result;
		}

		private Rect Rectangle(Vector2 offset)
		{
			if (!this.mRectIsDirty)
			{
				return this.mRectangle;
			}
			this.mCacheIsDirty = true;
			if (this.mTask == null)
			{
				return default(Rect);
			}
			float num = BehaviorDesignerUtility.TaskTitleGUIStyle.CalcSize(new GUIContent(this.ToString())).x + 20f;
			if (!this.isParent)
			{
				float num2;
				float num3;
				BehaviorDesignerUtility.TaskCommentGUIStyle.CalcMinMaxWidth(new GUIContent(this.mTask.get_NodeData().get_Comment()), ref num2, ref num3);
				num3 += 20f;
				num = ((num <= num3) ? num3 : num);
			}
			num = Mathf.Min(220f, Mathf.Max(100f, num));
			Vector2 absolutePosition = this.GetAbsolutePosition();
			float num4 = (float)(20 + ((!BehaviorDesignerPreferences.GetBool(BDPreferences.CompactMode)) ? 52 : 22));
			this.mRectangle = new Rect(absolutePosition.x + offset.x - num / 2f, absolutePosition.y + offset.y, num, num4);
			this.mRectIsDirty = false;
			return this.mRectangle;
		}

		private void UpdateCache(Rect nodeRect)
		{
			if (this.mCacheIsDirty)
			{
				this.nodeCollapsedTextureRect = new Rect(nodeRect.get_x() + (nodeRect.get_width() - 26f) / 2f + 1f, nodeRect.get_yMax() + 2f, 26f, 6f);
				this.iconTextureRect = new Rect(nodeRect.get_x() + (nodeRect.get_width() - 44f) / 2f, nodeRect.get_y() + 4f + 2f, 44f, 44f);
				this.titleRect = new Rect(nodeRect.get_x(), nodeRect.get_yMax() - (float)((!BehaviorDesignerPreferences.GetBool(BDPreferences.CompactMode)) ? 20 : 28) - 1f, nodeRect.get_width(), 20f);
				this.breakpointTextureRect = new Rect(nodeRect.get_xMax() - 16f, nodeRect.get_y() + 3f, 14f, 14f);
				this.errorTextureRect = new Rect(nodeRect.get_xMax() - 12f, nodeRect.get_y() - 8f, 20f, 20f);
				this.referenceTextureRect = new Rect(nodeRect.get_x() + 2f, nodeRect.get_y() + 3f, 14f, 14f);
				this.conditionalAbortTextureRect = new Rect(nodeRect.get_x() + 3f, nodeRect.get_y() + 3f, 16f, 16f);
				this.conditionalAbortLowerPriorityTextureRect = new Rect(nodeRect.get_x() + 3f, nodeRect.get_y(), 16f, 16f);
				this.disabledButtonTextureRect = new Rect(nodeRect.get_x() - 1f, nodeRect.get_y() - 17f, 14f, 14f);
				this.collapseButtonTextureRect = new Rect(nodeRect.get_x() + 15f, nodeRect.get_y() - 17f, 14f, 14f);
				this.incomingConnectionTextureRect = new Rect(nodeRect.get_x() + (nodeRect.get_width() - 42f) / 2f, nodeRect.get_y() - 14f - 3f + 3f, 42f, 17f);
				this.outgoingConnectionTextureRect = new Rect(nodeRect.get_x() + (nodeRect.get_width() - 42f) / 2f, nodeRect.get_yMax() - 3f, 42f, 19f);
				this.successReevaluatingExecutionStatusTextureRect = new Rect(nodeRect.get_xMax() - 37f, nodeRect.get_yMax() - 38f, 35f, 36f);
				this.successExecutionStatusTextureRect = new Rect(nodeRect.get_xMax() - 37f, nodeRect.get_yMax() - 33f, 35f, 31f);
				this.failureExecutionStatusTextureRect = new Rect(nodeRect.get_xMax() - 37f, nodeRect.get_yMax() - 38f, 35f, 36f);
				this.iconBorderTextureRect = new Rect(nodeRect.get_x() + (nodeRect.get_width() - 46f) / 2f, nodeRect.get_y() + 3f + 2f, 46f, 46f);
				this.CalculateNodeCommentRect(nodeRect);
				this.mCacheIsDirty = false;
			}
		}

		private void CalculateNodeCommentRect(Rect nodeRect)
		{
			bool flag = false;
			if (this.mTask.get_NodeData().get_WatchedFields() != null && this.mTask.get_NodeData().get_WatchedFields().get_Count() > 0)
			{
				string text = string.Empty;
				string text2 = string.Empty;
				for (int i = 0; i < this.mTask.get_NodeData().get_WatchedFields().get_Count(); i++)
				{
					FieldInfo fieldInfo = this.mTask.get_NodeData().get_WatchedFields().get_Item(i);
					text = text + BehaviorDesignerUtility.SplitCamelCase(fieldInfo.get_Name()) + ": \n";
					text2 = text2 + ((fieldInfo.GetValue(this.mTask) == null) ? "null" : fieldInfo.GetValue(this.mTask).ToString()) + "\n";
				}
				float num;
				float num2;
				BehaviorDesignerUtility.TaskCommentGUIStyle.CalcMinMaxWidth(new GUIContent(text), ref num, ref num2);
				float num3;
				BehaviorDesignerUtility.TaskCommentGUIStyle.CalcMinMaxWidth(new GUIContent(text2), ref num, ref num3);
				float num4 = num2;
				float num5 = num3;
				float num6 = Mathf.Min(220f, num2 + num3 + 20f);
				if (num6 == 220f)
				{
					num4 = num2 / (num2 + num3) * 220f;
					num5 = num3 / (num2 + num3) * 220f;
				}
				this.watchedFieldRect = new Rect(nodeRect.get_xMax() + 4f, nodeRect.get_y(), num6 + 8f, nodeRect.get_height());
				this.watchedFieldNamesRect = new Rect(nodeRect.get_xMax() + 6f, nodeRect.get_y() + 4f, num4, nodeRect.get_height() - 8f);
				this.watchedFieldValuesRect = new Rect(nodeRect.get_xMax() + 6f + num4, nodeRect.get_y() + 4f, num5, nodeRect.get_height() - 8f);
				flag = true;
			}
			if (!this.mTask.get_NodeData().get_Comment().Equals(string.Empty))
			{
				if (this.isParent)
				{
					float num7;
					float num8;
					BehaviorDesignerUtility.TaskCommentGUIStyle.CalcMinMaxWidth(new GUIContent(this.mTask.get_NodeData().get_Comment()), ref num7, ref num8);
					float num9 = Mathf.Min(220f, num8 + 20f);
					if (flag)
					{
						this.commentRect = new Rect(nodeRect.get_xMin() - 12f - num9, nodeRect.get_y(), num9 + 8f, nodeRect.get_height());
						this.commentLabelRect = new Rect(nodeRect.get_xMin() - 6f - num9, nodeRect.get_y() + 4f, num9, nodeRect.get_height() - 8f);
					}
					else
					{
						this.commentRect = new Rect(nodeRect.get_xMax() + 4f, nodeRect.get_y(), num9 + 8f, nodeRect.get_height());
						this.commentLabelRect = new Rect(nodeRect.get_xMax() + 6f, nodeRect.get_y() + 4f, num9, nodeRect.get_height() - 8f);
					}
				}
				else
				{
					float num10 = Mathf.Min(100f, BehaviorDesignerUtility.TaskCommentGUIStyle.CalcHeight(new GUIContent(this.mTask.get_NodeData().get_Comment()), nodeRect.get_width() - 4f));
					this.commentRect = new Rect(nodeRect.get_x(), nodeRect.get_yMax() + 4f, nodeRect.get_width(), num10 + 4f);
					this.commentLabelRect = new Rect(nodeRect.get_x(), nodeRect.get_yMax() + 4f, nodeRect.get_width() - 4f, num10);
				}
			}
		}

		public bool DrawNode(Vector2 offset, bool drawSelected, bool disabled)
		{
			if (drawSelected != this.mSelected)
			{
				return false;
			}
			if (this.ToString().get_Length() != this.prevFriendlyNameLength)
			{
				this.prevFriendlyNameLength = this.ToString().get_Length();
				this.mRectIsDirty = true;
			}
			Rect rect = this.Rectangle(offset, false, false);
			this.UpdateCache(rect);
			bool flag = (this.mTask.get_NodeData().get_PushTime() != -1f && this.mTask.get_NodeData().get_PushTime() >= this.mTask.get_NodeData().get_PopTime()) || (this.isEntryDisplay && this.outgoingNodeConnections.get_Count() > 0 && this.outgoingNodeConnections.get_Item(0).DestinationNodeDesigner.Task.get_NodeData().get_PushTime() != -1f);
			bool flag2 = this.mIdentifyUpdateCount != -1;
			bool result = this.prevRunningState != flag;
			float num = (!BehaviorDesignerPreferences.GetBool(BDPreferences.FadeNodes)) ? 0.01f : 0.5f;
			float num2 = 0f;
			if (flag2)
			{
				if (2000 - this.mIdentifyUpdateCount < 500)
				{
					num2 = (float)(2000 - this.mIdentifyUpdateCount) / 500f;
				}
				else
				{
					num2 = 1f;
				}
				if (this.mIdentifyUpdateCount != -1)
				{
					this.mIdentifyUpdateCount++;
					if (this.mIdentifyUpdateCount > 2000)
					{
						this.mIdentifyUpdateCount = -1;
					}
				}
				result = true;
			}
			else if (flag)
			{
				num2 = 1f;
			}
			else if ((this.mTask.get_NodeData().get_PopTime() != -1f && num != 0f && this.mTask.get_NodeData().get_PopTime() <= Time.get_realtimeSinceStartup() && Time.get_realtimeSinceStartup() - this.mTask.get_NodeData().get_PopTime() < num) || (this.isEntryDisplay && this.outgoingNodeConnections.get_Count() > 0 && this.outgoingNodeConnections.get_Item(0).DestinationNodeDesigner.Task.get_NodeData().get_PopTime() != -1f && this.outgoingNodeConnections.get_Item(0).DestinationNodeDesigner.Task.get_NodeData().get_PopTime() <= Time.get_realtimeSinceStartup() && Time.get_realtimeSinceStartup() - this.outgoingNodeConnections.get_Item(0).DestinationNodeDesigner.Task.get_NodeData().get_PopTime() < num))
			{
				if (this.isEntryDisplay)
				{
					num2 = 1f - (Time.get_realtimeSinceStartup() - this.outgoingNodeConnections.get_Item(0).DestinationNodeDesigner.Task.get_NodeData().get_PopTime()) / num;
				}
				else
				{
					num2 = 1f - (Time.get_realtimeSinceStartup() - this.mTask.get_NodeData().get_PopTime()) / num;
				}
				result = true;
			}
			if (!this.isEntryDisplay && !this.prevRunningState && this.parentNodeDesigner != null)
			{
				this.parentNodeDesigner.BringConnectionToFront(this);
			}
			this.prevRunningState = flag;
			if (num2 != 1f)
			{
				GUI.set_color((!disabled && !this.mTask.get_Disabled()) ? Color.get_white() : this.grayColor);
				GUIStyle backgroundGUIStyle;
				if (BehaviorDesignerPreferences.GetBool(BDPreferences.CompactMode))
				{
					backgroundGUIStyle = ((!this.mSelected) ? BehaviorDesignerUtility.GetTaskCompactGUIStyle(this.mTask.get_NodeData().get_ColorIndex()) : BehaviorDesignerUtility.GetTaskSelectedCompactGUIStyle(this.mTask.get_NodeData().get_ColorIndex()));
				}
				else
				{
					backgroundGUIStyle = ((!this.mSelected) ? BehaviorDesignerUtility.GetTaskGUIStyle(this.mTask.get_NodeData().get_ColorIndex()) : BehaviorDesignerUtility.GetTaskSelectedGUIStyle(this.mTask.get_NodeData().get_ColorIndex()));
				}
				this.DrawNodeTexture(rect, BehaviorDesignerUtility.GetTaskConnectionTopTexture(this.mTask.get_NodeData().get_ColorIndex()), BehaviorDesignerUtility.GetTaskConnectionBottomTexture(this.mTask.get_NodeData().get_ColorIndex()), backgroundGUIStyle, BehaviorDesignerUtility.GetTaskBorderTexture(this.mTask.get_NodeData().get_ColorIndex()));
			}
			if (num2 > 0f)
			{
				GUIStyle backgroundGUIStyle2;
				Texture2D iconBorderTexture;
				if (flag2)
				{
					if (BehaviorDesignerPreferences.GetBool(BDPreferences.CompactMode))
					{
						if (this.mSelected)
						{
							backgroundGUIStyle2 = BehaviorDesignerUtility.TaskIdentifySelectedCompactGUIStyle;
						}
						else
						{
							backgroundGUIStyle2 = BehaviorDesignerUtility.TaskIdentifyCompactGUIStyle;
						}
					}
					else if (this.mSelected)
					{
						backgroundGUIStyle2 = BehaviorDesignerUtility.TaskIdentifySelectedGUIStyle;
					}
					else
					{
						backgroundGUIStyle2 = BehaviorDesignerUtility.TaskIdentifyGUIStyle;
					}
					iconBorderTexture = BehaviorDesignerUtility.TaskBorderIdentifyTexture;
				}
				else
				{
					if (BehaviorDesignerPreferences.GetBool(BDPreferences.CompactMode))
					{
						if (this.mSelected)
						{
							backgroundGUIStyle2 = BehaviorDesignerUtility.TaskRunningSelectedCompactGUIStyle;
						}
						else
						{
							backgroundGUIStyle2 = BehaviorDesignerUtility.TaskRunningCompactGUIStyle;
						}
					}
					else if (this.mSelected)
					{
						backgroundGUIStyle2 = BehaviorDesignerUtility.TaskRunningSelectedGUIStyle;
					}
					else
					{
						backgroundGUIStyle2 = BehaviorDesignerUtility.TaskRunningGUIStyle;
					}
					iconBorderTexture = BehaviorDesignerUtility.TaskBorderRunningTexture;
				}
				Color color = (!disabled && !this.mTask.get_Disabled()) ? Color.get_white() : this.grayColor;
				color.a = num2;
				GUI.set_color(color);
				Texture2D connectionTopTexture = null;
				Texture2D connectionBottomTexture = null;
				if (!this.isEntryDisplay)
				{
					if (flag2)
					{
						connectionTopTexture = BehaviorDesignerUtility.TaskConnectionIdentifyTopTexture;
					}
					else
					{
						connectionTopTexture = BehaviorDesignerUtility.TaskConnectionRunningTopTexture;
					}
				}
				if (this.isParent)
				{
					if (flag2)
					{
						connectionBottomTexture = BehaviorDesignerUtility.TaskConnectionIdentifyBottomTexture;
					}
					else
					{
						connectionBottomTexture = BehaviorDesignerUtility.TaskConnectionRunningBottomTexture;
					}
				}
				this.DrawNodeTexture(rect, connectionTopTexture, connectionBottomTexture, backgroundGUIStyle2, iconBorderTexture);
				GUI.set_color(Color.get_white());
			}
			if (this.mTask.get_NodeData().get_Collapsed())
			{
				GUI.DrawTexture(this.nodeCollapsedTextureRect, BehaviorDesignerUtility.TaskConnectionCollapsedTexture);
			}
			if (!BehaviorDesignerPreferences.GetBool(BDPreferences.CompactMode))
			{
				GUI.DrawTexture(this.iconTextureRect, this.mTask.get_NodeData().get_Icon());
			}
			if (this.mTask.get_NodeData().get_InterruptTime() != -1f && Time.get_realtimeSinceStartup() - this.mTask.get_NodeData().get_InterruptTime() < 0.75f + num)
			{
				float a;
				if (Time.get_realtimeSinceStartup() - this.mTask.get_NodeData().get_InterruptTime() < 0.75f)
				{
					a = 1f;
				}
				else
				{
					a = 1f - (Time.get_realtimeSinceStartup() - (this.mTask.get_NodeData().get_InterruptTime() + 0.75f)) / num;
				}
				Color white = Color.get_white();
				white.a = a;
				GUI.set_color(white);
				GUI.Label(rect, string.Empty, BehaviorDesignerUtility.TaskHighlightGUIStyle);
				GUI.set_color(Color.get_white());
			}
			GUI.Label(this.titleRect, this.ToString(), BehaviorDesignerUtility.TaskTitleGUIStyle);
			if (this.mTask.get_NodeData().get_IsBreakpoint())
			{
				GUI.DrawTexture(this.breakpointTextureRect, BehaviorDesignerUtility.BreakpointTexture);
			}
			if (this.showReferenceIcon)
			{
				GUI.DrawTexture(this.referenceTextureRect, BehaviorDesignerUtility.ReferencedTexture);
			}
			if (this.hasError)
			{
				GUI.DrawTexture(this.errorTextureRect, BehaviorDesignerUtility.ErrorIconTexture);
			}
			if (this.mTask is Composite && (this.mTask as Composite).get_AbortType() != null)
			{
				switch ((this.mTask as Composite).get_AbortType())
				{
				case 1:
					GUI.DrawTexture(this.conditionalAbortTextureRect, BehaviorDesignerUtility.ConditionalAbortSelfTexture);
					break;
				case 2:
					GUI.DrawTexture(this.conditionalAbortLowerPriorityTextureRect, BehaviorDesignerUtility.ConditionalAbortLowerPriorityTexture);
					break;
				case 3:
					GUI.DrawTexture(this.conditionalAbortTextureRect, BehaviorDesignerUtility.ConditionalAbortBothTexture);
					break;
				}
			}
			GUI.set_color(Color.get_white());
			if (this.showHoverBar)
			{
				GUI.DrawTexture(this.disabledButtonTextureRect, (!this.mTask.get_Disabled()) ? BehaviorDesignerUtility.DisableTaskTexture : BehaviorDesignerUtility.EnableTaskTexture, 2);
				if (this.isParent || this.mTask is BehaviorReference)
				{
					bool collapsed = this.mTask.get_NodeData().get_Collapsed();
					if (this.mTask is BehaviorReference)
					{
						collapsed = (this.mTask as BehaviorReference).collapsed;
					}
					GUI.DrawTexture(this.collapseButtonTextureRect, (!collapsed) ? BehaviorDesignerUtility.CollapseTaskTexture : BehaviorDesignerUtility.ExpandTaskTexture, 2);
				}
			}
			return result;
		}

		private void DrawNodeTexture(Rect nodeRect, Texture2D connectionTopTexture, Texture2D connectionBottomTexture, GUIStyle backgroundGUIStyle, Texture2D iconBorderTexture)
		{
			if (!this.isEntryDisplay)
			{
				GUI.DrawTexture(this.incomingConnectionTextureRect, connectionTopTexture, 2);
			}
			if (this.isParent)
			{
				GUI.DrawTexture(this.outgoingConnectionTextureRect, connectionBottomTexture, 2);
			}
			GUI.Label(nodeRect, string.Empty, backgroundGUIStyle);
			if (this.mTask.get_NodeData().get_ExecutionStatus() == 2)
			{
				if (this.mTask.get_NodeData().get_IsReevaluating())
				{
					GUI.DrawTexture(this.successReevaluatingExecutionStatusTextureRect, BehaviorDesignerUtility.ExecutionSuccessRepeatTexture);
				}
				else
				{
					GUI.DrawTexture(this.successExecutionStatusTextureRect, BehaviorDesignerUtility.ExecutionSuccessTexture);
				}
			}
			else if (this.mTask.get_NodeData().get_ExecutionStatus() == 1)
			{
				GUI.DrawTexture(this.failureExecutionStatusTextureRect, (!this.mTask.get_NodeData().get_IsReevaluating()) ? BehaviorDesignerUtility.ExecutionFailureTexture : BehaviorDesignerUtility.ExecutionFailureRepeatTexture);
			}
			if (!BehaviorDesignerPreferences.GetBool(BDPreferences.CompactMode))
			{
				GUI.DrawTexture(this.iconBorderTextureRect, iconBorderTexture);
			}
		}

		public void DrawNodeConnection(Vector2 offset, bool disabled)
		{
			if (this.mConnectionIsDirty)
			{
				this.DetermineConnectionHorizontalHeight(this.Rectangle(offset, false, false), offset);
				this.mConnectionIsDirty = false;
			}
			if (this.isParent)
			{
				for (int i = 0; i < this.outgoingNodeConnections.get_Count(); i++)
				{
					this.outgoingNodeConnections.get_Item(i).DrawConnection(offset, disabled);
				}
			}
		}

		public void DrawNodeComment(Vector2 offset)
		{
			if (this.mTask.get_NodeData().get_Comment().get_Length() != this.prevCommentLength)
			{
				this.prevCommentLength = this.mTask.get_NodeData().get_Comment().get_Length();
				this.mRectIsDirty = true;
			}
			if (this.mTask.get_NodeData().get_WatchedFields() != null && this.mTask.get_NodeData().get_WatchedFields().get_Count() > 0)
			{
				if (this.mTask.get_NodeData().get_WatchedFields().get_Count() != this.prevWatchedFieldsLength.get_Count())
				{
					this.mRectIsDirty = true;
					this.prevWatchedFieldsLength.Clear();
					for (int i = 0; i < this.mTask.get_NodeData().get_WatchedFields().get_Count(); i++)
					{
						if (this.mTask.get_NodeData().get_WatchedFields().get_Item(i) != null)
						{
							object value = this.mTask.get_NodeData().get_WatchedFields().get_Item(i).GetValue(this.mTask);
							if (value != null)
							{
								this.prevWatchedFieldsLength.Add(value.ToString().get_Length());
							}
							else
							{
								this.prevWatchedFieldsLength.Add(0);
							}
						}
					}
				}
				else
				{
					for (int j = 0; j < this.mTask.get_NodeData().get_WatchedFields().get_Count(); j++)
					{
						if (this.mTask.get_NodeData().get_WatchedFields().get_Item(j) != null)
						{
							object value2 = this.mTask.get_NodeData().get_WatchedFields().get_Item(j).GetValue(this.mTask);
							int num = 0;
							if (value2 != null)
							{
								num = value2.ToString().get_Length();
							}
							if (num != this.prevWatchedFieldsLength.get_Item(j))
							{
								this.mRectIsDirty = true;
								break;
							}
						}
					}
				}
			}
			if (this.mTask.get_NodeData().get_Comment().Equals(string.Empty) && (this.mTask.get_NodeData().get_WatchedFields() == null || this.mTask.get_NodeData().get_WatchedFields().get_Count() == 0))
			{
				return;
			}
			if (this.mTask.get_NodeData().get_WatchedFields() != null && this.mTask.get_NodeData().get_WatchedFields().get_Count() > 0)
			{
				string text = string.Empty;
				string text2 = string.Empty;
				for (int k = 0; k < this.mTask.get_NodeData().get_WatchedFields().get_Count(); k++)
				{
					FieldInfo fieldInfo = this.mTask.get_NodeData().get_WatchedFields().get_Item(k);
					text = text + BehaviorDesignerUtility.SplitCamelCase(fieldInfo.get_Name()) + ": \n";
					text2 = text2 + ((fieldInfo.GetValue(this.mTask) == null) ? "null" : fieldInfo.GetValue(this.mTask).ToString()) + "\n";
				}
				GUI.Box(this.watchedFieldRect, string.Empty, BehaviorDesignerUtility.TaskDescriptionGUIStyle);
				GUI.Label(this.watchedFieldNamesRect, text, BehaviorDesignerUtility.TaskCommentRightAlignGUIStyle);
				GUI.Label(this.watchedFieldValuesRect, text2, BehaviorDesignerUtility.TaskCommentLeftAlignGUIStyle);
			}
			if (!this.mTask.get_NodeData().get_Comment().Equals(string.Empty))
			{
				GUI.Box(this.commentRect, string.Empty, BehaviorDesignerUtility.TaskDescriptionGUIStyle);
				GUI.Label(this.commentLabelRect, this.mTask.get_NodeData().get_Comment(), BehaviorDesignerUtility.TaskCommentGUIStyle);
			}
		}

		public bool Contains(Vector2 point, Vector2 offset, bool includeConnections)
		{
			return this.Rectangle(offset, includeConnections, false).Contains(point);
		}

		public NodeConnection NodeConnectionRectContains(Vector2 point, Vector2 offset)
		{
			bool incomingNodeConnection;
			if ((incomingNodeConnection = this.IncomingConnectionRect(offset).Contains(point)) || (this.isParent && this.OutgoingConnectionRect(offset).Contains(point)))
			{
				return this.CreateNodeConnection(incomingNodeConnection);
			}
			return null;
		}

		public NodeConnection CreateNodeConnection(bool incomingNodeConnection)
		{
			NodeConnection nodeConnection = ScriptableObject.CreateInstance<NodeConnection>();
			nodeConnection.LoadConnection(this, (!incomingNodeConnection) ? NodeConnectionType.Outgoing : NodeConnectionType.Incoming);
			return nodeConnection;
		}

		public void ConnectionContains(Vector2 point, Vector2 offset, ref List<NodeConnection> nodeConnections)
		{
			if (this.outgoingNodeConnections == null || this.isEntryDisplay)
			{
				return;
			}
			for (int i = 0; i < this.outgoingNodeConnections.get_Count(); i++)
			{
				if (this.outgoingNodeConnections.get_Item(i).Contains(point, offset))
				{
					nodeConnections.Add(this.outgoingNodeConnections.get_Item(i));
				}
			}
		}

		private void DetermineConnectionHorizontalHeight(Rect nodeRect, Vector2 offset)
		{
			if (this.isParent)
			{
				float num = 3.40282347E+38f;
				float num2 = num;
				for (int i = 0; i < this.outgoingNodeConnections.get_Count(); i++)
				{
					Rect rect = this.outgoingNodeConnections.get_Item(i).DestinationNodeDesigner.Rectangle(offset, false, false);
					if (rect.get_y() < num)
					{
						num = rect.get_y();
						num2 = rect.get_y();
					}
				}
				num = num * 0.75f + nodeRect.get_yMax() * 0.25f;
				if (num < nodeRect.get_yMax() + 15f)
				{
					num = nodeRect.get_yMax() + 15f;
				}
				else if (num > num2 - 15f)
				{
					num = num2 - 15f;
				}
				for (int j = 0; j < this.outgoingNodeConnections.get_Count(); j++)
				{
					this.outgoingNodeConnections.get_Item(j).HorizontalHeight = num;
				}
			}
		}

		public Vector2 GetConnectionPosition(Vector2 offset, NodeConnectionType connectionType)
		{
			Vector2 result;
			if (connectionType == NodeConnectionType.Incoming)
			{
				Rect rect = this.IncomingConnectionRect(offset);
				result = new Vector2(rect.get_center().x, rect.get_y() + 7f);
			}
			else
			{
				Rect rect2 = this.OutgoingConnectionRect(offset);
				result = new Vector2(rect2.get_center().x, rect2.get_yMax() - 8f);
			}
			return result;
		}

		public bool HoverBarAreaContains(Vector2 point, Vector2 offset)
		{
			Rect rect = this.Rectangle(offset, false, false);
			rect.set_y(rect.get_y() - 24f);
			return rect.Contains(point);
		}

		public bool HoverBarButtonClick(Vector2 point, Vector2 offset, ref bool collapsedButtonClicked)
		{
			Rect rect = this.Rectangle(offset, false, false);
			Rect rect2 = new Rect(rect.get_x() - 1f, rect.get_y() - 17f, 14f, 14f);
			Rect rect3 = rect2;
			bool flag = false;
			if (rect2.Contains(point))
			{
				this.mTask.set_Disabled(!this.mTask.get_Disabled());
				flag = true;
			}
			if (!flag && (this.isParent || this.mTask is BehaviorReference))
			{
				Rect rect4 = new Rect(rect.get_x() + 15f, rect.get_y() - 17f, 14f, 14f);
				rect3.set_xMax(rect4.get_xMax());
				if (rect4.Contains(point))
				{
					if (this.mTask is BehaviorReference)
					{
						(this.mTask as BehaviorReference).collapsed = !(this.mTask as BehaviorReference).collapsed;
					}
					else
					{
						this.mTask.get_NodeData().set_Collapsed(!this.mTask.get_NodeData().get_Collapsed());
					}
					collapsedButtonClicked = true;
					flag = true;
				}
			}
			if (!flag && rect3.Contains(point))
			{
				flag = true;
			}
			return flag;
		}

		public bool Intersects(Rect rect, Vector2 offset)
		{
			Rect rect2 = this.Rectangle(offset, false, false);
			return rect2.get_xMin() < rect.get_xMax() && rect2.get_xMax() > rect.get_xMin() && rect2.get_yMin() < rect.get_yMax() && rect2.get_yMax() > rect.get_yMin();
		}

		public void ChangeOffset(Vector2 delta)
		{
			Vector2 vector = this.mTask.get_NodeData().get_Offset();
			vector += delta;
			this.mTask.get_NodeData().set_Offset(vector);
			this.MarkDirty();
			if (this.parentNodeDesigner != null)
			{
				this.parentNodeDesigner.MarkDirty();
			}
		}

		public void AddChildNode(NodeDesigner childNodeDesigner, NodeConnection nodeConnection, bool adjustOffset, bool replaceNode)
		{
			this.AddChildNode(childNodeDesigner, nodeConnection, adjustOffset, replaceNode, -1);
		}

		public void AddChildNode(NodeDesigner childNodeDesigner, NodeConnection nodeConnection, bool adjustOffset, bool replaceNode, int replaceNodeIndex)
		{
			if (replaceNode)
			{
				ParentTask parentTask = this.mTask as ParentTask;
				parentTask.get_Children().set_Item(replaceNodeIndex, childNodeDesigner.Task);
			}
			else
			{
				if (!this.isEntryDisplay)
				{
					ParentTask parentTask2 = this.mTask as ParentTask;
					int i = 0;
					if (parentTask2.get_Children() != null)
					{
						for (i = 0; i < parentTask2.get_Children().get_Count(); i++)
						{
							if (childNodeDesigner.GetAbsolutePosition().x < (parentTask2.get_Children().get_Item(i).get_NodeData().get_NodeDesigner() as NodeDesigner).GetAbsolutePosition().x)
							{
								break;
							}
						}
					}
					parentTask2.AddChild(childNodeDesigner.Task, i);
				}
				if (adjustOffset)
				{
					NodeData expr_CA = childNodeDesigner.Task.get_NodeData();
					expr_CA.set_Offset(expr_CA.get_Offset() - this.GetAbsolutePosition());
				}
			}
			childNodeDesigner.ParentNodeDesigner = this;
			nodeConnection.DestinationNodeDesigner = childNodeDesigner;
			nodeConnection.NodeConnectionType = NodeConnectionType.Fixed;
			if (!nodeConnection.OriginatingNodeDesigner.Equals(this))
			{
				nodeConnection.OriginatingNodeDesigner = this;
			}
			this.outgoingNodeConnections.Add(nodeConnection);
			this.mConnectionIsDirty = true;
		}

		public void RemoveChildNode(NodeDesigner childNodeDesigner)
		{
			if (!this.isEntryDisplay)
			{
				ParentTask parentTask = this.mTask as ParentTask;
				parentTask.get_Children().Remove(childNodeDesigner.Task);
			}
			for (int i = 0; i < this.outgoingNodeConnections.get_Count(); i++)
			{
				NodeConnection nodeConnection = this.outgoingNodeConnections.get_Item(i);
				if (nodeConnection.DestinationNodeDesigner.Equals(childNodeDesigner) || nodeConnection.OriginatingNodeDesigner.Equals(childNodeDesigner))
				{
					this.outgoingNodeConnections.RemoveAt(i);
					break;
				}
			}
			childNodeDesigner.ParentNodeDesigner = null;
			this.mConnectionIsDirty = true;
		}

		public void SetID(ref int id)
		{
			this.mTask.set_ID(id++);
			if (this.isParent)
			{
				ParentTask parentTask = this.mTask as ParentTask;
				if (parentTask.get_Children() != null)
				{
					for (int i = 0; i < parentTask.get_Children().get_Count(); i++)
					{
						(parentTask.get_Children().get_Item(i).get_NodeData().get_NodeDesigner() as NodeDesigner).SetID(ref id);
					}
				}
			}
		}

		public int ChildIndexForTask(Task childTask)
		{
			if (this.isParent)
			{
				ParentTask parentTask = this.mTask as ParentTask;
				if (parentTask.get_Children() != null)
				{
					for (int i = 0; i < parentTask.get_Children().get_Count(); i++)
					{
						if (parentTask.get_Children().get_Item(i).Equals(childTask))
						{
							return i;
						}
					}
				}
			}
			return -1;
		}

		public NodeDesigner NodeDesignerForChildIndex(int index)
		{
			if (index < 0)
			{
				return null;
			}
			if (this.isParent)
			{
				ParentTask parentTask = this.mTask as ParentTask;
				if (parentTask.get_Children() != null)
				{
					if (index >= parentTask.get_Children().get_Count() || parentTask.get_Children().get_Item(index) == null)
					{
						return null;
					}
					return parentTask.get_Children().get_Item(index).get_NodeData().get_NodeDesigner() as NodeDesigner;
				}
			}
			return null;
		}

		public void MoveChildNode(int index, bool decreaseIndex)
		{
			int num = index + ((!decreaseIndex) ? 1 : -1);
			ParentTask parentTask = this.mTask as ParentTask;
			Task task = parentTask.get_Children().get_Item(index);
			parentTask.get_Children().set_Item(index, parentTask.get_Children().get_Item(num));
			parentTask.get_Children().set_Item(num, task);
		}

		private void BringConnectionToFront(NodeDesigner nodeDesigner)
		{
			for (int i = 0; i < this.outgoingNodeConnections.get_Count(); i++)
			{
				if (this.outgoingNodeConnections.get_Item(i).DestinationNodeDesigner.Equals(nodeDesigner))
				{
					NodeConnection nodeConnection = this.outgoingNodeConnections.get_Item(i);
					this.outgoingNodeConnections.set_Item(i, this.outgoingNodeConnections.get_Item(this.outgoingNodeConnections.get_Count() - 1));
					this.outgoingNodeConnections.set_Item(this.outgoingNodeConnections.get_Count() - 1, nodeConnection);
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
			return this.mTask.get_Disabled() || (this.parentNodeDesigner != null && this.parentNodeDesigner.IsDisabled());
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
			return !(this.parentNodeDesigner == null) && (this.parentNodeDesigner.Equals(nodeDesigner) || this.parentNodeDesigner.HasParent(nodeDesigner));
		}

		public void DestroyConnections()
		{
			if (this.outgoingNodeConnections != null)
			{
				for (int i = this.outgoingNodeConnections.get_Count() - 1; i > -1; i--)
				{
					Object.DestroyImmediate(this.outgoingNodeConnections.get_Item(i), true);
				}
			}
		}

		public override bool Equals(object obj)
		{
			return object.ReferenceEquals(this, obj);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public override string ToString()
		{
			return (this.mTask != null) ? ((!this.mTask.get_FriendlyName().Equals(string.Empty)) ? this.mTask.get_FriendlyName() : this.taskName) : string.Empty;
		}
	}
}
