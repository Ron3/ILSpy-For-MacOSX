using BehaviorDesigner.Runtime;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BehaviorDesigner.Editor
{
	[InitializeOnLoad]
	public class GizmoManager
	{
		private static string currentScene;

		static GizmoManager()
		{
			GizmoManager.currentScene = SceneManager.GetActiveScene().get_name();
			EditorApplication.hierarchyWindowChanged = (EditorApplication.CallbackFunction)Delegate.Combine(EditorApplication.hierarchyWindowChanged, new EditorApplication.CallbackFunction(GizmoManager.HierarchyChange));
			if (!Application.get_isPlaying())
			{
				GizmoManager.UpdateAllGizmos();
				EditorApplication.playmodeStateChanged = (EditorApplication.CallbackFunction)Delegate.Combine(EditorApplication.playmodeStateChanged, new EditorApplication.CallbackFunction(GizmoManager.UpdateAllGizmos));
			}
		}

		public static void UpdateAllGizmos()
		{
			Behavior[] array = Object.FindObjectsOfType<Behavior>();
			for (int i = 0; i < array.Length; i++)
			{
				GizmoManager.UpdateGizmo(array[i]);
			}
		}

		public static void UpdateGizmo(Behavior behavior)
		{
			behavior.gizmoViewMode = BehaviorDesignerPreferences.GetInt(BDPreferences.GizmosViewMode);
			behavior.showBehaviorDesignerGizmo = BehaviorDesignerPreferences.GetBool(BDPreferences.ShowSceneIcon);
		}

		public static void HierarchyChange()
		{
			BehaviorManager instance = BehaviorManager.instance;
			if (Application.get_isPlaying())
			{
				if (instance != null)
				{
					instance.onEnableBehavior = new BehaviorManager.BehaviorManagerHandler(GizmoManager.UpdateBehaviorManagerGizmos);
				}
			}
			else
			{
				string name = SceneManager.GetActiveScene().get_name();
				if (GizmoManager.currentScene != name)
				{
					GizmoManager.currentScene = name;
					GizmoManager.UpdateAllGizmos();
				}
			}
		}

		private static void UpdateBehaviorManagerGizmos()
		{
			BehaviorManager instance = BehaviorManager.instance;
			if (instance != null)
			{
				for (int i = 0; i < instance.get_BehaviorTrees().get_Count(); i++)
				{
					GizmoManager.UpdateGizmo(instance.get_BehaviorTrees().get_Item(i).behavior);
				}
			}
		}
	}
}
