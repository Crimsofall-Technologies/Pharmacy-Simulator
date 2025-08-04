using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AdvancedPeopleSystem
{
    public class EditorHelpMenu : EditorWindow
    {
        GUIStyle labelStyle;
        GUIStyle label2Style;
        GUIStyle linkStyle;
        GUIStyle defaultText;
        [MenuItem("APPack 2.8.1/Help")]
        public static void ShowWindow()
        {
            var window = GetWindow<EditorHelpMenu>(true, "Advanced People Pack 2.8.12", true);
            window.minSize = new Vector2(450, 245);
            window.maxSize = new Vector2(450, 245);
            window.position = new Rect(new Vector2(Screen.width / 2 - 225, Screen.height / 2 - 112), window.maxSize);
            window.ShowPopup();
        }

        private void OnGUI()
        {
            if(labelStyle == null)
            {
                labelStyle = new GUIStyle(GUI.skin.label);
                labelStyle.alignment = TextAnchor.MiddleCenter;
                labelStyle.fontSize = 18;
                labelStyle.richText = true;
            }
            if (label2Style == null)
            {
                label2Style = new GUIStyle(GUI.skin.label);
                label2Style.richText = true;
                label2Style.alignment = TextAnchor.MiddleLeft;
                label2Style.fontSize = 14;
            }
            if (linkStyle == null)
            {
                linkStyle = new GUIStyle(GUI.skin.label);
                linkStyle.richText = true;
                linkStyle.fontSize = 13;
                linkStyle.alignment = TextAnchor.MiddleLeft;
                linkStyle.margin.right = 100;
                linkStyle.fontStyle = FontStyle.Italic;

            }
            if (defaultText == null)
            {
                defaultText = new GUIStyle(GUI.skin.label);
                defaultText.richText = true;
                defaultText.fontSize = 13;
                defaultText.alignment = TextAnchor.MiddleLeft;
                defaultText.margin.left = 20;
                defaultText.fixedWidth = 200;
            }

            EditorGUILayout.BeginVertical();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("<color=#721aff><b>Advanced People Pack</b></color>", labelStyle);

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(20);
            EditorGUILayout.LabelField("Version: <b>2.8.12</b>", label2Style);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            GUILayout.BeginVertical("GroupBox");
            GUILayout.BeginHorizontal();
            GUILayout.Label("- Online Documentation: ", defaultText);
            LinkButton("Go To Site", "https://alexlenk.com/docs/app2-doc/");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("- Write a Review: ", defaultText);
            LinkButton("Go To Assets Store", "https://assetstore.unity.com/packages/slug/170756");
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(20);
            EditorGUILayout.LabelField("Contacts ↓", label2Style);
            EditorGUILayout.EndHorizontal();

            GUILayout.BeginVertical("GroupBox");
            GUILayout.BeginHorizontal();
            GUILayout.Label("- E-Mail: ", defaultText);
            LinkButton("contact@alexlenk.com", "mailto:contact@alexlenk.com");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("- Discord: ", defaultText);
            LinkButton("Accept invitation", "https://discordapp.com/invite/U26sFp4");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("- Site: ", defaultText);
            LinkButton("alexlenk.com", "https://alexlenk.com/");
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        private void LinkButton(string caption, string url)
        {
            if (EditorGUIUtility.isProSkin)
                caption = string.Format("<color=#4a6ed2><b>{0}</b></color>", caption);
            else
                caption = string.Format("<color=#628bfb><b>{0}</b></color>", caption);
            bool bClicked = GUILayout.Button(caption, linkStyle);
            var rect = GUILayoutUtility.GetLastRect();
            rect.width = linkStyle.CalcSize(new GUIContent(caption)).x;
            EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
            if (bClicked)
                Application.OpenURL(url);
        }
    }
}