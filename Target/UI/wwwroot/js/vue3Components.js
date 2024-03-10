
// Свойства (Props)
// text - тип String, текст для отображения.
// requre - тип Boolean, указывает, требуется ли заполнение поля.
// valudateonload - тип Boolean или String (возможные значения: true, false), указывает, должна ли производиться валидация при загрузке.
// requretext - тип String, текст сообщения об ошибке при отсутствии значения в обязательном поле.
// rules - список правил валидации.
// inputStyle - тип Object или String, стили для поля ввода.
// inputClass - тип String, класс для поля ввода по умолчанию.
export const componentBase = {
    props: {
        'text': String,
        'requre': Boolean,
        'valudateonload': { type: [Boolean, String], default: false },
        'readonly': Boolean,
        'requretext': { type: String, default: 'Значение должно быть заполнено!!!' },
        'rules': { type: Array, default: () => [] },
        'inputStyle': { type: [Object, String] },
        'inputClass': { type: String, default: 'def-inp' },
    },
    data() {
        return {
            valueInt: null,
            valid: true,
            externalReadonly: false,
            notValidText: null,
            inputClasses: {},
        };
    },
    methods: {
        valChanged(event) {
            this.virtChange(event.target.modelValue);
        },
        virtChange(val) {
            console.log(this.text +'   '+val)
            this.valueInt = val;
            this.$emit('input', val);
        },
        SetReadonly(val) {
            this.externalReadonly = val;
        },
        Validate(val) {
            if (typeof val === "undefined") {
                val = null;
            }
            this.valid = true;
            const errList = [];
            this.notValidText = '';

            if (this.requre && !val) {
                this.valid = false;
                this.notValidText = this.requretext;
            } else {
                try {
                    if (this.rules) {
                        this.rules.forEach(element => {
                            const valResult = element(val);
                            if (valResult !== true) {
                                errList.push(valResult);
                                this.valid = false;
                            }
                        });
                        if (!this.valid) {
                            this.notValidText = errList.join('\n');
                        }
                    }
                } catch (error) {
                    console.error("Error while validation rules executing: " + error);
                }
            }
            console.log(this.text + ' valid = ' + this.valid);
        },
        mounted() {
            this.valueInt = this.molelValue;
            if (this.valudateonload == true) {
                this.Validate(this.molelValue);
            }
        }
    },
    watch: {
        modelValue(newValue) {
            this.valueInt = newValue;
            this.Validate(newValue);
        }
    }
};
//end component base
//============================================================================================================================
//Компонент kf-field
export const KfField = {
    mixins: [componentBase],
    props: {
        'modelValue': { type: String },

    },
    methods: {
        valChanged(event) {
            this.$emit('input', event.target.molelValue);
        }
    },

    template: `
      <input 
          :class="inputClasses" 
          :style="inputStyle"
          :title="notValidText" 
          v-bind:value="modelValue"
          v-on:input="valChanged($event)"
      />
  `
}//end kf-field
//============================================================================================================================
export const KfInput = {
    mixins: [componentBase],
    props: {
        'modelValue': { type: [String, Number],default:'' },
    },

    methods: {
    },
    template: `
 <div class="flex-row" >
 <div class="title-col" >{{ text }}<span v-if="requre" style="color:red">*</span>:</div>
 <input
  :class="inputClasses" 
  :style="inputStyle"
  :title="notValidText" 
   v-bind:value="modelValue"
   v-on:input="valChanged($event)"
   v-bind:readonly="readonly || externalReadonly"
 />
 <img v-if="!valid" src="/images/invalid.png" class="invalidImage" /><slot></slot>
 </div>
`
}//kf-input
//============================================================================================================================
// Компонент kf-date
export const KfDate = {
    mixins: [componentBase],
    props: {
        'modelValue': { type: [String, Date] },
    },
    methods: {},
    template: `
        <div class="flex-row">
            <div class="title-col">{{ text }}<span v-if="requre" style="color:red">*</span>:</div>
            <input 
                type='date'
                :class="inputClasses" 
                :style="inputStyle"
                :title="notValidText" 
                v-bind:value="valueInt"
                v-bind:readonly="readonly || externalReadonly"
                v-on:input="valChanged($event)"
            />
            <img v-if="!valid" src="/images/invalid.png" class="invalidImage">
            <slot></slot>
        </div>
    `
}//kf-date
//============================================================================================================================
// Компонент kf-select
export const KfSelect = {
    mixins: [componentBase],
    props: {
        'modelValue': { type: [String, Number] },
        'items': Array
    },
    methods: {},
    template: `
        <div class="flex-row">
            <div class="title-col">{{ text }}<span v-if="requre" style="color:red">*</span>:</div>
            <select 
                :class="inputClasses" 
                :style="inputStyle"
                :title="notValidText"
                v-model="valueInt"
                v-bind:disabled="readonly || externalReadonly"
                v-on:input="valChanged($event)"
            >
                <option v-for="item in items" :key="item.Id" v-bind:value="item.Id">{{item.Name}}</option>
            </select>
            <img v-if="!valid" src="/images/invalid.png" class="invalidImage">
            <slot></slot>
        </div>
    `
};//kf-select
//============================================================================================================================
// Компонент kf-number
export const KfNumber = {
    mixins: [componentBase],
    props: {
        'modelValue': { type: [Number, String] },
        'scale': { type: [Number, String] ,default : 2},
    },
    watch: {
        modelValue(val) {
            this.valueInt = formatNum(val, this.scale)
            this.Validate(val)
        }
    },
    methods: {
        virtChange(val) {
            val = reFormatNum(val)
            this.$emit('input', val)//event to parent
        }
    },
    mounted: function () {
//        this.valueInt = formatNum(this.modelValue, this.scale)
//        console.log(this.valueInt)
//        this.Validate(this.modelValue)
    },
    template: `
 <div class="flex-row" >
 <div class="title-col" >{{ text }}<span v-if="requre" style="color:red">*</span>:</div>
<input  style="text-align:right"
  :class="inputClasses" 
  :style="inputStyle" 
  :title="notValidText" 
   v-bind:value="valueInt"
   v-on:input="valChanged($event)"
   onblur="numOnBlur(this,this.scale)"
   onfocus="numOnFocus(this)"
   v-bind:readonly="readonly || externalReadonly"
 />
 <img v-if="!valid" src="/images/invalid.png" class="invalidImage"/><slot></slot>
 </div>
`
}//kf-number
//============================================================================================================================
// Компонент kf-check
export const KfCheck = {
    mixins: [componentBase],
    props: {
        'modelValue': { type: [Boolean, String] },
    },
    watch: {
        immediate: true,
        valueInt(val) {
            this.$emit('input', val)//event to parent
        }
    },
    methods: {

    },
    template: `
 <div class="flex-row" >
 <div class="title-col" >{{ text }}<span v-if="requre" style="color:red">*</span>:</div>
<div style="width:30px;" >
 <input style="width:20px;margin:0px"
   type="checkbox"
  :class="inputClasses"
  :style="inputStyle"
  :title="notValidText" 
   v-model="valueInt"
   v-bind:disabled="readonly || externalReadonly"
 ></input>
 <img v-if="!valid" src="/images/invalid.png" class="invalidImage" /><slot></slot>
</div>
 </div>
`
}//kf-check
//============================================================================================================================
// Компонент kf-textarea
export const KfTextarea = {
    mixins: [componentBase],
    props: {
        'modelValue': { type: String },
    },

    methods: {
    },
    template: `
 <div class="flex-row" >
 <div class="title-col" >{{ text }}<span v-if="requre" style="color:red">*</span>:</div>
 <div
 :class="inputClasses"
 :style="inputStyle"
 >
 <textarea style= "width:100%"
  :class="{ invalid: !valid}"
   class="kf-inp"
  :title="notValidText" 
   v-bind:value="modelValue"
   v-on:input="valChanged($event)"
   v-bind:readonly="readonly || externalReadonly"
  rows="5"
 ></textarea>
</div>
 <img v-if="!valid" src="/images/invalid.png" class="invalidImage" /><slot></slot> 
 </div>
`
}//kf-textarea
//============================================================================================================================
// Компонент kf-text
export const KfText = {
    mixins: [componentBase],
    props: {
        'modelValue': { type: String },
    },
    template: `
 <div class="flex-row" >
 <div class="coll" >{{ text }}:</div>
 <div class="short bold">{{value}}</div><slot></slot>
 </div>
`
}//kf-text не редактируемый текст (class=" bold")
//============================================================================================================================
// Компонент kf-button
export const KfButton = {
    props: {
        'text': null,
        'image': null
    },
    computed: {
        imgSrc() {
            if (this.image) return /images/ + this.image + ".png";
        }
    },
    template: `<button
class="kf-button"

>
<img v-if="image" :src="imgSrc" class="kf-button-image" />{{text}}<slot></slot>
</button>`
}//kf-button
//============================================================================================================================
// Компонент kf-lookup

