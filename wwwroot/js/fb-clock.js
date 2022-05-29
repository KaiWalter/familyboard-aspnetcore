class Clock extends HTMLElement {
    constructor() {
        super();
        this._shadowRoot = this.attachShadow({ 'mode': 'open' });
        this._content = '';
        this._shadowRoot.innerHTML = this.template;
    }

    get style() {
        return `
        <style>
        .clock {
            margin-top: 30px;
            width: 100%;
            color: white;
            font-size: 500%;
            text-align: right;
            text-shadow: 2px 2px 2px #333;
        }
        
        .clock .sec {
            font-size: 0.4em;
            padding-left: 5px;
            padding-right: 20px;
            position: relative;
            top: -1em;
        }
        </style>
        `;
    }

    get template() {
        return `
        ${this.style}
        <div class="clock">${this._content}</div>
        `;
    }

    set content(value) {
        let parts = value.split(':');
        this._content = parts[0] + ":" + parts[1] + "<span class='sec'>" + parts[2] + "</span>";
        this._shadowRoot.innerHTML = this.template;
    }
}

customElements.define('fb-clock', Clock);