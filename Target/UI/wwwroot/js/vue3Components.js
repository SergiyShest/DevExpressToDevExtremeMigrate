function ConvertDate(value){
  return value
}
function numOnBlur(control) {
  control.type = "text";
  control.value = formatNum(control.value)

}

function numOnFocus(control) {
  control.value = reFormatNum(control.value)
  control.type = "number";

}

function formatNum(val) {
  return numeral(val).format('0,0.00').replaceAll(',', ' ');
}

function reFormatNum(val) {
  return val.replaceAll(' ', '');
}

// Свойства (Props)
// text - тип String, текст для отображения.
// req - тип Boolean, указывает, требуется ли заполнение поля.
// valudateonload - тип Boolean или String (возможные значения: true, false), указывает, должна ли производиться валидация при загрузке.
// requretext - тип String, текст сообщения об ошибке при отсутствии значения в обязательном поле.
// rules - список правил валидации.
// inputStyle - тип Object или String, стили для поля ввода.
// inputClass - тип String, класс для поля ввода по умолчанию.
    const componentBase = {
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
                this.virtChange(event.target.value);
            },
            virtChange(val) {
                this.valueInt = val;
                this.$emit('input', val);
            },
            SetReadonly(val) {
                this.externalReadonly = val;
            },
            Validate(val) {
                if (typeof val !== "undefined") {
                    val = this.value.value;
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
                this.valueInt = this.value;
                if (this.valudateonload == true) {
                    this.Validate(this.value);
                }
            }
        },
        watch: {
            value(newValue) {
                this.valueInt = newValue;
                this.Validate(newValue);
            }
        }
    };
export const KfField ={
    mixins: [componentBase],
    props: {
        'value': { type: String },

    },
    methods: {
        valChanged(event) {
            this.$emit('input', event.target.value);
        }
    },

    template: `
      <input 
          :class="inputClasses" 
          :style="inputStyle"
          :title="notValidText" 
          v-bind:value="value"
          v-on:input="valChanged($event)"
      />
  `
}//kf-field

export const KfInput = {
    mixins: [componentBase],
    props: {
        'value': { type: [String, Number] },
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
   v-bind:value="value"
   v-on:input="valChanged($event)"
   v-bind:readonly="readonly || externalReadonly"
 />
 <img v-if="!valid" src="/images/invalid.png" class="invalidImage" /><slot></slot>
 </div>
`
}//kf-input

// Компонент kf-date
export const KfDate = {
    mixins: [componentBase],
    props: {
        'value': { type: [String, Date] },
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

// Компонент kf-select
export const KfSelect = {
    mixins: [componentBase],
    props: {
        'value': { type: [String, Number] },
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
export const KfNumber =  {
    mixins: [componentBase],
    props: {
        'value': { type: [Number, String] },
    },
    watch: {
        value(val) {
            this.valueInt = formatNum(val)
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
        this.valueInt = formatNum(this.value)
        this.Validate(this.value)
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
   onblur="numOnBlur(this)"
   onfocus="numOnFocus(this)"
   v-bind:readonly="readonly || externalReadonly"
 />
 <img v-if="!valid" src="/images/invalid.png" class="invalidImage"/><slot></slot>
 </div>
`
}//kf-number

export const KfCheck = {
    mixins: [componentBase],
    props: {
        'value': { type: [Boolean, String] },
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

export const KfTextarea = {
    mixins: [componentBase],
    props: {
        'value': { type: String },
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
   v-bind:value="value"
   v-on:input="valChanged($event)"
   v-bind:readonly="readonly || externalReadonly"
  rows="5"
 ></textarea>
</div>
 <img v-if="!valid" src="/images/invalid.png" class="invalidImage" /><slot></slot> 
 </div>
`
}//kf-textarea

export const KfText = {
    mixins: [componentBase],
    props: {
        'value': { type: String },
    },
    template: `
 <div class="flex-row" >
 <div class="coll" >{{ text }}:</div>
 <div class="short bold">{{value}}</div><slot></slot>
 </div>
`
}//kf-text не редактируемый текст (class=" bold")



export const KfButton = {
  props: {
      'text': null,
      'image':null
  },
  computed: {
      imgSrc() {
          if(this.image) return /images/ + this.image+".png";
    }
  },
  template: `<button
class="kf-button"

>
<img v-if="image" :src="imgSrc" class="kf-button-image" />{{text}}<slot></slot>
</button>`
}//kf-button  

