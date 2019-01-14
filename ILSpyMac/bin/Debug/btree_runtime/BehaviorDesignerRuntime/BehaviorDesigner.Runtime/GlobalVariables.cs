using System;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviorDesigner.Runtime
{
	public class GlobalVariables : ScriptableObject, IVariableSource
	{
		private static GlobalVariables instance;

		[SerializeField]
		private List<SharedVariable> mVariables;

		private Dictionary<string, int> mSharedVariableIndex;

		[SerializeField]
		private VariableSerializationData mVariableData;

		[SerializeField]
		private string mVersion;

		public static GlobalVariables Instance
		{
			get
			{
				if (GlobalVariables.instance == null)
				{
					GlobalVariables.instance = (Resources.Load("BehaviorDesignerGlobalVariables", typeof(GlobalVariables)) as GlobalVariables);
					if (GlobalVariables.instance != null)
					{
						GlobalVariables.instance.CheckForSerialization(false);
					}
				}
				return GlobalVariables.instance;
			}
		}

		public List<SharedVariable> Variables
		{
			get
			{
				return this.mVariables;
			}
			set
			{
				this.mVariables = value;
				this.UpdateVariablesIndex();
			}
		}

		public VariableSerializationData VariableData
		{
			get
			{
				return this.mVariableData;
			}
			set
			{
				this.mVariableData = value;
			}
		}

		public string Version
		{
			get
			{
				return this.mVersion;
			}
			set
			{
				this.mVersion = value;
			}
		}

		public void CheckForSerialization(bool force)
		{
			if (force || this.mVariables == null || (this.mVariables.get_Count() > 0 && this.mVariables.get_Item(0) == null))
			{
				if (this.VariableData != null && !string.IsNullOrEmpty(this.VariableData.JSONSerialization))
				{
					JSONDeserialization.Load(this.VariableData.JSONSerialization, this, this.mVersion);
				}
				else
				{
					BinaryDeserialization.Load(this, this.mVersion);
				}
			}
		}

		public SharedVariable GetVariable(string name)
		{
			if (name == null)
			{
				return null;
			}
			this.CheckForSerialization(false);
			if (this.mVariables != null)
			{
				if (this.mSharedVariableIndex == null || this.mSharedVariableIndex.get_Count() != this.mVariables.get_Count())
				{
					this.UpdateVariablesIndex();
				}
				int num;
				if (this.mSharedVariableIndex.TryGetValue(name, ref num))
				{
					return this.mVariables.get_Item(num);
				}
			}
			return null;
		}

		public List<SharedVariable> GetAllVariables()
		{
			this.CheckForSerialization(false);
			return this.mVariables;
		}

		public void SetVariable(string name, SharedVariable sharedVariable)
		{
			this.CheckForSerialization(false);
			if (this.mVariables == null)
			{
				this.mVariables = new List<SharedVariable>();
			}
			else if (this.mSharedVariableIndex == null)
			{
				this.UpdateVariablesIndex();
			}
			sharedVariable.Name = name;
			int num;
			if (this.mSharedVariableIndex != null && this.mSharedVariableIndex.TryGetValue(name, ref num))
			{
				SharedVariable sharedVariable2 = this.mVariables.get_Item(num);
				if (!sharedVariable2.GetType().Equals(typeof(SharedVariable)) && !sharedVariable2.GetType().Equals(sharedVariable.GetType()))
				{
					Debug.LogError(string.Format("Error: Unable to set SharedVariable {0} - the variable type {1} does not match the existing type {2}", name, sharedVariable2.GetType(), sharedVariable.GetType()));
				}
				else
				{
					sharedVariable2.SetValue(sharedVariable.GetValue());
				}
			}
			else
			{
				this.mVariables.Add(sharedVariable);
				this.UpdateVariablesIndex();
			}
		}

		public void SetVariableValue(string name, object value)
		{
			SharedVariable variable = this.GetVariable(name);
			if (variable != null)
			{
				variable.SetValue(value);
				variable.ValueChanged();
			}
		}

		public void UpdateVariableName(SharedVariable sharedVariable, string name)
		{
			this.CheckForSerialization(false);
			sharedVariable.Name = name;
			this.UpdateVariablesIndex();
		}

		public void SetAllVariables(List<SharedVariable> variables)
		{
			this.mVariables = variables;
		}

		private void UpdateVariablesIndex()
		{
			if (this.mVariables == null)
			{
				if (this.mSharedVariableIndex != null)
				{
					this.mSharedVariableIndex = null;
				}
				return;
			}
			if (this.mSharedVariableIndex == null)
			{
				this.mSharedVariableIndex = new Dictionary<string, int>(this.mVariables.get_Count());
			}
			else
			{
				this.mSharedVariableIndex.Clear();
			}
			for (int i = 0; i < this.mVariables.get_Count(); i++)
			{
				if (this.mVariables.get_Item(i) != null)
				{
					this.mSharedVariableIndex.Add(this.mVariables.get_Item(i).Name, i);
				}
			}
		}
	}
}
