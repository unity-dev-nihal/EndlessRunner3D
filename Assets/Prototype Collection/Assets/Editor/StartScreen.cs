// Prototype Collection - Street Props
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;

namespace AmplifyCreations.PrototypeCollection.StreetProps
{
    public class StartScreen : EditorWindow
    {
        private static readonly string BIRPAssetsGUID =
            "5f78b443fb7cb9f478c4b3da85b8d8e5";

        private static readonly string URPAssetsGUID =
            "aa3bab5027749a648aa738f1cd165bc2";

        private static readonly string HDRPAssetsGUID =
            "da115db3158e72c47ae5ad3850c0baa4";

        private static readonly string DiscordURL =
            "https://discord.gg/SbNs7zK";

        private static readonly string StoreURL =
            "https://assetstore.unity.com/publishers/707?aid=1011lPwI&pubref=AF3D";

        private static readonly string WindowTitle =
            "Prototype Collection - Street Props";

        private static readonly GUIContent SamplesTitle =
            new GUIContent(
                "Street Props",
                "Import samples according to your project rendering pipeline"
            );

        private const string DisplayDialogTitle =
            "Import Street Props";

        private const string DisplayDialogText =
            "This will import the samples for the selected pipeline, " +
            "please make sure the pipeline is properly installed and set " +
            "before importing the samples.\n\nContinue?";

        private static readonly GUIContent Body =
            new GUIContent(
                "Enjoy our free collection of Street Props. Join the discussion, tell us\n" +
                "how you used them and learn more about our other Assets."
            );

        [NonSerialized]
        private Texture m_packageIcon;

        [NonSerialized]
        private Texture m_webIcon;

        private GUIContent m_BIRPButton;
        private GUIContent m_HDRPButton;
        private GUIContent m_URPButton;

        private GUIContent m_DiscordButton;
        private GUIContent m_StoreButton;

        [NonSerialized]
        private GUIStyle m_buttonStyle;

        [NonSerialized]
        private GUIStyle m_labelStyle;

        [NonSerialized]
        private GUIStyle m_linkStyle;

        private struct Banner
        {
            public string desc;
            public string url;
            public string imageGUID;
            public Texture2D image;

            public Banner(
                string desc,
                string url,
                string imageGUID
            )
            {
                this.desc = desc;
                this.url = url;
                this.imageGUID = imageGUID;
                this.image = null;
            }
        }

        private static readonly Banner[] Banners =
        {
            new Banner(
                "Save Big with Amplify Bundle!",
                "https://assetstore.unity.com/packages/tools/visual-scripting/amplify-bundle-173849?aid=1011lPwI&pubref=AF3D",
                "2e6f99c42f698fb40b05c220d90becbc"
            ),

            new Banner(
                "Amplify Shader editor, award-winnning editor for any project.",
                "https://assetstore.unity.com/packages/tools/visual-scripting/amplify-shader-editor-68570?aid=1011lPwI&pubref=AF3D",
                "7d025e5606137cc4ebae0c93f43f677e"
            ),

            new Banner(
                "Amplify Impostors, 1-Click optimization.",
                "https://assetstore.unity.com/packages/tools/utilities/amplify-impostors-119877?aid=1011lPwI&pubref=AF3D",
                "46f890fe7bef64f46a7ad9146a043887"
            ),

            new Banner(
                "Amplify Animation Pack, 600+ Anims, Blender Source and Character Controller.",
                "https://assetstore.unity.com/packages/3d/animations/amplify-animation-pack-207911?aid=1011lPwI&pubref=AF3D",
                "7cd417e77a5dd954f81f4cbb93786186"
            )
        };

        [MenuItem(
            "Window/Prototype Collection/Street Props",
            false,
            1998
        )]
        public static void Init()
        {
            StartScreen window =
                GetWindow<StartScreen>(
                    true,
                    WindowTitle
                );

            window.minSize =
                new Vector2(
                    670f,
                    700f
                );

            window.maxSize =
                new Vector2(
                    670f,
                    700f
                );

            window.Show();
        }

        private void OnEnable()
        {
            if (m_packageIcon == null)
            {
                m_packageIcon =
                    EditorGUIUtility
                        .IconContent(
                            "BuildSettings.Editor.Small"
                        )
                        .image;

                m_BIRPButton =
                    new GUIContent(
                        " Built-In Assets",
                        m_packageIcon
                    );

                m_HDRPButton =
                    new GUIContent(
                        " HDRP Assets",
                        m_packageIcon
                    );

                m_URPButton =
                    new GUIContent(
                        " URP Assets",
                        m_packageIcon
                    );
            }

            if (m_webIcon == null)
            {
                m_webIcon =
                    EditorGUIUtility
                        .IconContent(
                            "BuildSettings.Web.Small"
                        )
                        .image;

                m_DiscordButton =
                    new GUIContent(
                        " Discord",
                        m_webIcon
                    );

                m_StoreButton =
                    new GUIContent(
                        " Asset Store",
                        m_webIcon
                    );
            }
        }

