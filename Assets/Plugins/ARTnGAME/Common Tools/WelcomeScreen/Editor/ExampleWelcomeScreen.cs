using System;
using System.Collections.Generic;
using System.Linq;
using Artngame.CommonTools.WelcomeScreen;
using Artngame.CommonTools.WelcomeScreen.GuiElements;
using Artngame.CommonTools.WelcomeScreen.PreferenceDefinition;
using Artngame.CommonTools.WelcomeScreen.Utilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
namespace Artngame.CommonTools.WelcomeScreen
{
    public class WelcomeScreen : ProductWelcomeScreenBase
    {
        #region CustomisablePerProduct

        // Analytics
        // To enable analytics please go to https://app.immersiveVRtools.com
        // You can register there and get AnalyticsVerificationToken, website will also allow you to view data

        //v0.1
        //public Texture2D banners;

        //General
        public static bool IsUsageAnalyticsAndCommunicationsEnabled = false;
        public static readonly string AnalyticsVerificationToken = ""; //Only add if analytics enabled. Check website.
        public const string ProjectId = "";
        public static string VersionId = "1.0";

        public static string ProductName = "";
        private const string StartWindowMenuItemPath = "Window/ARTnGAME/Welcome Screen";
        public static string[] ProductKeywords = new[] { "start", "vr", "tools" };
        private static readonly string ProjectIconName = "ProductImage64";

        private static readonly string GIProxyIconName = "GIProxyIcon"; 
        private static readonly string SkyMasterIconName = "SkyMasterIcon"; 
        private static readonly string InfiniGRASSIconName = "InfiniGRASSIcon";
        private static readonly string OrionIconName = "OrionIcon";
        private static readonly string EtherealIconName = "EtherealIcon";

        private static readonly string InfiniCLOUDIconName = "InfiniCLOUDIcon";
        private static readonly string InfiniRIVERIconName = "InfiniRIVERIcon";
        private static readonly string InfiniTREEIconName = "InfiniTREEIcon";
        private static readonly string IvySTUDIOName = "ivyStudioIcon";
        private static readonly string PDMIconName = "ParticleDynamicMagicIcon";
        private static readonly string OCEANISIconName = "OceanisIcon";

        private static readonly string GIBLIIconName = "GIBLIIcon";
        private static readonly string PANGAEAIconName = "PANGAEAIcon";

        //Window Layout
        private static Vector2 _WindowSizePx = new Vector2(650, 500);
        private static readonly int LeftColumnWidth = 175;

        //Section Definitions
        private static readonly List<GuiSection> LeftSections = new List<GuiSection>() {
        new GuiSection("", new List<ClickableElement>
        {
            //Standard communication screen which is used when user has any message that you passed on to them
            new LastUpdateButton("New Update!", (screen) => LastUpdateSection.RenderMainScrollViewSection(screen)),
            //Standard welcome screen, that's visible unless there's new update
            new ChangeMainViewButton("Welcome", (screen) => MainContentSection.RenderMainScrollViewSection(screen)),
        }),
        new GuiSection("ARTnGAME", new List<ClickableElement>
        {
            //new ChangeMainViewButton("VR Integrations", (screen) =>
            //{
            //    GUILayout.Label(
            //        @"XR Toolkit will require some dependencies to run, please have a look in documentation, it should only take a few moments to set up.",
            //        screen.TextStyle
            //    );

            //    using (LayoutHelper.LabelWidth(200))
            //    {
            //        ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.EnableXrToolkitIntegrationPreferenceDefinition);
            //    }

            //    const int sectionBreakHeight = 15;
            //    GUILayout.Space(sectionBreakHeight);

            //    GUILayout.Label(
            //        @"VRTK require some dependencies to run, please have a look in documentation, it should only take a few moments to set up.",
            //        screen.TextStyle
            //    );

            //    using (LayoutHelper.LabelWidth(200))
            //    {
            //        ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.EnableVrtkIntegrationPreferenceDefinition);
            //    }
            //    GUILayout.Space(sectionBreakHeight);

            //}),
            //new ChangeMainViewButton("Shaders", (screen) =>
            //{
            //    GUILayout.Label(
            //        @"By default package uses HDRP shaders, you can change those to standard surface shaders from dropdown below",
            //        screen.TextStyle
            //    );

            //    using (LayoutHelper.LabelWidth(200))
            //    {
            //        ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
            //    }
            //})

             //if (base._productIconGIProxy != null) {
             //   GUILayout.Label(_productIconGIProxy);
             //   }

           

            //v0.1
            new ChangeMainViewButton("Global Illumination Proxy", (screen) =>
            {

                 //v0.1a
             GUILayout.Label(screen._productIconGIProxy, GUILayout.Width(450),GUILayout.Height(85));//  screen.banners,GUILayout.Width(120),GUILayout.Height(120));

                GUILayout.Label(
                    @"Global Illumination Proxy is a light manipulation framework that allows the emulation of indirect global illumination in Unity, with support for all Unity versions & platforms and optimizations for great performance on Mobile.


By default package uses Standard Pipeline shaders, you can change those to HDRP or URP by taking advantage of Unity material conversion tools.",
                    screen.TextStyle
                );

                using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("GI Proxy Tutorial Videos Playlist", "https://www.youtube.com/watch?v=d4B7gj5Myqw&list=PLJQvoQM6t9GfLLM7XDSIyzKApVXRKmHtq&index=7"
                     );
                }

                  GUILayout.Label(
                    @"Global Illumination Proxy can also be used with Sky Master ULTIMATE and is included in all Sky Master ULTIMATE versions.",
                    screen.TextStyle
                );

                using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("GI Proxy used with Sky Master ULTIMATE Video", "https://www.youtube.com/watch?v=G4T54SI0Cdk&list=PLJQvoQM6t9GfLLM7XDSIyzKApVXRKmHtq&index=6"
                     );
                }
                 using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("GI PROXY Asset Store Page", "https://assetstore.unity.com/packages/tools/level-design/global-illumination-proxy-21197"
                     );
                }

            }),
            //v0.1a
            new ChangeMainViewButton("Sky Master ULTIMATE", (screen) =>
            {

                 //v0.1a
             GUILayout.Label(screen._productIconSkyMaster, GUILayout.Width(450),GUILayout.Height(85));//  screen.banners,GUILayout.Width(120),GUILayout.Height(120));

                GUILayout.Label(
                    @"Sky Master ULTIMATE is a Dynamic Sky, Weather, Volumetric Cloud - Lighting, Dynamic GI & Ocean solution for Unity.",
                    screen.TextStyle
                );

                using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("Sky Master ULTIMATE Tutorial Videos Playlist", "https://www.youtube.com/watch?list=PLJQvoQM6t9Ge2ehO4N1kNq3jvHmVst_el&v=XXCMXiuM9VA"
                     );
                }

                  GUILayout.Label(
                    @"The Beta versions of the URP and HDRP complete remakes of the Sky Master ULTIMATE system are available to all users on PM request in email or Sky Master Unity forum thread.",
                    screen.TextStyle
                );

                using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("Sky Master URP - HDRP Tutorial Videos Playlist", "https://www.youtube.com/watch?list=PLJQvoQM6t9Gead80gg5MSyN-gsEzmO0LW&v=w6useVWMeMM"
                     );
                }
                using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("Sky Master URP Ethereal system Tutorial Videos Playlist", "https://www.youtube.com/watch?list=PLJQvoQM6t9GdvknDg470wVngbVAo_WEGo&v=vJIka5aX94o"
                     );
                }

