using System.Text.Json.Nodes;

namespace RPGPlatformer.Saving
{
    public interface ISavable
    {
        public JsonNode CaptureState();

        public void RestoreState(JsonNode jNode);
    }
}