// Decompiled with JetBrains decompiler
// Type: BehaviorDesigner.Editor.BehaviorUndo
// Assembly: BehaviorDesignerEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 99CE4D00-DFA2-42D1-ABFC-D630AB4C1372
// Assembly location: C:\Users\Ron\Desktop\bt\BehaviorDesignerEditor.dll

using System;
using UnityEditor;
using UnityEngine;

namespace BehaviorDesigner.Editor
{
  public class BehaviorUndo
  {
    public static void RegisterUndo(string undoName, Object undoObject)
    {
      Undo.RecordObject(undoObject, undoName);
    }

    public static Component AddComponent(GameObject undoObject, Type type)
    {
      return Undo.AddComponent(undoObject, type);
    }

    public static void DestroyObject(Object undoObject, bool registerScene)
    {
      Undo.DestroyObjectImmediate(undoObject);
    }
  }
}
