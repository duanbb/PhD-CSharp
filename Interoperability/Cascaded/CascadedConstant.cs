using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interoperability.Entity;

namespace Interoperability.Cascaded
{
    static class CascadedConstant
    {
        static public IDictionary<TargetLabeltruth, double> Pr_t;

        static public IDictionary<SourceLabeltruth, IDictionary<TargetLabeltruth, double>> Pr_t_s;

        static public IDictionary<SourceAnnotation, IDictionary<TargetAnnotation, double>> Pr_T_S;

        static public IDictionary<TargetLabeltruth, IDictionary<TargetAnnotation, double>> Pr_T_t;
    }
}