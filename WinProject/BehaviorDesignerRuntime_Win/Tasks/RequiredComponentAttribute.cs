// Decompiled with JetBrains decompiler
// Type: BehaviorDesigner.Runtime.Tasks.RequiredComponentAttribute
// Assembly: BehaviorDesignerRuntime, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 4E52B3C7-423D-4A3D-958C-02E4666F0F16
// Assembly location: C:\Users\Ron\Desktop\Runtime\BehaviorDesignerRuntime.dll

using System;

namespace BehaviorDesigner.Runtime.Tasks
{
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
  public class RequiredComponentAttribute : Attribute
  {
    public readonly Type mComponentType;

    public RequiredComponentAttribute(Type componentType)
    {
      this.mComponentType = componentType;
    }

    public Type ComponentType
    {
      get
      {
        return this.mComponentType;
      }
    }
  }
}
