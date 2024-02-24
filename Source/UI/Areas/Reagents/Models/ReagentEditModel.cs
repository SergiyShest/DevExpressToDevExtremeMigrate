using System;
using System.Collections.Generic;
using System.Linq;
using Sasha.Lims.Core;
using Sasha.Lims.Core.BusinessObjects;
using Sasha.Lims.Core.DTO.Access;
using Sasha.Lims.Core.Helpers;
using Sasha.Lims.DataAccess.Entities.Constants;
using Sasha.Lims.DataAccess.Entities.Entity;

namespace GroupChange
{
	public class ReagentEditModel : EditModelBase<ReagentBo>
	{

		#region SettingList 

		/// <summary>
		/// 
		/// </summary>
		public override List<IField> SettingList
		{
			get
			{
				var settingList = base.SettingList;
				ChangeSetting(settingList, nameof(ReagentBo.TypeName), "Name", Control.TextField);
				ChangeSetting(settingList, nameof(ReagentBo.VendorId), "Vendor", Control.ComboBox, DataHelper.Current.ReagentVendors);
				ChangeSetting(settingList, nameof(ReagentBo.ReagentTypeName), "Reagent Type", Control.TextField);

				var setting = ChangeSetting(settingList, nameof(ReagentBo.LocationId), "Location", Control.TextFieldWithButtonSerch);
				setting.ListName = "LocationId";


				var unitSettig = ChangeSetting(settingList, nameof(ReagentBo.UnitId), "Unit", Control.ComboBox, DataHelper.Current.QuantityUnits);
				settingList.Remove(unitSettig);
				var initQuantitySetting = ChangeSetting(settingList, nameof(ReagentBo.InitialQuantity), "Initial Quantity", Control.DoubleTextField);
				var quantitySetting = ChangeSetting(settingList, nameof(ReagentBo.Quantity), "Quantity", Control.DoubleTextField);

				initQuantitySetting.PairSetting = unitSettig;
				quantitySetting.PairSetting = unitSettig;


				settingList.Remove(initQuantitySetting);
				var index = settingList.IndexOf(quantitySetting);
				settingList.Insert(index, initQuantitySetting);//move initQuantity  beefore Quantity


				return settingList;
			}

		}

		private static IField ChangeSetting(List<IField> settingList, string name, string text, Control controlType, List<NamedInt> avaiableValues = null)
		{
			var setting = settingList.FirstOrDefault(x => x.Name == name);
			if (setting != null)
			{
				setting.ControlType = controlType;
				setting.Text = text;
				setting.AvaiableValues = avaiableValues;

			}

			return setting;
		}

		#endregion
		int? _reagentTypeId;

		public ReagentEditModel(int? id, int? reagentTypeId = null) : base(id, (int) TableType.Reagent)
		{
			if (id == null && reagentTypeId == null)
			{
				throw new ApplicationException("For create reagent id or reagentTypeId mast have value!");
			}
			_reagentTypeId = reagentTypeId;
		}

		protected override ReagentBo getObject(int? id)
		{
			if (id == null)
			{
				return new ReagentBo() { ReagentTypeId = (int) _reagentTypeId };

			} else
			{
				var reagentRecord = DataHelper.Current.UnitOfWorkEx.GetRepository<r_reagent>().FirstOrDefault(x => x.id == id);
				return new ReagentBo(reagentRecord);
			}
		}


		public override List<ObjectError> Save(UserDTO user)
		{
			var res = new List<ObjectError>();
			try
			{
				Object.Save(user);
			}
			catch (Exception ex)
			{
				res.Add(new ObjectError(Object.GetType().Name, ex.GetAllMessages()));
			}
			return res;
		}

		internal List<ObjectError> Copy(UserDTO userDTO)
		{
			var exampl = Object;

			Object = new ReagentBo()
			{

				Barcode = exampl.Barcode,
				Quantity = exampl.Quantity,

			};
			Object.Record.reagentType_id = exampl.Record.reagentType_id;
			Object.Record.parent_id = null;
			return Save(userDTO);
		}
	}

}





