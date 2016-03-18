using Interoperability.Entity;
using Interoperability.Space;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Interoperability
{
    static class InitializationFunction
    {
        static public void Group(int groupsize)
        {
            GeneralFunction.ConsoleAndFile("GroupSize," + groupsize.ToString());
            int numberOfAnnotationsPerGroup = 30 / groupsize;
            foreach (Sentence sentence in TrainConstant.SentenceList)
            {
                sentence.TargetWorkerTargetAnnotationDicGroup = new IDictionary<TargetWorker, TargetAnnotation>[numberOfAnnotationsPerGroup];
                for (int i = 0; i < numberOfAnnotationsPerGroup; ++i)
                {
                    sentence.TargetWorkerTargetAnnotationDicGroup[i] = new Dictionary<TargetWorker, TargetAnnotation>();
                }
            }
            foreach (TargetWorker targetWorker in TrainConstant.TargetWorkerList)
            {
                targetWorker.SentenceTargetAnnotationDicGroup = new Dictionary<Sentence, TargetAnnotation>[numberOfAnnotationsPerGroup];
                for (int i = 0; i < numberOfAnnotationsPerGroup; ++i)
                {
                    targetWorker.SentenceTargetAnnotationDicGroup[i] = new Dictionary<Sentence, TargetAnnotation>();
                }
            }
            //往各组里加TargetWorker, TargetAnnotation
            foreach (Sentence sentence in TrainConstant.SentenceList)
            {
                for (int i = 0; i < sentence.TargetWorkerTargetAnnotationDic.Count; ++i)
                {
                    int groupIndex = i / groupsize;
                    KeyValuePair<TargetWorker, TargetAnnotation> workerAnnotation = sentence.TargetWorkerTargetAnnotationDic.ElementAt(i);
                    sentence.TargetWorkerTargetAnnotationDicGroup[groupIndex].Add(workerAnnotation);
                    workerAnnotation.Key.SentenceTargetAnnotationDicGroup[groupIndex].Add(sentence, workerAnnotation.Value);
                }
            }
        }

        /// <summary>
        /// Nakamura→Ekman.
        /// </summary>
        static public void _InitializeLoveSample()
        {
            Constant.SourceTaxonomy.LabelArray = Constant.EkmanLabelArray;
            Constant.TargetTaxonomy.LabelArray = Constant.NakaLabelArray;

            string[] data = File.ReadAllLines("LoveSample/EkmanGold-sample.csv");
            for (int j = 0; j < data.Length; ++j)
            {
                string[] labels = data[j].Split(',');
                SourceAnnotation sourceAnnotation = new SourceAnnotation();
                for (int i = 0; i < Constant.EkmanLabelArray.Length; ++i)
                {
                    switch (labels[i])
                    {
                        case "joy":
                            sourceAnnotation.LabelAndTruthDic[Label.Joy] = true;
                            break;
                        case "anger":
                            sourceAnnotation.LabelAndTruthDic[Label.Anger] = true;
                            break;
                        case "sadness":
                            sourceAnnotation.LabelAndTruthDic[Label.Sadness] = true;
                            break;
                        case "fear":
                            sourceAnnotation.LabelAndTruthDic[Label.Fear] = true;
                            break;
                        case "disgust":
                            sourceAnnotation.LabelAndTruthDic[Label.Disgust] = true;
                            break;
                        case "surprise":
                            sourceAnnotation.LabelAndTruthDic[Label.Surprise] = true;
                            break;
                    }
                }
                TrainConstant.SentenceList[j].GoldSourceAnnotation.LabelAndTruthDic = new Dictionary<Label, bool>(sourceAnnotation.LabelAndTruthDic);
                sourceAnnotation = new SourceAnnotation();
            }

            initializeNakaAsTarget(Corpus.LoveSample, 0, 62, TrainConstant.SentenceList, ref TrainConstant.TargetWorkerList);
            SpaceConstant.TargetWorkerNumberPerSentence = 41;
        }

        /// <summary>
        /// 只包含52-77句
        /// </summary>
        static public void _InitializeAppleSample(IList<Sentence> sentences, ref IList<SourceWorker> sourceWorkerList, ref IList<TargetWorker> targetWorkerList)
        {
            if (Constant.SourceTaxonomy.Name == TaxonomyType.Ekman && Constant.TargetTaxonomy.Name == TaxonomyType.Nakamura)
            {
                initializeEkmanAsSource(Corpus.AppleSample, 0, 25, sentences, ref sourceWorkerList);
                initializeNakaAsTarget(Corpus.AppleSample, 0, 25, sentences, ref targetWorkerList);
                SpaceConstant.TargetWorkerNumberPerSentence = 40;
            }
            else if (Constant.SourceTaxonomy.Name == TaxonomyType.Nakamura && Constant.TargetTaxonomy.Name == TaxonomyType.Ekman)
            {
                initializeNakaAsSource(Corpus.AppleSample, 0, 25, sentences, ref sourceWorkerList);
                initializeEkmanAsTarget(Corpus.AppleSample, 0, 25, sentences, ref targetWorkerList);
                SpaceConstant.TargetWorkerNumberPerSentence = 30;
            }
        }

        static public void InitializeLove(IList<Sentence> sentences, ref IList<SourceWorker> sourceWorkerList, ref IList<TargetWorker> targetWorkerList)
        {
            if (Constant.SourceTaxonomy.Name == TaxonomyType.Ekman && Constant.TargetTaxonomy.Name == TaxonomyType.Nakamura)
            {
                initializeEkmanAsSource(Corpus.Love, 0, 33, sentences, ref sourceWorkerList);
                initializeEkmanAsSource(Corpus.Love, 34, 62, sentences, ref sourceWorkerList);
                initializeNakaAsTarget(Corpus.Love, 0, 62, sentences, ref targetWorkerList);
            }
            else if (Constant.SourceTaxonomy.Name == TaxonomyType.Nakamura && Constant.TargetTaxonomy.Name == TaxonomyType.Ekman)
            {
                initializeNakaAsSource(Corpus.Love, 0, 62, sentences, ref sourceWorkerList);
                initializeEkmanAsTarget(Corpus.Love, 0, 33, sentences, ref targetWorkerList);
                initializeEkmanAsTarget(Corpus.Love, 34, 62, sentences, ref targetWorkerList);
            }
        }

        static public void InitializeApple(IList<Sentence> sentences, ref IList<SourceWorker> sourceWorkerList, ref IList<TargetWorker> targetWorkerList)
        {
            if (Constant.SourceTaxonomy.Name == TaxonomyType.Ekman && Constant.TargetTaxonomy.Name == TaxonomyType.Nakamura)
            {
                initializeEkmanAsSource(Corpus.Apple, 0, 24, sentences, ref sourceWorkerList);
                initializeEkmanAsSource(Corpus.Apple, 25, 51, sentences, ref sourceWorkerList);
                initializeEkmanAsSource(Corpus.Apple, 52, 77, sentences, ref sourceWorkerList);
                initializeNakaAsTarget(Corpus.Apple, 0, 24, sentences, ref targetWorkerList);
                initializeNakaAsTarget(Corpus.Apple, 25, 51, sentences, ref targetWorkerList);
                initializeNakaAsTarget(Corpus.Apple, 52, 77, sentences, ref targetWorkerList);
            }
            else if (Constant.SourceTaxonomy.Name == TaxonomyType.Nakamura && Constant.TargetTaxonomy.Name == TaxonomyType.Ekman)
            {
                initializeNakaAsSource(Corpus.Apple, 0, 24, sentences, ref sourceWorkerList);
                initializeNakaAsSource(Corpus.Apple, 25, 51, sentences, ref sourceWorkerList);
                initializeNakaAsSource(Corpus.Apple, 52, 77, sentences, ref sourceWorkerList);
                initializeEkmanAsTarget(Corpus.Apple, 0, 24, sentences, ref targetWorkerList);
                initializeEkmanAsTarget(Corpus.Apple, 25, 51, sentences, ref targetWorkerList);
                initializeEkmanAsTarget(Corpus.Apple, 52, 77, sentences, ref targetWorkerList);
            }
        }

        static private void initializeEkmanAsSource(Corpus experiment, int startIndex, int endIndex, IList<Sentence> sentences, ref IList<SourceWorker> sourceWorkerList)
        {
            string[] data = File.ReadAllLines(experiment + "/EkmanData" + startIndex + "-" + endIndex + ".csv");
            foreach (string row in data)
            {
                string[] labels = row.Split(',');
                SourceWorker worker = new SourceWorker(labels[0]);
                if (!sourceWorkerList.Contains(worker))//重复的人不再添加
                {
                    sourceWorkerList.Add(worker);
                }
                else
                {
                    worker = sourceWorkerList.First(x => x.Equals(worker));
                }
                IList<Label> trueLabels = new List<Label>();
                for (int i = 1; i <= (endIndex - startIndex + 1) * (Constant.EkmanLabelArray.Length + 1); ++i)
                {
                    switch (labels[i])
                    {
                        case "Anger":
                            trueLabels.Add(Label.Anger);
                            break;
                        case "Sadness":
                            trueLabels.Add(Label.Sadness);
                            break;
                        case "Joy":
                            trueLabels.Add(Label.Joy);
                            break;
                        case "Disgust":
                            trueLabels.Add(Label.Disgust);
                            break;
                        case "Surprise":
                            trueLabels.Add(Label.Surprise);
                            break;
                        case "Fear":
                            trueLabels.Add(Label.Fear);
                            break;
                    }
                    if (i % (Constant.EkmanLabelArray.Length + 1) == 0)
                    {
                        //取出SentenceList里的一个Sentence
                        Sentence sentence = sentences[startIndex + (i - 1) / (Constant.EkmanLabelArray.Length + 1)];
                        SourceAnnotation sourceAnnotation = new SourceAnnotation(trueLabels.ToArray());
                        trueLabels.Clear();
                        worker.SentenceSourceAnnotationDic.Add(sentence, sourceAnnotation);
                        sentence.SourceWorkerSourceAnnotationDic.Add(worker, sourceAnnotation);
                    }
                }
            }
        }

        static private void initializeEkmanAsTarget(Corpus corpus, int startIndex, int endIndex, IList<Sentence> sentences, ref IList<TargetWorker> targetWorkerList)
        {
            string[] data = File.ReadAllLines(corpus + "/EkmanData" + startIndex + "-" + endIndex + ".csv");
            foreach (string row in data)
            {
                string[] labels = row.Split(',');
                TargetWorker worker = new TargetWorker(labels[0]);
                if (!targetWorkerList.Contains(worker))//重复的人不再添加
                {
                    targetWorkerList.Add(worker);
                }
                else
                {
                    worker = targetWorkerList.First(x => x.Equals(worker));
                }
                IList<Label> trueLabels = new List<Label>();
                for (int i = 1; i <= (endIndex - startIndex + 1) * (Constant.EkmanLabelArray.Length + 1); ++i)
                {
                    switch (labels[i])
                    {
                        case "Anger":
                            trueLabels.Add(Label.Anger);
                            break;
                        case "Sadness":
                            trueLabels.Add(Label.Sadness);
                            break;
                        case "Joy":
                            trueLabels.Add(Label.Joy);
                            break;
                        case "Disgust":
                            trueLabels.Add(Label.Disgust);
                            break;
                        case "Surprise":
                            trueLabels.Add(Label.Surprise);
                            break;
                        case "Fear":
                            trueLabels.Add(Label.Fear);
                            break;
                    }
                    if (i % (Constant.EkmanLabelArray.Length + 1) == 0)
                    {
                        //取出SentenceList里的一个Sentence
                        Sentence sentence = sentences[startIndex + (i - 1) / (Constant.EkmanLabelArray.Length + 1)];
                        TargetAnnotation targetAnnotation = new TargetAnnotation(trueLabels.ToArray());
                        trueLabels.Clear();
                        worker.SentenceTargetAnnotationDic.Add(sentence, targetAnnotation);
                        sentence.TargetWorkerTargetAnnotationDic.Add(worker, targetAnnotation);
                    }
                }
            }
        }

        static private void initializeNakaAsSource(Corpus corpus, int startIndex, int endIndex, IList<Sentence> sentences, ref IList<SourceWorker> sourceWorkerList)
        {
            string[] data = File.ReadAllLines(corpus + "/NakaData" + startIndex + "-" + endIndex + ".csv");//不需要单独做一个data-sample,因为sentence-sample以外的sentence在下面的for循环里遍历不到。
            foreach (string row in data)
            {
                string[] labels = row.Split(',');//labels[0]是用户名
                SourceWorker worker = new SourceWorker(labels[0]);
                if (!sourceWorkerList.Contains(worker))//重复的人不再添加
                {
                    sourceWorkerList.Add(worker);
                }
                else
                {
                    worker = sourceWorkerList.First(x => x.Equals(worker));
                }
                IList<Label> trueLabels = new List<Label>();
                for (int i = 1; i <= (endIndex - startIndex + 1) * (Constant.NakaLabelArray.Length + 1); ++i)
                {
                    switch (labels[i])
                    {
                        case "happiness":
                            trueLabels.Add(Label.喜Joy);
                            break;
                        case "fondness":
                            trueLabels.Add(Label.好Fondness);
                            break;
                        case "relief":
                            trueLabels.Add(Label.安Relief);
                            break;
                        case "anger":
                            trueLabels.Add(Label.怒Anger);
                            break;
                        case "sadness":
                            trueLabels.Add(Label.哀Sadness);
                            break;
                        case "fear":
                            trueLabels.Add(Label.怖Fear);
                            break;
                        case "shame":
                            trueLabels.Add(Label.恥Shame);
                            break;
                        case "disgust":
                            trueLabels.Add(Label.厭Disgust);
                            break;
                        case "excitement":
                            trueLabels.Add(Label.昂Excitement);
                            break;
                        case "surprise":
                            trueLabels.Add(Label.驚Surprise);
                            break;
                    }
                    if (i % (Constant.NakaLabelArray.Length + 1) == 0)
                    {
                        //取出SentenceList里的一个Sentence
                        Sentence sentence = sentences[startIndex + (i - 1) / (Constant.NakaLabelArray.Length + 1)];
                        SourceAnnotation targetAnnotation = new SourceAnnotation(trueLabels.ToArray());
                        trueLabels.Clear();
                        worker.SentenceSourceAnnotationDic.Add(sentence, targetAnnotation);
                        sentence.SourceWorkerSourceAnnotationDic.Add(worker, targetAnnotation);
                    }
                }
            }
        }

        static private void initializeNakaAsTarget(Corpus corpus, int startIndex, int endIndex, IList<Sentence> sentences, ref IList<TargetWorker> targetWorkerList)
        {
            string[] data = File.ReadAllLines(corpus + "/NakaData" + startIndex + "-" + endIndex + ".csv");//不需要单独做一个data-sample,因为sentence-sample以外的sentence在下面的for循环里遍历不到。
            foreach (string row in data)
            {
                string[] labels = row.Split(',');//labels[0]是用户名
                TargetWorker worker = new TargetWorker(labels[0]);
                if (!targetWorkerList.Contains(worker))//重复的人不再添加
                {
                    targetWorkerList.Add(worker);
                }
                else
                {
                    worker = targetWorkerList.First(x => x.Equals(worker));
                }
                IList<Label> trueLabels = new List<Label>();
                for (int i = 1; i <= (endIndex - startIndex + 1) * (Constant.NakaLabelArray.Length + 1); ++i)
                {
                    switch (labels[i])
                    {
                        case "happiness":
                            trueLabels.Add(Label.喜Joy);
                            break;
                        case "fondness":
                            trueLabels.Add(Label.好Fondness);
                            break;
                        case "relief":
                            trueLabels.Add(Label.安Relief);
                            break;
                        case "anger":
                            trueLabels.Add(Label.怒Anger);
                            break;
                        case "sadness":
                            trueLabels.Add(Label.哀Sadness);
                            break;
                        case "fear":
                            trueLabels.Add(Label.怖Fear);
                            break;
                        case "shame":
                            trueLabels.Add(Label.恥Shame);
                            break;
                        case "disgust":
                            trueLabels.Add(Label.厭Disgust);
                            break;
                        case "excitement":
                            trueLabels.Add(Label.昂Excitement);
                            break;
                        case "surprise":
                            trueLabels.Add(Label.驚Surprise);
                            break;
                    }
                    if (i % (Constant.NakaLabelArray.Length + 1) == 0)
                    {
                        //取出SentenceList里的一个Sentence
                        Sentence sentence = sentences[startIndex + (i - 1) / (Constant.NakaLabelArray.Length + 1)];
                        TargetAnnotation targetAnnotation = new TargetAnnotation(trueLabels.ToArray());
                        trueLabels.Clear();
                        worker.SentenceTargetAnnotationDic.Add(sentence, targetAnnotation);
                        sentence.TargetWorkerTargetAnnotationDic.Add(worker, targetAnnotation);
                    }
                }
            }
        }

        /// <summary>
        /// 只过滤标注Nakamura的worker，不管Train还是NotTrain，Source还是Target
        /// </summary>
        static public void FilterTargetWorker()
        {
            if (Constant.SourceTaxonomy.Name == TaxonomyType.Nakamura)
            {
                foreach (Sentence sentence in TrainConstant.SentenceList)
                {
                    for (int i = sentence.SourceWorkerSourceAnnotationDic.Count - 1; i >= 30; --i)//用.Count就不用管sentence被标了40次还是41次
                    {
                        SourceWorker sourceWorker = sentence.SourceWorkerSourceAnnotationDic.ElementAt(i).Key;
                        sentence.SourceWorkerSourceAnnotationDic.Remove(sourceWorker);
                        sourceWorker.SentenceSourceAnnotationDic.Remove(sentence);
                    }
                }
                foreach (SourceWorker sourceWorker in TrainConstant.SourceWorkerList.ToArray())
                {
                    if (sourceWorker.SentenceSourceAnnotationDic.Count == 0)
                    {
                        TrainConstant.SourceWorkerList.Remove(sourceWorker);
                    }
                }
                foreach (Sentence sentence in NotTrainConstant.SentenceList)
                {
                    for (int i = sentence.SourceWorkerSourceAnnotationDic.Count - 1; i >= 30; --i)//用.Count就不用管sentence被标了40次还是41次
                    {
                        SourceWorker sourceWorker = sentence.SourceWorkerSourceAnnotationDic.ElementAt(i).Key;
                        sentence.SourceWorkerSourceAnnotationDic.Remove(sourceWorker);
                        sourceWorker.SentenceSourceAnnotationDic.Remove(sentence);
                    }
                }
                foreach (SourceWorker sourceWorker in NotTrainConstant.SourceWorkerList.ToArray())
                {
                    if (sourceWorker.SentenceSourceAnnotationDic.Count == 0)
                    {
                        NotTrainConstant.SourceWorkerList.Remove(sourceWorker);
                    }
                }
            }
            else if (Constant.TargetTaxonomy.Name == TaxonomyType.Nakamura)
            {
                foreach (Sentence sentence in TrainConstant.SentenceList)
                {
                    for (int i = sentence.TargetWorkerTargetAnnotationDic.Count - 1; i >= 30; --i)
                    {
                        TargetWorker targetWorker = sentence.TargetWorkerTargetAnnotationDic.ElementAt(i).Key;
                        sentence.TargetWorkerTargetAnnotationDic.Remove(targetWorker);
                        targetWorker.SentenceTargetAnnotationDic.Remove(sentence);
                    }
                }
                foreach (TargetWorker targetWorker in TrainConstant.TargetWorkerList.ToArray())
                {
                    if (targetWorker.SentenceTargetAnnotationDic.Count == 0)
                    {
                        TrainConstant.TargetWorkerList.Remove(targetWorker);
                    }
                }
                foreach (Sentence sentence in NotTrainConstant.SentenceList)
                {
                    for (int i = sentence.TargetWorkerTargetAnnotationDic.Count - 1; i >= 30; --i)
                    {
                        TargetWorker targetWorker = sentence.TargetWorkerTargetAnnotationDic.ElementAt(i).Key;
                        sentence.TargetWorkerTargetAnnotationDic.Remove(targetWorker);
                        targetWorker.SentenceTargetAnnotationDic.Remove(sentence);
                    }
                }
                foreach (TargetWorker targetWorker in NotTrainConstant.TargetWorkerList.ToArray())
                {
                    if (targetWorker.SentenceTargetAnnotationDic.Count == 0)
                    {
                        NotTrainConstant.TargetWorkerList.Remove(targetWorker);
                    }
                }
            }
        }

        /// <summary>
        /// 根据与concensus的近似度来过滤worker，
        /// 生成Variable.WorkerList。
        /// 废弃。
        /// </summary>
        static void filterTargetWorkerOfLove()
        {
            IDictionary<Sentence, IDictionary<Label, double>> ConsensusAnnotation = getTargetConsensusGoldOfLove();
            IDictionary<TargetWorker, double> workerSimilarity = new Dictionary<TargetWorker, double>();
            foreach (TargetWorker worker in TrainConstant.TargetWorkerList)
            {
                workerSimilarity.Add(worker, 0);
                foreach (KeyValuePair<Sentence, TargetAnnotation> SentenceAnnotation in worker.SentenceTargetAnnotationDic)
                {
                    foreach (Label label in Constant.TargetTaxonomy.LabelArray)
                    {
                        workerSimilarity[worker] += 1 - Math.Abs(Convert.ToDouble(SentenceAnnotation.Value.LabelAndTruthDic[label]) - ConsensusAnnotation[SentenceAnnotation.Key][label]);
                    }
                }
                workerSimilarity[worker] /= TrainConstant.SentenceList.Count * Constant.TargetTaxonomy.LabelArray.Length;
            }
            IDictionary<TargetWorker, double> sortedElements = GeneralFunction.SortDictionary(workerSimilarity);
            for (int i = 30; i < sortedElements.Count; ++i)
            {
                TrainConstant.TargetWorkerList.Remove(sortedElements.ElementAt(i).Key);
            }
        }

        /// <summary>
        /// 用每个label被标注的比例来表示consensus annotation。
        /// 废弃
        /// </summary>
        /// <returns>每个句子和其consensus annotation</returns>
        static IDictionary<Sentence, IDictionary<Label, double>> getTargetConsensusGoldOfLove()
        {
            IDictionary<Sentence, IDictionary<Label, double>> ConsensusAnnotation = new Dictionary<Sentence, IDictionary<Label, double>>();
            foreach (Sentence sentence in TrainConstant.SentenceList)
            {
                ConsensusAnnotation[sentence] = new Dictionary<Label, double>();
                foreach (Label label in Constant.TargetTaxonomy.LabelArray)
                {
                    ConsensusAnnotation[sentence].Add(label, 0);
                }
            }

            foreach (TargetWorker worker in TrainConstant.TargetWorkerList)
            {
                foreach (KeyValuePair<Sentence, TargetAnnotation> SentenceAnnotation in worker.SentenceTargetAnnotationDic)
                {
                    foreach (Label label in Constant.TargetTaxonomy.LabelArray)
                    {
                        if (SentenceAnnotation.Value.LabelAndTruthDic[label])
                        {
                            ++ConsensusAnnotation[SentenceAnnotation.Key][label];
                        }
                    }
                }
            }
            foreach (Sentence sentence in TrainConstant.SentenceList)
            {
                foreach (Label label in Constant.TargetTaxonomy.LabelArray)
                {
                    ConsensusAnnotation[sentence][label] /= TrainConstant.TargetWorkerList.Count;
                }
            }
            return ConsensusAnnotation;
        }
    }
}