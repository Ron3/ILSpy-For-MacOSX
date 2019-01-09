// Decompiled with JetBrains decompiler
// Type: BehaviorDesigner.Editor.WelcomeScreen
// Assembly: BehaviorDesignerEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 99CE4D00-DFA2-42D1-ABFC-D630AB4C1372
// Assembly location: C:\Users\Ron\Desktop\bt\BehaviorDesignerEditor.dll

using UnityEditor;
using UnityEngine;

namespace BehaviorDesigner.Editor
{
  public class WelcomeScreen : EditorWindow
  {
    private Texture m_WelcomeScreenImage;
    private Texture m_SamplesImage;
    private Texture m_DocImage;
    private Texture m_VideoImage;
    private Texture m_ForumImage;
    private Texture m_ContactImage;
    private Rect m_WelcomeScreenImageRect;
    private Rect m_WelcomeIntroRect;
    private Rect m_SamplesImageRect;
    private Rect m_DocImageRect;
    private Rect m_VideoImageRect;
    private Rect m_ForumImageRect;
    private Rect m_ContactImageRect;
    private Rect m_VersionRect;
    private Rect m_ToggleButtonRect;
    private Rect m_SamplesHeaderRect;
    private Rect m_DocHeaderRect;
    private Rect m_VideoHeaderRect;
    private Rect m_ForumHeaderRect;
    private Rect m_ContactHeaderRect;
    private Rect m_SamplesDescriptionRect;
    private Rect m_DocDescriptionRect;
    private Rect m_VideoDescriptionRect;
    private Rect m_ForumDescriptionRect;
    private Rect m_ContactDescriptionRect;

    public WelcomeScreen()
    {
      base.\u002Ector();
    }

    [MenuItem("Tools/Behavior Designer/Welcome Screen", false, 3)]
    public static void ShowWindow()
    {
      WelcomeScreen window = (WelcomeScreen) EditorWindow.GetWindow<WelcomeScreen>(true, "Welcome to Behavior Designer");
      WelcomeScreen welcomeScreen = window;
      Vector2 vector2_1 = new Vector2(340f, 410f);
      window.set_maxSize(vector2_1);
      Vector2 vector2_2 = vector2_1;
      welcomeScreen.set_minSize(vector2_2);
    }

    public void OnEnable()
    {
      this.m_WelcomeScreenImage = (Texture) BehaviorDesignerUtility.LoadTexture("WelcomeScreenHeader.png", false, (Object) this);
      this.m_SamplesImage = (Texture) BehaviorDesignerUtility.LoadIcon("WelcomeScreenSamplesIcon.png", (ScriptableObject) this);
      this.m_DocImage = (Texture) BehaviorDesignerUtility.LoadIcon("WelcomeScreenDocumentationIcon.png", (ScriptableObject) this);
      this.m_VideoImage = (Texture) BehaviorDesignerUtility.LoadIcon("WelcomeScreenVideosIcon.png", (ScriptableObject) this);
      this.m_ForumImage = (Texture) BehaviorDesignerUtility.LoadIcon("WelcomeScreenForumIcon.png", (ScriptableObject) this);
      this.m_ContactImage = (Texture) BehaviorDesignerUtility.LoadIcon("WelcomeScreenContactIcon.png", (ScriptableObject) this);
    }

