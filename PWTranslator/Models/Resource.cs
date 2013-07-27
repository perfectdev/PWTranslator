using System.Xml;

namespace PWTranslator.Models {
    public class Resource {
        public XmlAttribute XmlAttribute { get; set; }
        public string Path { get; set; }
        public string OriginalValue { get; set; }
        public string NewValue { get; set; }
    }
}
