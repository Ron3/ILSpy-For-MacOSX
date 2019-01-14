// Decompiled with JetBrains decompiler
// Type: BehaviorDesigner.Runtime.BehaviorGameGUI
// Assembly: BehaviorDesignerRuntime, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 4E52B3C7-423D-4A3D-958C-02E4666F0F16
// Assembly location: C:\Users\Ron\Desktop\Runtime\BehaviorDesignerRuntime.dll

using BehaviorDesigner.Runtime.Tasks;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviorDesigner.Runtime
{
  [AddComponentMenu("Behavior Designer/Behavior Game GUI")]
  public class BehaviorGameGUI : MonoBehaviour
  {
    private BehaviorManager behaviorManager;
    private Camera mainCamera;

    public BehaviorGameGUI()
    {
      base.\u002Ector();
    }

    public void Start()
    {
      this.mainCamera = Camera.get_main();
    }

    public void OnGUI()
    {
      if (Object.op_Equality((Object) this.behaviorManager, (Object) null))
        this.behaviorManager = BehaviorManager.instance;
      if (Object.op_Equality((Object) this.behaviorManager, (Object) null) || Object.op_Equality((Object) this.mainCamera, (Object) null))
        return;
      List<BehaviorManager.BehaviorTree> behaviorTrees = this.behaviorManager.BehaviorTrees;
      for (int index1 = 0; index1 < behaviorTrees.Count; ++index1)
      {
        BehaviorManager.BehaviorTree behaviorTree = behaviorTrees[index1];
        string str = string.Empty;
        for (int index2 = 0; index2 < behaviorTree.activeStack.Count; ++index2)
        {
          Stack<int> active = behaviorTree.activeStack[index2];
          if (active.Count != 0 && behaviorTree.taskList[active.Peek()] is Action)
            str = str + behaviorTree.taskList[behaviorTree.activeStack[index2].Peek()].FriendlyName + (index2 >= behaviorTree.activeStack.Count - 1 ? string.Empty : "\n");
        }
        Vector2 guiPoint = GUIUtility.ScreenToGUIPoint(Vector2.op_Implicit(Camera.get_main().WorldToScreenPoint(((Component) behaviorTree.behavior).get_transform().get_position())));
        GUIContent guiContent = new GUIContent(str);
        Vector2 vector2 = GUI.get_skin().get_label().CalcSize(guiContent);
        ref Vector2 local1 = ref vector2;
        local1.x = (__Null) (local1.x + 14.0);
        ref Vector2 local2 = ref vector2;
        local2.y = (__Null) (local2.y + 5.0);
        GUI.Box(new Rect((float) (guiPoint.x - vector2.x / 2.0), (float) ((double) Screen.get_height() - guiPoint.y + vector2.y / 2.0), (float) vector2.x, (float) vector2.y), guiContent);
      }
    }
  }
}
