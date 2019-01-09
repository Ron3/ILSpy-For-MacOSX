// Decompiled with JetBrains decompiler
// Type: BehaviorDesigner.Editor.GlobalVariablesWindow
// Assembly: BehaviorDesignerEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 99CE4D00-DFA2-42D1-ABFC-D630AB4C1372
// Assembly location: C:\Users\Ron\Desktop\bt\BehaviorDesignerEditor.dll

using BehaviorDesigner.Runtime;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BehaviorDesigner.Editor
{
  public class GlobalVariablesWindow : EditorWindow
  {
    private string mVariableName;
    private int mVariableTypeIndex;
    private Vector2 mScrollPosition;
    private bool mFocusNameField;
    [SerializeField]
    private float mVariableStartPosition;
    [SerializeField]
    private List<float> mVariablePosition;
    [SerializeField]
    private int mSelectedVariableIndex;
    [SerializeField]
    private string mSelectedVariableName;
    [SerializeField]
    private int mSelectedVariableTypeIndex;
    private GlobalVariables mVariableSource;
    public static GlobalVariablesWindow instance;

    public GlobalVariablesWindow()
    {
      //base.\u002Ector();
    }

    [MenuItem("Tools/Behavior Designer/Global Variables", false, 1)]
    public static void ShowWindow()
    {
      GlobalVariablesWindow window = (GlobalVariablesWindow) EditorWindow.GetWindow<GlobalVariablesWindow>(false, "Global Variables");
      window.set_minSize(new Vector2(300f, 410f));
      window.set_maxSize(new Vector2(300f, float.MaxValue));
      window.set_wantsMouseMove(true);
    }

    public void OnFocus()
    {
      GlobalVariablesWindow.instance = this;
      this.mVariableSource = GlobalVariables.get_Instance();
      if (Object.op_Inequality((Object) this.mVariableSource, (Object) null))
        this.mVariableSource.CheckForSerialization(!Application.get_isPlaying());
      FieldInspector.Init();
    }

    public void OnGUI()
    {
      if (Object.op_Equality((Object) this.mVariableSource, (Object) null))
        this.mVariableSource = GlobalVariables.get_Instance();
      if (VariableInspector.DrawVariables((IVariableSource) this.mVariableSource, (BehaviorSource) null, ref this.mVariableName, ref this.mFocusNameField, ref this.mVariableTypeIndex, ref this.mScrollPosition, ref this.mVariablePosition, ref this.mVariableStartPosition, ref this.mSelectedVariableIndex, ref this.mSelectedVariableName, ref this.mSelectedVariableTypeIndex))
        this.SerializeVariables();
      if (Event.get_current().get_type() != null || !VariableInspector.LeftMouseDown((IVariableSource) this.mVariableSource, (BehaviorSource) null, Event.get_current().get_mousePosition(), this.mVariablePosition, this.mVariableStartPosition, this.mScrollPosition, ref this.mSelectedVariableIndex, ref this.mSelectedVariableName, ref this.mSelectedVariableTypeIndex))
        return;
      Event.get_current().Use();
      this.Repaint();
    }

    private void SerializeVariables()
    {
      if (Object.op_Equality((Object) this.mVariableSource, (Object) null))
        this.mVariableSource = GlobalVariables.get_Instance();
      if (BehaviorDesignerPreferences.GetBool(BDPreferences.BinarySerialization))
        BinarySerialization.Save(this.mVariableSource);
      else
        JSONSerialization.Save(this.mVariableSource);
    }
  }
}
