using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MPack {
    public class LanguageText : TextBehaviour
    {
		[SerializeField, LanguageID]
		private int id = -1;
		public int ID => id;


		/// <summary>
		/// Change the Language id of text, will automatically apply language after id changed
		/// </summary>
		/// <param name="_id">The new Id</param>
		public void ChangeId(int _id, bool forceRefresh=false) {
            if (!initialed) InitialGetTextComponent();

			if (id != _id || forceRefresh) {
				id = _id;

				if (LanguageMgr.DataLoaded)
					SetText(LanguageMgr.GetTextById(id));
			}
		}

        protected void Start()
        {
            if (LanguageMgr.DataLoaded)
				SetText(LanguageMgr.GetTextById(id));
        }

		protected void OnEnable()
		{
            if (LanguageMgr.DataLoaded)
				SetText(LanguageMgr.GetTextById(id));
		}

		public void Reload()
		{
            if (LanguageMgr.DataLoaded)
				SetText(LanguageMgr.GetTextById(id));
		}
    }
}