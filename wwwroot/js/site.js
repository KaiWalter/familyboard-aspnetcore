var calendarUpdateCounter = 1;
var imageUpdateCounter = 1;

window.onerror = function(message, source, lineno, colno, error) {
  console.error("JS ERROR:", message, "at", source + ":" + lineno);
};

function startTime() {
  var today = new Date();
  document.querySelector("fb-clock").time = {
    hh: today.getHours().toString().padStart(2, "0"),
    mm: today.getMinutes().toString().padStart(2, "0"),
    ss: today.getSeconds().toString().padStart(2, "0"),
  };
  var t = setTimeout(startTime, 500);
}

function startMainLoop() {
  MainLoop();
}

function MainLoop() {
  calendarUpdateCounter--;

  if (calendarUpdateCounter <= 0) {
    console.log("update calendar");
    updateCalendar();
    calendarUpdateCounter = 300;
  }

  imageUpdateCounter--;

  if (imageUpdateCounter <= 0) {
    console.log("update image");
    updateImage();
    imageUpdateCounter = 90;
  }

  putStatus(
    `next image update ${imageUpdateCounter}s - next calendar update ${calendarUpdateCounter}s`,
  );

  var t = setTimeout(MainLoop, 1000);
}

function putMessage(message) {
  document.querySelector("fb-message").content = message;
}

function putStatus(status) {
  document.querySelector("fb-status").content = status;
}

// --------------------------------------------------------------------------------
// update calendar

let monthNames;
let weekDayNames;

function initCalendar() {
  if (!monthNames || !weekDayNames) {
    fetch("/api/calendar/dateformatinfo")
      .then((response) => response.json())
      .then((data) => {
        if (data) {
          monthNames = data.monthNames;
          weekDayNames = data.weekDayNames;
        }
      });
  }
}

function updateCalendar() {
  initCalendar();

  fetch("/api/calendar")
    .then((response) => response.json())
    .then((data) => {
      if (data) {
        renderCalendar(data);
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
    tdt.setMonth(0, 1 + ((4 - tdt.getDay() + 7) % 7));
  }
  return 1 + Math.ceil((firstThursday - tdt) / 604800000);
}

function ISO8601_date(dt) {
  return (
    dt.getFullYear().toString() +
    "-" +
    (dt.getMonth() + 1).toString().padStart(2, "0") +
    "-" +
    dt.getDate().toString().padStart(2, "0")
  );
}

function addDays(date, days) {
  var result = new Date(date);
  result.setDate(result.getDate() + days);
  return result;
}

function currentDate() {
  let current = new Date();
  current.setHours(0, 0, 0, 0);
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

  // fill calendar
  let iDate = currentDate();
  let data = [];

  for (w = 0; w < 3; w++) {
    let iDate = addDays(firstDate, w * 7);
    let weekData = {
      weekNo: ISO8601_week_no(iDate).toString().padStart(2, "0"),
      days: [],
    };

    for (wd = 0; wd < 7; wd++) {
      let i = w * 7 + wd;
      let iDate = addDays(firstDate, i);
      let isToday = ISO8601_date(iDate) === ISO8601_date(current);

      let monthTitle = "";
      if (i === 0 || iDate.getDate() == 1) {
        monthTitle = monthNames[iDate.getMonth()];
      }

      let dayData = {
        monthOfYear: monthTitle,
        dayOfWeek: weekDayNames[wd],
        dayOfMonth: iDate.getDate().toString().padStart(2, "0"),
        isToday: isToday,
        events: [],
      };
      let iDateFormatted = ISO8601_date(iDate);

      // render all day events always on top
      events.forEach((entry) => {
        if (iDateFormatted === entry.date && entry.allDayEvent) {
          let eventData = {
            description: entry.description,
            isPrimary: entry.isPrimary,
            isSecondary: entry.isSecondary,
            allDayEvent: entry.allDayEvent,
            publicHoliday: entry.publicHoliday,
            schoolHoliday: entry.schoolHoliday,
          };
          dayData.events.push(eventData);
        }
      });

      // render timed events below
      events.forEach((entry) => {
        if (iDateFormatted === entry.date && !entry.allDayEvent) {
          let eventData = {
            description: entry.description,
            time: entry.time,
            isPrimary: entry.isPrimary,
            isSecondary: entry.isSecondary,
            allDayEvent: entry.allDayEvent,
            publicHoliday: entry.publicHoliday,
            schoolHoliday: entry.schoolHoliday,
          };
          dayData.events.push(eventData);
        }
      });

      weekData.days.push(dayData);
    }

    data.push(weekData);
  }

  document.querySelector("fb-calendar").data = data;

  putMessage("");
}

// --------------------------------------------------------------------------------
// update image

function updateImage() {
  fetch("/api/image")
    .then((response) => response.json())
    .then((data) => renderImage(data));
}

function renderImage(imageObj) {
  putMessage("updating image");
  console.log("Load image:", imageObj.src);
  var img = new Image();
  img.onload = function () {
    // imgContainer = document.getElementsByClassName('imageContainer')[0];
    // imgContainer.style.background = "#000 url(" + imageObj.src + ") center center";
    // imgContainer.style.backgroundSize = "cover";
    // imgContainer.style.backgroundRepeat = "no-repeat";

    imgContainer = document.getElementsByClassName("imageContainer")[0];

    const imgExisting = imgContainer.querySelector("img");
    if (imgExisting) {
      imgContainer.removeChild(imgExisting);
    }

    imgContainer.appendChild(img);
    img.style.width = "100%";
    img.style.height = "100%";
    img.style.objectFit = "cover";

    imgCreated = document.getElementsByClassName("imageCreated")[0];
    var imageCreatedLabel = "";
    if (imageObj.month && imageObj.year) {
      imageCreatedLabel = monthNames[imageObj.month - 1] + " " + imageObj.year;
    }
    imgCreated.innerHTML = imageCreatedLabel;
  };
  img.src = imageObj.src;

  console.log("Loaded image:", imageObj.src);
  putMessage("");
}
