using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MPack
{
    public class CompositeLanguageText : TextBehaviour
    {
        [SerializeField]
        private string format;
        [SerializeField]
        [LanguageID]
        private int[] idList;

        public void ChangeIds(int[] _idList, bool forceRefresh = false)
        {
            if (!initialed) InitialGetTextComponent();

            if (idList != _idList || forceRefresh)
            {
                idList = _idList;

                if (LanguageMgr.DataLoaded)
                    SetText(GetTexts());
            }
        }

        public void ChangeId(int idIndex, int id, bool forceRefresh=false)
        {
            if (!initialed) InitialGetTextComponent();

            if (idList[idIndex] != id || forceRefresh)
            {
                idList[idIndex] = id;

                if (LanguageMgr.DataLoaded)
                    SetText(GetTexts());
            }
        }

        private void Start()
        {
            if (LanguageMgr.DataLoaded)
				SetText(GetTexts());
        }

		private void OnEnable()
		{
            if (LanguageMgr.DataLoaded)
				SetText(GetTexts());
		}

		public void Reload()
		{
            if (LanguageMgr.DataLoaded)
				SetText(GetTexts());
		}


        string GetTexts()
        {
            var texts = new string[idList.Length];
            for (int i = 0; i < idList.Length; i++)
            {
                texts[i] = LanguageMgr.GetTextById(idList[i]);
            }

            return string.Format(format, texts);
        }
    }
}