var http = require('http');
var finalhandler = require('finalhandler');
var serveStatic = require('serve-static');

var path = process.argv[2];
var port = Number(process.argv[3]);
port = isNaN(port) ? 80 : port;

var serve = serveStatic("./" + path);
var server = http.createServer(function(req, res){
    var done = finalhandler(req, res);
    serve(req, res, done);
}).listen(port);
console.log("Server directed to " + path + " running at localhost:" + port);
