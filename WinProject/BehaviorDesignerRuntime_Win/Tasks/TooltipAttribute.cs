// Decompiled with JetBrains decompiler
// Type: BehaviorDesigner.Runtime.Tasks.TooltipAttribute
// Assembly: BehaviorDesignerRuntime, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 4E52B3C7-423D-4A3D-958C-02E4666F0F16
// Assembly location: C:\Users\Ron\Desktop\Runtime\BehaviorDesignerRuntime.dll

using System;

namespace BehaviorDesigner.Runtime.Tasks
{
  [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
  public class TooltipAttribute : Attribute
  {
    public readonly string mTooltip;

    public TooltipAttribute(string tooltip)
    {
      this.mTooltip = tooltip;
    }

    public string Tooltip
    {
      get
      {
        return this.mTooltip;
      }
    }
  }
}
