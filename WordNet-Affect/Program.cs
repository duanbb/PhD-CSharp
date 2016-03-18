using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace WordNet_Affect
{
    class Program
    {
        static void Main(string[] args)
        {
            //GenerateAffectTree();
            CalculateSimilarities();
        }

        //PPT用，计算需要多少similarity（affect pair）
        static private void CalculateSimilarities()
        {
            //XElement emotion = XElement.Parse(File.ReadAllText("output.xml")).Element("mental-state").Element("affective-state").Element("emotion");
            XElement emotion = XElement.Parse(File.ReadAllText("output.xml"));
            int numberOfbrotherSimilarities = 0;
            int numberOfParentChildrenSimilarities = 0;
            IList<string> affects = new List<string>();
            IList<string> existedAffects = new List<string>();
            foreach (XElement valenceElement in emotion.Elements())
            {
                foreach (XElement affectElement in valenceElement.Descendants())
                {
                    string affect = affectElement.Name.ToString();
                    if (!affects.Contains(affect))
                    {
                        affects.Add(affect);
                        int numberOfDescendants = affectElement.Elements().Count();
                        if (numberOfDescendants > 1)
                        {
                            numberOfbrotherSimilarities += numberOfDescendants * (numberOfDescendants - 1) / 2;
                            numberOfParentChildrenSimilarities += numberOfDescendants;
                        }
                    }
                    else
                    {
                        existedAffects.Add(affect);
                    }
                }
            }
            int all = numberOfbrotherSimilarities + numberOfParentChildrenSimilarities;
        }

        static private void GenerateAffectTree()
        {
            XElement xml = XElement.Parse(File.ReadAllText("a-hierarchy.xml"));
            IEnumerable<XElement> readxml = xml.Descendants();
            XElement writexml = null;
            List<string> list = new List<string>();
            foreach (XElement descendant in readxml)
            {
                //if (list.Contains(descendant.Attribute("name").Value))
                //{ 
                //}
                //list.Add(descendant.Attribute("name").Value);
                XAttribute parent = descendant.Attribute("isa");
                if (parent == null)
                {
                    writexml = new XElement("root");
                }
                else
                {
                    XElement x = new XElement(descendant.Attribute("name").Value);
                    writexml.DescendantsAndSelf(parent.Value).First().Add(x);
                }
            }
            StreamWriter writeFile = new StreamWriter("output.xml");
            writeFile.Write(writexml);
            writeFile.Close();
        }
    }
}