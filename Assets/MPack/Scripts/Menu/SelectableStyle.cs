using UnityEngine;

namespace MPack {
	[CreateAssetMenu(menuName="MPack/Selectable Style")]
	public class SelectableStyle : ScriptableObject {
		public Color NormalColor;
		public Color ActiveColor;
		public Color SelectedColor;
		public Color DisabledColor;
        public Material NormalMaterial;
		public Material SelectedMaterial;
	}
}