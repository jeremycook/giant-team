import { e } from "./etc.js";
import { ObservableSet } from "./observables.js";

type PortType = {
    category: string,
    kind: string,
    label: string,
    icon: string
}

type NodeTemplate = {
    type: string;
    label: string;
    ports: ({
        type: string;
        name: string;
        label?: string;
    })[];
};

type BlueprintPort = {
    type: string;
    name: string;
    label?: string;
};

type BlueprintNode = {
    id: string;
    type: string;
    ports: BlueprintPort[];
}

type BlueprintEdge = {
    sourceNodeId: string,
    sourcePortName: string,
    targetNodeId: string,
    targetPortName: string,
};


export function embed(selector: string) {

    const templates = {
        nodes: [
            {
                type: "ComparisonNode",
                label: "Comparison",
                ports: [
                    { type: "ActionPort", name: "Compare" },
                    { type: "InputPort", name: "Value1" },
                    { type: "InputPort", name: "Value2" },
                    { type: "EventPort", name: "Same" },
                    { type: "EventPort", name: "Different" },
                ]
            },
            {
                type: "HttpRequestNode",
                label: "HTTP Request",
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
                label: "HTTP Response",
                ports: [
                    { type: "ActionPort", name: "Respond" },
                    { type: "InputPort", name: "Body" },
                    { type: "EventPort", name: "Started" },
                    { type: "EventPort", name: "Completed" },
                ]
            },
        ],
        portTypes:
        {
            EventPort: { category: "Message", kind: "Source", label: "Event", icon: "▷" },
            ActionPort: { category: "Message", kind: "Target", label: "Action", icon: "▷" },

            OutputPort: { category: "Property", kind: "Source", label: "Output", icon: "○" },
            InputPort: { category: "Property", kind: "Target", label: "Input", icon: "○" },
        },
        edges:
        {

        }
    };

    const nodes = new Map<string, BlueprintNode>();
    const edges = new ObservableSet<BlueprintEdge>();

    function toggleTemplatePicker(ev: MouseEvent) {
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

    function addNodeToCanvas(nodeTemplate: NodeTemplate) {
        const nodeData = {
            id: randomText(),
            type: nodeTemplate.type,
            label: nodeTemplate.label,
            ports: nodeTemplate.ports.map(port => Object.assign({}, port))
        };

        nodes.set(nodeData.id, nodeData);

        canvas.appendChild(
            e(".blueprint-node",
                {
                    style: {
                        left: `${templatePicker.offsetLeft}px`,
                        top: `${templatePicker.offsetTop}px`,
                    },
                    "blueprint-node-id": nodeData.id,
                },
                e(".blueprint-node-header",
                    { onmousedown: startDrag },
                    nodeTemplate.label ?? nodeTemplate.type
                ),
                nodeTemplate.ports.map(port => {
                    const portType = <PortType>(<any>templates.portTypes)[port.type];

                    return e(`.blueprint-port.blueprint-port-type-${port.type}.blueprint-port-category-${portType.category}.blueprint-port-kind-${portType.kind}`,

                        { "blueprint-port-name": port.name },

                        portType.kind == "Source" ?
                            [port.label ?? port.name, " ", e("button.blueprint-port-icon", portType.icon, { onclick: connectPorts })] :
                            [e("button.blueprint-port-icon", portType.icon, { onclick: connectPorts }), " ", port.label ?? port.name]
                    )
                })
            )
        );
    }

    function randomText(): string {
        return (Number.MAX_SAFE_INTEGER * Math.random()).toFixed();
    }

    /**
     * Start dragging a node onmousedown.
     * @param ev
     */
    function startDrag(ev: MouseEvent) {
        ev.preventDefault();

        const handle = <HTMLElement>ev.currentTarget;
        const node = <HTMLElement>handle.closest(".blueprint-node");

        node.classList.add("is-moving");

        // The position of the mouse cursor will
        // be used to calculate the change in position
        // while dragging
        let x0 = ev.clientX;
        let y0 = ev.clientY;

        // Bind events
        document.onmouseup = stopDrag;
        document.onmousemove = dragging;

        function dragging(ev: MouseEvent) {
            ev.preventDefault();

            // Calculate the cursor's change in position
            const xDelta = ev.clientX - x0;
            const yDelta = ev.clientY - y0;

            // Update the initial position
            x0 = ev.clientX;
            y0 = ev.clientY;

            // Calculate the node's new position
            const newX = Math.max(0, node.offsetLeft + xDelta);
            const newY = Math.max(0, node.offsetTop + yDelta);

            // Set it
            node.style.left = newX + "px";
            node.style.top = newY + "px";
        }

        function stopDrag() {
            // Cleanup
            node.classList.remove("is-moving");
            document.onmouseup = null;
            document.onmousemove = null;
        }
    };

    let connectionHolder: {
        node: HTMLElement,
        nodeId: string,
        port: HTMLElement,
        portName: string
    } = null;

    /**
     * Start or finish connected two ports onmouseclick.
     * @param ev
     */
    function connectPorts(ev: MouseEvent): void {
        const button = <HTMLButtonElement>ev.currentTarget;
        const port = <HTMLElement>button.closest(".blueprint-port");
        const node = <HTMLElement>port.closest(".blueprint-node");

        const nodeId = node.getAttribute("blueprint-node-id");
        const portName = port.getAttribute("blueprint-port-name");

        if (connectionHolder === null) {
            // Store the node and port info
            connectionHolder = {
                node: node,
                nodeId: nodeId,
                port: port,
                portName: portName,
            };
        }
        else {
            const edge: BlueprintEdge = {
                sourceNodeId: connectionHolder.nodeId,
                sourcePortName: connectionHolder.portName,
                targetNodeId: nodeId,
                targetPortName: portName
            };

            // Save the connection
            edges.add(edge);
            connectionHolder = null;

            // Draw the line
            drawEdge(edge);
        }
    }

    function drawEdge(edge: BlueprintEdge) {
        const { sourceNodeId, sourcePortName, targetNodeId, targetPortName } = edge;

        const sourceNode = findNode(sourceNodeId);
        const sourcePort = findPort(sourceNode, sourcePortName);
        const sourceOffset = calculatePortOffset(sourceNode, sourcePort);

        const targetNode = findNode(targetNodeId);
        const targetPort = findPort(targetNode, targetPortName);
        const targetOffset = calculatePortOffset(targetNode, targetPort);

        const connection = <SVGLineElement>e("line.blueprint-connection", {
            onclick: e => {
                connection.remove();
                edges.delete(edge);
            },
            x1: sourceOffset.left.toFixed(),
            y1: sourceOffset.top.toFixed(),
            x2: targetOffset.left.toFixed(),
            y2: targetOffset.top.toFixed(),
        });

        svg.appendChild(connection)

        sourceNode.addEventListener("mousemove", e => {
            const offset = calculatePortOffset(sourceNode, sourcePort);

            connection.setAttribute("x1", offset.left.toFixed());
            connection.setAttribute("y1", offset.top.toFixed());
        })

        targetNode.addEventListener("mousemove", e => {
            const offset = calculatePortOffset(targetNode, targetPort);

            connection.setAttribute("x2", offset.left.toFixed());
            connection.setAttribute("y2", offset.top.toFixed());
        })
    }

    function findNode(nodeId: string) {
        return <HTMLElement>canvas.querySelector(`[blueprint-node-id="${nodeId}"]`);
    }

    function findPort(node: HTMLElement, portName: string) {
        return <HTMLElement>node.querySelector(`[blueprint-port-name="${portName}"] button`);
    }

    function calculatePortOffset(node: HTMLElement, port: HTMLElement) {
        return {
            left: node.offsetLeft + port.offsetLeft + (port.clientWidth / 2),
            top: node.offsetTop + port.offsetTop + (port.clientHeight / 2)
        };
    }



    const templatePicker = <HTMLDivElement>e(".blueprint-template-picker",
        templates.nodes.map(node => e("button.blueprint-template-picker-node",
            {
                onclick: (ev: MouseEvent) => {
                    addNodeToCanvas(node);
                    templatePicker.classList.remove("active");
                }
            },
            e("div", node.label ?? node.type)
        ))
    );

    const svg = <SVGElement>e("svg.blueprint-svg");

    const canvas = <HTMLDivElement>e(".blueprint-canvas", {
        onmousedown: toggleTemplatePicker
    });

    const designer = <HTMLDivElement>e(".blueprint-designer",
        canvas,
        e(".blueprint-svg-container", svg),
        templatePicker,
    );



    document
        .querySelector(selector)
        .appendChild(designer);



    return designer;
}
