using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace CyberCar.Editor
{
    public static class ProjectBuilder
    {
        [MenuItem("CyberCar/Create launch scene")]
        public static void CreateScene()
        {
            Directory.CreateDirectory("Assets/Scenes");
            Directory.CreateDirectory("Assets/Resources");
            foreach(string file in Directory.GetFiles("Assets/Resources/Photographic/Models","*.fbx")){
                var model=(ModelImporter)AssetImporter.GetAtPath(file.Replace('\\','/'));
                if(!model.isReadable){model.isReadable=true;model.materialImportMode=ModelImporterMaterialImportMode.ImportStandard;model.SaveAndReimport();}
            }
            if(AssetDatabase.LoadAssetAtPath<Material>("Assets/Resources/Impact.mat")==null)
                AssetDatabase.CreateAsset(new Material(Shader.Find("Particles/Standard Unlit")),"Assets/Resources/Impact.mat");
            CreateSky("DaySky","kloofendal_48d_partly_cloudy_puresky",.9f);
            CreateSky("NightSky","qwantani_night_puresky",.014f);
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            new GameObject("CyberCar Game").AddComponent<GameSession>();
            EditorSceneManager.SaveScene(scene,"Assets/Scenes/CyberCar.unity");
            EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/Scenes/CyberCar.unity",true)};
            PlayerSettings.companyName="CyberCarLearning";PlayerSettings.productName="CyberCarGame";
            PlayerSettings.defaultScreenWidth=1440;PlayerSettings.defaultScreenHeight=900;PlayerSettings.fullScreenMode=FullScreenMode.Windowed;PlayerSettings.resizableWindow=true;PlayerSettings.runInBackground=true;
            PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Standalone,ScriptingImplementation.Mono2x);
            PlayerSettings.colorSpace=ColorSpace.Linear;
            QualitySettings.vSyncCount=1;QualitySettings.antiAliasing=4;QualitySettings.shadowDistance=100;
            QualitySettings.pixelLightCount=8;
            AssetDatabase.SaveAssets();
            ValidateRoads();Debug.Log("CYBERCAR_SCENE_READY");
        }
        static void CreateSky(string name,string texture,float exposure)
        {
            string path="Assets/Resources/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(!m){m=new Material(Shader.Find("Skybox/Panoramic"));AssetDatabase.CreateAsset(m,path);}
            if(name=="NightSky")m.shader=Resources.Load<Shader>("StarrySky");
            var image=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Resources/Photographic/"+texture+".hdr");
            if(!image)throw new Exception("Missing HDR sky "+texture);
            m.SetTexture("_MainTex",image);m.SetFloat("_Exposure",exposure);m.SetFloat("_Rotation",40);EditorUtility.SetDirty(m);
        }
        public static void ValidateRoads()
        {
            for(int map=0;map<4;map++)
            {
                var graph=new RoadNetwork(map);
                for(int i=0;i<graph.Nodes.Count;i++)if(graph.Route(0,i).Count==0)throw new Exception("Disconnected map "+map+" node "+i);
                var route=graph.MissionRoute();if(route[0]!=0||route[route.Count-1]!=graph.Finish)throw new Exception("Invalid delivery route");
                var seen=new System.Collections.Generic.HashSet<string>();
                for(int seed=0;seed<32;seed++)
                {
                    var randomRoute=graph.MissionRoute(seed);seen.Add(string.Join(",",randomRoute));
                    for(int i=1;i<randomRoute.Count;i++)if(!graph.Edges.Exists(e=>(e.x==randomRoute[i-1]&&e.y==randomRoute[i])||(e.y==randomRoute[i-1]&&e.x==randomRoute[i])))throw new Exception("Non-adjacent route gates");
                }
                if(seen.Count<24)throw new Exception("Routes have insufficient variety");
                var large=new RoadNetwork(map,true);if(large.Nodes.Count!=81)throw new Exception("Extreme map size invalid");
            }
            Debug.Log("CYBERCAR_GRAPH_TESTS_PASS: 4 connected maps, 128 varied valid routes, 81-node extreme maps");
        }
        [MenuItem("CyberCar/Build Windows game")]
        public static void Build()
        {
            CreateScene();Directory.CreateDirectory("Builds/Windows");
            var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/Scenes/CyberCar.unity"},locationPathName="Builds/Windows/CyberCarGame.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development});
            if(result.summary.result!=BuildResult.Succeeded)throw new Exception("Build failed: "+result.summary.result);
            Debug.Log("CYBERCAR_BUILD_SUCCESS: "+result.summary.totalSize+" bytes");
        }
    }
    public sealed class CarImporter:AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
            if(!assetPath.Contains("/Photographic/"))return;
            var importer=(TextureImporter)assetImporter;importer.maxTextureSize=2048;importer.anisoLevel=8;importer.mipmapEnabled=true;
            if(assetPath.Contains("_normal.")||assetPath.Contains("_nor_gl_")){importer.textureType=TextureImporterType.NormalMap;importer.sRGBTexture=false;}
            else if(assetPath.Contains("_rough.")||assetPath.Contains("_arm_"))importer.sRGBTexture=false;
            if(assetPath.EndsWith(".hdr")){importer.wrapMode=TextureWrapMode.Clamp;importer.textureCompression=TextureImporterCompression.Uncompressed;}
        }
        void OnPreprocessModel()
        {
            if(assetPath.Contains("/Photographic/Models/")){var photo=(ModelImporter)assetImporter;photo.isReadable=true;photo.materialImportMode=ModelImporterMaterialImportMode.ImportStandard;return;}
            if(!assetPath.Contains("/Art/CyberInterceptor"))return;
            var importer=(ModelImporter)assetImporter;importer.materialImportMode=ModelImporterMaterialImportMode.ImportStandard;
        }
    }
}
