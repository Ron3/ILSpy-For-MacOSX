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
		public enum TaskTypes
		{
			Action = 0,
			Composite = 1,
			Conditional = 2,
			Decorator = 3,
			Last = 4
		}

		private class SearchableType
		{
			private Type mType;

			private bool mVisible = true;

			private string mName;

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

			public SearchableType(Type type)
			{
				this.mType = type;
				this.mName = BehaviorDesignerUtility.SplitCamelCase(this.mType.get_Name());
			}
		}

		private class CategoryList
		{
			private string mName = string.Empty;

			private string mFullpath = string.Empty;

			private List<TaskList.CategoryList> mSubcategories;

			private List<TaskList.SearchableType> mTasks;

			private bool mExpanded = true;

			private bool mVisible = true;

			private int mID;

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

			public CategoryList(string name, string fullpath, bool expanded, int id)
			{
				this.mName = name;
				this.mFullpath = fullpath;
				this.mExpanded = expanded;
				this.mID = id;
			}

			public void addSubcategory(TaskList.CategoryList category)
			{
				if (this.mSubcategories == null)
				{
					this.mSubcategories = new List<TaskList.CategoryList>();
				}
				this.mSubcategories.Add(category);
			}

			public void addTask(Type taskType)
			{
				if (this.mTasks == null)
				{
					this.mTasks = new List<TaskList.SearchableType>();
				}
				this.mTasks.Add(new TaskList.SearchableType(taskType));
			}
		}

		private List<TaskList.CategoryList> mCategoryList;

		private Dictionary<Type, TaskNameAttribute[]> mTaskNameAttribute = new Dictionary<Type, TaskNameAttribute[]>();

		private Vector2 mScrollPosition = Vector2.get_zero();

		private string mSearchString = string.Empty;

		private bool mFocusSearch;

		public void OnEnable()
		{
			base.set_hideFlags(61);
		}

		public void Init()
		{
			this.mCategoryList = new List<TaskList.CategoryList>();
			List<Type> list = new List<Type>();
			Assembly[] assemblies = AppDomain.get_CurrentDomain().GetAssemblies();
			for (int i = 0; i < assemblies.Length; i++)
			{
				Type[] types = assemblies[i].GetTypes();
				for (int j = 0; j < types.Length; j++)
				{
					if (!types[j].Equals(typeof(BehaviorReference)) && !types[j].get_IsAbstract())
					{
						if (types[j].IsSubclassOf(typeof(Action)) || types[j].IsSubclassOf(typeof(Composite)) || types[j].IsSubclassOf(typeof(Conditional)) || types[j].IsSubclassOf(typeof(Decorator)))
						{
							list.Add(types[j]);
						}
					}
				}
			}
			list.Sort(new AlphanumComparator<Type>());
			Dictionary<string, TaskList.CategoryList> dictionary = new Dictionary<string, TaskList.CategoryList>();
			string text = string.Empty;
			int id = 0;
			for (int k = 0; k < list.get_Count(); k++)
			{
				if (list.get_Item(k).IsSubclassOf(typeof(Action)))
				{
					text = "Actions";
				}
				else if (list.get_Item(k).IsSubclassOf(typeof(Composite)))
				{
					text = "Composites";
				}
				else if (list.get_Item(k).IsSubclassOf(typeof(Conditional)))
				{
					text = "Conditionals";
				}
				else
				{
					text = "Decorators";
				}
				TaskCategoryAttribute[] array;
				if ((array = (list.get_Item(k).GetCustomAttributes(typeof(TaskCategoryAttribute), false) as TaskCategoryAttribute[])).Length > 0)
				{
					text = text + "/" + array[0].get_Category();
				}
				string text2 = string.Empty;
				string[] array2 = text.Split(new char[]
				{
					'/'
				});
				TaskList.CategoryList categoryList = null;
				TaskList.CategoryList categoryList2;
				for (int l = 0; l < array2.Length; l++)
				{
					if (l > 0)
					{
						text2 += "/";
					}
					text2 += array2[l];
					if (!dictionary.ContainsKey(text2))
					{
						categoryList2 = new TaskList.CategoryList(array2[l], text2, this.PreviouslyExpanded(id), id++);
						if (categoryList == null)
						{
							this.mCategoryList.Add(categoryList2);
						}
						else
						{
							categoryList.addSubcategory(categoryList2);
						}
						dictionary.Add(text2, categoryList2);
					}
					else
					{
						categoryList2 = dictionary.get_Item(text2);
					}
					categoryList = categoryList2;
				}
				categoryList2 = dictionary.get_Item(text2);
				categoryList2.addTask(list.get_Item(k));
			}
			this.Search(BehaviorDesignerUtility.SplitCamelCase(this.mSearchString).ToLower().Replace(" ", string.Empty), this.mCategoryList);
		}

		public void AddTasksToMenu(ref GenericMenu genericMenu, Type selectedTaskType, string parentName, GenericMenu.MenuFunction2 menuFunction)
		{
			this.AddCategoryTasksToMenu(ref genericMenu, this.mCategoryList, selectedTaskType, parentName, menuFunction);
		}

		public void AddConditionalTasksToMenu(ref GenericMenu genericMenu, Type selectedTaskType, string parentName, GenericMenu.MenuFunction2 menuFunction)
		{
			if (this.mCategoryList.get_Item(2).Tasks != null)
			{
				for (int i = 0; i < this.mCategoryList.get_Item(2).Tasks.get_Count(); i++)
				{
					if (parentName.Equals(string.Empty))
					{
						genericMenu.AddItem(new GUIContent(string.Format("{0}/{1}", this.mCategoryList.get_Item(2).Fullpath, this.mCategoryList.get_Item(2).Tasks.get_Item(i).Name.ToString())), this.mCategoryList.get_Item(2).Tasks.get_Item(i).Type.Equals(selectedTaskType), menuFunction, this.mCategoryList.get_Item(2).Tasks.get_Item(i).Type);
					}
					else
					{
						genericMenu.AddItem(new GUIContent(string.Format("{0}/{1}/{2}", parentName, this.mCategoryList.get_Item(22).Fullpath, this.mCategoryList.get_Item(2).Tasks.get_Item(i).Name.ToString())), this.mCategoryList.get_Item(2).Tasks.get_Item(i).Type.Equals(selectedTaskType), menuFunction, this.mCategoryList.get_Item(2).Tasks.get_Item(i).Type);
					}
				}
			}
			this.AddCategoryTasksToMenu(ref genericMenu, this.mCategoryList.get_Item(2).Subcategories, selectedTaskType, parentName, menuFunction);
		}

		private void AddCategoryTasksToMenu(ref GenericMenu genericMenu, List<TaskList.CategoryList> categoryList, Type selectedTaskType, string parentName, GenericMenu.MenuFunction2 menuFunction)
		{
			for (int i = 0; i < categoryList.get_Count(); i++)
			{
				if (categoryList.get_Item(i).Subcategories != null)
				{
					this.AddCategoryTasksToMenu(ref genericMenu, categoryList.get_Item(i).Subcategories, selectedTaskType, parentName, menuFunction);
				}
				if (categoryList.get_Item(i).Tasks != null)
				{
					for (int j = 0; j < categoryList.get_Item(i).Tasks.get_Count(); j++)
					{
						if (parentName.Equals(string.Empty))
						{
							genericMenu.AddItem(new GUIContent(string.Format("{0}/{1}", categoryList.get_Item(i).Fullpath, categoryList.get_Item(i).Tasks.get_Item(j).Name.ToString())), categoryList.get_Item(i).Tasks.get_Item(j).Type.Equals(selectedTaskType), menuFunction, categoryList.get_Item(i).Tasks.get_Item(j).Type);
						}
						else
						{
							genericMenu.AddItem(new GUIContent(string.Format("{0}/{1}/{2}", parentName, categoryList.get_Item(i).Fullpath, categoryList.get_Item(i).Tasks.get_Item(j).Name.ToString())), categoryList.get_Item(i).Tasks.get_Item(j).Type.Equals(selectedTaskType), menuFunction, categoryList.get_Item(i).Tasks.get_Item(j).Type);
						}
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
			string text = GUILayout.TextField(this.mSearchString, GUI.get_skin().FindStyle("ToolbarSeachTextField"), new GUILayoutOption[0]);
			if (this.mFocusSearch)
			{
				GUI.FocusControl("Search");
				this.mFocusSearch = false;
			}
			if (!this.mSearchString.Equals(text))
			{
				this.mSearchString = text;
				this.Search(BehaviorDesignerUtility.SplitCamelCase(this.mSearchString).ToLower().Replace(" ", string.Empty), this.mCategoryList);
			}
			if (GUILayout.Button(string.Empty, (!this.mSearchString.Equals(string.Empty)) ? GUI.get_skin().FindStyle("ToolbarSeachCancelButton") : GUI.get_skin().FindStyle("ToolbarSeachCancelButtonEmpty"), new GUILayoutOption[0]))
			{
				this.mSearchString = string.Empty;
				this.Search(string.Empty, this.mCategoryList);
				GUI.FocusControl(null);
			}
			GUILayout.EndHorizontal();
			BehaviorDesignerUtility.DrawContentSeperator(2);
			GUILayout.Space(4f);
			this.mScrollPosition = GUILayout.BeginScrollView(this.mScrollPosition, new GUILayoutOption[0]);
			GUI.set_enabled(enabled);
			if (this.mCategoryList.get_Count() > 1)
			{
				this.DrawCategory(window, this.mCategoryList.get_Item(1));
			}
			if (this.mCategoryList.get_Count() > 3)
			{
				this.DrawCategory(window, this.mCategoryList.get_Item(3));
			}
			if (this.mCategoryList.get_Count() > 0)
			{
				this.DrawCategory(window, this.mCategoryList.get_Item(0));
			}
			if (this.mCategoryList.get_Count() > 2)
			{
				this.DrawCategory(window, this.mCategoryList.get_Item(2));
			}
			GUI.set_enabled(true);
			GUILayout.EndScrollView();
		}

		private void DrawCategory(BehaviorDesignerWindow window, TaskList.CategoryList category)
		{
			if (category.Visible)
			{
				category.Expanded = EditorGUILayout.Foldout(category.Expanded, category.Name, BehaviorDesignerUtility.TaskFoldoutGUIStyle);
				this.SetExpanded(category.ID, category.Expanded);
				if (category.Expanded)
				{
					EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() + 1);
					if (category.Tasks != null)
					{
						for (int i = 0; i < category.Tasks.get_Count(); i++)
						{
							if (category.Tasks.get_Item(i).Visible)
							{
								GUILayout.BeginHorizontal(new GUILayoutOption[0]);
								GUILayout.Space((float)(EditorGUI.get_indentLevel() * 16));
								TaskNameAttribute[] array = null;
								if (!this.mTaskNameAttribute.TryGetValue(category.Tasks.get_Item(i).Type, ref array))
								{
									array = (category.Tasks.get_Item(i).Type.GetCustomAttributes(typeof(TaskNameAttribute), false) as TaskNameAttribute[]);
									this.mTaskNameAttribute.Add(category.Tasks.get_Item(i).Type, array);
								}
								string name;
								if (array != null && array.Length > 0)
								{
									name = array[0].get_Name();
								}
								else
								{
									name = category.Tasks.get_Item(i).Name;
								}
								if (GUILayout.Button(name, EditorStyles.get_toolbarButton(), new GUILayoutOption[]
								{
									GUILayout.MaxWidth((float)(300 - EditorGUI.get_indentLevel() * 16 - 24))
								}))
								{
									window.AddTask(category.Tasks.get_Item(i).Type, false);
								}
								GUILayout.Space(3f);
								GUILayout.EndHorizontal();
							}
						}
					}
					if (category.Subcategories != null)
					{
						this.DrawCategoryTaskList(window, category.Subcategories);
					}
					EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() - 1);
				}
			}
		}

		private void DrawCategoryTaskList(BehaviorDesignerWindow window, List<TaskList.CategoryList> categoryList)
		{
			for (int i = 0; i < categoryList.get_Count(); i++)
			{
				this.DrawCategory(window, categoryList.get_Item(i));
			}
		}

		private bool Search(string searchString, List<TaskList.CategoryList> categoryList)
		{
			bool result = searchString.Equals(string.Empty);
			for (int i = 0; i < categoryList.get_Count(); i++)
			{
				bool flag = false;
				categoryList.get_Item(i).Visible = false;
				if (categoryList.get_Item(i).Subcategories != null && this.Search(searchString, categoryList.get_Item(i).Subcategories))
				{
					categoryList.get_Item(i).Visible = true;
					result = true;
				}
				if (BehaviorDesignerUtility.SplitCamelCase(categoryList.get_Item(i).Name).ToLower().Replace(" ", string.Empty).Contains(searchString))
				{
					result = true;
					flag = true;
					categoryList.get_Item(i).Visible = true;
					if (categoryList.get_Item(i).Subcategories != null)
					{
						this.MarkVisible(categoryList.get_Item(i).Subcategories);
					}
				}
				if (categoryList.get_Item(i).Tasks != null)
				{
					for (int j = 0; j < categoryList.get_Item(i).Tasks.get_Count(); j++)
					{
						categoryList.get_Item(i).Tasks.get_Item(j).Visible = searchString.Equals(string.Empty);
						if (flag || categoryList.get_Item(i).Tasks.get_Item(j).Name.ToLower().Replace(" ", string.Empty).Contains(searchString))
						{
							categoryList.get_Item(i).Tasks.get_Item(j).Visible = true;
							result = true;
							categoryList.get_Item(i).Visible = true;
						}
					}
				}
			}
			return result;
		}

		private void MarkVisible(List<TaskList.CategoryList> categoryList)
		{
			for (int i = 0; i < categoryList.get_Count(); i++)
			{
				categoryList.get_Item(i).Visible = true;
				if (categoryList.get_Item(i).Subcategories != null)
				{
					this.MarkVisible(categoryList.get_Item(i).Subcategories);
				}
				if (categoryList.get_Item(i).Tasks != null)
				{
					for (int j = 0; j < categoryList.get_Item(i).Tasks.get_Count(); j++)
					{
						categoryList.get_Item(i).Tasks.get_Item(j).Visible = true;
					}
				}
			}
		}

		private bool PreviouslyExpanded(int id)
		{
			return EditorPrefs.GetBool("BehaviorDesignerTaskList" + id, true);
		}

		private void SetExpanded(int id, bool visible)
		{
			EditorPrefs.SetBool("BehaviorDesignerTaskList" + id, visible);
		}
	}
}