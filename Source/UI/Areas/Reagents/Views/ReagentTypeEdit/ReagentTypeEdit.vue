<template>
  <v-app>
    <v-main>
      <v-row v-if="alert.show">
        <v-col cols="12">
          <v-alert
            dense
            outlined
            hide-details
            clearabledismissible
            type="error"
            v-model="alert.show"
            >{{ alert.message }}</v-alert
          >
        </v-col> </v-row
      ><!--alert-->
      <div v-if="loadingData" class="m-0 p-0" style="width: 100%">
        <v-progress-linear indeterminate color="cyan"></v-progress-linear>
      </div>
      <!--progress-->
      <v-container fluid>
        <v-tabs
          fixed-tabs
          v-if="!isSingle"
          v-model="tab"
          background-color="#dde"
          align-with-title
        >
          <v-tabs-slider></v-tabs-slider>
          <v-tab href="#tab-main" max-width="150px" style="width: 100px;"> Main </v-tab>
          <v-tab href="#tab-inner-reagents">Inner reagents {{ChaildReagents.length}} </v-tab>
        </v-tabs>
        <v-row no-gutters v-if="tab != 'tab-inner-reagents'">
          <v-col v-for="colIndex in ColumCount" :key="colIndex">
            <!--Loop for columns-->
            <v-row
              no-gutters
              class="roww"
              v-for="property in Model"
              :key="property.Name"
              align="end"
            >
              <!--Loop for property-->
              <v-col v-if="GetVisibility(property, colIndex)"
                cols="8"
                no-gutters
              >
              <FormField :property="property" />
              </v-col>
            </v-row>
          </v-col>
        </v-row>
        <v-row no-gutters v-if="tab == 'tab-inner-reagents'">
  <div style="display: flex;  justify-content: flex-end;  width:100%;  background-color:azure ;padding: 3px;" >
          <DxButton @click="AddChaild" text="Add"  height="35px" icon="add"/>
          <DxButton @click="RemoveChaild" text="Remove" height="35px"  icon="remove" />
          <DxButton @click="GetChaild" text="Refresh" height="35px" icon="refresh"/>
          </div>
          <DxDataGrid
            keyExpr="Id"
            ref="dataGrid"
            :data-source="ChaildReagents"
            :remote-operations="false"
            :allow-column-reordering="true"
            :row-alternation-enabled="true"
            :show-borders="true"
            @selection-changed="onSelectionChaildChanged"
          >
            <DxPaging :page-size="10" />
           <DxPager
              :visible="true"
              :allowed-page-sizes="[10, 15,20,50,100, 'all']"
              display-mode="full"
              :show-page-size-selector="true"
              :show-info="true"
              :show-navigation-buttons="true" />
            <DxSelection
              select-all-mode="allPages"
              show-check-boxes-mode="always"
              mode="multiple" />
            <DxFilterRow :visible="true" />
            <DxColumn
              data-field="Id"
              caption="Id"
              data-type="number"
              width="50px" />
            <DxColumn
              :allow-grouping="false"
              data-field="Name"
              caption="Name  "
              data-type="string" />
            <DxColumn width="150px" data-field="Barcode" data-type="string" />
            <DxColumn width="150px" data-field="Vendor" data-type="string" />
            <DxColumn
              width="150px"
              caption="Catalog Number"
              data-field="CatalogNumber"
              data-type="string"
          /></DxDataGrid>
        </v-row>
      </v-container>
      <v-footer fixed class="d-flex justify-end">
        <v-btn
          class="ma-2"
          style="min-width: 130px"
          @click="Save()"
          :disabled="!EnableSaveForm"
          >Submit</v-btn
        >
        <v-btn class="ma-2" style="min-width: 130px" @click="CloseThis()"
          >Cancel</v-btn
        >
        <div style="width: 30px" /> </v-footer
      ><!--buttons-->
    </v-main>
  </v-app>
</template>

