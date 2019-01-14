// Decompiled with JetBrains decompiler
// Type: BehaviorDesigner.Runtime.Tasks.Task
// Assembly: BehaviorDesignerRuntime, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 4E52B3C7-423D-4A3D-958C-02E4666F0F16
// Assembly location: C:\Users\Ron\Desktop\Runtime\BehaviorDesignerRuntime.dll

using System;
using System.Collections;
using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks
{
  public abstract class Task
  {
    [SerializeField]
    private int id = -1;
    [SerializeField]
    private string friendlyName = string.Empty;
    [SerializeField]
    private bool instant = true;
    private int referenceID = -1;
    protected GameObject gameObject;
    protected Transform transform;
    [SerializeField]
    private NodeData nodeData;
    [SerializeField]
    private Behavior owner;
    private bool disabled;

    public virtual void OnAwake()
    {
    }

    public virtual void OnStart()
    {
    }

    public virtual TaskStatus OnUpdate()
    {
      return TaskStatus.Success;
    }

    public virtual void OnLateUpdate()
    {
    }

    public virtual void OnFixedUpdate()
    {
    }

    public virtual void OnEnd()
    {
    }

    public virtual void OnPause(bool paused)
    {
    }

    public virtual void OnConditionalAbort()
    {
    }

    public virtual float GetPriority()
    {
      return 0.0f;
    }

    public virtual float GetUtility()
    {
      return 0.0f;
    }

    public virtual void OnBehaviorRestart()
    {
    }

    public virtual void OnBehaviorComplete()
    {
    }

    public virtual void OnReset()
    {
    }

    public virtual void OnDrawGizmos()
    {
    }

    protected void StartCoroutine(string methodName)
    {
      this.Owner.StartTaskCoroutine(this, methodName);
    }

    protected Coroutine StartCoroutine(IEnumerator routine)
    {
      return this.Owner.StartCoroutine(routine);
    }

    protected Coroutine StartCoroutine(string methodName, object value)
    {
      return this.Owner.StartTaskCoroutine(this, methodName, value);
    }

    protected void StopCoroutine(string methodName)
    {
      this.Owner.StopTaskCoroutine(methodName);
    }

    protected void StopCoroutine(IEnumerator routine)
    {
      this.Owner.StopCoroutine(routine);
    }

    protected void StopAllCoroutines()
    {
      this.Owner.StopAllTaskCoroutines();
    }

    public virtual void OnCollisionEnter(Collision collision)
    {
    }

    public virtual void OnCollisionExit(Collision collision)
    {
    }

    public virtual void OnTriggerEnter(Collider other)
    {
    }

    public virtual void OnTriggerExit(Collider other)
    {
    }

    public virtual void OnCollisionEnter2D(Collision2D collision)
    {
    }

    public virtual void OnCollisionExit2D(Collision2D collision)
    {
    }

    public virtual void OnTriggerEnter2D(Collider2D other)
    {
    }

    public virtual void OnTriggerExit2D(Collider2D other)
    {
    }

    public virtual void OnControllerColliderHit(ControllerColliderHit hit)
    {
    }

    public virtual void OnAnimatorIK()
    {
    }

    public GameObject GameObject
    {
      set
      {
        this.gameObject = value;
      }
    }

    public Transform Transform
    {
      set
      {
        this.transform = value;
      }
    }

    protected T GetComponent<T>() where T : Component
    {
      return this.gameObject.GetComponent<T>();
    }

    protected Component GetComponent(Type type)
    {
      return this.gameObject.GetComponent(type);
    }

    protected GameObject GetDefaultGameObject(GameObject go)
    {
      if (Object.op_Equality((Object) go, (Object) null))
        return this.gameObject;
      return go;
    }

    public NodeData NodeData
    {
      get
      {
        return this.nodeData;
      }
      set
      {
        this.nodeData = value;
      }
    }

    public Behavior Owner
    {
      get
      {
        return this.owner;
      }
      set
      {
        this.owner = value;
      }
    }

    public int ID
    {
      get
      {
        return this.id;
      }
      set
      {
        this.id = value;
      }
    }

    public string FriendlyName
    {
      get
      {
        return this.friendlyName;
      }
      set
      {
        this.friendlyName = value;
      }
    }

    public bool IsInstant
    {
      get
      {
        return this.instant;
      }
      set
      {
        this.instant = value;
      }
    }

    public int ReferenceID
    {
      get
      {
        return this.referenceID;
      }
      set
      {
        this.referenceID = value;
      }
    }

    public bool Disabled
    {
      get
      {
        return this.disabled;
      }
      set
      {
        this.disabled = value;
      }
    }
  }
}
