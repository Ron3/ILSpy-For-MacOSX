// Decompiled with JetBrains decompiler
// Type: BehaviorDesigner.Editor.VariableSynchronizerInspector
// Assembly: BehaviorDesignerEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 99CE4D00-DFA2-42D1-ABFC-D630AB4C1372
// Assembly location: C:\Users\Ron\Desktop\bt\BehaviorDesignerEditor.dll

using BehaviorDesigner.Runtime;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace BehaviorDesigner.Editor
{
  [CustomEditor(typeof (VariableSynchronizer))]
  public class VariableSynchronizerInspector : UnityEditor.Editor
  {
    [SerializeField]
    private VariableSynchronizerInspector.Synchronizer sharedVariableSynchronizer;
    [SerializeField]
    private string sharedVariableValueTypeName;
    private Type sharedVariableValueType;
    [SerializeField]
    private VariableSynchronizer.SynchronizationType synchronizationType;
    [SerializeField]
    private bool setVariable;
    [SerializeField]
    private VariableSynchronizerInspector.Synchronizer targetSynchronizer;
    private Action<VariableSynchronizerInspector.Synchronizer, Type> thirdPartySynchronizer;
    private Type playMakerSynchronizationType;
    private Type uFrameSynchronizationType;

    public VariableSynchronizerInspector()
    {
      //base.\u002Ector();
    }

    public virtual void OnInspectorGUI()
    {
      VariableSynchronizer target = this.get_target() as VariableSynchronizer;
      if (Object.op_Equality((Object) target, (Object) null))
        return;
      GUILayout.Space(5f);
      target.set_UpdateInterval((UpdateIntervalType) EditorGUILayout.EnumPopup("Update Interval", (Enum) (object) target.get_UpdateInterval(), new GUILayoutOption[0]));
      if (target.get_UpdateInterval() == 1)
        target.set_UpdateIntervalSeconds(EditorGUILayout.FloatField("Seconds", target.get_UpdateIntervalSeconds(), new GUILayoutOption[0]));
      GUILayout.Space(5f);
      GUI.set_enabled(!Application.get_isPlaying());
      this.DrawSharedVariableSynchronizer(this.sharedVariableSynchronizer, (Type) null);
      if (string.IsNullOrEmpty(this.sharedVariableSynchronizer.targetName))
      {
        this.DrawSynchronizedVariables(target);
      }
      else
      {
        EditorGUILayout.BeginHorizontal(new GUILayoutOption[0]);
        EditorGUILayout.LabelField("Direction", new GUILayoutOption[1]
        {
          GUILayout.MaxWidth(146f)
        });
        if (GUILayout.Button((Texture) BehaviorDesignerUtility.LoadTexture(!this.setVariable ? "RightArrowButton.png" : "LeftArrowButton.png", true, (Object) this), BehaviorDesignerUtility.ButtonGUIStyle, new GUILayoutOption[1]
        {
          GUILayout.Width(22f)
        }))
          this.setVariable = !this.setVariable;
        EditorGUILayout.EndHorizontal();
        EditorGUI.BeginChangeCheck();
        this.synchronizationType = (VariableSynchronizer.SynchronizationType) EditorGUILayout.EnumPopup("Type", (Enum) (object) this.synchronizationType, new GUILayoutOption[0]);
        if (EditorGUI.EndChangeCheck())
          this.targetSynchronizer = new VariableSynchronizerInspector.Synchronizer();
        if (this.targetSynchronizer == null)
          this.targetSynchronizer = new VariableSynchronizerInspector.Synchronizer();
        if (this.sharedVariableValueType == null && !string.IsNullOrEmpty(this.sharedVariableValueTypeName))
          this.sharedVariableValueType = TaskUtility.GetTypeWithinAssembly(this.sharedVariableValueTypeName);
        switch ((int) this.synchronizationType)
        {
          case 0:
            this.DrawSharedVariableSynchronizer(this.targetSynchronizer, this.sharedVariableValueType);
            break;
          case 1:
            this.DrawPropertySynchronizer(this.targetSynchronizer, this.sharedVariableValueType);
            break;
          case 2:
            this.DrawAnimatorSynchronizer(this.targetSynchronizer);
            break;
          case 3:
            this.DrawPlayMakerSynchronizer(this.targetSynchronizer, this.sharedVariableValueType);
            break;
          case 4:
            this.DrawuFrameSynchronizer(this.targetSynchronizer, this.sharedVariableValueType);
            break;
        }
        if (string.IsNullOrEmpty(this.targetSynchronizer.targetName))
          GUI.set_enabled(false);
        if (GUILayout.Button("Add", new GUILayoutOption[0]))
        {
          VariableSynchronizer.SynchronizedVariable synchronizedVariable = new VariableSynchronizer.SynchronizedVariable(this.synchronizationType, this.setVariable, this.sharedVariableSynchronizer.component as Behavior, this.sharedVariableSynchronizer.targetName, this.sharedVariableSynchronizer.global, this.targetSynchronizer.component, this.targetSynchronizer.targetName, this.targetSynchronizer.global);
          target.get_SynchronizedVariables().Add(synchronizedVariable);
          BehaviorDesignerUtility.SetObjectDirty((Object) target);
          this.sharedVariableSynchronizer = new VariableSynchronizerInspector.Synchronizer();
          this.targetSynchronizer = new VariableSynchronizerInspector.Synchronizer();
        }
        GUI.set_enabled(true);
        this.DrawSynchronizedVariables(target);
      }
    }

    public static void DrawComponentSelector(
      VariableSynchronizerInspector.Synchronizer synchronizer,
      Type componentType,
      VariableSynchronizerInspector.ComponentListType listType)
    {
      bool flag = false;
      EditorGUI.BeginChangeCheck();
      synchronizer.gameObject = EditorGUILayout.ObjectField("GameObject", (Object) synchronizer.gameObject, typeof (GameObject), true, new GUILayoutOption[0]) as GameObject;
      if (EditorGUI.EndChangeCheck())
        flag = true;
      if (Object.op_Equality((Object) synchronizer.gameObject, (Object) null))
        GUI.set_enabled(false);
      switch (listType)
      {
        case VariableSynchronizerInspector.ComponentListType.Instant:
          if (!flag)
            break;
          if (Object.op_Inequality((Object) synchronizer.gameObject, (Object) null))
          {
            synchronizer.component = synchronizer.gameObject.GetComponent(componentType);
            break;
          }
          synchronizer.component = (Component) null;
          break;
        case VariableSynchronizerInspector.ComponentListType.Popup:
          int num1 = 0;
          List<string> stringList = new List<string>();
          Component[] componentArray = (Component[]) null;
          stringList.Add("None");
          if (Object.op_Inequality((Object) synchronizer.gameObject, (Object) null))
          {
            componentArray = synchronizer.gameObject.GetComponents(componentType);
            for (int index1 = 0; index1 < componentArray.Length; ++index1)
            {
              if (((Object) componentArray[index1]).Equals((object) synchronizer.component))
                num1 = stringList.Count;
              string str = BehaviorDesignerUtility.SplitCamelCase(((object) componentArray[index1]).GetType().Name);
              int num2 = 0;
              for (int index2 = 0; index2 < stringList.Count; ++index2)
              {
                if (stringList[index1].Equals(str))
                  ++num2;
              }
              if (num2 > 0)
                str = str + " " + (object) num2;
              stringList.Add(str);
            }
          }
          EditorGUI.BeginChangeCheck();
          int num3 = EditorGUILayout.Popup("Component", num1, stringList.ToArray(), new GUILayoutOption[0]);
          if (!EditorGUI.EndChangeCheck())
            break;
          if (num3 != 0)
          {
            synchronizer.component = componentArray[num3 - 1];
            break;
          }
          synchronizer.component = (Component) null;
          break;
        case VariableSynchronizerInspector.ComponentListType.BehaviorDesignerGroup:
          if (!Object.op_Inequality((Object) synchronizer.gameObject, (Object) null))
            break;
          Behavior[] components = (Behavior[]) synchronizer.gameObject.GetComponents<Behavior>();
          if (components != null && components.Length > 1)
            synchronizer.componentGroup = EditorGUILayout.IntField("Behavior Tree Group", synchronizer.componentGroup, new GUILayoutOption[0]);
          synchronizer.component = (Component) VariableSynchronizerInspector.GetBehaviorWithGroup(components, synchronizer.componentGroup);
          break;
      }
    }

    private bool DrawSharedVariableSynchronizer(
      VariableSynchronizerInspector.Synchronizer synchronizer,
      Type valueType)
    {
      VariableSynchronizerInspector.DrawComponentSelector(synchronizer, typeof (Behavior), VariableSynchronizerInspector.ComponentListType.BehaviorDesignerGroup);
      int num = 0;
      int globalStartIndex = -1;
      string[] names = (string[]) null;
      if (Object.op_Inequality((Object) synchronizer.component, (Object) null))
      {
        Behavior component = synchronizer.component as Behavior;
        num = FieldInspector.GetVariablesOfType(valueType, synchronizer.global, synchronizer.targetName, component.GetBehaviorSource(), out names, ref globalStartIndex, valueType == null);
      }
      else
        names = new string[1]{ "None" };
      EditorGUI.BeginChangeCheck();
      int index = EditorGUILayout.Popup("Shared Variable", num, names, new GUILayoutOption[0]);
      if (EditorGUI.EndChangeCheck())
      {
        if (index != 0)
        {
          if (globalStartIndex != -1 && index >= globalStartIndex)
          {
            synchronizer.targetName = names[index].Substring(8, names[index].Length - 8);
            synchronizer.global = true;
          }
          else
          {
            synchronizer.targetName = names[index];
            synchronizer.global = false;
          }
          if (valueType == null)
          {
            this.sharedVariableValueTypeName = (!synchronizer.global ? (object) (synchronizer.component as Behavior).GetVariable(names[index]) : (object) GlobalVariables.get_Instance().GetVariable(synchronizer.targetName)).GetType().GetProperty("Value").PropertyType.FullName;
            this.sharedVariableValueType = (Type) null;
          }
        }
        else
          synchronizer.targetName = (string) null;
      }
      if (string.IsNullOrEmpty(synchronizer.targetName))
        GUI.set_enabled(false);
      return GUI.get_enabled();
    }

    private static Behavior GetBehaviorWithGroup(Behavior[] behaviors, int group)
    {
      if (behaviors == null || behaviors.Length == 0)
        return (Behavior) null;
      if (behaviors.Length == 1)
        return behaviors[0];
      for (int index = 0; index < behaviors.Length; ++index)
      {
        if (behaviors[index].get_Group() == group)
          return behaviors[index];
      }
      return behaviors[0];
    }

    private void DrawPropertySynchronizer(
      VariableSynchronizerInspector.Synchronizer synchronizer,
      Type valueType)
    {
      VariableSynchronizerInspector.DrawComponentSelector(synchronizer, typeof (Component), VariableSynchronizerInspector.ComponentListType.Popup);
      int num = 0;
      List<string> stringList = new List<string>();
      stringList.Add("None");
      if (Object.op_Inequality((Object) synchronizer.component, (Object) null))
      {
        PropertyInfo[] properties = ((object) synchronizer.component).GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
        for (int index = 0; index < properties.Length; ++index)
        {
          if (properties[index].PropertyType.Equals(valueType) && !properties[index].IsSpecialName)
          {
            if (properties[index].Name.Equals(synchronizer.targetName))
              num = stringList.Count;
            stringList.Add(properties[index].Name);
          }
        }
      }
      EditorGUI.BeginChangeCheck();
      int index1 = EditorGUILayout.Popup("Property", num, stringList.ToArray(), new GUILayoutOption[0]);
      if (!EditorGUI.EndChangeCheck())
        return;
      if (index1 != 0)
        synchronizer.targetName = stringList[index1];
      else
        synchronizer.targetName = string.Empty;
    }

    private void DrawAnimatorSynchronizer(
      VariableSynchronizerInspector.Synchronizer synchronizer)
    {
      VariableSynchronizerInspector.DrawComponentSelector(synchronizer, typeof (Animator), VariableSynchronizerInspector.ComponentListType.Instant);
      synchronizer.targetName = EditorGUILayout.TextField("Parameter Name", synchronizer.targetName, new GUILayoutOption[0]);
    }

    private void DrawPlayMakerSynchronizer(
      VariableSynchronizerInspector.Synchronizer synchronizer,
      Type valueType)
    {
      if (this.playMakerSynchronizationType == null)
      {
        this.playMakerSynchronizationType = Type.GetType("BehaviorDesigner.Editor.VariableSynchronizerInspector_PlayMaker, Assembly-CSharp-Editor");
        if (this.playMakerSynchronizationType == null)
        {
          EditorGUILayout.LabelField("Unable to find PlayMaker inspector task.", new GUILayoutOption[0]);
          return;
        }
      }
      if (this.thirdPartySynchronizer == null)
      {
        MethodInfo method = this.playMakerSynchronizationType.GetMethod(nameof (DrawPlayMakerSynchronizer));
        if (method != null)
          this.thirdPartySynchronizer = (Action<VariableSynchronizerInspector.Synchronizer, Type>) Delegate.CreateDelegate(typeof (Action<VariableSynchronizerInspector.Synchronizer, Type>), method);
      }
      this.thirdPartySynchronizer(synchronizer, valueType);
    }

    private void DrawuFrameSynchronizer(
      VariableSynchronizerInspector.Synchronizer synchronizer,
      Type valueType)
    {
      if (this.uFrameSynchronizationType == null)
      {
        this.uFrameSynchronizationType = Type.GetType("BehaviorDesigner.Editor.VariableSynchronizerInspector_uFrame, Assembly-CSharp-Editor");
        if (this.uFrameSynchronizationType == null)
        {
          EditorGUILayout.LabelField("Unable to find uFrame inspector task.", new GUILayoutOption[0]);
          return;
        }
      }
      if (this.thirdPartySynchronizer == null)
      {
        MethodInfo method = this.uFrameSynchronizationType.GetMethod("DrawSynchronizer");
        if (method != null)
          this.thirdPartySynchronizer = (Action<VariableSynchronizerInspector.Synchronizer, Type>) Delegate.CreateDelegate(typeof (Action<VariableSynchronizerInspector.Synchronizer, Type>), method);
      }
      this.thirdPartySynchronizer(synchronizer, valueType);
    }

    private void DrawSynchronizedVariables(VariableSynchronizer variableSynchronizer)
    {
      GUI.set_enabled(true);
      if (variableSynchronizer.get_SynchronizedVariables() == null || variableSynchronizer.get_SynchronizedVariables().Count == 0)
        return;
      Rect lastRect = GUILayoutUtility.GetLastRect();
      ((Rect) ref lastRect).set_x(-5f);
      ref Rect local1 = ref lastRect;
      ((Rect) ref local1).set_y(((Rect) ref local1).get_y() + (((Rect) ref lastRect).get_height() + 1f));
      ((Rect) ref lastRect).set_height(2f);
      ref Rect local2 = ref lastRect;
      ((Rect) ref local2).set_width(((Rect) ref local2).get_width() + 20f);
      GUI.DrawTexture(lastRect, (Texture) BehaviorDesignerUtility.LoadTexture("ContentSeparator.png", true, (Object) this));
      GUILayout.Space(6f);
      for (int index = 0; index < variableSynchronizer.get_SynchronizedVariables().Count; ++index)
      {
        VariableSynchronizer.SynchronizedVariable synchronizedVariable = variableSynchronizer.get_SynchronizedVariables()[index];
        if (synchronizedVariable.global != null)
        {
          if (GlobalVariables.get_Instance().GetVariable((string) synchronizedVariable.variableName) == null)
          {
            variableSynchronizer.get_SynchronizedVariables().RemoveAt(index);
            break;
          }
        }
        else if (((Behavior) synchronizedVariable.behavior).GetVariable((string) synchronizedVariable.variableName) == null)
        {
          variableSynchronizer.get_SynchronizedVariables().RemoveAt(index);
          break;
        }
        EditorGUILayout.BeginHorizontal(new GUILayoutOption[0]);
        EditorGUILayout.LabelField((string) synchronizedVariable.variableName, new GUILayoutOption[1]
        {
          GUILayout.MaxWidth(120f)
        });
        if (GUILayout.Button((Texture) BehaviorDesignerUtility.LoadTexture(synchronizedVariable.setVariable == null ? "RightArrowButton.png" : "LeftArrowButton.png", true, (Object) this), BehaviorDesignerUtility.ButtonGUIStyle, new GUILayoutOption[1]
        {
          GUILayout.Width(22f)
        }) && !Application.get_isPlaying())
          synchronizedVariable.setVariable = (__Null) (synchronizedVariable.setVariable == 0 ? 1 : 0);
        EditorGUILayout.LabelField(string.Format("{0} ({1})", (object) synchronizedVariable.targetName, (object) ((Enum) (object) (VariableSynchronizer.SynchronizationType) synchronizedVariable.synchronizationType).ToString()), new GUILayoutOption[1]
        {
          GUILayout.MinWidth(120f)
        });
        GUILayout.FlexibleSpace();
        if (GUILayout.Button((Texture) BehaviorDesignerUtility.LoadTexture("DeleteButton.png", true, (Object) this), BehaviorDesignerUtility.ButtonGUIStyle, new GUILayoutOption[1]
        {
          GUILayout.Width(22f)
        }))
        {
          variableSynchronizer.get_SynchronizedVariables().RemoveAt(index);
          EditorGUILayout.EndHorizontal();
          break;
        }
        GUILayout.Space(2f);
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(2f);
      }
      GUILayout.Space(4f);
    }

    public enum ComponentListType
    {
      Instant,
      Popup,
      BehaviorDesignerGroup,
      None,
    }

    [Serializable]
    public class Synchronizer
    {
      public GameObject gameObject;
      public Component component;
      public string targetName;
      public bool global;
      public int componentGroup;
      public string componentName;
    }
  }
}
