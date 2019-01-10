using BehaviorDesigner.Runtime.Tasks;
using System;
using UnityEngine;

namespace BehaviorDesigner.Editor
{
	[Serializable]
	public class ErrorDetails
	{
		public enum ErrorType
		{
			RequiredField = 0,
			SharedVariable = 1,
			MissingChildren = 2,
			UnknownTask = 3,
			InvalidTaskReference = 4,
			InvalidVariableReference = 5
		}

		[SerializeField]
		private ErrorDetails.ErrorType mType;

		[SerializeField]
		private NodeDesigner mNodeDesigner;

		[SerializeField]
		private string mTaskFriendlyName;

		[SerializeField]
		private string mTaskType;

		[SerializeField]
		private string mFieldName;

		public ErrorDetails.ErrorType Type
		{
			get
			{
				return this.mType;
			}
		}

		public NodeDesigner NodeDesigner
		{
			get
			{
				return this.mNodeDesigner;
			}
		}

		public string TaskFriendlyName
		{
			get
			{
				return this.mTaskFriendlyName;
			}
		}

		public string TaskType
		{
			get
			{
				return this.mTaskType;
			}
		}

		public string FieldName
		{
			get
			{
				return this.mFieldName;
			}
		}

		public ErrorDetails(ErrorDetails.ErrorType type, Task task, string fieldName)
		{
			this.mType = type;
			if (task != null)
			{
				this.mNodeDesigner = (task.NodeData.NodeDesigner as NodeDesigner);
				this.mTaskFriendlyName = task.FriendlyName;
				this.mTaskType = task.GetType().ToString();
			}
			this.mFieldName = fieldName;
		}
	}
}
