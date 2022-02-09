document.onload = (function(d3, saveAs, Blob, undefined){
  "use strict";

  // define storyboard object
  var StoryBoarder = function(svg, nodes, edges){
    var thisBoard = this;
        thisBoard.idct = 0;
    
    thisBoard.nodes = nodes || [];
    thisBoard.edges = edges || [];
    
    thisBoard.state = {
      selectedNode: null,
      selectedEdge: null,
      mouseDownNode: null,
      mouseDownLink: null,
      justDragged: false,
      justScaleTransBoard: false,
      lastKeyDown: -1,
      shiftNodeDrag: false,
      selectedText: null
    };

    // define arrow markers for board links
    var defs = svg.append('svg:defs');
    defs.append('svg:marker')
      .attr('id', 'end-arrow')
      .attr('viewBox', '0 -5 10 10')
      .attr('refX', "32")
      .attr('markerWidth', 3.5)
      .attr('markerHeight', 3.5)
      .attr('orient', 'auto')
      .append('svg:path')
      .attr('d', 'M0,-5L10,0L0,5');

    // define arrow markers for leading arrow
    defs.append('svg:marker')
      .attr('id', 'mark-end-arrow')
      .attr('viewBox', '0 -5 10 10')
      .attr('refX', 7)
      .attr('markerWidth', 3.5)
      .attr('markerHeight', 3.5)
      .attr('orient', 'auto')
      .append('svg:path')
      .attr('d', 'M0,-5L10,0L0,5');

    thisBoard.svg = svg;
    thisBoard.svgG = svg.append("g")
          .classed(thisBoard.consts.boardClass, true);
    var svgG = thisBoard.svgG;

    // displayed when dragging between nodes
    thisBoard.dragLine = svgG.append('svg:path')
          .attr('class', 'link dragline hidden')
          .attr('d', 'M0,0L0,0')
          .style('marker-end', 'url(#mark-end-arrow)');

    // svg nodes and edges 
    thisBoard.paths = svgG.append("g").selectAll("g");
    thisBoard.circles = svgG.append("g").selectAll("g");

    thisBoard.drag = d3.behavior.drag()
          .origin(function(d){
            return {x: d.x, y: d.y};
          })
          .on("drag", function(args){
            thisBoard.state.justDragged = true;
            thisBoard.dragmove.call(thisBoard, args);
          })
          .on("dragend", function() {
            // todo check if edge-mode is selected
          });

    // listen for key events
    d3.select(window).on("keydown", function(){
      thisBoard.svgKeyDown.call(thisBoard);
    })
    .on("keyup", function(){
      thisBoard.svgKeyUp.call(thisBoard);
    });
    svg.on("mousedown", function(d){thisBoard.svgMouseDown.call(thisBoard, d);});
    svg.on("mouseup", function(d){thisBoard.svgMouseUp.call(thisBoard, d);});

    // listen for dragging
    var dragSvg = d3.behavior.zoom()
          .on("zoom", function(){
            if (d3.event.sourceEvent.shiftKey){
              // TODO  the internal d3 state is still changing
              return false;
            } else{
              thisBoard.zoomed.call(thisBoard);
            }
            return true;
          })
          .on("zoomstart", function(){
            var ael = d3.select("#" + thisBoard.consts.activeEditId).node();
            if (ael){
              ael.blur();
            }
            if (!d3.event.sourceEvent.shiftKey) d3.select('body').style("cursor", "move");
          })
          .on("zoomend", function(){
            d3.select('body').style("cursor", "auto");
          });
    
    svg.call(dragSvg).on("dblclick.zoom", null);

    // listen for resize
    window.onresize = function(){thisBoard.updateWindow(svg);};

    // handle download data
    d3.select("#download-input").on("click", function(){
      var saveEdges = [];
      thisBoard.edges.forEach(function(val, i){
        saveEdges.push({source: val.source.id, target: val.target.id});
      });
      var blob = new Blob([window.JSON.stringify({"nodes": thisBoard.nodes, "edges": saveEdges})], {type: "text/plain;charset=utf-8"});
      saveAs(blob, "mydag.json");
    });


    // handle uploaded data
    d3.select("#upload-input").on("click", function(){
      document.getElementById("hidden-file-upload").click();
    });
    d3.select("#hidden-file-upload").on("change", function(){
      if (window.File && window.FileReader && window.FileList && window.Blob) {
        var uploadFile = this.files[0];
        var filereader = new window.FileReader();
        
        filereader.onload = function(){
          var txtRes = filereader.result;
          // TODO better error handling
          try{
            var jsonObj = JSON.parse(txtRes);
            thisBoard.deleteBoard(true);
            thisBoard.nodes = jsonObj.nodes;
            thisBoard.setIdCt(jsonObj.nodes.length + 1);
            var newEdges = jsonObj.edges;
            newEdges.forEach(function(e, i){
              newEdges[i] = {source: thisBoard.nodes.filter(function(n){return n.id == e.source;})[0],
                          target: thisBoard.nodes.filter(function(n){return n.id == e.target;})[0]};
            });
            thisBoard.edges = newEdges;
            thisBoard.updateBoard();
          }catch(err){
            window.alert("Error parsing uploaded file\nerror message: " + err.message);
            return;
          }
        };
        filereader.readAsText(uploadFile);
        
      } else {
        alert("Your browser won't let you save this board -- try upgrading your browser to IE 10+ or Chrome or Firefox.");
      }

    });

    // handle delete board
    d3.select("#delete-board").on("click", function(){
      thisBoard.deleteBoard(false);
    });
  };

  StoryBoarder.prototype.setIdCt = function(idct){
    this.idct = idct;
  };

  StoryBoarder.prototype.consts =  {
    selectedClass: "selected",
    connectClass: "connect-node",
    circleGClass: "conceptG",
    boardClass: "board",
    activeEditId: "active-editing",
    BACKSPACE_KEY: 8,
    DELETE_KEY: 46,
    ENTER_KEY: 13,
    nodeRadius: 50
  };

  /* PROTOTYPE FUNCTIONS */

  StoryBoarder.prototype.dragmove = function(d) {
    var thisBoard = this;
    if (thisBoard.state.shiftNodeDrag){
      thisBoard.dragLine.attr('d', 'M' + d.x + ',' + d.y + 'L' + d3.mouse(thisBoard.svgG.node())[0] + ',' + d3.mouse(this.svgG.node())[1]);
    } else{
      d.x += d3.event.dx;
      d.y +=  d3.event.dy;
      thisBoard.updateBoard();
    }
  };

  StoryBoarder.prototype.deleteBoard = function(skipPrompt){
    var thisBoard = this,
        doDelete = true;
    if (!skipPrompt){
      doDelete = window.confirm("Press OK to delete this board");
    }
    if(doDelete){
      thisBoard.nodes = [];
      thisBoard.edges = [];
      thisBoard.updateBoard();
    }
  };

  /* select all text in element: taken from http://stackoverflow.com/questions/6139107/programatically-select-text-in-a-contenteditable-html-element */
  StoryBoarder.prototype.selectElementContents = function(el) {
    var range = document.createRange();
    range.selectNodeContents(el);
    var sel = window.getSelection();
    sel.removeAllRanges();
    sel.addRange(range);
  };


  /* insert svg line breaks: taken from http://stackoverflow.com/questions/13241475/how-do-i-include-newlines-in-labels-in-d3-charts */
  StoryBoarder.prototype.insertTitleLinebreaks = function (gEl, title) {
    var words = title.split(/\s+/g),
        nwords = words.length;
    var el = gEl.append("text")
          .attr("text-anchor","middle")
          .attr("dy", "-" + (nwords-1)*7.5);

    for (var i = 0; i < words.length; i++) {
      var tspan = el.append('tspan').text(words[i]);
      if (i > 0)
        tspan.attr('x', 0).attr('dy', '15');
    }
  };

  
  // remove edges associated with a node
  StoryBoarder.prototype.spliceLinksForNode = function(node) {
    var thisBoard = this,
        toSplice = thisBoard.edges.filter(function(l) {
      return (l.source === node || l.target === node);
    });
    toSplice.map(function(l) {
      thisBoard.edges.splice(thisBoard.edges.indexOf(l), 1);
    });
  };

  StoryBoarder.prototype.replaceSelectEdge = function(d3Path, edgeData){
    var thisBoard = this;
    d3Path.classed(thisBoard.consts.selectedClass, true);
    if (thisBoard.state.selectedEdge){
      thisBoard.removeSelectFromEdge();
    }
    thisBoard.state.selectedEdge = edgeData;
  };

  StoryBoarder.prototype.replaceSelectNode = function(d3Node, nodeData){
    var thisBoard = this;
    d3Node.classed(this.consts.selectedClass, true);
    if (thisBoard.state.selectedNode){
      thisBoard.removeSelectFromNode();
    }
    thisBoard.state.selectedNode = nodeData;
  };
  
  StoryBoarder.prototype.removeSelectFromNode = function(){
    var thisBoard = this;
    thisBoard.circles.filter(function(cd){
      return cd.id === thisBoard.state.selectedNode.id;
    }).classed(thisBoard.consts.selectedClass, false);
    thisBoard.state.selectedNode = null;
  };

  StoryBoarder.prototype.removeSelectFromEdge = function(){
    var thisBoard = this;
    thisBoard.paths.filter(function(cd){
      return cd === thisBoard.state.selectedEdge;
    }).classed(thisBoard.consts.selectedClass, false);
    thisBoard.state.selectedEdge = null;
  };

  StoryBoarder.prototype.pathMouseDown = function(d3path, d){
    var thisBoard = this,
        state = thisBoard.state;
    d3.event.stopPropagation();
    state.mouseDownLink = d;

    if (state.selectedNode){
      thisBoard.removeSelectFromNode();
    }
    
    var prevEdge = state.selectedEdge;  
    if (!prevEdge || prevEdge !== d){
      thisBoard.replaceSelectEdge(d3path, d);
    } else{
      thisBoard.removeSelectFromEdge();
    }
  };

  // mousedown on node
  StoryBoarder.prototype.circleMouseDown = function(d3node, d){
    var thisBoard = this,
        state = thisBoard.state;
    d3.event.stopPropagation();
    state.mouseDownNode = d;
    if (d3.event.shiftKey){
      state.shiftNodeDrag = d3.event.shiftKey;
      // reposition dragged directed edge
      thisBoard.dragLine.classed('hidden', false)
        .attr('d', 'M' + d.x + ',' + d.y + 'L' + d.x + ',' + d.y);
      return;
    }
  };

  /* place editable text on node in place of svg text */
  StoryBoarder.prototype.changeTextOfNode = function(d3node, d){
    var thisBoard= this,
        consts = thisBoard.consts,
        htmlEl = d3node.node();
    d3node.selectAll("text").remove();
    var nodeBCR = htmlEl.getBoundingClientRect(),
        curScale = nodeBCR.width/consts.nodeRadius,
        placePad  =  5*curScale,
        useHW = curScale > 1 ? nodeBCR.width*0.71 : consts.nodeRadius*1.42;
    // replace with editableconent text
    var d3txt = thisBoard.svg.selectAll("foreignObject")
          .data([d])
          .enter()
          .append("foreignObject")
          .attr("x", nodeBCR.left + placePad )
          .attr("y", nodeBCR.top + placePad)
          .attr("height", 2*useHW)
          .attr("width", useHW)
          .append("xhtml:p")
          .attr("id", consts.activeEditId)
          .attr("contentEditable", "true")
          .text(d.title)
          .on("mousedown", function(d){
            d3.event.stopPropagation();
          })
          .on("keydown", function(d){
            d3.event.stopPropagation();
            if (d3.event.keyCode == consts.ENTER_KEY && !d3.event.shiftKey){
              this.blur();
            }
          })
          .on("blur", function(d){
            d.title = this.textContent;
            thisBoard.insertTitleLinebreaks(d3node, d.title);
            d3.select(this.parentElement).remove();
          });
    return d3txt;
  };

  // mouseup on nodes
  StoryBoarder.prototype.circleMouseUp = function(d3node, d){
    var thisBoard = this,
        state = thisBoard.state,
        consts = thisBoard.consts;
    // reset the states
    state.shiftNodeDrag = false;    
    d3node.classed(consts.connectClass, false);
    
    var mouseDownNode = state.mouseDownNode;
    
    if (!mouseDownNode) return;

    thisBoard.dragLine.classed("hidden", true);

    if (mouseDownNode !== d){
      // we're in a different node: create new edge for mousedown edge and add to board
      var newEdge = {source: mouseDownNode, target: d};
      var filtRes = thisBoard.paths.filter(function(d){
        if (d.source === newEdge.target && d.target === newEdge.source){
          thisBoard.edges.splice(thisBoard.edges.indexOf(d), 1);
        }
        return d.source === newEdge.source && d.target === newEdge.target;
      });
      if (!filtRes[0].length){
        thisBoard.edges.push(newEdge);
        thisBoard.updateBoard();
      }
    } else{
      // we're in the same node
      if (state.justDragged) {
        // dragged, not clicked
        state.justDragged = false;
      } else{
        // clicked, not dragged
        if (d3.event.shiftKey){
          // shift-clicked node: edit text content
          var d3txt = thisBoard.changeTextOfNode(d3node, d);
          var txtNode = d3txt.node();
          thisBoard.selectElementContents(txtNode);
          txtNode.focus();
        } else{
          if (state.selectedEdge){
            thisBoard.removeSelectFromEdge();
          }
          var prevNode = state.selectedNode;            
          
          if (!prevNode || prevNode.id !== d.id){
            thisBoard.replaceSelectNode(d3node, d);
          } else{
            thisBoard.removeSelectFromNode();
          }
        }
      }
    }
    state.mouseDownNode = null;
    return;
    
  }; // end of circles mouseup

  // mousedown on main svg
  StoryBoarder.prototype.svgMouseDown = function(){
    this.state.boardMouseDown = true;
  };

  // mouseup on main svg
  StoryBoarder.prototype.svgMouseUp = function(){
    var thisBoard = this,
        state = thisBoard.state;
    if (state.justScaleTransBoard) {
      // dragged not clicked
      state.justScaleTransBoard = false;
    } else if (state.boardMouseDown && d3.event.shiftKey){
      // clicked not dragged from svg
      var xycoords = d3.mouse(thisBoard.svgG.node()),
          d = {id: thisBoard.idct++, title: "new concept", x: xycoords[0], y: xycoords[1]};
      thisBoard.nodes.push(d);
      thisBoard.updateBoard();
      // make title of text immediently editable
      var d3txt = thisBoard.changeTextOfNode(thisBoard.circles.filter(function(dval){
        return dval.id === d.id;
      }), d),
          txtNode = d3txt.node();
      thisBoard.selectElementContents(txtNode);
      txtNode.focus();
    } else if (state.shiftNodeDrag){
      // dragged from node
      state.shiftNodeDrag = false;
      thisBoard.dragLine.classed("hidden", true);
    }
    state.boardMouseDown = false;
  };

  // keydown on main svg
  StoryBoarder.prototype.svgKeyDown = function() {
    var thisBoard = this,
        state = thisBoard.state,
        consts = thisBoard.consts;
    // make sure repeated key presses don't register for each keydown
    if(state.lastKeyDown !== -1) return;

    state.lastKeyDown = d3.event.keyCode;
    var selectedNode = state.selectedNode,
        selectedEdge = state.selectedEdge;

    switch(d3.event.keyCode) {
    case consts.BACKSPACE_KEY:
    case consts.DELETE_KEY:
      d3.event.preventDefault();
      if (selectedNode){
        thisBoard.nodes.splice(thisBoard.nodes.indexOf(selectedNode), 1);
        thisBoard.spliceLinksForNode(selectedNode);
        state.selectedNode = null;
        thisBoard.updateBoard();
      } else if (selectedEdge){
        thisBoard.edges.splice(thisBoard.edges.indexOf(selectedEdge), 1);
        state.selectedEdge = null;
        thisBoard.updateBoard();
      }
      break;
    }
  };

  StoryBoarder.prototype.svgKeyUp = function() {
    this.state.lastKeyDown = -1;
  };

  // call to propagate changes to board
  StoryBoarder.prototype.updateBoard = function(){
    
    var thisBoard = this,
        consts = thisBoard.consts,
        state = thisBoard.state;
    
    thisBoard.paths = thisBoard.paths.data(thisBoard.edges, function(d){
      return String(d.source.id) + "+" + String(d.target.id);
    });
    var paths = thisBoard.paths;
    // update existing paths
    paths.style('marker-end', 'url(#end-arrow)')
      .classed(consts.selectedClass, function(d){
        return d === state.selectedEdge;
      })
      .attr("d", function(d){
        return "M" + d.source.x + "," + d.source.y + "L" + d.target.x + "," + d.target.y;
      });

    // add new paths
    paths.enter()
      .append("path")
      .style('marker-end','url(#end-arrow)')
      .classed("link", true)
      .attr("d", function(d){
        return "M" + d.source.x + "," + d.source.y + "L" + d.target.x + "," + d.target.y;
      })
      .on("mousedown", function(d){
        thisBoard.pathMouseDown.call(thisBoard, d3.select(this), d);
        }
      )
      .on("mouseup", function(d){
        state.mouseDownLink = null;
      });

    // remove old links
    paths.exit().remove();
    
    // update existing nodes
    thisBoard.circles = thisBoard.circles.data(thisBoard.nodes, function(d){ return d.id;});
    thisBoard.circles.attr("transform", function(d){return "translate(" + d.x + "," + d.y + ")";});

    // add new nodes
    var newGs= thisBoard.circles.enter()
          .append("g");

    newGs.classed(consts.circleGClass, true)
      .attr("transform", function(d){return "translate(" + d.x + "," + d.y + ")";})
      .on("mouseover", function(d){        
        if (state.shiftNodeDrag){
          d3.select(this).classed(consts.connectClass, true);
        }
      })
      .on("mouseout", function(d){
        d3.select(this).classed(consts.connectClass, false);
      })
      .on("mousedown", function(d){
        thisBoard.circleMouseDown.call(thisBoard, d3.select(this), d);
      })
      .on("mouseup", function(d){
        thisBoard.circleMouseUp.call(thisBoard, d3.select(this), d);
      })
      .call(thisBoard.drag);

    newGs.append("circle")
      .attr("r", String(consts.nodeRadius));

    newGs.each(function(d){
      thisBoard.insertTitleLinebreaks(d3.select(this), d.title);
    });

    // remove old nodes
    thisBoard.circles.exit().remove();
  };

  StoryBoarder.prototype.zoomed = function(){
    this.state.justScaleTransBoard = true;
    d3.select("." + this.consts.boardClass)
      .attr("transform", "translate(" + d3.event.translate + ") scale(" + d3.event.scale + ")"); 
  };

  StoryBoarder.prototype.updateWindow = function(svg){
    var docEl = document.documentElement,
        bodyEl = document.getElementsByTagName('body')[0];
    var x = window.innerWidth || docEl.clientWidth || bodyEl.clientWidth;
    var y = window.innerHeight|| docEl.clientHeight|| bodyEl.clientHeight;
    svg.attr("width", x).attr("height", y);
  };


  
  /**** MAIN ****/

  // warn the user when leaving
  window.onbeforeunload = function(){
    return "Make sure to save your board locally before leaving :-)";
  };      

  var docEl = document.documentElement,
      bodyEl = document.getElementsByTagName('body')[0];
  
  var width = window.innerWidth || docEl.clientWidth || bodyEl.clientWidth,
      height =  window.innerHeight|| docEl.clientHeight|| bodyEl.clientHeight;

  var xLoc = width/2 - 25,
      yLoc = 100;

  // initial node data
  var nodes = [{title: "new concept", id: 0, x: xLoc, y: yLoc},
               {title: "new concept", id: 1, x: xLoc, y: yLoc + 200}];
  var edges = [{source: nodes[1], target: nodes[0]}];


  /** MAIN SVG **/
  var svg = d3.select("body").append("svg")
        .attr("width", width)
        .attr("height", height);
  var board = new StoryBoarder(svg, nodes, edges);
      board.setIdCt(2);
  board.updateBoard();
})(window.d3, window.saveAs, window.Blob);