/// <reference path="../scripts/jquery-1.9.1.js" />
/// <reference path="../scripts/jquery.signalR-1.1.0.js" />

/*!
    ASP.NET SignalR Stock Ticker Sample
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

    var ticker = $.connection.erpTicker, // the generated client-side hub proxy
        $erpTable = $('#erpTable'),
        $erpTableBody = $erpTable.find('tbody'),
        rowTemplate = '<tr data-type="{Type}"><td>{Type}</td><td>{Total}</td><td>{NumberOf}</td><td>{Last}</td><td>{Largest}</td><td>{Smallest}</td><td>{Average}</td></tr>',
        $erpTicker = $('#erpTicker'),
        $erpTickerUl = $erpTicker.find('ul'),
        liTemplate = '<li data-type="{Type}"><span class="type">{Type}</span> <span class="total">{Total}</span></li>';

    function formatKPI(kpi) {
        return $.extend(kpi, {
            Total: kpi.Total.toFixed(2),
            Last: kpi.Last.toFixed(2),
            Largest: kpi.Largest.toFixed(2),
            Smallest: kpi.Smallest.toFixed(2),
            Average: kpi.Average.toFixed(2)
        });
    }

    function scrollTicker() {
        var w = $erpTickerUl.width();
        $erpTickerUl.css({ marginLeft: w });
        $erpTickerUl.animate({ marginLeft: -w }, 35000, 'linear', scrollTicker);
    }
    //NOTE:  scrollTicker above could be another function that toggles between different channels and at the end calls scrollTicker - SWEET
    // ALERT channel should interrupt when first broadcasted.  After that we should store the last 5 alerts and handle as a separate channel

    function stopTicker() {
        $erpTickerUl.stop();
    }

    function init() {
        return ticker.server.getAllKPIs("Sales").done(function (kpis) {
            $erpTableBody.empty();
            $erpTickerUl.empty();
            $erpTicker.hi
            $.each(kpis, function () {
                var kpi = formatKPI(this);
                $erpTableBody.append(rowTemplate.supplant(kpi));
                $erpTickerUl.append(liTemplate.supplant(kpi));
            });
        });
    }
    
    function wireButtons() {
        $("#askSage").click(function () {
            $('#responses').append('<div class="border"><i>Question: ' + $('#question').val() + '</i></div>')
            ticker.server.sendRequest($.connection.hub.id, $('#question').val())
        });
        $("#clear").click(function() {
            $('#responses').empty()
            $('#question').val('')
        });
    }


    // Add client-side hub methods that the server will call
    $.extend(ticker.client, {
        updateSalesKPI: function (kpi) {
            var displayKPI = formatKPI(kpi),
                $row = $(rowTemplate.supplant(displayKPI)),
                $li = $(liTemplate.supplant(displayKPI));


            $erpTableBody.find('tr[data-type=' + kpi.Type + ']')
                .replaceWith($row);
            $erpTickerUl.find('li[data-type=' + kpi.Type + ']')
                .replaceWith($li);


        },
        addSalesKPI: function (kpi) {
            var displayKPI = formatKPI(kpi);
            $erpTableBody.append(rowTemplate.supplant(displayKPI));
            $erpTickerUl.append(liTemplate.supplant(displayKPI));
        },
        addResponse: function (response) {
            $('#responses').append('<div class="border"><i>Resp: ' + response + '</i></div>')
        },
        addTickerItem: function (channel, tickerText) {
            $erpTickerUl.append(tickerText);
        }

        /*                bg = stock.LastChange < 0
                                ? '255,148,148' // red
                                : '154,240,117'; // green
                          $row.flash(bg, 1000);
                          $li.flash(bg, 1000);
         */

/*
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
        }
 */
    });

    // Start the connection
    $.connection.hub.start()
        .pipe(init)
        .pipe(scrollTicker)
        .pipe(wireButtons);
 
});