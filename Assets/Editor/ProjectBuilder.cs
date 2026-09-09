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
            if(AssetDatabase.LoadAssetAtPath<Material>("Assets/Resources/Impact.mat")==null)
                AssetDatabase.CreateAsset(new Material(Shader.Find("Particles/Standard Unlit")),"Assets/Resources/Impact.mat");
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            new GameObject("CyberCar Game").AddComponent<GameSession>();
            EditorSceneManager.SaveScene(scene,"Assets/Scenes/CyberCar.unity");
            EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/Scenes/CyberCar.unity",true)};
            PlayerSettings.companyName="CyberCarLearning";PlayerSettings.productName="CyberCarGame";
            PlayerSettings.defaultScreenWidth=1440;PlayerSettings.defaultScreenHeight=900;PlayerSettings.fullScreenMode=FullScreenMode.Windowed;PlayerSettings.resizableWindow=true;PlayerSettings.runInBackground=true;
            PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Standalone,ScriptingImplementation.Mono2x);
            PlayerSettings.colorSpace=ColorSpace.Linear;
            QualitySettings.vSyncCount=1;QualitySettings.antiAliasing=4;QualitySettings.shadowDistance=100;
            AssetDatabase.SaveAssets();
            ValidateRoads();Debug.Log("CYBERCAR_SCENE_READY");
        }
        public static void ValidateRoads()
        {
            for(int map=0;map<3;map++)
            {
                var graph=new RoadNetwork(map);
                for(int i=0;i<graph.Nodes.Count;i++)if(graph.Route(0,i).Count==0)throw new Exception("Disconnected map "+map+" node "+i);
                var route=graph.MissionRoute();if(route[0]!=0||route[route.Count-1]!=graph.Finish)throw new Exception("Invalid delivery route");
            }
            Debug.Log("CYBERCAR_GRAPH_TESTS_PASS: all nodes reachable on all 3 maps");
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
        void OnPreprocessModel()
        {
            if(!assetPath.EndsWith("CyberInterceptor.fbx"))return;
            var importer=(ModelImporter)assetImporter;importer.materialImportMode=ModelImporterMaterialImportMode.ImportStandard;
        }
    }
}
