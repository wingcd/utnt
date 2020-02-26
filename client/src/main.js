// The Vue build version to load with the `import` command
// (runtime-only or standalone) has been set in webpack.base.conf with an alias.
import Vue from 'vue'
import VueResource from 'vue-resource'
import ElementUI from 'element-ui'
import locale from 'element-ui/lib/locale/lang/en'
import 'element-ui/lib/theme-chalk/index.css'

var VueQuillEditor = require('vue-quill-editor')
// mount with global
Vue.use(VueQuillEditor)
// If used in Nuxt.js/SSR, you should keep it only in browser build environment
// if (process.browser) {
//   const VueQuillEditor = require('vue-quill-editor/ssr')
//   Vue.use(VueQuillEditor)
// }
import { quillEditor } from 'vue-quill-editor'

import App from './App'

Vue.config.productionTip = false;
Vue.use(VueResource);
Vue.use(ElementUI,{locale});

Vue.prototype.g = {
	baseUrl : "http://{url}:{port}/",
	baseWSUrl : "ws://{url}:{port}/terminal"
} 

/* eslint-disable no-new */
new Vue({
  el: '#app',
  template: '<App/>',
  components: { 
  	quillEditor,
  	App 
  },
  beforeCreate: function () {
	 var port = window.location.port;
	 var url = window.location.host.replace(":"+port, "");
	 var tempPort = port; //"8086"; //
	 this.g.baseUrl = this.g.baseUrl.replace("{url}",url).replace("{port}",tempPort);
	 this.g.baseWSUrl = this.g.baseWSUrl.replace("{url}",url);		
  }
})
