// Decompiled with JetBrains decompiler
// Type: BehaviorDesigner.Runtime.TaskCoroutine
// Assembly: BehaviorDesignerRuntime, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 4E52B3C7-423D-4A3D-958C-02E4666F0F16
// Assembly location: C:\Users\Ron\Desktop\Runtime\BehaviorDesignerRuntime.dll

using System.Collections;
using System.Diagnostics;
using UnityEngine;

namespace BehaviorDesigner.Runtime
{
  public class TaskCoroutine
  {
    private IEnumerator mCoroutineEnumerator;
    private Coroutine mCoroutine;
    private Behavior mParent;
    private string mCoroutineName;
    private bool mStop;

    public TaskCoroutine(Behavior parent, IEnumerator coroutine, string coroutineName)
    {
      this.mParent = parent;
      this.mCoroutineEnumerator = coroutine;
      this.mCoroutineName = coroutineName;
      this.mCoroutine = parent.StartCoroutine(this.RunCoroutine());
    }

    public Coroutine Coroutine
    {
      get
      {
        return this.mCoroutine;
      }
    }

    public void Stop()
    {
      this.mStop = true;
    }

    [DebuggerHidden]
    public IEnumerator RunCoroutine()
    {
      // ISSUE: object of a compiler-generated type is created
      return (IEnumerator) new TaskCoroutine.\u003CRunCoroutine\u003Ec__Iterator1()
      {
        \u003C\u003Ef__this = this
      };
    }
  }
}
