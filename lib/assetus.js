var stylus = require('stylus');

module.exports = function assetus(options) {
    options = options || {};

    if (!options.root) {
        throw new Error("Please specify the root directory!");
    }

    var root = options.root;
    var destination = options.destination;
    var stylesDirectory = "styles/";

    return function assetus(req, res, next) {
        var a = stylus.middleware({
            src: root + stylesDirectory,
            dest: destination + stylesDirectory,
            compress: false
        });

        a();

        next();
    };
};