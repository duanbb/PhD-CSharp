using MultilabelEstimation.Supervised;

namespace MultilabelEstimation.Algorithm.IDS
{
    static class SIDSVariable
    {
        static public Sij TrainingSij;
        static public PAkjl PAkjl;
        static public Pj Pj;
        static public Sij Sij;
        static public Pj TrainingPj;
        static SIDSVariable()
        {
            TrainingSij = new Sij(0);
            TrainingPj = new Pj(0);
        }
    }
}