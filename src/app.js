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

app.post('/test', function(req, res){
	var order = getTestOrder();
	renderOrder(order, function(html){
	  	res.send("Printing test receipt...");
	  	printText(html);
	  });  
});

app.get('/printers', function(req, res){
	res.send({printers: printer.getPrinters(), formats: printer.getSupportedPrintFormats(), jobs: printer.getSupportedJobCommands()});
});

app.get('/preview', function (req, res) {
	var order = getOrderFromRequest(getTestOrder());
	renderOrder(order, function(html){
	  	res.send(html);	
	  });  
});

app.post('/preview', function (req, res) {
  var order = getOrderFromRequest(req.body);
  renderOrder(order, function(html){
  	res.send(html);	
  });  
});

var doT = require("dot");

var renderOrder = function(order, callback){
	fs = require('fs')
	fs.readFile('orderTemplate.html', 'utf8', function (err,data) {
		if (err) {
			return console.log(err);
		}
		var engine = doT.template(data);
		var resultText = engine(order);
		callback(resultText);
	});	
};

app.post('/print', function (req, res) {
	var order = getOrderFromRequest(req.body);
	renderOrder(order, function(html){
		printText(html);  		
		res.send("Printing receipt for order " + order._id);			
  	});    	
});

var server = app.listen(3000, function () {
	var host = server.address().address;
	var port = server.address().port;
	var startupMessage = 'Cafe Receipt Printserver ready at http://' + host + ':' + port + ' on ' + moment().format();
	console.log(startupMessage)
	printText(startupMessage);
});

var itemTemplate =  "{0} | {1} | {2}\n";

var footer = 
	"Gracias por su compra." + "\n" + 
	"\n" +
    "La factura es benefició de todos: Exijala!" + "\n" +
    "No se aceptan cambios ni devoluciones." + "\n" +
	"\n"+ 
    "www.CafeElGringo.com";

var itemsHeader = 
    "----------------------------------------" + "\n"+
    "Cant.  | Descripcion           |   Valor" + "\n"+
    "----------------------------------------" + "\n";

var totals = 
    "----------------------------------------" + "\n"+
	"                Importe gravado: L {0}" + "\n"+
	"                 Importe exento: L {1}" + "\n"+
	"                      Descuento: L {1}" + "\n"+
	"                            ISV: L {2}" + "\n"+
	"                  Total a pagar: L {3}" + "\n"+
	"\r"+
	"                      Effectivo: L {4}" + "\n"+
	"                         Cambio: L {5}" + "\n" +
    "----------------------------------------" + "\n" +
    "\n";

var receiptInfo = 
    "Factura {0}" + "\n"+
    "Condicion CONTADO" + "\n"+
	"Fecha/Hora {1}" + "\n"+
	"Cajero Pamela M - Caja #1" + "\n"	
    
var formatReceipt = function(order){	
	var itemsRendered = getItemsRendered(order.Items).join("");
	var receiptInfoRendered = getReceiptInfoRendered(order);
	var totalsRendered = getTotalsRendered(order);
	var receiptText = header + itemsHeader + itemsRendered + totalsRendered + receiptInfoRendered + footer;
	return receiptText;
};

var getTotalsRendered = function(order){
	var spacing = 6;
	return totals.format(
		rightAlign(order.AmountPaid - order.TaxPaid,spacing), 
		rightAlign(0,spacing), 
		rightAlign(order.TaxPaid,spacing),
		rightAlign(order.AmountPaid,spacing), 
		rightAlign(order.AmountPaid,spacing), 
		rightAlign(0,spacing));
};

var getReceiptInfoRendered = function(order){
	var paid = moment(order.Paid);
	return receiptInfo
		.format(order._id, 
			paid.format('MMMM Do YYYY, h:mm:ss a'));
};

var getItemsRendered = function(items){

	var itemGroups = _.groupBy(items,  function(i){
		return i.Name;
	});

	var itemsRendered = _.map(Object.keys(itemGroups), function(itemName){
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
		return itemTemplate.format(
			leftAlign(group.length, 7), 
			leftAlign(itemName, 22), 
			rightAlign(sum, 8));
	});

	return itemsRendered;
};

var rightAlign = function(str, len){
	var stringToAlign = str.toString();
	if(stringToAlign.length >= len) return stringToAlign;
	var spaceAmount = len - stringToAlign.length;
	var spaces = new Array(spaceAmount).join(" ");
	return spaces + stringToAlign;
};

var leftAlign = function(str, len){
	var stringToAlign = str.toString();
	if(stringToAlign.length >= len) return stringToAlign;
	var spaceAmount = len - stringToAlign.length;
	var spaces = new Array(spaceAmount).join(" ");
	return stringToAlign + spaces;
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
  	})
  };
  return order;
};

if (!String.prototype.format) {
  String.prototype.format = function() {
    var args = arguments;
    return this.replace(/{(\d+)}/g, function(match, number) { 
      return typeof args[number] != 'undefined'
        ? args[number]
        : match
      ;
    });
  };
}


var printText = function(html){
	var pdf = require('html-pdf');
	var printer = require("printer");
	var filename = "temp.pdf";
	var options = {
		"width": "2in",
		"height": "4in",
		"border": "0", 
		"type": "pdf",             // allowed file types: png, jpeg, pdf 
		"quality": "75",           // only used for types png & jpeg 
		//"border": {
		    //"top": ".25in",            // default is 0, units: mm, cm, in, px
		    //"right": ".25in",
		    //"bottom": ".25in",
		    //"left": ".25in"		    
		  //},
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