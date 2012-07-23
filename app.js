var flatiron = require('flatiron'),
    path = require('path'),
    http = require('http'),
    director = require('director'),
    connect = require('connect'),
    templater = require('./lib/templater.js'),
    stylus = require('stylus'),
    assetus = require('./lib/assetus.js');

templater.setTemplatePath(__dirname + '/resources/views/');

var routes = require('./routes.js');
var router = new director.http.Router(routes);

/*var assetOptions = {
    root: __dirname + "/resources/assets/",
    destination: __dirname + "/resources/public/"
};*/

var app = connect()
    //.use(assetus(assetOptions))
    .use(stylus.middleware({
        src: __dirname + "/resources/assets",
        dest: __dirname + "/public",
        compress: true
    }))
    .use(connect.static(__dirname + '/public/'))
    .use(function(req, res) {
        router.dispatch(req, res, function(err) {
            if (err) {
                res.writeHead(200);
                res.end(err.message);
            }
        });
    });

http.createServer(app).listen(80);