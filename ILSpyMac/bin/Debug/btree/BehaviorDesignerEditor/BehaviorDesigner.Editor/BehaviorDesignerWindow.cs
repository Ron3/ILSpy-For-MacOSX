using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace BehaviorDesigner.Editor
{
	public class BehaviorDesignerWindow : EditorWindow
	{
		private enum BreadcrumbMenuType
		{
			GameObjectBehavior = 0,
			GameObject = 1,
			Behavior = 2
		}

		public delegate void TaskCallbackHandler(BehaviorSource behaviorSource, Task task);

		[SerializeField]
		public static BehaviorDesignerWindow instance;

		private Rect mGraphRect;

		private Rect mGraphScrollRect;

		private Rect mFileToolBarRect;

		private Rect mDebugToolBarRect;

		private Rect mPropertyToolbarRect;

		private Rect mPropertyBoxRect;

		private Rect mPreferencesPaneRect;

		private Vector2 mGraphScrollSize = new Vector2(20000f, 20000f);

		private bool mSizesInitialized;

		private float mPrevScreenWidth = -1f;

		private float mPrevScreenHeight = -1f;

		private bool mPropertiesPanelOnLeft = true;

		private Vector2 mCurrentMousePosition = Vector2.get_zero();

		private Vector2 mGraphScrollPosition = new Vector2(-1f, -1f);

		private Vector2 mGraphOffset = Vector2.get_zero();

		private float mGraphZoom = 1f;

		private int mBehaviorToolbarSelection = 1;

		private string[] mBehaviorToolbarStrings = new string[]
		{
			"Behavior",
			"Tasks",
			"Variables",
			"Inspector"
		};

		private string mGraphStatus = string.Empty;

		private Material mGridMaterial;

		private int mGUITickCount;

		private Vector2 mSelectStartPosition = Vector2.get_zero();

		private Rect mSelectionArea;

		private bool mIsSelecting;

		private bool mIsDragging;

		private bool mKeepTasksSelected;

		private bool mNodeClicked;

		private Vector2 mDragDelta = Vector2.get_zero();

		private bool mCommandDown;

		private bool mUpdateNodeTaskMap;

		private bool mStepApplication;

		private Dictionary<NodeDesigner, Task> mNodeDesignerTaskMap;

		private bool mEditorAtBreakpoint;

		[SerializeField]
		private List<ErrorDetails> mErrorDetails;

		private GenericMenu mRightClickMenu;

		[SerializeField]
		private GenericMenu mBreadcrumbGameObjectBehaviorMenu;

		[SerializeField]
		private GenericMenu mBreadcrumbGameObjectMenu;

		[SerializeField]
		private GenericMenu mBreadcrumbBehaviorMenu;

		[SerializeField]
		private GenericMenu mReferencedBehaviorsMenu;

		private bool mShowRightClickMenu;

		private bool mShowPrefPane;

		[SerializeField]
		private GraphDesigner mGraphDesigner;

		private TaskInspector mTaskInspector;

		private TaskList mTaskList;

		private VariableInspector mVariableInspector;

		[SerializeField]
		private Object mActiveObject;

		private Object mPrevActiveObject;

		private BehaviorSource mActiveBehaviorSource;

		private BehaviorSource mExternalParent;

		private int mActiveBehaviorID = -1;

		[SerializeField]
		private List<Object> mBehaviorSourceHistory = new List<Object>();

		[SerializeField]
		private int mBehaviorSourceHistoryIndex = -1;

		private BehaviorManager mBehaviorManager;

		private bool mLockActiveGameObject;

		private bool mLoadedFromInspector;

		[SerializeField]
		private bool mIsPlaying;

		private WWW mUpdateCheckRequest;

		private DateTime mLastUpdateCheck = DateTime.MinValue;

		private string mLatestVersion;

		private bool mTakingScreenshot;

		private float mScreenshotStartGraphZoom;

		private Vector2 mScreenshotStartGraphOffset;

		private Texture2D mScreenshotTexture;

		private Rect mScreenshotGraphSize;

		private Vector2 mScreenshotGraphOffset;

		private string mScreenshotPath;

		public BehaviorDesignerWindow.TaskCallbackHandler onAddTask;

		public BehaviorDesignerWindow.TaskCallbackHandler onRemoveTask;

		private List<TaskSerializer> mCopiedTasks;

		public List<ErrorDetails> ErrorDetails
		{
			get
			{
				return this.mErrorDetails;
			}
		}

		public BehaviorSource ActiveBehaviorSource
		{
			get
			{
				return this.mActiveBehaviorSource;
			}
		}

		public int ActiveBehaviorID
		{
			get
			{
				return this.mActiveBehaviorID;
			}
		}

		private DateTime LastUpdateCheck
		{
			get
			{
				try
				{
					if (this.mLastUpdateCheck != DateTime.MinValue)
					{
						return this.mLastUpdateCheck;
					}
					this.mLastUpdateCheck = DateTime.Parse(EditorPrefs.GetString("BehaviorDesignerLastUpdateCheck", "1/1/1971 00:00:01"), CultureInfo.get_InvariantCulture());
				}
				catch (Exception)
				{
					this.mLastUpdateCheck = DateTime.get_UtcNow();
				}
				return this.mLastUpdateCheck;
			}
			set
			{
				this.mLastUpdateCheck = value;
				EditorPrefs.SetString("BehaviorDesignerLastUpdateCheck", this.mLastUpdateCheck.ToString(CultureInfo.get_InvariantCulture()));
			}
		}

		public string LatestVersion
		{
			get
			{
				if (!string.IsNullOrEmpty(this.mLatestVersion))
				{
					return this.mLatestVersion;
				}
				this.mLatestVersion = EditorPrefs.GetString("BehaviorDesignerLatestVersion", "1.5.11".ToString());
				return this.mLatestVersion;
			}
			set
			{
				this.mLatestVersion = value;
				EditorPrefs.SetString("BehaviorDesignerLatestVersion", this.mLatestVersion);
			}
		}

		public BehaviorDesignerWindow.TaskCallbackHandler OnAddTask
		{
			get
			{
				return this.onAddTask;
			}
			set
			{
				this.onAddTask = (BehaviorDesignerWindow.TaskCallbackHandler)Delegate.Combine(this.onAddTask, value);
			}
		}

		public BehaviorDesignerWindow.TaskCallbackHandler OnRemoveTask
		{
			get
			{
				return this.onRemoveTask;
			}
			set
			{
				this.onRemoveTask = (BehaviorDesignerWindow.TaskCallbackHandler)Delegate.Combine(this.onRemoveTask, value);
			}
		}

		[MenuItem("Tools/Behavior Designer/Editor", false, 0)]
		public static void ShowWindow()
		{
			BehaviorDesignerWindow window = EditorWindow.GetWindow<BehaviorDesignerWindow>(false, "Behavior Designer");
			window.set_wantsMouseMove(true);
			window.set_minSize(new Vector2(500f, 100f));
			window.Init();
			BehaviorDesignerPreferences.InitPrefernces();
			if (BehaviorDesignerPreferences.GetBool(BDPreferences.ShowWelcomeScreen))
			{
				WelcomeScreen.ShowWindow();
			}
		}

		public void OnEnable()
		{
			this.mIsPlaying = EditorApplication.get_isPlaying();
			this.mSizesInitialized = false;
			this.Repaint();
			EditorApplication.projectWindowChanged = (EditorApplication.CallbackFunction)Delegate.Combine(EditorApplication.projectWindowChanged, new EditorApplication.CallbackFunction(this.OnProjectWindowChange));
			EditorApplication.playmodeStateChanged = (EditorApplication.CallbackFunction)Delegate.Combine(EditorApplication.playmodeStateChanged, new EditorApplication.CallbackFunction(this.OnPlaymodeStateChange));
			Undo.undoRedoPerformed = (Undo.UndoRedoCallback)Delegate.Combine(Undo.undoRedoPerformed, new Undo.UndoRedoCallback(this.OnUndoRedo));
			this.Init();
			this.SetBehaviorManager();
		}

		public void OnFocus()
		{
			BehaviorDesignerWindow.instance = this;
			base.set_wantsMouseMove(true);
			this.Init();
			if (!this.mLockActiveGameObject)
			{
				this.mActiveObject = Selection.get_activeObject();
			}
			this.ReloadPreviousBehavior();
			this.UpdateGraphStatus();
		}

		public void OnSelectionChange()
		{
			if (!this.mLockActiveGameObject)
			{
				this.UpdateTree(false);
			}
			else
			{
				this.ReloadPreviousBehavior();
			}
			this.UpdateGraphStatus();
		}

		public void OnProjectWindowChange()
		{
			this.ReloadPreviousBehavior();
			this.ClearBreadcrumbMenu();
		}

		private void ReloadPreviousBehavior()
		{
			if (this.mActiveObject != null)
			{
				if (this.mActiveObject as GameObject)
				{
					GameObject gameObject = this.mActiveObject as GameObject;
					int num = -1;
					Behavior[] components = gameObject.GetComponents<Behavior>();
					for (int i = 0; i < components.Length; i++)
					{
						if (components[i].GetInstanceID() == this.mActiveBehaviorID)
						{
							num = i;
							break;
						}
					}
					if (num != -1)
					{
						this.LoadBehavior(components[num].GetBehaviorSource(), true, false);
					}
					else if (Enumerable.Count<Behavior>(components) > 0)
					{
						this.LoadBehavior(components[0].GetBehaviorSource(), true, false);
					}
					else if (this.mGraphDesigner != null)
					{
						this.ClearGraph();
					}
				}
				else if (this.mActiveObject is ExternalBehavior)
				{
					ExternalBehavior externalBehavior = this.mActiveObject as ExternalBehavior;
					BehaviorSource behaviorSource = externalBehavior.get_BehaviorSource();
					if (externalBehavior.get_BehaviorSource().get_Owner() == null)
					{
						externalBehavior.get_BehaviorSource().set_Owner(externalBehavior);
					}
					this.LoadBehavior(behaviorSource, true, false);
				}
				else if (this.mGraphDesigner != null)
				{
					this.mActiveObject = null;
					this.ClearGraph();
				}
			}
			else if (this.mGraphDesigner != null)
			{
				this.ClearGraph();
				this.Repaint();
			}
		}

		private void UpdateTree(bool firstLoad)
		{
			bool flag = firstLoad;
			if (Selection.get_activeObject() != null)
			{
				bool loadPrevBehavior = false;
				if (!Selection.get_activeObject().Equals(this.mActiveObject))
				{
					this.mActiveObject = Selection.get_activeObject();
					flag = true;
				}
				BehaviorSource behaviorSource = null;
				GameObject gameObject = this.mActiveObject as GameObject;
				if (gameObject != null && gameObject.GetComponent<Behavior>() != null)
				{
					if (flag)
					{
						if (this.mActiveObject.Equals(this.mPrevActiveObject) && this.mActiveBehaviorID != -1)
						{
							loadPrevBehavior = true;
							int num = -1;
							Behavior[] components = (this.mActiveObject as GameObject).GetComponents<Behavior>();
							for (int i = 0; i < components.Length; i++)
							{
								if (components[i].GetInstanceID() == this.mActiveBehaviorID)
								{
									num = i;
									break;
								}
							}
							if (num != -1)
							{
								behaviorSource = gameObject.GetComponents<Behavior>()[num].GetBehaviorSource();
							}
							else if (Enumerable.Count<Behavior>(components) > 0)
							{
								behaviorSource = gameObject.GetComponents<Behavior>()[0].GetBehaviorSource();
							}
						}
						else
						{
							behaviorSource = gameObject.GetComponents<Behavior>()[0].GetBehaviorSource();
						}
					}
					else
					{
						Behavior[] components2 = gameObject.GetComponents<Behavior>();
						bool flag2 = false;
						if (this.mActiveBehaviorSource != null)
						{
							for (int j = 0; j < components2.Length; j++)
							{
								if (components2[j].Equals(this.mActiveBehaviorSource.get_Owner()))
								{
									flag2 = true;
									break;
								}
							}
						}
						if (!flag2)
						{
							behaviorSource = gameObject.GetComponents<Behavior>()[0].GetBehaviorSource();
						}
						else
						{
							behaviorSource = this.mActiveBehaviorSource;
							loadPrevBehavior = true;
						}
					}
				}
				else if (this.mActiveObject is ExternalBehavior)
				{
					ExternalBehavior externalBehavior = this.mActiveObject as ExternalBehavior;
					if (externalBehavior.get_BehaviorSource().get_Owner() == null)
					{
						externalBehavior.get_BehaviorSource().set_Owner(externalBehavior);
					}
					if (flag && this.mActiveObject.Equals(this.mPrevActiveObject))
					{
						loadPrevBehavior = true;
					}
					behaviorSource = externalBehavior.get_BehaviorSource();
				}
				else
				{
					this.mPrevActiveObject = null;
				}
				if (behaviorSource != null)
				{
					this.LoadBehavior(behaviorSource, loadPrevBehavior, false);
				}
				else if (behaviorSource == null)
				{
					this.ClearGraph();
				}
			}
			else
			{
				if (this.mActiveObject != null && this.mActiveBehaviorSource != null)
				{
					this.mPrevActiveObject = this.mActiveObject;
				}
				this.mActiveObject = null;
				this.ClearGraph();
			}
		}

		private void Init()
		{
			if (this.mTaskList == null)
			{
				this.mTaskList = ScriptableObject.CreateInstance<TaskList>();
			}
			if (this.mVariableInspector == null)
			{
				this.mVariableInspector = ScriptableObject.CreateInstance<VariableInspector>();
			}
			if (this.mGraphDesigner == null)
			{
				this.mGraphDesigner = ScriptableObject.CreateInstance<GraphDesigner>();
			}
			if (this.mTaskInspector == null)
			{
				this.mTaskInspector = ScriptableObject.CreateInstance<TaskInspector>();
			}
			if (this.mGridMaterial == null)
			{
				this.mGridMaterial = new Material(Shader.Find("Hidden/Behavior Designer/Grid"));
				this.mGridMaterial.set_hideFlags(61);
				this.mGridMaterial.get_shader().set_hideFlags(61);
			}
			this.mTaskList.Init();
			FieldInspector.Init();
			this.ClearBreadcrumbMenu();
		}

		public void UpdateGraphStatus()
		{
			if (this.mActiveObject == null || this.mGraphDesigner == null || (this.mActiveObject as GameObject == null && this.mActiveObject as ExternalBehavior == null))
			{
				this.mGraphStatus = "Select a GameObject";
			}
			else if (this.mActiveObject as GameObject != null && object.ReferenceEquals((this.mActiveObject as GameObject).GetComponent<Behavior>(), null))
			{
				this.mGraphStatus = "Right Click, Add a Behavior Tree Component";
			}
			else if (this.ViewOnlyMode() && this.mActiveBehaviorSource != null)
			{
				ExternalBehavior externalBehavior = (this.mActiveBehaviorSource.get_Owner().GetObject() as Behavior).get_ExternalBehavior();
				if (externalBehavior != null)
				{
					this.mGraphStatus = externalBehavior.get_BehaviorSource().ToString() + " (View Only Mode)";
				}
				else
				{
					this.mGraphStatus = this.mActiveBehaviorSource.ToString() + " (View Only Mode)";
				}
			}
			else if (!this.mGraphDesigner.HasEntryNode())
			{
				this.mGraphStatus = "Add a Task";
			}
			else if (this.IsReferencingTasks())
			{
				this.mGraphStatus = "Select tasks to reference (right click to exit)";
			}
			else if (this.mActiveBehaviorSource != null && this.mActiveBehaviorSource.get_Owner() != null && this.mActiveBehaviorSource.get_Owner().GetObject() != null)
			{
				if (this.mExternalParent != null)
				{
					this.mGraphStatus = this.mExternalParent.ToString() + " (Editing External Behavior)";
				}
				else
				{
					this.mGraphStatus = this.mActiveBehaviorSource.ToString();
				}
			}
		}

		private void BuildBreadcrumbMenus(BehaviorDesignerWindow.BreadcrumbMenuType menuType)
		{
			Dictionary<BehaviorSource, string> dictionary = new Dictionary<BehaviorSource, string>();
			Dictionary<string, int> dictionary2 = new Dictionary<string, int>();
			HashSet<Object> hashSet = new HashSet<Object>();
			List<BehaviorSource> list = new List<BehaviorSource>();
			Behavior[] array = Resources.FindObjectsOfTypeAll(typeof(Behavior)) as Behavior[];
			for (int i = array.Length - 1; i > -1; i--)
			{
				BehaviorSource behaviorSource = array[i].GetBehaviorSource();
				if (behaviorSource.get_Owner() == null)
				{
					behaviorSource.set_Owner(array[i]);
				}
				list.Add(behaviorSource);
			}
			ExternalBehavior[] array2 = Resources.FindObjectsOfTypeAll(typeof(ExternalBehavior)) as ExternalBehavior[];
			for (int j = array2.Length - 1; j > -1; j--)
			{
				BehaviorSource behaviorSource2 = array2[j].GetBehaviorSource();
				if (behaviorSource2.get_Owner() == null)
				{
					behaviorSource2.set_Owner(array2[j]);
				}
				list.Add(behaviorSource2);
			}
			list.Sort(new AlphanumComparator<BehaviorSource>());
			int k = 0;
			while (k < list.get_Count())
			{
				Object @object = list.get_Item(k).get_Owner().GetObject();
				if (menuType != BehaviorDesignerWindow.BreadcrumbMenuType.Behavior)
				{
					goto IL_14E;
				}
				if (@object is Behavior)
				{
					if ((@object as Behavior).get_gameObject().Equals(this.mActiveObject))
					{
						goto IL_14E;
					}
				}
				else if ((@object as ExternalBehavior).Equals(this.mActiveObject))
				{
					goto IL_14E;
				}
				IL_29B:
				k++;
				continue;
				IL_14E:
				if (menuType == BehaviorDesignerWindow.BreadcrumbMenuType.GameObject && @object is Behavior)
				{
					if (hashSet.Contains((@object as Behavior).get_gameObject()))
					{
						goto IL_29B;
					}
					hashSet.Add((@object as Behavior).get_gameObject());
				}
				string text = string.Empty;
				if (@object is Behavior)
				{
					switch (menuType)
					{
					case BehaviorDesignerWindow.BreadcrumbMenuType.GameObjectBehavior:
						text = list.get_Item(k).ToString();
						break;
					case BehaviorDesignerWindow.BreadcrumbMenuType.GameObject:
						text = (@object as Behavior).get_gameObject().get_name();
						break;
					case BehaviorDesignerWindow.BreadcrumbMenuType.Behavior:
						text = list.get_Item(k).behaviorName;
						break;
					}
					if (!AssetDatabase.GetAssetPath(@object).Equals(string.Empty))
					{
						text += " (prefab)";
					}
				}
				else
				{
					text = list.get_Item(k).ToString() + " (external)";
				}
				int num = 0;
				if (dictionary2.TryGetValue(text, ref num))
				{
					dictionary2.set_Item(text, ++num);
					text += string.Format(" ({0})", num + 1);
				}
				else
				{
					dictionary2.Add(text, 0);
				}
				dictionary.Add(list.get_Item(k), text);
				goto IL_29B;
			}
			switch (menuType)
			{
			case BehaviorDesignerWindow.BreadcrumbMenuType.GameObjectBehavior:
				this.mBreadcrumbGameObjectBehaviorMenu = new GenericMenu();
				break;
			case BehaviorDesignerWindow.BreadcrumbMenuType.GameObject:
				this.mBreadcrumbGameObjectMenu = new GenericMenu();
				break;
			case BehaviorDesignerWindow.BreadcrumbMenuType.Behavior:
				this.mBreadcrumbBehaviorMenu = new GenericMenu();
				break;
			}
			using (Dictionary<BehaviorSource, string>.Enumerator enumerator = dictionary.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					KeyValuePair<BehaviorSource, string> current = enumerator.get_Current();
					switch (menuType)
					{
					case BehaviorDesignerWindow.BreadcrumbMenuType.GameObjectBehavior:
						this.mBreadcrumbGameObjectBehaviorMenu.AddItem(new GUIContent(current.get_Value()), current.get_Key().Equals(this.mActiveBehaviorSource), new GenericMenu.MenuFunction2(this.BehaviorSelectionCallback), current.get_Key());
						break;
					case BehaviorDesignerWindow.BreadcrumbMenuType.GameObject:
					{
						bool flag;
						if (current.get_Key().get_Owner().GetObject() is ExternalBehavior)
						{
							flag = (current.get_Key().get_Owner().GetObject() as ExternalBehavior).GetObject().Equals(this.mActiveObject);
						}
						else
						{
							flag = (current.get_Key().get_Owner().GetObject() as Behavior).get_gameObject().Equals(this.mActiveObject);
						}
						this.mBreadcrumbGameObjectMenu.AddItem(new GUIContent(current.get_Value()), flag, new GenericMenu.MenuFunction2(this.BehaviorSelectionCallback), current.get_Key());
						break;
					}
					case BehaviorDesignerWindow.BreadcrumbMenuType.Behavior:
						this.mBreadcrumbBehaviorMenu.AddItem(new GUIContent(current.get_Value()), current.get_Key().Equals(this.mActiveBehaviorSource), new GenericMenu.MenuFunction2(this.BehaviorSelectionCallback), current.get_Key());
						break;
					}
				}
			}
		}

		private void ClearBreadcrumbMenu()
		{
			this.mBreadcrumbGameObjectBehaviorMenu = null;
			this.mBreadcrumbGameObjectMenu = null;
			this.mBreadcrumbBehaviorMenu = null;
		}

		private void BuildRightClickMenu(NodeDesigner clickedNode)
		{
			if (this.mActiveObject == null)
			{
				return;
			}
			this.mRightClickMenu = new GenericMenu();
			this.mShowRightClickMenu = true;
			if (clickedNode == null && !EditorApplication.get_isPlaying() && !this.ViewOnlyMode())
			{
				this.mTaskList.AddTasksToMenu(ref this.mRightClickMenu, null, "Add Task", new GenericMenu.MenuFunction2(this.AddTaskCallback));
				if (this.mCopiedTasks != null && this.mCopiedTasks.get_Count() > 0)
				{
					this.mRightClickMenu.AddItem(new GUIContent("Paste Tasks"), false, new GenericMenu.MenuFunction(this.PasteNodes));
				}
				else
				{
					this.mRightClickMenu.AddDisabledItem(new GUIContent("Paste Tasks"));
				}
			}
			if (clickedNode != null && !clickedNode.IsEntryDisplay)
			{
				if (this.mGraphDesigner.SelectedNodes.get_Count() == 1)
				{
					this.mRightClickMenu.AddItem(new GUIContent("Edit Script"), false, new GenericMenu.MenuFunction2(this.OpenInFileEditor), clickedNode);
					this.mRightClickMenu.AddItem(new GUIContent("Locate Script"), false, new GenericMenu.MenuFunction2(this.SelectInProject), clickedNode);
					if (!this.ViewOnlyMode())
					{
						this.mRightClickMenu.AddItem(new GUIContent((!clickedNode.Task.get_Disabled()) ? "Disable" : "Enable"), false, new GenericMenu.MenuFunction2(this.ToggleEnableState), clickedNode);
						if (clickedNode.IsParent)
						{
							this.mRightClickMenu.AddItem(new GUIContent((!clickedNode.Task.get_NodeData().get_Collapsed()) ? "Collapse" : "Expand"), false, new GenericMenu.MenuFunction2(this.ToggleCollapseState), clickedNode);
						}
						this.mRightClickMenu.AddItem(new GUIContent((!clickedNode.Task.get_NodeData().get_IsBreakpoint()) ? "Set Breakpoint" : "Remove Breakpoint"), false, new GenericMenu.MenuFunction2(this.ToggleBreakpoint), clickedNode);
						this.mTaskList.AddTasksToMenu(ref this.mRightClickMenu, this.mGraphDesigner.SelectedNodes.get_Item(0).Task.GetType(), "Replace", new GenericMenu.MenuFunction2(this.ReplaceTaskCallback));
					}
				}
				if (!EditorApplication.get_isPlaying() && !this.ViewOnlyMode())
				{
					this.mRightClickMenu.AddItem(new GUIContent(string.Format("Copy Task{0}", (this.mGraphDesigner.SelectedNodes.get_Count() <= 1) ? string.Empty : "s")), false, new GenericMenu.MenuFunction(this.CopyNodes));
					if (this.mCopiedTasks != null && this.mCopiedTasks.get_Count() > 0)
					{
						this.mRightClickMenu.AddItem(new GUIContent(string.Format("Paste Task{0}", (this.mCopiedTasks.get_Count() <= 1) ? string.Empty : "s")), false, new GenericMenu.MenuFunction(this.PasteNodes));
					}
					else
					{
						this.mRightClickMenu.AddDisabledItem(new GUIContent("Paste Tasks"));
					}
					this.mRightClickMenu.AddItem(new GUIContent(string.Format("Delete Task{0}", (this.mGraphDesigner.SelectedNodes.get_Count() <= 1) ? string.Empty : "s")), false, new GenericMenu.MenuFunction(this.DeleteNodes));
				}
			}
			if (!EditorApplication.get_isPlaying() && this.mActiveObject as GameObject != null)
			{
				if (clickedNode != null && !clickedNode.IsEntryDisplay)
				{
					this.mRightClickMenu.AddSeparator(string.Empty);
				}
				this.mRightClickMenu.AddItem(new GUIContent("Add Behavior Tree"), false, new GenericMenu.MenuFunction(this.AddBehavior));
				if (this.mActiveBehaviorSource != null)
				{
					this.mRightClickMenu.AddItem(new GUIContent("Remove Behavior Tree"), false, new GenericMenu.MenuFunction(this.RemoveBehavior));
					this.mRightClickMenu.AddItem(new GUIContent("Save As External Behavior Tree"), false, new GenericMenu.MenuFunction(this.SaveAsAsset));
				}
			}
		}

		public void Update()
		{
			if (this.mTakingScreenshot)
			{
				this.Repaint();
			}
		}

		public void OnGUI()
		{
			this.mCurrentMousePosition = Event.get_current().get_mousePosition();
			this.SetupSizes();
			if (!this.mSizesInitialized)
			{
				this.mSizesInitialized = true;
				if (!this.mLockActiveGameObject || this.mActiveObject == null)
				{
					this.UpdateTree(true);
				}
				else
				{
					this.ReloadPreviousBehavior();
				}
			}
			if (this.Draw() && this.mGUITickCount > 1)
			{
				this.Repaint();
				this.mGUITickCount = 0;
			}
			this.HandleEvents();
			this.mGUITickCount++;
		}

		public void OnPlaymodeStateChange()
		{
			if (EditorApplication.get_isPlaying() && !EditorApplication.get_isPaused())
			{
				if (this.mBehaviorManager == null)
				{
					this.SetBehaviorManager();
					if (this.mBehaviorManager == null)
					{
						return;
					}
				}
				if (this.mBehaviorManager.get_BreakpointTree() != null && this.mEditorAtBreakpoint)
				{
					this.mEditorAtBreakpoint = false;
					this.mBehaviorManager.set_BreakpointTree(null);
				}
			}
			else if (EditorApplication.get_isPlaying() && EditorApplication.get_isPaused())
			{
				if (this.mBehaviorManager != null && this.mBehaviorManager.get_BreakpointTree() != null)
				{
					if (!this.mEditorAtBreakpoint)
					{
						this.mEditorAtBreakpoint = true;
						if (BehaviorDesignerPreferences.GetBool(BDPreferences.SelectOnBreakpoint) && !this.mLockActiveGameObject)
						{
							Selection.set_activeObject(this.mBehaviorManager.get_BreakpointTree());
							this.LoadBehavior(this.mBehaviorManager.get_BreakpointTree().GetBehaviorSource(), this.mActiveBehaviorSource == this.mBehaviorManager.get_BreakpointTree().GetBehaviorSource(), false);
						}
					}
					else
					{
						this.mEditorAtBreakpoint = false;
						this.mBehaviorManager.set_BreakpointTree(null);
					}
				}
			}
			else if (!EditorApplication.get_isPlaying())
			{
				this.mBehaviorManager = null;
			}
		}

		private void SetBehaviorManager()
		{
			this.mBehaviorManager = BehaviorManager.instance;
			if (this.mBehaviorManager == null)
			{
				return;
			}
			BehaviorManager expr_23 = this.mBehaviorManager;
			expr_23.set_OnTaskBreakpoint((BehaviorManager.BehaviorManagerHandler)Delegate.Combine(expr_23.get_OnTaskBreakpoint(), new BehaviorManager.BehaviorManagerHandler(this.OnTaskBreakpoint)));
			this.mUpdateNodeTaskMap = true;
		}

		public void OnTaskBreakpoint()
		{
			EditorApplication.set_isPaused(true);
			this.Repaint();
		}

		private void OnPreferenceChange(BDPreferences pref, object value)
		{
			switch (pref)
			{
			case BDPreferences.CompactMode:
				this.mGraphDesigner.GraphDirty();
				return;
			case BDPreferences.SnapToGrid:
			case BDPreferences.ShowTaskDescription:
				IL_1F:
				if (pref != BDPreferences.ShowSceneIcon && pref != BDPreferences.GizmosViewMode)
				{
					return;
				}
				GizmoManager.UpdateAllGizmos();
				return;
			case BDPreferences.BinarySerialization:
				this.SaveBehavior();
				return;
			case BDPreferences.ErrorChecking:
				this.CheckForErrors();
				return;
			}
			goto IL_1F;
		}

		public void OnInspectorUpdate()
		{
			if (this.mStepApplication)
			{
				EditorApplication.Step();
				this.mStepApplication = false;
			}
			if (EditorApplication.get_isPlaying() && !EditorApplication.get_isPaused() && this.mActiveBehaviorSource != null && this.mBehaviorManager != null)
			{
				if (this.mUpdateNodeTaskMap)
				{
					this.UpdateNodeTaskMap();
				}
				if (this.mBehaviorManager.get_BreakpointTree() != null)
				{
					this.mBehaviorManager.set_BreakpointTree(null);
				}
				this.Repaint();
			}
			if (Application.get_isPlaying() && this.mBehaviorManager == null)
			{
				this.SetBehaviorManager();
			}
			if (this.mBehaviorManager != null && this.mBehaviorManager.get_Dirty())
			{
				if (this.mActiveBehaviorSource != null)
				{
					this.LoadBehavior(this.mActiveBehaviorSource, true, false);
				}
				this.mBehaviorManager.set_Dirty(false);
			}
			if (!EditorApplication.get_isPlaying() && this.mIsPlaying)
			{
				this.ReloadPreviousBehavior();
			}
			this.mIsPlaying = EditorApplication.get_isPlaying();
			this.UpdateGraphStatus();
			this.UpdateCheck();
		}

		private void UpdateNodeTaskMap()
		{
			if (this.mUpdateNodeTaskMap && this.mBehaviorManager != null)
			{
				Behavior behavior = this.mActiveBehaviorSource.get_Owner() as Behavior;
				List<Task> taskList = this.mBehaviorManager.GetTaskList(behavior);
				if (taskList != null)
				{
					this.mNodeDesignerTaskMap = new Dictionary<NodeDesigner, Task>();
					for (int i = 0; i < taskList.get_Count(); i++)
					{
						NodeDesigner nodeDesigner = taskList.get_Item(i).get_NodeData().get_NodeDesigner() as NodeDesigner;
						if (nodeDesigner != null && !this.mNodeDesignerTaskMap.ContainsKey(nodeDesigner))
						{
							this.mNodeDesignerTaskMap.Add(nodeDesigner, taskList.get_Item(i));
						}
					}
					this.mUpdateNodeTaskMap = false;
				}
			}
		}

		private bool Draw()
		{
			bool result = false;
			Color color = GUI.get_color();
			Color backgroundColor = GUI.get_backgroundColor();
			GUI.set_color(Color.get_white());
			GUI.set_backgroundColor(Color.get_white());
			this.DrawFileToolbar();
			this.DrawDebugToolbar();
			this.DrawPropertiesBox();
			if (this.DrawGraphArea())
			{
				result = true;
			}
			this.DrawPreferencesPane();
			if (this.mTakingScreenshot)
			{
				GUI.DrawTexture(new Rect(0f, 0f, base.get_position().get_width(), base.get_position().get_height() + 22f), BehaviorDesignerUtility.ScreenshotBackgroundTexture, 0, false);
			}
			GUI.set_color(color);
			GUI.set_backgroundColor(backgroundColor);
			return result;
		}

		private void DrawFileToolbar()
		{
			GUILayout.BeginArea(this.mFileToolBarRect, EditorStyles.get_toolbar());
			GUILayout.BeginHorizontal(new GUILayoutOption[0]);
			if (GUILayout.Button(BehaviorDesignerUtility.HistoryBackwardTexture, EditorStyles.get_toolbarButton(), new GUILayoutOption[0]) && (this.mBehaviorSourceHistoryIndex > 0 || (this.mActiveBehaviorSource == null && this.mBehaviorSourceHistoryIndex == 0)))
			{
				BehaviorSource behaviorSource = null;
				if (this.mActiveBehaviorSource == null)
				{
					this.mBehaviorSourceHistoryIndex++;
				}
				while (behaviorSource == null && this.mBehaviorSourceHistory.get_Count() > 0 && this.mBehaviorSourceHistoryIndex > 0)
				{
					this.mBehaviorSourceHistoryIndex--;
					behaviorSource = this.BehaviorSourceFromIBehaviorHistory(this.mBehaviorSourceHistory.get_Item(this.mBehaviorSourceHistoryIndex) as IBehavior);
					if (behaviorSource == null || behaviorSource.get_Owner() == null || behaviorSource.get_Owner().GetObject() == null)
					{
						this.mBehaviorSourceHistory.RemoveAt(this.mBehaviorSourceHistoryIndex);
						behaviorSource = null;
					}
				}
				if (behaviorSource != null)
				{
					this.LoadBehavior(behaviorSource, false);
				}
			}
			if (GUILayout.Button(BehaviorDesignerUtility.HistoryForwardTexture, EditorStyles.get_toolbarButton(), new GUILayoutOption[0]))
			{
				BehaviorSource behaviorSource2 = null;
				if (this.mBehaviorSourceHistoryIndex < this.mBehaviorSourceHistory.get_Count() - 1)
				{
					this.mBehaviorSourceHistoryIndex++;
					while (behaviorSource2 == null && this.mBehaviorSourceHistoryIndex < this.mBehaviorSourceHistory.get_Count() && this.mBehaviorSourceHistoryIndex > 0)
					{
						behaviorSource2 = this.BehaviorSourceFromIBehaviorHistory(this.mBehaviorSourceHistory.get_Item(this.mBehaviorSourceHistoryIndex) as IBehavior);
						if (behaviorSource2 == null || behaviorSource2.get_Owner() == null || behaviorSource2.get_Owner().GetObject() == null)
						{
							this.mBehaviorSourceHistory.RemoveAt(this.mBehaviorSourceHistoryIndex);
							behaviorSource2 = null;
						}
					}
				}
				if (behaviorSource2 != null)
				{
					this.LoadBehavior(behaviorSource2, false);
				}
			}
			if (GUILayout.Button("...", EditorStyles.get_toolbarButton(), new GUILayoutOption[]
			{
				GUILayout.Width(22f)
			}))
			{
				this.BuildBreadcrumbMenus(BehaviorDesignerWindow.BreadcrumbMenuType.GameObjectBehavior);
				this.mBreadcrumbGameObjectBehaviorMenu.ShowAsContext();
			}
			string text = (!(this.mActiveObject as GameObject != null) && !(this.mActiveObject as ExternalBehavior != null)) ? "(None Selected)" : this.mActiveObject.get_name();
			if (GUILayout.Button(text, EditorStyles.get_toolbarPopup(), new GUILayoutOption[]
			{
				GUILayout.Width(140f)
			}))
			{
				this.BuildBreadcrumbMenus(BehaviorDesignerWindow.BreadcrumbMenuType.GameObject);
				this.mBreadcrumbGameObjectMenu.ShowAsContext();
			}
			string text2 = (this.mActiveBehaviorSource == null) ? "(None Selected)" : this.mActiveBehaviorSource.behaviorName;
			if (GUILayout.Button(text2, EditorStyles.get_toolbarPopup(), new GUILayoutOption[]
			{
				GUILayout.Width(140f)
			}) && this.mActiveBehaviorSource != null)
			{
				this.BuildBreadcrumbMenus(BehaviorDesignerWindow.BreadcrumbMenuType.Behavior);
				this.mBreadcrumbBehaviorMenu.ShowAsContext();
			}
			if (GUILayout.Button("Referenced Behaviors", EditorStyles.get_toolbarPopup(), new GUILayoutOption[]
			{
				GUILayout.Width(140f)
			}) && this.mActiveBehaviorSource != null)
			{
				List<BehaviorSource> list = this.mGraphDesigner.FindReferencedBehaviors();
				if (list.get_Count() > 0)
				{
					list.Sort(new AlphanumComparator<BehaviorSource>());
					this.mReferencedBehaviorsMenu = new GenericMenu();
					for (int i = 0; i < list.get_Count(); i++)
					{
						this.mReferencedBehaviorsMenu.AddItem(new GUIContent(list.get_Item(i).ToString()), false, new GenericMenu.MenuFunction2(this.BehaviorSelectionCallback), list.get_Item(i));
					}
					this.mReferencedBehaviorsMenu.ShowAsContext();
				}
			}
			if (GUILayout.Button("-", EditorStyles.get_toolbarButton(), new GUILayoutOption[]
			{
				GUILayout.Width(22f)
			}))
			{
				if (this.mActiveBehaviorSource != null)
				{
					this.RemoveBehavior();
				}
				else
				{
					EditorUtility.DisplayDialog("Unable to Remove Behavior Tree", "No behavior tree selected.", "OK");
				}
			}
			if (GUILayout.Button("+", EditorStyles.get_toolbarButton(), new GUILayoutOption[]
			{
				GUILayout.Width(22f)
			}))
			{
				if (this.mActiveObject != null)
				{
					this.AddBehavior();
				}
				else
				{
					EditorUtility.DisplayDialog("Unable to Add Behavior Tree", "No GameObject is selected.", "OK");
				}
			}
			if (GUILayout.Button("Lock", (!this.mLockActiveGameObject) ? EditorStyles.get_toolbarButton() : BehaviorDesignerUtility.ToolbarButtonSelectionGUIStyle, new GUILayoutOption[]
			{
				GUILayout.Width(42f)
			}))
			{
				if (this.mActiveObject != null)
				{
					this.mLockActiveGameObject = !this.mLockActiveGameObject;
					if (!this.mLockActiveGameObject)
					{
						this.UpdateTree(false);
					}
				}
				else if (this.mLockActiveGameObject)
				{
					this.mLockActiveGameObject = false;
				}
				else
				{
					EditorUtility.DisplayDialog("Unable to Lock GameObject", "No GameObject is selected.", "OK");
				}
			}
			GUI.set_enabled(this.mActiveBehaviorSource == null || this.mExternalParent == null);
			if (GUILayout.Button("Export", EditorStyles.get_toolbarButton(), new GUILayoutOption[]
			{
				GUILayout.Width(46f)
			}))
			{
				if (this.mActiveBehaviorSource != null)
				{
					if (this.mActiveBehaviorSource.get_Owner().GetObject() as Behavior)
					{
						this.SaveAsAsset();
					}
					else
					{
						this.SaveAsPrefab();
					}
				}
				else
				{
					EditorUtility.DisplayDialog("Unable to Save Behavior Tree", "Select a behavior tree from within the scene.", "OK");
				}
			}
			GUI.set_enabled(true);
			if (GUILayout.Button("Take Screenshot", EditorStyles.get_toolbarButton(), new GUILayoutOption[]
			{
				GUILayout.Width(96f)
			}))
			{
				if (this.mActiveBehaviorSource != null)
				{
					this.TakeScreenshot();
				}
				else
				{
					EditorUtility.DisplayDialog("Unable to Take Screenshot", "Select a behavior tree from within the scene.", "OK");
				}
			}
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Preferences", (!this.mShowPrefPane) ? EditorStyles.get_toolbarButton() : BehaviorDesignerUtility.ToolbarButtonSelectionGUIStyle, new GUILayoutOption[]
			{
				GUILayout.Width(80f)
			}))
			{
				this.mShowPrefPane = !this.mShowPrefPane;
			}
			GUILayout.EndVertical();
			GUILayout.EndArea();
		}

		private void DrawDebugToolbar()
		{
			GUILayout.BeginArea(this.mDebugToolBarRect, EditorStyles.get_toolbar());
			GUILayout.BeginHorizontal(new GUILayoutOption[0]);
			if (GUILayout.Button(BehaviorDesignerUtility.PlayTexture, (!EditorApplication.get_isPlaying()) ? EditorStyles.get_toolbarButton() : BehaviorDesignerUtility.ToolbarButtonSelectionGUIStyle, new GUILayoutOption[]
			{
				GUILayout.Width(40f)
			}))
			{
				EditorApplication.set_isPlaying(!EditorApplication.get_isPlaying());
			}
			if (GUILayout.Button(BehaviorDesignerUtility.PauseTexture, (!EditorApplication.get_isPaused()) ? EditorStyles.get_toolbarButton() : BehaviorDesignerUtility.ToolbarButtonSelectionGUIStyle, new GUILayoutOption[]
			{
				GUILayout.Width(40f)
			}))
			{
				EditorApplication.set_isPaused(!EditorApplication.get_isPaused());
			}
			if (GUILayout.Button(BehaviorDesignerUtility.StepTexture, EditorStyles.get_toolbarButton(), new GUILayoutOption[]
			{
				GUILayout.Width(40f)
			}) && EditorApplication.get_isPlaying())
			{
				this.mStepApplication = true;
			}
			if (this.mErrorDetails != null && this.mErrorDetails.get_Count() > 0 && GUILayout.Button(new GUIContent(this.mErrorDetails.get_Count() + " Error" + ((this.mErrorDetails.get_Count() <= 1) ? string.Empty : "s"), BehaviorDesignerUtility.SmallErrorIconTexture), BehaviorDesignerUtility.ToolbarButtonLeftAlignGUIStyle, new GUILayoutOption[]
			{
				GUILayout.Width(85f)
			}))
			{
				ErrorWindow.ShowWindow();
			}
			GUILayout.FlexibleSpace();
			Version version = new Version("1.5.11");
			if (version.CompareTo(new Version(this.LatestVersion)) < 0)
			{
				GUILayout.Label("Behavior Designer " + this.LatestVersion + " is now available.", BehaviorDesignerUtility.ToolbarLabelGUIStyle, new GUILayoutOption[0]);
			}
			GUILayout.EndHorizontal();
			GUILayout.EndArea();
		}

		private void DrawPreferencesPane()
		{
			if (this.mShowPrefPane)
			{
				GUILayout.BeginArea(this.mPreferencesPaneRect, BehaviorDesignerUtility.PreferencesPaneGUIStyle);
				BehaviorDesignerPreferences.DrawPreferencesPane(new PreferenceChangeHandler(this.OnPreferenceChange));
				GUILayout.EndArea();
			}
		}

		private void DrawPropertiesBox()
		{
			GUILayout.BeginArea(this.mPropertyToolbarRect, EditorStyles.get_toolbar());
			int num = this.mBehaviorToolbarSelection;
			this.mBehaviorToolbarSelection = GUILayout.Toolbar(this.mBehaviorToolbarSelection, this.mBehaviorToolbarStrings, EditorStyles.get_toolbarButton(), new GUILayoutOption[0]);
			GUILayout.EndArea();
			GUILayout.BeginArea(this.mPropertyBoxRect, BehaviorDesignerUtility.PropertyBoxGUIStyle);
			if (this.mBehaviorToolbarSelection == 0)
			{
				if (this.mActiveBehaviorSource != null)
				{
					GUILayout.Space(3f);
					BehaviorSource behaviorSource = (this.mExternalParent == null) ? this.mActiveBehaviorSource : this.mExternalParent;
					if (behaviorSource.get_Owner() as Behavior != null)
					{
						bool flag = false;
						bool flag2 = false;
						if (BehaviorInspector.DrawInspectorGUI(behaviorSource.get_Owner() as Behavior, new SerializedObject(behaviorSource.get_Owner() as Behavior), false, ref flag, ref flag2, ref flag2))
						{
							BehaviorDesignerUtility.SetObjectDirty(behaviorSource.get_Owner().GetObject());
							if (flag)
							{
								this.LoadBehavior(behaviorSource, false, false);
							}
						}
					}
					else
					{
						bool flag3 = false;
						ExternalBehaviorInspector.DrawInspectorGUI(behaviorSource, false, ref flag3);
					}
				}
				else
				{
					GUILayout.Space(5f);
					GUILayout.Label("No behavior tree selected. Create a new behavior tree or select one from the hierarchy.", BehaviorDesignerUtility.LabelWrapGUIStyle, new GUILayoutOption[]
					{
						GUILayout.Width(285f)
					});
				}
			}
			else if (this.mBehaviorToolbarSelection == 1)
			{
				this.mTaskList.DrawTaskList(this, !this.ViewOnlyMode());
				if (num != 1)
				{
					this.mTaskList.FocusSearchField();
				}
			}
			else if (this.mBehaviorToolbarSelection == 2)
			{
				if (this.mActiveBehaviorSource != null)
				{
					BehaviorSource behaviorSource2 = (this.mExternalParent == null) ? this.mActiveBehaviorSource : this.mExternalParent;
					if (this.mVariableInspector.DrawVariables(behaviorSource2))
					{
						this.SaveBehavior();
					}
					if (num != 2)
					{
						this.mVariableInspector.FocusNameField();
					}
				}
				else
				{
					GUILayout.Space(5f);
					GUILayout.Label("No behavior tree selected. Create a new behavior tree or select one from the hierarchy.", BehaviorDesignerUtility.LabelWrapGUIStyle, new GUILayoutOption[]
					{
						GUILayout.Width(285f)
					});
				}
			}
			else if (this.mBehaviorToolbarSelection == 3)
			{
				if (this.mGraphDesigner.SelectedNodes.get_Count() == 1 && !this.mGraphDesigner.SelectedNodes.get_Item(0).IsEntryDisplay)
				{
					Task task = this.mGraphDesigner.SelectedNodes.get_Item(0).Task;
					if (this.mNodeDesignerTaskMap != null && this.mNodeDesignerTaskMap.get_Count() > 0)
					{
						NodeDesigner nodeDesigner = this.mGraphDesigner.SelectedNodes.get_Item(0).Task.get_NodeData().get_NodeDesigner() as NodeDesigner;
						if (nodeDesigner != null && this.mNodeDesignerTaskMap.ContainsKey(nodeDesigner))
						{
							task = this.mNodeDesignerTaskMap.get_Item(nodeDesigner);
						}
					}
					if (this.mTaskInspector.DrawTaskInspector(this.mActiveBehaviorSource, this.mTaskList, task, !this.ViewOnlyMode()) && !Application.get_isPlaying())
					{
						this.SaveBehavior();
					}
				}
				else
				{
					GUILayout.Space(5f);
					if (this.mGraphDesigner.SelectedNodes.get_Count() > 1)
					{
						GUILayout.Label("Only one task can be selected at a time to\n view its properties.", BehaviorDesignerUtility.LabelWrapGUIStyle, new GUILayoutOption[]
						{
							GUILayout.Width(285f)
						});
					}
					else
					{
						GUILayout.Label("Select a task from the tree to\nview its properties.", BehaviorDesignerUtility.LabelWrapGUIStyle, new GUILayoutOption[]
						{
							GUILayout.Width(285f)
						});
					}
				}
			}
			GUILayout.EndArea();
		}

		private bool DrawGraphArea()
		{
			if (Event.get_current().get_type() != 6 && !this.mTakingScreenshot)
			{
				Vector2 vector = GUI.BeginScrollView(new Rect(this.mGraphRect.get_x(), this.mGraphRect.get_y(), this.mGraphRect.get_width() + 15f, this.mGraphRect.get_height() + 15f), this.mGraphScrollPosition, new Rect(0f, 0f, this.mGraphScrollSize.x, this.mGraphScrollSize.y), true, true);
				if (vector != this.mGraphScrollPosition && Event.get_current().get_type() != 9 && Event.get_current().get_type() != 11)
				{
					this.mGraphOffset -= (vector - this.mGraphScrollPosition) / this.mGraphZoom;
					this.mGraphScrollPosition = vector;
					this.mGraphDesigner.GraphDirty();
				}
				GUI.EndScrollView();
			}
			GUI.Box(this.mGraphRect, string.Empty, BehaviorDesignerUtility.GraphBackgroundGUIStyle);
			this.DrawGrid();
			EditorZoomArea.Begin(this.mGraphRect, this.mGraphZoom);
			Vector2 mousePosition;
			if (!this.GetMousePositionInGraph(out mousePosition))
			{
				mousePosition = new Vector2(-1f, -1f);
			}
			bool result = false;
			if (this.mGraphDesigner != null && this.mGraphDesigner.DrawNodes(mousePosition, this.mGraphOffset))
			{
				result = true;
			}
			if (this.mTakingScreenshot && Event.get_current().get_type() == 7)
			{
				this.RenderScreenshotTile();
			}
			if (this.mIsSelecting)
			{
				GUI.Box(this.GetSelectionArea(), string.Empty, BehaviorDesignerUtility.SelectionGUIStyle);
			}
			EditorZoomArea.End();
			this.DrawGraphStatus();
			this.DrawSelectedTaskDescription();
			return result;
		}

		private void DrawGrid()
		{
			if (!BehaviorDesignerPreferences.GetBool(BDPreferences.SnapToGrid))
			{
				return;
			}
			this.mGridMaterial.SetPass((!EditorGUIUtility.get_isProSkin()) ? 1 : 0);
			GL.PushMatrix();
			GL.Begin(1);
			this.DrawGridLines(10f * this.mGraphZoom, new Vector2(this.mGraphOffset.x % 10f * this.mGraphZoom, this.mGraphOffset.y % 10f * this.mGraphZoom));
			GL.End();
			GL.PopMatrix();
			this.mGridMaterial.SetPass((!EditorGUIUtility.get_isProSkin()) ? 3 : 2);
			GL.PushMatrix();
			GL.Begin(1);
			this.DrawGridLines(50f * this.mGraphZoom, new Vector2(this.mGraphOffset.x % 50f * this.mGraphZoom, this.mGraphOffset.y % 50f * this.mGraphZoom));
			GL.End();
			GL.PopMatrix();
		}

		private void DrawGridLines(float gridSize, Vector2 offset)
		{
			float num = this.mGraphRect.get_x() + offset.x;
			if (offset.x < 0f)
			{
				num += gridSize;
			}
			for (float num2 = num; num2 < this.mGraphRect.get_x() + this.mGraphRect.get_width(); num2 += gridSize)
			{
				this.DrawLine(new Vector2(num2, this.mGraphRect.get_y()), new Vector2(num2, this.mGraphRect.get_y() + this.mGraphRect.get_height()));
			}
			float num3 = this.mGraphRect.get_y() + offset.y;
			if (offset.y < 0f)
			{
				num3 += gridSize;
			}
			for (float num4 = num3; num4 < this.mGraphRect.get_y() + this.mGraphRect.get_height(); num4 += gridSize)
			{
				this.DrawLine(new Vector2(this.mGraphRect.get_x(), num4), new Vector2(this.mGraphRect.get_x() + this.mGraphRect.get_width(), num4));
			}
		}

		private void DrawLine(Vector2 p1, Vector2 p2)
		{
			GL.Vertex(p1);
			GL.Vertex(p2);
		}

		private void DrawGraphStatus()
		{
			if (!this.mGraphStatus.Equals(string.Empty))
			{
				GUI.Label(new Rect(this.mGraphRect.get_x() + 5f, this.mGraphRect.get_y() + 5f, this.mGraphRect.get_width(), 30f), this.mGraphStatus, BehaviorDesignerUtility.GraphStatusGUIStyle);
			}
		}

		private void DrawSelectedTaskDescription()
		{
			TaskDescriptionAttribute[] array;
			if (BehaviorDesignerPreferences.GetBool(BDPreferences.ShowTaskDescription) && this.mGraphDesigner.SelectedNodes.get_Count() == 1 && (array = (this.mGraphDesigner.SelectedNodes.get_Item(0).Task.GetType().GetCustomAttributes(typeof(TaskDescriptionAttribute), false) as TaskDescriptionAttribute[])).Length > 0)
			{
				float num;
				float num2;
				BehaviorDesignerUtility.TaskCommentGUIStyle.CalcMinMaxWidth(new GUIContent(array[0].get_Description()), ref num, ref num2);
				float num3 = Mathf.Min(400f, num2 + 20f);
				float num4 = Mathf.Min(300f, BehaviorDesignerUtility.TaskCommentGUIStyle.CalcHeight(new GUIContent(array[0].get_Description()), num3)) + 3f;
				GUI.Box(new Rect(this.mGraphRect.get_x() + 5f, this.mGraphRect.get_yMax() - num4 - 5f, num3, num4), string.Empty, BehaviorDesignerUtility.TaskDescriptionGUIStyle);
				GUI.Box(new Rect(this.mGraphRect.get_x() + 2f, this.mGraphRect.get_yMax() - num4 - 5f, num3, num4), array[0].get_Description(), BehaviorDesignerUtility.TaskCommentGUIStyle);
			}
		}

		private void AddBehavior()
		{
			if (EditorApplication.get_isPlaying())
			{
				return;
			}
			if (Selection.get_activeGameObject() != null)
			{
				GameObject activeGameObject = Selection.get_activeGameObject();
				this.mActiveObject = Selection.get_activeObject();
				this.mGraphDesigner = ScriptableObject.CreateInstance<GraphDesigner>();
				Type type = Type.GetType("BehaviorDesigner.Runtime.BehaviorTree, Assembly-CSharp");
				if (type == null)
				{
					type = Type.GetType("BehaviorDesigner.Runtime.BehaviorTree, Assembly-CSharp-firstpass");
				}
				Behavior behavior = BehaviorUndo.AddComponent(activeGameObject, type) as Behavior;
				Behavior[] components = activeGameObject.GetComponents<Behavior>();
				HashSet<string> hashSet = new HashSet<string>();
				string text = string.Empty;
				for (int i = 0; i < components.Length; i++)
				{
					text = components[i].GetBehaviorSource().behaviorName;
					int num = 2;
					while (hashSet.Contains(text))
					{
						text = string.Format("{0} {1}", components[i].GetBehaviorSource().behaviorName, num);
						num++;
					}
					components[i].GetBehaviorSource().behaviorName = text;
					hashSet.Add(components[i].GetBehaviorSource().behaviorName);
				}
				this.LoadBehavior(behavior.GetBehaviorSource(), false);
				this.Repaint();
				if (BehaviorDesignerPreferences.GetBool(BDPreferences.AddGameGUIComponent))
				{
					type = TaskUtility.GetTypeWithinAssembly("BehaviorDesigner.Runtime.BehaviorGameGUI");
					BehaviorUndo.AddComponent(activeGameObject, type);
				}
			}
		}

		private void RemoveBehavior()
		{
			if (EditorApplication.get_isPlaying())
			{
				return;
			}
			if (this.mActiveObject as GameObject != null && (this.mActiveBehaviorSource.get_EntryTask() == null || (this.mActiveBehaviorSource.get_EntryTask() != null && EditorUtility.DisplayDialog("Remove Behavior Tree", "Are you sure you want to remove this behavior tree?", "Yes", "No"))))
			{
				GameObject gameObject = this.mActiveObject as GameObject;
				int num = this.IndexForBehavior(this.mActiveBehaviorSource.get_Owner());
				BehaviorUndo.DestroyObject(this.mActiveBehaviorSource.get_Owner().GetObject(), true);
				num--;
				if (num == -1 && gameObject.GetComponents<Behavior>().Length > 0)
				{
					num = 0;
				}
				if (num > -1)
				{
					this.LoadBehavior(gameObject.GetComponents<Behavior>()[num].GetBehaviorSource(), true);
				}
				else
				{
					this.ClearGraph();
				}
				this.ClearBreadcrumbMenu();
				this.Repaint();
			}
		}

		private int IndexForBehavior(IBehavior behavior)
		{
			if (behavior.GetObject() as Behavior)
			{
				Behavior[] components = (behavior.GetObject() as Behavior).get_gameObject().GetComponents<Behavior>();
				for (int i = 0; i < components.Length; i++)
				{
					if (components[i].Equals(behavior))
					{
						return i;
					}
				}
				return -1;
			}
			return 0;
		}

		public NodeDesigner AddTask(Type type, bool useMousePosition)
		{
			if ((this.mActiveObject as GameObject == null && this.mActiveObject as ExternalBehavior == null) || EditorApplication.get_isPlaying())
			{
				return null;
			}
			Vector2 vector = new Vector2(this.mGraphRect.get_width() / (2f * this.mGraphZoom), 150f);
			if (useMousePosition)
			{
				this.GetMousePositionInGraph(out vector);
			}
			vector -= this.mGraphOffset;
			GameObject gameObject = this.mActiveObject as GameObject;
			if (gameObject != null && gameObject.GetComponent<Behavior>() == null)
			{
				this.AddBehavior();
			}
			BehaviorUndo.RegisterUndo("Add", this.mActiveBehaviorSource.get_Owner().GetObject());
			NodeDesigner nodeDesigner;
			if ((nodeDesigner = this.mGraphDesigner.AddNode(this.mActiveBehaviorSource, type, vector)) != null)
			{
				if (this.onAddTask != null)
				{
					this.onAddTask(this.mActiveBehaviorSource, nodeDesigner.Task);
				}
				this.SaveBehavior();
				return nodeDesigner;
			}
			return null;
		}

		public bool IsReferencingTasks()
		{
			return this.mTaskInspector.ActiveReferenceTask != null;
		}

		public bool IsReferencingField(FieldInfo fieldInfo)
		{
			return fieldInfo.Equals(this.mTaskInspector.ActiveReferenceTaskFieldInfo);
		}

		private void DisableReferenceTasks()
		{
			if (this.IsReferencingTasks())
			{
				this.ToggleReferenceTasks();
			}
		}

		public void ToggleReferenceTasks()
		{
			this.ToggleReferenceTasks(null, null);
		}

		public void ToggleReferenceTasks(Task task, FieldInfo fieldInfo)
		{
			bool flag = !this.IsReferencingTasks();
			this.mTaskInspector.SetActiveReferencedTasks((!flag) ? null : task, (!flag) ? null : fieldInfo);
			this.UpdateGraphStatus();
		}

		private void ReferenceTask(NodeDesigner nodeDesigner)
		{
			if (nodeDesigner != null && this.mTaskInspector.ReferenceTasks(nodeDesigner.Task))
			{
				this.SaveBehavior();
			}
		}

		public void IdentifyNode(NodeDesigner nodeDesigner)
		{
			this.mGraphDesigner.IdentifyNode(nodeDesigner);
		}

		private void TakeScreenshot()
		{
			this.mScreenshotPath = EditorUtility.SaveFilePanel("Save Screenshot", "Assets", this.mActiveBehaviorSource.behaviorName + "Screenshot.png", "png");
			if (this.mScreenshotPath.get_Length() != 0 && Application.get_dataPath().get_Length() < this.mScreenshotPath.get_Length())
			{
				this.mTakingScreenshot = true;
				this.mScreenshotGraphSize = this.mGraphDesigner.GraphSize(this.mGraphOffset);
				this.mGraphDesigner.GraphDirty();
				if (this.mScreenshotGraphSize.get_width() == 0f || this.mScreenshotGraphSize.get_height() == 0f)
				{
					this.mScreenshotGraphSize = new Rect(0f, 0f, 100f, 100f);
				}
				this.mScreenshotStartGraphZoom = this.mGraphZoom;
				this.mScreenshotStartGraphOffset = this.mGraphOffset;
				this.mGraphZoom = 1f;
				this.mGraphOffset.x = this.mGraphOffset.x - (this.mScreenshotGraphSize.get_xMin() - 10f);
				this.mGraphOffset.y = this.mGraphOffset.y - (this.mScreenshotGraphSize.get_yMin() - 10f);
				this.mScreenshotGraphOffset = this.mGraphOffset;
				this.mScreenshotGraphSize.Set(this.mScreenshotGraphSize.get_xMin() - 9f, this.mScreenshotGraphSize.get_yMin(), this.mScreenshotGraphSize.get_width() + 18f, this.mScreenshotGraphSize.get_height() + 18f);
				this.mScreenshotTexture = new Texture2D((int)this.mScreenshotGraphSize.get_width(), (int)this.mScreenshotGraphSize.get_height(), 3, false);
				this.Repaint();
			}
			else if (Path.GetExtension(this.mScreenshotPath).Equals(".png"))
			{
				Debug.LogError("Error: Unable to save screenshot. The save location must be within the Asset directory.");
			}
		}

		private void RenderScreenshotTile()
		{
			float num = Mathf.Min(this.mGraphRect.get_width(), this.mScreenshotGraphSize.get_width() - (this.mGraphOffset.x - this.mScreenshotGraphOffset.x));
			float num2 = Mathf.Min(this.mGraphRect.get_height(), this.mScreenshotGraphSize.get_height() + (this.mGraphOffset.y - this.mScreenshotGraphOffset.y));
			Rect rect = new Rect(this.mGraphRect.get_x(), 39f + this.mGraphRect.get_height() - num2 - 7f, num, num2);
			this.mScreenshotTexture.ReadPixels(rect, -(int)(this.mGraphOffset.x - this.mScreenshotGraphOffset.x), (int)(this.mScreenshotGraphSize.get_height() - num2 + (this.mGraphOffset.y - this.mScreenshotGraphOffset.y)));
			this.mScreenshotTexture.Apply(false);
			if (this.mScreenshotGraphSize.get_xMin() + num - (this.mGraphOffset.x - this.mScreenshotGraphOffset.x) < this.mScreenshotGraphSize.get_xMax())
			{
				this.mGraphOffset.x = this.mGraphOffset.x - (num - 1f);
				this.mGraphDesigner.GraphDirty();
				this.Repaint();
			}
			else if (this.mScreenshotGraphSize.get_yMin() + num2 - (this.mGraphOffset.y - this.mScreenshotGraphOffset.y) < this.mScreenshotGraphSize.get_yMax())
			{
				this.mGraphOffset.y = this.mGraphOffset.y - (num2 - 1f);
				this.mGraphOffset.x = this.mScreenshotGraphOffset.x;
				this.mGraphDesigner.GraphDirty();
				this.Repaint();
			}
			else
			{
				this.SaveScreenshot();
			}
		}

		private void SaveScreenshot()
		{
			byte[] array = ImageConversion.EncodeToPNG(this.mScreenshotTexture);
			Object.DestroyImmediate(this.mScreenshotTexture, true);
			File.WriteAllBytes(this.mScreenshotPath, array);
			string text = string.Format("Assets/{0}", this.mScreenshotPath.Substring(Application.get_dataPath().get_Length() + 1));
			AssetDatabase.ImportAsset(text);
			this.mTakingScreenshot = false;
			this.mGraphZoom = this.mScreenshotStartGraphZoom;
			this.mGraphOffset = this.mScreenshotStartGraphOffset;
			this.mGraphDesigner.GraphDirty();
			this.Repaint();
		}

		private void HandleEvents()
		{
			if (this.mTakingScreenshot)
			{
				return;
			}
			if (Event.get_current().get_type() != 1 && this.CheckForAutoScroll())
			{
				this.Repaint();
				return;
			}
			if (Event.get_current().get_type() == 7 || Event.get_current().get_type() == 8)
			{
				return;
			}
			switch (Event.get_current().get_type())
			{
			case 0:
				if (Event.get_current().get_button() == 0 && Event.get_current().get_modifiers() != 2)
				{
					Vector2 mousePosition;
					if (this.GetMousePositionInGraph(out mousePosition))
					{
						if (this.LeftMouseDown(Event.get_current().get_clickCount(), mousePosition))
						{
							Event.get_current().Use();
						}
					}
					else if (this.GetMousePositionInPropertiesPane(out mousePosition) && this.mBehaviorToolbarSelection == 2 && this.mVariableInspector.LeftMouseDown(this.mActiveBehaviorSource, this.mActiveBehaviorSource, mousePosition))
					{
						Event.get_current().Use();
						this.Repaint();
					}
				}
				else if ((Event.get_current().get_button() == 1 || (Event.get_current().get_modifiers() == 2 && Event.get_current().get_button() == 0)) && this.RightMouseDown())
				{
					Event.get_current().Use();
				}
				break;
			case 1:
				if (Event.get_current().get_button() == 0 && Event.get_current().get_modifiers() != 2)
				{
					if (this.LeftMouseRelease())
					{
						Event.get_current().Use();
					}
				}
				else if ((Event.get_current().get_button() == 1 || (Event.get_current().get_modifiers() == 2 && Event.get_current().get_button() == 0)) && this.mShowRightClickMenu)
				{
					this.mShowRightClickMenu = false;
					this.mRightClickMenu.ShowAsContext();
					Event.get_current().Use();
				}
				break;
			case 2:
				if (this.MouseMove())
				{
					Event.get_current().Use();
				}
				break;
			case 3:
				if (Event.get_current().get_button() == 0)
				{
					if (this.LeftMouseDragged())
					{
						Event.get_current().Use();
					}
					else if (Event.get_current().get_modifiers() == 4 && this.MousePan())
					{
						Event.get_current().Use();
					}
				}
				else if (Event.get_current().get_button() == 2 && this.MousePan())
				{
					Event.get_current().Use();
				}
				break;
			case 4:
				if (Event.get_current().get_keyCode() == 310 || Event.get_current().get_keyCode() == 309)
				{
					this.mCommandDown = true;
				}
				break;
			case 5:
				if (Event.get_current().get_keyCode() == 127 || Event.get_current().get_keyCode() == 8 || Event.get_current().get_commandName().Equals("Delete"))
				{
					if (this.PropertiesInspectorHasFocus() || EditorApplication.get_isPlaying())
					{
						return;
					}
					this.DeleteNodes();
					Event.get_current().Use();
				}
				else if (Event.get_current().get_keyCode() == 13 || Event.get_current().get_keyCode() == 271)
				{
					if (this.mBehaviorToolbarSelection == 2 && this.mVariableInspector.HasFocus())
					{
						if (this.mVariableInspector.ClearFocus(true, this.mActiveBehaviorSource))
						{
							this.SaveBehavior();
						}
						this.Repaint();
					}
					else
					{
						this.DisableReferenceTasks();
					}
					Event.get_current().Use();
				}
				else if (Event.get_current().get_keyCode() == 27)
				{
					this.DisableReferenceTasks();
				}
				else if (Event.get_current().get_keyCode() == 310 || Event.get_current().get_keyCode() == 309)
				{
					this.mCommandDown = false;
				}
				break;
			case 6:
				if (BehaviorDesignerPreferences.GetBool(BDPreferences.MouseWhellScrolls) && !this.mCommandDown)
				{
					this.MousePan();
				}
				else if (this.MouseZoom())
				{
					Event.get_current().Use();
				}
				break;
			case 13:
				if (EditorApplication.get_isPlaying())
				{
					return;
				}
				if (Event.get_current().get_commandName().Equals("Copy") || Event.get_current().get_commandName().Equals("Paste") || Event.get_current().get_commandName().Equals("Cut") || Event.get_current().get_commandName().Equals("SelectAll") || Event.get_current().get_commandName().Equals("Duplicate"))
				{
					if (this.PropertiesInspectorHasFocus() || EditorApplication.get_isPlaying() || this.ViewOnlyMode())
					{
						return;
					}
					Event.get_current().Use();
				}
				break;
			case 14:
				if (this.PropertiesInspectorHasFocus() || EditorApplication.get_isPlaying() || this.ViewOnlyMode())
				{
					return;
				}
				if (Event.get_current().get_commandName().Equals("Copy"))
				{
					this.CopyNodes();
					Event.get_current().Use();
				}
				else if (Event.get_current().get_commandName().Equals("Paste"))
				{
					this.PasteNodes();
					Event.get_current().Use();
				}
				else if (Event.get_current().get_commandName().Equals("Cut"))
				{
					this.CutNodes();
					Event.get_current().Use();
				}
				else if (Event.get_current().get_commandName().Equals("SelectAll"))
				{
					this.mGraphDesigner.SelectAll();
					Event.get_current().Use();
				}
				else if (Event.get_current().get_commandName().Equals("Duplicate"))
				{
					this.DuplicateNodes();
					Event.get_current().Use();
				}
				break;
			}
		}

		private bool CheckForAutoScroll()
		{
			Vector2 vector;
			if (!this.GetMousePositionInGraph(out vector))
			{
				return false;
			}
			if (this.mGraphScrollRect.Contains(this.mCurrentMousePosition))
			{
				return false;
			}
			if (this.mIsDragging || this.mIsSelecting || this.mGraphDesigner.ActiveNodeConnection != null)
			{
				Vector2 zero = Vector2.get_zero();
				if (this.mCurrentMousePosition.y < this.mGraphScrollRect.get_yMin() + 15f)
				{
					zero.y = 3f;
				}
				else if (this.mCurrentMousePosition.y > this.mGraphScrollRect.get_yMax() - 15f)
				{
					zero.y = -3f;
				}
				if (this.mCurrentMousePosition.x < this.mGraphScrollRect.get_xMin() + 15f)
				{
					zero.x = 3f;
				}
				else if (this.mCurrentMousePosition.x > this.mGraphScrollRect.get_xMax() - 15f)
				{
					zero.x = -3f;
				}
				this.ScrollGraph(zero);
				if (this.mIsDragging)
				{
					this.mGraphDesigner.DragSelectedNodes(-zero / this.mGraphZoom, Event.get_current().get_modifiers() != 4);
				}
				if (this.mIsSelecting)
				{
					this.mSelectStartPosition += zero / this.mGraphZoom;
				}
				return true;
			}
			return false;
		}

		private bool MouseMove()
		{
			Vector2 point;
			if (!this.GetMousePositionInGraph(out point))
			{
				return false;
			}
			NodeDesigner nodeDesigner = this.mGraphDesigner.NodeAt(point, this.mGraphOffset);
			if (this.mGraphDesigner.HoverNode != null && ((nodeDesigner != null && !this.mGraphDesigner.HoverNode.Equals(nodeDesigner)) || !this.mGraphDesigner.HoverNode.HoverBarAreaContains(point, this.mGraphOffset)))
			{
				this.mGraphDesigner.ClearHover();
				this.Repaint();
			}
			if (nodeDesigner && !nodeDesigner.IsEntryDisplay && !this.ViewOnlyMode())
			{
				this.mGraphDesigner.Hover(nodeDesigner);
			}
			return this.mGraphDesigner.HoverNode != null;
		}

		private bool LeftMouseDown(int clickCount, Vector2 mousePosition)
		{
			if (this.PropertiesInspectorHasFocus())
			{
				this.mTaskInspector.ClearFocus();
				this.mVariableInspector.ClearFocus(false, null);
				this.Repaint();
			}
			NodeDesigner nodeDesigner = this.mGraphDesigner.NodeAt(mousePosition, this.mGraphOffset);
			if (Event.get_current().get_modifiers() == 4)
			{
				this.mNodeClicked = this.mGraphDesigner.IsSelected(nodeDesigner);
				return false;
			}
			if (this.IsReferencingTasks())
			{
				if (nodeDesigner == null)
				{
					this.DisableReferenceTasks();
				}
				else
				{
					this.ReferenceTask(nodeDesigner);
				}
				return true;
			}
			if (nodeDesigner != null)
			{
				if (this.mGraphDesigner.HoverNode != null && !nodeDesigner.Equals(this.mGraphDesigner.HoverNode))
				{
					this.mGraphDesigner.ClearHover();
					this.mGraphDesigner.Hover(nodeDesigner);
				}
				NodeConnection nodeConnection;
				if (!this.ViewOnlyMode() && (nodeConnection = nodeDesigner.NodeConnectionRectContains(mousePosition, this.mGraphOffset)) != null)
				{
					if (this.mGraphDesigner.NodeCanOriginateConnection(nodeDesigner, nodeConnection))
					{
						this.mGraphDesigner.ActiveNodeConnection = nodeConnection;
					}
					return true;
				}
				if (nodeDesigner.Contains(mousePosition, this.mGraphOffset, false))
				{
					this.mKeepTasksSelected = false;
					if (this.mGraphDesigner.IsSelected(nodeDesigner))
					{
						if (Event.get_current().get_modifiers() == 2)
						{
							this.mKeepTasksSelected = true;
							this.mGraphDesigner.Deselect(nodeDesigner);
						}
						else if (Event.get_current().get_modifiers() == 1 && nodeDesigner.Task is ParentTask)
						{
							nodeDesigner.Task.get_NodeData().set_Collapsed(!nodeDesigner.Task.get_NodeData().get_Collapsed());
							this.mGraphDesigner.DeselectWithParent(nodeDesigner);
						}
						else if (clickCount == 2)
						{
							if (this.mBehaviorToolbarSelection != 3 && BehaviorDesignerPreferences.GetBool(BDPreferences.OpenInspectorOnTaskDoubleClick))
							{
								this.mBehaviorToolbarSelection = 3;
							}
							else if (nodeDesigner.Task is BehaviorReference)
							{
								BehaviorReference behaviorReference = nodeDesigner.Task as BehaviorReference;
								if (behaviorReference.GetExternalBehaviors() != null && behaviorReference.GetExternalBehaviors().Length > 0 && behaviorReference.GetExternalBehaviors()[0] != null)
								{
									if (this.mLockActiveGameObject)
									{
										this.LoadBehavior(behaviorReference.GetExternalBehaviors()[0].GetBehaviorSource(), false);
									}
									else
									{
										Selection.set_activeObject(behaviorReference.GetExternalBehaviors()[0]);
									}
								}
							}
						}
					}
					else
					{
						if (Event.get_current().get_modifiers() != 1 && Event.get_current().get_modifiers() != 2)
						{
							this.mGraphDesigner.ClearNodeSelection();
							this.mGraphDesigner.ClearConnectionSelection();
							if (BehaviorDesignerPreferences.GetBool(BDPreferences.OpenInspectorOnTaskSelection))
							{
								this.mBehaviorToolbarSelection = 3;
							}
						}
						else
						{
							this.mKeepTasksSelected = true;
						}
						this.mGraphDesigner.Select(nodeDesigner);
					}
					this.mNodeClicked = this.mGraphDesigner.IsSelected(nodeDesigner);
					return true;
				}
			}
			if (this.mGraphDesigner.HoverNode != null)
			{
				bool flag = false;
				if (this.mGraphDesigner.HoverNode.HoverBarButtonClick(mousePosition, this.mGraphOffset, ref flag))
				{
					this.SaveBehavior();
					if (flag && this.mGraphDesigner.HoverNode.Task.get_NodeData().get_Collapsed())
					{
						this.mGraphDesigner.DeselectWithParent(this.mGraphDesigner.HoverNode);
					}
					return true;
				}
			}
			List<NodeConnection> list = new List<NodeConnection>();
			this.mGraphDesigner.NodeConnectionsAt(mousePosition, this.mGraphOffset, ref list);
			if (list.get_Count() > 0)
			{
				if (Event.get_current().get_modifiers() != 1 && Event.get_current().get_modifiers() != 2)
				{
					this.mGraphDesigner.ClearNodeSelection();
					this.mGraphDesigner.ClearConnectionSelection();
				}
				for (int i = 0; i < list.get_Count(); i++)
				{
					if (this.mGraphDesigner.IsSelected(list.get_Item(i)))
					{
						if (Event.get_current().get_modifiers() == 2)
						{
							this.mGraphDesigner.Deselect(list.get_Item(i));
						}
					}
					else
					{
						this.mGraphDesigner.Select(list.get_Item(i));
					}
				}
				return true;
			}
			if (Event.get_current().get_modifiers() != 1)
			{
				this.mGraphDesigner.ClearNodeSelection();
				this.mGraphDesigner.ClearConnectionSelection();
			}
			this.mSelectStartPosition = mousePosition;
			this.mIsSelecting = true;
			this.mIsDragging = false;
			this.mDragDelta = Vector2.get_zero();
			this.mNodeClicked = false;
			return true;
		}

		private bool LeftMouseDragged()
		{
			Vector2 vector;
			if (!this.GetMousePositionInGraph(out vector))
			{
				return false;
			}
			if (Event.get_current().get_modifiers() != 4)
			{
				if (this.IsReferencingTasks())
				{
					return true;
				}
				if (this.mIsSelecting)
				{
					this.mGraphDesigner.DeselectAll(null);
					List<NodeDesigner> list = this.mGraphDesigner.NodesAt(this.GetSelectionArea(), this.mGraphOffset);
					if (list != null)
					{
						for (int i = 0; i < list.get_Count(); i++)
						{
							this.mGraphDesigner.Select(list.get_Item(i));
						}
					}
					return true;
				}
				if (this.mGraphDesigner.ActiveNodeConnection != null)
				{
					return true;
				}
			}
			if (this.mNodeClicked && !this.ViewOnlyMode())
			{
				Vector2 vector2 = Vector2.get_zero();
				if (BehaviorDesignerPreferences.GetBool(BDPreferences.SnapToGrid))
				{
					this.mDragDelta += Event.get_current().get_delta();
					if (Mathf.Abs(this.mDragDelta.x) > 10f)
					{
						float num = Mathf.Abs(this.mDragDelta.x) % 10f;
						vector2.x = (Mathf.Abs(this.mDragDelta.x) - num) * Mathf.Sign(this.mDragDelta.x);
						this.mDragDelta.x = num * Mathf.Sign(this.mDragDelta.x);
					}
					if (Mathf.Abs(this.mDragDelta.y) > 10f)
					{
						float num2 = Mathf.Abs(this.mDragDelta.y) % 10f;
						vector2.y = (Mathf.Abs(this.mDragDelta.y) - num2) * Mathf.Sign(this.mDragDelta.y);
						this.mDragDelta.y = num2 * Mathf.Sign(this.mDragDelta.y);
					}
				}
				else
				{
					vector2 = Event.get_current().get_delta();
				}
				bool flag = this.mGraphDesigner.DragSelectedNodes(vector2 / this.mGraphZoom, Event.get_current().get_modifiers() != 4);
				if (flag)
				{
					this.mKeepTasksSelected = true;
				}
				this.mIsDragging = true;
				return flag;
			}
			return false;
		}

		private bool LeftMouseRelease()
		{
			this.mNodeClicked = false;
			if (this.IsReferencingTasks())
			{
				if (!this.mTaskInspector.IsActiveTaskArray() && !this.mTaskInspector.IsActiveTaskNull())
				{
					this.DisableReferenceTasks();
					this.Repaint();
				}
				Vector2 vector;
				if (!this.GetMousePositionInGraph(out vector))
				{
					this.mGraphDesigner.ActiveNodeConnection = null;
					return false;
				}
				return true;
			}
			else
			{
				if (this.mIsSelecting)
				{
					this.mIsSelecting = false;
					return true;
				}
				if (this.mIsDragging)
				{
					BehaviorUndo.RegisterUndo("Drag", this.mActiveBehaviorSource.get_Owner().GetObject());
					this.SaveBehavior();
					this.mIsDragging = false;
					this.mDragDelta = Vector3.get_zero();
					return true;
				}
				if (this.mGraphDesigner.ActiveNodeConnection != null)
				{
					Vector2 point;
					if (!this.GetMousePositionInGraph(out point))
					{
						this.mGraphDesigner.ActiveNodeConnection = null;
						return false;
					}
					NodeDesigner nodeDesigner = this.mGraphDesigner.NodeAt(point, this.mGraphOffset);
					if (nodeDesigner != null && !nodeDesigner.Equals(this.mGraphDesigner.ActiveNodeConnection.OriginatingNodeDesigner) && this.mGraphDesigner.NodeCanAcceptConnection(nodeDesigner, this.mGraphDesigner.ActiveNodeConnection))
					{
						this.mGraphDesigner.ConnectNodes(this.mActiveBehaviorSource, nodeDesigner);
						BehaviorUndo.RegisterUndo("Task Connection", this.mActiveBehaviorSource.get_Owner().GetObject());
						this.SaveBehavior();
					}
					else
					{
						this.mGraphDesigner.ActiveNodeConnection = null;
					}
					return true;
				}
				else
				{
					if (Event.get_current().get_modifiers() == 1 || this.mKeepTasksSelected)
					{
						return false;
					}
					Vector2 point2;
					if (!this.GetMousePositionInGraph(out point2))
					{
						return false;
					}
					NodeDesigner nodeDesigner2 = this.mGraphDesigner.NodeAt(point2, this.mGraphOffset);
					if (nodeDesigner2 != null && !this.mGraphDesigner.IsSelected(nodeDesigner2))
					{
						this.mGraphDesigner.DeselectAll(nodeDesigner2);
					}
					return true;
				}
			}
		}

		private bool RightMouseDown()
		{
			if (this.IsReferencingTasks())
			{
				this.DisableReferenceTasks();
				return false;
			}
			Vector2 point;
			if (!this.GetMousePositionInGraph(out point))
			{
				return false;
			}
			NodeDesigner nodeDesigner = this.mGraphDesigner.NodeAt(point, this.mGraphOffset);
			if (nodeDesigner == null || !this.mGraphDesigner.IsSelected(nodeDesigner))
			{
				this.mGraphDesigner.ClearNodeSelection();
				this.mGraphDesigner.ClearConnectionSelection();
				if (nodeDesigner != null)
				{
					this.mGraphDesigner.Select(nodeDesigner);
				}
			}
			if (this.mGraphDesigner.HoverNode != null)
			{
				this.mGraphDesigner.ClearHover();
			}
			this.BuildRightClickMenu(nodeDesigner);
			return true;
		}

		private bool MouseZoom()
		{
			Vector2 vector;
			if (!this.GetMousePositionInGraph(out vector))
			{
				return false;
			}
			float num = -Event.get_current().get_delta().y / 150f;
			this.mGraphZoom += num;
			this.mGraphZoom = Mathf.Clamp(this.mGraphZoom, 0.4f, 1f);
			Vector2 vector2;
			this.GetMousePositionInGraph(out vector2);
			this.mGraphOffset += vector2 - vector;
			this.mGraphScrollPosition += vector2 - vector;
			this.mGraphDesigner.GraphDirty();
			return true;
		}

		private bool MousePan()
		{
			Vector2 vector;
			if (!this.GetMousePositionInGraph(out vector))
			{
				return false;
			}
			Vector2 vector2 = Event.get_current().get_delta();
			if (Event.get_current().get_type() == 6)
			{
				vector2 *= -1.5f;
				if (Event.get_current().get_modifiers() == 2)
				{
					vector2.x = vector2.y;
					vector2.y = 0f;
				}
			}
			this.ScrollGraph(vector2);
			return true;
		}

		private void ScrollGraph(Vector2 amount)
		{
			this.mGraphOffset += amount / this.mGraphZoom;
			this.mGraphScrollPosition -= amount;
			this.mGraphDesigner.GraphDirty();
			this.Repaint();
		}

		private bool PropertiesInspectorHasFocus()
		{
			return this.mTaskInspector.HasFocus() || this.mVariableInspector.HasFocus();
		}

		private void AddTaskCallback(object obj)
		{
			this.AddTask((Type)obj, true);
		}

		private void ReplaceTaskCallback(object obj)
		{
			Type type = (Type)obj;
			if (this.mGraphDesigner.SelectedNodes.get_Count() != 1 || this.mGraphDesigner.SelectedNodes.get_Item(0).Task.GetType().Equals(type))
			{
				return;
			}
			if (this.mGraphDesigner.ReplaceSelectedNode(this.mActiveBehaviorSource, type))
			{
				this.SaveBehavior();
			}
		}

		private void BehaviorSelectionCallback(object obj)
		{
			BehaviorSource behaviorSource = obj as BehaviorSource;
			if (behaviorSource.get_Owner() is Behavior)
			{
				this.mActiveObject = (behaviorSource.get_Owner() as Behavior).get_gameObject();
			}
			else
			{
				this.mActiveObject = (behaviorSource.get_Owner() as ExternalBehavior);
			}
			if (!this.mLockActiveGameObject)
			{
				Selection.set_activeObject(this.mActiveObject);
			}
			this.LoadBehavior(behaviorSource, false);
			this.UpdateGraphStatus();
			if (EditorApplication.get_isPaused())
			{
				this.mUpdateNodeTaskMap = true;
				this.UpdateNodeTaskMap();
			}
		}

		private void ToggleEnableState(object obj)
		{
			NodeDesigner nodeDesigner = obj as NodeDesigner;
			nodeDesigner.ToggleEnableState();
			this.SaveBehavior();
			this.Repaint();
		}

		private void ToggleCollapseState(object obj)
		{
			NodeDesigner nodeDesigner = obj as NodeDesigner;
			if (nodeDesigner.ToggleCollapseState())
			{
				this.mGraphDesigner.DeselectWithParent(nodeDesigner);
			}
			this.SaveBehavior();
			this.Repaint();
		}

		private void ToggleBreakpoint(object obj)
		{
			NodeDesigner nodeDesigner = obj as NodeDesigner;
			nodeDesigner.ToggleBreakpoint();
			this.SaveBehavior();
			this.Repaint();
		}

		private void OpenInFileEditor(object obj)
		{
			NodeDesigner nodeDesigner = obj as NodeDesigner;
			TaskInspector.OpenInFileEditor(nodeDesigner.Task);
		}

		private void SelectInProject(object obj)
		{
			NodeDesigner nodeDesigner = obj as NodeDesigner;
			TaskInspector.SelectInProject(nodeDesigner.Task);
		}

		private void CopyNodes()
		{
			this.mCopiedTasks = this.mGraphDesigner.Copy(this.mGraphOffset, this.mGraphZoom);
		}

		private void PasteNodes()
		{
			if (this.mActiveObject == null || EditorApplication.get_isPlaying())
			{
				return;
			}
			GameObject gameObject = this.mActiveObject as GameObject;
			if (gameObject != null && gameObject.GetComponent<Behavior>() == null)
			{
				this.AddBehavior();
			}
			if (this.mCopiedTasks != null && this.mCopiedTasks.get_Count() > 0)
			{
				BehaviorUndo.RegisterUndo("Paste", this.mActiveBehaviorSource.get_Owner().GetObject());
			}
			this.mGraphDesigner.Paste(this.mActiveBehaviorSource, new Vector2(this.mGraphRect.get_width() / (2f * this.mGraphZoom) - this.mGraphOffset.x, 150f - this.mGraphOffset.y), this.mCopiedTasks, this.mGraphOffset, this.mGraphZoom);
			this.SaveBehavior();
		}

		private void CutNodes()
		{
			this.mCopiedTasks = this.mGraphDesigner.Copy(this.mGraphOffset, this.mGraphZoom);
			if (this.mCopiedTasks != null && this.mCopiedTasks.get_Count() > 0)
			{
				BehaviorUndo.RegisterUndo("Cut", this.mActiveBehaviorSource.get_Owner().GetObject());
			}
			this.mGraphDesigner.Delete(this.mActiveBehaviorSource, null);
			this.SaveBehavior();
		}

		private void DuplicateNodes()
		{
			List<TaskSerializer> list = this.mGraphDesigner.Copy(this.mGraphOffset, this.mGraphZoom);
			if (list != null && list.get_Count() > 0)
			{
				BehaviorUndo.RegisterUndo("Duplicate", this.mActiveBehaviorSource.get_Owner().GetObject());
			}
			this.mGraphDesigner.Paste(this.mActiveBehaviorSource, new Vector2(this.mGraphRect.get_width() / (2f * this.mGraphZoom) - this.mGraphOffset.x, 150f - this.mGraphOffset.y), list, this.mGraphOffset, this.mGraphZoom);
			this.SaveBehavior();
		}

		private void DeleteNodes()
		{
			if (this.ViewOnlyMode())
			{
				return;
			}
			this.mGraphDesigner.Delete(this.mActiveBehaviorSource, this.onRemoveTask);
			this.SaveBehavior();
		}

		public void RemoveSharedVariableReferences(SharedVariable sharedVariable)
		{
			if (this.mGraphDesigner.RemoveSharedVariableReferences(sharedVariable))
			{
				this.SaveBehavior();
				this.Repaint();
			}
		}

		private void OnUndoRedo()
		{
			if (this.mActiveBehaviorSource != null)
			{
				this.LoadBehavior(this.mActiveBehaviorSource, true, false);
			}
		}

		private void SetupSizes()
		{
			float width = base.get_position().get_width();
			float num = base.get_position().get_height() + 22f;
			if (this.mPrevScreenWidth == width && this.mPrevScreenHeight == num && this.mPropertiesPanelOnLeft == BehaviorDesignerPreferences.GetBool(BDPreferences.PropertiesPanelOnLeft))
			{
				return;
			}
			if (BehaviorDesignerPreferences.GetBool(BDPreferences.PropertiesPanelOnLeft))
			{
				this.mFileToolBarRect = new Rect(300f, 0f, width - 300f, 18f);
				this.mPropertyToolbarRect = new Rect(0f, 0f, 300f, 18f);
				this.mPropertyBoxRect = new Rect(0f, this.mPropertyToolbarRect.get_height(), 300f, num - this.mPropertyToolbarRect.get_height() - 21f);
				this.mGraphRect = new Rect(300f, 18f, width - 300f - 15f, num - 36f - 21f - 15f);
				this.mPreferencesPaneRect = new Rect(300f + this.mGraphRect.get_width() - 290f, (float)(18 + ((!EditorGUIUtility.get_isProSkin()) ? 2 : 1)), 290f, 368f);
			}
			else
			{
				this.mFileToolBarRect = new Rect(0f, 0f, width - 300f, 18f);
				this.mPropertyToolbarRect = new Rect(width - 300f, 0f, 300f, 18f);
				this.mPropertyBoxRect = new Rect(width - 300f, this.mPropertyToolbarRect.get_height(), 300f, num - this.mPropertyToolbarRect.get_height() - 21f);
				this.mGraphRect = new Rect(0f, 18f, width - 300f - 15f, num - 36f - 21f - 15f);
				this.mPreferencesPaneRect = new Rect(this.mGraphRect.get_width() - 290f, (float)(18 + ((!EditorGUIUtility.get_isProSkin()) ? 2 : 1)), 290f, 368f);
			}
			this.mDebugToolBarRect = new Rect(this.mGraphRect.get_x(), num - 18f - 21f, this.mGraphRect.get_width() + 15f, 18f);
			this.mGraphScrollRect.Set(this.mGraphRect.get_xMin() + 15f, this.mGraphRect.get_yMin() + 15f, this.mGraphRect.get_width() - 30f, this.mGraphRect.get_height() - 30f);
			if (this.mGraphScrollPosition == new Vector2(-1f, -1f))
			{
				this.mGraphScrollPosition = (this.mGraphScrollSize - new Vector2(this.mGraphRect.get_width(), this.mGraphRect.get_height())) / 2f - 2f * new Vector2(15f, 15f);
			}
			this.mPrevScreenWidth = width;
			this.mPrevScreenHeight = num;
			this.mPropertiesPanelOnLeft = BehaviorDesignerPreferences.GetBool(BDPreferences.PropertiesPanelOnLeft);
		}

		private bool GetMousePositionInGraph(out Vector2 mousePosition)
		{
			mousePosition = this.mCurrentMousePosition;
			if (!this.mGraphRect.Contains(mousePosition))
			{
				return false;
			}
			if (this.mShowPrefPane && this.mPreferencesPaneRect.Contains(mousePosition))
			{
				return false;
			}
			mousePosition -= new Vector2(this.mGraphRect.get_xMin(), this.mGraphRect.get_yMin());
			mousePosition /= this.mGraphZoom;
			return true;
		}

		private bool GetMousePositionInPropertiesPane(out Vector2 mousePosition)
		{
			mousePosition = this.mCurrentMousePosition;
			if (!this.mPropertyBoxRect.Contains(mousePosition))
			{
				return false;
			}
			mousePosition.x -= this.mPropertyBoxRect.get_xMin();
			mousePosition.y -= this.mPropertyBoxRect.get_yMin();
			return true;
		}

		private Rect GetSelectionArea()
		{
			Vector2 vector;
			if (this.GetMousePositionInGraph(out vector))
			{
				float num = (this.mSelectStartPosition.x >= vector.x) ? vector.x : this.mSelectStartPosition.x;
				float num2 = (this.mSelectStartPosition.x <= vector.x) ? vector.x : this.mSelectStartPosition.x;
				float num3 = (this.mSelectStartPosition.y >= vector.y) ? vector.y : this.mSelectStartPosition.y;
				float num4 = (this.mSelectStartPosition.y <= vector.y) ? vector.y : this.mSelectStartPosition.y;
				this.mSelectionArea = new Rect(num, num3, num2 - num, num4 - num3);
			}
			return this.mSelectionArea;
		}

		public bool ViewOnlyMode()
		{
			if (Application.get_isPlaying())
			{
				return false;
			}
			if (this.mActiveBehaviorSource == null || this.mActiveBehaviorSource.get_Owner() == null || this.mActiveBehaviorSource.get_Owner().Equals(null))
			{
				return false;
			}
			Behavior behavior = this.mActiveBehaviorSource.get_Owner().GetObject() as Behavior;
			return behavior != null && !BehaviorDesignerPreferences.GetBool(BDPreferences.EditablePrefabInstances) && PrefabUtility.GetPrefabType(this.mActiveBehaviorSource.get_Owner().GetObject()) == 3;
		}

		private BehaviorSource BehaviorSourceFromIBehaviorHistory(IBehavior behavior)
		{
			if (behavior == null)
			{
				return null;
			}
			if (behavior.GetObject() is GameObject)
			{
				Behavior[] components = (behavior.GetObject() as GameObject).GetComponents<Behavior>();
				for (int i = 0; i < Enumerable.Count<Behavior>(components); i++)
				{
					if (components[i].GetBehaviorSource().get_BehaviorID() == behavior.GetBehaviorSource().get_BehaviorID())
					{
						return components[i].GetBehaviorSource();
					}
				}
				return null;
			}
			return behavior.GetBehaviorSource();
		}

		public void SaveBehavior()
		{
			if (this.mActiveBehaviorSource == null || this.ViewOnlyMode() || Application.get_isPlaying())
			{
				return;
			}
			this.mGraphDesigner.Save(this.mActiveBehaviorSource);
			this.CheckForErrors();
		}

		private void CheckForErrors()
		{
			if (this.mErrorDetails != null)
			{
				for (int i = 0; i < this.mErrorDetails.get_Count(); i++)
				{
					if (this.mErrorDetails.get_Item(i).NodeDesigner != null)
					{
						this.mErrorDetails.get_Item(i).NodeDesigner.HasError = false;
					}
				}
			}
			if (BehaviorDesignerPreferences.GetBool(BDPreferences.ErrorChecking))
			{
				BehaviorSource behaviorSource = (this.mExternalParent == null) ? this.mActiveBehaviorSource : this.mExternalParent;
				this.mErrorDetails = ErrorCheck.CheckForErrors(behaviorSource);
				if (this.mErrorDetails != null)
				{
					for (int j = 0; j < this.mErrorDetails.get_Count(); j++)
					{
						if (!(this.mErrorDetails.get_Item(j).NodeDesigner == null))
						{
							this.mErrorDetails.get_Item(j).NodeDesigner.HasError = true;
						}
					}
				}
			}
			else
			{
				this.mErrorDetails = null;
			}
			if (ErrorWindow.instance != null)
			{
				ErrorWindow.instance.ErrorDetails = this.mErrorDetails;
				ErrorWindow.instance.Repaint();
			}
		}

		public bool ContainsError(Task task, string fieldName)
		{
			if (this.mErrorDetails == null)
			{
				return false;
			}
			for (int i = 0; i < this.mErrorDetails.get_Count(); i++)
			{
				if (task == null)
				{
					if (!(this.mErrorDetails.get_Item(i).NodeDesigner != null))
					{
						if (this.mErrorDetails.get_Item(i).FieldName == fieldName)
						{
							return true;
						}
					}
				}
				else if (!(this.mErrorDetails.get_Item(i).NodeDesigner == null))
				{
					if (this.mErrorDetails.get_Item(i).NodeDesigner.Task == task && this.mErrorDetails.get_Item(i).FieldName == fieldName)
					{
						return true;
					}
				}
			}
			return false;
		}

		private bool UpdateCheck()
		{
			if (this.mUpdateCheckRequest != null && this.mUpdateCheckRequest.get_isDone())
			{
				if (!string.IsNullOrEmpty(this.mUpdateCheckRequest.get_error()))
				{
					this.mUpdateCheckRequest = null;
					return false;
				}
				if (!"1.5.11".ToString().Equals(this.mUpdateCheckRequest.get_text()))
				{
					this.LatestVersion = this.mUpdateCheckRequest.get_text();
				}
				this.mUpdateCheckRequest = null;
			}
			if (BehaviorDesignerPreferences.GetBool(BDPreferences.UpdateCheck) && DateTime.Compare(this.LastUpdateCheck.AddDays(1.0), DateTime.get_UtcNow()) < 0)
			{
				string text = string.Format("http://www.opsive.com/assets/BehaviorDesigner/UpdateCheck.php?version={0}&unityversion={1}&devplatform={2}&targetplatform={3}", new object[]
				{
					"1.5.11",
					Application.get_unityVersion(),
					Application.get_platform(),
					EditorUserBuildSettings.get_activeBuildTarget()
				});
				this.mUpdateCheckRequest = new WWW(text);
				this.LastUpdateCheck = DateTime.get_UtcNow();
			}
			return this.mUpdateCheckRequest != null;
		}

		private void SaveAsAsset()
		{
			if (this.mActiveBehaviorSource == null)
			{
				return;
			}
			string text = EditorUtility.SaveFilePanel("Save Behavior Tree", "Assets", this.mActiveBehaviorSource.behaviorName + ".asset", "asset");
			if (text.get_Length() != 0 && Application.get_dataPath().get_Length() < text.get_Length())
			{
				Type type = Type.GetType("BehaviorDesigner.Runtime.ExternalBehaviorTree, Assembly-CSharp");
				if (type == null)
				{
					type = Type.GetType("BehaviorDesigner.Runtime.ExternalBehaviorTree, Assembly-CSharp-firstpass");
				}
				if (BehaviorDesignerPreferences.GetBool(BDPreferences.BinarySerialization))
				{
					BinarySerialization.Save(this.mActiveBehaviorSource);
				}
				else
				{
					JSONSerialization.Save(this.mActiveBehaviorSource);
				}
				ExternalBehavior externalBehavior = ScriptableObject.CreateInstance(type) as ExternalBehavior;
				BehaviorSource behaviorSource = new BehaviorSource(externalBehavior);
				behaviorSource.behaviorName = this.mActiveBehaviorSource.behaviorName;
				behaviorSource.behaviorDescription = this.mActiveBehaviorSource.behaviorDescription;
				behaviorSource.set_TaskData(this.mActiveBehaviorSource.get_TaskData());
				externalBehavior.SetBehaviorSource(behaviorSource);
				text = string.Format("Assets/{0}", text.Substring(Application.get_dataPath().get_Length() + 1));
				AssetDatabase.DeleteAsset(text);
				AssetDatabase.CreateAsset(externalBehavior, text);
				AssetDatabase.ImportAsset(text);
				Selection.set_activeObject(externalBehavior);
			}
			else if (Path.GetExtension(text).Equals(".asset"))
			{
				Debug.LogError("Error: Unable to save external behavior tree. The save location must be within the Asset directory.");
			}
		}

		private void SaveAsPrefab()
		{
			if (this.mActiveBehaviorSource == null)
			{
				return;
			}
			string text = EditorUtility.SaveFilePanel("Save Behavior Tree", "Assets", this.mActiveBehaviorSource.behaviorName + ".prefab", "prefab");
			if (text.get_Length() != 0 && Application.get_dataPath().get_Length() < text.get_Length())
			{
				GameObject gameObject = new GameObject();
				Type type = Type.GetType("BehaviorDesigner.Runtime.BehaviorTree, Assembly-CSharp");
				if (type == null)
				{
					type = Type.GetType("BehaviorDesigner.Runtime.BehaviorTree, Assembly-CSharp-firstpass");
				}
				Behavior behavior = gameObject.AddComponent(type) as Behavior;
				BehaviorSource behaviorSource = new BehaviorSource(behavior);
				behaviorSource.behaviorName = this.mActiveBehaviorSource.behaviorName;
				behaviorSource.behaviorDescription = this.mActiveBehaviorSource.behaviorDescription;
				behaviorSource.set_TaskData(this.mActiveBehaviorSource.get_TaskData());
				behavior.SetBehaviorSource(behaviorSource);
				text = string.Format("Assets/{0}", text.Substring(Application.get_dataPath().get_Length() + 1));
				AssetDatabase.DeleteAsset(text);
				GameObject activeObject = PrefabUtility.CreatePrefab(text, gameObject);
				Object.DestroyImmediate(gameObject, true);
				AssetDatabase.ImportAsset(text);
				Selection.set_activeObject(activeObject);
			}
			else if (Path.GetExtension(text).Equals(".prefab"))
			{
				Debug.LogError("Error: Unable to save prefab. The save location must be within the Asset directory.");
			}
		}

		public void LoadBehavior(BehaviorSource behaviorSource, bool loadPrevBehavior)
		{
			this.LoadBehavior(behaviorSource, loadPrevBehavior, false);
		}

		public void LoadBehavior(BehaviorSource behaviorSource, bool loadPrevBehavior, bool inspectorLoad)
		{
			if (behaviorSource == null || object.ReferenceEquals(behaviorSource.get_Owner(), null) || behaviorSource.get_Owner().Equals(null))
			{
				return;
			}
			if (inspectorLoad && !this.mSizesInitialized)
			{
				this.mActiveBehaviorID = behaviorSource.get_Owner().GetInstanceID();
				this.mPrevActiveObject = Selection.get_activeObject();
				this.mLoadedFromInspector = true;
				return;
			}
			if (!this.mSizesInitialized)
			{
				return;
			}
			if (!loadPrevBehavior)
			{
				this.DisableReferenceTasks();
				this.mVariableInspector.ResetSelectedVariableIndex();
			}
			this.mExternalParent = null;
			this.mActiveBehaviorSource = behaviorSource;
			if (behaviorSource.get_Owner() is Behavior)
			{
				this.mActiveObject = (behaviorSource.get_Owner() as Behavior).get_gameObject();
				ExternalBehavior externalBehavior = (behaviorSource.get_Owner() as Behavior).get_ExternalBehavior();
				if (externalBehavior != null && !EditorApplication.get_isPlayingOrWillChangePlaymode())
				{
					this.mActiveBehaviorSource = externalBehavior.get_BehaviorSource();
					this.mActiveBehaviorSource.set_Owner(externalBehavior);
					this.mExternalParent = behaviorSource;
					behaviorSource.CheckForSerialization(true, null);
					if (VariableInspector.SyncVariables(behaviorSource, this.mActiveBehaviorSource.get_Variables()))
					{
						if (BehaviorDesignerPreferences.GetBool(BDPreferences.BinarySerialization))
						{
							BinarySerialization.Save(behaviorSource);
						}
						else
						{
							JSONSerialization.Save(behaviorSource);
						}
					}
				}
			}
			else
			{
				this.mActiveObject = behaviorSource.get_Owner().GetObject();
			}
			this.mActiveBehaviorSource.set_BehaviorID(this.mActiveBehaviorSource.get_Owner().GetInstanceID());
			this.mActiveBehaviorID = this.mActiveBehaviorSource.get_BehaviorID();
			this.mPrevActiveObject = Selection.get_activeObject();
			if (this.mBehaviorSourceHistory.get_Count() == 0 || this.mBehaviorSourceHistoryIndex >= this.mBehaviorSourceHistory.get_Count() || this.mBehaviorSourceHistory.get_Item(this.mBehaviorSourceHistoryIndex) == null || ((this.mBehaviorSourceHistory.get_Item(this.mBehaviorSourceHistoryIndex) as IBehavior).GetBehaviorSource() != null && !this.mActiveBehaviorSource.get_BehaviorID().Equals((this.mBehaviorSourceHistory.get_Item(this.mBehaviorSourceHistoryIndex) as IBehavior).GetBehaviorSource().get_BehaviorID())))
			{
				for (int i = this.mBehaviorSourceHistory.get_Count() - 1; i > this.mBehaviorSourceHistoryIndex; i--)
				{
					this.mBehaviorSourceHistory.RemoveAt(i);
				}
				this.mBehaviorSourceHistory.Add(this.mActiveBehaviorSource.get_Owner().GetObject());
				this.mBehaviorSourceHistoryIndex++;
			}
			Vector2 vector = new Vector2(this.mGraphRect.get_width() / (2f * this.mGraphZoom), 150f);
			vector -= this.mGraphOffset;
			if (this.mGraphDesigner.Load(this.mActiveBehaviorSource, loadPrevBehavior && !this.mLoadedFromInspector, vector) && this.mGraphDesigner.HasEntryNode() && (!loadPrevBehavior || this.mLoadedFromInspector))
			{
				this.mGraphOffset = new Vector2(this.mGraphRect.get_width() / (2f * this.mGraphZoom), 50f) - this.mGraphDesigner.EntryNodeOffset();
				this.mGraphScrollPosition = (this.mGraphScrollSize - new Vector2(this.mGraphRect.get_width(), this.mGraphRect.get_height())) / 2f - 2f * new Vector2(15f, 15f);
			}
			this.mLoadedFromInspector = false;
			if (!this.mLockActiveGameObject)
			{
				Selection.set_activeObject(this.mActiveObject);
			}
			if (EditorApplication.get_isPlaying() && this.mActiveBehaviorSource != null)
			{
				this.mRightClickMenu = null;
				this.mUpdateNodeTaskMap = true;
				this.UpdateNodeTaskMap();
			}
			this.CheckForErrors();
			this.UpdateGraphStatus();
			this.ClearBreadcrumbMenu();
			this.Repaint();
		}

		public void ClearGraph()
		{
			this.mGraphDesigner.Clear(true);
			this.mActiveBehaviorSource = null;
			this.CheckForErrors();
			this.UpdateGraphStatus();
			this.Repaint();
		}
	}
}
