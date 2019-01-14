// Decompiled with JetBrains decompiler
// Type: BehaviorDesigner.Runtime.Behavior
// Assembly: BehaviorDesignerRuntime, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 4E52B3C7-423D-4A3D-958C-02E4666F0F16
// Assembly location: C:\Users\Ron\Desktop\Runtime\BehaviorDesignerRuntime.dll

using BehaviorDesigner.Runtime.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace BehaviorDesigner.Runtime
{
  [Serializable]
  public abstract class Behavior : MonoBehaviour, IBehavior
  {
    [SerializeField]
    private bool startWhenEnabled;
    [SerializeField]
    private bool pauseWhenDisabled;
    [SerializeField]
    private bool restartWhenComplete;
    [SerializeField]
    private bool logTaskChanges;
    [SerializeField]
    private int group;
    [SerializeField]
    private bool resetValuesOnRestart;
    [SerializeField]
    private ExternalBehavior externalBehavior;
    private bool hasInheritedVariables;
    [SerializeField]
    private BehaviorSource mBehaviorSource;
    private bool isPaused;
    private TaskStatus executionStatus;
    private bool initialized;
    private Dictionary<Task, Dictionary<string, object>> defaultValues;
    private Dictionary<string, object> defaultVariableValues;
    private bool[] hasEvent;
    private Dictionary<string, List<TaskCoroutine>> activeTaskCoroutines;
    private Dictionary<Type, Dictionary<string, Delegate>> eventTable;
    [NonSerialized]
    public Behavior.GizmoViewMode gizmoViewMode;
    [NonSerialized]
    public bool showBehaviorDesignerGizmo;

    public Behavior()
    {
      base.\u002Ector();
      this.mBehaviorSource = new BehaviorSource((IBehavior) this);
    }

    public event Behavior.BehaviorHandler OnBehaviorStart;

    public event Behavior.BehaviorHandler OnBehaviorRestart;

    public event Behavior.BehaviorHandler OnBehaviorEnd;

    public bool StartWhenEnabled
    {
      get
      {
        return this.startWhenEnabled;
      }
      set
      {
        this.startWhenEnabled = value;
      }
    }

    public bool PauseWhenDisabled
    {
      get
      {
        return this.pauseWhenDisabled;
      }
      set
      {
        this.pauseWhenDisabled = value;
      }
    }

    public bool RestartWhenComplete
    {
      get
      {
        return this.restartWhenComplete;
      }
      set
      {
        this.restartWhenComplete = value;
      }
    }

    public bool LogTaskChanges
    {
      get
      {
        return this.logTaskChanges;
      }
      set
      {
        this.logTaskChanges = value;
      }
    }

    public int Group
    {
      get
      {
        return this.group;
      }
      set
      {
        this.group = value;
      }
    }

    public bool ResetValuesOnRestart
    {
      get
      {
        return this.resetValuesOnRestart;
      }
      set
      {
        this.resetValuesOnRestart = value;
      }
    }

    public ExternalBehavior ExternalBehavior
    {
      get
      {
        return this.externalBehavior;
      }
      set
      {
        if (Object.op_Inequality((Object) BehaviorManager.instance, (Object) null))
          BehaviorManager.instance.DisableBehavior(this);
        if (value.Initialized)
        {
          List<SharedVariable> allVariables = this.mBehaviorSource.GetAllVariables();
          this.mBehaviorSource = value.BehaviorSource;
          this.mBehaviorSource.HasSerialized = true;
          if (allVariables != null)
          {
            for (int index = 0; index < allVariables.Count; ++index)
            {
              if (allVariables[index] != null)
                this.mBehaviorSource.SetVariable(allVariables[index].Name, allVariables[index]);
            }
          }
        }
        else
          this.mBehaviorSource.HasSerialized = false;
        this.initialized = false;
        this.externalBehavior = value;
        if (!this.startWhenEnabled)
          return;
        this.EnableBehavior();
      }
    }

    public bool HasInheritedVariables
    {
      get
      {
        return this.hasInheritedVariables;
      }
      set
      {
        this.hasInheritedVariables = value;
      }
    }

    public string BehaviorName
    {
      get
      {
        return this.mBehaviorSource.behaviorName;
      }
      set
      {
        this.mBehaviorSource.behaviorName = value;
      }
    }

    public string BehaviorDescription
    {
      get
      {
        return this.mBehaviorSource.behaviorDescription;
      }
      set
      {
        this.mBehaviorSource.behaviorDescription = value;
      }
    }

    public BehaviorSource GetBehaviorSource()
    {
      return this.mBehaviorSource;
    }

    public void SetBehaviorSource(BehaviorSource behaviorSource)
    {
      this.mBehaviorSource = behaviorSource;
    }

    public Object GetObject()
    {
      return (Object) this;
    }

    public string GetOwnerName()
    {
      return ((Object) ((Component) this).get_gameObject()).get_name();
    }

    public TaskStatus ExecutionStatus
    {
      get
      {
        return this.executionStatus;
      }
      set
      {
        this.executionStatus = value;
      }
    }

    public bool[] HasEvent
    {
      get
      {
        return this.hasEvent;
      }
    }

    public void Start()
    {
      if (!this.startWhenEnabled)
        return;
      this.EnableBehavior();
    }

    private bool TaskContainsMethod(string methodName, Task task)
    {
      if (task == null)
        return false;
      MethodInfo method = task.GetType().GetMethod(methodName, BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
      if (method != null && method.DeclaringType.IsAssignableFrom(task.GetType()))
        return true;
      if (task is ParentTask)
      {
        ParentTask parentTask = task as ParentTask;
        if (parentTask.Children != null)
        {
          for (int index = 0; index < parentTask.Children.Count; ++index)
          {
            if (this.TaskContainsMethod(methodName, parentTask.Children[index]))
              return true;
          }
        }
      }
      return false;
    }

    public void EnableBehavior()
    {
      Behavior.CreateBehaviorManager();
      if (Object.op_Inequality((Object) BehaviorManager.instance, (Object) null))
        BehaviorManager.instance.EnableBehavior(this);
      if (this.initialized)
        return;
      for (int index = 0; index < 12; ++index)
        this.hasEvent[index] = this.TaskContainsMethod(((Behavior.EventTypes) index).ToString(), this.mBehaviorSource.RootTask);
      this.initialized = true;
    }

    public void DisableBehavior()
    {
      if (!Object.op_Inequality((Object) BehaviorManager.instance, (Object) null))
        return;
      BehaviorManager.instance.DisableBehavior(this, this.pauseWhenDisabled);
      this.isPaused = this.pauseWhenDisabled;
    }

    public void DisableBehavior(bool pause)
    {
      if (!Object.op_Inequality((Object) BehaviorManager.instance, (Object) null))
        return;
      BehaviorManager.instance.DisableBehavior(this, pause);
      this.isPaused = pause;
    }

    public void OnEnable()
    {
      if (Object.op_Inequality((Object) BehaviorManager.instance, (Object) null) && this.isPaused)
      {
        BehaviorManager.instance.EnableBehavior(this);
        this.isPaused = false;
      }
      else
      {
        if (!this.startWhenEnabled || !this.initialized)
          return;
        this.EnableBehavior();
      }
    }

    public void OnDisable()
    {
      this.DisableBehavior();
    }

    public void OnDestroy()
    {
      if (!Object.op_Inequality((Object) BehaviorManager.instance, (Object) null))
        return;
      BehaviorManager.instance.DestroyBehavior(this);
    }

    public SharedVariable GetVariable(string name)
    {
      this.CheckForSerialization();
      return this.mBehaviorSource.GetVariable(name);
    }

    public void SetVariable(string name, SharedVariable item)
    {
      this.CheckForSerialization();
      this.mBehaviorSource.SetVariable(name, item);
    }

    public void SetVariableValue(string name, object value)
    {
      SharedVariable variable = this.GetVariable(name);
      if (variable != null)
      {
        if (value is SharedVariable)
        {
          SharedVariable sharedVariable = value as SharedVariable;
          if (!string.IsNullOrEmpty(sharedVariable.PropertyMapping))
          {
            variable.PropertyMapping = sharedVariable.PropertyMapping;
            variable.PropertyMappingOwner = sharedVariable.PropertyMappingOwner;
            variable.InitializePropertyMapping(this.mBehaviorSource);
          }
          else
            variable.SetValue(sharedVariable.GetValue());
        }
        else
          variable.SetValue(value);
        variable.ValueChanged();
      }
      else if (value is SharedVariable)
      {
        SharedVariable sharedVariable = value as SharedVariable;
        SharedVariable instance = TaskUtility.CreateInstance(sharedVariable.GetType()) as SharedVariable;
        instance.Name = sharedVariable.Name;
        instance.IsShared = sharedVariable.IsShared;
        instance.IsGlobal = sharedVariable.IsGlobal;
        if (!string.IsNullOrEmpty(sharedVariable.PropertyMapping))
        {
          instance.PropertyMapping = sharedVariable.PropertyMapping;
          instance.PropertyMappingOwner = sharedVariable.PropertyMappingOwner;
          instance.InitializePropertyMapping(this.mBehaviorSource);
        }
        else
          instance.SetValue(sharedVariable.GetValue());
        this.mBehaviorSource.SetVariable(name, instance);
      }
      else
        Debug.LogError((object) ("Error: No variable exists with name " + name));
    }

    public List<SharedVariable> GetAllVariables()
    {
      this.CheckForSerialization();
      return this.mBehaviorSource.GetAllVariables();
    }

    public void CheckForSerialization()
    {
      if (Object.op_Inequality((Object) this.externalBehavior, (Object) null))
      {
        List<SharedVariable> sharedVariableList = (List<SharedVariable>) null;
        bool force = false;
        if (!this.hasInheritedVariables && !this.externalBehavior.Initialized)
        {
          this.mBehaviorSource.CheckForSerialization(false, (BehaviorSource) null);
          sharedVariableList = this.mBehaviorSource.GetAllVariables();
          this.hasInheritedVariables = true;
          force = true;
        }
        this.externalBehavior.BehaviorSource.Owner = (IBehavior) this.ExternalBehavior;
        this.externalBehavior.BehaviorSource.CheckForSerialization(force, this.GetBehaviorSource());
        this.externalBehavior.BehaviorSource.EntryTask = this.mBehaviorSource.EntryTask;
        if (sharedVariableList == null)
          return;
        for (int index = 0; index < sharedVariableList.Count; ++index)
        {
          if (sharedVariableList[index] != null)
            this.mBehaviorSource.SetVariable(sharedVariableList[index].Name, sharedVariableList[index]);
        }
      }
      else
        this.mBehaviorSource.CheckForSerialization(false, (BehaviorSource) null);
    }

    public void OnCollisionEnter(Collision collision)
    {
      if (!this.hasEvent[0] || !Object.op_Inequality((Object) BehaviorManager.instance, (Object) null))
        return;
      BehaviorManager.instance.BehaviorOnCollisionEnter(collision, this);
    }

    public void OnCollisionExit(Collision collision)
    {
      if (!this.hasEvent[1] || !Object.op_Inequality((Object) BehaviorManager.instance, (Object) null))
        return;
      BehaviorManager.instance.BehaviorOnCollisionExit(collision, this);
    }

    public void OnTriggerEnter(Collider other)
    {
      if (!this.hasEvent[2] || !Object.op_Inequality((Object) BehaviorManager.instance, (Object) null))
        return;
      BehaviorManager.instance.BehaviorOnTriggerEnter(other, this);
    }

    public void OnTriggerExit(Collider other)
    {
      if (!this.hasEvent[3] || !Object.op_Inequality((Object) BehaviorManager.instance, (Object) null))
        return;
      BehaviorManager.instance.BehaviorOnTriggerExit(other, this);
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
      if (!this.hasEvent[4] || !Object.op_Inequality((Object) BehaviorManager.instance, (Object) null))
        return;
      BehaviorManager.instance.BehaviorOnCollisionEnter2D(collision, this);
    }

    public void OnCollisionExit2D(Collision2D collision)
    {
      if (!this.hasEvent[5] || !Object.op_Inequality((Object) BehaviorManager.instance, (Object) null))
        return;
      BehaviorManager.instance.BehaviorOnCollisionExit2D(collision, this);
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
      if (!this.hasEvent[6] || !Object.op_Inequality((Object) BehaviorManager.instance, (Object) null))
        return;
      BehaviorManager.instance.BehaviorOnTriggerEnter2D(other, this);
    }

    public void OnTriggerExit2D(Collider2D other)
    {
      if (!this.hasEvent[7] || !Object.op_Inequality((Object) BehaviorManager.instance, (Object) null))
        return;
      BehaviorManager.instance.BehaviorOnTriggerExit2D(other, this);
    }

    public void OnControllerColliderHit(ControllerColliderHit hit)
    {
      if (!this.hasEvent[8] || !Object.op_Inequality((Object) BehaviorManager.instance, (Object) null))
        return;
      BehaviorManager.instance.BehaviorOnControllerColliderHit(hit, this);
    }

    public void OnAnimatorIK()
    {
      if (!this.hasEvent[11] || !Object.op_Inequality((Object) BehaviorManager.instance, (Object) null))
        return;
      BehaviorManager.instance.BehaviorOnAnimatorIK(this);
    }

    public void OnDrawGizmos()
    {
      this.DrawTaskGizmos(false);
    }

    public void OnDrawGizmosSelected()
    {
      if (this.showBehaviorDesignerGizmo)
        Gizmos.DrawIcon(((Component) this).get_transform().get_position(), "Behavior Designer Scene Icon.png");
      this.DrawTaskGizmos(true);
    }

    private void DrawTaskGizmos(bool selected)
    {
      if (this.gizmoViewMode == Behavior.GizmoViewMode.Never || this.gizmoViewMode == Behavior.GizmoViewMode.Selected && !selected || this.gizmoViewMode != Behavior.GizmoViewMode.Running && this.gizmoViewMode != Behavior.GizmoViewMode.Always && (!Application.get_isPlaying() || this.ExecutionStatus != TaskStatus.Running) && Application.get_isPlaying())
        return;
      this.CheckForSerialization();
      this.DrawTaskGizmos(this.mBehaviorSource.RootTask);
      List<Task> detachedTasks = this.mBehaviorSource.DetachedTasks;
      if (detachedTasks == null)
        return;
      for (int index = 0; index < detachedTasks.Count; ++index)
        this.DrawTaskGizmos(detachedTasks[index]);
    }

    private void DrawTaskGizmos(Task task)
    {
      if (task == null || this.gizmoViewMode == Behavior.GizmoViewMode.Running && !task.NodeData.IsReevaluating && (task.NodeData.IsReevaluating || task.NodeData.ExecutionStatus != TaskStatus.Running))
        return;
      task.OnDrawGizmos();
      if (!(task is ParentTask))
        return;
      ParentTask parentTask = task as ParentTask;
      if (parentTask.Children == null)
        return;
      for (int index = 0; index < parentTask.Children.Count; ++index)
        this.DrawTaskGizmos(parentTask.Children[index]);
    }

    public T FindTask<T>() where T : Task
    {
      return this.FindTask<T>(this.mBehaviorSource.RootTask);
    }

    private T FindTask<T>(Task task) where T : Task
    {
      if (task.GetType().Equals(typeof (T)))
        return (T) task;
      if (task is ParentTask parentTask && parentTask.Children != null)
      {
        for (int index = 0; index < parentTask.Children.Count; ++index)
        {
          T obj = (T) null;
          T task1;
          if ((object) (task1 = this.FindTask<T>(parentTask.Children[index])) != null)
            return task1;
        }
      }
      return (T) null;
    }

    public List<T> FindTasks<T>() where T : Task
    {
      this.CheckForSerialization();
      List<T> taskList = new List<T>();
      this.FindTasks<T>(this.mBehaviorSource.RootTask, ref taskList);
      return taskList;
    }

    private void FindTasks<T>(Task task, ref List<T> taskList) where T : Task
    {
      if (typeof (T).IsAssignableFrom(task.GetType()))
        taskList.Add((T) task);
      if (!(task is ParentTask parentTask) || parentTask.Children == null)
        return;
      for (int index = 0; index < parentTask.Children.Count; ++index)
        this.FindTasks<T>(parentTask.Children[index], ref taskList);
    }

    public Task FindTaskWithName(string taskName)
    {
      this.CheckForSerialization();
      return this.FindTaskWithName(taskName, this.mBehaviorSource.RootTask);
    }

    private Task FindTaskWithName(string taskName, Task task)
    {
      if (task.FriendlyName.Equals(taskName))
        return task;
      if (task is ParentTask parentTask && parentTask.Children != null)
      {
        for (int index = 0; index < parentTask.Children.Count; ++index)
        {
          Task taskWithName;
          if ((taskWithName = this.FindTaskWithName(taskName, parentTask.Children[index])) != null)
            return taskWithName;
        }
      }
      return (Task) null;
    }

    public List<Task> FindTasksWithName(string taskName)
    {
      this.CheckForSerialization();
      List<Task> taskList = new List<Task>();
      this.FindTasksWithName(taskName, this.mBehaviorSource.RootTask, ref taskList);
      return taskList;
    }

    private void FindTasksWithName(string taskName, Task task, ref List<Task> taskList)
    {
      if (task.FriendlyName.Equals(taskName))
        taskList.Add(task);
      if (!(task is ParentTask parentTask) || parentTask.Children == null)
        return;
      for (int index = 0; index < parentTask.Children.Count; ++index)
        this.FindTasksWithName(taskName, parentTask.Children[index], ref taskList);
    }

    public List<Task> GetActiveTasks()
    {
      if (Object.op_Equality((Object) BehaviorManager.instance, (Object) null))
        return (List<Task>) null;
      return BehaviorManager.instance.GetActiveTasks(this);
    }

    public Coroutine StartTaskCoroutine(Task task, string methodName)
    {
      MethodInfo method = task.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
      if (method == null)
      {
        Debug.LogError((object) ("Unable to start coroutine " + methodName + ": method not found"));
        return (Coroutine) null;
      }
      if (this.activeTaskCoroutines == null)
        this.activeTaskCoroutines = new Dictionary<string, List<TaskCoroutine>>();
      TaskCoroutine taskCoroutine = new TaskCoroutine(this, (IEnumerator) method.Invoke((object) task, new object[0]), methodName);
      if (this.activeTaskCoroutines.ContainsKey(methodName))
      {
        List<TaskCoroutine> activeTaskCoroutine = this.activeTaskCoroutines[methodName];
        activeTaskCoroutine.Add(taskCoroutine);
        this.activeTaskCoroutines[methodName] = activeTaskCoroutine;
      }
      else
        this.activeTaskCoroutines.Add(methodName, new List<TaskCoroutine>()
        {
          taskCoroutine
        });
      return taskCoroutine.Coroutine;
    }

    public Coroutine StartTaskCoroutine(Task task, string methodName, object value)
    {
      MethodInfo method = task.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
      if (method == null)
      {
        Debug.LogError((object) ("Unable to start coroutine " + methodName + ": method not found"));
        return (Coroutine) null;
      }
      if (this.activeTaskCoroutines == null)
        this.activeTaskCoroutines = new Dictionary<string, List<TaskCoroutine>>();
      TaskCoroutine taskCoroutine = new TaskCoroutine(this, (IEnumerator) method.Invoke((object) task, new object[1]
      {
        value
      }), methodName);
      if (this.activeTaskCoroutines.ContainsKey(methodName))
      {
        List<TaskCoroutine> activeTaskCoroutine = this.activeTaskCoroutines[methodName];
        activeTaskCoroutine.Add(taskCoroutine);
        this.activeTaskCoroutines[methodName] = activeTaskCoroutine;
      }
      else
        this.activeTaskCoroutines.Add(methodName, new List<TaskCoroutine>()
        {
          taskCoroutine
        });
      return taskCoroutine.Coroutine;
    }

    public void StopTaskCoroutine(string methodName)
    {
      if (!this.activeTaskCoroutines.ContainsKey(methodName))
        return;
      List<TaskCoroutine> activeTaskCoroutine = this.activeTaskCoroutines[methodName];
      for (int index = 0; index < activeTaskCoroutine.Count; ++index)
        activeTaskCoroutine[index].Stop();
    }

    public void StopAllTaskCoroutines()
    {
      this.StopAllCoroutines();
      using (Dictionary<string, List<TaskCoroutine>>.Enumerator enumerator = this.activeTaskCoroutines.GetEnumerator())
      {
        while (enumerator.MoveNext())
        {
          List<TaskCoroutine> taskCoroutineList = enumerator.Current.Value;
          for (int index = 0; index < taskCoroutineList.Count; ++index)
            taskCoroutineList[index].Stop();
        }
      }
    }

    public void TaskCoroutineEnded(TaskCoroutine taskCoroutine, string coroutineName)
    {
      if (!this.activeTaskCoroutines.ContainsKey(coroutineName))
        return;
      List<TaskCoroutine> activeTaskCoroutine = this.activeTaskCoroutines[coroutineName];
      if (activeTaskCoroutine.Count == 1)
      {
        this.activeTaskCoroutines.Remove(coroutineName);
      }
      else
      {
        activeTaskCoroutine.Remove(taskCoroutine);
        this.activeTaskCoroutines[coroutineName] = activeTaskCoroutine;
      }
    }

    public void OnBehaviorStarted()
    {
      if (this.OnBehaviorStart == null)
        return;
      this.OnBehaviorStart(this);
    }

    public void OnBehaviorRestarted()
    {
      if (this.OnBehaviorRestart == null)
        return;
      this.OnBehaviorRestart(this);
    }

    public void OnBehaviorEnded()
    {
      if (this.OnBehaviorEnd == null)
        return;
      this.OnBehaviorEnd(this);
    }

    private void RegisterEvent(string name, Delegate handler)
    {
      if (this.eventTable == null)
        this.eventTable = new Dictionary<Type, Dictionary<string, Delegate>>();
      Dictionary<string, Delegate> dictionary;
      if (!this.eventTable.TryGetValue(handler.GetType(), out dictionary))
      {
        dictionary = new Dictionary<string, Delegate>();
        this.eventTable.Add(handler.GetType(), dictionary);
      }
      Delegate a;
      if (dictionary.TryGetValue(name, out a))
        dictionary[name] = Delegate.Combine(a, handler);
      else
        dictionary.Add(name, handler);
    }

    public void RegisterEvent(string name, System.Action handler)
    {
      this.RegisterEvent(name, (Delegate) handler);
    }

    public void RegisterEvent<T>(string name, System.Action<T> handler)
    {
      this.RegisterEvent(name, (Delegate) handler);
    }

    public void RegisterEvent<T, U>(string name, System.Action<T, U> handler)
    {
      this.RegisterEvent(name, (Delegate) handler);
    }

    public void RegisterEvent<T, U, V>(string name, System.Action<T, U, V> handler)
    {
      this.RegisterEvent(name, (Delegate) handler);
    }

    private Delegate GetDelegate(string name, Type type)
    {
      Dictionary<string, Delegate> dictionary;
      Delegate @delegate;
      if (this.eventTable != null && this.eventTable.TryGetValue(type, out dictionary) && dictionary.TryGetValue(name, out @delegate))
        return @delegate;
      return (Delegate) null;
    }

    public void SendEvent(string name)
    {
      System.Action action = this.GetDelegate(name, typeof (System.Action)) as System.Action;
      if (action == null)
        return;
      action();
    }

    public void SendEvent<T>(string name, T arg1)
    {
      System.Action<T> action = this.GetDelegate(name, typeof (System.Action<T>)) as System.Action<T>;
      if (action == null)
        return;
      action(arg1);
    }

    public void SendEvent<T, U>(string name, T arg1, U arg2)
    {
      System.Action<T, U> action = this.GetDelegate(name, typeof (System.Action<T, U>)) as System.Action<T, U>;
      if (action == null)
        return;
      action(arg1, arg2);
    }

    public void SendEvent<T, U, V>(string name, T arg1, U arg2, V arg3)
    {
      System.Action<T, U, V> action = this.GetDelegate(name, typeof (System.Action<T, U, V>)) as System.Action<T, U, V>;
      if (action == null)
        return;
      action(arg1, arg2, arg3);
    }

    private void UnregisterEvent(string name, Delegate handler)
    {
      Dictionary<string, Delegate> dictionary;
      Delegate source;
      if (this.eventTable == null || !this.eventTable.TryGetValue(handler.GetType(), out dictionary) || !dictionary.TryGetValue(name, out source))
        return;
      dictionary[name] = Delegate.Remove(source, handler);
    }

    public void UnregisterEvent(string name, System.Action handler)
    {
      this.UnregisterEvent(name, (Delegate) handler);
    }

    public void UnregisterEvent<T>(string name, System.Action<T> handler)
    {
      this.UnregisterEvent(name, (Delegate) handler);
    }

    public void UnregisterEvent<T, U>(string name, System.Action<T, U> handler)
    {
      this.UnregisterEvent(name, (Delegate) handler);
    }

    public void UnregisterEvent<T, U, V>(string name, System.Action<T, U, V> handler)
    {
      this.UnregisterEvent(name, (Delegate) handler);
    }

    public void SaveResetValues()
    {
      if (this.defaultValues == null)
      {
        this.CheckForSerialization();
        this.defaultValues = new Dictionary<Task, Dictionary<string, object>>();
        this.defaultVariableValues = new Dictionary<string, object>();
        this.SaveValues();
      }
      else
        this.ResetValues();
    }

    private void SaveValues()
    {
      List<SharedVariable> allVariables = this.mBehaviorSource.GetAllVariables();
      if (allVariables != null)
      {
        for (int index = 0; index < allVariables.Count; ++index)
          this.defaultVariableValues.Add(allVariables[index].Name, allVariables[index].GetValue());
      }
      this.SaveValue(this.mBehaviorSource.RootTask);
    }

    private void SaveValue(Task task)
    {
      if (task == null)
        return;
      FieldInfo[] publicFields = TaskUtility.GetPublicFields(task.GetType());
      Dictionary<string, object> dictionary = new Dictionary<string, object>();
      for (int index = 0; index < publicFields.Length; ++index)
      {
        object obj = publicFields[index].GetValue((object) task);
        if (obj is SharedVariable)
        {
          SharedVariable sharedVariable = obj as SharedVariable;
          if (sharedVariable.IsGlobal || sharedVariable.IsShared)
            continue;
        }
        dictionary.Add(publicFields[index].Name, publicFields[index].GetValue((object) task));
      }
      this.defaultValues.Add(task, dictionary);
      if (!(task is ParentTask))
        return;
      ParentTask parentTask = task as ParentTask;
      if (parentTask.Children == null)
        return;
      for (int index = 0; index < parentTask.Children.Count; ++index)
        this.SaveValue(parentTask.Children[index]);
    }

    private void ResetValues()
    {
      using (Dictionary<string, object>.Enumerator enumerator = this.defaultVariableValues.GetEnumerator())
      {
        while (enumerator.MoveNext())
        {
          KeyValuePair<string, object> current = enumerator.Current;
          this.SetVariableValue(current.Key, current.Value);
        }
      }
      this.ResetValue(this.mBehaviorSource.RootTask);
    }

    private void ResetValue(Task task)
    {
      Dictionary<string, object> dictionary;
      if (task == null || !this.defaultValues.TryGetValue(task, out dictionary))
        return;
      using (Dictionary<string, object>.Enumerator enumerator = dictionary.GetEnumerator())
      {
        while (enumerator.MoveNext())
        {
          KeyValuePair<string, object> current = enumerator.Current;
          task.GetType().GetField(current.Key)?.SetValue((object) task, current.Value);
        }
      }
      if (!(task is ParentTask))
        return;
      ParentTask parentTask = task as ParentTask;
      if (parentTask.Children == null)
        return;
      for (int index = 0; index < parentTask.Children.Count; ++index)
        this.ResetValue(parentTask.Children[index]);
    }

    public virtual string ToString()
    {
      return this.mBehaviorSource.ToString();
    }

    public static BehaviorManager CreateBehaviorManager()
    {
      if (!Object.op_Equality((Object) BehaviorManager.instance, (Object) null) || !Application.get_isPlaying())
        return (BehaviorManager) null;
      GameObject gameObject = new GameObject();
      ((Object) gameObject).set_name("Behavior Manager");
      return (BehaviorManager) gameObject.AddComponent<BehaviorManager>();
    }

    int IBehavior.GetInstanceID()
    {
      return ((Object) this).GetInstanceID();
    }

    public enum EventTypes
    {
      OnCollisionEnter,
      OnCollisionExit,
      OnTriggerEnter,
      OnTriggerExit,
      OnCollisionEnter2D,
      OnCollisionExit2D,
      OnTriggerEnter2D,
      OnTriggerExit2D,
      OnControllerColliderHit,
      OnLateUpdate,
      OnFixedUpdate,
      OnAnimatorIK,
      None,
    }

    public enum GizmoViewMode
    {
      Running,
      Always,
      Selected,
      Never,
    }

    public delegate void BehaviorHandler(Behavior behavior);
  }
}
