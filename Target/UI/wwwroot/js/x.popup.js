function xPopup(header, path) {


	var content = `<H1> no content </H1>`
	var popupContentTemplate = function () {
		return content;
	};
	const height = window.screen.height - 150;

	var popup = $('#dxPopup')
		.dxPopup({
			width: '95vw',
			height: '95vh',
			visible: false,
			title: header,
			hideOnOutsideClick: true,
			showCloseButton: true,
			toolbarItems: [

				{
					location: "after",
					template: function () {
						return $("<div>")
							.addClass("dx-toolbar-item dx-popup-custom-html")
							.append("<div style=\"cursor: pointer;border-bottom: 2px solid #666;width:20px; height: 25px;\"><div>")
							.on("click", function () {

								popup.option("width", '50vw');
								popup.option("height", '50vh');
							});
					}
				},
				{
					location: "after",
					template: function () {
						return $("<div>")
							.addClass("dx-toolbar-item dx-popup-custom-html")
							.append("<div style=\"cursor: pointer;border: 2px solid #666;width:26px; height: 25px;\"><div>")
							.on("click", function () {
								popup.option("width", '100vw');
								popup.option("height", '100vh');
							});
					}
				}
			],
			contentTemplate: popupContentTemplate
		}).dxPopup('instance');

	path = document.location.origin + path
	content = '<iframe src="' + path + '" style= "width:100%; height:100%">'

	popup.show()
	console.log('popup show')
}