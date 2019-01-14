using BehaviorDesigner.Runtime.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using UnityEngine;

namespace BehaviorDesigner.Runtime
{
	[AddComponentMenu("Behavior Designer/Behavior Manager")]
	public class BehaviorManager : MonoBehaviour
	{
		public enum ExecutionsPerTickType
		{
			NoDuplicates = 0,
			Count = 1
		}

		public class BehaviorTree
		{
			public class ConditionalReevaluate
			{
				public int index;

				public TaskStatus taskStatus;

				public int compositeIndex = -1;

				public int stackIndex = -1;

				public void Initialize(int i, TaskStatus status, int stack, int composite)
				{
					this.index = i;
					this.taskStatus = status;
					this.stackIndex = stack;
					this.compositeIndex = composite;
				}
			}

			public List<Task> taskList = new List<Task>();

			public List<int> parentIndex = new List<int>();

			public List<List<int>> childrenIndex = new List<List<int>>();

			public List<int> relativeChildIndex = new List<int>();

			public List<Stack<int>> activeStack = new List<Stack<int>>();

			public List<TaskStatus> nonInstantTaskStatus = new List<TaskStatus>();

			public List<int> interruptionIndex = new List<int>();

			public List<BehaviorManager.BehaviorTree.ConditionalReevaluate> conditionalReevaluate = new List<BehaviorManager.BehaviorTree.ConditionalReevaluate>();

			public Dictionary<int, BehaviorManager.BehaviorTree.ConditionalReevaluate> conditionalReevaluateMap = new Dictionary<int, BehaviorManager.BehaviorTree.ConditionalReevaluate>();

			public List<int> parentReevaluate = new List<int>();

			public List<int> parentCompositeIndex = new List<int>();

			public List<List<int>> childConditionalIndex = new List<List<int>>();

			public int executionCount;

			public Behavior behavior;

			public bool destroyBehavior;

			public void Initialize(Behavior b)
			{
				this.behavior = b;
				for (int i = this.childrenIndex.get_Count() - 1; i > -1; i--)
				{
					ObjectPool.Return<List<int>>(this.childrenIndex.get_Item(i));
				}
				for (int j = this.activeStack.get_Count() - 1; j > -1; j--)
				{
					ObjectPool.Return<Stack<int>>(this.activeStack.get_Item(j));
				}
				for (int k = this.childConditionalIndex.get_Count() - 1; k > -1; k--)
				{
					ObjectPool.Return<List<int>>(this.childConditionalIndex.get_Item(k));
				}
				this.taskList.Clear();
				this.parentIndex.Clear();
				this.childrenIndex.Clear();
				this.relativeChildIndex.Clear();
				this.activeStack.Clear();
				this.nonInstantTaskStatus.Clear();
				this.interruptionIndex.Clear();
				this.conditionalReevaluate.Clear();
				this.conditionalReevaluateMap.Clear();
				this.parentReevaluate.Clear();
				this.parentCompositeIndex.Clear();
				this.childConditionalIndex.Clear();
			}
		}

		public enum ThirdPartyObjectType
		{
			PlayMaker = 0,
			uScript = 1,
			DialogueSystem = 2,
			uSequencer = 3,
			ICode = 4
		}

		public class ThirdPartyTask
		{
			private Task task;

			private BehaviorManager.ThirdPartyObjectType thirdPartyObjectType;

			public Task Task
			{
				get
				{
					return this.task;
				}
				set
				{
					this.task = value;
				}
			}

			public BehaviorManager.ThirdPartyObjectType ThirdPartyObjectType
			{
				get
				{
					return this.thirdPartyObjectType;
				}
			}

			public void Initialize(Task t, BehaviorManager.ThirdPartyObjectType objectType)
			{
				this.task = t;
				this.thirdPartyObjectType = objectType;
			}
		}

		public class ThirdPartyTaskComparer : IEqualityComparer<BehaviorManager.ThirdPartyTask>
		{
			public bool Equals(BehaviorManager.ThirdPartyTask a, BehaviorManager.ThirdPartyTask b)
			{
				return !object.ReferenceEquals(a, null) && !object.ReferenceEquals(b, null) && a.Task.Equals(b.Task);
			}

			public int GetHashCode(BehaviorManager.ThirdPartyTask obj)
			{
				return (obj == null) ? 0 : obj.Task.GetHashCode();
			}
		}

		public class TaskAddData
		{
			public class OverrideFieldValue
			{
				private object value;

				private int depth;

				public object Value
				{
					get
					{
						return this.value;
					}
				}

				public int Depth
				{
					get
					{
						return this.depth;
					}
				}

				public void Initialize(object v, int d)
				{
					this.value = v;
					this.depth = d;
				}
			}

			public bool fromExternalTask;

			public ParentTask parentTask;

			public int parentIndex = -1;

			public int depth;

			public int compositeParentIndex = -1;

			public Vector2 offset;

			public Dictionary<string, BehaviorManager.TaskAddData.OverrideFieldValue> overrideFields;

			public HashSet<object> overiddenFields = new HashSet<object>();

			public int errorTask = -1;

			public string errorTaskName = string.Empty;

			public void Initialize()
			{
				if (this.overrideFields != null)
				{
					using (Dictionary<string, BehaviorManager.TaskAddData.OverrideFieldValue>.Enumerator enumerator = this.overrideFields.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							KeyValuePair<string, BehaviorManager.TaskAddData.OverrideFieldValue> current = enumerator.get_Current();
							ObjectPool.Return<KeyValuePair<string, BehaviorManager.TaskAddData.OverrideFieldValue>>(current);
						}
					}
				}
				ObjectPool.Return<Dictionary<string, BehaviorManager.TaskAddData.OverrideFieldValue>>(this.overrideFields);
				this.fromExternalTask = false;
				this.parentTask = null;
				this.parentIndex = -1;
				this.depth = 0;
				this.compositeParentIndex = -1;
				this.overrideFields = null;
			}
		}

		public delegate void BehaviorManagerHandler();

		public static BehaviorManager instance;

		[SerializeField]
		private UpdateIntervalType updateInterval;

		[SerializeField]
		private float updateIntervalSeconds;

		[SerializeField]
		private BehaviorManager.ExecutionsPerTickType executionsPerTick;

		[SerializeField]
		private int maxTaskExecutionsPerTick = 100;

		private WaitForSeconds updateWait;

		public BehaviorManager.BehaviorManagerHandler onEnableBehavior;

		public BehaviorManager.BehaviorManagerHandler onTaskBreakpoint;

		private List<BehaviorManager.BehaviorTree> behaviorTrees = new List<BehaviorManager.BehaviorTree>();

		private Dictionary<Behavior, BehaviorManager.BehaviorTree> pausedBehaviorTrees = new Dictionary<Behavior, BehaviorManager.BehaviorTree>();

		private Dictionary<Behavior, BehaviorManager.BehaviorTree> behaviorTreeMap = new Dictionary<Behavior, BehaviorManager.BehaviorTree>();

		private List<int> conditionalParentIndexes = new List<int>();

		private Dictionary<object, BehaviorManager.ThirdPartyTask> objectTaskMap = new Dictionary<object, BehaviorManager.ThirdPartyTask>();

		private Dictionary<BehaviorManager.ThirdPartyTask, object> taskObjectMap = new Dictionary<BehaviorManager.ThirdPartyTask, object>(new BehaviorManager.ThirdPartyTaskComparer());

		private BehaviorManager.ThirdPartyTask thirdPartyTaskCompare = new BehaviorManager.ThirdPartyTask();

		private static MethodInfo playMakerStopMethod;

		private static MethodInfo uScriptStopMethod;

		private static MethodInfo dialogueSystemStopMethod;

		private static MethodInfo uSequencerStopMethod;

		private static MethodInfo iCodeStopMethod;

		private static object[] invokeParameters;

		private Behavior breakpointTree;

		private bool dirty;

		public UpdateIntervalType UpdateInterval
		{
			get
			{
				return this.updateInterval;
			}
			set
			{
				this.updateInterval = value;
				this.UpdateIntervalChanged();
			}
		}

		public float UpdateIntervalSeconds
		{
			get
			{
				return this.updateIntervalSeconds;
			}
			set
			{
				this.updateIntervalSeconds = value;
				this.UpdateIntervalChanged();
			}
		}

		public BehaviorManager.ExecutionsPerTickType ExecutionsPerTick
		{
			get
			{
				return this.executionsPerTick;
			}
			set
			{
				this.executionsPerTick = value;
			}
		}

		public int MaxTaskExecutionsPerTick
		{
			get
			{
				return this.maxTaskExecutionsPerTick;
			}
			set
			{
				this.maxTaskExecutionsPerTick = value;
			}
		}

		public BehaviorManager.BehaviorManagerHandler OnEnableBehavior
		{
			set
			{
				this.onEnableBehavior = value;
			}
		}

		public BehaviorManager.BehaviorManagerHandler OnTaskBreakpoint
		{
			get
			{
				return this.onTaskBreakpoint;
			}
			set
			{
				this.onTaskBreakpoint = (BehaviorManager.BehaviorManagerHandler)Delegate.Combine(this.onTaskBreakpoint, value);
			}
		}

		public List<BehaviorManager.BehaviorTree> BehaviorTrees
		{
			get
			{
				return this.behaviorTrees;
			}
		}

		private static MethodInfo PlayMakerStopMethod
		{
			get
			{
				if (BehaviorManager.playMakerStopMethod == null)
				{
					BehaviorManager.playMakerStopMethod = TaskUtility.GetTypeWithinAssembly("BehaviorDesigner.Runtime.BehaviorManager_PlayMaker").GetMethod("StopPlayMaker");
				}
				return BehaviorManager.playMakerStopMethod;
			}
		}

		private static MethodInfo UScriptStopMethod
		{
			get
			{
				if (BehaviorManager.uScriptStopMethod == null)
				{
					BehaviorManager.uScriptStopMethod = TaskUtility.GetTypeWithinAssembly("BehaviorDesigner.Runtime.BehaviorManager_uScript").GetMethod("StopuScript");
				}
				return BehaviorManager.uScriptStopMethod;
			}
		}

		private static MethodInfo DialogueSystemStopMethod
		{
			get
			{
				if (BehaviorManager.dialogueSystemStopMethod == null)
				{
					BehaviorManager.dialogueSystemStopMethod = TaskUtility.GetTypeWithinAssembly("BehaviorDesigner.Runtime.BehaviorManager_DialogueSystem").GetMethod("StopDialogueSystem");
				}
				return BehaviorManager.dialogueSystemStopMethod;
			}
		}

		private static MethodInfo USequencerStopMethod
		{
			get
			{
				if (BehaviorManager.uSequencerStopMethod == null)
				{
					BehaviorManager.uSequencerStopMethod = TaskUtility.GetTypeWithinAssembly("BehaviorDesigner.Runtime.BehaviorManager_uSequencer").GetMethod("StopuSequencer");
				}
				return BehaviorManager.uSequencerStopMethod;
			}
		}

