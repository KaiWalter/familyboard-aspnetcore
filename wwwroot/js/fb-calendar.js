class Calendar extends HTMLElement {
    constructor() {
        super();
        this._shadowRoot = this.attachShadow({ 'mode': 'open' });
        this._content = '';
    }

    get style() {
        return `
        <style>
            :host {
            display: block;
            }

            .calendar {
                display: grid;
                grid-template-columns: 1fr repeat(7, 3fr);
                grid-template-rows: repeat(3, 1fr);
                height: 100%;
            }

            .day {
                font-size: 1em;
                margin-left: 3px;
                margin-right: 3px;
            }
            
            .week_title {
                line-height: 0.9em;
                margin-left: 5px;
                margin-top: 1px;
                text-align: center;
                font-size: 2em;
                padding: 0 0 0 0;
            }
            
            .week_title .weekofyear {
                font-size: large;
            }
            
            .dayHeader {
                text-align: center;
                font-size: 2em;
            }
            
            .dayContent {
                text-align: left;
                font-size: 2em;
            }
            
            .day_title {
                line-height: 0.8em;
                margin-top: 1px;
                margin-bottom: 5px;
                padding: 0 0 0 0;
                border-bottom: 1px solid lightgray;
            }
            
            .day_title .dayofmonth {
                font-size: x-large;
            }
            
            .day_title .dayofweek {
                font-size: x-large;
            }
            
            .day_title .monthofyear {
                font-size: large;
            }
            
            .today {
                font-size: 1em;
                background-color: dimgray;
                border-radius: 3px;
            }
            
            .single_event {
                font-size: medium;
                line-height: 1em;
            }
            
            .primary_calendar {
                border-left: 2px solid white;
            }
            
            .secondary_calendar {
                border-left: 2px solid blue;
            }
            
            .all_day {
                background-color: lightgray;
                color: black;
                font-size: medium;
                line-height: 2em;
                padding-left: 2px;
                padding-right: 2px;
                margin-bottom: 2px;
                border-radius: 3px;
            }
            
            .public_holiday_day {
                opacity: 1;
                color: lightcyan;
                border: 1px solid white;
                font-size: medium;
                font-weight: bold;
                text-align: center;
                line-height: 2em;
                margin-bottom: 2px;
                border-radius: 3px;
            }
            
            .school_holiday_day {
                opacity: 1;
                color: lightcyan;
                border: 1px solid white;
                font-size: medium;
                text-align: center;
                line-height: 2em;
                padding-left: 2px;
                padding-right: 2px;
                margin-bottom: 2px;
                border-radius: 3px;
            }            
        </style>
        `;
    }

    get template() {
        return `
        ${this.style}
        <div class='calendar'>${this._content}</div>
        `;
    }

    weekHeaderTemplate(weekNo) {
        return "<div class='week_title'><br/><span class='weekofyear'>" + weekNo + "</span></div>";
    }

    dayTemplate(monthTitle, dayOfWeek, dayOfMonth, dayContent) {
        return "<div class='day'>" +
            "<div class='dayHeader'><div class='day_title'><span class='monthofyear'>" + monthTitle + "</span><br/>" +
            "<span class='dayofweek'>" + dayOfWeek + "</span>&nbsp;" +
            "<span class='dayofmonth'>" + dayOfMonth + "</span></div></div>" +
            "<div class='dayContent'>" + dayContent + "</div>" +
            "</div>";
    }

    _renderCalendar() {
        this._content = '';

        this._data.forEach(week => {
            this._content += this.weekHeaderTemplate(week.weekNo);
            week.days.forEach(day => {
                let dayContent = '';
                day.events.forEach(event => {
                    if (event.allDayEvent) {
                        if (event.publicHoliday) {
                            dayContent += "<div class='public_holiday_day'>" + event.description + "</div>";
                        } else if (event.schoolHoliday) {
                            dayContent += "<div class='school_holiday_day'>" + event.description + "</div>";
                        } else {
                            let addClass = (event.isPrimary ? " primary_calendar" : "") + (event.isSecondary ? " secondary_calendar" : "");
                            dayContent += "<div class='all_day" + addClass + "'>" + event.description + "</div>";
                        }
                    } else {
                        let addClass = (event.isPrimary ? " primary_calendar" : "") + (event.isSecondary ? " secondary_calendar" : "");
                        dayContent += "<p class='single_event" + addClass + "'>" + event.time + " " + event.description + "</p>";

                    }
                });
                this._content += this.dayTemplate(day.monthOfYear, day.dayOfWeek, day.dayOfMonth, dayContent);
            });
        });

        this._shadowRoot.innerHTML = this.template;
    }

    set data(value) {
        this._data = value;
        this._renderCalendar();
    }

    get data() {
        return this._data;
    }

}

customElements.define('fb-calendar', Calendar);