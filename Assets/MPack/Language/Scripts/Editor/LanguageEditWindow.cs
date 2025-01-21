using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Graphs;

namespace MPack
{
    public class LanguageEditWindow : EditorWindow
    {
        private const string Alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string Number = "0123456789";
        private const string Symbol = "!\"#$%^'()*+,-./:;<=>?@[]\\^_`{}|~";

        private const float LabelWidth = 50, LanguageWidth = 400;

        static private Color IDLabelBGColor = EditorUtilities.From256Color(75, 75, 75);
        static private Color LineColor = EditorUtilities.From256Color(65, 65, 65);

        private bool addAlphabet = true, addNumber = true, addSymbol = true;

        [MenuItem("Window/MPack/Language Editor")]
        [MenuItem("Tools/MPack/Language Editor")]
        static private void OpenEditorWindow()
        {
            GetWindow<LanguageEditWindow>("Language Editor");
        }

        private Vector2 scrollViewPos = Vector3.zero;
        private LanguageData[] m_languages;
        private string[] m_languageNames;
        private int[] m_languageDataIDs;
        private int[] m_languageDataIDsFiltered;
        private List<int> m_dataIDTranslationFetching = new List<int>();

        private int m_displayLanguage = int.MaxValue;

        private GUIStyle m_scrollBarStyle;
        private GUIStyle m_headerStyle;

        private string m_searchText = "";

        private void OnEnable()
        {
            string[] files = AssetDatabase.FindAssets("t:LanguageData");
            m_languages = new LanguageData[files.Length];

            List<int> dataIdList = new List<int>();
            for (int i = 0; i < files.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(files[i]);
                LanguageData data = AssetDatabase.LoadAssetAtPath<LanguageData>(path);
                m_languages[i] = data;

                for (int j = 0; j < data.Texts.Length; j++)
                {
                    if (!dataIdList.Contains(data.Texts[j].ID))
                        dataIdList.Add(data.Texts[j].ID);
                }
            }

            Array.Sort(m_languages, (data1, data2) => data1.ID - data2.ID);
            m_languageNames = new string[m_languages.Length];
            for (int i = 0; i < m_languages.Length; i++)
            {
                m_languageNames[i] = m_languages[i].name;
            }

            dataIdList.Sort();
            m_languageDataIDs = dataIdList.ToArray();

            m_scrollBarStyle = new GUIStyle();
            m_scrollBarStyle.normal.textColor = Color.white;

            m_headerStyle = new GUIStyle();
            m_headerStyle.fontSize = 18;
            m_headerStyle.normal.textColor = Color.white;
        }

        private void ScanTextInEveryLanguageData()
        {
            List<char> allChars = new List<char>();
            if (addAlphabet) allChars.AddRange(Alphabet.ToCharArray());
            if (addNumber) allChars.AddRange(Number.ToCharArray());
            if (addSymbol) allChars.AddRange(Symbol.ToCharArray());

            for (int i = 0; i < m_languages.Length; i++)
            {
                for (int j = 0; j < m_languages[i].Texts.Length; j++)
                {
                    char[] chars = m_languages[i].Texts[j].Text.ToCharArray();
                    foreach (char chr in chars)
                    {
                        if (!allChars.Contains(chr)) allChars.Add(chr);
                    }
                }
            }

            string allCharString = new string(allChars.ToArray());
            Debug.Log(allCharString);
        }

