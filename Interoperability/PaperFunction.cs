using Interoperability.Entity;
using System.Collections.Generic;

namespace Interoperability
{
    static class PaperFunction
    {
        static public IDictionary<Annotation, double> NumberOfEachAnnotation(SourceOrTarget sourceOrTarget, IList<Sentence> sentenceList)
        {
            IDictionary<Annotation, double> numberOfEachAnnotation = new Dictionary<Annotation, double>();

            switch (sourceOrTarget)
            {
                case SourceOrTarget.Source:
                    foreach (Sentence sentence in sentenceList)
                    {
                        foreach (KeyValuePair<SourceAnnotation, double> annotationAndCount in sentence.SortedSourceAnnotationDic)
                        {
                            if (numberOfEachAnnotation.ContainsKey(annotationAndCount.Key))
                                numberOfEachAnnotation[annotationAndCount.Key] += annotationAndCount.Value;
                            else
                                numberOfEachAnnotation.Add(annotationAndCount.Key, annotationAndCount.Value);
                        }
                    }
                    break;
                case SourceOrTarget.Target:
                    foreach (Sentence sentence in sentenceList)
                    {
                        foreach (KeyValuePair<TargetAnnotation, double> annotationAndCount in sentence.SortedTargetAnnotationDic)
                        {
                            if (numberOfEachAnnotation.ContainsKey(annotationAndCount.Key))
                                numberOfEachAnnotation[annotationAndCount.Key] += annotationAndCount.Value;
                            else
                                numberOfEachAnnotation.Add(annotationAndCount.Key, annotationAndCount.Value);
                        }
                    }
                    break;
            }
            return GeneralFunction.SortDictionary(numberOfEachAnnotation);
        }

        static public IDictionary<Label, double> NumberOfEachLabel(SourceOrTarget sourceOrTarget, IList<Sentence> sentenceList)
        {
            IDictionary<Label, double> numberOfEachLabel = new Dictionary<Label, double>();

            switch (sourceOrTarget)
            {
                case SourceOrTarget.Source:
                    foreach (Label label in Constant.SourceTaxonomy.LabelArray)
                    {
                        numberOfEachLabel.Add(label, 0);
                    }
                    numberOfEachLabel.Add(Label.None, 0);

                    foreach (Sentence sentence in sentenceList)
                    {
                        foreach (Label label in sentence.SortedSourceLabelDic.Keys)
                        {
                            numberOfEachLabel[label] += sentence.SortedSourceLabelDic[label];
                        }
                    }
                    break;
                case SourceOrTarget.Target:
                    foreach (Label label in Constant.TargetTaxonomy.LabelArray)
                    {
                        numberOfEachLabel.Add(label, 0);
                    }
                    numberOfEachLabel.Add(Label.None, 0);

                    foreach (Sentence sentence in sentenceList)
                    {
                        foreach (Label label in sentence.SortedTargetLabelDic.Keys)
                        {
                            numberOfEachLabel[label] += sentence.SortedTargetLabelDic[label];
                        }
                    }
                    break;
            }

            return GeneralFunction.SortDictionary(numberOfEachLabel);
        }

        static public int NumberOfWorkers(SourceOrTarget sourceOrTarget, IList<Sentence> sentenceList)
        {
            IList<Worker> workers = new List<Worker>();
            foreach (Sentence sentence in sentenceList)
            {
                switch(sourceOrTarget)
                {
                    case SourceOrTarget.Source:
                        foreach (Worker worker in sentence.SourceWorkerSourceAnnotationDic.Keys)
                        {
                            if (!workers.Contains(worker))
                                workers.Add(worker);
                        }
                        break;
                    case SourceOrTarget.Target:
                        foreach (Worker worker in sentence.TargetWorkerTargetAnnotationDic.Keys)
                        {
                            if (!workers.Contains(worker))
                                workers.Add(worker);
                        }
                        break;
                }
            }
            return workers.Count;
        }
    }
}