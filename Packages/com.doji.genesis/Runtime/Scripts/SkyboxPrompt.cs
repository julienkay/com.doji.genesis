using Newtonsoft.Json;
using System;

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
        Dreamlike,
        Advanced
    }

    public struct SkyboxPrompt {

        private string _query;
        private PromptType _type;

        public SkyboxPrompt(string query, PromptType type) {
            _query = query;
            _type = type;
        }

        private string CreatePromptString() {
            string prompt = ToFormattableString(_type);
            return string.Format(prompt, _query);
        }

        private static string ToFormattableString(PromptType type) {
            switch (type) {
                case PromptType.FantasyLandscape:
                    return "detailed digital painting, c4d computer render, (fantasy VR360 dreamscape) {0}, cinematic lighting, detailed retro (VR360 fantasy concept art illustration), artstation";
                case PromptType.AnimeArtStyle:
                    return "beautiful anime illustration(, VR360), view of a {0}, cinematic lighting, illustrated by studio ghibli(, VR360), pixiv, Miyazaki style, 8k anime style art(cel shading, VR360)";
                case PromptType.SurealStyle:
                    return "stunning beautiful surrealistic VR360 digital painting, (smooth 8k illustration, VR360), view of {0} , detailed fantasy matte painting, VR360, artstation, pixiv, game art, VR360";
                case PromptType.DigitalPainting:
                    return "digital art, detailed digital VR360 painting, {0}, cinematic lighting, detailed illustration, artstation, VR360";
                case PromptType.Scenic:
                    return "stunningly beautiful 8k VR360 digital painting, smooth fantasy {0}, volumetric lighting, pixiv, artstation, (smooth c4d VR360 render illustration), national geographic photography, (redshift render, photorealism, 8k, VR360)";
                case PromptType.Nebula:
                    return "stunning beautiful 8k astronomical VR360 illustration, deep space, view of a {0} nebula, deep space, volumetric lighting, artstation, smooth VR360 render illustration, astro photography, (detailed photorealism, 8k, VR360)";
                case PromptType.Realistic:
                    return "stunning professional 8k digital photo, (photorealism art, VR360), view of {0}, shot on 35mm film, volumetric lighting, equirectangular VR360 photograph, hyperrealism illustration, VR360";
                case PromptType.SciFi:
                    return "stunning 8k digital scifi illustration, (photorealistic c4d VR360 render), view of {0} , unreal engine, volumetric lighting, detailed VR360 scifi art, futuristic equirectangular, VR360";
                case PromptType.Dreamlike:
                    return "beautiful 8k VR360 digital painting, smooth geometric scifi, VR360 fractal art, view of a {0} four dimensional fractal, detailed futuristic VR360 fractal sculpture, volumetric lighting, highly detailed, artstation, pixiv, smooth cinema4d";
                case PromptType.Advanced:
                    return "(VR360) {0} (VR360)";
                default:
                    return "{0}";
            }
        }
        public class PromptConverter : JsonConverter {
            public override bool CanConvert(Type objectType) {
                return objectType == typeof(SkyboxPrompt);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
                throw new NotImplementedException();
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
                var prompt = (SkyboxPrompt)value;

                writer.WriteStartObject();
                writer.WritePropertyName("prompt");
                serializer.Serialize(writer, prompt.CreatePromptString());
            }
        }
    }
}