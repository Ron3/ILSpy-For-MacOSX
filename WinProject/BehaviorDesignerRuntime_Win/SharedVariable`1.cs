// Decompiled with JetBrains decompiler
// Type: BehaviorDesigner.Runtime.SharedVariable`1
// Assembly: BehaviorDesignerRuntime, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 4E52B3C7-423D-4A3D-958C-02E4666F0F16
// Assembly location: C:\Users\Ron\Desktop\Runtime\BehaviorDesignerRuntime.dll

using System;
using System.Reflection;
using UnityEngine;

namespace BehaviorDesigner.Runtime
{
  public abstract class SharedVariable<T> : SharedVariable
  {
    private Func<T> mGetter;
    private Action<T> mSetter;
    [SerializeField]
    protected T mValue;

    public override void InitializePropertyMapping(BehaviorSource behaviorSource)
    {
      if (!Application.get_isPlaying() || !(behaviorSource.Owner.GetObject() is Behavior) || string.IsNullOrEmpty(this.PropertyMapping))
        return;
      string[] strArray = this.PropertyMapping.Split('/');
      GameObject gameObject = object.Equals((object) this.PropertyMappingOwner, (object) null) ? ((Component) (behaviorSource.Owner.GetObject() as Behavior)).get_gameObject() : this.PropertyMappingOwner;
      if (Object.op_Equality((Object) gameObject, (Object) null))
      {
        Debug.LogError((object) ("Error: Unable to find GameObject on " + behaviorSource.behaviorName + " for property mapping with variable " + this.Name));
      }
      else
      {
        Component component = gameObject.GetComponent(TaskUtility.GetTypeWithinAssembly(strArray[0]));
        if (Object.op_Equality((Object) component, (Object) null))
        {
          Debug.LogError((object) ("Error: Unable to find component on " + behaviorSource.behaviorName + " for property mapping with variable " + this.Name));
        }
        else
        {
          PropertyInfo property = ((object) component).GetType().GetProperty(strArray[1]);
          if (property == null)
            return;
          MethodInfo getMethod = property.GetGetMethod();
          if (getMethod != null)
            this.mGetter = (Func<T>) Delegate.CreateDelegate(typeof (Func<T>), (object) component, getMethod);
          MethodInfo setMethod = property.GetSetMethod();
          if (setMethod == null)
            return;
          this.mSetter = (Action<T>) Delegate.CreateDelegate(typeof (Action<T>), (object) component, setMethod);
        }
      }
    }

    public T Value
    {
      get
      {
        if (this.mGetter != null)
          return this.mGetter();
        return this.mValue;
      }
      set
      {
        bool flag = this.NetworkSync && !this.mValue.Equals((object) value);
        if (this.mSetter != null)
          this.mSetter(value);
        else
          this.mValue = value;
        if (!flag)
          return;
        this.ValueChanged();
      }
    }

    public override object GetValue()
    {
      return (object) this.Value;
    }

    public override void SetValue(object value)
    {
      if (this.mSetter != null)
        this.mSetter((T) value);
      else
        this.mValue = (T) value;
    }

    public override string ToString()
    {
      if ((object) this.Value == null)
        return "(null)";
      return this.Value.ToString();
    }
  }
}
