// Decompiled with JetBrains decompiler
// Type: BehaviorDesigner.Runtime.Tasks.TaskNameAttribute
// Assembly: BehaviorDesignerRuntime, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 4E52B3C7-423D-4A3D-958C-02E4666F0F16
// Assembly location: C:\Users\Ron\Desktop\Runtime\BehaviorDesignerRuntime.dll

using System;

namespace BehaviorDesigner.Runtime.Tasks
{
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
  public class TaskNameAttribute : Attribute
  {
    public readonly string mName;

    public TaskNameAttribute(string name)
    {
      this.mName = name;
    }

    public string Name
    {
      get
      {
        return this.mName;
      }
    }
  }
}
