
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

function createGridLookup(el,dataSource, columns, val, pageSize, height, valChanged) {
    const $dataGrid = $('<div>').dxDataGrid({
        dataSource: dataSource,
        export: false,
        stateStoring: false,
        columnChooser: { enabled: true},
        toolbar: { items: [] },
        columns: columns,
        hoverStateEnabled: true,
        paging: { enabled: true, pageSize: pageSize },
        filterRow: { visible: true },
        height: height,
        selection: {
            mode: 'none',
        },
        focusedRowEnabled: true, // Включаем возможность фокусировки на строках
        focusedRowKey: val, // Устанавливаем начальную фокусированную строку
        onRowDblClick: valChanged
    });

    const dataGrid = $dataGrid.dxDataGrid('instance');

    dataGrid.selectRows(val, false);

    dataGrid.on('selectionChanged', (e) => {
        const { selectedRowKeys } = e;
        valChanged(selectedRowKeys);
    });

    $(el).append($dataGrid);
}


function getDataSource(loadUrl) {
    console.log(loadUrl)
    const dataSource = DevExpress.data.AspNet.createStore({
        key: "id",
        loadUrl: document.location.origin + loadUrl,
    });
    return dataSource;

}

function CreateGuid() {
    function _p8(s) {
        let p = (Math.random().toString(16) + "000000000").substr(2, 8);
        return s ? "-" + p.substr(0, 4) + "-" + p.substr(4, 4) : p;
    }
    return _p8() + _p8(true) + _p8(true) + _p8();
}
import {  componentBase } from '/js/vue3Components.js'
export const KfGridLookUp =  {
    mixins: [componentBase],
    props: {
        'value': { type: [String, Number] },
        "items": { type: Array, default: null },
        "loadUrl": { type: [String, Object], default: "zzz" },
        "pageSize": { type: Number, default: 10 },

        "dropDownWidth": { type: Number, default: 700 },
        "dropDownHeight": { type: Number, default: 500 },
        "displayExpr": { type: [String, Object], default: "name" },
        "columns": { type: Array, default: null },
    },
    data() {
        return {
            lookUpId: CreateGuid(),
            tableVisible: false,
            displValue:'null'
        }
    },

    methods: {
        toggleTableVisibility() {
            this.tableVisible = !this.tableVisible;
            if (this.tableVisible) {
                // При открытии таблицы добавляем обработчик кликов на уровне документа
                document.addEventListener('click', this.hideTable);
            } else {
                // При закрытии таблицы удаляем обработчик кликов на уровне документа
                document.removeEventListener('click', this.hideTable);
            }
        },
        hideTable(event) {
  
            const isOutsideClick = !this.$el.contains(event.target);
            if (isOutsideClick) {
                this.tableVisible = false;
                document.removeEventListener('click', this.hideTable);
            }
        },
        valueChanged(row) {
            console.log(row)
            if (this.tableVisible) this.toggleTableVisibility()
            const displayValue = typeof this.displayExpr === 'function' ? this.displayExpr(row.data) : this.parseDisplayExpr(this.displayExpr, row.data, row.key);
            this.displValue = displayValue;

        }
        , parseDisplayExpr(expr, data, fallbackKey) {
            try {
                if (/\$\{[^}]+\}/.test(expr)) {
                    // It's a template string
                    return expr.replace(/\$\{([^}]+)\}/g, (match, key) => data[key.trim()] || '');
                } else {
                    // It's likely a column name
                    return data[expr] || data[fallbackKey];
                }
            } catch (error) {
                console.error("Error processing display expression:", error);
                return data[fallbackKey]; // Fallback to key value on error
            }
        }
    },
    template: `
  <div class="cf-dropdown-container" >
     <div class="coll" style="width:100px;display:inline-block"  >{{ text }}:</div>

      <div style="width:150px;height:30px;border: solid 1px;display:inline-block;owerflow:hidden"
      :title="notValidText" 
      >{{displValue}}</div>
 
        <button style="display:inline-block" @click="toggleTableVisibility"  >Find</button>

        <img v-if="!valid"  style="display:inline-block"  src="invalid.png"/>
         <div
         class="cf-dropdown"
         v-bind:id="lookUpId"
         :style="{ display: tableVisible ? 'block' : 'none' }"
         @click.outside="hideTable"
         >
         </div>
  
  </div>
  `,
    mounted: function () {
        console.log(this.value + '   ' + this.displayExpr)
        const el = $('#' + this.lookUpId)

        createGridLookup(el, getDataSource(this.loadUrl), this.columns, this.value, this.pageSize, this.dropDownHeight, this.valueChanged)
    }
}