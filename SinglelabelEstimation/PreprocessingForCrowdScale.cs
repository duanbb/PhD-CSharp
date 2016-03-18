using System;
using System.Collections.Generic;
using System.IO;

namespace SinglelabelEstimation
{
    static class PreprocessingForCrowdScale
    {
        static public void MakeSentenceFileForCrowdScale()
        {
            string[] rows = File.ReadAllLines("CrowdScale 2013/full_final.csv");
            IList<string> sentenceIDs = new List<string>();
            StreamWriter sentenceFile = new StreamWriter("CrowdScale 2013/sentences.csv");
            foreach (string row in rows)
            {
                string[] cells = row.Split(',');
                string sentenceID = cells[0];
                if (!sentenceIDs.Contains(sentenceID))
                {
                    sentenceIDs.Add(sentenceID);
                    for (int i = 8; i < cells.Length; ++i)
                    {
                        sentenceID += "," + cells[i];
                    }
                    sentenceFile.WriteLine(sentenceID);
                }
            }
            sentenceFile.Close();
        }

        static public void MakeDataAndWorkerFileForCrowdSclae()
        {
            string[] rows = File.ReadAllLines("CrowdScale 2013/basic.csv");
            StreamWriter workerFile = new StreamWriter("CrowdScale 2013/workers.csv");
            IList<string> workers = new List<string>();
            foreach (string row in rows)
            {
                string worker = row.Split(',')[1];
                if (!workers.Contains(worker))
                {
                    workers.Add(worker);
                    workerFile.WriteLine(worker);
                }
            }
            workerFile.Close();

            StreamWriter dataFile = new StreamWriter("CrowdScale 2013/data.csv");
            string sentence = string.Empty;
            IDictionary<string, List<string>> voters = new Dictionary<string, List<string>>();
            string write = string.Empty;
            foreach (string row in rows)
            {
                string[] cells = row.Split(',');
                if (sentence != string.Empty)
                {
                    if (sentence == cells[0])
                    {
                        if (!voters.ContainsKey(cells[1]))
                            voters.Add(cells[1], new List<string>());
                        voters[cells[1]].Add(cells[2]);
                    }
                    else
                    {
                        foreach (string worker in workers)
                        {
                            if (voters.ContainsKey(worker))
                            {
                                foreach (string label in voters[worker])
                                    write += label;
                            }
                            write += ",";
                        }
                        dataFile.WriteLine(write.Remove(write.Length - 1));
                        sentence = cells[0];
                        voters.Clear();
                        voters.Add(cells[1], new List<string>());
                        voters[cells[1]].Add(cells[2]);
                        write = string.Empty;
                    }
                }
                else
                {
                    sentence = cells[0];
                    voters.Add(cells[1], new List<string>());
                    voters[cells[1]].Add(cells[2]);
                }
            }
            foreach (string worker in workers)
            {
                if (voters.ContainsKey(worker))
                {
                    foreach (string label in voters[worker])
                        write += label;
                }
                write += ",";
            }
            dataFile.WriteLine(write.Remove(write.Length - 1));
            dataFile.Close();
        }
    }
}