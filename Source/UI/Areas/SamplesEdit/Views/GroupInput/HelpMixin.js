import { highlight, languages } from "prismjs/components/prism-core";
export const helpMixin = {

  data: () => ({
    helpMenu: [],
    openedItems: []
  })
  ,
  methods: {
    ItemBackground(item) {

      if (item.Id == this.Id) {

        return "rgba(107, 141, 252, 0.5)";
      }


    },

    GetHelpMenu() {
      this.fetch(this.SetHelpMenu, "/Help/Help/GetHelpMenu");
    },
    SetHelpMenu(val) {
      if (val.Errors && val.Errors.length > 0) {
        this.ShowErrors(val);
      }
      this.helpMenu = val.Item;

      if(typeof this.initialHelpMenuJson!='undefined'){
      this.initialHelpMenuJson=JSON.stringify(this.helpMenu);
      }

      for (let index = 0; index < this.helpMenu.length; index++) {
        const menuItem = this.helpMenu[index];//
        if (menuItem.Id == this.Id) return;
        this.chaildSearchAndOpenCurrent(menuItem.children)
      }

    },
    chaildSearchAndOpenCurrent(menuItems) {

      for (let index = 0; index < menuItems.length; index++) {
        const menuItem = menuItems[index];
          if (menuItem.Id == this.Id) {
          if (menuItem.ParentId) {
             this.openedItems.push(menuItem.ParentId);
          }
          break;
        }
        this.chaildSearchAndOpenCurrent(menuItem.children)
      }
    },

    highlighter(code) {
      return highlight(code, languages.js);
    },
  }
}

