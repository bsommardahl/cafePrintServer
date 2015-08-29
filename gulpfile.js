var gulp = require('gulp');
var config = require('./gulp.config')();
var shell = require('./util/shell');
var _ = require('lodash');
var clean = require('gulp-clean');
var runSequence = require('run-sequence');
var tap = require('gulp-tap');
var debug = require('gulp-debug');
var zip = require('gulp-zip');
var fs = require('fs');
var path = require('path');
var merge = require('merge-stream');
var rename = require('gulp-rename');
var flatten = require('gulp-flatten');

gulp.task('default', function(callback){
	runSequence('build', 'specs', 'package', 'deploy', callback);
});

gulp.task('build', function(callback) {
  runSequence(
  	'restore-nuget-packages', 'compile-apps', callback);
});

gulp.task('restore-nuget-packages', function (taskDone) {	
	return gulp.src('src/**/*.sln')
		.pipe(tap(function(file){
			file.folder = file.path.substring(0,file.path.lastIndexOf("\\")+1);
		}))
		.pipe(shell(['nuget.exe restore -verbosity detailed'], { cwd: '<%= file.folder %>'}));
});

gulp.task('init', ['copy-connection-string', 'copy-logging-config', 'restore-nuget-packages']);

gulp.task('copy-connection-string', function(){
	fs.stat('src/connectionStrings.config', function(doesNotExist, stat) {
		if(doesNotExist) {
			return gulp.src('connectionStrings.config.default')
	    		.pipe(rename("connectionStrings.config"))
	    		.pipe(gulp.dest('src'));
	    }else{
			return;
	    } 
	});
});

gulp.task('copy-logging-config', function(){
	fs.stat('src/logging.config', function(doesNotExist, stat) {
		if(doesNotExist) {
			return gulp.src('logging.config.default')
	    		.pipe(rename("logging.config"))
	    		.pipe(gulp.dest('src'));
	    }else{
			return;
	    } 
	});
});

gulp.task('compile-apps', ['clean-build', 'copy-connection-string', 'copy-logging-config'], function () {

	return gulp.src(['src/**/.deployable'])
		.pipe(tap(function(file){
			file.folder = file.path.substring(0,file.path.lastIndexOf("\\")+1);	
			var pathParts = file.folder.split("\\");
			file.folderName = pathParts[pathParts.length-2];							
			file.csprojPath = file.folder + '\\' + file.folderName+ '.csproj'
		}))
		.pipe(shell([
			config.util.msbuild 
			+ ' /p:Configuration=Release'
			+ ' /p:OutDir=\"' + config.buildPath + '\\<%= file.folderName %>\"' 
			+ ' \"<%= file.csprojPath %>\"'
			+ ' /p:WebProjectOutputDir=\"' + config.buildPath + '\\<%= file.folderName %>-publish\"' 
			]));	
});

gulp.task('compile-specs', ['clean-spec'], function () {

	return gulp.src('src/**/*.Specs.csproj')
		.pipe(tap(function(file){
			file.folder = file.path.substring(0,file.path.lastIndexOf("\\")+1);	
			var pathParts = file.folder.split("\\");
			file.folderName = pathParts[pathParts.length-2];										
		}))
		.pipe(shell([
			config.util.msbuild 
			+ ' /p:Configuration=Release'
			+ ' /p:OutDir=\"' + config.specsPath + '\\<%= file.folderName %>\"' 
			+' \"<%= file.path %>\"'
			]));	
});

gulp.task('package', function(callback){
	runSequence('clean-deploy', 'zip-apps', callback);
});

gulp.task('clean-deploy', function(){
	return gulp.src('deploy').pipe(clean());
});

function getFolders(dir) {
    return fs.readdirSync(dir)
      .filter(function(file) {
        return fs.statSync(path.join(dir, file)).isDirectory();
      });
}

gulp.task('zip-apps', function(){
	var folders = getFolders(config.buildPath);

	var tasks = folders.map(function(folder) {		
      return gulp.src([config.buildPath+'/'+folder+'/**/'])      	
        .pipe(zip(folder+'-'+config.appVersion+'.zip'))
        .pipe(gulp.dest('deploy'));
   	});

   	return tasks;	
});

gulp.task('deploy', function(){

});

gulp.task('specs', ['compile-specs'], function(){

	gulp.src(config.buildPath + '/specs/**/*.Specs.dll')
	    .pipe(tap(function(file){
	    	file.path = file.path.replace('/','\\');
    	}))
	    .pipe(shell(config.util.mspec + ' --html ' + config.reportsPath + ' "<%= file.path %>"'));

});

gulp.task('clean-build', function(){
	return gulp.src(config.buildPath).pipe(clean());
});

gulp.task('clean-spec', function(){
	return gulp.src(config.specsPath).pipe(clean());
});