                GUILayout.BeginHorizontal();
                 using (LayoutHelper.LabelWidth(100))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("Sky Master Unity Forum Thread", "https://forum.unity.com/threads/78-off-offer-sky-master-hdrp-urp-mobile-sky-ocean-volume-clouds-lighting-weather-realgi.280612/"
                     );
                }
                  using (LayoutHelper.LabelWidth(100))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("Sky Master Asset Store Page", "https://assetstore.unity.com/packages/tools/particles-effects/sky-master-ultimate-volumetric-skies-weather-25357"
                     );
                }
                  GUILayout.EndHorizontal();

            }),
            //v0.1a
            new ChangeMainViewButton("InfiniGRASS", (screen) =>
            {

                 //v0.1a
                GUILayout.Label(screen._productIconInfiniGRASS, GUILayout.Width(450),GUILayout.Height(85));//  screen.banners,GUILayout.Width(120),GUILayout.Height(120));

                GUILayout.Label(
                    @"InfiniGRASS is a robust grass and prefab painting and optimization system, which allows very detailed next gen grass, trees and foliage to be placed in mass quantities on any surface.",
                    screen.TextStyle
                );

                using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("InfiniGRASS Tutorial Videos Playlist", "https://www.youtube.com/watch?list=PLJQvoQM6t9Gddp8uags3YC4D9zUQ4CbYt&v=l-apm8ZcZuc"
                     );
                }

                  GUILayout.Label(
                    @"InfiniGRASS can also be used with Sky Master ULTIMATE for various effects including snow build up on foliage depending on weather conditions.",
                    screen.TextStyle
                );

                using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("InfiniGRASS used with Sky Master ULTIMATE Video", "https://www.youtube.com/watch?list=PLJQvoQM6t9Gf8C5CxuX52Srzxd7BMyk-5&v=eERpEqGUbpc"
                     );
                }

                //InfiniGRASS STUDIO
                GUILayout.Label(
                    @"InfiniGRASS STUDIO is the upcoming new asset that includes the next generation of the system that uses the InfiniGRASS as base. The STUDIO version is in closed Beta for InfiniGRASS and Sky Master ULTIMATE users.",
                    screen.TextStyle
                );
                using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("InfiniGRASS STUDIO Tutorial and showcase Videos", "https://www.youtube.com/watch?v=grJBt5VfU8U&list=PLJQvoQM6t9Gd8knVRnO0FWLoG7QMzIcpk&index=1"
                     );
                }                

                GUILayout.BeginHorizontal();
                 using (LayoutHelper.LabelWidth(100))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("InfiniGRASS Unity Forum Thread", "https://forum.unity.com/threads/50-off-offer-infinigrass-gpu-optimized-interactive-grass-trees-meshes-work-on-mobile-hdrp-urp.351694/"
                     );
                }
                using (LayoutHelper.LabelWidth(100))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("InfiniGRASS Asset Store Page", "https://assetstore.unity.com/packages/tools/particles-effects/infini-grass-gpu-vegetation-45188"
                     );
                }
                GUILayout.EndHorizontal();

            }),
            //v0.1a
            new ChangeMainViewButton("Ethereal URP Volume Lighting", (screen) =>
            {

                 //v0.1a
                GUILayout.Label(screen._productIconETHEREAL, GUILayout.Width(450),GUILayout.Height(85));//  screen.banners,GUILayout.Width(120),GUILayout.Height(120));

               GUILayout.Label(
                    @"Ethereal URP Volumetric lighting and Fog is an asset dedicated to the creation of atmospheric fog and volumetric lighting effects in the Universal Pipeline, by simulating the illumination of the small particles present in the atmosphere.",
                    screen.TextStyle
                );

                using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("Ethereal Tutorial Videos Playlist", "https://www.youtube.com/watch?list=PLJQvoQM6t9GdvknDg470wVngbVAo_WEGo&v=vJIka5aX94o"
                     );
                }

                  GUILayout.Label(
                    @"Ethereal is included in Sky Master ULTIMATE URP Version.",
                    screen.TextStyle
                );

                using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("Ethereal used in Sky Master ULTIMATE URP Video", "https://www.youtube.com/watch?list=PLJQvoQM6t9GemRdXnNGgPVbO1SqIGsdpA&v=_wPenCtIjsM"
                     );
                }

                GUILayout.BeginHorizontal();
                 using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("Ethereal Unity Forum Thread", "https://forum.unity.com/threads/71-off-offer-ethereal-urp-volumetric-lighting-atmosphere-and-fog-system-for-the-urp.1031233/"
                     );
                }
                  using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("Ethereal Asset Store Page", "https://assetstore.unity.com/packages/tools/particles-effects/ethereal-urp-volumetric-lighting-fog-187323"
                     );
                }
                  GUILayout.EndHorizontal();
            }),
            //v0.1a
            new ChangeMainViewButton("ORION Space Scene Creation", (screen) =>
            {

                 //v0.1a
                GUILayout.Label(screen._productIconORION, GUILayout.Width(450),GUILayout.Height(85));//  screen.banners,GUILayout.Width(120),GUILayout.Height(120));

                GUILayout.Label(
                    @"ORION Space Scene Generation framework is a system that covers all space scene generation aspects, from procedural planets & spaceships to any relevant special effects, with support for all pipelines.",
                    screen.TextStyle
                );

                using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("ORION Tutorial Videos Playlist", "https://www.youtube.com/watch?list=PLJQvoQM6t9Ge296gQqQdO56Q28la5btOj&v=YhinFIat1s0"
                     );
                }

                  GUILayout.Label(
                    @"ORION can also be used with Sky Master ULTIMATE for planetary volumetric clouds.",
                    screen.TextStyle
                );

                using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("ORION used with Sky Master ULTIMATE Video", "https://www.youtube.com/watch?list=PLJQvoQM6t9GemRdXnNGgPVbO1SqIGsdpA&v=SPofagIFiWw"
                     );
                }

                GUILayout.BeginHorizontal();
                 using (LayoutHelper.LabelWidth(100))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("ORION Unity Forum Thread", "https://forum.unity.com/threads/50-off-at-refresh-sale-orion-space-scene-generation-framework.1192270/"
                     );
                }
                    using (LayoutHelper.LabelWidth(100))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("ORION Asset Store Page", "https://assetstore.unity.com/packages/tools/level-design/orion-space-scene-generation-framework-206113"
                     );
                }
                GUILayout.EndHorizontal();


            }),


            //v0.1b
            new ChangeMainViewButton("InfiniRIVER Fluid Dynamics", (screen) =>
            {
                 //v0.1a
                GUILayout.Label(screen._productIconInfiniRIVER, GUILayout.Width(450),GUILayout.Height(85));//  screen.banners,GUILayout.Width(120),GUILayout.Height(120));

                GUILayout.Label(
                    @"InfiniRIVER is a Fluid based water simulator system that can emulate the interaction of water with scene objects in real time using a fluid solver in a global or local way for versatility.",
                    screen.TextStyle
                );

                using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("InfiniRIVER Tutorial Videos Playlist", "https://www.youtube.com/watch?list=PLJQvoQM6t9GddDxIVi0jNHbKuLcuWSjH1&v=LlFBoo4OMBU"
                     );
                }

                  GUILayout.Label(
                    @"InfiniRIVER can be used with two main fluid solutions, one terrain wide for big dynamic bodies of water and one local for detailed vortexes.",
                    screen.TextStyle
                );

                using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("InfiniRIVER Fluid solvers.", "https://www.youtube.com/watch?v=hXD8ObIxBIg"
                     );
                }

                GUILayout.BeginHorizontal();
                 using (LayoutHelper.LabelWidth(100))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("InfiniRIVER Unity Forum Thread", "https://forum.unity.com/threads/discount-offers-infiniriver-real-time-fluid-water-lava-simulator-river-generation.1186711/"
                     );
                }
                    using (LayoutHelper.LabelWidth(100))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("InfiniRIVER Asset Store Page", "https://assetstore.unity.com/packages/tools/terrain/infiniriver-fluid-based-water-simulator-205568"
                     );
                }
                GUILayout.EndHorizontal();
            }),
            //v0.1b
            new ChangeMainViewButton("InfiniCLOUD", (screen) =>
            {
                 //v0.1a
                GUILayout.Label(screen._productIconInfniCLOUD, GUILayout.Width(450),GUILayout.Height(85));//  screen.banners,GUILayout.Width(120),GUILayout.Height(120));

                GUILayout.Label(
                    @"InfiniCLOUD HDRP-URP is a volumetric clouds system for the new Scriptable Render Pipelines in Unity, with focus on optimization with spectacular graphics.",
                    screen.TextStyle
                );

                using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("InfiniCLOUD Tutorial Video", "https://www.youtube.com/watch?v=pr0r60lpN3c"
                     );
                }
                using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("InfiniCLOUD Tutorial Video used in Sky Master ULTIMATE", "https://www.youtube.com/watch?v=XXCMXiuM9VA"
                     );
                }
                  GUILayout.Label(
                    @"InfiniCLOUD is included in Sky Master ULTIMATE in all pipelines.",
                    screen.TextStyle
                );

                using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("InfiniCLOUD vortex system.", "https://www.youtube.com/watch?v=WZyCFdCXUvs"
                     );
                }

                GUILayout.BeginHorizontal();
                 using (LayoutHelper.LabelWidth(100))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("InfiniCLOUD Unity Forum Thread", "https://forum.unity.com/threads/new-offers-infinicloud-hdrp-urp-volumetric-clouds-for-the-new-pipelines-mobile.745415/page-3"
                     );
                }
                    using (LayoutHelper.LabelWidth(100))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("InfiniCLOUD Asset Store Page", "https://assetstore.unity.com/packages/tools/particles-effects/infinicloud-hdrp-urp-volumetric-clouds-particles-154133"
                     );
                }
                GUILayout.EndHorizontal();
            }),
            //v0.1b
            new ChangeMainViewButton("InfiniTREE", (screen) =>
            {
                 //v0.1a
                GUILayout.Label(screen._productIconInfiniTREE, GUILayout.Width(450),GUILayout.Height(85));//  screen.banners,GUILayout.Width(120),GUILayout.Height(120));

                GUILayout.Label(
                    @"InfiniTREE is a procedural tree generation and optimization system for Unity for all pipelines. It supports dynamic LOD management, ultimate ease of customization, using any prefab or Unity Tree Creator to define tree parts and interactive trees.",
                    screen.TextStyle
                );

                using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("InfiniTREE HDRP", "https://www.youtube.com/watch?v=-0g_wYUITqk"
                     );
                }
                using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("InfiniTREE tree chopping", "https://vimeo.com/122318534?embedded=true&source=video_title&owner=31557072"
                     );
                }
                  
                
                GUILayout.BeginHorizontal();
                 using (LayoutHelper.LabelWidth(100))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("InfiniTREE Unity Forum Thread", "https://forum.unity.com/threads/new-big-offers-infinitree-procedural-ltree-generation-growth-dynamics.287512/page-4"
                     );
                }
                    using (LayoutHelper.LabelWidth(100))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("InfiniTREE Asset Store Page", "https://assetstore.unity.com/packages/tools/level-design/infinitree-procedural-forest-creation-optimization-27457"
                     );
                }
                GUILayout.EndHorizontal();
            }),
            //v0.1b
            new ChangeMainViewButton("Ivy Studio", (screen) =>
            {
                 //v0.1a
                GUILayout.Label(screen._productIconIvyStudio, GUILayout.Width(450),GUILayout.Height(85));//  screen.banners,GUILayout.Width(120),GUILayout.Height(120));

                GUILayout.Label(
                    @"Ivy Studio is a procedural Ivy and climbing plants generator that focuses on the creation of realistic environment adaptive vegetation. Generate Ivy in editor or grow it gradually at runtime and interact with it in various ways.",
                    screen.TextStyle
                );

                using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("Ivy Studio Tutorials Playlist", "https://www.youtube.com/watch?v=mNpAjVFhlA0&list=PLJQvoQM6t9GfscpJJBgGEUd6TI-1YUmhm&index=1"
                     );
                }
                using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("Ivy Studio interactive vines", "https://www.youtube.com/watch?v=i192PVLQjxs"
                     );
                }


                GUILayout.BeginHorizontal();
                 using (LayoutHelper.LabelWidth(100))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("Ivy Studio Unity Forum Thread", "https://forum.unity.com/threads/50-off-initial-release-ivy-studio-next-gen-of-ivy-creation-optimize-in-editor-run-time.1260143/"
                     );
                }
                    using (LayoutHelper.LabelWidth(100))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("Ivy Studio Asset Store Page", "https://assetstore.unity.com/packages/tools/modeling/ivy-studio-procedural-vine-generation-217205"
                     );
                }
                GUILayout.EndHorizontal();
            }),
             //v0.1b
            new ChangeMainViewButton("Particle Dynamic Magic", (screen) =>
            {
                 //v0.1a
                GUILayout.Label(screen._productIconParticleDynamicMagic, GUILayout.Width(450),GUILayout.Height(85));//  screen.banners,GUILayout.Width(120),GUILayout.Height(120));

                GUILayout.Label(
                    @"Particle Dynamic Magic is a dynamic decal, particle and spline creation & manipulation framework that allows creative, performant and unique control of particles & gameobjects in Unity.",
                    screen.TextStyle
                );

                using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("Particle Dynamic Magic Showcase Videos Playlist", "https://www.youtube.com/watch?v=Qcu7ky0h5l8&list=PLJQvoQM6t9GdjWxtAIS9A6o3vBwg1WTYB&index=1"
                     );
                }
                using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("Particle Dynamic Magic 2.0 Video", "https://vimeo.com/129664655?embedded=true&source=video_title&owner=31557072"
                     );
                }
                
                GUILayout.BeginHorizontal();
                 using (LayoutHelper.LabelWidth(100))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("Particle Dynamic Magic Unity Forum Thread", "https://forum.unity.com/threads/new-offers-particle-dynamic-magic-2-advanced-fx-framework-decal-particle-spline-ai.239305/page-11"
                     );
                }
                    using (LayoutHelper.LabelWidth(100))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("Asset Store Page", "https://assetstore.unity.com/packages/tools/level-design/particle-dynamic-magic-2-decal-spline-ai-particles-dynamics-16175"
                     );
                }
                GUILayout.EndHorizontal();
            }),
            //v0.1b
            new ChangeMainViewButton("GIBLION Anime Scene Creation", (screen) =>
            {
                 //v0.1a
                GUILayout.Label(screen._productIconGIBLI, GUILayout.Width(450),GUILayout.Height(85));//  screen.banners,GUILayout.Width(120),GUILayout.Height(120));

                GUILayout.Label(
                    @"GIBLION - Anime scene generation framework is a next gen Anime scene creation tool for Unity URP. The system packs multiple toon, outlining and non photoreal shading techniques, blob based creation of dynamic polygon clouds and meshes, particle based foliage generation, dynamic toon grass, dynamic wires system, toon water and much more.",
                    screen.TextStyle
                );

                using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("GIBLION Showcase Videos Playlist", "https://www.youtube.com/watch?v=9tjj9KGu9Cs&list=PLJQvoQM6t9GfjoOi_RIx5c_taEo6J1F1_&index=1"
                     );
                }


                GUILayout.BeginHorizontal();
                 using (LayoutHelper.LabelWidth(100))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("GIBLION Unity Forum Thread", "https://forum.unity.com/threads/gibli-next-generation-anime-cartoon-scene-generation-framework.1235929/"
                     );
                }
                  using (LayoutHelper.LabelWidth(100))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("Asset Store Page", "https://assetstore.unity.com/packages/vfx/giblion-anime-scene-generation-framework-215911"
                     );
                }
                GUILayout.EndHorizontal();

                  using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("GIBLION Tutorial Video", "https://www.youtube.com/watch?v=lVAzRLqBqBg"
                     );
                }
                //    using (LayoutHelper.LabelWidth(100))
                //{
                //    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                //    ProductPreferenceBase.RenderURL("Oceanis Asset Store Page", "Coming Soon"
                //     );
                //}
               // GUILayout.EndHorizontal();
            }),
             //v0.1b
            new ChangeMainViewButton("Oceanis (Coming Soon)", (screen) =>
            {
                 //v0.1a
                GUILayout.Label(screen._productIconOceanis, GUILayout.Width(450),GUILayout.Height(85));//  screen.banners,GUILayout.Width(120),GUILayout.Height(120));

                GUILayout.Label(
                    @"Oceanis is a new upcoming ocean and water system for Unity HDRP and URP pipelines. The system is in closed Beta for Sky Master UTLIMATE users. Also a base version of Oceanis is included in Sky Master ULTIMATE HDRP version.",
                    screen.TextStyle
                );

                using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("Oceanis Beta Tutorial Video A", "https://www.youtube.com/watch?v=GNM02VZmOzQ&list=PLJQvoQM6t9Gead80gg5MSyN-gsEzmO0LW&index=6"
                     );
                }
                 using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("Oceanis Beta Tutorial Video B", "https://www.youtube.com/watch?v=0bIq0WF_Mm4&list=PLJQvoQM6t9GemRdXnNGgPVbO1SqIGsdpA&index=5"
                     );
                }
