const Terminal={
	ws:null,
	onmessage:null,
	__id:0,
	
	start:function(url){
		let self = this;

		this.ws = new WebSocket(url);
        this.ws.onopen = function(evt) {
        	self._heartbeat();
            self._onmessage("connected...");
        }; 
        this.ws.onclose = function(evt) { 
            self._onmessage("disconnected..." + evt.data?evt.data:"");
            if(evt.data != "__server_close__"){
            	setTimeout(function(){
            		self.onmessage("reconnect...");
            		self.start(url);
            	},5000);
            }
        }; 
        this.ws.onmessage = function(evt) { 
            self._onmessage(evt.data, true);
        }; 
        this.ws.onerror = function(evt) { 
            self._onmessage("error:" + evt.data?evt.data:"");
        };
	},
	stop:function(){
		this.ws.close();
		this.ws = null;
	},

	//心跳包，30s左右无数据浏览器会断开连接Heartbeat
	_heartbeat:function(){
		let self = Terminal;
		if(self.ws){			
			self.ws.send("heartbeat");
			setTimeout(self._heartbeat, 20000);
		}
	},
	_onmessage:function(msg, remote){
		if(this.onmessage){
			if(remote){		
				try{		
					var data = JSON.parse(msg);
					if(data != null){
						var id = data.id;
						var cmd = data.cmd;
						var content = data.content;
						this.onmessage(content,data);
					}else{
						this.onmessage(content,null);
					}
				}catch(err){
					this.onmessage(msg,null);
				}
				
			}else{
				this.onmessage(msg,null);
			}
		}
	},
	send:function(msg){
		if(!msg || !this.ws){
			return false;
		}

		var args = msg.split(' ');
		if(args.length > 0){		
			var content = args.slice(1);	
			this.__id++;
			var data = {
				id : this.__id,
				cmd : args[0],
				content : content.join(" ")
			}
			var sendmsg = JSON.stringify(data);
			this.ws.send(sendmsg);
			return true;
		}
		return false;
	}
}

export default Terminal