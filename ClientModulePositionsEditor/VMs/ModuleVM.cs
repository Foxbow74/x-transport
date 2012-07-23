using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Data;
using AlphaStudioCore;
using AlphaXTransport;
using ClientCommonWpf;
using XTransport;

namespace ClientModulePositionsEditor.VMs
{
	internal class ModuleVM : AlphaNamedVM
	{
		private string m_instrumentFilterText;

		[X("INSTRUMENTS")]
		private ICollection<PortfolioInstrumentVM> m_slots;
		private string m_slotsFilterText;

		public ReadOnlyObservableCollection<AbstractDerivativeVM> Instruments { get; private set; }
		public ReadOnlyObservableCollection<PortfolioInstrumentVM> Slots { get; private set; }

		public override string DocumentTitle
		{
			get { return base.DocumentTitle + " positions"; }
		}

		public override EAlphaKind Kind
		{
			get { return EAlphaKind.PORTFOLIO; }
		}

		public RelayCommand AddCommand { get; private set; }
		public RelayCommand RemoveCommand { get; private set; }

		protected override void InstantiationFinished()
		{
			base.InstantiationFinished();
			var gnrl = AlphaClient.Instance.GetRoot<GeneralVM>();

			Instruments = gnrl.DerivativeVMs;
			Slots = CreateObservableCollection(m_slots);

			CollectionViewSource.GetDefaultView(Instruments).Filter = InstrumentsFilter;
			CollectionViewSource.GetDefaultView(Slots).Filter = SlotsFilter;

			CollectionViewSource.GetDefaultView(Slots).CollectionChanged += SlotsCollectionChanged;

			AddCommand = new RelayCommand(ExecuteAdd, _o => Instruments.Any(_vm => _vm.Selected && InstrumentsFilter(_vm)));
			RemoveCommand = new RelayCommand(ExecuteRemove, _o => Slots.Any(_vm => _vm.Selected && SlotsFilter(_vm)));
		}

		void SlotsCollectionChanged(object _sender, NotifyCollectionChangedEventArgs e)
		{
			CollectionViewSource.GetDefaultView(Instruments).Refresh();
		}

		public override void ViewCreated()
		{
			base.ViewCreated();
			RefreshCollections();
		}

		private void RefreshCollections()
		{
			CollectionViewSource.GetDefaultView(Instruments).Refresh();
			CollectionViewSource.GetDefaultView(Slots).Refresh();
		}

		private void ExecuteRemove(object _obj)
		{
			using (new BatchChanges(this))
			{
				var slots = Slots.Where(_vm => _vm.Selected && SlotsFilter(_vm)).ToArray();
				foreach (var slot in slots)
				{
					slot.Selected = false;
					m_slots.Remove(slot);
				}
			}
			RefreshCollections();
		}

		private void ExecuteAdd(object _obj)
		{
			using (new BatchChanges(this))
			{
				var abstractDerivativeVms = Instruments.Where(_vm => _vm.Selected && InstrumentsFilter(_vm)).ToArray();
				foreach (var derivativeVM in abstractDerivativeVms)
				{
					derivativeVM.Selected = false;
					var slot = AlphaClient.Instance.Join(new PortfolioInstrumentVM());
					slot.Derivative = derivativeVM;
					m_slots.Add(slot);
				}
			}
			RefreshCollections();
		}

		#region filtration

		public string InstrumentFilterText
		{
			get { return m_instrumentFilterText; }
			set
			{
				m_instrumentFilterText = value;
				RefreshCollections();
			}
		}

		public string SlotsFilterText
		{
			get { return m_slotsFilterText; }
			set
			{
				m_slotsFilterText = value;
				RefreshCollections();
			}
		}

		private bool SlotsFilter(object _obj)
		{
			return m_slotsFilterText.CheckPattern(_obj);
		}

		private bool InstrumentsFilter(object _obj)
		{
			var allowedInstr = (AbstractDerivativeVM) _obj;
			foreach (var slot in Slots)
			{
				if (slot.Derivative == allowedInstr)
				{
					return false;
				}
			}
			return m_instrumentFilterText.CheckPattern(_obj);
		}

		#endregion
	}
}