let dataGrid;

function setDataSource(el, path) {
    const dataSource = DevExpress.data.AspNet.createStore({
        key: "Id",
        loadUrl: document.location.origin + path,
    });
    var grid = $(el).dxDropDownBox("instance");
    grid.option("dataSource", dataSource);
}
function createLookUp(el, columns, val, dropDownWidth, pageSize, displayExpr, valChanged) {
    el.dxDropDownBox({
        value: val,
        valueExpr: 'Id',
        placeholder: 'Select a value...',
        displayExpr: displayExpr,
        showClearButton: true,
 //       dataSource: makeAsyncDataSource('customers.json'),
        onValueChanded: valChanged,
        dropDownOptions: {
            width: dropDownWidth
        },
        contentTemplate(e) {
            const v = e.component.option('modelValue');
            let firstShow = true
            const $dataGrid = $('<div>').dxDataGrid({
                dataSource: e.component.getDataSource(),
                columns: columns,
                export: { enabled: false },
                stateStoring: { enabled: false },
                columnChooser: { enabled: false },

                hoverStateEnabled: true,
                paging: { enabled: true, pageSize: pageSize },
                filterRow: { visible: true },
                height: 500,
               selection: { mode: 'multiple', },
              //  selection: { mode: 'single', },
                selectedRowKeys: v,
                onSelectionChanged: function (selectedItems) {
                    console.log(selectedItems)
                    if (false) {
                        firstShow = false;
                    }
                    else {
                        const keys = selectedItems.selectedRowKeys;
                        e.component.option('modelValue', keys);
                    }
                },
               });

            dataGrid = $dataGrid.dxDataGrid('instance');

            e.component.on('valueChanged', (args) => {
                const { value } = args;
                dataGrid.selectRows(value, false);
            });

            return $dataGrid;
        },
    });
}



