using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MPack;


namespace MPack
{
    public class TestLanguage : MonoBehaviour
    {
        [SerializeField, LanguageID]
        private int languageID;
        [SerializeField, LanguageID]
        private int[] languageIDs;


        // [SerializeField]
        // private LanguageText languageText;

        // void Start()
        // {
        //     languageText.ChangeId(languageID);
        // }
    }
}