// Project: WorldCreatorBridge
// Filename: BridgeEditor.cs
// Copyright (c) 2022 BiteTheBytes GmbH. All rights reserved
// *********************************************************

using System;
using System.Globalization;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

#if UNITY_EDITOR

namespace BtB.WC.Bridge
{
    [Serializable]
    public class BridgeEditor : EditorWindow, IHasCustomMenu
    {
        private class ImportPostprocessor : AssetPostprocessor
        {
            public static bool WorldCreatorModelImportActive = false;
            public static bool WorldCreatorTextureImportActive = false;

            private void OnPreprocessModel()
            {
                if (!WorldCreatorModelImportActive) return;
                
                ModelImporter modelImporter = assetImporter as ModelImporter;
                modelImporter.materialImportMode = ModelImporterMaterialImportMode.None;
            }
            
            private void OnPostprocessTexture(Texture2D texture)
            {
                if (!WorldCreatorTextureImportActive) return;
                TextureImporter importer = assetImporter as TextureImporter;

                // Detect original texture resolution
                Texture2D tmpTexture = new Texture2D(1, 1);
                byte[] tmpBytes = File.ReadAllBytes(importer.assetPath);
                tmpTexture.LoadImage(tmpBytes);

                importer.maxTextureSize = tmpTexture.width;
            }
        }

        #region Fields

        #region Private
        
        private BridgeLogic logic = new BridgeLogic();

        private readonly string[] toolbarItems =
        {
            "General",
            "About"
        };

        private bool locked;

        private Vector2 scrollPosGeneralTab;
        //private Vector2 scrollPosObjects; 

        private int selectedToolbarItemIndex;
        private Vector2 folderScrollPos;
        
        #endregion
        
        #region Public

        public static BridgeEditor Window;

        public BridgeSettings settings;

        private Sprite bannerWorldCreator;
        private Sprite logoYouTube;
        private Sprite logoFacebook;
        private Sprite logoTwitter;
        private Sprite logoDiscord;
        private Sprite logoArtstation;
        private Sprite logoInstagram;
        private Sprite logoVimeo;
        private Sprite logoTwitch;
        
        #endregion Public

        #endregion Fields
        
        #region Methods (Public)

        public void Awake()
        {
            LoadSettings();
        }

        #region Settings

        private string GetSettingsDirectory()
        {
            return Application.dataPath + @"/WorldCreatorBridge/Settings";
        }
        
        private string GetSettingsFilePath()
        {
            return GetSettingsDirectory() + "/BridgeSettings.json";
        }

        private void SaveSettings()
        {
            try
            {
                DirectoryInfo target = new DirectoryInfo(GetSettingsDirectory());

                if (!target.Exists)
                {
                    target.Create();
                    target.Refresh();
                }

                string settingsFilePath = GetSettingsFilePath();

                string dataAsJson = JsonUtility.ToJson(settings);
                File.WriteAllText(settingsFilePath, dataAsJson);

                Debug.Log("Saving Bridge Settings: " + settingsFilePath);
            }
            catch (Exception e)
            {
                Debug.Log("Couldn't save settings: " + e);
            }
        }

        private void LoadSettings()
        {
            try
            {
                string settingsFilePath = GetSettingsFilePath();
                
                Debug.Log("Loading Bridge Settings: " + settingsFilePath);

                if (File.Exists(settingsFilePath))
                {
                    string dataAsJson = File.ReadAllText(settingsFilePath);
                    settings = JsonUtility.FromJson<BridgeSettings>(dataAsJson);
                }
                else
                {
                    settings = new BridgeSettings();
                }
            }
            catch (Exception e)
            {
                Debug.Log("Couldn't load settings: " + e);
            }
        }

        #endregion Settings

        public void Update()
        {
            logic.Update();
        }

