import { o, html } from 'sinuous';
import { map } from 'sinuous/map';
import { computed, Observable, subscribe } from 'sinuous/observable';

type FlowPortType = "OutputPort" | "InputPort" | "EventPort" | "ListenPort";

class FlowPort {
    static makeConnection(e: MouseEvent, port: FlowPort,) {
        throw new Error('Method not implemented.');
    }

    constructor({ type, name, multiple = false }: {
        type: FlowPortType,
        name: string,
        multiple?: boolean
    }) {
        this.type = type;
        this.name = name;
        this.multiple = multiple;
    }
    type: FlowPortType;
    name: string;
    multiple: boolean;
}

class FlowNode {
    x0 = 0;
    y0 = 0;

    x: Observable<number>;
    y: Observable<number>;
    moving: Observable<boolean>;

    type: string;
    ports: FlowPort[];

    style: () => () => { left: string; top: string; };
    class: () => () => string;

    constructor({ x, y, type, ports }: {
        x: number,
        y: number,
        type: string,
        ports: FlowPort[]
    }) {
        const self = this;

        this.x = o(x);
        this.y = o(y);
        this.moving = o(false);

        this.type = type;
        this.ports = ports.map((o) => o);

        this.style = () => computed(() => FlowNode.genStyle(self.x(), self.y()));
        this.class = () => computed(() => FlowNode.genClass(self.moving()));
    };

    private static genStyle(x: number, y: number) {
        return {
            left: x + "px",
            top: y + "px"
        };
    }

    private static genClass(moving: boolean) {
        return "flow-node" + (moving ? " is-moving" : "");
    }

    static startDrag(e: MouseEvent, node: FlowNode) {
        e.preventDefault();

        node.moving(true);

        // The position of the mouse cursor will
        // be used to calculate the change in position
        // while dragging
        node.x0 = e.clientX;
        node.y0 = e.clientY;

        // Bind important events
        document.onmouseup = () => FlowNode.stopDrag(node);
        document.onmousemove = e => FlowNode.dragging(e, node);
    };

    private static dragging(e: MouseEvent, node: FlowNode) {
        e.preventDefault();

        // Calculate the change in position
        const xDelta = e.clientX - node.x0;
        const yDelta = e.clientY - node.y0;

        // Update the initial position
        node.x0 = e.clientX;
        node.y0 = e.clientY;

        // Set the node's new position
        node.x(Math.max(0, node.x() + xDelta));
        node.y(Math.max(0, node.y() + yDelta));
    }

    private static stopDrag(node: FlowNode) {
        node.moving(false);

        // Unbind important events
        document.onmouseup = null;
        document.onmousemove = null;
    }
}

const FlowDesigner = () => {
    let nodes: Observable<FlowNode[]> = o([
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

    const view = html`
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

    function handleSubmit(e: SubmitEvent) {
        e.preventDefault();
        if (!text().length) {
            return;
        }
        const newItem: FlowNode = new FlowNode({
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

    function handleChange(e: InputEvent) {
        text((e.target as HTMLInputElement).value);
    }

    return view;
};

const NodeList = ({ nodes }: { nodes: Observable<FlowNode[]> }) => map(nodes, (node) => html`
<div class=${node.class} style=${node.style}>
    <div class="flow-node-header" onmousedown=${(e: MouseEvent) => FlowNode.startDrag(e, node)}>${node.type} (${node.x}, ${node.y})</div>
${node.ports.map(port => {
    switch (port.type) {
        case "EventPort":
            return html`
    <div class="flow-node-port flow-node-port-EventPort">
        ${port.name} <button type="button">▷</button>
    </div>
`;
        case "OutputPort":
            return html`
    <div class="flow-node-port flow-node-port-OutputPort">
        ${port.name} <button type="button">○</button>
    </div>
`;
        case "ListenPort":
            return html`
    <div class="flow-node-port flow-node-port-ListenPort">
        <button onclick=${(e: MouseEvent) => FlowPort.makeConnection(e, port)} type="button">▷</button> ${port.name}
    </div>
`;
        case "InputPort":
            return html`
    <div class="flow-node-port flow-node-port-InputPort">
        <button type="button">○</button> ${port.name}
    </div>
`;
        default:
            return html`
    <div class="flow-node-port flow-node-port-${port.type}">
        ${port.name} (${port.type})
    </div>
`;
    }
})}
</div>
`);

document.querySelector('.flow-designer').append(FlowDesigner());
