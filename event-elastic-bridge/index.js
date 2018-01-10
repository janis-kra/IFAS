const FeedParser = require("feedparser");
const got = require("got");

require("dotenv").config();

const optionsAcceptAtomXml = {
  headers: {
    Accept: "application/atom+xml"
  }
};
const optionsAcceptJSON = {
  headers: {
    Accept: "application/json"
  }
};
const optionsAuth = {
  auth: `${process.env.user}:${process.env.pw}`
};
const optionsContentJSON = { headers: { "Content-Type": "application/json" } };

function initFeedparser(resolve, reject, rel) {
  const feedparser = new FeedParser({
    normalize: true
  });

  feedparser.on("error", function(error) {
    reject(error);
  });

  feedparser.on("meta", function(meta) {
    if (meta["atom:link"]) {
      const links = meta["atom:link"].map(link => link["@"]);
      let href;
      links.forEach(link => {
        if (link.rel === rel) {
          last = link.href;
        }
      });
      resolve(last);
    } else {
      resolve();
    }
  });

  return feedparser;
}

async function fetchLastPage(url) {
  return new Promise((resolve, reject) => {
    const feedparser = initFeedparser(resolve, reject, "last");
    got
      .stream(url, { ...optionsAcceptAtomXml, ...optionsAuth })
      .pipe(feedparser);
  });
}

async function read(url) {
  return new Promise((resolve, reject) => {
    const feedparser = initFeedparser(resolve, reject, "previous");

    feedparser.on("readable", function() {
      const parser = this; // `this` is `feedparser`, which is a stream
      const meta = this.meta; // **NOTE** the "meta" is always available in the context of the feedparser instance
      const links = [];
      const getOptions = {
        ...optionsAcceptJSON,
        ...optionsAuth
      };
      const ouptput = process.stdout;

      while ((item = parser.read())) {
        /*
        curl -XPOST 'localhost:9200/twitter/tweet/?pretty' -H 'Content-Type: application/json' -d'
{
    "user" : "kimchy",
    "post_date" : "2009-11-15T14:12:12",
    "message" : "trying out Elasticsearch"
}
'

        */
        const guid = item.guid;
        const link = item.link;
        const output = got.stream
          .post("http://localhost:9200/ui-data/data/?pretty", {
            headers: {
              ...optionsContentJSON.headers,
              ...optionsAcceptJSON.headers
            }
          })
          .on("response", response => console.log(`uploaded ${response}`))
          .on("error", (err, body, res) => console.error(err));
        // const outpput = got.stream.post('localhost:9200/twitter/tweet/?pretty', {headers:'Content-Type: application/json'})
        got.stream(item.link, getOptions).pipe(output);
      }
    });

    got.stream(url, optionsAcceptAtomXml).pipe(feedparser);
  });
}

async function readUiData() {
  const startUrl = "http://localhost:2113/streams/ui-data";
  let url = await fetchLastPage(startUrl);
  const count = 1; //Number.MAX_SAFE_INTEGER;
  for (let i = 0; i < count && url; i++) {
    try {
      url = await read(url);
    } catch (error) {
      console.error(error);
    }
  }
}

readUiData();

// var req = request(options).auth("admin", "changeit", false);

// req.on("error", function(error) {
//   console.error(error);
// });

// req.on("response", function(res) {
//   var stream = this; // `this` is `req`, which is a stream

//   if (res.statusCode !== 200) {
//     this.emit("error", new Error("Bad status code"));
//   } else {
//     stream.pipe(feedparser);
//   }
// });

// feedparser.on("error", function(error) {
//   console.error(error);
// });

// feedparser.on("meta", function(meta) {
//   // console.log(JSON.stringify(meta));
//   if (meta["atom:link"]) {
//     const links = meta["atom:link"].map(link => link["@"]);
//     links.forEach(link => {
//       if (link.rel === "next") {
//         console.log(`read next link: ${link.href}`);
//       }
//     });
//   }
// });

// let counter = 0;
// feedparser.on('readable', function () {
//   // This is where the action is!
//   var stream = this; // `this` is `feedparser`, which is a stream
//   var meta = this.meta; // **NOTE** the "meta" is always available in the context of the feedparser instance
//   var item;

//   while (item = stream.read()) {
//     counter++;
//     console.log(counter + ': ' + item['atom:id']['#']);

//     console.log(item);
//     /*
//       curl -XPUT 'localhost:9200/twitter/tweet/1?pretty' -H 'Content-Type: application/json' -d'
// {
//     "user" : "kimchy",
//     "post_date" : "2009-11-15T14:12:12",
//     "message" : "trying out Elasticsearch"
// }
// '

//     */
//     // request.put({
//     //   url: 'localhost:9200/streams'
//     // })
//   }
// });
