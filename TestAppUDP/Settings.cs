using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace TestAppUDP
{
    public class Settings
    {
        public readonly Hashtable parameters = new Hashtable();

        public Settings()
        {
            parameters = ReadSettings();
        }
        public Hashtable ReadSettings()
        {
            if (File.Exists("Settings.xml"))
            {
                XmlReader xmlReader = XmlReader.Create("Settings.xml");

                while (xmlReader.Read())
                {
                    if (xmlReader.NodeType == XmlNodeType.Element)
                    {
                        var name = xmlReader.Name;
                        xmlReader.Read();
                        if (xmlReader.HasValue)
                            parameters.Add(name, xmlReader.Value);
                    }
                }
            }
            else
            {
                Console.WriteLine("Файл настроек не найден");
                //throw new FileNotFoundException("Файл настроек не найден");
            }
                

            return parameters;
        }
    }
}
