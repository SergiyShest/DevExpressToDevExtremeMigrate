//сюда вынесены те функции которые не требуют this что бы разгрузить основную часть
export const locationMixin = {

	methods: {
		//функции для поиска 
		codeFunction(item, filterString) {
			return item.Code == filterString;
		},
		nameFunction(item, filterString) {
			return item.Path.toLowerCase().includes(filterString);
		},
		pathFunction(item, filterString) {
			return item.Path.toLowerCase().includes(filterString);
		},
		barcodeFunction(item, filterString) {
			if (item.Content)
				for (const key in item.Content) {
					const element = item.Content[key];
					if (element.barcode) {
						return element.barcode.toLowerCase().includes(filterString);
					} else {
						return element.Name.toLowerCase().includes(filterString);
					}
				}
			else {
				return item.Name.toLowerCase().includes(filterString);
			}
			return false;
		},

		//get part of image
		getSrc(item) {
			var img = "folder.png";
          if(!item) return img;
			if (item.Code == "FREEZER") img = "refrigerator.png";
			if (item.Code == "SHELF") {
				if (this.HasContent(item)) {
					img = "shelf.png";
				} else {
					img = "shelfEmpty.png";
				}
			}
			if (item.Code == "ROOM") img = "room.png";
			if (item.Code == "RACK") img = "Rows.png";
			if (item.Code == "ROW") img = "row.png";
			if (item.ModelId == 1014) img = "box.png";
			if (item.ModelId == 1013) img = "plate.png";
			if (item.ModelId == 1011 || item.ModelId == 1012) img = "tube.png";
			if (item.TypeId == 5201) img = "SingleReagent.png";
			if (item.TypeId == 5202) img = "KitReagents.png";
			if (item.TypeId == 5203) img = "BoxReagents.png";
			if (item.TypeId == 5204) img = "BoxKitsReagents.png";
			return img;
		},

		formatNumber(percent) {
			var num = new Intl.NumberFormat("en", {
				maximumFractionDigits: 2,
			}).format(percent);

			return num;
		},
		itemPercent(item) {
			if(!item)return 0;
			if(!item.PercentFilling)return 0;
			var num = parseInt(item.PercentFilling, 10);
			return num;
		},

		  ColorForPercent(item) {
			if (!item) return;
			const percent = item.PercentFilling;
			var color;
			if (percent <= 80) {
			  color = "green";
			} else if (percent <= 100) {
			  color = "yellow";
			} else {
			  color = "red";
			}
	  
			return color;
		  },
		  HasContent(item) {
			if (item.ContentSamples > 0 && item.ContentReagents > 0) {
			  return true;
			}
			if (!item.children) {
			  return false;
			}
			for (var i = 0; i < item.children.length; i++) {
			  var element = item.children[i];
			  if (this.HasContent(element)) {
				return true;
			  }
			}
			return false;
		  },
	}
}