    public void OnGUI()
    {
      GUI.DrawTexture(this.m_WelcomeScreenImageRect, this.m_WelcomeScreenImage);
      GUI.Label(this.m_WelcomeIntroRect, "Welcome To Behavior Designer", BehaviorDesignerUtility.WelcomeScreenIntroGUIStyle);
      GUI.DrawTexture(this.m_SamplesImageRect, this.m_SamplesImage);
      GUI.Label(this.m_SamplesHeaderRect, "Samples", BehaviorDesignerUtility.WelcomeScreenTextHeaderGUIStyle);
      GUI.Label(this.m_SamplesDescriptionRect, "Download sample projects to get a feel for Behavior Designer.", BehaviorDesignerUtility.WelcomeScreenTextDescriptionGUIStyle);
      GUI.DrawTexture(this.m_DocImageRect, this.m_DocImage);
      GUI.Label(this.m_DocHeaderRect, "Documentation", BehaviorDesignerUtility.WelcomeScreenTextHeaderGUIStyle);
      GUI.Label(this.m_DocDescriptionRect, "Browser our extensive online documentation.", BehaviorDesignerUtility.WelcomeScreenTextDescriptionGUIStyle);
      GUI.DrawTexture(this.m_VideoImageRect, this.m_VideoImage);
      GUI.Label(this.m_VideoHeaderRect, "Videos", BehaviorDesignerUtility.WelcomeScreenTextHeaderGUIStyle);
      GUI.Label(this.m_VideoDescriptionRect, "Watch our tutorial videos which cover a wide variety of topics.", BehaviorDesignerUtility.WelcomeScreenTextDescriptionGUIStyle);
      GUI.DrawTexture(this.m_ForumImageRect, this.m_ForumImage);
      GUI.Label(this.m_ForumHeaderRect, "Forums", BehaviorDesignerUtility.WelcomeScreenTextHeaderGUIStyle);
      GUI.Label(this.m_ForumDescriptionRect, "Join the forums!", BehaviorDesignerUtility.WelcomeScreenTextDescriptionGUIStyle);
      GUI.DrawTexture(this.m_ContactImageRect, this.m_ContactImage);
      GUI.Label(this.m_ContactHeaderRect, "Contact", BehaviorDesignerUtility.WelcomeScreenTextHeaderGUIStyle);
      GUI.Label(this.m_ContactDescriptionRect, "We are here to help.", BehaviorDesignerUtility.WelcomeScreenTextDescriptionGUIStyle);
      GUI.Label(this.m_VersionRect, "Version 1.5.11");
      bool flag = GUI.Toggle(this.m_ToggleButtonRect, BehaviorDesignerPreferences.GetBool(BDPreferences.ShowWelcomeScreen), "Show at Startup");
      if (flag != BehaviorDesignerPreferences.GetBool(BDPreferences.ShowWelcomeScreen))
        BehaviorDesignerPreferences.SetBool(BDPreferences.ShowWelcomeScreen, flag);
      EditorGUIUtility.AddCursorRect(this.m_SamplesImageRect, (MouseCursor) 4);
      EditorGUIUtility.AddCursorRect(this.m_SamplesHeaderRect, (MouseCursor) 4);
      EditorGUIUtility.AddCursorRect(this.m_SamplesDescriptionRect, (MouseCursor) 4);
      EditorGUIUtility.AddCursorRect(this.m_DocImageRect, (MouseCursor) 4);
      EditorGUIUtility.AddCursorRect(this.m_DocHeaderRect, (MouseCursor) 4);
      EditorGUIUtility.AddCursorRect(this.m_DocDescriptionRect, (MouseCursor) 4);
      EditorGUIUtility.AddCursorRect(this.m_VideoImageRect, (MouseCursor) 4);
      EditorGUIUtility.AddCursorRect(this.m_VideoHeaderRect, (MouseCursor) 4);
      EditorGUIUtility.AddCursorRect(this.m_VideoDescriptionRect, (MouseCursor) 4);
      EditorGUIUtility.AddCursorRect(this.m_ForumImageRect, (MouseCursor) 4);
      EditorGUIUtility.AddCursorRect(this.m_ForumHeaderRect, (MouseCursor) 4);
      EditorGUIUtility.AddCursorRect(this.m_ForumDescriptionRect, (MouseCursor) 4);
      EditorGUIUtility.AddCursorRect(this.m_ContactImageRect, (MouseCursor) 4);
      EditorGUIUtility.AddCursorRect(this.m_ContactHeaderRect, (MouseCursor) 4);
      EditorGUIUtility.AddCursorRect(this.m_ContactDescriptionRect, (MouseCursor) 4);
      if (Event.get_current().get_type() != 1)
        return;
      Vector2 mousePosition = Event.get_current().get_mousePosition();
      if (((Rect) ref this.m_SamplesImageRect).Contains(mousePosition) || ((Rect) ref this.m_SamplesHeaderRect).Contains(mousePosition) || ((Rect) ref this.m_SamplesDescriptionRect).Contains(mousePosition))
        Application.OpenURL("http://www.opsive.com/assets/BehaviorDesigner/samples.php");
      else if (((Rect) ref this.m_DocImageRect).Contains(mousePosition) || ((Rect) ref this.m_DocHeaderRect).Contains(mousePosition) || ((Rect) ref this.m_DocDescriptionRect).Contains(mousePosition))
        Application.OpenURL("http://www.opsive.com/assets/BehaviorDesigner/documentation.php");
      else if (((Rect) ref this.m_VideoImageRect).Contains(mousePosition) || ((Rect) ref this.m_VideoHeaderRect).Contains(mousePosition) || ((Rect) ref this.m_VideoDescriptionRect).Contains(mousePosition))
        Application.OpenURL("http://www.opsive.com/assets/BehaviorDesigner/videos.php");
      else if (((Rect) ref this.m_ForumImageRect).Contains(mousePosition) || ((Rect) ref this.m_ForumHeaderRect).Contains(mousePosition) || ((Rect) ref this.m_ForumDescriptionRect).Contains(mousePosition))
      {
        Application.OpenURL("http://www.opsive.com/forum");
      }
      else
      {
        if (!((Rect) ref this.m_ContactImageRect).Contains(mousePosition) && !((Rect) ref this.m_ContactHeaderRect).Contains(mousePosition) && !((Rect) ref this.m_ContactDescriptionRect).Contains(mousePosition))
          return;
        Application.OpenURL("http://www.opsive.com/assets/BehaviorDesigner/documentation.php?id=12");
      }
    }
  }
}
