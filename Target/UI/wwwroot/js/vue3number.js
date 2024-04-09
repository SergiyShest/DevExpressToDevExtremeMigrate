

// Свойства (Props)
// text - тип String, текст для отображения.
// require - тип Boolean, указывает, требуется ли заполнение поля.
// valudateonload - тип Boolean или String (возможные значения: true, false), указывает, должна ли производиться валидация при загрузке.
// requretext - тип String, текст сообщения об ошибке при отсутствии значения в обязательном поле.
// rules - список правил валидации.
// inputStyle - тип Object или String, стили для поля ввода.
// inputClass - тип String, класс для поля ввода по умолчанию.
export const componentBase = {
    props: {
        'id': String,
        'text': String,
        'require': Boolean,
        'valudateonload': { type: [Boolean, String], default: false },
        'readonly': Boolean,
        'requretext': { type: String, default: 'Значение должно быть заполнено!!!' },
        'rules': { type: Array, default: () => [] },
        'inputStyle': { type: [Object, String] },
        'inputClass': { type: String, default: 'def-inp' },
        'modelValue': { type: String },
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
            const val = event.target.value;
            this.valueInt = val;
            this.$emit('update:modelValue', val);
            this.validate(val);
           
        },
        setReadonly(val) {
            this.externalReadonly = val;
        },
        validate(val) {

            this.valid = true;    
            const errList = [];
            this.notValidText = '';

            if (this.require && !val) {
             
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

        }
    },
    watch: {
        modelValue(newValue) {
            
            this.valueInt = newValue;
            this.validate(newValue);
        }
    }
    ,
    mounted() {
        this.valueInt = this.modelValue;
        if (this.valudateonload == true) {
            this.validate(this.modelValue);
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
        'modelValue': { type: [String, Number], default: '' },
    },
    template: `
 <div class="flex-row" >
 <div class="title-col" >{{ text }}<span v-if="require" style="color:red">*</span>:</div>
 <input
  :id="'input_' + id"
  :class="inputClasses" 
  :style="inputStyle"

   v-bind:value="modelValue"
   v-on:input="valChanged($event)"
   v-bind:readonly="readonly || externalReadonly"
 />
 <img v-if="!valid" src="/images/invalid.png" class="invalidImage" /><slot></slot>
 </div>
`
}//kf-input  :title="notValidText" 
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
            <div class="title-col">{{ text }}<span v-if="require" style="color:red">*</span>:</div>
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
            <div class="title-col">{{ text }}<span v-if="require" style="color:red">*</span>:</div>
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
        'scale': { type: [Number, String], default: 2 },
    },
    watch: {
        modelValue(val) {
            this.valueInt = formatNum(val, this.scale)
            this.validate(val)
        }
    },
    methods: {
        onBlur(event) {//потеря фокуса
            this.valueInt = formatNum(event.target.value, this.scale)//установка правильного номера для отображения
            const val = reFormatNum(event.target.value)//получение правильного числа
            console.log(this.valueInt+'==>'+val);
            this.$emit('update:modelValue', val);//event to parent
            this.validate(val);
        },
       onFocus(event) {
           const control = event.target;
           control.value = reFormatNum(control.value)
          // control.type = "number";
        }

    },

    template: `
 <div class="flex-row" >
 <div class="title-col" >{{ text }}<span v-if="require" style="color:red">*</span>:</div>
<input  style="text-align:right"
  :class="inputClasses" 
  :style="inputStyle" 
  :title="notValidText" 
   v-bind:value="valueInt"
   v-on:focus = "onFocus($event)"
   v-on:blur =   "onBlur($event)"
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
            this.$emit('update:modelValue', val);//event to parent
        }
    },
    template: `
 <div class="flex-row" >
 <div class="title-col" >{{ text }}<span v-if="require" style="color:red">*</span>:</div>
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

    template: `
 <div class="flex-row" >
 <div class="title-col" >{{ text }}<span v-if="require" style="color:red">*</span>:</div>
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
