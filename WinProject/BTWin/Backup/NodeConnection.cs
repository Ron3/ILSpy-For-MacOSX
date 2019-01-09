// Decompiled with JetBrains decompiler
// Type: BehaviorDesigner.Editor.NodeConnection
// Assembly: BehaviorDesignerEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 99CE4D00-DFA2-42D1-ABFC-D630AB4C1372
// Assembly location: C:\Users\Ron\Desktop\bt\BehaviorDesignerEditor.dll

using System;
using UnityEditor;
using UnityEngine;

namespace BehaviorDesigner.Editor
{
  [Serializable]
  public class NodeConnection : ScriptableObject
  {
    [SerializeField]
    private NodeDesigner originatingNodeDesigner;
    [SerializeField]
    private NodeDesigner destinationNodeDesigner;
    [SerializeField]
    private NodeConnectionType nodeConnectionType;
    [SerializeField]
    private bool selected;
    [SerializeField]
    private float horizontalHeight;
    private readonly Color selectedDisabledProColor;
    private readonly Color selectedDisabledStandardColor;
    private readonly Color selectedEnabledProColor;
    private readonly Color selectedEnabledStandardColor;
    private readonly Color taskRunningProColor;
    private readonly Color taskRunningStandardColor;
    private bool horizontalDirty;
    private Vector2 startHorizontalBreak;
    private Vector2 endHorizontalBreak;
    private Vector3[] linePoints;

    public NodeConnection()
    {
      base.\u002Ector();
    }

    public NodeDesigner OriginatingNodeDesigner
    {
      get
      {
        return this.originatingNodeDesigner;
      }
      set
      {
        this.originatingNodeDesigner = value;
      }
    }

    public NodeDesigner DestinationNodeDesigner
    {
      get
      {
        return this.destinationNodeDesigner;
      }
      set
      {
        this.destinationNodeDesigner = value;
      }
    }

    public NodeConnectionType NodeConnectionType
    {
      get
      {
        return this.nodeConnectionType;
      }
      set
      {
        this.nodeConnectionType = value;
      }
    }

    public void select()
    {
      this.selected = true;
    }

    public void deselect()
    {
      this.selected = false;
    }

    public float HorizontalHeight
    {
      set
      {
        this.horizontalHeight = value;
        this.horizontalDirty = true;
      }
    }

    public void OnEnable()
    {
      ((Object) this).set_hideFlags((HideFlags) 61);
    }

    public void LoadConnection(NodeDesigner nodeDesigner, NodeConnectionType nodeConnectionType)
    {
      this.originatingNodeDesigner = nodeDesigner;
      this.nodeConnectionType = nodeConnectionType;
      this.selected = false;
    }

    public void DrawConnection(Vector2 offset, bool disabled)
    {
      this.DrawConnection(this.OriginatingNodeDesigner.GetConnectionPosition(offset, NodeConnectionType.Outgoing), this.DestinationNodeDesigner.GetConnectionPosition(offset, NodeConnectionType.Incoming), disabled);
    }

    public void DrawConnection(Vector2 source, Vector2 destination, bool disabled)
    {
      Color color = !disabled ? Color.get_white() : new Color(0.7f, 0.7f, 0.7f);
      bool flag = Object.op_Inequality((Object) this.destinationNodeDesigner, (Object) null) && this.destinationNodeDesigner.Task != null && (double) this.destinationNodeDesigner.Task.get_NodeData().get_PushTime() != -1.0 && (double) this.destinationNodeDesigner.Task.get_NodeData().get_PushTime() >= (double) this.destinationNodeDesigner.Task.get_NodeData().get_PopTime();
      float num1 = !BehaviorDesignerPreferences.GetBool(BDPreferences.FadeNodes) ? 0.01f : 0.5f;
      if (this.selected)
        color = !disabled ? (!EditorGUIUtility.get_isProSkin() ? this.selectedEnabledStandardColor : this.selectedEnabledProColor) : (!EditorGUIUtility.get_isProSkin() ? this.selectedDisabledStandardColor : this.selectedDisabledProColor);
      else if (flag)
        color = !EditorGUIUtility.get_isProSkin() ? this.taskRunningStandardColor : this.taskRunningProColor;
      else if ((double) num1 != 0.0 && Object.op_Inequality((Object) this.destinationNodeDesigner, (Object) null) && (this.destinationNodeDesigner.Task != null && (double) this.destinationNodeDesigner.Task.get_NodeData().get_PopTime() != -1.0) && ((double) this.destinationNodeDesigner.Task.get_NodeData().get_PopTime() <= (double) Time.get_realtimeSinceStartup() && (double) Time.get_realtimeSinceStartup() - (double) this.destinationNodeDesigner.Task.get_NodeData().get_PopTime() < (double) num1))
      {
        float num2 = (float) (1.0 - ((double) Time.get_realtimeSinceStartup() - (double) this.destinationNodeDesigner.Task.get_NodeData().get_PopTime()) / (double) num1);
        Color.get_white();
        color = Color.Lerp(Color.get_white(), !EditorGUIUtility.get_isProSkin() ? this.taskRunningStandardColor : this.taskRunningProColor, num2);
      }
      Handles.set_color(color);
      if (this.horizontalDirty)
      {
        this.startHorizontalBreak = new Vector2((float) source.x, this.horizontalHeight);
        this.endHorizontalBreak = new Vector2((float) destination.x, this.horizontalHeight);
        this.horizontalDirty = false;
      }
      this.linePoints[0] = Vector2.op_Implicit(source);
      this.linePoints[1] = Vector2.op_Implicit(this.startHorizontalBreak);
      this.linePoints[2] = Vector2.op_Implicit(this.endHorizontalBreak);
      this.linePoints[3] = Vector2.op_Implicit(destination);
      Handles.DrawPolyLine(this.linePoints);
      for (int index = 0; index < this.linePoints.Length; ++index)
      {
        ref Vector3 local1 = ref this.linePoints[index];
        local1.x = (__Null) (local1.x + 1.0);
        ref Vector3 local2 = ref this.linePoints[index];
        local2.y = (__Null) (local2.y + 1.0);
      }
      Handles.DrawPolyLine(this.linePoints);
    }

    public bool Contains(Vector2 point, Vector2 offset)
    {
      Rect rect1 = this.originatingNodeDesigner.OutgoingConnectionRect(offset);
      Vector2 center = ((Rect) ref rect1).get_center();
      Vector2 vector2_1;
      ((Vector2) ref vector2_1).\u002Ector((float) center.x, this.horizontalHeight);
      if ((double) Mathf.Abs((float) (point.x - center.x)) < 7.0 && (point.y >= center.y && point.y <= vector2_1.y || point.y <= center.y && point.y >= vector2_1.y))
        return true;
      Rect rect2 = this.destinationNodeDesigner.IncomingConnectionRect(offset);
      Vector2 vector2_2;
      ((Vector2) ref vector2_2).\u002Ector((float) ((Rect) ref rect2).get_center().x, ((Rect) ref rect2).get_y());
      Vector2 vector2_3;
      ((Vector2) ref vector2_3).\u002Ector((float) vector2_2.x, this.horizontalHeight);
      return (double) Mathf.Abs((float) point.y - this.horizontalHeight) < 7.0 && (point.x <= center.x && point.x >= vector2_3.x || point.x >= center.x && point.x <= vector2_3.x) || (double) Mathf.Abs((float) (point.x - vector2_2.x)) < 7.0 && (point.y >= vector2_2.y && point.y <= vector2_3.y || point.y <= vector2_2.y && point.y >= vector2_3.y);
    }
  }
}
