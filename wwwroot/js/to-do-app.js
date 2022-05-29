const template = document.createElement('template');
template.innerHTML = `
<style>
    :host {
    display: block;
    font-family: sans-serif;
    text-align: center;
    }

    button {
    border: none;
    cursor: pointer;
    }

    ul {
    list-style: none;
    padding: 0;
    }
</style>
<h1>To do</h1>

<input type="text" placeholder="Add a new to do"></input>
<button>✅</button>

<ul id="todos"></ul>
`;

class TodoApp extends HTMLElement {
    constructor() {
        super();
        this._shadowRoot = this.attachShadow({ 'mode': 'open' });
        this._shadowRoot.appendChild(template.content.cloneNode(true));
        this.$todoList = this._shadowRoot.querySelector('ul');
    }

    _renderTodoList() {
        this.$todoList.innerHTML = '';

        this._todos.forEach((todo, index) => {
            let $todoItem = document.createElement('div');
            $todoItem.innerHTML = todo.text;
            this.$todoList.appendChild($todoItem);
        });
    }

    set todos(value) {
        this._todos = value;
        this._renderTodoList();
    }

    get todos() {
        return this._todos;
    }

}

window.customElements.define('to-do-app', TodoApp);