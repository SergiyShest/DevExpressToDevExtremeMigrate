
//============================================================================================================================
// Компонент kf-number
export const KfNum = {
    data() {
        return {
            valueInt: null,
            valid: true,
        };
    },
    props: {
        'modelValue': { type: [Number, String] },
        'scale': { type: [Number, String], default: 2 },
    },
    watch: {
        modelValue(val) {
            this.valueInt = formatNum(val, this.scale)
        }
    },
    methods: {


        onBlur(event) {//потеря фокуса
            const control = event.target;
            control.type = "text";
            const inp = control.value;
            control.value = control.value.replace(',', '.');//замена запятых точками
            control.value = control.value.replace(/\.(?=.*\.)/g, '');//удаление всех точек кроме последней
//          control.value = control.value.replace(/(?!^)(-)/g, '');//удаление минуса если есть не в начале
            console.log(inp +'==>'+control.value);
            control.value = formatNum(control.value, this.scale)//установка форматированного числа с разделением разрядов для отображения
            const val = reFormatNum(control.value)//получение числа без разрядов запятая заменяется на точку 
            console.log(this.valueInt+'==>'+val);
            this.$emit('update:modelValue', val);//event to parent
 
        },
       onFocus(event) {
           const control = event.target;
           control.value = reFormatNum(control.value)
  //         control.type = "number";
        },
        onKeyDown(event) {
            const input = event.target;
            const cursorPosition = input.selectionStart;

            console.log(event.key + ' ==>');

            // Разрешаем управляющие символы и специальные клавиши
            if (['Backspace', 'Tab', 'Escape', 'Enter', 'Delete', 'Home', 'End', 'ArrowLeft', 'ArrowRight'].includes(event.key) ||
                // Разрешаем Ctrl+A (выделить всё)
                (event.key === 'a' && event.ctrlKey === true) ||
                // Разрешаем цифры, точку и запятую
                event.key.match(/^[0-9,\.]$/)) {
                // Если условие выполнено, разрешаем действие
                return;
            }

            // Разрешаем "минус" только если курсор находится в начале
            if (event.key === '-' && cursorPosition === 0 && !input.value.startsWith('-')) {
                return;
            }

            // В противном случае предотвращаем ввод символа
            event.preventDefault();
        }

    },

    mounted() {
        this.valueInt = formatNum(this.modelValue, this.scale);
       
    },
    template: `
 <div class="flex-row" >
 <div class="title-col" > text </div>
<input  style="text-align:right"

   v-bind:value="valueInt"
   v-on:focus = "onFocus($event)"
   v-on:blur =   "onBlur($event)"
   v-on:keydown =   "onKeyDown($event)"
 />
 
 </div>
`
}//kf-number
//============================================================================================================================
