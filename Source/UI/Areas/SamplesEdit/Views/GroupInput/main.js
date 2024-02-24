import Vue from 'vue'
import App from './GroupInput.vue'
//import vuetify from '../../../VueCore/vuetify'
import vuetify from './plugins/vuetify'
Vue.config.productionTip = false
window.globalEvent = new Vue();


new Vue({
  vuetify,
  render: h => h(App)
}).$mount('#app')
