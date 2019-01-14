// Decompiled with JetBrains decompiler
// Type: BehaviorDesigner.Runtime.Tasks.TaskIconAttribute
// Assembly: BehaviorDesignerRuntime, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 4E52B3C7-423D-4A3D-958C-02E4666F0F16
// Assembly location: C:\Users\Ron\Desktop\Runtime\BehaviorDesignerRuntime.dll

using System;

namespace BehaviorDesigner.Runtime.Tasks
{
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
  public class TaskIconAttribute : Attribute
  {
    public readonly string mIconPath;

    public TaskIconAttribute(string iconPath)
    {
      this.mIconPath = iconPath;
    }

    public string IconPath
    {
      get
      {
        return this.mIconPath;
      }
    }
  }
}