function CreateGuid() {
    function _p8(s) {
        var p = (Math.random().toString(16) + "000000000").substr(2, 8);
        return s ? "-" + p.substr(0, 4) + "-" + p.substr(4, 4) : p;
    }
    return _p8() + _p8(true) + _p8(true) + _p8();
}


export const KfGridLookUp = {
    mixins: [componentBase],
    props: {
        'modelValue': { type: [String, Number] },
        "items": { type: Array, default: null },
        "loadUrl": String,
        "pageSize": { type: Number, default: 10 },

        "dropDownWidth": { type: Number, default: 700 },
        "displayExpr": { type: [String, Object], default: "Name" },
        "columns": { type: Array, default: null },
    },
    data() {
        return {
            lookUpId: CreateGuid()
        }
    },

    methods: {
        vChange(val) {
            this.$emit('input', val)//event to parent
        }

    },
    template: `
      <div class="flex-row" >
         <div class="coll" >{{ text }}<span v-if="requre" style="color:red">*</span>:</div>
          <div 
          v-bind:id="lookUpId"
          :title="notValidText" 
          v-bind:value="modelValue"
          v-bind:readonly="readonly"
          v-on:input="valChanged($event)"
          />
         <img v-if="!valid" src="invalid.png"/> 
      </div>
  `,
    mounted: function () {
        console.log(this.modelValue + '   ' + this.displayExpr)
        const el = $('#' + this.lookUpId)
        const vch = this.vChange
        const vChanged = function (x) {
            console.log(x);
            if (x.value && x.value.length > 0) {
                vch(x.value[0])
            } else {
                vch(null)
            }
        }
        createLookUp(el, this.columns, this.value, this.dropDownWidth, this.pageSize, this.displayExpr, vChanged)

        setDataSource(el, this.loadUrl)

    }
}