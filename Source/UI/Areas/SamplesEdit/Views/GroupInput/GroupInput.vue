<template>
	<v-app id="app">
		<div class="modal" v-if="loadingData">
			<img class="loader-icon" :src="require('../../../../Content/Images/loading.gif')" />
		</div>
		<v-container fluid style="height:100vh;background-color: lightgray;">
			<splitpanes horizontal class="default-theme" style="height: 95%;">
				<pane size="30" min-size="15" style="overflow: scroll;">

					<v-textarea v-model="text" style="border: double black; margin: 1px;"
						placeholder="Copy from Excel and paste here"></v-textarea>
				</pane>
				<pane size="70" style="overflow: scroll;">

					<div style="display: flex;flex-direction: row;">
						
						<GroupColumn style="border: double black; margin: 1px;" 
						    v-bind:usedFildNames="FieldNames"
						    v-bind:fields="avaialbeFieldNames"
							v-bind:selfield="fieldNames[ind - 1]" 
							v-bind:index="ind - 1" 
							v-for="ind in columnCount"
							v-bind:key="ind" />
							<!-- <v-text-field type="number" label="Сount" v-model="columnCount"/>  -->
					</div>
					<DxDataGrid :data-source="resultData" 
					   keyExpr="Id" 
					   ref="dataGrid" 

						:row-alternation-enabled="true" 
						:allow-column-resizing="true"
						:show-borders="true" 
						column-resizing-mode="widget"
						>
						<DxEditing
            			:allow-updating="true"
            			:allow-adding="true"
            			:allow-deleting="true"
            			mode="cell" />
						<DxStateStoring :enabled="true" type="localStorage" storage-key="GroupEdit" />
						<DxPaging :page-size="50" />
						<DxColumn data-field="Id" caption="Nr" :allow-editing="false" :width="20"/>
						<DxColumn v-for="fieldName in FieldNames" v-bind:key="fieldName" :data-field="fieldName"
							:caption="GetName(fieldName)" :data-type="GetDataType(fieldName)" />
					</DxDataGrid>
				</pane>
			</splitpanes>
		</v-container>
		<v-footer fixed class="d-flex justify-end">
      <v-btn
        class="ma-2"
        style="min-width: 130px"
        @click="save()"
        :disabled="!EnableSaveForm"
        >Submit</v-btn
      >
      <v-btn
        class="ma-2"
        style="min-width: 130px; margin-right: 10px"
        @click="cancelForm()"
        >Cancel</v-btn
      >
      <div style="width: 30px" /> </v-footer
    ><!--buttons-->
	</v-app>
</template>

<script>



import { baseMixin } from "../../../../Vue/BaseMixin.js";
import { helpMixin } from "./HelpMixin.js";
import { Splitpanes, Pane } from 'splitpanes'
import GroupColumn from './components/CroupColumn'
import 'splitpanes/dist/splitpanes.css'
import { baseMixin } from "../../../../Vue/BaseMixin.js";
import {
	DxDataGrid,
	DxColumn,
	DxGrouping,
	DxGroupPanel,
	DxPager,
	DxPaging,
	DxFilterRow,
	DxSearchPanel,
	DxSelection,
	DxStateStoring,
	DxEditing
} from "devextreme-vue/data-grid";
import DxButton from 'devextreme-vue/button';



export default {
	name: "App",
	mixins: [baseMixin],
	components: {
		Splitpanes, Pane, GroupColumn,
		DxDataGrid,
		DxColumn,
		DxGrouping,
		DxGroupPanel,
		DxPager,
		DxPaging,
		DxFilterRow,
		DxSearchPanel,
		DxSelection,
		DxStateStoring,
		DxButton,DxEditing
	},

	data: () => ({
		avaialbeFieldNames: [], //список доступных полей
		text: '',

		contextMenuItems: [],
		fieldNames: [[], [], [], [], [], [], [], [], [], [], [], []],//Названия полей В каждом массиве всегда одно значение. Такая  конструкция использована, что бы получать данные изменяемые в дочерних компонентах по ссылке ( массивы передаются через ссылки)
		columnCount: 12,
		canSave: true,
		resultData: []
	}),
	watch: {
		columnCount: function () {
			var fNames = []
          for(var i=0;i<this.columnCount;i++){
              fNames.push([]);
              if(this.fieldNames[i]){
		      fNames[i]=this.fieldNames[i];
			  }
		  }
		  console.log(this.fieldNames)
		this.fieldNames=fNames;
		},
		text: function (text) {
			let rows = text.split('\n');
			this.resultData = [];
			for (let rowId = 0; rowId < rows.length; rowId++) {
				let row = rows[rowId];
				if (row == '') continue;
				let cells = row.split('\t');
				let dataRow = {};
				dataRow.Id = rowId;
				let i = 0;
				cells.forEach(cell => {
					eval('dataRow.' + this.FieldNames[i] + '="' + cell + '";')
					i++;
				});
				this.resultData.push(dataRow);
			}
		}

	}
	,
	computed: {
        EnableSaveForm:function () {
             return this.resultData.length>0
		},
		FieldNames: function () {
			let names = []
			this.fieldNames.forEach(name => {
				names.push(name[0])
			});
			return names;
		}
	},
	methods: {
		GetName(fieldName) {
			if (!fieldName) return;
			let field = this.avaialbeFieldNames.filter(x => x.Name == fieldName)[0]
			if (!field) return;
			return field.Value;
		}
		,
		GetDataType(fieldName) {
			if (!fieldName) return;
			let field = this.avaialbeFieldNames.filter(x => x.Name == fieldName)[0]
			if (!field) return;
			return field.DataType;
		},


		//Получение описания полей
		getAvailableFields: function () {
			this.fetch(this.setAvailableFields, "/SamplesEdit/GroupInput/GetAvailableFields");
		}
		,
		setAvailableFields: function (val) {
			this.avaialbeFieldNames = val;
			this.loadingData = false;
		},

		save: function () {

			var onSave = this.onSave;
			this.fetch(onSave, "/SamplesEdit/GroupInput/GroupInputSave?selectedIds="+SelectedIds+"&journalType="+JournalType, this.resultData);
		}
		,
		onSave: function (error) {
			if (error == '') {
				window.parent.postMessage("Refresh", '*'); //send message parent window for  Refresh
				this.cancelForm();
			} else {
				alert(error);
				console.log(error);
				this.loadingData = false;
			}
		}
		,
		cancelForm: function (value) {
			this.loadingData = false;
			window.parent.postMessage("CloseUrlForm", '*');//send message parent window for  close this form
		}
	},
	mounted: function () {
		this.getAvailableFields();
	},
};
</script>
<style>
html,
body {
	height: 100%;
}

.flex-row {
	display: flex;
	flex-direction: row;
	width: 410px;
}

.flex-column {
	display: flex;
	flex-direction: column;
	width: 210px;
}

.loader-icon {
	display: block;
	margin-left: auto;
	margin-right: auto;
}
</style>