        private void DrawRow(int ID, float width)
        {
            int[] textIndex = new int[m_languages.Length];
            int lineCount = 0;
            bool canAutoTranslate = false;

            // Find out all the text is link to this id
            for (int i = 0; i < m_languages.Length; i++)
            {
                var language = m_languages[i];
                bool assigned = false;

                for (int j = 0; j < language.Texts.Length; j++)
                {
                    var pair = language.Texts[j];

                    if (pair.ID == ID)
                    {
                        assigned = true;
                        textIndex[i] = j;

                        if (!language.IgnoreTranslation && pair.Text == "")
                            canAutoTranslate = true;

                        int count = m_languages[i].Texts[j].Text.Split(
                            new string[] { "\n" }, System.StringSplitOptions.None).Length;

                        if (count > lineCount) lineCount = count;
                        break;
                    }
                }

                if (!assigned)
                {
                    Array.Resize(ref m_languages[i].Texts, m_languages[i].Texts.Length + 1);
                    int last = m_languages[i].Texts.Length - 1;
                    m_languages[i].Texts[last].ID = ID;
                    m_languages[i].Texts[last].Text = "";
                    textIndex[i] = last;
                    canAutoTranslate = true;
                }
            }

            Rect rowRect = EditorGUILayout.GetControlRect(false, (lineCount * 15) + 10, GUILayout.Width(width));
            // EditorGUILayout.EndHorizontal();

            Rect labelRect = rowRect;
            labelRect.width = LabelWidth;
            labelRect.height -= 3;
            labelRect.y++;

            EditorGUI.DrawRect(labelRect, IDLabelBGColor);
            
            
            // Debug.Log(canAutoTranslate);
            if (canAutoTranslate)
            {
                if (m_dataIDTranslationFetching.Contains(ID))
                    GUI.Label(labelRect, "Fecthing");
                else if (GUI.Button(labelRect, ID.ToString()))
                    DoAutoTranslate(ID);
            }
            else GUI.Label(labelRect, ID.ToString());


            labelRect.x += LabelWidth + 2;
            labelRect.width = LanguageWidth;

            for (int i = 0; i < m_languages.Length; i++)
            {
                int power = Mathf.RoundToInt(Mathf.Pow(2, i));

                if (m_displayLanguage == -1 || ((power & m_displayLanguage) == power))
                {
                    if (textIndex[i] != -1)
                    {
                        EditorGUI.BeginChangeCheck();
                        m_languages[i].Texts[textIndex[i]].Text = EditorGUI.TextArea(labelRect, m_languages[i].Texts[textIndex[i]].Text);
                        if (EditorGUI.EndChangeCheck())
                            EditorUtility.SetDirty(m_languages[i]);
                    }
                    labelRect.x += labelRect.width + 2;
                }
            }

            Rect hrRect = rowRect;
            hrRect.y += rowRect.height - 1;
            hrRect.height = 1;

            EditorGUI.DrawRect(hrRect, LineColor);

            // EditorGUILayout.BeginHorizontal();
            // EditorGUILayout.LabelField("", GUILayout.Width(rowRect.width));
            // EditorGUILayout.EndHorizontal();
        }

        private void DrawHeaderRow(float width)
        {
            Rect rowRect = EditorGUILayout.GetControlRect(false, 30, GUILayout.Width(width));

            Rect labelRect = rowRect;
            labelRect.x += LabelWidth + (LanguageWidth / 2) - 40;
            labelRect.y++;
            labelRect.width = LanguageWidth;
            labelRect.height -= 3;


            for (int i = 0; i < m_languageNames.Length; i++)
            {
                int power = Mathf.RoundToInt(Mathf.Pow(2, i));

                if (m_displayLanguage == -1 || ((power & m_displayLanguage) == power))
                {
                    EditorGUI.LabelField(labelRect, m_languageNames[i], m_headerStyle);
                    labelRect.x += labelRect.width + 2;
                }
            }

            Rect hrRect = rowRect;
            hrRect.y += rowRect.height - 1;
            hrRect.height = 1;

            EditorGUI.DrawRect(hrRect, LineColor);
        }


        // private bool _textMeshProScan = false;
        private void OnGUI()
        {

            EditorGUILayout.BeginHorizontal();
            m_displayLanguage = EditorGUILayout.MaskField(
                "Display Language", m_displayLanguage, m_languageNames);

            float width = LabelWidth + 2;
            for (int i = 0; i < m_languages.Length; i++)
            {
                int power = Mathf.RoundToInt(Mathf.Pow(2, i));
                if (m_displayLanguage == -1 || ((power & m_displayLanguage) == power))
                {
                    width += LanguageWidth + 2;
                }
            }

            DrawSearchBar();
            EditorGUILayout.EndHorizontal();

            DrawScanText();

            EditorGUILayout.Space(20);

            EditorGUILayout.BeginScrollView(
                new Vector2(scrollViewPos.x, 0),
                false,
                false,
                m_scrollBarStyle,
                m_scrollBarStyle,
                m_scrollBarStyle,
                GUILayout.Height(30));
            DrawHeaderRow(width);
            EditorGUILayout.EndScrollView();

            scrollViewPos = EditorGUILayout.BeginScrollView(scrollViewPos, false, false);

            if (m_searchText != "")
            {
                for (int i = 0; i < m_languageDataIDsFiltered.Length; i++)
                    DrawRow(m_languageDataIDs[m_languageDataIDsFiltered[i]], width);
            }
            else
            {
                for (int i = 0; i < m_languageDataIDs.Length; i++)
                    DrawRow(m_languageDataIDs[i], width);
            }

            EditorGUILayout.EndScrollView();
        }


