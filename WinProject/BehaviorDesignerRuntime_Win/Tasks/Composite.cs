// Decompiled with JetBrains decompiler
// Type: BehaviorDesigner.Runtime.Tasks.Composite
// Assembly: BehaviorDesignerRuntime, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 4E52B3C7-423D-4A3D-958C-02E4666F0F16
// Assembly location: C:\Users\Ron\Desktop\Runtime\BehaviorDesignerRuntime.dll

using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks
{
  public abstract class Composite : ParentTask
  {
    [Tooltip("Specifies the type of conditional abort. More information is located at http://www.opsive.com/assets/BehaviorDesigner/documentation.php?id=89.")]
    [SerializeField]
    protected AbortType abortType;

    public AbortType AbortType
    {
      get
      {
        return this.abortType;
      }
    }
  }
}
