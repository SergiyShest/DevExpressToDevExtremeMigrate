<template>
							<select class="p1 nam "  v-model="selectedField"  style = "height: 35px;">
								<option selected value="" style="background: lightgray">dont use </option>
								 <option  v-bind:style="{color:cellColor(field.Name)}"
								 v-for=" field in fields" v-bind:key="field.Name" 
								 :v-value="field.Name" 
								 v-bind:value="field.Name" >
									{{field.Value}}
								</option>
							</select>

</template>
,
<script>

    export default {
        name: "GroupColumn",
        props: {
            fields: Array,
            index: Number,
            dat: Array,
            dataCh: Number,
            selfield: Array,
            usedFildNames: Array			
        }
        ,
        data: function () {
            return {
 //               pastedData: null,
                selectedField: '',
                editingRowCount: EditingRowCount //количество строк для вставки
            }
        }
        ,
        watch: {
            selectedField: function (newVal) {
                localStorage.setItem("selectedField" + this.index, newVal);
                this.selfield.length = 0;
                this.selfield.push(this.selectedField);
            }
        },
        methods:
        {
			cellColor: function(val) {
			if (this.usedFildNames.includes(val)) {
				return 'darkgrey';
			}
			return 'green';
		},
        },
        mounted: function () {
  
            var pr = localStorage.getItem("selectedField" + this.index);
            if (pr) {
                this.selectedField = pr;
            }
        },
    }
    </script>
<style>


		.p1 { /* Поля вокруг текста */
			margin: 2px;
			padding: 2px;
		}

		.nam {
			color: brown;
/* .			font-size: large; */
		}

	</style>
