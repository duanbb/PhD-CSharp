using MultilabelEstimation.Supervised;

namespace MultilabelEstimation.Algorithm.DDS.JDDS
{
    static class SDDSVariable
    {
        static public Sij TrainingSij;
        static public Sij Sij;
        static public Pj Pj;
        static public PAkjl PAkjl;
        static public Pj TrainingPj;
        static SDDSVariable()
        {
            TrainingSij = new Sij(0);
            TrainingPj = new Pj(0);
        }
    }
}