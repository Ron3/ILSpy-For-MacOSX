// Decompiled with JetBrains decompiler
// Type: BehaviorDesigner.Runtime.Tasks.BehaviorReference
// Assembly: BehaviorDesignerRuntime, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 4E52B3C7-423D-4A3D-958C-02E4666F0F16
// Assembly location: C:\Users\Ron\Desktop\Runtime\BehaviorDesignerRuntime.dll

using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks
{
  [TaskDescription("Behavior Reference allows you to run another behavior tree within the current behavior tree.")]
  [HelpURL("http://www.opsive.com/assets/BehaviorDesigner/documentation.php?id=53")]
  [TaskIcon("BehaviorTreeReferenceIcon.png")]
  public abstract class BehaviorReference : Action
  {
    [Tooltip("External behavior array that this task should reference")]
    public ExternalBehavior[] externalBehaviors;
    [Tooltip("Any variables that should be set for the specific tree")]
    public SharedNamedVariable[] variables;
    [HideInInspector]
    public bool collapsed;

    public virtual ExternalBehavior[] GetExternalBehaviors()
    {
      return this.externalBehaviors;
    }

    public override void OnReset()
    {
      this.externalBehaviors = (ExternalBehavior[]) null;
    }
  }
}
