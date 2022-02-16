// set up SVG for D3
var width = 600;
var height = 580;
var mode = "view";
window.onresize = pageSizer;
function pageSizer() {
    var w = window.innerWidth;
    var h = window.innerHeight;
    width = w - 80;
    height = h - 160;
    let exists = typeof svg !== "undefined";
    let hasattr = svg.attr;
    if (exists && hasattr ) {
        svg
            .attr('width', width)
            .attr('height', height);
    }
}
const colors = d3.scaleOrdinal(["#fff4d9"]);//#fff4d9
const linkGeneratorH = d3.linkHorizontal();
const linkGeneratorV = d3.linkVertical();
const linkGeneratorR = d3.linkRadial();

const rectWidth = 40;
const rectHeight = 40;

const svg = d3.select("body")
    .append("svg");
pageSizer();
const bg = svg.append("g")
    .classed("bg", true);

bg.append('rect')
    .attr("width","100%")
    .attr("height","100%")
    .attr("style","fill:#e5e5e5");

const zoomed = bg
    .append("g")
    .classed('zoomed', true)
const canvas = zoomed.append("g")
    .classed("canvas", true);
var zoom;

function changemode(newmode) {
    mode = newmode;
    document.getElementById("currentmode").innerHTML = mode;
    if (mode == "view") {
        zoom = d3
            // base d3 pan & zoom behavior
            .zoom()
            .filter((event => { return mode == "view";}))
            // limit zoom to between 20% and 200% of original size
            .scaleExtent([0.2, 2])
            // apply pan & zoom transform to 'zoomed' element
            .on('zoom', ({ transform }) => {
                bg
                    .selectAll('.zoomable')
                    .attr('transform', transform);
            })
            // add 'grabbing' class to 'bg' element when panning;
            // add 'scaling' class to 'bg' element when zooming
            .on('start', (event) => { 
                if (event.sourceEvent && event.sourceEvent.type) {
                    bg.classed(event.sourceEvent.type === 'wheel' ? 'scaling' : 'grabbing', true);
                }
            })
            // remove 'grabbing' and 'scaling' classes when done panning & zooming
            .on('end', () => bg.classed('grabbing scaling', false));

        bg
            .call(zoom)
        d3.selectAll(".zoomed")
            .classed("zoomable", true);
    } else {
        if (zoom && zoom.on) {
            zoom.on("zoom", null);
        }
        d3.selectAll(".zoomed")
            .classed("zoomable", false);
    }
}

// set up initial nodes and links
//  - nodes are known by 'id', not by index in array.
//  - reflexive edges are indicated on the node (as a bold black rect).
//  - links are always source < target; edge directions are set by 'left' and 'right'.
const nodes = [
    //{ id: 0, reflexive: false },
    //{ id: 1, reflexive: true },
    //{ id: 2, reflexive: false }
];
let lastNodeId =0;
const links = [
    //{ source: nodes[0], target: nodes[1], left: false, right: true, control:[0,0] },
    //{ source: nodes[1], target: nodes[2], left: false, right: true, control:[0,0] }
];

// init D3 force layout
const force = d3.forceSimulation()
    .force('collision', d3.forceCollide().radius(function (d) {
        return d.radius
      }))
    .on('tick', tick);

// init D3 drag support
const drag = d3.drag(event)
    // Mac Firefox doesn't distinguish between left/right click when Ctrl is held... 
    .filter(() => event.button === 0 || event.button === 2)
    .on('start', (event, d) => {
        if (!event.active) force.alphaTarget(0.3).restart();

        d.fx = d.x;
        d.fy = d.y;
    })
    .on('drag', (event, d) => {
        d.fx = event.x;
        d.fy = event.y;
    })
    .on('end', (event, d) => {
        if (!event.active) force.alphaTarget(0);

        d.fx = null;
        d.fy = null;
    });

// define arrow markers for graph links
canvas.append('defs').append('marker')
    .attr('id', 'end-arrow')
    .attr('viewBox', '0 -5 10 10')
    .attr('refX', 6)
    .attr('markerWidth', 3)
    .attr('markerHeight', 3)
    .attr('orient', 'auto')
    .append('path')
    .attr('d', 'M0,-5L10,0L0,5')
    .attr('fill', '#000');

canvas.append('defs').append('marker')
    .attr('id', 'start-arrow')
    .attr('viewBox', '0 -5 10 10')
    .attr('refX', 4)
    .attr('markerWidth', 3)
    .attr('markerHeight', 3)
    .attr('orient', 'auto')
    .append('path')
    .attr('d', 'M10,-5L0,0L10,5')
    .attr('fill', '#000');

// line displayed when dragging new nodes
const dragLine = canvas.append('path')
    .attr('class', 'link dragline hidden')
    .attr('d', 'M0,0L0,0');

// handles to link and node element groups
let path = canvas.append('g').selectAll('path');
let rect = canvas.append('g').selectAll('g');


canvas
    .on('contextmenu', (event) => { event.preventDefault(); })
    .attr('width', width)
    .attr('height', height);

