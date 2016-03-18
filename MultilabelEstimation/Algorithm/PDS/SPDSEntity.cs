using MultilabelEstimation.Supervised;

namespace MultilabelEstimation.Algorithm.PDS
{
    class SPDSVariable
    {
        static public Sij TrainingSij;
        static public Sij Sij;
        static public Pj Pj;
        static public PAkjl PAkjl;
        static public Pj TrainingPj;
        static SPDSVariable()
        {
            TrainingSij = new Sij(0);
            TrainingPj = new Pj(0);
        }
    }
}