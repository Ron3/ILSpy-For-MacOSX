// Decompiled with JetBrains decompiler
// Type: BehaviorDesigner.Runtime.IBehavior
// Assembly: BehaviorDesignerRuntime, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 4E52B3C7-423D-4A3D-958C-02E4666F0F16
// Assembly location: C:\Users\Ron\Desktop\Runtime\BehaviorDesignerRuntime.dll

using UnityEngine;

namespace BehaviorDesigner.Runtime
{
  public interface IBehavior
  {
    string GetOwnerName();

    int GetInstanceID();

    BehaviorSource GetBehaviorSource();

    void SetBehaviorSource(BehaviorSource behaviorSource);

    Object GetObject();

    SharedVariable GetVariable(string name);

    void SetVariable(string name, SharedVariable item);

    void SetVariableValue(string name, object value);
  }
}