// mouse event vars
let selectedNode = null;
let selectedLink = null;
let mousedownLink = null;
let mousedownNode = null;
let mouseupNode = null;

function resetMouseVars() {
    mousedownNode = null;
    mouseupNode = null;
    mousedownLink = null;
}

// update force layout (called automatically each iteration)
function tick() {
    // draw directed edges with proper padding from node centers
    path.attr('d', (d) => {
        const deltaX = d.target.x - d.source.x;
        const deltaY = d.target.y - d.source.y;
        const dist = Math.sqrt(deltaX * deltaX + deltaY * deltaY);
        const normX = deltaX / dist;
        const normY = deltaY / dist;
        const sourcePadding = d.left ? 30 : 20;
        const targetPadding = d.right ? 30 : 20;
        const sourceX = d.source.x + (sourcePadding * normX);
        const sourceY = d.source.y + (sourcePadding * normY);
        const targetX = d.target.x - (targetPadding * normX);
        const targetY = d.target.y - (targetPadding * normY);
        if (Math.abs(deltaX) > Math.abs(deltaY)) {
            return linkGeneratorH({ source: [sourceX, sourceY], target: [targetX, targetY] });

        } else {
            return linkGeneratorV({ source: [sourceX, sourceY], target: [targetX, targetY] });

        }
        //return linkGeneratorR({ source: [sourceX, sourceY], target: [targetX, targetY] });
    });
    rect.attr('transform', (d) => `translate(${d.x-rectWidth/2},${d.y-rectHeight/2})`);
}

// update graph (called when needed)
function restart() {
    // path (link) group
    path = path.data(links);

    // update existing links
    path.classed('selected', (d) => d === selectedLink)
        .style('marker-start', (d) => d.left ? 'url(#start-arrow)' : '')
        .style('marker-end', (d) => d.right ? 'url(#end-arrow)' : '');

    // remove old links
    path.exit().remove();

    // add new links

    //d3.select("#multiLink")
    //    .selectAll("path")
    //    .data(multiLinkData)
    //    .join("path")
    //    .attr("d", linkGen)
    //    .attr("fill", "none")
    //    .attr("stroke", "black");

    path = path.enter().append('path')
        .attr('class', 'link')
        .attr("fill", "none")
        .attr("stroke", "black")
        .classed('selected', (d) => d === selectedLink)
        .style('marker-start', (d) => d.left ? 'url(#start-arrow)' : '')
        .style('marker-end', (d) => d.right ? 'url(#end-arrow)' : '')
        .on('mousedown', (event, d) => {
            if (mode != "state") return;
            if (event.ctrlKey) return;

            // select link
            mousedownLink = d;
            selectedLink = (mousedownLink === selectedLink) ? null : mousedownLink;
            selectedNode = null;
            restart();
        })
        .merge(path);

    // rect (node) group
    // NB: the function arg is crucial here! nodes are known by id, not by index!
    rect = rect.data(nodes, (d) => d.id);

    // update existing nodes (reflexive & selected visual states)
    rect.selectAll('rect')
        .style('fill', (d) => (d === selectedNode) ? d3.rgb(colors(d.id)).brighter().toString() : colors(d.id))
        .classed('reflexive', (d) => d.reflexive);

    // remove old nodes
    rect.exit().remove();

    // add new nodes
    const g = rect.enter().append('g');

    g.append('rect')
        .attr('class', 'node')
        .attr('width', rectWidth)
        .attr('height', rectHeight)
        .style('fill', (d) => (d === selectedNode) ? d3.rgb(colors(d.id)).brighter().toString() : colors(d.id))
        .style('stroke', (d) => d3.rgb(colors(d.id)).darker().toString())
        .classed('reflexive', (d) => d.reflexive)
        .on('mouseover', function (event, d) {
            if (mode != "state") return;
            if (!mousedownNode || d === mousedownNode) return;
            // enlarge target node
            d3.select(this).attr('transform', 'scale(1.1)');
        })
        .on('mouseout', function (event, d) {
            if (mode != "state") return;
            if (!mousedownNode || d === mousedownNode) return;
            // unenlarge target node
            d3.select(this).attr('transform', '');
        })
        .on('mousedown', (event, d) => {
            if (mode != "state") return;
            if (event.ctrlKey) return;

            // select node
            mousedownNode = d;
            selectedNode = (mousedownNode === selectedNode) ? null : mousedownNode;
            selectedLink = null;

            // reposition drag line
            dragLine
                .style('marker-end', 'url(#end-arrow)')
                .classed('hidden', false)
                .attr('d', `M${mousedownNode.x},${mousedownNode.y}L${mousedownNode.x},${mousedownNode.y}`);

            restart();
        })
        .on('mouseup', function (event, d) {
            if (mode != "state") return;
            if (!mousedownNode) return;
            // needed by FF
            dragLine
                .classed('hidden', true)
                .style('marker-end', '');

            // check for drag-to-self
            mouseupNode = d;
            if (mouseupNode !== mousedownNode) {
                // unenlarge target node
                d3.select(this).attr('transform', '');

                // add link to graph (update if exists)
                // NB: links are strictly source < target; arrows separately specified by booleans
                const isRight = mousedownNode.id < mouseupNode.id;
                const source = isRight ? mousedownNode : mouseupNode;
                const target = isRight ? mouseupNode : mousedownNode;

                const link = links.filter((l) => l.source === source && l.target === target)[0];
                if (link) {
                    link[isRight ? 'right' : 'left'] = true;
                } else {
                    links.push({ source, target, left: !isRight, right: isRight });
                }
                // select new link
                selectedLink = link;
                selectedNode = null;
            }
            resetMouseVars();
            restart();
        });

    // show node IDs
    g.append('text')
        .attr('x', rectWidth/2)
        .attr('y', rectHeight/2)
        .attr('class', 'id')
        .text((d) => d.id);

    rect = g.merge(rect);

    // set the graph in motion
    force
        .nodes(nodes);
        //.force('link')
        //.links(links);

    force.restart();
}

