function FrameworkNavigation(uitarget,limitgroup,targetArray,callback,framework) {
  const frameworksearch={};
  initialize();

  function initialize(){
    Object.keys(framework.types).map(i=>framework.types[i].use).forEach(i=>frameworksearch[i]={});
    for(let i in framework.types){
      frameworksearch[framework.types[i].use][i]=searchInit(framework[framework.types[i].schema]);
    }
    let searchorganizer=document.querySelector(uitarget);
    searchorganizer.innerHTML="";
    let result = document.querySelector("#searchresult");
    result.innerHTML = "";
    result.style.display = "none";
    for(let searchgroup in frameworksearch){
      if(limitgroup.indexOf(searchgroup)>=0){
        switch(searchgroup){
          case "navigation":
            searchorganizer.innerHTML+=`<b>Search organising concepts:</b><br />`;
            break;
          case "classification":
            searchorganizer.innerHTML+=`<b>Search classification tags:</b><br />`;
            break;
          case "tag":
            searchorganizer.innerHTML+=`<b>Search context tags:</b><br />`;
            break;
        }
        searchgroup=frameworksearch[searchgroup];
        for(let schema in searchgroup){
          searchorganizer.innerHTML+=
          `<input type="radio" id="${schema}" name="searchframework" value="${schema}" />
          <label for="${schema}">${framework.types[schema].name}</label><br />`
        }
      }
    }
  }

  const searchPrep = (e) => {
    e = e.trim();
    e= (e.length > 0
      ? `+${e
          .split(" ")
          .filter((a) => a && (a.length > 1 || !isNaN(a)))
          .map((a) => (a.length > 1 ? a + "*" : a))
          .join(" ")}`
      : "").trim();
      return e=e.length>1?e:"";
  };
  const searchFrameworkHandler = (e) => {
    let type = document
      .querySelector("#searchpanel")
      .querySelector('input[name="searchframework"]:checked').value;
    let terms = searchPrep(
      document.querySelector('input[type="text"].searchframework').value || ""
    );
    let html = "";
    let results = [];
    let schematype=globalFramework.types[type];
    if (terms.length > 0) {
      let use=schematype.use;
      let searchgroup=frameworksearch[use];
      results = searchgroup[type].search(terms);
    } else {
      results = globalFramework[schematype.schema].map((a) => {
        return { ref: a.name };
      });
    }   
    html = results.map((e) =>
      renderResult(
        globalFramework[schematype.schema][
          globalFramework[schematype.schema].findIndex((a) => a.name == e.ref)
        ],schematype,true
      )
    );
    renderSearchResults(html);
    // Fetch all the details element.
    const details = document
      .querySelector("#searchresult")
      .querySelectorAll("details");
    // Add the onclick listeners.
    details.forEach((targetDetail) => {
      targetDetail.addEventListener("click", () => {
        // Close all the details that are not targetDetail.
        document
          .querySelector("#searchresult")
          .querySelectorAll("details")
          .forEach((detail) => {
            if (detail !== targetDetail) {
              if (detail.className == targetDetail.className) {
                detail.removeAttribute("open");
              }
            }
          });
      });
    });

    //add apply method
    const applybuttons = document.querySelectorAll(".applytag");     
    applybuttons.forEach((applybutton) => {
      applybutton.addEventListener("click", (event) => {
          if (event.target.dataset["obj"]) {
            //the selected element is the lowest child
              let obj = JSON.parse(event.target.dataset["obj"]);
          obj.type = obj.type.toLowerCase();
          let label = `(${obj.type}) ${obj.name}: ${obj.description}`;
          targetArray.push({id:obj.type+'-'+obj.id, label: label, data: obj });
              updateSelections();
          } else {
              alert('wut?')
        }
      });
    });
    };
    const updateSelections=()=>{
        let holder = document.querySelector("#searchselections");
        holder.innerHTML = "";
        let html = "";
        targetArray.forEach((e, i) => {
            console.log(e)
            html += `<div class="frameworkselection" data-id="${e.id}"><div class="dlabel" title="${e.label}">${e.label}</div><div class="deleteframeworkselection">X</div></div>`;
        });
        holder.innerHTML = html;
        //Jquery
        $(".deleteframeworkselection").click((event) => { targetArray.splice(targetArray.findIndex(a => a.id == event.target.parentNode.dataset["id"]), 1);
            updateSelections();
        })
    }
  const renderSearchResults = (results) => {
    //show top 5 results, show "...and XX more"
    //for each result, show related tags in organizing principles
    //render each as a details element
    let totalrecords = results.length;
    let ellipsis =
      totalrecords > maxrecords
        ? `...and ${
            totalrecords - maxrecords
          } more (add more terms to refine your search)`
        : "";
    let topresults = results.filter((a, i) => i <= maxrecords).join("");
    let html = `${topresults}<div>${ellipsis}</div>`;
    let result = document.querySelector("#searchresult");
    result.innerHTML = html;
    result.style.display = "block";
    return html;
  };
  const renderResult = (e,schematype,showtags) => {
    //returns html
    let title=`${e.name}:${e.description}`;
    let parent =false;
    let html="";
    //find navigation hierarchy
    parent=Object.keys(frameworksearch.navigation).find(a=>globalFramework.types[a].for.indexOf(schematype.id)>=0);   
    parent=typeof parent=="undefined" || !parent?false:globalFramework[schematype.schema][
      globalFramework[schematype.schema].findIndex(
        (a) => e.tags[parent] == a.id
      )
    ];
    let grandparent=!parent?false:Object.keys(frameworksearch.navigation).find(a=>globalFramework.types[a].for==parent);       
    grandparent=typeof grandparent=="undefined"?false:grandparent;
    title = `<span class="parent">${!grandparent?"":grandparent.name}</span><span class="parent">${!parent?"":parent.name}</span>`+title;
    var childselector=schematype.for ?schematype.for.split(",")[0]:false;
    let children=false;
    if(childselector || e.children){
      children = e.children || globalFramework[globalFramework.types[childselector].schema];
    }
    if(children && children.length>0){      
      children=children
        .filter((a) =>!childselector?true: (a.tags || {})[schematype.id] == e.id )
        .map((a) =>{
          let result=renderResult(a,childselector?globalFramework.types[childselector]:{id:schematype.id+"_",name:"Occupation"});
          return result;
        } );
      if(childselector){
        //handles stuctures with explicit hierarchical navigation objects
        html = `<details class="${schematype.id}"><summary class="breadcrumbtitle" title="${e.description}">${title}</summary>
            <div>
                ${children.join("")}
            </div>
            </details>`;
      }else{        
        //handles structures with children arrays
        html = `<details class="${schematype.id}"><summary class="breadcrumbtitle" title="${e.description}"><button class="applytag" data-obj='${JSON.stringify(e)}'>apply</button> <b>${schematype.name}:</b> ${e.name}: ${e.description} </summary>
                  <div>${e.description}</div>
                  <div>
                      ${children.join("")}
                  </div>
              </details>`;
      }
    }else if (showtags && schematype.use=="classification") {
      parents=Object.keys(frameworksearch.navigation).filter(a=>(a=="competency" || a=="practiceActivity") && globalFramework.types[a].for.indexOf(schematype.id)>=0); 
      let indices=parents.map(p=>globalFramework[globalFramework.types[p].schema].map((a,i) => {return {i:i,a:a,p:p}}).filter(a => e.tags[p] == a.a.id)).flat().map(a=>{return {p:globalFramework.types[a.p].schema,i:a.i};}); 
      
      let parent=indices.length==0?false:globalFramework[indices[0].p][
        indices[0].i
      ];
      let grandparent=!parent?false:Object.keys(frameworksearch.navigation).find(a=>globalFramework.types[a].for.indexOf(parents[0])>=0);  
      grandparent=typeof grandparent=="undefined"?false:grandparent;     
      if(grandparent){
        indices=globalFramework[globalFramework.types[grandparent].schema].map((a,i) => {return {i:i,a:a}}).filter(a => e.tags[grandparent] == a.a.id).map(a=>a.i); 
        grandparent=indices.length==0?false:globalFramework[globalFramework.types[grandparent].schema][
          indices[0]
        ];
      }
        let tagdetails = `<span class="parent">${!grandparent?"":grandparent.name}</span><span class="parent">${!parent?"":parent.name+":"+parent.description}</span>`;
        html = `<details class="${schematype.id}"><summary class="breadcrumbtitle" title="${e.description}"><button class="applytag" data-obj='${JSON.stringify(e)}'>apply</button> <b>${schematype.name}:</b> ${e.name}: ${e.description} </summary>
                  <div class="description">
                      ${e.description}<br/>		
                      <div class="breadcrumbtitle" onclick="document.querySelector('#${parent.type}').checked=true;document.querySelector('#searchframework').value='${parent.description}';frameworkNavigation.searchFrameworkHandler()">		
                          ${tagdetails}
                      </div>
                  </div>
              </details>`;
      } else {
        html = `<details class="${schematype.id}"><summary class="breadcrumbtitle" title="${e.description}"><button class="applytag" data-obj='${JSON.stringify(e)}'>apply</button> <b>${schematype.name}:</b> ${e.name}: ${e.description} </summary>
                  <div>${e.description}</div>
              </details>`;
      }
      return html;
  };

  var radios = document.querySelectorAll(
    'input[type=radio][name="searchframework"]'
  );
  radios.forEach((radio) =>
    radio.addEventListener("change", () => searchFrameworkHandler())
  );
  // Make the DIV element draggable:
  document.querySelector("#searchframework").value="";  
  document.querySelector("#searchpanel").style.display="block";
  return { searchFrameworkHandler: searchFrameworkHandler };
}
