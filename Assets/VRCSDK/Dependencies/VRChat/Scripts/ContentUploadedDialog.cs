using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using VRC.Core;
#if UNITY_EDITOR
using UnityEditor;

namespace VRCSDK2
{
    public class ContentUploadedDialog : EditorWindow
    {
        private Texture2D clIconGraphic = null;
        private Color dialogTextColor = Color.white;
        private string contentUrl = null;

        private void OnEnable()
        {
            if(EditorGUIUtility.isProSkin)
                dialogTextColor = Color.white;
            else
                dialogTextColor = Color.black;

            clIconGraphic = Resources.Load("vrcSdkClDialogNewIcon") as Texture2D;
        }

        public void setContentURL(string url = null)
        {
            contentUrl = url;
        }

        void OnGUI()
        {
            const int CONTENT_UPLOADED_BORDER_SIZE = 20;
            const int CONTENT_UPLOADED_HORIZONTAL_SPACING = 10;

            const int CONTENT_UPLOADED_BUTTON_WIDTH = 260;
            const int CONTENT_UPLOADED_BUTTON_HEIGHT = 40;
            const int CONTENT_CL_VERTICAL_HEADER_SPACING = 40;

            const int CONTENT_CL_TEXT_REGION_HEIGHT = 120;

            const int CONTENT_MIN_WINDOW_WIDTH = (CONTENT_UPLOADED_BUTTON_WIDTH * 2) + CONTENT_UPLOADED_HORIZONTAL_SPACING + (CONTENT_UPLOADED_BORDER_SIZE * 2);
            const int CONTENT_MIN_WINDOW_HEIGHT = CONTENT_UPLOADED_BUTTON_HEIGHT + CONTENT_CL_VERTICAL_HEADER_SPACING + CONTENT_CL_TEXT_REGION_HEIGHT + (CONTENT_UPLOADED_BORDER_SIZE * 2);

            GUILayout.BeginHorizontal();
            GUILayout.Space(CONTENT_UPLOADED_BORDER_SIZE);

            // Community Labs graphic
            if (RuntimeWorldCreation.IsCurrentWorldInCommunityLabs && (null != clIconGraphic))
            {
                GUILayout.Label(new GUIContent(clIconGraphic), GUIStyle.none);
            }

            this.minSize = new Vector2(CONTENT_MIN_WINDOW_WIDTH, CONTENT_MIN_WINDOW_HEIGHT);

            GUILayout.BeginVertical();
            if (RuntimeWorldCreation.IsCurrentWorldInCommunityLabs && (null != clIconGraphic))
                GUILayout.Space(CONTENT_CL_VERTICAL_HEADER_SPACING);
            GUIStyle uploadedTitleStyle = new GUIStyle(EditorStyles.boldLabel);
            uploadedTitleStyle.normal.textColor = dialogTextColor;
            uploadedTitleStyle.fontSize = 15;
            GUILayout.Label(CommunityLabsConstants.UPLOADED_CONTENT_SUCCESSFULLY_MESSAGE, uploadedTitleStyle);

            string uploadedMessage = CommunityLabsConstants.UPLOADED_NEW_PRIVATE_WORLD_CONFIRMATION_MESSAGE;

            if (!RuntimeWorldCreation.IsCurrentWorldUploaded)
            {
                if (RuntimeWorldCreation.IsCurrentWorldInCommunityLabs)
                    uploadedMessage = CommunityLabsConstants.PUBLISHED_WORLD_TO_COMMUNITY_LABS_CONFIRMATION_MESSAGE;
                else
                    uploadedMessage = CommunityLabsConstants.UPLOADED_NEW_PRIVATE_WORLD_CONFIRMATION_MESSAGE;
            }
            else
            {
                if (RuntimeWorldCreation.IsCurrentWorldInCommunityLabs)
                {
                    uploadedMessage = CommunityLabsConstants.UPDATED_COMMUNITY_LABS_WORLD_CONFIRMATION_MESSAGE;
                }
                else
                {
                    if (RuntimeWorldCreation.IsCurrentWorldPubliclyPublished)
                        uploadedMessage = CommunityLabsConstants.UPDATED_PUBLIC_WORLD_CONFIRMATION_MESSAGE;
                    else
                        uploadedMessage = CommunityLabsConstants.UPDATED_PRIVATE_WORLD_CONFIRMATION_MESSAGE;
                }
            }

            GUIStyle uploadedMessageStyle = new GUIStyle(EditorStyles.label);
            uploadedMessageStyle.normal.textColor = dialogTextColor;
            uploadedMessageStyle.fontSize = 13;
            uploadedMessageStyle.wordWrap = true;
            GUILayout.Label(uploadedMessage, uploadedMessageStyle);
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal();

            GUILayout.Space(CONTENT_UPLOADED_BORDER_SIZE);

            if (RuntimeWorldCreation.IsCurrentWorldInCommunityLabs)
            {
                if (GUILayout.Button(CommunityLabsConstants.READ_COMMUNITY_LABS_DOCS_STRING, GUILayout.Width(CONTENT_UPLOADED_BUTTON_WIDTH), GUILayout.Height(CONTENT_UPLOADED_BUTTON_HEIGHT)))
                {
                    Application.OpenURL(CommunityLabsConstants.COMMUNITY_LABS_DOCUMENTATION_URL);
                }
            }

            GUILayout.FlexibleSpace();

            if (GUILayout.Button(CommunityLabsConstants.MANAGE_WORLD_IN_BROWSER_STRING, GUILayout.Width(CONTENT_UPLOADED_BUTTON_WIDTH), GUILayout.Height(CONTENT_UPLOADED_BUTTON_HEIGHT)))
            {
                Application.OpenURL(contentUrl);
            }

            if (RuntimeWorldCreation.IsCurrentWorldInCommunityLabs)
                GUILayout.Space(CONTENT_UPLOADED_BORDER_SIZE);
            else
                GUILayout.FlexibleSpace();

            GUILayout.EndHorizontal();

            GUILayout.Space(CONTENT_UPLOADED_BORDER_SIZE);
        }
    }
}
#endif
