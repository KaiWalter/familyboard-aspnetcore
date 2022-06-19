class Clock extends HTMLElement {
    constructor() {
        super();
        this._shadowRoot = this.attachShadow({ 'mode': 'open' });
        this._hh = '00';
        this._mm = '00';
        this._ss = '00';
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
        <div class="clock">${this._hh}:${this._mm}<span class='sec'>${this._ss}</span></div>
        `;
    }

    set time(value) {
        this._hh = value.hh
        this._mm = value.mm
        this._ss = value.ss
        this._shadowRoot.innerHTML = this.template;
    }
}

customElements.define('fb-clock', Clock);