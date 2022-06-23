import { o, html } from 'sinuous';
import { map } from 'sinuous/map';
import { computed } from 'sinuous/observable';
class FlowPort {
    constructor({ type, name, multiple = false }) {
        this.type = type;
        this.name = name;
        this.multiple = multiple;
    }
    static makeConnection(e, port) {
        throw new Error('Method not implemented.');
    }
}
class FlowNode {
    constructor({ x, y, type, ports }) {
        this.x0 = 0;
        this.y0 = 0;
        const self = this;
        this.x = o(x);
        this.y = o(y);
        this.moving = o(false);
        this.type = type;
        this.ports = ports.map((o) => o);
        this.style = () => computed(() => FlowNode.genStyle(self.x(), self.y()));
        this.class = () => computed(() => FlowNode.genClass(self.moving()));
    }
    ;
    static genStyle(x, y) {
        return {
            left: x + "px",
            top: y + "px"
        };
    }
    static genClass(moving) {
        return "flow-node" + (moving ? " is-moving" : "");
    }
    static startDrag(e, node) {
        e.preventDefault();
        node.moving(true);
        node.x0 = e.clientX;
        node.y0 = e.clientY;
        document.onmouseup = () => FlowNode.stopDrag(node);
        document.onmousemove = e => FlowNode.dragging(e, node);
    }
    ;
    static dragging(e, node) {
        e.preventDefault();
        const xDelta = e.clientX - node.x0;
        const yDelta = e.clientY - node.y0;
        node.x0 = e.clientX;
        node.y0 = e.clientY;
        node.x(Math.max(0, node.x() + xDelta));
        node.y(Math.max(0, node.y() + yDelta));
    }
    static stopDrag(node) {
        node.moving(false);
        document.onmouseup = null;
        document.onmousemove = null;
    }
}
const FlowDesigner = () => {
    let nodes = o([
        new FlowNode({
            x: 50,
            y: 50,
            type: "HttpRequest",
            ports: [
                new FlowPort({ type: "EventPort", name: "Requested" }),
                new FlowPort({ type: "OutputPort", name: "Path", multiple: false }),
                new FlowPort({ type: "OutputPort", name: "Method", multiple: false }),
                new FlowPort({ type: "OutputPort", name: "BodyText", multiple: false }),
            ],
        }),
        new FlowNode({
            x: 300,
            y: 200,
            type: "String",
            ports: [
                { type: "OutputPort", name: "Value", multiple: false },
            ],
        }),
        new FlowNode({
            x: 300,
            y: 50,
            type: "CompareAll",
            ports: [
                { type: "ListenPort", name: "Compare", multiple: false },
                { type: "EventPort", name: "True", multiple: false },
                { type: "EventPort", name: "False", multiple: false },
                { type: "InputPort", name: "Values", multiple: true },
            ],
        }),
        new FlowNode({
            x: 550,
            y: 50,
            type: "HttpResponse",
            ports: [
                { type: "EventPort", name: "Requested", multiple: false },
                { type: "OutputPort", name: "Path", multiple: false },
                { type: "OutputPort", name: "Method", multiple: false },
                { type: "OutputPort", name: "BodyText", multiple: false },
            ],
        }),
    ]);
    let text = o('');
    const view = html `
<div class="flow-canvas">
    <${NodeList} nodes=${nodes} />
    <form onsubmit=${handleSubmit}>
        <label htmlFor="new-todo">
            What needs to be done?
        </label>
        <input id="new-todo" onchange=${handleChange} value=${text} />
        <button>
            Add #${() => nodes().length + 1}
        </button>
    </form>
</div>
`;
    function handleSubmit(e) {
        e.preventDefault();
        if (!text().length) {
            return;
        }
        const newItem = new FlowNode({
            x: 450,
            y: 50,
            type: text(),
            ports: [
                { type: "EventPort", name: "Requested", multiple: false },
                { type: "OutputPort", name: "Path", multiple: false },
                { type: "OutputPort", name: "Method", multiple: false },
                { type: "OutputPort", name: "BodyText", multiple: false },
            ],
        });
        nodes(nodes().concat(newItem));
        text('');
    }
    function handleChange(e) {
        text(e.target.value);
    }
    return view;
};
const NodeList = ({ nodes }) => map(nodes, (node) => html `
<div class=${node.class} style=${node.style}>
    <div class="flow-node-header" onmousedown=${(e) => FlowNode.startDrag(e, node)}>${node.type} (${node.x}, ${node.y})</div>
${node.ports.map(port => {
    switch (port.type) {
        case "EventPort":
            return html `
    <div class="flow-node-port flow-node-port-EventPort">
        ${port.name} <button type="button">▷</button>
    </div>
`;
        case "OutputPort":
            return html `
    <div class="flow-node-port flow-node-port-OutputPort">
        ${port.name} <button type="button">○</button>
    </div>
`;
        case "ListenPort":
            return html `
    <div class="flow-node-port flow-node-port-ListenPort">
        <button onclick=${(e) => FlowPort.makeConnection(e, port)} type="button">▷</button> ${port.name}
    </div>
`;
        case "InputPort":
            return html `
    <div class="flow-node-port flow-node-port-InputPort">
        <button type="button">○</button> ${port.name}
    </div>
`;
        default:
            return html `
    <div class="flow-node-port flow-node-port-${port.type}">
        ${port.name} (${port.type})
    </div>
`;
    }
})}
</div>
`);
document.querySelector('.flow-designer').append(FlowDesigner());
