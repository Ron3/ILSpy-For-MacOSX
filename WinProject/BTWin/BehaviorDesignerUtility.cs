// Decompiled with JetBrains decompiler
// Type: BehaviorDesigner.Editor.BehaviorDesignerUtility
// Assembly: BehaviorDesignerEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 99CE4D00-DFA2-42D1-ABFC-D630AB4C1372
// Assembly location: C:\Users\Ron\Desktop\bt\BehaviorDesignerEditor.dll

using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BehaviorDesigner.Editor
{
  public static class BehaviorDesignerUtility
  {
    private static GUIStyle graphStatusGUIStyle = (GUIStyle) null;
    private static GUIStyle taskFoldoutGUIStyle = (GUIStyle) null;
    private static GUIStyle taskTitleGUIStyle = (GUIStyle) null;
    private static GUIStyle[] taskGUIStyle = new GUIStyle[9];
    private static GUIStyle[] taskCompactGUIStyle = new GUIStyle[9];
    private static GUIStyle[] taskSelectedGUIStyle = new GUIStyle[9];
    private static GUIStyle[] taskSelectedCompactGUIStyle = new GUIStyle[9];
    private static GUIStyle taskRunningGUIStyle = (GUIStyle) null;
    private static GUIStyle taskRunningCompactGUIStyle = (GUIStyle) null;
    private static GUIStyle taskRunningSelectedGUIStyle = (GUIStyle) null;
    private static GUIStyle taskRunningSelectedCompactGUIStyle = (GUIStyle) null;
    private static GUIStyle taskIdentifyGUIStyle = (GUIStyle) null;
    private static GUIStyle taskIdentifyCompactGUIStyle = (GUIStyle) null;
    private static GUIStyle taskIdentifySelectedGUIStyle = (GUIStyle) null;
    private static GUIStyle taskIdentifySelectedCompactGUIStyle = (GUIStyle) null;
    private static GUIStyle taskHighlightGUIStyle = (GUIStyle) null;
    private static GUIStyle taskHighlightCompactGUIStyle = (GUIStyle) null;
    private static GUIStyle taskCommentGUIStyle = (GUIStyle) null;
    private static GUIStyle taskCommentLeftAlignGUIStyle = (GUIStyle) null;
    private static GUIStyle taskCommentRightAlignGUIStyle = (GUIStyle) null;
    private static GUIStyle taskDescriptionGUIStyle = (GUIStyle) null;
    private static GUIStyle graphBackgroundGUIStyle = (GUIStyle) null;
    private static GUIStyle selectionGUIStyle = (GUIStyle) null;
    private static GUIStyle sharedVariableToolbarPopup = (GUIStyle) null;
    private static GUIStyle labelWrapGUIStyle = (GUIStyle) null;
    private static GUIStyle tolbarButtonLeftAlignGUIStyle = (GUIStyle) null;
    private static GUIStyle toolbarLabelGUIStyle = (GUIStyle) null;
    private static GUIStyle taskInspectorCommentGUIStyle = (GUIStyle) null;
    private static GUIStyle taskInspectorGUIStyle = (GUIStyle) null;
    private static GUIStyle toolbarButtonSelectionGUIStyle = (GUIStyle) null;
    private static GUIStyle propertyBoxGUIStyle = (GUIStyle) null;
    private static GUIStyle preferencesPaneGUIStyle = (GUIStyle) null;
    private static GUIStyle plainButtonGUIStyle = (GUIStyle) null;
    private static GUIStyle transparentButtonGUIStyle = (GUIStyle) null;
    private static GUIStyle transparentButtonOffsetGUIStyle = (GUIStyle) null;
    private static GUIStyle buttonGUIStyle = (GUIStyle) null;
    private static GUIStyle plainTextureGUIStyle = (GUIStyle) null;
    private static GUIStyle arrowSeparatorGUIStyle = (GUIStyle) null;
    private static GUIStyle selectedBackgroundGUIStyle = (GUIStyle) null;
    private static GUIStyle errorListDarkBackground = (GUIStyle) null;
    private static GUIStyle errorListLightBackground = (GUIStyle) null;
    private static GUIStyle welcomeScreenIntroGUIStyle = (GUIStyle) null;
    private static GUIStyle welcomeScreenTextHeaderGUIStyle = (GUIStyle) null;
    private static GUIStyle welcomeScreenTextDescriptionGUIStyle = (GUIStyle) null;
    private static Texture2D[] taskBorderTexture = new Texture2D[9];
    private static Texture2D taskBorderRunningTexture = (Texture2D) null;
    private static Texture2D taskBorderIdentifyTexture = (Texture2D) null;
    private static Texture2D[] taskConnectionTopTexture = new Texture2D[9];
    private static Texture2D[] taskConnectionBottomTexture = new Texture2D[9];
    private static Texture2D taskConnectionRunningTopTexture = (Texture2D) null;
    private static Texture2D taskConnectionRunningBottomTexture = (Texture2D) null;
    private static Texture2D taskConnectionIdentifyTopTexture = (Texture2D) null;
    private static Texture2D taskConnectionIdentifyBottomTexture = (Texture2D) null;
    private static Texture2D taskConnectionCollapsedTexture = (Texture2D) null;
    private static Texture2D contentSeparatorTexture = (Texture2D) null;
    private static Texture2D docTexture = (Texture2D) null;
    private static Texture2D gearTexture = (Texture2D) null;
    private static Texture2D[] colorSelectorTexture = new Texture2D[9];
    private static Texture2D variableButtonTexture = (Texture2D) null;
    private static Texture2D variableButtonSelectedTexture = (Texture2D) null;
    private static Texture2D variableWatchButtonTexture = (Texture2D) null;
    private static Texture2D variableWatchButtonSelectedTexture = (Texture2D) null;
    private static Texture2D referencedTexture = (Texture2D) null;
    private static Texture2D conditionalAbortSelfTexture = (Texture2D) null;
    private static Texture2D conditionalAbortLowerPriorityTexture = (Texture2D) null;
    private static Texture2D conditionalAbortBothTexture = (Texture2D) null;
    private static Texture2D deleteButtonTexture = (Texture2D) null;
    private static Texture2D variableDeleteButtonTexture = (Texture2D) null;
    private static Texture2D downArrowButtonTexture = (Texture2D) null;
    private static Texture2D upArrowButtonTexture = (Texture2D) null;
    private static Texture2D variableMapButtonTexture = (Texture2D) null;
    private static Texture2D identifyButtonTexture = (Texture2D) null;
    private static Texture2D breakpointTexture = (Texture2D) null;
    private static Texture2D errorIconTexture = (Texture2D) null;
    private static Texture2D smallErrorIconTexture = (Texture2D) null;
    private static Texture2D enableTaskTexture = (Texture2D) null;
    private static Texture2D disableTaskTexture = (Texture2D) null;
    private static Texture2D expandTaskTexture = (Texture2D) null;
    private static Texture2D collapseTaskTexture = (Texture2D) null;
    private static Texture2D executionSuccessTexture = (Texture2D) null;
    private static Texture2D executionFailureTexture = (Texture2D) null;
    private static Texture2D executionSuccessRepeatTexture = (Texture2D) null;
    private static Texture2D executionFailureRepeatTexture = (Texture2D) null;
    public static Texture2D historyBackwardTexture = (Texture2D) null;
    public static Texture2D historyForwardTexture = (Texture2D) null;
    private static Texture2D playTexture = (Texture2D) null;
    private static Texture2D pauseTexture = (Texture2D) null;
    private static Texture2D stepTexture = (Texture2D) null;
    private static Texture2D screenshotBackgroundTexture = (Texture2D) null;
    private static Regex camelCaseRegex = new Regex("(?<=[A-Z])(?=[A-Z][a-z])|(?<=[^A-Z])(?=[A-Z])|(?<=[A-Za-z])(?=[^A-Za-z])", RegexOptions.IgnorePatternWhitespace);
    private static Dictionary<string, string> camelCaseSplit = new Dictionary<string, string>();
    [NonSerialized]
    private static Dictionary<Type, Dictionary<FieldInfo, bool>> attributeFieldCache = new Dictionary<Type, Dictionary<FieldInfo, bool>>();
    private static Dictionary<string, Texture2D> textureCache = new Dictionary<string, Texture2D>();
    private static Dictionary<string, Texture2D> iconCache = new Dictionary<string, Texture2D>();
    public const string Version = "1.5.11";
    public const int ToolBarHeight = 18;
    public const int PropertyBoxWidth = 300;
    public const int ScrollBarSize = 15;
    public const int EditorWindowTabHeight = 21;
    public const int PreferencesPaneWidth = 290;
    public const int PreferencesPaneHeight = 368;
    public const float GraphZoomMax = 1f;
    public const float GraphZoomMin = 0.4f;
    public const float GraphZoomSensitivity = 150f;
    public const float GraphAutoScrollEdgeDistance = 15f;
    public const float GraphAutoScrollEdgeSpeed = 3f;
    public const int LineSelectionThreshold = 7;
    public const int TaskBackgroundShadowSize = 3;
    public const int TitleHeight = 20;
    public const int TitleCompactHeight = 28;
    public const int IconAreaHeight = 52;
    public const int IconSize = 44;
    public const int IconBorderSize = 46;
    public const int CompactAreaHeight = 22;
    public const int ConnectionWidth = 42;
    public const int TopConnectionHeight = 14;
    public const int BottomConnectionHeight = 16;
    public const int TaskConnectionCollapsedWidth = 26;
    public const int TaskConnectionCollapsedHeight = 6;
    public const int MinWidth = 100;
    public const int MaxWidth = 220;
    public const int MaxCommentHeight = 100;
    public const int TextPadding = 20;
    public const float NodeFadeDuration = 0.5f;
    public const int IdentifyUpdateFadeTime = 500;
    public const int MaxIdentifyUpdateCount = 2000;
    public const float InterruptTaskHighlightDuration = 0.75f;
    public const int TaskPropertiesLabelWidth = 150;
    public const int MaxTaskDescriptionBoxWidth = 400;
    public const int MaxTaskDescriptionBoxHeight = 300;
    public const int MinorGridTickSpacing = 10;
    public const int MajorGridTickSpacing = 50;
    public const int RepaintGUICount = 1;
    public const float UpdateCheckInterval = 1f;

    public static GUIStyle GraphStatusGUIStyle
    {
      get
      {
        if (BehaviorDesignerUtility.graphStatusGUIStyle == null)
          BehaviorDesignerUtility.InitGraphStatusGUIStyle();
        return BehaviorDesignerUtility.graphStatusGUIStyle;
      }
    }

    public static GUIStyle TaskFoldoutGUIStyle
    {
      get
      {
        if (BehaviorDesignerUtility.taskFoldoutGUIStyle == null)
          BehaviorDesignerUtility.InitTaskFoldoutGUIStyle();
        return BehaviorDesignerUtility.taskFoldoutGUIStyle;
      }
    }

    public static GUIStyle TaskTitleGUIStyle
    {
      get
      {
        if (BehaviorDesignerUtility.taskTitleGUIStyle == null)
          BehaviorDesignerUtility.InitTaskTitleGUIStyle();
        return BehaviorDesignerUtility.taskTitleGUIStyle;
      }
    }

    public static GUIStyle GetTaskGUIStyle(int colorIndex)
    {
      if (BehaviorDesignerUtility.taskGUIStyle[colorIndex] == null)
        BehaviorDesignerUtility.InitTaskGUIStyle(colorIndex);
      return BehaviorDesignerUtility.taskGUIStyle[colorIndex];
    }

    public static GUIStyle GetTaskCompactGUIStyle(int colorIndex)
    {
      if (BehaviorDesignerUtility.taskCompactGUIStyle[colorIndex] == null)
        BehaviorDesignerUtility.InitTaskCompactGUIStyle(colorIndex);
      return BehaviorDesignerUtility.taskCompactGUIStyle[colorIndex];
    }

    public static GUIStyle GetTaskSelectedGUIStyle(int colorIndex)
    {
      if (BehaviorDesignerUtility.taskSelectedGUIStyle[colorIndex] == null)
        BehaviorDesignerUtility.InitTaskSelectedGUIStyle(colorIndex);
      return BehaviorDesignerUtility.taskSelectedGUIStyle[colorIndex];
    }

    public static GUIStyle GetTaskSelectedCompactGUIStyle(int colorIndex)
    {
      if (BehaviorDesignerUtility.taskSelectedCompactGUIStyle[colorIndex] == null)
        BehaviorDesignerUtility.InitTaskSelectedCompactGUIStyle(colorIndex);
      return BehaviorDesignerUtility.taskSelectedCompactGUIStyle[colorIndex];
    }

    public static GUIStyle TaskRunningGUIStyle
    {
      get
      {
        if (BehaviorDesignerUtility.taskRunningGUIStyle == null)
          BehaviorDesignerUtility.InitTaskRunningGUIStyle();
        return BehaviorDesignerUtility.taskRunningGUIStyle;
      }
    }

    public static GUIStyle TaskRunningCompactGUIStyle
    {
      get
      {
        if (BehaviorDesignerUtility.taskRunningCompactGUIStyle == null)
          BehaviorDesignerUtility.InitTaskRunningCompactGUIStyle();
        return BehaviorDesignerUtility.taskRunningCompactGUIStyle;
      }
    }

    public static GUIStyle TaskRunningSelectedGUIStyle
    {
      get
      {
        if (BehaviorDesignerUtility.taskRunningSelectedGUIStyle == null)
          BehaviorDesignerUtility.InitTaskRunningSelectedGUIStyle();
        return BehaviorDesignerUtility.taskRunningSelectedGUIStyle;
      }
    }

    public static GUIStyle TaskRunningSelectedCompactGUIStyle
    {
      get
      {
        if (BehaviorDesignerUtility.taskRunningSelectedCompactGUIStyle == null)
          BehaviorDesignerUtility.InitTaskRunningSelectedCompactGUIStyle();
        return BehaviorDesignerUtility.taskRunningSelectedCompactGUIStyle;
      }
    }

    public static GUIStyle TaskIdentifyGUIStyle
    {
      get
      {
        if (BehaviorDesignerUtility.taskIdentifyGUIStyle == null)
          BehaviorDesignerUtility.InitTaskIdentifyGUIStyle();
        return BehaviorDesignerUtility.taskIdentifyGUIStyle;
      }
    }

    public static GUIStyle TaskIdentifyCompactGUIStyle
    {
      get
      {
        if (BehaviorDesignerUtility.taskIdentifyCompactGUIStyle == null)
          BehaviorDesignerUtility.InitTaskIdentifyCompactGUIStyle();
        return BehaviorDesignerUtility.taskIdentifyCompactGUIStyle;
      }
    }

    public static GUIStyle TaskIdentifySelectedGUIStyle
    {
      get
      {
        if (BehaviorDesignerUtility.taskIdentifySelectedGUIStyle == null)
          BehaviorDesignerUtility.InitTaskIdentifySelectedGUIStyle();
        return BehaviorDesignerUtility.taskIdentifySelectedGUIStyle;
      }
    }

    public static GUIStyle TaskIdentifySelectedCompactGUIStyle
    {
      get
      {
        if (BehaviorDesignerUtility.taskIdentifySelectedCompactGUIStyle == null)
          BehaviorDesignerUtility.InitTaskIdentifySelectedCompactGUIStyle();
        return BehaviorDesignerUtility.taskIdentifySelectedCompactGUIStyle;
      }
    }

    public static GUIStyle TaskHighlightGUIStyle
    {
      get
      {
        if (BehaviorDesignerUtility.taskHighlightGUIStyle == null)
          BehaviorDesignerUtility.InitTaskHighlightGUIStyle();
        return BehaviorDesignerUtility.taskHighlightGUIStyle;
      }
    }

    public static GUIStyle TaskHighlightCompactGUIStyle
    {
      get
      {
        if (BehaviorDesignerUtility.taskHighlightCompactGUIStyle == null)
          BehaviorDesignerUtility.InitTaskHighlightCompactGUIStyle();
        return BehaviorDesignerUtility.taskHighlightCompactGUIStyle;
      }
    }

    public static GUIStyle TaskCommentGUIStyle
    {
      get
      {
        if (BehaviorDesignerUtility.taskCommentGUIStyle == null)
          BehaviorDesignerUtility.InitTaskCommentGUIStyle();
        return BehaviorDesignerUtility.taskCommentGUIStyle;
      }
    }

    public static GUIStyle TaskCommentLeftAlignGUIStyle
    {
      get
      {
        if (BehaviorDesignerUtility.taskCommentLeftAlignGUIStyle == null)
          BehaviorDesignerUtility.InitTaskCommentLeftAlignGUIStyle();
        return BehaviorDesignerUtility.taskCommentLeftAlignGUIStyle;
      }
    }

    public static GUIStyle TaskCommentRightAlignGUIStyle
    {
      get
      {
        if (BehaviorDesignerUtility.taskCommentRightAlignGUIStyle == null)
          BehaviorDesignerUtility.InitTaskCommentRightAlignGUIStyle();
        return BehaviorDesignerUtility.taskCommentRightAlignGUIStyle;
      }
    }

    public static GUIStyle TaskDescriptionGUIStyle
    {
      get
      {
        if (BehaviorDesignerUtility.taskDescriptionGUIStyle == null)
          BehaviorDesignerUtility.InitTaskDescriptionGUIStyle();
        return BehaviorDesignerUtility.taskDescriptionGUIStyle;
      }
    }

    public static GUIStyle GraphBackgroundGUIStyle
    {
      get
      {
        if (BehaviorDesignerUtility.graphBackgroundGUIStyle == null)
          BehaviorDesignerUtility.InitGraphBackgroundGUIStyle();
        return BehaviorDesignerUtility.graphBackgroundGUIStyle;
      }
    }

    public static GUIStyle SelectionGUIStyle
    {
      get
      {
        if (BehaviorDesignerUtility.selectionGUIStyle == null)
          BehaviorDesignerUtility.InitSelectionGUIStyle();
        return BehaviorDesignerUtility.selectionGUIStyle;
      }
    }

    public static GUIStyle SharedVariableToolbarPopup
    {
      get
      {
        if (BehaviorDesignerUtility.sharedVariableToolbarPopup == null)
          BehaviorDesignerUtility.InitSharedVariableToolbarPopup();
        return BehaviorDesignerUtility.sharedVariableToolbarPopup;
      }
    }

    public static GUIStyle LabelWrapGUIStyle
    {
      get
      {
        if (BehaviorDesignerUtility.labelWrapGUIStyle == null)
          BehaviorDesignerUtility.InitLabelWrapGUIStyle();
        return BehaviorDesignerUtility.labelWrapGUIStyle;
      }
    }

    public static GUIStyle ToolbarButtonLeftAlignGUIStyle
    {
      get
      {
        if (BehaviorDesignerUtility.tolbarButtonLeftAlignGUIStyle == null)
          BehaviorDesignerUtility.InitToolbarButtonLeftAlignGUIStyle();
        return BehaviorDesignerUtility.tolbarButtonLeftAlignGUIStyle;
      }
    }

    public static GUIStyle ToolbarLabelGUIStyle
    {
      get
      {
        if (BehaviorDesignerUtility.toolbarLabelGUIStyle == null)
          BehaviorDesignerUtility.InitToolbarLabelGUIStyle();
        return BehaviorDesignerUtility.toolbarLabelGUIStyle;
      }
    }

    public static GUIStyle TaskInspectorCommentGUIStyle
    {
      get
      {
        if (BehaviorDesignerUtility.taskInspectorCommentGUIStyle == null)
          BehaviorDesignerUtility.InitTaskInspectorCommentGUIStyle();
        return BehaviorDesignerUtility.taskInspectorCommentGUIStyle;
      }
    }

    public static GUIStyle TaskInspectorGUIStyle
    {
      get
      {
        if (BehaviorDesignerUtility.taskInspectorGUIStyle == null)
          BehaviorDesignerUtility.InitTaskInspectorGUIStyle();
        return BehaviorDesignerUtility.taskInspectorGUIStyle;
      }
    }

    public static GUIStyle ToolbarButtonSelectionGUIStyle
    {
      get
      {
        if (BehaviorDesignerUtility.toolbarButtonSelectionGUIStyle == null)
          BehaviorDesignerUtility.InitToolbarButtonSelectionGUIStyle();
        return BehaviorDesignerUtility.toolbarButtonSelectionGUIStyle;
      }
    }

    public static GUIStyle PropertyBoxGUIStyle
    {
      get
      {
        if (BehaviorDesignerUtility.propertyBoxGUIStyle == null)
          BehaviorDesignerUtility.InitPropertyBoxGUIStyle();
        return BehaviorDesignerUtility.propertyBoxGUIStyle;
      }
    }

    public static GUIStyle PreferencesPaneGUIStyle
    {
      get
      {
        if (BehaviorDesignerUtility.preferencesPaneGUIStyle == null)
          BehaviorDesignerUtility.InitPreferencesPaneGUIStyle();
        return BehaviorDesignerUtility.preferencesPaneGUIStyle;
      }
    }

    public static GUIStyle PlainButtonGUIStyle
    {
      get
      {
        if (BehaviorDesignerUtility.plainButtonGUIStyle == null)
          BehaviorDesignerUtility.InitPlainButtonGUIStyle();
        return BehaviorDesignerUtility.plainButtonGUIStyle;
      }
    }

    public static GUIStyle TransparentButtonGUIStyle
    {
      get
      {
        if (BehaviorDesignerUtility.transparentButtonGUIStyle == null)
          BehaviorDesignerUtility.InitTransparentButtonGUIStyle();
        return BehaviorDesignerUtility.transparentButtonGUIStyle;
      }
    }

    public static GUIStyle TransparentButtonOffsetGUIStyle
    {
      get
      {
        if (BehaviorDesignerUtility.transparentButtonOffsetGUIStyle == null)
          BehaviorDesignerUtility.InitTransparentButtonOffsetGUIStyle();
        return BehaviorDesignerUtility.transparentButtonOffsetGUIStyle;
      }
    }

    public static GUIStyle ButtonGUIStyle
    {
      get
      {
        if (BehaviorDesignerUtility.buttonGUIStyle == null)
          BehaviorDesignerUtility.InitButtonGUIStyle();
        return BehaviorDesignerUtility.buttonGUIStyle;
      }
    }

    public static GUIStyle PlainTextureGUIStyle
    {
      get
      {
        if (BehaviorDesignerUtility.plainTextureGUIStyle == null)
          BehaviorDesignerUtility.InitPlainTextureGUIStyle();
        return BehaviorDesignerUtility.plainTextureGUIStyle;
      }
    }

    public static GUIStyle ArrowSeparatorGUIStyle
    {
      get
      {
        if (BehaviorDesignerUtility.arrowSeparatorGUIStyle == null)
          BehaviorDesignerUtility.InitArrowSeparatorGUIStyle();
        return BehaviorDesignerUtility.arrowSeparatorGUIStyle;
      }
    }

    public static GUIStyle SelectedBackgroundGUIStyle
    {
      get
      {
        if (BehaviorDesignerUtility.selectedBackgroundGUIStyle == null)
          BehaviorDesignerUtility.InitSelectedBackgroundGUIStyle();
        return BehaviorDesignerUtility.selectedBackgroundGUIStyle;
      }
    }

    public static GUIStyle ErrorListDarkBackground
    {
      get
      {
        if (BehaviorDesignerUtility.errorListDarkBackground == null)
          BehaviorDesignerUtility.InitErrorListDarkBackground();
        return BehaviorDesignerUtility.errorListDarkBackground;
      }
    }

    public static GUIStyle ErrorListLightBackground
    {
      get
      {
        if (BehaviorDesignerUtility.errorListLightBackground == null)
          BehaviorDesignerUtility.InitErrorListLightBackground();
        return BehaviorDesignerUtility.errorListLightBackground;
      }
    }

    public static GUIStyle WelcomeScreenIntroGUIStyle
    {
      get
      {
        if (BehaviorDesignerUtility.welcomeScreenIntroGUIStyle == null)
          BehaviorDesignerUtility.InitWelcomeScreenIntroGUIStyle();
        return BehaviorDesignerUtility.welcomeScreenIntroGUIStyle;
      }
    }

    public static GUIStyle WelcomeScreenTextHeaderGUIStyle
    {
      get
      {
        if (BehaviorDesignerUtility.welcomeScreenTextHeaderGUIStyle == null)
          BehaviorDesignerUtility.InitWelcomeScreenTextHeaderGUIStyle();
        return BehaviorDesignerUtility.welcomeScreenTextHeaderGUIStyle;
      }
    }

    public static GUIStyle WelcomeScreenTextDescriptionGUIStyle
    {
      get
      {
        if (BehaviorDesignerUtility.welcomeScreenTextDescriptionGUIStyle == null)
          BehaviorDesignerUtility.InitWelcomeScreenTextDescriptionGUIStyle();
        return BehaviorDesignerUtility.welcomeScreenTextDescriptionGUIStyle;
      }
    }

    public static Texture2D GetTaskBorderTexture(int colorIndex)
    {
      if (Object.op_Equality((Object) BehaviorDesignerUtility.taskBorderTexture[colorIndex], (Object) null))
        BehaviorDesignerUtility.InitTaskBorderTexture(colorIndex);
      return BehaviorDesignerUtility.taskBorderTexture[colorIndex];
    }

    public static Texture2D TaskBorderRunningTexture
    {
      get
      {
        if (Object.op_Equality((Object) BehaviorDesignerUtility.taskBorderRunningTexture, (Object) null))
          BehaviorDesignerUtility.InitTaskBorderRunningTexture();
        return BehaviorDesignerUtility.taskBorderRunningTexture;
      }
    }

    public static Texture2D TaskBorderIdentifyTexture
    {
      get
      {
        if (Object.op_Equality((Object) BehaviorDesignerUtility.taskBorderIdentifyTexture, (Object) null))
          BehaviorDesignerUtility.InitTaskBorderIdentifyTexture();
        return BehaviorDesignerUtility.taskBorderIdentifyTexture;
      }
    }

    public static Texture2D GetTaskConnectionTopTexture(int colorIndex)
    {
      if (Object.op_Equality((Object) BehaviorDesignerUtility.taskConnectionTopTexture[colorIndex], (Object) null))
        BehaviorDesignerUtility.InitTaskConnectionTopTexture(colorIndex);
      return BehaviorDesignerUtility.taskConnectionTopTexture[colorIndex];
    }

    public static Texture2D GetTaskConnectionBottomTexture(int colorIndex)
    {
      if (Object.op_Equality((Object) BehaviorDesignerUtility.taskConnectionBottomTexture[colorIndex], (Object) null))
        BehaviorDesignerUtility.InitTaskConnectionBottomTexture(colorIndex);
      return BehaviorDesignerUtility.taskConnectionBottomTexture[colorIndex];
    }

    public static Texture2D TaskConnectionRunningTopTexture
    {
      get
      {
        if (Object.op_Equality((Object) BehaviorDesignerUtility.taskConnectionRunningTopTexture, (Object) null))
          BehaviorDesignerUtility.InitTaskConnectionRunningTopTexture();
        return BehaviorDesignerUtility.taskConnectionRunningTopTexture;
      }
    }

    public static Texture2D TaskConnectionRunningBottomTexture
    {
      get
      {
        if (Object.op_Equality((Object) BehaviorDesignerUtility.taskConnectionRunningBottomTexture, (Object) null))
          BehaviorDesignerUtility.InitTaskConnectionRunningBottomTexture();
        return BehaviorDesignerUtility.taskConnectionRunningBottomTexture;
      }
    }

    public static Texture2D TaskConnectionIdentifyTopTexture
    {
      get
      {
        if (Object.op_Equality((Object) BehaviorDesignerUtility.taskConnectionIdentifyTopTexture, (Object) null))
          BehaviorDesignerUtility.InitTaskConnectionIdentifyTopTexture();
        return BehaviorDesignerUtility.taskConnectionIdentifyTopTexture;
      }
    }

    public static Texture2D TaskConnectionIdentifyBottomTexture
    {
      get
      {
        if (Object.op_Equality((Object) BehaviorDesignerUtility.taskConnectionIdentifyBottomTexture, (Object) null))
          BehaviorDesignerUtility.InitTaskConnectionIdentifyBottomTexture();
        return BehaviorDesignerUtility.taskConnectionIdentifyBottomTexture;
      }
    }

    public static Texture2D TaskConnectionCollapsedTexture
    {
      get
      {
        if (Object.op_Equality((Object) BehaviorDesignerUtility.taskConnectionCollapsedTexture, (Object) null))
          BehaviorDesignerUtility.InitTaskConnectionCollapsedTexture();
        return BehaviorDesignerUtility.taskConnectionCollapsedTexture;
      }
    }

    public static Texture2D ContentSeparatorTexture
    {
      get
      {
        if (Object.op_Equality((Object) BehaviorDesignerUtility.contentSeparatorTexture, (Object) null))
          BehaviorDesignerUtility.InitContentSeparatorTexture();
        return BehaviorDesignerUtility.contentSeparatorTexture;
      }
    }

    public static Texture2D DocTexture
    {
      get
      {
        if (Object.op_Equality((Object) BehaviorDesignerUtility.docTexture, (Object) null))
          BehaviorDesignerUtility.InitDocTexture();
        return BehaviorDesignerUtility.docTexture;
      }
    }

    public static Texture2D GearTexture
    {
      get
      {
        if (Object.op_Equality((Object) BehaviorDesignerUtility.gearTexture, (Object) null))
          BehaviorDesignerUtility.InitGearTexture();
        return BehaviorDesignerUtility.gearTexture;
      }
    }

    public static Texture2D ColorSelectorTexture(int colorIndex)
    {
      if (Object.op_Equality((Object) BehaviorDesignerUtility.colorSelectorTexture[colorIndex], (Object) null))
        BehaviorDesignerUtility.InitColorSelectorTexture(colorIndex);
      return BehaviorDesignerUtility.colorSelectorTexture[colorIndex];
    }

    public static Texture2D VariableButtonTexture
    {
      get
      {
        if (Object.op_Equality((Object) BehaviorDesignerUtility.variableButtonTexture, (Object) null))
          BehaviorDesignerUtility.InitVariableButtonTexture();
        return BehaviorDesignerUtility.variableButtonTexture;
      }
    }

    public static Texture2D VariableButtonSelectedTexture
    {
      get
      {
        if (Object.op_Equality((Object) BehaviorDesignerUtility.variableButtonSelectedTexture, (Object) null))
          BehaviorDesignerUtility.InitVariableButtonSelectedTexture();
        return BehaviorDesignerUtility.variableButtonSelectedTexture;
      }
    }

    public static Texture2D VariableWatchButtonTexture
    {
      get
      {
        if (Object.op_Equality((Object) BehaviorDesignerUtility.variableWatchButtonTexture, (Object) null))
          BehaviorDesignerUtility.InitVariableWatchButtonTexture();
        return BehaviorDesignerUtility.variableWatchButtonTexture;
      }
    }

    public static Texture2D VariableWatchButtonSelectedTexture
    {
      get
      {
        if (Object.op_Equality((Object) BehaviorDesignerUtility.variableWatchButtonSelectedTexture, (Object) null))
          BehaviorDesignerUtility.InitVariableWatchButtonSelectedTexture();
        return BehaviorDesignerUtility.variableWatchButtonSelectedTexture;
      }
    }

    public static Texture2D ReferencedTexture
    {
      get
      {
        if (Object.op_Equality((Object) BehaviorDesignerUtility.referencedTexture, (Object) null))
          BehaviorDesignerUtility.InitReferencedTexture();
        return BehaviorDesignerUtility.referencedTexture;
      }
    }

    public static Texture2D ConditionalAbortSelfTexture
    {
      get
      {
        if (Object.op_Equality((Object) BehaviorDesignerUtility.conditionalAbortSelfTexture, (Object) null))
          BehaviorDesignerUtility.InitConditionalAbortSelfTexture();
        return BehaviorDesignerUtility.conditionalAbortSelfTexture;
      }
    }

    public static Texture2D ConditionalAbortLowerPriorityTexture
    {
      get
      {
        if (Object.op_Equality((Object) BehaviorDesignerUtility.conditionalAbortLowerPriorityTexture, (Object) null))
          BehaviorDesignerUtility.InitConditionalAbortLowerPriorityTexture();
        return BehaviorDesignerUtility.conditionalAbortLowerPriorityTexture;
      }
    }

    public static Texture2D ConditionalAbortBothTexture
    {
      get
      {
        if (Object.op_Equality((Object) BehaviorDesignerUtility.conditionalAbortBothTexture, (Object) null))
          BehaviorDesignerUtility.InitConditionalAbortBothTexture();
        return BehaviorDesignerUtility.conditionalAbortBothTexture;
      }
    }

    public static Texture2D DeleteButtonTexture
    {
      get
      {
        if (Object.op_Equality((Object) BehaviorDesignerUtility.deleteButtonTexture, (Object) null))
          BehaviorDesignerUtility.InitDeleteButtonTexture();
        return BehaviorDesignerUtility.deleteButtonTexture;
      }
    }

    public static Texture2D VariableDeleteButtonTexture
    {
      get
      {
        if (Object.op_Equality((Object) BehaviorDesignerUtility.variableDeleteButtonTexture, (Object) null))
          BehaviorDesignerUtility.InitVariableDeleteButtonTexture();
        return BehaviorDesignerUtility.variableDeleteButtonTexture;
      }
    }

    public static Texture2D DownArrowButtonTexture
    {
      get
      {
        if (Object.op_Equality((Object) BehaviorDesignerUtility.downArrowButtonTexture, (Object) null))
          BehaviorDesignerUtility.InitDownArrowButtonTexture();
        return BehaviorDesignerUtility.downArrowButtonTexture;
      }
    }

    public static Texture2D UpArrowButtonTexture
    {
      get
      {
        if (Object.op_Equality((Object) BehaviorDesignerUtility.upArrowButtonTexture, (Object) null))
          BehaviorDesignerUtility.InitUpArrowButtonTexture();
        return BehaviorDesignerUtility.upArrowButtonTexture;
      }
    }

    public static Texture2D VariableMapButtonTexture
    {
      get
      {
        if (Object.op_Equality((Object) BehaviorDesignerUtility.variableMapButtonTexture, (Object) null))
          BehaviorDesignerUtility.InitVariableMapButtonTexture();
        return BehaviorDesignerUtility.variableMapButtonTexture;
      }
    }

    public static Texture2D IdentifyButtonTexture
    {
      get
      {
        if (Object.op_Equality((Object) BehaviorDesignerUtility.identifyButtonTexture, (Object) null))
          BehaviorDesignerUtility.InitIdentifyButtonTexture();
        return BehaviorDesignerUtility.identifyButtonTexture;
      }
    }

    public static Texture2D BreakpointTexture
    {
      get
      {
        if (Object.op_Equality((Object) BehaviorDesignerUtility.breakpointTexture, (Object) null))
          BehaviorDesignerUtility.InitBreakpointTexture();
        return BehaviorDesignerUtility.breakpointTexture;
      }
    }

    public static Texture2D ErrorIconTexture
    {
      get
      {
        if (Object.op_Equality((Object) BehaviorDesignerUtility.errorIconTexture, (Object) null))
          BehaviorDesignerUtility.InitErrorIconTexture();
        return BehaviorDesignerUtility.errorIconTexture;
      }
    }

    public static Texture2D SmallErrorIconTexture
    {
      get
      {
        if (Object.op_Equality((Object) BehaviorDesignerUtility.smallErrorIconTexture, (Object) null))
          BehaviorDesignerUtility.InitSmallErrorIconTexture();
        return BehaviorDesignerUtility.smallErrorIconTexture;
      }
    }

    public static Texture2D EnableTaskTexture
    {
      get
      {
        if (Object.op_Equality((Object) BehaviorDesignerUtility.enableTaskTexture, (Object) null))
          BehaviorDesignerUtility.InitEnableTaskTexture();
        return BehaviorDesignerUtility.enableTaskTexture;
      }
    }

    public static Texture2D DisableTaskTexture
    {
      get
      {
        if (Object.op_Equality((Object) BehaviorDesignerUtility.disableTaskTexture, (Object) null))
          BehaviorDesignerUtility.InitDisableTaskTexture();
        return BehaviorDesignerUtility.disableTaskTexture;
      }
    }

    public static Texture2D ExpandTaskTexture
    {
      get
      {
        if (Object.op_Equality((Object) BehaviorDesignerUtility.expandTaskTexture, (Object) null))
          BehaviorDesignerUtility.InitExpandTaskTexture();
        return BehaviorDesignerUtility.expandTaskTexture;
      }
    }

    public static Texture2D CollapseTaskTexture
    {
      get
      {
        if (Object.op_Equality((Object) BehaviorDesignerUtility.collapseTaskTexture, (Object) null))
          BehaviorDesignerUtility.InitCollapseTaskTexture();
        return BehaviorDesignerUtility.collapseTaskTexture;
      }
    }

    public static Texture2D ExecutionSuccessTexture
    {
      get
      {
        if (Object.op_Equality((Object) BehaviorDesignerUtility.executionSuccessTexture, (Object) null))
          BehaviorDesignerUtility.InitExecutionSuccessTexture();
        return BehaviorDesignerUtility.executionSuccessTexture;
      }
    }

    public static Texture2D ExecutionFailureTexture
    {
      get
      {
        if (Object.op_Equality((Object) BehaviorDesignerUtility.executionFailureTexture, (Object) null))
          BehaviorDesignerUtility.InitExecutionFailureTexture();
        return BehaviorDesignerUtility.executionFailureTexture;
      }
    }

    public static Texture2D ExecutionSuccessRepeatTexture
    {
      get
      {
        if (Object.op_Equality((Object) BehaviorDesignerUtility.executionSuccessRepeatTexture, (Object) null))
          BehaviorDesignerUtility.InitExecutionSuccessRepeatTexture();
        return BehaviorDesignerUtility.executionSuccessRepeatTexture;
      }
    }

    public static Texture2D ExecutionFailureRepeatTexture
    {
      get
      {
        if (Object.op_Equality((Object) BehaviorDesignerUtility.executionFailureRepeatTexture, (Object) null))
          BehaviorDesignerUtility.InitExecutionFailureRepeatTexture();
        return BehaviorDesignerUtility.executionFailureRepeatTexture;
      }
    }

    public static Texture2D HistoryBackwardTexture
    {
      get
      {
        if (Object.op_Equality((Object) BehaviorDesignerUtility.historyBackwardTexture, (Object) null))
          BehaviorDesignerUtility.InitHistoryBackwardTexture();
        return BehaviorDesignerUtility.historyBackwardTexture;
      }
    }

    public static Texture2D HistoryForwardTexture
    {
      get
      {
        if (Object.op_Equality((Object) BehaviorDesignerUtility.historyForwardTexture, (Object) null))
          BehaviorDesignerUtility.InitHistoryForwardTexture();
        return BehaviorDesignerUtility.historyForwardTexture;
      }
    }

    public static Texture2D PlayTexture
    {
      get
      {
        if (Object.op_Equality((Object) BehaviorDesignerUtility.playTexture, (Object) null))
          BehaviorDesignerUtility.InitPlayTexture();
        return BehaviorDesignerUtility.playTexture;
      }
    }

    public static Texture2D PauseTexture
    {
      get
      {
        if (Object.op_Equality((Object) BehaviorDesignerUtility.pauseTexture, (Object) null))
          BehaviorDesignerUtility.InitPauseTexture();
        return BehaviorDesignerUtility.pauseTexture;
      }
    }

    public static Texture2D StepTexture
    {
      get
      {
        if (Object.op_Equality((Object) BehaviorDesignerUtility.stepTexture, (Object) null))
          BehaviorDesignerUtility.InitStepTexture();
        return BehaviorDesignerUtility.stepTexture;
      }
    }

    public static Texture2D ScreenshotBackgroundTexture
    {
      get
      {
        if (Object.op_Equality((Object) BehaviorDesignerUtility.screenshotBackgroundTexture, (Object) null))
          BehaviorDesignerUtility.InitScreenshotBackgroundTexture();
        return BehaviorDesignerUtility.screenshotBackgroundTexture;
      }
    }

    public static string SplitCamelCase(string s)
    {
      if (s.Equals(string.Empty))
        return s;
      if (BehaviorDesignerUtility.camelCaseSplit.ContainsKey(s))
        return BehaviorDesignerUtility.camelCaseSplit[s];
      string key = s;
      s = s.Replace("_uScript", "uScript");
      s = s.Replace("_PlayMaker", "PlayMaker");
      if (s.Length > 2 && s.Substring(0, 2).CompareTo("m_") == 0)
        s = s.Substring(2);
      else if (s.Length > 1 && s[0].CompareTo('_') == 0)
        s = s.Substring(1);
      s = BehaviorDesignerUtility.camelCaseRegex.Replace(s, " ");
      s = s.Replace("_", " ");
      s = s.Replace("u Script", " uScript");
      s = s.Replace("Play Maker", "PlayMaker");
      s = (((int) char.ToUpper(s[0])).ToString() + s.Substring(1)).Trim();
      BehaviorDesignerUtility.camelCaseSplit.Add(key, s);
      return s;
    }

    public static bool HasAttribute(FieldInfo field, Type attributeType)
    {
      Dictionary<FieldInfo, bool> dictionary = (Dictionary<FieldInfo, bool>) null;
      if (BehaviorDesignerUtility.attributeFieldCache.ContainsKey(attributeType))
        dictionary = BehaviorDesignerUtility.attributeFieldCache[attributeType];
      if (dictionary == null)
        dictionary = new Dictionary<FieldInfo, bool>();
      if (dictionary.ContainsKey(field))
        return dictionary[field];
      bool flag = field.GetCustomAttributes(attributeType, false).Length > 0;
      dictionary.Add(field, flag);
      if (!BehaviorDesignerUtility.attributeFieldCache.ContainsKey(attributeType))
        BehaviorDesignerUtility.attributeFieldCache.Add(attributeType, dictionary);
      return flag;
    }

    public static List<Task> GetAllTasks(BehaviorSource behaviorSource)
    {
      List<Task> taskList = new List<Task>();
      if (behaviorSource.get_RootTask() != null)
        BehaviorDesignerUtility.GetAllTasks(behaviorSource.get_RootTask(), ref taskList);
      if (behaviorSource.get_DetachedTasks() != null)
      {
        for (int index = 0; index < behaviorSource.get_DetachedTasks().Count; ++index)
          BehaviorDesignerUtility.GetAllTasks(behaviorSource.get_DetachedTasks()[index], ref taskList);
      }
      return taskList;
    }

    private static void GetAllTasks(Task task, ref List<Task> taskList)
    {
      taskList.Add(task);
      if (!(task is ParentTask parentTask) || parentTask.get_Children() == null)
        return;
      for (int index = 0; index < parentTask.get_Children().Count; ++index)
        BehaviorDesignerUtility.GetAllTasks(parentTask.get_Children()[index], ref taskList);
    }

    public static bool AnyNullTasks(BehaviorSource behaviorSource)
    {
      if (behaviorSource.get_RootTask() != null && BehaviorDesignerUtility.AnyNullTasks(behaviorSource.get_RootTask()))
        return true;
      if (behaviorSource.get_DetachedTasks() != null)
      {
        for (int index = 0; index < behaviorSource.get_DetachedTasks().Count; ++index)
        {
          if (BehaviorDesignerUtility.AnyNullTasks(behaviorSource.get_DetachedTasks()[index]))
            return true;
        }
      }
      return false;
    }

    private static bool AnyNullTasks(Task task)
    {
      if (task == null)
        return true;
      if (task is ParentTask parentTask && parentTask.get_Children() != null)
      {
        for (int index = 0; index < parentTask.get_Children().Count; ++index)
        {
          if (BehaviorDesignerUtility.AnyNullTasks(parentTask.get_Children()[index]))
            return true;
        }
      }
      return false;
    }

    public static bool HasRootTask(string serialization)
    {
      if (string.IsNullOrEmpty(serialization))
        return false;
      Dictionary<string, object> dictionary = MiniJSON.Deserialize(serialization) as Dictionary<string, object>;
      return dictionary != null && dictionary.ContainsKey("RootTask");
    }

    public static string GetEditorBaseDirectory(Object obj = null)
    {
      return Path.GetDirectoryName(Uri.UnescapeDataString(new UriBuilder(Assembly.GetExecutingAssembly().CodeBase).Path).Substring(Application.get_dataPath().Length - 6));
    }

    public static Texture2D LoadTexture(string imageName, bool useSkinColor = true, Object obj = null)
    {
      if (BehaviorDesignerUtility.textureCache.ContainsKey(imageName))
        return BehaviorDesignerUtility.textureCache[imageName];
      Texture2D texture2D = (Texture2D) null;
      Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(string.Format("{0}{1}", !useSkinColor ? (object) string.Empty : (!EditorGUIUtility.get_isProSkin() ? (object) "Light" : (object) "Dark"), (object) imageName)) ?? Assembly.GetExecutingAssembly().GetManifestResourceStream(string.Format("BehaviorDesignerEditor.Resources.{0}{1}", !useSkinColor ? (object) string.Empty : (!EditorGUIUtility.get_isProSkin() ? (object) "Light" : (object) "Dark"), (object) imageName));
      if (stream != null)
      {
        texture2D = new Texture2D(0, 0, (TextureFormat) 4, false, true);
        ImageConversion.LoadImage(texture2D, BehaviorDesignerUtility.ReadToEnd(stream));
        stream.Close();
      }
      ((Object) texture2D).set_hideFlags((HideFlags) 61);
      BehaviorDesignerUtility.textureCache.Add(imageName, texture2D);
      return texture2D;
    }

    private static Texture2D LoadTaskTexture(
      string imageName,
      bool useSkinColor = true,
      ScriptableObject obj = null)
    {
      if (BehaviorDesignerUtility.textureCache.ContainsKey(imageName))
        return BehaviorDesignerUtility.textureCache[imageName];
      Texture2D texture2D = (Texture2D) null;
      Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(string.Format("{0}{1}", !useSkinColor ? (object) string.Empty : (!EditorGUIUtility.get_isProSkin() ? (object) "Light" : (object) "Dark"), (object) imageName)) ?? Assembly.GetExecutingAssembly().GetManifestResourceStream(string.Format("BehaviorDesignerEditor.Resources.{0}{1}", !useSkinColor ? (object) string.Empty : (!EditorGUIUtility.get_isProSkin() ? (object) "Light" : (object) "Dark"), (object) imageName));
      if (stream != null)
      {
        texture2D = new Texture2D(0, 0, (TextureFormat) 4, false, true);
        ImageConversion.LoadImage(texture2D, BehaviorDesignerUtility.ReadToEnd(stream));
        stream.Close();
      }
      if (Object.op_Equality((Object) texture2D, (Object) null))
        Debug.Log((object) string.Format("{0}/Images/Task Backgrounds/{1}{2}", (object) BehaviorDesignerUtility.GetEditorBaseDirectory((Object) obj), !useSkinColor ? (object) string.Empty : (!EditorGUIUtility.get_isProSkin() ? (object) "Light" : (object) "Dark"), (object) imageName));
      ((Object) texture2D).set_hideFlags((HideFlags) 61);
      BehaviorDesignerUtility.textureCache.Add(imageName, texture2D);
      return texture2D;
    }

    public static Texture2D LoadIcon(string iconName, ScriptableObject obj = null)
    {
      if (BehaviorDesignerUtility.iconCache.ContainsKey(iconName))
        return BehaviorDesignerUtility.iconCache[iconName];
      Texture2D texture2D = (Texture2D) null;
      Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(iconName.Replace("{SkinColor}", !EditorGUIUtility.get_isProSkin() ? "Light" : "Dark")) ?? Assembly.GetExecutingAssembly().GetManifestResourceStream(string.Format("BehaviorDesignerEditor.Resources.{0}", (object) iconName.Replace("{SkinColor}", !EditorGUIUtility.get_isProSkin() ? "Light" : "Dark")));
      if (stream != null)
      {
        texture2D = new Texture2D(0, 0, (TextureFormat) 4, false, true);
        ImageConversion.LoadImage(texture2D, BehaviorDesignerUtility.ReadToEnd(stream));
        stream.Close();
      }
      if (Object.op_Equality((Object) texture2D, (Object) null))
        texture2D = AssetDatabase.LoadAssetAtPath(iconName.Replace("{SkinColor}", !EditorGUIUtility.get_isProSkin() ? "Light" : "Dark"), typeof (Texture2D)) as Texture2D;
      if (Object.op_Inequality((Object) texture2D, (Object) null))
        ((Object) texture2D).set_hideFlags((HideFlags) 61);
      BehaviorDesignerUtility.iconCache.Add(iconName, texture2D);
      return texture2D;
    }

    private static byte[] ReadToEnd(Stream stream)
    {
      byte[] buffer = new byte[16384];
      using (MemoryStream memoryStream = new MemoryStream())
      {
        int count;
        while ((count = stream.Read(buffer, 0, buffer.Length)) > 0)
          memoryStream.Write(buffer, 0, count);
        return memoryStream.ToArray();
      }
    }

    public static void DrawContentSeperator(int yOffset)
    {
      BehaviorDesignerUtility.DrawContentSeperator(yOffset, 0);
    }

    public static void DrawContentSeperator(int yOffset, int widthExtension)
    {
      Rect lastRect = GUILayoutUtility.GetLastRect();
      ((Rect) ref lastRect).set_x(-5f);
      ref Rect local1 = ref lastRect;
      ((Rect) ref local1).set_y(((Rect) ref local1).get_y() + (((Rect) ref lastRect).get_height() + (float) yOffset));
      ((Rect) ref lastRect).set_height(2f);
      ref Rect local2 = ref lastRect;
      ((Rect) ref local2).set_width(((Rect) ref local2).get_width() + (float) (10 + widthExtension));
      GUI.DrawTexture(lastRect, (Texture) BehaviorDesignerUtility.ContentSeparatorTexture);
    }

    public static float RoundToNearest(float num, float baseNum)
    {
      return (float) (int) Math.Round((double) num / (double) baseNum, MidpointRounding.AwayFromZero) * baseNum;
    }

    private static void InitGraphStatusGUIStyle()
    {
      BehaviorDesignerUtility.graphStatusGUIStyle = new GUIStyle(GUI.get_skin().get_label());
      BehaviorDesignerUtility.graphStatusGUIStyle.set_alignment((TextAnchor) 3);
      BehaviorDesignerUtility.graphStatusGUIStyle.set_fontSize(20);
      BehaviorDesignerUtility.graphStatusGUIStyle.set_fontStyle((FontStyle) 1);
      if (EditorGUIUtility.get_isProSkin())
        BehaviorDesignerUtility.graphStatusGUIStyle.get_normal().set_textColor(new Color(0.7058f, 0.7058f, 0.7058f));
      else
        BehaviorDesignerUtility.graphStatusGUIStyle.get_normal().set_textColor(new Color(0.8058f, 0.8058f, 0.8058f));
    }

    private static void InitTaskFoldoutGUIStyle()
    {
      BehaviorDesignerUtility.taskFoldoutGUIStyle = new GUIStyle(EditorStyles.get_foldout());
      BehaviorDesignerUtility.taskFoldoutGUIStyle.set_alignment((TextAnchor) 3);
      BehaviorDesignerUtility.taskFoldoutGUIStyle.set_fontSize(13);
      BehaviorDesignerUtility.taskFoldoutGUIStyle.set_fontStyle((FontStyle) 1);
    }

    private static void InitTaskTitleGUIStyle()
    {
      BehaviorDesignerUtility.taskTitleGUIStyle = new GUIStyle(GUI.get_skin().get_label());
      BehaviorDesignerUtility.taskTitleGUIStyle.set_alignment((TextAnchor) 1);
      BehaviorDesignerUtility.taskTitleGUIStyle.set_fontSize(12);
      BehaviorDesignerUtility.taskTitleGUIStyle.set_fontStyle((FontStyle) 0);
    }

    private static void InitTaskGUIStyle(int colorIndex)
    {
      BehaviorDesignerUtility.taskGUIStyle[colorIndex] = BehaviorDesignerUtility.InitTaskGUIStyle(BehaviorDesignerUtility.LoadTaskTexture("Task" + BehaviorDesignerUtility.ColorIndexToColorString(colorIndex) + ".png", true, (ScriptableObject) null), new RectOffset(5, 3, 3, 5));
    }

    private static void InitTaskCompactGUIStyle(int colorIndex)
    {
      BehaviorDesignerUtility.taskCompactGUIStyle[colorIndex] = BehaviorDesignerUtility.InitTaskGUIStyle(BehaviorDesignerUtility.LoadTaskTexture("TaskCompact" + BehaviorDesignerUtility.ColorIndexToColorString(colorIndex) + ".png", true, (ScriptableObject) null), new RectOffset(5, 4, 4, 5));
    }

    private static void InitTaskSelectedGUIStyle(int colorIndex)
    {
      BehaviorDesignerUtility.taskSelectedGUIStyle[colorIndex] = BehaviorDesignerUtility.InitTaskGUIStyle(BehaviorDesignerUtility.LoadTaskTexture("TaskSelected" + BehaviorDesignerUtility.ColorIndexToColorString(colorIndex) + ".png", true, (ScriptableObject) null), new RectOffset(5, 4, 4, 4));
    }

    private static void InitTaskSelectedCompactGUIStyle(int colorIndex)
    {
      BehaviorDesignerUtility.taskSelectedCompactGUIStyle[colorIndex] = BehaviorDesignerUtility.InitTaskGUIStyle(BehaviorDesignerUtility.LoadTaskTexture("TaskSelectedCompact" + BehaviorDesignerUtility.ColorIndexToColorString(colorIndex) + ".png", true, (ScriptableObject) null), new RectOffset(5, 4, 4, 4));
    }

    private static string ColorIndexToColorString(int index)
    {
      switch (index)
      {
        case 0:
          return string.Empty;
        case 1:
          return "Red";
        case 2:
          return "Pink";
        case 3:
          return "Brown";
        case 4:
          return "RedOrange";
        case 5:
          return "Turquoise";
        case 6:
          return "Cyan";
        case 7:
          return "Blue";
        case 8:
          return "Purple";
        default:
          return string.Empty;
      }
    }

    private static void InitTaskRunningGUIStyle()
    {
      BehaviorDesignerUtility.taskRunningGUIStyle = BehaviorDesignerUtility.InitTaskGUIStyle(BehaviorDesignerUtility.LoadTaskTexture("TaskRunning.png", true, (ScriptableObject) null), new RectOffset(5, 3, 3, 5));
    }

    private static void InitTaskRunningCompactGUIStyle()
    {
      BehaviorDesignerUtility.taskRunningCompactGUIStyle = BehaviorDesignerUtility.InitTaskGUIStyle(BehaviorDesignerUtility.LoadTaskTexture("TaskRunningCompact.png", true, (ScriptableObject) null), new RectOffset(5, 4, 4, 5));
    }

    private static void InitTaskRunningSelectedGUIStyle()
    {
      BehaviorDesignerUtility.taskRunningSelectedGUIStyle = BehaviorDesignerUtility.InitTaskGUIStyle(BehaviorDesignerUtility.LoadTaskTexture("TaskRunningSelected.png", true, (ScriptableObject) null), new RectOffset(5, 4, 4, 4));
    }

    private static void InitTaskRunningSelectedCompactGUIStyle()
    {
      BehaviorDesignerUtility.taskRunningSelectedCompactGUIStyle = BehaviorDesignerUtility.InitTaskGUIStyle(BehaviorDesignerUtility.LoadTaskTexture("TaskRunningSelectedCompact.png", true, (ScriptableObject) null), new RectOffset(5, 4, 4, 4));
    }

    private static void InitTaskIdentifyGUIStyle()
    {
      BehaviorDesignerUtility.taskIdentifyGUIStyle = BehaviorDesignerUtility.InitTaskGUIStyle(BehaviorDesignerUtility.LoadTaskTexture("TaskIdentify.png", true, (ScriptableObject) null), new RectOffset(5, 3, 3, 5));
    }

    private static void InitTaskIdentifyCompactGUIStyle()
    {
      BehaviorDesignerUtility.taskIdentifyCompactGUIStyle = BehaviorDesignerUtility.InitTaskGUIStyle(BehaviorDesignerUtility.LoadTaskTexture("TaskIdentifyCompact.png", true, (ScriptableObject) null), new RectOffset(5, 4, 4, 5));
    }

    private static void InitTaskIdentifySelectedGUIStyle()
    {
      BehaviorDesignerUtility.taskIdentifySelectedGUIStyle = BehaviorDesignerUtility.InitTaskGUIStyle(BehaviorDesignerUtility.LoadTaskTexture("TaskIdentifySelected.png", true, (ScriptableObject) null), new RectOffset(5, 4, 4, 4));
    }

    private static void InitTaskIdentifySelectedCompactGUIStyle()
    {
      BehaviorDesignerUtility.taskIdentifySelectedCompactGUIStyle = BehaviorDesignerUtility.InitTaskGUIStyle(BehaviorDesignerUtility.LoadTaskTexture("TaskIdentifySelectedCompact.png", true, (ScriptableObject) null), new RectOffset(5, 4, 4, 4));
    }

    private static void InitTaskHighlightGUIStyle()
    {
      BehaviorDesignerUtility.taskHighlightGUIStyle = BehaviorDesignerUtility.InitTaskGUIStyle(BehaviorDesignerUtility.LoadTaskTexture("TaskHighlight.png", true, (ScriptableObject) null), new RectOffset(5, 4, 4, 4));
    }

    private static void InitTaskHighlightCompactGUIStyle()
    {
      BehaviorDesignerUtility.taskHighlightCompactGUIStyle = BehaviorDesignerUtility.InitTaskGUIStyle(BehaviorDesignerUtility.LoadTaskTexture("TaskHighlightCompact.png", true, (ScriptableObject) null), new RectOffset(5, 4, 4, 4));
    }

    private static GUIStyle InitTaskGUIStyle(Texture2D texture, RectOffset overflow)
    {
      GUIStyle guiStyle = new GUIStyle(GUI.get_skin().get_box());
      guiStyle.set_border(new RectOffset(10, 10, 10, 10));
      guiStyle.set_overflow(overflow);
      guiStyle.get_normal().set_background(texture);
      guiStyle.get_active().set_background(texture);
      guiStyle.get_hover().set_background(texture);
      guiStyle.get_focused().set_background(texture);
      guiStyle.get_normal().set_textColor(Color.get_white());
      guiStyle.get_active().set_textColor(Color.get_white());
      guiStyle.get_hover().set_textColor(Color.get_white());
      guiStyle.get_focused().set_textColor(Color.get_white());
      guiStyle.set_stretchHeight(true);
      guiStyle.set_stretchWidth(true);
      return guiStyle;
    }

    private static void InitTaskCommentGUIStyle()
    {
      BehaviorDesignerUtility.taskCommentGUIStyle = new GUIStyle(GUI.get_skin().get_label());
      BehaviorDesignerUtility.taskCommentGUIStyle.set_alignment((TextAnchor) 1);
      BehaviorDesignerUtility.taskCommentGUIStyle.set_fontSize(12);
      BehaviorDesignerUtility.taskCommentGUIStyle.set_fontStyle((FontStyle) 0);
      BehaviorDesignerUtility.taskCommentGUIStyle.set_wordWrap(true);
    }

    private static void InitTaskCommentLeftAlignGUIStyle()
    {
      BehaviorDesignerUtility.taskCommentLeftAlignGUIStyle = new GUIStyle(GUI.get_skin().get_label());
      BehaviorDesignerUtility.taskCommentLeftAlignGUIStyle.set_alignment((TextAnchor) 0);
      BehaviorDesignerUtility.taskCommentLeftAlignGUIStyle.set_fontSize(12);
      BehaviorDesignerUtility.taskCommentLeftAlignGUIStyle.set_fontStyle((FontStyle) 0);
      BehaviorDesignerUtility.taskCommentLeftAlignGUIStyle.set_wordWrap(false);
    }

    private static void InitTaskCommentRightAlignGUIStyle()
    {
      BehaviorDesignerUtility.taskCommentRightAlignGUIStyle = new GUIStyle(GUI.get_skin().get_label());
      BehaviorDesignerUtility.taskCommentRightAlignGUIStyle.set_alignment((TextAnchor) 2);
      BehaviorDesignerUtility.taskCommentRightAlignGUIStyle.set_fontSize(12);
      BehaviorDesignerUtility.taskCommentRightAlignGUIStyle.set_fontStyle((FontStyle) 0);
      BehaviorDesignerUtility.taskCommentRightAlignGUIStyle.set_wordWrap(false);
    }

    private static void InitTaskDescriptionGUIStyle()
    {
      Texture2D texture2D = new Texture2D(1, 1, (TextureFormat) 4, false, true);
      if (EditorGUIUtility.get_isProSkin())
        texture2D.SetPixel(1, 1, new Color(0.1647f, 0.1647f, 0.1647f));
      else
        texture2D.SetPixel(1, 1, new Color(0.75f, 0.75f, 0.75f));
      ((Object) texture2D).set_hideFlags((HideFlags) 61);
      texture2D.Apply();
      BehaviorDesignerUtility.taskDescriptionGUIStyle = new GUIStyle(GUI.get_skin().get_box());
      BehaviorDesignerUtility.taskDescriptionGUIStyle.get_normal().set_background(texture2D);
      BehaviorDesignerUtility.taskDescriptionGUIStyle.get_active().set_background(texture2D);
      BehaviorDesignerUtility.taskDescriptionGUIStyle.get_hover().set_background(texture2D);
      BehaviorDesignerUtility.taskDescriptionGUIStyle.get_focused().set_background(texture2D);
    }

    private static void InitGraphBackgroundGUIStyle()
    {
      Texture2D texture2D = new Texture2D(1, 1, (TextureFormat) 4, false, true);
      if (EditorGUIUtility.get_isProSkin())
        texture2D.SetPixel(1, 1, new Color(0.1647f, 0.1647f, 0.1647f));
      else
        texture2D.SetPixel(1, 1, new Color(0.3647f, 0.3647f, 0.3647f));
      ((Object) texture2D).set_hideFlags((HideFlags) 61);
      texture2D.Apply();
      BehaviorDesignerUtility.graphBackgroundGUIStyle = new GUIStyle(GUI.get_skin().get_box());
      BehaviorDesignerUtility.graphBackgroundGUIStyle.get_normal().set_background(texture2D);
      BehaviorDesignerUtility.graphBackgroundGUIStyle.get_active().set_background(texture2D);
      BehaviorDesignerUtility.graphBackgroundGUIStyle.get_hover().set_background(texture2D);
      BehaviorDesignerUtility.graphBackgroundGUIStyle.get_focused().set_background(texture2D);
      BehaviorDesignerUtility.graphBackgroundGUIStyle.get_normal().set_textColor(Color.get_white());
      BehaviorDesignerUtility.graphBackgroundGUIStyle.get_active().set_textColor(Color.get_white());
      BehaviorDesignerUtility.graphBackgroundGUIStyle.get_hover().set_textColor(Color.get_white());
      BehaviorDesignerUtility.graphBackgroundGUIStyle.get_focused().set_textColor(Color.get_white());
    }

    private static void InitSelectionGUIStyle()
    {
      Texture2D texture2D = new Texture2D(1, 1, (TextureFormat) 4, false, true);
      Color color = !EditorGUIUtility.get_isProSkin() ? new Color(0.243f, 0.5686f, 0.839f, 0.5f) : new Color(0.188f, 0.4588f, 0.6862f, 0.5f);
      texture2D.SetPixel(1, 1, color);
      ((Object) texture2D).set_hideFlags((HideFlags) 61);
      texture2D.Apply();
      BehaviorDesignerUtility.selectionGUIStyle = new GUIStyle(GUI.get_skin().get_box());
      BehaviorDesignerUtility.selectionGUIStyle.get_normal().set_background(texture2D);
      BehaviorDesignerUtility.selectionGUIStyle.get_active().set_background(texture2D);
      BehaviorDesignerUtility.selectionGUIStyle.get_hover().set_background(texture2D);
      BehaviorDesignerUtility.selectionGUIStyle.get_focused().set_background(texture2D);
      BehaviorDesignerUtility.selectionGUIStyle.get_normal().set_textColor(Color.get_white());
      BehaviorDesignerUtility.selectionGUIStyle.get_active().set_textColor(Color.get_white());
      BehaviorDesignerUtility.selectionGUIStyle.get_hover().set_textColor(Color.get_white());
      BehaviorDesignerUtility.selectionGUIStyle.get_focused().set_textColor(Color.get_white());
    }

    private static void InitSharedVariableToolbarPopup()
    {
      BehaviorDesignerUtility.sharedVariableToolbarPopup = new GUIStyle(EditorStyles.get_toolbarPopup());
      BehaviorDesignerUtility.sharedVariableToolbarPopup.set_margin(new RectOffset(4, 4, 0, 0));
    }

    private static void InitLabelWrapGUIStyle()
    {
      BehaviorDesignerUtility.labelWrapGUIStyle = new GUIStyle(GUI.get_skin().get_label());
      BehaviorDesignerUtility.labelWrapGUIStyle.set_wordWrap(true);
      BehaviorDesignerUtility.labelWrapGUIStyle.set_alignment((TextAnchor) 4);
    }

    private static void InitToolbarButtonLeftAlignGUIStyle()
    {
      BehaviorDesignerUtility.tolbarButtonLeftAlignGUIStyle = new GUIStyle(EditorStyles.get_toolbarButton());
      BehaviorDesignerUtility.tolbarButtonLeftAlignGUIStyle.set_alignment((TextAnchor) 3);
    }

    private static void InitToolbarLabelGUIStyle()
    {
      BehaviorDesignerUtility.toolbarLabelGUIStyle = new GUIStyle(EditorStyles.get_label());
      BehaviorDesignerUtility.toolbarLabelGUIStyle.get_normal().set_textColor(!EditorGUIUtility.get_isProSkin() ? new Color(0.0f, 0.5f, 0.0f) : new Color(0.0f, 0.7f, 0.0f));
    }

    private static void InitTaskInspectorCommentGUIStyle()
    {
      BehaviorDesignerUtility.taskInspectorCommentGUIStyle = new GUIStyle(GUI.get_skin().get_textArea());
      BehaviorDesignerUtility.taskInspectorCommentGUIStyle.set_wordWrap(true);
    }

    private static void InitTaskInspectorGUIStyle()
    {
      BehaviorDesignerUtility.taskInspectorGUIStyle = new GUIStyle(GUI.get_skin().get_label());
      BehaviorDesignerUtility.taskInspectorGUIStyle.set_alignment((TextAnchor) 3);
      BehaviorDesignerUtility.taskInspectorGUIStyle.set_fontSize(11);
      BehaviorDesignerUtility.taskInspectorGUIStyle.set_fontStyle((FontStyle) 0);
    }

    private static void InitToolbarButtonSelectionGUIStyle()
    {
      BehaviorDesignerUtility.toolbarButtonSelectionGUIStyle = new GUIStyle(EditorStyles.get_toolbarButton());
      BehaviorDesignerUtility.toolbarButtonSelectionGUIStyle.get_normal().set_background(BehaviorDesignerUtility.toolbarButtonSelectionGUIStyle.get_active().get_background());
    }

    private static void InitPreferencesPaneGUIStyle()
    {
      BehaviorDesignerUtility.preferencesPaneGUIStyle = new GUIStyle(GUI.get_skin().get_box());
      BehaviorDesignerUtility.preferencesPaneGUIStyle.get_normal().set_background(EditorStyles.get_toolbarButton().get_normal().get_background());
    }

    private static void InitPropertyBoxGUIStyle()
    {
      BehaviorDesignerUtility.propertyBoxGUIStyle = new GUIStyle();
      BehaviorDesignerUtility.propertyBoxGUIStyle.set_padding(new RectOffset(2, 2, 0, 0));
    }

    private static void InitPlainButtonGUIStyle()
    {
      BehaviorDesignerUtility.plainButtonGUIStyle = new GUIStyle(GUI.get_skin().get_button());
      BehaviorDesignerUtility.plainButtonGUIStyle.set_border(new RectOffset(0, 0, 0, 0));
      BehaviorDesignerUtility.plainButtonGUIStyle.set_margin(new RectOffset(0, 0, 2, 2));
      BehaviorDesignerUtility.plainButtonGUIStyle.set_padding(new RectOffset(0, 0, 1, 0));
      BehaviorDesignerUtility.plainButtonGUIStyle.get_normal().set_background((Texture2D) null);
      BehaviorDesignerUtility.plainButtonGUIStyle.get_active().set_background((Texture2D) null);
      BehaviorDesignerUtility.plainButtonGUIStyle.get_hover().set_background((Texture2D) null);
      BehaviorDesignerUtility.plainButtonGUIStyle.get_focused().set_background((Texture2D) null);
      BehaviorDesignerUtility.plainButtonGUIStyle.get_normal().set_textColor(Color.get_white());
      BehaviorDesignerUtility.plainButtonGUIStyle.get_active().set_textColor(Color.get_white());
      BehaviorDesignerUtility.plainButtonGUIStyle.get_hover().set_textColor(Color.get_white());
      BehaviorDesignerUtility.plainButtonGUIStyle.get_focused().set_textColor(Color.get_white());
    }

    private static void InitTransparentButtonGUIStyle()
    {
      BehaviorDesignerUtility.transparentButtonGUIStyle = new GUIStyle(GUI.get_skin().get_button());
      BehaviorDesignerUtility.transparentButtonGUIStyle.set_border(new RectOffset(0, 0, 0, 0));
      BehaviorDesignerUtility.transparentButtonGUIStyle.set_margin(new RectOffset(4, 4, 2, 2));
      BehaviorDesignerUtility.transparentButtonGUIStyle.set_padding(new RectOffset(2, 2, 1, 0));
      BehaviorDesignerUtility.transparentButtonGUIStyle.get_normal().set_background((Texture2D) null);
      BehaviorDesignerUtility.transparentButtonGUIStyle.get_active().set_background((Texture2D) null);
      BehaviorDesignerUtility.transparentButtonGUIStyle.get_hover().set_background((Texture2D) null);
      BehaviorDesignerUtility.transparentButtonGUIStyle.get_focused().set_background((Texture2D) null);
      BehaviorDesignerUtility.transparentButtonGUIStyle.get_normal().set_textColor(Color.get_white());
      BehaviorDesignerUtility.transparentButtonGUIStyle.get_active().set_textColor(Color.get_white());
      BehaviorDesignerUtility.transparentButtonGUIStyle.get_hover().set_textColor(Color.get_white());
      BehaviorDesignerUtility.transparentButtonGUIStyle.get_focused().set_textColor(Color.get_white());
    }

    private static void InitTransparentButtonOffsetGUIStyle()
    {
      BehaviorDesignerUtility.transparentButtonOffsetGUIStyle = new GUIStyle(GUI.get_skin().get_button());
      BehaviorDesignerUtility.transparentButtonOffsetGUIStyle.set_border(new RectOffset(0, 0, 0, 0));
      BehaviorDesignerUtility.transparentButtonOffsetGUIStyle.set_margin(new RectOffset(4, 4, 4, 2));
      BehaviorDesignerUtility.transparentButtonOffsetGUIStyle.set_padding(new RectOffset(2, 2, 1, 0));
      BehaviorDesignerUtility.transparentButtonOffsetGUIStyle.get_normal().set_background((Texture2D) null);
      BehaviorDesignerUtility.transparentButtonOffsetGUIStyle.get_active().set_background((Texture2D) null);
      BehaviorDesignerUtility.transparentButtonOffsetGUIStyle.get_hover().set_background((Texture2D) null);
      BehaviorDesignerUtility.transparentButtonOffsetGUIStyle.get_focused().set_background((Texture2D) null);
      BehaviorDesignerUtility.transparentButtonOffsetGUIStyle.get_normal().set_textColor(Color.get_white());
      BehaviorDesignerUtility.transparentButtonOffsetGUIStyle.get_active().set_textColor(Color.get_white());
      BehaviorDesignerUtility.transparentButtonOffsetGUIStyle.get_hover().set_textColor(Color.get_white());
      BehaviorDesignerUtility.transparentButtonOffsetGUIStyle.get_focused().set_textColor(Color.get_white());
    }

    private static void InitButtonGUIStyle()
    {
      BehaviorDesignerUtility.buttonGUIStyle = new GUIStyle(GUI.get_skin().get_button());
      BehaviorDesignerUtility.buttonGUIStyle.set_margin(new RectOffset(0, 0, 2, 2));
      BehaviorDesignerUtility.buttonGUIStyle.set_padding(new RectOffset(0, 0, 1, 1));
    }

    private static void InitPlainTextureGUIStyle()
    {
      BehaviorDesignerUtility.plainTextureGUIStyle = new GUIStyle();
      BehaviorDesignerUtility.plainTextureGUIStyle.set_border(new RectOffset(0, 0, 0, 0));
      BehaviorDesignerUtility.plainTextureGUIStyle.set_margin(new RectOffset(0, 0, 0, 0));
      BehaviorDesignerUtility.plainTextureGUIStyle.set_padding(new RectOffset(0, 0, 0, 0));
      BehaviorDesignerUtility.plainTextureGUIStyle.get_normal().set_background((Texture2D) null);
      BehaviorDesignerUtility.plainTextureGUIStyle.get_active().set_background((Texture2D) null);
      BehaviorDesignerUtility.plainTextureGUIStyle.get_hover().set_background((Texture2D) null);
      BehaviorDesignerUtility.plainTextureGUIStyle.get_focused().set_background((Texture2D) null);
    }

    private static void InitArrowSeparatorGUIStyle()
    {
      BehaviorDesignerUtility.arrowSeparatorGUIStyle = new GUIStyle();
      BehaviorDesignerUtility.arrowSeparatorGUIStyle.set_border(new RectOffset(0, 0, 0, 0));
      BehaviorDesignerUtility.arrowSeparatorGUIStyle.set_margin(new RectOffset(0, 0, 3, 0));
      BehaviorDesignerUtility.arrowSeparatorGUIStyle.set_padding(new RectOffset(0, 0, 0, 0));
      Texture2D texture2D = BehaviorDesignerUtility.LoadTexture("ArrowSeparator.png", true, (Object) null);
      BehaviorDesignerUtility.arrowSeparatorGUIStyle.get_normal().set_background(texture2D);
      BehaviorDesignerUtility.arrowSeparatorGUIStyle.get_active().set_background(texture2D);
      BehaviorDesignerUtility.arrowSeparatorGUIStyle.get_hover().set_background(texture2D);
      BehaviorDesignerUtility.arrowSeparatorGUIStyle.get_focused().set_background(texture2D);
    }

    private static void InitSelectedBackgroundGUIStyle()
    {
      Texture2D texture2D = new Texture2D(1, 1, (TextureFormat) 4, false, true);
      Color color = !EditorGUIUtility.get_isProSkin() ? new Color(0.243f, 0.5686f, 0.839f, 0.5f) : new Color(0.188f, 0.4588f, 0.6862f, 0.5f);
      texture2D.SetPixel(1, 1, color);
      ((Object) texture2D).set_hideFlags((HideFlags) 61);
      texture2D.Apply();
      BehaviorDesignerUtility.selectedBackgroundGUIStyle = new GUIStyle();
      BehaviorDesignerUtility.selectedBackgroundGUIStyle.set_border(new RectOffset(0, 0, 0, 0));
      BehaviorDesignerUtility.selectedBackgroundGUIStyle.set_margin(new RectOffset(0, 0, -2, 2));
      BehaviorDesignerUtility.selectedBackgroundGUIStyle.get_normal().set_background(texture2D);
      BehaviorDesignerUtility.selectedBackgroundGUIStyle.get_active().set_background(texture2D);
      BehaviorDesignerUtility.selectedBackgroundGUIStyle.get_hover().set_background(texture2D);
      BehaviorDesignerUtility.selectedBackgroundGUIStyle.get_focused().set_background(texture2D);
    }

    private static void InitErrorListDarkBackground()
    {
      Texture2D texture2D = new Texture2D(1, 1, (TextureFormat) 4, false, true);
      Color color = !EditorGUIUtility.get_isProSkin() ? new Color(0.706f, 0.706f, 0.706f) : new Color(0.2f, 0.2f, 0.2f, 1f);
      texture2D.SetPixel(1, 1, color);
      ((Object) texture2D).set_hideFlags((HideFlags) 61);
      texture2D.Apply();
      BehaviorDesignerUtility.errorListDarkBackground = new GUIStyle();
      BehaviorDesignerUtility.errorListDarkBackground.set_padding(new RectOffset(2, 0, 2, 0));
      BehaviorDesignerUtility.errorListDarkBackground.get_normal().set_background(texture2D);
      BehaviorDesignerUtility.errorListDarkBackground.get_active().set_background(texture2D);
      BehaviorDesignerUtility.errorListDarkBackground.get_hover().set_background(texture2D);
      BehaviorDesignerUtility.errorListDarkBackground.get_focused().set_background(texture2D);
      BehaviorDesignerUtility.errorListDarkBackground.get_normal().set_textColor(!EditorGUIUtility.get_isProSkin() ? new Color(0.206f, 0.206f, 0.206f) : new Color(0.706f, 0.706f, 0.706f));
      BehaviorDesignerUtility.errorListDarkBackground.set_alignment((TextAnchor) 0);
      BehaviorDesignerUtility.errorListDarkBackground.set_wordWrap(true);
    }

    private static void InitErrorListLightBackground()
    {
      BehaviorDesignerUtility.errorListLightBackground = new GUIStyle();
      BehaviorDesignerUtility.errorListLightBackground.set_padding(new RectOffset(2, 0, 2, 0));
      BehaviorDesignerUtility.errorListLightBackground.get_normal().set_textColor(!EditorGUIUtility.get_isProSkin() ? new Color(0.106f, 0.106f, 0.106f) : new Color(0.706f, 0.706f, 0.706f));
      BehaviorDesignerUtility.errorListLightBackground.set_alignment((TextAnchor) 0);
      BehaviorDesignerUtility.errorListLightBackground.set_wordWrap(true);
    }

    private static void InitWelcomeScreenIntroGUIStyle()
    {
      BehaviorDesignerUtility.welcomeScreenIntroGUIStyle = new GUIStyle(GUI.get_skin().get_label());
      BehaviorDesignerUtility.welcomeScreenIntroGUIStyle.set_fontSize(16);
      BehaviorDesignerUtility.welcomeScreenIntroGUIStyle.set_fontStyle((FontStyle) 1);
      BehaviorDesignerUtility.welcomeScreenIntroGUIStyle.get_normal().set_textColor(new Color(0.706f, 0.706f, 0.706f));
    }

    private static void InitWelcomeScreenTextHeaderGUIStyle()
    {
      BehaviorDesignerUtility.welcomeScreenTextHeaderGUIStyle = new GUIStyle(GUI.get_skin().get_label());
      BehaviorDesignerUtility.welcomeScreenTextHeaderGUIStyle.set_alignment((TextAnchor) 3);
      BehaviorDesignerUtility.welcomeScreenTextHeaderGUIStyle.set_fontSize(14);
      BehaviorDesignerUtility.welcomeScreenTextHeaderGUIStyle.set_fontStyle((FontStyle) 1);
    }

    private static void InitWelcomeScreenTextDescriptionGUIStyle()
    {
      BehaviorDesignerUtility.welcomeScreenTextDescriptionGUIStyle = new GUIStyle(GUI.get_skin().get_label());
      BehaviorDesignerUtility.welcomeScreenTextDescriptionGUIStyle.set_wordWrap(true);
    }

    private static void InitTaskBorderTexture(int colorIndex)
    {
      BehaviorDesignerUtility.taskBorderTexture[colorIndex] = BehaviorDesignerUtility.LoadTaskTexture("TaskBorder" + BehaviorDesignerUtility.ColorIndexToColorString(colorIndex) + ".png", true, (ScriptableObject) null);
    }

    private static void InitTaskBorderRunningTexture()
    {
      BehaviorDesignerUtility.taskBorderRunningTexture = BehaviorDesignerUtility.LoadTaskTexture("TaskBorderRunning.png", true, (ScriptableObject) null);
    }

    private static void InitTaskBorderIdentifyTexture()
    {
      BehaviorDesignerUtility.taskBorderIdentifyTexture = BehaviorDesignerUtility.LoadTaskTexture("TaskBorderIdentify.png", true, (ScriptableObject) null);
    }

    private static void InitTaskConnectionTopTexture(int colorIndex)
    {
      BehaviorDesignerUtility.taskConnectionTopTexture[colorIndex] = BehaviorDesignerUtility.LoadTaskTexture("TaskConnectionTop" + BehaviorDesignerUtility.ColorIndexToColorString(colorIndex) + ".png", true, (ScriptableObject) null);
    }

    private static void InitTaskConnectionBottomTexture(int colorIndex)
    {
      BehaviorDesignerUtility.taskConnectionBottomTexture[colorIndex] = BehaviorDesignerUtility.LoadTaskTexture("TaskConnectionBottom" + BehaviorDesignerUtility.ColorIndexToColorString(colorIndex) + ".png", true, (ScriptableObject) null);
    }

    private static void InitTaskConnectionRunningTopTexture()
    {
      BehaviorDesignerUtility.taskConnectionRunningTopTexture = BehaviorDesignerUtility.LoadTaskTexture("TaskConnectionRunningTop.png", true, (ScriptableObject) null);
    }

    private static void InitTaskConnectionRunningBottomTexture()
    {
      BehaviorDesignerUtility.taskConnectionRunningBottomTexture = BehaviorDesignerUtility.LoadTaskTexture("TaskConnectionRunningBottom.png", true, (ScriptableObject) null);
    }

    private static void InitTaskConnectionIdentifyTopTexture()
    {
      BehaviorDesignerUtility.taskConnectionIdentifyTopTexture = BehaviorDesignerUtility.LoadTaskTexture("TaskConnectionIdentifyTop.png", true, (ScriptableObject) null);
    }

    private static void InitTaskConnectionIdentifyBottomTexture()
    {
      BehaviorDesignerUtility.taskConnectionIdentifyBottomTexture = BehaviorDesignerUtility.LoadTaskTexture("TaskConnectionIdentifyBottom.png", true, (ScriptableObject) null);
    }

    private static void InitTaskConnectionCollapsedTexture()
    {
      BehaviorDesignerUtility.taskConnectionCollapsedTexture = BehaviorDesignerUtility.LoadTexture("TaskConnectionCollapsed.png", true, (Object) null);
    }

    private static void InitContentSeparatorTexture()
    {
      BehaviorDesignerUtility.contentSeparatorTexture = BehaviorDesignerUtility.LoadTexture("ContentSeparator.png", true, (Object) null);
    }

    private static void InitDocTexture()
    {
      BehaviorDesignerUtility.docTexture = BehaviorDesignerUtility.LoadTexture("DocIcon.png", true, (Object) null);
    }

    private static void InitGearTexture()
    {
      BehaviorDesignerUtility.gearTexture = BehaviorDesignerUtility.LoadTexture("GearIcon.png", true, (Object) null);
    }

    private static void InitColorSelectorTexture(int colorIndex)
    {
      BehaviorDesignerUtility.colorSelectorTexture[colorIndex] = BehaviorDesignerUtility.LoadTexture("ColorSelector" + BehaviorDesignerUtility.ColorIndexToColorString(colorIndex) + ".png", true, (Object) null);
    }

    private static void InitVariableButtonTexture()
    {
      BehaviorDesignerUtility.variableButtonTexture = BehaviorDesignerUtility.LoadTexture("VariableButton.png", true, (Object) null);
    }

    private static void InitVariableButtonSelectedTexture()
    {
      BehaviorDesignerUtility.variableButtonSelectedTexture = BehaviorDesignerUtility.LoadTexture("VariableButtonSelected.png", true, (Object) null);
    }

    private static void InitVariableWatchButtonTexture()
    {
      BehaviorDesignerUtility.variableWatchButtonTexture = BehaviorDesignerUtility.LoadTexture("VariableWatchButton.png", true, (Object) null);
    }

    private static void InitVariableWatchButtonSelectedTexture()
    {
      BehaviorDesignerUtility.variableWatchButtonSelectedTexture = BehaviorDesignerUtility.LoadTexture("VariableWatchButtonSelected.png", true, (Object) null);
    }

    private static void InitReferencedTexture()
    {
      BehaviorDesignerUtility.referencedTexture = BehaviorDesignerUtility.LoadTexture("LinkedIcon.png", true, (Object) null);
    }

    private static void InitConditionalAbortSelfTexture()
    {
      BehaviorDesignerUtility.conditionalAbortSelfTexture = BehaviorDesignerUtility.LoadTexture("ConditionalAbortSelfIcon.png", true, (Object) null);
    }

    private static void InitConditionalAbortLowerPriorityTexture()
    {
      BehaviorDesignerUtility.conditionalAbortLowerPriorityTexture = BehaviorDesignerUtility.LoadTexture("ConditionalAbortLowerPriorityIcon.png", true, (Object) null);
    }

    private static void InitConditionalAbortBothTexture()
    {
      BehaviorDesignerUtility.conditionalAbortBothTexture = BehaviorDesignerUtility.LoadTexture("ConditionalAbortBothIcon.png", true, (Object) null);
    }

    private static void InitDeleteButtonTexture()
    {
      BehaviorDesignerUtility.deleteButtonTexture = BehaviorDesignerUtility.LoadTexture("DeleteButton.png", true, (Object) null);
    }

    private static void InitVariableDeleteButtonTexture()
    {
      BehaviorDesignerUtility.variableDeleteButtonTexture = BehaviorDesignerUtility.LoadTexture("VariableDeleteButton.png", true, (Object) null);
    }

    private static void InitDownArrowButtonTexture()
    {
      BehaviorDesignerUtility.downArrowButtonTexture = BehaviorDesignerUtility.LoadTexture("DownArrowButton.png", true, (Object) null);
    }

    private static void InitUpArrowButtonTexture()
    {
      BehaviorDesignerUtility.upArrowButtonTexture = BehaviorDesignerUtility.LoadTexture("UpArrowButton.png", true, (Object) null);
    }

    private static void InitVariableMapButtonTexture()
    {
      BehaviorDesignerUtility.variableMapButtonTexture = BehaviorDesignerUtility.LoadTexture("VariableMapButton.png", true, (Object) null);
    }

    private static void InitIdentifyButtonTexture()
    {
      BehaviorDesignerUtility.identifyButtonTexture = BehaviorDesignerUtility.LoadTexture("IdentifyButton.png", true, (Object) null);
    }

    private static void InitBreakpointTexture()
    {
      BehaviorDesignerUtility.breakpointTexture = BehaviorDesignerUtility.LoadTexture("BreakpointIcon.png", false, (Object) null);
    }

    private static void InitErrorIconTexture()
    {
      BehaviorDesignerUtility.errorIconTexture = BehaviorDesignerUtility.LoadTexture("ErrorIcon.png", true, (Object) null);
    }

    private static void InitSmallErrorIconTexture()
    {
      BehaviorDesignerUtility.smallErrorIconTexture = BehaviorDesignerUtility.LoadTexture("SmallErrorIcon.png", true, (Object) null);
    }

    private static void InitEnableTaskTexture()
    {
      BehaviorDesignerUtility.enableTaskTexture = BehaviorDesignerUtility.LoadTexture("TaskEnableIcon.png", false, (Object) null);
    }

    private static void InitDisableTaskTexture()
    {
      BehaviorDesignerUtility.disableTaskTexture = BehaviorDesignerUtility.LoadTexture("TaskDisableIcon.png", false, (Object) null);
    }

    private static void InitExpandTaskTexture()
    {
      BehaviorDesignerUtility.expandTaskTexture = BehaviorDesignerUtility.LoadTexture("TaskExpandIcon.png", false, (Object) null);
    }

    private static void InitCollapseTaskTexture()
    {
      BehaviorDesignerUtility.collapseTaskTexture = BehaviorDesignerUtility.LoadTexture("TaskCollapseIcon.png", false, (Object) null);
    }

    private static void InitExecutionSuccessTexture()
    {
      BehaviorDesignerUtility.executionSuccessTexture = BehaviorDesignerUtility.LoadTexture("ExecutionSuccess.png", false, (Object) null);
    }

    private static void InitExecutionFailureTexture()
    {
      BehaviorDesignerUtility.executionFailureTexture = BehaviorDesignerUtility.LoadTexture("ExecutionFailure.png", false, (Object) null);
    }

    private static void InitExecutionSuccessRepeatTexture()
    {
      BehaviorDesignerUtility.executionSuccessRepeatTexture = BehaviorDesignerUtility.LoadTexture("ExecutionSuccessRepeat.png", false, (Object) null);
    }

    private static void InitExecutionFailureRepeatTexture()
    {
      BehaviorDesignerUtility.executionFailureRepeatTexture = BehaviorDesignerUtility.LoadTexture("ExecutionFailureRepeat.png", false, (Object) null);
    }

    private static void InitHistoryBackwardTexture()
    {
      BehaviorDesignerUtility.historyBackwardTexture = BehaviorDesignerUtility.LoadTexture("HistoryBackward.png", true, (Object) null);
    }

    private static void InitHistoryForwardTexture()
    {
      BehaviorDesignerUtility.historyForwardTexture = BehaviorDesignerUtility.LoadTexture("HistoryForward.png", true, (Object) null);
    }

    private static void InitPlayTexture()
    {
      BehaviorDesignerUtility.playTexture = BehaviorDesignerUtility.LoadTexture("Play.png", true, (Object) null);
    }

    private static void InitPauseTexture()
    {
      BehaviorDesignerUtility.pauseTexture = BehaviorDesignerUtility.LoadTexture("Pause.png", true, (Object) null);
    }

    private static void InitStepTexture()
    {
      BehaviorDesignerUtility.stepTexture = BehaviorDesignerUtility.LoadTexture("Step.png", true, (Object) null);
    }

    private static void InitScreenshotBackgroundTexture()
    {
      BehaviorDesignerUtility.screenshotBackgroundTexture = new Texture2D(1, 1, (TextureFormat) 3, false, true);
      if (EditorGUIUtility.get_isProSkin())
        BehaviorDesignerUtility.screenshotBackgroundTexture.SetPixel(1, 1, new Color(0.1647f, 0.1647f, 0.1647f));
      else
        BehaviorDesignerUtility.screenshotBackgroundTexture.SetPixel(1, 1, new Color(0.3647f, 0.3647f, 0.3647f));
      BehaviorDesignerUtility.screenshotBackgroundTexture.Apply();
    }

    public static void SetObjectDirty(Object obj)
    {
      if (EditorApplication.get_isPlaying())
        return;
      if (!EditorUtility.IsPersistent(obj))
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
      else
        EditorUtility.SetDirty(obj);
    }
  }
}
