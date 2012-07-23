var templater = require('../lib/templater.js');

module.exports = {
    index: function() {
        this.res.writeHead(200, {
            'Content-Type': 'text/html'
        });

        templater.render('layout.ejs', {}, function(output) {
            this.res.end(output);
        }.bind(this));
    }
};