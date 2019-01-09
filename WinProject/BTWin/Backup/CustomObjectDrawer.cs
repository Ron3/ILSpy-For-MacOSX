// Decompiled with JetBrains decompiler
// Type: BehaviorDesigner.Editor.CustomObjectDrawer
// Assembly: BehaviorDesignerEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 99CE4D00-DFA2-42D1-ABFC-D630AB4C1372
// Assembly location: C:\Users\Ron\Desktop\bt\BehaviorDesignerEditor.dll

using System;

namespace BehaviorDesigner.Editor
{
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
  public sealed class CustomObjectDrawer : Attribute
  {
    private Type type;

    public CustomObjectDrawer(Type type)
    {
      this.type = type;
    }

    public Type Type
    {
      get
      {
        return this.type;
      }
    }
  }
}
