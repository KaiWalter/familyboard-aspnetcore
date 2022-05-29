var calendarUpdateCounter = 1;
var imageUpdateCounter = 1;

function startTime() {
    var today = new Date();
    var h = today.getHours().toString().padStart(2, "0");
    var m = today.getMinutes().toString().padStart(2, "0");
    var s = today.getSeconds().toString().padStart(2, "0");
    $(".clock").html(h + ":" + m + "<span class='sec'>" + s + "</span>");
    var t = setTimeout(startTime, 500);
}

function startMainLoop() {
    MainLoop();
}

function MainLoop() {

    calendarUpdateCounter--;

    if (calendarUpdateCounter <= 0) {
        console.log('update calendar');
        updateCalendar();
        calendarUpdateCounter = 300;
    }

    imageUpdateCounter--;

    if (imageUpdateCounter <= 0) {
        console.log('update image');
        updateImage();
        imageUpdateCounter = 90;
    }

    putStatus(`next image update ${imageUpdateCounter}s - next calendar update ${calendarUpdateCounter}s`);

    var t = setTimeout(MainLoop, 1000);
}

function putMessage(message) {
    $("#message").html(message);
}

function putStatus(status) {
    $("#status").html(status);
}

// --------------------------------------------------------------------------------
// update calendar

let monthNames;
let weekDayNames;

function initCalendar() {
    if (!monthNames || !weekDayNames) {
        $.ajax({
            type: "get",
            url: "/api/calendar/dateformatinfo",
            context: document.body,
            success: function(data) {
                if (data) {
                    monthNames = data.monthNames;
                    weekDayNames = data.weekDayNames;
                }
            }
        });
    }
}

function updateCalendar() {
    initCalendar();

    $.ajax({
        type: "get",
        url: "/api/calendar",
        context: document.body,
        success: function(data) {
            if (data) {
                renderCalendar(data);
            }
        }
    });
}

function ISO8601_week_no(dt) {
    var tdt = new Date(dt.valueOf());
    var dayn = (dt.getDay() + 6) % 7;
    tdt.setDate(tdt.getDate() - dayn + 3);
    var firstThursday = tdt.valueOf();
    tdt.setMonth(0, 1);
    if (tdt.getDay() !== 4) {
        tdt.setMonth(0, 1 + ((4 - tdt.getDay()) + 7) % 7);
    }
    return 1 + Math.ceil((firstThursday - tdt) / 604800000);
}

function ISO8601_date(dt) {
    return dt.getFullYear().toString() + "-" +
        (dt.getMonth() + 1).toString().padStart(2, "0") + "-" +
        dt.getDate().toString().padStart(2, "0");
}

function addDays(date, days) {
    var result = new Date(date);
    result.setDate(result.getDate() + days);
    return result;
}

function currentDate() {
    let current = new Date();
    current.setHours(0, 0, 0, 0)
    return current;
}

function renderCalendar(events) {
    putMessage("calendar refresh");

    // find first day of week
    let current = currentDate();

    let wd = current.getDay();
    let firstDate = currentDate();
    // flip week on Sunday
    if (wd === 0) {
        firstDate = addDays(firstDate, -1);
        wd = firstDate.getDay();
    }
    // find Monday
    while (wd > 1) {
        firstDate = addDays(firstDate, -1);
        wd = firstDate.getDay();
    }

    // https://dev.to/thepassle/web-components-from-zero-to-hero-4n4m#-a-components-lifecycle

    // fill calendar
    let iDate = currentDate();
    let data = [];

    for (w = 0; w < 3; w++) {
        let iDate = addDays(firstDate, w * 7);
        let weekData = { weekNo: ISO8601_week_no(iDate).toString().padStart(2, "0"), days: [] };

        for (wd = 0; wd < 7; wd++) {
            let i = (w * 7) + wd;
            let iDate = addDays(firstDate, i);
            let isToday = ISO8601_date(iDate) === ISO8601_date(current);

            let monthTitle = "";
            if (i === 0 || iDate.getDate() == 1) {
                monthTitle = monthNames[iDate.getMonth()];
            }

            let dayData = { monthOfYear: monthTitle, dayOfWeek: weekDayNames[wd], dayOfMonth: iDate.getDate().toString().padStart(2, "0"), events: [] };
            let iDateFormatted = ISO8601_date(iDate);

            // render all day events always on top
            events.forEach((entry) => {
                if (iDateFormatted === entry.date && entry.allDayEvent) {
                    let eventData = { description: entry.description, isPrimary: entry.isPrimary, isSecondary: entry.isSecondary, allDayEvent: entry.allDayEvent, publicHoliday: entry.publicHoliday, schoolHoliday: entry.schoolHoliday };
                    dayData.events.push(eventData);
                }
            });

            // render timed events below
            events.forEach((entry) => {
                if (iDateFormatted === entry.date && !entry.allDayEvent) {
                    let eventData = { description: entry.description, time: entry.time, isPrimary: entry.isPrimary, isSecondary: entry.isSecondary, allDayEvent: entry.allDayEvent, publicHoliday: entry.publicHoliday, schoolHoliday: entry.schoolHoliday };
                    dayData.events.push(eventData);
                }
            });

            weekData.days.push(dayData);
        }

        data.push(weekData);
    }

    console.log(JSON.stringify(data));
    document.querySelector('fb-calendar').data = data;

    putMessage("");
}

// --------------------------------------------------------------------------------
// update image

function updateImage() {
    $.ajax({
        type: "get",
        url: "/api/image",
        context: document.body,
        success: function(data) {
            renderImage(data);
        }
    });

}

function renderImage(imageObj) {
    putMessage("updating image");

    $("<img/>").attr("src", imageObj.src).on("load", function() {
        $(this).remove(), $(".imageContainer").css({
            background: "#000 url(" + imageObj.src + ") center center",
            backgroundSize: "cover",
            backgroundRepeat: "no-repeat"
        });

        var imageCreatedLabel = "";
        if (imageObj.month && imageObj.year) {
            imageCreatedLabel = monthNames[imageObj.month - 1] + " " + imageObj.year;
        }
        $(".imageCreated").html(imageCreatedLabel);
    });

    putMessage("");
}