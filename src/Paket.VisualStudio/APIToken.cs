using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Paket.VisualStudio
{
    [Serializable()]
    [XmlRoot("configuration")]
    public class APIToken
    {
        [XmlElement("credentials")]
        public APITokenCredential APITokenCredentials { get; set; }
    }

    [Serializable()]
    public class APITokenCredential
    {
        public APITokenCredential()
        {
            Tokens = new List<Token>();
        }

        [XmlElement("token")]
        public List<Token> Tokens { get; set; }
    }

    [Serializable()]
    public class Token
    {
        [XmlAttribute("source")]
        public string Source { get; set; }

        [XmlAttribute("value")]
        public string Value { get; set; }
    }
}