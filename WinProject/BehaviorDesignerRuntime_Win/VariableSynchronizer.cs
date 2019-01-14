// Decompiled with JetBrains decompiler
// Type: BehaviorDesigner.Runtime.VariableSynchronizer
// Assembly: BehaviorDesignerRuntime, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 4E52B3C7-423D-4A3D-958C-02E4666F0F16
// Assembly location: C:\Users\Ron\Desktop\Runtime\BehaviorDesignerRuntime.dll

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;

namespace BehaviorDesigner.Runtime
{
  [AddComponentMenu("Behavior Designer/Variable Synchronizer")]
  public class VariableSynchronizer : MonoBehaviour
  {
    [SerializeField]
    private UpdateIntervalType updateInterval;
    [SerializeField]
    private float updateIntervalSeconds;
    private WaitForSeconds updateWait;
    [SerializeField]
    private List<VariableSynchronizer.SynchronizedVariable> synchronizedVariables;

    public VariableSynchronizer()
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

    public List<VariableSynchronizer.SynchronizedVariable> SynchronizedVariables
    {
      get
      {
        return this.synchronizedVariables;
      }
      set
      {
        this.synchronizedVariables = value;
        ((Behaviour) this).set_enabled(true);
      }
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

    public void Awake()
    {
      for (int index = this.synchronizedVariables.Count - 1; index > -1; --index)
      {
        VariableSynchronizer.SynchronizedVariable synchronizedVariable = this.synchronizedVariables[index];
        synchronizedVariable.sharedVariable = !synchronizedVariable.global ? synchronizedVariable.behavior.GetVariable(synchronizedVariable.variableName) : GlobalVariables.Instance.GetVariable(synchronizedVariable.variableName);
        string str = string.Empty;
        if (synchronizedVariable.sharedVariable == null)
        {
          str = "the SharedVariable can't be found";
        }
        else
        {
          switch (synchronizedVariable.synchronizationType)
          {
            case VariableSynchronizer.SynchronizationType.BehaviorDesigner:
              Behavior targetComponent = synchronizedVariable.targetComponent as Behavior;
              if (Object.op_Equality((Object) targetComponent, (Object) null))
              {
                str = "the target component is not of type Behavior Tree";
                break;
              }
              synchronizedVariable.targetSharedVariable = !synchronizedVariable.targetGlobal ? targetComponent.GetVariable(synchronizedVariable.targetName) : GlobalVariables.Instance.GetVariable(synchronizedVariable.targetName);
              if (synchronizedVariable.targetSharedVariable == null)
              {
                str = "the target SharedVariable cannot be found";
                break;
              }
              break;
            case VariableSynchronizer.SynchronizationType.Property:
              PropertyInfo property = ((object) synchronizedVariable.targetComponent).GetType().GetProperty(synchronizedVariable.targetName);
              if (property == null)
              {
                str = "the property " + synchronizedVariable.targetName + " doesn't exist";
                break;
              }
              if (synchronizedVariable.setVariable)
              {
                MethodInfo getMethod = property.GetGetMethod();
                if (getMethod == null)
                {
                  str = "the property has no get method";
                  break;
                }
                synchronizedVariable.getDelegate = VariableSynchronizer.CreateGetDelegate((object) synchronizedVariable.targetComponent, getMethod);
                break;
              }
              MethodInfo setMethod = property.GetSetMethod();
              if (setMethod == null)
              {
                str = "the property has no set method";
                break;
              }
              synchronizedVariable.setDelegate = VariableSynchronizer.CreateSetDelegate((object) synchronizedVariable.targetComponent, setMethod);
              break;
            case VariableSynchronizer.SynchronizationType.Animator:
              synchronizedVariable.animator = synchronizedVariable.targetComponent as Animator;
              if (Object.op_Equality((Object) synchronizedVariable.animator, (Object) null))
              {
                str = "the component is not of type Animator";
                break;
              }
              synchronizedVariable.targetID = Animator.StringToHash(synchronizedVariable.targetName);
              Type propertyType = synchronizedVariable.sharedVariable.GetType().GetProperty("Value").PropertyType;
              if (propertyType.Equals(typeof (bool)))
              {
                synchronizedVariable.animatorParameterType = VariableSynchronizer.AnimatorParameterType.Bool;
                break;
              }
              if (propertyType.Equals(typeof (float)))
              {
                synchronizedVariable.animatorParameterType = VariableSynchronizer.AnimatorParameterType.Float;
                break;
              }
              if (propertyType.Equals(typeof (int)))
              {
                synchronizedVariable.animatorParameterType = VariableSynchronizer.AnimatorParameterType.Integer;
                break;
              }
              str = "there is no animator parameter type that can synchronize with " + (object) propertyType;
              break;
            case VariableSynchronizer.SynchronizationType.PlayMaker:
              Type typeWithinAssembly1 = TaskUtility.GetTypeWithinAssembly("BehaviorDesigner.Runtime.VariableSynchronizer_PlayMaker");
              if (typeWithinAssembly1 != null)
              {
                MethodInfo method1 = typeWithinAssembly1.GetMethod("Start");
                if (method1 != null)
                {
                  switch ((int) method1.Invoke((object) null, new object[1]
                  {
                    (object) synchronizedVariable
                  }))
                  {
                    case 1:
                      str = "the PlayMaker NamedVariable cannot be found";
                      break;
                    case 2:
                      str = "the Behavior Designer SharedVariable is not the same type as the PlayMaker NamedVariable";
                      break;
                    default:
                      MethodInfo method2 = typeWithinAssembly1.GetMethod("Tick");
                      if (method2 != null)
                      {
                        synchronizedVariable.thirdPartyTick = (Action<VariableSynchronizer.SynchronizedVariable>) Delegate.CreateDelegate(typeof (Action<VariableSynchronizer.SynchronizedVariable>), method2);
                        break;
                      }
                      break;
                  }
                }
                else
                  break;
              }
              else
              {
                str = "has the PlayMaker classes been imported?";
                break;
              }
            case VariableSynchronizer.SynchronizationType.uFrame:
              Type typeWithinAssembly2 = TaskUtility.GetTypeWithinAssembly("BehaviorDesigner.Runtime.VariableSynchronizer_uFrame");
              if (typeWithinAssembly2 != null)
              {
                MethodInfo method1 = typeWithinAssembly2.GetMethod("Start");
                if (method1 != null)
                {
                  switch ((int) method1.Invoke((object) null, new object[1]
                  {
                    (object) synchronizedVariable
                  }))
                  {
                    case 1:
                      str = "the uFrame property cannot be found";
                      break;
                    case 2:
                      str = "the Behavior Designer SharedVariable is not the same type as the uFrame property";
                      break;
                    default:
                      MethodInfo method2 = typeWithinAssembly2.GetMethod("Tick");
                      if (method2 != null)
                      {
                        synchronizedVariable.thirdPartyTick = (Action<VariableSynchronizer.SynchronizedVariable>) Delegate.CreateDelegate(typeof (Action<VariableSynchronizer.SynchronizedVariable>), method2);
                        break;
                      }
                      break;
                  }
                }
                else
                  break;
              }
              else
              {
                str = "has the uFrame classes been imported?";
                break;
              }
          }
        }
        if (!string.IsNullOrEmpty(str))
        {
          Debug.LogError((object) string.Format("Unable to synchronize {0}: {1}", (object) synchronizedVariable.sharedVariable.Name, (object) str));
          this.synchronizedVariables.RemoveAt(index);
        }
      }
      if (this.synchronizedVariables.Count == 0)
        ((Behaviour) this).set_enabled(false);
      else
        this.UpdateIntervalChanged();
    }

    public void Update()
    {
      this.Tick();
    }

    [DebuggerHidden]
    private IEnumerator CoroutineUpdate()
    {
      // ISSUE: object of a compiler-generated type is created
      return (IEnumerator) new VariableSynchronizer.\u003CCoroutineUpdate\u003Ec__Iterator2()
      {
        \u003C\u003Ef__this = this
      };
    }

    public void Tick()
    {
      for (int index = 0; index < this.synchronizedVariables.Count; ++index)
      {
        VariableSynchronizer.SynchronizedVariable synchronizedVariable = this.synchronizedVariables[index];
        switch (synchronizedVariable.synchronizationType)
        {
          case VariableSynchronizer.SynchronizationType.BehaviorDesigner:
            if (synchronizedVariable.setVariable)
            {
              synchronizedVariable.sharedVariable.SetValue(synchronizedVariable.targetSharedVariable.GetValue());
              break;
            }
            synchronizedVariable.targetSharedVariable.SetValue(synchronizedVariable.sharedVariable.GetValue());
            break;
          case VariableSynchronizer.SynchronizationType.Property:
            if (synchronizedVariable.setVariable)
            {
              synchronizedVariable.sharedVariable.SetValue(synchronizedVariable.getDelegate());
              break;
            }
            synchronizedVariable.setDelegate(synchronizedVariable.sharedVariable.GetValue());
            break;
          case VariableSynchronizer.SynchronizationType.Animator:
            if (synchronizedVariable.setVariable)
            {
              switch (synchronizedVariable.animatorParameterType)
              {
                case VariableSynchronizer.AnimatorParameterType.Bool:
                  synchronizedVariable.sharedVariable.SetValue((object) synchronizedVariable.animator.GetBool(synchronizedVariable.targetID));
                  continue;
                case VariableSynchronizer.AnimatorParameterType.Float:
                  synchronizedVariable.sharedVariable.SetValue((object) synchronizedVariable.animator.GetFloat(synchronizedVariable.targetID));
                  continue;
                case VariableSynchronizer.AnimatorParameterType.Integer:
                  synchronizedVariable.sharedVariable.SetValue((object) synchronizedVariable.animator.GetInteger(synchronizedVariable.targetID));
                  continue;
                default:
                  continue;
              }
            }
            else
            {
              switch (synchronizedVariable.animatorParameterType)
              {
                case VariableSynchronizer.AnimatorParameterType.Bool:
                  synchronizedVariable.animator.SetBool(synchronizedVariable.targetID, (bool) synchronizedVariable.sharedVariable.GetValue());
                  continue;
                case VariableSynchronizer.AnimatorParameterType.Float:
                  synchronizedVariable.animator.SetFloat(synchronizedVariable.targetID, (float) synchronizedVariable.sharedVariable.GetValue());
                  continue;
                case VariableSynchronizer.AnimatorParameterType.Integer:
                  synchronizedVariable.animator.SetInteger(synchronizedVariable.targetID, (int) synchronizedVariable.sharedVariable.GetValue());
                  continue;
                default:
                  continue;
              }
            }
          case VariableSynchronizer.SynchronizationType.PlayMaker:
          case VariableSynchronizer.SynchronizationType.uFrame:
            synchronizedVariable.thirdPartyTick(synchronizedVariable);
            break;
        }
      }
    }

    private static Func<object> CreateGetDelegate(object instance, MethodInfo method)
    {
      return ((Expression<Func<object>>) (() => Expression.Call(instance, method) as object)).Compile();
    }

    private static Action<object> CreateSetDelegate(object instance, MethodInfo method)
    {
      ConstantExpression constantExpression = Expression.Constant(instance);
      ParameterExpression parameterExpression = Expression.Parameter(typeof (object), "p");
      UnaryExpression unaryExpression = Expression.Convert((Expression) parameterExpression, method.GetParameters()[0].ParameterType);
      return Expression.Lambda<Action<object>>((Expression) Expression.Call((Expression) constantExpression, method, (Expression) unaryExpression), parameterExpression).Compile();
    }

    public enum SynchronizationType
    {
      BehaviorDesigner,
      Property,
      Animator,
      PlayMaker,
      uFrame,
    }

    public enum AnimatorParameterType
    {
      Bool,
      Float,
      Integer,
    }

    [Serializable]
    public class SynchronizedVariable
    {
      public VariableSynchronizer.SynchronizationType synchronizationType;
      public bool setVariable;
      public Behavior behavior;
      public string variableName;
      public bool global;
      public Component targetComponent;
      public string targetName;
      public bool targetGlobal;
      public SharedVariable targetSharedVariable;
      public Action<object> setDelegate;
      public Func<object> getDelegate;
      public Animator animator;
      public VariableSynchronizer.AnimatorParameterType animatorParameterType;
      public int targetID;
      public Action<VariableSynchronizer.SynchronizedVariable> thirdPartyTick;
      public Enum variableType;
      public object thirdPartyVariable;
      public SharedVariable sharedVariable;

      public SynchronizedVariable(
        VariableSynchronizer.SynchronizationType synchronizationType,
        bool setVariable,
        Behavior behavior,
        string variableName,
        bool global,
        Component targetComponent,
        string targetName,
        bool targetGlobal)
      {
        this.synchronizationType = synchronizationType;
        this.setVariable = setVariable;
        this.behavior = behavior;
        this.variableName = variableName;
        this.global = global;
        this.targetComponent = targetComponent;
        this.targetName = targetName;
        this.targetGlobal = targetGlobal;
      }
    }
  }
}
