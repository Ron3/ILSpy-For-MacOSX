// Decompiled with JetBrains decompiler
// Type: BehaviorDesigner.Editor.TaskSerializer
// Assembly: BehaviorDesignerEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 99CE4D00-DFA2-42D1-ABFC-D630AB4C1372
// Assembly location: C:\Users\Ron\Desktop\bt\BehaviorDesignerEditor.dll

using System;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviorDesigner.Editor
{
  [Serializable]
  public class TaskSerializer
  {
    public string serialization;
    public Vector2 offset;
    public List<Object> unityObjects;
    public List<int> childrenIndex;
  }
}
