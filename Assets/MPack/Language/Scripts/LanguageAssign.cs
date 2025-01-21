using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MPack;


namespace MPack
{
    public class LanguageAssign : MonoBehaviour
    {
        public static LanguageAssign ins;
        private static int s_currentIndex = 0;

        [SerializeField]
        private bool destroyDuplicatedGameObject = false;
        [SerializeField]
        private bool dontDestroyOnLoad = false;
        [SerializeField]
        private bool loopIndex = true;
        [SerializeField]
        private LanguageData[] languages;

        void Awake()
        {
            if (ins)
            {
                if (destroyDuplicatedGameObject)
                    Destroy(gameObject);
                else
                    Destroy(this);
                return;
            }

            ins = this;
            LanguageMgr.AssignLanguageData(languages[s_currentIndex]);

            if (dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        public void NextLanguage()
        {
            if (++s_currentIndex >= languages.Length)
            {
                if (loopIndex)
                    s_currentIndex = 0;
                else
                    s_currentIndex = languages.Length - 1;
            }

            LanguageMgr.AssignLanguageData(languages[s_currentIndex]);
        }

        public void PreviousLanguage()
        {
            if (--s_currentIndex < 0)
            {
                if (loopIndex)
                    s_currentIndex = languages.Length - 1;
                else
                    s_currentIndex = 0;
            }

            LanguageMgr.AssignLanguageData(languages[s_currentIndex]);
        }


        public void SetLanguageByID(int ID)
        {
            for (int i = 0; i < languages.Length; i++)
            {
                if (languages[i].ID == ID)
                {
                    s_currentIndex = i;
                    LanguageMgr.AssignLanguageData(languages[s_currentIndex]);
                    return;
                }
            }

            Debug.LogError("Language ID not found");
        }

        public void SetLanguageByIndex(int index)
        {
            if (index < 0 || index >= languages.Length)
            {
                Debug.LogError("Language index out of range");
                return;
            }

            s_currentIndex = index;
            LanguageMgr.AssignLanguageData(languages[s_currentIndex]);
        }
    }
}