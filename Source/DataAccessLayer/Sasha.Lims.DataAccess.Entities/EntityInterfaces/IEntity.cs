namespace Sasha.Lims.DataAccess.Entities.Entity
{
	/// <summary>
	/// For all business objects base record necessary implement IEntity interface
	/// </summary>
	public interface IEntity
	{
		int id { get; set; }
	}
	public interface IStatusedEntity : IEntity
	{
		int? status_id { get; set; }
	}
	public partial class wf_process : IStatusedEntity
	{
	}
	public partial class wf_run : IStatusedEntity
	{
	}
	public partial class a_prop : IEntity
	{
	}
	public partial class a_userField : IEntity
	{
		public bool Inherited { get { return (inherited ?? 0) > 0; } }
		public bool ShowOnForm
		{
			get { return (showInInterface ?? 0) > 0; }
			set { showInInterface = value ? 1 : 0; }
		}
		public bool ShowInGrid
		{
			get { return (showInGrid ?? 0) > 0; }
			set { showInGrid = value ? 1 : 0; }
		}

	}
	public partial class a_userFieldList : IEntity
	{
	}

	public partial class vReagent : IEntity
	{
		public int id
		{
			get { return Id; }
			set { Id = value; }
		}
	}
	public partial class vReagentType : IEntity
	{
		public int id
		{
			get { return Id; }
			set { Id = value; }
		}
	}
	public partial class vReagentLog : IEntity
	{
		public int id
		{
			get { return Id; }
			set { Id = value; }
		}
	}

	public partial class vProperty : IEntity
	{
		public int id
		{
			get { return Id; }
			set { Id = value; }
		}
	}
	public partial class vHospital : IEntity
	{
		public int id
		{
			get { return Id; }
			set { Id = value; }
		}
	}
	public partial class c_client : IEntity
	{
	}
	public partial class vPropLocation : IEntity
	{
	}
	public partial class s_clones : IEntity
	{
	}
	public partial class a_userLayout : IEntity
	{
	}
	public partial class a_file : IEntity
	{
	}
	public partial class wf_bom : IEntity
	{
	}
	public partial class c_patient : IEntity
	{
	}
	public partial class a_comment : IEntity
	{
	}

	public partial class vPatient : IEntity
	{
	}


	public partial class s_jourLine : IEntity
	{
		public s_jourLine()
		{

		}

		public override string ToString()
		{
			return this.barcode + " " + this.name;
		}

	}
	public partial class vSamplesBook : IEntity
	{
		public int id
		{
			get { return Id; }
			set { Id = value; }
		}
	}
	public partial class vSamplesBookInProcess : IEntity
	{
		public int id
		{
			get { return Id; }
			set { Id = value; }
		}
	}
	public partial class vContaiterDraft : IEntity
	{
		public int id
		{
			get { return Id; }
			set { Id = value; }
		}
	}
	public partial class vContaiter : IEntity
	{
		public int id
		{
			get { return Id; }
			set { Id = value; }
		}
	}
	public partial class vCustodyLog : IEntity
	{
		public int id
		{
			get { return Id; }
			set { Id = value; }
		}
	}

	public partial class c_doctor : IEntity
	{

	}
	public partial class vDoctor : IEntity
	{
		public int id
		{
			get { return Id; }
			set { Id = value; }
		}
	}

	public partial class vBillOfMaterial : IEntity
	{
		public int id
		{
			get { return Id; }
			set { Id = value; }
		}
	}
	public partial class vJourLine : IEntity
	{
		public int id
		{
			get { return Id; }
			set { Id = value; }
		}
	}
	public partial class wf_result : IEntity
	{
	}
	public partial class s_sample : IEntity
	{
	}
	public partial class r_reagent : IEntity
	{
	}
	public partial class r_reagentType : IEntity
	{
	}
	public partial class r_reagentTypeLink : IEntity
	{
	}
	public partial class r_reagentLedger : IEntity
	{
	}
	public partial class a_userData : IEntity
	{
	}
	public partial class a_userAddData : IEntity
	{
	}
	public partial class s_custodyLog : IEntity
	{
	}
	public partial class vCustomData : IEntity
	{
		public int id
		{
			get { return Id; }
			set { Id = value; }
		}
	}

	public partial class vQuestionnaire : IEntity
	{
		public int id
		{
			get { return Id; }
			set { Id = value; }
		}
	}


	public partial class h_item : IEntity
	{
	}

	public partial class testUI : IEntity
	{

	}
}