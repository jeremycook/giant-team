import { e } from "./etc.js";
const templates = {
    nodes: [
        {
            type: "HttpContextNode",
            ports: [
                { type: "InputPort", name: "PathConstraint", label: "Path Constraint" },
                { type: "EventPort", name: "Started" },
                { type: "EventPort", name: "Completed" },
                { type: "OutputPort", name: "Path" },
                { type: "OutputPort", name: "Method" },
            ]
        },
        {
            type: "HttpResponseNode",
            ports: [
                { type: "ActionPort", name: "Respond" },
                { type: "InputPort", name: "Started", label: "Started" },
                { type: "InputPort", name: "Body" },
                { type: "EventPort", name: "Completed", label: "Completed" },
            ]
        },
    ],
    portTypes: {
        EventPort: { category: "Message", kind: "Source", label: "Event", icon: "▷" },
        ActionPort: { category: "Message", kind: "Target", label: "Action", icon: "▷" },
        OutputPort: { category: "Property", kind: "Source", label: "Output", icon: "○" },
        InputPort: { category: "Property", kind: "Target", label: "Input", icon: "○" },
    },
    edges: {}
};
const nodes = new Map();
const edges = [];
function toggleTemplatePicker(ev) {
    if (ev.detail > 1) {
        ev.preventDefault();
        templatePicker.style.left = (ev.offsetX) + "px";
        templatePicker.style.top = (ev.offsetY) + "px";
        templatePicker.classList.add("active");
    }
    else {
        templatePicker.classList.remove("active");
    }
}
function addNodeToCanvas(nodeTemplate) {
    const nodeData = {
        id: randomText(),
        type: nodeTemplate.type,
        ports: nodeTemplate.ports.map(port => Object.assign({}, port))
    };
    nodes.set(nodeData.id, nodeData);
    canvas.appendChild(e(".blueprint-node", {
        style: {
            left: `${templatePicker.offsetLeft}px`,
            top: `${templatePicker.offsetTop}px`,
        },
        "blueprint-node-id": nodeData.id,
    }, e(".blueprint-node-header", { onmousedown: startDrag }, nodeTemplate.type), nodeTemplate.ports.map(port => {
        var _a, _b;
        const portType = templates.portTypes[port.type];
        return e(`.blueprint-port.blueprint-port-type-${port.type}.blueprint-port-category-${portType.category}.blueprint-port-kind-${portType.kind}`, { "blueprint-port-name": port.name }, portType.kind == "Source" ?
            [(_a = port.label) !== null && _a !== void 0 ? _a : port.name, " ", e("button.blueprint-port-icon", portType.icon, { onclick: connectPorts })] :
            [e("button.blueprint-port-icon", portType.icon, { onclick: connectPorts }), " ", (_b = port.label) !== null && _b !== void 0 ? _b : port.name]);
    })));
}
function randomText() {
    return (Number.MAX_SAFE_INTEGER * Math.random()).toFixed();
}
function startDrag(ev) {
    ev.preventDefault();
    const handle = ev.currentTarget;
    const node = handle.closest(".blueprint-node");
    node.classList.add("is-moving");
    let x0 = ev.clientX;
    let y0 = ev.clientY;
    document.onmouseup = stopDrag;
    document.onmousemove = dragging;
    function dragging(ev) {
        ev.preventDefault();
        const xDelta = ev.clientX - x0;
        const yDelta = ev.clientY - y0;
        x0 = ev.clientX;
        y0 = ev.clientY;
        const newX = Math.max(0, node.offsetLeft + xDelta);
        const newY = Math.max(0, node.offsetTop + yDelta);
        node.style.left = newX + "px";
        node.style.top = newY + "px";
    }
    function stopDrag() {
        node.classList.remove("is-moving");
        document.onmouseup = null;
        document.onmousemove = null;
    }
}
;
let connectionHolder = null;
function connectPorts(ev) {
    const button = ev.currentTarget;
    const port = button.closest(".blueprint-port");
    const node = button.closest(".blueprint-node");
    const nodeId = node.getAttribute("blueprint-node-id");
    const portName = port.getAttribute("blueprint-port-name");
    if (connectionHolder === null) {
        connectionHolder = {
            nodeId: nodeId,
            portName: portName,
        };
    }
    else {
        const edge = {
            sourceNodeId: connectionHolder.nodeId, sourcePortName: connectionHolder.portName,
            targetNodeId: nodeId, targetPortName: portName
        };
        edges.push(edge);
        connectionHolder = null;
        drawEdge(edge);
    }
}
function drawEdge({ sourceNodeId, sourcePortName, targetNodeId, targetPortName }) {
    const sourceNode = findNode(sourceNodeId);
    const sourcePort = findPort(sourceNode, sourcePortName);
    const sourceOffset = calculatePortOffset(sourceNode, sourcePort);
    const targetNode = findNode(targetNodeId);
    const targetPort = findPort(targetNode, targetPortName);
    const targetOffset = calculatePortOffset(targetNode, targetPort);
    const connection = e("line", {
        x1: sourceOffset.left.toFixed(),
        y1: sourceOffset.top.toFixed(),
        x2: targetOffset.left.toFixed(),
        y2: targetOffset.top.toFixed(),
        style: {
            stroke: "rgb(255,0,0)",
            "stroke-width": "2",
        }
    });
    svg.appendChild(connection);
    sourceNode.addEventListener("mousemove", e => {
        const offset = calculatePortOffset(sourceNode, sourcePort);
        connection.setAttribute("x1", offset.left.toFixed());
        connection.setAttribute("y1", offset.top.toFixed());
    });
    targetNode.addEventListener("mousemove", e => {
        const offset = calculatePortOffset(targetNode, targetPort);
        connection.setAttribute("x2", offset.left.toFixed());
        connection.setAttribute("y2", offset.top.toFixed());
    });
}
function findNode(nodeId) {
    return canvas.querySelector(`[blueprint-node-id="${nodeId}"]`);
}
function findPort(node, portName) {
    return node.querySelector(`[blueprint-port-name="${portName}"] button`);
}
function calculatePortOffset(node, port) {
    return {
        left: node.offsetLeft + port.offsetLeft + (port.clientWidth / 2),
        top: node.offsetTop + port.offsetTop + (port.clientHeight / 2)
    };
}
const templatePicker = e(".blueprint-template-picker", templates.nodes.map(node => e("button.blueprint-template-picker-node", {
    onclick: (ev) => {
        addNodeToCanvas(node);
        templatePicker.classList.remove("active");
    }
}, e("div", node.type))));
const svg = e("svg.blueprint-svg");
const canvas = e(".blueprint-canvas", {
    onmousedown: toggleTemplatePicker
});
const designer = e(".blueprint-designer", e(".blueprint-svg-container", svg), canvas, templatePicker);
document
    .getElementById("blueprint-designer")
    .appendChild(designer);
