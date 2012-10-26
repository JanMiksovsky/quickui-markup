/*
 * QuickUI markup compiler support for Grunt.
 */
module.exports = function( grunt ) {

    var child_process = require( "child_process" );
    var os = require( "os" );
    
    /*
     * Invoke the QuickUI markup compiler.
     */
    grunt.registerMultiTask( "qb", "Compile QuickUI Markup", function() {

        var done = this.async();
        var projectPath = this.data.path;
        var qbExecutablePath = getQbExecutablePath();

        var command = qbExecutablePath + " " + projectPath;
        if ( os.platform() === "darwin" ) {
            // On Mac, invoke qb.exe using Mono.
            command = "mono " + command;
        }
        console.log ( "command: " + command );

        child_process.exec( command, null, function( error, stdout, stderr ){
            if ( error ) {
                grunt.log.writeln( "qb failed: " + error );
            } else {
                grunt.verbose.writeln( "Compiled " + projectPath ); 
            }
            done( !error );
        });

    });

};

/*
 * Return the path of qb.exe.
 * This relies on knowledge of the directory structure of the quickui-markup
 * project. In this case, we walk up from the tasks folder to get to the
 * output directory for the .Net build tool.
 */
function getQbExecutablePath() {
    var path = require( "path" );
    var modulePath = path.dirname( module.filename );
    var qbPath = path.resolve( modulePath + "/../qb/bin/Release/qb.exe" );
    return qbPath;
}
