using System.Text.Json.Nodes;

namespace RPGPlatformer.Saving
{
    public interface ISavable
    {
        public JsonNode CaptureState();

        void RestoreState(JsonNode jNode);
    }
}