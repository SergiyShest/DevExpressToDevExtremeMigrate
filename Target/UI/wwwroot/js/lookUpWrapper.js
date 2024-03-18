
 let products = [{
    "Id": 1,
    "CompanyName": "Premier Buy",
    "Address": "7601 Penn Avenue South",
    "City": "Richfield",
    "State": "Minnesota",
     "Name":        "Vasiliy",
    "Phone": "(612) 291-1000",
    "Fax": "(612) 291-2001",
    "Website": "http://www.nowebsitepremierbuy.dx"
 },
     {
         "Id": 2,
         "CompanyName": "Premier Buy",
         "Address": "7601 Penn Avenue South",
         "City": "Richfield",
         "State": "Minnesota",
         "Name": "Petrillo",
         "Phone": "(612) 291-1044",
         "Fax": "(612) 291-2004",
         "Website": "http://www.nowebsitepremierbuy.dx"
     },
 ]

let dataGrid;
const makeAsyncDataSource = function (jsonFile) {
    return new DevExpress.data.CustomStore({
        loadMode: 'raw',
        key: 'Id',
        load() {
            return products;
        },
    });
};

function createLookUp(el, columns, val, dropDownWidth, pageSize, displayExpr, valChanged) {
    el.dxDropDownBox({
        value: val,
        valueExpr: 'Id',
        placeholder: 'Select a value...',
        displayExpr: displayExpr,
        showClearButton: true,
        dataSource: makeAsyncDataSource('customers.json'),
        onValueChanded: valChanged,
        dropDownOptions: {
            width: dropDownWidth
        },
        contentTemplate(e) {
            const v = e.component.option('value');
            const $dataGrid = $('<div>').dxDataGrid({
                dataSource: e.component.getDataSource(),
                columns: columns,// ['Id', 'CompanyName', 'City', 'Phone', 'State'],
                hoverStateEnabled: true,
                paging: { enabled: true, pageSize: pageSize },
                filterRow: { visible: true },
                height: 500,
                selection: {
                    mode: 'single',
                },
                selectedRowKeys: v,
                onSelectionChanged(selectedItems) {
                    const keys = selectedItems.selectedRowKeys;
                    e.component.option('value', keys);
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

function setDataSource() {
    const dataSource = DevExpress.data.AspNet.createStore({
        key: "Id",
        loadUrl: "https://localhost:7225/Api/GetOsts",
    });
    let grid = $("#gridBox").dxDropDownBox("instance");
    grid.option("dataSource", dataSource);
}

function CreateGuid() {
    function _p8(s) {
        let p = (Math.random().toString(16) + "000000000").substr(2, 8);
        return s ? "-" + p.substr(0, 4) + "-" + p.substr(4, 4) : p;
    }
    return _p8() + _p8(true) + _p8(true) + _p8();
}

export const KfGridLookUp =  {
    mixins: [componentBase],
    props: {
        'value': { type: [String, Number] },
        "items": { type: Array, default: null },
        "loadUrl": String,
        "pageSize": { type: Number, default: 10 },

        "dropDownWidth": { type: Number, default: 700 },
        "displayExpr": { type: [String, Object], default: "CompanyName" },
        "columns": { type: Array, default: null },
    },
    data() {
        return {
            lookUpId: CreateGuid()
        }
    },

    methods: {

    },
    template: `
  <div class="flex-row" >
     <div class="coll" >{{ text }}<span v-if="req" style="color:red">*</span>:</div>
      <div 
      v-bind:id="lookUpId"
      :title="notValidText" 
      v-bind:value="value"
      v-bind:readonly="dis"
      v-on:input="valChanged($event)"
      />
     <img v-if="!valid" src="invalid.png"></img> 
  </div>
  `,
    mounted: function () {
        console.log(this.value + '   ' + this.displayExpr)
        const el = $('#' + this.lookUpId)
        let vChanged = alert
        createLookUp(el, this.columns, this.value, this.dropDownWidth, this.pageSize, this.displayExpr, vChanged)
    }
}