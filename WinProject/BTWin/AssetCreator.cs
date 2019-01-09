// Decompiled with JetBrains decompiler
// Type: BehaviorDesigner.Editor.AssetCreator
// Assembly: BehaviorDesignerEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 99CE4D00-DFA2-42D1-ABFC-D630AB4C1372
// Assembly location: C:\Users\Ron\Desktop\bt\BehaviorDesignerEditor.dll

using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace BehaviorDesigner.Editor
{
  public class AssetCreator : EditorWindow
  {
    private bool m_CSharp;
    private AssetCreator.AssetClassType m_classType;
    private string m_AssetName;

    public AssetCreator()
    {
      //base.\u002Ector();
    }

    private bool CSharp
    {
      set
      {
        this.m_CSharp = value;
      }
    }

    private AssetCreator.AssetClassType ClassType
    {
      set
      {
        this.m_classType = value;
        switch (this.m_classType)
        {
          case AssetCreator.AssetClassType.Action:
            this.m_AssetName = "NewAction";
            break;
          case AssetCreator.AssetClassType.Conditional:
            this.m_AssetName = "NewConditional";
            break;
          case AssetCreator.AssetClassType.SharedVariable:
            this.m_AssetName = "SharedNewVariable";
            break;
        }
      }
    }

    public static void ShowWindow(AssetCreator.AssetClassType classType, bool cSharp)
    {
      AssetCreator window = (AssetCreator) EditorWindow.GetWindow<AssetCreator>(true, "Asset Name");
      AssetCreator assetCreator = window;
      Vector2 vector2_1 = new Vector2(300f, 55f);
      window.set_maxSize(vector2_1);
      Vector2 vector2_2 = vector2_1;
      assetCreator.set_minSize(vector2_2);
      window.ClassType = classType;
      window.CSharp = cSharp;
    }

    private void OnGUI()
    {
      this.m_AssetName = EditorGUILayout.TextField("Name", this.m_AssetName, new GUILayoutOption[0]);
      EditorGUILayout.BeginHorizontal(new GUILayoutOption[0]);
      if (GUILayout.Button("OK", new GUILayoutOption[0]))
      {
        AssetCreator.CreateScript(this.m_AssetName, this.m_classType, this.m_CSharp);
        this.Close();
      }
      if (GUILayout.Button("Cancel", new GUILayoutOption[0]))
        this.Close();
      EditorGUILayout.EndHorizontal();
    }

    public static void CreateAsset(Type type, string name)
    {
      ScriptableObject instance = ScriptableObject.CreateInstance(type);
      string path = AssetDatabase.GetAssetPath(Selection.get_activeObject());
      if (path == string.Empty)
        path = "Assets";
      else if (Path.GetExtension(path) != string.Empty)
        path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.get_activeObject())), string.Empty);
      string uniqueAssetPath = AssetDatabase.GenerateUniqueAssetPath(path + "/" + name + ".asset");
      AssetDatabase.CreateAsset((Object) instance, uniqueAssetPath);
      AssetDatabase.SaveAssets();
    }

    private static void CreateScript(
      string name,
      AssetCreator.AssetClassType classType,
      bool cSharp)
    {
      string path = AssetDatabase.GetAssetPath(Selection.get_activeObject());
      if (path == string.Empty)
        path = "Assets";
      else if (Path.GetExtension(path) != string.Empty)
        path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.get_activeObject())), string.Empty);
      string uniqueAssetPath = AssetDatabase.GenerateUniqueAssetPath(path + "/" + name + (!cSharp ? ".js" : ".cs"));
      StreamWriter streamWriter = new StreamWriter(uniqueAssetPath, false);
      string withoutExtension = Path.GetFileNameWithoutExtension(uniqueAssetPath);
      string str = string.Empty;
      switch (classType)
      {
        case AssetCreator.AssetClassType.Action:
          str = AssetCreator.ActionTaskContents(withoutExtension, cSharp);
          break;
        case AssetCreator.AssetClassType.Conditional:
          str = AssetCreator.ConditionalTaskContents(withoutExtension, cSharp);
          break;
        case AssetCreator.AssetClassType.SharedVariable:
          str = AssetCreator.SharedVariableContents(withoutExtension);
          break;
      }
      streamWriter.Write(str);
      streamWriter.Close();
      AssetDatabase.Refresh();
    }

    private static string ActionTaskContents(string name, bool cSharp)
    {
      if (cSharp)
        return "using UnityEngine;\nusing BehaviorDesigner.Runtime;\nusing BehaviorDesigner.Runtime.Tasks;\n\npublic class " + name + " : Action\n{\n\tpublic override void OnStart()\n\t{\n\t\t\n\t}\n\n\tpublic override TaskStatus OnUpdate()\n\t{\n\t\treturn TaskStatus.Success;\n\t}\n}";
      return "#pragma strict\n\nclass " + name + " extends BehaviorDesigner.Runtime.Tasks.Action\n{\n\tfunction OnStart()\n\t{\n\t\t\n\t}\n\n\tfunction OnUpdate()\n\t{\n\t\treturn BehaviorDesigner.Runtime.Tasks.TaskStatus.Success;\n\t}\n}";
    }

    private static string ConditionalTaskContents(string name, bool cSharp)
    {
      if (cSharp)
        return "using UnityEngine;\nusing BehaviorDesigner.Runtime;\nusing BehaviorDesigner.Runtime.Tasks;\n\npublic class " + name + " : Conditional\n{\n\tpublic override TaskStatus OnUpdate()\n\t{\n\t\treturn TaskStatus.Success;\n\t}\n}";
      return "#pragma strict\n\nclass " + name + " extends BehaviorDesigner.Runtime.Tasks.Conditional\n{\n\tfunction OnUpdate()\n\t{\n\t\treturn BehaviorDesigner.Runtime.Tasks.TaskStatus.Success;\n\t}\n}";
    }

    private static string SharedVariableContents(string name)
    {
      string str = name.Remove(0, 6);
      return "using UnityEngine;\nusing BehaviorDesigner.Runtime;\n\n[System.Serializable]\npublic class " + str + "\n{\n\n}\n\n[System.Serializable]\npublic class " + name + " : SharedVariable<" + str + ">\n{\n\tpublic override string ToString() { return mValue == null ? \"null\" : mValue.ToString(); }\n\tpublic static implicit operator " + name + "(" + str + " value) { return new " + name + " { mValue = value }; }\n}";
    }

    public enum AssetClassType
    {
      Action,
      Conditional,
      SharedVariable,
    }
  }
}
