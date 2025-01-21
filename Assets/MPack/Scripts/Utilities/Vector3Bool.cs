namespace MPack
{
    [System.Serializable]
    public struct Vector3Bool
    {
        public bool x, y, z;

        public bool Any() => x || y || z;
        public bool All() => x && y && z;
    }
}