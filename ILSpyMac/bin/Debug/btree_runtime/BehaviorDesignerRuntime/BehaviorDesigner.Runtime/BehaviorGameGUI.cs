using BehaviorDesigner.Runtime.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviorDesigner.Runtime
{
	[AddComponentMenu("Behavior Designer/Behavior Game GUI")]
	public class BehaviorGameGUI : MonoBehaviour
	{
		private BehaviorManager behaviorManager;

		private Camera mainCamera;

		public void Start()
		{
			this.mainCamera = Camera.get_main();
		}

		public void OnGUI()
		{
			if (this.behaviorManager == null)
			{
				this.behaviorManager = BehaviorManager.instance;
			}
			if (this.behaviorManager == null || this.mainCamera == null)
			{
				return;
			}
			List<BehaviorManager.BehaviorTree> behaviorTrees = this.behaviorManager.BehaviorTrees;
			for (int i = 0; i < behaviorTrees.get_Count(); i++)
			{
				BehaviorManager.BehaviorTree behaviorTree = behaviorTrees.get_Item(i);
				string text = string.Empty;
				for (int j = 0; j < behaviorTree.activeStack.get_Count(); j++)
				{
					Stack<int> stack = behaviorTree.activeStack.get_Item(j);
					if (stack.get_Count() != 0)
					{
						Task task = behaviorTree.taskList.get_Item(stack.Peek());
						if (task is Action)
						{
							text = text + behaviorTree.taskList.get_Item(behaviorTree.activeStack.get_Item(j).Peek()).FriendlyName + ((j >= behaviorTree.activeStack.get_Count() - 1) ? string.Empty : "\n");
						}
					}
				}
				Transform transform = behaviorTree.behavior.get_transform();
				Vector3 vector = Camera.get_main().WorldToScreenPoint(transform.get_position());
				Vector2 vector2 = GUIUtility.ScreenToGUIPoint(vector);
				GUIContent gUIContent = new GUIContent(text);
				Vector2 vector3 = GUI.get_skin().get_label().CalcSize(gUIContent);
				vector3.x += 14f;
				vector3.y += 5f;
				GUI.Box(new Rect(vector2.x - vector3.x / 2f, (float)Screen.get_height() - vector2.y + vector3.y / 2f, vector3.x, vector3.y), gUIContent);
			}
		}
	}
}
