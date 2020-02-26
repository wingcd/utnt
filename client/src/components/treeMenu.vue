<template>
	<el-container>
	  <el-header>
	  	<span v-on:click="toggle">
				<div v-if="isFold" class="el-icon-document" :class="[open ? 'el-icon-document': 'el-icon-date']"></div>
				{{ model.name }}
		</span>
		<span style="margin: 0px 0">
			<el-tooltip content="refresh" placement="top" v-if="model.isdir">
				<el-button type="primary" plain size="mini" icon="el-icon-refresh"></el-button>
			</el-tooltip>
			<el-tooltip content="add dir" placement="top" v-if="model.isdir">
				<el-button type="primary" plain size="mini" icon="el-icon-circle-plus"></el-button>
			</el-tooltip>
			<el-tooltip content="add file" placement="top" v-if="model.isdir">
				<el-button type="primary" plain size="mini" icon="el-icon-circle-plus-outline"></el-button>
			</el-tooltip>
			<el-tooltip content="rename" placement="top" v-if="model.editable">
				<el-button type="primary" plain size="mini" icon="el-icon-edit"></el-button>
			</el-tooltip>
			<el-tooltip content="edit" placement="top" v-if="model.modifiable">
				<el-button type="primary" plain size="mini" icon="el-icon-edit-outline"></el-button>
			</el-tooltip>
			<el-tooltip content="download" placement="top" v-if="!model.isdir">
				<el-button type="primary" plain size="mini" icon="el-icon-download"></el-button>
			</el-tooltip>
			<el-tooltip content="delete" placement="top" v-if="model.editable">
				<el-button type="primary" plain size="mini" icon="el-icon-delete"></el-button>
			</el-tooltip>
		</span>
	  </el-header>
	  <el-main v-show="open" v-if="isFold">
			<tree-menu v-for="item in model.children" :model="item"></tree-menu>
	  </el-main>
	</el-container>
</template>
<script>
export default {
	name: 'treeMenu',
	props: ['model'],
	data() {
		return {
			open: false,
			//isFold: true
		}
 	},
	computed: {
		isFold: function() {
			return this.model.isdir;
		}
	},
 	methods: {
		toggle: function() {
			if (this.isFold === true) {
				this.open = !this.open;
				if(this.open){
					this.$http.get(this.g.baseUrl + "/api/getdir?type="+this.model.dirtype+"&path="+this.model.path)
					    .then((response) => {
							this.model.children = response.data.children;
					    })
					    .catch(function(response) {
					    	console.log(response);
					    })
				}
			}
		}
	}
}
</script>

<style>
  .el-header, .el-footer {
    background-color: #B3C0D1;
    color: #333;
    text-align: left;
    line-height: 60px;
  }

  .right{
  	text-align: right;
  }
  
  .el-aside {
    background-color: #D3DCE6;
    color: #333;
    text-align: center;
    line-height: 200px;
  }
  
  .el-main {
    background-color: #E9EEF3;
    color: #033;
    text-align: center;
    line-height: 160px;
  }
  
  body > .el-container {
    margin-bottom: 0px;
  }
  
  .el-container:nth-child(5) .el-aside,
  .el-container:nth-child(6) .el-aside {
    line-height: 260px;
  }
  
  .el-container:nth-child(7) .el-aside {
    line-height: 320px;
  }
</style>