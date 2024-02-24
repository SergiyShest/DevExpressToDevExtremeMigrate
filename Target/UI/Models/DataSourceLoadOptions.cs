//using DevExtreme.AspNet.Data.Helpers;
//using System.Web.Mvc;
using System;
using DevExtreme.AspNet.Data.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace DevExtreme.AspNet.Data {

	[ModelBinder(BinderType = typeof(DataSourceLoadOptionsBinder))]
	public class DataSourceLoadOptions : DataSourceLoadOptionsBase { }

	public class DataSourceLoadOptionsBinder : IModelBinder
	{

		public Task BindModelAsync(ModelBindingContext bindingContext)
		{
			var loadOptions = new DataSourceLoadOptions();
			DataSourceLoadOptionsParser.Parse(loadOptions, key => bindingContext.ValueProvider.GetValue(key).FirstOrDefault());
			bindingContext.Result = ModelBindingResult.Success(loadOptions);
			return Task.CompletedTask;
		}

	}


}