        public void OnGUI()
        {
            if(settings == null)
                LoadSettings();

            EditorGUILayout.BeginVertical("box");
            {
                selectedToolbarItemIndex = GUILayout.Toolbar(selectedToolbarItemIndex, toolbarItems, GUILayout.Height(32));
            }
            
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("box");
            {
                scrollPosGeneralTab = GUILayout.BeginScrollView(scrollPosGeneralTab);
                {
                    switch (selectedToolbarItemIndex)
                    {
                        case 0:
                            DrawTabGeneral();
                            break;
                        case 1:
                            DrawTabAbout();
                            break;
                    }
                }
                
                GUILayout.EndScrollView();
                
                GUILayout.FlexibleSpace();
            }
            
            EditorGUILayout.EndVertical();
            
            // Only show the synchronize button when a project folder has been selected
            if (settings.IsBridgeFileValid())
            {
                if (GUILayout.Button("SYNCHRONIZE", GUILayout.Height(50)))
                {
                    if (!File.Exists(settings.bridgeFilePath))
                    {
                        Debug.LogError("Selected file does not exist");
                        return;
                    }
                    
                    // Copy the sync folder...
                    string terrainFolder = Application.dataPath + @"/" + settings.TerrainsFolderName + "/" + settings.TerrainAssetName;
                    DirectoryInfo target = new DirectoryInfo(terrainFolder + "/Assets");
                    DirectoryInfo source = new DirectoryInfo(settings.bridgeFilePath).Parent;

                    if (source != null && source.Parent != null)
                        source = new DirectoryInfo(source.FullName + "/Assets/");

                    if (settings.DeleteUnusedAssets && Directory.Exists(@"Assets/" + settings.TerrainsFolderName + "/" + settings.TerrainAssetName))
                    {
                        foreach (string num in Directory.GetFiles(@"Assets/" + settings.TerrainsFolderName + "/" + settings.TerrainAssetName))
                            if (num.Contains(settings.TerrainAssetName + "_") || num.EndsWith(".mat") || num.EndsWith(".terrainlayer"))
                                AssetDatabase.DeleteAsset(num);
                        
                        foreach (string num in Directory.GetFiles(@"Assets/" + settings.TerrainsFolderName + "/" + settings.TerrainAssetName + "/Assets/"))
                            AssetDatabase.DeleteAsset(num);
                    }
                    
                    ImportPostprocessor.WorldCreatorModelImportActive = true;
                    ImportPostprocessor.WorldCreatorTextureImportActive = true;
                    AssetDatabase.StartAssetEditing();
                    CopyAll(source, target);
                    AssetDatabase.StopAssetEditing();
                    AssetDatabase.Refresh();
                    ImportPostprocessor.WorldCreatorModelImportActive = false;

                    // Copy color map
                    try
                    {
                        if (File.Exists(source.Parent.FullName + "/colormap.png")) 
                            File.Copy(source.Parent.FullName + "/colormap.png", terrainFolder + "/colormap.png", true);
                        else 
                            File.Delete(terrainFolder + "/colormap.png");
                    }
                    catch (Exception e)
                    {
                        Debug.Log(e.Message);
                    }

                    AssetDatabase.Refresh();
                    ImportPostprocessor.WorldCreatorTextureImportActive = false;

                    // ... perform synchronization ...
                    logic.Synchronize(settings);
                    
                    // save the settings for the next time the window is used
                    SaveSettings();
                }
            }
        }

        private void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            Directory.CreateDirectory(target.FullName);

            foreach (FileInfo fileInfo in source.GetFiles())
                fileInfo.CopyTo(Path.Combine(target.FullName, fileInfo.Name), true);

            foreach(DirectoryInfo dirSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(dirSourceSubDir.Name);
                CopyAll(dirSourceSubDir, nextTargetSubDir);
            }
        }

        public void AddItemsToMenu(GenericMenu menu)
        {
            menu.AddItem(new GUIContent("Lock"), locked, () => { locked = !locked; });
        }
        
        #endregion Methods (Public)
        
        #region Methods (Private)