using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("Oceanis Underwater rendering", "https://www.youtube.com/watch?v=9hD10SzP_18"
                     );
                }

using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("Oceanis Massive number of Floaters", "https://www.youtube.com/watch?v=Pf00LuBCrLM"
                     );
                }

                GUILayout.BeginHorizontal();
                 using (LayoutHelper.LabelWidth(100))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("Oceanis Unity Forum Thread", "https://forum.unity.com/threads/oceanis-hdrp-urp-water-ocean-system.767168/"
                     );
                }
                //    using (LayoutHelper.LabelWidth(100))
                //{
                //    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                //    ProductPreferenceBase.RenderURL("Oceanis Asset Store Page", "Coming Soon"
                //     );
                //}
                GUILayout.EndHorizontal();
            }),            
            //v0.1b
            new ChangeMainViewButton("PANGAEA (Coming Soon)", (screen) =>
            {
                 //v0.1a
                GUILayout.Label(screen._productIconPANGAEA, GUILayout.Width(450),GUILayout.Height(85));//  screen.banners,GUILayout.Width(120),GUILayout.Height(120));

                GUILayout.Label(
                    @"PANGEA is the new ARTnGAME asset for Terrain generation based on a GPU enabled fluid simulator. The module presents a new way of sculpting of terrains using a real time fluid solution fully controllable by the user. The system allows for real time manipulation and generation of effects like erosion and land sculpting, and is working directly on GPU for maximum performance. A spline system is used to curve roads and rivers on the terrain, or place mountains and define terrain regions for editing.",
                    screen.TextStyle
                );

                using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("PANGAEA Tutorial and Showcase Videos Playlist", "https://www.youtube.com/watch?v=-P6GE5t3LZA&list=PLJQvoQM6t9GcQCSJtNtgP560BH_-dKy3O&index=1"
                     );
                }

                  GUILayout.Label(
                    @"PANGAEA supports multi mode editing, which includes a Non Destructive method of terrain generation.",
                    screen.TextStyle
                );

                  using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("PANGAEA Non Destructive workflow Video", "https://www.youtube.com/watch?v=B_9_MoRzPfg"
                     );
                }

                 GUILayout.Label(
                    @"PANGAEA is in closed Beta for InfiniGRASS and Sky Master ULTIMATE users.",
                    screen.TextStyle
                );

                GUILayout.BeginHorizontal();
                 using (LayoutHelper.LabelWidth(100))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("PANGAEA Unity Forum Thread", "https://forum.unity.com/threads/pangaea-next-gen-dynamic-fast-terrain-creation-using-gpu-fluid-simulation-and-spline-systems.1027885/"
                     );
                }
                //    using (LayoutHelper.LabelWidth(100))
                //{
                //    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                //    ProductPreferenceBase.RenderURL("Oceanis Asset Store Page", "Coming Soon"
                //     );
                //}
                GUILayout.EndHorizontal();
            }),
            ////// end list items
        }),
        //new GuiSection("Launch Demo", new List<ClickableElement>
        //{
        //    new LaunchSceneButton("XR Toolkit", (s) => GetXrToolkingDemoScenePath())
        //})
    };

        private static readonly GuiSection TopSection = new GuiSection("Support", new List<ClickableElement>
        {
            //new OpenUrlButton("Documentation", $"{BaseUrl}/{WebSafeProjectId}/documentation"),
            new OpenUrlButton("Unity Forum", $"https://forum.unity.com/threads/67-off-offer-sky-master-hdrp-urp-mobile-sky-ocean-volume-clouds-lighting-weather-realgi.280612/"),
            new OpenUrlButton("Email", $"mailto:artengames@gmail.com"),
            new OpenUrlButton("Discord", $"https://discord.gg/X6fX6J5")
        }
        );

        private static readonly ScrollViewGuiSection MainContentSection = new ScrollViewGuiSection(
            "", (screen) =>
            {
                GenerateCommonWelcomeText(ProductName, screen);

                GUILayout.Label("Latest Offers:", screen.LabelStyle);
                GUILayout.Label(
                   @"GI Proxy can be upgraded to Sky Master ULTIMATE for a discounted price.

Sky Master ULTIMATE can then be upgraded to all other major ARTnGAME 
assets with big discounts.

In order to enable the offers must first purchase the asset to upgrade 
from and then visit other asset pages to see the discounted price and 
buy the assets at a lower price directly.
");

                using (LayoutHelper.LabelWidth(200))
                {
                //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                ProductPreferenceBase.RenderURL("ARTnGAME Asset Store", "https://assetstore.unity.com/publishers/6503"
                     );
                }

            //GUILayout.Label("https://assetstore.unity.com/publishers/6503", screen.LabelStyle);
            //using (LayoutHelper.LabelWidth(220))
            //{
            //    ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.EnableXrToolkitIntegrationPreferenceDefinition);
            //    ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.EnableVrtkIntegrationPreferenceDefinition);
            //    ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
            //}
        }
        );

        private static readonly GuiSection BottomSection = new GuiSection(
            "Can I ask for a quick favour?",
            $"I'd be great help if you could spend few minutes to leave a review on:",
            new List<ClickableElement>
            {
            new OpenUrlButton(" ARTnGAME Unity Asset Store", $"https://assetstore.unity.com/publishers/6503"),
            }
        );

        private static readonly ScrollViewGuiSection LastUpdateSection = new ScrollViewGuiSection(
            "New Update", (screen) =>
            {
                GUILayout.Label(screen.LastUpdateText, screen.BoldTextStyle, GUILayout.ExpandHeight(true));
            }
        );

        #endregion
        private static string GetXrToolkingDemoScenePath()
        {
            var demoScene = AssetDatabase.FindAssets("t:Scene XRToolkitDemoScene").FirstOrDefault();
            return demoScene != null ? AssetDatabase.GUIDToAssetPath(demoScene) : null;
        }

        //Following code is required, please do not remove or amend
        #region RequiredSetupCode

        private static string WebSafeProjectId => Uri.EscapeDataString(ProjectId);
        public const string BaseUrl = "https://www.artengame.com";
        public static string GenerateGetUpdatesUrl(string userId, string versionId)
        {
            if (!IsUsageAnalyticsAndCommunicationsEnabled) return string.Empty;
            return $"{BaseUrl}/updates/{AnalyticsVerificationToken}/{WebSafeProjectId}/{versionId}/{userId}";
        }
        private static int RightColumnWidth => (int)_WindowSizePx.x - LeftSections.First().WidthPx - 15;
        public override string WindowTitle { get; } = ProductName;
        public override Vector2 WindowSizePx { get; } = _WindowSizePx;

        static WelcomeScreen()
        {
            foreach (var section in LeftSections)
                section.InitializeWidthPx(LeftColumnWidth);

            TopSection.InitializeWidthPx(RightColumnWidth);
            BottomSection.InitializeWidthPx(RightColumnWidth);
        }

        [MenuItem(StartWindowMenuItemPath, false, 1999)]
        public static void Init()
        {
            OpenWindow<WelcomeScreen>(ProductName, _WindowSizePx);
        }

        public void OnEnable()
        {
            OnEnableCommon(ProjectIconName);
            OnEnableCommonGIPROXY(GIProxyIconName);
            OnEnableCommonSKYMASTER(SkyMasterIconName);
            OnEnableCommonINFINIGRASS(InfiniGRASSIconName);
            OnEnableCommonORION(OrionIconName);
            OnEnableCommonETHEREAL(EtherealIconName);

            OnEnableCommonINFINITREE(InfiniTREEIconName);
            OnEnableCommonINFINICLOUD(InfiniCLOUDIconName);
            OnEnableCommonINFINIRIVER(InfiniRIVERIconName);
            OnEnableCommonIvySTUDIO(IvySTUDIOName);
            OnEnableCommonOCEANIS(OCEANISIconName);
            OnEnableCommonPDM(PDMIconName);

            OnEnableCommonGIBLI(GIBLIIconName);
            OnEnableCommonPANGAEA(PANGAEAIconName);

        }
        protected void OnEnableCommonETHEREAL(string productIconName)
        {
            if (!string.IsNullOrEmpty(productIconName) && base._productIconETHEREAL == null)
            {
                var iconGuid = AssetDatabase.FindAssets(productIconName).FirstOrDefault();
                if (!string.IsNullOrEmpty(iconGuid))
                {
                    _productIconETHEREAL = new GUIContent(AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(iconGuid)));
                }
            }
        }
        protected void OnEnableCommonORION(string productIconName)
        {
            if (!string.IsNullOrEmpty(productIconName) && base._productIconORION == null)
            {
                var iconGuid = AssetDatabase.FindAssets(productIconName).FirstOrDefault();
                if (!string.IsNullOrEmpty(iconGuid))
                {
                    _productIconORION = new GUIContent(AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(iconGuid)));
                }
            }
        }
        protected void OnEnableCommonGIPROXY(string productIconName)
        {
            if (!string.IsNullOrEmpty(productIconName) && base._productIconGIProxy == null)
            {
                var iconGuid = AssetDatabase.FindAssets(productIconName).FirstOrDefault();
                if (!string.IsNullOrEmpty(iconGuid))
                {
                    _productIconGIProxy = new GUIContent(AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(iconGuid)));
                }
            }
        }
        protected void OnEnableCommonSKYMASTER(string productIconName)
        {
            if (!string.IsNullOrEmpty(productIconName) && base._productIconSkyMaster == null)
            {
                var iconGuid = AssetDatabase.FindAssets(productIconName).FirstOrDefault();
                if (!string.IsNullOrEmpty(iconGuid))
                {
                    _productIconSkyMaster = new GUIContent(AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(iconGuid)));
                }
            }
        }
        protected void OnEnableCommonINFINIGRASS(string productIconName)
        {
            if (!string.IsNullOrEmpty(productIconName) && base._productIconInfiniGRASS == null)
            {
                var iconGuid = AssetDatabase.FindAssets(productIconName).FirstOrDefault();
                if (!string.IsNullOrEmpty(iconGuid))
                {
                    _productIconInfiniGRASS = new GUIContent(AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(iconGuid)));
                }
            }
        }

        //NEW1
        protected void OnEnableCommonINFINITREE(string productIconName)
        {
            if (!string.IsNullOrEmpty(productIconName) && base._productIconInfiniTREE == null)
            {
                var iconGuid = AssetDatabase.FindAssets(productIconName).FirstOrDefault();
                if (!string.IsNullOrEmpty(iconGuid))
                {
                    _productIconInfiniTREE = new GUIContent(AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(iconGuid)));
                }
            }
        }
        protected void OnEnableCommonINFINICLOUD(string productIconName)
        {
            if (!string.IsNullOrEmpty(productIconName) && base._productIconInfniCLOUD == null)
            {
                var iconGuid = AssetDatabase.FindAssets(productIconName).FirstOrDefault();
                if (!string.IsNullOrEmpty(iconGuid))
                {
                    _productIconInfniCLOUD = new GUIContent(AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(iconGuid)));
                }
            }
        }
        protected void OnEnableCommonINFINIRIVER(string productIconName)
        {
            if (!string.IsNullOrEmpty(productIconName) && base._productIconInfiniRIVER == null)
            {
                var iconGuid = AssetDatabase.FindAssets(productIconName).FirstOrDefault();
                if (!string.IsNullOrEmpty(iconGuid))
                {
                    _productIconInfiniRIVER = new GUIContent(AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(iconGuid)));
                }
            }
        }
        protected void OnEnableCommonIvySTUDIO(string productIconName)
        {
            if (!string.IsNullOrEmpty(productIconName) && base._productIconIvyStudio == null)
            {
                var iconGuid = AssetDatabase.FindAssets(productIconName).FirstOrDefault();
                if (!string.IsNullOrEmpty(iconGuid))
                {
                    _productIconIvyStudio = new GUIContent(AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(iconGuid)));
                }
            }
        }
        protected void OnEnableCommonOCEANIS(string productIconName)
        {
            if (!string.IsNullOrEmpty(productIconName) && base._productIconOceanis == null)
            {
                var iconGuid = AssetDatabase.FindAssets(productIconName).FirstOrDefault();
                if (!string.IsNullOrEmpty(iconGuid))
                {
                    _productIconOceanis = new GUIContent(AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(iconGuid)));
                }
            }
        }
        protected void OnEnableCommonPDM(string productIconName)
        {
            if (!string.IsNullOrEmpty(productIconName) && base._productIconParticleDynamicMagic == null)
            {
                var iconGuid = AssetDatabase.FindAssets(productIconName).FirstOrDefault();
                if (!string.IsNullOrEmpty(iconGuid))
                {
                    _productIconParticleDynamicMagic = new GUIContent(AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(iconGuid)));
                }
            }
        }

        protected void OnEnableCommonPANGAEA(string productIconName)
        {
            if (!string.IsNullOrEmpty(productIconName) && base._productIconPANGAEA == null)
            {
                var iconGuid = AssetDatabase.FindAssets(productIconName).FirstOrDefault();
                if (!string.IsNullOrEmpty(iconGuid))
                {
                    _productIconPANGAEA = new GUIContent(AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(iconGuid)));
                }
            }
        }
        protected void OnEnableCommonGIBLI(string productIconName)
        {
            if (!string.IsNullOrEmpty(productIconName) && base._productIconGIBLI == null)
            {
                var iconGuid = AssetDatabase.FindAssets(productIconName).FirstOrDefault();
                if (!string.IsNullOrEmpty(iconGuid))
                {
                    _productIconGIBLI = new GUIContent(AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(iconGuid)));
                }
            }
        }


        public void OnGUI()
        {
            RenderGUI(LeftSections, TopSection, BottomSection, MainContentSection);
        }

        #endregion
    }

    public class WelcomeScreenPreferences : ProductPreferenceBase
    {
        #region CustomisablePerProduct

        public static string BuildSymbol_EnableXrToolkit = "INTEGRATIONS_XRTOOLKIT";
        public static string BuildSymbol_EnableVRTK = "INTEGRATIONS_VRTK";


        public static readonly ToggleProjectEditorPreferenceDefinition EnableXrToolkitIntegrationPreferenceDefinition = new ToggleProjectEditorPreferenceDefinition(
            "Enable Unity XR Toolkit integration", "XRToolkitIntegrationEnabled", true,
            (newValue, oldValue) =>
            {
            //BuildDefineSymbolManager.SetBuildDefineSymbolState(BuildSymbol_EnableXrToolkit, (bool)newValue);
        });

        public static readonly ToggleProjectEditorPreferenceDefinition EnableVrtkIntegrationPreferenceDefinition = new ToggleProjectEditorPreferenceDefinition(
            "Enable VRTK integration", "VRTKIntegrationEnabled", false,
            (newValue, oldValue) =>
            {
            //BuildDefineSymbolManager.SetBuildDefineSymbolState(BuildSymbol_EnableVRTK, (bool)newValue);
        });

        public static readonly EnumProjectEditorPreferenceDefinition ShaderModePreferenceDefinition = new EnumProjectEditorPreferenceDefinition("Shaders",
            "ShadersMode",
            ShadersMode.HDRP,
            typeof(ShadersMode),
            (newValue, oldValue) =>
            {
                if (oldValue == null) oldValue = default(ShadersMode);

                var newShaderModeValue = (ShadersMode)newValue;
                var oldShaderModeValue = (ShadersMode)oldValue;

                if (newShaderModeValue != oldShaderModeValue)
                {
                //SetCommonMaterialsShader(newShaderModeValue);
            }
            }
        );

        public static void SetCommonMaterialsShader(ShadersMode newShaderModeValue)
        {
            var rootToolFolder = AssetPathResolver.GetAssetFolderPathRelativeToScript(ScriptableObject.CreateInstance(typeof(WelcomeScreen)), 1);
            var assets = AssetDatabase.FindAssets("t:Material", new[] { rootToolFolder });

            try
            {
                Shader shaderToUse = null;
                switch (newShaderModeValue)
                {
                    case ShadersMode.HDRP: shaderToUse = Shader.Find("HDRP/Lit"); break;
                    case ShadersMode.URP: shaderToUse = Shader.Find("Universal Render Pipeline/Lit"); break;
                    case ShadersMode.Surface: shaderToUse = Shader.Find("Standard"); break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                foreach (var guid in assets)
                {
                    var material = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));
                    if (material.shader.name != shaderToUse.name)
                    {
                        material.shader = shaderToUse;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Shader does not exist: {ex.Message}");
            }
        }

        public enum ShadersMode
        {
            HDRP,
            URP,
            Surface
        }

        //Add all preference options that you'd like to be persisted for project
        public static List<ProjectEditorPreferenceDefinitionBase> PreferenceDefinitions = new List<ProjectEditorPreferenceDefinitionBase>()
    {
        CreateDefaultShowOptionPreferenceDefinition(),
        EnableXrToolkitIntegrationPreferenceDefinition,
        EnableVrtkIntegrationPreferenceDefinition,
        ShaderModePreferenceDefinition
    };

        #endregion


        #region RequiredSetupCode
        private static bool PrefsLoaded = false;


#if UNITY_2019_1_OR_NEWER
        [SettingsProvider]
        public static SettingsProvider ImpostorsSettings()
        {
            return GenerateProvider(WelcomeScreen.ProductName, WelcomeScreen.ProductKeywords, PreferencesGUI);
        }

#else
	[PreferenceItem(ProductName)]
#endif
        public static void PreferencesGUI()
        {
            if (!PrefsLoaded)
            {
                LoadDefaults(PreferenceDefinitions);
                PrefsLoaded = true;
            }

            RenderGuiCommon(PreferenceDefinitions);
        }

        #endregion
    }

    [InitializeOnLoad]
    public class WelcomeScreenInitializer : WelcomeScreenInitializerBase
    {
        #region CustomisablePerProduct

        public static void RunOnWindowOpened(bool isFirstRun)
        {
            AutoDetectAndSetShaderMode();
        }

        private static void AutoDetectAndSetShaderMode()
        {
            //var usedShaderMode = WelcomeScreenPreferences.ShadersMode.Surface;
            //var renderPipelineAsset = GraphicsSettings.renderPipelineAsset;
            //if (renderPipelineAsset == null)
            //{
            //    usedShaderMode = WelcomeScreenPreferences.ShadersMode.Surface;
            //}
            //else if (renderPipelineAsset.GetType().Name.Contains("HDRenderPipelineAsset"))
            //{
            //    usedShaderMode = WelcomeScreenPreferences.ShadersMode.HDRP;
            //}
            //else if (renderPipelineAsset.GetType().Name.Contains("UniversalRenderPipelineAsset"))
            //{
            //    usedShaderMode = WelcomeScreenPreferences.ShadersMode.URP;
            //}

            //WelcomeScreenPreferences.ShaderModePreferenceDefinition.SetEditorPersistedValue(usedShaderMode);
            //WelcomeScreenPreferences.SetCommonMaterialsShader(usedShaderMode);
        }

        #endregion


        #region RequiredSetupCode

        static WelcomeScreenInitializer()
        {
            var userId = ProductPreferenceBase.CreateDefaultUserIdDefinition(WelcomeScreen.ProjectId).GetEditorPersistedValueOrDefault().ToString();

            HandleUnityStartup(WelcomeScreen.Init, WelcomeScreen.GenerateGetUpdatesUrl(userId, WelcomeScreen.VersionId), RunOnWindowOpened);
        }

        #endregion
    }
}