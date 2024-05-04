

function createGridLookup(el, dataSource, columns, val, pageSize, height, valChanged) {
    const $dataGrid = $('<div>').dxDataGrid({

        dataSource: dataSource,
        keyExpr:"id",
        export: false,
        stateStoring: false,
        columnChooser: { enabled: true },
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

//    const dataGrid = $dataGrid.dxDataGrid('instance');

    //dataGrid.selectRows(val, false);

    //dataGrid.on('selectionChanged', (e) => {
    //    const { selectedRowKeys } = e;
    //    valChanged(selectedRowKeys);
    //});

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

import { componentBase } from '/js/vue3Components.js';

export const KfGridLookUp = {
    mixins: [componentBase],
    props: {
        'value': { type: [String, Number] },
        "items": { type: Array, default: () => [] },
        "loadUrl": { type: [String, Object], default: null },
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
            displValue: null,
            tableInit: false
        }
    },
    watch: {
        'value': function (newVal) {
            this.updateDisplayValue(newVal);
        },
        'loadUrl': function (newUrl) {
            this.refreshGrid();
        }
    },
    methods: {
        toggleTableVisibility() {
            this.tableVisible = !this.tableVisible;
            if (this.tableVisible) {
                if (!this.tableInit) this.refreshGrid()
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
            this.$emit('input', row.key); // Эмитим обновленное значение в компоненте-родителе
            this.displValue = this.parseDisplayExpr(this.displayExpr, row.data, row.key);
            if (this.tableVisible) this.toggleTableVisibility();
        },
        refreshGrid() {
            this.tableInit = true;
            const el = $('#' + this.lookUpId);
            el.empty(); // Clear previous grid before creating new one

            const ds = this.loadUrl ? getDataSource(this.loadUrl) : this.items;
            createGridLookup(el, ds, this.columns, this.value, this.pageSize, this.dropDownHeight, this.valueChanged);
        },
        parseDisplayExpr(expr, data, fallbackKey) {
            try {
                if (/\$\{[^}]+\}/.test(expr)) {
                    return expr.replace(/\$\{([^}]+)\}/g, (match, key) => data[key.trim()] || '');
                } else {
                    return data[expr] || data[fallbackKey];
                }
            } catch (error) {
                console.error("Error processing display expression:", error);
                return data[fallbackKey];
            }
        },
        updateDisplayValue(value) {
            const localRow = this.items.find(item => item.id === value);
            if (localRow) {
                this.displValue = this.parseDisplayExpr(this.displayExpr, localRow, value);
            } else if (this.loadUrl && value) {
                this.fetchData(value);
            }
        },
        async fetchData(value) {
            try {
                const response = await fetch(`${this.loadUrl}?id=${value}`);
                let data = await response.json();
                if (data) {
                    if (Array.isArray(data)) { data = data[value] }
                    this.displValue = this.parseDisplayExpr(this.displayExpr, data, value);
                }
            } catch (error) {
                console.error("Error fetching data:", error);
                this.displValue = value; // Fallback to the key value on error
            }
        }
    },
    mounted() {
        this.updateDisplayValue(this.value);
    },
    template: `
<div class="cf-dropdown-container">
    <div class="coll"
         style="width:100px; display:inline-block">
        {{ text }}:
    </div>

    <div style="width:150px; height:30px; overflow:hidden; border: solid 1px; display:inline-block"
         :title="notValidText">
        {{ displValue }}
    </div>

    <button style="display:inline-block"
            @click="toggleTableVisibility">
        Find
    </button>

    <img v-if="!valid"
         style="display:inline-block"
         src="invalid.png"/>

    <div class="cf-dropdown"
         :id="lookUpId"
         :style="{ display: tableVisible ? 'block' : 'none' }"
         @click.outside="hideTable">
    </div>
</div>
    `,
};
