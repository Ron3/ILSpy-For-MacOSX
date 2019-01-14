// Decompiled with JetBrains decompiler
// Type: BehaviorDesigner.Runtime.GenericVariable
// Assembly: BehaviorDesignerRuntime, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 4E52B3C7-423D-4A3D-958C-02E4666F0F16
// Assembly location: C:\Users\Ron\Desktop\Runtime\BehaviorDesignerRuntime.dll

using System;
using UnityEngine;

namespace BehaviorDesigner.Runtime
{
  [Serializable]
  public class GenericVariable
  {
    [SerializeField]
    public string type = "SharedString";
    [SerializeField]
    public SharedVariable value;

    public GenericVariable()
    {
      this.value = Activator.CreateInstance(TaskUtility.GetTypeWithinAssembly("BehaviorDesigner.Runtime.SharedString")) as SharedVariable;
    }
  }
}
