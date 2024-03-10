
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
