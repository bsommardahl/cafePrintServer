var _ = require('underscore');
var moment = require('moment');
var express = require('express');
var bodyParser = require('body-parser')
var app = express()

app.use(bodyParser.urlencoded({ extended: false }))
app.use(bodyParser.json())

var getTestOrder = function(){
	return {
  	"TaxPaid": 110,
  	"LocationId": "location",
  	"Paid": "1994-11-05T08:15:30-05:00",
  	"Created": "1994-11-05T08:15:30-05:00",
  	"AmountPaid": 432,
  	"CustomerName": "Byron",
  	"AllDelivered": true,
  	"_id": "sampleId",
  	"Items": [
  	    {
  			"Price": 50,
  			"Delivered": true,
  			"Name": "coffee"
  		},
  		{
  			"Price": 50,
  			"Delivered": true,
  			"Name": "coffee"
  		},
  		{
  			"Price": 70,
  			"Delivered": true,
  			"Name": "cake"
  		}]
  };
};

app.get('/printers', function(req, res){
	res.send({printers: printer.getPrinters(), formats: printer.getSupportedPrintFormats(), jobs: printer.getSupportedJobCommands()});
});

app.post('/preview', function (req, res) {
  var order = getOrderFromRequest(req.body);
  renderOrder(order, function(html){
  	res.send(html);	
  });  
});

app.get('/preview', function (req, res) {
  var order = getOrderFromRequest(getTestOrder());
  renderOrder(order, function(html){
  	res.send(html);	
  });  
});

app.post('/print', function (req, res) {
  var order = getOrderFromRequest(req.body);
  renderOrder(order, function(html){
  	printText(html);
  	res.send(html);	
  });  
});

var server = app.listen(3000, function () {
	var serverIp = getServerIp();
	var host = server.address().address;
	var port = server.address().port;
	var startupMessage = 'Cafe Receipt Printserver ready at http://' + host + ':' + port + ' on ' + moment().format();
	console.log(startupMessage)
	printText(serverIp + " ready.");
});

var net = require('net');

var getServerIp = function(){
	var os = require('os');
	var interfaces = os.networkInterfaces();
	var ip = "127.0.0.1";
	for(name in interfaces) {
	    var interface = interfaces[name];
	     interface.forEach(function(entry) {
	    if(entry.family === 'IPv4' && entry.address.indexOf("169.254")==-1 && entry.address.indexOf("127.0")==-1) {	        
	       ip = entry.address;
	    }
	  });
	}
	return ip;
};

var doT = require("dot");

var renderOrder = function(order, callback){
	fs = require('fs')
	fs.readFile(__dirname + '/orderTemplate.html', 'utf8', function (err,data) {
		if (err) {
			return console.log(err);
		}
		var engine = doT.template(data);
		var resultText = engine(order);
		callback(resultText);
	});	
};

var getOrderFromRequest = function(data){

	var itemGroups = _.groupBy(data.Items,  function(i){
		return i.Name;
	});
	
	var order = {
  	TaxPaid: data.TaxPaid,
  	LocationId: data.LocationId,
  	Paid: data.Paid,
  	Created: data.Created,
  	AmountPaid: data.AmountPaid,
  	CustomerName: data.CustomerName,
  	AllDelivered: data.AllDelivered,
  	_id: data._id,
  	Items: _.map(Object.keys(itemGroups), function(itemName){
		var group = itemGroups[itemName];
		
		var sum = 0;
		if(group.length==1){
			sum = group[0].Price;
		}
		else{
			sum = _.reduce(group, function(prev, next){
				return prev.Price + next.Price;
			});
		}

  		return {
  			_id: group[0]._id,
  			Quantity: group.length,
  			Price: sum,
  			Name: itemName
  		};
  	}),

  	SubTotal: data.AmountPaid - data.TaxPaid  	
  };
  return order;
};
  
var printText = function(html){
	var pdf = require('html-pdf');
	var printer = require("printer");
	var filename = __dirname + "temp.pdf";
	var options = {
		"width": "2.5in",
		"height": "4in",
		"border": "0", 
		"type": "pdf",
		"quality": "75",
	};
	
	pdf.create(html, options).toFile(filename, function(err, file) {
		if (err) return console.log(err);
		console.log("Text converted to pdf. Printing file...");

		printer.printFile({
			filename: filename,
			success: function(jobId){
				console.log("Job " + jobId + " printed.");
			},
			error: function(err){
				console.log(err);
			}
		});
	});
};