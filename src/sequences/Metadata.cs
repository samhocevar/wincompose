//
//  WinCompose — a compose key for Windows — http://wincompose.info/
//
//  Copyright © 2013—2019 Sam Hocevar <sam@hocevar.net>
//
//  This program is free software. It comes without any warranty, to
//  the extent permitted by applicable law. You can redistribute it
//  and/or modify it under the terms of the Do What the Fuck You Want
//  to Public License, Version 2, as published by the WTFPL Task Force.
//  See http://www.wtfpl.net/ for more details.
//

using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace WinCompose
{
    public struct SequenceIdentifier
    {
        public SequenceIdentifier(KeySequence sequence, string result)
        {
            Sequence = sequence;
            Result = result;
        }

        public readonly KeySequence Sequence;
        public readonly string Result;
    }

    public class Data
    {
        public bool Favorite;
    }

    public class MetadataDB : Dictionary<SequenceIdentifier, Data>, IXmlSerializable
    {
        public Data GetOrAdd(KeySequence sequence, string result)
        {
            var key = new SequenceIdentifier(sequence, result);
            if (!TryGetValue(key, out var ret))
                Add(key, ret = new Data());
            return ret as Data;
        }

        #region IXmlSerializable
        public void WriteXml(XmlWriter writer)
        {
            foreach (var kv in this)
            {
                writer.WriteStartElement("Item");
                writer.WriteAttributeString("Sequence", kv.Key.Sequence.ToString());
                writer.WriteAttributeString("Result", kv.Key.Result.ToString());
                writer.WriteAttributeString("Favorite", kv.Value.Favorite.ToString());
                writer.WriteEndElement();
            }
        }

        public void ReadXml(XmlReader reader)
        {
            var cv = TypeDescriptor.GetConverter(typeof(KeySequence));

            while (reader.Read())
            {
                if (reader.NodeType != XmlNodeType.Element || reader.Name != "Item")
                    continue;
                var sequence = reader.GetAttribute("Sequence");
                var result = reader.GetAttribute("Result");
                if (sequence == null || result == null)
                    continue;
                var data = GetOrAdd(cv.ConvertFromString(sequence) as KeySequence, result);
                data.Favorite = bool.Parse(reader.GetAttribute("Favorite") ?? "false");
            }
        }

        public XmlSchema GetSchema() => null;
        #endregion
    }

    public class Metadata
    {
        public Metadata()
        {
        }

        public void AddFavorite(KeySequence sequence, string result)
        {
            m_dict.GetOrAdd(sequence, result).Favorite = true;
        }

        private void Load()
        {
            var xs = new XmlSerializer(typeof(MetadataDB));
            using (TextReader tr = new StreamReader(FileName))
                m_dict = xs.Deserialize(tr) as MetadataDB;
        }

        private void Save()
        {
            var xs = new XmlSerializer(typeof(MetadataDB));
            using (TextWriter tw = new StreamWriter(FileName))
                xs.Serialize(tw, m_dict);
        }

        private string FileName = Path.Combine(Utils.AppDataDir, "userdata.xml");

        private MetadataDB m_dict = new MetadataDB();
    }
}

