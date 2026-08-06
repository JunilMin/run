#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using MindfulRun.Analytics;
using MindfulRun.CameraSystem;
using MindfulRun.Core;
using MindfulRun.Obstacles;
using MindfulRun.Player;
using MindfulRun.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class PrototypeBuilder
{
    const string Root = "Assets";
    static Sprite square, circle, triangle;
    static readonly Color Ink = new(.08f,.08f,.09f), Paper = new(.94f,.94f,.92f), Danger = new(.8f,.05f,.05f);

    [MenuItem("Tools/Mindful Run/Rebuild Prototype")]
    public static void Build()
    {
        EnsureFolders(); CreateSprites(); CreateMaterial();
        BuildPrefabs(); BuildMenu(); BuildStage(1); BuildStage(2); BuildStage(3); BuildComplete();
        EditorBuildSettings.scenes = new[] { "MainMenu", "Stage_01", "Stage_02", "Stage_03", "PrototypeComplete" }
            .Select(n => new EditorBuildSettingsScene($"Assets/Scenes/{n}.unity", true)).ToArray();
        PlayerSettings.defaultScreenWidth = 1920; PlayerSettings.defaultScreenHeight = 1080;
        PlayerSettings.productName = "Mindful Run Prototype";
        AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
        Debug.Log("Mindful Run prototype generated successfully.");
    }

    public static void BuildFromCommandLine() { Build(); EditorApplication.Exit(0); }

    static void EnsureFolders()
    {
        string[] folders = { "Art","Art/Characters","Art/Environment","Art/Obstacles","Art/UI","Audio","Materials","Prefabs","Prefabs/Player","Prefabs/Obstacles","Prefabs/Environment","Prefabs/UI","Scenes","Scripts","Settings","Editor" };
        foreach (string folder in folders) { string path=Root; foreach(string part in folder.Split('/')) { string next=$"{path}/{part}"; if(!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(path,part); path=next; } }
    }

    static void CreateSprites()
    {
        square = MakeSprite("Square", 32, (x,y)=>true);
        circle = MakeSprite("Circle", 32, (x,y)=>Vector2.Distance(new Vector2(x,y),new Vector2(15.5f,15.5f))<15.5f);
        triangle = MakeSprite("Triangle", 32, (x,y)=>y <= x*1.85f && y <= (31-x)*1.85f);
    }
    static Sprite MakeSprite(string name,int size,Func<int,int,bool> inside)
    {
        string path=$"Assets/Art/{name}.png"; var tex=new Texture2D(size,size,TextureFormat.RGBA32,false); var px=new Color[size*size];
        for(int y=0;y<size;y++)for(int x=0;x<size;x++)px[y*size+x]=inside(x,y)?Color.white:Color.clear; tex.SetPixels(px); tex.Apply();
        System.IO.File.WriteAllBytes(System.IO.Path.GetFullPath(path),tex.EncodeToPNG()); UnityEngine.Object.DestroyImmediate(tex); AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceUpdate);
        var importer=(TextureImporter)AssetImporter.GetAtPath(path); importer.textureType=TextureImporterType.Sprite; importer.spritePixelsPerUnit=32; importer.filterMode=FilterMode.Point; importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
    static void CreateMaterial(){ var mat=new PhysicsMaterial2D("NoFriction"){friction=0,bounciness=0}; AssetDatabase.CreateAsset(mat,"Assets/Materials/NoFriction.physicsMaterial2D"); }

    static GameObject Visual(string name,Transform parent,Sprite sprite,Vector2 pos,Vector2 scale,Color color,int order=0)
    { var go=new GameObject(name,typeof(SpriteRenderer));go.transform.SetParent(parent,false);go.transform.localPosition=pos;go.transform.localScale=scale;var sr=go.GetComponent<SpriteRenderer>();sr.sprite=sprite;sr.color=color;sr.sortingOrder=order;return go; }

    static void BuildPrefabs()
    {
        var player=new GameObject("Player",typeof(Rigidbody2D),typeof(CapsuleCollider2D),typeof(PlayerController2D)); player.layer=6;
        var rb=player.GetComponent<Rigidbody2D>();rb.gravityScale=1;rb.collisionDetectionMode=CollisionDetectionMode2D.Continuous;rb.interpolation=RigidbodyInterpolation2D.Interpolate;rb.freezeRotation=true;
        var col=player.GetComponent<CapsuleCollider2D>();col.size=new Vector2(.62f,1.65f);col.sharedMaterial=AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>("Assets/Materials/NoFriction.physicsMaterial2D");
        var controllerSettings=new SerializedObject(player.GetComponent<PlayerController2D>());controllerSettings.FindProperty("groundMask").intValue=1;controllerSettings.ApplyModifiedPropertiesWithoutUndo();
        var rig=new GameObject("OriginalStickFigure",typeof(StickFigureAnimator));rig.transform.SetParent(player.transform,false);
        var head=Visual("Head",rig.transform,circle,new Vector2(0,.58f),new Vector2(.42f,.42f),Ink,2);
        var torso=Visual("Torso",rig.transform,square,new Vector2(0,.05f),new Vector2(.12f,.72f),Ink,1);
        var la=Limb("LeftArm",rig.transform,new Vector2(-.08f,.25f),-.42f); var ra=Limb("RightArm",rig.transform,new Vector2(.08f,.25f),.42f);
        var ll=Limb("LeftLeg",rig.transform,new Vector2(-.06f,-.28f),-.48f); var rl=Limb("RightLeg",rig.transform,new Vector2(.06f,-.28f),.48f);
        var anim=rig.GetComponent<StickFigureAnimator>();var so=new SerializedObject(anim);so.FindProperty("leftArm").objectReferenceValue=la.transform;so.FindProperty("rightArm").objectReferenceValue=ra.transform;so.FindProperty("leftLeg").objectReferenceValue=ll.transform;so.FindProperty("rightLeg").objectReferenceValue=rl.transform;so.FindProperty("torso").objectReferenceValue=torso.transform;
        var arr=so.FindProperty("renderers");arr.arraySize=6;var rs=rig.GetComponentsInChildren<SpriteRenderer>();for(int i=0;i<6;i++)arr.GetArrayElementAtIndex(i).objectReferenceValue=rs[i];so.ApplyModifiedPropertiesWithoutUndo();
        SavePrefab(player,"Assets/Prefabs/Player/Player.prefab");

        var spike=Visual("StaticSpike",null,triangle,Vector2.zero,new Vector2(.7f,.65f),Danger); spike.AddComponent<BoxCollider2D>().isTrigger=true;spike.AddComponent<Hazard>();SavePrefab(spike,"Assets/Prefabs/Obstacles/StaticSpike.prefab");
        var timed=Visual("TimedSpike",null,triangle,Vector2.zero,new Vector2(.8f,.7f),Color.gray);timed.AddComponent<BoxCollider2D>().isTrigger=true;timed.AddComponent<Hazard>();timed.AddComponent<TimedSpike>();SavePrefab(timed,"Assets/Prefabs/Obstacles/TimedSpike.prefab");
        var falling=new GameObject("FallingBlockTrigger",typeof(BoxCollider2D),typeof(FallingBlock));falling.GetComponent<BoxCollider2D>().isTrigger=true;falling.GetComponent<BoxCollider2D>().size=new Vector2(4,4);
        var warning=Visual("WarningShadow",falling.transform,square,new Vector2(0,-1.9f),new Vector2(1.1f,.12f),new Color(1,.4f,0,.65f));warning.GetComponent<SpriteRenderer>().enabled=false;
        var block=Visual("Block",falling.transform,square,new Vector2(0,2.2f),new Vector2(1.15f,1.15f),Danger);var br=block.AddComponent<Rigidbody2D>();br.bodyType=RigidbodyType2D.Kinematic;br.gravityScale=3;block.AddComponent<BoxCollider2D>();block.AddComponent<Hazard>();
        var fs=new SerializedObject(falling.GetComponent<FallingBlock>());fs.FindProperty("blockBody").objectReferenceValue=br;fs.FindProperty("warning").objectReferenceValue=warning.GetComponent<SpriteRenderer>();fs.ApplyModifiedPropertiesWithoutUndo();SavePrefab(falling,"Assets/Prefabs/Obstacles/FallingBlock.prefab");
        var exit=Visual("ExitZone",null,square,Vector2.zero,new Vector2(.15f,2.3f),new Color(.1f,.55f,.35f));exit.AddComponent<BoxCollider2D>().isTrigger=true;exit.AddComponent<ExitZone>();SavePrefab(exit,"Assets/Prefabs/Obstacles/ExitZone.prefab");
        var launcher=Visual("ProjectileLauncher_PLACEHOLDER",null,square,Vector2.zero,Vector2.one,Ink);launcher.AddComponent<ProjectileLauncherPlaceholder>();SavePrefab(launcher,"Assets/Prefabs/Obstacles/ProjectileLauncher.prefab");
        var moving=Visual("MovingPlatform_PLACEHOLDER",null,square,Vector2.zero,new Vector2(3,.35f),Ink);moving.AddComponent<BoxCollider2D>();moving.AddComponent<MovingPlatformPlaceholder>();SavePrefab(moving,"Assets/Prefabs/Obstacles/MovingPlatform.prefab");
    }
    static GameObject Limb(string name,Transform parent,Vector2 pos,float x){var pivot=new GameObject(name);pivot.transform.SetParent(parent,false);pivot.transform.localPosition=pos;Visual("Shape",pivot.transform,square,new Vector2(x/2,-.22f),new Vector2(.09f,.58f),Ink,0);return pivot;}
    static void SavePrefab(GameObject go,string path){PrefabUtility.SaveAsPrefabAsset(go,path);UnityEngine.Object.DestroyImmediate(go);}

    static void BaseScene(out GameObject player,out StageManager manager,int stage,string next)
    {
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);scene.name=$"Stage_{stage:00}";
        var cam=new GameObject("Main Camera",typeof(Camera),typeof(SmoothCameraFollow));cam.tag="MainCamera";cam.GetComponent<Camera>().orthographic=true;cam.GetComponent<Camera>().orthographicSize=5.4f;cam.GetComponent<Camera>().backgroundColor=Paper;cam.transform.position=new Vector3(4,2.5f,-10);
        player=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player/Player.prefab"));player.transform.position=new Vector3(-3,-.05f,0);
        var sm=new GameObject("StageManager",typeof(StageManager),typeof(PlayerBehaviorTracker));manager=sm.GetComponent<StageManager>();var so=new SerializedObject(manager);so.FindProperty("stageNumber").intValue=stage;so.FindProperty("nextSceneName").stringValue=next;so.FindProperty("player").objectReferenceValue=player.GetComponent<PlayerController2D>();so.FindProperty("tracker").objectReferenceValue=sm.GetComponent<PlayerBehaviorTracker>();so.ApplyModifiedPropertiesWithoutUndo();
        var tr=new SerializedObject(sm.GetComponent<PlayerBehaviorTracker>());tr.FindProperty("player").objectReferenceValue=player.GetComponent<PlayerController2D>();tr.ApplyModifiedPropertiesWithoutUndo();
        AddStageUI(stage,sm.transform); Platform("Start",new Vector2(-2,-1),new Vector2(7,.5f));
    }
    static void BuildStage(int stage)
    {
        string next=stage<3?$"Stage_{stage+1:00}":"PrototypeComplete";BaseScene(out var player,out var manager,stage,next);
        if(stage==1){Platform("FarFloor",new Vector2(7,-1),new Vector2(9,.5f));Platform("Raised",new Vector2(4.2f,.1f),new Vector2(3,.45f));Exit(new Vector2(11,-.1f));}
        if(stage==2){Platform("Floor",new Vector2(8,-1),new Vector2(20,.5f));for(int i=0;i<4;i++)Prefab("Assets/Prefabs/Obstacles/TimedSpike.prefab",new Vector2(2.5f+i*.85f,-.42f));Exit(new Vector2(15,-.1f));}
        if(stage==3){Platform("Floor",new Vector2(9,-1),new Vector2(22,.5f));Prefab("Assets/Prefabs/Obstacles/FallingBlock.prefab",new Vector2(4.5f,1.2f));Visual("WaitHint",null,square,new Vector2(1.2f,-.6f),new Vector2(.5f,.08f),new Color(1,.5f,0));Exit(new Vector2(17,-.1f));}
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(),$"Assets/Scenes/Stage_{stage:00}.unity");
    }
    static void Platform(string name,Vector2 pos,Vector2 scale){var go=Visual(name,null,square,pos,scale,Ink);go.AddComponent<BoxCollider2D>();}
    static GameObject Prefab(string path,Vector2 pos){var go=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(path));go.transform.position=pos;return go;}
    static void Exit(Vector2 pos)=>Prefab("Assets/Prefabs/Obstacles/ExitZone.prefab",pos);

    static void AddStageUI(int stage,Transform parent)
    {
        var canvas=CanvasRoot("HUD");Label(canvas.transform,$"STAGE {stage:00}",new Vector2(24,-24),TextAnchor.UpperLeft,28,new Vector2(300,70));
        var pause=new GameObject("PauseController",typeof(PauseMenu));pause.transform.SetParent(parent);var panel=new GameObject("PausePanel",typeof(RectTransform),typeof(Image));panel.transform.SetParent(canvas.transform,false);Stretch(panel.GetComponent<RectTransform>());panel.GetComponent<Image>().color=new Color(0,0,0,.82f);
        Label(panel.transform,"PAUSED",new Vector2(0,160),TextAnchor.MiddleCenter,50,new Vector2(500,90),true);Button(panel.transform,"RESUME",new Vector2(0,40),pause.GetComponent<PauseMenu>().Resume);Button(panel.transform,"RESTART",new Vector2(0,-45),pause.GetComponent<PauseMenu>().Restart);Button(panel.transform,"TITLE",new Vector2(0,-130),pause.GetComponent<PauseMenu>().QuitToTitle);
        var ps=new SerializedObject(pause.GetComponent<PauseMenu>());ps.FindProperty("panel").objectReferenceValue=panel;ps.ApplyModifiedPropertiesWithoutUndo();panel.SetActive(false);
    }
    static void BuildMenu(){EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);SimpleCamera();var c=CanvasRoot("TitleCanvas");var actions=new GameObject("MenuActions",typeof(MenuActions)).GetComponent<MenuActions>();Label(c.transform,"MINDFUL RUN",new Vector2(0,150),TextAnchor.MiddleCenter,72,new Vector2(900,130),true);Label(c.transform,"A minimalist obstacle study",new Vector2(0,60),TextAnchor.MiddleCenter,26,new Vector2(800,60));Button(c.transform,"BEGIN",new Vector2(0,-60),actions.Play);Button(c.transform,"QUIT",new Vector2(0,-150),actions.Quit);EditorSceneManager.SaveScene(SceneManager.GetActiveScene(),"Assets/Scenes/MainMenu.unity");}
    static void BuildComplete(){EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);SimpleCamera();var c=CanvasRoot("CompleteCanvas");var actions=new GameObject("MenuActions",typeof(MenuActions)).GetComponent<MenuActions>();Label(c.transform,"PROTOTYPE COMPLETE",new Vector2(0,100),TextAnchor.MiddleCenter,60,new Vector2(1100,110),true);Label(c.transform,"Three stages cleared. Behavior data was logged to the Console.",new Vector2(0,10),TextAnchor.MiddleCenter,24,new Vector2(1100,70));Button(c.transform,"TITLE",new Vector2(0,-110),actions.Title);EditorSceneManager.SaveScene(SceneManager.GetActiveScene(),"Assets/Scenes/PrototypeComplete.unity");}
    static void SimpleCamera(){var cam=new GameObject("Main Camera",typeof(Camera));cam.tag="MainCamera";cam.GetComponent<Camera>().backgroundColor=Paper;cam.GetComponent<Camera>().orthographic=true;cam.transform.position=new Vector3(0,0,-10);}
    static Canvas CanvasRoot(string name){var go=new GameObject(name,typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));var c=go.GetComponent<Canvas>();c.renderMode=RenderMode.ScreenSpaceOverlay;var s=go.GetComponent<CanvasScaler>();s.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;s.referenceResolution=new Vector2(1920,1080);new GameObject("EventSystem",typeof(UnityEngine.EventSystems.EventSystem),typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));return c;}
    static Text Label(Transform parent,string value,Vector2 pos,TextAnchor anchor,int size,Vector2 dims,bool bold=false){var go=new GameObject(value,typeof(RectTransform),typeof(Text));go.transform.SetParent(parent,false);var rt=go.GetComponent<RectTransform>();rt.sizeDelta=dims;rt.anchoredPosition=pos;if(anchor==TextAnchor.UpperLeft){rt.anchorMin=rt.anchorMax=new Vector2(0,1);rt.pivot=new Vector2(0,1);}var t=go.GetComponent<Text>();t.text=value;t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");t.fontSize=size;t.color=anchor==TextAnchor.UpperLeft?Ink:Color.white;t.alignment=anchor;t.fontStyle=bold?FontStyle.Bold:FontStyle.Normal;return t;}
    static Button Button(Transform parent,string value,Vector2 pos,UnityEngine.Events.UnityAction action){var go=new GameObject(value,typeof(RectTransform),typeof(Image),typeof(Button));go.transform.SetParent(parent,false);var rt=go.GetComponent<RectTransform>();rt.sizeDelta=new Vector2(320,64);rt.anchoredPosition=pos;go.GetComponent<Image>().color=new Color(.15f,.15f,.16f);var b=go.GetComponent<Button>();UnityEventTools.AddPersistentListener(b.onClick,action);Label(go.transform,value,Vector2.zero,TextAnchor.MiddleCenter,26,new Vector2(320,64),true);return b;}
    static void Stretch(RectTransform rt){rt.anchorMin=Vector2.zero;rt.anchorMax=Vector2.one;rt.offsetMin=rt.offsetMax=Vector2.zero;}
}
#endif