        private void DrawTabGeneral()
        {
            GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.alignment = TextAnchor.MiddleLeft;
            boxStyle.normal.textColor = EditorGUIUtility.isProSkin ? new Color(0.81f, 0.77f, 0.67f) : Color.black;
            boxStyle.stretchWidth = true;
            float spacePixels = 8;

            // Reset Button
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Reset Settings", GUILayout.Width(160)))
                    settings = new BridgeSettings
                    {
                        bridgeFilePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"/World Creator/Sync/bridge.xml"
                    };
            }
            EditorGUILayout.EndHorizontal();
            
            GUILayout.Box("If you moved the sync .xml please select it here. Its default location is in: \n[USER]/Documents/WorldCreator/Sync/bridge.xml", boxStyle);

            GUILayout.Space(spacePixels);

            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("SELECT BRIDGE .xml FILE", GUILayout.Height(30)))
                    logic.SelectProjectFolder(settings);
            }
            GUILayout.EndHorizontal();

            GUI.enabled = false;
            
            folderScrollPos = EditorGUILayout.BeginScrollView(folderScrollPos);
            { 
                string path = settings.IsBridgeFileValid() ? settings.bridgeFilePath : logic.projectFolderPath;

                EditorGUILayout.SelectableLabel(path, EditorStyles.textField, GUILayout.Height(EditorGUIUtility.singleLineHeight));
            }
            EditorGUILayout.EndScrollView();
            
            GUI.enabled = true;

            GUILayout.Space(spacePixels);
            
            GUILayout.BeginHorizontal();
            {
                settings.TerrainAssetName = EditorGUILayout.TextField(new GUIContent("Terrain Asset Name", "Name of the GameObject container that holds your terrain GameObject(s)."),settings.TerrainAssetName, GUILayout.ExpandWidth(true));
            }
            GUILayout.EndHorizontal();
            
            GUILayout.Space(spacePixels);
            
            GUILayout.BeginHorizontal();
            {
                settings.DeleteUnusedAssets = EditorGUILayout.Toggle(new GUIContent("Delete unused Assets", "If enabled automatically cleans up unused terrain assets."), settings.DeleteUnusedAssets);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(spacePixels);
            
            GUILayout.BeginHorizontal();
            {
                settings.IsImportTextures = EditorGUILayout.Toggle(new GUIContent("Import Textures", "Choose whether textures are automatically imported."), settings.IsImportTextures);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(spacePixels);

            GUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField(new GUIContent("Split Threshold", "Specifies when to split the created Unity terrain. This might be important if you want to split your Unity terrain into smaller chunks (e.g. for streaming)."), GUILayout.Width(160));
                settings.SplitThreshold = Mathf.RoundToInt(GUILayout.HorizontalSlider(settings.SplitThreshold, 0, 5));
                GUILayout.Label((settings.SplitResolution).ToString(), GUILayout.Width(36));            
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(spacePixels);

            GUIStyle textFieldStyle = new GUIStyle(GUI.skin.textField) {alignment = TextAnchor.MiddleRight};

            GUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField(new GUIContent("World Scale", "Allows you to scale the terrain to a different value."), GUILayout.Width(160));

                float oldScale = settings.WorldScale;
                float newScale = GUILayout.HorizontalSlider(settings.WorldScale, 0, 8, GUILayout.ExpandWidth(true));
                if (oldScale != newScale)
                {
                    settings.WorldScaleString = newScale.ToString("#0.00");
                    settings.WorldScale = newScale;
                }

                var oldString = settings.WorldScaleString;
                settings.WorldScaleString = GUILayout.TextField(settings.WorldScaleString, textFieldStyle, GUILayout.Width(50));
                
                if (oldString != settings.WorldScaleString)
                    if (float.TryParse(settings.WorldScaleString, NumberStyles.Any, CultureInfo.InvariantCulture, out float newVal))
                        settings.WorldScale = newVal;
                
                GUILayout.Label("m", GUILayout.Width(14));
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(spacePixels);
            
            GUILayout.BeginHorizontal();
            {
                settings.MaterialType = (MaterialType)EditorGUILayout.EnumPopup(new GUIContent("Material Type", "The rendering pipeline for which the terrain material will be set up for. Chose custom to set you own material."),settings.MaterialType, GUILayout.ExpandWidth(true));
            }
            GUILayout.EndHorizontal();
            
            GUIStyle warningStyle = new GUIStyle(GUI.skin.box);
            warningStyle.alignment = TextAnchor.MiddleLeft;
            warningStyle.normal.textColor = EditorGUIUtility.isProSkin ? new Color(0.988f, 0.746f, 0.02f) : Color.black;
            warningStyle.stretchWidth = true;

            if (settings.LayerWarning)
            {
                if (settings.MaterialType == MaterialType.URP || settings.MaterialType == MaterialType.Standard)
                {
                    GUILayout.Space(spacePixels);

                    GUILayout.Box("Warning - " + (settings.MaterialType == MaterialType.URP ? "URP" : "The Built-in Render Pipeline") + " uses additional shader passes with more than 4 terrain layers. This can cause performance issues. To avoid this reduce your material layers in World Creator." , warningStyle);

                    GUILayout.Space(spacePixels);
                }
                else if (settings.MaterialType == MaterialType.HDRP)
                {
                    GUILayout.Space(spacePixels);

                    GUILayout.Box("Warning - HDRP only supports up to 8 terrain layers. Every additional layer will not be rendered. To avoid this reduce your material layers in World Creator." , warningStyle);

                    GUILayout.Space(spacePixels);
                }
            }

            if (settings.MaterialType == MaterialType.Custom)
            {
                GUILayout.BeginHorizontal();
                {
                    settings.CustomMaterial = EditorGUILayout.ObjectField("", settings.CustomMaterial, typeof(Material), false, GUILayout.ExpandWidth(true)) as Material;
                }
                GUILayout.EndHorizontal();
            }
        }

        private void DrawTabAbout()
        {
            string spritesFolder = @"Assets/WorldCreatorBridge/Content/Sprites/";
            
            bannerWorldCreator = AssetDatabase.LoadAssetAtPath<Sprite>(spritesFolder + (EditorGUIUtility.isProSkin ? "banner_wc.png" : "banner_wc_inv.png"));
            logoYouTube = AssetDatabase.LoadAssetAtPath<Sprite>(spritesFolder + (EditorGUIUtility.isProSkin ? "icon_youtube.png" : "icon_youtube_inv.png"));
            logoFacebook = AssetDatabase.LoadAssetAtPath<Sprite>(spritesFolder + (EditorGUIUtility.isProSkin ? "icon_facebook.png" : "icon_facebook_inv.png"));
            logoTwitter = AssetDatabase.LoadAssetAtPath<Sprite>(spritesFolder + (EditorGUIUtility.isProSkin ? "icon_twitter.png" : "icon_twitter_inv.png"));
            logoInstagram = AssetDatabase.LoadAssetAtPath<Sprite>(spritesFolder + (EditorGUIUtility.isProSkin ? "icon_instagram.png" : "icon_instagram_inv.png"));
            logoVimeo = AssetDatabase.LoadAssetAtPath<Sprite>(spritesFolder + (EditorGUIUtility.isProSkin ? "icon_vimeo.png" : "icon_vimeo_inv.png"));
            logoTwitch = AssetDatabase.LoadAssetAtPath<Sprite>(spritesFolder + (EditorGUIUtility.isProSkin ? "icon_twitch.png" : "icon_twitch_inv.png"));
            logoDiscord = AssetDatabase.LoadAssetAtPath<Sprite>(spritesFolder + (EditorGUIUtility.isProSkin ? "icon_discord.png" : "icon_discord_inv.png"));
            logoArtstation = AssetDatabase.LoadAssetAtPath<Sprite>(spritesFolder + (EditorGUIUtility.isProSkin ? "icon_artstation.png" : "icon_artstation_inv.png"));
            
            if(bannerWorldCreator != null)
                if(GUILayout.Button(bannerWorldCreator.texture))
                    Application.OpenURL("https://www.world-creator.com");

            GUIStyle guiStyleButton = new GUIStyle(GUI.skin.button) { fontSize = 18 };
            GUIStyle styleLegal = new GUIStyle(GUI.skin.box) { richText = true };
            GUILayoutOption[] guiLayoutOptionsHelpLarge = {GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)};

            string col = EditorGUIUtility.isProSkin ? "#D0C6AB" : "#000000";

            GUILayout.Box
            ("<color=" + col + ">\nJoin our community on DISCORD and follow us on our social sites to get the " +
             "latest information of the World Creator product series.\n\n" +
             "Get in touch with the devs and share your ideas and suggestions.\n</color>", styleLegal, guiLayoutOptionsHelpLarge);

            GUILayout.BeginHorizontal();
            {
                if (logoDiscord != null)
                    if (GUILayout.Button(logoDiscord.texture))
                        Application.OpenURL("https://discordapp.com/invite/bjMteus");
                
                if(logoFacebook != null)
                    if(GUILayout.Button(logoFacebook.texture))
                        Application.OpenURL("https://www.facebook.com/worldcreator3d");
                
                if(logoTwitter != null)
                    if(GUILayout.Button(logoTwitter.texture))
                        Application.OpenURL("https://twitter.com/worldcreator3d");
                
                if(logoYouTube != null)
                    if(GUILayout.Button(logoYouTube.texture))
                        Application.OpenURL("https://www.youtube.com/channel/UClabqa6PHVjXzR2Y7s1MP0Q");
            }
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            {
                if (logoInstagram != null)
                    if (GUILayout.Button(logoInstagram.texture))
                        Application.OpenURL("https://www.instagram.com/worldcreator3d/");

                if (logoVimeo != null)
                    if (GUILayout.Button(logoVimeo.texture))
                        Application.OpenURL("https://vimeo.com/user82114310");

                if (logoTwitch != null)
                    if (GUILayout.Button(logoTwitch.texture))
                        Application.OpenURL("https://www.twitch.tv/worldcreator3d");

                if (logoArtstation != null)
                    if (GUILayout.Button(logoArtstation.texture))
                        Application.OpenURL("https://www.artstation.com/worldcreator");
            }
            GUILayout.EndHorizontal();

            GUILayout.Box("<color=" + col + ">\nWorld Creator Bridge for Unity \nVersion 1.0.0 \n</color>", styleLegal, guiLayoutOptionsHelpLarge);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("COMPANY", guiStyleButton))
                EditorUtility.DisplayDialog(
                    "About - Company",
                    "BiteTheBytes GmbH\n" + "Mainzer Str. 9\n" + "36039 Fulda\n\n" +
                    "Responsible: BiteTheBytes GmbH\n" + "Commercial Register Fulda: HRB 5804\n" +
                    "VAT / Ust-IdNr: DE 272746606", "OK");
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("WEBSITE", guiStyleButton))
                {
                    Application.OpenURL("https://www.world-creator.com");
                }

                if (GUILayout.Button("DISCORD", guiStyleButton))
                    Application.OpenURL("https://discordapp.com/invite/bjMteus");
            }
            GUILayout.EndHorizontal();
        }

        #endregion Methods (Private)

        #region Methods (Static / Public)

        [MenuItem("Window/World Creator Bridge")]
        public static void Init()
        {
            Window = (BridgeEditor) GetWindow(typeof(BridgeEditor));
            Window.autoRepaintOnSceneChange = true;
            Window.minSize = new Vector2(425, 500);
            Window.titleContent = new GUIContent("World Creator Bridge", AssetDatabase.LoadAssetAtPath<Texture2D>(@"Assets/WorldCreatorBridge/Content/Sprites/" + (EditorGUIUtility.isProSkin ? "icon_wc.png" : "icon_wc_inv.png")));
            Window.Show();
        }
        
        #endregion Methods (Static / Public)
    }
}

#endif