using System.Collections.Generic;
using System.Linq;
using OSPSuite.Utility.Collections;
using OSPSuite.Utility.Extensions;
using OSPSuite.Core.Domain.Services;

namespace PKSim.Core.Model
{
   public class ProcessSelectionGroup
   {
      private readonly List<ProcessSelection> _partialProcesses;
      private readonly ICache<SystemicProcessType, SystemicProcessSelection> _systemicProcesses;
      public string ProductNameTemplate { get; private set; }

      public ProcessSelectionGroup(string productNameTemplate)
      {
         _partialProcesses = new List<ProcessSelection>();
         _systemicProcesses = new Cache<SystemicProcessType, SystemicProcessSelection>(x => x.ProcessType, x => null);
         ProductNameTemplate = productNameTemplate;
      }

      public void AddPartialProcessSelection(ProcessSelection processSelection)
      {
         _partialProcesses.Add(processSelection);
      }

      public void RemovePartialProcessSelection(ProcessSelection processSelection)
      {
         _partialProcesses.Remove(processSelection);
      }

      public void AddSystemicProcessSelection(SystemicProcessSelection systemicProcessSelection)
      {
         if (_systemicProcesses.Contains(systemicProcessSelection.ProcessType))
            _systemicProcesses.Remove(systemicProcessSelection.ProcessType);
         _systemicProcesses.Add(systemicProcessSelection);
      }

      public IEnumerable<IReactionMapping> AllEnabledProcesses()
      {
         return AllEnabledPartialProcesses().Union(AllEnabledSystemicProcesses().Cast<IReactionMapping>());
      }

      public IEnumerable<string> AllInducedMoleculeNames(IndividualMolecule molecule)
      {
         var processesInducedByProtein = AllEnabledProcesses().Where(ps => string.Equals(ps.MoleculeName, molecule.Name));
         var allInducedMolecules = new List<string> {molecule.Name};
         allInducedMolecules.AddRange(processesInducedByProtein.Select(ps => ps.ProductName(ProductNameTemplate)));
         return allInducedMolecules;
      }

      /// <summary>
      ///    Returns the distinct name of all well defined molecules that enable a process (Enzyme, Transport or Protein name)
      /// </summary>
      public IEnumerable<string> AllEnablingMoleculeNames()
      {
         return AllEnabledProcesses().Select(x => x.MoleculeName).Distinct();
      }

      public IReadOnlyList<ProcessSelection> AllPartialProcesses()
      {
         return _partialProcesses;
      }

      public IEnumerable<ProcessSelection> AllEnabledPartialProcesses()
      {
         return _partialProcesses.Where(x => !string.IsNullOrEmpty(x.ProcessName));
      }

      public IEnumerable<SystemicProcessSelection> AllEnabledSystemicProcesses()
      {
         return _systemicProcesses.Where(x => !string.IsNullOrEmpty(x.ProcessName));
      }

      public IEnumerable<SystemicProcessSelection> AllSystemicProcesses()
      {
         return _systemicProcesses;
      }

      /// <summary>
      ///    returns the systemic process for the given type if it was defined and mapped
      ///    otherwise null
      /// </summary>
      public SystemicProcessSelection ProcessSelectionFor(SystemicProcessType systemicProcessType)
      {
         var process = _systemicProcesses[systemicProcessType];
         if (process == null) return null;
         return process.ProcessName.IsNullOrEmpty() ? null : process;
      }

      public bool IsEmpty
      {
         get { return !_systemicProcesses.Any() && _partialProcesses.Count == 0; }
      }

      public ProcessSelectionGroup Clone(ICloneManager cloneManager)
      {
         var clone = new ProcessSelectionGroup(ProductNameTemplate);
         _partialProcesses.Each(x => clone.AddPartialProcessSelection(x.Clone(cloneManager)));
         _systemicProcesses.Each(x => clone.AddSystemicProcessSelection(x.Clone(cloneManager)));
         return clone;
      }

      public void ClearProcesses()
      {
         _partialProcesses.Clear();
         _systemicProcesses.Clear();
      }
   }
}