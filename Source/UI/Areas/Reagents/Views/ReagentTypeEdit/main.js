import Vue from 'vue';
import Vuetify from 'vuetify';
import 'vuetify/dist/vuetify.css';
import App from './ReagentTypeEdit.vue';

let production = false;

Vue.config.productionTip = !production;
Vue.config.devtools = !production;
Vue.config.debug = !production;
Vue.config.silent = production;

Vue.use(Vuetify);

const vuetify = new Vuetify({
	theme: {
		myColor: "#e3f2fd" // "#545454"
	}
});
window.globalEvent = new Vue();
const app = new Vue({
	el: '#app',
	vuetify,
	render: h => h(App)
});