        void DrawScanText()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Scan Characters");
            addAlphabet = EditorGUILayout.Toggle("Include alphabet", addAlphabet);
            addNumber = EditorGUILayout.Toggle("Include number", addNumber);
            addSymbol = EditorGUILayout.Toggle("Include symbol", addSymbol);
            if (GUILayout.Button("Scan Text in Every LanguageData"))
            {
                ScanTextInEveryLanguageData();
            }
            EditorGUILayout.EndHorizontal();
        }


        void DrawSearchBar()
        {
            EditorGUI.BeginChangeCheck();
            m_searchText = EditorGUILayout.TextField("Search", m_searchText);

            if (EditorGUI.EndChangeCheck())
            {
                FilterLanguageDataIDBySearch();
            }
        }

        void FilterLanguageDataIDBySearch()
        {
            m_searchText = m_searchText.ToLower();

            List<int> filtered = new List<int>();
            for (int i = 0; i < m_languageDataIDs.Length; i++) {
                int id = m_languageDataIDs[i];
                string text = id.ToString();
                if (text.Contains(m_searchText))
                    filtered.Add(i);
            }

            int index = 0;
            for (int i = 0; i < m_languages.Length; i++)
            {
                var language = m_languages[i];

                for (int j = 0; j < language.Texts.Length; j++)
                {
                    if (language.Texts[j].Text.ToLower().Contains(m_searchText))
                    {
                        index = LanguageIDIndex(language.Texts[j].ID);
                        if (!filtered.Contains(index))
                            filtered.Add((index));
                    }
                }
            }

            m_languageDataIDsFiltered = filtered.ToArray();
        }

        int LanguageIDIndex(int languageID)
        {
            for (int i = 0; i < m_languageDataIDs.Length; i++)
            {
                if (m_languageDataIDs[i] == languageID)
                    return i;
            }
            return -1;
        }


        async void DoAutoTranslate(int languageID)
        {
            m_dataIDTranslationFetching.Add(languageID);

            string originalLanguage = "";
            string toLanguage = "";
            int toLanguageIndex = 0;
            int textIndex = 0;

            // Find Original language and the language to translate to
            for (int i = 0; i < m_languages.Length; i++) {
                var language = m_languages[i];
                if (language.IgnoreTranslation)
                    continue;

                for (int e = 0; e < language.Texts.Length; e++)
                {
                    var pair = m_languages[i].Texts[e];
                    if (pair.ID != languageID)
                        continue;

                    if (pair.Text != "")
                    {
                        originalLanguage = pair.Text;
                        break;
                    }
                    else
                    {
                        toLanguage = m_languages[i].name;
                        toLanguageIndex = i;
                        textIndex = e;
                        break;
                    }
                }
            }

            Repaint();
            Debug.Log($"Original Language: {originalLanguage}, To Language: {toLanguage}");

            ChatGPTRequest.ResponseJSON response = await ChatGPTRequest.Translate(toLanguage, originalLanguage);

            if (response.error.message != null)
            {
                Debug.Log(response.error.message);
                EditorUtility.DisplayDialog("GPT Error", "check console", "OK");
                m_dataIDTranslationFetching.Remove(languageID);
                Repaint();
                return;
            }

            string content = response.choices[0].message.content;

            content = content.TrimStart('"');
            content = content.TrimEnd('"');

            m_languages[toLanguageIndex].Texts[textIndex].Text = content;
            Repaint();

            m_dataIDTranslationFetching.Remove(languageID);
        }
    }

    internal class EditorUtilities : MonoBehaviour
    {
        /// <summary>
        /// Tanslate 0..255 to 0..1
        /// </summary>
        /// <param name="r">Red [0..255]</param>
        /// <param name="g">Green [0..255]</param>
        /// <param name="b">Blue [0..255]</param>
        /// <param name="a">Alpha [0..255]</param>
        /// <returns></returns>
        public static Color From256Color(float r, float g, float b, float a=255) {
            return new Color(
                r / 255 > 255 ? 1: r / 255,
                g / 255 > 255 ? 1: g / 255,
                b / 255 > 255 ? 1: b / 255,
                a / 255 > 255 ? 1 : a / 255
            );
        }
    }
}