<script>
import FormField     from "../../../../Vue/components/formField";
import { EditModel } from "../../../../Vue/EditModel.js";
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
} from "devextreme-vue/data-grid";
import DxButton from "devextreme-vue/button";
//import DataSource from 'devextreme/data/data_source';
//import 'devextreme/data/odata/store';
export default {
  name: "App",
  mixins: [baseMixin],
  components: {
    DxDataGrid,
    DxColumn,
    DxGrouping,
    DxGroupPanel,
    DxPager,
    DxPaging,
    DxSelection,
    DxFilterRow,
    DxSearchPanel,
    DxButton,
    FormField
  },
  data() {
    return {
      tab: null,
      ColumCount: 1,
      Model: {},
      loadingData: true,
      IsChanged: false,

      alert: { show: false, message: null },
      EnableSaveForm: false,
      rules: {
        required: (value) => !!value || "Required.",
        counter: (value) =>
          (value && value.length <= 30) || "Max 30 characters",
        fieldsNumb: (value) => (value && value <= 5) || "Max 5 ",
      },
      // dataSource:data_Source
      // new DataSource({
      //   store: {
      //     type: 'odata',
      //     url: 'https://js.devexpress.com/Demos/SalesViewer/odata/DaySaleDtoes',
      //     key: 'Id',
      //     beforeSend(request) {
      //       request.params.startDate = '2020-05-10';
      //       request.params.endDate = '2020-05-15';
      //     },
      //   },
      // })
      // ,
      pageSizes: [10, 25, 50, 100],
      onContentReady(e) {

      },
      ChaildReagents: [],
      selectedChaild: [],
    };
  },
  computed: {
    isSingle() {
      var type = this.Model["TypeId"];
      if(type==undefined)return true;
      if (type && type.Value == 5201) return true;
      return false;
    },
  },
  watch: {
    Model: {
      deep: true,
      immediate: true,
      handler: function (newValue) {
        if (newValue.Init) {
          newValue.Init = false;
          return; //исключение первого прохода
        }

        for (var prName in this.Model) {
          //проход по  свойствам
          var pr = this.Model[prName];
          var pairChanged=false;
          if(pr.PairProperty){
            console.log('PairProperty;'+pr.PairProperty.Value)
            if( pr.PairProperty.OldValue!=pr.PairProperty.Value)pairChanged=true;
          }

          if (pr.Value == pr.OldValue&& pairChanged==false) continue;//что бы избежать зацикливания

          pr.EditValue = true;
          pr.IsChanged = pr.Value != pr.DBValue;
          if(!pr.IsChanged && pr.PairProperty)
          {
             pr.IsChanged = pr.PairProperty.Value != pr.PairProperty.DBValue;
             pr.PairProperty.IsChanged=pr.IsChanged;
          }

          pr.EditValue = pr.IsChanged;
          if (pr.EditValue) {
            this.EnableSaveForm = true;
          }
          pr.OldValue = pr.Value;
          if(pr.PairProperty){
             pr.PairProperty.OldValue=pr.PairProperty.Value;
          }
          
        }
      },
    },
  },
  methods: {
    GetVisibility(property, colIndex) {
      var res = property.Column == colIndex;
      return res;
    },
    readViewModel() {
      this.Get();
      this.GetChaild();
    },
    //----------------------------read ----------------------

    Get() {
      this.fetch(
        this.Set,
        "/Reagents/ReagentTypeEdit/Get?id=" + Id + "&copy=" + ForCopy
      );
    },

    Set(val) {
      if (val.Errors && val.Errors.length > 0) {
        this.ShowErrors(val);
      }
      var Models = val.Item;
      var model = new EditModel(Models);

      this.Model = model;
    },

    GetChaild() {
      if (ForCopy) return;
      if (!Id) return;
 //   if (this.isSingle) return;      
      this.fetch(
        this.SetChaild,
        "/Reagents/ReagentTypeEdit/GetChaild?parentId=" + Id
      );
    },

    SetChaild(val) {
      console.log(this.dataGrid);
      this.ChaildReagents = val;
    },
    //----------------------------end read ----------------------

    //-------end----Fields--------------------

    EditValueChanged: function () {
      for (var prName in this.Model) {
        var pr = this.Model[prName];
        if (pr.EditValue) {
          this.EnableSaveForm = true;
          return;
        }
      }
      this.EnableSaveForm = false;
    },
    Save: function () {
      var result = {};
      for (var crPropName in this.Model) {
        var crProp = this.Model[crPropName];
        if (crProp.EditValue) {
          //если значение редактировалось
          if (crProp.Auto) {
            result[crPropName] = null;
          } else {
              result[crPropName] = crProp.Value;
             if(crProp.PairProperty){
              if(crProp.PairProperty.Value!=crProp.PairProperty.DBValue)
               result[crProp.Name]= {Value:crProp.Value,PairPropertyValue:crProp.PairProperty.Value};
            }
          }
        }
      }
      var path = "/Reagents/ReagentTypeEdit/Save?id=" + Id;
      if (ForCopy) {
        path = "/Reagents/ReagentTypeEdit/Copy?id=" + Id;
      }

      this.fetch(this.OnSave, path, result);
    },
    OnSave: function (item) {
      if (item.Errors.length > 0) {
        this.ShowErrors(item);
      } else {
        window.parent.postMessage("RefreshGrid", "*"); //send message parent window for  RefreshGrid
        this.CloseThis();
      }
    },
    AddChaild: function () {
       AddReagents(Id);
    },
    
    RemoveChaild: function () {
      var ids = [];
      for (var i = 0; i < this.selectedChaild.length; ++i) {
        ids.push(this.selectedChaild[i]["Id"]);
      }

      var path = "/Reagents/ReagentTypeEdit/RemoveChaild?parentId=" + Id + "&ids=" + ids;

      this.fetch(this.GetChaild, path, null);
    },

    onSelectionChaildChanged({ selectedRowsData }) {
      this.selectedChaild = selectedRowsData;
    },
  },
  mounted: function () {
      this.readViewModel();
	  window.globalEvent.$on('addded-reagents', (ids) => {
     var path = "/Reagents/ReagentTypeEdit/AddChaild?parentId=" + Id + "&ids=" + ids;

      this.fetch(this.GetChaild, path, null);

	  })
  },
};
</script>

<style>
html,
body {
  height: 100%;
}

.v-text-field--box .v-input__control .v-input__slot,
.v-text-field--outline .v-input__control .v-input__slot,
.v-text-field .v-input__control .v-input__slot {
  min-height: 45px;
  display: flex !important;
  align-items: center !important;
  margin: 5px;
}

.custom.v-text-field > .v-input__control > .v-input__slot:before {
  border-style: none;
  width: 100px;
}

.custom.v-text-field > .v-input__control > .v-input__slot:after {
  border-style: none;
  width: 100px;
}

.style-red {
  color: rgb(255, 0, 0);
}
</style>
