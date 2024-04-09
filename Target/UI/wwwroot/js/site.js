
function checkItem(id) {
	if (id) {
		return true;
	}
	alert("Необходимо выделить нужное значение в  дата грид")
	return false;
}

function numOnBlur(control, scale = 2) {
  //  control.type = "text";
    control.value = formatNum(control.value, scale)
    console.log("numOnBlur " + control.value)
}

function numOnFocus(control) {

  //  control.value = reFormatNum(control.value)

  //  control.type = "number";
  //  console.log("numOnFocus " + control.value)
}

function formatNum(val, scale) {
    if (!val) return null;
    const sc = '0'.repeat(scale)
    console.log(sc+" formatNum " + val)
    return numeral(val).format('0,0.' + sc).replaceAll(',', ' ');
}

function reFormatNum(val) {
    return val.replaceAll(' ', '');
}