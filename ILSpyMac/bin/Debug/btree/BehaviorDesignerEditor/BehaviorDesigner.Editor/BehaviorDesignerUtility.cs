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

		private static GUIStyle graphStatusGUIStyle = null;

		private static GUIStyle taskFoldoutGUIStyle = null;

		private static GUIStyle taskTitleGUIStyle = null;

		private static GUIStyle[] taskGUIStyle = new GUIStyle[9];

		private static GUIStyle[] taskCompactGUIStyle = new GUIStyle[9];

		private static GUIStyle[] taskSelectedGUIStyle = new GUIStyle[9];

		private static GUIStyle[] taskSelectedCompactGUIStyle = new GUIStyle[9];

		private static GUIStyle taskRunningGUIStyle = null;

		private static GUIStyle taskRunningCompactGUIStyle = null;

		private static GUIStyle taskRunningSelectedGUIStyle = null;

		private static GUIStyle taskRunningSelectedCompactGUIStyle = null;

		private static GUIStyle taskIdentifyGUIStyle = null;

		private static GUIStyle taskIdentifyCompactGUIStyle = null;

		private static GUIStyle taskIdentifySelectedGUIStyle = null;

		private static GUIStyle taskIdentifySelectedCompactGUIStyle = null;

		private static GUIStyle taskHighlightGUIStyle = null;

		private static GUIStyle taskHighlightCompactGUIStyle = null;

		private static GUIStyle taskCommentGUIStyle = null;

		private static GUIStyle taskCommentLeftAlignGUIStyle = null;

		private static GUIStyle taskCommentRightAlignGUIStyle = null;

		private static GUIStyle taskDescriptionGUIStyle = null;

		private static GUIStyle graphBackgroundGUIStyle = null;

		private static GUIStyle selectionGUIStyle = null;

		private static GUIStyle sharedVariableToolbarPopup = null;

		private static GUIStyle labelWrapGUIStyle = null;

		private static GUIStyle tolbarButtonLeftAlignGUIStyle = null;

		private static GUIStyle toolbarLabelGUIStyle = null;

		private static GUIStyle taskInspectorCommentGUIStyle = null;

		private static GUIStyle taskInspectorGUIStyle = null;

		private static GUIStyle toolbarButtonSelectionGUIStyle = null;

		private static GUIStyle propertyBoxGUIStyle = null;

		private static GUIStyle preferencesPaneGUIStyle = null;

		private static GUIStyle plainButtonGUIStyle = null;

		private static GUIStyle transparentButtonGUIStyle = null;

		private static GUIStyle transparentButtonOffsetGUIStyle = null;

		private static GUIStyle buttonGUIStyle = null;

		private static GUIStyle plainTextureGUIStyle = null;

		private static GUIStyle arrowSeparatorGUIStyle = null;

		private static GUIStyle selectedBackgroundGUIStyle = null;

		private static GUIStyle errorListDarkBackground = null;

		private static GUIStyle errorListLightBackground = null;

		private static GUIStyle welcomeScreenIntroGUIStyle = null;

		private static GUIStyle welcomeScreenTextHeaderGUIStyle = null;

		private static GUIStyle welcomeScreenTextDescriptionGUIStyle = null;

		private static Texture2D[] taskBorderTexture = new Texture2D[9];

		private static Texture2D taskBorderRunningTexture = null;

		private static Texture2D taskBorderIdentifyTexture = null;

		private static Texture2D[] taskConnectionTopTexture = new Texture2D[9];

		private static Texture2D[] taskConnectionBottomTexture = new Texture2D[9];

		private static Texture2D taskConnectionRunningTopTexture = null;

		private static Texture2D taskConnectionRunningBottomTexture = null;

		private static Texture2D taskConnectionIdentifyTopTexture = null;

		private static Texture2D taskConnectionIdentifyBottomTexture = null;

		private static Texture2D taskConnectionCollapsedTexture = null;

		private static Texture2D contentSeparatorTexture = null;

		private static Texture2D docTexture = null;

		private static Texture2D gearTexture = null;

		private static Texture2D[] colorSelectorTexture = new Texture2D[9];

		private static Texture2D variableButtonTexture = null;

		private static Texture2D variableButtonSelectedTexture = null;

		private static Texture2D variableWatchButtonTexture = null;

		private static Texture2D variableWatchButtonSelectedTexture = null;

		private static Texture2D referencedTexture = null;

		private static Texture2D conditionalAbortSelfTexture = null;

		private static Texture2D conditionalAbortLowerPriorityTexture = null;

		private static Texture2D conditionalAbortBothTexture = null;

		private static Texture2D deleteButtonTexture = null;

		private static Texture2D variableDeleteButtonTexture = null;

		private static Texture2D downArrowButtonTexture = null;

		private static Texture2D upArrowButtonTexture = null;

		private static Texture2D variableMapButtonTexture = null;

		private static Texture2D identifyButtonTexture = null;

		private static Texture2D breakpointTexture = null;

		private static Texture2D errorIconTexture = null;

		private static Texture2D smallErrorIconTexture = null;

		private static Texture2D enableTaskTexture = null;

		private static Texture2D disableTaskTexture = null;

		private static Texture2D expandTaskTexture = null;

		private static Texture2D collapseTaskTexture = null;

		private static Texture2D executionSuccessTexture = null;

		private static Texture2D executionFailureTexture = null;

		private static Texture2D executionSuccessRepeatTexture = null;

		private static Texture2D executionFailureRepeatTexture = null;

		public static Texture2D historyBackwardTexture = null;

		public static Texture2D historyForwardTexture = null;

		private static Texture2D playTexture = null;

		private static Texture2D pauseTexture = null;

		private static Texture2D stepTexture = null;

		private static Texture2D screenshotBackgroundTexture = null;

		private static Regex camelCaseRegex = new Regex("(?<=[A-Z])(?=[A-Z][a-z])|(?<=[^A-Z])(?=[A-Z])|(?<=[A-Za-z])(?=[^A-Za-z])", 32);

		private static Dictionary<string, string> camelCaseSplit = new Dictionary<string, string>();

		[NonSerialized]
		private static Dictionary<Type, Dictionary<FieldInfo, bool>> attributeFieldCache = new Dictionary<Type, Dictionary<FieldInfo, bool>>();

		private static Dictionary<string, Texture2D> textureCache = new Dictionary<string, Texture2D>();

		private static Dictionary<string, Texture2D> iconCache = new Dictionary<string, Texture2D>();

		public static GUIStyle GraphStatusGUIStyle
		{
			get
			{
				if (BehaviorDesignerUtility.graphStatusGUIStyle == null)
				{
					BehaviorDesignerUtility.InitGraphStatusGUIStyle();
				}
				return BehaviorDesignerUtility.graphStatusGUIStyle;
			}
		}

		public static GUIStyle TaskFoldoutGUIStyle
		{
			get
			{
				if (BehaviorDesignerUtility.taskFoldoutGUIStyle == null)
				{
					BehaviorDesignerUtility.InitTaskFoldoutGUIStyle();
				}
				return BehaviorDesignerUtility.taskFoldoutGUIStyle;
			}
		}

		public static GUIStyle TaskTitleGUIStyle
		{
			get
			{
				if (BehaviorDesignerUtility.taskTitleGUIStyle == null)
				{
					BehaviorDesignerUtility.InitTaskTitleGUIStyle();
				}
				return BehaviorDesignerUtility.taskTitleGUIStyle;
			}
		}

		public static GUIStyle TaskRunningGUIStyle
		{
			get
			{
				if (BehaviorDesignerUtility.taskRunningGUIStyle == null)
				{
					BehaviorDesignerUtility.InitTaskRunningGUIStyle();
				}
				return BehaviorDesignerUtility.taskRunningGUIStyle;
			}
		}

		public static GUIStyle TaskRunningCompactGUIStyle
		{
			get
			{
				if (BehaviorDesignerUtility.taskRunningCompactGUIStyle == null)
				{
					BehaviorDesignerUtility.InitTaskRunningCompactGUIStyle();
				}
				return BehaviorDesignerUtility.taskRunningCompactGUIStyle;
			}
		}

		public static GUIStyle TaskRunningSelectedGUIStyle
		{
			get
			{
				if (BehaviorDesignerUtility.taskRunningSelectedGUIStyle == null)
				{
					BehaviorDesignerUtility.InitTaskRunningSelectedGUIStyle();
				}
				return BehaviorDesignerUtility.taskRunningSelectedGUIStyle;
			}
		}

		public static GUIStyle TaskRunningSelectedCompactGUIStyle
		{
			get
			{
				if (BehaviorDesignerUtility.taskRunningSelectedCompactGUIStyle == null)
				{
					BehaviorDesignerUtility.InitTaskRunningSelectedCompactGUIStyle();
				}
				return BehaviorDesignerUtility.taskRunningSelectedCompactGUIStyle;
			}
		}

		public static GUIStyle TaskIdentifyGUIStyle
		{
			get
			{
				if (BehaviorDesignerUtility.taskIdentifyGUIStyle == null)
				{
					BehaviorDesignerUtility.InitTaskIdentifyGUIStyle();
				}
				return BehaviorDesignerUtility.taskIdentifyGUIStyle;
			}
		}

		public static GUIStyle TaskIdentifyCompactGUIStyle
		{
			get
			{
				if (BehaviorDesignerUtility.taskIdentifyCompactGUIStyle == null)
				{
					BehaviorDesignerUtility.InitTaskIdentifyCompactGUIStyle();
				}
				return BehaviorDesignerUtility.taskIdentifyCompactGUIStyle;
			}
		}

		public static GUIStyle TaskIdentifySelectedGUIStyle
		{
			get
			{
				if (BehaviorDesignerUtility.taskIdentifySelectedGUIStyle == null)
				{
					BehaviorDesignerUtility.InitTaskIdentifySelectedGUIStyle();
				}
				return BehaviorDesignerUtility.taskIdentifySelectedGUIStyle;
			}
		}

		public static GUIStyle TaskIdentifySelectedCompactGUIStyle
		{
			get
			{
				if (BehaviorDesignerUtility.taskIdentifySelectedCompactGUIStyle == null)
				{
					BehaviorDesignerUtility.InitTaskIdentifySelectedCompactGUIStyle();
				}
				return BehaviorDesignerUtility.taskIdentifySelectedCompactGUIStyle;
			}
		}

		public static GUIStyle TaskHighlightGUIStyle
		{
			get
			{
				if (BehaviorDesignerUtility.taskHighlightGUIStyle == null)
				{
					BehaviorDesignerUtility.InitTaskHighlightGUIStyle();
				}
				return BehaviorDesignerUtility.taskHighlightGUIStyle;
			}
		}

		public static GUIStyle TaskHighlightCompactGUIStyle
		{
			get
			{
				if (BehaviorDesignerUtility.taskHighlightCompactGUIStyle == null)
				{
					BehaviorDesignerUtility.InitTaskHighlightCompactGUIStyle();
				}
				return BehaviorDesignerUtility.taskHighlightCompactGUIStyle;
			}
		}

		public static GUIStyle TaskCommentGUIStyle
		{
			get
			{
				if (BehaviorDesignerUtility.taskCommentGUIStyle == null)
				{
					BehaviorDesignerUtility.InitTaskCommentGUIStyle();
				}
				return BehaviorDesignerUtility.taskCommentGUIStyle;
			}
		}

		public static GUIStyle TaskCommentLeftAlignGUIStyle
		{
			get
			{
				if (BehaviorDesignerUtility.taskCommentLeftAlignGUIStyle == null)
				{
					BehaviorDesignerUtility.InitTaskCommentLeftAlignGUIStyle();
				}
				return BehaviorDesignerUtility.taskCommentLeftAlignGUIStyle;
			}
		}

		public static GUIStyle TaskCommentRightAlignGUIStyle
		{
			get
			{
				if (BehaviorDesignerUtility.taskCommentRightAlignGUIStyle == null)
				{
					BehaviorDesignerUtility.InitTaskCommentRightAlignGUIStyle();
				}
				return BehaviorDesignerUtility.taskCommentRightAlignGUIStyle;
			}
		}

		public static GUIStyle TaskDescriptionGUIStyle
		{
			get
			{
				if (BehaviorDesignerUtility.taskDescriptionGUIStyle == null)
				{
					BehaviorDesignerUtility.InitTaskDescriptionGUIStyle();
				}
				return BehaviorDesignerUtility.taskDescriptionGUIStyle;
			}
		}

		public static GUIStyle GraphBackgroundGUIStyle
		{
			get
			{
				if (BehaviorDesignerUtility.graphBackgroundGUIStyle == null)
				{
					BehaviorDesignerUtility.InitGraphBackgroundGUIStyle();
				}
				return BehaviorDesignerUtility.graphBackgroundGUIStyle;
			}
		}

		public static GUIStyle SelectionGUIStyle
		{
			get
			{
				if (BehaviorDesignerUtility.selectionGUIStyle == null)
				{
					BehaviorDesignerUtility.InitSelectionGUIStyle();
				}
				return BehaviorDesignerUtility.selectionGUIStyle;
			}
		}

		public static GUIStyle SharedVariableToolbarPopup
		{
			get
			{
				if (BehaviorDesignerUtility.sharedVariableToolbarPopup == null)
				{
					BehaviorDesignerUtility.InitSharedVariableToolbarPopup();
				}
				return BehaviorDesignerUtility.sharedVariableToolbarPopup;
			}
		}

		public static GUIStyle LabelWrapGUIStyle
		{
			get
			{
				if (BehaviorDesignerUtility.labelWrapGUIStyle == null)
				{
					BehaviorDesignerUtility.InitLabelWrapGUIStyle();
				}
				return BehaviorDesignerUtility.labelWrapGUIStyle;
			}
		}

		public static GUIStyle ToolbarButtonLeftAlignGUIStyle
		{
			get
			{
				if (BehaviorDesignerUtility.tolbarButtonLeftAlignGUIStyle == null)
				{
					BehaviorDesignerUtility.InitToolbarButtonLeftAlignGUIStyle();
				}
				return BehaviorDesignerUtility.tolbarButtonLeftAlignGUIStyle;
			}
		}

		public static GUIStyle ToolbarLabelGUIStyle
		{
			get
			{
				if (BehaviorDesignerUtility.toolbarLabelGUIStyle == null)
				{
					BehaviorDesignerUtility.InitToolbarLabelGUIStyle();
				}
				return BehaviorDesignerUtility.toolbarLabelGUIStyle;
			}
		}

		public static GUIStyle TaskInspectorCommentGUIStyle
		{
			get
			{
				if (BehaviorDesignerUtility.taskInspectorCommentGUIStyle == null)
				{
					BehaviorDesignerUtility.InitTaskInspectorCommentGUIStyle();
				}
				return BehaviorDesignerUtility.taskInspectorCommentGUIStyle;
			}
		}

		public static GUIStyle TaskInspectorGUIStyle
		{
			get
			{
				if (BehaviorDesignerUtility.taskInspectorGUIStyle == null)
				{
					BehaviorDesignerUtility.InitTaskInspectorGUIStyle();
				}
				return BehaviorDesignerUtility.taskInspectorGUIStyle;
			}
		}

		public static GUIStyle ToolbarButtonSelectionGUIStyle
		{
			get
			{
				if (BehaviorDesignerUtility.toolbarButtonSelectionGUIStyle == null)
				{
					BehaviorDesignerUtility.InitToolbarButtonSelectionGUIStyle();
				}
				return BehaviorDesignerUtility.toolbarButtonSelectionGUIStyle;
			}
		}

		public static GUIStyle PropertyBoxGUIStyle
		{
			get
			{
				if (BehaviorDesignerUtility.propertyBoxGUIStyle == null)
				{
					BehaviorDesignerUtility.InitPropertyBoxGUIStyle();
				}
				return BehaviorDesignerUtility.propertyBoxGUIStyle;
			}
		}

		public static GUIStyle PreferencesPaneGUIStyle
		{
			get
			{
				if (BehaviorDesignerUtility.preferencesPaneGUIStyle == null)
				{
					BehaviorDesignerUtility.InitPreferencesPaneGUIStyle();
				}
				return BehaviorDesignerUtility.preferencesPaneGUIStyle;
			}
		}

		public static GUIStyle PlainButtonGUIStyle
		{
			get
			{
				if (BehaviorDesignerUtility.plainButtonGUIStyle == null)
				{
					BehaviorDesignerUtility.InitPlainButtonGUIStyle();
				}
				return BehaviorDesignerUtility.plainButtonGUIStyle;
			}
		}

		public static GUIStyle TransparentButtonGUIStyle
		{
			get
			{
				if (BehaviorDesignerUtility.transparentButtonGUIStyle == null)
				{
					BehaviorDesignerUtility.InitTransparentButtonGUIStyle();
				}
				return BehaviorDesignerUtility.transparentButtonGUIStyle;
			}
		}

		public static GUIStyle TransparentButtonOffsetGUIStyle
		{
			get
			{
				if (BehaviorDesignerUtility.transparentButtonOffsetGUIStyle == null)
				{
					BehaviorDesignerUtility.InitTransparentButtonOffsetGUIStyle();
				}
				return BehaviorDesignerUtility.transparentButtonOffsetGUIStyle;
			}
		}

		public static GUIStyle ButtonGUIStyle
		{
			get
			{
				if (BehaviorDesignerUtility.buttonGUIStyle == null)
				{
					BehaviorDesignerUtility.InitButtonGUIStyle();
				}
				return BehaviorDesignerUtility.buttonGUIStyle;
			}
		}

		public static GUIStyle PlainTextureGUIStyle
		{
			get
			{
				if (BehaviorDesignerUtility.plainTextureGUIStyle == null)
				{
					BehaviorDesignerUtility.InitPlainTextureGUIStyle();
				}
				return BehaviorDesignerUtility.plainTextureGUIStyle;
			}
		}

		public static GUIStyle ArrowSeparatorGUIStyle
		{
			get
			{
				if (BehaviorDesignerUtility.arrowSeparatorGUIStyle == null)
				{
					BehaviorDesignerUtility.InitArrowSeparatorGUIStyle();
				}
				return BehaviorDesignerUtility.arrowSeparatorGUIStyle;
			}
		}

		public static GUIStyle SelectedBackgroundGUIStyle
		{
			get
			{
				if (BehaviorDesignerUtility.selectedBackgroundGUIStyle == null)
				{
					BehaviorDesignerUtility.InitSelectedBackgroundGUIStyle();
				}
				return BehaviorDesignerUtility.selectedBackgroundGUIStyle;
			}
		}

		public static GUIStyle ErrorListDarkBackground
		{
			get
			{
				if (BehaviorDesignerUtility.errorListDarkBackground == null)
				{
					BehaviorDesignerUtility.InitErrorListDarkBackground();
				}
				return BehaviorDesignerUtility.errorListDarkBackground;
			}
		}

		public static GUIStyle ErrorListLightBackground
		{
			get
			{
				if (BehaviorDesignerUtility.errorListLightBackground == null)
				{
					BehaviorDesignerUtility.InitErrorListLightBackground();
				}
				return BehaviorDesignerUtility.errorListLightBackground;
			}
		}

		public static GUIStyle WelcomeScreenIntroGUIStyle
		{
			get
			{
				if (BehaviorDesignerUtility.welcomeScreenIntroGUIStyle == null)
				{
					BehaviorDesignerUtility.InitWelcomeScreenIntroGUIStyle();
				}
				return BehaviorDesignerUtility.welcomeScreenIntroGUIStyle;
			}
		}

		public static GUIStyle WelcomeScreenTextHeaderGUIStyle
		{
			get
			{
				if (BehaviorDesignerUtility.welcomeScreenTextHeaderGUIStyle == null)
				{
					BehaviorDesignerUtility.InitWelcomeScreenTextHeaderGUIStyle();
				}
				return BehaviorDesignerUtility.welcomeScreenTextHeaderGUIStyle;
			}
		}

		public static GUIStyle WelcomeScreenTextDescriptionGUIStyle
		{
			get
			{
				if (BehaviorDesignerUtility.welcomeScreenTextDescriptionGUIStyle == null)
				{
					BehaviorDesignerUtility.InitWelcomeScreenTextDescriptionGUIStyle();
				}
				return BehaviorDesignerUtility.welcomeScreenTextDescriptionGUIStyle;
			}
		}

		public static Texture2D TaskBorderRunningTexture
		{
			get
			{
				if (BehaviorDesignerUtility.taskBorderRunningTexture == null)
				{
					BehaviorDesignerUtility.InitTaskBorderRunningTexture();
				}
				return BehaviorDesignerUtility.taskBorderRunningTexture;
			}
		}

		public static Texture2D TaskBorderIdentifyTexture
		{
			get
			{
				if (BehaviorDesignerUtility.taskBorderIdentifyTexture == null)
				{
					BehaviorDesignerUtility.InitTaskBorderIdentifyTexture();
				}
				return BehaviorDesignerUtility.taskBorderIdentifyTexture;
			}
		}

		public static Texture2D TaskConnectionRunningTopTexture
		{
			get
			{
				if (BehaviorDesignerUtility.taskConnectionRunningTopTexture == null)
				{
					BehaviorDesignerUtility.InitTaskConnectionRunningTopTexture();
				}
				return BehaviorDesignerUtility.taskConnectionRunningTopTexture;
			}
		}

		public static Texture2D TaskConnectionRunningBottomTexture
		{
			get
			{
				if (BehaviorDesignerUtility.taskConnectionRunningBottomTexture == null)
				{
					BehaviorDesignerUtility.InitTaskConnectionRunningBottomTexture();
				}
				return BehaviorDesignerUtility.taskConnectionRunningBottomTexture;
			}
		}

		public static Texture2D TaskConnectionIdentifyTopTexture
		{
			get
			{
				if (BehaviorDesignerUtility.taskConnectionIdentifyTopTexture == null)
				{
					BehaviorDesignerUtility.InitTaskConnectionIdentifyTopTexture();
				}
				return BehaviorDesignerUtility.taskConnectionIdentifyTopTexture;
			}
		}

		public static Texture2D TaskConnectionIdentifyBottomTexture
		{
			get
			{
				if (BehaviorDesignerUtility.taskConnectionIdentifyBottomTexture == null)
				{
					BehaviorDesignerUtility.InitTaskConnectionIdentifyBottomTexture();
				}
				return BehaviorDesignerUtility.taskConnectionIdentifyBottomTexture;
			}
		}

		public static Texture2D TaskConnectionCollapsedTexture
		{
			get
			{
				if (BehaviorDesignerUtility.taskConnectionCollapsedTexture == null)
				{
					BehaviorDesignerUtility.InitTaskConnectionCollapsedTexture();
				}
				return BehaviorDesignerUtility.taskConnectionCollapsedTexture;
			}
		}

		public static Texture2D ContentSeparatorTexture
		{
			get
			{
				if (BehaviorDesignerUtility.contentSeparatorTexture == null)
				{
					BehaviorDesignerUtility.InitContentSeparatorTexture();
				}
				return BehaviorDesignerUtility.contentSeparatorTexture;
			}
		}

		public static Texture2D DocTexture
		{
			get
			{
				if (BehaviorDesignerUtility.docTexture == null)
				{
					BehaviorDesignerUtility.InitDocTexture();
				}
				return BehaviorDesignerUtility.docTexture;
			}
		}

		public static Texture2D GearTexture
		{
			get
			{
				if (BehaviorDesignerUtility.gearTexture == null)
				{
					BehaviorDesignerUtility.InitGearTexture();
				}
				return BehaviorDesignerUtility.gearTexture;
			}
		}

		public static Texture2D VariableButtonTexture
		{
			get
			{
				if (BehaviorDesignerUtility.variableButtonTexture == null)
				{
					BehaviorDesignerUtility.InitVariableButtonTexture();
				}
				return BehaviorDesignerUtility.variableButtonTexture;
			}
		}

		public static Texture2D VariableButtonSelectedTexture
		{
			get
			{
				if (BehaviorDesignerUtility.variableButtonSelectedTexture == null)
				{
					BehaviorDesignerUtility.InitVariableButtonSelectedTexture();
				}
				return BehaviorDesignerUtility.variableButtonSelectedTexture;
			}
		}

		public static Texture2D VariableWatchButtonTexture
		{
			get
			{
				if (BehaviorDesignerUtility.variableWatchButtonTexture == null)
				{
					BehaviorDesignerUtility.InitVariableWatchButtonTexture();
				}
				return BehaviorDesignerUtility.variableWatchButtonTexture;
			}
		}

		public static Texture2D VariableWatchButtonSelectedTexture
		{
			get
			{
				if (BehaviorDesignerUtility.variableWatchButtonSelectedTexture == null)
				{
					BehaviorDesignerUtility.InitVariableWatchButtonSelectedTexture();
				}
				return BehaviorDesignerUtility.variableWatchButtonSelectedTexture;
			}
		}

		public static Texture2D ReferencedTexture
		{
			get
			{
				if (BehaviorDesignerUtility.referencedTexture == null)
				{
					BehaviorDesignerUtility.InitReferencedTexture();
				}
				return BehaviorDesignerUtility.referencedTexture;
			}
		}

		public static Texture2D ConditionalAbortSelfTexture
		{
			get
			{
				if (BehaviorDesignerUtility.conditionalAbortSelfTexture == null)
				{
					BehaviorDesignerUtility.InitConditionalAbortSelfTexture();
				}
				return BehaviorDesignerUtility.conditionalAbortSelfTexture;
			}
		}

		public static Texture2D ConditionalAbortLowerPriorityTexture
		{
			get
			{
				if (BehaviorDesignerUtility.conditionalAbortLowerPriorityTexture == null)
				{
					BehaviorDesignerUtility.InitConditionalAbortLowerPriorityTexture();
				}
				return BehaviorDesignerUtility.conditionalAbortLowerPriorityTexture;
			}
		}

		public static Texture2D ConditionalAbortBothTexture
		{
			get
			{
				if (BehaviorDesignerUtility.conditionalAbortBothTexture == null)
				{
					BehaviorDesignerUtility.InitConditionalAbortBothTexture();
				}
				return BehaviorDesignerUtility.conditionalAbortBothTexture;
			}
		}

		public static Texture2D DeleteButtonTexture
		{
			get
			{
				if (BehaviorDesignerUtility.deleteButtonTexture == null)
				{
					BehaviorDesignerUtility.InitDeleteButtonTexture();
				}
				return BehaviorDesignerUtility.deleteButtonTexture;
			}
		}

		public static Texture2D VariableDeleteButtonTexture
		{
			get
			{
				if (BehaviorDesignerUtility.variableDeleteButtonTexture == null)
				{
					BehaviorDesignerUtility.InitVariableDeleteButtonTexture();
				}
				return BehaviorDesignerUtility.variableDeleteButtonTexture;
			}
		}

		public static Texture2D DownArrowButtonTexture
		{
			get
			{
				if (BehaviorDesignerUtility.downArrowButtonTexture == null)
				{
					BehaviorDesignerUtility.InitDownArrowButtonTexture();
				}
				return BehaviorDesignerUtility.downArrowButtonTexture;
			}
		}

		public static Texture2D UpArrowButtonTexture
		{
			get
			{
				if (BehaviorDesignerUtility.upArrowButtonTexture == null)
				{
					BehaviorDesignerUtility.InitUpArrowButtonTexture();
				}
				return BehaviorDesignerUtility.upArrowButtonTexture;
			}
		}

		public static Texture2D VariableMapButtonTexture
		{
			get
			{
				if (BehaviorDesignerUtility.variableMapButtonTexture == null)
				{
					BehaviorDesignerUtility.InitVariableMapButtonTexture();
				}
				return BehaviorDesignerUtility.variableMapButtonTexture;
			}
		}

		public static Texture2D IdentifyButtonTexture
		{
			get
			{
				if (BehaviorDesignerUtility.identifyButtonTexture == null)
				{
					BehaviorDesignerUtility.InitIdentifyButtonTexture();
				}
				return BehaviorDesignerUtility.identifyButtonTexture;
			}
		}

		public static Texture2D BreakpointTexture
		{
			get
			{
				if (BehaviorDesignerUtility.breakpointTexture == null)
				{
					BehaviorDesignerUtility.InitBreakpointTexture();
				}
				return BehaviorDesignerUtility.breakpointTexture;
			}
		}

		public static Texture2D ErrorIconTexture
		{
			get
			{
				if (BehaviorDesignerUtility.errorIconTexture == null)
				{
					BehaviorDesignerUtility.InitErrorIconTexture();
				}
				return BehaviorDesignerUtility.errorIconTexture;
			}
		}

		public static Texture2D SmallErrorIconTexture
		{
			get
			{
				if (BehaviorDesignerUtility.smallErrorIconTexture == null)
				{
					BehaviorDesignerUtility.InitSmallErrorIconTexture();
				}
				return BehaviorDesignerUtility.smallErrorIconTexture;
			}
		}

		public static Texture2D EnableTaskTexture
		{
			get
			{
				if (BehaviorDesignerUtility.enableTaskTexture == null)
				{
					BehaviorDesignerUtility.InitEnableTaskTexture();
				}
				return BehaviorDesignerUtility.enableTaskTexture;
			}
		}

		public static Texture2D DisableTaskTexture
		{
			get
			{
				if (BehaviorDesignerUtility.disableTaskTexture == null)
				{
					BehaviorDesignerUtility.InitDisableTaskTexture();
				}
				return BehaviorDesignerUtility.disableTaskTexture;
			}
		}

		public static Texture2D ExpandTaskTexture
		{
			get
			{
				if (BehaviorDesignerUtility.expandTaskTexture == null)
				{
					BehaviorDesignerUtility.InitExpandTaskTexture();
				}
				return BehaviorDesignerUtility.expandTaskTexture;
			}
		}

		public static Texture2D CollapseTaskTexture
		{
			get
			{
				if (BehaviorDesignerUtility.collapseTaskTexture == null)
				{
					BehaviorDesignerUtility.InitCollapseTaskTexture();
				}
				return BehaviorDesignerUtility.collapseTaskTexture;
			}
		}

		public static Texture2D ExecutionSuccessTexture
		{
			get
			{
				if (BehaviorDesignerUtility.executionSuccessTexture == null)
				{
					BehaviorDesignerUtility.InitExecutionSuccessTexture();
				}
				return BehaviorDesignerUtility.executionSuccessTexture;
			}
		}

		public static Texture2D ExecutionFailureTexture
		{
			get
			{
				if (BehaviorDesignerUtility.executionFailureTexture == null)
				{
					BehaviorDesignerUtility.InitExecutionFailureTexture();
				}
				return BehaviorDesignerUtility.executionFailureTexture;
			}
		}

		public static Texture2D ExecutionSuccessRepeatTexture
		{
			get
			{
				if (BehaviorDesignerUtility.executionSuccessRepeatTexture == null)
				{
					BehaviorDesignerUtility.InitExecutionSuccessRepeatTexture();
				}
				return BehaviorDesignerUtility.executionSuccessRepeatTexture;
			}
		}

		public static Texture2D ExecutionFailureRepeatTexture
		{
			get
			{
				if (BehaviorDesignerUtility.executionFailureRepeatTexture == null)
				{
					BehaviorDesignerUtility.InitExecutionFailureRepeatTexture();
				}
				return BehaviorDesignerUtility.executionFailureRepeatTexture;
			}
		}

		public static Texture2D HistoryBackwardTexture
		{
			get
			{
				if (BehaviorDesignerUtility.historyBackwardTexture == null)
				{
					BehaviorDesignerUtility.InitHistoryBackwardTexture();
				}
				return BehaviorDesignerUtility.historyBackwardTexture;
			}
		}

		public static Texture2D HistoryForwardTexture
		{
			get
			{
				if (BehaviorDesignerUtility.historyForwardTexture == null)
				{
					BehaviorDesignerUtility.InitHistoryForwardTexture();
				}
				return BehaviorDesignerUtility.historyForwardTexture;
			}
		}

		public static Texture2D PlayTexture
		{
			get
			{
				if (BehaviorDesignerUtility.playTexture == null)
				{
					BehaviorDesignerUtility.InitPlayTexture();
				}
				return BehaviorDesignerUtility.playTexture;
			}
		}

		public static Texture2D PauseTexture
		{
			get
			{
				if (BehaviorDesignerUtility.pauseTexture == null)
				{
					BehaviorDesignerUtility.InitPauseTexture();
				}
				return BehaviorDesignerUtility.pauseTexture;
			}
		}

		public static Texture2D StepTexture
		{
			get
			{
				if (BehaviorDesignerUtility.stepTexture == null)
				{
					BehaviorDesignerUtility.InitStepTexture();
				}
				return BehaviorDesignerUtility.stepTexture;
			}
		}

		public static Texture2D ScreenshotBackgroundTexture
		{
			get
			{
				if (BehaviorDesignerUtility.screenshotBackgroundTexture == null)
				{
					BehaviorDesignerUtility.InitScreenshotBackgroundTexture();
				}
				return BehaviorDesignerUtility.screenshotBackgroundTexture;
			}
		}

		public static GUIStyle GetTaskGUIStyle(int colorIndex)
		{
			if (BehaviorDesignerUtility.taskGUIStyle[colorIndex] == null)
			{
				BehaviorDesignerUtility.InitTaskGUIStyle(colorIndex);
			}
			return BehaviorDesignerUtility.taskGUIStyle[colorIndex];
		}

		public static GUIStyle GetTaskCompactGUIStyle(int colorIndex)
		{
			if (BehaviorDesignerUtility.taskCompactGUIStyle[colorIndex] == null)
			{
				BehaviorDesignerUtility.InitTaskCompactGUIStyle(colorIndex);
			}
			return BehaviorDesignerUtility.taskCompactGUIStyle[colorIndex];
		}

		public static GUIStyle GetTaskSelectedGUIStyle(int colorIndex)
		{
			if (BehaviorDesignerUtility.taskSelectedGUIStyle[colorIndex] == null)
			{
				BehaviorDesignerUtility.InitTaskSelectedGUIStyle(colorIndex);
			}
			return BehaviorDesignerUtility.taskSelectedGUIStyle[colorIndex];
		}

		public static GUIStyle GetTaskSelectedCompactGUIStyle(int colorIndex)
		{
			if (BehaviorDesignerUtility.taskSelectedCompactGUIStyle[colorIndex] == null)
			{
				BehaviorDesignerUtility.InitTaskSelectedCompactGUIStyle(colorIndex);
			}
			return BehaviorDesignerUtility.taskSelectedCompactGUIStyle[colorIndex];
		}

		public static Texture2D GetTaskBorderTexture(int colorIndex)
		{
			if (BehaviorDesignerUtility.taskBorderTexture[colorIndex] == null)
			{
				BehaviorDesignerUtility.InitTaskBorderTexture(colorIndex);
			}
			return BehaviorDesignerUtility.taskBorderTexture[colorIndex];
		}

		public static Texture2D GetTaskConnectionTopTexture(int colorIndex)
		{
			if (BehaviorDesignerUtility.taskConnectionTopTexture[colorIndex] == null)
			{
				BehaviorDesignerUtility.InitTaskConnectionTopTexture(colorIndex);
			}
			return BehaviorDesignerUtility.taskConnectionTopTexture[colorIndex];
		}

		public static Texture2D GetTaskConnectionBottomTexture(int colorIndex)
		{
			if (BehaviorDesignerUtility.taskConnectionBottomTexture[colorIndex] == null)
			{
				BehaviorDesignerUtility.InitTaskConnectionBottomTexture(colorIndex);
			}
			return BehaviorDesignerUtility.taskConnectionBottomTexture[colorIndex];
		}

		public static Texture2D ColorSelectorTexture(int colorIndex)
		{
			if (BehaviorDesignerUtility.colorSelectorTexture[colorIndex] == null)
			{
				BehaviorDesignerUtility.InitColorSelectorTexture(colorIndex);
			}
			return BehaviorDesignerUtility.colorSelectorTexture[colorIndex];
		}

		public static string SplitCamelCase(string s)
		{
			if (s.Equals(string.Empty))
			{
				return s;
			}
			if (BehaviorDesignerUtility.camelCaseSplit.ContainsKey(s))
			{
				return BehaviorDesignerUtility.camelCaseSplit.get_Item(s);
			}
			string text = s;
			s = s.Replace("_uScript", "uScript");
			s = s.Replace("_PlayMaker", "PlayMaker");
			if (s.get_Length() > 2 && s.Substring(0, 2).CompareTo("m_") == 0)
			{
				s = s.Substring(2);
			}
			else if (s.get_Length() > 1 && s.get_Chars(0).CompareTo('_') == 0)
			{
				s = s.Substring(1);
			}
			s = BehaviorDesignerUtility.camelCaseRegex.Replace(s, " ");
			s = s.Replace("_", " ");
			s = s.Replace("u Script", " uScript");
			s = s.Replace("Play Maker", "PlayMaker");
			s = (char.ToUpper(s.get_Chars(0)) + s.Substring(1)).Trim();
			BehaviorDesignerUtility.camelCaseSplit.Add(text, s);
			return s;
		}

		public static bool HasAttribute(FieldInfo field, Type attributeType)
		{
			Dictionary<FieldInfo, bool> dictionary = null;
			if (BehaviorDesignerUtility.attributeFieldCache.ContainsKey(attributeType))
			{
				dictionary = BehaviorDesignerUtility.attributeFieldCache.get_Item(attributeType);
			}
			if (dictionary == null)
			{
				dictionary = new Dictionary<FieldInfo, bool>();
			}
			if (dictionary.ContainsKey(field))
			{
				return dictionary.get_Item(field);
			}
			bool flag = field.GetCustomAttributes(attributeType, false).Length > 0;
			dictionary.Add(field, flag);
			if (!BehaviorDesignerUtility.attributeFieldCache.ContainsKey(attributeType))
			{
				BehaviorDesignerUtility.attributeFieldCache.Add(attributeType, dictionary);
			}
			return flag;
		}

		public static List<Task> GetAllTasks(BehaviorSource behaviorSource)
		{
			List<Task> result = new List<Task>();
			if (behaviorSource.get_RootTask() != null)
			{
				BehaviorDesignerUtility.GetAllTasks(behaviorSource.get_RootTask(), ref result);
			}
			if (behaviorSource.get_DetachedTasks() != null)
			{
				for (int i = 0; i < behaviorSource.get_DetachedTasks().get_Count(); i++)
				{
					BehaviorDesignerUtility.GetAllTasks(behaviorSource.get_DetachedTasks().get_Item(i), ref result);
				}
			}
			return result;
		}

		private static void GetAllTasks(Task task, ref List<Task> taskList)
		{
			taskList.Add(task);
			ParentTask parentTask;
			if ((parentTask = (task as ParentTask)) != null && parentTask.get_Children() != null)
			{
				for (int i = 0; i < parentTask.get_Children().get_Count(); i++)
				{
					BehaviorDesignerUtility.GetAllTasks(parentTask.get_Children().get_Item(i), ref taskList);
				}
			}
		}

		public static bool AnyNullTasks(BehaviorSource behaviorSource)
		{
			if (behaviorSource.get_RootTask() != null && BehaviorDesignerUtility.AnyNullTasks(behaviorSource.get_RootTask()))
			{
				return true;
			}
			if (behaviorSource.get_DetachedTasks() != null)
			{
				for (int i = 0; i < behaviorSource.get_DetachedTasks().get_Count(); i++)
				{
					if (BehaviorDesignerUtility.AnyNullTasks(behaviorSource.get_DetachedTasks().get_Item(i)))
					{
						return true;
					}
				}
			}
			return false;
		}

		private static bool AnyNullTasks(Task task)
		{
			if (task == null)
			{
				return true;
			}
			ParentTask parentTask;
			if ((parentTask = (task as ParentTask)) != null && parentTask.get_Children() != null)
			{
				for (int i = 0; i < parentTask.get_Children().get_Count(); i++)
				{
					if (BehaviorDesignerUtility.AnyNullTasks(parentTask.get_Children().get_Item(i)))
					{
						return true;
					}
				}
			}
			return false;
		}

		public static bool HasRootTask(string serialization)
		{
			if (string.IsNullOrEmpty(serialization))
			{
				return false;
			}
			Dictionary<string, object> dictionary = MiniJSON.Deserialize(serialization) as Dictionary<string, object>;
			return dictionary != null && dictionary.ContainsKey("RootTask");
		}

		public static string GetEditorBaseDirectory(Object obj = null)
		{
			string codeBase = Assembly.GetExecutingAssembly().get_CodeBase();
			string text = Uri.UnescapeDataString(new UriBuilder(codeBase).get_Path());
			return Path.GetDirectoryName(text.Substring(Application.get_dataPath().get_Length() - 6));
		}

		public static Texture2D LoadTexture(string imageName, bool useSkinColor = true, Object obj = null)
		{
			if (BehaviorDesignerUtility.textureCache.ContainsKey(imageName))
			{
				return BehaviorDesignerUtility.textureCache.get_Item(imageName);
			}
			Texture2D texture2D = null;
			string text = string.Format("{0}{1}", (!useSkinColor) ? string.Empty : ((!EditorGUIUtility.get_isProSkin()) ? "Light" : "Dark"), imageName);
			Stream manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(text);
			if (manifestResourceStream == null)
			{
				text = string.Format("BehaviorDesignerEditor.Resources.{0}{1}", (!useSkinColor) ? string.Empty : ((!EditorGUIUtility.get_isProSkin()) ? "Light" : "Dark"), imageName);
				manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(text);
			}
			if (manifestResourceStream != null)
			{
				texture2D = new Texture2D(0, 0, 4, false, true);
				ImageConversion.LoadImage(texture2D, BehaviorDesignerUtility.ReadToEnd(manifestResourceStream));
				manifestResourceStream.Close();
			}
			texture2D.set_hideFlags(61);
			BehaviorDesignerUtility.textureCache.Add(imageName, texture2D);
			return texture2D;
		}

		private static Texture2D LoadTaskTexture(string imageName, bool useSkinColor = true, ScriptableObject obj = null)
		{
			if (BehaviorDesignerUtility.textureCache.ContainsKey(imageName))
			{
				return BehaviorDesignerUtility.textureCache.get_Item(imageName);
			}
			Texture2D texture2D = null;
			string text = string.Format("{0}{1}", (!useSkinColor) ? string.Empty : ((!EditorGUIUtility.get_isProSkin()) ? "Light" : "Dark"), imageName);
			Stream manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(text);
			if (manifestResourceStream == null)
			{
				text = string.Format("BehaviorDesignerEditor.Resources.{0}{1}", (!useSkinColor) ? string.Empty : ((!EditorGUIUtility.get_isProSkin()) ? "Light" : "Dark"), imageName);
				manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(text);
			}
			if (manifestResourceStream != null)
			{
				texture2D = new Texture2D(0, 0, 4, false, true);
				ImageConversion.LoadImage(texture2D, BehaviorDesignerUtility.ReadToEnd(manifestResourceStream));
				manifestResourceStream.Close();
			}
			if (texture2D == null)
			{
				Debug.Log(string.Format("{0}/Images/Task Backgrounds/{1}{2}", BehaviorDesignerUtility.GetEditorBaseDirectory(obj), (!useSkinColor) ? string.Empty : ((!EditorGUIUtility.get_isProSkin()) ? "Light" : "Dark"), imageName));
			}
			texture2D.set_hideFlags(61);
			BehaviorDesignerUtility.textureCache.Add(imageName, texture2D);
			return texture2D;
		}

		public static Texture2D LoadIcon(string iconName, ScriptableObject obj = null)
		{
			if (BehaviorDesignerUtility.iconCache.ContainsKey(iconName))
			{
				return BehaviorDesignerUtility.iconCache.get_Item(iconName);
			}
			Texture2D texture2D = null;
			string text = iconName.Replace("{SkinColor}", (!EditorGUIUtility.get_isProSkin()) ? "Light" : "Dark");
			Stream manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(text);
			if (manifestResourceStream == null)
			{
				text = string.Format("BehaviorDesignerEditor.Resources.{0}", iconName.Replace("{SkinColor}", (!EditorGUIUtility.get_isProSkin()) ? "Light" : "Dark"));
				manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(text);
			}
			if (manifestResourceStream != null)
			{
				texture2D = new Texture2D(0, 0, 4, false, true);
				ImageConversion.LoadImage(texture2D, BehaviorDesignerUtility.ReadToEnd(manifestResourceStream));
				manifestResourceStream.Close();
			}
			if (texture2D == null)
			{
				texture2D = (AssetDatabase.LoadAssetAtPath(iconName.Replace("{SkinColor}", (!EditorGUIUtility.get_isProSkin()) ? "Light" : "Dark"), typeof(Texture2D)) as Texture2D);
			}
			if (texture2D != null)
			{
				texture2D.set_hideFlags(61);
			}
			BehaviorDesignerUtility.iconCache.Add(iconName, texture2D);
			return texture2D;
		}

		private static byte[] ReadToEnd(Stream stream)
		{
			byte[] array = new byte[16384];
			byte[] result;
			using (MemoryStream memoryStream = new MemoryStream())
			{
				int num;
				while ((num = stream.Read(array, 0, array.Length)) > 0)
				{
					memoryStream.Write(array, 0, num);
				}
				result = memoryStream.ToArray();
			}
			return result;
		}

		public static void DrawContentSeperator(int yOffset)
		{
			BehaviorDesignerUtility.DrawContentSeperator(yOffset, 0);
		}

		public static void DrawContentSeperator(int yOffset, int widthExtension)
		{
			Rect lastRect = GUILayoutUtility.GetLastRect();
			lastRect.set_x(-5f);
			lastRect.set_y(lastRect.get_y() + (lastRect.get_height() + (float)yOffset));
			lastRect.set_height(2f);
			lastRect.set_width(lastRect.get_width() + (float)(10 + widthExtension));
			GUI.DrawTexture(lastRect, BehaviorDesignerUtility.ContentSeparatorTexture);
		}

		public static float RoundToNearest(float num, float baseNum)
		{
			return (float)((int)Math.Round((double)(num / baseNum), 1)) * baseNum;
		}

		private static void InitGraphStatusGUIStyle()
		{
			BehaviorDesignerUtility.graphStatusGUIStyle = new GUIStyle(GUI.get_skin().get_label());
			BehaviorDesignerUtility.graphStatusGUIStyle.set_alignment(3);
			BehaviorDesignerUtility.graphStatusGUIStyle.set_fontSize(20);
			BehaviorDesignerUtility.graphStatusGUIStyle.set_fontStyle(1);
			if (EditorGUIUtility.get_isProSkin())
			{
				BehaviorDesignerUtility.graphStatusGUIStyle.get_normal().set_textColor(new Color(0.7058f, 0.7058f, 0.7058f));
			}
			else
			{
				BehaviorDesignerUtility.graphStatusGUIStyle.get_normal().set_textColor(new Color(0.8058f, 0.8058f, 0.8058f));
			}
		}

		private static void InitTaskFoldoutGUIStyle()
		{
			BehaviorDesignerUtility.taskFoldoutGUIStyle = new GUIStyle(EditorStyles.get_foldout());
			BehaviorDesignerUtility.taskFoldoutGUIStyle.set_alignment(3);
			BehaviorDesignerUtility.taskFoldoutGUIStyle.set_fontSize(13);
			BehaviorDesignerUtility.taskFoldoutGUIStyle.set_fontStyle(1);
		}

		private static void InitTaskTitleGUIStyle()
		{
			BehaviorDesignerUtility.taskTitleGUIStyle = new GUIStyle(GUI.get_skin().get_label());
			BehaviorDesignerUtility.taskTitleGUIStyle.set_alignment(1);
			BehaviorDesignerUtility.taskTitleGUIStyle.set_fontSize(12);
			BehaviorDesignerUtility.taskTitleGUIStyle.set_fontStyle(0);
		}

		private static void InitTaskGUIStyle(int colorIndex)
		{
			BehaviorDesignerUtility.taskGUIStyle[colorIndex] = BehaviorDesignerUtility.InitTaskGUIStyle(BehaviorDesignerUtility.LoadTaskTexture("Task" + BehaviorDesignerUtility.ColorIndexToColorString(colorIndex) + ".png", true, null), new RectOffset(5, 3, 3, 5));
		}

		private static void InitTaskCompactGUIStyle(int colorIndex)
		{
			BehaviorDesignerUtility.taskCompactGUIStyle[colorIndex] = BehaviorDesignerUtility.InitTaskGUIStyle(BehaviorDesignerUtility.LoadTaskTexture("TaskCompact" + BehaviorDesignerUtility.ColorIndexToColorString(colorIndex) + ".png", true, null), new RectOffset(5, 4, 4, 5));
		}

		private static void InitTaskSelectedGUIStyle(int colorIndex)
		{
			BehaviorDesignerUtility.taskSelectedGUIStyle[colorIndex] = BehaviorDesignerUtility.InitTaskGUIStyle(BehaviorDesignerUtility.LoadTaskTexture("TaskSelected" + BehaviorDesignerUtility.ColorIndexToColorString(colorIndex) + ".png", true, null), new RectOffset(5, 4, 4, 4));
		}

		private static void InitTaskSelectedCompactGUIStyle(int colorIndex)
		{
			BehaviorDesignerUtility.taskSelectedCompactGUIStyle[colorIndex] = BehaviorDesignerUtility.InitTaskGUIStyle(BehaviorDesignerUtility.LoadTaskTexture("TaskSelectedCompact" + BehaviorDesignerUtility.ColorIndexToColorString(colorIndex) + ".png", true, null), new RectOffset(5, 4, 4, 4));
		}

		private static string ColorIndexToColorString(int index)
		{
			if (index == 0)
			{
				return string.Empty;
			}
			if (index == 1)
			{
				return "Red";
			}
			if (index == 2)
			{
				return "Pink";
			}
			if (index == 3)
			{
				return "Brown";
			}
			if (index == 4)
			{
				return "RedOrange";
			}
			if (index == 5)
			{
				return "Turquoise";
			}
			if (index == 6)
			{
				return "Cyan";
			}
			if (index == 7)
			{
				return "Blue";
			}
			if (index == 8)
			{
				return "Purple";
			}
			return string.Empty;
		}

		private static void InitTaskRunningGUIStyle()
		{
			BehaviorDesignerUtility.taskRunningGUIStyle = BehaviorDesignerUtility.InitTaskGUIStyle(BehaviorDesignerUtility.LoadTaskTexture("TaskRunning.png", true, null), new RectOffset(5, 3, 3, 5));
		}

		private static void InitTaskRunningCompactGUIStyle()
		{
			BehaviorDesignerUtility.taskRunningCompactGUIStyle = BehaviorDesignerUtility.InitTaskGUIStyle(BehaviorDesignerUtility.LoadTaskTexture("TaskRunningCompact.png", true, null), new RectOffset(5, 4, 4, 5));
		}

		private static void InitTaskRunningSelectedGUIStyle()
		{
			BehaviorDesignerUtility.taskRunningSelectedGUIStyle = BehaviorDesignerUtility.InitTaskGUIStyle(BehaviorDesignerUtility.LoadTaskTexture("TaskRunningSelected.png", true, null), new RectOffset(5, 4, 4, 4));
		}

		private static void InitTaskRunningSelectedCompactGUIStyle()
		{
			BehaviorDesignerUtility.taskRunningSelectedCompactGUIStyle = BehaviorDesignerUtility.InitTaskGUIStyle(BehaviorDesignerUtility.LoadTaskTexture("TaskRunningSelectedCompact.png", true, null), new RectOffset(5, 4, 4, 4));
		}

		private static void InitTaskIdentifyGUIStyle()
		{
			BehaviorDesignerUtility.taskIdentifyGUIStyle = BehaviorDesignerUtility.InitTaskGUIStyle(BehaviorDesignerUtility.LoadTaskTexture("TaskIdentify.png", true, null), new RectOffset(5, 3, 3, 5));
		}

		private static void InitTaskIdentifyCompactGUIStyle()
		{
			BehaviorDesignerUtility.taskIdentifyCompactGUIStyle = BehaviorDesignerUtility.InitTaskGUIStyle(BehaviorDesignerUtility.LoadTaskTexture("TaskIdentifyCompact.png", true, null), new RectOffset(5, 4, 4, 5));
		}

		private static void InitTaskIdentifySelectedGUIStyle()
		{
			BehaviorDesignerUtility.taskIdentifySelectedGUIStyle = BehaviorDesignerUtility.InitTaskGUIStyle(BehaviorDesignerUtility.LoadTaskTexture("TaskIdentifySelected.png", true, null), new RectOffset(5, 4, 4, 4));
		}

		private static void InitTaskIdentifySelectedCompactGUIStyle()
		{
			BehaviorDesignerUtility.taskIdentifySelectedCompactGUIStyle = BehaviorDesignerUtility.InitTaskGUIStyle(BehaviorDesignerUtility.LoadTaskTexture("TaskIdentifySelectedCompact.png", true, null), new RectOffset(5, 4, 4, 4));
		}

		private static void InitTaskHighlightGUIStyle()
		{
			BehaviorDesignerUtility.taskHighlightGUIStyle = BehaviorDesignerUtility.InitTaskGUIStyle(BehaviorDesignerUtility.LoadTaskTexture("TaskHighlight.png", true, null), new RectOffset(5, 4, 4, 4));
		}

		private static void InitTaskHighlightCompactGUIStyle()
		{
			BehaviorDesignerUtility.taskHighlightCompactGUIStyle = BehaviorDesignerUtility.InitTaskGUIStyle(BehaviorDesignerUtility.LoadTaskTexture("TaskHighlightCompact.png", true, null), new RectOffset(5, 4, 4, 4));
		}

		private static GUIStyle InitTaskGUIStyle(Texture2D texture, RectOffset overflow)
		{
			GUIStyle gUIStyle = new GUIStyle(GUI.get_skin().get_box());
			gUIStyle.set_border(new RectOffset(10, 10, 10, 10));
			gUIStyle.set_overflow(overflow);
			gUIStyle.get_normal().set_background(texture);
			gUIStyle.get_active().set_background(texture);
			gUIStyle.get_hover().set_background(texture);
			gUIStyle.get_focused().set_background(texture);
			gUIStyle.get_normal().set_textColor(Color.get_white());
			gUIStyle.get_active().set_textColor(Color.get_white());
			gUIStyle.get_hover().set_textColor(Color.get_white());
			gUIStyle.get_focused().set_textColor(Color.get_white());
			gUIStyle.set_stretchHeight(true);
			gUIStyle.set_stretchWidth(true);
			return gUIStyle;
		}

		private static void InitTaskCommentGUIStyle()
		{
			BehaviorDesignerUtility.taskCommentGUIStyle = new GUIStyle(GUI.get_skin().get_label());
			BehaviorDesignerUtility.taskCommentGUIStyle.set_alignment(1);
			BehaviorDesignerUtility.taskCommentGUIStyle.set_fontSize(12);
			BehaviorDesignerUtility.taskCommentGUIStyle.set_fontStyle(0);
			BehaviorDesignerUtility.taskCommentGUIStyle.set_wordWrap(true);
		}

		private static void InitTaskCommentLeftAlignGUIStyle()
		{
			BehaviorDesignerUtility.taskCommentLeftAlignGUIStyle = new GUIStyle(GUI.get_skin().get_label());
			BehaviorDesignerUtility.taskCommentLeftAlignGUIStyle.set_alignment(0);
			BehaviorDesignerUtility.taskCommentLeftAlignGUIStyle.set_fontSize(12);
			BehaviorDesignerUtility.taskCommentLeftAlignGUIStyle.set_fontStyle(0);
			BehaviorDesignerUtility.taskCommentLeftAlignGUIStyle.set_wordWrap(false);
		}

		private static void InitTaskCommentRightAlignGUIStyle()
		{
			BehaviorDesignerUtility.taskCommentRightAlignGUIStyle = new GUIStyle(GUI.get_skin().get_label());
			BehaviorDesignerUtility.taskCommentRightAlignGUIStyle.set_alignment(2);
			BehaviorDesignerUtility.taskCommentRightAlignGUIStyle.set_fontSize(12);
			BehaviorDesignerUtility.taskCommentRightAlignGUIStyle.set_fontStyle(0);
			BehaviorDesignerUtility.taskCommentRightAlignGUIStyle.set_wordWrap(false);
		}

		private static void InitTaskDescriptionGUIStyle()
		{
			Texture2D texture2D = new Texture2D(1, 1, 4, false, true);
			if (EditorGUIUtility.get_isProSkin())
			{
				texture2D.SetPixel(1, 1, new Color(0.1647f, 0.1647f, 0.1647f));
			}
			else
			{
				texture2D.SetPixel(1, 1, new Color(0.75f, 0.75f, 0.75f));
			}
			texture2D.set_hideFlags(61);
			texture2D.Apply();
			BehaviorDesignerUtility.taskDescriptionGUIStyle = new GUIStyle(GUI.get_skin().get_box());
			BehaviorDesignerUtility.taskDescriptionGUIStyle.get_normal().set_background(texture2D);
			BehaviorDesignerUtility.taskDescriptionGUIStyle.get_active().set_background(texture2D);
			BehaviorDesignerUtility.taskDescriptionGUIStyle.get_hover().set_background(texture2D);
			BehaviorDesignerUtility.taskDescriptionGUIStyle.get_focused().set_background(texture2D);
		}

		private static void InitGraphBackgroundGUIStyle()
		{
			Texture2D texture2D = new Texture2D(1, 1, 4, false, true);
			if (EditorGUIUtility.get_isProSkin())
			{
				texture2D.SetPixel(1, 1, new Color(0.1647f, 0.1647f, 0.1647f));
			}
			else
			{
				texture2D.SetPixel(1, 1, new Color(0.3647f, 0.3647f, 0.3647f));
			}
			texture2D.set_hideFlags(61);
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
			Texture2D texture2D = new Texture2D(1, 1, 4, false, true);
			Color color = (!EditorGUIUtility.get_isProSkin()) ? new Color(0.243f, 0.5686f, 0.839f, 0.5f) : new Color(0.188f, 0.4588f, 0.6862f, 0.5f);
			texture2D.SetPixel(1, 1, color);
			texture2D.set_hideFlags(61);
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
			BehaviorDesignerUtility.labelWrapGUIStyle.set_alignment(4);
		}

		private static void InitToolbarButtonLeftAlignGUIStyle()
		{
			BehaviorDesignerUtility.tolbarButtonLeftAlignGUIStyle = new GUIStyle(EditorStyles.get_toolbarButton());
			BehaviorDesignerUtility.tolbarButtonLeftAlignGUIStyle.set_alignment(3);
		}

		private static void InitToolbarLabelGUIStyle()
		{
			BehaviorDesignerUtility.toolbarLabelGUIStyle = new GUIStyle(EditorStyles.get_label());
			BehaviorDesignerUtility.toolbarLabelGUIStyle.get_normal().set_textColor((!EditorGUIUtility.get_isProSkin()) ? new Color(0f, 0.5f, 0f) : new Color(0f, 0.7f, 0f));
		}

		private static void InitTaskInspectorCommentGUIStyle()
		{
			BehaviorDesignerUtility.taskInspectorCommentGUIStyle = new GUIStyle(GUI.get_skin().get_textArea());
			BehaviorDesignerUtility.taskInspectorCommentGUIStyle.set_wordWrap(true);
		}

		private static void InitTaskInspectorGUIStyle()
		{
			BehaviorDesignerUtility.taskInspectorGUIStyle = new GUIStyle(GUI.get_skin().get_label());
			BehaviorDesignerUtility.taskInspectorGUIStyle.set_alignment(3);
			BehaviorDesignerUtility.taskInspectorGUIStyle.set_fontSize(11);
			BehaviorDesignerUtility.taskInspectorGUIStyle.set_fontStyle(0);
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
			BehaviorDesignerUtility.plainButtonGUIStyle.get_normal().set_background(null);
			BehaviorDesignerUtility.plainButtonGUIStyle.get_active().set_background(null);
			BehaviorDesignerUtility.plainButtonGUIStyle.get_hover().set_background(null);
			BehaviorDesignerUtility.plainButtonGUIStyle.get_focused().set_background(null);
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
			BehaviorDesignerUtility.transparentButtonGUIStyle.get_normal().set_background(null);
			BehaviorDesignerUtility.transparentButtonGUIStyle.get_active().set_background(null);
			BehaviorDesignerUtility.transparentButtonGUIStyle.get_hover().set_background(null);
			BehaviorDesignerUtility.transparentButtonGUIStyle.get_focused().set_background(null);
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
			BehaviorDesignerUtility.transparentButtonOffsetGUIStyle.get_normal().set_background(null);
			BehaviorDesignerUtility.transparentButtonOffsetGUIStyle.get_active().set_background(null);
			BehaviorDesignerUtility.transparentButtonOffsetGUIStyle.get_hover().set_background(null);
			BehaviorDesignerUtility.transparentButtonOffsetGUIStyle.get_focused().set_background(null);
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
			BehaviorDesignerUtility.plainTextureGUIStyle.get_normal().set_background(null);
			BehaviorDesignerUtility.plainTextureGUIStyle.get_active().set_background(null);
			BehaviorDesignerUtility.plainTextureGUIStyle.get_hover().set_background(null);
			BehaviorDesignerUtility.plainTextureGUIStyle.get_focused().set_background(null);
		}

		private static void InitArrowSeparatorGUIStyle()
		{
			BehaviorDesignerUtility.arrowSeparatorGUIStyle = new GUIStyle();
			BehaviorDesignerUtility.arrowSeparatorGUIStyle.set_border(new RectOffset(0, 0, 0, 0));
			BehaviorDesignerUtility.arrowSeparatorGUIStyle.set_margin(new RectOffset(0, 0, 3, 0));
			BehaviorDesignerUtility.arrowSeparatorGUIStyle.set_padding(new RectOffset(0, 0, 0, 0));
			Texture2D background = BehaviorDesignerUtility.LoadTexture("ArrowSeparator.png", true, null);
			BehaviorDesignerUtility.arrowSeparatorGUIStyle.get_normal().set_background(background);
			BehaviorDesignerUtility.arrowSeparatorGUIStyle.get_active().set_background(background);
			BehaviorDesignerUtility.arrowSeparatorGUIStyle.get_hover().set_background(background);
			BehaviorDesignerUtility.arrowSeparatorGUIStyle.get_focused().set_background(background);
		}

		private static void InitSelectedBackgroundGUIStyle()
		{
			Texture2D texture2D = new Texture2D(1, 1, 4, false, true);
			Color color = (!EditorGUIUtility.get_isProSkin()) ? new Color(0.243f, 0.5686f, 0.839f, 0.5f) : new Color(0.188f, 0.4588f, 0.6862f, 0.5f);
			texture2D.SetPixel(1, 1, color);
			texture2D.set_hideFlags(61);
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
			Texture2D texture2D = new Texture2D(1, 1, 4, false, true);
			Color color = (!EditorGUIUtility.get_isProSkin()) ? new Color(0.706f, 0.706f, 0.706f) : new Color(0.2f, 0.2f, 0.2f, 1f);
			texture2D.SetPixel(1, 1, color);
			texture2D.set_hideFlags(61);
			texture2D.Apply();
			BehaviorDesignerUtility.errorListDarkBackground = new GUIStyle();
			BehaviorDesignerUtility.errorListDarkBackground.set_padding(new RectOffset(2, 0, 2, 0));
			BehaviorDesignerUtility.errorListDarkBackground.get_normal().set_background(texture2D);
			BehaviorDesignerUtility.errorListDarkBackground.get_active().set_background(texture2D);
			BehaviorDesignerUtility.errorListDarkBackground.get_hover().set_background(texture2D);
			BehaviorDesignerUtility.errorListDarkBackground.get_focused().set_background(texture2D);
			BehaviorDesignerUtility.errorListDarkBackground.get_normal().set_textColor((!EditorGUIUtility.get_isProSkin()) ? new Color(0.206f, 0.206f, 0.206f) : new Color(0.706f, 0.706f, 0.706f));
			BehaviorDesignerUtility.errorListDarkBackground.set_alignment(0);
			BehaviorDesignerUtility.errorListDarkBackground.set_wordWrap(true);
		}

		private static void InitErrorListLightBackground()
		{
			BehaviorDesignerUtility.errorListLightBackground = new GUIStyle();
			BehaviorDesignerUtility.errorListLightBackground.set_padding(new RectOffset(2, 0, 2, 0));
			BehaviorDesignerUtility.errorListLightBackground.get_normal().set_textColor((!EditorGUIUtility.get_isProSkin()) ? new Color(0.106f, 0.106f, 0.106f) : new Color(0.706f, 0.706f, 0.706f));
			BehaviorDesignerUtility.errorListLightBackground.set_alignment(0);
			BehaviorDesignerUtility.errorListLightBackground.set_wordWrap(true);
		}

		private static void InitWelcomeScreenIntroGUIStyle()
		{
			BehaviorDesignerUtility.welcomeScreenIntroGUIStyle = new GUIStyle(GUI.get_skin().get_label());
			BehaviorDesignerUtility.welcomeScreenIntroGUIStyle.set_fontSize(16);
			BehaviorDesignerUtility.welcomeScreenIntroGUIStyle.set_fontStyle(1);
			BehaviorDesignerUtility.welcomeScreenIntroGUIStyle.get_normal().set_textColor(new Color(0.706f, 0.706f, 0.706f));
		}

		private static void InitWelcomeScreenTextHeaderGUIStyle()
		{
			BehaviorDesignerUtility.welcomeScreenTextHeaderGUIStyle = new GUIStyle(GUI.get_skin().get_label());
			BehaviorDesignerUtility.welcomeScreenTextHeaderGUIStyle.set_alignment(3);
			BehaviorDesignerUtility.welcomeScreenTextHeaderGUIStyle.set_fontSize(14);
			BehaviorDesignerUtility.welcomeScreenTextHeaderGUIStyle.set_fontStyle(1);
		}

		private static void InitWelcomeScreenTextDescriptionGUIStyle()
		{
			BehaviorDesignerUtility.welcomeScreenTextDescriptionGUIStyle = new GUIStyle(GUI.get_skin().get_label());
			BehaviorDesignerUtility.welcomeScreenTextDescriptionGUIStyle.set_wordWrap(true);
		}

		private static void InitTaskBorderTexture(int colorIndex)
		{
			BehaviorDesignerUtility.taskBorderTexture[colorIndex] = BehaviorDesignerUtility.LoadTaskTexture("TaskBorder" + BehaviorDesignerUtility.ColorIndexToColorString(colorIndex) + ".png", true, null);
		}

		private static void InitTaskBorderRunningTexture()
		{
			BehaviorDesignerUtility.taskBorderRunningTexture = BehaviorDesignerUtility.LoadTaskTexture("TaskBorderRunning.png", true, null);
		}

		private static void InitTaskBorderIdentifyTexture()
		{
			BehaviorDesignerUtility.taskBorderIdentifyTexture = BehaviorDesignerUtility.LoadTaskTexture("TaskBorderIdentify.png", true, null);
		}

		private static void InitTaskConnectionTopTexture(int colorIndex)
		{
			BehaviorDesignerUtility.taskConnectionTopTexture[colorIndex] = BehaviorDesignerUtility.LoadTaskTexture("TaskConnectionTop" + BehaviorDesignerUtility.ColorIndexToColorString(colorIndex) + ".png", true, null);
		}

		private static void InitTaskConnectionBottomTexture(int colorIndex)
		{
			BehaviorDesignerUtility.taskConnectionBottomTexture[colorIndex] = BehaviorDesignerUtility.LoadTaskTexture("TaskConnectionBottom" + BehaviorDesignerUtility.ColorIndexToColorString(colorIndex) + ".png", true, null);
		}

		private static void InitTaskConnectionRunningTopTexture()
		{
			BehaviorDesignerUtility.taskConnectionRunningTopTexture = BehaviorDesignerUtility.LoadTaskTexture("TaskConnectionRunningTop.png", true, null);
		}

		private static void InitTaskConnectionRunningBottomTexture()
		{
			BehaviorDesignerUtility.taskConnectionRunningBottomTexture = BehaviorDesignerUtility.LoadTaskTexture("TaskConnectionRunningBottom.png", true, null);
		}

		private static void InitTaskConnectionIdentifyTopTexture()
		{
			BehaviorDesignerUtility.taskConnectionIdentifyTopTexture = BehaviorDesignerUtility.LoadTaskTexture("TaskConnectionIdentifyTop.png", true, null);
		}

		private static void InitTaskConnectionIdentifyBottomTexture()
		{
			BehaviorDesignerUtility.taskConnectionIdentifyBottomTexture = BehaviorDesignerUtility.LoadTaskTexture("TaskConnectionIdentifyBottom.png", true, null);
		}

		private static void InitTaskConnectionCollapsedTexture()
		{
			BehaviorDesignerUtility.taskConnectionCollapsedTexture = BehaviorDesignerUtility.LoadTexture("TaskConnectionCollapsed.png", true, null);
		}

		private static void InitContentSeparatorTexture()
		{
			BehaviorDesignerUtility.contentSeparatorTexture = BehaviorDesignerUtility.LoadTexture("ContentSeparator.png", true, null);
		}

		private static void InitDocTexture()
		{
			BehaviorDesignerUtility.docTexture = BehaviorDesignerUtility.LoadTexture("DocIcon.png", true, null);
		}

		private static void InitGearTexture()
		{
			BehaviorDesignerUtility.gearTexture = BehaviorDesignerUtility.LoadTexture("GearIcon.png", true, null);
		}

		private static void InitColorSelectorTexture(int colorIndex)
		{
			BehaviorDesignerUtility.colorSelectorTexture[colorIndex] = BehaviorDesignerUtility.LoadTexture("ColorSelector" + BehaviorDesignerUtility.ColorIndexToColorString(colorIndex) + ".png", true, null);
		}

		private static void InitVariableButtonTexture()
		{
			BehaviorDesignerUtility.variableButtonTexture = BehaviorDesignerUtility.LoadTexture("VariableButton.png", true, null);
		}

		private static void InitVariableButtonSelectedTexture()
		{
			BehaviorDesignerUtility.variableButtonSelectedTexture = BehaviorDesignerUtility.LoadTexture("VariableButtonSelected.png", true, null);
		}

		private static void InitVariableWatchButtonTexture()
		{
			BehaviorDesignerUtility.variableWatchButtonTexture = BehaviorDesignerUtility.LoadTexture("VariableWatchButton.png", true, null);
		}

		private static void InitVariableWatchButtonSelectedTexture()
		{
			BehaviorDesignerUtility.variableWatchButtonSelectedTexture = BehaviorDesignerUtility.LoadTexture("VariableWatchButtonSelected.png", true, null);
		}

		private static void InitReferencedTexture()
		{
			BehaviorDesignerUtility.referencedTexture = BehaviorDesignerUtility.LoadTexture("LinkedIcon.png", true, null);
		}

		private static void InitConditionalAbortSelfTexture()
		{
			BehaviorDesignerUtility.conditionalAbortSelfTexture = BehaviorDesignerUtility.LoadTexture("ConditionalAbortSelfIcon.png", true, null);
		}

		private static void InitConditionalAbortLowerPriorityTexture()
		{
			BehaviorDesignerUtility.conditionalAbortLowerPriorityTexture = BehaviorDesignerUtility.LoadTexture("ConditionalAbortLowerPriorityIcon.png", true, null);
		}

		private static void InitConditionalAbortBothTexture()
		{
			BehaviorDesignerUtility.conditionalAbortBothTexture = BehaviorDesignerUtility.LoadTexture("ConditionalAbortBothIcon.png", true, null);
		}

		private static void InitDeleteButtonTexture()
		{
			BehaviorDesignerUtility.deleteButtonTexture = BehaviorDesignerUtility.LoadTexture("DeleteButton.png", true, null);
		}

		private static void InitVariableDeleteButtonTexture()
		{
			BehaviorDesignerUtility.variableDeleteButtonTexture = BehaviorDesignerUtility.LoadTexture("VariableDeleteButton.png", true, null);
		}

		private static void InitDownArrowButtonTexture()
		{
			BehaviorDesignerUtility.downArrowButtonTexture = BehaviorDesignerUtility.LoadTexture("DownArrowButton.png", true, null);
		}

		private static void InitUpArrowButtonTexture()
		{
			BehaviorDesignerUtility.upArrowButtonTexture = BehaviorDesignerUtility.LoadTexture("UpArrowButton.png", true, null);
		}

		private static void InitVariableMapButtonTexture()
		{
			BehaviorDesignerUtility.variableMapButtonTexture = BehaviorDesignerUtility.LoadTexture("VariableMapButton.png", true, null);
		}

		private static void InitIdentifyButtonTexture()
		{
			BehaviorDesignerUtility.identifyButtonTexture = BehaviorDesignerUtility.LoadTexture("IdentifyButton.png", true, null);
		}

		private static void InitBreakpointTexture()
		{
			BehaviorDesignerUtility.breakpointTexture = BehaviorDesignerUtility.LoadTexture("BreakpointIcon.png", false, null);
		}

		private static void InitErrorIconTexture()
		{
			BehaviorDesignerUtility.errorIconTexture = BehaviorDesignerUtility.LoadTexture("ErrorIcon.png", true, null);
		}

		private static void InitSmallErrorIconTexture()
		{
			BehaviorDesignerUtility.smallErrorIconTexture = BehaviorDesignerUtility.LoadTexture("SmallErrorIcon.png", true, null);
		}

		private static void InitEnableTaskTexture()
		{
			BehaviorDesignerUtility.enableTaskTexture = BehaviorDesignerUtility.LoadTexture("TaskEnableIcon.png", false, null);
		}

		private static void InitDisableTaskTexture()
		{
			BehaviorDesignerUtility.disableTaskTexture = BehaviorDesignerUtility.LoadTexture("TaskDisableIcon.png", false, null);
		}

		private static void InitExpandTaskTexture()
		{
			BehaviorDesignerUtility.expandTaskTexture = BehaviorDesignerUtility.LoadTexture("TaskExpandIcon.png", false, null);
		}

		private static void InitCollapseTaskTexture()
		{
			BehaviorDesignerUtility.collapseTaskTexture = BehaviorDesignerUtility.LoadTexture("TaskCollapseIcon.png", false, null);
		}

		private static void InitExecutionSuccessTexture()
		{
			BehaviorDesignerUtility.executionSuccessTexture = BehaviorDesignerUtility.LoadTexture("ExecutionSuccess.png", false, null);
		}

		private static void InitExecutionFailureTexture()
		{
			BehaviorDesignerUtility.executionFailureTexture = BehaviorDesignerUtility.LoadTexture("ExecutionFailure.png", false, null);
		}

		private static void InitExecutionSuccessRepeatTexture()
		{
			BehaviorDesignerUtility.executionSuccessRepeatTexture = BehaviorDesignerUtility.LoadTexture("ExecutionSuccessRepeat.png", false, null);
		}

		private static void InitExecutionFailureRepeatTexture()
		{
			BehaviorDesignerUtility.executionFailureRepeatTexture = BehaviorDesignerUtility.LoadTexture("ExecutionFailureRepeat.png", false, null);
		}

		private static void InitHistoryBackwardTexture()
		{
			BehaviorDesignerUtility.historyBackwardTexture = BehaviorDesignerUtility.LoadTexture("HistoryBackward.png", true, null);
		}

		private static void InitHistoryForwardTexture()
		{
			BehaviorDesignerUtility.historyForwardTexture = BehaviorDesignerUtility.LoadTexture("HistoryForward.png", true, null);
		}

		private static void InitPlayTexture()
		{
			BehaviorDesignerUtility.playTexture = BehaviorDesignerUtility.LoadTexture("Play.png", true, null);
		}

		private static void InitPauseTexture()
		{
			BehaviorDesignerUtility.pauseTexture = BehaviorDesignerUtility.LoadTexture("Pause.png", true, null);
		}

		private static void InitStepTexture()
		{
			BehaviorDesignerUtility.stepTexture = BehaviorDesignerUtility.LoadTexture("Step.png", true, null);
		}

		private static void InitScreenshotBackgroundTexture()
		{
			BehaviorDesignerUtility.screenshotBackgroundTexture = new Texture2D(1, 1, 3, false, true);
			if (EditorGUIUtility.get_isProSkin())
			{
				BehaviorDesignerUtility.screenshotBackgroundTexture.SetPixel(1, 1, new Color(0.1647f, 0.1647f, 0.1647f));
			}
			else
			{
				BehaviorDesignerUtility.screenshotBackgroundTexture.SetPixel(1, 1, new Color(0.3647f, 0.3647f, 0.3647f));
			}
			BehaviorDesignerUtility.screenshotBackgroundTexture.Apply();
		}

		public static void SetObjectDirty(Object obj)
		{
			if (EditorApplication.get_isPlaying())
			{
				return;
			}
			if (!EditorUtility.IsPersistent(obj))
			{
				EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
				return;
			}
			EditorUtility.SetDirty(obj);
		}
	}
}
