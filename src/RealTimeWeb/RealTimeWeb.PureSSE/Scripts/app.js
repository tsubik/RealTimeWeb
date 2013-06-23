﻿/// <reference path="jquery-1.9.1.js" />

/*!
    ASP.NET PureSSE Stock Ticker Sample
*/

// Crockford's supplant method (poor man's templating)
if (!String.prototype.supplant) {
	String.prototype.supplant = function (o) {
		return this.replace(/{([^{}]*)}/g,
            function (a, b) {
            	var r = o[b];
            	return typeof r === 'string' || typeof r === 'number' ? r : a;
            }
        );
	};
}

// A simple background color flash effect that uses jQuery Color plugin
jQuery.fn.flash = function (color, duration) {
	var current = this.css('backgroundColor');
	this.animate({ backgroundColor: 'rgb(' + color + ')' }, duration / 2)
        .animate({ backgroundColor: current }, duration / 2);
}

$(function () {
	var up = '▲',
	down = '▼',
	$stockTable = $('#stockTable'),
	$stockTableBody = $stockTable.find('tbody'),
	rowTemplate = '<tr data-symbol="{Symbol}"><td>{Symbol}</td><td>{Price}</td><td>{DayOpen}</td><td>{DayHigh}</td><td>{DayLow}</td><td><span class="dir {DirectionClass}">{Direction}</span> {Change}</td><td>{PercentChange}</td></tr>',
	$stockTicker = $('#stockTicker'),
	$stockTickerUl = $stockTicker.find('ul'),
	liTemplate = '<li data-symbol="{Symbol}"><span class="symbol">{Symbol}</span> <span class="price">{Price}</span> <span class="change"><span class="dir {DirectionClass}">{Direction}</span> {Change} ({PercentChange})</span></li>';


	function formatStock(stock) {
		return $.extend(stock, {
			Price: stock.Price.toFixed(2),
			PercentChange: (stock.PercentChange * 100).toFixed(2) + '%',
			Direction: stock.Change === 0 ? '' : stock.Change >= 0 ? up : down,
			DirectionClass: stock.Change === 0 ? 'even' : stock.Change >= 0 ? 'up' : 'down'
		});
	}

	function scrollTicker() {
		var w = $stockTickerUl.width();
		$stockTickerUl.css({ marginLeft: w });
		$stockTickerUl.animate({ marginLeft: -w }, 15000, 'linear', scrollTicker);
	}

	function stopTicker() {
		$stockTickerUl.stop();
	}

	function init() {
		return $.getJSON('/api/stocks').done(function (stocks) {
			$stockTableBody.empty();
			$stockTickerUl.empty();
			$.each(stocks, function () {
				var stock = formatStock(this);
				$stockTableBody.append(rowTemplate.supplant(stock));
				$stockTickerUl.append(liTemplate.supplant(stock));
			});
		});
	}

	function getMarketState() {
		return $.get('market/state');
	}

	var initialization = $.Deferred();

	initialization
		.pipe(init)
		.pipe(getMarketState)
		.done(function (state) {
			if (state === 'Open') {
				ticker.marketOpened();
			} else {
				ticker.marketClosed();
			}
		});

	var ticker = {
		updateStockPrice: function (e) {
			var stock = JSON.parse(e.data);

			var displayStock = formatStock(stock),
                $row = $(rowTemplate.supplant(displayStock)),
                $li = $(liTemplate.supplant(displayStock)),
                bg = stock.LastChange < 0
                        ? '255,148,148' // red
                        : '154,240,117'; // green

			$stockTableBody.find('tr[data-symbol=' + stock.Symbol + ']')
                .replaceWith($row);
			$stockTickerUl.find('li[data-symbol=' + stock.Symbol + ']')
                .replaceWith($li);

			$row.flash(bg, 1000);
			$li.flash(bg, 1000);
		},

		marketOpened: function () {
			$("#open").prop("disabled", true);
			$("#close").prop("disabled", false);
			$("#reset").prop("disabled", true);
			scrollTicker();
		},

		marketClosed: function () {
			$("#open").prop("disabled", false);
			$("#close").prop("disabled", true);
			$("#reset").prop("disabled", false);
			stopTicker();
		},

		marketReset: function () {
			return init();
		},
	};

	if (!!window.EventSource) {
		var source = new EventSource('http://localhost:51270/api/sse');

		source.addEventListener('open', function (e) {
			console.log('Connection was opened');
		}, false);

		source.addEventListener('error', function (e) {
			if (e.readyState == EventSource.CLOSED) {
				console.log('Connection was closed');
			}
		}, false);
		source.addEventListener('marketOpened', ticker.marketOpened, false);
		source.addEventListener('marketClosed', ticker.marketClosed, false);
		source.addEventListener('marketReset', ticker.marketReset, false);
		source.addEventListener('updateStockPrice', ticker.updateStockPrice, false);
	}
	else {
		alert("SSE is not supported by your browser. Sorry.");
	}
	// Wire up the buttons
	$("#open").click(function () {
		$.post('market/open');
	});

	$("#close").click(function () {
		$.post('market/close');
	});

	$("#reset").click(function () {
		$.post('market/reset');
	});

	initialization.resolve();
});