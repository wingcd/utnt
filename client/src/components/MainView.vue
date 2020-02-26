<template>
	<el-container>
		<el-header>
			<el-row>
				<el-col :span="8">
					<div class="logo">
    					<img :src="logo"/>
    				</div>
				</el-col>
				<el-col :span="8">
					<div>UTNT</div>
					<div style="font-size:16px">
						<font color="#0000a0">U</font>nity 
						<font color="#0000a0">T</font>erminal
						<font color="#0000a0">N</font>ot only
						<font color="#0000a0">T</font>erminal
					</div>
				</el-col>
				<el-col :span="8">
				</el-col>
			</el-row>
		</el-header>
		<el-main>
			<div>
				<el-row>
					<div>{{platform}}</div>
				</el-row>
				<el-row>
					<el-col :span="14" :offset="5">
						<div v-if="enableTerminal">
							<terminal ref="terminal"></terminal>
						</div>
						<div class="grid-content bg-purple-light">
							<!--ul v-for="item in treeModel">
								<tree-menu :model="item"></tree-menu>
							</ul-->
							<tree-menu @updateroot="updateRootDir" :model="treeModel"></tree-menu>
						</div>
					</el-col>
				</el-row>		 
			 </div>
		</el-main>
	</el-container>	
</template>

<script>

import logo from '../assets/logo.png'

import treeMenu from './ResourceManager.vue'
import terminal from './Terminal.vue'
export default {
	name: 'MainView',
	components: {
		treeMenu,
		terminal
	},
	data() {
		return {
			logo:logo,

			treeModel: [],
			platform: "ios",
			enableTerminal:true
		}
	},
	methods:{
		updateRootDir(){
			this.$http.get(this.g.baseUrl + "/api/getrootdir")
			    .then((response) => {
			    	this.platform = response.data.platform;
					this.treeModel = response.data.children;
			    })
			    .catch(function(response) {
			      console.log(response)
			    })		
		}
	},
	mounted: function () {	    
		this.enableTerminal = WebSocket;
		if(this.enableTerminal){
			this.$http.get(this.g.baseUrl + "/api/terminalinfo")
			    .then((response) => {
			    	this.enableTerminal = response.data.enable;
			    	if(this.enableTerminal){
			    		this.$refs.terminal.start(response.data || {});	 
			    	}   	
			    })
			    .catch(function(response) {
			      console.log(response)
			    })
		}		

		this.updateRootDir()
	}
}
</script>

<style>
   .logo {
   		float:right;
   		margin-top: 5px;
   }

   .logo img {
   		width: 60px;
   		height: 50px;   		
   }

  .el-header, .el-footer {
    background-color: #B3C0D1;
    color: #333;
    text-align: center;
    #line-height: 30px;
    font-size: 30px;
    font-weight: bold;
  }
  
  .el-aside {
    background-color: #D3DCE6;
    color: #333;
    text-align: center;
    line-height: 200px;
  }
  
  .el-main {
    background-color: #E9EEF3;
    color: #333;
    text-align: center;    
    height: 800px;
  }
  
  body > .el-container {
    margin-bottom: 40px;
  }
  
  .el-container:nth-child(5) .el-aside,
  .el-container:nth-child(6) .el-aside {
    line-height: 260px;
  }
  
  .el-container:nth-child(7) .el-aside {
    line-height: 320px;
  }
</style>