function transformConstants() {
    let transform = d3.select('.zoomed').attr('transform');
    let translate = [0, 0];
    let scale = 1;
    if (transform) {
        transform = transform.split(" ");
        translate = transform[0].split("(")[1].split(")")[0].split(",").map(a => parseFloat(a));
        scale = transform[1].split("(")[1].split(")")[0];
    }
    return { translate: translate, scale: scale };
}
function mousedown(event) {
    // because :active only works in WebKit?
    canvas.classed('active', true);
    if (mode !== "state") { return; }
    else if (event.ctrlKey || mousedownNode || mousedownLink) return;

    // insert new node at point
    const point = d3.pointer(event);
    //get transforms from bg
    let transforms = transformConstants();
    point[0] = point[0] / transforms.scale - transforms.translate[0] / transforms.scale;
    point[1] = point[1] / transforms.scale - transforms.translate[1] / transforms.scale;
    const node = { id: ++lastNodeId, reflexive: false, x: point[0], y: point[1] };
    nodes.push(node);
    restart();
}
function mousemove(event) {
    if (mode !== "state") {
        return;
    }
    else if (!mousedownNode) {
        return;
    }
    const point = d3.pointer(event);
    let transforms = transformConstants();
    point[0] = point[0] / transforms.scale - transforms.translate[0] / transforms.scale;
    point[1] = point[1] / transforms.scale - transforms.translate[1] / transforms.scale;
    // update drag line
    dragLine.attr('d', `M${mousedownNode.x},${mousedownNode.y}L${point[0]},${point[1]}`);
}

function mouseup(event) {
    if (mode !== "state") { return; }
    else if (mousedownNode) {
        // hide drag line
        dragLine
            .classed('hidden', true)
            .style('marker-end', '');
    }

    // because :active only works in WebKit?
    canvas.classed('active', false);

    // clear mouse event vars
    resetMouseVars();
}

function spliceLinksForNode(node) {
    const toSplice = links.filter((l) => l.source === node || l.target === node);
    for (const l of toSplice) {
        links.splice(links.indexOf(l), 1);
    }
}

// only respond once per keydown
let lastKeyDown = -1;

function keydown(event) {
    if (mode !== "state") return;
    event.preventDefault();

    if (lastKeyDown !== -1) return;
    lastKeyDown = event.keyCode;

    // ctrl
    if (event.keyCode === 17) {
        rect.call(drag);
        canvas.classed('ctrl', true);
        return;
    }

    if (!selectedNode && !selectedLink) return;

    switch (event.keyCode) {
        case 8: // backspace
        case 46: // delete
            if (selectedNode) {
                nodes.splice(nodes.indexOf(selectedNode), 1);
                spliceLinksForNode(selectedNode);
            } else if (selectedLink) {
                links.splice(links.indexOf(selectedLink), 1);
            }
            selectedLink = null;
            selectedNode = null;
            restart();
            break;
        case 66: // B
            if (selectedLink) {
                // set link direction to both left and right
                selectedLink.left = true;
                selectedLink.right = true;
            }
            restart();
            break;
        case 76: // L
            if (selectedLink) {
                // set link direction to left only
                selectedLink.left = true;
                selectedLink.right = false;
            }
            restart();
            break;
        case 82: // R
            if (selectedNode) {
                // toggle node reflexivity
                selectedNode.reflexive = !selectedNode.reflexive;
            } else if (selectedLink) {
                // set link direction to right only
                selectedLink.left = false;
                selectedLink.right = true;
            }
            restart();
            break;
    }
}

function keyup(event) {
    lastKeyDown = -1;

    //if (mode !== "state") return;
    // ctrl
    if (event.keyCode === 17) {
        rect.on('.drag', null);
        canvas.classed('ctrl', false);
    }
}

// app starts here
bg.on('mousedown', mousedown)
    .on('mousemove', mousemove)
    .on('mouseup', mouseup);
d3.select(window)
    .on('keydown', keydown)
    .on('keyup', keyup);
pageSizer();
restart();