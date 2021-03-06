using System.Collections.Generic;
using System.Linq;
using OSPSuite.Utility.Extensions;
using OSPSuite.Core.Domain;
using OSPSuite.Core.Domain.Services;

namespace PKSim.Core.Model
{
   public interface ITransporterExpressionContainer : IMoleculeExpressionContainer, ITransporterContainer
   {
      string OrganName { get; }
      string CompartmentName { get; set; }
      bool HasPolarizedMembrane { get; }
      void UpdatePropertiesFrom(TransporterContainerTemplate transporterContainerTemplate);
   }

   public class TransporterExpressionContainer : MoleculeExpressionContainer, ITransporterExpressionContainer
   {
      private MembraneLocation _membraneLocation;
      public string CompartmentName { get; set; }
      private readonly IList<string> _allProcessNames = new List<string>();

      public IEnumerable<string> ProcessNames
      {
         get { return _allProcessNames; }
      }

      public void AddProcessName(string processName)
      {
         _allProcessNames.Add(processName);
      }

      public void ClearProcessNames()
      {
         _allProcessNames.Clear();
      }

      public MembraneLocation MembraneLocation
      {
         get { return _membraneLocation; }
         set
         {
            _membraneLocation = value;
            OnPropertyChanged(() => MembraneLocation);
         }
      }

      public string OrganName
      {
         get { return Name; }
      }

      public bool HasPolarizedMembrane
      {
         get
         {
            if (CoreConstants.Organ.PolarizedMembraneOrgans.Contains(OrganName))
               return true;

            return string.Equals(GroupName, CoreConstants.Groups.GI_MUCOSA);
         }
      }

      public void UpdatePropertiesFrom(TransporterContainerTemplate transporterContainerTemplate)
      {
         updatePropertiesFrom(transporterContainerTemplate);
         CompartmentName = transporterContainerTemplate.CompartmentName;
      }

      private void updatePropertiesFrom(ITransporterContainer transporterContainer)
      {
         MembraneLocation = transporterContainer.MembraneLocation;
         _allProcessNames.Clear();
         transporterContainer.ProcessNames.Each(AddProcessName);
      }

      public override void UpdatePropertiesFrom(IUpdatable sourceObject, ICloneManager cloneManager)
      {
         base.UpdatePropertiesFrom(sourceObject, cloneManager);
         var sourceTransporterContainer = sourceObject as ITransporterExpressionContainer;
         if (sourceTransporterContainer == null) return;
         updatePropertiesFrom(sourceTransporterContainer);
         CompartmentName = sourceTransporterContainer.CompartmentName;
      }
   }
}