// Decompiled with JetBrains decompiler
// Type: AOTLinker
// Assembly: BehaviorDesignerRuntime, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 4E52B3C7-423D-4A3D-958C-02E4666F0F16
// Assembly location: C:\Users\Ron\Desktop\Runtime\BehaviorDesignerRuntime.dll

using BehaviorDesigner.Runtime;
using UnityEngine;

public class AOTLinker : MonoBehaviour
{
  public AOTLinker()
  {
    base.\u002Ector();
  }

  public void Linker()
  {
    BehaviorManager.BehaviorTree behaviorTree = new BehaviorManager.BehaviorTree();
    BehaviorManager.BehaviorTree.ConditionalReevaluate conditionalReevaluate = new BehaviorManager.BehaviorTree.ConditionalReevaluate();
    BehaviorManager.TaskAddData taskAddData = new BehaviorManager.TaskAddData();
    BehaviorManager.TaskAddData.OverrideFieldValue overrideFieldValue = new BehaviorManager.TaskAddData.OverrideFieldValue();
  }
}
