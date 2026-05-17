namespace ChromaJigsaw.Core
{
    public static class BundleConfig
    {
        private const string BaseUrl = "https://chromalogic.aegisnet.org.uk/bundles";

        public static string CatalogueUrl          => $"{BaseUrl}/catalogue.json";
        public static string PackBundleUrl(string packId) => $"{BaseUrl}/{packId}/{packId}.bundle";
        public static string SouvenirUrl(string packId, string imageId) => $"{BaseUrl}/{packId}/souvenir/{imageId}_4k.jpg";
    }
}
