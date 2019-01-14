// Decompiled with JetBrains decompiler
// Type: BehaviorDesigner.Runtime.Tasks.UnknownTask
// Assembly: BehaviorDesignerRuntime, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 4E52B3C7-423D-4A3D-958C-02E4666F0F16
// Assembly location: C:\Users\Ron\Desktop\Runtime\BehaviorDesignerRuntime.dll

using System.Collections.Generic;
using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks
{
  public class UnknownTask : Task
  {
    [HideInInspector]
    public List<int> fieldNameHash = new List<int>();
    [HideInInspector]
    public List<int> startIndex = new List<int>();
    [HideInInspector]
    public List<int> dataPosition = new List<int>();
    [HideInInspector]
    public List<Object> unityObjects = new List<Object>();
    [HideInInspector]
    public List<byte> byteData = new List<byte>();
    [HideInInspector]
    public string JSONSerialization;
  }
}
