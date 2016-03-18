using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Statistic
{
    static class Function
    {
        static public void InitializeData()
        {
            string[] speeches = File.ReadAllLines("sentence.txt");
            for (int i = 0; i < Variable.NumberOfSentence; ++i)
            {
                Variable.Sentences[i] = new Sentence(i, speeches[i]);
            }
            string[] data = File.ReadAllLines("data.txt");
            int annotatorIndex = 0;
            foreach (string datum in data)
            {
                string[] affects = datum.Split(',');
                for (int j = 0; j < Variable.NumberOfLabel; ++j)
                {
                    switch (affects[j])
                    {
                        case "yorokobi":
                            Variable.Sentences[j / Variable.KindsOfLabel].Annotators[annotatorIndex].Affects[Affect.yorokobi] = true;
                            break;
                        case "suki":
                            Variable.Sentences[j / Variable.KindsOfLabel].Annotators[annotatorIndex].Affects[Affect.suki] = true;
                            break;
                        case "yasu":
                            Variable.Sentences[j / Variable.KindsOfLabel].Annotators[annotatorIndex].Affects[Affect.yasu] = true;
                            break;
                        case "ikari":
                            Variable.Sentences[j / Variable.KindsOfLabel].Annotators[annotatorIndex].Affects[Affect.ikari] = true;
                            break;
                        case "aware":
                            Variable.Sentences[j / Variable.KindsOfLabel].Annotators[annotatorIndex].Affects[Affect.aware] = true;
                            break;
                        case "kowa":
                            Variable.Sentences[j / Variable.KindsOfLabel].Annotators[annotatorIndex].Affects[Affect.kowa] = true;
                            break;
                        case "haji":
                            Variable.Sentences[j / Variable.KindsOfLabel].Annotators[annotatorIndex].Affects[Affect.haji] = true;
                            break;
                        case "iya":
                            Variable.Sentences[j / Variable.KindsOfLabel].Annotators[annotatorIndex].Affects[Affect.iya] = true;
                            break;
                        case "takaburi":
                            Variable.Sentences[j / Variable.KindsOfLabel].Annotators[annotatorIndex].Affects[Affect.takaburi] = true;
                            break;
                        case "odoroki":
                            Variable.Sentences[j / Variable.KindsOfLabel].Annotators[annotatorIndex].Affects[Affect.odoroki] = true;
                            break;
                        case "mu":
                            foreach (Affect affect in Variable.AffectArray)
                            {
                                if (Variable.Sentences[j / Variable.KindsOfLabel].Annotators[annotatorIndex].Affects[affect])
                                    break;
                                Variable.Sentences[j / Variable.KindsOfLabel].Annotators[annotatorIndex].Affects[Affect.mu] = true;
                            }
                            break;
                    }
                }
                ++annotatorIndex;
            }
        }

        //统计情感分布
        static public void Distribution()
        {
            int yorokobi = 0, suki = 0, yasu = 0, ikari = 0, aware = 0, kowa = 0, haji = 0, iya = 0, takaburi = 0, odoroki = 0, mu = 0;
            foreach (Sentence sentence in Variable.Sentences)
            {
                foreach (Annotator annotator in sentence.Annotators)
                {
                    if (annotator.Affects[Affect.yorokobi]) ++yorokobi;
                    if (annotator.Affects[Affect.suki]) ++suki;
                    if (annotator.Affects[Affect.yasu]) ++yasu;
                    if (annotator.Affects[Affect.ikari]) ++ikari;
                    if (annotator.Affects[Affect.aware]) ++aware;
                    if (annotator.Affects[Affect.kowa]) ++kowa;
                    if (annotator.Affects[Affect.haji]) ++haji;
                    if (annotator.Affects[Affect.iya]) ++iya;
                    if (annotator.Affects[Affect.takaburi]) ++takaburi;
                    if (annotator.Affects[Affect.odoroki]) ++odoroki;
                    if (annotator.Affects[Affect.mu]) ++mu;
                }
            }
            int all = yorokobi + suki + yasu + ikari + aware + kowa + haji + iya + takaburi + odoroki + mu;
        }

        //统计平均每人为每句标的情感数分布
        static public void LabelCountOfSentence()
        {
            int to25 = 0, to50 = 0, to75 = 0, to100 = 0, to125 = 0, to150 = 0, to175 = 0, to200 = 0, to225 = 0;
            foreach (Sentence sentence in Variable.Sentences)
            {
                double labelOfCount = 0;
                foreach (Annotator annotator in sentence.Annotators)
                {
                    if (annotator.Affects[Affect.yorokobi]) ++labelOfCount;
                    if (annotator.Affects[Affect.suki]) ++labelOfCount;
                    if (annotator.Affects[Affect.yasu]) ++labelOfCount;
                    if (annotator.Affects[Affect.ikari]) ++labelOfCount;
                    if (annotator.Affects[Affect.aware]) ++labelOfCount;
                    if (annotator.Affects[Affect.kowa]) ++labelOfCount;
                    if (annotator.Affects[Affect.haji]) ++labelOfCount;
                    if (annotator.Affects[Affect.iya]) ++labelOfCount;
                    if (annotator.Affects[Affect.takaburi]) ++labelOfCount;
                    if (annotator.Affects[Affect.odoroki]) ++labelOfCount;
                }
                labelOfCount /= Variable.NumberOfAnnotator;
                if (labelOfCount > 0 && labelOfCount < 0.25)
                    ++to25;
                else if (labelOfCount >= 0.25 && labelOfCount < 0.5)
                    ++to50;
                else if (labelOfCount >= 0.5 && labelOfCount < 0.75)
                    ++to75;
                else if (labelOfCount >= 0.75 && labelOfCount < 1)
                    ++to100;
                else if (labelOfCount >= 1 && labelOfCount < 1.25)
                    ++to125;
                else if (labelOfCount >= 1.25 && labelOfCount < 1.5)
                    ++to150;
                else if (labelOfCount >= 1.5 && labelOfCount < 1.75)
                    ++to175;
                else if (labelOfCount >= 1.75 && labelOfCount < 2)
                    ++to200;
                else
                    ++to225;
            }
        }

        //统计平均每人为每句标的情感数
        static public void AverageAffectOfSentence()
        {
            double labelOfCountOf = 0;
            foreach (Sentence sentence in Variable.Sentences)
            {
                double labelOfCountOfSentence = 0;
                foreach (Annotator annotator in sentence.Annotators)
                {
                    if (annotator.Affects[Affect.yorokobi]) ++labelOfCountOfSentence;
                    if (annotator.Affects[Affect.suki]) ++labelOfCountOfSentence;
                    if (annotator.Affects[Affect.yasu]) ++labelOfCountOfSentence;
                    if (annotator.Affects[Affect.ikari]) ++labelOfCountOfSentence;
                    if (annotator.Affects[Affect.aware]) ++labelOfCountOfSentence;
                    if (annotator.Affects[Affect.kowa]) ++labelOfCountOfSentence;
                    if (annotator.Affects[Affect.haji]) ++labelOfCountOfSentence;
                    if (annotator.Affects[Affect.iya]) ++labelOfCountOfSentence;
                    if (annotator.Affects[Affect.takaburi]) ++labelOfCountOfSentence;
                    if (annotator.Affects[Affect.odoroki]) ++labelOfCountOfSentence;
                }
                labelOfCountOf += labelOfCountOfSentence /= Variable.NumberOfAnnotator;
            }
            labelOfCountOf /= Variable.NumberOfSentence;
        }

        //统计不一致性
        static public void Disagreement()
        {
            int CountOfAllIsMu = 0;//所有人都标为无的句子数。结果：0句
            foreach (Sentence sentence in Variable.Sentences)
            {
                bool flatOfMu = true;
                foreach (Annotator annotator in sentence.Annotators)
                {
                    if (!annotator.Affects[Affect.mu])
                    {
                        flatOfMu = false;
                        break;
                    }
                }
                if (flatOfMu)
                    ++CountOfAllIsMu;
            }

            int CountOfMuAndAffect = 0;//无和情感混合的句子数。结果：49句
            foreach (Sentence sentence in Variable.Sentences)
            {
                foreach (Annotator annotator in sentence.Annotators)
                {
                    if (annotator.Affects[Affect.mu])
                    {
                        foreach (Annotator annotator1 in sentence.Annotators)
                        {
                            if (!annotator1.Affects[Affect.mu])
                            {
                                ++CountOfMuAndAffect;
                                break;
                            }
                        }
                        break;
                    }
                }
            }
            int CountOfMixedAffect = 0;//情感混合的句子:29
            int CountOfAgree = 0;//情感一致的句子:0
            foreach (Sentence sentence in Variable.Sentences)
            {
                bool flatOfNoMu = true;
                foreach (Annotator annotator in sentence.Annotators)
                {
                    if (annotator.Affects[Affect.mu])
                    {
                        flatOfNoMu = false;
                        break;
                    }
                }
                if (flatOfNoMu)
                {
                    bool flatOfMixed = false;
                    bool yorokobi = sentence.Annotators[0].Affects[Affect.yorokobi];
                    bool suki = sentence.Annotators[0].Affects[Affect.suki];
                    bool yasu = sentence.Annotators[0].Affects[Affect.yasu];
                    bool ikari = sentence.Annotators[0].Affects[Affect.ikari];
                    bool aware = sentence.Annotators[0].Affects[Affect.aware];
                    bool kowa = sentence.Annotators[0].Affects[Affect.kowa];
                    bool haji = sentence.Annotators[0].Affects[Affect.haji];
                    bool iya = sentence.Annotators[0].Affects[Affect.iya];
                    bool takaburi = sentence.Annotators[0].Affects[Affect.takaburi];
                    bool odoroki = sentence.Annotators[0].Affects[Affect.odoroki];
                    for (int i = 1; i < Variable.NumberOfAnnotator; ++i)
                    {
                        if (yorokobi != sentence.Annotators[i].Affects[Affect.yorokobi] ||
                            suki != sentence.Annotators[i].Affects[Affect.suki] ||
                            yasu != sentence.Annotators[i].Affects[Affect.yasu] ||
                            ikari != sentence.Annotators[i].Affects[Affect.ikari] ||
                            aware != sentence.Annotators[i].Affects[Affect.aware] ||
                            kowa != sentence.Annotators[i].Affects[Affect.kowa] ||
                            haji != sentence.Annotators[i].Affects[Affect.haji] ||
                            iya != sentence.Annotators[i].Affects[Affect.iya] ||
                            takaburi != sentence.Annotators[i].Affects[Affect.takaburi] ||
                            odoroki != sentence.Annotators[i].Affects[Affect.odoroki])
                        {
                            flatOfMixed = true;
                            break;
                        }
                    }
                    if (flatOfMixed)
                    {
                        ++CountOfMixedAffect;
                    }
                    else
                        ++CountOfAgree;
                }
            }
        }

        //Cij从每个人的标注结果中统计
        static public void Mi3OfEachAnnotator()
        {
            #region 统计单一的频率
            Dictionary<Affect, double> CountOfAffect = new Dictionary<Affect, double>();
            foreach (Affect affect in Variable.AffectArray)
            {
                CountOfAffect.Add(affect, 0);
            }
            foreach (Sentence sentence in Variable.Sentences)
            {
                foreach (Annotator annotator in sentence.Annotators)
                {
                    if (annotator.Affects[Affect.yorokobi]) ++CountOfAffect[Affect.yorokobi];
                    if (annotator.Affects[Affect.suki]) ++CountOfAffect[Affect.suki];
                    if (annotator.Affects[Affect.yasu]) ++CountOfAffect[Affect.yasu];
                    if (annotator.Affects[Affect.ikari]) ++CountOfAffect[Affect.ikari];
                    if (annotator.Affects[Affect.aware]) ++CountOfAffect[Affect.aware];
                    if (annotator.Affects[Affect.kowa]) ++CountOfAffect[Affect.kowa];
                    if (annotator.Affects[Affect.haji]) ++CountOfAffect[Affect.haji];
                    if (annotator.Affects[Affect.iya]) ++CountOfAffect[Affect.iya];
                    if (annotator.Affects[Affect.takaburi]) ++CountOfAffect[Affect.takaburi];
                    if (annotator.Affects[Affect.odoroki]) ++CountOfAffect[Affect.odoroki];
                }
            }
            double all = CountOfAffect[Affect.yorokobi] + CountOfAffect[Affect.suki] + CountOfAffect[Affect.yasu] + CountOfAffect[Affect.ikari] + CountOfAffect[Affect.aware] + CountOfAffect[Affect.kowa] + CountOfAffect[Affect.haji] + CountOfAffect[Affect.iya] + CountOfAffect[Affect.takaburi] + CountOfAffect[Affect.odoroki];
            #endregion
            #region 统计二元频率
            Dictionary<BiAffect, double> CountOfBiAffect = new Dictionary<BiAffect, double>();
            for (int i = 0; i < Variable.KindsOfLabel - 1; ++i)
            {
                for (int j = i + 1; j < Variable.KindsOfLabel - 1; ++j)
                {
                    foreach (Sentence sentence in Variable.Sentences)
                    {
                        foreach (Annotator annotator in sentence.Annotators)
                        {
                            BiAffect bi = new BiAffect(Variable.AffectArray[i], Variable.AffectArray[j]);
                            if (annotator.Affects[bi.Affect1] && annotator.Affects[bi.Affect2])
                            {
                                if (CountOfBiAffect.ContainsKey(bi))
                                    ++CountOfBiAffect[bi];
                                else
                                    CountOfBiAffect.Add(bi, 1);
                            }
                        }
                    }
                }
            }
            #endregion
            Dictionary<BiAffect, double> Mi3Dic = new Dictionary<BiAffect, double>();
            foreach (KeyValuePair<BiAffect, double> biAffect in CountOfBiAffect)
            {
                Mi3Dic.Add(biAffect.Key, Math.Log(Math.Pow(biAffect.Value, 3) * all / (CountOfAffect[biAffect.Key.Affect1] * CountOfAffect[biAffect.Key.Affect2]), 2));
            }
            List<KeyValuePair<BiAffect, double>> sortedMi3 = new List<KeyValuePair<BiAffect, double>>(Mi3Dic);
            sortedMi3.Sort(delegate(KeyValuePair<BiAffect, double> s1, KeyValuePair<BiAffect, double> s2)
            {
                return s2.Value.CompareTo(s1.Value);
            });
            foreach (KeyValuePair<BiAffect, double> biAffect in sortedMi3)
            {
                Variable.ResultFile.WriteLine(biAffect.Key.Affect1 + " + " + biAffect.Key.Affect2 + " " + biAffect.Value);
            }
        }

        //Cij从每句话对每个人的标注取并后的结果中统计(更精确) TODO 不应这么算，C应为所有人对所有句的标注，N应为句数x人数
        static public void Mi3OfAllAnnotators()
        {
            foreach (Sentence sentence in Variable.Sentences)
            {
                foreach (Annotator annotator in sentence.Annotators)
                {
                    foreach (Affect affect in Variable.AffectAndMuArray)
                    {
                        if (annotator.Affects[affect])
                            sentence.SynthesizedResult.Affects[affect] = true;
                    }
                }
            }
            #region 统计单一的频率
            Dictionary<Affect, double> CountOfAffect = new Dictionary<Affect, double>();
            foreach (Affect affect in Variable.AffectArray)
            {
                CountOfAffect.Add(affect, 0);
            }
            foreach (Sentence sentence in Variable.Sentences)
            {
                if (sentence.SynthesizedResult.Affects[Affect.yorokobi]) ++CountOfAffect[Affect.yorokobi];
                if (sentence.SynthesizedResult.Affects[Affect.suki]) ++CountOfAffect[Affect.suki];
                if (sentence.SynthesizedResult.Affects[Affect.yasu]) ++CountOfAffect[Affect.yasu];
                if (sentence.SynthesizedResult.Affects[Affect.ikari]) ++CountOfAffect[Affect.ikari];
                if (sentence.SynthesizedResult.Affects[Affect.aware]) ++CountOfAffect[Affect.aware];
                if (sentence.SynthesizedResult.Affects[Affect.kowa]) ++CountOfAffect[Affect.kowa];
                if (sentence.SynthesizedResult.Affects[Affect.haji]) ++CountOfAffect[Affect.haji];
                if (sentence.SynthesizedResult.Affects[Affect.iya]) ++CountOfAffect[Affect.iya];
                if (sentence.SynthesizedResult.Affects[Affect.takaburi]) ++CountOfAffect[Affect.takaburi];
                if (sentence.SynthesizedResult.Affects[Affect.odoroki]) ++CountOfAffect[Affect.odoroki];
            }
            double all = CountOfAffect[Affect.yorokobi] + CountOfAffect[Affect.suki] + CountOfAffect[Affect.yasu] + CountOfAffect[Affect.ikari] + CountOfAffect[Affect.aware] + CountOfAffect[Affect.kowa] + CountOfAffect[Affect.haji] + CountOfAffect[Affect.iya] + CountOfAffect[Affect.takaburi] + CountOfAffect[Affect.odoroki];
            #endregion
            #region 统计二元频率
            Dictionary<BiAffect, double> CountOfBiAffect = new Dictionary<BiAffect, double>();
            for (int i = 0; i < Variable.KindsOfLabel - 1; ++i)
            {
                for (int j = i + 1; j < Variable.KindsOfLabel - 1; ++j)
                {
                    foreach (Sentence sentence in Variable.Sentences)
                    {
                        BiAffect bi = new BiAffect(Variable.AffectArray[i], Variable.AffectArray[j]);
                        if (sentence.SynthesizedResult.Affects[bi.Affect1] && sentence.SynthesizedResult.Affects[bi.Affect2])
                        {
                            if (CountOfBiAffect.ContainsKey(bi))
                                ++CountOfBiAffect[bi];
                            else
                                CountOfBiAffect.Add(bi, 1);
                        }
                    }
                }
            }
            #endregion
            Dictionary<BiAffect, double> Mi3Dic = new Dictionary<BiAffect, double>();
            foreach (KeyValuePair<BiAffect, double> biAffect in CountOfBiAffect)
            {
                //Mi3Dic.Add(biAffect.Key, (32 / 2.99575052699864) * Math.Log(Math.Pow(biAffect.Value, 1) * all / (CountOfAffect[biAffect.Key.Affect1] * CountOfAffect[biAffect.Key.Affect2]), 2));//扩大的互信息
                Mi3Dic.Add(biAffect.Key, (32 / 12.7612852733616) * Math.Log(Math.Pow(biAffect.Value, 3) * all / (CountOfAffect[biAffect.Key.Affect1] * CountOfAffect[biAffect.Key.Affect2]), 2));//扩大的三次互信息
                //Mi3Dic.Add(biAffect.Key, Math.Pow(biAffect.Value, 3) / (CountOfAffect[biAffect.Key.Affect1] * CountOfAffect[biAffect.Key.Affect2]));
            }
            List<KeyValuePair<BiAffect, double>> sortedMi3 = new List<KeyValuePair<BiAffect, double>>(Mi3Dic);
            sortedMi3.Sort(delegate(KeyValuePair<BiAffect, double> s1, KeyValuePair<BiAffect, double> s2)
            {
                return s1.Value.CompareTo(s2.Value);
            });
            foreach (KeyValuePair<BiAffect, double> biAffect in sortedMi3)
            {
                Variable.ResultFile.WriteLine(affectToEnglish(biAffect.Key.Affect1) + "," + affectToEnglish(biAffect.Key.Affect2) + "," + biAffect.Value + "," + CountOfBiAffect[biAffect.Key]);
            }
        }
        static private string affectToEnglish(Affect affect)
        {
            Encoding encoding = Encoding.GetEncoding(932);
            switch (affect)
            {
                case Affect.yorokobi:
                    return "happiness";
                case Affect.suki:
                    return "fondness";
                case Affect.yasu:
                    return "relief";
                case Affect.ikari:
                    return "anger";
                case Affect.aware:
                    return "sadness";
                case Affect.kowa:
                    return "fear";
                case Affect.haji:
                    return "shame";
                case Affect.iya:
                    return "disgust";
                case Affect.takaburi:
                    return "excitement";
                case Affect.odoroki:
                    return "surprise";
                case Affect.mu:
                    return "neutral";
                default:
                    return "error";
            }
        }
        static private string affectToEnglishAbbreviation(Affect affect)
        {
            Encoding encoding = Encoding.GetEncoding(932);
            switch (affect)
            {
                case Affect.yorokobi:
                    return "H";
                case Affect.suki:
                    return "Fo";
                case Affect.yasu:
                    return "R";
                case Affect.ikari:
                    return "A";
                case Affect.aware:
                    return "Sa";
                case Affect.kowa:
                    return "Fe";
                case Affect.haji:
                    return "Sh";
                case Affect.iya:
                    return "D";
                case Affect.takaburi:
                    return "E";
                case Affect.odoroki:
                    return "Su";
                case Affect.mu:
                    return "N";
                default:
                    return "error";
            }
        }
        static private string affectToJapanese(Affect affect)
        {
            Encoding encoding = Encoding.GetEncoding(932);
            switch (affect)
            {
                case Affect.yorokobi:
                    return "喜";
                case Affect.suki:
                    return "好";
                case Affect.yasu:
                    return "安";
                case Affect.ikari:
                    return "怒";
                case Affect.aware:
                    return "哀";
                case Affect.kowa:
                    return "怖";
                case Affect.haji:
                    return "恥";
                case Affect.iya:
                    return "厭";
                case Affect.takaburi:
                    return "昂";
                case Affect.odoroki:
                    return "驚";
                case Affect.mu:
                    return "無";
                default:
                    return "error";
            }
        }

        static public void DisagreementWithEntropy()
        {
            int to25 = 0, to50 = 0, to75 = 0, to100 = 0, to125 = 0, to150 = 0, to175 = 0, to200 = 0, to225 = 0, to250 = 0, to275 = 0, to300 = 0;
                double average = 0;
            foreach (Sentence sentence in Variable.Sentences)
            {
                Dictionary<Affect, double> probability = new Dictionary<Affect, double>();
                foreach (Affect affect in Variable.AffectAndMuArray)
                {
                    probability.Add(affect, 0);
                }
                foreach (Annotator annotator in sentence.Annotators)
                {
                    foreach (Affect affect in Variable.AffectAndMuArray)
                    {
                        if (annotator.Affects[affect]) ++probability[affect];
                    }
                }
                double all = 0;//TODO 分母不对，应除以总人数（10）
                foreach (Affect affect in Variable.AffectAndMuArray)
                {
                    all += probability[affect];
                }
                foreach (Affect affect in Variable.AffectAndMuArray)
                {
                    probability[affect] /= all;
                }
                foreach (Affect affect in Variable.AffectAndMuArray)
                {
                    if (probability[affect] != 0)
                        sentence.Entropy -= probability[affect] * Math.Log(probability[affect], 2);
                }
                average += sentence.Entropy;
                if (sentence.Entropy > 0 && sentence.Entropy < 0.25)
                    ++to25;
                else if (sentence.Entropy >= 0.25 && sentence.Entropy < 0.5)
                    ++to50;
                else if (sentence.Entropy >= 0.5 && sentence.Entropy < 0.75)
                    ++to75;
                else if (sentence.Entropy >= 0.75 && sentence.Entropy < 1)
                    ++to100;
                else if (sentence.Entropy >= 1 && sentence.Entropy < 1.25)
                    ++to125;
                else if (sentence.Entropy >= 1.25 && sentence.Entropy < 1.5)
                    ++to150;
                else if (sentence.Entropy >= 1.5 && sentence.Entropy < 1.75)
                    ++to175;
                else if (sentence.Entropy >= 1.75 && sentence.Entropy < 2)
                    ++to200;
                else if (sentence.Entropy >= 2 && sentence.Entropy < 2.25)
                    ++to225;
                else if (sentence.Entropy >= 2.25 && sentence.Entropy < 2.5)
                    ++to250;
                else if (sentence.Entropy >= 2.5 && sentence.Entropy < 2.75)
                    ++to275;
                else if (sentence.Entropy >= 2.75 && sentence.Entropy < 3)
                    ++to300;
            }
            average /= Variable.NumberOfSentence;
            List<Sentence> sorted = new List<Sentence>(Variable.Sentences);
            sorted.Sort(delegate(Sentence s1, Sentence s2)
            {
                return s2.Entropy.CompareTo(s1.Entropy);
            });
            //foreach (Sentence sentence in sorted)
            //{
            //    Variable.ResultFile.WriteLine(sentence.ID + "," + sentence.Entropy);
            //}
        }

        static public void DistributionOfSentences()
        {
            foreach (Sentence sentence in Variable.Sentences)
            {
                foreach (Annotator annotator in sentence.Annotators)
                {
                    foreach (Affect affect in Variable.AffectAndMuArray)
                    {
                        if (annotator.Affects[affect])
                            sentence.SynthesizedResult.Affects[affect] = true;
                    }
                }
            }
            Dictionary<Affect, double> distributionOfSentences = new Dictionary<Affect, double>();
            foreach (Affect affect in Variable.AffectAndMuArray)
            {
                distributionOfSentences.Add(affect, 0);
            }
            foreach (Sentence sentence in Variable.Sentences)
            {
                foreach (Affect affect in Variable.AffectAndMuArray)
                {
                    if (sentence.SynthesizedResult.Affects[affect])
                        ++distributionOfSentences[affect];
                }
            }
            List<KeyValuePair<Affect, double>> sorted = new List<KeyValuePair<Affect, double>>(distributionOfSentences);
            sorted.Sort(delegate(KeyValuePair<Affect, double> s1, KeyValuePair<Affect, double> s2)
            {
                return s2.Value.CompareTo(s1.Value);
            });
            foreach (KeyValuePair<Affect, double> affect in sorted)
            {
                //Variable.ResultFile.WriteLine(affectToEnglishAbbreviation(affect.Key) + "," + affect.Value + "," + affect.Value / Variable.NumberOfSentence);
                Variable.ResultFile.WriteLine(affectToEnglish(affect.Key) + "," + affect.Value + "," + affect.Value / Variable.NumberOfSentence);
            }
        }
    }
}