        private void OnGUI()
        {
            if (m_buttonStyle == null)
            {
                m_buttonStyle =
                    new GUIStyle(
                        GUI.skin.button
                    );

                m_buttonStyle.alignment =
                    TextAnchor.MiddleLeft;
            }

            if (m_labelStyle == null)
            {
                m_labelStyle =
                    new GUIStyle(
                        "BoldLabel"
                    );

                m_labelStyle.margin =
                    new RectOffset(
                        4,
                        4,
                        4,
                        4
                    );

                m_labelStyle.padding =
                    new RectOffset(
                        2,
                        2,
                        2,
                        2
                    );

                m_labelStyle.fontSize = 13;
            }

            if (m_linkStyle == null)
            {
                Texture2D transparentTexture =
                    AssetDatabase.LoadAssetAtPath<Texture2D>(
                        AssetDatabase.GUIDToAssetPath(
                            "1004d06b4b28f5943abdf2313a22790a"
                        )
                    );

                m_linkStyle =
                    new GUIStyle();

                m_linkStyle.normal.textColor =
                    new Color(
                        0.2980392f,
                        0.4901961f,
                        1f
                    );

                m_linkStyle.hover.textColor =
                    Color.white;

                m_linkStyle.active.textColor =
                    Color.grey;

                m_linkStyle.margin.top = 3;
                m_linkStyle.margin.bottom = 2;

                m_linkStyle.hover.background =
                    transparentTexture;

                m_linkStyle.active.background =
                    transparentTexture;
            }

            EditorGUILayout.BeginHorizontal(
                GUIStyle.none,
                GUILayout.ExpandWidth(true)
            );

            // LEFT COLUMN
            EditorGUILayout.BeginVertical(
                GUILayout.Width(175)
            );

            GUILayout.Label(
                SamplesTitle,
                m_labelStyle
            );

            if (GUILayout.Button(
                m_HDRPButton,
                m_buttonStyle
            ))
            {
                if (EditorUtility.DisplayDialog(
                    DisplayDialogTitle,
                    DisplayDialogText,
                    "Yes",
                    "No"
                ))
                {
                    AssetDatabase.ImportPackage(
                        AssetDatabase.GUIDToAssetPath(
                            HDRPAssetsGUID
                        ),
                        false
                    );
                }
            }

            if (GUILayout.Button(
                m_URPButton,
                m_buttonStyle
            ))
            {
                if (EditorUtility.DisplayDialog(
                    DisplayDialogTitle,
                    DisplayDialogText,
                    "Yes",
                    "No"
                ))
                {
                    AssetDatabase.ImportPackage(
                        AssetDatabase.GUIDToAssetPath(
                            URPAssetsGUID
                        ),
                        false
                    );
                }
            }

            if (GUILayout.Button(
                m_BIRPButton,
                m_buttonStyle
            ))
            {
                if (EditorUtility.DisplayDialog(
                    DisplayDialogTitle,
                    DisplayDialogText,
                    "Yes",
                    "No"
                ))
                {
                    AssetDatabase.ImportPackage(
                        AssetDatabase.GUIDToAssetPath(
                            BIRPAssetsGUID
                        ),
                        false
                    );
                }
            }

            EditorGUILayout.EndVertical();

            // RIGHT COLUMN
            EditorGUILayout.BeginVertical(
                GUILayout.Width(470),
                GUILayout.ExpandHeight(true)
            );

            GUILayout.Space(20);

            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical();

            GUILayout.Label(Body);

            GUILayout.Space(2);

            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            GUILayout.Space(13);

            GUILayout.BeginHorizontal(
                GUILayout.ExpandWidth(true)
            );

            if (GUILayout.Button(
                m_DiscordButton,
                GUILayout.ExpandWidth(true)
            ))
            {
                Application.OpenURL(
                    DiscordURL
                );
            }

            if (GUILayout.Button(
                m_StoreButton,
                GUILayout.ExpandWidth(true)
            ))
            {
                Application.OpenURL(
                    StoreURL
                );
            }

            GUILayout.EndHorizontal();

            GUILayout.Space(20);

            for (
                int i = 0;
                i < Banners.Length;
                i++
            )
            {
                GUILayout.Label(
                    Banners[i].desc
                );

                if (Banners[i].image == null)
                {
                    Banners[i].image =
                        AssetDatabase.LoadAssetAtPath<Texture2D>(
                            AssetDatabase.GUIDToAssetPath(
                                Banners[i].imageGUID
                            )
                        );
                }

                if (GUILayout.Button(
                    new GUIContent(
                        Banners[i].image
                    ),
                    m_linkStyle,
                    GUILayout.Width(470)
                ))
                {
                    Application.OpenURL(
                        Banners[i].url
                    );
                }

                GUILayout.Space(15);
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
        }
    }
}