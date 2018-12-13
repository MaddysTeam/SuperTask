using System;
using System.Collections.Generic;

namespace TheSite.EvalAnalysis
{

   public abstract class EvalEngine
   {

      public abstract double FullScore { get; }

      public abstract string AnalysisKey { get; }

      public abstract string AnalysisName { get; }

      public abstract Dictionary<Guid,AnalysisUnit> SubjectEvals { get; set; }

      public abstract Dictionary<Guid,AnalysisUnit> AutoEvals { get; set; }

   }


   public partial class MonthlyEvalEngine : EvalEngine
   {

      public const string ViewPath = "../EvalModels/Monthly/";

      public override string AnalysisKey => "Monthly V1.0";

      public override string AnalysisName => "月度考核";

      public override double FullScore => 100;

      /// <summary>
      /// Guid means tableId,each table has one analysisUnit
      /// </summary>
      public override Dictionary<Guid, AnalysisUnit> SubjectEvals { get; set; }

      public override Dictionary<Guid, AnalysisUnit> AutoEvals { get; set; }

   }


   public partial class QuarteryEvalEngine : EvalEngine
   {
      public const string ViewPath = "../EvalModels/Quartery/";

      public override string AnalysisKey => "Quartery V1.0";

      public override string AnalysisName => "季度考核";

      public override double FullScore => 100;


      /// <summary>
      /// Guid means tableId,each table has one analysisUnit
      /// </summary>
      public override Dictionary<Guid, AnalysisUnit> SubjectEvals { get; set; }

      public override Dictionary<Guid, AnalysisUnit> AutoEvals { get; set; }

   }


   public partial class HalfYearEvalEngine: EvalEngine
   {
      public const string ViewPath = "../EvalModels/HalfYear/";

      public override string AnalysisKey => "HalfYear V1.0";

      public override string AnalysisName => "半年考核";

      public override double FullScore => 100;


      /// <summary>
      /// Guid means tableId,each table has one analysisUnit
      /// </summary>
      public override Dictionary<Guid, AnalysisUnit> SubjectEvals { get; set; }

      public override Dictionary<Guid, AnalysisUnit> AutoEvals { get; set; }
   }


   public partial class FullYearEvalEngine : EvalEngine
   {
      public const string ViewPath = "../EvalModels/FullYear/";

      public override string AnalysisKey => "FullYear V1.0";

      public override string AnalysisName => "整年考核";

      public override double FullScore => 100;


      /// <summary>
      /// Guid means tableId,each table has one analysisUnit
      /// </summary>
      public override Dictionary<Guid, AnalysisUnit> SubjectEvals { get; set; }

      public override Dictionary<Guid, AnalysisUnit> AutoEvals { get; set; }
   }

}  