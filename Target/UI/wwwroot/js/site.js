
function checkItem(id) {
	if (id) {
		return true;
	}
	alert("Необходимо выделить нужное значение в  дата грид")
	return false;
}

function numOnBlur(control,scale=2) {
    control.type = "text";
    control.value = formatNum(control.value,scale)

}

function numOnFocus(control) {
    control.value = reFormatNum(control.value)
    control.type = "number";

}

function formatNum(val, scale) {
    const sc = '0'.repeat(scale)
    return numeral(val).format('0,0.'+sc).replaceAll(',', ' ');
}

function reFormatNum(val) {
    return val.replaceAll(' ', '');
}
//function xPopup(header, path) {
//	console.log('popup')

//	var content = `<H1> no content </H1>`
//	var popupContentTemplate = function () {
//		return content;
//	};
//	const height = window.screen.height - 150;

//	var popup = $('#dxPopup')
//		.dxPopup({
//			width: '75vw',
//			height: '75vh',
//			visible: false,
//			title: header,
//			hideOnOutsideClick: true,
//			showCloseButton: true,
//			toolbarItems: [

//				{
//					location: "after",
//					template: function () {
//						return $("<div>")
//							.addClass("dx-toolbar-item dx-popup-custom-html")
//							.append("<div style=\"cursor: pointer;border-bottom: 2px solid #666;width:20px; height: 25px;\"><div>")
//							.on("click", function () {

//								popup.option("width", '50vw');
//								popup.option("height", '50vh');
//							});
//					}
//				},
//				{
//					location: "after",
//					template: function () {
//						return $("<div>")
//							.addClass("dx-toolbar-item dx-popup-custom-html")
//							.append("<div style=\"cursor: pointer;border: 2px solid #666;width:26px; height: 25px;\"><div>")
//							.on("click", function () {
//								popup.option("width", '100vw');
//								popup.option("height", '100vh');
//							});
//					}
//				}
//			],
//			contentTemplate: popupContentTemplate
//		}).dxPopup('instance');

//	path = document.location.origin + path
//	content = '<iframe src="' + path + '" style= "width:100%; height:100%">'

//	popup.show()
//	console.log(popup)
//}