// Decompiled with JetBrains decompiler
// Type: BehaviorDesigner.Runtime.BehaviorManager
// Assembly: BehaviorDesignerRuntime, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 4E52B3C7-423D-4A3D-958C-02E4666F0F16
// Assembly location: C:\Users\Ron\Desktop\Runtime\BehaviorDesignerRuntime.dll

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
    public static BehaviorManager instance;
    [SerializeField]
    private UpdateIntervalType updateInterval;
    [SerializeField]
    private float updateIntervalSeconds;
    [SerializeField]
    private BehaviorManager.ExecutionsPerTickType executionsPerTick;
    [SerializeField]
    private int maxTaskExecutionsPerTick;
    private WaitForSeconds updateWait;
    public BehaviorManager.BehaviorManagerHandler onEnableBehavior;
    public BehaviorManager.BehaviorManagerHandler onTaskBreakpoint;
    private List<BehaviorManager.BehaviorTree> behaviorTrees;
    private Dictionary<Behavior, BehaviorManager.BehaviorTree> pausedBehaviorTrees;
    private Dictionary<Behavior, BehaviorManager.BehaviorTree> behaviorTreeMap;
    private List<int> conditionalParentIndexes;
    private Dictionary<object, BehaviorManager.ThirdPartyTask> objectTaskMap;
    private Dictionary<BehaviorManager.ThirdPartyTask, object> taskObjectMap;
    private BehaviorManager.ThirdPartyTask thirdPartyTaskCompare;
    private static MethodInfo playMakerStopMethod;
    private static MethodInfo uScriptStopMethod;
    private static MethodInfo dialogueSystemStopMethod;
    private static MethodInfo uSequencerStopMethod;
    private static MethodInfo iCodeStopMethod;
    private static object[] invokeParameters;
    private Behavior breakpointTree;
    private bool dirty;

    public BehaviorManager()
    {
      base.\u002Ector();
    }

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
        this.onTaskBreakpoint += value;
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
          BehaviorManager.playMakerStopMethod = TaskUtility.GetTypeWithinAssembly("BehaviorDesigner.Runtime.BehaviorManager_PlayMaker").GetMethod("StopPlayMaker");
        return BehaviorManager.playMakerStopMethod;
      }
    }

    private static MethodInfo UScriptStopMethod
    {
      get
      {
        if (BehaviorManager.uScriptStopMethod == null)
          BehaviorManager.uScriptStopMethod = TaskUtility.GetTypeWithinAssembly("BehaviorDesigner.Runtime.BehaviorManager_uScript").GetMethod("StopuScript");
        return BehaviorManager.uScriptStopMethod;
      }
    }

    private static MethodInfo DialogueSystemStopMethod
    {
      get
      {
        if (BehaviorManager.dialogueSystemStopMethod == null)
          BehaviorManager.dialogueSystemStopMethod = TaskUtility.GetTypeWithinAssembly("BehaviorDesigner.Runtime.BehaviorManager_DialogueSystem").GetMethod("StopDialogueSystem");
        return BehaviorManager.dialogueSystemStopMethod;
      }
    }

    private static MethodInfo USequencerStopMethod
    {
      get
      {
        if (BehaviorManager.uSequencerStopMethod == null)
          BehaviorManager.uSequencerStopMethod = TaskUtility.GetTypeWithinAssembly("BehaviorDesigner.Runtime.BehaviorManager_uSequencer").GetMethod("StopuSequencer");
        return BehaviorManager.uSequencerStopMethod;
      }
    }

    private static MethodInfo ICodeStopMethod
    {
      get
      {
        if (BehaviorManager.iCodeStopMethod == null)
          BehaviorManager.iCodeStopMethod = TaskUtility.GetTypeWithinAssembly("BehaviorDesigner.Runtime.BehaviorManager_ICode").GetMethod("StopICode");
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
      this.StopCoroutine("CoroutineUpdate");
      if (this.updateInterval == UpdateIntervalType.EveryFrame)
        ((Behaviour) this).set_enabled(true);
      else if (this.updateInterval == UpdateIntervalType.SpecifySeconds)
      {
        if (Application.get_isPlaying())
        {
          this.updateWait = new WaitForSeconds(this.updateIntervalSeconds);
          this.StartCoroutine("CoroutineUpdate");
        }
        ((Behaviour) this).set_enabled(false);
      }
      else
        ((Behaviour) this).set_enabled(false);
    }

    public void OnDestroy()
    {
      for (int index = this.behaviorTrees.Count - 1; index > -1; --index)
        this.DisableBehavior(this.behaviorTrees[index].behavior);
      ObjectPool.Clear();
    }

    public void OnApplicationQuit()
    {
      for (int index = this.behaviorTrees.Count - 1; index > -1; --index)
        this.DisableBehavior(this.behaviorTrees[index].behavior);
    }

    public void EnableBehavior(Behavior behavior)
    {
      if (this.IsBehaviorEnabled(behavior))
        return;
      BehaviorManager.BehaviorTree behaviorTree;
      if (this.pausedBehaviorTrees.TryGetValue(behavior, out behaviorTree))
      {
        this.behaviorTrees.Add(behaviorTree);
        this.pausedBehaviorTrees.Remove(behavior);
        behavior.ExecutionStatus = TaskStatus.Running;
        for (int index = 0; index < behaviorTree.taskList.Count; ++index)
          behaviorTree.taskList[index].OnPause(false);
      }
      else
      {
        BehaviorManager.TaskAddData data = ObjectPool.Get<BehaviorManager.TaskAddData>();
        data.Initialize();
        behavior.CheckForSerialization();
        Task rootTask = behavior.GetBehaviorSource().RootTask;
        if (rootTask == null)
        {
          Debug.LogError((object) string.Format("The behavior \"{0}\" on GameObject \"{1}\" contains no root task. This behavior will be disabled.", (object) behavior.GetBehaviorSource().behaviorName, (object) ((Object) ((Component) behavior).get_gameObject()).get_name()));
        }
        else
        {
          behaviorTree = ObjectPool.Get<BehaviorManager.BehaviorTree>();
          behaviorTree.Initialize(behavior);
          behaviorTree.parentIndex.Add(-1);
          behaviorTree.relativeChildIndex.Add(-1);
          behaviorTree.parentCompositeIndex.Add(-1);
          bool hasExternalBehavior = Object.op_Inequality((Object) behavior.ExternalBehavior, (Object) null);
          int taskList = this.AddToTaskList(behaviorTree, rootTask, ref hasExternalBehavior, data);
          if (taskList < 0)
          {
            behaviorTree = (BehaviorManager.BehaviorTree) null;
            switch (taskList + 6)
            {
              case 0:
                Debug.LogError((object) string.Format("The behavior \"{0}\" on GameObject \"{1}\" contains a root task which is disabled. This behavior will be disabled.", (object) behavior.GetBehaviorSource().behaviorName, (object) ((Object) ((Component) behavior).get_gameObject()).get_name(), (object) data.errorTaskName, (object) data.errorTask));
                break;
              case 1:
                Debug.LogError((object) string.Format("The behavior \"{0}\" on GameObject \"{1}\" contains a Behavior Tree Reference task ({2} (index {3})) that which has an element with a null value in the externalBehaviors array. This behavior will be disabled.", (object) behavior.GetBehaviorSource().behaviorName, (object) ((Object) ((Component) behavior).get_gameObject()).get_name(), (object) data.errorTaskName, (object) data.errorTask));
                break;
              case 2:
                Debug.LogError((object) string.Format("The behavior \"{0}\" on GameObject \"{1}\" contains multiple external behavior trees at the root task or as a child of a parent task which cannot contain so many children (such as a decorator task). This behavior will be disabled.", (object) behavior.GetBehaviorSource().behaviorName, (object) ((Object) ((Component) behavior).get_gameObject()).get_name()));
                break;
              case 3:
                Debug.LogError((object) string.Format("The behavior \"{0}\" on GameObject \"{1}\" contains a null task (referenced from parent task {2} (index {3})). This behavior will be disabled.", (object) behavior.GetBehaviorSource().behaviorName, (object) ((Object) ((Component) behavior).get_gameObject()).get_name(), (object) data.errorTaskName, (object) data.errorTask));
                break;
              case 4:
                Debug.LogError((object) string.Format("The behavior \"{0}\" on GameObject \"{1}\" cannot find the referenced external task. This behavior will be disabled.", (object) behavior.GetBehaviorSource().behaviorName, (object) ((Object) ((Component) behavior).get_gameObject()).get_name()));
                break;
              case 5:
                Debug.LogError((object) string.Format("The behavior \"{0}\" on GameObject \"{1}\" contains a parent task ({2} (index {3})) with no children. This behavior will be disabled.", (object) behavior.GetBehaviorSource().behaviorName, (object) ((Object) ((Component) behavior).get_gameObject()).get_name(), (object) data.errorTaskName, (object) data.errorTask));
                break;
            }
          }
          else
          {
            this.dirty = true;
            if (Object.op_Inequality((Object) behavior.ExternalBehavior, (Object) null))
              behavior.GetBehaviorSource().EntryTask = behavior.ExternalBehavior.BehaviorSource.EntryTask;
            behavior.GetBehaviorSource().RootTask = behaviorTree.taskList[0];
            if (behavior.ResetValuesOnRestart)
              behavior.SaveResetValues();
            Stack<int> intStack = ObjectPool.Get<Stack<int>>();
            intStack.Clear();
            behaviorTree.activeStack.Add(intStack);
            behaviorTree.interruptionIndex.Add(-1);
            behaviorTree.nonInstantTaskStatus.Add(TaskStatus.Inactive);
            if (behaviorTree.behavior.LogTaskChanges)
            {
              for (int index = 0; index < behaviorTree.taskList.Count; ++index)
                Debug.Log((object) string.Format("{0}: Task {1} ({2}, index {3}) {4}", (object) this.RoundedTime(), (object) behaviorTree.taskList[index].FriendlyName, (object) behaviorTree.taskList[index].GetType(), (object) index, (object) behaviorTree.taskList[index].GetHashCode()));
            }
            for (int index = 0; index < behaviorTree.taskList.Count; ++index)
              behaviorTree.taskList[index].OnAwake();
            this.behaviorTrees.Add(behaviorTree);
            this.behaviorTreeMap.Add(behavior, behaviorTree);
            if (this.onEnableBehavior != null)
              this.onEnableBehavior();
            if (behaviorTree.taskList[0].Disabled)
              return;
            behaviorTree.behavior.OnBehaviorStarted();
            behavior.ExecutionStatus = TaskStatus.Running;
            this.PushTask(behaviorTree, 0, 0);
          }
        }
      }
    }

    private int AddToTaskList(
      BehaviorManager.BehaviorTree behaviorTree,
      Task task,
      ref bool hasExternalBehavior,
      BehaviorManager.TaskAddData data)
    {
      if (task == null)
        return -3;
      task.GameObject = ((Component) behaviorTree.behavior).get_gameObject();
      task.Transform = ((Component) behaviorTree.behavior).get_transform();
      task.Owner = behaviorTree.behavior;
      if (task is BehaviorReference)
      {
        BehaviorReference behaviorReference = task as BehaviorReference;
        if (behaviorReference == null)
          return -2;
        ExternalBehavior[] externalBehaviors;
        if ((externalBehaviors = behaviorReference.GetExternalBehaviors()) == null)
          return -2;
        BehaviorSource[] behaviorSourceArray = new BehaviorSource[externalBehaviors.Length];
        for (int index = 0; index < externalBehaviors.Length; ++index)
        {
          if (Object.op_Equality((Object) externalBehaviors[index], (Object) null))
          {
            data.errorTask = behaviorTree.taskList.Count;
            data.errorTaskName = string.IsNullOrEmpty(task.FriendlyName) ? task.GetType().ToString() : task.FriendlyName;
            return -5;
          }
          behaviorSourceArray[index] = externalBehaviors[index].BehaviorSource;
          behaviorSourceArray[index].Owner = (IBehavior) externalBehaviors[index];
        }
        if (behaviorSourceArray == null)
          return -2;
        ParentTask parentTask = data.parentTask;
        int parentIndex = data.parentIndex;
        int compositeParentIndex = data.compositeParentIndex;
        data.offset = task.NodeData.Offset;
        ++data.depth;
        for (int index1 = 0; index1 < behaviorSourceArray.Length; ++index1)
        {
          BehaviorSource behaviorSource = ObjectPool.Get<BehaviorSource>();
          behaviorSource.Initialize(behaviorSourceArray[index1].Owner);
          behaviorSourceArray[index1].CheckForSerialization(true, behaviorSource);
          Task rootTask = behaviorSource.RootTask;
          if (rootTask != null)
          {
            if (rootTask is ParentTask)
              rootTask.NodeData.Collapsed = (task as BehaviorReference).collapsed;
            rootTask.Disabled = task.Disabled;
            if (behaviorReference.variables != null)
            {
              for (int index2 = 0; index2 < behaviorReference.variables.Length; ++index2)
              {
                if (data.overrideFields == null)
                {
                  data.overrideFields = ObjectPool.Get<Dictionary<string, BehaviorManager.TaskAddData.OverrideFieldValue>>();
                  data.overrideFields.Clear();
                }
                if (!data.overrideFields.ContainsKey(behaviorReference.variables[index2].Value.name))
                {
                  BehaviorManager.TaskAddData.OverrideFieldValue overrideFieldValue1 = ObjectPool.Get<BehaviorManager.TaskAddData.OverrideFieldValue>();
                  overrideFieldValue1.Initialize((object) behaviorReference.variables[index2].Value, data.depth);
                  if (behaviorReference.variables[index2].Value is NamedVariable)
                  {
                    NamedVariable namedVariable = behaviorReference.variables[index2].Value;
                    if (string.IsNullOrEmpty(namedVariable.name))
                    {
                      Debug.LogWarning((object) ("Warning: Named variable on reference task " + behaviorReference.FriendlyName + " (id " + (object) behaviorReference.ID + ") is null"));
                      continue;
                    }
                    BehaviorManager.TaskAddData.OverrideFieldValue overrideFieldValue2;
                    if (namedVariable.value != null && data.overrideFields.TryGetValue(namedVariable.name, out overrideFieldValue2))
                      overrideFieldValue1 = overrideFieldValue2;
                  }
                  else if (behaviorReference.variables[index2].Value is GenericVariable)
                  {
                    GenericVariable genericVariable = (GenericVariable) behaviorReference.variables[index2].Value;
                    if (genericVariable.value != null)
                    {
                      if (string.IsNullOrEmpty(genericVariable.value.Name))
                      {
                        Debug.LogWarning((object) ("Warning: Named variable on reference task " + behaviorReference.FriendlyName + " (id " + (object) behaviorReference.ID + ") is null"));
                        continue;
                      }
                      BehaviorManager.TaskAddData.OverrideFieldValue overrideFieldValue2;
                      if (data.overrideFields.TryGetValue(genericVariable.value.Name, out overrideFieldValue2))
                        overrideFieldValue1 = overrideFieldValue2;
                    }
                  }
                  data.overrideFields.Add(behaviorReference.variables[index2].Value.name, overrideFieldValue1);
                }
              }
            }
            if (behaviorSource.Variables != null)
            {
              for (int index2 = 0; index2 < behaviorSource.Variables.Count; ++index2)
              {
                SharedVariable variable;
                if ((variable = behaviorTree.behavior.GetVariable(behaviorSource.Variables[index2].Name)) == null)
                {
                  variable = behaviorSource.Variables[index2];
                  behaviorTree.behavior.SetVariable(variable.Name, variable);
                }
                else
                  behaviorSource.Variables[index2].SetValue(variable.GetValue());
                if (data.overrideFields == null)
                {
                  data.overrideFields = ObjectPool.Get<Dictionary<string, BehaviorManager.TaskAddData.OverrideFieldValue>>();
                  data.overrideFields.Clear();
                }
                if (!data.overrideFields.ContainsKey(variable.Name))
                {
                  BehaviorManager.TaskAddData.OverrideFieldValue overrideFieldValue = ObjectPool.Get<BehaviorManager.TaskAddData.OverrideFieldValue>();
                  overrideFieldValue.Initialize((object) variable, data.depth);
                  data.overrideFields.Add(variable.Name, overrideFieldValue);
                }
              }
            }
            ObjectPool.Return<BehaviorSource>(behaviorSource);
            if (index1 > 0)
            {
              data.parentTask = parentTask;
              data.parentIndex = parentIndex;
              data.compositeParentIndex = compositeParentIndex;
              if (data.parentTask == null || index1 >= data.parentTask.MaxChildren())
                return -4;
              behaviorTree.parentIndex.Add(data.parentIndex);
              behaviorTree.relativeChildIndex.Add(data.parentTask.Children.Count);
              behaviorTree.parentCompositeIndex.Add(data.compositeParentIndex);
              behaviorTree.childrenIndex[data.parentIndex].Add(behaviorTree.taskList.Count);
              data.parentTask.AddChild(rootTask, data.parentTask.Children.Count);
            }
            hasExternalBehavior = true;
            bool fromExternalTask = data.fromExternalTask;
            data.fromExternalTask = true;
            int taskList;
            if ((taskList = this.AddToTaskList(behaviorTree, rootTask, ref hasExternalBehavior, data)) < 0)
              return taskList;
            data.fromExternalTask = fromExternalTask;
          }
          else
          {
            ObjectPool.Return<BehaviorSource>(behaviorSource);
            return -2;
          }
        }
        if (data.overrideFields != null)
        {
          Dictionary<string, BehaviorManager.TaskAddData.OverrideFieldValue> dictionary = ObjectPool.Get<Dictionary<string, BehaviorManager.TaskAddData.OverrideFieldValue>>();
          dictionary.Clear();
          using (Dictionary<string, BehaviorManager.TaskAddData.OverrideFieldValue>.Enumerator enumerator = data.overrideFields.GetEnumerator())
          {
            while (enumerator.MoveNext())
            {
              KeyValuePair<string, BehaviorManager.TaskAddData.OverrideFieldValue> current = enumerator.Current;
              if (current.Value.Depth != data.depth)
                dictionary.Add(current.Key, current.Value);
            }
          }
          ObjectPool.Return<Dictionary<string, BehaviorManager.TaskAddData.OverrideFieldValue>>(data.overrideFields);
          data.overrideFields = dictionary;
        }
        --data.depth;
      }
      else
      {
        if (behaviorTree.taskList.Count == 0 && task.Disabled)
          return -6;
        task.ReferenceID = behaviorTree.taskList.Count;
        behaviorTree.taskList.Add(task);
        if (data.overrideFields != null)
          this.OverrideFields(behaviorTree, data, (object) task);
        if (data.fromExternalTask)
        {
          if (data.parentTask == null)
          {
            task.NodeData.Offset = behaviorTree.behavior.GetBehaviorSource().RootTask.NodeData.Offset;
          }
          else
          {
            int index = behaviorTree.relativeChildIndex[behaviorTree.relativeChildIndex.Count - 1];
            data.parentTask.ReplaceAddChild(task, index);
            if (Vector2.op_Inequality(data.offset, Vector2.get_zero()))
            {
              task.NodeData.Offset = data.offset;
              data.offset = Vector2.get_zero();
            }
          }
        }
        if (task is ParentTask)
        {
          ParentTask parentTask = task as ParentTask;
          if (parentTask.Children == null || parentTask.Children.Count == 0)
          {
            data.errorTask = behaviorTree.taskList.Count - 1;
            data.errorTaskName = string.IsNullOrEmpty(behaviorTree.taskList[data.errorTask].FriendlyName) ? behaviorTree.taskList[data.errorTask].GetType().ToString() : behaviorTree.taskList[data.errorTask].FriendlyName;
            return -1;
          }
          int index1 = behaviorTree.taskList.Count - 1;
          List<int> intList1 = ObjectPool.Get<List<int>>();
          intList1.Clear();
          behaviorTree.childrenIndex.Add(intList1);
          List<int> intList2 = ObjectPool.Get<List<int>>();
          intList2.Clear();
          behaviorTree.childConditionalIndex.Add(intList2);
          int count = parentTask.Children.Count;
          for (int index2 = 0; index2 < count; ++index2)
          {
            behaviorTree.parentIndex.Add(index1);
            behaviorTree.relativeChildIndex.Add(index2);
            behaviorTree.childrenIndex[index1].Add(behaviorTree.taskList.Count);
            data.parentTask = task as ParentTask;
            data.parentIndex = index1;
            if (task is Composite)
              data.compositeParentIndex = index1;
            behaviorTree.parentCompositeIndex.Add(data.compositeParentIndex);
            int taskList;
            if ((taskList = this.AddToTaskList(behaviorTree, parentTask.Children[index2], ref hasExternalBehavior, data)) < 0)
            {
              if (taskList == -3)
              {
                data.errorTask = index1;
                data.errorTaskName = string.IsNullOrEmpty(behaviorTree.taskList[data.errorTask].FriendlyName) ? behaviorTree.taskList[data.errorTask].GetType().ToString() : behaviorTree.taskList[data.errorTask].FriendlyName;
              }
              return taskList;
            }
          }
        }
        else
        {
          behaviorTree.childrenIndex.Add((List<int>) null);
          behaviorTree.childConditionalIndex.Add((List<int>) null);
          if (task is Conditional)
          {
            int index1 = behaviorTree.taskList.Count - 1;
            int index2 = behaviorTree.parentCompositeIndex[index1];
            if (index2 != -1)
              behaviorTree.childConditionalIndex[index2].Add(index1);
          }
        }
      }
      return 0;
    }

    private void OverrideFields(
      BehaviorManager.BehaviorTree behaviorTree,
      BehaviorManager.TaskAddData data,
      object obj)
    {
      if (obj == null || object.Equals(obj, (object) null))
        return;
      FieldInfo[] allFields = TaskUtility.GetAllFields(obj.GetType());
      for (int index1 = 0; index1 < allFields.Length; ++index1)
      {
        object obj1 = allFields[index1].GetValue(obj);
        if (obj1 != null)
        {
          if (typeof (SharedVariable).IsAssignableFrom(allFields[index1].FieldType))
          {
            SharedVariable sharedVariable = this.OverrideSharedVariable(behaviorTree, data, allFields[index1].FieldType, obj1 as SharedVariable);
            if (sharedVariable != null)
              allFields[index1].SetValue(obj, (object) sharedVariable);
          }
          else if (typeof (IList).IsAssignableFrom(allFields[index1].FieldType))
          {
            Type fieldType;
            if (typeof (SharedVariable).IsAssignableFrom(fieldType = allFields[index1].FieldType.GetElementType()) || allFields[index1].FieldType.IsGenericType && typeof (SharedVariable).IsAssignableFrom(fieldType = allFields[index1].FieldType.GetGenericArguments()[0]))
            {
              IList<SharedVariable> sharedVariableList = obj1 as IList<SharedVariable>;
              if (sharedVariableList != null)
              {
                for (int index2 = 0; index2 < sharedVariableList.Count; ++index2)
                {
                  SharedVariable sharedVariable = this.OverrideSharedVariable(behaviorTree, data, fieldType, sharedVariableList[index2]);
                  if (sharedVariable != null)
                    sharedVariableList[index2] = sharedVariable;
                }
              }
            }
          }
          else if (allFields[index1].FieldType.IsClass && !allFields[index1].FieldType.Equals(typeof (Type)) && (!typeof (Delegate).IsAssignableFrom(allFields[index1].FieldType) && !data.overiddenFields.Contains(obj1)))
          {
            data.overiddenFields.Add(obj1);
            this.OverrideFields(behaviorTree, data, obj1);
            data.overiddenFields.Remove(obj1);
          }
        }
      }
    }

    private SharedVariable OverrideSharedVariable(
      BehaviorManager.BehaviorTree behaviorTree,
      BehaviorManager.TaskAddData data,
      Type fieldType,
      SharedVariable sharedVariable)
    {
      SharedVariable sharedVariable1 = sharedVariable;
      if (sharedVariable is SharedGenericVariable)
        sharedVariable = ((sharedVariable as SharedGenericVariable).GetValue() as GenericVariable).value;
      else if (sharedVariable is SharedNamedVariable)
        sharedVariable = ((sharedVariable as SharedNamedVariable).GetValue() as NamedVariable).value;
      if (sharedVariable == null)
        return (SharedVariable) null;
      BehaviorManager.TaskAddData.OverrideFieldValue overrideFieldValue;
      if (!string.IsNullOrEmpty(sharedVariable.Name) && data.overrideFields.TryGetValue(sharedVariable.Name, out overrideFieldValue))
      {
        SharedVariable sharedVariable2 = (SharedVariable) null;
        if (overrideFieldValue.Value is SharedVariable)
          sharedVariable2 = overrideFieldValue.Value as SharedVariable;
        else if (overrideFieldValue.Value is NamedVariable)
        {
          sharedVariable2 = (overrideFieldValue.Value as NamedVariable).value;
          if (sharedVariable2.IsShared)
            sharedVariable2 = behaviorTree.behavior.GetVariable(sharedVariable2.Name);
        }
        else if (overrideFieldValue.Value is GenericVariable)
        {
          sharedVariable2 = (overrideFieldValue.Value as GenericVariable).value;
          if (sharedVariable2.IsShared)
            sharedVariable2 = behaviorTree.behavior.GetVariable(sharedVariable2.Name);
        }
        if (sharedVariable1 is SharedNamedVariable || sharedVariable1 is SharedGenericVariable)
        {
          if (fieldType.Equals(typeof (SharedVariable)) || sharedVariable2.GetType().Equals(sharedVariable.GetType()))
          {
            if (sharedVariable1 is SharedNamedVariable)
              (sharedVariable1 as SharedNamedVariable).Value.value = sharedVariable2;
            else if (sharedVariable1 is SharedGenericVariable)
              (sharedVariable1 as SharedGenericVariable).Value.value = sharedVariable2;
            behaviorTree.behavior.SetVariableValue(sharedVariable.Name, sharedVariable2.GetValue());
          }
        }
        else if (sharedVariable2 is SharedVariable)
          return sharedVariable2;
      }
      return (SharedVariable) null;
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
          return;
        this.EnableBehavior(behavior);
      }
      if (behavior.LogTaskChanges)
        Debug.Log((object) string.Format("{0}: {1} {2}", (object) this.RoundedTime(), !paused ? (object) "Disabling" : (object) "Pausing", (object) behavior.ToString()));
      if (paused)
      {
        BehaviorManager.BehaviorTree behaviorTree;
        if (!this.behaviorTreeMap.TryGetValue(behavior, out behaviorTree) || this.pausedBehaviorTrees.ContainsKey(behavior))
          return;
        this.pausedBehaviorTrees.Add(behavior, behaviorTree);
        behavior.ExecutionStatus = TaskStatus.Inactive;
        for (int index = 0; index < behaviorTree.taskList.Count; ++index)
          behaviorTree.taskList[index].OnPause(true);
        this.behaviorTrees.Remove(behaviorTree);
      }
      else
        this.DestroyBehavior(behavior, executionStatus);
    }

    public void DestroyBehavior(Behavior behavior)
    {
      this.DestroyBehavior(behavior, TaskStatus.Success);
    }

    public void DestroyBehavior(Behavior behavior, TaskStatus executionStatus)
    {
      BehaviorManager.BehaviorTree behaviorTree;
      if (!this.behaviorTreeMap.TryGetValue(behavior, out behaviorTree) || behaviorTree.destroyBehavior)
        return;
      behaviorTree.destroyBehavior = true;
      if (this.pausedBehaviorTrees.ContainsKey(behavior))
      {
        this.pausedBehaviorTrees.Remove(behavior);
        for (int index = 0; index < behaviorTree.taskList.Count; ++index)
          behaviorTree.taskList[index].OnPause(false);
        behavior.ExecutionStatus = TaskStatus.Running;
      }
      TaskStatus status = executionStatus;
      for (int stackIndex = behaviorTree.activeStack.Count - 1; stackIndex > -1; --stackIndex)
      {
        while (behaviorTree.activeStack[stackIndex].Count > 0)
        {
          int count = behaviorTree.activeStack[stackIndex].Count;
          this.PopTask(behaviorTree, behaviorTree.activeStack[stackIndex].Peek(), stackIndex, ref status, true, false);
          if (count == 1)
            break;
        }
      }
      this.RemoveChildConditionalReevaluate(behaviorTree, -1);
      for (int index = 0; index < behaviorTree.taskList.Count; ++index)
        behaviorTree.taskList[index].OnBehaviorComplete();
      this.behaviorTreeMap.Remove(behavior);
      this.behaviorTrees.Remove(behaviorTree);
      behaviorTree.destroyBehavior = false;
      ObjectPool.Return<BehaviorManager.BehaviorTree>(behaviorTree);
      behavior.ExecutionStatus = status;
      behavior.OnBehaviorEnded();
    }

    public void RestartBehavior(Behavior behavior)
    {
      if (!this.IsBehaviorEnabled(behavior))
        return;
      BehaviorManager.BehaviorTree behaviorTree = this.behaviorTreeMap[behavior];
      TaskStatus status = TaskStatus.Success;
      for (int stackIndex = behaviorTree.activeStack.Count - 1; stackIndex > -1; --stackIndex)
      {
        while (behaviorTree.activeStack[stackIndex].Count > 0)
        {
          int count = behaviorTree.activeStack[stackIndex].Count;
          this.PopTask(behaviorTree, behaviorTree.activeStack[stackIndex].Peek(), stackIndex, ref status, true, false);
          if (count == 1)
            break;
        }
      }
      this.Restart(behaviorTree);
    }

    public bool IsBehaviorEnabled(Behavior behavior)
    {
      if (this.behaviorTreeMap != null && this.behaviorTreeMap.Count > 0 && Object.op_Inequality((Object) behavior, (Object) null))
        return behavior.ExecutionStatus == TaskStatus.Running;
      return false;
    }

    public void Update()
    {
      this.Tick();
    }

    public void LateUpdate()
    {
      for (int index1 = 0; index1 < this.behaviorTrees.Count; ++index1)
      {
        if (this.behaviorTrees[index1].behavior.HasEvent[9])
        {
          for (int index2 = this.behaviorTrees[index1].activeStack.Count - 1; index2 > -1; --index2)
          {
            int index3 = this.behaviorTrees[index1].activeStack[index2].Peek();
            this.behaviorTrees[index1].taskList[index3].OnLateUpdate();
          }
        }
      }
    }

    public void FixedUpdate()
    {
      for (int index1 = 0; index1 < this.behaviorTrees.Count; ++index1)
      {
        if (this.behaviorTrees[index1].behavior.HasEvent[10])
        {
          for (int index2 = this.behaviorTrees[index1].activeStack.Count - 1; index2 > -1; --index2)
          {
            int index3 = this.behaviorTrees[index1].activeStack[index2].Peek();
            this.behaviorTrees[index1].taskList[index3].OnFixedUpdate();
          }
        }
      }
    }

    [DebuggerHidden]
    private IEnumerator CoroutineUpdate()
    {
      // ISSUE: object of a compiler-generated type is created
      return (IEnumerator) new BehaviorManager.\u003CCoroutineUpdate\u003Ec__Iterator0()
      {
        \u003C\u003Ef__this = this
      };
    }

    public void Tick()
    {
      for (int index = 0; index < this.behaviorTrees.Count; ++index)
        this.Tick(this.behaviorTrees[index]);
    }

    public void Tick(Behavior behavior)
    {
      if (Object.op_Equality((Object) behavior, (Object) null) || !this.IsBehaviorEnabled(behavior))
        return;
      this.Tick(this.behaviorTreeMap[behavior]);
    }

    private void Tick(BehaviorManager.BehaviorTree behaviorTree)
    {
      behaviorTree.executionCount = 0;
      this.ReevaluateParentTasks(behaviorTree);
      this.ReevaluateConditionalTasks(behaviorTree);
      for (int stackIndex = behaviorTree.activeStack.Count - 1; stackIndex > -1; --stackIndex)
      {
        TaskStatus status = TaskStatus.Inactive;
        int taskIndex1;
        if (stackIndex < behaviorTree.interruptionIndex.Count && (taskIndex1 = behaviorTree.interruptionIndex[stackIndex]) != -1)
        {
          behaviorTree.interruptionIndex[stackIndex] = -1;
          while (behaviorTree.activeStack[stackIndex].Peek() != taskIndex1)
          {
            int count = behaviorTree.activeStack[stackIndex].Count;
            this.PopTask(behaviorTree, behaviorTree.activeStack[stackIndex].Peek(), stackIndex, ref status, true);
            if (count == 1)
              break;
          }
          if (stackIndex < behaviorTree.activeStack.Count && behaviorTree.activeStack[stackIndex].Count > 0 && behaviorTree.taskList[taskIndex1] == behaviorTree.taskList[behaviorTree.activeStack[stackIndex].Peek()])
          {
            if (behaviorTree.taskList[taskIndex1] is ParentTask)
              status = (behaviorTree.taskList[taskIndex1] as ParentTask).OverrideStatus();
            this.PopTask(behaviorTree, taskIndex1, stackIndex, ref status, true);
          }
        }
        int num = -1;
        int taskIndex2;
        for (; status != TaskStatus.Running && stackIndex < behaviorTree.activeStack.Count && behaviorTree.activeStack[stackIndex].Count > 0; status = this.RunTask(behaviorTree, taskIndex2, stackIndex, status))
        {
          taskIndex2 = behaviorTree.activeStack[stackIndex].Peek();
          if ((stackIndex >= behaviorTree.activeStack.Count || behaviorTree.activeStack[stackIndex].Count <= 0 || num != behaviorTree.activeStack[stackIndex].Peek()) && this.IsBehaviorEnabled(behaviorTree.behavior))
            num = taskIndex2;
          else
            break;
        }
      }
    }

    private void ReevaluateConditionalTasks(BehaviorManager.BehaviorTree behaviorTree)
    {
      for (int index1 = 0; index1 < behaviorTree.conditionalReevaluate.Count; ++index1)
      {
        if (behaviorTree.conditionalReevaluate[index1].compositeIndex != -1)
        {
          int index2 = behaviorTree.conditionalReevaluate[index1].index;
          TaskStatus taskStatus = behaviorTree.taskList[index2].OnUpdate();
          if (taskStatus != behaviorTree.conditionalReevaluate[index1].taskStatus)
          {
            if (behaviorTree.behavior.LogTaskChanges)
            {
              int index3 = behaviorTree.parentCompositeIndex[index2];
              MonoBehaviour.print((object) string.Format("{0}: {1}: Conditional abort with task {2} ({3}, index {4}) because of conditional task {5} ({6}, index {7}) with status {8}", (object) this.RoundedTime(), (object) behaviorTree.behavior.ToString(), (object) behaviorTree.taskList[index3].FriendlyName, (object) behaviorTree.taskList[index3].GetType(), (object) index3, (object) behaviorTree.taskList[index2].FriendlyName, (object) behaviorTree.taskList[index2].GetType(), (object) index2, (object) taskStatus));
            }
            int compositeIndex = behaviorTree.conditionalReevaluate[index1].compositeIndex;
            for (int stackIndex = behaviorTree.activeStack.Count - 1; stackIndex > -1; --stackIndex)
            {
              if (behaviorTree.activeStack[stackIndex].Count > 0)
              {
                int index3 = behaviorTree.activeStack[stackIndex].Peek();
                int lca = this.FindLCA(behaviorTree, index2, index3);
                if (this.IsChild(behaviorTree, lca, compositeIndex))
                {
                  for (int count = behaviorTree.activeStack.Count; index3 != -1 && index3 != lca && behaviorTree.activeStack.Count == count; index3 = behaviorTree.parentIndex[index3])
                  {
                    TaskStatus status = TaskStatus.Failure;
                    behaviorTree.taskList[index3].OnConditionalAbort();
                    this.PopTask(behaviorTree, index3, stackIndex, ref status, false);
                  }
                }
              }
            }
            for (int index3 = behaviorTree.conditionalReevaluate.Count - 1; index3 > index1 - 1; --index3)
            {
              BehaviorManager.BehaviorTree.ConditionalReevaluate conditionalReevaluate = behaviorTree.conditionalReevaluate[index3];
              if (this.FindLCA(behaviorTree, compositeIndex, conditionalReevaluate.index) == compositeIndex)
              {
                behaviorTree.taskList[behaviorTree.conditionalReevaluate[index3].index].NodeData.IsReevaluating = false;
                ObjectPool.Return<BehaviorManager.BehaviorTree.ConditionalReevaluate>(behaviorTree.conditionalReevaluate[index3]);
                behaviorTree.conditionalReevaluateMap.Remove(behaviorTree.conditionalReevaluate[index3].index);
                behaviorTree.conditionalReevaluate.RemoveAt(index3);
              }
            }
            Composite task1 = behaviorTree.taskList[behaviorTree.parentCompositeIndex[index2]] as Composite;
            for (int index3 = index1 - 1; index3 > -1; --index3)
            {
              BehaviorManager.BehaviorTree.ConditionalReevaluate conditionalReevaluate = behaviorTree.conditionalReevaluate[index3];
              if (task1.AbortType == AbortType.LowerPriority && behaviorTree.parentCompositeIndex[conditionalReevaluate.index] == behaviorTree.parentCompositeIndex[index2])
              {
                behaviorTree.taskList[behaviorTree.conditionalReevaluate[index3].index].NodeData.IsReevaluating = false;
                behaviorTree.conditionalReevaluate[index3].compositeIndex = -1;
              }
              else if (behaviorTree.parentCompositeIndex[conditionalReevaluate.index] == behaviorTree.parentCompositeIndex[index2])
              {
                for (int index4 = 0; index4 < behaviorTree.childrenIndex[compositeIndex].Count; ++index4)
                {
                  if (this.IsParentTask(behaviorTree, behaviorTree.childrenIndex[compositeIndex][index4], conditionalReevaluate.index))
                  {
                    int index5 = behaviorTree.childrenIndex[compositeIndex][index4];
                    while (!(behaviorTree.taskList[index5] is Composite) && behaviorTree.childrenIndex[index5] != null)
                      index5 = behaviorTree.childrenIndex[index5][0];
                    if (behaviorTree.taskList[index5] is Composite)
                    {
                      conditionalReevaluate.compositeIndex = index5;
                      break;
                    }
                    break;
                  }
                }
              }
            }
            this.conditionalParentIndexes.Clear();
            for (int index3 = behaviorTree.parentIndex[index2]; index3 != compositeIndex; index3 = behaviorTree.parentIndex[index3])
              this.conditionalParentIndexes.Add(index3);
            if (this.conditionalParentIndexes.Count == 0)
              this.conditionalParentIndexes.Add(behaviorTree.parentIndex[index2]);
            (behaviorTree.taskList[compositeIndex] as ParentTask).OnConditionalAbort(behaviorTree.relativeChildIndex[this.conditionalParentIndexes[this.conditionalParentIndexes.Count - 1]]);
            for (int index3 = this.conditionalParentIndexes.Count - 1; index3 > -1; --index3)
            {
              ParentTask task2 = behaviorTree.taskList[this.conditionalParentIndexes[index3]] as ParentTask;
              if (index3 == 0)
                task2.OnConditionalAbort(behaviorTree.relativeChildIndex[index2]);
              else
                task2.OnConditionalAbort(behaviorTree.relativeChildIndex[this.conditionalParentIndexes[index3 - 1]]);
            }
            behaviorTree.taskList[index2].NodeData.InterruptTime = Time.get_realtimeSinceStartup();
          }
        }
      }
    }

    private void ReevaluateParentTasks(BehaviorManager.BehaviorTree behaviorTree)
    {
      for (int index = behaviorTree.parentReevaluate.Count - 1; index > -1; --index)
      {
        int taskIndex = behaviorTree.parentReevaluate[index];
        if (behaviorTree.taskList[taskIndex] is Decorator)
        {
          if (behaviorTree.taskList[taskIndex].OnUpdate() == TaskStatus.Failure)
            this.Interrupt(behaviorTree.behavior, behaviorTree.taskList[taskIndex]);
        }
        else if (behaviorTree.taskList[taskIndex] is Composite)
        {
          ParentTask task = behaviorTree.taskList[taskIndex] as ParentTask;
          if (task.OnReevaluationStarted())
          {
            int stackIndex = 0;
            TaskStatus status = this.RunParentTask(behaviorTree, taskIndex, ref stackIndex, TaskStatus.Inactive);
            task.OnReevaluationEnded(status);
          }
        }
      }
    }

    private TaskStatus RunTask(
      BehaviorManager.BehaviorTree behaviorTree,
      int taskIndex,
      int stackIndex,
      TaskStatus previousStatus)
    {
      Task task1 = behaviorTree.taskList[taskIndex];
      if (task1 == null)
        return previousStatus;
      if (task1.Disabled)
      {
        if (behaviorTree.behavior.LogTaskChanges)
          MonoBehaviour.print((object) string.Format("{0}: {1}: Skip task {2} ({3}, index {4}) at stack index {5} (task disabled)", (object) this.RoundedTime(), (object) behaviorTree.behavior.ToString(), (object) behaviorTree.taskList[taskIndex].FriendlyName, (object) behaviorTree.taskList[taskIndex].GetType(), (object) taskIndex, (object) stackIndex));
        if (behaviorTree.parentIndex[taskIndex] != -1)
        {
          ParentTask task2 = behaviorTree.taskList[behaviorTree.parentIndex[taskIndex]] as ParentTask;
          if (!task2.CanRunParallelChildren())
          {
            task2.OnChildExecuted(TaskStatus.Inactive);
          }
          else
          {
            task2.OnChildExecuted(behaviorTree.relativeChildIndex[taskIndex], TaskStatus.Inactive);
            this.RemoveStack(behaviorTree, stackIndex);
          }
        }
        return previousStatus;
      }
      TaskStatus status1 = previousStatus;
      if (!task1.IsInstant && (behaviorTree.nonInstantTaskStatus[stackIndex] == TaskStatus.Failure || behaviorTree.nonInstantTaskStatus[stackIndex] == TaskStatus.Success))
      {
        TaskStatus instantTaskStatu = behaviorTree.nonInstantTaskStatus[stackIndex];
        this.PopTask(behaviorTree, taskIndex, stackIndex, ref instantTaskStatu, true);
        return instantTaskStatu;
      }
      this.PushTask(behaviorTree, taskIndex, stackIndex);
      if (Object.op_Inequality((Object) this.breakpointTree, (Object) null))
        return TaskStatus.Running;
      TaskStatus status2 = !(task1 is ParentTask) ? task1.OnUpdate() : (task1 as ParentTask).OverrideStatus(this.RunParentTask(behaviorTree, taskIndex, ref stackIndex, status1));
      if (status2 != TaskStatus.Running)
      {
        if (task1.IsInstant)
          this.PopTask(behaviorTree, taskIndex, stackIndex, ref status2, true);
        else
          behaviorTree.nonInstantTaskStatus[stackIndex] = status2;
      }
      return status2;
    }

    private TaskStatus RunParentTask(
      BehaviorManager.BehaviorTree behaviorTree,
      int taskIndex,
      ref int stackIndex,
      TaskStatus status)
    {
      ParentTask task = behaviorTree.taskList[taskIndex] as ParentTask;
      if (!task.CanRunParallelChildren() || task.OverrideStatus(TaskStatus.Running) != TaskStatus.Running)
      {
        TaskStatus taskStatus = TaskStatus.Inactive;
        int num1 = stackIndex;
        int num2 = -1;
        List<int> intList;
        int childIndex;
        for (Behavior behavior = behaviorTree.behavior; task.CanExecute() && (taskStatus != TaskStatus.Running || task.CanRunParallelChildren()) && this.IsBehaviorEnabled(behavior); status = taskStatus = this.RunTask(behaviorTree, intList[childIndex], stackIndex, status))
        {
          intList = behaviorTree.childrenIndex[taskIndex];
          childIndex = task.CurrentChildIndex();
          if (this.executionsPerTick == BehaviorManager.ExecutionsPerTickType.NoDuplicates && childIndex == num2 || this.executionsPerTick == BehaviorManager.ExecutionsPerTickType.Count && behaviorTree.executionCount >= this.maxTaskExecutionsPerTick)
          {
            if (this.executionsPerTick == BehaviorManager.ExecutionsPerTickType.Count)
              Debug.LogWarning((object) string.Format("{0}: {1}: More than the specified number of task executions per tick ({2}) have executed, returning early.", (object) this.RoundedTime(), (object) behaviorTree.behavior.ToString(), (object) this.maxTaskExecutionsPerTick));
            status = TaskStatus.Running;
            break;
          }
          num2 = childIndex;
          if (task.CanRunParallelChildren())
          {
            behaviorTree.activeStack.Add(ObjectPool.Get<Stack<int>>());
            behaviorTree.interruptionIndex.Add(-1);
            behaviorTree.nonInstantTaskStatus.Add(TaskStatus.Inactive);
            stackIndex = behaviorTree.activeStack.Count - 1;
            task.OnChildStarted(childIndex);
          }
          else
            task.OnChildStarted();
        }
        stackIndex = num1;
      }
      return status;
    }

    private void PushTask(BehaviorManager.BehaviorTree behaviorTree, int taskIndex, int stackIndex)
    {
      if (!this.IsBehaviorEnabled(behaviorTree.behavior) || stackIndex >= behaviorTree.activeStack.Count)
        return;
      Stack<int> active = behaviorTree.activeStack[stackIndex];
      if (active.Count != 0 && active.Peek() == taskIndex)
        return;
      active.Push(taskIndex);
      behaviorTree.nonInstantTaskStatus[stackIndex] = TaskStatus.Running;
      ++behaviorTree.executionCount;
      Task task = behaviorTree.taskList[taskIndex];
      task.NodeData.PushTime = Time.get_realtimeSinceStartup();
      task.NodeData.ExecutionStatus = TaskStatus.Running;
      if (task.NodeData.IsBreakpoint && this.onTaskBreakpoint != null)
      {
        this.breakpointTree = behaviorTree.behavior;
        this.onTaskBreakpoint();
      }
      if (behaviorTree.behavior.LogTaskChanges)
        MonoBehaviour.print((object) string.Format("{0}: {1}: Push task {2} ({3}, index {4}) at stack index {5}", (object) this.RoundedTime(), (object) behaviorTree.behavior.ToString(), (object) task.FriendlyName, (object) task.GetType(), (object) taskIndex, (object) stackIndex));
      task.OnStart();
      if (!(task is ParentTask) || !(task as ParentTask).CanReevaluate())
        return;
      behaviorTree.parentReevaluate.Add(taskIndex);
    }

    private void PopTask(
      BehaviorManager.BehaviorTree behaviorTree,
      int taskIndex,
      int stackIndex,
      ref TaskStatus status,
      bool popChildren)
    {
      this.PopTask(behaviorTree, taskIndex, stackIndex, ref status, popChildren, true);
    }

    private void PopTask(
      BehaviorManager.BehaviorTree behaviorTree,
      int taskIndex,
      int stackIndex,
      ref TaskStatus status,
      bool popChildren,
      bool notifyOnEmptyStack)
    {
      if (!this.IsBehaviorEnabled(behaviorTree.behavior) || stackIndex >= behaviorTree.activeStack.Count || (behaviorTree.activeStack[stackIndex].Count == 0 || taskIndex != behaviorTree.activeStack[stackIndex].Peek()))
        return;
      behaviorTree.activeStack[stackIndex].Pop();
      behaviorTree.nonInstantTaskStatus[stackIndex] = TaskStatus.Inactive;
      this.StopThirdPartyTask(behaviorTree, taskIndex);
      Task task1 = behaviorTree.taskList[taskIndex];
      task1.OnEnd();
      int index1 = behaviorTree.parentIndex[taskIndex];
      task1.NodeData.PushTime = -1f;
      task1.NodeData.PopTime = Time.get_realtimeSinceStartup();
      task1.NodeData.ExecutionStatus = status;
      if (behaviorTree.behavior.LogTaskChanges)
        MonoBehaviour.print((object) string.Format("{0}: {1}: Pop task {2} ({3}, index {4}) at stack index {5} with status {6}", (object) this.RoundedTime(), (object) behaviorTree.behavior.ToString(), (object) task1.FriendlyName, (object) task1.GetType(), (object) taskIndex, (object) stackIndex, (object) status));
      if (index1 != -1)
      {
        if (task1 is Conditional)
        {
          int index2 = behaviorTree.parentCompositeIndex[taskIndex];
          if (index2 != -1)
          {
            Composite task2 = behaviorTree.taskList[index2] as Composite;
            if (task2.AbortType != AbortType.None)
            {
              BehaviorManager.BehaviorTree.ConditionalReevaluate conditionalReevaluate1;
              if (behaviorTree.conditionalReevaluateMap.TryGetValue(taskIndex, out conditionalReevaluate1))
              {
                conditionalReevaluate1.compositeIndex = task2.AbortType == AbortType.LowerPriority ? -1 : index2;
                conditionalReevaluate1.taskStatus = status;
                task1.NodeData.IsReevaluating = task2.AbortType != AbortType.LowerPriority;
              }
              else
              {
                BehaviorManager.BehaviorTree.ConditionalReevaluate conditionalReevaluate2 = ObjectPool.Get<BehaviorManager.BehaviorTree.ConditionalReevaluate>();
                conditionalReevaluate2.Initialize(taskIndex, status, stackIndex, task2.AbortType == AbortType.LowerPriority ? -1 : index2);
                behaviorTree.conditionalReevaluate.Add(conditionalReevaluate2);
                behaviorTree.conditionalReevaluateMap.Add(taskIndex, conditionalReevaluate2);
                task1.NodeData.IsReevaluating = task2.AbortType == AbortType.Self || task2.AbortType == AbortType.Both;
              }
            }
          }
        }
        ParentTask task3 = behaviorTree.taskList[index1] as ParentTask;
        if (!task3.CanRunParallelChildren())
        {
          task3.OnChildExecuted(status);
          status = task3.Decorate(status);
        }
        else
          task3.OnChildExecuted(behaviorTree.relativeChildIndex[taskIndex], status);
      }
      if (task1 is ParentTask)
      {
        ParentTask parentTask = task1 as ParentTask;
        if (parentTask.CanReevaluate())
        {
          for (int index2 = behaviorTree.parentReevaluate.Count - 1; index2 > -1; --index2)
          {
            if (behaviorTree.parentReevaluate[index2] == taskIndex)
            {
              behaviorTree.parentReevaluate.RemoveAt(index2);
              break;
            }
          }
        }
        if (parentTask is Composite)
        {
          Composite composite = parentTask as Composite;
          if (composite.AbortType == AbortType.Self || composite.AbortType == AbortType.None || behaviorTree.activeStack[stackIndex].Count == 0)
            this.RemoveChildConditionalReevaluate(behaviorTree, taskIndex);
          else if (composite.AbortType == AbortType.LowerPriority || composite.AbortType == AbortType.Both)
          {
            int taskIndex1 = behaviorTree.parentCompositeIndex[taskIndex];
            if (taskIndex1 != -1)
            {
              if (!(behaviorTree.taskList[taskIndex1] as ParentTask).CanRunParallelChildren())
              {
                for (int index2 = 0; index2 < behaviorTree.childConditionalIndex[taskIndex].Count; ++index2)
                {
                  int key = behaviorTree.childConditionalIndex[taskIndex][index2];
                  BehaviorManager.BehaviorTree.ConditionalReevaluate conditionalReevaluate1;
                  if (behaviorTree.conditionalReevaluateMap.TryGetValue(key, out conditionalReevaluate1))
                  {
                    if (!(behaviorTree.taskList[taskIndex1] as ParentTask).CanRunParallelChildren())
                    {
                      conditionalReevaluate1.compositeIndex = behaviorTree.parentCompositeIndex[taskIndex];
                      behaviorTree.taskList[key].NodeData.IsReevaluating = true;
                    }
                    else
                    {
                      for (int index3 = behaviorTree.conditionalReevaluate.Count - 1; index3 > index2 - 1; --index3)
                      {
                        BehaviorManager.BehaviorTree.ConditionalReevaluate conditionalReevaluate2 = behaviorTree.conditionalReevaluate[index3];
                        if (this.FindLCA(behaviorTree, taskIndex1, conditionalReevaluate2.index) == taskIndex1)
                        {
                          behaviorTree.taskList[behaviorTree.conditionalReevaluate[index3].index].NodeData.IsReevaluating = false;
                          ObjectPool.Return<BehaviorManager.BehaviorTree.ConditionalReevaluate>(behaviorTree.conditionalReevaluate[index3]);
                          behaviorTree.conditionalReevaluateMap.Remove(behaviorTree.conditionalReevaluate[index3].index);
                          behaviorTree.conditionalReevaluate.RemoveAt(index3);
                        }
                      }
                    }
                  }
                }
              }
              else
                this.RemoveChildConditionalReevaluate(behaviorTree, taskIndex);
            }
            for (int index2 = 0; index2 < behaviorTree.conditionalReevaluate.Count; ++index2)
            {
              if (behaviorTree.conditionalReevaluate[index2].compositeIndex == taskIndex)
                behaviorTree.conditionalReevaluate[index2].compositeIndex = behaviorTree.parentCompositeIndex[taskIndex];
            }
          }
        }
      }
      if (popChildren)
      {
        for (int stackIndex1 = behaviorTree.activeStack.Count - 1; stackIndex1 > stackIndex; --stackIndex1)
        {
          if (behaviorTree.activeStack[stackIndex1].Count > 0 && this.IsParentTask(behaviorTree, taskIndex, behaviorTree.activeStack[stackIndex1].Peek()))
          {
            TaskStatus status1 = TaskStatus.Failure;
            for (int count = behaviorTree.activeStack[stackIndex1].Count; count > 0; --count)
              this.PopTask(behaviorTree, behaviorTree.activeStack[stackIndex1].Peek(), stackIndex1, ref status1, false, notifyOnEmptyStack);
          }
        }
      }
      if (stackIndex >= behaviorTree.activeStack.Count || behaviorTree.activeStack[stackIndex].Count != 0)
        return;
      if (stackIndex == 0)
      {
        if (notifyOnEmptyStack)
        {
          if (behaviorTree.behavior.RestartWhenComplete)
            this.Restart(behaviorTree);
          else
            this.DisableBehavior(behaviorTree.behavior, false, status);
        }
        status = TaskStatus.Inactive;
      }
      else
      {
        this.RemoveStack(behaviorTree, stackIndex);
        status = TaskStatus.Running;
      }
    }

    private void RemoveChildConditionalReevaluate(
      BehaviorManager.BehaviorTree behaviorTree,
      int compositeIndex)
    {
      for (int index1 = behaviorTree.conditionalReevaluate.Count - 1; index1 > -1; --index1)
      {
        if (behaviorTree.conditionalReevaluate[index1].compositeIndex == compositeIndex)
        {
          ObjectPool.Return<BehaviorManager.BehaviorTree.ConditionalReevaluate>(behaviorTree.conditionalReevaluate[index1]);
          int index2 = behaviorTree.conditionalReevaluate[index1].index;
          behaviorTree.conditionalReevaluateMap.Remove(index2);
          behaviorTree.conditionalReevaluate.RemoveAt(index1);
          behaviorTree.taskList[index2].NodeData.IsReevaluating = false;
        }
      }
    }

    private void Restart(BehaviorManager.BehaviorTree behaviorTree)
    {
      if (behaviorTree.behavior.LogTaskChanges)
        Debug.Log((object) string.Format("{0}: Restarting {1}", (object) this.RoundedTime(), (object) behaviorTree.behavior.ToString()));
      this.RemoveChildConditionalReevaluate(behaviorTree, -1);
      if (behaviorTree.behavior.ResetValuesOnRestart)
        behaviorTree.behavior.SaveResetValues();
      for (int index = 0; index < behaviorTree.taskList.Count; ++index)
        behaviorTree.taskList[index].OnBehaviorRestart();
      behaviorTree.behavior.OnBehaviorRestarted();
      this.PushTask(behaviorTree, 0, 0);
    }

    private bool IsParentTask(
      BehaviorManager.BehaviorTree behaviorTree,
      int possibleParent,
      int possibleChild)
    {
      int num;
      for (int index = possibleChild; index != -1; index = num)
      {
        num = behaviorTree.parentIndex[index];
        if (num == possibleParent)
          return true;
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
        return;
      int num = -1;
      BehaviorManager.BehaviorTree behaviorTree = this.behaviorTreeMap[behavior];
      for (int index = 0; index < behaviorTree.taskList.Count; ++index)
      {
        if (behaviorTree.taskList[index].ReferenceID == task.ReferenceID)
        {
          num = index;
          break;
        }
      }
      if (num <= -1)
        return;
      for (int index1 = 0; index1 < behaviorTree.activeStack.Count; ++index1)
      {
        if (behaviorTree.activeStack[index1].Count > 0)
        {
          for (int index2 = behaviorTree.activeStack[index1].Peek(); index2 != -1; index2 = behaviorTree.parentIndex[index2])
          {
            if (index2 == num)
            {
              behaviorTree.interruptionIndex[index1] = num;
              if (behavior.LogTaskChanges)
                Debug.Log((object) string.Format("{0}: {1}: Interrupt task {2} ({3}) with index {4} at stack index {5}", (object) this.RoundedTime(), (object) behaviorTree.behavior.ToString(), (object) task.FriendlyName, (object) task.GetType().ToString(), (object) num, (object) index1));
              interruptionTask.NodeData.InterruptTime = Time.get_realtimeSinceStartup();
              break;
            }
          }
        }
      }
    }

    public void StopThirdPartyTask(BehaviorManager.BehaviorTree behaviorTree, int taskIndex)
    {
      this.thirdPartyTaskCompare.Task = behaviorTree.taskList[taskIndex];
      object index;
      if (!this.taskObjectMap.TryGetValue(this.thirdPartyTaskCompare, out index))
        return;
      BehaviorManager.ThirdPartyObjectType thirdPartyObjectType = this.objectTaskMap[index].ThirdPartyObjectType;
      if (BehaviorManager.invokeParameters == null)
        BehaviorManager.invokeParameters = new object[1];
      BehaviorManager.invokeParameters[0] = (object) behaviorTree.taskList[taskIndex];
      switch (thirdPartyObjectType)
      {
        case BehaviorManager.ThirdPartyObjectType.PlayMaker:
          BehaviorManager.PlayMakerStopMethod.Invoke((object) null, BehaviorManager.invokeParameters);
          break;
        case BehaviorManager.ThirdPartyObjectType.uScript:
          BehaviorManager.UScriptStopMethod.Invoke((object) null, BehaviorManager.invokeParameters);
          break;
        case BehaviorManager.ThirdPartyObjectType.DialogueSystem:
          BehaviorManager.DialogueSystemStopMethod.Invoke((object) null, BehaviorManager.invokeParameters);
          break;
        case BehaviorManager.ThirdPartyObjectType.uSequencer:
          BehaviorManager.USequencerStopMethod.Invoke((object) null, BehaviorManager.invokeParameters);
          break;
        case BehaviorManager.ThirdPartyObjectType.ICode:
          BehaviorManager.ICodeStopMethod.Invoke((object) null, BehaviorManager.invokeParameters);
          break;
      }
      this.RemoveActiveThirdPartyTask(behaviorTree.taskList[taskIndex]);
    }

    public void RemoveActiveThirdPartyTask(Task task)
    {
      this.thirdPartyTaskCompare.Task = task;
      object key;
      if (!this.taskObjectMap.TryGetValue(this.thirdPartyTaskCompare, out key))
        return;
      ObjectPool.Return<object>(key);
      this.taskObjectMap.Remove(this.thirdPartyTaskCompare);
      this.objectTaskMap.Remove(key);
    }

    private void RemoveStack(BehaviorManager.BehaviorTree behaviorTree, int stackIndex)
    {
      Stack<int> active = behaviorTree.activeStack[stackIndex];
      active.Clear();
      ObjectPool.Return<Stack<int>>(active);
      behaviorTree.activeStack.RemoveAt(stackIndex);
      behaviorTree.interruptionIndex.RemoveAt(stackIndex);
      behaviorTree.nonInstantTaskStatus.RemoveAt(stackIndex);
    }

    private int FindLCA(BehaviorManager.BehaviorTree behaviorTree, int taskIndex1, int taskIndex2)
    {
      HashSet<int> intSet = ObjectPool.Get<HashSet<int>>();
      intSet.Clear();
      for (int index = taskIndex1; index != -1; index = behaviorTree.parentIndex[index])
        intSet.Add(index);
      int index1 = taskIndex2;
      while (!intSet.Contains(index1))
        index1 = behaviorTree.parentIndex[index1];
      ObjectPool.Return<HashSet<int>>(intSet);
      return index1;
    }

    private bool IsChild(BehaviorManager.BehaviorTree behaviorTree, int taskIndex1, int taskIndex2)
    {
      for (int index = taskIndex1; index != -1; index = behaviorTree.parentIndex[index])
      {
        if (index == taskIndex2)
          return true;
      }
      return false;
    }

    public List<Task> GetActiveTasks(Behavior behavior)
    {
      if (!this.IsBehaviorEnabled(behavior))
        return (List<Task>) null;
      List<Task> taskList = new List<Task>();
      BehaviorManager.BehaviorTree behaviorTree = this.behaviorTreeMap[behavior];
      for (int index = 0; index < behaviorTree.activeStack.Count; ++index)
      {
        Task task = behaviorTree.taskList[behaviorTree.activeStack[index].Peek()];
        if (task is BehaviorDesigner.Runtime.Tasks.Action)
          taskList.Add(task);
      }
      return taskList;
    }

    public void BehaviorOnCollisionEnter(Collision collision, Behavior behavior)
    {
      if (!this.IsBehaviorEnabled(behavior))
        return;
      BehaviorManager.BehaviorTree behaviorTree = this.behaviorTreeMap[behavior];
      for (int index1 = 0; index1 < behaviorTree.activeStack.Count; ++index1)
      {
        if (behaviorTree.activeStack[index1].Count != 0)
        {
          for (int index2 = behaviorTree.activeStack[index1].Peek(); index2 != -1 && !behaviorTree.taskList[index2].Disabled; index2 = behaviorTree.parentIndex[index2])
            behaviorTree.taskList[index2].OnCollisionEnter(collision);
        }
      }
      for (int index1 = 0; index1 < behaviorTree.conditionalReevaluate.Count; ++index1)
      {
        int index2 = behaviorTree.conditionalReevaluate[index1].index;
        if (!behaviorTree.taskList[index2].Disabled && behaviorTree.conditionalReevaluate[index1].compositeIndex != -1)
          behaviorTree.taskList[index2].OnCollisionEnter(collision);
      }
    }

    public void BehaviorOnCollisionExit(Collision collision, Behavior behavior)
    {
      if (!this.IsBehaviorEnabled(behavior))
        return;
      BehaviorManager.BehaviorTree behaviorTree = this.behaviorTreeMap[behavior];
      for (int index1 = 0; index1 < behaviorTree.activeStack.Count; ++index1)
      {
        if (behaviorTree.activeStack[index1].Count != 0)
        {
          for (int index2 = behaviorTree.activeStack[index1].Peek(); index2 != -1 && !behaviorTree.taskList[index2].Disabled; index2 = behaviorTree.parentIndex[index2])
            behaviorTree.taskList[index2].OnCollisionExit(collision);
        }
      }
      for (int index1 = 0; index1 < behaviorTree.conditionalReevaluate.Count; ++index1)
      {
        int index2 = behaviorTree.conditionalReevaluate[index1].index;
        if (!behaviorTree.taskList[index2].Disabled && behaviorTree.conditionalReevaluate[index1].compositeIndex != -1)
          behaviorTree.taskList[index2].OnCollisionExit(collision);
      }
    }

    public void BehaviorOnTriggerEnter(Collider other, Behavior behavior)
    {
      if (!this.IsBehaviorEnabled(behavior))
        return;
      BehaviorManager.BehaviorTree behaviorTree = this.behaviorTreeMap[behavior];
      for (int index1 = 0; index1 < behaviorTree.activeStack.Count; ++index1)
      {
        if (behaviorTree.activeStack[index1].Count != 0)
        {
          for (int index2 = behaviorTree.activeStack[index1].Peek(); index2 != -1 && !behaviorTree.taskList[index2].Disabled; index2 = behaviorTree.parentIndex[index2])
            behaviorTree.taskList[index2].OnTriggerEnter(other);
        }
      }
      for (int index1 = 0; index1 < behaviorTree.conditionalReevaluate.Count; ++index1)
      {
        int index2 = behaviorTree.conditionalReevaluate[index1].index;
        if (!behaviorTree.taskList[index2].Disabled && behaviorTree.conditionalReevaluate[index1].compositeIndex != -1)
          behaviorTree.taskList[index2].OnTriggerEnter(other);
      }
    }

    public void BehaviorOnTriggerExit(Collider other, Behavior behavior)
    {
      if (!this.IsBehaviorEnabled(behavior))
        return;
      BehaviorManager.BehaviorTree behaviorTree = this.behaviorTreeMap[behavior];
      for (int index1 = 0; index1 < behaviorTree.activeStack.Count; ++index1)
      {
        if (behaviorTree.activeStack[index1].Count != 0)
        {
          for (int index2 = behaviorTree.activeStack[index1].Peek(); index2 != -1 && !behaviorTree.taskList[index2].Disabled; index2 = behaviorTree.parentIndex[index2])
            behaviorTree.taskList[index2].OnTriggerExit(other);
        }
      }
      for (int index1 = 0; index1 < behaviorTree.conditionalReevaluate.Count; ++index1)
      {
        int index2 = behaviorTree.conditionalReevaluate[index1].index;
        if (!behaviorTree.taskList[index2].Disabled && behaviorTree.conditionalReevaluate[index1].compositeIndex != -1)
          behaviorTree.taskList[index2].OnTriggerExit(other);
      }
    }

    public void BehaviorOnCollisionEnter2D(Collision2D collision, Behavior behavior)
    {
      if (!this.IsBehaviorEnabled(behavior))
        return;
      BehaviorManager.BehaviorTree behaviorTree = this.behaviorTreeMap[behavior];
      for (int index1 = 0; index1 < behaviorTree.activeStack.Count; ++index1)
      {
        if (behaviorTree.activeStack[index1].Count != 0)
        {
          for (int index2 = behaviorTree.activeStack[index1].Peek(); index2 != -1 && !behaviorTree.taskList[index2].Disabled; index2 = behaviorTree.parentIndex[index2])
            behaviorTree.taskList[index2].OnCollisionEnter2D(collision);
        }
      }
      for (int index1 = 0; index1 < behaviorTree.conditionalReevaluate.Count; ++index1)
      {
        int index2 = behaviorTree.conditionalReevaluate[index1].index;
        if (!behaviorTree.taskList[index2].Disabled && behaviorTree.conditionalReevaluate[index1].compositeIndex != -1)
          behaviorTree.taskList[index2].OnCollisionEnter2D(collision);
      }
    }

    public void BehaviorOnCollisionExit2D(Collision2D collision, Behavior behavior)
    {
      if (!this.IsBehaviorEnabled(behavior))
        return;
      BehaviorManager.BehaviorTree behaviorTree = this.behaviorTreeMap[behavior];
      for (int index1 = 0; index1 < behaviorTree.activeStack.Count; ++index1)
      {
        if (behaviorTree.activeStack[index1].Count != 0)
        {
          for (int index2 = behaviorTree.activeStack[index1].Peek(); index2 != -1 && !behaviorTree.taskList[index2].Disabled; index2 = behaviorTree.parentIndex[index2])
            behaviorTree.taskList[index2].OnCollisionExit2D(collision);
        }
      }
      for (int index1 = 0; index1 < behaviorTree.conditionalReevaluate.Count; ++index1)
      {
        int index2 = behaviorTree.conditionalReevaluate[index1].index;
        if (!behaviorTree.taskList[index2].Disabled && behaviorTree.conditionalReevaluate[index1].compositeIndex != -1)
          behaviorTree.taskList[index2].OnCollisionExit2D(collision);
      }
    }

    public void BehaviorOnTriggerEnter2D(Collider2D other, Behavior behavior)
    {
      if (!this.IsBehaviorEnabled(behavior))
        return;
      BehaviorManager.BehaviorTree behaviorTree = this.behaviorTreeMap[behavior];
      for (int index1 = 0; index1 < behaviorTree.activeStack.Count; ++index1)
      {
        if (behaviorTree.activeStack[index1].Count != 0)
        {
          for (int index2 = behaviorTree.activeStack[index1].Peek(); index2 != -1 && !behaviorTree.taskList[index2].Disabled; index2 = behaviorTree.parentIndex[index2])
            behaviorTree.taskList[index2].OnTriggerEnter2D(other);
        }
      }
      for (int index1 = 0; index1 < behaviorTree.conditionalReevaluate.Count; ++index1)
      {
        int index2 = behaviorTree.conditionalReevaluate[index1].index;
        if (!behaviorTree.taskList[index2].Disabled && behaviorTree.conditionalReevaluate[index1].compositeIndex != -1)
          behaviorTree.taskList[index2].OnTriggerEnter2D(other);
      }
    }

    public void BehaviorOnTriggerExit2D(Collider2D other, Behavior behavior)
    {
      if (!this.IsBehaviorEnabled(behavior))
        return;
      BehaviorManager.BehaviorTree behaviorTree = this.behaviorTreeMap[behavior];
      for (int index1 = 0; index1 < behaviorTree.activeStack.Count; ++index1)
      {
        if (behaviorTree.activeStack[index1].Count != 0)
        {
          for (int index2 = behaviorTree.activeStack[index1].Peek(); index2 != -1 && !behaviorTree.taskList[index2].Disabled; index2 = behaviorTree.parentIndex[index2])
            behaviorTree.taskList[index2].OnTriggerExit2D(other);
        }
      }
      for (int index1 = 0; index1 < behaviorTree.conditionalReevaluate.Count; ++index1)
      {
        int index2 = behaviorTree.conditionalReevaluate[index1].index;
        if (!behaviorTree.taskList[index2].Disabled && behaviorTree.conditionalReevaluate[index1].compositeIndex != -1)
          behaviorTree.taskList[index2].OnTriggerExit2D(other);
      }
    }

    public void BehaviorOnControllerColliderHit(ControllerColliderHit hit, Behavior behavior)
    {
      if (!this.IsBehaviorEnabled(behavior))
        return;
      BehaviorManager.BehaviorTree behaviorTree = this.behaviorTreeMap[behavior];
      for (int index1 = 0; index1 < behaviorTree.activeStack.Count; ++index1)
      {
        if (behaviorTree.activeStack[index1].Count != 0)
        {
          for (int index2 = behaviorTree.activeStack[index1].Peek(); index2 != -1 && !behaviorTree.taskList[index2].Disabled; index2 = behaviorTree.parentIndex[index2])
            behaviorTree.taskList[index2].OnControllerColliderHit(hit);
        }
      }
      for (int index1 = 0; index1 < behaviorTree.conditionalReevaluate.Count; ++index1)
      {
        int index2 = behaviorTree.conditionalReevaluate[index1].index;
        if (!behaviorTree.taskList[index2].Disabled && behaviorTree.conditionalReevaluate[index1].compositeIndex != -1)
          behaviorTree.taskList[index2].OnControllerColliderHit(hit);
      }
    }

    public void BehaviorOnAnimatorIK(Behavior behavior)
    {
      if (!this.IsBehaviorEnabled(behavior))
        return;
      BehaviorManager.BehaviorTree behaviorTree = this.behaviorTreeMap[behavior];
      for (int index1 = 0; index1 < behaviorTree.activeStack.Count; ++index1)
      {
        if (behaviorTree.activeStack[index1].Count != 0)
        {
          for (int index2 = behaviorTree.activeStack[index1].Peek(); index2 != -1 && !behaviorTree.taskList[index2].Disabled; index2 = behaviorTree.parentIndex[index2])
            behaviorTree.taskList[index2].OnAnimatorIK();
        }
      }
      for (int index1 = 0; index1 < behaviorTree.conditionalReevaluate.Count; ++index1)
      {
        int index2 = behaviorTree.conditionalReevaluate[index1].index;
        if (!behaviorTree.taskList[index2].Disabled && behaviorTree.conditionalReevaluate[index1].compositeIndex != -1)
          behaviorTree.taskList[index2].OnAnimatorIK();
      }
    }

    public bool MapObjectToTask(
      object objectKey,
      Task task,
      BehaviorManager.ThirdPartyObjectType objectType)
    {
      if (this.objectTaskMap.ContainsKey(objectKey))
      {
        string str = string.Empty;
        switch (objectType)
        {
          case BehaviorManager.ThirdPartyObjectType.PlayMaker:
            str = "PlayMaker FSM";
            break;
          case BehaviorManager.ThirdPartyObjectType.uScript:
            str = "uScript Graph";
            break;
          case BehaviorManager.ThirdPartyObjectType.DialogueSystem:
            str = "Dialogue System";
            break;
          case BehaviorManager.ThirdPartyObjectType.uSequencer:
            str = "uSequencer sequence";
            break;
          case BehaviorManager.ThirdPartyObjectType.ICode:
            str = "ICode state machine";
            break;
        }
        Debug.LogError((object) string.Format("Only one behavior can be mapped to the same instance of the {0}.", (object) str));
        return false;
      }
      BehaviorManager.ThirdPartyTask key = ObjectPool.Get<BehaviorManager.ThirdPartyTask>();
      key.Initialize(task, objectType);
      this.objectTaskMap.Add(objectKey, key);
      this.taskObjectMap.Add(key, objectKey);
      return true;
    }

    public Task TaskForObject(object objectKey)
    {
      BehaviorManager.ThirdPartyTask thirdPartyTask;
      if (!this.objectTaskMap.TryGetValue(objectKey, out thirdPartyTask))
        return (Task) null;
      return thirdPartyTask.Task;
    }

    private Decimal RoundedTime()
    {
      return Math.Round((Decimal) Time.get_time(), 5, MidpointRounding.AwayFromZero);
    }

    public List<Task> GetTaskList(Behavior behavior)
    {
      if (!this.IsBehaviorEnabled(behavior))
        return (List<Task>) null;
      return this.behaviorTreeMap[behavior].taskList;
    }

    public enum ExecutionsPerTickType
    {
      NoDuplicates,
      Count,
    }

    public class BehaviorTree
    {
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
        for (int index = this.childrenIndex.Count - 1; index > -1; --index)
          ObjectPool.Return<List<int>>(this.childrenIndex[index]);
        for (int index = this.activeStack.Count - 1; index > -1; --index)
          ObjectPool.Return<Stack<int>>(this.activeStack[index]);
        for (int index = this.childConditionalIndex.Count - 1; index > -1; --index)
          ObjectPool.Return<List<int>>(this.childConditionalIndex[index]);
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

      public class ConditionalReevaluate
      {
        public int compositeIndex = -1;
        public int stackIndex = -1;
        public int index;
        public TaskStatus taskStatus;

        public void Initialize(int i, TaskStatus status, int stack, int composite)
        {
          this.index = i;
          this.taskStatus = status;
          this.stackIndex = stack;
          this.compositeIndex = composite;
        }
      }
    }

    public enum ThirdPartyObjectType
    {
      PlayMaker,
      uScript,
      DialogueSystem,
      uSequencer,
      ICode,
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
        if (object.ReferenceEquals((object) a, (object) null) || object.ReferenceEquals((object) b, (object) null))
          return false;
        return a.Task.Equals((object) b.Task);
      }

      public int GetHashCode(BehaviorManager.ThirdPartyTask obj)
      {
        if (obj != null)
          return obj.Task.GetHashCode();
        return 0;
      }
    }

    public class TaskAddData
    {
      public int parentIndex = -1;
      public int compositeParentIndex = -1;
      public HashSet<object> overiddenFields = new HashSet<object>();
      public int errorTask = -1;
      public string errorTaskName = string.Empty;
      public bool fromExternalTask;
      public ParentTask parentTask;
      public int depth;
      public Vector2 offset;
      public Dictionary<string, BehaviorManager.TaskAddData.OverrideFieldValue> overrideFields;

      public void Initialize()
      {
        if (this.overrideFields != null)
        {
          using (Dictionary<string, BehaviorManager.TaskAddData.OverrideFieldValue>.Enumerator enumerator = this.overrideFields.GetEnumerator())
          {
            while (enumerator.MoveNext())
              ObjectPool.Return<KeyValuePair<string, BehaviorManager.TaskAddData.OverrideFieldValue>>(enumerator.Current);
          }
        }
        ObjectPool.Return<Dictionary<string, BehaviorManager.TaskAddData.OverrideFieldValue>>(this.overrideFields);
        this.fromExternalTask = false;
        this.parentTask = (ParentTask) null;
        this.parentIndex = -1;
        this.depth = 0;
        this.compositeParentIndex = -1;
        this.overrideFields = (Dictionary<string, BehaviorManager.TaskAddData.OverrideFieldValue>) null;
      }

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
    }

    public delegate void BehaviorManagerHandler();
  }
}
