using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using ChoETL;
using Newtonsoft.Json;
using Sasha.Lims.Core;
using Sasha.Lims.Core.BusinessObjects;
using Sasha.Lims.Core.DTO.Access;
using Sasha.Lims.Core.Helpers;
using Sasha.Lims.Core.Infrastructure.Exceptions;
using Sasha.Lims.DataAccess.Entities.Constants;
using Sasha.Lims.DataAccess.Entities.Entity;

namespace GroupChange
{


	/// <summary>
	/// общий класс для редактирования объектов на основе Entity.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class EditModelEntity<T> : EditModelBase<T> where T : class, IEntity, new()
	{

		protected override T getObject(int? id)
		{
			T result = null;
			if (id == null || id == 0)
			{
				result = new T();
			} else
			{
				result = _uow.Get<T>((int) id);
			}
			return result;
		}

		PropsType propsType;

		#region Constructor


		/// <summary>
		/// 
		/// </summary>
		/// <param name="idsStr">Ids string, if empty mean create new</param>
		/// <param name="uow"></param>
		///  <param name="prObectTable">used for custom field</param>
		public EditModelEntity(int? id, TableType prObectTable) : base(id, (int) prObectTable)
		{
			switch (prObectTable)
			{
				case TableType.Sample: propsType = PropsType.SampleEditSettings; break;
				}
		}
		#endregion

		public override List<IField> SettingList
		{
			get
			{
				if (_settingList == null || _settingList.Count == 0)
				{
					var typeDescr = DataHelper.Current.GetTypeDescription(TypeId);
					_settingList = new List<IField>();
					foreach (var fieldDescription in typeDescr.Fields)
					{
						_settingList.Add(fieldDescription);
					}
				}
				return _settingList;
			}
		}

		public override List<ObjectError> Save(UserDTO user)
		{
			var res = new List<ObjectError>();
			try
			{
				_uow.GetRepository<T>().Save(Object);
				_id = Object.id;
				if (_addPropertyObject != null)
				{
					_addPropertyObject.aspNetUser_Id = user.Id;
					_addPropertyObject.row_id = Object.id;
					if (_addPropertyObject.createdDateTime == null)
						_addPropertyObject.createdDateTime = DateTime.Now;
					_uow.GetRepository<a_userData>().Save(_addPropertyObject);
				}
				foreach (var prop in base.PropertyList)
				{
					prop.Save(user,Object.id);
				}
			}
			catch (Exception ex)
			{
				res.Add(new ObjectError(Object.GetType().Name, ex.GetAllMessages()));
			}
			return res;
		}


		public override List<ObjectError> Delete()
		{
			var res = new List<ObjectError>();
			try
			{
				_uow.GetRepository<T>().Delete(Object.id);

			}
			catch (Exception ex)
			{
				res.Add(new ObjectError(Object.GetType().Name, ex.GetAllMessages()));
			}
			return res;
		}

	}




}
