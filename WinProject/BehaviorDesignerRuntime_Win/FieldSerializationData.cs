// Decompiled with JetBrains decompiler
// Type: BehaviorDesigner.Runtime.FieldSerializationData
// Assembly: BehaviorDesignerRuntime, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 4E52B3C7-423D-4A3D-958C-02E4666F0F16
// Assembly location: C:\Users\Ron\Desktop\Runtime\BehaviorDesignerRuntime.dll

using System;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviorDesigner.Runtime
{
  [Serializable]
  public class FieldSerializationData
  {
    [SerializeField]
    public List<string> typeName = new List<string>();
    [SerializeField]
    public List<int> fieldNameHash = new List<int>();
    [SerializeField]
    public List<int> startIndex = new List<int>();
    [SerializeField]
    public List<int> dataPosition = new List<int>();
    [SerializeField]
    public List<Object> unityObjects = new List<Object>();
    [SerializeField]
    public List<byte> byteData = new List<byte>();
    public byte[] byteDataArray;
  }
}
