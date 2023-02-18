namespace Genesis {

    public enum PromptType { 
        FantasyLandscape,
        AnimeArtStyle,
        SurealStyle,
        DigitalPainting,
        Scenic,
        Nebula,
        Realistic,
        SciFi,
        Fractal,
        Advanced
    }

    public class SkyboxPrompt {

        private string _query;
        private PromptType _type;

        public SkyboxPrompt(string query, PromptType type) {
            _query = query;
            _type = type;
        }

        public string CreatePromptString() {
            string prompt = ToFormattableString(_type);
            return string.Format(prompt, _query);
        }

        private static string ToFormattableString(PromptType type) {
            switch (type) {
                case PromptType.FantasyLandscape:
                    return "detailed digital painting, c4d computer render, (fantasy VR360 dreamscape) {0}, cinematic lighting, detailed retro (VR360 fantasy concept art illustration), artstation";
                case PromptType.AnimeArtStyle:
                case PromptType.SurealStyle:
                case PromptType.DigitalPainting:
                case PromptType.Scenic:
                case PromptType.Nebula:
                case PromptType.Realistic:
                case PromptType.SciFi:
                case PromptType.Fractal:
                case PromptType.Advanced:
                default:
                    return "{0}";
            }
        }
    }
}