using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using Sasha.Lims.Core;
using Sasha.Lims.Core.BusinessObjects;
using Sasha.Lims.Core.DTO.Access;
using Sasha.Lims.Core.Helpers;
using Sasha.Lims.DataAccess.Entities.Constants;
using Sasha.Lims.DataAccess.Entities.Entity;

namespace GroupChange
{
	public class ReagentTypeEditModel : EditModelBase<ReagentTypeBo>
	{

		//public ReagentTypeBo ReagentType
		//{ get
		//	{
		//		return base.Object;
		//	}
		//}




		#region SettingList 

		/// <summary>
		/// 
		/// </summary>
		public override List<IField> SettingList
		{
			get
			{
				var settingList = base.SettingList;
				ChangeSetting(settingList, nameof(ReagentTypeBo.TypeId), "Type", Control.ComboBox, DataHelper.Current.КitTypes);
				ChangeSetting(settingList, nameof(ReagentTypeBo.VendorId), "Vendor", Control.ComboBox, DataHelper.Current.ReagentVendors);
				ChangeSetting(settingList, nameof(ReagentTypeBo.StorageTempId), "Storage Temperature", Control.ComboBox, DataHelper.Current.StorageTemperature);

				var unitSettig = ChangeSetting(settingList, nameof(ReagentTypeBo.UnitId), "Unit", Control.ComboBox, DataHelper.Current.QuantityUnits);
				settingList.Remove(unitSettig);

				var quantitySetting = ChangeSetting(settingList, nameof(ReagentTypeBo.Quantity), "Quantity", Control.DoubleTextField);

				quantitySetting.PairSetting = unitSettig;


				//var sizeX_Y = ChangeSetting(settingList, nameof(ReagentTypeBo.SizeX), "Size X - Y ", ControlType.DoubleTextField);
    //            var sizeY=ChangeSetting(settingList, nameof(ReagentTypeBo.SizeY), "", ControlType.TextField);
				//sizeX_Y.PairSetting = sizeY;
				//settingList.Remove(sizeY);



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


		public ReagentTypeEditModel(int? id) : base(id, (int) TableType.ReagentType)
		{

		}

		protected override ReagentTypeBo getObject(int? id)
		{
			if (id == null)
			{
				return new ReagentTypeBo();
			} else
			{
				var reagentRecord = DataHelper.Current.UnitOfWorkEx.GetRepository<r_reagentType>().FirstOrDefault(x => x.id == id);
				if (reagentRecord.type_id == (int) ReagentTypes.SingleReagent)
				{
					return new ReagentTypeBo(reagentRecord);
				} else
				{
					return new KitOfReagentsType(reagentRecord);
				}
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

			Object = new ReagentTypeBo()
			{
				Name = exampl.Name,
				Barcode = exampl.Barcode,
				Quantity = exampl.Quantity,
				TypeId = exampl.TypeId,
				StorageTempId = exampl.StorageTempId,
				UnitId = exampl.UnitId,
				VendorId = exampl.VendorId,
				SizeY = exampl.SizeY,
				SizeX = exampl.SizeX,
			};
			return Save(userDTO);
		}
	}





}
