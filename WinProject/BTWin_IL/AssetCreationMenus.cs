// Decompiled with JetBrains decompiler
// Type: BehaviorDesigner.Editor.AssetCreationMenus
// Assembly: BehaviorDesignerEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 99CE4D00-DFA2-42D1-ABFC-D630AB4C1372
// Assembly location: C:\Users\Ron\Desktop\bt\BehaviorDesignerEditor.dll

using System;
using UnityEditor;

namespace BehaviorDesigner.Editor
{
  public class AssetCreationMenus
  {
    [MenuItem("Assets/Create/Behavior Designer/C# Action Task")]
    public static void CreateCSharpActionTask()
    {
      AssetCreator.ShowWindow(AssetCreator.AssetClassType.Action, true);
    }

    [MenuItem("Assets/Create/Behavior Designer/C# Conditional Task")]
    public static void CreateCSharpConditionalTask()
    {
      AssetCreator.ShowWindow(AssetCreator.AssetClassType.Conditional, true);
    }

    [MenuItem("Assets/Create/Behavior Designer/Unityscript Action Task")]
    public static void CreateUnityscriptActionTask()
    {
      AssetCreator.ShowWindow(AssetCreator.AssetClassType.Action, false);
    }

    [MenuItem("Assets/Create/Behavior Designer/Unityscript Conditional Task")]
    public static void CreateUnityscriptConditionalTask()
    {
      AssetCreator.ShowWindow(AssetCreator.AssetClassType.Conditional, false);
    }

    [MenuItem("Assets/Create/Behavior Designer/Shared Variable")]
    public static void CreateSharedVariable()
    {
      AssetCreator.ShowWindow(AssetCreator.AssetClassType.SharedVariable, true);
    }

    [MenuItem("Assets/Create/Behavior Designer/External Behavior Tree")]
    public static void CreateExternalBehaviorTree()
    {
      AssetCreator.CreateAsset(Type.GetType("BehaviorDesigner.Runtime.ExternalBehaviorTree, Assembly-CSharp") ?? Type.GetType("BehaviorDesigner.Runtime.ExternalBehaviorTree, Assembly-CSharp-firstpass"), "NewExternalBehavior");
    }
  }
}
