function showModalDialog(urlSource, targetDivId, modalId) {
    $.ajax({
        url: urlSource,
        type: 'GET',
        success: function (data) {
            $(targetDivId).html(data);
            $.validator.unobtrusive.parse(targetDivId);
            $(modalId).modal('show');
        }
    });
}

function scrollDown(id) {
    var elem = $(id);
    elem.animate({
        scrollTop: elem.prop("scrollHeight")
    }, 500);
}

$(function () {
    $.ajax({
        url: '/Mobile/GetEventCounter',
        type: 'GET',
        success: function (val) {
            setEventCounter(val);
        }
    });
});

function setEventCounter(num) {
    if (num > 0) {
        $('#event-icon').removeClass('bi-envelope-open').addClass('bi-envelope-exclamation-fill').addClass('text-primary');
        $('#event-counter').text(num).removeClass('invisible');
    }
    else {
        $('#event-icon').addClass('bi-envelope-open').removeClass('bi-envelope-exclamation-fill').removeClass('text-primary');
        $('#event-counter').text('0').addClass('invisible');
    }
}

/// SignalR area
var connection = new signalR.HubConnectionBuilder()
    .withUrl('/messageHub')
    .build();

connection.on("NotifyWeb", function () {
    Notification.requestPermission().then((result) => {
        if (result === "granted") {
            const notification = new Notification("رسالة حالة جديدة");
        }
    });
    var counter = 1 + parseInt($('#event-counter').text());
    setEventCounter(counter);
});

connection.on("NotifyChat", function (chatMsg) {
    Notification.requestPermission().then((result) => {
        if (result === "granted") {
            const notification = new Notification("محادثة ", {
                body: chatMsg.sender + ': ' + chatMsg.message,
                dir: "rtl",
            });
        }
    });
    if ($('#send-text')) {
        var lst = $('#messagelist');
        $.ajax({
            url: '/Admin/LoadChatMessage/' + chatMsg.id,
            type: 'GET',
            success: function (data) {
                lst.append(data);
            }
        });
        scrollDown('#chatspace');
    }
});

async function startSignalR() {
    try {
        await connection.start();
    } catch (err) {
        console.log(err);
        setTimeout(() => startSignalR(), 5000);
    }
};

connection.onclose(async () => {
    await startSignalR();
});

startSignalR();
///End of SignalR area

function sendChatMessage(text) {
    connection.invoke('Chat', text).catch(function (err) {
        console.error(err.toString());
        alert(err.toString());
    });
}