		private static MethodInfo ICodeStopMethod
		{
			get
			{
				if (BehaviorManager.iCodeStopMethod == null)
				{
					BehaviorManager.iCodeStopMethod = TaskUtility.GetTypeWithinAssembly("BehaviorDesigner.Runtime.BehaviorManager_ICode").GetMethod("StopICode");
				}
				return BehaviorManager.iCodeStopMethod;
			}
		}

		public Behavior BreakpointTree
		{
			get
			{
				return this.breakpointTree;
			}
			set
			{
				this.breakpointTree = value;
			}
		}

		public bool Dirty
		{
			get
			{
				return this.dirty;
			}
			set
			{
				this.dirty = value;
			}
		}

		public void Awake()
		{
			BehaviorManager.instance = this;
			this.UpdateIntervalChanged();
		}

		private void UpdateIntervalChanged()
		{
			base.StopCoroutine("CoroutineUpdate");
			if (this.updateInterval == UpdateIntervalType.EveryFrame)
			{
				base.set_enabled(true);
			}
			else if (this.updateInterval == UpdateIntervalType.SpecifySeconds)
			{
				if (Application.get_isPlaying())
				{
					this.updateWait = new WaitForSeconds(this.updateIntervalSeconds);
					base.StartCoroutine("CoroutineUpdate");
				}
				base.set_enabled(false);
			}
			else
			{
				base.set_enabled(false);
			}
		}

		public void OnDestroy()
		{
			for (int i = this.behaviorTrees.get_Count() - 1; i > -1; i--)
			{
				this.DisableBehavior(this.behaviorTrees.get_Item(i).behavior);
			}
			ObjectPool.Clear();
		}

		public void OnApplicationQuit()
		{
			for (int i = this.behaviorTrees.get_Count() - 1; i > -1; i--)
			{
				this.DisableBehavior(this.behaviorTrees.get_Item(i).behavior);
			}
		}

		public void EnableBehavior(Behavior behavior)
		{
			if (this.IsBehaviorEnabled(behavior))
			{
				return;
			}
			BehaviorManager.BehaviorTree behaviorTree;
			if (this.pausedBehaviorTrees.TryGetValue(behavior, ref behaviorTree))
			{
				this.behaviorTrees.Add(behaviorTree);
				this.pausedBehaviorTrees.Remove(behavior);
				behavior.ExecutionStatus = TaskStatus.Running;
				for (int i = 0; i < behaviorTree.taskList.get_Count(); i++)
				{
					behaviorTree.taskList.get_Item(i).OnPause(false);
				}
				return;
			}
			BehaviorManager.TaskAddData taskAddData = ObjectPool.Get<BehaviorManager.TaskAddData>();
			taskAddData.Initialize();
			behavior.CheckForSerialization();
			Task rootTask = behavior.GetBehaviorSource().RootTask;
			if (rootTask == null)
			{
				Debug.LogError(string.Format("The behavior \"{0}\" on GameObject \"{1}\" contains no root task. This behavior will be disabled.", behavior.GetBehaviorSource().behaviorName, behavior.get_gameObject().get_name()));
				return;
			}
			behaviorTree = ObjectPool.Get<BehaviorManager.BehaviorTree>();
			behaviorTree.Initialize(behavior);
			behaviorTree.parentIndex.Add(-1);
			behaviorTree.relativeChildIndex.Add(-1);
			behaviorTree.parentCompositeIndex.Add(-1);
			bool flag = behavior.ExternalBehavior != null;
			int num = this.AddToTaskList(behaviorTree, rootTask, ref flag, taskAddData);
			if (num < 0)
			{
				behaviorTree = null;
				int num2 = num;
				switch (num2 + 6)
				{
				case 0:
					Debug.LogError(string.Format("The behavior \"{0}\" on GameObject \"{1}\" contains a root task which is disabled. This behavior will be disabled.", new object[]
					{
						behavior.GetBehaviorSource().behaviorName,
						behavior.get_gameObject().get_name(),
						taskAddData.errorTaskName,
						taskAddData.errorTask
					}));
					break;
				case 1:
					Debug.LogError(string.Format("The behavior \"{0}\" on GameObject \"{1}\" contains a Behavior Tree Reference task ({2} (index {3})) that which has an element with a null value in the externalBehaviors array. This behavior will be disabled.", new object[]
					{
						behavior.GetBehaviorSource().behaviorName,
						behavior.get_gameObject().get_name(),
						taskAddData.errorTaskName,
						taskAddData.errorTask
					}));
					break;
				case 2:
					Debug.LogError(string.Format("The behavior \"{0}\" on GameObject \"{1}\" contains multiple external behavior trees at the root task or as a child of a parent task which cannot contain so many children (such as a decorator task). This behavior will be disabled.", behavior.GetBehaviorSource().behaviorName, behavior.get_gameObject().get_name()));
					break;
				case 3:
					Debug.LogError(string.Format("The behavior \"{0}\" on GameObject \"{1}\" contains a null task (referenced from parent task {2} (index {3})). This behavior will be disabled.", new object[]
					{
						behavior.GetBehaviorSource().behaviorName,
						behavior.get_gameObject().get_name(),
						taskAddData.errorTaskName,
						taskAddData.errorTask
					}));
					break;
				case 4:
					Debug.LogError(string.Format("The behavior \"{0}\" on GameObject \"{1}\" cannot find the referenced external task. This behavior will be disabled.", behavior.GetBehaviorSource().behaviorName, behavior.get_gameObject().get_name()));
					break;
				case 5:
					Debug.LogError(string.Format("The behavior \"{0}\" on GameObject \"{1}\" contains a parent task ({2} (index {3})) with no children. This behavior will be disabled.", new object[]
					{
						behavior.GetBehaviorSource().behaviorName,
						behavior.get_gameObject().get_name(),
						taskAddData.errorTaskName,
						taskAddData.errorTask
					}));
					break;
				}
				return;
			}
			this.dirty = true;
			if (behavior.ExternalBehavior != null)
			{
				behavior.GetBehaviorSource().EntryTask = behavior.ExternalBehavior.BehaviorSource.EntryTask;
			}
			behavior.GetBehaviorSource().RootTask = behaviorTree.taskList.get_Item(0);
			if (behavior.ResetValuesOnRestart)
			{
				behavior.SaveResetValues();
			}
			Stack<int> stack = ObjectPool.Get<Stack<int>>();
			stack.Clear();
			behaviorTree.activeStack.Add(stack);
			behaviorTree.interruptionIndex.Add(-1);
			behaviorTree.nonInstantTaskStatus.Add(TaskStatus.Inactive);
			if (behaviorTree.behavior.LogTaskChanges)
			{
				for (int j = 0; j < behaviorTree.taskList.get_Count(); j++)
				{
					Debug.Log(string.Format("{0}: Task {1} ({2}, index {3}) {4}", new object[]
					{
						this.RoundedTime(),
						behaviorTree.taskList.get_Item(j).FriendlyName,
						behaviorTree.taskList.get_Item(j).GetType(),
						j,
						behaviorTree.taskList.get_Item(j).GetHashCode()
					}));
				}
			}
			for (int k = 0; k < behaviorTree.taskList.get_Count(); k++)
			{
				behaviorTree.taskList.get_Item(k).OnAwake();
			}
			this.behaviorTrees.Add(behaviorTree);
			this.behaviorTreeMap.Add(behavior, behaviorTree);
			if (this.onEnableBehavior != null)
			{
				this.onEnableBehavior();
			}
			if (!behaviorTree.taskList.get_Item(0).Disabled)
			{
				behaviorTree.behavior.OnBehaviorStarted();
				behavior.ExecutionStatus = TaskStatus.Running;
				this.PushTask(behaviorTree, 0, 0);
			}
		}

