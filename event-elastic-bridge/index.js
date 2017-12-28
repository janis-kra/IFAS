var FeedParser = require("feedparser");
const got = require("got");

const options = {
  headers: {
    Accept: "application/atom+xml"
  },
  auth: "admin:changeit"
};

async function read(url) {
  return new Promise((resolve, reject) => {
    var feedparser = new FeedParser({
      normalize: false
    });

    feedparser.on("error", function(error) {
      reject(error);
    });

    feedparser.on("meta", function(meta) {
      // console.log(JSON.stringify(meta));
      debugger;
      if (meta["atom:link"]) {
        const links = meta["atom:link"].map(link => link["@"]);
        let next;
        links.forEach(link => {
          if (link.rel === "next") {
            next = link.href;
          }
        });
        resolve(next);
      } else {
        resolve();
      }
    });

    got.stream(url, options).pipe(feedparser);
  });
}

(async () => {
  let url = "http://localhost:2113/streams/ui-data";
  while (url) {
    console.log(url);
    url = await read(url);
  }
})();

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
