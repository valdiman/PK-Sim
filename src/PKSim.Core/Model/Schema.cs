using System;
using System.Collections.Generic;
using System.Linq;
using OSPSuite.Utility.Extensions;
using OSPSuite.Core.Domain;
using OSPSuite.Core.Domain.Services;

namespace PKSim.Core.Model
{
   public class Schema : Container
   {
      private double lastStartTime
      {
         get
         {
            if (!SchemaItems.Any())
               return 0;
            return SchemaItems.Select(si => si.StartTime.Value).Max();
         }
      }

      private double firstStartTime
      {
         get
         {
            if (!SchemaItems.Any())
               return 0;
            return SchemaItems.Select(si => si.StartTime.Value).Min();
         }
      }

      private double intervalTimeSpan
      {
         get { return lastStartTime - firstStartTime; }
      }

      /// <summary>
      ///    repetion length is the max betweed time span between the first and the last schema item
      ///    and the time between repetitions
      /// </summary>
      private double oneRepetionLength
      {
         get { return Math.Max(intervalTimeSpan, TimeBetweenRepetitions.Value); }
      }

      public virtual IEnumerable<ISchemaItem> SchemaItems
      {
         get { return GetChildren<ISchemaItem>(); }
      }

      public virtual IParameter StartTime
      {
         get { return this.Parameter(Constants.Parameters.START_TIME); }
      }

      public virtual IParameter TimeBetweenRepetitions
      {
         get { return this.Parameter(CoreConstants.Parameter.TIME_BETWEEN_REPETITIONS); }
      }

      public virtual IParameter NumberOfRepetitions
      {
         get { return this.Parameter(CoreConstants.Parameter.NUMBER_OF_REPETITIONS); }
      }

      public virtual void AddSchemaItem(ISchemaItem schemaItem)
      {
         Add(schemaItem);
      }

      public virtual IEnumerable<ISchemaItem> ExpandedSchemaItems(ICloneManager cloneManager)
      {
         {
            double offset = TimeBetweenRepetitions.Value;
            for (int repetition = 0; repetition < NumberOfRepetitions.Value; repetition++)
            {
               foreach (var schemaItem in SchemaItems)
               {
                  var expandedItem = cloneManager.Clone(schemaItem);
                  expandedItem.StartTime.Value += StartTime.Value + repetition * offset;
                  yield return expandedItem;
               }
            }
         }
      }

      public virtual void ClearSchemaItems()
      {
         SchemaItems.ToList().Each(RemoveChild);
      }

      public virtual double Duration
      {
         get { return StartTime.Value + oneRepetionLength * NumberOfRepetitions.Value; }
      }

      public virtual double EndTime
      {
         get
         {
            double endTime = Duration;
            double lastLagTime = TimeBetweenRepetitions.Value - lastStartTime;

            if (lastLagTime > 0)
               //do not count the last lag between repetitions
               endTime -= lastLagTime;

            return endTime;
         }
      }
   }
}