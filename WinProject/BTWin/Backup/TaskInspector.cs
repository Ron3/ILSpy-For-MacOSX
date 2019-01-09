// Decompiled with JetBrains decompiler
// Type: BehaviorDesigner.Editor.TaskInspector
// Assembly: BehaviorDesignerEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 99CE4D00-DFA2-42D1-ABFC-D630AB4C1372
// Assembly location: C:\Users\Ron\Desktop\bt\BehaviorDesignerEditor.dll

using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace BehaviorDesigner.Editor
{
  [Serializable]
  public class TaskInspector : ScriptableObject
  {
    private BehaviorDesignerWindow behaviorDesignerWindow;
    private Task activeReferenceTask;
    private FieldInfo activeReferenceTaskFieldInfo;
    private Task mActiveMenuSelectionTask;
    private Vector2 mScrollPosition;

    public TaskInspector()
    {
      base.\u002Ector();
    }

    public Task ActiveReferenceTask
    {
      get
      {
        return this.activeReferenceTask;
      }
    }

    public FieldInfo ActiveReferenceTaskFieldInfo
    {
      get
      {
        return this.activeReferenceTaskFieldInfo;
      }
    }

    public void OnEnable()
    {
      ((Object) this).set_hideFlags((HideFlags) 61);
    }

    public void ClearFocus()
    {
      GUIUtility.set_keyboardControl(0);
    }

    public bool HasFocus()
    {
      return GUIUtility.get_keyboardControl() != 0;
    }

    public bool DrawTaskInspector(
      BehaviorSource behaviorSource,
      TaskList taskList,
      Task task,
      bool enabled)
    {
      if (task == null || (task.get_NodeData().get_NodeDesigner() as NodeDesigner).IsEntryDisplay)
        return false;
      this.mScrollPosition = GUILayout.BeginScrollView(this.mScrollPosition, new GUILayoutOption[0]);
      GUI.set_enabled(enabled);
      if (Object.op_Equality((Object) this.behaviorDesignerWindow, (Object) null))
        this.behaviorDesignerWindow = BehaviorDesignerWindow.instance;
      EditorGUIUtility.set_labelWidth(150f);
      EditorGUI.BeginChangeCheck();
      GUILayout.BeginHorizontal(new GUILayoutOption[0]);
      EditorGUILayout.LabelField("Name", new GUILayoutOption[1]
      {
        GUILayout.Width(90f)
      });
      task.set_FriendlyName(EditorGUILayout.TextField(task.get_FriendlyName(), new GUILayoutOption[0]));
      if (GUILayout.Button((Texture) BehaviorDesignerUtility.DocTexture, BehaviorDesignerUtility.TransparentButtonGUIStyle, new GUILayoutOption[0]))
        this.OpenHelpURL(task);
      if (GUILayout.Button((Texture) BehaviorDesignerUtility.ColorSelectorTexture(task.get_NodeData().get_ColorIndex()), BehaviorDesignerUtility.TransparentButtonOffsetGUIStyle, new GUILayoutOption[0]))
      {
        GenericMenu menu = new GenericMenu();
        this.AddColorMenuItem(ref menu, task, "Default", 0);
        this.AddColorMenuItem(ref menu, task, "Red", 1);
        this.AddColorMenuItem(ref menu, task, "Pink", 2);
        this.AddColorMenuItem(ref menu, task, "Brown", 3);
        this.AddColorMenuItem(ref menu, task, "Orange", 4);
        this.AddColorMenuItem(ref menu, task, "Turquoise", 5);
        this.AddColorMenuItem(ref menu, task, "Cyan", 6);
        this.AddColorMenuItem(ref menu, task, "Blue", 7);
        this.AddColorMenuItem(ref menu, task, "Purple", 8);
        menu.ShowAsContext();
      }
      if (GUILayout.Button((Texture) BehaviorDesignerUtility.GearTexture, BehaviorDesignerUtility.TransparentButtonGUIStyle, new GUILayoutOption[0]))
      {
        GenericMenu genericMenu = new GenericMenu();
        // ISSUE: method pointer
        genericMenu.AddItem(new GUIContent("Edit Script"), false, new GenericMenu.MenuFunction2((object) null, __methodptr(OpenInFileEditor)), (object) task);
        // ISSUE: method pointer
        genericMenu.AddItem(new GUIContent("Locate Script"), false, new GenericMenu.MenuFunction2((object) null, __methodptr(SelectInProject)), (object) task);
        // ISSUE: method pointer
        genericMenu.AddItem(new GUIContent("Reset"), false, new GenericMenu.MenuFunction2((object) this, __methodptr(ResetTask)), (object) task);
        genericMenu.ShowAsContext();
      }
      GUILayout.EndHorizontal();
      string str = BehaviorDesignerUtility.SplitCamelCase(((object) task).GetType().Name.ToString());
      if (!task.get_FriendlyName().Equals(str))
      {
        GUILayout.BeginHorizontal(new GUILayoutOption[0]);
        EditorGUILayout.LabelField("Type", new GUILayoutOption[1]
        {
          GUILayout.Width(90f)
        });
        EditorGUILayout.LabelField(str, new GUILayoutOption[1]
        {
          GUILayout.MaxWidth(170f)
        });
        GUILayout.EndHorizontal();
      }
      GUILayout.BeginHorizontal(new GUILayoutOption[0]);
      EditorGUILayout.LabelField("Instant", new GUILayoutOption[1]
      {
        GUILayout.Width(90f)
      });
      task.set_IsInstant(EditorGUILayout.Toggle(task.get_IsInstant(), new GUILayoutOption[0]));
      GUILayout.EndHorizontal();
      EditorGUILayout.LabelField("Comment", new GUILayoutOption[0]);
      task.get_NodeData().set_Comment(EditorGUILayout.TextArea(task.get_NodeData().get_Comment(), BehaviorDesignerUtility.TaskInspectorCommentGUIStyle, new GUILayoutOption[1]
      {
        GUILayout.Height(48f)
      }));
      if (EditorGUI.EndChangeCheck())
        GUI.set_changed(true);
      BehaviorDesignerUtility.DrawContentSeperator(2);
      GUILayout.Space(6f);
      if (this.DrawTaskFields(behaviorSource, taskList, task, enabled))
      {
        BehaviorUndo.RegisterUndo("Inspector", behaviorSource.get_Owner().GetObject());
        GUI.set_changed(true);
      }
      GUI.set_enabled(true);
      GUILayout.EndScrollView();
      return GUI.get_changed();
    }

    private bool DrawTaskFields(
      BehaviorSource behaviorSource,
      TaskList taskList,
      Task task,
      bool enabled)
    {
      if (task == null)
        return false;
      EditorGUI.BeginChangeCheck();
      FieldInspector.behaviorSource = behaviorSource;
      this.DrawObjectFields(behaviorSource, taskList, task, (object) task, enabled, true);
      return EditorGUI.EndChangeCheck();
    }

    private void DrawObjectFields(
      BehaviorSource behaviorSource,
      TaskList taskList,
      Task task,
      object obj,
      bool enabled,
      bool drawWatch)
    {
      if (obj == null)
        return;
      List<Type> baseClasses = FieldInspector.GetBaseClasses(obj.GetType());
      BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
      bool isReflectionTask = this.IsReflectionTask(obj.GetType());
      for (int index1 = baseClasses.Count - 1; index1 > -1; --index1)
      {
        FieldInfo[] fields = baseClasses[index1].GetFields(bindingAttr);
        for (int index2 = 0; index2 < fields.Length; ++index2)
        {
          if (!BehaviorDesignerUtility.HasAttribute(fields[index2], typeof (NonSerializedAttribute)) && !BehaviorDesignerUtility.HasAttribute(fields[index2], typeof (HideInInspector)) && (!fields[index2].IsPrivate && !fields[index2].IsFamily || BehaviorDesignerUtility.HasAttribute(fields[index2], typeof (SerializeField))) && (!(obj is ParentTask) || !fields[index2].Name.Equals("children")) && (!isReflectionTask || !fields[index2].FieldType.Equals(typeof (SharedVariable)) && !fields[index2].FieldType.IsSubclassOf(typeof (SharedVariable)) || this.CanDrawReflectedField(obj, fields[index2])))
          {
            string s = fields[index2].Name;
            if (isReflectionTask && (fields[index2].FieldType.Equals(typeof (SharedVariable)) || fields[index2].FieldType.IsSubclassOf(typeof (SharedVariable))))
              s = this.InvokeParameterName(obj, fields[index2]);
            TooltipAttribute[] customAttributes;
            GUIContent guiContent = (customAttributes = fields[index2].GetCustomAttributes(typeof (TooltipAttribute), false) as TooltipAttribute[]).Length <= 0 ? new GUIContent(BehaviorDesignerUtility.SplitCamelCase(s)) : new GUIContent(BehaviorDesignerUtility.SplitCamelCase(s), customAttributes[0].get_Tooltip());
            object obj1 = fields[index2].GetValue(obj);
            Type fieldType = fields[index2].FieldType;
            if (typeof (Task).IsAssignableFrom(fieldType) || typeof (IList).IsAssignableFrom(fieldType) && (typeof (Task).IsAssignableFrom(fieldType.GetElementType()) || fieldType.IsGenericType && typeof (Task).IsAssignableFrom(fieldType.GetGenericArguments()[0])))
            {
              EditorGUI.BeginChangeCheck();
              this.DrawTaskValue(behaviorSource, taskList, fields[index2], guiContent, task, obj1 as Task, enabled);
              if (BehaviorDesignerWindow.instance.ContainsError(task, fields[index2].Name))
              {
                GUILayout.Space(-3f);
                GUILayout.Box((Texture) BehaviorDesignerUtility.ErrorIconTexture, BehaviorDesignerUtility.PlainTextureGUIStyle, new GUILayoutOption[1]
                {
                  GUILayout.Width(20f)
                });
              }
              if (EditorGUI.EndChangeCheck())
                GUI.set_changed(true);
            }
            else if (fieldType.Equals(typeof (SharedVariable)) || fieldType.IsSubclassOf(typeof (SharedVariable)))
            {
              GUILayout.BeginHorizontal(new GUILayoutOption[0]);
              EditorGUI.BeginChangeCheck();
              if (drawWatch)
                this.DrawWatchedButton(task, fields[index2]);
              SharedVariable sharedVariable = this.DrawSharedVariableValue(behaviorSource, fields[index2], guiContent, task, obj1 as SharedVariable, isReflectionTask, enabled, drawWatch);
              if (BehaviorDesignerWindow.instance.ContainsError(task, fields[index2].Name))
              {
                GUILayout.Space(-3f);
                GUILayout.Box((Texture) BehaviorDesignerUtility.ErrorIconTexture, BehaviorDesignerUtility.PlainTextureGUIStyle, new GUILayoutOption[1]
                {
                  GUILayout.Width(20f)
                });
              }
              GUILayout.EndHorizontal();
              GUILayout.Space(4f);
              if (EditorGUI.EndChangeCheck())
              {
                fields[index2].SetValue(obj, (object) sharedVariable);
                GUI.set_changed(true);
              }
            }
            else
            {
              GUILayout.BeginHorizontal(new GUILayoutOption[0]);
              EditorGUI.BeginChangeCheck();
              if (drawWatch)
                this.DrawWatchedButton(task, fields[index2]);
              object obj2 = FieldInspector.DrawField(task, guiContent, fields[index2], obj1);
              if (BehaviorDesignerWindow.instance.ContainsError(task, fields[index2].Name))
              {
                GUILayout.Space(-3f);
                GUILayout.Box((Texture) BehaviorDesignerUtility.ErrorIconTexture, BehaviorDesignerUtility.PlainTextureGUIStyle, new GUILayoutOption[1]
                {
                  GUILayout.Width(20f)
                });
              }
              if (EditorGUI.EndChangeCheck())
              {
                fields[index2].SetValue(obj, obj2);
                GUI.set_changed(true);
              }
              if (TaskUtility.HasAttribute(fields[index2], typeof (RequiredFieldAttribute)) && !ErrorCheck.IsRequiredFieldValid(fieldType, obj1))
              {
                GUILayout.Space(-3f);
                GUILayout.Box((Texture) BehaviorDesignerUtility.ErrorIconTexture, BehaviorDesignerUtility.PlainTextureGUIStyle, new GUILayoutOption[1]
                {
                  GUILayout.Width(20f)
                });
              }
              GUILayout.EndHorizontal();
              GUILayout.Space(4f);
            }
          }
        }
      }
    }

    private bool DrawWatchedButton(Task task, FieldInfo field)
    {
      GUILayout.Space(3f);
      bool flag = task.get_NodeData().ContainsWatchedField(field);
      if (!GUILayout.Button((Texture) (!flag ? BehaviorDesignerUtility.VariableWatchButtonTexture : BehaviorDesignerUtility.VariableWatchButtonSelectedTexture), BehaviorDesignerUtility.PlainButtonGUIStyle, new GUILayoutOption[1]
      {
        GUILayout.Width(15f)
      }))
        return false;
      if (flag)
        task.get_NodeData().RemoveWatchedField(field);
      else
        task.get_NodeData().AddWatchedField(field);
      return true;
    }

    private void DrawTaskValue(
      BehaviorSource behaviorSource,
      TaskList taskList,
      FieldInfo field,
      GUIContent guiContent,
      Task parentTask,
      Task task,
      bool enabled)
    {
      if (BehaviorDesignerUtility.HasAttribute(field, typeof (InspectTaskAttribute)))
      {
        GUILayout.BeginHorizontal(new GUILayoutOption[0]);
        GUILayout.Label(guiContent, new GUILayoutOption[1]
        {
          GUILayout.Width(144f)
        });
        if (GUILayout.Button(task == null ? "Select" : BehaviorDesignerUtility.SplitCamelCase(((object) task).GetType().Name.ToString()), EditorStyles.get_toolbarPopup(), new GUILayoutOption[1]
        {
          GUILayout.Width(134f)
        }))
        {
          GenericMenu genericMenu = new GenericMenu();
          // ISSUE: method pointer
          genericMenu.AddItem(new GUIContent("None"), task == null, new GenericMenu.MenuFunction2((object) this, __methodptr(InspectedTaskCallback)), (object) null);
          // ISSUE: method pointer
          taskList.AddConditionalTasksToMenu(ref genericMenu, task == null ? (Type) null : ((object) task).GetType(), string.Empty, new GenericMenu.MenuFunction2((object) this, __methodptr(InspectedTaskCallback)));
          genericMenu.ShowAsContext();
          this.mActiveMenuSelectionTask = parentTask;
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(2f);
        this.DrawObjectFields(behaviorSource, taskList, task, (object) task, enabled, false);
      }
      else
      {
        GUILayout.BeginHorizontal(new GUILayoutOption[0]);
        this.DrawWatchedButton(parentTask, field);
        GUILayout.Label(guiContent, BehaviorDesignerUtility.TaskInspectorGUIStyle, new GUILayoutOption[1]
        {
          GUILayout.Width(165f)
        });
        bool flag = this.behaviorDesignerWindow.IsReferencingField(field);
        Color backgroundColor = GUI.get_backgroundColor();
        if (flag)
          GUI.set_backgroundColor(new Color(0.5f, 1f, 0.5f));
        if (GUILayout.Button(!flag ? "Select" : "Done", EditorStyles.get_miniButtonMid(), new GUILayoutOption[1]
        {
          GUILayout.Width(80f)
        }))
        {
          if (this.behaviorDesignerWindow.IsReferencingTasks() && !flag)
            this.behaviorDesignerWindow.ToggleReferenceTasks();
          this.behaviorDesignerWindow.ToggleReferenceTasks(parentTask, field);
        }
        GUI.set_backgroundColor(backgroundColor);
        EditorGUILayout.EndHorizontal();
        if (typeof (IList).IsAssignableFrom(field.FieldType))
        {
          IList list = field.GetValue((object) parentTask) as IList;
          if (list == null || list.Count == 0)
          {
            GUILayout.Label("No Tasks Referenced", BehaviorDesignerUtility.TaskInspectorGUIStyle, new GUILayoutOption[0]);
          }
          else
          {
            for (int index = 0; index < list.Count; ++index)
            {
              if (list[index] is Task)
              {
                EditorGUILayout.BeginHorizontal(new GUILayoutOption[0]);
                GUILayout.Label((list[index] as Task).get_NodeData().get_NodeDesigner().ToString(), BehaviorDesignerUtility.TaskInspectorGUIStyle, new GUILayoutOption[1]
                {
                  GUILayout.Width(232f)
                });
                if (GUILayout.Button((Texture) BehaviorDesignerUtility.DeleteButtonTexture, BehaviorDesignerUtility.PlainButtonGUIStyle, new GUILayoutOption[1]
                {
                  GUILayout.Width(14f)
                }))
                {
                  this.ReferenceTasks(parentTask, ((list[index] as Task).get_NodeData().get_NodeDesigner() as NodeDesigner).Task, field);
                  GUI.set_changed(true);
                }
                GUILayout.Space(3f);
                if (GUILayout.Button((Texture) BehaviorDesignerUtility.IdentifyButtonTexture, BehaviorDesignerUtility.PlainButtonGUIStyle, new GUILayoutOption[1]
                {
                  GUILayout.Width(14f)
                }))
                  this.behaviorDesignerWindow.IdentifyNode((list[index] as Task).get_NodeData().get_NodeDesigner() as NodeDesigner);
                EditorGUILayout.EndHorizontal();
              }
            }
          }
        }
        else
        {
          EditorGUILayout.BeginHorizontal(new GUILayoutOption[0]);
          Task task1 = field.GetValue((object) parentTask) as Task;
          GUILayout.Label(task1 == null ? "No Tasks Referenced" : task1.get_NodeData().get_NodeDesigner().ToString(), BehaviorDesignerUtility.TaskInspectorGUIStyle, new GUILayoutOption[1]
          {
            GUILayout.Width(232f)
          });
          if (task1 != null)
          {
            if (GUILayout.Button((Texture) BehaviorDesignerUtility.DeleteButtonTexture, BehaviorDesignerUtility.PlainButtonGUIStyle, new GUILayoutOption[1]
            {
              GUILayout.Width(14f)
            }))
            {
              this.ReferenceTasks(parentTask, (Task) null, field);
              GUI.set_changed(true);
            }
            GUILayout.Space(3f);
            if (GUILayout.Button((Texture) BehaviorDesignerUtility.IdentifyButtonTexture, BehaviorDesignerUtility.PlainButtonGUIStyle, new GUILayoutOption[1]
            {
              GUILayout.Width(14f)
            }))
              this.behaviorDesignerWindow.IdentifyNode(task1.get_NodeData().get_NodeDesigner() as NodeDesigner);
          }
          EditorGUILayout.EndHorizontal();
        }
      }
    }

    private SharedVariable DrawSharedVariableValue(
      BehaviorSource behaviorSource,
      FieldInfo field,
      GUIContent guiContent,
      Task task,
      SharedVariable sharedVariable,
      bool isReflectionTask,
      bool enabled,
      bool drawWatch)
    {
      if (isReflectionTask)
      {
        if (!field.FieldType.Equals(typeof (SharedVariable)) && sharedVariable == null)
        {
          sharedVariable = Activator.CreateInstance(field.FieldType) as SharedVariable;
          if (TaskUtility.HasAttribute(field, typeof (RequiredFieldAttribute)) || TaskUtility.HasAttribute(field, typeof (SharedRequiredAttribute)))
            sharedVariable.set_IsShared(true);
          GUI.set_changed(true);
        }
        if (sharedVariable.get_IsShared())
        {
          GUILayout.Label(guiContent, new GUILayoutOption[1]
          {
            GUILayout.Width(126f)
          });
          string[] names = (string[]) null;
          int globalStartIndex = -1;
          int variablesOfType = FieldInspector.GetVariablesOfType(((object) sharedVariable).GetType().GetProperty("Value").PropertyType, sharedVariable.get_IsGlobal(), sharedVariable.get_Name(), behaviorSource, out names, ref globalStartIndex, false);
          Color backgroundColor = GUI.get_backgroundColor();
          if (variablesOfType == 0 && !TaskUtility.HasAttribute(field, typeof (SharedRequiredAttribute)))
            GUI.set_backgroundColor(Color.get_red());
          int num = variablesOfType;
          int index = EditorGUILayout.Popup(variablesOfType, names, EditorStyles.get_toolbarPopup(), new GUILayoutOption[0]);
          GUI.set_backgroundColor(backgroundColor);
          if (index != num)
          {
            if (index == 0)
            {
              sharedVariable = !field.FieldType.Equals(typeof (SharedVariable)) ? Activator.CreateInstance(field.FieldType) as SharedVariable : Activator.CreateInstance(FieldInspector.FriendlySharedVariableName(((object) sharedVariable).GetType().GetProperty("Value").PropertyType)) as SharedVariable;
              sharedVariable.set_IsShared(true);
            }
            else
              sharedVariable = globalStartIndex == -1 || index < globalStartIndex ? behaviorSource.GetVariable(names[index]) : GlobalVariables.get_Instance().GetVariable(names[index].Substring(8, names[index].Length - 8));
          }
          GUILayout.Space(8f);
        }
        else
        {
          bool drawComponentField;
          if ((drawComponentField = field.Name.Equals("componentName")) || field.Name.Equals("methodName") || (field.Name.Equals("fieldName") || field.Name.Equals("propertyName")))
            this.DrawReflectionField(task, guiContent, drawComponentField, field);
          else
            FieldInspector.DrawFields(task, (object) sharedVariable, guiContent);
        }
        if (!TaskUtility.HasAttribute(field, typeof (RequiredFieldAttribute)) && !TaskUtility.HasAttribute(field, typeof (SharedRequiredAttribute)))
          sharedVariable = FieldInspector.DrawSharedVariableToggleSharedButton(sharedVariable);
        else if (!sharedVariable.get_IsShared())
          sharedVariable.set_IsShared(true);
      }
      else
        sharedVariable = FieldInspector.DrawSharedVariable((Task) null, guiContent, field, field.FieldType, sharedVariable);
      GUILayout.Space(8f);
      return sharedVariable;
    }

    private void InspectedTaskCallback(object obj)
    {
      if (this.mActiveMenuSelectionTask != null)
      {
        FieldInfo field = ((object) this.mActiveMenuSelectionTask).GetType().GetField("conditionalTask");
        if (obj == null)
        {
          field.SetValue((object) this.mActiveMenuSelectionTask, (object) null);
        }
        else
        {
          Type type = (Type) obj;
          Task instance1 = Activator.CreateInstance(type, true) as Task;
          field.SetValue((object) this.mActiveMenuSelectionTask, (object) instance1);
          FieldInfo[] allFields = TaskUtility.GetAllFields(type);
          for (int index = 0; index < allFields.Length; ++index)
          {
            if (allFields[index].FieldType.IsSubclassOf(typeof (SharedVariable)) && !BehaviorDesignerUtility.HasAttribute(allFields[index], typeof (HideInInspector)) && !BehaviorDesignerUtility.HasAttribute(allFields[index], typeof (NonSerializedAttribute)) && (!allFields[index].IsPrivate && !allFields[index].IsFamily || BehaviorDesignerUtility.HasAttribute(allFields[index], typeof (SerializeField))))
            {
              SharedVariable instance2 = Activator.CreateInstance(allFields[index].FieldType) as SharedVariable;
              instance2.set_IsShared(false);
              allFields[index].SetValue((object) instance1, (object) instance2);
            }
          }
        }
      }
      BehaviorDesignerWindow.instance.SaveBehavior();
    }

    public void SetActiveReferencedTasks(Task referenceTask, FieldInfo fieldInfo)
    {
      this.activeReferenceTask = referenceTask;
      this.activeReferenceTaskFieldInfo = fieldInfo;
    }

    public bool ReferenceTasks(Task referenceTask)
    {
      return this.ReferenceTasks(this.activeReferenceTask, referenceTask, this.activeReferenceTaskFieldInfo);
    }

    private bool ReferenceTasks(Task sourceTask, Task referenceTask, FieldInfo sourceFieldInfo)
    {
      bool fullSync = false;
      bool doReference = false;
      if (!TaskInspector.ReferenceTasks(sourceTask, referenceTask, sourceFieldInfo, ref fullSync, ref doReference, true, false))
        return false;
      if (referenceTask != null)
      {
        (referenceTask.get_NodeData().get_NodeDesigner() as NodeDesigner).ShowReferenceIcon = doReference;
        if (fullSync)
          this.PerformFullSync(this.activeReferenceTask);
      }
      return true;
    }

    public static bool ReferenceTasks(
      Task sourceTask,
      Task referenceTask,
      FieldInfo sourceFieldInfo,
      ref bool fullSync,
      ref bool doReference,
      bool synchronize,
      bool unreferenceAll)
    {
      if (referenceTask == null)
      {
        Task task = sourceFieldInfo.GetValue((object) sourceTask) as Task;
        if (task != null)
          (task.get_NodeData().get_NodeDesigner() as NodeDesigner).ShowReferenceIcon = false;
        sourceFieldInfo.SetValue((object) sourceTask, (object) null);
        return true;
      }
      if (((object) referenceTask).Equals((object) sourceTask) || sourceFieldInfo == null || !typeof (IList).IsAssignableFrom(sourceFieldInfo.FieldType) && !sourceFieldInfo.FieldType.IsAssignableFrom(((object) referenceTask).GetType()) || typeof (IList).IsAssignableFrom(sourceFieldInfo.FieldType) && (sourceFieldInfo.FieldType.IsGenericType && !sourceFieldInfo.FieldType.GetGenericArguments()[0].IsAssignableFrom(((object) referenceTask).GetType()) || !sourceFieldInfo.FieldType.IsGenericType && !sourceFieldInfo.FieldType.GetElementType().IsAssignableFrom(((object) referenceTask).GetType())))
        return false;
      if (synchronize && !TaskInspector.IsFieldLinked(sourceFieldInfo))
        synchronize = false;
      if (unreferenceAll)
      {
        sourceFieldInfo.SetValue((object) sourceTask, (object) null);
        (sourceTask.get_NodeData().get_NodeDesigner() as NodeDesigner).ShowReferenceIcon = false;
      }
      else
      {
        doReference = true;
        bool flag = false;
        if (typeof (IList).IsAssignableFrom(sourceFieldInfo.FieldType))
        {
          Task[] taskArray1 = sourceFieldInfo.GetValue((object) sourceTask) as Task[];
          Type type1;
          if (sourceFieldInfo.FieldType.IsArray)
          {
            type1 = sourceFieldInfo.FieldType.GetElementType();
          }
          else
          {
            Type type2 = sourceFieldInfo.FieldType;
            while (!type2.IsGenericType)
              type2 = type2.BaseType;
            type1 = type2.GetGenericArguments()[0];
          }
          IList instance1 = Activator.CreateInstance(typeof (List<>).MakeGenericType(type1)) as IList;
          if (taskArray1 != null)
          {
            for (int index = 0; index < taskArray1.Length; ++index)
            {
              if (((object) referenceTask).Equals((object) taskArray1[index]))
                doReference = false;
              else
                instance1.Add((object) taskArray1[index]);
            }
          }
          if (synchronize)
          {
            if (taskArray1 != null && taskArray1.Length > 0)
            {
              for (int index = 0; index < taskArray1.Length; ++index)
              {
                TaskInspector.ReferenceTasks(taskArray1[index], referenceTask, ((object) taskArray1[index]).GetType().GetField(sourceFieldInfo.Name), ref flag, ref doReference, false, false);
                if (doReference)
                  TaskInspector.ReferenceTasks(referenceTask, taskArray1[index], ((object) referenceTask).GetType().GetField(sourceFieldInfo.Name), ref flag, ref doReference, false, false);
              }
            }
            else if (doReference)
            {
              FieldInfo field = ((object) referenceTask).GetType().GetField(sourceFieldInfo.Name);
              if (field != null)
              {
                Task[] taskArray2 = field.GetValue((object) referenceTask) as Task[];
                if (taskArray2 != null)
                {
                  for (int index = 0; index < taskArray2.Length; ++index)
                  {
                    instance1.Add((object) taskArray2[index]);
                    (taskArray2[index].get_NodeData().get_NodeDesigner() as NodeDesigner).ShowReferenceIcon = true;
                    TaskInspector.ReferenceTasks(taskArray2[index], sourceTask, ((object) taskArray2[index]).GetType().GetField(sourceFieldInfo.Name), ref doReference, ref flag, false, false);
                  }
                  doReference = true;
                }
              }
            }
            TaskInspector.ReferenceTasks(referenceTask, sourceTask, ((object) referenceTask).GetType().GetField(sourceFieldInfo.Name), ref flag, ref doReference, false, !doReference);
          }
          if (doReference)
            instance1.Add((object) referenceTask);
          if (sourceFieldInfo.FieldType.IsArray)
          {
            Array instance2 = Array.CreateInstance(sourceFieldInfo.FieldType.GetElementType(), instance1.Count);
            instance1.CopyTo(instance2, 0);
            sourceFieldInfo.SetValue((object) sourceTask, (object) instance2);
          }
          else
            sourceFieldInfo.SetValue((object) sourceTask, (object) instance1);
        }
        else
        {
          Task sourceTask1 = sourceFieldInfo.GetValue((object) sourceTask) as Task;
          doReference = !((object) referenceTask).Equals((object) sourceTask1);
          if (TaskInspector.IsFieldLinked(sourceFieldInfo) && sourceTask1 != null)
            TaskInspector.ReferenceTasks(sourceTask1, sourceTask, ((object) sourceTask1).GetType().GetField(sourceFieldInfo.Name), ref flag, ref doReference, false, true);
          if (synchronize)
            TaskInspector.ReferenceTasks(referenceTask, sourceTask, ((object) referenceTask).GetType().GetField(sourceFieldInfo.Name), ref flag, ref doReference, false, !doReference);
          sourceFieldInfo.SetValue((object) sourceTask, !doReference ? (object) (Task) null : (object) referenceTask);
        }
        if (synchronize)
          (referenceTask.get_NodeData().get_NodeDesigner() as NodeDesigner).ShowReferenceIcon = doReference;
        fullSync = doReference && synchronize;
      }
      return true;
    }

    public bool IsActiveTaskArray()
    {
      return this.activeReferenceTaskFieldInfo.FieldType.IsArray;
    }

    public bool IsActiveTaskNull()
    {
      return this.activeReferenceTaskFieldInfo.GetValue((object) this.activeReferenceTask) == null;
    }

    public static bool IsFieldLinked(FieldInfo field)
    {
      return BehaviorDesignerUtility.HasAttribute(field, typeof (LinkedTaskAttribute));
    }

    public static List<Task> GetReferencedTasks(Task task)
    {
      List<Task> taskList = new List<Task>();
      FieldInfo[] allFields = TaskUtility.GetAllFields(((object) task).GetType());
      for (int index1 = 0; index1 < allFields.Length; ++index1)
      {
        if (!allFields[index1].IsPrivate && !allFields[index1].IsFamily || BehaviorDesignerUtility.HasAttribute(allFields[index1], typeof (SerializeField)))
        {
          if (typeof (IList).IsAssignableFrom(allFields[index1].FieldType) && (typeof (Task).IsAssignableFrom(allFields[index1].FieldType.GetElementType()) || allFields[index1].FieldType.IsGenericType && typeof (Task).IsAssignableFrom(allFields[index1].FieldType.GetGenericArguments()[0])))
          {
            Task[] taskArray = allFields[index1].GetValue((object) task) as Task[];
            if (taskArray != null)
            {
              for (int index2 = 0; index2 < taskArray.Length; ++index2)
                taskList.Add(taskArray[index2]);
            }
          }
          else if (allFields[index1].FieldType.IsSubclassOf(typeof (Task)) && allFields[index1].GetValue((object) task) != null)
            taskList.Add(allFields[index1].GetValue((object) task) as Task);
        }
      }
      if (taskList.Count > 0)
        return taskList;
      return (List<Task>) null;
    }

    private void PerformFullSync(Task task)
    {
      List<Task> referencedTasks = TaskInspector.GetReferencedTasks(task);
      if (referencedTasks == null)
        return;
      FieldInfo[] allFields = TaskUtility.GetAllFields(((object) task).GetType());
      for (int index1 = 0; index1 < allFields.Length; ++index1)
      {
        if (!TaskInspector.IsFieldLinked(allFields[index1]))
        {
          for (int index2 = 0; index2 < referencedTasks.Count; ++index2)
          {
            FieldInfo field;
            if ((field = ((object) referencedTasks[index2]).GetType().GetField(allFields[index1].Name)) != null)
              field.SetValue((object) referencedTasks[index2], allFields[index1].GetValue((object) task));
          }
        }
      }
    }

    public static void OpenInFileEditor(object task)
    {
      MonoScript[] objectsOfTypeAll = (MonoScript[]) Resources.FindObjectsOfTypeAll(typeof (MonoScript));
      for (int index = 0; index < objectsOfTypeAll.Length; ++index)
      {
        if (Object.op_Inequality((Object) objectsOfTypeAll[index], (Object) null) && objectsOfTypeAll[index].GetClass() != null && objectsOfTypeAll[index].GetClass().Equals(task.GetType()))
        {
          AssetDatabase.OpenAsset((Object) objectsOfTypeAll[index]);
          break;
        }
      }
    }

    public static void SelectInProject(object task)
    {
      MonoScript[] objectsOfTypeAll = (MonoScript[]) Resources.FindObjectsOfTypeAll(typeof (MonoScript));
      for (int index = 0; index < objectsOfTypeAll.Length; ++index)
      {
        if (Object.op_Inequality((Object) objectsOfTypeAll[index], (Object) null) && objectsOfTypeAll[index].GetClass() != null && objectsOfTypeAll[index].GetClass().Equals(task.GetType()))
        {
          Selection.set_activeObject((Object) objectsOfTypeAll[index]);
          break;
        }
      }
    }

    private void ResetTask(object task)
    {
      (task as Task).OnReset();
      List<Type> baseClasses = FieldInspector.GetBaseClasses(task.GetType());
      BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
      for (int index1 = baseClasses.Count - 1; index1 > -1; --index1)
      {
        FieldInfo[] fields = baseClasses[index1].GetFields(bindingAttr);
        for (int index2 = 0; index2 < fields.Length; ++index2)
        {
          if (typeof (SharedVariable).IsAssignableFrom(fields[index2].FieldType))
          {
            SharedVariable sharedVariable = fields[index2].GetValue(task) as SharedVariable;
            if (TaskUtility.HasAttribute(fields[index2], typeof (RequiredFieldAttribute)) && sharedVariable != null && !sharedVariable.get_IsShared())
              sharedVariable.set_IsShared(true);
          }
        }
      }
    }

    private void AddColorMenuItem(ref GenericMenu menu, Task task, string color, int index)
    {
      // ISSUE: method pointer
      menu.AddItem(new GUIContent(color), task.get_NodeData().get_ColorIndex() == index, new GenericMenu.MenuFunction2((object) this, __methodptr(SetTaskColor)), (object) new TaskInspector.TaskColor(task, index));
    }

    private void SetTaskColor(object value)
    {
      TaskInspector.TaskColor taskColor = value as TaskInspector.TaskColor;
      if (taskColor.task.get_NodeData().get_ColorIndex() == taskColor.colorIndex)
        return;
      taskColor.task.get_NodeData().set_ColorIndex(taskColor.colorIndex);
      BehaviorDesignerWindow.instance.SaveBehavior();
    }

    private void OpenHelpURL(Task task)
    {
      HelpURLAttribute[] customAttributes;
      if ((customAttributes = ((object) task).GetType().GetCustomAttributes(typeof (HelpURLAttribute), false) as HelpURLAttribute[]).Length <= 0)
        return;
      Application.OpenURL(customAttributes[0].get_URL());
    }

    private bool IsReflectionTask(Type type)
    {
      if (!this.IsInvokeMethodTask(type) && !this.IsFieldReflectionTask(type))
        return this.IsPropertyReflectionTask(type);
      return true;
    }

    private bool IsInvokeMethodTask(Type type)
    {
      return TaskUtility.CompareType(type, "BehaviorDesigner.Runtime.Tasks.InvokeMethod");
    }

    private bool IsFieldReflectionTask(Type type)
    {
      if (!TaskUtility.CompareType(type, "BehaviorDesigner.Runtime.Tasks.GetFieldValue") && !TaskUtility.CompareType(type, "BehaviorDesigner.Runtime.Tasks.SetFieldValue"))
        return TaskUtility.CompareType(type, "BehaviorDesigner.Runtime.Tasks.CompareFieldValue");
      return true;
    }

    private bool IsPropertyReflectionTask(Type type)
    {
      if (!TaskUtility.CompareType(type, "BehaviorDesigner.Runtime.Tasks.GetPropertyValue") && !TaskUtility.CompareType(type, "BehaviorDesigner.Runtime.Tasks.SetPropertyValue"))
        return TaskUtility.CompareType(type, "BehaviorDesigner.Runtime.Tasks.ComparePropertyValue");
      return true;
    }

    private bool IsReflectionGetterTask(Type type)
    {
      if (!TaskUtility.CompareType(type, "BehaviorDesigner.Runtime.Tasks.GetFieldValue"))
        return TaskUtility.CompareType(type, "BehaviorDesigner.Runtime.Tasks.GetPropertyValue");
      return true;
    }

    private void DrawReflectionField(
      Task task,
      GUIContent guiContent,
      bool drawComponentField,
      FieldInfo field)
    {
      SharedVariable sharedVariable1 = ((object) task).GetType().GetField("targetGameObject").GetValue((object) task) as SharedVariable;
      if (drawComponentField)
      {
        GUILayout.Label(guiContent, new GUILayoutOption[1]
        {
          GUILayout.Width(146f)
        });
        SharedVariable sharedVariable2 = field.GetValue((object) task) as SharedVariable;
        string empty = string.Empty;
        string str;
        if (string.IsNullOrEmpty((string) sharedVariable2.GetValue()))
        {
          str = "Select";
        }
        else
        {
          string[] strArray = ((string) sharedVariable2.GetValue()).Split('.');
          str = strArray[strArray.Length - 1];
        }
        if (GUILayout.Button(str, EditorStyles.get_toolbarPopup(), new GUILayoutOption[1]
        {
          GUILayout.Width(92f)
        }))
        {
          GenericMenu genericMenu = new GenericMenu();
          // ISSUE: method pointer
          genericMenu.AddItem(new GUIContent("None"), string.IsNullOrEmpty((string) sharedVariable2.GetValue()), new GenericMenu.MenuFunction2((object) this, __methodptr(ComponentSelectionCallback)), (object) null);
          GameObject gameObject = (GameObject) null;
          if (sharedVariable1 == null || Object.op_Equality((Object) sharedVariable1.GetValue(), (Object) null))
          {
            if (Object.op_Inequality((Object) task.get_Owner(), (Object) null))
              gameObject = ((Component) task.get_Owner()).get_gameObject();
          }
          else
            gameObject = (GameObject) sharedVariable1.GetValue();
          if (Object.op_Inequality((Object) gameObject, (Object) null))
          {
            Component[] components = (Component[]) gameObject.GetComponents<Component>();
            for (int index = 0; index < components.Length; ++index)
            {
              // ISSUE: method pointer
              genericMenu.AddItem(new GUIContent(((object) components[index]).GetType().Name), ((object) components[index]).GetType().FullName.Equals((string) sharedVariable2.GetValue()), new GenericMenu.MenuFunction2((object) this, __methodptr(ComponentSelectionCallback)), (object) ((object) components[index]).GetType().FullName);
            }
            genericMenu.ShowAsContext();
            this.mActiveMenuSelectionTask = task;
          }
        }
      }
      else
      {
        GUILayout.Label(guiContent, new GUILayoutOption[1]
        {
          GUILayout.Width(146f)
        });
        SharedVariable sharedVariable2 = ((object) task).GetType().GetField("componentName").GetValue((object) task) as SharedVariable;
        SharedVariable sharedVariable3 = field.GetValue((object) task) as SharedVariable;
        string empty = string.Empty;
        if (GUILayout.Button(!string.IsNullOrEmpty((string) sharedVariable2.GetValue()) ? (!string.IsNullOrEmpty((string) sharedVariable3.GetValue()) ? (string) sharedVariable3.GetValue() : "Select") : "Component Required", EditorStyles.get_toolbarPopup(), new GUILayoutOption[1]
        {
          GUILayout.Width(92f)
        }) && !string.IsNullOrEmpty((string) sharedVariable2.GetValue()))
        {
          GenericMenu genericMenu = new GenericMenu();
          // ISSUE: method pointer
          genericMenu.AddItem(new GUIContent("None"), string.IsNullOrEmpty((string) sharedVariable3.GetValue()), new GenericMenu.MenuFunction2((object) this, __methodptr(SecondaryReflectionSelectionCallback)), (object) null);
          GameObject gameObject = (GameObject) null;
          if (sharedVariable1 == null || Object.op_Equality((Object) sharedVariable1.GetValue(), (Object) null))
          {
            if (Object.op_Inequality((Object) task.get_Owner(), (Object) null))
              gameObject = ((Component) task.get_Owner()).get_gameObject();
          }
          else
            gameObject = (GameObject) sharedVariable1.GetValue();
          if (Object.op_Inequality((Object) gameObject, (Object) null))
          {
            Component component = gameObject.GetComponent(TaskUtility.GetTypeWithinAssembly((string) sharedVariable2.GetValue()));
            List<Type> sharedVariableTypes = VariableInspector.FindAllSharedVariableTypes(false);
            if (this.IsInvokeMethodTask(((object) task).GetType()))
            {
              MethodInfo[] methods = ((object) component).GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public);
              for (int index1 = 0; index1 < methods.Length; ++index1)
              {
                if (!methods[index1].IsSpecialName && !methods[index1].IsGenericMethod && methods[index1].GetParameters().Length <= 4)
                {
                  ParameterInfo[] parameters = methods[index1].GetParameters();
                  bool flag = true;
                  for (int index2 = 0; index2 < parameters.Length; ++index2)
                  {
                    if (!this.SharedVariableTypeExists(sharedVariableTypes, parameters[index2].ParameterType))
                    {
                      flag = false;
                      break;
                    }
                  }
                  if (flag && (methods[index1].ReturnType.Equals(typeof (void)) || this.SharedVariableTypeExists(sharedVariableTypes, methods[index1].ReturnType)))
                  {
                    // ISSUE: method pointer
                    genericMenu.AddItem(new GUIContent(methods[index1].Name), methods[index1].Name.Equals((string) sharedVariable3.GetValue()), new GenericMenu.MenuFunction2((object) this, __methodptr(SecondaryReflectionSelectionCallback)), (object) methods[index1]);
                  }
                }
              }
            }
            else if (this.IsFieldReflectionTask(((object) task).GetType()))
            {
              FieldInfo[] fields = ((object) component).GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
              for (int index = 0; index < fields.Length; ++index)
              {
                if (!fields[index].IsSpecialName && this.SharedVariableTypeExists(sharedVariableTypes, fields[index].FieldType))
                {
                  // ISSUE: method pointer
                  genericMenu.AddItem(new GUIContent(fields[index].Name), fields[index].Name.Equals((string) sharedVariable3.GetValue()), new GenericMenu.MenuFunction2((object) this, __methodptr(SecondaryReflectionSelectionCallback)), (object) fields[index]);
                }
              }
            }
            else
            {
              PropertyInfo[] properties = ((object) component).GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
              for (int index = 0; index < properties.Length; ++index)
              {
                if (!properties[index].IsSpecialName && this.SharedVariableTypeExists(sharedVariableTypes, properties[index].PropertyType))
                {
                  // ISSUE: method pointer
                  genericMenu.AddItem(new GUIContent(properties[index].Name), properties[index].Name.Equals((string) sharedVariable3.GetValue()), new GenericMenu.MenuFunction2((object) this, __methodptr(SecondaryReflectionSelectionCallback)), (object) properties[index]);
                }
              }
            }
            genericMenu.ShowAsContext();
            this.mActiveMenuSelectionTask = task;
          }
        }
      }
      GUILayout.Space(8f);
    }

    private void ComponentSelectionCallback(object obj)
    {
      if (this.mActiveMenuSelectionTask != null)
      {
        FieldInfo field1 = ((object) this.mActiveMenuSelectionTask).GetType().GetField("componentName");
        SharedVariable instance1 = Activator.CreateInstance(TaskUtility.GetTypeWithinAssembly("BehaviorDesigner.Runtime.SharedString")) as SharedVariable;
        if (obj == null)
        {
          field1.SetValue((object) this.mActiveMenuSelectionTask, (object) instance1);
          SharedVariable instance2 = Activator.CreateInstance(TaskUtility.GetTypeWithinAssembly("BehaviorDesigner.Runtime.SharedString")) as SharedVariable;
          FieldInfo fieldInfo;
          if (this.IsInvokeMethodTask(((object) this.mActiveMenuSelectionTask).GetType()))
          {
            fieldInfo = ((object) this.mActiveMenuSelectionTask).GetType().GetField("methodName");
            this.ClearInvokeVariablesTask();
          }
          else
            fieldInfo = !this.IsFieldReflectionTask(((object) this.mActiveMenuSelectionTask).GetType()) ? ((object) this.mActiveMenuSelectionTask).GetType().GetField("propertyName") : ((object) this.mActiveMenuSelectionTask).GetType().GetField("fieldName");
          fieldInfo.SetValue((object) this.mActiveMenuSelectionTask, (object) instance2);
        }
        else
        {
          string str = (string) obj;
          SharedVariable sharedVariable = field1.GetValue((object) this.mActiveMenuSelectionTask) as SharedVariable;
          if (!str.Equals((string) sharedVariable.GetValue()))
          {
            FieldInfo field2;
            FieldInfo fieldInfo;
            if (this.IsInvokeMethodTask(((object) this.mActiveMenuSelectionTask).GetType()))
            {
              field2 = ((object) this.mActiveMenuSelectionTask).GetType().GetField("methodName");
              for (int index = 0; index < 4; ++index)
                ((object) this.mActiveMenuSelectionTask).GetType().GetField("parameter" + (object) (index + 1)).SetValue((object) this.mActiveMenuSelectionTask, (object) null);
              fieldInfo = ((object) this.mActiveMenuSelectionTask).GetType().GetField("storeResult");
            }
            else if (this.IsFieldReflectionTask(((object) this.mActiveMenuSelectionTask).GetType()))
            {
              field2 = ((object) this.mActiveMenuSelectionTask).GetType().GetField("fieldName");
              fieldInfo = ((object) this.mActiveMenuSelectionTask).GetType().GetField("fieldValue") ?? ((object) this.mActiveMenuSelectionTask).GetType().GetField("compareValue");
            }
            else
            {
              field2 = ((object) this.mActiveMenuSelectionTask).GetType().GetField("propertyName");
              fieldInfo = ((object) this.mActiveMenuSelectionTask).GetType().GetField("propertyValue") ?? ((object) this.mActiveMenuSelectionTask).GetType().GetField("compareValue");
            }
            field2.SetValue((object) this.mActiveMenuSelectionTask, (object) instance1);
            fieldInfo.SetValue((object) this.mActiveMenuSelectionTask, (object) null);
          }
          SharedVariable instance2 = Activator.CreateInstance(TaskUtility.GetTypeWithinAssembly("BehaviorDesigner.Runtime.SharedString")) as SharedVariable;
          instance2.SetValue((object) str);
          field1.SetValue((object) this.mActiveMenuSelectionTask, (object) instance2);
        }
      }
      BehaviorDesignerWindow.instance.SaveBehavior();
    }

    private void SecondaryReflectionSelectionCallback(object obj)
    {
      if (this.mActiveMenuSelectionTask != null)
      {
        SharedVariable instance1 = Activator.CreateInstance(TaskUtility.GetTypeWithinAssembly("BehaviorDesigner.Runtime.SharedString")) as SharedVariable;
        FieldInfo fieldInfo1;
        if (this.IsInvokeMethodTask(((object) this.mActiveMenuSelectionTask).GetType()))
        {
          this.ClearInvokeVariablesTask();
          fieldInfo1 = ((object) this.mActiveMenuSelectionTask).GetType().GetField("methodName");
        }
        else
          fieldInfo1 = !this.IsFieldReflectionTask(((object) this.mActiveMenuSelectionTask).GetType()) ? ((object) this.mActiveMenuSelectionTask).GetType().GetField("propertyName") : ((object) this.mActiveMenuSelectionTask).GetType().GetField("fieldName");
        if (obj == null)
          fieldInfo1.SetValue((object) this.mActiveMenuSelectionTask, (object) instance1);
        else if (this.IsInvokeMethodTask(((object) this.mActiveMenuSelectionTask).GetType()))
        {
          MethodInfo methodInfo = (MethodInfo) obj;
          instance1.SetValue((object) methodInfo.Name);
          fieldInfo1.SetValue((object) this.mActiveMenuSelectionTask, (object) instance1);
          ParameterInfo[] parameters = methodInfo.GetParameters();
          for (int index = 0; index < 4; ++index)
          {
            FieldInfo field = ((object) this.mActiveMenuSelectionTask).GetType().GetField("parameter" + (object) (index + 1));
            if (index < parameters.Length)
            {
              SharedVariable instance2 = Activator.CreateInstance(FieldInspector.FriendlySharedVariableName(parameters[index].ParameterType)) as SharedVariable;
              field.SetValue((object) this.mActiveMenuSelectionTask, (object) instance2);
            }
            else
              field.SetValue((object) this.mActiveMenuSelectionTask, (object) null);
          }
          if (!methodInfo.ReturnType.Equals(typeof (void)))
          {
            FieldInfo field = ((object) this.mActiveMenuSelectionTask).GetType().GetField("storeResult");
            SharedVariable instance2 = Activator.CreateInstance(FieldInspector.FriendlySharedVariableName(methodInfo.ReturnType)) as SharedVariable;
            instance2.set_IsShared(true);
            field.SetValue((object) this.mActiveMenuSelectionTask, (object) instance2);
          }
        }
        else if (this.IsFieldReflectionTask(((object) this.mActiveMenuSelectionTask).GetType()))
        {
          FieldInfo fieldInfo2 = (FieldInfo) obj;
          instance1.SetValue((object) fieldInfo2.Name);
          fieldInfo1.SetValue((object) this.mActiveMenuSelectionTask, (object) instance1);
          FieldInfo fieldInfo3 = ((object) this.mActiveMenuSelectionTask).GetType().GetField("fieldValue") ?? ((object) this.mActiveMenuSelectionTask).GetType().GetField("compareValue");
          SharedVariable instance2 = Activator.CreateInstance(FieldInspector.FriendlySharedVariableName(fieldInfo2.FieldType)) as SharedVariable;
          instance2.set_IsShared(this.IsReflectionGetterTask(((object) this.mActiveMenuSelectionTask).GetType()));
          fieldInfo3.SetValue((object) this.mActiveMenuSelectionTask, (object) instance2);
        }
        else
        {
          PropertyInfo propertyInfo = (PropertyInfo) obj;
          instance1.SetValue((object) propertyInfo.Name);
          fieldInfo1.SetValue((object) this.mActiveMenuSelectionTask, (object) instance1);
          FieldInfo fieldInfo2 = ((object) this.mActiveMenuSelectionTask).GetType().GetField("propertyValue") ?? ((object) this.mActiveMenuSelectionTask).GetType().GetField("compareValue");
          SharedVariable instance2 = Activator.CreateInstance(FieldInspector.FriendlySharedVariableName(propertyInfo.PropertyType)) as SharedVariable;
          instance2.set_IsShared(this.IsReflectionGetterTask(((object) this.mActiveMenuSelectionTask).GetType()));
          fieldInfo2.SetValue((object) this.mActiveMenuSelectionTask, (object) instance2);
        }
      }
      BehaviorDesignerWindow.instance.SaveBehavior();
    }

    private void ClearInvokeVariablesTask()
    {
      for (int index = 0; index < 4; ++index)
        ((object) this.mActiveMenuSelectionTask).GetType().GetField("parameter" + (object) (index + 1)).SetValue((object) this.mActiveMenuSelectionTask, (object) null);
      ((object) this.mActiveMenuSelectionTask).GetType().GetField("storeResult").SetValue((object) this.mActiveMenuSelectionTask, (object) null);
    }

    private bool CanDrawReflectedField(object task, FieldInfo field)
    {
      if (!field.Name.Contains("parameter") && !field.Name.Contains("storeResult") && (!field.Name.Contains("fieldValue") && !field.Name.Contains("propertyValue")) && !field.Name.Contains("compareValue"))
        return true;
      if (this.IsInvokeMethodTask(task.GetType()))
      {
        if (field.Name.Contains("parameter"))
          return task.GetType().GetField(field.Name).GetValue(task) != null;
        MethodInfo invokeMethodInfo;
        if ((invokeMethodInfo = this.GetInvokeMethodInfo(task)) == null)
          return false;
        if (field.Name.Equals("storeResult"))
          return !invokeMethodInfo.ReturnType.Equals(typeof (void));
        return true;
      }
      if (this.IsFieldReflectionTask(task.GetType()))
      {
        SharedVariable sharedVariable = task.GetType().GetField("fieldName").GetValue(task) as SharedVariable;
        if (sharedVariable != null)
          return !string.IsNullOrEmpty((string) sharedVariable.GetValue());
        return false;
      }
      SharedVariable sharedVariable1 = task.GetType().GetField("propertyName").GetValue(task) as SharedVariable;
      if (sharedVariable1 != null)
        return !string.IsNullOrEmpty((string) sharedVariable1.GetValue());
      return false;
    }

    private string InvokeParameterName(object task, FieldInfo field)
    {
      if (!field.Name.Contains("parameter"))
        return field.Name;
      MethodInfo invokeMethodInfo;
      if ((invokeMethodInfo = this.GetInvokeMethodInfo(task)) == null)
        return field.Name;
      ParameterInfo[] parameters = invokeMethodInfo.GetParameters();
      int index = int.Parse(field.Name.Substring(9)) - 1;
      if (index < parameters.Length)
        return parameters[index].Name;
      return field.Name;
    }

    private MethodInfo GetInvokeMethodInfo(object task)
    {
      SharedVariable sharedVariable1 = task.GetType().GetField("targetGameObject").GetValue(task) as SharedVariable;
      GameObject gameObject = (GameObject) null;
      if (sharedVariable1 == null || Object.op_Equality((Object) sharedVariable1.GetValue(), (Object) null))
      {
        if (Object.op_Inequality((Object) (task as Task).get_Owner(), (Object) null))
          gameObject = ((Component) (task as Task).get_Owner()).get_gameObject();
      }
      else
        gameObject = (GameObject) sharedVariable1.GetValue();
      if (Object.op_Equality((Object) gameObject, (Object) null))
        return (MethodInfo) null;
      SharedVariable sharedVariable2 = task.GetType().GetField("componentName").GetValue(task) as SharedVariable;
      if (sharedVariable2 == null || string.IsNullOrEmpty((string) sharedVariable2.GetValue()))
        return (MethodInfo) null;
      SharedVariable sharedVariable3 = task.GetType().GetField("methodName").GetValue(task) as SharedVariable;
      if (sharedVariable3 == null || string.IsNullOrEmpty((string) sharedVariable3.GetValue()))
        return (MethodInfo) null;
      List<Type> typeList = new List<Type>();
      for (int index = 0; index < 4 && task.GetType().GetField("parameter" + (object) (index + 1)).GetValue(task) is SharedVariable sharedVariable4; ++index)
        typeList.Add(((object) sharedVariable4).GetType().GetProperty("Value").PropertyType);
      Component component = gameObject.GetComponent(TaskUtility.GetTypeWithinAssembly((string) sharedVariable2.GetValue()));
      if (Object.op_Equality((Object) component, (Object) null))
        return (MethodInfo) null;
      return ((object) component).GetType().GetMethod((string) sharedVariable3.GetValue(), typeList.ToArray());
    }

    private bool SharedVariableTypeExists(List<Type> sharedVariableTypes, Type type)
    {
      for (int index = 0; index < sharedVariableTypes.Count; ++index)
      {
        if (FieldInspector.FriendlySharedVariableName(type).Equals(sharedVariableTypes[index]))
          return true;
      }
      return false;
    }

    private class TaskColor
    {
      public Task task;
      public int colorIndex;

      public TaskColor(Task task, int colorIndex)
      {
        this.task = task;
        this.colorIndex = colorIndex;
      }
    }
  }
}
