// Decompiled with JetBrains decompiler
// Type: BehaviorDesigner.Editor.TaskList
// Assembly: BehaviorDesignerEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 99CE4D00-DFA2-42D1-ABFC-D630AB4C1372
// Assembly location: C:\Users\Ron\Desktop\bt\BehaviorDesignerEditor.dll

using BehaviorDesigner.Runtime.Tasks;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace BehaviorDesigner.Editor
{
  [Serializable]
  public class TaskList : ScriptableObject
  {
    private List<TaskList.CategoryList> mCategoryList;
    private Dictionary<Type, TaskNameAttribute[]> mTaskNameAttribute;
    private Vector2 mScrollPosition;
    private string mSearchString;
    private bool mFocusSearch;

    public TaskList()
    {
      base.\u002Ector();
    }

    public void OnEnable()
    {
      ((Object) this).set_hideFlags((HideFlags) 61);
    }

    public void Init()
    {
      this.mCategoryList = new List<TaskList.CategoryList>();
      List<Type> typeList = new List<Type>();
      foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
      {
        Type[] types = assembly.GetTypes();
        for (int index = 0; index < types.Length; ++index)
        {
          if (!types[index].Equals(typeof (BehaviorReference)) && !types[index].IsAbstract && (types[index].IsSubclassOf(typeof (Action)) || types[index].IsSubclassOf(typeof (Composite)) || (types[index].IsSubclassOf(typeof (Conditional)) || types[index].IsSubclassOf(typeof (Decorator)))))
            typeList.Add(types[index]);
        }
      }
      typeList.Sort((IComparer<Type>) new AlphanumComparator<Type>());
      Dictionary<string, TaskList.CategoryList> dictionary = new Dictionary<string, TaskList.CategoryList>();
      string empty1 = string.Empty;
      int id = 0;
      for (int index1 = 0; index1 < typeList.Count; ++index1)
      {
        string str = !typeList[index1].IsSubclassOf(typeof (Action)) ? (!typeList[index1].IsSubclassOf(typeof (Composite)) ? (!typeList[index1].IsSubclassOf(typeof (Conditional)) ? "Decorators" : "Conditionals") : "Composites") : "Actions";
        TaskCategoryAttribute[] customAttributes;
        if ((customAttributes = typeList[index1].GetCustomAttributes(typeof (TaskCategoryAttribute), false) as TaskCategoryAttribute[]).Length > 0)
          str = str + "/" + customAttributes[0].get_Category();
        string empty2 = string.Empty;
        string[] strArray = str.Split('/');
        TaskList.CategoryList categoryList = (TaskList.CategoryList) null;
        for (int index2 = 0; index2 < strArray.Length; ++index2)
        {
          if (index2 > 0)
            empty2 += "/";
          empty2 += strArray[index2];
          TaskList.CategoryList category;
          if (!dictionary.ContainsKey(empty2))
          {
            category = new TaskList.CategoryList(strArray[index2], empty2, this.PreviouslyExpanded(id), id++);
            if (categoryList == null)
              this.mCategoryList.Add(category);
            else
              categoryList.addSubcategory(category);
            dictionary.Add(empty2, category);
          }
          else
            category = dictionary[empty2];
          categoryList = category;
        }
        dictionary[empty2].addTask(typeList[index1]);
      }
      this.Search(BehaviorDesignerUtility.SplitCamelCase(this.mSearchString).ToLower().Replace(" ", string.Empty), this.mCategoryList);
    }

    public void AddTasksToMenu(
      ref GenericMenu genericMenu,
      Type selectedTaskType,
      string parentName,
      GenericMenu.MenuFunction2 menuFunction)
    {
      this.AddCategoryTasksToMenu(ref genericMenu, this.mCategoryList, selectedTaskType, parentName, menuFunction);
    }

    public void AddConditionalTasksToMenu(
      ref GenericMenu genericMenu,
      Type selectedTaskType,
      string parentName,
      GenericMenu.MenuFunction2 menuFunction)
    {
      if (this.mCategoryList[2].Tasks != null)
      {
        for (int index = 0; index < this.mCategoryList[2].Tasks.Count; ++index)
        {
          if (parentName.Equals(string.Empty))
            genericMenu.AddItem(new GUIContent(string.Format("{0}/{1}", (object) this.mCategoryList[2].Fullpath, (object) this.mCategoryList[2].Tasks[index].Name.ToString())), this.mCategoryList[2].Tasks[index].Type.Equals(selectedTaskType), menuFunction, (object) this.mCategoryList[2].Tasks[index].Type);
          else
            genericMenu.AddItem(new GUIContent(string.Format("{0}/{1}/{2}", (object) parentName, (object) this.mCategoryList[22].Fullpath, (object) this.mCategoryList[2].Tasks[index].Name.ToString())), this.mCategoryList[2].Tasks[index].Type.Equals(selectedTaskType), menuFunction, (object) this.mCategoryList[2].Tasks[index].Type);
        }
      }
      this.AddCategoryTasksToMenu(ref genericMenu, this.mCategoryList[2].Subcategories, selectedTaskType, parentName, menuFunction);
    }

    private void AddCategoryTasksToMenu(
      ref GenericMenu genericMenu,
      List<TaskList.CategoryList> categoryList,
      Type selectedTaskType,
      string parentName,
      GenericMenu.MenuFunction2 menuFunction)
    {
      for (int index1 = 0; index1 < categoryList.Count; ++index1)
      {
        if (categoryList[index1].Subcategories != null)
          this.AddCategoryTasksToMenu(ref genericMenu, categoryList[index1].Subcategories, selectedTaskType, parentName, menuFunction);
        if (categoryList[index1].Tasks != null)
        {
          for (int index2 = 0; index2 < categoryList[index1].Tasks.Count; ++index2)
          {
            if (parentName.Equals(string.Empty))
              genericMenu.AddItem(new GUIContent(string.Format("{0}/{1}", (object) categoryList[index1].Fullpath, (object) categoryList[index1].Tasks[index2].Name.ToString())), categoryList[index1].Tasks[index2].Type.Equals(selectedTaskType), menuFunction, (object) categoryList[index1].Tasks[index2].Type);
            else
              genericMenu.AddItem(new GUIContent(string.Format("{0}/{1}/{2}", (object) parentName, (object) categoryList[index1].Fullpath, (object) categoryList[index1].Tasks[index2].Name.ToString())), categoryList[index1].Tasks[index2].Type.Equals(selectedTaskType), menuFunction, (object) categoryList[index1].Tasks[index2].Type);
          }
        }
      }
    }

    public void FocusSearchField()
    {
      this.mFocusSearch = true;
    }

    public void DrawTaskList(BehaviorDesignerWindow window, bool enabled)
    {
      GUILayout.BeginHorizontal(new GUILayoutOption[0]);
      GUI.SetNextControlName("Search");
      string str = GUILayout.TextField(this.mSearchString, GUI.get_skin().FindStyle("ToolbarSeachTextField"), new GUILayoutOption[0]);
      if (this.mFocusSearch)
      {
        GUI.FocusControl("Search");
        this.mFocusSearch = false;
      }
      if (!this.mSearchString.Equals(str))
      {
        this.mSearchString = str;
        this.Search(BehaviorDesignerUtility.SplitCamelCase(this.mSearchString).ToLower().Replace(" ", string.Empty), this.mCategoryList);
      }
      if (GUILayout.Button(string.Empty, !this.mSearchString.Equals(string.Empty) ? GUI.get_skin().FindStyle("ToolbarSeachCancelButton") : GUI.get_skin().FindStyle("ToolbarSeachCancelButtonEmpty"), new GUILayoutOption[0]))
      {
        this.mSearchString = string.Empty;
        this.Search(string.Empty, this.mCategoryList);
        GUI.FocusControl((string) null);
      }
      GUILayout.EndHorizontal();
      BehaviorDesignerUtility.DrawContentSeperator(2);
      GUILayout.Space(4f);
      this.mScrollPosition = GUILayout.BeginScrollView(this.mScrollPosition, new GUILayoutOption[0]);
      GUI.set_enabled(enabled);
      if (this.mCategoryList.Count > 1)
        this.DrawCategory(window, this.mCategoryList[1]);
      if (this.mCategoryList.Count > 3)
        this.DrawCategory(window, this.mCategoryList[3]);
      if (this.mCategoryList.Count > 0)
        this.DrawCategory(window, this.mCategoryList[0]);
      if (this.mCategoryList.Count > 2)
        this.DrawCategory(window, this.mCategoryList[2]);
      GUI.set_enabled(true);
      GUILayout.EndScrollView();
    }

    private void DrawCategory(BehaviorDesignerWindow window, TaskList.CategoryList category)
    {
      if (!category.Visible)
        return;
      category.Expanded = EditorGUILayout.Foldout(category.Expanded, category.Name, BehaviorDesignerUtility.TaskFoldoutGUIStyle);
      this.SetExpanded(category.ID, category.Expanded);
      if (!category.Expanded)
        return;
      EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() + 1);
      if (category.Tasks != null)
      {
        for (int index = 0; index < category.Tasks.Count; ++index)
        {
          if (category.Tasks[index].Visible)
          {
            GUILayout.BeginHorizontal(new GUILayoutOption[0]);
            GUILayout.Space((float) (EditorGUI.get_indentLevel() * 16));
            TaskNameAttribute[] taskNameAttributeArray = (TaskNameAttribute[]) null;
            if (!this.mTaskNameAttribute.TryGetValue(category.Tasks[index].Type, out taskNameAttributeArray))
            {
              taskNameAttributeArray = category.Tasks[index].Type.GetCustomAttributes(typeof (TaskNameAttribute), false) as TaskNameAttribute[];
              this.mTaskNameAttribute.Add(category.Tasks[index].Type, taskNameAttributeArray);
            }
            if (GUILayout.Button(taskNameAttributeArray == null || taskNameAttributeArray.Length <= 0 ? category.Tasks[index].Name : taskNameAttributeArray[0].get_Name(), EditorStyles.get_toolbarButton(), new GUILayoutOption[1]
            {
              GUILayout.MaxWidth((float) (300 - EditorGUI.get_indentLevel() * 16 - 24))
            }))
              window.AddTask(category.Tasks[index].Type, false);
            GUILayout.Space(3f);
            GUILayout.EndHorizontal();
          }
        }
      }
      if (category.Subcategories != null)
        this.DrawCategoryTaskList(window, category.Subcategories);
      EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() - 1);
    }

    private void DrawCategoryTaskList(
      BehaviorDesignerWindow window,
      List<TaskList.CategoryList> categoryList)
    {
      for (int index = 0; index < categoryList.Count; ++index)
        this.DrawCategory(window, categoryList[index]);
    }

    private bool Search(string searchString, List<TaskList.CategoryList> categoryList)
    {
      bool flag1 = searchString.Equals(string.Empty);
      for (int index1 = 0; index1 < categoryList.Count; ++index1)
      {
        bool flag2 = false;
        categoryList[index1].Visible = false;
        if (categoryList[index1].Subcategories != null && this.Search(searchString, categoryList[index1].Subcategories))
        {
          categoryList[index1].Visible = true;
          flag1 = true;
        }
        if (BehaviorDesignerUtility.SplitCamelCase(categoryList[index1].Name).ToLower().Replace(" ", string.Empty).Contains(searchString))
        {
          flag1 = true;
          flag2 = true;
          categoryList[index1].Visible = true;
          if (categoryList[index1].Subcategories != null)
            this.MarkVisible(categoryList[index1].Subcategories);
        }
        if (categoryList[index1].Tasks != null)
        {
          for (int index2 = 0; index2 < categoryList[index1].Tasks.Count; ++index2)
          {
            categoryList[index1].Tasks[index2].Visible = searchString.Equals(string.Empty);
            if (flag2 || categoryList[index1].Tasks[index2].Name.ToLower().Replace(" ", string.Empty).Contains(searchString))
            {
              categoryList[index1].Tasks[index2].Visible = true;
              flag1 = true;
              categoryList[index1].Visible = true;
            }
          }
        }
      }
      return flag1;
    }

    private void MarkVisible(List<TaskList.CategoryList> categoryList)
    {
      for (int index1 = 0; index1 < categoryList.Count; ++index1)
      {
        categoryList[index1].Visible = true;
        if (categoryList[index1].Subcategories != null)
          this.MarkVisible(categoryList[index1].Subcategories);
        if (categoryList[index1].Tasks != null)
        {
          for (int index2 = 0; index2 < categoryList[index1].Tasks.Count; ++index2)
            categoryList[index1].Tasks[index2].Visible = true;
        }
      }
    }

    private bool PreviouslyExpanded(int id)
    {
      return EditorPrefs.GetBool("BehaviorDesignerTaskList" + (object) id, true);
    }

    private void SetExpanded(int id, bool visible)
    {
      EditorPrefs.SetBool("BehaviorDesignerTaskList" + (object) id, visible);
    }

    public enum TaskTypes
    {
      Action,
      Composite,
      Conditional,
      Decorator,
      Last,
    }

    private class SearchableType
    {
      private bool mVisible = true;
      private Type mType;
      private string mName;

      public SearchableType(Type type)
      {
        this.mType = type;
        this.mName = BehaviorDesignerUtility.SplitCamelCase(this.mType.Name);
      }

      public Type Type
      {
        get
        {
          return this.mType;
        }
      }

      public bool Visible
      {
        get
        {
          return this.mVisible;
        }
        set
        {
          this.mVisible = value;
        }
      }

      public string Name
      {
        get
        {
          return this.mName;
        }
      }
    }

    private class CategoryList
    {
      private string mName = string.Empty;
      private string mFullpath = string.Empty;
      private bool mExpanded = true;
      private bool mVisible = true;
      private List<TaskList.CategoryList> mSubcategories;
      private List<TaskList.SearchableType> mTasks;
      private int mID;

      public CategoryList(string name, string fullpath, bool expanded, int id)
      {
        this.mName = name;
        this.mFullpath = fullpath;
        this.mExpanded = expanded;
        this.mID = id;
      }

      public string Name
      {
        get
        {
          return this.mName;
        }
      }

      public string Fullpath
      {
        get
        {
          return this.mFullpath;
        }
      }

      public List<TaskList.CategoryList> Subcategories
      {
        get
        {
          return this.mSubcategories;
        }
      }

      public List<TaskList.SearchableType> Tasks
      {
        get
        {
          return this.mTasks;
        }
      }

      public bool Expanded
      {
        get
        {
          return this.mExpanded;
        }
        set
        {
          this.mExpanded = value;
        }
      }

      public bool Visible
      {
        get
        {
          return this.mVisible;
        }
        set
        {
          this.mVisible = value;
        }
      }

      public int ID
      {
        get
        {
          return this.mID;
        }
      }

      public void addSubcategory(TaskList.CategoryList category)
      {
        if (this.mSubcategories == null)
          this.mSubcategories = new List<TaskList.CategoryList>();
        this.mSubcategories.Add(category);
      }

      public void addTask(Type taskType)
      {
        if (this.mTasks == null)
          this.mTasks = new List<TaskList.SearchableType>();
        this.mTasks.Add(new TaskList.SearchableType(taskType));
      }
    }
  }
}
