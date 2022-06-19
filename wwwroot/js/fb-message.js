class Message extends HTMLElement {
    constructor() {
        super();
        this._shadowRoot = this.attachShadow({ 'mode': 'open' });
        this._content = '';
        this._shadowRoot.innerHTML = this.template;
    }

    get style() {
        return `
        <style>
        .message {
            background-color: #111111;
            display: flex;
            align-items: center;
            justify-content: center;
            margin: 2px 2px 2px 2px;
        }
        </style>
        `;
    }

    get template() {
        return `
        ${this.style}
        <div class='message'>${this._content}</div>
        `;
    }

    set content(value) {
        if (value) {
            this._content = value;
        } else {
            this._content = "&nbsp;";
        }
        this._shadowRoot.innerHTML = this.template;
    }
}

customElements.define('fb-message', Message);