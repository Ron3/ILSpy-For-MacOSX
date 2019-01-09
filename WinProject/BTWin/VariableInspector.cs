// Decompiled with JetBrains decompiler
// Type: BehaviorDesigner.Editor.VariableInspector
// Assembly: BehaviorDesignerEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 99CE4D00-DFA2-42D1-ABFC-D630AB4C1372
// Assembly location: C:\Users\Ron\Desktop\bt\BehaviorDesignerEditor.dll

using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace BehaviorDesigner.Editor
{
  public class VariableInspector : ScriptableObject
  {
    private static string[] sharedVariableStrings;
    private static List<Type> sharedVariableTypes;
    private static Dictionary<string, int> sharedVariableTypesDict;
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
    private static SharedVariable mPropertyMappingVariable;
    private static BehaviorSource mPropertyMappingBehaviorSource;
    private static GenericMenu mPropertyMappingMenu;

    public VariableInspector()
    {
      //base.\u002Ector();
    }

    public void ResetSelectedVariableIndex()
    {
      this.mSelectedVariableIndex = -1;
      this.mVariableStartPosition = -1f;
      if (this.mVariablePosition == null)
        return;
      this.mVariablePosition.Clear();
    }

    public void OnEnable()
    {
      ((Object) this).set_hideFlags((HideFlags) 61);
    }

    public static List<Type> FindAllSharedVariableTypes(bool removeShared)
    {
      if (VariableInspector.sharedVariableTypes != null)
        return VariableInspector.sharedVariableTypes;
      VariableInspector.sharedVariableTypes = new List<Type>();
      foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
      {
        Type[] types = assembly.GetTypes();
        for (int index = 0; index < types.Length; ++index)
        {
          if (types[index].IsSubclassOf(typeof (SharedVariable)) && !types[index].IsAbstract)
            VariableInspector.sharedVariableTypes.Add(types[index]);
        }
      }
      VariableInspector.sharedVariableTypes.Sort((IComparer<Type>) new AlphanumComparator<Type>());
      VariableInspector.sharedVariableStrings = new string[VariableInspector.sharedVariableTypes.Count];
      VariableInspector.sharedVariableTypesDict = new Dictionary<string, int>();
      for (int index = 0; index < VariableInspector.sharedVariableTypes.Count; ++index)
      {
        string key = VariableInspector.sharedVariableTypes[index].Name;
        VariableInspector.sharedVariableTypesDict.Add(key, index);
        if (removeShared && key.Length > 6 && key.Substring(0, 6).Equals("Shared"))
          key = key.Substring(6, key.Length - 6);
        VariableInspector.sharedVariableStrings[index] = key;
      }
      return VariableInspector.sharedVariableTypes;
    }

    public bool ClearFocus(bool addVariable, BehaviorSource behaviorSource)
    {
      GUIUtility.set_keyboardControl(0);
      bool flag = false;
      if (addVariable && !string.IsNullOrEmpty(this.mVariableName) && VariableInspector.VariableNameValid((IVariableSource) behaviorSource, this.mVariableName))
      {
        flag = VariableInspector.AddVariable((IVariableSource) behaviorSource, this.mVariableName, this.mVariableTypeIndex, false);
        this.mVariableName = string.Empty;
      }
      return flag;
    }

    public bool HasFocus()
    {
      return GUIUtility.get_keyboardControl() != 0;
    }

    public void FocusNameField()
    {
      this.mFocusNameField = true;
    }

    public bool LeftMouseDown(
      IVariableSource variableSource,
      BehaviorSource behaviorSource,
      Vector2 mousePosition)
    {
      return VariableInspector.LeftMouseDown(variableSource, behaviorSource, mousePosition, this.mVariablePosition, this.mVariableStartPosition, this.mScrollPosition, ref this.mSelectedVariableIndex, ref this.mSelectedVariableName, ref this.mSelectedVariableTypeIndex);
    }

    public static bool LeftMouseDown(
      IVariableSource variableSource,
      BehaviorSource behaviorSource,
      Vector2 mousePosition,
      List<float> variablePosition,
      float variableStartPosition,
      Vector2 scrollPosition,
      ref int selectedVariableIndex,
      ref string selectedVariableName,
      ref int selectedVariableTypeIndex)
    {
      if (variablePosition != null && mousePosition.y > (double) variableStartPosition && variableSource != null)
      {
        List<SharedVariable> allVariables;
        if (!Application.get_isPlaying() && behaviorSource != null && behaviorSource.get_Owner() is Behavior)
        {
          Behavior owner = behaviorSource.get_Owner() as Behavior;
          if (Object.op_Inequality((Object) owner.get_ExternalBehavior(), (Object) null))
          {
            BehaviorSource behaviorSource1 = owner.GetBehaviorSource();
            behaviorSource1.CheckForSerialization(true, (BehaviorSource) null);
            allVariables = behaviorSource1.GetAllVariables();
            ExternalBehavior externalBehavior = owner.get_ExternalBehavior();
            externalBehavior.get_BehaviorSource().set_Owner((IBehavior) externalBehavior);
            externalBehavior.get_BehaviorSource().CheckForSerialization(true, behaviorSource);
          }
          else
            allVariables = variableSource.GetAllVariables();
        }
        else
          allVariables = variableSource.GetAllVariables();
        if (allVariables == null || allVariables.Count != variablePosition.Count)
          return false;
        for (int index = 0; index < variablePosition.Count; ++index)
        {
          if (mousePosition.y < (double) variablePosition[index] - scrollPosition.y)
          {
            if (index == selectedVariableIndex)
              return false;
            selectedVariableIndex = index;
            selectedVariableName = allVariables[index].get_Name();
            selectedVariableTypeIndex = VariableInspector.sharedVariableTypesDict[((object) allVariables[index]).GetType().Name];
            return true;
          }
        }
      }
      if (selectedVariableIndex == -1)
        return false;
      selectedVariableIndex = -1;
      return true;
    }

    public bool DrawVariables(BehaviorSource behaviorSource)
    {
      return VariableInspector.DrawVariables((IVariableSource) behaviorSource, behaviorSource, ref this.mVariableName, ref this.mFocusNameField, ref this.mVariableTypeIndex, ref this.mScrollPosition, ref this.mVariablePosition, ref this.mVariableStartPosition, ref this.mSelectedVariableIndex, ref this.mSelectedVariableName, ref this.mSelectedVariableTypeIndex);
    }

    public static bool DrawVariables(
      IVariableSource variableSource,
      BehaviorSource behaviorSource,
      ref string variableName,
      ref bool focusNameField,
      ref int variableTypeIndex,
      ref Vector2 scrollPosition,
      ref List<float> variablePosition,
      ref float variableStartPosition,
      ref int selectedVariableIndex,
      ref string selectedVariableName,
      ref int selectedVariableTypeIndex)
    {
      scrollPosition = GUILayout.BeginScrollView(scrollPosition, new GUILayoutOption[0]);
      bool flag1 = false;
      bool flag2 = false;
      List<SharedVariable> sharedVariableList = variableSource == null ? (List<SharedVariable>) null : variableSource.GetAllVariables();
      if (VariableInspector.DrawHeader(variableSource, behaviorSource == null, ref variableStartPosition, ref variableName, ref focusNameField, ref variableTypeIndex, ref selectedVariableIndex, ref selectedVariableName, ref selectedVariableTypeIndex))
        flag1 = true;
      List<SharedVariable> variables = variableSource == null ? (List<SharedVariable>) null : variableSource.GetAllVariables();
      if (variables != null && variables.Count > 0)
      {
        GUI.set_enabled(!flag2);
        if (VariableInspector.DrawAllVariables(true, variableSource, ref variables, true, ref variablePosition, ref selectedVariableIndex, ref selectedVariableName, ref selectedVariableTypeIndex, true, true))
          flag1 = true;
      }
      if (flag1 && variableSource != null)
        variableSource.SetAllVariables(variables);
      GUI.set_enabled(true);
      GUILayout.EndScrollView();
      if (flag1 && !EditorApplication.get_isPlayingOrWillChangePlaymode() && (behaviorSource != null && behaviorSource.get_Owner() is Behavior))
      {
        Behavior owner = behaviorSource.get_Owner() as Behavior;
        if (Object.op_Inequality((Object) owner.get_ExternalBehavior(), (Object) null))
        {
          if (BehaviorDesignerPreferences.GetBool(BDPreferences.BinarySerialization))
            BinarySerialization.Save(behaviorSource);
          else
            JSONSerialization.Save(behaviorSource);
          BehaviorSource behaviorSource1 = owner.get_ExternalBehavior().GetBehaviorSource();
          behaviorSource1.CheckForSerialization(true, (BehaviorSource) null);
          VariableInspector.SyncVariables(behaviorSource1, variables);
        }
      }
      return flag1;
    }

    public static bool SyncVariables(
      BehaviorSource localBehaviorSource,
      List<SharedVariable> variables)
    {
      List<SharedVariable> sharedVariableList = localBehaviorSource.GetAllVariables();
      if (variables == null)
      {
        if (sharedVariableList == null || sharedVariableList.Count <= 0)
          return false;
        sharedVariableList.Clear();
        return true;
      }
      bool flag = false;
      if (sharedVariableList == null)
      {
        sharedVariableList = new List<SharedVariable>();
        localBehaviorSource.SetAllVariables(sharedVariableList);
        flag = true;
      }
      for (int index = 0; index < variables.Count; ++index)
      {
        if (sharedVariableList.Count - 1 < index)
        {
          SharedVariable instance = Activator.CreateInstance(((object) variables[index]).GetType()) as SharedVariable;
          instance.set_Name(variables[index].get_Name());
          instance.set_IsShared(true);
          instance.SetValue(variables[index].GetValue());
          sharedVariableList.Add(instance);
          flag = true;
        }
        else if (sharedVariableList[index].get_Name() != variables[index].get_Name() || ((object) sharedVariableList[index]).GetType() != ((object) variables[index]).GetType())
        {
          SharedVariable instance = Activator.CreateInstance(((object) variables[index]).GetType()) as SharedVariable;
          instance.set_Name(variables[index].get_Name());
          instance.set_IsShared(true);
          instance.SetValue(variables[index].GetValue());
          sharedVariableList[index] = instance;
          flag = true;
        }
      }
      for (int index = sharedVariableList.Count - 1; index > variables.Count - 1; --index)
      {
        sharedVariableList.RemoveAt(index);
        flag = true;
      }
      return flag;
    }

    private static bool DrawHeader(
      IVariableSource variableSource,
      bool fromGlobalVariablesWindow,
      ref float variableStartPosition,
      ref string variableName,
      ref bool focusNameField,
      ref int variableTypeIndex,
      ref int selectedVariableIndex,
      ref string selectedVariableName,
      ref int selectedVariableTypeIndex)
    {
      if (VariableInspector.sharedVariableStrings == null)
        VariableInspector.FindAllSharedVariableTypes(true);
      EditorGUIUtility.set_labelWidth(150f);
      GUILayout.BeginHorizontal(new GUILayoutOption[0]);
      GUILayout.Space(4f);
      EditorGUILayout.LabelField("Name", new GUILayoutOption[1]
      {
        GUILayout.Width(70f)
      });
      GUI.SetNextControlName("Name");
      variableName = EditorGUILayout.TextField(variableName, new GUILayoutOption[1]
      {
        GUILayout.Width(212f)
      });
      if (focusNameField)
      {
        GUI.FocusControl("Name");
        focusNameField = false;
      }
      GUILayout.EndHorizontal();
      GUILayout.Space(2f);
      GUILayout.BeginHorizontal(new GUILayoutOption[0]);
      GUILayout.Space(4f);
      GUILayout.Label("Type", new GUILayoutOption[1]
      {
        GUILayout.Width(70f)
      });
      variableTypeIndex = EditorGUILayout.Popup(variableTypeIndex, VariableInspector.sharedVariableStrings, EditorStyles.get_toolbarPopup(), new GUILayoutOption[1]
      {
        GUILayout.Width(163f)
      });
      GUILayout.Space(8f);
      bool flag1 = false;
      bool flag2 = VariableInspector.VariableNameValid(variableSource, variableName);
      bool enabled = GUI.get_enabled();
      GUI.set_enabled(flag2 && enabled);
      GUI.SetNextControlName("Add");
      if (GUILayout.Button("Add", EditorStyles.get_toolbarButton(), new GUILayoutOption[1]
      {
        GUILayout.Width(40f)
      }) && flag2)
      {
        if (fromGlobalVariablesWindow && variableSource == null)
        {
          GlobalVariables instance = ScriptableObject.CreateInstance(typeof (GlobalVariables)) as GlobalVariables;
          string str1 = BehaviorDesignerUtility.GetEditorBaseDirectory((Object) null).Substring(6, BehaviorDesignerUtility.GetEditorBaseDirectory((Object) null).Length - 13);
          string str2 = str1 + "/Resources/BehaviorDesignerGlobalVariables.asset";
          if (!Directory.Exists(Application.get_dataPath() + str1 + "/Resources"))
            Directory.CreateDirectory(Application.get_dataPath() + str1 + "/Resources");
          if (!File.Exists(Application.get_dataPath() + str2))
          {
            AssetDatabase.CreateAsset((Object) instance, "Assets" + str2);
            EditorUtility.DisplayDialog("Created Global Variables", "Behavior Designer Global Variables asset created:\n\nAssets" + str1 + "/Resources/BehaviorDesignerGlobalVariables.asset\n\nNote: Copy this file to transfer global variables between projects.", "OK");
          }
          variableSource = (IVariableSource) instance;
        }
        flag1 = VariableInspector.AddVariable(variableSource, variableName, variableTypeIndex, fromGlobalVariablesWindow);
        if (flag1)
        {
          selectedVariableIndex = variableSource.GetAllVariables().Count - 1;
          selectedVariableName = variableName;
          selectedVariableTypeIndex = variableTypeIndex;
          variableName = string.Empty;
        }
      }
      GUILayout.Space(6f);
      GUILayout.EndHorizontal();
      if (!fromGlobalVariablesWindow)
      {
        GUI.set_enabled(true);
        GUILayout.Space(3f);
        GUILayout.BeginHorizontal(new GUILayoutOption[0]);
        GUILayout.Space(5f);
        if (GUILayout.Button("Global Variables", EditorStyles.get_toolbarButton(), new GUILayoutOption[1]
        {
          GUILayout.Width(284f)
        }))
          GlobalVariablesWindow.ShowWindow();
        GUILayout.EndHorizontal();
      }
      BehaviorDesignerUtility.DrawContentSeperator(2);
      GUILayout.Space(4f);
      if ((double) variableStartPosition == -1.0 && Event.get_current().get_type() == 7)
      {
        ref float local = ref variableStartPosition;
        Rect lastRect = GUILayoutUtility.GetLastRect();
        double yMax = (double) ((Rect) ref lastRect).get_yMax();
        local = (float) yMax;
      }
      GUI.set_enabled(enabled);
      return flag1;
    }

    private static bool AddVariable(
      IVariableSource variableSource,
      string variableName,
      int variableTypeIndex,
      bool fromGlobalVariablesWindow)
    {
      SharedVariable variable = VariableInspector.CreateVariable(variableTypeIndex, variableName, fromGlobalVariablesWindow);
      List<SharedVariable> sharedVariableList = (variableSource == null ? (List<SharedVariable>) null : variableSource.GetAllVariables()) ?? new List<SharedVariable>();
      sharedVariableList.Add(variable);
      variableSource.SetAllVariables(sharedVariableList);
      return true;
    }

    public static bool DrawAllVariables(
      bool showFooter,
      IVariableSource variableSource,
      ref List<SharedVariable> variables,
      bool canSelect,
      ref List<float> variablePosition,
      ref int selectedVariableIndex,
      ref string selectedVariableName,
      ref int selectedVariableTypeIndex,
      bool drawRemoveButton,
      bool drawLastSeparator)
    {
      if (variables == null)
        return false;
      bool flag = false;
      if (canSelect && variablePosition == null)
        variablePosition = new List<float>();
      for (int index1 = 0; index1 < variables.Count; ++index1)
      {
        SharedVariable sharedVariable = variables[index1];
        if (sharedVariable != null)
        {
          if (canSelect && selectedVariableIndex == index1)
          {
            if (index1 == 0)
              GUILayout.Space(2f);
            bool deleted = false;
            if (VariableInspector.DrawSelectedVariable(variableSource, ref variables, sharedVariable, ref selectedVariableIndex, ref selectedVariableName, ref selectedVariableTypeIndex, ref deleted))
              flag = true;
            if (deleted)
            {
              if (Object.op_Inequality((Object) BehaviorDesignerWindow.instance, (Object) null))
                BehaviorDesignerWindow.instance.RemoveSharedVariableReferences(sharedVariable);
              variables.RemoveAt(index1);
              if (selectedVariableIndex == index1)
                selectedVariableIndex = -1;
              else if (selectedVariableIndex > index1)
                --selectedVariableIndex;
              flag = true;
              break;
            }
          }
          else
          {
            GUILayout.BeginHorizontal(new GUILayoutOption[0]);
            if (VariableInspector.DrawSharedVariable(variableSource, sharedVariable, false))
              flag = true;
            if (drawRemoveButton)
            {
              if (GUILayout.Button((Texture) BehaviorDesignerUtility.VariableDeleteButtonTexture, BehaviorDesignerUtility.PlainButtonGUIStyle, new GUILayoutOption[1]
              {
                GUILayout.Width(19f)
              }) && EditorUtility.DisplayDialog("Delete Variable", "Are you sure you want to delete this variable?", "Yes", "No"))
              {
                if (Object.op_Inequality((Object) BehaviorDesignerWindow.instance, (Object) null))
                  BehaviorDesignerWindow.instance.RemoveSharedVariableReferences(sharedVariable);
                variables.RemoveAt(index1);
                if (canSelect)
                {
                  if (selectedVariableIndex == index1)
                    selectedVariableIndex = -1;
                  else if (selectedVariableIndex > index1)
                    --selectedVariableIndex;
                }
                flag = true;
                break;
              }
            }
            if (Object.op_Inequality((Object) BehaviorDesignerWindow.instance, (Object) null) && BehaviorDesignerWindow.instance.ContainsError((Task) null, variables[index1].get_Name()))
              GUILayout.Box((Texture) BehaviorDesignerUtility.ErrorIconTexture, BehaviorDesignerUtility.PlainTextureGUIStyle, new GUILayoutOption[1]
              {
                GUILayout.Width(20f)
              });
            GUILayout.Space(10f);
            GUILayout.EndHorizontal();
            if (index1 != variables.Count - 1 || drawLastSeparator)
              BehaviorDesignerUtility.DrawContentSeperator(2, 7);
          }
          GUILayout.Space(4f);
          if (canSelect && Event.get_current().get_type() == 7)
          {
            if (variablePosition.Count <= index1)
            {
              List<float> floatList = variablePosition;
              Rect lastRect = GUILayoutUtility.GetLastRect();
              double yMax = (double) ((Rect) ref lastRect).get_yMax();
              floatList.Add((float) yMax);
            }
            else
            {
              List<float> floatList = variablePosition;
              int index2 = index1;
              Rect lastRect = GUILayoutUtility.GetLastRect();
              double yMax = (double) ((Rect) ref lastRect).get_yMax();
              floatList[index2] = (float) yMax;
            }
          }
        }
      }
      if (canSelect && variables.Count < variablePosition.Count)
      {
        for (int index = variablePosition.Count - 1; index >= variables.Count; --index)
          variablePosition.RemoveAt(index);
      }
      if (showFooter && variables.Count > 0)
      {
        GUI.set_enabled(true);
        GUILayout.Label("Select a variable to change its properties.", BehaviorDesignerUtility.LabelWrapGUIStyle, new GUILayoutOption[0]);
      }
      return flag;
    }

    private static bool DrawSharedVariable(
      IVariableSource variableSource,
      SharedVariable sharedVariable,
      bool selected)
    {
      if (sharedVariable == null || ((object) sharedVariable).GetType().GetProperty("Value") == null)
        return false;
      GUILayout.BeginHorizontal(new GUILayoutOption[0]);
      bool flag = false;
      if (!string.IsNullOrEmpty(sharedVariable.get_PropertyMapping()))
      {
        if (selected)
          GUILayout.Label("Property", new GUILayoutOption[0]);
        else
          GUILayout.Label(sharedVariable.get_Name(), new GUILayoutOption[0]);
        string[] strArray = sharedVariable.get_PropertyMapping().Split('.');
        GUILayout.Label(strArray[strArray.Length - 1].Replace('/', '.'), new GUILayoutOption[0]);
      }
      else
      {
        EditorGUI.BeginChangeCheck();
        FieldInspector.DrawFields((Task) null, (object) sharedVariable, new GUIContent(sharedVariable.get_Name()));
        flag = EditorGUI.EndChangeCheck();
      }
      if (!sharedVariable.get_IsGlobal())
      {
        if (GUILayout.Button((Texture) BehaviorDesignerUtility.VariableMapButtonTexture, BehaviorDesignerUtility.PlainButtonGUIStyle, new GUILayoutOption[1]
        {
          GUILayout.Width(19f)
        }))
          VariableInspector.ShowPropertyMappingMenu(variableSource as BehaviorSource, sharedVariable);
      }
      GUILayout.EndHorizontal();
      return flag;
    }

    private static bool DrawSelectedVariable(
      IVariableSource variableSource,
      ref List<SharedVariable> variables,
      SharedVariable sharedVariable,
      ref int selectedVariableIndex,
      ref string selectedVariableName,
      ref int selectedVariableTypeIndex,
      ref bool deleted)
    {
      bool flag = false;
      GUILayout.BeginVertical(BehaviorDesignerUtility.SelectedBackgroundGUIStyle, new GUILayoutOption[0]);
      GUILayout.BeginHorizontal(new GUILayoutOption[0]);
      GUILayout.Label("Name", new GUILayoutOption[1]
      {
        GUILayout.Width(70f)
      });
      EditorGUI.BeginChangeCheck();
      selectedVariableName = GUILayout.TextField(selectedVariableName, new GUILayoutOption[1]
      {
        GUILayout.Width(140f)
      });
      if (EditorGUI.EndChangeCheck())
      {
        if (VariableInspector.VariableNameValid(variableSource, selectedVariableName))
          variableSource.UpdateVariableName(sharedVariable, selectedVariableName);
        flag = true;
      }
      GUILayout.Space(10f);
      bool enabled = GUI.get_enabled();
      GUI.set_enabled(enabled && selectedVariableIndex < variables.Count - 1);
      if (GUILayout.Button((Texture) BehaviorDesignerUtility.DownArrowButtonTexture, BehaviorDesignerUtility.PlainButtonGUIStyle, new GUILayoutOption[1]
      {
        GUILayout.Width(19f)
      }))
      {
        SharedVariable sharedVariable1 = variables[selectedVariableIndex + 1];
        variables[selectedVariableIndex + 1] = variables[selectedVariableIndex];
        variables[selectedVariableIndex] = sharedVariable1;
        ++selectedVariableIndex;
        flag = true;
      }
      GUI.set_enabled(enabled && (selectedVariableIndex < variables.Count - 1 || selectedVariableIndex != 0));
      GUILayout.Box(string.Empty, BehaviorDesignerUtility.ArrowSeparatorGUIStyle, new GUILayoutOption[2]
      {
        GUILayout.Width(1f),
        GUILayout.Height(18f)
      });
      GUI.set_enabled(enabled && selectedVariableIndex != 0);
      if (GUILayout.Button((Texture) BehaviorDesignerUtility.UpArrowButtonTexture, BehaviorDesignerUtility.PlainButtonGUIStyle, new GUILayoutOption[1]
      {
        GUILayout.Width(20f)
      }))
      {
        SharedVariable sharedVariable1 = variables[selectedVariableIndex - 1];
        variables[selectedVariableIndex - 1] = variables[selectedVariableIndex];
        variables[selectedVariableIndex] = sharedVariable1;
        --selectedVariableIndex;
        flag = true;
      }
      GUI.set_enabled(enabled);
      if (GUILayout.Button((Texture) BehaviorDesignerUtility.VariableDeleteButtonTexture, BehaviorDesignerUtility.PlainButtonGUIStyle, new GUILayoutOption[1]
      {
        GUILayout.Width(19f)
      }) && EditorUtility.DisplayDialog("Delete Variable", "Are you sure you want to delete this variable?", "Yes", "No"))
        deleted = true;
      GUILayout.EndHorizontal();
      GUILayout.Space(2f);
      GUILayout.BeginHorizontal(new GUILayoutOption[0]);
      GUILayout.Label("Type", new GUILayoutOption[1]
      {
        GUILayout.Width(70f)
      });
      EditorGUI.BeginChangeCheck();
      selectedVariableTypeIndex = EditorGUILayout.Popup(selectedVariableTypeIndex, VariableInspector.sharedVariableStrings, EditorStyles.get_toolbarPopup(), new GUILayoutOption[1]
      {
        GUILayout.Width(200f)
      });
      if (EditorGUI.EndChangeCheck() && VariableInspector.sharedVariableTypesDict[((object) sharedVariable).GetType().Name] != selectedVariableTypeIndex)
      {
        if (Object.op_Inequality((Object) BehaviorDesignerWindow.instance, (Object) null))
          BehaviorDesignerWindow.instance.RemoveSharedVariableReferences(sharedVariable);
        sharedVariable = VariableInspector.CreateVariable(selectedVariableTypeIndex, sharedVariable.get_Name(), sharedVariable.get_IsGlobal());
        variables[selectedVariableIndex] = sharedVariable;
        flag = true;
      }
      GUILayout.EndHorizontal();
      EditorGUI.BeginChangeCheck();
      GUILayout.Space(4f);
      GUILayout.BeginHorizontal(new GUILayoutOption[0]);
      GUI.set_enabled(VariableInspector.CanNetworkSync(((object) sharedVariable).GetType().GetProperty("Value").PropertyType));
      EditorGUI.BeginChangeCheck();
      sharedVariable.set_NetworkSync(EditorGUILayout.Toggle(new GUIContent("Network Sync", "Sync this variable over the network. Requires Unity 5.1 or greator. A NetworkIdentity must be attached to the behavior tree GameObject."), sharedVariable.get_NetworkSync(), new GUILayoutOption[0]));
      if (EditorGUI.EndChangeCheck())
        flag = true;
      GUILayout.EndHorizontal();
      GUI.set_enabled(enabled);
      GUILayout.BeginHorizontal(new GUILayoutOption[0]);
      if (VariableInspector.DrawSharedVariable(variableSource, sharedVariable, true))
        flag = true;
      if (Object.op_Inequality((Object) BehaviorDesignerWindow.instance, (Object) null) && BehaviorDesignerWindow.instance.ContainsError((Task) null, variables[selectedVariableIndex].get_Name()))
        GUILayout.Box((Texture) BehaviorDesignerUtility.ErrorIconTexture, BehaviorDesignerUtility.PlainTextureGUIStyle, new GUILayoutOption[1]
        {
          GUILayout.Width(20f)
        });
      GUILayout.EndHorizontal();
      BehaviorDesignerUtility.DrawContentSeperator(4, 7);
      GUILayout.EndVertical();
      GUILayout.Space(3f);
      return flag;
    }

    private static bool VariableNameValid(IVariableSource variableSource, string variableName)
    {
      if (variableName.Equals(string.Empty))
        return false;
      if (variableSource != null)
        return variableSource.GetVariable(variableName) == null;
      return true;
    }

    private static SharedVariable CreateVariable(int index, string name, bool global)
    {
      SharedVariable instance = Activator.CreateInstance(VariableInspector.sharedVariableTypes[index]) as SharedVariable;
      instance.set_Name(name);
      instance.set_IsShared(true);
      instance.set_IsGlobal(global);
      return instance;
    }

    private static bool CanNetworkSync(Type type)
    {
      return type == typeof (bool) || type == typeof (Color) || (type == typeof (float) || type == typeof (GameObject)) || (type == typeof (int) || type == typeof (Quaternion) || (type == typeof (Rect) || type == typeof (string))) || (type == typeof (Transform) || type == typeof (Vector2) || (type == typeof (Vector3) || type == typeof (Vector4)));
    }

    private static void ShowPropertyMappingMenu(
      BehaviorSource behaviorSource,
      SharedVariable sharedVariable)
    {
      VariableInspector.mPropertyMappingVariable = sharedVariable;
      VariableInspector.mPropertyMappingBehaviorSource = behaviorSource;
      VariableInspector.mPropertyMappingMenu = new GenericMenu();
      List<string> propertyNames = new List<string>();
      List<GameObject> propertyGameObjects = new List<GameObject>();
      propertyNames.Add("None");
      propertyGameObjects.Add((GameObject) null);
      int num1 = 0;
      if (behaviorSource.get_Owner().GetObject() is Behavior)
      {
        GameObject gameObject = ((Component) (behaviorSource.get_Owner().GetObject() as Behavior)).get_gameObject();
        int num2;
        if ((num2 = VariableInspector.AddPropertyName(sharedVariable, gameObject, ref propertyNames, ref propertyGameObjects, true)) != -1)
          num1 = num2;
        GameObject[] gameObjectArray;
        if (AssetDatabase.GetAssetPath((Object) gameObject).Length == 0)
        {
          gameObjectArray = (GameObject[]) Object.FindObjectsOfType<GameObject>();
        }
        else
        {
          Transform[] componentsInChildren = (Transform[]) gameObject.GetComponentsInChildren<Transform>();
          gameObjectArray = new GameObject[componentsInChildren.Length];
          for (int index = 0; index < componentsInChildren.Length; ++index)
            gameObjectArray[index] = ((Component) componentsInChildren[index]).get_gameObject();
        }
        for (int index = 0; index < gameObjectArray.Length; ++index)
        {
          int num3;
          if (!((Object) gameObjectArray[index]).Equals((object) gameObject) && (num3 = VariableInspector.AddPropertyName(sharedVariable, gameObjectArray[index], ref propertyNames, ref propertyGameObjects, false)) != -1)
            num1 = num3;
        }
      }
      for (int index = 0; index < propertyNames.Count; ++index)
      {
        string[] strArray = propertyNames[index].Split('.');
        if (Object.op_Inequality((Object) propertyGameObjects[index], (Object) null))
          strArray[strArray.Length - 1] = VariableInspector.GetFullPath(propertyGameObjects[index].get_transform()) + "/" + strArray[strArray.Length - 1];
        // ISSUE: method pointer
        VariableInspector.mPropertyMappingMenu.AddItem(new GUIContent(strArray[strArray.Length - 1]), index == num1, new GenericMenu.MenuFunction2((object) null, __methodptr(PropertySelected)), (object) new VariableInspector.SelectedPropertyMapping(propertyNames[index], propertyGameObjects[index]));
      }
      VariableInspector.mPropertyMappingMenu.ShowAsContext();
    }

    private static string GetFullPath(Transform transform)
    {
      if (Object.op_Equality((Object) transform.get_parent(), (Object) null))
        return ((Object) transform).get_name();
      return VariableInspector.GetFullPath(transform.get_parent()) + "/" + ((Object) transform).get_name();
    }

    private static int AddPropertyName(
      SharedVariable sharedVariable,
      GameObject gameObject,
      ref List<string> propertyNames,
      ref List<GameObject> propertyGameObjects,
      bool behaviorGameObject)
    {
      int num = -1;
      if (Object.op_Inequality((Object) gameObject, (Object) null))
      {
        Component[] components = gameObject.GetComponents(typeof (Component));
        Type propertyType = ((object) sharedVariable).GetType().GetProperty("Value").PropertyType;
        for (int index1 = 0; index1 < components.Length; ++index1)
        {
          if (!Object.op_Equality((Object) components[index1], (Object) null))
          {
            PropertyInfo[] properties = ((object) components[index1]).GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            for (int index2 = 0; index2 < properties.Length; ++index2)
            {
              if (properties[index2].PropertyType.Equals(propertyType) && !properties[index2].IsSpecialName)
              {
                string str = ((object) components[index1]).GetType().FullName + "/" + properties[index2].Name;
                if (str.Equals(sharedVariable.get_PropertyMapping()) && (object.Equals((object) sharedVariable.get_PropertyMappingOwner(), (object) gameObject) || object.Equals((object) sharedVariable.get_PropertyMappingOwner(), (object) null) && behaviorGameObject))
                  num = propertyNames.Count;
                propertyNames.Add(str);
                propertyGameObjects.Add(gameObject);
              }
            }
          }
        }
      }
      return num;
    }

    private static void PropertySelected(object selected)
    {
      VariableInspector.SelectedPropertyMapping selectedPropertyMapping = selected as VariableInspector.SelectedPropertyMapping;
      if (selectedPropertyMapping.Property.Equals("None"))
      {
        VariableInspector.mPropertyMappingVariable.set_PropertyMapping(string.Empty);
        VariableInspector.mPropertyMappingVariable.set_PropertyMappingOwner((GameObject) null);
      }
      else
      {
        VariableInspector.mPropertyMappingVariable.set_PropertyMapping(selectedPropertyMapping.Property);
        VariableInspector.mPropertyMappingVariable.set_PropertyMappingOwner(selectedPropertyMapping.GameObject);
      }
      if (BehaviorDesignerPreferences.GetBool(BDPreferences.BinarySerialization))
        BinarySerialization.Save(VariableInspector.mPropertyMappingBehaviorSource);
      else
        JSONSerialization.Save(VariableInspector.mPropertyMappingBehaviorSource);
    }

    private class SelectedPropertyMapping
    {
      private string mProperty;
      private GameObject mGameObject;

      public SelectedPropertyMapping(string property, GameObject gameObject)
      {
        this.mProperty = property;
        this.mGameObject = gameObject;
      }

      public string Property
      {
        get
        {
          return this.mProperty;
        }
      }

      public GameObject GameObject
      {
        get
        {
          return this.mGameObject;
        }
      }
    }
  }
}
