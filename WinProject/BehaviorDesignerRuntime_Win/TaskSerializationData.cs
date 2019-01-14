// Decompiled with JetBrains decompiler
// Type: BehaviorDesigner.Runtime.TaskSerializationData
// Assembly: BehaviorDesignerRuntime, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 4E52B3C7-423D-4A3D-958C-02E4666F0F16
// Assembly location: C:\Users\Ron\Desktop\Runtime\BehaviorDesignerRuntime.dll

using System;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviorDesigner.Runtime
{
  [Serializable]
  public class TaskSerializationData
  {
    [SerializeField]
    public List<string> types = new List<string>();
    [SerializeField]
    public List<int> parentIndex = new List<int>();
    [SerializeField]
    public List<int> startIndex = new List<int>();
    [SerializeField]
    public List<int> variableStartIndex = new List<int>();
    [SerializeField]
    public FieldSerializationData fieldSerializationData = new FieldSerializationData();
    [SerializeField]
    public string JSONSerialization;
    [SerializeField]
    public string Version;
  }
}
