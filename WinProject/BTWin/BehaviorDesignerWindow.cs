// Decompiled with JetBrains decompiler
// Type: BehaviorDesigner.Editor.BehaviorDesignerWindow
// Assembly: BehaviorDesignerEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 99CE4D00-DFA2-42D1-ABFC-D630AB4C1372
// Assembly location: C:\Users\Ron\Desktop\bt\BehaviorDesignerEditor.dll

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
    [SerializeField]
    public static BehaviorDesignerWindow instance;
    private Rect mGraphRect;
    private Rect mGraphScrollRect;
    private Rect mFileToolBarRect;
    private Rect mDebugToolBarRect;
    private Rect mPropertyToolbarRect;
    private Rect mPropertyBoxRect;
    private Rect mPreferencesPaneRect;
    private Vector2 mGraphScrollSize;
    private bool mSizesInitialized;
    private float mPrevScreenWidth;
    private float mPrevScreenHeight;
    private bool mPropertiesPanelOnLeft;
    private Vector2 mCurrentMousePosition;
    private Vector2 mGraphScrollPosition;
    private Vector2 mGraphOffset;
    private float mGraphZoom;
    private int mBehaviorToolbarSelection;
    private string[] mBehaviorToolbarStrings;
    private string mGraphStatus;
    private Material mGridMaterial;
    private int mGUITickCount;
    private Vector2 mSelectStartPosition;
    private Rect mSelectionArea;
    private bool mIsSelecting;
    private bool mIsDragging;
    private bool mKeepTasksSelected;
    private bool mNodeClicked;
    private Vector2 mDragDelta;
    private bool mCommandDown;
    private bool mUpdateNodeTaskMap;
    private bool mStepApplication;
    private Dictionary<NodeDesigner, Task> mNodeDesignerTaskMap;
    private bool mEditorAtBreakpoint;
    [SerializeField]
    private List<BehaviorDesigner.Editor.ErrorDetails> mErrorDetails;
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
    private int mActiveBehaviorID;
    [SerializeField]
    private List<Object> mBehaviorSourceHistory;
    [SerializeField]
    private int mBehaviorSourceHistoryIndex;
    private BehaviorManager mBehaviorManager;
    private bool mLockActiveGameObject;
    private bool mLoadedFromInspector;
    [SerializeField]
    private bool mIsPlaying;
    private WWW mUpdateCheckRequest;
    private DateTime mLastUpdateCheck;
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

    public BehaviorDesignerWindow()
    {
      //base.\u002Ector();
    }

    public List<BehaviorDesigner.Editor.ErrorDetails> ErrorDetails
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
            return this.mLastUpdateCheck;
          this.mLastUpdateCheck = DateTime.Parse(EditorPrefs.GetString("BehaviorDesignerLastUpdateCheck", "1/1/1971 00:00:01"), (IFormatProvider) CultureInfo.InvariantCulture);
        }
        catch (Exception ex)
        {
          this.mLastUpdateCheck = DateTime.UtcNow;
        }
        return this.mLastUpdateCheck;
      }
      set
      {
        this.mLastUpdateCheck = value;
        EditorPrefs.SetString("BehaviorDesignerLastUpdateCheck", this.mLastUpdateCheck.ToString((IFormatProvider) CultureInfo.InvariantCulture));
      }
    }

    public string LatestVersion
    {
      get
      {
        if (!string.IsNullOrEmpty(this.mLatestVersion))
          return this.mLatestVersion;
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
        this.onAddTask += value;
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
        this.onRemoveTask += value;
      }
    }

    [MenuItem("Tools/Behavior Designer/Editor", false, 0)]
    public static void ShowWindow()
    {
      BehaviorDesignerWindow window = (BehaviorDesignerWindow) EditorWindow.GetWindow<BehaviorDesignerWindow>(false, "Behavior Designer");
      window.set_wantsMouseMove(true);
      window.set_minSize(new Vector2(500f, 100f));
      window.Init();
      BehaviorDesignerPreferences.InitPrefernces();
      if (!BehaviorDesignerPreferences.GetBool(BDPreferences.ShowWelcomeScreen))
        return;
      WelcomeScreen.ShowWindow();
    }

    public void OnEnable()
    {
      this.mIsPlaying = EditorApplication.get_isPlaying();
      this.mSizesInitialized = false;
      this.Repaint();
      // ISSUE: method pointer
      EditorApplication.projectWindowChanged = (__Null) Delegate.Combine((Delegate) EditorApplication.projectWindowChanged, (Delegate) new EditorApplication.CallbackFunction((object) this, __methodptr(OnProjectWindowChange)));
      // ISSUE: method pointer
      EditorApplication.playmodeStateChanged = (__Null) Delegate.Combine((Delegate) EditorApplication.playmodeStateChanged, (Delegate) new EditorApplication.CallbackFunction((object) this, __methodptr(OnPlaymodeStateChange)));
      // ISSUE: method pointer
      Undo.undoRedoPerformed = (__Null) Delegate.Combine((Delegate) Undo.undoRedoPerformed, (Delegate) new Undo.UndoRedoCallback((object) this, __methodptr(OnUndoRedo)));
      this.Init();
      this.SetBehaviorManager();
    }

    public void OnFocus()
    {
      BehaviorDesignerWindow.instance = this;
      this.set_wantsMouseMove(true);
      this.Init();
      if (!this.mLockActiveGameObject)
        this.mActiveObject = Selection.get_activeObject();
      this.ReloadPreviousBehavior();
      this.UpdateGraphStatus();
    }

    public void OnSelectionChange()
    {
      if (!this.mLockActiveGameObject)
        this.UpdateTree(false);
      else
        this.ReloadPreviousBehavior();
      this.UpdateGraphStatus();
    }

    public void OnProjectWindowChange()
    {
      this.ReloadPreviousBehavior();
      this.ClearBreadcrumbMenu();
    }

    private void ReloadPreviousBehavior()
    {
      if (Object.op_Inequality(this.mActiveObject, (Object) null))
      {
        if (Object.op_Implicit((Object) (this.mActiveObject as GameObject)))
        {
          GameObject mActiveObject = this.mActiveObject as GameObject;
          int index1 = -1;
          Behavior[] components = (Behavior[]) mActiveObject.GetComponents<Behavior>();
          for (int index2 = 0; index2 < components.Length; ++index2)
          {
            if (((Object) components[index2]).GetInstanceID() == this.mActiveBehaviorID)
            {
              index1 = index2;
              break;
            }
          }
          if (index1 != -1)
            this.LoadBehavior(components[index1].GetBehaviorSource(), true, false);
          else if (((IEnumerable<Behavior>) components).Count<Behavior>() > 0)
          {
            this.LoadBehavior(components[0].GetBehaviorSource(), true, false);
          }
          else
          {
            if (!Object.op_Inequality((Object) this.mGraphDesigner, (Object) null))
              return;
            this.ClearGraph();
          }
        }
        else if (this.mActiveObject is ExternalBehavior)
        {
          ExternalBehavior mActiveObject = this.mActiveObject as ExternalBehavior;
          BehaviorSource behaviorSource = mActiveObject.get_BehaviorSource();
          if (mActiveObject.get_BehaviorSource().get_Owner() == null)
            mActiveObject.get_BehaviorSource().set_Owner((IBehavior) mActiveObject);
          this.LoadBehavior(behaviorSource, true, false);
        }
        else
        {
          if (!Object.op_Inequality((Object) this.mGraphDesigner, (Object) null))
            return;
          this.mActiveObject = (Object) null;
          this.ClearGraph();
        }
      }
      else
      {
        if (!Object.op_Inequality((Object) this.mGraphDesigner, (Object) null))
          return;
        this.ClearGraph();
        this.Repaint();
      }
    }

    private void UpdateTree(bool firstLoad)
    {
      bool flag1 = firstLoad;
      if (Object.op_Inequality(Selection.get_activeObject(), (Object) null))
      {
        bool loadPrevBehavior = false;
        if (!Selection.get_activeObject().Equals((object) this.mActiveObject))
        {
          this.mActiveObject = Selection.get_activeObject();
          flag1 = true;
        }
        BehaviorSource behaviorSource = (BehaviorSource) null;
        GameObject mActiveObject1 = this.mActiveObject as GameObject;
        if (Object.op_Inequality((Object) mActiveObject1, (Object) null) && Object.op_Inequality((Object) mActiveObject1.GetComponent<Behavior>(), (Object) null))
        {
          if (flag1)
          {
            if (this.mActiveObject.Equals((object) this.mPrevActiveObject) && this.mActiveBehaviorID != -1)
            {
              loadPrevBehavior = true;
              int index1 = -1;
              Behavior[] components = (Behavior[]) (this.mActiveObject as GameObject).GetComponents<Behavior>();
              for (int index2 = 0; index2 < components.Length; ++index2)
              {
                if (((Object) components[index2]).GetInstanceID() == this.mActiveBehaviorID)
                {
                  index1 = index2;
                  break;
                }
              }
              if (index1 != -1)
                behaviorSource = ((Behavior) mActiveObject1.GetComponents<Behavior>()[index1]).GetBehaviorSource();
              else if (((IEnumerable<Behavior>) components).Count<Behavior>() > 0)
                behaviorSource = ((Behavior) mActiveObject1.GetComponents<Behavior>()[0]).GetBehaviorSource();
            }
            else
              behaviorSource = ((Behavior) mActiveObject1.GetComponents<Behavior>()[0]).GetBehaviorSource();
          }
          else
          {
            Behavior[] components = (Behavior[]) mActiveObject1.GetComponents<Behavior>();
            bool flag2 = false;
            if (this.mActiveBehaviorSource != null)
            {
              for (int index = 0; index < components.Length; ++index)
              {
                if (((Object) components[index]).Equals((object) this.mActiveBehaviorSource.get_Owner()))
                {
                  flag2 = true;
                  break;
                }
              }
            }
            if (!flag2)
            {
              behaviorSource = ((Behavior) mActiveObject1.GetComponents<Behavior>()[0]).GetBehaviorSource();
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
          ExternalBehavior mActiveObject2 = this.mActiveObject as ExternalBehavior;
          if (mActiveObject2.get_BehaviorSource().get_Owner() == null)
            mActiveObject2.get_BehaviorSource().set_Owner((IBehavior) mActiveObject2);
          if (flag1 && this.mActiveObject.Equals((object) this.mPrevActiveObject))
            loadPrevBehavior = true;
          behaviorSource = mActiveObject2.get_BehaviorSource();
        }
        else
          this.mPrevActiveObject = (Object) null;
        if (behaviorSource != null)
        {
          this.LoadBehavior(behaviorSource, loadPrevBehavior, false);
        }
        else
        {
          if (behaviorSource != null)
            return;
          this.ClearGraph();
        }
      }
      else
      {
        if (Object.op_Inequality(this.mActiveObject, (Object) null) && this.mActiveBehaviorSource != null)
          this.mPrevActiveObject = this.mActiveObject;
        this.mActiveObject = (Object) null;
        this.ClearGraph();
      }
    }

    private void Init()
    {
      if (Object.op_Equality((Object) this.mTaskList, (Object) null))
        this.mTaskList = (TaskList) ScriptableObject.CreateInstance<TaskList>();
      if (Object.op_Equality((Object) this.mVariableInspector, (Object) null))
        this.mVariableInspector = (VariableInspector) ScriptableObject.CreateInstance<VariableInspector>();
      if (Object.op_Equality((Object) this.mGraphDesigner, (Object) null))
        this.mGraphDesigner = (GraphDesigner) ScriptableObject.CreateInstance<GraphDesigner>();
      if (Object.op_Equality((Object) this.mTaskInspector, (Object) null))
        this.mTaskInspector = (TaskInspector) ScriptableObject.CreateInstance<TaskInspector>();
      if (Object.op_Equality((Object) this.mGridMaterial, (Object) null))
      {
        this.mGridMaterial = new Material(Shader.Find("Hidden/Behavior Designer/Grid"));
        ((Object) this.mGridMaterial).set_hideFlags((HideFlags) 61);
        ((Object) this.mGridMaterial.get_shader()).set_hideFlags((HideFlags) 61);
      }
      this.mTaskList.Init();
      FieldInspector.Init();
      this.ClearBreadcrumbMenu();
    }

    public void UpdateGraphStatus()
    {
      if (Object.op_Equality(this.mActiveObject, (Object) null) || Object.op_Equality((Object) this.mGraphDesigner, (Object) null) || Object.op_Equality((Object) (this.mActiveObject as GameObject), (Object) null) && Object.op_Equality((Object) (this.mActiveObject as ExternalBehavior), (Object) null))
        this.mGraphStatus = "Select a GameObject";
      else if (Object.op_Inequality((Object) (this.mActiveObject as GameObject), (Object) null) && object.ReferenceEquals((object) (this.mActiveObject as GameObject).GetComponent<Behavior>(), (object) null))
        this.mGraphStatus = "Right Click, Add a Behavior Tree Component";
      else if (this.ViewOnlyMode() && this.mActiveBehaviorSource != null)
      {
        ExternalBehavior externalBehavior = (this.mActiveBehaviorSource.get_Owner().GetObject() as Behavior).get_ExternalBehavior();
        if (Object.op_Inequality((Object) externalBehavior, (Object) null))
          this.mGraphStatus = externalBehavior.get_BehaviorSource().ToString() + " (View Only Mode)";
        else
          this.mGraphStatus = this.mActiveBehaviorSource.ToString() + " (View Only Mode)";
      }
      else if (!this.mGraphDesigner.HasEntryNode())
        this.mGraphStatus = "Add a Task";
      else if (this.IsReferencingTasks())
      {
        this.mGraphStatus = "Select tasks to reference (right click to exit)";
      }
      else
      {
        if (this.mActiveBehaviorSource == null || this.mActiveBehaviorSource.get_Owner() == null || !Object.op_Inequality(this.mActiveBehaviorSource.get_Owner().GetObject(), (Object) null))
          return;
        if (this.mExternalParent != null)
          this.mGraphStatus = this.mExternalParent.ToString() + " (Editing External Behavior)";
        else
          this.mGraphStatus = this.mActiveBehaviorSource.ToString();
      }
    }

    private void BuildBreadcrumbMenus(BehaviorDesignerWindow.BreadcrumbMenuType menuType)
    {
      Dictionary<BehaviorSource, string> dictionary1 = new Dictionary<BehaviorSource, string>();
      Dictionary<string, int> dictionary2 = new Dictionary<string, int>();
      HashSet<Object> objectSet = new HashSet<Object>();
      List<BehaviorSource> behaviorSourceList = new List<BehaviorSource>();
      Behavior[] objectsOfTypeAll1 = Resources.FindObjectsOfTypeAll(typeof (Behavior)) as Behavior[];
      for (int index = objectsOfTypeAll1.Length - 1; index > -1; --index)
      {
        BehaviorSource behaviorSource = objectsOfTypeAll1[index].GetBehaviorSource();
        if (behaviorSource.get_Owner() == null)
          behaviorSource.set_Owner((IBehavior) objectsOfTypeAll1[index]);
        behaviorSourceList.Add(behaviorSource);
      }
      ExternalBehavior[] objectsOfTypeAll2 = Resources.FindObjectsOfTypeAll(typeof (ExternalBehavior)) as ExternalBehavior[];
      for (int index = objectsOfTypeAll2.Length - 1; index > -1; --index)
      {
        BehaviorSource behaviorSource = objectsOfTypeAll2[index].GetBehaviorSource();
        if (behaviorSource.get_Owner() == null)
          behaviorSource.set_Owner((IBehavior) objectsOfTypeAll2[index]);
        behaviorSourceList.Add(behaviorSource);
      }
      behaviorSourceList.Sort((IComparer<BehaviorSource>) new AlphanumComparator<BehaviorSource>());
      for (int index = 0; index < behaviorSourceList.Count; ++index)
      {
        Object @object = behaviorSourceList[index].get_Owner().GetObject();
        if (menuType == BehaviorDesignerWindow.BreadcrumbMenuType.Behavior)
        {
          if (@object is Behavior)
          {
            if (!((Object) ((Component) (@object as Behavior)).get_gameObject()).Equals((object) this.mActiveObject))
              continue;
          }
          else if (!((Object) (@object as ExternalBehavior)).Equals((object) this.mActiveObject))
            continue;
        }
        if (menuType == BehaviorDesignerWindow.BreadcrumbMenuType.GameObject && @object is Behavior)
        {
          if (!objectSet.Contains((Object) ((Component) (@object as Behavior)).get_gameObject()))
            objectSet.Add((Object) ((Component) (@object as Behavior)).get_gameObject());
          else
            continue;
        }
        string key = string.Empty;
        if (@object is Behavior)
        {
          switch (menuType)
          {
            case BehaviorDesignerWindow.BreadcrumbMenuType.GameObjectBehavior:
              key = behaviorSourceList[index].ToString();
              break;
            case BehaviorDesignerWindow.BreadcrumbMenuType.GameObject:
              key = ((Object) ((Component) (@object as Behavior)).get_gameObject()).get_name();
              break;
            case BehaviorDesignerWindow.BreadcrumbMenuType.Behavior:
              key = (string) behaviorSourceList[index].behaviorName;
              break;
          }
          if (!AssetDatabase.GetAssetPath(@object).Equals(string.Empty))
            key += " (prefab)";
        }
        else
          key = behaviorSourceList[index].ToString() + " (external)";
        int num1 = 0;
        if (dictionary2.TryGetValue(key, out num1))
        {
          int num2;
          dictionary2[key] = num2 = num1 + 1;
          key += string.Format(" ({0})", (object) (num2 + 1));
        }
        else
          dictionary2.Add(key, 0);
        dictionary1.Add(behaviorSourceList[index], key);
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
      using (Dictionary<BehaviorSource, string>.Enumerator enumerator = dictionary1.GetEnumerator())
      {
        while (enumerator.MoveNext())
        {
          KeyValuePair<BehaviorSource, string> current = enumerator.Current;
          switch (menuType)
          {
            case BehaviorDesignerWindow.BreadcrumbMenuType.GameObjectBehavior:
              // ISSUE: method pointer
              this.mBreadcrumbGameObjectBehaviorMenu.AddItem(new GUIContent(current.Value), ((object) current.Key).Equals((object) this.mActiveBehaviorSource), new GenericMenu.MenuFunction2((object) this, __methodptr(BehaviorSelectionCallback)), (object) current.Key);
              continue;
            case BehaviorDesignerWindow.BreadcrumbMenuType.GameObject:
              bool flag = !(current.Key.get_Owner().GetObject() is ExternalBehavior) ? ((Object) ((Component) (current.Key.get_Owner().GetObject() as Behavior)).get_gameObject()).Equals((object) this.mActiveObject) : (current.Key.get_Owner().GetObject() as ExternalBehavior).GetObject().Equals((object) this.mActiveObject);
              // ISSUE: method pointer
              this.mBreadcrumbGameObjectMenu.AddItem(new GUIContent(current.Value), flag, new GenericMenu.MenuFunction2((object) this, __methodptr(BehaviorSelectionCallback)), (object) current.Key);
              continue;
            case BehaviorDesignerWindow.BreadcrumbMenuType.Behavior:
              // ISSUE: method pointer
              this.mBreadcrumbBehaviorMenu.AddItem(new GUIContent(current.Value), ((object) current.Key).Equals((object) this.mActiveBehaviorSource), new GenericMenu.MenuFunction2((object) this, __methodptr(BehaviorSelectionCallback)), (object) current.Key);
              continue;
            default:
              continue;
          }
        }
      }
    }

    private void ClearBreadcrumbMenu()
    {
      this.mBreadcrumbGameObjectBehaviorMenu = (GenericMenu) null;
      this.mBreadcrumbGameObjectMenu = (GenericMenu) null;
      this.mBreadcrumbBehaviorMenu = (GenericMenu) null;
    }

    private void BuildRightClickMenu(NodeDesigner clickedNode)
    {
      if (Object.op_Equality(this.mActiveObject, (Object) null))
        return;
      this.mRightClickMenu = new GenericMenu();
      this.mShowRightClickMenu = true;
      if (Object.op_Equality((Object) clickedNode, (Object) null) && !EditorApplication.get_isPlaying() && !this.ViewOnlyMode())
      {
        // ISSUE: method pointer
        this.mTaskList.AddTasksToMenu(ref this.mRightClickMenu, (Type) null, "Add Task", new GenericMenu.MenuFunction2((object) this, __methodptr(AddTaskCallback)));
        if (this.mCopiedTasks != null && this.mCopiedTasks.Count > 0)
        {
          // ISSUE: method pointer
          this.mRightClickMenu.AddItem(new GUIContent("Paste Tasks"), false, new GenericMenu.MenuFunction((object) this, __methodptr(PasteNodes)));
        }
        else
          this.mRightClickMenu.AddDisabledItem(new GUIContent("Paste Tasks"));
      }
      if (Object.op_Inequality((Object) clickedNode, (Object) null) && !clickedNode.IsEntryDisplay)
      {
        if (this.mGraphDesigner.SelectedNodes.Count == 1)
        {
          // ISSUE: method pointer
          this.mRightClickMenu.AddItem(new GUIContent("Edit Script"), false, new GenericMenu.MenuFunction2((object) this, __methodptr(OpenInFileEditor)), (object) clickedNode);
          // ISSUE: method pointer
          this.mRightClickMenu.AddItem(new GUIContent("Locate Script"), false, new GenericMenu.MenuFunction2((object) this, __methodptr(SelectInProject)), (object) clickedNode);
          if (!this.ViewOnlyMode())
          {
            // ISSUE: method pointer
            this.mRightClickMenu.AddItem(new GUIContent(!clickedNode.Task.get_Disabled() ? "Disable" : "Enable"), false, new GenericMenu.MenuFunction2((object) this, __methodptr(ToggleEnableState)), (object) clickedNode);
            if (clickedNode.IsParent)
            {
              // ISSUE: method pointer
              this.mRightClickMenu.AddItem(new GUIContent(!clickedNode.Task.get_NodeData().get_Collapsed() ? "Collapse" : "Expand"), false, new GenericMenu.MenuFunction2((object) this, __methodptr(ToggleCollapseState)), (object) clickedNode);
            }
            // ISSUE: method pointer
            this.mRightClickMenu.AddItem(new GUIContent(!clickedNode.Task.get_NodeData().get_IsBreakpoint() ? "Set Breakpoint" : "Remove Breakpoint"), false, new GenericMenu.MenuFunction2((object) this, __methodptr(ToggleBreakpoint)), (object) clickedNode);
            // ISSUE: method pointer
            this.mTaskList.AddTasksToMenu(ref this.mRightClickMenu, ((object) this.mGraphDesigner.SelectedNodes[0].Task).GetType(), "Replace", new GenericMenu.MenuFunction2((object) this, __methodptr(ReplaceTaskCallback)));
          }
        }
        if (!EditorApplication.get_isPlaying() && !this.ViewOnlyMode())
        {
          // ISSUE: method pointer
          this.mRightClickMenu.AddItem(new GUIContent(string.Format("Copy Task{0}", this.mGraphDesigner.SelectedNodes.Count <= 1 ? (object) string.Empty : (object) "s")), false, new GenericMenu.MenuFunction((object) this, __methodptr(CopyNodes)));
          if (this.mCopiedTasks != null && this.mCopiedTasks.Count > 0)
          {
            // ISSUE: method pointer
            this.mRightClickMenu.AddItem(new GUIContent(string.Format("Paste Task{0}", this.mCopiedTasks.Count <= 1 ? (object) string.Empty : (object) "s")), false, new GenericMenu.MenuFunction((object) this, __methodptr(PasteNodes)));
          }
          else
            this.mRightClickMenu.AddDisabledItem(new GUIContent("Paste Tasks"));
          // ISSUE: method pointer
          this.mRightClickMenu.AddItem(new GUIContent(string.Format("Delete Task{0}", this.mGraphDesigner.SelectedNodes.Count <= 1 ? (object) string.Empty : (object) "s")), false, new GenericMenu.MenuFunction((object) this, __methodptr(DeleteNodes)));
        }
      }
      if (EditorApplication.get_isPlaying() || !Object.op_Inequality((Object) (this.mActiveObject as GameObject), (Object) null))
        return;
      if (Object.op_Inequality((Object) clickedNode, (Object) null) && !clickedNode.IsEntryDisplay)
        this.mRightClickMenu.AddSeparator(string.Empty);
      // ISSUE: method pointer
      this.mRightClickMenu.AddItem(new GUIContent("Add Behavior Tree"), false, new GenericMenu.MenuFunction((object) this, __methodptr(AddBehavior)));
      if (this.mActiveBehaviorSource == null)
        return;
      // ISSUE: method pointer
      this.mRightClickMenu.AddItem(new GUIContent("Remove Behavior Tree"), false, new GenericMenu.MenuFunction((object) this, __methodptr(RemoveBehavior)));
      // ISSUE: method pointer
      this.mRightClickMenu.AddItem(new GUIContent("Save As External Behavior Tree"), false, new GenericMenu.MenuFunction((object) this, __methodptr(SaveAsAsset)));
    }

    public void Update()
    {
      if (!this.mTakingScreenshot)
        return;
      this.Repaint();
    }

    public void OnGUI()
    {
      this.mCurrentMousePosition = Event.get_current().get_mousePosition();
      this.SetupSizes();
      if (!this.mSizesInitialized)
      {
        this.mSizesInitialized = true;
        if (!this.mLockActiveGameObject || Object.op_Equality(this.mActiveObject, (Object) null))
          this.UpdateTree(true);
        else
          this.ReloadPreviousBehavior();
      }
      if (this.Draw() && this.mGUITickCount > 1)
      {
        this.Repaint();
        this.mGUITickCount = 0;
      }
      this.HandleEvents();
      ++this.mGUITickCount;
    }

    public void OnPlaymodeStateChange()
    {
      if (EditorApplication.get_isPlaying() && !EditorApplication.get_isPaused())
      {
        if (Object.op_Equality((Object) this.mBehaviorManager, (Object) null))
        {
          this.SetBehaviorManager();
          if (Object.op_Equality((Object) this.mBehaviorManager, (Object) null))
            return;
        }
        if (!Object.op_Inequality((Object) this.mBehaviorManager.get_BreakpointTree(), (Object) null) || !this.mEditorAtBreakpoint)
          return;
        this.mEditorAtBreakpoint = false;
        this.mBehaviorManager.set_BreakpointTree((Behavior) null);
      }
      else if (EditorApplication.get_isPlaying() && EditorApplication.get_isPaused())
      {
        if (!Object.op_Inequality((Object) this.mBehaviorManager, (Object) null) || !Object.op_Inequality((Object) this.mBehaviorManager.get_BreakpointTree(), (Object) null))
          return;
        if (!this.mEditorAtBreakpoint)
        {
          this.mEditorAtBreakpoint = true;
          if (!BehaviorDesignerPreferences.GetBool(BDPreferences.SelectOnBreakpoint) || this.mLockActiveGameObject)
            return;
          Selection.set_activeObject((Object) this.mBehaviorManager.get_BreakpointTree());
          this.LoadBehavior(this.mBehaviorManager.get_BreakpointTree().GetBehaviorSource(), this.mActiveBehaviorSource == this.mBehaviorManager.get_BreakpointTree().GetBehaviorSource(), false);
        }
        else
        {
          this.mEditorAtBreakpoint = false;
          this.mBehaviorManager.set_BreakpointTree((Behavior) null);
        }
      }
      else
      {
        if (EditorApplication.get_isPlaying())
          return;
        this.mBehaviorManager = (BehaviorManager) null;
      }
    }

    private void SetBehaviorManager()
    {
      this.mBehaviorManager = (BehaviorManager) BehaviorManager.instance;
      if (Object.op_Equality((Object) this.mBehaviorManager, (Object) null))
        return;
      BehaviorManager mBehaviorManager = this.mBehaviorManager;
      // ISSUE: method pointer
      mBehaviorManager.set_OnTaskBreakpoint((BehaviorManager.BehaviorManagerHandler) Delegate.Combine((Delegate) mBehaviorManager.get_OnTaskBreakpoint(), (Delegate) new BehaviorManager.BehaviorManagerHandler((object) this, __methodptr(OnTaskBreakpoint))));
      this.mUpdateNodeTaskMap = true;
    }

    public void OnTaskBreakpoint()
    {
      EditorApplication.set_isPaused(true);
      this.Repaint();
    }

    private void OnPreferenceChange(BDPreferences pref, object value)
    {
      BDPreferences bdPreferences = pref;
      switch (bdPreferences)
      {
        case BDPreferences.CompactMode:
          this.mGraphDesigner.GraphDirty();
          break;
        case BDPreferences.BinarySerialization:
          this.SaveBehavior();
          break;
        case BDPreferences.ErrorChecking:
          this.CheckForErrors();
          break;
        default:
          if (bdPreferences != BDPreferences.ShowSceneIcon && bdPreferences != BDPreferences.GizmosViewMode)
            break;
          GizmoManager.UpdateAllGizmos();
          break;
      }
    }

    public void OnInspectorUpdate()
    {
      if (this.mStepApplication)
      {
        EditorApplication.Step();
        this.mStepApplication = false;
      }
      if (EditorApplication.get_isPlaying() && !EditorApplication.get_isPaused() && (this.mActiveBehaviorSource != null && Object.op_Inequality((Object) this.mBehaviorManager, (Object) null)))
      {
        if (this.mUpdateNodeTaskMap)
          this.UpdateNodeTaskMap();
        if (Object.op_Inequality((Object) this.mBehaviorManager.get_BreakpointTree(), (Object) null))
          this.mBehaviorManager.set_BreakpointTree((Behavior) null);
        this.Repaint();
      }
      if (Application.get_isPlaying() && Object.op_Equality((Object) this.mBehaviorManager, (Object) null))
        this.SetBehaviorManager();
      if (Object.op_Inequality((Object) this.mBehaviorManager, (Object) null) && this.mBehaviorManager.get_Dirty())
      {
        if (this.mActiveBehaviorSource != null)
          this.LoadBehavior(this.mActiveBehaviorSource, true, false);
        this.mBehaviorManager.set_Dirty(false);
      }
      if (!EditorApplication.get_isPlaying() && this.mIsPlaying)
        this.ReloadPreviousBehavior();
      this.mIsPlaying = EditorApplication.get_isPlaying();
      this.UpdateGraphStatus();
      this.UpdateCheck();
    }

    private void UpdateNodeTaskMap()
    {
      if (!this.mUpdateNodeTaskMap || !Object.op_Inequality((Object) this.mBehaviorManager, (Object) null))
        return;
      List<Task> taskList = this.mBehaviorManager.GetTaskList(this.mActiveBehaviorSource.get_Owner() as Behavior);
      if (taskList == null)
        return;
      this.mNodeDesignerTaskMap = new Dictionary<NodeDesigner, Task>();
      for (int index = 0; index < taskList.Count; ++index)
      {
        NodeDesigner nodeDesigner = taskList[index].get_NodeData().get_NodeDesigner() as NodeDesigner;
        if (Object.op_Inequality((Object) nodeDesigner, (Object) null) && !this.mNodeDesignerTaskMap.ContainsKey(nodeDesigner))
          this.mNodeDesignerTaskMap.Add(nodeDesigner, taskList[index]);
      }
      this.mUpdateNodeTaskMap = false;
    }

    private bool Draw()
    {
      bool flag = false;
      Color color = GUI.get_color();
      Color backgroundColor = GUI.get_backgroundColor();
      GUI.set_color(Color.get_white());
      GUI.set_backgroundColor(Color.get_white());
      this.DrawFileToolbar();
      this.DrawDebugToolbar();
      this.DrawPropertiesBox();
      if (this.DrawGraphArea())
        flag = true;
      this.DrawPreferencesPane();
      if (this.mTakingScreenshot)
      {
        Rect position1 = this.get_position();
        double width = (double) ((Rect) ref position1).get_width();
        Rect position2 = this.get_position();
        double num = (double) ((Rect) ref position2).get_height() + 22.0;
        GUI.DrawTexture(new Rect(0.0f, 0.0f, (float) width, (float) num), (Texture) BehaviorDesignerUtility.ScreenshotBackgroundTexture, (ScaleMode) 0, false);
      }
      GUI.set_color(color);
      GUI.set_backgroundColor(backgroundColor);
      return flag;
    }

    private void DrawFileToolbar()
    {
      GUILayout.BeginArea(this.mFileToolBarRect, EditorStyles.get_toolbar());
      GUILayout.BeginHorizontal(new GUILayoutOption[0]);
      if (GUILayout.Button((Texture) BehaviorDesignerUtility.HistoryBackwardTexture, EditorStyles.get_toolbarButton(), new GUILayoutOption[0]) && (this.mBehaviorSourceHistoryIndex > 0 || this.mActiveBehaviorSource == null && this.mBehaviorSourceHistoryIndex == 0))
      {
        BehaviorSource behaviorSource = (BehaviorSource) null;
        if (this.mActiveBehaviorSource == null)
          ++this.mBehaviorSourceHistoryIndex;
        while (behaviorSource == null && this.mBehaviorSourceHistory.Count > 0 && this.mBehaviorSourceHistoryIndex > 0)
        {
          --this.mBehaviorSourceHistoryIndex;
          behaviorSource = this.BehaviorSourceFromIBehaviorHistory(this.mBehaviorSourceHistory[this.mBehaviorSourceHistoryIndex] as IBehavior);
          if (behaviorSource == null || behaviorSource.get_Owner() == null || Object.op_Equality(behaviorSource.get_Owner().GetObject(), (Object) null))
          {
            this.mBehaviorSourceHistory.RemoveAt(this.mBehaviorSourceHistoryIndex);
            behaviorSource = (BehaviorSource) null;
          }
        }
        if (behaviorSource != null)
          this.LoadBehavior(behaviorSource, false);
      }
      if (GUILayout.Button((Texture) BehaviorDesignerUtility.HistoryForwardTexture, EditorStyles.get_toolbarButton(), new GUILayoutOption[0]))
      {
        BehaviorSource behaviorSource = (BehaviorSource) null;
        if (this.mBehaviorSourceHistoryIndex < this.mBehaviorSourceHistory.Count - 1)
        {
          ++this.mBehaviorSourceHistoryIndex;
          while (behaviorSource == null && this.mBehaviorSourceHistoryIndex < this.mBehaviorSourceHistory.Count && this.mBehaviorSourceHistoryIndex > 0)
          {
            behaviorSource = this.BehaviorSourceFromIBehaviorHistory(this.mBehaviorSourceHistory[this.mBehaviorSourceHistoryIndex] as IBehavior);
            if (behaviorSource == null || behaviorSource.get_Owner() == null || Object.op_Equality(behaviorSource.get_Owner().GetObject(), (Object) null))
            {
              this.mBehaviorSourceHistory.RemoveAt(this.mBehaviorSourceHistoryIndex);
              behaviorSource = (BehaviorSource) null;
            }
          }
        }
        if (behaviorSource != null)
          this.LoadBehavior(behaviorSource, false);
      }
      if (GUILayout.Button("...", EditorStyles.get_toolbarButton(), new GUILayoutOption[1]
      {
        GUILayout.Width(22f)
      }))
      {
        this.BuildBreadcrumbMenus(BehaviorDesignerWindow.BreadcrumbMenuType.GameObjectBehavior);
        this.mBreadcrumbGameObjectBehaviorMenu.ShowAsContext();
      }
      if (GUILayout.Button(Object.op_Inequality((Object) (this.mActiveObject as GameObject), (Object) null) || Object.op_Inequality((Object) (this.mActiveObject as ExternalBehavior), (Object) null) ? this.mActiveObject.get_name() : "(None Selected)", EditorStyles.get_toolbarPopup(), new GUILayoutOption[1]
      {
        GUILayout.Width(140f)
      }))
      {
        this.BuildBreadcrumbMenus(BehaviorDesignerWindow.BreadcrumbMenuType.GameObject);
        this.mBreadcrumbGameObjectMenu.ShowAsContext();
      }
      if (GUILayout.Button(this.mActiveBehaviorSource == null ? "(None Selected)" : (string) this.mActiveBehaviorSource.behaviorName, EditorStyles.get_toolbarPopup(), new GUILayoutOption[1]
      {
        GUILayout.Width(140f)
      }) && this.mActiveBehaviorSource != null)
      {
        this.BuildBreadcrumbMenus(BehaviorDesignerWindow.BreadcrumbMenuType.Behavior);
        this.mBreadcrumbBehaviorMenu.ShowAsContext();
      }
      if (GUILayout.Button("Referenced Behaviors", EditorStyles.get_toolbarPopup(), new GUILayoutOption[1]
      {
        GUILayout.Width(140f)
      }) && this.mActiveBehaviorSource != null)
      {
        List<BehaviorSource> referencedBehaviors = this.mGraphDesigner.FindReferencedBehaviors();
        if (referencedBehaviors.Count > 0)
        {
          referencedBehaviors.Sort((IComparer<BehaviorSource>) new AlphanumComparator<BehaviorSource>());
          this.mReferencedBehaviorsMenu = new GenericMenu();
          for (int index = 0; index < referencedBehaviors.Count; ++index)
          {
            // ISSUE: method pointer
            this.mReferencedBehaviorsMenu.AddItem(new GUIContent(referencedBehaviors[index].ToString()), false, new GenericMenu.MenuFunction2((object) this, __methodptr(BehaviorSelectionCallback)), (object) referencedBehaviors[index]);
          }
          this.mReferencedBehaviorsMenu.ShowAsContext();
        }
      }
      if (GUILayout.Button("-", EditorStyles.get_toolbarButton(), new GUILayoutOption[1]
      {
        GUILayout.Width(22f)
      }))
      {
        if (this.mActiveBehaviorSource != null)
          this.RemoveBehavior();
        else
          EditorUtility.DisplayDialog("Unable to Remove Behavior Tree", "No behavior tree selected.", "OK");
      }
      if (GUILayout.Button("+", EditorStyles.get_toolbarButton(), new GUILayoutOption[1]
      {
        GUILayout.Width(22f)
      }))
      {
        if (Object.op_Inequality(this.mActiveObject, (Object) null))
          this.AddBehavior();
        else
          EditorUtility.DisplayDialog("Unable to Add Behavior Tree", "No GameObject is selected.", "OK");
      }
      if (GUILayout.Button("Lock", !this.mLockActiveGameObject ? EditorStyles.get_toolbarButton() : BehaviorDesignerUtility.ToolbarButtonSelectionGUIStyle, new GUILayoutOption[1]
      {
        GUILayout.Width(42f)
      }))
      {
        if (Object.op_Inequality(this.mActiveObject, (Object) null))
        {
          this.mLockActiveGameObject = !this.mLockActiveGameObject;
          if (!this.mLockActiveGameObject)
            this.UpdateTree(false);
        }
        else if (this.mLockActiveGameObject)
          this.mLockActiveGameObject = false;
        else
          EditorUtility.DisplayDialog("Unable to Lock GameObject", "No GameObject is selected.", "OK");
      }
      GUI.set_enabled(this.mActiveBehaviorSource == null || this.mExternalParent == null);
      if (GUILayout.Button("Export", EditorStyles.get_toolbarButton(), new GUILayoutOption[1]
      {
        GUILayout.Width(46f)
      }))
      {
        if (this.mActiveBehaviorSource != null)
        {
          if (Object.op_Implicit((Object) (this.mActiveBehaviorSource.get_Owner().GetObject() as Behavior)))
            this.SaveAsAsset();
          else
            this.SaveAsPrefab();
        }
        else
          EditorUtility.DisplayDialog("Unable to Save Behavior Tree", "Select a behavior tree from within the scene.", "OK");
      }
      GUI.set_enabled(true);
      if (GUILayout.Button("Take Screenshot", EditorStyles.get_toolbarButton(), new GUILayoutOption[1]
      {
        GUILayout.Width(96f)
      }))
      {
        if (this.mActiveBehaviorSource != null)
          this.TakeScreenshot();
        else
          EditorUtility.DisplayDialog("Unable to Take Screenshot", "Select a behavior tree from within the scene.", "OK");
      }
      GUILayout.FlexibleSpace();
      if (GUILayout.Button("Preferences", !this.mShowPrefPane ? EditorStyles.get_toolbarButton() : BehaviorDesignerUtility.ToolbarButtonSelectionGUIStyle, new GUILayoutOption[1]
      {
        GUILayout.Width(80f)
      }))
        this.mShowPrefPane = !this.mShowPrefPane;
      GUILayout.EndVertical();
      GUILayout.EndArea();
    }

    private void DrawDebugToolbar()
    {
      GUILayout.BeginArea(this.mDebugToolBarRect, EditorStyles.get_toolbar());
      GUILayout.BeginHorizontal(new GUILayoutOption[0]);
      if (GUILayout.Button((Texture) BehaviorDesignerUtility.PlayTexture, !EditorApplication.get_isPlaying() ? EditorStyles.get_toolbarButton() : BehaviorDesignerUtility.ToolbarButtonSelectionGUIStyle, new GUILayoutOption[1]
      {
        GUILayout.Width(40f)
      }))
        EditorApplication.set_isPlaying(!EditorApplication.get_isPlaying());
      if (GUILayout.Button((Texture) BehaviorDesignerUtility.PauseTexture, !EditorApplication.get_isPaused() ? EditorStyles.get_toolbarButton() : BehaviorDesignerUtility.ToolbarButtonSelectionGUIStyle, new GUILayoutOption[1]
      {
        GUILayout.Width(40f)
      }))
        EditorApplication.set_isPaused(!EditorApplication.get_isPaused());
      if (GUILayout.Button((Texture) BehaviorDesignerUtility.StepTexture, EditorStyles.get_toolbarButton(), new GUILayoutOption[1]
      {
        GUILayout.Width(40f)
      }) && EditorApplication.get_isPlaying())
        this.mStepApplication = true;
      if (this.mErrorDetails != null && this.mErrorDetails.Count > 0)
      {
        if (GUILayout.Button(new GUIContent(this.mErrorDetails.Count.ToString() + " Error" + (this.mErrorDetails.Count <= 1 ? (object) string.Empty : (object) "s"), (Texture) BehaviorDesignerUtility.SmallErrorIconTexture), BehaviorDesignerUtility.ToolbarButtonLeftAlignGUIStyle, new GUILayoutOption[1]
        {
          GUILayout.Width(85f)
        }))
          ErrorWindow.ShowWindow();
      }
      GUILayout.FlexibleSpace();
      if (new Version("1.5.11").CompareTo(new Version(this.LatestVersion)) < 0)
        GUILayout.Label("Behavior Designer " + this.LatestVersion + " is now available.", BehaviorDesignerUtility.ToolbarLabelGUIStyle, new GUILayoutOption[0]);
      GUILayout.EndHorizontal();
      GUILayout.EndArea();
    }

    private void DrawPreferencesPane()
    {
      if (!this.mShowPrefPane)
        return;
      GUILayout.BeginArea(this.mPreferencesPaneRect, BehaviorDesignerUtility.PreferencesPaneGUIStyle);
      BehaviorDesignerPreferences.DrawPreferencesPane(new PreferenceChangeHandler(this.OnPreferenceChange));
      GUILayout.EndArea();
    }

    private void DrawPropertiesBox()
    {
      GUILayout.BeginArea(this.mPropertyToolbarRect, EditorStyles.get_toolbar());
      int toolbarSelection = this.mBehaviorToolbarSelection;
      this.mBehaviorToolbarSelection = GUILayout.Toolbar(this.mBehaviorToolbarSelection, this.mBehaviorToolbarStrings, EditorStyles.get_toolbarButton(), new GUILayoutOption[0]);
      GUILayout.EndArea();
      GUILayout.BeginArea(this.mPropertyBoxRect, BehaviorDesignerUtility.PropertyBoxGUIStyle);
      if (this.mBehaviorToolbarSelection == 0)
      {
        if (this.mActiveBehaviorSource != null)
        {
          GUILayout.Space(3f);
          BehaviorSource behaviorSource = this.mExternalParent == null ? this.mActiveBehaviorSource : this.mExternalParent;
          if (Object.op_Inequality((Object) (behaviorSource.get_Owner() as Behavior), (Object) null))
          {
            bool externalModification = false;
            bool flag = false;
            if (BehaviorInspector.DrawInspectorGUI(behaviorSource.get_Owner() as Behavior, new SerializedObject((Object) (behaviorSource.get_Owner() as Behavior)), false, ref externalModification, ref flag, ref flag))
            {
              BehaviorDesignerUtility.SetObjectDirty(behaviorSource.get_Owner().GetObject());
              if (externalModification)
                this.LoadBehavior(behaviorSource, false, false);
            }
          }
          else
          {
            bool showVariables = false;
            ExternalBehaviorInspector.DrawInspectorGUI(behaviorSource, false, ref showVariables);
          }
        }
        else
        {
          GUILayout.Space(5f);
          GUILayout.Label("No behavior tree selected. Create a new behavior tree or select one from the hierarchy.", BehaviorDesignerUtility.LabelWrapGUIStyle, new GUILayoutOption[1]
          {
            GUILayout.Width(285f)
          });
        }
      }
      else if (this.mBehaviorToolbarSelection == 1)
      {
        this.mTaskList.DrawTaskList(this, !this.ViewOnlyMode());
        if (toolbarSelection != 1)
          this.mTaskList.FocusSearchField();
      }
      else if (this.mBehaviorToolbarSelection == 2)
      {
        if (this.mActiveBehaviorSource != null)
        {
          if (this.mVariableInspector.DrawVariables(this.mExternalParent == null ? this.mActiveBehaviorSource : this.mExternalParent))
            this.SaveBehavior();
          if (toolbarSelection != 2)
            this.mVariableInspector.FocusNameField();
        }
        else
        {
          GUILayout.Space(5f);
          GUILayout.Label("No behavior tree selected. Create a new behavior tree or select one from the hierarchy.", BehaviorDesignerUtility.LabelWrapGUIStyle, new GUILayoutOption[1]
          {
            GUILayout.Width(285f)
          });
        }
      }
      else if (this.mBehaviorToolbarSelection == 3)
      {
        if (this.mGraphDesigner.SelectedNodes.Count == 1 && !this.mGraphDesigner.SelectedNodes[0].IsEntryDisplay)
        {
          Task task = this.mGraphDesigner.SelectedNodes[0].Task;
          if (this.mNodeDesignerTaskMap != null && this.mNodeDesignerTaskMap.Count > 0)
          {
            NodeDesigner nodeDesigner = this.mGraphDesigner.SelectedNodes[0].Task.get_NodeData().get_NodeDesigner() as NodeDesigner;
            if (Object.op_Inequality((Object) nodeDesigner, (Object) null) && this.mNodeDesignerTaskMap.ContainsKey(nodeDesigner))
              task = this.mNodeDesignerTaskMap[nodeDesigner];
          }
          if (this.mTaskInspector.DrawTaskInspector(this.mActiveBehaviorSource, this.mTaskList, task, !this.ViewOnlyMode()) && !Application.get_isPlaying())
            this.SaveBehavior();
        }
        else
        {
          GUILayout.Space(5f);
          if (this.mGraphDesigner.SelectedNodes.Count > 1)
            GUILayout.Label("Only one task can be selected at a time to\n view its properties.", BehaviorDesignerUtility.LabelWrapGUIStyle, new GUILayoutOption[1]
            {
              GUILayout.Width(285f)
            });
          else
            GUILayout.Label("Select a task from the tree to\nview its properties.", BehaviorDesignerUtility.LabelWrapGUIStyle, new GUILayoutOption[1]
            {
              GUILayout.Width(285f)
            });
        }
      }
      GUILayout.EndArea();
    }

    private bool DrawGraphArea()
    {
      if (Event.get_current().get_type() != 6 && !this.mTakingScreenshot)
      {
        Vector2 vector2 = GUI.BeginScrollView(new Rect(((Rect) ref this.mGraphRect).get_x(), ((Rect) ref this.mGraphRect).get_y(), ((Rect) ref this.mGraphRect).get_width() + 15f, ((Rect) ref this.mGraphRect).get_height() + 15f), this.mGraphScrollPosition, new Rect(0.0f, 0.0f, (float) this.mGraphScrollSize.x, (float) this.mGraphScrollSize.y), true, true);
        if (Vector2.op_Inequality(vector2, this.mGraphScrollPosition) && Event.get_current().get_type() != 9 && Event.get_current().get_type() != 11)
        {
          BehaviorDesignerWindow behaviorDesignerWindow = this;
          behaviorDesignerWindow.mGraphOffset = Vector2.op_Subtraction(behaviorDesignerWindow.mGraphOffset, Vector2.op_Division(Vector2.op_Subtraction(vector2, this.mGraphScrollPosition), this.mGraphZoom));
          this.mGraphScrollPosition = vector2;
          this.mGraphDesigner.GraphDirty();
        }
        GUI.EndScrollView();
      }
      GUI.Box(this.mGraphRect, string.Empty, BehaviorDesignerUtility.GraphBackgroundGUIStyle);
      this.DrawGrid();
      EditorZoomArea.Begin(this.mGraphRect, this.mGraphZoom);
      Vector2 mousePosition;
      if (!this.GetMousePositionInGraph(out mousePosition))
        ((Vector2) ref mousePosition).\u002Ector(-1f, -1f);
      bool flag = false;
      if (Object.op_Inequality((Object) this.mGraphDesigner, (Object) null) && this.mGraphDesigner.DrawNodes(mousePosition, this.mGraphOffset))
        flag = true;
      if (this.mTakingScreenshot && Event.get_current().get_type() == 7)
        this.RenderScreenshotTile();
      if (this.mIsSelecting)
        GUI.Box(this.GetSelectionArea(), string.Empty, BehaviorDesignerUtility.SelectionGUIStyle);
      EditorZoomArea.End();
      this.DrawGraphStatus();
      this.DrawSelectedTaskDescription();
      return flag;
    }

    private void DrawGrid()
    {
      if (!BehaviorDesignerPreferences.GetBool(BDPreferences.SnapToGrid))
        return;
      this.mGridMaterial.SetPass(!EditorGUIUtility.get_isProSkin() ? 1 : 0);
      GL.PushMatrix();
      GL.Begin(1);
      this.DrawGridLines(10f * this.mGraphZoom, new Vector2((float) (this.mGraphOffset.x % 10.0) * this.mGraphZoom, (float) (this.mGraphOffset.y % 10.0) * this.mGraphZoom));
      GL.End();
      GL.PopMatrix();
      this.mGridMaterial.SetPass(!EditorGUIUtility.get_isProSkin() ? 3 : 2);
      GL.PushMatrix();
      GL.Begin(1);
      this.DrawGridLines(50f * this.mGraphZoom, new Vector2((float) (this.mGraphOffset.x % 50.0) * this.mGraphZoom, (float) (this.mGraphOffset.y % 50.0) * this.mGraphZoom));
      GL.End();
      GL.PopMatrix();
    }

    private void DrawGridLines(float gridSize, Vector2 offset)
    {
      float num1 = ((Rect) ref this.mGraphRect).get_x() + (float) offset.x;
      if (offset.x < 0.0)
        num1 += gridSize;
      for (float num2 = num1; (double) num2 < (double) ((Rect) ref this.mGraphRect).get_x() + (double) ((Rect) ref this.mGraphRect).get_width(); num2 += gridSize)
        this.DrawLine(new Vector2(num2, ((Rect) ref this.mGraphRect).get_y()), new Vector2(num2, ((Rect) ref this.mGraphRect).get_y() + ((Rect) ref this.mGraphRect).get_height()));
      float num3 = ((Rect) ref this.mGraphRect).get_y() + (float) offset.y;
      if (offset.y < 0.0)
        num3 += gridSize;
      for (float num2 = num3; (double) num2 < (double) ((Rect) ref this.mGraphRect).get_y() + (double) ((Rect) ref this.mGraphRect).get_height(); num2 += gridSize)
        this.DrawLine(new Vector2(((Rect) ref this.mGraphRect).get_x(), num2), new Vector2(((Rect) ref this.mGraphRect).get_x() + ((Rect) ref this.mGraphRect).get_width(), num2));
    }

    private void DrawLine(Vector2 p1, Vector2 p2)
    {
      GL.Vertex(Vector2.op_Implicit(p1));
      GL.Vertex(Vector2.op_Implicit(p2));
    }

    private void DrawGraphStatus()
    {
      if (this.mGraphStatus.Equals(string.Empty))
        return;
      GUI.Label(new Rect(((Rect) ref this.mGraphRect).get_x() + 5f, ((Rect) ref this.mGraphRect).get_y() + 5f, ((Rect) ref this.mGraphRect).get_width(), 30f), this.mGraphStatus, BehaviorDesignerUtility.GraphStatusGUIStyle);
    }

    private void DrawSelectedTaskDescription()
    {
      TaskDescriptionAttribute[] customAttributes;
      if (!BehaviorDesignerPreferences.GetBool(BDPreferences.ShowTaskDescription) || this.mGraphDesigner.SelectedNodes.Count != 1 || (customAttributes = ((object) this.mGraphDesigner.SelectedNodes[0].Task).GetType().GetCustomAttributes(typeof (TaskDescriptionAttribute), false) as TaskDescriptionAttribute[]).Length <= 0)
        return;
      float num1;
      float num2;
      BehaviorDesignerUtility.TaskCommentGUIStyle.CalcMinMaxWidth(new GUIContent(customAttributes[0].get_Description()), ref num1, ref num2);
      float num3 = Mathf.Min(400f, num2 + 20f);
      float num4 = Mathf.Min(300f, BehaviorDesignerUtility.TaskCommentGUIStyle.CalcHeight(new GUIContent(customAttributes[0].get_Description()), num3)) + 3f;
      GUI.Box(new Rect(((Rect) ref this.mGraphRect).get_x() + 5f, (float) ((double) ((Rect) ref this.mGraphRect).get_yMax() - (double) num4 - 5.0), num3, num4), string.Empty, BehaviorDesignerUtility.TaskDescriptionGUIStyle);
      GUI.Box(new Rect(((Rect) ref this.mGraphRect).get_x() + 2f, (float) ((double) ((Rect) ref this.mGraphRect).get_yMax() - (double) num4 - 5.0), num3, num4), customAttributes[0].get_Description(), BehaviorDesignerUtility.TaskCommentGUIStyle);
    }

    private void AddBehavior()
    {
      if (EditorApplication.get_isPlaying() || !Object.op_Inequality((Object) Selection.get_activeGameObject(), (Object) null))
        return;
      GameObject activeGameObject = Selection.get_activeGameObject();
      this.mActiveObject = Selection.get_activeObject();
      this.mGraphDesigner = (GraphDesigner) ScriptableObject.CreateInstance<GraphDesigner>();
      Type type = Type.GetType("BehaviorDesigner.Runtime.BehaviorTree, Assembly-CSharp") ?? Type.GetType("BehaviorDesigner.Runtime.BehaviorTree, Assembly-CSharp-firstpass");
      Behavior behavior = BehaviorUndo.AddComponent(activeGameObject, type) as Behavior;
      Behavior[] components = (Behavior[]) activeGameObject.GetComponents<Behavior>();
      HashSet<string> stringSet = new HashSet<string>();
      string empty = string.Empty;
      for (int index = 0; index < components.Length; ++index)
      {
        string str = (string) components[index].GetBehaviorSource().behaviorName;
        int num = 2;
        while (stringSet.Contains(str))
        {
          str = string.Format("{0} {1}", (object) components[index].GetBehaviorSource().behaviorName, (object) num);
          ++num;
        }
        components[index].GetBehaviorSource().behaviorName = (__Null) str;
        stringSet.Add((string) components[index].GetBehaviorSource().behaviorName);
      }
      this.LoadBehavior(behavior.GetBehaviorSource(), false);
      this.Repaint();
      if (!BehaviorDesignerPreferences.GetBool(BDPreferences.AddGameGUIComponent))
        return;
      Type typeWithinAssembly = TaskUtility.GetTypeWithinAssembly("BehaviorDesigner.Runtime.BehaviorGameGUI");
      BehaviorUndo.AddComponent(activeGameObject, typeWithinAssembly);
    }

    private void RemoveBehavior()
    {
      if (EditorApplication.get_isPlaying() || !Object.op_Inequality((Object) (this.mActiveObject as GameObject), (Object) null) || this.mActiveBehaviorSource.get_EntryTask() != null && (this.mActiveBehaviorSource.get_EntryTask() == null || !EditorUtility.DisplayDialog("Remove Behavior Tree", "Are you sure you want to remove this behavior tree?", "Yes", "No")))
        return;
      GameObject mActiveObject = this.mActiveObject as GameObject;
      int num = this.IndexForBehavior(this.mActiveBehaviorSource.get_Owner());
      BehaviorUndo.DestroyObject(this.mActiveBehaviorSource.get_Owner().GetObject(), true);
      int index = num - 1;
      if (index == -1 && mActiveObject.GetComponents<Behavior>().Length > 0)
        index = 0;
      if (index > -1)
        this.LoadBehavior(((Behavior) mActiveObject.GetComponents<Behavior>()[index]).GetBehaviorSource(), true);
      else
        this.ClearGraph();
      this.ClearBreadcrumbMenu();
      this.Repaint();
    }

    private int IndexForBehavior(IBehavior behavior)
    {
      if (!Object.op_Implicit((Object) (behavior.GetObject() as Behavior)))
        return 0;
      Behavior[] components = (Behavior[]) ((Component) (behavior.GetObject() as Behavior)).get_gameObject().GetComponents<Behavior>();
      for (int index = 0; index < components.Length; ++index)
      {
        if (((Object) components[index]).Equals((object) behavior))
          return index;
      }
      return -1;
    }

    public NodeDesigner AddTask(Type type, bool useMousePosition)
    {
      if (Object.op_Equality((Object) (this.mActiveObject as GameObject), (Object) null) && Object.op_Equality((Object) (this.mActiveObject as ExternalBehavior), (Object) null) || EditorApplication.get_isPlaying())
        return (NodeDesigner) null;
      Vector2 mousePosition;
      ((Vector2) ref mousePosition).\u002Ector(((Rect) ref this.mGraphRect).get_width() / (2f * this.mGraphZoom), 150f);
      if (useMousePosition)
        this.GetMousePositionInGraph(out mousePosition);
      mousePosition = Vector2.op_Subtraction(mousePosition, this.mGraphOffset);
      GameObject mActiveObject = this.mActiveObject as GameObject;
      if (Object.op_Inequality((Object) mActiveObject, (Object) null) && Object.op_Equality((Object) mActiveObject.GetComponent<Behavior>(), (Object) null))
        this.AddBehavior();
      BehaviorUndo.RegisterUndo("Add", this.mActiveBehaviorSource.get_Owner().GetObject());
      NodeDesigner nodeDesigner;
      if (!Object.op_Inequality((Object) (nodeDesigner = this.mGraphDesigner.AddNode(this.mActiveBehaviorSource, type, mousePosition)), (Object) null))
        return (NodeDesigner) null;
      if (this.onAddTask != null)
        this.onAddTask(this.mActiveBehaviorSource, nodeDesigner.Task);
      this.SaveBehavior();
      return nodeDesigner;
    }

    public bool IsReferencingTasks()
    {
      return this.mTaskInspector.ActiveReferenceTask != null;
    }

    public bool IsReferencingField(FieldInfo fieldInfo)
    {
      return fieldInfo.Equals((object) this.mTaskInspector.ActiveReferenceTaskFieldInfo);
    }

    private void DisableReferenceTasks()
    {
      if (!this.IsReferencingTasks())
        return;
      this.ToggleReferenceTasks();
    }

    public void ToggleReferenceTasks()
    {
      this.ToggleReferenceTasks((Task) null, (FieldInfo) null);
    }

    public void ToggleReferenceTasks(Task task, FieldInfo fieldInfo)
    {
      bool flag = !this.IsReferencingTasks();
      this.mTaskInspector.SetActiveReferencedTasks(!flag ? (Task) null : task, !flag ? (FieldInfo) null : fieldInfo);
      this.UpdateGraphStatus();
    }

    private void ReferenceTask(NodeDesigner nodeDesigner)
    {
      if (!Object.op_Inequality((Object) nodeDesigner, (Object) null) || !this.mTaskInspector.ReferenceTasks(nodeDesigner.Task))
        return;
      this.SaveBehavior();
    }

    public void IdentifyNode(NodeDesigner nodeDesigner)
    {
      this.mGraphDesigner.IdentifyNode(nodeDesigner);
    }

    private void TakeScreenshot()
    {
      this.mScreenshotPath = EditorUtility.SaveFilePanel("Save Screenshot", "Assets", (string) this.mActiveBehaviorSource.behaviorName + "Screenshot.png", "png");
      if (this.mScreenshotPath.Length != 0 && Application.get_dataPath().Length < this.mScreenshotPath.Length)
      {
        this.mTakingScreenshot = true;
        this.mScreenshotGraphSize = this.mGraphDesigner.GraphSize(Vector2.op_Implicit(this.mGraphOffset));
        this.mGraphDesigner.GraphDirty();
        if ((double) ((Rect) ref this.mScreenshotGraphSize).get_width() == 0.0 || (double) ((Rect) ref this.mScreenshotGraphSize).get_height() == 0.0)
          this.mScreenshotGraphSize = new Rect(0.0f, 0.0f, 100f, 100f);
        this.mScreenshotStartGraphZoom = this.mGraphZoom;
        this.mScreenshotStartGraphOffset = this.mGraphOffset;
        this.mGraphZoom = 1f;
        ref Vector2 local1 = ref this.mGraphOffset;
        local1.x = (__Null) (local1.x - ((double) ((Rect) ref this.mScreenshotGraphSize).get_xMin() - 10.0));
        ref Vector2 local2 = ref this.mGraphOffset;
        local2.y = (__Null) (local2.y - ((double) ((Rect) ref this.mScreenshotGraphSize).get_yMin() - 10.0));
        this.mScreenshotGraphOffset = this.mGraphOffset;
        ((Rect) ref this.mScreenshotGraphSize).Set(((Rect) ref this.mScreenshotGraphSize).get_xMin() - 9f, ((Rect) ref this.mScreenshotGraphSize).get_yMin(), ((Rect) ref this.mScreenshotGraphSize).get_width() + 18f, ((Rect) ref this.mScreenshotGraphSize).get_height() + 18f);
        this.mScreenshotTexture = new Texture2D((int) ((Rect) ref this.mScreenshotGraphSize).get_width(), (int) ((Rect) ref this.mScreenshotGraphSize).get_height(), (TextureFormat) 3, false);
        this.Repaint();
      }
      else
      {
        if (!Path.GetExtension(this.mScreenshotPath).Equals(".png"))
          return;
        Debug.LogError((object) "Error: Unable to save screenshot. The save location must be within the Asset directory.");
      }
    }

    private void RenderScreenshotTile()
    {
      float num1 = Mathf.Min(((Rect) ref this.mGraphRect).get_width(), ((Rect) ref this.mScreenshotGraphSize).get_width() - (float) (this.mGraphOffset.x - this.mScreenshotGraphOffset.x));
      float num2 = Mathf.Min(((Rect) ref this.mGraphRect).get_height(), ((Rect) ref this.mScreenshotGraphSize).get_height() + (float) (this.mGraphOffset.y - this.mScreenshotGraphOffset.y));
      Rect rect;
      ((Rect) ref rect).\u002Ector(((Rect) ref this.mGraphRect).get_x(), (float) (39.0 + (double) ((Rect) ref this.mGraphRect).get_height() - (double) num2 - 7.0), num1, num2);
      this.mScreenshotTexture.ReadPixels(rect, -(int) (this.mGraphOffset.x - this.mScreenshotGraphOffset.x), (int) ((double) ((Rect) ref this.mScreenshotGraphSize).get_height() - (double) num2 + (this.mGraphOffset.y - this.mScreenshotGraphOffset.y)));
      this.mScreenshotTexture.Apply(false);
      if ((double) ((Rect) ref this.mScreenshotGraphSize).get_xMin() + (double) num1 - (this.mGraphOffset.x - this.mScreenshotGraphOffset.x) < (double) ((Rect) ref this.mScreenshotGraphSize).get_xMax())
      {
        ref Vector2 local = ref this.mGraphOffset;
        local.x = (__Null) (local.x - ((double) num1 - 1.0));
        this.mGraphDesigner.GraphDirty();
        this.Repaint();
      }
      else if ((double) ((Rect) ref this.mScreenshotGraphSize).get_yMin() + (double) num2 - (this.mGraphOffset.y - this.mScreenshotGraphOffset.y) < (double) ((Rect) ref this.mScreenshotGraphSize).get_yMax())
      {
        ref Vector2 local = ref this.mGraphOffset;
        local.y = (__Null) (local.y - ((double) num2 - 1.0));
        this.mGraphOffset.x = this.mScreenshotGraphOffset.x;
        this.mGraphDesigner.GraphDirty();
        this.Repaint();
      }
      else
        this.SaveScreenshot();
    }

    private void SaveScreenshot()
    {
      byte[] png = ImageConversion.EncodeToPNG(this.mScreenshotTexture);
      Object.DestroyImmediate((Object) this.mScreenshotTexture, true);
      File.WriteAllBytes(this.mScreenshotPath, png);
      AssetDatabase.ImportAsset(string.Format("Assets/{0}", (object) this.mScreenshotPath.Substring(Application.get_dataPath().Length + 1)));
      this.mTakingScreenshot = false;
      this.mGraphZoom = this.mScreenshotStartGraphZoom;
      this.mGraphOffset = this.mScreenshotStartGraphOffset;
      this.mGraphDesigner.GraphDirty();
      this.Repaint();
    }

    private void HandleEvents()
    {
      if (this.mTakingScreenshot)
        return;
      if (Event.get_current().get_type() != 1 && this.CheckForAutoScroll())
      {
        this.Repaint();
      }
      else
      {
        if (Event.get_current().get_type() == 7 || Event.get_current().get_type() == 8)
          return;
        switch ((int) Event.get_current().get_type())
        {
          case 0:
            if (Event.get_current().get_button() == 0 && Event.get_current().get_modifiers() != 2)
            {
              Vector2 mousePosition;
              if (this.GetMousePositionInGraph(out mousePosition))
              {
                if (!this.LeftMouseDown(Event.get_current().get_clickCount(), mousePosition))
                  break;
                Event.get_current().Use();
                break;
              }
              if (!this.GetMousePositionInPropertiesPane(out mousePosition) || this.mBehaviorToolbarSelection != 2 || !this.mVariableInspector.LeftMouseDown((IVariableSource) this.mActiveBehaviorSource, this.mActiveBehaviorSource, mousePosition))
                break;
              Event.get_current().Use();
              this.Repaint();
              break;
            }
            if (Event.get_current().get_button() != 1 && (Event.get_current().get_modifiers() != 2 || Event.get_current().get_button() != 0) || !this.RightMouseDown())
              break;
            Event.get_current().Use();
            break;
          case 1:
            if (Event.get_current().get_button() == 0 && Event.get_current().get_modifiers() != 2)
            {
              if (!this.LeftMouseRelease())
                break;
              Event.get_current().Use();
              break;
            }
            if (Event.get_current().get_button() != 1 && (Event.get_current().get_modifiers() != 2 || Event.get_current().get_button() != 0) || !this.mShowRightClickMenu)
              break;
            this.mShowRightClickMenu = false;
            this.mRightClickMenu.ShowAsContext();
            Event.get_current().Use();
            break;
          case 2:
            if (!this.MouseMove())
              break;
            Event.get_current().Use();
            break;
          case 3:
            if (Event.get_current().get_button() == 0)
            {
              if (this.LeftMouseDragged())
              {
                Event.get_current().Use();
                break;
              }
              if (Event.get_current().get_modifiers() != 4 || !this.MousePan())
                break;
              Event.get_current().Use();
              break;
            }
            if (Event.get_current().get_button() != 2 || !this.MousePan())
              break;
            Event.get_current().Use();
            break;
          case 4:
            if (Event.get_current().get_keyCode() != 310 && Event.get_current().get_keyCode() != 309)
              break;
            this.mCommandDown = true;
            break;
          case 5:
            if (Event.get_current().get_keyCode() == (int) sbyte.MaxValue || Event.get_current().get_keyCode() == 8 || Event.get_current().get_commandName().Equals("Delete"))
            {
              if (this.PropertiesInspectorHasFocus() || EditorApplication.get_isPlaying())
                break;
              this.DeleteNodes();
              Event.get_current().Use();
              break;
            }
            if (Event.get_current().get_keyCode() == 13 || Event.get_current().get_keyCode() == 271)
            {
              if (this.mBehaviorToolbarSelection == 2 && this.mVariableInspector.HasFocus())
              {
                if (this.mVariableInspector.ClearFocus(true, this.mActiveBehaviorSource))
                  this.SaveBehavior();
                this.Repaint();
              }
              else
                this.DisableReferenceTasks();
              Event.get_current().Use();
              break;
            }
            if (Event.get_current().get_keyCode() == 27)
            {
              this.DisableReferenceTasks();
              break;
            }
            if (Event.get_current().get_keyCode() != 310 && Event.get_current().get_keyCode() != 309)
              break;
            this.mCommandDown = false;
            break;
          case 6:
            if (BehaviorDesignerPreferences.GetBool(BDPreferences.MouseWhellScrolls) && !this.mCommandDown)
            {
              this.MousePan();
              break;
            }
            if (!this.MouseZoom())
              break;
            Event.get_current().Use();
            break;
          case 13:
            if (EditorApplication.get_isPlaying() || !Event.get_current().get_commandName().Equals("Copy") && !Event.get_current().get_commandName().Equals("Paste") && (!Event.get_current().get_commandName().Equals("Cut") && !Event.get_current().get_commandName().Equals("SelectAll")) && !Event.get_current().get_commandName().Equals("Duplicate") || (this.PropertiesInspectorHasFocus() || EditorApplication.get_isPlaying() || this.ViewOnlyMode()))
              break;
            Event.get_current().Use();
            break;
          case 14:
            if (this.PropertiesInspectorHasFocus() || EditorApplication.get_isPlaying() || this.ViewOnlyMode())
              break;
            if (Event.get_current().get_commandName().Equals("Copy"))
            {
              this.CopyNodes();
              Event.get_current().Use();
              break;
            }
            if (Event.get_current().get_commandName().Equals("Paste"))
            {
              this.PasteNodes();
              Event.get_current().Use();
              break;
            }
            if (Event.get_current().get_commandName().Equals("Cut"))
            {
              this.CutNodes();
              Event.get_current().Use();
              break;
            }
            if (Event.get_current().get_commandName().Equals("SelectAll"))
            {
              this.mGraphDesigner.SelectAll();
              Event.get_current().Use();
              break;
            }
            if (!Event.get_current().get_commandName().Equals("Duplicate"))
              break;
            this.DuplicateNodes();
            Event.get_current().Use();
            break;
        }
      }
    }

    private bool CheckForAutoScroll()
    {
      Vector2 mousePosition;
      if (!this.GetMousePositionInGraph(out mousePosition) || ((Rect) ref this.mGraphScrollRect).Contains(this.mCurrentMousePosition) || !this.mIsDragging && !this.mIsSelecting && !Object.op_Inequality((Object) this.mGraphDesigner.ActiveNodeConnection, (Object) null))
        return false;
      Vector2 zero = Vector2.get_zero();
      if (this.mCurrentMousePosition.y < (double) ((Rect) ref this.mGraphScrollRect).get_yMin() + 15.0)
        zero.y = (__Null) 3.0;
      else if (this.mCurrentMousePosition.y > (double) ((Rect) ref this.mGraphScrollRect).get_yMax() - 15.0)
        zero.y = (__Null) -3.0;
      if (this.mCurrentMousePosition.x < (double) ((Rect) ref this.mGraphScrollRect).get_xMin() + 15.0)
        zero.x = (__Null) 3.0;
      else if (this.mCurrentMousePosition.x > (double) ((Rect) ref this.mGraphScrollRect).get_xMax() - 15.0)
        zero.x = (__Null) -3.0;
      this.ScrollGraph(zero);
      if (this.mIsDragging)
        this.mGraphDesigner.DragSelectedNodes(Vector2.op_Division(Vector2.op_UnaryNegation(zero), this.mGraphZoom), Event.get_current().get_modifiers() != 4);
      if (this.mIsSelecting)
      {
        BehaviorDesignerWindow behaviorDesignerWindow = this;
        behaviorDesignerWindow.mSelectStartPosition = Vector2.op_Addition(behaviorDesignerWindow.mSelectStartPosition, Vector2.op_Division(zero, this.mGraphZoom));
      }
      return true;
    }

    private bool MouseMove()
    {
      Vector2 mousePosition;
      if (!this.GetMousePositionInGraph(out mousePosition))
        return false;
      NodeDesigner nodeDesigner = this.mGraphDesigner.NodeAt(mousePosition, this.mGraphOffset);
      if (Object.op_Inequality((Object) this.mGraphDesigner.HoverNode, (Object) null) && (Object.op_Inequality((Object) nodeDesigner, (Object) null) && !this.mGraphDesigner.HoverNode.Equals((object) nodeDesigner) || !this.mGraphDesigner.HoverNode.HoverBarAreaContains(mousePosition, this.mGraphOffset)))
      {
        this.mGraphDesigner.ClearHover();
        this.Repaint();
      }
      if (Object.op_Implicit((Object) nodeDesigner) && !nodeDesigner.IsEntryDisplay && !this.ViewOnlyMode())
        this.mGraphDesigner.Hover(nodeDesigner);
      return Object.op_Inequality((Object) this.mGraphDesigner.HoverNode, (Object) null);
    }

    private bool LeftMouseDown(int clickCount, Vector2 mousePosition)
    {
      if (this.PropertiesInspectorHasFocus())
      {
        this.mTaskInspector.ClearFocus();
        this.mVariableInspector.ClearFocus(false, (BehaviorSource) null);
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
        if (Object.op_Equality((Object) nodeDesigner, (Object) null))
          this.DisableReferenceTasks();
        else
          this.ReferenceTask(nodeDesigner);
        return true;
      }
      if (Object.op_Inequality((Object) nodeDesigner, (Object) null))
      {
        if (Object.op_Inequality((Object) this.mGraphDesigner.HoverNode, (Object) null) && !nodeDesigner.Equals((object) this.mGraphDesigner.HoverNode))
        {
          this.mGraphDesigner.ClearHover();
          this.mGraphDesigner.Hover(nodeDesigner);
        }
        NodeConnection connection;
        if (!this.ViewOnlyMode() && Object.op_Inequality((Object) (connection = nodeDesigner.NodeConnectionRectContains(mousePosition, this.mGraphOffset)), (Object) null))
        {
          if (this.mGraphDesigner.NodeCanOriginateConnection(nodeDesigner, connection))
            this.mGraphDesigner.ActiveNodeConnection = connection;
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
                this.mBehaviorToolbarSelection = 3;
              else if (nodeDesigner.Task is BehaviorReference)
              {
                BehaviorReference task = nodeDesigner.Task as BehaviorReference;
                if (task.GetExternalBehaviors() != null && task.GetExternalBehaviors().Length > 0 && Object.op_Inequality((Object) task.GetExternalBehaviors()[0], (Object) null))
                {
                  if (this.mLockActiveGameObject)
                    this.LoadBehavior(task.GetExternalBehaviors()[0].GetBehaviorSource(), false);
                  else
                    Selection.set_activeObject((Object) task.GetExternalBehaviors()[0]);
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
                this.mBehaviorToolbarSelection = 3;
            }
            else
              this.mKeepTasksSelected = true;
            this.mGraphDesigner.Select(nodeDesigner);
          }
          this.mNodeClicked = this.mGraphDesigner.IsSelected(nodeDesigner);
          return true;
        }
      }
      if (Object.op_Inequality((Object) this.mGraphDesigner.HoverNode, (Object) null))
      {
        bool collapsedButtonClicked = false;
        if (this.mGraphDesigner.HoverNode.HoverBarButtonClick(mousePosition, this.mGraphOffset, ref collapsedButtonClicked))
        {
          this.SaveBehavior();
          if (collapsedButtonClicked && this.mGraphDesigner.HoverNode.Task.get_NodeData().get_Collapsed())
            this.mGraphDesigner.DeselectWithParent(this.mGraphDesigner.HoverNode);
          return true;
        }
      }
      List<NodeConnection> nodeConnections = new List<NodeConnection>();
      this.mGraphDesigner.NodeConnectionsAt(mousePosition, this.mGraphOffset, ref nodeConnections);
      if (nodeConnections.Count > 0)
      {
        if (Event.get_current().get_modifiers() != 1 && Event.get_current().get_modifiers() != 2)
        {
          this.mGraphDesigner.ClearNodeSelection();
          this.mGraphDesigner.ClearConnectionSelection();
        }
        for (int index = 0; index < nodeConnections.Count; ++index)
        {
          if (this.mGraphDesigner.IsSelected(nodeConnections[index]))
          {
            if (Event.get_current().get_modifiers() == 2)
              this.mGraphDesigner.Deselect(nodeConnections[index]);
          }
          else
            this.mGraphDesigner.Select(nodeConnections[index]);
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
      Vector2 mousePosition;
      if (!this.GetMousePositionInGraph(out mousePosition))
        return false;
      if (Event.get_current().get_modifiers() != 4)
      {
        if (this.IsReferencingTasks())
          return true;
        if (this.mIsSelecting)
        {
          this.mGraphDesigner.DeselectAll((NodeDesigner) null);
          List<NodeDesigner> nodeDesignerList = this.mGraphDesigner.NodesAt(this.GetSelectionArea(), this.mGraphOffset);
          if (nodeDesignerList != null)
          {
            for (int index = 0; index < nodeDesignerList.Count; ++index)
              this.mGraphDesigner.Select(nodeDesignerList[index]);
          }
          return true;
        }
        if (Object.op_Inequality((Object) this.mGraphDesigner.ActiveNodeConnection, (Object) null))
          return true;
      }
      if (!this.mNodeClicked || this.ViewOnlyMode())
        return false;
      Vector2 vector2 = Vector2.get_zero();
      if (BehaviorDesignerPreferences.GetBool(BDPreferences.SnapToGrid))
      {
        BehaviorDesignerWindow behaviorDesignerWindow = this;
        behaviorDesignerWindow.mDragDelta = Vector2.op_Addition(behaviorDesignerWindow.mDragDelta, Event.get_current().get_delta());
        if ((double) Mathf.Abs((float) this.mDragDelta.x) > 10.0)
        {
          float num = Mathf.Abs((float) this.mDragDelta.x) % 10f;
          vector2.x = (__Null) (((double) Mathf.Abs((float) this.mDragDelta.x) - (double) num) * (double) Mathf.Sign((float) this.mDragDelta.x));
          this.mDragDelta.x = (__Null) ((double) num * (double) Mathf.Sign((float) this.mDragDelta.x));
        }
        if ((double) Mathf.Abs((float) this.mDragDelta.y) > 10.0)
        {
          float num = Mathf.Abs((float) this.mDragDelta.y) % 10f;
          vector2.y = (__Null) (((double) Mathf.Abs((float) this.mDragDelta.y) - (double) num) * (double) Mathf.Sign((float) this.mDragDelta.y));
          this.mDragDelta.y = (__Null) ((double) num * (double) Mathf.Sign((float) this.mDragDelta.y));
        }
      }
      else
        vector2 = Event.get_current().get_delta();
      bool flag = this.mGraphDesigner.DragSelectedNodes(Vector2.op_Division(vector2, this.mGraphZoom), Event.get_current().get_modifiers() != 4);
      if (flag)
        this.mKeepTasksSelected = true;
      this.mIsDragging = true;
      return flag;
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
        Vector2 mousePosition;
        if (this.GetMousePositionInGraph(out mousePosition))
          return true;
        this.mGraphDesigner.ActiveNodeConnection = (NodeConnection) null;
        return false;
      }
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
        this.mDragDelta = Vector2.op_Implicit(Vector3.get_zero());
        return true;
      }
      if (Object.op_Inequality((Object) this.mGraphDesigner.ActiveNodeConnection, (Object) null))
      {
        Vector2 mousePosition;
        if (!this.GetMousePositionInGraph(out mousePosition))
        {
          this.mGraphDesigner.ActiveNodeConnection = (NodeConnection) null;
          return false;
        }
        NodeDesigner nodeDesigner = this.mGraphDesigner.NodeAt(mousePosition, this.mGraphOffset);
        if (Object.op_Inequality((Object) nodeDesigner, (Object) null) && !nodeDesigner.Equals((object) this.mGraphDesigner.ActiveNodeConnection.OriginatingNodeDesigner) && this.mGraphDesigner.NodeCanAcceptConnection(nodeDesigner, this.mGraphDesigner.ActiveNodeConnection))
        {
          this.mGraphDesigner.ConnectNodes(this.mActiveBehaviorSource, nodeDesigner);
          BehaviorUndo.RegisterUndo("Task Connection", this.mActiveBehaviorSource.get_Owner().GetObject());
          this.SaveBehavior();
        }
        else
          this.mGraphDesigner.ActiveNodeConnection = (NodeConnection) null;
        return true;
      }
      Vector2 mousePosition1;
      if (Event.get_current().get_modifiers() == 1 || this.mKeepTasksSelected || !this.GetMousePositionInGraph(out mousePosition1))
        return false;
      NodeDesigner nodeDesigner1 = this.mGraphDesigner.NodeAt(mousePosition1, this.mGraphOffset);
      if (Object.op_Inequality((Object) nodeDesigner1, (Object) null) && !this.mGraphDesigner.IsSelected(nodeDesigner1))
        this.mGraphDesigner.DeselectAll(nodeDesigner1);
      return true;
    }

    private bool RightMouseDown()
    {
      if (this.IsReferencingTasks())
      {
        this.DisableReferenceTasks();
        return false;
      }
      Vector2 mousePosition;
      if (!this.GetMousePositionInGraph(out mousePosition))
        return false;
      NodeDesigner nodeDesigner = this.mGraphDesigner.NodeAt(mousePosition, this.mGraphOffset);
      if (Object.op_Equality((Object) nodeDesigner, (Object) null) || !this.mGraphDesigner.IsSelected(nodeDesigner))
      {
        this.mGraphDesigner.ClearNodeSelection();
        this.mGraphDesigner.ClearConnectionSelection();
        if (Object.op_Inequality((Object) nodeDesigner, (Object) null))
          this.mGraphDesigner.Select(nodeDesigner);
      }
      if (Object.op_Inequality((Object) this.mGraphDesigner.HoverNode, (Object) null))
        this.mGraphDesigner.ClearHover();
      this.BuildRightClickMenu(nodeDesigner);
      return true;
    }

    private bool MouseZoom()
    {
      Vector2 mousePosition1;
      if (!this.GetMousePositionInGraph(out mousePosition1))
        return false;
      this.mGraphZoom += (float) (-Event.get_current().get_delta().y / 150.0);
      this.mGraphZoom = Mathf.Clamp(this.mGraphZoom, 0.4f, 1f);
      Vector2 mousePosition2;
      this.GetMousePositionInGraph(out mousePosition2);
      BehaviorDesignerWindow behaviorDesignerWindow1 = this;
      behaviorDesignerWindow1.mGraphOffset = Vector2.op_Addition(behaviorDesignerWindow1.mGraphOffset, Vector2.op_Subtraction(mousePosition2, mousePosition1));
      BehaviorDesignerWindow behaviorDesignerWindow2 = this;
      behaviorDesignerWindow2.mGraphScrollPosition = Vector2.op_Addition(behaviorDesignerWindow2.mGraphScrollPosition, Vector2.op_Subtraction(mousePosition2, mousePosition1));
      this.mGraphDesigner.GraphDirty();
      return true;
    }

    private bool MousePan()
    {
      Vector2 mousePosition;
      if (!this.GetMousePositionInGraph(out mousePosition))
        return false;
      Vector2 amount = Event.get_current().get_delta();
      if (Event.get_current().get_type() == 6)
      {
        amount = Vector2.op_Multiply(amount, -1.5f);
        if (Event.get_current().get_modifiers() == 2)
        {
          amount.x = amount.y;
          amount.y = (__Null) 0.0;
        }
      }
      this.ScrollGraph(amount);
      return true;
    }

    private void ScrollGraph(Vector2 amount)
    {
      BehaviorDesignerWindow behaviorDesignerWindow1 = this;
      behaviorDesignerWindow1.mGraphOffset = Vector2.op_Addition(behaviorDesignerWindow1.mGraphOffset, Vector2.op_Division(amount, this.mGraphZoom));
      BehaviorDesignerWindow behaviorDesignerWindow2 = this;
      behaviorDesignerWindow2.mGraphScrollPosition = Vector2.op_Subtraction(behaviorDesignerWindow2.mGraphScrollPosition, amount);
      this.mGraphDesigner.GraphDirty();
      this.Repaint();
    }

    private bool PropertiesInspectorHasFocus()
    {
      if (!this.mTaskInspector.HasFocus())
        return this.mVariableInspector.HasFocus();
      return true;
    }

    private void AddTaskCallback(object obj)
    {
      this.AddTask((Type) obj, true);
    }

    private void ReplaceTaskCallback(object obj)
    {
      Type type = (Type) obj;
      if (this.mGraphDesigner.SelectedNodes.Count != 1 || ((object) this.mGraphDesigner.SelectedNodes[0].Task).GetType().Equals(type) || !this.mGraphDesigner.ReplaceSelectedNode(this.mActiveBehaviorSource, type))
        return;
      this.SaveBehavior();
    }

    private void BehaviorSelectionCallback(object obj)
    {
      BehaviorSource behaviorSource = obj as BehaviorSource;
      this.mActiveObject = !(behaviorSource.get_Owner() is Behavior) ? (Object) (behaviorSource.get_Owner() as ExternalBehavior) : (Object) ((Component) (behaviorSource.get_Owner() as Behavior)).get_gameObject();
      if (!this.mLockActiveGameObject)
        Selection.set_activeObject(this.mActiveObject);
      this.LoadBehavior(behaviorSource, false);
      this.UpdateGraphStatus();
      if (!EditorApplication.get_isPaused())
        return;
      this.mUpdateNodeTaskMap = true;
      this.UpdateNodeTaskMap();
    }

    private void ToggleEnableState(object obj)
    {
      (obj as NodeDesigner).ToggleEnableState();
      this.SaveBehavior();
      this.Repaint();
    }

    private void ToggleCollapseState(object obj)
    {
      NodeDesigner nodeDesigner = obj as NodeDesigner;
      if (nodeDesigner.ToggleCollapseState())
        this.mGraphDesigner.DeselectWithParent(nodeDesigner);
      this.SaveBehavior();
      this.Repaint();
    }

    private void ToggleBreakpoint(object obj)
    {
      (obj as NodeDesigner).ToggleBreakpoint();
      this.SaveBehavior();
      this.Repaint();
    }

    private void OpenInFileEditor(object obj)
    {
      TaskInspector.OpenInFileEditor((object) (obj as NodeDesigner).Task);
    }

    private void SelectInProject(object obj)
    {
      TaskInspector.SelectInProject((object) (obj as NodeDesigner).Task);
    }

    private void CopyNodes()
    {
      this.mCopiedTasks = this.mGraphDesigner.Copy(this.mGraphOffset, this.mGraphZoom);
    }

    private void PasteNodes()
    {
      if (Object.op_Equality(this.mActiveObject, (Object) null) || EditorApplication.get_isPlaying())
        return;
      GameObject mActiveObject = this.mActiveObject as GameObject;
      if (Object.op_Inequality((Object) mActiveObject, (Object) null) && Object.op_Equality((Object) mActiveObject.GetComponent<Behavior>(), (Object) null))
        this.AddBehavior();
      if (this.mCopiedTasks != null && this.mCopiedTasks.Count > 0)
        BehaviorUndo.RegisterUndo("Paste", this.mActiveBehaviorSource.get_Owner().GetObject());
      this.mGraphDesigner.Paste(this.mActiveBehaviorSource, Vector2.op_Implicit(new Vector2((float) ((double) ((Rect) ref this.mGraphRect).get_width() / (2.0 * (double) this.mGraphZoom) - this.mGraphOffset.x), (float) (150.0 - this.mGraphOffset.y))), this.mCopiedTasks, this.mGraphOffset, this.mGraphZoom);
      this.SaveBehavior();
    }

    private void CutNodes()
    {
      this.mCopiedTasks = this.mGraphDesigner.Copy(this.mGraphOffset, this.mGraphZoom);
      if (this.mCopiedTasks != null && this.mCopiedTasks.Count > 0)
        BehaviorUndo.RegisterUndo("Cut", this.mActiveBehaviorSource.get_Owner().GetObject());
      this.mGraphDesigner.Delete(this.mActiveBehaviorSource, (BehaviorDesignerWindow.TaskCallbackHandler) null);
      this.SaveBehavior();
    }

    private void DuplicateNodes()
    {
      List<TaskSerializer> copiedTasks = this.mGraphDesigner.Copy(this.mGraphOffset, this.mGraphZoom);
      if (copiedTasks != null && copiedTasks.Count > 0)
        BehaviorUndo.RegisterUndo("Duplicate", this.mActiveBehaviorSource.get_Owner().GetObject());
      this.mGraphDesigner.Paste(this.mActiveBehaviorSource, Vector2.op_Implicit(new Vector2((float) ((double) ((Rect) ref this.mGraphRect).get_width() / (2.0 * (double) this.mGraphZoom) - this.mGraphOffset.x), (float) (150.0 - this.mGraphOffset.y))), copiedTasks, this.mGraphOffset, this.mGraphZoom);
      this.SaveBehavior();
    }

    private void DeleteNodes()
    {
      if (this.ViewOnlyMode())
        return;
      this.mGraphDesigner.Delete(this.mActiveBehaviorSource, this.onRemoveTask);
      this.SaveBehavior();
    }

    public void RemoveSharedVariableReferences(SharedVariable sharedVariable)
    {
      if (!this.mGraphDesigner.RemoveSharedVariableReferences(sharedVariable))
        return;
      this.SaveBehavior();
      this.Repaint();
    }

    private void OnUndoRedo()
    {
      if (this.mActiveBehaviorSource == null)
        return;
      this.LoadBehavior(this.mActiveBehaviorSource, true, false);
    }

    private void SetupSizes()
    {
      Rect position1 = this.get_position();
      float width = ((Rect) ref position1).get_width();
      Rect position2 = this.get_position();
      float num = ((Rect) ref position2).get_height() + 22f;
      if ((double) this.mPrevScreenWidth == (double) width && (double) this.mPrevScreenHeight == (double) num && this.mPropertiesPanelOnLeft == BehaviorDesignerPreferences.GetBool(BDPreferences.PropertiesPanelOnLeft))
        return;
      if (BehaviorDesignerPreferences.GetBool(BDPreferences.PropertiesPanelOnLeft))
      {
        this.mFileToolBarRect = new Rect(300f, 0.0f, width - 300f, 18f);
        this.mPropertyToolbarRect = new Rect(0.0f, 0.0f, 300f, 18f);
        this.mPropertyBoxRect = new Rect(0.0f, ((Rect) ref this.mPropertyToolbarRect).get_height(), 300f, (float) ((double) num - (double) ((Rect) ref this.mPropertyToolbarRect).get_height() - 21.0));
        this.mGraphRect = new Rect(300f, 18f, (float) ((double) width - 300.0 - 15.0), (float) ((double) num - 36.0 - 21.0 - 15.0));
        this.mPreferencesPaneRect = new Rect((float) (300.0 + (double) ((Rect) ref this.mGraphRect).get_width() - 290.0), (float) (18 + (!EditorGUIUtility.get_isProSkin() ? 2 : 1)), 290f, 368f);
      }
      else
      {
        this.mFileToolBarRect = new Rect(0.0f, 0.0f, width - 300f, 18f);
        this.mPropertyToolbarRect = new Rect(width - 300f, 0.0f, 300f, 18f);
        this.mPropertyBoxRect = new Rect(width - 300f, ((Rect) ref this.mPropertyToolbarRect).get_height(), 300f, (float) ((double) num - (double) ((Rect) ref this.mPropertyToolbarRect).get_height() - 21.0));
        this.mGraphRect = new Rect(0.0f, 18f, (float) ((double) width - 300.0 - 15.0), (float) ((double) num - 36.0 - 21.0 - 15.0));
        this.mPreferencesPaneRect = new Rect(((Rect) ref this.mGraphRect).get_width() - 290f, (float) (18 + (!EditorGUIUtility.get_isProSkin() ? 2 : 1)), 290f, 368f);
      }
      this.mDebugToolBarRect = new Rect(((Rect) ref this.mGraphRect).get_x(), (float) ((double) num - 18.0 - 21.0), ((Rect) ref this.mGraphRect).get_width() + 15f, 18f);
      ((Rect) ref this.mGraphScrollRect).Set(((Rect) ref this.mGraphRect).get_xMin() + 15f, ((Rect) ref this.mGraphRect).get_yMin() + 15f, ((Rect) ref this.mGraphRect).get_width() - 30f, ((Rect) ref this.mGraphRect).get_height() - 30f);
      if (Vector2.op_Equality(this.mGraphScrollPosition, new Vector2(-1f, -1f)))
        this.mGraphScrollPosition = Vector2.op_Subtraction(Vector2.op_Division(Vector2.op_Subtraction(this.mGraphScrollSize, new Vector2(((Rect) ref this.mGraphRect).get_width(), ((Rect) ref this.mGraphRect).get_height())), 2f), Vector2.op_Multiply(2f, new Vector2(15f, 15f)));
      this.mPrevScreenWidth = width;
      this.mPrevScreenHeight = num;
      this.mPropertiesPanelOnLeft = BehaviorDesignerPreferences.GetBool(BDPreferences.PropertiesPanelOnLeft);
    }

    private bool GetMousePositionInGraph(out Vector2 mousePosition)
    {
      mousePosition = this.mCurrentMousePosition;
      if (!((Rect) ref this.mGraphRect).Contains(mousePosition) || this.mShowPrefPane && ((Rect) ref this.mPreferencesPaneRect).Contains(mousePosition))
        return false;
      mousePosition = Vector2.op_Subtraction(mousePosition, new Vector2(((Rect) ref this.mGraphRect).get_xMin(), ((Rect) ref this.mGraphRect).get_yMin()));
      mousePosition = Vector2.op_Division(mousePosition, this.mGraphZoom);
      return true;
    }

    private bool GetMousePositionInPropertiesPane(out Vector2 mousePosition)
    {
      mousePosition = this.mCurrentMousePosition;
      if (!((Rect) ref this.mPropertyBoxRect).Contains(mousePosition))
        return false;
      ref Vector2 local1 = ref mousePosition;
      local1.x = (__Null) (local1.x - (double) ((Rect) ref this.mPropertyBoxRect).get_xMin());
      ref Vector2 local2 = ref mousePosition;
      local2.y = (__Null) (local2.y - (double) ((Rect) ref this.mPropertyBoxRect).get_yMin());
      return true;
    }

    private Rect GetSelectionArea()
    {
      Vector2 mousePosition;
      if (this.GetMousePositionInGraph(out mousePosition))
      {
        float num1 = this.mSelectStartPosition.x >= mousePosition.x ? (float) mousePosition.x : (float) this.mSelectStartPosition.x;
        float num2 = this.mSelectStartPosition.x <= mousePosition.x ? (float) mousePosition.x : (float) this.mSelectStartPosition.x;
        float num3 = this.mSelectStartPosition.y >= mousePosition.y ? (float) mousePosition.y : (float) this.mSelectStartPosition.y;
        float num4 = this.mSelectStartPosition.y <= mousePosition.y ? (float) mousePosition.y : (float) this.mSelectStartPosition.y;
        this.mSelectionArea = new Rect(num1, num3, num2 - num1, num4 - num3);
      }
      return this.mSelectionArea;
    }

    public bool ViewOnlyMode()
    {
      return !Application.get_isPlaying() && this.mActiveBehaviorSource != null && (this.mActiveBehaviorSource.get_Owner() != null && !((object) this.mActiveBehaviorSource.get_Owner()).Equals((object) null)) && (Object.op_Inequality((Object) (this.mActiveBehaviorSource.get_Owner().GetObject() as Behavior), (Object) null) && !BehaviorDesignerPreferences.GetBool(BDPreferences.EditablePrefabInstances) && PrefabUtility.GetPrefabType(this.mActiveBehaviorSource.get_Owner().GetObject()) == 3);
    }

    private BehaviorSource BehaviorSourceFromIBehaviorHistory(IBehavior behavior)
    {
      if (behavior == null)
        return (BehaviorSource) null;
      if (!(behavior.GetObject() is GameObject))
        return behavior.GetBehaviorSource();
      Behavior[] components = (Behavior[]) (behavior.GetObject() as GameObject).GetComponents<Behavior>();
      for (int index = 0; index < ((IEnumerable<Behavior>) components).Count<Behavior>(); ++index)
      {
        if (components[index].GetBehaviorSource().get_BehaviorID() == behavior.GetBehaviorSource().get_BehaviorID())
          return components[index].GetBehaviorSource();
      }
      return (BehaviorSource) null;
    }

    public void SaveBehavior()
    {
      if (this.mActiveBehaviorSource == null || this.ViewOnlyMode() || Application.get_isPlaying())
        return;
      this.mGraphDesigner.Save(this.mActiveBehaviorSource);
      this.CheckForErrors();
    }

    private void CheckForErrors()
    {
      if (this.mErrorDetails != null)
      {
        for (int index = 0; index < this.mErrorDetails.Count; ++index)
        {
          if (Object.op_Inequality((Object) this.mErrorDetails[index].NodeDesigner, (Object) null))
            this.mErrorDetails[index].NodeDesigner.HasError = false;
        }
      }
      if (BehaviorDesignerPreferences.GetBool(BDPreferences.ErrorChecking))
      {
        this.mErrorDetails = ErrorCheck.CheckForErrors(this.mExternalParent == null ? this.mActiveBehaviorSource : this.mExternalParent);
        if (this.mErrorDetails != null)
        {
          for (int index = 0; index < this.mErrorDetails.Count; ++index)
          {
            if (!Object.op_Equality((Object) this.mErrorDetails[index].NodeDesigner, (Object) null))
              this.mErrorDetails[index].NodeDesigner.HasError = true;
          }
        }
      }
      else
        this.mErrorDetails = (List<BehaviorDesigner.Editor.ErrorDetails>) null;
      if (!Object.op_Inequality((Object) ErrorWindow.instance, (Object) null))
        return;
      ErrorWindow.instance.ErrorDetails = this.mErrorDetails;
      ErrorWindow.instance.Repaint();
    }

    public bool ContainsError(Task task, string fieldName)
    {
      if (this.mErrorDetails == null)
        return false;
      for (int index = 0; index < this.mErrorDetails.Count; ++index)
      {
        if (task == null)
        {
          if (!Object.op_Inequality((Object) this.mErrorDetails[index].NodeDesigner, (Object) null) && this.mErrorDetails[index].FieldName == fieldName)
            return true;
        }
        else if (!Object.op_Equality((Object) this.mErrorDetails[index].NodeDesigner, (Object) null) && this.mErrorDetails[index].NodeDesigner.Task == task && this.mErrorDetails[index].FieldName == fieldName)
          return true;
      }
      return false;
    }

    private bool UpdateCheck()
    {
      if (this.mUpdateCheckRequest != null && this.mUpdateCheckRequest.get_isDone())
      {
        if (!string.IsNullOrEmpty(this.mUpdateCheckRequest.get_error()))
        {
          this.mUpdateCheckRequest = (WWW) null;
          return false;
        }
        if (!"1.5.11".ToString().Equals(this.mUpdateCheckRequest.get_text()))
          this.LatestVersion = this.mUpdateCheckRequest.get_text();
        this.mUpdateCheckRequest = (WWW) null;
      }
      if (BehaviorDesignerPreferences.GetBool(BDPreferences.UpdateCheck) && DateTime.Compare(this.LastUpdateCheck.AddDays(1.0), DateTime.UtcNow) < 0)
      {
        this.mUpdateCheckRequest = new WWW(string.Format("http://www.opsive.com/assets/BehaviorDesigner/UpdateCheck.php?version={0}&unityversion={1}&devplatform={2}&targetplatform={3}", (object) "1.5.11", (object) Application.get_unityVersion(), (object) Application.get_platform(), (object) EditorUserBuildSettings.get_activeBuildTarget()));
        this.LastUpdateCheck = DateTime.UtcNow;
      }
      return this.mUpdateCheckRequest != null;
    }

    private void SaveAsAsset()
    {
      if (this.mActiveBehaviorSource == null)
        return;
      string path = EditorUtility.SaveFilePanel("Save Behavior Tree", "Assets", (string) this.mActiveBehaviorSource.behaviorName + ".asset", "asset");
      if (path.Length != 0 && Application.get_dataPath().Length < path.Length)
      {
        Type type = Type.GetType("BehaviorDesigner.Runtime.ExternalBehaviorTree, Assembly-CSharp") ?? Type.GetType("BehaviorDesigner.Runtime.ExternalBehaviorTree, Assembly-CSharp-firstpass");
        if (BehaviorDesignerPreferences.GetBool(BDPreferences.BinarySerialization))
          BinarySerialization.Save(this.mActiveBehaviorSource);
        else
          JSONSerialization.Save(this.mActiveBehaviorSource);
        ExternalBehavior instance = ScriptableObject.CreateInstance(type) as ExternalBehavior;
        BehaviorSource behaviorSource = new BehaviorSource((IBehavior) instance);
        behaviorSource.behaviorName = this.mActiveBehaviorSource.behaviorName;
        behaviorSource.behaviorDescription = this.mActiveBehaviorSource.behaviorDescription;
        behaviorSource.set_TaskData(this.mActiveBehaviorSource.get_TaskData());
        instance.SetBehaviorSource(behaviorSource);
        string str = string.Format("Assets/{0}", (object) path.Substring(Application.get_dataPath().Length + 1));
        AssetDatabase.DeleteAsset(str);
        AssetDatabase.CreateAsset((Object) instance, str);
        AssetDatabase.ImportAsset(str);
        Selection.set_activeObject((Object) instance);
      }
      else
      {
        if (!Path.GetExtension(path).Equals(".asset"))
          return;
        Debug.LogError((object) "Error: Unable to save external behavior tree. The save location must be within the Asset directory.");
      }
    }

    private void SaveAsPrefab()
    {
      if (this.mActiveBehaviorSource == null)
        return;
      string path = EditorUtility.SaveFilePanel("Save Behavior Tree", "Assets", (string) this.mActiveBehaviorSource.behaviorName + ".prefab", "prefab");
      if (path.Length != 0 && Application.get_dataPath().Length < path.Length)
      {
        GameObject gameObject = new GameObject();
        Type type = Type.GetType("BehaviorDesigner.Runtime.BehaviorTree, Assembly-CSharp") ?? Type.GetType("BehaviorDesigner.Runtime.BehaviorTree, Assembly-CSharp-firstpass");
        Behavior behavior = gameObject.AddComponent(type) as Behavior;
        BehaviorSource behaviorSource = new BehaviorSource((IBehavior) behavior);
        behaviorSource.behaviorName = this.mActiveBehaviorSource.behaviorName;
        behaviorSource.behaviorDescription = this.mActiveBehaviorSource.behaviorDescription;
        behaviorSource.set_TaskData(this.mActiveBehaviorSource.get_TaskData());
        behavior.SetBehaviorSource(behaviorSource);
        string str = string.Format("Assets/{0}", (object) path.Substring(Application.get_dataPath().Length + 1));
        AssetDatabase.DeleteAsset(str);
        GameObject prefab = PrefabUtility.CreatePrefab(str, gameObject);
        Object.DestroyImmediate((Object) gameObject, true);
        AssetDatabase.ImportAsset(str);
        Selection.set_activeObject((Object) prefab);
      }
      else
      {
        if (!Path.GetExtension(path).Equals(".prefab"))
          return;
        Debug.LogError((object) "Error: Unable to save prefab. The save location must be within the Asset directory.");
      }
    }

    public void LoadBehavior(BehaviorSource behaviorSource, bool loadPrevBehavior)
    {
      this.LoadBehavior(behaviorSource, loadPrevBehavior, false);
    }

    public void LoadBehavior(
      BehaviorSource behaviorSource,
      bool loadPrevBehavior,
      bool inspectorLoad)
    {
      if (behaviorSource == null || object.ReferenceEquals((object) behaviorSource.get_Owner(), (object) null) || ((object) behaviorSource.get_Owner()).Equals((object) null))
        return;
      if (inspectorLoad && !this.mSizesInitialized)
      {
        this.mActiveBehaviorID = behaviorSource.get_Owner().GetInstanceID();
        this.mPrevActiveObject = Selection.get_activeObject();
        this.mLoadedFromInspector = true;
      }
      else
      {
        if (!this.mSizesInitialized)
          return;
        if (!loadPrevBehavior)
        {
          this.DisableReferenceTasks();
          this.mVariableInspector.ResetSelectedVariableIndex();
        }
        this.mExternalParent = (BehaviorSource) null;
        this.mActiveBehaviorSource = behaviorSource;
        if (behaviorSource.get_Owner() is Behavior)
        {
          this.mActiveObject = (Object) ((Component) (behaviorSource.get_Owner() as Behavior)).get_gameObject();
          ExternalBehavior externalBehavior = (behaviorSource.get_Owner() as Behavior).get_ExternalBehavior();
          if (Object.op_Inequality((Object) externalBehavior, (Object) null) && !EditorApplication.get_isPlayingOrWillChangePlaymode())
          {
            this.mActiveBehaviorSource = externalBehavior.get_BehaviorSource();
            this.mActiveBehaviorSource.set_Owner((IBehavior) externalBehavior);
            this.mExternalParent = behaviorSource;
            behaviorSource.CheckForSerialization(true, (BehaviorSource) null);
            if (VariableInspector.SyncVariables(behaviorSource, this.mActiveBehaviorSource.get_Variables()))
            {
              if (BehaviorDesignerPreferences.GetBool(BDPreferences.BinarySerialization))
                BinarySerialization.Save(behaviorSource);
              else
                JSONSerialization.Save(behaviorSource);
            }
          }
        }
        else
          this.mActiveObject = behaviorSource.get_Owner().GetObject();
        this.mActiveBehaviorSource.set_BehaviorID(this.mActiveBehaviorSource.get_Owner().GetInstanceID());
        this.mActiveBehaviorID = this.mActiveBehaviorSource.get_BehaviorID();
        this.mPrevActiveObject = Selection.get_activeObject();
        if (this.mBehaviorSourceHistory.Count == 0 || this.mBehaviorSourceHistoryIndex >= this.mBehaviorSourceHistory.Count || Object.op_Equality(this.mBehaviorSourceHistory[this.mBehaviorSourceHistoryIndex], (Object) null) || (this.mBehaviorSourceHistory[this.mBehaviorSourceHistoryIndex] as IBehavior).GetBehaviorSource() != null && !this.mActiveBehaviorSource.get_BehaviorID().Equals((this.mBehaviorSourceHistory[this.mBehaviorSourceHistoryIndex] as IBehavior).GetBehaviorSource().get_BehaviorID()))
        {
          for (int index = this.mBehaviorSourceHistory.Count - 1; index > this.mBehaviorSourceHistoryIndex; --index)
            this.mBehaviorSourceHistory.RemoveAt(index);
          this.mBehaviorSourceHistory.Add(this.mActiveBehaviorSource.get_Owner().GetObject());
          ++this.mBehaviorSourceHistoryIndex;
        }
        Vector2 vector2;
        ((Vector2) ref vector2).\u002Ector(((Rect) ref this.mGraphRect).get_width() / (2f * this.mGraphZoom), 150f);
        Vector2 nodePosition = Vector2.op_Subtraction(vector2, this.mGraphOffset);
        if (this.mGraphDesigner.Load(this.mActiveBehaviorSource, loadPrevBehavior && !this.mLoadedFromInspector, nodePosition) && this.mGraphDesigner.HasEntryNode() && (!loadPrevBehavior || this.mLoadedFromInspector))
        {
          this.mGraphOffset = Vector2.op_Subtraction(new Vector2(((Rect) ref this.mGraphRect).get_width() / (2f * this.mGraphZoom), 50f), this.mGraphDesigner.EntryNodeOffset());
          this.mGraphScrollPosition = Vector2.op_Subtraction(Vector2.op_Division(Vector2.op_Subtraction(this.mGraphScrollSize, new Vector2(((Rect) ref this.mGraphRect).get_width(), ((Rect) ref this.mGraphRect).get_height())), 2f), Vector2.op_Multiply(2f, new Vector2(15f, 15f)));
        }
        this.mLoadedFromInspector = false;
        if (!this.mLockActiveGameObject)
          Selection.set_activeObject(this.mActiveObject);
        if (EditorApplication.get_isPlaying() && this.mActiveBehaviorSource != null)
        {
          this.mRightClickMenu = (GenericMenu) null;
          this.mUpdateNodeTaskMap = true;
          this.UpdateNodeTaskMap();
        }
        this.CheckForErrors();
        this.UpdateGraphStatus();
        this.ClearBreadcrumbMenu();
        this.Repaint();
      }
    }

    public void ClearGraph()
    {
      this.mGraphDesigner.Clear(true);
      this.mActiveBehaviorSource = (BehaviorSource) null;
      this.CheckForErrors();
      this.UpdateGraphStatus();
      this.Repaint();
    }

    private enum BreadcrumbMenuType
    {
      GameObjectBehavior,
      GameObject,
      Behavior,
    }

    public delegate void TaskCallbackHandler(BehaviorSource behaviorSource, Task task);
  }
}
