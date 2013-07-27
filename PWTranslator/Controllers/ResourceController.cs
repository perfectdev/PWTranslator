using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml;
using PWTranslator.Helpers;
using PWTranslator.Models;

namespace PWTranslator.Controllers {
    public class ResourceController {
        private readonly string _file;
        public XmlDocument XmlDoc { get; private set; }
        public List<Resource> Resources { get; set; }

        public ResourceController(string file) {
            _file = file;
            Resources = new List<Resource>();
        }

        public static ResourceController Create(string file) {
            var self = new ResourceController(file);
            self.ExtractResources();
            return self;
        }

        public void ExtractResources() {
            try {
                using (var fs = File.OpenRead(_file)) {
                    XmlDoc = new XmlDocument {PreserveWhitespace = true};
                    using (var bReader = new BinaryReader(fs)) {
                        var b = bReader.ReadByte();
                        var garbageExists = b != 0x3C;
                        fs.Seek(garbageExists ? 2 : 0, SeekOrigin.Begin);
                        XmlDoc.Load(fs);
                    }
                    fs.Close();
                }
                ReadNode(XmlDoc.DocumentElement);
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message, "Parse Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ReadNode(XmlNode node) {
            if (node != null && node.Attributes != null) {
                foreach (var attribute in node.Attributes.Cast<XmlAttribute>().Where(t => t.Name.ToLower() == "string")) {
                    Resources.Add(new Resource {XmlAttribute = attribute, Path = GetPath(attribute), OriginalValue = attribute.Value });
                }
            }
            if (node == null) return;
            foreach (XmlNode subNode in node.ChildNodes) {
                ReadNode(subNode);
            }
        }

        private string GetPath(XmlAttribute attr) {
            var path = "";
            XmlNode node = attr.OwnerElement;
            while (true) {
                if (node == null || node.Name == "#document") break;
                path = string.Format(@"{0}\{1}", node.Name, path);
                node = node.ParentNode;
            }
            return path.Substring(0, path.Length - 1);
        }

        public void Translate(string translateDirection) {
            try {
                foreach (var resource in Resources) {
                    resource.NewValue = YandexTranslator.Translate(translateDirection, resource.OriginalValue);
                }
            } catch (Exception ex) {
                MessageBox.Show(ex.Message, "Yandex API", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public string GetXml() {
            foreach (var resource in Resources.Where(t=>!string.IsNullOrEmpty(t.NewValue))) {
                resource.XmlAttribute.Value = resource.NewValue;
            }
            using (var ms = new MemoryStream()) {
                XmlDoc.Save(ms);
                ms.Seek(0, SeekOrigin.Begin);
                var reader = new StreamReader(ms);
                return reader.ReadToEnd();
            }
        }

        public void SaveFile() {
            try {
                string text;
                using (var ms = new MemoryStream()) {
                    XmlDoc.Save(ms);
                    ms.Seek(0, SeekOrigin.Begin);
                    var reader = new StreamReader(ms);
                    text = reader.ReadToEnd();
                }
                var prefixBytes = new byte[] {255, 254};
                var prefix = Encoding.GetEncoding("utf-16LE").GetString(prefixBytes);
                text = string.Format("{0}{1}", prefix, text);
                File.WriteAllText(_file, text, Encoding.GetEncoding("utf-16LE"));
            } catch (Exception ex) {
                MessageBox.Show(ex.Message, "Warning", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
