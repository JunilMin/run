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
    static readonly Color Ink = new(.055f,.075f,.12f), Paper = new(.055f,.07f,.12f), Danger = new(.96f,.16f,.24f);
    static readonly Color Terrain = new(.16f,.23f,.34f), TerrainLight = new(.25f,.88f,.86f), TerrainInset = new(.11f,.16f,.26f), Character = new(.92f,.96f,1f);

    [MenuItem("Tools/Mindful Run/Rebuild Prototype")]
    public static void Build()
    {
        EnsureFolders(); CreateSprites(); CreateMaterial();
        BuildPrefabs(); BuildMenu(); for(int stage=1;stage<=20;stage++) BuildStage(stage); BuildComplete();
        EditorBuildSettings.scenes = new[] { "MainMenu" }.Concat(Enumerable.Range(1,20).Select(i=>$"Stage_{i:00}")).Append("PrototypeComplete")
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
        var col=player.GetComponent<CapsuleCollider2D>();col.size=new Vector2(.115f,.43f);col.sharedMaterial=AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>("Assets/Materials/NoFriction.physicsMaterial2D");
        var controllerSettings=new SerializedObject(player.GetComponent<PlayerController2D>());controllerSettings.FindProperty("groundMask").intValue=1;controllerSettings.FindProperty("groundCheckSize").vector2Value=new Vector2(.12f,.035f);controllerSettings.FindProperty("groundCheckDistance").floatValue=.025f;controllerSettings.FindProperty("jumpSpeed").floatValue=13.5f;controllerSettings.FindProperty("riseGravityMultiplier").floatValue=4f;controllerSettings.FindProperty("fallGravityMultiplier").floatValue=8.8f;controllerSettings.FindProperty("maxFallSpeed").floatValue=40f;controllerSettings.ApplyModifiedPropertiesWithoutUndo();
        var rig=new GameObject("OriginalStickFigure",typeof(StickFigureAnimator));rig.transform.SetParent(player.transform,false);
        rig.transform.localScale=Vector3.one*.25f;
        var head=Visual("Head",rig.transform,circle,new Vector2(0,.64f),new Vector2(.31f,.31f),Character,2);
        var torso=Visual("Torso",rig.transform,square,new Vector2(0,.08f),new Vector2(.075f,.83f),Character,1);
        var pelvis=Visual("Pelvis",rig.transform,circle,new Vector2(0,-.34f),new Vector2(.16f,.13f),Character,2);
        var la=Limb("LeftArm",rig.transform,new Vector2(-.04f,.33f),-.46f); var ra=Limb("RightArm",rig.transform,new Vector2(.04f,.33f),.46f);
        var ll=Limb("LeftLeg",rig.transform,new Vector2(-.035f,-.34f),-.54f); var rl=Limb("RightLeg",rig.transform,new Vector2(.035f,-.34f),.54f);
        var anim=rig.GetComponent<StickFigureAnimator>();var so=new SerializedObject(anim);so.FindProperty("leftArm").objectReferenceValue=la.transform;so.FindProperty("rightArm").objectReferenceValue=ra.transform;so.FindProperty("leftLeg").objectReferenceValue=ll.transform;so.FindProperty("rightLeg").objectReferenceValue=rl.transform;so.FindProperty("torso").objectReferenceValue=torso.transform;
        var arr=so.FindProperty("renderers");var rs=rig.GetComponentsInChildren<SpriteRenderer>();arr.arraySize=rs.Length;for(int i=0;i<rs.Length;i++)arr.GetArrayElementAtIndex(i).objectReferenceValue=rs[i];so.ApplyModifiedPropertiesWithoutUndo();
        SavePrefab(player,"Assets/Prefabs/Player/Player.prefab");

        var spike=Visual("StaticSpike",null,triangle,Vector2.zero,new Vector2(.34f,.32f),Danger); spike.AddComponent<BoxCollider2D>().isTrigger=true;spike.AddComponent<Hazard>();SavePrefab(spike,"Assets/Prefabs/Obstacles/StaticSpike.prefab");
        var timed=Visual("TimedSpike",null,triangle,Vector2.zero,new Vector2(.38f,.34f),new Color(.42f,.48f,.58f));timed.AddComponent<BoxCollider2D>().isTrigger=true;timed.AddComponent<Hazard>();timed.AddComponent<TimedSpike>();SavePrefab(timed,"Assets/Prefabs/Obstacles/TimedSpike.prefab");
        var falling=new GameObject("FallingBlockTrigger",typeof(BoxCollider2D),typeof(FallingBlock));falling.GetComponent<BoxCollider2D>().isTrigger=true;falling.GetComponent<BoxCollider2D>().size=new Vector2(4,4);
        var warning=Visual("WarningShadow",falling.transform,square,new Vector2(0,-1.9f),new Vector2(1.1f,.12f),new Color(1,.4f,0,.65f));warning.GetComponent<SpriteRenderer>().enabled=false;
        var block=Visual("Block",falling.transform,square,new Vector2(0,2.2f),new Vector2(1.15f,1.15f),Danger);var br=block.AddComponent<Rigidbody2D>();br.bodyType=RigidbodyType2D.Kinematic;br.gravityScale=3;block.AddComponent<BoxCollider2D>();block.AddComponent<Hazard>();
        var fs=new SerializedObject(falling.GetComponent<FallingBlock>());fs.FindProperty("blockBody").objectReferenceValue=br;fs.FindProperty("warning").objectReferenceValue=warning.GetComponent<SpriteRenderer>();fs.ApplyModifiedPropertiesWithoutUndo();SavePrefab(falling,"Assets/Prefabs/Obstacles/FallingBlock.prefab");
        var exit=Visual("ExitFlag",null,square,Vector2.zero,new Vector2(.09f,2.4f),Ink);exit.AddComponent<BoxCollider2D>().isTrigger=true;exit.GetComponent<BoxCollider2D>().size=new Vector2(5,1);exit.AddComponent<ExitZone>();
        Visual("Flag",exit.transform,triangle,new Vector2(.55f,.82f),new Vector2(1.05f,.72f),new Color(.1f,.55f,.35f),1).transform.localRotation=Quaternion.Euler(0,0,-90);SavePrefab(exit,"Assets/Prefabs/Obstacles/ExitZone.prefab");
        var collapsing=Visual("CollapsingPlatform",null,square,Vector2.zero,new Vector2(2.6f,.32f),Terrain);collapsing.AddComponent<BoxCollider2D>();collapsing.AddComponent<Rigidbody2D>();Visual("TopGlow",collapsing.transform,square,new Vector2(0,.43f),new Vector2(.98f,.08f),new Color(1,.62f,.18f),2);Visual("InsetPanel",collapsing.transform,square,new Vector2(0,-.08f),new Vector2(.88f,.5f),TerrainInset,1);collapsing.AddComponent<CollapsingPlatform>();SavePrefab(collapsing,"Assets/Prefabs/Obstacles/CollapsingPlatform.prefab");
        var rising=new GameObject("RisingSpikeTrap",typeof(BoxCollider2D),typeof(RisingSpikeTrap));rising.GetComponent<BoxCollider2D>().isTrigger=true;rising.GetComponent<BoxCollider2D>().size=new Vector2(3.2f,3.2f);
        var spikeRoot=new GameObject("Spikes");spikeRoot.transform.SetParent(rising.transform,false);spikeRoot.transform.localPosition=new Vector3(0,-1.25f,0);for(int i=-1;i<=1;i++){var s=Visual("Spike",spikeRoot.transform,triangle,new Vector2(i*.55f,0),new Vector2(.62f,.7f),Danger,2);s.AddComponent<BoxCollider2D>().isTrigger=true;s.AddComponent<Hazard>();}
        var riseWarning=Visual("Warning",rising.transform,square,new Vector2(0,-.55f),new Vector2(1.7f,.07f),new Color(1,.45f,0,.8f));riseWarning.GetComponent<SpriteRenderer>().enabled=false;var riseSo=new SerializedObject(rising.GetComponent<RisingSpikeTrap>());riseSo.FindProperty("spikes").objectReferenceValue=spikeRoot.transform;riseSo.FindProperty("warning").objectReferenceValue=riseWarning.GetComponent<SpriteRenderer>();riseSo.ApplyModifiedPropertiesWithoutUndo();SavePrefab(rising,"Assets/Prefabs/Obstacles/RisingSpikeTrap.prefab");
        var crusher=new GameObject("CrushingBlockTrap",typeof(BoxCollider2D),typeof(CrushingBlockTrap));crusher.GetComponent<BoxCollider2D>().isTrigger=true;crusher.GetComponent<BoxCollider2D>().size=new Vector2(3.4f,4);
        var shadow=Visual("WarningShadow",crusher.transform,square,new Vector2(0,-1.8f),new Vector2(1.2f,.1f),new Color(1,.45f,0,.7f));shadow.GetComponent<SpriteRenderer>().enabled=false;var crushBlock=Visual("CrushingBlock",crusher.transform,square,new Vector2(0,2.2f),new Vector2(1.25f,1.25f),Danger);var crushBody=crushBlock.AddComponent<Rigidbody2D>();crushBody.bodyType=RigidbodyType2D.Kinematic;crushBlock.AddComponent<BoxCollider2D>();crushBlock.AddComponent<Hazard>();var crushSo=new SerializedObject(crusher.GetComponent<CrushingBlockTrap>());crushSo.FindProperty("block").objectReferenceValue=crushBody;crushSo.FindProperty("warningShadow").objectReferenceValue=shadow.GetComponent<SpriteRenderer>();crushSo.ApplyModifiedPropertiesWithoutUndo();SavePrefab(crusher,"Assets/Prefabs/Obstacles/CrushingBlockTrap.prefab");
        var launcher=Visual("ProjectileLauncher_PLACEHOLDER",null,square,Vector2.zero,Vector2.one,Ink);launcher.AddComponent<ProjectileLauncherPlaceholder>();SavePrefab(launcher,"Assets/Prefabs/Obstacles/ProjectileLauncher.prefab");
        var moving=Visual("MovingPlatform_PLACEHOLDER",null,square,Vector2.zero,new Vector2(3,.35f),Ink);moving.AddComponent<BoxCollider2D>();moving.AddComponent<MovingPlatformPlaceholder>();SavePrefab(moving,"Assets/Prefabs/Obstacles/MovingPlatform.prefab");
    }
    static GameObject Limb(string name,Transform parent,Vector2 pos,float x){var pivot=new GameObject(name);pivot.transform.SetParent(parent,false);pivot.transform.localPosition=pos;float thickness=name.Contains("Leg")?.082f:.05f;Visual("Shape",pivot.transform,square,new Vector2(x/2,-.24f),new Vector2(thickness,.64f),Character,0);if(name.Contains("Leg"))Visual("Foot",pivot.transform,square,new Vector2(x*.62f,-.54f),new Vector2(.13f,.075f),Character,1);return pivot;}
    static void SavePrefab(GameObject go,string path){PrefabUtility.SaveAsPrefabAsset(go,path);UnityEngine.Object.DestroyImmediate(go);}

    static void BaseScene(out GameObject player,out StageManager manager,int stage,string next)
    {
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);scene.name=$"Stage_{stage:00}";
        var cam=new GameObject("Main Camera",typeof(Camera),typeof(SmoothCameraFollow));cam.tag="MainCamera";cam.GetComponent<Camera>().orthographic=true;cam.GetComponent<Camera>().orthographicSize=7.4f;cam.GetComponent<Camera>().backgroundColor=ChapterBackground(stage);cam.transform.position=new Vector3(4,2.5f,-10);var camSo=new SerializedObject(cam.GetComponent<SmoothCameraFollow>());camSo.FindProperty("xBounds").vector2Value=new Vector2(0,82);camSo.ApplyModifiedPropertiesWithoutUndo();
        player=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player/Player.prefab"));player.transform.position=new Vector3(-3,-.5f,0);
        var sm=new GameObject("StageManager",typeof(StageManager),typeof(PlayerBehaviorTracker));manager=sm.GetComponent<StageManager>();var so=new SerializedObject(manager);so.FindProperty("stageNumber").intValue=stage;so.FindProperty("nextSceneName").stringValue=next;so.FindProperty("player").objectReferenceValue=player.GetComponent<PlayerController2D>();so.FindProperty("tracker").objectReferenceValue=sm.GetComponent<PlayerBehaviorTracker>();so.ApplyModifiedPropertiesWithoutUndo();
        var tr=new SerializedObject(sm.GetComponent<PlayerBehaviorTracker>());tr.FindProperty("player").objectReferenceValue=player.GetComponent<PlayerController2D>();tr.ApplyModifiedPropertiesWithoutUndo();
        AddStageUI(stage,sm.transform); Platform("Start",new Vector2(-2,-1),new Vector2(6,.5f));
    }
    static void BuildStage(int stage)
    {
        string next=stage<20?$"Stage_{stage+1:00}":"PrototypeComplete";BaseScene(out var player,out var manager,stage,next);
        float exitX=BuildComplexLayout(stage); Exit(new Vector2(exitX,1.15f));
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(),$"Assets/Scenes/Stage_{stage:00}.unity");
    }
    static float BuildComplexLayout(int stage)
    {
        var rng=new System.Random(1701+stage*97);float x=2.2f;float y=-.7f;int segments=9+stage/5;
        for(int i=0;i<segments;i++)
        {
            float width=3.0f+(float)rng.NextDouble()*1.15f;float nextY=Mathf.Clamp(y+(rng.Next(-1,2)*.7f),-.7f,2.3f);x+=width*.5f+.32f;
            bool collapse=stage>=4&&(i+stage)%4==0;
            if(collapse){var p=Prefab("Assets/Prefabs/Obstacles/CollapsingPlatform.prefab",new Vector2(x,nextY));p.transform.localScale=new Vector3(width/2.6f,1,1);}
            else Platform($"Platform_{i:00}",new Vector2(x,nextY),new Vector2(width,.42f));
            int trap=(stage+i*3)%7;
            if(stage>=2&&trap==0)for(int s=0;s<2;s++)Prefab("Assets/Prefabs/Obstacles/TimedSpike.prefab",new Vector2(x-.35f+s*.7f,nextY+.5f));
            if(stage>=3&&trap==1)Prefab("Assets/Prefabs/Obstacles/FallingBlock.prefab",new Vector2(x,nextY+2.1f));
            if(stage>=5&&trap==2)Prefab("Assets/Prefabs/Obstacles/RisingSpikeTrap.prefab",new Vector2(x,nextY+1.35f));
            if(stage>=6&&trap==3)Prefab("Assets/Prefabs/Obstacles/CrushingBlockTrap.prefab",new Vector2(x,nextY+2.1f));
            if(stage>=8&&trap==4){var mover=Prefab("Assets/Prefabs/Obstacles/MovingPlatform.prefab",new Vector2(x,nextY+1.2f));mover.transform.localScale=new Vector3(.65f,.8f,1);}
            if(stage>=10&&trap==5)for(int s=-1;s<=1;s++)Prefab("Assets/Prefabs/Obstacles/StaticSpike.prefab",new Vector2(x+s*.68f,nextY+.48f));
            if(stage>=7&&i%3==1)Platform("CeilingPressure",new Vector2(x,nextY+3.15f),new Vector2(width*.8f,.35f));
            if(stage>=11&&i%4==2)Prefab("Assets/Prefabs/Obstacles/CrushingBlockTrap.prefab",new Vector2(x+width*.28f,nextY+2.1f));
            x+=width*.5f+.34f+(float)rng.NextDouble()*.18f;y=nextY;
        }
        float finalX=x+3.3f;Platform("FinalApproach",new Vector2(finalX,0),new Vector2(5.8f,.45f));return finalX+1.6f;
    }
    static void Platform(string name,Vector2 pos,Vector2 scale)
    {
        var go=Visual(name,null,square,pos,scale,Terrain);go.AddComponent<BoxCollider2D>();
        Visual("TopGlow",go.transform,square,new Vector2(0,.43f),new Vector2(.98f,.075f),TerrainLight,2);
        Visual("InsetPanel",go.transform,square,new Vector2(0,-.08f),new Vector2(.88f,.54f),TerrainInset,1);
        int divisions=Mathf.Clamp(Mathf.RoundToInt(scale.x/1.3f),1,8);
        for(int i=1;i<divisions;i++){float localX=-.5f+i/(float)divisions;Visual("Joint",go.transform,square,new Vector2(localX,0),new Vector2(.012f,.72f),new Color(.22f,.36f,.48f),2);}
    }
    static Color ChapterBackground(int stage)
    {
        if(stage<=5)return new Color(.055f,.07f,.12f);
        if(stage<=10)return new Color(.075f,.055f,.13f);
        if(stage<=15)return new Color(.035f,.105f,.13f);
        return new Color(.11f,.045f,.075f);
    }
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
    static void BuildComplete(){EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);SimpleCamera();var c=CanvasRoot("CompleteCanvas");var actions=new GameObject("MenuActions",typeof(MenuActions)).GetComponent<MenuActions>();Label(c.transform,"PROTOTYPE COMPLETE",new Vector2(0,100),TextAnchor.MiddleCenter,60,new Vector2(1100,110),true);Label(c.transform,"Twenty stages cleared. Behavior data was logged to the Console.",new Vector2(0,10),TextAnchor.MiddleCenter,24,new Vector2(1100,70));Button(c.transform,"TITLE",new Vector2(0,-110),actions.Title);EditorSceneManager.SaveScene(SceneManager.GetActiveScene(),"Assets/Scenes/PrototypeComplete.unity");}
    static void SimpleCamera(){var cam=new GameObject("Main Camera",typeof(Camera));cam.tag="MainCamera";cam.GetComponent<Camera>().backgroundColor=Paper;cam.GetComponent<Camera>().orthographic=true;cam.transform.position=new Vector3(0,0,-10);}
    static Canvas CanvasRoot(string name){var go=new GameObject(name,typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));var c=go.GetComponent<Canvas>();c.renderMode=RenderMode.ScreenSpaceOverlay;var s=go.GetComponent<CanvasScaler>();s.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;s.referenceResolution=new Vector2(1920,1080);new GameObject("EventSystem",typeof(UnityEngine.EventSystems.EventSystem),typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));return c;}
    static Text Label(Transform parent,string value,Vector2 pos,TextAnchor anchor,int size,Vector2 dims,bool bold=false){var go=new GameObject(value,typeof(RectTransform),typeof(Text));go.transform.SetParent(parent,false);var rt=go.GetComponent<RectTransform>();rt.sizeDelta=dims;rt.anchoredPosition=pos;if(anchor==TextAnchor.UpperLeft){rt.anchorMin=rt.anchorMax=new Vector2(0,1);rt.pivot=new Vector2(0,1);}var t=go.GetComponent<Text>();t.text=value;t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");t.fontSize=size;t.color=anchor==TextAnchor.UpperLeft?Ink:Color.white;t.alignment=anchor;t.fontStyle=bold?FontStyle.Bold:FontStyle.Normal;return t;}
    static Button Button(Transform parent,string value,Vector2 pos,UnityEngine.Events.UnityAction action){var go=new GameObject(value,typeof(RectTransform),typeof(Image),typeof(Button));go.transform.SetParent(parent,false);var rt=go.GetComponent<RectTransform>();rt.sizeDelta=new Vector2(320,64);rt.anchoredPosition=pos;go.GetComponent<Image>().color=new Color(.15f,.15f,.16f);var b=go.GetComponent<Button>();UnityEventTools.AddPersistentListener(b.onClick,action);Label(go.transform,value,Vector2.zero,TextAnchor.MiddleCenter,26,new Vector2(320,64),true);return b;}
    static void Stretch(RectTransform rt){rt.anchorMin=Vector2.zero;rt.anchorMax=Vector2.one;rt.offsetMin=rt.offsetMax=Vector2.zero;}
}
#endif
