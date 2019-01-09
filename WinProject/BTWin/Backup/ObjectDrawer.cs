// Decompiled with JetBrains decompiler
// Type: BehaviorDesigner.Editor.ObjectDrawer
// Assembly: BehaviorDesignerEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 99CE4D00-DFA2-42D1-ABFC-D630AB4C1372
// Assembly location: C:\Users\Ron\Desktop\bt\BehaviorDesignerEditor.dll

using BehaviorDesigner.Runtime.Tasks;
using System.Reflection;
using UnityEngine;

namespace BehaviorDesigner.Editor
{
  public class ObjectDrawer
  {
    protected FieldInfo fieldInfo;
    protected ObjectDrawerAttribute attribute;
    protected object value;
    protected Task task;

    public FieldInfo FieldInfo
    {
      get
      {
        return this.fieldInfo;
      }
      set
      {
        this.fieldInfo = value;
      }
    }

    public ObjectDrawerAttribute Attribute
    {
      get
      {
        return this.attribute;
      }
      set
      {
        this.attribute = value;
      }
    }

    public object Value
    {
      get
      {
        return this.value;
      }
      set
      {
        this.value = value;
      }
    }

    public Task Task
    {
      get
      {
        return this.task;
      }
      set
      {
        this.task = value;
      }
    }

    public virtual void OnGUI(GUIContent label)
    {
    }
  }
}
