var _ = require('underscore');
var moment = require('moment');
var express = require('express');
var bodyParser = require('body-parser')
var app = express()

app.use(bodyParser.urlencoded({ extended: false }))
app.use(bodyParser.json())

app.get('/printers', function(req, res){
	var util = require('util');
	res.send("installed printers: "
		+ JSON.stringify(printer.getPrinters()));
});

app.post('/preview', function (req, res) {
  var order = getOrderFromRequest(req.body);
  var receiptText = formatReceipt(order);
  res.send(receiptText);
});

app.post('/print', function (req, res) {
  var order = getOrderFromRequest(req.body);
  var receiptText = formatReceipt(order);  
  res.send("Printing receipt for order " + order._id);
  printText(receiptText);
});

var itemTemplate =  "{0} | {1} | {2}\n";

var footer = 
	"Gracias por su compra." + "\n" + 
	"\n" +
    "La factura es benefició de todos: Exijala!" + "\n" +
    "No se aceptan cambios ni devoluciones." + "\n" +
	"\n"+ 
    "www.CafeElGringo.com";

var header = "Cafe El Gringo" + "\n" +
	"RTN: 08221886000084" + "\n"+
	"Barrio el Centro" + "\n"+
	"Santa Ana, FM Honduras" + "\n"+
	"Tel +504 9754-5002" + "\n"+
	"\n"+
	"FACTURA\n"+
	"\n";

var itemsHeader = 
    "----------------------------------------" + "\n"+
    "Cant.  | Descripcion           |   Valor" + "\n"+
    "----------------------------------------" + "\n";

var totals = 
    "----------------------------------------" + "\n"+
	"                   Sub Total:    L {0}" + "\n"+
	"                   Descuento:    L {1}" + "\n"+
	"                   ISV:          L {2}" + "\n"+
	"                   Total:        L {3}" + "\n"+
	"\r"+
	"                   Effectivo:    L {4}" + "\n"+
	"                   Cambio:       L {5}" + "\n" +
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
	var order = {
  	TaxPaid: data.TaxPaid,
  	LocationId: data.LocationId,
  	Paid: data.Paid,
  	Created: data.Created,
  	AmountPaid: data.AmountPaid,
  	CustomerName: data.CustomerName,
  	AllDelivered: data.AllDelivered,
  	_id: data._id,
  	Items: _.map(data.Items, function(i){
  		return {
  			_id: i._id,
  			Tag: i.Tag,
  			Price: i.Price,
  			TaxRate: i.TaxRate,
  			Delivered: i.Delivered,
  			Name: i.Name
  		};
  	})
  };
  return order;
};

var server = app.listen(3000, function () {
  var host = server.address().address;
  var port = server.address().port;
  console.log('Cafe Receipt Printserver ready at http://%s:%s', host, port);
});

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

var printer = require("printer");

var printText = function(text){
	printer.printDirect({
		data: text,
		type: 'TEXT',
		success: function(jobId){
			console.log("Job " + jobId + " printed.");
		},
		error: function(err){
			console.log(err);
		}
	})
};