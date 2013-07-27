using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml;

namespace PWTranslator.Helpers {
    public class YandexTranslator {
        private static string APIKey = "trnsl.1.1.20130727T162804Z.a3fb6a4f4d678896.885f78807e8afb4bf97fe3938eae4ccf3ef7b2cb";
        public static string Detect(string text) {
            var request = WebRequest.Create("http://translate.yandex.net/api/v1/tr/detect?text=" + text);
            var response = request.GetResponse();

            using (var sr = new StreamReader(response.GetResponseStream())) {
                var fetchedXml = sr.ReadToEnd();

                var d = new XmlDocument();
                d.LoadXml(fetchedXml);

                var langNodes = d.GetElementsByTagName("DetectedLang");
                var node = langNodes.Item(0);

                return node.Attributes[1].Value;
            }
        }

        public static List<string> GetLangs() {
            var url = string.Format(@"https://translate.yandex.net/api/v1.5/tr/getLangs?key={0}&ui=ru", APIKey);
            var request = WebRequest.Create(url);
            var response = request.GetResponse();

            using (var sr = new StreamReader(response.GetResponseStream())) {
                var fetchedXml = sr.ReadToEnd();

                var d = new XmlDocument();
                d.LoadXml(fetchedXml);
                var trDirectionNodes = d.GetElementsByTagName("string");

                return (from XmlNode trDirectionNode in trDirectionNodes select trDirectionNode.InnerText).ToList();
            }
        }

        public static string Translate(string lang, string text) {
            var url = string.Format(@"https://translate.yandex.net/api/v1.5/tr/translate?key={0}&lang=en-ru&text={1}", APIKey, Uri.EscapeDataString(text));
            var request = WebRequest.Create(url);
            var response = request.GetResponse();

            using (var sr = new StreamReader(response.GetResponseStream())) {
                var fetchedXml = sr.ReadToEnd();

                var d = new XmlDocument();
                d.LoadXml(fetchedXml);
                var textNodes = d.GetElementsByTagName("text");
                var result = "";
                foreach (XmlNode textNode in textNodes)
                    result = string.Format("{0}\n{1}", result, textNode.InnerText);

                return result.Replace("\n", "");
            }
        }
    }
}
