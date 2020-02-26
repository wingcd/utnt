<template>
	<div>
		<el-button style="width:100%;" @click="updateAll" class="el-icon-refresh" v-if="model.length==0"></el-button>
		<el-tree 
			:data="model" 
			:props="defaultProps" 
			:expand-on-click-node="true"
			accordion
			:render-content="renderContent"
			@node-click="handleNodeClick">
		</el-tree>
		<el-upload	
			v-show="false"	
			action=""
			:file-list="fileList"
			:before-upload="beforeUpload">
			<el-button size="small" type="primary"  ref="fileupload">点击上传</el-button>
		</el-upload>
		<el-dialog
		  :visible.sync="dialogVisible"
		  width="30%">
			<span>
			  <el-input
				type="textarea"
				:rows="15"
				placeholder="请输入内容"
				v-model="textarea">
			  </el-input>
			</span>
			<span slot="footer" class="dialog-footer">
				<el-button @click="dialogVisible = false">cancel</el-button>
				<el-button type="primary" @click="onDialogOK">accept</el-button>
			</span>
		</el-dialog>
		<el-dialog
		  :visible.sync="fileInfoDialogVisiable"
		  width="30%"
		  center>
		  	<ul>
				<li>name:{{fileInfoName}}</li>
				<li>path:{{fileInfoPath}}</li>
				<li>modify time:{{fileInfoTime}}</li>
				<li>size:{{fileInfoSize}} byte</li>
				<li v-if="fileInfoImage">
					<img style="width:100px;height:100px;" v-bind:src="fileInfoImage"></img>
				</li>
			</ul>
			<span slot="footer" class="dialog-footer">
				<el-button type="primary" @click="fileInfoDialogVisiable = false">OK</el-button>
			</span>
		</el-dialog>
	</div>
</template>

