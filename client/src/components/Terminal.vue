<template>
	<span>
		<div class="consoleContainer">
			<quill-editor id="console"
					ref="console"
					v-model="textarea"
					:options="editorOption"
					class="richConsole">
	        </quill-editor>
        </div>
		<el-input
			placeholder="请输入内容" 
			v-model="inputarea"
			prefix-icon="el-icon-arrow-right"
			@keyup.up.native="onUp"
			@keyup.down.native="onDown"
			@keyup.enter.native="onEnter">	
		</el-input>
	</span>
</template>

<script>
import T from '../api/terminal.js'
export default {
	name: 'Terminal',
	data () {
		return {
			__firstToBottom:true,
			editorOption: {
				readOnly:true,
				placeholder:"input command...",
				modules: {
					toolbar: '',
				}
	        },
	        svrConfigData:null,

			textarea:"",
			inputarea:"",
			isResponse:true,
			commands:[],
			cmdIndex:0,
		}
	},
  	watch:{
  	},
	methods:{
		onEnter(){
			this.requst();
		},
		onUp(){
			if(this.cmdIndex > 0){
				this.cmdIndex--;
				this.inputarea = this.commands[this.cmdIndex];
			}
		},
		onDown(){
			if(this.cmdIndex < this.commands.length - 1){
				this.cmdIndex++;
				this.inputarea = this.commands[this.cmdIndex];
			}else{
				this.cmdIndex = this.commands.length;
				this.inputarea = "";
			}
		},
		requst(){
			let msg = this.inputarea;

			let timeout = true;
			let self = this;
			if(this.isResponse){
				var quill = this.$refs.console.quill;
				var count = quill.getLength();
				quill.insertText(count, ">> " + msg.trim("\n"));	
				this.inputarea = "";	
				this.isResponse = false;

				this.__pretreatment(msg);

    			setTimeout(()=>{
					if(!self.isResponse){
						self.response("command timeout...");
					}
				}, 5000);

				this.trim();
    			this._scrollToBottom();
			}			
		},
		trim(){
			var quill = this.$refs.console.quill;
			var ops = quill.editor.delta.ops;
			var count = ops.length;
			let maxlen = this.svrConfigData ? this.svrConfigData.maxRow : 1000;
			if(count > maxlen){
				let length = count - maxlen;
			 	quill.updateContents([{
					  delete: length,
					}]);
			}
		},
		response(msg, remoteInfo){
			if(!msg && msg != "" && remoteInfo == null){
				return;
			}

			let onbuttom = this._isOnButtom();
			
			var quill = this.$refs.console.quill;
			var count = quill.getLength();
			this.isResponse = true;

			if(remoteInfo != null){
				if(remoteInfo.type == "text"){			
					var inline = remoteInfo.inline;
					quill.insertText(count, msg, inline);
				}else if(remoteInfo.type == "image"){
					quill.insertEmbed(count, 'image', msg);
				}else if(remoteInfo.type == "delete"){
					quill.updateContents([{
					  delete: remoteInfo.value,
					}]);
				}else{
					quill.insertText(count, msg);
				}
			}else{
				quill.insertText(count, msg);
			}	

			this.trim();	

			if(onbuttom){
				this._scrollToBottom();
			}
		},
		_isOnButtom(){
			let textArea = document.getElementById('console').children[0].children[0];
			this.__firstToBottom = this.__firstToBottom && textArea.scrollHeight>textArea.clientHeight;

			return this.__firstToBottom || Math.abs(textArea.scrollHeight - textArea.scrollTop - textArea.offsetHeight) < 100;
		},
		_scrollToBottom(){
			this.__firstToBottom = this.__firstToBottom ? false : true;

			let textArea = document.getElementById('console').children[0].children[0];
			textArea.scrollTop = textArea.scrollHeight - textArea.offsetHeight;
		},
		__pretreatment(msg){
			msg = msg.trim();
			let args = msg.split(" ");
			if(args.length > 0){

				this.commands.push(msg);
				this.cmdIndex = this.commands.length;

				var cmd = args[0];
				if(cmd == "clear"){
					this.textarea = "";
					this.isResponse = true;
					return;
				}else if(cmd == "help"){
					this.onmessage("clear:clear console");
				}

				if(!T.send(msg)){
					this.isResponse = true;
				}				
			}else{
				this.isResponse = true;
			}
		},
		onmessage(msg, info){
			this.response(msg, info);
		},
		start(data){
			this.svrConfigData = data;

			var url = this.g.baseWSUrl.replace("{port}", data.port);
			T.start(url);
			T.onmessage = this.onmessage;

			this.inputarea = data.defaultInput;
		}
	},
	created: function () {
		// this.$http.get(this.g.baseUrl + "/api/getternimal")
		//    .then((response) => {
		//    	this.platform = response.data.platform;
		// 	this.treeModel = response.data.children;
		//    })
		//    .catch(function(response) {
		//      console.log(response)
		//    })
	},
	mounted: function(){	
		var console = document.getElementById('console');
		console.children[0].style.border="0px";
		var quill = this.$refs.console.quill;
		quill.addContainer('ql-custom');
	}
}
</script>

<style>
	.console .el-textarea__inner{
		background-color: #000000;
		color:#ffffff;	
	}
	.richConsole{
		background-color: #000000;
		color:#ffffff;	
		
	    padding: 5px 5px 5px 0px;
	    box-sizing: border-box;

	    width: 100%;
	    height:100%;
	    border:0px solid #ccc;
	}
	.consoleContainer{
		height: 500px;

	    overflow:auto;
	    resize: vertical;
	    min-height:200px;

	    border-radius: 4px;
	    transition: border-color .2s cubic-bezier(.645,.045,.355,1);
	}
	/*.console .el-textarea__inner ::-webkit-scrollbar{
		width: 2px;
	}
	.console .el-textarea__inner ::-webkit-scrollbar-track{
		-webkit-box-shadow: inset 0 0 1px rgba(0,0,0,0.3);
		border-radius: 10px;
	}*/
</style>