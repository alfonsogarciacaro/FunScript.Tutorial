var http = require('http')
var finalhandler = require('finalhandler')
var serveStatic = require('serve-static')

var port = Number(process.argv[2])
port = isNaN(port) ? 8080 : port

var serve = serveStatic("./")
var server = http.createServer(function(req, res){
    var done = finalhandler(req, res)
    serve(req, res, done)
}).listen(port)
console.log("Server running at localhost:" + port)
