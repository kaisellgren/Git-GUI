var ejs = require('ejs');
    fs = require('fs');

var templatePath = null;
var outputCache = {};

module.exports = {
    setTemplatePath: function(path) {
        templatePath = path;
    },

    render: function(file, data, callback) {
        if (outputCache[file] && process.argv[2] !== 'debug') {
            callback(outputCache[file]);
        }

        fs.readFile(templatePath + file, function(err, data) {
            if (err) throw err;

            var output = ejs.render(data.toString('utf8'));
            outputCache[file] = output;

            callback(output);
        });
    },

    invalidateCaches: function() {
        outputCache = {};
    }
};