<script>
  export default {
  	name: 'ResourceManager',
	props: ['model'],
    data() {
      return {
        fileList:[],
        defaultProps: {
          children: 'children',
          label: 'name'
        },
      	currentNode:null,
      	currentUploadAction:"",
      	currentUploadDone:null,
      	dialogVisible:false,
      	textarea:"",

      	fileInfoDialogVisiable:false,
      	fileInfoName:"",
      	fileInfoPath:"",
      	fileInfoTime:"",
      	fileInfoSize:"",
      	fileInfoImage:null,
      };
    },
    methods: {
    	updateAll(){
    		this.$emit('updateroot');
    	},
		handleNodeClick(data,node) {
			this.currentNode = node;
			if(data.isdir && !node.expanded && data.children.length == 0){
				this.refresh(null,node, data)
			}
		},
		beforeUpload (file) {         
			let param = new FormData(); //创建form对象
			param.append('file',file,file.name);//通过append向form对象添加数据
			param.append('chunk','0');//添加form表单中其他数据
			console.log(param.get('file')); //FormData私有类对象，访问不到，可以通过get判断值是否传进去
		    
		    let config = {
				headers:{'Content-Type':'multipart/form-data'}
			};  //添加请求头
			this.$http.post(this.currentUploadAction,param,config)
				.then((response) => {
					this.$message({
						type: 'success',
						message: 'upload successed'
					});

					if(this.currentUploadDone){
						this.currentUploadDone(this,response);
						this.currentUploadDone = null;
					}				
				})
				.catch(function(response) {
					console.log(response);
					this.$message({
						type: 'info',
						message: 'upload failed'
					});		
				})     
		    return false // 返回false不会自动上传
		},
		renderContent(h, { node, data, store }) {
			var icon = null;
			if(data.isdir){
    			icon = node.expanded?<div class="icon open"></div>:<div class="icon close"></div>
    		}
    		else{
    			if(data.filetype == "txt"){
    				icon = <div class="icon txt"></div>
    			}else if(data.filetype == "img"){
    				icon = <div class="icon img"></div>
    			}else if(data.filetype == "zip"){
    				icon = <div class="icon zip"></div>
    			}else{
    				icon = <div class="icon file"></div>
    			}
    		}

			return (
				<span style="flex: 1; display: flex; align-items: center; justify-content: space-between; font-size: 18px; padding-right: 8px;">
					<span>
					    <span>
					    	{icon}			    	
					    	{node.label}
					    </span>
					</span>
				    <span> 
				    	{data.isdir? 
				    		<el-tooltip content="refresh" placement="top">
				    			<el-button class="button" type="text" icon="el-icon-refresh" on-click={ (e) => this.refresh(e, node, data)} ></el-button>
				    		</el-tooltip>
				    	:""}
				    	{!data.isdir && data.editable?
				    		<el-tooltip content="update" placement="top">
				    			<el-button class="button" type="text" icon="el-icon-refresh" on-click={ (e) => this.update(e, node, data)} ></el-button>
				    		</el-tooltip>			    		
				    	:""}
				      	{data.editable && data.isdir?
				      		<el-tooltip content="add fold" placement="top">
					      		<el-button class="button" type="text" icon="el-icon-circle-plus-outline" on-click={ (e) => this.addFold(e, node, data) }></el-button>
					      	</el-tooltip>
				      	:""}
				      	{data.isdir && data.editable? 
				    		<el-tooltip content="add file" placement="top">
				      			<el-button class="button" type="text" icon="el-icon-upload2" on-click={ (e) => this.addFile(e, node, data) }></el-button>
				      		</el-tooltip>
				      	:""}
				      	{data.renameable? 
				    		<el-tooltip content="rename" placement="top">
				      			<el-button class="button" type="text" icon="el-icon-edit" on-click={ (e) => this.rename(e, node, data) }></el-button>
				      		</el-tooltip>
				      	:""}
				      	{data.editable && data.filetype == "txt"? 
				    		<el-tooltip content="edit" placement="top">
				      			<el-button class="button" type="text" icon="el-icon-edit-outline" on-click={ (e) => this.editContent(e, node, data) }></el-button>
				      		</el-tooltip>
				      	:""}
				      	{!data.isdir?
				    		<el-tooltip content="download" placement="top">
				      			<el-button class="button" type="text" icon="el-icon-download" on-click={ (e) => this.download(e, node, data) }></el-button>
				      		</el-tooltip>
				      	:""}
				      	{data.editable && data.filetype == "zip"?
				    		<el-tooltip content="unzip" placement="top">
				      			<el-button class="button" type="text" icon="el-icon-tickets" on-click={ (e) => this.unzip(e, node, data) }></el-button>
				      		</el-tooltip>
				      	:""}
				      	{data.editable && data.path != "/" ?
				      		<el-tooltip content="delete" placement="top">
				      			<el-button class="button" type="text" icon="el-icon-delete" on-click={ (e) => this.deleteFile(e, node, data) }></el-button>
				      		</el-tooltip>
				      	:""}
				      	<el-tooltip content="detail" placement="top">
			      			<el-button class="button" type="text" icon="el-icon-info" on-click={ (e) => this.showDetail(e, node, data) }></el-button>
			      		</el-tooltip>
				    </span>	
			    </span>
			);
		},
		stopPro(evt){
			if(!evt){
				return
			}
		    var e = evt || window.event;
		    window.event?e.cancelBubble=true:e.stopPropagation(); 
		},
		refresh(e, node, data){
			this.stopPro(e);
			if(data.isdir){
				this.$http.get(this.g.baseUrl + "/api/getdir?type="+data.dirtype+"&path="+data.path)
					.then((response) => {
						node.data.name = response.data.name;
						node.data.path = response.data.path;
						node.data.children = response.data.children;
						node.expanded = true     				
					})
					.catch(function(response) {
						console.log(response);
					})
			}else{
				
			}
		},
		update(e, node, data){
			this.stopPro(e);
			this.$confirm('update file will change old file,go on?', 'Warning', {
		          confirmButtonText: 'accept',
		          cancelButtonText: 'cancel',
		          type: 'warning'
		        }).then(() => {
		        	this.currentUploadAction = this.g.baseUrl + "/api/replacefile?type="+data.dirtype+"&path="+data.path
					this.$refs.fileupload.$parent.handleClick()
		        }).catch(() => {
		          this.$message({
		            type: 'info',
		            message: 'canceled!'
		          });          
		        });
		},
		addFold(e, node, data){
			this.stopPro(e);
			this.$prompt('Please input folder name', 'Tips', {
				confirmButtonText: 'Accept',
				cancelButtonText: 'Cancel',
				inputPattern: /\S+/,//^[0-9|A-z|_|-|\.]+$/,
				inputErrorMessage: "name can not be empty",//'名称不能为空且不能有特殊字符',
				inputValue:"NewFolder"
			}).then(({ value }) => {
				this.$http.get(this.g.baseUrl + "/api/addfold?type="+data.dirtype+"&path="+data.path+"&name="+value)
					.then((response) => {
						node.data = response.data;
						node.expanded = true;
						this.$message({
							type: 'success',
							message: 'add folder named ' + value
						});				
					})
					.catch(function(response) {
						this.$message({
							type: 'info',
							message: 'failed to add folder named ' + value
						});		
					})
			}).catch(() => {
				this.$message({
					type: 'info',
					message: 'canceled'
				});       
			});
		},
		addFile(e, node, data){
			this.stopPro(e);
			this.currentUploadAction = this.g.baseUrl + "/api/addfile?type="+data.dirtype+"&path="+data.path
			this.$refs.fileupload.$parent.handleClick()
			this.currentUploadDone = (self, response)=>{
				node.data = response.data;
				node.expanded = true;
			};
		},
		rename(e, node, data){
			this.stopPro(e);
			this.$prompt('Please input name', 'Tips', {
				confirmButtonText: 'Accept',
				cancelButtonText: 'Cancel',
				inputPattern: /\S+/,//^[0-9|A-z|_|-|\.]+$/,
				inputErrorMessage: "name can not be empty",//'名称不能为空且不能有特殊字符',
				inputValue:data.name
			}).then(({ value }) => {
				this.$http.get(this.g.baseUrl + "/api/rename?type="+data.dirtype+"&path="+data.path+"&name="+value)
					.then((response) => {
						node.data.name = response.data.name;
						node.data.path = response.data.path;
						node.data.children = response.data.children;
						this.$message({
							type: 'success',
							message: 'changed name to ' + value
						});				
					})
					.catch(function(response) {
						this.$message({
							type: 'info',
							message: 'failed to changed name to ' + value
						});		
					})
			}).catch(() => {
				this.$message({
					type: 'info',
					message: 'canceled'
				});       
			});
		},
		editContent(e, node, data){
			this.$http.get(this.g.baseUrl + "/api/getfile?type="+data.dirtype+"&path="+data.path, 
					{emulateJSON: true}
				)
				.then((response) => {	
					this.textarea = response.bodyText;
					this.dialogVisible = true;			
				})
				.catch(function(response) {
					this.$message({
						type: 'info',
						message: 'failed get content ' + JSON.stringify(response)
					});		
				})
		},
		onDialogOK(){
			let data = this.currentNode.data;
			let param = new FormData();
			param.append('content',this.textarea);
			this.$http.post(this.g.baseUrl + "/api/setcontent?type="+data.dirtype+"&path="+data.path,param)
				.then((response) => {
					this.dialogVisible = false;
					this.$message({
						type: 'success',
						message: 'update content successed'
					});			
				})
				.catch(function(response) {
					this.$message({
						type: 'info',
						message: 'update content failed'
					});		
				}) 
		},
		download(e, node, data){
			this.stopPro(e);
			window.open(this.g.baseUrl + "/api/getfile?type="+data.dirtype+"&path="+data.path);
		},
		deleteFile(e, node, data){
			this.stopPro(e);
			this.$confirm('this operation will delete file '+data.name+', do you want to go on?', 'Warning', {
		          confirmButtonText: 'accept',
		          cancelButtonText: 'cancel',
		          type: 'warning'
		        }).then(() => {
					this.$http.get(this.g.baseUrl + "/api/delete?type="+data.dirtype+"&path="+data.path)
					.then((response) => {
						node.parent.data = response.data;
						this.$message({
							type: 'success',
							message: 'delete successed'
						});				
					})
					.catch(function(response) {
						this.$message({
							type: 'info',
							message: 'delete failed'
						});		
					})
		        }).catch(() => {
		          this.$message({
		            type: 'info',
		            message: 'canceled!'
		          });          
		        });
		},
		showDetail(e, node, data){
			this.stopPro(e);

			this.fileInfoDialogVisiable = true;
			this.fileInfoName = data.name;
			this.fileInfoPath = data.path;
	      	this.fileInfoTime = data.modifytime;
	      	this.fileInfoSize = data.size;
	      	if(data.filetype == "img"){
	      		this.fileInfoImage = this.g.baseUrl + "/api/getfile?type="+data.dirtype+"&path="+data.path;
	      	}else{
	      		this.fileInfoImage = "";
	      	}
		},
		unzip(e, node, data){
			this.stopPro(e);

			this.$http.get(this.g.baseUrl + "/api/unzip?type="+data.dirtype+"&path="+data.path)
				.then((response) => {
					node.parent.data = response.data;
					this.$message({
						type: 'success',
						message: 'unzip successed'
					});				
				})
				.catch(function(response) {
					this.$message({
						type: 'info',
						message: 'unzip failed'
					});		
				})
		}
	}
 };
</script>

<style>
	.button {
		font-size: 12px;
	}
	.icon{
		float:left;
		border: 0;
		height: 18px;
		width: 18px;
		background-repeat:no-repeat; 
		background-size:100% 100%;
		-moz-background-size:100% 100%;
	}
	.icon.close{		
		background-image: url(/static/icons/dir-1.svg);
	}
	.icon.open{		
		background-image: url(/static/icons/dir-open.svg);
	}
	.icon.file{		
		background-image: url(/static/icons/file.svg);
	}
	.icon.txt{		
		background-image: url(/static/icons/file-text-o.svg);
	}
	.icon.img{		
		background-image: url(/static/icons/file-image.svg);
	}
	.icon.zip{		
		background-image: url(/static/icons/zip.svg);
	}
	ul{
	    list-style-type: none;
	    padding: 0px;
	    margin: 0px;
	}
	ul li{
		text-align: left;
	    padding-left: 0px; 
	}
</style>