		private int AddToTaskList(BehaviorManager.BehaviorTree behaviorTree, Task task, ref bool hasExternalBehavior, BehaviorManager.TaskAddData data)
		{
			if (task == null)
			{
				return -3;
			}
			task.GameObject = behaviorTree.behavior.get_gameObject();
			task.Transform = behaviorTree.behavior.get_transform();
			task.Owner = behaviorTree.behavior;
			if (task is BehaviorReference)
			{
				BehaviorReference behaviorReference = task as BehaviorReference;
				if (behaviorReference == null)
				{
					return -2;
				}
				ExternalBehavior[] externalBehaviors;
				if ((externalBehaviors = behaviorReference.GetExternalBehaviors()) == null)
				{
					return -2;
				}
				BehaviorSource[] array = new BehaviorSource[externalBehaviors.Length];
				for (int i = 0; i < externalBehaviors.Length; i++)
				{
					if (externalBehaviors[i] == null)
					{
						data.errorTask = behaviorTree.taskList.get_Count();
						data.errorTaskName = (string.IsNullOrEmpty(task.FriendlyName) ? task.GetType().ToString() : task.FriendlyName);
						return -5;
					}
					array[i] = externalBehaviors[i].BehaviorSource;
					array[i].Owner = externalBehaviors[i];
				}
				if (array == null)
				{
					return -2;
				}
				ParentTask parentTask = data.parentTask;
				int parentIndex = data.parentIndex;
				int compositeParentIndex = data.compositeParentIndex;
				data.offset = task.NodeData.Offset;
				data.depth++;
				for (int j = 0; j < array.Length; j++)
				{
					BehaviorSource behaviorSource = ObjectPool.Get<BehaviorSource>();
					behaviorSource.Initialize(array[j].Owner);
					array[j].CheckForSerialization(true, behaviorSource);
					Task rootTask = behaviorSource.RootTask;
					if (rootTask == null)
					{
						ObjectPool.Return<BehaviorSource>(behaviorSource);
						return -2;
					}
					if (rootTask is ParentTask)
					{
						rootTask.NodeData.Collapsed = (task as BehaviorReference).collapsed;
					}
					rootTask.Disabled = task.Disabled;
					if (behaviorReference.variables != null)
					{
						for (int k = 0; k < behaviorReference.variables.Length; k++)
						{
							if (data.overrideFields == null)
							{
								data.overrideFields = ObjectPool.Get<Dictionary<string, BehaviorManager.TaskAddData.OverrideFieldValue>>();
								data.overrideFields.Clear();
							}
							if (!data.overrideFields.ContainsKey(behaviorReference.variables[k].Value.name))
							{
								BehaviorManager.TaskAddData.OverrideFieldValue overrideFieldValue = ObjectPool.Get<BehaviorManager.TaskAddData.OverrideFieldValue>();
								overrideFieldValue.Initialize(behaviorReference.variables[k].Value, data.depth);
								if (behaviorReference.variables[k].Value is NamedVariable)
								{
									NamedVariable value = behaviorReference.variables[k].Value;
									if (string.IsNullOrEmpty(value.name))
									{
										Debug.LogWarning(string.Concat(new object[]
										{
											"Warning: Named variable on reference task ",
											behaviorReference.FriendlyName,
											" (id ",
											behaviorReference.ID,
											") is null"
										}));
										goto IL_39D;
									}
									BehaviorManager.TaskAddData.OverrideFieldValue overrideFieldValue2;
									if (value.value != null && data.overrideFields.TryGetValue(value.name, ref overrideFieldValue2))
									{
										overrideFieldValue = overrideFieldValue2;
									}
								}
								else if (behaviorReference.variables[k].Value is GenericVariable)
								{
									GenericVariable value2 = behaviorReference.variables[k].Value;
									if (value2.value != null)
									{
										if (string.IsNullOrEmpty(value2.value.Name))
										{
											Debug.LogWarning(string.Concat(new object[]
											{
												"Warning: Named variable on reference task ",
												behaviorReference.FriendlyName,
												" (id ",
												behaviorReference.ID,
												") is null"
											}));
											goto IL_39D;
										}
										BehaviorManager.TaskAddData.OverrideFieldValue overrideFieldValue3;
										if (data.overrideFields.TryGetValue(value2.value.Name, ref overrideFieldValue3))
										{
											overrideFieldValue = overrideFieldValue3;
										}
									}
								}
								data.overrideFields.Add(behaviorReference.variables[k].Value.name, overrideFieldValue);
							}
							IL_39D:;
						}
					}
					if (behaviorSource.Variables != null)
					{
						for (int l = 0; l < behaviorSource.Variables.get_Count(); l++)
						{
							SharedVariable sharedVariable;
							if ((sharedVariable = behaviorTree.behavior.GetVariable(behaviorSource.Variables.get_Item(l).Name)) == null)
							{
								sharedVariable = behaviorSource.Variables.get_Item(l);
								behaviorTree.behavior.SetVariable(sharedVariable.Name, sharedVariable);
							}
							else
							{
								behaviorSource.Variables.get_Item(l).SetValue(sharedVariable.GetValue());
							}
							if (data.overrideFields == null)
							{
								data.overrideFields = ObjectPool.Get<Dictionary<string, BehaviorManager.TaskAddData.OverrideFieldValue>>();
								data.overrideFields.Clear();
							}
							if (!data.overrideFields.ContainsKey(sharedVariable.Name))
							{
								BehaviorManager.TaskAddData.OverrideFieldValue overrideFieldValue4 = ObjectPool.Get<BehaviorManager.TaskAddData.OverrideFieldValue>();
								overrideFieldValue4.Initialize(sharedVariable, data.depth);
								data.overrideFields.Add(sharedVariable.Name, overrideFieldValue4);
							}
						}
					}
					ObjectPool.Return<BehaviorSource>(behaviorSource);
					if (j > 0)
					{
						data.parentTask = parentTask;
						data.parentIndex = parentIndex;
						data.compositeParentIndex = compositeParentIndex;
						if (data.parentTask == null || j >= data.parentTask.MaxChildren())
						{
							return -4;
						}
						behaviorTree.parentIndex.Add(data.parentIndex);
						behaviorTree.relativeChildIndex.Add(data.parentTask.Children.get_Count());
						behaviorTree.parentCompositeIndex.Add(data.compositeParentIndex);
						behaviorTree.childrenIndex.get_Item(data.parentIndex).Add(behaviorTree.taskList.get_Count());
						data.parentTask.AddChild(rootTask, data.parentTask.Children.get_Count());
					}
					hasExternalBehavior = true;
					bool fromExternalTask = data.fromExternalTask;
					data.fromExternalTask = true;
					int result;
					if ((result = this.AddToTaskList(behaviorTree, rootTask, ref hasExternalBehavior, data)) < 0)
					{
						return result;
					}
					data.fromExternalTask = fromExternalTask;
				}
				if (data.overrideFields != null)
				{
					Dictionary<string, BehaviorManager.TaskAddData.OverrideFieldValue> dictionary = ObjectPool.Get<Dictionary<string, BehaviorManager.TaskAddData.OverrideFieldValue>>();
					dictionary.Clear();
					using (Dictionary<string, BehaviorManager.TaskAddData.OverrideFieldValue>.Enumerator enumerator = data.overrideFields.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							KeyValuePair<string, BehaviorManager.TaskAddData.OverrideFieldValue> current = enumerator.get_Current();
							if (current.get_Value().Depth != data.depth)
							{
								dictionary.Add(current.get_Key(), current.get_Value());
							}
						}
					}
					ObjectPool.Return<Dictionary<string, BehaviorManager.TaskAddData.OverrideFieldValue>>(data.overrideFields);
					data.overrideFields = dictionary;
				}
				data.depth--;
			}
			else
			{
				if (behaviorTree.taskList.get_Count() == 0 && task.Disabled)
				{
					return -6;
				}
				task.ReferenceID = behaviorTree.taskList.get_Count();
				behaviorTree.taskList.Add(task);
				if (data.overrideFields != null)
				{
					this.OverrideFields(behaviorTree, data, task);
				}
				if (data.fromExternalTask)
				{
					if (data.parentTask == null)
					{
						task.NodeData.Offset = behaviorTree.behavior.GetBehaviorSource().RootTask.NodeData.Offset;
					}
					else
					{
						int index = behaviorTree.relativeChildIndex.get_Item(behaviorTree.relativeChildIndex.get_Count() - 1);
						data.parentTask.ReplaceAddChild(task, index);
						if (data.offset != Vector2.get_zero())
						{
							task.NodeData.Offset = data.offset;
							data.offset = Vector2.get_zero();
						}
					}
				}
				if (task is ParentTask)
				{
					ParentTask parentTask2 = task as ParentTask;
					if (parentTask2.Children == null || parentTask2.Children.get_Count() == 0)
					{
						data.errorTask = behaviorTree.taskList.get_Count() - 1;
						data.errorTaskName = (string.IsNullOrEmpty(behaviorTree.taskList.get_Item(data.errorTask).FriendlyName) ? behaviorTree.taskList.get_Item(data.errorTask).GetType().ToString() : behaviorTree.taskList.get_Item(data.errorTask).FriendlyName);
						return -1;
					}
					int num = behaviorTree.taskList.get_Count() - 1;
					List<int> list = ObjectPool.Get<List<int>>();
					list.Clear();
					behaviorTree.childrenIndex.Add(list);
					list = ObjectPool.Get<List<int>>();
					list.Clear();
					behaviorTree.childConditionalIndex.Add(list);
					int count = parentTask2.Children.get_Count();
					for (int m = 0; m < count; m++)
					{
						behaviorTree.parentIndex.Add(num);
						behaviorTree.relativeChildIndex.Add(m);
						behaviorTree.childrenIndex.get_Item(num).Add(behaviorTree.taskList.get_Count());
						data.parentTask = (task as ParentTask);
						data.parentIndex = num;
						if (task is Composite)
						{
							data.compositeParentIndex = num;
						}
						behaviorTree.parentCompositeIndex.Add(data.compositeParentIndex);
						int num2;
						if ((num2 = this.AddToTaskList(behaviorTree, parentTask2.Children.get_Item(m), ref hasExternalBehavior, data)) < 0)
						{
							if (num2 == -3)
							{
								data.errorTask = num;
								data.errorTaskName = (string.IsNullOrEmpty(behaviorTree.taskList.get_Item(data.errorTask).FriendlyName) ? behaviorTree.taskList.get_Item(data.errorTask).GetType().ToString() : behaviorTree.taskList.get_Item(data.errorTask).FriendlyName);
							}
							return num2;
						}
					}
				}
				else
				{
					behaviorTree.childrenIndex.Add(null);
					behaviorTree.childConditionalIndex.Add(null);
					if (task is Conditional)
					{
						int num3 = behaviorTree.taskList.get_Count() - 1;
						int num4 = behaviorTree.parentCompositeIndex.get_Item(num3);
						if (num4 != -1)
						{
							behaviorTree.childConditionalIndex.get_Item(num4).Add(num3);
						}
					}
				}
			}
			return 0;
		}

		private void OverrideFields(BehaviorManager.BehaviorTree behaviorTree, BehaviorManager.TaskAddData data, object obj)
		{
			if (obj == null || object.Equals(obj, null))
			{
				return;
			}
			FieldInfo[] allFields = TaskUtility.GetAllFields(obj.GetType());
			for (int i = 0; i < allFields.Length; i++)
			{
				object value = allFields[i].GetValue(obj);
				if (value != null)
				{
					if (typeof(SharedVariable).IsAssignableFrom(allFields[i].get_FieldType()))
					{
						SharedVariable sharedVariable = this.OverrideSharedVariable(behaviorTree, data, allFields[i].get_FieldType(), value as SharedVariable);
						if (sharedVariable != null)
						{
							allFields[i].SetValue(obj, sharedVariable);
						}
					}
					else if (typeof(IList).IsAssignableFrom(allFields[i].get_FieldType()))
					{
						Type fieldType;
						if (typeof(SharedVariable).IsAssignableFrom(fieldType = allFields[i].get_FieldType().GetElementType()) || (allFields[i].get_FieldType().get_IsGenericType() && typeof(SharedVariable).IsAssignableFrom(fieldType = allFields[i].get_FieldType().GetGenericArguments()[0])))
						{
							IList<SharedVariable> list = value as IList<SharedVariable>;
							if (list != null)
							{
								for (int j = 0; j < list.get_Count(); j++)
								{
									SharedVariable sharedVariable2 = this.OverrideSharedVariable(behaviorTree, data, fieldType, list.get_Item(j));
									if (sharedVariable2 != null)
									{
										list.set_Item(j, sharedVariable2);
									}
								}
							}
						}
					}
					else if (allFields[i].get_FieldType().get_IsClass() && !allFields[i].get_FieldType().Equals(typeof(Type)) && !typeof(Delegate).IsAssignableFrom(allFields[i].get_FieldType()) && !data.overiddenFields.Contains(value))
					{
						data.overiddenFields.Add(value);
						this.OverrideFields(behaviorTree, data, value);
						data.overiddenFields.Remove(value);
					}
				}
			}
		}

		private SharedVariable OverrideSharedVariable(BehaviorManager.BehaviorTree behaviorTree, BehaviorManager.TaskAddData data, Type fieldType, SharedVariable sharedVariable)
		{
			SharedVariable sharedVariable2 = sharedVariable;
			if (sharedVariable is SharedGenericVariable)
			{
				sharedVariable = ((sharedVariable as SharedGenericVariable).GetValue() as GenericVariable).value;
			}
			else if (sharedVariable is SharedNamedVariable)
			{
				sharedVariable = ((sharedVariable as SharedNamedVariable).GetValue() as NamedVariable).value;
			}
			if (sharedVariable == null)
			{
				return null;
			}
			BehaviorManager.TaskAddData.OverrideFieldValue overrideFieldValue;
			if (!string.IsNullOrEmpty(sharedVariable.Name) && data.overrideFields.TryGetValue(sharedVariable.Name, ref overrideFieldValue))
			{
				SharedVariable sharedVariable3 = null;
				if (overrideFieldValue.Value is SharedVariable)
				{
					sharedVariable3 = (overrideFieldValue.Value as SharedVariable);
				}
				else if (overrideFieldValue.Value is NamedVariable)
				{
					sharedVariable3 = (overrideFieldValue.Value as NamedVariable).value;
					if (sharedVariable3.IsShared)
					{
						sharedVariable3 = behaviorTree.behavior.GetVariable(sharedVariable3.Name);
					}
				}
				else if (overrideFieldValue.Value is GenericVariable)
				{
					sharedVariable3 = (overrideFieldValue.Value as GenericVariable).value;
					if (sharedVariable3.IsShared)
					{
						sharedVariable3 = behaviorTree.behavior.GetVariable(sharedVariable3.Name);
					}
				}
				if (sharedVariable2 is SharedNamedVariable || sharedVariable2 is SharedGenericVariable)
				{
					if (fieldType.Equals(typeof(SharedVariable)) || sharedVariable3.GetType().Equals(sharedVariable.GetType()))
					{
						if (sharedVariable2 is SharedNamedVariable)
						{
							(sharedVariable2 as SharedNamedVariable).Value.value = sharedVariable3;
						}
						else if (sharedVariable2 is SharedGenericVariable)
						{
							(sharedVariable2 as SharedGenericVariable).Value.value = sharedVariable3;
						}
						behaviorTree.behavior.SetVariableValue(sharedVariable.Name, sharedVariable3.GetValue());
					}
				}
				else if (sharedVariable3 is SharedVariable)
				{
					return sharedVariable3;
				}
			}
			return null;
		}

		public void DisableBehavior(Behavior behavior)
		{
			this.DisableBehavior(behavior, false);
		}

		public void DisableBehavior(Behavior behavior, bool paused)
		{
			this.DisableBehavior(behavior, paused, TaskStatus.Success);
		}

		public void DisableBehavior(Behavior behavior, bool paused, TaskStatus executionStatus)
		{
			if (!this.IsBehaviorEnabled(behavior))
			{
				if (!this.pausedBehaviorTrees.ContainsKey(behavior) || paused)
				{
					return;
				}
				this.EnableBehavior(behavior);
			}
			if (behavior.LogTaskChanges)
			{
				Debug.Log(string.Format("{0}: {1} {2}", this.RoundedTime(), (!paused) ? "Disabling" : "Pausing", behavior.ToString()));
			}
			if (paused)
			{
				BehaviorManager.BehaviorTree behaviorTree;
				if (!this.behaviorTreeMap.TryGetValue(behavior, ref behaviorTree))
				{
					return;
				}
				if (!this.pausedBehaviorTrees.ContainsKey(behavior))
				{
					this.pausedBehaviorTrees.Add(behavior, behaviorTree);
					behavior.ExecutionStatus = TaskStatus.Inactive;
					for (int i = 0; i < behaviorTree.taskList.get_Count(); i++)
					{
						behaviorTree.taskList.get_Item(i).OnPause(true);
					}
					this.behaviorTrees.Remove(behaviorTree);
				}
			}
			else
			{
				this.DestroyBehavior(behavior, executionStatus);
			}
		}

		public void DestroyBehavior(Behavior behavior)
		{
			this.DestroyBehavior(behavior, TaskStatus.Success);
		}

		public void DestroyBehavior(Behavior behavior, TaskStatus executionStatus)
		{
			BehaviorManager.BehaviorTree behaviorTree;
			if (!this.behaviorTreeMap.TryGetValue(behavior, ref behaviorTree) || behaviorTree.destroyBehavior)
			{
				return;
			}
			behaviorTree.destroyBehavior = true;
			if (this.pausedBehaviorTrees.ContainsKey(behavior))
			{
				this.pausedBehaviorTrees.Remove(behavior);
				for (int i = 0; i < behaviorTree.taskList.get_Count(); i++)
				{
					behaviorTree.taskList.get_Item(i).OnPause(false);
				}
				behavior.ExecutionStatus = TaskStatus.Running;
			}
			TaskStatus executionStatus2 = executionStatus;
			for (int j = behaviorTree.activeStack.get_Count() - 1; j > -1; j--)
			{
				while (behaviorTree.activeStack.get_Item(j).get_Count() > 0)
				{
					int count = behaviorTree.activeStack.get_Item(j).get_Count();
					this.PopTask(behaviorTree, behaviorTree.activeStack.get_Item(j).Peek(), j, ref executionStatus2, true, false);
					if (count == 1)
					{
						break;
					}
				}
			}
			this.RemoveChildConditionalReevaluate(behaviorTree, -1);
			for (int k = 0; k < behaviorTree.taskList.get_Count(); k++)
			{
				behaviorTree.taskList.get_Item(k).OnBehaviorComplete();
			}
			this.behaviorTreeMap.Remove(behavior);
			this.behaviorTrees.Remove(behaviorTree);
			behaviorTree.destroyBehavior = false;
			ObjectPool.Return<BehaviorManager.BehaviorTree>(behaviorTree);
			behavior.ExecutionStatus = executionStatus2;
			behavior.OnBehaviorEnded();
		}

		public void RestartBehavior(Behavior behavior)
		{
			if (!this.IsBehaviorEnabled(behavior))
			{
				return;
			}
			BehaviorManager.BehaviorTree behaviorTree = this.behaviorTreeMap.get_Item(behavior);
			TaskStatus taskStatus = TaskStatus.Success;
			for (int i = behaviorTree.activeStack.get_Count() - 1; i > -1; i--)
			{
				while (behaviorTree.activeStack.get_Item(i).get_Count() > 0)
				{
					int count = behaviorTree.activeStack.get_Item(i).get_Count();
					this.PopTask(behaviorTree, behaviorTree.activeStack.get_Item(i).Peek(), i, ref taskStatus, true, false);
					if (count == 1)
					{
						break;
					}
				}
			}
			this.Restart(behaviorTree);
		}

		public bool IsBehaviorEnabled(Behavior behavior)
		{
			return this.behaviorTreeMap != null && this.behaviorTreeMap.get_Count() > 0 && behavior != null && behavior.ExecutionStatus == TaskStatus.Running;
		}

		public void Update()
		{
			this.Tick();
		}

		public void LateUpdate()
		{
			for (int i = 0; i < this.behaviorTrees.get_Count(); i++)
			{
				if (this.behaviorTrees.get_Item(i).behavior.HasEvent[9])
				{
					for (int j = this.behaviorTrees.get_Item(i).activeStack.get_Count() - 1; j > -1; j--)
					{
						int num = this.behaviorTrees.get_Item(i).activeStack.get_Item(j).Peek();
						this.behaviorTrees.get_Item(i).taskList.get_Item(num).OnLateUpdate();
					}
				}
			}
		}

		public void FixedUpdate()
		{
			for (int i = 0; i < this.behaviorTrees.get_Count(); i++)
			{
				if (this.behaviorTrees.get_Item(i).behavior.HasEvent[10])
				{
					for (int j = this.behaviorTrees.get_Item(i).activeStack.get_Count() - 1; j > -1; j--)
					{
						int num = this.behaviorTrees.get_Item(i).activeStack.get_Item(j).Peek();
						this.behaviorTrees.get_Item(i).taskList.get_Item(num).OnFixedUpdate();
					}
				}
			}
		}

		[DebuggerHidden]
		private IEnumerator CoroutineUpdate()
		{
			while (true)
			{
				this.Tick();
				yield return this.updateWait;
			}
			yield break;
		}

		public void Tick()
		{
			for (int i = 0; i < this.behaviorTrees.get_Count(); i++)
			{
				this.Tick(this.behaviorTrees.get_Item(i));
			}
		}

		public void Tick(Behavior behavior)
		{
			if (behavior == null || !this.IsBehaviorEnabled(behavior))
			{
				return;
			}
			this.Tick(this.behaviorTreeMap.get_Item(behavior));
		}

		private void Tick(BehaviorManager.BehaviorTree behaviorTree)
		{
			behaviorTree.executionCount = 0;
			this.ReevaluateParentTasks(behaviorTree);
			this.ReevaluateConditionalTasks(behaviorTree);
			for (int i = behaviorTree.activeStack.get_Count() - 1; i > -1; i--)
			{
				TaskStatus taskStatus = TaskStatus.Inactive;
				int num;
				if (i < behaviorTree.interruptionIndex.get_Count() && (num = behaviorTree.interruptionIndex.get_Item(i)) != -1)
				{
					behaviorTree.interruptionIndex.set_Item(i, -1);
					while (behaviorTree.activeStack.get_Item(i).Peek() != num)
					{
						int count = behaviorTree.activeStack.get_Item(i).get_Count();
						this.PopTask(behaviorTree, behaviorTree.activeStack.get_Item(i).Peek(), i, ref taskStatus, true);
						if (count == 1)
						{
							break;
						}
					}
					if (i < behaviorTree.activeStack.get_Count() && behaviorTree.activeStack.get_Item(i).get_Count() > 0 && behaviorTree.taskList.get_Item(num) == behaviorTree.taskList.get_Item(behaviorTree.activeStack.get_Item(i).Peek()))
					{
						if (behaviorTree.taskList.get_Item(num) is ParentTask)
						{
							taskStatus = (behaviorTree.taskList.get_Item(num) as ParentTask).OverrideStatus();
						}
						this.PopTask(behaviorTree, num, i, ref taskStatus, true);
					}
				}
				int num2 = -1;
				while (taskStatus != TaskStatus.Running && i < behaviorTree.activeStack.get_Count() && behaviorTree.activeStack.get_Item(i).get_Count() > 0)
				{
					int num3 = behaviorTree.activeStack.get_Item(i).Peek();
					if ((i < behaviorTree.activeStack.get_Count() && behaviorTree.activeStack.get_Item(i).get_Count() > 0 && num2 == behaviorTree.activeStack.get_Item(i).Peek()) || !this.IsBehaviorEnabled(behaviorTree.behavior))
					{
						break;
					}
					num2 = num3;
					taskStatus = this.RunTask(behaviorTree, num3, i, taskStatus);
				}
			}
		}

		private void ReevaluateConditionalTasks(BehaviorManager.BehaviorTree behaviorTree)
		{
			for (int i = 0; i < behaviorTree.conditionalReevaluate.get_Count(); i++)
			{
				if (behaviorTree.conditionalReevaluate.get_Item(i).compositeIndex != -1)
				{
					int index = behaviorTree.conditionalReevaluate.get_Item(i).index;
					TaskStatus taskStatus = behaviorTree.taskList.get_Item(index).OnUpdate();
					if (taskStatus != behaviorTree.conditionalReevaluate.get_Item(i).taskStatus)
					{
						if (behaviorTree.behavior.LogTaskChanges)
						{
							int num = behaviorTree.parentCompositeIndex.get_Item(index);
							MonoBehaviour.print(string.Format("{0}: {1}: Conditional abort with task {2} ({3}, index {4}) because of conditional task {5} ({6}, index {7}) with status {8}", new object[]
							{
								this.RoundedTime(),
								behaviorTree.behavior.ToString(),
								behaviorTree.taskList.get_Item(num).FriendlyName,
								behaviorTree.taskList.get_Item(num).GetType(),
								num,
								behaviorTree.taskList.get_Item(index).FriendlyName,
								behaviorTree.taskList.get_Item(index).GetType(),
								index,
								taskStatus
							}));
						}
						int compositeIndex = behaviorTree.conditionalReevaluate.get_Item(i).compositeIndex;
						for (int j = behaviorTree.activeStack.get_Count() - 1; j > -1; j--)
						{
							if (behaviorTree.activeStack.get_Item(j).get_Count() > 0)
							{
								int num2 = behaviorTree.activeStack.get_Item(j).Peek();
								int num3 = this.FindLCA(behaviorTree, index, num2);
								if (this.IsChild(behaviorTree, num3, compositeIndex))
								{
									int count = behaviorTree.activeStack.get_Count();
									while (num2 != -1 && num2 != num3 && behaviorTree.activeStack.get_Count() == count)
									{
										TaskStatus taskStatus2 = TaskStatus.Failure;
										behaviorTree.taskList.get_Item(num2).OnConditionalAbort();
										this.PopTask(behaviorTree, num2, j, ref taskStatus2, false);
										num2 = behaviorTree.parentIndex.get_Item(num2);
									}
								}
							}
						}
						for (int k = behaviorTree.conditionalReevaluate.get_Count() - 1; k > i - 1; k--)
						{
							BehaviorManager.BehaviorTree.ConditionalReevaluate conditionalReevaluate = behaviorTree.conditionalReevaluate.get_Item(k);
							if (this.FindLCA(behaviorTree, compositeIndex, conditionalReevaluate.index) == compositeIndex)
							{
								behaviorTree.taskList.get_Item(behaviorTree.conditionalReevaluate.get_Item(k).index).NodeData.IsReevaluating = false;
								ObjectPool.Return<BehaviorManager.BehaviorTree.ConditionalReevaluate>(behaviorTree.conditionalReevaluate.get_Item(k));
								behaviorTree.conditionalReevaluateMap.Remove(behaviorTree.conditionalReevaluate.get_Item(k).index);
								behaviorTree.conditionalReevaluate.RemoveAt(k);
							}
						}
						Composite composite = behaviorTree.taskList.get_Item(behaviorTree.parentCompositeIndex.get_Item(index)) as Composite;
						for (int l = i - 1; l > -1; l--)
						{
							BehaviorManager.BehaviorTree.ConditionalReevaluate conditionalReevaluate2 = behaviorTree.conditionalReevaluate.get_Item(l);
							if (composite.AbortType == AbortType.LowerPriority && behaviorTree.parentCompositeIndex.get_Item(conditionalReevaluate2.index) == behaviorTree.parentCompositeIndex.get_Item(index))
							{
								behaviorTree.taskList.get_Item(behaviorTree.conditionalReevaluate.get_Item(l).index).NodeData.IsReevaluating = false;
								behaviorTree.conditionalReevaluate.get_Item(l).compositeIndex = -1;
							}
							else if (behaviorTree.parentCompositeIndex.get_Item(conditionalReevaluate2.index) == behaviorTree.parentCompositeIndex.get_Item(index))
							{
								for (int m = 0; m < behaviorTree.childrenIndex.get_Item(compositeIndex).get_Count(); m++)
								{
									if (this.IsParentTask(behaviorTree, behaviorTree.childrenIndex.get_Item(compositeIndex).get_Item(m), conditionalReevaluate2.index))
									{
										int num4 = behaviorTree.childrenIndex.get_Item(compositeIndex).get_Item(m);
										while (!(behaviorTree.taskList.get_Item(num4) is Composite))
										{
											if (behaviorTree.childrenIndex.get_Item(num4) == null)
											{
												break;
											}
											num4 = behaviorTree.childrenIndex.get_Item(num4).get_Item(0);
										}
										if (behaviorTree.taskList.get_Item(num4) is Composite)
										{
											conditionalReevaluate2.compositeIndex = num4;
										}
										break;
									}
								}
							}
						}
						this.conditionalParentIndexes.Clear();
						for (int num5 = behaviorTree.parentIndex.get_Item(index); num5 != compositeIndex; num5 = behaviorTree.parentIndex.get_Item(num5))
						{
							this.conditionalParentIndexes.Add(num5);
						}
						if (this.conditionalParentIndexes.get_Count() == 0)
						{
							this.conditionalParentIndexes.Add(behaviorTree.parentIndex.get_Item(index));
						}
						ParentTask parentTask = behaviorTree.taskList.get_Item(compositeIndex) as ParentTask;
						parentTask.OnConditionalAbort(behaviorTree.relativeChildIndex.get_Item(this.conditionalParentIndexes.get_Item(this.conditionalParentIndexes.get_Count() - 1)));
						for (int n = this.conditionalParentIndexes.get_Count() - 1; n > -1; n--)
						{
							parentTask = (behaviorTree.taskList.get_Item(this.conditionalParentIndexes.get_Item(n)) as ParentTask);
							if (n == 0)
							{
								parentTask.OnConditionalAbort(behaviorTree.relativeChildIndex.get_Item(index));
							}
							else
							{
								parentTask.OnConditionalAbort(behaviorTree.relativeChildIndex.get_Item(this.conditionalParentIndexes.get_Item(n - 1)));
							}
						}
						behaviorTree.taskList.get_Item(index).NodeData.InterruptTime = Time.get_realtimeSinceStartup();
					}
				}
			}
		}

		private void ReevaluateParentTasks(BehaviorManager.BehaviorTree behaviorTree)
		{
			for (int i = behaviorTree.parentReevaluate.get_Count() - 1; i > -1; i--)
			{
				int num = behaviorTree.parentReevaluate.get_Item(i);
				if (behaviorTree.taskList.get_Item(num) is Decorator)
				{
					if (behaviorTree.taskList.get_Item(num).OnUpdate() == TaskStatus.Failure)
					{
						this.Interrupt(behaviorTree.behavior, behaviorTree.taskList.get_Item(num));
					}
				}
				else if (behaviorTree.taskList.get_Item(num) is Composite)
				{
					ParentTask parentTask = behaviorTree.taskList.get_Item(num) as ParentTask;
					if (parentTask.OnReevaluationStarted())
					{
						int num2 = 0;
						TaskStatus status = this.RunParentTask(behaviorTree, num, ref num2, TaskStatus.Inactive);
						parentTask.OnReevaluationEnded(status);
					}
				}
			}
		}

		private TaskStatus RunTask(BehaviorManager.BehaviorTree behaviorTree, int taskIndex, int stackIndex, TaskStatus previousStatus)
		{
			Task task = behaviorTree.taskList.get_Item(taskIndex);
			if (task == null)
			{
				return previousStatus;
			}
			if (task.Disabled)
			{
				if (behaviorTree.behavior.LogTaskChanges)
				{
					MonoBehaviour.print(string.Format("{0}: {1}: Skip task {2} ({3}, index {4}) at stack index {5} (task disabled)", new object[]
					{
						this.RoundedTime(),
						behaviorTree.behavior.ToString(),
						behaviorTree.taskList.get_Item(taskIndex).FriendlyName,
						behaviorTree.taskList.get_Item(taskIndex).GetType(),
						taskIndex,
						stackIndex
					}));
				}
				if (behaviorTree.parentIndex.get_Item(taskIndex) != -1)
				{
					ParentTask parentTask = behaviorTree.taskList.get_Item(behaviorTree.parentIndex.get_Item(taskIndex)) as ParentTask;
					if (!parentTask.CanRunParallelChildren())
					{
						parentTask.OnChildExecuted(TaskStatus.Inactive);
					}
					else
					{
						parentTask.OnChildExecuted(behaviorTree.relativeChildIndex.get_Item(taskIndex), TaskStatus.Inactive);
						this.RemoveStack(behaviorTree, stackIndex);
					}
				}
				return previousStatus;
			}
			TaskStatus taskStatus = previousStatus;
			if (!task.IsInstant && (behaviorTree.nonInstantTaskStatus.get_Item(stackIndex) == TaskStatus.Failure || behaviorTree.nonInstantTaskStatus.get_Item(stackIndex) == TaskStatus.Success))
			{
				taskStatus = behaviorTree.nonInstantTaskStatus.get_Item(stackIndex);
				this.PopTask(behaviorTree, taskIndex, stackIndex, ref taskStatus, true);
				return taskStatus;
			}
			this.PushTask(behaviorTree, taskIndex, stackIndex);
			if (this.breakpointTree != null)
			{
				return TaskStatus.Running;
			}
			if (task is ParentTask)
			{
				ParentTask parentTask2 = task as ParentTask;
				taskStatus = this.RunParentTask(behaviorTree, taskIndex, ref stackIndex, taskStatus);
				taskStatus = parentTask2.OverrideStatus(taskStatus);
			}
			else
			{
				taskStatus = task.OnUpdate();
			}
			if (taskStatus != TaskStatus.Running)
			{
				if (task.IsInstant)
				{
					this.PopTask(behaviorTree, taskIndex, stackIndex, ref taskStatus, true);
				}
				else
				{
					behaviorTree.nonInstantTaskStatus.set_Item(stackIndex, taskStatus);
				}
			}
			return taskStatus;
		}

		private TaskStatus RunParentTask(BehaviorManager.BehaviorTree behaviorTree, int taskIndex, ref int stackIndex, TaskStatus status)
		{
			ParentTask parentTask = behaviorTree.taskList.get_Item(taskIndex) as ParentTask;
			if (!parentTask.CanRunParallelChildren() || parentTask.OverrideStatus(TaskStatus.Running) != TaskStatus.Running)
			{
				TaskStatus taskStatus = TaskStatus.Inactive;
				int num = stackIndex;
				int num2 = -1;
				Behavior behavior = behaviorTree.behavior;
				while (parentTask.CanExecute() && (taskStatus != TaskStatus.Running || parentTask.CanRunParallelChildren()) && this.IsBehaviorEnabled(behavior))
				{
					List<int> list = behaviorTree.childrenIndex.get_Item(taskIndex);
					int num3 = parentTask.CurrentChildIndex();
					if ((this.executionsPerTick == BehaviorManager.ExecutionsPerTickType.NoDuplicates && num3 == num2) || (this.executionsPerTick == BehaviorManager.ExecutionsPerTickType.Count && behaviorTree.executionCount >= this.maxTaskExecutionsPerTick))
					{
						if (this.executionsPerTick == BehaviorManager.ExecutionsPerTickType.Count)
						{
							Debug.LogWarning(string.Format("{0}: {1}: More than the specified number of task executions per tick ({2}) have executed, returning early.", this.RoundedTime(), behaviorTree.behavior.ToString(), this.maxTaskExecutionsPerTick));
						}
						status = TaskStatus.Running;
						break;
					}
					num2 = num3;
					if (parentTask.CanRunParallelChildren())
					{
						behaviorTree.activeStack.Add(ObjectPool.Get<Stack<int>>());
						behaviorTree.interruptionIndex.Add(-1);
						behaviorTree.nonInstantTaskStatus.Add(TaskStatus.Inactive);
						stackIndex = behaviorTree.activeStack.get_Count() - 1;
						parentTask.OnChildStarted(num3);
					}
					else
					{
						parentTask.OnChildStarted();
					}
					taskStatus = (status = this.RunTask(behaviorTree, list.get_Item(num3), stackIndex, status));
				}
				stackIndex = num;
			}
			return status;
		}

		private void PushTask(BehaviorManager.BehaviorTree behaviorTree, int taskIndex, int stackIndex)
		{
			if (!this.IsBehaviorEnabled(behaviorTree.behavior) || stackIndex >= behaviorTree.activeStack.get_Count())
			{
				return;
			}
			Stack<int> stack = behaviorTree.activeStack.get_Item(stackIndex);
			if (stack.get_Count() == 0 || stack.Peek() != taskIndex)
			{
				stack.Push(taskIndex);
				behaviorTree.nonInstantTaskStatus.set_Item(stackIndex, TaskStatus.Running);
				behaviorTree.executionCount++;
				Task task = behaviorTree.taskList.get_Item(taskIndex);
				task.NodeData.PushTime = Time.get_realtimeSinceStartup();
				task.NodeData.ExecutionStatus = TaskStatus.Running;
				if (task.NodeData.IsBreakpoint && this.onTaskBreakpoint != null)
				{
					this.breakpointTree = behaviorTree.behavior;
					this.onTaskBreakpoint();
				}
				if (behaviorTree.behavior.LogTaskChanges)
				{
					MonoBehaviour.print(string.Format("{0}: {1}: Push task {2} ({3}, index {4}) at stack index {5}", new object[]
					{
						this.RoundedTime(),
						behaviorTree.behavior.ToString(),
						task.FriendlyName,
						task.GetType(),
						taskIndex,
						stackIndex
					}));
				}
				task.OnStart();
				if (task is ParentTask)
				{
					ParentTask parentTask = task as ParentTask;
					if (parentTask.CanReevaluate())
					{
						behaviorTree.parentReevaluate.Add(taskIndex);
					}
				}
			}
		}

		private void PopTask(BehaviorManager.BehaviorTree behaviorTree, int taskIndex, int stackIndex, ref TaskStatus status, bool popChildren)
		{
			this.PopTask(behaviorTree, taskIndex, stackIndex, ref status, popChildren, true);
		}

		private void PopTask(BehaviorManager.BehaviorTree behaviorTree, int taskIndex, int stackIndex, ref TaskStatus status, bool popChildren, bool notifyOnEmptyStack)
		{
			if (!this.IsBehaviorEnabled(behaviorTree.behavior) || stackIndex >= behaviorTree.activeStack.get_Count() || behaviorTree.activeStack.get_Item(stackIndex).get_Count() == 0 || taskIndex != behaviorTree.activeStack.get_Item(stackIndex).Peek())
			{
				return;
			}
			behaviorTree.activeStack.get_Item(stackIndex).Pop();
			behaviorTree.nonInstantTaskStatus.set_Item(stackIndex, TaskStatus.Inactive);
			this.StopThirdPartyTask(behaviorTree, taskIndex);
			Task task = behaviorTree.taskList.get_Item(taskIndex);
			task.OnEnd();
			int num = behaviorTree.parentIndex.get_Item(taskIndex);
			task.NodeData.PushTime = -1f;
			task.NodeData.PopTime = Time.get_realtimeSinceStartup();
			task.NodeData.ExecutionStatus = status;
			if (behaviorTree.behavior.LogTaskChanges)
			{
				MonoBehaviour.print(string.Format("{0}: {1}: Pop task {2} ({3}, index {4}) at stack index {5} with status {6}", new object[]
				{
					this.RoundedTime(),
					behaviorTree.behavior.ToString(),
					task.FriendlyName,
					task.GetType(),
					taskIndex,
					stackIndex,
					status
				}));
			}
			if (num != -1)
			{
				if (task is Conditional)
				{
					int num2 = behaviorTree.parentCompositeIndex.get_Item(taskIndex);
					if (num2 != -1)
					{
						Composite composite = behaviorTree.taskList.get_Item(num2) as Composite;
						if (composite.AbortType != AbortType.None)
						{
							BehaviorManager.BehaviorTree.ConditionalReevaluate conditionalReevaluate;
							if (behaviorTree.conditionalReevaluateMap.TryGetValue(taskIndex, ref conditionalReevaluate))
							{
								conditionalReevaluate.compositeIndex = ((composite.AbortType == AbortType.LowerPriority) ? -1 : num2);
								conditionalReevaluate.taskStatus = status;
								task.NodeData.IsReevaluating = (composite.AbortType != AbortType.LowerPriority);
							}
							else
							{
								BehaviorManager.BehaviorTree.ConditionalReevaluate conditionalReevaluate2 = ObjectPool.Get<BehaviorManager.BehaviorTree.ConditionalReevaluate>();
								conditionalReevaluate2.Initialize(taskIndex, status, stackIndex, (composite.AbortType == AbortType.LowerPriority) ? -1 : num2);
								behaviorTree.conditionalReevaluate.Add(conditionalReevaluate2);
								behaviorTree.conditionalReevaluateMap.Add(taskIndex, conditionalReevaluate2);
								task.NodeData.IsReevaluating = (composite.AbortType == AbortType.Self || composite.AbortType == AbortType.Both);
							}
						}
					}
				}
				ParentTask parentTask = behaviorTree.taskList.get_Item(num) as ParentTask;
				if (!parentTask.CanRunParallelChildren())
				{
					parentTask.OnChildExecuted(status);
					status = parentTask.Decorate(status);
				}
				else
				{
					parentTask.OnChildExecuted(behaviorTree.relativeChildIndex.get_Item(taskIndex), status);
				}
			}
			if (task is ParentTask)
			{
				ParentTask parentTask2 = task as ParentTask;
				if (parentTask2.CanReevaluate())
				{
					for (int i = behaviorTree.parentReevaluate.get_Count() - 1; i > -1; i--)
					{
						if (behaviorTree.parentReevaluate.get_Item(i) == taskIndex)
						{
							behaviorTree.parentReevaluate.RemoveAt(i);
							break;
						}
					}
				}
				if (parentTask2 is Composite)
				{
					Composite composite2 = parentTask2 as Composite;
					if (composite2.AbortType == AbortType.Self || composite2.AbortType == AbortType.None || behaviorTree.activeStack.get_Item(stackIndex).get_Count() == 0)
					{
						this.RemoveChildConditionalReevaluate(behaviorTree, taskIndex);
					}
					else if (composite2.AbortType == AbortType.LowerPriority || composite2.AbortType == AbortType.Both)
					{
						int num3 = behaviorTree.parentCompositeIndex.get_Item(taskIndex);
						if (num3 != -1)
						{
							if (!(behaviorTree.taskList.get_Item(num3) as ParentTask).CanRunParallelChildren())
							{
								for (int j = 0; j < behaviorTree.childConditionalIndex.get_Item(taskIndex).get_Count(); j++)
								{
									int num4 = behaviorTree.childConditionalIndex.get_Item(taskIndex).get_Item(j);
									BehaviorManager.BehaviorTree.ConditionalReevaluate conditionalReevaluate3;
									if (behaviorTree.conditionalReevaluateMap.TryGetValue(num4, ref conditionalReevaluate3))
									{
										if (!(behaviorTree.taskList.get_Item(num3) as ParentTask).CanRunParallelChildren())
										{
											conditionalReevaluate3.compositeIndex = behaviorTree.parentCompositeIndex.get_Item(taskIndex);
											behaviorTree.taskList.get_Item(num4).NodeData.IsReevaluating = true;
										}
										else
										{
											for (int k = behaviorTree.conditionalReevaluate.get_Count() - 1; k > j - 1; k--)
											{
												BehaviorManager.BehaviorTree.ConditionalReevaluate conditionalReevaluate4 = behaviorTree.conditionalReevaluate.get_Item(k);
												if (this.FindLCA(behaviorTree, num3, conditionalReevaluate4.index) == num3)
												{
													behaviorTree.taskList.get_Item(behaviorTree.conditionalReevaluate.get_Item(k).index).NodeData.IsReevaluating = false;
													ObjectPool.Return<BehaviorManager.BehaviorTree.ConditionalReevaluate>(behaviorTree.conditionalReevaluate.get_Item(k));
													behaviorTree.conditionalReevaluateMap.Remove(behaviorTree.conditionalReevaluate.get_Item(k).index);
													behaviorTree.conditionalReevaluate.RemoveAt(k);
												}
											}
										}
									}
								}
							}
							else
							{
								this.RemoveChildConditionalReevaluate(behaviorTree, taskIndex);
							}
						}
						for (int l = 0; l < behaviorTree.conditionalReevaluate.get_Count(); l++)
						{
							if (behaviorTree.conditionalReevaluate.get_Item(l).compositeIndex == taskIndex)
							{
								behaviorTree.conditionalReevaluate.get_Item(l).compositeIndex = behaviorTree.parentCompositeIndex.get_Item(taskIndex);
							}
						}
					}
				}
			}
			if (popChildren)
			{
				for (int m = behaviorTree.activeStack.get_Count() - 1; m > stackIndex; m--)
				{
					if (behaviorTree.activeStack.get_Item(m).get_Count() > 0 && this.IsParentTask(behaviorTree, taskIndex, behaviorTree.activeStack.get_Item(m).Peek()))
					{
						TaskStatus taskStatus = TaskStatus.Failure;
						for (int n = behaviorTree.activeStack.get_Item(m).get_Count(); n > 0; n--)
						{
							this.PopTask(behaviorTree, behaviorTree.activeStack.get_Item(m).Peek(), m, ref taskStatus, false, notifyOnEmptyStack);
						}
					}
				}
			}
			if (stackIndex < behaviorTree.activeStack.get_Count() && behaviorTree.activeStack.get_Item(stackIndex).get_Count() == 0)
			{
				if (stackIndex == 0)
				{
					if (notifyOnEmptyStack)
					{
						if (behaviorTree.behavior.RestartWhenComplete)
						{
							this.Restart(behaviorTree);
						}
						else
						{
							this.DisableBehavior(behaviorTree.behavior, false, status);
						}
					}
					status = TaskStatus.Inactive;
				}
				else
				{
					this.RemoveStack(behaviorTree, stackIndex);
					status = TaskStatus.Running;
				}
			}
		}

		private void RemoveChildConditionalReevaluate(BehaviorManager.BehaviorTree behaviorTree, int compositeIndex)
		{
			for (int i = behaviorTree.conditionalReevaluate.get_Count() - 1; i > -1; i--)
			{
				if (behaviorTree.conditionalReevaluate.get_Item(i).compositeIndex == compositeIndex)
				{
					ObjectPool.Return<BehaviorManager.BehaviorTree.ConditionalReevaluate>(behaviorTree.conditionalReevaluate.get_Item(i));
					int index = behaviorTree.conditionalReevaluate.get_Item(i).index;
					behaviorTree.conditionalReevaluateMap.Remove(index);
					behaviorTree.conditionalReevaluate.RemoveAt(i);
					behaviorTree.taskList.get_Item(index).NodeData.IsReevaluating = false;
				}
			}
		}

		private void Restart(BehaviorManager.BehaviorTree behaviorTree)
		{
			if (behaviorTree.behavior.LogTaskChanges)
			{
				Debug.Log(string.Format("{0}: Restarting {1}", this.RoundedTime(), behaviorTree.behavior.ToString()));
			}
			this.RemoveChildConditionalReevaluate(behaviorTree, -1);
			if (behaviorTree.behavior.ResetValuesOnRestart)
			{
				behaviorTree.behavior.SaveResetValues();
			}
			for (int i = 0; i < behaviorTree.taskList.get_Count(); i++)
			{
				behaviorTree.taskList.get_Item(i).OnBehaviorRestart();
			}
			behaviorTree.behavior.OnBehaviorRestarted();
			this.PushTask(behaviorTree, 0, 0);
		}

		private bool IsParentTask(BehaviorManager.BehaviorTree behaviorTree, int possibleParent, int possibleChild)
		{
			int num2;
			for (int num = possibleChild; num != -1; num = num2)
			{
				num2 = behaviorTree.parentIndex.get_Item(num);
				if (num2 == possibleParent)
				{
					return true;
				}
			}
			return false;
		}

		public void Interrupt(Behavior behavior, Task task)
		{
			this.Interrupt(behavior, task, task);
		}

		public void Interrupt(Behavior behavior, Task task, Task interruptionTask)
		{
			if (!this.IsBehaviorEnabled(behavior))
			{
				return;
			}
			int num = -1;
			BehaviorManager.BehaviorTree behaviorTree = this.behaviorTreeMap.get_Item(behavior);
			for (int i = 0; i < behaviorTree.taskList.get_Count(); i++)
			{
				if (behaviorTree.taskList.get_Item(i).ReferenceID == task.ReferenceID)
				{
					num = i;
					break;
				}
			}
			if (num > -1)
			{
				for (int j = 0; j < behaviorTree.activeStack.get_Count(); j++)
				{
					if (behaviorTree.activeStack.get_Item(j).get_Count() > 0)
					{
						for (int num2 = behaviorTree.activeStack.get_Item(j).Peek(); num2 != -1; num2 = behaviorTree.parentIndex.get_Item(num2))
						{
							if (num2 == num)
							{
								behaviorTree.interruptionIndex.set_Item(j, num);
								if (behavior.LogTaskChanges)
								{
									Debug.Log(string.Format("{0}: {1}: Interrupt task {2} ({3}) with index {4} at stack index {5}", new object[]
									{
										this.RoundedTime(),
										behaviorTree.behavior.ToString(),
										task.FriendlyName,
										task.GetType().ToString(),
										num,
										j
									}));
								}
								interruptionTask.NodeData.InterruptTime = Time.get_realtimeSinceStartup();
								break;
							}
						}
					}
				}
			}
		}

		public void StopThirdPartyTask(BehaviorManager.BehaviorTree behaviorTree, int taskIndex)
		{
			this.thirdPartyTaskCompare.Task = behaviorTree.taskList.get_Item(taskIndex);
			object obj;
			if (this.taskObjectMap.TryGetValue(this.thirdPartyTaskCompare, ref obj))
			{
				BehaviorManager.ThirdPartyObjectType thirdPartyObjectType = this.objectTaskMap.get_Item(obj).ThirdPartyObjectType;
				if (BehaviorManager.invokeParameters == null)
				{
					BehaviorManager.invokeParameters = new object[1];
				}
				BehaviorManager.invokeParameters[0] = behaviorTree.taskList.get_Item(taskIndex);
				switch (thirdPartyObjectType)
				{
				case BehaviorManager.ThirdPartyObjectType.PlayMaker:
					BehaviorManager.PlayMakerStopMethod.Invoke(null, BehaviorManager.invokeParameters);
					break;
				case BehaviorManager.ThirdPartyObjectType.uScript:
					BehaviorManager.UScriptStopMethod.Invoke(null, BehaviorManager.invokeParameters);
					break;
				case BehaviorManager.ThirdPartyObjectType.DialogueSystem:
					BehaviorManager.DialogueSystemStopMethod.Invoke(null, BehaviorManager.invokeParameters);
					break;
				case BehaviorManager.ThirdPartyObjectType.uSequencer:
					BehaviorManager.USequencerStopMethod.Invoke(null, BehaviorManager.invokeParameters);
					break;
				case BehaviorManager.ThirdPartyObjectType.ICode:
					BehaviorManager.ICodeStopMethod.Invoke(null, BehaviorManager.invokeParameters);
					break;
				}
				this.RemoveActiveThirdPartyTask(behaviorTree.taskList.get_Item(taskIndex));
			}
		}

		public void RemoveActiveThirdPartyTask(Task task)
		{
			this.thirdPartyTaskCompare.Task = task;
			object obj;
			if (this.taskObjectMap.TryGetValue(this.thirdPartyTaskCompare, ref obj))
			{
				ObjectPool.Return<object>(obj);
				this.taskObjectMap.Remove(this.thirdPartyTaskCompare);
				this.objectTaskMap.Remove(obj);
			}
		}

		private void RemoveStack(BehaviorManager.BehaviorTree behaviorTree, int stackIndex)
		{
			Stack<int> stack = behaviorTree.activeStack.get_Item(stackIndex);
			stack.Clear();
			ObjectPool.Return<Stack<int>>(stack);
			behaviorTree.activeStack.RemoveAt(stackIndex);
			behaviorTree.interruptionIndex.RemoveAt(stackIndex);
			behaviorTree.nonInstantTaskStatus.RemoveAt(stackIndex);
		}

		private int FindLCA(BehaviorManager.BehaviorTree behaviorTree, int taskIndex1, int taskIndex2)
		{
			HashSet<int> hashSet = ObjectPool.Get<HashSet<int>>();
			hashSet.Clear();
			int num;
			for (num = taskIndex1; num != -1; num = behaviorTree.parentIndex.get_Item(num))
			{
				hashSet.Add(num);
			}
			num = taskIndex2;
			while (!hashSet.Contains(num))
			{
				num = behaviorTree.parentIndex.get_Item(num);
			}
			ObjectPool.Return<HashSet<int>>(hashSet);
			return num;
		}

		private bool IsChild(BehaviorManager.BehaviorTree behaviorTree, int taskIndex1, int taskIndex2)
		{
			for (int num = taskIndex1; num != -1; num = behaviorTree.parentIndex.get_Item(num))
			{
				if (num == taskIndex2)
				{
					return true;
				}
			}
			return false;
		}

		public List<Task> GetActiveTasks(Behavior behavior)
		{
			if (!this.IsBehaviorEnabled(behavior))
			{
				return null;
			}
			List<Task> list = new List<Task>();
			BehaviorManager.BehaviorTree behaviorTree = this.behaviorTreeMap.get_Item(behavior);
			for (int i = 0; i < behaviorTree.activeStack.get_Count(); i++)
			{
				Task task = behaviorTree.taskList.get_Item(behaviorTree.activeStack.get_Item(i).Peek());
				if (task is Action)
				{
					list.Add(task);
				}
			}
			return list;
		}

		public void BehaviorOnCollisionEnter(Collision collision, Behavior behavior)
		{
			if (!this.IsBehaviorEnabled(behavior))
			{
				return;
			}
			BehaviorManager.BehaviorTree behaviorTree = this.behaviorTreeMap.get_Item(behavior);
			for (int i = 0; i < behaviorTree.activeStack.get_Count(); i++)
			{
				if (behaviorTree.activeStack.get_Item(i).get_Count() != 0)
				{
					for (int num = behaviorTree.activeStack.get_Item(i).Peek(); num != -1; num = behaviorTree.parentIndex.get_Item(num))
					{
						if (behaviorTree.taskList.get_Item(num).Disabled)
						{
							break;
						}
						behaviorTree.taskList.get_Item(num).OnCollisionEnter(collision);
					}
				}
			}
			for (int j = 0; j < behaviorTree.conditionalReevaluate.get_Count(); j++)
			{
				int num = behaviorTree.conditionalReevaluate.get_Item(j).index;
				if (!behaviorTree.taskList.get_Item(num).Disabled)
				{
					if (behaviorTree.conditionalReevaluate.get_Item(j).compositeIndex != -1)
					{
						behaviorTree.taskList.get_Item(num).OnCollisionEnter(collision);
					}
				}
			}
		}

		public void BehaviorOnCollisionExit(Collision collision, Behavior behavior)
		{
			if (!this.IsBehaviorEnabled(behavior))
			{
				return;
			}
			BehaviorManager.BehaviorTree behaviorTree = this.behaviorTreeMap.get_Item(behavior);
			for (int i = 0; i < behaviorTree.activeStack.get_Count(); i++)
			{
				if (behaviorTree.activeStack.get_Item(i).get_Count() != 0)
				{
					for (int num = behaviorTree.activeStack.get_Item(i).Peek(); num != -1; num = behaviorTree.parentIndex.get_Item(num))
					{
						if (behaviorTree.taskList.get_Item(num).Disabled)
						{
							break;
						}
						behaviorTree.taskList.get_Item(num).OnCollisionExit(collision);
					}
				}
			}
			for (int j = 0; j < behaviorTree.conditionalReevaluate.get_Count(); j++)
			{
				int num = behaviorTree.conditionalReevaluate.get_Item(j).index;
				if (!behaviorTree.taskList.get_Item(num).Disabled)
				{
					if (behaviorTree.conditionalReevaluate.get_Item(j).compositeIndex != -1)
					{
						behaviorTree.taskList.get_Item(num).OnCollisionExit(collision);
					}
				}
			}
		}

		public void BehaviorOnTriggerEnter(Collider other, Behavior behavior)
		{
			if (!this.IsBehaviorEnabled(behavior))
			{
				return;
			}
			BehaviorManager.BehaviorTree behaviorTree = this.behaviorTreeMap.get_Item(behavior);
			for (int i = 0; i < behaviorTree.activeStack.get_Count(); i++)
			{
				if (behaviorTree.activeStack.get_Item(i).get_Count() != 0)
				{
					for (int num = behaviorTree.activeStack.get_Item(i).Peek(); num != -1; num = behaviorTree.parentIndex.get_Item(num))
					{
						if (behaviorTree.taskList.get_Item(num).Disabled)
						{
							break;
						}
						behaviorTree.taskList.get_Item(num).OnTriggerEnter(other);
					}
				}
			}
			for (int j = 0; j < behaviorTree.conditionalReevaluate.get_Count(); j++)
			{
				int num = behaviorTree.conditionalReevaluate.get_Item(j).index;
				if (!behaviorTree.taskList.get_Item(num).Disabled)
				{
					if (behaviorTree.conditionalReevaluate.get_Item(j).compositeIndex != -1)
					{
						behaviorTree.taskList.get_Item(num).OnTriggerEnter(other);
					}
				}
			}
		}

		public void BehaviorOnTriggerExit(Collider other, Behavior behavior)
		{
			if (!this.IsBehaviorEnabled(behavior))
			{
				return;
			}
			BehaviorManager.BehaviorTree behaviorTree = this.behaviorTreeMap.get_Item(behavior);
			for (int i = 0; i < behaviorTree.activeStack.get_Count(); i++)
			{
				if (behaviorTree.activeStack.get_Item(i).get_Count() != 0)
				{
					for (int num = behaviorTree.activeStack.get_Item(i).Peek(); num != -1; num = behaviorTree.parentIndex.get_Item(num))
					{
						if (behaviorTree.taskList.get_Item(num).Disabled)
						{
							break;
						}
						behaviorTree.taskList.get_Item(num).OnTriggerExit(other);
					}
				}
			}
			for (int j = 0; j < behaviorTree.conditionalReevaluate.get_Count(); j++)
			{
				int num = behaviorTree.conditionalReevaluate.get_Item(j).index;
				if (!behaviorTree.taskList.get_Item(num).Disabled)
				{
					if (behaviorTree.conditionalReevaluate.get_Item(j).compositeIndex != -1)
					{
						behaviorTree.taskList.get_Item(num).OnTriggerExit(other);
					}
				}
			}
		}

		public void BehaviorOnCollisionEnter2D(Collision2D collision, Behavior behavior)
		{
			if (!this.IsBehaviorEnabled(behavior))
			{
				return;
			}
			BehaviorManager.BehaviorTree behaviorTree = this.behaviorTreeMap.get_Item(behavior);
			for (int i = 0; i < behaviorTree.activeStack.get_Count(); i++)
			{
				if (behaviorTree.activeStack.get_Item(i).get_Count() != 0)
				{
					for (int num = behaviorTree.activeStack.get_Item(i).Peek(); num != -1; num = behaviorTree.parentIndex.get_Item(num))
					{
						if (behaviorTree.taskList.get_Item(num).Disabled)
						{
							break;
						}
						behaviorTree.taskList.get_Item(num).OnCollisionEnter2D(collision);
					}
				}
			}
			for (int j = 0; j < behaviorTree.conditionalReevaluate.get_Count(); j++)
			{
				int num = behaviorTree.conditionalReevaluate.get_Item(j).index;
				if (!behaviorTree.taskList.get_Item(num).Disabled)
				{
					if (behaviorTree.conditionalReevaluate.get_Item(j).compositeIndex != -1)
					{
						behaviorTree.taskList.get_Item(num).OnCollisionEnter2D(collision);
					}
				}
			}
		}

		public void BehaviorOnCollisionExit2D(Collision2D collision, Behavior behavior)
		{
			if (!this.IsBehaviorEnabled(behavior))
			{
				return;
			}
			BehaviorManager.BehaviorTree behaviorTree = this.behaviorTreeMap.get_Item(behavior);
			for (int i = 0; i < behaviorTree.activeStack.get_Count(); i++)
			{
				if (behaviorTree.activeStack.get_Item(i).get_Count() != 0)
				{
					for (int num = behaviorTree.activeStack.get_Item(i).Peek(); num != -1; num = behaviorTree.parentIndex.get_Item(num))
					{
						if (behaviorTree.taskList.get_Item(num).Disabled)
						{
							break;
						}
						behaviorTree.taskList.get_Item(num).OnCollisionExit2D(collision);
					}
				}
			}
			for (int j = 0; j < behaviorTree.conditionalReevaluate.get_Count(); j++)
			{
				int num = behaviorTree.conditionalReevaluate.get_Item(j).index;
				if (!behaviorTree.taskList.get_Item(num).Disabled)
				{
					if (behaviorTree.conditionalReevaluate.get_Item(j).compositeIndex != -1)
					{
						behaviorTree.taskList.get_Item(num).OnCollisionExit2D(collision);
					}
				}
			}
		}

		public void BehaviorOnTriggerEnter2D(Collider2D other, Behavior behavior)
		{
			if (!this.IsBehaviorEnabled(behavior))
			{
				return;
			}
			BehaviorManager.BehaviorTree behaviorTree = this.behaviorTreeMap.get_Item(behavior);
			for (int i = 0; i < behaviorTree.activeStack.get_Count(); i++)
			{
				if (behaviorTree.activeStack.get_Item(i).get_Count() != 0)
				{
					for (int num = behaviorTree.activeStack.get_Item(i).Peek(); num != -1; num = behaviorTree.parentIndex.get_Item(num))
					{
						if (behaviorTree.taskList.get_Item(num).Disabled)
						{
							break;
						}
						behaviorTree.taskList.get_Item(num).OnTriggerEnter2D(other);
					}
				}
			}
			for (int j = 0; j < behaviorTree.conditionalReevaluate.get_Count(); j++)
			{
				int num = behaviorTree.conditionalReevaluate.get_Item(j).index;
				if (!behaviorTree.taskList.get_Item(num).Disabled)
				{
					if (behaviorTree.conditionalReevaluate.get_Item(j).compositeIndex != -1)
					{
						behaviorTree.taskList.get_Item(num).OnTriggerEnter2D(other);
					}
				}
			}
		}

		public void BehaviorOnTriggerExit2D(Collider2D other, Behavior behavior)
		{
			if (!this.IsBehaviorEnabled(behavior))
			{
				return;
			}
			BehaviorManager.BehaviorTree behaviorTree = this.behaviorTreeMap.get_Item(behavior);
			for (int i = 0; i < behaviorTree.activeStack.get_Count(); i++)
			{
				if (behaviorTree.activeStack.get_Item(i).get_Count() != 0)
				{
					for (int num = behaviorTree.activeStack.get_Item(i).Peek(); num != -1; num = behaviorTree.parentIndex.get_Item(num))
					{
						if (behaviorTree.taskList.get_Item(num).Disabled)
						{
							break;
						}
						behaviorTree.taskList.get_Item(num).OnTriggerExit2D(other);
					}
				}
			}
			for (int j = 0; j < behaviorTree.conditionalReevaluate.get_Count(); j++)
			{
				int num = behaviorTree.conditionalReevaluate.get_Item(j).index;
				if (!behaviorTree.taskList.get_Item(num).Disabled)
				{
					if (behaviorTree.conditionalReevaluate.get_Item(j).compositeIndex != -1)
					{
						behaviorTree.taskList.get_Item(num).OnTriggerExit2D(other);
					}
				}
			}
		}

		public void BehaviorOnControllerColliderHit(ControllerColliderHit hit, Behavior behavior)
		{
			if (!this.IsBehaviorEnabled(behavior))
			{
				return;
			}
			BehaviorManager.BehaviorTree behaviorTree = this.behaviorTreeMap.get_Item(behavior);
			for (int i = 0; i < behaviorTree.activeStack.get_Count(); i++)
			{
				if (behaviorTree.activeStack.get_Item(i).get_Count() != 0)
				{
					for (int num = behaviorTree.activeStack.get_Item(i).Peek(); num != -1; num = behaviorTree.parentIndex.get_Item(num))
					{
						if (behaviorTree.taskList.get_Item(num).Disabled)
						{
							break;
						}
						behaviorTree.taskList.get_Item(num).OnControllerColliderHit(hit);
					}
				}
			}
			for (int j = 0; j < behaviorTree.conditionalReevaluate.get_Count(); j++)
			{
				int num = behaviorTree.conditionalReevaluate.get_Item(j).index;
				if (!behaviorTree.taskList.get_Item(num).Disabled)
				{
					if (behaviorTree.conditionalReevaluate.get_Item(j).compositeIndex != -1)
					{
						behaviorTree.taskList.get_Item(num).OnControllerColliderHit(hit);
					}
				}
			}
		}

		public void BehaviorOnAnimatorIK(Behavior behavior)
		{
			if (!this.IsBehaviorEnabled(behavior))
			{
				return;
			}
			BehaviorManager.BehaviorTree behaviorTree = this.behaviorTreeMap.get_Item(behavior);
			for (int i = 0; i < behaviorTree.activeStack.get_Count(); i++)
			{
				if (behaviorTree.activeStack.get_Item(i).get_Count() != 0)
				{
					for (int num = behaviorTree.activeStack.get_Item(i).Peek(); num != -1; num = behaviorTree.parentIndex.get_Item(num))
					{
						if (behaviorTree.taskList.get_Item(num).Disabled)
						{
							break;
						}
						behaviorTree.taskList.get_Item(num).OnAnimatorIK();
					}
				}
			}
			for (int j = 0; j < behaviorTree.conditionalReevaluate.get_Count(); j++)
			{
				int num = behaviorTree.conditionalReevaluate.get_Item(j).index;
				if (!behaviorTree.taskList.get_Item(num).Disabled)
				{
					if (behaviorTree.conditionalReevaluate.get_Item(j).compositeIndex != -1)
					{
						behaviorTree.taskList.get_Item(num).OnAnimatorIK();
					}
				}
			}
		}

		public bool MapObjectToTask(object objectKey, Task task, BehaviorManager.ThirdPartyObjectType objectType)
		{
			if (this.objectTaskMap.ContainsKey(objectKey))
			{
				string text = string.Empty;
				switch (objectType)
				{
				case BehaviorManager.ThirdPartyObjectType.PlayMaker:
					text = "PlayMaker FSM";
					break;
				case BehaviorManager.ThirdPartyObjectType.uScript:
					text = "uScript Graph";
					break;
				case BehaviorManager.ThirdPartyObjectType.DialogueSystem:
					text = "Dialogue System";
					break;
				case BehaviorManager.ThirdPartyObjectType.uSequencer:
					text = "uSequencer sequence";
					break;
				case BehaviorManager.ThirdPartyObjectType.ICode:
					text = "ICode state machine";
					break;
				}
				Debug.LogError(string.Format("Only one behavior can be mapped to the same instance of the {0}.", text));
				return false;
			}
			BehaviorManager.ThirdPartyTask thirdPartyTask = ObjectPool.Get<BehaviorManager.ThirdPartyTask>();
			thirdPartyTask.Initialize(task, objectType);
			this.objectTaskMap.Add(objectKey, thirdPartyTask);
			this.taskObjectMap.Add(thirdPartyTask, objectKey);
			return true;
		}

		public Task TaskForObject(object objectKey)
		{
			BehaviorManager.ThirdPartyTask thirdPartyTask;
			if (!this.objectTaskMap.TryGetValue(objectKey, ref thirdPartyTask))
			{
				return null;
			}
			return thirdPartyTask.Task;
		}

		private decimal RoundedTime()
		{
			return Math.Round((decimal)Time.get_time(), 5, 1);
		}

		public List<Task> GetTaskList(Behavior behavior)
		{
			if (!this.IsBehaviorEnabled(behavior))
			{
				return null;
			}
			BehaviorManager.BehaviorTree behaviorTree = this.behaviorTreeMap.get_Item(behavior);
			return behaviorTree.taskList;
		}
	}
}