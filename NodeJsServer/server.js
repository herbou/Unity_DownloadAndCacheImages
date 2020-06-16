var st = require("node-static");

var file = new st.Server("./public");
const port = 9000;

require("http").createServer((req,res)=>{
	req.addListener("end",()=>{
		file.serve(req,res);
	}).resume();
}).listen(port);

console.log(".  Listening on :  http://127.0.0.1:"+port+"/");