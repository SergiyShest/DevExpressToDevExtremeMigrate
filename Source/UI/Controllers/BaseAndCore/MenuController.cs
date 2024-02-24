using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.UI;
using Sasha.Lims.Core.Interfaces.Common;
using Sasha.Lims.DataAccess.Entities.Constants;

namespace Sasha.Lims.WebUI.Controllers.BaseAndCore
{
    public class MenuController : BaseController
	{
		private IPropertiesService _propertiesService;

		public MenuController(IPropertiesService propertiesService)
		{
			_propertiesService = propertiesService;
		}

		public ActionResult Index()
		{

			return View("_MainMenuAdv", new CategoriesData());
        }


		public abstract class ItemsData : IHierarchicalEnumerable, IEnumerable
		{
			public ItemsData()
			{
			}

			public abstract IEnumerable Data { get; }

			public IEnumerator GetEnumerator()
			{
				return Data.GetEnumerator();
			}
			public IHierarchyData GetHierarchyData(object enumeratedItem)
			{
				return (IHierarchyData) enumeratedItem;
			}
		}

		public class ItemData : IHierarchyData
		{
			public string Text { get; protected set; }
			public string NavigateUrl { get; protected set; }

			public ItemData(string text, string navigateUrl)
			{
				Text = text;
				NavigateUrl = navigateUrl;
			}

			// IHierarchyData
			bool IHierarchyData.HasChildren
			{
				get { return HasChildren(); }
			}
			object IHierarchyData.Item
			{
				get { return this; }
			}
			string IHierarchyData.Path
			{
				get { return NavigateUrl; }
			}
			string IHierarchyData.Type
			{
				get { return GetType().ToString(); }
			}
			IHierarchicalEnumerable IHierarchyData.GetChildren()
			{
				return CreateChildren();
			}
			IHierarchyData IHierarchyData.GetParent()
			{
				return null;
			}

			protected virtual bool HasChildren()
			{
				return false;
			}
			protected virtual IHierarchicalEnumerable CreateChildren()
			{
				return null;
			}
		}

		public class CategoriesData : ItemsData
		{
			public override IEnumerable Data
			{
				get { return new List<CategoryData>()
					{
						new CategoryData(new Category(){CategoryID = 1,CategoryName = "Home"}),
                        new CategoryData(new Category(){CategoryID = 2,CategoryName = "second"}),
                        new CategoryData(new Category(){CategoryID = 2,CategoryName = "t"})
					};
				}
			}
		}

		public class CategoryData : ItemData
		{
			public Category Category { get; protected set; }

			public CategoryData(Category category)
				: base(category.CategoryName, "?CategoryID=" + category.CategoryID)
			{
				Category = category;
			}

			protected override bool HasChildren()
			{
				return true;
			}
			protected override IHierarchicalEnumerable CreateChildren()
			{
				return new ProductsData(Category.CategoryID);
			}
		}

		public class ProductsData : ItemsData
		{
			public int CategoryID { get; protected set; }

			public ProductsData(int categoryID)
				: base()
			{
				CategoryID = categoryID;
			}

			public override IEnumerable Data
			{
				get
				{
					return new List<ProductData>()
					{
						new ProductData(new Product(){CategoryID = 1,ProductID = 1,ProductName = "ProductName"})
					};
				}
			}
		}

		public class ProductData : ItemData
		{
			public ProductData(Product product)
				: base(product.ProductName, "?CategoryID=" + product.CategoryID + "&ProductID=" + product.ProductID)
			{
			}
		}

	}

	public class Product

	{
		public int CategoryID { get; internal set; }
		public string ProductName { get; internal set; }
		public int ProductID { get; internal set; }
	}
}