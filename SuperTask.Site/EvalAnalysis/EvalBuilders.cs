using Business;
using Business.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using TheSite.Models;

namespace TheSite.EvalAnalysis
{

   public interface IEvalIndicationBuilder
   {

      IEnumerable<EvalIndicationItem> AutoBuildEvalItem(Guid evalIndicationId, string itemName,double fullScore,int count);

      double GetFullScore(IEnumerable<EvalIndication> evalIndications );

   }

   public interface IIndicationAlgorithmn<Paras,Result>
   {
      Guid Id { get; }

      string Name { get; }

      Func<Paras, Result> Algorithmn { get; }
   }

   public interface IAutoEvalProcessBuilder<Paras, Result>
   {
      void BuildAutoEvalProcess(Guid indicationId,IIndicationAlgorithmn<Paras, Result> algorithmn);
   }


   public class EvalIndicationBuilder : IEvalIndicationBuilder
   {
      public virtual IEnumerable<EvalIndicationItem> AutoBuildEvalItem(Guid evalIndicationId,string name, double fullScore, int count)
      {
         var gap =fullScore / count;

         for (int i = 0; i < count; i++)
         {
            var score = fullScore - (gap * i);
            yield return new EvalIndicationItem
            {
               EvalIndicationId = evalIndicationId,
               ItemId = Guid.NewGuid(),
               ItemName =string.IsNullOrEmpty(name)? "指标"+i:name,
               ItemScore = score < 0 ? 0 : score
            };
         }
      }

      public virtual double GetFullScore(IEnumerable<EvalIndication> evalIndications)
      {
         return evalIndications.Sum(e => e.FullScore) / evalIndications.GroupBy(x => x.AccessorRoleId).Count();
      }
   }


   /// <summary>
   /// 自动考核建造者
   /// </summary>
   public class AutoEvalBuilder : EvalIndicationBuilder, IAutoEvalProcessBuilder<AutoEvalParams, EvalResultItem>
   {
      static Dictionary<Guid, IIndicationAlgorithmn<AutoEvalParams, EvalResultItem>> algorithmns=new Dictionary<Guid, IIndicationAlgorithmn<AutoEvalParams, EvalResultItem>>();

      public IReadOnlyDictionary<Guid, IIndicationAlgorithmn<AutoEvalParams, EvalResultItem>> Algorithmns => algorithmns;

      public void BuildAutoEvalProcess(Guid indicationId, IIndicationAlgorithmn<AutoEvalParams, EvalResultItem> algorithmn)
      {
         if (indicationId.IsEmpty() || algorithmn==null) throw new ArgumentNullException();

         if (algorithmns.ContainsKey(indicationId)) return;
         else
            algorithmns.Add(indicationId, algorithmn);
      }

      public override IEnumerable<EvalIndicationItem> AutoBuildEvalItem(Guid evalIndicationId, string itemName, double fullScore, int count)
      {
         yield return new EvalIndicationItem
         {
            EvalIndicationId = evalIndicationId,
            ItemId = Guid.NewGuid(),
            ItemName = string.IsNullOrEmpty(itemName) ? "自动指标" : itemName,
            ItemScore = fullScore
         };
      }
   }


   /// <summary>
   /// 主观考核建造者
   /// </summary>
   public class SubjectEvalBuilder : EvalIndicationBuilder
   {
   }


   /// <summary>
   /// 管理者
   /// </summary>
   public static class BuilderManager
   {
      public static Dictionary<int, IEvalIndicationBuilder> Builders = new Dictionary<int, IEvalIndicationBuilder>
      {
         {IndicationKeys.SubjectType,new SubjectEvalBuilder() },
         {IndicationKeys.AutoType,new AutoEvalBuilder() }
      };
   }

}
