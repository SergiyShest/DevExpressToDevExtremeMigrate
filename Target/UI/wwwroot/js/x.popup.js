let x_popup = null;

$(window).on("load", function () {
    window.addEventListener("message", receivedMessage, false);

});
function receivedMessage(parameters) {
    if (!parameters.data) return
    if (parameters.data == "closePopup") {
        x_popup.hide();
    }
}

function xPopup(headerText, path, width = null, height = null, callback = null, elId = null,) {
    let content = ``
    let popupContentTemplate = function () {
        return content;
    };
    if (width == null && height == null) {
        width = "95vw";
        height = "95vh";
    } else {
        if (width) {
            if (/^\d+$/.test(width)) width = width + 'px'//если это ширина заданная в цифрах

        } else {
            width = (window.innerWidth / 100) * 95 + 'px'
        }
        if (height) {
            if (/^\d+$/.test(height)) height = height + 'px'//если это высота заданная в цифрах
        } else {
            height = (window.innerHeight / 100) * 95 + 'px'
        }
    }
    if (!elId) {
        elId = 'dxPopup'
    }

    try {
        let el = document.getElementById(elId);
        if (!el) console.error('не найден еlement c id' + elId)

        x_popup = $('#' + elId).dxPopup({
            showTitle: true,
            title: headerText,
            width: width,
            height: height,
            onHidden: callback,
            resizeEnabled: true,
            dragEnabled: true,
            visible: false,
            hideOnOutsideClick: true,
            showCloseButton: true,
            toolbarItems: [
                {
                    location: "after",
                    template: function () {
                        return $("<div>")
                            .addClass("dx-toolbar-item dx-popup-custom-html")
                            .append("<div style='font:bold'>_<div>")
                            .on("click", function () {
                                popup.option("width", '10vw');
                                popup.option("height", '10vh');
                            });
                    },
                },
                {
                    location: "after",
                    template: function () {
                        return $("<div>")
                            .addClass("dx-toolbar-item dx-popup-custom-html")
                            .append("<div>+<div>")
                            .on("click", function () {
                                popup.option("width", '100vw');
                                popup.option("height", '100vh');
                            });
                    }
                }
            ],
            contentTemplate: popupContentTemplate
        }).dxPopup('instance');
        content = '<iframe src="' + document.location.origin + path + '" style="width:110%; height:110%">'
        x_popup.show();
        return x_popup;
    } catch (error) {
        console.error(error)
    }
}

function closePopup() {

    if (window.parent != null) {
        window.parent.postMessage("closePopup", '*');
    }
    else {
        console.error('Не найден window.parent, не получается закрыть окно');
    }
}