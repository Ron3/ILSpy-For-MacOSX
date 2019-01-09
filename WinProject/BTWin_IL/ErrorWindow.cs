// Decompiled with JetBrains decompiler
// Type: BehaviorDesigner.Editor.ErrorWindow
// Assembly: BehaviorDesignerEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 99CE4D00-DFA2-42D1-ABFC-D630AB4C1372
// Assembly location: C:\Users\Ron\Desktop\bt\BehaviorDesignerEditor.dll

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BehaviorDesigner.Editor
{
  public class ErrorWindow : EditorWindow
  {
    private List<BehaviorDesigner.Editor.ErrorDetails> mErrorDetails;
    private Vector2 mScrollPosition;
    public static ErrorWindow instance;

    public ErrorWindow()
    {
      base.\u002Ector();
    }

    public List<BehaviorDesigner.Editor.ErrorDetails> ErrorDetails
    {
      set
      {
        this.mErrorDetails = value;
      }
    }

    [MenuItem("Tools/Behavior Designer/Error List", false, 2)]
    public static void ShowWindow()
    {
      ErrorWindow window = (ErrorWindow) EditorWindow.GetWindow<ErrorWindow>(false, "Error List");
      window.set_minSize(new Vector2(400f, 200f));
      window.set_wantsMouseMove(true);
    }

    public void OnFocus()
    {
      ErrorWindow.instance = this;
      if (!Object.op_Inequality((Object) BehaviorDesignerWindow.instance, (Object) null))
        return;
      this.mErrorDetails = BehaviorDesignerWindow.instance.ErrorDetails;
    }

    public void OnGUI()
    {
      this.mScrollPosition = EditorGUILayout.BeginScrollView(this.mScrollPosition, new GUILayoutOption[0]);
      if (this.mErrorDetails != null && this.mErrorDetails.Count > 0)
      {
        for (int index = 0; index < this.mErrorDetails.Count; ++index)
        {
          BehaviorDesigner.Editor.ErrorDetails mErrorDetail = this.mErrorDetails[index];
          if (mErrorDetail != null && (mErrorDetail.Type == BehaviorDesigner.Editor.ErrorDetails.ErrorType.InvalidVariableReference || !Object.op_Equality((Object) mErrorDetail.NodeDesigner, (Object) null) && mErrorDetail.NodeDesigner.Task != null))
          {
            string str = string.Empty;
            switch (mErrorDetail.Type)
            {
              case BehaviorDesigner.Editor.ErrorDetails.ErrorType.RequiredField:
                str = string.Format("The task {0} ({1}, index {2}) requires a value for the field {3}.", (object) mErrorDetail.TaskFriendlyName, (object) mErrorDetail.TaskType, (object) mErrorDetail.NodeDesigner.Task.get_ID(), (object) BehaviorDesignerUtility.SplitCamelCase(mErrorDetail.FieldName));
                break;
              case BehaviorDesigner.Editor.ErrorDetails.ErrorType.SharedVariable:
                str = string.Format("The task {0} ({1}, index {2}) has a Shared Variable field ({3}) that is marked as shared but is not referencing a Shared Variable.", (object) mErrorDetail.TaskFriendlyName, (object) mErrorDetail.TaskType, (object) mErrorDetail.NodeDesigner.Task.get_ID(), (object) BehaviorDesignerUtility.SplitCamelCase(mErrorDetail.FieldName));
                break;
              case BehaviorDesigner.Editor.ErrorDetails.ErrorType.MissingChildren:
                str = string.Format("The {0} task ({1}, index {2}) is a parent task which does not have any children", (object) mErrorDetail.TaskFriendlyName, (object) mErrorDetail.TaskType, (object) mErrorDetail.NodeDesigner.Task.get_ID());
                break;
              case BehaviorDesigner.Editor.ErrorDetails.ErrorType.UnknownTask:
                str = string.Format("The task at index {0} is unknown. Has a task been renamed or deleted?", (object) mErrorDetail.NodeDesigner.Task.get_ID());
                break;
              case BehaviorDesigner.Editor.ErrorDetails.ErrorType.InvalidTaskReference:
                str = string.Format("The task {0} ({1}, index {2}) has a field ({3}) which is referencing an object within the scene. Behavior tree variables at the project level cannot reference objects within a scene.", (object) mErrorDetail.TaskFriendlyName, (object) mErrorDetail.TaskType, (object) mErrorDetail.NodeDesigner.Task.get_ID(), (object) BehaviorDesignerUtility.SplitCamelCase(mErrorDetail.FieldName));
                break;
              case BehaviorDesigner.Editor.ErrorDetails.ErrorType.InvalidVariableReference:
                str = string.Format("The variable {0} is referencing an object within the scene. Behavior tree variables at the project level cannot reference objects within a scene.", (object) mErrorDetail.FieldName);
                break;
            }
            EditorGUILayout.LabelField(str, index % 2 != 0 ? BehaviorDesignerUtility.ErrorListDarkBackground : BehaviorDesignerUtility.ErrorListLightBackground, new GUILayoutOption[2]
            {
              GUILayout.Height(30f),
              GUILayout.Width((float) (Screen.get_width() - 7))
            });
          }
        }
      }
      else if (!BehaviorDesignerPreferences.GetBool(BDPreferences.ErrorChecking))
        EditorGUILayout.LabelField("Enable realtime error checking from the preferences to view the errors.", BehaviorDesignerUtility.ErrorListLightBackground, new GUILayoutOption[0]);
      else
        EditorGUILayout.LabelField("The behavior tree has no errors.", BehaviorDesignerUtility.ErrorListLightBackground, new GUILayoutOption[0]);
      EditorGUILayout.EndScrollView();
    }
  }
}
